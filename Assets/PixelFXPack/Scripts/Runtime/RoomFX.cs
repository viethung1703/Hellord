using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// #142-146 RoomFX (v2.3.0): nest destroyed + stairs / descend. Reward and exit language (gold #feae34/#fee761 + cyan #2ce8f5, dark outlines), never a hazard tell:
    /// no orange fills, no crawling borders, nothing closes in on the player, nothing above 3 Hz, no full-screen white. Pooled, one draw call, int handles (-1 when full).
    /// 142 NestCollapse  0.45 s: 10 outlined debris chunks drop and settle onto the nest, then a gold ring pops outward with 6 gold sparks. OnNestCollapsed at 0.2 s.
    /// 143 NestDead      persistent INACTIVE marker (until Kill): cold slate broken seal (3 px ring with 2 gaps + a crack) and 3 cold grey embers drifting up.
    ///                   Dull on purpose: reads as "off", so the player sees spawning has stopped. Kill the handle when the room unloads.
    /// 144 StairsOpen    0.6 s reveal (cyan ring + 4 gold light posts rising), then a calm beacon: gold corner brackets breathing at 0.8 Hz, a bobbing gold down-chevron, cyan motes.
    /// 145 StairsEdge    while the stairs are off-screen: gold stairs glyph on a dark plate + cyan arrow, EdgeInsetPx (40) from the screen edge, like DoorFX. Needs Camera.main (ortho).
    /// 146 Descend       beats: 0 step on (t 0, SFX + lock input) -> 1 gold column rises (0.15 s) -> 2 iris to DARK (0.35-0.9 s) -> 3 black: OnTransition (load the next floor).
    ///                   After loading call DescendReveal(pos) to iris back open (0.5 s). The iris is dark #181425, never white.
    /// </summary>
    public static class RoomFX
    {
        public static int SortingOrder = 70; public static string SortingLayer = "Default"; public static Material MaterialOverride; public static int MaxItems = 64;
        public static float EdgeInsetPx = 40f, EdgeScale = 2f, IrisSeconds = 0.55f, RevealSeconds = 0.5f;
        /// <summary>(handle, pos) when the debris lands: SFX "nest_collapse", ScreenShake 2 px, drop rewards.</summary>
        public static event Action<int, Vector2> OnNestCollapsed;
        /// <summary>Descend beats (beat 0..3, stairsPos).</summary>
        public static event Action<int, Vector2> OnDescendBeat;
        /// <summary>Screen is fully dark: load the next floor now, then call DescendReveal.</summary>
        public static event Action<Vector2> OnTransition;
        static RoomRunner R => RoomRunner.I;
        public static void Prewarm() => R.Prewarm();
        /// <summary>Nest killed: collapse burst + a persistent dead marker. Returns the MARKER handle (Kill it on room unload).</summary>
        public static int NestDestroyed(Vector2 pos) { int h = R.Spawn(0, pos * HellFX.PPU, Vector2.zero, 0, 0, 0.45f, 0, 1); NestDeadMarker(pos); return R.LastMarker; }
        public static int NestDeadMarker(Vector2 pos) { int h = R.Spawn(1, pos * HellFX.PPU, Vector2.zero, 0, 0, float.MaxValue, 0, 1); R.LastMarker = h; return h; }
        /// <summary>Stairs open: reveal then a persistent beacon + off-screen edge marker. Returns the beacon handle.</summary>
        public static int StairsOpen(Vector2 pos) => R.Spawn(2, pos * HellFX.PPU, Vector2.zero, 0, 0, float.MaxValue, 0, 1);
        /// <summary>Player stepped on the stairs (pass the beacon handle to retire it).</summary>
        public static int Descend(Vector2 stairsPos, int beacon = -1) { R.Kill(beacon); return R.Spawn(3, stairsPos * HellFX.PPU, Vector2.zero, 0, 0, 0.9f + 0.05f, 0, 1); }
        public static int DescendReveal(Vector2 heroPos) => R.Spawn(4, heroPos * HellFX.PPU, Vector2.zero, 0, 0, RevealSeconds, 0, 1);
        internal static void Collapsed(int h, Vector2 p) => OnNestCollapsed?.Invoke(h, p);
        internal static void Beat(int b, Vector2 p) { OnDescendBeat?.Invoke(b, p); if (b == 3) OnTransition?.Invoke(p); }
        public static void Kill(int h) => R.Kill(h);
        public static void Clear() => R.ClearAll();
    }

    public class RoomRunner : MonoBehaviour
    {
        static RoomRunner _i;
        public static RoomRunner I { get { if (_i == null) { var go = new GameObject("RoomFX"); DontDestroyOnLoad(go); _i = go.AddComponent<RoomRunner>(); } return _i; } }
        struct It { public bool used; public byte k; public int lv; public Vector2 p, d; public float r, a, life, fuse, t, seed; public ushort gen; public int beat; }
        It[] _it; int[] _free; int _freeN; public int LastMarker = -1;
        const int MaxQ = 4096;
        struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _q = new Q[MaxQ]; int _n, _last;
        Mesh _mesh; Vector3[] _vtx; Color32[] _col;
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255), Lt = new Color32(0xc0, 0xcb, 0xdc, 255),
            Y = new Color32(0xfe, 0xe7, 0x61, 255), Go = new Color32(0xfe, 0xae, 0x34, 255), Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Cb = new Color32(0x00, 0x99, 0xdb, 255),
            Sl = new Color32(0x5a, 0x69, 0x88, 255), Sg = new Color32(0x8b, 0x9b, 0xb4, 255), Br = new Color32(0x73, 0x3e, 0x39, 255), Bw = new Color32(0xbe, 0x4a, 0x2f, 255);
        static readonly float[] Beats = { 0f, 0.15f, 0.35f, 0.9f };

        public void Prewarm()
        {
            if (_mesh != null) return;
            int cap = Mathf.Clamp(RoomFX.MaxItems, 16, 4096); _it = new It[cap]; _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) _free[_freeN++] = i;
            _vtx = new Vector3[MaxQ * 4]; _col = new Color32[MaxQ * 4]; var uv = new Vector2[MaxQ * 4]; var tri = new int[MaxQ * 6];
            for (int i = 0; i < MaxQ; i++) { int v = i * 4, t = i * 6; tri[t] = v; tri[t + 1] = v + 1; tri[t + 2] = v + 2; tri[t + 3] = v; tri[t + 4] = v + 2; tri[t + 5] = v + 3;
                uv[v] = new Vector2(.25f, .25f); uv[v + 1] = new Vector2(.25f, .75f); uv[v + 2] = new Vector2(.75f, .75f); uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "RoomFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Room White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = RoomFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "RoomFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] RoomFX: no sprite shader found; set RoomFX.MaterialOverride.");
            mr.sortingOrder = RoomFX.SortingOrder; mr.sortingLayerName = RoomFX.SortingLayer; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = uv; _mesh.colors32 = _col; _mesh.triangles = tri;
        }
        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && _it != null && i < _it.Length && _it[i].used && _it[i].gen == (ushort)(h >> 16); }
        public int Spawn(byte k, Vector2 p, Vector2 d, float r, float a, float life, float fuse, int lv)
        {
            if (_mesh == null) Prewarm();
            if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ref var it = ref _it[i]; ushort g = (ushort)(it.gen + 1);
            it = new It { used = true, k = k, p = p, d = d, r = r, a = a, life = life, fuse = fuse, lv = Mathf.Clamp(lv, 1, 5), gen = g, seed = (i * 0.618f + Time.frameCount * 0.31f) % 1f };
            return Hd(i);
        }
        public void Kill(int h) { if (!Get(h, out int i)) return; _it[i].used = false; _free[_freeN++] = i; }
        public void ClearAll() { if (_it == null) return; for (int i = 0; i < _it.Length; i++) if (_it[i].used) { _it[i].used = false; _free[_freeN++] = i; } _n = 0; }

        /* primitives (pixel units, snapped) */
        void A(float x, float y, float w, float h, Color32 c) { if (_n >= MaxQ || c.a == 0) return; _q[_n++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        void Dot(float cx, float cy, int s, Color32 c) => A(Mathf.Round(cx) - (s >> 1), Mathf.Round(cy) - (s >> 1), s, s, c);
        void Disc(float cx, float cy, int s, Color32 c) { int k = s >= 7 ? 2 : 1; Dot(cx, cy, s - 2 * k, c); A(Mathf.Round(cx) - (s >> 1) + k, Mathf.Round(cy) - (s >> 1), s - 2 * k, s, c); A(Mathf.Round(cx) - (s >> 1), Mathf.Round(cy) - (s >> 1) + k, s, s - 2 * k, c); }
        static Color32 Fa(Color32 c, float a) { c.a = (byte)(Mathf.Clamp01(a) * 255); return c; }
        void Arc(Vector2 c, float r, float a0, float a1, int s, Color32 col, float ys = 1f)
        {
            if (r < 1) return; float span = a1 - a0; int n = Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(span) * Mathf.Deg2Rad * r / 2f), 1, 400);
            for (int j = 0; j <= n; j++) { float a = (a0 + span * j / n) * Mathf.Deg2Rad; Dot(c.x + Mathf.Cos(a) * r, c.y + Mathf.Sin(a) * r * ys, s, col); }
        }
        void Line(Vector2 a, Vector2 b, int s, Color32 col) { float L = (b - a).magnitude; int n = Mathf.Max(1, Mathf.CeilToInt(L / 2f)); for (int j = 0; j <= n; j++) { var p = Vector2.Lerp(a, b, j / (float)n); Dot(p.x, p.y, s, col); } }


        static float H(float x) { x = Mathf.Sin(x * 127.1f + 311.7f) * 43758.5453f; return x - Mathf.Floor(x); }
        void Brackets(Vector2 c, float hw, float hh, Color32 col)
        {
            float x0 = Mathf.Round(c.x - hw), x1 = Mathf.Round(c.x + hw) - 3, y0 = Mathf.Round(c.y - hh), y1 = Mathf.Round(c.y + hh) - 3;
            A(x0, y0, 7, 3, col); A(x0, y0, 3, 7, col); A(x1 - 4, y0, 7, 3, col); A(x1, y0, 3, 7, col);
            A(x0, y1, 7, 3, col); A(x0, y1 - 4, 3, 7, col); A(x1 - 4, y1, 7, 3, col); A(x1, y1 - 4, 3, 7, col);
        }
        void Chev(Vector2 tip, Vector2 d, int len, Color32 c) { Vector2 n = new Vector2(-d.y, d.x); for (int s = 0; s <= len; s++) { Dot(tip.x - d.x * s + n.x * s, tip.y - d.y * s + n.y * s, 5, Ko); } for (int s = 0; s <= len; s++) Dot(tip.x - d.x * s + n.x * s, tip.y - d.y * s + n.y * s, 3, c); for (int s = 0; s <= len; s++) { Dot(tip.x - d.x * s - n.x * s, tip.y - d.y * s - n.y * s, 5, Ko); } for (int s = 0; s <= len; s++) Dot(tip.x - d.x * s - n.x * s, tip.y - d.y * s - n.y * s, 3, c); }
        void StairsGlyph(Vector2 c, int k)
        {   /* (22k)x(22k) dark plate, 3 gold steps going down-right, cyan corner; k = RoomFX.EdgeScale */
            float x = Mathf.Round(c.x) - 11 * k, y = Mathf.Round(c.y) - 11 * k;
            A(x - 1, y - 1, 22 * k + 2, 22 * k + 2, Ko); A(x + 1, y + 1, 22 * k - 2, 22 * k - 2, new Color32(0x26, 0x2b, 0x44, 255));
            A(x + 3 * k, y + 15 * k, 6 * k, 3 * k, Go); A(x + 8 * k, y + 10 * k, 6 * k, 3 * k, Go); A(x + 13 * k, y + 5 * k, 6 * k, 3 * k, Go);
            A(x + 6 * k, y + 10 * k, 3 * k, 8 * k, Go); A(x + 11 * k, y + 5 * k, 3 * k, 8 * k, Go); A(x + 3 * k, y + 3 * k, 3 * k, 3 * k, Cy);
        }
        void Iris(Vector2 c, float r)
        {   /* dark everywhere outside radius r (screen space): rows of 4 px */
            var cam = Camera.main; if (cam == null || !cam.orthographic) { if (r < 1) A(c.x - 2000, c.y - 2000, 4000, 4000, Ko); return; }
            float hh = cam.orthographicSize, hw = hh * cam.aspect; Vector2 cp = (Vector2)cam.transform.position * HellFX.PPU; hh *= HellFX.PPU; hw *= HellFX.PPU;
            float x0 = Mathf.Floor(cp.x - hw) - 4, x1 = Mathf.Ceil(cp.x + hw) + 4;
            for (float y = Mathf.Floor(cp.y - hh) - 4; y < cp.y + hh + 4; y += 4)
            {
                float dy = y + 2 - c.y, s = r * r - dy * dy;
                if (s <= 0) { A(x0, y, x1 - x0, 4, Ko); continue; }
                float hx = Mathf.Sqrt(s), l = Mathf.Round(c.x - hx), rr = Mathf.Round(c.x + hx);
                if (l > x0) A(x0, y, l - x0, 4, Ko); if (rr < x1) A(rr, y, x1 - rr, 4, Ko);
            }
        }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.unscaledDeltaTime, u = 1f / HellFX.PPU;
            var cam = Camera.main; bool hasCam = cam != null && cam.orthographic; Vector2 cmin = default, cmax = default;
            if (hasCam) { float hh = cam.orthographicSize, hw = hh * cam.aspect; Vector2 cp = cam.transform.position; cmin = (cp - new Vector2(hw, hh)) * HellFX.PPU; cmax = (cp + new Vector2(hw, hh)) * HellFX.PPU; }
            for (int i = 0; i < _it.Length; i++)
            {
                if (!_it[i].used) continue; ref var it = ref _it[i]; float t0 = it.t; it.t += dt;
                if (it.k == 0 && t0 < 0.2f && it.t >= 0.2f) RoomFX.Collapsed(Hd(i), it.p * u);
                if (it.k == 3) while (it.beat < 4 && it.t >= Beats[it.beat]) { RoomFX.Beat(it.beat, it.p * u); it.beat++; }
                if (it.t >= it.life) { if (it.k == 3) { Iris(it.p, 0); } Kill(Hd(i)); continue; }
                switch (it.k)
                {
                    case 0: DrawCollapse(ref it); break;
                    case 1: DrawDead(ref it); break;
                    case 2: DrawStairs(ref it); if (hasCam && (it.p.x < cmin.x || it.p.x > cmax.x || it.p.y < cmin.y || it.p.y > cmax.y)) DrawEdge(it.p, cmin, cmax); break;
                    case 3: DrawDescend(ref it); break;
                    case 4: Iris(it.p, Mathf.Lerp(0, 420, Mathf.SmoothStep(0, 1, it.t / it.life))); break;
                }
            }
            for (int b = 0; b < _n; b++)
            {
                int v = b * 4; ref var s = ref _q[b]; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
                _vtx[v] = new Vector3(x0, y0, 0); _vtx[v + 1] = new Vector3(x0, y1, 0); _vtx[v + 2] = new Vector3(x1, y1, 0); _vtx[v + 3] = new Vector3(x1, y0, 0);
                _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
            }
            for (int z = _n; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds(); _last = _n; _n = 0;
        }
        void DrawCollapse(ref It it)
        {
            float t = it.t; Vector2 p = it.p;
            if (t < 0.2f) { float k = t / 0.2f, e = k * k; for (int j = 0; j < 10; j++) { float a = (j * 36f + H(it.seed + j) * 20f) * Mathf.Deg2Rad, r0 = 10f + 6f * H(j * 2.3f); Vector2 q = p + new Vector2(Mathf.Cos(a) * r0 * (1 - e * 0.7f), Mathf.Sin(a) * r0 * 0.6f * (1 - e * 0.7f) + 20f * (1 - e)); int s = j % 3 == 0 ? 7 : 5; Dot(q.x, q.y, s, Ko); Dot(q.x, q.y, s - 2, j % 2 == 0 ? Bw : Br); } }
            if (t >= 0.2f) { float k = (t - 0.2f) / 0.25f, al = 1 - k * k, rr = 6f + 26f * Mathf.Sqrt(k); Arc(p, rr, 0, 360, 5, Fa(Ko, al * 0.7f), 0.6f); Arc(p, rr, 0, 360, 3, Fa(Go, al), 0.6f);
                for (int j = 0; j < 6; j++) { float a = (30 + j * 24f) * Mathf.Deg2Rad; Vector2 q = p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (6 + 22 * k); Dot(q.x, q.y, 5, Fa(Ko, al)); Dot(q.x, q.y, 3, Fa(j % 2 == 0 ? Y : Go, al)); } }
        }
        void DrawDead(ref It it)
        {   /* broken seal, cold, low contrast but outlined; embers 0.5 Hz */
            Vector2 p = it.p; float fade = Mathf.Clamp01(it.t / 0.45f);
            Arc(p, 10, 20, 150, 5, Fa(Ko, 0.7f * fade), 0.6f); Arc(p, 10, 20, 150, 3, Fa(Sl, fade), 0.6f);
            Arc(p, 10, 185, 330, 5, Fa(Ko, 0.7f * fade), 0.6f); Arc(p, 10, 185, 330, 3, Fa(Sl, fade), 0.6f);
            Line(p + new Vector2(-5, 3), p + new Vector2(4, -3), 3, Fa(Sl, fade));
            for (int j = 0; j < 3; j++) { float k = (it.t * 0.5f + j / 3f) % 1f; Vector2 q = p + new Vector2((j - 1) * 6 + Mathf.Sin(it.t + j) * 1.5f, 2 + 16 * k); Dot(q.x, q.y, 5, Fa(Ko, 0.5f * (1 - k) * fade)); Dot(q.x, q.y, 3, Fa(Sg, (1 - k) * fade)); }
        }
        void DrawStairs(ref It it)
        {
            float t = it.t; Vector2 p = it.p;
            if (t < 0.6f) { float k = t / 0.6f, al = 1 - k; Arc(p, 6 + 30 * Mathf.Sqrt(k), 0, 360, 5, Fa(Ko, al * 0.6f), 0.6f); Arc(p, 6 + 30 * Mathf.Sqrt(k), 0, 360, 3, Fa(Cy, al), 0.6f);
                for (int j = 0; j < 4; j++) { float x = p.x + (j - 1.5f) * 14f, hgt = 30 * Mathf.Min(1, k * 2) * al; Line(new Vector2(x, p.y), new Vector2(x, p.y + hgt), 5, Fa(Ko, al * 0.6f)); Line(new Vector2(x, p.y), new Vector2(x, p.y + hgt), 3, Fa(Y, al)); } }
            float br = 0.65f + 0.35f * (0.5f + 0.5f * Mathf.Sin(t * 0.8f * 6.2832f)), pop = t < 0.6f ? 1.3f - 0.3f * t / 0.6f : 1f;
            Brackets(p, 20 * pop + 2, 18 * pop + 2, Fa(Ko, 0.8f)); Brackets(p, 20 * pop, 18 * pop, Fa(Go, br));
            float bob = Mathf.Round(Mathf.Sin(t * 0.8f * 6.2832f) * 2f); Chev(p + new Vector2(0, 26 + bob), Vector2.down, 4, Y);
            for (int j = 0; j < 3; j++) { float k = (t * 0.6f + j / 3f) % 1f; Dot(p.x + (j - 1) * 10, p.y - 6 + 22 * k, 3, Fa(Cy, 1 - k)); }
        }
        void DrawEdge(Vector2 target, Vector2 cmin, Vector2 cmax)
        {
            float m = RoomFX.EdgeInsetPx; Vector2 q = new Vector2(Mathf.Clamp(target.x, cmin.x + m, cmax.x - m), Mathf.Clamp(target.y, cmin.y + m, cmax.y - m)), dir = (target - q).normalized;
            int k = Mathf.Max(1, Mathf.RoundToInt(RoomFX.EdgeScale)); StairsGlyph(q, k); Vector2 s0 = q + dir * (11 * k + 3), s1 = q + dir * (11 * k + 9); Line(s0, s1, 5, Ko); Line(s0, s1, 3, Cy); Chev(s1 + dir * 2, dir, 3, Cy);
        }
        void DrawDescend(ref It it)
        {
            float t = it.t; Vector2 p = it.p;
            if (t >= 0.15f) { float k = Mathf.Clamp01((t - 0.15f) / 0.4f), al = t < 0.6f ? 1f : Mathf.Clamp01(1 - (t - 0.6f) / 0.3f); for (int w = -1; w <= 1; w++) { Line(new Vector2(p.x + w * 6, p.y), new Vector2(p.x + w * 6, p.y + 60 * k), 5, Fa(Ko, al * 0.6f)); Line(new Vector2(p.x + w * 6, p.y), new Vector2(p.x + w * 6, p.y + 60 * k * (w == 0 ? 1 : 0.7f)), 3, Fa(w == 0 ? Y : Go, al)); } }
            Brackets(p, 20, 18, Go);
            if (t >= 0.35f) { float k = Mathf.Clamp01((t - 0.35f) / RoomFX.IrisSeconds); Iris(p, Mathf.Lerp(420, 0, Mathf.SmoothStep(0, 1, k))); }
        }
    }
}
