using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="StatusHazardFX"/> item.</summary>
    public enum HazKind { None = 0, Chill = 1, IcePatch = 2, LavaCrack = 3 }
    /// <summary>Lava crack state, as in Hellord v0.0.37 (dormant -> warning -> active).</summary>
    public enum LavaState { Dormant = 0, Warning = 1, Active = 2 }

    /// <summary>
    /// 183-185. Hellord v0.0.37 / v0.0.44 hero status + floor hazards (v2.12.0). World space, 16 PPU, sizes in world units like HellordTuning.
    /// 183 HERO CHILL / FREEZE (<see cref="ChillBegin"/>, <see cref="ChillSet"/>, <see cref="ChillEnd"/>): stack pips above the head (pop on gain), rime crust at the feet, snow motes and edge crystals at the
    /// silhouette sides; FROZEN = an open ice-block OUTLINE (no fill, the hero stays readable), one white pop outside the body, cracks creep in over the last 0.3 s, shatter; IMMUNE = steady dashed ring at the feet.
    /// 184 ICE PATCH v2 (<see cref="IcePatch"/>, <see cref="IceSkid"/>): frost spreads from the centre (0.38 s ease-out) with 7 creeping dendrites, deep-blue lip, lit zone, 6 facet lines, full 1 px bright rim,
    /// a 45 deg sheen sweeping every 2.4 s, 6 upright shards popping up staggered, 2 glint slots; skid spray.
    /// 185 LAVA CRACK v2 (<see cref="LavaCrack"/>, <see cref="LavaSet"/>): navy seam with pulsing ember glints when dormant; WARNING = seam heats navy -> orange -> amber -> yellow, marching yellow border dashes, white double flash;
    /// ACTIVE = eruption, orange floor glow wash, molten channel orange rim -> deep orange -> yellow -> cream core, flowing cream dashes, drifting navy crust floes (more as it cools), popping bubbles, heat-shimmer wisps, embers; cools in 4 steps.
    /// Colours: ice #ffffff / #c0cbdc / #8b9bb4 / #5a6988 / #124e89 (never the hero cyan #2ce8f5); lava #fff6d0 / #fee761 / #feae34 / #f77622 + crust #262b44 / #3a4466 (no red: the floors and enemy HP own red).
    /// Floor art on the GROUND mesh (under characters); pips, crystals, ice block and tongues on the TOP mesh. Rects at least 1 px (fx_redo; was 3 px), opaque; only shadows use alpha. Visual only: the game keeps the rules.
    /// </summary>
    public static class StatusHazardFX
    {
        static HazRunner R => HazRunner.I;
        /// <summary>Defaults = HellordTuning v0.0.37 / v0.0.44.</summary>
        public static float LavaWidthU = 1.0f, LavaActiveS = 1.8f, IceFlat = 0.7f;
        public static int GroundSortingOrder = -20000, TopSortingOrder = 70; public static string SortingLayer = "Default"; public static Material MaterialOverride;
        public static bool Paused;
        /// <summary>Hellord v0.0.46: true = original 3 px ice patch instead of fx_redo Ice v2.</summary>
        public static bool ClassicIce;
        /// <summary>Hellord v0.0.46: true = original 3 px lava crack instead of fx_redo Lava v2.</summary>
        public static bool ClassicLava;
        /// <summary>Hellord v0.0.46: Lava v2 ACTIVE molten channel width multiplier (1 = MoneyMaker mock).</summary>
        public static float LavaActiveWidthMul = 1f;
        /// <summary>Hellord v0.0.46: Lava v2 floor glow wash + heat shimmer alpha multiplier (1 = MoneyMaker mock).</summary>
        public static float LavaGlowAlphaMul = 1f;

        public static void Prewarm(int items = 32) => R.Prewarm(items);
        public static void SetGroundSorting(int order) { GroundSortingOrder = order; if (HazRunner.Exists) R.ApplySorting(); }
        /// <summary>One per hero. Feed it every frame with <see cref="ChillSet"/>.</summary>
        public static int ChillBegin() => R.Add(HazKind.Chill, Vector2.zero, 0, 0);
        /// <summary>Hero feet, Chill stacks, stacks that freeze, seconds left frozen (0 = not frozen), seconds left immune. Transitions (gain, freeze, shatter) are detected here.</summary>
        public static void ChillSet(int h, Vector2 feet, int stacks, int freezeAt = 3, float frozenLeft = 0f, float immuneLeft = 0f) => R.ChillSet(h, feet, stacks, freezeAt, frozenLeft, immuneLeft);
        public static void ChillEnd(int h) => Cancel(h);
        /// <summary>Static ice patch (stays until <see cref="Cancel"/> / <see cref="Clear"/>). Radius in world units; drawn <see cref="IceFlat"/> as tall as wide.</summary>
        public static int IcePatch(Vector2 center, float radiusU) => R.Add(HazKind.IcePatch, center, radiusU, 0);
        /// <summary>Skid spray behind the feet; call every few frames while the hero slides on ice.</summary>
        public static void IceSkid(Vector2 feet, Vector2 vel) => R.Skid(feet, vel);
        /// <summary>Lava crack along a polyline (world units). Drive its state each frame with <see cref="LavaSet"/>.</summary>
        public static int LavaCrack(Vector2[] ptsU, float widthU = -1) => R.AddLava(ptsU, widthU > 0 ? widthU : LavaWidthU);
        /// <summary>State + progress 0..1 inside that state (Hellord: st and wp from the crack phase; grace = Dormant).</summary>
        public static void LavaSet(int h, LavaState state, float progress) => R.LavaSet(h, state, progress);
        public static void Cancel(int h) { if (HazRunner.Exists) R.Cancel(h); }
        public static void Clear() { if (HazRunner.Exists) R.ClearAll(); }
        public static int ActiveCount => HazRunner.Exists ? R.Count : 0;
        /// <summary>Smallest rect side emitted so far (QC: must stay at least 3).</summary>
        public static int MinSidePx => HazRunner.Exists ? R.MinSide : 999;
    }

    public class HazRunner : MonoBehaviour, ICraftSink
    {
        static HazRunner _i;
        public static bool Exists => _i != null;
        public static HazRunner I { get { if (_i == null) { var go = new GameObject("StatusHazardFX"); DontDestroyOnLoad(go); _i = go.AddComponent<HazRunner>(); } return _i; } }

        const int MaxPts = 192;
        struct It { public HazKind kind; public Vector2 a; public float r, w, age, fz, im, pfz, pop, fza, brk, u, at; public int seed, n, st, pst, fa, lst, lpst; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; Vector2[] _pts = new Vector2[0], _nrm = new Vector2[0];
        public int Count { get; private set; }
        public int MinSide { get; private set; } = 999;

        const int MaxQ = 24000 /* fx_redo: 1 px rows need ~3x the quads */;
        struct Q { public int x, y, w, h; public Color32 c; }
        readonly Q[] _gs = new Q[MaxQ], _gb = new Q[MaxQ], _gf = new Q[MaxQ], _tb = new Q[MaxQ], _tf = new Q[MaxQ]; int _ngs, _ngb, _ngf, _ntb, _ntf;
        Mesh _mg, _mt; MeshRenderer _rg, _rt; Vector3[] _vg, _vt; Color32[] _cg, _ct; int _lastG, _lastT;
        /* Endesga 32 */
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255),
            Ic = new Color32(0xc0, 0xcb, 0xdc, 255), I2 = new Color32(0x8b, 0x9b, 0xb4, 255), Sl = new Color32(0x5a, 0x69, 0x88, 255), Bd = new Color32(0x12, 0x4e, 0x89, 255),
            Lr = new Color32(0xe4, 0x3b, 0x44, 255), Ld = new Color32(0xa2, 0x26, 0x33, 255), Lp = new Color32(0xf6, 0x75, 0x7a, 255), Lk = new Color32(0x3e, 0x27, 0x31, 255), Lm = new Color32(0x73, 0x3e, 0x39, 255),
            Lw = new Color32(0xff, 0xf6, 0xd0, 255),   // fx_redo: pale hot core
            Ly = new Color32(0xfe, 0xe7, 0x61, 255), Lo = new Color32(0xfe, 0xae, 0x34, 255), Lf = new Color32(0xf7, 0x76, 0x22, 255),   // Task 7 lava v2: no red on the red floors
            Cn = new Color32(0x26, 0x2b, 0x44, 255), Cl = new Color32(0x3a, 0x44, 0x66, 255);   // cooled crust
        static float Hs(int a, int b) => Craft.Hs(a, b);
        static int Rn(float v) => Mathf.RoundToInt(v);
        static int Q3(float v) => Mathf.RoundToInt(v / 3f) * 3;
        float _clock;

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); var np = new Vector2[cap * MaxPts]; var nn = new Vector2[cap * MaxPts];
                Array.Copy(_pts, np, _pts.Length); Array.Copy(_nrm, nn, _nrm.Length); _pts = np; _nrm = nn;
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == HazKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mg != null) return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX StatusHazard White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = StatusHazardFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "StatusHazardFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] StatusHazardFX: no sprite shader found; set StatusHazardFX.MaterialOverride.");
            _mg = Make("StatusHazardFX Ground", m, MaxQ * 3, out _rg, out _vg, out _cg); _mt = Make("StatusHazardFX Top", m, MaxQ * 2, out _rt, out _vt, out _ct); ApplySorting();
        }
        Mesh Make(string nm, Material m, int qs, out MeshRenderer mr, out Vector3[] v, out Color32[] c)
        {
            var go = new GameObject(nm); go.transform.SetParent(transform, false);
            v = new Vector3[qs * 4]; c = new Color32[qs * 4]; var uv = new Vector2[qs * 4]; var tri = new int[qs * 6];
            for (int i = 0; i < qs; i++) { int a = i * 4, t = i * 6; tri[t] = a; tri[t + 1] = a + 1; tri[t + 2] = a + 2; tri[t + 3] = a; tri[t + 4] = a + 2; tri[t + 5] = a + 3;
                uv[a] = new Vector2(.25f, .25f); uv[a + 1] = new Vector2(.25f, .75f); uv[a + 2] = new Vector2(.75f, .75f); uv[a + 3] = new Vector2(.75f, .25f); }
            var mesh = new Mesh { name = nm }; mesh.MarkDynamic(); mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            go.AddComponent<MeshFilter>().sharedMesh = mesh; mr = go.AddComponent<MeshRenderer>(); if (m != null) mr.sharedMaterial = m;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            mesh.vertices = v; mesh.uv = uv; mesh.colors32 = c; mesh.triangles = tri; mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000f); return mesh;
        }
        public void ApplySorting()
        {
            if (_rg == null) return; _rg.sortingOrder = StatusHazardFX.GroundSortingOrder; _rt.sortingOrder = StatusHazardFX.TopSortingOrder;
            _rg.sortingLayerName = StatusHazardFX.SortingLayer; _rt.sortingLayerName = StatusHazardFX.SortingLayer;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != HazKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = HazKind.None; _free[_freeN++] = i; Count--; }

        public int Add(HazKind kind, Vector2 a, float rU, float wU)
        {
            if (_mg == null) Prewarm(32); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++;
            ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k = new It { gen = k.gen, kind = kind, a = a * ppu, r = rU * ppu, w = wU * ppu, seed = (i * 7919 + k.gen * 131) & 0xFFFF, pop = 9, fza = 9, brk = 9, at = 9, fa = 3, lpst = -1 };
            return Hd(i);
        }
        public int AddLava(Vector2[] ptsU, float wU)
        {
            if (ptsU == null || ptsU.Length < 2) return -1; int h = Add(HazKind.LavaCrack, ptsU[0], 0, wU); if (h < 0) return h;
            int i = h & 0xFFFF; ref var k = ref _it[i]; int o = i * MaxPts; float ppu = HellFX.PPU;
            for (int s = 0; s < ptsU.Length - 1 && k.n < MaxPts; s++)
            {
                Vector2 a = ptsU[s] * ppu, b = ptsU[s + 1] * ppu, d = b - a; float L = d.magnitude; if (L < 1e-3f) continue; d /= L; Vector2 n = new Vector2(-d.y, d.x);
                for (float t = 0; t < L && k.n < MaxPts; t += 3)
                {
                    float hz = Hs(k.seed, k.n); _pts[o + k.n] = a + d * t + n * (hz < 0.25f ? 2 : hz > 0.8f ? -2 : 0); _nrm[o + k.n] = n; k.n++;
                }
            }
            if (k.n < 2) { Free(i); return -1; }
            return h;
        }
        public void ChillSet(int h, Vector2 feet, int stacks, int fa, float fz, float im)
        {
            if (!Get(h, out int i) || _it[i].kind != HazKind.Chill) return; ref var k = ref _it[i];
            k.a = feet * HellFX.PPU; k.st = Mathf.Max(0, stacks); k.fa = Mathf.Clamp(fa, 1, 8); k.fz = Mathf.Max(0, fz); k.im = Mathf.Max(0, im);
        }
        public void LavaSet(int h, LavaState s, float u) { if (Get(h, out int i) && _it[i].kind == HazKind.LavaCrack) { _it[i].lst = (int)s; _it[i].u = Mathf.Clamp01(u); } }
        public void Skid(Vector2 feet, Vector2 vel)
        {
            if (_mg == null) Prewarm(32); Vector2 p = feet * HellFX.PPU, b = p - (vel.sqrMagnitude > 1e-6f ? vel.normalized : Vector2.zero) * 6; int sd = Mathf.FloorToInt(_clock * 977) & 0xFFFF;
            _deb.Spawn(CraftDebris.Snow, b, 2, 30, 20, Ic, I2, Ic, I2, sd, 0, 0.45f); _deb.Spawn(CraftDebris.Chip, b, 1, 40, 30, Wh, Ic, I2, Ic, sd + 1, 0, 0.3f);
        }
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { _deb.Clear(); for (int i = 0; i < _it.Length; i++) if (_it[i].kind != HazKind.None) Free(i); }

        /* ---------- emit: every coloured rect gets a dark outline rect in the backing pass ---------- */
        void Box(bool top, int x, int y, int w, int h, Color32 c)
        {
            if (w <= 0 || h <= 0) return; if (_fine) { if (w < 1) w = 1; if (h < 1) h = 1; } else { if (w < 3) { x -= (3 - w) / 2; w = 3; } if (h < 3) h = 3; }
            if (top) { if (_ntf >= MaxQ) return; _tf[_ntf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _tb[_ntb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            else { if (_ngf >= MaxQ) return; _gf[_ngf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _gb[_ngb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            int m = w < h ? w : h; if (m < MinSide) MinSide = m;
        }
        void Shadow(int x, int y, int w, int h, byte a) { if (w > 0 && h > 0 && _ngs < MaxQ) _gs[_ngs++] = new Q { x = x, y = y, w = w, h = h, c = new Color32(0x18, 0x14, 0x25, a) }; }
        /* ---------- PixelCraft sink ---------- */
        void ICraftSink.CBox(bool top, int x, int y, int w, int h, Color32 c) => Box(top, x, y, w, h, c);
        void ICraftSink.CShadow(int x, int y, int w, int h, byte a) => Shadow(x, y, w, h, a);
        readonly CraftDebris _deb = new CraftDebris(384);
        readonly CraftDebris1 _deb1 = new CraftDebris1(512);
        void Cell(bool top, Vector2 q, Color32 c) => Box(top, Rn(q.x) - 1, Rn(q.y) - 1, 3, 3, c);
        void CellXY(bool top, float x, float y, Color32 c) => Box(top, Rn(x) - 1, Rn(y) - 1, 3, 3, c);
        /* fx_redo: 2 px detail cells, used by the swapped Ice / Lava v2 */
        void Cell2(bool top, Vector2 q, Color32 c) => Box(top, Rn(q.x) - 1, Rn(q.y) - 1, 2, 2, c);
        void CellXY2(bool top, float x, float y, Color32 c) => Box(top, Rn(x) - 1, Rn(y) - 1, 2, 2, c);
        bool _fine; /* true while a 1 px fx_redo effect draws (1 px box minimum); false = original 3 px minimum */

        void Update()
        {
            if (_mg == null) return; float dt = StatusHazardFX.Paused ? 0f : Time.deltaTime; _clock += dt; _ngs = _ngb = _ngf = _ntb = _ntf = 0;
            for (int i = 0; i < _it.Length; i++)
            {
                if (_it[i].kind == HazKind.None) continue; ref var k = ref _it[i]; k.age += dt;
                if (k.kind == HazKind.IcePatch) { if (StatusHazardFX.ClassicIce) IceV0(ref k); else { _fine = true; Ice(ref k); _fine = false; } }
                else if (k.kind == HazKind.LavaCrack) { if (StatusHazardFX.ClassicLava) LavaV0(ref k, i, dt); else { _fine = true; Lava(ref k, i, dt); _fine = false; } }
            }
            for (int i = 0; i < _it.Length; i++) if (_it[i].kind == HazKind.Chill) Chill(ref _it[i], dt);   // hero status draws last (on top)
            _deb.Step(dt); _deb.Draw(this); _fine = true; _deb1.Step(dt); _deb1.Draw(this); _fine = false;
            Flush(_mg, _vg, _cg, ref _lastG, true); Flush(_mt, _vt, _ct, ref _lastT, false);
        }

        /* ---------- 183 hero chill / freeze ---------- */
        void Pip(Vector2 c, bool on, float pop)
        {
            if (!on) { Cell(true, c, Sl); return; }
            float g = pop < 0.1f ? 3 : 0;
            Cell(true, c + new Vector2(0, 3 + g), Wh); Cell(true, c + new Vector2(-3 - g, 0), Ic); Cell(true, c + new Vector2(3 + g, 0), I2); Cell(true, c + new Vector2(0, -3 - g), Bd); Cell(true, c, Ic);
        }
        void IceFrame(Vector2 f, float sc, float cracks, int seed)
        {
            f.x += 2; float hw = Q3(15 * sc), y0 = f.y - 3, y1 = f.y + Q3(24 * sc);
            for (int x = -(int)hw; x <= (int)hw; x += 3)
            {
                float j = Hs(seed, x) < 0.2f ? 3 : 0;
                CellXY(true, f.x + x, y1 + j, Hs(seed + 1, x) < 0.8f ? Ic : Wh); CellXY(false, f.x + x, y0 - j, Bd);
            }
            for (int y = (int)y0 + 3; y < (int)y1; y += 3) { float j = Hs(seed + 2, y) < 0.2f ? 3 : 0; CellXY(true, f.x - hw - j, y, Ic); CellXY(true, f.x + hw + j, y, I2); }
            for (int sx = -1; sx <= 1; sx += 2)
            {
                CellXY(true, f.x + sx * (hw - 3), y1 - 3, I2); CellXY(true, f.x + sx * (hw - 6), y1 - 6, sx > 0 ? I2 : Ic); CellXY(true, f.x + sx * (hw - 3), y0 + 3, Sl);
            }
            CellXY(true, f.x - hw + 3, y1 + 6, Ic); CellXY(true, f.x - hw + 6, y1 + 3, Wh); CellXY(true, f.x + hw - 6, y1 + 6, I2);
            if (cracks > 0) { int n = Mathf.FloorToInt(cracks * 4) + 1; for (int m = 0; m < n; m++) { CellXY(true, f.x - hw + 3 + m * 3, y1 - 6 - m * 3, Wh); CellXY(true, f.x + hw - 3 - m * 3, y0 + 6 + m * 3, Wh); } }
        }
        void Chill(ref It c, float dt)
        {
            Vector2 f = c.a;
            if (c.st > c.pst) { c.pop = 0; int sd = c.seed + Mathf.FloorToInt(c.age * 91); _deb.Spawn(CraftDebris.Chip, f + new Vector2(0, 24), 3, 40, 40, Ic, I2, Wh, Sl, sd, 0, 0.35f); _deb.Spawn(CraftDebris.Snow, f + new Vector2(0, 26), 3, 20, 10, Ic, I2, Ic, I2, sd + 7, 0.02f, 0.6f); }
            if (c.fz > 0 && c.pfz <= 0) { c.fza = 0; _deb.Spawn(CraftDebris.Shard, f + new Vector2(0, 28), 5, 50, 40, Wh, Ic, I2, I2, c.seed + 5, 0.01f, 0.4f); }
            if (c.fz <= 0 && c.pfz > 0)
            {
                c.brk = 0;
                for (int q = 0; q < 4; q++) { Vector2 p = f + new Vector2(q % 2 == 0 ? -15 : 15, q < 2 ? 24 : 0); _deb.Spawn(CraftDebris.Shard, p, 3, 80, 80, Wh, Ic, I2, I2, c.seed + 7 + q, 0.01f, 0.7f); _deb.Spawn(CraftDebris.Chip, p, 2, 60, 50, Ic, I2, Sl, Wh, c.seed + 19 + q, 0.015f, 0.6f); }
            }
            c.pop += dt; c.fza += dt; c.brk += dt; c.pst = c.st; c.pfz = c.fz;
            if (c.fz > 0)
            {
                Craft.DecalFrost(this, f, 16, Mathf.Min(0.3f, c.fza), 1f, c.seed, Sl, I2);
                float sc = c.fza < 0.15f ? 1f + 0.25f * (1f - Craft.EaseBack(c.fza / 0.15f)) : 1f;
                if (c.fza < 0.067f) Craft.RoughBand(this, true, f + new Vector2(0, 10), 21, 24, 0, Craft.Two, Wh, c.seed, 2f, 0.4f, 1f, 0, 24, 1.15f);   // one-step freeze pop, outside the body
                IceFrame(f, sc, Mathf.Max(0, (0.3f - c.fz) / 0.3f), c.seed);
            }
            else if (c.st > 0)
            {
                bool two = c.st >= 2; float span = Mathf.PI * (two ? 0.9f : 0.55f);
                Craft.RoughBand(this, false, f, 9, 12, Mathf.PI * 0.05f, span, two ? Ic : I2, c.seed, 1.5f, 0, 1, 0, 16, 0.45f);
                Craft.RoughBand(this, false, f, 9, 12, Mathf.PI * 1.05f, span, Sl, c.seed + 3, 1.5f, 0, 1, 0, 16, 0.45f);
                if (two) for (int sx = -1; sx <= 1; sx += 2) { CellXY(true, f.x + 2 + sx * 14, f.y + 10, Ic); CellXY(true, f.x + 2 + sx * 17, f.y + 13, sx < 0 ? Wh : I2); CellXY(true, f.x + 2 + sx * 14, f.y + 4, I2); }
                float iv = 0.5f / c.st;
                if (c.age % iv < dt) { int sx = Mathf.FloorToInt(c.age / iv) % 2 == 1 ? -1 : 1; _deb.Spawn(CraftDebris.Snow, f + new Vector2(2 + sx * 13, 18), 1, 6, -6, Ic, I2, Ic, I2, c.seed + Mathf.FloorToInt(c.age * 53), 0, 0.9f); }
                for (int j = 0; j < c.fa; j++) Pip(f + new Vector2(2 + (j - (c.fa - 1) * 0.5f) * 11, 29), j < c.st, j == c.st - 1 ? c.pop : 9);
            }
            if (c.im > 0 && c.fz <= 0) Craft.RoughBand(this, false, f, 12, 15, 0, Craft.Two, Sl, c.seed + 11, 0f, -1f, Mathf.Min(1f, c.im / 0.6f), c.seed, 24, 0.45f);
        }

        /* ---------- v0.0.46: ORIGINAL 184 ice / 185 lava, kept for StatusHazardFX.ClassicIce / ClassicLava ---------- */
        void IceV0(ref It k)
        {
            Vector2 c = k.a; float r = k.r, fl = StatusHazardFX.IceFlat; int s = k.seed;
            Craft.Blob(this, false, c, r - 3, Bd, Sl, I2, I2, 3, s, 0.08f, 1f, fl);
            Craft.RoughBand(this, false, c, r - 3, r, 0, Craft.Two, Ic, s, 2f, 0, 1, 0, 24, fl);
            for (int q = 0; q < 3; q++)
            {
                float a = Hs(s, q) * Craft.Two; int L = Mathf.FloorToInt(r * 0.45f / 3f); float ca = Mathf.Cos(a), sa = Mathf.Sin(a);
                for (int m = 0; m < L; m++) CellXY(false, c.x + ca * (r * 0.15f + m * 3) + (Hs(s + q, m) < 0.3f ? 3 : 0), c.y + sa * (r * 0.15f + m * 3) * fl, m % 3 != 0 ? Ic : I2);
            }
            float u = (_clock * 0.4f + Hs(s, 9)) % 1f;   // glint slides across, never blinks
            if (u < 0.6f) { Vector2 q = c + new Vector2((u / 0.6f - 0.5f) * r * 1.2f, (u / 0.6f - 0.5f) * r * 0.5f); Cell(false, q, Wh); Cell(false, q + new Vector2(3, 0), Ic); Cell(false, q - new Vector2(3, 0), Ic); }
        }

        /* ---------- 185 lava crack ---------- */
        void Caps(int o, int n, float hw, Color32 c)
        {
            for (int e = 0; e < 2; e++)
            {
                Vector2 p = _pts[o + (e == 0 ? 0 : n - 1)], d = p - _pts[o + (e == 0 ? 1 : n - 2)]; d = d.sqrMagnitude > 1e-6f ? d.normalized : Vector2.right;
                for (int j = -1; j <= 1; j++) { float a = j * 0.8f, ca = Mathf.Cos(a), sa = Mathf.Sin(a); Cell(false, p + new Vector2(d.x * ca - d.y * sa, d.x * sa + d.y * ca) * hw, c); }
            }
        }
        void LavaV0(ref It L, int i, float dt)
        {
            int o = i * MaxPts, n = L.n, s = L.seed; float hw = L.w * 0.5f + 0.3f * HellFX.PPU, u = L.u, t = _clock;
            if (L.lst == 2 && L.lpst != 2) { L.at = 0; for (int k = 0; k < n; k += 5) _deb.Spawn(CraftDebris.Chip, _pts[o + k], 2, 60, 70, Lp, Lr, Ld, Lk, s + k, 0.004f, 0.5f); }
            L.lpst = L.lst; L.at += dt;
            for (int k = 0; k < n; k++)
            {
                Cell(false, _pts[o + k], Lk);
                if (k % 7 == 3) Cell(false, _pts[o + k] + _nrm[o + k] * (Hs(s, k) < 0.5f ? 3 : -3) + new Vector2(3, 0), Lk);
            }
            if (L.lst == 0) { for (int k = 0; k < n; k += 2) Cell(false, _pts[o + k], Lm); return; }
            if (L.lst == 1)
            {
                int g = Mathf.FloorToInt(u * 4);   // glow steps up 0..3, no blinking
                for (int k = 0; k < n; k++) { Cell(false, _pts[o + k], g < 2 ? Ld : Lr); if (g >= 2 && k % 2 == 0) { Cell(false, _pts[o + k] + _nrm[o + k] * 3, Ld); Cell(false, _pts[o + k] - _nrm[o + k] * 3, Ld); } }
                float vis = Mathf.Min(1f, u / 0.45f); Color32 c = u >= 0.85f ? Wh : Lp; int m = Mathf.FloorToInt(n * vis);
                for (int k = 0; k < m; k += 2) for (int sg = -1; sg <= 1; sg += 2) Cell(false, _pts[o + k] + _nrm[o + k] * (sg * (hw + (Hs(s + sg, k) < 0.2f ? 3 : 0))), c);
                if (vis >= 1) Caps(o, n, hw, c);
                if (t % (0.2f - 0.12f * u) < dt) _deb.Spawn(CraftDebris.Ember, _pts[o + Mathf.Min(n - 1, Mathf.FloorToInt(Hs(s, Mathf.FloorToInt(t * 30)) * n))], 1, 8, 25, Lk, Lk, Lk, Lk, s + Mathf.FloorToInt(t * 99), 0, 0.7f);
                if (u > 0.7f && t % 0.1f < dt) _deb.Spawn(CraftDebris.Chip, _pts[o + Mathf.Min(n - 1, Mathf.FloorToInt(Hs(s + 5, Mathf.FloorToInt(t * 30)) * n))], 1, 20, 50, Lk, Ld, Lk, Ld, s + Mathf.FloorToInt(t * 71), 0, 0.3f);
                return;
            }
            float left = StatusHazardFX.LavaActiveS * (1 - u), sc = 1f;
            if (left < 0.45f) sc = Mathf.Max(0.35f, 1f - (Mathf.Floor((0.45f - left) / 0.15f) + 1) * 0.22f);
            for (int k = 0; k < n; k += 3)
                Craft.Blob(this, false, _pts[o + k], (hw - 5) * sc * (0.85f + 0.3f * Hs(s, k)), Ld, Lr, Lp, Lp, 3, s + k * 7 + Mathf.FloorToInt(t * 6) % 2, 0.3f, 1f, 0.8f, left > 0.3f ? 1f : left / 0.3f, s + k);
            if (L.at < 0.067f) for (int k = 0; k < n; k += 9) Craft.Impact(this, _pts[o + k], 10, L.at, s + k, Lp, Ld);
            if (left > 0.35f)
            {
                for (int k = 2; k < n; k += 8) Craft.Tongue(this, true, _pts[o + k].x, _pts[o + k].y - 2, 5 + 5 * Hs(s, k + Mathf.FloorToInt(t * 4)), 6, Lr, Lp, s + k * 13, t, 0f);
                for (int k = 0; k < n; k++) Cell(false, _pts[o + k], Lp);
                for (int k = 0; k < n; k += 2) for (int sg = -1; sg <= 1; sg += 2) Cell(false, _pts[o + k] + _nrm[o + k] * (sg * (hw + (Hs(s + sg, k) < 0.2f ? 3 : 0))), Lp);
                Caps(o, n, hw, Lp);
                if (t % 0.12f < dt) _deb.Spawn(CraftDebris.Chip, _pts[o + Mathf.Min(n - 1, Mathf.FloorToInt(Hs(s + 9, Mathf.FloorToInt(t * 30)) * n))], 1, 25, 55, Lp, Lr, Lp, Lr, s + Mathf.FloorToInt(t * 57), 0, 0.35f);
            }
            else if (t % 0.08f < dt) _deb.Spawn(CraftDebris.Ember, _pts[o + Mathf.Min(n - 1, Mathf.FloorToInt(Hs(s + 3, Mathf.FloorToInt(t * 30)) * n))], 1, 8, 22, Lk, Lk, Lk, Lk, s + Mathf.FloorToInt(t * 91), 0, 0.8f);
        }

        /* ---------- shared 1 px helpers (hazards remake) ---------- */
        static int Rh(float v) => Mathf.FloorToInt(v + 0.5f);   // round half up (matches the Python mock-up)
        void Line1(float ax, float ay, float bx, float by, Color32 c)   // 1 px line, one pixel per major-axis step
        {
            float dx = bx - ax, dy = by - ay; int n = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy))));
            for (int i = 0; i <= n; i++) Box(false, Rh(ax + dx * i / n), Rh(ay + dy * i / n), 1, 1, c);
        }
        void Disc(float x, float y, float R, Color32 c)
        {
            if (R < 0.5f) return; int xi = Rh(x), yi = Rh(y), ri = Mathf.FloorToInt(R);
            for (int dy = -ri; dy <= ri; dy++) { int hw = Mathf.FloorToInt(Mathf.Sqrt(Mathf.Max(0f, R * R - dy * dy)) + 0.35f); Box(false, xi - hw, yi + dy, 2 * hw + 1, 1, c); }
        }

        /* ---------- 184 ice patch: faceted slab, dark thickness lip, bevel, 45 deg gloss streaks + travelling glint ---------- */
        static readonly float[] _vx = new float[12], _vy = new float[12];
        static readonly int[] _r0 = new int[256], _r1 = new int[256];
        static void IcePoly(float cx, float cy, float r, float sc, int seed, float fl)
        {
            float a0 = Hs(seed, 40) * 0.5f;
            for (int i = 0; i < 12; i++) { float a = a0 + i * Craft1.Two / 12f, rr = r * sc * (0.9f + 0.16f * Hs(seed, i)); _vx[i] = cx + Mathf.Cos(a) * rr; _vy[i] = cy + Mathf.Sin(a) * rr * fl; }
        }
        static bool Span(int y, out int x0, out int x1)
        {
            float yc = y + 0.5f, xl = 1e9f, xr = -1e9f;
            for (int i = 0; i < 12; i++)
            {
                float px = _vx[i], py = _vy[i], qx = _vx[(i + 1) % 12], qy = _vy[(i + 1) % 12];
                if ((py - yc) * (qy - yc) <= 0 && py != qy) { float x = px + (yc - py) * (qx - px) / (qy - py); if (x < xl) xl = x; if (x > xr) xr = x; }
            }
            x0 = Mathf.CeilToInt(xl - 0.5f); x1 = Mathf.FloorToInt(xr - 0.5f); return xl <= xr && x0 <= x1;
        }
        /* ---------- 184 ice patch v2 (Task 7): frost spread -> dendrites, lip, body, facets, 1 px bright rim, sweeping sheen, popping shards, glints ---------- */
        static float Eo3(float u) { u = Mathf.Clamp01(u); float v = 1 - u; return 1 - v * v * v; }
        void Wash(int x, int y, int w, int h, Color32 c, byte a) { if (w > 0 && h > 0 && _ngs < MaxQ) _gs[_ngs++] = new Q { x = x, y = y, w = w, h = h, c = new Color32(c.r, c.g, c.b, a) }; }
        void Ice(ref It k)
        {
            float r = k.r, fl = StatusHazardFX.IceFlat, t = _clock, age = k.age; int sd = k.seed;
            float g = Eo3(age / 0.38f), sc = 0.18f + 0.82f * g;   // frost spread from the centre (ease-out, 0.38 s)
            int cx = Rh(k.a.x), cy = Rh(k.a.y); IcePoly(cx + 0.5f, cy + 0.5f, r, sc, sd, fl);
            int ry = Mathf.Min(127, Mathf.CeilToInt(r * sc * 1.06f * fl) + 1), y0 = cy - ry, nr = 2 * ry + 1; float R = r * sc;
            for (int y = cy - ry; y <= cy + ry; y++) { int j = y - y0; if (!Span(y, out _r0[j], out _r1[j])) { _r0[j] = 1; _r1[j] = 0; } }
            // 1 frost dendrites creeping out past the slab
            float g2 = Mathf.Clamp01((age - 0.05f) / 0.55f);
            for (int b = 0; b < 7; b++)
            {
                float an = b * Craft1.Two / 7f + Hs(sd, 50 + b) * 0.6f, px = cx + Mathf.Cos(an) * R * 0.92f, py = cy + Mathf.Sin(an) * R * 0.92f * fl;
                int ns = (int)(g2 * 3.999f) + (g2 > 0 ? 1 : 0);
                for (int m = 0; m < Mathf.Min(3, ns); m++)
                {
                    float bb = an + (Hs(sd + b, m) - 0.5f) * 0.8f, ln = 4 - m, nx = px + Mathf.Cos(bb) * ln, ny = py + Mathf.Sin(bb) * ln * fl;
                    Line1(px, py, nx, ny, m < 2 ? Sl : I2);
                    if (m < 2) { float tw = bb + ((m + b) % 2 == 0 ? 1f : -1f); Line1(nx, ny, nx + Mathf.Cos(tw) * 2.5f, ny + Mathf.Sin(tw) * 2.5f * fl, Sl); }
                    px = nx; py = ny;
                }
                if (ns >= 3) Box(false, Rh(px), Rh(py), 1, 1, Ic);
            }
            // 2 thickness lip
            for (int j = 0; j < nr; j++) { int y = y0 + j; if (_r0[j] <= _r1[j] && y < cy) Box(false, _r0[j], y - 2, _r1[j] - _r0[j] + 1, 2, Bd); }
            // 3 body + lit upper-left zone
            float kz = R * 0.30f;
            for (int j = 0; j < nr; j++)
            {
                int y = y0 + j, a = _r0[j], b = _r1[j]; if (a > b) continue; Box(false, a, y, b - a + 1, 1, I2);
                int xz = Mathf.Min(b, Mathf.FloorToInt(cx + (y - cy) / fl - kz)); if (xz >= a) Box(false, a, y, xz - a + 1, 1, Ic);
            }
            // 4 facets: centre -> alternate vertices
            for (int i = 0; i < 12; i += 2)
            {
                float vx = _vx[i], vy = _vy[i]; bool lit = (vx - cx) - (vy - cy) / fl < 0;
                Line1(cx + (vx - cx) * 0.15f, cy + (vy - cy) * 0.15f, cx + (vx - cx) * 0.78f, cy + (vy - cy) * 0.78f, lit ? Wh : Sl);
            }
            // 5 1 px bright rim (Ko backing outside = dark outline)
            for (int j = 0; j < nr; j++)
            {
                int y = y0 + j, a = _r0[j], b = _r1[j]; if (a > b) continue;
                bool up = j + 1 < nr && _r0[j + 1] <= _r1[j + 1], dn = j > 0 && _r0[j - 1] <= _r1[j - 1];
                if (!up) Box(false, a, y, b - a + 1, 1, Wh); else { if (_r0[j + 1] > a) Box(false, a, y, _r0[j + 1] - a, 1, Wh); if (_r1[j + 1] < b) Box(false, _r1[j + 1] + 1, y, b - _r1[j + 1], 1, Ic); }
                if (!dn) Box(false, a, y, b - a + 1, 1, Ic); else { if (_r0[j - 1] > a) Box(false, a, y, _r0[j - 1] - a, 1, Wh); if (_r1[j - 1] < b) Box(false, _r1[j - 1] + 1, y, b - _r1[j - 1], 1, Ic); }
                Box(false, a, y, 1, 1, Wh); Box(false, b, y, 1, 1, Ic);
            }
            // 6 sheen: 45 deg band sweeps every 2.4 s + static streak
            float ph = ((t + Hs(sd, 9) * 2.4f) % 2.4f) / 2.4f;
            if (ph < 0.55f && g >= 1f)
            {
                float off = -1.6f * R + 3.2f * R * (ph / 0.55f);
                for (int j = 0; j < nr; j++)
                {
                    int y = y0 + j, a = _r0[j], b = _r1[j];
                    for (int x = a + 1; x < b; x++) { float dd = Mathf.Abs(x - cx - (y - cy) - off); if (dd < 1.5f) Box(false, x, y, 1, 1, Wh); else if (dd < 2.5f) Box(false, x, y, 1, 1, Ic); }
                }
            }
            for (int j = 0; j < nr; j++)
            {
                int y = y0 + j; if (_r0[j] > _r1[j] || Mathf.Abs(y - cy) > R * fl * 0.45f) continue;
                int x = cx + (y - cy) - (int)(R * 0.35f); if (x >= _r0[j] + 1 && x <= _r1[j] - 1) Box(false, x, y, 1, 1, Wh);
            }
            // 7 shards: 6 crystals pop up staggered 50 ms, 1-frame overshoot
            int nt = 0;
            for (int j = 0; j < 6; j++)
            {
                float an = (j + 0.5f) * Craft1.Two / 6f + Hs(sd, 60 + j) * 0.5f, ta = age - 0.18f - j * 0.05f; if (ta < 0) continue;
                int h = 6 + (int)(Hs(sd, 70 + j) * 4); if (ta < 2f / 60f) h += 2;
                int bx = Rh(cx + Mathf.Cos(an) * R * 0.86f), by = Rh(cy + Mathf.Sin(an) * R * 0.86f * fl) - 1;
                for (int i = 0; i < h; i++)
                {
                    int wd = i < h * 0.4f ? 3 : i < h * 0.75f ? 2 : 1;
                    for (int q = 0; q < wd; q++) Box(false, bx - 1 + q + (wd == 1 ? 1 : 0), by + i, 1, 1, q == 0 ? Wh : (q == 1 || wd == 1) ? Ic : Sl);
                }
                Box(false, bx, by + h, 1, 1, Wh); _tx[nt] = bx; _ty[nt] = by + h; nt++;
            }
            // 8 glints: 2 slots, 4-step star every 1.3 s
            for (int q = 0; q < 2 && nt > 0; q++)
            {
                float tt = t + Hs(sd, 11 + q) * 1.3f + q * 0.65f, p2 = tt % 1.3f; int ci = (int)(tt / 1.3f); if (p2 >= 0.28f) continue;
                int st = (int)(p2 / 0.07f), sx, sy;
                if (q == 0) { int ti = ((int)(Hs(sd + ci, 3) * nt)) % nt; sx = _tx[ti]; sy = _ty[ti]; }
                else { sx = Rh(cx - R * 0.4f + Hs(sd + ci, 1) * R * 0.3f); sy = Rh(cy + R * fl * (0.1f + Hs(sd + ci, 2) * 0.3f)); }
                if (st == 1 || st == 2) { Color32 cc = st == 2 ? Wh : Ic; Box(false, sx + 1, sy, 1, 1, cc); Box(false, sx - 1, sy, 1, 1, cc); Box(false, sx, sy + 1, 1, 1, cc); Box(false, sx, sy - 1, 1, 1, cc); }
                if (st == 2) { Box(false, sx + 2, sy, 1, 1, Ic); Box(false, sx - 2, sy, 1, 1, Ic); Box(false, sx, sy + 2, 1, 1, Ic); Box(false, sx, sy - 2, 1, 1, Ic); Box(false, sx + 3, sy, 1, 1, Ic); Box(false, sx - 3, sy, 1, 1, Ic); }
                Box(false, sx, sy, 1, 1, st == 3 ? Ic : Wh);
            }
        }
        static readonly int[] _tx = new int[6], _ty = new int[6];

        /* ---------- 185 lava crack v2 (Task 7): navy seam -> heating warning -> orange/yellow/cream molten channel (no red) with crust floes, bubbles, shimmer, embers -> stepped cooling ---------- */
        float Rk(ref It L, int k, int n) { float hw = L.w * 0.5f, tp = Mathf.Min(1f, Mathf.Min((k + 1) / 4f, (n - k) / 4f)); return hw * (0.72f + 0.28f * Hs(L.seed, k)) * (0.35f + 0.65f * tp); }
        Vector2 At(int o, int n, float sarc, out Vector2 nn, out int i)
        {
            float f = sarc / 3f; i = Mathf.Min(n - 2, Mathf.Max(0, (int)f)); float u = Mathf.Clamp01(f - i); nn = _nrm[o + i]; return _pts[o + i] * (1 - u) + _pts[o + i + 1] * u;
        }
        void Seg(int o, int k, Color32 c) { Vector2 a = _pts[o + k - 1], b = _pts[o + k]; Line1(a.x, a.y, b.x, b.y, c); }
        int Pick(int n, int s, float t) => Mathf.Min(n - 1, Mathf.FloorToInt(Hs(s, Mathf.FloorToInt(t * 30)) * n));
        static readonly float[] LIns = { 0f, 1f, 3.6f, 6f }; static readonly float[] LShr = { 1f, 1f, 0.95f, 0.8f, 0.6f }; static readonly int[] LFloe = { 11, 9, 6, 5, 4 };
        void Lava(ref It L, int i, float dt)
        {
            int o = i * MaxPts, n = L.n, sd = L.seed; float u = L.u, t = _clock, hw = L.w * 0.5f;
            if (L.lst == 2 && L.lpst != 2) { L.at = 0; for (int k = 0; k < n; k += 5) _deb1.Spawn(CraftDebris1.Chip, _pts[o + k], 2, 70, 90, Lw, Ly, Lo, Ly, sd + k, 0.004f, 0.55f); }
            L.lpst = L.lst; L.at += dt;
            for (int k = 1; k < n; k++) Seg(o, k, Cn);
            for (int k = 3; k < n; k += 7) { Vector2 p = _pts[o + k], q = p + _nrm[o + k] * (Hs(sd, k) < 0.5f ? 4 : -4); Line1(p.x, p.y, q.x + 2, q.y, Cn); }
            if (L.lst == 0)
            {
                for (int k = 1; k < n; k++) Seg(o, k, Cl);
                for (int k = 2; k < n; k += 6) if (Mathf.Sin(t * 2.2f + Hs(sd, k) * 6.28f) > 0.55f) Box(false, Rh(_pts[o + k].x), Rh(_pts[o + k].y), 1, 1, Lf);
                return;
            }
            if (L.lst == 1)
            {
                int g = Mathf.Min(3, Mathf.FloorToInt(u * 4));
                if (g >= 2) for (int k = 0; k < n; k++) Box(false, Rh(_pts[o + k].x) - 1, Rh(_pts[o + k].y) - 1, 3, 3, Lf);
                Color32 sc = g == 0 ? Cl : g == 1 ? Lf : g == 2 ? Lo : Ly; for (int k = 1; k < n; k++) Seg(o, k, sc);
                float zr = hw + 2, vis = Mathf.Min(1f, u / 0.3f); int lo = (int)(n * 0.5f * (1 - vis)), hi = n - lo;
                bool now = u >= 0.85f, off = u >= 0.90f && u < 0.925f; Color32 col = now && !off ? Wh : Ly; int mo = Mathf.FloorToInt(t * 24);
                for (int k = Mathf.Max(1, lo); k < hi; k++)
                {
                    if (!now && ((k * 3 + mo) % 9) >= 5) continue;
                    float zk = zr * (0.35f + 0.65f * Mathf.Min(1f, Mathf.Min((k + 1) / 4f, (n - k) / 4f))), zj = zr * (0.35f + 0.65f * Mathf.Min(1f, Mathf.Min(k / 4f, (n - k + 1) / 4f)));
                    for (int sg = -1; sg <= 1; sg += 2) { Vector2 a = _pts[o + k - 1] + _nrm[o + k - 1] * sg * zj, b = _pts[o + k] + _nrm[o + k] * sg * zk; Line1(a.x, a.y, b.x, b.y, col); }
                }
                if (g >= 2)
                    for (int j = 0; j < 2; j++)
                    {
                        float cyc = t * 5 + j * 0.5f, ph = cyc % 1f; int kk = (int)(Hs(sd + j, (int)cyc) * (n - 2)) + 1; Vector2 p = _pts[o + kk];
                        if (ph < 0.3f) Box(false, Rh(p.x), Rh(p.y), 2, 2, Lo); else if (ph < 0.45f) Box(false, Rh(p.x), Rh(p.y) + 1, 1, 1, Lw);
                    }
                if (t % (0.2f - 0.12f * u) < dt) _deb1.Spawn(CraftDebris1.Ember, _pts[o + Pick(n, sd, t)], 1, 8, 25, Ly, Lo, Lf, Lo, sd + Mathf.FloorToInt(t * 99), 0, 0.7f);
                return;
            }
            // ACTIVE
            float left = StatusHazardFX.LavaActiveS * (1 - u); int cs = left >= 0.6f ? 0 : Mathf.Min(4, (int)((0.6f - left) / 0.15f) + 1);
            float shr = LShr[cs] * StatusHazardFX.LavaActiveWidthMul, ew = L.at < 0.25f ? 1f + 0.25f * (1f - L.at / 0.25f) : 1f, S = 3f * (n - 1);
            for (int k = 0; k < n; k += 3) { float R = Rk(ref L, k, n) * shr * ew + 4; Wash(Rh(_pts[o + k].x - R), Rh(_pts[o + k].y - R * 0.8f), (int)(2 * R), (int)(1.6f * R), Lf, (byte)Mathf.Clamp((cs <= 1 ? 34 : 18) * StatusHazardFX.LavaGlowAlphaMul, 0f, 255f)); }
            for (int layer = 0; layer < 4; layer++)
            {
                Color32 c;
                if (cs == 0) c = layer == 0 ? Lo : layer == 1 ? Lf : layer == 2 ? Ly : Lw;
                else if (cs == 1) { if (layer == 3) continue; c = layer == 0 ? Lo : layer == 1 ? Lf : Ly; }
                else { if (layer >= 2) continue; c = cs == 2 ? (layer == 0 ? Lf : Lo) : cs == 3 ? (layer == 0 ? Lf : Cl) : (layer == 0 ? Cl : Cn); }
                for (int k = 0; k < n; k += 2) { float R = Rk(ref L, k, n) * shr * ew + (Hs(sd + k, Mathf.FloorToInt(t * 6)) < 0.25f && cs == 0 ? 1 : 0); Disc(_pts[o + k].x, _pts[o + k].y, R - LIns[layer], c); }
            }
            if (cs <= 1)
                for (int j = 0; j < (int)(S / 9) + 1; j++) { Vector2 p = At(o, n, (j * 9 + t * 22) % S, out Vector2 nn, out int ii); if (Rk(ref L, ii, n) * shr < 4f) continue; Box(false, Rh(p.x), Rh(p.y), cs == 0 ? 3 : 2, 1, cs == 0 ? Lw : Ly); }
            int nf = (int)(S / LFloe[cs]);
            for (int j = 0; j <= nf; j++)
            {
                Vector2 p = At(o, n, (j * S / (nf + 1) + t * (cs <= 1 ? 9 : 3)) % S, out Vector2 nn, out int ii); float R = Rk(ref L, ii, n) * shr; if (R < 3.5f) continue;
                float of = (Hs(sd + 7, j) - 0.5f) * 2 * (R - 2.5f); int x = Rh(p.x + nn.x * of), y = Rh(p.y + nn.y * of), fw = Hs(sd + 8, j) < 0.5f ? 4 : 3;
                Box(false, x, y, fw, 2, Cn); Box(false, x, y + 1, fw - 1, 1, Cl);
            }
            if (cs <= 1)
                for (int j = 0; j < 3; j++)
                {
                    float cyc = t * 0.9f + j / 3f, ph = cyc % 1f; int kk = (int)(Hs(sd + 11 + j, (int)cyc) * (n - 6)) + 3; if (kk >= n || Rk(ref L, kk, n) < 4.5f) continue;
                    Vector2 p = _pts[o + kk] + _nrm[o + kk] * ((Hs(sd + 12 + j, (int)cyc) - 0.5f) * 3); int x = Rh(p.x), y = Rh(p.y);
                    if (ph < 0.32f) { int rr = 1 + (int)(ph / 0.16f); Box(false, x - rr, y, 2 * rr + 1, 1, Ly); Box(false, x - rr + 1, y + rr, 2 * rr - 1, 1, Ly); Box(false, x, y + 1, 1, 1, Lw); }
                    else if (ph < 0.38f)
                    {
                        Box(false, x, y, 1, 1, Wh); Box(false, x + 2, y, 1, 1, Wh); Box(false, x - 2, y, 1, 1, Wh); Box(false, x, y + 2, 1, 1, Wh); Box(false, x, y - 2, 1, 1, Wh);
                        if ((cyc - dt * 0.9f) % 1f < 0.32f) _deb1.Spawn(CraftDebris1.Chip, new Vector2(x, y), 3, 30, 45, Lw, Ly, Lo, Ly, sd + (int)cyc * 7 + j, 0, 0.35f);
                    }
                }
            if (cs < 3)
                for (int k = 1; k < n; k += 4)
                {
                    float R = Rk(ref L, k, n) * shr, h = (t * 12 + Hs(sd, 90 + k) * 10) % 10, x = _pts[o + k].x + Mathf.Sin(t * 6 + k) * 1.5f;
                    Wash(Rh(x - 2), Rh(_pts[o + k].y + R + 2 + h), 4, 1, Ly, (byte)Mathf.Clamp(70 * (1 - h / 10) * StatusHazardFX.LavaGlowAlphaMul, 0f, 255f));
                }
            if (L.at < 0.1f) for (int k = 0; k < n; k += 9) Craft1.Impact(this, _pts[o + k], 8, L.at, sd + k, Lw, Cn);
            if (L.at < 0.5f) for (int k = 2; k < n; k += 8) Craft1.Tongue(this, true, _pts[o + k].x, _pts[o + k].y - 2, (6 + 6 * Hs(sd, k + Mathf.FloorToInt(t * 4))) * (1 - L.at / 0.5f) + 3, 5, Lf, Ly, Lw, sd + k * 13, t, 0f);
            if (cs <= 1 && t % 0.09f < dt) _deb1.Spawn(CraftDebris1.Ember, _pts[o + Pick(n, sd + 9, t)], 1, 10, 30, Ly, Lo, Lf, Lo, sd + Mathf.FloorToInt(t * 57), 0, 0.8f);
            if (cs >= 2 && t % 0.1f < dt) _deb1.Spawn(CraftDebris1.Ember, _pts[o + Pick(n, sd + 3, t)], 1, 8, 22, Cl, Lf, Cl, Lf, sd + Mathf.FloorToInt(t * 91), 0, 0.8f);
        }

        void Flush(Mesh mesh, Vector3[] v, Color32[] c, ref int last, bool ground)
        {
            int q = 0; float s = 1f / HellFX.PPU;
            if (ground) { Put(_gs, _ngs, v, c, ref q, s); Put(_gb, _ngb, v, c, ref q, s); Put(_gf, _ngf, v, c, ref q, s); }
            else { Put(_tb, _ntb, v, c, ref q, s); Put(_tf, _ntf, v, c, ref q, s); }
            for (int j = q; j < last; j++) { int a = j * 4; v[a] = v[a + 1] = v[a + 2] = v[a + 3] = Vector3.zero; }
            last = q; mesh.vertices = v; mesh.colors32 = c;
        }
        static void Put(Q[] src, int n, Vector3[] v, Color32[] c, ref int q, float s)
        {
            int cap = v.Length / 4;
            for (int j = 0; j < n && q < cap; j++, q++)
            {
                var r = src[j]; int a = q * 4; float x0 = r.x * s, y0 = r.y * s, x1 = (r.x + r.w) * s, y1 = (r.y + r.h) * s;
                v[a] = new Vector3(x0, y0, 0); v[a + 1] = new Vector3(x0, y1, 0); v[a + 2] = new Vector3(x1, y1, 0); v[a + 3] = new Vector3(x1, y0, 0);
                c[a] = c[a + 1] = c[a + 2] = c[a + 3] = r.c;
            }
        }
    }
}
