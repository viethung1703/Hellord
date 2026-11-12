using PixelFXPack;
using System;
using System.Collections.Generic;
using UnityEngine;

// v0.0.38: in-game TEST MENU for the designer. F1 (or the small TEST button, top left) opens it and PAUSES the game.
// Mouse-clickable, plus keyboard (arrows + Enter, Esc / F1 closes). Switch off with HellordTuning.testMenuEnabled for a release.
// Everything here is a debug tool: it goes through the normal game code paths (transitions, spawns, reward picks), no cheats in shipped logic
// except the god-mode check in HurtHero and the speed toggle.
namespace Hellord
{
    public partial class HellordGame
    {
        bool _tmOpen, _tmGod, _tmFast; int _tmTab, _tmSel, _tmCount, _tmCols = 3, _tmTarget, _tmSpawnN = 1, _tmI, _jumpFloor = -1;
        string _tmMsg = "Click a button, or arrows + Enter. F1 / Esc closes."; Action _tmPending; bool _tmAct, _tmDownK, _tmUpK, _tmLeftK, _tmRightK;
        GUIStyle _tmBtn, _tmTab0, _tmHead, _tmTxt;
        static readonly string[] TmTabs = { "FLOOR", "PLAYER", "SKILLS", "SUPPORTS", "ENEMIES", "BOSS", "PACT", "SINS", "KEYS" }; // t770u: SINS tab // v0.0.53 pt480u: PACT tab (souls / sigils / reset save)
        static readonly string[] TmFloorNames = { "Ox Pit", "Frozen Hell", "Burning Hell" }; // v0.0.42: themes (any depth)

        bool TestMenuOn => HellordTuning.I.testMenuEnabled;

        /// <summary>Called early in Update. Handles F1, the pending action after a jump, the x2 speed and returns true while the menu pauses the game.</summary>
        bool TestMenuUpdate()
        {
            var T = HellordTuning.I;
            if (!T.testMenuEnabled) { _tmOpen = false; _tmGod = false; _tmPending = null; if (_tmFast) { _tmFast = false; if (!HitStop.Active) Time.timeScale = 1f; } return false; }
            if (!HitStop.Active) { float want = _tmFast ? Mathf.Max(0.1f, T.testFastSpeed) : 1f; if (Mathf.Abs(Time.timeScale - want) > 0.001f) Time.timeScale = want; }
            bool ok = mode == Mode.Play || mode == Mode.StartPick || MenuMode53; // pt480u: also on the main menu (PACT tab)
            if (_tmOpen && !ok) _tmOpen = false;
            if (ok && HInput.Down(HKey.F1)) { _tmOpen = !_tmOpen; _tmSel = 0; _tmResetArm53 = false; if (MenuMode53) _tmTab = TmPactTab53; }
            if (mode == Mode.Play && !_tmOpen && _tmPending != null) { var a = _tmPending; _tmPending = null; a(); }
            if (_tmGod && hp > 0 && hp < maxHp * 0.25f) hp = maxHp; // god mode safety net for damage that bypasses HurtHero
            return _tmOpen;
        }

        void TmClose() { _tmOpen = false; }
        void TmSay(string s) { _tmMsg = s; Toast(s); }

