using System;
using UnityEngine;

// Pixel FX Pack v1.6.0 "Frost & Reactions" - frost cleave, shock status, overload, bleed status, frost shatter, elite chest open.
// Sized for 240x135 art with a ~15px hero. Same design as ChainFX: one pooled runner with fixed instance slots and
// preallocated SpriteRenderer blocks, every sprite in ONE atlas texture, two shared materials (additive / default sprite).
// No allocations per spawn after warm-up (call ReactionFXRunner.I.Prewarm()). Statuses never touch the target's material,
// so HordeFX batching stays intact. UNTESTED IN EDITOR - compiles against Unity 6000.0.85f1 DLLs; see README "Known limits".
namespace PixelFXPack
{
    [AddComponentMenu("")]
    public class ReactionFXRunner : MonoBehaviour
    {
        static ReactionFXRunner _i;
        public static ReactionFXRunner I { get { if (_i == null) { var go = new GameObject("ReactionFX"); _i = go.AddComponent<ReactionFXRunner>(); _i.Init(); } return _i; } }
        /// <summary>Instance slots (all six effects share them) and sprites per instance.</summary>
        public const int MaxInst = 48, Slots = 48, MaxHits = 16;
        /// <summary>Shared cap for looping statuses (Shock + Bleed, and from v1.7 Burn + Chill). Keeps slots free for bursts. Clamped to MaxInst.</summary>
        public static int StatusCap = 32;
        internal enum Kind { None, Cleave, Shock, Bleed, Overload, Shatter, Chest }
        internal class Inst
        {
            public Kind kind; public int gen, order, seed, n, hits, n2; public float t, life, a0, a1, rad, h2, tickAcc;
            public Vector2 pos; public Transform follow; public bool hasFollow, stopping; public Color color, color2;
            public readonly int[] hitIds = new int[MaxHits];
            public SpriteRenderer[] srs; public int used;
        }
        internal readonly Inst[] inst = new Inst[MaxInst];
        readonly Collider2D[] _buf = new Collider2D[32];
        ContactFilter2D _filter;
        /// <summary>Looping statuses alive right now (counts toward StatusCap).</summary>
        public int StatusAlive { get; private set; }
        /// <summary>v1.7: statuses owned by other runners (TellFXRunner Burn/Chill) that share StatusCap.</summary>
        internal static int ExternalStatus;
        /// <summary>All live statuses that count toward StatusCap (Shock + Bleed + Burn + Chill).</summary>
        public static int SharedStatusAlive => (_i != null ? _i.StatusAlive : 0) + ExternalStatus;
        internal static bool StatusCapReached => SharedStatusAlive >= Mathf.Min(StatusCap, MaxInst);
        void OnDestroy() { if (_i == this) _i = null; }
        void Init()
        {
            for (int k = 0; k < MaxInst; k++) inst[k] = new Inst();
            _filter = new ContactFilter2D(); _filter.useTriggers = true;
            ReactionSprites.Build();
        }
        /// <summary>Creates all 48 x 48 SpriteRenderers up front (otherwise each slot creates its block on first use).</summary>
        public void Prewarm() { for (int k = 0; k < MaxInst; k++) Block(inst[k]); }
        void Block(Inst s)
        {
            if (s.srs != null) return;
            s.srs = new SpriteRenderer[Slots];
            for (int n = 0; n < Slots; n++) { var sr = new GameObject("r").AddComponent<SpriteRenderer>(); sr.transform.SetParent(transform, false); sr.gameObject.SetActive(false); s.srs[n] = sr; }
        }
        static bool IsStatus(Kind k) => k == Kind.Shock || k == Kind.Bleed;
        internal Inst Alloc(Kind k, int order, Color c)
        {
            if (IsStatus(k) && StatusCapReached) return null; // shared status cap reached (v1.7: includes Burn/Chill)
            for (int n = 0; n < MaxInst; n++)
            {
                var s = inst[n]; if (s.kind != Kind.None) continue;
                Block(s); s.kind = k; s.gen = (s.gen + 1) & 0x7fffff; s.t = 0; s.order = order; s.color = c; s.color2 = c; s.n = 0; s.n2 = 0; s.hits = 0;
                s.follow = null; s.hasFollow = false; s.stopping = false; s.tickAcc = 0; s.seed = s.gen * 131 + n;
                if (IsStatus(k)) StatusAlive++;
                return s;
            }
            return null; // over budget: effect skipped
        }
        internal int Handle(Inst s) { for (int n = 0; n < MaxInst; n++) if (inst[n] == s) return n | (s.gen << 8); return -1; }
        internal Inst FromHandle(int h) { if (h < 0) return null; var s = inst[h & 0xff]; return s.kind != Kind.None && s.gen == (h >> 8) ? s : null; }
        internal int Overlap(Vector2 at, float r) => Physics2D.OverlapCircle(at, r, _filter, _buf);
        internal Collider2D Buf(int i) => _buf[i];

