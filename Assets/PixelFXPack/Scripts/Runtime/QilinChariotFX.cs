using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="QilinChariotFX"/> item.</summary>
    public enum QilinKind { None = 0, ChargeTell = 1, Trail = 2, WheelTell = 3, Wheel = 4, Breath = 5, Ring = 6, Meteor = 7, Embers = 8 }
    /// <summary>Phase of a <see cref="QilinChariotFX"/> item.</summary>
    public enum QilinPhase { None = 0, Delay = 1, Tell = 2, Active = 3, Collapse = 4, Outro = 5 }

    /// <summary>
    /// 174-178. Qilin Chariot boss FX for Hellord v0.0.44 (v2.10.0). World space, 16 PPU, sizes in world units like HellordTuning (the q* fields).
    /// HAZARD fire = crimson family only: red #e43b44, pink #f6757a, dark crimson #a22633, white only as the one-step "now" cue / impact flash, #181425 outline.
    /// It never uses the hero's gold / amber / orange (#fee761 #feae34 #f77622: Fireball, Hellfire Pillar), so the boss fire and the hero fire differ in hue (red-pink vs gold-orange).
    /// NON-HAZARD fire (chariot embers, meteor smoke, wheel-end poof, impact debris, scorch marks) = muted purple / slate / dark only (#68386c #5a6988 #3e2731), never red, never white,
    /// never a border or ring shape, so it cannot read as a tell. The drift ring's SAFE GAP = no fire at all + two pale steel posts (#c0cbdc / #8b9bb4) + outward chevrons; no hero colour
    /// (#63c74d is Bone Orbit, so it is not used). Every hazard has a wind-up and a bright 6 px border (3 px red outside, 3 px pink inside, white in the last 15 %, one step, no blinking).
    /// 174 CHARGE lane tell + burning trail (<see cref="ChargeTell"/>, <see cref="TrailBegin"/>), 175 BURNING WHEELS lanes + wheel + bounce (<see cref="WheelTell"/>, <see cref="Wheel"/>),
    /// 176 FLAME BREATH sweep (<see cref="Breath"/>), 177 DRIFT RING closing with a safe gap (<see cref="DriftRing"/>), 178 METEOR tell circle + fall + impact (<see cref="Meteor"/>).
    /// Plus <see cref="Embers"/> (cosmetic, non-hazard). Visual only. Two meshes, one material (2 draw calls), fixed arrays, int handles (-1 when full; every call accepts -1).
    /// </summary>
    public static class QilinChariotFX
    {
        static QilinRunner R => QilinRunner.I;
        /// <summary>Defaults = HellordTuning v0.0.44 values.</summary>
        public static float ChargeTellS = 0.85f, ChargeHalfWidth = 1.0f, TrailRadius = 0.675f, TrailLife = 2.4f, WheelTellS = 0.7f, WheelSpreadDeg = 25f, WheelLaneLength = 5f, WheelRadius = 0.6f;
        public static float BreathTellS = 0.8f, BreathS = 1.4f, BreathReach = 7f, BreathHalfDeg = 14f, BreathSweepDeg = 90f;
        public static float RingRadius = 6f, RingTellS = 0.6f, RingDriveS = 1.6f, RingCollapseS = 1.4f, RingGapDeg = 50f, RingWidth = 0.6f, RingEndRadius = 0.6f, MeteorFallS = 0.9f, MeteorRadius = 1.2f;
        public static int Wheels = 2;
        /// <summary>Look lengths (s / px).</summary>
        public static float WaveSeconds = 0.3f, MeteorDropPx = 170f, ScorchSeconds = 0.6f, BreathFlowPxPerS = 120f;
        public static int GroundSortingOrder = -20000, TopSortingOrder = 70; public static string SortingLayer = "Default"; public static Material MaterialOverride;
        public static bool Paused;
        /// <summary>Hellord v0.0.46: when true the breath tell and the meteor ground tell (shadow + border) are not drawn; the fire and the falling meteor still are.</summary>
        public static bool HideTell;
        /// <summary>(handle, kind) when a tell ends and the hazard becomes live (charge, wheel tell, breath, ring collapse, meteor impact).</summary>
        public static Action<int, QilinKind> OnStrike;

        public static void Prewarm(int items = 64) => R.Prewarm(items);
        public static void SetGroundSorting(int order) { GroundSortingOrder = order; if (QilinRunner.Exists) R.ApplySorting(); }
        /// <summary>Charge lane tell from the boss feet along <paramref name="dir"/> to the wall (<paramref name="length"/> u). Grows out in the first 60 %, ends at the tell.</summary>
        public static int ChargeTell(Vector2 from, Vector2 dir, float length, float halfWidth = -1, float tell = -1)
            => R.Add(QilinKind.ChargeTell, from, dir, length, halfWidth > 0 ? halfWidth : ChargeHalfWidth, tell >= 0 ? tell : ChargeTellS, 0, 0);
        /// <summary>Burning trail (charge or wheel). Add the position each frame; each blob burns <paramref name="life"/> s, shrinks, dissolves, then leaves a dark scorch.</summary>
        public static int TrailBegin(float radius = -1, float life = -1) => R.Add(QilinKind.Trail, Vector2.zero, Vector2.right, 0, radius > 0 ? radius : TrailRadius, 1f, life > 0 ? life : TrailLife, 0);
        public static void TrailAdd(int h, Vector2 pos) => R.PtAdd(h, pos);
        public static void TrailEnd(int h) => R.End(h);
        /// <summary>Wheel launch lanes: <paramref name="n"/> lanes fanned +/- <paramref name="spreadDeg"/> around <paramref name="baseDeg"/>.</summary>
        public static int WheelTell(Vector2 from, float baseDeg, int n = -1, float spreadDeg = -1, float length = -1, float radius = -1, float tell = -1)
        {
            int h = R.Add(QilinKind.WheelTell, from, new Vector2(Mathf.Cos(baseDeg * Mathf.Deg2Rad), Mathf.Sin(baseDeg * Mathf.Deg2Rad)), length > 0 ? length : WheelLaneLength, radius > 0 ? radius : WheelRadius,
                tell >= 0 ? tell : WheelTellS, spreadDeg >= 0 ? spreadDeg : WheelSpreadDeg, n > 0 ? n : Wheels); return h;
        }
        /// <summary>One burning wheel (body only; pair it with a <see cref="TrailBegin"/> if the wheel leaves fire). Move it each frame; call WheelBounce on a wall hit.</summary>
        public static int Wheel(Vector2 pos, float radius = -1) => R.Add(QilinKind.Wheel, pos, Vector2.right, 0, radius > 0 ? radius : WheelRadius, 1f, 0, 0);
        public static void WheelMove(int h, Vector2 pos) => R.Move(h, pos);
        public static void WheelBounce(int h, Vector2 pos) => R.Bounce(h, pos);
        public static void WheelEnd(int h) => R.End(h);
        /// <summary>Flame breath: tell shows the whole sweep sector + start beam + sweep arrow, then the beam sweeps from centre - sweep/2 to centre + sweep/2.</summary>
        public static int Breath(Vector2 mouth, float centerDeg, float sweepDeg = -1, float halfDeg = -1, float reach = -1, float tell = -1, float dur = -1)
        {
            int h = R.Add(QilinKind.Breath, mouth, new Vector2(centerDeg, sweepDeg >= 0 ? sweepDeg : BreathSweepDeg), reach > 0 ? reach : BreathReach, halfDeg > 0 ? halfDeg : BreathHalfDeg,
                tell >= 0 ? tell : BreathTellS, dur > 0 ? dur : BreathS, 0); return h;
        }
        /// <summary>Drift ring around <paramref name="center"/>. The fire never covers the gap (<paramref name="gapCenterDeg"/> +/- gapDeg/2): pass the same angle the game uses for its safe check.
        /// Tell (boss drives to the start) -> drive (fire grows from one gap edge round to the other) -> collapse (radius shrinks to <see cref="RingEndRadius"/>, live hazard).</summary>
        public static int DriftRing(Vector2 center, float gapCenterDeg, float radius = -1, float gapDeg = -1, float width = -1, float tell = -1, float drive = -1, float collapse = -1)
            => R.AddRing(center, gapCenterDeg, radius > 0 ? radius : RingRadius, gapDeg > 0 ? gapDeg : RingGapDeg, width > 0 ? width : RingWidth, tell >= 0 ? tell : RingTellS, drive > 0 ? drive : RingDriveS, collapse > 0 ? collapse : RingCollapseS);
        /// <summary>Move the ring centre (if the game re-centres it).</summary>
        public static void RingMove(int h, Vector2 center) => R.Move(h, center);
        /// <summary>One meteor: optional delay, tell circle + growing shadow while the meteor falls, impact wave, scorch.</summary>
        public static int Meteor(Vector2 spot, float delay = 0, float radius = -1, float fall = -1)
            => R.Add(QilinKind.Meteor, spot, Vector2.zero, 0, radius > 0 ? radius : MeteorRadius, fall >= 0 ? fall : MeteorFallS, Mathf.Max(0, delay), 0);
        /// <summary>Cosmetic, non-hazard ember puff (muted purple / slate, rises and dissolves in 0.5 s).</summary>
        public static int Embers(Vector2 pos, int n = 5) => R.Add(QilinKind.Embers, pos, Vector2.zero, 0, 0.5f, 0.5f, 0, Mathf.Clamp(n, 1, 12));
        public static QilinPhase GetPhase(int h) => QilinRunner.Exists ? R.Phase(h) : QilinPhase.None;
        public static void Cancel(int h) { if (QilinRunner.Exists) R.Cancel(h); }
        public static void Clear() { if (QilinRunner.Exists) R.ClearAll(); }
        public static int ActiveCount => QilinRunner.Exists ? R.Count : 0;
        /// <summary>Smallest rect side emitted so far (QC: must stay at least 3).</summary>
        public static int MinSidePx => QilinRunner.Exists ? R.MinSide : 999;
    }

    public class QilinRunner : MonoBehaviour, ICraftSink
    {
        static QilinRunner _i;
        public static bool Exists => _i != null;
        public static QilinRunner I { get { if (_i == null) { var go = new GameObject("QilinChariotFX"); DontDestroyOnLoad(go); _i = go.AddComponent<QilinRunner>(); } return _i; } }

        const int MaxPts = 160;
        struct It { public QilinKind kind; public QilinPhase ph; public Vector2 a, d, b; public float len, r, tell, x1, x2, x3, pt, age, travel, bt; public int seed, n, cnt; public bool ended, sp; public float dvx; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; Vector2[] _pts = new Vector2[0]; float[] _ptT = new float[0];
        public int Count { get; private set; }
        public int MinSide { get; private set; } = 999;

        const int MaxQ = 8000;
        struct Q { public int x, y, w, h; public Color32 c; }
        readonly Q[] _gs = new Q[MaxQ], _gb = new Q[MaxQ], _gf = new Q[MaxQ], _tb = new Q[MaxQ], _tf = new Q[MaxQ]; int _ngs, _ngb, _ngf, _ntb, _ntf;
        Mesh _mg, _mt; MeshRenderer _rg, _rt; Vector3[] _vg, _vt; Color32[] _cg, _ct; int _lastG, _lastT;
        /* Endesga 32. Hazard: Rd Pk Cr (+Wh now-cue). Non-hazard: Pu Sl Dk. Safe gap: Ic St. */
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255), Rd = new Color32(0xe4, 0x3b, 0x44, 255), Pk = new Color32(0xf6, 0x75, 0x7a, 255),
            Cr = new Color32(0xa2, 0x26, 0x33, 255), Pu = new Color32(0x68, 0x38, 0x6c, 255), Sl = new Color32(0x5a, 0x69, 0x88, 255), Dk = new Color32(0x3e, 0x27, 0x31, 255),
            Ic = new Color32(0xc0, 0xcb, 0xdc, 255), St = new Color32(0x8b, 0x9b, 0xb4, 255);
        static Color32 Sh(float a) => new Color32(0x18, 0x14, 0x25, (byte)Mathf.Clamp(a, 0, 255));

        static float Hs(int a, int b) { unchecked { uint h = (uint)(a * 374761393 + b * 668265263); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; } }

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); var np = new Vector2[cap * MaxPts]; var nt = new float[cap * MaxPts];
                Array.Copy(_pts, np, _pts.Length); Array.Copy(_ptT, nt, _ptT.Length); _pts = np; _ptT = nt;
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == QilinKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mg != null) return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Qilin White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = QilinChariotFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "QilinChariotFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] QilinChariotFX: no sprite shader found; set QilinChariotFX.MaterialOverride.");
            _mg = Make("QilinChariotFX Ground", m, MaxQ * 3, out _rg, out _vg, out _cg); _mt = Make("QilinChariotFX Top", m, MaxQ * 2, out _rt, out _vt, out _ct); ApplySorting();
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
            if (_rg == null) return; _rg.sortingOrder = QilinChariotFX.GroundSortingOrder; _rt.sortingOrder = QilinChariotFX.TopSortingOrder;
            _rg.sortingLayerName = QilinChariotFX.SortingLayer; _rt.sortingLayerName = QilinChariotFX.SortingLayer;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != QilinKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = QilinKind.None; _it[i].ph = QilinPhase.None; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, QilinPhase p) { k.ph = p; k.pt = 0; k.sp = false; }

        public int Add(QilinKind kind, Vector2 a, Vector2 d, float lenU, float rU, float tell, float x1, float x2)
        {
            if (_mg == null) Prewarm(64); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++;
            ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.a = a * ppu; k.b = k.a; k.d = d; k.len = lenU * ppu; k.r = Mathf.Max(6f, rU * ppu); k.tell = Mathf.Max(0.2f, tell); k.x1 = x1; k.x2 = x2; k.x3 = 0;
            k.age = 0; k.n = 0; k.cnt = 0; k.travel = 0; k.bt = 99f; k.ended = false; k.seed = (i * 7919 + k.gen * 131) & 0xFFFF;
            if (kind == QilinKind.ChargeTell || kind == QilinKind.WheelTell) { if (k.d.sqrMagnitude < 1e-6f) k.d = Vector2.right; k.d.Normalize(); }
            if (kind == QilinKind.WheelTell) k.cnt = Mathf.Clamp((int)x2, 1, 8);
            if (kind == QilinKind.Embers) { k.cnt = (int)x2; k.r = 6; }
            if (kind == QilinKind.Breath) k.x3 = rU;
            if (kind == QilinKind.Trail || kind == QilinKind.Wheel || kind == QilinKind.Embers) Go(ref k, QilinPhase.Active);
            else if (kind == QilinKind.Meteor && x1 > 0) Go(ref k, QilinPhase.Delay); else Go(ref k, QilinPhase.Tell);
            return Hd(i);
        }
        public int AddRing(Vector2 c, float gapDeg0, float rU, float gap, float wU, float tell, float drive, float collapse)
        {
            int h = Add(QilinKind.Ring, c, new Vector2(gapDeg0, Mathf.Clamp(gap, 10f, 180f)), rU, wU, tell, drive, collapse);
            if (Get(h, out int i)) { _it[i].r = Mathf.Max(6f, wU * HellFX.PPU); _it[i].x3 = QilinChariotFX.RingEndRadius * HellFX.PPU; } return h;
        }
        public void PtAdd(int h, Vector2 p)
        {
            if (!Get(h, out int i) || _it[i].ended) return; ref var k = ref _it[i]; p *= HellFX.PPU; int o = i * MaxPts;
            if (k.n > 0 && (_pts[o + k.n - 1] - p).sqrMagnitude < k.r * k.r * 0.36f) return;
            if (k.n >= MaxPts) { Array.Copy(_pts, o + 1, _pts, o, MaxPts - 1); Array.Copy(_ptT, o + 1, _ptT, o, MaxPts - 1); k.n = MaxPts - 1; }
            _pts[o + k.n] = p; _ptT[o + k.n] = k.age; k.n++;
        }
        public void Move(int h, Vector2 p) { if (!Get(h, out int i)) return; p *= HellFX.PPU; _it[i].travel += (p - _it[i].a).magnitude; _it[i].dvx = p.x - _it[i].a.x; _it[i].a = p; }
        public void Bounce(int h, Vector2 p) { if (!Get(h, out int i)) return; _it[i].b = p * HellFX.PPU; _it[i].bt = 0; }
        public void End(int h) { if (!Get(h, out int i)) return; ref var k = ref _it[i]; if (k.kind == QilinKind.Wheel && !k.ended) { k.ended = true; Go(ref k, QilinPhase.Outro); } else k.ended = true; }
        public QilinPhase Phase(int h) => Get(h, out int i) ? _it[i].ph : QilinPhase.None;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { _deb.Clear(); _nDec = 0; for (int i = 0; i < _it.Length; i++) if (_it[i].kind != QilinKind.None) Free(i); }

        /* ---------- emit: coloured rects get a dark outline rect in the backing pass; alpha colours are dark ground shadows ---------- */
        void Box(bool top, int x, int y, int w, int h, Color32 c)
        {
            if (w <= 0 || h <= 0) return;
            if (c.a < 255) { if (_ngs < MaxQ) _gs[_ngs++] = new Q { x = x, y = y, w = w, h = h, c = c }; return; }
            if (w < 3) { x -= (3 - w) / 2; w = 3; } if (h < 3) h = 3;
            if (top) { if (_ntf >= MaxQ) return; _tf[_ntf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _tb[_ntb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            else { if (_ngf >= MaxQ) return; _gf[_ngf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _gb[_ngb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            int m = w < h ? w : h; if (m < MinSide) MinSide = m;
        }
        /* Grid scan (3 px cells snapped to world px, runs merged per row).
           polar: p = distance from c in [p0,p1), q = angle (rad) measured CCW from q0 within span q1 (q1 >= 2pi = full).
           lane:  p = along d in [p0,p1), q = across (signed, or |across| when absQ) in [q0,q1). keep < 1 dissolves cells by a stable hash. */
        void Scan(bool top, bool polar, Vector2 c, Vector2 d, float p0, float p1, float q0, float q1, bool absQ, Color32 col, float keep, int seed)
        {
            if (p1 <= p0) return; float x0, x1, y0, y1;
            if (polar) { x0 = c.x - p1; x1 = c.x + p1; y0 = c.y - p1; y1 = c.y + p1; }
            else
            {
                Vector2 n = new Vector2(-d.y, d.x); float qa = absQ ? -q1 : q0, qb = q1;
                Vector2 A = c + d * p0 + n * qa, B = c + d * p1 + n * qa, C = c + d * p0 + n * qb, D = c + d * p1 + n * qb;
                x0 = Mathf.Min(Mathf.Min(A.x, B.x), Mathf.Min(C.x, D.x)); x1 = Mathf.Max(Mathf.Max(A.x, B.x), Mathf.Max(C.x, D.x));
                y0 = Mathf.Min(Mathf.Min(A.y, B.y), Mathf.Min(C.y, D.y)); y1 = Mathf.Max(Mathf.Max(A.y, B.y), Mathf.Max(C.y, D.y));
            }
            int gy0 = Mathf.FloorToInt(y0 / 3f) * 3, gy1 = Mathf.CeilToInt(y1 / 3f) * 3, gx0 = Mathf.FloorToInt(x0 / 3f) * 3, gx1 = Mathf.CeilToInt(x1 / 3f) * 3;
            const float TwoPi = Mathf.PI * 2f; bool full = q1 >= TwoPi - 1e-4f; float p0s = p0 * p0, p1s = p1 * p1;
            for (int y = gy0; y < gy1; y += 3)
            {
                int run = int.MinValue; float cy = y + 1.5f - c.y;
                for (int x = gx0; x <= gx1; x += 3)
                {
                    bool inn = false;
                    if (x < gx1)
                    {
                        float cx = x + 1.5f - c.x;
                        if (polar) { float r2 = cx * cx + cy * cy; inn = r2 >= p0s && r2 < p1s && (full || Mathf.Repeat(Mathf.Atan2(cy, cx) - q0, TwoPi) <= q1); }
                        else { float al = cx * d.x + cy * d.y, ac = -cx * d.y + cy * d.x; if (absQ) ac = Mathf.Abs(ac); inn = al >= p0 && al < p1 && ac >= q0 && ac < q1; }
                        if (inn && keep < 1f && Hs(seed + x, y) >= keep) inn = false;
                    }
                    if (inn) { if (run == int.MinValue) run = x; }
                    else if (run != int.MinValue) { Box(top, run, y, x - run, 3, col); run = int.MinValue; }
                }
            }
        }
        void Disc(bool top, Vector2 c, float r, Color32 col, float keep = 1f, int seed = 0) => Scan(top, true, c, Vector2.right, 0, r, 0, 7f, false, col, keep, seed);
        void Band(bool top, Vector2 c, float r0, float r1, float a0, float span, Color32 col, float keep = 1f, int seed = 0) => Scan(top, true, c, Vector2.right, Mathf.Max(0, r0), r1, a0, span, false, col, keep, seed);
        /* hazard border round a disc: pop-in +6,+3,+1,0 px, 3 px red outside, 3 px pink inside (white once p >= 0.85) */
        void Border(Vector2 c, float r, float p, float t)
        {
            r += t < 0.033f ? 6 : t < 0.067f ? 3 : t < 0.1f ? 1 : 0;
            Band(false, c, r - 3, r, 0, 7f, Rd); Band(false, c, r - 6, r - 3, 0, 7f, p >= 0.85f ? Wh : Pk);
        }
        /* lane tell: dark fill, 3+3 px border on both sides and the far end, chevrons every 24 px; grows out over the first 60 % */
        void LaneTell(Vector2 o, Vector2 d, float len, float hw, float p, float t, int seed)
        {
            float L = len * Mathf.Min(1f, p / 0.6f); if (L < 6) L = 6; Color32 inner = p >= 0.85f ? Wh : Pk;
            Scan(false, false, o, d, 0, L, 0, hw - 6, true, Sh(50 + 60 * p), 1f, 0);
            Scan(false, false, o, d, 0, L, hw - 3, hw, true, Rd, 1f, 0); Scan(false, false, o, d, 0, L - 3, hw - 6, hw - 3, true, inner, 1f, 0);
            Scan(false, false, o, d, L - 3, L, 0, hw - 3, true, Rd, 1f, 0); Scan(false, false, o, d, L - 6, L - 3, 0, hw - 6, true, inner, 1f, 0);
            Vector2 n = new Vector2(-d.y, d.x);
            for (float s = 18; s < L - 12; s += 24) for (int j = -2; j <= 2; j++) { Vector2 q = o + d * (s - Mathf.Abs(j) * 3) + n * (j * 3); Box(false, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Pk); }
        }
        /* ---------- v2.10.1 hand-made layer (PixelCraft): flame tongues, broken waves, shaded rock, debris with gravity, textured scorch ---------- */
        void ICraftSink.CBox(bool top, int x, int y, int w, int h, Color32 c) => Box(top, x, y, w, h, c);
        void ICraftSink.CShadow(int x, int y, int w, int h, byte a) => Box(false, x, y, w, h, Sh(a));
        /* wheels are drawn this much bigger than the hit radius they are given (visual only; see HELLORD_FX_MAP) */
        public const float WheelVisualScale = 1.35f;
        readonly CraftDebris _deb = new CraftDebris(512);
        const int MaxDec = 48; struct Dec { public Vector2 c; public float r, t, life; public int seed; }
        readonly Dec[] _dec = new Dec[MaxDec]; int _nDec;
        void Ember(Vector2 c, int n, int seed, float delay = 0f, float life = 0.7f) => _deb.Spawn(CraftDebris.Ember, c, n, 30, 20, Pu, Sl, Dk, Pu, seed, delay, life);
        void Flame(float x, float y, float h, float w, int seed, float t, float lean = 0f) => Craft.Tongue(this, true, x, y, h, w, Rd, Pk, seed, t, lean);
        /* impact wave: overshoot ease, pink lead (white for 2 frames) + dithered red trail, breaks into chunks as it fades */
        void Wave(Vector2 c, float r0, float r1, float u, int seed, bool white)
        {
            if (u < 0 || u >= 1) return; float r = Mathf.Round((r0 + (r1 - r0) * Craft.EaseBack(u)) / 3f) * 3f, br = u < 0.35f ? 0f : (u - 0.35f) / 0.65f * 0.7f;
            Craft.RoughBand(this, false, c, r - 3, r, 0, Craft.Two, white ? Wh : Pk, seed, 3f, br);
            if (r > 12) Craft.RoughBand(this, false, c, r - 9, r - 3, 0, Craft.Two, Rd, seed, 3f, br, u < 0.5f ? 0.8f : 0.5f, seed + 9);
        }
        /* NON-HAZARD bits: muted purple / slate / dark 3x3, rise in 3 px steps, dissolve */
        void Puff(Vector2 c, int n, float t, float life, int seed, float spread)
        {
            for (int j = 0; j < n; j++)
            {
                float u = t / life; if (u >= 1 || Hs(seed, j + 300) < (u - 0.5f) / 0.5f) continue;
                float a = Hs(seed, j) * Mathf.PI * 2f, rr = spread * (0.3f + 0.7f * Hs(seed, j + 7)) + 30f * t;
                float x = c.x + Mathf.Cos(a) * rr, y = c.y + Mathf.Sin(a) * rr * 0.5f + Mathf.Floor(40f * t / 3f) * 3f + 6;
                Box(true, Mathf.RoundToInt(x) - 1, Mathf.RoundToInt(y) - 1, 3, 3, j % 3 == 0 ? Pu : j % 3 == 1 ? Sl : Dk);
            }
        }
        void Star(Vector2 c) { int x = Mathf.RoundToInt(c.x), y = Mathf.RoundToInt(c.y); Box(true, x - 1, y - 4, 3, 9, Wh); Box(true, x - 4, y - 1, 9, 3, Wh); }

        void Update()
        {
            if (_mg == null) return; float dt = QilinChariotFX.Paused ? 0f : Time.deltaTime; _ngs = _ngb = _ngf = _ntb = _ntf = 0;
            for (int i = 0; i < _it.Length; i++) { if (_it[i].kind == QilinKind.None) continue; ref var k = ref _it[i]; k.age += dt; k.pt += dt; k.bt += dt; if (!Step(ref k, i)) Free(i); }
            int w = 0; for (int j = 0; j < _nDec; j++) { _dec[j].t += dt; Craft.DecalScorch(this, _dec[j].c, _dec[j].r, _dec[j].t, _dec[j].life, _dec[j].seed, Pu, Dk); if (_dec[j].t < _dec[j].life) _dec[w++] = _dec[j]; } _nDec = w;
            _deb.Step(dt); _deb.Draw(this);
            Flush(_mg, _vg, _cg, ref _lastG, true); Flush(_mt, _vt, _ct, ref _lastT, false);
        }
        void Strike(ref It k, int i, QilinPhase next) { Go(ref k, next); QilinChariotFX.OnStrike?.Invoke(Hd(i), k.kind); }

        bool Step(ref It k, int i)
        {
            float p = Mathf.Clamp01(k.pt / k.tell);
            switch (k.kind)
            {
                case QilinKind.ChargeTell:
                    LaneTell(k.a, k.d, k.len, k.r, p, k.pt, k.seed);
                    if (k.pt >= k.tell) { QilinChariotFX.OnStrike?.Invoke(Hd(i), k.kind); return false; }
                    return true;
                case QilinKind.WheelTell:
                {
                    float b = Mathf.Atan2(k.d.y, k.d.x), sp = k.x1 * Mathf.Deg2Rad;
                    for (int w = 0; w < k.cnt; w++) { float a = b + (k.cnt == 1 ? 0 : Mathf.Lerp(-sp, sp, w / (float)(k.cnt - 1))); LaneTell(k.a, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), k.len, k.r + 3, p, k.pt, k.seed + w); }
                    if (k.pt >= k.tell) { QilinChariotFX.OnStrike?.Invoke(Hd(i), k.kind); return false; }
                    return true;
                }
                case QilinKind.Trail:
                {
                    int o = i * MaxPts, live = 0; float life = k.x1, sc = QilinChariotFX.ScorchSeconds;
                    for (int j = 0; j < k.n; j++)
                    {
                        float left = life - (k.age - _ptT[o + j]); if (left <= -sc) continue; live++;
                        if (left <= 0) { Craft.ShadowBlob(this, _pts[o + j], k.r - 3, 110, k.seed + j, 0.3f, 0.8f, 1f + left / sc); continue; }
                        Craft.ShadowBlob(this, _pts[o + j], k.r, 110, k.seed + j, 0.3f, 0.8f);
                    }
                    for (int pass = 0; pass < 2; pass++)
                        for (int j = 0; j < k.n; j++)
                        {
                            float left = life - (k.age - _ptT[o + j]); if (left <= 0) continue;
                            float rr = k.r; if (left < 0.6f) rr -= 3 * Mathf.Floor((0.6f - left) / 0.15f); if (rr < 6) continue;
                            float keep = left >= 0.3f ? 1f : left / 0.3f; Vector2 c = _pts[o + j];
                            if (pass == 0)
                            {
                                Craft.Blob(this, false, c, rr, Pk, Pk, Pk, Pk, 0, k.seed + j * 7, 0.3f, 1f, 0.8f, keep, k.seed + j);
                                Craft.Blob(this, false, c, rr - 3, Cr, Rd, Pk, Pk, 3, k.seed + j * 7, 0.3f, 1f, 0.8f, keep, k.seed + j);
                            }
                            else if (j % 2 == 0 && left > 0.3f) Flame(c.x + (Hs(k.seed, j + 2) - 0.5f) * rr, c.y - 3, 9 + 9 * Mathf.Min(1f, left / 1.2f), j % 3 == 0 ? 9 : 6, k.seed + j * 13, k.age);
                        }
                    if (!k.ended && k.n > 0 && Mathf.FloorToInt(k.age * 30) % 3 == 0) Ember(_pts[o + k.n - 1], 1, k.seed + Mathf.FloorToInt(k.age * 90));
                    return !k.ended || live > 0;
                }
                case QilinKind.Wheel:
                {
                    Vector2 c = k.a; float r = k.r * WheelVisualScale;
                    if (k.ph == QilinPhase.Outro)
                    {
                        if (!k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Chip, c, 8, 90, 90, Pk, Rd, Dk, Pk, k.seed, 0.01f, 0.8f); Ember(c, 6, k.seed + 1, 0.02f, 0.8f); }
                        Craft.Impact(this, c, r, k.pt, k.seed, Wh, Dk); return k.pt < 0.4f;
                    }
                    float lean = k.dvx > 0.5f ? -0.5f : k.dvx < -0.5f ? 0.5f : 0f;
                    float sx = k.bt < 0.05f ? 1.3f : k.bt < 0.1f ? 0.85f : 1f, sy = k.bt < 0.05f ? 0.7f : k.bt < 0.1f ? 1.15f : 1f; /* squash on the bounce, then overshoot */
                    Craft.ShadowBlob(this, c + new Vector2(0, -r * 0.7f), r, 100, k.seed, 0.1f, 0.5f);
                    for (int f = -1; f <= 1; f++) Flame(c.x + f * r * 0.45f, c.y + r * 0.35f, r * (f == 0 ? 1f : 0.7f), f == 0 ? 9 : 6, k.seed + f * 7, k.age, lean);
                    Craft.Blob(this, true, c, r, Cr, Rd, Pk, Pk, 7, k.seed, 0.08f, sx, sy);
                    Craft.RoughBand(this, true, c, r * 0.55f, r * 0.55f + 3, 0, Craft.Two, Cr, k.seed + 2, 0f, 0f, 1f, 0, 24, sy);
                    float ang = Mathf.Floor(k.travel / Mathf.Max(3f, r) / (Mathf.PI / 4f)) * (Mathf.PI / 4f);
                    for (int s = 0; s < 4; s++) { float a = -ang + s * Mathf.PI * 0.5f;
                        for (int m = 1; m <= 2; m++) Box(true, Mathf.RoundToInt(c.x + Mathf.Cos(a) * m * r * 0.3f * sx) - 1, Mathf.RoundToInt(c.y + Mathf.Sin(a) * m * r * 0.3f * sy) - 1, 3, 3, Cr); }
                    Box(true, Mathf.RoundToInt(c.x) - 3, Mathf.RoundToInt(c.y) - 3, 6, 6, Pk);
                    if (k.bt < 0.034f && !k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Chip, k.b, 6, 100, 110, Pk, Rd, Dk, Pk, k.seed + Mathf.RoundToInt(k.b.x), 0.01f, 0.6f); }
                    if (k.bt >= 0.034f) k.sp = false;
                    Craft.Impact(this, k.b, 8, k.bt, k.seed, Wh, Dk);
                    if (Mathf.FloorToInt(k.age * 30) % 3 == 0) Ember(c, 1, k.seed + Mathf.FloorToInt(k.age * 90), 0f, 0.6f);
                    return true;
                }
                case QilinKind.Breath:
                {
                    float c0 = k.d.x * Mathf.Deg2Rad, sw = k.d.y * Mathf.Deg2Rad, hf = k.x3 * Mathf.Deg2Rad;
                    float A0 = c0 - sw * 0.5f - hf, span = sw + 2 * hf, R = k.len;
                    Vector2 o = k.a;
                    if (k.ph == QilinPhase.Tell && QilinChariotFX.HideTell)
                    {
                        if (k.pt >= k.tell) Strike(ref k, i, QilinPhase.Active);
                        return true;
                    }
                    if (k.ph == QilinPhase.Tell)
                    {
                        float pop = k.pt < 0.033f ? 6 : k.pt < 0.067f ? 3 : k.pt < 0.1f ? 1 : 0, RR = R + pop; Color32 inner = p >= 0.85f ? Wh : Pk;
                        Band(false, o, 6, RR * Mathf.Min(1f, p / 0.6f), A0, span, Sh(45 + 55 * p));
                        if (p > 0.4f) Band(false, o, 9, RR - 6, A0, 2 * hf, Cr);
                        Band(false, o, RR - 3, RR, A0, span, Rd); Band(false, o, RR - 6, RR - 3, A0, span, inner);
                        Vector2 e0 = new Vector2(Mathf.Cos(A0), Mathf.Sin(A0)), e1 = new Vector2(Mathf.Cos(A0 + span), Mathf.Sin(A0 + span));
                        Scan(false, false, o, e0, 6, RR, 0, 3, false, Rd, 1f, 0); Scan(false, false, o, e0, 6, RR - 3, 3, 6, false, inner, 1f, 0);
                        Scan(false, false, o, e1, 6, RR, -3, 0, false, Rd, 1f, 0); Scan(false, false, o, e1, 6, RR - 3, -6, -3, false, inner, 1f, 0);
                        float am = A0 + hf * 2f + 0.12f; Vector2 tip = o + new Vector2(Mathf.Cos(am), Mathf.Sin(am)) * (RR + 9), tg = new Vector2(-Mathf.Sin(am), Mathf.Cos(am)), nr = new Vector2(Mathf.Cos(am), Mathf.Sin(am));
                        for (int j = 0; j < 4; j++) { Vector2 q = tip - tg * (j * 3); Box(false, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Pk); }
                        for (int j = 1; j <= 2; j++) for (int s = -1; s <= 1; s += 2) { Vector2 q = tip - tg * (j * 3) + nr * (s * j * 3); Box(false, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Pk); }
                        if (k.pt >= k.tell) Strike(ref k, i, QilinPhase.Active);
                        return true;
                    }
                    if (k.ph == QilinPhase.Active)
                    {
                        float u = Mathf.Clamp01(k.pt / Mathf.Max(0.1f, k.x1)), cur = A0 + hf + sw * u, rest0 = cur + hf, restSpan = A0 + span - rest0;
                        if (restSpan > 0.02f) { Band(false, o, R - 3, R, rest0, restSpan, Rd); Vector2 e1 = new Vector2(Mathf.Cos(A0 + span), Mathf.Sin(A0 + span)); Scan(false, false, o, e1, 6, R, -3, 0, false, Rd, 1f, 0); }
                        float flow = Mathf.Repeat(k.age * QilinChariotFX.BreathFlowPxPerS, 18f); flow = Mathf.Floor(flow / 3f) * 3f;
                        Craft.RoughBand(this, false, o, 6, R, cur - hf, 2 * hf, Pk, k.seed + Mathf.FloorToInt(k.age * 10), 3f);
                        for (float rs = 6 + flow - 18; rs < R - 3; rs += 18) Band(false, o, Mathf.Max(9, rs), Mathf.Min(R - 3, rs + 9), cur - hf + 0.05f, 2 * hf - 0.1f, Rd);
                        Band(false, o, 6, R * 0.8f, cur - hf * 0.45f, hf * 0.9f, Pk);
                        Band(false, o, 6, Mathf.Max(12, R * 0.25f), cur - hf * 0.45f, hf * 0.9f, Wh);
                        for (int j = 0; j < 5; j++) { float a = cur - hf * 0.8f + hf * 1.6f * j / 4f, rr = R * (0.55f + 0.4f * Hs(k.seed, j)); Flame(o.x + Mathf.Cos(a) * rr, o.y + Mathf.Sin(a) * rr, 12 + 6 * (j % 2), 9, k.seed + j * 11, k.age, 0.3f * Mathf.Cos(a)); }
                        if (Mathf.FloorToInt(k.age * 30) % 2 == 0) _deb.Spawn(CraftDebris.Ember, o + new Vector2(Mathf.Cos(cur), Mathf.Sin(cur)) * R * 0.9f, 1, 40, 20, Pu, Sl, Dk, Pu, k.seed + Mathf.FloorToInt(k.age * 90), 0, 0.6f);
                        if (k.pt >= k.x1) Go(ref k, QilinPhase.Outro);
                        return true;
                    }
                    { float u = k.pt / 0.2f, cur = A0 + hf + sw; Band(false, o, 6, R, cur - hf, 2 * hf, Rd, 1f - u, k.seed); Puff(o + new Vector2(Mathf.Cos(cur), Mathf.Sin(cur)) * R * 0.6f, 6, k.pt, 0.4f, k.seed, 12); return k.pt < 0.4f; }
                }
                case QilinKind.Ring:
                {
                    float gC = k.d.x * Mathf.Deg2Rad, gap = k.d.y * Mathf.Deg2Rad, two = Mathf.PI * 2f, a0 = gC + gap * 0.5f, full = two - gap, w = k.r;
                    float R = k.len; if (k.ph == QilinPhase.Collapse) R = Mathf.Round(Mathf.Lerp(k.len, k.x3, Mathf.Clamp01(k.pt / k.x2)) / 3f) * 3f; else if (k.ph == QilinPhase.Outro) R = k.x3;
                    Vector2 c = k.a;
                    if (k.ph == QilinPhase.Outro)
                    {
                        Craft.Impact(this, c, w * 1.5f, k.pt, k.seed, Wh, Dk);
                        if (!k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Chip, c, 10, 110, 100, Pk, Rd, Dk, Pk, k.seed, 0.01f, 0.9f); _deb.Spawn(CraftDebris.Ember, c, 8, 50, 30, Pu, Sl, Dk, Pu, k.seed + 1, 0.02f, 1f); }
                        Wave(c, 6, w * 3, k.pt / QilinChariotFX.WaveSeconds, k.seed, k.pt < 0.067f); return k.pt < 0.5f;
                    }
                    bool tell = k.ph == QilinPhase.Tell; float q = tell ? 0f : k.ph == QilinPhase.Active ? Mathf.Clamp01(k.pt / k.x1) : 1f;
                    float pt = tell ? k.pt : 1f, pAll = tell ? p * 0.5f : k.ph == QilinPhase.Active ? 0.5f + 0.5f * q : 1f;
                    if (k.ph != QilinPhase.Collapse)
                    {
                        float pop = pt < 0.033f ? 6 : pt < 0.067f ? 3 : pt < 0.1f ? 1 : 0; Color32 inner = k.ph == QilinPhase.Active && q >= 0.85f ? Wh : Pk;
                        Band(false, c, R - w, R + w, a0, full, Sh(40 + 60 * pAll));
                        Band(false, c, R + w + pop - 3, R + w + pop, a0, full, Rd); Band(false, c, R + w + pop - 6, R + w + pop - 3, a0, full, inner);
                        Band(false, c, R - w - pop, R - w - pop + 3, a0, full, Rd); Band(false, c, R - w - pop + 3, R - w - pop + 6, a0, full, inner);
                    }
                    float burn = k.ph == QilinPhase.Active ? full * q : k.ph == QilinPhase.Collapse ? full : 0f;
                    if (burn > 0.01f)
                    {
                        float wr = Mathf.Max(6, w);
                        Craft.RoughBand(this, false, c, R - wr, R + wr, a0, burn, Pk, k.seed, 2f); Craft.RoughBand(this, false, c, R - wr + 3, R + wr - 3, a0, burn, Rd, k.seed, 2f);
                        int nT = Mathf.Max(1, (int)(burn * R / 24f));
                        for (int j = 0; j < nT; j++) { float a = a0 + burn * (j + 0.5f) / nT; Flame(c.x + Mathf.Cos(a) * R, c.y + Mathf.Sin(a) * R, 12 + 9 * Hs(k.seed, j), 9, k.seed + j * 19, k.age); }
                        float seg = 0.35f, fl = Mathf.Repeat(k.age * 1.2f, seg);
                        for (float s = fl - seg; s < burn; s += seg) { float s0 = Mathf.Max(0, s), s1 = Mathf.Min(burn, s + seg * 0.5f); if (s1 > s0) Band(false, c, R - 3, R + 3, a0 + s0, s1 - s0, Cr); }
                    }
                    for (int sgn = -1; sgn <= 1; sgn += 2)
                    {
                        float ga = gC + sgn * (gap * 0.5f + 0.02f); Vector2 dr = new Vector2(Mathf.Cos(ga), Mathf.Sin(ga));
                        Scan(false, false, c, dr, R - w - 9, R + w + 9, -3, 3, false, Ic, 1f, 0); Scan(false, false, c, dr, R - w - 9, R + w + 9, 3, 6, false, St, 1f, 0);
                    }
                    Vector2 gd = new Vector2(Mathf.Cos(gC), Mathf.Sin(gC)), gt = new Vector2(-gd.y, gd.x);
                    for (int cv = 0; cv < 2; cv++) { float rr = R - 3 + cv * 9; for (int j = -2; j <= 2; j++) { Vector2 qq = c + gd * (rr - Mathf.Abs(j) * 3) + gt * (j * 3); Box(false, Mathf.RoundToInt(qq.x) - 1, Mathf.RoundToInt(qq.y) - 1, 3, 3, cv == 0 ? St : Ic); } }
                    if (k.ph == QilinPhase.Tell && k.pt >= k.tell) Go(ref k, QilinPhase.Active);
                    else if (k.ph == QilinPhase.Active && k.pt >= k.x1) Strike(ref k, i, QilinPhase.Collapse);
                    else if (k.ph == QilinPhase.Collapse && k.pt >= k.x2) Go(ref k, QilinPhase.Outro);
                    return true;
                }
                case QilinKind.Meteor:
                {
                    if (k.ph == QilinPhase.Delay) { if (k.pt >= k.x1) Go(ref k, QilinPhase.Tell); return true; }
                    if (k.ph == QilinPhase.Tell)
                    {
                        float sr = Mathf.Max(3, Mathf.Floor(k.r * (0.3f + 0.7f * p) / 3f) * 3f); if (!QilinChariotFX.HideTell) { Craft.ShadowBlob(this, k.a, sr, (byte)Mathf.Clamp(70 + 80 * p, 0, 255), k.seed, 0.2f, 1f); Border(k.a, k.r, p, k.pt); }
                        if (p > 0.15f)
                        {
                            float hgt = QilinChariotFX.MeteorDropPx * (1 - p * p); Vector2 off = new Vector2(0.55f, 1f) * hgt, mp = k.a + off;
                            for (int s = 1; s <= 4; s++) { Vector2 sp = mp + new Vector2(0.55f, 1f).normalized * (s * 9); int sz = s < 3 ? 6 : 3; Box(true, Mathf.RoundToInt(sp.x) - sz / 2, Mathf.RoundToInt(sp.y) - sz / 2, sz, sz, s % 2 == 0 ? Sl : Pu); }
                            Flame(mp.x + 3, mp.y, 18, 12, k.seed, k.age, 0.55f); Flame(mp.x - 3, mp.y + 3, 12, 6, k.seed + 5, k.age, 0.55f);
                            Craft.Blob(this, true, mp, 9, Dk, Pu, Rd, Pk, 7, k.seed + Mathf.FloorToInt(k.age / 0.1f) * 7, 0.3f);
                        }
                        if (k.pt >= k.tell) Strike(ref k, i, QilinPhase.Outro);
                        return true;
                    }
                    if (!k.sp)
                    {
                        k.sp = true; _deb.Spawn(CraftDebris.Chip, k.a, 10, 110, 110, Dk, Pu, Sl, Pk, k.seed, 0.012f, 0.9f, 2); Ember(k.a, 6, k.seed + 3, 0.03f, 1f);
                        if (_nDec < MaxDec) _dec[_nDec++] = new Dec { c = k.a, r = k.r * 0.7f, life = 1.4f, seed = k.seed };
                    }
                    Craft.Impact(this, k.a, 12, k.pt, k.seed, Wh, Dk);
                    if (k.pt >= 0.067f && k.pt < 0.17f) Craft.Blob(this, true, k.a + new Vector2(0, -3), 10, Dk, Pu, Rd, Pk, 7, k.seed, 0.3f, 1.4f, 0.55f);
                    Wave(k.a, k.r * 0.3f, k.r + 6, k.pt / QilinChariotFX.WaveSeconds, k.seed, false);
                    if (k.pt < 0.3f) for (int j = 0; j < 3; j++) { float h = (15 * (1 - k.pt / 0.3f) + 6) * (j % 2 == 0 ? 0.7f + 0.5f * Hs(k.seed, j) : 0.6f); Flame(k.a.x + (j - 1) * k.r * 0.4f, k.a.y, h, 6, k.seed + j * 17, k.age); }
                    return k.pt < 0.5f;
                }
                case QilinKind.Embers:
                    if (!k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Ember, k.a, k.cnt, 40, 30, Pu, Sl, Dk, Pu, k.seed, 0.02f, 0.8f); }
                    return k.pt < 0.5f;
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
