using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1639 (VH approved plan): level-up with the 4-slot kit.
    //  - A run starts with the basic only; movement / special / ultimate slots fill through level-up picks (ZhRun*1639, reset per run).
    //  - NewSkill cards come only from the running pact's kit pools (Zhurong: the kitMove1618 / kitSpecial1618 / kitUlt1618 options);
    //    once a type is picked, no more cards of that type. No basics, no other pacts' skills, no old auto-cast skills.
    //    A pact without kit pools offers no new-skill cards. Skill-level / support / stat cards are unchanged.
    //  - kitSlotOverride1639 (testing only, default off): the slots come from the tuning picks / F1 menu as in t1618 and no kit cards are offered.
    //  Cards reuse the card UI: CardKind.NewSkill with kit = 1/2/3 (move/special/ult), kitPick = option index, skill = null.
    // =====================================================================================================================
    public partial class HellordGame
    {
        [System.NonSerialized] public int ZhRunMove1639, ZhRunSpec1639, ZhRunUlt1639; // 0 = slot empty
        bool ZhSlotOverride1639 => TZ != null && TZ.kitSlotOverride1639;
        void ZhRunReset1639() { ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0; ResetKit1651(); } // t1651: kit skill levels / supports are per run

        static readonly string[] ZhKitType1639 = { "", "MOVEMENT - SPACE", "SPECIAL - RMB", "ULTIMATE - R" };
        static readonly string[] ZhMoveDesc1639 = { "", "Dash leaves a burning decoy that draws enemies.", "Dash leaves an ember. Dash again soon to snap back.", "Dash start point pulls nearby enemies in." };
        static readonly string[] ZhSpecDesc1639 = { "", "Mark the enemy near the cursor. Burn it for bonus fire damage.", "Curse an area: enemies burn more easily.", "Roar: next basics are powered, roared foes explode on death.", "Ring at the cursor slows and scorches enemies.", "Heal over time, cleanse and resist debuffs." };
        static readonly string[] ZhUltDesc1639 = { "", "Deal burn damage to fill the meter, then call down a sun.", "Automatic: a killing hit revives you in a firestorm.", "Arms with enough burning foes: basics cast twice.", "Arms after near-miss dashes: a firebird sweeps your aim line." };

        string ZhKitName1639(Card c) => NkOn1713 ? (c.kit == 1 ? NkMoveNames1713[c.kitPick] : c.kit == 2 ? NkSpecNames1713[c.kitPick] : c.kit == 3 ? NkUltNames1713[c.kitPick] : "") : GgOn1682 ? (c.kit == 1 ? GgMoveNames1682[c.kitPick] : c.kit == 2 ? GgSpecNames1682[c.kitPick] : c.kit == 3 ? GgUltNames1682[c.kitPick] : "") : LzOn1666 ? (c.kit == 1 ? LzMoveNames1666[c.kitPick] : c.kit == 2 ? LzSpecNames1666[c.kitPick] : c.kit == 3 ? LzUltNames1666[c.kitPick] : "") : c.kit == 1 ? ZhMoveNames1618[c.kitPick] : c.kit == 2 ? ZhSpecNames1618[c.kitPick] : c.kit == 3 ? ZhUltNames1618[c.kitPick] : "";
        string ZhKitDesc1639(Card c) => NkOn1713 ? (c.kit == 1 ? NkMoveDesc1713[c.kitPick] : c.kit == 2 ? NkSpecDesc1713[c.kitPick] : c.kit == 3 ? NkUltDesc1713[c.kitPick] : "") : GgOn1682 ? (c.kit == 1 ? GgMoveDesc1682[c.kitPick] : c.kit == 2 ? GgSpecDesc1682[c.kitPick] : c.kit == 3 ? GgUltDesc1682[c.kitPick] : "") : LzOn1666 ? (c.kit == 1 ? LzMoveDesc1666[c.kitPick] : c.kit == 2 ? LzSpecDesc1666[c.kitPick] : c.kit == 3 ? LzUltDesc1666[c.kitPick] : "") : c.kit == 1 ? ZhMoveDesc1639[c.kitPick] : c.kit == 2 ? ZhSpecDesc1639[c.kitPick] : c.kit == 3 ? ZhUltDesc1639[c.kitPick] : "";
        /// <summary>icons_v2 key (ZhKeys1618 order: 3 movement, 5 specials, 4 ultimates).</summary>
        string ZhKitKey1639(Card c) => NkOn1713 ? (c.kit >= 1 && c.kit <= 3 ? NkKeys1713[NkK1713(c.kit - 1, c.kitPick) - NkK0] : null) : GgOn1682 ? (c.kit >= 1 && c.kit <= 3 ? GgKeys1682[GgK1682(c.kit - 1, c.kitPick) - GgK0] : null) : LzOn1666 ? (c.kit >= 1 && c.kit <= 3 ? LzKeys1666[(c.kit - 1) * 4 + c.kitPick - 1] : null) : c.kit == 1 ? ZhKeys1618[c.kitPick - 1] : c.kit == 2 ? ZhKeys1618[3 + c.kitPick - 1] : c.kit == 3 ? ZhKeys1618[8 + c.kitPick - 1] : null;

        /// <summary>The running pact's new-skill cards: every option of each kit type not picked yet. Empty for pacts without kit pools / override on.</summary>
        List<Card> KitOffers1639()
        {
            var l = new List<Card>(12); if (!KitOn1666 || ZhSlotOverride1639) return l;
            if (NkOn1713) { for (int t = 1; t <= 3; t++) { if ((t == 1 ? ZhRunMove1639 : t == 2 ? ZhRunSpec1639 : ZhRunUlt1639) != 0) continue; int n = t == 1 ? 3 : t == 2 ? 4 : 3; for (int i = 1; i <= n; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = t, kitPick = i }); } return l; } // t1772 Zhong Kui: 3 + 4 + 3
            if (GgOn1682) { for (int t = 1; t <= 3; t++) { if (GgPickOn1682(t - 1, t == 1 ? ZhRunMove1639 : t == 2 ? ZhRunSpec1639 : ZhRunUlt1639)) continue; /* t1687: a disabled pick (old run) counts as an empty slot */ for (int i = 1; i <= (t == 2 ? 5 : 4); i++) if (GgPickOn1682(t - 1, i)) l.Add(new Card { kind = CardKind.NewSkill, kit = t, kitPick = i }); } return l; } // t1682 Gonggong; t1687: 4 + 4 + 3 (Toppled Pillar / Pillars of Ao off)
            if (LzOn1666) { for (int t = 1; t <= 3; t++) { if ((t == 1 ? ZhRunMove1639 : t == 2 ? ZhRunSpec1639 : ZhRunUlt1639) != 0) continue; for (int i = 1; i <= 4; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = t, kitPick = i }); } return l; } // t1666 Leizu: 4 + 4 + 4
            if (ZhRunMove1639 == 0) for (int i = 1; i <= 3; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = 1, kitPick = i });
            if (ZhRunSpec1639 == 0) for (int i = 1; i <= 5; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = i });
            if (ZhRunUlt1639 == 0) for (int i = 1; i <= 4; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = i });
            return l;
        }
        void ChooseKit1639(Card c)
        {
            if (c.kitLv1651) { KitLevelUp1651(c); return; } // t1651: owned kit skill picked again = level up
            if (c.kit == 1) { ZhRunMove1639 = c.kitPick; ZhMoveCd = 0f; } else if (c.kit == 2) { ZhRunSpec1639 = c.kitPick; ZhSpecCd = 0f; NkOnSpecPicked1713(); } else { ZhRunUlt1639 = c.kitPick; ZhUltCd = 0f; }
            Toast(ZhKitName1639(c) + " - " + ZhKitType1639[c.kit]);
        }

        /// <summary>Card body for a kit card in DrawCardsB: icons_v2 icon (48 px like the other new-ids), NEW badge, type label + description.</summary>
        void ZhKitCardBody1639(Card c, CardCopy copy)
        {
            if (c.kitLv1651)
            {
                var ki = ZkSlotInst1651(c.kit - 1);
                copy.AddSection(CardTone.Header, ki != null ? $"LV {ki.level} - {ki.level + 1}" : "LEVEL UP");
                copy.AddSection(CardTone.Body, $"SUPPORT SLOT {Mathf.Min((ki != null ? ki.level : 0) + 1, HData.MaxSupports)}");
                if (ki != null && ki.sup.Count > 0) foreach (var su in ki.sup) copy.AddSection(CardTone.Need, HData.Support(su).name + " " + ki.Lvl(su));
                return;
            }
            copy.AddSection(CardTone.Header, ZhKitType1639[c.kit]);
            copy.AddSection(CardTone.Body, WrapCard(ZhKitDesc1639(c)));
        }
    }
}
