// DT Chapter 2: Even Stronger - patcher
//
// Patches YOUR OWN copy of DELTARUNE Chapter 2 (chapter2_windows/data.win). Nothing from the game ships
// with this exe: the mod is new GML plus small anchored edits to existing code, matched by content rather
// than by offsets, so it works across game versions. Sprites and music the mod borrows from Chapters 3
// and 4 are exported from your own chapter3_windows / chapter4_windows data.win at patch time.
//
//   EvenStrongerPatcher                 (run from the DELTARUNE folder, or drag the folder/data.win onto it)
//   EvenStrongerPatcher <path>          (DELTARUNE folder, chapter2_windows folder or data.win)
//   EvenStrongerPatcher <path> --restore
//   EvenStrongerPatcher <path> --test   (also compiles the test harness, dev only)
//   EvenStrongerPatcher <path> --mod <dir> --out <file>   (dev: read the mod from disk, write elsewhere)

using System.Reflection;
using System.Text;
using ImageMagick;
using UndertaleModLib;
using UndertaleModLib.Compiler;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using Underanalyzer.Decompiler;


namespace EvenStronger;

static class Program
{
    const string Version = "0.1.6";
    const string BackupName = "data.win.even_stronger_backup";
    const string MarkerCode = "gml_GlobalScript_scr_es_version";

    static string modDir;          // dev override: read gml/ and patches/ from here
    static bool testBuild;
    static bool ownSaves;   // standalone zip: keep saves apart from the real DELTARUNE
    static readonly List<string> warnings = new();

