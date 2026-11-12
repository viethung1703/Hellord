using System;
using UnityEngine;

// Pixel FX Pack v1.5 "Projectiles" - survivor-style projectile weapons for a top-down horde game (Hellord), usable in any 2D game.
// Code-made pixel sprites only (no AI art). One pooled runner moves every projectile from a single Update:
// fixed-size arrays, cached non-capturing delegates, shared materials (additive or default) so projectiles batch.
// UNTESTED IN EDITOR - compiles against Unity 6 DLLs; see README "Known limits".
namespace PixelFXPack
{
    /// <summary>Projectile element: picks sprite, palette, cast flash, trail and impact.</summary>
    public enum ProjElement { Fire, Blood, Bone, Soul, Curse, Hellfire, Lightning, Arcane, Blade }

    /// <summary>Spawn options. Struct = no allocation. Start from <see cref="ProjectileFX.Preset"/>.</summary>
    public struct ProjShot
    {
        public ProjElement element;
        public float speedPx;        // pixels per second
        public float lifeSeconds;
        public int pierce;           // enemies passed through before dying (-1 = infinite)
        public int bounce;           // wall/enemy bounces (retargets to nearest enemy if homingRadiusPx > 0)
        public int split;            // on first hit, split into this many children (children don't split)
        public float homingDegPerSec; // 0 = straight
        public float homingRadiusPx;
        public bool boomerang;       // flies out, decelerates, returns to owner
        public float spinDegPerSec;  // sprite spin (bones, blades)
        public float damage;
        public float radiusPx;       // hit radius
        public int sortingOrder;
    }

