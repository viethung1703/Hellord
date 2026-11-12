using System;
using UnityEngine;

// Pixel FX Pack v1.8.1 juice pass (impact frames, hit-stop, kicks, sparks, speed lines, juicier stun) on top of
// v1.8.0 Ox Pit rework (looks re-designed from research/oxpit_v18_references.md; API unchanged).
// Originally v1.7.0 "Ox Pit" vertical slice (Hellord design doc v0.2, Level 10):
// lane / cone telegraphs, stomp rings, stun stars, punish-window glow, dust puff + dust trail, stampede lane, bloodrage fire trail
// (TellFX #101-109) and pooled Burn / Chill overlays (StatusFX #110-111). Visual only: hit hooks are optional; damage lives in game code.
// Sized for 240x135 art with a ~15px hero. Same design as ChainFX / ReactionFX: one pooled runner, fixed instance slots with
// preallocated SpriteRenderer blocks, ONE atlas texture, two shared materials (additive / default sprite), no per-spawn allocations
// after TellFXRunner.I.Prewarm(). Burn / Chill never touch the target's material (HordeFX batching intact) and share
// ReactionFXRunner.StatusCap with Shock / Bleed. UNTESTED IN EDITOR - compiles against Unity 6000.0.85f1 DLLs.
namespace PixelFXPack
{
    [AddComponentMenu("")]
    public class TellFXRunner : MonoBehaviour
    {
        static TellFXRunner _i;
        public static TellFXRunner I { get { if (_i == null) { var go = new GameObject("TellFX"); _i = go.AddComponent<TellFXRunner>(); _i.Init(); } return _i; } }
        /// <summary>Instance slots (all 11 effects share them), sprites per instance, stampede hits remembered, trail puffs/patches per instance.</summary>
        public const int MaxInst = 48, Slots = 48, MaxHits = 16, MaxTrail = 10; // v1.8: 48 sprites/instance (opaque stomp rings + flame flipbooks)
        internal enum Kind { None, Lane, Cone, Stomp, Stun, Punish, Dust, DustTrail, Stampede, Bloodrage, Burn, Chill, Impact, Speed, Hit }
        internal class Inst
        {
            public Kind kind; public int gen, order, seed, n, hits, fired, trailHead; public float t, life, tell, a0, a1, rad, w, h2, spd, acc, squash;
            public Vector2 pos, pos2, last; public Transform follow; public bool hasFollow, stopping, flag; public Color color, color2;
            public Sprite[] frames; public SpriteRenderer tsr; public Color tint0; public bool tinted;
            public readonly int[] hitIds = new int[MaxHits];
            public readonly Vector3[] trail = new Vector3[MaxTrail]; // x, y, birth time
            public SpriteRenderer[] srs; public int used;
        }
        internal readonly Inst[] inst = new Inst[MaxInst];
        readonly Collider2D[] _buf = new Collider2D[16];
        ContactFilter2D _filter;
        /// <summary>Burn + Chill overlays alive (they also count toward ReactionFXRunner.StatusCap).</summary>
        public int StatusAlive { get; private set; }
        void OnDestroy() { if (_i == this) { ReactionFXRunner.ExternalStatus = Mathf.Max(0, ReactionFXRunner.ExternalStatus - StatusAlive); _i = null; } }
        void Init()
        {
            for (int k = 0; k < MaxInst; k++) inst[k] = new Inst();
            _filter = new ContactFilter2D(); _filter.useTriggers = true;
            TellSprites.Build();
        }
        /// <summary>Creates all 48 x 48 = 2304 SpriteRenderers up front (otherwise each slot creates its block on first use).</summary>
        public void Prewarm() { for (int k = 0; k < MaxInst; k++) Block(inst[k]); }
        void Block(Inst s)
        {
            if (s.srs != null) return;
            s.srs = new SpriteRenderer[Slots];
            for (int n = 0; n < Slots; n++) { var sr = new GameObject("t").AddComponent<SpriteRenderer>(); sr.transform.SetParent(transform, false); sr.gameObject.SetActive(false); s.srs[n] = sr; }
        }
        internal static bool IsStatus(Kind k) => k == Kind.Burn || k == Kind.Chill;
        internal Inst Alloc(Kind k, int order, Color c)
        {
            if (IsStatus(k) && ReactionFXRunner.StatusCapReached) return null; // shared status cap
            for (int n = 0; n < MaxInst; n++)
            {
                var s = inst[n]; if (s.kind != Kind.None) continue;
                Block(s); s.kind = k; s.gen = (s.gen + 1) & 0x7fffff; s.t = 0; s.order = order; s.color = c; s.color2 = c; s.n = 0; s.hits = 0; s.fired = 0;
                s.follow = null; s.hasFollow = false; s.stopping = false; s.flag = false; s.acc = 0; s.trailHead = 0; s.frames = null; s.tsr = null; s.tinted = false; s.squash = 1; s.a0 = 0; s.a1 = 0; s.rad = 0; s.w = 0; s.spd = 0; s.last = Vector3.zero; s.seed = s.gen * 131 + n;
                for (int q = 0; q < MaxTrail; q++) s.trail[q] = new Vector3(0, 0, -99);
                if (IsStatus(k)) { StatusAlive++; ReactionFXRunner.ExternalStatus++; }
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
                    case Kind.Lane: alive = TellFX.TickLane(this, s, frame); break;
                    case Kind.Cone: alive = TellFX.TickCone(this, s, frame); break;
                    case Kind.Stomp: alive = TellFX.TickStomp(this, s, frame); break;
                    case Kind.Stun: alive = TellFX.TickStun(this, s, frame); break;
                    case Kind.Punish: alive = TellFX.TickPunish(this, s, frame); break;
                    case Kind.Dust: alive = TellFX.TickDust(this, s, frame); break;
                    case Kind.DustTrail: alive = TellFX.TickDustTrail(this, s, frame, dt); break;
                    case Kind.Stampede: alive = TellFX.TickStampede(this, s, frame); break;
                    case Kind.Bloodrage: alive = TellFX.TickBloodrage(this, s, frame, dt); break;
                    case Kind.Burn: alive = StatusFX.TickBurn(this, s, frame); break;
                    case Kind.Chill: alive = StatusFX.TickChill(this, s, frame); break;
                    case Kind.Impact: alive = TellFX.TickImpact(this, s, frame); break;
                    case Kind.Speed: alive = TellFX.TickSpeed(this, s, frame, dt); break;
                    default: alive = TellFX.TickHit(this, s, frame); break;
                }
                for (int n = _used; n < s.used; n++) s.srs[n].gameObject.SetActive(false);
                s.used = _used;
                if (!alive) Free(s);
            }
        }
        void Free(Inst s)
        {
            for (int n = 0; n < s.used; n++) s.srs[n].gameObject.SetActive(false);
            if (s.kind == Kind.Stun && s.flag && s.tsr) s.tsr.transform.localPosition = new Vector3(s.pos2.x, s.pos2.y, s.tsr.transform.localPosition.z); // v1.8.1 wobble restore
            if (s.tinted && s.tsr) s.tsr.color = s.tint0; // v1.8: Burn/Chill vertex tint restored
            s.tinted = false;
            if (IsStatus(s.kind)) { StatusAlive--; ReactionFXRunner.ExternalStatus = Mathf.Max(0, ReactionFXRunner.ExternalStatus - 1); }
            s.used = 0; s.kind = Kind.None; s.follow = null; s.hasFollow = false; s.frames = null; s.tsr = null;
        }
        internal static bool Fin(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        internal SpriteRenderer Put(Inst s, Sprite spr, Vector2 pos, Color c, bool additive, int orderOff = 0)
        {
            if (_used >= Slots || c.a <= 0.001f || spr == null || !Fin(pos.x) || !Fin(pos.y)) return null; // v1.8.5: never push NaN/Inf into a Transform
            var sr = s.srs[_used++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            sr.sprite = spr; sr.color = c; sr.sortingOrder = s.order + orderOff; sr.flipX = false; if (sr.sortingLayerID != 0) sr.sortingLayerID = 0;
            var m = additive ? PixelFXMaterials.Additive : null; m = m != null ? m : HellFX.DefaultSpriteMaterial(sr);
            if (sr.sharedMaterial != m) sr.sharedMaterial = m;
            float ppu = HellFX.PPU; var t = sr.transform;
            t.position = new Vector3(Mathf.Round(pos.x * ppu) / ppu, Mathf.Round(pos.y * ppu) / ppu, 0);
            t.localScale = Vector3.one; t.rotation = Quaternion.identity;
            return sr;
        }
        /// <summary>Strip from a to b, thick px wide (stretched atlas pixel).</summary>
        internal void Seg(Inst s, Vector2 a, Vector2 b, Color c, float thick, bool additive, int orderOff = 0)
        {
            Vector2 d = b - a; float len = d.magnitude * HellFX.PPU; if (!(len >= 0.01f) || !Fin(len) || !Fin(thick)) return;
            var sr = Put(s, TellSprites.Pixel, (a + b) * 0.5f, c, additive, orderOff); if (!sr) return;
            sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            sr.transform.localScale = new Vector3(Mathf.Max(1, len + 0.6f), Mathf.Max(1, thick), 1);
        }
        /// <summary>Rotated filled rectangle centred on c, wPx along deg, hPx across.</summary>
        internal void RBox(Inst s, Vector2 c, float wPx, float hPx, float deg, Color col, bool additive, int orderOff = 0)
        {
            if (!Fin(wPx) || !Fin(hPx) || !Fin(deg)) return;
            var sr = Put(s, TellSprites.Pixel, c, col, additive, orderOff); if (!sr) return;
            sr.transform.rotation = Quaternion.Euler(0, 0, deg);
            sr.transform.localScale = new Vector3(Mathf.Max(1, Mathf.Round(wPx)), Mathf.Max(1, Mathf.Round(hPx)), 1);
        }
        /// <summary>Ellipse outline (rx px, ry = rx * squash) from segs strips. Every second segment skipped when dashed.</summary>
        internal void Ring(Inst s, Vector2 c, float rPx, float squash, int segs, Color col, float thick, bool additive, int orderOff = 0, bool dashed = false, int dashPhase = 0)
        {
            float U = 1f / HellFX.PPU;
            for (int i = 0; i < segs; i++)
            {
                if (dashed && ((i + dashPhase) & 1) == 1) continue;
                float a = i * Mathf.PI * 2 / segs, b = (i + 1) * Mathf.PI * 2 / segs;
                Seg(s, c + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * squash) * rPx * U, c + new Vector2(Mathf.Cos(b), Mathf.Sin(b) * squash) * rPx * U, col, thick, additive, orderOff);
            }
        }
        internal void Kill(Inst s, float fade = 0.2f) { if (s != null && s.kind != Kind.None) { s.stopping = true; s.life = Mathf.Min(s.life, s.t + fade); } }
    }