        int _used;
        void Update()
        {
            float dt = Time.deltaTime; int frame = Time.frameCount;
            for (int k = 0; k < MaxInst; k++)
            {
                var s = inst[k]; if (s.kind == Kind.None) continue;
                s.t += dt; _used = 0;
                if (s.hasFollow && (!s.follow || !s.follow.gameObject.activeInHierarchy)) { s.hasFollow = false; s.follow = null; s.life = Mathf.Min(s.life, s.t); }
                bool alive;
                switch (s.kind)
                {
                    case Kind.Cleave: alive = ReactionFX.TickCleave(this, s, frame); break;
                    case Kind.Shock: alive = ReactionFX.TickShock(this, s, frame); break;
                    case Kind.Bleed: alive = ReactionFX.TickBleed(this, s, frame); break;
                    case Kind.Overload: alive = ReactionFX.TickOverload(this, s, frame); break;
                    case Kind.Shatter: alive = ReactionFX.TickShatter(this, s, frame); break;
                    default: alive = ReactionFX.TickChest(this, s, frame); break;
                }
                for (int n = _used; n < s.used; n++) s.srs[n].gameObject.SetActive(false);
                s.used = _used;
                if (!alive) Free(s);
            }
        }
        void Free(Inst s)
        {
            for (int n = 0; n < s.used; n++) s.srs[n].gameObject.SetActive(false);
            if (IsStatus(s.kind)) StatusAlive--;
            s.used = 0; s.kind = Kind.None; s.follow = null; s.hasFollow = false;
        }
        internal SpriteRenderer Put(Inst s, Sprite spr, Vector2 pos, Color c, bool additive, int orderOff = 0)
        {
            if (_used >= Slots || c.a <= 0.001f) return null;
            var sr = s.srs[_used++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            sr.sprite = spr; sr.color = c; sr.sortingOrder = s.order + orderOff; sr.flipX = false;
            var m = additive ? PixelFXMaterials.Additive : null; m = m != null ? m : HellFX.DefaultSpriteMaterial(sr);
            if (sr.sharedMaterial != m) sr.sharedMaterial = m;
            float ppu = HellFX.PPU; var t = sr.transform;
            t.position = new Vector3(Mathf.Round(pos.x * ppu) / ppu, Mathf.Round(pos.y * ppu) / ppu, 0);
            t.localScale = Vector3.one; t.rotation = Quaternion.identity;
            return sr;
        }
        /// <summary>1px strip from a to b, thick px wide (stretched atlas pixel).</summary>
        internal void Seg(Inst s, Vector2 a, Vector2 b, Color c, float thick, bool additive, int orderOff = 0)
        {
            Vector2 d = b - a; float len = d.magnitude * HellFX.PPU; if (len < 0.01f) return;
            var sr = Put(s, ReactionSprites.Pixel, (a + b) * 0.5f, c, additive, orderOff); if (!sr) return;
            sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            sr.transform.localScale = new Vector3(Mathf.Max(1, len + 0.6f), Mathf.Max(1, thick), 1);
        }
        internal void Box(Inst s, Vector2 c, float wPx, float hPx, Color col, bool additive, int orderOff = 0)
        {
            var sr = Put(s, ReactionSprites.Pixel, c, col, additive, orderOff); if (sr) sr.transform.localScale = new Vector3(Mathf.Max(1, Mathf.Round(wPx)), Mathf.Max(1, Mathf.Round(hPx)), 1);
        }
        internal void Kill(Inst s) { if (s != null && s.kind != Kind.None) { s.stopping = true; s.life = Mathf.Min(s.life, s.t + 0.2f); } }
    }

