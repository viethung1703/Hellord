using UnityEngine;

// v0.0.48: boss music tracks (Hellord Sound procedural drafts). 0 Ox-Head, 1 Horse-Face P1, 2 Horse-Face P2, 3 Ice Gorilla, 4 Qilin Chariot.
namespace Hellord
{
    public partial class HellordGame
    {
        /// <summary>Boss music track for this frame, -1 = floor music. Frost Ox-Head (old) uses the Ox-Head track.</summary>
        int BossTrack48()
        {
            if (!_bossSpawned || !BossAlive) return -1;
            int k = BossKind44;
            if (k == 0 || k == 1) return 0;
            if (k == 2) return _phase2 ? 2 : 1;
            if (k == 3) return 3;
            if (k == 4) return 4;
            return -1;
        }
    }
}
