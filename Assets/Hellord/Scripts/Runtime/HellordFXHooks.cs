using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.3: every Pixel FX Pack call that is a "combo" (no dedicated effect in the pack) goes through HFX below,
// following PixelFXPack/HELLORD_FX_MAP.md. v0.4: Pixel FX Pack v1.6.0 ReactionFX first; the v0.3 combos are kept as
// fallbacks for when ReactionFX reports failure (pool/cap full). ReactionFX hit hooks are NOT used: they need Collider2D,
// our horde is collider-less (hash grid), so all damage stays in game code and ReactionFX is purely visual. Swap any of these bodies to change the look in one place.
namespace Hellord
{
    public static class HFX
    {
        static readonly Color Ice = new Color(0.6f, 0.9f, 1f), Gold = new Color(1f, 0.82f, 0.3f), ShockBlue = new Color(0.7f, 0.9f, 1f);
        static PixelBurstSettings _shatter, _blood, _sparkle;
        static float _shockZapT, _bleedT, _overloadT;

        /// <summary>Once at startup: PPU for every runner + prewarm the pooled chain renderer.</summary>
        public static void Setup()
        {
            HellFX.PPU = HArt.PPU;
            HordeFX.I.pixelsPerUnit = HArt.PPU;
            ChainFXRunner.I.Prewarm();
            ReactionFXRunner.StatusCap = 24;  // v0.0.15: was 48 = the whole pack pool -> shocked crowds starved Frost Cleave/bursts of slots (cleave silently fell back)    // v0.5: shared live Shock + Bleed + Burn + Chill (pack max 48)
            ReactionFXRunner.I.Prewarm();
            TellFXRunner.I.Prewarm();           // v0.5 (pack v1.8.4): TellFX + StatusFX pool, 2304 renderers
            // Our enemy loop writes SpriteRenderer.color every frame (art tint x status tint), so StatusFX must not also drive it:
            // StatusFX draws only its overlays (flame crown / frost glint / ice shards), our colour tint stays the single owner.
            StatusFX.TintTargets = false;
            ReactionFX.OnCleaveHit = null; ReactionFX.OnOverloadHit = null; // visual only (no colliders)
        }

