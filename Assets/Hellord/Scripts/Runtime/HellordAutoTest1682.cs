#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;
namespace Hellord
{
    /// <summary>t1682 Gonggong kit: clips (run with -atRec 30) + functional checks (PASS / FAIL per skill from the GgXxx1682 counters):
    /// ggclip1682_basics (Tidal Surge, Whirlpool, Breath of the Deep, Buzhou Rift, Eye of the Black Waters), ggclip1682_move (Ride the Flood, Rime Coat, Floodgate, Frozen River),
    /// ggclip1682_special (Drowning Mark, Hypothermia, Shatter, Toppled Pillar, Frost Wall), ggclip1682_ult (Ram of Mount Buzhou, The Great Flood, Eternal Winter, Pillars of Ao).
    /// Starts a Zhurong test run (ZhStart1618), then switches to a Gonggong run (pact path 4) with the testing slot override (ggMove / ggSpecial / ggUlt1682).
    /// Also the sup1651 kit hooks (SmGgCast1682 / SmGgProbe1682): each kit skill cast with its condition pre-met.</summary>
    public partial class HellordGame
    {
        int _ggM0, _ggS0, _ggU0;
        IEnumerator AtGgClip1682(int which)
        {
            string[] names = { "ggclip1682_basics", "ggclip1682_move", "ggclip1682_special", "ggclip1682_ult" };
            string scen = names[which], clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Frostbolt);
            _ggM0 = T.ggMove1682; _ggS0 = T.ggSpecial1682; _ggU0 = T.ggUlt1682; _pactPath51 = 4; T.kitSlotOverride1639 = true; T.ggMove1682 = T.ggSpecial1682 = T.ggUlt1682 = 0; ClearGg1682(); GgRunReset1682();
            var L = new List<Enemy>(); Vector2 H0 = _heroPos; ZhFxLogOn1625 = true; ZhFxLogReset1625(); int q0 = ZhFxQuads1625; // which GonggongFX effects drew
            IEnumerator Ms(int n, float[] o) { var sw = System.Diagnostics.Stopwatch.StartNew(); int qa = ZhFxQuads1625; yield return LF(n); o[0] = (float)sw.Elapsed.TotalMilliseconds / n; o[1] = (ZhFxQuads1625 - qa) / (float)n; }
            Enemy Pack(EType t, Vector2 at, float speed, float hpv = 600f) => ZMk1618(L, t, at, speed, hpv);
            SkillInst Basic(SkillId id, string name) { var s = new SkillInst { def = HData.Skill(id) }; if (skills.Count > 0) skills[0] = s; else skills.Add(s); Toast(name); return s; }
            IEnumerator LF(int n) { for (int i = 0; i < n; i++) { yield return ZF1618(1); pendingLevels = 0; if (mode != Mode.Play) mode = Mode.Play; foreach (var p in _pk) if (p.alive) { var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j); p.alive = false; p.sr.gameObject.SetActive(false); } } }
            IEnumerator Hold(int n) { HInput.AtLmb = true; yield return LF(n); HInput.AtLmb = false; }
            void Res(string k, bool ok, string d) => ARes(scen + ":" + k, ok, d);
            void Reset() { ZKill1618(L); ClearGg1682(); _heroPos = H0; SnapCamera(); ZhMoveCd = ZhSpecCd = ZhUltCd = 0f; HInput.AtLmb = false; }
            bool on = GgOn1682; Res("active", on, $"GgOn1682 {on} (kit1618 {T.kit1618}, ggKit1682 {T.ggKit1682}, path {_pactPath51})");
            yield return LF(15);
            if (which == 0)
            {
                // Tidal Surge: the wave carries the demons in front of it, then crashes (chill)
                Basic(SkillId.TidalSurge, "TIDAL SURGE"); for (int i = 0; i < 6; i++) Pack(EType.Imp, H0 + new Vector2(2f + (i % 2) * 0.6f, -0.9f + (i / 2) * 0.9f), 0f);
                yield return LF(5); float x0 = 0f; foreach (var e in L) x0 += e.pos.x; int ca0 = GgSurgeCarried1682, cr0 = GgSurgeCrashes1682; atMouse986 = H0 + new Vector2(6f, 0f); skills[0].cd = 0f; yield return Hold(3); yield return LF(40);
                float x1 = 0f; int ch = 0; foreach (var e in L) { x1 += e.pos.x; if (e.chillT > 0f || e.freezeT > 0f) ch++; }
                Res("tidalsurge", GgSurgeCarried1682 - ca0 >= 4 && GgSurgeCrashes1682 - cr0 == 1 && (x1 - x0) / L.Count > 1.5f && ch >= 4, $"carried {GgSurgeCarried1682 - ca0}, crashes {GgSurgeCrashes1682 - cr0}, mean push {(x1 - x0) / L.Count:0.##} u, chilled {ch}/{L.Count}");
                yield return Hold(80);
                // Whirlpool: pulls demons in, collapses; at most 2 at once
                Reset(); Basic(SkillId.Whirlpool, "WHIRLPOOL"); Vector2 c = H0 + new Vector2(4f, 0f); for (int i = 0; i < 8; i++) Pack(EType.Imp, c + new Vector2(Mathf.Cos(i * 0.785f) * 2.2f, Mathf.Sin(i * 0.785f) * 1.6f), 0f);
                yield return LF(5); float d0 = 0f; foreach (var e in L) d0 += (e.pos - c).magnitude; int co0 = GgPoolCollapses1682; atMouse986 = c; skills[0].cd = 0f; yield return Hold(3); yield return LF(20);
                float d1 = 0f; foreach (var e in L) d1 += (e.pos - c).magnitude; yield return LF(20); int maxPools = 0; yield return null;
                HInput.AtLmb = true; for (int f = 0; f < 90; f++) { yield return LF(1); maxPools = Mathf.Max(maxPools, _ggPools1682.Count); } HInput.AtLmb = false; yield return LF(30);
                Res("whirlpool", GgPoolCollapses1682 - co0 >= 2 && d1 < d0 * 0.75f && maxPools <= T.wpMax1682, $"collapses {GgPoolCollapses1682 - co0}, mean dist to centre {d0 / L.Count:0.##} -> {d1 / L.Count:0.##} u, most alive {maxPools} (cap {T.wpMax1682})");
                // Breath of the Deep: held cone; a demon held 1 s freezes; hero slowed while holding
                Reset(); Basic(SkillId.BreathOfTheDeep, "BREATH OF THE DEEP"); for (int i = 0; i < 5; i++) Pack(EType.Imp, H0 + new Vector2(2.2f + (i % 3) * 0.6f, -0.5f + (i / 3) * 1f), 0f);
                atMouse986 = H0 + new Vector2(4f, 0f); yield return LF(5); int bt0 = GgBreathTicks1682, bf0 = GgBreathFreezes1682; float mm = 1f; HInput.AtLmb = true; for (int f = 0; f < 60; f++) { yield return LF(1); mm = Mathf.Min(mm, GgMoveMul1682()); } HInput.AtLmb = false; yield return LF(30);
                Res("breath", GgBreathTicks1682 - bt0 >= 7 && GgBreathFreezes1682 - bf0 >= 3 && Mathf.Abs(mm - T.brMove1682) < 0.01f, $"ticks {GgBreathTicks1682 - bt0}, freezes {GgBreathFreezes1682 - bf0}, hero move x{mm:0.##} while holding");
                // Buzhou Rift: 6 bursts then the spike; the spike consumes a freeze for x3
                Reset(); Basic(SkillId.BuzhouRift, "BUZHOU RIFT"); var big = Pack(EType.Brute, H0 + new Vector2(5f, 0f), 0f, 5000f); for (int i = 0; i < 4; i++) Pack(EType.Imp, H0 + new Vector2(1.2f + i * 0.9f, (i % 2 - 0.5f) * 0.6f), 0f);
                yield return LF(5); IceFreeze(big, 3f); float bh0 = big.hp; int rb0 = GgRiftBursts1682, rs0 = GgRiftSpikes1682, rz0 = GgRiftShatters1682; atMouse986 = big.pos; skills[0].cd = 0f; yield return Hold(3); yield return LF(30);
                Res("buzhourift", GgRiftBursts1682 - rb0 == T.rfBursts1682 && GgRiftSpikes1682 - rs0 == 1 && GgRiftShatters1682 - rz0 == 1 && big.freezeT <= 0f && bh0 - big.hp > 0f, $"bursts {GgRiftBursts1682 - rb0}, spikes {GgRiftSpikes1682 - rs0}, shattered freezes {GgRiftShatters1682 - rz0}, brute -{bh0 - big.hp:0.#} HP, still frozen {big.freezeT > 0f}");
                yield return Hold(60);
                // Eye of the Black Waters: spiral shards; at most 2 orbs
                Reset(); Basic(SkillId.EyeOfTheBlackWaters, "EYE OF THE BLACK WATERS"); for (int i = 0; i < 10; i++) Pack(EType.Imp, H0 + new Vector2(3.5f + Mathf.Cos(i * 0.63f) * 2.2f, Mathf.Sin(i * 0.63f) * 1.8f), 0f);
                atMouse986 = H0 + new Vector2(4f, 0f); yield return LF(5); int es0 = GgEyeShards1682, eh0 = GgEyeHits1682, maxEyes = 0; HInput.AtLmb = true; for (int f = 0; f < 150; f++) { yield return LF(1); maxEyes = Mathf.Max(maxEyes, _ggEyes1682.Count); } HInput.AtLmb = false; yield return LF(40);
                Res("eye", GgEyeShards1682 - es0 >= 16 && GgEyeHits1682 - eh0 >= 4 && maxEyes <= T.eyeMax1682, $"shards {GgEyeShards1682 - es0}, shard hits {GgEyeHits1682 - eh0}, most orbs {maxEyes} (cap {T.eyeMax1682})");
            }
            else if (which == 1)
            {
                Basic(SkillId.Frostbolt, "GONGGONG MOVEMENT"); _atGod = false;
                // Ride the Flood: a longer, steered dash; chill cleared
                T.ggMove1682 = 1; Toast("RIDE THE FLOOD"); _chill44 = 2; atMouse986 = H0 + new Vector2(10f, 6f); /* far cursor: the ride curves toward it */ int r0 = GgRides1682; Vector2 hb = _heroPos; yield return ZDash1618(Vector2.right); yield return LF(15);
                Vector2 moved = _heroPos - hb; Res("ridetheflood", GgRides1682 - r0 == 1 && moved.magnitude > T49.dashDistU49 * 1.2f && moved.y > 0.8f && _chill44 == 0, $"rides {GgRides1682 - r0}, moved ({moved.x:0.#},{moved.y:0.#}) vs plain dash {T49.dashDistU49:0.#} u, curved up {moved.y > 0.8f}, chill {_chill44}");
                // Rime Coat: the first 2 contact attackers freeze, the third does not
                Reset(); T.ggMove1682 = 2; Toast("RIME COAT"); var at = new List<Enemy>(); for (int i = 0; i < 3; i++) at.Add(Pack(EType.Imp, H0 + new Vector2(-1.5f, -1f + i), 0f)); yield return ZDash1618(Vector2.right); _heroPos = H0; yield return LF(5);
                int rf0 = GgRimeFreezes1682; var fzA = new bool[3]; for (int ai = 0; ai < 3; ai++) { var a = at[ai]; _iframe = 0f; _hurtBy53 = a; HurtHero(5f, "clip", false, true); fzA[ai] = a.freezeT > 0f; yield return LF(12); }
                Res("rimecoat", GgRimeFreezes1682 - rf0 == 2 && fzA[0] && fzA[1] && !fzA[2], $"frozen attackers {GgRimeFreezes1682 - rf0} (on hit: {fzA[0]}/{fzA[1]}/{fzA[2]})"); hp = maxHp; yield return LF(30);
                // Floodgate: a wall at the dash start holds the chasers back
                Reset(); T.ggMove1682 = 3; Toast("FLOODGATE"); _heroPos = H0 + new Vector2(-2f, 0f); SnapCamera(); for (int i = 0; i < 4; i++) Pack(EType.Imp, H0 + new Vector2(-6f, -1.2f + i * 0.8f), 2.5f, 5000f);
                yield return LF(5); int gb0 = GgGateBlocks1682; yield return ZDash1618(Vector2.right); bool wall = _ggGate1682 != null; float gx = wall ? _ggGate1682.a.x : 0f; yield return LF(55);
                float maxX = -99f; foreach (var e in L) maxX = Mathf.Max(maxX, e.pos.x); Res("floodgate", wall && GgGateBlocks1682 - gb0 > 0 && maxX < gx, $"wall {wall} at x {gx - H0.x:0.#}, blocks {GgGateBlocks1682 - gb0}, front chaser x {maxX - H0.x:0.#} (behind the wall {maxX < gx})"); yield return LF(30);
                // Frozen River: demons on the strip slowed; hero faster on it
                Reset(); T.ggMove1682 = 4; Toast("FROZEN RIVER"); _heroPos = H0 + new Vector2(-2f, 0f); SnapCamera(); for (int i = 0; i < 4; i++) Pack(EType.Imp, H0 + new Vector2(-1f + i * 1f, 0f), 0f, 5000f);
                yield return LF(5); int rs0 = GgRiverSlows1682; yield return ZDash1618(Vector2.right); yield return LF(5); bool strip = _ggRiver1682.Count > 0; Vector2 keep = _heroPos; _heroPos = H0; float hm = GgMoveMul1682(); _heroPos = keep; int slowed = 0; foreach (var e in L) if (e.slowT > 0f && e.slowAmt >= T.frSlow1682 - 0.01f) slowed++; yield return LF(80);
                Res("frozenriver", strip && GgRiverSlows1682 - rs0 > 0 && slowed >= 3 && Mathf.Abs(hm - (1f + T.frHero1682)) < 0.01f, $"strip {strip}, slowed {slowed}/4, hero move x{hm:0.##} on it"); _atGod = true;
            }
            else if (which == 2)
            {
                var cold = Basic(SkillId.TidalSurge, "GONGGONG SPECIALS"); Vector2 c = H0 + new Vector2(4f, 0f);
                // Drowning Mark: a cold hit on the marked demon chills those around it; its freeze lasts +50%
                T.ggSpecial1682 = 1; Toast("DROWNING MARK"); var mk = Pack(EType.Brute, c, 0f, 5000f); for (int i = 0; i < 6; i++) Pack(EType.Imp, c + new Vector2(Mathf.Cos(i * 1.05f) * 1.8f, Mathf.Sin(i * 1.05f) * 1.3f), 0f);
                atMouse986 = c; yield return LF(5); yield return ZPress1618(1); yield return LF(10); bool marked = GgMark1682 == mk; int mr0 = GgMarkRelays1682;
                float fz = 0f; for (int h = 0; h < 3; h++) { Hit(mk, 1f, cold, Vector2.zero); fz = Mathf.Max(fz, mk.freezeT); yield return LF(8); } // freeze length read on the hit that froze it
                int ch = 0; foreach (var e in L) if (e != mk && (e.chillT > 0f || e.freezeT > 0f)) ch++;
                Res("drowningmark", marked && GgMarkRelays1682 - mr0 >= 6 && ch >= 6 && fz > 1.2f * 1.4f, $"marked {marked}, relayed chills {GgMarkRelays1682 - mr0}, neighbours chilled {ch}/6, marked demon froze for {fz:0.##} s (base 1.2)"); yield return LF(20);
                // Hypothermia: cursed demons freeze in 2 chills (never fewer), +30% longer
                Reset(); T.ggSpecial1682 = 2; Toast("HYPOTHERMIA"); for (int i = 0; i < 7; i++) Pack(EType.Imp, c + new Vector2(Mathf.Cos(i * 0.9f) * 1.5f, Mathf.Sin(i * 0.9f) * 1.1f), 0f);
                atMouse986 = c; yield return LF(5); int cu0 = GgCursed1682; yield return ZPress1618(1); yield return LF(45); int cursed = GgCursed1682 - cu0;
                var hy = L[0]; ApplyStatus(hy, Status.Chill, 0f, 1f, cold); bool f1 = hy.freezeT > 0f; ApplyStatus(hy, Status.Chill, 0f, 1f, cold); float hz = hy.freezeT; int need = GgChillNeed1682(hy, 2);
                Res("hypothermia", cursed == 7 && !f1 && hz > 1.2f * 1.25f && need == 2, $"cursed {cursed}/7, frozen after 1 chill {f1}, after 2 for {hz:0.##} s (base 1.2), need with Chill-support Lv3 + curse {need} (min 2)"); yield return LF(20);
                // Shatter: frozen demons near the cursor explode, freezes used up
                Reset(); T.ggSpecial1682 = 3; Toast("SHATTER"); for (int i = 0; i < 8; i++) Pack(EType.Imp, c + new Vector2(-1.5f + (i % 4) * 1f, -0.6f + (i / 4) * 1.2f), 0f, 3000f);
                atMouse986 = c; yield return LF(5); var frz = new List<Enemy>(); for (int i = 0; i < 4; i++) { IceFreeze(L[i * 2], 3f); frz.Add(L[i * 2]); } int sh0 = GgShatters1682; float hp0 = 0f; foreach (var e in L) hp0 += e.hp; yield return ZPress1618(1); yield return LF(5);
                float hp1 = 0f; int still = 0; int refz = 0; foreach (var e in L) { hp1 += e.hp; if (e.freezeT > 0f) { if (frz.Contains(e)) still++; else refz++; } }
                Res("shatter", GgShatters1682 - sh0 == 4 && still == 0 && hp0 - hp1 > 0f, $"shattered {GgShatters1682 - sh0}/4, originals still frozen {still}, damage {hp0 - hp1:0}, neighbours frozen by the burst chills {refz}"); yield return LF(30);
                // Toppled Pillar: blocks while standing, then falls along the aim and crushes
                Reset(); T.ggSpecial1682 = 4; Toast("TOPPLED PILLAR"); Vector2 pc = H0 + new Vector2(3f, 0f); for (int i = 0; i < 3; i++) Pack(EType.Imp, pc + new Vector2(2.5f + i * 0.6f, 0f), 2f, 3000f); for (int i = 0; i < 3; i++) Pack(EType.Imp, pc + new Vector2(1f + i * 1f, 0.3f), 0f, 3000f);
                atMouse986 = pc; yield return LF(5); int pb0 = GgPillarBlocks1682, ph0 = GgPillarHits1682; yield return ZPress1618(1); yield return LF(30); int blocks = GgPillarBlocks1682 - pb0; yield return LF(40);
                Res("toppledpillar", blocks > 0 && GgPillarHits1682 - ph0 >= 3, $"blocks while standing {blocks}, crushed {GgPillarHits1682 - ph0}"); yield return LF(20);
                // Frost Wall: holds chasers back; crystals hit by your area skills explode
                Reset(); T.ggSpecial1682 = 5; Toast("FROST WALL"); for (int i = 0; i < 5; i++) Pack(EType.Imp, H0 + new Vector2(8f, -2f + i), 2f, 3000f);
                atMouse986 = c; yield return LF(5); int wb0 = GgWallBlocks1682, cb0 = GgCrystalBooms1682; yield return ZPress1618(1); yield return LF(60); float mx = 99f; foreach (var e in L) mx = Mathf.Min(mx, e.pos.x - H0.x); int held = GgWallBlocks1682 - wb0;
                Basic(SkillId.Whirlpool, "WHIRLPOOL ON THE WALL"); skills[0].cd = 0f; atMouse986 = c; yield return Hold(3); yield return LF(45);
                Res("frostwall", held > 0 && mx > 3.5f && GgCrystalBooms1682 - cb0 >= 1, $"blocks {held}, nearest chaser x {mx:0.#} (wall at ~4), crystals exploded by a Whirlpool collapse {GgCrystalBooms1682 - cb0}"); yield return LF(30);
            }
            else
            {
                Basic(SkillId.Frostbolt, "GONGGONG ULTIMATES"); _zKeepHp = false; _atGod = false;
                // Ram of Mount Buzhou: damage taken fills wrath -> unhurt charge, x8 along the path, shard rain
                T.ggUlt1682 = 1; Toast("RAM OF MOUNT BUZHOU"); for (int i = 0; i < 6; i++) Pack(EType.Imp, H0 + new Vector2(2f + i * 1.4f, (i % 2 - 0.5f) * 0.8f), 0f, 5000f);
                yield return LF(5); bool notYet = !GgUltArmed1682; int g = 0; while (GgWrath1682 < 1f && g++ < 40) { _iframe = 0f; hp = maxHp; HurtHero(maxHp * 0.2f, "clip"); } hp = maxHp; bool armed = GgUltArmed1682;
                atMouse986 = H0 + new Vector2(8f, 0f); int rh0 = GgRamHits1682, rs0 = GgRamShardHits1682; Vector2 hb = _heroPos; float hpB = hp; yield return ZPress1618(2); yield return LF(4); _iframe = 0f; HurtHero(10f, "clip"); bool unhurt = hp >= hpB - 0.01f; yield return LF(40);
                Res("ram", notYet && armed && GgRamHits1682 - rh0 >= 5 && GgRamShardHits1682 - rs0 >= 1 && (_heroPos - hb).magnitude > 6f && unhurt, $"armed after {g} hits ({notYet}/{armed}), path hits {GgRamHits1682 - rh0}, shard hits {GgRamShardHits1682 - rs0}, charged {(_heroPos - hb).magnitude:0.#} u, unhurt mid-charge {unhurt}"); hp = maxHp; yield return LF(20);
                // The Great Flood: only at High Tide; NW -> SE wave, pushes non-bosses SE, 2 chills
                Reset(); T.ggUlt1682 = 2; Toast("THE GREAT FLOOD"); GgTide1682 = 0f; for (int i = 0; i < 8; i++) Pack(EType.Imp, H0 + new Vector2(-4f + i * 1.3f, (i % 3 - 1) * 1.4f), 0f, 5000f);
                yield return LF(10); bool low = !GgUltArmed1682; GgTide1682 = T.gfRise1682 - 0.1f; yield return LF(10); bool high = GgUltArmed1682; var p0 = new List<Vector2>(); foreach (var e in L) p0.Add(e.pos); int fh0 = GgFloodHits1682;
                var pb = new float[2]; yield return Ms(20, pb); // baseline: same pack, no ultimate
                yield return ZPress1618(2); int gOrd = GgGround1682(_heroPos), minAct = int.MaxValue; foreach (var e in L) if (e.sr != null) minAct = Mathf.Min(minAct, e.sr.sortingOrder); minAct = Mathf.Min(minAct, ZhHeroOrd1625);
                var pf = new float[2]; yield return Ms(25, pf); yield return LF(25); int se = 0, chl = 0;
                Res("perf_flood", true, $"frame {pb[0]:0.0} ms (quads/frame {pb[1]:0.#}) -> {pf[0]:0.0} ms with the Great Flood (quads/frame {pf[1]:0.#}); wall-clock, software GL on the box");
                Res("sort_ground", gOrd < minAct && gOrd > HellordRoom.LightOrder, $"ground tier order {gOrd} (FbGround {FbGround} + YSort + 40): above the room floor / props / lights (<= {HellordRoom.LightOrder}) and below the lowest actor order {minAct}"); for (int i = 0; i < L.Count; i++) { var d = L[i].pos - p0[i]; if (d.x > 0.8f && d.y < -0.8f) se++; if (L[i].chillT > 0f || L[i].freezeT > 0f) chl++; }
                Res("greatflood", low && high && GgFloodHits1682 - fh0 == L.Count && se >= L.Count - 1 && chl == L.Count, $"locked before High Tide {low}, armed at High Tide {high}, hits {GgFloodHits1682 - fh0}/{L.Count}, pushed SE {se}, chilled {chl}"); yield return LF(20);
                // Eternal Winter: 25 frozen kills arm it; frozen deaths shatter and chain
                Reset(); T.ggUlt1682 = 3; Toast("ETERNAL WINTER"); GgFrozenKills1682 = 0; for (int i = 0; i < 25; i++) { var e = Pack(EType.Imp, H0 + new Vector2(6f + (i % 5) * 0.3f, -3f + (i / 5) * 0.3f), 0f, 20f); IceFreeze(e, 3f); Damage(e, e.hp + 10f, false, true); } yield return LF(10);
                bool armedW = GgUltArmed1682; int fk = GgFrozenKills1682; L.Clear(); for (int i = 0; i < 9; i++) Pack(EType.Imp, H0 + new Vector2(3f + (i % 3) * 1.2f, -1.2f + (i / 3) * 1.2f), 0f, 30f);
                yield return LF(5); int wb0 = GgWinterBursts1682, wc0 = GgWinterChills1682; var wb = new float[2]; yield return Ms(20, wb); yield return ZPress1618(2); var wf = new float[2]; yield return Ms(30, wf); T.ggFx1682 = false; var wn = new float[2]; yield return Ms(20, wn); T.ggFx1682 = true; yield return LF(20);
                Res("perf_winter", true, $"frame {wb[0]:0.0} ms -> {wf[0]:0.0} ms with the Eternal Winter overlay + pulses (quads/frame {wf[1]:0.#}); {wn[0]:0.0} ms with the GGFX shaders off (placeholders)"); var first = L[4]; IceFreeze(first, 3f); foreach (var e in L) if (e != first) IceFreeze(e, 3f); Damage(first, first.hp + 10f, false, true); yield return LF(20);
                int dead = 0; foreach (var e in L) if (!e.alive) dead++;
                Res("eternalwinter", armedW && fk == T.ewNeed1682 && GgWinterChills1682 - wc0 >= 9 && GgWinterBursts1682 - wb0 >= 2 && dead >= 3, $"frozen kills {fk}/{T.ewNeed1682}, armed {armedW}, winter chills {GgWinterChills1682 - wc0}, shatter bursts {GgWinterBursts1682 - wb0} (chain), dead {dead}/9"); yield return LF(60);
                // Pillars of Ao: below 35% HP; demons pushed out of the square; icicles
                Reset(); T.ggUlt1682 = 4; Toast("PILLARS OF AO"); for (int i = 0; i < 6; i++) Pack(EType.Imp, H0 + new Vector2(-1f + (i % 3) * 1f, -0.6f + (i / 3) * 1.2f), 0.5f, 5000f); for (int i = 0; i < 4; i++) Pack(EType.Imp, H0 + new Vector2(4f, -1.5f + i), 1f, 5000f);
                hp = maxHp; yield return LF(5); bool offHigh = !GgUltArmed1682; hp = maxHp * 0.3f; yield return LF(5); bool armedA = GgUltArmed1682; int ai0 = GgAoIcicles1682, ap0 = GgAoPushes1682;
                yield return ZPress1618(2); yield return LF(60); int inside = 0; float h = T.aoHalf1682; foreach (var e in L) { var d = e.pos - _ggAoC1682; if (Mathf.Abs(d.x) < h && Mathf.Abs(d.y) < h) inside++; } yield return LF(60); hp = maxHp; yield return LF(5); bool disarm = !GgAoArmedLatch1682;
                Res("pillarsofao", offHigh && armedA && GgAoPushes1682 - ap0 > 0 && inside == 0 && GgAoIcicles1682 - ai0 >= 8 && disarm, $"off at full HP {offHigh}, armed below {T.aoArm1682:0.##} {armedA}, pushes {GgAoPushes1682 - ap0}, demons inside {inside}, icicles {GgAoIcicles1682 - ai0}, disarmed above {T.aoDisarm1682:0.##} {disarm}");
                _zKeepHp = true; _atGod = true; hp = maxHp;
            }
            {   // every GonggongFX effect this clip exercises drew through its shader (material present + hook reached)
                string[][] want = {
                    new[] { "TidalSurge", "TidalSurge_Crest", "TidalSurge_Crash", "Whirlpool", "Whirlpool_Collapse", "BreathOfTheDeep", "BreathOfTheDeep_Ground", "BuzhouRift_Burst", "BuzhouRift_Spike", "BuzhouRift_Consume", "EyeBlackWaters", "EyeShard", "EyeShard_Hit" },
                    new[] { "RideTheFlood", "RimeCoat", "Floodgate", "FrozenRiver" },
                    new[] { "DrowningMark", "Hypothermia", "Hypothermia_Cursed", "Shatter_Burst", "ToppledPillar", "ToppledPillar_Crush", "FrostWall", "FrostWall_Pop" },
                    new[] { "Ram_Charge", "Ram_Head", "Ram_Shard", "GreatFlood", "EternalWinter", "EternalWinter_Pulse", "EternalWinter_Shatter", "PillarsOfAo", "Icicle", "Icicle_Hit" } };
                var miss = new List<string>(); foreach (var w in want[which]) if (!ZhFxSeen1625("GGFX_" + w) || GgFxMat1682("GGFX_" + w) == null) miss.Add(w);
                Res("hfx", miss.Count == 0 && ZhFxQuads1625 > q0, $"drawn {want[which].Length - miss.Count}/{want[which].Length}, quads {ZhFxQuads1625 - q0}" + (miss.Count > 0 ? " | missing " + string.Join(",", miss) : ""));
                ZhFxLogOn1625 = false;
            }
            ARes(scen + ":clip", _r1561 > 0 || _atRecFps <= 0, $"frames {_r1561}");
            ZKill1618(L); ClearGg1682(); GgRunReset1682(); T.ggMove1682 = _ggM0; T.ggSpecial1682 = _ggS0; T.ggUlt1682 = _ggU0; HInput.AtLmb = false;
            _zClip = null; ZhEnd1618(scen, clip);
        }

        // ---- sup1651 hooks: Gonggong kit casts with their conditions pre-met (frozen demons, a contact attacker, meters full, low life)
        float _smGgP1682;
        IEnumerator SmGgCast1682(int i)
        {
            _smGgP1682 = 0f; var T = TZ;
            if (i < 4)
            {
                _dashDir49 = Vector2.right; GgDashStart1682(_heroPos); AtCd1651 = ZhMoveCd; GgDashAfter1682(); _dashT49 = 0f; GgRideT1682 = 0f;
                if (i == 1) { _smGgP1682 = GgRimeT1682; Enemy a = null; foreach (var e in _smL1651) if (e.alive) { a = e; break; } if (a != null) GgHeroHit1682(0f, true, a); } // one contact attacker freezes
                if (i == 2 && _ggGate1682 != null) _smGgP1682 = _ggGate1682.life;
                if (i == 3) foreach (var r in _ggRiver1682) _smGgP1682 = Mathf.Max(_smGgP1682, r.life);
            }
            else if (i < 9)
            {
                int sp = i - 3; Vector2? m0 = atMouse986;
                if (sp == 3) { Vector2 cur = MouseWorld(); foreach (var e in _smL1651) if (e.alive && (e.pos - cur).magnitude <= 0.5f) IceFreeze(e, 5f); } // Shatter: the demon on the cursor is frozen
                if (sp == 4) atMouse986 = _heroPos + new Vector2(0.8f, 0.25f); // Toppled Pillar: stand at the near edge of the pack and fall along +x between two rows, so longer / wider crushes reach more
                GgSpecial1682(); AtCd1651 = ZhSpecCd; atMouse986 = m0;
                if (sp == 1) _smGgP1682 = GgMarkT1682;
                if (sp == 5 && _ggFw1682 != null) { _smGgP1682 = _ggFw1682.life; foreach (var c in _ggFw1682.c.ToArray()) GgCrystalBoom1682(c); } // your skills hit every crystal
            }
            else
            {
                int u = i - 8; GgFillUlt1682(); GgUltimate1682(); AtCd1651 = ZhUltCd;
                if (u == 3) { _smGgP1682 = GgWinterT1682; Enemy best = null; float bd = 1e9f; foreach (var e in _smL1651) { if (!e.alive) continue; float d = (e.pos - _heroPos).sqrMagnitude; if (d < bd) { bd = d; best = e; } } if (best != null) { int ix = _smL1651.IndexOf(best); IceFreeze(best, 3f); best.hp = 1f; if (ix >= 0) _smHp0[ix] = 1f; Damage(best, 2f, false, true); } } // a 1 HP frozen demon dies: its kill is not the measured damage // a frozen death = the shatter burst (that kill is not the measured damage)
                if (u == 4) { _smGgP1682 = GgAoT1682; _ggAoC1682 = _heroPos + new Vector2(3.5f, 0f); } // the square sits inside the dense pack, so a bigger square holds more
            }
            yield return null;
        }
        float SmGgProbe1682(int i)
        {
            if (i == 5) { float mx = 0f; foreach (var e in _smL1651) mx = Mathf.Max(mx, e.ggHypo1682 - _smKT0); return mx; } // curse end, from the cast
            return _smGgP1682;
        }
    }
}
#endif
