using Raylib_cs;
using UTBattle.Engine;
using T = UTBattle.Battle.Monsters.UndText;

namespace UTBattle.Battle.Monsters;

// obj_undyneboss + obj_undyneb_body, obj_undynesiner2 (the melting waver), obj_boxsiner
public class Undyne : UndyneBase
{
    // global.flag[99] (encounters) and flag[351] (her HP) carry over when you run away
    static int f99;
    static double f351 = 1500;

    public override string Name => "Undyne";
    public override double Hp => 1500;
    public override double Atk => 7;
    public override double MercyMod => -9999999999;
    public override int Wd => 250;
    public override string Encounter => " ";
    public override string? MusicKey => null; // Spear of Justice starts after her intro line
    public override (int w, int h) Size => (174, 238);
    public override (double x, double y)? Pos(int i, int n, double cx) => (X0, Y0);
    public override (double x, double y)? DmgPos(Monster m) => (m.X0, m.Y + 80);

    int hitfirst, berserk, mercied, dc, borderspec, defadjust, img, shakify;
    double sx, sy;
    int mc;
    bool started, finale;
    RedGen? rgen;
    class RedGen { public string Kind = ""; public int A0, Mem = -1; }

    public override void Setup(Fight f, Monster m)
    {
        f99++;
        if (f99 == 1) f351 = 1500;
        m.MaxHp = 1500; m.Hp = f351;
        lesson = 1; order = 1; rating = 16;
        face = f99 == 2 ? 1 : f99 == 3 ? 4 : f99 > 3 ? 5 : 0;
        body = new Body();
    }
    public override void OnExit(Fight f, Monster m, string reason) { if (reason == "win" || reason == "gameover") { f99 = 0; f351 = 1500; } }