    static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine($"DT Chapter 2: Even Stronger  -  patcher v{Version}");
        Console.WriteLine();
        string target = null, outFile = null;
        bool restore = false, pause = args.Length == 0;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--restore": restore = true; break;
                case "--test": testBuild = true; break;
                case "--own-saves": ownSaves = true; break;
                case "--mod": modDir = args[++i]; break;
                case "--out": outFile = args[++i]; break;
                case "--no-pause": pause = false; break;
                default: target = args[i]; break;
            }
        }
        int code;
        try { code = Run(target, outFile, restore); }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("Something broke: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            code = 1;
        }
        if (pause)
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter to close.");
            Console.ReadLine();
        }
        return code;
    }

    static string FindDataWin(string target)
    {
        var tries = new List<string>();
        void add(string p) { if (!string.IsNullOrEmpty(p)) tries.Add(p); }
        if (target != null)
        {
            if (File.Exists(target)) return Path.GetFullPath(target);
            add(Path.Combine(target, "data.win"));
            add(Path.Combine(target, "chapter2_windows", "data.win"));
        }
        foreach (var root in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            add(Path.Combine(root, "chapter2_windows", "data.win"));
            add(Path.Combine(root, "data.win"));
        }
        foreach (var p in tries) if (File.Exists(p)) return Path.GetFullPath(p);
        return null;
    }

    static UndertaleData Load(string path)
    {
        using var fs = File.OpenRead(path);
        return UndertaleIO.Read(fs, (w, b) => { }, m => { });
    }

    static void UnpackGameSpecificData()
    {
        string dir = Path.Combine(Path.GetTempPath(), "even_stronger_gsd");
        var asm = Assembly.GetExecutingAssembly();
        foreach (var r in asm.GetManifestResourceNames().Where(n => n.StartsWith("gsd/")))
        {
            string rel = r.Substring(4).Replace('\\', '/');
            string dest = Path.Combine(dir, "GameSpecificData", rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            using var s = asm.GetManifestResourceStream(r);
            using var f = File.Create(dest);
            s.CopyTo(f);
        }
        GameSpecificResolver.BaseDirectory = dir;
    }

    static int Run(string target, string outFile, bool restore)
    {
        UnpackGameSpecificData();
        string dataPath = FindDataWin(target);
        if (dataPath == null)
        {
            Console.WriteLine("Couldn't find chapter2_windows/data.win.");
            Console.WriteLine("Put this exe in your DELTARUNE folder (next to DELTARUNE.exe) and run it again,");
            Console.WriteLine("or drag the DELTARUNE folder onto it.");
            return 1;
        }
        string chDir = Path.GetDirectoryName(dataPath);
        string backup = Path.Combine(chDir, BackupName);
        Console.WriteLine("Game file: " + dataPath);

        if (restore)
        {
            if (!File.Exists(backup)) { Console.WriteLine("No backup found, nothing to restore."); return 1; }
            File.Copy(backup, dataPath, true);
            string assetDir = Path.Combine(chDir, "even_stronger");
            if (Directory.Exists(assetDir)) Directory.Delete(assetDir, true);
            Console.WriteLine("Restored the original Chapter 2.");
            return 0;
        }

        // always patch a clean copy: the backup if we already patched once, else the file itself
        string source = dataPath;
        if (File.Exists(backup))
        {
            Console.WriteLine("Found an earlier backup, patching from that clean copy.");
            source = backup;
        }
        Console.WriteLine("Reading the game...");
        var data = Load(source);
        if (data.Code.ByName("gml_Object_obj_ch2_scene9_Step_0") == null)
        {
            Console.WriteLine("This doesn't look like DELTARUNE Chapter 2. Point me at chapter2_windows/data.win.");
            return 1;
        }
        if (data.Code.ByName(MarkerCode) != null)
        {
            Console.WriteLine("This copy is already patched and there's no clean backup next to it.");
            Console.WriteLine("Reinstall/verify the game files, then run the patcher again.");
            return 1;
        }
        Console.WriteLine($"  {data.GeneralInfo?.DisplayName?.Content}, {data.Code.Count} code entries");

        if (outFile == null && source == dataPath)
        {
            File.Copy(dataPath, backup, false);
            Console.WriteLine("Backed up the original to " + BackupName);
        }

        ApplyMod(data);
        if (ownSaves)
        {
            // GameMaker keeps saves in %LOCALAPPDATA%\<game name>
            data.GeneralInfo.Name = data.Strings.MakeString("DELTARUNE_EVEN_STRONGER");
            Console.WriteLine("Saves go to %LOCALAPPDATA%\\DELTARUNE_EVEN_STRONGER");
        }

        string dest = outFile ?? dataPath;
        string tmp = dest + ".es_tmp";
        Console.WriteLine("Writing...");
        using (var fs = new FileStream(tmp, FileMode.Create))
            UndertaleIO.Write(fs, data);
        File.Move(tmp, dest, true);

        ExportMusic(Path.GetDirectoryName(Path.GetFullPath(dest)));
        ExportBorrowedAssets(Path.GetDirectoryName(Path.GetFullPath(dest)));

        Console.WriteLine();
        if (warnings.Count > 0)
        {
            Console.WriteLine($"Done, with {warnings.Count} warning(s):");
            foreach (var w in warnings) Console.WriteLine("  - " + w);
            Console.WriteLine("Warnings mean this game version moved something the mod hooks into;");
            Console.WriteLine("the rest of the mod still works, but send me that list.");
        }
        else Console.WriteLine("Done. Start Chapter 2 on a NEW file to play Even Stronger.");
        Console.WriteLine("To undo: run with --restore");
        return 0;
    }

    // ---------- mod content ----------

    static IEnumerable<(string name, string text)> ModFiles(string folder, string ext)
    {
        if (modDir != null)
        {
            string d = Path.Combine(modDir, folder);
            if (!Directory.Exists(d)) yield break;
            foreach (var f in Directory.GetFiles(d, "*" + ext, SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal))
                yield return (Path.GetFileNameWithoutExtension(f), File.ReadAllText(f));
            yield break;
        }
        var asm = Assembly.GetExecutingAssembly();
        foreach (var r in asm.GetManifestResourceNames().Where(n => n.StartsWith(folder + "/") && n.EndsWith(ext)).OrderBy(x => x, StringComparer.Ordinal))
        {
            using var s = asm.GetManifestResourceStream(r);
            using var sr = new StreamReader(s);
            yield return (Path.GetFileNameWithoutExtension(r.Substring(folder.Length + 1)), sr.ReadToEnd());
        }
    }

    static string ModText(string name)
    {
        if (modDir != null)
        {
            string p = Path.Combine(modDir, name);
            return File.Exists(p) ? File.ReadAllText(p) : "";
        }
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        if (s == null) return "";
        using var sr = new StreamReader(s);
        return sr.ReadToEnd();
    }

    static void ApplyMod(UndertaleData data)
    {
        var gctx = new GlobalDecompileContext(data);
        var settings = new DecompileSettings();

        // 1. edits to the game's own code, matched by content
        var patches = new List<Patch>();
        foreach (var (name, text) in ModFiles("patches", ".es"))
        {
            if (name.StartsWith("test_") && !testBuild) continue;
            patches.AddRange(Patch.Parse(name, text));
        }
        var edited = new Dictionary<string, List<string>>();
        int ok = 0;
        foreach (var p in patches)
        {
            var code = data.Code.ByName(p.Code);
            if (code == null)
            {
                Warn(p, "code entry not found in this version");
                continue;
            }
            if (!edited.TryGetValue(p.Code, out var lines))
            {
                string src = new DecompileContext(gctx, code, settings).DecompileToString();
                lines = src.Replace("\r\n", "\n").Split('\n').ToList();
                edited[p.Code] = lines;
            }
            if (p.Apply(lines)) ok++;
            else Warn(p, "anchor not found");
        }
        Console.WriteLine($"Edits: {ok}/{patches.Count} applied");

        // 2. new scripts and objects
        var objProps = ParseObjects(ModText("objects.txt"));
        var group = new CodeImportGroup(data, gctx, settings);
        int newCount = 0;
        foreach (var (name, text) in ModFiles("gml", ".gml"))
        {
            if (name.Contains("_es_test") && !testBuild) continue;
            group.QueueReplace(name, text);
            newCount++;
        }
        group.QueueReplace(MarkerCode, $"function scr_es_version()\n{{\n    return \"{Version}\";\n}}\n");
        foreach (var kv in edited)
            group.QueueReplace(kv.Key, string.Join("\n", kv.Value));
        Console.WriteLine($"Compiling {newCount} new code files and {edited.Count} edited ones...");
        group.Import(true);

        foreach (var (obj, props) in objProps)
        {
            var o = data.GameObjects.ByName(obj);
            if (o == null) { warnings.Add($"object {obj} was not created"); continue; }
            if (props.TryGetValue("persistent", out var pv)) o.Persistent = pv == "1";
            if (props.TryGetValue("depth", out var dv)) o.Depth = int.Parse(dv);
            if (props.TryGetValue("visible", out var vv)) o.Visible = vv == "1";
        }
    }

    static Dictionary<string, Dictionary<string, string>> ParseObjects(string text)
    {
        var res = new Dictionary<string, Dictionary<string, string>>();
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var d = new Dictionary<string, string>();
            foreach (var kv in parts.Skip(1))
            {
                var i = kv.IndexOf('=');
                if (i > 0) d[kv[..i]] = kv[(i + 1)..];
                else d[kv] = "1";
            }
            res[parts[0]] = d;
        }
        return res;
    }

    static void Warn(Patch p, string why)
    {
        if (p.Optional) Console.WriteLine($"  (optional edit skipped: {p.Id})");
        else warnings.Add($"{p.Id} ({p.Code}): {why}");
    }

    // ---------- the mod's own music (ships inside the exe) ----------
    // es_battle.ogg: encounter battle theme, "SWORD (FM Cover)" by plrusek
    static void ExportMusic(string chDir)
    {
        string outDir = Path.Combine(chDir, "even_stronger");
        Directory.CreateDirectory(outDir);
        int n = 0;
        if (modDir != null)
        {
            string d = Path.Combine(modDir, "music");
            if (Directory.Exists(d))
                foreach (var f in Directory.GetFiles(d, "*.ogg")) { File.Copy(f, Path.Combine(outDir, Path.GetFileName(f)), true); n++; }
        }
        else
        {
            var asm = Assembly.GetExecutingAssembly();
            foreach (var r in asm.GetManifestResourceNames().Where(x => x.StartsWith("music/")))
            {
                using var s = asm.GetManifestResourceStream(r);
                using var f = File.Create(Path.Combine(outDir, r.Substring(6)));
                s.CopyTo(f);
                n++;
            }
        }
        if (n > 0) Console.WriteLine($"Music: {n} track(s) -> {outDir}  (battle theme: SWORD FM cover by plrusek)");
    }

    // ---------- borrowed assets from Chapters 3 and 4 ----------
    // assets.txt lines:  sprite <chapter> <sprite_name> [outname]
    //                    sound  <chapter> <sound_name>  [outname]   (embedded or streamed .ogg)
    //                    music  <chapter> <mus/file.ogg> [outname]  (copied from the DELTARUNE folder)
    static void ExportBorrowedAssets(string chDir)
    {
        var lines = ModText("assets.txt").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#")).ToList();
        if (lines.Count == 0) return;
        string outDir = Path.Combine(chDir, "even_stronger");
        Directory.CreateDirectory(outDir);
        string root = Path.GetDirectoryName(chDir);
        var cache = new Dictionary<string, UndertaleData>();
        UndertaleData chapter(string n)
        {
            if (cache.TryGetValue(n, out var d)) return d;
            string p = Path.Combine(root, $"chapter{n}_windows", "data.win");
            if (!File.Exists(p)) { cache[n] = null; return null; }
            Console.WriteLine($"Reading Chapter {n} for borrowed sprites/sounds...");
            return cache[n] = Load(p);
        }
        int done = 0;
        foreach (var l in lines)
        {
            var a = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (a.Length < 3) continue;
            string kind = a[0], ch = a[1], name = a[2], outName = a.Length > 3 ? a[3] : name;
            try
            {
                if (kind == "music")
                {
                    string src = Path.Combine(root, name.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(src)) { File.Copy(src, Path.Combine(outDir, outName), true); done++; }
                    else warnings.Add($"missing {name} (needed from the Chapter {ch} music folder)");
                    continue;
                }
                var d = chapter(ch);
                if (d == null) { warnings.Add($"Chapter {ch} isn't installed, so {name} is missing"); continue; }
                if (kind == "sprite")
                {
                    var spr = d.Sprites.ByName(name);
                    if (spr == null) { warnings.Add($"sprite {name} not in Chapter {ch}"); continue; }
                    using var tw = new TextureWorker();
                    int w = (int)spr.Width, h = (int)spr.Height, n = spr.Textures.Count;
                    using var strip = new MagickImage(MagickColors.Transparent, (uint)(w * n), (uint)h);
                    for (int i = 0; i < n; i++)
                    {
                        var tex = spr.Textures[i]?.Texture;
                        if (tex == null) continue;
                        using var fr = tw.GetTextureFor(tex, name + "_" + i, true);
                        strip.Composite(fr, i * w, 0, CompositeOperator.Over);
                    }
                    strip.Write(Path.Combine(outDir, outName + ".png"), MagickFormat.Png32);
                    File.WriteAllText(Path.Combine(outDir, outName + ".txt"), $"{n} {spr.OriginX} {spr.OriginY}");
                    done++;
                }
                else if (kind == "sound")
                {
                    var snd = d.Sounds.ByName(name);
                    if (snd == null) { warnings.Add($"sound {name} not in Chapter {ch}"); continue; }
                    byte[] bytes = snd.AudioFile?.Data;
                    if (bytes == null && snd.AudioGroup != null && snd.AudioID >= 0)
                    {
                        // sound lives in audiogroupN.dat
                        string agPath = Path.Combine(root, $"chapter{ch}_windows", $"audiogroup{d.AudioGroups.IndexOf(snd.AudioGroup)}.dat");
                        if (File.Exists(agPath))
                        {
                            using var fs = File.OpenRead(agPath);
                            var ag = UndertaleIO.Read(fs, (w, b) => { }, m => { });
                            bytes = ag.EmbeddedAudio[snd.AudioID].Data;
                        }
                    }
                    if (bytes == null)
                    {
                        string fileName = snd.File?.Content ?? name;
                        string src = Path.Combine(root, $"chapter{ch}_windows", fileName.EndsWith(".ogg") ? fileName : fileName + ".ogg");
                        if (File.Exists(src)) bytes = File.ReadAllBytes(src);
                    }
                    if (bytes == null) { warnings.Add($"couldn't read sound {name}"); continue; }
                    bool wav = bytes.Length > 4 && bytes[0] == 'R' && bytes[1] == 'I';
                    File.WriteAllBytes(Path.Combine(outDir, outName + (wav ? ".wav" : ".ogg")), bytes);
                    done++;
                }
            }
            catch (Exception ex) { warnings.Add($"{kind} {name}: {ex.Message}"); }
        }
        Console.WriteLine($"Borrowed assets: {done}/{lines.Count} exported to {outDir}");
    }
}

