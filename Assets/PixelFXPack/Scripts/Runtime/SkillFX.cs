using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// 132-136. HERO SKILL FX (v2.1.0). One hue family + one silhouette/motion per skill, and a shared HERO SIGNATURE so the player's own attacks
    /// never borrow enemy tell language (no red/orange fills, no bright filled borders, no chevrons, no blink-to-strike):
    ///   HERO SIGNATURE (v2.1.1) = own hue family per skill + dark #181425 outline on solids, strokes only (never a filled area), continuous motion (sweep / spin /
    ///   travel / radiate outward; never closes in, brightness never toggles), and a CLEAR ZONE around the hero (SetHero; ClearZonePx 26, alpha 0.3). White is an accent
    ///   only (frost edge, storm bolt core); Lv5 crowns use the skill hue, not white, so Lv5 with all skills is not a white cloud.
    /// 132 Fireball   GOLD/AMBER  #fee761 core + #feae34 rim, 13 px head + 6-step amber comet trail (travel).          FireballSpawn/Move/End, FireballBurst.
    /// 133 FrostCleave CYAN       #2ce8f5 + white, crescent band sweeping the arc + ice motes.         FrostCleave.
    /// 134 StormRing  DEEP BLUE   #0099db/#124e89 + white, 3-5 rotating arc segments (never a full ring) + zigzag bolts. StormRing (per frame), StormTick, StormArc.
    /// 135 BoneOrbit  BONE+GREEN  #ead4aa skull, #63c74d glow rim + green orbit trail (80 deg at Lv3).  BoneOrbit (per frame), BoneHit.
    /// 136 CurseRune  VIOLET      #68386c/#b55088 + white, glyph circle draws in (arm), rotates while short arcs radiate OUTWARD (fuse), burst. CurseRune -> OnRuneBurst; CurseMark (per frame).
    /// LEVEL 1..5: size comes from the game (radius / arc you pass). Tier detail: Lv3+ adds a second layer (thicker band, extra segment, twin sparks,
    /// longer trail, second glyph ring); Lv5 adds a white spark crown. Same hue and silhouette at every level, so an upgrade reads as MORE, not DIFFERENT.
    /// Visual only, 1 pooled mesh (1 draw call), no per-frame alloc after Prewarm, int handles (-1 when the pool is full). Positions in world units (16 PPU).
    /// </summary>
    public static class SkillFX
    {
        static SkillRunner R => SkillRunner.I;
        public static Material MaterialOverride; public static int SortingOrder = 60; public static string SortingLayer = "Default";
        public static int MaxItems = 512;
        /// <summary>v2.1.1 clear zone: every SkillFX quad within ClearZonePx of the hero (SetHero) is drawn at ClearZoneAlpha, fading back to full at 2x the radius,
        /// so enemy tells on or next to the hero always stay readable even with all skills at Lv5. 0 = off.</summary>
        public static float ClearZonePx = 26f, ClearZoneAlpha = 0.3f;
        public static void SetHero(Vector2 heroPos) => R._hero = heroPos * HellFX.PPU;
        /// <summary>(handle, pos) when a CurseRune detonates. Apply damage here.</summary>
        public static Action<int, Vector2> OnRuneBurst;

        public static void Prewarm() => R.Prewarm();
        /// <summary>Visual fireball. Move it every frame with FireballMove (it hides itself if not moved for 0.25 s).</summary>
        public static int FireballSpawn(Vector2 pos, Vector2 dir, int level = 1) => R.Spawn(0, pos * HellFX.PPU, dir, 0, 0, 0, 0, level);
        public static void FireballMove(int h, Vector2 pos, Vector2 dir) => R.Move(h, pos * HellFX.PPU, dir);
        /// <summary>Ends the shot; burst=true plays FireballBurst at its last position with burstRadius (world units).</summary>
        public static void FireballEnd(int h, bool burst = true, float burstRadius = 0.8f) => R.EndFireball(h, burst, burstRadius * HellFX.PPU);
        public static int FireballBurst(Vector2 pos, float radius = 0.8f, int level = 1) => R.Spawn(1, pos * HellFX.PPU, Vector2.right, radius * HellFX.PPU, 0, 0.3f, 0, level);
        /// <summary>Crescent sweep over arcDeg centred on dirDeg, radius in world units, 0.22 s.</summary>
        public static int FrostCleave(Vector2 pos, float dirDeg, float radius = 2.3f, float arcDeg = 130f, int level = 1, bool clockwise = true)
            => R.Spawn(2, pos * HellFX.PPU, new Vector2(dirDeg, clockwise ? 1 : -1), radius * HellFX.PPU, arcDeg, 0.22f, 0, level);
        /// <summary>Call every frame while the aura is active.</summary>
        public static void StormRing(Vector2 center, float radius = 2.6f, int level = 1) => R.Storm(center * HellFX.PPU, radius * HellFX.PPU, level);
        /// <summary>Damage tick: 6 white sparks run outward along the ring.</summary>
        public static int StormTick(Vector2 center, float radius = 2.6f) => R.Spawn(3, center * HellFX.PPU, Vector2.right, radius * HellFX.PPU, 0, 0.16f, 0, 1);
        /// <summary>Zigzag bolt from a to b, 0.12 s (jitters, never blinks).</summary>
        public static int StormArc(Vector2 a, Vector2 b, int level = 1) => R.Spawn(4, a * HellFX.PPU, b * HellFX.PPU, 0, 0, 0.12f, 0, level);
        /// <summary>Call every frame: count skulls at angleDeg (+ 360/count each), radius in world units, ySquash 0.75 matches Hellord.</summary>
        public static void BoneOrbit(Vector2 center, int count, float angleDeg, float radius = 2f, float ySquash = 0.75f, int level = 1, float degPerSec = 200f)
            => R.Orbit(center * HellFX.PPU, count, angleDeg, radius * HellFX.PPU, ySquash, level, degPerSec);
        public static int BoneHit(Vector2 pos) => R.Spawn(5, pos * HellFX.PPU, Vector2.right, 0, 0, 0.16f, 0, 1);
        /// <summary>Rune: draws in over armSec, inner ring closes over fuseSec, then bursts (OnRuneBurst). Returns handle or -1.</summary>
        public static int CurseRune(Vector2 pos, float radius = 1.5f, float armSec = 0.5f, float fuseSec = 2f, int level = 1)
            => R.Spawn(6, pos * HellFX.PPU, Vector2.right, radius * HellFX.PPU, armSec, armSec + fuseSec + 0.3f, fuseSec, level);
        /// <summary>Detonate a rune now.</summary>
        public static void CurseRuneDetonate(int h) => R.Detonate(h);
        /// <summary>Per frame over a cursed enemy's head: 7x7 violet diamond mark with a white core.</summary>
        public static void CurseMark(Vector2 headPos) => R.Mark(headPos * HellFX.PPU);
        public static void Kill(int h) => R.Kill(h);
        public static void Clear() => R.ClearAll();
    }

    public class SkillRunner : MonoBehaviour
    {
        static SkillRunner _i;
        public static SkillRunner I { get { if (_i == null) { var go = new GameObject("SkillFX"); DontDestroyOnLoad(go); _i = go.AddComponent<SkillRunner>(); } return _i; } }
        struct It { public bool used; public byte k; public int lv; public Vector2 p, d; public float r, a, life, fuse, t, idle, seed; public ushort gen; public bool burst; public Vector2 h0, h1, h2, h3, h4; }
        It[] _it; int[] _free; int _freeN;
        const int MaxQ = 8192;
        struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _q = new Q[MaxQ]; int _n, _last;
        Mesh _mesh; Vector3[] _vtx; Color32[] _col; public Vector2 _hero = new Vector2(-1e6f, -1e6f);
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255), Lt = new Color32(0xc0, 0xcb, 0xdc, 255),
            Y = new Color32(0xfe, 0xe7, 0x61, 255), Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Cb = new Color32(0x00, 0x99, 0xdb, 255), Bd = new Color32(0x12, 0x4e, 0x89, 255),
            Ds = new Color32(0xea, 0xd4, 0xaa, 255), Pu = new Color32(0x68, 0x38, 0x6c, 255), Pk = new Color32(0xb5, 0x50, 0x88, 255), G = new Color32(0xfe, 0xae, 0x34, 255), Gr = new Color32(0x63, 0xc7, 0x4d, 255), Gd = new Color32(0x3e, 0x89, 0x48, 255);

        public void Prewarm()
        {
            if (_mesh != null) return;
            int cap = Mathf.Clamp(SkillFX.MaxItems, 16, 4096); _it = new It[cap]; _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) _free[_freeN++] = i;
            _vtx = new Vector3[MaxQ * 4]; _col = new Color32[MaxQ * 4]; var uv = new Vector2[MaxQ * 4]; var tri = new int[MaxQ * 6];
            for (int i = 0; i < MaxQ; i++) { int v = i * 4, t = i * 6; tri[t] = v; tri[t + 1] = v + 1; tri[t + 2] = v + 2; tri[t + 3] = v; tri[t + 4] = v + 2; tri[t + 5] = v + 3;
                uv[v] = new Vector2(.25f, .25f); uv[v + 1] = new Vector2(.25f, .75f); uv[v + 2] = new Vector2(.75f, .75f); uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "SkillFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Skill White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = SkillFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "SkillFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] SkillFX: no sprite shader found; set SkillFX.MaterialOverride.");
            mr.sortingOrder = SkillFX.SortingOrder; mr.sortingLayerName = SkillFX.SortingLayer; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            _mesh.vertices = _vtx; _mesh.uv = uv; _mesh.colors32 = _col; _mesh.triangles = tri;
        }
        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && _it != null && i < _it.Length && _it[i].used && _it[i].gen == (ushort)(h >> 16); }
        public int Spawn(byte k, Vector2 p, Vector2 d, float r, float a, float life, float fuse, int lv)
        {
            if (_mesh == null) Prewarm();
            if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ref var it = ref _it[i]; ushort g = (ushort)(it.gen + 1);
            it = new It { used = true, k = k, p = p, d = d, r = r, a = a, life = life, fuse = fuse, lv = Mathf.Clamp(lv, 1, 5), gen = g, seed = (i * 0.618f + Time.frameCount * 0.31f) % 1f, h0 = p, h1 = p, h2 = p, h3 = p, h4 = p };
            return Hd(i);
        }
        public void Move(int h, Vector2 p, Vector2 d)
        {
            if (!Get(h, out int i)) return; ref var it = ref _it[i];
            if ((p - it.h0).sqrMagnitude > 9f) { it.h4 = it.h3; it.h3 = it.h2; it.h2 = it.h1; it.h1 = it.h0; it.h0 = it.p; }
            it.p = p; if (d.sqrMagnitude > 1e-6f) it.d = d; it.idle = 0;
        }
        public void EndFireball(int h, bool burst, float rpx) { if (!Get(h, out int i)) return; var it = _it[i]; Kill(h); if (burst) Spawn(1, it.p, Vector2.right, rpx, 0, 0.3f, 0, it.lv); }
        public void Detonate(int h) { if (Get(h, out int i) && _it[i].k == 6 && _it[i].t < _it[i].a + _it[i].fuse) _it[i].t = _it[i].a + _it[i].fuse; }
        public void Kill(int h) { if (!Get(h, out int i)) return; _it[i].used = false; _free[_freeN++] = i; }
        public void ClearAll() { if (_it == null) return; for (int i = 0; i < _it.Length; i++) if (_it[i].used) { _it[i].used = false; _free[_freeN++] = i; } _n = 0; }

        /* primitives (pixel units, snapped) */
        void A(float x, float y, float w, float h, Color32 c) { if (_n >= MaxQ || c.a == 0) return;
            if (SkillFX.ClearZonePx > 0) { float dx = x + w * 0.5f - _hero.x, dy = y + h * 0.5f - _hero.y, rc = SkillFX.ClearZonePx, d = Mathf.Sqrt(dx * dx + dy * dy); if (d < rc * 2f) c.a = (byte)(c.a * Mathf.Lerp(SkillFX.ClearZoneAlpha, 1f, Mathf.Clamp01((d - rc) / rc))); } _q[_n++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c }; }
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

        /* immediate (per-frame) */
        public void Storm(Vector2 c, float r, int lv)
        {
            if (_mesh == null) Prewarm(); lv = Mathf.Clamp(lv, 1, 5);
            int segs = lv >= 5 ? 5 : lv >= 3 ? 4 : 3; float rot = Time.time * 120f, len = 46f;
            if (lv >= 5) for (int s = 0; s < segs; s++) Arc(c, r - 6, -rot * 0.7f + s * 360f / segs, -rot * 0.7f + s * 360f / segs + 24f, 3, Fa(Bd, 0.8f));
            for (int s = 0; s < segs; s++)
            {
                float a0 = rot + s * 360f / segs;
                Arc(c, r, a0, a0 + len, 5, Fa(Bd, 0.9f)); Arc(c, r, a0 + 6, a0 + len, 3, Cb); Arc(c, r, a0 + len - 8, a0 + len, 3, Lt);
                float ah = (a0 + len) * Mathf.Deg2Rad; Dot(c.x + Mathf.Cos(ah) * r, c.y + Mathf.Sin(ah) * r, 5, Ko); Dot(c.x + Mathf.Cos(ah) * r, c.y + Mathf.Sin(ah) * r, 3, Lt);
            }
        }
        public void Orbit(Vector2 c, int count, float ang, float r, float ys, int lv, float dps)
        {
            if (_mesh == null) Prewarm(); lv = Mathf.Clamp(lv, 1, 5); count = Mathf.Clamp(count, 1, 12);
            float trail = lv >= 3 ? 80f : 60f; float dir = dps >= 0 ? 1 : -1;
            for (int s = 0; s < count; s++)
            {
                float a = ang + s * 360f / count;
                for (int j = 6; j >= 1; j--) { float ta = a - dir * trail * j / 6f, rr = ta * Mathf.Deg2Rad; Dot(c.x + Mathf.Cos(rr) * r, c.y + Mathf.Sin(rr) * r * ys, j < 3 ? 5 : 3, Fa(j < 3 ? Gr : Gd, 1f - j / 8f)); }
                float ra = a * Mathf.Deg2Rad; Skull(c.x + Mathf.Cos(ra) * r, c.y + Mathf.Sin(ra) * r * ys, lv);
            }
        }
        void Skull(float x, float y, int lv)
        {
            x = Mathf.Round(x); y = Mathf.Round(y);
            Disc(x, y, 15, Gr); Disc(x, y, 13, Ko); Disc(x, y, 11, Ds);
            A(x - 4, y - 2, 3, 3, Ko); A(x + 2, y - 2, 3, 3, Ko);
            if (lv >= 5) { float a = Time.time * 9f; Dot(x + Mathf.Cos(a) * 9, y + Mathf.Sin(a) * 9, 3, Gr); }
        }
        public void Mark(Vector2 p)
        {
            if (_mesh == null) Prewarm(); float x = Mathf.Round(p.x), y = Mathf.Round(p.y) + 2;
            A(x - 1, y - 4, 3, 9, Ko); A(x - 4, y - 1, 9, 3, Ko); A(x - 2, y - 2, 5, 5, Ko); A(x - 1, y - 3, 3, 7, Pk); A(x - 3, y - 1, 7, 3, Pk); A(x - 1, y - 1, 3, 3, Wh);
        }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, u = 1f / HellFX.PPU;
            for (int i = 0; i < _it.Length; i++)
            {
                if (!_it[i].used) continue; ref var it = ref _it[i]; it.t += dt;
                if (it.k == 0) { it.idle += dt; if (it.idle > 0.25f) { Kill(Hd(i)); continue; } DrawFireball(ref it); continue; }
                if (it.k == 6 && !it.burst && it.t >= it.a + it.fuse) { it.burst = true; SkillFX.OnRuneBurst?.Invoke(Hd(i), it.p * u); }
                if (it.t >= it.life) { Kill(Hd(i)); continue; }
                switch (it.k) { case 1: DrawBurst(ref it); break; case 2: DrawCleave(ref it); break; case 3: DrawTick(ref it); break; case 4: DrawBolt(ref it); break; case 5: DrawBoneHit(ref it); break; case 6: DrawRune(ref it); break; }
            }
            for (int b = 0; b < _n; b++)
            {
                int v = b * 4; ref var s = ref _q[b]; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
                _vtx[v] = new Vector3(x0, y0, 0); _vtx[v + 1] = new Vector3(x0, y1, 0); _vtx[v + 2] = new Vector3(x1, y1, 0); _vtx[v + 3] = new Vector3(x1, y0, 0);
                _col[v] = _col[v + 1] = _col[v + 2] = _col[v + 3] = s.c;
            }
            for (int z = _n; z < _last; z++) { int v = z * 4; _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; }
            _last = _n; _n = 0; _mesh.vertices = _vtx; _mesh.colors32 = _col; _mesh.RecalculateBounds();
        }
        void DrawFireball(ref It it)
        {   /* v2.1.1: warm gold/amber identity, 13 px head, 6-step amber comet trail; no white/silver */
            Vector2 p = it.p, d = it.d.sqrMagnitude > 1e-6f ? it.d.normalized : Vector2.right;
            for (int j = 6; j >= 1; j--) { Vector2 q = p - d * (4 + j * 5); int sz = j <= 2 ? 9 : j <= 4 ? 7 : 5; Dot(q.x, q.y, sz, Fa(Ko, 0.8f)); Dot(q.x, q.y, sz - 2, Fa(j <= 3 ? Y : G, 1f - j * 0.1f)); }
            if (it.lv >= 3) { Vector2 n = new Vector2(-d.y, d.x) * 7, q = p - d * 12; Dot(q.x + n.x, q.y + n.y, 3, G); Dot(q.x - n.x, q.y - n.y, 3, G); }
            Disc(p.x, p.y, 15, Ko); Disc(p.x, p.y, 13, G); Disc(p.x + d.x, p.y + d.y, 9, Y);
            if (it.lv >= 5) { float a = it.t * 12f; Dot(p.x + Mathf.Cos(a) * 10, p.y + Mathf.Sin(a) * 10, 3, Y); }
        }
        void DrawBurst(ref It it)
        {
            float k = it.t / it.life, r = Mathf.Lerp(4, it.r, 1 - (1 - k) * (1 - k)), al = 1 - k * k;
            if (it.t < 0.08f) { Disc(it.p.x, it.p.y, 11, Ko); Disc(it.p.x, it.p.y, 9, Y); }
            Arc(it.p, r, 0, 360, 5, Fa(Ko, al * 0.7f)); Arc(it.p, r, 0, 360, 3, Fa(k < 0.5f ? Y : G, al));
            if (it.lv >= 3) Arc(it.p, r * 0.6f, 0, 360, 3, Fa(Y, al * 0.7f));
            for (int j = 0; j < 6; j++) { float a = (j * 60 + it.seed * 60) * Mathf.Deg2Rad, rr = r + 4; Dot(it.p.x + Mathf.Cos(a) * rr, it.p.y + Mathf.Sin(a) * rr, 3, Fa(G, al)); }
        }
        void DrawCleave(ref It it)
        {
            float k = Mathf.Clamp01(it.t / 0.16f), fade = 1 - Mathf.Clamp01((it.t - 0.12f) / 0.1f), half = it.a * 0.5f, cw = it.d.y;
            float a0 = it.d.x + half * cw, a1 = a0 - cw * it.a * k;
            int th = it.lv >= 3 ? 7 : 5;
            float sw = a1 - a0; /* crescent: stacked strokes, inner ones shorter and centred on the sweep = tapered moon shape, never a ring segment */
            Arc(it.p, it.r, a0, a1, th + 2, Fa(Ko, fade * 0.8f)); Arc(it.p, it.r, a0, a1, th, Fa(Cy, fade));
            Arc(it.p, it.r - 4, a0 + sw * 0.15f, a1 - sw * 0.1f, th, Fa(Cy, fade)); Arc(it.p, it.r - 8, a0 + sw * 0.35f, a1 - sw * 0.2f, 3, Fa(Lt, fade));
            Arc(it.p, it.r - 2, a1 + cw * 18f, a1, th + 2, Fa(Wh, fade));
            if (it.lv >= 3) Arc(it.p, it.r - 12, a0 + sw * 0.5f, a1 - sw * 0.3f, 3, Fa(Cy, fade * 0.8f));
            int motes = 5;
            for (int j = 0; j < motes; j++) { float f = H(it.seed + j), aa = (a0 - cw * it.a * f * k) * Mathf.Deg2Rad, rr = it.r + 6 + 4 * H(f + 3); Dot(it.p.x + Mathf.Cos(aa) * rr, it.p.y + Mathf.Sin(aa) * rr + it.t * 30, 3, Fa(j % 2 == 0 ? Wh : Cy, fade)); }
        }
        void DrawTick(ref It it)
        {
            float k = it.t / it.life, al = 1 - k;
            for (int j = 0; j < 6; j++) { float a = (j * 60 + Time.time * 120f) * Mathf.Deg2Rad, rr = it.r - 4 + k * 10; Dot(it.p.x + Mathf.Cos(a) * rr, it.p.y + Mathf.Sin(a) * rr, 3, Fa(Lt, al * 0.8f)); }
        }
        void DrawBolt(ref It it)
        {
            Vector2 a = it.p, b = it.d, dd = b - a; float L = dd.magnitude; if (L < 1) return;
            Vector2 n = new Vector2(-dd.y, dd.x) / L; int seg = Mathf.Clamp(Mathf.CeilToInt(L / 9f), 2, 24); float sd = Mathf.Floor(Time.time * 30f) * 0.13f + it.seed;
            float al = 1 - Mathf.Clamp01((it.t - 0.06f) / 0.06f); Vector2 prev = a;
            for (int j = 1; j <= seg; j++)
            {
                Vector2 q = Vector2.Lerp(a, b, j / (float)seg); if (j < seg) q += n * ((H(sd + j) - 0.5f) * 8f);
                Line(prev, q, it.lv >= 3 ? 7 : 5, Fa(Cb, al)); prev = q;
            }
            prev = a;
            for (int j = 1; j <= seg; j++) { Vector2 q = Vector2.Lerp(a, b, j / (float)seg); if (j < seg) q += n * ((H(sd + j) - 0.5f) * 8f); Line(prev, q, 3, Fa(Wh, al)); prev = q; }
            Dot(b.x, b.y, 5, Fa(Wh, al)); if (it.lv >= 5) { Dot(b.x + 5, b.y + 5, 3, Fa(Lt, al)); Dot(b.x - 5, b.y - 4, 3, Fa(Lt, al)); }
        }
        void DrawBoneHit(ref It it)
        {
            float k = it.t / it.life, al = 1 - k; Dot(it.p.x, it.p.y, 3, Fa(Wh, al));
            for (int j = 0; j < 4; j++) { float a = (j * 90 + 45 + it.seed * 40) * Mathf.Deg2Rad, rr = 3 + k * 9; Dot(it.p.x + Mathf.Cos(a) * rr, it.p.y + Mathf.Sin(a) * rr, 3, Fa(j % 2 == 0 ? Gr : Ds, al)); }
        }
        void DrawRune(ref It it)
        {
            float arm = it.a, fz = it.fuse, t = it.t, r = it.r; Vector2 p = it.p;
            if (t < arm + fz)
            {
                float ak = arm > 0 ? Mathf.Clamp01(t / arm) : 1, rot = t * 40f;
                Arc(p, r, 90, 90 - 360 * ak, 5, Fa(Ko, 0.7f), 0.75f); Arc(p, r, 90, 90 - 360 * ak, 3, Pu, 0.75f);
                int gl = 6; for (int g = 0; g < gl; g++)
                {
                    if (g / (float)gl > ak) break; float a = (rot + g * 60) * Mathf.Deg2Rad; float x = p.x + Mathf.Cos(a) * r, y = p.y + Mathf.Sin(a) * r * 0.75f;
                    A(Mathf.Round(x) - 2, Mathf.Round(y) - 4, 5, 9, Ko); A(Mathf.Round(x) - 1, Mathf.Round(y) - 3, 3, 7, Pk); A(Mathf.Round(x) - 1, Mathf.Round(y) - 1, 3, 3, Wh);
                }
                if (it.lv >= 3) Arc(p, r * 0.55f, -rot * 1.5f, -rot * 1.5f + 240, 3, Fa(Pk, 0.8f), 0.75f);
                if (t >= arm) { float wk = ((t - arm) / 0.6f) % 1f, ir = Mathf.Lerp(4, r - 4, wk); for (int s3 = 0; s3 < 3; s3++) Arc(p, ir, rot * 2 + s3 * 120, rot * 2 + s3 * 120 + 50, 3, Fa(Pk, 0.8f * (1 - wk)), 0.75f); } /* v2.1.1: 3 short arcs RADIATE OUTWARD every 0.6 s, never close in */
                Dot(p.x, p.y, 3, Wh);
                if (it.lv >= 5) { float a = t * 6f; Dot(p.x + Mathf.Cos(a) * (r + 7), p.y + Mathf.Sin(a) * (r + 7) * 0.75f, 3, Pk); }
            }
            else
            {
                float k = Mathf.Clamp01((t - arm - fz) / 0.3f), al = 1 - k * k, br = Mathf.Lerp(r * 0.5f, r * 1.3f, 1 - (1 - k) * (1 - k));
                if (k < 0.2f) { Disc(p.x, p.y, 11, Ko); Disc(p.x, p.y, 9, Pk); }
                Arc(p, br, 0, 360, 5, Fa(Ko, al * 0.7f), 0.75f); Arc(p, br, 0, 360, 3, Fa(k < 0.4f ? Pk : Pu, al), 0.75f);
                for (int j = 0; j < 8; j++) { float a = (j * 45 + 22) * Mathf.Deg2Rad; Dot(p.x + Mathf.Cos(a) * (br + 5), p.y + Mathf.Sin(a) * (br + 5) * 0.75f, 3, Fa(Pk, al)); }
            }
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
