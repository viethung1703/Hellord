using UnityEngine;

// t989u interim Frost Cleave port — superseded by t990u HellordFrostCleave990 (fx_audit/frost_cleave_wire).
// Stubs kept so earlier V44 hooks still compile; they no-op when frostCleaveShader990 is on (default).
namespace Hellord
{
    public partial class HellordGame
    {
        public bool FrostCleave989Public(Vector2 c, Vector2 dir, float r, float half) => false;
        void FrostCleave989Tick(float dt) { }
    }
}
