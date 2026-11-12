using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="JudgeFX"/> item.</summary>
    public enum JudgeKind { None = 0, Tongue = 1, Pull = 2, Slam = 3, Gavel = 4 }
    /// <summary>Phase of a <see cref="JudgeFX"/> item.</summary>
    public enum JudgePhase { None = 0, Tell = 1, Strike = 2, Peel = 3, Idle = 4, Break = 5, Stun = 6 }

    /// <summary>
    /// 116. Judge Kit (v1.9.0), the boss telegraph set for a "lying" melee boss (Hellord Level 1 Silver-Tongued Judge).
    /// Three attack warnings, each with a REAL and a FAKE version, plus a gavel break meter and stun beat.
    /// One rule tells real from fake, the same for every attack and readable without colour:
    ///   REAL = SOLID orange rim + a red TIMING FILL that grows toward the hit (full = hit), white blink in the last 15%, then the strike.
    ///   FAKE = DASHED rim (every other pixel) that crawls and wobbles 1px, NO fill, then peels apart (dashes scatter, no impact).
    /// TONGUE WHIP: lane from the mouth to max reach (solid edges + reach bar), fill runs mouth -> reach; strike = red tongue lashes out to
    ///   the reach bar and back (0.15 s), splat ring at the tip.
    /// HOOK PULL: line from the boss hand to the target, lock-on brackets shrink onto the target, "&lt;" chevrons pile up toward the boss
    ///   (pull direction); strike = taut steel line with fast chevrons (0.35 s). The game moves the player / can start HookChainFX.
    /// GAVEL SLAM: circle area, fill grows centre -> rim; strike = 1-frame white disc, shock ring past the rim, 6 floor cracks, dust.
    /// GAVEL METER (<see cref="AttachGavel"/>): N gold 8x6 px pips with a dark outline (default 5) above the boss; <see cref="GavelHit"/> turns
    ///   one steel with a crack and sparks; the last one BREAKS the gavel (white ring + 12 wood/iron shards) and starts the STUN beat in the
    ///   Boss Stun Bar #114 style: "STUN" badge, 3 big 7x7 stars orbiting with depth, a 32x3 gold hazard-stripe bar draining (default 3 s,
    ///   all blink in the last 0.5 s), then the pips re-forge with a white flash.
    /// Visual only (the game owns damage, timing and which warnings are fake). Never tints the boss sprite.
    /// One pooled mesh, one 4x4 white texture, one material (1 draw call); no per-frame allocations; handles are ints, -1 when the pool is full.
    /// </summary>
    public static class JudgeFX
    {
        static JudgeRunner R => JudgeRunner.I;
        public static float TongueReachPx = 112f, TongueWidthPx = 8f, TongueTell = 0.55f, TongueStrike = 0.15f;
        public static float PullTell = 0.6f, PullStrike = 0.35f;
        public static float SlamRadiusPx = 40f, SlamTell = 0.8f, SlamStrike = 0.35f;
        /// <summary>How long a fake warning takes to peel apart after its tell ends.</summary>
        public static float FakePeel = 0.25f;
        public static int GavelHitsToBreak = 5; public static float StunSeconds = 3f;
        public static Material MaterialOverride; public static int SortingOrder = 71; public static string SortingLayer = "Default";
        /// <summary>(handle) when a REAL warning strikes (tell end). Fakes never fire this.</summary>
        public static Action<int> OnStrike;
        /// <summary>(handle) when a gavel breaks / when its stun ends.</summary>
        public static Action<int> OnGavelBreak, OnStunEnd;

        public static void Prewarm(int items = 24) => R.Prewarm(items);
        /// <summary>Tongue whip lane from <paramref name="mouth"/> toward <paramref name="target"/> (world units, direction locked now).</summary>
        public static int TongueWhip(Vector2 mouth, Vector2 target, bool fake = false, float reachPx = -1, float tell = -1)
            => R.Add(JudgeKind.Tongue, mouth, target, reachPx > 0 ? reachPx : TongueReachPx, TongueWidthPx, tell >= 0 ? tell : TongueTell, TongueStrike, fake, null);
        /// <summary>Hook pull line from <paramref name="hand"/> to <paramref name="victim"/> (follows it during the tell).</summary>
        public static int HookPull(Vector2 hand, Transform victim, bool fake = false, float tell = -1)
            => R.Add(JudgeKind.Pull, hand, victim != null ? (Vector2)victim.position : hand + Vector2.right, 0, 0, tell >= 0 ? tell : PullTell, PullStrike, fake, victim);
        public static int HookPull(Vector2 hand, Vector2 target, bool fake = false, float tell = -1)
            => R.Add(JudgeKind.Pull, hand, target, 0, 0, tell >= 0 ? tell : PullTell, PullStrike, fake, null);
        /// <summary>Gavel slam area centred on <paramref name="center"/>.</summary>
        public static int GavelSlam(Vector2 center, bool fake = false, float radiusPx = -1, float tell = -1)
            => R.Add(JudgeKind.Slam, center, center, radiusPx > 0 ? radiusPx : SlamRadiusPx, 0, tell >= 0 ? tell : SlamTell, SlamStrike, fake, null);
        /// <summary>Gavel break meter that follows <paramref name="boss"/>; pips sit <paramref name="pipsOffsetPx"/> above its pivot.</summary>
        public static int AttachGavel(Transform boss, Vector2 pipsOffsetPx, Vector2 gavelOffsetPx, int hits = -1)
            => R.AddGavel(boss, pipsOffsetPx, gavelOffsetPx, hits > 0 ? hits : GavelHitsToBreak);
        /// <summary>One hit on the gavel. Returns true when this hit breaks it (stun starts).</summary>
        public static bool GavelHit(int gavel) => R.GavelHit(gavel);
        public static bool IsStunned(int gavel) => R.Phase(gavel) == JudgePhase.Stun || R.Phase(gavel) == JudgePhase.Break;
        public static int GavelHitsLeft(int gavel) => R.HitsLeft(gavel);
        public static JudgePhase GetPhase(int h) => R.Phase(h);
        public static bool IsFake(int h) => R.Fake(h);
        public static void Cancel(int h) => R.Cancel(h);
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => JudgeRunner.Exists ? R.Count : 0;
    }

    public class JudgeRunner : MonoBehaviour
    {
        static JudgeRunner _i;
        public static bool Exists => _i != null;
        public static JudgeRunner I { get { if (_i == null) { var go = new GameObject("JudgeFX"); DontDestroyOnLoad(go); _i = go.AddComponent<JudgeRunner>(); } return _i; } }

        struct It
        {
            public JudgeKind kind; public JudgePhase ph; public bool fake; public Vector2 a, b, d, off2; public float L, w, tell, strike, t, pt;
            public Transform v; public int hits, need; public ushort gen; public float hitT;
        }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }

        const int MaxQ = 6144; struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ]; int _nb, _nf, _last;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        // Endesga 32
        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), Wh = new Color32(255, 255, 255, 255), Y = new Color32(0xfe, 0xe7, 0x61, 255),
            O = new Color32(0xf7, 0x76, 0x22, 255), Rd = new Color32(0xe4, 0x3b, 0x44, 255), Pk = new Color32(0xf6, 0x75, 0x7a, 255), Bl = new Color32(0x9e, 0x28, 0x35, 255),
            Lt = new Color32(0x8b, 0x9b, 0xb4, 255), Md = new Color32(0x5a, 0x69, 0x88, 255), Wd = new Color32(0xbe, 0x4a, 0x2f, 255), Wk = new Color32(0x73, 0x3e, 0x39, 255),
            Ds = new Color32(0xea, 0xd4, 0xaa, 255), G = new Color32(0xfe, 0xae, 0x34, 255), Ko = new Color32(0x10, 0x00, 0x10, 255);
        static readonly string[] StunTxt = { "WWW W.. WWW ..W WWW", "WWW .W. .W. .W. .W.", "W.W W.W W.W W.W WWW", "WWW W.W W.W W.W W.W" }; // S T U N, rows top->bottom
        static readonly string[][] StunRows = Array.ConvertAll(StunTxt, x => x.Split(' '));

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == JudgeKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mesh != null) return;
            _vtx = new Vector3[MaxQ * 8]; _uv = new Vector2[MaxQ * 8]; _col = new Color32[MaxQ * 8]; _tri = new int[MaxQ * 12];
            for (int i = 0; i < MaxQ * 2; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "JudgeFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Judge White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = JudgeFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "JudgeFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] JudgeFX: no sprite shader found; set JudgeFX.MaterialOverride.");
            mr.sortingOrder = JudgeFX.SortingOrder; mr.sortingLayerName = JudgeFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != JudgeKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = JudgeKind.None; _it[i].ph = JudgePhase.None; _it[i].v = null; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, JudgePhase p) { k.ph = p; k.pt = 0; }
        int Alloc() { if (_mesh == null) Prewarm(24); if (_freeN == 0) return -1; int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++; return i; }

        public int Add(JudgeKind kind, Vector2 a, Vector2 b, float L, float w, float tell, float strike, bool fake, Transform v)
        {
            int i = Alloc(); if (i < 0) return -1; ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.fake = fake; k.a = a * ppu; k.b = b * ppu; var d = k.b - k.a; k.d = d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.right;
            k.L = L; k.w = w; k.tell = Mathf.Max(0.25f, tell); k.strike = strike; k.v = v; k.t = 0; k.hits = 0; k.need = 0;
            Go(ref k, JudgePhase.Tell); return Hd(i);
        }
        public int AddGavel(Transform boss, Vector2 pipOff, Vector2 gavOff, int need)
        {
            int i = Alloc(); if (i < 0) return -1; ref var k = ref _it[i];
            k.kind = JudgeKind.Gavel; k.fake = false; k.v = boss; k.b = pipOff; k.off2 = gavOff; k.need = Mathf.Clamp(need, 1, 12); k.hits = 0; k.hitT = 9; k.t = 0;
            k.a = boss != null ? (Vector2)boss.position * HellFX.PPU : Vector2.zero; Go(ref k, JudgePhase.Idle); return Hd(i);
        }
        public bool GavelHit(int h)
        {
            if (!Get(h, out int i) || _it[i].kind != JudgeKind.Gavel || _it[i].ph != JudgePhase.Idle) return false; ref var k = ref _it[i];
            k.hits++; k.hitT = 0; if (k.hits < k.need) return false;
            Go(ref k, JudgePhase.Break); JudgeFX.OnGavelBreak?.Invoke(h); return true;
        }
        public int HitsLeft(int h) => Get(h, out int i) && _it[i].kind == JudgeKind.Gavel ? Mathf.Max(0, _it[i].need - _it[i].hits) : 0;
        public JudgePhase Phase(int h) => Get(h, out int i) ? _it[i].ph : JudgePhase.None;
        public bool Fake(int h) => Get(h, out int i) && _it[i].fake;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].kind != JudgeKind.None) Free(i); }

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
        void Disc(Vector2 c, float r, Color32 col) { for (int y = -(int)r; y <= (int)r; y++) { float hw = Mathf.Sqrt(Mathf.Max(0, r * r - y * y)); Span(c.x - hw, c.x + hw, c.y + y, col); } }
        void Chev(Vector2 tip, Vector2 d, int len, Color32 c) { Vector2 n = N(d); for (int s = 0; s <= len; s++) { var p = tip - d * s + n * s; Px(p.x, p.y, c); if (s > 0) { p = tip - d * s - n * s; Px(p.x, p.y, c); } } }
        void Rect(float x, float y, float w, float h, Color32 c) { if (_nf < MaxQ) _fg[_nf++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        // 7x7 star with dark backing: white core, yellow (front) / gold (back) arms 3px
        void BigStar(Vector2 p, bool front) { Color32 a = front ? Y : G; Px(p.x, p.y, Wh); for (int r = 1; r <= 3; r++) { Color32 c = r == 1 ? (front ? Wh : Y) : a; Px(p.x + r, p.y, c); Px(p.x - r, p.y, c); Px(p.x, p.y + r, c); Px(p.x, p.y - r, c); } Px(p.x + 1, p.y + 1, a); Px(p.x - 1, p.y - 1, a); Px(p.x + 1, p.y - 1, a); Px(p.x - 1, p.y + 1, a); }
        void Star(Vector2 p, Color32 c) { Px(p.x, p.y, Wh); Px(p.x + 1, p.y, c); Px(p.x - 1, p.y, c); Px(p.x, p.y + 1, c); Px(p.x, p.y - 1, c); }
        // peel: the dashed rim scatters outward and fades (fake ending, no impact)
        void Peel(Vector2 c, float r, float q, int n, Color32 col)
        { for (int i = 0; i < n; i++) { float a = 2 * Mathf.PI * (i + 0.5f * H(i)) / n, rr = r + q * (6 + 6 * H(i + 7)); if (H(i * 3 + 1) < q) continue; Px(c.x + Mathf.Cos(a) * rr, c.y + Mathf.Sin(a) * rr, Al(col, 1 - q), false); } }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu; _nb = _nf = 0;
            for (int i = 0; i < _it.Length; i++)
            {
                ref var k = ref _it[i]; if (k.kind == JudgeKind.None) continue;
                k.t += dt; k.pt += dt; k.hitT += dt;
                if (k.v != null && (k.kind == JudgeKind.Gavel || (k.kind == JudgeKind.Pull && k.ph == JudgePhase.Tell))) { if (k.kind == JudgeKind.Gavel) k.a = (Vector2)k.v.position * ppu; else k.b = (Vector2)k.v.position * ppu; }
                Step(ref k, i);
                if (k.kind != JudgeKind.None) Draw(ref k);
            }
            int q = 0;
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
        }
        void Step(ref It k, int i)
        {
            switch (k.ph)
            {
                case JudgePhase.Tell:
                    if (k.pt >= k.tell) { if (k.fake) Go(ref k, JudgePhase.Peel); else { Go(ref k, JudgePhase.Strike); JudgeFX.OnStrike?.Invoke(Hd(i)); } }
                    break;
                case JudgePhase.Strike: if (k.pt >= k.strike) Free(i); break;
                case JudgePhase.Peel: if (k.pt >= JudgeFX.FakePeel) Free(i); break;
                case JudgePhase.Break: if (k.pt >= 0.4f) Go(ref k, JudgePhase.Stun); break;
                case JudgePhase.Stun: if (k.pt >= JudgeFX.StunSeconds) { k.hits = 0; k.hitT = 0; Go(ref k, JudgePhase.Idle); JudgeFX.OnStunEnd?.Invoke(Hd(i)); } break;
            }
        }
        void Draw(ref It k)
        {
            float t = k.pt, q = Mathf.Clamp01(t / k.tell); bool blink = !k.fake && k.ph == JudgePhase.Tell && q > 0.85f && ((int)(t * 20) & 1) == 0;
            Color32 rim = blink ? Wh : O, fill = Al(Rd, 0.45f);
            switch (k.kind)
            {
                case JudgeKind.Tongue:
                {
                    Vector2 a = k.a, d = k.d, n = N(d), e = a + d * k.L; float hw = k.w * 0.5f;
                    if (k.ph == JudgePhase.Tell)
                    {
                        Line(a + n * hw, e + n * hw, rim, k.fake, t); Line(a - n * hw, e - n * hw, rim, k.fake, t);
                        Line(e + n * (hw + 3), e - n * (hw + 3), k.fake ? O : (q > 0.85f ? Wh : Y), k.fake, t); // reach bar
                        if (!k.fake) { float f = k.L * q; for (float r = 1; r < f; r += 1) for (float s = -hw + 1; s < hw; s += 1) if ((((int)r + (int)(s + 8)) & 1) == 0) { var p = a + d * r + n * s; Px(p.x, p.y, fill, false); } }
                    }
                    else if (k.ph == JudgePhase.Strike)
                    {
                        float s = t / k.strike, ext = s < 0.4f ? Mathf.Min(1, s / 0.15f) : 1 - (s - 0.4f) / 0.6f; var tip = a + d * k.L * ext;
                        for (float r = 0; r <= k.L * ext; r += 1) { var p = a + d * r; float wob = Mathf.Sin(r * 0.35f - t * 40) * (1 - ext * 0.5f); p += n * wob;
                            Px(p.x, p.y, Rd); Px(p.x + n.x, p.y + n.y, Pk); Px(p.x - n.x, p.y - n.y, Bl); }
                        Px(tip.x, tip.y, Wh); Px(tip.x + d.x, tip.y + d.y, Wh);
                        if (s < 0.5f) Circle(e, 2 + s * 14, s < 0.2f ? Wh : Pk, false, t);
                    }
                    else Peel(a + d * k.L * 0.5f, k.L * 0.25f, t / JudgeFX.FakePeel, 18, O);
                    break;
                }
                case JudgeKind.Pull:
                {
                    Vector2 a = k.a, b = k.b, dv = b - a; float len = dv.magnitude; Vector2 d = len > 0.01f ? dv / len : k.d, n = N(d);
                    if (k.ph == JudgePhase.Tell)
                    {
                        Line(a, b, rim, k.fake, t);
                        float br = k.fake ? 9 + Mathf.Sin(t * 9) * 2 : Mathf.Lerp(14, 6, q); // lock-on brackets
                        for (int c = 0; c < 4; c++) { float sx = (c & 1) == 0 ? 1 : -1, sy = (c & 2) == 0 ? 1 : -1; var cn = b + new Vector2(sx, sy) * br;
                            Line(cn, cn - new Vector2(sx * 3, 0), rim, k.fake, t); Line(cn, cn - new Vector2(0, sy * 3), rim, k.fake, t); }
                        if (!k.fake) { int nC = Mathf.FloorToInt(q * len / 12); for (int c = 0; c < nC; c++) { float r = len - 6 - c * 12 + Mod(-t * 30, 12); if (r > 4 && r < len - 4) Chev(a + d * r, -d, 2, Y); } }
                    }
                    else if (k.ph == JudgePhase.Strike)
                    {
                        for (float r = 0; r <= len; r += 1) { var p = a + d * r; float m = Mod(r + t * 240, 4); Px(p.x, p.y, m < 0.6f ? Wh : (m < 2.5f ? Lt : Md)); }
                        for (float r = Mod(-t * 240, 10); r < len; r += 10) Chev(a + d * r, -d, 2, Wh);
                        if (t < 0.07f) Circle(b, 4, Wh, false, t);
                    }
                    else Peel((a + b) * 0.5f, len * 0.3f, t / JudgeFX.FakePeel, 16, O);
                    break;
                }
                case JudgeKind.Slam:
                {
                    Vector2 c = k.a; float R = k.L;
                    if (k.ph == JudgePhase.Tell)
                    {
                        if (!k.fake) { Disc(c, R * q, fill); Circle(c, Mathf.Max(1, R * q), Al(Pk, 0.9f), false, t, false); }
                        Circle(c, R, rim, k.fake, t);
                        if (!k.fake) { Px(c.x, c.y, Y); Px(c.x + 1, c.y, Y); Px(c.x - 1, c.y, Y); Px(c.x, c.y + 1, Y); Px(c.x, c.y - 1, Y); }
                    }
                    else if (k.ph == JudgePhase.Strike)
                    {
                        float s = t / k.strike;
                        if (t < 0.034f) Disc(c, R, Wh); else if (t < 0.1f) Disc(c, R, Al(Ds, 0.5f));
                        Circle(c, R + s * 14, s < 0.3f ? Wh : Y, false, t); if (s < 0.6f) Circle(c, R * 0.6f + s * 10, O, false, t, false);
                        for (int j = 0; j < 6; j++) { float an = j * 1.0472f + 0.3f; Vector2 dd = new Vector2(Mathf.Cos(an), Mathf.Sin(an)); for (int r = 3; r < R * Mathf.Min(1, s * 3) * 0.8f; r++) { var p = c + dd * r + N(dd) * ((r / 4 & 1) == 0 ? 1 : 0); Px(p.x, p.y, Wk); } }
                        for (int j = 0; j < 14; j++) { float an = j * 0.4488f, rr = R * (0.7f + 0.5f * H(j)) + s * 10; Px(c.x + Mathf.Cos(an) * rr, c.y + Mathf.Sin(an) * rr * 0.8f + s * 4, Al(Ds, 1 - s), false); }
                    }
                    else Peel(c, R, t / JudgeFX.FakePeel, 28, O);
                    break;
                }
                case JudgeKind.Gavel:
                {
                    Vector2 p0 = k.a + k.b, g = k.a + k.off2; int n = k.need; float x0 = p0.x - (n * 11 - 3) * 0.5f;
                    if (k.ph == JudgePhase.Idle)
                    {
                        bool reforge = k.hits == 0 && k.hitT < 0.3f;
                        for (int j = 0; j < n; j++)
                        {
                            // 8x6 pip with an opaque dark outline: intact = gold (bright on red floors), broken = steel with a dark crack
                            float x = x0 + j * 11; bool broken = j < k.hits, fresh = j == k.hits - 1 && k.hitT < 0.12f;
                            Rect(x - 1, p0.y - 1, 10, 8, Ko);
                            if (fresh || reforge) { Rect(x, p0.y, 8, 6, Wh); continue; }
                            if (!broken) { Rect(x, p0.y, 8, 6, Y); Rect(x, p0.y + 5, 8, 1, Wh); Rect(x, p0.y, 8, 1, G); }
                            else { Rect(x, p0.y, 8, 6, Md); Rect(x, p0.y + 5, 8, 1, Lt); for (int c = 0; c < 4; c++) Rect(x + 2 + c, p0.y + 4 - c, 1, 1, Ko); }
                        }
                        if (k.hits > 0 && k.hitT < 0.2f) { float s = k.hitT / 0.2f; for (int j = 0; j < 8; j++) { float an = j * 0.785f + k.hits; Px(g.x + Mathf.Cos(an) * (2 + s * 9), g.y + Mathf.Sin(an) * (2 + s * 9), s < 0.4f ? Wh : Y); } }
                    }
                    else if (k.ph == JudgePhase.Break)
                    {
                        float s = t / 0.4f;
                        if (t < 0.05f) Disc(g, 6, Wh);
                        Circle(g, 3 + s * 22, s < 0.3f ? Wh : Y, false, t, s < 0.5f); if (s < 0.7f) Circle(g, 2 + s * 12, O, false, t, false);
                        for (int j = 0; j < 12; j++) { float an = j * 0.5236f + 0.2f * H(j), v = 50 + 40 * H(j + 3); var sp = g + new Vector2(Mathf.Cos(an), Mathf.Sin(an)) * v * t + Vector2.down * 120 * t * t;
                            Color32 c = (j % 3) == 0 ? Lt : ((j & 1) == 0 ? Wd : Wk); Px(sp.x, sp.y, c); if ((j & 1) == 0) Px(sp.x + 1, sp.y, c); }
                    }
                    else if (k.ph == JudgePhase.Stun)
                    {
                        // StunBarFX #114 language: 'STUN' badge, big orbiting stars with depth, gold hazard-stripe bar with a dark frame
                        float left = JudgeFX.StunSeconds - t; bool hide = left < 0.5f && ((int)(t * 10) & 1) == 0;
                        Vector2 sc = p0 + new Vector2(0, 2);
                        float pop = t < 0.15f ? 2 * (1 - t / 0.15f) : 0;
                        if (!hide)
                        {
                            float bx0 = sc.x - 9, by0 = sc.y + 8 + pop; Rect(bx0 - 2, by0 - 2, 19, 9, Ko);
                            Color32 tc = ((int)(t * 4) & 1) == 0 ? Y : Wh;
                            for (int l = 0; l < 4; l++) { var rows = StunRows[l]; for (int r = 0; r < 5; r++) for (int c = 0; c < 3; c++) if (rows[r][c] == 'W') Rect(bx0 + l * 4 + c, by0 + 4 - r, 1, 1, tc); }
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            if (hide) break; float an = t * 4 + j * 2.094f; bool front = Mathf.Sin(an) < 0;
                            BigStar(sc + new Vector2(Mathf.Cos(an) * 18, -6 + Mathf.Sin(an) * 5), front);
                        }
                        float frac = Mathf.Clamp01(left / JudgeFX.StunSeconds); int bw = 32; float bx = sc.x - bw * 0.5f, by = sc.y + 19; /* v1.9.3: bar above the STUN badge, clear of the face */
                        Rect(bx - 1, by - 1, bw + 2, 5, Ko);
                        Rect(bx - 1, by + 3, bw + 2, 1, ((int)(t * 8) & 1) == 0 ? Y : Wh);
                        int fillN = Mathf.RoundToInt(bw * frac);
                        for (int x = 0; x < bw; x++) for (int y = 0; y < 3; y++) Rect(bx + x, by + y, 1, 1, x < fillN ? ((((x + y + (int)(t * 16)) & 3) < 2) ? Y : G) : Md);
                    }
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
