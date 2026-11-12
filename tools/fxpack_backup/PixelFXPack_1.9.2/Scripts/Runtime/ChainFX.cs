using System;
using UnityEngine;

// Pixel FX Pack v1.5.1 "Chains & Pools" - chain lightning (jump chain + hit spark), chain whip sweep, poison puddle.
// Sized for 240x135 art with a ~15px hero: 1px bolts, 4x3 iron links, 6-24px puddles. Code-made pixel sprites only.
// One pooled runner: fixed instance slots with preallocated SpriteRenderer blocks, every sprite in ONE atlas texture,
// two shared materials (additive / default sprite) -> batches. No allocations per spawn after warm-up (call Prewarm()).
// UNTESTED IN EDITOR - compiles against Unity 6 DLLs; see README "Known limits".
namespace PixelFXPack
{
    [AddComponentMenu("")]
    public class ChainFXRunner : MonoBehaviour
    {
        static ChainFXRunner _i;
        public static ChainFXRunner I { get { if (_i == null) { var go = new GameObject("ChainFX"); _i = go.AddComponent<ChainFXRunner>(); _i.Init(); } return _i; } }
        public const int MaxInst = 48, Slots = 32, MaxPts = 7, MaxSparks = 8;
        internal enum Kind { None, Lightning, Whip, Puddle }
        internal class Inst
        {
            public Kind kind; public int gen; public float t, life, hopDelay, bolt, a0, a1, lenPx, tickAcc, tickEvery; public int order, n, revealed, hits, radIdx;
            public Vector2 pos; public Transform follow; public Color color; public bool stopping;
            public readonly Vector2[] pts = new Vector2[MaxPts]; public readonly Collider2D[] cols = new Collider2D[MaxPts];
            public readonly int[] hitIds = new int[8];
            public readonly Vector2[] sparkPos = new Vector2[MaxSparks]; public readonly float[] sparkT = new float[MaxSparks]; public int sparkNext;
            public SpriteRenderer[] srs; public int used;
        }
        internal readonly Inst[] inst = new Inst[MaxInst];
        readonly Collider2D[] _buf = new Collider2D[16];
        ContactFilter2D _filter;
        void OnDestroy() { if (_i == this) _i = null; }
        void Init()
        {
            for (int k = 0; k < MaxInst; k++) inst[k] = new Inst();
            _filter = new ContactFilter2D(); _filter.useTriggers = true;
            ChainSprites.Build();
        }
        /// <summary>Creates all SpriteRenderers up front (first spawn per slot otherwise creates its block once).</summary>
        public void Prewarm() { for (int k = 0; k < MaxInst; k++) Block(inst[k]); }
        void Block(Inst s)
        {
            if (s.srs != null) return;
            s.srs = new SpriteRenderer[Slots];
            for (int n = 0; n < Slots; n++) { var sr = new GameObject("c").AddComponent<SpriteRenderer>(); sr.transform.SetParent(transform, false); sr.gameObject.SetActive(false); s.srs[n] = sr; }
        }
        internal Inst Alloc(Kind k, int order, Color c)
        {
            for (int n = 0; n < MaxInst; n++)
            {
                var s = inst[n]; if (s.kind != Kind.None) continue;
                Block(s); s.kind = k; s.gen++; s.t = 0; s.order = order; s.color = c; s.n = 0; s.revealed = 0; s.hits = 0; s.follow = null; s.stopping = false;
                s.tickAcc = 0; s.sparkNext = 0; for (int q = 0; q < MaxSparks; q++) s.sparkT[q] = -1; for (int q = 0; q < MaxPts; q++) s.cols[q] = null;
                return s;
            }
            return null; // over budget: effect skipped
        }
        internal int Handle(Inst s) { for (int n = 0; n < MaxInst; n++) if (inst[n] == s) return n | (s.gen << 8); return -1; }
        internal Inst FromHandle(int h) { if (h < 0) return null; var s = inst[h & 0xff]; return s.kind != Kind.None && s.gen == (h >> 8) ? s : null; }

