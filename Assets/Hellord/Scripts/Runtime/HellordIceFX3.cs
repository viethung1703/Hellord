using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  t1410u: ICE VFX = CPU pixel-art sprites (replaces the t1409u PixelIce shader, which rendered garbage and is gone).
    //  Every effect is drawn into a small pooled Texture2D canvas (Endesga 32 colours, 16 PPU, Point filter, integer pixel positions) shown by a
    //  plain SpriteRenderer with the default sprite material: exactly what every other Hellord sprite uses, so it renders the same in URP 2D.
    //  DRAWS ONLY: no gameplay in here. Switch: HellordTuning.iceVfx1410 (default on).
    //  Ground effects sort under every actor (demons are never covered); the Hurricane front arcs sort just over the hero.
    // =====================================================================================================================
    public class IceFX3 : MonoBehaviour
    {
        public const float PPU = 16f, SQ = 0.75f;
        public const int OrdGround = -29490, OrdSpike = -29480, OrdBack = -29470;
        static readonly Color32 INK = new Color32(24, 20, 37, 255), NAVY = new Color32(18, 78, 137, 255), BLUE = new Color32(0, 153, 219, 255), CYAN = new Color32(44, 232, 245, 255), PALE = new Color32(192, 203, 220, 255), WHITE = new Color32(255, 255, 255, 255);
        static IceFX3 _i; static bool _tried, _ok;
        public static bool Ok
        {
            get
            {
                if (HellordTuning.I != null && !HellordTuning.I.iceVfx1410) return false;
                if (!_tried) { _tried = true; var go = new GameObject("IceFX3"); if (Application.isPlaying) DontDestroyOnLoad(go); _i = go.AddComponent<IceFX3>(); _ok = true; }
                return _ok && _i != null;
            }
        }

        // ------------------------------------------------------------------ canvas + pool
        sealed class Cv
        {
            public int w, h; public Color32[] px; public Texture2D tex; public Sprite sp;
            public Cv(int w, int h)
            {
                this.w = w; this.h = h; px = new Color32[w * h];
                tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "ice3" };
                sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
            }
            public void Clear() { System.Array.Clear(px, 0, px.Length); }
            public void P(int x, int y, Color32 c) { if ((uint)x < (uint)w && (uint)y < (uint)h) px[y * w + x] = c; }
            public void Apply() { tex.SetPixels32(px); tex.Apply(false); }
        }
        sealed class Q { public SpriteRenderer sr; public Cv cv; }
        readonly List<Q> _pool = new List<Q>(); int _used, _frame = -1; readonly Dictionary<int, Stack<Cv>> _free = new Dictionary<int, Stack<Cv>>();
        void BeginFrame() { if (_frame != Time.frameCount) { _frame = Time.frameCount; _used = 0; } }
        static float Snap(float u) => Mathf.Round(u * PPU) / PPU;
        Cv Acquire(int w, int h) { int k = w << 16 | h; if (_free.TryGetValue(k, out var st) && st.Count > 0) return st.Pop(); return new Cv(w, h); }
        void Release(Cv c) { int k = c.w << 16 | c.h; if (!_free.TryGetValue(k, out var st)) _free[k] = st = new Stack<Cv>(); st.Push(c); }
        /// <summary>A cleared canvas of (at least) w x h px, centred on world pos `ctr` (snapped to the pixel grid). Even sizes in steps of 8 so the centre sits on a pixel edge.</summary>
        Q Take(int w, int h, Vector2 ctr, int order)
        {
            BeginFrame(); w = (w + 7) & ~7; h = (h + 7) & ~7;
            if (_used >= _pool.Count) { var go = new GameObject("ice3"); go.transform.SetParent(transform, false); _pool.Add(new Q { sr = go.AddComponent<SpriteRenderer>() }); }
            var q = _pool[_used++];
            if (q.cv == null || q.cv.w != w || q.cv.h != h) { if (q.cv != null) Release(q.cv); q.cv = Acquire(w, h); q.sr.sprite = q.cv.sp; }
            q.cv.Clear(); q.sr.enabled = true; q.sr.transform.position = new Vector3(Snap(ctr.x), Snap(ctr.y), 0); q.sr.sortingOrder = order; return q;
        }
        void LateUpdate() { Tick(Time.deltaTime * 30f); }
        /// <summary>Advances every self-timed effect by `frames` (30 f/s), draws it, hides the quads not used this frame. The editor shot harness calls it directly.</summary>
        public static void EditorTick(float frames) { if (Ok) { _i._frame = -1; _i.Tick(frames); } }
        void Tick(float frames)
        {
            BeginFrame();
            for (int i = _inst.Count - 1; i >= 0; i--) { var n = _inst[i]; n.age += frames; if (n.age >= n.life) { _inst.RemoveAt(i); continue; } DrawInst(n); }
            Flush();
        }
        void Flush() { for (int k = 0; k < _pool.Count; k++) { var q = _pool[k]; if (k < _used) q.cv.Apply(); else if (q.sr.enabled) q.sr.enabled = false; } }
        public static void EditorFlush() { if (Ok) _i.Flush(); }
        public static void Clear() { if (_i == null) return; _i._inst.Clear(); _i._used = 0; foreach (var q in _i._pool) if (q.sr != null) q.sr.enabled = false; }

        static float H(int a, int b = 0, int c = 0) { uint x = (uint)(a * 374761393 + b * 668265263 + c * 1274126177); x = (x ^ (x >> 13)) * 1274126177u; x ^= x >> 16; return (x & 0xFFFFFF) / 16777216f; }
        static float Ease(float k) { k = Mathf.Clamp01(k); return 1f - (1f - k) * (1f - k); }

        // ------------------------------------------------------------------ pixel primitives
        /// <summary>Upright ice crystal: base centre (x,y), height h px, base width w (3 or 5). 1 px INK outline, left facet light, right facet dark.</summary>
        static void Spike(Cv cv, int x, int y, int h, int w)
        {
            if (h < 2) { cv.P(x, y, CYAN); return; }
            int hw = (w - 1) / 2;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int j = 0; j < h; j++)
                {
                    float k = 1f - (float)j / h; int half = Mathf.Max(0, Mathf.RoundToInt(hw * k)); int e = pass == 0 ? 1 : 0;
                    for (int dx = -half - e; dx <= half + e; dx++)
                    {
                        if (pass == 0) cv.P(x + dx, y + j, INK);
                        else
                        {
                            Color32 c = dx < 0 ? (j > h * 0.65f ? PALE : CYAN) : dx == 0 ? (j > h * 0.7f ? WHITE : PALE) : (dx == half && half > 0 ? NAVY : BLUE);
                            cv.P(x + dx, y + j, c);
                        }
                    }
                }
                if (pass == 0) { cv.P(x, y + h, INK); cv.P(x - 1, y - 1, INK); cv.P(x, y - 1, INK); cv.P(x + 1, y - 1, INK); }
            }
        }
        /// <summary>Flat frost plate on the ground (a squashed shard): w px wide, 2-3 rows.</summary>
        static void Plate(Cv cv, int x, int y, int w, bool lit)
        {
            int hw = w / 2;
            for (int dx = -hw; dx <= hw; dx++) { cv.P(x + dx, y, dx == hw || dx == -hw ? NAVY : (lit ? CYAN : BLUE)); if (Mathf.Abs(dx) < hw) cv.P(x + dx, y - 1, NAVY); }
            if (w >= 5) for (int dx = -hw + 1; dx < hw; dx++) cv.P(x + dx, y + 1, dx < 0 ? (lit ? PALE : CYAN) : BLUE);
            cv.P(x - hw + 1, y, lit ? WHITE : PALE);
        }
        /// <summary>Angled crystal chip from (x,y) along (dx,dy), len px, 2 px wide.</summary>
        static void Shard(Cv cv, float x, float y, float dx, float dy, int len, float fade)
        {
            for (int s = 0; s < len; s++)
            {
                if (fade < 1f && H(s, (int)(x * 7 + y)) > fade) continue;
                int px = Mathf.RoundToInt(x + dx * s), py = Mathf.RoundToInt(y + dy * s);
                Color32 c = s >= len - 2 ? PALE : s >= len / 2 ? CYAN : BLUE; cv.P(px, py, c); cv.P(px + (Mathf.Abs(dx) > Mathf.Abs(dy) ? 0 : 1), py + (Mathf.Abs(dx) > Mathf.Abs(dy) ? 1 : 0), s >= len - 1 ? CYAN : NAVY);
            }
        }

        // ------------------------------------------------------------------ self-timed effects
        public sealed class Piece { public float th, rho, hmax, tIn, mDel; public int w; }
        public sealed class Inst
        {
            public int kind;            // 0 frost field, 1 crest (spike wave), 2 band, 3 shard burst, 4 flash+crest (bomb blast)
            public Vector2 c; public float r;   // r in px
            public float age, life, seed, reach, bright, bare = 0.5f, front, hmax, meltAfter = 9999, meltDur = 10, amp = 1f; public int order = OrdGround; public Piece[] pcs;
        }
        readonly List<Inst> _inst = new List<Inst>(48);
        static Inst Add(Inst n) { if (_i._inst.Count < 48) { _i._inst.Add(n); return n; } return null; }
        static Piece[] MakePieces(int n, float rin, float gap, float hmax, float tfront, float seed, float rpx)
        {
            var l = new List<Piece>();
            for (int i = 0; i < n; i++)
            {
                if (H(i, (int)(seed * 31), 5) < gap) continue; float k = Mathf.Lerp(rin, 1f, Mathf.Sqrt(H(i, (int)(seed * 31), 1))); float th = H(i, (int)(seed * 31), 2) * Mathf.PI * 2f;
                l.Add(new Piece { th = th, rho = k * rpx, hmax = Mathf.Lerp(hmax * 0.45f, hmax, H(i, (int)(seed * 31), 3)), tIn = tfront * (1f - Mathf.Sqrt(Mathf.Max(0f, 1f - k))), mDel = H(i, (int)(seed * 31), 4) * 5f, w = H(i, (int)(seed * 31), 6) > 0.5f ? 3 : 5 });
            }
            l.Sort((a, b) => (Mathf.Sin(b.th) * b.rho).CompareTo(Mathf.Sin(a.th) * a.rho)); // far (high y) first
            return l.ToArray();
        }
        /// <summary>Light frost field: sparse flat plates inside `reach`, they vanish whole (melt). kind 0. Returns the instance (bomb field: caller edits reach / bright).</summary>
        public static Inst Field(Vector2 c, float rPx, float lifeFrames, float meltAfter, float meltDur, float seed, float bare) { if (!Ok) return null; return Add(new Inst { kind = 0, c = c, r = rPx, reach = rPx, life = lifeFrames, meltAfter = meltAfter, meltDur = meltDur, seed = seed, bare = bare }); }
        /// <summary>Ice Nova / blast: a wave of small ice spikes that rise as the front passes, then melt. `count` spikes, heights up to hmax px.</summary>
        public static void Crest(Vector2 c, float rPx, int count, float hmax, float front, float rin, float gap, float amp, float seed, float life, float meltAfter, float meltDur)
        { if (!Ok) return; Add(new Inst { kind = 1, c = c, r = rPx, front = front, hmax = hmax, amp = amp, seed = seed, life = life, meltAfter = meltAfter, meltDur = meltDur, order = OrdSpike, pcs = MakePieces(count, rin, gap, hmax, front, seed, rPx) }); }
        /// <summary>Bomb pulse: a thin travelling band of dots + a few tiny spikes, reaching the rim in 9 frames.</summary>
        public static void Band(Vector2 c, float rPx, float seed) { if (!Ok) return; Add(new Inst { kind = 2, c = c, r = rPx, front = 9, life = 11, seed = seed, order = OrdSpike, pcs = MakePieces(14, 0.3f, 0.2f, 5f, 9f, seed, rPx) }); }
        /// <summary>t1414u Ice Nova: upright crystals riding the expanding front (far half behind actors, near half drawn at `order`, which must beat every actor in the ring), a thin base line, ~10 arcing shards with shadows that land as small spikes, small cast flash.</summary>
        public static void Shock(Vector2 c, float rPx, float front, int order, float seed) { if (!Ok) return; Add(new Inst { kind = 5, c = c, r = rPx, front = Mathf.Max(3f, front), life = Mathf.Max(3f, front) + 36f, seed = seed, order = order }); }

        /// <summary>Frostbolt burst: ring of crystal shards flying out then fading, + a brief light frost film.</summary>
        public static void ShardBurst(Vector2 c, float rPx, float seed) { if (!Ok) return; Add(new Inst { kind = 3, c = c, r = rPx, life = 14, seed = seed, order = OrdSpike }); }
        /// <summary>Frost Bomb blast: bright dithered flash + ring of spikes + flying crystal chunks. Reads clearly, ~0.6 s.</summary>
        public static void BlastBurst(Vector2 c, float rPx, float seed)
        {
            if (!Ok) return; float hm = Mathf.Clamp(rPx * 0.2f, 8f, 14f);
            Add(new Inst { kind = 4, c = c, r = rPx, front = 8, life = 22, seed = seed, hmax = hm, meltAfter = 11, meltDur = 9, order = OrdSpike, pcs = MakePieces(Mathf.Clamp((int)(rPx / 2.2f), 14, 30), 0.25f, 0.12f, hm, 8f, seed, rPx) });
        }

        void DrawInst(Inst n)
        {
            int R = Mathf.CeilToInt(n.r); int pad = 28, w = 2 * R + 2 * pad, h = (int)(2 * R * SQ) + 2 * pad + 16;
            var q = Take(w, h, n.c, n.order); var cv = q.cv; int cx = cv.w / 2, cy = cv.h / 2; float a = n.age;
            if (n.kind == 0 || n.kind == 4)
            {
                float reach = n.kind == 4 ? 0f : n.reach; float melt = Mathf.Clamp01((a - n.meltAfter) / Mathf.Max(1f, n.meltDur)); int seed = (int)(n.seed * 17);
                if (reach > 1f) DrawFilm(cv, cx, cy, reach, melt, seed, n.bare, n.bright > 0f);
            }
            if (n.kind == 4)
            {
                // flash: dithered disc + bright rim, 4 frames
                if (a < 4.5f)
                {
                    float rr = n.r * Mathf.Lerp(0.55f, 1.0f, Ease(a / 4f)); float k = 1f - a / 4.5f;
                    for (int y = -(int)(rr * SQ) - 1; y <= (int)(rr * SQ) + 1; y++) for (int x = -(int)rr - 1; x <= (int)rr + 1; x++)
                    {
                        float d = Mathf.Sqrt(x * x + (y / SQ) * (y / SQ)); if (d > rr) continue;
                        bool rim = d > rr - 3f; bool dith = ((x + y) & 1) == 0;
                        if (rim) cv.P(cx + x, cy + y, a < 2f ? PALE : CYAN); else if (dith && H(x, y, (int)a) < 0.55f * k + 0.1f) cv.P(cx + x, cy + y, d < rr * 0.4f ? PALE : CYAN);
                    }
                }
                // flying chunks
                for (int i = 0; i < 12; i++)
                {
                    float th = (i + H(i, 3) * 0.7f) * Mathf.PI * 2f / 12f, k = Ease(a / 12f), dist = n.r * (0.35f + 0.6f * H(i, 7)) * k, z = Mathf.Sin(Mathf.Clamp01(a / 14f) * Mathf.PI) * (6f + 8f * H(i, 9));
                    float fade = a > 11f ? Mathf.Clamp01(1f - (a - 11f) / 9f) : 1f; if (fade <= 0f) continue;
                    float x = cx + Mathf.Cos(th) * dist, y = cy + Mathf.Sin(th) * dist * SQ + z; Shard(cv, x, y, Mathf.Cos(th), Mathf.Sin(th) * 0.7f, 5, fade);
                }
            }
            if (n.kind == 1 || n.kind == 4)
            {
                float meltA = n.meltAfter; foreach (var p in n.pcs)
                {
                    if (a < p.tIn) continue; float g = Mathf.Pow(Mathf.Clamp01((a - p.tIn) / 3f), 0.7f); float m = Mathf.Clamp01((a - meltA - p.mDel) / Mathf.Max(1f, n.meltDur)); int hh = Mathf.RoundToInt(p.hmax * n.amp * g * (1f - m));
                    if (hh < 2) continue; Spike(cv, cx + Mathf.RoundToInt(Mathf.Cos(p.th) * p.rho), cy + Mathf.RoundToInt(Mathf.Sin(p.th) * p.rho * SQ), hh, hh < 5 ? 3 : p.w);
                }
                if (n.kind == 1 && a < n.front + 3f) DrawFront(cv, cx, cy, n.r * Ease(a / n.front), 1f - a / (n.front + 3f), (int)(a), (int)(n.seed * 5));
            }
            if (n.kind == 5) DrawShock(cv, cx, cy, n);
            if (n.kind == 2)
            {
                float rr = 4f + (n.r - 4f) * Mathf.Pow(Mathf.Clamp01(a / n.front), 0.8f); DrawFront(cv, cx, cy, rr, 1f - a / n.life, (int)a, (int)(n.seed * 5));
                foreach (var p in n.pcs)
                {
                    if (Mathf.Abs(p.rho - rr) > 6f && !(a > n.front * 0.6f && p.rho < rr)) continue; float m = Mathf.Clamp01((a - 6f - p.mDel) / 4f); int hh = Mathf.RoundToInt(p.hmax * (1f - m) * (1f - Mathf.Abs(p.rho - rr) / 8f)); if (hh < 2) continue;
                    Spike(cv, cx + Mathf.RoundToInt(Mathf.Cos(p.th) * p.rho), cy + Mathf.RoundToInt(Mathf.Sin(p.th) * p.rho * SQ), hh, 3);
                }
            }
            if (n.kind == 3)
            {
                float k = Ease(a / 8f); float fade = a > 7f ? Mathf.Clamp01(1f - (a - 7f) / 7f) : 1f;
                DrawFilm(cv, cx, cy, n.r * 0.7f * Ease(a / 4f), Mathf.Clamp01((a - 4f) / 8f), (int)(n.seed * 13), 0.5f, false);
                for (int i = 0; i < 8; i++)
                {
                    float th = (i + H(i, (int)n.seed) * 0.4f) * Mathf.PI * 2f / 8f, dist = n.r * (0.45f + 0.5f * H(i, 4)) * k, z = Mathf.Sin(Mathf.Clamp01(a / 12f) * Mathf.PI) * 5f;
                    Shard(cv, cx + Mathf.Cos(th) * dist, cy + Mathf.Sin(th) * dist * SQ + z, Mathf.Cos(th), Mathf.Sin(th) * 0.7f, 4, fade);
                }
                if (a < 3f) for (int y = -2; y <= 2; y++) for (int x = -2; x <= 2; x++) if (x * x + y * y <= 5) cv.P(cx + x, cy + y + 3, a < 1.5f ? WHITE : PALE);
            }
        }
        static void DrawShock(Cv cvF, int cx, int cy, Inst n)
        {
            // t1414u: Ice Nova = a wave of UPRIGHT ice crystals riding the expanding front (far half behind actors, near half in front) + a thin 1 px base line on the front + ~10 shards that fly out on a z arc with ground shadows and land as small spikes. No flat ring, no echo, no floor streaks.
            var cvB = _i.Take(cvF.w, cvF.h, n.c, OrdSpike).cv; float a = n.age, R = n.r; int sd = (int)(n.seed * 7);
            // small cast flash (front canvas), 5 frames
            if (a < 5f)
            {
                int fr = Mathf.Min(12, Mathf.RoundToInt(R * 0.22f)); float f = 1f - a / 5f; int rad = Mathf.RoundToInt(fr * Mathf.Lerp(0.5f, 1f, a / 5f));
                for (int y = -(int)(rad * SQ); y <= (int)(rad * SQ); y++) for (int x = -rad; x <= rad; x++)
                {
                    float d = Mathf.Sqrt(x * x + (y / SQ) * (y / SQ)); if (d > rad) continue; bool dith = ((x + y) & 1) == 0; if (d > rad - 1.5f) cvF.P(cx + x, cy + y, a < 2f ? WHITE : PALE); else if (dith && H(x, y, (int)a) < 0.7f * f + 0.15f) cvF.P(cx + x, cy + y, d < rad * 0.45f ? PALE : CYAN);
                }
            }
            // thin 1 px base line on the front radius (dashed), only while the front travels
            float k = Ease(a / n.front); if (a < n.front + 3f)
            {
                float rr = R * k, vis = a <= n.front ? 1f : 1f - (a - n.front) / 3f; int steps = Mathf.Max(40, (int)(rr * 6.2832f * 1.2f));
                for (int i = 0; i < steps; i++)
                {
                    if (H(i, sd, (int)(a / 2f)) > 0.8f * vis) continue; float th = i * 6.2832f / steps, sth = Mathf.Sin(th); var c = sth > 0f ? cvB : cvF;
                    c.P(cx + Mathf.RoundToInt(Mathf.Cos(th) * rr), cy + Mathf.RoundToInt(sth * rr * SQ), H(i, 3, sd) < 0.5f ? CYAN : BLUE);
                }
            }
            // upright crystals: each rises when the front reaches its radius, holds ~3 frames, drops ~3 frames later
            int count = Mathf.Clamp(Mathf.RoundToInt(R * 0.7f), 40, 60);
            for (int i = 0; i < count; i++)
            {
                float th = (i + (H(i, sd, 1) - 0.5f) * 0.8f) * 6.2832f / count, rho = R * (0.55f + 0.45f * Mathf.Pow(H(i, sd, 2), 0.55f)); float tin = n.front * (1f - Mathf.Sqrt(Mathf.Max(0f, 1f - rho / R)));
                float u = a - tin; if (u < 0f || u > 8f) continue; float hk = u < 2f ? u / 2f : u < 5f ? 1f : 1f - (u - 5f) / 3f; int hh = Mathf.RoundToInt(Mathf.Lerp(12f, 18f, H(i, sd, 3)) * Mathf.Clamp01(hk) * (0.85f + 0.15f * Mathf.Sin(u)));
                if (hh < 2) continue; float sth = Mathf.Sin(th); var c = sth > 0f ? cvB : cvF; Spike(c, cx + Mathf.RoundToInt(Mathf.Cos(th) * rho), cy + Mathf.RoundToInt(sth * rho * SQ), hh, hh < 6 ? 3 : (H(i, sd, 4) > 0.5f ? 3 : 5));
            }
            // light B: ~10 shards on a z arc with a 1 px shadow, landing as small stuck spikes that melt
            const float flight = 12f;
            for (int i = 0; i < 10; i++)
            {
                float th = (i + H(i, sd, 7) * 0.7f) * 6.2832f / 10f, L = R * (0.6f + 0.4f * H(i, sd, 8)), zmax = Mathf.Lerp(8f, 16f, H(i, sd, 9)); float u = a / flight; float sth = Mathf.Sin(th), cth = Mathf.Cos(th); var c = sth > 0f ? cvB : cvF;
                if (u < 1f)
                {
                    float rho = L * Ease(u); int gx = cx + Mathf.RoundToInt(cth * rho), gy = cy + Mathf.RoundToInt(sth * rho * SQ); int z = Mathf.RoundToInt(zmax * 4f * u * (1f - u));
                    c.P(gx - 1, gy, INK); c.P(gx, gy, INK); c.P(gx + 1, gy, INK);                      // shadow (dark ellipse, 1 px)
                    int ty = gy + z; Vector2 dir = new Vector2(cth * 2f, sth * SQ * 2f + (1f - 2f * u) * zmax * 0.5f).normalized; int lx = Mathf.RoundToInt(-dir.x * 2f), ly = Mathf.RoundToInt(-dir.y * 2f);
                    c.P(gx + 2 * lx, ty + 2 * ly, NAVY); c.P(gx + lx, ty + ly, BLUE); c.P(gx, ty, CYAN); c.P(gx + 1, ty, PALE); c.P(gx, ty + 1, WHITE);   // crystal chip with a short tail
                }
                else
                {
                    float m = Mathf.Clamp01((a - flight - 12f) / 8f); int hh = Mathf.RoundToInt(Mathf.Lerp(5f, 7f, H(i, sd, 10)) * (1f - m)); if (hh < 2) continue;
                    Spike(c, cx + Mathf.RoundToInt(cth * L), cy + Mathf.RoundToInt(sth * L * SQ), hh, 3);
                }
            }
        }
        /// <summary>Sparse flat frost plates (jittered 10 px grid) inside an ellipse of radius `reach` px. A plate melts whole when `melt` passes its own threshold.</summary>
        static void DrawFilm(Cv cv, int cx, int cy, float reach, float melt, int seed, float bare, bool lit)
        {
            int cell = 9, n = Mathf.CeilToInt(reach / cell) + 1;
            for (int gy = -n; gy <= n; gy++) for (int gx = -n; gx <= n; gx++)
            {
                float jx = (H(gx, gy, seed) - 0.5f) * 6f, jy = (H(gx, gy, seed + 1) - 0.5f) * 5f; float x = gx * cell + jx, y = gy * cell * SQ + jy;
                float d = Mathf.Sqrt(x * x + (y / SQ) * (y / SQ)); if (d > reach) continue; if (H(gx, gy, seed + 2) > bare) continue; if (H(gx, gy, seed + 3) < melt) continue;
                Plate(cv, cx + Mathf.RoundToInt(x), cy + Mathf.RoundToInt(y), 3 + 2 * (int)(H(gx, gy, seed + 4) * 3f), lit && H(gx, gy, seed + 5) < 0.5f);
            }
        }
        /// <summary>Thin dotted wave line at radius rr (ellipse), 1 px, random gaps; thick head.</summary>
        static void DrawFront(Cv cv, int cx, int cy, float rr, float vis, int frame, int seed)
        {
            if (rr < 3f || vis <= 0f) return; int steps = Mathf.Max(12, (int)(rr * 2.4f));
            for (int i = 0; i < steps; i++)
            {
                if (H(i, seed, frame / 2) > 0.35f + 0.5f * vis) continue; float th = i * Mathf.PI * 2f / steps;
                int x = cx + Mathf.RoundToInt(Mathf.Cos(th) * rr), y = cy + Mathf.RoundToInt(Mathf.Sin(th) * rr * SQ); cv.P(x, y, H(i, 1, seed) < 0.35f ? PALE : CYAN); if (H(i, 2, seed) < 0.3f) cv.P(x, y - 1, BLUE);
            }
        }

        // ------------------------------------------------------------------ immediate-mode (call every frame)
        /// <summary>Frostbolt orb: crystal-cored cold bolt with a comet tail of shrinking ice blobs, 12 fps sparkle. p = world pos (centre), v = velocity.</summary>
        public static void Orb(Vector2 p, Vector2 v, float age, int order)
        {
            if (!Ok) return; var q = _i.Take(40, 40, p, order); var cv = q.cv; int cx = 20, cy = 20; Vector2 d = v.sqrMagnitude > 0.0001f ? v.normalized : Vector2.right; Vector2 pr = new Vector2(-d.y, d.x);
            int fr = (int)(age * 12f); float wob = 0.6f;
            for (int k = 7; k >= 1; k--)
            {
                float s = k * 2.3f, off = (H(k, fr) - 0.5f) * 2f * wob * (k / 7f); float x = cx - d.x * s + pr.x * off, y = cy - d.y * s + pr.y * off; float rad = k <= 2 ? 2.3f : k <= 4 ? 1.7f : k <= 6 ? 1.1f : 0.6f; Color32 c = k <= 2 ? CYAN : k <= 5 ? BLUE : NAVY;
                int ir = Mathf.CeilToInt(rad + 0.3f);
                for (int yy = -ir; yy <= ir; yy++) for (int xx = -ir; xx <= ir; xx++) if (xx * xx + yy * yy <= rad * rad + 0.3f) cv.P(Mathf.RoundToInt(x) + xx, Mathf.RoundToInt(y) + yy, k <= 2 && xx * xx + yy * yy <= 1 ? PALE : c);
            }
            for (int y = -6; y <= 6; y++) for (int x = -6; x <= 6; x++)
            {
                float r = Mathf.Sqrt(x * x + y * y); if (r > 5f) continue; Color32 c = r > 4.2f ? INK : r > 3f ? BLUE : r > 2f ? CYAN : r > 1f ? PALE : WHITE; cv.P(cx + x, cy + y, c);
            }
            for (int i = 0; i < 3; i++)
            {
                float an = (i * 2.094f) + fr * 0.55f + H(i, 3) * 0.3f; float ca = Mathf.Cos(an), sa = Mathf.Sin(an);
                for (int s = 5; s <= 8; s++) cv.P(cx + Mathf.RoundToInt(ca * s), cy + Mathf.RoundToInt(sa * s), s >= 8 ? PALE : CYAN);
            }
            if ((fr & 1) == 0) { cv.P(cx - 1, cy + 1, WHITE); cv.P(cx, cy, WHITE); }
            cv.P(cx + Mathf.RoundToInt(-d.x * 11 + pr.x * 2 * (H(fr, 7) - 0.5f)), cy + Mathf.RoundToInt(-d.y * 11 + pr.y * 2 * (H(fr, 8) - 0.5f)), PALE);
        }
        /// <summary>Chill ground patch: a few flat frost plates + 2 tiny spikes inside rPx; they melt whole over the last 40 % of the life. life01 = remaining 1..0.</summary>
        public static void Patch(Vector2 p, float rPx, float life01, int seed)
        {
            if (!Ok) return; int R = Mathf.CeilToInt(rPx); var q = _i.Take(2 * R + 16, (int)(2 * R * SQ) + 24, p, OrdGround); float melt = Mathf.Clamp01(1f - life01 / 0.4f);
            DrawFilm(q.cv, q.cv.w / 2, q.cv.h / 2, rPx * 0.9f, melt, seed, 0.55f, false);
            for (int i = 0; i < 2; i++) { if (H(i, seed, 9) < melt) continue; float th = H(i, seed, 10) * 6.28f, rr = rPx * 0.55f * H(i, seed, 11); Spike(q.cv, q.cv.w / 2 + Mathf.RoundToInt(Mathf.Cos(th) * rr), q.cv.h / 2 + Mathf.RoundToInt(Mathf.Sin(th) * rr * SQ), 4, 3); }
        }
        /// <summary>Hurricane: a thin wind vortex around the hero. Pale wind streak arcs spiral round and up, dotted rim arcs, and blown snow / frost flecks spiral outward and rise. Nothing is filled, so demons stay visible.
        /// Back half sorts under the actors, front half just over the hero. age in s, fade 0..1 (last 0.5 s).</summary>
        public static void Vortex(Vector2 c, float rPx, float age, float fade, int orderFront)
        {
            if (!Ok || fade <= 0.01f) return; const int Hh = 40; int R = Mathf.CeilToInt(rPx); int w = 2 * R + 32, hgt = (int)(2 * R * SQ) + Hh + 24;
            float grow = Mathf.Clamp01(age / 0.35f); float vis = fade * grow; float rad = rPx * (0.55f + 0.45f * grow);
            Vector2 ctr = c + Vector2.up * (Hh * 0.5f / PPU); var qb = _i.Take(w, hgt, ctr, OrdBack); var qf = _i.Take(w, hgt, ctr, orderFront);
            int cx = qb.cv.w / 2, gy = qb.cv.h / 2 - Hh / 2; float t = Mathf.Floor(age * 15f) / 15f;   // 15 fps stepped animation
            // dotted rim arcs (3 heights, wider at the top = funnel)
            for (int ring = 0; ring < 3; ring++)
            {
                float z = 3f + ring * 13f, rho = rad * (0.62f + 0.19f * ring); int steps = Mathf.Max(20, (int)(rho * 2f));
                for (int i = 0; i < steps; i++)
                {
                    float th = i * 6.2832f / steps + t * (1.6f - 0.3f * ring); if (H(i, ring, (int)(t * 5f)) > 0.55f * vis) continue;
                    int x = cx + Mathf.RoundToInt(Mathf.Cos(th) * rho), y = gy + Mathf.RoundToInt(Mathf.Sin(th) * rho * SQ + z); var cvv = Mathf.Sin(th) > 0f ? qb.cv : qf.cv; cvv.P(x, y, Mathf.Sin(th) > 0f ? BLUE : CYAN); if (H(i, 9, ring) < 0.5f) { cvv.P(x + 1, y, Mathf.Sin(th) > 0f ? NAVY : PALE); cvv.P(x, y + 1, Mathf.Sin(th) > 0f ? NAVY : CYAN); }
                }
            }
            // wind streaks: tapered arcs, head bright, tail dithered
            for (int s = 0; s < 12; s++)
            {
                if (H(s, 77) > vis + 0.15f) continue; float z0 = 3f + 32f * H(s, 2), rho = rad * (0.42f + 0.38f * z0 / 35f + 0.2f * H(s, 1)), om = 4.2f + 2.2f * H(s, 3), L = 1.1f + 0.7f * H(s, 4), th0 = H(s, 5) * 6.2832f + t * om;
                int n = Mathf.Max(8, (int)(rho * L / 1.4f));
                for (int i = 0; i < n; i++)
                {
                    float u = (float)i / n, th = th0 - u * L; if (u > 0.55f && (i & 1) == 1) continue; if (u > 0.8f && (i & 3) != 0) continue;
                    float z = z0 + 6f * u; int x = cx + Mathf.RoundToInt(Mathf.Cos(th) * rho), y = gy + Mathf.RoundToInt(Mathf.Sin(th) * rho * SQ + z); Color32 col = u < 0.12f ? WHITE : u < 0.35f ? PALE : u < 0.65f ? CYAN : BLUE; var cvv = Mathf.Sin(th) > 0f ? qb.cv : qf.cv;
                    cvv.P(x, y, col); cvv.P(x, y + 1, u < 0.12f ? PALE : u < 0.5f ? CYAN : BLUE); if (u < 0.45f) cvv.P(x, y - 1, u < 0.12f ? CYAN : NAVY); if (u < 0.12f) cvv.P(x + 1, y, WHITE);
                }
            }
            // blown snow / frost flecks spiralling outward and up
            for (int s = 0; s < 44; s++)
            {
                if (H(s, 91) > vis) continue; float ph = Mathf.Repeat(t * (0.5f + 0.25f * H(s, 1)) + H(s, 2), 1f), rho = rad * (0.2f + 0.8f * ph), th = H(s, 3) * 6.2832f + t * (3.5f + 3f * (1f - ph)), z = 2f + 34f * Mathf.Pow(ph, 0.8f);
                if (ph > 0.85f && (s & 1) == 1) continue; int x = cx + Mathf.RoundToInt(Mathf.Cos(th) * rho), y = gy + Mathf.RoundToInt(Mathf.Sin(th) * rho * SQ + z); var cvv = Mathf.Sin(th) > 0f ? qb.cv : qf.cv; float hh = H(s, 4);
                Color32 col = hh < 0.5f ? PALE : hh < 0.85f ? CYAN : WHITE; cvv.P(x, y, col); if (H(s, 5) < 0.5f) { cvv.P(x + 1, y, col); cvv.P(x, y + 1, col); cvv.P(x + 1, y + 1, hh < 0.5f ? CYAN : PALE); }
            }
        }
        /// <summary>Chill (level 1) / freeze (level 3) chips on a demon: tiny ice crystals at its feet and shoulders, over its own sprite. Draws a few pixels only.</summary>
        public static void Chips(Vector2 feet, float halfPx, int level, int seed, float timeS, int order)
        {
            if (!Ok) return; var q = _i.Take(48, 48, feet + Vector2.up * 1.0f, order); var cv = q.cv; int cx = 24, by = 24 - 16; int hw = Mathf.RoundToInt(Mathf.Clamp(halfPx, 6f, 14f)); bool fr = level >= 3;
            Spike(cv, cx - hw + 1, by + 1, fr ? 6 : 4, 3); Spike(cv, cx + hw - 1, by + 2, fr ? 5 : 3, 3);
            if (fr) { Spike(cv, cx + 1, by, 5, 3); Spike(cv, cx - hw - 1, by + 16 + (seed & 3), 4, 3); Spike(cv, cx + hw + 1, by + 14 + ((seed >> 2) & 3), 4, 3); }
            int tw = (int)(timeS * 6f) + seed; if (H(tw, seed) < 0.5f) cv.P(cx + (int)((H(tw, 1) - 0.5f) * hw * 2f), by + 6 + (int)(H(tw, 2) * 18f), fr ? WHITE : PALE);
        }
    }
}
