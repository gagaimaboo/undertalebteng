using Raylib_cs;
using UTBattle.Engine;

namespace UTBattle.Battle.Monsters;

// obj_torielboss + obj_1sidegen 7/8/9/10, blt_handbullet1/2, obj_torheart
public class Toriel : MonsterDef
{
    public override string Name => "Toriel";
    public override double Hp => 440;
    public override double Atk => 6;
    public override double Def => 1;
    public override double Xp => 150;
    public override double MercyMod => -20000;
    public override int Wd => 144;
    public override string? Voice => "torielText";
    public override string? MusicKey => "toriel";
    public override string Encounter => "* Toriel blocks the way!";
    public override (int w, int h) Size => (144, 202);
    public override (double x, double y)? Pos(int i, int n, double cx) => n == 1 ? (250, 42) : (Math.Round(cx - 72), 42);
    public override (double x, double y)? BubblePos(Monster m) => (m.X + 144 + 21, 42 + 24);

    // m.v
    int conv, tt, face, cmd;
    double hplast;
    string spr = "normal", hurtSpr = "hurt";
    bool dying, torgen, hands;
    Gen? gen;
    class Gen { public int Type, Rate, T; }

    static readonly string?[] TalkSmall = { null, " .....", " .....& .....", " .....& .....& .....", " ...?", " What are& you& doing?", " Attack& or run& away!", " What are& you& proving& this way?", " Fight me& or& leave!", " Stop it.", " Stop& looking& at me& that way.", " Go away!", " ...", " ...& ..." };
    static readonly Dictionary<int, string> TalkBig = new()
    {
        [14] = "I know you want&to go home^1, but...", [15] = "But please... go&upstairs now.", [16] = "I promise I will&take good care&of you here.",
        [17] = "I know we do not&have much^1, but...", [18] = "We can have a&good life here.", [19] = "Why are you&making this so&difficult?",
        [20] = "Please^1, go upstairs.", [21] = ".....", [22] = "Ha ha...", [23] = "Pathetic^1, is it not^2?&I cannot save even&a single child.", [24] = "...",
    };
    static readonly Dictionary<int, string> TalkSpr = new() { [12] = "side", [13] = "sidesad", [14] = "sad", [19] = "sad", [20] = "sidesad", [21] = "sidesad2", [22] = "sidesadhappy", [24] = "sidesad" };
    static readonly string[] Goodbye = { "No^1, I understand./", "You would just be&unhappy trapped&down here./", "The RUINS are very&small once you&get used to them./", "It would not be&right for you to&grow up in a&place like this./", "My expectations...&My loneliness...&My fear.../", "For you^1, my child...&I will put them aside./" };
    static readonly string[] Dying = { "\\E0Urgh.../", "\\E0You are stronger&than I thought.../", "Listen to me^1,& small one.../", "If you go beyond&this door,/", "Keep walking as&far as you can./", "Eventually you will&reach an exit./", "\\E1..^1.&..../", "\\RASGORE\\X..^1.&Do not let \\RASGORE\\X &take your soul./", "His plan&cannot be allowed&to succeed./", "\\E2....../", "Be good^1,&won't you?/", "\\E3My child./" };
    static readonly string[] DyingBetrayed = { "\\E4You.../", "... at my most&vulnerable&moment.../", "To think I was&worried you&wouldn't fit&in out there.../", "\\E5Eheheheh!!!&You really are&no different than&them!/", "\\E3Ha... ha.../" };
    static readonly string[] Faces = { "kneel", "kneelanguish", "kneelanguish2", "kneelsmile", "murdered" };

