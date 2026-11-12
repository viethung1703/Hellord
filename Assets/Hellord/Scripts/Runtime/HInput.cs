using UnityEngine;
#if ENABLE_INPUT_SYSTEM && HELLORD_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Works with the new Input System package (if installed + active) or the legacy Input Manager.
namespace Hellord
{
    public enum HKey { Tab, F1, F2, F3, F4, F5, F6, F7, F8, F9, K, R, Space, One, Two, Three, Four, Five, Escape, F10, SkillCycle, Glow, Traps, Atmos, HookThrower, GossipImp } // v0.0.21: G = CRT glow cycle, T = spawn tongue traps (typed g / t aliases)
    public static class HInput
    {
        /// <summary>v0.0.9 WebGL-safe debug aliases: Shift+1..9 = F1..F9, Shift+0 or M = F10 (browsers eat F5 reload, F10/F11/F12).</summary>
        static int DebugDigit(HKey k) => k >= HKey.F1 && k <= HKey.F9 ? (int)k : k == HKey.SkillCycle ? 0 : -1; // v0.0.14: Shift+0 = skill cycle (map stays on M / F10)
        /// <summary>v0.0.15: skill cycle = Shift+0 or N. F12 removed: in browsers it opens DevTools and steals keyboard focus from the canvas.</summary>
        public const bool SkillCycleUsesF12 = false;
        public static bool Shift()
        {
#if ENABLE_INPUT_SYSTEM && HELLORD_INPUT_SYSTEM
            var kb = Keyboard.current; return kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
#else
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
        }
#if ENABLE_INPUT_SYSTEM && HELLORD_INPUT_SYSTEM
        static Key Map(HKey k)
        {
            switch (k)
            {
                case HKey.Tab: return Key.Tab; case HKey.F1: return Key.F1; case HKey.F2: return Key.F2; case HKey.F3: return Key.F3; case HKey.F4: return Key.F4; case HKey.F5: return Key.F5; case HKey.F6: return Key.F6; case HKey.F7: return Key.F7; case HKey.F8: return Key.F8; case HKey.F9: return Key.F9; case HKey.K: return Key.K; case HKey.R: return Key.R;
                case HKey.Space: return Key.Space; case HKey.One: return Key.Digit1; case HKey.Two: return Key.Digit2; case HKey.Three: return Key.Digit3;
                case HKey.Four: return Key.Digit4; case HKey.Five: return Key.Digit5; case HKey.F10: return Key.F10; case HKey.SkillCycle: return Key.N; case HKey.Glow: return Key.G; case HKey.Traps: return Key.T; case HKey.Atmos: return Key.H; case HKey.HookThrower: return Key.J; case HKey.GossipImp: return Key.I; default: return Key.Escape;
            }
        }
        // v0.0.16: robust Shift+digit debug keys in browsers. Three sources, deduped per digit:
        //  (a) digit key pressed this frame with Shift held / pressed / released this frame (fast taps),
        //  (b) the typed character from Keyboard.onTextInput ('!'..'(' and ')' on US layouts).
        static readonly string ShiftChars = ")!@#$%^&*(";   // index = digit
        static readonly int[] _textFrame = new int[10], _keyFrame = new int[10];
        static bool _hooked; static int _pendingMask, _frameMask, _maskFrame = -1;
        public static int TextHits, KeyHits;   // harness counters
        static void Hook()
        {
            var kb = Keyboard.current; if (_hooked || kb == null) return; _hooked = true;
            for (int i = 0; i < 10; i++) { _textFrame[i] = -999; _keyFrame[i] = -999; }
            kb.onTextInput += OnText;
        }
        public static void OnText(char c) { int d = ShiftChars.IndexOf(c); if (d >= 0) _pendingMask |= 1 << d; if (c == 'g' || c == 'G') _letterPending |= 1; if (c == 't' || c == 'T') _letterPending |= 2; if (c == 'h' || c == 'H') _letterPending |= 4; if (c == 'j' || c == 'J') _letterPending |= 8; if (c == 'i' || c == 'I') _letterPending |= 16; }
        static int _letterPending, _letterMask, _letterFrame = -1, _letterKeyFrame = -999;
        /// <summary>v0.0.21: letter debug keys: key press, or the typed char (g / t) if the key event was eaten (browser / xdotool type). Deduped per frame.</summary>
        static bool LetterKey(bool pressed, int bit) { int f = Time.frameCount; if (_letterFrame != f) { _letterFrame = f; _letterMask = _letterPending; _letterPending = 0; } if (pressed) { _letterKeyFrame = f; return true; } return (_letterMask & bit) != 0 && f - _letterKeyFrame > 2; }
        static bool ShiftDigit(Keyboard kb, int d)
        {
            int f = Time.frameCount;
            if (_maskFrame != f) { _maskFrame = f; _frameMask = _pendingMask; _pendingMask = 0; }
            var dk = kb[d == 0 ? Key.Digit0 : Key.Digit1 + (d - 1)];
            bool sh = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed || kb.leftShiftKey.wasReleasedThisFrame || kb.rightShiftKey.wasReleasedThisFrame;
            if (sh && dk.wasPressedThisFrame) { if (f - _textFrame[d] > 15) { _keyFrame[d] = f; KeyHits++; return true; } return false; }
            if ((_frameMask & (1 << d)) != 0) { if (f - _keyFrame[d] > 15 && _keyFrame[d] != f) { _textFrame[d] = f; TextHits++; return true; } }
            return false;
        }
        /// <summary>t1352: second key for the TEST MENU (` / ~): the Unity Editor can swallow F1 (Help), so the menu also opens with the backquote key.</summary>
        public static bool BackQuote() { var kb = Keyboard.current; return kb != null && kb.backquoteKey.wasPressedThisFrame; }
        public static bool Down(HKey k)
        {
            var kb = Keyboard.current; if (kb == null) return false; Hook();
            if (k == HKey.Glow || k == HKey.Traps || k == HKey.Atmos || k == HKey.HookThrower || k == HKey.GossipImp) return LetterKey(kb[Map(k)].wasPressedThisFrame, k == HKey.Glow ? 1 : k == HKey.Traps ? 2 : k == HKey.Atmos ? 4 : k == HKey.HookThrower ? 8 : 16); // v0.0.25: I = Gossip Imp
            if (k >= HKey.One && k <= HKey.Five && Shift()) return false; if (kb[Map(k)].wasPressedThisFrame) return true;
            int d = DebugDigit(k); if (d < 0) return k == HKey.F10 && kb.mKey.wasPressedThisFrame;
            return ShiftDigit(kb, d);
        }
        public static Vector2 Move()
        {
            Vector2 v = Vector2.zero; var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1; if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1; if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1;
            }
            var gp = Gamepad.current; if (gp != null) v += gp.leftStick.ReadValue();
            return Vector2.ClampMagnitude(v, 1);
        }
        /// <summary>v0.0.49 dash: Space, pad East or RB.</summary>
        public static bool DashDown() { var kb = Keyboard.current; var gp = Gamepad.current; return (kb != null && kb.spaceKey.wasPressedThisFrame) || (gp != null && (gp.buttonEast.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame)); }
        public static bool DashHeld() { var kb = Keyboard.current; var gp = Gamepad.current; return (kb != null && kb.spaceKey.isPressed) || (gp != null && (gp.buttonEast.isPressed || gp.rightShoulder.isPressed)); }
        public static Vector2 AimStick() { var gp = Gamepad.current; return gp != null ? gp.rightStick.ReadValue() : Vector2.zero; }
        public static Vector2 MouseScreen() { var m = Mouse.current; return m != null ? m.position.ReadValue() : Vector2.zero; }
        public static bool MouseHeld() { var m = Mouse.current; return m != null && (m.leftButton.isPressed || m.rightButton.isPressed); }
        public static bool PadConfirm() { var gp = Gamepad.current; return gp != null && gp.buttonSouth.wasPressedThisFrame; }
        // ---- t716u: rebindable actions. Binding = Input System Key name ("Q", "Digit1", "Space") or "LMB" / "RMB" / "MMB".
        static readonly System.Collections.Generic.Dictionary<string, Key> _keyCache716 = new System.Collections.Generic.Dictionary<string, Key>();
        static bool K716(string b, out Key k) { if (string.IsNullOrEmpty(b)) { k = Key.None; return false; } if (!_keyCache716.TryGetValue(b, out k)) { if (!System.Enum.TryParse(b, out k)) k = Key.None; _keyCache716[b] = k; } return k != Key.None; }
        public static bool BindHeld(string b)
        {
            var m = Mouse.current; if (b == "LMB") return m != null && m.leftButton.isPressed; if (b == "RMB") return m != null && m.rightButton.isPressed; if (b == "MMB") return m != null && m.middleButton.isPressed;
            var kb = Keyboard.current; return kb != null && K716(b, out var k) && kb[k].isPressed;
        }
        public static bool BindDown(string b)
        {
            var m = Mouse.current; if (b == "LMB") return m != null && m.leftButton.wasPressedThisFrame; if (b == "RMB") return m != null && m.rightButton.wasPressedThisFrame; if (b == "MMB") return m != null && m.middleButton.wasPressedThisFrame;
            var kb = Keyboard.current; return kb != null && K716(b, out var k) && kb[k].wasPressedThisFrame;
        }
        /// <summary>t716u rebinding: the first key / mouse button pressed this frame, or null.</summary>
        public static string AnyPressedBind()
        {
            var m = Mouse.current; if (m != null) { if (m.leftButton.wasPressedThisFrame) return "LMB"; if (m.rightButton.wasPressedThisFrame) return "RMB"; if (m.middleButton.wasPressedThisFrame) return "MMB"; }
            var kb = Keyboard.current; if (kb == null) return null;
            foreach (var kc in kb.allKeys) if (kc != null && kc.wasPressedThisFrame && kc.keyCode != Key.LeftShift && kc.keyCode != Key.RightShift) return kc.keyCode.ToString();
            return null;
        }
        public static Vector2 PadMove716() { var gp = Gamepad.current; return gp != null ? Vector2.ClampMagnitude(gp.leftStick.ReadValue(), 1) : Vector2.zero; }
        public static bool PadDash716() { var gp = Gamepad.current; return gp != null && (gp.buttonEast.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame); }
        public static bool Up716() { var kb = Keyboard.current; return kb != null && kb.upArrowKey.wasPressedThisFrame; }
        public static bool Dn716() { var kb = Keyboard.current; return kb != null && kb.downArrowKey.wasPressedThisFrame; }
        public static bool Enter716() { var kb = Keyboard.current; return kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame); }
        public static bool Esc716() { var kb = Keyboard.current; return kb != null && kb.escapeKey.wasPressedThisFrame; }
        public static Vector2 Arrows716() { var v = Vector2.zero; var kb = Keyboard.current; if (kb == null) return v; if (kb.leftArrowKey.isPressed) v.x -= 1; if (kb.rightArrowKey.isPressed) v.x += 1; if (kb.downArrowKey.isPressed) v.y -= 1; if (kb.upArrowKey.isPressed) v.y += 1; return v; }
        public static bool LmbDown716() { var m = Mouse.current; return m != null && m.leftButton.wasPressedThisFrame; }
        public static bool LmbHeld716() { var m = Mouse.current; return m != null && m.leftButton.isPressed; }
