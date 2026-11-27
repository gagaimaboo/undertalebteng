// Transplant: copies objects, script functions, sprites and sounds from the player's own Chapter 3 into
// Chapter 2 at patch time, by name, following everything the copied code refers to. Nothing from the game
// ships with the patcher: transplant.txt only lists names.
//
//   object <name>                  copy a Chapter 3 object (and everything it needs)
//   dbc <newname> <type> <type>..  a copy of Chapter 3's obj_dbulletcontroller holding only those pattern types
//   rename <old> <new>             rename an identifier inside all transplanted code
//   stub <name>                    point an identifier at obj_es_stub (an empty object) instead of following it
//   assetvar <var> object|sprite [codeentry]   numbers the decompiler left in "var = N" / "var == N" are
//                                  Chapter 3 asset indices: put the asset's name back (only in that entry, if given)
//   keep <function>                use Chapter 2's version of a function even if Chapter 3's differs
//                                  (by default, a same-named function that differs is copied as es3_<name>)
//   argfix <function> <arg#> object|sprite|sound   a plain number passed as that argument (counting from 0) is a
//                                  Chapter 3 asset index: put its name back
//   allow <prefix>                 only Chapter 3 objects starting with one of these are copied; any other
//                                  Chapter 3-only object the code mentions is stubbed (only if allow is used)
//   sprite <name> / sound <name>   copy one asset explicitly

using System.Text.RegularExpressions;
using ImageMagick;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using Underanalyzer.Decompiler;

namespace EvenStronger;

sealed class TransplantResult
{
    public readonly Dictionary<string, string> Code = new();                       // code entry name -> GML
    public readonly Dictionary<string, UndertaleGameObject> Objects = new();      // new object name -> Chapter 3 original (for its properties)
    public readonly List<(string from, string to)> Files = new();                 // streamed audio files to copy next to data.win
    public readonly List<string> Stubbed = new();
    public readonly List<string> Owned = new();
    public readonly List<string> NumFixes = new();
    public int Sprites, Sounds, Functions;
}

static class Transplant
{
    static readonly Regex Ident = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
    static readonly Regex EnumDecl = new(@"enum\s+\w+\s*\{[^}]*\}", RegexOptions.Compiled);