        internal int Nearest(Vector2 from, float r, Inst s)
        {
            int n = Physics2D.OverlapCircle(from, r, _filter, _buf); int best = -1; float bd = float.MaxValue;
            for (int q = 0; q < n; q++)
            {
                var c = _buf[q]; if (!c || !c.enabled) continue; bool seen = false;
                for (int p = 0; p < s.n; p++) if (s.cols[p] == c) { seen = true; break; }
                if (seen) continue; float d = ((Vector2)c.transform.position - from).sqrMagnitude; if (d < bd) { bd = d; best = q; }
            }
            return best;
        }
        internal Collider2D Buf(int i) => _buf[i];
        internal int Overlap(Vector2 at, float r) => Physics2D.OverlapCircle(at, r, _filter, _buf);

        // ---------------- per-frame ----------------
        int _used;
        void Update()
        {
            float dt = Time.deltaTime; int frame = Time.frameCount;
            for (int k = 0; k < MaxInst; k++)
            {
                var s = inst[k]; if (s.kind == Kind.None) continue;
                s.t += dt; _used = 0;
                bool alive = s.kind == Kind.Lightning ? ChainFX.TickLightning(this, s, frame) : s.kind == Kind.Whip ? ChainFX.TickWhip(this, s, frame) : ChainFX.TickPuddle(this, s, frame, dt);
                ChainFX.TickSparks(this, s);
                for (int n = _used; n < s.used; n++) s.srs[n].gameObject.SetActive(false);
                s.used = _used;
                if (!alive) { for (int n = 0; n < s.used; n++) s.srs[n].gameObject.SetActive(false); s.used = 0; s.kind = Kind.None; s.follow = null; for (int q = 0; q < MaxPts; q++) s.cols[q] = null; }
            }
        }
        /// <summary>Next free sprite in this instance's block, placed and coloured (snapped to the pixel grid).</summary>
        internal SpriteRenderer Put(Inst s, Sprite spr, Vector2 pos, Color c, bool additive, int orderOff = 0)
        {
            if (_used >= Slots) return null;
            var sr = s.srs[_used++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            sr.sprite = spr; sr.color = c; sr.sortingOrder = s.order + orderOff;
            var m = additive ? PixelFXMaterials.Additive : null; m = m != null ? m : HellFX.DefaultSpriteMaterial(sr);
            if (sr.sharedMaterial != m) sr.sharedMaterial = m;
            float ppu = HellFX.PPU; var t = sr.transform;
            t.position = new Vector3(Mathf.Round(pos.x * ppu) / ppu, Mathf.Round(pos.y * ppu) / ppu, 0);
            t.localScale = Vector3.one; t.rotation = Quaternion.identity;
            return sr;
        }
    }

    /// <summary>Atlas of every v1.5.1 sprite (one texture -> one batch per material).</summary>
    public static class ChainSprites
    {
        public static readonly int[] PuddleRadii = { 6, 8, 10, 12, 16, 20, 24 };
        public static Sprite Pixel, LinkFace, LinkEdge, Star, Star3, Bubble, Pop;
        public static readonly Sprite[] Puddle = new Sprite[7 * 3]; // [radiusIdx*3 + stage]
        static Texture2D _tex; static float _ppu;
        public static void Build()
        {
            if (_tex != null && Mathf.Approximately(_ppu, HellFX.PPU)) return;
            _ppu = HellFX.PPU; const int W = 256, Ht = 128;
            _tex = new Texture2D(W, Ht, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "ChainFXAtlas" };
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
            LinkFace = Add(new[] { ".hm.", "m..k", ".mk." });
            LinkEdge = Add(new[] { "hmmk", "kkkk" });
            Star = Add(new[] { "..w..", "..w..", "wwwww", "..w..", "..w.." });
            Star3 = Add(new[] { ".w.", "www", ".w." });
            Bubble = Add(new[] { "wh", "hm" });
            Pop = Add(new[] { ".w.", "w.w", ".w." });
            for (int ri = 0; ri < PuddleRadii.Length; ri++) for (int st = 0; st < 3; st++)
            {
                int rx = Mathf.Max(2, Mathf.RoundToInt(PuddleRadii[ri] * (st == 0 ? 0.45f : st == 1 ? 0.75f : 1f))), ry = Mathf.Max(1, rx / 2);
                var rows = new string[ry * 2 + 1]; var sb = new char[rx * 2 + 1];
                for (int r = -ry; r <= ry; r++)
                {
                    for (int c = -rx; c <= rx; c++)
                    {
                        float e = (c * c) / (float)(rx * rx) + (r * r) / (float)(ry * ry);
                        // wobbly blob edge so it reads as liquid, not a perfect oval
                        float wob = 1f + 0.12f * Mathf.Sin(c * 1.3f + r * 2.1f + ri);
                        char ch = '.';
                        if (e <= wob) ch = e > wob * 0.72f ? 'k' : 'm';
                        if (ch == 'm' && r >= ry / 2 && c < 0 && e > 0.2f && e < 0.45f) ch = 'h'; // top-left sheen
                        if (ch == 'm' && r == 0 && c == -rx / 3) ch = 'w';
                        sb[c + rx] = ch;
                    }
                    rows[r + ry] = new string(sb);
                }
                Puddle[ri * 3 + st] = Add(rows);
            }
            _tex.SetPixels32(px); _tex.Apply(false, true);
        }
    }