    // sprite -> (image, dx, dy, scale): dx/dy undo the exporter's crop, in sprite pixels
    (Texture2D? im, double dx, double dy, double sc) Spr(string key, Monster m) => key switch
    {
        "normal" => (Assets.Tex("toriel"), 0, 1, 2),
        "hurt" => (Assets.Tex("torielHurt"), 0, 0, 2),
        "reallyhurt" => (Assets.Tex("torielReallyHurt"), 0, 0, 2),
        "side" => (Assets.Tex("nbTorSide"), 0, 0, 2),
        "sidesad" => (Assets.Tex("nbTorSideSad"), 0, 0, 2),
        "sad" => (Assets.Tex("nbTorSad"), 0, 0, 2),
        "sadhappy" => (Assets.Frame("nbTorSadHappy", Math.Floor(m.T * .2) % 2), 0, 0, 2),
        "sidesad2" => (Assets.Tex("nbTorSideSad2"), 0, 0, 2),
        "sidesadhappy" => (Assets.Tex("nbTorSideSadHappy"), 0, 0, 2),
        "neutral" => (Assets.Tex("nbTorNeutral"), 0, 0, 2),
        "murdered" => (Assets.Frame("nbTorMurdered", face == 5 ? 1 : 0), 0, 0, 2),
        "kneel" => (Assets.Tex("torielKneel"), 6, 24, 2),
        "kneelanguish" => (Assets.Tex("nbTorKneelAnguish"), 6, 25, 2),
        "kneelanguish2" => (Assets.Tex("nbTorKneelAnguish2"), 6, 25, 2),
        "kneelsmile" => (Assets.Tex("torielKneelSmile"), 6, 24, 2),
        "kneelsmile2" => (Assets.Tex("nbTorKneelSmile2"), 0, 56, 1),
        _ => (Assets.Tex("toriel"), 0, 1, 2),
    };

    public override void Setup(Fight f, Monster m) { hplast = m.Hp; }

    public override void Draw(Fight f, Monster m)
    {
        var key = spr;
        if (m.Hurt) key = hurtSpr;
        var (im, dx, dy, sc) = Spr(key, m);
        if (im is { } t) Gfx.Draw(t, Math.Round(m.X + dx * sc), Math.Round(m.Y + dy * sc), t.Width * sc, t.Height * sc, alpha: m.Alpha);
        // obj_torgen: she looks away while the low-HP fire falls
        if (torgen && f.MnFight == 2) Gfx.Draw(Assets.Tex("nbTorSide"), Math.Round(m.X), Math.Round(m.Y), 144, 204);
    }

    public override string[] Acts(Monster m) => new[] { "Check", "Talk" };
    public override string? Check(Monster m) => "* TORIEL - ATK 80 DEF 80&* Knows best for you./";
    public override (string, Action?) Act(Fight f, Monster m, int i, string name)
    {
        tt++;
        if (tt > 2) return ("* Ironically^1, talking does not&  seem to be the solution&  to this situation./", null);
        if (tt > 1) return ("* You tried to think&  of something to say&  again^1, but.../", null);
        return ("* You couldn't think of&  any conversation&  topics./", null);
    }

    // a hit that would kill her doesn't: she kneels and talks first (alarm[6], destroyed=1)
    public override bool OnLethal(Fight f, Monster m) { dying = true; spr = "kneel"; f.EndPlayerTurn(); return true; }
    public override double OnHitCalc(Fight f, Monster m, double dmg)
    {
        hurtSpr = dmg > 100 ? (conv > 13 ? "murdered" : "reallyhurt") : "hurt";
        if (conv < 4) conv = 0;
        return dmg;
    }
    public override void OnDamaged(Fight f, Monster m, double dmg)
    {
        if (m.Hp <= 150) m.Df = -140;
        if (conv < 13) spr = f.Hp > 2 ? "normal" : "side";
    }

    public override Say? SpeechEx(Fight f, Monster m)
    {
        f.TalkLenExact = Math.Max(f.TalkLenExact, 15);
        if (dying)
        {
            f.StopMusic();
            return new Say { Pages = conv > 13 ? DyingBetrayed : Dying, Wide = true, Hold = true, Pos = (m.X + 144 + 2, m.Y + 36), Tx = 36, Img = "nbBlconWdShrt",
                OnFace = n => { face = n; spr = Faces[Math.Min(4, n)]; } };
        }
        // every turn she wasn't hurt (and you didn't ACT or use an item), the talk goes on
        bool talk = hplast == m.Hp && f.Talked == -1;
        if (talk) conv++;
        hplast = m.Hp; cmd = (int)Math.Round(f.Rnd(100));
        if (!talk) return null;
        f.TalkLenExact = Math.Max(f.TalkLenExact, 85);
        int c = conv;
        if (TalkSpr.TryGetValue(c, out var s)) spr = s;
        if (c == 10) torgen = true;
        if (c <= 13) return new Say { Pages = new[] { TalkSmall[c] ?? " ..." } };
        if (c == 14) f.StopMusic();
        if (c > 15 && !TalkSpr.ContainsKey(c)) spr = "sadhappy";
        Say Big(string[] pages) => new() { Pages = pages, Wide = true, Hold = true, Pos = (m.X + 144 + 3, m.Y + 32), Tx = 36, Img = "nbBlconWdShrt" };
        if (c >= 25) { conv = 99; spr = "neutral"; return Big(Goodbye); }
        return Big(new[] { (TalkBig.TryGetValue(c, out var t) ? t : "...") + "/" });
    }

