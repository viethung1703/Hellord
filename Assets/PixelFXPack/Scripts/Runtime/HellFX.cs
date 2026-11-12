using System;
using System.Collections.Generic;
using UnityEngine;

// Pixel FX Pack v1.4 "Hell" survivor effects (built for a Vampire-Survivors-style top-down horde game, standalone-usable).
// All art is procedural (code-made pixel sprites, no AI art). One pooled runner animates every effect from a single Update.
// UNTESTED IN EDITOR - compiles against Unity 6 DLLs; see README "Known limits".
namespace PixelFXPack
{
    /// <summary>Red hell palette preset + helpers.</summary>
    public static class HellPalette
    {
        public static readonly Color Black = new Color32(18, 6, 10, 255), Blood = new Color32(92, 12, 22, 255), Crimson = new Color32(178, 24, 32, 255),
            Fire = new Color32(240, 84, 24, 255), Ember = new Color32(255, 160, 40, 255), Gold = new Color32(255, 222, 96, 255), White = new Color32(255, 246, 220, 255),
            Soul = new Color32(110, 255, 220, 255), Ash = new Color32(70, 52, 60, 255), Xp = new Color32(80, 220, 255, 255);
        public static readonly Color[] Ramp = { Black, Blood, Crimson, Fire, Ember, Gold, White };
        public static Gradient FireGradient() => G(White, Gold, Fire, Blood);
        public static Gradient BloodGradient() => G(new Color(1f, 0.5f, 0.5f), Crimson, Blood, Black);
        public static Gradient BoneGradient() => G(White, new Color(0.93f, 0.88f, 0.8f), new Color(0.7f, 0.65f, 0.6f), new Color(0.4f, 0.35f, 0.35f));
        public static Gradient SoulGradient() => G(White, Soul, new Color(0.2f, 0.5f, 0.9f), new Color(0.1f, 0.1f, 0.3f));
        static Gradient G(Color a, Color b, Color c, Color d)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, 0.25f), new GradientColorKey(c, 0.6f), new GradientColorKey(d, 1) },
                      new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
            return g;
        }
        /// <summary>Applies the hell gradient-map grade (dark blood -> crimson -> ember) to a PixelFX sprite.</summary>
        public static void ApplyGrade(PixelFX fx, float amount = 1f)
        {
            if (!fx) return;
            fx.gradientDark = Black; fx.gradientMid = Crimson; fx.gradientLight = Gold; fx.gradientMapAmount = amount;
        }
        /// <summary>3-row palette LUT for PaletteSwap/PixelFX.paletteLUT: row0 = source ramp, row1 = hell, row2 = soul-fire.</summary>
        public static Texture2D LUT(Color[] source)
        {
            int n = source.Length;
            var t = new Texture2D(n, 3, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "HellPaletteLUT" };
            for (int i = 0; i < n; i++)
            {
                float k = n > 1 ? i / (n - 1f) : 0;
                t.SetPixel(i, 0, source[i]);
                t.SetPixel(i, 1, Ramp[Mathf.RoundToInt(k * (Ramp.Length - 1))]);
                t.SetPixel(i, 2, Color.Lerp(new Color(0.05f, 0.05f, 0.15f), Soul, k));
            }
            t.Apply();
            return t;
        }
    }

    /// <summary>Procedural sprites for hell effects (cached).</summary>
    public static class HellSprites
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        static readonly float[,] Bayer = { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } };
        static Sprite Make(string key, int w, int h, Func<int, int, Color32> f, float ppu)
        {
            key += "_" + ppu;
            if (Cache.TryGetValue(key, out var s) && s) return s;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            return Cache[key] = PixelSpriteFactory.FromPixels(px, w, h, ppu, key);
        }
        static float H(int x, int y) { uint h = (uint)(x * 374761393 + y * 668265263); h = (h ^ (h >> 13)) * 1274126177; return (h & 0xffff) / 65535f; }
        static float SegD(Vector2 p, Vector2 a, Vector2 b) { var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude); return (a + ab * t - p).magnitude; }

        /// <summary>Pentagram rune circle, white (tint it). <paramref name="squash"/> &lt;1 flattens it for a three-quarter ground view.</summary>
        public static Sprite Pentagram(int r, float squash = 0.6f, float ppu = 16f)
        {
            int w = r * 2 + 3, h = Mathf.CeilToInt((r * 2 + 3) * squash);
            return Make($"penta{r}_{squash}", w, h, (x, y) =>
            {
                var p = new Vector2(x + 0.5f - w * 0.5f, (y + 0.5f - h * 0.5f) / squash);
                float d = p.magnitude;
                if (Mathf.Abs(d - r) < 0.9f || Mathf.Abs(d - (r - 3)) < 0.6f) return new Color32(255, 255, 255, 255);
                float ang = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;
                if (d > r - 3 && d < r && Mathf.Repeat(ang, 30) < 4) return new Color32(255, 255, 255, 200); // rune ticks
                float rr = r - 3; Vector2[] v = new Vector2[5];
                for (int i = 0; i < 5; i++) { float a = (90 + i * 72) * Mathf.Deg2Rad; v[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rr; }
                for (int i = 0; i < 5; i++) if (SegD(p, v[i], v[(i + 2) % 5]) < 0.75f) return new Color32(255, 255, 255, 255);
                return new Color32(0, 0, 0, 0);
            }, ppu);
        }
        /// <summary>Flat ellipse disk with dithered falloff (aura fields, telegraphs, portals).</summary>
        public static Sprite Disk(int r, float squash = 0.6f, float ppu = 16f) => Make($"disk{r}_{squash}", r * 2, Mathf.Max(2, Mathf.CeilToInt(r * 2 * squash)), (x, y) =>
        {
            float hh = Mathf.Ceil(r * 2 * squash);
            var p = new Vector2(x + 0.5f - r, (y + 0.5f - hh * 0.5f) / squash); float d = p.magnitude / r;
            if (d > 1) return new Color32(0, 0, 0, 0);
            if (d > 0.88f) return new Color32(255, 255, 255, 255);
            float a = 0.15f + 0.45f * d * d; a = Mathf.Floor(a * 4 + Bayer[y & 3, x & 3] / 16f) / 4f;
            return new Color32(255, 255, 255, (byte)(a * 255));
        }, ppu);
        /// <summary>Noisy flame ring (hellfire nova). White core edge, tint via color.</summary>
        public static Sprite FireRing(int r, int frame, float squash = 0.6f, float ppu = 16f) => Make($"fring{r}_{frame}_{squash}", r * 2 + 6, Mathf.CeilToInt((r * 2 + 6) * squash) + 6, (x, y) =>
        {
            int w = r * 2 + 6, h = Mathf.CeilToInt((r * 2 + 6) * squash) + 6;
            var p = new Vector2(x + 0.5f - w * 0.5f, (y + 0.5f - h * 0.5f) / squash);
            float d = p.magnitude, ang = Mathf.Atan2(p.y, p.x);
            float n = H(Mathf.RoundToInt(ang * 9) + frame * 7, frame) * 3f;
            float band = d - (r - 2);
            if (band > -2 && band < 1.5f + n) { byte a = (byte)(band < 0.5f ? 255 : 190); return new Color32(255, 255, 255, a); }
            if (band > -5 && band <= -2 && H(x + frame, y) > 0.6f) return new Color32(255, 255, 255, 110);
            return new Color32(0, 0, 0, 0);
        }, ppu);
        /// <summary>XP gem (tier 0 blue, 1 green, 2 red).</summary>
        public static Sprite Gem(int tier, float ppu = 16f)
        {
            Color32 hi = tier == 2 ? new Color32(255, 200, 200, 255) : tier == 1 ? new Color32(200, 255, 190, 255) : new Color32(210, 250, 255, 255);
            Color32 mid = tier == 2 ? new Color32(230, 40, 60, 255) : tier == 1 ? new Color32(70, 210, 90, 255) : new Color32(70, 190, 255, 255);
            Color32 lo = tier == 2 ? new Color32(110, 10, 30, 255) : tier == 1 ? new Color32(20, 90, 50, 255) : new Color32(30, 70, 160, 255);
            string[] rows = { "..k..", ".khk.", "khmmk", "kmmlk", ".kmk.", "..k.." };
            return Make($"gem{tier}", 5, 6, (x, y) =>
            {
                char c = rows[5 - y][x];
                return c == 'k' ? new Color32(16, 10, 24, 255) : c == 'h' ? hi : c == 'm' ? mid : c == 'l' ? lo : new Color32(0, 0, 0, 0);
            }, ppu);
        }
        static readonly string[][] BloodRows = {
            new[] { "..k..", ".krk.", "krhrk", "krrRk", ".kkk." },
            new[] { "...k...", "..krk..", ".krrrk.", "krhrrRk", "krhrrRk", "krrrRRk", ".kRRRk.", "..kkk.." },
            new[] { "....k....", "...krk...", "..krrrk..", ".krhhrrk.", "krhhrrrRk", "krhrrrrRk", "krrrrrRRk", "krrrrRRRk", ".krrRRRk.", "..kkkkk.." } };
        static readonly string[][] BoneRows = {
            new[] { "kk...kk", "kwk.kwk", ".kwwwk.", ".kwwwk.", "kwk.kwk", "kk...kk" },
            new[] { ".kkkkk.", "kwwwwwk", "kwkwkwk", "kkwwwkk", "kwwwwwk", ".kwkwk.", ".kkkkk." } };
        static Sprite FromStrings(string key, string[] rows, Func<char, Color32> pal, float ppu)
        {
            int h = rows.Length, w = rows[0].Length;
            return Make(key, w, h, (x, y) => { char c = rows[h - 1 - y][x]; return c == '.' ? new Color32(0, 0, 0, 0) : pal(c); }, ppu);
        }
        /// <summary>Blood drop/orb pickup: tier 0 small, 1 medium, 2 large. Dark outline, glossy highlight.</summary>
        public static Sprite BloodDrop(int tier, float ppu = 16f) => FromStrings($"blood{tier}", BloodRows[Mathf.Clamp(tier, 0, 2)], c =>
            c == 'k' ? new Color32(30, 4, 10, 255) : c == 'h' ? new Color32(255, 190, 190, 255) : c == 'R' ? new Color32(110, 8, 22, 255) : new Color32(210, 24, 40, 255), ppu);
        /// <summary>Bone pickup: 0 = crossed bone shard, 1 = skull (big drop).</summary>
        public static Sprite Bone(int big, float ppu = 16f) => FromStrings($"bone{big}", BoneRows[Mathf.Clamp(big, 0, 1)], c =>
            c == 'k' ? new Color32(30, 18, 20, 255) : new Color32(236, 226, 206, 255), ppu);
        /// <summary>Soul wisp: bright head with a short tail pointing -x.</summary>
        public static Sprite Wisp(float ppu = 16f) => Make("wisp", 7, 3, (x, y) =>
        {
            if (y == 1 && x >= 4) return new Color32(255, 255, 255, 255);
            if (y == 1) return new Color32(255, 255, 255, (byte)(60 + x * 45));
            if (x >= 5) return new Color32(255, 255, 255, 160);
            return new Color32(0, 0, 0, 0);
        }, ppu);
        /// <summary>Meteor: 5x5 rock with molten core.</summary>
        public static Sprite Meteor(float ppu = 16f) => Make("meteor", 6, 6, (x, y) =>
        {
            float d = new Vector2(x - 2.5f, y - 2.5f).magnitude;
            if (d > 2.9f) return new Color32(0, 0, 0, 0);
            if (d < 1.3f) return new Color32(255, 240, 160, 255);
            return d < 2.1f ? new Color32(255, 120, 30, 255) : new Color32(90, 30, 20, 255);
        }, ppu);
        /// <summary>Cracked hell-stone tile with lava seams (64x64, tileable). <paramref name="glow"/> 0..1 brightens the seams.</summary>
        public static Sprite LavaTile(int frame, float ppu = 16f) => Make($"lava{frame}", 64, 64, (x, y) =>
        {
            // Voronoi-ish cracks from a 4x4 cell grid (wraps -> tileable)
            float best = 99, second = 99;
            for (int cy = -1; cy <= 4; cy++) for (int cx = -1; cx <= 4; cx++)
            {
                int wx = (cx + 4) % 4, wy = (cy + 4) % 4;
                var c = new Vector2(cx * 16 + 3 + H(wx, wy) * 10, cy * 16 + 3 + H(wy + 9, wx) * 10);
                float d = (new Vector2(x, y) - c).magnitude;
                if (d < best) { second = best; best = d; } else if (d < second) second = d;
            }
            float edge = second - best;
            float pulse = 0.5f + 0.5f * Mathf.Sin(frame * 1.57f + (x + y) * 0.08f);
            if (edge < 1.1f) return Color32.Lerp(new Color32(255, 110, 20, 255), new Color32(255, 220, 90, 255), pulse * 0.6f);
            if (edge < 2.2f) return Color32.Lerp(new Color32(140, 24, 18, 255), new Color32(200, 50, 20, 255), pulse);
            float n = H(x, y);
            return n > 0.93f ? new Color32(62, 36, 40, 255) : n < 0.05f ? new Color32(22, 10, 14, 255) : new Color32(38, 18, 24, 255);
        }, ppu);
        /// <summary>Star burst for crit pops (4 spikes).</summary>
        public static Sprite Star(int r, float ppu = 16f) => Make($"star{r}", r * 2 + 1, r * 2 + 1, (x, y) =>
        {
            int dx = Mathf.Abs(x - r), dy = Mathf.Abs(y - r);
            bool on = (dx == 0 && dy <= r) || (dy == 0 && dx <= r) || (dx == dy && dx <= r / 2) || (dx + dy <= r / 3);
            return on ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
        }, ppu);
        public static Sprite Pixel(float ppu = 16f) => Make("px", 1, 1, (x, y) => new Color32(255, 255, 255, 255), ppu);
    }

    /// <summary>Pooled runner behind every HellFX call. One Update, no coroutines.</summary>
    [AddComponentMenu("")]
    public class HellFXRunner : MonoBehaviour
    {
        public class Job
        {
            public SpriteRenderer sr; public float age, life, delay; public Vector3 a, b, v; public Color c; public float f, g; public int i;
            public Action<Job, float> tick; public Action<Job> end; public Transform follow; public bool started;
        }
        static HellFXRunner _i;
        public static HellFXRunner I { get { if (_i == null) { var go = new GameObject("HellFX"); _i = go.AddComponent<HellFXRunner>(); } return _i; } }
        [Min(64)] public int maxSprites = 600;
        readonly List<Job> _jobs = new List<Job>(256);
        readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();
        readonly Stack<Job> _jobPool = new Stack<Job>();
        int _alive;
        public int Alive => _alive;
        void OnDestroy() { if (_i == this) _i = null; }

        /// <summary>Returns null when over budget - callers must handle that (effect is skipped).</summary>
        public Job Spawn(Sprite s, Vector3 pos, Color c, int order, float life, bool additive, Action<Job, float> tick, float delay = 0)
        {
            if (_alive >= maxSprites) return null;
            var sr = _pool.Count > 0 ? _pool.Pop() : null;
            if (!sr) { sr = new GameObject("hfx").AddComponent<SpriteRenderer>(); sr.transform.SetParent(transform, false); }
            sr.sprite = s; sr.color = c; sr.sortingOrder = order; sr.flipX = false;
            var am = additive ? PixelFXMaterials.Additive : null;
            sr.sharedMaterial = am != null ? am : HellFX.DefaultSpriteMaterial(sr);
            sr.transform.position = pos; sr.transform.localScale = Vector3.one; sr.transform.rotation = Quaternion.identity;
            sr.gameObject.SetActive(delay <= 0);
            var j = _jobPool.Count > 0 ? _jobPool.Pop() : new Job();
            j.sr = sr; j.age = 0; j.life = Mathf.Max(0.01f, life); j.delay = delay; j.a = pos; j.b = pos; j.v = Vector3.zero; j.c = c; j.f = j.g = 0; j.i = 0;
            j.tick = tick; j.end = null; j.follow = null; j.started = delay <= 0;
            _jobs.Add(j); _alive++;
            return j;
        }
        public void Kill(Job j) { if (j != null) j.life = -1; }
        void Update()
        {
            float dt = Time.deltaTime;
            for (int n = _jobs.Count - 1; n >= 0; n--)
            {
                var j = _jobs[n];
                if (j.delay > 0) { j.delay -= dt; if (j.delay > 0) continue; j.sr.gameObject.SetActive(true); j.started = true; }
                j.age += dt;
                if (j.life < 0 || j.age >= j.life || !j.sr)
                {
                    var e = j.end; j.end = null;
                    if (j.sr) { j.sr.gameObject.SetActive(false); _pool.Push(j.sr); }
                    _jobs.RemoveAt(n); _alive--; j.tick = null; j.follow = null; _jobPool.Push(j);
                    e?.Invoke(j);
                    continue;
                }
                j.tick?.Invoke(j, j.age / j.life);
            }
        }
    }

    /// <summary>Static entry points for the hell survivor effects. Positions are world space; sizes in pixels.</summary>
    public static class HellFX
    {
        public static float PPU = 16f;
        static Material _spriteDefault;
        internal static Material DefaultSpriteMaterial(SpriteRenderer sr) { if (_spriteDefault == null) _spriteDefault = sr.sharedMaterial; return _spriteDefault; }
        static HellFXRunner R => HellFXRunner.I;
        static float U => 1f / PPU;
        static Vector3 Snap(Vector3 p) => new Vector3(Mathf.Round(p.x * PPU) / PPU, Mathf.Round(p.y * PPU) / PPU, p.z);
        static float Step(float k, int n) => Mathf.Floor(k * n) / n;
        static Color A(Color c, float a) => new Color(c.r, c.g, c.b, c.a * a);
        static PixelBurstSettings _embers, _sparks;
        static PixelBurstSettings Embers(int n) { if (_embers == null) _embers = new PixelBurstSettings { speed = new Vector2(0.6f, 2.2f), directionDegrees = 90, spreadDegrees = 70, lifetime = new Vector2(0.4f, 0.9f), gravity = 1.5f, drag = 1.5f, sizePixels = new Vector2Int(1, 1), colorOverLife = HellPalette.FireGradient(), colorSteps = 4, additive = true, sway = 3f }; _embers.count = n; return _embers; }
        static PixelBurstSettings Sparks(int n) { if (_sparks == null) _sparks = new PixelBurstSettings { speed = new Vector2(2f, 5f), spreadDegrees = 360, lifetime = new Vector2(0.15f, 0.35f), drag = 5, sizePixels = new Vector2Int(1, 2), colorOverLife = HellPalette.FireGradient(), colorSteps = 3, additive = true }; _sparks.count = n; return _sparks; }

        // 1-2. XP gem + magnet pull -------------------------------------------------
        /// <summary>Drops a bobbing XP gem with a 1px sparkle. Returns a handle; call Magnet() to pull it in.</summary>
        public static HellFXRunner.Job XPGem(Vector3 pos, int tier = 0, float lifetime = 60f, int order = 20)
            => Pickup(HellSprites.Gem(tier, PPU), pos, tier, lifetime, order);
        /// <summary>Hell pickup: blood drop/orb (tier 0 small, 1 medium, 2 large). Glossy glint, red splat + drips on collect. Use Magnet/MagnetAll like gems.</summary>
        public static HellFXRunner.Job BloodDrop(Vector3 pos, int tier = 0, float lifetime = 60f, int order = 20)
            => Pickup(HellSprites.BloodDrop(tier, PPU), pos, 10 + Mathf.Clamp(tier, 0, 2), lifetime, order);
        /// <summary>Hell pickup: bone shard (big=false) or skull (big=true). Bone chips + pop on collect.</summary>
        public static HellFXRunner.Job BonePickup(Vector3 pos, bool big = false, float lifetime = 60f, int order = 20)
            => Pickup(HellSprites.Bone(big ? 1 : 0, PPU), pos, big ? 21 : 20, lifetime, order);
        /// <summary>Generic bobbing pickup. kind: 0-2 gem tiers, 10-12 blood tiers, 20-21 bone/skull (selects the collect effect).</summary>
        public static HellFXRunner.Job Pickup(Sprite sprite, Vector3 pos, int kind, float lifetime = 60f, int order = 20)
        {
            var j = R.Spawn(sprite, pos, Color.white, order, lifetime, false, (job, k) =>
            {
                if (job.follow != null)
                {   // magnet: accelerate toward target, collect on contact
                    job.g += Time.deltaTime * 60f * U * 4f; job.f = Mathf.Min(job.f + job.g, 14f);
                    var to = job.follow.position - job.a; float d = to.magnitude;
                    if (d < 4 * U) { job.life = -1; Collect(job.follow.position, job.i); return; }
                    job.a += to / d * Mathf.Min(d, job.f * Time.deltaTime);
                    job.sr.transform.position = Snap(job.a);
                }
                else
                {
                    float bob = Mathf.Round(Mathf.Sin(job.age * 5f + job.a.x) * 1.4f) * U;
                    job.sr.transform.position = Snap(job.a) + new Vector3(0, bob, 0);
                    job.sr.color = Mathf.Repeat(job.age + job.a.x, 1.2f) < 0.08f ? new Color(1.6f, 1.6f, 1.6f) : Color.white;
                }
            });
            if (j != null) { j.i = kind; j.f = 0; }
            return j;
        }
        /// <summary>Magnet pull: the gem accelerates into <paramref name="target"/> and pops on collect.</summary>
        public static void Magnet(HellFXRunner.Job gem, Transform target, float startSpeed = -1.5f)
        {
            if (gem == null || gem.life < 0 || gem.follow != null) return;
            gem.follow = target; gem.f = startSpeed; gem.g = 0; gem.life = gem.age + 10f; // a small hop away first (negative speed)
        }
        /// <summary>Pulls every gem within <paramref name="radiusPx"/>. Pass the list you keep of spawned gems.</summary>
        public static int MagnetAll(List<HellFXRunner.Job> gems, Transform target, float radiusPx)
        {
            int n = 0; float r = radiusPx * U;
            for (int i = gems.Count - 1; i >= 0; i--)
            {
                var g = gems[i];
                if (g == null || g.sr == null || g.life < 0 || !g.sr.gameObject.activeSelf) { gems.RemoveAt(i); continue; }
                if (g.follow == null && (g.a - target.position).sqrMagnitude < r * r) { Magnet(g, target); n++; gems.RemoveAt(i); }
            }
            return n;
        }
        static PixelBurstSettings _splat, _chips;
        static void Collect(Vector3 at, int tier)
        {
            if (tier >= 10 && tier < 20)
            {   // blood: splat ring + falling drips
                if (_splat == null) _splat = new PixelBurstSettings { speed = new Vector2(1f, 3f), directionDegrees = 90, spreadDegrees = 160, lifetime = new Vector2(0.25f, 0.5f), gravity = 6f, drag = 2, sizePixels = new Vector2Int(1, 2), colorOverLife = HellPalette.BloodGradient(), colorSteps = 2 };
                _splat.count = 4 + (tier - 10) * 4; PixelParticles.Burst(_splat, at);
                R.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), at, HellPalette.Crimson, 60, 0.16f, true, (j, k) => { j.sr.transform.localScale = new Vector3(1, 0.6f, 1) * (0.5f + Step(k, 3)); j.sr.color = A(j.c, 1 - k); });
                return;
            }
            if (tier >= 20)
            {   // bone: white chips + pop
                if (_chips == null) _chips = new PixelBurstSettings { speed = new Vector2(1.5f, 3.5f), directionDegrees = 90, spreadDegrees = 120, lifetime = new Vector2(0.2f, 0.4f), gravity = 5f, drag = 2, sizePixels = new Vector2Int(1, 1), colorOverLife = HellPalette.BoneGradient(), colorSteps = 2 };
                _chips.count = tier == 21 ? 10 : 4; PixelParticles.Burst(_chips, at);
                PixelFXSpawn.LightPop(at, Color.white, tier == 21 ? 20 : 12, 0.15f, 61, PPU);
                return;
            }
            var c = tier == 2 ? HellPalette.Crimson : tier == 1 ? new Color(0.4f, 1f, 0.5f) : HellPalette.Xp;
            R.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), at, c, 60, 0.18f, true, (j, k) => { j.sr.transform.localScale = Vector3.one * (0.5f + Step(k, 3)); j.sr.color = A(j.c, 1 - k); });
        }

        // 3-4. Level-up burst + choice glow ------------------------------------------
        /// <summary>Gold pillar + double ring + rune circle flash + rising embers. Pair with HitStop for the choice pause.</summary>
        public static void LevelUpBurst(Vector3 feet, int order = 70)
        {
            R.Spawn(HellSprites.Pentagram(22, 0.6f, PPU), feet, HellPalette.Gold, order - 10, 0.9f, true, (j, k) =>
            { j.sr.transform.localScale = Vector3.one * (k < 0.15f ? Step(k / 0.15f, 4) : 1f); j.sr.color = A(j.c, 1 - Step(Mathf.Max(0, k - 0.5f) * 2, 4)); });
            for (int r = 0; r < 2; r++)
                R.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), feet, r == 0 ? HellPalette.White : HellPalette.Gold, order, 0.45f, true, (j, k) =>
                { float e = 1 - (1 - k) * (1 - k); j.sr.transform.localScale = new Vector3(1, 0.6f, 1) * (0.5f + Step(e, 6) * 3.5f); j.sr.color = A(j.c, 1 - Step(k, 4)); }, r * 0.12f);
            var px = HellSprites.Pixel(PPU);
            R.Spawn(px, feet, HellPalette.Gold, order + 1, 0.6f, true, (j, k) =>
            {   // light pillar: tall 6px column shrinking to 1px
                float w = k < 0.2f ? 8 : Mathf.Max(1, Mathf.Round(8 * (1 - (k - 0.2f) / 0.8f)));
                j.sr.transform.localScale = new Vector3(w, 70, 1); j.sr.transform.position = Snap(j.a + new Vector3(0, 35 * U, 0));
                j.sr.color = A(k < 0.1f ? HellPalette.White : j.c, 1 - Step(k, 5) * 0.8f);
            });
            PixelParticles.Burst(Embers(24), feet);
            PixelFXSpawn.LightPop(feet + Vector3.up * 0.5f, HellPalette.Gold, 32, 0.3f, order - 1, PPU);
        }
        /// <summary>Pulsing hellfire outline + emission on an upgrade-choice card sprite. Returns a handle; Kill it when chosen.</summary>
        public static HellFXRunner.Job ChoiceGlow(PixelFX card, Color color, bool selected)
        {
            if (!card) return null;
            card.outlineSize = 1; card.outlineDiagonals = true;
            var j = R.Spawn(PixelFXTextures.Glow(24, 5, HellFX.PPU), card.transform.position, color, card.SpriteRenderer.sortingOrder - 1, 9999f, true, (job, k) =>
            {
                if (!card) { job.life = -1; return; }
                float p = 0.5f + 0.5f * Mathf.Sin(job.age * (selected ? 9f : 4f));
                card.outlineColor = Color.Lerp(color, HellPalette.White, Step(p, 3) * (selected ? 0.8f : 0.3f));
                card.emissionColor = color; card.emissionAmount = selected ? 0.3f + 0.5f * Step(p, 3) : 0.1f;
                job.sr.transform.position = card.transform.position;
                job.sr.color = A(color, (selected ? 0.5f : 0.15f) + 0.25f * Step(p, 3));
            });
            return j;
        }

        // 5. Orbiting weapons ----------------------------------------------------------
        /// <summary>N blades/skulls orbiting <paramref name="center"/> with 3-step afterimage trail. Returns handles (Kill to stop).</summary>
        public static List<HellFXRunner.Job> OrbitWeapons(Transform center, Sprite weapon, int count, float radiusPx, float revPerSec, float seconds, Color trail, int order = 55)
        {
            var list = new List<HellFXRunner.Job>();
            for (int n = 0; n < count; n++)
            {
                float phase = n / (float)count * Mathf.PI * 2;
                var j = R.Spawn(weapon, center.position, Color.white, order, seconds, false, (job, k) =>
                {
                    if (!center) { job.life = -1; return; }
                    float ang = phase + job.age * revPerSec * Mathf.PI * 2;
                    var p = center.position + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang) * 0.6f, 0) * radiusPx * U;
                    job.sr.transform.position = Snap(p);
                    job.sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Round((ang * Mathf.Rad2Deg + 90) / 45f) * 45f);
                    job.sr.sortingOrder = Mathf.Sin(ang) > 0 ? order - 20 : order; // behind the player on the far side
                    if ((job.i++ & 1) == 0)
                        R.Spawn(weapon, job.sr.transform.position, A(trail, 0.6f), job.sr.sortingOrder - 1, 0.12f, true, (t, kk) => { t.sr.color = A(t.c, 1 - Step(kk, 3)); t.sr.transform.rotation = job.sr ? job.sr.transform.rotation : Quaternion.identity; });
                });
                if (j != null) list.Add(j);
            }
            return list;
        }

        // 6. Aura damage field ------------------------------------------------------------
        /// <summary>Ground aura following <paramref name="target"/>: dithered disk + pulsing rim + rising embers each tick.
        /// <paramref name="onTick"/> fires every <paramref name="tickSeconds"/> so you can apply damage.</summary>
        public static HellFXRunner.Job AuraField(Transform target, float radiusPx, Color color, float seconds, float tickSeconds = 0.5f, Action onTick = null, int order = 4)
        {
            int r = Mathf.RoundToInt(radiusPx);
            return R.Spawn(HellSprites.Disk(r, 0.6f, PPU), target.position, color, order, seconds, true, (j, k) =>
            {
                if (!target) { j.life = -1; return; }
                j.sr.transform.position = Snap(target.position);
                j.f += Time.deltaTime;
                float pulse = j.f < 0.1f ? 1f : 0f;
                j.sr.color = A(color, 0.45f + 0.35f * pulse + 0.1f * Step(0.5f + 0.5f * Mathf.Sin(j.age * 6), 2));
                j.sr.transform.localScale = Vector3.one * (1f + pulse * 0.06f);
                if (j.f >= tickSeconds)
                {
                    j.f = 0; onTick?.Invoke();
                    for (int e = 0; e < 3; e++)
                    {
                        float a = UnityEngine.Random.value * Mathf.PI * 2, d = UnityEngine.Random.value * radiusPx * U;
                        PixelParticles.Burst(Embers(1), target.position + new Vector3(Mathf.Cos(a) * d, Mathf.Sin(a) * d * 0.6f, 0));
                    }
                }
            });
        }

        // 7. Projectile trail ------------------------------------------------------------
        /// <summary>Attach to any moving projectile transform: drops fading 2px->1px fire pixels and a hot head glow.</summary>
        public static HellFXRunner.Job ProjectileTrail(Transform projectile, Color color, float seconds = 5f, int order = 58)
        {
            var px = HellSprites.Pixel(PPU);
            return R.Spawn(PixelFXTextures.Glow(4, 5, HellFX.PPU), projectile.position, color, order, seconds, true, (j, k) =>
            {
                if (!projectile || !projectile.gameObject.activeInHierarchy) { j.life = -1; return; }
                var p = projectile.position;
                j.sr.transform.position = Snap(p);
                float dist = (p - j.b).magnitude;
                int drops = Mathf.Min(4, Mathf.FloorToInt(dist * PPU / 2f));
                for (int d = 1; d <= drops; d++)
                {
                    var q = Vector3.Lerp(j.b, p, d / (float)drops);
                    R.Spawn(px, Snap(q), HellPalette.White, order - 1, 0.22f, true, (t, kk) =>
                    {
                        float s = kk < 0.3f ? 2 : 1; t.sr.transform.localScale = new Vector3(s, s, 1);
                        t.sr.color = A(Color.Lerp(HellPalette.White, color, Step(kk * 2, 2)), 1 - Step(kk, 3));
                    });
                }
                if (drops > 0) j.b = p;
            }) is var job && job != null ? Init(job, projectile.position) : null;
        }
        static HellFXRunner.Job Init(HellFXRunner.Job j, Vector3 b) { j.b = b; return j; }

        // 8. Chain lightning ----------------------------------------------------------------
        /// <summary>Jagged bolt hopping through <paramref name="points"/> (player, enemy1, enemy2...). Built from pooled pixel strips,
        /// re-jittered every 50 ms. Much cheaper than PixelVFX.Lightning (no per-pixel GameObjects).</summary>
        public static void ChainLightning(IList<Vector3> points, Color color, float seconds = 0.25f, float hopDelay = 0.04f, int order = 82)
        {
            // v1.5.1: forwards to the pooled ChainFX bolt (no per-call allocations, adds 3-frame hit sparks). Max 7 points.
            ChainFX.LightningFromList(points, color, seconds, hopDelay, order);
        }

        // 9. Hellfire nova --------------------------------------------------------------------
        /// <summary>Expanding flame ring (3-frame flicker) + core flash + sparks + optional screen shockwave/shake.</summary>
        /// <param name="lightPop">t1001u: soft Glow disc. Off for Fireball splash — under CRT/URP that Glow read as the purple dither bubble.</param>
        public static void HellfireNova(Vector3 pos, float radiusPx = 40, float seconds = 0.45f, bool screenFX = true, int order = 66, bool lightPop = true)
        {
            int r = Mathf.RoundToInt(radiusPx);
            R.Spawn(HellSprites.FireRing(r, 0, 0.6f, PPU), pos, HellPalette.Ember, order, seconds, true, (j, k) =>
            {
                float e = 1 - (1 - k) * (1 - k) * (1 - k);
                j.sr.sprite = HellSprites.FireRing(r, (j.i++ / 2) % 3, 0.6f, PPU);
                j.sr.transform.localScale = Vector3.one * Mathf.Max(0.1f, Step(e, 10));
                j.sr.color = Color.Lerp(HellPalette.White, Color.Lerp(HellPalette.Ember, HellPalette.Crimson, k), Step(k * 3, 3)) * new Color(1, 1, 1, 1 - Step(Mathf.Max(0, k - 0.5f) * 2, 3));
            });
            R.Spawn(HellSprites.Disk(Mathf.Max(4, r / 3), 0.6f, PPU), pos, HellPalette.White, order + 1, 0.12f, true, (j, k) => j.sr.color = A(j.c, 1 - Step(k, 2)));
            PixelParticles.Burst(Sparks(18), pos);
            PixelParticles.Burst(Embers(10), pos);
            if (lightPop) PixelFXSpawn.LightPop(pos, HellPalette.Fire, Mathf.Max(16, r), 0.25f, order - 2, PPU);
            if (screenFX) { ScreenShake.Add(3, 0.18f); if (Camera.main) PixelScreenFX.Shockwave(Camera.main, pos, 0.6f, 0.4f, 0.35f); }
        }

        // 10. Rune / pentagram circle cast ---------------------------------------------------
        /// <summary>Pentagram draws in (scale steps), spins slowly, flares, fades. <paramref name="onCast"/> fires at the flare.</summary>
        public static void RuneCircle(Vector3 pos, int radiusPx, Color color, float seconds = 1.2f, Action onCast = null, int order = 6)
        {
            var j = R.Spawn(HellSprites.Pentagram(radiusPx, 0.6f, PPU), pos, color, order, seconds, true, (job, k) =>
            {
                float grow = Mathf.Min(1, k / 0.25f);
                job.sr.transform.localScale = new Vector3(Step(grow, 6), Step(grow, 6), 1);
                bool flare = k > 0.6f && k < 0.7f;
                if (flare && job.i == 0) { job.i = 1; onCast?.Invoke(); PixelParticles.Burst(Embers(16), pos); }
                job.sr.color = flare ? HellPalette.White : A(color, (0.6f + 0.4f * Step(0.5f + 0.5f * Mathf.Sin(job.age * 14), 2)) * (1 - Step(Mathf.Max(0, k - 0.75f) * 4, 4)));
            });
            R.Spawn(PixelFXTextures.Glow(radiusPx, 5, HellFX.PPU), pos, color, order - 1, seconds, true, (g, k) => g.sr.color = A(g.c, 0.35f * Mathf.Min(1, k * 4) * (1 - Step(Mathf.Max(0, k - 0.75f) * 4, 4))));
        }

        // 11. Meteor / fire rain -----------------------------------------------------------------
        /// <summary>Meteors fall diagonally onto random points in a circle; each shows a ground telegraph first, then impacts with a mini nova.
        /// <paramref name="onImpact"/> gets each impact point (apply damage there).</summary>
        public static void MeteorRain(Vector3 center, float radiusPx, int count, float seconds, Action<Vector3> onImpact = null, int order = 85)
        {
            var rock = HellSprites.Meteor(PPU);
            for (int n = 0; n < count; n++)
            {
                float a = UnityEngine.Random.value * Mathf.PI * 2, d = Mathf.Sqrt(UnityEngine.Random.value) * radiusPx * U;
                var hit = Snap(center + new Vector3(Mathf.Cos(a) * d, Mathf.Sin(a) * d * 0.6f, 0));
                float delay = seconds * n / Mathf.Max(1, count), fall = 0.45f;
                Telegraph(hit, 7, fall, null, delay);
                var start = hit + new Vector3(-40, 90, 0) * U;
                var j = R.Spawn(rock, start, Color.white, order, fall, false, (m, k) =>
                {
                    var p = Vector3.Lerp(start, hit, k * k);
                    m.sr.transform.position = Snap(p);
                    if ((m.i++ & 1) == 0) PixelParticles.Burst(Embers(1), p);
                }, delay);
                if (j != null) j.end = _ => { HellfireNova(hit, 14, 0.3f, false, order - 20); ScreenShake.Add(1.5f, 0.1f); onImpact?.Invoke(hit); };
            }
        }

        // 12. Lava / ember ground ----------------------------------------------------------------
        /// <summary>Tiles an animated cracked-lava floor (4-frame seam pulse) over <paramref name="tilesX"/> x <paramref name="tilesY"/> 64px tiles,
        /// plus ambient embers. Returns the parent GameObject (static, cheap: tiles share 4 sprites from one tile size).</summary>
        public static GameObject LavaGround(Vector3 center, int tilesX, int tilesY, float embersPerSecond = 6f, int order = -100)
        {
            var root = new GameObject("LavaGround");
            root.transform.position = center;
            var tiles = new List<SpriteRenderer>();
            for (int y = 0; y < tilesY; y++) for (int x = 0; x < tilesX; x++)
            {
                var sr = new GameObject("tile").AddComponent<SpriteRenderer>();
                sr.sprite = HellSprites.LavaTile(0, PPU); sr.sortingOrder = order;
                sr.transform.SetParent(root.transform, false);
                sr.transform.localPosition = new Vector3((x - (tilesX - 1) * 0.5f) * 64 * U, (y - (tilesY - 1) * 0.5f) * 64 * U, 0);
                tiles.Add(sr);
            }
            float w = tilesX * 64 * U, h = tilesY * 64 * U;
            R.Spawn(HellSprites.Pixel(PPU), center, Color.clear, order, 1e6f, false, (j, k) =>
            {
                if (!root) { j.life = -1; return; }
                int f = Mathf.FloorToInt(j.age * 3f) % 4;
                if (f != j.i) { j.i = f; var s = HellSprites.LavaTile(f, PPU); foreach (var t in tiles) if (t) t.sprite = s; }
                j.f += Time.deltaTime * embersPerSecond;
                while (j.f >= 1) { j.f -= 1; PixelParticles.Burst(Embers(1), root.transform.position + new Vector3((UnityEngine.Random.value - 0.5f) * w, (UnityEngine.Random.value - 0.5f) * h, 0)); }
            });
            return root;
        }

        // 13. Soul absorb stream -----------------------------------------------------------------
        /// <summary>Cyan wisps curve from <paramref name="from"/> into <paramref name="target"/> (bezier with random bulge), flash on arrival.</summary>
        public static void SoulStream(Vector3 from, Transform target, int count = 6, float seconds = 0.6f, Color? color = null, int order = 72)
        {
            var c = color ?? HellPalette.Soul; var wisp = HellSprites.Wisp(PPU);
            for (int n = 0; n < count; n++)
            {
                var bulge = new Vector3(UnityEngine.Random.Range(-24f, 24f), UnityEngine.Random.Range(8f, 30f), 0) * U;
                var j = R.Spawn(wisp, from, c, order, seconds, true, (w, k) =>
                {
                    if (!target) { w.life = -1; return; }
                    var to = target.position; float e = k * k * (3 - 2 * k);
                    var mid = (from + to) * 0.5f + bulge;
                    var p = Vector3.Lerp(Vector3.Lerp(from, mid, e), Vector3.Lerp(mid, to, e), e);
                    var d = p - w.b; w.b = p;
                    w.sr.transform.position = Snap(p);
                    if (d.sqrMagnitude > 1e-6f) w.sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Round(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg / 45) * 45);
                    w.sr.color = (w.i++ & 3) == 0 ? HellPalette.White : c;
                }, n * 0.05f);
                if (j != null) { j.b = from; j.end = _ => { if (target) R.Spawn(PixelFXTextures.Glow(6, 5, HellFX.PPU), target.position, c, order - 1, 0.12f, true, (g, k) => g.sr.color = A(g.c, 1 - k)); }; }
            }
        }

        /// <summary>Blood absorb stream: red wisps from a kill into the player (SoulStream in blood colours).</summary>
        public static void BloodStream(Vector3 from, Transform target, int count = 5, float seconds = 0.55f, int order = 72)
            => SoulStream(from, target, count, seconds, HellPalette.Crimson, order);

        // 14. Enemy spawn portal ------------------------------------------------------------------
        /// <summary>Ground rift opens (ellipse grows in steps), swirls, spits sparks, <paramref name="onOpen"/> fires at full size, then closes.</summary>
        public static void SpawnPortal(Vector3 pos, int radiusPx = 10, float seconds = 1f, Action onOpen = null, int order = 3)
        {
            R.Spawn(HellSprites.Disk(radiusPx, 0.5f, PPU), pos, HellPalette.Black, order, seconds, false, (j, k) =>
            {
                float s = k < 0.3f ? Step(k / 0.3f, 4) : k > 0.8f ? Step((1 - k) / 0.2f, 4) : 1;
                j.sr.transform.localScale = new Vector3(s, s, 1);
                if (k >= 0.3f && j.i == 0) { j.i = 1; onOpen?.Invoke(); PixelParticles.Burst(Sparks(10), pos); }
            });
            R.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), pos, HellPalette.Crimson, order + 1, seconds, true, (j, k) =>
            {
                float s = (k < 0.3f ? Step(k / 0.3f, 4) : k > 0.8f ? Step((1 - k) / 0.2f, 4) : 1) * radiusPx / 6f;
                j.sr.transform.localScale = new Vector3(s, s * 0.5f, 1);
                j.sr.color = (Mathf.FloorToInt(j.age * 12) & 1) == 0 ? HellPalette.Fire : HellPalette.Crimson;
                if ((j.i++ % 6) == 0) PixelParticles.Burst(Embers(1), pos + new Vector3(UnityEngine.Random.Range(-radiusPx, radiusPx) * U, 0, 0));
            });
        }

        // 15. Elite / boss telegraph --------------------------------------------------------------
        /// <summary>Ground warning: red ellipse whose fill grows to the edge over <paramref name="seconds"/>, rim blinking faster near the end.</summary>
        public static void Telegraph(Vector3 pos, int radiusPx, float seconds, Action onDone = null, float delay = 0, int order = 5)
        {
            var rim = R.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), pos, HellPalette.Crimson, order + 1, seconds, true, (j, k) =>
            {
                j.sr.transform.localScale = new Vector3(radiusPx / 6f, radiusPx / 6f * 0.6f, 1);
                float rate = Mathf.Lerp(4, 20, k);
                j.sr.color = (Mathf.FloorToInt(j.age * rate) & 1) == 0 ? HellPalette.Fire : A(HellPalette.Crimson, 0.6f);
            }, delay);
            R.Spawn(HellSprites.Disk(radiusPx, 0.6f, PPU), pos, HellPalette.Crimson, order, seconds, true, (j, k) =>
            { float s = Step(k, 8); j.sr.transform.localScale = new Vector3(s, s, 1); j.sr.color = A(j.c, 0.35f + 0.3f * k); }, delay);
            if (rim != null && onDone != null) rim.end = _ => onDone();
        }

        // 16. Crit pop -----------------------------------------------------------------------------
        /// <summary>Big gold number with "!" + 4-spike star + white light pop + 1-frame hitstop-friendly shake.</summary>
        public static void CritPop(Vector3 pos, int value, int order = 96)
        {
            R.Spawn(HellSprites.Star(7, PPU), pos, HellPalette.White, order - 1, 0.2f, true, (j, k) =>
            { j.sr.transform.localScale = Vector3.one * (k < 0.3f ? 1.5f : 1f); j.sr.color = A(k < 0.3f ? HellPalette.White : HellPalette.Gold, 1 - Step(k, 3)); });
            HordeFX.Number(pos + Vector3.up * 6 * U, value, true);
            PixelParticles.Burst(Sparks(8), pos);
            ScreenShake.Add(1.5f, 0.08f);
        }

        // 17. Kill-streak pop ------------------------------------------------------------------------
        /// <summary>"x25!" counter that slams in above the player, wobbles, and fades. Colour climbs white -> gold -> red with the count.</summary>
        public static void KillStreakPop(Vector3 pos, int count, int order = 97)
        {
            var col = count >= 100 ? (Color32)HellPalette.Crimson : count >= 50 ? (Color32)HellPalette.Fire : count >= 25 ? (Color32)HellPalette.Gold : new Color32(255, 255, 255, 255);
            var spr = DamageNumbers.TextSprite("x" + count + "!", col, PPU);
            R.Spawn(spr, pos + Vector3.up * 20 * U, Color.white, order, 1.1f, false, (j, k) =>
            {
                float s = k < 0.06f ? 3f : k < 0.12f ? 1.6f : 2f;
                j.sr.transform.localScale = new Vector3(s, s, 1);
                j.sr.transform.position = Snap(j.a + new Vector3(k < 0.3f ? Mathf.Round(Mathf.Sin(j.age * 60) * 1) * U : 0, Step(k, 8) * 6 * U, 0));
                j.sr.color = new Color(1, 1, 1, 1 - Step(Mathf.Max(0, k - 0.7f) / 0.3f, 3));
            });
            if (count % 25 == 0) R.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), pos + Vector3.up * 20 * U, (Color)col, order - 1, 0.3f, true, (j, k) => { j.sr.transform.localScale = Vector3.one * (1 + Step(k, 4) * 3); j.sr.color = A(j.c, 1 - k); });
        }

        // 18. Demon transformation / evolution --------------------------------------------------------
        /// <summary>Evolution sequence on the player: rune circle + soul vortex charge (white flash ramps up), silhouette pop,
        /// sprite swap at the peak (<paramref name="onSwap"/>, e.g. set the evolved sprite), then nova + shockwave + chromatic.
        /// Total ~1.6 s. Use unscaled pauses in your game (HitStop) around it if you freeze gameplay.</summary>
        public static void DemonEvolve(PixelFX demon, Action onSwap, float charge = 1.1f)
        {
            if (!demon) return;
            var pos = demon.transform.position;
            RuneCircle(pos, 26, HellPalette.Crimson, charge + 0.5f);
            R.Spawn(HellSprites.Pixel(PPU), pos, Color.clear, 0, charge, false, (j, k) =>
            {
                if (!demon) { j.life = -1; return; }
                demon.flashColor = HellPalette.White; demon.flashAmount = Step(k, 6) * 0.9f;
                demon.outlineSize = 1; demon.outlineColor = (j.i++ & 2) == 0 ? HellPalette.Gold : HellPalette.Crimson;
                if ((j.i % 3) == 0)
                {   // souls spiral in
                    float a = UnityEngine.Random.value * Mathf.PI * 2;
                    SoulStream(pos + new Vector3(Mathf.Cos(a), Mathf.Sin(a) * 0.6f, 0) * 3f, demon.transform, 1, 0.4f, k > 0.6f ? HellPalette.Gold : HellPalette.Soul);
                }
                if (k > 0.5f) ScreenShake.Add(0.5f, 0.05f);
            }).end = _ =>
            {
                if (!demon) return;
                onSwap?.Invoke();
                demon.flashAmount = 1f;
                R.Spawn(HellSprites.Pixel(PPU), pos, Color.clear, 0, 0.35f, false, (j, k) => { if (demon) demon.flashAmount = 1 - Step(k, 4); }).end = __ => { if (demon) { demon.flashAmount = 0; demon.outlineColor = HellPalette.Black; } };
                HellfireNova(pos, 56, 0.55f, true);
                LevelUpBurst(pos);
                PixelScreenFX.ChromaticPulse(3f, 0.3f);
            };
        }

        // 19. Heat distortion ---------------------------------------------------------------------------
        /// <summary>Heat haze over a world-space rect (needs the Full Screen Pass renderer feature). amplitude 0 turns it off.</summary>
        public static void HeatDistortion(Camera cam, Rect worldRect, float amplitudePixels = 1.5f)
        {
            if (!cam) return;
            var a = cam.WorldToViewportPoint(new Vector3(worldRect.xMin, worldRect.yMin, 0));
            var b = cam.WorldToViewportPoint(new Vector3(worldRect.xMax, worldRect.yMax, 0));
            PixelScreenFX.SetHeatHaze(Rect.MinMaxRect(a.x, a.y, b.x, b.y), amplitudePixels);
        }

        // shared
        static Sprite Ring() => PixelFXTextures.Ring(6, 1, PPU);
        static Sprite Glow(int r) => PixelFXTextures.Glow(r, 5, PPU);
    }

    /// <summary>Component form of the heat distortion: keeps a haze rect over this transform (e.g. a lava pool or the evolved demon).</summary>
    [AddComponentMenu("Pixel FX/Hell/Heat Distortion Zone")]
    public class HeatDistortionZone : MonoBehaviour
    {
        public Vector2 sizePixels = new Vector2(48, 32);
        [Range(0, 4)] public float amplitudePixels = 1.5f;
        public float pixelsPerUnit = 16f;
        void LateUpdate()
        {
            var s = sizePixels / pixelsPerUnit; var p = (Vector2)transform.position;
            HellFX.HeatDistortion(Camera.main, new Rect(p - s * 0.5f, s), amplitudePixels);
        }
        void OnDisable() { PixelScreenFX.SetHeatHaze(new Rect(0, 0, 0, 0), 0); }
    }
}
