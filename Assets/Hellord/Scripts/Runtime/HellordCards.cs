using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public enum CardKind { NewSkill, SkillLevel, Support, Stat }
    public class Card { public CardKind kind; public SkillDef skill; public int skillIndex; public SupportDef support; public StatDef stat; }

    public partial class HellordGame
    {
        public readonly List<Card> cards = new List<Card>(5);
        public int freeRerolls; public string warning;

        void BuildStartCards()
        {
            cards.Clear(); var pool = new List<SkillDef>(HData.Skills); pool.RemoveAll(PactBanned53); Shuffle(pool);
            for (int i = 0; i < Mathf.Min(5, pool.Count); i++) cards.Add(new Card { kind = CardKind.NewSkill, skill = pool[i] });
            warning = null;
        }
        /// <summary>v0.0.9: the level the current card screen is for (GainLevel bumps level at once; queued screens count up one by one).</summary>
        public int ShownLevel => level - pendingLevels;
        readonly HashSet<string> _lastCards = new HashSet<string>();
        static string CardKey(Card c) => c.kind + ":" + (c.skill != null ? c.skill.name : c.support != null ? c.support.name : c.stat != null ? c.stat.name : "");
        void OpenLevelUp() { HellordAudio.Play("levelup_sting"); HellordAudio.DuckMusic(4f, 1.5f); pendingLevels--; mode = Mode.LevelUp; freeRerolls = Mathf.Max(0, HellordTuning.I.levelUpFreeRerolls1057); RollFreshLevelCards(); FX26LevelOpen(); }

        void RollLevelCards()
        {
            cards.Clear(); warning = null;
            var lvl = new List<Card>(); var news = new List<Card>(); var sups = new List<Card>(); var stats = new List<Card>();
            for (int i = 0; i < skills.Count; i++) if (skills[i].level < HData.MaxLv(skills[i].def)) lvl.Add(new Card { kind = CardKind.SkillLevel, skill = skills[i].def, skillIndex = i });
            if (skills.Count < (Manual716 ? HellordTuning.I.manualSkillCap716 : HData.SlotsUnlocked(ShownLevel))) // t716u
                foreach (var d in HData.Skills) if (!PactBanned53(d) && !skills.Exists(s => s.def == d)) news.Add(new Card { kind = CardKind.NewSkill, skill = d });
            var ups = new List<Card>(); // v0.0.27: owned supports below Lv5 are offered as UPGRADES
            foreach (var sd in HData.Supports) if (skills.Exists(s => s.CanSocket(sd) == null)) { var sc = new Card { kind = CardKind.Support, support = sd }; if (skills.Exists(s => s.CanUpgrade(sd))) ups.Add(sc); else sups.Add(sc); }
            foreach (var st in HData.Stats) stats.Add(new Card { kind = CardKind.Stat, stat = st });
            Shuffle(lvl); Shuffle(news); Shuffle(sups); Shuffle(ups); Shuffle(stats);
            if (lvl.Count > 0) { cards.Add(lvl[0]); lvl.RemoveAt(0); } // guarantee: one card levels an owned skill
            var rest = new List<Card>();
            if (news.Count > 0) rest.Add(news[0]);
            if (ups.Count > 0) rest.Add(ups[0]); if (ups.Count > 1 && Random.value < 0.4f) rest.Add(ups[1]); // upgrades show up often
            for (int i = 0; i < Mathf.Min(ups.Count > 0 ? 1 : 2, sups.Count); i++) rest.Add(sups[i]);
            if (lvl.Count > 0) rest.Add(lvl[0]);
            rest.Add(stats[0]); if (stats.Count > 1) rest.Add(stats[1]);
            Shuffle(rest);
            // prefer variety: new skill first if available, then fill
            if (news.Count > 0) { cards.Add(news[0]); rest.Remove(news[0]); }
            int baseChoices1057 = Mathf.Clamp(HellordTuning.I.levelUpCardChoices1057, 2, 5);
            if (ups.Count > 0 && !cards.Contains(ups[0]) && cards.Count < baseChoices1057 + SinExtraCard818() + (news.Count > 0 ? 1 : 0)) { cards.Add(ups[0]); rest.Remove(ups[0]); } // guarantee one support upgrade when any is possible
            int cap818 = baseChoices1057 + SinExtraCard818(); // t1057u: 4 choices (was 3) + sin #25
            for (int i = 0; cards.Count < cap818 && i < rest.Count; i++) if (!cards.Contains(rest[i])) cards.Add(rest[i]);
            Shuffle(cards);
        }
        /// <summary>v0.0.9: chained level-ups re-roll until the hand differs from the previous screen (pool permitting).</summary>
        void RollFreshLevelCards()
        {
            for (int t = 0; t < 6; t++)
            {
                RollLevelCards(); int same = 0; foreach (var c in cards) if (_lastCards.Contains(CardKey(c))) same++;
                if (same < Mathf.Max(1, cards.Count - 1) || _lastCards.Count == 0) break;
            }
            _lastCards.Clear(); foreach (var c in cards) _lastCards.Add(CardKey(c));
        }
        void OpenChest()
        {
            _bossReward37 = false; _chestTitle = _chestSub = null;
            var sups = new List<Card>(); var upg = new List<Card>();
            foreach (var sd in HData.Supports) if (skills.Exists(s => s.CanSocket(sd) == null)) { var sc = new Card { kind = CardKind.Support, support = sd }; if (skills.Exists(s => s.CanUpgrade(sd))) upg.Add(sc); else sups.Add(sc); }
            Shuffle(upg); sups.AddRange(upg); // (re-sorted below: the shuffle keeps upgrades possible in the first 3)
            if (sups.Count == 0) { bones += 15; Toast("Chest: no open support slot - got 15 bones"); return; }
            Shuffle(sups); cards.Clear(); for (int i = 0; i < Mathf.Min(3, sups.Count); i++) cards.Add(sups[i]);
            warning = null; mode = Mode.Chest; HitStop.Play(0.05f);
        }

        /// <summary>t1058u: one free reroll per level-up screen, then bones (levelUpRerollBones1057). Opening does not spend; only a successful reroll debits once.</summary>
        public bool CanReroll1058()
        {
            if (mode != Mode.LevelUp) return false;
            if (freeRerolls > 0) return true;
            int cost = Mathf.Max(0, HellordTuning.I.levelUpRerollBones1057);
            return bones >= cost;
        }
        public int RerollBoneCost1058() => freeRerolls > 0 ? 0 : Mathf.Max(0, HellordTuning.I.levelUpRerollBones1057);
        public void Reroll()
        {
            if (mode != Mode.LevelUp) return;
            if (freeRerolls > 0) { freeRerolls--; RollLevelCards(); HellordAudio.Play("card_reroll"); return; }
            int cost = Mathf.Max(0, HellordTuning.I.levelUpRerollBones1057);
            if (bones < cost) { HellordAudio.Play("ui_denied"); warning = $"NEED {cost} BONES TO REROLL"; Toast($"NEED {cost} BONES"); return; }
            bones -= cost; // exactly one debit on success
            RollLevelCards(); HellordAudio.Play("card_reroll");
            Toast($"REROLL -{cost} BONES");
        }

        /// <summary>Pick a card. For supports pass the target skill index.</summary>
        public void Choose(Card c, int target = -1)
        {
            switch (c.kind)
            {
                case CardKind.NewSkill: { var ns = new SkillInst { def = c.skill }; skills.Add(ns); SetHeroLayers(); OfferSlot716(ns); break; } // t716u: Manual = pick a slot
                case CardKind.SkillLevel:
                    var s = skills[c.skillIndex]; s.level++;
                    if (HData.Summon561(s.def)) Toast($"{s.def.name} " + SummonLvText561(s.def, s.level)); else if (s.level == HData.MaxSkillLevel) Toast($"{s.def.name} MAX: widest reach!"); else Toast($"{s.def.name} Lv{s.level}: bigger reach, support slot {Mathf.Min(s.level, 4)} open");
                    break;
                case CardKind.Support:
                    var t = skills[target]; var why = t.CanSocket(c.support);
                    if (why != null) { warning = why; return; }
                    bool upg = t.Has(c.support.id); t.AddSupport(c.support.id); // v0.0.27: an owned support is upgraded (Lv1-5)
                    Toast(upg ? $"{c.support.name} Lv{t.Lvl(c.support.id)} on {t.def.name}" : $"{c.support.name} added to {t.def.name}"); break;
                case CardKind.Stat:
                    switch (c.stat.id)
                    {
                        case StatId.MaxHP: maxHp *= 1f + HellordTuning.I.statHpFrac; hp = Mathf.Min(maxHp, hp + maxHp * HellordTuning.I.statHpFrac); break;
                        case StatId.Speed: speedMul += HellordTuning.I.statSpeed; break;
                        case StatId.Pickup: pickupMul += 0.15f; break;
                        case StatId.Armor: armor += HellordTuning.I.statArmor; break;
                        case StatId.Crit: crit += HellordTuning.I.statCrit; break;
                        case StatId.Slot: HData.BonusSlots++; _sbFlash = 1.2f; _slotSkillPending = true; Toast("NEW SKILL SLOT UNLOCKED"); break; // v0.0.37 boss reward
                    }
                    break;
            }
            HellordAudio.Play("ui_upgrade_confirm"); // v0.0.34
            if (mode == Mode.LevelUp) FX26LevelPicked(PickKind(c.kind)); // v0.0.26 LevelUpFX pick beat
            cards.Clear(); warning = null; _bossReward37 = false;
            if (mode == Mode.StartPick) { mode = Mode.Play; _mouseT = -99; }
            else mode = Mode.Play;
        }

        /// <summary>v0.0.27: which skill a support card goes to: a skill that already has it (upgrade) first, else the first skill with an open slot that fits. -1 = none.</summary>
        public int SupportTarget(SupportDef sd)
        {
            int u = skills.FindIndex(s => s.CanUpgrade(sd)); if (u >= 0) return u;
            return skills.FindIndex(s => s.CanSocket(sd) == null);
        }

        void UpdateCardKeys()
        {
            if (mode == Mode.Dead && HInput.Down(HKey.R)) { HellordAudio.Play("restart"); /* v0.0.61 */ ResetRun(); PactAfterReset51(); return; } // v0.0.51
            if (_tp != null && mode == Mode.Play && !(Cur != null && Cur.type == RoomType.Shrine)) _tp = null; // v0.0.36: shop pick dies with the shrine
            if (_tmOpen) return; // v0.0.38: test menu open, keys 1-5 must not pick cards
            if (mode != Mode.StartPick && mode != Mode.LevelUp && mode != Mode.Chest) return;
            if (_tp != null) { UpdateTargetKeys(); return; } // v0.0.36: second pick (which skill gets the support) - keys 1-n, ESC back
            if (mode == Mode.LevelUp && HInput.Down(HKey.Space)) Reroll();
            if (FX26CardsLocked) return; // v0.0.26: cards are still sliding in (0.45 s) - no accidental picks
            if ((mode == Mode.LevelUp || (mode == Mode.Chest && !_bossReward37)) && HInput.Down(HKey.Escape)) { SkipCards(); return; }
            HKey[] keys = { HKey.One, HKey.Two, HKey.Three, HKey.Four, HKey.Five };
            for (int i = 0; i < cards.Count && i < 5; i++)
                if (HInput.Down(keys[i]))
                {
                    var c = cards[i]; HellordAudio.Play("ui_card_select"); // v0.0.34
                    if (c.kind == CardKind.Support) { var tl = SupportTargets(c.support); if (tl.Count == 1) Choose(c, tl[0]); else if (tl.Count > 1) OpenTargetPick(c.support, tl, false, tIx => Choose(c, tIx)); }
                    else Choose(c);
                    return;
                }
        }

        /// <summary>v0.8: SKIP on the 640 card screen: no card, +10 bones.</summary>
        void SkipCards() { HellordAudio.Play("card_skip"); /* v0.0.61 */ if (mode == Mode.LevelUp) FX26LevelPicked(LevelPick.Skip); cards.Clear(); warning = null; bones += 10; mode = Mode.Play; Toast("SKIPPED: +10 BONES"); }

        static void Shuffle<T>(List<T> l) { for (int i = l.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (l[i], l[j]) = (l[j], l[i]); } }
    }
}
