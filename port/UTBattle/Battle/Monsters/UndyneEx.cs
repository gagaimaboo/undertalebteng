using Raylib_cs;
using UTBattle.Engine;

namespace UTBattle.Battle.Monsters;

// obj_undyne_ex + obj_undynex_body; obj_rotspeargen(_gen)/obj_rotspear, obj_followspeargen_2/
// obj_followspear_2. The green SOUL's spear blocking and the follow/rise spears are Undyne's own.
public class UndyneEx : UndyneBase
{
    public override string Name => "Undyne the Undying";
    public override double Hp => 23000;
    public override double Atk => 12;
    public override double Def => 5;
    public override double MercyMod => -9999999999;
    public override int Wd => 200;
    public override string Encounter => "* The heroine appears.";
    public override string? MusicKey => "undyneex";
    public override (int w, int h) Size => (200, 200);
    public override (double x, double y)? Pos(int i, int n, double cx) => (X0, Y0);
    public override (double x, double y)? DmgPos(Monster m) => (m.X0, m.Y + 150);
    public override bool NoFlee(Fight f, Monster m) => true;

    const string Check_ = "* UNDYNE THE UNDYING 99ATK 99DEF&* Heroine reformed by her own&  DETERMINATION to save Earth./^";
    const string Wind = "* The wind is howling...";
    static readonly string[] D52 = { "Damn it.../", "So even THAT&power..^1.&It wasn't enough...?/", ".../", "\\E1Heh.../", "Heheheh.../", "\\E2If you..^1./", "If you think I'm&gonna give up hope^1,&you're wrong./", "'Cause I've..^1.&Got my friends&behind me./", "\\E3Alphys told me that&she would watch me&fight you.../", "\\E4And if anything went&wrong^1, she would..^1.&evacuate everyone./", "\\E5By now she's called&ASGORE and told him&to absorb the 6&human SOULs./%%" };
    static readonly string[] D53 = { "And with that&power.../%%" };
    static readonly string[] D54 = { "This world will&live on...!/%%" };

    int orderb;
    class RedGen { public string Kind = ""; public int A0, Mem = -1; }
    class RotGen { public int Type, T, A0; }
    class F2Gen { public double CurAng; public int Rate, A0; }
    RedGen? rgen; RotGen? rot; F2Gen? f2;
    class MeltPos { public double X, Y; public int F; }
    MeltPos? melt;

    public override void Setup(Fight f, Monster m)
    {
        lesson = -5; order = 1; rating = 9; spearDmg = 11;
        body = new Body();
        SetSoul(f, true); // she starts you off green
    }

