using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        // =================================================================================================================
        // t1831: 1920x1080 Play-mode test of the frame D cards. Captures every card screen, measures each drawn card in engine
        // (the draw code records what it drew) and FAILS on any layout violation; audits the copy of every skill / kit
        // skill / support / stat at every level for overflow; writes /workspace/tmp/cards_D_*.png, cards_D_metrics.txt and
        // cards_D_copy.txt.  -executeMethod Hellord.EditorTools.HellordMenu.RunCardsDPlaytest1831 -cardsDPlaytest
        // =================================================================================================================
        System.Collections.IEnumerator CardsDPlaytestCo1831()
        {
            const string dir = "/workspace/tmp";
            void Log(string m) { Debug.Log("[t1831] " + m); }
            string ex = null;
            Application.logMessageReceived += (c, s, typ) => { if (typ != LogType.Exception || ex != null) return; if ((s != null && s.IndexOf("UnityEditor.Search") >= 0) || (c != null && c.IndexOf("UnityEditor.Search") >= 0)) return; ex = c + " | " + s; };
            yield return null;
#if UNITY_EDITOR
            try { var t = System.Type.GetType("Hellord.EditorTools.HellordMenu, Hellord.Editor"); t?.GetMethod("ForceGameView1920x1080_1808", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null); }
            catch (System.Exception e) { Debug.LogWarning("[t1831] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
#endif
            System.IO.Directory.CreateDirectory(dir);
            var T = HellordTuning.I;
            T.testMenuEnabled = false; T.cardsD1831 = true;
            // t1831u: a legal Zhong Kui (Necromancer) run — kit on, testing override OFF, no any-pact debug flag
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = false; T.kitBar1641 = true; T.skillBelt720 = true;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun(); _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51(); PreloadStop1675();
            _dfRunPick1559 = 1; DescendRunPact1555 = 1; _altarRun1561 = true; // so BuildStartCards / DfStartCards1559 uses only Zhong Kui basics
            pendingLevels = 0; level = 4; _tmOpen = false; _tp = null; _luH = -2; freeRerolls = 0;
            var met = new StringBuilder(); int fails = 0; var shots = new List<string>();
            yield return null; GlyphInk1831();
            met.AppendLine("glyph ink table: " + (_gInkL1831 != null ? "read from the font" : "MISSING"));

            // ---------------- copy audit of every entry (all levels) + the copy list
            var copyTxt = new StringBuilder(); var rank = new List<KeyValuePair<int, SkillDef>>(); var srank = new List<KeyValuePair<int, SupportDef>>();
            fails += CopyAuditD1831(copyTxt, met, rank, srank);
            rank.Sort((a, b) => b.Key.CompareTo(a.Key)); srank.Sort((a, b) => b.Key.CompareTo(a.Key));
            System.IO.File.WriteAllText(dir + "/cards_D_copy.txt", copyTxt.ToString());

            // t1831u: every damaging skill card shows a DAMAGE row whose printed value matches the live evaluator
            {
                var damaging = new HashSet<SkillId> {
                    SkillId.Fireball, SkillId.FrostCleave, SkillId.StormRing, SkillId.BoneOrbit, SkillId.CurseRune, SkillId.ChainLightning,
                    SkillId.HellfirePillar, SkillId.BloodWhip, SkillId.SoulSpears, SkillId.GravePool, SkillId.SkeletonArchers, SkillId.SkeletonWarriors,
                    SkillId.Frostbolt, SkillId.IceNova, SkillId.FrostBomb, SkillId.Hurricane, SkillId.MagmaThrow, SkillId.RollingMagmaRock,
                    SkillId.FirePillar, SkillId.FireWall, SkillId.Hydra, SkillId.SparkSpray, SkillId.BallLightning, SkillId.ThunderJavelin,
                    SkillId.HeadsmansChop, SkillId.EarthSplittingSlam, SkillId.TwinAxeThrow, SkillId.PhantomAxes, SkillId.ThunderDrumStrikes,
                    SkillId.ThunderLash, SkillId.LiveWire, SkillId.ThunderHound, SkillId.TidalSurge, SkillId.Whirlpool, SkillId.BreathOfTheDeep,
                    SkillId.BuzhouRift, SkillId.EyeOfTheBlackWaters, SkillId.Teeth,
                    SkillId.ZkWarCry, SkillId.ZkSmolderRing, SkillId.ZkSunsWrath, SkillId.ZkRebirthFlame, SkillId.ZkVermilionBird,
                    SkillId.LzMarkOfJudgement, SkillId.LzThunderDrum, SkillId.LzStaticField, SkillId.LzDecree, SkillId.LzHeavenArc, SkillId.LzTribunal, SkillId.LzJudgementWarp,
                    SkillId.GgShatter, SkillId.GgToppledPillar, SkillId.GgFrostWall, SkillId.GgRamOfBuzhou, SkillId.GgGreatFlood, SkillId.GgEternalWinter, SkillId.GgPillarsOfAo,
                    SkillId.NkSoulPlague, SkillId.NkSkeletalMage, SkillId.NkClayGolem, SkillId.NkOssuaryNova, SkillId.NkBoneColossus
                };
                int dmgN = 0, dmgFail = 0;
                foreach (var d in HData.Skills)
                {
                    if (d == null || !damaging.Contains(d.id)) continue;
                    int max = HData.MaxLv(d);
                    for (int L = 1; L <= max; L++)
                    {
                        dmgN++;
                        var f = new CardFaceD(); f.name = d.name; f.title = WrapCard(d.name);
                        SkillFaceD1831(f, d, null, L, null);
                        string got = null; foreach (var r in f.rows) if (LineStr(r[0]) == "DAMAGE") { got = LineStr(r[1]); break; }
                        if (got == null) { dmgFail++; fails++; met.AppendLine("FAIL damage row missing on " + d.name + " Lv" + L); continue; }
                        CardCopyD1831.Skill.TryGetValue(d.id, out var src);
                        string wantTok = null;
                        if (src != null) foreach (var line in src)
                        {
                            string b = line;
                            if (b.StartsWith("?")) { int e = b.IndexOf('?', 1); if (e > 0) b = b.Substring(e + 1); }
                            if (b.StartsWith("R:DAMAGE ~ ")) { wantTok = b.Substring("R:DAMAGE ~ ".Length); break; }
                        }
                        if (wantTok == null) { dmgFail++; fails++; met.AppendLine("FAIL " + d.name + " Lv" + L + " has no R:DAMAGE in D copy"); continue; }
                        var ctx = CtxFor1825(null, d, L, 0); ctx.dmg = LiveDmg1831(d, null, L, f.errs); ctx.kit = LiveKit1831(null, L, f.errs);
                        var ev = new List<string>(); string want = CardEval1825.Resolve(wantTok, ctx, ev);
                        if (got != want) { dmgFail++; fails++; met.AppendLine("FAIL " + d.name + " Lv" + L + " DAMAGE row '" + got + "' != live '" + want + "'" + (ev.Count > 0 ? " eval:" + string.Join(",", ev) : "") + (f.errs.Count > 0 ? " face:" + string.Join(",", f.errs) : "")); }
                        // also any SLAM / MELEE row must match the live evaluator (Jiangshi shows both melee DAMAGE and SLAM)
                        if (src != null) foreach (var line in src)
                        {
                            string b = line;
                            if (b.StartsWith("?")) { int e = b.IndexOf('?', 1); if (e > 0) b = b.Substring(e + 1); }
                            if (!b.StartsWith("R:SLAM ~ ") && !b.StartsWith("R:MELEE ~ ")) continue;
                            int sp = b.IndexOf(" ~ "); string lab = b.Substring(2, sp - 2); string tok = b.Substring(sp + 3);
                            string got2 = null; foreach (var r in f.rows) if (LineStr(r[0]) == lab) { got2 = LineStr(r[1]); break; }
                            var ev2 = new List<string>(); string want2 = CardEval1825.Resolve(tok, ctx, ev2);
                            if (got2 == null) { dmgFail++; fails++; met.AppendLine("FAIL " + d.name + " Lv" + L + " " + lab + " row missing"); }
                            else if (got2 != want2) { dmgFail++; fails++; met.AppendLine("FAIL " + d.name + " Lv" + L + " " + lab + " row '" + got2 + "' != live '" + want2 + "'" + (ev2.Count > 0 ? " eval:" + string.Join(",", ev2) : "")); }
                        }
                    }
                }
                met.AppendLine("t1831u damage-row audit: " + (dmgN - dmgFail) + "/" + dmgN + " damaging skill x level cards match the live value" + (dmgFail > 0 ? "  FAILS=" + dmgFail : ""));
                // Jiangshi melee: named T.nkGolemDmgPerLv1713 must equal the retired 0.12-per-level formula at every level (no balance change)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("t1831u Jiangshi melee before/after (base T.nkGolemDmg1713=" + T.nkGolemDmg1713.ToString("0.##") + "  perLv T.nkGolemDmgPerLv1713=" + T.nkGolemDmgPerLv1713.ToString("0.##") + "  |i| is the card display):");
                    sb.AppendLine("  Lv  before(raw)  after(raw)  match  card(|i|)");
                    int mm = 0;
                    for (int L = 1; L <= 5; L++)
                    {
                        float before = T.nkGolemDmg1713 * (1f + 0.12f * (L - 1));
                        float after = T.nkGolemDmg1713 * (1f + T.nkGolemDmgPerLv1713 * (L - 1));
                        bool same = Mathf.Approximately(before, after);
                        if (!same) { mm++; fails++; }
                        sb.AppendLine("  " + L + "   " + before.ToString("0.00") + "        " + after.ToString("0.00") + "       " + (same ? "OK" : "FAIL") + "    " + Mathf.RoundToInt(after));
                    }
                    if (mm > 0) met.AppendLine("FAIL Jiangshi melee before/after mismatch at " + mm + " level(s)");
                    met.Append(sb.ToString());
                }
            }


            // ---------------- t1831 task 9: icon sweep (every skill / kit skill / support / stat icon source -> integer scale)
            met.AppendLine("== icon sweep (each icon source at the largest whole scale that fits the 48 px ring)"); int uneq = 0, nIc = 0;
            void Sweep(string who, IconSrcD s)
            {
                nIc++; if (s.t == null) { fails++; met.AppendLine("  FAIL icon " + who + " has no texture"); return; }
                int sc = Mathf.Max(1, Mathf.Min(CardIcon / s.sw, CardIcon / s.sh)), mw = CardIcon - s.sw * sc, mh = CardIcon - s.sh * sc; bool un = mw % 2 != 0 || mh % 2 != 0; if (un) uneq++;
                met.AppendLine("  icon " + who + " " + s.via + " '" + s.t.name + "' " + s.sw + "x" + s.sh + " x" + sc + " -> " + s.sw * sc + "x" + s.sh * sc + " margins L" + mw / 2 + " R" + (mw - mw / 2) + " T" + mh / 2 + " B" + (mh - mh / 2) + (un ? " UNEQUAL" : "") + (s.sw > CardIcon || s.sh > CardIcon ? " CROPPED" : ""));
            }
            foreach (var d in HData.Skills) if (d != null) Sweep("skill " + d.id, SkillIconSrcD1831(d.id));
            foreach (var sd in HData.Supports) if (sd != null) Sweep("support " + sd.id, CardIconSrcD1831(new Card { kind = CardKind.Support, support = sd }));
            foreach (var st in HData.Stats) Sweep("stat " + st.id, CardIconSrcD1831(new Card { kind = CardKind.Stat, stat = st }));
            met.AppendLine("  icon sweep: " + nIc + " icons, unequal margins " + uneq);

            // ---------------- rows (t1831u: every captured screen is a legal Zhong Kui loadout / offer)
            UnityEngine.Random.InitState(1831);
            bool InActivePactSkill1831u(SkillDef d)
            {
                if (d == null) return false;
                foreach (var id in DfBasics1555(_pactPath51, true)) if (id == d.id) return true;
                var raw = DfBasicsRaw1555(_pactPath51); if (raw != null) foreach (var id in raw) if (id == d.id) return true;
                int m = ZhRunMove1639, sp = ZhRunSpec1639, u = ZhRunUlt1639;
                ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0;
                bool ok = false;
                foreach (var c in KitOffers1639()) { var id = KitOfferId1831u(c); if (id != 0 && id == d.id) { ok = true; break; } }
                ZhRunMove1639 = m; ZhRunSpec1639 = sp; ZhRunUlt1639 = u;
                return ok;
            }
            SkillId KitOfferId1831u(Card c)
            {
                if (c == null || c.kit < 1) return 0; int k = KitK1666(c.kit - 1, c.kitPick);
                if (NkOn1713 && k >= NkK0 && k - NkK0 < NkIds1713.Length) return NkIds1713[k - NkK0];
                if (GgOn1682 && k >= GgK0 && k - GgK0 < GgIds1682.Length) return GgIds1682[k - GgK0];
                if (LzOn1666 && k >= LzK0 && k - LzK0 < LzIds1666.Length) return LzIds1666[k - LzK0];
                if (ZhOn1618 && k >= 0 && k < ZkIds1651.Length) return ZkIds1651[k];
                return 0;
            }
            bool InActivePactCard1831u(Card c)
            {
                if (c == null) return false;
                if (c.kit > 0)
                {
                    int m = ZhRunMove1639, sp = ZhRunSpec1639, u = ZhRunUlt1639;
                    ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0;
                    bool ok = false; foreach (var o in KitOffers1639()) if (o.kit == c.kit && o.kitPick == c.kitPick) { ok = true; break; }
                    ZhRunMove1639 = m; ZhRunSpec1639 = sp; ZhRunUlt1639 = u;
                    return ok;
                }
                return InActivePactSkill1831u(c.skill);
            }
            bool GainBasic1831u(SkillId want)
            {
                skills.Clear(); ZhRunReset1639(); SetHeroLayers();
                BuildStartCards();
                Card c = null; foreach (var x in cards) if (x.skill != null && x.skill.id == want) c = x;
                if (c == null) return false; Choose(c); SetHeroLayers(); return skills.Count > 0 && skills[0].def.id == want;
            }
            bool GainKit1831u(int kit, int pick)
            {
                Card c = null; foreach (var x in KitOffers1639()) if (x.kit == kit && x.kitPick == pick) c = x;
                if (c == null) return false; Choose(c); SetHeroLayers(); return ZkSlotInst1651(kit - 1) != null;
            }
            Card ST(StatId id) { foreach (var st in HData.Stats) if (st.id == id) return new Card { kind = CardKind.Stat, stat = st }; return new Card { kind = CardKind.Stat, stat = new StatDef { id = id, name = "New Skill Slot" } }; }
            System.Collections.IEnumerator Row(string tag, Mode m)
            {
                mode = m; _luH = -2; warning = null;
                for (int i = 0; i < 14; i++) yield return null;
                _auditD1831 = new List<AuditRecD>();
                for (int i = 0; i < 3 && _auditD1831.Count == 0; i++) yield return new WaitForEndOfFrame();
                var recs = _auditD1831; _auditD1831 = null;
                fails += RowAuditD1831(tag, recs, met);
                foreach (var c in cards)
                {
                    if (c.kind == CardKind.Support) { if (SupportTargets(c.support).Count == 0) { fails++; met.AppendLine("FAIL " + tag + ": offered unusable support " + c.support.name); } continue; }
                    if (c.kind == CardKind.Stat) continue;
                    if (!InActivePactCard1831u(c)) { fails++; met.AppendLine("FAIL " + tag + ": shown skill is not in the active pact's pool (" + CardKey(c) + ")"); }
                }
                foreach (var sk in skills) if (!InActivePactSkill1831u(sk.def)) { fails++; met.AppendLine("FAIL " + tag + ": owned skill " + sk.def.name + " is not in the active pact's pool"); }
                if (_tp != null) foreach (int ti in _tp.targets)
                { var sk = SupInst1651(ti); if (sk == null || sk.CanSocket(_tp.sd) != null) { fails++; met.AppendLine("FAIL " + tag + ": picker lists a skill that can't take " + _tp.sd.name); }
                  else if (!InActivePactSkill1831u(sk.def)) { fails++; met.AppendLine("FAIL " + tag + ": picker skill " + sk.def.name + " is not in the active pact's pool"); } }
                string path = dir + "/cards_D_" + tag + ".png"; shots.Add(path);
                yield return Shot1822(path);
            }


            if (!NkOn1713) { fails++; met.AppendLine("FAIL t1831u Zhong Kui kit is not active (NkOn1713=false, path=" + _pactPath51 + " kit=" + T.kit1618 + " nk=" + T.nkKit1713 + " any=" + T.kitAnyPact1618 + " override=" + T.kitSlotOverride1639 + ")"); }
            {
                var names = new List<string>();
                foreach (var id in DfBasics1555(1, true)) names.Add(HData.Skill(id).name);
                int m = ZhRunMove1639, sp = ZhRunSpec1639, u = ZhRunUlt1639; ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0;
                foreach (var c in KitOffers1639()) names.Add(ZhKitName1639(c));
                ZhRunMove1639 = m; ZhRunSpec1639 = sp; ZhRunUlt1639 = u;
                met.AppendLine("t1831u pact=Zhong Kui (Necromancer, path 1)  NkOn=" + NkOn1713 + "  pool: " + string.Join(", ", names));
            }

            if (CardKeyRange1831(1) != "1" || CardKeyRange1831(2) != "1-2" || CardKeyRange1831(4) != "1-4")
            { fails++; met.AppendLine("FAIL CardKeyRange1831: 1='" + CardKeyRange1831(1) + "' 2='" + CardKeyRange1831(2) + "' 4='" + CardKeyRange1831(4) + "'"); }

            // 1 start pick: real BuildStartCards / DfStartCards1559 (only Zhong Kui basics)
            skills.Clear(); ZhRunReset1639(); SetHeroLayers();
            BuildStartCards();
            met.AppendLine("start cards: " + string.Join(", ", cards.ConvertAll(c => c.skill != null ? c.skill.name : CardKey(c))));
            yield return Row("start", Mode.StartPick);

            // 2 pick Skeleton Warriors through Choose, then a real first level-up (kit NewSkill cards from this pact)
            if (!GainBasic1831u(SkillId.SkeletonWarriors)) { fails++; met.AppendLine("FAIL t1831u could not Choose Skeleton Warriors from the start pick"); }
            {
                int bestN = -1; var best = new List<Card>();
                for (int k = 0; k < 40; k++)
                {
                    RollLevelCards(); int n = 0; foreach (var c in cards) if (c.kind == CardKind.NewSkill && c.kit > 0 && !c.kitLv1651) n++;
                    if (n > bestN) { bestN = n; best = new List<Card>(cards); }
                }
                cards.Clear(); cards.AddRange(best);
                met.AppendLine("skills row (real RollLevelCards, " + bestN + " kit NewSkill cards): " + string.Join(", ", cards.ConvertAll(CardKey)));
                if (bestN < 1) { fails++; met.AppendLine("FAIL t1831u level-up skill offer had no kit NewSkill card"); }
            }
            yield return Row("skills", Mode.LevelUp);

            // 3 gain Jiangshi + Ossuary Nova through Choose from the real kit offer pool (same pool RollLevelCards uses)
            if (!GainKit1831u(2, 4)) { fails++; met.AppendLine("FAIL t1831u could not Choose Jiangshi from KitOffers1639"); }
            if (!GainKit1831u(3, 1)) { fails++; met.AppendLine("FAIL t1831u could not Choose Ossuary Nova from KitOffers1639"); }
            met.AppendLine("t1831u loadout: " + skills[0].def.name + " + " + (ZkSlotInst1651(1) != null ? ZkSlotInst1651(1).def.name : "?") + " + " + (ZkSlotInst1651(2) != null ? ZkSlotInst1651(2).def.name : "?") + " (movement empty)");

            // 4 support offer: real OpenChest (only supports the loadout can use)
            OpenChest();
            met.AppendLine("supports row (real OpenChest): " + string.Join(", ", cards.ConvertAll(c => c.support != null ? c.support.name : CardKey(c))));
            if (cards.Count == 0 || cards.Exists(c => c.kind != CardKind.Support)) { fails++; met.AppendLine("FAIL t1831u OpenChest did not leave a support-only row"); }
            yield return Row("supports", Mode.Chest);

            // 5 WHICH SKILL?: a support from the real offer filter with 2+ matching skills in this loadout
            {
                SupportDef sd = null; var tl = new List<int>();
                foreach (var s in HData.Supports) if (HData.Offered1654(s)) { var t = SupportTargets(s); if (t.Count > (tl != null ? tl.Count : 0)) { sd = s; tl = t; } }
                if (sd != null && sd.id != SupportId.Burn)
                { var burn = HData.Support(SupportId.Burn); var bt = SupportTargets(burn); if (bt.Count >= 2) { sd = burn; tl = bt; } }
                cards.Clear(); if (sd != null) cards.Add(new Card { kind = CardKind.Support, support = sd });
                if (sd == null || tl.Count < 2)
                { fails++; met.AppendLine("FAIL t1831u this Zhong Kui loadout has no support with 2+ matching skills (best=" + (sd != null ? sd.name + " x" + tl.Count : "none") + ")"); _tp = null; }
                else
                {
                    PickSupTarget1831u(sd, tl, false, ix => { });
                    met.AppendLine("picker " + sd.name + " targets=" + (_tp != null ? _tp.targets.Count : 0) + " (" + string.Join(", ", tl.ConvertAll(i => { var sk = SupInst1651(i); return sk != null ? sk.def.name : i.ToString(); })) + ")");
                    if (_tp == null || _tp.targets.Count < 2) { fails++; met.AppendLine("FAIL t1831u picker did not open with 2+ matching skills"); }
                }
            }
            yield return Row("picker", Mode.LevelUp);
            _tp = null;
            // Soul Plague + Ossuary Nova as kit NewSkill cards (DAMAGE first), the pair VH asked to see
            cards.Clear();
            { int m = ZhRunMove1639, sp = ZhRunSpec1639, u = ZhRunUlt1639; ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0;
              var off = KitOffers1639();
              foreach (var x in off) if (x.kit == 2 && x.kitPick == 2) cards.Add(x);
              foreach (var x in off) if (x.kit == 3 && x.kitPick == 1) cards.Add(x);
              ZhRunMove1639 = m; ZhRunSpec1639 = sp; ZhRunUlt1639 = u; }
            met.AppendLine("plague_nova cards: " + string.Join(", ", cards.ConvertAll(c => ZhKitName1639(c))));
            yield return Row("plague_nova", Mode.LevelUp);
            // Jiangshi in a normal 3-4 card level-up row from this legal Zhong Kui loadout (real RollLevelCards)
            {
                bool IsJiangshi(Card c) => c != null && ((c.kit == 2 && c.kitPick == 4) || (c.kind == CardKind.SkillLevel && c.skill != null && c.skill.id == SkillId.NkClayGolem) || (c.kind == CardKind.NewSkill && c.kit == 0 && c.skill != null && c.skill.id == SkillId.NkClayGolem));
                List<Card> best = null;
                for (int k = 0; k < 80; k++)
                {
                    RollLevelCards();
                    if (cards.Count >= 3 && cards.Count <= 4 && cards.Exists(IsJiangshi)) { best = new List<Card>(cards); break; }
                }
                if (best == null) { fails++; met.AppendLine("FAIL t1831u no 3-4 card RollLevelCards row included Jiangshi"); }
                else { cards.Clear(); cards.AddRange(best); }
                met.AppendLine("jiangshi row (" + cards.Count + " cards, real RollLevelCards): " + string.Join(", ", cards.ConvertAll(CardKey)));
                if (cards.Count < 3 || cards.Count > 4) { fails++; met.AppendLine("FAIL t1831u Jiangshi row is not a 3-4 card row (" + cards.Count + ")"); }
            }
            yield return Row("jiangshi", Mode.LevelUp);

            // 6 owned-skill level-up row (real RollLevelCards after the three skills exist)
            RollLevelCards();
            met.AppendLine("levelup row (real RollLevelCards): " + string.Join(", ", cards.ConvertAll(CardKey)));
            yield return Row("levelup", Mode.LevelUp);

            // 7 layout stress: the 4 longest cards in the active pact's kit pool (not a single roll; all are offerable on this pact)
            {
                int m = ZhRunMove1639, sp = ZhRunSpec1639, u = ZhRunUlt1639; ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0;
                var kit = new List<Card>(KitOffers1639());
                ZhRunMove1639 = m; ZhRunSpec1639 = sp; ZhRunUlt1639 = u;
                var scored = new List<KeyValuePair<int, Card>>();
                foreach (var c in kit)
                {
                    int n = 0; foreach (var kv in rank) { var id = KitOfferId1831u(c); if (id != 0 && kv.Value != null && kv.Value.id == id) { n = kv.Key; break; } }
                    scored.Add(new KeyValuePair<int, Card>(n, c));
                }
                scored.Sort((a, b) => b.Key.CompareTo(a.Key));
                cards.Clear(); for (int i = 0; i < Mathf.Min(4, scored.Count); i++) cards.Add(scored[i].Value);
                met.AppendLine("longest pact kit cards (layout stress): " + string.Join(", ", cards.ConvertAll(c => ZhKitName1639(c) + " (" + scored[cards.IndexOf(c)].Key + ")")));
            }
            yield return Row("longest", Mode.LevelUp);

            // 8 longest eligible supports for this loadout (real offer filter, layout stress)
            {
                var fit = srank.FindAll(k => HData.Offered1654(k.Value) && SupportTargets(k.Value).Count > 0);
                cards.Clear(); for (int i = 0; i < Mathf.Min(4, fit.Count); i++) cards.Add(new Card { kind = CardKind.Support, support = fit[i].Value });
                met.AppendLine("longest eligible supports: " + string.Join(", ", cards.ConvertAll(c => c.support.name + " (" + fit[cards.IndexOf(c)].Key + ")")));
            }
            yield return Row("longest_sup", Mode.LevelUp);

            // 9 stats (no pact identity; loadout stays the legal Zhong Kui one)
            cards.Clear(); cards.Add(ST(StatId.Speed)); cards.Add(ST(StatId.Armor)); cards.Add(ST(StatId.Crit)); cards.Add(ST(StatId.Slot));
            yield return Row("stats", Mode.LevelUp);

            // ---------------- t1831u offer-filter + pick-path checks ----------------
            {
                int sup = 0, bad = 0, empty = 0; var seen = new HashSet<string>();
                for (int k = 0; k < 300; k++) { RollLevelCards(); if (cards.Count == 0) empty++; foreach (var c in cards) { if (c.kind == CardKind.NewSkill || c.kind == CardKind.SkillLevel) { if (!InActivePactCard1831u(c)) { bad++; met.AppendLine("FAIL t1831u roll offered off-pact skill " + CardKey(c)); } } if (c.kind == CardKind.Support) { sup++; seen.Add(c.support.name); if (SupportTargets(c.support).Count == 0) bad++; } } }
                string ln = "t1831u offer sweep [Zhong Kui loadout] support cards=" + sup + " bad=" + bad + " empty rows=" + empty + " offered supports: " + string.Join(", ", seen);
                met.AppendLine((bad > 0 || empty > 0 ? "FAIL " : "") + ln); if (bad > 0 || empty > 0) fails++;
                cards.Clear();
            }
            // 1 match = apply directly (drop to one HIT skill: only the basic, kit slots cleared)
            { int sm = ZhRunMove1639, ssp = ZhRunSpec1639, su = ZhRunUlt1639; ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0;
              var sd = HData.Support(SupportId.Burn); var tl = SupportTargets(sd); bool applied = false; PickSupTarget1831u(sd, tl, false, ix => { applied = true; });
              if (tl.Count != 1 || !applied || _tp != null) { fails++; met.AppendLine("FAIL t1831u 1-match should apply directly, targets=" + tl.Count + " applied=" + applied + " picker=" + (_tp != null)); }
              else met.AppendLine("t1831u 1-match applied directly, no picker");
              ZhRunMove1639 = sm; ZhRunSpec1639 = ssp; ZhRunUlt1639 = su; _tp = null; }
            // 0 matches = no empty picker (Pierce needs Projectile; this loadout has none)
            { warning = null; var sd = HData.Support(SupportId.Pierce); PickSupTarget1831u(sd, SupportTargets(sd), false, ix => { fails++; met.AppendLine("FAIL t1831u 0-match applied a target"); });
              if (_tp != null) { fails++; met.AppendLine("FAIL t1831u 0-match opened a picker"); _tp = null; }
              else if (string.IsNullOrEmpty(warning)) { fails++; met.AppendLine("FAIL t1831u 0-match left no fallback warning"); }
              else met.AppendLine("t1831u 0-match fallback: " + warning + " (no picker)");
              warning = null; }

            met.Insert(0, "t1831 frame D card audit  fails=" + fails + "\n");
            System.IO.File.WriteAllText(dir + "/cards_D_metrics.txt", met.ToString());
            foreach (var line in met.ToString().Split('\n')) if (line.Contains("FAIL")) Log(line);
            int z = CamZoom();
            bool camOk = Screen.width == 1920 && Screen.height == 1080 && z == 3 && T.camTargetPx49.x == 640f && T.camTargetPx49.y == 360f;
            bool files = shots.TrueForAll(System.IO.File.Exists);
            bool ok = camOk && files && ex == null && fails == 0;
            Log("screen " + Screen.width + "x" + Screen.height + " zoom=" + z + " camTarget=" + T.camTargetPx49 + " fails=" + fails);
            if (ex != null) Log("exception " + ex);
            Log(ok ? "PASS" : "FAIL cam=" + camOk + " files=" + files + " fails=" + fails + " ex=" + (ex ?? "none"));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }

        int FaceChecksD1831(string who, CardFaceD f, StringBuilder met, int extraFooter = 0)
        {
            int fails = 0; void F(string m) { fails++; met.AppendLine("  FAIL " + who + ": " + m); }
            foreach (var e in f.errs) F("copy error " + e);
            foreach (var w in f.widows) met.AppendLine("  WRAP " + who + " last-line single word: '" + w + "'");
            foreach (var l in f.tags) { string t = LineStr(l).Trim(); if (t.EndsWith(",") && f.tags.IndexOf(l) == f.tags.Count - 1) F("tag block ends with a comma"); }
            if (f.title.Count > CardDTitleMax) F("title " + f.title.Count + " lines");
            foreach (var l in f.title) if (InkW(l) > CardTextW) F("title " + InkW(l) + " px: '" + l + "'");
            if (f.head.Count > CardDHeadMax) F("headline " + f.head.Count + " lines (max " + CardDHeadMax + ")"); else if (f.head.Count > 2) met.AppendLine("  WARN " + who + ": headline 3 lines");
            if (f.detail.Count > CardDDetailMax) F("detail " + f.detail.Count + " lines (max " + CardDDetailMax + ")");
            if (f.tags.Count > CardDTagMax) F("tags " + f.tags.Count + " lines (max " + CardDTagMax + ")");
            foreach (var blk in new[] { f.head, f.detail, f.tags }) foreach (var l in blk) { int w = InkW(LineStr(l)); if (w > CardTextW) F("line " + w + " px > " + CardTextW + ": '" + LineStr(l) + "'"); }
            foreach (var r in f.rows) { int lw = InkW(LineStr(r[0])), vw = InkW(LineStr(r[1])); if (lw + CardDRowGap + vw > CardTextW) F("row " + lw + "+" + CardDRowGap + "+" + vw + " px > " + CardTextW + ": '" + LineStr(r[0]) + " | " + LineStr(r[1]) + "'"); if (lw <= 0 || vw <= 0) F("row with an empty side"); }
            var lay = LayoutRowD1831(new[] { f }); int bottom = CardRowY() + lay.cardH + CardPad + CardGlyph + extraFooter;
            if (bottom > HH) F("card + footer bottom " + bottom + " > " + HH);
            return fails;
        }
        static string DumpD(CardFaceD f)
        {
            var sb = new StringBuilder();
            foreach (var l in f.head) sb.AppendLine("  head : " + LineStr(l));
            foreach (var r in f.rows) sb.AppendLine("  row  : " + LineStr(r[0]).PadRight(14) + " | " + LineStr(r[1]));
            foreach (var l in f.detail) sb.AppendLine("  det  : " + LineStr(l));
            foreach (var l in f.tags) sb.AppendLine("  tags : " + LineStr(l));
            return sb.ToString();
        }
        static int ScoreD(CardFaceD f) => f.head.Count * 100 + f.rows.Count * 100 + f.detail.Count * 100 + f.tags.Count * 10;
        int CopyAuditD1831(StringBuilder txt, StringBuilder met, List<KeyValuePair<int, SkillDef>> rank, List<KeyValuePair<int, SupportDef>> srank)
        {
            int fails = 0; var wrapR = new List<string>(); met.AppendLine("== copy audit (every skill / kit skill at Lv1..max, every support Lv1..3, every stat)");
            txt.AppendLine("t1831 frame D card copy - every skill, kit skill, support and stat (resolved from live data at test time, new run, no supports).");
            txt.AppendLine();
            txt.AppendLine("==================== SKILLS ===================="); int nSk = 0, maxH = 0;
            foreach (var d in HData.Skills)
            {
                if (d == null) continue; nSk++;
                if (!CardCopyD1831.Skill.ContainsKey(d.id)) { fails++; met.AppendLine("  FAIL no D copy for skill " + d.id); continue; }
                for (int lv = 1; lv <= HData.MaxLv(d); lv++)
                {
                    var inst = lv > 1 ? new SkillInst { def = d, level = lv - 1 } : null;
                    var f = new CardFaceD { name = d.name, title = WrapCard(d.name) }; SkillFaceD1831(f, d, inst, lv, KitHeaderD1831(d));
                    fails += FaceChecksD1831(d.id + " Lv" + lv, f, met);
                    foreach (var n in f.wrapNotes) wrapR.Add(d.name + " Lv" + lv + ": " + n);
                    maxH = Mathf.Max(maxH, LayoutRowD1831(new[] { f }).cardH);
                    if (lv == 1) { txt.AppendLine(d.name.ToUpperInvariant() + (d.kit1651 ? "  [kit]" : "")); txt.Append(DumpD(f)); if (d.id != SkillId.ZhurongKit) rank.Add(new KeyValuePair<int, SkillDef>(ScoreD(f), d)); }
                    else { var sb = new StringBuilder(); foreach (var r in f.rows) sb.Append(LineStr(r[0]) + " " + LineStr(r[1]) + " | "); txt.AppendLine("  Lv" + lv + ": " + sb); }
                }
                txt.AppendLine();
            }
            txt.AppendLine("==================== SUPPORTS ===================="); int nSu = 0;
            foreach (var sd in HData.Supports)
            {
                if (sd == null) continue; nSu++;
                if (!CardCopyD1831.Support.ContainsKey(sd.id)) { fails++; met.AppendLine("  FAIL no D copy for support " + sd.id); continue; }
                for (int lv = 1; lv <= SkillInst.MaxSupLevel; lv++)
                {
                    var f = new CardFaceD { name = sd.name, title = WrapCard(sd.name) }; SupportFaceD1831(f, sd, lv, null, true);
                    fails += FaceChecksD1831(sd.id + " Lv" + lv, f, met, CardLine * 2);
                    foreach (var n in f.wrapNotes) wrapR.Add(sd.name + " Lv" + lv + ": " + n);
                    maxH = Mathf.Max(maxH, LayoutRowD1831(new[] { f }).cardH);
                    if (lv == 1) { txt.AppendLine(sd.name.ToUpperInvariant() + (sd.retired1654 ? "  [retired, never offered]" : "")); txt.Append(DumpD(f)); if (!sd.retired1654) srank.Add(new KeyValuePair<int, SupportDef>(ScoreD(f), sd)); }
                    else { var sb = new StringBuilder(); foreach (var r in f.rows) sb.Append(LineStr(r[0]) + " " + LineStr(r[1]) + " | "); foreach (var l in f.detail) sb.Append("(" + LineStr(l) + ") "); txt.AppendLine("  Lv" + lv + ": " + sb); }
                }
                txt.AppendLine();
            }
            { var f = new CardFaceD { name = "Echo" }; var cr = new SkillInst { def = HData.Skill(SkillId.CurseRune), level = 2 }; SupportFaceD1831(f, HData.Support(SupportId.Echo), 1, cr, false); fails += FaceChecksD1831("Echo on Curse Rune", f, met); foreach (var n in f.wrapNotes) wrapR.Add("Echo on Curse Rune: " + n); txt.AppendLine("ECHO (ON CURSE RUNE)"); txt.Append(DumpD(f)); txt.AppendLine(); }
            txt.AppendLine("==================== STATS ===================="); int nSt = 0;
            foreach (var st in HData.Stats)
            {
                nSt++; if (!CardCopyD1831.Stat.ContainsKey(st.id)) { fails++; met.AppendLine("  FAIL no D copy for stat " + st.id); continue; }
                var f = BuildFaceD1831(new Card { kind = CardKind.Stat, stat = st }); fails += FaceChecksD1831("stat " + st.id, f, met); foreach (var n in f.wrapNotes) wrapR.Add("stat " + st.name + ": " + n); txt.AppendLine(st.name.ToUpperInvariant()); txt.Append(DumpD(f)); txt.AppendLine();
            }
            { var slot = new StatDef { id = StatId.Slot, name = "New Skill Slot" }; var f = BuildFaceD1831(new Card { kind = CardKind.Stat, stat = slot }); fails += FaceChecksD1831("stat Slot", f, met); foreach (var n in f.wrapNotes) wrapR.Add("stat Slot: " + n); txt.AppendLine("NEW SKILL SLOT"); txt.Append(DumpD(f)); }
            met.AppendLine("t1831u wrap balance: " + (wrapR.Count == 0 ? "every block meets shortest >= 50% of longest and has no single-word line" : wrapR.Count + " block(s) cannot meet 50% / no single-word (reported, not a fail)"));
            foreach (var n in wrapR) met.AppendLine("  WRAP " + n);
            met.AppendLine("  entries: skills " + nSk + " (D copy " + CardCopyD1831.Skill.Count + "), supports " + nSu + " (D copy " + CardCopyD1831.Support.Count + "), stats " + nSt + "+slot, tallest single card " + maxH + " px, fails " + fails);
            return fails;
        }

        int RowAuditD1831(string tag, List<AuditRecD> recs, StringBuilder met)
        {
            int fails = 0; void F(string m) { fails++; met.AppendLine("  FAIL " + m); }
            met.AppendLine("== row " + tag + " (" + (recs?.Count ?? 0) + " cards)");
            if (recs == null || recs.Count == 0) { F(tag + " nothing drawn"); return fails; }
            int n = recs.Count, x0 = recs[0].x, right = HW - (recs[n - 1].x + CardW);
            met.AppendLine("  row x0=" + x0 + " right=" + right + " cardH=" + recs[0].cardH + " rowY=" + CardRowY());
            if (Mathf.Abs(x0 - right) > 1) F(tag + " row margins L" + x0 + " R" + right);
            for (int i = 1; i < n; i++) if (recs[i].x - (recs[i - 1].x + CardW) != CardGap) F(tag + " gap " + i + " = " + (recs[i].x - (recs[i - 1].x + CardW)));
            var secRef = new Dictionary<string, int>(); int iconRef = int.MinValue;
            for (int i = 0; i < n; i++)
            {
                var r = recs[i]; string who = tag + "#" + (i + 1) + " " + r.name;
                met.AppendLine(" card " + (i + 1) + " '" + r.name + "' " + (r.support ? "support" : "skill") + " x=" + r.x + " y=" + r.y + " h=" + r.cardH);
                foreach (var l in r.log) met.AppendLine("   " + l);
                if (r.cardH != recs[0].cardH) F(who + " height " + r.cardH + " vs " + recs[0].cardH);
                foreach (var c in r.centred)
                {
                    int L = c[0] - r.x, R = r.x + CardW - 1 - c[1];
                    if (L < CardTextPad || R < CardTextPad) F(who + " " + SecD[c[3]] + " ink " + L + "/" + R + " px from the edge (min " + CardTextPad + ")");
                    if (R - L != 0 && R - L != 1) F(who + " " + SecD[c[3]] + " not ink-centred L" + L + " R" + R);
                }
                for (int q = 0; q < r.rows.Count; q++)
                {
                    var w = r.rows[q]; int L = w[0] - r.x, R = r.x + CardW - 1 - w[3], gap = w[2] - w[1] - 1;
                    if (L != CardTextPad) F(who + " row " + q + " label inset " + L); if (R != CardTextPad) F(who + " row " + q + " value inset " + R);
                    if (gap < CardDRowGap) F(who + " row " + q + " label/value gap " + gap);
                    if (q > 0 && w[4] - r.rows[q - 1][4] != CardLine) F(who + " row pitch " + (w[4] - r.rows[q - 1][4]));
                }
                foreach (var kv in r.secY) { if (kv.Key == "detail" || kv.Key == "tags") continue; if (secRef.TryGetValue(kv.Key, out int o)) { if (o != kv.Value && kv.Key != "title") F(who + " section " + kv.Key + " +" + kv.Value + " vs +" + o); } else secRef[kv.Key] = kv.Value; }
                if (r.iconD == null) F(who + " icon not drawn by DrawIconD1831");
                else
                {
                    var d = r.iconD; // {dx, dy, dw, dh, sc, mL, mR, mT, mB} card-relative
                    if (d[4] < 1 || d[2] > CardIcon || d[3] > CardIcon) F(who + " icon scale " + d[4] + " size " + d[2] + "x" + d[3]);
                    if (d[2] % d[4] != 0 || d[3] % d[4] != 0) F(who + " icon size not a whole multiple of its scale");
                    if (d[5] < 0 || d[6] < 0 || d[7] < 0 || d[8] < 0) F(who + " icon outside the ring interior");
                    if (d[5] != d[6] || d[7] != d[8]) met.AppendLine("  UNEQUAL-MARGIN " + who + " icon margins L" + d[5] + " R" + d[6] + " T" + d[7] + " B" + d[8]);
                }
                if (r.icon != null)
                {
                    int il = r.icon[0] - r.x, ir = r.x + CardW - (r.icon[0] + CardIcon), off = r.icon[1] - r.y;
                    if (il != ir) F(who + " icon not centred L" + il + " R" + ir);
                    if (iconRef == int.MinValue) iconRef = off; else if (iconRef != off) F(who + " icon y +" + off + " vs +" + iconRef);
                }
                int dotsY = r.cardH - CardPad - CardDot;
                if (r.tagsBottom >= 0 && r.tagsBottom + CardPad != dotsY) F(who + " tags bottom +" + r.tagsBottom + " not " + CardPad + " px above the dots row +" + dotsY);
                int tagsTop = int.MaxValue; foreach (var c in r.centred) if (c[3] == 3) tagsTop = Mathf.Min(tagsTop, c[2] - r.y);
                if (r.lastBodyBottom >= 0 && tagsTop != int.MaxValue && tagsTop - r.lastBodyBottom < CardDTagGap) F(who + " body to tags " + (tagsTop - r.lastBodyBottom) + " px (min " + CardDTagGap + ")");
                if (r.y + r.cardH > HH) F(who + " card bottom " + (r.y + r.cardH) + " off the 360 canvas");
            }
            int foot = _lastFootY1825, cardBottom = CardRowY() + _lastCardH1825;
            met.AppendLine("  footer y=" + foot + " card bottom=" + cardBottom + " gap=" + (foot - cardBottom));
            if (foot - cardBottom != CardPad) F(tag + " footer gap " + (foot - cardBottom) + " (want " + CardPad + ")");
            if (foot + CardGlyph + (mode == Mode.Chest ? CardLine : 0) > HH) F(tag + " footer off the canvas");
            met.AppendLine(fails == 0 ? "  row OK" : "  row fails " + fails);
            return fails;
        }
    }
}
