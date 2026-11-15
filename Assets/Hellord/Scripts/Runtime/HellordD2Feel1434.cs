// t1434: Diablo 2 style hit / death feel. Everything is behind HellordTuning.d2feel1434 (+ per-feature toggles).
// This file: asset lookups (all optional, null = fall back), corpse pool, blood one-shots, and the HellordGame partial that holds the logic.
// Asset names and folders: Docs/d2feel_assets.md
using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public sealed class D2Corpse1434 { public SpriteRenderer sr, pool, decal; public Sprite[] decalFr; public int key = -1; public float t, life, fade; public bool forced; public float a0 = 1f, poolA0 = 0.85f; }

    public static class D2Feel1434
    {
        const int OrdCorpse = -15000, OrdPool = -15100, OrdSplat = -15050; // ground decals: above the floor (-29xxx), under every actor (about -432 +- 1000)
        static readonly string[] Dirs = { "south", "east", "north", "west" }; // = HArt walk rows 0,4,8,12
        static Transform _root;
        static readonly List<D2Corpse1434> _c = new List<D2Corpse1434>(80);
        static readonly Stack<D2Corpse1434> _free = new Stack<D2Corpse1434>();
        sealed class Shot { public SpriteRenderer sr; public Sprite[] fr; public float t, fps, hold, fade; public int holdOrder; public bool held; }
        static readonly List<Shot> _shots = new List<Shot>(32); static readonly Stack<Shot> _shotFree = new Stack<Shot>();
        static readonly Dictionary<string, Sprite[][]> _stag = new Dictionary<string, Sprite[][]>();
        static readonly Dictionary<string, Sprite[]> _corpse = new Dictionary<string, Sprite[]>(); static readonly Dictionary<string, Sprite[][]> _death = new Dictionary<string, Sprite[][]>(); static readonly Dictionary<string, Sprite> _pool = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Sprite[]> _blood = new Dictionary<string, Sprite[]>();
        static Sprite _phPool;
        static readonly List<D2Corpse1434> _st = new List<D2Corpse1434>(80); // t1447: landed-blood sprites kept on the ground (persistent mode)
        static int _curKey = -1; public static int CurKey => _curKey; public static int PersistCap = 400; // t1447 d2CorpseCap1447
        public static int CorpseCount => _c.Count; public static int StaticCount => _st.Count;
        /// <summary>t1447: corpses stay for the whole floor. Key = FloorIdx * 1000 + room index (-1 outside floor mode). Entering a room shows its corpses and hides the others; entries of other floors are dropped.</summary>
        public static void SetRoom(int key)
        {
            if (_root != null)
            {
                int fl = key / 1000; bool floor = key >= 0;
                for (int i = _c.Count - 1; i >= 0; i--) { var c = _c[i]; if (floor && c.key >= 0 && c.key / 1000 != fl || c.key < 0 && floor) { Release(i); continue; } c.sr.gameObject.SetActive(c.key == key); }
                for (int i = _st.Count - 1; i >= 0; i--) { var c = _st[i]; if (floor && c.key >= 0 && c.key / 1000 != fl || c.key < 0 && floor) { _st[i].sr.gameObject.SetActive(false); _free.Push(_st[i]); _st.RemoveAt(i); continue; } c.sr.gameObject.SetActive(c.key == key); }
                foreach (var s in _shots) { s.sr.gameObject.SetActive(false); _shotFree.Push(s); } _shots.Clear();
            }
            _curKey = key;
        }
        static void AddStatic(Sprite sp, Vector3 pos, bool flipX, int order)
        {
            if (!Ready()) return; while (_st.Count >= Mathf.Max(1, PersistCap)) { var o = _st[0]; _st.RemoveAt(0); o.sr.gameObject.SetActive(false); _free.Push(o); } // oldest removed quietly
            D2Corpse1434 c = _free.Count > 0 ? _free.Pop() : null;
            if (c == null) { var g = new GameObject("corpse"); g.transform.SetParent(_root, false); var pg = new GameObject("pool"); pg.transform.SetParent(g.transform, false); var dg = new GameObject("decal"); dg.transform.SetParent(g.transform, false); c = new D2Corpse1434 { sr = g.AddComponent<SpriteRenderer>(), pool = pg.AddComponent<SpriteRenderer>(), decal = dg.AddComponent<SpriteRenderer>() }; }
            c.key = _curKey; c.decalFr = null; c.pool.enabled = false; c.decal.enabled = false; c.forced = false; c.life = float.PositiveInfinity; c.t = 0;
            c.sr.sprite = sp; c.sr.enabled = true; c.sr.flipX = flipX; c.sr.color = Color.white; c.sr.sortingOrder = order; c.sr.transform.position = pos; c.sr.transform.localScale = Vector3.one; c.sr.gameObject.SetActive(true); _st.Add(c);
        }
        public static bool Verbose; public static int Spawned, Faded, ShotsPlayed; // play-check counters

        public static string Key(string sheet) => sheet.Replace("PLACEHOLDER_", "").Replace("enemy_", "");

        static bool Ready() { if (_root == null) { _root = new GameObject("D2Feel1434").transform; _c.Clear(); _free.Clear(); _shots.Clear(); _shotFree.Clear(); } return true; }

        static Texture2D Load(string rel) { var t = Resources.Load<Texture2D>("HellordArt/" + rel); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        /// <summary>Horizontal strip of `count` equal frames, pivot = bottom-centre (x = floor(w/2), 1 px above the bottom): the art team's convention for stagger / death / corpse (= the enemy sheet pivot for the standing sprites).</summary>
        static Sprite[] Strip(string rel, int count, bool pl = false)
        {
            var t = Load(rel); if (t == null) return null; int n = Mathf.Max(1, count), w = t.width / n; var r = new Sprite[n]; var piv = pl ? PlPivot(rel, w, t.height) : new Vector2(Mathf.Floor(w / 2f) / w, 1f / t.height);
            if (t.width % n != 0 || w < 2) return null;
            for (int i = 0; i < n; i++) r[i] = Sprite.Create(t, new Rect(i * w, 0, w, t.height), piv, HArt.PPU, 0, SpriteMeshType.FullRect);
            return r;
        }
        // t1446 PixelLab strips: pivot x = w/2 except the corpses whose trimmed images carry the death-strip pivot (art notes): brute E 21.5, N 22.5, W 24.5 (S = w/2)
        static Vector2 PlPivot(string rel, int w, int h)
        {   // t1454: batch 1 / 2 corpses carry a measured pivot; other strips: x = w/2, y = the sheet's ground row (1 px above the bottom, 5 for the 38x32 cells, 4 for the wheel)
            int a = rel.LastIndexOf('/') + 1; string nm = rel.Substring(a); int ci = nm.IndexOf("_corpse_"); int si = nm.IndexOf("_death_") >= 0 ? nm.IndexOf("_death_") : nm.IndexOf("_hit_");
            if (ci > 0 && D2Pl1454.TryCorpse(rel, out var cp)) return new Vector2(cp.x / w, (h - cp.y) / h);
            string key = ci > 0 ? nm.Substring(0, ci) : si > 0 ? nm.Substring(0, si) : ""; return new Vector2(PlPivotX(rel, w) / w, D2Pl1454.GroundPx(key) / h);
        }
        static float PlPivotX(string rel, int w) { if (rel.StartsWith("Enemies/brute_corpse_east")) return 21.5f; if (rel.StartsWith("Enemies/brute_corpse_north")) return 22.5f; if (rel.StartsWith("Enemies/brute_corpse_west")) return 24.5f; return w / 2f; }
        static Sprite[][] Dir4Pl(string sheet, string kind, int count)
        {
            string k = Key(sheet); var d = new Sprite[4][]; int any = -1;
            for (int i = 0; i < 4; i++) { d[i] = Strip("Enemies/" + k + "_" + kind + "_" + Dirs[i] + "_pl", count, true); if (d[i] != null && any < 0) any = i; }
            if (any < 0) return null; if (d[1] != null) { if (d[0] == null) d[0] = d[1]; if (d[2] == null) d[2] = d[1]; } for (int i = 0; i < 4; i++) if (d[i] == null) return null; return d; // t1454: Fire Wheel ships E / W only (side view): S / N use E
        }
        static readonly Dictionary<string, Sprite[][]> _plHit = new Dictionary<string, Sprite[][]>(), _plDeath = new Dictionary<string, Sprite[][]>(), _plCorpse = new Dictionary<string, Sprite[][]>();
        /// <summary>t1446 PixelLab get-hit (4 frames) / death (7 frames, POP = frame 2) / corpse per facing: Enemies/&lt;key&gt;_{hit,death,corpse}_{south,east,north,west}_pl.png. Only imp_v2 + brute ship. null = none.</summary>
        public static Sprite[][] HitPl(string sheet) { if (_plHit.TryGetValue(sheet, out var r)) return r; r = Dir4Pl(sheet, "hit", 4); _plHit[sheet] = r; return r; }
        public static Sprite[][] DeathPl(string sheet) { if (_plDeath.TryGetValue(sheet, out var r)) return r; r = Dir4Pl(sheet, "death", 7); _plDeath[sheet] = r; return r; }
        public static Sprite[][] CorpsePl(string sheet) { if (_plCorpse.TryGetValue(sheet, out var r)) return r; r = Dir4Pl(sheet, "corpse", 1); _plCorpse[sheet] = r; return r; }
        static readonly Dictionary<string, Sprite[][]> _bossDeath = new Dictionary<string, Sprite[][]>(), _bossCorpse = new Dictionary<string, Sprite[][]>();
        static Sprite[][] BossStrips1457(string key, string kind, int count, Dictionary<string, Sprite[][]> cache)
        {   // t1457 batch 3 boss art: [0] = Enemies/<key>_<kind>_south (Horse-Face: southeast) strip, [1] = ..._east strip (Qilin only, else null). null = no art.
            if (cache.TryGetValue(key, out var r)) return r; var a = Strip("Enemies/" + key + "_" + kind + "_" + (key == "boss_horseface_pestle" ? "southeast" : "south") + "_pl", count, true);
            r = a == null ? null : new[] { a, Strip("Enemies/" + key + "_" + kind + "_east_pl", count, true) }; cache[key] = r; return r;
        }
        public static Sprite[][] BossDeathPl(string key) => BossStrips1457(key, "death", 7, _bossDeath);
        public static Sprite[][] BossCorpsePl(string key) => BossStrips1457(key, "corpse", 1, _bossCorpse);
        /// <summary>Give an already-spawned corpse object its body (PL: the pool/decal appears at the slam, the body when the death anim ends).</summary>
        public static void SetBody(D2Corpse1434 c, Sprite sp, Vector3 pos, Vector3 scale, Color tint, int ysort, bool flip = false)
        {
            if (c == null || sp == null) return; c.sr.sprite = sp; c.sr.enabled = true; c.sr.color = tint; c.a0 = tint.a; c.sr.sortingOrder = OrdCorpse + ysort; c.sr.transform.position = pos; c.sr.transform.localScale = scale; c.sr.flipX = flip;
        }
        /// <summary>Per-facing set [S,E,N,W] of strips: Enemies/&lt;key&gt;_&lt;kind&gt;_{south,east,north,west}.png; falls back to one file Enemies/&lt;key&gt;_&lt;kind&gt;.png for all four. null = none.</summary>
        static Sprite[][] Dir4(string sheet, string kind, int count)
        {
            string k = Key(sheet); var d = new Sprite[4][]; int any = -1;
            for (int i = 0; i < 4; i++) { d[i] = Strip("Enemies/" + k + "_" + kind + "_" + Dirs[i], count); if (d[i] != null && any < 0) any = i; }
            if (any < 0) { var one = Strip("Enemies/" + k + "_" + kind, count); if (one != null) { for (int i = 0; i < 4; i++) d[i] = one; any = 0; } }
            if (any < 0) return null; for (int i = 0; i < 4; i++) if (d[i] == null) d[i] = d[any]; return d;
        }

        /// <summary>Stagger frames per facing [S,E,N,W] (HArt walk row / 4). Files: Enemies/&lt;key&gt;_stagger_{south,east,north,west}.png, or one Enemies/&lt;key&gt;_stagger.png used for all four. Cell size = the enemy sheet's.</summary>
        public static Sprite[][] Stagger(string sheet) { if (_stag.TryGetValue(sheet, out var r)) return r; r = Dir4(sheet, "stagger", 2); _stag[sheet] = r; return r; }
        /// <summary>Fast collapse (3 frames @12 fps) per facing, ending on the corpse pose: Enemies/&lt;key&gt;_death_{south,east,north,west}.png</summary>
        public static Sprite[][] Death(string sheet) { if (_death.TryGetValue(sheet, out var r)) return r; r = Dir4(sheet, "death", 3); _death[sheet] = r; return r; }
        /// <summary>Lying body per facing: Enemies/&lt;key&gt;_corpse_{south,east,north,west}.png, or one Enemies/&lt;key&gt;_corpse.png (blobs). Any size, bottom-centre pivot.</summary>
        public static Sprite[] Corpse(string sheet)
        {
            if (_corpse.TryGetValue(sheet, out var r)) return r; var d = Dir4(sheet, "corpse", 1); r = null;
            if (d != null) { r = new Sprite[4]; for (int i = 0; i < 4; i++) r[i] = d[i][0]; }
            _corpse[sheet] = r; return r;
        }
        /// <summary>Per-enemy pool BloodFX/pool_&lt;key&gt;.png, else the shared BloodFX/blood_pool_m (big bodies) / blood_pool_s. Centre pivot.</summary>
        public static Sprite Pool(string sheet, bool big)
        {
            string ck = sheet + (big ? "|m" : "|s"); if (_pool.TryGetValue(ck, out var s)) return s; s = null; string k = Key(sheet);
            var t = (D2Pl1454.IsGhost(k) ? Load("BloodFX/ectoplasm_pool_s") : null) ?? Load("BloodFX/pool_" + k) ?? Load("Enemies/" + k + "_pool") ?? Load("BloodFX/blood_pool_" + (big ? "m" : "s"));
            if (t != null) s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), HArt.PPU, 0, SpriteMeshType.FullRect);
            _pool[ck] = s; return s;
        }
        static Sprite PlaceholderPool()
        {
            if (_phPool != null) return _phPool; const int W = 20, H = 10; var t = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                float dx = (x + 0.5f - W / 2f) / (W / 2f), dy = (y + 0.5f - H / 2f) / (H / 2f), r = dx * dx + dy * dy; float j = 0.12f * Mathf.Sin(x * 2.3f + y * 1.7f);
                t.SetPixel(x, y, r + j < 0.85f ? new Color(0.30f, 0.03f, 0.05f, 1f) : new Color(0, 0, 0, 0));
            }
            t.Apply(); return _phPool = Sprite.Create(t, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), HArt.PPU, 0, SpriteMeshType.FullRect);
        }
        // ---- Hellord FX t1435 strips (Resources/HellordArt/BloodFX/<name>_strip.png): frame w, h, count, pivot px from top-left (chest pivots; feet = pivot y + 18) ----
        static readonly Dictionary<string, int[]> FxDefs = new Dictionary<string, int[]>
        {
            { "blood_hit_v1", new[] { 24, 20, 8, 6, 10 } }, { "blood_hit_v2", new[] { 24, 20, 8, 6, 10 } }, { "blood_hit_v3", new[] { 24, 20, 8, 6, 10 } },
            { "hit_spark", new[] { 22, 22, 6, 6, 11 } },
            { "blood_death_v1", new[] { 56, 44, 12, 20, 18 } }, { "blood_death_v2", new[] { 56, 44, 12, 20, 18 } }, { "blood_death_v3", new[] { 56, 44, 12, 20, 18 } },
            { "blood_decal_v1", new[] { 40, 20, 6, 20, 10 } }, { "blood_decal_v2", new[] { 40, 20, 6, 20, 10 } }, { "blood_decal_v3", new[] { 40, 20, 6, 20, 10 } },
            { "frozen_shatter2", new[] { 72, 72, 21, 36, 36 } }, { "gib_burst", new[] { 72, 56, 14, 36, 26 } }, { "burn_death", new[] { 48, 56, 16, 24, 36 } },
        };
        public const float FxFps = 30f;
        public static Sprite[] Fx(string name)
        {
            if (_blood.TryGetValue(name, out var r)) return r; r = null;
            if (FxDefs.TryGetValue(name, out var d)) { var t = Load("BloodFX/" + name + "_strip"); if (t != null && t.width == d[0] * d[2] && t.height == d[1]) { r = new Sprite[d[2]]; var piv = new Vector2(d[3] / (float)d[0], (d[1] - d[4]) / (float)d[1]); for (int i = 0; i < d[2]; i++) r[i] = Sprite.Create(t, new Rect(i * d[0], 0, d[0], d[1]), piv, HArt.PPU, 0, SpriteMeshType.FullRect); } else if (t != null) Debug.LogWarning("[D2Feel1434] " + name + " has an unexpected size " + t.width + "x" + t.height); }
            _blood[name] = r; return r;
        }
        public static Sprite[] Variant(string baseName) => Fx(baseName + "_v" + Random.Range(1, 4)) ?? Fx(baseName + "_v1");
        public static Sprite[] BloodHit() => Variant("blood_hit");
        public static Sprite[] BloodDeath() => Variant("blood_death");
        public static bool HasFx(string name) => Fx(name) != null;

        /// <summary>One-shot sprite animation. hold &gt; 0 = keep the last frame on the ground (holdOrder) for that many seconds, fading over `fade`. flipX mirrors about the pivot (base art points east).</summary>
        public static void OneShot(Sprite[] fr, Vector3 pos, bool flipX, int order, float fps, float hold = 0f, int holdOrder = 0, float fade = 1f)
        {
            if (fr == null || fr.Length == 0 || !Ready()) return; int act = 0, held = 0; for (int i = 0; i < _shots.Count; i++) if (_shots[i].hold > 0) held++; else act++;
            if (hold > 0 ? held >= 90 : act >= 40) return; // caps
            Shot s = _shotFree.Count > 0 ? _shotFree.Pop() : null;
            if (s == null) { var go = new GameObject("fx"); go.transform.SetParent(_root, false); s = new Shot { sr = go.AddComponent<SpriteRenderer>() }; }
            s.fr = fr; s.t = 0; s.fps = Mathf.Max(1f, fps); s.hold = hold; s.holdOrder = holdOrder; s.fade = fade; s.held = false; s.sr.sprite = fr[0]; s.sr.flipX = flipX; s.sr.sortingOrder = order; s.sr.color = Color.white; s.sr.transform.position = pos; s.sr.transform.localScale = Vector3.one; s.sr.gameObject.SetActive(true); _shots.Add(s); ShotsPlayed++;
        }

        /// <summary>Corpse + ground decals. sp may be null (gib: decal only).</summary>
        public static D2Corpse1434 Spawn(Sprite sp, Vector3 pos, Vector3 scale, Color tint, int ysort, Sprite pool, Sprite[] decal, float life, float fade, int cap, bool flip = false)
        {
            if ((sp == null && pool == null && decal == null) || !Ready()) return null;
            if (float.IsInfinity(life))
            {   // persistent: only the oldest beyond the cap go, quietly (a short fade if still visible in this room)
                int unforced = 0; for (int k = 0; k < _c.Count; k++) if (!_c[k].forced) unforced++; int over = unforced - Mathf.Max(1, PersistCap) + 1; int i0 = 0; // entries already fading out do not count against the cap
                while (over > 0 && i0 < _c.Count) { var o = _c[i0]; if (o.forced) { i0++; continue; } if (o.key != _curKey) { Release(i0); } else { o.forced = true; o.life = o.t + 0.4f; o.fade = 0.4f; i0++; } over--; }
            }
            else if (_c.Count >= cap) { for (int i = 0; i < _c.Count; i++) if (!_c[i].forced) { var o = _c[i]; o.forced = true; o.life = o.t + 0.4f; o.fade = 0.4f; break; } } // oldest fades first
            if (_c.Count >= (float.IsInfinity(life) ? PersistCap : cap) + 16) Release(0);
            D2Corpse1434 c = _free.Count > 0 ? _free.Pop() : null;
            if (c == null) { var g = new GameObject("corpse"); g.transform.SetParent(_root, false); var pg = new GameObject("pool"); pg.transform.SetParent(g.transform, false); var dg = new GameObject("decal"); dg.transform.SetParent(g.transform, false); c = new D2Corpse1434 { sr = g.AddComponent<SpriteRenderer>(), pool = pg.AddComponent<SpriteRenderer>(), decal = dg.AddComponent<SpriteRenderer>() }; }
            c.t = 0; c.life = life; c.fade = Mathf.Min(fade, life); c.forced = false; c.a0 = tint.a; c.decalFr = decal; c.key = _curKey;
            c.sr.sprite = sp; c.sr.enabled = sp != null; c.sr.color = tint; c.sr.flipX = flip; c.sr.sortingOrder = OrdCorpse + ysort; c.sr.transform.position = pos; c.sr.transform.localScale = scale; c.sr.gameObject.SetActive(true);
            c.pool.transform.localPosition = new Vector3(0f, 3f / HArt.PPU, 0f); c.pool.transform.localScale = Vector3.one; // art note: pool centre ~3 px above the feet point
            if (pool != null) { c.pool.sprite = pool; c.pool.color = new Color(1, 1, 1, c.poolA0); c.pool.sortingOrder = OrdPool + ysort; c.pool.enabled = true; } else c.pool.enabled = false;
            c.decal.transform.localPosition = Vector3.zero; c.decal.transform.localScale = Vector3.one;
            if (decal != null) { c.decal.sprite = decal[0]; c.decal.color = Color.white; c.decal.sortingOrder = OrdPool + ysort - 1; c.decal.enabled = true; } else c.decal.enabled = false;
            _c.Add(c); Spawned++; return c;
        }
        public static Sprite PoolOrPlaceholder(string sheet, bool big, bool placeholders) => Pool(sheet, big) ?? (placeholders ? PlaceholderPool() : null);
        static void Release(int i) { var c = _c[i]; _c.RemoveAt(i); c.sr.gameObject.SetActive(false); _free.Push(c); Faded++; }

        public static void Tick(float dt)
        {
            if (_root == null) return;
            for (int i = _c.Count - 1; i >= 0; i--)
            {
                var c = _c[i]; c.t += dt; float rem = c.life - c.t; if (rem <= 0) { Release(i); continue; }
                bool fading = rem < c.fade; float a = fading ? rem / c.fade : 1f;
                if (fading) { var col = c.sr.color; col.a = c.a0 * a; c.sr.color = col; var pc = c.pool.color; pc.a = c.poolA0 * a; c.pool.color = pc; }
                if (c.decalFr != null)
                {   // decal stages (art note): 0 fresh -> 1 dried after 1.5 s -> when the corpse fades 2,3,4,5 in equal steps (0.25 s at the normal 1 s fade), no loop
                    int st = fading ? Mathf.Min(5, 2 + (int)((c.fade - rem) / (c.fade * 0.25f))) : c.t > 1.5f ? 1 : 0; var sp = c.decalFr[Mathf.Min(st, c.decalFr.Length - 1)]; if (c.decal.sprite != sp) c.decal.sprite = sp; if (st == 1 && float.IsInfinity(c.life)) c.decalFr = null; // persistent decals stop at the dried stage
                }
            }
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                var s = _shots[i]; s.t += dt; float total = s.fr.Length / s.fps; int k = (int)(s.t * s.fps);
                if (k < s.fr.Length) { s.sr.sprite = s.fr[k]; continue; }
                if (float.IsInfinity(s.hold)) { AddStatic(s.fr[s.fr.Length - 1], s.sr.transform.position, s.sr.flipX, s.holdOrder); s.sr.gameObject.SetActive(false); _shots.RemoveAt(i); _shotFree.Push(s); continue; } // persistent: landed blood stays on the floor
                if (s.hold <= 0 || s.t >= total + s.hold) { s.sr.gameObject.SetActive(false); _shots.RemoveAt(i); _shotFree.Push(s); continue; }
                if (!s.held) { s.held = true; s.sr.sprite = s.fr[s.fr.Length - 1]; s.sr.sortingOrder = s.holdOrder; }
                float rem = total + s.hold - s.t; if (rem < s.fade) { var col = s.sr.color; col.a = rem / s.fade; s.sr.color = col; }
            }
        }
        public static void Clear()
        {
            if (_root == null) return;
            for (int i = _c.Count - 1; i >= 0; i--) { _c[i].sr.gameObject.SetActive(false); _free.Push(_c[i]); } _c.Clear();
            for (int i = _st.Count - 1; i >= 0; i--) { _st[i].sr.gameObject.SetActive(false); _free.Push(_st[i]); } _st.Clear();
            foreach (var s in _shots) { s.sr.gameObject.SetActive(false); _shotFree.Push(s); } _shots.Clear();
        }
    }

    public partial class HellordGame
    {
        float _d2flash0 = -1; Vector2 _d2Dir; bool _d2DirSet, _d2Fire;
        static readonly Color D2Statue = new Color(0.62f, 0.86f, 1f, 1f);

        void D2Tick1434(float dt)
        {
            var T = HellordTuning.I; var fx = HordeFX.I;
            if (fx != null) { if (_d2flash0 < 0) _d2flash0 = fx.flashSeconds; fx.flashSeconds = T.d2feel1434 && T.d2Flash1434 ? T.d2FlashS1434 : _d2flash0; }
            D2Feel1434.PersistCap = T.d2CorpseCap1447; D2Feel1434.Tick(dt);
        }
        void D2ClearCorpses1434() { D2Feel1434.Clear(); }
        static bool D2Persist1447() { var T = HellordTuning.I; return T.d2feel1434 && T.d2CorpsePersist1447; }
        void D2SetDir1434(Vector2 knock, SkillInst s) { _d2DirSet = knock.sqrMagnitude > 0.0004f; if (_d2DirSet) _d2Dir = knock.normalized; _d2Fire = s != null && s.def != null && (s.def.innate == Status.Burn || s.Has(SupportId.Burn)); }
        void D2Reset1434(Enemy e) { e.pop1434 = e.slam1434 = false; e.cp1434 = null; e.fxFr1434 = null; e.stag1434 = e.stagDur1434 = e.stagCd1434 = 0; e.kbDist1434 = 0; e.over1434 = false; e.frozenDeath1434 = false; e.d2scaled1434 = false; e.gib1434 = e.burn1434 = e.fx1434 = e.fxFrozen1434 = false; e.corpseLife1434 = 0; }
        Vector2 D2DirOf(Enemy e) { if (_d2DirSet) return _d2Dir; Vector2 d = e.pos - _heroPos; return d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.right; }
        /// <summary>PixelLab art key of this enemy ("enemy_&lt;key&gt;" like the sheets): the own-sheet demons (Frost Caster, Fire Wheel, Mortar Bloater / Rice Ghost / Grinder Monk) and the Frost Brute have their own strips (t1454); everything else = its ETypes sheet.</summary>
        static string D2Sheet(Enemy e)
        {
            if (e.m53 == 1) return "enemy_bloater"; if (e.m53 == 2) return "enemy_rice_ghost"; if (e.m53 == 3) return "enemy_grinder_monk";
            if (e.variant == 3) return "enemy_frost_caster"; if (e.variant == 4) return "enemy_fire_wheel";
            string v = null, fb = null;
            switch (e.k49)
            {   // t1457 batch 3: the range-kit recolours / Archer have their own strips, matching the colour of the standing sprite
                case 1: v = "enemy_imp_v2_k1"; break; case 2: v = "enemy_imp_v2_k2"; break; case 3: v = "enemy_imp_v2_k3"; break; case 7: v = "enemy_imp_v2_k7"; break;
                case 4: v = "enemy_wraith_v2_k4"; fb = "enemy_wraith_v2"; break; case 5: v = "enemy_wraith_v2_k5"; fb = "enemy_wraith_v2"; break; case 6: return "enemy_frost_brute"; case 8: v = "enemy_archer"; break;
                default: v = VarSheet1457(e.type, e.variant); break;
            }
            if (v != null && D2Feel1434.DeathPl(v) != null) return v; return fb ?? EnemyArtMap.Map[e.type].sheet;
        }
        /// <summary>t1457: variant key lookup (e.variant 1 frost demon / 2 bomber): enemy_&lt;base&gt;_var1 / _var2 for the types the game recolours. null = base art.</summary>
        static string VarSheet1457(EType t, int variant)
        {
            if (variant == 1) { switch (t) { case EType.Imp: return "enemy_imp_v2_var1"; case EType.Ghoul: return "enemy_shambler_v2_var1"; case EType.Brute: return "enemy_brute_var1"; case EType.Splitter: return "enemy_splitter_var1"; } }
            else if (variant == 2) { switch (t) { case EType.Imp: return "enemy_imp_v2_var2"; case EType.Ghoul: return "enemy_shambler_v2_var2"; } }
            return null;
        }
        static string D2SheetHA(Enemy e) => EnemyArtMap.Map[e.type].sheet; // the t1434 HA art lookups stay on the ETypes sheet
        /// <summary>Facing 0 S / 1 E / 2 N / 3 W for the PixelLab strips. Frost Caster and Fire Wheel keep their own facing state.</summary>
        static int D2Dir4(Enemy e)
        {
            if (e.variant == 3) return e.v44Face == 1 ? (e.v44West ? 3 : 1) : (e.v44Face & 3); if (e.variant == 4) return e.v44West ? 3 : 1;
            return (e.walkRow >> 2) & 3;
        }

        bool D2Impact1434()
        {
            var T = HellordTuning.I; if (!(T.d2feel1434 && T.d2Hooks1434) || !HellordAudio.Has("impact_flesh")) return false;
            HellordAudio.Play("impact_flesh"); return true;
        }
        static string D2PainHook(EType t)
        {
            switch (t) { case EType.Imp: case EType.GossipImp: return "pain_imp"; case EType.Ghoul: case EType.Splitter: case EType.SplitChild: return "pain_ghoul"; case EType.Brute: case EType.HookThrower: return "pain_brute"; default: return "pain_wraith"; }
        }

        /// <summary>Called from Damage() after the HP change, for every non-DoT hit. Stagger + knockback on survivors; overkill slide marker on the killing blow.</summary>
        void D2Hit1434(Enemy e, float dmg, float hpBefore, bool crit)
        {
            var T = HellordTuning.I; if (!T.d2feel1434) return;
            Vector2 dir = D2DirOf(e); Vector3 at = e.pos + Vector2.up * e.hitY;
            if (e.hp <= 0)
            {
                if (T.d2Overkill1434 && !e.boss && e.freezeT <= 0 && hpBefore > 0)
                {
                    float r = dmg / hpBefore; bool over = r >= T.d2OverkillMul1434 || (crit && T.d2OverkillCrit1434);
                    if (over)
                    {
                        float u = Mathf.Clamp01(Mathf.InverseLerp(T.d2OverkillMul1434, T.d2OverkillMul1434 * 3f, r)); float px = Mathf.Lerp(T.d2OverkillPx1434.x, T.d2OverkillPx1434.y, u) * (e.elite ? T.d2KnockElite1434 + 0.4f : 1f);
                        if (T.d2Gib1434 && T.d2Death1434 && T.d2Hooks1434 && !e.elite && r >= T.d2GibMul1434 && D2Feel1434.HasFx("gib_burst")) e.gib1434 = true;
                        e.over1434 = true; e.kbDir1434 = dir; e.kbDist1434 = px / HArt.PPU; e.kbDur1434 = Mathf.Max(0.05f, T.d2OverkillS1434); e.kbT1434 = 0; e.kbDone1434 = 0;
                    }
                }
                return; // the death FX (blood_death / gib) cover the killing blow
            }
            if (T.d2Hooks1434) D2BloodHit1434(e, at, dir);
            if (!T.d2Stagger1434 || e.boss || e.nest || ((e.variant >= 3 || e.m53 > 0 || e.type == EType.HookThrower || e.type == EType.GossipImp) && !(T.d2anim1446 && T.d2Hooks1434 && D2Feel1434.HitPl(D2Sheet(e)) != null)) || (e.variant == 4 && e.v44St != 0) || e.freezeT > 0 || e.stun53 > 0 || e.stagCd1434 > 0) return;
            float trig = e.maxHp / Mathf.Max(1f, T.d2StaggerDiv1434), ratio = dmg / Mathf.Max(0.01f, trig); if (ratio < 1f) return;
            float chance = ratio < 2f ? 0.5f : ratio < 4f ? 0.75f : 1f; if (e.elite) chance *= T.d2StaggerElite1434.x; if (Random.value >= chance) return;
            float dur = Random.Range(T.d2StaggerS1434.x, T.d2StaggerS1434.y) * (e.elite ? T.d2StaggerElite1434.y : 1f);
            if (T.d2anim1446 && T.d2Hooks1434 && D2Feel1434.HitPl(D2Sheet(e)) != null) dur = Mathf.Max(dur, 4f / Mathf.Max(1f, T.d2HitFps1446)); // t1446: let the 4-frame PixelLab hit play out
            e.stag1434 = e.stagDur1434 = dur; e.stagCd1434 = dur + T.d2StaggerCdS1434; e.kbDir1434 = dir; D2Stagger1434Count++;
            if (T.d2Knock1434)
            {
                float px = Mathf.Lerp(T.d2KnockPx1434.x, T.d2KnockPx1434.y, Mathf.Clamp01((ratio - 1f) / 3f)) * (e.elite ? T.d2KnockElite1434 : 1f);
                e.kbDist1434 = px / HArt.PPU; e.kbDur1434 = Mathf.Max(0.05f, T.d2KnockS1434); e.kbT1434 = 0; e.kbDone1434 = 0;
            }
            if (T.d2Hooks1434)
            {
                { string ph = D2PainHook(e.type), p47 = D2Sfx1447() ? D2Hook1457("pain1447_", D2Fam1447(e)) : null; if (p47 != null) HellordAudio.Play(p47); else if (HellordAudio.Has("pain_imp")) HellordAudio.Play(ph); }
                var sparkFr = D2Feel1434.Fx("hit_spark");
                if (sparkFr != null) D2Feel1434.OneShot(sparkFr, at, dir.x < 0, YSort(e.pos.y) + 4, D2Feel1434.FxFps); else if (T.d2Placeholders1434) HordeFX.CheapBurst(at, new Color(0.45f, 0.04f, 0.06f), 3);
            }
        }
        public static int D2Stagger1434Count, D2OverkillCount, D2FrozenCount, D2KnockCount;

        void D2BloodHit1434(Enemy e, Vector3 at, Vector2 dir)
        {
            if (D2Pl1454.IsGhost(D2Feel1434.Key(D2Sheet(e))) && D2Feel1434.HitPl(D2Sheet(e)) != null) return; var fr = D2Feel1434.BloodHit(); if (fr != null) D2Feel1434.OneShot(fr, at, dir.x < 0, YSort(e.pos.y) + 3, HellordTuning.I.d2BloodFps1434);
        }

        void D2StagTick1434(Enemy e, float dt)
        {
            if (e.stag1434 > 0) { e.stag1434 -= dt; if (e.stag1434 <= 0) D2EndStag1434(e); }
            e.stagCd1434 -= dt;
        }
        void D2EndStag1434(Enemy e) { e.stag1434 = 0; if (e.d2scaled1434) { e.tr.localScale = Vector3.one * EnemyArtMap.Map[e.type].scale; e.d2scaled1434 = false; } }

        void D2Knock1434(Enemy e, float dt)
        {
            e.kbT1434 += dt; float u = Mathf.Clamp01(e.kbT1434 / e.kbDur1434), p = 1f - (1f - u) * (1f - u); float tgt = e.kbDist1434 * p; float d = tgt - e.kbDone1434; e.kbDone1434 = tgt;
            e.pos += e.kbDir1434 * d; if (u >= 1f) e.kbDist1434 = 0f;
        }

        /// <summary>Walk-frame replacement while staggered: stagger art if present, else squash. Called after the walk sprite is set.</summary>
        void D2StagVisual1434(Enemy e)
        {
            var T = HellordTuning.I; float prog = 1f - e.stag1434 / Mathf.Max(0.01f, e.stagDur1434);
            var hpl = T.d2anim1446 && T.d2Hooks1434 ? D2Feel1434.HitPl(D2Sheet(e)) : null;
            if (hpl != null) { var ha = hpl[D2Dir4(e)]; e.sr.flipX = false; e.sr.sprite = ha[Mathf.Min(ha.Length - 1, (int)((e.stagDur1434 - e.stag1434) * T.d2HitFps1446))]; return; } // t1446 PixelLab get-hit, 15 fps
            var st = T.d2Hooks1434 ? D2Feel1434.Stagger(D2SheetHA(e)) : null; var arr = st != null ? st[D2Dir4(e)] : null;
            if (arr != null && arr.Length > 0) { e.sr.sprite = arr[Mathf.Min(arr.Length - 1, (int)(prog * arr.Length))]; return; }
            float k = (1f - prog); k *= k; float a = T.d2StaggerRecoil1434.y * k; e.tr.localScale = Vector3.one * EnemyArtMap.Map[e.type].scale * 1f; e.tr.localScale = new Vector3(EnemyArtMap.Map[e.type].scale * (1f + a), EnemyArtMap.Map[e.type].scale * (1f - a), 1f); e.d2scaled1434 = true;
        }
        Vector3 D2StagOff1434(Enemy e)
        {
            float k = e.stag1434 / Mathf.Max(0.01f, e.stagDur1434); k *= k; float px = Mathf.Round(HellordTuning.I.d2StaggerRecoil1434.x * k);
            return new Vector3(e.kbDir1434.x, e.kbDir1434.y, 0) * px / HArt.PPU;
        }

        /// <summary>KillEnemy: a frozen kill defers the shatter and the death anim (flash, statue, then shatter). True = deferred.</summary>
        bool D2FrozenBegin1434(Enemy e)
        {
            var T = HellordTuning.I; if (!(T.d2feel1434 && T.d2Frozen1434) || e.boss || e.nest || e.variant >= 3 || e.m53 > 0) return false;
            e.frozenDeath1434 = true; e.kbDist1434 = 0; e.over1434 = false; D2FrozenCount++;
            var fx = T.d2Hooks1434 ? D2Feel1434.Fx("frozen_shatter2") : null; e.fxFrozen1434 = fx != null; if (fx != null) D2Feel1434.OneShot(fx, e.pos + Vector2.up * e.hitY, false, YSort(e.pos.y) + 4, D2Feel1434.FxFps); // flash 0-1, statue 2-5, hide enemy at 6, shards to 20
            return true;
        }
        /// <summary>t1446: PixelLab death art for this enemy (imp_v2, brute) = the POP sequence replaces the HA collapse.</summary>
        bool D2Pl1446(Enemy e)
        {
            var T = HellordTuning.I; return T.d2feel1434 && T.d2anim1446 && T.d2Death1434 && T.d2Hooks1434 && !e.frozenDeath1434 && !e.boss && !e.nest && D2Feel1434.DeathPl(D2Sheet(e)) != null; // t1454: also the own-sheet demons / Frost Brute (art key from D2Sheet)
        }
        // ================= t1447 D2 per-family voices (toggle d2sfx1447) =================
        /// <summary>Voice family of an enemy / boss (death_/attack_/aggro_ hook suffix).</summary>
        string D2Fam1447(Enemy e)
        {
            if (e.boss) { switch (BossKind44) { case 2: return "horseface"; case 3: return "icegorilla"; case 4: return "qilin"; default: return "oxhead"; } }
            if (e.m53 == 1) return "bloater"; if (e.m53 == 2) return "rice_ghost"; if (e.m53 == 3) return "grinder_monk";
            if (e.variant == 2) return "bloater"; if (e.variant == 3) return "frost_caster"; if (e.variant == 4) return "fire_wheel";
            if (e.k49 == 2 || e.k49 == 5) return "frost_caster"; if (e.k49 == 6) return "brute"; if (e.k49 == 8) return "archer"; if (e.k49 == 4) return "wraith";
            switch (e.type) { case EType.Ghoul: return "shambler"; case EType.Splitter: case EType.SplitChild: return "splitter"; case EType.Brute: return "brute"; case EType.HookThrower: return "hook_thrower"; case EType.GossipImp: return "gossip_imp"; case EType.Elite: return "wraith"; default: return "imp"; } // t1457: phase-2 families map to their enemy types
        }
        /// <summary>Phase-1 family a phase-2 family falls back to when it has no clip of its own.</summary>
        static string D2LegacyFam1447(string fam)
        {
            switch (fam) { case "bloater": case "shambler": case "splitter": return "ghoul"; case "grinder_monk": case "hook_thrower": return "brute"; case "rice_ghost": return "wraith"; case "frost_caster": case "fire_wheel": return "caster"; case "gossip_imp": case "archer": return "imp"; default: return fam; }
        }
        /// <summary>Hook name prefix + family, own clips first, else the legacy family; null = silent.</summary>
        static string D2Hook1457(string prefix, string fam)
        {
            string h = prefix + fam; if (HellordAudio.Has(h)) return h; h = prefix + D2LegacyFam1447(fam); return HellordAudio.Has(h) ? h : null;
        }
        static bool D2Sfx1447() { var T = HellordTuning.I; return T.d2sfx1447 && T.sfxEnabled; }
        /// <summary>Death voice: per-family D2 clip (shared cap 4, overlap allowed) or the t1434 death_scream set when d2sfx1447 is off / the clip is missing.</summary>
        void D2DeathVoice1447(Enemy e)
        {
            if (D2Sfx1447()) { string h = D2Hook1457("death_", D2Fam1447(e)); if (h != null) { HellordAudio.Play(h); return; } }
            HellordAudio.Play("death_scream");
        }
        /// <summary>Attack voice at the start of an attack (tell / wind-up / contact hit). Brute-type voices play 65% of the time so a crowd does not roar on every swing.</summary>
        void D2Atk1447(Enemy e)
        {
            if (!D2Sfx1447() || e == null) return; string fam = D2Fam1447(e);
            if ((fam == "brute" || fam == "grinder_monk" || fam == "hook_thrower") && Random.value > 0.65f) return;
            if (Time.time < e.atkV1457) return; string h = D2Hook1457("attack_", fam); if (h != null) { HellordAudio.Play(h); e.atkV1457 = Time.time + HellordTuning.I.d2AtkVoiceCd1457; } // t1457: per-unit cooldown (a brute swinging every 0.6 s barked every time)
        }
        /// <summary>Idle growl per unit at the per-family interval (Imp 4-6 s, most 6 s, Ghoul / Grinder Monk 12 s, Fire Wheel 8-12 s, bosses 18-28 s). Only enemies near the hero.</summary>
        void D2Aggro1447(Enemy e, float dist, float dt)
        {
            if (!D2Sfx1447() || mode != Mode.Play) return;
            string fam = D2Fam1447(e);
            if (e.agT1447 == 0f) { e.agT1447 = Time.time + 0.5f + Random.value * D2AggroGap1447(fam, e.boss); return; } // first call: stagger the units
            if (Time.time < e.agT1447) return;
            e.agT1447 = Time.time + D2AggroGap1447(fam, e.boss);
            if (dist > (e.boss ? 40f : 16f) || e.freezeT > 0) return;
            string h = D2Hook1457("aggro_", fam); if (h != null) HellordAudio.Play(h);
        }
        static float D2AggroGap1447(string fam, bool boss)
        {
            if (boss) return Random.Range(18f, 28f); switch (fam) { case "imp": return Random.Range(4f, 6f); case "ghoul": case "shambler": case "grinder_monk": return Random.Range(10f, 14f); case "fire_wheel": return Random.Range(8f, 12f); default: return Random.Range(5f, 7f); }
        }
        bool D2DeathSfx1434(Enemy e)
        {
            var T = HellordTuning.I; if (!(T.d2feel1434 && T.d2Hooks1434) || !HellordAudio.Has("death_scream")) return false;
            if (D2Pl1446(e)) return true; // played on the POP instead (D2PopSfx1446); the old enemy_death is replaced
            if (!e.boss) { if (!e.frozenDeath1434) D2DeathVoice1447(e); if (e.over1434 && HellordAudio.Has("gore_splat")) HellordAudio.Play("gore_splat"); }
            return !e.boss;
        }
        void D2PopSfx1446(Enemy e)
        {
            if (!HellordAudio.Has("death_scream")) return; D2DeathVoice1447(e); if (e.over1434 && HellordAudio.Has("gore_splat")) HellordAudio.Play("gore_splat");
        }
        void D2DeathStart1434(Enemy e)
        {
            var T = HellordTuning.I; D2EndStag1434(e); if (e.over1434) D2OverkillCount++; e.fx1434 = false; e.corpseLife1434 = Random.Range(T.d2CorpseLifeS1434.x, T.d2CorpseLifeS1434.y);
            e.pop1434 = e.slam1434 = false; e.cp1434 = null; e.fxFr1434 = null; e.fxHold1434 = false;
            if (!T.d2Hooks1434) return;
            Vector2 dir = D2DirOf(e); e.deathDir1434 = dir; e.fxFlip1434 = dir.x < 0; Sprite[] fr = null; bool hold = false;
            if (e.gib1434) { fr = D2Feel1434.Fx("gib_burst"); hold = true; }
            else if (e.burn1434 && T.d2Burn1434) fr = D2Feel1434.Fx("burn_death");
            if (fr == null && !e.gib1434 && !(D2Pl1446(e) && D2Pl1454.IsGhost(D2Feel1434.Key(D2Sheet(e))))) { fr = D2Feel1434.BloodDeath(); hold = true; } // t1454: ghosts do not bleed
            e.fxFr1434 = fr; e.fx1434 = fr != null; e.fxHold1434 = hold && fr != null;
            if (D2Feel1434.Verbose) Debug.Log($"[D2] death {e.type} gib {e.gib1434} burn {e.burn1434} over {e.over1434} fx {fr != null} pl {D2Pl1446(e)}");
            if (!D2Pl1446(e)) D2FireDeathFx1434(e); // PixelLab deaths fire it on the POP
        }
        void D2FireDeathFx1434(Enemy e)
        {
            if (e.fxFr1434 == null) return; var T = HellordTuning.I; Vector3 chest = e.pos + Vector2.up * e.hitY; int ys = YSort(e.pos.y);
            if (e.fxHold1434) D2Feel1434.OneShot(e.fxFr1434, chest, e.fxFlip1434, ys + (e.gib1434 ? 4 : 3), e.gib1434 ? D2Feel1434.FxFps : T.d2BloodFps1434, D2Persist1447() ? float.PositiveInfinity : e.corpseLife1434 + 0.3f, -15050 + ys);
            else D2Feel1434.OneShot(e.fxFr1434, chest, false, ys + 4, D2Feel1434.FxFps);
        }
        int D2CorpseFrame1434(Enemy e) => e.type == EType.Splitter ? -1 : HArt.EnemyDeathN - 3; // fallback (no art): last solid frame of the old ember death

        /// <summary>Replaces the normal UpdateDeath tail: with art = the 3-frame collapse (12 fps) then the lying corpse; without = the old frames trimmed to d2DeathS. Frozen kills run the statue sequence. True = handled.</summary>
        bool D2Death1434(Enemy e, float dt)
        {
            var T = HellordTuning.I; if (!(T.d2feel1434 && T.d2Death1434)) return false;
            if (e.frozenDeath1434) return D2FrozenDeath1434(e, dt);
            if (D2Pl1446(e)) return D2DeathPl1446(e, dt);
            if (e.gib1434) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); if (e.type == EType.Splitter && !e.splitDone) D2Split1434(e); D2SpawnCorpse1434(e, -2, 0); return true; } // gib: body hidden at frame 0, decal only
            e.deathT += dt;
            if (e.kbDist1434 > 0) { D2Knock1434(e, dt); ClampToRoom(ref e.pos, 6); e.tr.position = new Vector3(Snap(e.pos.x), Snap(e.pos.y), 0); e.sr.sortingOrder = YSort(e.pos.y); }
            string sheet = D2SheetHA(e); var dart = T.d2Hooks1434 ? D2Feel1434.Death(sheet) : null; int dir = (e.walkRow >> 2) & 3;
            if (dart != null)
            {   // art: collapse frames at d2DeathFps, last frame = corpse pose; the corpse sprite takes over seamlessly
                var fr = dart[dir]; int k = (int)(e.deathT * T.d2DeathFps1434);
                if (e.type == EType.Splitter && !e.splitDone && k >= 2) D2Split1434(e);
                if (k >= fr.Length || e.deathT > 2f) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); D2SpawnCorpse1434(e, -1, dir); return true; }
                e.sr.sprite = fr[k]; e.sr.flipX = false; return true;
            }
            int ci = D2CorpseFrame1434(e); int n = ci < 0 ? HArt.EnemyDeathN : ci + 1; int k2 = (int)(e.deathT / Mathf.Max(0.05f, T.d2DeathS1434) * n);
            if (e.type == EType.Splitter && !e.splitDone && k2 >= HArt.SplitFrame) D2Split1434(e);
            if (k2 >= n || e.deathT > 2f) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); if (ci >= 0) D2SpawnCorpse1434(e, ci, dir); return true; }
            e.sr.sprite = e.frames[HArt.EnemyDeath0 + k2]; return true;
        }
        void D2Split1434(Enemy e)
        {
            e.splitDone = true;
            for (int s = -1; s <= 1; s += 2) { var c = SpawnEnemyAt(EType.SplitChild, e.pos + new Vector2(s * 13f / HArt.PPU, 0)); if (c != null) c.push = new Vector2(s * 1.5f, 0); }
        }
        Sprite D2CorpseSprite1434(Enemy e, int ci, int dir, out bool art)
        {
            var T = HellordTuning.I; string sheet = D2SheetHA(e); art = false; if (ci == -2 || ci == -3) { art = true; return null; }
            Sprite sp = null;
            if (T.d2Hooks1434 && D2Pl1446(e)) { var cp = D2Feel1434.CorpsePl(D2Sheet(e)); if (cp != null) sp = cp[dir][0]; }
            if (sp == null && T.d2Hooks1434) { var ca = D2Feel1434.Corpse(sheet); if (ca != null) sp = ca[dir]; }
            art = sp != null;
            if (sp == null && ci < 0) { var da = D2Feel1434.Death(sheet); if (da != null) { sp = da[dir][da[dir].Length - 1]; art = true; } }
            if (sp == null) sp = e.frames[HArt.EnemyDeath0 + Mathf.Max(0, ci)];
            return sp;
        }
        Color D2CorpseTint1434(Enemy e, bool art)
        {
            var T = HellordTuning.I; Color tint = art ? Color.Lerp(Color.white, T.d2CorpseTint1434, 0.35f) : T.d2CorpseTint1434; tint.a = 1f;
            if (e.burn1434 && T.d2Burn1434) tint = Color.Lerp(tint, new Color(0.15f, 0.17f, 0.27f, 1f), 0.55f); // charred
            return tint;
        }
        /// <summary>ci &lt; 0 = art death just finished (corpse art, else the death strip's last frame); -2 = gib (decal only); -3 = PixelLab slam (pool/decal now, the body later via SetBody). Returns the corpse object.</summary>
        D2Corpse1434 D2SpawnCorpse1434(Enemy e, int ci, int dir, float extraLife = 0f, Sprite over = null, bool flip = false)
        {
            var T = HellordTuning.I; bool pl = D2Pl1446(e); string sheet = pl ? D2Sheet(e) : D2SheetHA(e); Sprite sp = D2CorpseSprite1434(e, ci, dir, out bool art); if (over != null) { sp = over; art = true; } Color tint = D2CorpseTint1434(e, art);
            bool ghost = pl && D2Pl1454.IsGhost(D2Feel1434.Key(sheet)); // t1454: wraith / rice ghost = ectoplasm pool, no blood decal
            bool big = e.type == EType.Brute || e.type == EType.Splitter; bool hooks = T.d2Hooks1434 && (e.type != EType.Elite || ghost); // the HA wraith gets no pool / decal (art brief); the PixelLab wraith has the ectoplasm pool
            Sprite[] decal = hooks && !ghost && T.d2DecalMode1434 >= 1 ? D2Feel1434.Variant("blood_decal") : null;
            Sprite pool = hooks && (T.d2DecalMode1434 != 1 || decal == null) ? D2Feel1434.PoolOrPlaceholder(sheet, big, T.d2Placeholders1434 && !art) : null;
            float life = D2Persist1447() ? float.PositiveInfinity : (e.corpseLife1434 > 0 ? e.corpseLife1434 : Random.Range(T.d2CorpseLifeS1434.x, T.d2CorpseLifeS1434.y)) + extraLife;
            return D2Feel1434.Spawn(sp, e.tr.position, e.tr.localScale, tint, YSort(e.pos.y), pool, decal, life, T.d2CorpseFadeS1434, Mathf.Max(1, T.d2CorpseCap1434), flip);
        }
        // ================= t1457 boss PixelLab deaths + corpse hooks =================
        int _bplSt; bool _bplWest, _bplPop, _bplSlam; float _bplT; D2Corpse1434 _bplCp; Sprite[][] _bplD, _bplC; string _bplKey; int _bplFace;
        string BossPlKey1457() { switch (BossKind44) { case 2: return "boss_horseface_pestle"; case 3: return "boss_ice_gorilla"; case 4: return "boss_qilin_chariot"; default: return "boss_oxhead_clubjailer"; } }
        /// <summary>Called once per boss-death frame BEFORE AnimateBoss. True = the PixelLab death (7 f @ d2DeathFps1446, POP f2, pool f4, permanent corpse) drives the body sprite.</summary>
        bool D2BossPlBegin1457()
        {
            if (_bplSt != 0) return _bplSt == 1; var T = HellordTuning.I; _bplSt = 2; _bplCp = null; _bplPop = _bplSlam = false; _bplT = 0;
            if (!(T.d2feel1434 && T.d2anim1446 && T.d2Death1434 && T.d2Hooks1434) || _bossBody == null) return false;
            _bplKey = BossPlKey1457(); _bplD = D2Feel1434.BossDeathPl(_bplKey); _bplC = D2Feel1434.BossCorpsePl(_bplKey); if (_bplD == null || _bplC == null) return false;
            _bplWest = _bossBody.flipX; _bplFace = _bplKey == "boss_qilin_chariot" && _qLastFace44 == 1 && _bplD[1] != null ? 1 : 0; _bplSt = 1; return true;
        }
        bool D2BossPlWest1457 => _bplWest;
        /// <summary>t1457: boss death voice (phase-2 death_oxhead / horseface / icegorilla / qilin) when d2sfx1447 is on; silent otherwise (the sheet death has its own sound).</summary>
        void D2BossDeathVoice1457(Enemy e) { if (!D2Sfx1447()) return; string h = D2Hook1457("death_", D2Fam1447(e)); if (h != null) HellordAudio.Play(h); }
        /// <summary>After AnimateBoss: overrides the body sprite with the PixelLab strip; spawns the pool at the slam and the permanent corpse at the end.</summary>
        void D2BossPlTick1457(Enemy e, float dt)
        {
            if (_bplSt != 1 || _bossBody == null) return; var T = HellordTuning.I; _bplT += dt; int k = (int)(_bplT * Mathf.Max(1f, T.d2DeathFps1446));
            var fr = _bplD[_bplFace]; var cs = _bplC[_bplFace] != null ? _bplC[_bplFace] : _bplC[0]; var tr = _bossBody.transform; tr.localPosition = Vector3.zero;
            bool flip = _bplKey == "boss_horseface_pestle" || _bplKey == "boss_oxhead_clubjailer" ? _bplWest : _bplKey == "boss_qilin_chariot" && _bplFace == 1 ? _bplWest : false; // Horse-Face SE and Ox-Head S flip for W; Qilin E flips for W; Gorilla one pose for every facing
            int ys = YSort(e.pos.y); Vector3 sc = tr.lossyScale;
            if (!_bplPop && k >= 2) { _bplPop = true; HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, new Color(0.55f, 0.06f, 0.08f), 14); }
            if (!_bplSlam && k >= 4)
            {
                _bplSlam = true; bool persist = D2Persist1447(); Sprite pool = D2Feel1434.PoolOrPlaceholder("enemy_" + _bplKey, true, T.d2Placeholders1434); Sprite[] decal = T.d2DecalMode1434 >= 1 ? D2Feel1434.Variant("blood_decal") : null; if (decal != null && T.d2DecalMode1434 == 1) pool = null;
                _bplCp = D2Feel1434.Spawn(null, tr.position, sc, Color.white, ys, pool, decal, persist ? float.PositiveInfinity : 20f, T.d2CorpseFadeS1434, Mathf.Max(1, T.d2CorpseCap1434));
            }
            if (k >= fr.Length) { Color tint = Color.Lerp(Color.white, T.d2CorpseTint1434, 0.35f); tint.a = 1f; if (_bplCp != null) D2Feel1434.SetBody(_bplCp, cs[0], tr.position, sc, tint, ys, flip); _bossBody.enabled = false; _bplSt = 3; return; }
            _bossBody.sprite = fr[k]; _bossBody.flipX = flip; _bossBody.enabled = true;
            if (k == 2) { HordeFX.Flash(_bossBody); _bossBody.color = Color.white; }
        }
        /// <summary>Boss death finished without PixelLab art: leave pool / decal and the last body frame as a corpse.</summary>
        void D2BossCorpseFallback1457(Enemy e)
        {
            var T = HellordTuning.I; if (_bplSt == 3 || !(T.d2feel1434 && T.d2Death1434 && T.d2Hooks1434) || _bossBody == null) return; _bplSt = 3; int ys = YSort(e.pos.y);
            Sprite pool = D2Feel1434.PoolOrPlaceholder("enemy_" + BossPlKey1457(), true, T.d2Placeholders1434); Sprite[] decal = T.d2DecalMode1434 >= 1 ? D2Feel1434.Variant("blood_decal") : null; if (decal != null && T.d2DecalMode1434 == 1) pool = null;
            var c = D2Feel1434.Spawn(_bossBody.sprite, _bossBody.transform.position, _bossBody.transform.lossyScale, Color.Lerp(Color.white, T.d2CorpseTint1434, 0.35f), ys, pool, decal, D2Persist1447() ? float.PositiveInfinity : 20f, T.d2CorpseFadeS1434, Mathf.Max(1, T.d2CorpseCap1434), _bossBody.flipX);
        }
        /// <summary>t1457: Death44 (Frost Caster / Fire Wheel sheet death) and Death53 (Mortar demons) leave a corpse too: the last body frame + pool / decal.</summary>
        void D2CorpseFromSprite1457(Enemy e, Sprite sp, bool flip)
        {
            var T = HellordTuning.I; if (!(T.d2feel1434 && T.d2Death1434 && T.d2Hooks1434) || e.frozenDeath1434 || sp == null) return; D2SpawnCorpse1434(e, -4, 0, 0f, sp, flip);
        }
        /// <summary>t1446 PixelLab death: 7 frames @ d2DeathFps1446. POP = frame 2: white silhouette flash, blood_death / gib / burn FX, death scream, gore_splat. Slam = frame 4: blood pool / decal appears. Frame 6 = corpse pose; the corpse art takes over when the anim ends. Gib: body hidden after the POP frame, decal at the POP, no corpse.</summary>
        bool D2DeathPl1446(Enemy e, float dt)
        {
            var T = HellordTuning.I; var fr = D2Feel1434.DeathPl(D2Sheet(e))[D2Dir4(e)]; int dir = D2Dir4(e); if (e.variant == 4) e.tr.rotation = Quaternion.identity;
            e.deathT += dt;
            if (e.kbDist1434 > 0) { D2Knock1434(e, dt); ClampToRoom(ref e.pos, 6); e.tr.position = new Vector3(Snap(e.pos.x), Snap(e.pos.y), 0); e.sr.sortingOrder = YSort(e.pos.y); }
            int k = (int)(e.deathT * Mathf.Max(1f, T.d2DeathFps1446));
            if (!e.pop1434 && k >= 2) { e.pop1434 = true; D2FireDeathFx1434(e); D2PopSfx1446(e); if (e.type == EType.Splitter && !e.splitDone) D2Split1434(e); if (e.gib1434) D2SpawnCorpse1434(e, -2, dir); }
            if (e.gib1434 && k >= 3) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); return true; }
            if (!e.slam1434 && k >= 4 && !e.gib1434) { e.slam1434 = true; e.cp1434 = D2SpawnCorpse1434(e, -3, dir, (fr.Length - 4) / Mathf.Max(1f, T.d2DeathFps1446)); }
            if (k >= fr.Length || e.deathT > 2f)
            {
                e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false);
                if (e.cp1434 != null) { var sp = D2CorpseSprite1434(e, 0, dir, out bool art); D2Feel1434.SetBody(e.cp1434, sp, e.tr.position, e.tr.localScale, D2CorpseTint1434(e, art), YSort(e.pos.y)); }
                else D2SpawnCorpse1434(e, 0, dir);
                return true;
            }
            e.sr.sprite = fr[k]; e.sr.flipX = false;
            if (k == 2) { HordeFX.Flash(e.sr); e.sr.color = Color.white; } // POP: white silhouette of the frame (Flash material swap, re-armed every update of the 50 ms frame)
            return true;
        }
        bool D2FrozenDeath1434(Enemy e, float dt)
        {
            var T = HellordTuning.I; bool fx = e.fxFrozen1434; float f = fx ? 1f / D2Feel1434.FxFps : Mathf.Max(0.01f, T.d2FrameS1434); e.deathT += dt; int k = (int)(e.deathT / f);
            if (k < 2) { HordeFX.Flash(e.sr); e.sr.color = Color.white; return true; } // 2 frames: white flash
            if (k < 6) { e.sr.color = fx ? Color.Lerp(Color.white, new Color(0.753f, 0.796f, 0.863f, 1f), 0.6f) : D2Statue; return true; } // 4 frames: frozen statue (pose tinted 60% toward c0cbdc)
            Vector2 c = e.pos + Vector2.up * e.hitY; if (!fx) HFX.Shatter(c); // the strip carries the shatter (mist + shards) when present
            if (T.d2Hooks1434 && HellordAudio.Has("ice_shatter")) HellordAudio.Play("ice_shatter");
            if (e.type == EType.Splitter && !e.splitDone) D2Split1434(e);
            e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); return true;
        }
    }
}