    // ------------------------------------------------------ obj_undyneb_body --
    // seven obj_npc_marker parts at 2x; depth = body depth + index (legs at the back)
    class Part { public double X, Y, X0, Y0; }
    class Smear { public double X, Y, A; public int F; }
    class Body
    {
        public readonly Dictionary<string, Part> P = new();
        public double Siner, Hair, Slash, LarmA, RarmA, Alpha = 1, Ia = 1; public int Move, A1; public bool Fade; public Smear? Smear;
        public Body()
        {
            foreach (var (k, x, y) in new[] { ("head", 80, 0), ("hair", 4, -4), ("armor", 52, 56), ("rarm", 148, 78), ("pants", 78, 120), ("larm", 82, 66), ("legs", 78, 154) })
                P[k] = new Part { X = X0 + x, Y = Y0 + y, X0 = X0 + x, Y0 = Y0 + y };
        }
    }
    Body? body;
    class Melt { public int F; public double A; }
    Melt? melt;
    // User Event 2: the spear slash that turns the SOUL green or red
    static void BodySlash(Body? b) { if (b != null && b.Move == 0) b.Move = 2; }
    void BodyStep()
    {
        var b = body; if (b == null) return;
        b.Hair += .25;
        if (b.A1 > 0 && --b.A1 <= 0) b.Slash++;
        var p = b.P;
        if (b.Move == 0)
        {
            b.Siner += 1.2;
            if (order == -39) b.Siner -= .2; if (order == -38) b.Siner -= .4; if (order == -37) b.Siner -= .6; if (order == -36) b.Siner -= .8; if (order == -35) b.Siner -= 1;
            double s = Math.Sin(b.Siner / 6), s3 = Math.Sin(b.Siner / 3);
            p["hair"].Y = p["hair"].Y0 + s * 4; p["head"].Y = p["head"].Y0 + s * 2; p["armor"].Y = p["armor"].Y0 + s * 4; p["pants"].Y = p["pants"].Y0 + s * 2;
            p["larm"].Y = p["larm"].Y0 + s * 5; p["larm"].X = p["larm"].X0 + s * 5;
            p["rarm"].Y = p["rarm"].Y0 + s * 6 + s3 * 2; p["rarm"].X = p["rarm"].X0 - s3 * 2; p["legs"].Y = p["legs"].Y0;
            if (b.RarmA > 0) b.RarmA -= 15;
            if (b.LarmA < 0) b.LarmA += 15;
        }
        else foreach (var k in new[] { "head", "hair", "armor", "pants" }) { p[k].X = p[k].X0; p[k].Y = p[k].Y0; }
        if (b.Move == 2)
        {
            if (b.Slash == 0 && b.LarmA > -84) b.LarmA -= 25;
            if (b.Slash == 0 && b.LarmA <= -84) { b.LarmA = -84; b.A1 = 5; b.Slash = .1; }
            if (Math.Abs(b.Slash - 1.1) < 1e-9) { b.Slash = 1; b.A1 = 2; }
            if (b.Slash == 1) b.LarmA += 55;
            if (b.Slash == 2) { b.LarmA = 66; b.Smear = new Smear { X = p["larm"].X - 180, Y = p["larm"].Y + 20, A = 1 }; b.Slash = 3; }
            if (b.Slash == 3) { b.Smear!.A -= .1; b.LarmA += .5; if (b.LarmA > 70) { b.Smear = null; b.Slash = 4; b.A1 = 3; } }
            if (b.Slash == 5) { b.LarmA -= 15; if (b.LarmA <= 6) { b.LarmA = 0; b.Slash = 0; b.Move = 0; } }
        }
        if (b.Smear != null) b.Smear.F++;
        // User Event 3 (fadeout): the parts fade out while the melting sprite fades in
        if (b.Fade)
        {
            if (melt != null) { melt.F = 0; melt.A = Math.Min(1, melt.A + .2); }
            b.Ia -= .05; b.Alpha = Math.Max(0, b.Alpha - .2);
            if (b.Ia <= 0) body = null;
        }
    }
    void BodyDraw(Body b, double ox = 0, double oy = 0)
    {
        var p = b.P; double a = b.Alpha; if (a <= 0) return;
        void Put(Texture2D? im, double x, double y) { if (im is { } t) Gfx.Draw(t, Math.Round(x) + ox, Math.Round(y) + oy, t.Width * 2, t.Height * 2, alpha: a); }
        Put(Rv("nbUndLegs", b.Move == 1 ? 1 : 0), p["legs"].X, p["legs"].Y);
        // spr_undyneb_leftarm: origin (35,-6), rotated by image_angle
        Gfx.DrawRot(Assets.Tex("nbUndLarm"), Math.Round(p["larm"].X) + ox, Math.Round(p["larm"].Y) + oy, 35, -6, b.LarmA, 2, 2, alpha: a);
        Put(Assets.Tex("nbUndPants"), p["pants"].X, p["pants"].Y);
        Gfx.DrawRot(Assets.Tex("nbUndRarm"), Math.Round(p["rarm"].X) + ox, Math.Round(p["rarm"].Y) + oy, 0, 0, b.RarmA, 2, 2, alpha: a);
        Put(Assets.Tex("nbUndArmor"), p["armor"].X, p["armor"].Y);
        Put(Rv("nbUndHair", Math.Floor(b.Hair)), p["hair"].X, p["hair"].Y);
        Put(Rv("nbUndFace", face), p["head"].X, p["head"].Y);
        var s = b.Smear;
        if (s != null && s.A > 0) Gfx.Draw(Rv("nbUndSmear", s.F % 2), Math.Round(s.X) + ox, Math.Round(s.Y) + oy, alpha: Math.Max(0, s.A));
    }
    void DrawMelt(double ox = 0, double oy = 0)
    {
        var m = melt; if (m == null || m.A <= 0) return;
        if (Rv("nbUndMelt", m.F) is { } t) Gfx.Draw(t, X0 + ox, Y0 + oy, t.Width * 2, t.Height * 2, alpha: Math.Min(1, m.A));
    }

