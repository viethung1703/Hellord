using UnityEngine;

namespace Hellord
{
    // t716u: player control (Hellord_player_control_design_v0.1.md).
    // Manual mode: 8 skill slots cast on their keys, aim = mouse cursor, no basic attack, no mana.
    // Movement: Mouse (PoE2 click-to-move) or WASD. Rebindable keys on the Controls page (O, or the Pause key), saved in PlayerPrefs.
    // Auto mode = the old behaviour (every skill auto-fires, 3-slot bar), untouched.
    public partial class HellordGame
    {
        public enum Act716 { S1, S2, S3, S4, S5, S6, S7, S8, Dash, Interact, Inventory, Map, Pause }
        const int NAct716 = 13, NSlot716 = 8; // t818u: arrays keep 8; the belt shows BeltN716 (= skillSlotsMax818, 5)
        static int BeltN716 => Mathf.Clamp(HellordTuning.I.skillSlotsMax818, 1, NSlot716);
        static readonly string[] ActName716 = { "Skill 1", "Skill 2", "Skill 3", "Skill 4", "Skill 5", "Skill 6", "Skill 7", "Skill 8", "Dash", "Interact", "Inventory", "Map", "Pause" };
        static readonly string[] DefMouse716 = { "LMB", "RMB", "Q", "W", "E", "R", "T", "F", "Space", "G", "I", "Tab", "Escape" };
        static readonly string[] DefWasd716 = { "LMB", "RMB", "Q", "E", "R", "F", "Digit1", "Digit2", "Space", "G", "I", "Tab", "Escape" };
        public static bool Manual716, MouseMove716; static bool _ctrlLoaded716;
        static readonly string[] _bMouse716 = new string[NAct716], _bWasd716 = new string[NAct716];
        static string[] Binds716 => MouseMove716 ? _bMouse716 : _bWasd716;
        static string Bind716(Act716 a) => Binds716[(int)a];

        static void LoadCtrl716()
        {
            if (_ctrlLoaded716) return; _ctrlLoaded716 = true; var T = HellordTuning.I;
            Manual716 = T.manualControl720 && PlayerPrefs.GetInt("hl716_manual", T.manualDefault716 ? 1 : 0) == 1;
            MouseMove716 = PlayerPrefs.GetInt("hl716_mouse", T.mouseMoveDefault716 ? 1 : 0) == 1;
            for (int i = 0; i < NAct716; i++) { _bMouse716[i] = PlayerPrefs.GetString("hl716_bm" + i, DefMouse716[i]); _bWasd716[i] = PlayerPrefs.GetString("hl716_bw" + i, DefWasd716[i]); }
        }
        static void SaveCtrl716()
        {
            PlayerPrefs.SetInt("hl716_manual", Manual716 ? 1 : 0); PlayerPrefs.SetInt("hl716_mouse", MouseMove716 ? 1 : 0);
            for (int i = 0; i < NAct716; i++) { PlayerPrefs.SetString("hl716_bm" + i, _bMouse716[i]); PlayerPrefs.SetString("hl716_bw" + i, _bWasd716[i]); }
            PlayerPrefs.Save();
        }
        /// <summary>Debug letter keys (G glow, T traps, ...) only fire when no Manual binding uses that key.</summary>
        static bool Free716(string key) { if (!Manual716) return true; foreach (var b in Binds716) if (b == key) return false; return true; }

