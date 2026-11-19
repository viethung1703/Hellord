using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// =====================================================================================================================
// t1567 (VH: "Do all skills that I picked, ask HA and HFX to do with you"): 7 new LEFT-CLICK basics for Leizu and Xingtian.
// They ride the t1517 basic path unchanged (lmbBasicIds1517 / IsLmbBasic1563: fire only while LMB is held = no auto-fire, own cooldown
// with the cursor ring, HUD sweep and not-ready click, instant response, face the cursor, the t1528 tail cast). Spells leave the tail tip.
// Everything is D2-style SCRIPTED motion (no physics bodies): positions are functions of time, hits are distance queries.
//   Leizu:    Spark Spray (SkillId.SparkSpray), Ball Lightning (BallLightning), Thunder Javelin (ThunderJavelin)
//   Xingtian: Headsman's Chop (HeadsmansChop), Earth-Splitting Slam (EarthSplittingSlam), Twin Axe Throw (TwinAxeThrow), Phantom Axes (PhantomAxes)
// Every number lives in HellordTuning (t1567 section). FIRST-PASS balance.
// Visuals: code-drawn PLACEHOLDER pixel shapes (existing palette) unless an HA / HFX strip exists at
//   Resources/HellordArt/Skills1567/<skill>/<name>.png  (horizontal strip; frame = square of the height unless pbStripFrames1567 says
//   "<skill>/<name>:frameW:pivotX:pivotY:msPerFrame"). PbStrip1567 is the only loader, so dropping a strip in is a file copy (+ one table line).
// Strip names looked for: spark_spray/spark, ball_lightning/orb, thunder_javelin/javelin, thunder_javelin/burst, headsmans_chop/arc,
//   headsmans_chop/blood_wave, earth_slam/impact, earth_slam/cracks, earth_slam/erupt, twin_axe/axe, phantom_axes/ghost, phantom_axes/arc,
//   icons/icon_<key>_24 (key = spark_spray, ball_lightning, thunder_javelin, headsmans_chop, earth_slam, twin_axe, phantom_axes).
// =====================================================================================================================
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool IsPb1567(SkillId id) => id >= SkillId.SparkSpray && id <= SkillId.PhantomAxes;
        static int PbIx1567(SkillId id) => (int)id - (int)SkillId.SparkSpray;
        static readonly string[] PbKey1567 = { "spark_spray", "ball_lightning", "thunder_javelin", "headsmans_chop", "earth_slam", "twin_axe", "phantom_axes" };
        static readonly string[] PbIconName1567 = { "sparkspray", "balllightning", "thunderjavelin", "headsmanschop", "earthsplittingslam", "twinaxethrow", "phantomaxes" }; // HA icons/icon_<name>_24.png
        bool PbOn1567(SkillId id)
        {
            var T = HellordTuning.I; if (T == null || !T.pbOn1567) return false; int i = PbIx1567(id);
            return T.pbSkillOn1567 == null || i >= T.pbSkillOn1567.Length || T.pbSkillOn1567[i];
        }
        /// <summary>level-up pools leave the 7 out (they are start-pick basics; taken later they would have no LMB).</summary>
        bool PbNoOffer1567(SkillDef d) => IsPb1567(d.id) && HellordTuning.I.pbNoLevelUp1567;
        /// <summary>damage / cooldown of the 7 SkillDefs come from HellordTuning (pbBaseDmg1567 / pbCd1567).</summary>
        void PbDefs1567()
        {
            var T = HellordTuning.I; if (T == null) return;
            foreach (var d in HData.Skills) { if (!IsPb1567(d.id)) continue; int i = PbIx1567(d.id); if (T.pbBaseDmg1567 != null && i < T.pbBaseDmg1567.Length) d.baseDmg = T.pbBaseDmg1567[i]; if (T.pbCd1567 != null && i < T.pbCd1567.Length) d.cooldown = T.pbCd1567[i]; }
        }

        // ---------------------------------------------------------------- test counters (public, read by HellordAutoTest1567)
        public readonly int[] PbCasts1567 = new int[7], PbHits1567 = new int[7];
        public int SparkLastN1567, SparkSpawned1567, OrbAlive1567, OrbPeak1567, OrbZaps1567, OrbPopped1567, JavBursts1567, JavForkHits1567, JavFizzles1567,
            ChopHits1567, ChopWaves1567, ChopWaveHits1567, SlamImpacts1567, SlamErupts1567, SlamEruptHits1567, SlamSlowed1567, SlamCracksPeak1567, SlamCracks1567,
            AxeThrows1567, AxeOutHits1567, AxeBackHits1567, AxeRefused1567, AxeCatches1567, PhSpawned1567, PhLastN1567, PhHits1567;
        public int SparkJits1567, SparkBounces1567, SparkChaosN1567, JavChainJumps1567, JavChainMax1567, JavChains1567; public float SparkMaxTurnDeg1567, JavChainLastMul1567 = 1f, JavChainMaxJumpU1567; public int JavChainLastL1567, ChopHitStops1567; public bool ChopV2Last1567; public float ChopLastHalf1567; // t1578 / t1579
        public bool AxeOut1567 => _pbAxe != null;
        public Vector2 SlamLastAt1567; public float SlamLastHeroMove1567;

        public float ChopLastR1567, ChopArtScale1567 = 1f, PhLastR1567, PhArtScale1567 = 1f;
        // ---------------------------------------------------------------- entities
        class PbSpark { public Vector2 a, dir, nrm, p; public float dist, range, t, wob, hz, ph, z0, dmg, aimA, hr, life, stepT, trailT, baseA, jitT; public int idx, step, seed, jn, bounces; public bool steer, chaos; public Vector2 v; public SkillInst s; }
        class PbOrb { public Vector2 a, dir, p; public float dist, total, zap, t, dmg, die = -1f, trailT; public SkillInst s; }
        class PbJav { public Vector2 a, dir, p; public float dist, range, dmg, t; public SkillInst s; }
        class PbLine { public List<Vector2> pts = new List<Vector2>(); public float t, dur; public Color c; public int order; public int w = 1; }
        class PbArc { public Vector2 c, dir; public float r, half, t, sweep, fade; public Color c1, c2; public int order; public string strip; }
        class PbRing { public Vector2 c; public float r0, r1, t, dur, delay; public Color col; public int order; public string strip; }
        class PbCrack { public PbFis fis; public float r = -1f, rot, eruptAt = -1f; public Vector2 p; public float t, life, dmg; public bool erupted; public SkillInst s; public readonly HashSet<int> slowed = new HashSet<int>(); public int seed; }
        class PbAxe { public Vector2 a, b, p, vel; public int phase; public float t, dmg, spin, life; public SkillInst s; public readonly HashSet<int> hitOut = new HashSet<int>(), hitBack = new HashSet<int>(); }
        class PbGhost { public Vector2 p, face; public float t, dmg; public bool chopped, flip; public Sprite spr; public SkillInst s; public int idx; }
        class PbPop { public Vector2 p; public float t, dur; public Color c; public int n; public float r; public int order; }
        readonly List<PbSpark> _pbSparks = new List<PbSpark>(); readonly List<PbOrb> _pbOrbs = new List<PbOrb>(); readonly List<PbJav> _pbJavs = new List<PbJav>();
        readonly List<PbLine> _pbLines = new List<PbLine>(); readonly List<PbArc> _pbArcs = new List<PbArc>(); readonly List<PbRing> _pbRings = new List<PbRing>();
        readonly List<PbCrack> _pbCracks = new List<PbCrack>(); readonly List<PbGhost> _pbGhosts = new List<PbGhost>(); readonly List<PbPop> _pbPops = new List<PbPop>();
        readonly List<int> _pbQ = new List<int>(64); PbAxe _pbAxe;
        void ClearPb1567()
        {
            ClearFis1567(); _pbSparks.Clear(); _pbOrbs.Clear(); _pbJavs.Clear(); _pbChains1567.Clear(); _pbLines.Clear(); _pbArcs.Clear(); _pbRings.Clear(); _pbCracks.Clear(); _pbGhosts.Clear(); _pbPops.Clear(); _pbFx.Clear(); _pbAxe = null;
            PbBegin1567(); PbEnd1567();
        }

        // ---------------------------------------------------------------- helpers
        static Vector2 PbRot(Vector2 d, float deg) => (Vector2)(Quaternion.Euler(0, 0, deg) * d);
        static float PbSegDist(Vector2 p, Vector2 a, Vector2 b) { Vector2 ab = b - a; float l = ab.sqrMagnitude; float k = l < 1e-6f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / l); return (a + ab * k - p).magnitude; }
        bool PbOk(Enemy e) => e.alive && !e.dying;
        Vector2 PbAim1567() { Vector2 d = AimDir(); if (d.sqrMagnitude < 0.0001f) d = _lastDir.sqrMagnitude > 0.0001f ? _lastDir : Vector2.right; return d.normalized; }
        Vector2 PbCursor1567(float maxU, float minU = 0f)
        {
            Vector2 at = _basicAim1517 ? MouseWorld() : _heroPos + PbAim1567() * maxU * 0.6f; Vector2 v = at - _heroPos; float m = v.magnitude;
            if (m < 0.0001f) { v = PbAim1567(); m = 1f; at = _heroPos + v * minU; } else if (m > maxU) at = _heroPos + v / m * maxU; else if (m < minU) at = _heroPos + v / m * minU;
            ClampToRoom(ref at, 4); return at;
        }
        void PbHit1567(Enemy e, float dmg, SkillInst s, Vector2 knock) { if (!PbOk(e)) return; Hit(e, dmg, s, knock); int i = PbIx1567(s.def.id); PbHitFlash1567(e, i); if (i >= 0 && i < 7) PbHits1567[i]++; }
        /// <summary>arc test on the ground (hero / ghost feet = c): inside inner = always; else within range and within half + the body's angular size.</summary>
        bool PbInArc1567(Enemy e, Vector2 c, Vector2 dir, float r, float halfDeg, float innerU)
        {
            Vector2 v = e.pos - c; float m = v.magnitude; if (m > r + e.radius) return false; if (m <= innerU + e.radius) return true;
            float ang = Vector2.Angle(dir, v), pad = Mathf.Atan2(e.radius, Mathf.Max(0.01f, m)) * Mathf.Rad2Deg; return ang <= halfDeg + pad;
        }

        // =============================================================== SPARK SPRAY
        // 3-5 sparks leave the tail tip, drop to the ground in sparkDropS and skitter toward the cursor. t1567 HFX motion (sparkSteer1567, default):
        // start on a fixed fan (sparkFan3/4/5), every sparkStepS the heading = keep x heading + (1 - keep) x (aim + wobble), wobble = +-sparkWobRad from a
        // fixed hash of (spark, step) = SCRIPTED, the same every cast; ground y x sparkYScale; each spark lives sparkLifeF frames. OLD (toggle off): straight
        // fan lines + a sine wobble (sparkFanDeg / sparkWobbleU / sparkWobbleHz / sparkPhase) to sparkRangeU. A spark dies on its first demon.
        bool CastSparkSpray1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 dir = PbAim1567(); Vector2 o = CastOrigin1528(HeroCenter + dir * 0.4f, ref dir); dir.Normalize();
            int n = Mathf.Clamp(HData.At(T.sparkCount1567, s.level, 4), 1, 8); Vector2 g = _heroPos + dir * T.sparkStartU1567; ClampToRoom(ref g, 4);
            var fan = n <= 3 ? T.sparkFan3_1567 : n == 4 ? T.sparkFan4_1567 : T.sparkFan5_1567; float aimA = Mathf.Atan2(dir.y, dir.x);
            for (int i = 0; i < n; i++)
            {
                if (T.sparkChaos1567)
                {   // t1578 D2 / PoE 2 chaotic crawl: fan base heading, re-jittered every sparkChaosJitMin..MaxS, bounces off walls, lives sparkChaosLifeS
                    float f0 = fan != null && i < fan.Length ? fan[i] : 0f; float ba = aimA + f0 * Mathf.Deg2Rad;
                    var k = new PbSpark { a = g, p = g, dir = dir, nrm = new Vector2(-dir.y, dir.x), aimA = aimA, baseA = ba, life = HData.At(T.sparkChaosLifeS1567, i + 1, 1.2f), idx = i, seed = _pbSparkSeed1567 * 16 + i, z0 = Mathf.Max(0f, o.y - g.y), dmg = dmg * T.sparkDmgMul1567, s = s, chaos = true, v = new Vector2(Mathf.Cos(ba), Mathf.Sin(ba)) };
                    k.jitT = SparkJitDt1567(k); _pbSparks.Add(k); SparkChaosN1567++; continue;
                }
                if (T.sparkSteer1567)
                {
                    float f0 = fan != null && i < fan.Length ? fan[i] : 0f; float life = HData.At(T.sparkLifeF1567, i + 1, 36) / 30f;
                    _pbSparks.Add(new PbSpark { a = g, p = g, dir = dir, nrm = new Vector2(-dir.y, dir.x), aimA = aimA, hr = f0 * Mathf.Deg2Rad, life = life, range = T.sparkSpeedU1567 * life, idx = i, z0 = Mathf.Max(0f, o.y - g.y), dmg = dmg * T.sparkDmgMul1567, s = s, steer = true });
                    continue;
                }
                Vector2 d = PbRot(dir, HData.At(T.sparkFanDeg1567, i + 1, 0f)); float range = T.sparkRangeU1567 * HData.At(T.sparkRangeMul1567, i + 1, 1f);
                _pbSparks.Add(new PbSpark { a = g, p = g, dir = d, nrm = new Vector2(-d.y, d.x), range = range, idx = i, wob = HData.At(T.sparkWobbleU1567, i + 1, 0.3f), hz = T.sparkWobbleHz1567 * HData.At(T.sparkHzMul1567, i + 1, 1f), ph = HData.At(T.sparkPhase1567, i + 1, 0f), z0 = Mathf.Max(0f, o.y - g.y), dmg = dmg * T.sparkDmgMul1567, s = s });
            }
            _pbSparkSeed1567++; SparkLastN1567 = n; SparkSpawned1567 += n; PbCasts1567[0]++; PbFlash1567(0, _heroPos + PbAim1567() * 0.6f); PbShake1567(0); HellordAudio.Play("storm_ring", PbPitch1567(0));
            return true;
        }
        int _pbSparkSeed1567 = 1;
        float SparkJitDt1567(PbSpark k) { var T = HellordTuning.I; return Mathf.Lerp(T.sparkChaosJitMinS1567, T.sparkChaosJitMaxS1567, (PbHash1567(k.seed + 29, k.jn) + 1f) * 0.5f); }
        /// <summary>fixed hash in -1..1 (no RNG state): the same spark index and step always give the same wobble</summary>
        static float PbHash1567(int a, int b) { float x = Mathf.Sin(a * 12.9898f + b * 78.233f) * 43758.5453f; return (x - Mathf.Floor(x)) * 2f - 1f; }
        Vector2 PbChest1567(Enemy e) => e.pos + Vector2.up * (HellordTuning.I.pbChestPx1567 / HArt.PPU);
        void UpdateSparks1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbSparks.Count - 1; i >= 0; i--)
            {
                var k = _pbSparks[i]; k.t += dt; Vector2 p; bool end;
                if (k.chaos)
                {   // t1578: zigzag = new heading baseA +- sparkChaosJitDeg each jitter tick (spread stays toward the aim); wall / arena edge = mirror the axis it crossed
                    k.jitT -= dt;
                    while (k.jitT <= 0f)
                    {
                        k.jn++; float h;
                        if (T.sparkChaosKick1567)
                        {   // HFX MOCKUP: kick the CURRENT heading +-(KickMin..KickMax) deg, then pull sparkChaosPull of the way back toward the spark's fan heading (toward the aim)
                            float cur = Mathf.Atan2(k.v.y, k.v.x), mag = Mathf.Lerp(T.sparkChaosKickMinDeg1567, T.sparkChaosKickMaxDeg1567, (PbHash1567(k.seed + 41, k.jn) + 1f) * 0.5f), sg = PbHash1567(k.seed + 11, k.jn) >= 0f ? 1f : -1f;
                            h = cur + sg * mag * Mathf.Deg2Rad; h += T.sparkChaosPull1567 * Mathf.DeltaAngle(h * Mathf.Rad2Deg, k.baseA * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                        }
                        else h = k.baseA + T.sparkChaosJitDeg1567 * Mathf.Deg2Rad * PbHash1567(k.seed + 11, k.jn);
                        Vector2 nv = new Vector2(Mathf.Cos(h), Mathf.Sin(h)); SparkMaxTurnDeg1567 = Mathf.Max(SparkMaxTurnDeg1567, Vector2.Angle(k.v, nv)); k.v = nv; k.jitT += Mathf.Max(0.02f, SparkJitDt1567(k)); SparkJits1567++; }
                    float sp = T.sparkChaosSpeedU1567; Vector2 vv = new Vector2(k.v.x, k.v.y * T.sparkChaosYScale1567); p = k.p + vv * (sp * dt);
                    Vector2 q = p; ClampToRoom(ref q, 2);
                    if (T.sparkChaosBounce1567 && (q - p).sqrMagnitude > 1e-6f)
                    {
                        Vector2 qx = new Vector2(p.x, k.p.y), qy = new Vector2(k.p.x, p.y), cx = qx, cy = qy; ClampToRoom(ref cx, 2); ClampToRoom(ref cy, 2);
                        bool bx = (cx - qx).sqrMagnitude > 1e-6f, by = (cy - qy).sqrMagnitude > 1e-6f; if (!bx && !by) bx = by = true;
                        if (bx) { k.v.x = -k.v.x; k.baseA = Mathf.PI - k.baseA; } if (by) { k.v.y = -k.v.y; k.baseA = -k.baseA; }
                        SparkBounces1567++; k.bounces++; Vector2 cp = q + Vector2.up * (T.sparkBounceLiftPx1567 / HArt.PPU); PbFxOnce1567("ss2_bounce", cp, YSort(q.y) + 25); PbFxOnce1567("ss2_bounceflash", cp, PbTop1567(q)); // HFX: at the contact point, frame 0 on the bounce frame
                        vv = new Vector2(k.v.x, k.v.y * T.sparkChaosYScale1567); p = k.p + vv * (sp * dt);
                    }
                    k.dist += sp * dt; end = k.t >= k.life;
                }
                else if (k.steer)
                {
                    k.stepT += dt; float step = Mathf.Max(0.02f, T.sparkStepS1567);
                    while (k.stepT >= step) { k.stepT -= step; k.step++; float wob = T.sparkWobRad1567 * PbHash1567(k.idx + 1, k.step); k.hr = T.sparkSteerKeep1567 * k.hr + (1f - T.sparkSteerKeep1567) * wob; }
                    float h = k.aimA + k.hr; Vector2 v = new Vector2(Mathf.Cos(h), Mathf.Sin(h) * T.sparkYScale1567); p = k.p + v * (T.sparkSpeedU1567 * dt); k.dist += T.sparkSpeedU1567 * dt; end = k.t >= k.life;
                }
                else
                {
                    k.dist += T.sparkSpeedU1567 * dt; end = k.dist >= k.range; if (end) k.dist = k.range;
                    float ease = Mathf.Clamp01(k.dist / Mathf.Max(0.01f, T.sparkWobbleEaseU1567)); p = k.a + k.dir * k.dist + k.nrm * (k.wob * ease * Mathf.Sin(k.t * k.hz * Mathf.PI * 2f + k.ph));
                }
                ClampToRoom(ref p, 2); k.p = p; bool dead = false; Vector2 up4 = Vector2.up * (T.sparkLiftPx1567 / HArt.PPU);
                k.trailT += dt; if (k.trailT >= 2f / 30f) { k.trailT -= 2f / 30f; PbFxOnce1567("ss_trail", p + up4, YSort(p.y) + 10); }
                Query(p, T.sparkHitRU1567 + 1f, _pbQ); int best = -1; float bd = 1e9f;
                foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e)) continue; float d = (e.pos - p).magnitude; if (d <= T.sparkHitRU1567 + e.radius * 0.5f && d < bd) { bd = d; best = ei; } }
                if (best >= 0)
                {
                    var e = _en[best]; Vector2 kd = k.chaos ? k.v : k.steer ? new Vector2(Mathf.Cos(k.aimA + k.hr), Mathf.Sin(k.aimA + k.hr)) : k.dir; PbHit1567(e, k.dmg, k.s, kd * T.sparkKnock1567);
                    if (!PbFxOnce1567("ss_hit", new Vector2(p.x, PbChest1567(e).y), YSort(e.pos.y) + 30)) PbPop1567(p + Vector2.up * 0.2f, T.sparkPopS1567, PbYellow, 5, 0.25f); dead = true;
                    PbFxOnce1567("ss2_hitflash", new Vector2(p.x, PbChest1567(e).y), PbTop1567(e.pos)); PbFxOnce1567("ss2_hitground", e.pos, FbGround + YSort(e.pos.y) + 12); // t1578 v2 layers (no-op without v2 art)
                }
                else if (end) { if (!PbFxOnce1567("ss_fizzle", p + up4, YSort(p.y) + 20)) PbPop1567(p, T.sparkPopS1567, PbCyan, 3, 0.15f); PbFxOnce1567("ss2_scorch", p, FbGround + YSort(p.y) + 8); dead = true; }
                if (dead) _pbSparks.RemoveAt(i);
            }
        }

        // =============================================================== BALL LIGHTNING
        // A slow orb drifts from the tail tip toward the cursor (distance clamped to orbMinU..orbMaxU) and passes orbPassU beyond it. Every orbZapEveryS
        // it zaps the nearest orbZapTargets demons within orbZapRU (HFX zap arcs: length variant L16..L48 nearest the distance, dir16 row; old Chain
        // Lightning bolt when the art is missing). At most orbCap orbs alive: a new cast pops the oldest (orbCapPopOldest1567; off = refused).
        bool CastBallLightning1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 dir = PbAim1567(); Vector2 o = CastOrigin1528(HeroCenter + dir * 0.4f, ref dir); dir.Normalize();
            int live = 0; foreach (var b in _pbOrbs) if (b.die < 0f) live++;
            if (live >= Mathf.Max(1, T.orbCap1567))
            {
                if (!T.orbCapPopOldest1567) { s.cd = 0.1f; return false; }
                foreach (var b in _pbOrbs) if (b.die < 0f) { b.die = 0f; OrbPopped1567++; break; }
            }
            Vector2 cur = PbCursor1567(T.orbMaxU1567, T.orbMinU1567); Vector2 g = o - Vector2.up * T.orbHeightU1567; float total = (cur - g).magnitude + T.orbPassU1567;
            Vector2 d2 = cur - g; if (d2.sqrMagnitude > 0.0001f) dir = d2.normalized;
            _pbOrbs.Add(new PbOrb { a = g, p = g, dir = dir, total = total, zap = T.orbFirstZapS1567, dmg = dmg * T.orbZapMul1567, s = s });
            PbCasts1567[1]++; HellordAudio.Play("storm_ring", PbPitch1567(1)); OrbAlive1567 = live + 1 - (live >= T.orbCap1567 ? 1 : 0); OrbPeak1567 = Mathf.Max(OrbPeak1567, OrbAlive1567);
            return true;
        }
        float OrbPopLen1567 => Mathf.Max(HellordTuning.I.orbPopS1567, PbArtLen1567("bl_fade"));
        void UpdateOrbs1567(float dt)
        {
            var T = HellordTuning.I; int live = 0;
            for (int i = _pbOrbs.Count - 1; i >= 0; i--)
            {
                var b = _pbOrbs[i]; b.t += dt;
                if (b.die >= 0f) { b.die += dt; if (b.die >= OrbPopLen1567) _pbOrbs.RemoveAt(i); continue; }
                live++; b.dist = Mathf.Min(b.total, b.dist + T.orbSpeedU1567 * dt); Vector2 p = b.a + b.dir * b.dist; ClampToRoom(ref p, 4); b.p = p;
                b.trailT += dt; if (b.trailT >= T.orbTrailEveryS1567) { b.trailT -= T.orbTrailEveryS1567; PbFxOnce1567("bl2_trail", p + Vector2.up * T.orbHeightU1567, YSort(p.y) + 28); } // t1584 HFX v2: one trail puff every 3 f (no-op without v2)
                b.zap -= dt;
                if (b.zap <= 0f)
                {
                    b.zap += Mathf.Max(0.05f, T.orbZapEveryS1567); int nT = HData.At(T.orbZapTargets1567, b.s.level, 2); Vector2 op = p + Vector2.up * T.orbHeightU1567; Query(p, T.orbZapRU1567 + 1.5f, _pbQ); int got = 0;
                    _pbQSort.Clear(); foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e)) continue; float d = (PbChest1567(e) - op).magnitude; if (d <= T.orbZapRU1567 + e.radius * 0.5f) _pbQSort.Add(new Vector2(d, ei)); }
                    _pbQSort.Sort((x, y) => x.x.CompareTo(y.x));
                    foreach (var q in _pbQSort)
                    {
                        if (got >= nT) break; var e = _en[(int)q.y]; Vector2 ep = PbChest1567(e); PbHit1567(e, b.dmg, b.s, Vector2.zero); got++; OrbZaps1567++; if (got == 1) PbFlash1567(1, e.pos, T.orbZapFlashMul1567);
                        Vector2 d = ep - op; int Lpx = Mathf.Clamp(Mathf.RoundToInt(d.magnitude * HArt.PPU / 8f) * 8, 16, 48); int ord = YSort(Mathf.Min(b.p.y, e.pos.y)) + 40;
                        if (PbFxOnce1567("bl_zap" + Lpx, op, ord, PbRow16(d))) { PbFxOnce1567("bl_zaphit", ep, YSort(e.pos.y) + 41); PbFxOnce1567("bl2_zapflash", ep, PbTop1567(e.pos)); }
                        else if (T44.fxV44Pack) { EnsureFX44(); HeroSkillFX.ChainBolt(new[] { op, ep }); } else PbBolt1567(op, ep, T.orbZapShowS1567, PbYellow, YSort(p.y) + 40, 2, 0.2f, OrbZaps1567);
                    }
                    if (got > 0) HellordAudio.Play("storm_ring", PbPitch1567(1) * T.orbZapPitchMul1567);
                }
                if (b.dist >= b.total) { b.die = 0f; }
            }
            OrbAlive1567 = live; OrbPeak1567 = Mathf.Max(OrbPeak1567, live);
        }
        readonly List<Vector2> _pbQSort = new List<Vector2>(32);

        // =============================================================== THUNDER JAVELIN
        // A fast straight bolt from the tail tip toward the cursor (javRangeU). On its FIRST demon it bursts: the target takes the full hit and javForks
        // short forked bolts shoot OUT of that point at fixed angles (javForkAngles, a starburst; not hops from demon to demon = not Chain Lightning).
        // HFX rule (javSnap1567): a fork snaps to another demon within javSnapU and javSnapDeg of its angle, else it is javForkLenEvenU / OddU long and
        // hits what its line touches. Each demon is hit once per burst (javForkMul). Nothing hit = it fizzles at full range or on a wall.
        bool CastThunderJavelin1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 dir = PbAim1567(); Vector2 o = CastOrigin1528(HeroCenter + dir * 0.4f, ref dir); dir.Normalize();
            Vector2 g = o - Vector2.up * T.javHeightU1567; _pbJavs.Add(new PbJav { a = g, p = g, dir = dir, range = T.javRangeU1567, dmg = dmg, s = s });
            PbCasts1567[2]++; HellordAudio.Play("storm_ring", PbPitch1567(2)); return true;
        }
        void UpdateJavs1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbJavs.Count - 1; i >= 0; i--)
            {
                var j = _pbJavs[i]; j.t += dt; Vector2 p0 = j.p; j.dist = Mathf.Min(j.range, j.dist + T.javSpeedU1567 * dt); Vector2 p1 = j.a + j.dir * j.dist; j.p = p1;
                Vector2 mid = (p0 + p1) * 0.5f; Query(mid, (p1 - p0).magnitude * 0.5f + T.javHitRU1567 + 1f, _pbQ); int best = -1; float bAlong = 1e9f;
                foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e)) continue; if (PbSegDist(e.pos, p0, p1) > T.javHitRU1567 + e.radius * 0.5f) continue; float al = Vector2.Dot(e.pos - p0, j.dir); if (al < bAlong) { bAlong = al; best = ei; } }
                bool wall = false; { Vector2 c = p1; ClampToRoom(ref c, 4); wall = (c - p1).sqrMagnitude > 0.0004f; }
                if (best >= 0) { JavBurst1567(j, _en[best], best); _pbJavs.RemoveAt(i); continue; }
                if (j.dist >= j.range || wall) { JavFizzles1567++; Vector2 fp = p1 + Vector2.up * T.javHeightU1567; if (!PbFxOnce1567("tj_bolthit", fp, YSort(p1.y) + 46)) PbPop1567(fp, T.javFizzleS1567, PbCyan, 4, 0.3f); _pbJavs.RemoveAt(i); }
            }
        }
        void JavBurst1567(PbJav j, Enemy first, int firstIx)
        {
            var T = HellordTuning.I; Vector2 c = first.pos; Vector2 cc = PbChest1567(first); PbHit1567(first, j.dmg, j.s, j.dir * T.javKnock1567); JavBursts1567++;
            int nf = Mathf.Clamp(HData.At(T.javForks1567, j.s.level, 5), 0, 12); var hit = new HashSet<int> { firstIx }; int order = YSort(c.y) + 45; bool art = PbA1567("tj_fork48") != null;
            if (!PbFxOnce1567("tj_impact", cc, YSort(c.y) + 50)) PbPop1567(cc, T.javFizzleS1567 * 1.4f, PbWhiteY, 8, 0.45f);
            PbFxOnce1567("tj2_impflash", cc, PbTop1567(c)); PbFxOnce1567("tj2_impground", c, FbGround + YSort(c.y) + 12); PbFxOnce1567("tj2_scorch", c, FbGround + YSort(c.y) + 8);
            if (T.javChain1567)
            {   // t1579 Chain-Lightning style: jump to the nearest unhit demon within javChainRangeU, javChainJumps times, damage x javChainFalloff per jump
                _pbChains1567.Add(new PbChain1567 { at = firstIx, from = c, dmg = j.dmg, left = Mathf.Clamp(HData.At(T.javChainJumps1567, j.s.level, 4), 0, 20), t = T.javChainDelayS1567 - T.javChainFirstS1567, s = j.s, hit = hit, dir = j.dir }); JavChains1567++;
                if (!T.javChainKeepForks1567) nf = 0;
            }
            for (int k = 0; k < nf; k++)
            {
                float ang = T.javForkAngles1567 != null && T.javForkAngles1567.Length > 0 ? T.javForkAngles1567[k % T.javForkAngles1567.Length] : k * 360f / nf;
                Vector2 d = PbRot(j.dir, ang), nrm = new Vector2(-d.y, d.x); float delay = HData.At(T.javForkDelayF1567, k + 1, 0) / 30f;
                // HFX snap: the nearest other demon within javSnapU and javSnapDeg of this fork's angle
                int snap = -1; float sd = 1e9f;
                if (T.javSnap1567) { Query(c, T.javSnapU1567 + 1f, _pbQ); foreach (int ei in _pbQ) { if (hit.Contains(ei)) continue; var e = _en[ei]; if (!PbOk(e)) continue; Vector2 v = e.pos - c; float m = v.magnitude; if (m < 0.05f || m > T.javSnapU1567) continue; if (Vector2.Angle(d, v) > T.javSnapDeg1567) continue; if (m < sd) { sd = m; snap = ei; } } }
                float len = (k & 1) == 0 ? T.javForkLenEvenU1567 : T.javForkLenOddU1567; Vector2 end = c + d * len;
                if (snap >= 0) { var e = _en[snap]; hit.Add(snap); end = e.pos; PbHit1567(e, j.dmg * T.javForkMul1567, j.s, d * T.javKnock1567 * 0.5f); JavForkHits1567++; PbFxOnce1567("tj_bolthit", PbChest1567(e), YSort(e.pos.y) + 46, 0, delay + 1f / 30f); }
                else
                {   // no snap: the fork's straight line hits what it touches
                    Query((c + end) * 0.5f, len * 0.5f + T.javForkHitRU1567 + 1f, _pbQ);
                    foreach (int ei in _pbQ) { if (hit.Contains(ei)) continue; var e = _en[ei]; if (!PbOk(e)) continue; if (PbSegDist(e.pos, c, end) <= T.javForkHitRU1567 + e.radius * 0.5f) { hit.Add(ei); PbHit1567(e, j.dmg * T.javForkMul1567, j.s, d * T.javKnock1567 * 0.5f); JavForkHits1567++; PbFxOnce1567("tj_bolthit", PbChest1567(e), YSort(e.pos.y) + 46, 0, delay + 1f / 30f); } }
                }
                Vector2 fd = end - c; float lpx = fd.magnitude * HArt.PPU;
                if (art) { PbFxOnce1567(lpx < 40f ? "tj_fork32" : "tj_fork48", cc, order, PbRow16(fd), delay); continue; }
                // placeholder: jagged code line + a twig
                int segs = Mathf.Max(1, T.javForkSegs1567); Vector2 up = cc - c; var ln = new PbLine { t = -delay, dur = T.javForkShowS1567, c = PbWhiteY, order = order, w = 1 }; ln.pts.Add(cc); Vector2 dn = fd.sqrMagnitude > 1e-6f ? fd.normalized : d; nrm = new Vector2(-dn.y, dn.x);
                for (int q = 1; q <= segs; q++) { float u = q / (float)segs; float jit = q == segs ? 0f : (((q + k) & 1) == 0 ? 1f : -1f) * T.javForkJitterU1567; ln.pts.Add(c + fd * u + nrm * jit + up); }
                _pbLines.Add(ln);
                if (T.javForkTwig1567 && segs >= 2) { var tw = new PbLine { t = -delay, dur = T.javForkShowS1567 * 0.8f, c = PbYellow, order = order - 1 }; Vector2 m = ln.pts[1]; tw.pts.Add(m); tw.pts.Add(m + PbRot(dn, (k & 1) == 0 ? 35f : -35f) * fd.magnitude * 0.35f); _pbLines.Add(tw); }
            }
            HellordAudio.Play("storm_ring", PbPitch1567(2) * T.javBurstPitchMul1567); PbShake1567(2); PbFlash1567(2, c);
        }
        class PbChain1567 { public int at, jumps, left; public Vector2 from, dir; public float dmg, t; public SkillInst s; public HashSet<int> hit; }
        readonly List<PbChain1567> _pbChains1567 = new List<PbChain1567>();
        void UpdateChains1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbChains1567.Count - 1; i >= 0; i--)
            {
                var ch = _pbChains1567[i]; ch.t += dt; if (ch.t < T.javChainDelayS1567) continue; ch.t -= T.javChainDelayS1567;
                if (ch.left <= 0) { _pbChains1567.RemoveAt(i); continue; }
                Vector2 c = ch.at >= 0 && ch.at < _en.Length && _en[ch.at] != null ? _en[ch.at].pos : ch.from; float R = T.javChainRangeU1567; Query(c, R + 1f, _pbQ); int best = -1; float bd = 1e9f;
                foreach (int ei in _pbQ) { if (ch.hit.Contains(ei)) continue; var e = _en[ei]; if (!PbOk(e)) continue; float d = (e.pos - c).magnitude; if (d <= R && d < bd) { bd = d; best = ei; } }
                if (best < 0) { _pbChains1567.RemoveAt(i); continue; }
                var t = _en[best]; ch.hit.Add(best); ch.jumps++; ch.left--; float mul = Mathf.Pow(T.javChainFalloff1567, ch.jumps); Vector2 fd = t.pos - c; Vector2 kd = fd.sqrMagnitude > 1e-6f ? fd.normalized : ch.dir;
                PbHit1567(t, ch.dmg * mul, ch.s, kd * T.javKnock1567 * 0.5f); JavChainJumps1567++; JavForkHits1567++; JavChainMax1567 = Mathf.Max(JavChainMax1567, ch.jumps); JavChainLastMul1567 = mul; JavChainMaxJumpU1567 = Mathf.Max(JavChainMaxJumpU1567, fd.magnitude);
                Vector2 c0 = c + Vector2.up * (T.pbChestPx1567 / HArt.PPU); float lpx = fd.magnitude * HArt.PPU; int so = YSort(Mathf.Min(c.y, t.pos.y)) + 45; // HFX sort: max(source, target feet screen y) + 45
                // HFX v2b chain strip: the L variant (32/48/64/80/96) nearest the chest distance, dir16 row, pivot on the SOURCE chest; else the t1567 fork strips
                int bl = 0; float bdl = 1e9f; var Ls = T.javChainLensPx1567; if (Ls != null) for (int q = 0; q < Ls.Length; q++) { float dd = Mathf.Abs(Ls[q] - lpx); if (dd < bdl && PbA1567("tj2_chain" + Ls[q]) != null) { bdl = dd; bl = Ls[q]; } }
                string fk = bl > 0 ? "tj2_chain" + bl : lpx < T.javChainForkSplitPx1567 ? "tj_fork32" : "tj_fork48"; float baseL = bl > 0 ? bl : fk == "tj_fork32" ? 32f : 48f; var fa = PbA1567(fk); JavChainLastL1567 = bl;
                if (fa != null) { float sc = T.javChainScaleArt1567 ? Mathf.Clamp(lpx / baseL, 0.6f, 1.6f) : 1f; PbFxOnce1567(fk, c0, so, PbRow16(fd), 0f, false, sc); }
                else { var ln = new PbLine { t = 0f, dur = T.javForkShowS1567, c = PbWhiteY, order = YSort(Mathf.Min(c.y, t.pos.y)) + 45, w = 1 }; Vector2 nrm = new Vector2(-kd.y, kd.x); ln.pts.Add(c0); for (int q = 1; q <= 3; q++) ln.pts.Add(c0 + fd * (q / 3f) + (q == 3 ? Vector2.zero : nrm * ((q & 1) == 0 ? 1f : -1f) * T.javForkJitterU1567)); _pbLines.Add(ln); }
                Vector2 tc = PbChest1567(t); float sd = T.javChainHitDelayS1567; // HFX: hit set starts on the chain strip's strike frame (frame 1)
                if (PbA1567("tj2_chainhit") != null) { PbFxOnce1567("tj2_chainhit", tc, YSort(t.pos.y) + 46, 0, sd); PbFxOnce1567("tj2_chainhitflash", tc, PbTop1567(t.pos), 0, sd); PbFxOnce1567("tj2_chainhitground", t.pos, FbGround + YSort(t.pos.y) + 12, 0, sd); }
                else { if (!PbFxOnce1567("tj_bolthit", tc, YSort(t.pos.y) + 46, 0, sd)) PbPop1567(tc, T.javFizzleS1567, PbWhiteY, 5, 0.3f); PbFxOnce1567("tj2_boltflash", tc, PbTop1567(t.pos), 0, sd); }
                HellordAudio.Play("storm_ring", PbPitch1567(2) * (1f + 0.06f * ch.jumps)); ch.at = best; ch.from = t.pos;
            }
        }
        /// <summary>t1578: HFX "flash" layer order = above actors / other FX around p</summary>
        int PbTop1567(Vector2 p) => YSort(p.y - HellordTuning.I.slamV2TopU1567) + 60;
        // =============================================================== HEADSMAN'S CHOP
        // Instant short WIDE axe arc toward the cursor from the hero's feet (forgiving: chopHalfDeg + each demon's body width, and anything inside
        // chopInnerU is always hit). Knock-back outward. Demons that were ALREADY bleeding when struck release a blood shockwave (up to chopWaveMax).
        bool CastHeadsmansChop1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 dir = PbAim1567(); Vector2 c = _heroPos; float r = T.chopRangeU1567 * Mathf.Sqrt(s.AreaMul); bool v2 = T.chopV2_1567 && PbA1567("xc2_flash") != null; float half = v2 ? T.chopHalfDegV2_1567 : T.chopHalfDeg1567; ChopV2Last1567 = v2; ChopLastHalf1567 = half;
            Query(c, r + 1.5f, _pbQ); var waves = new List<Vector2>(); int hits = 0;
            foreach (int ei in _pbQ)
            {
                var e = _en[ei]; if (!PbOk(e) || !PbInArc1567(e, c, dir, r, half, T.chopInnerU1567)) continue; bool bleeding = e.bleedT > 0f;
                Vector2 kd = e.pos - c; kd = kd.sqrMagnitude > 0.0001f ? kd.normalized : dir; PbHit1567(e, dmg, s, kd * T.chopKnock1567); hits++; ChopHits1567++;
                if (!(v2 && PbFxOnce1567(PbEv1567("chop.v2.hit", "xc2_hit"), e.pos + Vector2.up * (T.chopV2HitPx1567 / HArt.PPU), YSort(e.pos.y) + 40, PbRow16(kd)))) // t1586 v2: body centre (feet + 11 px), row = knock-back
                PbFxOnce1567(PbEv1567("chop.hit", "xc_spark"), e.pos + Vector2.up * (T.chopSparkPx1567 / HArt.PPU), YSort(e.pos.y) + 40, PbRow16(kd)); // HFX hit spark, row = knock-back
                PbFxOnce1567(PbEv1567("chop.skid", "xc_skid"), e.pos, FbGround + YSort(e.pos.y) + 5, 0, T.chopSkidDelayF1567 / 30f, kd.x < 0f); // HFX skid dust decal (flip X = pushed left)
                if (bleeding && waves.Count < T.chopWaveMax1567) waves.Add(e.pos);
            }
            Color steel = PbSteel; // HFX arc: row = aim, at the waist, sorted above the actors; starts on f1 = the hit frame (hits are instant)
            ChopLastR1567 = r; ChopArtScale1567 = PbMatch1567(r, v2 ? T.chopArtRadiusV2Px1567 : T.chopArtRadiusPx1567); string ak = PbEv1567("chop.arc", "xc_arc");
            if (v2)
            {   // t1586 HFX v2: every dir16 layer at the hero FEET, row = aim, scaled with the arc so art = hitbox; hits are instant so the arc starts on its DAMAGE frame (chopV2ArcStartF = 2)
                float a0 = -T.chopV2ArcStartF1567 / 30f, sc = ChopArtScale1567; int row = PbRow16(dir), top = YSort(c.y - 4f);
                PbFxOnce1567(ak, c, top + 60, row, a0, false, sc);
                PbFxOnce1567(PbEv1567("chop.v2.sparks", "xc2_sparks"), c, top + 62, row, a0 + 1f / 30f, false, sc);           // from arc f1
                PbFxOnce1567(PbEv1567("chop.v2.flash", "xc2_flash"), c, PbTop1567(c), row, a0 + 1f / 30f, false, sc);          // top, from arc f1
                PbFxOnce1567(PbEv1567("chop.v2.scrape", "xc2_scrape"), c, FbGround + YSort(c.y) - 100, row, a0 + 2f / 30f, false, sc); // ground decal from the hit
                PbFxOnce1567(PbEv1567("chop.v2.dshadow", "xc2_dshadow"), c, FbGround + YSort(c.y) - 95, row, a0, false, sc);
                PbFxOnce1567(PbEv1567("chop.v2.debris", "xc2_debris"), c, YSort(c.y) + 3, row, a0, false, sc);
                PbFxOnce1567(PbEv1567("chop.v2.dust", "xc2_dust"), c, top + 58, row, a0, false, sc);
                if (hits > 0 && T.chopV2HitStopS1567 > 0f) { HitStop.Play(T.chopV2HitStopS1567); ChopHitStops1567++; } // HFX: hold on arc f2 for 2 frames
            }
            else
            if (!(ak.Length > 0 && PbFxOnce1567(ak, c + Vector2.up * (T.chopArcLiftPx1567 / HArt.PPU), YSort(c.y - 4f) + 60, PbRow16(dir), -1f / 30f, false, ChopArtScale1567))) _pbArcs.Add(new PbArc { c = c + Vector2.up * T.chopArcLiftU1567, dir = dir, r = r, half = T.chopHalfDeg1567, sweep = T.chopSweepS1567, fade = T.chopFadeS1567, c1 = steel, c2 = PbBlood, order = YSort(c.y) + 30, strip = "headsmans_chop/arc" });
            foreach (var w in waves) ChopWave1567(w, dmg * T.chopWaveMul1567, s);
            PbCasts1567[3]++; HellordAudio.Play("frost_cleave", PbPitch1567(3)); if (hits > 0) { PbShake1567(3); PbFlash1567(3, c + dir * (r * 0.5f)); }
            return true;
        }
        void ChopWave1567(Vector2 at, float dmg, SkillInst s)
        {
            var T = HellordTuning.I; ChopWaves1567++; float r = T.chopWaveRU1567 * Mathf.Sqrt(s.AreaMul); Query(at, r + 1f, _pbQ);
            foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e)) continue; Vector2 v = e.pos - at; float m = v.magnitude; if (m < 0.05f || m > r + e.radius * 0.5f) continue; PbHit1567(e, dmg, s, v / m * T.chopWaveKnock1567); ChopWaveHits1567++; }
            if (!PbFxOnce1567(PbEv1567("chop.wave", "xc_wave"), at, YSort(at.y) + 2, 0, T.chopWaveDelayF1567 / 30f, false, PbMatch1567(r, T.chopWaveArtPx1567))) { // HFX blood wave at the feet, 2 f after the hit
            _pbRings.Add(new PbRing { c = at, r0 = 0.2f, r1 = r, dur = T.chopWaveS1567, col = PbBlood, order = YSort(at.y) + 35, strip = "headsmans_chop/blood_wave" });
            PbPop1567(at + Vector2.up * 0.4f, T.chopWaveS1567, PbBloodHi, 6, 0.35f); } HellordAudio.Play("gore_splat");
        }

        // =============================================================== EARTH-SPLITTING SLAM
        // The axe hits a FIXED spot slamAheadU in front of the hero (toward the cursor); round impact (slamRU). The hero does not move. It leaves cracked
        // ground (crackRU, crackLifeS) that slows demons standing in it by crackSlow; after eruptDelayS the cracks erupt once more (eruptRU, eruptMul).
        // At most crackCap crack fields: a new slam removes the oldest.
        bool CastEarthSlam1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; if (T.slamForward1567) return CastSlamLine1567(s, dmg); // t1572: PoE 2 forward fissure line (off = the round first-pass slam below)
            Vector2 dir = PbAim1567(); Vector2 hero0 = _heroPos; Vector2 at = _heroPos + dir * T.slamAheadU1567; ClampToRoom(ref at, 4); float area = Mathf.Sqrt(s.AreaMul);
            float r = T.slamRU1567 * area; Query(at, r + 1f, _pbQ);
            foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e)) continue; Vector2 v = e.pos - at; float m = v.magnitude; if (m > r + e.radius * 0.5f) continue; PbHit1567(e, dmg, s, (m > 0.05f ? v / m : dir) * T.slamKnock1567); }
            while (_pbCracks.Count >= Mathf.Max(1, T.crackCap1567)) _pbCracks.RemoveAt(0);
            _pbCracks.Add(new PbCrack { p = at, life = T.crackLifeS1567, dmg = dmg * T.eruptMul1567, s = s, seed = SlamImpacts1567 });
            SlamImpacts1567++; SlamCracks1567 = _pbCracks.Count; SlamCracksPeak1567 = Mathf.Max(SlamCracksPeak1567, _pbCracks.Count); SlamLastAt1567 = at; SlamLastHeroMove1567 = (_heroPos - hero0).magnitude;
            if (PbFxOnce1567("xs_impact", at, YSort(at.y) + 4)) SlamDust1567(at, T.slamDustPx1567, 1); else { // HFX round impact + 4 dust puffs at +1/+2 f
            _pbRings.Add(new PbRing { c = at, r0 = 0.3f, r1 = r, dur = T.slamRingS1567, col = PbDust, order = YSort(at.y) + 30, strip = "earth_slam/impact" });
            PbPop1567(at + Vector2.up * 0.2f, T.slamRingS1567, PbDust, 10, r * 0.6f); } PbShake1567(4); PbFlash1567(4, at);
            PbCasts1567[4]++; HellordAudio.Play("oxhead_stomp_strike", PbPitch1567(4)); return true;
        }
        void UpdateCracks1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbCracks.Count - 1; i >= 0; i--)
            {
                var k = _pbCracks[i]; k.t += dt; float area = Mathf.Sqrt(k.s.AreaMul), r = k.r > 0f ? k.r : T.crackRU1567 * area;
                Query(k.p, r + 1f, _pbQ);
                if (k.t < k.life - T.crackFadeS1567) foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e) || e.boss) continue; if ((e.pos - k.p).magnitude > r + e.radius * 0.5f) continue; e.slowT = Mathf.Max(e.slowT, T.crackSlowRefreshS1567); e.slowAmt = Mathf.Max(e.slowT > T.crackSlowRefreshS1567 ? e.slowAmt : 0f, T.crackSlow1567); if (k.slowed.Add(ei)) SlamSlowed1567++; }
                if (!k.erupted && k.t >= (k.eruptAt > 0f ? k.eruptAt : T.eruptDelayS1567))
                {
                    k.erupted = true; SlamErupts1567++; bool v2q = k.fis != null && k.fis.v2; /* t1576 v2: damage only, the fissure replays the v2 layers */ float er = k.r > 0f ? k.r * T.slamSegEruptMul1567 : T.eruptRU1567 * area; Query(k.p, er + 1f, _pbQ);
                    foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e)) continue; Vector2 v = e.pos - k.p; float m = v.magnitude; if (m > er + e.radius * 0.5f) continue; if (k.fis != null && !k.fis.eruptHit.Add(ei)) continue; PbHit1567(e, k.dmg, k.s, (m > 0.05f ? v / m : Vector2.up) * T.eruptKnock1567); SlamEruptHits1567++; }
                    string ek = PbEv1567("slam.erupt", "xs_erupt"); if (v2q) { } else if (ek.Length > 0 && PbFxOnce1567(ek, k.p, YSort(k.p.y) + 3, 0, -2f / 30f, (k.seed & 1) == 1, PbMatch1567(er, T.slamEruptArtPx1567))) { if (k.fis == null || (k.seed & 1) == 0) SlamDust1567(k.p, T.eruptDustPx1567, 3); } else { // HFX eruption: its damage frame f2 lands now
                    _pbRings.Add(new PbRing { c = k.p, r0 = 0.2f, r1 = er, dur = T.eruptRingS1567, col = PbEmber, order = YSort(k.p.y) + 32, strip = "earth_slam/erupt" });
                    PbPop1567(k.p + Vector2.up * 0.5f, T.eruptRingS1567, PbEmber, 12, er * 0.7f); } if (!v2q && (k.fis == null || k.fis.segs == 0 || (k.seed - k.fis.id * 31) == 0)) { PbShake1567(4, T.eruptShakeMul1567); PbFlash1567(4, k.p, T.eruptFlashMul1567); PbHeat1567(k.p, er * 1.5f, 0.3f); } HellordAudio.Play("curse_rune_burst", PbPitch1567(4) * T.eruptPitchMul1567);
                }
                if (k.t >= k.life) _pbCracks.RemoveAt(i);
            }
            if (!T.slamForward1567) SlamCracks1567 = _pbCracks.Count;
        }

        void SlamDust1567(Vector2 at, float px, int f0)
        {   // HFX: 4 puffs around P, alternating v1 / v2, at +f0 / +f0+1 frames, flip X when left of P
            for (int q = 0; q < 4; q++) { float ang = 45f + 90f * q; Vector2 d = PbRot(Vector2.right, ang); d.y *= 0.6f; Vector2 p = at + d * (px / HArt.PPU); PbFxOnce1567((q & 1) == 0 ? "xs_dust1" : "xs_dust2", p, YSort(p.y) + 2, 0, (f0 + (q & 1)) / 30f, d.x < 0f); }
        }

        // =============================================================== TWIN AXE THROW
        // The axe spins from the tail tip to the cursor (axeRangeMinU..axeRangeMaxU), piercing (each demon once on the way out), hangs axeHangS, flies
        // back to the hero's CURRENT position hitting each demon once more (axeReturnMul), and is caught within axeCatchU. ONE axe out at a time: while it
        // is out the basic's cooldown holds at the time it needs to come back (ring / HUD / not-ready click show it).
        bool CastTwinAxe1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; if (_pbAxe != null) { s.cd = Mathf.Max(s.cd, 0.05f); AxeRefused1567++; return false; }
            Vector2 dir = PbAim1567(); Vector2 o = CastOrigin1528(HeroCenter + dir * 0.4f, ref dir); dir.Normalize();
            Vector2 g = o - Vector2.up * T.axeHeightU1567; if (T.axeFromHand1567) { g = _heroPos + new Vector2((dir.x < 0f ? -1f : 1f) * T.axeHandPx1567.x / HArt.PPU, T.axeHandPx1567.y / HArt.PPU - T.axeHeightU1567); } /* HA: leaves the hand */ Vector2 b = PbCursor1567(T.axeRangeMaxU1567, T.axeRangeMinU1567); Vector2 d2 = b - _heroPos; if (d2.sqrMagnitude > 0.0001f) b = g + d2.normalized * Mathf.Clamp(d2.magnitude, T.axeRangeMinU1567, T.axeRangeMaxU1567); ClampToRoom(ref b, 4);
            _pbAxe = new PbAxe { a = g, b = b, p = g, dmg = dmg, s = s }; AxeThrows1567++; PbCasts1567[5]++; PbShake1567(5); HellordAudio.Play("hook_chain_throw", PbPitch1567(5)); return true;
        }
        void UpdateAxe1567(float dt)
        {
            var T = HellordTuning.I; var x = _pbAxe; if (x == null) return; x.t += dt; x.life += dt; x.spin += T.axeSpinDps1567 * dt; Vector2 p0 = x.p;
            if (x.phase == 0) { Vector2 v = x.b - x.p; float st = T.axeOutSpeedU1567 * dt; if (v.magnitude <= st) { x.p = x.b; x.phase = 1; x.t = 0f; } else x.p += v.normalized * st; }
            else if (x.phase == 1) { if (x.t >= T.axeHangS1567) { x.phase = 2; x.t = 0f; } }
            else { Vector2 v = _heroPos - x.p; float sp = T.axeBackEase1567 ? Mathf.Lerp(T.axeBackStartU1567, T.axeBackSpeedU1567, Mathf.Clamp01(x.t / Mathf.Max(0.01f, T.axeBackRampS1567))) : T.axeBackSpeedU1567; float st = sp * dt;
                if (v.magnitude <= Mathf.Max(st, T.axeCatchU1567)) { _pbAxe = null; AxeCatches1567++; HellordAudio.Play("whip_cast", PbPitch1567(5) * T.axeCatchPitchMul1567); PbFxOnce1567(PbEv1567("axe.catch", "xa_catch"), _heroPos + Vector2.up * T.axeHeightU1567, YSort(_heroPos.y) + 40); HeroCatchPose1567(x.s); PbFlash1567(5, _heroPos, T.axeCatchFlashMul1567); return; } x.p += v.normalized * st; }
            x.vel = x.p - p0;
            if (x.life > T.axeMaxLifeS1567) { _pbAxe = null; return; } // safety
            var set = x.phase == 2 ? x.hitBack : x.hitOut; Vector2 mid = (p0 + x.p) * 0.5f; Query(mid, (x.p - p0).magnitude * 0.5f + T.axeHitRU1567 + 1f, _pbQ);
            Vector2 md = x.p - p0; Vector2 kd = md.sqrMagnitude > 1e-6f ? md.normalized : Vector2.zero;
            foreach (int ei in _pbQ)
            {
                var e = _en[ei]; if (!PbOk(e) || set.Contains(ei)) continue; if (PbSegDist(e.pos, p0, x.p) > T.axeHitRU1567 + e.radius * 0.5f) continue; set.Add(ei);
                if (!PbFxOnce1567(PbEv1567("axe.v2.hit", "xa2_hit"), e.pos + Vector2.up * (11f / HArt.PPU), YSort(e.pos.y) + 40, PbRow16(kd))) // t1586 v2 pierce burst (no-op without v2)
                PbFxOnce1567(PbEv1567("axe.hit", "xa_spark"), e.pos + Vector2.up * (12f / HArt.PPU), YSort(e.pos.y) + 40, PbRow16(kd)); // HFX: each pierce, row = axe velocity
                if (x.phase == 2) { PbHit1567(e, x.dmg * T.axeReturnMul1567, x.s, kd * T.axeKnock1567); AxeBackHits1567++; } else { PbHit1567(e, x.dmg, x.s, kd * T.axeKnock1567); AxeOutHits1567++; }
            }
            // one axe out: hold the basic's cooldown at the time it needs to come back
            if (_basic1517 != null && _basic1517 == x.s)
            {
                float left = x.phase == 0 ? (x.b - x.p).magnitude / Mathf.Max(0.1f, T.axeOutSpeedU1567) + T.axeHangS1567 + (x.b - _heroPos).magnitude / Mathf.Max(0.1f, T.axeBackSpeedU1567)
                           : x.phase == 1 ? T.axeHangS1567 - x.t + (x.p - _heroPos).magnitude / Mathf.Max(0.1f, T.axeBackSpeedU1567) : (x.p - _heroPos).magnitude / Mathf.Max(0.1f, T.axeBackSpeedU1567);
                x.s.cd = Mathf.Max(x.s.cd, left);
            }
        }

        // =============================================================== PHANTOM AXES
        // One click: phCount ghost Xingtians appear on a fixed ring (phRingU) around the cursor (clamped to phRangeU), staggered phStaggerS apart; each one
        // winds up phWindS, then chops ONE arc (phArcRU, phArcHalfDeg) toward the cursor point and fades out at phLifeS.
        bool CastPhantomAxes1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 cen = PbCursor1567(T.phRangeU1567, 0f); int n = Mathf.Clamp(HData.At(T.phCount1567, s.level, 2), 1, 6);
            var angs = n == 2 ? T.phAng2_1567 : n == 3 ? T.phAng3_1567 : null; Sprite spr = _heroBase != null ? _heroBase.sprite : null;
            if (_heroBaseF != null && _heroBaseF.Length > 0 && T.phIdleBody1567) spr = _heroBaseF[0];
            for (int i = 0; i < n; i++)
            {
                float a = angs != null && i < angs.Length ? angs[i] : 90f + i * 360f / n; Vector2 p = cen + PbRot(Vector2.right, a) * T.phRingU1567; ClampToRoom(ref p, 6);
                Vector2 f = cen - p; f = f.sqrMagnitude > 0.0001f ? f.normalized : Vector2.right;
                _pbGhosts.Add(new PbGhost { p = p, face = f, t = -i * T.phStaggerS1567, dmg = dmg * T.phDmgMul1567, flip = f.x < 0f, spr = spr, s = s, idx = i });
            }
            PhSpawned1567 += n; PhLastN1567 = n; PbCasts1567[6]++; HellordAudio.Play("whip_cast", PbPitch1567(6)); return true;
        }
        void UpdateGhosts1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbGhosts.Count - 1; i >= 0; i--)
            {
                var g = _pbGhosts[i]; g.t += dt;
                if (!g.chopped && g.t >= T.phWindS1567)
                {
                    g.chopped = true; float r = T.phArcRU1567 * Mathf.Sqrt(g.s.AreaMul); Query(g.p, r + 1.5f, _pbQ);
                    foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e) || !PbInArc1567(e, g.p, g.face, r, T.phArcHalfDeg1567, T.phInnerU1567)) continue; Vector2 kd = e.pos - g.p; kd = kd.sqrMagnitude > 0.0001f ? kd.normalized : g.face; PbHit1567(e, g.dmg, g.s, kd * T.phKnock1567); PhHits1567++; PbFxOnce1567(PbEv1567("ghost.hit", "xp_flash"), PbChest1567(e), YSort(e.pos.y) + 40); }
                    PbShake1567(6); PbFlash1567(6, g.p + g.face * (r * 0.5f)); PhLastR1567 = r;
                    if (PbA1567(PbEv1567("ghost.chop", "xp_chop")) == null) _pbArcs.Add(new PbArc { c = g.p + Vector2.up * T.chopArcLiftU1567, dir = g.face, r = r, half = T.phArcHalfDeg1567, sweep = T.chopSweepS1567, fade = T.chopFadeS1567, c1 = PbGhostCol, c2 = PbBlood, order = YSort(g.p.y) + 30, strip = "phantom_axes/arc" });
                    HellordAudio.Play("frost_cleave", PbPitch1567(6) * T.phChopPitchMul1567);
                }
                if (g.t >= T.phLifeS1567) _pbGhosts.RemoveAt(i);
            }
        }

        Sprite[] _phBody1567; bool _phBodyTried1567;
        Sprite[] PhBody1567()
        {
            var T = HellordTuning.I; if (!T.phHaBody1567) return null;
            if (!_phBodyTried1567) { _phBodyTried1567 = true; string n = Art47 + "hero_phantom_chop_2tone_36"; if (!HArt.Missing(n)) _phBody1567 = HArt.Strip(n, 5, T.hero36CellPx, T.hero36CellPx, T.hero36PivotPx); }
            return _phBody1567;
        }

        // =============================================================== update + draw
        void UpdatePb1567(float dt)
        {
            if (_pbSparks.Count + _pbOrbs.Count + _pbJavs.Count + _pbLines.Count + _pbArcs.Count + _pbRings.Count + _pbCracks.Count + _pbGhosts.Count + _pbPops.Count + _pbFx.Count + _pbFis.Count + _pbDiscs1567.Count + _pbHitFl1567.Count + _pbHeat1567.Count == 0 && _pbAxe == null && _pbN1567 == 0) return;
            UpdateSparks1567(dt); UpdateOrbs1567(dt); UpdateJavs1567(dt); UpdateChains1567(dt); UpdateFis1567(dt); UpdateCracks1567(dt); UpdateAxe1567(dt); UpdateGhosts1567(dt);
            for (int i = _pbLines.Count - 1; i >= 0; i--) { _pbLines[i].t += dt; if (_pbLines[i].t >= _pbLines[i].dur) _pbLines.RemoveAt(i); }
            _pbFxDt1567 = dt;
            for (int i = _pbArcs.Count - 1; i >= 0; i--) { _pbArcs[i].t += dt; if (_pbArcs[i].t >= _pbArcs[i].sweep + _pbArcs[i].fade) _pbArcs.RemoveAt(i); }
            for (int i = _pbRings.Count - 1; i >= 0; i--) { _pbRings[i].t += dt; if (_pbRings[i].t >= _pbRings[i].dur) _pbRings.RemoveAt(i); }
            for (int i = _pbPops.Count - 1; i >= 0; i--) { _pbPops[i].t += dt; if (_pbPops[i].t >= _pbPops[i].dur) _pbPops.RemoveAt(i); }
            DrawPb1567();
        }
        void PbPop1567(Vector2 p, float dur, Color c, int n, float r) { _pbPops.Add(new PbPop { p = p, dur = Mathf.Max(0.02f, dur), c = c, n = n, r = r, order = YSort(p.y - 0.3f) + 42 }); }
        void PbBolt1567(Vector2 a, Vector2 b, float dur, Color c, int order, int segs, float jit, int seed)
        {
            var ln = new PbLine { dur = dur, c = c, order = order }; Vector2 d = b - a, nrm = new Vector2(-d.y, d.x).normalized; ln.pts.Add(a);
            for (int q = 1; q < segs; q++) ln.pts.Add(a + d * (q / (float)segs) + nrm * ((((q + seed) & 1) == 0) ? jit : -jit)); ln.pts.Add(b); _pbLines.Add(ln);
        }
        float PbPitch1567(int i) { var a = HellordTuning.I.pbPitch1567; return a != null && i < a.Length ? a[i] : -1f; }

        // palette (existing Hellord colours: chain / shock blue, storm yellow, blood red, iron, dust)
        static readonly Color PbYellow = new Color(1f, 0.95f, 0.5f), PbCyan = new Color(0.7f, 0.9f, 1f), PbWhiteY = new Color(1f, 1f, 0.85f), PbSteel = new Color(0.86f, 0.88f, 0.92f),
            PbBlood = new Color(0.75f, 0.08f, 0.1f), PbBloodHi = new Color(1f, 0.25f, 0.2f), PbDust = new Color(0.62f, 0.5f, 0.38f), PbEmber = new Color(1f, 0.45f, 0.2f),
            PbCrackCol = new Color(0.12f, 0.06f, 0.05f), PbGhostCol = new Color(1f, 0.55f, 0.5f);

        void DrawPb1567()
        {
            var T = HellordTuning.I; PbBegin1567();
            // cracks (ground decals, under the actors)
            foreach (var k in _pbCracks)
            {
                if (k.fis != null && k.fis.v2) continue; // t1576: the v2 fissure draws its own baked decal
                if (PbA1567(PbEv1567("slam.crack", "xs_crack")) != null)
                {   // HFX code-stepped decal: 0 fresh, 1/2 alternate (tell) before the eruption, 3 spent while it slows, 4 / 5 fade
                    float fs = T.crackFadeS1567, tell = T.eruptDelayS1567 - T.crackTellS1567; int stg = k.t < tell ? 0 : !k.erupted ? 1 + ((int)((k.t - tell) * 30f) / Mathf.Max(1, T.crackBlinkF1567)) % 2 : k.t < k.life - fs ? 3 : k.t < k.life - fs * 0.5f ? 4 : 5;
                    PbArtPutF1567(PbEv1567("slam.crack", "xs_crack"), 0, stg, k.p, FbGround + YSort(k.p.y), (k.seed & 1) == 1, 1f, k.r > 0f ? PbMatch1567(k.r, T.slamCrackArtPx1567) : 1f); continue;
                }
                float a = Mathf.Clamp01(k.t / 0.08f) * Mathf.Clamp01((k.life - k.t) / 0.4f); var st = PbStrip1567("earth_slam/cracks");
                if (st != null) { PbPut1567(PbFrame1567("earth_slam/cracks", st, k.t, false), k.p, 0f, 1f, 1f, new Color(1, 1, 1, a), FbGround + YSort(k.p.y)); continue; }
                float r = T.crackRU1567 * Mathf.Sqrt(k.s.AreaMul); bool warn = !k.erupted && k.t > T.eruptDelayS1567 - T.eruptWarnS1567 && ((int)(k.t * 20f) & 1) == 0;
                Color cc = warn ? PbEmber : PbCrackCol; cc.a = a * (warn ? 0.9f : 0.85f); int ord = FbGround + YSort(k.p.y);
                for (int q = 0; q < 7; q++)
                {
                    float ang = q * 51.4f + ((k.seed * 37 + q * 23) % 30) - 15f; Vector2 d = PbRot(Vector2.right, ang); d.y *= 0.6f; float len = r * (0.65f + ((q * 7 + k.seed) % 4) * 0.1f);
                    Vector2 m = k.p + d * len * 0.5f + PbRot(d, 90f) * ((q & 1) == 0 ? 0.12f : -0.12f); PbLine1567(k.p, m, cc, ord, 1); PbLine1567(m, k.p + d * len, cc, ord, 1);
                }
                PbDots1567(k.p, r * 0.35f, 0.6f, new Color(cc.r, cc.g, cc.b, cc.a * 0.6f), ord, 10);
            }
            // sparks
            foreach (var k in _pbSparks)
            {
                float z = k.z0 * (1f - Mathf.Clamp01(k.t / Mathf.Max(0.01f, T.sparkDropS1567))); Vector2 p = k.p + Vector2.up * (z + 0.06f); int ord = YSort(k.p.y) + 20;
                p = k.p + Vector2.up * (z + T.sparkLiftPx1567 / HArt.PPU);
                if (PbA1567("ss2_spark") != null)
                {   // t1578 v2: dir16 row = current velocity + additive ground light under it
                    Vector2 vel = k.chaos ? k.v : k.steer ? new Vector2(Mathf.Cos(k.aimA + k.hr), Mathf.Sin(k.aimA + k.hr)) : k.dir; float ph = k.t + k.idx * T.sparkLoopPhaseF1567 * 0.05f; // HFX: loop frame = age / 50 ms + index x 3, kept across row swaps
                    PbArtPut1567("ss2_spark", PbRow16(vel), ph, true, p, ord); PbArtPut1567("ss2_ground", 0, ph, true, k.p, FbGround + YSort(k.p.y) + 12); continue;
                }
                if (PbArtPut1567("ss_spark", 0, k.t + k.idx * 3f / 30f, true, p, ord)) continue; // HFX loop, phase = index x 3 f
                bool fl = ((int)(k.t * 30f) + k.idx & 1) == 0; PbPut1567(PbDot, p, 0f, 2f, 2f, fl ? PbWhiteY : PbYellow, ord);
                for (int q = 1; q <= 3; q++) PbPut1567(PbDot, p - k.dir * (q * 0.09f) + k.nrm * (((q + k.idx) & 1) == 0 ? 0.04f : -0.04f), 0f, 1f, 1f, new Color(PbCyan.r, PbCyan.g, PbCyan.b, 1f - q * 0.25f), ord - 1);
            }
            // orbs
            foreach (var b in _pbOrbs)
            {
                Vector2 p = b.p + Vector2.up * T.orbHeightU1567; int ord = YSort(b.p.y) + 14; float pa = b.die >= 0f ? 1f - Mathf.Clamp01(b.die / Mathf.Max(0.01f, T.orbPopS1567)) : 1f;
                PbPut1567(PbDisc, b.p, 0f, 0.6f, 0.25f, new Color(0f, 0f, 0f, 0.35f * pa), FbGround + YSort(b.p.y) + 10); // shadow
                if (PbA1567("bl_orb") != null)
                {   // HFX: spawn 6 x 40 ms, then the 12-frame loop; fade 8 x 50 ms when it ends or is popped; 1-frame shadow on the ground point
                    _pbSr1567[_pbN1567 - 1].enabled = false; int o2 = YSort(b.p.y) + 30; float sl = PbArtLen1567("bl_spawn"); int gO2 = FbGround + YSort(b.p.y) + 12;
                    if (PbA1567("bl2_ground") != null)
                    {   // t1584 HFX v2: orb_ground light replaces the shadow; spawn / fade flash from frame 2; fade_ground at the ground point
                        if (b.die >= 0f) { PbArtPut1567("bl2_fadeground", 0, b.die, false, b.p, gO2); PbArtPut1567("bl2_fadeflash", 0, b.die - 0.1f, false, p, PbTop1567(b.p)); }
                        else { PbArtPut1567("bl2_ground", 0, b.t, true, b.p, gO2); if (b.t >= 0.08f) PbArtPut1567("bl2_spawnflash", 0, b.t - 0.08f, false, p, PbTop1567(b.p)); }
                    }
                    else PbArtPut1567("bl_shadow", 0, 0f, true, b.p, FbGround + YSort(b.p.y) + 10);
                    if (b.die >= 0f) PbArtPut1567("bl_fade", 0, b.die, false, p, o2); else if (b.t < sl) PbArtPut1567("bl_spawn", 0, b.t, false, p, o2); else PbArtPut1567("bl_orb", 0, b.t - sl, true, p, o2);
                    continue;
                }
                float pulse = 0.85f + 0.15f * Mathf.Sin(b.t * 18f); PbPut1567(PbDisc, p, 0f, 0.62f * pulse * (2f - pa), 0.62f * pulse * (2f - pa), new Color(PbCyan.r, PbCyan.g, PbCyan.b, 0.5f * pa), ord);
                PbPut1567(PbDisc, p, 0f, 0.38f, 0.38f, new Color(PbWhiteY.r, PbWhiteY.g, PbWhiteY.b, pa), ord + 1);
                for (int q = 0; q < 4; q++) { float ang = b.t * 260f + q * 90f + ((int)(b.t * 12f) % 2) * 20f; Vector2 d = PbRot(Vector2.right, ang); PbPut1567(PbDot, p + d * 0.5f, 0f, 1f, 1f, new Color(PbYellow.r, PbYellow.g, PbYellow.b, pa), ord + 2); PbPut1567(PbDot, p + d * 0.62f, 0f, 1f, 1f, new Color(PbCyan.r, PbCyan.g, PbCyan.b, 0.7f * pa), ord + 2); }
            }
            // javelins
            foreach (var j in _pbJavs)
            {
                Vector2 p = j.p + Vector2.up * T.javHeightU1567; int ord = YSort(j.p.y) + 30; float ang = Mathf.Atan2(j.dir.y, j.dir.x) * Mathf.Rad2Deg;
                PbArtPut1567("tj2_ground", PbRow16(j.dir), j.t, true, j.p, FbGround + YSort(j.p.y) + 12); // t1579 v2 ground light (no-op without v2)
                if (PbArtPut1567("tj_flight", PbRow16(j.dir), j.t, true, p, ord)) continue; // HFX dir16 flight loop
                PbLine1567(p - j.dir * 0.65f, p, PbYellow, ord, 1); PbLine1567(p - j.dir * 0.35f, p + j.dir * 0.06f, PbWhiteY, ord + 1, 2); PbPut1567(PbDot, p - j.dir * 0.8f, 0f, 1f, 1f, new Color(PbCyan.r, PbCyan.g, PbCyan.b, 0.6f), ord);
            }
            // axe
            if (_pbAxe != null)
            {
                var x = _pbAxe; Vector2 p = x.p + Vector2.up * T.axeHeightU1567; int ord = YSort(x.p.y) + 30; var st = PbStrip1567("twin_axe/axe");
                if (!PbArtPutF1567("xa2_shadow", 0, x.phase == 1 ? 1 : 0, x.p, FbGround + YSort(x.p.y) + 10)) // t1586 v2 ragged shadow (frame 1 = smaller), else the disc
                PbPut1567(PbDisc, x.p, 0f, 0.4f, 0.16f, new Color(0, 0, 0, 0.3f), FbGround + YSort(x.p.y) + 10);
                if (PbA1567("xa_spin") != null)
                {   // HFX: trail (row = travel) under the smear under the axe; spin index += stride per frame; flip X when thrown left
                    bool fl = x.b.x < x.a.x; int fi = (int)(x.life * 30f) * Mathf.Max(1, T.axeSpinStride1567); Vector2 vd = x.vel.sqrMagnitude > 1e-8f ? x.vel : x.b - x.a;
                    if (x.phase != 1) PbArtPut1567("xa_trail", PbRow16(vd), x.life, true, p, ord - 2);
                    PbArtPutF1567("xa_smear", 0, fi, p, ord - 1, fl); PbArtPutF1567("xa_spin", 0, fi, p, ord, fl); st = null;
                }
                else
                if (st != null) PbPut1567(PbFrame1567("twin_axe/axe", st, x.life, true), p, 0f, 1f, 1f, Color.white, ord); else PbPut1567(PbAxeSpr, p, -x.spin, 1f, 1f, Color.white, ord);
            }
            // ghosts
            foreach (var g in _pbGhosts)
            {
                if (g.t < 0f) continue;
                string gk = PbEv1567("ghost.chop", "xp_chop"); if (PbA1567(gk) != null)
                {   // HFX chop (pivot = ghost feet, row = ghost -> cursor) + HA 2-tone ghost body on the same point, faded by the HFX opacity curve
                    int fi = Mathf.Min(15, (int)(g.t * 1000f / 33f)); var hb = PhBody1567();
                    if (hb != null) { var mp = T.phBodyMap1567; int bi = mp != null && fi < mp.Length ? Mathf.Clamp(mp[fi], 0, hb.Length - 1) : 0; var oc = T.phOpacity1567; float op = oc != null && fi < oc.Length ? oc[fi] : 0.55f; PbPut1567(hb[bi], g.p, 0f, 1f, 1f, new Color(1f, 1f, 1f, op), YSort(g.p.y) + 5, g.flip); }
                    float gs = PbMatch1567(T.phArcRU1567 * Mathf.Sqrt(g.s.AreaMul), T.phArtReachPx1567); PhArtScale1567 = gs; PbArtPutF1567(gk, PbRow16(g.face), fi, g.p, YSort(g.p.y) + 7, false, 1f, gs); continue;
                }
                float a = Mathf.Clamp01((g.t + 0.0001f) / 0.08f) * Mathf.Clamp01((T.phLifeS1567 - g.t) / 0.15f); var st = PbStrip1567("phantom_axes/ghost");
                Color tc = new Color(T.phTint1567.r, T.phTint1567.g, T.phTint1567.b, T.phTint1567.a * a); Sprite sp = st != null ? PbFrame1567("phantom_axes/ghost", st, g.t, false) : g.spr;
                if (sp != null) PbPut1567(sp, g.p, 0f, 1f, 1f, tc, YSort(g.p.y) + 5, g.flip);
                if (!g.chopped) { float k = Mathf.Clamp01(g.t / Mathf.Max(0.01f, T.phWindS1567)); Vector2 ax = g.p + Vector2.up * (1.4f + 0.3f * k) - g.face * 0.3f * k; PbPut1567(PbAxeSpr, ax, g.face.x < 0 ? 40f : -40f, 1f, 1f, tc, YSort(g.p.y) + 6, g.face.x < 0); }
            }
            // arcs
            foreach (var r in _pbArcs)
            {
                float k = Mathf.Clamp01(r.t / Mathf.Max(0.01f, r.sweep)), a = r.t <= r.sweep ? 1f : 1f - Mathf.Clamp01((r.t - r.sweep) / Mathf.Max(0.01f, r.fade));
                var st = PbStrip1567(r.strip); float baseAng = Mathf.Atan2(r.dir.y, r.dir.x) * Mathf.Rad2Deg;
                if (st != null) { PbPut1567(PbFrame1567(r.strip, st, r.t, false), r.c, baseAng, 1f, 1f, new Color(1, 1, 1, a), r.order); continue; }
                int n = Mathf.Max(6, Mathf.RoundToInt(r.r * r.half * Mathf.Deg2Rad * 2f * 16f / 2f)); int shown = Mathf.Max(1, Mathf.RoundToInt(n * k));
                for (int q = 0; q < shown; q++)
                {
                    float u = n > 1 ? q / (float)(n - 1) : 0.5f; float ang = -r.half + 2f * r.half * u; Vector2 d = PbRot(r.dir, ang); d.y *= 0.75f;
                    float tail = 1f - Mathf.Clamp01((shown - 1 - q) / (float)Mathf.Max(1, n)) * 0.6f;
                    PbPut1567(PbDot, r.c + d * r.r, 0f, 2f, 2f, new Color(r.c1.r, r.c1.g, r.c1.b, a * tail), r.order + 1);
                    PbPut1567(PbDot, r.c + d * (r.r - 0.16f), 0f, 1f, 1f, new Color(r.c2.r, r.c2.g, r.c2.b, a * tail), r.order);
                    if ((q & 1) == 0) PbPut1567(PbDot, r.c + d * (r.r - 0.32f), 0f, 1f, 1f, new Color(r.c2.r, r.c2.g, r.c2.b, a * tail * 0.5f), r.order);
                }
            }
            // rings
            foreach (var g in _pbRings)
            {
                float k = Mathf.Clamp01(g.t / Mathf.Max(0.01f, g.dur)); float rr = Mathf.Lerp(g.r0, g.r1, 1f - (1f - k) * (1f - k)); float a = 1f - k * k;
                var st = PbStrip1567(g.strip); if (st != null) { PbPut1567(PbFrame1567(g.strip, st, g.t, false), g.c, 0f, 1f, 1f, new Color(1, 1, 1, 1f), g.order); continue; }
                PbDots1567(g.c, rr, 0.6f, new Color(g.col.r, g.col.g, g.col.b, a), g.order, Mathf.Max(10, Mathf.RoundToInt(rr * 6.2832f * 16f / 2.5f)));
            }
            // lines (forks, fallback zaps)
            foreach (var l in _pbLines) { float a = 1f - Mathf.Clamp01(l.t / Mathf.Max(0.01f, l.dur)) * 0.7f; if (l.t < 0f) continue; bool vis = ((int)(l.t * 30f) & 1) == 0 || l.t < 0.05f; if (!vis) continue; for (int q = 0; q + 1 < l.pts.Count; q++) PbLine1567(l.pts[q], l.pts[q + 1], new Color(l.c.r, l.c.g, l.c.b, a), l.order, l.w); }
            // pops
            foreach (var p in _pbPops)
            {
                float k = Mathf.Clamp01(p.t / p.dur); for (int q = 0; q < p.n; q++) { Vector2 d = PbRot(Vector2.right, q * 360f / p.n + p.n * 11f); PbPut1567(PbDot, p.p + d * (p.r * (0.3f + 0.7f * k)), 0f, 1f, 1f, new Color(p.c.r, p.c.g, p.c.b, 1f - k), p.order); }
            }
            UpdateDrawPbFx1567(_pbFxDt1567, true); // HFX one-shots (hits, zaps, forks, trails)
            DrawFis1567(); UpdateDrawPbHooks1567(_pbFxDt1567); // t1572 fissure seam + light-flash discs / hit-flash pass / heat wave
            PbEnd1567();
        }
        float _pbFxDt1567;

        // ---------------------------------------------------------------- tiny sprite pool (immediate mode: every frame re-puts what is alive)
        readonly List<SpriteRenderer> _pbSr1567 = new List<SpriteRenderer>(); Material _pbDefMat1567; int _pbN1567; Transform _pbRoot1567;
        static Sprite _pbDot, _pbLineSpr, _pbDisc, _pbAxeSpr;
        static Sprite PbDot => _pbDot != null ? _pbDot : (_pbDot = PbMake1567(1, 1, (x, y) => Color.white, 0.5f, 0.5f));
        static Sprite PbLineS => _pbLineSpr != null ? _pbLineSpr : (_pbLineSpr = PbMake1567(1, 1, (x, y) => Color.white, 0f, 0.5f));
        static Sprite PbDisc => _pbDisc != null ? _pbDisc : (_pbDisc = PbMake1567(16, 16, (x, y) => { float dx = x - 7.5f, dy = y - 7.5f; return dx * dx + dy * dy <= 60f ? Color.white : Color.clear; }, 0.5f, 0.5f));
        static readonly string[] PbAxeRows = {   // 11x11 placeholder axe (code-drawn): o = iron edge, i = iron, h = haft, k = dark
            "....oo.....",
            "...oiik....",
            "..oiiik....",
            "..oiiikh...",
            "...oiikhh..",
            "....kk.hh..",
            ".......hh..",
            "........hh.",
            "........hh.",
            ".........h.",
            "..........." };
        static Sprite PbAxeSpr => _pbAxeSpr != null ? _pbAxeSpr : (_pbAxeSpr = PbMake1567(11, 11, (x, y) =>
        {
            char c = PbAxeRows[10 - y][x]; return c == 'o' ? new Color(0.92f, 0.94f, 0.97f) : c == 'i' ? new Color(0.62f, 0.64f, 0.7f) : c == 'k' ? new Color(0.3f, 0.3f, 0.36f) : c == 'h' ? new Color(0.45f, 0.22f, 0.12f) : Color.clear;
        }, 0.5f, 0.5f));
        static Sprite PbMake1567(int w, int h, System.Func<int, int, Color> f, float px, float py)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) t.SetPixel(x, y, f(x, y)); t.Apply(false, false);
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(px, py), HArt.PPU, 0, SpriteMeshType.FullRect);
        }
        void PbBegin1567() { _pbN1567 = 0; }
        void PbEnd1567() { for (int i = _pbN1567; i < _pbSr1567.Count; i++) if (_pbSr1567[i] != null && _pbSr1567[i].enabled) _pbSr1567[i].enabled = false; }
        void PbPut1567(Sprite sp, Vector2 p, float rot, float sx, float sy, Color c, int order, bool flip = false, Material mat = null)
        {
            if (sp == null) return; if (_pbRoot1567 == null) { _pbRoot1567 = new GameObject("PactBasics1567").transform; _pbRoot1567.SetParent(transform, false); }
            if (_pbN1567 >= _pbSr1567.Count) { var go = new GameObject("pb"); go.transform.SetParent(_pbRoot1567, false); var nsr = go.AddComponent<SpriteRenderer>(); if (_heroBase != null) nsr.sortingLayerID = _heroBase.sortingLayerID; if (_pbDefMat1567 == null) _pbDefMat1567 = nsr.sharedMaterial; _pbSr1567.Add(nsr); }
            var sr = _pbSr1567[_pbN1567++]; var m = mat != null ? mat : _pbDefMat1567; if (m != null && sr.sharedMaterial != m) sr.sharedMaterial = m; sr.enabled = true; sr.sprite = sp; sr.color = c; sr.sortingOrder = order; sr.flipX = flip;
            sr.transform.position = new Vector3(Snap(p.x), Snap(p.y), 0f); sr.transform.rotation = Quaternion.Euler(0, 0, rot); sr.transform.localScale = new Vector3(sx, sy, 1f);
        }
        void PbLine1567(Vector2 a, Vector2 b, Color c, int order, int w)
        {
            Vector2 d = b - a; float l = d.magnitude; if (l < 0.01f) return; if (_pbRoot1567 == null) PbPut1567(PbDot, a, 0, 0, 0, Color.clear, order);
            PbPut1567(PbLineS, a, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, l * HArt.PPU, w, c, order); _pbSr1567[_pbN1567 - 1].transform.position = new Vector3(Snap(a.x), Snap(a.y), 0f);
        }
        void PbDots1567(Vector2 c, float r, float ySc, Color col, int order, int n) { for (int q = 0; q < n; q++) { float ang = q * Mathf.PI * 2f / n; PbPut1567(PbDot, c + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r * ySc), 0f, 1f, 1f, col, order); } }

        // ---------------------------------------------------------------- HA / HFX strip loader (the ONLY place art is looked up)
        class PbStripInfo { public Sprite[] f; public float ms = 50f; }
        static readonly Dictionary<string, PbStripInfo> _pbStrips1567 = new Dictionary<string, PbStripInfo>();
        static Sprite[] PbStrip1567(string key)
        {
            var T = HellordTuning.I; if (T == null || !T.pbArt1567 || string.IsNullOrEmpty(key)) return null;
            if (_pbStrips1567.TryGetValue(key, out var si)) return si.f;
            si = new PbStripInfo(); var t = Resources.Load<Texture2D>("HellordArt/Skills1567/" + key);
            if (t != null)
            {
                t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; int fw = t.height; float px = -1f, py = -1f;
                if (T.pbStripFrames1567 != null) foreach (var line in T.pbStripFrames1567) { if (line == null || !line.StartsWith(key + ":")) continue; var p = line.Substring(key.Length + 1).Split(':'); if (p.Length > 0) int.TryParse(p[0], out fw); if (p.Length > 2) { float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out px); float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out py); } if (p.Length > 3) float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out si.ms); }
                fw = Mathf.Clamp(fw, 1, t.width); int n = Mathf.Max(1, t.width / fw); si.f = new Sprite[n]; Vector2 piv = px >= 0f ? new Vector2(px / fw, 1f - py / t.height) : new Vector2(0.5f, 0.5f);
                for (int i = 0; i < n; i++) si.f[i] = Sprite.Create(t, new Rect(i * fw, 0, fw, t.height), piv, HArt.PPU, 0, SpriteMeshType.FullRect);
            }
            _pbStrips1567[key] = si; return si.f;
        }
        static Sprite PbFrame1567(string key, Sprite[] f, float t, bool loop)
        {
            float ms = _pbStrips1567.TryGetValue(key, out var si) ? Mathf.Max(1f, si.ms) : 50f; int i = Mathf.FloorToInt(Mathf.Max(0f, t) * 1000f / ms); i = loop ? i % f.Length : Mathf.Min(i, f.Length - 1); return f[i];
        }
        /// <summary>which keys found real art (for the report / test log)</summary>
        public static string PbArtReport1567()
        {
            var keys = new[] { "spark_spray/spark", "ball_lightning/orb", "thunder_javelin/javelin", "thunder_javelin/burst", "headsmans_chop/arc", "headsmans_chop/blood_wave", "earth_slam/impact", "earth_slam/cracks", "earth_slam/erupt", "twin_axe/axe", "phantom_axes/ghost", "phantom_axes/arc" };
            var sb = new System.Text.StringBuilder(PbArtKeys1567()); foreach (var k in keys) if (PbStrip1567(k) != null) sb.Append(k).Append("=ART "); for (int i = 0; i < 7; i++) sb.Append("icon_" + PbIconName1567[i]).Append(PbIconArt1567(i) ? "=ART " : "=placeholder "); return sb.ToString();
        }

        // ---------------------------------------------------------------- 24 px icons (HA icons/icon_<key>_24.png when present, else code-drawn placeholders)
        static readonly Texture2D[] _pbIcon1567 = new Texture2D[7]; static readonly bool[] _pbIconArt1567 = new bool[7];
        static bool PbIconArt1567(int i) { PbIcon1567(SkillId.SparkSpray + i); return _pbIconArt1567[i]; }
        static Texture2D PbIcon1567(SkillId id)
        {
            int i = PbIx1567(id); if (i < 0 || i >= 7) return null; if (_pbIcon1567[i] != null) return _pbIcon1567[i];
            var T = HellordTuning.I; Texture2D t = T != null && T.pbArt1567 ? Resources.Load<Texture2D>("HellordArt/Skills1567/icons/icon_" + PbIconName1567[i] + "_24") : null;
            if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; _pbIconArt1567[i] = true; return _pbIcon1567[i] = t; }
            return _pbIcon1567[i] = PbDrawIcon1567(i);
        }
        static Texture2D PbDrawIcon1567(int i)
        {
            var px = new Color[24 * 24]; bool leizu = i < 3; Color bg = new Color(0.09f, 0.07f, 0.11f), rim = leizu ? new Color(0.95f, 0.82f, 0.32f) : new Color(0.72f, 0.12f, 0.12f), rim2 = new Color(0.2f, 0.16f, 0.2f);
            System.Action<int, int, Color> P = (x, y, c) => { if (x >= 0 && y >= 0 && x < 24 && y < 24) px[(23 - y) * 24 + x] = c; };
            System.Action<int, int, int, int, Color> L = (x0, y0, x1, y1, c) => { int n = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)); for (int k = 0; k <= n; k++) P(Mathf.RoundToInt(Mathf.Lerp(x0, x1, n == 0 ? 0 : k / (float)n)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, n == 0 ? 0 : k / (float)n)), c); };
            System.Action<int, int, float, Color, bool> C = (cx, cy, r, c, fill) => { for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++) { float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)); if (fill ? d <= r : Mathf.Abs(d - r) < 0.6f) P(x, y, c); } };
            for (int k = 0; k < px.Length; k++) px[k] = bg; for (int k = 0; k < 24; k++) { P(k, 0, rim); P(k, 23, rim); P(0, k, rim); P(23, k, rim); P(k, 1, rim2); P(k, 22, rim2); P(1, k, rim2); P(22, k, rim2); }
            Color Y = PbYellow, W = PbWhiteY, B = PbCyan, S = PbSteel, Rd = PbBlood, Rh = PbBloodHi, H = new Color(0.45f, 0.22f, 0.12f), Dk = new Color(0.3f, 0.3f, 0.36f), Du = PbDust;
            switch (i)
            {
                case 0: // Spark Spray: 4 sparks fanning right with wiggly trails
                    { int[,] s = { { 15, 6 }, { 18, 10 }, { 18, 14 }, { 15, 18 } }; for (int k = 0; k < 4; k++) { int x = s[k, 0], y = s[k, 1]; P(x, y, W); P(x + 1, y, W); P(x, y + 1, W); P(x + 1, y + 1, Y); for (int q = 1; q < 9; q++) P(x - q, y + ((q / 2) % 2 == 0 ? 0 : (k < 2 ? 1 : -1)) + (12 - y) * q / 12, q < 4 ? Y : B); } P(4, 12, W); P(5, 12, Y); break; }
                case 1: // Ball Lightning: bright orb, cyan halo, 4 crackle rays
                    C(12, 12, 4.5f, W, true); C(12, 12, 3f, new Color(1f, 1f, 1f), true); C(12, 12, 6.5f, B, false); L(12, 3, 13, 5, Y); L(19, 8, 21, 7, Y); L(4, 15, 6, 14, Y); L(15, 19, 16, 21, Y); break;
                case 2: // Thunder Javelin: diagonal bolt, forks at the tip
                    L(3, 20, 15, 8, Y); L(4, 20, 16, 8, W); P(17, 7, W); P(16, 6, W); L(17, 7, 21, 5, B); L(17, 7, 20, 11, B); L(17, 7, 16, 3, B); L(19, 9, 21, 10, Y); break;
                case 3: // Headsman's Chop: wide axe arc + head
                    for (int a = -20; a <= 95; a += 3) { float r = a * Mathf.Deg2Rad; P(5 + Mathf.RoundToInt(Mathf.Cos(r) * 14f), 19 - Mathf.RoundToInt(Mathf.Sin(r) * 14f), S); P(5 + Mathf.RoundToInt(Mathf.Cos(r) * 12f), 19 - Mathf.RoundToInt(Mathf.Sin(r) * 12f), Rd); }
                    L(5, 19, 11, 13, H); L(11, 9, 14, 12, S); L(10, 10, 13, 13, S); L(9, 11, 12, 14, Dk); P(16, 16, Rh); P(17, 17, Rd); P(19, 15, Rd); break;
                case 4: // Earth-Splitting Slam: axe head down on a cracked ground line
                    L(3, 16, 20, 16, Du); L(12, 16, 9, 20, Dk); L(9, 20, 7, 21, Dk); L(12, 16, 15, 19, Dk); L(15, 19, 18, 21, Dk); L(12, 16, 12, 21, Dk); L(12, 4, 12, 11, H); L(9, 11, 15, 11, S); L(9, 12, 15, 12, S); L(10, 13, 14, 13, S); L(10, 14, 14, 14, Dk); P(6, 14, Du); P(18, 13, Du); P(16, 14, Du); P(7, 13, Du); break;
                case 5: // Twin Axe Throw: spinning axe + return arrow
                    L(7, 15, 15, 7, H); L(13, 5, 17, 9, S); L(12, 6, 16, 10, S); L(14, 4, 18, 8, W); for (int a = 200; a <= 340; a += 12) { float r = a * Mathf.Deg2Rad; P(12 + Mathf.RoundToInt(Mathf.Cos(r) * 8f), 13 - Mathf.RoundToInt(Mathf.Sin(r) * 7f), Rh); } P(5, 15, Rh); P(4, 14, Rh); P(4, 16, Rh); break;
                default: // Phantom Axes: translucent ghost figure with an axe
                    { var G = new Color(0.85f, 0.32f, 0.32f); var G2 = new Color(0.5f, 0.15f, 0.18f); C(10, 7, 2.6f, G, true); for (int y = 10; y <= 20; y++) { int hw = 2 + (y - 10) / 3; for (int x = 10 - hw; x <= 10 + hw; x++) P(x, y, ((x + y) & 1) == 0 ? G : G2); } L(14, 15, 19, 6, H); L(17, 4, 21, 8, S); L(16, 5, 20, 9, S); P(9, 7, bg); P(11, 7, bg); break; }
            }
            var t = new Texture2D(24, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; t.SetPixels(px); t.Apply(false, false); return t;
        }
    }
}