    // ------------------------------------------------------ obj_undynesiner2 --
    // copies the picture around Undyne and redraws it a row at a time, each row pushed sideways by
    // sin(a/qq)*c, where c starts at ww and drops 0.1 per row (scr_waver_height)
    class Waver { public double A, A0, Qq = 1, Ww = 1, Dww = 1, IdealWw; }
    Waver? waver;
    RenderTexture2D? wcan;
    void WaverUser1()
    {
        var w = waver; if (w == null) return;
        double I = w.IdealWw;
        if (I == 50) { w.IdealWw = .1; if (body != null) { body.Fade = true; melt = new Melt { F = 1, A = 0 }; } }
        if (I == 26) w.IdealWw = 50; if (I == 18) w.IdealWw = 26; if (I == 14) w.IdealWw = 18; if (I == 10) w.IdealWw = 14;
        if (I == 6) w.IdealWw = 10; if (I == 2) w.IdealWw = 6; if (I == 0) w.IdealWw = 2;
    }
    void WaverStep(double top)
    {
        var w = waver; if (w == null) return;
        double ht = Math.Max(1, Math.Round(top - (Y0 - 20)));
        w.A0 = w.A; w.Dww = w.Ww; w.A += ht;
        double ideal2 = w.IdealWw + Random.Shared.NextDouble() * 6;
        if (w.Ww > ideal2) w.Ww--;
        if (w.Ww < ideal2) w.Ww++;
        if (w.IdealWw == .1 && w.Ww < 2) waver = null;
    }
    public override void PreRender(Fight f, Monster m)
    {
        if (waver == null || m.Hurt) return;
        wcan ??= Raylib.LoadRenderTexture(240, 480);
        Raylib.BeginTextureMode(wcan.Value);
        Raylib.ClearBackground(Color.Blank);
        double xx = X0 - 20, yy = Y0 - 20;
        DrawMelt(-xx, -yy);
        if (body != null) BodyDraw(body, -xx, -yy);
        Raylib.EndTextureMode();
    }
    void WaverDraw(Fight f)
    {
        var w = waver; if (w == null || wcan == null) return;
        double xx = X0 - 20, yy = Y0 - 20; int ht = (int)Math.Max(1, Math.Round(f.ET - yy));
        var tex = wcan.Value.Texture; int H = tex.Height;
        double cc = w.Dww, a = w.A0;
        var tint = Gfx.A(Color.White, .9);
        for (int i = Math.Min(ht, H - 1); i > 0; i--)
        {
            a++;
            if (cc > 0) cc -= .1;
            if (cc < 0) cc -= .1;
            // render textures are stored bottom-up: drawn row i is texture row H-1-i
            Raylib.DrawTexturePro(tex, new Rectangle(0, H - 1 - i, 240, 1), new Rectangle((float)(xx + Math.Sin(a / w.Qq) * cc), (float)(yy + i), 240, 1), System.Numerics.Vector2.Zero, 0, tint);
        }
    }

    // --------------------------------------------------------- obj_boxsiner --
    // two rows of six green rectangles that bob on a sine (Undyne has no battle grid)
    double bsSiner, bsAlpha = 1; bool bsFade;
    void BoxsinerDraw()
    {
        if (bsAlpha <= 0) return;
        var c = Gfx.A(Gfx.Hex("#008000"), bsAlpha);
        foreach (var yoff in new[] { 0, 120 })
            for (int i = 0; i < 6; i++)
            {
                double ysin = Math.Sin((i * 3 + bsSiner / 2) / 8) * 20;
                Gfx.RectLines(20 + i * 100, Math.Round(16 + yoff + ysin), 101, 121, c);
            }
    }

    public override void Draw(Fight f, Monster m)
    {
        BoxsinerDraw();
        if (gone) return;
        DrawMelt();
        int im = m.Hurt ? (died ? 2 : 1) : img;
        if (im != 0) Gfx.Draw(Rv("nbUndB", im), Math.Round(m.X + sx), Math.Round(m.Y + sy));
        if (body != null && !m.Hurt) BodyDraw(body);
        if (!m.Hurt) WaverDraw(f);
        DrawDark();
    }