        /// <summary>Frost Cleave arc: Slash (player-only, unpooled is fine at ~1/s) + ice light pop + 3 cheap pooled bursts.</summary>
        public static int CleaveOk, CleaveFallbacks;
        /// <summary>t990u: optional override (unused when frostCleaveShader990 is on). Kept for tests.</summary>
        public static System.Func<Vector2, Vector2, float, float, bool> FrostCleaveOverride;
        /// <summary>Set by Cast before calling FrostCleave so hit-stop holds match the mock when something was hit.</summary>
        public static bool FrostCleaveAnyHit990;
        public static void FrostCleave(Vector2 c, Vector2 dir, float r, float area, float half = 65f)
        {
            // t990u: VH-locked sample_v7 PixelFrostCleave (fx_audit/frost_cleave_wire). Off / missing = FrostArcFX.
            if (HellordTuning.I.frostCleaveShader990 && FrostCleave990.Ok)
            { FrostCleave990.Spawn(c, dir, r, half, dir.x >= 0, FrostCleaveAnyHit990); FrostCleaveAnyHit990 = false; return; }
            if (FrostCleaveOverride != null && FrostCleaveOverride(c, dir, r, half)) return;
            // v0.0.35 fallback: FrostArcFX draws the swing at EXACTLY the radius r and half-angle the damage test uses.
            float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            FrostArcFX.Spawn(c, deg, r, half, dir.x >= 0);
            bool full = half >= 179f;
            if (r > 2.6f)
            {
                int k = Mathf.Clamp(Mathf.RoundToInt(half * 2f / 36f), 3, 10);
                for (int i = 0; i < k; i++) { float a = deg + (k == 1 ? 0f : (i / (float)(k - 1) - 0.5f) * (full ? 360f : half * 2f)); HordeFX.CheapBurst(c + (Vector2)(Quaternion.Euler(0, 0, a) * Vector2.right) * r * 0.92f, Ice, 5); }
            }
            CleaveOk++; HordeFX.CheapBurst(c + dir * r * 0.85f, Ice, 6);
        }
        /// <summary>Frozen enemy shatters on death (cached burst settings, no allocation).</summary>
        public static void Shatter(Vector2 pos)
        {
            if (IceFX3.Ok) { var p = pos + Vector2.up * 0.3f; HordeFX.CheapBurst(p, IceArt.Pale, 3); HordeFX.CheapBurst(p, IceArt.Cyan, 4); HordeFX.CheapBurst(p, IceArt.Mid, 3); return; } // t1409u v3: a few small chips, no white burst
            if (ReactionFX.FrostShatter(pos, 10, 14)) return;
            if (_shatter == null) _shatter = new PixelBurstSettings { count = 14, speed = new Vector2(1.5f, 4f), spreadDegrees = 360, lifetime = new Vector2(0.25f, 0.5f), gravity = 6f, drag = 1.5f, sizePixels = new Vector2Int(1, 2), colorOverLife = PixelBurstSettings.DefaultGradient(Color.white, Ice), colorSteps = 3 };
            PixelParticles.Burst(_shatter, pos);
            ProjectileFX.Impact(pos, ProjElement.Soul, Vector2.zero);
        }
        /// <summary>Shock applied: short pooled 2-point zap on the enemy, rate-limited.</summary>
        /// <summary>Starts a pooled ReactionFX shock/bleed status on the enemy (no material change). -1 = failed -> caller uses stand-ins.</summary>
        public static int ShockStatus(Transform t, float seconds, float radiusUnits) => ReactionFX.ShockStatus(t, seconds, Mathf.Clamp(radiusUnits * HArt.PPU, 5, 14), radiusUnits * HArt.PPU);
        public static int BleedStatus(Transform t, float seconds, float radiusUnits) => ReactionFX.BleedStatus(t, seconds, Mathf.Max(6, radiusUnits * 2 * HArt.PPU), 0);
        /// <summary>v0.5: StatusFX burn / chill overlays (loop until we stop them; feet = enemy root).</summary>
        public static int BurnStatus(Enemy e) => StatusFX.BurnStatus(e.tr, 0f, Mathf.Clamp(e.radius * 2.5f * HArt.PPU, 6, 24), 0f);
        public static int ChillStatus(Enemy e) => !e.boss && IceFX3.Ok ? -1 : StatusFX.ChillStatus(e.tr, 0f, e.freezeT > 0, Mathf.Clamp(e.radius * 2.5f * HArt.PPU, 6, 24), Mathf.Clamp(e.radius * 2f * HArt.PPU, 4, 24), 0f);
        public static void StopTell(ref int handle) { if (handle >= 0) StatusFX.StopStatus(handle); handle = -1; }
        public static void Stop(ref int handle) { if (handle >= 0) ReactionFX.StopStatus(handle); handle = -1; }
        public static bool Alive(int handle) => handle >= 0 && ReactionFX.IsAlive(handle);
        /// <summary>Stand-in shock zap (v0.3), used when ShockStatus fails.</summary>
        public static void Shock(Vector2 top)
        {
            if (Time.time < _shockZapT) return; _shockZapT = Time.time + 0.12f;
            ChainFX.ChainLightningPoints(top + new Vector2(-0.3f, 0.5f), top + new Vector2(0.25f, -0.2f), default, default, 2, ShockBlue, 0.05f, 0.1f);
        }
        /// <summary>Bleed tick: cached blood burst (Impact would add shake), rate-limited.</summary>
        public static void Bleed(Vector2 pos)
        {
            if (Time.time < _bleedT) return; _bleedT = Time.time + 0.05f;
            if (_blood == null) _blood = new PixelBurstSettings { count = 3, speed = new Vector2(0.8f, 2f), directionDegrees = 90, spreadDegrees = 120, lifetime = new Vector2(0.25f, 0.45f), gravity = 7f, drag = 1f, sizePixels = new Vector2Int(1, 1), colorOverLife = HellPalette.BloodGradient(), colorSteps = 2 };
            PixelParticles.Burst(_blood, pos);
        }
        /// <summary>Burn + Shock overload: Hellfire Nova (no screen pass) + pooled chain zaps + tiny hit-stop (rate-limited).</summary>
        public static void Overload(Vector2 pos, float r, IList<Vector3> nearby)
        {
            // ReactionFX.Overload returns a hit count, and 0 both on pool-full and on "no colliders" (always, for us),
            // so failure can't be detected -> it is always called, and the cheap pooled chain zaps below are always kept as well.
            ReactionFX.Overload(pos, Mathf.Clamp(r * HArt.PPU, 10, 64), 0.5f, true);
            for (int i = 0; i < nearby.Count && i < 3; i++) ChainFX.ChainLightningPoints(pos, nearby[i], default, default, 2, ShockBlue, 0.04f, 0.14f);
            return;
        }
        public static void OverloadStandIn(Vector2 pos, float r, IList<Vector3> nearby)
        {
            HellFX.HellfireNova(pos, r * HArt.PPU, 0.4f, false);
            for (int i = 0; i < nearby.Count && i < 3; i++) ChainFX.ChainLightningPoints(pos, nearby[i], default, default, 2, ShockBlue, 0.04f, 0.14f);
            ScreenShake.Add(2.5f, 0.15f);
            if (Time.unscaledTime > _overloadT) { _overloadT = Time.unscaledTime + 0.5f; HitStop.Play(0.04f); }
        }
        /// <summary>Elite chest drop/open: gold light pop + light column + sparkle (cached) + ring.</summary>
        public static void Chest(Vector2 feet, bool open)
        {
            if (open && ReactionFX.EliteChestOpen(feet, 1.6f, 48, 6, 3)) return; // chest sprite is hidden by game code at the call
            if (_sparkle == null) _sparkle = PixelBurstSettings.Sparkle();
            PixelFXSpawn.LightPop(feet + Vector2.up * 0.4f, Gold, open ? 28 : 18, 0.15f, 40, HArt.PPU);
            PixelParticles.Burst(_sparkle, feet + Vector2.up * 0.4f);
            if (open) { PixelVFX.TeleportBeam(feet, Gold, 64, 0.6f, 75, HArt.PPU); PixelFXSpawn.Shockwave(feet, 20, 0.35f, 60, HArt.PPU); }
        }
    }