        // ---------- GUI ----------
        void TestMenuGui()
        {
            if (!TestMenuOn) return;
            bool ok = mode == Mode.Play || mode == Mode.StartPick || MenuMode53;
            if (!ok) return;
            if (_tmBtn == null)
            {
                _tmBtn = new GUIStyle(GUI.skin.button) { fontSize = 13, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
                _tmTab0 = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold };
                _tmHead = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
                _tmTxt = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true, normal = { textColor = new Color(0.92f, 0.9f, 0.96f) } };
            }
            var old = GUI.matrix; var oc = GUI.color; bool oe = GUI.enabled; int od = GUI.depth;
            float sc = Mathf.Min(Screen.width / VW, Screen.height / VH);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - VW * sc) / 2, (Screen.height - VH * sc) / 2, 0), Quaternion.identity, new Vector3(sc, sc, 1));
            GUI.depth = -10; GUI.enabled = true; GUI.color = Color.white;
            var ev = Event.current;
            if (!_tmOpen)
            {
                // t1175 (VH): no on-screen 'TEST F1' badge; the test menu still opens with F1.
            }
            else
            {
                _tmDownK = _tmUpK = _tmLeftK = _tmRightK = _tmAct = false; bool used = false;
                if (ev.type == EventType.KeyDown)
                {
                    switch (ev.keyCode)
                    {
                        case KeyCode.DownArrow: _tmDownK = used = true; break;
                        case KeyCode.UpArrow: _tmUpK = used = true; break;
                        case KeyCode.LeftArrow: _tmLeftK = used = true; break;
                        case KeyCode.RightArrow: _tmRightK = used = true; break;
                        case KeyCode.Return: case KeyCode.KeypadEnter: case KeyCode.Space: _tmAct = used = true; break;
                        case KeyCode.Tab: _tmTab = (_tmTab + (ev.shift ? TmTabs.Length - 1 : 1)) % TmTabs.Length; _tmSel = 0; used = true; break;
                        case KeyCode.PageDown: _tmTab = (_tmTab + 1) % TmTabs.Length; _tmSel = 0; used = true; break;
                        case KeyCode.PageUp: _tmTab = (_tmTab + TmTabs.Length - 1) % TmTabs.Length; _tmSel = 0; used = true; break;
                        case KeyCode.Escape: TmClose(); used = true; break;
                    }
                }
                if (_tmOpen) DrawTestMenu(); // (Esc may have closed it this event)
                if (used) ev.Use();
            }
            GUI.matrix = old; GUI.color = oc; GUI.enabled = oe; GUI.depth = od;
        }

        void DrawTestMenu()
        {
            var P = new Rect(50, 24, 860, 492);
            GUI.color = new Color(0, 0, 0, 0.55f); GUI.DrawTexture(new Rect(0, 0, VW, VH), _white);
            GUI.color = new Color(0.08f, 0.06f, 0.12f, 0.97f); GUI.DrawTexture(P, _white); GUI.color = Color.white;
            GUI.Label(new Rect(P.x + 14, P.y + 6, 600, 28), "TEST MENU   (game paused)", _tmHead);
            if (GUI.Button(new Rect(P.xMax - 92, P.y + 8, 80, 24), "CLOSE", _tmTab0)) TmClose();
            // tabs
            float tw = (P.width - 28) / TmTabs.Length;
            for (int i = 0; i < TmTabs.Length; i++)
            {
                GUI.color = i == _tmTab ? new Color(1f, 0.8f, 0.3f) : Color.white;
                if (GUI.Button(new Rect(P.x + 14 + i * tw, P.y + 40, tw - 4, 26), TmTabs[i], _tmTab0)) { _tmTab = i; _tmSel = 0; }
            }
            GUI.color = Color.white;
            // content: auto grid
            _tmI = 0; _tmCols = 3;
            if (_tmDownK) _tmSel += _tmCols; if (_tmUpK) _tmSel -= _tmCols; if (_tmRightK) _tmSel++; if (_tmLeftK) _tmSel--;
            if (_tmCount > 0) _tmSel = Mathf.Clamp(_tmSel, 0, _tmCount - 1);
            _gx = P.x + 14; _gy = P.y + 76; _gw = (P.width - 28 - 16) / 3f; _gcol = 0;
            if (MenuMode53 && _tmTab != TmPactTab53 && _tmTab != TmSinTab737 && _tmTab != TmTabs.Length - 1) _tmTab = TmPactTab53; // other tabs need a run
            switch (_tmTab)
            {
                case 0: TabFloor(); break;
                case 1: TabPlayer(); break;
                case 2: TabSkills(); break;
                case 3: TabSupports(); break;
                case 4: TabEnemies(); break;
                case 5: TabBoss(); break;
                case TmPactTab53: TabPact53(); break;
                case TmSinTab737: TabSins737(); break;
                default: TabKeys(P); break;
            }
            _tmCount = _tmI;
            GUI.color = new Color(1f, 0.9f, 0.5f); GUI.Label(new Rect(P.x + 14, P.yMax - 30, P.width - 28, 24), _tmMsg, _tmTxt); GUI.color = Color.white;
        }

        float _gx, _gy, _gw; int _gcol; const float GH = 28, GG = 6;
        bool TmBtn(string label, bool enabled = true, bool on = false)
        {
            var r = new Rect(_gx + _gcol * (_gw + 8), _gy, _gw, GH); int idx = _tmI++;
            bool sel = idx == _tmSel;
            GUI.color = new Color(1, 1, 1, 1);
            if (sel) { GUI.color = new Color(1f, 0.85f, 0.3f); GUI.DrawTexture(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), _white); GUI.color = new Color(0.08f, 0.06f, 0.12f, 1f); GUI.DrawTexture(r, _white); GUI.color = Color.white; } // v0.0.53 B12: yellow frame + dark fill so text stays readable
            if (on && !sel) GUI.color = new Color(0.6f, 1f, 0.6f);
            GUI.enabled = enabled; bool hit = GUI.Button(r, label, _tmBtn); GUI.enabled = true; GUI.color = Color.white;
            if (++_gcol >= 3) { _gcol = 0; _gy += GH + GG; }
            if (hit) { _tmSel = idx; return true; }
            if (sel && _tmAct && Event.current.type == EventType.KeyDown) { _tmAct = false; return enabled; }
            return false;
        }
        void TmRow() { if (_gcol != 0) { _gcol = 0; _gy += GH + GG; } }
        void TmLabel(string s) { TmRow(); GUI.color = new Color(0.7f, 0.85f, 1f); GUI.Label(new Rect(_gx, _gy, 800, 22), s, _tmTxt); GUI.color = Color.white; _gy += 22; }

        // ---------- tabs ----------
        void TabFloor()
        {
            // v0.0.42: depth (difficulty) and theme (look / boss / hazards) are separate now; the run seed decides the theme order
            TmLabel("RUN SEED " + RunSeed42 + "   THEMES: " + ThemeOrderText42());
            TmLabel("JUMP TO A DEPTH (new random rooms; keeps skills / level)");
            for (int f = 0; f < Mathf.Max(3, FloorCount); f++) if (TmBtn("Depth " + (f + 1) + " - " + FloorLabel53(ThemeAt(f), false), f < FloorCount, FloorIdx == f)) { int ff = f; TmAction(() => TmJumpFloor(ff)); }
            TmLabel("JUMP TO A THEME (its depth in this run; if it is not in this run, the current depth is re-themed)");
            for (int th = 0; th < 3; th++) if (TmBtn(TmFloorNames[th], true, Theme == th)) { int tt = th; TmAction(() => TmJumpFloor(DepthForTheme42(tt))); }
            TmLabel("THIS FLOOR (depth " + (FloorIdx + 1) + ", " + TmFloorNames[Mathf.Clamp(Theme, 0, 2)] + ")");
            if (TmBtn("Boss room (fight starts)")) TmAction(TmJumpBoss);
            if (TmBtn("Start room")) TmAction(() => TmJumpRoom(RoomType.Start));
            if (TmBtn(_mapRevealed ? "Reveal secret rooms: ON" : "Reveal secret rooms: OFF", true, _mapRevealed)) FloorRevealMap();
            TmLabel("JUMP TO A ROOM TYPE (if this floor has none, a fight room is converted)");
            if (TmBtn("Shop (bone shrine)")) TmAction(() => TmJumpRoom(RoomType.Shrine));
            if (TmBtn("Treasure room")) TmAction(() => TmJumpRoom(RoomType.Treasure));
            if (TmBtn("Elite room")) TmAction(() => TmJumpRoom(RoomType.Elite));
            if (TmBtn("Curse altar room")) TmAction(() => TmJumpRoom(RoomType.Curse));
            if (TmBtn("Survive room")) TmAction(() => TmJumpRoom(RoomType.Survive));
            if (TmBtn("Hunt room")) TmAction(() => TmJumpRoom(RoomType.Hunt));
            if (TmBtn("Nests room")) TmAction(() => TmJumpRoom(RoomType.Nests));
            if (TmBtn("Secret room")) TmAction(() => TmJumpRoom(RoomType.Secret));
        }
        void TabPlayer()
        {
            TmLabel("HERO");
            if (TmBtn("+1 level (opens the level-up pick)")) TmAction(() => { GainLevel(); TmClose(); _tmMsg = "level " + level; });
            if (TmBtn("+5 levels (5 picks in a row)")) TmAction(() => { for (int i = 0; i < 5; i++) GainLevel(); TmClose(); });
            if (TmBtn("Full heal")) TmAction(() => { hp = maxHp; TmSay("Full heal"); });
            if (TmBtn(_tmGod ? "God mode: ON" : "God mode: OFF", true, _tmGod)) { _tmGod = !_tmGod; TmSay("God mode " + (_tmGod ? "ON (no damage)" : "OFF")); }
            if (TmBtn("SIN SELF-TEST (t818u)")) TmAction(() => { string r = SinSelfTest818(); TmSay(r == "" ? "sins t818u: all checks pass" : "FAIL: " + r); Debug.Log("[t818u] sin self-test: " + (r == "" ? "PASS" : r)); });
            if (TmBtn("+" + HellordTuning.I.testBones + " bones")) TmAction(() => { bones += HellordTuning.I.testBones; TmSay("+" + HellordTuning.I.testBones + " bones (now " + bones + ")"); });
            if (TmBtn(_tmFast ? "Game speed x2: ON" : "Game speed x2: OFF", true, _tmFast)) { _tmFast = !_tmFast; TmSay("Game speed " + (_tmFast ? "x" + HellordTuning.I.testFastSpeed : "normal")); }
            if (TmBtn(_tmFreeze44 ? "Hero frozen: ON" : "Hero frozen: OFF", true, _tmFreeze44)) { _tmFreeze44 = !_tmFreeze44; TmSay("Hero frozen " + (_tmFreeze44 ? "ON" : "OFF")); }
            if (TmBtn("+1 Chill stack")) TmAction(() => { AddChill44(1); TmSay("Chill stacks " + _chill44 + (HeroFrozen44 ? " (FROZEN)" : "")); });
            TmLabel("STATUS");
            TmRow(); GUI.Label(new Rect(_gx, _gy, 800, 24), $"Level {level}   HP {Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}   bones {bones}   kills {kills}   floor {FloorIdx + 1}   skills {skills.Count}", _tmTxt);
        }
        void TabSkills()
        {
            TmLabel("Column 1: give  |  column 2: level up (max 5)  |  column 3: remove.   Owned: " + skills.Count + " / 3");
            foreach (var d in HData.Skills)
            {
                var def = d; SkillInst own = skills.Find(x => x.def.id == def.id);
                if (TmBtn(own != null ? def.name + "  (Lv " + own.level + ", " + own.sup.Count + " sup)" : "Give " + def.name, own == null, own != null)) TmAction(() => TmGiveSkill(def), false);
                if (TmBtn("Level up " + def.name, own != null && own.level < HData.MaxLv(own.def))) TmAction(() => { var s = skills.Find(x => x.def.id == def.id); if (s != null && s.level < HData.MaxSkillLevel) { s.level++; TmSay(def.name + " -> Lv " + s.level); } });
                if (TmBtn("Remove " + def.name, own != null)) TmAction(() => { int i = skills.FindIndex(x => x.def.id == def.id); if (i >= 0) { skills.RemoveAt(i); SetHeroLayers(); TmSay("Removed " + def.name); } });
            }
        }
        void TabSupports()
        {
            if (skills.Count == 0) { TmLabel("No skills yet - give one in the SKILLS tab."); return; }
            _tmTarget = Mathf.Clamp(_tmTarget, 0, skills.Count - 1);
            TmLabel("1) pick the target skill   (column order = skill bar order)");
            for (int i = 0; i < skills.Count && i < 5; i++) if (TmBtn("Target: " + skills[i].def.name + " Lv" + skills[i].level, true, i == _tmTarget)) _tmTarget = i;
            TmRow();
            var t = skills[_tmTarget];
            TmLabel("2) click a support: gives it (if it fits + has a slot) or levels it up (max 5).  Slots on " + t.def.name + ": " + t.sup.Count + "/" + Mathf.Min(t.level, HData.MaxSupports));
            foreach (var sd0 in HData.Supports)
            {
                var sd = sd0; int lv = t.Lvl(sd.id); bool fits = HData.Fits(sd, t.def);
                string lab = (lv > 0 ? "[Lv" + lv + "] " : "") + sd.name + "  (" + sd.needsLabel + ")" + (fits ? "" : " - no fit");
                if (TmBtn(lab, true, lv > 0)) TmAction(() =>
                {
                    var s = skills[Mathf.Clamp(_tmTarget, 0, skills.Count - 1)]; string w = s.CanSocket(sd);
                    if (w != null) { TmSay(w); return; }
                    bool had = s.Has(sd.id); s.AddSupport(sd.id); TmSay(s.def.name + ": " + sd.name + (had ? " leveled to Lv" : " added, Lv") + s.Lvl(sd.id));
                });
            }
            TmRow();
            if (TmBtn("Clear all supports on target")) TmAction(() => { var s = skills[Mathf.Clamp(_tmTarget, 0, skills.Count - 1)]; s.sup.Clear(); TmSay("Supports cleared on " + s.def.name); });
            if (TmBtn("Target +1 skill level (more slots)", t.level < HData.MaxLv(t.def))) TmAction(() => { var s = skills[Mathf.Clamp(_tmTarget, 0, skills.Count - 1)]; if (s.level < HData.MaxSkillLevel) { s.level++; TmSay(s.def.name + " -> Lv " + s.level); } });
        }
        void TabEnemies()
        {
            TmLabel("SPAWN near the hero.  Count per click: " + _tmSpawnN);
            if (TmBtn("Count: x" + _tmSpawnN + "  (click to change 1/5/10)")) _tmSpawnN = _tmSpawnN == 1 ? 5 : _tmSpawnN == 5 ? 10 : 1;
            TmRow();
            if (TmBtn("Imp")) TmAction(() => TmSpawn(EType.Imp, 0));
            if (TmBtn("Ghoul")) TmAction(() => TmSpawn(EType.Ghoul, 0));
            if (TmBtn("Brute")) TmAction(() => TmSpawn(EType.Brute, 0));
            if (TmBtn("Splitter")) TmAction(() => TmSpawn(EType.Splitter, 0));
            if (TmBtn("Frost Imp (chills)")) TmAction(() => TmSpawn(EType.Imp, 1));
            if (TmBtn("Frost Brute (chills)")) TmAction(() => TmSpawn(EType.Brute, 1));
            if (TmBtn("Exploding Imp")) TmAction(() => TmSpawn(EType.Imp, 2));
            if (TmBtn("Exploding Ghoul")) TmAction(() => TmSpawn(EType.Ghoul, 2));
            if (TmBtn("Frost Caster (ice bolts, chill)")) TmAction(() => TmSpawn(EType.Imp, 3));
            if (TmBtn("Fire Wheel (rolls)")) TmAction(() => TmSpawn(EType.Brute, 4));
            if (TmBtn("Elite")) TmAction(() => TmSpawn(EType.Elite, 0));
            if (TmBtn("Hook Thrower")) TmAction(() => TmSpawn(EType.HookThrower, 0));
            if (TmBtn("Gossip Imp")) TmAction(() => TmSpawn(EType.GossipImp, 0));
            TmLabel("CLEAN UP");
            if (TmBtn("Kill all demons (not the boss)")) TmAction(TmKillAll);
            if (TmBtn("Clear room (open doors)")) TmAction(() => { TmClose(); FloorClearRoom(); });
            if (TmBtn("Kill the boss", BossAlive)) TmAction(() => { TmClose(); if (BossAlive) Damage(_boss, _boss.hp + 1, false, true); });
        }
        // ---- v0.0.53 pt480u: PACT test tab (Souls / Sigils / wipe the Pact meta save). Paths-only Pact is unchanged. ----
        bool MenuMode53 => mode == Mode.MainMenu || mode == Mode.Altar; const int TmPactTab53 = 6; bool _tmResetArm53;
        void TmSouls53(int add, int set = -1) { PactLoad51(); _souls51 = set >= 0 ? set : Mathf.Max(0, _souls51 + add); PactSave51(); TmSay("Souls now " + _souls51); }
        void TmSigils53(int add, int set = -1) { PactLoad51(); _sigils51 = set >= 0 ? set : Mathf.Max(0, _sigils51 + add); PactSave51(); TmSay("Sigils now " + _sigils51); }
        const int TmSinTab737 = 7;
        void TabSins737()
        {
            SinLoad737(); PactLoad51();
            TmBtn("Sins owned " + SinOwnedCount737 + "/" + HSins.Count + "   slots " + _sinSlots737 + "   Sigils " + _sigils51 + "   cheats " + (SinCheatsOn737 ? "ON" : "OFF"), false);
            if (!SinCheatsOn737) { if (TmBtn("Turn sin cheats ON (this session only; tunable sinDebugCheats737 is OFF)")) { _sinCheats737 = true; TmSay("Sin cheats ON"); } return; }
            if (TmBtn("Force drops: " + (_sinForce737 ? "ON (every kill rolls at sinDebugForceChance737)" : "OFF"))) _sinForce737 = !_sinForce737;
            if (TmBtn("+5 Sigils")) TmSigils53(5);
            if (TmBtn("+1000 souls")) TmSouls53(1000);
            if (TmBtn("Grant random Mortal sin")) { SinCheatRandom737(SinTier.Mortal); TmSay("Granted"); }
            if (TmBtn("Grant random Demonic sin (no effect yet)")) { SinCheatRandom737(SinTier.Demonic); TmSay("Granted"); }
            if (TmBtn("Grant random Heaven-Defying sin (no effect yet)")) { SinCheatRandom737(SinTier.HeavenDefying); TmSay("Granted"); }
            if (TmBtn("Force perfect roll: " + (_sinPerfect737 ? "ON" : "OFF"))) _sinPerfect737 = !_sinPerfect737;
            if (TmBtn("Grant duplicate of equipped sin (tests BETTER ROLL / worse -> Souls)")) TmSay(SinCheatDupEquipped737());
            if (TmBtn("Grant ALL " + HSins.Count + " sins")) { SinCheatGrantAll737(); TmSay("All sins owned"); }
            if (TmBtn("Unlock all 3 slots (free)")) { _sinSlots737 = SlotsMax737; SinSave737(); TmSay("Slots unlocked"); }
            if (TmBtn("RESET sins save (owned, slots, equipped)")) { SinCheatReset737(); TmSay("Sins reset"); }
        }
        void TabPact53()
        {
            var T = HellordTuning.I; PactLoad51();
            TmBtn("Souls: " + _souls51 + "   Sigils: " + _sigils51, false);
            if (TmBtn("+100 souls")) TmSouls53(100);
            if (TmBtn("+1000 souls")) TmSouls53(1000);
            if (TmBtn("Set souls = " + T.testSoulsSet53)) TmSouls53(0, T.testSoulsSet53);
            if (TmBtn("Souls = 0")) TmSouls53(0, 0);
            TmLabel("NECROMANCER (t536u TEST)");
            if (TmBtn("Unlock all 18 Necromancer nodes")) { PactLoad51(); _unlock51[(int)PactPath.Necromancer] = (1 << PactData.Nodes[(int)PactPath.Necromancer].Length) - 1; PactSave51(); TmSay("All Necromancer nodes unlocked (take them in the Altar)"); }
            if (TmBtn("TEST skeletons: give Skeleton Warriors + Skeleton Archers (Lv1)")) { foreach (var d in HData.Skills) if (HData.Summon561(d) && skills.Find(x => x.def.id == d.id) == null) skills.Add(new SkillInst { def = d }); TmSay("Summons given: " + SummonStatus561()); } // t561u
            if (TmBtn("TEST skeletons: +1 level both summon skills")) { foreach (var x in skills) if (HData.Summon561(x.def) && x.level < HData.MaxLv(x.def)) x.level++; TmSay(SummonStatus561()); }
            if (TmBtn("Kill my minions (test respawn)")) { foreach (var m in _nmin536) m.hp = 0; TmSay("Minions killed: watch them respawn"); }
            if (TmBtn("+1 sigil")) TmSigils53(1);
            if (TmBtn("+10 sigils")) TmSigils53(10);
            if (TmBtn("Set sigils = " + T.testSigilsSet53)) TmSigils53(0, T.testSigilsSet53);
            if (TmBtn("Sigils = 0")) TmSigils53(0, 0);
            if (TmBtn(_tmResetArm53 ? "CLICK AGAIN TO WIPE PACT SAVE" : "RESET PACT SAVE...", true, _tmResetArm53))
            {
                if (!_tmResetArm53) { _tmResetArm53 = true; TmSay("Wipes Souls, Sigils, unlocks and the selected Pact. Click again to confirm."); }
                else
                {
                    _tmResetArm53 = false;
                    foreach (var k in new[] { "souls", "sigils", "path", "sel", "unlock0", "unlock1", "unlock2", "unlock3", "unlock4", "mig539" }) PlayerPrefs.DeleteKey(PK + k);
                    PlayerPrefs.Save(); _pactLoaded51 = false; PactLoad51(); _pactActive51 = false;
                    TmSay("Pact save wiped (souls back to " + _souls51 + "). Current run's Pact is off; new runs start clean.");
                }
            }
            if (_tmResetArm53 && TmBtn("Cancel reset")) { _tmResetArm53 = false; TmSay("Reset cancelled"); }
        }

        void TabBoss()
        {
            TmLabel("SPAWN A BOSS (jumps to that floor if needed, then to its boss room)");
            if (TmBtn("Ox-Head (Ox Pit)")) TmAction(() => { _bossKind44 = -1; TmSpawnBoss(0); });
            if (TmBtn("Ice Gorilla (Frozen Hell)")) TmAction(() => { _bossKind44 = -1; TmSpawnBoss(1); });
            if (TmBtn("Qilin Chariot (Burning Hell)")) TmAction(() => { _bossKind44 = -1; TmSpawnBoss(2); });
            TmLabel("UNUSED BOSSES (kept for a future floor; spawn in this floor's boss room)");
            if (TmBtn("Horse-Face")) TmAction(() => { TmDespawnBoss(); _bossKind44 = 2; TmJumpBoss(); });
            if (TmBtn("Frost Ox-Head (old)")) TmAction(() => { TmDespawnBoss(); _bossKind44 = 1; TmJumpBoss(); });
            TmLabel("BOSS TOOLS");
            if (TmBtn("Kill the boss", BossAlive)) TmAction(() => { TmClose(); if (BossAlive) Damage(_boss, _boss.hp + 1, false, true); });
            if (TmBtn("Boss reward pick (current floor)")) TmAction(() => { OpenBossReward37(); TmClose(); });
            if (TmBtn("Boss room (this floor)")) TmAction(TmJumpBoss);
            TmRow(); GUI.color = new Color(0.8f, 0.8f, 0.9f);
            GUI.Label(new Rect(_gx, _gy + 4, 800, 60), "Boss = " + (BossAlive ? BossName37() + " ALIVE" : "none alive") + "   |   current floor " + (FloorIdx + 1) + ": " + BossName37(), _tmTxt); GUI.color = Color.white;
        }
        void TabKeys(Rect P)
        {
            string s =
                "OPEN / CLOSE:  F1 or the TEST button (top left).  Esc also closes.  The game is paused while the menu is open.\n" +
                "MENU KEYS:  arrows move, Enter / Space press, Tab or PageUp / PageDown change tab.  Mouse: click anything.\n\n" +
                "OLD DEBUG KEYS (in play, menu closed):  F2 elite  |  F3 transformation  |  F4 boss room  |  F5 hit-stop  |  F6 stun wobble  |  F7 impact frames\n" +
                "F8 clear room  |  F9 debug text  |  F10 secret rooms  |  J hook thrower  |  I gossip imp  |  T tongue traps  |  K kill counter  |  G glow  |  Tab aim\n" +
                "(F1 used to give a level: use PLAYER > +1 level now.  Shift+1..9 = F1..F9 on keyboards without F-keys.)\n\n" +
                "Tuning: HellordTuning.testMenuEnabled turns this whole menu off for a release build.\n" +
                "Known limits: x2 speed also speeds the boss tells; god mode only blocks damage (no knock-back change); floor jumps build a fresh random layout.";
            TmRow(); GUI.Label(new Rect(_gx, _gy, P.width - 28, 330), s, _tmTxt);
        }

        // ---------- actions ----------
        /// <summary>Run a menu action. Leaving the starting-skill pick (the first run screen) first, so the world is playable.</summary>
        void TmAction(Action a, bool autoFireball = true)
        {
            if (mode == Mode.StartPick) TmLeaveStartPick(autoFireball);
            a();
        }
        void TmLeaveStartPick(bool autoFireball)
        {
            // Giving a specific skill from the menu replaces the starting pick; any other action grants Fireball so the hero can fight.
            cards.Clear(); mode = Mode.Play;
            if (autoFireball && skills.Count == 0) { skills.Add(new SkillInst { def = HData.Skills[0] }); SetHeroLayers(); }
        }
        void TmGiveSkill(SkillDef d)
        {
            if (skills.Exists(x => x.def.id == d.id)) { TmSay(d.name + " already owned (use Level up)"); return; }
            if (skills.Count >= HellordTuning.I.skillSlotsMax818) { TmSay(HellordTuning.I.skillSlotsMax818 + " skill slots full - remove one first"); return; }
            if (skills.Count >= HData.SlotsUnlocked(level)) HData.BonusSlots++;
            skills.Add(new SkillInst { def = d }); SetHeroLayers(); TmSay("Got " + d.name);
        }

        void TmDespawnBoss() { if (BossAlive) { ResetBoss(); _boss.alive = false; } ClearBBullets(); }
        void TmJumpFloor(int f)
        {
            TmClose(); TmDespawnBoss(); _jumpFloor = Mathf.Clamp(f, 0, FloorCount - 1); StartTransition(-2, 0); _tmMsg = "Jumping to floor " + (f + 1);
        }
        void TmJumpBoss()
        {
            int b = BossIndex(); if (b < 0) { TmSay("No boss room on this floor"); return; }
            TmClose(); TmDespawnBoss(); TmResetRoom(floor[b]); StartTransition(b, -1);
        }
        void TmResetRoom(FloorRoom r)
        {
            r.cleared = false; r.active = false; r.locked = false; r.goalMet = false; r.goalMetT = 0; r.accepted = false; r.chestPending = false; r.rewardTaken = false; r.goalHave = 0;
        }
        void TmJumpRoom(RoomType t)
        {
            int idx = -1; for (int i = 0; i < floor.Count; i++) if (floor[i].type == t) { idx = i; break; }
            if (idx < 0)
            {   // not on this floor's layout: convert a plain fight room (never the start, boss, secret or the room we stand in if possible)
                for (int i = 0; i < floor.Count && idx < 0; i++) { var r = floor[i]; if ((r.type == RoomType.Survive || r.type == RoomType.Hunt || r.type == RoomType.Nests) && i != curRoom) idx = i; }
                if (idx < 0) { TmSay("This floor has no " + t + " room"); return; }
                var rr = floor[idx]; rr.type = t; rr.art = PickArt(t, out rr.sizeClass);
                rr.cleared = !rr.IsFight; rr.exitsSet = rr.exitsSet && t != RoomType.Secret; // v0.0.42
                if (t == RoomType.Shrine) { RollShop(rr); rr.cleared = true; }
                if (t == RoomType.Secret) { rr.chestOpen = new bool[8]; rr.cleared = true; } // v0.0.42: was missing (secret chests need it)
            }
            var room = floor[idx];
            if (room.type != RoomType.Shrine && room.type != RoomType.Start && room.type != RoomType.Secret) TmResetRoom(room);
            TmClose(); TmDespawnBoss(); StartTransition(idx, -1); _tmMsg = "Jumping to " + t + " room";
        }
        void TmSpawnBoss(int f)
        {
            f = DepthForTheme42(f); // v0.0.42: the BOSS tab buttons are per theme; find that theme's depth in this run
            if (FloorIdx == f) { TmJumpBoss(); return; }
            TmJumpFloor(f); _tmPending = TmJumpBoss;
        }
        void TmSpawn(EType t, int variant)
        {
            int n = _tmSpawnN, made = 0; var T = HellordTuning.I;
            for (int i = 0; i < n; i++)
            {
                Enemy e;
                if (t == EType.HookThrower) e = SpawnHookThrowerNearHero();
                else if (t == EType.GossipImp) e = SpawnGossipImpNearHero();
                else
                {
                    var p = _heroPos + UnityEngine.Random.insideUnitCircle.normalized * (T.testSpawnDistU + UnityEngine.Random.value * 1.5f); ClampToRoom(ref p, 10);
                    e = SpawnEnemyAt(t, p);
                    if (e != null && variant == 1) { e.variant = 1; e.maxHp *= T37.frostHpMul; e.hp = e.maxHp; e.speed *= T37.frostSpeedMul; e.frames = VariantFrames37(e.type, 1); e.sr.sprite = e.frames[0]; }
                    else if (e != null && variant == 2) { e.variant = 2; e.maxHp *= T37.bomberHpMul; e.hp = e.maxHp; e.speed *= T37.bomberSpeedMul; e.fuseT = -1; e.frames = VariantFrames37(e.type, 2); e.sr.sprite = e.frames[0]; }
                    else if (e != null && variant >= 3) MakeVariant44(e, variant);
                }
                if (e != null) made++;
            }
            TmSay(made == n ? "Spawned " + made + " x " + t + (variant == 1 ? " (frost)" : variant == 2 ? " (exploding)" : "") : "Spawned " + made + "/" + n + " (no free enemy slots)");
        }
        void TmKillAll()
        {
            int c = 0; foreach (var e in _en) if (e.alive && !e.boss) { e.splitDone = true; KillEnemy(e); c++; }
            TmSay("Killed " + c + " demons");
        }
    }
}
