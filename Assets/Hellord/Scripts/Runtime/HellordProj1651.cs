using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1651 stage 5: ONE shared projectile helper for the custom projectile entities (sparks, javelins, boulders, hydra bolts,
    // twin axes). Count (Multi), Pierce, Fork, Bounce and Homing are read here from the skill's support stats, so every skill
    // that embeds a ProjCore1651 and calls these gets all five, and future projectile skills get them by doing the same.
    //  - ProjCount1651 / ProjFan1651: base count + ProjExtra, fanned around the aim (multiFanDeg1651 apart).
    //  - ProjCore1651: per projectile state (pierce left, bounce left, fork used, child, hit set).
    //  - ProjOnHit1651: after a projectile hits an enemy. Same order as the old Fireball path: Pierce > Fork > Bounce > die.
    //    Fork children (forkSpreadDeg apart, ForkDmg x damage) are spawned through the skill's callback and never fork again.
    //  - ProjHome1651: steers a heading toward the nearest live enemy within HomingR not hit yet (HomingTurn, same easing as Fireball).
    //  - A skill whose projectile can't use a mechanic declares it in SkillDef.projCaps1651 (e.g. NoPierce on a boulder that already
    //    passes through everything); HData.Fits then never offers that support there.
    //  - Echo ignores projectile caps (_echo738 is checked by the capped casts).
    // =====================================================================================================================
    public class ProjCore1651 { public int pierce, bounce; public bool fork, child; public float forkDmg = 0.55f, bounceR = 4f, homR, homTurn = 6f; public readonly HashSet<int> hit = new HashSet<int>(); }
    public enum ProjAfter1651 { Die, Continue, Redirect, Forked }

    public partial class HellordGame
    {
        public static int AtForks1651, AtBounces1651, AtPierces1651; // probes
        /// <summary>The projectile support state for one new projectile of skill s (child = a fork child: no pierce, no fork).</summary>
        ProjCore1651 ProjCoreFor1651(SkillInst s, bool child = false, ProjCore1651 parent = null)
        {
            var T = HellordTuning.I; var c = new ProjCore1651 { child = child };
            if (s == null) return c;
            float fd = s.Stat(SStat1651.ForkDmg), br = s.Stat(SStat1651.BounceR), ht = s.Stat(SStat1651.HomingTurn);
            c.pierce = child ? 0 : s.PierceN; c.fork = !child && s.ForkN > 0; c.forkDmg = fd > 0 ? fd : 0.55f;
            c.bounce = child && parent != null ? parent.bounce : s.BounceN; c.bounceR = br > 0 ? br : 4f; c.homR = s.HomingR; c.homTurn = ht > 0 ? ht : 6f;
            if (parent != null) foreach (var h in parent.hit) c.hit.Add(h);
            return c;
        }
        /// <summary>How many projectiles a cast of s fires: base + Multi's extra.</summary>
        int ProjCount1651(SkillInst s, int baseN) => Mathf.Max(1, baseN + (s != null ? s.ProjExtra : 0));
        /// <summary>Heading of projectile i of n, fanned around dir (multiFanDeg1651 apart, centred).</summary>
        Vector2 ProjFan1651(Vector2 dir, int i, int n, float stepDeg = -1f) { if (stepDeg < 0f) stepDeg = HellordTuning.I.multiFanDeg1651; return n <= 1 ? dir : (Vector2)(Quaternion.Euler(0, 0, (i - (n - 1) * 0.5f) * stepDeg) * dir); }
        /// <summary>After projectile c (skill s) hit enemy hitIx at pos with heading dir. Pierce > Fork > Bounce > die.
        /// spawnChild(dir, dmgMul) makes one fork child (the caller builds the entity with ProjCoreFor1651(s, true, c)). Redirect = new heading in dir.</summary>
        ProjAfter1651 ProjOnHit1651(ProjCore1651 c, SkillInst s, int hitIx, Vector2 pos, ref Vector2 dir, ref float dmg, System.Action<Vector2, float> spawnChild)
        {
            var T = HellordTuning.I; c.hit.Add(hitIx);
            if (c.pierce > 0) { c.pierce--; dmg *= 1f - HData.PierceFalloffOf1651(s); AtPierces1651++; return ProjAfter1651.Continue; }
            if (c.fork && spawnChild != null)
            {
                c.fork = false; int fn = Mathf.Max(2, s != null ? s.ForkN : 2); AtForks1651++;
                // t1654: Multi's damage drawback already applies to forked children because spawnChild multiplies the parent projectile's damage, which went through SkillInst.More() (DmgMul includes multiProjDmgMul).
                for (int k = 0; k < fn; k++) spawnChild((Vector2)(Quaternion.Euler(0, 0, (k - (fn - 1) * 0.5f) * T.forkSpreadDeg) * dir), c.forkDmg * SinForkDmg818());
                return ProjAfter1651.Forked;
            }
            if (c.bounce > 0) { int n = ProjNearest1651(pos, c.bounceR, c); if (n >= 0) { c.bounce--; dir = (_en[n].pos - pos).normalized; AtBounces1651++; return ProjAfter1651.Redirect; } }
            return ProjAfter1651.Die;
        }
        /// <summary>When a projectile ends without a hit (range / wall): Bounce can still send it to a demon nearby.</summary>
        bool ProjOnEnd1651(ProjCore1651 c, Vector2 pos, ref Vector2 dir)
        {
            if (c == null || c.bounce <= 0) return false; int n = ProjNearest1651(pos, c.bounceR, c); if (n < 0) return false;
            c.bounce--; dir = (_en[n].pos - pos).normalized; AtBounces1651++; return true;
        }
        int ProjNearest1651(Vector2 p, float maxR, ProjCore1651 c)
        {
            int best = -1; float bd = maxR * maxR;
            for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (e == null || !e.alive || e.dying || c.hit.Contains(i)) continue; float d = (e.pos - p).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
            return best;
        }
        /// <summary>Homing: turn heading dir toward the nearest live demon within HomingR that this projectile has not hit (same easing as Fireball's homing).</summary>
        Vector2 ProjHome1651(ProjCore1651 c, Vector2 pos, Vector2 dir, float dt)
        {
            if (c == null || c.homR <= 0f) return dir; int n = ProjNearest1651(pos, c.homR, c); if (n < 0) return dir;
            Vector2 want = (_en[n].pos - pos); if (want.sqrMagnitude < 1e-6f) return dir; want.Normalize();
            Vector2 nd = Vector2.Lerp(dir, want, 1f - Mathf.Exp(-c.homTurn * dt)); return nd.sqrMagnitude > 1e-8f ? nd.normalized : dir;
        }
    }
}
