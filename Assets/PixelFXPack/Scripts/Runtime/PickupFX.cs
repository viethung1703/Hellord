using System;
using UnityEngine;

namespace PixelFXPack
{
    public enum PickupKind { GemSmall = 0, GemMedium = 1, GemLarge = 2, Bone = 3 }

    /// <summary>
    /// 128-131. PICKUPS + LEVEL-UP (v2.0.0). Reward language only: cyan / green / gold gems and bone-white bones, never red, no hazard stripes, no blinking fills.
    /// 128 Drop(pos, kind, value): bobbing 5x5 plus-gem (large: 7x7 faceted pale-yellow octagon, white core, periodic 3x3 sparkle; bone 9x5) with a dark outline, no part under 3 px. Hundreds at once: one pooled mesh (1 draw call), ~6 quads each; pops / level-up / bar use a second small mesh above the hero (2 draw calls total).
    ///     Pool full (MaxPickups) -> MergeWhenFull adds the value to the nearest same-kind pickup and returns ITS handle (Vampire Survivors / Brotato cap); otherwise -1.
    /// 129 Magnet: SetTarget(heroPos) every frame; pickups inside AutoMagnetRadiusPx (or Magnet(h) / MagnetAll(radius) / Vacuum()) hop out 4 px, then accelerate in
    ///     with a 2-dot low-alpha trail. Collect within CollectRadiusPx -> 0.18 s pop (white core, 3 px ring in the gem colour, 4 sparks). No ring larger than 10 px.
    /// 130 XP bar (optional, ShowBar): 6 px tall bar inset from the top of the view; gains flash white at the leading edge; level-up turns it gold for 0.4 s.
    /// 131 LevelUp(feet): 0.5 s, LOW alpha (LevelUpAlpha 0.5): two thin-free 3 px rings (white then gold) + 10 rising 3x3 sparks + a 11x13 stepped up-arrow (3 px parts) on a dark plate
    ///     over the head. No pillar, no full-screen flash, so enemies and tells stay readable.
    /// SFX: OnPickupSfx(kind, pitch, batched) is rate-limited (MinSfxInterval 0.045 s, extra pickups batched into the next call); pitch steps +1 semitone per
    ///     emitted SFX call in a chain (so a batched burst steps once) (ChainWindow 0.35 s), capped at ChainMaxSemitones 12, resets after a gap. OnCollect(kind, value) fires for EVERY pickup (game logic).
    ///     OnLevelUp(level) at the burst start (stinger); OnBarFull just before it.
    /// Visual only, 2 draw calls (pickups layer + top layer; +1 when SetDropSprites is used), no per-frame alloc, int handles. Positions in world units (16 PPU).
    /// </summary>
    public static class PickupFX
    {
        static PickupRunner R => PickupRunner.I;
        public static int MaxPickups = 600; public static bool MergeWhenFull = true;
        public static float AutoMagnetRadiusPx = 40f, CollectRadiusPx = 6f, MagnetAccelPx = 900f, MagnetMaxSpeedPx = 420f, HopPx = 4f;
        public static float MinSfxInterval = 0.045f, ChainWindow = 0.35f; public static int ChainMaxSemitones = 12;
        public static float LevelUpSeconds = 0.5f, LevelUpAlpha = 0.5f, LevelUpRadiusPx = 44f;
        public static bool ShowBar = true; public static float BarWidthPx = 220f, BarInsetPx = 24f;
        public static Material MaterialOverride; public static int SortingOrder = 20, SortingOrderTop = 74; public static string SortingLayer = "Default";
        public static Action<PickupKind, int> OnCollect;
        public static Action<PickupKind, float, int> OnPickupSfx;
        public static Action<int> OnLevelUp; public static Action OnBarFull;

