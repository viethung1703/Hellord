using UnityEngine;

// t1148/t1149/t1150 Sins CONTROL feel (VH: "buttons feel are bad", "where is ask player before unlock?", research t1150).
// Spec: /workspace/Hellord/Docs/UI_FEEL_RESEARCH_t1150.md. Mouse activates on RELEASE over the same control (drag-off cancels),
// keys act on press; press = inverted bevel + label 1 native px down-right; slot unlock asks first, then a ~1 s skippable
// ceremony; denied = red flash + integer shake + reason text; currency rolls down after a spend. Collection tiles stay quiet.
// Economy/save unchanged: one debit + one save per confirmed unlock. All timings in SinFeelTune (one-line tuning).
namespace Hellord
{
    public static class SinFeelTune
    {
        public static float HoverWash = 0.20f, PressWash = 0.45f, CommitFlashS = 0.12f;
        public static float DenyS = 0.40f, DenyButtonS = 0.30f, DenyShakePx = 2f, DenyKicks = 3.5f, DenyWash = 0.25f, DenyMsgS = 1.6f;
        public static float UnlockGuardS = 0.25f;                  // ignore further unlock clicks after a confirm
        public static float CrackEndS = 0.20f, HitstopEndS = 0.28f, BreakEndS = 0.55f, OpenFullS = 1.0f, GlowHoldS = 1.6f, GlowEndS = 2.0f;
        public static int Shards = 8; public static float ShardMinPx = 10, ShardMaxPx = 18, KickPx = 2f;
        public static float UnlockMsgS = 2.4f, CurrencyRollS = 0.5f;
        public static float ModalOpenS = 0.15f, ModalCloseS = 0.10f, ModalDim = 0.55f, ModalOutsideGraceS = 0.30f;
        /// <summary>t1151: modal ignores all clicks/Enter this long after opening; buttons then need a fresh mouse-down that starts on them.</summary>
        public static float ModalInputGuardS = 0.30f;
        /// <summary>t1151b: the guard also needs this many repaints of the open modal. Input lag is counted in frames, so on a slow machine (13 fps measured in software rendering) early clicks/Enter that arrive late still cannot pass. At 60 fps the 0.30 s time guard dominates.</summary>
        public static int ModalGuardMinFrames = 8;
        /// <summary>t1155 ember hover: seconds per pulse step (2 steps), heat band alphas. 0 = no pulse.</summary>
        public static float EmberPulseS = 0.9f, EmberBand1 = 0.14f, EmberBand2 = 0.07f;
        /// <summary>t1155: true = ember/fire palette for hover, press, unlock reward and currency roll; false = previous gold.</summary>
        public static bool EmberTheme = true;
        /// <summary>t1151: after the modal closes, panel controls ignore clicks this long (stops the 2nd click of a double-click landing on a tile).</summary>
        public static float ModalAfterCloseGuardS = 0.25f;
    }
    public partial class HellordGame
    {
        public static bool SinFeel1148On = true, SinConfirm1149On = true;
        string _f48Down, _f48Released, _f48Flash, _f48Deny, _f48Hover, _f48HoverSeen; float _f48FlashT = -9, _f48DenyT = -9;
        public int _f48UnlockSlot = -1, _f48DenySlot = -1, _f48Purchases; public float _f48UnlockT = -9, _f48DenyST = -9; bool _f48UnlockSfx;
        string _f48Hdr; Color _f48HdrC; float _f48HdrUntil = -9;
        int _cr48S0, _cr48S1, _cr48G0, _cr48G1; float _cr48T = -9;
        static readonly Color F48_DARK = new Color32(8, 7, 6, 255), F48_LIGHT = new Color32(120, 108, 88, 255), F48_WHITE = new Color32(255, 248, 228, 255);
        static float Now48 => Time.unscaledTime;
        static Color A48(Color c, float a) { c.a = a; return c; }
        // t1155 Endesga 32 fire ramp (matches braziers / lava cracks)
        static readonly Color E_DEEP = new Color32(0x3e, 0x27, 0x31, 255), E_RED = new Color32(0xa2, 0x26, 0x33, 255), E_HOT = new Color32(0xe4, 0x3b, 0x44, 255),
            E_ORANGE = new Color32(0xf7, 0x76, 0x22, 255), E_AMBER = new Color32(0xfe, 0xae, 0x34, 255), E_YEL = new Color32(0xfe, 0xe7, 0x61, 255);
        static Color Reward48 => SinFeelTune.EmberTheme ? E_AMBER : (Color)S74_GOLD;   // unlock text / header
        static Color Heat48 => SinFeelTune.EmberTheme ? E_ORANGE : (Color)S74_GOLD;    // rims, currency roll
        int EmberStep48 => SinFeelTune.EmberPulseS <= 0 ? 0 : ((int)(Now48 / SinFeelTune.EmberPulseS)) & 1;
        /// <summary>t1155 hover: dark-red outer rim, dithered hot inner rim (heat seeping through stone cracks), banded heat rising from the bottom, 2 tiny hot specks; 2-step slow pulse.</summary>
        void Ember48(string id, float x, float y, float w, float h, bool pressed)
        {
            if (!SinFeelTune.EmberTheme)
            {   if (pressed) { Ol74(x, y, w, h, S74_GOLD); return; }
                Wash48(x + 1, y + 1, w - 2, h - 2, A48(new Color(1f, 0.72f, 0.38f), SinFeelTune.HoverWash)); Ol74(x, y, w, h, S74_GOLD); Ol74(x + 1, y + 1, w - 2, h - 2, A48(S74_GOLD, 0.55f)); return; }
            x = Mathf.Round(x); y = Mathf.Round(y); w = Mathf.Round(w); h = Mathf.Round(h); int st = EmberStep48;
            if (pressed) { Ol74(x, y, w, h, E_HOT); Box74(x, y + h - 1, w, 1, E_ORANGE); return; }
            float b = Mathf.Max(1, Mathf.Floor(h / 6));
            Wash48(x + 2, y + h - 2 - b, w - 4, b, A48(E_HOT, SinFeelTune.EmberBand1 + 0.03f * st));
            Wash48(x + 2, y + h - 2 - 2 * b, w - 4, b, A48(E_RED, SinFeelTune.EmberBand2));
            Ol74(x, y, w, h, E_RED);
            Color hot = A48(E_HOT, 0.9f);
            for (int i = 1; i < w - 1; i++) { if (((i + st) & 1) != 0) continue; Box74(x + i, y + 1, 1, 1, A48(E_RED, 0.8f)); Box74(x + i, y + h - 2, 1, 1, hot); }
            for (int j = 2; j < h - 2; j++) { if (((j + st) & 1) != 0) continue; Box74(x + 1, y + j, 1, 1, A48(E_RED, 0.8f)); Box74(x + w - 2, y + j, 1, 1, A48(E_RED, 0.8f)); }
            int hs = id.GetHashCode() & 0x7fffffff, s1 = 2 + hs % Mathf.Max(1, (int)w - 4), s2 = 2 + (hs / 7) % Mathf.Max(1, (int)w - 4);
            Box74(x + s1, y + h - 1, 1, 1, E_ORANGE); Box74(x + s2, y, 1, 1, st == 1 ? E_AMBER : E_ORANGE);
        }

