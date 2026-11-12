using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="MinionFX"/> item.</summary>
    public enum MinionKind { None = 0, FrostCharge = 1, FrostBolt = 2, WheelSpin = 3, WheelTrail = 4 }
    /// <summary>Phase of a <see cref="MinionFX"/> item.</summary>
    public enum MinionPhase { None = 0, Tell = 1, Fly = 2, Hit = 3, Fizzle = 4, Trail = 5 }

    /// <summary>
    /// 179-182. Hellord v0.0.44 enemy-attack FX (v2.11.0): Frost Caster (variant 3) and Fire Wheel (variant 4). World space, 16 PPU, sizes in world units like HellordTuning.
    /// Lighter than the boss FX: ONE wobbly 3 px border (dark #181425 outline) that turns white once in the last 15 % of the tell, a faint shadow fill, smaller bodies.
    /// Frost Caster colours: violet #b55088, dark slate #3a4466, white core (never the hero cyan #2ce8f5, never the Ice Gorilla blues).
    /// Fire Wheel colours: rust #be4a2f, dark rust #733e39, peach #e8b796 (never the hero golds #fee761 / #feae34 / #f77622).
    /// 179 FROST CHARGE glow + aim lane (<see cref="FrostCharge"/>), 180 ICE BOLT + impact (<see cref="FrostBolt"/>, <see cref="FrostBoltMove"/>, <see cref="FrostBoltHit"/>, <see cref="FrostBoltEnd"/>),
    /// 181 WHEEL SPIN-UP sparks + roll lane (<see cref="WheelSpin"/>), 182 ROLLING FIRE TRAIL + stop (<see cref="WheelTrailBegin"/>, <see cref="WheelTrailAdd"/>, <see cref="WheelTrailEnd"/>).
    /// Lanes, trail and scorch are on the GROUND mesh (under characters); glow, bolt, sparks and flame tongues on the TOP mesh. Rects at least 3 px, opaque; only shadows use alpha.
    /// Visual only: the game keeps damage, Chill and collisions. Two meshes, one material, fixed arrays, int handles (-1 when full; every call accepts -1).
    /// </summary>
    public static class MinionFX
    {
        static MinionRunner R => MinionRunner.I;
        /// <summary>Defaults = HellordTuning v0.0.44 values.</summary>
        public static float FrostTell = 0.6f, FrostBoltRadius = 0.35f, WheelWind = 0.7f, WheelHalfWidth = 0.7f, WheelRollMax = 12f, FireTrailLife = 1.6f, FireTrailRadius = 0.45f;
        public static int GroundSortingOrder = -20000, TopSortingOrder = 70; public static string SortingLayer = "Default"; public static Material MaterialOverride;
        public static bool Paused;
        /// <summary>(handle, kind) when a wind-up ends (frost charge -> fire the bolt; wheel spin -> start rolling).</summary>
        public static Action<int, MinionKind> OnStrike;

        public static void Prewarm(int items = 64) => R.Prewarm(items);
        public static void SetGroundSorting(int order) { GroundSortingOrder = order; if (MinionRunner.Exists) R.ApplySorting(); }
        /// <summary>Frost Caster charge at the hand <paramref name="hand"/>: glow with overshoot + crystal spikes, aim lane of <paramref name="length"/> u along <paramref name="dir"/>.</summary>
        public static int FrostCharge(Vector2 hand, Vector2 dir, float length, float tell = -1)
            => R.Add(MinionKind.FrostCharge, hand, dir, length, FrostBoltRadius + 0.2f, tell >= 0 ? tell : FrostTell, 0);
        /// <summary>Ice bolt. Move it each frame with <see cref="FrostBoltMove"/>; end it with <see cref="FrostBoltHit"/> (impact) or <see cref="FrostBoltEnd"/> (fizzle).</summary>
        public static int FrostBolt(Vector2 pos, Vector2 dir) => R.Add(MinionKind.FrostBolt, pos, dir, 0, FrostBoltRadius, 0.2f, 0);
        public static void FrostBoltMove(int h, Vector2 pos) => R.Move(h, pos);
        public static void FrostBoltHit(int h, Vector2 pos) => R.Hit(h, pos);
        public static void FrostBoltEnd(int h) => R.Fizzle(h);
        /// <summary>Fire Wheel wind-up at <paramref name="pos"/>: spin arc speeding up, sparks, roll lane of <paramref name="length"/> u.</summary>
        public static int WheelSpin(Vector2 pos, Vector2 dir, float length = -1, float halfWidth = -1, float tell = -1)
            => R.Add(MinionKind.WheelSpin, pos, dir, length > 0 ? length : WheelRollMax, halfWidth > 0 ? halfWidth : WheelHalfWidth, tell >= 0 ? tell : WheelWind, 0);
        /// <summary>Rolling fire trail: add the wheel position each frame; <see cref="WheelTrailEnd"/> plays the stop squash and the trail burns out.</summary>
        public static int WheelTrailBegin(float radius = -1, float life = -1) => R.Add(MinionKind.WheelTrail, Vector2.zero, Vector2.right, 0, radius > 0 ? radius : FireTrailRadius, 0.2f, life > 0 ? life : FireTrailLife);
        public static void WheelTrailAdd(int h, Vector2 pos) => R.TrailAdd(h, pos);
        public static void WheelTrailEnd(int h) => R.TrailEnd(h);
        public static MinionPhase GetPhase(int h) => MinionRunner.Exists ? R.Phase(h) : MinionPhase.None;
        public static void Cancel(int h) { if (MinionRunner.Exists) R.Cancel(h); }
        public static void Clear() { if (MinionRunner.Exists) R.ClearAll(); }
        public static int ActiveCount => MinionRunner.Exists ? R.Count : 0;
        /// <summary>Smallest rect side emitted so far (QC: must stay at least 3).</summary>
        public static int MinSidePx => MinionRunner.Exists ? R.MinSide : 999;
    }

    public class MinionRunner : MonoBehaviour, ICraftSink
    {
        static MinionRunner _i;
        public static bool Exists => _i != null;
        public static MinionRunner I { get { if (_i == null) { var go = new GameObject("MinionFX"); DontDestroyOnLoad(go); _i = go.AddComponent<MinionRunner>(); } return _i; } }

        const int MaxPts = 160;
        struct It { public MinionKind kind; public MinionPhase ph; public Vector2 a, d, hit; public float len, r, tell, life, pt, age, tHit; public int seed, n; public bool ended, sp, hasHit; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; Vector2[] _pts = new Vector2[0]; float[] _ptT = new float[0];
        public int Count { get; private set; }
        public int MinSide { get; private set; } = 999;

        const int MaxQ = 6000;
        struct Q { public int x, y, w, h; public Color32 c; }
        readonly Q[] _gs = new Q[MaxQ], _gb = new Q[MaxQ], _gf = new Q[MaxQ], _tb = new Q[MaxQ], _tf = new Q[MaxQ]; int _ngs, _ngb, _ngf, _ntb, _ntf;
        Mesh _mg, _mt; MeshRenderer _rg, _rt; Vector3[] _vg, _vt; Color32[] _cg, _ct; int _lastG, _lastT;
        /* Endesga 32 */
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255),
            Vi = new Color32(0xb5, 0x50, 0x88, 255), Vd = new Color32(0x3a, 0x44, 0x66, 255),
            Ru = new Color32(0xbe, 0x4a, 0x2f, 255), Rk = new Color32(0x73, 0x3e, 0x39, 255), Pe = new Color32(0xe8, 0xb7, 0x96, 255);
        static float Hs(int a, int b) => Craft.Hs(a, b);
        static int Rn(float v) => Mathf.RoundToInt(v);


        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); var np = new Vector2[cap * MaxPts]; var nt = new float[cap * MaxPts];
                Array.Copy(_pts, np, _pts.Length); Array.Copy(_ptT, nt, _ptT.Length); _pts = np; _ptT = nt;
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == MinionKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mg != null) return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Minion White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = MinionFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "MinionFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] MinionFX: no sprite shader found; set MinionFX.MaterialOverride.");
            _mg = Make("MinionFX Ground", m, MaxQ * 3, out _rg, out _vg, out _cg); _mt = Make("MinionFX Top", m, MaxQ * 2, out _rt, out _vt, out _ct); ApplySorting();
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
            if (_rg == null) return; _rg.sortingOrder = MinionFX.GroundSortingOrder; _rt.sortingOrder = MinionFX.TopSortingOrder;
            _rg.sortingLayerName = MinionFX.SortingLayer; _rt.sortingLayerName = MinionFX.SortingLayer;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != MinionKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = MinionKind.None; _it[i].ph = MinionPhase.None; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, MinionPhase p) { k.ph = p; k.pt = 0; k.sp = false; }

        public int Add(MinionKind kind, Vector2 a, Vector2 d, float lenU, float rU, float tell, float life)
        {
            if (_mg == null) Prewarm(64); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++;
            ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.a = a * ppu; k.d = d.sqrMagnitude > 1e-6f ? d.normalized : Vector2.right; k.len = lenU * ppu; k.r = Mathf.Max(5f, rU * ppu); k.tell = Mathf.Max(0.2f, tell);
            k.life = life; k.age = 0; k.n = 0; k.ended = false; k.hasHit = false; k.seed = (i * 7919 + k.gen * 131) & 0xFFFF;
            Go(ref k, kind == MinionKind.FrostBolt ? MinionPhase.Fly : kind == MinionKind.WheelTrail ? MinionPhase.Trail : MinionPhase.Tell);
            return Hd(i);
        }
        public void Move(int h, Vector2 p) { if (!Get(h, out int i) || _it[i].ph != MinionPhase.Fly) return; p *= HellFX.PPU; Vector2 v = p - _it[i].a; if (v.sqrMagnitude > 0.01f) _it[i].d = v.normalized; _it[i].a = p; }
        public void Hit(int h, Vector2 p) { if (Get(h, out int i) && _it[i].ph == MinionPhase.Fly) { _it[i].a = p * HellFX.PPU; Go(ref _it[i], MinionPhase.Hit); } }
        public void Fizzle(int h) { if (Get(h, out int i) && _it[i].ph == MinionPhase.Fly) Go(ref _it[i], MinionPhase.Fizzle); }
        public void TrailAdd(int h, Vector2 p)
        {
            if (!Get(h, out int i) || _it[i].ended) return; ref var k = ref _it[i]; p *= HellFX.PPU; int o = i * MaxPts;
            if (k.n > 0) { Vector2 v = p - _pts[o + k.n - 1]; if (v.sqrMagnitude < k.r * k.r * 0.36f) return; k.d = v.normalized; }
            if (k.n >= MaxPts) { Array.Copy(_pts, o + 1, _pts, o, MaxPts - 1); Array.Copy(_ptT, o + 1, _ptT, o, MaxPts - 1); k.n = MaxPts - 1; }
            _pts[o + k.n] = p; _ptT[o + k.n] = k.age; k.n++;
        }
        public void TrailEnd(int h) { if (!Get(h, out int i) || _it[i].ended) return; ref var k = ref _it[i]; k.ended = true; k.sp = false; if (k.n > 0) { k.hasHit = true; k.hit = _pts[i * MaxPts + k.n - 1]; k.tHit = k.age; } }
        public MinionPhase Phase(int h) => Get(h, out int i) ? _it[i].ph : MinionPhase.None;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { _deb.Clear(); _nDec = 0; for (int i = 0; i < _it.Length; i++) if (_it[i].kind != MinionKind.None) Free(i); }

        /* ---------- emit: every coloured rect gets a dark outline rect in the backing pass ---------- */
        void Box(bool top, int x, int y, int w, int h, Color32 c)
        {
            if (w <= 0 || h <= 0) return; if (w < 3) { x -= (3 - w) / 2; w = 3; } if (h < 3) h = 3;
            if (top) { if (_ntf >= MaxQ) return; _tf[_ntf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _tb[_ntb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            else { if (_ngf >= MaxQ) return; _gf[_ngf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _gb[_ngb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            int m = w < h ? w : h; if (m < MinSide) MinSide = m;
        }
        void Shadow(int x, int y, int w, int h, byte a) { if (w > 0 && h > 0 && _ngs < MaxQ) _gs[_ngs++] = new Q { x = x, y = y, w = w, h = h, c = new Color32(0x18, 0x14, 0x25, a) }; }
        /* ---------- PixelCraft sink ---------- */
        void ICraftSink.CBox(bool top, int x, int y, int w, int h, Color32 c) => Box(top, x, y, w, h, c);
        void ICraftSink.CShadow(int x, int y, int w, int h, byte a) => Shadow(x, y, w, h, a);
        readonly CraftDebris _deb = new CraftDebris(384);
        const int MaxDec = 32; struct Dec { public Vector2 c; public float r, t, life; public int seed; }
        readonly Dec[] _dec = new Dec[MaxDec]; int _nDec;
        void Cell(bool top, Vector2 q, Color32 c) => Box(top, Rn(q.x) - 1, Rn(q.y) - 1, 3, 3, c);
        /* enemy lane: ONE wobbly 3 px border (white in the last 15 %), faint shadow fill, grows over the first 60 %, 3 px pop on the first frames */
        void Lane(Vector2 o, Vector2 d, float len, float hw, float p, float t, Color32 col, int seed)
        {
            if (t < 0.05f) hw += 3; len *= Mathf.Min(1f, p / 0.6f); Vector2 n = new Vector2(-d.y, d.x); Color32 c = p >= 0.85f ? Wh : col;
            int ns = Mathf.FloorToInt(len / 6f);
            for (int s = 0; s <= ns; s++) { Vector2 b = o + d * (s * 6); Cell(false, b - n * (hw + (Hs(seed, s * 2) < 0.22f ? 3 : 0)), c); Cell(false, b + n * (hw + (Hs(seed, s * 2 + 1) < 0.22f ? 3 : 0)), c); }
            byte a = (byte)(30 + 40 * p); int m = Mathf.FloorToInt(2 * hw / 3f);
            for (int s = 0; s < Mathf.FloorToInt(len / 3f); s++) { Vector2 b = o + d * (s * 3) - n * hw; for (int j = 0; j < m; j++) { Vector2 q = b + n * (j * 3); Shadow(Rn(q.x) - 1, Rn(q.y) - 1, 3, 3, a); } }
            Vector2 e = o + d * (len + 3); for (int j = -1; j <= 1; j++) Cell(false, e + n * (j * 3), c);
        }

        void Update()
        {
            if (_mg == null) return; float dt = MinionFX.Paused ? 0f : Time.deltaTime; _ngs = _ngb = _ngf = _ntb = _ntf = 0;
            for (int i = 0; i < _it.Length; i++) { if (_it[i].kind == MinionKind.None) continue; ref var k = ref _it[i]; k.age += dt; k.pt += dt; if (!Step(ref k, i, dt)) Free(i); }
            int w = 0; for (int j = 0; j < _nDec; j++) { _dec[j].t += dt; Craft.DecalFrost(this, _dec[j].c, _dec[j].r, _dec[j].t, _dec[j].life, _dec[j].seed, Vd, Vi); if (_dec[j].t < _dec[j].life) _dec[w++] = _dec[j]; } _nDec = w;
            _deb.Step(dt); _deb.Draw(this);
            Flush(_mg, _vg, _cg, ref _lastG, true); Flush(_mt, _vt, _ct, ref _lastT, false);
        }

        bool Step(ref It k, int i, float dt)
        {
            float p = Mathf.Clamp01(k.pt / k.tell);
            switch (k.kind)
            {
                case MinionKind.FrostCharge:   /* 179 */
                {
                    Lane(k.a, k.d, k.len, k.r + 3, p, k.pt, Vi, k.seed);
                    float g = Craft.EaseBack(Mathf.Min(1f, p / 0.75f)), sq = p > 0.88f ? 0.75f : 1f;
                    Craft.Blob(this, true, k.a, 3 + 6 * g, Vd, Vi, Wh, Wh, 3, k.seed + Mathf.FloorToInt(k.age * 10) / 3, 0.4f, 1f / sq, sq);
                    if (p > 0.55f)
                    {
                        int nL = 1 + Mathf.FloorToInt((p - 0.55f) * 7); float rot = 0.4f + k.age * 1.5f;
                        for (int q = 0; q < 4; q++) { float an = rot + q * Mathf.PI * 0.5f; Vector2 u = new Vector2(Mathf.Cos(an), Mathf.Sin(an)); for (int m = 0; m < nL; m++) Cell(true, k.a + u * (9 + m * 3), m == nL - 1 ? Wh : Vi); }
                    }
                    for (int j = 0; j < 3; j++) { float an = k.age * (5 + 8 * p) + j * Craft.Two / 3f, rr = 18 * (1 - p) + 6; Cell(true, k.a + new Vector2(Mathf.Cos(an) * rr, Mathf.Sin(an) * rr * 0.7f), j == 0 && p > 0.85f ? Wh : Vi); }
                    if (k.pt >= k.tell) { MinionFX.OnStrike?.Invoke(Hd(i), MinionKind.FrostCharge); return false; }
                    return true;
                }
                case MinionKind.FrostBolt:     /* 180 */
                {
                    if (k.ph == MinionPhase.Fly)
                    {
                        Vector2 d = k.d, n = new Vector2(-d.y, d.x), P = k.a; int fl = Mathf.FloorToInt(k.age * 15) % 2; float s = fl == 1 ? -3 : 3;
                        Craft.ShadowBlob(this, P + new Vector2(0, -9), 5, 90, k.seed, 0.1f, 0.5f);
                        Craft.Blob(this, true, P - d * 2, 5, Vd, Vi, Wh, Wh, 3, k.seed + fl, 0.35f, 1.25f, 0.9f);
                        Cell(true, P + d * 6, Wh); Cell(true, P + d * 9, Wh); Cell(true, P + d * 3 - n * s, Vi); Cell(true, P - d * 6 - n * s, Vi);
                        Cell(true, P - d * 9, Vi); Cell(true, P - d * 12 + n * s, Vd); Cell(true, P - d * 15, Vd);
                        if (Mathf.FloorToInt(k.age * 30) % 3 == 0) _deb.Spawn(CraftDebris.Snow, P - d * 9, 1, 25, 20, Vi, Vd, Vi, Vd, k.seed + Mathf.FloorToInt(k.age * 90), 0, 0.45f);
                        return true;
                    }
                    if (k.ph == MinionPhase.Hit)
                    {
                        if (!k.sp)
                        {
                            k.sp = true; _deb.Spawn(CraftDebris.Shard, k.a, 4, 70, 90, Wh, Vi, Vd, Vd, k.seed, 0.015f, 0.8f);
                            _deb.Spawn(CraftDebris.Chip, k.a, 4, 55, 60, Vi, Vd, Wh, Vi, k.seed + 3, 0.02f, 0.6f);
                            if (_nDec < MaxDec) _dec[_nDec++] = new Dec { c = k.a, r = 10, life = 1f, seed = k.seed };
                        }
                        Craft.Impact(this, k.a, 9, k.pt, k.seed, Wh, Vd);
                        if (k.pt >= 0.067f && k.pt < 0.2f) { float e = 12 * Craft.EaseBack((k.pt - 0.067f) / 0.133f); Craft.RoughBand(this, false, k.a, 6 + e, 9 + e, 0, Craft.Two, Vi, k.seed, 2f, 0.3f); }
                        return k.pt < 0.3f;
                    }
                    if (!k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Snow, k.a, 4, 40, 30, Vi, Vd, Vi, Vd, k.seed, 0.02f, 0.5f); }
                    return k.pt < 0.25f;
                }
                case MinionKind.WheelSpin:     /* 181 */
                {
                    Lane(k.a, k.d, k.len, k.r, p, k.pt, Pe, k.seed);
                    float om = 4 + 22 * p * p, a0 = k.age * om; Vector2 hub = k.a + new Vector2(0, 6);
                    Craft.RoughBand(this, true, hub, 9, 12, a0, 0.9f + 0.5f * p, Pe, k.seed, 0f, 0f, 1f, 0, 16, 0.6f);
                    Craft.RoughBand(this, true, hub, 9, 12, a0 + Mathf.PI, 0.9f + 0.5f * p, Ru, k.seed + 1, 0f, 0f, 1f, 0, 16, 0.6f);
                    float iv = 0.14f - 0.1f * p;
                    if (k.age % iv < dt) _deb.Spawn(CraftDebris.Chip, k.a + new Vector2(0, 3), 1 + Mathf.FloorToInt(p * 2), 50 + 90 * p, 50 + 60 * p, Pe, Ru, Rk, Pe, k.seed + Mathf.FloorToInt(k.age * 97), 0, 0.4f);
                    if (p > 0.8f && !k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Chip, k.a, 5, 100, 80, Pe, Ru, Rk, Pe, k.seed + 5, 0.01f, 0.45f); }
                    if (k.pt >= k.tell) { MinionFX.OnStrike?.Invoke(Hd(i), MinionKind.WheelSpin); return false; }
                    return true;
                }
                case MinionKind.WheelTrail:    /* 182 */
                {
                    int o = i * MaxPts, live = 0; float life = k.life, sc = 0.5f;
                    for (int j = 0; j < k.n; j++)
                    {
                        float left = life - (k.age - _ptT[o + j]); if (left <= -sc) continue; live++;
                        if (left <= 0) Craft.ShadowBlob(this, _pts[o + j], k.r - 2, 90, k.seed + j, 0.3f, 0.7f, 1 + left / sc); else Craft.ShadowBlob(this, _pts[o + j], k.r, 90, k.seed + j, 0.3f, 0.7f);
                    }
                    for (int j = 0; j < k.n; j++)
                    {
                        float left = life - (k.age - _ptT[o + j]); if (left <= 0) continue;
                        float rr = k.r - (left < 0.45f ? 3 * Mathf.Floor((0.45f - left) / 0.15f) : 0); if (rr < 4) continue;
                        Craft.Blob(this, false, _pts[o + j], rr, Rk, Ru, Pe, Pe, 3, k.seed + j * 7, 0.3f, 1f, 0.75f, left >= 0.3f ? 1f : left / 0.3f, k.seed + j);
                    }
                    for (int j = 0; j < k.n; j += 3)
                    {
                        float left = life - (k.age - _ptT[o + j]); if (left <= 0.35f) continue; Vector2 c = _pts[o + j];
                        Craft.Tongue(this, true, c.x + (Hs(k.seed, j) - 0.5f) * k.r, c.y - 2, 6 + 6 * Mathf.Min(1f, left), 6, Ru, Pe, k.seed + j * 13, k.age, -0.4f * Mathf.Sign(k.d.x));
                    }
                    if (!k.ended && k.n > 0 && Mathf.FloorToInt(k.age * 30) % 3 == 0) _deb.Spawn(CraftDebris.Chip, _pts[o + k.n - 1], 1, 45, 40, Pe, Ru, Pe, Ru, k.seed + Mathf.FloorToInt(k.age * 91), 0, 0.35f);
                    if (k.hasHit)
                    {
                        float ht = k.age - k.tHit; if (ht < 0.07f) Craft.Impact(this, k.hit, 7, ht, k.seed, Wh, Rk);
                        if (!k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Chip, k.hit, 5, 70, 70, Pe, Ru, Rk, Pe, k.seed + 9, 0.015f, 0.5f); _deb.Spawn(CraftDebris.Ember, k.hit, 3, 20, 15, Rk, Rk, Rk, Rk, k.seed + 11, 0.03f, 0.7f); }
                    }
                    return !k.ended || live > 0;
                }
            }
            return false;
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
