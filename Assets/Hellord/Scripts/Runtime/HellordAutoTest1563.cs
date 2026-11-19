#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1563 Chain Lightning LMB basic ("chain1563"). Save: Leizu SEALED (1 node). Recorded pass (rec_chain1563): main menu -> DESCEND -> altar opens on sealed Leizu ->
    /// fake click DESCEND -> old start pick shows only Chain Lightning -> pick it -> a group of imps on the right: idle (no LMB, no casts), aimed clicks chaining through the
    /// group, one click aimed left at nothing (miss bolt), a click spam during the cooldown (not-ready), more aimed clicks. Checks: casts only on click, cooldown, chain >= 2, miss bolt.</summary>
    public partial class HellordGame
    {
        int _r1563;
        IEnumerator AtChain1563()
        {
            yield return AtBegin("chain1563"); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51(); var T = HellordTuning.I;
            int so = _souls51, si = _sigils51, pp = _pactPath51, ps = _pactSel51; bool sd = _pactSealed51; var un = (int[])_unlock51.Clone();
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 2; _pactSel51 = 0x1; _pactSealed51 = true; PactSave51(); _altarView51 = 0;
            _atGod = true; Manual716 = false;
            AtMenu1555(); yield return AtWait(1.0f);
            const string clip = "chain1563"; _r1561 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1561(0.7f, clip);
            Act69(0); bool altarLeizu = mode == Mode.Altar && _altarView51 == 2; yield return AtHold1561(1.5f, clip); yield return AtShot("chain1563_altar_opened_on_leizu");
            var pc = AtPlayC1561(); yield return AtFake1561(pc.x, pc.y, clip);
            string ids = AtCardIds1559(), gL = AtOldPickGaps1559(); bool pickCl = mode == Mode.StartPick && ids == "ChainLightning";
            yield return AtHold1561(1.4f, clip); yield return AtShot("chain1563_ingame_pick_leizu");
            if (cards.Count > 0) Choose(cards[0]);
            var sk = skills.Count > 0 ? skills[0] : null; bool basic = Basic1517() != null && Basic1517().def.id == SkillId.ChainLightning;
            maxHp = hp = 100000; SnapCamera(); var list = new List<Enemy>();
            void Mk(float dx, float dy) { var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed *= 0.05f; e.hp = e.maxHp = 2000; list.Add(e); } }
            Mk(4.2f, 0.2f); Mk(5.4f, 1.2f); Mk(5.6f, -0.9f); Mk(6.8f, 0.4f); Mk(7.6f, -0.6f); Mk(6.6f, 1.9f);
            int c0 = ChainLmbCasts1563, m0 = ChainLmbMisses1563, maxHits = 0, idleCasts = 0, castsR = 0, notReady = 0; float cdAfter = 0f; bool missSeen = false;
            // frame plan (at 30 fps): 0-44 idle (no LMB) | clicks (4 frames each) at 50, 95, 140 aimed right | 185 aimed LEFT (miss) | 200,206,212 spam during cooldown | 245, 290 aimed right
            int[] clk = { 50, 95, 140, 185, 200, 206, 212, 245, 290 };
            for (int f = 0; f < 330; f++)
            {
                yield return null; AtFrameChecks(); if (mode != Mode.Play) mode = Mode.Play; hp = maxHp;
                bool left = f >= 180 && f < 240; atMouse986 = _heroPos + (left ? new Vector2(-6f, 0.3f) : new Vector2(5.6f, 0.3f));
                bool down = false; foreach (var c in clk) if (f >= c && f < c + 3) down = true; HInput.AtLmb = down;
                int before = ChainLmbCasts1563; // cast happens in Update after this frame's input
                if (f == 44) idleCasts = ChainLmbCasts1563 - c0;
                if (ChainLmbLastHits1563 > maxHits) maxHits = ChainLmbLastHits1563;
                if (ChainLmbMisses1563 > m0) missSeen = true;
                if (sk != null && cdAfter <= 0f && ChainLmbCasts1563 > c0) cdAfter = sk.cd;
                AtAudioLog1481.Frame = _r1561; AtRecFrame(clip, ref _r1561);
            }
            HInput.AtLmb = false; yield return AtShot("chain1563_run");
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            int casts = ChainLmbCasts1563 - c0, misses = ChainLmbMisses1563 - m0; float cdBase = sk != null ? sk.def.cooldown * sk.CdMul * T.chainLmbCdMul1563 : 0f;
            ARes("chain1563:clip_flow", altarLeizu && pickCl && basic && idleCasts == 0 && casts >= 5 && casts <= 7 && maxHits >= 2 && missSeen && misses >= 1 && cdAfter > 0.5f,
                $"altarOpensOnSealedLeizu {altarLeizu} pick [{ids}] basicCL {basic} idleCasts(no LMB) {idleCasts} clickCasts {casts} (6 clicks off-cooldown, spam ignored) maxChainHits {maxHits} misses {misses} cdAfterCast {cdAfter:0.00} (base {cdBase:0.00}) frames {_r1561}");
            // unrecorded: held LMB keeps firing at the cooldown rate only; toggle off = old auto chain
            int h0 = ChainLmbCasts1563; HInput.AtLmb = true; atMouse986 = _heroPos + new Vector2(5.6f, 0.3f); float t = 0; while (t < 2.3f) { yield return null; hp = maxHp; t += Time.deltaTime; } HInput.AtLmb = false;
            int held = ChainLmbCasts1563 - h0; int expect = Mathf.CeilToInt(2.3f / Mathf.Max(0.1f, cdBase));
            T.chainLmb1563 = false; bool offAuto = Basic1517() == null; int a0 = ChainLmbCasts1563; t = 0; while (t < 1.5f) { yield return null; hp = maxHp; t += Time.deltaTime; } bool offNoAim = ChainLmbCasts1563 == a0; T.chainLmb1563 = true;
            ALog("oldpick leizu " + gL);
            ARes("chain1563:rules", held >= 1 && held <= expect + 1 && offAuto && offNoAim && gL.Contains("left 262") && gL.Contains("right 262"),
                $"heldLmb2.3s casts {held} (<= {expect + 1}) toggleOff=noBasic {offAuto} toggleOff=noAimedCasts {offNoAim} gaps {gL}");
            AtClearWorld();
            _souls51 = so; _sigils51 = si; _pactPath51 = pp; _pactSel51 = ps; _pactSealed51 = sd; _unlock51 = un; PactSave51(); _atGod = false;
            AtEnd("chain1563", "hero_sprite_not_base_final2");
        }
    }
}
#endif