    // ------------------------------------------------------------- the turn --
    void Slash() { if (glock == 0) { a10 = 10; BodySlash(body); glock = 2; } }
    void Darken() { dark = 1; darkA = 0; }
    // alarm[10] flips the SOUL ten steps after the slash
    void Alarm10(Fight f)
    {
        if (!green) { SetSoul(f, true); if (order == 9 || order == 15 || order == 24) a11 = 15; }
        else SetSoul(f, false);
        glock = 0;
        if (lesson == 7 || lesson == 12 || lesson == 21)
        {
            tt = 60;
            SpawnCross(f, f.H.Y, lesson == 7 ? 11 : lesson == 12 ? 13 : 14);
        }
    }
    // mnfight 4: the ratings move with how the turn went, then alarm[7] returns to the menu
    void Mn4(Fight f)
    {
        if (!attacked) return;
        if (hitno > 0)
        {
            if (hitno == 1) rating++;
            if (hitno > 1) rating += 2;
            if (berserk != 0) rating--;
        }
        else
        {
            if (rating >= 10 && lesson > 5) rating -= 2;
            else if (rating > 8) rating--;
            if (berserk != 0 && rating > 6) rating--;
        }
        attacked = false;
        if (f.BorderNo == 12) f.SetBorder(13);
        endT = 10;
        WaverUser1();
    }

    public override string[] Acts(Monster m) => new[] { "Check", "Plead", "Challenge" };
    public override string? Check(Monster m) => T.S("check");
    public override (string, Action?) Act(Fight f, Monster m, int i, string name)
    {
        if (name == "Challenge")
        {
            var map = new Dictionary<int, int> { [8] = 7, [9] = 8, [10] = 9, [11] = 10, [12] = 10, [13] = 11, [14] = 11, [15] = 12, [16] = 12 };
            string text = T.S("chalNo");
            if (map.TryGetValue((int)rating, out var to)) { rating = to; if (ratingb < 6) ratingb++; text = rating == 7 ? T.S("chalUnfair") : T.S("chalYes"); }
            return (con >= 50 ? T.S("noWork") : text, null);
        }
        if (name == "Plead")
        {
            bool e = false;
            var rbm = new Dictionary<int, int> { [7] = 6, [8] = 7, [9] = 7 };
            if (rbm.TryGetValue((int)ratingb, out var rb)) { ratingb = rb; e = true; }
            var rm = new Dictionary<int, int> { [9] = 10, [8] = 10, [7] = 9 };
            if (rm.TryGetValue((int)rating, out var r)) { rating = r; e = true; }
            return (con >= 50 ? T.S("noWork") : (e ? string.Join("", T.A("pleadYes")) : T.S("pleadNo")), null);
        }
        return ("* Nothing happened.", null);
    }
    public override void OnMercy(Fight f, Monster m) => mercied++;
    public override bool NoFlee(Fight f, Monster m) => green || died;
    public override void OnDamaged(Fight f, Monster m, double dmg) { body = new Body(); f351 = m.Hp; }
    public override bool OnLethal(Fight f, Monster m)
    {
        if (died) { f.EndPlayerTurn(); return true; }
        f.StopMusic();
        con = 50; SetSoul(f, false); bsFade = true; order = -40; lesson = -40; died = true;
        f.MnFight = 99; f.MyFight = 99;
        return true;
    }
    public override int? Border(Fight f, Monster m) => talkBorder;