        void Feel48Begin()
        {
            var e = Event.current; _f48Released = null;
            if (e.rawType == EventType.MouseUp && e.button == 0) { _f48Released = _f48Down; _f48Down = null; }
            // skippable ceremony: any click while it is still cracking/breaking jumps straight to the OPEN state
            if (SinFeel1148On && e.type == EventType.MouseDown && !Confirm49Open && _f48UnlockSlot >= 0)
            { float t = Now48 - _f48UnlockT; if (t >= 0 && t < SinFeelTune.BreakEndS) { _f48UnlockT = Now48 - SinFeelTune.BreakEndS; UnlockSfx48(); e.Use(); } }
            if (e.type != EventType.Repaint) return;
            if (_f48Hover != null && _f48Hover != _f48HoverSeen) HellordAudio.Play("ui_hover");
            _f48HoverSeen = _f48Hover; _f48Hover = null;
        }
        void Wash48(float x, float y, float w, float h, Color c) { var g = GUI.color; R53(U74(x), U74(y), U74(w), U74(h), c); GUI.color = g; }
        bool Pressed48(string id, float x, float y, float w, float h) => SinFeel1148On && _f48Down == id && Over74(x, y, w, h);
        float Shake48(float t0, float dur) { float t = Now48 - t0; return t >= 0 && t < dur ? Mathf.Round(Mathf.Sin(t / dur * SinFeelTune.DenyKicks * 2 * Mathf.PI) * SinFeelTune.DenyShakePx * (1 - t / dur)) : 0; }
        /// <summary>Label offset for a control: +1,+1 native px while pressed; integer shake while denied.</summary>
        Vector2 Off48(string id, float x, float y, float w, float h)
        {
            if (!SinFeel1148On) return Vector2.zero;
            if (Pressed48(id, x, y, w, h)) return Vector2.one;
            return _f48Deny == id ? new Vector2(Shake48(_f48DenyT, SinFeelTune.DenyButtonS), 0) : Vector2.zero;
        }
        /// <summary>State visuals; call BEFORE the label (state replaces the face, never stacks on the text).</summary>
        void BtnFx48(string id, float x, float y, float w, float h, bool enabled)
        {
            bool over = Over74(x, y, w, h) && (!Confirm49Open || id.StartsWith("c49"));
            if (!SinFeel1148On || Event.current.type != EventType.Repaint) return;
            float now = Now48;
            if (enabled && over) _f48Hover = id;
            if (_f48Deny == id && now - _f48DenyT < SinFeelTune.DenyButtonS)
            {
                float a = 1 - (now - _f48DenyT) / SinFeelTune.DenyButtonS;
                Wash48(x + 1, y + 1, w - 2, h - 2, A48(new Color(0.84f, 0.27f, 0.22f), SinFeelTune.DenyWash * a)); Ol74(x, y, w, h, A48(S74_RED, a)); Ol74(x + 1, y + 1, w - 2, h - 2, A48(S74_RED, a));
            }
            else if (enabled && over && _f48Down == id)
            {   // pressed: inverted bevel (dark top/left, light bottom/right), dark face; caller shifts the label +1,+1
                Wash48(x + 1, y + 1, w - 2, h - 2, new Color(0, 0, 0, SinFeelTune.PressWash));
                Box74(x + 1, y + 1, w - 2, 2, F48_DARK); Box74(x + 1, y + 1, 2, h - 2, F48_DARK);
                Box74(x + 1, y + h - 2, w - 2, 1, F48_LIGHT); Box74(x + w - 2, y + 1, 1, h - 2, F48_LIGHT);
                Ember48(id, x, y, w, h, true);
            }
            else if (enabled && over)
            {   // hover: warm firelit wash + gold rim
                Ember48(id, x, y, w, h, false);
            }
            if (_f48Flash == id && now - _f48FlashT < SinFeelTune.CommitFlashS)
            {   // commit flash on release: 1 frame white rim, then gold easing out
                float k = (now - _f48FlashT) / SinFeelTune.CommitFlashS; var c = k < 0.25f ? F48_WHITE : A48(Heat48, 1 - k);
                Ol74(x - 1, y - 1, w + 2, h + 2, c); Ol74(x, y, w, h, c);
            }
        }
        /// <summary>Unified press model: mouse-down shows pressed, mouse-up over the SAME control commits, drag-off cancels.
        /// Disabled controls deny on press (immediate ack). Autotest fake clicks commit immediately.</summary>
        bool Press48(string id, float x, float y, float w, float h, bool enabled)
        {
            if (!SinFeel1148On) return enabled && Hit74(x, y, w, h);
            if (!Gate49) return false;
            if (_s74Click.HasValue && new Rect(x, y, w, h).Contains(_s74Click.Value)) { _s74Click = null; if (enabled) Commit48(id); else Deny48(id); return enabled; }
            if (_block53) return false;
            var e = Event.current; bool over = Over74(x, y, w, h);
            if (e.type == EventType.MouseDown && e.button == 0 && over) { e.Use(); if (enabled) _f48Down = id; else Deny48(id); return false; }
            if (_f48Released == id && e.rawType == EventType.MouseUp && over && enabled) { _f48Released = null; e.Use(); if (e.clickCount > 1 && id.StartsWith("slot")) return false; Commit48(id); return true; }
            return false;
        }
        void Commit48(string id) { _f48Flash = id; _f48FlashT = Now48; }
        void Deny48(string id) { _f48Deny = id; _f48DenyT = Now48; HellordAudio.Play("ui_denied"); }
        bool Btn48(string id, float x, float y, float w, float h, bool enabled) => Press48(id, x, y, w, h, enabled);
        bool BackBtn48(float x, float y, float w, float h) => !Confirm49Open && Press48("back", x, y, w, h, true);