    [AddComponentMenu("")]
    public class ProjectileFXRunner : MonoBehaviour
    {
        static ProjectileFXRunner _i;
        public static ProjectileFXRunner I { get { if (_i == null) { var go = new GameObject("ProjectileFX"); _i = go.AddComponent<ProjectileFXRunner>(); _i.Init(); } return _i; } }
        public const int Max = 1024;
        internal struct P
        {
            public bool alive; public ProjShot s; public Vector2 pos, vel; public float age, trailAcc, rot; public int pierceLeft, bounceLeft, gen;
            public Transform owner; public Vector2 last; public int lastHitId;
        }
        internal readonly P[] ps = new P[Max];
        internal readonly SpriteRenderer[] srs = new SpriteRenderer[Max];
        readonly int[] _free = new int[Max]; int _freeN;
        readonly Collider2D[] _hits = new Collider2D[16];
        public int Alive => Max - _freeN;
        /// <summary>Layer mask used for enemy hits (default: everything).</summary>
        public LayerMask enemyMask = ~0;
        ContactFilter2D _filter;
        void OnDestroy() { if (_i == this) _i = null; }
        void Init()
        {
            for (int n = 0; n < Max; n++)
            {
                var sr = new GameObject("p").AddComponent<SpriteRenderer>(); sr.transform.SetParent(transform, false);
                sr.gameObject.SetActive(false); srs[n] = sr; _free[_freeN++] = Max - 1 - n;
            }
            _filter = new ContactFilter2D(); _filter.useTriggers = true;
        }
        internal int Spawn(in ProjShot s, Vector2 pos, Vector2 dir, Transform owner, int gen)
        {
            if (_freeN == 0) return -1;
            int id = _free[--_freeN]; ref var p = ref ps[id];
            p.alive = true; p.s = s; p.pos = pos; p.last = pos; p.vel = dir.normalized * (s.speedPx / HellFX.PPU); p.age = 0; p.trailAcc = 0; p.rot = 0;
            p.pierceLeft = s.pierce; p.bounceLeft = s.bounce; p.gen = gen; p.owner = owner; p.lastHitId = 0;
            var sr = srs[id]; sr.sprite = ProjSprites.Body(s.element, 0); sr.color = Color.white; sr.sortingOrder = s.sortingOrder;
            var am = ProjectileFX.Glows(s.element) ? PixelFXMaterials.Additive : null; // 2 shared materials total -> projectiles batch
            sr.sharedMaterial = am != null ? am : HellFX.DefaultSpriteMaterial(sr);
            sr.transform.position = pos; sr.transform.localScale = Vector3.one; sr.gameObject.SetActive(true);
            return id;
        }
        internal void Kill(int id) { if (id < 0 || !ps[id].alive) return; ps[id].alive = false; ps[id].owner = null; srs[id].gameObject.SetActive(false); _free[_freeN++] = id; }
        void Update()
        {
            float dt = Time.deltaTime, U = 1f / HellFX.PPU;
            _filter.SetLayerMask(enemyMask); _filter.useLayerMask = true;
            for (int id = 0; id < Max; id++)
            {
                if (!ps[id].alive) continue;
                ref var p = ref ps[id];
                p.age += dt;
                if (p.age >= p.s.lifeSeconds) { ProjectileFX.Fizzle(p.pos, p.s.element); Kill(id); continue; }
                // steering
                if (p.s.boomerang)
                {
                    float spd = p.s.speedPx * U; float k = p.age / p.s.lifeSeconds;
                    if (k > 0.45f && p.owner)
                    {
                        Vector2 to = (Vector2)p.owner.position - p.pos;
                        if (to.sqrMagnitude < 0.04f) { Kill(id); continue; }
                        p.vel = Vector2.Lerp(p.vel, to.normalized * spd * 1.3f, dt * 6f);
                    }
                    else p.vel *= 1f - dt * 1.2f;
                }
                else if (p.s.homingDegPerSec > 0 && ProjectileFX.FindTarget != null)
                {
                    if (ProjectileFX.FindTarget(p.pos, p.s.homingRadiusPx * U, out var tgt))
                    {
                        float a = Vector2.SignedAngle(p.vel, tgt - p.pos); float m = Mathf.Clamp(a, -p.s.homingDegPerSec * dt, p.s.homingDegPerSec * dt);
                        p.vel = Quaternion.Euler(0, 0, m) * p.vel;
                    }
                }
                p.last = p.pos; p.pos += p.vel * dt;
                // trail
                p.trailAcc += (p.pos - p.last).magnitude * HellFX.PPU;
                while (p.trailAcc >= 3f) { p.trailAcc -= 3f; ProjectileFX.TrailPixel(Vector2.Lerp(p.last, p.pos, 0.5f), p.s.element, p.s.sortingOrder - 1); }
                // draw (pixel snapped, 4-frame flicker, face velocity or spin)
                var sr = srs[id]; var t = sr.transform;
                t.position = new Vector3(Mathf.Round(p.pos.x * HellFX.PPU) * U, Mathf.Round(p.pos.y * HellFX.PPU) * U, 0);
                sr.sprite = ProjSprites.Body(p.s.element, (int)(p.age * 14f) & 3);
                if (p.s.spinDegPerSec != 0) p.rot += p.s.spinDegPerSec * dt; else p.rot = Mathf.Atan2(p.vel.y, p.vel.x) * Mathf.Rad2Deg;
                t.rotation = Quaternion.Euler(0, 0, Mathf.Round(p.rot / 22.5f) * 22.5f); // 16 directions keeps pixels crisp
                // hits
                int n = Physics2D.OverlapCircle(p.pos, p.s.radiusPx * U, _filter, _hits);
                for (int h = 0; h < n; h++)
                {
                    var col = _hits[h]; int hid = col.GetInstanceID();
                    if (hid == p.lastHitId || (p.owner && col.transform == p.owner)) continue;
                    p.lastHitId = hid;
                    ProjectileFX.OnHit?.Invoke(col, p.s.damage, p.s.element);
                    if (p.s.split > 0 && p.gen == 0) { ProjectileFX.SplitAt(p.pos, p.vel, p.s, p.owner); ProjectileFX.HitSplit(p.pos, p.s.element); Kill(id); break; }
                    if (p.bounceLeft > 0)
                    {
                        p.bounceLeft--; ProjectileFX.HitBounce(p.pos, p.s.element);
                        Vector2 nd = ProjectileFX.FindTarget != null && ProjectileFX.FindTarget(p.pos + p.vel.normalized * 0.6f, 80 * U, out var nt) ? (nt - p.pos) : Vector2.Reflect(p.vel, (p.pos - (Vector2)col.transform.position).normalized);
                        p.vel = nd.normalized * p.vel.magnitude; break;
                    }
                    if (p.pierceLeft != 0) { if (p.pierceLeft > 0) p.pierceLeft--; ProjectileFX.HitPierce(p.pos, p.vel, p.s.element); continue; }
                    ProjectileFX.Impact(p.pos, p.s.element, p.vel); Kill(id); break;
                }
            }
        }
    }