    // alarm[6]: what she says, which box comes next and where the SOUL waits
    public override Say? SpeechEx(Fight f, Monster m)
    {
        if (order == -35)
        {
            // the last stand: no talk, no menu; the closing speech plays out
            finale = true; con = 60; f.MnFight = 99; f.MyFight = 99; f.H.Visible = false;
            return null;
        }
        if (green && !died)
        {
            if (m.Hp < m.MaxHp * .75 && lesson < 5) { lesson = 6; order = 6; dc = 1; }
            if (m.Hp < m.MaxHp * .5 && lesson < 11) { lesson = 11; order = 12; dc = 2; }
            if (m.Hp < m.MaxHp * .2 && lesson < 20) { lesson = 20; order = 22; dc = 3; rating += 2; }
        }
        if (order == 12 && f99 < 2) f99 = 2;
        if (order == 22 && f99 < 3) f99 = 3;
        if (lesson > 19 && !green && defadjust == 0)
        {
            double h = m.Hp, mx = m.MaxHp, d = -15;
            if (h > mx * .3) d = -25; if (h > mx * .4) d = -35; if (h > mx * .5) d = -50; if (h > mx * .75) d = -70; if (h == mx) d = -90;
            m.Df = d; defadjust = 1;
        }
        bool wide = false, unc = false;
        mc = (int)Math.Round(f.Rnd(100));
        string[] pages = { T.S("taunt") };
        if (dc > 0) { pages = new[] { T.In("cause", dc.ToString()) }; wide = true; }
        bool mer = mercied > 0;
        bool O(int n) => order == n;
        if (order > 0)
        {
            if (O(1)) { unc = wide = true; pages = T.A("o1"); }
            if (O(2))
            {
                unc = wide = true; pages = new[] { T.S("o2") };
                if (hitno > 0 && hitfirst == 3) { pages = T.A("hit3"); rating = 6; berserk = 1; order = 12; lesson = 11; hitfirst++; }
                if (hitno > 0 && hitfirst == 2) { pages = T.A("hit2"); order = 1; lesson = 1; hitfirst++; }
                if (hitno > 0 && hitfirst == 1) { pages = T.A("hit1"); order = 1; lesson = 1; hitfirst++; }
                if (hitno > 0 && hitfirst == 0) { pages = T.A("hit0"); order = 1; lesson = 1; hitfirst++; }
            }
            foreach (var n in new[] { 3, 4, 5 }) if (O(n)) { wide = true; pages = new[] { T.S("o" + n) }; }
            if (O(6)) { pages = new[] { T.S("o6") }; wide = unc = true; }
            if (O(7)) { wide = true; pages = new[] { T.S("o7") }; if (mer) { unc = true; pages = T.A("o7m"); } }
            if (O(8)) { wide = true; pages = new[] { mer ? T.S("o8m") : T.S("o8") }; }
            foreach (var n in new[] { 9, 10, 11 }) if (O(n)) { wide = true; pages = new[] { T.S("o" + n) }; }
            if (f99 > 1) foreach (var n in new[] { 9, 10, 11 }) if (O(n)) { wide = true; pages = new[] { T.S("r" + n) }; }
            if (O(12) && hitfirst < 3) { pages = new[] { T.S("o12") }; wide = unc = true; }
            for (int n = 13; n <= 21; n++) if (O(n)) { wide = true; pages = new[] { T.S("o" + n) }; }
            if (O(22)) { pages = new[] { T.S("o22") }; wide = unc = true; }
            if (O(23)) { wide = true; pages = new[] { mer ? T.S("o23m") : T.S("o23") }; }
            if (O(24)) { wide = true; pages = new[] { mer ? T.S("o24m") : T.S("o24") }; }
            if (order > 24) { wide = true; pages = new[] { T.S("oX") }; }
        }
        if (order < 0)
        {
            var k = order switch { -40 => "n40", -39 => "n39", -38 => "n38", -37 => "n37", -36 => "n36", _ => null };
            if (k != null) pages = T.A(k);
            wide = unc = true;
        }
        borderspec = mc > 50 && !green ? 1 : 0;
        int border = 6; heartPos = null;
        if (green) { border = 13; heartPos = (312, f.ET + 34); greenlock = 1; }
        else if (borderspec != 0) { border = 14; heartPos = (312, f.ET + 34); }
        talkBorder = border; dc = 0;
        if (!unc) f.TalkLenExact = 110;
        return new Say { Pages = pages, Wide = wide, Hold = unc, Pos = wide ? (BLX, BLY) : (X0 + 245, Y0 + 52), Tx = 15, Typer = 39, Img = wide ? "nbBlconWdShrt" : null, OnFace = d => face = d };
    }