    /// <summary>v1.5.1 effects #92-94. Positions are world space, sizes in art pixels (HellFX.PPU).</summary>
    public static class ChainFX
    {
        static ChainFXRunner R => ChainFXRunner.I;
        static float U => 1f / HellFX.PPU;
        /// <summary>Called when a chain-lightning hop lands (collider, hop index 0..). Hook damage here.</summary>
        public static Action<Collider2D, int> OnChainHit;
        /// <summary>Called once per collider the whip tip sweeps through.</summary>
        public static Action<Collider2D> OnWhipHit;
        /// <summary>Called for each collider inside a puddle every tick (see PoisonPuddle tickSeconds).</summary>
        public static Action<Collider2D> OnPuddleTick;
        public static readonly Color LightningBlue = new Color32(150, 210, 255, 255), Iron = Color.white, PoisonGreen = new Color32(110, 220, 70, 230), SparkHot = new Color32(255, 230, 160, 255);

        static int Hash(int a, int b, int c) { unchecked { int h = a * 73856093 ^ b * 19349663 ^ c * 83492791; h ^= h >> 13; h *= 0x5bd1e995; return h ^ (h >> 15); } }
        static void Shake(float px, float s) { if (ProjectileFX.ShakeScale > 0) ScreenShake.Add(px * ProjectileFX.ShakeScale, s); }
        static void Stop(float s) { if (ProjectileFX.HitStopScale > 0 && !HitStop.Active) HitStop.Play(s * ProjectileFX.HitStopScale); }
        static void Spark(ChainFXRunner.Inst s, Vector2 at) { s.sparkPos[s.sparkNext] = at; s.sparkT[s.sparkNext] = 0; s.sparkNext = (s.sparkNext + 1) % ChainFXRunner.MaxSparks; }