    /// <summary>Atlas of every TellFX / StatusFX sprite (greyscale, tinted per use -> one texture, one batch per material).
    /// v1.8: 128x48, adds flame flipbooks, glint, pebble, chevron, ice shard, scorch, impact star.</summary>
    public static class TellSprites
    {
        public static Sprite Pixel, Star3, StunStar, PuffS, PuffM, PuffL, Ember, Flake, Crust;
        /// <summary>v1.7 flame tongues (kept for compatibility).</summary>
        public static readonly Sprite[] Flame = new Sprite[3];
        /// <summary>v1.8: 5x6 flame flipbook (3 loop frames + [3] = small ignite / die-down frame), pivot at the base.</summary>
        public static readonly Sprite[] FlameBook = new Sprite[4];
        /// <summary>v1.8: 5x3 flame-crown flipbook for tiny burning enemies, pivot at the base.</summary>
        public static readonly Sprite[] FlameSmall = new Sprite[4];
        /// <summary>v1.8: 4-point wind-up glint, 1px pebble, 4x7 chevron (points +x), 5x5 ice shard (pivot base), 9x3 scorch, 9x9 impact star.</summary>
        public static Sprite Glint, Pebble, Chevron, IceShard, Scorch, Impact;
        /// <summary>v1.8.1: 5x3 dizzy birdie, 2 flap frames.</summary>
        public static readonly Sprite[] Birdie = new Sprite[2];
        /// <summary>Placeholder 14x9 running ox silhouette, 2 frames (stand-in until Hellord Art draws the stampede oxen).</summary>
        public static readonly Sprite[] Ox = new Sprite[2];
        static Texture2D _tex; static float _ppu;
        public static void Build()
        {
            if (_tex != null && Mathf.Approximately(_ppu, HellFX.PPU)) return;
            _ppu = HellFX.PPU; const int W = 128, Ht = 48;
            _tex = new Texture2D(W, Ht, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "TellFXAtlas" };
            var px = new Color32[W * Ht];
            int x = 1, y = 1, rowH = 0;
            Sprite Add(string[] rows, float pivotY = 0.5f)
            {
                int w = rows[0].Length, h = rows.Length;
                if (x + w + 1 > W) { x = 1; y += rowH + 1; rowH = 0; }
                for (int r = 0; r < h; r++) for (int c = 0; c < w; c++)
                {
                    byte v = rows[r][c] switch { 'k' => 70, 'm' => 150, 'h' => 215, 'w' => 255, _ => 0 };
                    if (v > 0) px[(y + h - 1 - r) * W + x + c] = new Color32(v, v, v, 255);
                }
                var s = Sprite.Create(_tex, new Rect(x, y, w, h), new Vector2(0.5f, pivotY), _ppu, 0, SpriteMeshType.FullRect);
                x += w + 1; rowH = Mathf.Max(rowH, h); return s;
            }
            Pixel = Add(new[] { "w" });
            Star3 = Add(new[] { ".w.", "www", ".w." });
            StunStar = Add(new[] { "..w..", ".www.", "wwwww", ".w.w." });
            PuffS = Add(new[] { "hm", "mk" });
            PuffM = Add(new[] { ".hh.", "hhmm", ".mk." });
            PuffL = Add(new[] { ".hhh.", "hhhmm", "hmmmk", ".mkk." });
            Ember = Add(new[] { "w", "m" });
            Flake = Add(new[] { "h.h", ".w.", "h.h" });
            Crust = Add(new[] { "h.w.h", "mhmhm" });
            Flame[0] = Add(new[] { ".w.", "whw", "hmh" }, 0);
            Flame[1] = Add(new[] { ".w.", ".w.", "whw", "hmh" }, 0);
            Flame[2] = Add(new[] { "w..", ".w.", "whw", "whh", "hmh" }, 0);
            FlameBook[0] = Add(new[] { ".....", "..w..", ".whw.", ".whw.", "whmhw", ".mmm." }, 0);
            FlameBook[1] = Add(new[] { "..w..", ".ww..", ".whw.", "whhw.", "whhmw", ".mmm." }, 0);
            FlameBook[2] = Add(new[] { ".....", ".w.w.", ".whw.", "whwhw", "whmhw", ".mmm." }, 0);
            FlameBook[3] = Add(new[] { ".....", ".....", "..w..", ".whw.", "whmhw", ".mmm." }, 0);
            FlameSmall[0] = Add(new[] { "..w..", ".whw.", ".hmh." }, 0);
            FlameSmall[1] = Add(new[] { ".w...", ".whw.", ".hmh." }, 0);
            FlameSmall[2] = Add(new[] { "...w.", ".whw.", ".hmh." }, 0);
            FlameSmall[3] = Add(new[] { ".....", ".wh..", ".hmh." }, 0);
            Glint = Add(new[] { "..w..", "..h..", "whwhw", "..h..", "..w.." });
            Pebble = Add(new[] { "hm" });
            Chevron = Add(new[] { "w...", "hw..", ".hw.", "..hw", ".hw.", "hw..", "w..." });
            IceShard = Add(new[] { "..w..", ".whw.", ".hmh.", "whmm.", "mmmk." }, 0);
            Scorch = Add(new[] { "..wwwww..", "wwwwwwwww", "..wwwww.." });
            Impact = Add(new[] { "w...w...w", ".w..w..w.", "..w.w.w..", "...www...", "wwwwwwwww", "...www...", "..w.w.w..", ".w..w..w.", "w...w...w" });
            Birdie[0] = Add(new[] { "w...w", ".wmw.", "..h.." });
            Birdie[1] = Add(new[] { ".....", "wwmww", "..h.." });
            Ox[0] = Add(new[] { ".....kk.......", "....kmmk..k.k.", "kkkkmmmmkkkhk.", "kmmmmmmmmmmmmk", "kmmmmmmmmmmmhw", ".kmmmmmmmmmmk.", "..km.k...mk...", "..k..k...k.k..", ".k....k..k..k." });
            Ox[1] = Add(new[] { ".....kk.......", "....kmmk..k.k.", "kkkkmmmmkkkhk.", "kmmmmmmmmmmmmk", "kmmmmmmmmmmmhw", ".kmmmmmmmmmmk.", "...kmk..kmk...", "...k.k..k.k...", "...k..k.k..k.." });
            _tex.SetPixels32(px); _tex.Apply(false, true);
        }
    }

    /// <summary>Telegraph / hazard / boss FX #101-109. Positions are world space, sizes in art pixels (HellFX.PPU).
    /// Tell times are clamped to the doc's 0.5-1 s rule. v1.8 look: every tell shares one vocabulary - dark outline, thin wash,
    /// 1px crimson rim, a bright "clock" front that reaches the impact edge exactly when it fires, rim blinks white twice, then a
    /// 1-2 frame flash. See research/oxpit_v18_references.md.</summary>
    public static class TellFX
    {
        static TellFXRunner R => TellFXRunner.I;
        static float U => 1f / HellFX.PPU;
        /// <summary>Optional: fires when a tell resolves (handle, index). Lane/cone/stampede: index 0 when the tell ends; stomp: ring index as each ring launches.</summary>
        public static Action<int, int> OnTellFire;
        /// <summary>Optional: once per collider a stampede ox runs over (visual helper only; the doc says damage should live in game code).</summary>
        public static Action<Collider2D> OnStampedeHit;
        /// <summary>Enemy-tell colours (Endesga 32). TellRed = rim, TellHot = clock front / chevrons.</summary>
        public static readonly Color TellRed = new Color32(0xe4, 0x3b, 0x44, 255), TellHot = new Color32(0xfe, 0xae, 0x34, 255), Dust = new Color32(232, 183, 150, 255),
            StunYellow = new Color32(254, 231, 97, 255), PunishGold = new Color32(254, 174, 52, 255), RageRed = new Color32(228, 59, 68, 255), OxBrown = new Color32(62, 39, 49, 255);
        /// <summary>v1.8 extra palette: wash (dark crimson #a22633), clock front (#fee761), scorch (#3e2731), steam (#c0cbdc), dust shade (#c28569).</summary>
        public static readonly Color Wash = new Color32(0xa2, 0x26, 0x33, 255), Clock = new Color32(0xfe, 0xe7, 0x61, 255), ScorchCol = new Color32(0x3e, 0x27, 0x31, 255),
            Steam = new Color32(0xc0, 0xcb, 0xdc, 255), DustShade = new Color32(0xc2, 0x85, 0x69, 255), Orange = new Color32(0xf7, 0x76, 0x22, 255);
        public const float MinTell = 0.5f, MaxTell = 1f;
        /// <summary>Fraction of the tell after which the rim blinks white twice (the "now" cue).</summary>
        public const float BlinkFrom = 0.85f;

