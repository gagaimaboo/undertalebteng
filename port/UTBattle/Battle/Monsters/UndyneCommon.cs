using System.Text.Json;
using Raylib_cs;
using UTBattle.Engine;
using static UTBattle.Engine.Input;

namespace UTBattle.Battle.Monsters;

// What Undyne and Undyne the Undying share: obj_spearblocker + obj_greenspeargen (the green SOUL),
// obj_blockbullet(2), obj_spearbullet_follow, obj_risespearbullet, the spear across the box, the
// darkening, the speech bubble and the SOUL colour switch.
public abstract class UndyneBase : MonsterDef
{
    protected const double X0 = 210, Y0 = 20, BLX = X0 + 180, BLY = Y0 + 10;
    public override int BgFrame => -1; // obj_battlebg is destroyed
    public override (int start, int step) ShudderSteps => (8, 1);

    // ---- m.v
    protected double con, lesson, order, rating, ratingb;
    protected int a4, a9, a10, a11, glock, hitno, face, dark, endT, greenlock, talkBorder = 6, tt, fr = 15;
    protected double darkA, spearDmg = 7;
    protected bool green, attacked, endNow, died, gone;
    protected (double x, double y)? heartPos;
    protected Fight.Bubble? say;
    protected Gen? gen;
    protected Blocker? blk;

    protected static bool Tick(ref int v) => v > 0 && --v <= 0;
    protected static string Choose(Fight f, params string[] a) => a[(int)Math.Floor(f.Rnd(a.Length))];
    protected static double PDir(double x1, double y1, double x2, double y2) => (Math.Atan2(-(y2 - y1), x2 - x1) * 180 / Math.PI + 360) % 360;

    // multi-frame exports are stored in reverse: file _k is image_index N-1-k
    public static Texture2D? Rv(string key, double i)
    {
        var a = Assets.Frames(key); if (a == null) return Assets.Tex(key);
        return a[a.Length - 1 - (((int)i % a.Length) + a.Length) % a.Length];
    }
    public static string RvKey(string key, int i) { var a = Assets.Frames(key); return a == null ? key : $"{key}_{a.Length - 1 - i % a.Length}"; }

    // ---------------------------------------------------------------- speech --
    protected void Say(Fight f, Monster m, string[] pages, int typer)
    {
        if (say != null) f.Bubbles.Remove(say);
        var w = new Writer(f, pages, typer: typer, x: BLX + 15, y: BLY + 10) { OnFace = d => face = d };
        say = new Fight.Bubble { M = m, X = BLX, Y = BLY, Wide = true, Img = "nbBlconWdShrt", W = w };
        f.Bubbles.Add(say);
    }
    protected void SayStep(Fight f)
    {
        var b = say; if (b == null) return;
        // the talk and attack phases step bubbles themselves
        if (f.MnFight != 1 && f.MnFight != 2) b.W.Step();
        if (b.W.Done || !f.Bubbles.Contains(b)) { f.Bubbles.Remove(b); say = null; }
    }
    protected void SetSoul(Fight f, bool g)
    {
        green = g; f.SoulMode = g ? "green" : "red"; f.H.Mode = f.SoulMode;
    }
    protected void ToMenu(Fight f, string flavor) { f.NextFlavor = flavor; f.MyFight = 0; f.MnFight = 3; f.SetBorder(0); }

    // darkify 1 fades the pseudodarker in to half, 3 fades it out; the box walls follow
    protected void DarkStep(Fight f)
    {
        if (dark == 1) { if (darkA < .5) darkA += .04; else dark = 2; f.BoxAlpha = 1 - darkA; }
        if (dark == 3) { if (darkA > 0) darkA -= .04; f.BoxAlpha = 1 - Math.Max(0, darkA); if (darkA <= 0) { dark = 0; darkA = 0; f.BoxAlpha = 1; } }
        if (f.MnFight == 3 && dark > 0 && dark != 3) dark = 3;
        // greenlock: the green SOUL rides 34px under the top of the box
        if (greenlock != 0 && f.MnFight > 0 && f.MnFight < 99) { if (green) f.H.Y = f.ET + 34; else greenlock = 0; } else greenlock = 0;
    }
    protected void DrawDark() { if (darkA > 0) Gfx.Rect(-20, -20, 680, 520, Gfx.A(Color.Black, Math.Min(1, darkA))); }