    /// <summary>Atlas of every v1.6.0 sprite (greyscale, tinted per use -> one texture, one batch per material).</summary>
    public static class ReactionSprites
    {
        public static Sprite Pixel, Star, Star3, Drop, Splat, SplatBig, Coin, Lid;
        public static readonly Sprite[] Shard = new Sprite[3];
        static Texture2D _tex; static float _ppu;
        public static void Build()
        {
            if (_tex != null && Mathf.Approximately(_ppu, HellFX.PPU)) return;
            _ppu = HellFX.PPU; const int W = 64, Ht = 32;
            _tex = new Texture2D(W, Ht, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "ReactionFXAtlas" };
            var px = new Color32[W * Ht];
            int x = 1, y = 1, rowH = 0;
            Sprite Add(string[] rows)
            {
                int w = rows[0].Length, h = rows.Length;
                if (x + w + 1 > W) { x = 1; y += rowH + 1; rowH = 0; }
                for (int r = 0; r < h; r++) for (int c = 0; c < w; c++)
                {
                    byte v = rows[r][c] switch { 'k' => 70, 'm' => 150, 'h' => 215, 'w' => 255, _ => 0 };
                    if (v > 0) px[(y + h - 1 - r) * W + x + c] = new Color32(v, v, v, 255);
                }
                var s = Sprite.Create(_tex, new Rect(x, y, w, h), new Vector2(0.5f, 0.5f), _ppu, 0, SpriteMeshType.FullRect);
                x += w + 1; rowH = Mathf.Max(rowH, h); return s;
            }
            Pixel = Add(new[] { "w" });
            Star = Add(new[] { "..w..", "..w..", "wwwww", "..w..", "..w.." });
            Star3 = Add(new[] { ".w.", "www", ".w." });
            Shard[0] = Add(new[] { ".w", "hm" });
            Shard[1] = Add(new[] { "whm" });
            Shard[2] = Add(new[] { "w.", "hh", ".m" });
            Drop = Add(new[] { "h", "m" });
            Splat = Add(new[] { "mhm" });
            SplatBig = Add(new[] { "k.m.k", ".mhm." });
            Coin = Add(new[] { ".h.", "hwm", ".m." });
            Lid = Add(new[] { ".kmmmhhmmmk.", "kmhhhwwhhhmk", "mmmmmmmmmmmm", "kkkkkmmkkkkk" });
            _tex.SetPixels32(px); _tex.Apply(false, true);
        }
    }

    /// <summary>v1.6.0 effects #95-100. Positions are world space, sizes in art pixels (HellFX.PPU).</summary>
    public static class ReactionFX
    {
        static ReactionFXRunner R => ReactionFXRunner.I;
        static float U => 1f / HellFX.PPU;
        /// <summary>Called once per collider the frost cleave arc passes over.</summary>
        public static Action<Collider2D> OnCleaveHit;
        /// <summary>Called once per collider inside an overload blast (fired during the Overload call).</summary>
        public static Action<Collider2D> OnOverloadHit;
        public static readonly Color FrostBlue = new Color32(140, 215, 255, 255), IceDeep = new Color32(60, 130, 220, 255), ShockBlue = new Color32(150, 210, 255, 255),
            BleedRed = new Color32(200, 24, 40, 255), FireOrange = new Color32(255, 120, 30, 255), Gold = new Color32(255, 205, 70, 255);

        static int Hash(int a, int b, int c) { unchecked { int h = a * 73856093 ^ b * 19349663 ^ c * 83492791; h ^= h >> 13; h *= 0x5bd1e995; return (h ^ (h >> 15)) & 0x7fffffff; } }
        static float H01(int a, int b, int c) => (Hash(a, b, c) & 1023) / 1023f;
        static void Shake(float px, float s) { if (ProjectileFX.ShakeScale > 0) ScreenShake.Add(px * ProjectileFX.ShakeScale, s); }
        static void Stop(float s) { if (ProjectileFX.HitStopScale > 0 && !HitStop.Active) HitStop.Play(s * ProjectileFX.HitStopScale); }
        static float Flash => Mathf.Clamp01(ProjectileFX.FlashScale);
        static Color A(Color c, float a) { c.a *= a; return c; }
        static float Step(float v, int steps) => Mathf.Floor(Mathf.Clamp01(v) * steps) / steps;
        static Vector2 Org(ReactionFXRunner.Inst s) => s.hasFollow && s.follow ? (Vector2)s.follow.position : s.pos;
        static Vector2 Dir(float deg) { float r = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)); }

        /// <summary>Stops a looping status (Shock or Bleed) with a short fade. Safe with stale or -1 handles.</summary>
        public static void StopStatus(int handle) { var s = R.FromHandle(handle); if (s != null && (s.kind == ReactionFXRunner.Kind.Shock || s.kind == ReactionFXRunner.Kind.Bleed)) R.Kill(s); }
        /// <summary>True while the handle still points at a live effect.</summary>
        public static bool IsAlive(int handle) => R.FromHandle(handle) != null;

