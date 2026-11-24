#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1677 regression (scenario supkit1677): Leizu run, the basic's only support slot filled, then a level-up card offers a support whose
    /// ONLY target is a kit slot (index >= KitTarget1651). The card screen is drawn for a while (DrawCardsB used to index skills[t] -> ArgumentOutOfRange)
    /// and the card is picked through the same code as the 1-5 keys. PASS = no exception / error logged, support lands on the kit skill.</summary>
    public partial class HellordGame
    {
        IEnumerator AtSupKit1677()
        {
            string scen = "supkit1677"; var T = HellordTuning.I;
            yield return ZhStart1618(scen, null, SkillId.ThunderDrumStrikes, 2);
            int m0 = T.lzMove1666, s0 = T.lzSpecial1666, u0 = T.lzUlt1666; bool o0 = T.kitSlotOverride1639;
            T.kitSlotOverride1639 = true; T.lzMove1666 = 1; T.lzSpecial1666 = 1; T.lzUlt1666 = 1; yield return ZF1618(3);
            int errs = 0; var errTxt = new List<string>();
            Application.LogCallback cb = (msg, st, ty) => { if (ty == LogType.Exception || ty == LogType.Error) { errs++; if (errTxt.Count < 3) errTxt.Add(msg); } };
            Application.logMessageReceived += cb;
            var b = skills.Count > 0 ? skills[0] : null; SupportDef fill = null, pick = null; int tgt = -1;
            if (b != null) { b.sup.Clear(); b.level = 1; foreach (var sd in HData.Supports) if (HData.Offered1654(sd) && b.CanSocket(sd) == null) { fill = sd; break; } if (fill != null) b.AddSupport(fill.id); }
            foreach (var sd in HData.Supports) { if (!HData.Offered1654(sd)) continue; var tl = SupportTargets(sd); if (tl.Count == 1 && tl[0] >= KitTarget1651) { pick = sd; tgt = tl[0]; break; } }
            ARes(scen + ":setup", pick != null, $"basic {b?.def.id} filled with {fill?.name}; kit-only support {pick?.name} -> target {tgt} (kit slot {tgt - KitTarget1651}, {SupInst1651(tgt)?.def.name})");
            if (pick != null)
            {
                var kit = SupInst1651(tgt); int lv0 = kit.Lvl(pick.id);
                var c = new Card { kind = CardKind.Support, support = pick }; cards.Clear(); cards.Add(c); mode = Mode.LevelUp;
                yield return ZF1618(45); // card screen drawn (OnGUI -> DrawCardsB): 'TO: <kit skill>' line + level dots
                int drawErrs = errs; ARes(scen + ":draw", drawErrs == 0, $"card screen drawn 45 frames, errors {drawErrs} {string.Join(" | ", errTxt)}");
                var tl2 = SupportTargets(c.support); if (tl2.Count == 1) Choose(c, tl2[0]); else if (tl2.Count > 1) OpenTargetPick(c.support, tl2, false, tIx => Choose(c, tIx)); // = the 1-5 key path
                yield return ZF1618(10);
                ARes(scen + ":pick", errs == 0 && kit.Lvl(pick.id) == lv0 + 1 && mode == Mode.Play, $"{pick.name} on {kit.def.name} Lv {lv0} -> {kit.Lvl(pick.id)}, mode {mode}, errors {errs}");
            }
            Application.logMessageReceived -= cb;
            T.lzMove1666 = m0; T.lzSpecial1666 = s0; T.lzUlt1666 = u0; T.kitSlotOverride1639 = o0; cards.Clear(); if (mode != Mode.Play) mode = Mode.Play;
            ZhEnd1618(scen, null);
        }
    }
}
#endif
