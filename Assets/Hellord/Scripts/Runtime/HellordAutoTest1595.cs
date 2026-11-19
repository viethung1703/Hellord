#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1595 body hitbox scenario (bh1595): Xingtian start pick -> Twin Axe Throw basic, one still demon of each sprite type with the debug overlay
    /// (feet circle green, body box magenta, axe / javelin hit circle yellow). Chest-aimed throws right, down-right and STRAIGHT UP must hit on the way out,
    /// the drawn axe must pass through the cursor and flash on the body; one Thunder Javelin (injected) at a chest; then the feet-only fallback misses
    /// the same chest-aimed straight-up throw (the old bug) and a ground skill is unchanged.</summary>
    public partial class HellordGame
    {
        IEnumerator AtBody1595()
        {
            string scen = "bh1595"; SkillId id = SkillId.TwinAxeThrow;
            yield return AtBegin(scen); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51(); var T = HellordTuning.I;
            int so = _souls51, si = _sigils51, pp = _pactPath51, ps = _pactSel51; bool sd = _pactSealed51; var un = (int[])_unlock51.Clone();
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 3; _pactSel51 = 0x1; _pactSealed51 = true; PactSave51(); _altarView51 = 0;
            _atGod = true; Manual716 = false;
            AtMenu1555(); yield return AtWait(1.0f);
            string clip = scen; _r1561 = 0; AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            yield return AtHold1561(0.3f, clip); Act69(0); yield return AtHold1561(0.6f, clip);
            var pc = AtPlayC1561(); yield return AtFake1561(pc.x, pc.y, clip);
            yield return AtHold1561(0.5f, clip);
            int ci = cards.FindIndex(c => c.skill != null && c.skill.id == id); if (ci >= 0) Choose(cards[ci]);
            bool basic = Basic1517() != null && Basic1517().def.id == id; var sk = Basic1517();
            maxHp = hp = 100000; SnapCamera(); bool dbg0 = T.bodyDebug1595; T.bodyDebug1595 = true;
            void Rec95() { AtAudioLog1481.Frame = _r1561; AtRecFrame(clip, ref _r1561); }
            var list = new List<Enemy>(); var sb = new System.Text.StringBuilder();
            Enemy Mk(EType t, float dx, float dy) { var e = SpawnEnemyAt(t, _heroPos + new Vector2(dx, dy)); if (e != null) { e.dmg = 0; e.speed = 0f; e.hp = e.maxHp = 50000; list.Add(e); var s = BodySize1595(e); sb.Append($"{t} feetR {e.radius:0.00} body {s.x:0.00}x{s.y:0.00} hitY {e.hitY:0.00}; "); } return e; }
            var impR = Mk(EType.Imp, 5.0f, 0.0f); var brute = Mk(EType.Brute, 5.5f, -4.0f); var impU = Mk(EType.Imp, 0.0f, 5.0f); var wraith = Mk(EType.Elite, -5.0f, 1.5f);
            var ghoul = Mk(EType.Ghoul, -4.5f, -3.5f); Mk(EType.Splitter, 8.0f, 2.5f); Mk(EType.SplitChild, 2.5f, -2.5f);
            if (wraith != null) wraith.elite = false;
            ALog(scen + " bodies: " + sb);
            // offline geometry check
            bool geo = SegBoxDist1595(new Vector2(-1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, Vector2.one) == 0f && Mathf.Abs(SegBoxDist1595(new Vector2(2, 0), new Vector2(2, 1), Vector2.zero, Vector2.one) - 1f) < 1e-4f && Mathf.Abs(SegBoxDist1595(new Vector2(2, 2), new Vector2(3, 3), Vector2.zero, Vector2.one) - Mathf.Sqrt(2f)) < 1e-4f;
            for (int f = 0; f < 20; f++) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; foreach (var e in list) if (e.alive) e.speed = 0f; }
            var res = new List<string>(); bool allHit = true, allThrough = true, allFlash = true;
            Vector2 Chest(Enemy e) => e.pos + Vector2.up * (BodySize1595(e).y * 0.55f);
            IEnumerator Throw(Enemy tgt, string name, bool expectHit)
            {
                while (AxeOut1567) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; }
                for (int q = 0; q < 6; q++) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; }
                Vector2 aim = Chest(tgt); atMouse986 = aim; float hp0 = tgt.hp; int t0 = AxeThrows1567; AxeLastHitE1595 = null; Vector2 fl = new Vector2(1e9f, 1e9f); float through = 9f; bool outHit = false;
                sk.cd = 0f; HInput.AtLmb = true; for (int q = 0; q < 3; q++) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; atMouse986 = aim; } HInput.AtLmb = false;
                if (_pbAxe != null) { Vector2 hu = Vector2.up * T.axeHeightU1567; through = PbSegDist(aim, _pbAxe.a + hu, _pbAxe.b + hu); }
                for (int q = 0; q < 120 && AxeOut1567; q++) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; atMouse986 = aim; if (_pbAxe != null && _pbAxe.phase == 0 && AxeLastHitE1595 == tgt && !outHit) { outHit = true; fl = AxeLastFlash1595; } }
                BodyBox1595(tgt, out var lo, out var hi); bool flOk = !T.axeFlashDrawn1595 || PtBox1595(fl, lo, hi) <= T.axeHitRU1567 + 0.01f;
                bool hitOk = outHit == expectHit; bool thrOk = !T.axeThroughCursor1595 || through < 0.05f;
                res.Add($"{name}: aim chest {aim - _heroPos:F2} throws {AxeThrows1567 - t0} outHit {outHit} (want {expectHit}) dmg {hp0 - tgt.hp:0} cursor->drawnLine {through:0.000} flash {(outHit ? (fl - tgt.pos).ToString("F2") : "-")} inBody {flOk}");
                allHit &= hitOk && AxeThrows1567 - t0 == 1; allThrough &= thrOk; if (expectHit) allFlash &= flOk;
            }
            yield return Throw(impR, "imp right", true);
            yield return Throw(brute, "brute down-right", true);
            yield return Throw(impU, "imp STRAIGHT UP", true);
            yield return Throw(wraith, "wraith left", true);
            // injected Thunder Javelin (Leizu pact not loaded in this run): from the hand height at the ghoul's chest
            int jb0 = JavBursts1567; float gh0 = ghoul.hp; { Vector2 o = _heroPos + new Vector2(-0.5f, T.javHeightU1567); Vector2 c = Chest(ghoul); Vector2 d = (c - o).normalized; Vector2 g = o - Vector2.up * T.javHeightU1567; _pbJavs.Add(new PbJav { a = g, p = g, dir = d, range = T.javRangeU1567, dmg = 10f, s = sk }); }
            for (int q = 0; q < 40; q++) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; }
            bool javOk = JavBursts1567 > jb0 && ghoul.hp < gh0; res.Add($"javelin at ghoul chest: bursts {JavBursts1567 - jb0} dmg {gh0 - ghoul.hp:0} -> {javOk}");
            bool newOk = allHit && allThrough && allFlash && javOk && geo;
            yield return AtShot(scen + "_body");
            // feet-only fallback: the same chest-aimed straight-up throw passes over the imp's feet circle (the old bug)
            T.bodyHitbox1595 = false; T.axeThroughCursor1595 = false; T.axeFlashDrawn1595 = false; float r0 = T.axeHitRU1567; T.axeHitRU1567 = 0.55f;
            bool h0 = allHit; allHit = true; yield return Throw(impU, "OLD feet-only imp straight up", false); bool oldMiss = allHit; allHit = h0;
            T.bodyHitbox1595 = true; T.axeThroughCursor1595 = true; T.axeFlashDrawn1595 = true; T.axeHitRU1567 = r0;
            for (int q = 0; q < 20; q++) { yield return null; AtFrameChecks(); Rec95(); hp = maxHp; }
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_" + clip, "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            {   // t1597 lightning ground glow: off = every ground key unloaded after the javelin impact (flash still drawn); on = they load again
                string ak = PbArtKeys1567(); bool offOk = !T.lightningGroundGlow1597; var gs = new System.Text.StringBuilder();
                foreach (var k in T.lightningGroundKeys1597) { bool n = PbA1567(k) == null; offOk &= n; gs.Append(k).Append(n ? "=off " : "=LOADED "); }
                bool flashOk = PbA1567("tj2_impflash") != null && PbA1567("bl2_spawnflash") != null && PbA1567("bl_shadow1597") != null && ak.Contains("tj2_impflash=ART");
                T.lightningGroundGlow1597 = true; bool onOk = PbA1567("bl2_ground") != null && PbA1567("tj2_impground") != null; T.lightningGroundGlow1597 = false; bool reOff = PbA1567("bl2_ground") == null;
                ARes(scen + ":lightning_ground_glow_off", offOk && flashOk && onOk && reOff, $"default off {gs}| flashes kept (tj2_impflash, bl2_spawnflash) + dark v1 orb shadow bl_shadow1597 {flashOk} | toggle on loads bl2_ground + tj2_impground {onOk} | back off {reOff}");
            }
            ARes(scen + ":axe_body", basic && newOk, $"basic {basic} geo {geo} | " + string.Join(" | ", res));
            ARes(scen + ":feet_only_fallback", oldMiss, "toggle off (bodyHitbox1595, axeThroughCursor1595, axeFlashDrawn1595, axeHitRU 0.55) -> chest-aimed straight-up throw misses as before t1595: " + res[res.Count - 1]);
            ALog(scen + " body tests " + BodyTests1595 + " hits " + BodyHits1595);
            T.bodyDebug1595 = dbg0; AtClearWorld();
            _souls51 = so; _sigils51 = si; _pactPath51 = pp; _pactSel51 = ps; _pactSealed51 = sd; _unlock51 = un; PactSave51(); _atGod = false;
            AtEnd(scen, "hero_sprite_not_base_final2");
        }
    }
}
#endif
