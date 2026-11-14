using System.Text.Json;
using Raylib_cs;

namespace UTBattle.Engine;

// Sprites and sounds exported from the web build (port/tools/export_assets.js).
// Sprites are named by their key in the JS `images` table; arrays become key_0, key_1, ...
public static class Assets
{
    static readonly Dictionary<string, Texture2D> textures = new();
    static readonly Dictionary<string, Texture2D[]> frames = new();
    static readonly Dictionary<string, Sound[]> sounds = new();
    static readonly Dictionary<string, int> soundNext = new();

    // name -> bytes: the assets embedded in the exe (one file to hand around), or an assets
    // folder next to it, which wins so sprites can be swapped without rebuilding
    static Dictionary<string, byte[]> Files()
    {
        var files = new Dictionary<string, byte[]>();
        var asm = typeof(Assets).Assembly;
        foreach (var n in asm.GetManifestResourceNames())
        {
            if (!n.StartsWith("assets/")) continue;
            using var st = asm.GetManifestResourceStream(n)!;
            using var ms = new MemoryStream(); st.CopyTo(ms);
            files[n["assets/".Length..].Replace('\\', '/')] = ms.ToArray();
        }
        var dir = Path.Combine(AppContext.BaseDirectory, "assets");
        if (Directory.Exists(dir))
            foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                files[Path.GetRelativePath(dir, f).Replace('\\', '/')] = File.ReadAllBytes(f);
        return files;
    }
    static Dictionary<string, byte[]> data = new();

    public static void Init()
    {
        data = Files();
        foreach (var (name, bytes) in data)
        {
            if (!name.StartsWith("img/") || !name.EndsWith(".png")) continue;
            var im = Raylib.LoadImageFromMemory(".png", bytes);
            var t = Raylib.LoadTextureFromImage(im);
            Raylib.UnloadImage(im);
            Raylib.SetTextureFilter(t, TextureFilter.Point);
            textures[Path.GetFileNameWithoutExtension(name)] = t;
        }
        // group key_0, key_1, ... into frame arrays
        foreach (var name in textures.Keys.ToList())
        {
            int us = name.LastIndexOf('_');
            if (us < 0 || !int.TryParse(name[(us + 1)..], out _)) continue;
            var key = name[..us];
            if (frames.ContainsKey(key)) continue;
            var list = new List<Texture2D>();
            for (int i = 0; textures.TryGetValue($"{key}_{i}", out var t); i++) list.Add(t);
            if (list.Count > 0) frames[key] = list.ToArray();
        }
        // no sound card (or it failed to open): run silent instead of crashing
        if (!Raylib.IsAudioDeviceReady()) return;
        foreach (var (name, bytes) in data)
        {
            if (!name.StartsWith("snd/")) continue;
            var wave = Raylib.LoadWaveFromMemory(Path.GetExtension(name), bytes);
            var s = Raylib.LoadSoundFromWave(wave);
            Raylib.UnloadWave(wave);
            // a few voices each, so the same sound can overlap itself
            var pool = new Sound[4];
            pool[0] = s;
            for (int i = 1; i < pool.Length; i++) pool[i] = Raylib.LoadSoundAlias(s);
            sounds[Path.GetFileNameWithoutExtension(name)] = pool;
        }
    }

    public static Texture2D? Tex(string key) => textures.TryGetValue(key, out var t) ? t : null;
    public static Texture2D[]? Frames(string key) => frames.TryGetValue(key, out var f) ? f : null;
    public static Texture2D? Frame(string key, double i)
    {
        var f = Frames(key);
        if (f == null || f.Length == 0) return Tex(key);
        int k = ((int)Math.Floor(i) % f.Length + f.Length) % f.Length;
        return f[k];
    }

    public static void Play(string key, float pitch = 1f, float volume = 1f)
    {
        if (!sounds.TryGetValue(key, out var pool)) return;
        int n = soundNext.TryGetValue(key, out var v) ? v : 0;
        soundNext[key] = (n + 1) % pool.Length;
        var s = pool[n];
        Raylib.SetSoundPitch(s, pitch);
        Raylib.SetSoundVolume(s, volume);
        Raylib.PlaySound(s);
    }

    public static JsonElement Json(string rel)
    {
        using var doc = JsonDocument.Parse(data[rel]);
        return doc.RootElement.Clone();
    }
}
