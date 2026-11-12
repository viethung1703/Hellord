using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// #137-141 HurtFX (v2.2.0): hero damage + death feedback for a 120-enemy horde. One pooled mesh, int handles (-1 when full), no per-frame alloc.
    /// 137 HeroHit      1-2 frame WHITE SILHOUETTE OUTLINE (PixelFX.outlineSize) + v2.2.1 punch: gold brackets that pop 1.35x for 2 frames, a 0.12 s gold/white-core ring
    ///                  (3-5 px stroke) expanding 8 -> 22 px (round, kept inside a typical tell border so it never reads as one), and 6 bright gold/white outlined sparks AWAY from the source.
    ///                  Replaces the whole-sprite red tint. Never a screen flash.
    /// 138 IFrames      silver #c0cbdc dashed ellipse at the feet that rotates and SHRINKS as i-frames run out (continuous, no blink).
    /// 139 LowHpPips    up to 3 heart pips 8x6 (#e43b44, dark outline) above the head; the last pip breathes at the CRT pulse rate (0.8 Hz, clamp 1.5).
    ///                  The screen-edge pulse stays the game's existing CRT pulse: feed it LowHpEdge(hp) + the one-shot OnEdgeBump, never a second vignette.
    /// 140 Death        cyan soul flame; beats: 0 impact (t 0) -> 1 shatter (0.12 s, 14 outlined shards) -> 2 soul rise (0.45 s) -> 3 done (1.6 s, show UI). OnDeathBeat fires each beat.
    /// 141 EdgeBump     callback only: one-shot (amount, seconds) to add onto the existing low-HP edge pulse on every hit.
    /// Recommended timing: hit -> SFX "hurt" at beat 0, HitStop.Play(HitStopSec 0.05); death -> HitStop.Play(0.15, 0.3, 0.5) at beat 0, SFX "shatter" beat 1, "soul" beat 2.
    /// All motion uses unscaled time so it plays through hit-stop. Nothing toggles faster than 3 Hz; nothing fills the screen; no FX sits on enemy tells (hero-local, small).
    /// </summary>
    public static class HurtFX
    {
        public static int SortingOrder = 61; public static string SortingLayer = "Default"; public static Material MaterialOverride; public static int MaxItems = 64;
        public static float HitStopSec = 0.05f, OutlineSec = 0.033f, EdgeBumpAmt = 0.35f, EdgeBumpSec = 0.25f, LowHpThreshold = 0.3f, PulseHz = 0.8f;
        /// <summary>Fired on every Hit: (heroPos, awayDir, damage). Play the hurt SFX and HitStop here.</summary>
        public static event Action<Vector2, Vector2, float> OnHit;
        /// <summary>One-shot edge bump for the EXISTING CRT low-HP pulse: (amount, seconds). Add it to _PulseAmt and decay linearly.</summary>
        public static event Action<float, float> OnEdgeBump;
        /// <summary>Death beats: (beat 0..3, pos). 0 impact, 1 shatter, 2 soul rise, 3 done.</summary>
        public static event Action<int, Vector2> OnDeathBeat;
        static HurtRunner R => HurtRunner.I;
        public static void Prewarm() => R.Prewarm();
        /// <summary>Hero took damage. heroFx (optional) gets a 1-2 frame white outline; null = corner-bracket fallback.</summary>
        public static int Hit(Vector2 heroPos, Vector2 sourcePos, float damage = 1f, PixelFX heroFx = null)
        {
            Vector2 d = heroPos - sourcePos; if (d.sqrMagnitude < 1e-6f) d = Vector2.up; d.Normalize();
            R.Outline(heroFx); OnHit?.Invoke(heroPos, d, damage); OnEdgeBump?.Invoke(EdgeBumpAmt, EdgeBumpSec);
            return R.Spawn(0, heroPos * HellFX.PPU, d, 0, 0, 0.22f, 0, 1);
        }
        /// <summary>Per frame while invulnerable: remaining / total i-frame seconds.</summary>
        public static void IFrames(Vector2 feetPos, float remaining, float total = 0.8f) => R.IFrames(feetPos * HellFX.PPU, total > 0 ? Mathf.Clamp01(remaining / total) : 0);
        /// <summary>Per frame: draws pips only when hpFrac &lt;= LowHpThreshold.</summary>
        public static void LowHpPips(Vector2 headPos, float hpFrac) { if (hpFrac <= LowHpThreshold && hpFrac > 0) R.Pips(headPos * HellFX.PPU, hpFrac / LowHpThreshold); }
        /// <summary>Suggested _PulseAmt for the existing CRT edge pulse (0 above threshold, ramps to 0.35 at 0 HP). Feed it; do not add a second vignette.</summary>
        public static float LowHpEdge(float hpFrac) => hpFrac > LowHpThreshold ? 0f : Mathf.Lerp(0.35f, 0.15f, hpFrac / LowHpThreshold);
        public static int Death(Vector2 heroPos, Vector2 sourcePos) { Vector2 d = heroPos - sourcePos; if (d.sqrMagnitude < 1e-6f) d = Vector2.up; return R.Spawn(1, heroPos * HellFX.PPU, d.normalized, 0, 0, 1.6f, 0, 1); }
        internal static void Beat(int b, Vector2 p) => OnDeathBeat?.Invoke(b, p);
        public static void Kill(int h) => R.Kill(h);
        public static void Clear() => R.ClearAll();
    }

    public class HurtRunner : MonoBehaviour
    {
        static HurtRunner _i;
        public static HurtRunner I { get { if (_i == null) { var go = new GameObject("HurtFX"); DontDestroyOnLoad(go); _i = go.AddComponent<HurtRunner>(); } return _i; } }
        struct It { public bool used; public byte k; public int lv; public Vector2 p, d; public float r, a, life, fuse, t, seed; public ushort gen; public int beat; }
        It[] _it; int[] _free; int _freeN;
        const int MaxQ = 2048;
        struct Q { public float x, y, w, h; public Color32 c; }
        readonly Q[] _q = new Q[MaxQ]; int _n, _last;
        Mesh _mesh; Vector3[] _vtx; Color32[] _col;
        PixelFX _ofx; Color _oCol; int _oSize; float _oT = -1f;
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Wh = new Color32(255, 255, 255, 255), Lt = new Color32(0xc0, 0xcb, 0xdc, 255),
            Rd = new Color32(0xe4, 0x3b, 0x44, 255), Dr = new Color32(0x9e, 0x28, 0x35, 255), Ds = new Color32(0xea, 0xd4, 0xaa, 255), Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Cb = new Color32(0x00, 0x99, 0xdb, 255), Pl = new Color32(0xf6, 0x75, 0x7a, 255), Y = new Color32(0xfe, 0xe7, 0x61, 255), Go = new Color32(0xfe, 0xae, 0x34, 255);

        public void Prewarm()
        {
            if (_mesh != null) return;
            int cap = Mathf.Clamp(HurtFX.MaxItems, 16, 4096); _it = new It[cap]; _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) _free[_freeN++] = i;
            _vtx = new Vector3[MaxQ * 4]; _col = new Color32[MaxQ * 4]; var uv = new Vector2[MaxQ * 4]; var tri = new int[MaxQ * 6];
            for (int i = 0; i < MaxQ; i++) { int v = i * 4, t = i * 6; tri[t] = v; tri[t + 1] = v + 1; tri[t + 2] = v + 2; tri[t + 3] = v; tri[t + 4] = v + 2; tri[t + 5] = v + 3;
                uv[v] = new Vector2(.25f, .25f); uv[v + 1] = new Vector2(.25f, .75f); uv[v + 2] = new Vector2(.75f, .75f); uv[v + 3] = new Vector2(.75f, .25f); }
            _mesh = new Mesh { name = "HurtFX" }; _mesh.MarkDynamic(); _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh; var mr = gameObject.AddComponent<MeshRenderer>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX Hurt White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = HurtFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "HurtFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) { m.mainTexture = tex; mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] HurtFX: no sprite shader found; set HurtFX.MaterialOverride.");
            mr.sortingOrder = HurtFX.SortingOrder; mr.sortingLayerName = HurtFX.SortingLayer; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
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

        public void Outline(PixelFX fx)
        {
            if (_mesh == null) Prewarm();
            if (fx == null) { _ofx = null; return; }
            if (_oT < 0 || _ofx != fx) { if (_ofx != null && _oT >= 0) { _ofx.outlineColor = _oCol; _ofx.outlineSize = _oSize; } _ofx = fx; _oCol = fx.outlineColor; _oSize = fx.outlineSize; }
            fx.outlineColor = Color.white; fx.outlineSize = Mathf.Max(1, _oSize); _oT = HurtFX.OutlineSec;
        }
        public void IFrames(Vector2 c, float k)
        {
            if (_mesh == null) Prewarm(); if (k <= 0) return;
            float rot = Time.unscaledTime * 180f, len = 40f * k + 8f;
            for (int s = 0; s < 4; s++) { float a0 = rot + s * 90f; Arc(c, 13, a0, a0 + len, 5, Fa(Ko, 0.7f), 0.5f); Arc(c, 13, a0, a0 + len, 3, Lt, 0.5f); }
        }
        public void Pips(Vector2 c, float k)
        {
            if (_mesh == null) Prewarm(); int n = Mathf.Clamp(Mathf.CeilToInt(k * 3f), 1, 3);
            float hz = Mathf.Min(HurtFX.PulseHz, 1.5f), br = 0.55f + 0.45f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * hz * 6.2832f));
            float x0 = Mathf.Round(c.x) - (n * 10 - 2) * 0.5f, y = Mathf.Round(c.y) + 10;
            for (int j = 0; j < n; j++) { float x = Mathf.Round(x0 + j * 10); A(x - 1, y - 1, 10, 8, Ko); A(x, y, 8, 6, Fa(Rd, j == n - 1 ? br : 1f)); A(x + 1, y + 3, 2, 2, Fa(Ds, 0.8f)); }
        }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.unscaledDeltaTime, u = 1f / HellFX.PPU;
            if (_oT >= 0) { _oT -= dt; if (_oT < 0 && _ofx != null) { _ofx.outlineColor = _oCol; _ofx.outlineSize = _oSize; _ofx = null; } }
            for (int i = 0; i < _it.Length; i++)
            {
                if (!_it[i].used) continue; ref var it = ref _it[i]; it.t += dt;
                if (it.k == 1) { while (it.beat < 4 && it.t >= Beats[it.beat]) { HurtFX.Beat(it.beat, it.p * u); it.beat++; } }
                if (it.t >= it.life) { Kill(Hd(i)); continue; }
                if (it.k == 0) DrawHit(ref it); else DrawDeath(ref it);
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
        static float H(float x) { x = Mathf.Sin(x * 127.1f + 311.7f) * 43758.5453f; return x - Mathf.Floor(x); }
        static readonly float[] Beats = { 0f, 0.12f, 0.45f, 1.6f };

        void Brackets(Vector2 c, float hw, float hh, Color32 col)
        {
            float x0 = Mathf.Round(c.x - hw), x1 = Mathf.Round(c.x + hw) - 3, y0 = Mathf.Round(c.y - hh), y1 = Mathf.Round(c.y + hh) - 3;
            A(x0, y0, 7, 3, col); A(x0, y0, 3, 7, col); A(x1 - 4, y0, 7, 3, col); A(x1, y0, 3, 7, col);
            A(x0, y1, 7, 3, col); A(x0, y1 - 4, 3, 7, col); A(x1 - 4, y1, 7, 3, col); A(x1, y1 - 4, 3, 7, col);
        }
        void DrawHit(ref It it)
        {   /* v2.2.1: punch = gold brackets that POP (1.35x for 2 frames, then 1x), a 0.12 s gold ring with white core expanding 8 -> 22 px (round, kept inside a typical tell border so it never reads as one),
               and 6 bright gold/white sparks (5 px stroke incl. dark outline, 8-13 px long) flying AWAY from the source. Gold, not ghost white. */
            float k = it.t / it.life, al = 1f - k; Vector2 d = it.d, n = new Vector2(-d.y, d.x);
            if (it.t < 0.1f) { float sc = it.t < 0.034f ? 1.35f : 1f; Brackets(it.p, 11 * sc + 2, 13 * sc + 2, Fa(Ko, 0.85f)); Brackets(it.p, 11 * sc, 13 * sc, it.t < 0.034f ? Wh : Y); }
            if (it.t < 0.12f) { float rk = it.t / 0.12f, rr = 8f + 14f * Mathf.Sqrt(rk), ra = 1f - rk * rk; Arc(it.p, rr, 0, 360, 7, Fa(Ko, ra * 0.7f)); Arc(it.p, rr, 0, 360, 5, Fa(Go, ra)); Arc(it.p, rr, 0, 360, 3, Fa(Wh, ra)); }
            for (int j = 0; j < 6; j++)
            {
                float sp = (j - 2.5f) / 2.5f * 0.6f, len = 8f + 5f * H(it.seed + j); Vector2 dir = (d + n * sp).normalized, a = it.p + dir * (10 + 30 * Mathf.Sqrt(k));
                Line(a, a - dir * len, 7, Fa(Ko, al * 0.85f)); Line(a, a - dir * len, 5, Fa(Go, al)); Line(a, a - dir * (len * 0.5f), 3, Fa(k < 0.5f ? Wh : Y, al));
            }
        }
        void DrawDeath(ref It it)
        {
            float t = it.t; Vector2 p = it.p;
            if (t < 0.12f) { Brackets(p, 12, 14, Wh); Arc(p, 6 + t * 120f, 0, 360, 3, Fa(Lt, 1 - t / 0.12f), 0.6f); }
            if (t >= 0.12f && t < 1.1f)
            {
                float k = (t - 0.12f) / 0.98f, e = 1 - (1 - k) * (1 - k), al = 1 - k * k;
                for (int j = 0; j < 14; j++)
                {
                    float a = (j / 14f * 360f + H(j + it.seed) * 20f) * Mathf.Deg2Rad, sp = 18f + 22f * H(j * 1.7f); Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) + it.d * 0.6f;
                    Vector2 q = p + dir * sp * e + Vector2.down * (60f * k * k); int s = j % 3 == 0 ? 9 : 7;
                    Dot(q.x, q.y, s, Fa(Ko, al)); Dot(q.x, q.y, s - 2, Fa(j % 2 == 0 ? Pl : Rd, al)); /* v2.2.0 self-fix: light #f6757a / red shards read on the red floor */
                }
            }
            if (t >= 0.45f)
            {
                float k = Mathf.Clamp01((t - 0.45f) / 1.15f), al = k < 0.7f ? 1f : 1 - (k - 0.7f) / 0.3f; Vector2 s = p + Vector2.up * (4 + 44f * k);
                for (int j = 4; j >= 1; j--) { Vector2 q = s + new Vector2(Mathf.Sin(t * 5f + j) * 3f, -j * 5); Dot(q.x, q.y, 5, Fa(Ko, al * 0.6f)); Dot(q.x, q.y, 3, Fa(Cy, al * (1 - j / 5f))); }
                Disc(s.x, s.y, 11, Fa(Ko, al)); Disc(s.x, s.y, 9, Fa(Cb, al)); Dot(s.x, s.y + 1, 5, Fa(Cy, al)); /* soul = cyan flame, never a bone skull (would read as a skeleton enemy) */
                Arc(p, 10 + 12 * k, 0, 360, 3, Fa(Dr, al * 0.8f), 0.5f);
            }
        }
    }
}