    // mnfight 2: the green SOUL's lesson, or spears that follow you, or spears from below
    public override string? Attack(Fight f, Monster m)
    {
        if (finale) return null;
        var h = f.H;
        f.Turntimer(99999); endNow = false; endT = 0;
        if (borderspec == 0) f.SetBorder(7);
        if (green) { f.SetBorder(12); Darken(); }
        if (heartPos is { } hp) { h.X = hp.x; h.Y = hp.y; }
        hitno = 0; tt = 200; fr = 15;
        if (green) BlkNew(f, lesson, rating, 7);
        else if (mc <= 50)
        {
            Darken(); ratingb++; lesson--; if (ratingb > 9) ratingb = 9; fr = (int)(19 - ratingb);
            rgen = new RedGen { Kind = "follow", A0 = 1 };
        }
        else
        {
            borderspec = 1; ratingb++; lesson--; if (ratingb > 9) ratingb = 9; fr = (int)(23 - ratingb);
            rgen = new RedGen { Kind = "rise", A0 = 10 };
        }
        string? fl = null;
        if (m.Hp < m.MaxHp) fl = T.At("flavHurt", mc >= 80 ? 4 : mc >= 60 ? 3 : mc >= 40 ? 2 : mc >= 20 ? 1 : 0);
        if (m.Hp >= m.MaxHp / 2)
        {
            fl = T.At("flavHigh", Math.Min(9, mc / 10));
            if (order > 22) fl = T.At("flavLate", mc >= 90 ? 4 : mc >= 75 ? 3 : mc >= 50 ? 2 : mc >= 25 ? 1 : 0);
        }
        if (order < 0 && T.InOpt("flavDead", ((int)order).ToString()) is { } fd) fl = fd;
        attacked = true; order++; lesson++;
        return fl;
    }
    // obj_spearbulletfollowgen / obj_risespearbulletgen: both stop at turntimer 3-4; on orders 9 and
    // 15 Undyne turns the SOUL green instead and the turn ends there
    void RedGenStep(Fight f)
    {
        var g = rgen; if (g == null) return;
        if (g.Kind == "follow")
        {
            if (Tick(ref g.A0)) { SpawnFollow(f); g.A0 = fr; }
            if (tt < 3) { DeactAll(f, "follow"); dark = 3; rgen = null; return; }
        }
        else
        {
            if (Tick(ref g.A0) && tt > 8)
            {
                int xset = (int)Math.Floor(f.Rnd(3)); if (xset == g.Mem) xset++; if (xset == 3) xset = 0; g.Mem = xset;
                SpawnRise(f, f.EL + xset * 23, f.EB);
                g.A0 = fr;
            }
            if (tt < 1) { DeactAll(f, "rise"); rgen = null; return; }
        }
        if (tt < 4)
        {
            g.A0 = 0;
            if (g.Kind == "rise") DeactAll(f, "rise");
            if (order == 9 || order == 15)
            {
                tt = 30;
                if (order == 9) lesson = 8; if (order == 15) lesson = 13;
                DeactAll(f, "follow"); DeactAll(f, "rise"); if (g.Kind == "follow") dark = 3;
                Slash();
            }
            rgen = null;
        }
    }
    public override void AttackStep(Fight f, Monster m)
    {
        f.TurnTimer = endNow ? 0 : 99999;
        tt--;
        if (endT > 0 && --endT == 0) endNow = true;
        GenStep(f);
        BlkStep(f, b =>
        {
            if (b.Refuse == 0) Mn4(f);
            if (b.Refuse == 1 && (b.Lesson == 6 || b.Lesson == 11 || b.Lesson == 20)) { dark = 3; Slash(); }
        });
        RedGenStep(f);
        if (!green && tt < 1) Mn4(f);
    }
    public override void EndAttack(Fight f, Monster m) { blk = null; gen = null; rgen = null; if (dark > 0 && dark != 3) dark = 3; }
    public override string Flavor(Fight f, Monster m) => T.S("attacks");

