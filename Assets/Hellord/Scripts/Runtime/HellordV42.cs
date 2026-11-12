using UnityEngine;

namespace Hellord
{
    /// <summary>
    /// v0.0.42: shrinking drops. In rooms where demons keep spawning with no kill or time limit (Nests by default; list in
    /// HellordTuning.dropDecayRooms) XP and bone drops shrink linearly after a grace time, down to a floor. Normal rooms are untouched.
    /// Feedback is only the smaller blood-drop tier (HArt.BloodTier follows the value); no UI.
    /// </summary>
    public partial class HellordGame
    {
        /// <summary>1 = full drops. Below 1 only inside a listed, active (not yet cleared) room after the grace time.</summary>
        float DropDecayMul()
        {
            var T = HellordTuning.I; if (!T.dropDecayEnabled || !floorMode) return 1f;
            var r = Cur; if (r == null || !r.active || r.cleared || T.dropDecayRooms == null) return 1f;
            bool listed = false; foreach (var t in T.dropDecayRooms) if (t == r.type) { listed = true; break; }
            if (!listed) return 1f;
            float over = _roomT - T.dropDecayGraceSeconds; if (over <= 0) return 1f;
            return Mathf.Clamp(1f - T.dropDecayPerMinute * (over / 60f), Mathf.Clamp01(T.dropDecayMinMul), 1f);
        }
        /// <summary>Rounds randomly so the average stays exact (1 x 0.3 drops 30% of the time instead of never).</summary>
        static int StochRound(float v) { int f = Mathf.FloorToInt(v); return f + (Random.value < v - f ? 1 : 0); }
    }
}
