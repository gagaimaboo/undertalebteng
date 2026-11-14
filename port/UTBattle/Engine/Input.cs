using Raylib_cs;

namespace UTBattle.Engine;

// UNDERTALE's keys: Z/Enter confirm, X/Shift cancel, C/Ctrl menu, arrows or WASD.
// A press between two 30 fps steps is latched, so a quick tap still counts.
public static class Input
{
    public enum K { Confirm, Cancel, Menu, Up, Down, Left, Right }
    static readonly KeyboardKey[][] map =
    {
        new[] { KeyboardKey.Z, KeyboardKey.Enter },
        new[] { KeyboardKey.X, KeyboardKey.LeftShift, KeyboardKey.RightShift },
        new[] { KeyboardKey.C, KeyboardKey.LeftControl, KeyboardKey.RightControl },
        new[] { KeyboardKey.Up, KeyboardKey.W },
        new[] { KeyboardKey.Down, KeyboardKey.S },
        new[] { KeyboardKey.Left, KeyboardKey.A },
        new[] { KeyboardKey.Right, KeyboardKey.D },
    };
    static readonly bool[] latched = new bool[7], held = new bool[7], pressed = new bool[7], prev = new bool[7];

    // every rendered frame
    public static void Poll()
    {
        for (int k = 0; k < 7; k++)
            foreach (var key in map[k]) if (Raylib.IsKeyPressed(key)) latched[k] = true;
    }

    // every 30 fps step
    public static void Step()
    {
        for (int k = 0; k < 7; k++)
        {
            bool h = map[k].Any(key => (bool)Raylib.IsKeyDown(key));
            pressed[k] = (h && !prev[k]) || latched[k];
            latched[k] = false; prev[k] = h; held[k] = h;
        }
    }

    // scripted input (the --autotest mode): counts as a key tap on the next step
    public static void Inject(K k) => latched[(int)k] = true;
    public static bool Pressed(K k) => pressed[(int)k];
    public static bool Held(K k) => held[(int)k];
    public static void Clear(K k) { pressed[(int)k] = false; latched[(int)k] = false; }
    public static void ClearAll() { for (int k = 0; k < 7; k++) { pressed[k] = false; latched[k] = false; } }
}
