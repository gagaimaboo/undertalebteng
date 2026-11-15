using Raylib_cs;

namespace UTBattle.Engine;

// Battle music: the .ogg tracks exported into assets/mus, streamed (one plays at a time).
public static class Music
{
    static readonly Dictionary<string, byte[]> files = new();
    static Raylib_cs.Music? cur;
    static byte[]? curBytes; // the stream reads from this buffer, so it has to stay alive
    public static string? Key { get; private set; }
    public static float Volume = .5f;

    public static void Add(string key, byte[] bytes) => files[key] = bytes;

    public static void Play(string key, bool loop = true)
    {
        if (Key == key) return;
        Stop();
        Key = key; // what should be playing, even with no sound card
        if (!Raylib.IsAudioDeviceReady() || !files.TryGetValue(key, out var b)) return;
        curBytes = b;
        var m = Raylib.LoadMusicStreamFromMemory(".ogg", b);
        m.Looping = loop;
        Raylib.SetMusicVolume(m, Volume);
        Raylib.PlayMusicStream(m);
        cur = m;
    }

    public static void Stop()
    {
        if (cur is { } m) { Raylib.StopMusicStream(m); Raylib.UnloadMusicStream(m); }
        cur = null; curBytes = null; Key = null;
    }

    public static void Pause() { if (cur is { } m) Raylib.PauseMusicStream(m); }
    public static void Resume() { if (cur is { } m) Raylib.ResumeMusicStream(m); }

    // every rendered frame: keeps the stream buffers filled
    public static void Update() { if (cur is { } m) Raylib.UpdateMusicStream(m); }
}
