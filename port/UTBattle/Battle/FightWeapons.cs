using Raylib_cs;
using UTBattle.Engine;
using static UTBattle.Engine.Input;

namespace UTBattle.Battle;

// FIGHT, per weapon (scr_attack): the knives get obj_target + obj_targetchoice (one bar), the Tough
// Glove the fist bar (obj_targetchoicefist: then mash Z for up to 4 punches), and Ballet Shoes /
// Burnt Pan / Empty Gun / Torn Notebook obj_shoetargettest's bolts (press as each one crosses
// the middle). The Machine Gun and the Dirty Blaster are the web build's own weapons.
public partial class Fight
{
    public record WeaponInfo(string Id, string Name, double Atk, string Kind);
    public static readonly WeaponInfo[] Weapons =
    {
        new("stick", "Stick", 0, "knife"), new("toyknife", "Toy Knife", 3, "knife"), new("toughglove", "Tough Glove", 5, "fist"),
        new("balletshoes", "Ballet Shoes", 7, "bolts"), new("tornnotebook", "Torn Notebook", 2, "bolts"), new("burntpan", "Burnt Pan", 10, "bolts"),
        new("emptygun", "Empty Gun", 12, "bolts"), new("machinegun", "Machine Gun", 12, "bolts"), new("worndagger", "Worn Dagger", 15, "knife"),
        new("dirtyblaster", "Dirty Blaster", 10, "knife"), new("realknife", "Real Knife", 99, "knife"),
    };
    public static WeaponInfo WeaponOf(string id) => Weapons.FirstOrDefault(w => w.Id == id) ?? Weapons[0];

    public string Weapon = "stick";
    public Action? FightDone;
    static double R(double v) => Math.Floor(v + .5); // JS Math.round

    public class Bolt { public double X, Hs, Alpha = 1; public int Pri; public bool Dead; }
    public class FightBar
    {
        public double Tx, Ty, X, X0, Hs, Alpha = 1, Xs = 1, Blink, Dmg, Score, FatalX;
        public bool Hit, Fade, CursorGone, Crit, Gone;
        public string Kind = "knife", Weapon = "stick", Phase = "";
        public int PunchT, Punches, Num, Shots, Shake;
        public List<Bolt> Bolts = new();
    }

    // ---------------------------------------------------------------- the bar --
    void StartFight()
    {
        MyFight = 1; H.Visible = false; Wr = null;
        // obj_target at (idealborder0+6, idealborder2+6)
        double tx = Ideal[0] + 6, ty = Ideal[2] + 6;
        var w = Weapon; var kind = WeaponOf(w).Kind;
        var f = new FightBar { Tx = tx, Ty = ty, Kind = kind, Weapon = w };
        if (kind == "bolts") { BoltSetup(f); Bar = f; return; }
        // the Stick's bar always starts on the left; the others pick a side (x-16 or x+570)
        bool right = w != "stick" && R(Rnd(1)) == 1;
        double hs = 11 + Rnd(2);
        if (w == "toyknife") hs *= 1.25;
        if (kind == "fist") hs *= 1.2;
        double x = right ? tx + 570 : tx - 16;
        f.X = x; f.X0 = x; f.Hs = right ? -hs : hs;
        Bar = f;
    }
    // obj_shoetargettest Create: where each bolt starts (user events 4 = from the left, 5 = from
    // the right) and how many there are
    void BoltSetup(FightBar f)
    {
        int Ch(params int[] a) => a[(int)Math.Floor(Rnd(a.Length))];
        int type = f.Weapon switch { "balletshoes" => 0, "burntpan" => 1, "emptygun" => 2, "tornnotebook" => 3, _ => 4 };
        double sp = f.Weapon == "emptygun" || f.Weapon == "machinegun" ? 15 : 10;
        f.FatalX = f.Tx + 274;
        void Add(bool fromRight, double wherex)
        {
            int pri = f.Bolts.Count;
            if (fromRight) f.Bolts.Add(new Bolt { X = f.Tx + 546 + wherex - 20, Hs = -sp, Pri = pri }); // obj_ztarget
            else f.Bolts.Add(new Bolt { X = f.Tx - wherex, Hs = sp, Pri = pri });                    // obj_xtarget
        }
        if (type == 0) { Add(false, -50); Add(false, Ch(0, 25, 50)); Add(false, Ch(100, 125, 150)); }
        if (type == 1) { Add(true, -50); Add(true, Ch(0, 25, 50)); Add(true, Ch(100, 125, 150)); Add(true, 200); }
        if (type == 2) { Add(false, 120); Add(false, Ch(180, 210, 240)); Add(false, Ch(300, 330, 360)); Add(false, Ch(400, 430, 460)); }
        if (type == 3) { Add(true, Ch(-50, -25)); Add(true, Ch(0, 25, 50)); }
        // the Machine Gun: fifteen of the gun's bolts, 4-6 steps apart
        if (type == 4) { int at = 120; for (int k = 0; k < 15; k++) { Add(false, at); at += Ch(60, 75, 90); } }
        f.Num = f.Bolts.Count;
    }

