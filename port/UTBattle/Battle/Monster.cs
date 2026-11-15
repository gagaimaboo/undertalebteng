using UTBattle.Engine;

namespace UTBattle.Battle;

// One enemy in a fight (the JS engine's `m`).
public class Monster
{
    public string Id = "";
    public MonsterDef Def = null!;
    public int I;
    public string Name = "";
    public double Hp, MaxHp, Atk, Df, Xp, Gold, MercyMod;
    public bool Alive = true, Spared, Killed, Gone, Hurt, Talked;
    public double Alpha = 1, X, Y, X0;
    public int W, H, Wd;
    public int HurtAnim, Shudder, ShakeT, T;
    public double TakeDamage;
    public int WhatIHeard = -1;
    // per-monster script state (the JS m.v)
    public readonly Dictionary<string, object?> V = new();
    public T Get<T>(string k, T def) => V.TryGetValue(k, out var o) && o is T t ? t : def;
}

// A bullet (blt_parent): box is [x, y, w, h] relative to (x, y).
public class Bullet
{
    public double X, Y, VX, VY, AX, AY;
    public double XPrev, YPrev;
    // GameMaker motion (Fight.Gm): friction, gravity and its direction
    public double Fric, Grav, GDir = 270;
    public double[] Box = { 0, 0, 8, 8 };
    public double Dmg = 1, Frame, Anim, Alpha = 1;
    public bool WallKill = true, Kill = true, Alive = true, Manual, Hidden, DiedOnce;
    // clip: drawn only inside the box
    public bool Clip;
    public int T, Wait, A0, A1;
    public string? Img;                // a sprite key (Assets.Frame picks the frame)
    public Action<Bullet>? Step;
    public Action<Bullet>? OnDie;
    // a hit on the SOUL: return false when it didn't count (the default hurts by Dmg)
    public Func<Bullet, bool>? OnHit;
    // precise check after the boxes overlap (the default is the SOUL's pixel mask)
    public Func<Fight.Heart, bool>? Mask;
    public Action<Bullet>? Draw;       // replaces the sprite draw
    public Monster? Owner;
    public object? Tag;                // what kind of bullet this is, for the script that made it
}

// What a monster says on its turn (the JS speech() object form).
public class Say
{
    public string[] Pages = Array.Empty<string>();
    public bool Wide, Hold, Instant;
    public (double x, double y)? Pos;
    public double? Tx, Ty;
    public int? Typer;
    public string? Voice;
    public string? Img;                // bubble sprite; null = the default small/wide one
    public Action<int>? OnFace;
}

// A monster's script (the JS NB.defs entry). Override what the monster does differently.
// One def is made per monster, so fields on a def are that monster's own state.
public abstract class MonsterDef
{
    public abstract string Name { get; }
    public virtual double Hp => 10;
    public virtual double Atk => 1;
    public virtual double Def => 0;
    public virtual double Xp => 0;
    public virtual double Gold => 0;
    public virtual double MercyMod => 0;
    public virtual int Wd => 100;
    public virtual string Encounter => $"* {Name} draws near!";
    public virtual (int w, int h) Size => (100, 100);
    public virtual (double x, double y)? Pos(int i, int n, double cx) => null;
    // the battle music key (assets/mus); null = silence
    public virtual string? MusicKey => "battle";
    // obj_battlebg frame: 1 = the green grid, -1 = none
    public virtual int BgFrame => 1;
    public virtual string? Voice => null;
    public virtual int Typer => 2;
    // the hurt shake: first offset and how much it shrinks each swing
    public virtual (int start, int step) ShudderSteps => (16, 2);
    public virtual void Setup(Fight f, Monster m) { }
    public virtual string[] Acts(Monster m) => new[] { "Check" };
    public virtual string? Check(Monster m) => null;
    // text to write, and an optional follow-up once it has been read
    public virtual (string text, Action? then) Act(Fight f, Monster m, int i, string name) => ("* Nothing happened.", null);
    public virtual string? Speech(Fight f, Monster m) => null;
    // the full form; by default the plain Speech text in a small bubble
    public virtual Say? SpeechEx(Fight f, Monster m) { var s = Speech(f, m); return s == null ? null : new Say { Pages = new[] { s } }; }
    public virtual (double x, double y)? BubblePos(Monster m) => null;
    public virtual int? Border(Fight f, Monster m) => null;
    // sets up the attack; returns the next turn's flavour text
    public virtual string? Attack(Fight f, Monster m) => null;
    public virtual void AttackStep(Fight f, Monster m) { }
    public virtual void EndAttack(Fight f, Monster m) { }
    public virtual string Flavor(Fight f, Monster m) => $"* {m.Name} is here.";
    public virtual void Step(Fight f, Monster m) { }
    public virtual void PreRender(Fight f, Monster m) { }
    public abstract void Draw(Fight f, Monster m);
    // drawn over the bullets (Undyne's shield)
    public virtual void DrawOver(Fight f, Monster m) { }
    // FIGHT damage before it lands, after it lands, and a hit that would kill
    // (return true when the script takes over instead of dying)
    public virtual double OnHitCalc(Fight f, Monster m, double dmg) => dmg;
    public virtual void OnDamaged(Fight f, Monster m, double dmg) { }
    public virtual bool OnLethal(Fight f, Monster m) => false;
    public virtual void OnMercy(Fight f, Monster m) { }
    public virtual void OnExit(Fight f, Monster m, string reason) { }
    public virtual bool NoFlee(Fight f, Monster m) => false;
    // where the damage number and HP bar go
    public virtual (double x, double y)? DmgPos(Monster m) => null;
    // the sprite the death animation dissolves (and its size on screen)
    public virtual string? VaporSprite(Monster m) => null;
    public virtual (string key, double x, double y, double w, double h)? Vapor(Monster m) =>
        VaporSprite(m) is { } k ? (k, m.X, m.Y, m.W, m.H) : null;
}