        // ================= slots =================
        readonly SkillInst[] _slot716 = new SkillInst[NSlot716];
        SkillInst _slotPick716;
        readonly float[] _pressT716 = new float[NSlot716];
        /// <summary>t720u: cursor aim for skills (Manual, or Auto with autoMouseAim720).</summary>
        bool CurAim720 => Manual716 || HellordTuning.I.autoMouseAim720;
        int SlotOf716(SkillInst s) { for (int i = 0; i < NSlot716; i++) if (_slot716[i] == s) return i; return -1; }
        void SyncSlots716()
        {
            for (int i = 0; i < NSlot716; i++) if (_slot716[i] != null && !skills.Contains(_slot716[i])) _slot716[i] = null;
            foreach (var s in skills)
            {
                if (s == _slotPick716 || SlotOf716(s) >= 0) continue;
                for (int i = 0; i < BeltN716; i++) if (_slot716[i] == null) { _slot716[i] = s; break; }
            }
        }
        /// <summary>Called when a new-skill card is picked. Manual + at least one filled slot = the player picks the slot.</summary>
        void OfferSlot716(SkillInst s)
        {
            if (!Manual716) return; SyncSlots716();
            bool any = false; foreach (var x in _slot716) if (x != null && x != s) any = true;
            if (!any) { int k = SlotOf716(s); if (k < 0) _slot716[0] = s; return; }
            int at = SlotOf716(s); if (at >= 0) _slot716[at] = null; _slotPick716 = s;
            // t718u: highlight the first EMPTY slot, and ignore the click/key that picked the card
            _pickSel716 = 0; for (int i = 0; i < BeltN716; i++) if (_slot716[i] == null) { _pickSel716 = i; break; }
            _pickT716 = Time.unscaledTime; _pickArm716 = false; _pickConfirm716 = -1;
        }
        void PutSlot716(int i)
        {
            var s = _slotPick716; if (s == null) return; var old = _slot716[i];
            if (old != null && old != s) { skills.Remove(old); SetHeroLayers(); Toast($"{s.def.name} replaces {old.def.name}"); }
            _slot716[i] = s; _slotPick716 = null; _pickConfirm716 = -1; _pickCool716 = 0.2f;
        }
        float _pickCool716, _pickT716; int _pickSel716, _pickConfirm716 = -1; bool _pickArm716; // t718u

        /// <summary>Does the player want this skill cast this frame? (Manual only)</summary>
        bool Want716(SkillInst s)
        {
            if (_pickCool716 > 0) return false;
            int i = SlotOf716(s); if (i < 0) return false; string b = Bind716((Act716)i); bool overUi = MouseOverBelt716();
            if (b == "LMB" && MouseMove716) return _lmbCast716;
            if ((b == "LMB" || b == "RMB" || b == "MMB") && overUi) return false;
            bool chan = s.def.id == SkillId.BloodWhip && HellordTuning.I.whipChannel716;
            if (HInput.BindDown(b)) _pressT716[i] = Time.time;
            if (chan || HellordTuning.I.holdRepeat716) { if (HInput.BindHeld(b)) return true; }
            if (Time.time - _pressT716[i] < 0.25f) { _pressT716[i] = -9; return true; } // short press buffer when the cooldown was nearly ready
            return false;
        }

        // ================= aim helpers =================
        Vector2 GroundAim716()
        {
            Vector2 at = MouseWorld(); float R = HellordTuning.I.groundAoeRange716;
            if ((at - _heroPos).magnitude > R) at = _heroPos + (at - _heroPos).normalized * R;
            ClampToRoom(ref at, 4); return at;
        }
        int ChainFirst716(int first)
        {
            if (!CurAim720) return first; var T = HellordTuning.I;
            int c = Nearest(MouseWorld(), T.chainCursorPick716, -1);
            return c >= 0 && (_en[c].pos - _heroPos).magnitude <= T.chainFirstRangeU + T.chainCursorPick716 ? c : first;
        }
        bool CastPillar716(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; int n = HData.At(T.pillarCount, s.level, 1); float r = HData.At(T.pillarRadiusU, s.level, 1.3f) * s.AreaMul;
            Vector2 c = GroundAim716(); float a0 = Random.value * 360f;
            for (int p = 0; p < n; p++)
            {
                Vector2 at = p == 0 ? c : c + (Vector2)(Quaternion.Euler(0, 0, a0 + p * 360f / Mathf.Max(1, n - 1)) * Vector2.right) * T.pillarScatter716;
                ClampToRoom(ref at, 4); var sk = s; float dd = dmg * T.pillarDmgMul; After(p * T.pillarStaggerSeconds, () => PillarTell(at, r, dd, sk));
            }
            return true;
        }

