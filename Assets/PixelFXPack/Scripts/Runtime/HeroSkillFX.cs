using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="HeroSkillFX"/> item.</summary>
    public enum HeroSkillKind { None = 0, Chain = 1, Pillar = 2, Whip = 3, Spear = 4, Pool = 5 }

    /// <summary>
    /// 186-190. Hellord v0.0.44 hero skills (v2.13.0). World space, 16 PPU, sizes in world units like HellordTuning. HERO language only: white core, silver (#c0cbdc) accents, dark outline,
    /// no hazard borders, no fills that read as danger zones, no blinking / wind-up steps. Every skill has its own colour family, clear of the enemy reds (#e43b44 / #a22633 / #f6757a / #be4a2f).
    /// 186 CHAIN LIGHTNING (<see cref="ChainBolt"/>): sky-blue #0099db + white jagged bolt (re-jitters at 15 fps), side forks, hop-by-hop reveal (0.03 s), shock star + chip spray on each demon. Not Storm Ring's deep blue ring.
    /// 187 HELLFIRE PILLAR (<see cref="PillarCast"/>), fx_redo Task 4 remake: <see cref="PillarTellS"/> ground cracks glow red->orange->gold over a dark disc + floor light -> white squash dome -> thin over-tall stretch spike + shock ring
    /// -> roaring 1 px-row column (white/gold/amber/orange/red heat noise, #3f2832 outer ring, no black outline) with licks, rising chunks, embers, falling sparks, violet-grey smoke -> collapse -> scorch whose cracks cool. 2.2 s after the tell.
    /// 188 WHIP (<see cref="WhipLash"/>, <see cref="WhipHit"/>), fx_redo Task 8 v5 OCTOPUS TENTACLE: v4 smooth eased motion + wave, tapered magenta/violet tentacle with sucker row and wet glint, curling tip at rest/recoil, wet slap splash (~26 px) with ink/slime droplets at 0.16 s.
    /// Hit: white plus-flash -> cream X -> rose corners + 4 crimson droplets that splat wine; heal mote flying to the hero + "+" pop (unchanged). Never hazard red.
    /// 189 SOUL SPEARS (<see cref="Spear"/>, <see cref="SpearMove"/>, <see cref="SpearPierce"/>, <see cref="SpearEnd"/>): green #63c74d shaft, white diamond head, silver fletching, orbiting wisps; pierce = 2-frame white -> green ring pop.
    /// 190 GRAVE POOL (<see cref="GravePool"/>, <see cref="PoolSlowed"/>): dark teal #193c3e / #265c42 pool with a soft dithered edge (no border), slow swirl, outward tick ripple every 0.5 s, ivory #ead4aa bubbles; slowed demons get teal tendrils.
    /// Ground art on the GROUND mesh, bolts / column / lash / spears on the TOP mesh. fx_redo: rects at least 1 px (1 px detail, small hot cores), opaque; only shadows use alpha. Visual only: the game keeps the rules.
    /// </summary>
    public static class HeroSkillFX
    {
        static HeroSkillRunner R => HeroSkillRunner.I;
        /// <summary>Defaults = HellordTuning v0.0.44.</summary>
        public static float ChainBoltS = 0.22f, ChainHopS = 0.03f, PillarTellS = 0.5f, PoolTickS = 0.5f, SpearSpeedU = 14f;
        public static int GroundSortingOrder = -20000, TopSortingOrder = 70; public static string SortingLayer = "Default"; public static Material MaterialOverride;
        public static bool Paused;
        /// <summary>Hellord v0.0.46: true = original 3 px Hellfire Pillar instead of the fx_redo Task 4 remake.</summary>
        public static bool ClassicPillar;
        /// <summary>t934u: the game draws the pillar tell + crust with the PixelFX934 shader (mode 10); skip the CPU ground marks.</summary>
        public static bool PillarGpuGround;
        /// <summary>t934u Spear v2 sharp head (off = old round cell head) + its shape numbers.</summary>
        public static bool SpearV2 = true; public static float SpearHL = 11f, SpearHW = 2.9f, SpearWideAt = 8f;
        /// <summary>t934u: impact chips = dust, bone, char (VH batch6 pillar v2).</summary>
        public static Color32 PillarChipA = new Color32(0x8b, 0x9b, 0xb4, 255), PillarChipB = new Color32(0xea, 0xd4, 0xaa, 255), PillarChipC = new Color32(0x2b, 0x1d, 0x24, 255), PillarChipD = new Color32(0x5a, 0x4e, 0x68, 255);
        /// <summary>Hellord v0.0.46: true = original 3 px Blood Whip instead of the fx_redo v5 octopus tentacle.</summary>
        public static bool ClassicWhip;
        /// <summary>Hellord t934u: blood-red tentacle palette.</summary>
        public static void SetBloodRed(bool on) => HeroSkillRunner.SetBloodRed(on);
        /// <summary>Hellord t948u: darker blood whip (no white glints/flashes).</summary>
        public static void SetWhipDark(bool on) => HeroSkillRunner.SetWhipDark(on);
        /// <summary>Hellord t957u: whip blood colours (0xRRGGBB dark, mid, highlight).</summary>
        public static void SetWhipColors(int dk, int md, int hi) => HeroSkillRunner.SetWhipColors(dk, md, hi);
        /// <summary>Hellord t965u: whip value roles.</summary>
        public static void SetWhipRoles(bool on, int ol, int bd, int un, int ed, int gl, int fc, int fk, int mc, int mt) => HeroSkillRunner.SetWhipRoles(on, ol, bd, un, ed, gl, fc, fk, mc, mt);
        /// <summary>Hero position (world units, feet). Heal motes fly here and whips follow it; set it every frame.</summary>
        public static Vector2 Hero;

        public static void Prewarm(int items = 64) => R.Prewarm(items);
        public static void SetHero(Vector2 feetU) => Hero = feetU;
        /// <summary>One chain cast: the hero hand first, then each struck demon in hop order (world units, max 16 points).</summary>
        public static int ChainBolt(Vector2[] ptsU) => R.AddChain(ptsU);
        /// <summary>One pillar: hero sigil for <see cref="PillarTellS"/>, then the eruption. delayS = stagger (Hellord pillarStaggerSeconds * index).</summary>
        public static int PillarCast(Vector2 atU, float radiusU, float delayS = 0f) => R.Add(HeroSkillKind.Pillar, atU, Vector2.zero, radiusU, 0, delayS, 0);
        /// <summary>One lash from the hero (follows <see cref="Hero"/>) along dir. delayS = lash index * 0.14.</summary>
        public static int WhipLash(Vector2 dirU, float reachU, float halfWidthU, float delayS = 0f) => R.Add(HeroSkillKind.Whip, Hero, dirU.sqrMagnitude > 1e-6f ? dirU.normalized : Vector2.right, reachU, halfWidthU, delayS, 0);
        /// <summary>A demon hit by the whip: slash mark + heal mote that flies to the hero. heal = false skips the mote (heal cap reached).</summary>
        public static void WhipHit(Vector2 atU, bool heal = true) => R.WhipHit(atU, heal);
        public static int Spear(Vector2 posU, Vector2 dirU) => R.Add(HeroSkillKind.Spear, posU, dirU.sqrMagnitude > 1e-6f ? dirU.normalized : Vector2.right, 0, 0, 0, 0);
        /// <summary>Feed the projectile position each frame (otherwise it flies at <see cref="SpearSpeedU"/>).</summary>
        public static void SpearMove(int h, Vector2 posU) => R.SpearMove(h, posU);
        public static void SpearPierce(int h, Vector2 atU) => R.SpearPierce(h, atU);
        public static void SpearEnd(int h) => R.SpearEnd(h);
        public static int GravePool(Vector2 centerU, float radiusU, float lifeS) => R.Add(HeroSkillKind.Pool, centerU, Vector2.zero, radiusU, 0, 0, lifeS);
        /// <summary>Teal tendrils at a slowed demon's feet for this frame.</summary>
        public static void PoolSlowed(Vector2 feetU) => R.Slowed(feetU);
        public static void Cancel(int h) { if (HeroSkillRunner.Exists) R.Cancel(h); }
        public static void Clear() { if (HeroSkillRunner.Exists) R.ClearAll(); }
        public static int ActiveCount => HeroSkillRunner.Exists ? R.Count : 0;
        /// <summary>Smallest rect side emitted so far (QC: must stay at least 3).</summary>
        public static int MinSidePx => HeroSkillRunner.Exists ? R.MinSide : 999;
    }

    public class HeroSkillRunner : MonoBehaviour, ICraftSink
    {
        static HeroSkillRunner _i;
        public static bool Exists => _i != null;
        public static HeroSkillRunner I { get { if (_i == null) { var go = new GameObject("HeroSkillFX"); DontDestroyOnLoad(go); _i = go.AddComponent<HeroSkillRunner>(); } return _i; } }

        const int MaxPts = 16, MaxPops = 4;
        struct It { public HeroSkillKind kind; public Vector2 a, d; public float r, w, age, delay, life, dead; public int seed, n, flags; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; Vector2[] _pts = new Vector2[0], _prev = new Vector2[0]; Vector3[] _pops = new Vector3[0];
        public int Count { get; private set; }
        public int MinSide { get; private set; } = 999;

        const int MaxQ = 24000 /* fx_redo: 1 px rows need ~3x the quads */, MaxFx = 64;
        struct Q { public int x, y, w, h; public Color32 c; }
        readonly Q[] _gs = new Q[MaxQ], _gl = new Q[MaxQ], _gb = new Q[MaxQ], _gf = new Q[MaxQ], _tb = new Q[MaxQ], _tf = new Q[MaxQ]; int _ngs, _ngl, _ngb, _ngf, _ntb, _ntf;
        Mesh _mg, _mt; MeshRenderer _rg, _rt; Vector3[] _vg, _vt; Color32[] _cg, _ct; int _lastG, _lastT;
        readonly Vector3[] _slash = new Vector3[MaxFx], _mote = new Vector3[MaxFx], _motePos = new Vector3[MaxFx]; readonly float[] _healPop = new float[8]; int _nSlash, _nMote, _nPop;
        readonly Vector2[] _slow = new Vector2[256]; int _nSlow;
        /* Endesga 32 + hero sprite palette */
        static readonly Color32 Ko = C(0x181425), Wh = C(0xffffff), Sv = C(0xc0cbdc),
            Sk = C(0x0099db), Sn = C(0x262b44),
            Or = C(0xf77622), Am = C(0xfeae34), Gd = C(0xfee761), Fr = C(0xbe4a2f), /* fx_redo: red edge of the pillar ramp */
            Wn = C(0x4a0a28), Rs = C(0xff9aa8), Cr = C(0xfff4dc),
            Gn = C(0x63c74d), Dg = C(0x3e8948),
            Te = C(0x193c3e), Md = C(0x265c42), Iv = C(0xead4aa);
        static Color32 C(int h) => new Color32((byte)(h >> 16), (byte)(h >> 8), (byte)h, 255);
        static float Hs(int a, int b) => Craft.Hs(a, b);
        static int Rn(float v) => Mathf.RoundToInt(v);
        static int Q3(float v) => Craft.Q3(v);
        static Vector2 Perp(Vector2 d) => new Vector2(-d.y, d.x);
        static Vector2 Rot(Vector2 v, float a) { float c = Mathf.Cos(a), s = Mathf.Sin(a); return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c); }
        float _clock;

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length);
                var np = new Vector2[cap * MaxPts]; Array.Copy(_pts, np, _pts.Length); _pts = np;
                var nv = new Vector2[cap * 64]; Array.Copy(_prev, nv, _prev.Length); _prev = nv;
                var no = new Vector3[cap * MaxPops]; Array.Copy(_pops, no, _pops.Length); _pops = no;
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == HeroSkillKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mg != null) return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX HeroSkill White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = HeroSkillFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "HeroSkillFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] HeroSkillFX: no sprite shader found; set HeroSkillFX.MaterialOverride.");
            _mg = Make("HeroSkillFX Ground", m, MaxQ * 4, out _rg, out _vg, out _cg); _mt = Make("HeroSkillFX Top", m, MaxQ * 2, out _rt, out _vt, out _ct); ApplySorting();
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
            if (_rg == null) return; _rg.sortingOrder = HeroSkillFX.GroundSortingOrder; _rt.sortingOrder = HeroSkillFX.TopSortingOrder;
            _rg.sortingLayerName = HeroSkillFX.SortingLayer; _rt.sortingLayerName = HeroSkillFX.SortingLayer;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != HeroSkillKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = HeroSkillKind.None; _free[_freeN++] = i; Count--; }

        public int Add(HeroSkillKind kind, Vector2 aU, Vector2 d, float rU, float wU, float delay, float life)
        {
            if (_mg == null) Prewarm(64); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++;
            ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k = new It { gen = k.gen, kind = kind, a = aU * ppu, d = d, r = rU * ppu, w = wU * ppu, delay = Mathf.Max(0, delay), life = life, dead = -1, seed = (i * 7919 + k.gen * 131) & 0xFFFF };
            for (int p = 0; p < MaxPops; p++) _pops[i * MaxPops + p].z = 9;
            return Hd(i);
        }
        public int AddChain(Vector2[] ptsU)
        {
            if (ptsU == null || ptsU.Length < 2) return -1; int h = Add(HeroSkillKind.Chain, ptsU[0], Vector2.zero, 0, 0, 0, 0); if (h < 0) return h;
            int i = h & 0xFFFF; ref var k = ref _it[i]; k.n = Mathf.Min(MaxPts, ptsU.Length);
            for (int p = 0; p < k.n; p++) _pts[i * MaxPts + p] = ptsU[p] * HellFX.PPU; return h;
        }
        public void WhipHit(Vector2 atU, bool heal)
        {
            if (_mg == null) Prewarm(64); Vector2 p = atU * HellFX.PPU; int sd = Mathf.FloorToInt(_clock * 977 + p.x) & 0xFFFF;
            if (_nSlash < MaxFx) _slash[_nSlash++] = new Vector3(p.x, p.y, 0);
            _deb.Spawn(CraftDebris.Chip, p, 3, 50, 60, WDk, WMd, WDk, WCr, sd, 0, 0.4f); /* t957u: was Rs pink */
            if (heal && _nMote < 12) { _motePos[_nMote] = new Vector3(p.x, p.y, 0); _mote[_nMote++] = new Vector3(0, sd, 0); }
        }
        public void SpearMove(int h, Vector2 posU) { if (Get(h, out int i) && _it[i].kind == HeroSkillKind.Spear) { _it[i].a = posU * HellFX.PPU; _it[i].flags |= 1; } }
        public void SpearPierce(int h, Vector2 atU)
        {
            if (!Get(h, out int i)) return; Vector2 p = atU * HellFX.PPU; int o = i * MaxPops, best = o;
            for (int q = o; q < o + MaxPops; q++) if (_pops[q].z > _pops[best].z) best = q;
            _pops[best] = new Vector3(p.x, p.y, 0); _deb.Spawn(CraftDebris.Shard, p, 2, 50, 50, Gn, Wh, Dg, Gn, _it[i].seed + Mathf.FloorToInt(_clock * 61), 0, 0.35f);
        }
        public void SpearEnd(int h)
        {
            if (!Get(h, out int i) || _it[i].dead >= 0) return; ref var k = ref _it[i]; k.dead = 0;
            _deb.Spawn(CraftDebris.Shard, k.a, 4, 70, 60, Gn, Wh, Dg, Gn, k.seed + 99, 0, 0.5f); _deb.Spawn(CraftDebris.Ember, k.a, 2, 8, 20, Gn, Dg, Gn, Dg, k.seed + 7, 0.05f, 0.7f);
        }
        public void Slowed(Vector2 feetU) { if (_nSlow < _slow.Length) _slow[_nSlow++] = feetU * HellFX.PPU; }
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { _deb.Clear(); _nSlash = _nMote = _nPop = _nSlow = 0; for (int i = 0; i < _it.Length; i++) if (_it[i].kind != HeroSkillKind.None) Free(i); }

        /* ---------- emit: every coloured rect gets a dark outline rect in the backing pass ---------- */
        void Box(bool top, int x, int y, int w, int h, Color32 c)
        {
            if (w <= 0 || h <= 0) return; if (_fine) { if (w < 1) w = 1; if (h < 1) h = 1; } else { if (w < 3) { x -= (3 - w) / 2; w = 3; } if (h < 3) h = 3; }
            if (top) { if (_ntf >= MaxQ) return; _tf[_ntf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _tb[_ntb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            else { if (_ngf >= MaxQ) return; _gf[_ngf++] = new Q { x = x, y = y, w = w, h = h, c = c }; _gb[_ngb++] = new Q { x = x - 1, y = y - 1, w = w + 2, h = h + 2, c = Ko }; }
            int m = w < h ? w : h; if (m < MinSide) MinSide = m;
        }
        void Shadow(int x, int y, int w, int h, byte a) { if (w > 0 && h > 0 && _ngs < MaxQ) _gs[_ngs++] = new Q { x = x, y = y, w = w, h = h, c = new Color32(0x18, 0x14, 0x25, a) }; }
        void ICraftSink.CBox(bool top, int x, int y, int w, int h, Color32 c) => Box(top, x, y, w, h, c);
        void ICraftSink.CShadow(int x, int y, int w, int h, byte a) => Shadow(x, y, w, h, a);
        readonly CraftDebris _deb = new CraftDebris(512);
        readonly CraftDebris1 _deb1 = new CraftDebris1(512);
        void Cell(bool top, Vector2 q, Color32 c) => Box(top, Rn(q.x) - 1, Rn(q.y) - 1, 3, 3, c);
        void Cell2(bool top, Vector2 q, Color32 c) => Box(top, Rn(q.x) - 1, Rn(q.y) - 1, 2, 2, c); /* fx_redo: 2 px detail cell, used by the swapped pillar / whip */
        static int Q31(float v) => Craft1.Q3(v);
        bool _fine; /* true while a 1 px fx_redo effect draws (1 px box minimum); false = original 3 px minimum */

        void Update()
        {
            if (_mg == null) return; float dt = HeroSkillFX.Paused ? 0f : Time.deltaTime; _clock += dt; _ngs = _ngl = _ngb = _ngf = _ntb = _ntf = 0;
            for (int pass = 0; pass < 2; pass++)   // pools first (ground), then the rest
                for (int i = 0; i < _it.Length; i++)
                {
                    if (_it[i].kind == HeroSkillKind.None || (_it[i].kind == HeroSkillKind.Pool) != (pass == 0)) continue; ref var k = ref _it[i];
                    if (pass == 0 || k.kind != HeroSkillKind.Pool) k.age += dt;
                    if (k.age < k.delay) continue; float a = k.age - k.delay; bool alive = true;
                    switch (k.kind)
                    {
                        case HeroSkillKind.Chain: alive = Chain(ref k, i, a); break;
                        case HeroSkillKind.Pillar: if (HeroSkillFX.ClassicPillar) alive = PillarV0(ref k, a); else { _fine = true; alive = Pillar(ref k, a); _fine = false; } break;
                        case HeroSkillKind.Whip: if (HeroSkillFX.ClassicWhip) alive = WhipV0(ref k, i, a); else { _fine = true; alive = Whip(ref k, i, a); _fine = false; } break;
                        case HeroSkillKind.Spear: alive = Spear(ref k, i, a, dt); break;
                        case HeroSkillKind.Pool: alive = Pool(ref k, a); break;
                    }
                    if (!alive) Free(i);
                }
            Slowed(); if (HeroSkillFX.ClassicWhip) WhipFxV0(dt); else { _fine = true; WhipFx(dt); _fine = false; }
            _deb.Step(dt); _deb.Draw(this); _fine = true; _deb1.Step(dt); _deb1.Draw(this); _fine = false;
            Flush(_mg, _vg, _cg, ref _lastG, true); Flush(_mt, _vt, _ct, ref _lastT, false);
        }

        /* ---------- 186 chain lightning ---------- */
        bool Chain(ref It s, int i, float a)
        {
            int o = i * MaxPts, fr = Mathf.FloorToInt(a * 15); float life = HeroSkillFX.ChainBoltS, hop = HeroSkillFX.ChainHopS;
            if (a < 0.1f) for (int k = 0; k < 3; k++) Cell(true, _pts[o] + new Vector2(Mathf.Cos(k * 2.1f + a * 9) * 6, Mathf.Sin(k * 2.1f + a * 9) * 6 + 3), k == 0 ? Wh : Sk);
            for (int k = 0; k < s.n - 1; k++)
            {
                float sa = a - k * hop; if (sa < 0) break;
                Vector2 A = _pts[o + k], B = _pts[o + k + 1];
                if ((s.flags & (1 << k)) == 0) { s.flags |= 1 << k; _deb.Spawn(CraftDebris.Chip, B, 4, 80, 90, Wh, Sk, Sk, Sv, s.seed + k * 7, 0, 0.4f); }
                if (sa < 0.067f) Craft.Impact(this, B, 9, sa, s.seed + k, Wh, Sn);
                if (sa < 0.34f) { float L = sa > 0.22f ? 3 : 6; for (int j = 0; j < 4; j++) { float an = j * Mathf.PI / 2 + (sa > 0.15f ? 0.4f : 0); Cell(true, B + new Vector2(Mathf.Cos(an), Mathf.Sin(an)) * L, sa > 0.1f ? Sk : Wh); } }
                if (sa > life) continue;
                float u = sa / life, keep = u < 0.55f ? 1 : 1 - (u - 0.55f) / 0.45f;
                Vector2 d = B - A; float Ln = d.magnitude; if (Ln < 1e-3f) continue; Vector2 n = Perp(d / Ln); int m = Mathf.Max(2, (int)(Ln / 9));
                Vector2 p0 = A;
                for (int q = 1; q <= m; q++)
                {
                    Vector2 p1 = q == m ? B : A + d * (q / (float)m) + n * ((Hs(s.seed + k * 31 + fr, q) - 0.5f) * 12);
                    float L2 = (p1 - p0).magnitude; int st = Mathf.Max(1, (int)(L2 / 3));
                    for (int j = 0; j < st; j++)
                    {
                        Vector2 qq = p0 + (p1 - p0) * (j / (float)st); if (Hs(s.seed + (int)qq.x, (int)qq.y) >= keep) continue;
                        if (u < 0.5f) Cell(j % 2 == 0, qq + n * 3, Sk);
                        Cell(true, qq, u < 0.55f ? Wh : Sk);
                    }
                    if (q == 2 || q == m - 1)
                    {
                        Vector2 fd = (p1 - p0).normalized, fv = Rot(fd, Hs(s.seed + fr, q - 1) < 0.5f ? 0.9f : -0.9f);
                        for (int j = 1; j < 4; j++) if (Hs(s.seed + j, q - 1 + k) < keep) Cell(true, p1 + fv * j * 4, j < 3 ? Sk : Sv);
                    }
                    p0 = p1;
                }
            }
            return a < (s.n - 1) * hop + 0.36f;
        }

        /* ---------- v0.0.46: ORIGINAL 187 pillar / 188 whip, kept for HeroSkillFX.ClassicPillar / ClassicWhip ---------- */
        bool PillarV0(ref It s, float a)
        {
            Vector2 c = s.a; float r = s.r, tell = HeroSkillFX.PillarTellS, t = _clock;
            if (a < tell)
            {
                float u = a / tell; int L = Q3(3 + 9 * Craft.EaseBack(Mathf.Min(1, u / 0.6f))); float rot = u > 0.55f ? Mathf.PI / 4 : 0;
                Cell(true, c, Wh);
                for (int j = 0; j < 4; j++) { float an = rot + j * Mathf.PI / 2; for (int m = 3; m <= L; m += 3) Cell(true, c + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.8f) * m, m < L ? Gd : Am); }
                for (int j = 0; j < 6; j++)
                {
                    float an = j * Craft.Two / 6 + u * 2.6f, rr = r * Mathf.Pow(1 - u, 1.2f) + 4; Vector2 q = c + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.6f) * rr;
                    Cell(true, q, j % 2 == 1 ? Gd : Am); Cell(false, q - new Vector2(Mathf.Cos(an + 0.5f), Mathf.Sin(an + 0.5f) * 0.6f) * 4, Or);
                }
                return true;
            }
            float e = a - tell;
            if ((s.flags & 1) == 0) { s.flags |= 1; _deb.Spawn(CraftDebris.Chip, c, 10, 130, 130, Gd, Am, Wh, Or, s.seed, 0.004f, 0.6f); _deb.Spawn(CraftDebris.Ember, c + new Vector2(0, 6), 8, 20, 40, Am, Or, Gd, Am, s.seed + 3, 0.03f, 0.9f); }
            if (e < 0.067f) Craft.Impact(this, c, r * 0.8f, e, s.seed, Wh, Or);
            if (e < 1.1f) Craft.DecalScorch(this, c, r * 0.7f, e, 1.1f, s.seed, Sv, Am);
            if (e < 0.06f) { Craft.Blob(this, true, c + new Vector2(0, 3), r * 0.75f, Or, Am, Gd, Gd, 3, s.seed, 0.2f, 1.5f, 0.5f); return true; }
            if (e > 0.62f) return e < 1.1f;
            int h = Q3(18 + 52 * Craft.EaseBack(Mathf.Min(1, (e - 0.06f) / 0.12f))); float w = r * 0.55f, left = 0.62f - e;
            if (left < 0.25f) { int k = (int)((0.25f - left) / 0.06f) + 1; w = Mathf.Max(4, w - 3 * k); h = Mathf.Max(9, h - 12 * k); }
            Craft.ShadowBlob(this, c, w * 1.2f, 110, s.seed, 0.2f, 0.5f);
            for (int yy = 0; yy < h; yy += 6)
            {
                float f = yy / (float)Mathf.Max(1, h); int sw = Q3(Mathf.Sin(yy * 0.15f + t * 14) * 2 * f);
                Craft.Blob(this, true, c + new Vector2(sw, yy + 3), Mathf.Max(3, w * (1 - 0.45f * f)), Or, Am, Gd, Gd, 3, s.seed + yy + Mathf.FloorToInt(t * 15) % 3, 0.25f, 1f, 0.9f);
            }
            if (e < 0.32f) for (int yy = 3; yy < h - 3; yy += 3) Cell(true, c + new Vector2(0, yy + 3), Wh);
            for (int j = 0; j < 3; j++) Craft.Tongue(this, true, c.x + (j - 1) * w * 0.5f, c.y + h - 3, 10 + 4 * Hs(s.seed, j), Mathf.Max(6, w * 0.7f), Or, Am, s.seed + j * 11, t, (j - 1) * 0.3f);
            for (int j = 0; j < 5; j++) { float an = j * Craft.Two / 5 + 0.3f; Craft.Tongue(this, false, c.x + Mathf.Cos(an) * w * 1.1f, c.y + Mathf.Sin(an) * w * 0.5f, 7, 6, Or, Am, s.seed + 40 + j, t, Mathf.Cos(an) * 0.4f); }
            return true;
        }

        /* ---------- 188 blood whip ---------- */
        bool WhipV0(ref It s, int i, float a)
        {
            if (a > 0.6f) return false;
            Vector2 d = s.d, n = Perp(d), o = HeroSkillFX.Hero * HellFX.PPU + new Vector2(0, 10) + d * 10; int po = i * 64;
            for (int q = 0; q < s.n; q++) if (Hs(s.seed + (int)_prev[po + q].x, (int)_prev[po + q].y) < 0.55f) Cell(false, _prev[po + q], Sv);   // silver swoosh afterimage
            if (a < 0.22f)
            {
                float u, s0 = 0; if (a < 0.09f) u = Mathf.Min(1.08f, Craft.EaseBack(a / 0.09f)); else if (a < 0.13f) u = 1f; else { float v = Mathf.Min(1, (a - 0.13f) / 0.09f); u = 1 - v * 0.15f; s0 = s.r * v; }
                float amp = 10 * Mathf.Max(0, 1 - a / 0.16f) + 2; int idx = 0, np = 0; Vector2 tip = o;
                for (float sv = s0; sv < s.r * u; sv += 3f, idx++)
                {
                    float f = sv / s.r; Vector2 p = o + d * sv + n * (amp * Mathf.Sin(f * Mathf.PI * 2.2f - a * 40) * f); tip = p;
                    Cell(true, p, Wn); if (f < 0.35f) Cell(true, p - n * 3, Wn); if (f < 0.8f) Cell(true, p + n * 3, Rs);
                    if (idx % 5 == 2 && f > 0.2f) Cell(true, p + n * 6, Rs);
                    if (idx % 2 == 0 && np < 64) _prev[po + np++] = p - n * 4;
                }
                s.n = np; if (idx > 0) { Cell(true, tip, Cr); Cell(true, tip - d * 3, Cr); }
            }
            else s.n = 0;
            Vector2 tp = HeroSkillFX.Hero * HellFX.PPU + new Vector2(0, 10) + d * (s.r + 10);
            if (a >= 0.09f && (s.flags & 1) == 0) { s.flags |= 1; _deb.Spawn(CraftDebris.Chip, tp, 5, 90, 80, Rs, Cr, Wn, Rs, s.seed, 0, 0.45f); }
            if (a >= 0.09f && a < 0.155f) Craft.Impact(this, tp, 9, a - 0.09f, s.seed, Cr, Wn);
            return true;
        }
        void WhipFxV0(float dt)
        {
            Vector2 hero = HeroSkillFX.Hero * HellFX.PPU;
            for (int j = _nSlash - 1; j >= 0; j--)
            {
                Vector3 sl = _slash[j]; float a = sl.z; Vector2 p = sl;
                if (a < 0.12f) for (int x = -1; x <= 1; x += 2) { Cell(true, p + new Vector2(x * 3, 3), a < 0.05f ? Cr : Rs); Cell(true, p + new Vector2(-x * 3, -3), Rs); }
                sl.z += dt; _slash[j] = sl; if (sl.z > 0.2f) _slash[j] = _slash[--_nSlash];
            }
            for (int j = _nMote - 1; j >= 0; j--)
            {
                Vector2 p0 = _motePos[j]; float u = Mathf.Min(1, _mote[j].x / 0.35f); Vector2 tgt = hero + new Vector2(0, 12);
                Vector2 q = p0 + (tgt - p0) * (u * u) + new Vector2(0, Mathf.Sin(u * Mathf.PI) * 10);
                Cell(true, q, Cr); Cell(true, q - (tgt - p0).normalized * 3, Rs);
                _mote[j].x += dt;
                if (u >= 1) { if (_nPop < _healPop.Length) _healPop[_nPop++] = 0; _mote[j] = _mote[--_nMote]; _motePos[j] = _motePos[_nMote]; }
            }
            for (int j = _nPop - 1; j >= 0; j--)
            {
                float a = _healPop[j]; Vector2 c = hero + new Vector2(9, 22 + a * 30);
                Cell(true, c, Cr); Cell(true, c + new Vector2(3, 0), Rs); Cell(true, c - new Vector2(3, 0), Rs); Cell(true, c + new Vector2(0, 3), Rs); Cell(true, c - new Vector2(0, 3), Rs);
                _healPop[j] += dt; if (_healPop[j] > 0.18f) _healPop[j] = _healPop[--_nPop];
            }
        }

        /* ---------- fx_redo Task 4 helpers (mirrors mock/fx_v3.py) ---------- */
        static readonly Color32 Dk = C(0x3f2832), Sm1 = C(0x8b7f9a), Sm2 = C(0x5a4e68), Sm3 = C(0x3a3046), Ck = C(0x2b1d24), Pb = C(0x8b9bb4);
        static int Rf(float v) => Mathf.FloorToInt(v + 0.5f);
        /// light sources (fire, sparks): opaque, NO dark outline
        void Glow(bool top, int x, int y, int w, int h, Color32 c)
        {
            if (w <= 0 || h <= 0) return;
            if (top) { if (_ntf < MaxQ) _tf[_ntf++] = new Q { x = x, y = y, w = w, h = h, c = c }; }
            else if (_ngf < MaxQ) _gf[_ngf++] = new Q { x = x, y = y, w = w, h = h, c = c };
        }
        /// tinted alpha floor light (drawn after shadows, before ground art)
        void Light(int x, int y, int w, int h, Color32 c, float a) { if (w > 0 && h > 0 && _ngl < MaxQ) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(a * 255), 0, 255); _gl[_ngl++] = new Q { x = x, y = y, w = w, h = h, c = c }; } }
        static float Vn(int s, float x, float y)
        {
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y); float fx = x - ix, fy = y - iy; fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            float a = Hs(s + ix * 7919, iy), b = Hs(s + (ix + 1) * 7919, iy), c = Hs(s + ix * 7919, iy + 1), d = Hs(s + (ix + 1) * 7919, iy + 1);
            return a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy;
        }
        static float Eb(float u) { u = Mathf.Clamp01(u) - 1; return 1 + 2.70158f * u * u * u + 1.70158f * u * u; }
        void Ln(bool top, Vector2 a, Vector2 b, Color32 c, int w = 1, bool glow = false)
        {
            float dx = b.x - a.x, dy = b.y - a.y; int n = Mathf.Max(1, (int)Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy))), o = -(w / 2);
            for (int i = 0; i <= n; i++) { int x = Rf(a.x + dx * i / n) + o, y = Rf(a.y + dy * i / n) + o; if (glow) Glow(top, x, y, w, w, c); else Box(top, x, y, w, w, c); }
        }
        void Disc(bool top, float cx, float cy, float r, Color32 col, bool hasRim, Color32 rim, bool glow, float sy = 1f)
        {
            int R = Mathf.CeilToInt(r * sy);
            for (int yy = -R; yy <= R; yy++)
            {
                float t = yy / sy, hw = Mathf.Sqrt(Mathf.Max(0, r * r - t * t)); int x0 = Rf(cx - hw), x1 = Rf(cx + hw); if (x1 < x0) continue; int y = Rf(cy) + yy;
                if (hasRim && x1 - x0 >= 2 && Mathf.Abs(yy) < R) { E(top, x0, y, 1, 1, rim, glow); E(top, x1, y, 1, 1, rim, glow); E(top, x0 + 1, y, x1 - x0 - 1, 1, col, glow); }
                else E(top, x0, y, x1 - x0 + 1, 1, hasRim ? rim : col, glow);
            }
        }
        void E(bool top, int x, int y, int w, int h, Color32 c, bool glow) { if (glow) Glow(top, x, y, w, h, c); else Box(top, x, y, w, h, c); }
        void RingLight(float cx, float cy, float r, float sy, Color32 col, float a)
        {
            int R = (int)(r * sy);
            for (int yy = -R; yy <= R; yy++) { float t = yy / sy, hw = Mathf.Sqrt(Mathf.Max(0, r * r - t * t)); Light(Rf(cx - hw), Rf(cy) + yy, Rf(2 * hw) + 1, 1, col, a); }
        }
        void ShadeEllipse(float cx, float cy, float rr, int a, int jitterSeed = -1)
        {
            int R = (int)(rr * 0.55f);
            for (int yy = -R; yy <= R; yy++)
            {
                float t = yy / 0.55f, hw = Mathf.Sqrt(Mathf.Max(0, rr * rr - t * t)); if (jitterSeed >= 0) hw *= 0.9f + 0.2f * Hs(jitterSeed, yy);
                Shadow(Rf(cx - hw), Rf(cy) + yy, Rf(2 * hw) + 1, 1, (byte)Mathf.Clamp(a, 0, 255));
            }
        }

        /* ---------- 187 hellfire pillar (Task 4 remake) ----------
           tell: dark contrast disc + ground cracks glowing FR->OR->AM/GD + floor light + hopping pebbles
           -> 1 frame white SQUASH dome -> thin over-tall STRETCH spike + shock ring -> settle (overshoot) into a ROARING column of 1 px rows
           (value-noise heat ramp white/gold/amber/orange/red, #3f2832 outer ring, no black outline, noise gaps up top, travelling lobes + sway)
           with licking side tongues, detached rising chunks, 1 px embers, falling sparks, violet-grey smoke behind -> COLLAPSE (shrinks + cools)
           -> smoke + scorch decal whose cracks cool OR->FR->dark. Life = tell + 2.2 s. */
        const float PillarLife = 2.2f;
        readonly Vector2[] _ck = new Vector2[14 * 5]; readonly int[] _ckn = new int[14]; int _nck;
        void PillarCracks(int sd, float r)
        {
            _nck = 0; const int n = 7;
            for (int k = 0; k < n; k++)
            {
                float an = k * Craft1.Two / n + (Hs(sd, k) - 0.5f) * 0.7f, L = r * (0.75f + 0.5f * Hs(sd + 1, k)); int o = _nck * 5; _ck[o] = Vector2.zero;
                for (int j = 1; j < 5; j++) { float u = j / 4f, aa = an + (Hs(sd + 2, k * 9 + j) - 0.5f) * 0.5f; _ck[o + j] = new Vector2(Mathf.Cos(aa) * L * u, Mathf.Sin(aa) * L * u * 0.55f); }
                _ckn[_nck++] = 5;
                if (Hs(sd + 3, k) < 0.6f)
                {
                    Vector2 b = _ck[o + 2]; float ba = an + (Hs(sd + 4, k) < 0.5f ? 0.7f : -0.7f), bl = L * 0.35f; int q = _nck * 5;
                    _ck[q] = b; _ck[q + 1] = b + new Vector2(Mathf.Cos(ba) * bl * 0.5f, Mathf.Sin(ba) * bl * 0.28f); _ck[q + 2] = b + new Vector2(Mathf.Cos(ba) * bl, Mathf.Sin(ba) * bl * 0.55f); _ckn[_nck++] = 3;
                }
            }
        }
        void DrawCracks(Vector2 c, float g, Color32 col, bool hasCore, Color32 core)
        {
            for (int p = 0; p < _nck; p++)
            {
                int o = p * 5, tot = _ckn[p] - 1; float m = g * tot;
                for (int j = 0; j < tot; j++)
                {
                    if (j >= m) break; Vector2 a = _ck[o + j], b = _ck[o + j + 1]; float f = Mathf.Min(1, m - j); b = a + (b - a) * f;
                    Ln(false, c + a, c + b, j > 0 || !hasCore ? col : core);
                }
            }
        }
        void PillarPool(Vector2 c, float r, float k) { if (k <= 0.02f) return; float R = r * 1.55f; RingLight(c.x, c.y, R, 0.55f, Or, 0.10f * k); RingLight(c.x, c.y, R * 0.72f, 0.55f, Or, 0.16f * k); RingLight(c.x, c.y, R * 0.45f, 0.55f, Am, 0.20f * k); }
        float PillarColumn(int sd, float cx, float by, float hw0, float h, float bias, float tq, float wsc)
        {
            int rows = (int)h; float c = cx;
            for (int yy = 0; yy < rows; yy++)
            {
                float f = yy / Mathf.Max(1, h), base_ = hw0 * wsc * Mathf.Pow(1 - f, 0.62f) * (1 + 0.12f * (1 - f)) * (yy < 4 ? 0.72f + 0.07f * yy : 1);
                float hl = base_ * (1 + 0.20f * Mathf.Sin(yy * 0.27f - tq * 15 + sd) * (0.35f + f) + 0.09f * Mathf.Sin(yy * 0.63f - tq * 26 + sd * 2));
                float hr = base_ * (1 + 0.20f * Mathf.Sin(yy * 0.27f - tq * 15 + sd + 2.2f) * (0.35f + f) + 0.09f * Mathf.Sin(yy * 0.63f - tq * 26 + sd * 2 + 2.2f));
                float sway = Mathf.Pow(f, 1.6f) * (4.5f * Mathf.Sin(tq * 4.1f + sd) + 2.2f * Mathf.Sin(yy * 0.11f - tq * 8));
                c = cx + sway; int x0 = Rf(c - hl), x1 = Rf(c + hr); if (x1 < x0) continue;
                int y = Rf(by + yy), runX = 0; bool open = false; Color32 runC = Wh;
                for (int x = x0; x <= x1 + 1; x++)
                {
                    bool has = false; Color32 col = Wh;
                    if (x <= x1)
                    {
                        float d = x < c ? (c - x) / Mathf.Max(0.8f, hl) : (x - c) / Mathf.Max(0.8f, hr);
                        bool gap = f > 0.42f && d > 0.25f && Vn(sd + 5, x / 3.5f, (yy - tq * 95) / 4.5f) > 0.74f - 0.25f * (f - 0.42f);
                        if (!gap)
                        {
                            has = true; float heat = (1 - Mathf.Pow(Mathf.Min(1, d), 1.6f)) * (1 - 0.78f * f) + 0.42f * (Vn(sd, x / 5f, (yy - tq * 70) / 6f) - 0.5f) + bias;
                            if (yy < 3 && d < 0.3f && bias > -0.2f) heat = 1;
                            col = d > 0.84f || heat < 0.08f ? Dk : heat > 0.93f ? Wh : heat > 0.64f ? Gd : heat > 0.44f ? Am : heat > 0.25f ? Or : Fr;
                        }
                    }
                    if (open && (!has || !Same(col, runC))) { Glow(true, runX, y, x - runX, 1, runC); open = false; }
                    if (has && !open) { runX = x; runC = col; open = true; }
                }
            }
            return c;
        }
        bool Pillar(ref It s, float a)
        {
            Vector2 c = s.a; float cx = c.x, cy = c.y, r = s.r, hw0 = r * 0.62f, Hc = 56 + r * 0.9f, tell = HeroSkillFX.PillarTellS, t = _clock; int sd = s.seed;
            float tq = Mathf.Floor(t * 15) / 15f; PillarCracks(sd, r);
            if (a < tell)
            {
                float u = a / tell, g = Eb(Mathf.Min(1, u / 0.7f));
                if (!HeroSkillFX.PillarGpuGround) for (int k = 0; k < 3; k++) ShadeEllipse(cx, cy, r * (0.5f + 0.35f * u) * (1 - k * 0.25f), (int)(35 + 35 * u));
                PillarPool(c, r, 0.25f + 0.5f * u * u);
                bool pulse = ((int)(a * 15)) % 2 == 0;
                Color32 col = u < 0.3f ? Fr : u < 0.62f ? Or : u < 0.86f ? (pulse ? Am : Or) : Gd;
                if (!HeroSkillFX.PillarGpuGround) DrawCracks(c, g, col, u > 0.5f, Gd);
                if (u > 0.86f && !HeroSkillFX.PillarGpuGround) Glow(false, Rf(cx) - 1, Rf(cy) - 1, 3, 2, Wh);
                for (int j = 0; j < 5; j++)
                {
                    float ph = (a * 3 + Hs(sd + 6, j)) % 1f;
                    if (u > 0.35f) { int p = j % _nck; Vector2 P = _ck[p * 5 + _ckn[p] - 1]; Box(true, Rf(cx + P.x * g * 0.8f), Rf(cy + P.y * g * 0.8f + ph * 14 * u), 1, 1, ph < 0.5f ? Am : Or); }
                }
                if (u > 0.55f) for (int j = 0; j < 3; j++) { float hop = Mathf.Abs(Mathf.Sin(a * 30 + j * 2)) * 3; Box(false, Rf(cx + (j - 1) * r * 0.4f), Rf(cy - 3 + j * 2 + hop), 1, 1, Pb); }
                return true;
            }
            float e = a - tell; if (e > PillarLife) return false;
            if ((s.flags & 1) == 0) { s.flags |= 1; _deb1.Spawn(CraftDebris1.Chip, c, 8, 120, 120, Gd, Am, Wh, Or, sd, 0.004f, 0.6f); _deb1.Spawn(CraftDebris1.Chip, c, 10, 110, 110, HeroSkillFX.PillarChipA, HeroSkillFX.PillarChipB, HeroSkillFX.PillarChipC, HeroSkillFX.PillarChipD, sd + 3, 0.004f, 0.6f); }
            float sk = e < 1.4f ? 1 : Mathf.Max(0, 1 - (e - 1.4f) / 0.8f);   // scorch decal, cracks cool down
            if (sk > 0 && !HeroSkillFX.PillarGpuGround)
            {
                ShadeEllipse(cx, cy, r * 0.78f, (int)(120 * sk), sd + 7);
                if (e > 0.85f && e < 2.1f) DrawCracks(c, 0.85f, e < 1.15f ? Or : e < 1.5f ? Fr : e < 1.9f ? Dk : Ck, false, Wh);
            }
            int ph2 = e < 0.034f ? 0 : e < 0.10f ? 1 : e < 0.78f ? 2 : e < 1.02f ? 3 : 4;   // flash, stretch, sustain, collapse, smoke
            float inten = ph2 == 0 ? 1.4f : ph2 == 1 ? 1.3f : ph2 == 2 ? 1f : ph2 == 3 ? 1 - (e - 0.78f) / 0.24f * 0.8f : Mathf.Max(0, 0.2f - (e - 1.02f));
            PillarPool(c, r, inten * (0.9f + 0.2f * Hs(sd + 8, (int)(t * 15))));
            // t934u: the CPU ground shock ring is gone (VH batch6): the game spawns a PixelShock934 ripple 0.6->1.4 r over 0.16 s here.
            if (e >= 0.12f && e < 1.9f)   // lobed violet-grey smoke puffs, behind the flame: rise, shrink, split
                for (int j = 0; j < 9; j++)
                {
                    const float P = 0.75f; float q = (e - 0.12f) / P + Hs(sd + 25, j); int gen = (int)q; float u = q % 1f, born = 0.12f + (gen - Hs(sd + 25, j)) * P;
                    if (born > 1.15f || born < 0.12f) continue; bool late = born > 0.8f;
                    float hb = late ? 8 + Hc * 0.25f : Hc * 0.8f, x = cx + (Hs(sd + 26, gen * 9 + j) - 0.5f) * hw0 * (late ? 1.8f : 1.1f) + u * 8 * (Hs(sd + 27, j) - 0.5f);
                    float y = cy + hb + u * (late ? 30 : 40), rr = (1.8f + 2.6f * Hs(sd + 28, j)) * (1.2f - u * 0.5f) * (u < 0.7f ? 1 : (1 - u) / 0.3f);
                    if (rr < 0.8f) continue; int nl = u < 0.55f ? 3 : 2;
                    for (int lb = 0; lb < nl; lb++) { float ox = (lb - (nl - 1) / 2f) * rr * (u < 0.55f ? 1f : 1.9f), oy = (lb == 1 && nl == 3 ? 0.6f : 0) * rr; Disc(true, x + ox, y + oy, rr * (lb != 1 ? 0.8f : 1), u > 0.5f ? Sm3 : Sm2, false, Wh, false); }
                    if (rr > 1.6f) for (int lb = 0; lb < nl; lb++) { float ox = (lb - (nl - 1) / 2f) * rr * (u < 0.55f ? 1f : 1.9f), oy = (lb == 1 && nl == 3 ? 0.6f : 0) * rr; Disc(true, x + ox - rr * 0.15f, y + oy + rr * 0.3f, rr * 0.45f, u < 0.5f ? Sm1 : Sm2, false, Wh, true); }
                }
            if (ph2 == 0) { Disc(true, cx, cy + 3, hw0 * 1.7f, Wh, true, Gd, true, 0.38f); Disc(true, cx, cy + 3, hw0 * 1.2f, Wh, false, Wh, true, 0.30f); }   // SQUASH
            else if (ph2 == 1) { float u = (e - 0.034f) / 0.066f; PillarColumn(sd, cx, cy + 1, hw0, Hc * (1 + 0.3f * Eb(u)), 0.38f, tq, 0.5f); }   // STRETCH
            else if (ph2 == 2 || ph2 == 3)
            {
                float wsc, hs, bias;
                if (ph2 == 2) { float u = Mathf.Min(1, (e - 0.10f) / 0.10f); wsc = 0.5f + 0.5f * Eb(u); hs = 1.3f - 0.3f * Eb(u); bias = 0.3f * (1 - u) + 0.05f * Mathf.Sin(t * 9); }
                else { float v = (e - 0.78f) / 0.24f; wsc = 1 - 0.45f * v; hs = Mathf.Max(0.05f, Mathf.Pow(1 - v, 1.4f)); bias = -0.38f * v; }
                PillarColumn(sd, cx, cy + 1, hw0, Hc * hs * (0.94f + 0.08f * Hs(sd + 10, (int)(t * 15))), bias, tq, wsc);
                for (int j = 0; j < 4; j++)   // licking side tongues at the base
                {
                    float sg = j % 2 == 0 ? -1 : 1, pp = (t * 2.7f + Hs(sd + 11, j)) % 1f, bx = cx + sg * hw0 * wsc * 0.75f, th = (6 + 6 * Hs(sd + 12, j)) * Mathf.Sin(pp * Mathf.PI) * hs;
                    for (int yy = 0; yy < (int)th; yy++) { float f = yy / Mathf.Max(1, th); int w = Mathf.Max(0, Rf(2.2f * (1 - f))), x = Rf(bx + sg * f * f * 4); Glow(true, x - w / 2, Rf(cy + 1 + yy), w + 1, 1, f < 0.3f ? Am : f < 0.7f ? Or : Fr); }
                }
            }
            if (ph2 >= 1)   // detached flame chunks rising off the tip
                for (int j = 0; j < 6; j++)
                {
                    const float P = 0.42f; float q = e / P + Hs(sd + 13, j); int gen = (int)q; float u = q % 1f, born = (gen - Hs(sd + 13, j)) * P;
                    if (born < 0.05f || born > 0.95f) continue;
                    float x = cx + (Hs(sd + 15, gen * 7 + j) - 0.5f) * hw0 * 0.9f + Mathf.Sin(u * 5 + j) * 2.5f, y = cy + Hc * (0.55f + 0.3f * Hs(sd + 14, gen * 7 + j)) + u * (30 + 12 * Hs(sd + 16, j));
                    Disc(true, x, y, 3.2f * (1 - u) + 0.4f, u < 0.3f ? Am : u < 0.6f ? Or : Fr, true, u < 0.3f ? Or : u < 0.6f ? Fr : Dk, true);
                }
            if (ph2 >= 2 || e < 1.3f)   // embers: 1 px, rise, sway, cool, blink
                for (int j = 0; j < 16; j++)
                {
                    float P = 0.7f + 0.4f * Hs(sd + 17, j), q = e / P + Hs(sd + 18, j); int gen = (int)q; float u = q % 1f, born = (gen - Hs(sd + 18, j)) * P;
                    if (born < 0.04f || born > 1f || Hs(sd + 19, j * 31 + (int)(t * 30)) < 0.12f) continue;
                    int x = Rf(cx + (Hs(sd + 20, gen * 16 + j) - 0.5f) * hw0 * 1.8f + Mathf.Sin(u * 7 + j) * 3.5f), y = Rf(cy + Hc * 0.25f + u * Hc);
                    Glow(true, x, y, 1, 1, u < 0.25f ? Gd : u < 0.5f ? Am : u < 0.75f ? Or : Fr); if (u < 0.3f) Glow(true, x, y - 1, 1, 1, Or);
                }
            if (e >= 0.034f && e < 0.75f)   // falling sparks with gravity
                for (int j = 0; j < 10; j++)
                {
                    float tt = e - 0.034f - Hs(sd + 21, j) * 0.08f; if (tt < 0 || tt > 0.55f) continue;
                    float vx = (Hs(sd + 22, j) - 0.5f) * 150, vy = 60 + 90 * Hs(sd + 23, j), y0 = cy + Hc * (0.6f + 0.5f * Hs(sd + 24, j));
                    float x = cx + vx * tt, y = y0 + vy * tt - 330 * tt * tt; if (y < cy - r * 0.4f) continue;
                    float t2 = tt - 0.02f; Vector2 q2 = new Vector2(cx + vx * t2, y0 + vy * t2 - 330 * t2 * t2);
                    Ln(true, q2, new Vector2(x, y), tt > 0.2f ? Or : Gd, 1, true); Glow(true, Rf(x), Rf(y), 1, 1, tt < 0.12f ? Wh : tt < 0.25f ? Gd : tt < 0.4f ? Am : Or);
                }
            return true;
        }

        static bool Same(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b;

        /* 188 WHIP v5 (fx_redo Task 8): OCTOPUS TENTACLE. v4 C1 Hermite keyframes + eased uncurl + sin^2 travelling wave, retimed: slap at 0.16 s.
            28 links tapering 5 -> 1 px: dark violet #68386c top, magenta #b55088 body, pale #e8b796 underside, #fff4dc sucker row (2 px near base, 1 px out),
            1-2 px white wet glint running base -> tip; tip spirals (curl) at rest and on recoil. Impact = wet slap: flat pale splat flash (~22x12, white core),
            10-streak splash crown r5 -> r13 (~26 px), ink/slime puddle (#262b44 / #3a4466), 10 slime + ink droplets that splat. No red; Ko backing outline.
            State: _pts[i*MaxPts] = slap point, flags bit 1 = slapped, s.life = slap age. */
        static readonly Color32 Dc = C(0x8a1040), Cm = C(0xc8205a), Rb = C(0xe0457a), Lh = C(0x5a2a1a);
        static Color32 Tt = C(0x68386c), Tb = C(0xb55088), Tu = C(0xe8b796), Tk = C(0xfff4dc), Ik = C(0x262b44), Il = C(0x3a4466), Th = C(0x3e2731);
        static Color32 WHi = C(0xffffff), WCr = C(0xfff4dc); /* Hellord t948u: whip glint / hit-flash colours (white by default, blood-dark when SetWhipDark) */
        static bool _bloodSet, _darkSet; static int _wDk = 0x3e2731, _wMd = 0xa22633, _wHi = 0xe43b44;
        static Color32 WDk = C(0x3e2731), WMd = C(0xa22633); /* Hellord t957u: whip-only hit chips / droplets / heal motes (were pink Rs / cream Cr / crimson Cm) */
        /// <summary>Hellord t948u: whip glints + hit flashes in dark blood red (Endesga 32) instead of white/cream.</summary>
        internal static void SetWhipDark(bool on) { if (_darkSet == on) return; _darkSet = on; ApplyWhip(); }
        /// <summary>Hellord t934u (VH approved batch 4): blood-red tentacle palette (Endesga 32). false = original magenta/violet.</summary>
        internal static void SetBloodRed(bool on) { if (_bloodSet == on) return; _bloodSet = on; ApplyWhip(); }
        /// <summary>Hellord t957u: the 3 blood colours (dark, mid, highlight) every whip draw uses when blood + dark are on. Idempotent.</summary>
        /// <summary>Hellord t965u: whip value roles (0xRRGGBB): outline, body, underside band, lash edge, glint, hit-flash core/corners, heal-mote core/trail.</summary>
        internal static void SetWhipRoles(bool on, int ol, int bd, int un, int ed, int gl, int fc, int fk, int mc, int mt) { if (_wr.Length == 10 && _wr[0] == (on ? 1 : 0) && _wr[1] == ol && _wr[2] == bd && _wr[3] == un && _wr[4] == ed && _wr[5] == gl && _wr[6] == fc && _wr[7] == fk && _wr[8] == mc && _wr[9] == mt) return; _wr = new[] { on ? 1 : 0, ol, bd, un, ed, gl, fc, fk, mc, mt }; ApplyWhip(); }
        static int[] _wr = { 0 }; static Color32 WFc = C(0xffffff), WFk = C(0xfff4dc), WMc = C(0xfff4dc), WMt = C(0xf6757a);
        internal static void SetWhipColors(int dk, int md, int hi) { if (dk == _wDk && md == _wMd && hi == _wHi) return; _wDk = dk; _wMd = md; _wHi = hi; ApplyWhip(); }
        /* t957u: the old toggle code (_bloodSet = !_bloodSet + recursive SetBloodRed) could leave the magenta/cream palette live; now one pure function of the flags. */
        /* t957u ROOT CAUSE: the project renders in Linear colour space and Sprites/Default treats vertex colours as linear, so sRGB hex
           colours come out gamma-brightened (#a22633 -> ~#d06e80, #e43b44 -> ~#f2a8c0 pink). CL() converts the hex to linear so the
           whip shows the exact Endesga value on screen. */
        static Color32 CL(int h) { Color c = C(h); return QualitySettings.activeColorSpace == ColorSpace.Linear ? (Color32)c.linear : (Color32)c; }
        static void ApplyWhip()
        {
            if (_bloodSet && _darkSet && _wr[0] == 1) { /* t965u: value contrast within Endesga instead of 3 flat reds */
                Tt = CL(_wr[1]); Th = CL(_wr[1]); Tb = CL(_wr[2]); Tu = CL(_wr[3]); Tk = CL(_wr[4]); Ik = CL(_wr[1]); Il = CL(_wr[2]);
                WHi = CL(_wr[5]); WCr = CL(_wr[4]); WDk = CL(_wr[1]); WMd = CL(_wr[3]); WFc = CL(_wr[6]); WFk = CL(_wr[7]); WMc = CL(_wr[8]); WMt = CL(_wr[9]); return; }
            if (_bloodSet && _darkSet) { WFc = CL(_wHi); WFk = CL(_wMd); WMc = CL(_wHi); WMt = CL(_wMd); Tt = CL(_wDk); Tb = CL(_wMd); Tu = CL(_wHi); Tk = CL(_wDk); Ik = CL(_wDk); Il = CL(_wMd); Th = CL(0x181425); WHi = CL(_wHi); WCr = CL(_wMd); WDk = CL(_wDk); WMd = CL(_wMd); return; }
            if (_bloodSet) { Tt = C(0x3e2731); Tb = C(0xa22633); Tu = C(0xe43b44); Tk = C(0xf6757a); Ik = C(0x3e2731); Il = C(0x732e3a); Th = C(0x181425); }
            else { Tt = C(0x68386c); Tb = C(0xb55088); Tu = C(0xe8b796); Tk = C(0xfff4dc); Ik = C(0x262b44); Il = C(0x3a4466); Th = C(0x3e2731); }
            WHi = C(0xffffff); WCr = C(0xfff4dc); WDk = Wn; WMd = Cm;
        }
        const int WK = 28; const float WCrack = 0.16f, WLife = 0.62f;
        readonly Vector2[] _wP = new Vector2[WK + 1], _wQ = new Vector2[WK + 1];
        static readonly float[] WKt = { 0f, 0.055f, 0.09f, 0.125f, 0.16f, 0.22f, 0.33f, 0.45f }, WKa = { 2.30f, 2.75f, 1.20f, -0.22f, 0f, 0f, 0.30f, 0.35f }, WKe = { 0.30f, 0.40f, 0.80f, 1.05f, 1f, 1f, 0.75f, 0.30f };
        static float WSl(float[] v, int i)
        {
            int a = Mathf.Max(0, i - 1), b = Mathf.Min(v.Length - 1, i + 1);
            if (v[i] == v[a] || v[i] == v[b]) return 0f;
            return (v[b] - v[a]) / (WKt[b] - WKt[a]);
        }
        static float WHerm(float[] v, float tau)
        {
            int n = WKt.Length; if (tau <= WKt[0]) return v[0]; if (tau >= WKt[n - 1]) return v[n - 1];
            int j = 0; while (j < n - 2 && tau > WKt[j + 1]) j++;
            float t0 = WKt[j], t1 = WKt[j + 1], h = t1 - t0, u = (tau - t0) / h, u2 = u * u, u3 = u2 * u;
            return (2 * u3 - 3 * u2 + 1) * v[j] + (u3 - 2 * u2 + u) * WSl(v, j) * h + (-2 * u3 + 3 * u2) * v[j + 1] + (u3 - u2) * WSl(v, j + 1) * h;
        }
        static void WBase(float tau, out float da, out float ext) { da = WHerm(WKa, tau); ext = WHerm(WKe, tau); }
        static int WWid(int k) => k < 5 ? 5 : k < 11 ? 4 : k < 17 ? 3 : k < 23 ? 2 : 1;
        static float WCurl(float a) { float c0 = 1 - Mathf.Clamp01((a - 0.04f) / 0.08f), c1 = Mathf.Clamp01((a - 0.22f) / 0.15f); c1 = c1 * c1 * (3 - 2 * c1); return Mathf.Max(c0, c1); }
        static void WCurve(Vector2[] P, Vector2 o, float th, float sg, float L, float a)
        {
            P[0] = o; Vector2 p = o; WBase(a, out _, out float ext); float ds = L * ext / WK, cu = WCurl(a);
            float rec = Mathf.Clamp01((a - 0.22f) / 0.23f); rec = rec * rec * (3 - 2 * rec);
            float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01((a - 0.045f) / 0.13f)); env *= env;
            for (int k = 0; k < WK; k++)
            {
                float sk = (k + 0.5f) / WK, lag = 0.04f * (1 - (1 - sk) * (1 - sk)); WBase(a - lag, out float da, out _);
                float slack = -1.1f * rec * sk * sk * Mathf.Sin(rec * Mathf.PI * 0.85f + 0.25f), wv = 0.34f * Mathf.Sin(sk * 6.3f - a * 60) * sk * env;
                float curl = -cu * 3.2f * Mathf.Pow(Mathf.Max(0f, (sk - 0.7f) / 0.3f), 1.6f);
                float phi = th + sg * (da + slack + wv + curl); p += new Vector2(Mathf.Cos(phi), Mathf.Sin(phi)) * ds; P[k + 1] = p;
            }
        }
        bool Whip(ref It s, int i, float a)
        {
            if (a > WLife) return false;
            Vector2 d = s.d, n = Perp(d), o = HeroSkillFX.Hero * HellFX.PPU + new Vector2(0, 12) + d * 5; int sd = s.seed; float sg = Hs(sd, 1) < 0.5f ? 1 : -1, th = Mathf.Atan2(d.y, d.x), L = s.r;
            WCurve(_wP, o, th, sg, L, a); var P = _wP;
            if (a >= 0.075f && a < 0.16f)
                for (int g = 0; g < 3; g++) { WCurve(_wQ, o, th, sg, L, a - 0.008f * (g + 1)); for (int k = WK / 2 + g; k <= WK; k += 3) Glow(true, Rf(_wQ[k].x), Rf(_wQ[k].y), 1, 1, g < 2 ? Tb : Tt); }
            if (a < 0.52f)
            {
                for (int k = 0; k < WK; k++) { int w = WWid(k); Vector2 up = new Vector2(0, w * 0.35f); Ln(true, P[k] + up, P[k + 1] + up, Tt, w); }
                for (int k = 0; k < WK; k++) { int w = WWid(k); Ln(true, P[k], P[k + 1], Tb, Mathf.Max(1, w - 1)); }
                for (int k = 0; k < WK - 3; k++) { int w = WWid(k); if (w < 3) continue; Vector2 dn = new Vector2(0, -(w / 2)); Ln(true, P[k] + dn, P[k + 1] + dn, Tu); }
                for (int k = 1; k < WK - 4; k += 2) { int w = WWid(k); Vector2 q = P[k] + new Vector2(0, w >= 3 ? -(w / 2) - 1 : -1); Box(true, Rf(q.x), Rf(q.y), w >= 4 ? 2 : 1, 1, Tk); }
                int gk = (int)(((a * 3.2f) % 1f) * (WK - 4));
                for (int kk = gk; kk <= gk + 1; kk++) { Vector2 q = P[kk] + new Vector2(0, WWid(kk) * 0.35f); Glow(true, Rf(q.x), Rf(q.y), 1, 1, WHi); }
                Box(true, Rf(o.x) - 2, Rf(o.y) - 2, 4, 4, Th); Box(true, Rf(o.x) - 1, Rf(o.y) - 1, 2, 2, Tt);
            }
            if (a >= WCrack && (s.flags & 1) == 0) { s.flags |= 1; s.life = a; _pts[i * MaxPts] = P[WK]; }
            if ((s.flags & 1) != 0)
            {
                Vector2 tp = _pts[i * MaxPts]; float c = a - WCrack;
                if (c < 0.045f)   // pale flat splat flash
                {
                    float gr = 0.6f + 0.4f * Mathf.Min(1, c / 0.015f), rx = 11 * gr, ry = 6 * gr;
                    for (int yy = -(int)ry; yy <= (int)ry; yy++) { float hw = rx * Mathf.Sqrt(Mathf.Max(0, 1 - (yy / ry) * (yy / ry))); Glow(true, Rf(tp.x - hw), Rf(tp.y) + yy, Mathf.Max(1, (int)(2 * hw)), 1, Mathf.Abs(yy) < ry * 0.5f ? Tk : Tu); }
                    Glow(true, Rf(tp.x) - 2, Rf(tp.y) - 1, 5, 3, WCr); Glow(true, Rf(tp.x), Rf(tp.y), 1, 1, WHi);
                }
                if (c >= 0.02f && c < 0.14f)   // splash crown r5 -> r13
                {
                    float u = (c - 0.02f) / 0.12f, r0 = 5 + 6 * u, r1 = r0 + 3 * (1 - u) + 1;
                    for (int k = 0; k < 10; k++) { if (u > 0.45f && Hs(sd + 40, k) < (u - 0.45f) * 1.8f) continue; float an = k * Mathf.PI * 0.2f + Hs(sd + 44, k) * 0.3f; Vector2 v = new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.7f); Ln(true, tp + v * r0, tp + v * r1, u < 0.4f ? Tu : Tb); }
                }
                if (c >= 0.03f && c < 0.30f)   // ink / slime puddle
                {
                    float u = (c - 0.03f) / 0.27f, r = 5 + 5 * Mathf.Min(1, u * 3);
                    for (int k = 0; k < 7; k++) { if (u >= 0.8f && k % 2 == 0) continue; float an = k * Mathf.PI * 2 / 7 + Hs(sd + 45, k), rr = r * (0.5f + 0.5f * Hs(sd + 46, k)); Box(false, Rf(tp.x + Mathf.Cos(an) * rr) - 1, Rf(tp.y + Mathf.Sin(an) * rr * 0.6f), k % 2 == 1 ? 2 : 3, 1, k % 3 != 0 ? Ik : Il); }
                }
                for (int j = 0; j < 10; j++)   // slime + ink droplets
                {
                    bool ink = j % 3 == 0; Vector2 v = d * (20 + 50 * Hs(sd + 31, j)) + n * (Hs(sd + 32, j) - 0.5f) * 130; float vz = 40 + 70 * Hs(sd + 33, j), tt = a - (s.life + Hs(sd + 34, j) * 0.03f);
                    if (tt < 0) continue;
                    float z = 3 + vz * tt - 320 * tt * tt;
                    if (z > 0) { Vector2 p = tp + v * tt + new Vector2(0, z); Box(true, Rf(p.x), Rf(p.y), 2, 2, ink ? Ik : Tb); Box(true, Rf(p.x), Rf(p.y) + 1, 1, 1, ink ? Il : Tu); }
                    else if (a < WLife - 0.03f) { float tl = (vz + Mathf.Sqrt(vz * vz + 4 * 320 * 3)) / 640; Vector2 p = tp + v * tl; Box(false, Rf(p.x) - 1, Rf(p.y), 3, 1, ink ? Ik : Tt); }
                }
            }
            return true;
        }

        void WhipFx(float dt)
        {
            Vector2 hero = HeroSkillFX.Hero * HellFX.PPU;
            for (int j = _nSlash - 1; j >= 0; j--)   // per-enemy hit: white flash -> cream X -> rose corners, + 4 crimson droplets that splat
            {
                Vector3 sl = _slash[j]; float a = sl.z; Vector2 p = sl; int fr = (int)(a * 30), sd = Mathf.FloorToInt(p.x * 7 + p.y * 13) & 0xFFFF;
                if (fr == 0) { Glow(true, Rf(p.x) - 2, Rf(p.y) - 1, 5, 3, WFk); Glow(true, Rf(p.x) - 1, Rf(p.y) - 2, 3, 5, WFk); Glow(true, Rf(p.x) - 1, Rf(p.y) - 1, 3, 3, WFc); for (int q = 0; q < 4; q++) Glow(true, Rf(p.x + ((q & 1) * 2 - 1) * 4), Rf(p.y + ((q >> 1) * 2 - 1) * 4), 1, 1, WFk); }
                else if (fr == 1) { for (int q = 0; q < 4; q++) { Vector2 v = new Vector2((q & 1) * 2 - 1, (q >> 1) * 2 - 1); Ln(true, p + v * 3, p + v * 6, WFk, 1, true); } Glow(true, Rf(p.x), Rf(p.y), 1, 1, WFc); }
                else if (fr == 2) for (int q = 0; q < 4; q++) Glow(true, Rf(p.x + ((q & 1) * 2 - 1) * 7), Rf(p.y + ((q >> 1) * 2 - 1) * 7), 1, 1, WFk);
                for (int q = 0; q < 4; q++)
                {
                    float an = Hs(sd, q) * Craft1.Two, sp = 25 + 45 * Hs(sd + 1, q), vz = 30 + 40 * Hs(sd + 2, q), z = vz * a - 300 * a * a; Vector2 v = new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.6f) * sp;
                    if (z > 0) { Vector2 w = p + v * a + new Vector2(0, z); Box(true, Rf(w.x), Rf(w.y), 2, 2, WMd); Box(true, Rf(w.x), Rf(w.y) + 1, 1, 1, WCr); }
                    else if (a < 0.48f) { Vector2 w = p + v * (vz / 300f); Box(false, Rf(w.x) - 1, Rf(w.y) - 1, 3, 1, WDk); }
                }
                sl.z += dt; _slash[j] = sl; if (sl.z > 0.5f) _slash[j] = _slash[--_nSlash];
            }
            for (int j = _nMote - 1; j >= 0; j--)
            {
                Vector2 p0 = _motePos[j]; float u = Mathf.Min(1, _mote[j].x / 0.35f); Vector2 tgt = hero + new Vector2(0, 12);
                Vector2 q = p0 + (tgt - p0) * (u * u) + new Vector2(0, Mathf.Sin(u * Mathf.PI) * 10);
                Cell2(true, q, _darkSet ? WMc : Cr); Cell2(true, q - (tgt - p0).normalized * 3, _darkSet ? WMt : Rs);
                _mote[j].x += dt;
                if (u >= 1) { if (_nPop < _healPop.Length) _healPop[_nPop++] = 0; _mote[j] = _mote[--_nMote]; _motePos[j] = _motePos[_nMote]; }
            }
            for (int j = _nPop - 1; j >= 0; j--)
            {
                float a = _healPop[j]; Vector2 c = hero + new Vector2(9, 22 + a * 30);
                Color32 pc = _darkSet ? WMc : Cr, pr = _darkSet ? WMt : Rs; Cell2(true, c, pc); Cell2(true, c + new Vector2(3, 0), pr); Cell2(true, c - new Vector2(3, 0), pr); Cell2(true, c + new Vector2(0, 3), pr); Cell2(true, c - new Vector2(0, 3), pr);
                _healPop[j] += dt; if (_healPop[j] > 0.18f) _healPop[j] = _healPop[--_nPop];
            }
        }

        /* ---------- 189 soul spears ---------- */
        bool Spear(ref It s, int i, float a, float dt)
        {
            Vector2 d = s.d, n = Perp(d); int o = i * MaxPops; bool pops = false; float t = _clock;
            for (int q = o; q < o + MaxPops; q++)
            {
                float e = _pops[q].z; if (e > 0.1f) continue; pops = true; Vector2 pp = _pops[q];
                Craft.RoughBand(this, true, pp, e < 0.034f ? 5 : 7, e < 0.034f ? 8 : 10, 0, Craft.Two, e < 0.034f ? Wh : Gn, s.seed, 1f, 0.3f);
                _pops[q].z += dt;
            }
            if (s.dead >= 0) { s.dead += dt; return s.dead < 0.12f || pops; }
            if ((s.flags & 1) == 0) s.a += d * HeroSkillFX.SpearSpeedU * HellFX.PPU * dt; s.flags &= ~1;
            Vector2 p = s.a;
            if (a < 0.1f) for (int j = -1; j <= 1; j++) Cell(true, p - d * (a * HeroSkillFX.SpearSpeedU * HellFX.PPU) + d * 6 + n * j * 4, j == 0 ? Wh : Gn);
            for (int j = 1; j < 7; j++) { Cell(true, p - d * 3 * (j + 1), Gn); if (j % 2 == 1) Cell(false, p - d * 3 * (j + 1) - n * 3, Dg); }
            if (!HeroSkillFX.SpearV2) { Cell(true, p, Wh); Cell(true, p - d * 3, Wh); Cell(true, p + n * 3 - d * 6, Wh); Cell(true, p - n * 3 - d * 6, Gn); }
            else
            {   // t934u Spear v2: sharp leaf head (VH: "more pointing like a spear"), per-pixel; HL px long, widest HeroSkillFX.SpearHW px at HeroSkillFX.SpearWideAt
                bool f0 = _fine; _fine = true; float HL = HeroSkillFX.SpearHL, W = HeroSkillFX.SpearHW, A = HeroSkillFX.SpearWideAt;
                for (float al = 0; al <= HL; al += 0.5f)
                {
                    float hw = al < A ? W * al / A : Mathf.Max(0.6f, W - (al - A) * 1.4f);
                    for (float w = -hw; w <= hw; w += 0.5f)
                    {
                        Vector2 q = p - d * al + n * w; float aw = Mathf.Abs(w);
                        Color32 col = al < 1.5f ? Wh : aw < 0.6f ? (al < 5 ? Wh : Gn) : aw > hw - 0.8f ? Dg : (w > 0 ? Gn : Dg);
                        Box(true, Rf(q.x), Rf(q.y), 1, 1, col);
                    }
                }
                _fine = f0;
            }
            for (int j = -1; j <= 1; j += 2) Cell(true, p - d * 24 + n * j * 3, Sv);
            int trail = Mathf.Min((int)(a * HeroSkillFX.SpearSpeedU * HellFX.PPU / 3), 10);
            for (int j = 0; j < trail; j++) if (Hs(s.seed + j, Mathf.FloorToInt(t * 30) / 2) < 1 - j / 10f) Cell(false, p - d * (27 + j * 3), j < 5 ? Dg : Te);
            for (int k = 0; k < 2; k++) Cell(true, p - d * (30 + k * 9) + n * Mathf.Sin(t * 22 + k * 2) * 5, Gn);
            return a < 8f;   // safety: SpearEnd normally ends it
        }

        /* ---------- 190 grave pool ---------- */
        bool Pool(ref It s, float a)
        {
            if (a > s.life) return false; Vector2 c = s.a;
            if ((s.flags & 1) == 0) { s.flags |= 1; _deb.Spawn(CraftDebris.Chip, c, 6, 70, 70, Te, Md, Iv, Md, s.seed, 0, 0.45f); }
            float r = s.r * Craft.EaseBack(Mathf.Min(1, a / 0.15f)), left = s.life - a, keep = 1f;
            if (left < 0.6f) { int k = (int)((0.6f - left) / 0.1f) + 1; r = Mathf.Max(6, r - 3 * k); keep = Mathf.Max(0.2f, left / 0.6f); }
            Craft.Blob(this, false, c, r, Te, Te, Md, Md, 3, s.seed, 0.14f, 1f, 0.6f, keep, s.seed);
            Craft.RoughBand(this, false, c, r - 3, r, 0, Craft.Two, Md, s.seed + 1, 2f, 0, 0.5f * keep, s.seed + 2, 24, 0.6f);   // soft dithered edge, not a hazard border
            for (int j = 0; j < 3; j++) { float an = a * 0.9f + j * Craft.Two / 3; for (int m = 0; m < 3; m++) Cell(false, c + new Vector2(Mathf.Cos(an - m * 0.35f), Mathf.Sin(an - m * 0.35f) * 0.6f) * r * (0.55f - m * 0.08f), m == 0 ? Dg : Md); }
            float tick = HeroSkillFX.PoolTickS, tk = (a % tick) / (tick * 0.6f);
            if (tk < 1 && left > 0.6f) { float rr = r * (0.25f + 0.6f * tk); Craft.RoughBand(this, false, c, rr, rr + 3, 0, Craft.Two, Dg, s.seed + (int)(a / tick), 1f, 0.5f, 1f, 0, 16, 0.6f); }
            if (left > 0.4f)
                for (int j = 0; j < 3; j++)
                {
                    float ph = (a + Hs(s.seed, j) * 0.7f) / 0.7f, b = ph % 1f; int ix = (int)ph;
                    Vector2 q = c + new Vector2((Hs(s.seed + ix, j) - 0.5f) * r * 1.2f, (Hs(s.seed + ix, j + 9) - 0.5f) * r * 0.6f);
                    if (b < 0.4f) Cell(false, q, b > 0.15f ? Iv : Md);
                    else if (b < 0.55f) { Cell(false, q + new Vector2(3, 0), Iv); Cell(false, q - new Vector2(3, 0), Iv); Cell(false, q + new Vector2(0, 3), Iv); Cell(false, q - new Vector2(0, 3), Iv); }
                }
            return true;
        }
        void Slowed() { for (int j = 0; j < _nSlow; j++) { Cell(true, _slow[j] + new Vector2(-3, 0), Dg); Cell(true, _slow[j] + new Vector2(3, 3), Md); } _nSlow = 0; }

        void Flush(Mesh mesh, Vector3[] v, Color32[] c, ref int last, bool ground)
        {
            int q = 0; float s = 1f / HellFX.PPU;
            if (ground) { Put(_gs, _ngs, v, c, ref q, s); Put(_gl, _ngl, v, c, ref q, s); Put(_gb, _ngb, v, c, ref q, s); Put(_gf, _ngf, v, c, ref q, s); }
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