    /// <summary>Code-made projectile sprites (4 flicker frames each, white core -> element ramp, 1px dark outline).</summary>
    public static class ProjSprites
    {
        static readonly Sprite[,] _body = new Sprite[9, 4];
        static Sprite _trail;
        public static Color32[] Ramp(ProjElement e)
        {
            switch (e)
            {
                case ProjElement.Fire: return new Color32[] { new Color32(255, 246, 220, 255), new Color32(255, 222, 96, 255), new Color32(240, 120, 30, 255), new Color32(178, 24, 32, 255) };
                case ProjElement.Blood: return new Color32[] { new Color32(255, 192, 192, 255), new Color32(255, 90, 90, 255), new Color32(200, 24, 40, 255), new Color32(92, 12, 22, 255) };
                case ProjElement.Bone: return new Color32[] { new Color32(255, 255, 255, 255), new Color32(236, 226, 206, 255), new Color32(180, 166, 150, 255), new Color32(90, 80, 80, 255) };
                case ProjElement.Soul: return new Color32[] { new Color32(240, 255, 255, 255), new Color32(140, 230, 255, 255), new Color32(60, 140, 230, 255), new Color32(30, 30, 90, 255) };
                case ProjElement.Curse: return new Color32[] { new Color32(240, 220, 255, 255), new Color32(190, 120, 240, 255), new Color32(120, 50, 170, 255), new Color32(50, 20, 70, 255) };
                case ProjElement.Hellfire: return new Color32[] { new Color32(255, 255, 230, 255), new Color32(255, 200, 60, 255), new Color32(255, 84, 24, 255), new Color32(120, 16, 28, 255) };
                case ProjElement.Lightning: return new Color32[] { new Color32(255, 255, 255, 255), new Color32(255, 250, 160, 255), new Color32(150, 220, 255, 255), new Color32(60, 80, 200, 255) };
                case ProjElement.Arcane: return new Color32[] { new Color32(255, 240, 255, 255), new Color32(255, 140, 220, 255), new Color32(200, 60, 200, 255), new Color32(80, 20, 90, 255) };
                default: return new Color32[] { new Color32(255, 255, 255, 255), new Color32(220, 228, 240, 255), new Color32(130, 140, 160, 255), new Color32(50, 50, 70, 255) };
            }
        }
        static float H(int x, int y, int f) { uint h = (uint)(x * 374761393 + y * 668265263 + f * 1442695041); h = (h ^ (h >> 13)) * 1274126177; return (h & 0xffff) / 65535f; }
        /// <summary>Body sprite facing +X. Frame 0-3.</summary>
        public static Sprite Body(ProjElement e, int frame)
        {
            int ei = (int)e; frame &= 3;
            if (_body[ei, frame]) return _body[ei, frame];
            var r = Ramp(e); var k = new Color32(18, 6, 10, 255); var clear = new Color32(0, 0, 0, 0);
            int w = 16, h = 16; var px = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                float dx = x - 7.5f, dy = y - 7.5f; int v = -1; bool outline = false;
                switch (e)
                {
                    case ProjElement.Fire: case ProjElement.Hellfire:
                    {   // teardrop head with flickering tail (Hellfire = long spear)
                        bool spear = e == ProjElement.Hellfire; float hx = spear ? 11 : 9;
                        float d = Mathf.Sqrt((x - hx) * (x - hx) + dy * dy * (spear ? 2.2f : 1f));
                        float tail = (hx - x) * (spear ? 0.18f : 0.35f); float rr = (spear ? 2.2f : 3.6f) - Mathf.Max(0, tail) + (H(x, y, frame) - 0.5f) * 1.2f;
                        if (x <= hx && Mathf.Abs(dy) < rr && x > (spear ? 0 : 2)) d = Mathf.Abs(dy) / Mathf.Max(rr, 0.1f) * 3.6f;
                        if (spear && x >= 11 && x <= 15 && Mathf.Abs(dy) <= (15 - x) * 0.6f) d = 0.5f; // spear tip
                        v = d < 1.2f ? 0 : d < 2.2f ? 1 : d < 3f ? 2 : d < 3.8f ? 3 : -1; outline = d >= 3.8f && d < 4.6f;
                        break;
                    }
                    case ProjElement.Blood:
                    {   // glossy elongated drop
                        float d = Mathf.Sqrt(dx * dx * 0.45f + dy * dy); v = d < 1f ? 0 : d < 2f ? 1 : d < 2.8f ? 2 : d < 3.4f ? 3 : -1; outline = d >= 3.4f && d < 4.1f;
                        if (v > 0 && x == 9 + (frame & 1) && y == 9) v = 0; break;
                    }
                    case ProjElement.Bone:
                    {   // bone shard: shaft with knobs
                        bool shaft = Mathf.Abs(dy) < 1.1f && Mathf.Abs(dx) < 5.5f; bool knob = (Mathf.Abs(Mathf.Abs(dx) - 5.5f) < 1.5f) && Mathf.Abs(Mathf.Abs(dy) - 1.2f) < 1.2f;
                        if (shaft || knob) v = dy > 0.5f ? 0 : dy < -0.5f ? 2 : 1;
                        bool near = !shaft && !knob && (Mathf.Abs(dy) < 2.1f && Mathf.Abs(dx) < 6.5f || (Mathf.Abs(Mathf.Abs(dx) - 5.5f) < 2.5f && Mathf.Abs(Mathf.Abs(dy) - 1.2f) < 2.2f)); outline = near; break;
                    }
                    case ProjElement.Soul: case ProjElement.Arcane:
                    {   // orb with orbiting sparkle
                        float d = Mathf.Sqrt(dx * dx + dy * dy); v = d < 1.5f ? 0 : d < 2.8f ? 1 : d < 3.8f ? 2 : d < 4.6f ? 3 : -1;
                        float a = frame * 1.5708f; if (Mathf.Abs(x - (7.5f + Mathf.Cos(a) * 6)) < 0.8f && Mathf.Abs(y - (7.5f + Mathf.Sin(a) * 6)) < 0.8f) v = 0;
                        if (e == ProjElement.Arcane && v < 0 && Mathf.Abs(dy) < 1 && dx < -4 && dx > -8) v = 2; break;
                    }
                    case ProjElement.Curse:
                    {   // flaming purple skull
                        bool skull = (dx * dx / 16f + (dy - 0.5f) * (dy - 0.5f) / 14f) < 1 || (Mathf.Abs(dx) < 2.6f && dy < -2 && dy > -5);
                        bool eye = (Mathf.Abs(Mathf.Abs(dx) - 1.6f) < 0.9f && Mathf.Abs(dy - 0.3f) < 1f); bool flame = !skull && dx < -3 && Mathf.Abs(dy) < (dx + 9) * 0.6f && H(x, y, frame) > 0.35f;
                        if (skull) v = eye ? -2 : dy > 2 ? 1 : 2; else if (flame) v = x < 3 ? 3 : 2; break;
                    }
                    case ProjElement.Lightning:
                    {   // jagged bolt segment, re-rolled each frame
                        float zig = (H(x, 0, frame) - 0.5f) * 3f + Mathf.Sin(x * 1.3f + frame) * 1.2f; float d = Mathf.Abs(dy - zig);
                        v = d < 0.7f ? 0 : d < 1.4f ? 1 : d < 2.2f && H(x, y, frame) > 0.5f ? 2 : -1; break;
                    }
                    default:
                    {   // spinning crescent blade
                        float d = Mathf.Sqrt(dx * dx + dy * dy), d2 = Mathf.Sqrt((dx + 2.5f) * (dx + 2.5f) + dy * dy);
                        if (d < 6.5f && d2 > 5f) v = d > 5.5f ? 0 : d > 4f ? 1 : 2; outline = v < 0 && d < 7.3f && d2 > 4.3f; break;
                    }
                }
                px[y * w + x] = v == -2 ? k : v >= 0 ? r[v] : outline ? k : clear;
            }
            return _body[ei, frame] = PixelSpriteFactory.FromPixels(px, w, h, HellFX.PPU, "proj_" + e + frame);
        }
        public static Sprite TrailPixel() => _trail ? _trail : (_trail = PixelSpriteFactory.Rect(1, 1, new Color32(255, 255, 255, 255), HellFX.PPU, "projpx"));
    }

    /// <summary>Static API: projectile weapons, cast flashes, trails, impacts, pierce/bounce/split hit FX, pattern helpers.</summary>
    public static class ProjectileFX
    {
        /// <summary>Called when a projectile overlaps a collider. Hook your damage here (no allocation per hit).</summary>
        public static Action<Collider2D, float, ProjElement> OnHit;
        /// <summary>Optional target finder for homing/bounce retargeting: (from, radiusWorld, out target) -> found. Write it allocation-free (e.g. spatial grid).</summary>
        public delegate bool TargetFinder(Vector2 from, float radius, out Vector2 target);
        public static TargetFinder FindTarget;
        /// <summary>Global juice scales (0 turns a hook off). Hit-stop is real-time and only fires on big impacts.</summary>
        public static float ShakeScale = 1f, HitStopScale = 1f, FlashScale = 1f;
        static ProjectileFXRunner R => ProjectileFXRunner.I;
        static HellFXRunner H => HellFXRunner.I;
        static Color C(Color32 c) => c;
        static float Step(float k, int n) => Mathf.Floor(k * n) / n;
        internal static bool Glows(ProjElement e) => e != ProjElement.Bone && e != ProjElement.Blood && e != ProjElement.Blade;

        /// <summary>Tuned defaults per weapon type (speed, pierce, spin...).</summary>
        public static ProjShot Preset(ProjElement e)
        {
            var s = new ProjShot { element = e, speedPx = 160, lifeSeconds = 1.6f, damage = 1, radiusPx = 4, sortingOrder = 58 };
            switch (e)
            {
                case ProjElement.Bone: s.spinDegPerSec = 900; s.speedPx = 140; break;
                case ProjElement.Soul: s.homingDegPerSec = 240; s.homingRadiusPx = 90; s.speedPx = 110; s.lifeSeconds = 2.5f; break;
                case ProjElement.Curse: s.speedPx = 80; s.pierce = -1; s.lifeSeconds = 2.5f; s.radiusPx = 6; break;
                case ProjElement.Hellfire: s.speedPx = 260; s.pierce = 3; break;
                case ProjElement.Lightning: s.speedPx = 320; s.bounce = 4; s.lifeSeconds = 0.9f; break;
                case ProjElement.Arcane: s.homingDegPerSec = 420; s.homingRadiusPx = 120; s.speedPx = 150; s.lifeSeconds = 2.2f; break;
                case ProjElement.Blade: s.boomerang = true; s.spinDegPerSec = -1080; s.pierce = -1; s.speedPx = 200; s.lifeSeconds = 1.4f; s.radiusPx = 6; break;
                case ProjElement.Blood: s.speedPx = 200; break;
            }
            return s;
        }

        // 75-83 + 85-86: fire one projectile (cast flash included). Returns a handle id or -1 if the pool is full.
        public static int Fire(in ProjShot s, Vector2 pos, Vector2 dir, Transform owner = null, bool castFlash = true)
        {
            if (castFlash) CastFlash(pos, dir, s.element, s.sortingOrder + 2);
            return R.Spawn(s, pos, dir, owner, 0);
        }
        public static int Fireball(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Fire), pos, dir, owner);
        public static int BloodBolt(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Blood), pos, dir, owner);
        public static int BoneShard(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Bone), pos, dir, owner);
        public static int SoulOrb(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Soul), pos, dir, owner);
        public static int CurseSkull(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Curse), pos, dir, owner);
        public static int HellfireSpear(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Hellfire), pos, dir, owner);
        public static int LightningBolt(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Lightning), pos, dir, owner);
        public static int MagicMissile(Vector2 pos, Vector2 dir, Transform owner = null) => Fire(Preset(ProjElement.Arcane), pos, dir, owner);
        public static int ReturningBlade(Vector2 pos, Vector2 dir, Transform owner) => Fire(Preset(ProjElement.Blade), pos, dir, owner);
        public static int Bouncing(ProjElement e, int bounces, Vector2 pos, Vector2 dir, Transform owner = null) { var s = Preset(e); s.bounce = bounces; return Fire(s, pos, dir, owner); }
        public static int Splitting(ProjElement e, int children, Vector2 pos, Vector2 dir, Transform owner = null) { var s = Preset(e); s.split = children; s.pierce = 0; return Fire(s, pos, dir, owner); }
        public static void Kill(int handle) => R.Kill(handle);

        internal static void SplitAt(Vector2 pos, Vector2 vel, ProjShot s, Transform owner)
        {
            int n = s.split; s.split = 0; s.speedPx *= 0.9f; s.lifeSeconds *= 0.5f;
            float baseA = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg, spread = 360f / n;
            for (int i = 0; i < n; i++) { float a = (baseA + spread * (i + 0.5f)) * Mathf.Deg2Rad; R.Spawn(s, pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), owner, 1); }
        }

        // 91: pattern helpers --------------------------------------------------------------------------------
        /// <summary>Fan of <paramref name="count"/> shots across <paramref name="arcDeg"/>.</summary>
        public static void Spread(in ProjShot s, Vector2 pos, Vector2 dir, int count, float arcDeg, Transform owner = null)
        {
            float a0 = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - arcDeg * 0.5f, st = count > 1 ? arcDeg / (count - 1) : 0;
            CastFlash(pos, dir, s.element, s.sortingOrder + 2);
            for (int i = 0; i < count; i++) { float a = (a0 + st * i) * Mathf.Deg2Rad; R.Spawn(s, pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), owner, 0); }
        }
        /// <summary>Full ring of shots (nova volley).</summary>
        public static void Ring(in ProjShot s, Vector2 pos, int count, float offsetDeg = 0, Transform owner = null)
        {
            CastRing(pos, s.element, s.sortingOrder + 2);
            for (int i = 0; i < count; i++) { float a = (offsetDeg + 360f * i / count) * Mathf.Deg2Rad; R.Spawn(s, pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), owner, 0); }
        }
        /// <summary>Call every frame (or every N seconds) with a growing <paramref name="t"/>: arms x rotating stream = spiral.</summary>
        public static void SpiralStep(in ProjShot s, Vector2 pos, float t, int arms = 3, float degPerSec = 220, Transform owner = null)
        {
            for (int i = 0; i < arms; i++) { float a = (t * degPerSec + 360f * i / arms) * Mathf.Deg2Rad; R.Spawn(s, pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a)), owner, 0); }
        }
        /// <summary>Homing volley: shots leave in a wide fan then curve into targets (needs FindTarget).</summary>
        public static void HomingVolley(ProjElement e, Vector2 pos, Vector2 dir, int count, Transform owner = null)
        {
            var s = Preset(e); if (s.homingDegPerSec <= 0) { s.homingDegPerSec = 300; s.homingRadiusPx = 120; }
            Spread(s, pos, dir, count, 150, owner);
        }

        // 84: piercing beam ------------------------------------------------------------------------------------
        static readonly RaycastHit2D[] _beamHits = new RaycastHit2D[32];
        /// <summary>Instant piercing beam: 2-frame white core, element-coloured outer, sparks along the line, damage to all hit colliders.</summary>
        public static void Beam(Vector2 from, Vector2 dir, float lengthPx, ProjElement e, float damage = 1, float widthPx = 5, float seconds = 0.22f, int order = 60)
        {
            float U = 1f / HellFX.PPU, len = lengthPx * U; dir.Normalize(); var r = ProjSprites.Ramp(e);
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; Vector3 mid = from + dir * len * 0.5f;
            var px = ProjSprites.TrailPixel();
            for (int layer = 0; layer < 2; layer++)
            {
                var j = H.Spawn(px, mid, layer == 0 ? C(r[2]) : C(r[0]), order + layer, seconds, true, BeamTick);
                if (j == null) continue;
                j.sr.transform.rotation = Quaternion.Euler(0, 0, ang); j.f = lengthPx; j.g = layer == 0 ? widthPx : Mathf.Max(1, widthPx * 0.4f);
                j.sr.transform.localScale = new Vector3(j.f, j.g, 1);
            }
            CastFlash(from, dir, e, order + 2);
            var cf = new ContactFilter2D(); cf.useTriggers = true; cf.SetLayerMask(R.enemyMask);
            int n = Physics2D.Raycast(from, dir, cf, _beamHits, len);
            for (int i = 0; i < n; i++) { OnHit?.Invoke(_beamHits[i].collider, damage, e); HitPierce(_beamHits[i].point, dir, e); }
            for (int i = 0; i < 6; i++) PixelParticles.Burst(Sparks(e, 2), from + dir * len * (i / 6f));
            ShakeScaled(1.5f, 0.1f); Kick(dir * -1.5f);
        }
        static readonly Action<HellFXRunner.Job, float> BeamTick = (j, k) =>
        {
            float w = j.g * (k < 0.15f ? 1.4f : 1f - Step(k, 4) * 0.9f); j.sr.transform.localScale = new Vector3(j.f, Mathf.Max(1, Mathf.Round(w)), 1);
            j.sr.color = new Color(j.c.r, j.c.g, j.c.b, 1 - Step(k, 3));
        };

        // 87: cast / spawn flash --------------------------------------------------------------------------
        static readonly Action<HellFXRunner.Job, float> PopTick = (j, k) =>
        {   // 2-frame white pop, then element colour, stepped shrink
            j.sr.color = k < 0.34f ? new Color(1.5f, 1.5f, 1.5f, 1) : new Color(j.c.r, j.c.g, j.c.b, 1 - Step(k, 3));
            float s = j.f * (1 - Step(k, 4) * 0.7f); j.sr.transform.localScale = new Vector3(s, s, 1);
        };
        public static void CastFlash(Vector2 pos, Vector2 dir, ProjElement e, int order = 60)
        {
            var r = ProjSprites.Ramp(e);
            var j = H.Spawn(PixelFXTextures.Glow(5, 4, HellFX.PPU), pos + dir.normalized * 0.25f, C(r[1]), order, 0.09f, true, PopTick); if (j != null) j.f = FlashScale;
            PixelParticles.Burst(Sparks(e, 4, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, 50), pos);
            Kick(dir.normalized * -0.5f);
        }
        static void CastRing(Vector2 pos, ProjElement e, int order)
        {
            var r = ProjSprites.Ramp(e);
            var j = H.Spawn(PixelFXTextures.Ring(10, 2, HellFX.PPU), pos, C(r[1]), order, 0.18f, true, RingTick); if (j != null) j.f = 0.4f;
            PixelParticles.Burst(Sparks(e, 10), pos); ShakeScaled(1f, 0.08f);
        }
        static readonly Action<HellFXRunner.Job, float> RingTick = (j, k) => { float s = j.f + Step(k, 6) * 1.2f; j.sr.transform.localScale = new Vector3(s, s, 1); j.sr.color = new Color(j.c.r, j.c.g, j.c.b, 1 - Step(k, 3)); };

        // 88: element trails --------------------------------------------------------------------------------
        static readonly Action<HellFXRunner.Job, float> TrailTick = (j, k) =>
        {   // 2px -> 1px, hot -> element -> dark, stepped fade; i = element
            var r = ProjSprites.Ramp((ProjElement)j.i); int idx = Mathf.Min(3, 1 + (int)(k * 3));
            Color c = r[idx]; float s = k < 0.35f ? 2 : 1; j.sr.transform.localScale = new Vector3(s, s, 1);
            j.sr.color = new Color(c.r, c.g, c.b, 1 - Step(k, 3));
            if ((ProjElement)j.i == ProjElement.Fire || (ProjElement)j.i == ProjElement.Hellfire || (ProjElement)j.i == ProjElement.Curse) j.sr.transform.position += new Vector3(0, Time.deltaTime * 0.6f, 0); // flames rise
            else if ((ProjElement)j.i == ProjElement.Blood) j.sr.transform.position -= new Vector3(0, Time.deltaTime * 0.5f, 0); // blood drips
        };
        internal static void TrailPixel(Vector2 pos, ProjElement e, int order)
        {
            if (e == ProjElement.Blade) return;
            var j = H.Spawn(ProjSprites.TrailPixel(), pos, Color.white, order, e == ProjElement.Lightning ? 0.08f : 0.26f, Glows(e), TrailTick);
            if (j != null) j.i = (int)e;
        }

        // 89: impact / explosion per element ------------------------------------------------------------------------
        /// <summary>Death-of-projectile impact: flash pop, ring, element debris, shake/kick; Fire/Hellfire/Curse explode bigger.</summary>
        public static bool SkipNova928; // t928u: Hellord shader blast replaces the nova puffs
        public static bool SkipPackFlash988; // t988u/t1001u: skip soft Glow+Ring wash (keeps HellfireNova / sparks)
        public static bool SkipHitStop; // t1004u: Hellord sets for Fireball spam (no freeze stack)
        public static float NovaRadiusPx; // t1001u: 0 = default 14; set to splash radius px for Fireball
        public static void Impact(Vector2 pos, ProjElement e, Vector2 vel, int order = 64)
        {
            var r = ProjSprites.Ramp(e); bool big = e == ProjElement.Fire || e == ProjElement.Hellfire || e == ProjElement.Curse;
            bool noWash = SkipPackFlash988 && big; // t1001u: fire blast without soft Glow/Ring disc (nova puffs stay unless SkipNova928)
            if (!noWash)
            {
                var j = H.Spawn(PixelFXTextures.Glow(big ? 9 : 5, 4, HellFX.PPU), pos, C(r[1]), order, big ? 0.16f : 0.1f, true, PopTick); if (j != null) j.f = FlashScale;
                var ring = H.Spawn(PixelFXTextures.Ring(big ? 8 : 5, 1, HellFX.PPU), pos, C(r[2]), order - 1, 0.2f, true, RingTick); if (ring != null) ring.f = 0.5f;
            }
            else
            {   // tiny 1-frame white core under the nova so the hit still pops
                var j = H.Spawn(PixelFXTextures.Glow(3, 2, HellFX.PPU), pos, C(r[0]), order, 0.06f, true, PopTick); if (j != null) j.f = 0.35f;
            }
            PixelParticles.Burst(Sparks(e, noWash ? 10 : (big ? 14 : 7), Mathf.Atan2(-vel.y, -vel.x) * Mathf.Rad2Deg, 140), pos);
            if (e == ProjElement.Blood) PixelParticles.Burst(BloodDrops(6), pos);
            if (e == ProjElement.Bone) PixelParticles.Burst(BoneChips(5), pos);
            if (big) { if (!SkipNova928) HellFX.HellfireNova(pos, NovaRadiusPx > 0 ? NovaRadiusPx : 14, 0.28f, false, order - 2, lightPop: !noWash); ShakeScaled(2f, 0.12f); if (!SkipHitStop) HitStopScaled(0.03f); }
            else ShakeScaled(0.75f, 0.06f);
            Kick(vel.normalized * 0.75f);
            NovaRadiusPx = 0;
        }
        internal static void Fizzle(Vector2 pos, ProjElement e) { PixelParticles.Burst(Sparks(e, 3), pos); }

        // 90: pierce / bounce / split hit FX -------------------------------------------------------------------------
        /// <summary>Pierce: thin streak through the target in the travel direction + small sparks (no shake, it spams).</summary>
        public static void HitPierce(Vector2 pos, Vector2 vel, ProjElement e, int order = 63)
        {
            var r = ProjSprites.Ramp(e); var j = H.Spawn(ProjSprites.TrailPixel(), pos, C(r[0]), order, 0.08f, true, StreakTick);
            if (j != null) { j.sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg); j.sr.transform.localScale = new Vector3(10, 1, 1); }
            PixelParticles.Burst(Sparks(e, 4, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg, 40), pos);
        }
        static readonly Action<HellFXRunner.Job, float> StreakTick = (j, k) => { j.sr.transform.localScale = new Vector3(Mathf.Round(10 * (1 - k)) + 1, 1, 1); j.sr.color = new Color(j.c.r, j.c.g, j.c.b, 1 - Step(k, 2)); };
        /// <summary>Bounce: 1-frame white star + 360 spark ring + tiny shake.</summary>
        public static void HitBounce(Vector2 pos, ProjElement e, int order = 63)
        {
            var j = H.Spawn(PixelFXTextures.Ring(3, 1, HellFX.PPU), pos, Color.white, order, 0.1f, true, RingTick); if (j != null) j.f = 0.6f;
            PixelParticles.Burst(Sparks(e, 6), pos); ShakeScaled(0.5f, 0.04f);
        }
        /// <summary>Split: bright core pop + ring, the children leave from here.</summary>
        public static void HitSplit(Vector2 pos, ProjElement e, int order = 64)
        {
            var r = ProjSprites.Ramp(e);
            var j = H.Spawn(PixelFXTextures.Glow(7, 4, HellFX.PPU), pos, C(r[0]), order, 0.12f, true, PopTick); if (j != null) j.f = FlashScale;
            var ring = H.Spawn(PixelFXTextures.Ring(6, 1, HellFX.PPU), pos, C(r[1]), order - 1, 0.18f, true, RingTick); if (ring != null) ring.f = 0.4f;
            PixelParticles.Burst(Sparks(e, 10), pos); ShakeScaled(1f, 0.06f);
        }

        // juice hooks ---------------------------------------------------------------------------------------
        static void ShakeScaled(float px, float s) { if (ShakeScale > 0) ScreenShake.Add(px * ShakeScale, s); }
        static void HitStopScaled(float s) { if (!SkipHitStop && HitStopScale > 0 && !HitStop.Active) HitStop.Play(s * HitStopScale); }
        static void Kick(Vector2 px) { if (ShakeScale > 0 && ScreenShake.Main) ScreenShake.Main.Kick(px * ShakeScale); }

        // cached particle settings per element (no allocation after first use) -----------------------------------
        static readonly PixelBurstSettings[] _sparks = new PixelBurstSettings[9];
        static PixelBurstSettings _blood, _bone;
        static Gradient G(Color32[] r) { var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(r[0], 0), new GradientColorKey(r[1], 0.3f), new GradientColorKey(r[2], 0.65f), new GradientColorKey(r[3], 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }); return g; }
        static PixelBurstSettings Sparks(ProjElement e, int n, float dirDeg = 0, float spread = 360)
        {
            ref var s = ref _sparks[(int)e];
            if (s == null) s = new PixelBurstSettings { speed = new Vector2(2f, 5.5f), lifetime = new Vector2(0.12f, 0.3f), drag = 6, sizePixels = new Vector2Int(1, 2), colorOverLife = G(ProjSprites.Ramp(e)), colorSteps = 3, additive = Glows(e) };
            s.count = n; s.directionDegrees = dirDeg; s.spreadDegrees = spread; return s;
        }
        static PixelBurstSettings BloodDrops(int n) { if (_blood == null) _blood = new PixelBurstSettings { speed = new Vector2(1.5f, 3.5f), spreadDegrees = 360, lifetime = new Vector2(0.3f, 0.55f), gravity = 8, drag = 2, sizePixels = new Vector2Int(1, 2), colorOverLife = G(ProjSprites.Ramp(ProjElement.Blood)), colorSteps = 3 }; _blood.count = n; return _blood; }
        static PixelBurstSettings BoneChips(int n) { if (_bone == null) _bone = new PixelBurstSettings { speed = new Vector2(1.5f, 3f), directionDegrees = 90, spreadDegrees = 160, lifetime = new Vector2(0.35f, 0.6f), gravity = 10, drag = 1, sizePixels = new Vector2Int(1, 2), colorOverLife = G(ProjSprites.Ramp(ProjElement.Bone)), colorSteps = 3 }; _bone.count = n; return _bone; }
    }
}