// One anchored edit. File format (patches/*.es):
//
//   @@ gml_Object_obj_heroparent_Alarm_1  [optional] [all]   # id: name of the patch
//   @find
//   ...lines to look for (indentation and blank lines ignored)...
//   @after | @before | @replace
//   ...new lines...
//   @end
//
//   @@ <code>  /  @prepend ... @end   and   @@ <code>  /  @append ... @end
class Patch
{
    public string Id, Code, Mode;
    public bool Optional, All;
    public List<string> Find = new(), Body = new();

    static string Norm(string s) => string.Join(" ", s.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

    public static IEnumerable<Patch> Parse(string file, string text)
    {
        Patch cur = null;
        List<string> into = null;
        int n = 0;
        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            var t = raw.Trim();
            if (t.StartsWith("@@"))
            {
                var head = t[2..].Trim();
                string id = null;
                int hash = head.IndexOf('#');
                if (hash >= 0) { id = head[(hash + 1)..].Trim(); head = head[..hash].Trim(); }
                var parts = head.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                cur = new Patch { Code = parts[0], Id = $"{file}:{id ?? (++n).ToString()}", Optional = parts.Contains("optional"), All = parts.Contains("all") };
                into = null;
                continue;
            }
            if (cur == null) continue;
            switch (t)
            {
                case "@find": into = cur.Find; continue;
                case "@after": case "@before": case "@replace": case "@prepend": case "@append":
                    cur.Mode = t[1..]; into = cur.Body; continue;
                case "@end":
                    yield return cur;
                    cur = new Patch { Code = cur.Code, Id = $"{file}:{++n}", Optional = cur.Optional, All = cur.All };
                    into = null; continue;
            }
            into?.Add(raw);
        }
    }