    // ------------------------------------------------------- obj_undynex_body --
    class Larm { public double X = X0 + 64, Y = Y0 + 78, Ang; public bool Vis, Front; }
    class Smear { public double X, Y, Y2, A; public int F; }
    class Body
    {
        public double X = X0, Y = Y0, Siner, SlashNo, HeadY; public int EyeTimer, Move, ArmV, FaceType, Pause, A1; public double Shakify;
        public Larm L = new(); public Smear? Smear;
    }
    Body? body;
    // User Event 1: the windup and slash with her left arm (movetype 2)
    static void BodySlash(Body b) { if (b.Move == 0) b.Move = 2; }
    void BodyStep(Fight f, Body b)
    {
        // alarm[1]: slashno++ (0.1 -> 1.1 starts the two-step swing)
        if (b.A1 > 0 && --b.A1 <= 0) b.SlashNo = Math.Round((b.SlashNo + 1) * 10) / 10;
        var L = b.L;
        if (b.Move == 2)
        {
            L.Vis = true;
            if (b.SlashNo == 0 && L.Ang > -104) { b.FaceType = 1; L.Ang -= 35; L.X -= 4; L.Y -= 4; b.HeadY -= 2; }
            if (b.SlashNo == 0 && L.Ang <= -104) { L.Ang = -104; b.A1 = 5; b.SlashNo = .1; }
            if (b.SlashNo == 1.1) { b.SlashNo = 1; b.A1 = 2; }
            if (b.SlashNo == 1) { b.FaceType = 0; L.Front = true; L.Ang += 73.3333333333333; }
            if (b.SlashNo == 2)
            {
                L.X = X0 + 64; L.Y = Y0 + 78; L.Ang = 66;
                b.Smear = new Smear { X = L.X - 180, Y = L.Y + 20, Y2 = L.Y + 20, A = 1 };
                b.SlashNo = 3; Assets.Play("undyneSpearAppear");
            }
            if (b.SlashNo == 3)
            {
                if (b.HeadY < 6) b.HeadY += 3;
                var s = b.Smear; if (s != null) { s.A -= .1; s.Y2 += 24; s.F++; }
                L.Ang += .5;
                if (L.Ang > 70) { b.Smear = null; b.SlashNo = 4; b.A1 = 3; }
            }
            if (b.SlashNo == 5)
            {
                if (b.HeadY > 0) b.HeadY--;
                L.Ang -= 15;
                if (L.Ang <= 6) { b.HeadY = 0; L.Front = false; L.Ang = 0; b.SlashNo = 0; b.Move = 0; b.ArmV = 0; L.Vis = false; }
            }
        }
        b.Siner += 1.4; if (b.Pause != 0) b.Siner = 0;
        if (b.FaceType != 0) b.EyeTimer = 0;
        b.EyeTimer++; if (b.EyeTimer >= 41) b.EyeTimer = 0;
        if (b.Move == 2) b.ArmV = 1;
        if (b.Shakify > 0) { b.X = X0 + f.Rnd(b.Shakify) - f.Rnd(b.Shakify); b.Y = Y0 + f.Rnd(b.Shakify) - f.Rnd(b.Shakify); } else { b.X = X0; b.Y = Y0; }
    }
    // draw_sprite_ext with the sprite's origin
    static void Dse(Texture2D? im, double x, double y, double xs, double ys, double rot, double alpha, double ox, double oy)
    {
        if (im == null || alpha <= 0 || xs == 0 || ys == 0) return;
        if (rot == 0) { x = Math.Round(x); y = Math.Round(y); }
        Gfx.DrawRot(im, x, y, ox, oy, rot, xs, ys, alpha: Math.Min(1, alpha));
    }
    void BodyDraw(Body b)
    {
        double x = b.X, y = b.Y, sf = Math.Sin(b.Siner / 6), sf2 = Math.Sin(b.Siner / 3), sf3 = Math.Sin(b.Siner / 14);
        var L = b.L;
        void LarmDraw() { if (L.Vis) Dse(Assets.Tex("nbUxLarm"), L.X + (b.X - X0), L.Y + (b.Y - Y0), 2, 2, L.Ang, 1, 33, 5); }
        if (!L.Front) LarmDraw();
        Dse(Assets.Tex("nbUxHair"), x + 85, y + sf * 3 + b.HeadY + 4, 2, 2, 70 - sf * 15, 1, 16, 30);
        Dse(Assets.Tex("nbUxLegs"), x + 100, y + 164, 2, 2, 0, 1, 20, 10);
        if (b.ArmV == 0) Dse(Assets.Tex("nbUxLarm"), x + 64 + sf * 5, y + 78 + sf * 5, 2, 2, 0, 1, 33, 5);
        Dse(Assets.Tex("nbUxRarm"), x + 136 + sf2 * 3, y + 78 + sf * 6 + sf2 * 2, 2, 2, 0, 1, 4, 6);
        Dse(Assets.Tex("nbUxTorso"), x + 100, y + 78 + sf * 4, 2, 2, -(sf * 4), 1, 39, 29);
        Dse(Assets.Tex("nbUxPants"), x + 100, y + 122 + sf * 2, 2, 2, sf * 2, 1, 20, 0);
        double fy = y + 28 + sf * 2 + b.HeadY; int frn = (int)Math.Floor(b.Siner / 3);
        if (b.FaceType == 0)
        {
            if (b.Pause == 0) Dse(Rv("nbUxFace1", frn), x + 100, fy, 2, 2, 0, 1, 20, 17);
            else Dse(Rv("nbUxFaceDmg", frn), x + 100, fy, 2, 2, 0, 1, 20, 16);
        }
        if (b.FaceType == 1) Dse(Rv("nbUxFaceLaugh", frn), x + 100, fy, 2, 2, 0, 1, 18, 16);
        if (b.FaceType == 2) Dse(Rv("nbUxFaceE", face), x + 100, fy, 2, 2, 0, 1, 20, 16);
        if (b.EyeTimer >= 10)
        {
            int e = b.EyeTimer - 10;
            Dse(Assets.Tex("nbUxEyebeam"), x + 110, y + 24 + sf * 2, e / 4.0, 2.5 - e / 20.0, -(sf3 * 32), Math.Min(1, 1.5 - e / 20.0), 0, 12);
        }
        if (L.Front) LarmDraw();
        var s = b.Smear;
        if (s != null && s.A > 0)
        {
            var im = Rv("nbUndSmear", s.F % 2);
            Gfx.Draw(im, Math.Round(s.X), Math.Round(s.Y), alpha: Math.Max(0, s.A));
            Gfx.Draw(im, Math.Round(s.X), Math.Round(s.Y2), alpha: Math.Max(0, s.A));
        }
    }

