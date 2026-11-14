using Raylib_cs;
using UTBattle.Engine;
using static UTBattle.Engine.Input;

namespace UTBattle.Battle;

// UNDERTALE's battle controller (obj_battlecontroller turn flow, menus, FIGHT bar, the SOUL,
// the box, bullets, the writer), ported from the web build's NB engine. One step = 1/30 s.
public class Fight
{
    // SCR_BORDERSETUP presets: [left, right, top, bottom]
    public static readonly Dictionary<int, int[]> Borders = new()
    {
        [0] = new[] { 32, 602, 250, 385 }, [1] = new[] { 217, 417, 180, 385 }, [2] = new[] { 217, 417, 125, 385 },
        [3] = new[] { 237, 397, 250, 385 }, [4] = new[] { 267, 367, 295, 385 }, [5] = new[] { 192, 442, 250, 385 },
        [6] = new[] { 227, 407, 250, 385 }, [7] = new[] { 227, 407, 200, 385 }, [8] = new[] { 202, 432, 290, 385 },
        [9] = new[] { 132, 492, 250, 385 }, [10] = new[] { 147, 487, 200, 385 }, [11] = new[] { 32, 602, 330, 465 },
        [12] = new[] { 280, 360, 200, 280 }, [13] = new[] { 280, 360, 250, 385 },
    };
    static readonly (int x, string img)[] Buttons = { (32, "fight"), (185, "act"), (345, "item"), (500, "mercy") };
    // spr_heart's hit mask
    static readonly string[] HeartMask = { "..##........##..", ".#####....#####.", "#######..#######", "#######..#######", "################", "################", "################", "################", "################", "################", "..############..", "..############..", "....########....", "....########....", "......####......", "......####......" };
    static readonly List<(int, int)> HeartPx = new();
    static Fight() { for (int j = 0; j < 16; j++) for (int i = 0; i < 16; i++) if (HeartMask[j][i] == '#') HeartPx.Add((i, j)); }

    public class Heart { public double X = 40, Y = 446, XPrev, YPrev, Frame; public bool Visible = true, Gtfo; public double Hs; }
    public class Item { public string Name = ""; public int Heal; }
    public class Fx { public string Kind = ""; public double X, Y, Frame, Alpha = 1, Sc = 1, Sp, Dir, Isp, Cx, Cy; public int T, Wait, Line; public Monster? M; public bool Dead; public List<double[]>? Parts; public List<(int x0, int n)>[]? Lines; }
    public class DmgWriter { public double X, Y, YStart, Vy, Dmg, MaxHp, Apparent, Actual; public Monster M = null!; public int Life = -1, Tick; public bool Dead; }
    public class FightBar { public double Tx, Ty, X, X0, Hs, Alpha = 1, Xs = 1, Blink; public bool Hit, Fade, CursorGone, Crit, Gone, Wait; }
    public class Bubble { public Monster M = null!; public double X, Y; public Writer W = null!; }

    // ---- state (the JS S)
    public int T, Turn, MyFight, MnFight, BMenuNo, Page, SkipT = -1, TurnTimer = -1, Talked, TalkT, TalkLen, TalkLenMin, SparePause, FightWait;
    public int[] Coord = new int[7];
    public double Hp, MaxHp, At, Df, WStrength, Sp = 4;
    public int Lv, Inv = 30, Invc;
    public string PlayerName;
    public List<Item> Items = new();
    public int[] Ideal = (int[])Borders[0].Clone();
    public double EL = 32, ER = 602, ET = 250, EB = 385;
    public Heart H = new();
    public Writer? Wr;
    public List<Bubble> Bubbles = new();
    public List<Monster> Mons = new();
    public List<Bullet> Bullets = new();
    public List<Fx> FxList = new();
    public List<DmgWriter> Dmgw = new();
    public FightBar? Bar;
    public int Target;
    public class ShakeState { public double H, V; public int Speed, T; }
    public ShakeState? Shake;
    public double ViewX, ViewY;
    public bool Won, Runaway, Exited, ExitQueued;
    public string Flavor = "", NextFlavor = "";
    public double XpTotal, GoldTotal;
    public Monster? CurMon;
    public GameOverState? GameOver;
    public Action<string, Fight>? OnExit;
    // stats for the results board
    public int StatDmg, StatAttacks, StatMisses, StatCrits, StatHits, StatHpLost, StatItems, StatActs, LastDrop = -99;
    public double MinHp;

    public class GameOverState { public int T; public double X, Y; public List<double[]>? Shards; public Writer? Text; public bool Exit; }

    public static readonly Dictionary<string, Func<MonsterDef>> Defs = new();