    public partial class HellordGame
    {
        // ---------- status visuals (PixelFX Uber material breaks batching -> capped) ----------
        const int MaxStatusFx = 8;   // elites always; other enemies only while fewer than this many have one
        int _statusFxCount;
        void UpdateStatusFx(Enemy e)
        {
            bool tint = runTime < e.tint948Until; bool ice3 = !e.boss && IceFX3.Ok; /* t1409u: v3 draws chill/freeze itself (IceFX3.StatusSprite) */ bool want = (!ice3 && (e.chillT > 0 || e.freezeT > 0)) || e.shockT > 0 || e.bleedT > 0 || tint;
            if (want && e.fx == null && (e.elite || _statusFxCount < MaxStatusFx)) AttachFx(e);
            if (e.fx == null) return;
            if (!want && !e.elite) { DetachFx(e); return; }
            e.fx.burnAmount = 0f; // v0.0.12: _BurnAmount is the pack's burn-AWAY dissolve (erases the sprite bottom-up; assumes uv 0..1, our sprites are sheet frames) -> it erased hit enemies. Burn = orange pulse instead.
            e.fx.frozenAmount = ice3 ? 0f : e.boss ? (e.freezeT > 0 || e.chillT > 0 ? 0.25f : 0f) : e.freezeT > 0 ? 1f : e.chillT > 0 ? 0.35f : 0f; // v0.0.14: full 'frozen' read as a translucent blue ghost on the boss
            // t948u: no 3 s full pulse on shock/bleed any more; a brief partial tint (speed 0 -> constant 0.5*amount in the Uber shader)
            e.fx.pulseAmount = tint ? Mathf.Clamp01(e.tint948Amt * 2f) : 0f; e.fx.pulseSpeed = 0f;
            e.fx.pulseColor = e.tint948Col;
            e.fx.flashAmount = e.hitT > 0 ? 1f : 0f;
        }
        void Tint948(Enemy e, float amt, float sec, Color c) { if (amt <= 0 || sec <= 0) return; e.tint948Amt = amt; e.tint948Col = c; e.tint948Until = runTime + sec; UpdateStatusFx(e); }
        void AttachFx(Enemy e)
        {
            var fx = e.sr.GetComponent<PixelFX>(); if (fx == null) fx = e.sr.gameObject.AddComponent<PixelFX>();
            fx.enabled = true; e.fx = fx; _statusFxCount++;
        }
        void DetachFx(Enemy e)
        {
            if (e.fx == null) return;
            e.fx.burnAmount = e.fx.frozenAmount = e.fx.pulseAmount = e.fx.flashAmount = 0;
            e.fx.enabled = false; e.sr.SetPropertyBlock(null); { var sp = e.sr.sprite; e.sr.sprite = null; e.sr.sprite = sp; } HordeFX.Prepare(e.sr); // re-seed _MainTex after clearing the block // back onto the shared batched material
            e.fx = null; _statusFxCount--;
        }

