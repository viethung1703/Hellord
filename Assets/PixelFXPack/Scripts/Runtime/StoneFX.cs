using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Kind of a <see cref="StoneFX"/> item.</summary>
    public enum StoneKind { None = 0, BoulderDrop = 1, BoulderThrow = 2, QuakeLine = 3, RockArmor = 4, DisguisePop = 5 }
    /// <summary>Phase of a <see cref="StoneFX"/> item.</summary>
    public enum StonePhase { None = 0, Tell = 1, Strike = 2, Active = 3, Broken = 4 }

    /// <summary>
    /// 123-126. Stone Crushing (Hellord Level 11) tells (v1.9.6). Sizes are world pixels (16 PPU, 640x360 view), from the feet pivot.
    /// Rules from producer QC: bright border on every danger area, fills kept clear of the hero's red, a visible wind-up on every attack,
    /// icons at least 5x7 on dark plates, pixel-snapped fills.
    /// 123 BOULDER DROP (<see cref="BoulderDrop"/>): level hazard and Stone Warden ceiling falls. A dark shadow grows on the landing spot inside a crawling
    ///   yellow/dark hazard border while the boulder falls from above (the falling rock + growing shadow are the wind-up); white blink in the last 15 %.
    ///   IMPACT 0.35 s: white ring, dust ring, pebbles, ground cracks. <see cref="StoneFX.OnStrike"/>.
    /// 123b BOULDER THROW (<see cref="BoulderThrow"/>): Warden attack. Wind-up glow at the hand, a dotted yellow arc to the target, an orange 45 % landing
    ///   circle with a bright yellow/orange 2 px border; the boulder flies the arc in the second half and lands as the tell ends.
    /// 124 QUAKE LINE (<see cref="QuakeLine"/>): wind-up stomp burst at the origin, then a bright yellow-orange 20 px band (v1.9.7) with a bright border and a crack that races along it;
    ///   STRIKE 0.45 s: stone spikes erupt one after another down the line.
    /// 125 ROCK ARMOR (<see cref="RockArmor"/>): grey plate pips above the boss head; <see cref="ArmorHit"/> cracks one; the last one breaks the armor
    ///   (shard burst) and starts the OPEN punish window: pulsing bright corner brackets around the boss and a 5x7 broken-shield icon on a dark plate.
    /// 126 DISGUISE POP (<see cref="DisguisePop"/>): Rock Crawler. Wind-up: pebbles jitter, a bright dashed ring tightens and a 5x7 '!' on a dark plate
    ///   bobs above; POP 0.35 s: dust ring + rock chips.
    /// Visual only. One pooled mesh, one 4x4 white texture, one material (1 draw call); no per-frame allocations; int handles, -1 when the pool is full.
    /// </summary>
    public static class StoneFX
    {
        static StoneRunner R => StoneRunner.I;
        public static float BoulderRadiusPx = 14f, BoulderTell = 0.9f, BoulderFallPx = 110f, ImpactSeconds = 0.35f, ThrowTell = 0.9f, ThrowHandPx = 60f;
        public static float QuakeWidthPx = 20f, QuakeTell = 0.8f, QuakeStrike = 0.45f, ArmorHeadPx = 100f, ArmorTopMarginPx = 24f, ArmorBoxPx = 40f, OpenSeconds = 4f, PopTell = 0.5f, PopSeconds = 0.35f;
        public static Material MaterialOverride; public static int SortingOrder = 70; public static string SortingLayer = "Default";
        /// <summary>(handle) when a boulder lands, a quake erupts or a crawler pops.</summary>
        public static Action<int> OnStrike;
        /// <summary>(handle) when the armor breaks (punish window opens) / when it re-forms.</summary>
        public static Action<int> OnArmorBreak, OnArmorRestore;

        public static void Prewarm(int items = 32) => R.Prewarm(items);
        public static int BoulderDrop(Vector2 spot, float radiusPx = -1, float tell = -1)
            => R.Add(StoneKind.BoulderDrop, spot, spot, radiusPx > 0 ? radiusPx : BoulderRadiusPx, 0, tell >= 0 ? tell : BoulderTell, ImpactSeconds, 0, null);
        public static int BoulderThrow(Vector2 bossFeet, Vector2 spot, float radiusPx = -1, float tell = -1)
            => R.Add(StoneKind.BoulderThrow, bossFeet, spot, radiusPx > 0 ? radiusPx : BoulderRadiusPx, 0, tell >= 0 ? tell : ThrowTell, ImpactSeconds, 0, null);
        public static int QuakeLine(Vector2 from, Vector2 to, float widthPx = -1, float tell = -1)
            => R.Add(StoneKind.QuakeLine, from, to, 0, widthPx > 0 ? widthPx : QuakeWidthPx, tell >= 0 ? tell : QuakeTell, QuakeStrike, 0, null);
        public static int RockArmor(Transform boss, int plates = 4)
            => boss == null ? -1 : R.Add(StoneKind.RockArmor, boss.position, boss.position, 0, 0, 0, 0, Mathf.Clamp(plates, 1, 8), boss);
        /// <summary>Cracks one plate; returns true when this hit broke the armor.</summary>
        public static bool ArmorHit(int h) => R.ArmorHit(h);
        public static int DisguisePop(Vector2 pos, float tell = -1)
            => R.Add(StoneKind.DisguisePop, pos, pos, 0, 0, tell >= 0 ? tell : PopTell, PopSeconds, 0, null);
        public static StonePhase GetPhase(int h) => R.Phase(h);
        public static void Cancel(int h) => R.Cancel(h);
        public static void Clear() => R.ClearAll();
        public static int ActiveCount => StoneRunner.Exists ? R.Count : 0;
    }

    public class StoneRunner : MonoBehaviour
    {
        static StoneRunner _i;
        public static bool Exists => _i != null;
        public static StoneRunner I { get { if (_i == null) { var go = new GameObject("StoneFX"); DontDestroyOnLoad(go); _i = go.AddComponent<StoneRunner>(); } return _i; } }

        struct It { public StoneKind kind; public StonePhase ph; public Vector2 a, b; public float L, W, tell, dur, pt, age; public int plates, cracked; public Transform v; public ushort gen; }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }

        const int MaxQ = 8192, MaxF = 4096;
        struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _bk = new Q[MaxQ], _fg = new Q[MaxQ], _fl = new Q[MaxF]; int _nb, _nf, _nl, _last;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri;
        // Endesga 32
        static readonly Color32 K = new Color32(0x10, 0x00, 0x10, 170), Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255),
            Y = new Color32(0xfe, 0xe7, 0x61, 255), O = new Color32(0xf7, 0x76, 0x22, 255), G = new Color32(0xfe, 0xae, 0x34, 255),
            Lt = new Color32(0x8b, 0x9b, 0xb4, 255), Lt2 = new Color32(0xc0, 0xcb, 0xdc, 255), Sl = new Color32(0x5a, 0x69, 0x88, 255), Sd = new Color32(0x3a, 0x44, 0x66, 255),
            Ds = new Color32(0xea, 0xd4, 0xaa, 255), Sh = new Color32(0x18, 0x14, 0x25, 255);
        // 5x7 glyphs, rows top->bottom
        static readonly string[] Ex7 = { ".WWW.", "WWWWW", "WWWWW", ".WWW.", ".WWW.", ".....", ".WWW." };   // bold, fills the 5x7 cell
        static readonly string[] Ex9 = { ".WWWWW.", "WWWWWWW", "WWWWWWW", ".WWWWW.", "..WWW..", "..WWW..", ".......", "..WWW..", "..WWW.." };   // 7x9 pop icon
        static readonly string[] Sh7 = { "WW.WW", "W..WW", "W.W.W", "WW..W", ".W.W.", ".WW..", "..W.." };   // broken shield

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 4, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length); _free = new int[cap]; _freeN = 0;
                for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || _it[i].kind == StoneKind.None) _free[_freeN++] = i; _it = n;
            }
            if (_mesh != null) return;
            int qs = MaxQ * 2 + MaxF;
            _vtx = new Vector3[qs * 4]; _uv = new Vector2[qs * 4]; _col = new Color32[qs * 4]; _tri = new int[qs * 6];
            for (int i = 0; i < qs; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3;
                _uv[v] = new Vector2(.25f, .25f); _uv[v + 1] = new Vector2(.25f, .75f); _uv[v + 2] = new Vector2(.75f, .75f); _uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "StoneFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Stone White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = StoneFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "StoneFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] StoneFX: no sprite shader found; set StoneFX.MaterialOverride.");
            mr.sortingOrder = StoneFX.SortingOrder; mr.sortingLayerName = StoneFX.SortingLayer;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].kind != StoneKind.None && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].kind = StoneKind.None; _it[i].ph = StonePhase.None; _it[i].v = null; _free[_freeN++] = i; Count--; }
        static void Go(ref It k, StonePhase p) { k.ph = p; k.pt = 0; }
        int Alloc() { if (_mesh == null) Prewarm(24); if (_freeN == 0) return -1; int i = _free[--_freeN]; _it[i].gen = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++; return i; }

        public int Add(StoneKind kind, Vector2 a, Vector2 b, float L, float W, float tell, float dur, int plates, Transform v)
        {
            int i = Alloc(); if (i < 0) return -1; ref var k = ref _it[i]; float ppu = HellFX.PPU;
            k.kind = kind; k.a = a * ppu; k.b = b * ppu; k.L = L; k.W = W; k.tell = kind == StoneKind.RockArmor ? 0 : Mathf.Max(0.2f, tell); k.dur = dur;
            k.plates = plates; k.cracked = 0; k.v = v; k.age = 0;
            Go(ref k, kind == StoneKind.RockArmor ? StonePhase.Active : StonePhase.Tell); return Hd(i);
        }
        public bool ArmorHit(int h)
        {
            if (!Get(h, out int i)) return false; ref var k = ref _it[i]; if (k.kind != StoneKind.RockArmor || k.ph != StonePhase.Active) return false;
            k.cracked++; k.age = 0; if (k.cracked < k.plates) return false;
            Go(ref k, StonePhase.Broken); StoneFX.OnArmorBreak?.Invoke(h); return true;
        }
        public StonePhase Phase(int h) => Get(h, out int i) ? _it[i].ph : StonePhase.None;
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].kind != StoneKind.None) Free(i); }

        // ---------- emit ----------
        void Px(float x, float y, Color32 c, bool back = true)
        {
            float sx = Mathf.Round(x), sy = Mathf.Round(y);
            if (back && _nb < MaxQ) _bk[_nb++] = new Q { x = sx - 1, y = sy - 1, w = 3, h = 3, c = K };
            if (_nf < MaxQ) _fg[_nf++] = new Q { x = sx, y = sy, w = 1, h = 1, c = c };
        }
        void Rect(float x, float y, float w, float h, Color32 c) { if (_nf < MaxQ) _fg[_nf++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
        void Span(int x0, int x1, int y, Color32 c) { if (x1 > x0 && _nl < MaxF) _fl[_nl++] = new Q { x = x0, y = y, w = x1 - x0, h = 1, c = c }; }
        // pixel-snapped filled ellipse (whole-pixel row spans)
        void FillEll(Vector2 c0, float rx, float ry, Color32 c)
        {
            int cy = Mathf.RoundToInt(c0.y), ry0 = Mathf.CeilToInt(ry);
            for (int y = -ry0; y < ry0; y++) { float yy = (y + 0.5f) / ry; if (yy * yy >= 1) continue; float hw = rx * Mathf.Sqrt(1 - yy * yy); Span(Mathf.RoundToInt(c0.x - hw), Mathf.RoundToInt(c0.x + hw), cy + y, c); }
        }
        // pixel-snapped convex quad
        readonly Vector2[] _pp = new Vector2[4];
        void FillQuad(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color32 c)
        {
            _pp[0] = p0; _pp[1] = p1; _pp[2] = p2; _pp[3] = p3;
            float y0 = Mathf.Min(Mathf.Min(p0.y, p1.y), Mathf.Min(p2.y, p3.y)), y1 = Mathf.Max(Mathf.Max(p0.y, p1.y), Mathf.Max(p2.y, p3.y));
            for (int y = Mathf.RoundToInt(y0); y < Mathf.RoundToInt(y1); y++)
            {
                float yc = y + 0.5f, lo = float.MaxValue, hi = float.MinValue;
                for (int e = 0; e < 4; e++) { Vector2 a = _pp[e], b = _pp[(e + 1) & 3]; if ((a.y <= yc) == (b.y <= yc)) continue; float x = a.x + (yc - a.y) / (b.y - a.y) * (b.x - a.x); if (x < lo) lo = x; if (x > hi) hi = x; }
                if (hi > lo) Span(Mathf.RoundToInt(lo), Mathf.RoundToInt(hi), y, c);
            }
        }
        void Ring(Vector2 c0, float rx, float ry, Color32 c, bool back = true, int dash = 0, int crawl = 0, Color32 c2 = default)
        {
            int n = Mathf.Max(12, Mathf.CeilToInt(2 * Mathf.PI * Mathf.Max(rx, ry)));
            for (int j = 0; j < n; j++) { float an = j * Mathf.PI * 2 / n; Color32 cc = dash > 0 && (((j + crawl) / dash) & 1) == 1 ? c2 : c; Px(c0.x + Mathf.Cos(an) * rx, c0.y + Mathf.Sin(an) * ry, cc, back); }
        }
        static Color32 Al(Color32 c, float a) { c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(255 * a), 0, 255); return c; }
        static Vector2 N(Vector2 d) => new Vector2(-d.y, d.x);
        static float H(float x) { x = Mathf.Sin(x * 12.9898f) * 43758.5453f; return x - Mathf.Floor(x); }
        void Line(Vector2 a, Vector2 b, Color32 c, bool back = true)
        { Vector2 d = b - a; float len = d.magnitude; if (len < 0.5f) { Px(a.x, a.y, c, back); return; } d /= len; for (int r = 0; r <= (int)len; r++) { var p = a + d * r; Px(p.x, p.y, c, back); } }
        void Star(Vector2 p, Color32 c, int r) { Px(p.x, p.y, Wh); for (int s = 1; s <= r; s++) { Px(p.x + s, p.y, c); Px(p.x - s, p.y, c); Px(p.x, p.y + s, c); Px(p.x, p.y - s, c); } }
        // 5x7 glyph on an opaque 7x9 dark plate; (x,y) = bottom-left of the glyph
        void Icon(string[] g, float x, float y, Color32 c) { Rect(x - 1, y - 1, 7, 9, Ko); for (int r = 0; r < 7; r++) for (int cc = 0; cc < 5; cc++) if (g[r][cc] == 'W') Rect(x + cc, y + 6 - r, 1, 1, c); }
        void Icon9(string[] g, float x, float y, Color32 c) { Rect(x - 1, y - 1, 9, 11, Ko); for (int r = 0; r < 9; r++) for (int cc = 0; cc < 7; cc++) if (g[r][cc] == 'W') Rect(x + cc, y + 8 - r, 1, 1, c); }
        void Rock(Vector2 p, float r, bool back = true)
        {
            if (back) FillEll(p, r + 1, r + 1, Ko);
            FillEll(p, r, r, Sl); FillEll(p + new Vector2(-r * 0.25f, r * 0.3f), r * 0.55f, r * 0.45f, Lt); Px(p.x - r * 0.35f, p.y + r * 0.45f, Lt2, false);
        }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu; _nb = _nf = _nl = 0;
            for (int i = 0; i < _it.Length; i++)
            {
                ref var k = ref _it[i]; if (k.kind == StoneKind.None) continue;
                k.pt += dt; k.age += dt;
                if (k.kind == StoneKind.RockArmor) { if (k.v == null) { Free(i); continue; } k.a = (Vector2)k.v.position * ppu; }
                Step(ref k, i);
                if (k.kind != StoneKind.None) Draw(ref k);
            }
            int q = 0;
            for (int f = 0; f < _nl; f++) Write(q++, ref _fl[f], u);
            for (int b = 0; b < _nb; b++) Write(q++, ref _bk[b], u);
            for (int f = 0; f < _nf; f++) Write(q++, ref _fg[f], u);
            for (int z = q; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = q; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
        }
        void Step(ref It k, int i)
        {
            switch (k.ph)
            {
                case StonePhase.Tell: if (k.pt >= k.tell) { StoneFX.OnStrike?.Invoke(Hd(i)); Go(ref k, StonePhase.Strike); } break;
                case StonePhase.Strike: if (k.pt >= k.dur) Free(i); break;
                case StonePhase.Broken: if (k.pt >= StoneFX.OpenSeconds) { k.cracked = 0; Go(ref k, StonePhase.Active); StoneFX.OnArmorRestore?.Invoke(Hd(i)); } break;
            }
        }
        void Impact(Vector2 c0, float R, float s)
        {
            if (s < 0.25f) Ring(c0, R * (0.8f + s), R * 0.6f * (0.8f + s), Wh, false);
            Ring(c0, R * (1 + s * 0.9f), R * 0.6f * (1 + s * 0.9f), Al(Ds, 1 - s), false);
            for (int j = 0; j < 4; j++) { float an = j * 1.5708f + 0.6f; var d = new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.6f); Line(c0 + d * 3, c0 + d * (3 + R * 0.8f * Mathf.Min(1, s * 3)), Al(Sh, 1 - s * 0.7f), false); }
            for (int j = 0; j < 8; j++) { float an = j * 0.785f + H(j) * 0.5f, sp = R * (0.6f + H(j + 3) * 0.6f); var p = c0 + new Vector2(Mathf.Cos(an) * sp * s * 2, Mathf.Sin(an) * sp * 0.6f * s * 2 + 10 * s - 22 * s * s); Rect(p.x, p.y, 2, 2, j % 3 == 0 ? Lt2 : Lt); }
            if (s < 0.5f) Rock(c0 + new Vector2(0, 2), R * 0.45f * (1 - s), false);
        }

        void Draw(ref It k)
        {
            float t = k.pt, q = k.tell > 0 ? Mathf.Clamp01(t / k.tell) : 0; bool tell = k.ph == StonePhase.Tell, blink = tell && q > 0.85f && ((int)(t * 20) & 1) == 0;
            Vector2 a = k.a;
            switch (k.kind)
            {
                case StoneKind.BoulderDrop:
                {
                    float R = k.L;
                    if (tell)
                    {
                        float g = 0.45f + 0.55f * q; FillEll(a, R * g, R * 0.6f * g, Al(Sh, 0.30f + 0.35f * q));          // growing shadow
                        Ring(a, R, R * 0.6f, blink ? Wh : Y, true, 3, (int)(t * 16), Ko);                                       // bright hazard border
                        Rock(a + new Vector2(0, StoneFX.BoulderFallPx * (1 - q * q) + R * 0.4f), R * 0.7f);                       // falling boulder = wind-up
                        if (q > 0.5f) for (int j = -1; j <= 1; j += 2) Line(a + new Vector2(j * R * 0.5f, StoneFX.BoulderFallPx * (1 - q * q) + R * 1.3f), a + new Vector2(j * R * 0.5f, StoneFX.BoulderFallPx * (1 - q * q) + R * 1.3f + 6), Lt2, false);
                    }
                    else Impact(a, R, t / k.dur);
                    break;
                }
                case StoneKind.BoulderThrow:
                {
                    float R = k.L; Vector2 hand = a + new Vector2(0, StoneFX.ThrowHandPx), e = k.b; float apex = Mathf.Max(40, (e - hand).magnitude * 0.35f);
                    if (tell)
                    {
                        FillEll(e, R * (0.4f + 0.6f * q), R * 0.6f * (0.4f + 0.6f * q), Al(O, 0.45f));
                        Ring(e, R, R * 0.6f, blink ? Wh : Y); Ring(e, R - 1, R * 0.6f - 1, O, false);
                        float tr = (t * 1.6f) % 1f;   // v1.9.7 QC: 3x3 dots with a 1 px dark outline + solid white tracer
                        for (int j = 1; j < 48; j++)
                        {
                            float s = j / 48f; var p = Vector2.Lerp(hand, e, s) + new Vector2(0, apex * 4 * s * (1 - s));
                            bool head = s <= tr && s > tr - 0.12f; if (!head && j % 3 != 0) continue;
                            if (_nb < MaxQ) _bk[_nb++] = new Q { x = Mathf.Round(p.x) - 2, y = Mathf.Round(p.y) - 2, w = 5, h = 5, c = Ko };
                            Rect(p.x - 1, p.y - 1, 3, 3, head || blink ? Wh : Y);
                        }
                        if (q < 0.5f) { Rock(hand, R * 0.7f); Ring(hand, 6 + q * 10, 6 + q * 10, ((int)(t * 20) & 1) == 0 ? Wh : Y, true); }      // wind-up at the hand
                        else { float s = (q - 0.5f) * 2; Rock(Vector2.Lerp(hand, e, s) + new Vector2(0, apex * 4 * s * (1 - s)), R * 0.7f); }
                    }
                    else Impact(e, R, t / k.dur);
                    break;
                }
                case StoneKind.QuakeLine:
                {
                    Vector2 e = k.b, d = (e - a); float L = d.magnitude; if (L < 1) break; d /= L; Vector2 n = N(d); float hw = k.W * 0.5f;
                    if (tell)
                    {
                        if (q < 0.3f) { float w = q / 0.3f; Star(a, ((int)(t * 30) & 1) == 0 ? Wh : Y, 3); Ring(a, 4 + w * 12, 2 + w * 6, Ds, true); }   // stomp wind-up
                        Vector2 f = a + d * (L * q);
                        FillQuad(a + n * hw, f + n * hw, f - n * hw, a - n * hw, Al(G, 0.60f));   // v1.9.7: bright yellow-orange, clear of enemy reds
                        Color32 brd = blink ? Wh : Y; Line(a + n * hw, e + n * hw, brd); Line(a - n * hw, e - n * hw, brd);
                        Line(a + n * (hw - 1), e + n * (hw - 1), G, false); Line(a - n * (hw - 1), e - n * (hw - 1), G, false);
                        for (int s = 0; s < 3; s++) Line(e + n * hw - d * s, e - n * hw - d * s, s == 1 ? brd : G, s != 1);
                        int m = (int)(L * q); for (int r = 0; r <= m; r++) { float z = ((r / 4) & 1) == 0 ? (r % 4) - 2 : 2 - (r % 4); var p = a + d * r + n * z * 0.8f; Px(p.x, p.y, r > m - 4 ? Wh : Sh, false); }
                    }
                    else
                    {
                        float s = t / k.dur, front = L * Mathf.Min(1, s * 1.6f);
                        for (float r = 6; r <= front; r += 10)
                        {
                            float age = (front - r) / (L * 1.6f) / Mathf.Max(0.01f, k.dur) * k.dur; float hgt = Mathf.Clamp01(1 - age * 2.5f) * 9; if (hgt < 1) continue;
                            var p = a + d * r + n * ((H(r) - 0.5f) * hw);
                            Rect(p.x - 2, p.y - 1, 5, hgt + 2, Ko); Rect(p.x - 1, p.y, 3, hgt, Lt); Rect(p.x, p.y + hgt - 1, 1, 1, Wh); Rect(p.x - 1, p.y, 1, hgt, Sl);
                        }
                        if (s < 0.3f) { Line(a + n * hw, e + n * hw, Wh, false); Line(a - n * hw, e - n * hw, Wh, false); }
                    }
                    break;
                }
                case StoneKind.RockArmor:
                {
                    Vector2 hd = a + new Vector2(0, StoneFX.ArmorHeadPx); float camTop = (Camera.main != null ? Camera.main.transform.position.y + Camera.main.orthographicSize : 1e6f) * HellFX.PPU; if (hd.y + 10 > camTop - StoneFX.ArmorTopMarginPx) hd.y = a.y - 14; /* v1.9.7: flips under the feet near the top wall */ float w = k.plates * 11 - 1, x0 = hd.x - w * 0.5f;
                    if (k.ph == StonePhase.Active)
                    {
                        for (int j = 0; j < k.plates; j++)
                        {
                            float x = x0 + j * 11; bool cr = j < k.cracked, fresh = cr && j == k.cracked - 1 && k.age < 0.25f;   // v1.9.7: 8x6 bright pips, 1 px dark outline (JudgeFX #116 size)
                            Rect(x - 1, hd.y - 1, 10, 8, Ko); Rect(x, hd.y, 8, 6, cr ? Sd : Lt2); Rect(x, hd.y + 5, 8, 1, cr ? Sl : Wh); Rect(x, hd.y, 8, 1, cr ? Sd : Lt);
                            if (cr) { Rect(x + 4, hd.y + 1, 1, 4, Ko); Rect(x + 3, hd.y + 3, 1, 1, Ko); Rect(x + 5, hd.y + 2, 1, 1, Ko); }
                            if (fresh) Star(new Vector2(x + 4, hd.y + 3), Y, 3);
                        }
                    }
                    else
                    {
                        float s = t; bool on = ((int)(t * 6) & 1) == 0 || StoneFX.OpenSeconds - t > 0.5f && ((int)(t * 4) & 1) == 0;
                        if (s < 0.4f) for (int j = 0; j < 12; j++) { float an = j * 0.5236f + H(j) * 0.3f, sp = 30 + H(j + 5) * 30; var p = hd + new Vector2(Mathf.Cos(an) * sp * s * 2, Mathf.Sin(an) * sp * s * 2 - 40 * s * s); Rect(p.x, p.y, 3, 2, j % 2 == 0 ? Lt : Lt2); }
                        if (s < 0.15f) Ring(hd, 8 + s * 60, 8 + s * 60, Wh, false);
                        float bx = StoneFX.ArmorBoxPx, by = StoneFX.ArmorHeadPx * 0.55f; Vector2 c0 = a + new Vector2(0, StoneFX.ArmorHeadPx * 0.45f); float pulse = ((int)(t * 8) & 1);
                        Color32 bc = on ? Y : O;
                        for (int sx = -1; sx <= 1; sx += 2) for (int sy = -1; sy <= 1; sy += 2)
                        { var cpt = c0 + new Vector2(sx * (bx + pulse), sy * (by + pulse)); Line(cpt, cpt - new Vector2(sx * 8, 0), bc); Line(cpt, cpt - new Vector2(0, sy * 8), bc); }
                        Icon(Sh7, hd.x - 2, hd.y + 1 + pulse, on ? Y : G);
                    }
                    break;
                }
                case StoneKind.DisguisePop:
                {
                    if (tell)
                    {
                        bool wb = q > 0.75f && ((int)(t * 20) & 1) == 0;   // v1.9.6 QC: white blink before the pop
                        float r = 20 - 8 * q; Ring(a, r, r * 0.6f, wb ? Wh : Y, true, 3, (int)(t * 20), O); Ring(a, r + 1, r * 0.6f + 1, wb ? Wh : Y, false, 3, (int)(t * 20), O);
                        if (wb) Star(a + new Vector2(0, 4), Wh, 4);
                        for (int j = 0; j < 5; j++) { float an = j * 1.2566f + 0.3f, jit = ((int)(t * 24 + j) & 1); var p = a + new Vector2(Mathf.Cos(an) * 6, Mathf.Sin(an) * 3 + jit); Rect(p.x, p.y, 2, 2, j % 2 == 0 ? Lt : Sl); }
                        Icon9(Ex9, a.x - 3, a.y + 18 + ((int)(t * 6) & 1), wb ? Wh : Y);
                    }
                    else
                    {
                        float s = t / k.dur; Ring(a, 6 + s * 16, (6 + s * 16) * 0.6f, Al(Ds, 1 - s), false); if (s < 0.3f) Star(a + new Vector2(0, 4), Wh, 4);
                        for (int j = 0; j < 6; j++) { float an = 0.5f + j * 0.42f, sp = 26 + H(j) * 14; var p = a + new Vector2(Mathf.Cos(an) * sp * s, Mathf.Sin(an) * sp * s + 18 * s - 30 * s * s); Rect(p.x, p.y, 2, 2, j % 2 == 0 ? Lt : Sl); }
                    }
                    break;
                }
            }
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
