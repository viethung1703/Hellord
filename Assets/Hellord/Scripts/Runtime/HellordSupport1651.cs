using UnityEngine;

namespace Hellord
{
    /// <summary>t1651 support system: probe counters read by the support autotest (sup1651). Cheap ints / floats, always compiled.</summary>
    public partial class HellordGame
    {
        [System.NonSerialized] public int AtCasts1651, AtProj1651, AtHits1651, AtSkillKills1651;          // real casts (Cast + kit casts), projectiles spawned (every custom projectile too)
        [System.NonSerialized] public float AtCd1651, AtKill1651;            // cooldown set by the last cast; on-kill support output (harvest XP + blast damage)
        [System.NonSerialized] public int AtDotKills1651;

        /// <summary>t1651 stage 6: burn / bleed damage carries the skill that applied it (Enemy.burnSrc1651 / bleedSrc1651), so the
        /// skill's Executioner bonus and Life Leech work on each DoT tick, and a DoT kill is that skill's kill (Soul Harvest, Explode on Kill,
        /// pact on-kill). Knockback stays hit-only (a tick never pushes). No source (pact / sin burns) = plain damage as before.</summary>
        void DotDamage1651(Enemy e, float d, SkillInst src, bool noNumber)
        {
            if (src == null || !e.alive) { Damage(e, d, false, true, noNumber); return; }
            Vector2 kz = Vector2.zero; d = SupportsBeforeHit(e, d, src, ref kz, true);
            Damage(e, d, false, true, noNumber);
            if (!e.alive) { AtDotKills1651++; OnSkillKill(e, src); }
        }
    }
}