    void Miss(FightBar f, Monster m) { StatMisses++; f.Fade = true; f.Hit = true; f.CursorGone = true; f.Phase = "wait"; SpawnDmgWriter(m, 0); Dmgw[^1].Life = 30; EndPlayerTurn(); }

    void FightStep()
    {
        // global.damagetimer: the monster reacts once the weapon's animation has played
        // (FightDone: what runs instead, when the hits have already landed - the Machine Gun)
        if (FightWait > 0 && --FightWait == 0) { var d = FightDone; FightDone = null; if (d != null) d(); else HurtMonster(); }
        var f = Bar; if (f == null) return;
        if (f.Phase == "wait") return;
        if (f.Kind == "bolts") { BoltStep(f); return; }
        if (f.Phase == "punch") { PunchStep(f); return; }
        if (f.Hit) { f.Blink += .4; return; }
        f.X += f.Hs;
        var m = Mons[Target];
        bool Off(double x) => f.Hs > 0 ? x > f.Tx + 546 : x < f.Tx;
        if (Off(f.X)) { Miss(f, m); return; }
        if (!Pressed(K.Confirm)) return;
        Clear(K.Confirm);
        // scr_attackcalc + obj_targetchoice: pwr - def + random(2), scaled by accuracy
        // (x2.2 within 12px of the middle; the glove's fist bar x2.1)
        double dmg = At + WStrength - m.Df + Rnd(2);
        double myx = f.X + 7, perfect = f.Tx + 273, bonus = Math.Abs(myx - perfect);
        if (bonus == 0) bonus = 1;
        double stretch = (546 - bonus) / 546; f.Crit = bonus <= 12;
        dmg = bonus <= 12 ? R(dmg * (f.Kind == "fist" ? 2.1 : 2.2)) : R(dmg * stretch * 2);
        dmg = m.Def.OnHitCalc(this, m, dmg);
        f.Hit = true;
        if (f.Weapon == "dirtyblaster") { f.Fade = true; f.Phase = "wait"; DirtyBlaster(m, Math.Max(0, dmg), f.Crit); return; }
        if (f.Kind == "fist")
        {
            // obj_targetchoicefist: the bar blinks and PRESS Z flashes on the monster for 30 frames
            f.Phase = "punch"; f.Dmg = Math.Max(0, dmg); f.PunchT = 0; f.Punches = 0;
            double cx = m.X + m.Wd / 2.0, cy = m.Y + 50;
            var e = new Fx { X = cx, Y = cy };
            e.StepFn = ee => { ee.T++; if (Bar != f || f.Phase != "punch") ee.Dead = true; };
            e.DrawFn = ee => { if (ee.T % 9 >= 3) WFrame("press", -1, ee.X, ee.Y); };
            FxList.Add(e);
            return;
        }
        m.TakeDamage = Math.Max(0, dmg); f.Fade = true;
        // obj_slice: spr_strike at image_speed .5-stretch/4, scaled stretch*2-.5
        double isp = Math.Max(.1, .5 - stretch / 4), sc = stretch * 2 - .5;
        FxList.Add(new Fx { Kind = "slice", X = m.X + m.Wd / 2.0 - 5, Y = m.Y - 5, Cx = m.X + m.Wd / 2.0 + 5, Cy = m.Y + 45, Sc = sc, Isp = isp });
        Assets.Play("laz");
        f.Phase = "wait"; FightWait = (int)R(1 / isp * 6 + 3);
    }

    // Tough Glove: up to 4 Z presses in 30 frames; damage x punches/4, the 4th a big punch
    void PunchStep(FightBar f)
    {
        var m = Mons[Target];
        f.Blink += .4; f.PunchT++;
        double wd = m.Wd > 0 ? m.Wd : 100, ht = 100;
        if (Pressed(K.Confirm) && f.Punches < 4)
        {
            Clear(K.Confirm); f.Punches++;
            if (f.Punches < 4) PunchFx(m.X + Rnd(wd), m.Y + Rnd(ht), false, false, false);
            else PunchFx(m.X + wd / 2, m.Y + ht / 2, true, false, false);
        }
        if (f.PunchT >= 30 || f.Punches == 4)
        {
            f.Fade = true; f.CursorGone = true;
            if (f.Punches == 0) { Miss(f, m); return; }
            m.TakeDamage = Math.Ceiling(f.Dmg * f.Punches / 4); f.Crit = f.Punches == 4;
            f.Phase = "wait"; FightWait = 10;
        }
    }

