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
        void ZhRunReset1639() { ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0; }

        static readonly string[] ZhKitType1639 = { "", "MOVEMENT - SPACE", "SPECIAL - RMB", "ULTIMATE - R" };
        static readonly string[] ZhMoveDesc1639 = { "", "Dash leaves a burning decoy that draws enemies.", "Dash leaves an ember. Dash again soon to snap back.", "Dash start point pulls nearby enemies in." };
        static readonly string[] ZhSpecDesc1639 = { "", "Mark the enemy near the cursor. Burn it for bonus fire damage.", "Curse an area: enemies burn more easily.", "Roar: next basics are powered, roared foes explode on death.", "Ring at the cursor slows and scorches enemies.", "Heal over time, cleanse and resist debuffs." };
        static readonly string[] ZhUltDesc1639 = { "", "Deal burn damage to fill the meter, then call down a sun.", "Automatic: a killing hit revives you in a firestorm.", "Arms with enough burning foes: basics cast twice.", "Arms after near-miss dashes: a firebird sweeps your aim line." };

        string ZhKitName1639(Card c) => c.kit == 1 ? ZhMoveNames1618[c.kitPick] : c.kit == 2 ? ZhSpecNames1618[c.kitPick] : c.kit == 3 ? ZhUltNames1618[c.kitPick] : "";
        string ZhKitDesc1639(Card c) => c.kit == 1 ? ZhMoveDesc1639[c.kitPick] : c.kit == 2 ? ZhSpecDesc1639[c.kitPick] : c.kit == 3 ? ZhUltDesc1639[c.kitPick] : "";
        /// <summary>icons_v2 key (ZhKeys1618 order: 3 movement, 5 specials, 4 ultimates).</summary>
        static string ZhKitKey1639(Card c) => c.kit == 1 ? ZhKeys1618[c.kitPick - 1] : c.kit == 2 ? ZhKeys1618[3 + c.kitPick - 1] : c.kit == 3 ? ZhKeys1618[8 + c.kitPick - 1] : null;

        /// <summary>The running pact's new-skill cards: every option of each kit type not picked yet. Empty for pacts without kit pools / override on.</summary>
        List<Card> KitOffers1639()
        {
            var l = new List<Card>(12); if (!ZhOn1618 || ZhSlotOverride1639) return l;
            if (ZhRunMove1639 == 0) for (int i = 1; i <= 3; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = 1, kitPick = i });
            if (ZhRunSpec1639 == 0) for (int i = 1; i <= 5; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = i });
            if (ZhRunUlt1639 == 0) for (int i = 1; i <= 4; i++) l.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = i });
            return l;
        }
        void ChooseKit1639(Card c)
        {
            if (c.kit == 1) { ZhRunMove1639 = c.kitPick; ZhMoveCd = 0f; } else if (c.kit == 2) { ZhRunSpec1639 = c.kitPick; ZhSpecCd = 0f; } else { ZhRunUlt1639 = c.kitPick; ZhUltCd = 0f; }
            Toast(ZhKitName1639(c) + " - " + ZhKitType1639[c.kit]);
        }

        /// <summary>Card body for a kit card in DrawCardsB: icons_v2 icon (48 px like the other new-ids), NEW badge, type label + description.</summary>
        void ZhKitCardBody1639(Card c, float x, float y, float ra, List<string> desc)
        {
            var ic = ZhIcon1618(ZhKitKey1639(c));
            if (ic != null) { GUI.color = new Color(1, 1, 1, ra); GUI.DrawTexture(new Rect(x + 34, y + 19, 48, 48), ic); GUI.color = new Color(1, 1, 1, ra); }
            else BlitInset(_tRoomIcons, 0, 0, 8, 8, x + 42, y + 27, 32, 32);
            Blit(_tNew, 0, 0, 25, 9, x + 84, y + 24);
            desc.Add(ZhKitType1639[c.kit]); var dl = Wrap(ZhKitDesc1639(c), 17); if (dl.Count > 6) dl.RemoveRange(6, dl.Count - 6); desc.AddRange(dl);
        }
    }
}
