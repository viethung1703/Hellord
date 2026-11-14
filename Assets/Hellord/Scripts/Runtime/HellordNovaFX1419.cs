using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  t1419: Ice Nova burst (Hellord FX strips burst_back / burst_front / burst_glow_mask, 15 frames of 112x84 @ 30 fps, origin (56,46) = ground centre).
    //   * BACK strip: under every actor (OrdBack).   * FRONT strip (baked 50 % checker): over the actors, y-sorted just in front of the hero's row.
    //   * GLOW: the rim-only mask drawn by the BUILT-IN pipeline shader Hellord/NovaGlow1419 (additive, few px of bloom, 4 Endesga ice blues, plus a
    //     short centre flash on frames 0-1). HAZE: Hellord/NovaHaze1419 (GrabPass heat-haze ripple behind the wave front, frames 0-6).
    //   * Fallbacks: shader missing / unsupported -> strips only (no glow); GrabPass impossible (SRP) -> no haze; strips missing -> the old IceFX3 visual.
    //   * Gameplay link: the hit ring follows the visual front radius curve (Front01) so damage lands as the ring passes the demons. Max radius and damage unchanged.
    //  Toggle: HellordTuning.novaGlow1419 (default on; off = the old t1414 IceFX3 nova). Integer 1x nearest only, strips are never scaled. DRAWS ONLY.
    // =====================================================================================================================
    public class NovaFX1419 : MonoBehaviour
    {
        public const string NBack = "fx_icenova_burst_back", NFront = "fx_icenova_burst_front", NGlow = "fx_icenova_burst_glow";
        // t1420: one strip set per skill level (HFX bigger canvases), set 0 = the single t1419 burst set (fallback), none -> the old IceFX3 visual.
        const float FPS = 30f, PPU = 16f, SQ = 0.6f; const int FullFrame = 5; // frame 5 = full radius (strip data: lvN r = 51/56/61/66/72 px)
        static readonly float[] LvR = { 48f, 51f, 56f, 61f, 66f, 72f };                 // index 0 = single set, 1..5 = level; used when the glow mask cannot be read
        static readonly float[] FallbackFrac = { 0f, .19f, .58f, .86f, .96f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        static readonly float[] GlowI = { 1f, 1f, 1f, 1f, 0.95f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f, 0.12f, 0.06f };
        sealed class Set { public IceArt.Strip back, front, glow; public int fw, fh; public float[] ext; public float ref5; public string name; }
        static readonly Set[] _sets = new Set[6]; // [0] single, [1..5] levels (null = missing)
        static NovaFX1419 _i; static bool _tried, _ok;
        static Shader _glowSh, _hazeSh; static bool _glowOk, _hazeOk; static Material _glowMat; static Sprite _white;
        public static bool ForceNoGlow, ForceNoHaze, ForceSingleSet; // test / fallback switches: strips only, no haze, ignore the per-level sets
        public static string Status = "novafx not tried";

        public static bool Ok
        {
            get
            {
                var T = HellordTuning.I; if (T != null && !T.novaGlow1419) return false;
                if (!_tried) Init(); return _ok && _i != null;
            }
        }
        // canvas (w,h) and origin y (px from the top) per set; origin x = w/2. Single set 112x84 (56,46); lvN from HFX.
        static readonly int[] CW = { 112, 118, 128, 140, 150, 164 }, CH = { 84, 82, 88, 96, 102, 110 }, OY = { 46, 46, 50, 53, 56, 60 };
        static Set Load(int id)
        {
            string pre = id == 0 ? "fx_icenova_burst" : "fx_icenova_burst_lv" + id; int fw = CW[id], fh = CH[id]; float pivY = (fh - OY[id]) / (float)fh;
            var st = new Set { fw = fw, fh = fh, name = pre, back = IceArt.Get(pre + "_back", fw, fh, FPS, false, pivY), front = IceArt.Get(pre + "_front", fw, fh, FPS, false, pivY), glow = IceArt.Get(pre + "_glow", fw, fh, FPS, false, pivY) };
            if (st.back == null || st.front == null) return null;
            int n = Mathf.Min(st.back.f.Length, st.front.f.Length); if (n <= FullFrame) return null; st.ext = new float[n];
            var ge = st.glow != null ? st.glow.f[0].texture : null;
            if (ge != null && ge.isReadable)
            {
                var px = ge.GetPixels32(); int W = ge.width;
                for (int f = 0; f < n; f++) { int m = 0; for (int y = 0; y < fh; y++) for (int x = 0; x < fw; x++) if (px[y * W + f * fw + x].a > 0) m = Mathf.Max(m, Mathf.Abs(x - fw / 2)); st.ext[f] = m; }
            }
            else for (int f = 0; f < n; f++) st.ext[f] = Mathf.Round(LvR[id] * FallbackFrac[Mathf.Min(f, FallbackFrac.Length - 1)]);
            st.ref5 = Mathf.Max(1f, st.ext[FullFrame]); return st;
        }
        static void Init()
        {
            _tried = true; int cnt = 0; string have = "";
            for (int id = 0; id < 6; id++) { _sets[id] = Load(id); if (_sets[id] != null) { cnt++; have += id + ":" + _sets[id].ref5 + "px "; } }
            if (cnt == 0) { Status = "novafx strips missing -> old visual"; return; }
            _glowSh = Resources.Load<Shader>("HellordShaders/HellordNovaGlow"); if (_glowSh == null) _glowSh = Shader.Find("Hellord/NovaGlow1419");
            _hazeSh = Resources.Load<Shader>("HellordShaders/HellordNovaHaze"); if (_hazeSh == null) _hazeSh = Shader.Find("Hellord/NovaHaze1419");
            bool srp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            _glowOk = !ForceNoGlow && _glowSh != null && _glowSh.isSupported; _hazeOk = !ForceNoHaze && !srp && _hazeSh != null && _hazeSh.isSupported;
            if (_glowOk) _glowMat = new Material(_glowSh) { name = "novaglow1419" };
            var tx = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; tx.SetPixel(0, 0, Color.white); tx.Apply(false, true); _white = Sprite.Create(tx, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            var go = new GameObject("NovaFX1419"); if (Application.isPlaying) DontDestroyOnLoad(go); _i = go.AddComponent<NovaFX1419>(); _ok = true;
            Status = "novafx ok: sets " + have + "| glow " + (_glowOk ? "shader" : "OFF(" + (ForceNoGlow ? "forced" : _glowSh == null ? "missing" : "unsupported") + ")") + ", haze " + (_hazeOk ? "grab" : srp ? "off(SRP)" : "off(" + (_hazeSh == null ? "missing" : ForceNoHaze ? "forced" : "unsupported") + ")");
        }

        // ------------------------------------------------------------------ set choice + gameplay link (read-only curve)
        /// <summary>Strip set for a hit radius (px): the level set whose full radius is nearest (so lv N with no area bonus gets lvN's strips); falls back to the single set; -1 = none (old visual).</summary>
        public static int Pick(float rMaxPx)
        {
            if (!Ok) return -1; int best = -1; float bd = 1e9f;
            if (!ForceSingleSet) for (int id = 1; id <= 5; id++) if (_sets[id] != null) { float d = Mathf.Abs(_sets[id].ref5 - rMaxPx); if (d < bd) { bd = d; best = id; } }
            return best >= 0 ? best : (_sets[0] != null ? 0 : -1);
        }
        /// <summary>Visual front radius 0..1 (of the full radius, frame 5) at game time t seconds since the cast; linear between strip frames.</summary>
        public static float Front01(int set, float tSec) { if (set < 0 || _sets[set] == null) return 1f; var e = _sets[set].ext; float f = Mathf.Max(0f, tSec * FPS); int i = Mathf.Min((int)f, e.Length - 1), j = Mathf.Min(i + 1, e.Length - 1); return Mathf.Clamp01(Mathf.Lerp(e[i], e[j], f - i) / _sets[set].ref5); }
        /// <summary>Seconds until the visual front reaches its full radius (frame 5) = the length of the hit ring.</summary>
        public static float FullSec(int set) { return FullFrame / FPS; }
        /// <summary>Full visual radius of a set in px (for tests / logs).</summary>
        public static float FullPx(int set) { return set >= 0 && _sets[set] != null ? _sets[set].ref5 : 0f; }

        // ------------------------------------------------------------------ instances
        sealed class Inst { public object key; public Vector2 pos; public float age; public bool driven; public int order, set; public SpriteRenderer back, front, glow, haze; public Material hazeMat; public bool live; }
        readonly List<Inst> _all = new List<Inst>();
        static SpriteRenderer NewSR(Transform p, string n) { var go = new GameObject(n); go.transform.SetParent(p, false); return go.AddComponent<SpriteRenderer>(); }
        Inst Take()
        {
            foreach (var x in _all) if (!x.live) { x.live = true; return x; }
            var s = new Inst { live = true }; s.back = NewSR(transform, "nb"); s.front = NewSR(transform, "nf"); s.glow = NewSR(transform, "ng"); s.haze = NewSR(transform, "nh");
            if (_glowOk) s.glow.sharedMaterial = _glowMat; if (_hazeOk) { s.hazeMat = new Material(_hazeSh) { name = "novahaze1419" }; s.haze.sharedMaterial = s.hazeMat; s.haze.sprite = _white; }
            _all.Add(s); return s;
        }
        /// <summary>Every frame while the hit ring lives: keeps the burst on game time t (seconds since the cast). `ground` = hero feet, `frontOrder` = sorting order of the front strip. First call creates the burst; after the ring is gone it plays out on its own.</summary>
        public static void Drive(object key, Vector2 ground, float tSec, int frontOrder, int set)
        {
            if (!Ok || set < 0 || _sets[set] == null) return; Inst s = null; foreach (var x in _i._all) if (x.live && x.key == key) { s = x; break; }
            if (s == null) { s = _i.Take(); s.key = key; s.pos = ground; s.set = set; }
            s.age = tSec * FPS; s.driven = true; s.order = frontOrder;
        }
        public static void Clear() { if (_i == null) return; foreach (var x in _i._all) Hide(x); }
        static void Hide(Inst s) { s.live = false; s.key = null; s.back.enabled = s.front.enabled = s.glow.enabled = s.haze.enabled = false; }
        static float Snap(float u) => Mathf.Round(u * PPU) / PPU;
        void LateUpdate() { Tick(Time.deltaTime * FPS); }
        public static void EditorStep() { if (Ok) _i.Tick(0f); }
        public static void EditorReset() { Clear(); }
        void Tick(float dFrames)
        {
            foreach (var s in _all)
            {
                if (!s.live) continue; if (!s.driven) s.age += dFrames; s.driven = false;
                var st = _sets[s.set]; var _ext = st.ext; int n = _ext.Length; if (s.age >= n) { Hide(s); continue; }
                int fi = Mathf.Clamp((int)s.age, 0, n - 1); var p = new Vector3(Snap(s.pos.x), Snap(s.pos.y), 0);
                s.back.enabled = true; s.back.sprite = st.back.f[fi]; s.back.transform.position = p; s.back.sortingOrder = IceFX3.OrdBack;
                s.front.enabled = true; s.front.sprite = st.front.f[fi]; s.front.transform.position = p; s.front.sortingOrder = s.order;
                if (_glowOk && st.glow != null) { s.glow.enabled = true; s.glow.sprite = st.glow.f[Mathf.Min(fi, st.glow.f.Length - 1)]; s.glow.transform.position = p; s.glow.sortingOrder = s.order + 3; float fl = fi == 0 ? 1f : fi == 1 ? 0.6f : 0f; s.glow.color = new Color(fl, 0f, 0f, GlowI[Mathf.Min(fi, GlowI.Length - 1)]); }
                if (_hazeOk && s.age < 7f)
                {
                    float f = Mathf.Max(0f, s.age); int i0 = Mathf.Min((int)f, n - 1), i1 = Mathf.Min(i0 + 1, n - 1); float front = Mathf.Lerp(_ext[i0], _ext[i1], f - i0);
                    s.haze.enabled = true; s.haze.transform.position = p; s.haze.sortingOrder = s.order + 2; s.haze.transform.localScale = new Vector3(2f * (front + 14f) / PPU, 2f * ((front + 14f) * SQ + 4f) / PPU, 1f);
                    s.hazeMat.SetVector("_Center", new Vector4(p.x, p.y, 0, 0)); s.hazeMat.SetFloat("_FrontPx", front); s.hazeMat.SetFloat("_Width", Mathf.Max(6f, front * 0.45f)); s.hazeMat.SetFloat("_Amp", 3f * (1f - s.age / 7f)); s.hazeMat.SetFloat("_Squash", SQ); s.hazeMat.SetFloat("_PPU", PPU);
                }
                else s.haze.enabled = false;
            }
        }
    }
}
