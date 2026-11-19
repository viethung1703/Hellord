#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1561 altar flow + Gonggong ("altar1561"). Save: all 5 pacts unlocked, altar Path = Gonggong with 3 picks (N1..N3), NOT sealed.
    /// Recorded pass (rec_altar1561 + audio_events.txt): main menu -> Act69(0) = DESCEND -> Sacrifice Altar (Gonggong in the centre) -> fake click DESCEND (nothing sealed:
    /// notice, no run) -> fake click SEAL -> fake click DESCEND -> run starts in the old start pick (Frostbolt, Frost Cleave) -> Frostbolt (Choose = key 1 / click) -> LMB at imps.
    /// Then unrecorded rules: seal alone never starts a run, BACK -> main menu, every sealed pact's filtered pick (Zhurong 3 fire, Zhong Kui, Xingtian, Leizu fallback),
    /// nodes apply, unsealed = no run, toggles (altarFlow1561 off -> 8c2cd2a picker, pactGonggong1561 off -> 4 pacts). Stills of the altar and every in-game pick.</summary>
    public partial class HellordGame
    {
        int _r1561;
        IEnumerator AtHold1561(float s, string clip) { float t = 0; while (t < s) { yield return null; t += Time.unscaledDeltaTime; if (clip != null) { AtAudioLog1481.Frame = _r1561; AtRecFrame(clip, ref _r1561); } } }
        IEnumerator AtFake1561(int x, int y, string clip)
        {
            _mtFakeUsed = false; MenuThemeFake = new Vector2(x, y); float w = 0;
            while (MenuThemeFake.HasValue && !_mtFakeUsed && w < 1f) { yield return null; w += Time.unscaledDeltaTime; if (clip != null) { AtAudioLog1481.Frame = _r1561; AtRecFrame(clip, ref _r1561); } }
            MenuThemeFake = null;
        }
        Vector2Int AtPlayC1561() => new Vector2Int(AltarBtnX1561(1) + PT.altarPlayW1561 / 2, AltarMenuData.BtnY + 9);
        Vector2Int AtSealC1561() => new Vector2Int(AltarMenuData.SealX + 44, (PT.pactCardsV2 ? AmV2SealY() : AltarMenuData.SealY) + 7);
        /// <summary>seal p (switching Path clears picks, like the altar confirm) and press the altar DESCEND (same function as the button).</summary>
        void AtSealPlay1561(int p, int sel) { AtMenu1555(); _pactPath51 = p; _pactSel51 = sel; _pactSealed51 = true; Act69(0); AltarPlay1561(); }

        IEnumerator AtAltar1561()
        {
            yield return AtBegin("altar1561"); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51(); var T = HellordTuning.I;
            int so = _souls51, si = _sigils51, pp = _pactPath51, ps = _pactSel51; bool sd = _pactSealed51; var un = (int[])_unlock51.Clone();
            bool five = NP97 == 5 && PactData.PathShort[4] == "GONGGONG" && PactData.PathTitle[4] == "BREAKER OF HEAVEN";
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 4; _pactSel51 = 0x7; _pactSealed51 = false; PactSave51();
            _atGod = true; Manual716 = false;
            AtMenu1555(); yield return AtWait(1.0f);
            // ---------------- recorded pass
            const string clip = "altar1561"; _r1561 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1561(0.9f, clip);
            Act69(0); bool altar = mode == Mode.Altar && _altarScr53 == 0 && _altarView51 == 4 && _df1555 == 0; yield return AtHold1561(1.4f, clip);
            yield return AtShot("altar1561_altar_gonggong_unsealed");
            var pc = AtPlayC1561(); yield return AtFake1561(pc.x, pc.y, clip);
            bool notice = mode == Mode.Altar && _altarMsg51 == PT.altarNoSealMsg1561 && !_pactSealed51; yield return AtHold1561(0.5f, clip); yield return AtShot("altar1561_notice"); yield return AtHold1561(0.9f, clip);
            var sc = AtSealC1561(); yield return AtFake1561(sc.x, sc.y, clip);
            bool sealedNoRun = _pactSealed51 && _pactPath51 == 4 && mode == Mode.Altar; yield return AtHold1561(1.3f, clip); yield return AtShot("altar1561_altar_gonggong_sealed");
            string gB = AltarBtnGaps1561;
            yield return AtFake1561(pc.x, pc.y, clip);
            bool inPick = mode == Mode.StartPick; string ids = AtCardIds1559(), gG = AtOldPickGaps1559(); bool frost = ids == "Frostbolt,FrostCleave";
            float inc = incDmg; bool nodes = _pactActive51 && _pactPath51 == 4 && SelCount51() == 3;
            yield return AtHold1561(1.6f, clip); yield return AtShot("altar1561_ingame_pick_gonggong");
            if (cards.Count > 0) Choose(cards[0]);
            bool started = mode == Mode.Play && skills.Count == 1 && skills[0].def.id == SkillId.Frostbolt; var basic = Basic1517();
            maxHp = hp = 100000; SnapCamera(); var list = new List<Enemy>();
            void Mk(float dx, float dy) { var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed *= 0.15f; e.hp = e.maxHp = 80; list.Add(e); } }
            Mk(4.5f, 0.4f); Mk(5.6f, -1.0f); Mk(6.4f, 1.1f); Mk(-4.8f, 0.6f); Mk(-5.8f, -0.9f);
            int casts = 0; float lastCd = 0; var sk = skills.Count > 0 ? skills[0] : null;
            for (int f = 0; f < 150; f++)
            {
                yield return null; AtFrameChecks(); if (mode != Mode.Play) mode = Mode.Play; hp = maxHp;
                bool right = f < 75; atMouse986 = _heroPos + new Vector2(right ? 5.2f : -5.2f, right ? 0.2f : 0.0f); HInput.AtLmb = f >= 10 && f < 66 || f >= 90 && f < 140;
                if (sk != null && sk.cd > lastCd + 0.05f) casts++; if (sk != null) lastCd = sk.cd;
                AtAudioLog1481.Frame = _r1561; AtRecFrame(clip, ref _r1561);
            }
            HInput.AtLmb = false; yield return AtShot("altar1561_run");
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            ARes("altar1561:clip_flow", five && altar && notice && sealedNoRun && inPick && frost && nodes && started && basic != null && casts >= 2,
                $"fivePacts {five} descendOpensAltar {altar} unsealedNotice {notice} '{_altarMsg51}' sealAloneNoRun {sealedNoRun} runStartsInOldPick {inPick} cards [{ids}] frostOnly {frost} gonggongNodesApply {nodes} incDmg {inc:0.###} started {started} lmbBasic {(basic != null ? basic.def.id.ToString() : "none")} casts {casts} frames {_r1561}");
            AtClearWorld();

            // ---------------- rules + stills (unrecorded)
            AtMenu1555(); _pactSealed51 = false; Act69(0); AltarPlay1561(); bool noSealNoRun = mode == Mode.Altar;
            AmDo("amback"); bool back = mode == Mode.MainMenu;
            AtSealPlay1561(0, 0x7); yield return AtWait(0.6f); yield return AtShot("altar1561_ingame_pick_zhurong"); string idsZ = AtCardIds1559(), gZ = AtOldPickGaps1559(); bool zNodes = _pactActive51 && SelCount51() == 3; Choose(cards[0]);
            AtSealPlay1561(1, 0); yield return AtWait(0.6f); yield return AtShot("altar1561_ingame_pick_zhongkui"); string idsZk = AtCardIds1559(), gZk = AtOldPickGaps1559(); bool zkEmptyNoNodes = !_pactActive51; Choose(cards[0]);
            AtSealPlay1561(3, 0x3); yield return AtWait(0.6f); yield return AtShot("altar1561_ingame_pick_xingtian"); string idsXt = AtCardIds1559(), gXt = AtOldPickGaps1559(); bool xtNodes = _pactActive51 && SelCount51() == 2; Choose(cards[0]);
            AtSealPlay1561(2, 0x1); yield return AtWait(0.6f); yield return AtShot("altar1561_ingame_pick_leizu_fallback"); string idsL = AtCardIds1559(); bool leizu = mode == Mode.StartPick && cards.Count > 0 && _pactActive51; if (cards.Count > 0) Choose(cards[0]);
            AtMenu1555(); bool cleared = _dfRunPick1559 < 0 && !_altarRun1561;
            // altar stills: Gonggong in the centre (sealed), the 5 carousel positions, the Gonggong tree
            AtMenu1555(); _pactPath51 = 4; _pactSel51 = 0x7; _pactSealed51 = true; Act69(0); yield return AtWait(0.6f); yield return AtShot("altar1561_altar_measure");
            for (int v = 0; v < NP97; v++) { _altarView51 = v; _slide97 = 0; yield return AtWait(0.35f); yield return AtShot("altar1561_view" + v); }
            _altarView51 = 4; _altarScr53 = 1; _focus97 = -1; yield return AtWait(0.6f); yield return AtShot("altar1561_gonggong_tree"); _altarScr53 = 0;
            AtMenu1555();
            // toggles
            _pactPath51 = 0; _pactSealed51 = true; T.altarFlow1561 = false; T.pactGonggong1561 = false; Act69(0); bool picker = _df1555 == 1 && NP97 == 4; DfBack1555(); T.pactGonggong1561 = true;
            AtMenu1555(); Act69(0); yield return null; bool offNoPicker5 = mode == Mode.StartPick && _df1555 == 0; T.altarFlow1561 = true;
            AtMenu1555(); yield return AtWait(0.2f);
            ARes("altar1561:rules", noSealNoRun && back && idsZ == "Fireball,RollingMagmaRock,FirePillar" && zNodes && idsZk == "SoulSpears" && zkEmptyNoNodes && idsXt == "BloodWhip" && xtNodes && leizu && cleared && picker,
                $"noSealNoRun {noSealNoRun} backToMenu {back} zhurong [{idsZ}] nodes {zNodes} zhongKui [{idsZk}] emptySealNoNodes {zkEmptyNoNodes} xingtian [{idsXt}] nodes {xtNodes} leizuSealedFallback {leizu} [{idsL}] filterCleared {cleared} altarFlowOff+4pacts=picker {picker} altarFlowOff+5pacts=oldDescend {offNoPicker5}");
            ALog("buttons " + gB); ALog("oldpick gonggong " + gG); ALog("oldpick zhurong " + gZ); ALog("oldpick zhongkui " + gZk); ALog("oldpick xingtian " + gXt);
            ARes("altar1561:gaps", gB == "left 141 | open 105 | 21 | descend 106 | 21 | back 105 | right 141" && gG.Contains("left 200 gap 8 right 200") && gZ.Contains("left 138 gap 8 right 138") && gZk.Contains("left 262") && gXt.Contains("right 262"),
                gB + " || GG " + gG + " || Z " + gZ + " || ZK " + gZk + " || XT " + gXt);
            _souls51 = so; _sigils51 = si; _pactPath51 = pp; _pactSel51 = ps; _pactSealed51 = sd; _unlock51 = un; PactSave51(); _atGod = false;
            AtEnd("altar1561", "hero_sprite_not_base_final2");
        }
    }
}
#endif