    public static TransplantResult Run(UndertaleData d2, UndertaleData d3, string ch3Dir, string spec, List<string> warnings, GlobalDecompileContext gctx2)
    {
        var res = new TransplantResult();
        var renames = new Dictionary<string, string>();
        var stubs = new HashSet<string>();
        var rootObjects = new List<string>();
        var dbcs = new List<(string name, HashSet<int> types)>();
        var wantSprites = new List<string>();
        var wantSounds = new List<string>();
        var allow = new List<string>();
        var assetVars = new List<(string var, string kind, string entry)>();
        var keep = new HashSet<string>();
        var argFixes = new List<(string fn, int arg, string kind)>();
        foreach (var raw in spec.Split('\n'))
        {
            var l = raw.Trim();
            if (l.Length == 0 || l.StartsWith("#")) continue;
            var a = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (a[0])
            {
                case "object": rootObjects.Add(a[1]); break;
                case "rename": renames[a[1]] = a[2]; break;
                case "stub": stubs.Add(a[1]); break;
                case "sprite": wantSprites.Add(a[1]); break;
                case "sound": wantSounds.Add(a[1]); break;
                case "allow": allow.Add(a[1]); break;
                case "keep": keep.Add(a[1]); break;
                case "argfix": argFixes.Add((a[1], int.Parse(a[2]), a[3])); break;
                case "assetvar": assetVars.Add((a[1], a[2], a.Length > 3 ? a[3] : null)); break;
                case "dbc": dbcs.Add((a[1], a.Skip(2).Select(int.Parse).ToHashSet())); break;
            }
        }

        // what Chapter 2 already has, by name
        var have = new HashSet<string>();
        void AddNames<T>(IEnumerable<T> list, Func<T, UndertaleString> name) { foreach (var x in list) { var n = name(x)?.Content; if (n != null) have.Add(n); } }
        AddNames(d2.Sprites, x => x.Name); AddNames(d2.Sounds, x => x.Name); AddNames(d2.GameObjects, x => x.Name);
        AddNames(d2.Rooms, x => x.Name); AddNames(d2.Fonts, x => x.Name); AddNames(d2.Shaders, x => x.Name);
        AddNames(d2.Scripts, x => x.Name); AddNames(d2.Paths, x => x.Name);
        foreach (var c in d2.Code) { var n = c.Name?.Content; if (n != null && n.StartsWith("gml_Script_")) have.Add(n.Substring(11)); }
        foreach (var r in renames.Values) have.Add(r);
        foreach (var (n, _) in dbcs) have.Add(n);

        // Chapter 3's names, by kind
        var s3 = d3.Sprites.Where(x => x.Name != null).ToDictionary(x => x.Name.Content, x => x);
        var a3 = d3.Sounds.Where(x => x.Name != null).ToDictionary(x => x.Name.Content, x => x);
        var o3 = d3.GameObjects.Where(x => x.Name != null).ToDictionary(x => x.Name.Content, x => x);
        var f3 = new Dictionary<string, UndertaleCode>();
        foreach (var c in d3.Code) { var n = c.Name?.Content; if (n != null && n.StartsWith("gml_Script_")) f3[n.Substring(11)] = c; }
        var other3 = new HashSet<string>();
        foreach (var x in d3.Rooms) if (x.Name != null) other3.Add(x.Name.Content);
        foreach (var x in d3.Fonts) if (x.Name != null) other3.Add(x.Name.Content);
        foreach (var x in d3.Shaders) if (x.Name != null) other3.Add(x.Name.Content);
        foreach (var x in d3.Paths) if (x.Name != null) other3.Add(x.Name.Content);

        string q = Environment.GetEnvironmentVariable("ES_INDEX_QUERY");
        if (!string.IsNullOrEmpty(q))
            foreach (var t in q.Split(','))
            {
                int i = int.Parse(t);
                Console.WriteLine($"  #{i}: sprite={(i < d3.Sprites.Count ? d3.Sprites[i].Name?.Content : "-")} object={(i < d3.GameObjects.Count ? d3.GameObjects[i].Name?.Content : "-")} sound={(i < d3.Sounds.Count ? d3.Sounds[i].Name?.Content : "-")}");
            }
        var f2 = new Dictionary<string, UndertaleCode>();
        foreach (var c in d2.Code) { var n = c.Name?.Content; if (n != null && n.StartsWith("gml_Script_")) f2[n.Substring(11)] = c; }
        var own = new Dictionary<string, string>();       // functions Chapter 3 code gets its own copy of
        var gctx = new GlobalDecompileContext(d3);
        var settings = new DecompileSettings();
        var decompCache = new Dictionary<UndertaleCode, string>();
        string Decomp(UndertaleCode c)
        {
            if (!decompCache.TryGetValue(c, out var t))
                decompCache[c] = t = new DecompileContext(gctx, c, settings).DecompileToString().Replace("\r\n", "\n");
            return t;
        }
        string Rewrite(string text)
        {
            if (renames.Count == 0) return text;
            return Ident.Replace(text, m => renames.TryGetValue(m.Value, out var r) ? r : m.Value);
        }
        string FixNums(string text, string entry)
        {
            foreach (var (v, kind, only) in assetVars)
            {
                if (only != null && only != entry) continue;
                text = Regex.Replace(text, @"\b(" + Regex.Escape(v) + @")(\s*(?:==|!=|=)\s*)(\d+)\b(?!\.)", m =>
                {
                    int i = int.Parse(m.Groups[3].Value);
                    string name = kind == "sprite" ? (i < d3.Sprites.Count ? d3.Sprites[i].Name?.Content : null)
                                : (i < d3.GameObjects.Count ? d3.GameObjects[i].Name?.Content : null);
                    return name == null ? m.Value : m.Groups[1].Value + m.Groups[2].Value + name;
                });
            }
            // numbers passed as a known asset argument of a function
            foreach (var (fn, argi, kind) in argFixes)
            {
                int pos = 0;
                while (true)
                {
                    var m = Regex.Match(text.Substring(pos), @"\b" + Regex.Escape(fn) + @"\s*\(");
                    if (!m.Success) break;
                    int open = pos + m.Index + m.Length - 1;
                    int depth = 0, argStart = open + 1, n = 0, close = -1;
                    var args = new List<(int s, int e)>();
                    for (int k = open; k < text.Length; k++)
                    {
                        char ch = text[k];
                        if (ch == '"') { for (k++; k < text.Length && text[k] != '"'; k++) { } continue; }
                        if (ch == '(' || ch == '[') depth++;
                        else if (ch == ')' || ch == ']') { depth--; if (depth == 0) { args.Add((argStart, k)); close = k; break; } }
                        else if (ch == ',' && depth == 1) { args.Add((argStart, k)); argStart = k + 1; n++; }
                    }
                    if (close < 0) break;
                    if (argi < args.Count)
                    {
                        var (as0, ae) = args[argi];
                        string arg = text.Substring(as0, ae - as0).Trim();
                        if (Regex.IsMatch(arg, @"^\d+$"))
                        {
                            int i = int.Parse(arg);
                            string name = kind == "sprite" ? (i < d3.Sprites.Count ? d3.Sprites[i].Name?.Content : null)
                                        : kind == "sound" ? (i < d3.Sounds.Count ? d3.Sounds[i].Name?.Content : null)
                                        : (i < d3.GameObjects.Count ? d3.GameObjects[i].Name?.Content : null);
                            if (name != null)
                            {
                                res.NumFixes.Add($"{entry}: {fn} arg {argi} {i} -> {name}");
                                int lead = text.Substring(as0, ae - as0).IndexOf(arg);
                                text = text.Substring(0, as0 + lead) + name + text.Substring(as0 + lead + arg.Length);
                                close += name.Length - arg.Length;
                            }
                        }
                    }
                    pos = close + 1;
                }
            }
            // and generally: "growtangle = 1517" where Chapter 3's asset #1517 is obj_growtangle
            text = Regex.Replace(text, @"\b([A-Za-z_][A-Za-z0-9_]*)(\s*(?:==|!=|=)\s*)(\d{3,5})\b(?![.\d])", m =>
            {
                int i = int.Parse(m.Groups[3].Value);
                // the asset with that index is named elsewhere in the same code: certainly that one
                string o3n = i < d3.GameObjects.Count ? d3.GameObjects[i].Name?.Content : null;
                string s3n = i < d3.Sprites.Count ? d3.Sprites[i].Name?.Content : null;
                foreach (var named in new[] { o3n, s3n })
                    if (named != null && Regex.IsMatch(text, @"\b" + Regex.Escape(named) + @"\b"))
                    {
                        res.NumFixes.Add($"{entry}: {m.Groups[1].Value} {m.Groups[3].Value} -> {named}");
                        return m.Groups[1].Value + m.Groups[2].Value + named;
                    }
                string v = m.Groups[1].Value.ToLowerInvariant().Replace("_", "");
                if (v.Length < 4) return m.Value;
                foreach (var cand in new[] { i < d3.GameObjects.Count ? d3.GameObjects[i].Name?.Content : null, i < d3.Sprites.Count ? d3.Sprites[i].Name?.Content : null })
                {
                    if (cand == null) continue;
                    string c = cand.ToLowerInvariant().Replace("obj_", "").Replace("spr_", "").Replace("_", "");
                    if (c.Contains(v) || v.Contains(c))
                    {
                        res.NumFixes.Add($"{entry}: {m.Groups[1].Value} {m.Groups[3].Value} -> {cand}");
                        return m.Groups[1].Value + m.Groups[2].Value + cand;
                    }
                }
                return m.Value;
            });
            return text;
        }
        var rootSet = new HashSet<string>(rootObjects);
        bool Allowed(string obj) => allow.Count == 0 || rootSet.Contains(obj) || allow.Any(p => obj.StartsWith(p));

        var objQueue = new Queue<string>();
        var fnQueue = new Queue<string>();
        var sprites = new HashSet<string>();
        var sounds = new HashSet<string>();
        var seen = new HashSet<string>();
        var warned = new HashSet<string>();

        string Norm(string t) => Regex.Replace(t ?? "", @"\s+", " ").Trim();
        string Ch2Function(string fn)
        {
            var c = f2[fn];
            var parent = c.ParentEntry ?? c;
            return ExtractFunction(new DecompileContext(gctx2, parent, settings).DecompileToString().Replace("\r\n", "\n"), fn);
        }
        void Scan(string text)
        {
            foreach (Match m in Ident.Matches(text))
            {
                var id = m.Value;
                if (!seen.Add(id)) continue;
                if (stubs.Contains(id)) continue;
                // same-named function in both chapters: if they differ, Chapter 3 code keeps Chapter 3's
                if (f2.ContainsKey(id) && f3.ContainsKey(id) && !keep.Contains(id) && !own.ContainsKey(id))
                {
                    var p3 = f3[id].ParentEntry ?? f3[id];
                    string t3 = ExtractFunction(Decomp(p3), id);
                    string t2 = null;
                    try { t2 = Ch2Function(id); } catch { }
                    if (t3 != null && Norm(t2) != Norm(t3))
                    {
                        own[id] = "es3_" + id;
                        fnQueue.Enqueue(id);
                        continue;
                    }
                }
                if (have.Contains(id)) continue;
                if (s3.ContainsKey(id)) sprites.Add(id);
                else if (a3.ContainsKey(id)) sounds.Add(id);
                else if (o3.ContainsKey(id)) { if (Allowed(id)) objQueue.Enqueue(id); else stubs.Add(id); }
                else if (f3.ContainsKey(id)) fnQueue.Enqueue(id);
                else if (other3.Contains(id) && warned.Add(id)) warnings.Add($"transplant: {id} is a Chapter 3 room/font/shader/path, not copied");
            }
        }
        void Object(string name)
        {
            if (res.Objects.ContainsKey(name)) return;
            var o = o3[name];
            res.Objects[name] = o;
            have.Add(name);
            if (o.Sprite != null) Scan(o.Sprite.Name.Content);
            if (o.TextureMaskId != null) Scan(o.TextureMaskId.Name.Content);
            if (o.ParentId != null)
            {
                // a parent can't be stubbed: copy it even if it's not on the allow list
                string pn = o.ParentId.Name.Content;
                if (!have.Contains(pn) && o3.ContainsKey(pn)) objQueue.Enqueue(pn);
            }
            foreach (var evList in o.Events)
                foreach (var ev in evList)
                    foreach (var act in ev.Actions)
                    {
                        var code = act.CodeId;
                        if (code == null) continue;
                        string cname = Rewrite(code.Name.Content);
                        string text = Rewrite(FixNums(Decomp(code), code.Name.Content));
                        if (cname.Contains("_Collision_"))
                        {
                            string other = cname.Substring(cname.IndexOf("_Collision_") + 11);
                            if (!have.Contains(other) && !(o3.ContainsKey(other) && Allowed(other))) { stubs.Add(other); continue; }
                        }
                        res.Code[cname] = text;
                        Scan(cname);
                        Scan(text);
                    }
        }
        var doneFns = new HashSet<string>();
        void Function(string fn)
        {
            if (!doneFns.Add(fn)) return;
            if (have.Contains(fn) && !own.ContainsKey(fn)) return;
            have.Add(fn);
            var child = f3[fn];
            var parent = child.ParentEntry ?? child;
            string src = Decomp(parent);
            string block = ExtractFunction(src, fn);
            if (block == null) { warnings.Add($"transplant: couldn't find function {fn} in {parent.Name.Content}"); return; }
            string enums = string.Join("\n", EnumDecl.Matches(src).Select(m => m.Value));
            string text = Rewrite(block + "\n\n" + enums + "\n");
            res.Code["gml_GlobalScript_es3_" + fn] = text;
            res.Functions++;
            Scan(text);
        }

        foreach (var n in rootObjects)
        {
            if (!o3.ContainsKey(n)) { warnings.Add($"transplant: object {n} isn't in Chapter 3"); continue; }
            objQueue.Enqueue(n);
        }
        foreach (var (name, types) in dbcs)
        {
            if (!o3.TryGetValue("obj_dbulletcontroller", out var dbc)) { warnings.Add("transplant: Chapter 3 has no obj_dbulletcontroller"); continue; }
            foreach (var evList in dbc.Events)
                foreach (var ev in evList)
                    foreach (var act in ev.Actions)
                    {
                        var code = act.CodeId;
                        if (code == null) continue;
                        string text = Decomp(code);
                        string cname = code.Name.Content.Replace("obj_dbulletcontroller", name);
                        if (cname.EndsWith("_Step_0")) text = KeepTypeBlocks(text, types);
                        text = Rewrite(text);
                        res.Code[cname] = text;
                        Scan(text);
                    }
            res.Objects[name] = dbc;
        }
        foreach (var n in wantSprites) if (s3.ContainsKey(n) && !have.Contains(n)) sprites.Add(n);
        foreach (var n in wantSounds) if (a3.ContainsKey(n) && !have.Contains(n)) sounds.Add(n);

        while (objQueue.Count > 0 || fnQueue.Count > 0)
        {
            while (objQueue.Count > 0) Object(objQueue.Dequeue());
            while (fnQueue.Count > 0) Function(fnQueue.Dequeue());
        }

        if (Environment.GetEnvironmentVariable("ES_NUMSCAN") == "1")
        {
            var hint = new Regex("knight|rk_|sword|star|slash|roar|crescent|bullet|heart|box|ball|spark|beam|warp|circle|stream", RegexOptions.IgnoreCase);
            foreach (var kv in res.Code)
                foreach (Match m in Regex.Matches(kv.Value, @"(?<![\w.])(\d{2,5})(?![\w.])"))
                {
                    int i = int.Parse(m.Groups[1].Value);
                    string sn = i < d3.Sprites.Count ? d3.Sprites[i].Name?.Content : null;
                    string on = i < d3.GameObjects.Count ? d3.GameObjects[i].Name?.Content : null;
                    if (on != null && hint.IsMatch(on)) sn = (sn ?? "") + " / OBJ " + on;
                    if (sn != null && hint.IsMatch(sn))
                    {
                        int ls = kv.Value.LastIndexOf('\n', m.Index) + 1; int le = kv.Value.IndexOf('\n', m.Index); if (le < 0) le = kv.Value.Length;
                        Console.WriteLine($"NUM {kv.Key.Replace("gml_Object_", "")}: {i}={sn} | {kv.Value.Substring(ls, le - ls).Trim()}");
                    }
                }
        }
        // anything we chose not to copy points at an empty object; Chapter 3's own versions of
        // functions Chapter 2 also has are called by their es3_ names
        if (stubs.Count > 0 || own.Count > 0)
            foreach (var k in res.Code.Keys.ToList())
                res.Code[k] = Ident.Replace(res.Code[k], m => stubs.Contains(m.Value) ? "obj_es_stub" : own.TryGetValue(m.Value, out var o) ? o : m.Value);
        res.Owned.AddRange(own.Keys.OrderBy(x => x));
        res.Stubbed.AddRange(stubs.OrderBy(x => x));

        // assets go in before any code is compiled, so the copied code finds them by name
        var pagePack = new PagePacker(d2);
        foreach (var n in sprites.OrderBy(x => x)) { if (ImportSprite(d2, s3[n], pagePack, warnings)) res.Sprites++; }
        pagePack.Flush();
        foreach (var n in sounds.OrderBy(x => x)) { if (ImportSound(d2, d3, a3[n], ch3Dir, res, warnings)) res.Sounds++; }

        // the empty object stubs point at (never has instances)
        if (d2.GameObjects.ByName("obj_es_stub") == null)
            d2.GameObjects.Add(new UndertaleGameObject { Name = d2.Strings.MakeString("obj_es_stub") });
        // objects exist before the code import (some have no events of their own)
        foreach (var (name, _) in res.Objects)
            if (d2.GameObjects.ByName(name) == null)
                d2.GameObjects.Add(new UndertaleGameObject { Name = d2.Strings.MakeString(name) });
        return res;
    }

