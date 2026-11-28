using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        // =================================================================================================================
        // t1825: 1920x1080 Play-mode test of the Option A cards. Captures every card screen, measures each drawn card in
        // engine (the draw code records what it really drew) and FAILS on any OCD violation; audits the copy of every
        // skill / kit skill / support / stat at every level; writes /workspace/tmp/cards_A_*.png, cards_A_metrics.txt and
        // cards_A_copy.txt.  -executeMethod Hellord.EditorTools.HellordMenu.RunCardsAPlaytest1825 -cardsAPlaytest
        // =================================================================================================================
        System.Collections.IEnumerator CardsAPlaytestCo1825()
        {
            const string dir = "/workspace/tmp";
            void Log(string m) { Debug.Log("[t1825] " + m); }
            string ex = null;
            Application.logMessageReceived += (c, s, typ) => { if (typ != LogType.Exception || ex != null) return; if ((s != null && s.IndexOf("UnityEditor.Search") >= 0) || (c != null && c.IndexOf("UnityEditor.Search") >= 0)) return; ex = c + " | " + s; };
            yield return null;
#if UNITY_EDITOR
            try { var t = System.Type.GetType("Hellord.EditorTools.HellordMenu, Hellord.Editor"); t?.GetMethod("ForceGameView1920x1080_1808", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null); }
            catch (System.Exception e) { Debug.LogWarning("[t1825] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
#endif
            System.IO.Directory.CreateDirectory(dir);
            var T = HellordTuning.I;
            T.testMenuEnabled = false; T.cardsA1825 = true; T.cardsD1831 = false; /* t1831: D is checked first */
            T.kit1618 = false; T.nkKit1713 = false; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = true; T.kitBar1641 = true; T.skillBelt720 = true;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun(); _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51(); PreloadStop1675();
            pendingLevels = 0; level = 4; _tmOpen = false; _tp = null; _luH = -2; freeRerolls = 0;
            var met = new StringBuilder(); int fails = 0; var shots = new List<string>();

            // ---------------- copy audit of every entry (all levels) + the copy list
            var copyTxt = new StringBuilder(); string longest = null; SkillDef longestDef = null; int longestScore = -1; var longRank = new List<KeyValuePair<int, SkillDef>>();
            fails += CopyAudit1825(copyTxt, met, longRank);
            longRank.Sort((a, b) => b.Key.CompareTo(a.Key));
            if (longRank.Count > 0) { longestDef = longRank[0].Value; longestScore = longRank[0].Key; longest = longestDef.name; }
            System.IO.File.WriteAllText(dir + "/cards_A_copy.txt", copyTxt.ToString());
            Log("copy list written, longest skill card: " + longest + " score " + longestScore);

            // ---------------- rows
            System.Collections.IEnumerator Row(string tag, Mode m)
            {
                mode = m; _luH = -2; warning = null;
                for (int i = 0; i < 14; i++) yield return null;
                _auditA1825 = new List<AuditRec>();
                for (int i = 0; i < 3 && _auditA1825.Count == 0; i++) yield return new WaitForEndOfFrame();
                var recs = _auditA1825; _auditA1825 = null;
                fails += RowAudit1825(tag, recs, met);
                string path = dir + "/cards_A_" + tag + ".png"; shots.Add(path);
                yield return Shot1822(path);
            }
            SkillInst Sk(SkillId id, int lv) => new SkillInst { def = HData.Skill(id), level = lv };
            Card NS(SkillId id) => new Card { kind = CardKind.NewSkill, skill = HData.Skill(id) };
            Card SP(SupportId id) => new Card { kind = CardKind.Support, support = HData.Support(id) };

            // 1 kit row as asked: Infernal Boulder, Hurricane, Skeleton Warriors, Faster Casting (TO: Skeleton Warriors)
            skills.Clear(); skills.Add(Sk(SkillId.SkeletonWarriors, 1)); SetHeroLayers();
            cards.Clear(); cards.Add(NS(SkillId.RollingMagmaRock)); cards.Add(NS(SkillId.Hurricane)); cards.Add(NS(SkillId.SkeletonWarriors)); cards.Add(SP(SupportId.FasterCasting));
            yield return Row("kit", Mode.LevelUp);
            // 2 support row: Burn, Executioner, Bleed
            cards.Clear(); cards.Add(SP(SupportId.Burn)); cards.Add(SP(SupportId.Executioner)); cards.Add(SP(SupportId.Bleed));
            yield return Row("support", Mode.LevelUp);
            // 3 level-up row: upgrades to owned skills (+ a support upgrade on the only open target)
            skills.Clear(); skills.Add(Sk(SkillId.StormRing, 1)); skills.Add(Sk(SkillId.FrostCleave, 2)); skills.Add(Sk(SkillId.SkeletonWarriors, 4));
            skills[0].sup.Add(SupportId.Shock); skills[1].sup.Add(SupportId.Burn); skills[1].sup.Add(SupportId.Chill); skills[2].sup.Add(SupportId.Bleed); skills[2].sup.Add(SupportId.AddedDamage); skills[2].sup.Add(SupportId.FasterCasting); skills[2].sup.Add(SupportId.LifeLeech);
            SetHeroLayers();
            cards.Clear(); cards.Add(new Card { kind = CardKind.SkillLevel, skillIndex = 0 }); cards.Add(new Card { kind = CardKind.SkillLevel, skillIndex = 1 }); cards.Add(new Card { kind = CardKind.SkillLevel, skillIndex = 2 }); cards.Add(SP(SupportId.Burn));
            yield return Row("levelup", Mode.LevelUp);
            // 4 WHICH SKILL? picker: Burn with 3 targets (new on two, upgrade on Frost Cleave)
            skills.Clear(); skills.Add(Sk(SkillId.StormRing, 1)); skills.Add(Sk(SkillId.FrostCleave, 2)); skills.Add(Sk(SkillId.SkeletonWarriors, 1)); skills[1].sup.Add(SupportId.Burn); SetHeroLayers();
            cards.Clear(); cards.Add(SP(SupportId.Burn));
            { var tl = SupportTargets(HData.Support(SupportId.Burn)); Log("picker targets " + tl.Count); OpenTargetPick(HData.Support(SupportId.Burn), tl, false, ix => { }); }
            yield return Row("picker", Mode.LevelUp);
            _tp = null;
            // 5 red NEEDS: More Orbitals and Pierce while only Skeleton Warriors is owned (no Orbital / Projectile), Burn green
            skills.Clear(); skills.Add(Sk(SkillId.SkeletonWarriors, 1)); SetHeroLayers();
            cards.Clear(); cards.Add(SP(SupportId.MoreOrbitals)); cards.Add(SP(SupportId.Pierce)); cards.Add(SP(SupportId.Burn));
            yield return Row("needs_red", Mode.LevelUp);
            // 6 longest copy (computed above) + the next two
            skills.Clear(); skills.Add(Sk(SkillId.Fireball, 1)); SetHeroLayers();
            cards.Clear(); for (int i = 0; i < Mathf.Min(3, longRank.Count); i++) cards.Add(new Card { kind = CardKind.NewSkill, skill = longRank[i].Value });
            met.AppendLine("longest cards: " + string.Join(", ", longRank.GetRange(0, Mathf.Min(3, longRank.Count)).ConvertAll(k => k.Value.name + " (" + k.Key + ")")));
            yield return Row("longest", Mode.LevelUp);
            // 7 start pick screen (StartPick)
            skills.Clear(); SetHeroLayers();
            cards.Clear(); cards.Add(NS(SkillId.Fireball)); cards.Add(NS(SkillId.FrostCleave)); cards.Add(NS(SkillId.StormRing));
            yield return Row("start", Mode.StartPick);
            // 8 chest: Echo (NOT MINION OR KIT), Chain, Executioner on Chain Lightning
            skills.Clear(); skills.Add(Sk(SkillId.ChainLightning, 2)); SetHeroLayers();
            cards.Clear(); cards.Add(SP(SupportId.Echo)); cards.Add(SP(SupportId.Chain)); cards.Add(SP(SupportId.Executioner));
            yield return Row("chest", Mode.Chest);
            // 9 real kit cards (Zhong Kui run): 2 specials + 2 ultimates, then a kit level-up card
            T.kit1618 = true; T.nkKit1713 = true;
            skills.Clear(); skills.Add(Sk(SkillId.Teeth, 1)); SetHeroLayers();
            cards.Clear(); cards.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 4 }); cards.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 3 }); cards.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 1 }); cards.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 3 });
            Log("NkOn=" + NkOn1713);
            yield return Row("kitcards", Mode.LevelUp);
            T.kit1618 = false; T.nkKit1713 = false;

            met.Insert(0, "t1825 Option A card audit  fails=" + fails + "\n");
            System.IO.File.WriteAllText(dir + "/cards_A_metrics.txt", met.ToString());
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

        int FaceChecks1825(string who, CardFaceA f, StringBuilder met)
        {
            int fails = 0; void F(string m) { fails++; met.AppendLine("  FAIL " + who + ": " + m); }
            foreach (var e in f.errs) F("copy error " + e);
            if (f.tagOverflow || f.tags.Count > CardTagMax) F("tag line " + f.tags.Count + " lines (max " + CardTagMax + ")");
            if (f.facts.Count > CardFactMax) F("fact lines " + f.facts.Count + " (max " + CardFactMax + ")");
            foreach (var blk in new[] { f.tags, f.stats, f.facts, f.to }) foreach (var l in blk) { int w = LineW(l); if (w > CardTextW) F("line " + w + " px > " + CardTextW + ": '" + LineStr(l) + "'"); }
            foreach (var l in f.title) if (TextW(l) > CardTextW) F("title " + TextW(l) + " px: '" + l + "'");
            int words = f.Words(); if (words > 30) met.AppendLine("  WARN " + who + ": " + words + " words (aim ~25)");
            return fails;
        }

        int CopyAudit1825(StringBuilder txt, StringBuilder met, List<KeyValuePair<int, SkillDef>> rank)
        {
            int fails = 0; met.AppendLine("== copy audit (every skill / kit skill at Lv1..max, every support Lv1..3, every stat)");
            txt.AppendLine("t1825 Option A card copy - every skill, kit skill, support and stat (resolved from live data at test time).");
            txt.AppendLine("Format: TAGS | stat lines | facts. Skills: new card (Lv1) then the level card stats per level. Supports: Lv1 then Lv2, Lv3 stats.");
            txt.AppendLine();
            string Dump(CardFaceA f) { var sb = new StringBuilder(); sb.Append("  tags : "); foreach (var l in f.tags) sb.Append(LineStr(l) + " / "); sb.AppendLine(); foreach (var l in f.stats) sb.AppendLine("  stat : " + LineStr(l)); foreach (var l in f.facts) sb.AppendLine("  fact : " + LineStr(l)); foreach (var l in f.to) sb.AppendLine("  to   : " + LineStr(l)); return sb.ToString(); }
            txt.AppendLine("==================== SKILLS ===================="); int nSk = 0;
            foreach (var d in HData.Skills)
            {
                if (d == null) continue; nSk++;
                if (!CardCopyA1825.Skill.ContainsKey(d.id)) { fails++; met.AppendLine("  FAIL no copy for skill " + d.id); continue; }
                var f = new CardFaceA { name = d.name, title = WrapCard(d.name) }; SkillBody1825(f, d, null, 1, 0, false, d.kit1651 ? "KIT SKILL" : null);
                fails += FaceChecks1825(d.id + " Lv1", f, met);
                txt.AppendLine(d.name.ToUpperInvariant() + (d.kit1651 ? "  [kit]" : "") + "  (" + f.Words() + " words)"); txt.Append(Dump(f));
                int score = (f.stats.Count + f.facts.Count) * 100; foreach (var l in f.facts) score += LineStr(l).Length; foreach (var l in f.stats) score += LineStr(l).Length;
                if (d.id != SkillId.ZhurongKit) rank.Add(new KeyValuePair<int, SkillDef>(score, d));
                for (int lv = 2; lv <= HData.MaxLv(d); lv++)
                {
                    var inst = new SkillInst { def = d, level = lv - 1 }; var g = new CardFaceA { name = d.name, title = WrapCard(d.name) }; SkillBody1825(g, d, inst, lv, lv - 1, true, null);
                    fails += FaceChecks1825(d.id + " Lv" + lv, g, met);
                    var sb = new StringBuilder(); foreach (var l in g.stats) sb.Append(LineStr(l) + " | "); foreach (var l in g.facts) sb.Append("(" + LineStr(l) + ") ");
                    txt.AppendLine("  Lv" + lv + " card: " + sb);
                }
                txt.AppendLine();
            }
            txt.AppendLine("==================== SUPPORTS ===================="); int nSu = 0;
            foreach (var sd in HData.Supports)
            {
                if (sd == null) continue; nSu++;
                if (!CardCopyA1825.Support.ContainsKey(sd.id)) { fails++; met.AppendLine("  FAIL no copy for support " + sd.id); continue; }
                for (int lv = 1; lv <= SkillInst.MaxSupLevel; lv++)
                {
                    var f = new CardFaceA { name = sd.name, title = WrapCard(sd.name) }; SupportBody1825(f, sd, lv, lv - 1, null, 1, true, lv > 1 ? $"UPGRADE LV {lv - 1} - {lv}" : null, false);
                    fails += FaceChecks1825(sd.id + " Lv" + lv, f, met);
                    if (lv == 1) { txt.AppendLine(sd.name.ToUpperInvariant() + (sd.retired1654 ? "  [retired, never offered]" : "") + "  (" + f.Words() + " words)"); txt.Append(Dump(f)); }
                    else { var sb = new StringBuilder(); foreach (var l in f.stats) sb.Append(LineStr(l) + " | "); txt.AppendLine("  Lv" + lv + " stats: " + sb); }
                }
                txt.AppendLine();
            }
            // Curse Rune echo variant
            { var f = new CardFaceA { name = "Echo" }; var cr = new SkillInst { def = HData.Skill(SkillId.CurseRune), level = 2 }; SupportBody1825(f, HData.Support(SupportId.Echo), 1, 0, cr, 1, true, null, true); fails += FaceChecks1825("Echo TO Curse Rune", f, met); txt.AppendLine("ECHO (TO: CURSE RUNE)"); txt.Append(Dump(f)); txt.AppendLine(); }
            txt.AppendLine("==================== STATS ===================="); int nSt = 0;
            foreach (var st in HData.Stats)
            {
                nSt++; if (!CardCopyA1825.Stat.ContainsKey(st.id)) { fails++; met.AppendLine("  FAIL no copy for stat " + st.id); continue; }
                var f = BuildFace1825(new Card { kind = CardKind.Stat, stat = st }); fails += FaceChecks1825("stat " + st.id, f, met); txt.AppendLine(st.name.ToUpperInvariant()); txt.Append(Dump(f)); txt.AppendLine();
            }
            { var slot = new StatDef { id = StatId.Slot, name = "New Skill Slot" }; var f = BuildFace1825(new Card { kind = CardKind.Stat, stat = slot }); fails += FaceChecks1825("stat Slot", f, met); txt.AppendLine("NEW SKILL SLOT"); txt.Append(Dump(f)); }
            met.AppendLine("  entries: skills " + nSk + " (copy " + CardCopyA1825.Skill.Count + "), supports " + nSu + " (copy " + CardCopyA1825.Support.Count + "), stats " + nSt + "+slot, fails " + fails);
            return fails;
        }

        int RowAudit1825(string tag, List<AuditRec> recs, StringBuilder met)
        {
            int fails = 0; void F(string m) { fails++; met.AppendLine("  FAIL " + m); }
            met.AppendLine("== row " + tag + " (" + (recs?.Count ?? 0) + " cards)");
            if (recs == null || recs.Count == 0) { F("nothing drawn"); return fails; }
            int n = recs.Count, rowW = n * CardW + (n - 1) * CardGap, x0 = recs[0].x, right = HW - (recs[n - 1].x + CardW);
            met.AppendLine("  row x0=" + x0 + " right=" + right + " w=" + rowW + " cardH=" + recs[0].cardH + " rowY=" + CardRowY());
            if (Mathf.Abs(x0 - right) > 1) F(tag + " row margins L" + x0 + " R" + right);
            for (int i = 1; i < n; i++) if (recs[i].x - (recs[i - 1].x + CardW) != CardGap) F(tag + " gap " + i + " = " + (recs[i].x - (recs[i - 1].x + CardW)));
            int topMin = int.MaxValue; foreach (var r in recs) topMin = Mathf.Min(topMin, r.y);
            int baseY = CardRowY();
            var secRef = new Dictionary<string, int>(); int iconRef = int.MinValue, dotsRef = int.MinValue;
            for (int i = 0; i < n; i++)
            {
                var r = recs[i]; string who = tag + "#" + (i + 1);
                met.AppendLine(" card " + (i + 1) + " x=" + r.x + " y=" + r.y + (r.y != baseY ? " (hover/slide " + (r.y - baseY) + ")" : "") + " h=" + r.cardH);
                foreach (var row in r.rows) met.AppendLine("   " + row);
                foreach (var l in r.lines)
                {
                    int x = l[0], w = l[2], cx = l[3], L = x - cx, R = cx + CardW - x - w;
                    if (w > CardTextW) F(who + " line wider than " + CardTextW + " (" + w + ")");
                    if (L < CardTextPad || R < CardTextPad) F(who + " text " + L + "/" + R + " px from the edge (min " + CardTextPad + ")");
                    if (R - L != 0 && R - L != 1) F(who + " line not centred L" + L + " R" + R);
                }
                foreach (var kv in r.secY) { int off = kv.Value - r.y; if (secRef.TryGetValue(kv.Key, out int o)) { if (o != off) F(who + " section " + kv.Key + " starts at +" + off + ", row uses +" + o); } else secRef[kv.Key] = off; }
                if (r.icon != null)
                {
                    int il = r.icon[0] - r.x, ir = r.x + CardW - (r.icon[0] + CardIcon), off = r.icon[1] - r.y;
                    met.AppendLine("   icon x=" + r.icon[0] + " L" + il + " R" + ir + " y+" + off);
                    if (il != ir) F(who + " icon not centred L" + il + " R" + ir);
                    if (iconRef == int.MinValue) iconRef = off; else if (iconRef != off) F(who + " icon y +" + off + " vs +" + iconRef);
                }
                if (r.rule != null) { int rl = r.rule[0] - r.x, rr = r.x + CardW - (r.rule[0] + CardRuleW); met.AppendLine("   rule y+" + (r.rule[1] - r.y) + " L" + rl + " R" + rr); if (rl != rr) F(who + " rule not centred"); }
                if (r.toRule != null) { int rl = r.toRule[0] - r.x, rr = r.x + CardW - (r.toRule[0] + CardRuleW); met.AppendLine("   to-rule y+" + (r.toRule[1] - r.y) + " L" + rl + " R" + rr); if (rl != rr) F(who + " to-rule not centred"); }
                if (r.dots != null)
                {
                    int dl = r.dots[0] - r.x, dr = r.x + CardW - (r.dots[0] + CardDotsW), off = r.dots[1] - r.y, bp = r.cardH - (off + CardDot);
                    met.AppendLine("   dots L" + dl + " R" + dr + " y+" + off + " bottomPad=" + bp);
                    if (Mathf.Abs(dl - dr) > 1) F(who + " dots not centred"); if (bp != CardPad) F(who + " dots bottom pad " + bp);
                    if (dotsRef == int.MinValue) dotsRef = off; else if (dotsRef != off) F(who + " dots y");
                    int lastBottom = 0; foreach (var l in r.lines) lastBottom = Mathf.Max(lastBottom, l[1] - r.y + CardGlyph);
                    if (off - lastBottom < CardPad) F(who + " text " + (off - lastBottom) + " px above the dots (min " + CardPad + ")");
                }
                if (r.y + r.cardH > HH) F(who + " card bottom " + (r.y + r.cardH) + " off the 360 canvas");
            }
            // section geometry from the shared layout: gaps around the rules = 6, section gaps = 8
            met.AppendLine("  sections (y from card top): " + string.Join(" ", new List<string>(secRef.Keys).ConvertAll(k => k + "+" + secRef[k])) + " icon+" + iconRef + " dots+" + dotsRef);
            int foot = _lastFootY1825, cardBottom = CardRowY() + _lastCardH1825;
            met.AppendLine("  footer y=" + foot + " card bottom=" + cardBottom + " gap=" + (foot - cardBottom) + " footer bottom=" + (foot + CardGlyph) + (mode == Mode.Chest ? " chest line bottom=" + (foot + CardLine + CardGlyph) : ""));
            if (foot - cardBottom != CardPad) F(tag + " footer gap " + (foot - cardBottom) + " (want " + CardPad + ")");
            if (foot + CardGlyph + (mode == Mode.Chest ? CardLine : 0) > HH) F(tag + " footer off the canvas");
            met.AppendLine(fails == 0 ? "  row OK" : "  row fails " + fails);
            return fails;
        }
    }
}
