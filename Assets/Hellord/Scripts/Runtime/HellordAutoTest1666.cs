#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1666 Leizu kit clips (run with -atRec 30) + functional checks (PASS / FAIL per skill from the LzXxx1666 counters):
    /// lzclip1666_basics (Drum Strikes, Thunder Lash, Live Wire, Thunder Hound), lzclip1666_move (Thunder Step, Grounding Ward, Storm Stride, Mirror Flash),
    /// lzclip1666_special (Mark of Judgement, Conductivity, Thunder Ministry Drum, Static Field), lzclip1666_ult (Decree, Heaven-Splitting Arc, Tribunal, Judgement Warp).
    /// Starts a Zhurong test run (ZhStart1618), then switches to a Leizu run (pact path 2) with the testing slot override (lzMove / lzSpecial / lzUlt1666).</summary>
    public partial class HellordGame
    {
        int _lzM0, _lzS0, _lzU0;
        IEnumerator AtLzClip1666(int which)
        {
            string[] names = { "lzclip1666_basics", "lzclip1666_move", "lzclip1666_special", "lzclip1666_ult" };
            string scen = names[which], clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball);
            _lzM0 = T.lzMove1666; _lzS0 = T.lzSpecial1666; _lzU0 = T.lzUlt1666; _pactPath51 = 2; T.kitSlotOverride1639 = true; T.lzMove1666 = T.lzSpecial1666 = T.lzUlt1666 = 0; ClearLz1666(); LzRunReset1666();
            var L = new List<Enemy>(); Vector2 H0 = _heroPos;
            Enemy Pack(EType t, Vector2 at, float speed, float hpv = 600f) => ZMk1618(L, t, at, speed, hpv);
            SkillInst Basic(SkillId id, string name) { var s = new SkillInst { def = HData.Skill(id) }; if (skills.Count > 0) skills[0] = s; else skills.Add(s); Toast(name); return s; }
            IEnumerator LF(int n) { for (int i = 0; i < n; i++) { yield return ZF1618(1); pendingLevels = 0; if (mode != Mode.Play) mode = Mode.Play; foreach (var p in _pk) if (p.alive) { var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j); p.alive = false; p.sr.gameObject.SetActive(false); } } } // no level-up screen / pickups mid-clip
            IEnumerator LT(float sec) { float t0 = runTime; int g = 0; while (runTime - t0 < sec && g++ < 3000) yield return LF(1); }
            IEnumerator Hold(int n) { HInput.AtLmb = true; yield return LF(n); HInput.AtLmb = false; }
            void Res(string k, bool ok, string d) => ARes(scen + ":" + k, ok, d);
            bool on = LzOn1666; Res("active", on, $"LzOn1666 {on} (kit1618 {T.kit1618}, lzKit1666 {T.lzKit1666}, path {_pactPath51})");
            yield return LF(15);
            if (which == 0)
            {
                // Thunder Drum Strikes: beat, beat, Thunderclap
                Basic(SkillId.ThunderDrumStrikes, "THUNDER DRUM STRIKES"); for (int i = 0; i < 8; i++) Pack(i % 3 == 0 ? EType.Brute : EType.Imp, H0 + new Vector2(4f + (i % 3) * 0.7f, -1f + (i / 3) * 0.9f), 0.2f);
                atMouse986 = H0 + new Vector2(4.7f, 0f); int b0 = LzBeats1666, c0 = LzClaps1666; yield return Hold(95); yield return LF(30);
                Res("drumstrikes", LzBeats1666 - b0 >= 5 && LzClaps1666 - c0 >= 1, $"beats {LzBeats1666 - b0}, claps {LzClaps1666 - c0}");
                // Thunder Lash: shocked demons are knocked back
                ZKill1618(L); Basic(SkillId.ThunderLash, "THUNDER LASH"); for (int i = 0; i < 6; i++) { var e = Pack(EType.Imp, H0 + new Vector2(1.6f + (i % 2) * 0.5f, -1f + (i / 2) * 1f), 0f); }
                yield return LF(10); foreach (var e in L) if (L.IndexOf(e) % 2 == 0) ApplyStatus(e, Status.Shock, 1f, 1f, skills[0]);
                atMouse986 = H0 + new Vector2(3f, 0f); int l0 = LzLashHits1666, k0 = LzLashKnocks1666; yield return Hold(60); yield return LF(25);
                Res("thunderlash", LzLashHits1666 - l0 > 0 && LzLashKnocks1666 - k0 > 0, $"hits {LzLashHits1666 - l0}, knockbacks on shocked {LzLashKnocks1666 - k0}");
                // Live Wire: held beam, overload after 1 s on one target, hero slowed while held
                ZKill1618(L); Basic(SkillId.LiveWire, "LIVE WIRE"); var big = Pack(EType.Brute, H0 + new Vector2(4.5f, 0.2f), 0f, 3000f); Pack(EType.Imp, H0 + new Vector2(3f, -2.2f), 0.5f);
                atMouse986 = BodyCentre1595(big); int w0 = LzWireTicks1666, o0 = LzWireOverloads1666; float mm = 1f; HInput.AtLmb = true; AtMove1396 = Vector2.up * 0.3f;
                for (int f = 0; f < 100; f++) { yield return LF(1); if (big.alive) atMouse986 = BodyCentre1595(big); mm = Mathf.Min(mm, LzMoveMul1666()); }
                HInput.AtLmb = false; AtMove1396 = Vector2.zero; yield return LF(30); _heroPos = H0;
                Res("livewire", LzWireTicks1666 - w0 >= 8 && LzWireOverloads1666 - o0 > 0 && mm < 0.99f, $"ticks {LzWireTicks1666 - w0}, overload ticks {LzWireOverloads1666 - o0}, move x{mm:0.##} while held");
                // Thunder Hound: summons at the cursor, bite demons in its leash
                ZKill1618(L); Basic(SkillId.ThunderHound, "THUNDER HOUND"); for (int i = 0; i < 6; i++) Pack(EType.Imp, H0 + new Vector2(4f + (i % 3) * 1f, -1.2f + (i / 3) * 2f), 0.3f);
                int h0 = LzHoundsMade1666, bi0 = LzBites1666; atMouse986 = H0 + new Vector2(4.5f, 0f); yield return Hold(2); yield return LF(35); atMouse986 = H0 + new Vector2(5.5f, -0.5f); skills[0].cd = 0f; yield return Hold(2); yield return LF(90);
                Res("thunderhound", LzHoundsMade1666 - h0 >= 2 && LzBites1666 - bi0 >= 3, $"hounds {LzHoundsMade1666 - h0}, bites {LzBites1666 - bi0}");
            }
            else if (which == 1)
            {
                Basic(SkillId.ThunderDrumStrikes, "LEIZU MOVEMENT");
                // Thunder Step: instant blink (+ snap into a Ball Lightning orb)
                T.lzMove1666 = 1; ZhMoveCd = 0; Toast("THUNDER STEP"); int bl0 = LzBlinks1666; Vector2 p0 = _heroPos; yield return ZDash1618(Vector2.right); float d1 = (_heroPos - p0).magnitude; yield return LF(25);
                var orb = new SkillInst { def = HData.Skill(SkillId.BallLightning) }; skills[0] = orb; atMouse986 = _heroPos + new Vector2(-4f, 1f); yield return Hold(2); yield return LF(40);
                Vector2 op = _pbOrbs.Count > 0 ? _pbOrbs[_pbOrbs.Count - 1].p : Vector2.zero; atMouse986 = op; ZhMoveCd = 0; AtMove1396 = Vector2.left; _dashCd49 = 0f; yield return ZPress1618(0); AtMove1396 = Vector2.zero;
                float snap = 99f; foreach (var o in _pbOrbs) snap = Mathf.Min(snap, (LzBlinkTo1666 - o.p).magnitude); yield return LF(40); /* landed on the orb's ground point (the orb keeps drifting after) */
                Res("thunderstep", LzBlinks1666 - bl0 >= 2 && d1 > T49.dashDistU49 * 1.1f && snap < 1f, $"blinks {LzBlinks1666 - bl0}, first blink {d1:0.##} u (dash {T49.dashDistU49:0.##}), orb snap off by {snap:0.##} u");
                Basic(SkillId.ThunderDrumStrikes, "GROUNDING WARD"); _heroPos = H0; SnapCamera();
                // Grounding Ward: next 2 hits absorbed, attackers shocked
                T.lzMove1666 = 2; ZhMoveCd = 0; var a1 = Pack(EType.Imp, H0 + new Vector2(1.2f, 0.3f), 0f); var a2 = Pack(EType.Imp, H0 + new Vector2(-1.2f, -0.3f), 0f); yield return ZDash1618(Vector2.up); _heroPos = H0;
                int ab0 = LzWardAbsorbs1666; float hp0 = hp; _atGod = false; foreach (var a in new[] { a1, a2 }) { _iframe = 0f; _hurtBy53 = a; HurtHero(10f, "clip", false, true); yield return LF(20); }
                _atGod = true; bool shocked = a1.shockT > 0f && a2.shockT > 0f; Res("groundingward", LzWardAbsorbs1666 - ab0 == 2 && shocked, $"absorbed {LzWardAbsorbs1666 - ab0}, attackers shocked {shocked}"); yield return LF(30);
                // Storm Stride: faster moves + basics after a dash
                ZKill1618(L); T.lzMove1666 = 3; ZhMoveCd = 0; Toast("STORM STRIDE"); for (int i = 0; i < 6; i++) Pack(EType.Imp, H0 + new Vector2(5f + (i % 2), -1f + (i / 2) * 1f), 0f);
                yield return ZDash1618(Vector2.left); float st = LzStrideT1666, mv = LzMoveMul1666(), ck = LzBasicCdK1666(skills[0]); atMouse986 = H0 + new Vector2(5.5f, 0f); AtMove1396 = Vector2.right * 0.5f; yield return Hold(60); AtMove1396 = Vector2.zero; yield return LF(20);
                Res("stormstride", st > 0f && mv > 1.01f && ck < 0.99f, $"buff {st:0.##} s, move x{mv:0.##}, basic cooldown x{ck:0.##}");
                // Mirror Flash: demons near the dash start are dazzled
                ZKill1618(L); _heroPos = H0; SnapCamera(); T.lzMove1666 = 4; ZhMoveCd = 0; Toast("MIRROR FLASH"); for (int i = 0; i < 7; i++) Pack(EType.Imp, H0 + new Vector2(Mathf.Cos(i * 0.9f) * 2.2f, Mathf.Sin(i * 0.9f) * 1.6f), 1.2f);
                yield return LF(20); int dz0 = LzDazzled1666; yield return ZDash1618(Vector2.down); yield return LF(60);
                Res("mirrorflash", LzDazzled1666 - dz0 >= 4, $"dazzled {LzDazzled1666 - dz0}");
            }
            else if (which == 2)
            {
                var sk = Basic(SkillId.ThunderDrumStrikes, "LEIZU SPECIALS");
                // Mark of Judgement: lightning on the marked demon charges it -> bolt + arcs to shocked demons
                T.lzSpecial1666 = 1; ZhSpecCd = 0; Toast("MARK OF JUDGEMENT"); var big = Pack(EType.Brute, H0 + new Vector2(4.5f, 0.3f), 0f, 900f); var o1 = Pack(EType.Imp, H0 + new Vector2(5.8f, -1.4f), 0f); var o2 = Pack(EType.Imp, H0 + new Vector2(3.4f, 1.7f), 0f);
                atMouse986 = big.pos; yield return LF(10); yield return ZPress1618(1); int mp0 = LzMarkPays1666; bool onBig = LzMark1666 == big; ApplyStatus(o1, Status.Shock, 1f, 1f, sk); ApplyStatus(o2, Status.Shock, 1f, 1f, sk);
                HInput.AtLmb = true; for (int f = 0; f < 400 && LzMarkPays1666 == mp0; f++) { atMouse986 = big.alive ? big.pos : atMouse986; if (f % 60 == 0) { if (o1.alive) ApplyStatus(o1, Status.Shock, 1f, 1f, sk); if (o2.alive) ApplyStatus(o2, Status.Shock, 1f, 1f, sk); } yield return LF(1); } HInput.AtLmb = false; yield return LF(40);
                Res("markofjudgement", onBig && LzMarkPays1666 - mp0 == 1, $"marked the demon under the cursor {onBig}, payoffs {LzMarkPays1666 - mp0}");
                // Conductivity: delayed curse; cursed demons take more shock damage, shock lasts longer and jumps
                ZKill1618(L); ZhSpecCd = 0; T.lzSpecial1666 = 2; Toast("CONDUCTIVITY"); Vector2 c = H0 + new Vector2(4.5f, 0f); for (int i = 0; i < 6; i++) Pack(EType.Imp, c + new Vector2((i % 3 - 1) * 0.9f, (i / 3) * 1.1f - 0.5f), 0f);
                atMouse986 = c; int cu0 = LzCursed1666, sp0 = LzSpreads1666; yield return ZPress1618(1); yield return LT(T.cdDelay1666 + 0.2f);
                float amp = 0f; skills[0].cd = 0f; atMouse986 = L[0].pos; yield return Hold(2); foreach (var e in L) if (e.alive && e.shockT > 0f) amp = Mathf.Max(amp, e.shockAmp); yield return LF(60);
                Res("conductivity", LzCursed1666 - cu0 >= 5 && amp >= T.cdAmp1666 - 0.01f && LzSpreads1666 - sp0 > 0, $"cursed {LzCursed1666 - cu0}, shock amp {amp:0.##}, spreads {LzSpreads1666 - sp0}");
                // Thunder Ministry Drum: lightning aimed at / hitting near the drum booms it
                ZKill1618(L); ZhSpecCd = 0; T.lzSpecial1666 = 3; Toast("THUNDER MINISTRY DRUM"); c = H0 + new Vector2(4f, 0f); for (int i = 0; i < 7; i++) Pack(EType.Imp, c + new Vector2(Mathf.Cos(i * 0.9f) * 1.8f, Mathf.Sin(i * 0.9f) * 1.3f), 0.2f);
                atMouse986 = c; int db0 = LzDrumBooms1666; yield return ZPress1618(1); yield return LF(15); atMouse986 = c + Vector2.up * 0.5f; yield return Hold(100); yield return LF(40);
                Res("thunderdrum", LzDrumBooms1666 - db0 >= 2, $"booms {LzDrumBooms1666 - db0}");
                // Static Field: share of current HP, never kills (floor 40%)
                ZKill1618(L); ZhSpecCd = 0; T.lzSpecial1666 = 4; Toast("STATIC FIELD"); _heroPos = H0; var sl = new List<Enemy>(); for (int i = 0; i < 7; i++) sl.Add(Pack(i == 0 ? EType.Brute : EType.Imp, H0 + new Vector2(Mathf.Cos(i * 0.9f) * 1.9f, Mathf.Sin(i * 0.9f) * 1.4f), 0f, 100f));
                sl[0].elite = true; yield return LF(10); int sh0 = LzStaticHits1666; var hp0s = new List<float>(); foreach (var e in sl) hp0s.Add(e.hp); yield return ZPress1618(1); yield return LF(20);
                float n0 = sl[1].hp; for (int r = 0; r < 4; r++) { ZhSpecCd = 0; yield return ZPress1618(1); yield return LF(15); }
                bool alive = true; float minF = 1f; foreach (var e in sl) { alive &= e.alive; minF = Mathf.Min(minF, e.hp / e.maxHp); }
                Res("staticfield", LzStaticHits1666 - sh0 >= 7 && alive && minF >= T.sfFloor1666 - 0.001f && Mathf.Abs(n0 - hp0s[1] * (1f - T.sfNormal1666)) < 0.5f, $"hits {LzStaticHits1666 - sh0}, all alive {alive}, lowest {minF:0.###} of max (floor {T.sfFloor1666}), first pulse normal {hp0s[1]:0}->{n0:0.#}, elite {hp0s[0]:0}->{sl[0].hp:0.#}"); yield return LF(30);
            }
            else
            {
                var sk = Basic(SkillId.ThunderDrumStrikes, "LEIZU ULTIMATES");
                // Decree: real combos (basic hit + movement + special inside the window) x2, then 24 bolts
                T.lzUlt1666 = 1; T.lzMove1666 = 3; T.lzSpecial1666 = 4; ZhUltCd = 0; Toast("DECREE OF THE 24 THUNDER LORDS"); Vector2 c = H0 + new Vector2(4.5f, 0f);
                for (int i = 0; i < 14; i++) Pack(i % 5 == 0 ? EType.Brute : EType.Imp, H0 + new Vector2(Mathf.Cos(i * 0.45f) * (2.5f + (i % 3)), Mathf.Sin(i * 0.45f) * (1.8f + (i % 3) * 0.6f)), 0.1f, 2000f);
                for (int r = 0; r < 2; r++) { atMouse986 = L[r].pos; skills[0].cd = 0f; yield return Hold(2); ZhMoveCd = 0; yield return ZDash1618(r == 0 ? Vector2.up : Vector2.down); _heroPos = H0; ZhSpecCd = 0; yield return ZPress1618(1); yield return LF(20); }
                int cb = LzCombos1666; bool armed = LzUltArmed1666; int db0 = LzDecreeBolts1666; yield return ZPress1618(2); yield return LF(110);
                Res("decree", cb == T.dcNeed1666 && armed && LzDecreeBolts1666 - db0 == T.dcBolts1666, $"combos {cb}, armed {armed}, bolts {LzDecreeBolts1666 - db0}");
                // Heaven-Splitting Arc: infusions drop from shocked elites -> pick 3 -> 12-chain arc + shocked ground
                ZKill1618(L); _heroPos = H0; SnapCamera(); T.lzUlt1666 = 2; ZhUltCd = 0; Toast("HEAVEN-SPLITTING ARC");
                var els = new List<Enemy>(); for (int i = 0; i < 3; i++) { var e = Pack(EType.Imp, H0 + new Vector2(2f + i * 1.5f, -1.5f), 0f, 30f); e.elite = true; els.Add(e); }
                yield return LF(10); foreach (var e in els) { ApplyStatus(e, Status.Shock, 1f, 1f, sk); Hit(e, 1000f, sk, Vector2.zero); } yield return LF(30); _chestAlive = false; if (_chestSr != null) _chestSr.gameObject.SetActive(false);
                int dropped = _lzInf1666.Count; var pts = new List<Vector2>(); foreach (var f in _lzInf1666) pts.Add(f.p); foreach (var p in pts) { _heroPos = p; yield return LF(12); } _heroPos = H0; yield return LF(15);
                int inf = LzInfusions1666; for (int i = 0; i < 12; i++) Pack(EType.Imp, H0 + new Vector2(3f + (i % 4) * 1.6f, -2f + (i / 4) * 1.8f), 0f, 2000f); yield return LF(10);
                int ah0 = LzArcHits1666; atMouse986 = L[L.Count - 12].pos; yield return ZPress1618(2); int gr = _lzGround1666.Count; yield return LF(90);
                Res("heavenarc", dropped == 3 && inf == T.haNeed1666 && LzArcHits1666 - ah0 == T.haChains1666 && gr > 0, $"drops {dropped}, infusions {inf}, chain hits {LzArcHits1666 - ah0}, shocked ground {gr}");
                // Tribunal: energy from shocking demons -> bolts on the strongest shocked demon
                ZKill1618(L); _heroPos = H0; ZhUltCd = 0; T.lzUlt1666 = 3; Toast("TRIBUNAL OF THE NINE HEAVENS"); for (int i = 0; i < 9; i++) Pack(i == 4 ? EType.Brute : EType.Imp, H0 + new Vector2(3.5f + (i % 3) * 1f, -1f + (i / 3) * 1f), 0f, 3000f);
                float en0 = LzEnergy1666; int sh = 0; while (LzEnergy1666 < T.trNeed1666 && sh++ < 400) foreach (var e in L) ApplyStatus(e, Status.Shock, 1f, 1f, sk); float en1 = LzEnergy1666; yield return LF(10);
                int tb0 = LzTribBolts1666; yield return ZPress1618(2); yield return LT(T.trDur1666 + 0.3f);
                Res("tribunal", en1 >= T.trNeed1666 && LzTribBolts1666 - tb0 >= Mathf.FloorToInt(T.trDur1666 / T.trEvery1666) - 1, $"energy {en0:0}->{en1:0} after {sh} shock rounds, bolts {LzTribBolts1666 - tb0}");
                // Judgement Warp: a cull-ready elite -> teleport in, it explodes
                ZKill1618(L); _heroPos = H0 + new Vector2(-3f, 0f); SnapCamera(); ZhUltCd = 0; T.lzUlt1666 = 4; Toast("JUDGEMENT WARP");
                var el = Pack(EType.Brute, H0 + new Vector2(4f, 0.4f), 0f, 1000f); el.elite = true; for (int i = 0; i < 6; i++) Pack(EType.Imp, el.pos + new Vector2(Mathf.Cos(i * 1.05f) * 1.6f, Mathf.Sin(i * 1.05f) * 1.2f), 0f, 2000f);
                yield return LF(15); bool notYet = !LzUltArmed1666; el.hp = el.maxHp * (T.jwCull1666 - 0.05f); yield return LF(25); bool armedW = LzUltArmed1666; int wp0 = LzWarps1666; Vector2 hb = _heroPos;
                yield return ZPress1618(2); float moved = (_heroPos - hb).magnitude; yield return LF(90); _chestAlive = false; if (_chestSr != null) _chestSr.gameObject.SetActive(false);
                Res("judgementwarp", notYet && armedW && LzWarps1666 - wp0 == 1 && !el.alive && moved > 3f, $"armed only below {T.jwCull1666:0.##} ({notYet}/{armedW}), warps {LzWarps1666 - wp0}, elite dead {!el.alive}, hero moved {moved:0.#} u");
            }
            ARes(scen + ":clip", _r1561 > 0 || _atRecFps <= 0, $"frames {_r1561}");
            ZKill1618(L); ClearLz1666(); LzRunReset1666(); T.lzMove1666 = _lzM0; T.lzSpecial1666 = _lzS0; T.lzUlt1666 = _lzU0;
            _zClip = null; ZhEnd1618(scen, clip);
        }
    }
}
#endif