#else
        static KeyCode Map(HKey k)
        {
            switch (k)
            {
                case HKey.Tab: return KeyCode.Tab; case HKey.F1: return KeyCode.F1; case HKey.F2: return KeyCode.F2; case HKey.F3: return KeyCode.F3; case HKey.F4: return KeyCode.F4; case HKey.F5: return KeyCode.F5; case HKey.F6: return KeyCode.F6; case HKey.F7: return KeyCode.F7; case HKey.F8: return KeyCode.F8; case HKey.F9: return KeyCode.F9; case HKey.K: return KeyCode.K; case HKey.R: return KeyCode.R;
                case HKey.Space: return KeyCode.Space; case HKey.One: return KeyCode.Alpha1; case HKey.Two: return KeyCode.Alpha2; case HKey.Three: return KeyCode.Alpha3;
                case HKey.Four: return KeyCode.Alpha4; case HKey.Five: return KeyCode.Alpha5; case HKey.F10: return KeyCode.F10; case HKey.SkillCycle: return KeyCode.N; case HKey.Glow: return KeyCode.G; case HKey.Traps: return KeyCode.T; case HKey.Atmos: return KeyCode.H; case HKey.HookThrower: return KeyCode.J; case HKey.GossipImp: return KeyCode.I; default: return KeyCode.Escape;
            }
        }
        public static bool BackQuote() { return Input.GetKeyDown(KeyCode.BackQuote); }
        public static bool Down(HKey k)
        {
            if (k >= HKey.One && k <= HKey.Five && Shift()) return false;
            if (k == HKey.Glow || k == HKey.Traps || k == HKey.Atmos || k == HKey.HookThrower || k == HKey.GossipImp) { string tx = Input.inputString; bool typed = !string.IsNullOrEmpty(tx) && tx.IndexOf(k == HKey.Glow ? 'g' : k == HKey.Traps ? 't' : k == HKey.Atmos ? 'h' : k == HKey.HookThrower ? 'j' : 'i') >= 0; return Input.GetKeyDown(Map(k)) || typed; }
            if (Input.GetKeyDown(Map(k))) return true;
            int d = DebugDigit(k); if (d < 0) return k == HKey.F10 && Input.GetKeyDown(KeyCode.M);
            bool sh = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); if (sh && Input.GetKeyDown(KeyCode.Alpha0 + d)) return true;
            string t = Input.inputString; return !string.IsNullOrEmpty(t) && t.IndexOf(")!@#$%^&*("[d]) >= 0;   // v0.0.16 typed-char fallback
        }
        public static Vector2 Move()
        {
            Vector2 v = Vector2.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v.x -= 1; if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v.x += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v.y -= 1; if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v.y += 1;
            try { v += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); } catch (System.Exception) { }
            return Vector2.ClampMagnitude(v, 1);
        }
        // Legacy Input Manager has no default right-stick axis; gamepad aim needs the Input System package.
        public static bool DashDown() => Input.GetKeyDown(KeyCode.Space); // v0.0.49 dash
        public static bool DashHeld() => Input.GetKey(KeyCode.Space);
        public static Vector2 AimStick() => Vector2.zero;
        public static Vector2 MouseScreen() => Input.mousePosition;
        public static bool MouseHeld() => Input.GetMouseButton(0) || Input.GetMouseButton(1);
        public static bool PadConfirm() => false;
        // ---- t716u (legacy Input Manager fallback; the project uses the Input System)
        static KeyCode KC716(string b) { if (string.IsNullOrEmpty(b)) return KeyCode.None; if (b.StartsWith("Digit")) b = "Alpha" + b.Substring(5); return System.Enum.TryParse(b, out KeyCode k) ? k : KeyCode.None; }
        public static bool BindHeld(string b) { if (b == "LMB") return Input.GetMouseButton(0); if (b == "RMB") return Input.GetMouseButton(1); if (b == "MMB") return Input.GetMouseButton(2); var k = KC716(b); return k != KeyCode.None && Input.GetKey(k); }
        public static bool BindDown(string b) { if (b == "LMB") return Input.GetMouseButtonDown(0); if (b == "RMB") return Input.GetMouseButtonDown(1); if (b == "MMB") return Input.GetMouseButtonDown(2); var k = KC716(b); return k != KeyCode.None && Input.GetKeyDown(k); }
        public static string AnyPressedBind()
        {
            if (Input.GetMouseButtonDown(0)) return "LMB"; if (Input.GetMouseButtonDown(1)) return "RMB"; if (Input.GetMouseButtonDown(2)) return "MMB"; if (!Input.anyKeyDown) return null;
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode))) if (k < KeyCode.Mouse0 && k != KeyCode.LeftShift && k != KeyCode.RightShift && Input.GetKeyDown(k)) { var n = k.ToString(); return n.StartsWith("Alpha") ? "Digit" + n.Substring(5) : n; }
            return null;
        }
        public static Vector2 PadMove716() => Vector2.zero;
        public static bool PadDash716() => false;
        public static bool Up716() => Input.GetKeyDown(KeyCode.UpArrow);
        public static bool Dn716() => Input.GetKeyDown(KeyCode.DownArrow);
        public static bool Enter716() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        public static bool Esc716() => Input.GetKeyDown(KeyCode.Escape);
        public static Vector2 Arrows716() { var v = Vector2.zero; if (Input.GetKey(KeyCode.LeftArrow)) v.x -= 1; if (Input.GetKey(KeyCode.RightArrow)) v.x += 1; if (Input.GetKey(KeyCode.DownArrow)) v.y -= 1; if (Input.GetKey(KeyCode.UpArrow)) v.y += 1; return v; }
        public static bool LmbDown716() => Input.GetMouseButtonDown(0);
        public static bool LmbHeld716() => Input.GetMouseButton(0);
#endif
    }
}