    // properties of the copied objects, once everything exists
    public static void FinishObjects(UndertaleData d2, TransplantResult res, List<string> warnings)
    {
        foreach (var (name, src) in res.Objects)
        {
            var o = d2.GameObjects.ByName(name);
            if (o == null) { warnings.Add($"transplant: object {name} wasn't created"); continue; }
            o.Visible = src.Visible;
            o.Solid = src.Solid;
            o.Depth = src.Depth;
            o.Persistent = src.Persistent;
            o.Sprite = src.Sprite == null ? null : d2.Sprites.ByName(src.Sprite.Name.Content);
            o.TextureMaskId = src.TextureMaskId == null ? null : d2.Sprites.ByName(src.TextureMaskId.Name.Content);
            if (src.ParentId != null)
            {
                var p = d2.GameObjects.ByName(src.ParentId.Name.Content);
                if (p == null) warnings.Add($"transplant: parent {src.ParentId.Name.Content} of {name} missing");
                o.ParentId = p;
            }
        }
    }

    // "function name(...) { ... }" out of a global script
    static string ExtractFunction(string src, string fn)
    {
        var m = Regex.Match(src, @"(?m)^function\s+" + Regex.Escape(fn) + @"\s*\(");
        if (!m.Success) return null;
        int brace = src.IndexOf('{', m.Index);
        if (brace < 0) return null;
        int end = MatchBrace(src, brace);
        return end < 0 ? null : src.Substring(m.Index, end - m.Index + 1);
    }