    public override int? Border(Fight f, Monster m) => 6;

    public override string? Attack(Fight f, Monster m)
    {
        if (dying)
        {
            // alarm[10]: she smiles one last time and turns to dust. The battle UI goes away,
            // the room slows to 20 steps a second and her SOUL (obj_torheart) appears.
            spr = "kneelsmile2";
            var im = Assets.Tex("nbTorKneelSmile2");
            double hx = f.H.X, hy = m.Y + 56 + (im?.Height ?? 100) / 2.0;
            f.KillMonster(m);
            f.Cinematic = true; f.SpeedMul = 20.0 / 30; f.Bullets.Clear(); f.Bubbles.Clear(); f.Wr = null; f.MyFight = 99; f.MnFight = 99;
            f.SetBorder(0); f.StopMusic();
            f.FxList.Add(TorHeart(f, hx, hy));
            return null;
        }
        if (conv == 99) { f.Remove(m); f.End("spare"); return null; }
        int c = cmd;
        f.SetBorder(c < 40 || f.Hp < 3 ? 7 : 6);
        f.Turntimer(160);
        gen = null; hands = false;
        if (f.Hp > 2 && conv < 13)
        {
            if (c <= 20) { f.Turntimer(140); gen = new Gen { Type = 7, Rate = 5, T = 1 }; }
            else if (c <= 40) { f.Turntimer(140); gen = new Gen { Type = 8, Rate = 2, T = 1 }; }
            else if (c <= 60) { f.Turntimer(140); gen = new Gen { Type = 10, Rate = 6, T = 1 }; }
            else if (c <= 80) { f.Turntimer(200); Hand(f, m, 1, true); Hand(f, m, 2, true); hands = true; }
            else { f.Turntimer(200); Hand(f, m, 1, false); hands = true; }
        }
        else
        {
            if (m.Hp > 150) m.Df = -15;
            f.Turntimer(140); gen = new Gen { Type = 9, Rate = 2, T = 1 };
            if (conv < 13) torgen = true;
        }
        if (conv > 13) { f.Turntimer(1); m.Df = -2000; gen = null; }
        string fl = "* Toriel prepares a magical&  attack.";
        if (c >= 30) fl = "* Toriel looks through you.";
        if (c >= 70) fl = "* Toriel is acting aloof.";
        if (c >= 90) fl = "* Toriel takes a deep breath.";
        if (conv > 13) fl = "* ...";
        return fl;
    }

    public override void AttackStep(Fight f, Monster m)
    {
        var g = gen;
        // with 2 HP or less her fire stops (turntimer = -100)
        if (f.Hp <= 2 && (hands || (g != null && g.Type != 9))) { f.EndTurn(); return; }
        if (g == null || --g.T > 0) return;
        g.T = g.Rate;
        if (g.Type == 7) Helix(f, m);
        else if (g.Type == 8) Mini(f, m);
        else if (g.Type == 9) Avoid(f, m);
        else if (g.Type == 10) { Helix(f, m); FloatFire(f, m, f.Ideal[0] + 2, f.Ideal[2] + 90); FloatFire(f, m, f.Ideal[1] - 18, f.Ideal[2] + 90); }
    }
    public override void EndAttack(Fight f, Monster m) { gen = null; torgen = false; }
    public override string Flavor(Fight f, Monster m) => "* Toriel prepares a magical&  attack.";
    public override (string key, double x, double y, double w, double h)? Vapor(Monster m)
    {
        var t = Assets.Tex("nbTorKneelSmile2");
        return t is { } tx ? ("nbTorKneelSmile2", m.X, m.Y + 56, tx.Width, tx.Height) : null;
    }

