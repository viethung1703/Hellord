#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1567 pact LMB basics, one scenario per skill: pb1567spark / pb1567orb / pb1567jav (Leizu sealed) and pb1567chop / pb1567slam / pb1567axe /
    /// pb1567ghost (Xingtian sealed). Recorded pass (rec_&lt;scen&gt;): main menu -> DESCEND -> altar -> fake click DESCEND -> in-game start pick (all the pact's
    /// basics, gaps logged) -> pick the skill -> a group of imps: idle 45 f (no LMB), clicks c1, c2 aimed at the group, c3 aimed LEFT at nothing, a 3-click spam
    /// inside the cooldown, c4, c5 at the group. Checks: casts only on click (instant), cooldown, hits, and the skill's own mechanic. Then unrecorded rules.</summary>
    public partial class HellordGame
    {
        static readonly string[] PbScen1567 = { "pb1567spark", "pb1567orb", "pb1567jav", "pb1567chop", "pb1567slam", "pb1567axe", "pb1567ghost" };
        IEnumerator AtPb1567(int ix)
        {
            string scen = PbScen1567[ix]; SkillId id = SkillId.SparkSpray + ix; bool leizu = ix < 3;
            yield return AtBegin(scen); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51(); var T = HellordTuning.I;
            int so = _souls51, si = _sigils51, pp = _pactPath51, ps = _pactSel51; bool sd = _pactSealed51; var un = (int[])_unlock51.Clone();
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = leizu ? 2 : 3; _pactSel51 = 0x1; _pactSealed51 = true; PactSave51(); _altarView51 = 0;
            _atGod = true; Manual716 = false;
            AtMenu1555(); yield return AtWait(1.0f);
            string clip = scen; _r1561 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1561(0.5f, clip);
            Act69(0); yield return AtHold1561(1.0f, clip);
            var pc = AtPlayC1561(); yield return AtFake1561(pc.x, pc.y, clip);
            string ids = AtCardIds1559(), gaps = AtOldPickGaps1559();
            string want = leizu ? "ChainLightning,SparkSpray,BallLightning,ThunderJavelin" : "BloodWhip,HeadsmansChop,EarthSplittingSlam,TwinAxeThrow,PhantomAxes";
            bool pickOk = mode == Mode.StartPick && ids == want;
            yield return AtHold1561(1.2f, clip); yield return AtShot(scen + "_pick_" + (leizu ? "leizu" : "xingtian"));
            ALog(scen + " pick [" + ids + "] " + gaps + " art: " + PbArtReport1567());
            int ci = cards.FindIndex(c => c.skill != null && c.skill.id == id); if (ci >= 0) Choose(cards[ci]);
            var sk = skills.Count > 0 ? skills[0] : null; bool basic = Basic1517() != null && Basic1517().def.id == id;
            maxHp = hp = 100000; SnapCamera(); var list = new List<Enemy>();
            float spd0 = 0.05f;
            void Mk(float dx, float dy) { var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed *= spd0; e.hp = e.maxHp = 3000; list.Add(e); } }
            Vector2 aimR = new Vector2(5.6f, 0.3f);
            switch (ix)
            {
                case 3: Mk(1.5f, 0.2f); Mk(1.9f, 0.9f); Mk(2.0f, -0.7f); Mk(2.7f, 0.3f); Mk(3.2f, -0.5f); Mk(3.0f, 1.2f); aimR = new Vector2(3f, 0.2f); break;          // chop
                case 4: Mk(1.5f, 0.2f); Mk(2.0f, 0.8f); Mk(2.2f, -0.6f); Mk(3.0f, 0.2f); Mk(2.6f, 1.3f); Mk(3.4f, -0.8f); aimR = new Vector2(3f, 0.2f); break;          // slam
                case 5: Mk(2.5f, 0.2f); Mk(3.5f, -0.3f); Mk(4.5f, 0.4f); Mk(5.2f, -0.2f); Mk(4.0f, 1.0f); Mk(3.0f, -1.0f); break;                                         // axe
                case 6: Mk(5.6f, 0.3f); Mk(5.0f, 0.8f); Mk(6.2f, -0.2f); Mk(5.4f, -0.5f); Mk(6.0f, 1.0f); Mk(4.8f, -0.1f); break;                                         // phantoms
                default: Mk(4.2f, 0.2f); Mk(5.4f, 1.2f); Mk(5.6f, -0.9f); Mk(6.8f, 0.4f); Mk(7.6f, -0.6f); Mk(6.6f, 1.9f); break;                                        // spells
            }
            float cd = sk != null ? sk.def.cooldown * sk.CdMul : 1f; int g = Mathf.CeilToInt(cd * 30f) + 6; if (ix == 5) g = Mathf.Max(g, 66); // HFX axe is slower (11.25 u/s out, eased return): first pass 42
            int c1 = 50, c2 = c1 + g, c3 = c2 + g, c4 = c3 + g, c5 = c4 + g; int[] clk = { c1, c2, c3, c3 + 5, c3 + 10, c3 + 14, c4, c5 }; int nF = c5 + (ix == 1 ? 110 : 60);
            int k0 = PbCasts1567[ix], h0 = PbHits1567[ix], idleCasts = 0, firstCastF = -1, spamCasts = 0; float cdAfter = 0f;
            float maxPerp = 0f; int maxGhosts = 0; bool axeDouble = false, axePrevOut = false; int axeT0 = AxeThrows1567, axeC0 = AxeCatches1567; int firstImpactF = -1, firstEruptF = -1; float knockOut = -9f; bool slowSeen = false; Vector2 heroAt = _heroPos;
            Vector2[] p0 = new Vector2[list.Count]; int shotF = c2 + (ix == 1 ? 30 : ix == 4 ? 34 : ix == 5 ? 10 : 4);
            for (int f = 0; f < nF; f++)
            {
                yield return null; AtFrameChecks(); if (mode != Mode.Play) mode = Mode.Play; hp = maxHp;
                bool left = f >= c3 - 4 && f < c3 + 20; atMouse986 = _heroPos + (left ? new Vector2(-6f, 0.3f) : aimR);
                bool down = false; foreach (var c in clk) if (f >= c && f < c + 3) down = true; HInput.AtLmb = down;
                int casts = PbCasts1567[ix] - k0;
                if (f == 44) idleCasts = casts;
                if (firstCastF < 0 && casts > 0) firstCastF = f;
                if (f == c3 + 4) spamCasts = casts; if (f == c4 - 1) spamCasts = casts - spamCasts;
                if (sk != null && cdAfter <= 0f && casts > 0) cdAfter = sk.cd;
                foreach (var sp in _pbSparks) { float pr = Mathf.Abs((sp.p.x - sp.a.x) * sp.dir.y - (sp.p.y - sp.a.y) * sp.dir.x); if (pr > maxPerp) maxPerp = pr; }
                if (_pbGhosts.Count > maxGhosts) maxGhosts = _pbGhosts.Count;
                if (ix == 5) { if (AxeThrows1567 > axeT0 && axePrevOut && AxeCatches1567 == axeC0) axeDouble = true; axeT0 = AxeThrows1567; axeC0 = AxeCatches1567; axePrevOut = AxeOut1567; }
                if (ix == 4) { if (firstImpactF < 0 && SlamImpacts1567 > 0) firstImpactF = f; if (firstEruptF < 0 && SlamErupts1567 > 0) firstEruptF = f; foreach (var e in list) if (e.alive && e.slowT > 0f && e.slowAmt >= T.crackSlow1567 - 0.001f) slowSeen = true; }
                if (ix == 3 && f == c1 - 1) for (int q = 0; q < list.Count; q++) p0[q] = list[q].pos;
                if (ix == 3 && f == c1 + 8) { knockOut = -9f; for (int q = 0; q < list.Count; q++) { Vector2 r0 = p0[q] - _heroPos; if (r0.magnitude > T.chopRangeU1567) continue; float o = Vector2.Dot(list[q].pos - p0[q], r0.normalized); if (o > knockOut) knockOut = o; } foreach (var e in list) e.speed = e.speed / spd0 * 0.5f; }
                if (ix == 4 && f == c1 + 8) foreach (var e in list) e.speed = e.speed / spd0 * 0.5f;
                if (f == shotF) yield return AtShot(scen + "_action");
                AtAudioLog1481.Frame = _r1561; AtRecFrame(clip, ref _r1561);
            }
            HInput.AtLmb = false;
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            int castsT = PbCasts1567[ix] - k0, hits = PbHits1567[ix] - h0;
            bool common = pickOk && basic && idleCasts == 0 && firstCastF >= c1 && firstCastF <= c1 + 1 && castsT == 5 && spamCasts == 0 && cdAfter > 0.3f && hits > 0;
            string cs = $"pick [{ids}] ok {pickOk} basic {basic} idleCasts(no LMB) {idleCasts} firstCast f{firstCastF} (click f{c1}) clickCasts {castsT} (5 off-cooldown clicks, 3-click spam -> {spamCasts}) cdAfterCast {cdAfter:0.00} (cd {cd:0.00}) hits {hits} frames {_r1561}";
            // ---- the skill's own mechanic (recorded part)
            bool mech = false; string ms = "";
            switch (ix)
            {
                case 0: mech = SparkLastN1567 == HData.At(T.sparkCount1567, sk.level, 4) && SparkSpawned1567 == 5 * SparkLastN1567 && maxPerp > 0.08f && maxPerp < 2.0f; ms = $"sparksPerCast {SparkLastN1567} (Lv{sk.level} table {HData.At(T.sparkCount1567, sk.level, 4)}) spawned {SparkSpawned1567} maxSideways {maxPerp:0.00}u (wobble, scripted)"; break;
                case 1: mech = OrbZaps1567 >= 3 && OrbPeak1567 <= T.orbCap1567; ms = $"zaps {OrbZaps1567} orbPeak {OrbPeak1567} (cap {T.orbCap1567})"; break;
                case 2: mech = JavBursts1567 >= 3 && JavBursts1567 <= 4 && JavForkHits1567 >= 2 && JavFizzles1567 >= 1; ms = $"bursts {JavBursts1567} forkHits {JavForkHits1567} fizzles(left click) {JavFizzles1567}"; break;
                case 3: mech = knockOut > 0.15f && ChopWaves1567 >= 1 && ChopWaveHits1567 >= 1; ms = $"knockOutward {knockOut:0.00}u (8 f after the first chop) bloodWaves {ChopWaves1567} waveHits {ChopWaveHits1567} chopHits {ChopHits1567}"; break;
                case 4: { int dly = firstEruptF - firstImpactF; int exp = Mathf.RoundToInt(T.eruptDelayS1567 * 30f); mech = SlamErupts1567 >= 3 && SlamEruptHits1567 >= 1 && Mathf.Abs(dly - exp) <= 2 && SlamSlowed1567 >= 1 && slowSeen && SlamLastHeroMove1567 < 0.001f && (_heroPos - heroAt).magnitude < 0.001f && SlamCracksPeak1567 <= T.crackCap1567; ms = $"eruptions {SlamErupts1567} eruptHits {SlamEruptHits1567} eruptDelay {dly} f (expect {exp}) slowedDemons {SlamSlowed1567} slowSeen {slowSeen} heroMoved {(_heroPos - heroAt).magnitude:0.000} cracksPeak {SlamCracksPeak1567}"; break; }
                case 5: mech = AxeOutHits1567 >= 2 && AxeBackHits1567 >= 2 && !axeDouble && AxeCatches1567 >= 4; ms = $"outHits {AxeOutHits1567} returnHits {AxeBackHits1567} catches {AxeCatches1567} throws {AxeThrows1567} doubleOut {axeDouble}"; break;
                case 6: mech = PhLastN1567 == HData.At(T.phCount1567, sk.level, 2) && maxGhosts == PhLastN1567 && PhHits1567 >= 2; ms = $"ghostsPerClick {PhLastN1567} maxGhostsAlive {maxGhosts} phantomHits {PhHits1567}"; break;
            }
            ARes(scen + ":clip_flow", common && mech, cs + " | " + ms);
            // ---- unrecorded rules: held LMB fires at the cooldown rate only, the skill's cap / count rule, toggle off = no basic
            float tt = 0; int hc0 = PbCasts1567[ix]; HInput.AtLmb = true; atMouse986 = _heroPos + aimR; while (tt < 2.0f) { yield return null; hp = maxHp; tt += Time.deltaTime; } HInput.AtLmb = false;
            int held = PbCasts1567[ix] - hc0; int expect = Mathf.CeilToInt(2.0f / Mathf.Max(0.1f, cd)) + 1; while (_pbAxe != null && tt < 6f) { yield return null; tt += Time.deltaTime; }
            bool rule = true; string rs = "";
            switch (ix)
            {
                case 1: { int pk = OrbPeak1567, pop0 = OrbPopped1567; OrbPeak1567 = 0; HInput.AtLmb = true; for (int q = 0; q < 8; q++) { sk.cd = 0f; yield return null; hp = maxHp; } HInput.AtLmb = false; rule = OrbPeak1567 == T.orbCap1567 && OrbPopped1567 - pop0 >= 5; rs = $"8 casts in 8 frames: orbPeak {OrbPeak1567} (cap {T.orbCap1567}) popped {OrbPopped1567 - pop0}"; break; }
                case 4: { SlamCracksPeak1567 = 0; HInput.AtLmb = true; for (int q = 0; q < 5; q++) { sk.cd = 0f; yield return null; hp = maxHp; } HInput.AtLmb = false; rule = SlamCracksPeak1567 == T.crackCap1567 && _pbCracks.Count <= T.crackCap1567; rs = $"5 slams in 5 frames: cracksPeak {SlamCracksPeak1567} (cap {T.crackCap1567})"; break; }
                case 5: { int r0 = AxeThrows1567; HInput.AtLmb = true; for (int q = 0; q < 10; q++) { sk.cd = 0f; yield return null; hp = maxHp; } HInput.AtLmb = false; rule = AxeThrows1567 - r0 == 1 && AxeRefused1567 >= 5; rs = $"10 frames of forced-ready clicks: throws {AxeThrows1567 - r0} (one axe out) refused {AxeRefused1567}"; break; }
                case 6: { int lv = sk.level; sk.level = 3; sk.cd = 0f; HInput.AtLmb = true; yield return null; yield return null; HInput.AtLmb = false; rule = PhLastN1567 == 3; rs = $"Lv3 click: ghosts {PhLastN1567}"; sk.level = lv; break; }
                case 0: { int lv = sk.level; sk.level = 5; sk.cd = 0f; HInput.AtLmb = true; yield return null; yield return null; HInput.AtLmb = false; rule = SparkLastN1567 == 5; rs = $"Lv5 click: sparks {SparkLastN1567}"; sk.level = lv; break; }
            }
            tt = 0; while (tt < 1.0f) { yield return null; hp = maxHp; tt += Time.deltaTime; }
            T.pbSkillOn1567[ix] = false; bool offNo = Basic1517() == null; bool offNoCast = !DfBasics1555(leizu ? 2 : 3, false).Contains(id); T.pbSkillOn1567[ix] = true; // off = not an LMB basic and gone from the pact's start pick
            bool gapsEven = gaps.Contains(leizu ? "left 79 gap 6 right 79" : "left 18 gap 6 right 18");
            ARes(scen + ":rules", held >= 1 && held <= expect && rule && offNo && offNoCast && gapsEven, $"heldLmb2s casts {held} (<= {expect}) {rs} toggleOff=noBasic {offNo} notInStartPick {offNoCast} pickGaps {gaps}");
            AtClearWorld();
            _souls51 = so; _sigils51 = si; _pactPath51 = pp; _pactSel51 = ps; _pactSealed51 = sd; _unlock51 = un; PactSave51(); _atGod = false;
            AtEnd(scen, "hero_sprite_not_base_final2");
        }
    }
}
#endif