        // ================= movement + dash =================
        Vector2 _moveTgt716, _prevHero716; bool _moving716, _lmbAttack716, _lmbCast716; float _stuck716;
        Vector2 Move716(float dt)
        {
            _lmbCast716 = false;
            if (!MouseMove716) return HInput.Move();
            var T = HellordTuning.I; Vector2 kb = HInput.PadMove716() + HInput.Arrows716();
            bool lmb = HInput.LmbHeld716() && _pickCool716 <= 0, hasLmbSkill = false;
            for (int i = 0; i < NSlot716; i++) if (_slot716[i] != null && Bind716((Act716)i) == "LMB") hasLmbSkill = true;
            if (HInput.LmbDown716()) _lmbAttack716 = hasLmbSkill && !MouseOverBelt716() && Nearest(MouseWorld(), T.enemyPickR716, -1) >= 0;
            if (!lmb) _lmbAttack716 = false;
            if (lmb && !MouseOverBelt716())
            {
                if (HInput.Shift()) { _lmbCast716 = hasLmbSkill; _moving716 = false; }              // Shift+LMB: cast in place
                else if (_lmbAttack716) { _lmbCast716 = true; _moving716 = false; }               // LMB on an enemy: use the LMB skill
                else { _moveTgt716 = MouseWorld(); if (!_moving716) _stuck716 = 0; _moving716 = true; } // LMB on the ground: walk, hold = follow the cursor
            }
            if (kb.sqrMagnitude > 0.01f) { _moving716 = false; return Vector2.ClampMagnitude(kb, 1); }
            if (!_moving716) return Vector2.zero;
            // stop when stuck on a wall for a moment
            if ((_heroPos - _prevHero716).sqrMagnitude < 1e-6f) _stuck716 += dt; else _stuck716 = 0; _prevHero716 = _heroPos;
            Vector2 d = _moveTgt716 - _heroPos;
            if (d.magnitude < T.moveStopDist716 || (_stuck716 > 0.25f && !lmb)) { _moving716 = false; return Vector2.zero; }
            return d.magnitude < 0.6f ? d / 0.6f : d.normalized; // ease into the click point
        }
        bool DashPressed716() => Manual716 ? (HInput.BindDown(Bind716(Act716.Dash)) || HInput.PadDash716()) && !MouseOverBelt716() : HInput.DashDown();
        Vector2 DashDir716(Vector2 mv)
        {
            if (Manual716 && MouseMove716) { var d = MouseWorld() - _heroPos; if (d.sqrMagnitude > 0.01f) return d.normalized; }
            return mv.sqrMagnitude > 0.01f ? mv.normalized : _lastDir;
        }

        // ================= Update hook =================
        bool _ctrlOpen716; float _ctrlPrevTs716 = 1; int _sel716, _wait716 = -1, _waitFrame716, _confA716 = -1, _confB716 = -1; string _confKey716;
        bool CtrlBlock716()
        {
            LoadCtrl716(); _pickCool716 = Mathf.Max(0, _pickCool716 - Time.unscaledDeltaTime);
            SyncSlots716(); // t720u: belt slots fill in Auto too (next empty slot)
            if (_ctrlOpen716) { CtrlPageTick716(); return true; }
            if (_slotPick716 != null)
            {
                if (!Manual716 || !skills.Contains(_slotPick716)) { _slotPick716 = null; return false; }
                int hov = BeltHover716();
                // t718u: wait for the input delay AND for every slot binding to be released before accepting a choice
                if (!_pickArm716)
                {
                    bool held = HInput.LmbHeld716(); for (int i = 0; i < NSlot716; i++) if (HInput.BindHeld(Bind716((Act716)i))) held = true;
                    if (HInput.Enter716() || HInput.Esc716()) held = true;
                    if (Time.unscaledTime - _pickT716 >= HellordTuning.I.slotPickDelay718 && !held) _pickArm716 = true;
                    return true;
                }
                int want = -1;
                if (hov >= 0 && HInput.LmbDown716()) want = hov;
                else for (int i = 0; i < BeltN716; i++) { var b = Bind716((Act716)i); if (b == "LMB" && hov >= 0) continue; if (HInput.BindDown(b)) { want = i; break; } }
                if (want < 0 && HInput.Enter716()) want = _pickSel716;
                if (want < 0 && HInput.Esc716()) { for (int i = 0; i < BeltN716; i++) if (_slot716[i] == null) { PutSlot716(i); return true; } want = _pickSel716; }
                if (want >= 0)
                {
                    _pickSel716 = want;
                    if (_slot716[want] == null || _pickConfirm716 == want || !HellordTuning.I.slotReplaceConfirm718) PutSlot716(want); // empty slot, or second press on the same full slot
                    else { _pickConfirm716 = want; Toast($"PRESS AGAIN TO REPLACE {_slot716[want].def.name.ToUpperInvariant()}"); }
                }
                return true;
            }
            bool open = HellordTuning.I.manualControl720 && HInput.BindDown("O") || (Manual716 && Bind716(Act716.Pause) != "O" && HInput.BindDown(Bind716(Act716.Pause)) && _tp == null);
            if (open && !_tmOpen) { _ctrlOpen716 = true; _ctrlPrevTs716 = Time.timeScale > 0 ? Time.timeScale : 1; Time.timeScale = 0; _wait716 = -1; _confA716 = -1; _waitFrame716 = Time.frameCount; return true; }
            return false;
        }
        void CloseCtrl716() { _ctrlOpen716 = false; _wait716 = -1; _confA716 = -1; Time.timeScale = _ctrlPrevTs716; SaveCtrl716(); _pickCool716 = 0.25f; }

