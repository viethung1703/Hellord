using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// #147-150 InteractFX (v2.4.0, v2.4.1 + v2.4.2 fixes): elite marker, use-spot proximity pulse, chest / shrine / shop open bursts.
    /// Reward language only (cyan #2ce8f5 + gold #feae34/#fee761 + white, dark outlines, purple/brown chest art colours). Never a hazard tell:
    /// no red or orange, no filled areas, no chevrons, no blink-to-strike, nothing above 3 Hz, no full-screen flash. Pooled, int handles (-1 when full).
    /// 147 EliteMark   persistent beacon above an elite (v2.4.1: always lit, readable between ripples): 32x30 plate with a steady 3 px cyan border + dark outline, a soft cyan halo behind it,
    ///                 a 3 px cyan pin down to the head, and a steady cyan 3 px ring on the ground under the elite. Plate shows the purple chest + cyan gem (the game's elite-chest art) = "drops a chest".
    ///                 v2.4.2: fully steady (no bob, no sawtooth ripple, no sparkles); the only motion is a 0.5 Hz smooth sine on the outer ring alpha (floor 67%). Every ring and halo draws on the
    ///                 ground layer (under characters, -20000), so nothing tints a sprite; only the plate + pin are above. Follow the enemy with MoveMark; Kill on death.
    /// 148 UseSpot     persistent at a chest / shrine / shop use-spot (centre of HA's 30x16 spot). Far: 3 slow cyan motes. Hero within ApproachPx: a cyan 3 px ground ring fades in.
    ///                 Hero inside the zone: ring turns white-cyan, a steady outer ring (0.5 Hz sine breathing, no sawtooth ripple in v2.4.2) and a dark plate with a kind glyph (chest / rune eye / coin) rises above the spot.
    ///                 Ground parts (ring, outer ring, halo, motes) sort UNDER characters (GroundSortingOrder -20000), the plate sorts above. Feed the hero with SetHero(feet) each frame.
    ///                 v2.4.1: the plate height is lifted per kind (PlateLiftPx / SetPlateLift) or per call (UseSpot liftPx) so it clears tall props (HA shrine statue, shop awning).
    ///                 v2.4.2: defaults lowered so the plate touches the prop (shrine nests in the statue's raised hands, shop sits on the awning); plates no longer bob.
    /// 149 ChestOpen   1.2 s: small white pop, 8-10 gold rays, gold ring (+ cyan ring for elite), 8 gold coins (elite: 12 + 4 cyan gems) fountain and land, sparkles. OnOpened at 0.12 s.
    /// 150 ShrineUsed / Purchase  1.0 s / 0.9 s: shrine = cyan ring + 6 orbiting rune diamonds + rising motes + white star; shop = 5 gold coins + gold ring + 2 sparkles.
    /// </summary>
    public enum UseKind { Chest = 0, Shrine = 1, Shop = 2 }

    public static class InteractFX
    {
        public static int SortingOrder = 70; public static string SortingLayer = "Default";
        public static int GroundSortingOrder = -20000; public static string GroundSortingLayer = "Default";
        public static Material MaterialOverride; public static int MaxItems = 64;
        /// <summary>Hero feet within this horizontal radius (ground ellipse, 0.6 tall) = approach (ring fades in). Zone = HA's interact_zone_px 30x16.</summary>
        public static float ApproachPx = 40f, ZoneWidthPx = 30f, ZoneHeightPx = 16f;
        /// <summary>Height of the elite sprite above its feet in px (wraith 41); the marker plate floats 4 px above it.</summary>
        public static float MarkHeadPx = 42f;
        /// <summary>Extra plate height in px above the default 32 px (plate centre = spot centre + 32 + lift), per kind. Defaults measured on HA's step12 props with the spot centre as reference:
        /// Chest 0 (chest top ~15 px), Shrine 73 (v2.4.2: plate bottom edge = 94 px above the spot centre = the row where the statue's raised arms are 25 px apart, so the 24 px plate nests between the hands; the skull top is 4 px under it), Shop 64 (v2.4.2: plate bottom = 85 px above the spot centre = top edge of the awning).</summary>
        public static float[] PlateLiftPx = { 0f, 73f, 64f };
        /// <summary>Change the default plate lift for one kind (e.g. when the props change size).</summary>
        public static void SetPlateLift(UseKind kind, float px) { PlateLiftPx[(int)kind] = px; }
        /// <summary>(kind 0 chest, 1 elite chest, 2 shrine, 3 shop; pos) at 0.12 s: spawn the real pickups, play the SFX, swap the chest sprite.</summary>
        public static event Action<int, Vector2> OnOpened;
        static InteractRunner R => InteractRunner.I;
        public static void Prewarm() => R.Prewarm();
        /// <summary>Move the under-characters ground parts (ring, outer ring, halo, motes) to another layer / order, e.g. just above your floor decor.</summary>
        public static void SetGroundSorting(string layerName, int order) { GroundSortingLayer = layerName; GroundSortingOrder = order; R.ApplySorting(); }
        /// <summary>Hero feet in world units, every frame (drives the use-spot pulse).</summary>
        public static void SetHero(Vector2 feet) => R.SetHero(feet * HellFX.PPU);
        /// <summary>Elite marker (#147). feet = the elite's feet in world units. Returns a handle, -1 if the pool is full.</summary>
        public static int EliteMark(Vector2 feet) => R.Spawn(0, feet * HellFX.PPU, float.MaxValue, 0);
        /// <summary>Call every frame while the elite moves.</summary>
        public static void MoveMark(int h, Vector2 feet) => R.Move(h, feet * HellFX.PPU);
        /// <summary>Use-spot pulse (#148) at the centre of the spot. Persistent until Kill (room unload, or after the chest / shrine is used).</summary>
        /// <param name="liftPx">extra plate height in px (screen px, not world units); NaN (default) = PlateLiftPx[kind].</param>
        public static int UseSpot(Vector2 centre, UseKind kind, float liftPx = float.NaN) => R.Spawn(1, centre * HellFX.PPU, float.MaxValue, (int)kind, float.IsNaN(liftPx) ? PlateLiftPx[(int)kind] : liftPx);
        /// <summary>True while the hero is inside the use zone (gate your Interact input on this if you like).</summary>
        public static bool InRange(int h) => R.InRange(h);
        /// <summary>Chest open burst (#149). pos = chest centre (world units). Hide / swap the closed chest at the call.</summary>
        public static int OpenChest(Vector2 pos, bool elite = false) => R.Spawn(2, pos * HellFX.PPU, elite ? 1.3f : 1.2f, elite ? 1 : 0);
        /// <summary>Shrine used burst (#150).</summary>
        public static int ShrineUsed(Vector2 pos) => R.Spawn(2, pos * HellFX.PPU, 1.0f, 2);
        /// <summary>Shop purchase burst (#150).</summary>
        public static int Purchase(Vector2 pos) => R.Spawn(2, pos * HellFX.PPU, 0.9f, 3);
        internal static void Opened(int kind, Vector2 p) => OnOpened?.Invoke(kind, p);
        public static void Kill(int h) => R.Kill(h);
        public static void Clear() => R.ClearAll();
    }

    public class InteractRunner : MonoBehaviour
    {
        static InteractRunner _i;
        public static InteractRunner I { get { if (_i == null) { var go = new GameObject("InteractFX"); DontDestroyOnLoad(go); _i = go.AddComponent<InteractRunner>(); } return _i; } }
        struct It { public bool used, fired, inZone; public byte k; public int sub; public Vector2 p; public float life, t, near, ready, seed, lift; public ushort gen; }
        It[] _it; int[] _free; int _freeN;
        Vector2 _hero; bool _heroSet;
        struct Q { public float x, y, w, h; public Color32 c; }
        class Layer
        {
            public Q[] q; public int n, last; public Mesh mesh; public Vector3[] vtx; public Color32[] col; public MeshRenderer mr;
            public Layer(int cap, GameObject go, string name, Material m, int order, string layer)
            {
                q = new Q[cap]; vtx = new Vector3[cap * 4]; col = new Color32[cap * 4]; var uv = new Vector2[cap * 4]; var tri = new int[cap * 6];
                for (int i = 0; i < cap; i++) { int v = i * 4, t = i * 6; tri[t] = v; tri[t + 1] = v + 1; tri[t + 2] = v + 2; tri[t + 3] = v; tri[t + 4] = v + 2; tri[t + 5] = v + 3;
                    uv[v] = new Vector2(.25f, .25f); uv[v + 1] = new Vector2(.25f, .75f); uv[v + 2] = new Vector2(.75f, .75f); uv[v + 3] = new Vector2(.75f, .25f); }
                mesh = new Mesh { name = name }; mesh.MarkDynamic(); mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                go.AddComponent<MeshFilter>().sharedMesh = mesh; mr = go.AddComponent<MeshRenderer>(); if (m != null) mr.sharedMaterial = m;
                mr.sortingOrder = order; mr.sortingLayerName = layer; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                mesh.vertices = vtx; mesh.uv = uv; mesh.colors32 = col; mesh.triangles = tri;
            }
            public void Upload(float u)
            {
                for (int b = 0; b < n; b++)
                {
                    int v = b * 4; ref var s = ref q[b]; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
                    vtx[v] = new Vector3(x0, y0, 0); vtx[v + 1] = new Vector3(x0, y1, 0); vtx[v + 2] = new Vector3(x1, y1, 0); vtx[v + 3] = new Vector3(x1, y0, 0);
                    col[v] = col[v + 1] = col[v + 2] = col[v + 3] = s.c;
                }
                for (int z = n; z < last; z++) { int v = z * 4; vtx[v] = vtx[v + 1] = vtx[v + 2] = vtx[v + 3] = Vector3.zero; }
                mesh.vertices = vtx; mesh.colors32 = col; mesh.RecalculateBounds(); last = n; n = 0; mr.enabled = last > 0;
            }
        }
        Layer _top, _gnd; bool _g;
        const int TopQ = 3072, GndQ = 4096;
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Pl = new Color32(0x26, 0x2b, 0x44, 255), Wh = new Color32(255, 255, 255, 255),
            Y = new Color32(0xfe, 0xe7, 0x61, 255), Go = new Color32(0xfe, 0xae, 0x34, 255), Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Cb = new Color32(0x00, 0x99, 0xdb, 255),
            Pp = new Color32(0x68, 0x38, 0x6c, 255), Pk = new Color32(0xb5, 0x50, 0x88, 255), Kp = new Color32(0x3e, 0x21, 0x37, 255), Br = new Color32(0x73, 0x3e, 0x39, 255), Tn = new Color32(0xb8, 0x6f, 0x50, 255);
        static readonly string[] IconElite = { "...CC...", ".LLLLLL.", "LPPPPPPL", "GGGGGGGG", "PPPYYPPP", "PPPKKPPP", "KKKKKKKK" };
        static readonly string[] IconChest = { "........", ".TTTTTT.", "TTTTTTTT", "GGGGGGGG", "BBBYYBBB", "BBBKKBBB", "KKKKKKKK" };
        static readonly string[] IconShrine = { "...CC...", "..CCCC..", ".CCWWCC.", "CCWKKWCC", ".CCWWCC.", "..CCCC..", "...CC..." };
        static readonly string[] IconShop = { "..GGGG..", ".GYYYYG.", "GYYKKYYG", "GYYKKYYG", "GYYKKYYG", ".GYYYYG.", "..GGGG.." };

        public void Prewarm()
        {
            if (_top != null) return;
            int cap = Mathf.Clamp(InteractFX.MaxItems, 8, 4096); _it = new It[cap]; _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) _free[_freeN++] = i;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Interact White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = InteractFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "InteractFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] InteractFX: no sprite shader found; set InteractFX.MaterialOverride.");
            _top = new Layer(TopQ, gameObject, "InteractFX", m, InteractFX.SortingOrder, InteractFX.SortingLayer);
            var g = new GameObject("InteractFX Ground"); g.transform.SetParent(transform, false);
            _gnd = new Layer(GndQ, g, "InteractFX Ground", m, InteractFX.GroundSortingOrder, InteractFX.GroundSortingLayer);
        }
        public void ApplySorting()
        {
            if (_top == null) return;
            _top.mr.sortingLayerName = InteractFX.SortingLayer; _top.mr.sortingOrder = InteractFX.SortingOrder;
            _gnd.mr.sortingLayerName = InteractFX.GroundSortingLayer; _gnd.mr.sortingOrder = InteractFX.GroundSortingOrder;
        }
        public void SetHero(Vector2 p) { _hero = p; _heroSet = true; }
        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && _it != null && i < _it.Length && _it[i].used && _it[i].gen == (ushort)(h >> 16); }
        public int Spawn(byte k, Vector2 p, float life, int sub, float lift = 0f)
        {
            if (_top == null) Prewarm();
            if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ushort g = (ushort)(_it[i].gen + 1);
            _it[i] = new It { used = true, k = k, p = p, life = life, sub = sub, lift = lift, gen = g, seed = (i * 0.618f + Time.frameCount * 0.31f) % 1f };
            return Hd(i);
        }
        public void Move(int h, Vector2 p) { if (Get(h, out int i)) _it[i].p = p; }
        public bool InRange(int h) => Get(h, out int i) && _it[i].inZone;
        public void Kill(int h) { if (!Get(h, out int i)) return; _it[i].used = false; _free[_freeN++] = i; }
        public void ClearAll() { if (_it == null) return; for (int i = 0; i < _it.Length; i++) if (_it[i].used) { _it[i].used = false; _free[_freeN++] = i; } _top.n = 0; _gnd.n = 0; }

        /* primitives (pixel units, snapped); _g selects the under-characters layer */
        void A(float x, float y, float w, float h, Color32 c)
        {
            if (c.a == 0) return; var L = _g ? _gnd : _top; if (L.n >= L.q.Length) return;
            L.q[L.n++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c };
        }
        void Disc(float cx, float cy, int r, Color32 c) { float x0 = Mathf.Round(cx), y0 = Mathf.Round(cy); for (int dy = -r; dy <= r; dy++) { int hw = Mathf.RoundToInt(Mathf.Sqrt(r * r - dy * dy + 0.25f)); A(x0 - hw, y0 + dy, hw * 2, 1, c); } }
        void Dot(float cx, float cy, int s, Color32 c) => A(Mathf.Round(cx) - (s >> 1), Mathf.Round(cy) - (s >> 1), s, s, c);
        static Color32 Fa(Color32 c, float a) { c.a = (byte)(Mathf.Clamp01(a) * 255); return c; }
        static Color32 Mix(Color32 a, Color32 b, float t) => Color32.Lerp(a, b, Mathf.Clamp01(t));
        static float H(float x) { x = Mathf.Sin(x * 127.1f + 311.7f) * 43758.5453f; return x - Mathf.Floor(x); }
        /* ring: 3 px (or s) dots on an ellipse with a dark 5 px backing; step ~2 px along the arc */
        void Ring(Vector2 c, float rx, float ys, int s, Color32 col, float backA)
        {
            if (rx < 1 || col.a == 0) return; int n = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI * rx * (0.5f + 0.5f * ys) * 2f / 2f), 8, 200);
            if (backA > 0) for (int j = 0; j < n; j++) { float a = 2 * Mathf.PI * j / n; Dot(c.x + Mathf.Cos(a) * rx, c.y + Mathf.Sin(a) * rx * ys, s + 2, Fa(Ko, backA * col.a / 255f)); }
            for (int j = 0; j < n; j++) { float a = 2 * Mathf.PI * j / n; Dot(c.x + Mathf.Cos(a) * rx, c.y + Mathf.Sin(a) * rx * ys, s, col); }
        }
        void Line(Vector2 a, Vector2 b, int s, Color32 col) { float L = (b - a).magnitude; int n = Mathf.Max(1, Mathf.CeilToInt(L / 2f)); for (int j = 0; j <= n; j++) { var p = Vector2.Lerp(a, b, j / (float)n); Dot(p.x, p.y, s, col); } }
        /* plus sparkle: 3x3 core, 3x3 arms at size 1 (9x9 total), core only at size 0.5 */
        void Spark(Vector2 p, float k, Color32 col)
        {
            if (k <= 0.05f) return; Color32 o = Fa(Ko, 0.8f);
            if (k > 0.6f) { Dot(p.x, p.y, 5, o); for (int j = 0; j < 4; j++) { int dx = (j & 1) == 0 ? (j == 0 ? 3 : -3) : 0, dy = (j & 1) == 1 ? (j == 1 ? 3 : -3) : 0; Dot(p.x + dx, p.y + dy, 5, o); } for (int j = 0; j < 4; j++) { int dx = (j & 1) == 0 ? (j == 0 ? 3 : -3) : 0, dy = (j & 1) == 1 ? (j == 1 ? 3 : -3) : 0; Dot(p.x + dx, p.y + dy, 3, col); } Dot(p.x, p.y, 3, Wh); }
            else { Dot(p.x, p.y, 5, o); Dot(p.x, p.y, 3, col); }
        }
        Color32 IconCol(char ch)
        {
            switch (ch) { case 'C': return Cy; case 'L': return Pk; case 'P': return Pp; case 'G': return Go; case 'Y': return Y; case 'K': return Kp; case 'W': return Wh; case 'T': return Tn; case 'B': return Br; default: return default; }
        }
        /* icon: 8x7 cells of 2 px on a dark plate with a 1 px darker outline. alpha fades the whole plate. */
        void Plate(Vector2 c, string[] ic, float al)
        {
            float x = Mathf.Round(c.x) - 11, y = Mathf.Round(c.y) - 10; if (al <= 0.01f) return;
            A(x - 1, y - 1, 24, 22, Fa(Ko, al)); A(x, y, 22, 20, Fa(Pl, al));
            for (int r = 0; r < 7; r++) for (int col = 0; col < 8; col++)
            { char ch = ic[r][col]; if (ch == '.') continue; Color32 cc = IconCol(ch); if (ic == IconShrine && ch == 'K') cc = Ko; if (ic == IconShop && ch == 'K') cc = Ko; cc.a = (byte)(al * 255); A(x + 3 + col * 2, y + 3 + (6 - r) * 2, 2, 2, cc); }
        }

        void LateUpdate()
        {
            if (_top == null) return;
            float dt = Time.unscaledDeltaTime, u = 1f / HellFX.PPU;
            for (int i = 0; i < _it.Length; i++)
            {
                if (!_it[i].used) continue; ref var it = ref _it[i]; it.t += dt;
                if (it.k == 2)
                {
                    if (!it.fired && it.t >= 0.12f) { it.fired = true; InteractFX.Opened(it.sub, it.p * u); }
                    if (it.t >= it.life) { Kill(Hd(i)); continue; }
                }
                else if (it.t >= it.life) { Kill(Hd(i)); continue; }
                switch (it.k)
                {
                    case 0: _g = false; DrawMark(ref it); break;
                    case 1: DrawSpot(ref it, dt); break;
                    case 2: _g = false; DrawBurst(ref it); break;
                }
            }
            _top.Upload(u); _gnd.Upload(u);
        }

        /* v2.4.1 elite beacon: steady (never off), all rings and halos on the ground layer; only plate + pin are above. 32x30 px: 1 px dark outline, 3 px cyan border, 24x22 dark plate, 16x14 icon. */
        void DrawMark(ref It it)
        {
            float t = it.t; Vector2 p = it.p; float al = Mathf.Clamp01(t / 0.2f), rise = (1f - al) * 6f;
            Vector2 c = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y + InteractFX.MarkHeadPx + 8 + 15 - rise));
            float br = 0.6f + 0.12f * Mathf.Sin((t * 0.5f + it.seed) * 6.2832f);
            _g = true;
            Ring(new Vector2(p.x, p.y - 1), 11, 0.45f, 3, Fa(Cy, 0.9f * al), 0.55f);
            Disc(c.x, c.y, 24, Fa(Cy, 0.13f * al)); Disc(c.x, c.y, 19, Fa(Cy, 0.17f * al));
            Ring(c, 29, 0.8f, 3, Fa(Cy, br * al), 0.5f);
            _g = false;
            Line(new Vector2(c.x, c.y - 14), new Vector2(p.x, p.y + InteractFX.MarkHeadPx + 1), 5, Fa(Ko, al)); Line(new Vector2(c.x, c.y - 14), new Vector2(p.x, p.y + InteractFX.MarkHeadPx + 1), 3, Fa(Cy, al));
            float x = c.x - 16, y = c.y - 15;
            A(x, y, 32, 30, Fa(Ko, al)); A(x + 1, y + 1, 30, 28, Fa(Cy, al)); A(x + 4, y + 4, 24, 22, Fa(Pl, al));
            for (int r = 0; r < 7; r++) for (int col = 0; col < 8; col++)
            { char ch = IconElite[r][col]; if (ch == '.') continue; Color32 cc = IconCol(ch); cc.a = (byte)(al * 255); A(x + 8 + col * 2, y + 8 + (6 - r) * 2, 2, 2, cc); }
        }

        void DrawSpot(ref It it, float dt)
        {
            Vector2 p = it.p; float t = it.t; float zw = InteractFX.ZoneWidthPx * 0.5f + 4f, zh = InteractFX.ZoneHeightPx * 0.5f + 4f;
            bool near = false, zone = it.inZone;
            if (_heroSet)
            {
                float ax = InteractFX.ApproachPx, dx = _hero.x - p.x, dy = _hero.y - p.y;
                near = (dx * dx) / (ax * ax) + (dy * dy) / (ax * ax * 0.36f) <= 1f;
                float d2 = (dx * dx) / (zw * zw) + (dy * dy) / (zh * zh); zone = it.inZone ? d2 <= 1.3f : d2 <= 1f;
            }
            it.inZone = zone; if (zone) near = true;
            it.near = Mathf.MoveTowards(it.near, near ? 1f : 0f, dt * 3f); it.ready = Mathf.MoveTowards(it.ready, zone ? 1f : 0f, dt * 3f);
            float rx = zw + 2f, ys = (zh + 1f) / rx;
            _g = true;
            if (it.near < 0.99f) for (int j = 0; j < 3; j++) { float k = (t * 0.5f + j / 3f + it.seed) % 1f; float mx = p.x + (j - 1) * 9 + Mathf.Sin(t + j * 2f) * 2f, my = p.y - 2 + 14 * k;
                float a = Mathf.Sin(k * Mathf.PI) * 0.6f * (1f - it.near); Dot(mx, my, 5, Fa(Ko, a * 0.6f)); Dot(mx, my, 3, Fa(Cy, a)); }
            if (it.near > 0.02f)
            {
                float br = 0.72f + 0.10f * Mathf.Sin(t * 0.5f * 6.2832f); float a = it.near * Mathf.Lerp(br, 1f, it.ready);
                Ring(p, rx, ys, 3, Fa(Mix(Cy, Wh, it.ready * 0.6f), a), 0.6f);
                if (it.ready > 0.02f) Ring(p, rx + 8, ys, 3, Fa(Cy, (0.55f + 0.12f * Mathf.Sin((t * 0.5f + 0.5f) * 6.2832f)) * it.ready), 0.4f);
            }
            _g = false;
            if (it.ready > 0.02f)
            {
                string[] ic = it.sub == 1 ? IconShrine : it.sub == 2 ? IconShop : IconChest;
                Plate(new Vector2(p.x, p.y + 32 + it.lift - (1f - it.ready) * 6f), ic, it.ready);
            }
        }

        void Coins(Vector2 p, float t, int n, float seed, int gemEvery, float speed)
        {
            for (int j = 0; j < n; j++)
            {
                float h = H(seed + j * 1.7f), h2 = H(seed + j * 3.1f + 5f), t0 = 0.06f + 0.05f * j / n, tt = t - t0; if (tt <= 0) continue;
                float ang = (-65f + 130f * (j + 0.5f * h) / n) * Mathf.Deg2Rad, vx = Mathf.Sin(ang) * (55f + 25f * h2) * speed, vy = (95f + 35f * h) * speed, g = 280f;
                float gy = p.y - 5f - 7f * h2, tl = (vy + Mathf.Sqrt(vy * vy + 2 * g * (p.y + 4 - gy))) / g, te = Mathf.Min(tt, tl);
                float x = p.x + vx * te, y = Mathf.Max(gy, p.y + 4 + vy * te - 0.5f * g * te * te);
                float fade = Mathf.Clamp01((0.95f - t) / 0.2f); if (fade <= 0) continue;
                bool gem = gemEvery > 0 && j % gemEvery == 0;
                if (gem) { Dot(x, y, 7, Fa(Ko, fade)); Dot(x, y, 5, Fa(Cy, fade)); Dot(x - 1, y + 1, 3, Fa(Wh, fade)); }
                else { bool wide = (((int)(tt * 6f + h * 4f)) & 1) == 0 || tt >= tl; int w = wide ? 5 : 3; A(x - (w + 2) * 0.5f, y - 3.5f, w + 2, 7, Fa(Ko, fade)); A(x - w * 0.5f, y - 2.5f, w, 5, Fa(j % 2 == 0 ? Go : Y, fade)); if (wide) Dot(x, y, 3, Fa(Y, fade)); }
            }
        }

        void DrawBurst(ref It it)
        {
            float t = it.t; Vector2 p = it.p; int s = it.sub;
            if (s == 0 || s == 1)
            {
                bool el = s == 1;
                if (t < 0.1f) { float a = 1f - t / 0.1f; Disc(p.x, p.y + 6, 11, Fa(Wh, a * 0.5f)); Disc(p.x, p.y + 6, 7, Fa(Wh, a * 0.9f)); }
                int rn = el ? 10 : 8;
                if (t >= 0.02f && t < 0.5f)
                {
                    float k = (t - 0.02f) / 0.48f, e = 1f - (1f - k) * (1f - k), al = 1f - k;
                    for (int j = 0; j < rn; j++)
                    {
                        float a = (360f / rn * j + 11f) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.8f), c0 = p + new Vector2(0, 6);
                        Line(c0 + d * (9 + 6 * e), c0 + d * (12 + 24 * e), 5, Fa(Ko, al * 0.6f)); Line(c0 + d * (9 + 6 * e), c0 + d * (12 + 24 * e), 3, Fa(j % 2 == 0 ? Y : Wh, al));
                    }
                }
                if (t >= 0.04f && t < 0.44f) { float k = (t - 0.04f) / 0.4f; Ring(p + new Vector2(0, 4), 6 + 30 * Mathf.Sqrt(k), 0.6f, 3, Fa(Y, 1f - k), 0.6f); }
                if (el && t >= 0.14f && t < 0.6f) { float k = (t - 0.14f) / 0.46f; Ring(p + new Vector2(0, 4), 4 + 38 * Mathf.Sqrt(k), 0.6f, 3, Fa(Cy, 1f - k), 0.6f); }
                Coins(p, t, el ? 12 : 8, it.seed * 9f, el ? 3 : 0, 1f);
                for (int j = 0; j < (el ? 6 : 4); j++) { float st = 0.15f + 0.1f * j, k = (t - st) / 0.45f; if (k < 0 || k > 1) continue; float h = H(it.seed * 5f + j); Spark(p + new Vector2((j - 2.5f) * 10f + h * 4f, 14 + 16 * h + 14 * k), Mathf.Sin(k * Mathf.PI), j % 2 == 0 ? Y : Cy); }
            }
            else if (s == 2)
            {
                if (t < 0.2f) { float k = t / 0.2f; Spark(p + new Vector2(0, 10), 1f - k * 0.3f, Wh); Disc(p.x, p.y + 10, 3 + (int)(4 * k), Fa(Wh, (1 - k) * 0.7f)); }
                Ring(p + new Vector2(0, 2), 6 + 30 * Mathf.Sqrt(Mathf.Clamp01(t / 0.5f)), 0.6f, 3, Fa(Cy, Mathf.Clamp01(1f - t / 0.5f)), 0.6f);
                if (t > 0.1f && t < 0.6f) Ring(p + new Vector2(0, 2), 4 + 24 * Mathf.Sqrt((t - 0.1f) / 0.5f), 0.6f, 3, Fa(Cb, 1f - (t - 0.1f) / 0.5f), 0.6f);
                for (int j = 0; j < 6; j++)
                {
                    float k = Mathf.Clamp01((t - 0.05f) / 0.7f), al = 1f - k * k, a = (60f * j + 90f * k) * Mathf.Deg2Rad, rr = 8 + 26 * Mathf.Sqrt(k);
                    Vector2 q = p + new Vector2(Mathf.Cos(a) * rr, 6 + Mathf.Sin(a) * rr * 0.6f + 8 * k); Dot(q.x, q.y, 7, Fa(Ko, al * 0.8f)); Dot(q.x, q.y, 5, Fa(Cy, al)); Dot(q.x, q.y, 3, Fa(Wh, al));
                }
                for (int j = 0; j < 8; j++) { float k = (t * 1.2f + j / 8f) % 1f; if (t > 0.8f) break; Dot(p.x + (j - 3.5f) * 4f, p.y + 2 + 30 * k, 5, Fa(Ko, (1 - k) * 0.5f)); Dot(p.x + (j - 3.5f) * 4f, p.y + 2 + 30 * k, 3, Fa(j % 2 == 0 ? Cy : Wh, 1 - k)); }
            }
            else
            {
                if (t < 0.4f) Ring(p + new Vector2(0, 4), 4 + 18 * Mathf.Sqrt(t / 0.4f), 0.6f, 3, Fa(Y, 1f - t / 0.4f), 0.6f);
                Coins(p, t, 5, it.seed * 7f, 0, 0.75f);
                for (int j = 0; j < 2; j++) { float k = (t - 0.1f - 0.12f * j) / 0.4f; if (k < 0 || k > 1) continue; Spark(p + new Vector2(j == 0 ? -12 : 13, 16 + 8 * j + 10 * k), Mathf.Sin(k * Mathf.PI), j == 0 ? Y : Wh); }
            }
        }
    }
}
