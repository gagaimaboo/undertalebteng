using System.Numerics;
using System.Text.Json;
using Raylib_cs;

namespace UTBattle.Engine;

// Drawing in UNDERTALE's 640x480 room space, and its bitmap fonts:
// fnt_main (battle text), fnt_plain (speech bubbles) and the HUD's Mars font.
public static class Gfx
{
    // [sx, sy, sw, sh, shift, offset] per glyph
    static int[][] mainGlyphs = Array.Empty<int[]>();
    static readonly Dictionary<int, int[]> plainGlyphs = new(), marsGlyphs = new();

    public static void Init()
    {
        var main = Assets.Json("fonts/main.json");
        mainGlyphs = main.EnumerateArray().Select(a => a.EnumerateArray().Select(v => (int)v.GetDouble()).ToArray()).ToArray();
        Load(Assets.Json("fonts/plain.json"), plainGlyphs);
        Load(Assets.Json("fonts/mars.json"), marsGlyphs);
    }
    static void Load(JsonElement o, Dictionary<int, int[]> into)
    {
        foreach (var p in o.EnumerateObject())
            into[int.Parse(p.Name)] = p.Value.EnumerateArray().Select(v => (int)v.GetDouble()).ToArray();
    }

    public static Color Hex(string h)
    {
        h = h.TrimStart('#');
        return new Color(Convert.ToByte(h[..2], 16), Convert.ToByte(h[2..4], 16), Convert.ToByte(h[4..6], 16), (byte)255);
    }
    public static Color A(Color c, double alpha) => new(c.R, c.G, c.B, (byte)Math.Clamp(alpha * 255, 0, 255));

    public static void Rect(double x, double y, double w, double h, Color c) =>
        Raylib.DrawRectangle((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(w), (int)Math.Round(h), c);

    public static void Draw(Texture2D? t, double x, double y, double w = -1, double h = -1, Color? tint = null, double alpha = 1)
    {
        if (t is not { } tex) return;
        if (w < 0) w = tex.Width; if (h < 0) h = tex.Height;
        var c = tint ?? Color.White;
        Raylib.DrawTexturePro(tex, new Rectangle(0, 0, tex.Width, tex.Height),
            new Rectangle((float)Math.Round(x), (float)Math.Round(y), (float)w, (float)h), Vector2.Zero, 0, A(c, alpha * c.A / 255.0));
    }

    // GameMaker image_angle: counter-clockwise about the origin (ox, oy), which sits at (x, y)
    public static void DrawRot(Texture2D? t, double x, double y, double ox, double oy, double angle, double sx = 1, double sy = 1, Color? tint = null, double alpha = 1)
    {
        if (t is not { } tex) return;
        Raylib.DrawTexturePro(tex, new Rectangle(0, 0, tex.Width, tex.Height),
            new Rectangle((float)x, (float)y, (float)(tex.Width * sx), (float)(tex.Height * sy)),
            new Vector2((float)(ox * sx), (float)(oy * sy)), (float)-angle, A(tint ?? Color.White, alpha));
    }

    public static void DrawPart(Texture2D? t, int sx, int sy, int sw, int sh, double x, double y, double w, double h, Color tint)
    {
        if (t is not { } tex) return;
        Raylib.DrawTexturePro(tex, new Rectangle(sx, sy, sw, sh), new Rectangle((float)Math.Round(x), (float)Math.Round(y), (float)w, (float)h), Vector2.Zero, 0, tint);
    }

    // NB.drawGlyph
    public static void Glyph(string font, char ch, double x, double y, Color color)
    {
        if (font == "plain")
        {
            if (!plainGlyphs.TryGetValue(ch, out var g)) return;
            if (g[2] > 0 && g[3] > 0) DrawPart(Assets.Tex("nbPlainAtlas"), g[0], g[1], g[2], g[3], x + g[5], y, g[2], g[3], color);
            return;
        }
        int idx = ch - 32;
        var gl = idx >= 0 && idx < mainGlyphs.Length ? mainGlyphs[idx] : mainGlyphs[31];
        double k = font == "half" ? .5 : 1;
        if (gl[2] > 0 && gl[3] > 0) DrawPart(Assets.Tex("fntMainAtlas"), gl[0], gl[1], gl[2], gl[3], x + gl[5] * k, y, gl[2] * k, gl[3] * k, color);
    }

    public static void Text(string text, double x, double y, Color color, int spacing = 16)
    {
        double cx = x;
        foreach (var c in text) { Glyph("main", c, cx, y, color); cx += spacing; }
    }

    // the HUD's name / LV / HP numbers (drawMars)
    public static void Mars(string text, double x, double y, Color color)
    {
        var at = Assets.Tex("marsHudAtlas");
        double cx = x;
        foreach (var ch in text)
        {
            var g = marsGlyphs.TryGetValue(ch, out var v) ? v : marsGlyphs[32];
            if (g[2] > 0 && g[3] > 0) DrawPart(at, g[0], g[1], g[2], g[3], cx + g[5], y - 3, g[2], g[3], color);
            cx += g[4];
        }
    }
}
