using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.21: burn status on Pixel FX Pack 1.8.7 BurnFX #110. Exact visible-body sizes per enemy sheet (measured from the PNGs:
// median opaque box of the 16 walk frames; feet = px above the pivot). Damage is applied in BurnFX.OnTick (the pack draws the tick
// number, so Damage(noNumber)). No enemy tint, no orange pulse, TickTint off.
namespace Hellord
{
    public partial class HellordGame
    {
        static readonly Dictionary<EType, Vector3Int> BurnBody = new Dictionary<EType, Vector3Int>
        {
            { EType.Imp, new Vector3Int(19, 37, 1) }, { EType.Ghoul, new Vector3Int(16, 37, 0) }, { EType.Elite, new Vector3Int(17, 40, 0) },
            { EType.Brute, new Vector3Int(22, 47, 1) }, { EType.Splitter, new Vector3Int(26, 28, 1) }, { EType.SplitChild, new Vector3Int(17, 18, 1) },
        };
        static readonly Dictionary<Transform, Enemy> _burnMap = new Dictionary<Transform, Enemy>();
        bool _burnInit;
        void InitBurn()
        {
            if (_burnInit) return; _burnInit = true;
            var tmp = NewSR("matprobe", null, null, 0); BurnFX.MaterialOverride = new Material(tmp.sharedMaterial); Destroy(tmp.gameObject);
            BurnFX.BackSortingOrder = -20000; BurnFX.FrontSortingOrder = 3000; BurnFX.TickTint = false; BurnFX.ShowNumbers = true; BurnFX.Prewarm(256);
            BurnFX.OnTick += OnBurnTick;
        }
        void IgniteBurn(Enemy e)
        {
            InitBurn(); var tu = HellordTuning.I; int tick = Mathf.Max(1, Mathf.RoundToInt(e.burnDps * tu.burnTickInterval));
            float w = 0, h = 0, f = float.NaN;
            if (!e.boss && !e.nest && BurnBody.TryGetValue(e.type, out var b)) { w = b.x; h = b.y; f = b.z; }
            e.burnH = BurnFX.Ignite(e.tr, e.burnT, tick, tu.burnTickInterval, w, h, f); // re-ignite keeps the higher dmg / longer time
            _burnMap[e.tr] = e;
        }
        static void StopBurn(Enemy e, bool smoke) { if (e.burnH >= 0) BurnFX.Extinguish(e.burnH, smoke); e.burnH = -1; _burnMap.Remove(e.tr); }
        void OnBurnTick(int h, Transform tr, int dmg)
        {
            if (tr == null || !_burnMap.TryGetValue(tr, out var e) || !e.alive || e.burnH != h) return;
            Damage(e, dmg, false, true, true);
        }
        void OnDestroy() { BurnFX.OnTick -= OnBurnTick; DisposeSin84(); }
    }
}