        // ---- currency roll-down after a spend (shared counter draws through Shown48)
        int Shown48(bool sigil, int actual)
        {
            float k = (Now48 - _cr48T) / Mathf.Max(0.01f, SinFeelTune.CurrencyRollS);
            if (!SinFeel1148On || k < 0 || k >= 1) return actual;
            int a = sigil ? _cr48G0 : _cr48S0, b = sigil ? _cr48G1 : _cr48S1; if (b != actual) return actual;
            float e = 1 - (1 - k) * (1 - k); return Mathf.RoundToInt(Mathf.Lerp(a, b, e));
        }
        bool Rolling48 => SinFeel1148On && Now48 - _cr48T < SinFeelTune.CurrencyRollS + 0.15f;

        // ---- t1149 confirm before any slot unlock spends currency
        public int _c49Slot = -1; int _c49Frames; float _c49T = -9, _c49CloseT = -9; int _c49Sc, _c49Gc; bool _c49Hit;
        public static readonly Rect C49Box = new Rect(140, 92, 200, 86), C49No = new Rect(156, 154, 80, 17), C49Yes = new Rect(244, 154, 80, 17); // t1151: UNLOCK on the right, x>=244, never under a slot socket (sockets end at x=214)
        public bool Confirm49Open => _c49Slot >= 0;
        bool Gate49 => _c49Hit || !Confirm49Open && Now48 - _c49CloseT >= SinFeelTune.ModalAfterCloseGuardS; // every Sins control hit-test asks this
        void OpenConfirm49(int i, int sc, int gc) { _c49Slot = i; _c49Sc = sc; _c49Gc = gc; _c49T = Now48; _c49Frames = 0; _f48Down = null; _f48Released = null; HellordAudio.Play("ui_confirm"); }
        bool Guard49 => Now48 - _c49T < SinFeelTune.ModalInputGuardS || _c49Frames < SinFeelTune.ModalGuardMinFrames;
        void CancelConfirm49() { if (_c49Slot < 0) return; _c49Slot = -1; _c49CloseT = Now48; _sinBackFrame87 = Time.frameCount; HellordAudio.Play("ui_back"); }
        void AcceptConfirm49()
        {
            int i = _c49Slot; if (i < 0) return; _c49Slot = -1; _c49CloseT = Now48; _sinBackFrame87 = Time.frameCount;
            PactLoad51(); int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1);
            if (i != _sinSlots737 || i >= SlotsMax737 || sc != _c49Sc || gc != _c49Gc || _souls51 < sc || _sigils51 < gc)
            { _f48DenySlot = i; _f48DenyST = Now48; HellordAudio.Play("ui_denied"); Hdr48("NEED " + Num88(sc) + " SOULS + " + Num88(gc) + " SIGIL" + (gc == 1 ? "" : "S"), S74_RED, SinFeelTune.DenyMsgS); return; }
            _cr48S0 = _souls51; _cr48G0 = _sigils51;
            _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f;
            _cr48S1 = _souls51; _cr48G1 = _sigils51; _cr48T = Now48; HellordAudio.Play("ui_confirm"); Unlocked48(i);
        }
        public bool ConfirmKeys49()
        {   // keys act on press
            if (!Confirm49Open) return false;
            if (HInput.Down(HKey.Escape)) CancelConfirm49(); else if (HInput.Enter716() && !Guard49) AcceptConfirm49();
            return true;
        }
        string Bal49(int n) => n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        void Confirm49Draw()
        {
            var ev = Event.current; float now = Now48;
            if (_c49Slot < 0)
            {
                float c = now - _c49CloseT;
                if (c >= 0 && c < SinFeelTune.ModalCloseS && ev.type == EventType.Repaint) R53(DimX46, DimX46, DimW46, DimH46, new Color(0, 0, 0, SinFeelTune.ModalDim * (1 - c / SinFeelTune.ModalCloseS)));
                return;
            }
            float k = Mathf.Clamp01((now - _c49T) / Mathf.Max(0.01f, SinFeelTune.ModalOpenS)), e = 1 - (1 - k) * (1 - k);
            if (ev.type == EventType.Repaint) { R53(DimX46, DimX46, DimW46, DimH46, new Color(0, 0, 0, SinFeelTune.ModalDim * e)); if (_c49Frames < 1000) _c49Frames++; }
            float dy = Mathf.Round((1 - e) * -8); var b = C49Box; b.y += dy; var yes = C49Yes; yes.y += dy; var no = C49No; no.y += dy;
            int sc = _c49Sc, gc = _c49Gc; string title = "UNLOCK SIN SLOT " + (_c49Slot + 1) + "?";
            string cost = "COST " + Bal49(sc) + " SOULS + " + Bal49(gc) + " SIGIL" + (gc == 1 ? "" : "S");
            string l1 = "SOULS LEFT: " + Bal49(_souls51 - sc), l2 = "SIGILS LEFT: " + Bal49(_sigils51 - gc);
            if (_k98Draw) { b = K98Modal; b.y += dy; Confirm98Draw(b, ref yes, ref no, dy, title, cost, l1, l2); } // t1175 kit modal (same guard/press logic below)
            else if (_t46Draw) { b = new Rect(140, 92, 200, 84); b.y += dy; Confirm1246Draw(b, ref yes, ref no, dy, title, cost, l1, l2); } // t1259 pass3e modal
            else {
            Box74(b.x - 2, b.y - 2, b.width + 4, b.height + 4, new Color32(8, 7, 6, 255));
            StonePlaque89(b.x, b.y, b.width, b.height, 3);
            Ol74(b.x + 4, b.y + 4, b.width - 8, b.height - 8, new Color32(112, 102, 84, 255));
            Tx74(title, b.x + (b.width - TW74(title)) / 2, b.y + 10, S74_GOLD);
            Box74(b.x + 12, b.y + 21, b.width - 24, 1, new Color32(14, 13, 11, 255)); Box74(b.x + 12, b.y + 22, b.width - 24, 1, new Color32(57, 54, 47, 255));
            Tx74(cost, b.x + (b.width - TW74(cost)) / 2, b.y + 28, S74_SAND);
            Tx74(l1, b.x + (b.width - TW74(l1)) / 2, b.y + 40, S74_DIM); Tx74(l2, b.x + (b.width - TW74(l2)) / 2, b.y + 50, S74_DIM);
            Patch88(_s74on, new Rect(0, 0, 55, 20), yes); BtnFx48("c49yes", yes.x, yes.y, yes.width, yes.height, true);
            var oy = Off48("c49yes", yes.x, yes.y, yes.width, yes.height); Lbl74("UNLOCK", yes.x + oy.x, yes.y + oy.y, yes.width, yes.height, S74_EMB, 2);
            Recess87(no.x, no.y, no.width, no.height); BtnFx48("c49no", no.x, no.y, no.width, no.height, true);
            var on = Off48("c49no", no.x, no.y, no.width, no.height); Lbl74("CANCEL", no.x + on.x, no.y + on.y, no.width, no.height, S74_SAND, 2);
            }
            if (Guard49)
            {   // t1151 input guard: nothing in the modal can be pressed yet; drop stray presses so they cannot complete later
                if (_s74Click.HasValue && b.Contains(_s74Click.Value)) _s74Click = null;
                if (ev.type == EventType.MouseDown || ev.type == EventType.MouseUp) ev.Use();
                if (_f48Down == "c49yes" || _f48Down == "c49no") _f48Down = null;
                return;
            }
            _c49Hit = true; bool y1 = false, n1 = false;
            try { y1 = Press48("c49yes", yes.x, yes.y, yes.width, yes.height, true); if (!y1) n1 = Press48("c49no", no.x, no.y, no.width, no.height, true); }
            finally { _c49Hit = false; }
            if (y1) { AcceptConfirm49(); return; }
            if (n1) { CancelConfirm49(); return; }
            if (ev.type == EventType.MouseDown && ev.button == 0 && now - _c49T > SinFeelTune.ModalOutsideGraceS && !Guard49 && !b.Contains(ev.mousePosition / K74)) { ev.Use(); CancelConfirm49(); return; }
            if (ev.type == EventType.MouseDown && Confirm49Open) ev.Use(); // modal swallows clicks inside the box
        }
        void Hdr48(string s, Color c, float dwell) { _f48Hdr = s; _f48HdrC = c; _f48HdrUntil = Now48 + dwell; }
        bool HdrOverride48(out string s, out Color c) { s = _f48Hdr; c = _f48HdrC; return SinFeel1148On && _f48Hdr != null && Now48 < _f48HdrUntil; }

