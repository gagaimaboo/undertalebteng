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
    public double[] Box = { 0, 0, 8, 8 };
    public double Dmg = 1, Frame, Anim, Alpha = 1;
    public bool WallKill = true, Kill = true, Alive = true, Manual, Hidden, DiedOnce;
    public int T, Wait, A0, A1;
    public string? Img;                // a sprite key (Assets.Frame picks the frame)
    public Action<Bullet>? Step;
    public Action<Bullet>? OnDie;
    public Monster? Owner;
}

// A monster's script (the JS NB.defs entry). Override what the monster does differently.
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
    public virtual string[] Acts(Monster m) => new[] { "Check" };
    public virtual string? Check(Monster m) => null;
    // text to write, and an optional follow-up once it has been read
    public virtual (string text, Action? then) Act(Fight f, Monster m, int i, string name) => ("* Nothing happened.", null);
    public virtual string? Speech(Fight f, Monster m) => null;
    public virtual int? Border(Fight f, Monster m) => null;
    // sets up the attack; returns the next turn's flavour text
    public virtual string? Attack(Fight f, Monster m) => null;
    public virtual void AttackStep(Fight f, Monster m) { }
    public virtual void EndAttack(Fight f, Monster m) { }
    public virtual string Flavor(Fight f, Monster m) => $"* {m.Name} is here.";
    public virtual void Step(Fight f, Monster m) { }
    public virtual void PreRender(Fight f, Monster m) { }
    public abstract void Draw(Fight f, Monster m);
    // the sprite the death animation dissolves (and its size on screen)
    public virtual string? VaporSprite(Monster m) => null;
}