    // obj_shoetargettest + obj_xtarget/obj_ztarget: only the first bolt in line takes Z
    void BoltStep(FightBar f)
    {
        var m = Mons[Target];
        // the Machine Gun's shots land as they are fired
        bool burst = f.Weapon == "machinegun";
        if (f.Shake > 0) { f.Shake--; m.X = m.X0 + (f.Shake != 0 ? (f.Shake % 2 != 0 ? 3 : -3) : 0); if (f.Shake == 0) m.Hurt = false; }
        foreach (var b in f.Bolts.ToList())
        {
            if (b.Dead) continue;
            if (b.Pri < 0) b.Pri = 0;
            bool past = b.Hs > 0 ? b.X > f.FatalX : b.X < f.FatalX;
            if (past) { b.Alpha -= .2; if (b.Alpha < .1) { b.Dead = true; foreach (var o in f.Bolts) o.Pri--; continue; } }
            if (b.Pri == 0 && Pressed(K.Confirm))
            {
                Clear(K.Confirm);
                double n = BoltScore(f, b, b.X);
                b.Dead = true; foreach (var o in f.Bolts) o.Pri--;
                if (burst && n > 0 && GunShot(f, m, n, !f.Bolts.Any(o => !o.Dead))) return;
                continue;
            }
            b.X += b.Hs;
        }
        f.Bolts.RemoveAll(b => b.Dead);
        if (f.Bolts.Count > 0 || f.Hit) return;
        f.Hit = true; f.Fade = true; f.CursorGone = true;
        if (f.Score <= 0) { Miss(f, m); return; }
        // Machine Gun, the last bolt missed: its shots have landed; the turn ends once the last one plays
        if (burst)
        {
            m.X = m.X0; m.Hurt = false; StatAttacks++; f.Phase = "wait"; FightWait = 20;
            FightDone = () => { m.Def.OnDamaged(this, m, 0); EndPlayerTurn(); };
            return;
        }
        double dmg = (At + WStrength - m.Df) * f.Score / 160 * 4 / f.Num;
        dmg = R(dmg) + R(Rnd(2));
        dmg = m.Def.OnHitCalc(this, m, dmg);
        m.TakeDamage = Math.Max(0, dmg);
        bool crit = f.Score > 400.0 * f.Num / 4; double cx = m.X + (m.Wd > 0 ? m.Wd : 100) / 2.0, cy = m.Y + 50; f.Crit = crit;
        var w = f.Weapon;
        if (w == "balletshoes") { PunchFx(cx, cy, true, true, crit); FightWait = 10; }
        else if (w == "emptygun") { GunFx(cx, cy, crit); FightWait = 20; }
        else if (w == "burntpan") { PanFx(cx, cy, crit); FightWait = 20; }
        else { BookFx(cx, cy, crit); FightWait = 24; }
        f.Phase = "wait";
    }
    // obj_shoetargettest user event 3: score by how many frames off the middle it was
    double BoltScore(FightBar f, Bolt b, double x)
    {
        double sc = Math.Abs(Math.Floor(x / b.Hs) - Math.Floor(f.FatalX / b.Hs));
        double n = 0;
        if (sc < 28) n = 10; if (sc < 22) n = 15; if (sc < 16) n = 20; if (sc < 10) n = 40; if (sc < 5) n = 50; if (sc < 4) n = 70; if (sc < 3) n = 80; if (sc < 2) n = 90;
        if (sc < 1) { n = 110; x = f.FatalX; }
        f.Score += n;
        FadebarFx(x, f.Ty, sc < 1 ? 2 : sc < 5 ? 1 : 0);
        if (sc < 1) Assets.Play("victor"); else if (sc < 5) Assets.Play("hit");
        // (the gun's bonuses are for four bolts; fifteen would multiply them up again and again)
        if (f.Weapon != "machinegun") { if (f.Score > 430) f.Score *= 1.8; if (f.Score >= 400) f.Score *= 1.25; }
        return n;
    }
    // a Machine Gun shot: the Empty Gun's hit somewhere on the monster, which flinches and loses the
    // HP now (its number pops up off to the side). The last shot, or the one that finishes it, goes
    // through the usual hurt (shudder, death, end of the turn). Returns true when that happened.
    bool GunShot(FightBar f, Monster m, double n, bool last)
    {
        double dmg = R((At + WStrength - m.Df) * n / 220) + R(Rnd(1));
        dmg = Math.Max(0, m.Def.OnHitCalc(this, m, dmg));
        bool crit = n >= 110; f.Shots++;
        Shake = new ShakeState { H = 2, V = 2, Speed = 1 };
        double wd = m.Wd > 0 ? m.Wd : 100;
        if (last || dmg >= m.Hp)
        {
            GunFx(m.X0 + wd / 2, m.Y + 50, crit);
            m.X = m.X0; m.Hurt = false; f.Bolts.Clear(); f.Crit = crit; f.Hit = true; f.Fade = true; f.CursorGone = true; f.Phase = "wait";
            m.TakeDamage = dmg; FightWait = 8; return true;
        }
        GunFx(m.X0 + 10 + Rnd(Math.Max(1, wd - 20)), m.Y + 10 + Rnd(Math.Max(1, m.H * .7)), crit, .5);
        SpawnDmgWriter(m, dmg); var w = Dmgw[^1]; w.Life = 24; w.Ox = R(Rnd(70)) - 35; w.Oy = R(Rnd(36)) - 18;
        StatDmg += (int)dmg;
        m.Hp -= dmg; m.Hurt = true; f.Shake = 6;
        return false;
    }