        // ---------- pickups: HellFX pooled pickup jobs with our art; own sprite pool when the 600 budget is hit ----------
        void OnPickupJobEnd(Pickup p, HellFXRunner.Job j)
        {
            if (p.job != j) return;               // stale (reset/reused)
            p.job = null; p.alive = false;
            if (p.magnet) { if (p.bone) { bones += p.value; SinBone818(p.value); } else { AddXp(p.value); HellordAudio.XpPickup(); } } // HellFX plays the collect splat/chips itself
        }

        // ---------- Bone Orbit visuals: HellFX.OrbitWeapons, re-cast only when count/speed change ----------
        List<HellFXRunner.Job> _orbitJobs; int _orbitN; float _orbitRev, _orbitR = 2f; Transform _heroCenterT;
        void RecastOrbit(int n, float revPerSec, float radiusU)
        {
            KillOrbit();
            _orbitN = n; _orbitRev = revPerSec; _orbitR = radiusU;
            _orbitJobs = HellFX.OrbitWeapons(_heroCenterT, HArt.Proj("proj_bone")[0], n, radiusU * HArt.PPU, revPerSec, 1e6f, new Color(0.9f, 0.85f, 0.7f), 55);
        }
        void KillOrbit()
        {
            if (_orbitJobs != null) foreach (var j in _orbitJobs) HellFXRunner.I.Kill(j);
            _orbitJobs = null; _orbitN = 0;
        }
    }

