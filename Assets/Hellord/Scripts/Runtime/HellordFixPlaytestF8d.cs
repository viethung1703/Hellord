using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        /// <summary>t1823: 1920x1080 Play-mode shots for the f8d3426 retest fixes (LV badge, F1, picker, colossus, storm tips, Sun's Wrath, Smolder).</summary>
        System.Collections.IEnumerator FixPlaytestCoF8d()
        {
            const string tag = "[t1823-f8d]";
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
            catch (System.Exception e) { Debug.LogWarning(tag + " ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
            Log("screen after GameView force " + Screen.width + "x" + Screen.height + " renderer=" + SystemInfo.graphicsDeviceName);
#endif
            System.IO.Directory.CreateDirectory(dir);
            var T = HellordTuning.I;
            T.testMenuEnabled = true;
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = true;
            T.kitBar1641 = true; T.skillBelt720 = true;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun();
            _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            PreloadStop1675();
            pendingLevels = 0; mode = Mode.Play; _tmOpen = false;
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 5 });
            SetHeroLayers();
            foreach (var e in _en) if (e != null && e.alive) { e.alive = false; e.speed = 0f; e.dmg = 0f; }

            // B5: XP badge must show the full LVn label at 2 and 3 digits
            level = 10; pendingLevels = 0; xp = Mathf.Max(1, HData.XpToNext(level) / 2);
            SnapCamera();
            for (int i = 0; i < 10; i++) yield return null;
            Log("badge LV10 ShownLevel=" + ShownLevel + " (expect 10)");
            yield return ShotF8d(dir + "/fix_f8d3426_lv10.png");
            level = 100; pendingLevels = 0;
            for (int i = 0; i < 8; i++) yield return null;
            Log("badge LV100 ShownLevel=" + ShownLevel + " (expect 100)");
            yield return ShotF8d(dir + "/fix_f8d3426_lv100.png");
            level = 10; pendingLevels = 0;

            // B9: F1 FLOOR name vs HUD (one source: FloorLabel53(ThemeSlot53))
            string hudName = FloorLabel53(ThemeSlot53, true);
            string floorLine = "THIS FLOOR (depth " + (FloorIdx + 1) + ", " + FloorLabel53(ThemeSlot53, true) + ")";
            Log("B9 HUD=" + hudName + " F1=" + floorLine + " Theme=" + Theme + " ThemeSlot53=" + ThemeSlot53 + " FloorIdx=" + FloorIdx);
            _tmOpen = true; _tmTab = 0; _tmSel = 0; _tmScroll = Vector2.zero; _tmResetArm53 = false;
            for (int i = 0; i < 12; i++) yield return null;
            yield return ShotF8d(dir + "/fix_f8d3426_f1_floor.png");

            // X5: F1 PACT labels (2 columns, wrap) + scrolled body (wheel writes _tmScroll)
            _tmTab = TmPactTab53; _tmSel = 0; _tmScroll = Vector2.zero;
            for (int i = 0; i < 12; i++) yield return null;
            Log("F1 PACT contentH=" + _tmContentH + " viewH=" + _tmViewH + " cols=" + _tmCols + " (expect cols=2, wheel clamps _tmScroll)");
            yield return ShotF8d(dir + "/fix_f8d3426_f1_pact.png");
            _tmScroll = new Vector2(0f, 4000f);
            for (int i = 0; i < 8; i++) yield return null;
            yield return ShotF8d(dir + "/fix_f8d3426_f1_pact_scrolled.png");
            _tmOpen = false;

            // B4: WHICH SKILL? picker uses the same centred card layout
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 2 });
            skills.Add(new SkillInst { def = HData.Skill(SkillId.NkBoneColossus), level = 1 });
            SetHeroLayers();
            cards.Clear();
            OpenTargetPick(HData.Support(SupportId.ExplodeOnKill), new List<int> { 0, 1 }, false, _ => { });
            mode = Mode.Chest;
            for (int i = 0; i < 12; i++) yield return null;
            yield return ShotF8d(dir + "/fix_f8d3426_picker.png");
            _tp = null; mode = Mode.Play; cards.Clear();

            // B11: Bone Colossus live — skill bar must show the same green icon as the strip (no orange wash)
            T.nkKit1713 = true; T.kitSlotOverride1639 = true; T.nkMove1713 = 0; T.nkSpecial1713 = 0; T.nkUlt1713 = 2;
            _pactPath51 = 1; PactApply51();
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 5 });
            SetHeroLayers();
            ChooseKit1639(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 2 });
            for (int i = 0; i < 4; i++) NSpawn536(0, false, _heroPos + new Vector2(-1.2f + i * 0.6f, 0.4f));
            bool colOk = NkBeginColossus1713(NkK1713(2, 2));
            Log("colossus begin=" + colOk + " on=" + _nkColossus1713 + " n=" + _nkColN1713 + " hp=" + _nkColHp1713 + "/" + _nkColMax1713 + " KitUlt=" + KitUlt1666 + " key=" + KitUltKey1666() + " icon=" + (KitIcon1666(KitUltKey1666()) != null ? KitIcon1666(KitUltKey1666()).name : "NULL"));
            SnapCamera();
            for (int i = 0; i < 16; i++) yield return null;
            yield return ShotF8d(dir + "/fix_f8d3426_colossus.png");
            if (_nkColossus1713) NkEndColossus1713(false);

            // Storm Ring tips: east/west inside the oval must be queried + hit
            T.nkKit1713 = false; T.kit1618 = false; PactApply51();
            foreach (var e in _en) if (e != null && e.alive) e.alive = false;
            skills.Clear();
            var storm = new SkillInst { def = HData.Skill(SkillId.StormRing), level = 1 };
            skills.Add(storm); SetHeroLayers();
            float rStorm = storm.R(HData.At(T.stormRadiusU, storm.level, 2.6f));
            var marks = new (string name, Vector2 d, bool wantHit)[] {
                ("east-tip", new Vector2(rStorm * 0.92f, 0f), true),
                ("west-tip", new Vector2(-rStorm * 0.92f, 0f), true),
                ("north-out", new Vector2(0f, rStorm * 0.92f), false),
                ("south-out", new Vector2(0f, -rStorm * 0.92f), false)
            };
            var placed = new List<Enemy>();
            foreach (var m in marks)
            {
                var e = SpawnEnemyAt(EType.Imp, _heroPos + m.d);
                if (e == null) continue;
                e.dmg = 0; e.speed = 0.0001f; e.hp = e.maxHp = 1e5f; placed.Add(e);
            }
            SnapCamera();
            for (int i = 0; i < 8; i++) yield return null;
            var hp0 = new float[placed.Count];
            for (int i = 0; i < placed.Count; i++) hp0[i] = placed[i].hp;
            try { Cast(storm, 1f); } catch (System.Exception e) { if (ex == null) ex = e.Message; Log("Cast StormRing ex " + e.Message); }
            for (int i = 0; i < 10; i++) yield return null;
            int stormOkN = 0, stormBad = 0;
            for (int i = 0; i < placed.Count && i < marks.Length; i++)
            {
                var e = placed[i]; var m = marks[i];
                bool oval = e != null && e.alive && InGroundOval(_heroPos, e.pos, rStorm, e.radius);
                bool hit = e != null && e.hp < hp0[i] - 0.01f;
                bool rowOk = hit == m.wantHit && oval == m.wantHit;
                if (rowOk) stormOkN++; else stormBad++;
                Log("storm " + m.name + " d=" + m.d + " r=" + rStorm + " er=" + (e != null ? e.radius : 0) + " oval=" + oval + " hit=" + hit + " want=" + m.wantHit + " " + (rowOk ? "OK" : "BAD"));
            }
            yield return ShotF8d(dir + "/fix_f8d3426_storm_tips.png");
            Log("storm tips ok=" + stormOkN + " bad=" + stormBad + " squash=" + HellFX.GroundYSquash);

            // Sun's Wrath impact uses FxDiscRc1815 / GroundYSquash (not 0.83)
            foreach (var e in placed) if (e != null) e.alive = false;
            T.kit1618 = true; T.nkKit1713 = false; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = true;
            T.kitUlt1618 = 1; // Sun's Wrath
            _pactPath51 = 0; PactApply51();
            float swR = ZkR1651(8, T.swR1618);
            var swRc = FxDiscRc1815(swR);
            var swOld = FxDiscRc1815(swR, 0.83f);
            Log("suns wrath R=" + swR + " rc=" + swRc + " h/innerY=" + swRc.w + " w=" + swRc.z + " GroundYSquash=" + HellFX.GroundYSquash + " old83=" + swOld + " ZhOn=" + ZhOn1618);
            ZFx1618(-1, _heroPos, swR, 1.0f, "ZFX_SunsWrath_Impact", swRc, YSort(_heroPos.y) + 200, 2);
            ZFx1618(-1, _heroPos, swR, 1.0f, "ZFX_SunsWrath_Telegraph", swRc, YSort(_heroPos.y) + 6, 1);
            SnapCamera();
            for (int i = 0; i < 12; i++) yield return null;
            yield return ShotF8d(dir + "/fix_f8d3426_suns_wrath.png");

            // B8: Smolder Ring FX (report whether torn bands remain on this renderer)
            _zFx1618.Clear();
            T.kitSpecial1618 = 4; T.kitSlotOverride1639 = true;
            _pactPath51 = 0; PactApply51();
            float rSm = ZkR1651(6, T.srR1618);
            var smRc = FxDiscRc1815(rSm);
            Log("smolder r=" + rSm + " rc=" + smRc + " squashMatWouldBe0.55 vs GroundYSquash=" + HellFX.GroundYSquash + " renderer=" + SystemInfo.graphicsDeviceName);
            _zRings1618.Add(new ZRingS { p = _heroPos + new Vector2(2.2f, 0.4f), t = 0.25f, tick = 0f });
            SnapCamera();
            for (int i = 0; i < 16; i++) yield return null;
            yield return ShotF8d(dir + "/fix_f8d3426_smolder.png");
            _zRings1618.Clear();

            int z = CamZoom();
            bool camOk = Screen.width == 1920 && Screen.height == 1080 && z == 3;
            string[] need = {
                "fix_f8d3426_lv10.png", "fix_f8d3426_lv100.png", "fix_f8d3426_f1_floor.png",
                "fix_f8d3426_f1_pact.png", "fix_f8d3426_f1_pact_scrolled.png", "fix_f8d3426_picker.png",
                "fix_f8d3426_colossus.png", "fix_f8d3426_storm_tips.png", "fix_f8d3426_suns_wrath.png",
                "fix_f8d3426_smolder.png"
            };
            bool files = true;
            foreach (var n in need)
            {
                bool okF = System.IO.File.Exists(dir + "/" + n);
                if (!okF) files = false;
                Log("file " + n + " " + (okF ? "ok" : "MISSING"));
            }
            bool namesMatch = hudName != null && floorLine.IndexOf(hudName) >= 0;
            bool ok = camOk && files && stormBad == 0 && colOk && namesMatch && ex == null;
            Log("screen " + Screen.width + "x" + Screen.height + " zoom=" + z + " namesMatch=" + namesMatch);
            if (ex != null) Log("exception " + ex);
            Log(ok ? "PASS" : "FAIL cam=" + camOk + " files=" + files + " stormBad=" + stormBad + " colOk=" + colOk + " names=" + namesMatch + " ex=" + (ex ?? "none"));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }

        System.Collections.IEnumerator ShotF8d(string path)
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < 25 && !System.IO.File.Exists(path); i++) yield return null;
            Debug.Log("[t1823-f8d] screenshot " + path + " " + (System.IO.File.Exists(path) ? ("ok " + new System.IO.FileInfo(path).Length + " bytes") : "MISSING"));
        }
    }
}