    // ------------------------------------------------------------- the hits --
    // a frame of the weapon sheet (origin at its centre); idx -1 for the single ones
    static void WFrame(string name, int idx, double x, double y, double scale = 1, double alpha = 1, double angle = 0, bool gold = false, double? sx = null)
    {
        if (alpha <= 0 || scale <= 0) return;
        var key = (gold ? "wpnG_" : "wpn_") + name + (idx >= 0 ? "_" + idx : "");
        if (Assets.Tex(key) is not { } t) return;
        Gfx.DrawFrame(t, Math.Round(x), Math.Round(y), t.Width / 2.0, t.Height / 2.0, sx ?? scale, scale, angle, Math.Min(1, alpha));
    }
    Fx Wfx(Action<Fx> step, Action<Fx> draw, bool top = false) { var e = new Fx { StepFn = step, DrawFn = draw, Top = top }; FxList.Add(e); return e; }

    // obj_lightpunch / obj_strongpunch (spr_regfist/hyperfist, regfoot/hyperfoot for the shoes)
    void PunchFx(double x, double y, bool strong, bool foot, bool crit)
    {
        Assets.Play(strong ? "weaponPunchStrong" : "weaponPunchWeak");
        if (strong) { Shake = new ShakeState { H = 3, V = 3, Speed = 2 }; if (crit) Assets.Play("weaponSaber"); }
        var set = strong ? (foot ? "shoes" : "glove") : (foot ? "shoeSmall" : "gloveSmall");
        var e = Wfx(ee =>
        {
            ee.T++; ee.Frame += .5; double j = strong ? 2 : 1; ee.X += -j + Rnd(j * 2); ee.Y += -j + Rnd(j * 2);
            if (ee.Frame >= (strong ? 6 : 5)) ee.Dead = true;
        }, ee => WFrame(set, (int)Math.Floor(ee.Frame), ee.X, ee.Y, gold: crit));
        e.X = x; e.Y = y;
    }
    // obj_fadebar: grows and fades where a bolt was stopped (red / cyan good / yellow perfect)
    void FadebarFx(double x, double y, int frame)
    {
        var e = Wfx(ee => { ee.Alpha -= .1; ee.Sc += .1; if (ee.Alpha < .1) ee.Dead = true; },
            ee => WFrame("fade", frame, ee.X, ee.Y, ee.Sc, ee.Alpha), top: true);
        e.X = x + 7; e.Y = y + 64; e.Sc = 1; e.Alpha = 1;
    }
    // obj_gunshot_stab (+ obj_gunshot_star x8, obj_gunshot_2 rings x3); k sizes it (the Machine Gun's shots)
    class Star { public double Ang, Siner, Amt, Sp, A = 1, Size = .5; public bool Dead; }
    class Ring { public double Size = 1, A = 1; public int Form; public bool Dead; }
    void GunFx(double x, double y, bool crit, double k = 1)
    {
        Assets.Play("weaponGun");
        var stars = new List<Star>(); var rings = new List<Ring>();
        var e = Wfx(ee =>
        {
            ee.T++; ee.Frame += .5;
            if (ee.T == 7) { for (int i = 0; i < 8; i++) stars.Add(new Star { Ang = 20 * i, Siner = 45 * i, Sp = 16 }); rings.Add(new Ring()); }
            if (ee.T == 10 || ee.T == 13) { rings.Add(new Ring()); if (crit) Assets.Play("weaponSaber"); }
            foreach (var q in stars) { q.Siner += 15; q.Amt += q.Sp; q.Sp -= 2; q.Ang += 20; q.Size = Math.Max(0, 1 + q.Sp / 20); if (q.Sp < 0) q.A -= .07; if (q.Amt <= .5) q.Dead = true; }
            foreach (var r in rings) { if (r.Form == 0) r.Size += .5; if (r.Size > 3.5) r.Form = 1; if (r.Form == 1) { r.A -= .2; r.Size -= .3; if (r.A < .1) r.Dead = true; } }
            if (ee.T > 60) ee.Dead = true;
        }, ee =>
        {
            if (ee.T < 7) WFrame("gunStar", (int)Math.Floor(ee.Frame) % 2, ee.X, ee.Y, 2 * k, gold: crit);
            foreach (var r in rings) if (!r.Dead) WFrame("gunRing", -1, ee.X, ee.Y, r.Size * k, r.A, gold: crit);
            foreach (var q in stars) if (!q.Dead) { double a = q.Siner * Math.PI / 180; WFrame("gunStar", (int)Math.Floor(ee.Frame) % 2, ee.X + Math.Sin(a) * q.Amt * k, ee.Y + Math.Cos(a) * q.Amt * k, q.Size * k, q.A, -q.Ang, crit); }
        });
        e.X = x; e.Y = y;
    }
    // obj_frypan_impact (+ 8 obj_frypan_star)
    class PanStar { public double X, Y, Dir, Sp = 8, Ang = 12.25, Rot, A = 1; }
    void PanFx(double x, double y, bool crit)
    {
        Assets.Play("weaponPan"); if (crit) Assets.Play("weaponSaber");
        double ang = 6 * (Rnd(2) < 1 ? 1 : -1), size = 2, rot = 0; int form = 0;
        var stars = Enumerable.Range(0, 8).Select(i => new PanStar { X = x, Y = y, Dir = i * 45 }).ToList();
        var e = Wfx(ee =>
        {
            ee.T++; ee.Frame += 1; rot += ang;
            if (form == 0) size += .3;
            if (size > 2.8 && form == 0) { form = 1; Shake = new ShakeState { H = 2, V = 2, Speed = 2 }; }
            if (form == 1) { size -= .6; ee.Alpha -= .2; }
            foreach (var q in stars)
            {
                q.Sp = Math.Max(0, q.Sp - .34); q.X += Math.Cos(q.Dir * Math.PI / 180) * q.Sp; q.Y -= Math.Sin(q.Dir * Math.PI / 180) * q.Sp;
                if (q.Sp < 6) { q.A -= .05; if (q.Ang > 1) q.Ang -= .5; }
                q.Rot += q.Ang;
            }
            if (ee.T > 70) ee.Dead = true;
        }, ee =>
        {
            if (ee.Alpha > 0 && size > 0) WFrame("bookBurst", (int)ee.Frame % 2, ee.X, ee.Y, size, ee.Alpha, -rot, crit);
            foreach (var q in stars) if (q.A > .05) WFrame("star", -1, q.X, q.Y, 1, q.A, -q.Rot, crit);
        });
        e.X = x; e.Y = y;
    }
    // obj_notebook_attackanim: the book spins in, then bursts like the pan
    void BookFx(double x, double y, bool crit)
    {
        Assets.Play("weaponBook");
        double siner = 0, sx = 2, sc = 2; int frame = 0;
        var e = Wfx(ee =>
        {
            ee.T++; siner++;
            if (ee.T < 15) sx = Math.Cos(siner / 2) * 2;
            if (ee.T == 15) { Assets.Play("weaponPunchStrong"); if (crit) Assets.Play("weaponSaber"); sx = sc = .5; }
            if (ee.T > 15) { frame++; sc += .5; sx = sc; if (sc > 2) ee.Alpha -= .2; if (ee.Alpha < .1) ee.Dead = true; }
        }, ee =>
        {
            if (ee.T < 15) WFrame("notebook", -1, ee.X, ee.Y, 2, 1, 0, crit, sx);
            else WFrame("bookBurst", frame % 2, ee.X, ee.Y, sc, ee.Alpha, 0, crit);
        });
        e.X = x; e.Y = y;
    }

