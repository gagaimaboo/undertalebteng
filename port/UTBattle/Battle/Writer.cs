using Raylib_cs;
using UTBattle.Engine;

namespace UTBattle.Battle;

// SCR_TEXTTYPE typers
public record Typer(string Font, Color Color, int Dx, int Dy, Func<double, double> Wrap, int Speed, string[]? Sound, int Spacing, int VSpacing, double Shake = 0);

// OBJ_WRITER: types a page one character a step. UNDERTALE's codes:
//  &  new line      ^n  pause n*10 steps     /  wait for Z     %  end of message
//  \R \Y \W ...  colour     \E n  face change (passed to OnFace)
public class Writer
{
    public static readonly Dictionary<int, Typer> Typers = new()
    {
        [1] = new("main", Color.White, 20, 20, x => x + 602 - 55, 1, new[] { "text" }, 16, 32),
        [2] = new("plain", Color.Black, 0, 0, x => x + 190, 1, new[] { "txt2" }, 9, 20),
        // Undyne (39-45; her melting speech slows down line by line) and Undyne the Undying (94-96)
        [39] = Und(1, "txtUnd"), [40] = Und(1, "txtUnd", 1), [41] = Und(1, "txtUnd"),
        [42] = Und(2, "txtUnd", 2), [43] = Und(2, "txtUnd2", 2), [44] = Und(3, "txtUnd3", 2), [45] = Und(4, "txtUnd4", 2),
        [94] = Und(1, "txtUnd", 1), [95] = Und(2, "txtUnd", 2), [96] = Und(2, "txtUnd", 3),
    };
    static Typer Und(int speed, string snd, double shake = 0) => new("plain", Color.Black, 16, 0, x => x + 190, speed, new[] { snd }, 9, 20, shake);
    public static readonly Dictionary<char, string> TextColors = new()
    {
        ['R'] = "#ff0000", ['G'] = "#00ff00", ['W'] = "#ffffff", ['Y'] = "#ffff00", ['X'] = "#000000", ['B'] = "#0000ff",
        ['O'] = "#ff9f40", ['L'] = "#14a9ff", ['P'] = "#ff00ff", ['p'] = "#ffbbd4", ['g'] = "#888888",
    };

    readonly Fight S;
    public readonly string[] Msgs;
    public Typer Typer;
    public double X, Y;
    public bool Instant, NoSkip, Done;
    public string[]? Sound;
    public int No, Pos, Wait, Halt;
    public string Str = "";
    public Action? OnEnd;
    public Action<int>? OnFace;

    public Writer(Fight s, IEnumerable<string> msgs, int typer = 1, double? x = null, double? y = null, bool instant = false, string[]? sound = null, Action? onEnd = null)
    {
        S = s;
        Msgs = msgs.ToArray();
        Typer = Typers[typer];
        X = x ?? S.Ideal[0]; Y = y ?? S.Ideal[2];
        Instant = instant;
        Sound = sound ?? Typer.Sound;
        OnEnd = onEnd;
        Load();
    }

    void Load() { Str = No < Msgs.Length ? Msgs[No] : ""; Pos = Instant ? Str.Length : 0; Wait = Typer.Speed; Halt = 0; }

    static bool EndsWithClose(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, @"(^|[^/%])%%?$");

    public void Step()
    {
        if (Done) return;
        if (Halt > 0)
        {
            if (Input.Pressed(Input.K.Confirm))
            {
                Input.Clear(Input.K.Confirm);
                if (No + 1 < Msgs.Length) { No++; Load(); } else Finish();
            }
            return;
        }
        // X fills in the rest of any page, Z the rest of a page that waits for Z anyway
        if (Pos < Str.Length && !NoSkip)
        {
            bool z = S.MnFight != 2 && Input.Pressed(Input.K.Confirm) && WaitsForZ();
            if (Input.Pressed(Input.K.Cancel) || z || S.SkipT == S.T)
            {
                if (Input.Pressed(Input.K.Cancel)) Input.Clear(Input.K.Cancel); else if (z) Input.Clear(Input.K.Confirm);
                S.SkipT = S.T; Skip();
            }
        }
        if (Pos < Str.Length)
        {
            if (--Wait > 0) return;
            Pos++; Wait = Typer.Speed;
            char c = Str[Pos - 1];
            char n = Pos < Str.Length ? Str[Pos] : '\0';
            if (c == '^' && n != '\0' && n != '0') { Wait = (char.IsDigit(n) ? n - '0' : 0) * 10; Pos++; return; }
            if (c == '&') return;
            if (c == '\\')
            {
                if (n == 'E') { int d = Pos + 1 < Str.Length ? Str[Pos + 1] - '0' : 0; Pos += 2; OnFace?.Invoke(d); return; }
                Pos++; return;
            }
            if (Sound != null && c != ' ' && c != '/' && c != '%') Assets.Play(Sound[Random.Shared.Next(Sound.Length)]);
        }
        if (Pos >= Str.Length)
        {
            var t = Str.TrimEnd();
            var end = t.EndsWith("%%") ? t[..^1] : t;
            // a message ending in "%" without "/" closes as soon as it is typed out
            if (EndsWithClose(t)) { if (No + 1 < Msgs.Length) { No++; Load(); } else Finish(); }
            else if (end.EndsWith("/%") || end.EndsWith("/^") || end.EndsWith("/") || No + 1 < Msgs.Length) Halt = 1;
        }
    }