    // ------------------------------------------------------------------ fire --
    // fire damage drops when the player is nearly dead (obj_1sidegen / blt_handbullet)
    static double FireDmg(Fight f, double d) => f.Hp < 6 ? 1 : f.Hp < 8 ? 2 : d;
    // wall objects around the box (obj_l/r/u/dborder), from where the border is right now
    static (bool l, bool r, bool u, bool d) Walls(Fight f, Bullet b)
    {
        double x0 = b.X + b.Box[0], y0 = b.Y + b.Box[1], x1 = x0 + b.Box[2], y1 = y0 + b.Box[3];
        double L = f.EL, R = f.ER, T = f.ET, B = f.EB;
        return (x0 <= L + 5 && x1 >= L && y1 >= T && y0 <= B + 5, x1 >= R && x0 <= R + 5 && y1 >= T && y0 <= B + 5,
                y0 <= T + 5 && y1 >= T && x1 >= L && x0 <= R + 5, y1 >= B && y0 <= B + 5 && x1 >= L && x0 <= R + 5);
    }
    // blt_parent: a bullet dies on the wall it's moving into
    static void ParentWalls(Fight f, Bullet b) { var w = Walls(f, b); if ((w.u && b.VY < 0) || (w.d && b.VY > 0) || (w.l && b.VX < 0) || (w.r && b.VX > 0)) b.Alive = false; }
    static void Move(Bullet b) { b.XPrev = b.X; b.YPrev = b.Y; Fight.Gm(b); b.X += b.VX; b.Y += b.VY; }
    static Bullet Fire(Fight f, Bullet b) { b.Box = new double[] { 6, 8, 4, 4 }; b.Img = "torielFire"; b.Anim = .5; b.Manual = true; b.WallKill = false; return f.Spawn(b); }
    static void GenFx(Fight f, double x, double y) => f.FxList.Add(new Fight.Fx { Kind = "gen", X = x, Y = y });