    // ------------------------------------------------------- the Dirty Blaster --
    // The web build's own UNDERTALE weapon. FIGHT is one knife bar; a hit calls in a Gaster Blaster
    // (obj_gasterblaster's flight, jaw and beam) that fires through the monster, two crossing ones
    // on a perfect hit. Its ACT menu gets Bones: a ring of bones flies in, then a row of them juts up
    // from under the monster. Both land their damage in ticks while they hit, and every tick leaves
    // KARMA on the monster, which eats its HP away afterwards (KrStep).
    bool HasBones(Monster m) => Weapon == "dirtyblaster";
    public string[] ActsOf(Monster m) { var l = m.Def.Acts(m); return HasBones(m) ? l.Take(5).Append("Bones").ToArray() : l; }
    class KrHits { public Monster M = null!; public double Total, Kr, Dealt; public int N, I, Shake; public bool Crit, Over; public DmgWriter? W; }
    KrHits NewKr(Monster m, double total, double kr, int n, bool crit) => new() { M = m, Total = total, Kr = kr, N = n, Crit = crit };
    void KrHit(KrHits c)
    {
        var m = c.M; if (c.Over || c.I >= c.N) return;
        double Share(double v) => Math.Floor(v * (c.I + 1) / c.N) - Math.Floor(v * c.I / c.N);
        double d = Share(c.Total), k = Share(c.Kr); c.I++;
        // the tick that finishes it: everything dealt so far goes through the usual hurt in one piece
        if (d > 0 && d >= m.Hp)
        {
            c.Over = true; if (c.W != null) c.W.Dead = true;
            StatDmg -= (int)c.Dealt; m.Hp += c.Dealt; m.TakeDamage = c.Dealt + d; m.Kr = 0; m.X = m.X0; m.Hurt = false;
            if (Bar != null) Bar.Crit = c.Crit; HurtMonster(); return;
        }
        // the others: the monster flinches and the number over it counts up as its HP bar drains
        if (d > 0)
        {
            if (c.W == null) { SpawnDmgWriter(m, 0); c.W = Dmgw[^1]; c.W.Vy = -4; }
            else if (c.W.Vy == 0) c.W.Vy = -2;
            c.W.Dmg = c.Dealt + d;
        }
        c.Dealt += d; m.Hp -= d; m.Kr += k; StatDmg += (int)d;
        m.Hurt = true; c.Shake = 6;
    }
    static void KrShake(KrHits c)
    {
        var m = c.M; if (c.Over || c.Shake <= 0) return;
        c.Shake--; m.X = m.X0 + (c.Shake != 0 ? (c.Shake % 2 != 0 ? 3 : -3) : 0); if (c.Shake == 0) m.Hurt = false;
    }
    void KrHitsDone(KrHits c)
    {
        if (c.Over) return; c.Over = true;
        var m = c.M; m.X = m.X0; m.Hurt = false;
        if (c.W != null) c.W.Life = 30; else { SpawnDmgWriter(m, 0); Dmgw[^1].Life = 30; }
        StatAttacks++; if (c.Crit) StatCrits++;
        m.Def.OnDamaged(this, m, c.Dealt);
        EndPlayerTurn();
    }
    static (double cx, double cy) MonCentre(Monster m) => (m.X0 + (m.Wd > 0 ? m.Wd : 100) / 2.0, m.Y + Math.Min(60, (m.H > 0 ? m.H : 100) / 2.0));