        // ---- HUD-space mouse (same integer zoom as HudBegin)
        static Vector2 MouseHud716()
        {
            float z = Mathf.Min(Mathf.Floor(Screen.width / (float)HW), Mathf.Floor(Screen.height / (float)HH)); if (z < 1) z = Mathf.Min(Screen.width / (float)HW, Screen.height / (float)HH);
            float ox = Mathf.Round((Screen.width - HW * z) / 2), oy = Mathf.Round((Screen.height - HH * z) / 2); var m = HInput.MouseScreen();
            return new Vector2((m.x - ox) / z, (Screen.height - m.y - oy) / z);
        }
        static Rect Cell716(int i)
        {
            var T = HellordTuning.I; float y0 = HH - T.beltBottom716 - T.beltCellH716; // t818u: one row of up to 5 (was 2x4)
            return new Rect(Mathf.Round(T.beltX716 + i * (T.beltCellW716 + T.beltGap716)), Mathf.Round(y0), T.beltCellW716, T.beltCellH716);
        }
        int BeltHover716() { var m = MouseHud716(); for (int i = 0; i < BeltN716; i++) if (Cell716(i).Contains(m)) return i; return -1; }
        bool MouseOverBelt716() => BeltHover716() >= 0;

        // ---- Controls page layout (640x360)
        static Rect Row716(int i) => new Rect(18, 62 + i * 21, 276, 18);
        static readonly Rect RWasd716 = new Rect(322, 72, 300, 16), RMouse716 = new Rect(322, 92, 300, 16), RAuto716 = new Rect(322, 132, 300, 16), RManual716 = new Rect(322, 152, 300, 16);
        static readonly Rect RSwap716 = new Rect(330, 268, 136, 14), RCancel716 = new Rect(478, 268, 136, 14), RReset716 = new Rect(322, 308, 300, 15), RBack716 = new Rect(322, 328, 300, 15);
        void CtrlPageTick716()
        {
            var B = Binds716; var m = MouseHud716(); bool click = HInput.LmbDown716() && Time.frameCount != _waitFrame716;
            if (_confA716 >= 0)
            {
                if (HInput.Enter716() || (click && RSwap716.Contains(m))) { B[_confB716] = B[_confA716]; B[_confA716] = _confKey716; _confA716 = -1; SaveCtrl716(); }
                else if (HInput.Esc716() || (click && RCancel716.Contains(m))) _confA716 = -1;
                return;
            }
            if (_wait716 >= 0)
            {
                if (Time.frameCount == _waitFrame716) return;
                if (HInput.Esc716()) { _wait716 = -1; return; }
                var k = HInput.AnyPressedBind(); if (k == null || k == "Escape" && _wait716 != (int)Act716.Pause) return;
                int other = -1; for (int i = 0; i < NAct716; i++) if (i != _wait716 && B[i] == k) other = i;
                if (other >= 0) { _confA716 = _wait716; _confB716 = other; _confKey716 = k; }
                else { B[_wait716] = k; SaveCtrl716(); }
                _wait716 = -1; _waitFrame716 = Time.frameCount; return;
            }
            if (HInput.Esc716() || HInput.BindDown("O")) { CloseCtrl716(); return; }
            if (HInput.Up716()) _sel716 = (_sel716 + NAct716 - 1) % NAct716; if (HInput.Dn716()) _sel716 = (_sel716 + 1) % NAct716;
            for (int i = 0; i < NAct716; i++) if (Row716(i).Contains(m)) { _sel716 = i; if (click) { _wait716 = i; _waitFrame716 = Time.frameCount; return; } }
            if (HInput.Enter716()) { _wait716 = _sel716; _waitFrame716 = Time.frameCount; return; }
            if (!click) return;
            if (RWasd716.Contains(m)) { MouseMove716 = false; _moving716 = false; SaveCtrl716(); }
            else if (RMouse716.Contains(m)) { MouseMove716 = true; SaveCtrl716(); }
            else if (RAuto716.Contains(m)) { Manual716 = false; _slotPick716 = null; SaveCtrl716(); }
            else if (RManual716.Contains(m)) { Manual716 = true; SyncSlots716(); SaveCtrl716(); }
            else if (RReset716.Contains(m)) { var D = MouseMove716 ? DefMouse716 : DefWasd716; for (int i = 0; i < NAct716; i++) B[i] = D[i]; SaveCtrl716(); Toast("Controls reset to default"); }
            else if (RBack716.Contains(m)) CloseCtrl716();
        }

