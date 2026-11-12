using UnityEngine;
using PixelFXPack;

// IMGUI HUD + card screens (no UGUI/TMP dependency). Layout in a 960x540 virtual canvas scaled to the screen.
namespace Hellord
{
    public partial class HellordGame
    {
        Matrix4x4 _hudM = Matrix4x4.identity; GUIStyle _box, _title, _body, _small, _btn, _warn, _center; Texture2D _white; float _fps = 60;
        const float VW = 960, VH = 540;

        void InitStyles()
        {
            if (_box != null) return;
            _white = Texture2D.whiteTexture;
            _box = new GUIStyle(GUI.skin.box) { normal = { background = Tex(new Color(0.08f, 0.03f, 0.05f, 0.92f)) }, padding = new RectOffset(10, 10, 8, 8) };
            _title = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = new Color(1f, 0.85f, 0.5f) } };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true, normal = { textColor = new Color(0.95f, 0.9f, 0.88f) } };
            _small = new GUIStyle(_body) { fontSize = 11 };
            _warn = new GUIStyle(_body) { fontSize = 12, normal = { textColor = new Color(1f, 0.45f, 0.35f) } };
            _btn = new GUIStyle(GUI.skin.button) { fontSize = 13, wordWrap = true };
            _center = new GUIStyle(_title) { alignment = TextAnchor.MiddleCenter, fontSize = 28 };
        }
        static Texture2D Tex(Color c) { var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t; }
        void Bar(Rect r, float t, Color c) { GUI.color = new Color(0, 0, 0, 0.7f); GUI.DrawTexture(r, _white); GUI.color = c; GUI.DrawTexture(new Rect(r.x + 1, r.y + 1, (r.width - 2) * Mathf.Clamp01(t), r.height - 2), _white); GUI.color = Color.white; }

        void OnGUI()
        {
            GUI.depth = 0; DrawLowResWorld();
            InitStyles();
            _fps = Mathf.Lerp(_fps, 1f / Mathf.Max(1e-4f, Time.unscaledDeltaTime), 0.05f);
            // v0.7: in-run HUD = Option B on a 640x360 canvas at the same integer zoom as the camera (HellordHud.cs)
            // v0.8: everything in-run is on the 640x360 canvas (HUD Option B, floor UI, cards, death/won); F9 debug stays on the old 960 canvas
            // v0.0.9 perf: OnGUI runs for Layout + Repaint (+ input) events; in play there is nothing to click, so only paint once
            if ((mode == Mode.Play || mode == Mode.Transition) && Event.current.type != EventType.Repaint && _tp == null && _sinSwap737 == null) { TestMenuGui(); return; } // v0.0.38: the TEST button / menu needs mouse events
            HudBegin();
            if (mode == Mode.MainMenu || mode == Mode.Altar) { if (!_tmOpen || Event.current.type == EventType.Repaint) DrawPact51(); TestMenuGui(); return; } // pt480u: F1 PACT test tab on menu/altar, no click-through // v0.0.51
            if (mode != Mode.StartPick) { DrawHudB(); if (floorMode) DrawFloorWorldUI(); if (mode == Mode.Play || mode == Mode.Transition || mode == Mode.Won) { if (HellordTuning.I.skillBelt720) DrawBelt716(); else DrawSkillBarB(); } }
            if (mode == Mode.Play) DrawCtrl716(); // t716u: Controls page + slot pick
            if (mode == Mode.Play && _tp != null) DrawTargetPickB(); // v0.0.36: shop second pick
            if (mode == Mode.Play) DrawFX26ResumeDim(); // v0.0.26 LevelUpFX: dim fades out after the pick
            if (mode == Mode.StartPick && _tmOpen) { }
            else if (mode == Mode.StartPick) DrawCardsB("HELLORD", "CHOOSE YOUR STARTING SKILL", false);
            else if (mode == Mode.LevelUp) DrawCardsB("LEVEL " + ShownLevel + "!", pendingLevels > 0 ? "PICK ONE  -  " + pendingLevels + " MORE" : "PICK ONE", true);
            else if (mode == Mode.Chest) DrawCardsB(_chestTitle ?? "CHEST", _chestSub ?? "TAKE 1 SUPPORT", false);
            else if (mode == Mode.Dead) DrawDeathB();
            else if (mode == Mode.Won) DrawWonB();
            if (toastT > 0 && !string.IsNullOrEmpty(toast) && (mode == Mode.Play || mode == Mode.Transition || mode == Mode.Won)) DrawToastNearHero();
            DrawBannerB(); // v0.0.26: big centred notices
            DrawRageBannerB();
            if (mode == Mode.Play || mode == Mode.Transition) DrawSinBetter737(); // t784u BETTER ROLL notice
            if (mode == Mode.Play && _sinSwap737 != null) DrawSinSwap737(); // t770u Infernal Sins swap card
            if (floorMode) DrawFadeB();
            // v0.0.13: remember where the 640x360 HUD sits on screen so the debug overlay can avoid it
            _hudM = GUI.matrix;
            float sc = Mathf.Min(Screen.width / VW, Screen.height / VH);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - VW * sc) / 2, (Screen.height - VH * sc) / 2, 0), Quaternion.identity, new Vector3(sc, sc, 1));
            if (showDebug && (mode == Mode.Play || mode == Mode.Transition)) DrawHud(); // v0.0.14: hidden on card / chest / death screens (covered the left card) // F9: v0.6 debug text + skill slots
            TestMenuGui(); // v0.0.38: on top of everything
        }

        // v0.0.10: FX self-check - counts what is actually on screen so a skill that hits but draws nothing is caught at a glance
        void DrawFxSelfCheck(System.Action<string> L)
        {
            int pa = 0, pv = 0; foreach (var p in _pr) if (p.alive) { pa++; if (p.sr && p.sr.gameObject.activeInHierarchy && p.sr.enabled && p.sr.sprite) pv++; }
            int ov = 0; if (_orbitJobs != null) foreach (var j in _orbitJobs) if (j != null && j.sr && j.sr.gameObject.activeInHierarchy) ov++;
            foreach (var o in _orbs) if (o && o.gameObject.activeInHierarchy && o.enabled) ov++;
            int hfx = HellFXRunner.I.Alive;
            L($"FX: proj {pv}/{pa} vis  orbs {ov}  hellfx {hfx}  aura {(_aura != null && _aura.sr && _aura.sr.gameObject.activeInHierarchy ? "on" : "-")}");
            string warn = "";
            foreach (var s in skills)
            {
                bool recent = Time.time - _lastCast[(int)s.def.id] < 1.5f;
                switch (s.def.id)
                {
                    case SkillId.Fireball: if (recent && pa > 0 && pv == 0) warn += " FIREBALL-INVISIBLE"; break;
                    case SkillId.BoneOrbit: if (ov == 0) warn += " ORBIT-INVISIBLE"; break;
                    case SkillId.StormRing: if (_aura == null || !_aura.sr || !_aura.sr.gameObject.activeInHierarchy) warn += " RING-NO-AURA"; break;
                    case SkillId.CurseRune: if (recent && hfx == 0) warn += " CURSE-NO-FX"; break; // FrostCleave draws via ReactionFX (own pool), not counted here
                }
            }
            // v0.0.13: boss + status-shader checks (catch 'sprite erased by a shader param' bugs like the v0.0.11 burn dissolve)
            if (BossAlive && _bossBody != null && (!_bossBody.gameObject.activeInHierarchy || !_bossBody.enabled || _bossBody.sprite == null)) warn += " BOSS-HIDDEN";
            else if (BossAlive && _bossOffT > 0.5f) warn += " BOSS-OFFSCREEN";
            foreach (var e in _en) if (e.alive && e.fx != null && (e.fx.burnAmount > 0 || e.fx.dissolveAmount > 0 || e.fx.ditherFade > 0 || e.fx.silhouetteAmount > 0)) { warn += " ENEMY-ERASED"; break; }
            if (warn.Length > 0) { var c = GUI.color; GUI.color = new Color(1f, 0.35f, 0.3f); L("FX WARN:" + warn); GUI.color = c; }
            else L("FX check OK");
        }
        float _bossOffT; // v0.0.14: seconds the live boss has been outside the camera view (updated in UpdateCamera)
        void DrawHud()
        {
            // v0.0.9: debug text moved to a left column under the HP/XP widgets (was y 100 across the goal box / minimap / toast)
            int alive = 0; for (int i = 0; i < _en.Length; i++) if (_en[i].alive) alive++;
            // v0.0.13: start below the HP/XP widgets (HUD canvas y 34) whatever the screen size / zoom (was a fixed y 50 that hit the HP bar)
            float sc = Mathf.Min(Screen.width / VW, Screen.height / VH), oy = (Screen.height - VH * sc) / 2;
            float HudToDbgY(float hy) => (_hudM.MultiplyPoint3x4(new Vector3(0, hy, 0)).y - oy) / sc;
            float HudToDbgX(float hx) => (_hudM.MultiplyPoint3x4(new Vector3(hx, 0, 0)).x - (Screen.width - VW * sc) / 2) / sc;
            float ly = Mathf.Max(4, HudToDbgY(36)), lx = Mathf.Max(4, HudToDbgX(4));
            void L(string t) { var tc = GUI.color; GUI.color = new Color(0.07f, 0.05f, 0.1f, 0.85f); GUI.DrawTexture(new Rect(lx - 2, ly, 330, 14), _white); GUI.color = tc; GUI.Label(new Rect(lx, ly, 330, 16), t, _small); GUI.color = Color.white; ly += 14; }
            L($"FPS {_fps:0}  enemies {alive}  room {(_room != null ? _room.Name : "-")}");
            L($"Lv {ShownLevel}  Kills {kills}  Bones {bones}  Aim {(autoAim ? "AUTO" : "MOUSE")} [Tab]");
            L("Sh+1 lvl  Sh+2 elite  Sh+3 tf  Sh+4 boss  Sh+8 clear  Sh+0 skill  M map  K");
            L($"Sh+5 hitstop {(TellFX.JuiceHitStop ? "ON" : "off")}  Sh+6 wobble {(TellFX.StunWobble ? "ON" : "off")}  Sh+7 impact {(TellFX.ImpactFrames ? "ON" : "off")}");
            L("(desktop: F1-F10, F12 = skill also work)");
            DrawFxSelfCheck(L);
            // skill slots
            int unlocked = HData.SlotsUnlocked(level);
            for (int i = 0; i < 3; i++)
            {
                // v0.0.14: moved out of the play area - stacked in the left debug column under the text (was centred on the bottom edge, hid hero/boss)
                var r = new Rect(lx - 2, ly + 4 + i * 58, 146, 54);
                // v0.0.13: solid backing (HUD ink #181425 + 1px light rim) - the old 0.92 GUI.Box let the world show through
                GUI.color = new Color(0.55f, 0.4f, 0.32f, 1f); GUI.DrawTexture(new Rect(r.x - 1, r.y - 1, r.width + 2, r.height + 2), _white);
                GUI.color = HInk; GUI.DrawTexture(r, _white); GUI.color = Color.white;
                if (i < skills.Count)
                {
                    var k = skills[i];
                    GUI.Label(new Rect(r.x + 6, r.y + 3, 140, 18), $"{k.def.name}  Lv{k.level}", _body);
                    for (int p = 0; p < 4; p++)
                    {
                        var pr = new Rect(r.x + 6 + p * 35, r.y + 24, 31, 24);
                        bool open = p < Mathf.Min(k.level, 4);
                        GUI.color = p < k.sup.Count ? new Color(1f, 0.65f, 0.2f) : open ? new Color(0.35f, 0.25f, 0.25f) : new Color(0.12f, 0.08f, 0.08f);
                        GUI.DrawTexture(pr, _white); GUI.color = Color.white;
                        if (p < k.sup.Count) GUI.Label(pr, Abbrev(HData.Support(k.sup[p]).name) + k.Lvl(k.sup[p]), _supLbl ?? (_supLbl = new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } }));
                    }
                }
                else GUI.Label(new Rect(r.x + 6, r.y + 16, 140, 20), i < unlocked ? "(free slot)" : $"Locked: Lv {(i == 1 ? 5 : 12)}", _small);
            }
        }
        GUIStyle _supLbl;
        static string Abbrev(string n) { var p = n.Split(' '); return p.Length > 1 ? (p[0].Substring(0, 1) + p[1].Substring(0, 2)) : n.Substring(0, Mathf.Min(4, n.Length)); }

        Card _pc; int _pt;
        void Pick(Card c, int t) { _pc = c; _pt = t; }
        void DrawCards(string header, bool canReroll)
        {
            GUI.color = new Color(0, 0, 0, 0.55f); GUI.DrawTexture(new Rect(0, 0, VW, VH), _white); GUI.color = Color.white;
            GUI.Label(new Rect(0, 40, VW, 40), header, _center);
            int n = cards.Count; float w = n > 3 ? 172 : 240, gap = 12, total = n * w + (n - 1) * gap, x0 = (VW - total) / 2;
            for (int i = 0; i < n; i++)
            {
                var c = cards[i]; var r = new Rect(x0 + i * (w + gap), 110, w, 320);
                GUI.Box(r, GUIContent.none, _box);
                GUILayout.BeginArea(new Rect(r.x + 10, r.y + 8, r.width - 20, r.height - 16));
                GUILayout.Label($"[{i + 1}] {Kind(c)}", _small);
                switch (c.kind)
                {
                    case CardKind.NewSkill:
                        GUILayout.Label(c.skill.name, _title); GUILayout.Label(c.skill.desc, _body); GUILayout.Label("Tags: " + HData.TagText(c.skill.tags, " · ", false), _small);
                        GUILayout.FlexibleSpace(); if (GUILayout.Button("Take skill", _btn, GUILayout.Height(34))) Pick(c, -1); break;
                    case CardKind.SkillLevel:
                        var s = skills[c.skillIndex]; int nl = s.level + 1;
                        GUILayout.Label($"{s.def.name} Lv{s.level} -> Lv{nl}", _title);
                        GUILayout.Label(nl >= HData.MaxSkillLevel ? "MAX level: damage x1.5 spike" : $"+30% base damage, opens support slot {nl}", _body);
                        GUILayout.FlexibleSpace(); if (GUILayout.Button("Level up", _btn, GUILayout.Height(34))) Pick(c, -1); break;
                    case CardKind.Stat:
                        GUILayout.Label(c.stat.name, _title); GUILayout.Label(HData.StatDesc(c.stat), _body);
                        GUILayout.FlexibleSpace(); if (GUILayout.Button("Take", _btn, GUILayout.Height(34))) Pick(c, -1); break;
                    case CardKind.Support:
                        GUILayout.Label(c.support.name, _title); GUILayout.Label(c.support.desc, _body); { int ti = SupportTarget(c.support); int cl = ti >= 0 ? skills[ti].Lvl(c.support.id) : 0; GUILayout.Label((cl > 0 ? $"UPGRADE Lv{cl} -> Lv{cl + 1}: " : "NEW Lv1: ") + HData.SupportEffect(c.support.id, cl + 1), _small); } GUILayout.Label("Needs: " + HData.TagText(c.support.needs, " or ", false) + (c.support.excludes != 0 ? "  (not " + HData.TagText(c.support.excludes, "/", false) + ")" : ""), _small);
                        GUILayout.FlexibleSpace(); GUILayout.Label("Put it in:", _small);
                        for (int k = 0; k < skills.Count; k++)
                        {
                            string why = skills[k].CanSocket(c.support);
                            GUI.enabled = why == null;
                            if (GUILayout.Button(skills[k].def.name + (why == null ? (skills[k].Has(c.support.id) ? $"  (upgrade Lv{skills[k].Lvl(c.support.id)}->{skills[k].Lvl(c.support.id) + 1})" : $"  ({skills[k].OpenSlots} open)") : "  (x)"), _btn, GUILayout.Height(28))) Pick(c, k);
                            GUI.enabled = true;
                            if (why != null) GUILayout.Label("! " + why, _warn);
                        }
                        break;
                }
                GUILayout.EndArea();
            }
            if (_pc != null) { var pc = _pc; _pc = null; Choose(pc, _pt); }
            if (canReroll)
            {
                // t1058u: enabled when free or enough bones; label shows cost
                GUI.enabled = CanReroll1058();
                string lbl = freeRerolls > 0 ? $"Reroll FREE x{freeRerolls} [Space]" : $"Reroll {RerollBoneCost1058()} bones [Space]";
                if (GUI.Button(new Rect(VW / 2 - 120, 446, 240, 32), lbl, _btn)) Reroll();
                GUI.enabled = true;
            }
            if (!string.IsNullOrEmpty(warning)) GUI.Label(new Rect(0, 486, VW, 24), "! " + warning, new GUIStyle(_warn) { alignment = TextAnchor.MiddleCenter });
            GUI.Label(new Rect(0, 512, VW, 20), "Keys 1-5 pick (supports go to the first skill that fits)", new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter });
        }
        static string Kind(Card c) => c.kind == CardKind.NewSkill ? "NEW SKILL" : c.kind == CardKind.SkillLevel ? "SKILL LEVEL" : c.kind == CardKind.Support ? "SUPPORT" : "STAT";

        void DrawDeath()
        {
            GUI.color = new Color(0.2f, 0, 0, 0.7f); GUI.DrawTexture(new Rect(0, 0, VW, VH), _white); GUI.color = Color.white;
            GUI.Label(new Rect(0, 150, VW, 50), "YOU DIED", new GUIStyle(_center) { fontSize = 44 });
            int m = (int)(runTime / 60), s = (int)(runTime % 60);
            GUI.Label(new Rect(0, 215, VW, 30), $"Survived {m:00}:{s:00}   Level {level}   Kills {kills}   Bones {bones}", new GUIStyle(_center) { fontSize = 16 });
            if (GUI.Button(new Rect(VW / 2 - 90, 280, 180, 44), "Restart [R]", _btn)) { ResetRun(); PactAfterReset51(); } // v0.0.53 pact: same path as the R key
        }
    }
}
