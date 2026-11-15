using System.Numerics;
using Raylib_cs;
using UTBattle.Battle;
using UTBattle.Battle.Monsters;
using UTBattle.Engine;
using static UTBattle.Engine.Input;
using Music = UTBattle.Engine.Music;

namespace UTBattle;

// Undertale and Deltarune Battle Engine, C# port (work in progress).
// A 1280x720 window (16:9); the game draws in UNDERTALE's 640x480 room space and is
// scaled into the middle of it. Logic runs at 30 steps a second, like the game.
public static class Program
{
    const double StepSec = 1.0 / 30;
    static Fight? fight;

    // the gear each fight starts with (the web build's AUTO LOADOUT: that stage of the game)
    record Loadout(int Lv, string Weapon, double Atk, string Armor, double Df, (string name, int heal, int n)[] Items);
    static readonly Loadout Tutorial = new(1, "Stick", 0, "Bandage", 0, new[] { ("Monster Candy", 10, 2) });
    static readonly Loadout Ruins = new(3, "Toy Knife", 3, "Faded Ribbon", 3, new[] { ("Monster Candy", 10, 3), ("Spider Donut", 12, 1) });
    static readonly Loadout Waterfall = new(9, "Ballet Shoes", 7, "Old Tutu", 10, new[] { ("Crab Apple", 18, 2), ("Sea Tea", 10, 2), ("Nice Cream", 15, 2), ("Spider Donut", 12, 1) });
    static readonly (string id, string label, Loadout gear, int lv)[] Roster =
    {
        ("froggit", "FROGGIT", Ruins, 3), ("dummy", "DUMMY", Tutorial, 1), ("toriel", "TORIEL", Ruins, 3),
        ("undyne", "UNDYNE", Waterfall, 9), ("undyneex", "UNDYNE THE UNDYING", Waterfall, 14),
    };
    static int sel, lv = 3, menuT;
    // Esc in a fight: the pause menu (RESUME / QUIT TO MENU); QUIT on the main menu closes the game
    static bool paused, quitGame, escQueued;
    static int pauseSel;
    static bool OnQuit => sel == Roster.Length;
    static string notice = "";