    // 7: blt_firehelix1
    void Helix(Fight f, Monster m)
    {
        double L = f.Ideal[0], R = f.Ideal[1], T = f.Ideal[2];
        double y = T - 25; if (y < T + 20) y += 20;
        int r = (int)Math.Round(f.Rnd(1)); bool landed = false;
        var b = Fire(f, new Bullet { X = L + (R - L) / 2 - 3, Y = y, VY = .7, Grav = .12, GDir = 270, Dmg = FireDmg(f, m.Atk) });
        b.Step = bb =>
        {
            double s = Math.Sin(f.T / 10.0) * 4; bb.VX = r == 0 ? -s : s; Move(bb);
            var w = Walls(f, bb);
            if (w.d && !landed) { bb.Y = bb.YPrev; bb.VY = 0; bb.GDir = r == 1 ? 180 : 0; landed = true; }
            if ((w.u && bb.VY < 0) || (w.l && bb.VX < 0) || (w.r && bb.VX > 0)) bb.Alive = false;
        };
    }
    // 8: blt_minihelix
    void Mini(Fight f, Monster m)
    {
        double L = f.Ideal[0], R = f.Ideal[1], T = f.Ideal[2];
        int r = (int)Math.Round(f.Rnd(1)), hh = 0, dink = 0;
        var b = Fire(f, new Bullet { X = L + (R - L) / 2 - 3, Y = T + 5, VY = Math.Abs(Math.Sin(f.T / 20.0) * .95), Grav = .06, GDir = 270, Dmg = FireDmg(f, m.Atk) });
        b.Step = bb =>
        {
            double s = Math.Sin(hh / 5.0) * 8; bb.VX = r == 0 ? -s : s; hh++; Move(bb);
            var w = Walls(f, bb);
            if (w.d && bb.VY >= 0) { bb.Grav = 0; bb.VY = -.2; Assets.Play("encounterNoise"); dink++; bb.Y = bb.YPrev; if (dink == 3) bb.Alive = false; }
            else if ((w.u && bb.VY < 0) || (w.l && bb.VX < 0) || (w.r && bb.VX > 0)) bb.Alive = false;
        };
    }
    // 10: blt_floatfire at each side
    void FloatFire(Fight f, Monster m, double x, double y)
    {
        GenFx(f, x, y);
        var b = Fire(f, new Bullet { X = x, Y = y, VY = f.Rnd(8) - 4, Dmg = FireDmg(f, f.Hp < 7 ? 2 : 4) });
        b.Step = bb => { Move(bb); ParentWalls(f, bb); };
    }
    // 9: blt_avoidfire, drifts down and shies away from the SOUL
    void Avoid(Fight f, Monster m)
    {
        double L = f.Ideal[0], R = f.Ideal[1], T = f.Ideal[2];
        var b = Fire(f, new Bullet { X = Math.Round(f.Rnd(R - L)) + L - 40, Y = T - 20, Grav = .1 + f.Rnd(.2), GDir = 250 + f.Rnd(40), Hidden = true, Dmg = 1 });
        b.Anim = .2;
        b.Step = bb =>
        {
            Move(bb);
            var h = f.H;
            if (Math.Abs(h.Y - (bb.Y + 8)) < 100) bb.VX = 180 / (Math.Max(0, Math.Abs(h.X - (bb.X + 8)) - 2) + 10) - 1;
            if (h.X > bb.X) bb.VX = -bb.VX;
            if (bb.X < f.Ideal[0] || bb.X > f.Ideal[1] || bb.Y > f.Ideal[3]) { bb.Alive = false; return; }
            if (bb.Y > f.Ideal[2]) bb.Hidden = false;
            ParentWalls(f, bb);
        };
    }
    // blt_handbullet1/2 follow path_hand1/2: a sweep along the top/bottom edge, bowing 12px in, at a
    // path speed that grows .2/step to the halfway point and then drops .1/step, dropping a
    // chase-fire every 4 steps
    void Hand(Fight f, Monster m, int which, bool chase2)
    {
        double L = f.Ideal[0], R = f.Ideal[1], T = f.Ideal[2], B = f.Ideal[3], W = R - L + 90;
        double x0 = which == 1 ? L - 45 : R + 5, y0 = which == 1 ? T + 5 : B - 15; int dir = which == 1 ? 1 : -1;
        (double x, double y) At(double p) => (x0 + dir * W * p, y0 + dir * 12 * Math.Sin(Math.PI * p));
        double len = 0; var pr = At(0);
        for (int i = 1; i <= 40; i++) { var q = At(i / 40.0); len += Math.Sqrt((q.x - pr.x) * (q.x - pr.x) + (q.y - pr.y) * (q.y - pr.y)); pr = q; }
        double pos = 0, sp = .2; int a0 = 8, a1 = -1; bool inactive = false;
        var b = f.Spawn(new Bullet { X = x0, Y = y0, Manual = true, Box = new double[] { 22, 8, 17, 27 }, Img = $"torielHand_{(y0 < T + 60 ? 1 : 0)}", Clip = true, WallKill = false, Dmg = m.Atk, Tag = which });
        // blt_parent: destroyed on contact, ending the turn
        b.OnHit = bb => { if (!f.DamagePlayer(bb.Dmg)) return false; f.EndTurn(); return true; };
        b.Step = bb =>
        {
            if (a0 > 0 && --a0 == 0)
            {
                if (!inactive)
                {
                    double cx = bb.X + 22, cy = bb.Y + 5;
                    if (!chase2 && cx >= f.Ideal[0]) ChaseFire1(f, cx, cy); else if (chase2) ChaseFire2(f, cx, cy);
                    Assets.Play("encounterNoise"); a0 = 4;
                }
                else a1 = which == 1 ? 100 : 70;
            }
            if (a1 > 0 && --a1 == 0) { bb.Alive = false; f.EndTurn(); return; }
            bb.Dmg = FireDmg(f, m.Atk);
            if (!inactive)
            {
                if (pos < .5) sp += .2; else sp -= .1;
                pos = Math.Min(1, pos + sp / len);
                var p = At(pos); bb.X = p.x; bb.Y = p.y;
                if (pos >= 1) { inactive = true; bb.A0 = 1; } // A0: the path is done
            }
        };
    }
    // bounced[0]: it has come off the top wall
    static void Bounce(Fight f, Bullet b, double fric, bool[] bounced, Func<Bullet, bool>? onDown = null)
    {
        var w = Walls(f, b);
        if (w.u && b.VY < 0) { b.VY = -b.VY; b.Fric = fric; bounced[0] = true; }
        if (w.d && b.VY > 0) { if (onDown != null && onDown(b)) return; b.VY = -b.VY; b.Fric = fric; }
        if (w.l && b.VX < 0) { b.VX = -b.VX; b.Fric = fric; }
        if (w.r && b.VX > 0) { b.VX = -b.VX; b.Fric = fric; }
    }
    static void Toward(Bullet b, Fight.Heart h, double sp)
    {
        double dx = h.X + 2 - b.X, dy = h.Y + 2 - b.Y, l = Math.Sqrt(dx * dx + dy * dy); if (l == 0) l = 1;
        b.VX = dx / l * sp; b.VY = dy / l * sp;
    }
    static void ChaseFire1(Fight f, double x, double y)
    {
        GenFx(f, x, y);
        int a1 = 45, a2 = -1; var bounced = new bool[1];
        var b = Fire(f, new Bullet { X = x, Y = y, Clip = true, Dmg = f.Hp < 7 ? 2 : 4 });
        b.Step = bb =>
        {
            var h = f.H;
            if (a1 > 0 && --a1 == 0) { Toward(bb, h, 2); bb.Fric = -.05; a2 = 4; bb.Dmg = f.Hp < 7 ? 2 : 5; }
            if (a2 > 0 && --a2 == 0) { bb.VX += h.X > bb.X ? .1 : -.1; bb.VY += h.Y > bb.Y ? .1 : -.1; a2 = 2; }
            Move(bb);
            if (bb.Y > f.Ideal[3] + 4) { bb.Alive = false; return; }
            Bounce(f, bb, .04, bounced, c => { if (bounced[0]) { c.Alive = false; return true; } return false; });
        };
    }
    static void ChaseFire2(Fight f, double x, double y)
    {
        GenFx(f, x, y);
        bool goof = false; var bounced = new bool[1];
        var b = Fire(f, new Bullet { X = x, Y = y, Clip = true, Dmg = f.Hp < 7 ? 2 : 5 });
        b.Step = bb =>
        {
            var h1 = f.Bullets.FirstOrDefault(o => o.Tag is int w && w == 1 && o.Alive);
            if (h1 != null && h1.A0 == 1 && !goof) { Toward(bb, f.H, .6); bb.Fric = -.1; goof = true; }
            Move(bb);
            if (bb.Y > f.Ideal[3] + 4) { bb.Alive = false; return; }
            Bounce(f, bb, .08, bounced);
        };
    }