        public static void Prewarm(int capacity = 600) => R.Prewarm(capacity);
        public static int Drop(Vector2 pos, PickupKind kind = PickupKind.GemSmall, int value = 1) => R.Drop(pos * HellFX.PPU, kind, value);
        public static void SetTarget(Vector2 heroPos) => R.Target = heroPos * HellFX.PPU;
        public static void Magnet(int h) => R.Magnet(h);
        public static int MagnetAll(float radiusPx) => R.MagnetAll(radiusPx);
        public static int Vacuum() => R.MagnetAll(1e6f);
        /// <summary>XP bar fill 0..1 (game owns the numbers).</summary>
        public static void SetBar(float fill01) => R.SetBar(fill01);
        public static void LevelUp(Vector2 feet, int newLevel = 0) => R.LevelUp(feet * HellFX.PPU, newLevel);
        /// <summary>v2.0.1: draw this kind with external sprites (e.g. your own blood-drop art) instead of the code gem. frames = animation (fps), all
        /// frames of all kinds must share ONE texture (pack them in a Unity SpriteAtlas, Packing = Full Rect, or one sheet). If a new set uses a different
        /// texture and every texture is Read/Write enabled, the runner repacks them once into its own atlas; otherwise it logs a warning and returns false
        /// (that kind keeps the code gem). Sprites render at native pixel size (scaled by HellFX.PPU / sprite PPU), pivot on the pickup position.
        /// Still pooled: all sprite pickups go in ONE extra mesh (3 draw calls total with the top layer). Bob, magnet, trail, pop and merge are unchanged.</summary>
        public static bool SetDropSprites(PickupKind kind, Sprite[] frames, float fps = 8f) => R.SetSprites(kind, frames, fps);
        public static bool SetDropSprite(PickupKind kind, Sprite sprite) => R.SetSprites(kind, sprite != null ? new[] { sprite } : null, 0f);
        /// <summary>Back to the built-in code gem/bone for this kind (the default).</summary>
        public static void ClearDropSprites(PickupKind kind) => R.SetSprites(kind, null, 0f);
        /// <summary>Trail / pop colour per kind (defaults: cyan, green, pale yellow, bone). Set e.g. to a pale pink when using red blood-drop art.</summary>
        public static void SetKindColor(PickupKind kind, Color32 c) => R.SetColor(kind, c);
        public static void Remove(int h) => R.Remove(h);
        public static void Clear() => R.ClearAll();
        public static int AliveCount => PickupRunner.Exists ? R.Count : 0;
    }

    public class PickupRunner : MonoBehaviour
    {
        static PickupRunner _i;
        public static bool Exists => _i != null;
        public static PickupRunner I { get { if (_i == null) { var go = new GameObject("PickupFX"); DontDestroyOnLoad(go); _i = go.AddComponent<PickupRunner>(); } return _i; } }

        struct It { public bool used, mag; public PickupKind k; public int v; public Vector2 p, vel, t1, t2; public float age, mt, seed; public ushort gen; }
        struct Pop { public Vector2 p; public float t; public byte k; }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }
        readonly Pop[] _pop = new Pop[128]; int _popN;
        public Vector2 Target;
        float _bar, _barShown, _barFlash = 9, _gold = 9, _lt = 9, _sfxT = 9, _chainT = 9; int _chain = -1, _batched, _lastKind; Vector2 _lp;