    public override void Draw(Fight f, Monster m)
    {
        if (gone) return;
        if (melt != null) Gfx.Draw(Rv("nbUxMelt", melt.F), Math.Round(melt.X), Math.Round(melt.Y));
        else if (body != null) BodyDraw(body);
        DrawDark();
    }

    // ------------------------------- scr_damagestandard_x (obj_followspear_2) --
    // a spear that would kill leaves you at 1 HP; at 1 HP it kills
    static bool HitX(Fight f, ref double dmg)
    {
        if (f.Hp <= 1) return HitStd(f, ref dmg);
        double hp = f.Hp; int bonus = hp > 20 ? 1 : 0; for (int t = 30; t <= 90; t += 10) if (hp >= t) bonus++;
        dmg += bonus; return f.DamagePlayer(Math.Min(dmg, hp - 1));
    }
    static void DrawSpear(double x, double y, double ang, double a) => Dse(Assets.Tex("nbUxSpear2"), x, y, 1, 1, ang, Math.Clamp(a, 0, 1), 30, 30);

    // ------------------------------------------ obj_rotspeargen + obj_rotspear --
    // a ring of spears pointing in at where the SOUL was, spinning and closing in
    class RingSpear { public double X, Y, Ang, Alpha, Dmg = 12; public bool Deact, Gone; }
    class Ring : Bullet { public RingSpear[] Sp = Array.Empty<RingSpear>(); public bool RingGone; }
    void SpawnRing(Fight f, double x, double y, int type)
    {
        var cfg = new (double? ca, double rs, double rm, int num, double rr)[] { (0, 8, 2, 7, 220), (0, -8, -2, 7, 220), (null, 8, 2, 8, 230), (null, -8, -2, 8, 230) }[type];
        double curang = cfg.ca ?? f.Rnd(360), rotspeed = cfg.rs, rotmin = cfg.rm, rr = cfg.rr; int num = cfg.num;
        var b = new Ring { X = x, Y = y, Manual = true, WallKill = false, Kill = false, Box = new double[] { 0, 0, 0, 0 } };
        b.Sp = Enumerable.Range(0, num).Select(_ => new RingSpear()).ToArray();
        b.OnHit = _ => false;
        b.Step = _ =>
        {
            if (rotspeed > rotmin) rotspeed -= .2; if (rotspeed < rotmin) rotspeed += .2;
            for (int i = 0; i < num; i++)
            {
                var s = b.Sp[i]; if (s.Gone) continue;
                double a = curang + (double)i / num * 360;
                s.X = b.X + Fight.DCos(a) * rr; s.Y = b.Y - Fight.DSin(a) * rr; s.Ang = PDir(s.X, s.Y, b.X, b.Y);
                if (rr < 8) s.Deact = true;
            }
            if (rr < 8) { rr++; rotspeed *= .8; }
            if (rr < -20) b.RingGone = true;
            rr -= 4; curang += rotspeed;
            var h = f.H;
            foreach (var s in b.Sp)
            {
                if (s.Gone) continue;
                if (!s.Deact && s.Alpha < 1) s.Alpha += .2;
                if (s.Deact) { s.Alpha -= .2; if (s.Alpha < .3) { s.Gone = true; continue; } }
                double xo = Fight.DCos(s.Ang) * 25, yo = -Fight.DSin(s.Ang) * 25;
                if (s.Alpha >= .8 && f.MnFight == 2 && h.Visible && LineHitsHeart(h, s.X - xo / 2, s.Y - yo / 2, s.X + xo, s.Y + yo)) HitStd(f, ref s.Dmg);
            }
            if (b.RingGone && b.Sp.All(s => s.Gone)) b.Alive = false;
            if (tt < 1) b.Alive = false;
        };
        b.Draw = _ => { foreach (var s in b.Sp) if (!s.Gone) DrawSpear(s.X, s.Y, s.Ang, s.Alpha); };
        f.Spawn(b);
    }
    void RotGenStep(Fight f)
    {
        var g = rot; if (g == null) return;
        var h = f.H;
        if (Tick(ref g.A0))
        {
            if (g.Type == 0) { SpawnRing(f, h.X + 8, h.Y + 8, g.T); g.T = g.T != 0 ? 0 : 1; g.A0 = 27; }
            else { SpawnRing(f, h.X + 8, h.Y + 8, f.Rnd(2) < 1 ? 2 : 3); g.A0 = 24; }
        }
        if (tt < 4)
        {
            if (orderb == 4) { tt = 30; a11 = 40; if (order == 11) lesson = -11; Slash(); }
            foreach (var b in f.Bullets.OfType<Ring>()) { b.RingGone = true; foreach (var s in b.Sp) s.Deact = true; }
            rot = null;
        }
    }

