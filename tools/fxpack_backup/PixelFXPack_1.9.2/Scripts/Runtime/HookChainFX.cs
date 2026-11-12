using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Phase of a hook chain (see <see cref="HookChainFX"/>).</summary>
    public enum HookPhase { None = 0, Tell = 1, Fly = 2, Retract = 3, Drag = 4, Snap = 5, Release = 6, Dormant = 7 }

    /// <summary>
    /// 115. Hook Chain (v1.8.9), Level 1 Tongue Pulling hazard (wall hooks, Hook Thrower, Judge "Hook Pull").
    /// TELL (default 0.7 s): a lane exactly as wide as the hook hitbox (dotted rust-orange edges = where you are safe outside),
    ///   a hollow double-barbed hook ghost at max range, steel chevrons marching outward, anchor ring pulsing; blinks white in the last 15%.
    /// FLY: 2-frame white release flash at the anchor, the hook head flies out (default 420 px/s) trailing alternating steel chain links
    ///   and 3 white speed pixels. MISS: clank sparks at max range, then the chain RETRACTs (0.8x speed).
    /// DRAG (after <see cref="HookChainFX.Latch"/>): latch burst on the victim, the chain is taut between anchor and victim, links march
    ///   toward the anchor and rust/white "&lt;" chevrons run along it (pull direction). Break read: N link pips above the victim
    ///   (filled per <see cref="HookChainFX.AddBreak"/>), sideways double-chevrons blinking at the victim (dash out sideways) and the weak
    ///   middle link glowing and shaking more with each pip.
    /// SNAP (break count reached): white burst at the middle link, the anchor half whips back, the victim half shatters into 6 links.
    /// RELEASE (<see cref="HookChainFX.Release"/>, e.g. victim reached the anchor): chain reels in over 0.15 s.
    /// Family look: dark #100010 3x3 backing per pixel like #113, white blink before release like #101/#113; its own colours are steel
    /// (#8b9bb4/#5a6988/white) chain + rust (#f77622) tell. Visual only: the game owns collision and movement; query <see cref="HookChainFX.TryGetTip"/>.
    /// One pooled mesh, one 4x4 white texture, one material for all hooks; no per-frame allocations.
    /// </summary>
    public static class HookChainFX
    {
        static HookChainRunner R => HookChainRunner.I;
        public static float DefaultTell = 0.7f, DefaultSpeedPx = 420f, DefaultRangePx = 160f, DefaultWidthPx = 6f;
        /// <summary>Break pips shown during the drag (proposal: each dash / hit on the chain = 1 pip). Per hook via Throw.</summary>
        public static int DefaultBreakCount = 3;
        /// <summary>Seconds the hook hangs dormant at its anchor after a miss/snap/release (dim, no backing, slow refill tick). 0 = free immediately.</summary>
        public static float DefaultCooldown = 4f;   // Hellord: Hook Thrower re-arm time (team decision 27 Oct 2026); change freely
        /// <summary>(handle) when the dormant cooldown ends and the slot is freed (the hook can be thrown again).</summary>
        public static Action<int> OnReady;
        public static Material MaterialOverride;
        public static int SortingOrder = 72; public static string SortingLayer = "Default";
        /// <summary>(handle) when the hook leaves the anchor.</summary>
        public static Action<int> OnLaunch;
        /// <summary>(handle) when the hook reaches max range without a latch.</summary>
        public static Action<int> OnMiss;
        /// <summary>(handle) when the chain snaps from break pips.</summary>
        public static Action<int> OnSnap;

        public static void Prewarm(int hooks = 16) => R.Prewarm(hooks);
        /// <summary>Starts a hook: tell, then fly from <paramref name="anchor"/> toward <paramref name="target"/> (world units; direction locked now).</summary>
        public static int Throw(Vector2 anchor, Vector2 target, float rangePx = -1, float tell = -1, float speedPx = -1, float widthPx = -1, int breakCount = -1)
            => R.Add(anchor, target, rangePx > 0 ? rangePx : DefaultRangePx, tell >= 0 ? tell : DefaultTell, speedPx > 0 ? speedPx : DefaultSpeedPx, widthPx > 0 ? widthPx : DefaultWidthPx, breakCount > 0 ? breakCount : DefaultBreakCount);
        /// <summary>World position of the flying hook head (Fly phase only), for the game's hit test.</summary>
        public static bool TryGetTip(int h, out Vector2 tip) => R.TryTip(h, out tip);
        /// <summary>Hook hit: start the drag. The chain follows <paramref name="victim"/> (+offset px) until Release/Snap/Cancel.</summary>
        public static void Latch(int h, Transform victim, Vector2 offsetPx = default) => R.Latch(h, victim, offsetPx);
        /// <summary>One break pip. Returns true when the chain snaps.</summary>
        public static bool AddBreak(int h) => R.AddBreak(h);
        public static void Release(int h) => R.Release(h);
        public static void SetAnchor(int h, Vector2 anchor) => R.SetAnchor(h, anchor);
        public static HookPhase GetPhase(int h) => R.Phase(h);
        /// <summary>Breaks done / needed (0..1) during the drag.</summary>
        public static float Break01(int h) => R.Break01(h);
        public static void Cancel(int h) => R.Cancel(h);
        public static bool IsAlive(int h) => R.Phase(h) != HookPhase.None;
        /// <summary>True while the hook hangs dormant (cooling down) at its anchor.</summary>
        public static bool IsDormant(int h) => R.Phase(h) == HookPhase.Dormant;
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => HookChainRunner.Exists ? R.Count : 0;
    }

    public class HookChainRunner : MonoBehaviour
    {
        static HookChainRunner _i;
        public static bool Exists => _i != null;
        public static HookChainRunner I { get { if (_i == null) { var go = new GameObject("HookChainFX"); DontDestroyOnLoad(go); _i = go.AddComponent<HookChainRunner>(); } return _i; } }

        struct Hk
        {
            public Vector2 a, d; public float L, tell, speed, width, t, pt, s; public HookPhase ph; public int breaks, need; public Transform v; public Vector2 off, tip, last; public ushort gen; public float cd;
        }
        Hk[] _h = new Hk[0]; int[] _free; int _freeN; public int Count { get; private set; }

        const int MaxQ = 4096; struct Q { public float x, y, s; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ]; int _nb, _nf, _last;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        public const float FlashT = 0.067f, LatchT = 0.15f, SnapT = 0.45f, ReleaseT = 0.15f;
        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), Wh = new Color32(255, 255, 255, 255), Y = new Color32(0xfe, 0xe7, 0x61, 255),
            O = new Color32(0xf7, 0x76, 0x22, 255), Lt = new Color32(0x8b, 0x9b, 0xb4, 255), Md = new Color32(0x5a, 0x69, 0x88, 255), Dk = new Color32(0x3a, 0x44, 0x66, 255);
        // double-barbed hook in local (u along flight, v across) pixels, tip at (0,0)
        static readonly sbyte[] HookU = { -5, -4, -3, -2, -1, 0, 1, 1, 0, -1, -2, 1, 0, -1, -2, -3, -3 };
        static readonly sbyte[] HookV = { 0, 0, 0, 0, 0, 0, 0, -1, -2, -3, -2, 1, 2, 3, 2, -1, 1 };

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 1024);
            if (_h.Length < cap)
            {
                var n = new Hk[cap]; Array.Copy(_h, n, _h.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _h.Length || _h[i].ph == HookPhase.None) _free[_freeN++] = i; _h = n;
            }
            if (_mesh != null) return;
            _vtx = new Vector3[MaxQ * 8]; _uv = new Vector2[MaxQ * 8]; _col = new Color32[MaxQ * 8]; _tri = new int[MaxQ * 12];
            for (int i = 0; i < MaxQ * 2; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "HookChain" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX HookChain White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = HookChainFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "HookChain (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] HookChainFX: no sprite shader found; set HookChainFX.MaterialOverride.");
            mr.sortingOrder = HookChainFX.SortingOrder; mr.sortingLayerName = HookChainFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }

        int Hd(int i) => i | (_h[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _h.Length && _h[i].ph != HookPhase.None && _h[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _h[i].ph = HookPhase.None; _h[i].v = null; _free[_freeN++] = i; Count--; }
        void Go(ref Hk k, HookPhase p) { k.ph = p; k.pt = 0; }

        public int Add(Vector2 a, Vector2 tgt, float L, float tell, float spd, float w, int need)
        {
            if (_mesh == null) Prewarm(16); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ref var k = ref _h[i]; float ppu = HellFX.PPU;
            k.a = a * ppu; Vector2 d = tgt * ppu - k.a; k.d = d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.right;
            k.L = L; k.tell = tell; k.speed = spd; k.width = w; k.need = Mathf.Clamp(need, 1, 8); k.breaks = 0; k.t = 0; k.s = 0; k.v = null; k.cd = HookChainFX.DefaultCooldown;
            k.gen = (ushort)((k.gen + 1) & 0x7FFF); Count++;
            if (tell > 0) Go(ref k, HookPhase.Tell); else { Go(ref k, HookPhase.Fly); HookChainFX.OnLaunch?.Invoke(Hd(i)); }
            return Hd(i);
        }
        public bool TryTip(int h, out Vector2 tip) { tip = default; if (!Get(h, out int i) || _h[i].ph != HookPhase.Fly) return false; tip = (_h[i].a + _h[i].d * _h[i].s) / HellFX.PPU; return true; }
        public void Latch(int h, Transform v, Vector2 off)
        { if (!Get(h, out int i)) return; ref var k = ref _h[i]; if (k.ph != HookPhase.Fly && k.ph != HookPhase.Retract) return; k.v = v; k.off = off; k.breaks = 0; Go(ref k, HookPhase.Drag); }
        public bool AddBreak(int h)
        {
            if (!Get(h, out int i) || _h[i].ph != HookPhase.Drag) return false; ref var k = ref _h[i];
            if (++k.breaks >= k.need) { Go(ref k, HookPhase.Snap); HookChainFX.OnSnap?.Invoke(h); return true; } return false;
        }
        public void Release(int h) { if (Get(h, out int i) && (_h[i].ph == HookPhase.Drag || _h[i].ph == HookPhase.Fly)) { if (_h[i].ph == HookPhase.Fly) _h[i].tip = _h[i].a + _h[i].d * _h[i].s; Go(ref _h[i], HookPhase.Release); } }
        public void SetAnchor(int h, Vector2 a) { if (Get(h, out int i)) _h[i].a = a * HellFX.PPU; }
        public HookPhase Phase(int h) => Get(h, out int i) ? _h[i].ph : HookPhase.None;
        public float Break01(int h) => Get(h, out int i) ? (float)_h[i].breaks / _h[i].need : 0;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _h.Length; i++) if (_h[i].ph != HookPhase.None) Free(i); }

        // ---------- pixel emit ----------
        void Px(float x, float y, Color32 c, bool back = true)
        {
            float sx = Mathf.Round(x), sy = Mathf.Round(y);
            if (back && _nb < MaxQ) _bk[_nb++] = new Q { x = sx, y = sy, s = 3, c = Al(K, c.a / 255f) };
            if (_nf < MaxQ) _fg[_nf++] = new Q { x = sx, y = sy, s = 1, c = c };
        }
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(c.a * a), 0, 255); return c; }
        static Vector2 N(Vector2 d) => new Vector2(-d.y, d.x);
        void Hook(Vector2 p, Vector2 d, Color32 body, Color32 tipC)
        {
            Vector2 n = N(d);
            for (int j = 0; j < HookU.Length; j++) { var q = p + d * HookU[j] + n * HookV[j]; Px(q.x, q.y, j == 6 || j == 9 || j == 13 ? tipC : body); }
        }
        void Ring(Vector2 c, float r, Color32 col, int every = 1)
        { int n = Mathf.Max(8, Mathf.CeilToInt(2 * Mathf.PI * r)); for (int i = 0; i < n; i += every) { float a = 2 * Mathf.PI * i / n; Px(c.x + Mathf.Cos(a) * r, c.y + Mathf.Sin(a) * r, col); } }
        void Chevron(Vector2 tip, Vector2 d, int len, Color32 c)
        { Vector2 n = N(d); for (int s = 0; s <= len; s++) { var p1 = tip - d * s + n * s; Px(p1.x, p1.y, c); if (s > 0) { var p2 = tip - d * s - n * s; Px(p2.x, p2.y, c); } } }
        // chain from a to b, links marching by phase (px); long link = 2px bright, short = 1px mid
        void Chain(Vector2 a, Vector2 b, float phase, Color32 hi, Color32 lo, float shakeAt = -1, int shake = 0, float t = 0)
        {
            Vector2 d = b - a; float len = d.magnitude; if (len < 1) return; d /= len; Vector2 n = N(d);
            for (float r = 0; r <= len; r += 1f)
            {
                float m = Mod(r + phase, 4f); var p = a + d * r;
                if (shakeAt >= 0 && Mathf.Abs(r - shakeAt) < 6 && shake > 0) p += n * ((((int)(t * 30) + (int)r) & 1) == 0 ? shake : -shake) * 0.5f;
                if (m < 0.5f) Px(p.x, p.y, Wh); else if (m < 2f) Px(p.x, p.y, hi); else if (m < 3f) Px(p.x, p.y, lo); else { Px(p.x + n.x, p.y + n.y, lo); Px(p.x - n.x, p.y - n.y, lo); }
            }
        }
        static float Mod(float a, float m) => a - m * Mathf.Floor(a / m);

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu; _nb = _nf = 0;
            for (int i = 0; i < _h.Length; i++)
            {
                ref var k = ref _h[i]; if (k.ph == HookPhase.None) continue;
                k.t += dt; k.pt += dt; Step(ref k, i, dt, ppu);
                if (k.ph != HookPhase.None) Draw(ref k);
            }
            int q = 0;
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
        }
        void Step(ref Hk k, int i, float dt, float ppu)
        {
            switch (k.ph)
            {
                case HookPhase.Tell: if (k.pt >= k.tell) { Go(ref k, HookPhase.Fly); k.s = 0; HookChainFX.OnLaunch?.Invoke(Hd(i)); } break;
                case HookPhase.Fly: k.s = Mathf.Min(k.L, k.s + k.speed * dt); if (k.s >= k.L) { Go(ref k, HookPhase.Retract); HookChainFX.OnMiss?.Invoke(Hd(i)); } break;
                case HookPhase.Retract: if (k.pt > 0.12f) k.s -= k.speed * 0.8f * dt; if (k.s <= 0) Rest(ref k, i); break;
                case HookPhase.Drag: if (k.v == null) { k.tip = k.last; Go(ref k, HookPhase.Release); break; } k.last = (Vector2)k.v.position * ppu + k.off; break;
                case HookPhase.Snap: if (k.pt >= SnapT) Rest(ref k, i); break;
                case HookPhase.Release: if (k.pt >= ReleaseT) Rest(ref k, i); break;
                case HookPhase.Dormant: if (k.pt >= k.cd) { int hd = Hd(i); Free(i); HookChainFX.OnReady?.Invoke(hd); } break;
            }
            if (k.ph == HookPhase.Drag || k.ph == HookPhase.Snap) k.tip = k.last;
        }
        void Rest(ref Hk k, int i) { if (k.cd > 0) Go(ref k, HookPhase.Dormant); else { int hd = Hd(i); Free(i); HookChainFX.OnReady?.Invoke(hd); } }
        void Draw(ref Hk k)
        {
            Vector2 a = k.a, d = k.d, n = N(d); float t = k.pt;
            switch (k.ph)
            {
                case HookPhase.Tell:
                {
                    float q = t / Mathf.Max(0.01f, k.tell); bool blink = q > 0.85f && ((int)(t * 15) & 1) == 0;
                    Color32 edge = blink ? Wh : O; float grow = k.L * Mathf.Min(1, q * 2.5f), hw = k.width * 0.5f + 1;
                    for (float r = 4; r <= grow; r += 3) { var p = a + d * r; Px(p.x + n.x * hw, p.y + n.y * hw, edge); Px(p.x - n.x * hw, p.y - n.y * hw, edge); }
                    for (float r = Mod(t * 30, 12) + 6; r < grow - 6; r += 12) Chevron(a + d * r, d, 2, blink ? Wh : Lt);
                    if (q > 0.25f) { var e = a + d * (k.L - (q > 0.85f ? 2 : 0)); /* v1.8.9 anticipation: ghost recoils 2px toward the anchor in the last 15% */ Hook(e, d, blink ? Wh : Md, blink ? Wh : O); Px(e.x + n.x * hw, e.y + n.y * hw, edge); Px(e.x - n.x * hw, e.y - n.y * hw, edge); }
                    Ring(a, 5 - ((int)(t * 6) & 1), blink ? Wh : O, 2);
                    break;
                }
                case HookPhase.Fly: case HookPhase.Retract:
                {
                    var tip = a + d * Mathf.Max(0, k.s); float ft = k.ph == HookPhase.Fly ? t : 1;
                    if (k.ph == HookPhase.Fly && t < 0.05f) { /* 1-frame smear: anchor->tip in the two brightest steel colours, no backing */ float sl = Mathf.Max(0, k.s - 5); for (float r = 0; r < sl; r += 1) { var p = a + d * r; Px(p.x, p.y, Wh, false); Px(p.x + n.x, p.y + n.y, Lt, false); } }
                    if (k.ph == HookPhase.Fly && t < FlashT) { Ring(a, t < FlashT * 0.5f ? 4 : 7, Wh); }
                    if (k.ph == HookPhase.Fly && t < 0.15f) { float al = 1 - t / 0.15f, hw = k.width * 0.5f + 1; for (float r = 4; r <= k.L; r += 3) { var p = a + d * r; Px(p.x + n.x * hw, p.y + n.y * hw, Al(O, al)); Px(p.x - n.x * hw, p.y - n.y * hw, Al(O, al)); } }
                    Chain(a, tip - d * 5, k.ph == HookPhase.Fly ? 0 : -k.s, Lt, Md);
                    Hook(tip, d, Lt, Wh);
                    if (k.ph == HookPhase.Fly) for (int s = 1; s <= 3; s++) { var p = tip - d * (6 + s * 3) + n * ((s & 1) == 0 ? 2 : -2); Px(p.x, p.y, Al(Wh, 1 - s * 0.25f), false); }
                    if (k.ph == HookPhase.Retract && t < 0.12f) for (int s = 0; s < 4; s++) { float an = s * 1.5708f + 0.785f, r = 2 + t * 60; Px(tip.x + Mathf.Cos(an) * r, tip.y + Mathf.Sin(an) * r, t < 0.05f ? Wh : Y); }
                    break;
                }
                case HookPhase.Drag:
                {
                    var tip = k.last; Vector2 cd = tip - a; float len = cd.magnitude; Vector2 dd = len > 0.01f ? cd / len : d, nn = N(dd);
                    float br = (float)k.breaks / k.need; bool flash = t < 0.067f;
                    Chain(a, tip - dd * 4, t * 90f, flash ? Wh : Lt, flash ? Wh : Md, len * 0.5f, k.breaks, t);
                    for (float r = Mod(-t * 60, 16) + 10; r < len - 10; r += 16) Chevron(a + dd * r, -dd, 2, ((int)(r / 16 + t * 8) & 1) == 0 ? O : Wh);
                    var mid = a + dd * (len * 0.5f); { /* weak (break) link: gold diamond r2, pulses to r3 (faster per break pip), white core */ float pf = 3f + 1.5f * k.breaks; int wr = Mathf.Sin(t * 6.2832f * pf) > 0.3f ? 3 : 2; for (int wi = -wr; wi <= wr; wi++) for (int wj = -wr; wj <= wr; wj++) { int m = Mathf.Abs(wi) + Mathf.Abs(wj); if (m > wr) continue; var wp = mid + dd * wi + nn * wj; Px(wp.x, wp.y, m <= wr - 2 ? Wh : Y); } }
                    Hook(tip, dd, Lt, Wh);
                    if (t < 0.034f) { /* contrast frame: dark disc before the white latch ring */ for (int yy = -3; yy <= 3; yy++) for (int xx = -3; xx <= 3; xx++) if (xx * xx + yy * yy <= 10) { if (_nb < MaxQ) _bk[_nb++] = new Q { x = Mathf.Round(tip.x) + xx, y = Mathf.Round(tip.y) + yy, s = 1, c = new Color32(0x10, 0, 0x10, 230) }; } }
                    else if (t < LatchT) { Ring(tip, 3 + t * 40, t < 0.06f ? Wh : Y, 2); }
                    // break pips above the victim
                    int need = k.need; float x0 = tip.x - (need * 4 - 1) * 0.5f, py = tip.y + 14;
                    for (int p = 0; p < need; p++)
                    {
                        bool full = p < k.breaks; bool pulse = !full && ((int)(k.t * 4) & 1) == 0; Color32 c = full ? Y : pulse ? Lt : Dk;
                        for (int yy = 0; yy < 3; yy++) for (int xx = 0; xx < 3; xx++) { if (xx == 1 && yy == 1 && !full) continue; Px(x0 + p * 4 + xx, py + yy, c); }
                    }
                    // sideways escape arrows (dash out perpendicular to the chain)
                    if (((int)(k.t * 6) & 1) == 0) { Chevron(tip + nn * 9, nn, 2, Y); Chevron(tip + nn * 12, nn, 2, Y); Chevron(tip - nn * 9, -nn, 2, Y); Chevron(tip - nn * 12, -nn, 2, Y); }
                    break;
                }
                case HookPhase.Snap:
                {
                    var tip = k.tip; Vector2 cd = tip - a; float len = cd.magnitude; Vector2 dd = len > 0.01f ? cd / len : d, nn = N(dd);
                    float q = t / SnapT, half = len * 0.5f; var mid = a + dd * half;
                    float back = half * Mathf.Min(1, q * 2.2f); if (half - back > 1) Chain(a, a + dd * (half - back), 0, Lt, Md);
                    if (t < 0.1f) { Ring(mid, 2 + t * 70, t < 0.05f ? Wh : Y); Ring(mid, 1 + t * 30, Wh, 2); }
                    for (int s = 0; s < 6; s++)
                    {
                        float side = (s & 1) == 0 ? 1 : -1, along = (s / 2) * 5 + 2; var p = mid + dd * along + nn * side * (2 + t * (50 + s * 8)) + Vector2.down * (t * t * 120);
                        Color32 c = Al(s < 2 ? Wh : Lt, 1 - q); Px(p.x, p.y, c); Px(p.x + dd.x, p.y + dd.y, Al(Md, 1 - q));
                    }
                    if (t < 0.2f) { int need = k.need; float x0 = tip.x - (need * 4 - 1) * 0.5f; for (int p = 0; p < need; p++) for (int yy = 0; yy < 3; yy++) for (int xx = 0; xx < 3; xx++) Px(x0 + p * 4 + xx, tip.y + 14 + yy + t * 20, Wh); }
                    break;
                }
                case HookPhase.Dormant:
                {
                    // hangs limp at the anchor (readable at 1x over the long cooldown): light steel with backing, slack chain drooping
                    // into the arena, 8 orange refill ticks; the last 0.5 s blinks white = "about to re-arm", then a ready ring.
                    Vector2 hd = (d + Vector2.down * 1.5f).normalized; var e = a + hd * 9;
                    float left = k.cd - t; bool warn = left < 0.5f && ((int)(t * 10) & 1) == 0;
                    for (int r = 1; r < 6; r++) { var p = a + hd * r; Px(p.x, p.y, (r & 1) == 0 ? Lt : Md); }
                    Hook(e, hd, warn ? Wh : Lt, warn ? Wh : Md);
                    float q = Mathf.Clamp01(t / Mathf.Max(0.01f, k.cd)); int ticks = Mathf.FloorToInt(q * 8);
                    for (int s = 0; s < 8; s++) { float an = 1.5708f - s * 0.7854f; Px(a.x + Mathf.Cos(an) * 6, a.y + Mathf.Sin(an) * 6, s < ticks ? O : Md); }
                    if (left < 0.15f) Ring(a, 5, Wh, 2); // ready glint
                    break;
                }
                case HookPhase.Release:
                {
                    float q = 1 - t / ReleaseT; var tip = a + (k.tip - a) * q; Chain(a, tip, t * 200, Lt, Md); Hook(tip, (k.tip - a).normalized, Lt, Wh); break;
                }
            }
        }
        void Write(int q, ref Q s, float u)
        {
            int v = q * 4; float h = s.s * 0.5f * u, cx = (s.x + 0.5f) * u, cy = (s.y + 0.5f) * u;
            _vtx[v] = new Vector3(cx - h, cy - h, 0); _vtx[v + 1] = new Vector3(cx - h, cy + h, 0); _vtx[v + 2] = new Vector3(cx + h, cy + h, 0); _vtx[v + 3] = new Vector3(cx + h, cy - h, 0);
            _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
