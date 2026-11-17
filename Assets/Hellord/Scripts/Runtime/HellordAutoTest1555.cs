#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1555 descend flow ("descend1555"). Save: Zhurong / Zhong Kui / Leizu unlocked, Xingtian locked, Zhong Kui SEALED (3 nodes).
    /// Recorded pass (rec_descend1555 + audio_events.txt): main menu (DESCEND row) -> Act69(0) = DESCEND -> pact picker (Zhong Kui preselected = sealed) ->
    /// fake click Zhurong (select) -> fake click CHOOSE -> skill picker (Fireball / Infernal Boulder / Fire Pillar) -> fake click Infernal Boulder -> fake click DESCEND ->
    /// the run starts, LMB held at imps (Infernal Boulder fired). Then unrecorded checks: keyboard moves (DfMove1555) skip Leizu (no basic) and locked Xingtian,
    /// Back on both steps, the sealed god keeps its nodes, Zhong Kui / Xingtian single-card pickers, the fresh-save fallback, flow off = old DESCEND. Stills of every screen.</summary>
    public partial class HellordGame
    {
        int _r1555;
        IEnumerator AtFakeClick1555(int x, int y, string clip)
        {
            _mtFakeUsed = false; MenuThemeFake = new Vector2(x, y); float w = 0;
            while (MenuThemeFake.HasValue && !_mtFakeUsed && w < 1f) { yield return null; w += Time.unscaledDeltaTime; if (clip != null) { AtAudioLog1481.Frame = _r1555; AtRecFrame(clip, ref _r1555); } }
            MenuThemeFake = null;
        }
        IEnumerator AtHold1555(float s, string clip) { float t = 0; while (t < s) { yield return null; t += Time.unscaledDeltaTime; if (clip != null) { AtAudioLog1481.Frame = _r1555; AtRecFrame(clip, ref _r1555); } } }
        void AtMenu1555() { ResetRun(); PactRunReset51(); _df1555 = 0; mode = Mode.MainMenu; _sel69 = _row69 = 0; _panel69 = 0; _press69 = -1; }
        /// <summary>centre of card i of n in the current step (same layout code as the draw).</summary>
        Vector2Int AtCardC1555(int i, int n, bool pact)
        {
            int x0, g, ml, mr, cy, by, gv, rem; DfHorz1555(n, out x0, out g, out ml, out mr); int h = pact ? DfPactH1555() : DfSkillH1555(); DfVert1555(h, out cy, out by, out gv, out rem);
            return new Vector2Int(x0 + i * (PT.descendCardW1555 + g) + PT.descendCardW1555 / 2, cy + 60);
        }
        int AtBtnY1555() { int cy, by, g, rem; DfVert1555(_df1555 == 1 ? DfPactH1555() : DfSkillH1555(), out cy, out by, out g, out rem); return by + 9; }

        /// <summary>old in-game start pick card rects (DrawCardsB layout: CardW 116, gap 8 for up to 3 cards / 6 above, row centred in 640, y 80): left margin, gap, right margin.</summary>
        string AtOldPickGaps1559() { int n = cards.Count; int gap = n > 3 ? 6 : 8; float x0 = Mathf.Round((640 - (n * CardW + (n - 1) * gap)) / 2f); int r = 640 - (int)x0 - (n * CardW + (n - 1) * gap); return $"n {n} cards {CardW}x{CardH} left {x0} gap {gap} right {r} top 80"; }
        string AtCardIds1559() { var l = new List<string>(); foreach (var c in cards) l.Add(c.skill != null ? c.skill.id.ToString() : c.kind.ToString()); return string.Join(",", l); }

        IEnumerator AtDescend1555()
        {
            yield return AtBegin("descend1555"); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51(); var T = HellordTuning.I;
            int so = _souls51, si = _sigils51, pp = _pactPath51, ps = _pactSel51; bool sd = _pactSealed51; var un = (int[])_unlock51.Clone();
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1; _unlock51[3] = 0;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 0; _pactSel51 = 0x7; _pactSealed51 = false; PactSave51();   // altar Path = Zhurong, 3 picks, NOT sealed
            _atGod = true; Manual716 = false;
            AtMenu1555(); yield return AtWait(1.0f);
            // ---------------- recorded pass: DESCEND -> pick Zhurong -> run starts -> old start pick (fire basics only) -> Infernal Boulder -> fire
            const string clip = "descend1555"; _r1555 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1555(1.0f, clip);
            Act69(0); bool opened = _df1555 == 1; yield return AtHold1555(1.0f, clip);
            bool selZ = _dfSel1555 == 0;                                                              // Zhurong preselected (your altar Path; nothing sealed)
            yield return AtHold1555(0.6f, clip);
            yield return AtFakeClick1555(PT.descendOkX1555 + 52, AtBtnY1555(), clip);
            bool inPick = mode == Mode.StartPick && _df1555 == 0; string ids = AtCardIds1559(), gZ = AtOldPickGaps1559(); bool fireOnly = ids == "Fireball,RollingMagmaRock,FirePillar";
            bool nodesZ = _pactActive51 && !_pactSealed51 && _pactPath51 == 0 && _pactSel51 == 0x7;          // unsealed altar Path: its picks apply, seal unchanged
            yield return AtHold1555(1.6f, clip); yield return AtShot("descend1559_ingame_pick_zhurong");
            if (cards.Count > 1) Choose(cards[1]);                                                   // = key 2 / click on card 2 (the same Choose)
            bool started = mode == Mode.Play && skills.Count == 1 && skills[0].def.id == SkillId.RollingMagmaRock; var basic = Basic1517();
            maxHp = hp = 100000; SnapCamera(); var list = new List<Enemy>();
            void Mk(float dx, float dy) { var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed *= 0.15f; e.hp = e.maxHp = 80; list.Add(e); } }
            Mk(4.5f, 0.4f); Mk(5.6f, -1.0f); Mk(6.4f, 1.1f); Mk(-4.8f, 0.6f); Mk(-5.8f, -0.9f);
            int casts = 0; float lastCd = 0; var sk = skills.Count > 0 ? skills[0] : null;
            for (int f = 0; f < 150; f++)
            {
                yield return null; AtFrameChecks(); if (mode != Mode.Play) mode = Mode.Play; hp = maxHp;
                bool right = f < 75; atMouse986 = _heroPos + new Vector2(right ? 5.2f : -5.2f, right ? 0.2f : 0.0f); HInput.AtLmb = f >= 10 && f < 66 || f >= 90 && f < 140;
                if (sk != null && sk.cd > lastCd + 0.05f) casts++; if (sk != null) lastCd = sk.cd;
                AtAudioLog1481.Frame = _r1555; AtRecFrame(clip, ref _r1555);
            }
            HInput.AtLmb = false; yield return AtShot("descend1559_run");
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            ARes("descend1555:clip_flow", opened && selZ && inPick && fireOnly && nodesZ && started && basic != null && casts >= 2,
                $"opened {opened} zhurongPreselected {selZ} runStartsInOldPick {inPick} cards [{ids}] fireOnly {fireOnly} zhurongNodesApply(unsealed) {nodesZ} nodes {SelCount51()} started {started} skills0 {(skills.Count > 0 ? skills[0].def.id.ToString() : "-")} lmbBasic {(basic != null ? basic.def.id.ToString() : "none")} casts {casts} frames {_r1555}");
            AtClearWorld();

            // ---------------- stills + rules (unrecorded)
            AtMenu1555(); yield return AtWait(0.3f); Act69(0); yield return AtWait(0.6f); yield return AtShot("descend1559_pact_picker"); string gP = DescendGaps1555;
            bool leizuNo = !DfPickable1555(2), xingLocked = !DfPickable1555(3), noStep2 = true;
            _dfSel1555 = 0; DfMove1555(1); bool skipR = _dfSel1555 == 1; DfMove1555(1); bool skipR2 = _dfSel1555 == 0;   // 0 -> 1 -> (2 no basic, 3 locked) -> 0
            DfBack1555(); bool back0 = _df1555 == 0 && mode == Mode.MainMenu;
            // another god (Zhong Kui): no stored picks -> no nodes; 1 card; altar save untouched
            AtMenu1555(); Act69(0); yield return AtWait(0.2f); _dfSel1555 = 1; DfConfirm1555(); noStep2 = _df1555 == 0; yield return AtWait(0.8f); yield return AtShot("descend1559_ingame_pick_zhongkui");
            string idsZk = AtCardIds1559(), gZk = AtOldPickGaps1559(); bool zkNoNodes = !_pactActive51 && _pactPath51 == 0 && _pactSel51 == 0x7 && !_pactSealed51;
            Choose(cards[0]); bool zk = mode == Mode.Play && skills.Count == 1 && skills[0].def.id == SkillId.SoulSpears;
            // Xingtian (unlocked): 1 card Blood Whip
            AtMenu1555(); _unlock51[3] = 1; Act69(0); yield return AtWait(0.2f); _dfSel1555 = 3; DfConfirm1555(); yield return AtWait(0.8f); yield return AtShot("descend1559_ingame_pick_xingtian");
            string idsXt = AtCardIds1559(), gXt = AtOldPickGaps1559(); Choose(cards[0]); bool xt = mode == Mode.Play && skills.Count == 1 && skills[0].def.id == SkillId.BloodWhip;
            // sealed altar Path: nodes apply (as before); Path with 0 picks: no nodes
            AtMenu1555(); _pactSealed51 = true; Act69(0); yield return AtWait(0.2f); _dfSel1555 = 0; DfConfirm1555(); bool sealedNodes = _pactActive51 && _pactSealed51; Choose(cards[0]);
            AtMenu1555(); _pactSealed51 = false; int keep = _pactSel51; _pactSel51 = 0; Act69(0); yield return AtWait(0.2f); _dfSel1555 = 0; DfConfirm1555(); bool zeroPicks = !_pactActive51 && cards.Count == 3; Choose(cards[0]); _pactSel51 = keep;
            // restart after death clears the filter (old random pick if no pact menu)
            AtMenu1555(); bool cleared = _dfRunPick1559 < 0;
            // 5316082 menu skill screen still behind its toggle
            T.descendSkillScreen1555 = true; Act69(0); yield return AtWait(0.2f); _dfSel1555 = 0; DfConfirm1555(); bool oldScreen = _df1555 == 2; DfBack1555(); DfBack1555(); T.descendSkillScreen1555 = false;
            // fresh save fallback + flow off
            AtMenu1555(); for (int p = 0; p < NP97; p++) _unlock51[p] = 0; int pp2 = _pactPath51, ps2 = _pactSel51; _pactPath51 = -1; _pactSel51 = 0; _pactSealed51 = false;
            Act69(0); yield return AtWait(0.6f); yield return AtShot("descend1559_pact_fresh_save"); bool fresh = DfPickable1555(0) && DfPickable1555(1) && !DfPickable1555(2) && DfPickable1555(3); DfBack1555();
            AtMenu1555(); T.descendFlow1555 = false; Act69(0); yield return null; bool old = mode == Mode.StartPick && _df1555 == 0 && cards.Count == 5; T.descendFlow1555 = true;
            AtMenu1555(); yield return AtWait(0.2f);
            ARes("descend1555:rules", leizuNo && xingLocked && noStep2 && skipR && skipR2 && back0 && idsZk == "SoulSpears" && zkNoNodes && zk && idsXt == "BloodWhip" && xt && sealedNodes && zeroPicks && cleared && oldScreen && fresh && old,
                $"leizuNoBasic {leizuNo} xingtianLocked {xingLocked} noMenuSkillScreen {noStep2} keyMoves {skipR}/{skipR2} backToMenu {back0} zhongKuiCards [{idsZk}] otherGodNoNodes+saveKept {zkNoNodes} zkRun {zk} xingtianCards [{idsXt}] xtRun {xt} sealedNodes {sealedNodes} zeroPicksNoNodes {zeroPicks} filterClearedOnReset {cleared} skillScreenToggle {oldScreen} freshSave {fresh} flowOffOld5Cards {old}");
            ALog("gaps " + gP); ALog("oldpick zhurong " + gZ); ALog("oldpick zhongkui " + gZk); ALog("oldpick xingtian " + gXt);
            ARes("descend1555:gaps", gP.Contains("gapX 13 frameL 13 frameR 13") && gP.Contains("top 39 mid 39 bottom 39") && gZ.Contains("left 138 gap 8 right 138") && gZk.Contains("left 262") && gZk.Contains("right 262"), gP + " || Z " + gZ + " || ZK " + gZk + " || XT " + gXt);
            _souls51 = so; _sigils51 = si; _pactPath51 = pp; _pactSel51 = ps; _pactSealed51 = sd; _unlock51 = un; PactSave51(); _atGod = false;
            AtEnd("descend1555", "hero_sprite_not_base_final2"); // the t1517 cast pose replaces the base hero sprite while the basic fires (as in lmb1517)
        }
    }
}
#endif