    // obj_torheart: fades in shaking; after 120 steps it cracks (snd_break1), 40 steps later it
    // shatters into six obj_theartshard (snd_break2); then the screen fades out
    static Fight.Fx TorHeart(Fight f, double x, double y)
    {
        double xs = x, ys = y, a = 0, shake = 3; int a0 = 120, a1 = -1, a2 = -1, a3 = -1; bool brk = false, vis = true, fading = false;
        var shards = new List<double[]>();
        var e = new Fight.Fx { Kind = "torheart", X = x, Y = y };
        e.StepFn = ee =>
        {
            if (a0 > 0 && --a0 == 0) { f.SpeedMul = 1; Assets.Play("break1"); brk = true; xs -= 2; shake = 0; a1 = 40; }
            if (a1 > 0 && --a1 == 0)
            {
                Assets.Play("break2"); vis = false;
                foreach (var (dx, dy) in new[] { (-2, 0), (0, 3), (2, 6), (8, 0), (10, 3), (12, 6) }) { double d = f.Rnd(360); shards.Add(new[] { ee.X + dx, ee.Y + dy, Fight.DCos(d) * 7, -Fight.DSin(d) * 7, 0 }); }
                a2 = 180;
            }
            if (a2 > 0 && --a2 == 0) { fading = true; a3 = 20; }
            if (fading) f.Fade += 1 / 20.0;
            if (a3 > 0 && --a3 == 0) { f.End("win"); return; }
            ee.X = xs + f.Rnd(shake) - f.Rnd(shake); ee.Y = ys + f.Rnd(shake) - f.Rnd(shake); a += .01;
            foreach (var s in shards) { s[3] += .2; s[0] += s[2]; s[1] += s[3]; s[4] += .25; }
        };
        e.DrawFn = ee =>
        {
            if (vis) Gfx.Draw(Assets.Tex(brk ? "monsterSoulBreak" : "monsterSoul"), Math.Round(ee.X), Math.Round(ee.Y), alpha: Math.Min(1, a));
            var fr = Assets.Frames("monsterSoulShards"); int n = fr?.Length ?? 0;
            if (n > 0) foreach (var s in shards) Gfx.Draw(fr![n - 1 - ((int)Math.Floor(s[4]) % n)], Math.Round(s[0]), Math.Round(s[1]));
        };
        return e;
    }
}
