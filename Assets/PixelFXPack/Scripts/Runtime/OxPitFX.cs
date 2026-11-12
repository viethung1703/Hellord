using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of an <see cref="OxPitFX"/> item.</summary>
    public enum OxKind { None = 0, GoreLane = 1, HornSweep = 2, StampedeLane = 3, Whip = 4, Haste = 5 }
    /// <summary>Phase of an <see cref="OxPitFX"/> item.</summary>
    public enum OxPhase { None = 0, Tell = 1, Strike = 2, Active = 3 }

    /// <summary>
    /// 119-122. Ox Pit (Hellord Level 10) tells (v1.9.4). Sizes are world pixels from the attacker's feet pivot (16 PPU, 640x360 view).
    /// 119 GORE CHARGE LANE (<see cref="GoreLane"/>): Ox-Head charge, 40 px wide (= contact width), solid orange edges, red timing fill growing
    ///   from the feet to the end cap, yellow chevrons marching in the run direction, white blink in the last 15 %. wallEnd = true adds a jagged
    ///   yellow "wall hit" cue past the end cap (the boss stuns on the wall). Calf variant (calf = true): 24 px, plus dust puffs kicked back from the feet.
    ///   STRIKE 0.3 s: the edges flash white and speed streaks run down the lane. <see cref="OxPitFX.OnStrike"/>.
    /// 120 HORN SWEEP (<see cref="HornSweep"/>): cone from the horns (38 px above the feet), reach 60 px, 110 deg. Solid orange rim; the red fill sweeps
    ///   across the cone in the swing direction with a white leading edge, so you can read which side the horns come from. STRIKE 0.2 s: white/yellow slash band.
    /// 121 STAMPEDE LANE (<see cref="StampedeLane"/>): environment hazard, 40 px, YELLOW/DARK hazard-striped edges that crawl away from the gate,
    ///   faint yellow fill, hoof marks marching, and a bone-tooth bracket at the gate end. Kept yellow/black on purpose: boss lanes are orange, hazards are striped.
    /// 122 HERDER WHIP + HASTE (<see cref="Whip"/>): whip lash from the herder to the ally (0.25 s = whip frame 3 at 12 fps), crack star on the ally,
    ///   then a haste status: a yellow '>>' icon on a dark plate over the head and flickering speed streaks at the feet. The ally sprite is never tinted.
    /// v1.9.5 fixes: gore/calf fill is orange (clears the red hero) with a bright yellow 2 px border; calf chevrons every 10 px; whip lash 2 px yellow/gold
    ///   with a wind-up flash at the herder's hand; haste shows a ground-ellipse colour rim (FX mesh, no tint) instead of speed streaks; all fills pixel-snapped.
    /// Visual only. One pooled mesh, one 4x4 white texture, one material (1 draw call); no per-frame allocations; int handles, -1 when the pool is full.
    /// </summary>
    public static class OxPitFX
    {
        static OxPitRunner R => OxPitRunner.I;
        public static float GoreWidthPx = 40f, GoreLengthPx = 160f, GoreTell = 0.75f, CalfWidthPx = 24f, CalfLengthPx = 80f, CalfTell = 0.58f, StrikeSeconds = 0.3f;
        public static float HornOriginPx = 38f, HornReachPx = 60f, HornArcDeg = 110f, HornTell = 0.7f, HornStrike = 0.2f;
        public static float StampedeWidthPx = 40f, StampedeLengthPx = 320f, StampedeTell = 1.0f;
        public static float WhipSeconds = 0.25f, HasteSeconds = 3f, HerderHandPx = 18f, HasteHeadPx = 24f, HasteRimRxPx = 12f, HasteRimRyPx = 5f;
        public static Material MaterialOverride; public static int SortingOrder = 70; public static string SortingLayer = "Default";
        /// <summary>(handle) when a tell ends and the attack lands (gore/horn/stampede), or when the whip cracks.</summary>
        public static Action<int> OnStrike;

        public static void Prewarm(int items = 32) => R.Prewarm(items);
        /// <summary>Ox-Head (or Calf) charge lane from <paramref name="feet"/> toward <paramref name="target"/> (world units; locked at call time).</summary>
        public static int GoreLane(Vector2 feet, Vector2 target, bool calf = false, bool wallEnd = false, float lengthPx = -1, float widthPx = -1, float tell = -1)
            => R.Add(calf ? 1 : 0, OxKind.GoreLane, feet, target, lengthPx > 0 ? lengthPx : (calf ? CalfLengthPx : GoreLengthPx), widthPx > 0 ? widthPx : (calf ? CalfWidthPx : GoreWidthPx), tell >= 0 ? tell : (calf ? CalfTell : GoreTell), StrikeSeconds, wallEnd ? 1 : 0, null);
        /// <summary>Horn sweep cone; <paramref name="sweepDir"/> +1 = counter-clockwise swing, -1 = clockwise.</summary>
        public static int HornSweep(Vector2 feet, Vector2 facing, int sweepDir = 1, float reachPx = -1, float arcDeg = -1, float tell = -1)
            => R.Add(sweepDir >= 0 ? 1 : -1, OxKind.HornSweep, feet, feet + facing.normalized, reachPx > 0 ? reachPx : HornReachPx, arcDeg > 0 ? arcDeg : HornArcDeg, tell >= 0 ? tell : HornTell, HornStrike, 0, null);
        /// <summary>Stampede warning from a gate, running along <paramref name="dir"/>.</summary>
        public static int StampedeLane(Vector2 gate, Vector2 dir, float lengthPx = -1, float widthPx = -1, float tell = -1)
            => R.Add(0, OxKind.StampedeLane, gate, gate + dir.normalized, lengthPx > 0 ? lengthPx : StampedeLengthPx, widthPx > 0 ? widthPx : StampedeWidthPx, tell >= 0 ? tell : StampedeTell, StrikeSeconds, 0, null);
        /// <summary>Herder whip to <paramref name="ally"/>; when it cracks, a haste status follows the ally for <paramref name="hasteSeconds"/>.</summary>
        public static int Whip(Transform herder, Transform ally, float hasteSeconds = -1)
            => herder == null || ally == null ? -1 : R.AddWhip(herder, ally, hasteSeconds > 0 ? hasteSeconds : HasteSeconds);
        /// <summary>Haste status only (no whip) on <paramref name="ally"/>.</summary>
        public static int Haste(Transform ally, float seconds = -1)
            => ally == null ? -1 : R.Add(0, OxKind.Haste, ally.position, ally.position, 0, 0, 0, seconds > 0 ? seconds : HasteSeconds, 0, ally);
        public static OxPhase GetPhase(int h) => R.Phase(h);
        public static void Cancel(int h) => R.Cancel(h);
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => OxPitRunner.Exists ? R.Count : 0;
    }

    public class OxPitRunner : MonoBehaviour
    {
        static OxPitRunner _i;
        public static bool Exists => _i != null;
        public static OxPitRunner I { get { if (_i == null) { var go = new GameObject("OxPitFX"); DontDestroyOnLoad(go); _i = go.AddComponent<OxPitRunner>(); } return _i; } }

        struct It { public OxKind kind; public OxPhase ph; public Vector2 a, d; public float L, W, tell, dur, pt, haste; public int v1, flag; public Transform v, tgt; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }

        const int MaxQ = 8192;
        // axis rect (x,y,w,h) or, when poly, a free quad p0..p3 (triangles = two of them equal)
        struct Q { public float x, y, w, h; public bool poly; public Vector2 p0, p1, p2, p3; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ], _fl = new Q[4096]; int _nb, _nf, _nl, _last;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        // Endesga 32
        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), Ko = new Color32(0x10, 0x00, 0x10, 255), Wh = new Color32(255, 255, 255, 255),
            Y = new Color32(0xfe, 0xe7, 0x61, 255), O = new Color32(0xf7, 0x76, 0x22, 255), Rd = new Color32(0xe4, 0x3b, 0x44, 255), G = new Color32(0xfe, 0xae, 0x34, 255),
            Lt = new Color32(0x8b, 0x9b, 0xb4, 255), Ds = new Color32(0xea, 0xd4, 0xaa, 255), Wd = new Color32(0xc2, 0x85, 0x69, 255), Wk = new Color32(0x73, 0x3e, 0x39, 255);
        static readonly string[] Hs = { "W..W..", ".W..W.", "..W..W", ".W..W.", "W..W.." }; // '>>' 6x5

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == OxKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mesh != null) return;
            int qs = MaxQ * 2 + 4096;
            _vtx = new Vector3[qs * 4]; _uv = new Vector2[qs * 4]; _col = new Color32[qs * 4]; _tri = new int[qs * 6];
            for (int i = 0; i < qs; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "OxPitFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX OxPit White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = OxPitFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "OxPitFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] OxPitFX: no sprite shader found; set OxPitFX.MaterialOverride.");
            mr.sortingOrder = OxPitFX.SortingOrder; mr.sortingLayerName = OxPitFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != OxKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = OxKind.None; _it[i].ph = OxPhase.None; _it[i].v = null; _it[i].tgt = null; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, OxPhase p) { k.ph = p; k.pt = 0; }
        int Alloc() { if (_mesh == null) Prewarm(24); if (_freeN == 0) return -1; int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++; return i; }

        public int Add(int v1, OxKind kind, Vector2 a, Vector2 b, float L, float W, float tell, float dur, int flag, Transform v)
        {
            int i = Alloc(); if (i < 0) return -1; ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.a = a * ppu; Vector2 d = (b - a); k.d = d.sqrMagnitude > 1e-8f ? d.normalized : Vector2.right; k.L = L; k.W = W; k.v1 = v1; k.flag = flag;
            k.tell = kind == OxKind.Haste ? 0 : Mathf.Max(0.2f, tell); k.dur = dur; k.v = v; k.tgt = null; k.haste = 0;
            Go(ref k, kind == OxKind.Haste ? OxPhase.Active : OxPhase.Tell); return Hd(i);
        }
        public int AddWhip(Transform herder, Transform ally, float haste)
        {
            int h = Add(0, OxKind.Whip, herder.position, ally.position, 0, 0, OxPitFX.WhipSeconds, 0.12f, 0, herder); if (h < 0) return -1;
            ref var k = ref _it[h & 0xFFFF]; k.tgt = ally; k.haste = haste; return h;
        }
        public OxPhase Phase(int h) => Get(h, out int i) ? _it[i].ph : OxPhase.None;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].kind != OxKind.None) Free(i); }

        // ---------- emit ----------
        void Px(float x, float y, Color32 c, bool back = true)
        {
            float sx = Mathf.Round(x), sy = Mathf.Round(y);
            if (back && _nb < MaxQ) _bk[_nb++] = new Q { x = sx - 1, y = sy - 1, w = 3, h = 3, c = K };
            if (_nf < MaxQ) _fg[_nf++] = new Q { x = sx, y = sy, w = 1, h = 1, c = c };
        }
        void Rect(float x, float y, float w, float h, Color32 c) { if (_nf < MaxQ) _fg[_nf++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        // translucent fill polygon (drawn under the backing + pixels)
        // v1.9.5: pixel-snapped. The convex quad (p3 == p0 for a triangle) is rasterised into 1 px tall, whole-pixel row spans, so fills never shimmer.
        readonly Vector2[] _pp = new Vector2[4];
        void Poly(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color32 c)
        {
            _pp[0] = p0; _pp[1] = p1; _pp[2] = p2; _pp[3] = p3;
            float y0 = Mathf.Min(Mathf.Min(p0.y, p1.y), Mathf.Min(p2.y, p3.y)), y1 = Mathf.Max(Mathf.Max(p0.y, p1.y), Mathf.Max(p2.y, p3.y));
            for (int y = Mathf.RoundToInt(y0); y < Mathf.RoundToInt(y1); y++)
            {
                float yc = y + 0.5f, lo = float.MaxValue, hi = float.MinValue;
                for (int e = 0; e < 4; e++)
                {
                    Vector2 a = _pp[e], b = _pp[(e + 1) & 3]; if ((a.y <= yc) == (b.y <= yc)) continue;
                    float x = a.x + (yc - a.y) / (b.y - a.y) * (b.x - a.x); if (x < lo) lo = x; if (x > hi) hi = x;
                }
                int xa = Mathf.RoundToInt(lo), xb = Mathf.RoundToInt(hi);
                if (xb > xa && _nl < _fl.Length) _fl[_nl++] = new Q { x = xa, y = y, w = xb - xa, h = 1, c = c };
            }
        }
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(255 * a), 0, 255); return c; }
        static Vector2 N(Vector2 d) => new Vector2(-d.y, d.x);
        static float H(float x) { x = Mathf.Sin(x * 12.9898f) * 43758.5453f; return x - Mathf.Floor(x); }
        void Line(Vector2 a, Vector2 b, Color32 c, bool back = true)
        { Vector2 d = b - a; float len = d.magnitude; if (len < 0.5f) { Px(a.x, a.y, c, back); return; } d /= len; for (int r = 0; r <= (int)len; r++) { var p = a + d * r; Px(p.x, p.y, c, back); } }
        void Chev(Vector2 tip, Vector2 d, int len, Color32 c) { Vector2 n = N(d); for (int s = 0; s <= len; s++) { var p = tip - d * s + n * s; Px(p.x, p.y, c); if (s > 0) { p = tip - d * s - n * s; Px(p.x, p.y, c); } } }
        void Star(Vector2 p, Color32 c, int r) { Px(p.x, p.y, Wh); for (int s = 1; s <= r; s++) { Px(p.x + s, p.y, c); Px(p.x - s, p.y, c); Px(p.x, p.y + s, c); Px(p.x, p.y - s, c); } }
        static Vector2 Rot(Vector2 d, float a) { float c = Mathf.Cos(a), s = Mathf.Sin(a); return new Vector2(d.x * c - d.y * s, d.x * s + d.y * c); }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu; _nb = _nf = _nl = 0;
            for (int i = 0; i < _it.Length; i++)
            {
                ref var k = ref _it[i]; if (k.kind == OxKind.None) continue;
                k.pt += dt;
                if (k.kind == OxKind.Whip || k.kind == OxKind.Haste)
                {
                    if (k.v == null || (k.kind == OxKind.Whip && k.tgt == null)) { Free(i); continue; }
                    k.a = (Vector2)k.v.position * ppu;
                }
                Step(ref k, i);
                if (k.kind != OxKind.None) Draw(ref k);
            }
            int q = 0;
            for (int f = 0; f < _nl; f++) Write(q++, ref _fl[f], u);
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
        }
        void Step(ref It k, int i)
        {
            switch (k.ph)
            {
                case OxPhase.Tell:
                    if (k.pt < k.tell) break;
                    OxPitFX.OnStrike?.Invoke(Hd(i));
                    if (k.kind == OxKind.Whip && k.tgt != null && k.haste > 0) { var ally = k.tgt; float hs = k.haste; Go(ref k, OxPhase.Strike); OxPitFX.Haste(ally, hs); }
                    else Go(ref k, OxPhase.Strike);
                    break;
                case OxPhase.Strike: if (k.pt >= k.dur) Free(i); break;
                case OxPhase.Active: if (k.pt >= k.dur) Free(i); break;
            }
        }
        void Draw(ref It k)
        {
            float t = k.pt, q = k.tell > 0 ? Mathf.Clamp01(t / k.tell) : 0; bool tell = k.ph == OxPhase.Tell, blink = tell && q > 0.85f && ((int)(t * 20) & 1) == 0;
            Color32 rim = blink ? Wh : O; Vector2 a = k.a, d = k.d, n = N(d);
            switch (k.kind)
            {
                case OxKind.GoreLane:
                {
                    float hw = k.W * 0.5f, L = k.L; Vector2 e = a + d * L;
                    if (tell)
                    {
                        Vector2 f = a + d * (L * q);
                        Poly(a + n * hw, f + n * hw, f - n * hw, a - n * hw, Al(O, 0.45f));   // v1.9.5: orange fill clears the red hero (#ff2448)
                        Color32 brd = blink ? Wh : Y;                                          // v1.9.5: bright 2 px border (yellow outer, orange inner)
                        Line(a + n * hw, e + n * hw, brd); Line(a - n * hw, e - n * hw, brd);
                        Line(a + n * (hw - 1), e + n * (hw - 1), rim, false); Line(a - n * (hw - 1), e - n * (hw - 1), rim, false);
                        for (int s = 0; s < 3; s++) Line(e + n * hw - d * s, e - n * hw - d * s, s == 1 ? (blink ? Wh : Y) : rim, s != 1);   // 3px end cap
                        float sp = k.v1 == 1 ? 10f : 14f; for (float r = (t * 40f) % sp + 6; r < L - 5; r += sp) Chev(a + d * r, d, k.v1 == 1 ? 3 : 5, blink ? Wh : Y);   // calf: same chevrons, scaled down
                        if (k.flag == 1) for (int j = -2; j <= 2; j++) { Vector2 b0 = e + d * 3 + n * (j * hw * 0.4f); Px(b0.x, b0.y, Y); var b1 = b0 + d * 2 + n * (((j & 1) == 0) ? 1 : -1); Px(b1.x, b1.y, Y); var b2 = b1 + d * 2; Px(b2.x, b2.y, blink ? Wh : Y); }
                        if (k.v1 == 1) for (int j = 0; j < 6; j++) { float ph = (t * 3 + j / 6f) % 1f; var p = a - d * (2 + ph * 10) + n * ((H(j + 1) - 0.5f) * k.W * 0.6f) + new Vector2(0, ph * 4); Color32 c = ph < 0.5f ? Ds : Wd; Rect(p.x, p.y, ph < 0.6f ? 2 : 1, ph < 0.6f ? 2 : 1, c); }
                    }
                    else
                    {
                        float s = t / k.dur; Color32 c = s < 0.3f ? Wh : Al(O, 1 - s);
                        Line(a + n * hw, e + n * hw, c, false); Line(a - n * hw, e - n * hw, c, false);
                        for (int j = -2; j <= 2; j++) { float r0 = L * s; var p0 = a + d * Mathf.Max(0, r0 - 12) + n * (j * hw * 0.35f); var p1 = a + d * r0 + n * (j * hw * 0.35f); Line(p0, p1, Al(Wh, 1 - s), false); }
                    }
                    break;
                }
                case OxKind.HornSweep:
                {
                    Vector2 o = a + new Vector2(0, OxPitFX.HornOriginPx); float R = k.L, half = k.W * 0.5f * Mathf.Deg2Rad; int sd = k.v1;
                    int segs = Mathf.Max(6, Mathf.CeilToInt(k.W / 10f));
                    if (tell)
                    {
                        float a0 = -half * sd, sweep = 2 * half * q;
                        for (int s = 0; s < segs; s++) { float u0 = a0 + sd * sweep * s / segs, u1 = a0 + sd * sweep * (s + 1) / segs; Poly(o, o + Rot(d, u0) * R, o + Rot(d, u1) * R, o, Al(Rd, 0.40f)); }
                        int na = Mathf.CeilToInt(R * 2 * half); for (int j = 0; j <= na; j++) { var p = o + Rot(d, -half + 2 * half * j / na) * R; Px(p.x, p.y, rim); }
                        Line(o, o + Rot(d, -half) * R, rim); Line(o, o + Rot(d, half) * R, rim);
                        if (q > 0.02f) Line(o + Rot(d, a0 + sd * sweep) * 6, o + Rot(d, a0 + sd * sweep) * (R - 1), blink ? Wh : Y);
                        Chev(o + Rot(d, a0 + sd * sweep * 1.0f) * (R + 4), Rot(d, a0 + sd * sweep + sd * 1.5708f), 2, blink ? Wh : Y);   // swing-direction arrowhead on the rim
                        Star(o, rim, 2);
                    }
                    else
                    {
                        float s = t / k.dur, cur = -half * sd + sd * 2 * half * s;
                        for (int tr = 0; tr < 4; tr++) { float an = cur - sd * tr * 0.18f; for (float r = R - 6; r <= R; r += 1) { var p = o + Rot(d, an) * r; Px(p.x, p.y, tr == 0 ? Wh : (tr == 1 ? Y : G), tr < 2); } }
                    }
                    break;
                }
                case OxKind.StampedeLane:
                {
                    float hw = k.W * 0.5f, L = k.L; Vector2 e = a + d * L;
                    if (tell)
                    {
                        Poly(a + n * hw, e + n * hw, e - n * hw, a - n * hw, Al(Y, 0.10f + 0.12f * q));
                        int crawl = (int)(t * 24);
                        for (int r = 0; r <= (int)L; r++)
                        {
                            Color32 c = (((r - crawl) >> 2) & 1) == 0 ? (blink ? Wh : Y) : Ko;
                            var p = a + d * r; Px(p.x + n.x * hw, p.y + n.y * hw, c); Px(p.x - n.x * hw, p.y - n.y * hw, c);
                        }
                        for (int s = 0; s < 3; s++) Line(a + n * (hw + 2) + d * s, a - n * (hw + 2) + d * s, s == 1 ? Y : Ko, s != 1);   // gate bar
                        for (int j = -1; j <= 1; j++) { var p = a + n * (j * hw * 0.6f); for (int s = 1; s <= 4; s++) Px(p.x + d.x * (2 + s) , p.y + d.y * (2 + s), s == 4 ? Wh : Ds); } // bone teeth
                        for (float r = (t * 60f) % 24f + 10; r < L - 4; r += 24)
                        { var c0 = a + d * r; for (int side = -1; side <= 1; side += 2) { var hp = c0 + n * (side * hw * 0.35f) + d * (side * 3); Rect(hp.x, hp.y, 2, 2, blink ? Wh : Ds); Rect(hp.x + n.x * 3, hp.y + n.y * 3, 2, 2, blink ? Wh : Ds); } }
                    }
                    else { float s = t / k.dur; Color32 c = s < 0.3f ? Wh : Al(Y, 1 - s); Line(a + n * hw, e + n * hw, c, false); Line(a - n * hw, e - n * hw, c, false); }
                    break;
                }
                case OxKind.Whip:
                {
                    if (k.tgt == null) break;
                    Vector2 hand = a + new Vector2(0, OxPitFX.HerderHandPx), tp = (Vector2)k.tgt.position * HellFX.PPU + new Vector2(0, 12);
                    if (tell)
                    {
                        Vector2 dv = tp - hand; float len = dv.magnitude; if (len < 1) break; Vector2 dd = dv / len, nn = N(dd); int m = (int)(len * q);
                        for (int r = 0; r <= m; r++) { float w = Mathf.Sin(r / len * Mathf.PI) * 6 * (1 - q) + Mathf.Sin(r * 0.35f - t * 30) * (1 - r / len) * 1.5f; var p = hand + dd * r + nn * w; Color32 lc = r > m - 4 ? Wh : Y; Px(p.x, p.y, lc); Px(p.x + nn.x, p.y + nn.y, r > m - 4 ? Wh : G); }   // v1.9.5: 2 px, bright
                        if (q < 0.45f) { float fq = q / 0.45f; Star(hand + new Vector2(0, 3), ((int)(t * 30) & 1) == 0 ? Wh : Y, fq < 0.5f ? 3 : 2); int nr2 = 8; float rr2 = 3 + fq * 6; for (int j = 0; j < nr2; j++) { float an = j * Mathf.PI * 2 / nr2; Px(hand.x + Mathf.Cos(an) * rr2, hand.y + 3 + Mathf.Sin(an) * rr2, Y); } }   // wind-up flash on the Herder
                        Chev(tp + new Vector2(0, 10), new Vector2(0, -1), 2, blink ? Wh : Y);
                    }
                    else
                    {
                        float s = t / k.dur; Star(tp, Y, s < 0.5f ? 4 : 2);
                        int nr = 12; float rr = 3 + s * 8; for (int j = 0; j < nr; j += 2) { float an = j * Mathf.PI * 2 / nr; Px(tp.x + Mathf.Cos(an) * rr, tp.y + Mathf.Sin(an) * rr, s < 0.5f ? Wh : Y); }
                    }
                    break;
                }
                case OxKind.Haste:
                {
                    float left = k.dur - t; if (left < 0.5f && ((int)(t * 10) & 1) == 0) break;
                    Vector2 hd = a + new Vector2(0, OxPitFX.HasteHeadPx); float bob = ((int)(t * 6) & 1);
                    Rect(hd.x - 4, hd.y - 1 + bob, 8, 7, Ko); for (int r = 0; r < 5; r++) for (int c = 0; c < 6; c++) if (Hs[r][c] == 'W') Rect(hd.x - 3 + c, hd.y + 4 - r + bob, 1, 1, ((int)(t * 8) & 1) == 0 ? Y : G);
                    // v1.9.5: colour rim = FX-mesh ground ellipse around the feet (rx 12, ry 5), yellow/gold dashes crawling; the sprite itself is never tinted.
                    int crawl = (int)(t * 16); const int segs = 40;
                    for (int j = 0; j < segs; j++) { float an = j * Mathf.PI * 2 / segs; Color32 c = (((j + crawl) >> 2) & 1) == 0 ? Y : G; Px(a.x + Mathf.Cos(an) * OxPitFX.HasteRimRxPx, a.y + Mathf.Sin(an) * OxPitFX.HasteRimRyPx, c); }
                    break;
                }
            }
        }
        void Write(int q, ref Q s, float u)
        {
            int v = q * 4;
            if (s.poly) { _vtx[v] = s.p0 * u; _vtx[v + 1] = s.p1 * u; _vtx[v + 2] = s.p2 * u; _vtx[v + 3] = s.p3 * u; }
            else { float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u; _vtx[v] = new Vector3(x0, y0, 0); _vtx[v + 1] = new Vector3(x0, y1, 0); _vtx[v + 2] = new Vector3(x1, y1, 0); _vtx[v + 3] = new Vector3(x1, y0, 0); }
            _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
