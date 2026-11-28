using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        /// <summary>t1823: 1920x1080 Play-mode audit of the redone card layout. Real level-up titles, no test captions.</summary>
        System.Collections.IEnumerator CardLayoutPlaytestCo1823()
        {
            const string tag = "[t1823]";
            const string dir = "/workspace/tmp";
            void Log(string m) { Debug.Log(tag + " " + m); }
            string ex = null;
            Application.logMessageReceived += (c, s, typ) =>
            {
                if (typ != LogType.Exception || ex != null) return;
                if ((s != null && s.IndexOf("UnityEditor.Search") >= 0) || (c != null && c.IndexOf("UnityEditor.Search") >= 0)) return;
                ex = c;
            };
            yield return null;
#if UNITY_EDITOR
            try
            {
                var t = System.Type.GetType("Hellord.EditorTools.HellordMenu, Hellord.Editor");
                t?.GetMethod("ForceGameView1920x1080_1808", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            }
            catch (System.Exception e) { Debug.LogWarning("[t1823] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
            Log("screen after GameView force " + Screen.width + "x" + Screen.height);
#endif
            System.IO.Directory.CreateDirectory(dir);
            var T = HellordTuning.I;
            T.testMenuEnabled = false;
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = true;
            T.kitBar1641 = true; T.skillBelt720 = true;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun();
            _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            PreloadStop1675();
            pendingLevels = 0; level = 4; _tmOpen = false; _tp = null; _luH = -2;
            var sb = new StringBuilder();

            // ---- kit row (4)
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.Teeth), level = 1 });
            SetHeroLayers();
            cards.Clear();
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 4 });
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 3 });
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 1 });
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 3 });
            mode = Mode.LevelUp; _luH = -2;
            Log("NkOn=" + NkOn1713);
            for (int i = 0; i < 16; i++) yield return null;
            sb.Append(CardRowMetrics1823("kit"));
            yield return Shot1822(dir + "/cards_v3_kit.png");

            // ---- support row (3): kit off so only Warriors is a target; TO: is the long skill name
            T.kit1618 = false; T.nkKit1713 = false;
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 });
            SetHeroLayers();
            cards.Clear();
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Executioner) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.ExplodeOnKill) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Bleed) });
            mode = Mode.LevelUp; _tp = null; _luH = -2;
            for (int i = 0; i < 16; i++) yield return null;
            sb.Append(CardRowMetrics1823("support"));
            yield return Shot1822(dir + "/cards_v3_support.png");

            // ---- normal level-up row (3)
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.StormRing), level = 1 });
            skills.Add(new SkillInst { def = HData.Skill(SkillId.FrostCleave), level = 2 });
            SetHeroLayers();
            cards.Clear();
            cards.Add(new Card { kind = CardKind.SkillLevel, skillIndex = 0 });
            cards.Add(new Card { kind = CardKind.Stat, stat = System.Array.Find(HData.Stats, st => st.id == StatId.Armor) });
            cards.Add(new Card { kind = CardKind.SkillLevel, skillIndex = 1 });
            mode = Mode.LevelUp; _tp = null; _luH = -2;
            for (int i = 0; i < 16; i++) yield return null;
            sb.Append(CardRowMetrics1823("levelup"));
            yield return Shot1822(dir + "/cards_v3_levelup.png");

            // ---- cramped-support set from the f8d3426 retest: Burn, Executioner, Bleed
            T.kit1618 = false; T.nkKit1713 = false;
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 });
            SetHeroLayers();
            cards.Clear();
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Burn) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Executioner) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Bleed) });
            mode = Mode.LevelUp; _tp = null; _luH = -2;
            for (int i = 0; i < 16; i++) yield return null;
            sb.Append(CardRowMetrics1823("burn_exec_bleed"));
            yield return Shot1822(dir + "/cards_v3_burn_exec_bleed.png");

            // ---- Infernal Boulder, Hurricane, Skeleton Warriors start card, Faster Casting
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 });
            SetHeroLayers();
            cards.Clear();
            cards.Add(new Card { kind = CardKind.NewSkill, skill = HData.Skill(SkillId.RollingMagmaRock) });
            cards.Add(new Card { kind = CardKind.NewSkill, skill = HData.Skill(SkillId.Hurricane) });
            cards.Add(new Card { kind = CardKind.NewSkill, skill = HData.Skill(SkillId.SkeletonWarriors) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.FasterCasting) });
            mode = Mode.LevelUp; _tp = null; _luH = -2;
            for (int i = 0; i < 16; i++) yield return null;
            sb.Append(CardRowMetrics1823("boulder_hurricane_warriors_faster"));
            yield return Shot1822(dir + "/cards_v3_boulder_hurricane_warriors_faster.png");

            System.IO.File.WriteAllText(dir + "/cards_v3_metrics.txt", sb.ToString());
            Log("metrics written " + (dir + "/cards_v3_metrics.txt"));
            foreach (var line in sb.ToString().Split('\n')) if (line.Length > 0) Log(line);

            int z = CamZoom();
            bool camOk = Screen.width == 1920 && Screen.height == 1080 && z == 3 && T.camTargetPx49.x == 640f && T.camTargetPx49.y == 360f;
            bool files = System.IO.File.Exists(dir + "/cards_v3_kit.png") && System.IO.File.Exists(dir + "/cards_v3_support.png") && System.IO.File.Exists(dir + "/cards_v3_levelup.png")
                && System.IO.File.Exists(dir + "/cards_v3_burn_exec_bleed.png") && System.IO.File.Exists(dir + "/cards_v3_boulder_hurricane_warriors_faster.png");
            bool ok = camOk && files && ex == null;
            Log("screen " + Screen.width + "x" + Screen.height + " zoom=" + z + " camTarget=" + T.camTargetPx49);
            if (ex != null) Log("exception " + ex);
            Log(ok ? "PASS" : "FAIL cam=" + camOk + " files=" + files + " ex=" + (ex ?? "none"));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }

        string CardRowMetrics1823(string tag)
        {
            int n = cards.Count; int x0 = CardRowX0(n), rowY = CardRowY(), rowW = CardRowWidth(n);
            int right = HW - x0 - rowW;
            var copies = new CardCopy[n]; var titles = new List<string>[n];
            int maxBody = 0, maxTitle = 1;
            for (int i = 0; i < n; i++)
            {
                copies[i] = new CardCopy(); FillCardDesc(cards[i], copies[i]);
                int bh = copies[i].BodyH(); if (bh > maxBody) maxBody = bh;
                titles[i] = WrapCard(CardNameOf(cards[i])); if (titles[i].Count > maxTitle) maxTitle = titles[i].Count;
            }
            int plateH = CardPlateH(maxTitle), cardH = CardHeight(maxTitle, maxBody);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== " + tag + " ===");
            sb.AppendLine("constants W=" + CardW + " H=" + cardH + " pad=" + CardPad + " textPad=" + CardTextPad + " gap=" + CardGap + " section=" + CardSection + " line=" + CardLine + " glyph=" + CardGlyph + " icon=" + CardIcon + " plate=" + plateH + " inner=" + CardInnerW + " textW=" + CardTextW + " dotsW=" + CardDotsW);
            sb.AppendLine("row n=" + n + " x0=" + x0 + " right=" + right + " marginDiff=" + (x0 - right) + " y=" + rowY + " rowW=" + rowW + " rowEnd=" + (x0 + rowW) + " footY=" + (rowY + cardH + CardPad));
            int iconY = plateH + CardPad, textY0 = iconY + CardIcon + CardPad, footY = cardH - CardPad - CardDot;
            sb.AppendLine("vertical plate=0.." + plateH + " gap=" + CardPad + " icon=" + iconY + ".." + (iconY + CardIcon) + " gap=" + CardPad + " textStart=" + textY0 + " (shared) bodyH=" + maxBody + " gap=" + (footY - (textY0 + maxBody)) + " footer=" + footY + ".." + (footY + CardDot) + " bottomPad=" + (cardH - (footY + CardDot)));
            for (int i = 0; i < n; i++)
            {
                int x = x0 + i * (CardW + CardGap);
                int gapL = i == 0 ? x0 : CardGap, gapR = i == n - 1 ? right : CardGap;
                sb.AppendLine("-- card " + (i + 1) + " '" + CardNameOf(cards[i]) + "' x=" + x + " y=" + rowY + " w=" + CardW + " h=" + cardH + " gapL=" + gapL + " gapR=" + gapR);
                sb.AppendLine("   titleLines=" + titles[i].Count + " descRuns=" + copies[i].LineN + " bodyH=" + copies[i].BodyH());
                int fillTop = 4, fillH = plateH - 5, tblock = CardTextH(Mathf.Max(1, titles[i].Count));
                int above = (fillH - tblock) / 2, below = fillH - tblock - above;
                sb.AppendLine("   titleInPlate above=" + above + " below=" + below + " (should match)");
                for (int k = 0; k < titles[i].Count; k++)
                {
                    string s = titles[i][k]; int tw = TextW(s), lx = CardLineX(x, s);
                    int padL = lx - x, padR = (x + CardW) - (lx + tw);
                    sb.AppendLine("   TITLE " + k + " tw=" + tw + " x=" + lx + " frameL=" + padL + " frameR=" + padR + " textPadWant=" + CardTextPad + " '" + s + "'");
                }
                int ix = BoxCenterX(x, CardW, CardIcon);
                sb.AppendLine("   icon x=" + ix + " y=" + (rowY + iconY) + " s=" + CardIcon + " frameL=" + (ix - x) + " frameR=" + (x + CardW - (ix + CardIcon)) + " centerDiff=" + ((ix - x) - (x + CardW - (ix + CardIcon))));
                bool showNew = (cards[i].kit > 0 && !cards[i].kitLv1651) || cards[i].kind == CardKind.NewSkill;
                if (showNew)
                {
                    int bx = x + CardW - CardPad - CardBadgeW, by = rowY + iconY;
                    sb.AppendLine("   badge x=" + bx + " y=" + by + " frameR=" + (x + CardW - (bx + CardBadgeW)) + " (pad should be " + CardPad + ")");
                }
                int ly = rowY + textY0; int prevBottom = -1;
                for (int k = 0; k < copies[i].runs.Count; k++)
                {
                    var r = copies[i].runs[k];
                    if (r.sectionStart) ly += CardSection;
                    string s = r.text; int tw = TextW(s), lx = CardLineX(x, s);
                    int padL = lx - x, padR = (x + CardW) - (lx + tw);
                    int sec = r.sectionStart && prevBottom >= 0 ? (ly - prevBottom) : (k == 0 ? 0 : CardLine - CardGlyph);
                    sb.AppendLine("   LINE " + k + " tone=" + r.tone + " y=" + ly + " tw=" + tw + " x=" + lx + " frameL=" + padL + " frameR=" + padR + " clearOK=" + (padL >= CardTextPad && padR >= CardTextPad) + " gapFromPrev=" + sec + " '" + s + "'");
                    if (k >= 1 && !r.sectionStart) sb.AppendLine("   lineStep=" + CardLine + " gapBetweenGlyphs=" + (CardLine - CardGlyph) + " (want " + (CardLine - CardGlyph) + ")");
                    if (r.sectionStart) sb.AppendLine("   sectionGap=" + CardSection + " emptyBetweenGlyphs=" + (CardSection + CardLine - CardGlyph));
                    prevBottom = ly + CardGlyph;
                    ly += CardLine;
                }
                bool hasDots = (cards[i].kind == CardKind.NewSkill && cards[i].kit == 0) || cards[i].kind == CardKind.SkillLevel || cards[i].kind == CardKind.Support;
                int dy = rowY + footY; int dx = BoxCenterX(x, CardW, CardDotsW);
                int lastText = rowY + textY0 + copies[i].BodyH();
                if (hasDots) sb.AppendLine("   dots x=" + dx + " y=" + dy + " w=" + CardDotsW + " frameL=" + (dx - x) + " frameR=" + (x + CardW - (dx + CardDotsW)) + " diff=" + ((dx - x) - (x + CardW - (dx + CardDotsW))) + " aboveFromText=" + (dy - lastText) + " bottomPad=" + (rowY + cardH - (dy + CardDot)));
                int nx = x + CardPad, ny = rowY + cardH - CardPad - CardGlyph;
                sb.AppendLine("   index x=" + nx + " y=" + ny + " frameL=" + (nx - x) + " bottomPad=" + (rowY + cardH - (ny + CardGlyph)));
            }
            return sb.ToString();
        }
    }
}