    // scr_damagestandard on the calling object: its dmg grows by the HP bonus every call (the
    // source adds to the instance variable itself), then the hit costs round(dmg-df/5)
    public static bool HitStd(Fight f, ref double dmg)
    {
        double hp = f.Hp; int bonus = hp > 20 ? 1 : 0; for (int t = 30; t <= 90; t += 10) if (hp >= t) bonus++;
        dmg += bonus;
        return f.DamagePlayer(dmg);
    }
    static readonly string[] HeartMask = { "..##........##..", ".#####....#####.", "#######..#######", "#######..#######", "################", "################", "################", "################", "################", "################", "..############..", "..############..", "....########....", "....########....", "......####......", "......####......" };
    // collision_line against the SOUL's precise mask
    public static bool LineHitsHeart(Fight.Heart h, double x1, double y1, double x2, double y2)
    {
        int n = (int)Math.Ceiling(Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) * 2) + 1;
        for (int k = 0; k <= n; k++)
        {
            double px = x1 + (x2 - x1) * k / n - h.X, py = y1 + (y2 - y1) * k / n - h.Y;
            int i = (int)Math.Floor(px), j = (int)Math.Floor(py);
            if (i >= 0 && i < 16 && j >= 0 && j < 16 && HeartMask[j][i] == '#') return true;
        }
        return false;
    }
    static bool SegRect(double x1, double y1, double x2, double y2, double l, double t, double r, double b)
    {
        int n = (int)Math.Ceiling(Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))) + 1;
        for (int k = 0; k <= n; k++) { double x = x1 + (x2 - x1) * k / n, y = y1 + (y2 - y1) * k / n; if (x >= l && x <= r + 1 && y >= t && y <= b + 1) return true; }
        return false;
    }

    // ------------------------------------------------- obj_blockbullet(2) arrows --
    protected class Arrow : Bullet
    {
        public bool Yellow, Active, Down;
        public int Site, Cd, Fr, A0a = 1, Part, TrueSite, Siner;
        public double Sm, Rating, Hs, Vs, RemX, RemY, TotalX;
    }
    protected static List<Arrow> Arrows(Fight f) => f.Bullets.OfType<Arrow>().Where(b => b.Alive).ToList();
    static readonly Dictionary<char, (int, int)> OR = new() { ['L'] = (16, 12), ['R'] = (13, 12), ['U'] = (12, 12), ['D'] = (12, 18) };
    protected Arrow SpawnArrow(Fight f, int site, double sm, bool yellow)
    {
        // instance_number(): obj_blockbullet counts its child obj_blockbullet2 as well
        int cd = Arrows(f).Count(b => !yellow || b.Yellow) + 1;
        double ox = blk?.X ?? 320, oy = blk?.Y ?? 240;
        var a = new Arrow { Yellow = yellow, Site = site, Sm = sm == 0 ? 1 : sm, Cd = cd, Fr = cd <= 1 && !yellow ? 1 : 0, Alpha = 0, Rating = yellow ? (gen?.Rating ?? 10) : 0,
            Manual = true, WallKill = false, Kill = false, Box = new double[] { 0, 0, 0, 0 } };
        a.OnHit = _ => false;
        a.Step = _ =>
        {
            var b = a;
            if (b.A0a > 0 && --b.A0a <= 0)
            {
                if (b.Yellow) b.TrueSite = new[] { 1, 0, 3, 2 }[b.Site];
                int s = b.Site;
                b.X = s == 0 ? ox - 300 : s == 1 ? ox + 300 : ox; b.Y = s == 2 ? oy + 300 : s == 3 ? oy - 300 : oy;
                double d = Math.Sqrt((ox - b.X) * (ox - b.X) + (oy - b.Y) * (oy - b.Y)); if (d == 0) d = 1;
                double sp = 8 * b.Sm; b.Hs = (ox - b.X) / d * sp; b.Vs = (oy - b.Y) / d * sp;
                b.Active = true;
            }
            if (b.Yellow && b.Active)
            {
                // obj_blockbullet2: 80px out it hops over the SOUL and comes back from the far side
                int s = b.Site, rater = 10, r2 = 20;
                if (b.Part == 0 && ((s == 0 && b.X > ox - 80) || (s == 1 && b.X < ox + 80) || (s == 2 && b.Y < oy + 80) || (s == 3 && b.Y > oy - 80))) b.Part = 1;
                if (b.Part == 1) { b.Siner = 0; b.Part = 2; b.Hs = 0; b.Vs = 0; b.RemX = b.X; b.RemY = b.Y; b.TotalX = 145 + Math.Max(0, b.Rating - 8) * 8; }
                if (b.Down) { b.Cd--; b.Down = false; }
                if (b.Part == 2)
                {
                    b.Siner++; double S1 = Math.Sin(b.Siner * Math.PI / r2) * b.TotalX, S2 = Math.Sin(b.Siner * Math.PI / rater) * 100;
                    if (s == 0) { b.X = b.RemX + S1; b.Y = b.RemY - S2; }
                    if (s == 1) { b.X = b.RemX - S1; b.Y = b.RemY - S2; }
                    if (s == 2) { b.Y = b.RemY - S1; b.X = b.RemX - S2; }
                    if (s == 3) { b.Y = b.RemY + S1; b.X = b.RemX + S2; }
                    if (b.Siner == rater) { b.Part = 3; double sp = 8 * b.Sm; if (s == 0) b.Hs = -sp; if (s == 1) b.Hs = sp; if (s == 2) b.Vs = sp; if (s == 3) b.Vs = -sp; }
                }
            }
            else if (!b.Yellow)
            {
                if (b.Down) { b.Cd--; b.Down = false; }
                if (b.Cd <= 1) b.Fr = 1;
            }
            if (b.Alpha < 1) b.Alpha += .2;
            b.X += b.Hs; b.Y += b.Vs;
        };
        a.Draw = _ =>
        {
            var b = a; char k; Texture2D? im;
            if (b.Yellow) { if (!b.Active) return; im = Rv("nbUndArrowX", b.TrueSite); k = 'X'; }
            else
            {
                if (b.Hs > 0) k = 'L'; else if (b.Hs < 0) k = 'R'; else if (b.Vs > 0) k = 'D'; else if (b.Vs < 0) k = 'U'; else return;
                im = Rv("nbUndArrow" + k, b.Fr);
            }
            var o = k == 'X' ? (13, 12) : OR[k];
            Gfx.Draw(im, Math.Round(b.X - o.Item1), Math.Round(b.Y - o.Item2), alpha: Math.Clamp(b.Alpha, 0, 1));
        };
        f.Spawn(a);
        return a;
    }
    // masks: spr_bullet_test (obj_blockbullet) and spr_bullet_testx (obj_blockbullet2), origin (12,12)
    static (double l, double t, double r, double b) ArrowRect(Arrow b) => b.Yellow ? (b.X - 11, b.Y - 11, b.X + 7, b.Y + 6) : (b.X - 10, b.Y - 11, b.X + 7, b.Y + 6);

    // ------------------------------------------------------ obj_greenspeargen --
    protected class Gen { public List<(int dir, int type, double time, double speed)> List = new(); public int No, A0 = 5; public bool Done; public double Rating, Dmg; }
    Gen GenNew(Fight f, LessonCtx c)
    {
        var g = new Gen();
        UndLessons.Run(c, (a0, a1, a2, a3) =>
        {
            if (a0 == 4) a0 = Math.Floor(f.Rnd(4));
            if (a1 == 3) a1 = Math.Floor(f.Rnd(2));
            if (a2 == 0 && a1 != 2) a2 = 1;
            if (a3 == 0) a3 = 1;
            g.List.Add(((int)a0, (int)a1, a2, a3));
        });
        g.Rating = c.genRating; g.Dmg = c.genDmg;
        return g;
    }
    protected void GenStep(Fight f)
    {
        var g = gen; if (g == null) return;
        if (tt < 1) { gen = null; return; }
        if (!(g.A0 > 0 && --g.A0 <= 0)) return;
        if (g.No >= g.List.Count) { g.Done = true; return; }
        var s = g.List[g.No];
        g.A0 = (int)(g.Rating * s.time);
        if (s.type == 0) SpawnArrow(f, s.dir, s.speed, false);
        if (s.type == 1) SpawnArrow(f, s.dir, s.speed, true);
        if (s.type == 2) { g.A0 = (int)(g.Rating * 2); SpawnArrow(f, (int)s.time, 1, true); SpawnArrow(f, s.dir, s.speed, false); }
        g.No++;
        if (g.No > g.List.Count - 1) { g.A0 = 0; g.Done = true; }
    }

    // ------------------------------------------------------- obj_spearblocker --
    protected class Blocker { public double X = 320, Y = 240, Dir = 270, Ideal = 270, DrawDir = 270, Lesson, Rating, Dmg; public int Flash, Neg, Refuse, Buffer; public bool Red; }
    protected void BlkNew(Fight f, double lessonNo, double ratingNo, double dmg)
    {
        var b = new Blocker { Lesson = lessonNo, Rating = ratingNo, Dmg = dmg };
        blk = b;
        var c = new LessonCtx { lesson = lessonNo, rating = ratingNo, genRating = ratingNo, genDmg = 7, dmg = dmg, refuse = 0, tt = tt };
        gen = GenNew(f, c);
        b.Rating = c.rating; b.Dmg = c.dmg; b.Refuse = (int)c.refuse; tt = (int)c.tt;
    }
    // the turn ends once every spear is dealt with (or time runs out, or the SOUL went red)
    protected void BlkStep(Fight f, Action<Blocker> onEnd)
    {
        var b = blk; if (b == null) return;
        bool ender = false; b.Buffer++;
        if (b.Refuse != 2)
        {
            if (tt < 1) { tt = -1; ender = true; }
            if (!green) ender = true;
            if (b.Buffer > 30 && Arrows(f).Count == 0 && gen != null && gen.Done) ender = true;
        }
        if (ender) { foreach (var a in Arrows(f)) a.Alive = false; blk = null; onEnd(b); }
    }
    // the arrow keys turn the shield; a spear on its line is blocked, one that reaches the middle hits
    protected void BlkLogic(Fight f)
    {
        var b = blk; if (b == null) return;
        b.Red = b.Flash >= 2; b.Flash--; b.DrawDir = b.Dir;
        if (Held(K.Down)) b.Ideal = 90; if (Held(K.Up)) b.Ideal = 270; if (Held(K.Left)) b.Ideal = 0; if (Held(K.Right)) b.Ideal = 180;
        if (b.Dir != b.Ideal)
        {
            if (b.Ideal == 0 && b.Dir > 180) { b.Neg = 1; b.Dir -= 360; }
            if (b.Dir >= 0 && b.Dir < 90 && b.Ideal == 270) { b.Neg = 2; b.Dir = 360; }
            const double k = .666666666666667;
            if (b.Neg == 0)
            {
                b.Dir %= 360; double dif = b.Ideal - b.Dir; b.Dir += dif * k;
                if (b.Dir < 0) b.Dir += 360;
                if (Math.Abs(dif) < 15) { b.Dir = b.Ideal; b.Neg = 0; }
            }
            if (b.Neg == 1)
            {
                b.Dir %= 360; double dif = Math.Abs(b.Ideal - b.Dir); b.Dir += dif * k;
                if (Math.Abs(Math.Abs(b.Ideal) - Math.Abs(b.Dir)) < 15) { b.Dir = b.Ideal; b.Neg = 0; }
            }
            if (b.Neg == 2)
            {
                double dif = b.Ideal - b.Dir; b.Dir += dif * k;
                if (Math.Abs(Math.Abs(b.Ideal) - Math.Abs(b.Dir)) < 15) { b.Dir = b.Ideal; b.Neg = 0; }
            }
        }
        double t = b.Ideal * Math.PI / 180, r = 30, x = b.X, y = b.Y, c = Math.Cos(t), s = Math.Sin(t);
        double x1 = x - c * r - s * r, y1 = y + s * r - c * r, x2 = x - c * r + s * r, y2 = y + s * r + c * r;
        var all = Arrows(f);
        var col = all.FirstOrDefault(a => a.Active && SegRect(x1, y1, x2, y2, ArrowRect(a).l, ArrowRect(a).t, ArrowRect(a).r, ArrowRect(a).b));
        if (col != null)
        {
            col.Alive = false; foreach (var a in all) a.Down = true;
            b.Dir = b.Ideal; b.Flash = 5; Assets.Play("mdBell");
        }
        var col2 = all.FirstOrDefault(a => a.Alive && a.Active && ArrowRect(a) is var q && q.l <= x + 5 && q.r >= x - 5 && q.t <= y + 5 && q.b >= y - 5);
        if (col2 != null)
        {
            col2.Alive = false; foreach (var a in all) a.Down = true;
            if (f.Invc < 1) hitno++;
            HitStd(f, ref b.Dmg);
        }
    }
    protected void BlkDraw()
    {
        var b = blk; if (b == null) return;
        double t = b.DrawDir * Math.PI / 180, r = 30, x = b.X, y = b.Y, c = Math.Cos(t), s = Math.Sin(t);
        var XB = Gfx.Hex("#4040ff"); // merge_color(c_blue, c_white, .25)
        double x1 = x - c * r - s * r, y1 = y + s * r - c * r;
        Gfx.Line(x1, y1, x - c * r + s * r, y + s * r + c * r, 3, b.Red ? Gfx.Hex("#ff0000") : XB, true);
        double t2 = t - .2, c2 = Math.Cos(t2), s2 = Math.Sin(t2);
        Gfx.Line(x1, y1, x - c2 * r / 2 + s2 * r / 2, y + s2 * r / 2 + c2 * r / 2, 3, XB, true);
        // draw_circle with circle precision 12, outline, c_green
        var g = Gfx.Hex("#008000");
        for (int i = 0; i < 12; i++)
        {
            double a0 = i / 12.0 * Math.PI * 2, a1 = (i + 1) / 12.0 * Math.PI * 2;
            Gfx.Line(x + Math.Cos(a0) * r, y + Math.Sin(a0) * r, x + Math.Cos(a1) * r, y + Math.Sin(a1) * r, 1, g);
        }
    }

    // ------------------------------------------------- spears on the red SOUL --
    protected class Spear : Bullet { public string Kind = ""; public bool Deact; }
    // obj_spearbullet_follow: appears around the SOUL, spins in, then shoots at it
    protected void SpawnFollow(Fight f)
    {
        var h = f.H;
        double x = h.X - 4 + f.Rnd(8), y = h.Y - 4 + f.Rnd(8);
        double od = PDir(x, y, h.X, h.Y);
        x += Fight.DCos(od) * 140; y -= Fight.DSin(od) * 140;
        Assets.Play("undyneSpearAppear");
        double dir = od, sp = 4, fric = .2, ang = 0; int rot = 32;
        var b = new Spear { Kind = "follow", X = x, Y = y, Alpha = 0, Dmg = spearDmg, Manual = true, WallKill = false, Kill = false, Box = new double[] { -40, -40, 80, 80 } };
        b.OnHit = _ => false;
        b.Step = _ =>
        {
            b.Alpha += .05; ang -= rot; if (rot > 0) rot--;
            if (rot == 0 && sp < 1)
            {
                Assets.Play("undyneArrow");
                dir = PDir(b.X, b.Y, h.X + 10, h.Y + 10); sp = 3; fric = -.3; ang = dir;
            }
            double xo = Fight.DCos(dir) * 25, yo = -Fight.DSin(dir) * 25;
            if (rot == 0 && !b.Deact && f.MnFight == 2 && h.Visible && LineHitsHeart(h, b.X - xo / 2, b.Y - yo / 2, b.X + xo, b.Y + yo)) HitStd(f, ref b.Dmg);
            if (b.Deact) { b.Alpha -= .1; if (b.Alpha <= 0) b.Alive = false; }
            // GameMaker motion: friction, then the move
            if (fric > 0) sp = Math.Max(0, sp - fric); else if (fric < 0) sp -= fric;
            b.X += Fight.DCos(dir) * sp; b.Y -= Fight.DSin(dir) * sp;
            if (b.X < -60 || b.X > 700 || b.Y < -60 || b.Y > 540) b.Alive = false;
            if (tt < 1) b.Alive = false;
        };
        b.Draw = _ =>
        {
            double a = Math.Clamp(b.Alpha, 0, 1);
            Gfx.DrawRot(Assets.Tex("nbUndWhiteSpear"), b.X, b.Y, 30, 30, ang, alpha: a);
            if (rot == 0)
            {
                double xo = Fight.DCos(dir) * 25, yo = -Fight.DSin(dir) * 25;
                Gfx.Line(b.X - xo / 2, b.Y - yo / 2, b.X + xo, b.Y + yo, 1, Gfx.A(Color.White, a));
            }
        };
        f.Spawn(b);
    }
    // obj_risespearbullet: rises out of the floor of the box
    protected void SpawnRise(Fight f, double x, double y)
    {
        bool vis = false; int a1 = 1, a0 = 20, part = 0; double vs = 0;
        var b = new Spear { Kind = "rise", X = x, Y = y, Dmg = spearDmg, Manual = true, WallKill = false, Kill = false, Clip = true, Box = new double[] { 8, 6, 7, 54 }, Hidden = true };
        b.Step = _ =>
        {
            if (Tick(ref a1)) { vis = true; vs = -1; Assets.Play("undyneSpearAppear"); }
            b.Hidden = !vis;
            if (Tick(ref a0))
            {
                if (part == 3) part = 4;
                else if (part == 2) { vs = 0; part = 3; a0 = 2; }
                else if (part == 1) { vs = -10; Assets.Play("undyneSpearRise"); part = 2; a0 = 6; }
                else if (part == 0) { part = 1; vs = 0; a0 = 12; }
            }
            if (part == 4 || b.Deact) { b.Alpha -= .1; if (b.Alpha <= 0) b.Alive = false; }
            b.Y += vs;
        };
        b.OnHit = _ => { if (part > 0 && part < 4 && !b.Deact) HitStd(f, ref b.Dmg); return false; };
        b.Draw = _ => { if (vis) Gfx.Draw(Assets.Tex("nbUndRiseSpear"), Math.Round(b.X), Math.Round(b.Y), alpha: Math.Max(0, b.Alpha)); };
        f.Spawn(b);
    }
    // the spear thrown across the box when the SOUL turns back to red (blt_parent_noborder)
    protected static void SpawnCross(Fight f, double y, double hs)
    {
        var b = new Bullet { X = -10, Y = y, VX = hs, Dmg = 6, WallKill = false, Kill = true, Box = new double[] { 0, 13, 53, 7 }, Img = "nbUndSpearR" };
        b.Step = bb => { if (bb.X > 700) bb.Alive = false; };
        f.Spawn(b);
    }
    protected static void DeactAll(Fight f, string kind) { foreach (var b in f.Bullets.OfType<Spear>()) if (b.Kind == kind) b.Deact = true; }

    public override void DrawOver(Fight f, Monster m) => BlkDraw();
}

// NB.undText: Undyne's lines (exported from the web build into assets/data/undText.json)
public static class UndText
{
    static JsonElement? root;
    static JsonElement R => root ??= Assets.Json("data/undText.json");
    public static string S(string k) => R.GetProperty(k).GetString() ?? "";
    public static string[] A(string k) => R.GetProperty(k).EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
    public static string In(string k, string sub) => R.GetProperty(k).GetProperty(sub).GetString() ?? "";
    public static string? InOpt(string k, string sub) => R.GetProperty(k).TryGetProperty(sub, out var v) ? v.GetString() : null;
    public static string[] Pages(string k) => R.GetProperty(k).ValueKind == JsonValueKind.Array ? A(k) : new[] { S(k) };
    public static string At(string k, int i) => R.GetProperty(k)[i].GetString() ?? "";
}