        // ---------------- 92. Chain lightning (jump chain) ----------------
        /// <summary>Jagged 1px bolt that jumps from <paramref name="origin"/> to the nearest Collider2D, then on to the nearest
        /// un-hit collider within <paramref name="jumpRangePx"/>, up to <paramref name="jumps"/> (max 6). Hit spark + OnChainHit per hop.
        /// Returns the number of targets hit (0 = nothing in range; a short fizzle bolt toward <paramref name="fallbackDir"/> is drawn).</summary>
        public static int ChainLightning(Vector2 origin, int jumps = 4, float jumpRangePx = 48, float firstRangePx = 64, Vector2 fallbackDir = default, Color? color = null, float hopDelay = 0.05f, float boltSeconds = 0.16f, int order = 82)
        {
            var s = R.Alloc(ChainFXRunner.Kind.Lightning, order, color ?? LightningBlue); if (s == null) return 0;
            s.pts[0] = origin; s.n = 1; jumps = Mathf.Clamp(jumps, 1, ChainFXRunner.MaxPts - 1);
            for (int h = 0; h < jumps; h++)
            {
                int q = R.Nearest(s.pts[s.n - 1], (h == 0 ? firstRangePx : jumpRangePx) * U, s); if (q < 0) break;
                var c = R.Buf(q); s.cols[s.n] = c; s.pts[s.n++] = c.transform.position;
            }
            int hit = s.n - 1;
            if (hit == 0) { var d = fallbackDir == default ? Vector2.right : fallbackDir.normalized; s.pts[s.n++] = origin + d * 14 * U; }
            s.hopDelay = hopDelay; s.bolt = boltSeconds; s.life = (s.n - 2) * hopDelay + boltSeconds + 0.14f;
            return hit;
        }
        /// <summary>Same bolt through explicit points (up to 7) - no physics. HellFX.ChainLightning (#62) forwards here.</summary>
        public static void ChainLightningPoints(Vector3 p0, Vector3 p1, Vector3 p2 = default, Vector3 p3 = default, int count = 2, Color? color = null, float hopDelay = 0.05f, float boltSeconds = 0.16f, int order = 82)
        {
            var s = R.Alloc(ChainFXRunner.Kind.Lightning, order, color ?? LightningBlue); if (s == null) return;
            s.n = Mathf.Clamp(count, 2, 4); s.pts[0] = p0; s.pts[1] = p1; s.pts[2] = p2; s.pts[3] = p3;
            s.hopDelay = hopDelay; s.bolt = boltSeconds; s.life = (s.n - 2) * hopDelay + boltSeconds + 0.14f;
        }
        internal static void LightningFromList(System.Collections.Generic.IList<Vector3> pts, Color color, float seconds, float hopDelay, int order)
        {
            var s = R.Alloc(ChainFXRunner.Kind.Lightning, order, color); if (s == null) return;
            s.n = Mathf.Min(pts.Count, ChainFXRunner.MaxPts); for (int i = 0; i < s.n; i++) s.pts[i] = pts[i];
            if (s.n < 2) { s.kind = ChainFXRunner.Kind.None; return; }
            s.hopDelay = hopDelay; s.bolt = seconds; s.life = (s.n - 2) * hopDelay + seconds + 0.14f;
        }
        internal static bool TickLightning(ChainFXRunner r, ChainFXRunner.Inst s, int frame)
        {
            const int Sub = 4; float ppu = HellFX.PPU;
            for (int h = 0; h + 1 < s.n; h++)
            {
                float t0 = h * s.hopDelay, k = (s.t - t0) / s.bolt;
                if (k < 0) break;
                if (h + 1 > s.revealed)
                {
                    s.revealed = h + 1; var b = s.pts[h + 1]; Spark(s, b);
                    var c = s.cols[h + 1]; if (c && OnChainHit != null) OnChainHit(c, h);
                    if (c) { Shake(0.5f + 0.25f * h, 0.05f); if (h == 0) Stop(0.02f); }
                }
                if (k >= 1) continue;
                Vector2 a = s.pts[h], e = s.pts[h + 1], d = e - a; float len = d.magnitude; if (len < 1e-4f) continue;
                Vector2 perp = new Vector2(-d.y, d.x) / len; Vector2 prev = a; int seed = frame / 2;
                for (int i = 1; i <= Sub; i++)
                {
                    float j = i == Sub ? 0 : ((Hash(seed, h, i) & 0x7fffffff) % 7 - 3) * Mathf.Min(1, len * ppu / 24f);
                    Vector2 p = Vector2.Lerp(a, e, i / (float)Sub) + perp * j * U, sd = p - prev;
                    bool hot = ((frame / 2 + i) & 1) == 0;
                    var sr = r.Put(s, ChainSprites.Pixel, (prev + p) * 0.5f, hot ? Color.white : new Color(s.color.r, s.color.g, s.color.b, k < 0.5f ? 1 : 0.6f), true);
                    if (sr) { sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(sd.y, sd.x) * Mathf.Rad2Deg); sr.transform.localScale = new Vector3(Mathf.Max(1, sd.magnitude * ppu + 1), k < 0.35f ? 2 : 1, 1); }
                    prev = p;
                }
            }
            return s.t < s.life;
        }