    class Gb { public double X, Y, IdealX, IdealY, Rot, Ang, Xs = 2, Ys = 2, Bt, Fade = 1, Bb, Bbs, Sp; public int Con = 1, A4, Pause = 10, Terminal = 24, Idx, BTimer; public bool Dead; }
    void DirtyBlaster(Monster m, double dmg, bool crit)
    {
        var c = NewKr(m, dmg, Math.Ceiling(dmg / 2), 14, crit); var (cx, cy) = MonCentre(m);
        // from up and to one side (both sides on a perfect hit), flying in from off the screen
        var sides = crit ? new[] { -1, 1 } : new[] { Rnd(2) < 1 ? -1 : 1 };
        var gbs = sides.Select(s =>
        {
            double ix = R(Math.Clamp(cx + s * 230, 50, 590)), iy = R(Math.Clamp(cy - 60, 70, 200));
            return new Gb { X = R(cx + s * 480), Y = iy - 220, IdealX = ix, IdealY = iy, Rot = R(Math.Atan2(-(cy - iy), cx - ix) * 180 / Math.PI) + 90 };
        }).ToList();
        Assets.Play("gbCharge");
        Wfx(e =>
        {
            KrShake(c);
            foreach (var b in gbs) GbMove(b, b == gbs[0]);
            // a tick every other step while the first one's beam is on
            var b0 = gbs[0]; if (b0.Con == 7 && b0.BTimer >= 2 && b0.BTimer % 2 == 0) KrHit(c);
            if (gbs.All(b => b.Dead)) { KrHitsDone(c); e.Dead = true; }
        }, e =>
        {
            Raylib.BeginScissorMode(0, 0, 640, (int)Math.Max(1, ET));
            foreach (var b in gbs) if (!b.Dead) GbPaint(b);
            Raylib.EndScissorMode();
        });
    }
    // obj_gasterblaster's step (the Sans fight's, less its collision)
    void GbMove(Gb b, bool loud)
    {
        if (b.Dead) return;
        if (b.Con == 7 && b.Sp != 0) { double d = b.Rot + 90; b.X += DCos(d) * b.Sp; b.Y -= DSin(d) * b.Sp; }
        if (b.A4 > 0 && --b.A4 == 0) b.Con++;
        if (b.Con == 1)
        {
            b.X += Math.Floor((b.IdealX - b.X) / 3); b.Y += Math.Floor((b.IdealY - b.Y) / 3);
            if (b.X < b.IdealX) b.X++; if (b.Y < b.IdealY) b.Y++; if (b.X > b.IdealX) b.X--; if (b.Y > b.IdealY) b.Y--;
            if (Math.Abs(b.X - b.IdealX) < 3) b.X = b.IdealX; if (Math.Abs(b.Y - b.IdealY) < 3) b.Y = b.IdealY;
            b.Ang += Math.Floor((b.Rot - b.Ang) / 3);
            if (b.Ang < b.Rot) b.Ang++; if (b.Ang > b.Rot) b.Ang--;
            if (Math.Abs(b.Ang - b.Rot) < 3) b.Ang = b.Rot;
            if (b.X == b.IdealX && b.Y == b.IdealY && b.Ang == b.Rot) { b.Con = 4; b.A4 = b.Pause; }
        }
        if (b.Con == 5) { b.Con = 6; b.A4 = 4; }
        if (b.Con == 6) b.Idx++;
        if (b.Con != 7) return;
        if (b.Idx == 4) b.Idx = 5; else if (b.Idx == 5) b.Idx = 4;
        if (b.BTimer == 0 && loud) { Assets.Play("gbFire"); Assets.Play("sansGiga"); Shake = new ShakeState { H = 5, V = 5, Speed = 2 }; }
        b.BTimer++;
        if (b.BTimer < 5) { b.Sp++; b.Bt += Math.Floor(35 * b.Xs / 4); } else b.Sp += 4;
        if (b.BTimer > 5 + b.Terminal) { b.Bt *= .8; b.Fade -= .1; if (b.Bt <= 2) { b.Dead = true; return; } }
        double sw = 43 * b.Xs, sh = 57 * b.Ys;
        if (b.X < -sw || b.X > 640 + sw || b.Y > 480 + sh || b.Y < -sh) b.Sp = 0;
        b.Bbs++; b.Bb = Math.Sin(b.Bbs / 1.5) * b.Bt / 4;
    }
    // spr_gasterblaster (origin 21,28 on its 43px cell) and the beam's three white lines,
    // see-through so the monster shows under it while it takes the hits
    void GbPaint(Gb b)
    {
        var fr = Assets.Frames("snsBlaster");
        if (fr != null) { var im = fr[b.Idx % fr.Length]; double ox = 21 - (43 - im.Width) / 2.0; Gfx.DrawRot(im, b.X, b.Y, ox, 28, b.Ang, b.Xs, b.Ys); }
        if (b.Con != 7 || b.BTimer < 1) return;
        double a = b.Ang - 90;
        (double, double) L(double n) => (DCos(a) * n * b.Xs / 2, -DSin(a) * n * b.Xs / 2);
        double rr = Rnd(2) - Rnd(2), rr2 = Rnd(2) - Rnd(2);
        var (xx, yy) = L(70); var (xa, ya) = L(50); var (xb, yb) = L(60); double fx = DCos(a) * 1000, fy = -DSin(a) * 1000;
        double fade = b.BTimer > 5 + b.Terminal ? Math.Max(0, b.Fade) : 1;
        void Ln(double x2, double y2, double w, double al) { if (w > 0) Gfx.Line(b.X + xx + rr, b.Y + yy + rr2, x2 + rr, y2 + rr2, w, Gfx.A(Color.White, al * fade)); }
        Ln(b.X + fx, b.Y + fy, b.Bt + b.Bb, .45);
        Ln(b.X + xa, b.Y + ya, b.Bt / 2 + b.Bb, .55);
        Ln(b.X + xb, b.Y + yb, b.Bt / 1.25 + b.Bb, .45);
        Ln(b.X + fx, b.Y + fy, (b.Bt + b.Bb) / 3, .6);
    }

