using Raylib_cs;
using UTBattle.Engine;

namespace UTBattle.Battle.Monsters;

static class Common
{
    // SCR_BORDER(side, a): a random point on that edge of the box
    public static (double x, double y) BorderPoint(Fight f, int side, double a)
    {
        var (L, R, T, B) = (f.Ideal[0], f.Ideal[1], f.Ideal[2], f.Ideal[3]);
        return side switch
        {
            0 => (Math.Round(f.Rnd(R - L)) + L, T),
            1 => (Math.Round(f.Rnd(R - L)) + L, B - a),
            2 => (L, Math.Round(f.Rnd(B - T)) + T),
            _ => (R - a, Math.Round(f.Rnd(B - T)) + T),
        };
    }
    public static void GenFlash(Fight f, double x, double y) => f.FxList.Add(new Fight.Fx { Kind = "gen", X = x, Y = y });
    public static void Aim(Fight f, Bullet b, double sp)
    {
        double dx = f.H.X + 2 - b.X, dy = f.H.Y + 2 - b.Y, l = Math.Sqrt(dx * dx + dy * dy);
        if (l == 0) l = 1;
        b.VX = dx / l * sp; b.VY = dy / l * sp;
    }
}

// obj_froggit
public class Froggit : MonsterDef
{
    public override string Name => "Froggit";
    public override double Hp => 30;
    public override double Atk => 4;
    public override double Def => 4;
    public override double Xp => 3;
    public override double Gold => 2;
    public override double MercyMod => 3;
    public override string Encounter => "* Froggit hopped close!";
    public override (int w, int h) Size => (112, 112);
    public override (double x, double y)? Pos(int i, int n, double cx) => n == 1 ? (214, 134) : (Math.Round(cx - 56), 134);
    public override string[] Acts(Monster m) => new[] { "Check", "Compliment", "Threaten" };
    public override string? Check(Monster m) => "* FROGGIT - ATK 4 DEF 5&* Life is difficult for&  this enemy./^";
    public override (string, Action?) Act(Fight f, Monster m, int i, string name)
    {
        m.MercyMod = 30;
        if (name == "Compliment") { m.V["blush"] = true; return ("* Froggit didn't understand&  what you said^1, but was&  flattered anyway./^", null); }
        m.V["shiver"] = true; return ("* Froggit didn't understand&  what you said^1, but was&  scared anyway./^", null);
    }
    public override string? Speech(Fight f, Monster m)
    {
        int c = (int)Math.Round(f.Rnd(100)); m.V["cmd"] = c;
        var t = c < 30 ? "Ribbit,&ribbit." : c < 50 ? "Croak,&croak." : c < 80 ? "Hop,&hop." : "Meow.";
        if (m.Get("blush", false)) { t = "(Blushes&deeply.)&Ribbit.."; f.TalkLenMin = 108; }
        if (m.Get("shiver", false)) t = "Shiver,&shiver.";
        m.V["blush"] = false; m.V["shiver"] = false;
        return t;
    }
    public override int? Border(Fight f, Monster m) => 3;
    public override string? Attack(Fight f, Monster m)
    {
        int n = f.Alive().Count;
        f.Turntimer(100);
        double rate = 20; if (n == 3) rate *= 1.5; if (n == 2) rate *= 1.25;
        int c = m.Get("cmd", 0);
        if (c <= 40 && n == 1)
        {
            // blt_leapfrog: sits in the corner, then leaps; when it's gone the turn ends
            var b = f.Spawn(new Bullet { X = f.Ideal[1] - 40, Y = f.Ideal[3] - 40, Box = new double[] { 0, 1, 40, 38 }, Img = "froggitLeap_0", Dmg = m.Atk, Wait = 30 + (int)f.Rnd(30) });
            b.Step = bb =>
            {
                if (bb.Wait > 0 && --bb.Wait == 0)
                {
                    double d = 145 - f.Rnd(20), sp = 7 + f.Rnd(3);
                    bb.VX = Fight.DCos(d) * sp; bb.VY = -Fight.DSin(d) * sp;
                    bb.AX = Fight.DCos(280) * .4; bb.AY = -Fight.DSin(280) * .4;
                    bb.Img = "froggitLeap_1"; bb.Dmg *= 1.8;
                }
            };
            b.OnDie = _ => f.Turntimer(0);
        }
        else m.V["gen"] = new double[] { 0, rate, c <= 40 ? 0 : 1 };
        string fl = "* Froggit doesn't seem to&  know why it's here.";
        if (c >= 30) fl = "* Froggit hops to and fro.";
        if (c >= 60) fl = "* The battlefield is filled&  with the smell of mustard&  seed.";
        if (c >= 80) fl = "* You are intimidated by&  Froggit's raw strength^4.&* Only kidding.";
        if (m.MercyMod > 5) fl = "* Froggit seems reluctant&  to fight you.";
        if (m.Hp < 5) fl = "* Froggit is trying to&  run away.";
        return fl;
    }
    public override void AttackStep(Fight f, Monster m)
    {
        if (m.Get<double[]?>("gen", null) is not { } gn) return;
        if (gn[0]-- <= 0)
        {
            gn[0] = gn[1];
            var p = Common.BorderPoint(f, 0, 6);
            Common.GenFlash(f, p.x, p.y);
            if (gn[2] == 0)
            {
                // blt_direct: straight at the SOUL
                var b = f.Spawn(new Bullet { X = p.x, Y = p.y, Box = new double[] { 5, 5, 2, 2 }, Img = "froggitFly", Dmg = m.Atk });
                Common.Aim(f, b, 2.5 + f.Rnd(.2) * 2 - .2);
            }
            else
            {
                // blt_flybullet: homes at 2.5, stops at 30 steps, re-aims at 3 on 45, repeat
                var b = f.Spawn(new Bullet { X = p.x, Y = p.y, Box = new double[] { 6, 6, 1, 1 }, Img = "froggitFly", Dmg = m.Atk, Anim = .5, A0 = 30, A1 = 45 });
                b.Step = bb =>
                {
                    bb.Frame += .5;
                    if (--bb.A0 == 0) { bb.VX = 0; bb.VY = 0; }
                    if (--bb.A1 == 0) { bb.A0 = 30; bb.A1 = 45; Common.Aim(f, bb, 3); }
                };
                Common.Aim(f, b, 2.5);
            }
        }
    }
    public override void EndAttack(Fight f, Monster m) => m.V["gen"] = null;
    public override string Flavor(Fight f, Monster m) => "* Froggit hops to and fro.";