    // keep the shared code before the first pattern, plus only the wanted "if (type == N)" blocks
    static string KeepTypeBlocks(string src, HashSet<int> types)
    {
        var lines = src.Split('\n');
        var starts = new List<(int line, int type)>();
        for (int i = 0; i < lines.Length; i++)
        {
            var m = Regex.Match(lines[i], @"^if \(type == (\d+)\)\s*$");
            if (m.Success) starts.Add((i, int.Parse(m.Groups[1].Value)));
        }
        if (starts.Count == 0) return src;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < starts[0].line; i++) sb.Append(lines[i]).Append('\n');
        string rest = string.Join("\n", lines.Skip(starts[0].line));
        int pos = 0;
        while (pos < rest.Length)
        {
            var m = Regex.Match(rest.Substring(pos), @"(?m)^if \(type == (\d+)\)\s*\n\{");
            if (!m.Success) break;
            int at = pos + m.Index;
            int brace = rest.IndexOf('{', at);
            int end = MatchBrace(rest, brace);
            if (end < 0) break;
            if (types.Contains(int.Parse(m.Groups[1].Value)))
                sb.Append(rest, at, end - at + 1).Append("\n\n");
            pos = end + 1;
        }
        foreach (Match e in EnumDecl.Matches(src)) sb.Append(e.Value).Append('\n');
        return sb.ToString();
    }

    // index of the brace closing the one at 'open' (skips strings and comments)
    static int MatchBrace(string s, int open)
    {
        int depth = 0;
        for (int i = open; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '"')
            {
                for (i++; i < s.Length && s[i] != '"'; i++) if (s[i] == '\\') i++;
                continue;
            }
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '/') { while (i < s.Length && s[i] != '\n') i++; continue; }
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '*') { int e = s.IndexOf("*/", i + 2); i = e < 0 ? s.Length : e + 1; continue; }
            if (c == '{') depth++;
            else if (c == '}') { depth--; if (depth == 0) return i; }
        }
        return -1;
    }

    // ---- sprites: every frame is re-packed onto new texture pages in Chapter 2 ----
    sealed class PagePacker
    {
        const int Max = 2048;
        readonly UndertaleData d2;
        readonly List<(IMagickImage<byte> img, UndertaleTexturePageItem tpi)> pending = new();
        int x, y, rowH, w, h;
        public PagePacker(UndertaleData d) { d2 = d; }
        public void Add(IMagickImage<byte> img, UndertaleTexturePageItem tpi)
        {
            int iw = (int)img.Width + 2, ih = (int)img.Height + 2;
            if (x + iw > Max) { x = 0; y += rowH; rowH = 0; }
            if (y + ih > Max) { Flush(); }
            tpi.SourceX = (ushort)(x + 1);
            tpi.SourceY = (ushort)(y + 1);
            pending.Add((img, tpi));
            x += iw;
            rowH = Math.Max(rowH, ih);
            w = Math.Max(w, x);
            h = Math.Max(h, y + rowH);
        }
        public void Flush()
        {
            if (pending.Count == 0) return;
            int pw = Pow2(w), ph = Pow2(h);
            using var page = new MagickImage(MagickColors.Transparent, (uint)pw, (uint)ph);
            foreach (var (img, tpi) in pending)
            {
                page.Composite(img, tpi.SourceX, tpi.SourceY, CompositeOperator.Copy);
                img.Dispose();
            }
            var tex = new UndertaleEmbeddedTexture();
            var like = d2.EmbeddedTextures.FirstOrDefault();
            if (like != null) { tex.Scaled = like.Scaled; tex.GeneratedMips = like.GeneratedMips; }
            tex.Name = d2.Strings.MakeString("Texture " + d2.EmbeddedTextures.Count);
            tex.TextureWidth = pw;
            tex.TextureHeight = ph;
            tex.TextureData = new UndertaleEmbeddedTexture.TexData { Image = GMImage.FromMagickImage(page).ConvertToPng() };
            d2.EmbeddedTextures.Add(tex);
            foreach (var (_, tpi) in pending) tpi.TexturePage = tex;
            pending.Clear();
            x = y = rowH = w = h = 0;
        }
        static int Pow2(int v) { int p = 1; while (p < v) p <<= 1; return Math.Max(p, 1); }
    }

    static bool ImportSprite(UndertaleData d2, UndertaleSprite src, PagePacker pack, List<string> warnings)
    {
        try
        {
            var spr = new UndertaleSprite
            {
                Name = d2.Strings.MakeString(src.Name.Content),
                Width = src.Width, Height = src.Height,
                MarginLeft = src.MarginLeft, MarginRight = src.MarginRight, MarginTop = src.MarginTop, MarginBottom = src.MarginBottom,
                Transparent = src.Transparent, Smooth = src.Smooth, Preload = src.Preload,
                BBoxMode = src.BBoxMode, SepMasks = src.SepMasks,
                OriginX = src.OriginX, OriginY = src.OriginY,
                SVersion = src.SVersion, SSpriteType = src.SSpriteType,
                GMS2PlaybackSpeed = src.GMS2PlaybackSpeed, GMS2PlaybackSpeedType = src.GMS2PlaybackSpeedType,
                IsSpecialType = src.IsSpecialType,
            };
            foreach (var m in src.CollisionMasks)
                spr.CollisionMasks.Add(new UndertaleSprite.MaskEntry((byte[])m.Data.Clone(), m.Width, m.Height));
            using var tw = new TextureWorker();
            foreach (var te in src.Textures)
            {
                var t3 = te.Texture;
                if (t3 == null) { spr.Textures.Add(new UndertaleSprite.TextureEntry()); continue; }
                var tpi = new UndertaleTexturePageItem
                {
                    Name = d2.Strings.MakeString("PageItem " + d2.TexturePageItems.Count),
                    SourceWidth = t3.SourceWidth, SourceHeight = t3.SourceHeight,
                    TargetX = t3.TargetX, TargetY = t3.TargetY, TargetWidth = t3.TargetWidth, TargetHeight = t3.TargetHeight,
                    BoundingWidth = t3.BoundingWidth, BoundingHeight = t3.BoundingHeight,
                };
                var img = tw.GetTextureFor(t3, src.Name.Content, false);
                var copy = new MagickImage(img);
                pack.Add(copy, tpi);
                d2.TexturePageItems.Add(tpi);
                spr.Textures.Add(new UndertaleSprite.TextureEntry { Texture = tpi });
            }
            d2.Sprites.Add(spr);
            return true;
        }
        catch (Exception ex) { warnings.Add($"transplant: sprite {src.Name.Content}: {ex.Message}"); return false; }
    }

    static bool ImportSound(UndertaleData d2, UndertaleData d3, UndertaleSound src, string ch3Dir, TransplantResult res, List<string> warnings)
    {
        try
        {
            byte[] bytes = src.AudioFile?.Data;
            if (bytes == null && src.AudioGroup != null && src.AudioID >= 0)
            {
                string ag = Path.Combine(ch3Dir, $"audiogroup{d3.AudioGroups.IndexOf(src.AudioGroup)}.dat");
                if (File.Exists(ag))
                {
                    using var fs = File.OpenRead(ag);
                    var agd = UndertaleIO.Read(fs, (w, b) => { }, m => { });
                    bytes = agd.EmbeddedAudio[src.AudioID].Data;
                }
            }
            var snd = new UndertaleSound
            {
                Name = d2.Strings.MakeString(src.Name.Content),
                Flags = src.Flags,
                Type = src.Type == null ? null : d2.Strings.MakeString(src.Type.Content),
                File = src.File == null ? null : d2.Strings.MakeString(src.File.Content),
                Effects = src.Effects, Volume = src.Volume, Preload = src.Preload, Pitch = src.Pitch,
                AudioGroup = d2.AudioGroups.FirstOrDefault(),
                GroupID = 0,
                AudioID = -1,
            };
            if (bytes != null)
            {
                var ea = new UndertaleEmbeddedAudio { Data = bytes };
                d2.EmbeddedAudio.Add(ea);
                snd.AudioFile = ea;
                snd.AudioID = d2.EmbeddedAudio.Count - 1;
                snd.Flags |= UndertaleSound.AudioEntryFlags.IsEmbedded;
            }
            else
            {
                // a streamed file next to Chapter 3's data.win
                string fname = src.File?.Content ?? src.Name.Content;
                if (!fname.Contains('.')) fname += ".ogg";
                string from = Path.Combine(ch3Dir, fname);
                if (!File.Exists(from)) { warnings.Add($"transplant: sound {src.Name.Content} has no data"); return false; }
                res.Files.Add((from, fname));
            }
            d2.Sounds.Add(snd);
            return true;
        }
        catch (Exception ex) { warnings.Add($"transplant: sound {src.Name.Content}: {ex.Message}"); return false; }
    }
}
