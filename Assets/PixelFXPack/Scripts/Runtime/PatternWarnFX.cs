using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Boss bullet pattern families that <see cref="PatternWarnFX"/> can telegraph (and fire through <see cref="BossBulletFX"/>).</summary>
    public enum BossPattern { Ring = 0, RadialBurst = 1, AimedFan = 2, Spiral = 3 }

    /// <summary>
    /// 113. Pattern Warning (v1.8.6). Shows WHERE a boss bullet pattern comes from and WHICH WAY it goes, for a short lead
    /// (default 0.45 s), then (optionally) fires the pattern through BossBulletFX at the end of the lead.
    /// Look: PAL14 lime/white like the boss bullet (#112). A magic-circle sigil converges onto the source while hollow "ghost"
    /// orbs (outline of the bullets to come) slide out along the exact firing angles. Each family has its own shape: Ring = dotted
    /// circle, RadialBurst = marching dashed spokes, AimedFan = wedge + chevrons, Spiral = curved arms + spin arrows.
    /// Markers blink white twice in the last 15 %, then a 2-frame release flash and a lime shock ring.
    /// Every mark has a dark #100010 backing so it reads over 125+ lime bullets. One mesh, one atlas, one material for all warnings.
    /// Angles are locked when the warning starts (shown == resolved). Visual only: no collision.
    /// </summary>
    public static class PatternWarnFX
    {
        static PatternWarnRunner R => PatternWarnRunner.I;
        /// <summary>Default lead time in seconds.</summary>
        public static float DefaultLead = 0.45f;
        /// <summary>Lifetime passed to BossBulletFX for bullets fired by a warning.</summary>
        public static float BulletLifetime = 6f;
        /// <summary>Called when a warning releases its pattern: (handle, pattern, origin). Called even when fire = false, so game code can fire its own bullets.</summary>
        public static Action<int, BossPattern, Vector2> OnFire;
        public static Material MaterialOverride;
        /// <summary>Drawn above boss bullets (70) by default.</summary>
        public static int SortingOrder = 71;
        public static string SortingLayer = "Default";

        public static void Prewarm(int warnings = 32) => R.Prewarm(warnings);

        /// <summary>Ring: <paramref name="count"/> evenly spaced bullets at one speed. Fires BossBulletFX.Ring.</summary>
        public static int WarnRing(Vector2 origin, int count = 16, float speedPx = 60, float offsetDeg = 0, BossBulletSize size = BossBulletSize.Small, float lead = -1, bool fire = true)
            => R.Add(BossPattern.Ring, origin, offsetDeg, count, speedPx, 0, size, lead, fire, 0, 0, 0, 0);

        /// <summary>Radial burst: <paramref name="count"/> Large bullets, plus <paramref name="count"/> Small at 65% speed between them (two waves).</summary>
        public static int WarnRadialBurst(Vector2 origin, int count = 10, float speedPx = 80, float offsetDeg = 0, float lead = -1, bool fire = true)
            => R.Add(BossPattern.RadialBurst, origin, offsetDeg, count, speedPx, 0, BossBulletSize.Large, lead, fire, 0, 0, 0, 0);

        /// <summary>Aimed fan toward <paramref name="target"/> (aim is locked now). Fires BossBulletFX.Spread.</summary>
        public static int WarnFan(Vector2 origin, Vector2 target, int count = 5, float arcDeg = 40, float speedPx = 90, BossBulletSize size = BossBulletSize.Large, float lead = -1, bool fire = true)
        {
            Vector2 d = target - origin; float a = d.sqrMagnitude > 1e-8f ? Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg : -90f;
            return R.Add(BossPattern.AimedFan, origin, a, count, speedPx, arcDeg, size, lead, fire, 0, 0, 0, 0);
        }

        /// <summary>Spiral: <paramref name="arms"/> arms starting at <paramref name="startDeg"/>, emission angle turning at
        /// <paramref name="spinDegPerSec"/> (positive = counter-clockwise), one bullet per arm every <paramref name="interval"/> s for <paramref name="duration"/> s.</summary>
        public static int WarnSpiral(Vector2 origin, int arms = 3, float startDeg = 0, float spinDegPerSec = 150, float duration = 1.5f, float interval = 0.1f, float speedPx = 70, BossBulletSize size = BossBulletSize.Small, float lead = -1, bool fire = true)
            => R.Add(BossPattern.Spiral, origin, startDeg, arms, speedPx, 0, size, lead, fire, spinDegPerSec, Mathf.Max(0.02f, interval), Mathf.Max(0, duration), 0);

        /// <summary>Moves a live warning (and a firing spiral) with its boss. Angles stay locked.</summary>
        public static void SetOrigin(int handle, Vector2 origin) => R.SetOrigin(handle, origin);
        /// <summary>Cancels a warning (before it fires, nothing is fired; a firing spiral stops).</summary>
        public static void Cancel(int handle) => R.Cancel(handle);
        public static bool IsAlive(int handle) => R.Alive(handle);
        /// <summary>Cancels every warning.</summary>
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => R.Count;
    }

    [AddComponentMenu("")]
    public class PatternWarnRunner : MonoBehaviour
    {
        static PatternWarnRunner _i;
        public static PatternWarnRunner I { get { if (_i == null) { var go = new GameObject("PatternWarnFX"); DontDestroyOnLoad(go); _i = go.AddComponent<PatternWarnRunner>(); } return _i; } }

        struct W
        {
            public BossPattern kind; public Vector2 o; public float baseDeg, speed, arc, lead, t, spin, interval, duration, shotT; public int count, shots;
            public BossBulletSize size; public bool fire, fired; public byte state; public ushort gen;
        }
        W[] _w = new W[0]; int[] _free; int _freeN, _count;
        public int Count => _count;

        // quad lists: backing (dark halo) first, then foreground; copied into one mesh each frame
        const int MaxQ = 3072;
        struct Q { public float x, y, s; public byte cell; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ]; int _nb, _nf, _lastUsed;
        // danger wash: horizontal 1px spans drawn by a second renderer UNDER the bullets (sorting - 2) so the threatened area darkens the floor, never the bullets
        const int MaxSpan = 2048; struct Sp { public float x0, x1, y; public Color32 c; }
        readonly Sp[] _sp = new Sp[MaxSpan]; int _ns, _lastSpan; Mesh _wmesh; Vector3[] _wv; Vector2[] _wuv; Color32[] _wc; int[] _wt;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri; MeshRenderer _mr; Texture2D _atlas;
        const int Cell = 12, Cells = 6; // 0 solid, 1 ghost S, 2 ghost S white, 3 ghost L, 4 ghost L white, 5 seed orb
        const float Hold = 0.85f, FlashT = 0.067f, ShockT = 0.12f;

        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), G = BossBulletFX.RingColor, Y = BossBulletFX.InnerColor, Wh = BossBulletFX.CoreColor;

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 4096);
            if (_w.Length < cap)
            {
                var nw = new W[cap]; Array.Copy(_w, nw, _w.Length);
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _w.Length || _w[i].state == 0) _free[_freeN++] = i;
                _w = nw;
            }
            if (_mesh != null) return;
            _vtx = new Vector3[MaxQ * 8]; _uv = new Vector2[MaxQ * 8]; _col = new Color32[MaxQ * 8]; _tri = new int[MaxQ * 12];
            for (int i = 0; i < MaxQ * 2; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3; }
            _mesh = new Mesh { name = "PatternWarn" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _mr = gameObject.AddComponent<MeshRenderer>();
            BuildAtlas();
            var m = PatternWarnFX.MaterialOverride;
            if (m == null)
            {
                var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (sh == null) sh = Shader.Find("Sprites/Default");
                m = sh != null ? new Material(sh) { name = "PatternWarn (Shared)", hideFlags = HideFlags.DontSave } : null;
            }
            if (m != null) { m.mainTexture = _atlas; _mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] PatternWarnFX: no sprite shader found; set PatternWarnFX.MaterialOverride.");
            _mr.sortingOrder = PatternWarnFX.SortingOrder; _mr.sortingLayerName = PatternWarnFX.SortingLayer;
            _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; _mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
            // wash renderer (same material, lower sorting)
            var wgo = new GameObject("PatternWarnWash"); wgo.transform.SetParent(transform, false);
            _wmesh = new Mesh { name = "PatternWarnWash" }; _wmesh.MarkDynamic();
            _wv = new Vector3[MaxSpan * 4]; _wuv = new Vector2[MaxSpan * 4]; _wc = new Color32[MaxSpan * 4]; _wt = new int[MaxSpan * 6];
            float m0 = 0.4f / Cells, m1 = 0.6f / Cells;
            for (int i = 0; i < MaxSpan; i++) { int v = i * 4, t = i * 6; _wt[t] = v; _wt[t + 1] = v + 1; _wt[t + 2] = v + 2; _wt[t + 3] = v; _wt[t + 4] = v + 2; _wt[t + 5] = v + 3;
                _wuv[v] = new Vector2(m0, 0.4f); _wuv[v + 1] = new Vector2(m0, 0.6f); _wuv[v + 2] = new Vector2(m1, 0.6f); _wuv[v + 3] = new Vector2(m1, 0.4f); }
            wgo.AddComponent<MeshFilter>().sharedMesh = _wmesh; var wmr = wgo.AddComponent<MeshRenderer>();
            if (m != null) wmr.sharedMaterial = m; wmr.sortingOrder = PatternWarnFX.SortingOrder - 2; wmr.sortingLayerName = PatternWarnFX.SortingLayer;
            wmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; wmr.receiveShadows = false;
            _wmesh.vertices = _wv; _wmesh.uv = _wuv; _wmesh.colors32 = _wc; _wmesh.triangles = _wt;
        }

        void BuildAtlas()
        {
            var px = new Color32[Cell * Cells * Cell]; var clr = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clr;
            Color32 KK = BossBulletFX.RimColor;
            void P(int cell, int x, int y, Color32 c) { px[y * Cell * Cells + cell * Cell + x] = c; }
            for (int y = 0; y < Cell; y++) for (int x = 0; x < Cell; x++)
            {
                float d = Mathf.Sqrt((x - 5.5f) * (x - 5.5f) + (y - 5.5f) * (y - 5.5f));
                P(0, x, y, Wh);
                // hollow ghost = bullet outline: dark | lime ring | dark, empty centre
                if (d > 2.6f && d <= 3.6f) { P(1, x, y, G); P(2, x, y, Wh); } else if ((d > 1.6f && d <= 2.6f) || (d > 3.6f && d <= 4.6f)) { P(1, x, y, KK); P(2, x, y, KK); }
                if (d > 4.0f && d <= 5.0f) { P(3, x, y, G); P(4, x, y, Wh); } else if ((d > 3.0f && d <= 4.0f) || (d > 5.0f && d <= 6.0f)) { P(3, x, y, KK); P(4, x, y, KK); }
                if (d <= 1.0f) P(5, x, y, Wh); else if (d <= 2.1f) P(5, x, y, Y); else if (d <= 2.6f) P(5, x, y, G); else if (d <= 3.6f) P(5, x, y, KK);
            }
            _atlas = new Texture2D(Cell * Cells, Cell, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PFX PatternWarn Atlas", hideFlags = HideFlags.DontSave };
            _atlas.SetPixels32(px); _atlas.Apply(false, true);
        }

        int H(int i) => i | (_w[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _w.Length && _w[i].state == 1 && _w[i].gen == (ushort)(h >> 16); }
        public bool Alive(int h) => Get(h, out _);
        public void SetOrigin(int h, Vector2 o) { if (Get(h, out int i)) _w[i].o = o; }
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _w.Length; i++) if (_w[i].state == 1) Free(i); }
        void Free(int i) { _w[i].state = 0; _free[_freeN++] = i; _count--; }

        public int Add(BossPattern k, Vector2 o, float baseDeg, int count, float speed, float arc, BossBulletSize size, float lead, bool fire, float spin, float interval, float duration, int _)
        {
            if (_mesh == null) Prewarm(32);
            if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ref var w = ref _w[i];
            w.kind = k; w.o = o; w.baseDeg = baseDeg; w.count = Mathf.Clamp(count, 1, 64); w.speed = speed; w.arc = arc; w.size = size;
            w.lead = lead > 0 ? lead : Mathf.Max(0.05f, PatternWarnFX.DefaultLead); w.fire = fire; w.spin = spin; w.interval = interval; w.duration = duration;
            w.t = 0; w.fired = false; w.shotT = 0; w.shots = 0; w.state = 1; w.gen = (ushort)((w.gen + 1) & 0x7FFF);
            _count++; return H(i);
        }

        // ---------- quad emit (pixel space, world origin) ----------
        void Px(float x, float y, Color32 c)
        {
            float sx = Mathf.Round(x), sy = Mathf.Round(y);
            if (_nb < MaxQ) _bk[_nb++] = new Q { x = sx, y = sy, s = 3, cell = 0, c = K };
            if (_nf < MaxQ) _fg[_nf++] = new Q { x = sx, y = sy, s = 1, cell = 0, c = c };
        }
        void Spr(float x, float y, int cell, byte a = 255)
        { if (_nf < MaxQ) _fg[_nf++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), s = 12, cell = (byte)cell, c = new Color32(255, 255, 255, a) }; }
        static Vector2 Dir(float deg) { float r = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)); }
        void Circle(Vector2 c, float r, Color32 col, int every = 1)
        {
            int n = Mathf.Max(8, Mathf.CeilToInt(2 * Mathf.PI * r));
            for (int i = 0; i < n; i += every) { float a = 2 * Mathf.PI * i / n; Px(c.x + Mathf.Cos(a) * r, c.y + Mathf.Sin(a) * r, col); }
        }
        void Ray(Vector2 c, Vector2 d, float r0, float r1, Color32 col, float dashPhase = -1)
        {
            for (float r = r0; r <= r1; r += 1f)
            {
                if (dashPhase >= 0 && Mod(r - dashPhase, 5f) >= 3f) continue;
                Px(c.x + d.x * r, c.y + d.y * r, col);
            }
        }
        void Chevron(Vector2 tip, Vector2 d, float len, Color32 col)
        {
            Vector2 n = new Vector2(-d.y, d.x);
            for (int s = 0; s <= len; s++) { Vector2 p1 = tip - d * s + n * s, p2 = tip - d * s - n * s; Px(p1.x, p1.y, col); if (s > 0) Px(p2.x, p2.y, col); }
        }
        // wash: disc (arc >= 360) or sector of radius r around c, centred on deg, as 1px rows
        void Wash(Vector2 c, float r, float deg, float arc, float a)
        {
            if (a <= 0 || r < 2) return; Color32 col = Al(new Color32(0x10, 0x00, 0x10, 255), a);
            int R = Mathf.CeilToInt(r), cx = Mathf.RoundToInt(c.x), cy = Mathf.RoundToInt(c.y); float ha = arc * 0.5f;
            for (int y = -R; y <= R; y++)
            {
                if (_ns >= MaxSpan) return;
                if (arc >= 360) { float hw = Mathf.Sqrt(Mathf.Max(0, r * r - y * y)); if (hw < 0.5f) continue; _sp[_ns++] = new Sp { x0 = cx - Mathf.Round(hw), x1 = cx + Mathf.Round(hw), y = cy + y, c = col }; continue; }
                int x0 = int.MaxValue, x1 = int.MinValue;
                for (int x = -R; x <= R; x++)
                {
                    if (x * x + y * y > r * r) continue;
                    float d = Mathf.Abs(Mathf.DeltaAngle(deg, Mathf.Atan2(y, x) * Mathf.Rad2Deg));
                    if (d <= ha || x * x + y * y <= 36) { if (x < x0) x0 = x; if (x > x1) x1 = x; }
                }
                if (x1 >= x0) _sp[_ns++] = new Sp { x0 = cx + x0, x1 = cx + x1 + 1, y = cy + y, c = col };
            }
        }
        static float Mod(float a, float m) => a - m * Mathf.Floor(a / m);
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(c.a * a), 0, 255); return c; }
        float DirDeg(ref W w, int i)
        {
            switch (w.kind)
            {
                case BossPattern.AimedFan: return w.baseDeg + (w.count > 1 ? -w.arc * 0.5f + w.arc * i / (w.count - 1) : 0);
                case BossPattern.Spiral: return w.baseDeg + 360f * i / w.count;
                default: return w.baseDeg + 360f * i / w.count;
            }
        }

        void DrawTell(ref W w, Vector2 c)
        {
            float k = w.t / w.lead, ck = Mathf.Min(1, k / Hold);
            bool blink = k >= Hold && (((int)((k - Hold) / (1 - Hold) * 4)) & 1) == 0;
            Color32 main = blink ? Wh : Y, line = blink ? Wh : G;
            bool large = w.size == BossBulletSize.Large || w.kind == BossPattern.RadialBurst;
            int ghost = (large ? 3 : 1) + (blink ? 1 : 0);
            // sigil: converging ring + 4 rotating ticks (the timer)
            float R = 20 - 13 * ck; Circle(c, R, main); Circle(c, R + 1, Al(main, 0.6f));
            float rot = w.t * (w.kind == BossPattern.Spiral ? Mathf.Sign(w.spin == 0 ? 1 : w.spin) * 240f : 240f);
            for (int q = 0; q < 4; q++) { Vector2 d = Dir(rot + 90 * q); Ray(c, d, R + 2, R + 4, Wh); }
            if (k < 0.5f) { int s = 1 + (int)(k * 6); for (int y = 0; y < s; y++) for (int x = 0; x < s; x++) Px(c.x + x - s / 2, c.y + y - s / 2, Wh); }
            else Spr(c.x, c.y, 5);
            switch (w.kind)
            {
                case BossPattern.Ring:
                {
                    float rg = 6 + 34 * ck; Wash(c, rg + 6, 0, 360, 0.45f * Mathf.Min(1, k * 4)); Circle(c, rg, line, 3);
                    for (int i = 0; i < w.count; i++) { Vector2 d = Dir(DirDeg(ref w, i)); Spr(c.x + d.x * rg, c.y + d.y * rg, ghost); }
                    break;
                }
                case BossPattern.RadialBurst:
                {
                    float L = 6 + 50 * ck, ph = w.t * 60f; Wash(c, L + 8, 0, 360, 0.45f * Mathf.Min(1, k * 4));
                    for (int i = 0; i < w.count; i++) { Vector2 d = Dir(DirDeg(ref w, i)); Ray(c, d, 6, L, line, ph); Spr(c.x + d.x * (L + 3), c.y + d.y * (L + 3), ghost); }
                    break;
                }
                case BossPattern.AimedFan:
                {
                    float L = 10 + 70 * ck, rg = 6 + 46 * ck; Vector2 bd = Dir(w.baseDeg); Wash(c, L, w.baseDeg, w.arc + 8, 0.45f * Mathf.Min(1, k * 4));
                    Ray(c, Dir(w.baseDeg - w.arc * 0.5f - 4), 6, L, line); Ray(c, Dir(w.baseDeg + w.arc * 0.5f + 4), 6, L, line);
                    for (int i = 0; i < w.count; i++) { Vector2 d = Dir(DirDeg(ref w, i)); Spr(c.x + d.x * rg, c.y + d.y * rg, ghost); }
                    for (int q = 0; q < 3; q++) { float dd = 8 + Mod(w.t * 70f + q * 12f, 36f); if (dd <= L) Chevron(c + bd * dd, bd, 4, main); }
                    break;
                }
                case BossPattern.Spiral:
                {
                    float sg = w.spin >= 0 ? 1 : -1; Wash(c, 6 + 7 * 6.5f * ck + 6, 0, 360, 0.45f * Mathf.Min(1, k * 4));
                    for (int j = 0; j < w.count; j++)
                    {
                        float a0 = DirDeg(ref w, j);
                        // arm = where the first 7 shots of this arm will be a moment after release (older shots further out)
                        for (int m = 0; m < 7; m++)
                        {
                            float rr = 6 + (7 - m) * 6.5f * ck; Vector2 d = Dir(a0 + w.spin * w.interval * m);
                            Vector2 pm = c + d * rr;
                            if (m == 0) Spr(pm.x, pm.y, ghost);
                            else { float pr = 6 + (8 - m) * 6.5f * ck; Vector2 pp = c + Dir(a0 + w.spin * w.interval * (m - 1)) * pr; float n = Mathf.Ceil((pp - pm).magnitude), stop = m == 1 ? n - 4 : n;
                                   for (int z = 0; z < stop; z++) { Vector2 q = Vector2.Lerp(pm, pp, z / n); Px(q.x, q.y, line); } } // v1.8.6: connected arm trail
                        }
                        // spin arrow: 50 deg arc at r=R+7, arrowhead at the leading end
                        float ra = R + 7, aS = a0 + rot * 0.25f;
                        for (int s = 0; s <= 50; s += 4) { Vector2 d = Dir(aS + sg * s); Px(c.x + d.x * ra, c.y + d.y * ra, main); }
                        Vector2 e = Dir(aS + sg * 54), tg = Dir(aS + sg * 54 + sg * 90);
                        Chevron(c + e * ra, tg, 2, main);
                    }
                    break;
                }
            }
        }

        void Release(ref W w, int i)
        {
            w.fired = true; w.shotT = 0; w.shots = 0;
            float lt = PatternWarnFX.BulletLifetime;
            if (w.fire)
                switch (w.kind)
                {
                    case BossPattern.Ring: BossBulletFX.Ring(w.o, w.count, w.speed, w.baseDeg, w.size, lt); break;
                    case BossPattern.RadialBurst:
                        BossBulletFX.Ring(w.o, w.count, w.speed, w.baseDeg, BossBulletSize.Large, lt);
                        BossBulletFX.Ring(w.o, w.count, w.speed * 0.65f, w.baseDeg + 180f / w.count, BossBulletSize.Small, lt); break;
                    case BossPattern.AimedFan: BossBulletFX.Spread(w.o, Dir(w.baseDeg), w.count, w.arc, w.speed, w.size, lt); break;
                    case BossPattern.Spiral: break; // fired over time in Tick
                }
            PatternWarnFX.OnFire?.Invoke(H(i), w.kind, w.o);
        }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu;
            _nb = 0; _nf = 0; _ns = 0;
            for (int i = 0; i < _w.Length; i++)
            {
                ref var w = ref _w[i]; if (w.state != 1) continue;
                w.t += dt;
                if (!w.fired && w.t >= w.lead) Release(ref w, i);
                Vector2 c = w.o * ppu;
                if (!w.fired) { DrawTell(ref w, c); continue; }
                float ft = w.t - w.lead; bool firing = false;
                if (w.kind == BossPattern.Spiral && w.fire)
                {
                    float uu = 1f / ppu;
                    while (w.shotT <= ft && w.shotT <= w.duration)
                    {
                        for (int j = 0; j < w.count; j++) { float a = DirDeg(ref w, j) + w.spin * w.shotT; BossBulletFX.Spawn(w.o, Dir(a) * w.speed * uu, w.size, PatternWarnFX.BulletLifetime, w.shots == 0); }
                        w.shots++; w.shotT = w.shots * w.interval;
                    }
                    firing = ft <= w.duration;
                    if (firing) { Circle(c, 6, Al(G, 0.7f), 2); Vector2 d = Dir(w.baseDeg + w.spin * ft); Ray(c, d, 7, 10, Wh); }
                }
                // release: 2 white frames then a lime shock ring
                if (ft < FlashT) { Circle(c, ft < FlashT * 0.5f ? 7 : 11, Wh); if (ft < FlashT * 0.5f) Circle(c, 4, Y); }
                else if (ft < FlashT + ShockT) Circle(c, 11 + (ft - FlashT) * 140f, Al(G, 1 - (ft - FlashT) / ShockT), 2);
                if (!firing && ft >= FlashT + ShockT) Free(i);
            }
            // write mesh: backing then foreground
            int q = 0; float cw = 1f / Cells;
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u, cw);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u, cw);
            for (int z = q; z < _lastUsed; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _lastUsed = q;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col;
            _mesh.RecalculateBounds();
            for (int s = 0; s < _ns; s++)
            {
                ref var sp = ref _sp[s]; int v = s * 4; float x0 = sp.x0 * u, x1 = sp.x1 * u, y0 = sp.y * u, y1 = y0 + u;
                _wv[v] = new Vector3(x0, y0, 0); _wv[v + 1] = new Vector3(x0, y1, 0); _wv[v + 2] = new Vector3(x1, y1, 0); _wv[v + 3] = new Vector3(x1, y0, 0);
                _wc[v] = _wc[v + 1] = _wc[v + 2] = _wc[v + 3] = sp.c;
            }
            for (int z = _ns; z < _lastSpan; z++) { int v = z * 4; _wv[v] = _wv[v + 1] = _wv[v + 2] = _wv[v + 3] = Vector3.zero; }
            _lastSpan = _ns;
            _wmesh.vertices = _wv; _wmesh.colors32 = _wc; _wmesh.RecalculateBounds();
        }
        void Write(int q, ref Q s, float u, float cw)
        {
            int v = q * 4; float h = s.s * 0.5f * u, cx = s.x * u, cy = s.y * u;
            if (s.s != 12) { cx += 0.5f * u; cy += 0.5f * u; } // odd-sized quads: centre on the pixel centre so they fill whole pixels
            _vtx[v] = new Vector3(cx - h, cy - h, 0); _vtx[v + 1] = new Vector3(cx - h, cy + h, 0); _vtx[v + 2] = new Vector3(cx + h, cy + h, 0); _vtx[v + 3] = new Vector3(cx + h, cy - h, 0);
            float u0 = s.cell * cw, u1 = u0 + cw;
            if (s.cell == 0) { float m = 0.4f * cw; u0 += m; u1 -= m; }
            _uv[v] = new Vector2(u0, 0); _uv[v + 1] = new Vector2(u0, 1); _uv[v + 2] = new Vector2(u1, 1); _uv[v + 3] = new Vector2(u1, 0);
            _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
