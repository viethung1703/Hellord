using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="GossipHookFX"/> item.</summary>
    public enum GHKind { None = 0, Shout = 1, Scramble = 2, HookWindup = 3 }
    /// <summary>Phase of a <see cref="GossipHookFX"/> item.</summary>
    public enum GHPhase { None = 0, Tell = 1, Burst = 2, Active = 3 }

    /// <summary>
    /// 117 + 118. Gossip Shout / Aim Scramble and Hook Thrower Wind-up (v1.9.2): attack tells for small "liar" enemies (Hellord Level 1 Gossip Imp and Hook Thrower).
    /// 117 SHOUT (<see cref="Shout"/>): area warning around the imp in the same REAL style as Judge Kit #116, with a SOLID orange rim at the
    ///   shout radius, a red timing fill growing centre -> rim (full = shout), a white blink in the last 15% and a bouncing '!' on a dark plate above the imp.
    ///   BURST (0.3 s): 1-frame cream flash, a jagged white->yellow "sound" ring expands to the rim with an orange ring behind it. <see cref="GossipHookFX.OnShout"/>
    ///   fires at burst start; the game tests the radius and applies its aim scramble.
    /// 117 SCRAMBLE (<see cref="Scramble"/>): status on the victim (never tints it), with 3 '?' glyphs on dark plates orbiting the head and two arrows
    ///   circling the feet in OPPOSITE directions ("your aim is swapped"); blinks in the last 0.5 s. The game owns the actual input scramble.
    /// 118 HOOK WIND-UP (<see cref="HookWindup"/>): the thrower whirls a steel hook over its head (spin speeds up, white blink in the last 15%)
    ///   with an orange aim chevron at its body pointing at the target, so you can see WHICH enemy in a crowd is about to throw. At the end it calls
    ///   <see cref="HookChainFX.Throw"/> (lane tell, flight, drag, 4 s re-arm) aimed at the target's position at that moment and fires
    ///   <see cref="GossipHookFX.OnHookThrown"/>(windup, hook). If the thrower is destroyed first, nothing is thrown.
    /// Visual only. One pooled mesh, one 4x4 white texture, one material (1 draw call); no per-frame allocations; int handles, -1 when the pool is full.
    /// </summary>
    public static class GossipHookFX
    {
        static GossipHookRunner R => GossipHookRunner.I;
        public static float ShoutRadiusPx = 48f, ShoutTell = 0.6f, ShoutBurst = 0.3f, ShoutBangPx = 22f;
        public static float ScrambleSeconds = 2.5f, ScrambleHeadPx = 20f;
        public static float WindupSeconds = 0.45f, WindupHeadPx = 22f, ThrowLaneTell = 0.5f;
        public static Material MaterialOverride; public static int SortingOrder = 71; public static string SortingLayer = "Default";
        /// <summary>v2.3.1: the Shout warning FILL (pink disc + the 2-frame burst flash) is drawn by a second renderer UNDER characters so it never tints an imp.
        /// Default -20000 = the same back band BurnFX uses (above floor/props/atmos, below the y-sorted characters). The rim, growth edge and '!' stay on <see cref="SortingOrder"/> (above) so they read in a horde.</summary>
        public static int FillSortingOrder = -20000; public static string FillSortingLayer = "Default";
        /// <summary>Colour of the Shout fill (alpha = opacity). Default #f6757a at 62%.</summary>
        /// <summary>Thickness (px) of the Shout rim and growth edge. 3 = QC minimum (CRT scanlines eat 1-2 px); 1 = the v1.9.3 look.</summary>
        public static int ShoutEdgePx = 3;
        public static Color32 ShoutFillColor = new Color32(0xf6, 0x75, 0x7a, 158);
        /// <summary>Set the fill's sorting layer + order (applies immediately). Pass the same layer/order as <see cref="SortingOrder"/> to get the old over-characters behaviour.</summary>
        public static void SetFillSorting(string layerName, int order) { FillSortingLayer = layerName; FillSortingOrder = order; if (GossipHookRunner.Exists) GossipHookRunner.I.ApplyFillSorting(); }
        /// <summary>(handle) when a shout bursts. Test your radius here.</summary>
        public static Action<int> OnShout;
        /// <summary>(windup handle, HookChainFX handle) when a wind-up ends and the hook is thrown.</summary>
        public static Action<int, int> OnHookThrown;

        public static void Prewarm(int items = 32) => R.Prewarm(items);
        /// <summary>Shout warning centred on <paramref name="center"/> (world units; locked).</summary>
        public static int Shout(Vector2 center, float radiusPx = -1, float tell = -1)
            => R.Add(GHKind.Shout, center, center, radiusPx > 0 ? radiusPx : ShoutRadiusPx, tell >= 0 ? tell : ShoutTell, ShoutBurst, null, null);
        /// <summary>Aim-scramble status that follows <paramref name="victim"/> for <paramref name="seconds"/>.</summary>
        public static int Scramble(Transform victim, float seconds = -1)
            => victim == null ? -1 : R.Add(GHKind.Scramble, victim.position, victim.position, 0, 0, seconds > 0 ? seconds : ScrambleSeconds, victim, null);
        /// <summary>Hook wind-up over <paramref name="thrower"/>, aimed at <paramref name="target"/>; throws a HookChainFX hook when done.</summary>
        public static int HookWindup(Transform thrower, Transform target, float seconds = -1)
            => thrower == null ? -1 : R.Add(GHKind.HookWindup, thrower.position, target != null ? (Vector2)target.position : (Vector2)thrower.position + Vector2.right, 0, seconds >= 0 ? seconds : WindupSeconds, 0, thrower, target);
        public static GHPhase GetPhase(int h) => R.Phase(h);
        public static void Cancel(int h) => R.Cancel(h);
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => GossipHookRunner.Exists ? R.Count : 0;
    }

    public class GossipHookRunner : MonoBehaviour
    {
        static GossipHookRunner _i;
        public static bool Exists => _i != null;
        public static GossipHookRunner I { get { if (_i == null) { var go = new GameObject("GossipHookFX"); DontDestroyOnLoad(go); _i = go.AddComponent<GossipHookRunner>(); } return _i; } }

        struct It
        {
            public GHKind kind; public GHPhase ph; public Vector2 a, b; public float R, tell, dur, t, pt; public Transform v, tgt; public Vector2 tgtPos; public int hook; public ushort gen;
        }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }

        const int MaxQ = 6144; struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ]; int _nb, _nf, _last;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        const int MaxF = 4096; readonly Q[] _fl = new Q[MaxF]; int _nl, _lastF; Mesh _fmesh; MeshRenderer _fmr; Vector3[] _fvtx; Vector2[] _fuv; Color32[] _fcol; int[] _ftri;
        // Endesga 32
        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), Wh = new Color32(255, 255, 255, 255), Y = new Color32(0xfe, 0xe7, 0x61, 255),
            O = new Color32(0xf7, 0x76, 0x22, 255), Rd = new Color32(0xe4, 0x3b, 0x44, 255), Pk = new Color32(0xf6, 0x75, 0x7a, 255), Bl = new Color32(0x9e, 0x28, 0x35, 255),
            Lt = new Color32(0x8b, 0x9b, 0xb4, 255), Md = new Color32(0x5a, 0x69, 0x88, 255), Wd = new Color32(0xbe, 0x4a, 0x2f, 255), Wk = new Color32(0x73, 0x3e, 0x39, 255),
            Ds = new Color32(0xea, 0xd4, 0xaa, 255), G = new Color32(0xfe, 0xae, 0x34, 255), Ko = new Color32(0x10, 0x00, 0x10, 255);
        // glyphs, rows top->bottom: Q7 = bold 5x7 '?', Ex = 3x5 '!'
        static readonly string[] Q7 = { ".WWW.", "WW.WW", "...WW", "..WW.", "..WW.", ".....", "..WW." };
        static readonly string[] Ex = { ".W.", ".W.", ".W.", "...", ".W." };

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == GHKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mesh != null) return;
            _vtx = new Vector3[MaxQ * 8]; _uv = new Vector2[MaxQ * 8]; _col = new Color32[MaxQ * 8]; _tri = new int[MaxQ * 12];
            for (int i = 0; i < MaxQ * 2; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "GossipHookFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX GossipHook White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = GossipHookFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "GossipHookFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] GossipHookFX: no sprite shader found; set GossipHookFX.MaterialOverride.");
            mr.sortingOrder = GossipHookFX.SortingOrder; mr.sortingLayerName = GossipHookFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
            /* v2.3.1: second mesh + renderer for the fill, sorted under characters */
            _fvtx = new Vector3[MaxF * 4]; _fuv = new Vector2[MaxF * 4]; _fcol = new Color32[MaxF * 4]; _ftri = new int[MaxF * 6];
            for (int i = 0; i < MaxF; i++) { int v = i * 4, t = i * 6; _ftri[t] = v; _ftri[t + 1] = v + 1; _ftri[t + 2] = v + 2; _ftri[t + 3] = v; _ftri[t + 4] = v + 2; _ftri[t + 5] = v + 3;
                _fuv[v] = new Vector2(.25f, .25f); _fuv[v + 1] = new Vector2(.25f, .75f); _fuv[v + 2] = new Vector2(.75f, .75f); _fuv[v + 3] = new Vector2(.75f, .25f); }
            _fmesh = new Mesh { name = "GossipHookFX Fill" }; _fmesh.MarkDynamic(); _fmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            var fgo = new GameObject("GossipHookFX Fill"); fgo.transform.SetParent(transform, false); fgo.AddComponent<MeshFilter>().sharedMesh = _fmesh; _fmr = fgo.AddComponent<MeshRenderer>();
            if (m != null) _fmr.sharedMaterial = m; _fmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; _fmr.receiveShadows = false; _fmr.enabled = false; ApplyFillSorting();
            _fmesh.vertices = _fvtx; _fmesh.uv = _fuv; _fmesh.colors32 = _fcol; _fmesh.triangles = _ftri;
        }
        public void ApplyFillSorting() { if (_fmr == null) return; _fmr.sortingLayerName = GossipHookFX.FillSortingLayer; _fmr.sortingOrder = GossipHookFX.FillSortingOrder; }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != GHKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = GHKind.None; _it[i].ph = GHPhase.None; _it[i].v = null; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, GHPhase p) { k.ph = p; k.pt = 0; }
        int Alloc() { if (_mesh == null) Prewarm(24); if (_freeN == 0) return -1; int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++; return i; }

        public int Add(GHKind kind, Vector2 a, Vector2 b, float R, float tell, float dur, Transform v, Transform tgt)
        {
            int i = Alloc(); if (i < 0) return -1; ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.a = a * ppu; k.b = b * ppu; k.R = R; k.tell = kind == GHKind.Scramble ? 0 : Mathf.Max(0.2f, tell); k.dur = dur; k.v = v; k.tgt = tgt; k.t = 0; k.hook = -1;
            Go(ref k, kind == GHKind.Scramble ? GHPhase.Active : GHPhase.Tell); return Hd(i);
        }
        public GHPhase Phase(int h) => Get(h, out int i) ? _it[i].ph : GHPhase.None;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].kind != GHKind.None) Free(i); }
        // ---------- emit ----------
        void Px(float x, float y, Color32 c, bool back = true)
        {
            float sx = Mathf.Round(x), sy = Mathf.Round(y);
            if (back && _nb < MaxQ) _bk[_nb++] = new Q { x = sx - 1, y = sy - 1, w = 3, h = 3, c = K };
            if (_nf < MaxQ) _fg[_nf++] = new Q { x = sx, y = sy, w = 1, h = 1, c = c };
        }
        void Span(float x0, float x1, float y, Color32 c) { if (_nf < MaxQ && x1 >= x0) _fg[_nf++] = new Q { x = Mathf.Round(x0), y = Mathf.Round(y), w = Mathf.Round(x1) - Mathf.Round(x0) + 1, h = 1, c = c }; }
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(255 * a), 0, 255); return c; }
        static Vector2 N(Vector2 d) => new Vector2(-d.y, d.x);
        static float H(float x) { x = Mathf.Sin(x * 12.9898f) * 43758.5453f; return x - Mathf.Floor(x); }
        // rim: solid (real) or dashed + crawling + 1px wobble (fake)
        void Line(Vector2 a, Vector2 b, Color32 c, bool fake, float t, bool back = true)
        {
            Vector2 d = b - a; float len = d.magnitude; if (len < 0.5f) { Px(a.x, a.y, c, back); return; } d /= len; Vector2 n = N(d); int crawl = (int)(t * 16);
            for (int r = 0; r <= (int)len; r++)
            {
                var p = a + d * r;
                if (fake) { if (((r + crawl) & 3) >= 2) continue; p += n * ((((r >> 2) + (int)(t * 12)) & 1) == 0 ? 1 : 0); }
                Px(p.x, p.y, c, back);
            }
        }
        void Circle(Vector2 c, float r, Color32 col, bool fake, float t, bool back = true)
        {
            int n = Mathf.Max(8, Mathf.CeilToInt(2 * Mathf.PI * r)); int crawl = (int)(t * 16);
            for (int i = 0; i < n; i++)
            {
                if (fake && ((i + crawl) & 3) >= 2) continue;
                float a = 2 * Mathf.PI * i / n, rr = r + (fake && (((i >> 2) + (int)(t * 12)) & 1) == 0 ? 1 : 0);
                Px(c.x + Mathf.Cos(a) * rr, c.y + Mathf.Sin(a) * rr, col, back);
            }
        }
        /* thick ring: ShoutEdgePx square dots every ~2 px over a dark 2-px-larger backing */
        void ThickRing(Vector2 c, float r, Color32 col, bool back)
        {
            int w = Mathf.Max(1, GossipHookFX.ShoutEdgePx); if (w == 1) { Circle(c, r, col, false, 0, back); return; }
            int n = Mathf.Max(12, Mathf.CeilToInt(Mathf.PI * r)); float h = w * 0.5f;
            for (int i = 0; i < n; i++)
            {
                float a = 2 * Mathf.PI * i / n, x = Mathf.Round(c.x + Mathf.Cos(a) * r - h), y = Mathf.Round(c.y + Mathf.Sin(a) * r - h);
                if (back && _nb < MaxQ) _bk[_nb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = w + 2, c = K };
                if (_nf < MaxQ) _fg[_nf++] = new Q { x = x, y = y, w = w, h = w, c = col };
            }
        }
        void Disc(Vector2 c, float r, Color32 col) { for (int y = -(int)r; y <= (int)r; y++) { float hw = Mathf.Sqrt(Mathf.Max(0, r * r - y * y)); FSpan(c.x - hw, c.x + hw, c.y + y, col); } }
        /* fill row (1 px high, merged per row) -> the under-characters mesh */
        void FSpan(float x0, float x1, float y, Color32 c) { if (_nl < MaxF && x1 >= x0) _fl[_nl++] = new Q { x = Mathf.Round(x0), y = Mathf.Round(y), w = Mathf.Round(x1) - Mathf.Round(x0) + 1, h = 1, c = c }; }
        void Chev(Vector2 tip, Vector2 d, int len, Color32 c) { Vector2 n = N(d); for (int s = 0; s <= len; s++) { var p = tip - d * s + n * s; Px(p.x, p.y, c); if (s > 0) { p = tip - d * s - n * s; Px(p.x, p.y, c); } } }
        void Rect(float x, float y, float w, float h, Color32 c) { if (_nf < MaxQ) _fg[_nf++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        // 7x7 star with dark backing: white core, yellow (front) / gold (back) arms 3px
        void BigStar(Vector2 p, bool front) { Color32 a = front ? Y : G; Px(p.x, p.y, Wh); for (int r = 1; r <= 3; r++) { Color32 c = r == 1 ? (front ? Wh : Y) : a; Px(p.x + r, p.y, c); Px(p.x - r, p.y, c); Px(p.x, p.y + r, c); Px(p.x, p.y - r, c); } Px(p.x + 1, p.y + 1, a); Px(p.x - 1, p.y - 1, a); Px(p.x + 1, p.y - 1, a); Px(p.x - 1, p.y + 1, a); }
        void Glyph(string[] g, float x, float y, Color32 c, bool plate) { if (plate) Rect(x - 1, y - 1, 5, 7, Ko); for (int r = 0; r < 5; r++) for (int cc = 0; cc < 3; cc++) if (g[r][cc] == 'W') Rect(x + cc, y + 4 - r, 1, 1, c); }
        // v1.9.3: bold 5x7 glyph on an opaque 7x9 dark plate (1px outline all round)
        void Glyph7(string[] g, float x, float y, Color32 c) { Rect(x - 1, y - 1, 7, 9, Ko); for (int r = 0; r < 7; r++) for (int cc = 0; cc < 5; cc++) if (g[r][cc] == 'W') Rect(x + cc, y + 6 - r, 1, 1, c); }
        // small arrow on an arc: 6 px of arc ending in a 2 px head, direction dir = +1 ccw / -1 cw
        void ArcArrow(Vector2 c, float rx, float ry, float a0, int dir, Color32 col)
        {
            Vector2 last = c, prev = c;
            for (int s = 0; s <= 6; s++) { float a = a0 + dir * s * 0.12f; var p = c + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry); Px(p.x, p.y, col); prev = last; last = p; }
            Vector2 d = (last - prev); if (d.sqrMagnitude < 1e-4f) return; d.Normalize(); Vector2 n = N(d);
            for (int s = 1; s <= 2; s++) { var p = last - d * s + n * s; Px(p.x, p.y, col); p = last - d * s - n * s; Px(p.x, p.y, col); }
        }
        void Star(Vector2 p, Color32 c) { Px(p.x, p.y, Wh); Px(p.x + 1, p.y, c); Px(p.x - 1, p.y, c); Px(p.x, p.y + 1, c); Px(p.x, p.y - 1, c); }
        // peel: the dashed rim scatters outward and fades (fake ending, no impact)
        void Peel(Vector2 c, float r, float q, int n, Color32 col)
        { for (int i = 0; i < n; i++) { float a = 2 * Mathf.PI * (i + 0.5f * H(i)) / n, rr = r + q * (6 + 6 * H(i + 7)); if (H(i * 3 + 1) < q) continue; Px(c.x + Mathf.Cos(a) * rr, c.y + Mathf.Sin(a) * rr, Al(col, 1 - q), false); } }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu; _nb = _nf = _nl = 0;
            for (int i = 0; i < _it.Length; i++)
            {
                ref var k = ref _it[i]; if (k.kind == GHKind.None) continue;
                k.t += dt; k.pt += dt;
                if (k.kind == GHKind.Scramble || k.kind == GHKind.HookWindup)
                {
                    if (k.v == null) { Free(i); continue; }   // owner destroyed: drop the tell, never throw from nowhere
                    k.a = (Vector2)k.v.position * ppu;
                    if (k.tgt != null) k.b = (Vector2)k.tgt.position * ppu;
                }
                Step(ref k, i);
                if (k.kind != GHKind.None) Draw(ref k);
            }
            int q = 0;
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
            if (_fmesh != null)
            {
                for (int f = 0; f < _nl; f++) { int v = f * 4; ref var s = ref _fl[f]; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
                    _fvtx[v] = new Vector3(x0, y0, 0); _fvtx[v + 1] = new Vector3(x0, y1, 0); _fvtx[v + 2] = new Vector3(x1, y1, 0); _fvtx[v + 3] = new Vector3(x1, y0, 0); _fcol[v] = _fcol[v + 1] = _fcol[v + 2] = _fcol[v + 3] = s.c; }
                for (int z = _nl; z < _lastF; z++) { int v = z * 4; _fvtx[v] = _fvtx[v + 1] = _fvtx[v + 2] = _fvtx[v + 3] = Vector3.zero; }
                if (_nl > 0 || _lastF > 0) { _fmesh.vertices = _fvtx; _fmesh.colors32 = _fcol; _fmesh.RecalculateBounds(); }
                _fmr.enabled = _nl > 0; _lastF = _nl;
            }
        }
        void Step(ref It k, int i)
        {
            switch (k.ph)
            {
                case GHPhase.Tell:
                    if (k.pt < k.tell) break;
                    if (k.kind == GHKind.Shout) { Go(ref k, GHPhase.Burst); GossipHookFX.OnShout?.Invoke(Hd(i)); }
                    else
                    {
                        float u = 1f / HellFX.PPU; int hook = HookChainFX.Throw((k.a + new Vector2(0, GossipHookFX.WindupHeadPx * 0.5f)) * u, k.b * u, -1, GossipHookFX.ThrowLaneTell);
                        GossipHookFX.OnHookThrown?.Invoke(Hd(i), hook); Free(i);
                    }
                    break;
                case GHPhase.Burst: if (k.pt >= k.dur) Free(i); break;
                case GHPhase.Active: if (k.pt >= k.dur) Free(i); break;
            }
        }
        void Draw(ref It k)
        {
            float t = k.pt, q = k.tell > 0 ? Mathf.Clamp01(t / k.tell) : 0; bool blink = k.ph == GHPhase.Tell && q > 0.85f && ((int)(t * 20) & 1) == 0;
            Color32 rim = blink ? Wh : O;
            switch (k.kind)
            {
                case GHKind.Shout:
                {
                    Vector2 c = k.a; float R = k.R;
                    if (k.ph == GHPhase.Tell)
                    {
                        Disc(c, R * q, GossipHookFX.ShoutFillColor); ThickRing(c, Mathf.Max(1, R * q), Wh, false); /* v1.9.3: brighter pink fill + white growth edge */ ThickRing(c, R, rim, true);
                        float bob = Mathf.Abs(Mathf.Sin(t * 14)) * 2; Glyph(Ex, c.x - 1, c.y + GossipHookFX.ShoutBangPx + bob, blink ? Wh : Y, true);
                    }
                    else
                    {
                        float s = t / k.dur;
                        if (t < 0.067f) Disc(c, R, Al(Ds, 0.5f));
                        float rr = R * (0.35f + 0.65f * s); int n = Mathf.Max(12, Mathf.CeilToInt(2 * Mathf.PI * rr)); Color32 rc = s < 0.3f ? Wh : Y;
                        for (int j = 0; j < n; j++) { float a = 2 * Mathf.PI * j / n, r2 = rr + (((j / 3) & 1) == 0 ? 1.5f : -1.5f); Px(c.x + Mathf.Cos(a) * r2, c.y + Mathf.Sin(a) * r2, rc); }
                        if (s < 0.8f) Circle(c, R * 0.7f * s + 2, O, false, t, false);
                    }
                    break;
                }
                case GHKind.Scramble:
                {
                    float left = k.dur - t; if (left < 0.5f && ((int)(t * 10) & 1) == 0) break;
                    Vector2 hd = k.a + new Vector2(0, GossipHookFX.ScrambleHeadPx);
                    for (int j = 0; j < 3; j++) { float an = t * 3 + j * 2.094f; Glyph7(Q7, hd.x - 2 + Mathf.Cos(an) * 13, hd.y + Mathf.Sin(an) * 3, (j & 1) == 0 ? Pk : Wh); }
                    float a0 = t * 5; ArcArrow(k.a, 11, 4, a0, 1, Pk); ArcArrow(k.a, 11, 4, -a0 + Mathf.PI, -1, Y);
                    break;
                }
                case GHKind.HookWindup:
                {
                    Vector2 hd = k.a + new Vector2(0, GossipHookFX.WindupHeadPx); float spin = t * (10 + 20 * q);
                    for (int tr = 3; tr >= 1; tr--) { float an = spin - tr * 0.45f; Px(hd.x + Mathf.Cos(an) * 7, hd.y + Mathf.Sin(an) * 3, Al(Lt, 0.25f * (4 - tr)), false); }
                    Vector2 hp = hd + new Vector2(Mathf.Cos(spin) * 7, Mathf.Sin(spin) * 3);
                    Line(k.a + new Vector2(0, 8), hp, Md, false, t);
                    Color32 hc = blink ? Wh : Lt; Px(hp.x, hp.y, Wh); Px(hp.x + 1, hp.y, hc); Px(hp.x - 1, hp.y, hc); Px(hp.x, hp.y + 1, hc); Px(hp.x, hp.y - 1, hc);
                    if (q > 0.85f) Circle(hd, 9, blink ? Wh : Y, false, t, false);
                    Vector2 d = k.b - k.a; if (d.sqrMagnitude > 1e-3f) { d.Normalize(); Chev(k.a + d * 14, d, 3, rim); Chev(k.a + d * 18, d, 3, rim); }
                    break;
                }
            }
        }
        static float Mod(float a, float m) => a - m * Mathf.Floor(a / m);
        void Write(int q, ref Q s, float u)
        {
            int v = q * 4; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
            _vtx[v] = new Vector3(x0, y0, 0); _vtx[v + 1] = new Vector3(x0, y1, 0); _vtx[v + 2] = new Vector3(x1, y1, 0); _vtx[v + 3] = new Vector3(x1, y0, 0);
            _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