        // ---------------- 95. Frost cleave arc ----------------
        /// <summary>Icy crescent that sweeps <paramref name="arcDeg"/> around <paramref name="origin"/> (or <paramref name="follow"/>), centred on
        /// <paramref name="dirDeg"/>: white leading edge, tapered 1-3px tintable body, frost motes left behind. Hits colliders it passes once each
        /// (OnCleaveHit). Returns false if the pool is full.</summary>
        public static bool FrostCleave(Vector2 origin, float dirDeg, float radiusPx = 18, float arcDeg = 150, float seconds = 0.22f, bool clockwise = true, Transform follow = null, Color? color = null, int order = 70)
        {
            var s = R.Alloc(ReactionFXRunner.Kind.Cleave, order, color ?? FrostBlue); if (s == null) return false;
            float half = Mathf.Clamp(arcDeg, 30, 300) * 0.5f;
            s.pos = origin; s.follow = follow; s.hasFollow = follow; s.rad = Mathf.Clamp(radiusPx, 8, 40);
            s.a0 = clockwise ? dirDeg + half : dirDeg - half; s.a1 = clockwise ? dirDeg - half : dirDeg + half;
            s.h2 = Mathf.Max(0.08f, seconds); s.life = s.h2 + 0.22f;
            Shake(0.4f, 0.05f);
            return true;
        }
        internal static bool TickCleave(ReactionFXRunner r, ReactionFXRunner.Inst s, int frame)
        {
            Vector2 o = Org(s); float sw = s.h2, k = Mathf.Clamp01(s.t / sw); k = 1 - (1 - k) * (1 - k);
            float head = Mathf.Lerp(s.a0, s.a1, k), kt = Mathf.Clamp01((s.t - sw * 0.35f) / sw); kt = 1 - (1 - kt) * (1 - kt);
            float tail = Mathf.Lerp(s.a0, s.a1, kt), fade = s.t > sw ? 1 - Step((s.t - sw) / 0.22f, 4) : 1;
            float rr = s.rad * U; const int N = 9;
            Color body = A(s.color, fade), hi = A(Color.Lerp(s.color, Color.white, 0.6f), fade);
            for (int i = 0; i < N; i++)
            {
                float u0 = i / (float)N, u1 = (i + 1) / (float)N; // 0 = tail, 1 = head
                Vector2 a = o + Dir(Mathf.Lerp(tail, head, u0)) * rr, b = o + Dir(Mathf.Lerp(tail, head, u1)) * rr;
                float th = u1 < 0.3f ? 1 : u1 < 0.7f ? 2 : 3; bool tip = i == N - 1 && s.t < sw;
                r.Seg(s, a, b, tip ? A(Color.white, fade) : (i & 1) == 0 ? body : hi, th, i >= N - 3, 0);
                if (u0 > 0.35f) r.Seg(s, o + Dir(Mathf.Lerp(tail, head, u0)) * (rr - 3 * U), o + Dir(Mathf.Lerp(tail, head, u1)) * (rr - 3 * U), A(s.color, fade * 0.45f), 1, true, -1);
            }
            for (int m = 0; m < 8; m++) // frost motes: appear where the head passed, drift out, twinkle
            {
                float when = sw * (0.1f + 0.1f * m), age = s.t - when; if (age < 0 || age > 0.32f) continue;
                float ang = Mathf.Lerp(s.a0, s.a1, Mathf.Clamp01(1 - (1 - (0.1f + 0.1f * m)) * (1 - (0.1f + 0.1f * m))));
                var p = o + Dir(ang + (H01(s.seed, m, 3) - 0.5f) * 10) * (rr + (1 + age * 14 + H01(s.seed, m, 4) * 3) * U) + Vector2.down * age * 6 * U;
                bool tw = ((frame / 3 + m) & 1) == 0;
                r.Put(s, age < 0.08f && tw ? ReactionSprites.Star3 : ReactionSprites.Pixel, p, A(tw ? Color.white : s.color, 1 - Step(age / 0.32f, 3)), true, 1);
            }
            if (s.t < sw && s.hits < ReactionFXRunner.MaxHits && OnCleaveHit != null)
            {
                int n = r.Overlap(o, rr + 3 * U); float lo = Mathf.Min(s.a0, head), hiA = Mathf.Max(s.a0, head);
                for (int q = 0; q < n; q++)
                {
                    var c = r.Buf(q); if (!c || !c.enabled) continue; var d = (Vector2)c.transform.position - o;
                    float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; float mid = (lo + hiA) * 0.5f; ang = mid + Mathf.DeltaAngle(mid, ang);
                    if (ang < lo || ang > hiA) continue;
                    int id = c.GetInstanceID(); bool seen = false; for (int h = 0; h < s.hits; h++) if (s.hitIds[h] == id) { seen = true; break; }
                    if (seen || s.hits >= ReactionFXRunner.MaxHits) continue;
                    s.hitIds[s.hits++] = id; if (s.hits == 1) { Stop(0.03f); Shake(0.8f, 0.06f); }
                    OnCleaveHit(c);
                }
            }
            return s.t < s.life;
        }