        const int MaxQ = 12000;
        struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _lo = new Q[MaxQ], _hi = new Q[2048]; int _nl, _nh, _last;
        Mesh _mesh, _mesh2; Vector3[] _vtx2; Color32[] _col2; int _last2; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255), Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Cb = new Color32(0x00, 0x99, 0xdb, 255),
            Gr = new Color32(0x63, 0xc7, 0x4d, 255), Gd = new Color32(0x3e, 0x89, 0x48, 255), Y = new Color32(0xfe, 0xe7, 0x61, 255), G = new Color32(0xfe, 0xae, 0x34, 255),
            Ds = new Color32(0xea, 0xd4, 0xaa, 255), Wd = new Color32(0xc2, 0x85, 0x69, 255), Sd = new Color32(0x3a, 0x44, 0x66, 255);
        static readonly Color32[] _kc = { new Color32(0x2c, 0xe8, 0xf5, 255), new Color32(0x63, 0xc7, 0x4d, 255), new Color32(0xfe, 0xe7, 0x61, 255), new Color32(0xea, 0xd4, 0xaa, 255) };
        static Color32 Main(PickupKind k) => _kc[(int)k];
        public void SetColor(PickupKind k, Color32 c) { c.a = 255; _kc[(int)k] = c; }
        // ---- v2.0.1 external drop sprites ----
        class SprSet { public Sprite[] src; public Rect[] uv; public Vector2[] size, piv; public float fps; }
        readonly SprSet[] _ss = new SprSet[4]; Texture2D _sprTex, _packed; Mesh _mesh3; MeshRenderer _mr3; Material _mat3;
        Vector3[] _vtx3; Vector2[] _uv3; Color32[] _col3; int _n3, _last3; const int MaxS = 2048;
        public bool SetSprites(PickupKind k, Sprite[] fr, float fps)
        {
            if (_mesh == null) Prewarm(PickupFX.MaxPickups);
            if (fr == null || fr.Length == 0) { _ss[(int)k] = null; return true; }
            foreach (var sp in fr) if (sp == null) { Debug.LogWarning("[PixelFX] PickupFX.SetDropSprites: null frame"); return false; }
            var keep = _ss[(int)k]; _ss[(int)k] = new SprSet { src = fr, fps = fps };
            if (!Rebuild()) { _ss[(int)k] = keep; Rebuild(); return false; }
            return true;
        }
        bool Rebuild()
        {
            Texture2D shared = null; bool same = true, any = false;
            foreach (var st in _ss) if (st != null) foreach (var sp in st.src) { any = true; if (shared == null) shared = sp.texture; else if (sp.texture != shared) same = false; }
            if (!any) return true;
            Texture2D tex = shared; Rect[] packed = null;
            if (!same)
            {   // repack once (allocates; registration-time only). Needs Read/Write on every source texture.
                int n = 0; foreach (var st in _ss) if (st != null) n += st.src.Length;
                var parts = new Texture2D[n]; int q = 0;
                foreach (var st in _ss) if (st != null) foreach (var sp in st.src)
                {
                    if (!sp.texture.isReadable) { Debug.LogWarning("[PixelFX] PickupFX: drop sprites use different textures and '" + sp.texture.name + "' is not Read/Write. Put them in one SpriteAtlas (Full Rect) or enable Read/Write. Keeping the code gem."); return false; }
                    var r = sp.textureRect; var t = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.SetPixels(sp.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height)); t.Apply(); parts[q++] = t;
                }
                if (_packed == null) _packed = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "PFX Pickup Atlas", filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave };
                packed = _packed.PackTextures(parts, 1, 2048); _packed.filterMode = FilterMode.Point; tex = _packed;
                foreach (var t in parts) Destroy(t);
            }
            int pi = 0;
            foreach (var st in _ss) if (st != null)
            {
                int n = st.src.Length; st.uv = new Rect[n]; st.size = new Vector2[n]; st.piv = new Vector2[n];
                for (int j = 0; j < n; j++)
                {
                    var sp = st.src[j]; float sc = HellFX.PPU / sp.pixelsPerUnit;
                    st.size[j] = sp.rect.size * sc; st.piv[j] = sp.pivot * sc;
                    if (packed != null) st.uv[j] = packed[pi++];
                    else { var r = sp.textureRect; st.uv[j] = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height); }
                }
            }
            _sprTex = tex;
            if (_mesh3 == null)
            {
                _vtx3 = new Vector3[MaxS * 4]; _uv3 = new Vector2[MaxS * 4]; _col3 = new Color32[MaxS * 4]; var tri = new int[MaxS * 6]; Array.Copy(_tri, tri, MaxS * 6);
                for (int i = 0; i < _col3.Length; i++) _col3[i] = Wh;
                var go = new GameObject("PickupFX Sprites"); go.transform.SetParent(transform, false);
                _mesh3 = new Mesh { name = "PickupFX Sprites" }; _mesh3.MarkDynamic(); go.AddComponent<MeshFilter>().sharedMesh = _mesh3; _mr3 = go.AddComponent<MeshRenderer>();
                var baseM = GetComponent<MeshRenderer>().sharedMaterial;
                if (baseM != null) { _mat3 = new Material(baseM) { name = "PickupFX Sprites (Shared)", hideFlags = HideFlags.DontSave }; _mr3.sharedMaterial = _mat3; }
                _mr3.sortingOrder = PickupFX.SortingOrder; _mr3.sortingLayerName = PickupFX.SortingLayer;
                _mr3.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; _mr3.receiveShadows = false;
                _mesh3.vertices = _vtx3; _mesh3.uv = _uv3; _mesh3.colors32 = _col3; _mesh3.triangles = tri;
            }
            if (_mat3 != null) _mat3.mainTexture = _sprTex;
            return true;
        }
        bool DrawSprite(ref It it, float bob)
        {
            var st = _ss[(int)it.k]; if (st == null || st.uv == null) return false;
            if (_n3 >= MaxS) return true;
            int f = st.fps > 0 ? (int)((it.age + it.seed) * st.fps) % st.uv.Length : 0;
            float u = 1f / HellFX.PPU, x0 = Mathf.Round(it.p.x - st.piv[f].x), y0 = Mathf.Round(it.p.y + bob - st.piv[f].y), x1 = x0 + st.size[f].x, y1 = y0 + st.size[f].y;
            int v = _n3++ * 4; var r = st.uv[f];
            _vtx3[v] = new Vector3(x0 * u, y0 * u); _vtx3[v + 1] = new Vector3(x0 * u, y1 * u); _vtx3[v + 2] = new Vector3(x1 * u, y1 * u); _vtx3[v + 3] = new Vector3(x1 * u, y0 * u);
            _uv3[v] = new Vector2(r.xMin, r.yMin); _uv3[v + 1] = new Vector2(r.xMin, r.yMax); _uv3[v + 2] = new Vector2(r.xMax, r.yMax); _uv3[v + 3] = new Vector2(r.xMax, r.yMin);
            return true;
        }
        static Color32 Dark(PickupKind k) => k == PickupKind.GemSmall ? Cb : k == PickupKind.GemMedium ? Gd : k == PickupKind.GemLarge ? Wd : Wd;
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(255 * a), 0, 255); return c; }

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 16, 2000);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || !_it[i].used) _free[_freeN++] = i; _it = n;
            }
            if (_mesh != null) return;
            int nq = MaxQ;
            _vtx = new Vector3[nq * 4]; _uv = new Vector2[nq * 4]; _col = new Color32[nq * 4]; _tri = new int[nq * 6];
            for (int i = 0; i < nq; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "PickupFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Pickup White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = PickupFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "PickupFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] PickupFX: no sprite shader found; set PickupFX.MaterialOverride.");
            // Two meshes, 2 draw calls total: pickups + trails at SortingOrder (under enemies/hero); pops, level-up and XP bar at SortingOrderTop.
            int n2 = 2048; _vtx2 = new Vector3[n2 * 4]; _col2 = new Color32[n2 * 4]; var uv2 = new Vector2[n2 * 4]; var tri2 = new int[n2 * 6];
            Array.Copy(_uv, uv2, n2 * 4); Array.Copy(_tri, tri2, n2 * 6);
            var top = new GameObject("PickupFX Top"); top.transform.SetParent(transform, false);
            _mesh2 = new Mesh { name = "PickupFX Top" }; _mesh2.MarkDynamic(); top.AddComponent<MeshFilter>().sharedMesh = _mesh2; var mr2 = top.AddComponent<MeshRenderer>();
            if (m != null) mr2.sharedMaterial = m; mr2.sortingOrder = PickupFX.SortingOrderTop; mr2.sortingLayerName = PickupFX.SortingLayer;
            mr2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr2.receiveShadows = false;
            _mesh2.vertices = _vtx2; _mesh2.uv = uv2; _mesh2.colors32 = _col2; _mesh2.triangles = tri2;
            mr.sortingOrder = PickupFX.SortingOrder; mr.sortingLayerName = PickupFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }
        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].used && _it[i].gen == (ushort)(h >> 16); }
        public int Drop(Vector2 p, PickupKind k, int v)
        {
            if (_mesh == null) Prewarm(PickupFX.MaxPickups);
            if (_freeN == 0 || Count >= PickupFX.MaxPickups)
            {
                if (!PickupFX.MergeWhenFull) return -1;
                int best = -1; float bd = 1e18f;
                for (int j = 0; j < _it.Length; j++) { if (!_it[j].used || _it[j].mag || (_it[j].k == PickupKind.Bone) != (k == PickupKind.Bone)) continue; float d = (_it[j].p - p).sqrMagnitude; if (d < bd) { bd = d; best = j; } }
                if (best < 0) return -1; ref var b = ref _it[best]; b.v += Mathf.Max(1, v);
                if (b.k != PickupKind.Bone) b.k = b.v >= 10 ? PickupKind.GemLarge : b.v >= 3 ? PickupKind.GemMedium : b.k;
                return Hd(best);
            }
            int i = _free[--_freeN]; ref var it = ref _it[i];
            it.gen = (ushort)((it.gen + 1) & 0x7FFF); it.used = true; it.mag = false; it.k = k; it.v = Mathf.Max(1, v); it.p = p; it.t1 = it.t2 = p; it.vel = Vector2.zero; it.age = 0; it.seed = (i * 0.618f) % 1f; Count++;
            return Hd(i);
        }
        public void Magnet(int h) { if (Get(h, out int i)) StartMag(ref _it[i]); }
        void StartMag(ref It it) { if (it.mag) return; it.mag = true; it.mt = 0; Vector2 d = it.p - Target; it.vel = (d.sqrMagnitude > 0.01f ? d.normalized : Vector2.up) * PickupFX.HopPx * 12f; }
        public int MagnetAll(float r) { int n = 0; float r2 = r * r; for (int i = 0; i < _it.Length; i++) if (_it[i].used && !_it[i].mag && (_it[i].p - Target).sqrMagnitude < r2) { StartMag(ref _it[i]); n++; } return n; }
        public void Remove(int h) { if (Get(h, out int i)) Free(i); }
        void Free(int i) { _it[i].used = false; _free[_freeN++] = i; Count--; }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].used) Free(i); _popN = 0; }
        public void SetBar(float f) { if (_mesh == null) Prewarm(PickupFX.MaxPickups); f = Mathf.Clamp01(f); if (f > _bar + 0.0001f) _barFlash = 0; _bar = f; }
        public void LevelUp(Vector2 feet, int lvl) { if (_mesh == null) Prewarm(PickupFX.MaxPickups); PickupFX.OnBarFull?.Invoke(); _lp = feet; _lt = 0; _gold = 0; _bar = 0; _barShown = 1; PickupFX.OnLevelUp?.Invoke(lvl); }

        void L(float x, float y, float w, float h, Color32 c) { if (_nl < MaxQ) _lo[_nl++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        void Hi(float x, float y, float w, float h, Color32 c) { if (_nh < 2048) _hi[_nh++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        void Dot(Vector2 p, Color32 c) => Hi(p.x - 1, p.y - 1, 3, 3, c);
        void Ring(Vector2 c0, float r, Color32 c, float sq = 1f) { int n = Mathf.Max(10, Mathf.CeilToInt(2 * Mathf.PI * r / 2.5f)); for (int j = 0; j < n; j++) { float an = j * 6.2832f / n; Dot(c0 + new Vector2(Mathf.Cos(an) * r, Mathf.Sin(an) * r * sq), c); } }

        void DrawPickup(ref It it, float bob)
        {
            if (DrawSprite(ref it, bob)) return;
            float x = Mathf.Round(it.p.x), y = Mathf.Round(it.p.y + bob); Color32 m = Main(it.k);
            if (it.k == PickupKind.Bone)
            {   /* 9x5 bone: two 3x5 knobs + 3x3 shaft on an 11x7 dark plate */
                L(x - 5, y - 3, 11, 7, Ko); L(x - 4, y - 2, 3, 5, Ds); L(x + 2, y - 2, 3, 5, Ds); L(x - 1, y - 1, 3, 3, Ds);
            }
            else if (it.k == PickupKind.GemLarge)
            {   /* 7x7 faceted octagon, pale yellow, gold lower-right facet, 3x3 white core, 9x9 dark outline; 3x3 sparkle on a dark backing every 1.6 s */
                L(x - 4, y - 2, 9, 5, Ko); L(x - 2, y - 4, 5, 9, Ko); L(x - 3, y - 3, 7, 7, Ko);
                L(x - 3, y - 1, 7, 3, Y); L(x - 1, y - 3, 3, 7, Y); L(x - 2, y - 2, 5, 5, Y);
                L(x, y - 3, 3, 3, G); L(x - 1, y - 1, 3, 3, Wh);
                float g = (it.age + it.seed * 3f) % 1.6f; if (g < 0.15f) { L(x + 3, y + 3, 5, 5, Ko); L(x + 4, y + 4, 3, 3, Wh); }
            }
            else
            {   /* 5x5 plus-gem (3 px arms, no 1 px tips) on a 7x7 dark outline; no sub-3px glint */
                L(x - 3, y - 2, 7, 5, Ko); L(x - 2, y - 3, 5, 7, Ko);
                L(x - 1, y - 2, 3, 5, m); L(x - 2, y - 1, 5, 3, m);
            }
        }
        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, u = 1f / HellFX.PPU; _nl = _nh = 0; _n3 = 0; _sfxT += dt; _chainT += dt;
            float am = PickupFX.AutoMagnetRadiusPx, am2 = am * am, cr2 = PickupFX.CollectRadiusPx * PickupFX.CollectRadiusPx;
            for (int i = 0; i < _it.Length; i++)
            {
                ref var it = ref _it[i]; if (!it.used) continue; it.age += dt;
                if (!it.mag && am > 0 && (it.p - Target).sqrMagnitude < am2) StartMag(ref it);
                if (it.mag)
                {
                    it.mt += dt; Vector2 to = Target - it.p; float dist = to.magnitude;
                    if (dist * dist < cr2 || it.mt > 3f) { Collect(ref it); Free(i); continue; }
                    if (it.mt > 0.08f) it.vel = Vector2.ClampMagnitude(it.vel * 0.85f + to / dist * PickupFX.MagnetAccelPx * it.mt * dt * 4f, PickupFX.MagnetMaxSpeedPx);
                    else it.vel *= 0.8f;
                    it.t2 = it.t1; it.t1 = it.p; it.p += it.vel * dt;
                    if (dist < 6f + it.vel.magnitude * dt) { Collect(ref it); Free(i); continue; }
                    Color32 c = Main(it.k); L(it.t2.x - 1, it.t2.y - 1, 3, 3, Al(c, 0.35f)); L(it.t1.x - 1, it.t1.y - 1, 3, 3, Al(c, 0.6f));
                    DrawPickup(ref it, 0);
                }
                else DrawPickup(ref it, Mathf.Round(Mathf.Sin(it.age * 4f + it.seed * 6.28f)));
            }
            // pops
            for (int j = _popN - 1; j >= 0; j--)
            {
                ref var p = ref _pop[j]; p.t += dt; float s = p.t / 0.18f; if (s >= 1) { _pop[j] = _pop[--_popN]; continue; }
                Color32 c = Main((PickupKind)p.k);
                if (s < 0.3f) Dot(p.p, Wh);
                Ring(p.p, 3 + 7 * s, Al(c, 1 - s * 0.6f));
                for (int k = 0; k < 4; k++) { float an = k * 1.5708f + 0.785f; Dot(p.p + new Vector2(Mathf.Cos(an), Mathf.Sin(an)) * (4 + 10 * s) + new Vector2(0, 6 * s), Al(k % 2 == 0 ? Wh : c, 1 - s)); }
            }
            // level-up: brief, low alpha
            if (_lt < PickupFX.LevelUpSeconds)
            {
                _lt += dt; float s = Mathf.Clamp01(_lt / PickupFX.LevelUpSeconds), a = PickupFX.LevelUpAlpha * (1 - s * s), R = PickupFX.LevelUpRadiusPx;
                Ring(_lp + new Vector2(0, 2), 8 + R * (1 - (1 - s) * (1 - s)), Al(Wh, a), 0.6f);
                if (s > 0.15f) { float s2 = (s - 0.15f) / 0.85f; Ring(_lp + new Vector2(0, 2), 6 + R * 0.7f * (1 - (1 - s2) * (1 - s2)), Al(Y, a), 0.6f); }
                for (int k = 0; k < 10; k++) { float x = (k - 4.5f) * 5f, h = 6 + 40 * s * (0.6f + 0.4f * ((k * 7) % 5) / 4f); Dot(_lp + new Vector2(x, h), Al(k % 2 == 0 ? Y : Wh, a * 1.4f)); }
                if (s < 0.9f) { Vector2 ic = _lp + new Vector2(0, 40 + 6 * s); float ax = Mathf.Round(ic.x) - 3, ay = Mathf.Round(ic.y);   // 7x9 up arrow, 3 px stem, on a dark plate (full alpha so it stays readable)
                    Hi(ax - 4, ay - 2, 15, 17, Ko); Hi(ax + 2, ay, 3, 4, Y); Hi(ax - 2, ay + 4, 11, 3, Y); Hi(ax, ay + 7, 7, 3, Y); Hi(ax + 2, ay + 10, 3, 3, Wh); }
            }
            // XP bar
            if (PickupFX.ShowBar)
            {
                var cam = Camera.main;
                if (cam != null && cam.orthographic)
                {
                    _barFlash += dt; _gold += dt;
                    _barShown = Mathf.MoveTowards(_barShown, _bar, dt * (_barShown > _bar ? 2.5f : 1.5f));
                    float hh = cam.orthographicSize * HellFX.PPU; Vector2 cp = (Vector2)cam.transform.position * HellFX.PPU;
                    float w = PickupFX.BarWidthPx, x0 = Mathf.Round(cp.x - w * 0.5f), y0 = Mathf.Round(cp.y + hh - PickupFX.BarInsetPx);
                    Hi(x0 - 2, y0 - 2, w + 4, 10, Ko); Hi(x0, y0, w, 6, Sd);
                    float fw = Mathf.Round(w * _barShown); bool gold = _gold < 0.4f;
                    if (fw > 0) { Hi(x0, y0, fw, 6, gold ? G : Cy); Hi(x0, y0 + 3, fw, 3, gold ? Y : Wh); }
                    if (_barFlash < 0.2f && fw > 0) Hi(x0 + fw - 3, y0 - 1, 3, 8, Wh);
                }
            }
            int q = 0;
            for (int b = 0; b < _nl; b++) Write(_vtx, _col, q++, ref _lo[b], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
            q = 0;
            for (int f = 0; f < _nh; f++) Write(_vtx2, _col2, q++, ref _hi[f], u);
            for (int z = q; z < _last2; z++) { int v = z * 4; _vtx2[v] = _vtx2[v + 1] = _vtx2[v + 2] = _vtx2[v + 3] = Vector3.zero; }
            _last2 = q; _mesh2.vertices = _vtx2; _mesh2.colors32 = _col2; _mesh2.RecalculateBounds();
            if (_mesh3 != null)
            {
                for (int z = _n3; z < _last3; z++) { int v = z * 4; _vtx3[v] = _vtx3[v + 1] = _vtx3[v + 2] = _vtx3[v + 3] = Vector3.zero; }
                _last3 = _n3; _mesh3.vertices = _vtx3; _mesh3.uv = _uv3; _mesh3.RecalculateBounds();
            }
        }
        void Collect(ref It it)
        {
            if (_popN < _pop.Length) _pop[_popN++] = new Pop { p = Target + new Vector2(0, 4), t = 0, k = (byte)it.k };
            PickupFX.OnCollect?.Invoke(it.k, it.v);
            if (_chainT >= PickupFX.ChainWindow) _chain = -1; _chainT = 0; _batched++; _lastKind = (int)it.k;
            if (_sfxT >= PickupFX.MinSfxInterval) { _chain = Mathf.Min(_chain + 1, PickupFX.ChainMaxSemitones); _sfxT = 0; PickupFX.OnPickupSfx?.Invoke((PickupKind)_lastKind, Mathf.Pow(2f, _chain / 12f), _batched); _batched = 0; }
        }
        void Write(Vector3[] _vtx, Color32[] _col, int q, ref Q s, float u)
        {
            int v = q * 4; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
            _vtx[v] = new Vector3(x0, y0, 0); _vtx[v + 1] = new Vector3(x0, y1, 0); _vtx[v + 2] = new Vector3(x1, y1, 0); _vtx[v + 3] = new Vector3(x1, y0, 0);
            _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