    // --------------------------------- obj_followspeargen_2 / obj_followspear_2 --
    // six spears at a time from a circle around you; each spins in, points at where you were,
    // then shoots straight through
    void SpawnF2(Fight f, double x, double y)
    {
        var h = f.H; double off = PDir(x, y, h.X + 8, h.Y + 8);
        double rotv = 38, ang = off + 20, dir = off, sp = 4, fric = .2; int timer = 0; bool fade = true;
        var b = new Spear { Kind = "f2", X = x, Y = y, Alpha = 0, Dmg = 11, Manual = true, WallKill = false, Kill = false, Box = new double[] { 0, 0, 0, 0 } };
        b.OnHit = _ => false;
        b.Step = _ =>
        {
            b.Alpha += .05; ang -= rotv; if (rotv > 0) rotv -= 2;
            if (rotv == 0 && sp < 1) { timer++; if (timer == 5) { sp = 8; fric = -.3; dir = ang; Assets.Play("undyneArrow"); } }
            if (fade && sp >= 7) { timer++; if (timer >= 22) b.Deact = true; }
            double xo = Fight.DCos(dir) * 25, yo = -Fight.DSin(dir) * 25;
            if (rotv == 0 && !b.Deact && f.MnFight == 2 && h.Visible && LineHitsHeart(h, b.X - xo / 2, b.Y - yo / 2, b.X + xo, b.Y + yo)) HitX(f, ref b.Dmg);
            if (b.Deact) { b.Alpha -= .25; if (b.Alpha <= 0) b.Alive = false; }
            if (fric > 0) sp = Math.Max(0, sp - fric); else if (fric < 0) sp -= fric;
            b.X += Fight.DCos(dir) * sp; b.Y -= Fight.DSin(dir) * sp;
            if (tt < 1) b.Alive = false;
        };
        b.Draw = _ => DrawSpear(b.X, b.Y, ang, b.Alpha);
        f.Spawn(b);
    }
    void F2GenStep(Fight f)
    {
        var g = f2; if (g == null) return;
        var h = f.H;
        if (Tick(ref g.A0))
        {
            for (int i = 0; i < 6; i++) { double a = g.CurAng + i / 6.0 * 360; SpawnF2(f, h.X + 8 + Fight.DCos(a) * 180, h.Y + 8 - Fight.DSin(a) * 180); }
            Assets.Play("undyneSpearAppear");
            if (g.Rate > 10) g.Rate--;
            g.CurAng += 10 + new[] { 10, 20, 30 }[(int)Math.Floor(f.Rnd(3))];
            g.A0 = g.Rate;
        }
        if (tt < 3) { DeactAll(f, "f2"); f2 = null; }
    }

