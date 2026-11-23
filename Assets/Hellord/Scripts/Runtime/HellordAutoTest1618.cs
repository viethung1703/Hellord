#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1618 Zhurong kit autotests. zh1618 = framework + every skill (functional, PASS / FAIL lines); zhclip1618_basics / _move / _special / _ult =
    /// the in-game clips (run with -atRec 30). AtExtra1618 also dispatches the repo-only scenarios the main HellordAutoTest.cs (not in the repo) does not
    /// list, so the harness can be rebuilt from the repo: chain1563, pb1567_0..6, bh1595, descend1555, altar1561.</summary>
    public partial class HellordGame
    {
        [System.NonSerialized] public Vector2 AtMove1396; // autotest scripted walk (was injected into HellordGame by the old /tmp build script)
        IEnumerator AtExtra1618(string only)
        {
            switch (only)
            {
                case "zh1618": return AtZh1618();
                case "zhclip1618_basics": return AtZhClip1618(0);
                case "zhclip1618_move": return AtZhClip1618(1);
                case "zhclip1618_special": return AtZhClip1618(2);
                case "zhclip1618_ult": return AtZhClip1618(3);
                case "lzclip1666_basics": return AtLzClip1666(0); // t1666 Leizu kit clips + checks
                case "lzclip1666_move": return AtLzClip1666(1);
                case "lzclip1666_special": return AtLzClip1666(2);
                case "lzclip1666_ult": return AtLzClip1666(3);
                case "kindling1635": return AtKindlingClip1635();
                case "levelup1639": return AtLevelUpClip1639(); // t1639 clip: kit cards on level-up
                case "skillbar1641": return AtSkillBarClip1641(); // t1641 clip: the 4-slot kit bar
                case "startpact1643": return AtStartPact1643(); // t1643 clip: fresh save -> Zhurong unlocked + sealed
                case "groundsort1644": return AtGroundSort1644(); // t1644 clip: fire ground over corpses, under living enemies
                case "ultprog1646": return AtUltProg1646(); // t1646 clip: R slot ultimate progress
                case "sup1651": return AtSup1651(); // t1651 support matrix
                case "supclip1651": return AtSupClip1651(); // t1651 clip: dead pairs now working
                case "hydrascorch1647": return AtHydraScorch1647(); // t1647 clip: Hydra ends -> v3 scorch (own seed) // t1635 clip: Kindling shader patches under Fireball hits
                case "firewall986_nosplit": return ZNoSplit1618(AtFireWall986()); // the t986 Fire Wall test, with the t1618 split off (= Hellfire Pillar is the wall again)
                case "chain1563": return AtChain1563();
                case "bh1595": return AtBody1595();
                case "descend1555": return AtDescend1555();
                case "altar1561": return AtAltar1561();
            }
            if (only.StartsWith("pb1567_") && int.TryParse(only.Substring(7), out int ix)) return AtPb1567(ix);
            return null;
        }

        IEnumerator ZNoSplit1618(IEnumerator inner) { var T = HellordTuning.I; T.fireWallSplit1618 = false; FireWallDefs948(); yield return inner; T.fireWallSplit1618 = true; FireWallDefs948(); }
        // ---------------------------------------------------------------- shared run start: Zhurong pact, pick the basic from the descend cards
        int _zso, _zsi, _zpp, _zps; bool _zsd; int[] _zun; int _zM0, _zS0, _zU0; bool _zAny0;
        IEnumerator ZhStart1618(string scen, string clip, SkillId basic)
        {
            yield return AtBegin(scen); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51(); var T = HellordTuning.I;
            _zso = _souls51; _zsi = _sigils51; _zpp = _pactPath51; _zps = _pactSel51; _zsd = _pactSealed51; _zun = (int[])_unlock51.Clone(); _zM0 = T.kitMove1618; _zS0 = T.kitSpecial1618; _zU0 = T.kitUlt1618; _zAny0 = T.kitAnyPact1618; _zOv0 = T.kitSlotOverride1639; T.kitSlotOverride1639 = true; // t1639: the zh1618 tests drive the slots through the tuning override
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 0; _pactSel51 = 0x1; _pactSealed51 = true; PactSave51(); _altarView51 = 0; T.kitAnyPact1618 = false;
            _atGod = true; Manual716 = false;
            AtMenu1555(); yield return AtWait(1.0f);
            _r1561 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1561(0.3f, clip); Act69(0); yield return AtHold1561(0.6f, clip);
            var pc = AtPlayC1561(); yield return AtFake1561(pc.x, pc.y, clip);
            yield return AtHold1561(0.5f, clip);
            var ids = new System.Text.StringBuilder(); foreach (var c in cards) if (c.skill != null) ids.Append(c.skill.id).Append(' ');
            _zCards1618 = ids.ToString(); ALog(scen + " debug: raw " + string.Join(",", T.descendBasicsZhurong1555) + " | lmb " + string.Join(",", T.lmbBasicIds1517) + " | df " + string.Join(",", DfBasics1555(0, false)) + " | mode " + mode + " runpick " + _dfRunPick1559);
            int ci = cards.FindIndex(c => c.skill != null && c.skill.id == basic); if (ci >= 0) Choose(cards[ci]);
            maxHp = hp = 100000; SnapCamera();
        }
        string _zCards1618 = ""; bool _zOv0;
        void ZhEnd1618(string scen, string clip)
        {
            var T = HellordTuning.I; HInput.AtLmb = HInput.AtDash = HInput.AtRmb = HInput.AtUlt = false; AtMove1396 = Vector2.zero; atMouse986 = null;
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0 && clip != null) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            AtClearWorld(); T.kitMove1618 = _zM0; T.kitSpecial1618 = _zS0; T.kitUlt1618 = _zU0; T.kitAnyPact1618 = _zAny0; T.kitSlotOverride1639 = _zOv0;
            _souls51 = _zso; _sigils51 = _zsi; _pactPath51 = _zpp; _pactSel51 = _zps; _pactSealed51 = _zsd; _unlock51 = _zun; PactSave51(); _atGod = false;
            AtEnd(scen, "hero_sprite_not_base_final2");
        }
        string _zClip; bool _zKeepHp = true;
        IEnumerator ZF1618(int n) { for (int i = 0; i < n; i++) { yield return null; AtFrameChecks(); if (_zKeepHp && _atGod) hp = maxHp; if (_zClip != null) { AtAudioLog1481.Frame = _r1561; AtRecFrame(_zClip, ref _r1561); } } }
        IEnumerator ZT1618(float sec) { float t0 = runTime; int g = 0; while (runTime - t0 < sec && g++ < 3000) yield return ZF1618(1); }
        IEnumerator ZPress1618(int which) { if (which == 0) HInput.AtDash = true; else if (which == 1) HInput.AtRmb = true; else HInput.AtUlt = true; yield return ZF1618(1); HInput.AtDash = HInput.AtRmb = HInput.AtUlt = false; }
        IEnumerator ZDash1618(Vector2 dir) { AtMove1396 = dir; _dashCd49 = 0f; yield return ZPress1618(0); yield return ZF1618(10); AtMove1396 = Vector2.zero; }
        Enemy ZMk1618(List<Enemy> l, EType t, Vector2 at, float speed = 0f, float hpv = 50000f)
        {
            var e = SpawnEnemyAt(t, at); if (e == null) return null; e.dmg = 0; e.speed = speed; e.hp = e.maxHp = hpv; e.elite = false; l.Add(e); return e;
        }
        void ZKill1618(List<Enemy> l) { foreach (var e in l) if (e.alive) { StopStatuses(e); DetachFx(e); e.alive = false; e.dying = false; e.sr.gameObject.SetActive(false); } l.Clear(); }

        // ---------------------------------------------------------------- zh1618: framework + each skill
        IEnumerator AtZh1618()
        {
            string scen = "zh1618"; var T = HellordTuning.I; _zClip = null;
            ZhFxLogOn1625 = true; ZhFxLogReset1625();
            yield return ZhStart1618(scen, null, SkillId.Hydra);
            var sk = Basic1517(); bool picker = _zCards1618.Contains("FireWall") && _zCards1618.Contains("Hydra") && !_zCards1618.Contains("VolcanicOrb") && _zCards1618.Contains("FirePillar") && _zCards1618.Contains("RollingMagmaRock") && _zCards1618.Contains("Fireball");
            ARes(scen + ":picker_basics", picker && sk != null && sk.def.id == SkillId.Hydra, $"Zhurong start cards: {_zCards1618}| basic = {(sk != null ? sk.def.id.ToString() : "none")}");
            var L = new List<Enemy>(); Vector2 H() => _heroPos; Vector2 HS = _heroPos; void Home() { _heroPos = HS; _heroVel37 = Vector2.zero; SnapCamera(); }
            yield return ZF1618(10);

            // ---- framework
            {
                T.kitMove1618 = 1; T.kitSpecial1618 = 3; T.kitUlt1618 = 1; ZhMoveCd = ZhSpecCd = ZhUltCd = 0; ZhSunMeter1618 = 0;
                bool on = ZhOn1618; T.kit1618 = false; bool off = !ZhOn1618; T.kit1618 = true; int pp = _pactPath51; _pactPath51 = 2; bool leizuOff = !ZhOn1618; T.kitAnyPact1618 = true; bool anyOn = ZhOn1618; T.kitAnyPact1618 = false; _pactPath51 = pp;
                // movement: dash 1 = effigy + own cooldown; dash 2 (dash cd ready, effigy cd not) = plain dash
                int c0 = ZhCasts1618; Vector2 p0 = H(); yield return ZDash1618(Vector2.right); bool eff1 = ZhEffT1618 > 0f && (ZhEffP1618 - p0).magnitude < 0.01f && ZhMoveCd > T.effCd1618 - 1f; float moved1 = (H() - p0).magnitude;
                yield return ZT1618(T.dashCooldownS49 + 5f / 60f); Vector2 p1 = H(); float effT = ZhEffT1618; int c1 = ZhCasts1618; yield return ZDash1618(Vector2.left); float moved2 = (H() - p1).magnitude;
                bool plain = ZhCasts1618 == c1 && moved2 > 1f && (ZhEffP1618 - p0).magnitude < 0.01f;
                ARes(scen + ":fw_movement_own_cd", on && off && leizuOff && anyOn && eff1 && moved1 > 1f && plain && c1 == c0 + 1, $"kit on in Zhurong {on}, kit1618 off -> off {off}, Leizu run off {leizuOff}, kitAnyPact on {anyOn} | dash 1: effigy at start {eff1} moved {moved1:0.00} move cd {ZhMoveCd:0.0} | dash 2 after the dash cd: plain dash {plain} moved {moved2:0.00}");
                // special: press = cast + cooldown; press again = nothing
                int s0 = ZhCasts1618; yield return ZPress1618(1); bool sp1 = ZhCryCharges1625 == T.wcCharges1625 && ZhSpecCd > T.wcCd1625 - 1f; yield return ZF1618(5); yield return ZPress1618(1); bool sp2 = ZhCasts1618 == s0 + 1;
                // ultimate: not armed (meter 0) = nothing; armed = cast
                int u0 = ZhCasts1618; atMouse986 = H() + new Vector2(3f, 0f); yield return ZPress1618(2); bool notArmed = ZhCasts1618 == u0 && _zSuns1618.Count == 0;
                ZhSunMeter1618 = T.swMeter1618; bool armed = ZhUltArmed1618; yield return ZPress1618(2); bool cast = ZhCasts1618 == u0 + 1 && ZhUltCd > T.swCd1618 - 1f && ZhSunMeter1618 == 0f;
                yield return ZF1618(60);
                ARes(scen + ":fw_special_ult_press", sp1 && sp2 && notArmed && armed && cast, $"RMB: cast {sp1} (cd {ZhSpecCd:0.0}), 2nd press on cd ignored {sp2} | R: meter 0 ignored {notArmed}, full = armed {armed}, cast + cd {cast} ({ZhUltCd:0.0})");
                ZhCryCharges1625 = 0; ZhMoveCd = ZhSpecCd = ZhUltCd = 0; ZhEffT1618 = 0;
            }
            // ---- HUD readout strings (drawn in OnGUI; here: the names and states it prints)
            ARes(scen + ":hud_names", ZhMoveNames1618.Length == 4 && ZhSpecNames1618.Length == 6 && ZhUltNames1618.Length == 5 && T.kitHud1618, "readout lines MOV SPACE / SPC RMB / ULT R with cooldown, active timer or condition (meter %, n/15 BURNING, n/5 DODGES, AUTO ON DEATH)");

            Home(); yield return ZF1618(2);
            // ---- Hydra (t1625): placed at the cursor, heads spit bolts at the nearest enemy, capped, expires
            {
                ZKill1618(L); Vector2 c = H() + new Vector2(4f, 0f); for (int i = 0; i < 6; i++) ZMk1618(L, EType.Imp, c + new Vector2(Mathf.Cos(i * 1.05f), Mathf.Sin(i * 1.05f) * 0.7f) * 1.6f);
                yield return ZF1618(3); _zHy1625.Clear(); _zBolt1625.Clear();
                Vector2 spot = H() + new Vector2(1.5f, 1.5f); atMouse986 = spot; int b0 = ZhBoltsShot1625, h0 = ZhBoltHits1625, maxHy = 0; float hp0 = 0; foreach (var e in L) hp0 += e.hp;
                HInput.AtLmb = true; yield return ZF1618(2); HInput.AtLmb = false; bool placed = ZhHydras1625 == 1 && (_zHy1625[0].p - spot).magnitude < 0.05f;
                yield return ZT1618(1.8f); int b1 = ZhBoltsShot1625 - b0; // spec: heads wind up at 0.75 / 1.0 / 1.25 s, bolt + 0.27 s
                // hold LMB 4.5 s at 3 spots: 1 s cooldown -> 4+ casts, capped at hyMax (the oldest burns out)
                int burning = 0; float t0 = runTime; HInput.AtLmb = true; while (runTime - t0 < 4.5f) { atMouse986 = H() + new Vector2(1f + Mathf.Repeat(runTime, 3f), -1.5f); yield return ZF1618(1); maxHy = Mathf.Max(maxHy, ZhHydras1625); int bn = 0; foreach (var e in L) if (e.burnT > 0) bn++; burning = Mathf.Max(burning, bn); } HInput.AtLmb = false;
                float hp1 = 0; foreach (var e in L) hp1 += e.hp;
                // far placement clamps to hyRange
                _zHy1625.Clear(); atMouse986 = H() + new Vector2(30f, 0f); sk.cd = 0f; HInput.AtLmb = true; yield return ZF1618(2); HInput.AtLmb = false; float reach = ZhHydras1625 > 0 ? (_zHy1625[0].p - H()).magnitude : -1f;
                // no enemy in sight: no bolts; then expiry
                ZKill1618(L); yield return ZF1618(3); int b2 = ZhBoltsShot1625; float life = T.hyLife1625; T.hyLife1625 = 1.2f; yield return ZT1618(1.5f); bool quiet = ZhBoltsShot1625 == b2; bool expired = ZhHydras1625 == 0; T.hyLife1625 = life;
                bool fx = ZhFxSeen1625("ZFX_Hydra2") && ZhFxLast1625("ZFX_Hydra2", "_Rise") >= 1f && ZhFxSeen1625("ZFX_Hydra_Bolt");
                bool ok = placed && b1 >= 2 && maxHy == T.hyMax1625 && ZhBoltHits1625 - h0 > 0 && hp1 < hp0 && burning > 0 && reach <= T.hyRange1625 + 0.01f && reach > T.hyRange1625 - 1.5f && quiet && expired && fx;
                ARes(scen + ":hydra", ok, $"placed at the cursor {placed} | 1.5 s: {b1} bolts | LMB held 4.5 s: max alive {maxHy} (cap {T.hyMax1625}), bolts {ZhBoltsShot1625 - b0}, hits {ZhBoltHits1625 - h0}, pack dmg {hp0 - hp1:0}, burning {burning}/6 | far cursor -> placed {reach:0.0} u (range {T.hyRange1625}) | no target: no bolts {quiet} | expires {expired} | fx hook drive {fx}");
                _zHy1625.Clear(); _zBolt1625.Clear();
            }
            Home(); yield return ZF1618(2);
            // ---- Fire Wall split / Fire Pillar unchanged
            {
                ZKill1618(L); var wallDef = HData.Skill(SkillId.FireWall); var hpDef = HData.Skill(SkillId.HellfirePillar); FireWallDefs948();
                var fw = new SkillInst { def = wallDef }; skills[0] = fw; Vector2 at = H() + new Vector2(3.5f, 1.5f); atMouse986 = at; int w0 = FireWallLive948;
                HInput.AtLmb = true; yield return ZF1618(3); HInput.AtLmb = false; yield return ZF1618(2);
                bool wall = FireWallLive948 > w0 && Basic1517() == fw; string wd = FireWallDebug948();
                var hpI = new SkillInst { def = hpDef }; int w1 = FireWallLive948; ZMk1618(L, EType.Imp, H() + new Vector2(2f, 0)); float lc = _lastCast[(int)SkillId.HellfirePillar]; Cast(hpI, 1f); bool pillarOld = FireWallLive948 == w1 && !FireWallOn948 && hpDef.name == "Hellfire Pillar";
                T.fireWallSplit1618 = false; FireWallDefs948(); bool oldName = hpDef.name == "Fire Wall" && FireWallOn948; T.fireWallSplit1618 = true; FireWallDefs948();
                ARes(scen + ":fire_wall_split", wall && pillarOld && oldName && wallDef.name == "Fire Wall", $"Fire Wall LMB basic places a wall at the cursor {wall} [{wd}] | Hellfire Pillar = old Pillar again (no wall, name '{hpDef.name}') {pillarOld} | split off -> old t948 (Hellfire Pillar is the wall) {oldName}");
                skills[0] = sk; yield return ZF1618(30); ZKill1618(L);
            }
            Home(); yield return ZF1618(2);
            // ---- Ember Effigy: enemies near the decoy go to it
            {
                T.kitMove1618 = 1; ZhMoveCd = 0; Vector2 p0 = H(); var a = ZMk1618(L, EType.Imp, p0 + new Vector2(-3f, 0.5f), 2.5f); var far = ZMk1618(L, EType.Imp, p0 + new Vector2(-12f, 0f), 2.5f);
                yield return ZDash1618(Vector2.right); yield return ZDash1618(Vector2.right); // 2 dashes: effigy behind, hero ~2 dashes away
                float dA0 = (a.pos - ZhEffP1618).magnitude, dH0 = (a.pos - H()).magnitude; int lur0 = ZhLured1618; yield return ZF1618(40);
                float dA1 = (a.pos - ZhEffP1618).magnitude; bool goes = dA1 < dA0 - 0.5f && ZhLured1618 > lur0; float farHero = (far.pos - H()).magnitude;
                yield return ZT1618(T.effLife1618); bool gone = ZhEffT1618 <= 0f;
                ARes(scen + ":ember_effigy", goes && gone, $"imp near the decoy: dist to decoy {dA0:0.00} -> {dA1:0.00} (hero was {dH0:0.00} away), lure frames {ZhLured1618 - lur0} | decoy gone after {T.effLife1618} s {gone} | far imp still chases the hero (dist {farHero:0.0})");
                ZKill1618(L);
            }
            Home(); yield return ZF1618(2);
            // ---- Ember Anchor: dash leaves an ember, dash again inside the window = snap back (free), then plain
            {
                T.kitMove1618 = 2; ZhMoveCd = 0; Vector2 p0 = H(); yield return ZDash1618(Vector2.up); Vector2 p1 = H(); bool lit = ZhAncT1618 > 0f && (ZhAncP1618 - p0).magnitude < 0.01f;
                _dashCd49 = 5f; int sn = ZhSnaps1618; HInput.AtDash = true; yield return ZF1618(1); HInput.AtDash = false; yield return ZF1618(2);
                bool snapped = ZhSnaps1618 == sn + 1 && (H() - p0).magnitude < 0.05f && _dashCd49 > 4f; // dash cooldown untouched (free)
                _dashCd49 = 0f; Vector2 p2 = H(); yield return ZDash1618(Vector2.up); bool plainAfter = ZhSnaps1618 == sn + 1 && (H() - p2).magnitude > 1f && ZhAncT1618 <= 0f;
                ZhMoveCd = 0; yield return ZDash1618(Vector2.down); yield return ZT1618((T.ancWindow1618 + 0.2f)); _dashCd49 = 0; Vector2 p3 = H(); yield return ZDash1618(Vector2.down); bool expired = ZhSnaps1618 == sn + 1 && (H() - p3).magnitude > 1f;
                ARes(scen + ":ember_anchor", lit && snapped && plainAfter && expired, $"ember at the dash start {lit} | 2nd press inside {T.ancWindow1618} s with the dash on cooldown: snapped back to it {snapped} (moved {(p1 - p0).magnitude:0.00} out) | next press: plain dash {plainAfter} | after the window: plain dash {expired}");
            }
            Home(); yield return ZF1618(2);
            // ---- Backdraft: start point pulls nearby enemies into a group
            {
                T.kitMove1618 = 3; ZhMoveCd = 0; Vector2 p0 = H(); var ring = new List<Enemy>();
                for (int i = 0; i < 6; i++) ring.Add(ZMk1618(L, EType.Imp, p0 + new Vector2(Mathf.Cos(i * 1.05f) * 3.2f, Mathf.Sin(i * 1.05f) * 2.4f)));
                var outside = ZMk1618(L, EType.Imp, p0 + new Vector2(0f, -7f)); yield return ZF1618(3); Vector2 o0 = outside.pos;
                float m0 = 0; foreach (var e in ring) m0 += (e.pos - p0).magnitude; m0 /= ring.Count; int pu = ZhPulled1618;
                yield return ZDash1618(Vector2.right); yield return ZF1618(30);
                float m1 = 0; foreach (var e in ring) m1 += (e.pos - p0).magnitude; m1 /= ring.Count; bool outStay = (outside.pos - o0).magnitude < 0.3f;
                ARes(scen + ":backdraft", m1 < m0 * 0.6f && ZhPulled1618 - pu == 6 && outStay, $"6 imps around the start: mean dist {m0:0.00} -> {m1:0.00} u, pulled {ZhPulled1618 - pu} | imp outside {T.bdR1618} u not pulled {outStay}");
                ZKill1618(L);
            }
            Home(); yield return ZF1618(2);
            // ---- Pyre Mark
            {
                T.kitSpecial1618 = 1; ZhSpecCd = 0; Vector2 c = H() + new Vector2(4f, 0f);
                var a = ZMk1618(L, EType.Brute, c + new Vector2(0.3f, 0f), 0f, 400f); var b = ZMk1618(L, EType.Brute, c + new Vector2(2.2f, 0.5f), 0f, 400f); var d = ZMk1618(L, EType.Imp, c + new Vector2(-1.8f, -1f), 0f, 400f); yield return ZF1618(3);
                atMouse986 = BodyCentre1595(a); yield return ZPress1618(1); bool marked = ZhMark1618 == a; float kM = ZhBurnK1618(a), kO = ZhBurnK1618(b);
                yield return ZPress1618(1); bool one = ZhMark1618 == a; // cooldown, still the one mark
                int pays = ZhPyrePays1618; for (int i = 0; i < 12 && ZhPyrePays1618 == pays; i++) { Hit(a, 40f, sk, Vector2.zero); yield return ZT1618(0.5f); }
                bool paidBurn = ZhPyrePays1618 == pays + 1 && ZhMark1618 == null && ZhPyreBuffT1618 > 0f; float buff = ZhPreHit1618(10f, sk), buffPhys = ZhPreHit1618(10f, new SkillInst { def = HData.Skill(SkillId.FrostCleave) });
                ZhSpecCd = 0; ZhPyreBuffT1618 = 0; atMouse986 = BodyCentre1595(b); yield return ZPress1618(1); bool m2 = ZhMark1618 == b; Hit(b, 5000f, sk, Vector2.zero); yield return ZF1618(5); bool paidDeath = ZhPyrePays1618 == pays + 2 && ZhPyreBuffT1618 > 0f;
                ZhSpecCd = 0; atMouse986 = H() + new Vector2(-6f, 6f); int cc = ZhCasts1618; yield return ZPress1618(1); bool noTgt = ZhCasts1618 == cc && ZhSpecCd == 0f;
                ARes(scen + ":pyre_mark", marked && kM == T.pmBurnK1618 && kO == 1f && one && paidBurn && buff > 12.9f && buff < 13.1f && buffPhys == 10f && m2 && paidDeath && noTgt, $"marks the enemy nearest the cursor {marked} (burn k {kM} vs others {kO}), 1 mark {one} | burn threshold -> payoff {paidBurn}: fire hit 10 -> {buff:0.0}, frost hit 10 -> {buffPhys:0.0} | 2nd mark dies -> payoff {paidDeath} | no enemy near the cursor: no cast, no cd {noTgt}");
                ZKill1618(L); ZhPyreBuffT1618 = 0;
            }
            Home(); yield return ZF1618(2);
            // ---- Flammability
            {
                T.kitSpecial1618 = 2; ZhSpecCd = 0; Vector2 c = H() + new Vector2(4f, 0f); var a = ZMk1618(L, EType.Imp, c + new Vector2(0.5f, 0f)); var b = ZMk1618(L, EType.Imp, c + new Vector2(-0.6f, 0.4f)); var o = ZMk1618(L, EType.Imp, c + new Vector2(5f, 0f)); yield return ZF1618(3);
                atMouse986 = c; yield return ZPress1618(1); yield return ZT1618(T.flDelay1618 * 0.6f); bool early = a.zFlam1618 <= runTime;
                yield return ZT1618(T.flDelay1618 * 0.6f); bool cursed = a.zFlam1618 > runTime && b.zFlam1618 > runTime && o.zFlam1618 <= runTime;
                float k = ZhBurnK1618(a), tick = ZhBurnTick1618(a, 10f), tickO = ZhBurnTick1618(o, 10f);
                Hit(a, 10f, sk, Vector2.zero); Hit(o, 10f, sk, Vector2.zero); float dpsA = a.burnDps, dpsO = o.burnDps;
                yield return ZT1618(T.flDur1618 + 10f / 60f); bool ends = a.zFlam1618 <= runTime;
                ARes(scen + ":flammability", early && cursed && k == T.flBurnK1618 && tick == T.flBurnDmg1618 && tickO == 1f && dpsA > dpsO * 1.4f && ends, $"not cursed during the {T.flDelay1618} s delay {early} | then 2 inside cursed, 1 outside not {cursed} | burn build x{k}, burn tick x{tick} (outside x{tickO}), burn dps {dpsA:0.0} vs {dpsO:0.0} | wears off after {T.flDur1618} s {ends}");
                ZKill1618(L);
            }
            Home(); yield return ZF1618(2);
            // ---- War Cry (t1625; replaced Molten Shell): next 5 basics powered, roared-at enemies explode on death for 4 s
            {
                ZKill1618(L); _zHy1625.Clear(); _zBolt1625.Clear(); T.kitSpecial1618 = 3; ZhSpecCd = 0; ZhCryCharges1625 = 0;
                var a = ZMk1618(L, EType.Imp, H() + new Vector2(2f, 0f)); var nb = ZMk1618(L, EType.Imp, H() + new Vector2(2.8f, 0.3f)); var c2 = ZMk1618(L, EType.Imp, H() + new Vector2(-2f, 0.5f)); var far = ZMk1618(L, EType.Imp, H() + new Vector2(0f, -T.wcR1625 - 1.5f));
                yield return ZF1618(3); yield return ZPress1618(1);
                bool roar = ZhCryCharges1625 == T.wcCharges1625 && ZhSpecCd > T.wcCd1625 - 1f && a.zCry1625 > runTime && nb.zCry1625 > runTime && far.zCry1625 == 0f;
                // powered basics: 5 casts at x wcMul, the 6th normal (Hydra damage is stored per hydra)
                var dm = new List<float>(); int pw0 = ZhCryPowered1625;
                for (int k = 0; k < 6; k++) { sk.cd = 0f; atMouse986 = H() + new Vector2(-1f, 1.5f); HInput.AtLmb = true; yield return ZF1618(1); HInput.AtLmb = false; yield return ZF1618(1); if (_zHy1625.Count > 0) dm.Add(_zHy1625[_zHy1625.Count - 1].dmg); }
                bool pow = dm.Count == 6 && Mathf.Abs(dm[0] / dm[5] - T.wcMul1625) < 0.01f && Mathf.Abs(dm[4] / dm[5] - T.wcMul1625) < 0.01f && ZhCryCharges1625 == 0 && ZhCryPowered1625 - pw0 == 5;
                _zHy1625.Clear(); _zBolt1625.Clear();
                // death blast: the marked imp dies -> its neighbour takes the blast; the far (unmarked) one dies quietly
                int bo = ZhCryBooms1625; float nh0 = nb.hp; a.hp = 1f; Hit(a, 50f, sk, Vector2.zero); yield return ZF1618(2); bool boom = ZhCryBooms1625 == bo + 1 && nb.hp < nh0 && !a.alive;
                far.hp = 1f; Hit(far, 50f, sk, Vector2.zero); yield return ZF1618(2); bool quiet = ZhCryBooms1625 == bo + 1;
                yield return ZT1618(T.wcMarkS1625 + 0.2f); c2.hp = 1f; Hit(c2, 50f, sk, Vector2.zero); yield return ZF1618(2); bool expired = ZhCryBooms1625 == bo + 1;
                ARes(scen + ":war_cry", roar && pow && boom && quiet && expired, $"roar: {ZhCryCharges1625}->{T.wcCharges1625} charges, cd {T.wcCd1625} s, imps within {T.wcR1625} u marked, far one not {roar} | 6 basics: dmg {string.Join("/", dm.ConvertAll(x => x.ToString("0.0")))} (first 5 x{T.wcMul1625}) {pow} | marked dies -> blast hits the neighbour ({nh0 - nb.hp:0}) {boom} | unmarked: no blast {quiet} | after {T.wcMarkS1625} s: no blast {expired}");
                ZKill1618(L);
            }
            Home(); yield return ZF1618(2);
            // ---- Smolder Ring
            {
                T.kitSpecial1618 = 4; ZhSpecCd = 0; Vector2 c = H() + new Vector2(4f, 2f); var a = ZMk1618(L, EType.Imp, c + new Vector2(0.4f, 0f), 2.0f); var o = ZMk1618(L, EType.Imp, c + new Vector2(0f, -6f), 2.0f);
                atMouse986 = c; yield return ZPress1618(1); Vector2 a0 = a.pos, o0 = o.pos; float ah = a.hp; yield return ZF1618(60);
                float ma = (a.pos - a0).magnitude, mo = (o.pos - o0).magnitude; bool slowed = ma < mo * 0.5f; bool scorched = a.hp < ah && ZhRingHits1618 > 0;
                yield return ZT1618(T.srLife1618); bool gone = _zRings1618.Count == 0;
                ARes(scen + ":smolder_ring", slowed && scorched && gone, $"imp inside moved {ma:0.00} u in 1 s vs outside {mo:0.00} u (slow {T.srSlow1618:P0}) {slowed} | scorched dmg {ah - a.hp:0} {scorched} | ring gone after {T.srLife1618} s {gone}");
                ZKill1618(L);
            }
            // ---- Hearth Blessing
            {
                T.kitSpecial1618 = 5; ZhSpecCd = 0; _atGod = false; maxHp = 1000; hp = 500; _heroSlowT = 3f; _chill44 = 2; float spd0 = HeroSlowMul37() * HeroChillMul44();
                yield return ZPress1618(1); bool clean = _heroSlowT <= 0f && _chill44 == 0; ApplyFrostSlow37(3f); AddChill44(2); bool blocked = _heroSlowT <= 0f && _chill44 == 0;
                yield return ZT1618(T.hbDur1618 + 5f / 60f); float healed = hp - 500f; ApplyFrostSlow37(1f); bool after = _heroSlowT > 0f; _heroSlowT = 0f;
                hp = maxHp = 100000; _atGod = true;
                ARes(scen + ":hearth_blessing", clean && blocked && healed > T.hbHeal1618 * 1000f * 0.85f && healed < T.hbHeal1618 * 1000f * 1.15f && after && spd0 < 1f, $"slow + chill removed {clean} | new slow / chill blocked in the window {blocked} | healed {healed:0} of 1000 max over {T.hbDur1618} s (want {T.hbHeal1618:P0}) | debuffs land again after {after}");
            }
            Home(); yield return ZF1618(2);
            // ---- Sun's Wrath: meter from real burn ticks, falling sun at the cursor, burning area
            {
                T.kitUlt1618 = 1; ZhUltCd = 0; ZhSunMeter1618 = 0; Vector2 c = H() + new Vector2(4f, 0f);
                for (int i = 0; i < 5; i++) ZMk1618(L, EType.Brute, c + new Vector2((i - 2) * 0.7f, (i % 2) * 0.5f));
                foreach (var e in L) Hit(e, 30f, sk, Vector2.zero); yield return ZF1618(120); float meter = ZhSunMeter1618;
                ZhSunMeter1618 = T.swMeter1618; atMouse986 = c; int p0 = _fbPatches.Count, sh = ZhSunHits1618; float hp0 = 0; foreach (var e in L) hp0 += e.hp;
                yield return ZPress1618(2); yield return ZT1618(T.swFall1618 + 5f / 60f); float hp1 = 0; foreach (var e in L) hp1 += e.hp;
                bool area = _fbPatches.Count > p0; int hits = ZhSunHits1618 - sh;
                ARes(scen + ":suns_wrath", meter > 0f && hits == 5 && hp1 < hp0 && area && ZhUltCd > 0f, $"meter from burn ticks {meter:0}/{T.swMeter1618} | sun at the cursor after {T.swFall1618} s: hit {hits}/5, dmg {hp0 - hp1:0} | burning area {area} | cd {ZhUltCd:0}");
                ZKill1618(L); ZhUltCd = 0;
            }
            Home(); yield return ZF1618(2);
            // ---- Rebirth Flame
            {
                T.kitUlt1618 = 2; ZhUltCd = 0; _atGod = false; maxHp = 1000; hp = 30; var near = ZMk1618(L, EType.Imp, H() + new Vector2(1.5f, 0f)); yield return ZF1618(3); hp = 30; float nh = near.hp; _iframe = 0;
                int sh = ZhStormHits1618; HurtHero(500f, "test"); bool rev = mode == Mode.Play && Mathf.Abs(hp - 500f) < 0.5f && ZhUltCd > T.rbCd1618 - 1f && ZhStormHits1618 > sh && near.hp < nh && _iframe > 1f;
                yield return ZF1618(5); yield return ZPress1618(2); bool notPressed = ZhRebirths1618 == 1; bool onceOnly = !ZhRebirth1618();
                hp = maxHp = 100000; _atGod = true; _iframe = 0;
                ARes(scen + ":rebirth_flame", rev && notPressed && onceOnly, $"lethal hit -> revived at {T.rbHp1618:P0} HP, firestorm hit the imp, i-frames {T.rbInv1618} s {rev} | R does nothing {notPressed} | on cooldown ({T.rbCd1618} s) it will not fire again {onceOnly}");
                ZKill1618(L); ZhUltCd = 0;
            }
            Home(); yield return ZF1618(2);
            // ---- Molten Heart
            {
                T.kitUlt1618 = 3; ZhUltCd = 0; Vector2 c = H() + new Vector2(5f, 0f);
                for (int i = 0; i < 14; i++) ZMk1618(L, EType.Imp, c + new Vector2((i % 5) * 0.8f, (i / 5) * 0.8f));
                foreach (var e in L) ApplyStatus(e, Status.Burn, 5f, 1f, sk); yield return ZF1618(2); bool notYet = !ZhUltArmed1618 && ZhBurning1618 == 14;
                var x = ZMk1618(L, EType.Imp, c + new Vector2(0f, -1.5f)); ApplyStatus(x, Status.Burn, 5f, 1f, sk); yield return ZF1618(2); bool armed = ZhUltArmed1618;
                yield return ZPress1618(2); bool act = ZhHeartT1618 > 0f; ApplyFrostSlow37(2f); AddChill44(3); bool unslow = ZhNoSlow1618 && _heroSlowT <= 0f && _chill44 == 0;
                int ec = ZhHeartEchoes1618; float lc0 = _lastCast[(int)sk.def.id]; atMouse986 = c; sk.cd = 0; HInput.AtLmb = true; yield return ZF1618(2); HInput.AtLmb = false; yield return ZF1618(20);
                bool twice = ZhHeartEchoes1618 == ec + 1;
                // move speed while slowed: walk 0.5 s
                Vector2 w0 = H(); AtMove1396 = Vector2.down; _holdT49 = 3f; yield return ZT1618(0.5f); AtMove1396 = Vector2.zero; float walk = (H() - w0).magnitude; _heroSlowT = 0f;
                yield return ZT1618(T.mhDur1618); bool ends = ZhHeartT1618 <= 0f && !ZhNoSlow1618;
                ARes(scen + ":molten_heart", notYet && armed && act && unslow && twice && walk > baseSpeed * speedMul * 0.5f * 0.8f && ends, $"14 burning = not armed {notYet}, 15 = armed {armed} | active {act} | basic fired twice {twice} | frost slow / chill blocked {unslow}, ghoul-held walk {walk:0.00} u in 0.5 s (unslowed {baseSpeed * speedMul * 0.5f:0.00}) {unslow} | ends after {T.mhDur1618} s {ends}");
                ZKill1618(L); ZhUltCd = 0;
            }
            Home(); yield return ZF1618(2);
            // ---- Vermilion Bird
            {
                T.kitUlt1618 = 4; ZhUltCd = 0; ZhNearMiss1618 = 0; T.kitMove1618 = 0;
                var far = ZMk1618(L, EType.Imp, H() + new Vector2(0f, 6f)); yield return ZDash1618(Vector2.left); bool farNo = ZhNearMiss1618 == 0; ZKill1618(L);
                for (int i = 0; i < T.vbNeed1618; i++) { var e = ZMk1618(L, EType.Imp, H() + new Vector2(0.45f, 0.1f)); yield return ZF1618(2); yield return ZDash1618(i % 2 == 0 ? Vector2.left : Vector2.right); ZKill1618(L); }
                bool armed = ZhUltArmed1618; Vector2 c = H() + new Vector2(5f, 0f); for (int i = 0; i < 4; i++) ZMk1618(L, EType.Imp, H() + new Vector2(2f + i * 2.5f, (i % 2) * 0.4f)); var off = ZMk1618(L, EType.Imp, H() + new Vector2(4f, 4f));
                atMouse986 = c; int bh = ZhBirdHits1618, p0 = _fbPatches.Count; float offHp = off.hp; yield return ZPress1618(2); yield return ZF1618(60);
                bool hits = ZhBirdHits1618 - bh == 4 && off.hp == offHp && _fbPatches.Count > p0 + 5 && ZhNearMiss1618 == 0 && ZhUltCd > 0f;
                ARes(scen + ":vermilion_bird", farNo && armed && hits, $"dash with nothing near: no count {farNo} | {T.vbNeed1618} near-miss dashes -> armed {armed} | sweep along the aim: hit {ZhBirdHits1618 - bh}/4 on the line, off-line imp untouched {off.hp == offHp}, burning strips {_fbPatches.Count - p0} | count reset, cd {ZhUltCd:0}");
                ZKill1618(L); ZhUltCd = 0;
            }
            // ---- t1625 FX (fx_v2, approved): every effect drives its spec material; approved materials load from Resources/ZhurongFX and draw
            {
                string[] want = { "ZFX_EmberEffigy", "ZFX_EmberAnchor_Marker", "ZFX_EmberAnchor_Snap", "ZFX_Backdraft_Vortex", "ZFX_PyreMark", "ZFX_PyreMark_Burst", "ZFX_Flammability_Curse", "ZFX_Flammability_Tick",
                    "ZFX_SmolderRing", "ZFX_HearthBlessing", "ZFX_HearthBlessing_Cleanse", "ZFX_HearthBlessing_Light", "ZFX_SunsWrath_Telegraph", "ZFX_SunsWrath_Sun", "ZFX_SunsWrath_Impact", "ZFX_SunsWrath_Burn", "ZFX_SunsWrath_Light",
                    "ZFX_RebirthFlame_Pillar", "ZFX_RebirthFlame_Storm", "ZFX_MoltenHeart_Back", "ZFX_MoltenHeart_Front", "ZFX_VermilionBird" };
                string[] pending = { "ZFX_Hydra2", "ZFX_Hydra_Light", "ZFX_Hydra_Bolt", "ZFX_Hydra_Erupt", "ZFX_Hydra_Scorch", "ZFX_Hydra_BoltPop", "ZFX_WarCry_Roar", "ZFX_WarCry_Light", "ZFX_WarCry_Empowered", "ZFX_WarCry_Mark", "ZFX_WarCry_Consume", "ZFX_WarCry_DeathPop" }; // t1625 §14 / §15 (landed, VH not seen yet)
                var miss = new System.Text.StringBuilder(); var noMat = new System.Text.StringBuilder(); foreach (var w in want) { if (!ZhFxSeen1625(w)) miss.Append(w).Append(' '); if (ZhFxMat1625(w) == null) noMat.Append(w).Append(' '); }
                var pend = new System.Text.StringBuilder(); foreach (var w in pending) { if (!ZhFxSeen1625(w)) miss.Append(w).Append(' '); if (ZhFxMat1625(w) == null) pend.Append(w).Append(' '); }
                bool shellGone = ZhFxMat1625("ZFX_MoltenShell_Back") == null && ZhFxMat1625("ZFX_VolcanicOrb_Body") == null && ZhFxMat1625("ZFX_Hydra") == null; // + Hydra v1 (replaced by v2)
                bool prog = ZhFxMax1625("ZFX_Hydra2", "_Rise") >= 1f && ZhFxMax1625("ZFX_WarCry_Roar", "_Radius") == T.wcR1625 * 16f && ZhFxMax1625("ZFX_SunsWrath_Impact", "_Progress") > 0.5f && ZhFxMax1625("ZFX_MoltenHeart_Back", "_Form") >= 1f && ZhFxMax1625("ZFX_Flammability_Curse", "_Ignite") >= 1f && !float.IsNaN(ZhFxLast1625("ZFX_VermilionBird", "_Dir.y"));
                ARes(scen + ":fx_v2", miss.Length == 0 && noMat.Length == 0 && pend.Length == 0 && shellGone && prog && T.zhFxV2_1625 && ZhFxQuads1625 > 0, $"driven: missing [{miss}] | approved materials not loaded [{noMat}] | Hydra / War Cry materials not loaded [{pend}] | Molten Shell / Volcanic Orb / Hydra v1 materials not shipped {shellGone} | drive values reach their ends {prog} (hydra rise {ZhFxMax1625("ZFX_Hydra2", "_Rise"):0.00}, roar R {ZhFxMax1625("ZFX_WarCry_Roar", "_Radius"):0}, sun impact {ZhFxMax1625("ZFX_SunsWrath_Impact", "_Progress"):0.00}, heart form {ZhFxMax1625("ZFX_MoltenHeart_Back", "_Form"):0.00}, curse ignite {ZhFxMax1625("ZFX_Flammability_Curse", "_Ignite"):0.00}, bird dir.y {ZhFxMax1625("ZFX_VermilionBird", "_Dir.y"):0.00}) | shader quads drawn {ZhFxQuads1625}");
                ZhFxLogOn1625 = false;
            }
            yield return AtShot(scen + "_end");
            ZhEnd1618(scen, null);
        }

        // ---------------------------------------------------------------- t1647 clip: two Hydras (own seeds) spit, fade, and leave the v3 scorch cooling in place
        IEnumerator AtHydraScorch1647()
        {
            string scen = "hydrascorch1647", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Hydra);
            var sk = Basic1517(); float life = T.hyLife1625; T.hyLife1625 = 2.6f; _zHy1625.Clear(); _zBolt1625.Clear();
            var L = new List<Enemy>(); Vector2 H0 = _heroPos; ZMk1618(L, EType.Imp, H0 + new Vector2(0.5f, 3.2f), 0f, 50000f); ZMk1618(L, EType.Imp, H0 + new Vector2(-0.5f, 3.4f), 0f, 50000f);
            Vector2[] spots = { H0 + new Vector2(-2.4f, 0.9f), H0 + new Vector2(2.4f, 0.9f) }; var seeds = new List<float>(); int scorch0 = 0;
            foreach (var at in spots) { if (sk != null) sk.cd = 0f; atMouse986 = at; HInput.AtLmb = true; yield return ZF1618(2); HInput.AtLmb = false; if (_zHy1625.Count > 0) seeds.Add(_zHy1625[_zHy1625.Count - 1].seed); yield return ZF1618(8); }
            atMouse986 = H0 + new Vector2(0f, -2.5f);
            float t0 = runTime; bool sawScorch = false; float sMin = 9f, sMax = -9f;
            while (runTime - t0 < 4.6f) { yield return ZF1618(1); foreach (var f in _zFx1618) if (f.mat == "ZFX_Hydra_Scorch") { sawScorch = true; sMin = Mathf.Min(sMin, f.seed); sMax = Mathf.Max(sMax, f.seed); } }
            T.hyLife1625 = life; ZKill1618(L);
            string sd = string.Join(",", seeds.ConvertAll(x => x.ToString("0.000")));
            ARes(scen + ":clip", seeds.Count == 2 && sawScorch && _zHy1625.Count == 0, $"frames {_r1561} | hydra seeds {sd} | scorch seeds seen {sMin:0.000}..{sMax:0.000} | scorch mat {(Resources.Load<Material>("ZhurongFX/Materials/ZFX_Hydra_Scorch") is Material mm ? mm.shader.name : "none")}");
            _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---------------------------------------------------------------- t1646 clip: R slot: cooldown sweep -> condition fill -> armed pulse + chime (Sun's Wrath, Vermilion Bird, Molten Heart)
        IEnumerator AtUltProg1646()
        {
            string scen = "ultprog1646", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball); // tuning override on (ZhStart): the slots come from kitMove / kitSpecial / kitUlt
            T.kitMove1618 = 1; T.kitSpecial1618 = 3; var log = new System.Text.StringBuilder(); var sk = skills.Count > 0 ? skills[0] : null;
            // Sun's Wrath: 2 s cooldown, then the meter builds (scripted burn-damage meter), arms, chimes
            T.kitUlt1618 = 1; ZhSunMeter1618 = 0f; ZhUltCd = 2f; yield return ZT1618(2.1f);
            for (int k = 0; k <= 40; k++) { ZhSunMeter1618 = T.swMeter1618 * k / 40f; yield return ZF1618(2); }
            log.Append(" sun armed ").Append(ZhUltArmed1618); yield return ZF1618(40);
            // Vermilion Bird: 0/5 .. 5/5 near-miss dodges (scripted count), arms, chimes
            T.kitUlt1618 = 4; ZhUltCd = 0f; ZhNearMiss1618 = 0; yield return ZF1618(20);
            for (int k = 1; k <= T.vbNeed1618; k++) { ZhNearMiss1618 = k; yield return ZF1618(18); }
            log.Append(" | bird armed ").Append(ZhUltArmed1618); yield return ZF1618(40);
            // Molten Heart: the LIVE burning count (real Burn on 15 imps, one by one; it falls again as the burns run out)
            T.kitUlt1618 = 3; ZhUltCd = 0f; var L = new List<Enemy>(); Vector2 H0 = _heroPos;
            for (int i = 0; i < 16; i++) ZMk1618(L, EType.Imp, H0 + new Vector2(3.6f + (i % 4) * 0.7f, -1.2f + (i / 4) * 0.8f), 0f, 50000f);
            yield return ZF1618(5); int maxB = 0;
            for (int i = 0; i < L.Count; i++) { ApplyStatus(L[i], Status.Burn, 2.5f, 1f, sk); yield return ZF1618(4); maxB = Mathf.Max(maxB, ZhBurning1618); }
            bool heartArmed = ZhUltArmed1618; yield return ZT1618(3.2f); int after = ZhBurning1618;
            log.Append(" | heart max ").Append(maxB).Append(" armed ").Append(heartArmed).Append(" after burns ").Append(after);
            yield return ZF1618(20);
            ARes(scen + ":clip", log.ToString().Contains("sun armed True") && log.ToString().Contains("bird armed True") && (_r1561 > 0 || _atRecFps <= 0), $"frames {_r1561} |{log} | R cell {Cell716(3)}");
            ZKill1618(L); _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---------------------------------------------------------------- t1644 clip: a corpse pile, living imps on it, Fireball (Kindling patches) then Infernal Boulder (trail band) over the pile
        IEnumerator AtGroundSort1644()
        {
            string scen = "groundsort1644", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball); bool kit0 = T.kit1618; T.kit1618 = false;
            Vector2 H0 = _heroPos, P = H0 + new Vector2(4.6f, 0f); var dead = new List<Enemy>(); var L = new List<Enemy>();
            for (int i = 0; i < 16; i++) ZMk1618(dead, i % 4 == 0 ? EType.Brute : EType.Imp, P + new Vector2((i % 4) * 0.55f - 0.8f, (i / 4) * 0.5f - 0.75f), 0f, 1f);
            yield return ZF1618(3); foreach (var e in dead) if (e.alive) KillEnemy(e); dead.Clear(); yield return ZF1618(40); // corpses + blood stay (d2CorpsePersist1447)
            int corpses = 0; foreach (var e in _en) if (!e.alive && e.sr != null && e.sr.gameObject.activeSelf) corpses++;
            for (int i = 0; i < 4; i++) ZMk1618(L, EType.Imp, P + new Vector2((i % 2) * 1.0f - 0.5f, (i / 2) * 0.9f - 0.45f), 0f, 50000f); // living imps standing on the pile
            yield return ZF1618(10); Toast("KINDLING ON CORPSES");
            HInput.AtLmb = true; float t0 = runTime; int maxP = 0;
            while (runTime - t0 < 3.5f) { atMouse986 = P + new Vector2(0f, Mathf.Sin((runTime - t0) * 2f) * 0.5f); yield return ZF1618(1); maxP = Mathf.Max(maxP, _patch53.Count); }
            HInput.AtLmb = false; yield return ZF1618(10);
            skills[0] = new SkillInst { def = HData.Skill(SkillId.RollingMagmaRock), level = 1 }; SetHeroLayers(); Toast("INFERNAL BOULDER TRAIL");
            HInput.AtLmb = true; t0 = runTime;
            while (runTime - t0 < 3.0f) { atMouse986 = P + new Vector2(2.5f, ((runTime - t0) < 1.5f ? -0.4f : 0.4f)); yield return ZF1618(1); }
            HInput.AtLmb = false; yield return ZF1618(30);
            ARes(scen + ":clip", maxP >= 2 && (_r1561 > 0 || _atRecFps <= 0), $"frames {_r1561} | corpse pile spawned (16 killed) | max Kindling patches {maxP} | FbGround {FbGround} FbTrail {FbTrail} | corpse max order {D2Feel1434.OrdCorpse + 568}");
            ZKill1618(L); T.kit1618 = kit0; _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---------------------------------------------------------------- t1643 clip: a FRESH save (pact keys deleted) -> menu -> altar (Zhurong unlocked + sealed, others locked) -> DESCEND -> run
        IEnumerator AtStartPact1643()
        {
            string scen = "startpact1643", clip = scen; yield return AtBegin(scen); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme;
            foreach (var k in new[] { "souls", "sigils", "path", "sel", "sealed", "mig539", "grant1643", "unlock0", "unlock1", "unlock2", "unlock3", "unlock4" }) PlayerPrefs.DeleteKey(PK + k);
            PlayerPrefs.Save(); System.Array.Clear(_unlock51, 0, _unlock51.Length); _souls51 = _sigils51 = 0; _pactPath51 = -1; _pactSel51 = 0; _pactSealed51 = false; // = a fresh boot (zeroed memory)
            _pactLoaded51 = false; PactLoad51(); _pactActive51 = false;
            ALog(scen + " debug: after load unlock " + string.Join(",", _unlock51) + " path " + _pactPath51 + " sealed " + _pactSealed51 + " grantKey " + PlayerPrefs.GetInt(PK + "grant1643", -1) + " toggle " + PT.startPactGrant1643 + " loaded " + _pactLoaded51);
            int root = AmRoot(0); bool zOk = _unlock51[0] == (1 << root) && _pactPath51 == 0 && _pactSealed51 && _pactSel51 == 0 && PlayerPrefs.GetInt(PK + "sealed", 0) == 1;
            bool othersLocked = true; for (int p = 1; p < NP97; p++) if (!PathLocked97(p)) othersLocked = false;
            int s0 = _souls51, g0 = _sigils51;
            _atGod = true; Manual716 = false; AtMenu1555(); yield return AtWait(0.5f);
            _r1561 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1561(1.0f, clip);
            Act69(0); bool altar = mode == Mode.Altar && _altarView51 == 0; yield return AtHold1561(1.6f, clip);
            for (int v = 1; v < NP97; v++) { _altarView51 = v; yield return AtHold1561(0.7f, clip); } // the other pacts: still locked
            _altarView51 = 0; yield return AtHold1561(1.0f, clip);
            var pc = AtPlayC1561(); yield return AtFake1561(pc.x, pc.y, clip);
            bool pick = mode == Mode.StartPick; string ids = AtCardIds1559(); yield return AtHold1561(1.2f, clip);
            int ci = cards.FindIndex(c => c.skill != null && c.skill.id == SkillId.Fireball); if (ci >= 0) Choose(cards[ci]);
            bool run = mode == Mode.Play && skills.Count == 1 && ZhOn1618; maxHp = hp = 100000; SnapCamera();
            yield return AtHold1561(1.5f, clip);
            ARes(scen + ":clip", zOk && othersLocked && altar && pick && run && _souls51 == s0 && _sigils51 == g0 && (_r1561 > 0 || _atRecFps <= 0),
                $"frames {_r1561} | Zhurong root {root} ({PactData.Nodes[0][root].name}) unlock {_unlock51[0]} path {_pactPath51} sealed {_pactSealed51} | others locked {othersLocked} | altar {altar} | pick {pick} [{ids}] | run {run} | souls {_souls51} sigils {_sigils51}");
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            AtClearWorld(); _atGod = false; AtEnd(scen, "hero_sprite_not_base_final2");
        }

        // ---------------------------------------------------------------- t1641 clip: bar with only the basic -> level-up picks fill the kit slots -> using them shows the sweeps
        IEnumerator AtSkillBarClip1641()
        {
            string scen = "skillbar1641", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball); T.kitSlotOverride1639 = false;
            Vector2 H0 = _heroPos; var L = new List<Enemy>();
            for (int i = 0; i < 6; i++) ZMk1618(L, EType.Imp, H0 + new Vector2(4.2f + (i % 3) * 0.8f, -0.8f + (i / 3) * 1.6f), 0f, 50000f);
            yield return ZF1618(3); atMouse986 = H0 + new Vector2(4.6f, 0f);
            HInput.AtLmb = true; yield return ZF1618(45); HInput.AtLmb = false; yield return ZF1618(10); // only the basic: LMB sweep, 3 locked slots
            var log = new System.Text.StringBuilder(); var picked = new List<int>();
            for (int lv = 0; lv < 3; lv++)
            {
                GainLevel(); int g = 0; while (mode != Mode.LevelUp && g++ < 200) yield return ZF1618(1);
                Card kc = null; for (int t = 0; t < 40; t++) { kc = cards.Find(c => c.kit > 0 && !c.kitLv1651); if (kc != null && (kc.kit != 3 || kc.kitPick == 1 || kc.kitPick == 4)) break; RollLevelCards(); } // ultimates with a pressable condition read best on film
                yield return ZF1618(40); if (kc == null) { log.Append(" no kit card"); break; }
                Choose(kc); picked.Add(kc.kit); log.Append(' ').Append(ZhKitName1639(kc)); yield return ZF1618(20);
                _heroPos = H0; SnapCamera(); atMouse986 = H0 + new Vector2(4.6f, 0f);
                if (kc.kit == 1) yield return ZDash1618(new Vector2(-1f, 0.3f).normalized);
                else if (kc.kit == 2) yield return ZPress1618(1);
                else { ZhSunMeter1618 = T.swMeter1618; ZhNearMiss1618 = T.vbNeed1618; yield return ZF1618(2); yield return ZPress1618(2); }
                HInput.AtLmb = true; yield return ZF1618(35); HInput.AtLmb = false; yield return ZF1618(10);
            }
            bool ok = picked.Count == 3 && ZhMove1618 > 0 && ZhSpec1618 > 0 && ZhUlt1618 > 0 && KitBar1641;
            ARes(scen + ":clip", ok && (_r1561 > 0 || _atRecFps <= 0), $"frames {_r1561} | picks{log} | cd move {ZhMoveCd:0.0} spec {ZhSpecCd:0.0} ult {ZhUltCd:0.0} | cell {Cell716(0)} {Cell716(1)} {Cell716(2)} {Cell716(3)}");
            ZKill1618(L); _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---------------------------------------------------------------- t1639 clip: a Zhurong run's level-ups offer only kit cards; a picked type is not offered again
        IEnumerator AtLevelUpClip1639()
        {
            string scen = "levelup1639", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball); T.kitSlotOverride1639 = false; // the new behaviour (default)
            bool startEmpty = ZhMove1618 == 0 && ZhSpec1618 == 0 && ZhUlt1618 == 0 && ZhRunMove1639 + ZhRunSpec1639 + ZhRunUlt1639 == 0;
            yield return ZF1618(25); var picked = new List<int>(); var log = new System.Text.StringBuilder(); bool ok = startEmpty;
            for (int lv = 0; lv < 3; lv++)
            {
                GainLevel(); int g = 0; while (mode != Mode.LevelUp && g++ < 200) yield return ZF1618(1);
                yield return ZF1618(50); // cards slide in and sit on screen
                Card kc = null; int other = 0; log.Append(" L").Append(lv + 1).Append(':');
                foreach (var c in cards) { if (c.kind == CardKind.NewSkill && c.kit == 0) other++; if (c.kit > 0 && !c.kitLv1651 /* t1651: owned kit skill level cards are allowed */) { kc = c; log.Append(' ').Append(ZhKitName1639(c)).Append('(').Append(c.kit).Append(')'); if (picked.Contains(c.kit)) ok = false; } }
                if (kc == null || other > 0) ok = false; if (kc == null) { log.Append(" no kit card"); break; }
                picked.Add(kc.kit); Choose(kc); yield return ZF1618(40); // the HUD readout now shows the new slot
            }
            ok &= ZhMove1618 > 0 && ZhSpec1618 > 0 && ZhUlt1618 > 0 && KitOffers1639().Count == 0;
            ARes(scen + ":clip", ok && (_r1561 > 0 || _atRecFps <= 0), $"frames {_r1561} | start slots empty {startEmpty} |{log} | after: {ZhMoveNames1618[ZhMove1618]} / {ZhSpecNames1618[ZhSpec1618]} / {ZhUltNames1618[ZhUlt1618]} | offers left {KitOffers1639().Count}");
            _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---------------------------------------------------------------- t1635 Kindling clip: Zhurong pact (Kindling node), Fireball into a slow pack, overlapping patches
        IEnumerator AtKindlingClip1635()
        {
            string scen = "kindling1635", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball); bool kit0 = T.kit1618; T.kit1618 = false; // kit off: only the pact patches on the ground
            var L = new List<Enemy>(); Vector2 H0 = _heroPos; int maxP = 0;
            for (int i = 0; i < 10; i++) ZMk1618(L, EType.Imp, H0 + new Vector2(3.6f + (i % 5) * 0.9f, -1.1f + (i / 5) * 2.2f), 0.05f, 70f); // small, slow, die in a few hits so the patches show
            yield return ZF1618(15); Toast("KINDLING (FIREBALL HITS)");
            HInput.AtLmb = true; float t0 = runTime;
            while (runTime - t0 < 4.5f) { atMouse986 = H0 + new Vector2(5.4f, Mathf.Sin((runTime - t0) * 1.6f) * 1.3f); yield return ZF1618(1); maxP = Mathf.Max(maxP, _patch53.Count); }
            HInput.AtLmb = false; yield return ZT1618(2.4f);
            ARes(scen + ":clip", (_r1561 > 0 || _atRecFps <= 0) && maxP >= 2, $"frames {_r1561} | Kindling node {Has51(PactPath.Pyromancer, 0)} | max patches alive {maxP} | shader {KdShader1635}");
            ZKill1618(L); T.kit1618 = kit0; _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---------------------------------------------------------------- in-game clips (-atRec 30)
        IEnumerator AtZhClip1618(int which)
        {
            string[] names = { "zhclip1618_basics", "zhclip1618_move", "zhclip1618_special", "zhclip1618_ult" };
            string scen = names[which], clip = scen; var T = HellordTuning.I; _zClip = clip;
            SkillId basic = which == 0 ? SkillId.Hydra : SkillId.Fireball;
            yield return ZhStart1618(scen, clip, basic);
            var L = new List<Enemy>(); Vector2 H0 = _heroPos; var sk = Basic1517();
            Enemy Pack(EType t, Vector2 at, float speed) => ZMk1618(L, t, at, speed, 600f);
            yield return ZF1618(15);
            if (which == 0)
            {   // Hydra: three placed in front of a pack, spitting at it; then Fire Wall across their path
                for (int i = 0; i < 10; i++) Pack(i % 3 == 0 ? EType.Brute : EType.Imp, H0 + new Vector2(5f + (i % 4) * 0.8f, -1.5f + (i / 4) * 1.2f), 0.6f);
                Vector2[] spots = { new Vector2(2.2f, 1.4f), new Vector2(2.6f, -1.6f), new Vector2(1.4f, 0f) };
                foreach (var sp in spots) { atMouse986 = H0 + sp; sk.cd = 0f; HInput.AtLmb = true; yield return ZF1618(2); HInput.AtLmb = false; yield return ZF1618(25); }
                yield return ZF1618(110);
                var fw = new SkillInst { def = HData.Skill(SkillId.FireWall) }; skills[0] = fw; Toast("FIRE WALL (own skill)");
                atMouse986 = H0 + new Vector2(3.2f, 0f); HInput.AtLmb = true; yield return ZF1618(3); HInput.AtLmb = false; yield return ZF1618(120);
                var fp = new SkillInst { def = HData.Skill(SkillId.FirePillar) }; skills[0] = fp; Toast("FIRE PILLAR");
                atMouse986 = H0 + new Vector2(5f, 1f); HInput.AtLmb = true; yield return ZF1618(70); HInput.AtLmb = false; yield return ZF1618(40);
                skills[0] = sk;
            }
            else if (which == 1)
            {   // Effigy (pack chases the decoy), Anchor (out and snap back), Backdraft (pull into a group, then a Fireball)
                T.kitMove1618 = 1; ZhMoveCd = 0; Toast("EMBER EFFIGY");
                for (int i = 0; i < 7; i++) Pack(EType.Imp, H0 + new Vector2(-4f + (i % 3) * 0.7f, -1.5f + (i / 3) * 1.1f), 1.8f);
                yield return ZF1618(30); yield return ZDash1618(Vector2.right); AtMove1396 = Vector2.right; yield return ZF1618(25); AtMove1396 = Vector2.zero; yield return ZF1618(90);
                ZKill1618(L); _heroPos = H0; SnapCamera(); T.kitMove1618 = 2; ZhMoveCd = 0; Toast("EMBER ANCHOR"); yield return ZF1618(25);
                for (int i = 0; i < 4; i++) Pack(EType.Imp, H0 + new Vector2(4f, -1.5f + i), 0.8f);
                yield return ZDash1618(Vector2.right); atMouse986 = H0 + new Vector2(4f, 0f); HInput.AtLmb = true; yield return ZF1618(40); HInput.AtLmb = false; _dashCd49 = 1f; yield return ZPress1618(0); yield return ZF1618(60);
                ZKill1618(L); _heroPos = H0; SnapCamera(); T.kitMove1618 = 3; ZhMoveCd = 0; Toast("BACKDRAFT"); yield return ZF1618(20);
                for (int i = 0; i < 8; i++) Pack(EType.Imp, H0 + new Vector2(Mathf.Cos(i * 0.785f) * 3.3f, Mathf.Sin(i * 0.785f) * 2.6f), 0.3f);
                yield return ZF1618(25); yield return ZDash1618(Vector2.left); yield return ZF1618(25); atMouse986 = H0; HInput.AtLmb = true; yield return ZF1618(40); HInput.AtLmb = false; yield return ZF1618(50);
            }
            else if (which == 2)
            {   // Pyre Mark -> payoff, Flammability, War Cry (powered basics + death blasts), Smolder Ring, Hearth Blessing
                ZhSpecCd = 0; T.kitSpecial1618 = 1; Toast("PYRE MARK");
                var big = Pack(EType.Brute, H0 + new Vector2(4.5f, 0.3f), 0f); Pack(EType.Imp, H0 + new Vector2(5.8f, -1.2f), 0f); Pack(EType.Imp, H0 + new Vector2(3.5f, 1.6f), 0f);
                atMouse986 = BodyCentre1595(big); yield return ZF1618(10); yield return ZPress1618(1); HInput.AtLmb = true; for (int f = 0; f < 150 && ZhPyrePays1618 == 0; f++) { atMouse986 = big.alive ? BodyCentre1595(big) : atMouse986; yield return ZF1618(1); } HInput.AtLmb = false; yield return ZF1618(40);
                ZKill1618(L); ZhSpecCd = 0; T.kitSpecial1618 = 2; Toast("FLAMMABILITY");
                Vector2 c = H0 + new Vector2(4.5f, 0f); for (int i = 0; i < 6; i++) Pack(EType.Imp, c + new Vector2((i % 3 - 1) * 0.9f, (i / 3) * 0.9f - 0.4f), 0f);
                atMouse986 = c; yield return ZPress1618(1); yield return ZT1618(T.flDelay1618 + 10f / 60f); HInput.AtLmb = true; yield return ZF1618(70); HInput.AtLmb = false; yield return ZF1618(30);
                ZKill1618(L); ZhSpecCd = 0; T.kitSpecial1618 = 3; Toast("WAR CRY"); for (int i = 0; i < 7; i++) Pack(EType.Imp, H0 + new Vector2(2.2f + (i % 3) * 0.7f, (i / 3) * 0.9f - 0.9f), 0.3f).hp = 60f;
                _heroPos = H0; yield return ZF1618(10); yield return ZPress1618(1); yield return ZF1618(15); atMouse986 = H0 + new Vector2(3f, 0f); HInput.AtLmb = true; yield return ZF1618(90); HInput.AtLmb = false; yield return ZF1618(30);
                ZKill1618(L); ZhSpecCd = 0; T.kitSpecial1618 = 4; Toast("SMOLDER RING"); c = H0 + new Vector2(4f, 0f);
                for (int i = 0; i < 6; i++) Pack(EType.Imp, c + new Vector2(2.5f + (i % 2) * 0.6f, (i / 2) * 1f - 1f), 1.6f);
                atMouse986 = c; yield return ZPress1618(1); yield return ZF1618(170);
                ZKill1618(L); ZhSpecCd = 0; T.kitSpecial1618 = 5; Toast("HEARTH BLESSING"); _atGod = false; maxHp = 1000; hp = 450; ApplyFrostSlow37(4f); yield return ZF1618(20); yield return ZPress1618(1); yield return ZF1618(130); hp = maxHp = 100000; _atGod = true;
            }
            else
            {   // Sun's Wrath, Rebirth Flame, Molten Heart, Vermilion Bird
                ZhUltCd = 0; T.kitUlt1618 = 1; Toast("SUN'S WRATH"); Vector2 c = H0 + new Vector2(5f, 0f);
                for (int i = 0; i < 9; i++) Pack(i % 4 == 0 ? EType.Brute : EType.Imp, c + new Vector2((i % 3 - 1) * 1.0f, (i / 3 - 1) * 0.9f), 0.2f);
                foreach (var e in L) Hit(e, 20f, sk, Vector2.zero); yield return ZF1618(50); ZhSunMeter1618 = T.swMeter1618; atMouse986 = c; yield return ZPress1618(2); yield return ZF1618(120);
                ZKill1618(L); ZhUltCd = 0; T.kitUlt1618 = 2; Toast("REBIRTH FLAME"); for (int i = 0; i < 8; i++) Pack(EType.Imp, H0 + new Vector2(Mathf.Cos(i * 0.785f) * 2.4f, Mathf.Sin(i * 0.785f) * 1.8f), 0f);
                _heroPos = H0; yield return ZF1618(20); _atGod = false; maxHp = 1000; hp = 20; _iframe = 0; HurtHero(300f, "clip"); yield return ZF1618(90); hp = maxHp = 100000; _atGod = true;
                ZKill1618(L); ZhUltCd = 0; T.kitUlt1618 = 3; Toast("MOLTEN HEART"); c = H0 + new Vector2(5f, 0f);
                for (int i = 0; i < 16; i++) Pack(EType.Imp, c + new Vector2((i % 4) * 0.8f - 1.2f, (i / 4) * 0.8f - 1.2f), 0f);
                foreach (var e in L) ApplyStatus(e, Status.Burn, 6f, 1f, sk); yield return ZF1618(15); yield return ZPress1618(2); atMouse986 = c; HInput.AtLmb = true; yield return ZF1618(110); HInput.AtLmb = false; yield return ZF1618(10);
                ZKill1618(L); ZhUltCd = 0; T.kitUlt1618 = 4; ZhNearMiss1618 = T.vbNeed1618; Toast("VERMILION BIRD"); _heroPos = H0 + new Vector2(-4f, 0f); SnapCamera();
                for (int i = 0; i < 7; i++) Pack(EType.Imp, _heroPos + new Vector2(2.5f + i * 1.4f, (i % 2) * 0.5f - 0.2f), 0.2f);
                yield return ZF1618(20); atMouse986 = _heroPos + new Vector2(6f, 0f); yield return ZPress1618(2); yield return ZF1618(120);
            }
            ARes(scen + ":clip", _r1561 > 0 || _atRecFps <= 0, $"frames {_r1561}");
            _zClip = null; ZhEnd1618(scen, clip);
        }
    }
}
#endif