        // ================= drawing =================
        static Color C716(int hex, float a = 1) => new Color(((hex >> 16) & 255) / 255f, ((hex >> 8) & 255) / 255f, (hex & 255) / 255f, a);
        static void Fill716(float x, float y, float w, float h, Color c) { GUI.color = c; GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture); }
        static void Frame716(float x, float y, float w, float h, Color c) { Fill716(x, y, w, 1, c); Fill716(x, y + h - 1, w, 1, c); Fill716(x, y, 1, h, c); Fill716(x + w - 1, y, 1, h, c); }
        static string Label716(string b)
        {
            if (string.IsNullOrEmpty(b)) return "-";
            if (b.StartsWith("Digit")) return b.Substring(5); if (b.StartsWith("Numpad")) return "N" + b.Substring(6);
            switch (b) { case "Space": return "SPACE"; case "Escape": return "ESC"; case "Tab": return "TAB"; case "Enter": return "ENTER"; case "Backspace": return "BKSP"; case "LeftCtrl": case "RightCtrl": return "CTRL"; case "LeftAlt": case "RightAlt": return "ALT"; case "Minus": return "-"; case "Equals": return "="; case "Slash": return "/"; case "Period": return "."; case "Quote": return "'"; case "UpArrow": return "UP"; case "DownArrow": return "DN"; case "LeftArrow": return "LT"; case "RightArrow": return "RT"; }
            return b.Length > 5 ? b.Substring(0, 5).ToUpperInvariant() : b.ToUpperInvariant();
        }
        /// <summary>39j V1 keycap: cream #ead4aa, 1px black outline, 2px #c28569 bottom lip, dark letter. Mouse buttons = a small mouse with the pressed side red.</summary>
        float Keycap716(string b, float x, float y)
        {
            bool mouse = b == "LMB" || b == "RMB" || b == "MMB"; string t = mouse ? "" : Label716(b);
            float w = mouse || t.Length <= 1 ? 12 : TextW(t) + 6, h = 12;
            Fill716(x - 1, y - 1, w + 2, h + 2, Color.black); Fill716(x, y, w, h, C716(0xead4aa)); Fill716(x, y + h - 2, w, 2, C716(0xc28569));
            if (mouse)
            {   // 6x8 mouse: dark body, pressed button red
                float mx = x + 3, my = y + 1; Fill716(mx, my, 6, 8, C716(0x181425)); Fill716(mx + 1, my + 4, 4, 3, C716(0x3a4466));
                if (b == "LMB") Fill716(mx + 1, my + 1, 2, 3, C716(0xe43b44)); else if (b == "RMB") Fill716(mx + 3, my + 1, 2, 3, C716(0xe43b44)); else Fill716(mx + 2, my + 1, 2, 2, C716(0xe43b44));
            }
            else { float pa = _guiA; Text(t, Mathf.Round(x + (w - TextW(t)) / 2f), y + 2, C716(0x181425), false); _guiA = pa; }
            GUI.color = Color.white; return w;
        }

