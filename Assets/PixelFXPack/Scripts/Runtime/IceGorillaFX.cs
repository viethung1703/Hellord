using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of an <see cref="IceGorillaFX"/> item.</summary>
    public enum IceGorillaKind { None = 0, Slam = 1, Icicle = 2, Boulder = 3, Leap = 4, Slide = 5, Roar = 6 }
    /// <summary>Phase of an <see cref="IceGorillaFX"/> item.</summary>
    public enum IceGorillaPhase { None = 0, Delay = 1, Tell = 2, Strike = 3, Stay = 4, Shatter = 5, Trail = 6 }

    /// <summary>
    /// 167-173. Ice Gorilla boss FX for Hellord v0.0.44 (v2.9.0). World space, 16 PPU, sizes in world units like HellordTuning (the g* fields).
    /// Family colours: deep blue #0099db / #124e89, pale ice #c0cbdc / #8b9bb4, white highlights. The hero's cyan #2ce8f5 (Frost Cleave) is never used.
    /// Every hazard has a wind-up and a bright 6 px border (3 px blue outside, 3 px pale ice inside, dark #181425 outline); the inner border turns white
    /// once in the last 15 % of the tell (one step, no blinking). Tells, shadows, waves and the ice trail are drawn on the GROUND mesh (under characters,
    /// <see cref="GroundSortingOrder"/>); falling icicles, the boulder, chunks and shards on the TOP mesh. Every rect is at least 3 px, opaque, with a dark outline
    /// drawn first; only the dark shadows use alpha. Dissolves use a stable hash. Nothing blinks or pulses.
    /// 167 GLACIER SLAM ring (<see cref="Slam"/>), 168 ICICLE fall + growing shadow (<see cref="Icicle"/>), 169 ICICLE land + stay + shatter (same handle),
    /// 170 BOULDER throw + 7-shard fan (<see cref="Boulder"/>, <see cref="ShardHit"/>), 171 LEAP landing ring (<see cref="Leap"/>, <see cref="LeapTrack"/>),
    /// 172 BELLY-SLIDE ice trail (<see cref="SlideBegin"/>, <see cref="SlideAdd"/>, <see cref="SlideEnd"/>), 173 ROAR wave (<see cref="Roar"/>).
    /// Visual only: the game keeps damage, Chill and collisions. Two meshes, one material (2 draw calls), fixed arrays, int handles (-1 when full; every call accepts -1).
    /// </summary>
    public static class IceGorillaFX
    {
        static IceGorillaRunner R => IceGorillaRunner.I;
        /// <summary>Defaults = HellordTuning v0.0.44 values.</summary>
        public static float SlamTell = 0.9f, SlamRadius = 3.2f, IcicleTell = 0.8f, IcicleRadius = 0.7f, IcicleStay = 1.5f, BoulderTell = 0.8f, BoulderFlight = 0.8f, BoulderRadius = 1.6f;
        public static float ShardFanDeg = 50f, ShardSpeed = 9f, ShardLife = 2f, LeapAir = 1.1f, LeapLock = 0.4f, LeapRadius = 2.6f, SlideHalfWidth = 1.1f, SlideTrailLife = 3.5f, RoarTell = 0.8f, RoarRadius = 5f;
        public static int Shards = 7;
        /// <summary>Strike / outro lengths (s).</summary>
        public static float WaveSeconds = 0.3f, RoarWaveSeconds = 0.36f, ShatterSeconds = 0.35f, IcicleFallPx = 150f, BoulderArcPx = 48f, BoulderHandPx = 56f;
        public static int GroundSortingOrder = -20000, TopSortingOrder = 70; public static string SortingLayer = "Default"; public static Material MaterialOverride;
        public static bool Paused;
        /// <summary>Hellord v0.0.46: when true the icicle and boulder ground tells (shadow + border) are not drawn; the falling icicle and the boulder still are.</summary>
        public static bool HideTell;
        /// <summary>(handle, kind) when a tell ends and the hit lands (slam, icicle, boulder, leap, roar).</summary>
        public static Action<int, IceGorillaKind> OnStrike;

        public static void Prewarm(int items = 64) => R.Prewarm(items);
        public static void SetGroundSorting(int order) { GroundSortingOrder = order; if (IceGorillaRunner.Exists) R.ApplySorting(); }
        /// <summary>Glacier Slam on the boss feet: tell ring + clock front, then the shockwave ring.</summary>
        public static int Slam(Vector2 feet, float radius = -1, float tell = -1) => R.Add(IceGorillaKind.Slam, feet, feet, radius > 0 ? radius : SlamRadius, tell >= 0 ? tell : SlamTell, 0, 0);
        /// <summary>One icicle: optional stagger delay, growing shadow tell while it falls, landing, stays <paramref name="stay"/> s, then shatters.</summary>
        public static int Icicle(Vector2 spot, float delay = 0, float radius = -1, float tell = -1, float stay = -1)
            => R.Add(IceGorillaKind.Icicle, spot, spot, radius > 0 ? radius : IcicleRadius, tell >= 0 ? tell : IcicleTell, Mathf.Max(0, delay), stay >= 0 ? stay : IcicleStay);
        /// <summary>Shatter a standing icicle early (e.g. hit by the hero).</summary>
        public static void IcicleShatter(int h) => R.Shatter(h);
        /// <summary>Boulder: wind-up at the hand, flight along an arc, landing ring, 7 shards fanned away from <paramref name="bossFeet"/>.</summary>
        public static int Boulder(Vector2 bossFeet, Vector2 spot, float radius = -1, float tell = -1, float flight = -1)
            => R.Add(IceGorillaKind.Boulder, bossFeet, spot, radius > 0 ? radius : BoulderRadius, tell >= 0 ? tell : BoulderTell, flight >= 0 ? flight : BoulderFlight, 0);
        /// <summary>Stop drawing shard <paramref name="index"/> (0..Shards-1) of a boulder, e.g. when it hits the hero or a wall.</summary>
        public static void ShardHit(int h, int index) => R.ShardHit(h, index);
        /// <summary>Leap Smash shadow: call when the boss leaves the ground. It follows <see cref="LeapTrack"/> until the last <paramref name="lockS"/> s, then lands.</summary>
        public static int Leap(Vector2 target, float radius = -1, float air = -1, float lockS = -1)
            => R.Add(IceGorillaKind.Leap, target, target, radius > 0 ? radius : LeapRadius, air >= 0 ? air : LeapAir, lockS >= 0 ? lockS : LeapLock, 0);
        public static void LeapTrack(int h, Vector2 target) => R.Track(h, target);
        /// <summary>Belly-slide ice trail. Add the boss position each frame while it slides; SlideEnd stops adding and the trail melts on its own.</summary>
        public static int SlideBegin(float halfWidth = -1, float life = -1) => R.Add(IceGorillaKind.Slide, Vector2.zero, Vector2.zero, halfWidth > 0 ? halfWidth : SlideHalfWidth, 0, life > 0 ? life : SlideTrailLife, 0);
        public static void SlideAdd(int h, Vector2 pos) => R.SlideAdd(h, pos);
        public static void SlideEnd(int h) => R.SlideEnd(h);
        /// <summary>Chest-beat Roar on the boss: tell ring closing on the boss, then three rings out to <paramref name="radius"/>.</summary>
        public static int Roar(Vector2 feet, float radius = -1, float tell = -1) => R.Add(IceGorillaKind.Roar, feet, feet, radius > 0 ? radius : RoarRadius, tell >= 0 ? tell : RoarTell, 0, 0);
        public static IceGorillaPhase GetPhase(int h) => IceGorillaRunner.Exists ? R.Phase(h) : IceGorillaPhase.None;
        public static void Cancel(int h) { if (IceGorillaRunner.Exists) R.Cancel(h); }
        public static void Clear() { if (IceGorillaRunner.Exists) R.ClearAll(); }
        public static int ActiveCount => IceGorillaRunner.Exists ? R.Count : 0;
        /// <summary>Smallest rect side emitted so far (QC: must stay at least 3).</summary>
        public static int MinSidePx => IceGorillaRunner.Exists ? R.MinSide : 999;
    }

    public class IceGorillaRunner : MonoBehaviour, ICraftSink
    {
        static IceGorillaRunner _i;
        public static bool Exists => _i != null;
        public static IceGorillaRunner I { get { if (_i == null) { var go = new GameObject("IceGorillaFX"); DontDestroyOnLoad(go); _i = go.AddComponent<IceGorillaRunner>(); } return _i; } }

        const int MaxPts = 160;
        struct It { public IceGorillaKind kind; public IceGorillaPhase ph; public Vector2 a, b; public float r, tell, x1, x2, pt, age, tStrike; public int shardMask, seed, n; public bool ended, sp; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; Vector2[] _pts = new Vector2[0]; float[] _ptT = new float[0];
        public int Count { get; private set; }
        public int MinSide { get; private set; } = 999;

        const int MaxQ = 6000;
        struct Q { public int x, y, w, h; public Color32 c; }
        readonly Q[] _gs = new Q[MaxQ], _gb = new Q[MaxQ], _gf = new Q[MaxQ], _tb = new Q[MaxQ], _tf = new Q[MaxQ]; int _ngs, _ngb, _ngf, _ntb, _ntf;
        Mesh _mg, _mt; MeshRenderer _rg, _rt; Vector3[] _vg, _vt; Color32[] _cg, _ct; int _lastG, _lastT;
        /* Endesga 32 */
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255), Ice = new Color32(0xc0, 0xcb, 0xdc, 255), Ice2 = new Color32(0x8b, 0x9b, 0xb4, 255),
            Bl = new Color32(0x00, 0x99, 0xdb, 255), Bd = new Color32(0x12, 0x4e, 0x89, 255), Sl = new Color32(0x5a, 0x69, 0x88, 255);

        static float Hs(int a, int b) { unchecked { uint h = (uint)(a * 374761393 + b * 668265263); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; } }

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); var np = new Vector2[cap * MaxPts]; var nt = new float[cap * MaxPts];
                Array.Copy(_pts, np, _pts.Length); Array.Copy(_ptT, nt, _ptT.Length); _pts = np; _ptT = nt;
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == IceGorillaKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mg != null) return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX IceGorilla White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = IceGorillaFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "IceGorillaFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] IceGorillaFX: no sprite shader found; set IceGorillaFX.MaterialOverride.");
            _mg = Make("IceGorillaFX Ground", m, MaxQ * 3, out _rg, out _vg, out _cg); _mt = Make("IceGorillaFX Top", m, MaxQ * 2, out _rt, out _vt, out _ct); ApplySorting();
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
            if (_rg == null) return; _rg.sortingOrder = IceGorillaFX.GroundSortingOrder; _rt.sortingOrder = IceGorillaFX.TopSortingOrder;
            _rg.sortingLayerName = IceGorillaFX.SortingLayer; _rt.sortingLayerName = IceGorillaFX.SortingLayer;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != IceGorillaKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = IceGorillaKind.None; _it[i].ph = IceGorillaPhase.None; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, IceGorillaPhase p) { k.ph = p; k.pt = 0; k.sp = false; }

        public int Add(IceGorillaKind kind, Vector2 a, Vector2 b, float rU, float tell, float x1, float x2)
        {
            if (_mg == null) Prewarm(64); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++;
            ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.a = a * ppu; k.b = b * ppu; k.r = Mathf.Max(6f, rU * ppu); k.tell = Mathf.Max(0.2f, tell); k.x1 = x1; k.x2 = x2; k.age = 0; k.n = 0; k.ended = false;
            k.shardMask = 0; k.seed = (i * 7919 + k.gen * 131) & 0xFFFF; k.tStrike = 0;
            if (kind == IceGorillaKind.Slide) Go(ref k, IceGorillaPhase.Trail);
            else if (kind == IceGorillaKind.Icicle && x1 > 0) Go(ref k, IceGorillaPhase.Delay); else Go(ref k, IceGorillaPhase.Tell);
            return Hd(i);
        }
        public void Track(int h, Vector2 p) { if (Get(h, out int i) && _it[i].ph == IceGorillaPhase.Tell && _it[i].tell - _it[i].pt > _it[i].x1) _it[i].a = _it[i].b = p * HellFX.PPU; }
        public void Shatter(int h) { if (Get(h, out int i) && _it[i].ph == IceGorillaPhase.Stay) Go(ref _it[i], IceGorillaPhase.Shatter); }
        public void ShardHit(int h, int s) { if (Get(h, out int i) && s >= 0 && s < 31) _it[i].shardMask |= 1 << s; }
        public void SlideAdd(int h, Vector2 p)
        {
            if (!Get(h, out int i) || _it[i].ended) return; ref var k = ref _it[i]; p *= HellFX.PPU; int o = i * MaxPts;
            if (k.n > 0 && (_pts[o + k.n - 1] - p).sqrMagnitude < k.r * k.r * 0.25f) return;
            if (k.n >= MaxPts) { Array.Copy(_pts, o + 1, _pts, o, MaxPts - 1); Array.Copy(_ptT, o + 1, _ptT, o, MaxPts - 1); k.n = MaxPts - 1; }
            _pts[o + k.n] = p; _ptT[o + k.n] = k.age; k.n++;
        }
        public void SlideEnd(int h) { if (Get(h, out int i)) _it[i].ended = true; }
        public IceGorillaPhase Phase(int h) => Get(h, out int i) ? _it[i].ph : IceGorillaPhase.None;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { _deb.Clear(); _nDec = 0; for (int i = 0; i < _it.Length; i++) if (_it[i].kind != IceGorillaKind.None) Free(i); }

        /* ---------- emit: every coloured rect gets a dark outline rect in the backing pass ---------- */
        void Box(bool top, int x, int y, int w, int h, Color32 c)
        {
            if (w <= 0 || h <= 0) return; if (w < 3) { x -= (3 - w) / 2; w = 3; } if (h < 3) h = 3;
            if (top) { if (_ntf >= MaxQ) return; _tf[_ntf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _tb[_ntb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            else { if (_ngf >= MaxQ) return; _gf[_ngf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _gb[_ngb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            int m = w < h ? w : h; if (m < MinSide) MinSide = m;
        }
        void Shadow(int x, int y, int w, int h, byte a) { if (w > 0 && h > 0 && _ngs < MaxQ) _gs[_ngs++] = new Q { x = x, y = y, w = w, h = h, c = new Color32(0x18, 0x14, 0x25, a) }; }
        /* ---------- v2.10.1 hand-made layer (PixelCraft): wobbly borders, broken waves, shaded rock, angular shards, debris, frost decals ---------- */
        void ICraftSink.CBox(bool top, int x, int y, int w, int h, Color32 c) => Box(top, x, y, w, h, c);
        void ICraftSink.CShadow(int x, int y, int w, int h, byte a) => Shadow(x, y, w, h, a);
        readonly CraftDebris _deb = new CraftDebris(512);
        const int MaxDec = 48; struct Dec { public Vector2 c; public float r, t, life; public int seed; }
        readonly Dec[] _dec = new Dec[MaxDec]; int _nDec;
        void AddDecal(Vector2 c, float r, float life, int seed) { if (_nDec < MaxDec) _dec[_nDec++] = new Dec { c = c, r = r, life = life, seed = seed }; }
        /* hazard border: 3 px blue outside, 3 px pale ice inside (white in the last 15 %), hand-drawn wobble, squash on the first frame, pop overshoot +6,+3,+1,0 px */
        void Border(Vector2 c, float r, float p, float t, int seed, bool dashed = false)
        {
            float pop = t < 0.033f ? 6 : t < 0.067f ? 3 : t < 0.1f ? 1 : 0, sy = t < 0.033f ? 0.85f : 1f; r += pop;
            Craft.RoughBand(this, false, c, r - 3, r, 0, Craft.Two, Bl, seed, 2f, dashed ? -1f : 0f, 1f, 0, 24, sy);
            Craft.RoughBand(this, false, c, r - 6, r - 3, 0, Craft.Two, p >= 0.85f ? Wh : Ice, seed, 2f, dashed ? -1f : 0f, 1f, 0, 24, sy);
        }
        /* strike wave: overshoot ease, pale ice lead + dithered blue trail, breaks into chunks as it fades */
        void Wave(Vector2 c, float r0, float r1, float u, int seed)
        {
            if (u < 0 || u >= 1) return; float r = r0 + (r1 - r0) * Craft.EaseBack(u), br = u < 0.35f ? 0f : (u - 0.35f) / 0.65f * 0.7f;
            Craft.RoughBand(this, false, c, r - 6, r, 0, Craft.Two, Ice, seed, 3f, br);
            if (r > 15) Craft.RoughBand(this, false, c, r - 12, r - 6, 0, Craft.Two, Bl, seed, 3f, br, u < 0.5f ? 0.75f : 0.45f, seed + 9);
        }
        void Burst(Vector2 c, int seed, bool big)
        {
            _deb.Spawn(CraftDebris.Chip, c, big ? 14 : 6, big ? 120 : 80, 95, Wh, Ice, Bl, Ice2, seed, 0.012f, 0.9f, 2);
            _deb.Spawn(CraftDebris.Shard, c, big ? 6 : 3, big ? 95 : 70, 120, Wh, Ice, Ice2, Ice2, seed + 7, 0.02f, 1.1f);
            AddDecal(c, big ? IceGorillaFX.SlamRadius * HellFX.PPU * 0.55f : 14, 1.8f, seed);
        }
        static readonly int[] Wd2 = { 3, 3, 6, 6, 9, 9, 12, 12 };
        /* angular icicle, tip down: lit left facet, shaded right facet, white edge, dark rim, broken base. sq 1 = squash, 2 = overshoot */
        void IcicleSpr(int x, int y, int sq, int seed)
        {
            int rows = sq == 0 ? 8 : sq == 1 ? 5 : 9;
            for (int s = 0; s < rows; s++)
            {
                int w = sq != 1 ? Wd2[Mathf.Min(s, 7)] : Mathf.Min(15, Wd2[Mathf.Min(s, 7)] + 3), yy = y + s * 3, x0 = x - w / 2;
                if (s >= 7 && Hs(seed, s) < 0.5f) { Box(true, x0 + 3, yy, Mathf.Max(3, w - 6), 3, Ice2); continue; }
                int lw = Mathf.Max(3, (w / 6) * 3); Box(true, x0, yy, lw, 3, Ice); if (w - lw > 0) Box(true, x0 + lw, yy, w - lw, 3, Ice2);
                if (w >= 6 && s % 3 != 2) Box(true, x0, yy, 3, 3, Wh); if (w >= 9) Box(true, x0 + w - 3, yy, 3, 3, Bd);
            }
            if (sq == 0 && Hs(seed, 40) < 0.6f) Box(true, x + 3, y + 24, 3, 3, Ice);
        }
        void BoulderSpr(Vector2 c, float r, float t, int seed)
        {
            int rot = Mathf.FloorToInt(t / 0.09f); Craft.Blob(this, true, c, r, Bd, Ice2, Ice, Wh, 7, seed + rot * 13, 0.28f);
            if (r >= 9) { int x = Mathf.RoundToInt(c.x + r * 0.15f), y = Mathf.RoundToInt(c.y - r * 0.35f); Box(true, x, y, 6, 3, Bd); Box(true, x + 3, y + 3, 3, 3, Bd); }
        }
        /* kite-shaped ice shard: white tip, two facet cells, blue tail, flickering dark wake */
        void ShardSpr(Vector2 p, Vector2 d, float t)
        {
            Vector2 n = new Vector2(-d.y, d.x); Box(true, Mathf.RoundToInt(p.x) - 1, Mathf.RoundToInt(p.y) - 1, 3, 3, Wh);
            Vector2 q = p - d * 4 - n * 3; Box(true, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Ice);
            q = p - d * 4 + n * 3; Box(true, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Ice2);
            q = p - d * 8; Box(true, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Bl);
            if (Mathf.FloorToInt(t * 15f) % 3 != 2) { q = p - d * 12; Box(true, Mathf.RoundToInt(q.x) - 1, Mathf.RoundToInt(q.y) - 1, 3, 3, Bd); }
        }

        void Update()
        {
            if (_mg == null) return; float dt = IceGorillaFX.Paused ? 0f : Time.deltaTime; _ngs = _ngb = _ngf = _ntb = _ntf = 0;
            for (int i = 0; i < _it.Length; i++) { if (_it[i].kind == IceGorillaKind.None) continue; ref var k = ref _it[i]; k.age += dt; k.pt += dt; if (!Step(ref k, i)) Free(i); }
            int w = 0; for (int j = 0; j < _nDec; j++) { _dec[j].t += dt; Craft.DecalFrost(this, _dec[j].c, _dec[j].r, _dec[j].t, _dec[j].life, _dec[j].seed, Ice2, Ice); if (_dec[j].t < _dec[j].life) _dec[w++] = _dec[j]; } _nDec = w;
            _deb.Step(dt); _deb.Draw(this);
            Flush(_mg, _vg, _cg, ref _lastG, true); Flush(_mt, _vt, _ct, ref _lastT, false);
        }

        bool Step(ref It k, int i)
        {
            float p = Mathf.Clamp01(k.pt / k.tell);
            switch (k.kind)
            {
                case IceGorillaKind.Slam:
                case IceGorillaKind.Roar:
                {
                    bool roar = k.kind == IceGorillaKind.Roar;
                    if (k.ph == IceGorillaPhase.Tell)
                    {
                        Craft.ShadowBlob(this, k.a, k.r, (byte)(60 + 50 * p), k.seed, 0.18f, 1f); Border(k.a, k.r, p, k.pt, k.seed);
                        float fr = roar ? k.r * (1 - Mathf.Min(1, p / 0.85f)) + 9 : k.r * Mathf.Min(1, p / 0.85f);
                        if (fr > 6 && fr < k.r - 6) Craft.RoughBand(this, false, k.a, fr - 3, fr, 0, Craft.Two, Ice2, k.seed + 3, 2f, 0f, 0.8f, Mathf.FloorToInt(k.pt * 10));
                        if (!roar && p > 0.5f && Mathf.FloorToInt(k.pt * 20) % 4 == 0) _deb.Spawn(CraftDebris.Snow, k.a, 1, 60, 40, Ice, Ice2, Ice, Ice2, k.seed + Mathf.FloorToInt(k.pt * 100), 0, 0.6f);
                        if (k.pt >= k.tell) { Go(ref k, IceGorillaPhase.Strike); IceGorillaFX.OnStrike?.Invoke(Hd(i), k.kind); }
                        return true;
                    }
                    float wd = roar ? IceGorillaFX.RoarWaveSeconds : IceGorillaFX.WaveSeconds;
                    if (!k.sp) { k.sp = true; if (roar) _deb.Spawn(CraftDebris.Snow, k.a, 18, 150, 80, Wh, Ice, Ice2, Ice, k.seed, 0.01f, 1f); else Burst(k.a, k.seed, true); }
                    Craft.Impact(this, k.a, 18, k.pt, k.seed, Wh, Bd);
                    if (roar) { for (int w = 0; w < 3; w++) Wave(k.a, 12, k.r + 6, (k.pt - w * 0.07f) / wd, k.seed + w * 31); }
                    else Wave(k.a, k.r * 0.25f, k.r + 6, k.pt / wd, k.seed);
                    return k.pt < wd + (roar ? 0.14f : 0.45f);
                }
                case IceGorillaKind.Icicle:
                {
                    if (k.ph == IceGorillaPhase.Delay) { if (k.pt >= k.x1) Go(ref k, IceGorillaPhase.Tell); return true; }
                    int tx = Mathf.RoundToInt(k.a.x), ty = Mathf.RoundToInt(k.a.y);
                    if (k.ph == IceGorillaPhase.Tell)
                    {
                        float sr = Mathf.Max(3, Mathf.Floor(k.r * (0.3f + 0.7f * p) / 3f) * 3f); if (!IceGorillaFX.HideTell) { Craft.ShadowBlob(this, k.a, sr, (byte)(70 + 80 * p), k.seed, 0.2f, 1f); Border(k.a, k.r, p, k.pt, k.seed); }
                        float hgt = IceGorillaFX.IcicleFallPx * (1 - p * p); if (p > 0.2f) IcicleSpr(tx, ty + Mathf.RoundToInt(hgt), 0, k.seed);
                        if (k.pt >= k.tell) { Go(ref k, IceGorillaPhase.Stay); IceGorillaFX.OnStrike?.Invoke(Hd(i), k.kind); }
                        return true;
                    }
                    if (k.ph == IceGorillaPhase.Stay)
                    {
                        if (!k.sp) { k.sp = true; _deb.Spawn(CraftDebris.Chip, k.a, 5, 70, 70, Wh, Ice, Bl, Wh, k.seed, 0.015f, 0.7f); }
                        Wave(k.a, 4, k.r + 4, k.pt / 0.2f, k.seed); Craft.ShadowBlob(this, k.a, k.r * 0.6f, 120, k.seed, 0.25f, 0.8f);
                        Craft.Impact(this, new Vector2(tx, ty + 3), 9, k.pt, k.seed, Wh, Bd);
                        IcicleSpr(tx, ty - 3, k.pt < 0.067f ? 1 : k.pt < 0.134f ? 2 : 0, k.seed);
                        if (k.pt >= k.x2) Go(ref k, IceGorillaPhase.Shatter); return true;
                    }
                    if (!k.sp)
                    {
                        k.sp = true; _deb.Spawn(CraftDebris.Shard, k.a + new Vector2(0, 9), 6, 85, 110, Wh, Ice, Ice2, Ice2, k.seed + 5, 0.01f, 1f);
                        _deb.Spawn(CraftDebris.Chip, k.a, 4, 60, 60, Ice, Bl, Ice, Bl, k.seed + 6, 0.02f, 0.7f); AddDecal(k.a, 12, 1.4f, k.seed);
                    }
                    Wave(k.a, 3, k.r, k.pt / IceGorillaFX.ShatterSeconds * 1.4f, k.seed + 5);
                    return k.pt < IceGorillaFX.ShatterSeconds;
                }
                case IceGorillaKind.Boulder:
                {
                    Vector2 hand = k.a + new Vector2(0, IceGorillaFX.BoulderHandPx), away = k.b - k.a; if (away.sqrMagnitude < 1) away = Vector2.down; away.Normalize();
                    float bd = Mathf.Atan2(away.y, away.x), fan = IceGorillaFX.ShardFanDeg * Mathf.Deg2Rad; int ns = Mathf.Clamp(IceGorillaFX.Shards, 1, 31);
                    if (k.ph == IceGorillaPhase.Tell)
                    {
                        float tot = k.tell + k.x1, q = Mathf.Clamp01(k.pt / tot), fl = Mathf.Clamp01((k.pt - k.tell) / Mathf.Max(0.05f, k.x1));
                        if (!IceGorillaFX.HideTell) { Craft.ShadowBlob(this, k.b, Mathf.Max(3, Mathf.Floor(k.r * (0.3f + 0.7f * q) / 3f) * 3f), (byte)(60 + 80 * q), k.seed, 0.2f, 1f); Border(k.b, k.r, q, k.pt, k.seed); }
                        if (k.pt < k.tell)
                        {
                            /* anticipation: grows in the hand with an overshoot, squashes down just before the throw */
                            float g = Craft.EaseBack(Mathf.Min(1, p / 0.7f)), sq = p > 0.85f ? 0.8f : 1f;
                            Craft.Blob(this, true, hand + new Vector2(0, p > 0.85f ? -3 : 0), 6 + 9 * g, Bd, Ice2, Ice, Wh, 7, k.seed, 0.28f, 1f / sq, sq);
                            if (Mathf.FloorToInt(k.pt * 20) % 3 == 0) _deb.Spawn(CraftDebris.Snow, hand, 1, 40, 30, Ice, Ice, Ice, Ice, k.seed + Mathf.FloorToInt(k.pt * 60), 0, 0.5f);
                        }
                        else
                        {
                            Vector2 pos = Vector2.Lerp(hand, k.b, fl) + Vector2.up * (Mathf.Sin(fl * Mathf.PI) * IceGorillaFX.BoulderArcPx); BoulderSpr(pos, 13, k.pt, k.seed);
                            if (fl > 0.5f) for (int s = 0; s < ns; s++)
                            {
                                float a = bd + (ns == 1 ? 0 : Mathf.Lerp(-fan, fan, s / (float)(ns - 1))); Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                                for (int dot = 0; dot < 3; dot++) { Vector2 o = k.b + d * (k.r + 6 + dot * 9); Box(false, Mathf.RoundToInt(o.x) - 1, Mathf.RoundToInt(o.y) - 1, 3, 3, Ice); }
                            }
                        }
                        if (k.pt >= tot) { Go(ref k, IceGorillaPhase.Strike); IceGorillaFX.OnStrike?.Invoke(Hd(i), k.kind); }
                        return true;
                    }
                    if (!k.sp) { k.sp = true; Burst(k.b, k.seed, true); }
                    Craft.Impact(this, k.b, 16, k.pt, k.seed, Wh, Bd);
                    if (k.pt >= 0.067f && k.pt < 0.2f) Craft.Blob(this, true, k.b + new Vector2(0, -3), 13, Bd, Ice2, Ice, Wh, 7, k.seed, 0.3f, 1.35f, 0.6f, 1f - (k.pt - 0.067f) / 0.133f, k.seed);
                    Wave(k.b, k.r * 0.3f, k.r + 6, k.pt / IceGorillaFX.WaveSeconds, k.seed);
                    bool any = false; float sp = IceGorillaFX.ShardSpeed * HellFX.PPU;
                    if (k.pt < IceGorillaFX.ShardLife) for (int s = 0; s < ns; s++)
                    {
                        if ((k.shardMask & (1 << s)) != 0) continue; any = true;
                        float a = bd + (ns == 1 ? 0 : Mathf.Lerp(-fan, fan, s / (float)(ns - 1))); Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)); ShardSpr(k.b + d * (sp * k.pt), d, k.pt + s * 0.07f);
                    }
                    return any || k.pt < IceGorillaFX.WaveSeconds + 0.45f;
                }
                case IceGorillaKind.Leap:
                {
                    if (k.ph == IceGorillaPhase.Tell)
                    {
                        bool locked = k.tell - k.pt <= k.x1; float sr = Mathf.Max(6, Mathf.Floor(k.r * (0.25f + 0.75f * p) / 3f) * 3f);
                        Craft.ShadowBlob(this, k.a, sr, (byte)(60 + 90 * p), k.seed, 0.18f, 1f); Border(k.a, k.r, p, locked ? k.pt - (k.tell - k.x1) : 1f, k.seed, !locked);
                        if (k.pt >= k.tell) { Go(ref k, IceGorillaPhase.Strike); IceGorillaFX.OnStrike?.Invoke(Hd(i), k.kind); }
                        return true;
                    }
                    if (!k.sp) { k.sp = true; Burst(k.a, k.seed, true); _deb.Spawn(CraftDebris.Snow, k.a, 10, 140, 70, Ice, Ice2, Ice, Ice2, k.seed + 3, 0.01f, 0.8f); }
                    Craft.Impact(this, k.a, 20, k.pt, k.seed, Wh, Bd);
                    Wave(k.a, k.r * 0.3f, k.r + 6, k.pt / IceGorillaFX.WaveSeconds, k.seed);
                    return k.pt < IceGorillaFX.WaveSeconds + 0.45f;
                }
                case IceGorillaKind.Slide:
                {
                    int o = i * MaxPts, live = 0; float life = k.x1;
                    for (int j = 0; j < k.n; j++)
                    {
                        float left = life - (k.age - _ptT[o + j]); if (left <= 0) continue; live++;
                        float rr = k.r; if (left < 0.6f) rr -= 3 * Mathf.Floor((0.6f - left) / 0.15f); if (rr < 6) continue;
                        float keep = left >= 0.3f ? 1f : left / 0.3f; Vector2 c = _pts[o + j];
                        Craft.Blob(this, false, c, rr, Bd, Bl, Ice2, Ice, 7, k.seed + j * 7, 0.3f, 1f, 0.8f, keep, k.seed + j);
                        if (Hs(k.seed, j) < 0.45f && keep >= 1f) Box(false, Mathf.RoundToInt(c.x + (Hs(k.seed, j + 1) - 0.5f) * rr), Mathf.RoundToInt(c.y + (Hs(k.seed, j + 2) - 0.5f) * rr * 0.6f), 3, 3, Wh);
                    }
                    if (!k.ended && k.n > 0 && Mathf.FloorToInt(k.age * 30) % 2 == 0) _deb.Spawn(CraftDebris.Snow, _pts[o + k.n - 1], 1, 90, 60, Wh, Ice, Ice2, Ice, k.seed + Mathf.FloorToInt(k.age * 90), 0, 0.6f);
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