    public override void Step(Fight f, Monster m)
    {
        var h = f.H;
        if (!started) { started = true; f.Wr = null; f.StopMusic(); }
        if (a4 > 0 && --a4 <= 0) con = Math.Round((con + 1) * 10) / 10;
        if (a9 > 0 && --a9 <= 0) { f.End("win"); return; }
        if (a10 > 0 && --a10 <= 0) Alarm10(f);
        if (a11 > 0 && --a11 <= 0 && f.MnFight == 2) Mn4(f);
        if (f.MnFight == 2) BlkLogic(f);
        BodyStep(); WaverStep(f.ET); bsSiner++; if (bsFade) bsAlpha -= .01;
        DarkStep(f);

        // ------------------------------------------------------------- intro --
        if (con == 0) { con = 1; a4 = 30; }
        if (con == 1) { f.MyFight = 99; f.MnFight = 99; h.Visible = true; h.X = 312; h.Y = 232; }
        if (con == 2) { if (f99 < 4) { Slash(); con = 3.1; a4 = 30; } else con = 4.1; }
        if (con == 4.1)
        {
            if (f99 == 2) { lesson = 8; order = 9; }
            if (f99 == 3) { lesson = 13; order = 15; }
            if (f99 >= 4) { lesson = 22; order = 22; }
            Say(f, m, new[] { T.In("intro", Math.Min(5, f99).ToString()) }, 39); con = 5;
        }
        if (con == 5 && say == null) { ToMenu(f, T.S("attacks")); f.PlayMusic("undyne"); con = 6; }
        // ------------------------------------------------------------- death --
        if (con == 50) { body = null; img = 2; shakify = 1; f.MnFight = 99; f.MyFight = 99; f.SetBorder(4); Say(f, m, T.A("d50"), 40); con = 51; }
        if (con == 51 && say == null) { con = 50.1; shakify = 3; a4 = 60; }
        if (con == 51.1) { shakify = 0; img = 3; face = 8; Say(f, m, T.A("d51"), 40); con = 52; }
        if (con == 53 && say == null) { Slash(); con = 54; a4 = 30; }
        if (con == 52 && say == null) { img = 0; body = new Body(); waver = new Waver(); Say(f, m, T.A("d52"), 41); con = 53; }
        if (con == 55) { Say(f, m, T.A("d55"), 41); con = 56; }
        if (con == 56 && say == null) { ToMenu(f, T.S("determined")); con = 57; }
        // ------------------------------------------------------------ finale --
        if (con == 60) { f.StopMusic(); face = 7; Say(f, m, T.A("d60"), 40); con = 61; }
        if (con == 61 && say == null) { face = 8; WaverUser1(); Say(f, m, T.A("d61"), 40); con = 62; }
        if (con == 62 && say == null) { WaverUser1(); Say(f, m, T.A("d62"), 42); con = 63; }
        if (con == 63 && say == null) { con = 64; WaverUser1(); a4 = 120; Say(f, m, T.A("d63"), 42); }
        if (con == 65 && say == null) { con = 66; if (melt != null) melt.F = 1; a4 = 120; Say(f, m, T.A("d65"), 43); }
        if (con == 67 && say == null) { con = 68; if (melt != null) melt.F = 2; a4 = 120; Say(f, m, T.A("d67"), 44); }
        if (con == 69 && say == null) { con = 70; if (melt != null) melt.F = 3; a4 = 120; Say(f, m, T.A("d69"), 45); }
        if (con == 71 && say == null)
        {
            // obj_vaporized on spr_undyneb_lastmelt
            if (Assets.Tex("nbUndLastMelt") is { } lm) f.AddVapor("nbUndLastMelt", X0, Y0, lm.Width, lm.Height);
            melt = null; body = null; waver = null; gone = true; f.XpTotal += 500;
            con = 72; a4 = 200;
        }
        if (con == 73) { con = 74; a9 = 10; }
        if (con >= 73) f.Fade = Math.Min(1, f.Fade + .1);

        if (shakify > 0) { sx = f.Rnd(shakify); sy = f.Rnd(shakify); } else { sx = 0; sy = 0; }
        SayStep(f);
        f351 = m.Hp;
    }
}