    public bool Apply(List<string> lines)
    {
        if (Mode == "prepend") { lines.InsertRange(0, Body); return true; }
        if (Mode == "append") { lines.AddRange(Body); return true; }
        var want = Find.Select(Norm).Where(s => s.Length > 0).ToList();
        if (want.Count == 0) return false;
        bool any = false;
        int from = 0;
        while (true)
        {
            int start = -1, end = -1;
            for (int i = from; i < lines.Count && start < 0; i++)
            {
                if (Norm(lines[i]) != want[0]) continue;
                int j = i, k = 0;
                while (j < lines.Count && k < want.Count)
                {
                    var nl = Norm(lines[j]);
                    if (nl.Length == 0) { j++; continue; }
                    if (nl != want[k]) break;
                    j++; k++;
                }
                if (k == want.Count) { start = i; end = j; }
            }
            if (start < 0) return any;
            any = true;
            switch (Mode)
            {
                case "after": lines.InsertRange(end, Body); from = end + Body.Count; break;
                case "before": lines.InsertRange(start, Body); from = end + Body.Count; break;
                case "replace":
                    lines.RemoveRange(start, end - start);
                    lines.InsertRange(start, Body);
                    from = start + Body.Count; break;
                default: return false;
            }
            if (!All) return true;
        }
    }
}