        void DrawBelt716()
        {
            if (!HellordTuning.I.skillBarEnabled) return;
            FloorTex(); SkillBarTex(); var T = HellordTuning.I; var g = GUI.color; float gm = T.beltGem716;
            Vector2 mp = Event.current.mousePosition; int hov = -1;
            bool pick = _slotPick716 != null; float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
            for (int i = 0; i < BeltN716; i++)
            {
                var r = Cell716(i); float x = r.x, y = r.y; if (r.Contains(mp)) hov = i; var k = _slot716[i];
                Fill716(x - 1, y - 1, r.width + 2, r.height + 2, Color.black); Fill716(x, y, r.width, r.height, C716(0x3a4466)); Fill716(x + 1, y + 1, r.width - 2, r.height - 2, C716(0x181425));
                if (pick && i == _pickConfirm716) Frame716(x - 2, y - 2, r.width + 4, r.height + 4, C716(0xa22633, 0.5f + 0.5f * pulse));
                else if (pick && i == _pickSel716) Frame716(x - 2, y - 2, r.width + 4, r.height + 4, C716(0xfeae34, 0.4f + 0.6f * pulse));
                else if (pick) Frame716(x - 1, y - 1, r.width + 2, r.height + 2, C716(0x8b9bb4, 0.5f));
                else if (hov == i) Frame716(x, y, r.width, r.height, C716(0xead4aa));
                float ix = x + 6, iy = y + Mathf.Round((r.height - 24) / 2f) + 2;
                Frame716(ix - 1, iy - 1, 26, 26, C716(0x3a4466, 0.9f));
                if (k != null)
                {
                    int ic = _icons44 ? (int)k.def.id : BarTile(k.def.id);
                    if (_tSkill24 != null) { var pt = _icons44 ? Color.white : PhTint(k.def.id); GUI.color = pt; Blit(_tSkill24, ic * 28, 0, 24, 24, ix, iy); }
                    float full = Mathf.Max(0.05f, k.def.cooldown * k.CdMul); bool cont = k.def.id == SkillId.BoneOrbit || HData.Summon561(k.def);
                    float cdf = cont ? 0 : Mathf.Clamp01(k.cd / full);
                    if (cdf > 0.02f && k.cd > 0.15f && _tCdMask != null) { int nf = _tCdMask.Length - 1; int fr = Mathf.Clamp(Mathf.FloorToInt((1 - cdf) * nf), 0, nf); GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]); if (full >= 1.5f) CdNumber(Mathf.CeilToInt(k.cd), ix, iy, true); }
                    if (cont) { GUI.color = Color.white; Text("AUTO", ix, iy + 25, C716(0x8b9bb4)); }
                    float gx0 = ix + 24 + 4, gy0 = iy + 1;
                    for (int p = 0; p < 4; p++)
                    {
                        float sx = gx0 + (p % 2) * (gm + 1), sy = gy0 + (p / 2) * (gm + 1); bool open = p < Mathf.Min(k.level, 4), fill = p < k.sup.Count;
                        if (fill && _tSup20 != null) { int si = SupIx(k.sup[p]); GUI.color = Color.white; Blit(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, sx, sy, gm, gm); }
                        else { Frame716(sx, sy, gm, gm, open ? C716(0x5a6988) : C716(0x262b44)); if (!open) Fill716(sx + 1, sy + 1, gm - 2, gm - 2, C716(0x262b44)); }
                    }
                    GUI.color = Color.white; Text("L" + k.level, gx0, gy0 + 2 * (gm + 1) + 1, k.level >= 5 ? C716(0xfee761) : C716(0x8b9bb4));
                }
                else { Fill716(ix + 11, iy + 7, 2, 10, C716(0x3a4466)); Fill716(ix + 7, iy + 11, 10, 2, C716(0x3a4466)); }
                if (HellordTuning.I.showKeyBadges720) Keycap716(Bind716((Act716)i), ix - 4, iy - 6);
            }
            DrawSinHud818(Cell716(0).x, Cell716(0).y - 1); // t818u sins frame (3 slots) above the belt, left-aligned
            if (pick)
            {
                string a = "PICK A SLOT FOR " + _slotPick716.def.name.ToUpperInvariant(), b2 = _pickConfirm716 >= 0 ? "PRESS THAT SLOT AGAIN TO REPLACE - OR PICK ANOTHER" : "PRESS A SLOT KEY OR CLICK IT - ENTER = HIGHLIGHTED EMPTY SLOT";
                float w = Mathf.Max(TextW(a), TextW(b2)) + 16, bx = Mathf.Round((HW - w) / 2), by = 120;
                Fill716(bx - 1, by - 1, w + 2, 34, Color.black); Fill716(bx, by, w, 32, C716(0x3e2731)); Frame716(bx, by, w, 32, C716(0xa22633));
                Text(a, bx + 8, by + 6, C716(0xfeae34)); Text(b2, bx + 8, by + 18, Color.white);
            }
            else if (hov >= 0 && _slot716[hov] != null && T.beltTooltip716 && !_ctrlOpen716) DrawTip716(_slot716[hov], Cell716(hov));
            GUI.color = g;
        }
        void DrawTip716(SkillInst k, Rect cell)
        {
            var lines = new System.Collections.Generic.List<string>(); string head = k.def.name.ToUpperInvariant() + "  LV " + k.level; float w = TextW(head);
            for (int p = 0; p < k.sup.Count; p++)
            {
                var sd = HData.Supports[(int)k.sup[p]]; string d = sd.desc ?? ""; if (d.Length > 44) d = d.Substring(0, 43) + ".";
                string l = (p + 1) + " " + sd.name.ToUpperInvariant() + " " + k.Lvl(k.sup[p]) + ": " + d.ToUpperInvariant(); lines.Add(l); w = Mathf.Max(w, TextW(l));
            }
            if (lines.Count == 0) lines.Add("NO SUPPORTS YET");
            w = Mathf.Max(w, TextW(lines[0])) + 24; float h = 18 + lines.Count * 12, x = Mathf.Min(cell.x, HW - w - 4), y = Cell716(0).y - h - 6;
            Fill716(x - 1, y - 1, w + 2, h + 2, Color.black); Fill716(x, y, w, h, C716(0x181425)); Frame716(x, y, w, h, C716(0x3a4466));
            Text(head, x + 6, y + 5, C716(0xfeae34));
            for (int i = 0; i < lines.Count; i++)
            {
                float ly = y + 17 + i * 12;
                if (i < k.sup.Count && _tSup20 != null) { int si = SupIx(k.sup[i]); GUI.color = Color.white; Blit(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, x + 6, ly - 1, 9, 9); }
                Text(lines[i], x + 18, ly, Color.white);
            }
            GUI.color = Color.white;
        }

        void DrawCtrl716()
        {
            if (!_ctrlOpen716) return; HudBegin(); var g = GUI.color; var B = Binds716; Vector2 mp = Event.current.mousePosition;
            Fill716(0, 0, HW, HH, C716(0x181425, 0.94f));
            for (int y = 0; y < HH; y += 3) Fill716(0, y, HW, 1, C716(0x000000, 0.18f)); // faint scanlines
            string ti = "CONTROLS", st = "PICK AN ACTION, THEN PRESS ITS NEW KEY";
            Text(ti, Mathf.Round((HW - TextW(ti) * 2) / 2f), 12, C716(0xfeae34), true, 2); Text(st, Mathf.Round((HW - TextW(st)) / 2f), 32, C716(0x8b9bb4));
            // left panel
            Fill716(9, 41, 294, 312, Color.black); Fill716(10, 42, 292, 310, C716(0x3e2731)); Frame716(10, 42, 292, 310, C716(0xa22633));
            Text("ACTION", 18, 49, C716(0xfeae34)); Text("KEY", 294 - TextW("KEY"), 49, C716(0xfeae34));
            for (int i = 0; i < NAct716; i++)
            {
                var r = Row716(i); bool wait = _wait716 == i, conf = _confA716 == i || _confB716 == i && _confA716 >= 0, hov = !wait && (i == _sel716 || r.Contains(mp));
                Fill716(r.x, r.y, r.width, r.height, C716(0x181425)); Frame716(r.x, r.y, r.width, r.height, wait ? C716(0xfeae34) : conf ? C716(0xe43b44) : hov ? C716(0xead4aa) : C716(0x3a4466));
                if (hov) Text(">", r.x + 4, r.y + 6, C716(0xfeae34));
                Text(ActName716[i], r.x + 12, r.y + 6, hov || wait ? Color.white : C716(0xc0cbdc));
                if (wait) { string pk = "PRESS A KEY..."; float pw = TextW(pk) + 8; Frame716(r.xMax - pw - 4, r.y + 2, pw, 14, C716(0xfeae34)); Text(pk, r.xMax - pw, r.y + 6, C716(0xfeae34)); }
                else
                {
                    string kb = B[i]; float kw = kb == "LMB" || kb == "RMB" || kb == "MMB" || Label716(kb).Length <= 1 ? 12 : TextW(Label716(kb)) + 6;
                    Keycap716(kb, r.xMax - kw - 5, r.y + 3); if (conf) Text("!", r.xMax - kw - 14, r.y + 6, C716(0xe43b44));
                }
            }
            // right panel
            Fill716(313, 41, 318, 312, Color.black); Fill716(314, 42, 316, 310, C716(0x3e2731)); Frame716(314, 42, 316, 310, C716(0xa22633));
            Text("MOVEMENT", 322, 58, C716(0xfeae34));
            Toggle716(RWasd716, "WASD", !MouseMove716, mp); Toggle716(RMouse716, "MOUSE CLICK-TO-MOVE", MouseMove716, mp);
            Text("CONTROL", 322, 118, C716(0xfeae34));
            Toggle716(RAuto716, "AUTO (SKILLS FIRE BY THEMSELVES)", !Manual716, mp); Toggle716(RManual716, "MANUAL (SKILLS ON KEYS)", Manual716, mp);
            string h1 = MouseMove716 ? "CLICK-TO-MOVE: W IS FREE FOR A SKILL." : "WASD MOVES. LMB = SKILL 1.", h2 = MouseMove716 ? "LMB ENEMY = LMB SKILL, SHIFT+LMB = CAST HERE" : "DASH GOES THE WAY YOU MOVE.";
            Text(h1, 322, 176, C716(0x8b9bb4)); Text(h2, 322, 188, C716(0x8b9bb4)); Text("SETS ARE SAVED PER MOVEMENT MODE.", 322, 200, C716(0x8b9bb4));
            if (_confA716 >= 0)
            {
                Fill716(322, 216, 300, 72, C716(0x181425)); Frame716(322, 216, 300, 72, C716(0xe43b44)); Text("KEY IN USE", 330, 222, C716(0xe43b44));
                float kw = Keycap716(_confKey716, 330, 236); Text("IS USED BY " + ActName716[_confB716].ToUpperInvariant() + ".", 336 + kw, 238, Color.white);
                Text("SWAP? " + ActName716[_confB716].ToUpperInvariant() + " GETS " + Label716(B[_confA716]) + ".", 330, 252, Color.white);
                Button716(RSwap716, "SWAP", true, mp); Button716(RCancel716, "CANCEL", false, mp);
            }
            else if (_wait716 >= 0) Text("WAITING FOR " + ActName716[_wait716].ToUpperInvariant() + ": ESC CANCELS.", 322, 222, C716(0x8b9bb4));
            else Text("O OR ESC: BACK TO THE GAME", 322, 222, C716(0x8b9bb4));
            Button716(RReset716, "RESET TO DEFAULT", false, mp); Button716(RBack716, "BACK", true, mp);
            GUI.color = g;
        }
        void Toggle716(Rect r, string label, bool on, Vector2 mp)
        {
            Fill716(r.x, r.y, r.width, r.height, C716(0x181425)); Frame716(r.x, r.y, r.width, r.height, on ? C716(0xfeae34) : r.Contains(mp) ? C716(0xead4aa) : C716(0x3a4466));
            Frame716(r.x + 5, r.y + 4, 8, 8, C716(0xc0cbdc)); if (on) Fill716(r.x + 7, r.y + 6, 4, 4, C716(0xfeae34));
            Text(label, r.x + 18, r.y + 5, Color.white);
        }
        void Button716(Rect r, string label, bool gold, Vector2 mp)
        {
            Fill716(r.x, r.y, r.width, r.height, gold ? C716(0x3e2731) : C716(0x181425)); Frame716(r.x, r.y, r.width, r.height, r.Contains(mp) ? C716(0xead4aa) : gold ? C716(0xfeae34) : C716(0x8b9bb4));
            Text(label, Mathf.Round(r.x + (r.width - TextW(label)) / 2f), r.y + 4, gold ? C716(0xfeae34) : C716(0xc0cbdc));
        }
    }
}