        // ---------------- 93. Chain whip sweep ----------------
        /// <summary>Iron-link chain (4x3 links, 3px pitch) that swings from <paramref name="fromDeg"/> to <paramref name="toDeg"/>
        /// around <paramref name="origin"/> (or <paramref name="follow"/>), with a 2-step ghost trail on the tip. The tip hits colliders
        /// (OnWhipHit once each, hit spark). 40px ~ 13 links reads well next to a 15px hero.</summary>
        public static void ChainWhip(Vector2 origin, float fromDeg, float toDeg, float lengthPx = 40, float seconds = 0.32f, Transform follow = null, Color? trail = null, int order = 62)
        {
            var s = R.Alloc(ChainFXRunner.Kind.Whip, order, trail ?? SparkHot); if (s == null) return;
            s.pos = origin; s.follow = follow; s.a0 = fromDeg; s.a1 = toDeg; s.lenPx = Mathf.Clamp(lengthPx, 9, 54); s.life = Mathf.Max(0.12f, seconds) + 0.12f;
            Shake(0.4f, 0.05f);
        }
        static float WhipAngle(ChainFXRunner.Inst s, float t)
        {
            float sw = s.life - 0.12f, k = Mathf.Clamp01(t / sw); k = 1 - (1 - k) * (1 - k) * (1 - k);
            return Mathf.LerpAngle(s.a0, s.a1, k);
        }
        static Vector2 LinkPos(ChainFXRunner.Inst s, Vector2 o, float t, int j, int links, out float ang)
        {
            // links further out lag behind -> the chain bends like it has weight
            ang = WhipAngle(s, t - j * 0.012f); float rad = ang * Mathf.Deg2Rad;
            float reach = Mathf.Min(1, t / 0.06f); // flick out
            return o + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (j + 1) * 3 * U * reach;
        }
        internal static bool TickWhip(ChainFXRunner r, ChainFXRunner.Inst s, int frame)
        {
            Vector2 o = s.follow ? (Vector2)s.follow.position : s.pos;
            int links = Mathf.Min(16, Mathf.RoundToInt(s.lenPx / 3));
            float sw = s.life - 0.12f, fade = s.t > sw ? 1 - (s.t - sw) / 0.12f : 1;
            // trail: 2 ghost positions of the 4 tip links
            for (int g = 2; g >= 1; g--)
                for (int j = links - 4; j < links; j++)
                {
                    if (j < 0) continue; var p = LinkPos(s, o, s.t - g * 0.025f, j, links, out _);
                    r.Put(s, ChainSprites.Pixel, p, new Color(s.color.r, s.color.g, s.color.b, (g == 1 ? 0.55f : 0.25f) * fade), true, -1);
                }
            Vector2 tip = o;
            for (int j = 0; j < links; j++)
            {
                var p = LinkPos(s, o, s.t, j, links, out float ang); tip = p;
                var sr = r.Put(s, (j & 1) == 0 ? ChainSprites.LinkFace : ChainSprites.LinkEdge, p, new Color(1, 1, 1, fade > 0.5f ? 1 : 0.5f), false);
                if (sr) sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Round(ang / 22.5f) * 22.5f);
            }
            if (s.t < sw)
            {
                int n = r.Overlap(tip, 5 * U);
                for (int q = 0; q < n; q++)
                {
                    var c = r.Buf(q); if (!c || !c.enabled) continue; int id = c.GetInstanceID(); bool seen = false;
                    for (int h = 0; h < s.hits; h++) if (s.hitIds[h] == id) { seen = true; break; }
                    if (seen || s.hits >= s.hitIds.Length) continue;
                    s.hitIds[s.hits++] = id; Spark(s, c.transform.position); Shake(0.6f, 0.05f); if (s.hits == 1) Stop(0.025f);
                    OnWhipHit?.Invoke(c);
                }
            }
            return s.t < s.life;
        }