    public static (int hp, int at, int df) LvStats(int lv)
    {
        lv = Math.Clamp(lv, 1, 20);
        int[] df = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 };
        return (lv == 20 ? 99 : 16 + lv * 4, (lv - 1) * 2, df[lv - 1]);
    }

    public Fight(string[] monsters, int lv, string name, List<Item> items, double weaponAtk = 0)
    {
        var st = LvStats(lv);
        Lv = lv; Hp = MaxHp = st.hp; At = st.at + 10; Df = st.df + 10; WStrength = weaponAtk;
        PlayerName = name.ToUpperInvariant(); Items = items; MinHp = Hp;
        int n = monsters.Length;
        for (int i = 0; i < n; i++) Mons.Add(MakeMonster(monsters[i], i, n));
        var intro = Mons.Count > 0 ? Mons[0].Def.Encounter : "* Something draws near!";
        Flavor = intro;
        Wr = new Writer(this, Writer.PagesOf(intro, ""));
        Input.ClearAll();
    }

    static double[] SlotCentres(int n) => n <= 1 ? new double[] { 320 } : n == 2 ? new double[] { 220, 424 } : new double[] { 118, 320, 522 };

    Monster MakeMonster(string id, int i, int n)
    {
        var d = Defs[id]();
        var m = new Monster { Id = id, Def = d, I = i, Name = d.Name, Hp = d.Hp, MaxHp = d.Hp, Atk = d.Atk, Df = d.Def, Xp = d.Xp, Gold = d.Gold, MercyMod = d.MercyMod, Wd = d.Wd };
        (m.W, m.H) = d.Size;
        double cx = SlotCentres(n)[Math.Min(i, 2)];
        var p = d.Pos(i, n, cx) ?? (Math.Round(cx - m.W / 2.0), 250 - m.H - 4);
        m.X = p.x; m.Y = p.y; m.X0 = m.X;
        return m;
    }

    // ---- helpers
    public double Rnd(double n) => Random.Shared.NextDouble() * n;
    public static double DCos(double d) => Math.Cos(d * Math.PI / 180);
    public static double DSin(double d) => Math.Sin(d * Math.PI / 180);
    public IEnumerable<Monster> AliveMons => Mons.Where(m => m.Alive);
    public List<Monster> Alive() => Mons.Where(m => m.Alive).ToList();
    public (double l, double r, double t, double b) Inner() => (EL + 5, ER, ET + 5, EB);
    public void SetBorder(int n) => Ideal = (int[])(Borders.TryGetValue(n, out var b) ? b : Borders[0]).Clone();
    bool BorderSettled() => EL == Ideal[0];
    public bool Spareable(Monster m) => m.Alive && m.Hp - At - WStrength + m.Df - m.MercyMod < 0;
    public void Turntimer(int n) => TurnTimer = n;

    void StepBorder()
    {
        static double Walk(double v, double to) => Math.Abs(v - to) <= 15 ? to : v + (v < to ? 15 : -15);
        EL = Walk(EL, Ideal[0]); ER = Walk(ER, Ideal[1]); ET = Walk(ET, Ideal[2]); EB = Walk(EB, Ideal[3]);
    }

    public Writer Write(string text, string end = "", Action? onEnd = null) { Wr = new Writer(this, Writer.PagesOf(text, end), onEnd: onEnd); return Wr; }
    Writer WriteInstant(string text) { Wr = new Writer(this, new[] { text }, instant: true); return Wr; }

    // ---- the SOUL
    void HeartStep()
    {
        var h = H; h.XPrev = h.X; h.YPrev = h.Y;
        if (Invc > 0) Invc--;
        h.Frame = Invc > 0 ? (h.Frame + .5) % 2 : 0;
        if (MnFight != 2 || !h.Visible) return;
        double slow = Held(K.Cancel) ? Sp / 2 : 0;
        if (Held(K.Up)) h.Y -= Sp - slow;
        if (Held(K.Down)) h.Y += Sp - slow;
        if (Held(K.Left)) h.X -= Sp - slow;
        if (Held(K.Right)) h.X += Sp - slow;
        var f = Inner();
        if (h.X < f.l) h.X = f.l;
        if (h.X + 16 > f.r) h.X = f.r - 16;
        if (h.Y < f.t) h.Y = f.t;
        if (h.Y + 16 > f.b) h.Y = f.b - 16;
    }

    // ---- menus
    string NamesText() => string.Join("&", Alive().Skip(Page * 3).Take(3).Select(m => (Spareable(m) ? "\\Y" : "\\W") + "   * " + m.Name + "\\W"));
    string ActText(Monster m)
    {
        var a = m.Def.Acts(m).Take(6).ToArray();
        var rows = new List<string>();
        for (int r = 0; r < 3; r++)
        {
            if (r * 2 >= a.Length) break;
            var L = a[r * 2]; var R = r * 2 + 1 < a.Length ? a[r * 2 + 1] : null;
            rows.Add("   * " + L.PadRight(12) + (R != null ? "  * " + R : ""));
        }
        return string.Join("&", rows);
    }
    static string ShortName(string? n) => ((n ?? "").Length > 9 ? n![..9] : (n ?? "")).PadRight(9);
    string ItemText()
    {
        var it = Items.Skip(Page * 4).Take(4).ToList();
        string s = "   * " + ShortName(it.ElementAtOrDefault(0)?.Name) + "     " + (it.Count > 1 ? "* " + ShortName(it[1].Name) : "") + " &";
        if (it.Count > 2) s += "   * " + ShortName(it[2].Name) + "     ";
        if (it.Count > 3) s += "* " + ShortName(it[3].Name);
        if (Items.Count > 4) s += " &                     PAGE " + (Page + 1);
        return s;
    }
    string MercyText() => (AliveMons.Any(Spareable) ? "\\Y" : "\\W") + "   * Spare&\\W   * Flee";

    void OpenMenu(int n)
    {
        BMenuNo = n;
        if (n == 1 || n == 2) { Coord[1] = 0; Page = 0; WriteInstant(NamesText()); }
        if (n == 10) { Coord[2] = 0; WriteInstant(ActText(Mons[Target])); }
        if (n == 3) { Coord[3] = 0; Page = 0; WriteInstant(ItemText()); }
        if (n == 4) { Coord[4] = 0; WriteInstant(MercyText()); }
    }
    void BackToMain() { BMenuNo = 0; Write(Flavor); }

    void MenuStep()
    {
        var h = H;
        if (BMenuNo == 0)
        {
            h.X = Buttons[Coord[0]].x + 8; h.Y = 432 + 14;
            if (Pressed(K.Left) || Pressed(K.Right)) { Coord[0] = (Coord[0] + (Pressed(K.Right) ? 1 : 3)) % 4; Assets.Play("move"); }
            if (Pressed(K.Confirm))
            {
                Clear(K.Confirm);
                int c = Coord[0];
                if (c == 2 && Items.Count == 0) return;
                Talked = -1; Assets.Play("select"); OpenMenu(c + 1);
            }
            return;
        }
        if (Pressed(K.Cancel))
        {
            Clear(K.Cancel);
            if (BMenuNo == 10) { OpenMenu(2); return; }
            BackToMain(); return;
        }
        var al = Alive();
        if (BMenuNo == 1 || BMenuNo == 2)
        {
            int count = Math.Min(3, al.Count - Page * 3);
            if (Pressed(K.Up)) { Coord[1]--; if (Coord[1] < 0) Coord[1] = count - 1; Assets.Play("move"); }
            if (Pressed(K.Down)) { Coord[1]++; if (Coord[1] >= count) Coord[1] = 0; Assets.Play("move"); }
            h.X = Ideal[0] + 32; h.Y = Ideal[2] + 28 + Coord[1] * 32;
            if (Pressed(K.Confirm))
            {
                Clear(K.Confirm); Assets.Play("select");
                var m = al[Page * 3 + Coord[1]]; Target = Mons.IndexOf(m);
                if (BMenuNo == 1) StartFight(); else OpenMenu(10);
            }
            return;
        }
        if (BMenuNo == 10)
        {
            int n = Mons[Target].Def.Acts(Mons[Target]).Take(6).Count();
            int c = Coord[2];
            if (Pressed(K.Left) || Pressed(K.Right)) { int o = c % 2 == 0 ? c + 1 : c - 1; if (o < n) c = o; }
            int col = c % 2, rows = (int)Math.Ceiling((n - col) / 2.0);
            if (Pressed(K.Up)) { int r = c / 2 - 1; if (r < 0) r = rows - 1; c = r * 2 + col; }
            if (Pressed(K.Down)) { int r = c / 2 + 1; if (r >= rows) r = 0; c = r * 2 + col; }
            if (c != Coord[2]) { Coord[2] = c; Assets.Play("move"); }
            h.X = Ideal[0] + (c % 2 == 1 ? 292 : 32); h.Y = Ideal[2] + 28 + c / 2 * 32;
            if (Pressed(K.Confirm)) { Clear(K.Confirm); Assets.Play("select"); DoAct(Mons[Target], Coord[2]); }
            return;
        }
        if (BMenuNo == 3)
        {
            var pg = Items.Skip(Page * 4).Take(4).ToList(); int c = Coord[3];
            if (Pressed(K.Left) || Pressed(K.Right))
            {
                int nc = c % 2 == 0 ? c + 1 : c - 1;
                if (Pressed(K.Right) && c % 2 == 1 && (Page + 1) * 4 < Items.Count) { Page++; c--; WriteInstant(ItemText()); }
                else if (Pressed(K.Left) && c % 2 == 0 && Page > 0) { Page--; c++; WriteInstant(ItemText()); }
                else if (nc < pg.Count) c = nc;
            }
            if (Pressed(K.Up) && c >= 2) c -= 2;
            if (Pressed(K.Down) && c < 2 && Page * 4 + c + 2 < Items.Count) c += 2;
            if (c != Coord[3]) { Coord[3] = c; Assets.Play("move"); }
            h.X = Ideal[0] + (c % 2 == 1 ? 280 : 32); h.Y = Ideal[2] + (c < 2 ? 28 : 60);
            if (Pressed(K.Confirm)) { Clear(K.Confirm); Assets.Play("select"); UseItem(Page * 4 + Coord[3]); }
            return;
        }
        if (BMenuNo == 4)
        {
            if (Pressed(K.Up) || Pressed(K.Down)) { Coord[4] = Coord[4] == 1 ? 0 : 1; Assets.Play("move"); }
            h.X = Ideal[0] + 32; h.Y = Ideal[2] + 28 + Coord[4] * 32;
            if (Pressed(K.Confirm)) { Clear(K.Confirm); if (Coord[4] == 0) DoSpare(); else DoFlee(); }
        }
    }

    // ---- FIGHT: obj_target + obj_targetchoice (the knife bar)
    void StartFight()
    {
        MyFight = 1; H.Visible = false; Wr = null;
        double tx = Ideal[0] + 6, ty = Ideal[2] + 6;
        bool right = Math.Round(Rnd(1)) == 1;
        double hs = 11 + Rnd(2);
        double x = right ? tx + 570 : tx - 16;
        Bar = new FightBar { Tx = tx, Ty = ty, X = x, X0 = x, Hs = right ? -hs : hs };
    }

    void FightStep()
    {
        if (FightWait > 0 && --FightWait == 0) HurtMonster();
        var f = Bar; if (f == null || f.Wait) return;
        if (f.Hit) { f.Blink += .4; return; }
        f.X += f.Hs;
        var m = Mons[Target];
        bool Off(double x) => f.Hs > 0 ? x > f.Tx + 546 : x < f.Tx;
        void Miss() { StatMisses++; f.Fade = true; f.Hit = true; f.CursorGone = true; SpawnDmgWriter(m, 0); Dmgw[^1].Life = 30; EndPlayerTurn(); }
        if (Off(f.X)) { Miss(); return; }
        if (Pressed(K.Confirm))
        {
            Clear(K.Confirm);
            // scr_attackcalc + obj_targetchoice: x2.2 within 12px of the middle
            double dmg = At + WStrength - m.Df + Rnd(2);
            double myx = f.X + 7, perfect = f.Tx + 273, bonus = Math.Abs(myx - perfect);
            if (bonus == 0) bonus = 1;
            double stretch = (546 - bonus) / 546; f.Crit = bonus <= 12;
            dmg = bonus <= 12 ? Math.Round(dmg * 2.2) : Math.Round(dmg * stretch * 2);
            f.Hit = true;
            m.TakeDamage = Math.Max(0, dmg); f.Fade = true;
            double isp = Math.Max(.1, .5 - stretch / 4), sc = stretch * 2 - .5;
            FxList.Add(new Fx { Kind = "slice", X = m.X + m.Wd / 2.0 - 5, Y = m.Y - 5, Cx = m.X + m.Wd / 2.0 + 5, Cy = m.Y + 45, Sc = sc, Isp = isp });
            Assets.Play("laz");
            f.Wait = true; FightWait = (int)Math.Round(1 / isp * 6 + 3);
        }
    }

    void HurtMonster()
    {
        var m = Mons[Target];
        StatAttacks++; StatDmg += (int)m.TakeDamage; if (Bar?.Crit == true) StatCrits++;
        m.Hurt = true; m.Shudder = 16; m.ShakeT = 0; Assets.Play("damage");
        SpawnDmgWriter(m, m.TakeDamage);
        m.HurtAnim = 3;
    }
    void SpawnDmgWriter(Monster m, double dmg)
    {
        double x = m.X + m.Wd / 2.0 - 48, y = m.Y - 24;
        Dmgw.Add(new DmgWriter { X = x, Y = y, YStart = y, Vy = dmg != 0 ? -4 : 0, Dmg = dmg, M = m, MaxHp = m.MaxHp, Apparent = m.Hp, Actual = m.Hp });
    }
    void MonsterHurtStep(Monster m)
    {
        if (m.HurtAnim != 3) return;
        if (++m.ShakeT < 2) return; m.ShakeT = 0;
        m.X += m.Shudder;
        if (m.Shudder < 0) m.Shudder = -(m.Shudder + 2); else m.Shudder = -m.Shudder;
        if (m.Shudder == 0)
        {
            m.HurtAnim = 0; m.X = m.X0; m.Hurt = false;
            m.Hp -= m.TakeDamage;
            foreach (var w in Dmgw) if (w.M == m && w.Life < 0) w.Life = 15;
            if (m.Hp < 1) KillMonster(m);
            EndPlayerTurn();
        }
    }
    void KillMonster(Monster m)
    {
        m.Alive = false; m.Killed = true; XpTotal += m.Xp; GoldTotal += m.Gold;
        FxList.Add(new Fx { Kind = "vapor", M = m, Parts = new() });
        Assets.Play("vaporized");
    }
    public void SpareMonster(Monster m)
    {
        m.Alive = false; m.Spared = true; GoldTotal += m.Gold; m.Alpha = .5;
        Assets.Play("vaporized");
        for (int n = 0; n < 14; n++)
        {
            double x = Rnd(m.W / 2.0) + m.W / 4.0 + m.X - 8, y = Rnd(m.H / 2.0) + m.W / 4.0 + m.Y - 8;
            double rs = (8 + x - m.X) / m.W / 2, ts = (8 + y - m.Y) / m.H / 2, dir = Rnd(360);
            if (rs < .75) dir = 180; if (rs > 1.25) dir = 0;
            if (ts > 1.25 && rs > 1.25) dir = 45; if (ts > 1.25 && rs > .75 && rs < 1.25) dir = 90; if (ts > 1.25 && rs < .75) dir = 135;
            if (ts < .75 && rs > 1.25) dir = 315; if (ts < .75 && rs > .75 && rs < 1.25) dir = 270; if (ts < .75 && rs < .75) dir = 235;
            FxList.Add(new Fx { Kind = "dust", X = x, Y = y, Sc = Rnd(1) + .7, Sp = 8, Dir = -dir, Wait = 1 });
        }
    }
    public void Remove(Monster m) { m.Alive = false; m.Gone = true; }

    // ---- ACT / ITEM / MERCY
    void DoAct(Monster m, int i)
    {
        StatActs++;
        MyFight = 2; H.Visible = false;
        var name = m.Def.Acts(m)[i];
        string text; Action? then = null;
        if (i == 0 && name == "Check") text = m.Def.Check(m) ?? $"* {m.Name.ToUpperInvariant()} - ATK {m.Atk} DEF {m.Df}&* ...";
        else (text, then) = m.Def.Act(this, m, i, name);
        m.WhatIHeard = i; Talked = i;
        Write(text, "/", () => { then?.Invoke(); if (!Exited && !Won && MyFight == 2) EndPlayerTurn(); });
    }
    void UseItem(int idx)
    {
        if (idx >= Items.Count) return;
        var it = Items[idx]; StatItems++;
        MyFight = 4; H.Visible = false; Talked = 91;
        Items.RemoveAt(idx);
        double before = Hp; Hp = Math.Min(MaxHp, Hp + it.Heal);
        var got = Hp >= MaxHp ? "&* Your HP was maxed out." : $"&* You recovered {Hp - before} HP!";
        Assets.Play("heal");
        Write($"* You ate the {it.Name}.{got}", "/", EndPlayerTurn);
    }
    void DoSpare()
    {
        MyFight = 4; H.Visible = false; Wr = null; Assets.Play("select");
        bool any = false;
        foreach (var m in Alive()) if (Spareable(m)) { SpareMonster(m); any = true; }
        if (!AliveMons.Any()) { Win(); return; }
        SparePause = any ? 20 : 1;
    }
    void DoFlee()
    {
        Talked = 90;
        double v = Rnd(100) + 10 * Turn;
        if (v > 50)
        {
            Runaway = true; MyFight = 4; Assets.Play("escaped");
            H.Visible = true; H.Gtfo = true; H.Hs = -3;
            int i = (int)Math.Round(Rnd(20));
            WriteInstant(i <= 1 ? "   * I'm outta here." : i == 2 ? "   * I've got better to do." : i == 3 ? "   * Don't slow me down." : "   * Escaped...");
            return;
        }
        MyFight = 4; H.Visible = false; Wr = null; SparePause = 1; Assets.Play("select");
    }

    // ---- turn flow
    public void EndPlayerTurn() => StartEnemyTurn();
    void StartEnemyTurn()
    {
        if (!AliveMons.Any()) { Win(); return; }
        MyFight = 0; MnFight = 1; Wr = null; H.Visible = false; TalkT = 0;
        foreach (var m in Alive()) m.Talked = false;
    }
    void MonsterTalkStep()
    {
        if (TalkT == 0)
        {
            Bubbles.Clear();
            int? border = null;
            foreach (var m in Alive())
            {
                var sp = m.Def.Speech(this, m);
                if (sp != null)
                {
                    double bx = m.X + m.W, by = m.Y;
                    var w = new Writer(this, new[] { sp.Replace("\n", "&") }, typer: 2, x: bx + 15, y: by + 10);
                    Bubbles.Add(new Bubble { M = m, X = bx, Y = by, W = w });
                }
                var b = m.Def.Border(this, m); if (b != null && border == null) border = b;
            }
            SetBorder(border ?? 3);
            TalkLen = Math.Max(60, TalkLenMin); TalkLenMin = 0;
        }
        TalkT++;
        foreach (var b in Bubbles) b.W.Step();
        if (Pressed(K.Confirm) && TalkLen - TalkT > 5 && BorderSettled()) TalkLen = TalkT + 2;
        if (TalkT >= TalkLen)
        {
            Bubbles.Clear(); MnFight = 2; TurnTimer = -1;
            H.Visible = true;
            H.X = Math.Round((Ideal[0] + Ideal[1]) / 2.0) - 8; H.Y = Math.Round((Ideal[2] + Ideal[3]) / 2.0) - 8;
            NextFlavor = "";
            foreach (var m in Alive())
            {
                CurMon = m; var r = m.Def.Attack(this, m); CurMon = null;
                if (r != null && NextFlavor == "") NextFlavor = r;
            }
            if (TurnTimer < 0) TurnTimer = 100;
        }
    }
    void AttackStep()
    {
        if (TurnTimer > 0) TurnTimer--;
        foreach (var m in Alive()) { CurMon = m; m.Def.AttackStep(this, m); }
        CurMon = null;
        if (TurnTimer <= 0)
        {
            MnFight = 3; TurnTimer = -1;
            foreach (var m in Mons) m.Def.EndAttack(this, m);
            Bullets.Clear(); Bubbles.Clear(); H.Visible = false;
            SetBorder(0);
        }
    }
    void ReturnStep()
    {
        if (BorderSettled() && ET == Ideal[2] && ER == Ideal[1])
        {
            MnFight = 0; MyFight = 0; BMenuNo = 0; Turn++; H.Visible = true;
            var al = Alive();
            var flav = NextFlavor;
            if (string.IsNullOrEmpty(flav)) { var m = al[(int)Rnd(al.Count)]; flav = m.Def.Flavor(this, m); }
            Flavor = flav; Write(flav);
        }
    }
    public void Win()
    {
        Won = true; MyFight = 0; MnFight = 0; H.Visible = false; Bullets.Clear();
        Write($"* YOU WON!&* You earned {XpTotal} XP and {GoldTotal} gold.", "/%", () => ExitBattle("win"));
    }
    void ExitBattle(string reason) { Exited = true; OnExit?.Invoke(reason, this); }

    // ---- bullets
    public Bullet Spawn(Bullet b) { b.Owner ??= CurMon; Bullets.Add(b); return b; }
    bool HeartHit(Bullet b)
    {
        var h = H; if (!h.Visible || b.Hidden) return false;
        double x0 = b.X + b.Box[0], y0 = b.Y + b.Box[1], bw = b.Box[2], bh = b.Box[3];
        if (x0 > h.X + 16 || x0 + bw < h.X || y0 > h.Y + 16 || y0 + bh < h.Y) return false;
        foreach (var (i, j) in HeartPx) { double px = h.X + i + .5, py = h.Y + j + .5; if (px >= x0 && px < x0 + bw && py >= y0 && py < y0 + bh) return true; }
        return false;
    }
    public bool DamagePlayer(double dmg)
    {
        if (Invc > 0) return false;
        int amt = (int)Math.Round(dmg - Df / 5.0); if (amt < 1) amt = 1;
        Hp = Math.Max(0, Hp - amt); Assets.Play("hurt");
        Shake = new ShakeState { H = 2, V = 2, Speed = 2 };
        Invc = Inv;
        if (Hp <= 0) StartGameOver();
        return true;
    }
    void BulletsStep()
    {
        var f = Inner();
        foreach (var b in Bullets.ToList())
        {
            if (!b.Alive) continue;
            b.T++;
            if (b.Anim != 0) b.Frame += b.Anim;
            CurMon = b.Owner;
            b.Step?.Invoke(b);
            if (!b.Manual) { b.VX += b.AX; b.VY += b.AY; b.X += b.VX; b.Y += b.VY; }
            if (b.WallKill)
            {
                double bx = b.Box[0], by = b.Box[1], bw = b.Box[2], bh = b.Box[3];
                if (b.VY < 0 && b.Y + by <= f.t && b.Y + by + bh < f.t) b.Alive = false;
                if (b.VX > 0 && b.X + bx > f.r) b.Alive = false;
                if (b.VX < 0 && b.X + bx + bw < f.l) b.Alive = false;
                if (b.VY > 0 && b.Y + by > f.b) b.Alive = false;
            }
            if (b.Alive && MnFight == 2 && HeartHit(b))
            {
                if (DamagePlayer(b.Dmg) && b.Kill) b.Alive = false;
            }
        }
        foreach (var b in Bullets) if (!b.Alive && b.OnDie != null && !b.DiedOnce) { b.DiedOnce = true; CurMon = b.Owner; b.OnDie(b); }
        CurMon = null;
        Bullets.RemoveAll(b => !b.Alive);
    }

    // ---- game over
    void StartGameOver()
    {
        if (GameOver != null) return;
        GameOver = new GameOverState { X = H.X, Y = H.Y };
        Bullets.Clear(); Bubbles.Clear(); Wr = null;
    }
    void GameOverStep()
    {
        var g = GameOver!; g.T++;
        if (g.T == 20) Assets.Play("break1");
        if (g.T == 40)
        {
            Assets.Play("break2"); g.Shards = new();
            for (int i = 0; i < 6; i++) g.Shards.Add(new[] { g.X + Rnd(10), g.Y + Rnd(8), -2 + Rnd(4), -(3 + Rnd(4)), Math.Floor(Rnd(4)) });
        }
        if (g.Shards != null) foreach (var s in g.Shards) { s[3] += .2; s[0] += s[2]; s[1] += s[3]; }
        if (g.T >= 110 && g.Text == null)
        {
            g.Text = new Writer(this, new[] { "  You cannot give&  up just yet.../", "  " + PlayerName + "!&  Stay determined.../%" }, x: 100, y: 300, sound: new[] { "asgoreText" });
            g.Text.Typer = Writer.Typers[1] with { Speed = 2, Dx = 0, Dy = 0 };
        }
        if (g.Text != null) { g.Text.Step(); if (g.Text.Done && !g.Exit) { g.Exit = true; ExitBattle("gameover"); } }
    }

    // ---- effects
    void FxStep()
    {
        foreach (var e in FxList)
        {
            if (e.Kind == "slice") { e.Frame += e.Isp; e.T++; if (e.Frame >= 6) e.Dead = true; }
            else if (e.Kind == "gen") { e.Frame += 1; if (e.Frame >= 3) e.Dead = true; }
            else if (e.Kind == "dust")
            {
                if (e.Wait > 0) e.Wait--; else { e.X += DCos(e.Dir) * e.Sp; e.Y -= DSin(e.Dir) * e.Sp; e.Sp = Math.Max(0, e.Sp - .8); }
                e.Alpha -= .03; e.Frame += .25; if (e.Frame >= 3 || e.Alpha <= 0) e.Dead = true;
            }
            else if (e.Kind == "vapor")
            {
                // obj_vaporized_new: read the sprite 2px a line, 4 lines a step; each run of pixels turns to dust
                var m = e.M!;
                e.Lines ??= VaporLines(m);
                bool wide = m.W > 120;
                for (int r = 0; r < 4 && e.Line < e.Lines.Length; r++, e.Line++)
                {
                    double y = m.Y + e.Line * 2;
                    foreach (var (x0, n) in e.Lines[e.Line])
                    {
                        if (wide) e.Parts!.Add(new[] { m.X + x0, y, n * 2, Rnd(4) - 2, 0, Rnd(.5) + .2, 0 });
                        else for (int k = 0; k < n; k++) e.Parts!.Add(new[] { m.X + x0 + k * 2, y + 2, 2, Rnd(4) - 2, 0, Rnd(.5) + .2, 0 });
                    }
                }
                foreach (var p in e.Parts!) { p[4] -= p[5]; p[0] += p[3]; p[1] += p[4]; p[6]++; }
                e.Parts.RemoveAll(p => p[6] >= 12 || p[1] < -20);
                if (e.Line >= e.Lines.Length && e.Parts.Count == 0) e.Dead = true;
            }
        }
        FxList.RemoveAll(e => e.Dead);
        foreach (var w in Dmgw)
        {
            w.Tick++;
            if (w.Vy != 0 || w.Y < w.YStart) { w.Vy += .5; w.Y += w.Vy; if (w.Y > w.YStart) { w.Y = w.YStart; w.Vy = 0; } }
            if (w.Tick % 2 == 0) { if (w.Apparent > w.Actual - w.Dmg) w.Apparent -= w.Dmg / 15; else w.Apparent = w.Actual - w.Dmg; if (w.Apparent < 0) w.Apparent = 0; }
            if (w.Life > 0 && --w.Life == 0) w.Dead = true;
        }
        Dmgw.RemoveAll(w => w.Dead);
        // obj_shaker
        if (Shake is { } s && ++s.T >= s.Speed)
        {
            s.T = 0;
            if (s.H != 0) { ViewX += s.H; if (s.H < 0) s.H++; s.H = -s.H; }
            if (s.V != 0) { ViewY += s.V; if (s.V < 0) s.V++; s.V = -s.V; }
            if (s.H == 0 && s.V == 0) { Shake = null; ViewX = 0; ViewY = 0; }
        }
    }

    // the dying sprite in 2px lines: runs of opaque pixels per line
    static readonly Dictionary<string, List<(int, int)>[]> vaporCache = new();
    List<(int, int)>[] VaporLines(Monster m)
    {
        var key = m.Def.VaporSprite(m);
        if (key == null || Assets.Tex(key) is not { } tex) return Array.Empty<List<(int, int)>>();
        var ck = $"{key}|{m.W}|{m.H}";
        if (vaporCache.TryGetValue(ck, out var cached)) return cached;
        var img = Raylib.LoadImageFromTexture(tex);
        var lines = new List<(int, int)>[(m.H + 1) / 2];
        for (int ly = 0; ly < lines.Length; ly++)
        {
            lines[ly] = new();
            int sy = Math.Min(tex.Height - 1, (int)(ly * 2.0 * tex.Height / m.H));
            int run = -1;
            for (int lx = 0; lx <= m.W / 2; lx++)
            {
                bool on = false;
                if (lx < m.W / 2) { int sx = Math.Min(tex.Width - 1, (int)(lx * 2.0 * tex.Width / m.W)); on = Raylib.GetImageColor(img, sx, sy).A > 0; }
                if (on && run < 0) run = lx;
                if (!on && run >= 0) { lines[ly].Add((run * 2, lx - run)); run = -1; }
            }
        }
        Raylib.UnloadImage(img);
        vaporCache[ck] = lines;
        return lines;
    }

    // ---- one step (1/30 s)
    public void Step()
    {
        Input.Step(); T++;
        foreach (var m in Mons) m.T++;
        StepBorder();
        double hpBefore = Hp;
        if (GameOver != null) { GameOverStep(); FxStep(); return; }
        if (Won) { Wr?.Step(); FxStep(); return; }
        if (Runaway) { H.X += H.Hs; Wr?.Step(); if (H.X < -60 && !ExitQueued) { ExitQueued = true; ExitBattle("flee"); } FxStep(); return; }
        if (MnFight == 0 && MyFight == 0) { Wr?.Step(); MenuStep(); }
        else if (MyFight == 1) FightStep();
        else if (MyFight == 2 || MyFight == 4)
        {
            Wr?.Step();
            if (SparePause > 0 && --SparePause <= 0) { SparePause = 0; EndPlayerTurn(); }
        }
        if (MnFight == 1) MonsterTalkStep();
        else if (MnFight == 2) { AttackStep(); BulletsStep(); }
        else if (MnFight == 3) ReturnStep();
        foreach (var m in Mons) { CurMon = m; MonsterHurtStep(m); if (m.Alive) m.Def.Step(this, m); }
        CurMon = null;
        HeartStep();
        FxStep();
        var f = Bar;
        if (f != null && f.Fade) { f.Alpha -= .08; f.Xs -= .06; f.Tx += 15.8; if (f.Xs < .08) f.Gone = true; }
        if (Bar?.Gone == true) Bar = null;
        // stats
        if (Hp < hpBefore) { StatHpLost += (int)(hpBefore - Hp); if (T - LastDrop >= 20) StatHits++; LastDrop = T; }
        if (Hp < MinHp) MinHp = Hp;
    }

    // ---- drawing, in 640x480 room space
    public void PreRender() { foreach (var m in Mons) if (!m.Killed && !m.Gone) m.Def.PreRender(this, m); }

    public void Draw()
    {
        Raylib.ClearBackground(Color.Black);
        if (GameOver != null) { DrawGameOver(); return; }
        Gfx.Draw(Assets.Frame("nbBattleBg", 1), 20, 3, 601, Assets.Frame("nbBattleBg", 1)?.Height ?? 236);
        foreach (var m in Mons)
        {
            if (m.Killed || m.Gone) continue;
            m.Def.Draw(this, m);
        }
        DrawFx();
        foreach (var b in Bubbles) { Gfx.Draw(Assets.Tex("torielBubbleSmall"), b.X, b.Y); b.W.Draw(); }
        DrawBox();
        if (Wr != null && !Wr.Done) Wr.Draw();
        if (BMenuNo == 1 && MyFight == 0 && MnFight == 0)
        {
            var al = Alive().Skip(Page * 3).Take(3).ToList();
            int len = al.Count > 0 ? al.Max(m => m.Name.Length) : 1;
            double xw = 190 + len * 16;
            for (int i = 0; i < al.Count; i++)
            {
                Gfx.Rect(xw, 280 + i * 32, 101, 17, Gfx.Hex("#ff0000"));
                Gfx.Rect(xw, 280 + i * 32, Math.Round(al[i].Hp / al[i].MaxHp * 100) + 1, 17, Gfx.Hex("#00ff00"));
            }
        }
        DrawBar();
        DrawBullets();
        DrawHud(); DrawButtons(); DrawHeart();
        foreach (var w in Dmgw) DrawDmg(w);
    }

    void DrawBox()
    {
        Gfx.Rect(EL, ET, ER + 5 - EL, EB + 5 - ET, Color.Black);
        Gfx.Rect(EL, ET, ER - EL, 5, Color.White);
        Gfx.Rect(EL, EB, ER - EL + 5, 5, Color.White);
        Gfx.Rect(EL, ET, 5, EB - ET, Color.White);
        Gfx.Rect(ER, ET, 5, EB - ET + 5, Color.White);
    }
    void DrawHud()
    {
        Gfx.Mars($"{PlayerName}   LV {Lv}", 30, 400, Color.White);
        Gfx.Draw(Assets.Tex("hpHudSprite"), 244, 405);
        double mw = Math.Round(MaxHp * 1.2), hw = Math.Round(Math.Max(0, Hp) * 1.2);
        Gfx.Rect(275, 400, mw + 1, 21, Gfx.Hex("#ff0000"));
        if (Hp > 0) Gfx.Rect(275, 400, hw + 1, 21, Gfx.Hex("#ffff00"));
        var hp = Hp < 10 ? "0" + Math.Max(0, Hp) : Hp.ToString();
        Gfx.Mars($"{hp} / {MaxHp}", 290 + MaxHp * 1.2, 400, Color.White);
    }
    void DrawButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            bool lit = Coord[0] == i && MyFight == 0 && MnFight == 0 && !Won;
            Gfx.Draw(Assets.Tex($"btn_{Buttons[i].img}_{(lit ? "sel" : "unsel")}"), Buttons[i].x, 432, 110, 42);
        }
    }
    void DrawHeart()
    {
        var h = H; if (!h.Visible) return;
        var im = Assets.Frame("nbHeart", h.Frame);
        if (h.Gtfo) im = Assets.Frame("nbHeartGtfo", T / 2) ?? im;
        Gfx.Draw(im, Math.Round(h.X), Math.Round(h.Y), 16, 16);
    }
    void DrawBar()
    {
        var f = Bar; if (f == null) return;
        Gfx.Draw(Assets.Tex("target"), f.Tx, f.Ty, 546 * f.Xs, 115, alpha: Math.Max(0, f.Alpha));
        if (!f.CursorGone)
        {
            int fr = f.Hit ? (int)Math.Floor(f.Blink) % 2 : 0;
            if (f.X <= f.Tx + 560 && f.X >= f.Tx - 20) Gfx.Draw(Assets.Frame("targetChoice", fr), Math.Round(f.X), f.Ty, 14, 128);
        }
    }
    void DrawDmg(DmgWriter w)
    {
        double sf = (w.M.Wd > 0 ? w.M.Wd : 100) / w.MaxHp;
        if (w.Dmg > 0)
        {
            Gfx.Rect(w.X - 1, w.YStart + 7, Math.Round(w.MaxHp * sf + 1) + 2, 15, Color.Black);
            Gfx.Rect(w.X, w.YStart + 8, Math.Round(w.MaxHp * sf) + 1, 13, Gfx.Hex("#404040"));
            if (w.Apparent > 0) Gfx.Rect(w.X, w.YStart + 8, Math.Round(w.Apparent * sf) + 1, 13, Gfx.Hex("#00ff00"));
            var s = ((long)Math.Floor(w.Dmg)).ToString(); int place = s.Length - 1;
            for (int i = place; i >= 0; i--)
            {
                int d = s[place - i] - '0';
                double x = (w.M.Wd <= 120 ? w.X + 30 - i * 32 + place * 16 : w.X - 30 + w.M.Wd / 2.0 - i * 32 + place * 16);
                Gfx.Draw(Assets.Frame("damageDigits", 9 - d), x, w.Y - 28, tint: Gfx.Hex("#ff0000"));
            }
        }
        else Gfx.Draw(Assets.Tex("dmgMiss"), w.X - 10, w.Y - 16, tint: Gfx.Hex("#c0c0c0"));
    }
    void DrawFx()
    {
        foreach (var e in FxList)
        {
            if (e.Kind == "slice")
            {
                var im = Assets.Frame("strike", Math.Min(5, Math.Floor(e.Frame)));
                if (im is { } t) { double s = Math.Max(.5, e.Sc) * 2; Gfx.Draw(t, e.Cx - t.Width * s / 2, e.Cy - t.Height * s / 2, t.Width * s, t.Height * s); }
            }
            else if (e.Kind == "gen") Gfx.Draw(Assets.Frame("nbBulletGen", Math.Min(2, Math.Floor(e.Frame))), e.X, e.Y);
            else if (e.Kind == "dust") Gfx.Draw(Assets.Frame("dustCloud", Math.Min(2, Math.Floor(e.Frame))), e.X, e.Y, 16 * e.Sc, 16 * e.Sc, alpha: Math.Max(0, e.Alpha));
            else if (e.Kind == "vapor")
            {
                var m = e.M!;
                var key = m.Def.VaporSprite(m);
                if (key != null && Assets.Tex(key) is { } tex)
                {
                    int top = e.Line * 2;
                    if (top < m.H)
                    {
                        double sy = (double)tex.Height / m.H;
                        Gfx.DrawPart(tex, 0, (int)(top * sy), tex.Width, (int)((m.H - top) * sy), m.X, m.Y + top, m.W, m.H - top, Color.White);
                    }
                }
                int[] grey = { 255, 251, 251, 251, 251, 251, 251, 219, 182, 146, 109, 73 };
                foreach (var p in e.Parts!) { byte g = (byte)grey[Math.Min(11, (int)p[6])]; Gfx.Rect(Math.Round(p[0]), Math.Round(p[1]), p[2], 2, new Color(g, g, g, (byte)255)); }
            }
        }
    }
    void DrawBullets()
    {
        foreach (var b in Bullets)
        {
            if (b.Hidden || b.Img == null) continue;
            Gfx.Draw(Assets.Frame(b.Img, b.Frame), Math.Round(b.X), Math.Round(b.Y), alpha: b.Alpha);
        }
    }
    void DrawGameOver()
    {
        var o = GameOver!;
        if (o.T < 40) Gfx.Draw(o.T < 20 ? Assets.Tex("heart") : Assets.Tex("nbHeartBreak"), Math.Round(o.X), Math.Round(o.Y));
        else if (o.Shards != null) foreach (var s in o.Shards) Gfx.Draw(Assets.Frame("nbShards", s[4]), Math.Round(s[0]), Math.Round(s[1]));
        if (o.T >= 70) Gfx.Draw(Assets.Tex("gameOverBg"), 114, 36, alpha: Math.Min(1, (o.T - 70) / 30.0));
        o.Text?.Draw();
    }

}
