using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  t1481: HFX strips for Magma Throw / Rolling Magma Rock / Fire Pillar (Resources/HellordArt/FireBasics/<key>_strip.png + <key>_glow_strip.png).
    //  30 fps, 16 PPU, point filtered, alpha 0/255, drawn unrotated at integer 1x (only the rock-hit spark is rotated). Pivots are HFX NOTES.txt values (px from the top-left of a frame).
    //  Three kinds of drawing:  Show()  = immediate, every frame (flying globs, children, the rolling rock, shadows)
    //                           Once()  = one-shot strip that plays itself (burst, splash, hit spark, crumble, tuft, telegraph, eruption)
    //                           Decal() = staged floor decal (pools, scorch) stepped by HFX frame tables and capped; later stages are dither subsets
    //  Glow: optional white masks drawn additive with Hellord/FrostGlow1430 (tint ff963c, HellordTuning.fbGlowAdd1481). No shader / no mask -> sprites only.
    // =====================================================================================================================
    public class FireBasicsFX1481 : MonoBehaviour
    {
        const string Dir = "HellordArt/FireBasics/"; const float FPS = 30f, PPU = 16f;
        sealed class Strip { public Sprite[,] f, g; public int n, rows; public float add; public Material mat; }
        // key, frame w, h, frames, pivot x, y (px from the top-left), rows, glow _Add (0 = no mask). Glow _Add values are HFX v2 NOTES section 6 (mask sigma is baked per strip, so small frames get their own small glow).
        static readonly object[][] Spec =
        {
            new object[] { "molten_spit_glob_dir16", 56, 56, 8, 28, 28, 16, 0.30f }, new object[] { "molten_spit_shadow", 16, 8, 4, 8, 4, 1, 0f }, new object[] { "molten_spit_ember", 8, 8, 6, 4, 1, 1, 0f },
            new object[] { "molten_spit_marker", 24, 14, 8, 12, 7, 1, 0f }, new object[] { "molten_spit_burst", 104, 76, 18, 52, 58, 1, 0.40f },
            new object[] { "molten_spit_shard_dir16", 80, 80, 6, 40, 40, 16, 0.35f }, new object[] { "molten_spit_shard_land", 48, 32, 10, 24, 22, 1, 0.30f }, new object[] { "molten_spit_shard_scar_dir16", 18, 18, 5, 9, 9, 16, 0f },
            new object[] { "molten_spit_scorch_v1", 64, 30, 8, 32, 15, 1, 0f }, new object[] { "molten_spit_scorch_v2", 64, 30, 8, 32, 15, 1, 0f }, new object[] { "molten_spit_scorch_v3", 64, 30, 8, 32, 15, 1, 0f },
            new object[] { "molten_spit_scorch_small_v1", 26, 14, 6, 13, 7, 1, 0f }, new object[] { "molten_spit_scorch_small_v2", 26, 14, 6, 13, 7, 1, 0f },
            new object[] { "infernal_boulder_roll_dir16", 96, 104, 16, 48, 56, 16, 0.30f }, new object[] { "infernal_boulder_trail", 28, 32, 28, 14, 26, 1, 0.30f }, new object[] { "infernal_boulder_scorch_v1", 24, 10, 6, 12, 5, 1, 0f },
            new object[] { "infernal_boulder_scorch_v2", 24, 10, 6, 12, 5, 1, 0f }, new object[] { "infernal_boulder_hit", 48, 44, 8, 24, 30, 1, 0.30f }, new object[] { "infernal_boulder_crumble", 88, 88, 24, 44, 60, 1, 0.40f },
            new object[] { "fire_pillar_telegraph", 40, 20, 12, 20, 10, 1, 0f }, new object[] { "fire_pillar_erupt", 56, 76, 18, 28, 66, 1, -1f }, new object[] { "fire_pillar_scorch_v1", 36, 16, 5, 18, 8, 1, 0f }, new object[] { "fire_pillar_scorch_v2", 36, 16, 5, 18, 8, 1, 0f },
        };
        static readonly Dictionary<string, Strip> _s = new Dictionary<string, Strip>(); static FireBasicsFX1481 _i; static bool _tried, _ok, _glowOk; static Shader _sh; static readonly Dictionary<float, Material> _mats = new Dictionary<float, Material>();
        public static string Status = "fireFx1481 not tried"; public static bool ForceNoGlow;
        public static bool Ok { get { if (!_tried) Init(); return _ok && _i != null; } }
        public static bool Glow => _glowOk;
        static Texture2D Tex(string n) { var t = Resources.Load<Texture2D>(Dir + n); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        static Sprite[,] Grid(Texture2D t, int fw, int fh, int cols, int rows, float px, float py)
        {
            var g = new Sprite[rows, cols]; for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++) g[r, c] = Sprite.Create(t, new Rect(c * fw, t.height - (r + 1) * fh, fw, fh), new Vector2(px / fw, 1f - py / fh), PPU, 0, SpriteMeshType.FullRect); return g;
        }
        static Material GlowMat(float add)
        {
            if (_mats.TryGetValue(add, out var m)) return m; m = new Material(_sh) { name = "firebasicsglow1481_" + add.ToString("0.00") }; m.SetColor("_Tint", new Color32(0xff, 0x96, 0x3c, 255)); m.SetFloat("_Gain", 1.6f); m.SetFloat("_Add", add); return _mats[add] = m;
        }
        static void Init()
        {
            _tried = true; int got = 0, glow = 0;
            foreach (var sp in Spec)
            {
                string k = (string)sp[0]; int fw = (int)sp[1], fh = (int)sp[2], n = (int)sp[3], rows = (int)sp[6]; float px = (int)sp[4], py = (int)sp[5], gadd = (float)sp[7];
                var t = Tex(k + "_strip"); if (t == null || t.width < fw * n || t.height < fh * rows) continue;
                var st = new Strip { n = n, rows = rows, f = Grid(t, fw, fh, n, rows, px, py), add = gadd }; got++;
                if (gadd != 0f) { var tg = Tex(k + "_glow_strip"); if (tg != null && tg.width >= fw * n && tg.height >= fh * rows) { st.g = Grid(tg, fw, fh, n, rows, px, py); glow++; } }
                _s[k] = st;
            }
            if (got == 0) { Status = "fireFx1481: strips missing"; return; }
            _sh = Resources.Load<Shader>("HellordShaders/HellordFrostGlow"); if (_sh == null) _sh = Shader.Find("Hellord/FrostGlow1430");
            var T = HellordTuning.I; _glowOk = !ForceNoGlow && glow > 0 && _sh != null && _sh.isSupported && (T == null || T.fbGlow1481);
            if (_glowOk) { float mul = T != null ? T.fbGlowMul1481 : 1f; foreach (var st in _s.Values) if (st.g != null) st.mat = GlowMat(st.add < 0f ? (T != null ? T.fbGlowAdd1481 : 0.3f) : st.add * mul); }
            var go = new GameObject("FireBasicsFX1481"); if (Application.isPlaying) DontDestroyOnLoad(go); _i = go.AddComponent<FireBasicsFX1481>(); _ok = true;
            Status = "fireFx1481 ok: " + got + "/" + Spec.Length + " strips, glow " + (_glowOk ? glow + " masks (shader)" : "OFF");
        }
        static float Snap(float u) => Mathf.Round(u * PPU) / PPU;
        public static int Frames(string key) => Ok && _s.TryGetValue(key, out var s) ? s.n : 0;
        /// <summary>Row of the 16-direction strip for a direction (world, y up): 22.5 deg steps, 0 = east, clockwise on screen.</summary>
        public static int DirRow(Vector2 v) { float a = Mathf.Atan2(-v.y, v.x) * Mathf.Rad2Deg; if (a < 0f) a += 360f; return Mathf.RoundToInt(a / 22.5f) & 15; }

        // ------------------------------------------------------------------ pooled renderer pairs
        sealed class Pair { public SpriteRenderer sp, gl; public bool live; }
        readonly List<Pair> _im = new List<Pair>(); int _imN; bool _called;
        Pair NewPair() { return new Pair { sp = NewSR("fb"), gl = NewSR("fbg") }; }
        SpriteRenderer NewSR(string n) { var go = new GameObject(n); go.transform.SetParent(transform, false); var sr = go.AddComponent<SpriteRenderer>(); sr.enabled = false; return sr; }
        static void Put(Pair p, Strip s, int row, int frame, Vector2 pos, int order, float rot, bool flip)
        {
            frame = Mathf.Clamp(frame, 0, s.n - 1); row = Mathf.Clamp(row, 0, s.rows - 1); var tp = new Vector3(Snap(pos.x), Snap(pos.y), 0f);
            p.sp.enabled = true; p.sp.sprite = s.f[row, frame]; p.sp.transform.position = tp; p.sp.transform.rotation = Quaternion.Euler(0, 0, rot); p.sp.sortingOrder = order; p.sp.flipX = flip; p.sp.color = Color.white;
            if (_glowOk && s.g != null && s.mat != null) { p.gl.enabled = true; p.gl.sharedMaterial = s.mat; p.gl.sprite = s.g[row, frame]; p.gl.transform.position = tp; p.gl.transform.rotation = Quaternion.Euler(0, 0, rot); p.gl.sortingOrder = order + 1; p.gl.flipX = flip; p.gl.color = Color.white; } else p.gl.enabled = false;
        }
        static void Hide(Pair p) { p.sp.enabled = false; p.gl.enabled = false; }

        /// <summary>Call once per game frame BEFORE any Show(): marks the frame as driven (a paused game keeps the last frame on screen).</summary>
        public static void Frame() { if (Ok) _i._called = true; }
        /// <summary>IMMEDIATE: draw frame `frame` (and `row` of a dir16 strip) this frame only.</summary>
        public static void Show(string key, Vector2 pos, int frame, int order, int row = 0)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return; var a = _i; if (a._imN >= a._im.Count) a._im.Add(a.NewPair()); Put(a._im[a._imN++], s, row, frame, pos, order, 0f, false);
        }

        // ------------------------------------------------------------------ one-shots
        sealed class OnceI { public Pair p; public Strip s; public float age, speed, rot, life; public int phase; public Vector2 pos; public int order; public bool flip; public string key; }
        readonly List<OnceI> _once = new List<OnceI>();
        /// <summary>ONE-SHOT strip. speed 1 = 30 fps. cap > 0: ignored while that many of the same key are alive (HFX spam guard).</summary>
        public static bool Once(string key, Vector2 pos, int order, float rot = 0f, float speed = 1f, int cap = 0, bool flip = false)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return false; var a = _i;
            if (cap > 0) { int c = 0; foreach (var o in a._once) if (o.p.live && o.key == key) c++; if (c >= cap) return false; }
            OnceI x = null; foreach (var o in a._once) if (!o.p.live) { x = o; break; } if (x == null) { x = new OnceI { p = a.NewPair() }; a._once.Add(x); }
            x.p.live = true; x.s = s; x.age = 0; x.life = 0f; x.phase = 0; x.speed = Mathf.Max(0.05f, speed); x.pos = pos; x.order = order; x.rot = rot; x.flip = flip; x.key = key; Put(x.p, s, 0, 0, pos, order, rot, flip); return true;
        }

        /// <summary>Burning-line tile (Infernal Boulder trail): 28 frames = ignite f0-5, 12-frame loop f6-17 with a random start phase, die f18-27 over the last 10 frames of its lifetime (lifeS seconds).</summary>
        public static void Tile(string key, Vector2 pos, int order, float lifeS)
        {
            if (!Ok || !_s.TryGetValue(key, out var s) || s.n < 28) return; var a = _i;
            OnceI x = null; foreach (var o in a._once) if (!o.p.live) { x = o; break; } if (x == null) { x = new OnceI { p = a.NewPair() }; a._once.Add(x); }
            x.p.live = true; x.s = s; x.age = 0; x.speed = 1f; x.pos = pos; x.order = order; x.rot = 0; x.flip = false; x.key = key; x.life = Mathf.Max(16f, lifeS * FPS); x.phase = Random.Range(0, 12); Put(x.p, s, 0, 0, pos, order, 0f, false);
        }
        static int TileFrame(OnceI o) { float f = o.age * FPS; if (f >= o.life) return -1; if (f < 6f) return (int)f; if (f >= o.life - 10f) return 18 + Mathf.Clamp((int)(f - (o.life - 10f)), 0, 9); return 6 + (((int)f - 6 + o.phase) % 12); }

        // ------------------------------------------------------------------ staged floor decals
        public enum DecalKind { SpitScorch, SpitScorchSmall, ShardScar, BoulderScorch, PillarScorch }
        sealed class DecalI { public Pair p; public Strip s; public DecalKind kind; public float f; public Vector2 pos; public int order, row; public int stage = -9; }
        readonly List<DecalI> _dec = new List<DecalI>();
        // HFX v2 stage ends (frames after the decal starts): a stage shows until its end frame, after the last end the decal is gone
        static readonly float[][] Ends =
        {
            new float[] { 14, 28, 44, 60, 74, 84, 92, 98 },      // spit scorch 0 YL/AM 14, 1 AM/OR 14, 2 OR/RD 16, 3 RD 16, 4 RD/brown 14, 5 55% 10, 6 22% 8, 7 specks 6
            new float[] { 8, 16, 26, 38, 48, 56 },              // small scorch 8,8,10,12,10,8
            new float[] { 6, 16, 30, 46, 56 },                  // shard scar 6,10,14,16,10
            new float[] { 18, 36, 60, 96, 120, 136 },           // boulder scorch 18,18,24,36,24,16
            new float[] { 30, 46, 58, 70, 78 },                 // pillar scorch
        };
        static readonly int[] Cap = { 8, 12, 220, 24, 12 }, DissolveStage = { 5, 3, 3, 4, 3 };
        static int Stage(DecalKind k, float f) { var e = Ends[(int)k]; for (int i = 0; i < e.Length; i++) if (f < e[i]) return i; return -2; }
        /// <summary>Staged floor decal. delayF = frames before it starts. row = direction row of a dir16 strip (shard scar). Over the HFX cap the oldest jumps to its dissolve stage.</summary>
        public static void Decal(string key, DecalKind kind, Vector2 pos, int order, float delayF = 0f, int row = 0)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return; var a = _i; int cap = Cap[(int)kind]; int live = 0; DecalI oldest = null;
            foreach (var d in a._dec) if (d.p.live && d.kind == kind) { live++; if (oldest == null || d.f > oldest.f) oldest = d; }
            if (live >= cap && oldest != null) oldest.f = Mathf.Max(oldest.f, Ends[(int)kind][DissolveStage[(int)kind] - 1]);
            DecalI x = null; foreach (var d in a._dec) if (!d.p.live) { x = d; break; } if (x == null) { x = new DecalI { p = a.NewPair() }; a._dec.Add(x); }
            x.p.live = true; x.s = s; x.kind = kind; x.f = -delayF; x.pos = pos; x.order = order; x.row = row; x.stage = -9; Hide(x.p);
        }
        public static void Clear() { if (_i == null) return; foreach (var o in _i._once) { o.p.live = false; Hide(o.p); } foreach (var d in _i._dec) { d.p.live = false; Hide(d.p); } foreach (var p in _i._im) Hide(p); _i._imN = 0; }
        /// <summary>Clears one-shots / immediates only; floor decals keep fading (used on a room reset is Clear()).</summary>
        void LateUpdate()
        {
            if (_called) { for (int i = _imN; i < _im.Count; i++) Hide(_im[i]); _imN = 0; _called = false; }
            float dt = Time.deltaTime;
            foreach (var o in _once)
            {
                if (!o.p.live) continue; o.age += dt * o.speed; int fi = o.life > 0f ? TileFrame(o) : (int)(o.age * FPS); if (fi < 0 || fi >= o.s.n) { o.p.live = false; Hide(o.p); continue; } Put(o.p, o.s, 0, fi, o.pos, o.order, o.rot, o.flip);
            }
            foreach (var d in _dec)
            {
                if (!d.p.live) continue; d.f += dt * FPS; int st = Stage(d.kind, d.f);
                if (st == -2) { d.p.live = false; Hide(d.p); continue; } if (st == -1 || d.f < 0f) { Hide(d.p); continue; }
                if (st != d.stage) { d.stage = st; Put(d.p, d.s, d.row, st, d.pos, d.order, 0f, false); }
            }
        }
    }
}