    public void Finish() { Done = true; var f = OnEnd; OnEnd = null; f?.Invoke(); }

    public bool WaitsForZ()
    {
        var s = Str.TrimEnd();
        if (EndsWithClose(s)) return false;
        var end = s.EndsWith("%%") ? s[..^1] : s;
        return end.EndsWith("/%") || end.EndsWith("/^") || end.EndsWith("/") || No + 1 < Msgs.Length;
    }

    public void Skip()
    {
        for (int i = Pos; i < Str.Length; i++)
            if (Str[i] == '\\' && i + 2 < Str.Length && Str[i + 1] == 'E') { OnFace?.Invoke(Str[i + 2] - '0'); i += 2; }
        Pos = Str.Length; Wait = Typer.Speed;
    }

    public void Draw()
    {
        var t = Typer;
        double x = X + t.Dx, y = Y + t.Dy, x0 = x, wrap = t.Wrap(X);
        var color = t.Color;
        var s = Str;
        for (int i = 0; i < Math.Min(Pos, s.Length); i++)
        {
            char c = s[i];
            if (c == '&') { x = x0; y += t.VSpacing; continue; }
            if (c == '^') { i++; continue; }
            if (c == '\\')
            {
                char k = i + 1 < s.Length ? s[i + 1] : '\0';
                if (k == 'E') { i += 2; continue; }
                if (TextColors.TryGetValue(k, out var hex)) color = Gfx.Hex(hex);
                i++; continue;
            }
            if (c == '/' || c == '%') break;
            if (x > wrap) { x = x0; y += t.VSpacing; }
            if (t.Shake > 0) { double k = t.Shake; Gfx.Glyph(t.Font, c, x + Random.Shared.NextDouble() * k - k / 2, y + Random.Shared.NextDouble() * k - k / 2, color); }
            else Gfx.Glyph(t.Font, c, x, y, color);
            x += t.Spacing;
        }
    }

    // NB.toUT: plain text ("\n") becomes UNDERTALE pages of up to 3 lines of 33 columns
    public static string[] ToUT(string text, int cols = 33, int rows = 3)
    {
        if (System.Text.RegularExpressions.Regex.IsMatch(text, "[&/%]") && !text.Contains('\n')) return new[] { text };
        var lines = new List<string>();
        foreach (var raw in text.Split('\n'))
        {
            int ind = raw.Length - raw.TrimStart(' ').Length;
            string indent = raw[..ind], cur = "";
            foreach (var w in raw[ind..].Split(' '))
            {
                var t = cur.Length > 0 ? cur + " " + w : indent + w;
                if (cur.Length > 0 && t.Length > cols) { lines.Add(cur); cur = (indent.Length > 0 ? indent : (raw.StartsWith("*") ? "  " : "")) + w; }
                else cur = t;
            }
            lines.Add(cur);
        }
        var pages = new List<string>();
        for (int i = 0; i < lines.Count; i += rows) pages.Add(string.Join("&", lines.Skip(i).Take(rows)));
        return pages.ToArray();
    }

    // pagesOf: text already in UT codes keeps its own end codes; a mid-string "/" starts the next page
    public static string[] PagesOf(string text, string end)
    {
        var p = ToUT(text);
        if (p.Length == 1 && System.Text.RegularExpressions.Regex.IsMatch(text, "[&/%]") && !text.Contains('\n'))
        {
            var q = System.Text.RegularExpressions.Regex.IsMatch(text, @"[/%^]\s*$") && System.Text.RegularExpressions.Regex.IsMatch(text, @"/[%^]?\s*$") ? text : text + end;
            return System.Text.RegularExpressions.Regex.Split(q, @"(?<=/)(?![%^]|\s*$)");
        }
        return p.Select((s, i) => i < p.Length - 1 ? s + "/" : s + end).ToArray();
    }
}
