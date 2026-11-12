using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>State of a door registered with <see cref="DoorFX"/>.</summary>
    public enum DoorState { None = 0, Open = 1, Locking = 2, Locked = 3, Unlocking = 4 }

    /// <summary>
    /// 127. DOOR LOCK / ROOM CLEARED (v1.9.8). A "sealed" language that never reads as a hazard tell: cool steel bars + a cyan seal seam + a padlock,
    /// no red, no hazard stripes, no blink-to-strike. Unlock is a gold reward burst. Sizes in world pixels (16 PPU, 640x360 view); nothing thinner than 3 px.
    /// Register each door once (<see cref="RegisterDoor"/>: centre of the doorway on the wall line, inward normal pointing into the room, width).
    /// LOCK (<see cref="LockAll"/>), staggered by <see cref="StaggerSeconds"/> per door: 3 px steel bars slide in from outside and SLAM at 0.18 s
    ///   (white 3 px threshold flash + grey dust puffs), then an 11x13 padlock on a dark plate with a steel frame drops onto the seal and a cyan
    ///   3 px seam lights across the doorway. Idle: a slow white glint travels along the seam (no blinking).
    /// UNLOCK (<see cref="UnlockAll"/>): padlock pops open and lifts (0.15 s), bars retract (0.15-0.45 s), a gold 3 px ring + 12 gold sparks burst from each
    ///   door and 3 gold outward chevrons pulse through the doorway (to 1.4 s). <see cref="ClearBurst"/> = optional gold "room cleared" ring at the hero.
    /// OFF-SCREEN: a door outside the camera view gets an edge marker (padlock on a dark plate, cyan frame, 3 px chevron toward the door; gold + open
    ///   padlock while unlocking). Needs Camera.main (orthographic).
    /// SFX beats (use the callbacks): OnLock (t=0, room-seal whoosh/sting) -> OnDoorLocked per door at its slam (t = stagger*i + 0.18, heavy clang)
    ///   -> padlock click ~0.12 s later. OnUnlock (t=0, clear sting/chime) -> OnDoorUnlocked per door at its pop (t = stagger*i + 0.15, unlock click),
    ///   bar scrape 0.15-0.45 s, sparkle tail.
    /// Visual only. One pooled mesh, 1 draw call, no per-frame alloc, int handles (-1 when the pool is full).
    /// </summary>
    public static class DoorFX
    {
        static DoorRunner R => DoorRunner.I;
        public static float DepthPx = 14f, StaggerSeconds = 0.06f, SlamAt = 0.18f, LockSeconds = 0.45f, PopAt = 0.15f, UnlockSeconds = 1.4f, EdgeMarginPx = 14f, EdgeInsetPx = 40f, EdgeScale = 2f, SameWallGapPx = 60f, ClearSeconds = 0.8f;
        /// <summary>Off-screen markers: EdgeInsetPx = distance of the marker centre from the screen edge (keeps it out of the CRT vignette and debug-text strips); EdgeScale = padlock size multiplier (2 = 22x26 glyph on a 30x34 plate); SameWallGapPx = min distance from a visible door before the marker slides along the edge.</summary>
        public static Material MaterialOverride; public static int SortingOrder = 72; public static string SortingLayer = "Default";
        /// <summary>Room-wide: lock started / unlock started.</summary>
        public static Action OnLock, OnUnlock;
        /// <summary>(door handle) at each door's slam / at each padlock pop.</summary>
        public static Action<int> OnDoorLocked, OnDoorUnlocked;

        public static void Prewarm(int items = 16) => R.Prewarm(items);
        /// <summary>Doorway centre on the wall line, inward normal (into the room), doorway width. Starts Open (nothing drawn).</summary>
        public static int RegisterDoor(Vector2 centre, Vector2 inwardNormal, float widthPx = 32f) => R.Add(centre, inwardNormal, widthPx);
        public static void Unregister(int door) => R.Remove(door);
        public static void LockAll() => R.LockAll();
        public static void UnlockAll() => R.UnlockAll();
        public static void ClearBurst(Vector2 centre) => R.Burst(centre);
        public static DoorState GetState(int door) => R.State(door);
        public static void Clear() => R.ClearAll();
        public static int DoorCount => DoorRunner.Exists ? R.Count : 0;
    }

    public class DoorRunner : MonoBehaviour
    {
        static DoorRunner _i;
        public static bool Exists => _i != null;
        public static DoorRunner I { get { if (_i == null) { var go = new GameObject("DoorFX"); DontDestroyOnLoad(go); _i = go.AddComponent<DoorRunner>(); } return _i; } }

        struct It { public bool used; public DoorState st; public Vector2 c, n; public float w, pt, delay; public bool fired; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }
        Vector2 _bc; float _bt = 99f;

        const int MaxQ = 8192;
        struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ]; int _nb, _nf, _last;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        // Endesga 32: steel, cyan seal, gold reward (no red)
        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255),
            Lt2 = new Color32(0xc0, 0xcb, 0xdc, 255), Lt = new Color32(0x8b, 0x9b, 0xb4, 255), Sl = new Color32(0x5a, 0x69, 0x88, 255), Sd = new Color32(0x3a, 0x44, 0x66, 255),
            Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Cb = new Color32(0x00, 0x99, 0xdb, 255), Y = new Color32(0xfe, 0xe7, 0x61, 255), G = new Color32(0xfe, 0xae, 0x34, 255);
        // 11x13 padlock, 3 px shackle; 'k' = keyhole
        static readonly string[] LockC = { "..WWWWWWW..", ".WWWWWWWWW.", "WWW.....WWW", "WWW.....WWW", "WWW.....WWW", "WWWWWWWWWWW", "WWWWWWWWWWW", "WWWWkkkWWWW", "WWWWkkkWWWW", "WWWWWkWWWWW", "WWWWWkWWWWW", "WWWWWWWWWWW", "WWWWWWWWWWW" };
        static readonly string[] LockO = { "..WWWWWWW..", ".WWWWWWWWW.", "WWW.....WWW", "WWW........", "WWW........", "WWWWWWWWWWW", "WWWWWWWWWWW", "WWWWkkkWWWW", "WWWWkkkWWWW", "WWWWWkWWWWW", "WWWWWkWWWWW", "WWWWWWWWWWW", "WWWWWWWWWWW" };

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 256);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || !_it[i].used) _free[_freeN++] = i; _it = n;
            }
            if (_mesh != null) return;
            _vtx = new Vector3[MaxQ * 2 * 4]; _uv = new Vector2[MaxQ * 2 * 4]; _col = new Color32[MaxQ * 2 * 4]; _tri = new int[MaxQ * 2 * 6];
            for (int i = 0; i < MaxQ * 2; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "DoorFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Door White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = DoorFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "DoorFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] DoorFX: no sprite shader found; set DoorFX.MaterialOverride.");
            mr.sortingOrder = DoorFX.SortingOrder; mr.sortingLayerName = DoorFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }
        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].used && _it[i].gen == (ushort)(h >> 16); }
        public int Add(Vector2 c, Vector2 n, float w)
        {
            if (_mesh == null) Prewarm(16); if (_freeN == 0) return -1; int i = _free[--_freeN]; ref var k = ref _it[i];
            k.gen = (ushort)((k.gen + 1) & 0x7FFF); k.used = true; k.st = DoorState.Open; k.c = c * HellFX.PPU; k.n = n.sqrMagnitude > 1e-6f ? n.normalized : Vector2.down; k.w = Mathf.Max(12, w); k.pt = 0; Count++; return Hd(i);
        }
        public void Remove(int h) { if (!Get(h, out int i)) return; _it[i].used = false; _it[i].st = DoorState.None; _free[_freeN++] = i; Count--; }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].used) { _it[i].used = false; _free[_freeN++] = i; Count--; } _bt = 99f; }
        public DoorState State(int h) => Get(h, out int i) ? _it[i].st : DoorState.None;
        public void LockAll()
        {
            if (_mesh == null) Prewarm(16); int o = 0;
            for (int i = 0; i < _it.Length; i++) { ref var k = ref _it[i]; if (!k.used || k.st == DoorState.Locked || k.st == DoorState.Locking) continue; k.st = DoorState.Locking; k.pt = 0; k.delay = o++ * DoorFX.StaggerSeconds; k.fired = false; }
            DoorFX.OnLock?.Invoke();
        }
        public void UnlockAll()
        {
            if (_mesh == null) Prewarm(16); int o = 0;
            for (int i = 0; i < _it.Length; i++) { ref var k = ref _it[i]; if (!k.used || k.st == DoorState.Open || k.st == DoorState.Unlocking) continue; k.st = DoorState.Unlocking; k.pt = 0; k.delay = o++ * DoorFX.StaggerSeconds; k.fired = false; }
            DoorFX.OnUnlock?.Invoke();
        }
        public void Burst(Vector2 c) { if (_mesh == null) Prewarm(16); _bc = c * HellFX.PPU; _bt = 0; }

        // ---------- emit ----------
        void Rect(float x, float y, float w, float h, Color32 c) { if (_nf < MaxQ) _fg[_nf++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        void Back(float x, float y, float w, float h) { if (_nb < MaxQ) _bk[_nb++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = K }; }
        void Dot3(Vector2 p, Color32 c) { Back(p.x - 2, p.y - 2, 5, 5); Rect(p.x - 1, p.y - 1, 3, 3, c); }
        void Bar3(Vector2 a, Vector2 b, Color32 c) { Vector2 d = b - a; float L = d.magnitude; if (L < 0.5f) { Dot3(a, c); return; } d /= L; for (int r = 0; r <= (int)L; r++) Dot3(a + d * r, c); }
        void Ring3(Vector2 c0, float r, Color32 c) { int n = Mathf.Max(12, Mathf.CeilToInt(2 * Mathf.PI * r / 2)); for (int j = 0; j < n; j++) { float an = j * 6.2832f / n; Dot3(c0 + new Vector2(Mathf.Cos(an) * r, Mathf.Sin(an) * r * 0.7f), c); } }
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(255 * a), 0, 255); return c; }
        static float H(float x) { x = Mathf.Sin(x * 12.9898f) * 43758.5453f; return x - Mathf.Floor(x); }
        void Padlock(Vector2 p, bool open, Color32 body, Color32 frame, int sc = 1)
        {
            float x = Mathf.Round(p.x) - 5 * sc, y = Mathf.Round(p.y) - 6 * sc; int pad = 2 * sc + 1;
            Rect(x - pad - 1, y - pad - 1, 11 * sc + 2 * pad + 2, 13 * sc + 2 * pad + 2, Ko);   // outer dark plate
            Rect(x - pad, y - pad, 11 * sc + 2 * pad, 13 * sc + 2 * pad, frame); Rect(x - pad + 3, y - pad + 3, 11 * sc + 2 * pad - 6, 13 * sc + 2 * pad - 6, Ko);
            var g = open ? LockO : LockC;
            for (int r = 0; r < 13; r++) for (int cc = 0; cc < 11; cc++) { char ch = g[r][cc]; if (ch == '.') continue; Rect(x + cc * sc, y + (12 - r) * sc, sc, sc, ch == 'k' ? Ko : (r < 5 ? Lt2 : body)); }
        }
        void Chev3(Vector2 tip, Vector2 d, int len, Color32 c) { Vector2 n = new Vector2(-d.y, d.x); for (int s = 0; s <= len; s += 1) { Dot3(tip - d * s + n * s, c); Dot3(tip - d * s - n * s, c); } }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, u = 1f / HellFX.PPU; _nb = _nf = 0;
            var cam = Camera.main; bool hasCam = cam != null && cam.orthographic; Vector2 cmin = default, cmax = default;
            if (hasCam) { float hh = cam.orthographicSize, hw = hh * cam.aspect; Vector2 cp = cam.transform.position; cmin = (cp - new Vector2(hw, hh)) * HellFX.PPU; cmax = (cp + new Vector2(hw, hh)) * HellFX.PPU; }
            for (int i = 0; i < _it.Length; i++)
            {
                ref var k = ref _it[i]; if (!k.used) continue; k.pt += dt;
                float t = k.pt - k.delay;
                if (k.st == DoorState.Locking) { if (!k.fired && t >= DoorFX.SlamAt) { k.fired = true; DoorFX.OnDoorLocked?.Invoke(Hd(i)); } if (t >= DoorFX.LockSeconds) { k.st = DoorState.Locked; k.pt = 0; k.delay = 0; } }
                else if (k.st == DoorState.Unlocking) { if (!k.fired && t >= DoorFX.PopAt) { k.fired = true; DoorFX.OnDoorUnlocked?.Invoke(Hd(i)); } if (t >= DoorFX.UnlockSeconds) { k.st = DoorState.Open; k.pt = 0; } }
                if (k.st == DoorState.Open) continue;
                Draw(ref k, Mathf.Max(0, t));
                if (hasCam && (k.c.x < cmin.x || k.c.x > cmax.x || k.c.y < cmin.y || k.c.y > cmax.y)) Edge(ref k, Mathf.Max(0, t), cmin, cmax);
            }
            if (_bt < DoorFX.ClearSeconds) { _bt += dt; float s = _bt / DoorFX.ClearSeconds; Ring3(_bc, 10 + 70 * s, s < 0.25f ? Wh : (s < 0.6f ? Y : G));
                for (int j = 0; j < 16; j++) { float an = j * 0.3927f; Dot3(_bc + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.7f) * (16 + 50 * s) + new Vector2(0, 8), j % 2 == 0 ? Y : Wh); } }
            int q = 0;
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
        }
        void Draw(ref It k, float t)
        {
            Vector2 n = k.n, d = new Vector2(n.y, -n.x), c = k.c; float hw = k.w * 0.5f, D = DoorFX.DepthPx;
            int bars = Mathf.Max(3, Mathf.RoundToInt(k.w / 8f));
            float slide = 1f;   // 1 = bars fully in
            if (k.st == DoorState.Locking) { float s = Mathf.Clamp01(t / DoorFX.SlamAt); slide = s * s; }
            else if (k.st == DoorState.Unlocking) slide = 1 - Mathf.Clamp01((t - DoorFX.PopAt) / 0.3f);
            if (k.st == DoorState.Locking && t < 0.0001f) return;
            // bars: run along n (from outside -n*D to inside +n*2), shifted outward by (1-slide)*D*1.4
            Vector2 off = -n * ((1 - slide) * D * 1.4f);
            if (slide > 0.02f)
            {
                for (int b = 0; b < bars; b++)
                {
                    float x = -hw + 3 + b * (k.w - 6) / (bars - 1);
                    Vector2 a0 = c + d * x - n * D + off, a1 = c + d * x + n * 2 + off;
                    Bar3(a0, a1, b % 2 == 0 ? Lt2 : Lt);
                    Dot3(a1, Wh);
                }
                Bar3(c - d * hw - n * (D * 0.5f) + off, c + d * hw - n * (D * 0.5f) + off, Sl);   // crossbar
            }
            if (k.st == DoorState.Locking)
            {
                if (t >= DoorFX.SlamAt && t < DoorFX.SlamAt + 0.12f) { Bar3(c - d * (hw + 2) + n * 4, c + d * (hw + 2) + n * 4, Wh);
                    for (int j = 0; j < 6; j++) { float s = (t - DoorFX.SlamAt) / 0.12f; Dot3(c + d * ((H(j) - 0.5f) * k.w * 1.2f) + n * (6 + 10 * s * H(j + 3)), j % 2 == 0 ? Lt2 : Lt); } }
                if (t >= DoorFX.SlamAt + 0.12f) { float s = Mathf.Clamp01((t - DoorFX.SlamAt - 0.12f) / 0.1f); Seam(ref k, t); Padlock(c + n * (D + 6 + 8 * (1 - s)), false, Lt, Cy); }
            }
            else if (k.st == DoorState.Locked) { Seam(ref k, t); Padlock(c + n * (D + 6), false, Lt, Cy); }
            else if (k.st == DoorState.Unlocking)
            {
                if (t < DoorFX.PopAt) { Seam(ref k, t); Padlock(c + n * (D + 6), false, Lt, Cy); }
                else
                {
                    float s = (t - DoorFX.PopAt) / (DoorFX.UnlockSeconds - DoorFX.PopAt);
                    if (s < 0.45f) Padlock(c + n * (D + 6) + new Vector2(0, 4 + 30 * s), true, G, Y);
                    if (s < 0.5f) Ring3(c + n * 4, 8 + 40 * s, s < 0.15f ? Wh : Y);
                    if (s < 0.6f) for (int j = 0; j < 12; j++) { float an = j * 0.5236f + 0.2f; Dot3(c + n * 4 + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.7f) * (10 + 36 * s) + new Vector2(0, 10 * s), j % 2 == 0 ? G : Wh); }
                    for (int j = 0; j < 3; j++) { float ph = (s * 3 + j / 3f) % 1f; if (s > 0.85f) break; Chev3(c - n * (4 + ph * D * 1.6f), -n, 3, ph < 0.3f ? Wh : Y); /* starts outside the wall line, only moves outward (-n) */ }
                }
            }
        }
        void Seam(ref It k, float t)
        {
            Vector2 n = k.n, d = new Vector2(n.y, -n.x), c = k.c + n * 5; float hw = k.w * 0.5f + 2;
            Bar3(c - d * hw, c + d * hw, Cb);
            float g = (k.pt * 0.6f) % 1.6f; if (g < 1f) { Vector2 p = c - d * hw + d * (2 * hw * g); Dot3(p, Wh); Dot3(p - d * 3, Cy); Dot3(p + d * 3, Cy); }
        }
        void Edge(ref It k, float t, Vector2 cmin, Vector2 cmax)
        {
            float m = DoorFX.EdgeInsetPx; int sc = Mathf.Max(1, Mathf.RoundToInt(DoorFX.EdgeScale));
            Vector2 p = new Vector2(Mathf.Clamp(k.c.x, cmin.x + m, cmax.x - m), Mathf.Clamp(k.c.y, cmin.y + m, cmax.y - m));
            Vector2 dir = k.c - p; if (dir.sqrMagnitude < 1) return; dir.Normalize();
            bool open = k.st == DoorState.Unlocking && t >= DoorFX.PopAt;
            if (open && t > DoorFX.UnlockSeconds - 0.3f) return;
            // same wall as a visible door: slide the marker along the edge, away from that door, so it reads as a SECOND exit
            Vector2 tan = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? Vector2.up : Vector2.right;
            for (int j = 0; j < _it.Length; j++)
            {
                ref var o = ref _it[j]; if (!o.used || o.st == DoorState.Open) continue;
                if (o.c.x < cmin.x || o.c.x > cmax.x || o.c.y < cmin.y || o.c.y > cmax.y) continue;
                Vector2 dd = p - o.c; float along = Vector2.Dot(dd, tan);
                if (dd.magnitude < DoorFX.SameWallGapPx)
                {
                    float sgn = Vector2.Dot(k.c - o.c, tan) >= 0 ? 1 : -1; p += tan * (sgn * DoorFX.SameWallGapPx - along);
                    p = new Vector2(Mathf.Clamp(p.x, cmin.x + m, cmax.x - m), Mathf.Clamp(p.y, cmin.y + m, cmax.y - m));
                    dir = (k.c - p).normalized;
                }
            }
            Color32 fc = open ? Y : Cy;
            float half = 9 * sc;
            Padlock(p - dir * 3, open, open ? G : Lt, fc, sc);
            // short arrow: 3 px shaft + chevron pointing at the off-screen door
            Vector2 s0 = p + dir * (half + 2), s1 = p + dir * (half + 7);
            Bar3(s0, s1, fc); Chev3(s1 + dir * 2, dir, 3, fc);
        }
        void Write(int q, ref Q s, float u)
        {
            int v = q * 4; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
            _vtx[v] = new Vector3(x0, y0, 0); _vtx[v + 1] = new Vector3(x0, y1, 0); _vtx[v + 2] = new Vector3(x1, y1, 0); _vtx[v + 3] = new Vector3(x1, y0, 0);
            _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