    public static void Main(string[] args)
    {
        Fight.Defs["froggit"] = () => new Froggit();
        Fight.Defs["dummy"] = () => new Dummy();
        Fight.Defs["toriel"] = () => new Toriel();
        Fight.Defs["undyne"] = () => new Undyne();
        Fight.Defs["undyneex"] = () => new UndyneEx();

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
        string Arg(string name, string def) { int i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : def; }
        // --test-steps N: run N steps of a fight with no input, then quit (headless checks)
        int testSteps = int.Parse(Arg("--test-steps", "-1"));
        // --autotest DIR: plays a scripted fight, saving screenshots to DIR
        //   --fight ID picks the monster, --mon-hp N starts it at N HP, --steps N how long
        string? shotDir = Arg("--autotest", "") is { Length: > 0 } d ? d : null;
        string testId = Arg("--fight", "froggit");
        double monHp = double.Parse(Arg("--mon-hp", "-1"));
        int testLen = int.Parse(Arg("--steps", "905"));
        double playerHp = double.Parse(Arg("--player-hp", "-1"));
        if (testSteps > 0 || shotDir != null) { sel = Math.Max(0, Array.FindIndex(Roster, r => r.id == testId)); lv = Roster[sel].lv; }
        if (testSteps > 0) StartFight();
        int autoStep = 0;
        var froggitScript = new Dictionary<int, string>
        {
            [5] = "start", [40] = "shot:menu", [45] = "Z", [55] = "shot:target", [60] = "Z", [70] = "shot:bar", [80] = "Z", [92] = "shot:hit",
            [150] = "shot:talk", [230] = "shot:attack", [300] = "shot:attack2", [400] = "shot:next", [405] = "R", [410] = "Z", [418] = "Z", [425] = "shot:actmenu",
            [430] = "D", [436] = "Z", [480] = "shot:acted", [600] = "shot:turn3", [700] = "R", [705] = "R", [712] = "Z", [720] = "shot:mercy", [725] = "Z", [760] = "shot:spared", [900] = "shot:end", [905] = "quit",
        };
        // the bosses: Z every 12 steps (FIGHT, aim, hit, read on), screenshots every 45, the
        // arrow keys tapped through the enemy's turn so the green SOUL's shield turns
        string? BossCmd(int s)
        {
            if (s == 5) return "start";
            if (s == testLen) return "quit";
            if (s % 45 == 0) return "shot:t" + s;
            if (s % 12 == 0) return "Z";
            if (s % 12 == 6) return new[] { "U", "L", "D", "R" }[s / 12 % 4];
            return null;
        }

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F11) || (Raylib.IsKeyPressed(KeyboardKey.Enter) && Raylib.IsKeyDown(KeyboardKey.LeftAlt)))
                Raylib.ToggleBorderlessWindowed();
            Input.Poll();
            if (Raylib.IsKeyPressed(KeyboardKey.Escape)) escQueued = true;
            Music.Update();
            acc += Math.Min(Raylib.GetFrameTime(), .25) * (fight?.SpeedMul ?? 1);
            if (testSteps > 0 || shotDir != null) acc = StepSec * 4;
            string? shot = null;
            while (acc >= StepSec)
            {
                acc -= StepSec;
                if (shotDir != null)
                {
                    ++autoStep;
                    // --fight pausequit: Esc mid-fight, QUIT TO MENU, then QUIT on the menu
                    var pq = new Dictionary<int, string> { [5] = "start", [60] = "shot:fight", [61] = "ESC", [70] = "shot:paused", [100] = "shot:still", [104] = "D", [110] = "shot:quitsel", [114] = "Z", [120] = "shot:menu", [124] = "U", [130] = "shot:onquit", [134] = "Z", [200] = "quit" };
                    string? cmd = testId == "froggit" ? (froggitScript.TryGetValue(autoStep, out var c) ? c : null) : testId == "pausequit" ? (pq.TryGetValue(autoStep, out var c2) ? c2 : null) : BossCmd(autoStep);
                    if (cmd == "start") { StartFight(); if (monHp > 0) foreach (var m in fight!.Mons) m.Hp = monHp; if (playerHp > 0) fight!.Hp = fight.MaxHp = playerHp; }
                    else if (cmd == "Z") Input.Inject(K.Confirm);
                    else if (cmd == "X") Input.Inject(K.Cancel);
                    else if (cmd == "R") Input.Inject(K.Right);
                    else if (cmd == "L") Input.Inject(K.Left);
                    else if (cmd == "U") Input.Inject(K.Up);
                    else if (cmd == "D") Input.Inject(K.Down);
                    else if (cmd == "ESC") escQueued = true;
                    else if (cmd?.StartsWith("shot:") == true) shot = cmd[5..];
                    else if (cmd == "quit") { Console.WriteLine($"autotest done: {(fight == null ? "back at menu" : $"fight turn {fight.Turn} hp {fight.Hp}")} notice='{notice}'"); goto done; }
                }
                if (escQueued)
                {
                    escQueued = false;
                    if (fight == null) quitGame = true;
                    else if (!fight.Exited) SetPaused(!paused);
                }
                if (fight != null && paused) PauseStep();
                else if (fight != null) { fight.Step(); if (fight.Exited) fight = null; }
                else MenuStep();
                if (quitGame) { Console.WriteLine("quit"); goto done; }
                if (testSteps > 0 && --testSteps == 0) { Console.WriteLine(fight != null ? $"ok: fight running, turn {fight.Turn}, hp {fight.Hp}" : "ok: back at the menu"); goto done; }
            }

            fight?.PreRender();
            Raylib.BeginTextureMode(screen);
            if (fight != null) { fight.Draw(); if (paused) PauseDraw(); } else MenuDraw();
            Raylib.EndTextureMode();
            if (shot != null)
            {
                var im = Raylib.LoadImageFromTexture(screen.Texture);
                Raylib.ImageFlipVertical(ref im);
                Raylib.ExportImage(im, Path.Combine(shotDir!, $"{autoStep:0000}_{shot}.png"));
                Raylib.UnloadImage(im);
                var mon = fight?.Mons.FirstOrDefault();
                Console.WriteLine($"step {autoStep} {shot}: t {fight?.T} paused {paused} sel {sel} hp {fight?.Hp} mnfight {fight?.MnFight} myfight {fight?.MyFight} bullets {fight?.Bullets.Count} soul {fight?.H.Mode} {mon?.Name} hp {mon?.Hp} music {Music.Key}");
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
        Music.Stop();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    static void StartFight()
    {
        var g = Roster[sel].gear;
        var items = new List<Fight.Item>();
        foreach (var (name, heal, n) in g.Items) for (int i = 0; i < n; i++) items.Add(new() { Name = name, Heal = heal });
        var name0 = Roster[sel].label;
        fight = new Fight(new[] { Roster[sel].id }, lv, "Frisk", items, g.Atk, g.Df)
        {
            OnExit = (reason, f) => notice = reason switch
            {
                "win" => "* YOU WON.", "flee" => "* You ran away.", "gameover" => "* Stay determined...",
                "spare" => "* You spared " + (f.Mons.FirstOrDefault()?.Name ?? "them") + ".", _ => "",
            },
        };
    }

    static void SetPaused(bool on)
    {
        paused = on; pauseSel = 0;
        Assets.Play("select");
        if (on) Music.Pause(); else Music.Resume();
        Input.ClearAll();
    }
    static void PauseStep()
    {
        Input.Step();
        if (Pressed(K.Up) || Pressed(K.Down)) { pauseSel = 1 - pauseSel; Assets.Play("move"); }
        if (Pressed(K.Cancel)) { SetPaused(false); return; }
        if (Pressed(K.Confirm))
        {
            if (pauseSel == 0) { SetPaused(false); return; }
            // quit the fight: back to the fight select, nothing won or lost
            Assets.Play("select"); paused = false; Music.Stop(); fight = null; notice = ""; Input.ClearAll();
        }
    }
    static void PauseDraw()
    {
        Gfx.Rect(0, 0, 640, 480, Gfx.A(Color.Black, .6));
        Gfx.Rect(170, 160, 300, 160, Color.White); Gfx.Rect(175, 165, 290, 150, Color.Black);
        Gfx.Text("PAUSED", 320 - 6 * 8, 182, Color.White);
        string[] opts = { "RESUME", "QUIT TO MENU" };
        for (int i = 0; i < 2; i++)
        {
            bool on = i == pauseSel;
            Gfx.Text(opts[i], 230, 232 + i * 36, on ? Gfx.Hex("#ffff00") : Color.White);
            if (on) Gfx.Draw(Assets.Frame("nbHeart", 0), 202, 234 + i * 36, 16, 16);
        }
    }

    static void MenuStep()
    {
        Input.Step(); menuT++;
        int n = Roster.Length + 1; // the fights, then QUIT
        if (Pressed(K.Up)) { sel = (sel + n - 1) % n; if (!OnQuit) lv = Roster[sel].lv; Assets.Play("move"); }
        if (Pressed(K.Down)) { sel = (sel + 1) % n; if (!OnQuit) lv = Roster[sel].lv; Assets.Play("move"); }
        if (OnQuit) { if (Pressed(K.Confirm)) quitGame = true; return; }
        if (Pressed(K.Left) && lv > 1) { lv--; Assets.Play("move"); }
        if (Pressed(K.Right) && lv < 20) { lv++; Assets.Play("move"); }
        if (Pressed(K.Confirm)) { Assets.Play("select"); notice = ""; StartFight(); }
    }

    static void MenuDraw()
    {
        Raylib.ClearBackground(Color.Black);
        Gfx.Text("UNDERTALE BATTLE ENGINE", 320 - 23 * 8, 30, Color.White);
        Gfx.Text("C# PORT (WORK IN PROGRESS)", 320 - 26 * 8, 62, Gfx.Hex("#808080"));
        for (int i = 0; i <= Roster.Length; i++)
        {
            bool on = i == sel;
            Gfx.Text(i < Roster.Length ? Roster[i].label : "QUIT", 150, 110 + i * 32, on ? Gfx.Hex("#ffff00") : Color.White);
            if (on) Gfx.Draw(Assets.Frame("nbHeart", 0), 120, 112 + i * 32, 16, 16);
        }
        if (!OnQuit)
        {
            var st = Fight.LvStats(lv); var g = Roster[sel].gear;
            Gfx.Text($"LV {lv}  (< >)", 150, 314, Color.White);
            Gfx.Text($"HP {st.hp}  AT {st.at}({g.Atk})  DF {st.df}({g.Df})", 150, 346, Gfx.Hex("#808080"));
            Gfx.Text($"{g.Weapon} / {g.Armor}", 150, 374, Gfx.Hex("#808080"));
        }
        Gfx.Text("Z FIGHT  ESC PAUSE/QUIT  F11 FULL", 56, 440, Gfx.Hex("#808080"));
        if (notice != "") Gfx.Text(notice, 150, 406, Gfx.Hex("#ffff00"));
    }
}
