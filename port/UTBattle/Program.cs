using System.Numerics;
using Raylib_cs;
using UTBattle.Battle;
using UTBattle.Battle.Monsters;
using UTBattle.Engine;
using static UTBattle.Engine.Input;

namespace UTBattle;

// Undertale and Deltarune Battle Engine, C# port (work in progress).
// A 1280x720 window (16:9); the game draws in UNDERTALE's 640x480 room space and is
// scaled into the middle of it. Logic runs at 30 steps a second, like the game.
public static class Program
{
    const double StepSec = 1.0 / 30;
    static Fight? fight;
    static readonly (string id, string label)[] Roster = { ("froggit", "FROGGIT"), ("dummy", "DUMMY") };
    static int sel, lv = 1, menuT;
    static string notice = "";

    public static void Main(string[] args)
    {
        Fight.Defs["froggit"] = () => new Froggit();
        Fight.Defs["dummy"] = () => new Dummy();

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(1280, 720, "Undertale and Deltarune Battle Engine (C#)");
        Raylib.SetWindowMinSize(640, 360);
        Raylib.InitAudioDevice();
        Raylib.SetExitKey(KeyboardKey.Null);
        Assets.Init();
        Gfx.Init();

        var screen = Raylib.LoadRenderTexture(640, 480);
        Raylib.SetTextureFilter(screen.Texture, TextureFilter.Point);
        double acc = 0;
        // --test-steps N: run N steps of a Froggit fight with no input, then quit (used by CI / headless checks)
        int testSteps = args.Length >= 2 && args[0] == "--test-steps" ? int.Parse(args[1]) : -1;
        if (testSteps > 0) StartFight();
        // --autotest DIR: plays a scripted Froggit fight, saving screenshots to DIR
        string? shotDir = args.Length >= 2 && args[0] == "--autotest" ? args[1] : null;
        int autoStep = 0;
        var script = new Dictionary<int, string>
        {
            [5] = "start", [40] = "shot:menu", [45] = "Z", [55] = "shot:target", [60] = "Z", [70] = "shot:bar", [80] = "Z", [92] = "shot:hit",
            [150] = "shot:talk", [230] = "shot:attack", [300] = "shot:attack2", [400] = "shot:next", [405] = "R", [410] = "Z", [418] = "Z", [425] = "shot:actmenu",
            [430] = "D", [436] = "Z", [480] = "shot:acted", [600] = "shot:turn3", [700] = "R", [705] = "R", [712] = "Z", [720] = "shot:mercy", [725] = "Z", [760] = "shot:spared", [900] = "shot:end", [905] = "quit",
        };

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F11) || (Raylib.IsKeyPressed(KeyboardKey.Enter) && Raylib.IsKeyDown(KeyboardKey.LeftAlt)))
                Raylib.ToggleBorderlessWindowed();
            Input.Poll();
            acc += Math.Min(Raylib.GetFrameTime(), .25);
            if (testSteps > 0 || shotDir != null) acc = StepSec * 4;
            string? shot = null;
            while (acc >= StepSec)
            {
                acc -= StepSec;
                if (shotDir != null && script.TryGetValue(++autoStep, out var cmd))
                {
                    if (cmd == "start") StartFight();
                    else if (cmd == "Z") Input.Inject(K.Confirm);
                    else if (cmd == "X") Input.Inject(K.Cancel);
                    else if (cmd == "R") Input.Inject(K.Right);
                    else if (cmd == "D") Input.Inject(K.Down);
                    else if (cmd.StartsWith("shot:")) shot = cmd[5..];
                    else if (cmd == "quit") { Console.WriteLine($"autotest done: {(fight == null ? "back at menu" : $"fight turn {fight.Turn} hp {fight.Hp}")} notice='{notice}'"); goto done; }
                }
                if (fight != null) { fight.Step(); if (fight.Exited) fight = null; }
                else MenuStep();
                if (testSteps > 0 && --testSteps == 0) { Console.WriteLine(fight != null ? $"ok: fight running, turn {fight.Turn}, hp {fight.Hp}" : "ok: back at the menu"); goto done; }
            }

            fight?.PreRender();
            Raylib.BeginTextureMode(screen);
            if (fight != null) fight.Draw(); else MenuDraw();
            Raylib.EndTextureMode();
            if (shot != null)
            {
                var im = Raylib.LoadImageFromTexture(screen.Texture);
                Raylib.ImageFlipVertical(ref im);
                Raylib.ExportImage(im, Path.Combine(shotDir!, $"{autoStep:0000}_{shot}.png"));
                Raylib.UnloadImage(im);
                Console.WriteLine($"step {autoStep} {shot}: hp {fight?.Hp} mnfight {fight?.MnFight} myfight {fight?.MyFight} bullets {fight?.Bullets.Count} froggit hp {fight?.Mons.FirstOrDefault()?.Hp}");
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            float sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
            float scale = Math.Min(sw / 640f, sh / 480f);
            float vx = (float)(fight?.ViewX ?? 0) * scale, vy = (float)(fight?.ViewY ?? 0) * scale;
            var dest = new Rectangle((sw - 640 * scale) / 2 - vx, (sh - 480 * scale) / 2 - vy, 640 * scale, 480 * scale);
            Raylib.DrawTexturePro(screen.Texture, new Rectangle(0, 0, 640, -480), dest, Vector2.Zero, 0, Color.White);
            Raylib.EndDrawing();
        }
        done:
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    static void StartFight()
    {
        var items = new List<Fight.Item>
        {
            new() { Name = "Monster Candy", Heal = 10 }, new() { Name = "Spider Donut", Heal = 12 },
            new() { Name = "Butterscotch Pie", Heal = 99 }, new() { Name = "Monster Candy", Heal = 10 },
        };
        fight = new Fight(new[] { Roster[sel].id }, lv, "Frisk", items) { OnExit = (reason, f) => notice = reason switch { "win" => "* YOU WON.", "flee" => "* You ran away.", "gameover" => "* Stay determined...", _ => "" } };
    }

    static void MenuStep()
    {
        Input.Step(); menuT++;
        if (Pressed(K.Up)) { sel = (sel + Roster.Length - 1) % Roster.Length; Assets.Play("move"); }
        if (Pressed(K.Down)) { sel = (sel + 1) % Roster.Length; Assets.Play("move"); }
        if (Pressed(K.Left) && lv > 1) { lv--; Assets.Play("move"); }
        if (Pressed(K.Right) && lv < 20) { lv++; Assets.Play("move"); }
        if (Pressed(K.Confirm)) { Assets.Play("select"); notice = ""; StartFight(); }
    }

    static void MenuDraw()
    {
        Raylib.ClearBackground(Color.Black);
        Gfx.Text("UNDERTALE BATTLE ENGINE", 320 - 23 * 8, 40, Color.White);
        Gfx.Text("C# PORT (WORK IN PROGRESS)", 320 - 26 * 8, 76, Gfx.Hex("#808080"));
        for (int i = 0; i < Roster.Length; i++)
        {
            bool on = i == sel;
            Gfx.Text(Roster[i].label, 220, 160 + i * 40, on ? Gfx.Hex("#ffff00") : Color.White);
            if (on) Gfx.Draw(Assets.Frame("nbHeart", 0), 190, 162 + i * 40, 16, 16);
        }
        var st = Fight.LvStats(lv);
        Gfx.Text($"LV {lv}  (< >)", 220, 300, Color.White);
        Gfx.Text($"HP {st.hp}  AT {st.at}  DF {st.df}", 220, 336, Gfx.Hex("#808080"));
        Gfx.Text("Z  FIGHT     F11  FULLSCREEN", 120, 420, Gfx.Hex("#808080"));
        if (notice != "") Gfx.Text(notice, 220, 380, Gfx.Hex("#ffff00"));
    }
}
