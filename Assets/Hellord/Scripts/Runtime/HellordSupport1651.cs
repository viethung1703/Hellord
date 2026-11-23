using UnityEngine;

namespace Hellord
{
    /// <summary>t1651 support system: probe counters read by the support autotest (sup1651). Cheap ints / floats, always compiled.</summary>
    public partial class HellordGame
    {
        [System.NonSerialized] public int AtCasts1651, AtProj1651, AtHits1651, AtSkillKills1651;          // real casts (Cast + kit casts), projectiles spawned (every custom projectile too)
        [System.NonSerialized] public float AtCd1651, AtKill1651;            // cooldown set by the last cast; on-kill support output (harvest XP + blast damage)
    }
}