        // ---------------- 96. Shock status ----------------
        /// <summary>Looping status: 2 short jagged zaps re-rolled every ~0.15s around <paramref name="target"/> plus end sparks.
        /// Draws its own sprites (target material untouched -> horde batching intact). <paramref name="seconds"/> &lt;= 0 loops until
        /// StopStatus. Ends itself if the target is destroyed/disabled. Returns a handle, or -1 when the status cap or pool is full.</summary>
        public static int ShockStatus(Transform target, float seconds = 2f, float radiusPx = 7, float centerYPx = 0, Color? color = null, int order = 60)
        {
            if (!target) return -1;
            var s = R.Alloc(ReactionFXRunner.Kind.Shock, order, color ?? ShockBlue); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.rad = Mathf.Clamp(radiusPx, 3, 20); s.h2 = centerYPx; s.life = seconds > 0 ? seconds : float.PositiveInfinity;
            return R.Handle(s);
        }
        internal static bool TickShock(ReactionFXRunner r, ReactionFXRunner.Inst s, int frame)
        {
            Vector2 c = Org(s) + Vector2.up * s.h2 * U; float rr = s.rad * U, fade = s.stopping ? 0.5f : 1;
            for (int z = 0; z < 2; z++)
            {
                float period = 0.14f + 0.06f * z, ph = (s.t + z * 0.07f) / period; int cyc = (int)ph; float k = ph - cyc;
                if (k > 0.6f) continue;
                float a0 = H01(s.seed, cyc, z) * 360, a1 = a0 + 90 + H01(s.seed, z, cyc) * 120;
                Vector2 p0 = c + Dir(a0) * rr, p3 = c + Dir(a1) * rr, prev = p0;
                for (int i = 1; i <= 3; i++)
                {
                    float u = i / 3f; Vector2 p = i == 3 ? p3 : c + Dir(Mathf.Lerp(a0, a1, u)) * rr * (0.55f + H01(s.seed + frame / 2, i, z) * 0.7f);
                    bool hot = ((frame / 2 + i + z) & 1) == 0;
                    r.Seg(s, prev, p, A(hot ? Color.white : s.color, fade * (k < 0.3f ? 1 : 0.6f)), 1, true); prev = p;
                }
                if (k < 0.25f) r.Put(s, ReactionSprites.Star3, p3, A(Color.white, fade), true, 1);
                else r.Put(s, ReactionSprites.Pixel, p3, A(s.color, fade), true, 1);
            }
            for (int m = 0; m < 2; m++) // static crackle pixels hugging the body
            {
                if (((frame + m * 3) % 5) > 1) continue;
                r.Put(s, ReactionSprites.Pixel, c + Dir(H01(s.seed, frame / 3, m + 7) * 360) * rr * 0.8f, A(Color.white, fade), true, 1);
            }
            return s.t < s.life;
        }