        // ---------------- 94. Poison puddle ----------------
        /// <summary>Green ground pool: grows in 3 pixel steps, bubbles in a loop (6 bubbles: dot -> 2x2 -> pop ring), shrinks + fades.
        /// <paramref name="seconds"/> &lt;= 0 loops until <see cref="StopPuddle"/>. Rendered under units (order ~ -20).
        /// If <paramref name="tickSeconds"/> &gt; 0, OnPuddleTick fires for colliders inside the ellipse. Returns a handle.</summary>
        public static int PoisonPuddle(Vector2 pos, float radiusPx = 12, float seconds = 4f, float tickSeconds = 0.5f, Color? color = null, int order = -20)
        {
            var s = R.Alloc(ChainFXRunner.Kind.Puddle, order, color ?? PoisonGreen); if (s == null) return -1;
            int best = 0; for (int i = 1; i < ChainSprites.PuddleRadii.Length; i++) if (Mathf.Abs(ChainSprites.PuddleRadii[i] - radiusPx) < Mathf.Abs(ChainSprites.PuddleRadii[best] - radiusPx)) best = i;
            s.radIdx = best; s.pos = pos; s.life = seconds > 0 ? seconds : float.PositiveInfinity; s.tickEvery = tickSeconds; s.hopDelay = Hash(s.gen, (int)(pos.x * 97), (int)(pos.y * 89)) & 1023;
            return R.Handle(s);
        }
        /// <summary>Starts the fade-out of a looping (or timed) puddle.</summary>
        public static void StopPuddle(int handle) { var s = R.FromHandle(handle); if (s != null && s.kind == ChainFXRunner.Kind.Puddle && !s.stopping) { s.stopping = true; s.life = s.t + 0.45f; } }
        internal static bool TickPuddle(ChainFXRunner r, ChainFXRunner.Inst s, int frame, float dt)
        {
            const float Grow = 0.24f, Fade = 0.45f;
            float left = s.life - s.t; int rad = ChainSprites.PuddleRadii[s.radIdx];
            int stage = s.t < Grow ? Mathf.Min(2, (int)(s.t / Grow * 3)) : left < Fade ? Mathf.Clamp((int)(left / Fade * 3), 0, 2) : 2;
            float alpha = left < Fade ? Mathf.Floor(left / Fade * 4) / 4 : 1;
            var c = s.color; c.a *= alpha;
            r.Put(s, ChainSprites.Puddle[s.radIdx * 3 + stage], s.pos, c, false);
            if (s.t >= Grow && left > Fade * 0.5f)
            {
                int seed = (int)s.hopDelay; float rx = rad * (stage == 2 ? 0.75f : 0.45f), ry = rx * 0.5f;
                Color hi = Color.Lerp(s.color, Color.white, 0.55f); hi.a = alpha;
                for (int b = 0; b < 6; b++)
                {
                    float period = 0.8f + (Hash(seed, b, 1) & 255) / 255f * 0.5f, ph = (s.t + b * 0.37f) / period;
                    int cyc = (int)ph; float k = ph - cyc;
                    int hx = Hash(seed, b, cyc), hy = Hash(seed, cyc, b);
                    float ux = ((hx & 1023) / 1023f) * 2 - 1, uy = ((hy & 1023) / 1023f) * 2 - 1;
                    if (ux * ux + uy * uy > 1) { ux *= 0.7f; uy *= 0.7f; }
                    var at = s.pos + new Vector2(ux * rx, uy * ry) * U;
                    if (k < 0.3f) r.Put(s, ChainSprites.Pixel, at, hi, false, 1);
                    else if (k < 0.62f) r.Put(s, ChainSprites.Bubble, at + Vector2.up * U * (k > 0.45f ? 1 : 0), hi, false, 1);
                    else if (k < 0.74f) r.Put(s, ChainSprites.Pop, at + Vector2.up * U, new Color(hi.r, hi.g, hi.b, alpha * 0.8f), true, 1);
                }
            }
            if (s.tickEvery > 0 && OnPuddleTick != null && left > Fade)
            {
                s.tickAcc += dt;
                if (s.tickAcc >= s.tickEvery)
                {
                    s.tickAcc -= s.tickEvery; int n = r.Overlap(s.pos, rad * U);
                    for (int q = 0; q < n; q++)
                    {
                        var col = r.Buf(q); if (!col || !col.enabled) continue; var d = ((Vector2)col.transform.position - s.pos) * HellFX.PPU;
                        if ((d.x * d.x) / (rad * rad) + (d.y * d.y) / (rad * rad * 0.25f) <= 1) OnPuddleTick(col);
                    }
                }
            }
            return s.t < s.life;
        }

        // shared 3-frame hit spark: 5px star -> 3px star -> 1px, white then tint
        internal static void TickSparks(ChainFXRunner r, ChainFXRunner.Inst s)
        {
            float dt = Time.deltaTime;
            for (int q = 0; q < ChainFXRunner.MaxSparks; q++)
            {
                if (s.sparkT[q] < 0) continue; float k = s.sparkT[q] / 0.15f; s.sparkT[q] += dt;
                if (k >= 1) { s.sparkT[q] = -1; continue; }
                var spr = k < 0.34f ? ChainSprites.Star : k < 0.67f ? ChainSprites.Star3 : ChainSprites.Pixel;
                r.Put(s, spr, s.sparkPos[q], k < 0.34f ? Color.white : s.color, true, 2);
            }
        }
    }
}