    // ---------------------------- obj_spearbulletfollowgen / obj_risespearbulletgen --
    // on her order 6 the red turn hands the SOUL back to green instead
    void RedGenStep(Fight f)
    {
        var g = rgen; if (g == null) return;
        if (g.Kind == "follow") { if (Tick(ref g.A0)) { SpawnFollow(f); g.A0 = fr; } }
        else if (Tick(ref g.A0) && tt > 8)
        {
            int xset = (int)Math.Floor(f.Rnd(3)); if (xset == g.Mem) xset++; if (xset == 3) xset = 0; g.Mem = xset;
            SpawnRise(f, f.EL + xset * 23, f.EB); g.A0 = fr;
        }
        if (tt < 4)
        {
            g.A0 = 0; DeactAll(f, g.Kind);
            if (g.Kind == "follow") dark = 3;
            if (order == 6) { tt = 30; a11 = 40; lesson = -8; dark = 3; Slash(); }
            rgen = null;
        }
    }

    // ------------------------------------------------------------- the turn --
    // User Event 1 (Other 11): she slashes, and alarm[10] flips the SOUL ten steps later
    void Slash() { if (glock == 0 && body != null) { a10 = 10; BodySlash(body); glock = 2; } }
    void Alarm10(Fight f)
    {
        if (!green) { SetSoul(f, true); if (order == 3 || order == 7 || order == 12) a11 = 15; }
        else SetSoul(f, false);
        glock = 0;
        if (lesson == -7 || lesson == -11 || lesson == -14) tt = 10;
    }
    // mnfight 4: a turn you got hit in makes her faster, a clean one slower; alarm[7] ends it
    void Mn4(Fight f)
    {
        if (!attacked) return;
        if (hitno > 0) { if (hitno >= 1 && rating < 10) rating++; if (hitno >= 3 && rating < 10) rating = 10; }
        else if (rating > 8) rating--;
        attacked = false;
        if (f.BorderNo == 12) f.SetBorder(13);
        endT = 10;
    }