        // ---------------- 97. Overload explosion ----------------
        /// <summary>Burn+Shock reaction: white core pop, expanding stepped fire ring (white-gold-orange-blood), inner ring, 5 re-rolled
        /// electric spokes, flying sparks. Shake/hit-stop through ProjectileFX.ShakeScale/HitStopScale; core flash through FlashScale.
        /// OnOverloadHit fires now for each collider in range (max 32). Returns the number of colliders hit (0 if none or pool full).</summary>
        public static int Overload(Vector2 pos, float radiusPx = 28, float seconds = 0.5f, bool juice = true, Color? fire = null, Color? shock = null, int order = 66)
        {
            var s = R.Alloc(ReactionFXRunner.Kind.Overload, order, fire ?? FireOrange); if (s == null) return 0;
            s.color2 = shock ?? ShockBlue; s.pos = pos; s.rad = Mathf.Clamp(radiusPx, 10, 64); s.life = Mathf.Max(0.25f, seconds);
            if (juice) { Shake(2.5f, 0.18f); Stop(0.06f); }
            int hits = 0;
            if (OnOverloadHit != null)
            {
                int n = R.Overlap(pos, s.rad * U);
                for (int q = 0; q < n; q++) { var c = R.Buf(q); if (!c || !c.enabled) continue; hits++; OnOverloadHit(c); }
            }
            else hits = 0;
            return hits;
        }
        internal static bool TickOverload(ReactionFXRunner r, ReactionFXRunner.Inst s, int frame)
        {
            float k = Mathf.Clamp01(s.t / s.life), e = 1 - (1 - k) * (1 - k) * (1 - k); Vector2 o = s.pos; float rr = s.rad * U * (0.25f + 0.75f * e);
            Color ring = k < 0.12f ? Color.white : k < 0.3f ? (Color)Gold : k < 0.65f ? s.color : (Color)BleedRed;
            float ringA = 1 - Step((k - 0.6f) / 0.4f, 3);
            if (k < 0.16f) { float sz = Mathf.Lerp(9, 3, k / 0.16f); r.Box(s, o, sz, sz, A(Color.white, Flash), true, 3); }
            else if (k < 0.3f) r.Put(s, ReactionSprites.Star, o, A(Gold, Flash), true, 3);
            const int N = 16;
            for (int i = 0; i < N; i++)
            {
                if (k > 0.45f && ((i + frame / 3) & 3) == 0) continue; // ring breaks up as it cools
                Vector2 a = o + Vector2.Scale(Dir(i * 360f / N), new Vector2(1, 0.75f)) * rr, b = o + Vector2.Scale(Dir((i + 1) * 360f / N), new Vector2(1, 0.75f)) * rr;
                r.Seg(s, a, b, A(ring, ringA), k < 0.35f ? 2 : 1, true, 1);
            }
            float ir = rr * 0.6f;
            for (int i = 0; i < 10 && k > 0.06f && k < 0.7f; i++)
            {
                Vector2 a = o + Vector2.Scale(Dir(i * 36f + 18), new Vector2(1, 0.75f)) * ir, b = o + Vector2.Scale(Dir(i * 36f + 54), new Vector2(1, 0.75f)) * ir;
                r.Seg(s, a, b, A(s.color, 0.7f), 1, true, 0);
            }
            if (k < 0.55f) // electric spokes
                for (int b = 0; b < 5; b++)
                {
                    float ang = b * 72 + H01(s.seed, b, 0) * 40; Vector2 prev = o; float len = rr * 1.1f;
                    for (int i = 1; i <= 3; i++)
                    {
                        var p = o + Dir(ang + (i < 3 ? (H01(s.seed + frame / 2, b, i) - 0.5f) * 50 : 0)) * len * (i / 3f);
                        r.Seg(s, prev, p, A(((frame / 2 + i) & 1) == 0 ? Color.white : s.color2, k < 0.3f ? 1 : 0.6f), k < 0.2f ? 2 : 1, true, 2); prev = p;
                    }
                    if (((frame / 2 + b) & 1) == 0) r.Put(s, ReactionSprites.Star3, prev, A(Color.white, 0.9f), true, 2);
                }
            for (int p = 0; p < 6; p++) // sparks
            {
                float ang = H01(s.seed, p, 9) * 360, sp = (40 + H01(s.seed, p, 10) * 50) * s.rad / 28f, t = s.t;
                var at = o + Dir(ang) * sp * t * (1 - 0.5f * k) * U + Vector2.down * 30 * t * t * U;
                r.Put(s, ReactionSprites.Pixel, at, A((p & 1) == 0 ? (Color)Gold : s.color2, 1 - Step(k, 3)), true, 2);
            }
            return s.t < s.life;
        }