        static int Hash(int a, int b, int c) { unchecked { int h = a * 73856093 ^ b * 19349663 ^ c * 83492791; h ^= h >> 13; h *= 0x5bd1e995; return (h ^ (h >> 15)) & 0x7fffffff; } }
        internal static float H01(int a, int b, int c) => (Hash(a, b, c) & 1023) / 1023f;
        static void Shake(float px, float s) { if (ProjectileFX.ShakeScale > 0) ScreenShake.Add(px * ProjectileFX.ShakeScale, s); }
        internal static Color A(Color c, float a) { c.a *= a; return c; }
        internal static float Step(float v, int steps) => Mathf.Floor(Mathf.Clamp01(v) * steps) / steps;
        internal static Vector2 Org(TellFXRunner.Inst s) => s.hasFollow && s.follow ? (Vector2)s.follow.position : s.pos;
        static float fade(TellFXRunner.Inst s) => s.stopping ? 0.5f : 1;
        static Vector2 Dir(float deg) { float r = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)); }
        static float Tell(float s) => Mathf.Clamp(s, MinTell, MaxTell);
        static void Fire(TellFXRunner.Inst s, int index) { var cb = OnTellFire; if (cb != null) cb(R.Handle(s), index); }
        /// <summary>Clock position 0..1: the front reaches the end at BlinkFrom, then holds while the rim blinks.</summary>
        static float ClockK(float k) => Mathf.Clamp01(k / BlinkFrom);
        static bool Blink(float k) => k >= BlinkFrom && (((int)((k - BlinkFrom) / (1 - BlinkFrom) * 4)) & 1) == 0;
        static float Flash => Mathf.Clamp01(ProjectileFX.FlashScale);

        // ---- solid-colour silhouette copies (boss outlines). Two shared materials, created once, only used on boss-sized targets. ----
        static Material _gold, _red;
        static readonly int ID_Flash = Shader.PropertyToID("_FlashAmount"), ID_FlashColor = Shader.PropertyToID("_FlashColor");
        static Material _white, _black;
        /// <summary>v1.8.1: 0 = white impact frame, 1 = black impact frame (cached clones of PixelFX_Uber, created on first use).</summary>
        internal static Material SolidWB(bool black)
        {
            var m = black ? _black : _white; if (m != null) return m;
            var b = PixelFXMaterials.Shared; if (b == null || !b.HasProperty(ID_Flash)) return null;
            m = new Material(b) { name = black ? "TellFX Solid Black" : "TellFX Solid White", hideFlags = HideFlags.DontSave };
            m.SetFloat(ID_Flash, 1); m.SetColor(ID_FlashColor, black ? (Color)Outline : Color.white);
            if (black) _black = m; else _white = m; return m;
        }
        /// <summary>v1.8.1: one squashed solid copy of <paramref name="t"/> drawn OVER it (impact / hit frame). sx/sy = squash factors.</summary>
        // v1.8.2: one impact-frame pair per target at a time (WallImpact + StunStars used to stack two silhouettes).
        static readonly System.Collections.Generic.Dictionary<int, TellFXRunner.Inst> _ifOwner = new System.Collections.Generic.Dictionary<int, TellFXRunner.Inst>();
        static readonly System.Collections.Generic.Dictionary<int, float> _ifStart = new System.Collections.Generic.Dictionary<int, float>();
        internal static bool ImpactFrameFree(TellFXRunner.Inst s)
        {
            int id = s.tsr.GetInstanceID(); float now = Time.unscaledTime;
            if (_ifOwner.TryGetValue(id, out var o) && o != s && now - _ifStart[id] < 0.2f) return false;
            if (!_ifOwner.TryGetValue(id, out o) || o != s) { _ifOwner[id] = s; _ifStart[id] = now; }
            return true;
        }
        internal static void SilhouetteOver(TellFXRunner r, TellFXRunner.Inst s, SpriteRenderer t, Material solid, float sx, float sy, Vector2 offPx)
        {
            if (solid == null || !Shaped(t)) return;
            var sr = r.Put(s, t.sprite, (Vector2)t.transform.position + offPx * U, Color.white, false, 0); if (!sr) return;
            sr.sharedMaterial = solid; sr.sortingLayerID = t.sortingLayerID; sr.sortingOrder = t.sortingOrder + 1; sr.flipX = t.flipX; sr.flipY = t.flipY;
            sr.transform.rotation = t.transform.rotation; var ls = t.transform.lossyScale; sr.transform.localScale = new Vector3(ls.x * sx, ls.y * sy, 1);
        }
        /// <summary>v1.8.1 juice switches. Hit-stop changes Time.timeScale (via HitStop) - set JuiceHitStop = false if your game owns timeScale.
        /// All shake / kick goes through ScreenShake.Main and ProjectileFX.ShakeScale; hit-stop through ProjectileFX.HitStopScale.</summary>
        public static bool JuiceHitStop = true, StunWobble = true, ImpactFrames = true;
        static void Kick(Vector2 dirPx) { var m = ScreenShake.Main; if (m != null && ProjectileFX.ShakeScale > 0) m.Kick(dirPx * ProjectileFX.ShakeScale); }
        static void Freeze(float sec) { if (JuiceHitStop && ProjectileFX.HitStopScale > 0 && sec > 0) HitStop.Play(sec * ProjectileFX.HitStopScale); }
        /// <summary>Shared "tell complete" pop: glint + 1px ring that snaps out to 7px over 0.1 s at the attacker.</summary>
        static void TellPop(TellFXRunner r, TellFXRunner.Inst s, Vector2 at, float ft)
        {
            if (ft >= 0.1f) return; float k = ft / 0.1f;
            r.Ring(s, at, 2 + k * 5, 0.6f, 8, A(Color.white, (1 - k * 0.5f)), 1, true, 4);
            if (ft < 0.05f) r.Put(s, TellSprites.Glint, at, Color.white, true, 5);
        }
        internal static Material Solid(bool gold)
        {
            var m = gold ? _gold : _red; if (m != null) return m;
            var b = PixelFXMaterials.Shared; if (b == null || !b.HasProperty(ID_Flash)) return null;
            m = new Material(b) { name = gold ? "TellFX Solid Gold" : "TellFX Solid Red", hideFlags = HideFlags.DontSave };
            m.SetFloat(ID_Flash, 1); m.SetColor(ID_FlashColor, gold ? (Color)PunishGold : RageRed);
            if (gold) _gold = m; else _red = m; return m;
        }
        static readonly Vector2[] Off4 = { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) };
        internal static bool Shaped(SpriteRenderer t) => t && t.enabled && t.sprite && t.gameObject.activeInHierarchy;
        /// <summary>1px silhouette outline of <paramref name="t"/> (4 offset copies, solid colour material, behind the target).</summary>
        internal static void Outline1(TellFXRunner r, TellFXRunner.Inst s, SpriteRenderer t, Material solid, float alpha)
        {
            if (solid == null || alpha <= 0.001f) return;
            for (int i = 0; i < 4; i++)
            {
                var sr = r.Put(s, t.sprite, (Vector2)t.transform.position + Off4[i] * U, new Color(1, 1, 1, alpha), false, 0); if (!sr) return;
                sr.sharedMaterial = solid; sr.sortingLayerID = t.sortingLayerID; sr.sortingOrder = t.sortingOrder - 1; sr.flipX = t.flipX; sr.flipY = t.flipY;
                sr.transform.rotation = t.transform.rotation; sr.transform.localScale = t.transform.lossyScale;
            }
        }

        /// <summary>Stops any TellFX effect early (short fade). Safe with stale or -1 handles.</summary>
        public static void Stop(int handle) { var s = R.FromHandle(handle); if (s != null) R.Kill(s); }
        /// <summary>True while the handle still points at a live TellFX / StatusFX effect.</summary>
        public static bool IsAlive(int handle) => R.FromHandle(handle) != null;

        // ---------------- 101. Lane telegraph (line / charge lane; Hound dash, Ox Demon Calf, Ox-Head Gore Charge) ----------------
        /// <summary>Ground lane from <paramref name="from"/> to <paramref name="to"/>, <paramref name="widthPx"/> wide. v1.8 look: dark outline,
        /// thin wash, 1px crimson rim; a bright clock bar sweeps from the attacker to the end (stronger fill behind it, 3 chevrons trailing it)
        /// and reaches the end at 85% of the tell; the rim then blinks white twice; a wind-up glint sits on the attacker. On fire: 2-frame
        /// white flash, rim fades in 0.2 s (OnTellFire(handle, 0)). Returns a handle, or -1 if the pool is full.</summary>
        public static int LaneTelegraph(Vector2 from, Vector2 to, float widthPx = 12, float seconds = 0.75f, Color? color = null, int order = 5)
        {
            var s = R.Alloc(TellFXRunner.Kind.Lane, order, color ?? TellRed); if (s == null) return -1;
            s.pos = from; s.pos2 = to; s.w = Mathf.Clamp(widthPx, 4, 48); s.tell = Tell(seconds); s.life = s.tell + 0.25f; s.color2 = TellHot;
            return R.Handle(s);
        }
        internal static readonly Color Outline = new Color32(24, 20, 37, 255);
        internal static bool TickLane(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 a = s.pos, d = s.pos2 - a; float L = d.magnitude; if (L < 1e-4f) return false;
            Vector2 f = d / L, n = new Vector2(-f.y, f.x); float deg = Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg, Lpx = L * HellFX.PPU, hw = s.w * 0.5f * U;
            float fd = fade(s); Vector2 mid = a + f * L * 0.5f, ei = n * (hw - 0.5f * U);
            if (s.t < s.tell)
            {
                float k = s.t / s.tell, ck = ClockK(k), fl = Mathf.Round(Lpx * ck) * U; bool bl = Blink(k);
                Color rim = A(bl ? Color.white : s.color, fd);
                // v1.8.5 fix: border only (was a filled Lpx+2 x w+2 box at 75% -> solid dark-red strip under the wash). 1px ring just outside the rim.
                Color ol = A(Outline, 0.75f * fd); Vector2 on = n * (hw + 0.5f * U), of = f * (L * 0.5f + 0.5f * U);
                r.RBox(s, mid + on, Lpx + 2, 1, deg, ol, false, -3); r.RBox(s, mid - on, Lpx + 2, 1, deg, ol, false, -3);    // long sides
                r.RBox(s, mid + of, 1, s.w, deg, ol, false, -3);                                                            // far end cap only (v1.8.6: matches the approved v1.8 mock-up; open at the attacker)
                r.RBox(s, mid, Lpx, s.w, deg, A(Wash, 0.16f * fd), false, -2);                                              // thin wash
                if (fl * HellFX.PPU >= 1) r.RBox(s, a + f * fl * 0.5f, fl * HellFX.PPU, s.w - 2, deg, A(Wash, 0.22f * fd), false, -1); // filled behind clock
                r.Seg(s, a + ei, a + f * L + ei, rim, 1, false); r.Seg(s, a - ei, a + f * L - ei, rim, 1, false);           // 1px rim
                Vector2 end = a + f * (L - 0.5f * U); r.Seg(s, end + n * hw, end - n * hw, rim, 1, false);
                if (ck < 1) { Vector2 c = a + f * fl; r.Seg(s, c + n * (hw - U), c - n * (hw - U), A(Clock, fd), 1, false, 2); r.RBox(s, c, 3, s.w - 2, deg, A(s.color2, 0.25f * fd), true, 1); }
                float chp = Mathf.Max(1, s.w * 0.5f - 2), ch = chp * U, cb = 0.8f * chp * U;                                // v1.8.6: chevron size as in the v1.8 mock-up (arms reach hw-2, slope 0.8)
                for (int i = 1; i <= 3; i++)                                                                                // chevrons trail the clock only
                {
                    float dp = fl * HellFX.PPU - i * 7; if (dp < 3) break;
                    Vector2 c = a + f * dp * U; Color cc = A(i == 1 ? s.color2 : s.color, (0.9f - 0.2f * i) * fd);
                    r.Seg(s, c - f * cb + n * ch, c, cc, 1, false, 1); r.Seg(s, c - f * cb - n * ch, c, cc, 1, false, 1);
                }
                if (k < 0.3f) r.Put(s, ((frame >> 2) & 1) == 0 ? TellSprites.Glint : TellSprites.Star3, a + n * hw + f * 2 * U, A(Color.white, fd), true, 3); // wind-up glint
                return true;
            }
            if (!s.flag) { s.flag = true; Fire(s, 0); Kick(-f * 1.5f); }
            float ft = s.t - s.tell; TellPop(r, s, a, ft);
            if (ft < 0.067f) r.RBox(s, mid, Lpx, s.w, deg, A(Color.white, 0.55f * Flash), true, 1);
            else { Color rc = A(s.color, (1 - (ft - 0.067f) / 0.18f) * fd); r.Seg(s, a + ei, a + f * L + ei, rc, 1, false); r.Seg(s, a - ei, a + f * L - ei, rc, 1, false); }
            return s.t < s.life;
        }

        // ---------------- 102. Cone telegraph (Ox-Head Horn Sweep; shared cone tell) ----------------
        /// <summary>Wedge from <paramref name="origin"/> facing <paramref name="dirDeg"/>, <paramref name="arcDeg"/> wide, <paramref name="rangePx"/> long.
        /// v1.8 look: same vocabulary as the lane - dark outline, thin wash, 1px crimson rim, a clock arc sweeping out to the range (stronger
        /// fill behind it), rim blinks white twice. On fire: a 2-frame white-then-gold smear band on the outer 40% (the horn swing), then
        /// a thin crimson remnant arc fades (OnTellFire(handle, 0)). Optional <paramref name="follow"/> moves the origin. Returns a handle or -1.</summary>
        public static int ConeTelegraph(Vector2 origin, float dirDeg, float arcDeg = 90, float rangePx = 28, float seconds = 0.75f, Transform follow = null, Color? color = null, int order = 5)
        {
            var s = R.Alloc(TellFXRunner.Kind.Cone, order, color ?? TellRed); if (s == null) return -1;
            s.pos = origin; s.follow = follow; s.hasFollow = follow; s.a0 = dirDeg; s.a1 = Mathf.Clamp(arcDeg, 10, 300) * 0.5f; s.rad = Mathf.Clamp(rangePx, 8, 96);
            s.tell = Tell(seconds); s.life = s.tell + 0.3f; s.color2 = TellHot; return R.Handle(s);
        }
        internal static bool TickCone(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 o = Org(s); float rr = s.rad * U, fd = fade(s); const int N = 5;
            float lo = s.a0 - s.a1, hi = s.a0 + s.a1, span = (hi - lo) / N, tn = Mathf.Tan(span * 0.5f * Mathf.Deg2Rad);
            if (s.t < s.tell)
            {
                float k = s.t / s.tell, ck = ClockK(k), fr = rr * Mathf.Max(0.1f, ck); bool bl = Blink(k);
                Color rim = A(bl ? Color.white : s.color, fd), dark = A(Outline, 0.75f * fd);
                r.Seg(s, o, o + Dir(lo - 3) * (rr + U), dark, 1, false, -3); r.Seg(s, o, o + Dir(hi + 3) * (rr + U), dark, 1, false, -3);
                r.Seg(s, o, o + Dir(lo) * rr, rim, 1, false); r.Seg(s, o, o + Dir(hi) * rr, rim, 1, false);
                for (int i = 0; i < N; i++)
                {
                    float u0 = lo + span * i, u1 = u0 + span, um = u0 + span * 0.5f;
                    float a1 = 0.72f * rr <= fr ? 0.30f : 0.13f, a2 = 0.3f * rr <= fr ? 0.30f : 0.13f;
                    r.RBox(s, o + Dir(um) * rr * 0.72f, rr * 0.56f * HellFX.PPU, 2 * rr * 0.72f * tn * HellFX.PPU, um, A(Wash, a1 * fd), false, -2);
                    r.RBox(s, o + Dir(um) * rr * 0.3f, rr * 0.28f * HellFX.PPU, 2 * rr * 0.3f * tn * HellFX.PPU, um, A(Wash, a2 * fd), false, -2);
                    r.Seg(s, o + Dir(u0) * (rr + U), o + Dir(u1) * (rr + U), dark, 1, false, -3);
                    r.Seg(s, o + Dir(u0) * rr, o + Dir(u1) * rr, rim, 1, false);
                    if (ck < 1) r.Seg(s, o + Dir(u0) * fr, o + Dir(u1) * fr, A(Clock, fd), 1, false, 2);
                }
                return true;
            }
            if (!s.flag) { s.flag = true; Fire(s, 0); Shake(1f, 0.08f); Kick(Dir(s.a0) * 2f); }
            float ft = s.t - s.tell; TellPop(r, s, o, ft);
            if (ft < 0.067f)
            {
                Color c = ft < 0.034f ? A(Color.white, 0.85f * Flash) : A(Clock, 0.85f * Flash); float band = rr * 0.4f;
                for (int i = 0; i < N; i++) { float u0 = lo + span * i, u1 = u0 + span; r.Seg(s, o + Dir(u0) * (rr - band * 0.5f), o + Dir(u1) * (rr - band * 0.5f), c, band * HellFX.PPU, true, 1); }
            }
            else
            {
                Color c = A(s.color, (1 - (ft - 0.067f) / 0.23f) * fd);
                for (int i = 0; i < N; i++) { float u0 = lo + span * i, u1 = u0 + span; r.Seg(s, o + Dir(u0) * rr, o + Dir(u1) * rr, c, 1, false, 1); }
            }
            return s.t < s.life;
        }

        // ---------------- 103. Stomp rings (Ox-Head) ----------------
        /// <summary>Tell: dark + crimson rim at <paramref name="maxRadiusPx"/> (the reach) and a gold clock ring that TIGHTENS from the rim to
        /// the centre - when it closes, the stomp lands; rim blinks white twice. Then a 2-frame centre flash, short dark ground cracks (1 s)
        /// and <paramref name="rings"/> (1-4) OPAQUE shock rings (white leading edge over a gold band, dust trailing, dust puffs riding them)
        /// launch every <paramref name="ringGap"/> s and reach the rim in 0.45 s - dodge through the gaps. OnTellFire(handle, ringIndex)
        /// per ring. <paramref name="squash"/> 0.6 = ground ellipse. Returns a handle or -1.</summary>
        public static int StompRings(Vector2 pos, int rings = 3, float maxRadiusPx = 40, float tellSeconds = 0.75f, float ringGap = 0.25f, float squash = 0.6f, Color? color = null, int order = 6)
        {
            var s = R.Alloc(TellFXRunner.Kind.Stomp, order, color ?? TellRed); if (s == null) return -1;
            s.pos = pos; s.n = Mathf.Clamp(rings, 1, 4); s.rad = Mathf.Clamp(maxRadiusPx, 8, 96); s.tell = Tell(tellSeconds); s.spd = Mathf.Clamp(ringGap, 0.08f, 1f);
            s.squash = Mathf.Clamp(squash, 0.3f, 1f); s.color2 = Dust; s.life = s.tell + Mathf.Max((s.n - 1) * s.spd + 0.45f, 1.0f); return R.Handle(s);
        }
        internal static bool TickStomp(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 o = s.pos; float fd = fade(s);
            if (s.t < s.tell)
            {
                float k = s.t / s.tell; bool bl = Blink(k);
                r.Ring(s, o, s.rad, s.squash, 10, A(Outline, 0.75f * fd), 3, false, -1);
                r.Ring(s, o, s.rad, s.squash, 10, A(bl ? Color.white : s.color, fd), 1, false, 0);
                float cr = s.rad * (1 - Mathf.Clamp01(k / 0.95f));
                if (cr > 2) r.Ring(s, o, cr, s.squash, cr < 12 ? 8 : 10, A(Clock, fd), 1, false, 1);
                r.Put(s, TellSprites.Star3, o, A(bl ? Color.white : (Color)TellHot, fd), false, 1);
                return true;
            }
            float ht = s.t - s.tell; TellPop(r, s, o, ht);
            if (s.acc < 0.5f) { s.acc = 1; Kick(Vector2.down * 2.5f); Freeze(0.05f); }
            if (ht < 0.05f) { var im = r.Put(s, TellSprites.Impact, o, A(ht < 0.017f ? (Color)StunYellow : (Color)PunishGold, Flash * 0.8f), true, 3); if (im) im.transform.localScale = Vector3.one * (ht < 0.034f ? 1f : 0.7f); }
            if (ht < 0.15f) { float k = ht / 0.15f; r.Ring(s, o, 4 + k * 14, s.squash, 10, A((Color)PunishGold, (1 - k) * Flash * 0.7f), 1, true, 3); } // fast shockwave pulse
            if (ht < 1f) // ground cracks: 5 short dark polylines, hold then step out
            {
                Color cc = A(Outline, (ht < 0.8f ? 0.9f : 0.45f) * fd); float grow = Mathf.Clamp01(ht / 0.08f);
                for (int c = 0; c < 5; c++)
                {
                    float ang = c * 72 + H01(s.seed, c, 7) * 40, len = (5 + H01(s.seed, c, 8) * 6) * grow;
                    Vector2 d1 = Vector2.Scale(Dir(ang), new Vector2(1, s.squash)), d2 = Vector2.Scale(Dir(ang + 25 - H01(s.seed, c, 9) * 50), new Vector2(1, s.squash));
                    Vector2 p0 = o + d1 * 3 * U, p1 = p0 + d1 * len * 0.6f * U; r.Seg(s, p0, p1, cc, 1, false, -1); r.Seg(s, p1, p1 + d2 * len * 0.4f * U, cc, 1, false, -1);
                }
            }
            for (int i = 0; i < s.n; i++)
            {
                float st = ht - i * s.spd; if (st < 0) continue;
                if (s.fired <= i) { s.fired = i + 1; Fire(s, i); Shake(1.5f, 0.1f); }
                float k = st / 0.45f; if (k >= 1) continue;
                float rp = Mathf.Max(3, s.rad * (1 - (1 - k) * (1 - k))), al = (k < 0.75f ? 1f : 0.5f) * fd; int segs = rp < 12 ? 8 : 12;
                r.Ring(s, o, rp - 1.5f, s.squash, segs, A(DustShade, 0.7f * al), 2, false, -1);   // trailing dust band
                r.Ring(s, o, rp, s.squash, segs, A(k < 0.35f ? Color.white : (Color)TellHot, al), 2, false, 0); // opaque leading edge
                if (((frame >> 1) + i) % 3 == 0) for (int q = 0; q < 4; q++)
                {
                    float ang = (q * 90 + 45 + i * 20) * Mathf.Deg2Rad; var p = o + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * s.squash) * (rp - 2) * U;
                    r.Put(s, k < 0.4f ? TellSprites.PuffM : TellSprites.PuffS, p, A(Dust, al), false, 1);
                }
            }
            if (ht < 0.6f) for (int q = 0; q < 6; q++) // pebbles hop out
            {
                float ang = q * 60 + H01(s.seed, q, 11) * 40, kk = ht / 0.6f, z = Mathf.Sin(kk * Mathf.PI) * (4 + H01(s.seed, q, 12) * 5);
                var p = o + Vector2.Scale(Dir(ang), new Vector2(1, s.squash)) * (4 + kk * (10 + H01(s.seed, q, 13) * 10)) * U + Vector2.up * z * U;
                r.Put(s, TellSprites.Pebble, p, A(DustShade, fd), false, 2);
            }
            return s.t < s.life;
        }

        // ---------------- 104. Stun stars loop (Ox-Head wall-hit stun; any stun) ----------------
        /// <summary>Dizzy loop above <paramref name="target"/> (<paramref name="headYPx"/> over its pivot). v1.8.1 juice:
        /// 2-frame impact frames on the target sprite (white squashed silhouette, then black), 9x9 impact star, 6 bouncing pebbles;
        /// then 3 squash-and-stretch stars + 2 flapping birdies orbiting a tilted ellipse (front ones brighter), cracked-horn sparks
        /// every ~0.45 s at both horn tips and a 1px side-to-side wobble of the target sprite (only if the SpriteRenderer is on a CHILD
        /// of <paramref name="target"/>, restored on end; StunWobble = false to disable). <paramref name="seconds"/> &lt;= 0 loops until Stop.
        /// Ends with the target. Not counted toward StatusCap. Returns a handle or -1.</summary>
        public static int StunStars(Transform target, float seconds = 3f, float headYPx = 12, float radiusPx = 6, Color? color = null, int order = 80)
        {
            if (!target) return -1;
            var s = R.Alloc(TellFXRunner.Kind.Stun, order, color ?? StunYellow); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.h2 = headYPx; s.rad = Mathf.Clamp(radiusPx, 3, 24); s.life = seconds > 0 ? seconds : float.PositiveInfinity;
            s.tsr = target.GetComponentInChildren<SpriteRenderer>();
            if (StunWobble && s.tsr && s.tsr.transform != target) { var lp = s.tsr.transform.localPosition; s.pos2 = new Vector2(lp.x, lp.y); s.flag = true; }
            return R.Handle(s);
        }
        internal static bool TickStun(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 c = Org(s) + Vector2.up * s.h2 * U; float fd = fade(s);
            if (ImpactFrames && s.t < 0.034f && Shaped(s.tsr) && ImpactFrameFree(s)) SilhouetteOver(r, s, s.tsr, SolidWB(false), 1.03f, 1.03f, Vector2.zero);
            if (s.t < 0.1f) { var im = r.Put(s, TellSprites.Impact, c, A(Color.white, Flash), true, 2); if (im) im.transform.localScale = Vector3.one * (s.t < 0.05f ? 1f : 0.7f); }
            if (s.t < 0.6f) for (int q = 0; q < 6; q++)
            {
                float vx = (H01(s.seed, q, 1) - 0.5f) * 40, vy = 25 + H01(s.seed, q, 2) * 30;
                r.Put(s, TellSprites.Pebble, c + new Vector2(vx * s.t, vy * s.t - 90 * s.t * s.t) * U, A(DustShade, fd), false, 2);
            }
            if (s.flag && s.tsr) // wobble: 1px sway, 2.5 Hz, eases out over the first second then holds a lazy sway
            {
                float amp = s.t < 1f ? 1.5f : 1f, ox = Mathf.Round(Mathf.Sin(s.t * Mathf.PI * 5f) * amp);
                var tr = s.tsr.transform; tr.localPosition = new Vector3(s.pos2.x + ox * U / Mathf.Max(1e-4f, tr.parent ? tr.parent.lossyScale.x : 1), s.pos2.y, tr.localPosition.z);
            }
            float tilt = Mathf.Sin(s.t * 3f) * 0.25f;
            for (int i = 0; i < 5; i++) // 3 stars + 2 birdies
            {
                bool bird = i >= 3; float a = (s.t / (bird ? 0.9f : 0.7f) * (bird ? -1 : 1) + i / (bird ? 2f : 3f) + (bird ? 0.25f : 0)) * Mathf.PI * 2;
                float rr = s.rad * (bird ? 1.25f : 1f), sn = Mathf.Sin(a);
                var p = c + new Vector2(Mathf.Cos(a) * rr, sn * rr * 0.35f + Mathf.Cos(a) * rr * tilt + (bird ? 2 : 0)) * U;
                bool front = sn < 0;
                if (bird) { var sr = r.Put(s, TellSprites.Birdie[((frame >> 2) + i) & 1], p, A(Color.white, fd * (front ? 1 : 0.6f)), false, front ? 1 : -1); if (sr) sr.flipX = Mathf.Cos(a) > 0; continue; }
                bool tw = ((frame / 4 + i) % 3) == 0;
                var st = r.Put(s, tw ? TellSprites.Star3 : TellSprites.StunStar, p, A(tw ? Color.white : s.color, fd * (front ? 1 : 0.65f)), false, front ? 1 : -1);
                if (st) { float q = Mathf.Sin(a * 2); st.transform.localScale = new Vector3(1 + 0.3f * q, 1 - 0.3f * q, 1); } // squash & stretch through the orbit
            }
            float cp = s.t % 0.45f; // cracked-horn sparks
            if (s.t > 0.2f && cp < 0.12f)
            {
                int cyc = (int)(s.t / 0.45f); float k = cp / 0.12f;
                for (int h = -1; h <= 1; h += 2)
                {
                    Vector2 horn = c + new Vector2(h * s.rad * 0.9f, -2) * U;
                    if (k < 0.3f) r.Put(s, TellSprites.Star3, horn, A(Color.white, fd), true, 3);
                    for (int q = 0; q < 3; q++)
                    {
                        float ang = (h > 0 ? 20 : 160) + (H01(s.seed, cyc * 7 + q, h + 3) - 0.5f) * 90, d0 = 1 + k * 6;
                        Vector2 dd = Dir(ang); r.Seg(s, horn + dd * d0 * U, horn + dd * (d0 + 2 * (1 - k) + 1) * U, A(k < 0.5f ? (Color)StunYellow : (Color)TellHot, fd), 1, true, 3);
                    }
                }
            }
            return s.t < s.life;
        }

        // ---------------- v1.8.1 juice: wall impact, speed lines, boss hit reaction ----------------
        /// <summary>One-shot wall / hard-landing impact at <paramref name="pos"/>, <paramref name="normal"/> pointing away from the wall.
        /// Impact frames on <paramref name="target"/>'s sprite (white squashed silhouette 2 frames, black 2 frames), 2x impact star,
        /// a fast squashed shock ring, 8 sparks fanned along the normal, 8 debris pebbles + 3 dust puffs, camera kick INTO the wall,
        /// shake and a 0.07 s hit-stop (JuiceHitStop). <paramref name="strength"/> 0.5-2 scales kick / shake / spark length.
        /// Pair with StunStars for the Ox-Head wall-hit stun. Returns a handle or -1.</summary>
        public static int WallImpact(Vector2 pos, Vector2 normal, Transform target = null, float strength = 1f, int order = 82)
        {
            var s = R.Alloc(TellFXRunner.Kind.Impact, order, TellHot); if (s == null) return -1;
            s.pos = pos; s.pos2 = normal.sqrMagnitude > 1e-6f ? normal.normalized : Vector2.up; s.spd = Mathf.Clamp(strength, 0.5f, 2f); s.life = 0.75f;
            if (target) s.tsr = target.GetComponentInChildren<SpriteRenderer>();
            Kick(-s.pos2 * 3f * s.spd); Shake(3f * s.spd, 0.25f); Freeze(0.07f); return R.Handle(s);
        }
        internal static bool TickImpact(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 o = s.pos, nrm = s.pos2; float nd = Mathf.Atan2(nrm.y, nrm.x) * Mathf.Rad2Deg, t = s.t, fd = fade(s);
            if (ImpactFrames && t < 0.034f && Shaped(s.tsr) && ImpactFrameFree(s)) SilhouetteOver(r, s, s.tsr, SolidWB(false), 1 + 0.04f * Mathf.Abs(nrm.y), 1 + 0.04f * Mathf.Abs(nrm.x), nrm * -1);
            if (t < 0.1f) { var im = r.Put(s, TellSprites.Impact, o, A(Color.white, Flash), true, 3); if (im) im.transform.localScale = Vector3.one * (t < 0.05f ? 1.2f : 0.8f); }
            if (t < 0.18f) { float k = t / 0.18f; r.Ring(s, o, 3 + Drag(k) * 18 * s.spd, 0.6f, 10, A(k < 0.4f ? Color.white : (Color)TellHot, (1 - k) * fd), 1, true, 2); }
            if (t < 0.25f) for (int q = 0; q < 8; q++)
            {
                float k = t / 0.25f, ang = nd + (H01(s.seed, q, 1) - 0.5f) * 150, sp = (40 + H01(s.seed, q, 2) * 50) * s.spd;
                Vector2 dd = Dir(ang), p = o + dd * sp * Drag(k) * 0.25f * U; float len = Mathf.Max(1, (1 - k) * 5);
                r.Seg(s, p, p + dd * len * U, A(k < 0.3f ? Color.white : k < 0.6f ? (Color)StunYellow : (Color)Orange, fd), 1, true, 3);
            }
            for (int q = 0; q < 8; q++)
            {
                float lt = 0.45f + H01(s.seed, q, 3) * 0.3f; if (t > lt) continue;
                Vector2 v = Dir(nd + (H01(s.seed, q, 4) - 0.5f) * 120) * (30 + H01(s.seed, q, 5) * 40);
                r.Put(s, TellSprites.Pebble, o + new Vector2(v.x * t, v.y * t + 20 * t - 110 * t * t) * U, A(q % 3 == 0 ? (Color)Outline : (Color)DustShade, fd), false, 1);
            }
            if (t < 0.45f) for (int q = 0; q < 3; q++) { float k = t / 0.45f; r.Put(s, PuffAt(k), o + (nrm * (3 + Drag(k) * 8) + Dir(nd + (q - 1) * 50) * Drag(k) * 6) * U, A(Dust, fd), false, 0); }
            return t < s.life;
        }

        /// <summary>Speed lines behind a moving <paramref name="target"/> (Ox-Head charge, calves). Only draws while it moves faster than
        /// <paramref name="minSpeedPx"/> px/s: 5 white/gold 1px streaks trailing it, length grows with speed (max <paramref name="lengthPx"/>),
        /// lateral spread <paramref name="widthPx"/>. Local to the target - never full-screen. <paramref name="seconds"/> &lt;= 0 loops until Stop.</summary>
        public static int SpeedLines(Transform target, float seconds = 1f, float lengthPx = 16, float widthPx = 18, float minSpeedPx = 60, int order = 39)
        {
            if (!target) return -1;
            var s = R.Alloc(TellFXRunner.Kind.Speed, order, Color.white); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.last = target.position; s.a0 = 0; s.rad = Mathf.Clamp(lengthPx, 4, 48); s.w = Mathf.Clamp(widthPx, 4, 48); s.spd = Mathf.Max(1, minSpeedPx);
            s.life = seconds > 0 ? seconds : float.PositiveInfinity; return R.Handle(s);
        }
        internal static bool TickSpeed(TellFXRunner r, TellFXRunner.Inst s, int frame, float dt)
        {
            Vector2 o = Org(s), v = dt > 1e-5f ? (o - s.last) / dt * HellFX.PPU : Vector2.zero; s.last = o;
            float sp = v.magnitude; if (!TellFXRunner.Fin(sp)) sp = 0; if (!TellFXRunner.Fin(s.a0)) s.a0 = 0; s.a0 = Mathf.Lerp(s.a0, sp, 0.5f); if (s.a0 < s.spd || sp < 1e-3f) return s.t < s.life;
            Vector2 f = v / sp, n = new Vector2(-f.y, f.x); float k = Mathf.Clamp01((s.a0 - s.spd) / (s.spd * 2)), fd = fade(s);
            for (int i = 0; i < 5; i++)
            {
                int cyc = frame / 2 + i * 3; float lat = (H01(s.seed, cyc, i) - 0.5f) * s.w, back = 4 + H01(s.seed, cyc, i + 9) * 8, len = (0.4f + 0.6f * k) * s.rad * (0.6f + 0.4f * H01(s.seed, i, 2));
                Vector2 p0 = o + (n * lat - f * back) * U; r.Seg(s, p0, p0 - f * len * U, A(i == 0 ? (Color)StunYellow : Color.white, (0.55f + 0.4f * k) * fd), 1, true, 0);
            }
            return s.t < s.life;
        }

        static int _hitAlive;
        /// <summary>Boss / elite hit reaction (NOT for horde fodder): 2-frame white silhouette squashed along <paramref name="hitDir"/> and
        /// nudged 1px with it, 3 sparks (5 + star on <paramref name="crit"/>, plus 1px camera kick). Max 6 alive at once - extra calls return -1.</summary>
        public static int BossHit(Transform target, Vector2 hitDir, Vector2 hitPoint, bool crit = false, int order = 83)
        {
            if (!target || _hitAlive >= 6) return -1;
            var s = R.Alloc(TellFXRunner.Kind.Hit, order, crit ? (Color)StunYellow : Color.white); if (s == null) return -1;
            s.tsr = target.GetComponentInChildren<SpriteRenderer>(); s.pos = hitPoint; s.pos2 = hitDir.sqrMagnitude > 1e-6f ? hitDir.normalized : Vector2.right; s.flag = crit; s.life = 0.16f; _hitAlive++;
            if (crit) Kick(s.pos2 * 1f); return R.Handle(s);
        }
        internal static bool TickHit(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 d = s.pos2; float t = s.t;
            if (t < 0.034f && Shaped(s.tsr)) SilhouetteOver(r, s, s.tsr, SolidWB(false), 1 + 0.03f * Mathf.Abs(d.y), 1 + 0.03f * Mathf.Abs(d.x), d);
            if (s.flag && t < 0.067f) r.Put(s, TellSprites.Glint, s.pos, Color.white, true, 2);
            int n = s.flag ? 5 : 3; float k = t / s.life, nd = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            for (int q = 0; q < n; q++)
            {
                Vector2 dd = Dir(nd + 180 + (H01(s.seed, q, 1) - 0.5f) * 110); Vector2 p = s.pos + dd * (1 + Drag(k) * 7) * U;
                r.Seg(s, p, p + dd * Mathf.Max(1, 3 * (1 - k)) * U, A(k < 0.4f ? Color.white : s.flag ? (Color)StunYellow : (Color)TellHot, 1), 1, true, 1);
            }
            bool alive = t < s.life; if (!alive) _hitAlive = Mathf.Max(0, _hitAlive - 1); return alive;
        }

        // ---------------- 105. Punish window (boss template phase 3 / stun damage window) ----------------
        /// <summary>"Hit it now" read, v1.8: a blinking 1px gold silhouette outline around the target's SpriteRenderer (4 solid copies, one shared
        /// material) plus 3 countdown pips under it that go out one per second (last pip blinks before it drops). Targets without a
        /// SpriteRenderer (or no PixelFX_Uber) fall back to a gold ground ellipse. <paramref name="radiusPx"/> = pip / fallback offset,
        /// <paramref name="centerYPx"/> = ellipse centre. <paramref name="seconds"/> &lt;= 0 loops until Stop (pips hidden).
        /// Not counted toward StatusCap. Returns a handle or -1.</summary>
        public static int PunishGlow(Transform target, float seconds = 3f, float radiusPx = 14, float centerYPx = 0, Color? color = null, int order = 4)
        {
            if (!target) return -1;
            var s = R.Alloc(TellFXRunner.Kind.Punish, order, color ?? PunishGold); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.rad = Mathf.Clamp(radiusPx, 6, 48); s.h2 = centerYPx; s.life = seconds > 0 ? seconds : float.PositiveInfinity;
            s.tsr = target.GetComponentInChildren<SpriteRenderer>(); return R.Handle(s);
        }
        internal static bool TickPunish(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 o = Org(s), c = o + Vector2.up * s.h2 * U; float fd = fade(s); bool on = ((int)(s.t * 8) & 1) == 0;
            var solid = Solid(true);
            if (Shaped(s.tsr) && solid != null) Outline1(r, s, s.tsr, solid, (on ? 1f : 0.35f) * fd);
            else r.Ring(s, c, s.rad, 0.6f, 14, A(s.color, (on ? 0.9f : 0.4f) * fd), 1, false, 0);
            if (!float.IsInfinity(s.life))
            {
                float rem = s.life - s.t; int left = Mathf.CeilToInt(rem * 3f / Mathf.Max(0.01f, s.life)); // 3 pips across the window
                float seg = s.life / 3f, inSeg = rem - (left - 1) * seg;
                Vector2 pc = o + Vector2.down * 3 * U;
                for (int i = 0; i < 3; i++)
                {
                    bool lit = i < left, blink = lit && i == left - 1 && inSeg < 0.3f && ((frame >> 2) & 1) == 0;
                    var p = pc + Vector2.right * (i - 1) * 6 * U;
                    r.Put(s, TellSprites.Star3, p, lit && !blink ? A(s.color, fd) : A(Outline, 0.85f * fd), false, 2);
                    if (lit && !blink) r.Put(s, TellSprites.Pixel, p, A(StunYellow, fd), false, 3);
                }
            }
            return s.t < s.life;
        }

        // ---------------- 106. Dust puff (one-shot) ----------------
        /// <summary>Ground dust: <paramref name="puffs"/> (1-8) two-tone puffs roll out to <paramref name="radiusPx"/> with drag and drift up,
        /// stepping L -> M -> S (v1.8: no alpha fade - the smallest step simply ends), 0.45 s. <paramref name="dirDeg"/> biases them
        /// (NaN = all around). Returns false if the pool is full.</summary>
        public static bool DustPuff(Vector2 pos, int puffs = 5, float radiusPx = 8, float dirDeg = float.NaN, Color? color = null, int order = 7)
        {
            var s = R.Alloc(TellFXRunner.Kind.Dust, order, color ?? Dust); if (s == null) return false;
            s.pos = pos; s.n = Mathf.Clamp(puffs, 1, 8); s.rad = Mathf.Clamp(radiusPx, 2, 32); s.a0 = dirDeg; s.life = 0.45f; return true;
        }
        static Sprite PuffAt(float k) => k < 0.4f ? TellSprites.PuffL : k < 0.75f ? TellSprites.PuffM : TellSprites.PuffS;
        static float Drag(float k) => 1 - (1 - k) * (1 - k) * (1 - k);
        internal static bool TickDust(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            float k = s.t / s.life, e = Drag(k);
            for (int i = 0; i < s.n; i++)
            {
                float ang = float.IsNaN(s.a0) ? i * 360f / s.n + H01(s.seed, i, 1) * 30 : s.a0 + (H01(s.seed, i, 1) - 0.5f) * 100;
                float kk = Mathf.Clamp01(k * (0.85f + 0.3f * H01(s.seed, i, 4)));
                var p = s.pos + Vector2.Scale(Dir(ang), new Vector2(1, 0.6f)) * s.rad * (0.3f + 0.7f * e) * U + new Vector2(0, e * 4) * U;
                if (kk < 1) r.Put(s, PuffAt(kk), p, A(s.color, fade(s)), false, 0);
            }
            return s.t < s.life;
        }

        // ---------------- 107. Dust trail (follows a charger) ----------------
        /// <summary>Kicks a small two-tone dust puff behind <paramref name="target"/> every <paramref name="spacingPx"/> it moves (feet at
        /// <paramref name="feetYPx"/>); each puff steps M -> S and rises over 0.4 s, max 10 alive. <paramref name="seconds"/> &lt;= 0 loops until Stop.
        /// Returns a handle or -1.</summary>
        public static int DustTrail(Transform target, float seconds = 1f, float spacingPx = 5, float feetYPx = -6, Color? color = null, int order = 7)
        {
            if (!target) return -1;
            var s = R.Alloc(TellFXRunner.Kind.DustTrail, order, color ?? Dust); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.w = Mathf.Clamp(spacingPx, 2, 24); s.h2 = feetYPx; s.last = target.position; s.life = seconds > 0 ? seconds : float.PositiveInfinity;
            return R.Handle(s);
        }
        static void Drop(TellFXRunner.Inst s, Vector2 p) { s.trail[s.trailHead] = new Vector3(p.x, p.y, s.t); s.trailHead = (s.trailHead + 1) % TellFXRunner.MaxTrail; }
        internal static bool TickDustTrail(TellFXRunner r, TellFXRunner.Inst s, int frame, float dt)
        {
            if (s.hasFollow && !s.stopping)
            {
                Vector2 o = Org(s) + Vector2.up * s.h2 * U;
                if ((o - s.last).sqrMagnitude >= s.w * s.w * U * U) { Drop(s, o + new Vector2((H01(s.seed, frame, 1) - 0.5f) * 3, 0) * U); s.last = o; }
            }
            bool any = false;
            for (int q = 0; q < TellFXRunner.MaxTrail; q++)
            {
                var d = s.trail[q]; float age = s.t - d.z; if (age < 0 || age > 0.4f) continue; any = true;
                float k = age / 0.4f, e = Drag(k);
                r.Put(s, k < 0.5f ? TellSprites.PuffM : TellSprites.PuffS, new Vector2(d.x + (H01(s.seed, q, 5) - 0.5f) * 3 * e * U, d.y + e * 4 * U), A(s.color, fade(s)), false, 0);
            }
            return s.t < s.life || (any && s.t < s.life + 0.4f);
        }

        // ---------------- 108. Stampede lane (Level 10 Ox Pit hazard "Stampede lanes"; Ox-Head "Call the Herd") ----------------
        /// <summary>v1.8 look - built to stay readable with 3+ lanes on screen: NO lane fill. During <paramref name="tellSeconds"/>: blinking
        /// ">>>" chevrons at the lane's entry point (clamped into the main camera view), dashed 1px crimson borders marching in the run
        /// direction, pebbles hopping along the lane (ground tremor); chevrons + borders flash white in the last 15%. Then
        /// <paramref name="oxen"/> (1-4) oxen run from <paramref name="from"/> to <paramref name="to"/> at <paramref name="speedPx"/> px/s,
        /// 0.18 s apart, each with a dust plume; the lane marks are gone. <paramref name="oxFrames"/> = your run frames (null = built-in
        /// placeholder, tinted <paramref name="color"/>). OnTellFire(handle, 0) when the herd starts; optional OnStampedeHit once per collider.</summary>
        public static int Stampede(Vector2 from, Vector2 to, float widthPx = 16, float tellSeconds = 1f, int oxen = 3, float speedPx = 160, Sprite[] oxFrames = null, Color? color = null, int order = 40)
        {
            var s = R.Alloc(TellFXRunner.Kind.Stampede, order, TellRed); if (s == null) return -1; Color oxTint = color ?? (oxFrames != null ? Color.white : OxBrown);
            s.pos = from; s.pos2 = to; s.w = Mathf.Clamp(widthPx, 6, 48); s.tell = Tell(tellSeconds); s.n = Mathf.Clamp(oxen, 1, 4); s.spd = Mathf.Clamp(speedPx, 30, 600);
            s.frames = oxFrames != null && oxFrames.Length > 0 ? oxFrames : TellSprites.Ox; s.color2 = TellHot; s.a0 = oxTint.r; s.a1 = oxTint.g; s.h2 = oxTint.b;
            float L = (to - from).magnitude * HellFX.PPU; s.life = s.tell + (s.n - 1) * 0.18f + (L + 16) / s.spd + 0.4f;
            return R.Handle(s);
        }
        /// <summary>Distance (world) along the lane where it first enters the main camera view (0 if no ortho camera or never visible).</summary>
        static float EntryInView(Vector2 a, Vector2 f, float L)
        {
            var cam = Camera.main; if (!cam || !cam.orthographic) return 0;
            Vector2 cp = cam.transform.position; float hh = cam.orthographicSize, hw = hh * cam.aspect, m = 4 * U;
            for (int i = 0; i <= 48; i++) { float d = L * i / 48f; Vector2 p = a + f * d; if (Mathf.Abs(p.x - cp.x) < hw - m && Mathf.Abs(p.y - cp.y) < hh - m) return d; }
            return 0;
        }
        internal static bool TickStampede(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 a = s.pos, d = s.pos2 - a; float L = d.magnitude; if (L < 1e-4f) return false;
            Vector2 f = d / L, n = new Vector2(-f.y, f.x); float hw = s.w * 0.5f * U, deg = Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg, Lpx = L * HellFX.PPU;
            float fd = fade(s);
            if (s.t < s.tell)
            {
                float k = s.t / s.tell; bool hot = k >= BlinkFrom && ((frame >> 2) & 1) == 0;
                Color bc = A(hot ? Color.white : s.color, 0.9f * fd), dk = A(Outline, 0.5f * fd);
                float dash = Mathf.Max(3, Lpx / 16f), ph = (s.t * 40) % (dash * 2);
                for (int i = -1; i < 8; i++) // 8 marching dashes per border
                {
                    float d0 = Mathf.Max(0, i * dash * 2 + ph), d1 = Mathf.Min(Lpx, d0 + dash); if (d1 <= d0) continue;
                    for (int sd = -1; sd <= 1; sd += 2)
                    {
                        Vector2 off = n * sd * hw; r.Seg(s, a + f * d0 * U + off, a + f * d1 * U + off, bc, 1, false, -1);
                        if (i % 2 == 0) r.Seg(s, a + f * d0 * U + off + n * sd * U, a + f * d1 * U + off + n * sd * U, dk, 1, false, -2);
                    }
                }
                float ent = EntryInView(a, f, L);
                if (((int)(k * 8) & 1) == 0 || k >= BlinkFrom)
                    for (int i = 0; i < 3; i++) { var sr = r.Put(s, TellSprites.Chevron, a + f * (ent + (3 + i * 5) * U), hot ? A(Color.white, fd) : A(i == 2 ? s.color2 : s.color, fd), false, 2); if (sr) sr.transform.rotation = Quaternion.Euler(0, 0, deg); }
                for (int q = 0; q < 5; q++) // tremor pebbles
                {
                    float dp = ((H01(s.seed, q, 3) * Lpx) + s.t * 60) % Lpx, hop = ((frame / 3 + q) % 3) == 0 ? 1 : 0;
                    r.Put(s, TellSprites.Pebble, a + f * dp * U + n * (H01(s.seed, q, 4) - 0.5f) * s.w * 0.7f * U + Vector2.up * hop * U, A(DustShade, fd), false, 0);
                }
                return true;
            }
            if (!s.flag) { s.flag = true; Fire(s, 0); Shake(0.8f, Mathf.Min(1.2f, s.life - s.t)); }
            float rt = s.t - s.tell; bool flip = f.x < 0;
            for (int i = 0; i < s.n; i++)
            {
                float ot = rt - i * 0.18f; if (ot < 0) continue;
                float dist = ot * s.spd - 8; if (dist > Lpx + 8) continue;
                float lat = (H01(s.seed, i, 1) - 0.5f) * s.w * 0.6f, bob = (((int)(ot * 12)) & 1) == 0 ? 0 : 1;
                Vector2 p = a + f * dist * U + n * lat * U + Vector2.up * bob * U;
                var spr = s.frames[((int)(ot * 12.5f)) % s.frames.Length];
                var sr = r.Put(s, spr, p, new Color(s.a0, s.a1, s.h2, fd), false, 2); if (sr) sr.flipX = flip;
                for (int q = 0; q < 4; q++) // dust plume behind: puffs step L -> S as they fall back
                {
                    float back = 6 + q * 4 + H01(s.seed, i, q + frame / 4) * 2; if (dist - back < 0) continue;
                    float kq = q / 4f + ((ot * 8) % 1) / 4f;
                    r.Put(s, PuffAt(kq), a + f * (dist - back) * U + n * (lat + (H01(s.seed + frame / 3, i, q) - 0.5f) * 4) * U + Vector2.down * 3 * U + Vector2.up * kq * 4 * U, A(Dust, fd), false, 1);
                }
                if (OnStampedeHit != null && s.hits < TellFXRunner.MaxHits && dist >= 0 && dist <= Lpx)
                {
                    int cnt = r.Overlap(p, 6 * U);
                    for (int q = 0; q < cnt; q++)
                    {
                        var c = r.Buf(q); if (!c || !c.enabled) continue; int id = c.GetInstanceID(); bool seen = false;
                        for (int h = 0; h < s.hits; h++) if (s.hitIds[h] == id) { seen = true; break; }
                        if (seen || s.hits >= TellFXRunner.MaxHits) continue; s.hitIds[s.hits++] = id; OnStampedeHit(c);
                    }
                }
            }
            return s.t < s.life;
        }

        // ---------------- 109. Bloodrage fire trail (Ox-Head phase 3) ----------------
        /// <summary>v1.8 look: the enraged boss gets a flickering 1px red silhouette outline and snorts steam puffs from the front of its
        /// sprite every ~0.6 s; behind it, every <paramref name="spacingPx"/> moved, a burning hoof-print (max 10 alive, each
        /// <paramref name="patchSeconds"/>, 0.4-2 s): dark scorch, 1-3 flames on a 4-frame flipbook with independent phases (ignite small ->
        /// burn -> die down small -> smoke wisp), an ember. Visual only (burning-ground damage is game code).
        /// <paramref name="seconds"/> &lt;= 0 loops until Stop. Returns a handle or -1.</summary>
        public static int BloodrageTrail(Transform target, float seconds = 0f, float spacingPx = 7, float patchSeconds = 1.2f, float feetYPx = -6, Color? color = null, int order = 8)
        {
            if (!target) return -1;
            var s = R.Alloc(TellFXRunner.Kind.Bloodrage, order, color ?? RageRed); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.w = Mathf.Clamp(spacingPx, 3, 24); s.rad = Mathf.Clamp(patchSeconds, 0.4f, 2f); s.h2 = feetYPx; s.last = target.position;
            s.life = seconds > 0 ? seconds : float.PositiveInfinity; s.color2 = Orange; s.tsr = target.GetComponentInChildren<SpriteRenderer>(); return R.Handle(s);
        }
        internal static bool TickBloodrage(TellFXRunner r, TellFXRunner.Inst s, int frame, float dt)
        {
            float fd = fade(s);
            if (s.hasFollow && !s.stopping)
            {
                Vector2 o = Org(s) + Vector2.up * s.h2 * U;
                if ((o - s.last).sqrMagnitude >= s.w * s.w * U * U) { Drop(s, o + new Vector2((H01(s.seed, frame, 2) - 0.5f) * 4, 0) * U); s.last = o; }
                if (Shaped(s.tsr))
                {
                    if ((frame % 3) != 0) Outline1(r, s, s.tsr, Solid(false), fd);                                   // rage outline flicker
                    float sp = (s.t % 0.6f) / 0.35f;                                                                   // nostril steam
                    if (sp < 1)
                    {
                        var b = s.tsr.bounds; float fx = s.tsr.flipX ? -1 : 1; Vector2 nose = new Vector2(b.center.x + fx * b.extents.x * 0.85f, b.center.y + b.extents.y * 0.35f);
                        for (int q = 0; q < 2; q++) r.Put(s, sp < 0.5f ? TellSprites.PuffM : TellSprites.PuffS, nose + new Vector2(fx * (2 + sp * 5 + q * 2), sp * 3 + q) * U, A(Steam, fd), false, 3);
                    }
                }
            }
            bool any = false; float pl = s.rad;
            for (int q = 0; q < TellFXRunner.MaxTrail; q++)
            {
                var d = s.trail[q]; float age = s.t - d.z; if (age < 0 || age > pl) continue; any = true;
                int ps = (int)(d.z * 997) + q; float k = age / pl; Vector2 p = new Vector2(d.x, d.y);
                r.Put(s, TellSprites.Scorch, p + Vector2.down * U, A(ScorchCol, (k < 0.8f ? 0.9f : 0.45f) * fd), false, -1);
                int flames = 1 + (ps % 3);
                for (int j = 0; j < flames; j++)
                {
                    float jk = k * (1 + 0.25f * j); if (jk >= 0.9f) continue;
                    int rate = 3 + (int)(H01(s.seed, ps, j) * 3), fi = jk < 0.08f || jk > 0.72f ? 3 : ((frame / rate) + j + ps) % 3;
                    Vector2 off = new Vector2((j == 0 ? 0 : j == 1 ? -3 : 3) + (H01(s.seed, ps, j + 4) - 0.5f) * 2, (j == 0 ? 0 : 1)) * U;
                    Color c = Color.Lerp(j == 0 ? (Color)StunYellow : (Color)TellHot, s.color2, jk);
                    r.Put(s, j == 0 ? TellSprites.FlameBook[fi] : TellSprites.FlameSmall[fi], p + off, A(c, fd), false, 0);
                }
                if (k >= 0.8f) { float w = (k - 0.8f) / 0.2f; r.Put(s, TellSprites.Pixel, p + new Vector2(w * 1.5f, 3 + w * 5) * U, A(Steam, 0.6f * fd), false, 1); } // smoke wisp
                else if ((ps & 1) == 0) { float ep = (s.t * 1.5f + H01(ps, 1, 1)) % 1f; r.Put(s, TellSprites.Pixel, p + new Vector2((H01(s.seed, ps, (int)(s.t * 1.5f)) - 0.5f) * 5, 2 + ep * 6) * U, A(StunYellow, fd), true, 1); }
            }
            return s.t < s.life || (any && s.t < s.life + pl);
        }
    }

    /// <summary>Pooled status overlays #110-111 for tiny horde enemies. v1.8: the body read is a vertex-colour tint on the target's own
    /// SpriteRenderer (SpriteRenderer.color - keeps the shared material and SRP batching; no sprite copies, no per-sprite material),
    /// plus 1-3 tiny atlas sprites. The original colour is stored and restored when the status ends. Share ReactionFXRunner.StatusCap.</summary>
    public static class StatusFX
    {
        static TellFXRunner R => TellFXRunner.I;
        static float U => 1f / HellFX.PPU;
        public static readonly Color BurnOrange = new Color32(0xf7, 0x76, 0x22, 255), ChillBlue = new Color32(0x2c, 0xe8, 0xf5, 255);
        /// <summary>Vertex tints (multiplied into the sprite): burn flickers between the two, chill / frozen are steady.</summary>
        public static Color BurnTintA = new Color(1f, 0.62f, 0.42f, 1f), BurnTintB = new Color(1f, 0.74f, 0.5f, 1f), ChillTint = new Color(0.55f, 0.82f, 1f, 1f), FrozenTint = new Color(0.72f, 0.95f, 1f, 1f);
        /// <summary>Set false if your game drives SpriteRenderer.color itself (overlay sprites only then). Only one status tints a target at a time.</summary>
        public static bool TintTargets = true;
        static Color A(Color c, float a) => TellFX.A(c, a);
        static float H01(int a, int b, int c) => TellFX.H01(a, b, c);

        /// <summary>Stops a Burn or Chill overlay (restores the target colour). Safe with stale or -1 handles.</summary>
        public static void StopStatus(int handle) { if (handle >= 0 && (handle & BurnFlag) != 0) { BurnFX.Extinguish(handle & ~BurnFlag); return; } var s = R.FromHandle(handle); if (s != null && TellFXRunner.IsStatus(s.kind)) R.Kill(s); }
        public static bool IsAlive(int handle) => handle >= 0 && (handle & BurnFlag) != 0 ? BurnFX.IsAlive(handle & ~BurnFlag) : R.FromHandle(handle) != null;
        /// <summary>Bit set on handles returned by BurnStatus (v1.8.7: burns run in BurnFX, not in this pool).</summary>
        public const int BurnFlag = 1 << 30;
        /// <summary>Switches a Chill overlay between chilled (blue tint + glint) and frozen (pale ice tint + ice shards at the feet).</summary>
        public static void SetFrozen(int handle, bool frozen) { var s = R.FromHandle(handle); if (s != null && s.kind == TellFXRunner.Kind.Chill) s.flag = frozen; }

        static void Grab(TellFXRunner r, TellFXRunner.Inst s, Transform target)
        {
            s.tsr = target.GetComponentInChildren<SpriteRenderer>(); if (!TintTargets || !s.tsr) return;
            for (int k = 0; k < TellFXRunner.MaxInst; k++) { var o = r.inst[k]; if (o != s && o.kind != TellFXRunner.Kind.None && o.tinted && o.tsr == s.tsr) return; } // already tinted by another status
            s.tint0 = s.tsr.color; s.tinted = true;
        }
        static void Tint(TellFXRunner.Inst s, Color c) { if (s.tinted && s.tsr && !s.stopping) { c.a = s.tint0.a; s.tsr.color = new Color(s.tint0.r * c.r, s.tint0.g * c.g, s.tint0.b * c.b, c.a); } else if (s.tinted && s.tsr) s.tsr.color = s.tint0; }

        // ---------------- 110. Burn status ----------------
        /// <summary>v1.8.7: routed to <see cref="BurnFX"/> (flames behind the silhouette, embers, tick sparks + numbers, end smoke; the sprite is never covered
        /// or permanently tinted). Same signature as v1.8: <paramref name="bodyHeightPx"/> / <paramref name="feetYPx"/> size the burn (width comes from the
        /// SpriteRenderer bounds). <paramref name="color"/> and <paramref name="order"/> are ignored now (one shared mesh: use BurnFX.FrontSortingOrder /
        /// BackSortingOrder). No damage ticks or numbers here (tickDamage 0); call BurnFX.Tick(handle &amp; ~BurnFlag, dmg) or use BurnFX.Ignite for synced ticks.
        /// No longer counts toward ReactionFXRunner.StatusCap. <paramref name="seconds"/> &lt;= 0 loops until StopStatus. Returns a handle (BurnFlag set) or -1.</summary>
        public static int BurnStatus(Transform target, float seconds = 3f, float bodyHeightPx = 8, float feetYPx = -7, Color? color = null, int order = 61)
        {
            int h = BurnFX.Ignite(target, seconds, 0, 0.5f, 0, bodyHeightPx, feetYPx);
            return h < 0 ? -1 : h | BurnFlag;
        }
        /// <summary>Legacy v1.8 burn renderer (no longer allocated by BurnStatus; kept for TellFXRunner Kind.Burn).</summary>
        internal static bool TickBurn(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 o = TellFX.Org(s); float fd = s.stopping ? 0.5f : 1;
            Tint(s, (((frame / 5) + s.seed) & 1) == 0 ? BurnTintA : BurnTintB);
            int fi = ((frame / 5) + s.seed) & 3;
            r.Put(s, TellSprites.FlameSmall[fi], o + new Vector2(0, s.h2 + s.rad) * U, A(Color.Lerp(TellFX.StunYellow, s.color, 0.35f), fd), false, 0);
            float ph = (s.t * 1.5f + H01(s.seed, 0, 1)) % 1f; int cyc = (int)(s.t * 1.5f + H01(s.seed, 0, 1));
            r.Put(s, TellSprites.Pixel, o + new Vector2((H01(s.seed, cyc, 4) - 0.5f) * 5, s.h2 + s.rad + 2 + ph * 6) * U, A(ph < 0.5f ? TellFX.StunYellow : s.color, fd), true, 1);
            return s.t < s.life;
        }

        // ---------------- 111. Chill / freeze status ----------------
        /// <summary>v1.8: cold-blue vertex tint + a twinkling frost glint near the head. <paramref name="frozen"/> (or SetFrozen): pale ice
        /// tint, two ice shards at the feet (<paramref name="widthPx"/> apart) and a slower glint - reads as "covered in ice" without copying
        /// the sprite. Targets without a SpriteRenderer get a frost crust at the feet. <paramref name="seconds"/> &lt;= 0 loops until StopStatus.
        /// Returns a handle, or -1 at StatusCap or pool full.</summary>
        public static int ChillStatus(Transform target, float seconds = 2f, bool frozen = false, float bodyHeightPx = 8, float widthPx = 8, float feetYPx = -7, Color? color = null, int order = 61)
        {
            if (!target) return -1;
            var s = R.Alloc(TellFXRunner.Kind.Chill, order, color ?? ChillBlue); if (s == null) return -1;
            s.follow = target; s.hasFollow = true; s.rad = Mathf.Clamp(bodyHeightPx, 2, 24); s.w = Mathf.Clamp(widthPx, 3, 24); s.h2 = feetYPx; s.flag = frozen;
            s.life = seconds > 0 ? seconds : float.PositiveInfinity; Grab(R, s, target); return R.Handle(s);
        }
        internal static bool TickChill(TellFXRunner r, TellFXRunner.Inst s, int frame)
        {
            Vector2 o = TellFX.Org(s); float fd = s.stopping ? 0.5f : 1;
            Tint(s, s.flag ? FrozenTint : ChillTint);
            if (!s.tinted) r.Put(s, TellSprites.Crust, o + new Vector2(0, s.h2) * U, A(s.color, 0.85f * fd), false, -1);
            if (s.flag)
            {
                var a = r.Put(s, TellSprites.IceShard, o + new Vector2(-s.w * 0.4f, s.h2 - 1) * U, A(Color.Lerp(s.color, Color.white, 0.5f), fd), false, 1);
                var b = r.Put(s, TellSprites.IceShard, o + new Vector2(s.w * 0.4f, s.h2 - 1) * U, A(Color.Lerp(s.color, Color.white, 0.5f), fd), false, 1); if (b) b.flipX = true;
            }
            int per = s.flag ? 24 : 16; bool tw = ((frame + s.seed) % per) < 4;
            if (tw) r.Put(s, ((frame + s.seed) % per) < 2 ? TellSprites.Star3 : TellSprites.Pixel, o + new Vector2(s.w * 0.3f, s.h2 + s.rad - 1) * U, A(Color.white, fd), true, 2);
            return s.t < s.life;
        }
    }
}
