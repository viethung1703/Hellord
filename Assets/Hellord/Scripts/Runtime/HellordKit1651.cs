using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1651 stage 4: the 12 Zhurong kit skills are real skills.
    //  - Each has its own SkillId / SkillDef (tags from the t1651 audit table) and one SkillInst per run (_zkI1651, kept for the run).
    //  - Picking the owned kit skill again (a level card) levels it: Lv2-4 open support sockets 2-4 (same rule as basics:
    //    OpenSlots = min(level, 4) - socketed), damage scales by HData.LevelMul like basics.
    //  - Supports go on them through the normal support cards: SupPool1651() = owned basics + the picked kit skills;
    //    a target index >= KitTarget1651 means kit slot (index - KitTarget1651): 0 move, 1 special, 2 ultimate.
    //  - Kit code reads its numbers through ZkCd1651 / ZkR1651 / ZkDur1651 / ZkHit1651 and passes ZkI1651(k) as the hit source,
    //    so Faster Casting, Bigger Area, Duration, Added Damage and every on-hit support work by tags, like on basics.
    //  - Kit skills read only the SUPPORT part of CdMul / AreaMul (def.kit1651): global intensity / sin multipliers never
    //    applied to the kit before and still do not.
    // =====================================================================================================================
    public partial class HellordGame
    {
        public static readonly SkillId[] ZkIds1651 = { SkillId.ZkEmberEffigy, SkillId.ZkEmberAnchor, SkillId.ZkBackdraft, SkillId.ZkPyreMark, SkillId.ZkFlammability, SkillId.ZkWarCry, SkillId.ZkSmolderRing, SkillId.ZkHearthBlessing, SkillId.ZkSunsWrath, SkillId.ZkRebirthFlame, SkillId.ZkMoltenHeart, SkillId.ZkVermilionBird };
        public const int KitTarget1651 = 100;
        readonly SkillInst[] _zkI1651 = new SkillInst[64]; /* t1772: + 10 Zhong Kui (NkK0) */ /* t1682: + 13 Gonggong (GgK0) */ /* t1666: 12 Zhurong + 12 Leizu (LzK0) + spare */
        /// <summary>kit skill k (0-2 movement, 3-7 specials, 8-11 ultimates): its run instance (made on first use, Lv1).</summary>
        public SkillInst ZkI1651(int k) { k = Mathf.Clamp(k, 0, NkK0 + 9); return _zkI1651[k] ?? (_zkI1651[k] = new SkillInst { def = HData.Skill(k < LzK0 ? ZkIds1651[k] : k < GgK0 ? LzIds1666[k - LzK0] : k < NkK0 ? GgIds1682[k - GgK0] : NkIds1713[k - NkK0]), level = 1 }); /* t1713: k >= NkK0 = Zhong Kui kit */ /* t1682: k >= GgK0 = Gonggong kit */ } // t1666: k >= LzK0 = Leizu kit
        public static int ZkK1651(int slot, int pick) => slot == 0 ? pick - 1 : slot == 1 ? 2 + pick : 7 + pick;
        int ZkPick1651(int slot) => slot == 0 ? KitMove1666 : slot == 1 ? KitSpec1666 : KitUlt1666; // t1666: the running pact's kit (Zhurong or Leizu)
        /// <summary>The kit skill in slot 0/1/2 (move / special / ultimate), null when the slot is empty or the kit is off.</summary>
        public SkillInst ZkSlotInst1651(int slot) { int p = ZkPick1651(slot); return p > 0 ? ZkI1651(KitK1666(slot, p)) : null; }
        void ResetKit1651() { for (int i = 0; i < _zkI1651.Length; i++) _zkI1651[i] = null; }

        // ---- the numbers kit code reads (support stats of that kit skill)
        float ZkCd1651(int k, float baseS) => baseS * ZkI1651(k).CdMul;
        float ZkR1651(int k, float r) => ZkI1651(k).R(r);
        float ZkDur1651(int k, float s) => ZkI1651(k).Dur(s);
        float ZkHit1651(int k) { var s = ZkI1651(k); return ZkHit1618 * s.More() * HData.LevelMul(s.level) / Mathf.Max(1e-4f, HData.LevelMul(1)); }

        // ---- support targets: basics + picked kit skills
        readonly List<SkillInst> _supPool1651 = new List<SkillInst>(8);
        public List<SkillInst> SupPool1651() { _supPool1651.Clear(); _supPool1651.AddRange(skills); for (int sl = 0; sl < 3; sl++) { var k = ZkSlotInst1651(sl); if (k != null) _supPool1651.Add(k); } return _supPool1651; }
        public SkillInst SupInst1651(int ix) => ix >= KitTarget1651 ? ZkSlotInst1651(ix - KitTarget1651) : ix >= 0 && ix < skills.Count ? skills[ix] : null;
        void AddKitTargets1651(SupportDef sd, List<int> l, bool upgradeOnly) { for (int sl = 0; sl < 3; sl++) { var k = ZkSlotInst1651(sl); if (k != null && (upgradeOnly ? k.CanUpgrade(sd) : k.CanSocket(sd) == null)) l.Add(KitTarget1651 + sl); } }

        // ---- level cards: the owned kit skills below max level
        List<Card> KitLevelCards1651()
        {
            var l = new List<Card>(3); if (!KitOn1666 || ZhSlotOverride1639) return l; // t1666: + Leizu
            for (int sl = 0; sl < 3; sl++) { var k = ZkSlotInst1651(sl); if (k != null && k.level < HData.MaxSkillLevel) l.Add(new Card { kind = CardKind.NewSkill, kit = sl + 1, kitPick = ZkPick1651(sl), kitLv1651 = true }); }
            return l;
        }
        void KitLevelUp1651(Card c)
        {
            var k = ZkSlotInst1651(c.kit - 1); if (k == null) return; k.level = Mathf.Min(HData.MaxSkillLevel, k.level + 1);
            Toast($"{k.def.name} Lv{k.level}: support slot {Mathf.Min(k.level, HData.MaxSupports)} open");
        }
    }
}