        // ---------------- 98. Bleed status ----------------
        /// <summary>Looping status: 3 blood drops fall from the body of <paramref name="target"/> to its feet (<paramref name="feetYPx"/>)
        /// and land as small splats that fade. Own sprites only (horde-batch safe). <paramref name="seconds"/> &lt;= 0 loops until StopStatus.
        /// Returns a handle, or -1 when the status cap or pool is full.</summary>
        public static int BleedStatus(Transform target, float seconds = 3f, float bodyHeightPx = 8, float feetYPx = -7, Color? color = null, int order = 60)
        {
            if (!target) return -1;
            var s = R.Alloc(ReactionFXRunner.Kind.Bleed, order, color ?? BleedRed); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.rad = Mathf.Clamp(bodyHeightPx, 2, 24); s.h2 = feetYPx; s.life = seconds > 0 ? seconds : float.PositiveInfinity;
            return R.Handle(s);
        }
        internal static bool TickBleed(ReactionFXRunner r, ReactionFXRunner.Inst s, int frame)
        {
            Vector2 o = Org(s); float fade = s.stopping ? 0.5f : 1; Color dark = Color.Lerp(s.color, Color.black, 0.45f);
            for (int d = 0; d < 3; d++)
            {
                float period = 0.55f + 0.2f * d, ph = (s.t + d * 0.23f) / period; int cyc = (int)ph; float k = ph - cyc, tt = k * period;
                float x = (H01(s.seed, cyc, d) - 0.5f) * 7, y0 = s.h2 + 2 + H01(s.seed, d, cyc) * s.rad, fall = y0 - s.h2;
                float tf = Mathf.Sqrt(2 * fall / 220f); // gravity 220 px/s^2
                if (tt < 0.08f) r.Put(s, ReactionSprites.Pixel, o + new Vector2(x, y0) * U, A(s.color, fade), false, 1); // bead forms
                else if (tt < 0.08f + tf) { float ft = tt - 0.08f; r.Put(s, ReactionSprites.Drop, o + new Vector2(x, y0 - 110 * ft * ft) * U, A(s.color, fade), false, 1); }
                else
                {
                    float st = tt - 0.08f - tf; if (st > 0.4f) continue;
                    r.Put(s, st < 0.1f ? ReactionSprites.SplatBig : ReactionSprites.Splat, o + new Vector2(x, s.h2) * U, A(st < 0.1f ? s.color : dark, fade * (1 - Step(st / 0.4f, 3))), false, -1);
                }
            }
            if (((frame / 4) % 7) == 0) r.Put(s, ReactionSprites.Pixel, o + new Vector2((H01(s.seed, frame / 28, 5) - 0.5f) * 6, s.h2 + s.rad * 0.7f) * U, A(Color.Lerp(s.color, Color.white, 0.5f), fade), true, 2); // wet glint
            return s.t < s.life;
        }

