using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        /// <summary>t1822: 1920x1080 Play-mode check of the 2e80984 playtest bugfixes (oval hits, cards, picker, F1).</summary>
        System.Collections.IEnumerator BugfixPlaytestCo1822()
        {
            const string tag = "[t1822]";
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
            catch (System.Exception e) { Debug.LogWarning("[t1822] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
            Log("screen after GameView force " + Screen.width + "x" + Screen.height);
#endif
            System.IO.Directory.CreateDirectory(dir);
            var T = HellordTuning.I;
            T.testMenuEnabled = true;
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = true;
            T.kitBar1641 = true; T.skillBelt720 = true;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 0; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun();
            _pactPath51 = 0; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            PreloadStop1675();
            pendingLevels = 0; mode = Mode.Play; _tmOpen = false;
            skills.Clear();
            var storm = new SkillInst { def = HData.Skill(SkillId.StormRing), level = 1 };
            skills.Add(storm); SetHeroLayers();
            foreach (var e in _en) if (e != null && e.alive) { e.alive = false; e.speed = 0f; e.dmg = 0f; }
            float rStorm = storm.R(HData.At(T.stormRadiusU, storm.level, 2.6f));
            var marks = new (string name, Vector2 d, bool wantHit)[] {
                ("east-in", new Vector2(3.2f, 0f), true),
                ("west-in", new Vector2(-3.2f, 0f), true),
                ("north-in", new Vector2(0f, 1.6f), true),
                ("south-in", new Vector2(0f, -1.6f), true),
                ("north-out", new Vector2(0f, 3.5f), false),
                ("south-out", new Vector2(0f, -3.5f), false),
                ("east-out", new Vector2(5.4f, 0f), false)
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
                bool circ = e != null && e.alive && (e.pos - _heroPos).magnitude <= rStorm + e.radius;
                bool hit = e != null && e.hp < hp0[i] - 0.01f;
                bool rowOk = hit == m.wantHit && oval == m.wantHit;
                if (rowOk) stormOkN++; else stormBad++;
                Log("storm " + m.name + " d=" + m.d + " r=" + rStorm + " er=" + (e != null ? e.radius : 0) + " circ=" + circ + " oval=" + oval + " hit=" + hit + " want=" + m.wantHit + " " + (rowOk ? "OK" : "BAD"));
            }
            yield return Shot1822(dir + "/bugfix_2e80984_storm_ring.png");
            Log("storm oval r=" + rStorm + " squash=" + HellFX.GroundYSquash + " ok=" + stormOkN + " bad=" + stormBad);

            // Zhurong Smolder Ring (squashed ground zone)
            foreach (var e in placed) if (e != null) { e.alive = false; }
            T.kitSpecial1618 = 4; T.kitSlotOverride1639 = true; T.kit1618 = true; T.kitAnyPact1618 = false;
            _pactPath51 = 0; PactApply51();
            float rSm = ZkR1651(6, T.srR1618);
            var smMarks = new (string name, Vector2 d, bool wantHit)[] {
                ("east-in", new Vector2(2.4f, 0f), true),
                ("north-in", new Vector2(0f, 1.2f), true),
                ("north-out", new Vector2(0f, 3.0f), false),
                ("south-out", new Vector2(0f, -3.0f), false)
            };
            var smPlaced = new List<Enemy>();
            foreach (var m in smMarks)
            {
                var e = SpawnEnemyAt(EType.Imp, _heroPos + m.d);
                if (e == null) continue;
                e.dmg = 0; e.speed = 0.0001f; e.hp = e.maxHp = 1e5f; smPlaced.Add(e);
            }
            _zRings1618.Add(new ZRingS { p = _heroPos, t = 0f, tick = 0f });
            SnapCamera();
            for (int i = 0; i < 16; i++) yield return null;
            int smOkN = 0, smBad = 0;
            for (int i = 0; i < smPlaced.Count && i < smMarks.Length; i++)
            {
                var e = smPlaced[i]; var m = smMarks[i];
                bool oval = e != null && e.alive && InGroundOval(_heroPos, e.pos, rSm, e.radius);
                bool want = m.wantHit; // first tick may already have applied scorch; check oval vs placement
                bool rowOk = oval == want;
                if (rowOk) smOkN++; else smBad++;
                Log("smolder " + m.name + " d=" + m.d + " r=" + rSm + " oval=" + oval + " want=" + want + " " + (rowOk ? "OK" : "BAD"));
            }
            yield return Shot1822(dir + "/bugfix_2e80984_smolder_ring.png");
            Log("smolder r=" + rSm + " ZhOn=" + ZhOn1618 + " rings=" + _zRings1618.Count + " ok=" + smOkN + " bad=" + smBad);

            // Cards: Zhong Kui kit descriptions + support wrap / TO:
            foreach (var e in smPlaced) if (e != null) e.alive = false;
            _zRings1618.Clear();
            T.nkKit1713 = true; T.kitAnyPact1618 = false;
            _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 });
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonArchers), level = 0 }); // shows LV 0-1 + "1 ARCHER"
            SetHeroLayers();
            cards.Clear();
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 4 }); // Jiangshi
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 3 }); // Skeletal Mage
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 1 }); // Ossuary Nova
            cards.Add(new Card { kind = CardKind.NewSkill, kit = 3, kitPick = 3 }); // Grave Legion
            _chestTitle = "KIT CARDS"; _chestSub = "FULL TEXT MUST FIT";
            _tp = null; mode = Mode.Chest;
            Log("NkOn=" + NkOn1713 + " card lines J/M/O/L=" + Wrap(ZhKitDesc1639(cards[0]), CardWrap).Count + "/" + Wrap(ZhKitDesc1639(cards[1]), CardWrap).Count + "/" + Wrap(ZhKitDesc1639(cards[2]), CardWrap).Count + "/" + Wrap(ZhKitDesc1639(cards[3]), CardWrap).Count);
            for (int i = 0; i < 12; i++) yield return null;
            yield return Shot1822(dir + "/bugfix_2e80984_cards_kit.png");

            cards.Clear();
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Executioner) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.ExplodeOnKill) });
            cards.Add(new Card { kind = CardKind.Support, support = HData.Support(SupportId.Bleed) });
            cards.Add(new Card { kind = CardKind.SkillLevel, skillIndex = 1 }); // 1 ARCHER
            _chestTitle = "SUPPORT CARDS"; _chestSub = "SEVERAL FIT = YOU PICK";
            mode = Mode.Chest;
            for (int i = 0; i < 12; i++) yield return null;
            yield return Shot1822(dir + "/bugfix_2e80984_cards_support.png");

            // WHICH SKILL? picker with Mage + Ossuary Nova
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.NkSkeletalMage), level = 1 });
            skills.Add(new SkillInst { def = HData.Skill(SkillId.NkOssuaryNova), level = 1 });
            SetHeroLayers();
            cards.Clear();
            OpenTargetPick(HData.Support(SupportId.Executioner), new List<int> { 0, 1 }, false, _ => { });
            mode = Mode.Chest;
            var mageIc = ZhCardIcon1625(SkillId.NkSkeletalMage);
            var ossIc = ZhCardIcon1625(SkillId.NkOssuaryNova);
            Log("picker icons mage=" + (mageIc != null ? mageIc.name : "NULL") + " ossuary=" + (ossIc != null ? ossIc.name : "NULL"));
            for (int i = 0; i < 12; i++) yield return null;
            yield return Shot1822(dir + "/bugfix_2e80984_picker.png");
            _tp = null;

            // F1 PACT tab: first the top (footer visible, no overlap), then scrolled to the bottom
            mode = Mode.Play; cards.Clear();
            _tmOpen = true; _tmTab = TmPactTab53; _tmSel = 0; _tmScroll = Vector2.zero; _tmResetArm53 = false;
            for (int i = 0; i < 12; i++) yield return null;
            yield return Shot1822(dir + "/bugfix_2e80984_f1_pact.png");
            _tmScroll = new Vector2(0f, 4000f);
            for (int i = 0; i < 8; i++) yield return null;
            yield return Shot1822(dir + "/bugfix_2e80984_f1_pact_scrolled.png");
            _tmOpen = false;

            int z = CamZoom();
            bool camOk = Screen.width == 1920 && Screen.height == 1080 && z == 3 && T.camTargetPx49.x == 640f && T.camTargetPx49.y == 360f;
            bool files = System.IO.File.Exists(dir + "/bugfix_2e80984_storm_ring.png")
                && System.IO.File.Exists(dir + "/bugfix_2e80984_smolder_ring.png")
                && System.IO.File.Exists(dir + "/bugfix_2e80984_cards_kit.png")
                && System.IO.File.Exists(dir + "/bugfix_2e80984_cards_support.png")
                && System.IO.File.Exists(dir + "/bugfix_2e80984_picker.png")
                && System.IO.File.Exists(dir + "/bugfix_2e80984_f1_pact.png")
                && System.IO.File.Exists(dir + "/bugfix_2e80984_f1_pact_scrolled.png");
            bool iconsOk = mageIc != null && ossIc != null;
            bool ok = camOk && files && stormBad == 0 && smBad == 0 && iconsOk && NkOn1713 && ex == null;
            Log("screen " + Screen.width + "x" + Screen.height + " zoom=" + z + " camTarget=" + T.camTargetPx49 + " (expect 640x360 @ zoom 3)");
            if (ex != null) Log("exception " + ex);
            Log(ok ? "PASS" : "FAIL cam=" + camOk + " files=" + files + " stormBad=" + stormBad + " smBad=" + smBad + " icons=" + iconsOk + " ex=" + (ex ?? "none"));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }

        System.Collections.IEnumerator Shot1822(string path)
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < 25 && !System.IO.File.Exists(path); i++) yield return null;
            Debug.Log("[t1822] screenshot " + path + " " + (System.IO.File.Exists(path) ? ("ok " + new System.IO.FileInfo(path).Length + " bytes") : "MISSING"));
        }
    }
}