    /// <summary>v0.0.35: Frost Cleave swing drawn from the real tuning values: outer rim at radius r, faint inner rim, and (for a partial arc) the two
    /// edge rays at +-half-angle. half &gt;= 179 draws a full circle. Pooled 1px sprites, one component, no allocation per frame after warm-up.</summary>
    public class FrostArcFX : MonoBehaviour
    {
        struct Arc { public Vector2 c; public float a0, a1, r, half, t; public bool full; }
        const float Sweep = 0.16f, Hold = 0.30f, PxU = 16f;
        static FrostArcFX _i; readonly List<Arc> _arcs = new List<Arc>(); readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>(); int _used, _prevUsed;
        static readonly Color Rim = new Color(0.45f, 0.88f, 1f), Hi = new Color(0.9f, 0.98f, 1f);
        public static void Spawn(Vector2 c, float dirDeg, float r, float half, bool clockwise)
        {
            if (_i == null) { var go = new GameObject("FrostArcFX"); DontDestroyOnLoad(go); _i = go.AddComponent<FrostArcFX>(); }
            bool full = half >= 179f; float h = full ? 180f : half;
            var a = new Arc { c = c, r = r, half = h, full = full, t = 0 };
            a.a0 = clockwise ? dirDeg + h : dirDeg - h; a.a1 = clockwise ? dirDeg - h : dirDeg + h;
            if (_i._arcs.Count >= 4) _i._arcs.RemoveAt(0);
            _i._arcs.Add(a);
        }
        void Update()
        {
            float dt = Time.unscaledDeltaTime; if (dt > 0.1f) dt = 0.1f; _used = 0;
            for (int n = _arcs.Count - 1; n >= 0; n--)
            {
                var a = _arcs[n]; a.t += dt; if (a.t > Sweep + Hold) { _arcs.RemoveAt(n); continue; } _arcs[n] = a; Draw(a);
            }
            for (int i = _used; i < _prevUsed && i < _pool.Count; i++) _pool[i].enabled = false;
            _prevUsed = _used;
        }
        void Draw(Arc a)
        {
            float k = Mathf.Clamp01(a.t / Sweep); k = 1 - (1 - k) * (1 - k);
            float fade = a.t <= Sweep ? 1f : 1f - (a.t - Sweep) / Hold; fade = Mathf.Clamp01(fade);
            float head = Mathf.Lerp(a.a0, a.a1, k);
            if (!a.full)
            {   // edge rays: the exact +-half-angle boundary of the hit test
                DrawRay(a, a.a0, fade * 0.45f); DrawRay(a, a.a1, fade * 0.45f);
            }
            DrawArc(a, a.r, head, 2, Rim, fade, true);
            DrawArc(a, a.r * 0.6f, head, 1, Rim, fade * 0.35f, false);
        }
        void DrawRay(Arc a, float deg, float alpha)
        {
            Vector2 d = new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad)); float len = a.r * PxU;
            for (float p = 6; p < len; p += 10) Box(a.c + d * (p / PxU), Mathf.Min(5, len - p + 1), 1, deg, Rim, alpha);
        }
        void DrawArc(Arc a, float rad, float head, float thick, Color col, float alpha, bool tip)
        {
            float span = Mathf.Abs(head - a.a0); if (span < 0.5f) return;
            float rpx = rad * PxU; int n = Mathf.Max(2, Mathf.CeilToInt(span * Mathf.Deg2Rad * rpx / 6f)); float sgn = Mathf.Sign(head - a.a0);
            for (int i = 0; i < n; i++)
            {
                float u0 = i / (float)n, u1 = (i + 1) / (float)n, g0 = a.a0 + sgn * span * u0, g1 = a.a0 + sgn * span * u1, gm = (g0 + g1) * 0.5f;
                float chord = 2f * rpx * Mathf.Sin(Mathf.Abs(g1 - g0) * 0.5f * Mathf.Deg2Rad) + 1f; Vector2 pos = a.c + new Vector2(Mathf.Cos(gm * Mathf.Deg2Rad), Mathf.Sin(gm * Mathf.Deg2Rad)) * rad;
                bool lead = tip && i >= n - 2 && a.t < Sweep;
                Box(pos, chord, lead ? thick + 1 : thick, gm + 90f, lead ? Color.white : ((i & 1) == 0 ? col : Hi), alpha);
            }
        }
        void Box(Vector2 c, float lenPx, float thickPx, float deg, Color col, float alpha)
        {
            if (_used == _pool.Count) { var go = new GameObject("fa"); go.transform.SetParent(transform, false); var s = go.AddComponent<SpriteRenderer>(); s.sprite = HArt.Pixel(); _pool.Add(s); }
            var sr = _pool[_used++]; sr.enabled = true; col.a = alpha; sr.color = col; sr.sortingOrder = 32010;
            sr.transform.position = c; sr.transform.rotation = Quaternion.Euler(0, 0, deg); sr.transform.localScale = new Vector3(Mathf.Max(1, Mathf.Round(lenPx)), Mathf.Max(1, Mathf.Round(thickPx)), 1);
        }
    }
}