    // the body with its head and legs swapped in (obj_froghead .02, obj_froglegs .04)
    RenderTexture2D? canvas;
    public override void PreRender(Fight f, Monster m)
    {
        if (m.Hurt || !m.Alive) return;
        canvas ??= Raylib.LoadRenderTexture(112, 112);
        var c = canvas.Value;
        int hf = (int)Math.Floor(m.T * .02) % 2, lf = (int)Math.Floor(m.T * .04) % 2;
        double ph = m.T / 30.0 * Math.PI * .72;
        int hx = (int)Math.Round(Math.Sin(ph) * .75), hy = (int)Math.Round(Math.Cos(ph) * .75);
        Raylib.BeginTextureMode(c);
        Raylib.ClearBackground(Color.Blank);
        Gfx.Draw(Assets.Frame("froggit", 0), 0, 0, 112, 112);
        Raylib.BeginScissorMode(1, 10, 105, 64); Raylib.ClearBackground(Color.Blank); Raylib.EndScissorMode();
        Raylib.BeginScissorMode(9, 61, 94, 50); Raylib.ClearBackground(Color.Blank); Raylib.EndScissorMode();
        Gfx.Draw(Assets.Frame("froggitHead", hf), 2 + hx, 12 + hy, 102, 60);
        Gfx.Draw(Assets.Frame("froggitLegs", lf), 10, 62, 92, 48);
        Raylib.EndTextureMode();
    }
    public override void Draw(Fight f, Monster m)
    {
        if (m.Hurt || !m.Alive || canvas == null) { Gfx.Draw(Assets.Frame("froggit", m.Hurt || !m.Alive ? 1 : 0), Math.Round(m.X), Math.Round(m.Y), 112, 112, alpha: m.Alpha); return; }
        var t = canvas.Value.Texture;
        // render textures are stored upside down
        Raylib.DrawTexturePro(t, new Rectangle(0, 0, t.Width, -t.Height), new Rectangle((float)Math.Round(m.X), (float)Math.Round(m.Y), 112, 112), System.Numerics.Vector2.Zero, 0, Gfx.A(Color.White, m.Alpha));
    }
    public override string? VaporSprite(Monster m) => "froggit_1";
}

// obj_dummymonster
public class Dummy : MonsterDef
{
    public override string Name => "Dummy";
    public override double Hp => 15;
    public override double Atk => 0;
    public override double Def => -5;
    public override double MercyMod => -99;
    public override string Encounter => "* You encountered the Dummy.";
    public override (int w, int h) Size => (98, 106);
    public override (double x, double y)? Pos(int i, int n, double cx) => n == 1 ? (216, 136) : (Math.Round(cx - 49), 136);
    public override string[] Acts(Monster m) => new[] { "Check", "Talk" };
    public override string? Check(Monster m) => "* DUMMY - ATK 0 DEF 0^1 &* A cotton heart and a button&  eye^1.&* You are the apple of my eye./";
    public override (string, Action?) Act(Fight f, Monster m, int i, string name) =>
        ("* You talk to the DUMMY^2.&* .../* It doesn't seem much for&  conversation./* TORIEL seems happy with you.", () => { f.Remove(m); f.Win(); });
    public override string? Speech(Fight f, Monster m) => " ^1.^1.^1.^1.^1.";
    public override int? Border(Fight f, Monster m) => 0;
    public override string? Attack(Fight f, Monster m)
    {
        f.Turntimer(1);
        if (f.Turn > 6 && !m.Get("leaving", false)) m.V["leaving"] = true;
        if (m.Get("leaving", false)) return "* Dummy tires of your&  aimless shenanigans.";
        return f.Rnd(100) >= 50 ? "* Dummy stands around&  absentmindedly." : "* Dummy looks like it's&  going to fall over.";
    }
    public override void Step(Fight f, Monster m)
    {
        if (m.Get("leaving", false) && f.MnFight == 0) { m.X += 4; if (m.X > 640) { f.Remove(m); f.Win(); } }
    }
    public override void Draw(Fight f, Monster m) =>
        Gfx.Draw(Assets.Tex(m.Hurt || !m.Alive ? "dummyDeath" : "dummy"), m.X, m.Y, alpha: m.Alpha);
    public override string? VaporSprite(Monster m) => "dummyDeath";
}