    // Bones (the Dirty Blaster's ACT): eight bones (spr_s_bonewall) pop up in a ring and fly in one
    // at a time, then obj_bonestab's red warning and a row of bones (spr_s_bonestab_v_wide) jutting up
    // from under the monster. Twelve ticks; the KARMA is as much as the damage.
    class Bone { public double X, Y, V = 2; public int Born, Go; public bool Dead; }
    void BonesAct(Monster m)
    {
        MyFight = 2; H.Visible = false; Wr = null;
        double dmg = R((At + WStrength - m.Df) * 1.2 + Rnd(2));
        dmg = Math.Max(0, m.Def.OnHitCalc(this, m, dmg));
        var c = NewKr(m, dmg, dmg, 12, false); var (cx, cy) = MonCentre(m);
        double mh = m.H > 0 ? m.H : 100;
        double yg = Math.Min(244, m.Y + mh + 4), hh = R(Math.Clamp(mh * .8, 40, 100));
        // (flatter over a monster that stands high, so the top ones stay on the screen)
        double ry = Math.Clamp(cy - 30, 40, 150);
        var order = new[] { 0, 7, 1, 6, 2, 5, 3, 4 };
        var bones = order.Select((j, n) => { double a = -20 + j * 220 / 7.0; return new Bone { X = cx + DCos(a) * 170, Y = cy - DSin(a) * ry, Born = j * 2, Go = 18 + n * 3 }; }).ToList();
        var pops = new List<double[]>(); double rise = 0, jx = 0, jy = 0;
        Assets.Play("sansB");
        Wfx(e =>
        {
            int t = e.T++; KrShake(c);
            foreach (var b in bones)
            {
                if (b.Dead || t < b.Go) continue;
                double dx = cx - b.X, dy = cy - b.Y, dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist <= b.V + 12) { b.Dead = true; pops.Add(new[] { b.X + dx * .6, b.Y + dy * .6, 0 }); Assets.Play("damage"); KrHit(c); continue; }
                b.X += dx / dist * b.V; b.Y += dy / dist * b.V; b.V += 2.5;
            }
            foreach (var p in pops) p[2]++;
            if (t == 48) Assets.Play("sansB");
            if (t == 56) Assets.Play("undyneSpearRise");
            if (t >= 56 && t < 59) rise = R(hh * (t - 55) / 3);
            if (t >= 58 && t <= 64 && t % 2 == 0) { KrHit(c); Shake = new ShakeState { H = 2, V = 2, Speed = 1 }; }
            if (t >= 59 && t <= 68) { jx = R(Rnd(4) - 2); jy = R(Rnd(4) - 2); }
            if (t > 68) { jx = jy = 0; rise = Math.Max(0, rise - Math.Ceiling(hh / 4)); }
            if (t >= 78) { KrHitsDone(c); e.Dead = true; }
        }, e =>
        {
            int t = e.T;
            if (Assets.Tex("snsBoneWall") is { } im)
                foreach (var b in bones)
                {
                    if (b.Dead || t < b.Born) continue;
                    double k = Math.Min(1, (t - b.Born) / 4.0);
                    double ang = (Math.Atan2(cy - b.Y, cx - b.X) - Math.PI / 2) * 180 / Math.PI;
                    Gfx.DrawFrame(im, b.X, b.Y, 5, 25, k, k, ang, k);
                }
            foreach (var p in pops)
            {
                if (p[2] > 6) continue;
                double r = 4 + p[2] * 3;
                Raylib.DrawRing(new System.Numerics.Vector2((float)p[0], (float)p[1]), (float)r - 1, (float)r + 1, 0, 360, 24, Gfx.A(Color.White, 1 - p[2] / 7));
            }
            if (t >= 48 && t < 56 && t % 4 < 2) Raylib.DrawRectangleLinesEx(new Rectangle((float)(cx - 101), (float)(yg - hh - 1), 203, (float)hh + 2), 2, Gfx.Hex("#ff0000"));
            if (Assets.Tex("snsStabV") is { } st && rise > 0)
            {
                Raylib.BeginScissorMode((int)(cx - 110), (int)(yg - hh - 10), 220, (int)(hh + 10));
                Gfx.Draw(st, Math.Round(cx - 101 + jx), yg - rise + jy);
                Raylib.EndScissorMode();
            }
        });
    }
    // KARMA on a monster eats its HP away a point at a time, faster the more there is (the player's
    // KR schedule in obj_sansb_body), and like the player's it never takes the last HP
    void KrStep()
    {
        foreach (var m in Mons)
        {
            if (m.Kr == 0) continue;
            if (!m.Alive || m.Hp <= 1) { m.Kr = 0; continue; }
            if (m.Kr > m.Hp - 1) m.Kr = m.Hp - 1;
            int t = ++m.KrT; double k = m.Kr;
            if ((t >= 1 && k >= 40) || (t >= 2 && k >= 30) || (t >= 5 && k >= 20) || (t >= 15 && k >= 10) || t >= 30) { m.KrT = 0; m.Hp--; m.Kr--; }
        }
    }
    // KARMA on a monster: its HP bar stays up while the KARMA (magenta, spr_krmeter beside it) eats it
    void DrawKrBar(Monster m)
    {
        var p = m.Def.DmgPos(m);
        double x = p?.x ?? m.X0 + m.Wd / 2.0 - 48, y = (p?.y ?? m.Y - 24) + 8, sf = (m.Wd > 0 ? m.Wd : 100) / m.MaxHp, W = R(m.MaxHp * sf);
        double hp = Math.Max(0, m.Hp), kr = Math.Min(m.Kr, hp);
        Gfx.Rect(x - 1, y - 1, W + 3, 15, Color.Black);
        Gfx.Rect(x, y, W + 1, 13, Gfx.Hex("#404040"));
        Gfx.Rect(x, y, R(hp * sf) + 1, 13, Gfx.Hex("#00ff00"));
        Gfx.Rect(x + R((hp - kr) * sf), y, R(kr * sf) + 1, 13, Gfx.Hex("#ff00ff"));
        Gfx.Draw(Assets.Tex("snsKr"), x + W + 8, y + 2);
    }
}
