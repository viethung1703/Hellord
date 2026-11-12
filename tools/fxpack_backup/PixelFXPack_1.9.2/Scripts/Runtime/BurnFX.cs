using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// 110. Burn Status, rebuilt in v1.8.7 (replaces the v1.7/v1.8 tint-flicker burn; StatusFX.BurnStatus keeps its signature and now routes here).
    /// Readable "this enemy is burning and taking ticks" for 100+ horde enemies, without hiding, dissolving or recolouring the sprite:
    ///  - Flame tongues on a BACK renderer (sorting <see cref="BackSortingOrder"/>, put it below your enemies): they rise from the head and lick out
    ///    at the shoulders, so they only show around the silhouette and never cover it. 6-frame uneven loop, white root -> yellow -> orange -> red tip.
    ///  - 1 px embers rising above the head (front renderer).
    ///  - Every damage tick: flames flare one ramp step hotter for 0.13 s, 4 sparks pop from the head, a small outlined number floats up ABOVE the head
    ///    (white for 2 frames, then gold), and optionally a 2-frame warm multiply on SpriteRenderer.color (never brightens to white, material untouched).
    ///  - End: flames shrink, then 3 violet-grey smoke puffs rise and shrink.
    /// All burns share 2 meshes, 1 atlas, 1 material (2 draw calls total). Pooled structs, no per-frame allocation after Prewarm.
    /// Damage stays in game code: subscribe <see cref="OnTick"/> (fires in sync with the visual tick) or drive ticks yourself with <see cref="Tick"/>.
    /// </summary>
    public static class BurnFX
    {
        static BurnRunner R => BurnRunner.I;
        /// <summary>(handle, target, damage) at every automatic tick. Apply the damage here so numbers and HP stay in sync.</summary>
        public static Action<int, Transform, int> OnTick;
        public static Material MaterialOverride;
        /// <summary>Flames. Must be BELOW your enemy sprites' sorting order (same layer) so the body hides the flame roots.</summary>
        public static int BackSortingOrder = -1;
        /// <summary>Embers, sparks, numbers, smoke. Above enemies.</summary>
        public static int FrontSortingOrder = 62;
        public static string SortingLayer = "Default";
        /// <summary>Off by default since v1.8.7 (HM: no full-sprite tint). 2-frame warm multiply on the target's SpriteRenderer.color at each tick (restored after). Set false if your game owns SpriteRenderer.color.</summary>
        public static bool TickTint = false;
        public static Color TickTintColor = new Color(1f, 0.8f, 0.62f, 1f);
        public static bool ShowNumbers = true;
        /// <summary>Above this many live burns, each burn draws 2 tongues and 1 ember (horde LOD).</summary>
        public static int LodThreshold = 64;

        public static void Prewarm(int burns = 256) => R.Prewarm(burns);

        /// <summary>Sets <paramref name="target"/> on fire for <paramref name="seconds"/> (&lt;= 0 = until Extinguish), ticking <paramref name="tickDamage"/>
        /// every <paramref name="tickInterval"/> s (&lt;= 0 = no automatic ticks, call Tick). Size in pixels: <paramref name="widthPx"/> / <paramref name="heightPx"/>
        /// of the visible body and <paramref name="feetYPx"/> (bottom of the body relative to the transform); 0 / NaN = taken from the SpriteRenderer bounds.
        /// Re-igniting a burning target refreshes it (higher damage and longer remaining time win, Brotato-style) and returns the same handle.
        /// Returns a handle, or -1 if the pool is full.</summary>
        public static int Ignite(Transform target, float seconds = 3f, int tickDamage = 1, float tickInterval = 0.5f, float widthPx = 0, float heightPx = 0, float feetYPx = float.NaN)
            => R.Ignite(target, seconds, tickDamage, tickInterval, widthPx, heightPx, feetYPx);
        /// <summary>Plays one tick pulse (flare, sparks, number, tint) now. Does not call OnTick.</summary>
        public static void Tick(int handle, int damage) => R.TickNow(handle, damage);
        /// <summary>Ends a burn: flames shrink and smoke rises (smoke = false: removed at once).</summary>
        public static void Extinguish(int handle, bool smoke = true) => R.End(handle, smoke);
        public static bool IsAlive(int handle) => R.Alive(handle);
        public static int Find(Transform target) => R.Find(target);
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => R.Count;
    }

    [AddComponentMenu("")]
    public class BurnRunner : MonoBehaviour
    {
        static BurnRunner _i;
        public static BurnRunner I { get { if (_i == null) { var go = new GameObject("BurnFX"); DontDestroyOnLoad(go); _i = go.AddComponent<BurnRunner>(); } return _i; } }

        // ---- pixel art (also parsed by review/v187/burn_v187.py, so the mock-up uses the same art) ----
        // . clear  W white  Y #fee761  G #feae34  O #f77622  R #e43b44  D #a22633  K #3e2731  L #8b9bb4  M #5a6988  N #3a4466
        internal static readonly string[][] FlameBig = {
            new[]{"...R...","..RO...","..ROR..",".ROGOR.",".OGYGO.","ROGYYGR","OGYWYGO",".GYWWG.","..YWY.."},
            new[]{"..R....","..RR...",".ROR...",".ROOR..","ROGGOR.","OGYYGOR","OGYWYGO",".GYWWG.","..YWY.."},
            new[]{"....R..","...RR..","...ROR.","..ROGO.",".ROGYGR","ROGYYGO","OGYWYGO",".GYWWG.","..YWY.."},
            new[]{".......","...R...","..ROR..",".ROGOR.",".OGYGO.","ROGYYGR","OGYWYGO",".GYWWG.","..YWY.."},
            new[]{"..R.R..","..R.R..",".ROROR.",".OGOGO.","ROGYGOR","OGYYYGO","OGYWYGO",".GYWWG.","..YWY.."},
            new[]{".......","..R....","..ORR..",".ROGOR.",".OGYGOR","ROGYYGO","OGYWYGO",".GYWWG.","..YWY.."} };
        internal static readonly string[][] FlameSmall = {
            new[]{"..R..",".ROR.",".OGO.","OGYGO","GYWYG",".YWY."},
            new[]{".R...",".RO..",".OGR.","OGYGO","GYWYG",".YWY."},
            new[]{"...R.","..OR.",".ROO.","OGYGO","GYWYG",".YWY."},
            new[]{".....","..R..",".ROR.","OGYGO","GYWYG",".YWY."} };
        internal static readonly string[] Digits = { // 3x5, white fill (vertex colour tints it), K outline baked at build
            "WWW W.W W.W W.W WWW", ".W. WW. .W. .W. WWW", "WWW ..W WWW W.. WWW", "WWW ..W .WW ..W WWW", "W.W W.W WWW ..W ..W",
            "WWW W.. WWW ..W WWW", "WWW W.. WWW W.W WWW", "WWW ..W .W. .W. .W.", "WWW W.W WWW W.W WWW", "WWW W.W WWW ..W WWW" };
        internal static readonly string[][] FlameTiny = { new[]{".R.","ROR","OYO"}, new[]{"R..",".OR","OYO"}, new[]{"..R","RO.","OYO"} }; // on-body licks
        internal static readonly string[] SmokeBig = { ".LL..", "LLLL.", "LLMMN", ".MNN." }, SmokeSmall = { ".L.", "LMN" };
        internal static readonly float[] FrameDur = { 0.09f, 0.11f, 0.08f, 0.12f, 0.10f, 0.09f }; // uneven loop, 0.59 s
        // cells: 0 solid | 1-6 big | 7-12 big hot | 13-16 small | 17-20 small hot | 21-30 digits | 31 smoke big | 32 smoke small | 33-35 tiny body licks
        internal const int Cell = 12, Cells = 36, CTiny = 33, CBig = 1, CBigHot = 7, CSmall = 13, CSmallHot = 17, CDigit = 21, CSmokeB = 31, CSmokeS = 32;
        internal const float FlareT = 0.13f, EndShrinkT = 0.2f, SmokeT = 0.6f, NumT = 0.5f, SparkT = 0.15f, TintT = 0.067f, EmberPeriod = 0.8f;
        internal static readonly Color32 Wh = new Color32(255, 255, 255, 255), Yc = new Color32(0xfe, 0xe7, 0x61, 255), Gc = new Color32(0xfe, 0xae, 0x34, 255),
            Oc = new Color32(0xf7, 0x76, 0x22, 255), Rc = new Color32(0xe4, 0x3b, 0x44, 255), Dc = new Color32(0xa2, 0x26, 0x33, 255), Kc = new Color32(0x3e, 0x27, 0x31, 255);

        static Color32 Pal(char ch)
        {
            switch (ch)
            {
                case 'W': return Wh; case 'Y': return Yc; case 'G': return Gc; case 'O': return Oc; case 'R': return Rc; case 'D': return Dc; case 'K': return Kc;
                case 'L': return new Color32(0x8b, 0x9b, 0xb4, 255); case 'M': return new Color32(0x5a, 0x69, 0x88, 255); case 'N': return new Color32(0x3a, 0x44, 0x66, 255);
            }
            return new Color32(0, 0, 0, 0);
        }
        internal static char Hot(char c) => c == 'D' ? 'R' : c == 'R' ? 'O' : c == 'O' ? 'G' : c == 'G' ? 'Y' : c == 'Y' ? 'W' : c;

        struct B
        {
            public Transform tr; public SpriteRenderer sr; public Vector2 last; public float t, life, interval, nextTick, w, h, feet, lastTick, endT, tintT, seed;
            public int dmg, numVal, ticks; public byte state; public bool ending, smoke, tinted; public Color tint0; public ushort gen;
        }
        B[] _b = new B[0]; int[] _free; int _freeN, _count;
        public int Count => _count;

        sealed class QM
        {
            public const int MaxQ = 8192; public int n, last; public Mesh mesh; public Vector3[] v; public Vector2[] uv; public Color32[] c;
            public QM(string name, Material m, int order, Transform parent)
            {
                v = new Vector3[MaxQ * 4]; uv = new Vector2[MaxQ * 4]; c = new Color32[MaxQ * 4]; var tri = new int[MaxQ * 6];
                for (int i = 0; i < MaxQ; i++) { int a = i * 4, t = i * 6; tri[t] = a; tri[t + 1] = a + 1; tri[t + 2] = a + 2; tri[t + 3] = a; tri[t + 4] = a + 2; tri[t + 5] = a + 3; }
                mesh = new Mesh { name = name }; mesh.MarkDynamic(); mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = v; mesh.uv = uv; mesh.colors32 = c; mesh.triangles = tri;
                var go = new GameObject(name); go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh; var mr = go.AddComponent<MeshRenderer>();
                if (m != null) mr.sharedMaterial = m; mr.sortingOrder = order; mr.sortingLayerName = BurnFX.SortingLayer;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            }
            // 12x12 px quad, bottom-centre anchored at integer pixel (x, y)
            public void Add(int cell, int x, int y, Color32 col)
            {
                if (n >= MaxQ) return; float u = 1f / HellFX.PPU; int a = n * 4; float x0 = (x - 6) * u, x1 = (x + 6) * u, y0 = y * u, y1 = (y + 12) * u;
                v[a] = new Vector3(x0, y0, 0); v[a + 1] = new Vector3(x0, y1, 0); v[a + 2] = new Vector3(x1, y1, 0); v[a + 3] = new Vector3(x1, y0, 0);
                float s0 = (float)cell / Cells, s1 = (float)(cell + 1) / Cells;
                uv[a] = new Vector2(s0, 0); uv[a + 1] = new Vector2(s0, 1); uv[a + 2] = new Vector2(s1, 1); uv[a + 3] = new Vector2(s1, 0);
                c[a] = c[a + 1] = c[a + 2] = c[a + 3] = col; n++;
            }
            // 1x1 px quad at pixel (x, y), sampling the solid cell 0
            public void Px(int x, int y, Color32 col)
            {
                if (n >= MaxQ) return; float u = 1f / HellFX.PPU; int a = n * 4; float x0 = x * u, x1 = (x + 1) * u, y0 = y * u, y1 = (y + 1) * u;
                v[a] = new Vector3(x0, y0, 0); v[a + 1] = new Vector3(x0, y1, 0); v[a + 2] = new Vector3(x1, y1, 0); v[a + 3] = new Vector3(x1, y0, 0);
                var m = new Vector2(0.5f / Cells, 0.5f); uv[a] = uv[a + 1] = uv[a + 2] = uv[a + 3] = m;
                c[a] = c[a + 1] = c[a + 2] = c[a + 3] = col; n++;
            }
            public void Upload()
            {
                for (int z = n; z < last; z++) { int a = z * 4; v[a] = v[a + 1] = v[a + 2] = v[a + 3] = Vector3.zero; }
                last = n; mesh.vertices = v; mesh.uv = uv; mesh.colors32 = c; mesh.RecalculateBounds(); n = 0;
            }
        }
        QM _back, _front; Texture2D _atlas;

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 4096);
            if (_b.Length < cap)
            {
                var nb = new B[cap]; Array.Copy(_b, nb, _b.Length);
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _b.Length || _b[i].state == 0) _free[_freeN++] = i;
                _b = nb;
            }
            if (_back != null) return;
            BuildAtlas();
            var m = BurnFX.MaterialOverride;
            if (m == null)
            {
                var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (sh == null) sh = Shader.Find("Sprites/Default");
                m = sh != null ? new Material(sh) { name = "BurnFX (Shared)", hideFlags = HideFlags.DontSave } : null;
            }
            if (m != null) m.mainTexture = _atlas; else Debug.LogWarning("[PixelFX] BurnFX: no sprite shader found; set BurnFX.MaterialOverride.");
            _back = new QM("BurnFX_Back", m, BurnFX.BackSortingOrder, transform);
            _front = new QM("BurnFX_Front", m, BurnFX.FrontSortingOrder, transform);
        }

        void BuildAtlas()
        {
            int W = Cell * Cells; var px = new Color32[W * Cell];
            // rows given top-down; art bottom-aligned (cell y = 0) and centred on column 6
            void Art(int cell, string[] rows, bool hot, int yOff = 0)
            {
                int h = rows.Length, w = rows[0].Length, x0 = 6 - w / 2;
                for (int r = 0; r < h; r++) for (int x = 0; x < w; x++)
                {
                    char ch = rows[r][x]; if (ch == '.') continue; if (hot) ch = Hot(ch);
                    int yy = yOff + (h - 1 - r), xx = x0 + x; if (xx < 0 || xx >= Cell || yy < 0 || yy >= Cell) continue;
                    px[yy * W + cell * Cell + xx] = Pal(ch);
                }
            }
            for (int y = 0; y < Cell; y++) for (int x = 0; x < Cell; x++) px[y * W + x] = Wh; // cell 0 solid (sampled at its centre for 1 px marks)
            for (int i = 0; i < 6; i++) { Art(CBig + i, FlameBig[i], false); Art(CBigHot + i, FlameBig[i], true); }
            for (int i = 0; i < 4; i++) { Art(CSmall + i, FlameSmall[i], false); Art(CSmallHot + i, FlameSmall[i], true); }
            for (int d = 0; d < 10; d++)
            {
                var rows = Digits[d].Split(' '); var o = new char[7][]; for (int r = 0; r < 7; r++) o[r] = ".....".ToCharArray();
                for (int r = 0; r < 5; r++) for (int x = 0; x < 3; x++) if (rows[r][x] == 'W')
                    for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) if (o[r + 1 + dy][x + 1 + dx] == '.') o[r + 1 + dy][x + 1 + dx] = 'K';
                for (int r = 0; r < 5; r++) for (int x = 0; x < 3; x++) if (rows[r][x] == 'W') o[r + 1][x + 1] = 'W';
                var s = new string[7]; for (int r = 0; r < 7; r++) s[r] = new string(o[r]); Art(CDigit + d, s, false);
            }
            Art(CSmokeB, SmokeBig, false); for (int k = 0; k < 3; k++) Art(CTiny + k, FlameTiny[k], false); Art(CSmokeS, SmokeSmall, false);
            _atlas = new Texture2D(W, Cell, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "BurnFX_Atlas", hideFlags = HideFlags.DontSave };
            _atlas.SetPixels32(px); _atlas.Apply(false, true);
        }

        static float H(int a, int b) { uint h = (uint)(a * 374761393 + b * 668265263); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xffff) / 65535f; }
        int Handle(int i) => i | (_b[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xffff; return h >= 0 && i < _b.Length && _b[i].state != 0 && _b[i].gen == (ushort)(h >> 16); }
        public bool Alive(int h) => Get(h, out _);
        public int Find(Transform t) { for (int i = 0; i < _b.Length; i++) if (_b[i].state != 0 && !_b[i].ending && _b[i].tr == t) return Handle(i); return -1; }

        public int Ignite(Transform target, float seconds, int dmg, float interval, float w, float h, float feet)
        {
            if (!target) return -1;
            int ex = Find(target);
            if (ex >= 0)
            {
                ref var e = ref _b[ex & 0xffff]; e.dmg = Mathf.Max(e.dmg, dmg);
                float rem = seconds > 0 ? seconds : float.PositiveInfinity; if (rem > e.life - e.t) e.life = e.t + rem; return ex;
            }
            if (_b.Length == 0) Prewarm(256);
            if (_freeN == 0) { if (_b.Length >= 4096) return -1; Prewarm(_b.Length * 2); }
            int i = _free[--_freeN]; ref var b = ref _b[i];
            ushort g = (ushort)(b.gen + 1); b = default; b.gen = g; b.state = 1; _count++;
            b.tr = target; b.sr = target.GetComponentInChildren<SpriteRenderer>(); b.last = target.position;
            float ppu = HellFX.PPU;
            if (b.sr && b.sr.sprite && (w <= 0 || h <= 0 || float.IsNaN(feet)))
            {
                var bb = b.sr.bounds; if (w <= 0) w = bb.size.x * ppu * 0.7f; if (h <= 0) h = bb.size.y * ppu * 0.85f; if (float.IsNaN(feet)) feet = (bb.min.y - target.position.y) * ppu;
            }
            b.w = w > 0 ? w : 8; b.h = h > 0 ? h : 8; b.feet = float.IsNaN(feet) ? -7 : feet;
            b.life = seconds > 0 ? seconds : float.PositiveInfinity; b.interval = interval; b.nextTick = interval > 0 ? interval : float.PositiveInfinity;
            b.dmg = dmg; b.lastTick = -9; b.seed = H(i, g) * 7f;
            return Handle(i);
        }
        public void TickNow(int h, int dmg) { if (Get(h, out int i)) Pulse(ref _b[i], dmg); }
        void Pulse(ref B b, int dmg)
        {
            b.lastTick = b.t; b.numVal = dmg; b.ticks++;
            if (BurnFX.TickTint && b.sr && !b.tinted) { b.tint0 = b.sr.color; var c = BurnFX.TickTintColor; b.sr.color = new Color(b.tint0.r * c.r, b.tint0.g * c.g, b.tint0.b * c.b, b.tint0.a); b.tinted = true; b.tintT = b.t; }
        }
        void Untint(ref B b) { if (b.tinted) { if (b.sr) b.sr.color = b.tint0; b.tinted = false; } }
        public void End(int h, bool smoke) { if (Get(h, out int i)) Begin(ref _b[i], smoke, i); }
        void Begin(ref B b, bool smoke, int i)
        {
            if (b.ending) return; b.ending = true; b.smoke = smoke; b.endT = b.t; Untint(ref b);
            if (!smoke) Free(i);
        }
        void Free(int i) { ref var b = ref _b[i]; Untint(ref b); b.state = 0; b.tr = null; b.sr = null; _free[_freeN++] = i; _count--; }
        public void ClearAll() { for (int i = 0; i < _b.Length; i++) if (_b[i].state != 0) Free(i); }

        static int FrameAt(float t)
        {
            float loop = 0; for (int k = 0; k < 6; k++) loop += FrameDur[k];
            float x = t % loop; for (int k = 0; k < 6; k++) { if (x < FrameDur[k]) return k; x -= FrameDur[k]; }
            return 5;
        }
        static Color32 A(Color32 c, float a) { c.a = (byte)(c.a * Mathf.Clamp01(a)); return c; }

        void LateUpdate()
        {
            if (_back == null) return; float dt = Time.deltaTime; bool lod = _count > BurnFX.LodThreshold; float ppu = HellFX.PPU;
            for (int i = 0; i < _b.Length; i++)
            {
                ref var b = ref _b[i]; if (b.state == 0) continue;
                b.t += dt;
                if (!b.ending)
                {
                    if (!b.tr) Begin(ref b, true, i);
                    else
                    {
                        b.last = b.tr.position;
                        while (b.t >= b.nextTick && b.nextTick <= b.life)
                        {
                            Pulse(ref b, b.dmg); b.nextTick += b.interval;
                            BurnFX.OnTick?.Invoke(Handle(i), b.tr, b.dmg);
                            if (b.state == 0 || b.ending) break;
                        }
                        if (b.t >= b.life) Begin(ref b, true, i);
                    }
                    if (b.state == 0) continue;
                }
                if (b.tinted && b.t - b.tintT >= TintT) Untint(ref b);
                Draw(ref b, i, lod, ppu);
                if (b.ending && b.t - b.endT >= EndShrinkT + SmokeT) Free(i);
            }
            _back.Upload(); _front.Upload();
        }

        void Draw(ref B b, int i, bool lod, float ppu)
        {
            int fx = Mathf.RoundToInt(b.last.x * ppu), fy = Mathf.RoundToInt(b.last.y * ppu + b.feet);
            int top = fy + Mathf.RoundToInt(b.h), hw = Mathf.Max(2, Mathf.RoundToInt(b.w * 0.5f));
            float et = b.ending ? b.t - b.endT : 0; bool flames = !b.ending || et < EndShrinkT;
            bool flare = !b.ending && b.t - b.lastTick < FlareT;
            if (flames)
            {
                int n = lod ? 2 : Mathf.Clamp(Mathf.RoundToInt(b.w / 5f), 2, 4); bool shrink = b.ending;
                for (int j = 0; j < n; j++)
                {
                    int x = fx + Mathf.RoundToInt(Mathf.Lerp(-hw * 0.55f, hw * 0.55f, n == 1 ? 0.5f : j / (float)(n - 1)));
                    bool small = shrink || (n >= 3 && (j == 0 || j == n - 1));
                    float ph = b.t + b.seed + j * 0.23f; int fr = FrameAt(ph);
                    int cell = small ? (flare ? CSmallHot : CSmall) + (fr & 3) : (flare ? CBigHot : CBig) + fr;
                    int y = top - (small ? 3 : 4) + (flare ? 1 : 0) - (shrink ? Mathf.RoundToInt(et / EndShrinkT * 4) : 0);
                    _back.Add(cell, x, y, Wh);
                }
                if (b.h >= 10 && !shrink) // shoulder licks: half outside the silhouette, so only the outer half shows
                {
                    int ys = fy + Mathf.RoundToInt(b.h * 0.55f) - 3 + (flare ? 1 : 0);
                    for (int s = -1; s <= 1; s += 2) { int fr = FrameAt(b.t + b.seed + (s > 0 ? 0.37f : 0.11f)); _back.Add((flare ? CSmallHot : CSmall) + (fr & 3), fx + s * hw, ys, Wh); }
                }
            }
            if (!b.ending)
            {
                // small flames ON the body (front): 2 tiny 3x3 licks that hop between hashed spots on the upper 2/3 every ~0.25 s;
                // they cover <= 18 px of a 25-41 px enemy and never the whole sprite. A 3rd lick on the far edge when not in LOD.
                int nb = lod ? 1 : (b.h >= 20 ? 3 : 2);
                for (int j = 0; j < nb; j++)
                {
                    float q = b.t * 4f + b.seed + j * 1.7f; int cyc = (int)q; if (q - cyc > 0.8f) continue;
                    int x = fx + Mathf.RoundToInt((H(i * 5 + j, cyc) - 0.5f) * 1.6f * hw), y = fy + Mathf.RoundToInt(b.h * (0.3f + 0.55f * H(i * 5 + j, cyc + 999)));
                    _front.Add(CTiny + (FrameAt(b.t + j * 0.2f + b.seed) % 3), x, y, flare ? Wh : new Color32(255, 255, 255, 230));
                }
                int ne = lod ? 1 : 3;
                for (int j = 0; j < ne; j++)
                {
                    float q = b.t / EmberPeriod + j / (float)ne + b.seed; int cyc = (int)q; float p = q - cyc; if (p > 0.9f) continue;
                    int x = fx + Mathf.RoundToInt((H(i * 7 + j, cyc) - 0.5f) * 2f * hw + Mathf.Sin(p * 6.3f + j) * 1.5f), y = top + 1 + Mathf.RoundToInt(p * 14);
                    _front.Px(x, y, p < 0.3f ? Yc : p < 0.6f ? Oc : Rc);
                }
                float st = b.t - b.lastTick;
                if (st < SparkT)
                {
                    float k = st / SparkT; int d = 2 + Mathf.RoundToInt(k * 5);
                    Color32 sc = k < 0.4f ? Wh : Yc;
                    _front.Px(fx - d, top + 1 + d / 2, sc); _front.Px(fx + d, top + 1 + d / 2, sc);
                    _front.Px(fx - d / 2, top + 2 + d, sc); _front.Px(fx + d / 2, top + 2 + d, sc);
                }
                if (BurnFX.ShowNumbers && st < NumT && b.numVal > 0)
                {
                    float k = st / NumT; int rise = Mathf.RoundToInt((1 - (1 - k) * (1 - k)) * 8);
                    int side = (b.ticks & 1) == 0 ? 1 : -1, v = Mathf.Min(b.numVal, 999), nd = v >= 100 ? 3 : v >= 10 ? 2 : 1;
                    int x0 = fx + side * 3 - (nd * 4 - 1) / 2 + 1, y0 = top + 5 + rise;
                    Color32 col = st < 0.067f ? Wh : Gc; if (k > 0.8f && (((int)(st * 30)) & 1) == 1) col.a = 0;
                    for (int dgi = 0; dgi < nd; dgi++) { int div = nd - 1 - dgi == 2 ? 100 : nd - 1 - dgi == 1 ? 10 : 1; _front.Add(CDigit + (v / div) % 10, x0 + dgi * 4, y0, col); }
                }
            }
            else if (et >= 0.05f)
            {
                for (int j = 0; j < 3; j++)
                {
                    float s = et - 0.05f - j * 0.08f; if (s < 0) continue; float k = s / (SmokeT - 0.2f); if (k >= 1) continue;
                    int x = fx + (j == 0 ? -2 : j == 1 ? 2 : 0) + Mathf.RoundToInt(Mathf.Sin(s * 9 + j) * 1), y = top - 1 + Mathf.RoundToInt(k * 14);
                    if (k < 0.45f) _front.Add(CSmokeB, x, y, Wh); else if (k < 0.8f) _front.Add(CSmokeS, x, y, Wh); else _front.Px(x, y, new Color32(0x5a, 0x69, 0x88, 255));
                }
            }
        }
    }
}