        // ---------------- 99. Frost shatter ----------------
        /// <summary>Death burst: white pop, then <paramref name="shards"/> (max 14) 2-3px ice shards fly out, spin in 45-degree steps, fall and
        /// fade in steps, with an expanding frost-mist ring. Returns false if the pool is full.</summary>
        public static bool FrostShatter(Vector2 pos, int shards = 10, float radiusPx = 14, Color? color = null, int order = 64)
        {
            var s = R.Alloc(ReactionFXRunner.Kind.Shatter, order, color ?? FrostBlue); if (s == null) return false;
            s.pos = pos; s.n = Mathf.Clamp(shards, 3, 14); s.rad = Mathf.Clamp(radiusPx, 6, 32); s.life = 0.6f; Shake(0.8f, 0.06f);
            return true;
        }
        internal static bool TickShatter(ReactionFXRunner r, ReactionFXRunner.Inst s, int frame)
        {
            float k = s.t / s.life, sc = s.rad / 14f; Vector2 o = s.pos;
            if (k < 0.1f) r.Put(s, ReactionSprites.Star, o, A(Color.white, Flash), true, 2);
            for (int i = 0; i < s.n; i++)
            {
                float ang = i * 360f / s.n + (H01(s.seed, i, 1) - 0.5f) * 30, sp = (45 + H01(s.seed, i, 2) * 55) * sc, t = s.t;
                var v = Dir(ang) * sp; var p = o + (v * t + new Vector2(0, -150 * t * t + 25 * t)) * U;
                var sr = r.Put(s, ReactionSprites.Shard[Hash(s.seed, i, 3) % 3], p, A(k < 0.08f ? Color.white : (i & 1) == 0 ? s.color : (Color)IceDeep, 1 - Step((k - 0.6f) / 0.4f, 3)), false, 1);
                if (sr) sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Round((H01(s.seed, i, 4) * 8 + t * (H01(s.seed, i, 5) > 0.5f ? 14 : -14)) % 8) * 45);
            }
            if (k < 0.7f)
                for (int m = 0; m < 8; m++)
                {
                    float rr = s.rad * (0.3f + 0.7f * k) * U; var p = o + Vector2.Scale(Dir(m * 45 + 22), new Vector2(1, 0.7f)) * rr;
                    r.Put(s, ReactionSprites.Pixel, p, A(Color.Lerp(s.color, Color.white, 0.5f), 0.7f * (1 - Step(k / 0.7f, 3))), true, 0);
                }
            return s.t < s.life;
        }

        // ---------------- 100. Elite chest open ----------------
        /// <summary>Chest reveal at <paramref name="pos"/> (chest centre): gold flash, banded light column (<paramref name="columnPx"/> tall),
        /// lid sprite pops off and tumbles, 10 rising twinkles, <paramref name="coins"/> gold coins (max 8) and <paramref name="bloodGlints"/>
        /// blood drops (max 4) fountain out and land. Hide or swap your closed-chest sprite yourself. Returns false if the pool is full.</summary>
        public static bool EliteChestOpen(Vector2 pos, float seconds = 1.6f, float columnPx = 48, int coins = 6, int bloodGlints = 3, Color? gold = null, int order = 75)
        {
            var s = R.Alloc(ReactionFXRunner.Kind.Chest, order, gold ?? Gold); if (s == null) return false;
            s.pos = pos; s.life = Mathf.Max(0.8f, seconds); s.rad = Mathf.Clamp(columnPx, 16, 120); s.n = Mathf.Clamp(coins, 0, 8); s.n2 = Mathf.Clamp(bloodGlints, 0, 4);
            Shake(1f, 0.08f); return true;
        }
        internal static bool TickChest(ReactionFXRunner r, ReactionFXRunner.Inst s, int frame)
        {
            float k = s.t / s.life, t = s.t; Vector2 o = s.pos; Color g = s.color;
            if (t < 0.12f) { float sz = Mathf.Lerp(13, 5, t / 0.12f); r.Box(s, o, sz, sz * 0.7f, A(Color.white, Flash * 0.9f), true, 4); }
            // light column
            float grow = Mathf.Clamp01(t / 0.15f), h = s.rad * grow, wk = 1 - Step(Mathf.Clamp01((k - 0.2f) / 0.8f), 4), colA = 1 - Step((k - 0.7f) / 0.3f, 3);
            bool fl = ((frame / 3) & 1) == 0;
            var baseP = o + Vector2.up * (h * 0.5f) * U;
            r.Box(s, baseP, 1 + 6 * wk, h, A(g, 0.22f * colA), true, -2);
            r.Box(s, baseP, 1 + 3 * wk, h, A(g, (fl ? 0.55f : 0.45f) * colA), true, -1);
            r.Box(s, baseP, 1, h, A(Color.white, 0.9f * colA), true, 0);
            // lid pop
            float lt = t; if (lt < 0.7f)
            {
                var lp = o + new Vector2(14 * lt, 4 + 80 * lt - 230 * lt * lt) * U;
                var sr = r.Put(s, ReactionSprites.Lid, lp, A(g, 1 - Step((lt - 0.45f) / 0.25f, 3)), false, 3);
                if (sr) sr.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Round(lt * 300 / 22.5f) * 22.5f);
            }
            // twinkles
            for (int i = 0; i < 10; i++)
            {
                float st = t - 0.1f - i * 0.06f; if (st < 0 || st > 0.9f) continue;
                var p = o + new Vector2((H01(s.seed, i, 1) - 0.5f) * 16, 2 + st * (18 + H01(s.seed, i, 2) * 20)) * U;
                bool tw = ((frame / 3 + i) % 3) == 0;
                r.Put(s, tw ? ReactionSprites.Star3 : ReactionSprites.Pixel, p, A(tw ? Color.white : g, 1 - Step(st / 0.9f, 3)), true, 2);
            }
            // coins + blood glints fountain, land on the chest's base line
            float ground = -5;
            for (int i = 0; i < s.n + s.n2; i++)
            {
                bool blood = i >= s.n; float st = t - 0.05f - i * 0.03f; if (st < 0) continue;
                float vx = (H01(s.seed, i, 6) - 0.5f) * 90, vy = 90 + H01(s.seed, i, 7) * 50, gy = 320;
                float tl = (vy + Mathf.Sqrt(vy * vy + 2 * gy * -ground)) / gy; float tc = Mathf.Min(st, tl);
                var p = o + new Vector2(vx * tc, vy * tc - 0.5f * gy * tc * tc) * U;
                float a = 1 - Step((k - 0.75f) / 0.25f, 3);
                if (blood) r.Put(s, ReactionSprites.Drop, p, A(BleedRed, a), false, 2);
                else
                {
                    r.Put(s, ReactionSprites.Coin, p, A(g, a), false, 2);
                    if (((frame / 4 + i) % 5) == 0) r.Put(s, ReactionSprites.Pixel, p + new Vector2(-1, 1) * U, A(Color.white, a), true, 3); // glint
                }
            }
            return s.t < s.life;
        }
    }
}