        /// <summary>Locked socket: press shows carved-in, release opens the confirm (or denies with a reason). Never spends.</summary>
        bool LockSocket48(int i, float x, float y, float s, bool next, bool afford, int sc, int gc)
        {
            float now = Now48; string id = "slot" + i;
            if (SinFeel1148On && Event.current.type == EventType.Repaint)
            {
                bool over = Over74(x, y, s, s) && !Confirm49Open, deny = _f48DenySlot == i && now - _f48DenyST < SinFeelTune.DenyS;
                if (over && _f48Down == id) { Wash48(x + 3, y + 3, s - 6, s - 6, new Color(0, 0, 0, SinFeelTune.PressWash)); Box74(x + 3, y + 3, s - 6, 2, F48_DARK); Box74(x + 3, y + 3, 2, s - 6, F48_DARK); Box74(x + 3, y + s - 4, s - 6, 1, F48_LIGHT); Box74(x + s - 4, y + 3, 1, s - 6, F48_LIGHT); if (next && afford) Ember48(id, x + 2, y + 2, s - 4, s - 4, true); else Ol74(x + 2, y + 2, s - 4, s - 4, S74_RED); }
                else if (next && over && !deny) { _f48Hover = id; if (afford) Ember48(id, x + 2, y + 2, s - 4, s - 4, false); else Ol74(x + 3, y + 3, s - 6, s - 6, A48(S74_RED, 0.8f)); }
                if (deny) { float a = 1 - (now - _f48DenyST) / SinFeelTune.DenyS; Wash48(x + 3, y + 3, s - 6, s - 6, A48(new Color(0.84f, 0.2f, 0.16f), SinFeelTune.DenyWash * a)); Ol74(x + 1, y + 1, s - 2, s - 2, A48(S74_RED, a)); Ol74(x + 2, y + 2, s - 4, s - 4, A48(S74_RED, a)); }
            }
            if (!Press48(id, x, y, s, s, true)) return false;
            if (SinFeel1148On && now - _f48UnlockT < SinFeelTune.UnlockGuardS) return false; // never chains into a 2nd purchase
            if (next && afford) { if (SinConfirm1149On) { OpenConfirm49(i, sc, gc); return false; } return true; }
            if (SinFeel1148On)
            {
                _f48DenySlot = i; _f48DenyST = now; _f48Flash = null; HellordAudio.Play("ui_denied");
                if (next) Hdr48("NEED " + Num88(sc) + " SOULS + " + Num88(gc) + " SIGIL" + (gc == 1 ? "" : "S"), S74_RED, SinFeelTune.DenyMsgS);
                else Hdr48("UNLOCK SLOT " + (_sinSlots737 + 1) + " FIRST", S74_RED, SinFeelTune.DenyMsgS);
            }
            return false;
        }
        void Unlocked48(int i)
        {
            _f48Purchases++; if (!SinFeel1148On) return;
            _f48UnlockSlot = i; _f48UnlockT = Now48; _f48DenySlot = -1; _f48UnlockSfx = false;
            Hdr48("SIN SLOT " + (i + 1) + " UNLOCKED", Reward48, SinFeelTune.UnlockMsgS);
        }
        void UnlockSfx48() { if (_f48UnlockSfx) return; _f48UnlockSfx = true; HellordAudio.Play("pact_unlock"); }
        float DenyShake48(int i) => _f48DenySlot == i ? Shake48(_f48DenyST, SinFeelTune.DenyS) : 0;
        /// <summary>Ceremony in the (now unlocked) socket: crack -> hitstop flash + kick -> shards -> OPEN gold -> settle.</summary>
        bool UnlockReveal48(int i, float x, float y, float s)
        {
            if (!SinFeel1148On || i != _f48UnlockSlot) return false; float t = Now48 - _f48UnlockT; if (t < 0 || t > SinFeelTune.GlowEndS) return false;
            if (Event.current.type != EventType.Repaint) return true;
            float cx = x + s / 2, cy = y + s / 2;
            if (t >= SinFeelTune.CrackEndS) UnlockSfx48();
            if (t < SinFeelTune.HitstopEndS)
            {   // anticipation: socket darkens, hairline ember cracks grow toward the centre; then a 1-2 frame hitstop: frozen, white flash, kick
                bool hit = t >= SinFeelTune.CrackEndS; float k = Mathf.Clamp01(t / SinFeelTune.CrackEndS);
                float kick = hit ? SinFeelTune.KickPx : 0;
                Wash48(x + 3, y + 3, s - 6, s - 6, new Color(0, 0, 0, 0.35f * k));
                GUI.color = Color.white; if (_t46Draw) Spr46("lock_glyph", x + 12 + kick, y + 11); else Img74(_s74lock, x + kick, y);
                int[,] dirs = { { 1, -1 }, { -1, -1 }, { 1, 1 }, { -1, 1 }, { 0, -1 }, { 1, 0 } };
                int len = Mathf.RoundToInt(2 + k * 12);
                for (int r = 0; r < 6; r++) for (int j = 0; j < len; j++)
                {   int jig = ((j * 7 + r * 3) % 3) - 1; float px = cx + kick + dirs[r, 0] * j + (dirs[r, 0] == 0 ? jig : 0), py = cy + dirs[r, 1] * j + (dirs[r, 1] == 0 ? jig : 0);
                    Box74(Mathf.Round(px), Mathf.Round(py), 1, 1, j < len - 2 ? (SinFeelTune.EmberTheme ? E_AMBER : (Color)S74_GOLD) : (SinFeelTune.EmberTheme ? E_HOT : (Color)S74_EMB)); }
                if (hit) { Wash48(x + 1, y + 1, s - 2, s - 2, A48(F48_WHITE, 0.85f)); Ol74(x, y, s, s, F48_WHITE); }
                return true;
            }
            // t1151: the lock is gone for good after the hitstop: flatten the well so no lock/plinth silhouette shows under OPEN
            Box74(x + 5, y + 6, s - 10, s - 13, new Color32(8, 7, 7, 255));
            if (t < SinFeelTune.BreakEndS)
            {   // break: hard pixel shards fly out and fade
                float u = (t - SinFeelTune.HitstopEndS) / (SinFeelTune.BreakEndS - SinFeelTune.HitstopEndS);
                Wash48(x + 1, y + 1, s - 2, s - 2, A48(SinFeelTune.EmberTheme ? new Color(0.97f, 0.46f, 0.13f) : new Color(1f, 0.86f, 0.5f), 0.6f * (1 - u)));
                for (int j = 0; j < SinFeelTune.Shards; j++)
                {
                    float ang = j * 2.39996f, dist = Mathf.Lerp(SinFeelTune.ShardMinPx, SinFeelTune.ShardMaxPx, (j * 37 % 11) / 10f) * u + 6;
                    Color c = j % 3 == 0 ? (SinFeelTune.EmberTheme ? (j % 2 == 0 ? E_ORANGE : E_HOT) : (Color)S74_GOLD) : j % 3 == 1 ? new Color32(118, 110, 98, 255) : new Color32(70, 66, 60, 255);
                    Box74(Mathf.Round(cx + Mathf.Cos(ang) * dist), Mathf.Round(cy + Mathf.Sin(ang) * dist + 6 * u * u), 2, 2, A48(c, 1 - u * 0.8f));
                }
            }
            // reward: socket lit gold, OPEN; holds then settles
            float glow = t < SinFeelTune.GlowHoldS ? 1f : 1 - (t - SinFeelTune.GlowHoldS) / (SinFeelTune.GlowEndS - SinFeelTune.GlowHoldS);
            float rise = Mathf.Clamp01((t - SinFeelTune.HitstopEndS) / (SinFeelTune.OpenFullS - SinFeelTune.HitstopEndS));
            var gcol = A48(Heat48, glow * Mathf.Max(rise, 0.5f));
            Ol74(x + 1, y + 1, s - 2, s - 2, gcol); Ol74(x + 2, y + 2, s - 4, s - 4, SinFeelTune.EmberTheme ? A48(E_RED, gcol.a) : gcol);
            Wash48(x + 4, y + 4, s - 8, s - 8, A48(SinFeelTune.EmberTheme ? new Color(0.89f, 0.23f, 0.27f) : new Color(1f, 0.7f, 0.3f), 0.10f * glow));
            Lbl74("OPEN", x, y, s, s, A48(Reward48, Mathf.Max(glow, 0.35f)), 2);
            return true;
        }
    }
}
