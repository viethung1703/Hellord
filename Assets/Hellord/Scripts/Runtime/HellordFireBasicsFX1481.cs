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
        sealed class Strip { public Sprite[,] f, g; public int n, rows; public bool small; }
        // small frames (no padding for the bloom to spread into) get a weak glow gain, otherwise the baked + shader blur fills the whole quad into a visible square (play-check t1481)
        static readonly HashSet<string> SmallGlow = new HashSet<string> { "magma_throw_glob", "magma_child_blob", "magma_child_splash" };
        // key, frame w, h, frames, pivot x, y (px from top-left), rows, has glow mask
        static readonly object[][] Spec =
        {
            new object[] { "magma_throw_glob", 16, 16, 8, 8, 8, 1, true }, new object[] { "magma_throw_shadow", 16, 8, 4, 8, 4, 1, false }, new object[] { "magma_throw_ember", 8, 8, 6, 4, 1, 1, false },
            new object[] { "magma_throw_marker", 24, 14, 8, 12, 7, 1, false }, new object[] { "magma_throw_burst", 64, 48, 14, 32, 34, 1, true }, new object[] { "magma_child_blob", 10, 10, 6, 5, 5, 1, true },
            new object[] { "magma_child_splash", 20, 14, 6, 10, 10, 1, true }, new object[] { "magma_pool_v1", 36, 18, 7, 18, 9, 1, false }, new object[] { "magma_pool_v2", 36, 18, 7, 18, 9, 1, false }, new object[] { "magma_pool_v3", 36, 18, 7, 18, 9, 1, false },
            new object[] { "magma_pool_small_v1", 16, 10, 6, 8, 5, 1, false }, new object[] { "magma_pool_small_v2", 16, 10, 6, 8, 5, 1, false },
            new object[] { "rolling_magma_roll_dir16", 56, 56, 10, 28, 28, 16, true }, new object[] { "rolling_magma_trail", 16, 20, 12, 8, 17, 1, false }, new object[] { "rolling_magma_scorch_v1", 24, 10, 5, 12, 5, 1, false },
            new object[] { "rolling_magma_scorch_v2", 24, 10, 5, 12, 5, 1, false }, new object[] { "rolling_magma_hit", 22, 22, 6, 6, 11, 1, false }, new object[] { "rolling_magma_crumble", 48, 40, 16, 24, 28, 1, true },
            new object[] { "fire_pillar_telegraph", 40, 20, 12, 20, 10, 1, false }, new object[] { "fire_pillar_erupt", 56, 76, 18, 28, 66, 1, true }, new object[] { "fire_pillar_scorch_v1", 36, 16, 5, 18, 8, 1, false }, new object[] { "fire_pillar_scorch_v2", 36, 16, 5, 18, 8, 1, false },
        };
        static readonly Dictionary<string, Strip> _s = new Dictionary<string, Strip>(); static FireBasicsFX1481 _i; static bool _tried, _ok, _glowOk; static Shader _sh; static Material _mat, _matSmall;
        public static string Status = "fireFx1481 not tried"; public static bool ForceNoGlow;
        public static bool Ok { get { if (!_tried) Init(); return _ok && _i != null; } }
        public static bool Glow => _glowOk;
        static Texture2D Tex(string n) { var t = Resources.Load<Texture2D>(Dir + n); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        static Sprite[,] Grid(Texture2D t, int fw, int fh, int cols, int rows, float px, float py)
        {
            var g = new Sprite[rows, cols]; for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++) g[r, c] = Sprite.Create(t, new Rect(c * fw, t.height - (r + 1) * fh, fw, fh), new Vector2(px / fw, 1f - py / fh), PPU, 0, SpriteMeshType.FullRect); return g;
        }
        static void Init()
        {
            _tried = true; int got = 0, glow = 0;
            foreach (var sp in Spec)
            {
                string k = (string)sp[0]; int fw = (int)sp[1], fh = (int)sp[2], n = (int)sp[3], rows = (int)sp[6]; float px = (int)sp[4], py = (int)sp[5];
                var t = Tex(k + "_strip"); if (t == null || t.width < fw * n || t.height < fh * rows) continue;
                var st = new Strip { n = n, rows = rows, f = Grid(t, fw, fh, n, rows, px, py), small = SmallGlow.Contains(k) }; got++;
                if ((bool)sp[7]) { var tg = Tex(k + "_glow_strip"); if (tg != null && tg.width >= fw * n && tg.height >= fh * rows) { st.g = Grid(tg, fw, fh, n, rows, px, py); glow++; } }
                _s[k] = st;
            }
            if (got == 0) { Status = "fireFx1481: strips missing"; return; }
            _sh = Resources.Load<Shader>("HellordShaders/HellordFrostGlow"); if (_sh == null) _sh = Shader.Find("Hellord/FrostGlow1430");
            var T = HellordTuning.I; _glowOk = !ForceNoGlow && glow > 0 && _sh != null && _sh.isSupported && (T == null || T.fbGlow1481);
            if (_glowOk) { _mat = new Material(_sh) { name = "firebasicsglow1481" }; _mat.SetColor("_Tint", new Color32(0xff, 0x96, 0x3c, 255)); _mat.SetFloat("_Gain", 1.6f); _mat.SetFloat("_Add", T != null ? T.fbGlowAdd1481 : 0.3f); _matSmall = new Material(_mat) { name = "firebasicsglowsmall1481" }; _matSmall.SetFloat("_Gain", T != null ? T.fbGlowSmallGain1481 : 0.5f); }
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
        Pair NewPair() { var p = new Pair { sp = NewSR("fb"), gl = NewSR("fbg") }; if (_mat != null) p.gl.sharedMaterial = _mat; return p; }
        SpriteRenderer NewSR(string n) { var go = new GameObject(n); go.transform.SetParent(transform, false); var sr = go.AddComponent<SpriteRenderer>(); sr.enabled = false; return sr; }
        static void Put(Pair p, Strip s, int row, int frame, Vector2 pos, int order, float rot, bool flip)
        {
            frame = Mathf.Clamp(frame, 0, s.n - 1); row = Mathf.Clamp(row, 0, s.rows - 1); var tp = new Vector3(Snap(pos.x), Snap(pos.y), 0f);
            p.sp.enabled = true; p.sp.sprite = s.f[row, frame]; p.sp.transform.position = tp; p.sp.transform.rotation = Quaternion.Euler(0, 0, rot); p.sp.sortingOrder = order; p.sp.flipX = flip; p.sp.color = Color.white;
            if (_glowOk && s.g != null) { p.gl.enabled = true; p.gl.sharedMaterial = s.small ? _matSmall : _mat; p.gl.sprite = s.g[row, frame]; p.gl.transform.position = tp; p.gl.transform.rotation = Quaternion.Euler(0, 0, rot); p.gl.sortingOrder = order + 1; p.gl.flipX = flip; p.gl.color = Color.white; } else p.gl.enabled = false;
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
        sealed class OnceI { public Pair p; public Strip s; public float age, speed, rot; public Vector2 pos; public int order; public bool flip; public string key; }
        readonly List<OnceI> _once = new List<OnceI>();
        /// <summary>ONE-SHOT strip. speed 1 = 30 fps. cap > 0: ignored while that many of the same key are alive (HFX spam guard).</summary>
        public static bool Once(string key, Vector2 pos, int order, float rot = 0f, float speed = 1f, int cap = 0, bool flip = false)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return false; var a = _i;
            if (cap > 0) { int c = 0; foreach (var o in a._once) if (o.p.live && o.key == key) c++; if (c >= cap) return false; }
            OnceI x = null; foreach (var o in a._once) if (!o.p.live) { x = o; break; } if (x == null) { x = new OnceI { p = a.NewPair() }; a._once.Add(x); }
            x.p.live = true; x.s = s; x.age = 0; x.speed = Mathf.Max(0.05f, speed); x.pos = pos; x.order = order; x.rot = rot; x.flip = flip; x.key = key; Put(x.p, s, 0, 0, pos, order, rot, flip); return true;
        }

        // ------------------------------------------------------------------ staged floor decals
        public enum DecalKind { BigPool, SmallPool, Scorch, PillarScorch }
        sealed class DecalI { public Pair p; public Strip s; public DecalKind kind; public float f; public float delay; public Vector2 pos; public int order; public int stage = -9; }
        readonly List<DecalI> _dec = new List<DecalI>();
        // HFX NOTES: frames after the decal starts. -1 = not shown yet (the burst mound owns it), -2 = finished
        static int Stage(DecalKind k, float f)
        {
            switch (k)
            {
                case DecalKind.BigPool: if (f < 6) return -1; if (f < 36) return ((int)((f - 6f) / 5f)) & 1; if (f < 52) return 2; if (f < 68) return 3; if (f < 78) return 4; if (f < 88) return 5; if (f < 96) return 6; return -2;
                case DecalKind.SmallPool: if (f < 18) return ((int)(f / 5f)) & 1; if (f < 28) return 2; if (f < 36) return 3; if (f < 44) return 4; if (f < 52) return 5; return -2;
                case DecalKind.Scorch: if (f < 14) return 0; if (f < 28) return 1; if (f < 40) return 2; if (f < 52) return 3; if (f < 60) return 4; return -2;
                default: if (f < 30) return 0; if (f < 46) return 1; if (f < 58) return 2; if (f < 70) return 3; if (f < 78) return 4; return -2;
            }
        }
        static float DissolveFrame(DecalKind k) => k == DecalKind.BigPool ? 68f : k == DecalKind.SmallPool ? 36f : k == DecalKind.Scorch ? 40f : 58f;
        /// <summary>Staged floor decal. delayF = frames before it starts. Caps (HFX): 8 big pools, 10 small pools - beyond that the oldest jumps to the dissolve stage.</summary>
        public static void Decal(string key, DecalKind kind, Vector2 pos, int order, float delayF = 0f)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return; var a = _i; int cap = kind == DecalKind.BigPool ? 8 : kind == DecalKind.SmallPool ? 10 : 12; int live = 0; DecalI oldest = null;
            foreach (var d in a._dec) if (d.p.live && d.kind == kind) { live++; if (oldest == null || d.f > oldest.f) oldest = d; }
            if (live >= cap && oldest != null) oldest.f = Mathf.Max(oldest.f, DissolveFrame(kind));
            DecalI x = null; foreach (var d in a._dec) if (!d.p.live) { x = d; break; } if (x == null) { x = new DecalI { p = a.NewPair() }; a._dec.Add(x); }
            x.p.live = true; x.s = s; x.kind = kind; x.f = -delayF; x.pos = pos; x.order = order; x.stage = -9; Hide(x.p);
        }
        public static void Clear() { if (_i == null) return; foreach (var o in _i._once) { o.p.live = false; Hide(o.p); } foreach (var d in _i._dec) { d.p.live = false; Hide(d.p); } foreach (var p in _i._im) Hide(p); _i._imN = 0; }
        /// <summary>Clears one-shots / immediates only; floor decals keep fading (used on a room reset is Clear()).</summary>
        void LateUpdate()
        {
            if (_called) { for (int i = _imN; i < _im.Count; i++) Hide(_im[i]); _imN = 0; _called = false; }
            float dt = Time.deltaTime;
            foreach (var o in _once)
            {
                if (!o.p.live) continue; o.age += dt * o.speed; int fi = (int)(o.age * FPS); if (fi >= o.s.n) { o.p.live = false; Hide(o.p); continue; } Put(o.p, o.s, 0, fi, o.pos, o.order, o.rot, o.flip);
            }
            foreach (var d in _dec)
            {
                if (!d.p.live) continue; d.f += dt * FPS; int st = Stage(d.kind, d.f);
                if (st == -2) { d.p.live = false; Hide(d.p); continue; } if (st == -1 || d.f < 0f) { Hide(d.p); continue; }
                if (st != d.stage) { d.stage = st; Put(d.p, d.s, 0, st, d.pos, d.order, 0f, false); }
            }
        }
    }
}
