using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  t1481: HFX strips for Molten Spit (v5) / Infernal Boulder (v4) and Fire Pillar (Resources/HellordArt/FireBasics/<key>_strip.png + <key>_glow_strip.png).
    //  30 fps, 16 PPU, point filtered, alpha 0/255, drawn unrotated at integer 1x. Pivots are HFX NOTES values (Spit v5, Boulder v4) (px from the top-left of a frame).
    //  Kinds of drawing:  Show()  = immediate, every frame (flying rock, strands, rolling ball, shadows)
    //                     Once()  = one-shot strip that plays itself, optional start delay (impact, star, heat, gouge, cast ring, end blast, fire pillar)
    //                     Tile()  = burning tile of any strip with 6 ignite / 12 loop / (n-18) die frames: lives lifeS seconds, loop starts at a random phase (band, fire patch, ember patch)
    //                     Decal() = staged floor decal, Fire Pillar scorch only
    //  Glow: white masks drawn additive with Hellord/FrostGlow1430; tint, _Add and gain are per strip (Spit ff6028 / _Add 0.34, Boulder ff963c / 0.30, patches as in HFX NOTES section 4). No shader / no mask -> sprites only.
    // =====================================================================================================================
    public class FireBasicsFX1481 : MonoBehaviour
    {
        const string Dir = "HellordArt/FireBasics/"; const float FPS = 30f, PPU = 16f;
        sealed class Strip { public Sprite[,] f, g; public int n, rows; public float add, gain; public uint tint; public Material mat; }
        // key, frame w, h, frames, pivot x, y (px from the top-left), rows, glow _Add (0 = no mask, <0 = fbGlowAdd1481), tint rrggbb, gain (_Gain; 1.6 = standard)
        const uint Orange = 0xff963c, Red = 0xff6028, Or4 = 0xf77622, Ye4 = 0xfeae34, Lt4 = 0xfee761; const float G = 1.6f;
        static readonly object[][] Spec =
        {
            // Molten Spit (HFX v5 NOTES / spec_v5.json): native sizes, gain = G x (HFX gain / 0.45). Unused v4 keys (star, ember_patch_v3, heat) are not loaded.
            new object[] { "molten_spit_gouge", 44, 38, 30, 6, 29, 1, 0.34f, Or4, G * 0.55f / 0.45f }, new object[] { "molten_spit_rock_dir16", 63, 63, 8, 31, 31, 16, 0.34f, Or4, G },
            new object[] { "molten_spit_shadow", 16, 6, 3, 8, 3, 1, 0f, Or4, G }, new object[] { "molten_spit_impact", 71, 44, 7, 34, 27, 1, 0.34f, Ye4, G * 0.70f / 0.45f },
            new object[] { "molten_spit_burst_med", 37, 26, 6, 19, 16, 1, 0.34f, Ye4, G * 0.55f / 0.45f }, new object[] { "molten_spit_pop_v1", 11, 9, 3, 5, 4, 1, 0.34f, Lt4, G }, new object[] { "molten_spit_pop_v2", 10, 9, 3, 5, 4, 1, 0.34f, Lt4, G },
            new object[] { "molten_spit_darken", 43, 20, 6, 20, 10, 1, 0f, Or4, G }, new object[] { "molten_spit_strand_dir16", 35, 35, 24, 17, 17, 16, 0.34f, Or4, G * 0.30f / 0.45f },
            new object[] { "molten_spit_scorch", 21, 11, 6, 12, 6, 1, 0f, Or4, G },
            new object[] { "molten_spit_ember_patch_v1", 12, 11, 30, 6, 7, 1, 0.34f, Or4, G * 0.25f / 0.45f }, new object[] { "molten_spit_ember_patch_v2", 11, 11, 30, 5, 7, 1, 0.34f, Or4, G * 0.25f / 0.45f },
            // Infernal Boulder (HFX v4)
            new object[] { "infernal_boulder_roll_dir16", 56, 56, 32, 28, 28, 16, 0.30f, Or4, G * 0.40f / 0.45f }, new object[] { "infernal_boulder_shadow", 40, 12, 1, 20, 6, 1, 0f, Or4, G },
            new object[] { "infernal_boulder_band", 32, 40, 40, 16, 33, 1, 0.30f, Ye4, G * 0.36f / 0.45f }, new object[] { "infernal_boulder_band_b", 32, 40, 40, 16, 33, 1, 0.30f, Ye4, G * 0.36f / 0.45f },
            new object[] { "infernal_boulder_cast_ring", 40, 48, 24, 20, 40, 1, 0.30f, Ye4, G * 0.30f / 0.45f }, new object[] { "infernal_boulder_end_blast", 176, 128, 30, 88, 92, 1, 0.30f, Ye4, G * 0.60f / 0.45f },
            new object[] { "infernal_boulder_fire_patch", 120, 72, 40, 60, 56, 1, 0.18f, Ye4, G * 0.32f / 0.45f },
            // Fire Pillar (unchanged, v1 art)
            new object[] { "fire_pillar_telegraph", 40, 20, 12, 20, 10, 1, 0f, Orange, G }, new object[] { "fire_pillar_erupt", 56, 76, 18, 28, 66, 1, -1f, Orange, G }, new object[] { "fire_pillar_scorch_v1", 36, 16, 5, 18, 8, 1, 0f, Orange, G }, new object[] { "fire_pillar_scorch_v2", 36, 16, 5, 18, 8, 1, 0f, Orange, G },
        };
        static readonly Dictionary<string, Strip> _s = new Dictionary<string, Strip>(); static FireBasicsFX1481 _i; static bool _tried, _ok, _glowOk; static Shader _sh; static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();
        public static string Status = "fireFx1481 not tried"; public static bool ForceNoGlow;
        public static bool Ok { get { if (!_tried) Init(); return _ok && _i != null; } }
        public static bool Glow => _glowOk;
        static Texture2D Tex(string n) { var t = Resources.Load<Texture2D>(Dir + n); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        static Sprite[,] Grid(Texture2D t, int fw, int fh, int cols, int rows, float px, float py)
        {
            var g = new Sprite[rows, cols]; for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++) g[r, c] = Sprite.Create(t, new Rect(c * fw, t.height - (r + 1) * fh, fw, fh), new Vector2(px / fw, 1f - py / fh), PPU, 0, SpriteMeshType.FullRect); return g;
        }
        static Material GlowMat(float add, uint tint, float gain)
        {
            string key = add.ToString("0.000") + "_" + tint.ToString("x6") + "_" + gain.ToString("0.00"); if (_mats.TryGetValue(key, out var m)) return m;
            m = new Material(_sh) { name = "firebasicsglow1481_" + key }; m.SetColor("_Tint", new Color32((byte)(tint >> 16), (byte)(tint >> 8), (byte)tint, 255)); m.SetFloat("_Gain", gain); m.SetFloat("_Add", add); return _mats[key] = m;
        }
        static void Init()
        {
            _tried = true; int got = 0, glow = 0;
            foreach (var sp in Spec)
            {
                string k = (string)sp[0]; int fw = (int)sp[1], fh = (int)sp[2], n = (int)sp[3], rows = (int)sp[6]; float px = (int)sp[4], py = (int)sp[5], gadd = (float)sp[7], gg = (float)sp[9]; uint gt = (uint)sp[8];
                var t = Tex(k + "_strip"); if (t == null || t.width < fw * n || t.height < fh * rows) continue;
                var st = new Strip { n = n, rows = rows, f = Grid(t, fw, fh, n, rows, px, py), add = gadd, gain = gg, tint = gt }; got++;
                if (gadd != 0f) { var tg = Tex(k + "_glow_strip"); if (tg != null && tg.width >= fw * n && tg.height >= fh * rows) { st.g = Grid(tg, fw, fh, n, rows, px, py); glow++; } }
                _s[k] = st;
            }
            if (got == 0) { Status = "fireFx1481: strips missing"; return; }
            _sh = Resources.Load<Shader>("HellordShaders/HellordFrostGlow"); if (_sh == null) _sh = Shader.Find("Hellord/FrostGlow1430");
            var T = HellordTuning.I; _glowOk = !ForceNoGlow && glow > 0 && _sh != null && _sh.isSupported && (T == null || T.fbGlow1481);
            if (_glowOk) { float mul = T != null ? T.fbGlowMul1481 : 1f; foreach (var st in _s.Values) if (st.g != null) st.mat = GlowMat(st.add < 0f ? (T != null ? T.fbGlowAdd1481 : 0.3f) : st.add * mul, st.tint, st.gain); }
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
        static void Put(Pair p, Strip s, int row, int frame, Vector2 pos, int order, float rot, bool flip, float scale = 1f, float alpha = 1f, float sy = -1f)
        {
            frame = Mathf.Clamp(frame, 0, s.n - 1); row = Mathf.Clamp(row, 0, s.rows - 1); var tp = new Vector3(Snap(pos.x), Snap(pos.y), 0f);
            p.sp.enabled = true; p.sp.sprite = s.f[row, frame]; p.sp.transform.position = tp; p.sp.transform.rotation = Quaternion.Euler(0, 0, rot); p.sp.sortingOrder = order; p.sp.flipX = flip; p.sp.color = new Color(1f, 1f, 1f, alpha); p.sp.transform.localScale = new Vector3(scale, sy < 0f ? scale : sy, 1f);
            if (_glowOk && s.g != null && s.mat != null) { p.gl.enabled = true; p.gl.sharedMaterial = s.mat; p.gl.sprite = s.g[row, frame]; p.gl.transform.position = tp; p.gl.transform.rotation = Quaternion.Euler(0, 0, rot); p.gl.sortingOrder = order + 1; p.gl.flipX = flip; p.gl.color = new Color(1f, 1f, 1f, alpha); p.gl.transform.localScale = new Vector3(scale, sy < 0f ? scale : sy, 1f); } else p.gl.enabled = false;
        }
        static void Hide(Pair p) { p.sp.enabled = false; p.gl.enabled = false; }

        /// <summary>Call once per game frame BEFORE any Show(): marks the frame as driven (a paused game keeps the last frame on screen).</summary>
        public static void Frame() { if (Ok) _i._called = true; }
        /// <summary>IMMEDIATE: draw frame `frame` (and `row` of a dir16 strip) this frame only.</summary>
        public static void Show(string key, Vector2 pos, int frame, int order, int row = 0, bool flip = false, float scale = 1f, float alpha = 1f)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return; var a = _i; if (a._imN >= a._im.Count) a._im.Add(a.NewPair()); Put(a._im[a._imN++], s, row, frame, pos, order, 0f, flip, scale, alpha);
        }

        // ------------------------------------------------------------------ one-shots
        sealed class OnceI { public Pair p; public Strip s; public float age, speed, rot, life, sx = 1f, sy = 1f; public int phase, ign = 6; public Vector2 pos; public int order; public bool flip; public string key; }
        readonly List<OnceI> _once = new List<OnceI>();
        /// <summary>ONE-SHOT strip. speed 1 = 30 fps. cap > 0: ignored while that many of the same key are alive (HFX spam guard).</summary>
        /// <remarks>t1514: sx / sy = x / y scale (non-uniform allowed: flattened blast, shrunken pops).</remarks>
        public static bool Once(string key, Vector2 pos, int order, float rot = 0f, float speed = 1f, int cap = 0, bool flip = false, float delayF = 0f, float sx = 1f, float sy = 1f)
        {
            if (!Ok || !_s.TryGetValue(key, out var s)) return false; var a = _i;
            if (cap > 0) { int c = 0; foreach (var o in a._once) if (o.p.live && o.key == key) c++; if (c >= cap) return false; }
            OnceI x = null; foreach (var o in a._once) if (!o.p.live) { x = o; break; } if (x == null) { x = new OnceI { p = a.NewPair() }; a._once.Add(x); }
            x.p.live = true; x.s = s; x.age = -delayF / FPS; x.life = 0f; x.phase = 0; x.sx = sx; x.sy = sy; x.speed = Mathf.Max(0.05f, speed); x.pos = pos; x.order = order; x.rot = rot; x.flip = flip; x.key = key; if (delayF > 0f) Hide(x.p); else Put(x.p, s, 0, 0, pos, order, rot, flip, sx, 1f, sy); return true;
        }

        /// <summary>Burning tile of any HFX band / patch strip: ignite f0-5, 12-frame loop f6-17 with a random start phase, then the die frames (f18..n-1) over the last n-18 frames of its lifetime (lifeS seconds, from the end of delayF). cap > 0: when that many of the same key are alive the oldest starts dying now.</summary>
        public static void Tile(string key, Vector2 pos, int order, float lifeS, float delayF = 0f, int cap = 0, int ign = 6, int phase = -1)
        {
            if (!Ok || !_s.TryGetValue(key, out var s) || s.n < ign + 12 + 4) return; var a = _i; int dieN = s.n - ign - 12;
            if (cap > 0) { int c = 0; OnceI old = null; foreach (var o in a._once) if (o.p.live && o.key == key && o.life > 0f) { c++; if (o.age * FPS + dieN < o.life && (old == null || o.age > old.age)) old = o; } if (c >= cap && old != null) old.life = old.age * FPS + dieN; }
            OnceI x = null; foreach (var o in a._once) if (!o.p.live) { x = o; break; } if (x == null) { x = new OnceI { p = a.NewPair() }; a._once.Add(x); }
            x.p.live = true; x.s = s; x.age = -delayF / FPS; x.sx = 1f; x.sy = 1f; x.speed = 1f; x.pos = pos; x.order = order; x.rot = 0; x.flip = false; x.key = key; x.life = Mathf.Max(ign + dieN, lifeS * FPS); x.ign = ign; x.phase = phase >= 0 ? phase % 12 : Random.Range(0, 12); if (delayF > 0f) Hide(x.p); else Put(x.p, s, 0, 0, pos, order, 0f, false);
        }
        static int TileFrame(OnceI o) { float f = o.age * FPS; int g = o.ign, dieN = o.s.n - g - 12; if (f >= o.life) return -1; if (f < g) return (int)f; if (f >= o.life - dieN) return g + 12 + Mathf.Clamp((int)(f - (o.life - dieN)), 0, dieN - 1); return g + (((int)f - g + o.phase) % 12); } // HFX v4 life_idx(age, T, ign, loop 12, n)

        // ------------------------------------------------------------------ staged floor decals
        public enum DecalKind { PillarScorch, SpitScorch }
        sealed class DecalI { public Pair p; public Strip s; public DecalKind kind; public float f; public Vector2 pos; public int order, row; public int stage = -9; }
        readonly List<DecalI> _dec = new List<DecalI>();
        static readonly float[][] Ends = { new float[] { 30, 46, 58, 70, 78 }, new float[] { 6, 12, 18, 24, 30, 36 } };   // Fire Pillar scorch stages (v1 art, unchanged); HFX v5 Spit scorch: stage = f div 6, 6 stages (gone at b38)
        static readonly int[] Cap = { 12, 8 }, DissolveStage = { 3, 6 };
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
        // ------------------------------------------------------------------ t1511b floor darkening (code-drawn soft dark ellipse, alpha-black; swappable for HFX art via DarkOverride)
        /// <summary>Set this to draw an art version instead (pos, rx px, ry px, peak alpha, hold s, order); the code ellipse is skipped while it is set.</summary>
        public static System.Action<Vector2, float, float, float, float, int> DarkOverride;
        sealed class DarkI { public SpriteRenderer sr; public float age, hold, a0; public bool live; }
        readonly List<DarkI> _dark = new List<DarkI>(); static readonly Dictionary<long, Sprite> _darkSpr = new Dictionary<long, Sprite>();
        static readonly int[] Bayer4 = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
        static Sprite DarkSprite(int rx, int ry, int levels, float falloff)
        {
            long key = ((long)rx << 32) | ((long)ry << 12) | ((long)levels << 4); if (_darkSpr.TryGetValue(key, out var sp)) return sp;
            int w = rx * 2 + 2, h = ry * 2 + 2; var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; var px = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {   // soft ellipse alpha (1-d^2)^falloff, quantized to `levels` steps with a 4x4 ordered dither: 1 texel = 1 art px, so it stays pixel-art
                float dx = (x + 0.5f - w * 0.5f) / rx, dy = (y + 0.5f - h * 0.5f) / ry, d2 = dx * dx + dy * dy; float v = d2 >= 1f ? 0f : Mathf.Pow(1f - d2, falloff);
                int lv = Mathf.Clamp(Mathf.FloorToInt(v * levels + Bayer4[(y & 3) * 4 + (x & 3)] / 16f), 0, levels); px[y * w + x] = new Color32(255, 255, 255, (byte)(255 * lv / Mathf.Max(1, levels)));
            }
            t.SetPixels32(px); t.Apply(false, true); sp = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect); return _darkSpr[key] = sp;
        }
        /// <summary>Brief floor darkening: fades in over fbDarkInF frames, holds holdS, fades out over fbDarkOutS. rx / ry in art px.</summary>
        public static void Dark(Vector2 pos, float rxPx, float ryPx, float alpha, float holdS, int order)
        {
            var T = HellordTuning.I; if (T == null || !T.fbDark1481 || alpha <= 0f) return;
            if (DarkOverride != null) { DarkOverride(pos, rxPx, ryPx, alpha, holdS, order); return; }
            if (!Ok) return; var a = _i; DarkI x = null; foreach (var d in a._dark) if (!d.live) { x = d; break; }
            if (x == null) { x = new DarkI { sr = a.NewSR("fbdark") }; a._dark.Add(x); }
            x.live = true; x.age = 0f; x.hold = holdS; x.a0 = alpha; x.sr.enabled = true; x.sr.sprite = DarkSprite(Mathf.Max(2, Mathf.RoundToInt(rxPx)), Mathf.Max(1, Mathf.RoundToInt(ryPx)), Mathf.Max(1, T.fbDarkLevels1481), T.fbDarkFalloff1481);
            x.sr.transform.position = new Vector3(Snap(pos.x), Snap(pos.y), 0f); x.sr.transform.localScale = Vector3.one; x.sr.transform.rotation = Quaternion.identity; x.sr.sortingOrder = order; x.sr.flipX = false;
            var c = T.fbDarkTint1481; c.a = 0f; x.sr.color = c;
        }
        void UpdateDark(float dt)
        {
            var T = HellordTuning.I; if (T == null) return; float inS = Mathf.Max(0.001f, T.fbDarkInF1481 / FPS), outS = Mathf.Max(0.001f, T.fbDarkOutS1481);
            foreach (var d in _dark)
            {
                if (!d.live) continue; d.age += dt; float k = d.age < inS ? d.age / inS : d.age < inS + d.hold ? 1f : 1f - (d.age - inS - d.hold) / outS;
                if (k <= 0f && d.age > inS) { d.live = false; d.sr.enabled = false; continue; }
                var c = T.fbDarkTint1481; c.a = d.a0 * Mathf.Clamp01(k); d.sr.color = c;
            }
        }
        public static void Clear() { if (_i == null) return; foreach (var d in _i._dark) { d.live = false; d.sr.enabled = false; } foreach (var o in _i._once) { o.p.live = false; Hide(o.p); } foreach (var d in _i._dec) { d.p.live = false; Hide(d.p); } foreach (var p in _i._im) Hide(p); _i._imN = 0; }
        /// <summary>Clears one-shots / immediates only; floor decals keep fading (used on a room reset is Clear()).</summary>
        void LateUpdate()
        {
            if (_called) { for (int i = _imN; i < _im.Count; i++) Hide(_im[i]); _imN = 0; _called = false; }
            float dt = Time.deltaTime; UpdateDark(dt);
            foreach (var o in _once)
            {
                if (!o.p.live) continue; o.age += dt * o.speed; if (o.age < 0f) { Hide(o.p); continue; } int fi = o.life > 0f ? TileFrame(o) : (int)(o.age * FPS); if (fi < 0 || fi >= o.s.n) { o.p.live = false; Hide(o.p); continue; } Put(o.p, o.s, 0, fi, o.pos, o.order, o.rot, o.flip, o.sx, 1f, o.sy);
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