    public override string[] Acts(Monster m) => new[] { "Check" };
    public override string? Check(Monster m) => Check_;
    // obj_undyne_ex Alarm 3: FIGHT hits for 21 times as much, never under 600
    public override double OnHitCalc(Fight f, Monster m, double dmg)
    {
        if (!(dmg > 0)) return dmg;
        double d = dmg * 21; if (d < 600) d = 600 + Math.Floor(f.Rnd(67));
        if (m.Hp - d <= 0 && body != null) { body.FaceType = 2; face = 0; f.StopMusic(); }
        return d;
    }
    public override bool OnLethal(Fight f, Monster m)
    {
        if (con >= 50) return true;
        f.StopMusic();
        con = 50; died = true; SetSoul(f, false);
        f.MnFight = 99; f.MyFight = 99; f.Bullets.Clear();
        return true;
    }
    public override int? Border(Fight f, Monster m) => talkBorder;
    // alarm[6]: no talking, just which box comes next and where the SOUL waits
    public override Say? SpeechEx(Fight f, Monster m)
    {
        if (con >= 50) return null;
        heartPos = (312, f.ET + 34);
        if (green) { talkBorder = 13; greenlock = 1; }
        else
        {
            talkBorder = 31;
            if (orderb == 0 || orderb == 6) talkBorder = 7;
            if (orderb == 1 || orderb == 7) talkBorder = 14;
        }
        return null;
    }
    // mnfight 2: the lesson on green, or the next red attack in her order
    public override string? Attack(Fight f, Monster m)
    {
        if (con >= 50) return null;
        var h = f.H;
        f.Turntimer(99999); endNow = false; endT = 0;
        if (heartPos is { } hp) { h.X = hp.x; h.Y = hp.y; }
        hitno = 0; tt = 300; fr = 15;
        void Rb() { ratingb = Math.Max(8, Math.Min(10, ratingb + 1)); lesson++; }
        if (green) { f.SetBorder(12); dark = 1; BlkNew(f, lesson, rating, 12); }
        else
        {
            int ob = orderb;
            if (ob == 0 || ob == 6) { tt = 240; dark = 1; Rb(); fr = (int)(18 - ratingb); rgen = new RedGen { Kind = "follow", A0 = 1 }; }
            if (ob == 1 || ob == 7) { tt = 220; Rb(); fr = (int)(23 - ratingb); rgen = new RedGen { Kind = "rise", A0 = 10 }; }
            if (ob == 2 || ob == 3) { tt = 215; lesson++; rot = new RotGen { Type = 0, T = 0, A0 = 1 }; }
            if (ob == 4) { tt = 400; lesson++; f2 = new F2Gen { CurAng = 0, Rate = 20, A0 = 1 }; }
            if (ob == 5) { tt = 215; lesson++; rot = new RotGen { Type = 1, T = 1, A0 = 1 }; }
            orderb++; if (orderb >= 8) orderb = 4;
        }
        attacked = true; order++; lesson--;
        return Wind;
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
            if (b.Refuse == 1 && (b.Lesson == -7 || b.Lesson == -10 || b.Lesson == -14)) { dark = 3; Slash(); }
        });
        RedGenStep(f); RotGenStep(f); F2GenStep(f);
        if (!green && tt < 1) Mn4(f);
    }
    public override void EndAttack(Fight f, Monster m) { blk = null; gen = null; rgen = null; rot = null; f2 = null; if (dark > 0 && dark != 3) dark = 3; }
    public override string Flavor(Fight f, Monster m) => Wind;

    public override void Step(Fight f, Monster m)
    {
        var h = f.H;
        if (a4 > 0 && --a4 <= 0) con++;
        if (a9 > 0 && --a9 <= 0) { f.End("win"); return; }
        if (a10 > 0 && --a10 <= 0) Alarm10(f);
        if (a11 > 0 && --a11 <= 0 && f.MnFight == 2) Mn4(f);
        if (f.MnFight == 2) BlkLogic(f);
        if (body != null) { body.Pause = m.Hurt && con < 50 ? 1 : 0; BodyStep(f, body); }
        DarkStep(f);
        // ------------------------------------------------------------- death --
        if (con == 50) { con = 51; a4 = 90; }
        if (con == 52)
        {
            if (body != null) { body.Shakify = 1; body.FaceType = 2; }
            f.MnFight = 99; f.MyFight = 99; h.Visible = false; f.SetBorder(0);
            Say(f, m, D52, 94); con = 53;
        }
        if (con == 53 && say == null)
        {
            // scr_marker(mypart1.x - 20, -40, spr_undynex_melt)
            melt = new MeltPos { X = X0 - 20, Y = -40, F = 0 }; body = null;
            Say(f, m, D53, 95); con = 54;
        }
        if (con == 54 && say == null) { melt!.F = 1; Say(f, m, D54, 96); con = 71; }
        if (con == 71 && say == null)
        {
            // spr_undynex_melt sits on an opaque black backdrop: only her pixels turn to dust
            var key = RvKey("nbUxMeltKeyed", 1);
            if (Assets.Tex(key) is { } t) f.AddVapor(key, melt!.X, melt.Y, t.Width, t.Height);
            melt = null; gone = true; f.XpTotal += 1500;
            con = 72; a4 = 180;
        }
        if (con == 73) { con = 74; a9 = 10; }
        if (con >= 73) f.Fade = Math.Min(1, f.Fade + .1);
        SayStep(f);
    }
}
