using UnityEngine;
#if ENABLE_INPUT_SYSTEM && HELLORD_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// t503u: VH-locked main menu art (Art/previews/step36_main_menu/v69_pact_seal_hover_simple), baked by tools/main_menu_v69.py
// into Resources/HellordArt/MainMenu69. Drawn in the art's own 700x392 space. Pact panel is a PLACEHOLDER (VH remakes later).
namespace Hellord
{
    public partial class HellordGame
    {
        const float K69 = 640f / 700f; const int NF69 = 48;
        static readonly int[][] RowY69 = { new[] { 129, 164, 190, 216, 242 }, new[] { 130, 155, 190, 216, 242 }, new[] { 130, 156, 181, 216, 242 }, new[] { 130, 156, 182, 207, 242 }, new[] { 130, 156, 182, 208, 233 } };
        static readonly string[] Rows69 = { "DESCEND", "SACRIFICE ALTAR", "CODEX", "SETTINGS", "QUIT" };
        static readonly Vector2[] Sock69 = { new Vector2(578, 135), new Vector2(648, 135), new Vector2(663, 169), new Vector2(648, 203), new Vector2(578, 203), new Vector2(563, 169) }; // t581u: all 6 ring orbs, clockwise from top-left
        static readonly Color C69_RED = new Color32(0xff, 0x00, 0x49, 255);
        int _sel69, _row69, _panel69; // _sel69 0..4 rows, 5 = seal. _panel69: 0 closed, 1 open(ing), 2 closing
        float _hov69, _press69 = -1, _panelU69; Vector2 _lastMouse69 = new Vector2(-9, -9); float _navY69, _navX69;
        readonly System.Collections.Generic.Dictionary<string, Texture2D> _tex69 = new System.Collections.Generic.Dictionary<string, Texture2D>();
        Texture2D T69(string n) { if (_tex69.TryGetValue(n, out var t)) return t; t = Resources.Load<Texture2D>("HellordArt/MainMenu69/mm69_" + n); if (t) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } _tex69[n] = t; return t; }
        void I69(string n, float x, float y, float a = 1) { var t = T69(n); if (!t || a <= 0.001f) return; GUI.color = new Color(1, 1, 1, Mathf.Clamp01(a)); GUI.DrawTexture(new Rect(x, y, t.width, t.height), t); GUI.color = Color.white; }
        static float TW69(string s) => (Sanitize(s).Length * 6 - 1) / K69;
        void Tx69(string s, float x, float y, Color c) { Tx56Map(ref x, ref y); var m = GUI.matrix; GUI.matrix = _base53; Text(Sanitize(s), Mathf.Round(x * K69), Mathf.Round(y * K69), c, true, 1); GUI.matrix = m; }
        void Tx69(string s, float x, float y, Color c, bool shadow) { Tx56Map(ref x, ref y); var m = GUI.matrix; GUI.matrix = _base53; Text(Sanitize(s), Mathf.Round(x * K69), Mathf.Round(y * K69), c, shadow, 1); GUI.matrix = m; }
        void TxC69(string s, float cx, float y, Color c) => Tx69(s, cx - TW69(s) / 2, y, c);
        bool SealOver69() => Over51(549, 108, 128, 140);
        int SockOver69() { var m = Event.current.mousePosition; for (int i = 0; i < Sock69.Length; i++) if ((m - Sock69[i]).sqrMagnitude <= 11 * 11) return i; return -1; }
        int SelNode69(int k) { for (int i = 0, c = 0; i < 31; i++) if ((_pactSel51 & Bit(i)) != 0) { if (c == k) return i; c++; } return -1; }

        // ---------------- input (keyboard, gamepad, legacy) ----------------
        static bool K69Down(int which)
        {   // 0 up 1 down 2 left 3 right 4 confirm 5 back
#if ENABLE_INPUT_SYSTEM && HELLORD_INPUT_SYSTEM
            var kb = Keyboard.current; var gp = Gamepad.current; bool r = false;
            if (kb != null) switch (which)
                {
                    case 0: r = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame; break;
                    case 1: r = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame; break;
                    case 2: r = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame; break;
                    case 3: r = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame; break;
                    case 4: r = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame; break;
                    case 5: r = kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame; break;
                }
            if (!r && gp != null) switch (which)
                {
                    case 0: r = gp.dpad.up.wasPressedThisFrame; break; case 1: r = gp.dpad.down.wasPressedThisFrame; break;
                    case 2: r = gp.dpad.left.wasPressedThisFrame; break; case 3: r = gp.dpad.right.wasPressedThisFrame; break;
                    case 4: r = gp.buttonSouth.wasPressedThisFrame || gp.startButton.wasPressedThisFrame; break; case 5: r = gp.buttonEast.wasPressedThisFrame; break;
                }
            return r;
#else
            switch (which)
            {
                case 0: return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
                case 1: return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
                case 2: return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
                case 3: return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
                case 4: return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);
                default: return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1);
            }
#endif
        }
        bool Nav69(int w)
        {   // keys/dpad, plus a left-stick edge (HInput.Move) so analog sticks navigate too
            if (K69Down(w)) return true;
            var mv = HInput.Move(); float v = w < 2 ? mv.y : mv.x; ref float last = ref (w < 2 ? ref _navY69 : ref _navX69);
            return false || (w == 0 && v > 0.6f && last <= 0.6f) || (w == 1 && v < -0.6f && last >= -0.6f) || (w == 2 && v < -0.6f && last >= -0.6f) || (w == 3 && v > 0.6f && last <= 0.6f);
        }

        void MenuTick69()
        {
            if(Time.frameCount==_sinBackFrame87)return; // modal Back/Escape consumed this frame; never fall through to Quit/another panel
            if (_sinPanel737) { SinPanelTick737(); return; } // t770u
            float dt = Time.unscaledDeltaTime;
            bool up = Nav69(0), dn = Nav69(1), lf = Nav69(2), rt = Nav69(3), ok = K69Down(4), back = K69Down(5);
            var mv = HInput.Move(); _navY69 = mv.y; _navX69 = mv.x;
            if (_press69 >= 0) { _press69 += dt; if (_press69 >= PT.mm69PressS) { _press69 = -1; if (PT.mm69SealOpensAltar) OpenAltar53(); else _panel69 = 1; } /* t511: seal = Sacrifice Altar */ }
            if (_panel69 == 1) _panelU69 = Mathf.Min(1, _panelU69 + dt / PT.mm69PanelOpenS);
            else if (_panel69 == 2) { _panelU69 -= dt / PT.mm69PanelCloseS; if (_panelU69 <= 0) { _panelU69 = 0; _panel69 = 0; } }
            _hov69 = Mathf.MoveTowards(_hov69, _sel69 == 5 && _panel69 == 0 ? 1 : 0, dt / PT.mm69HoverFadeS);
            if (_panel69 == 1) { if (back) _panel69 = 2; else if (ok && _panelU69 >= 1) { _panel69 = 0; _panelU69 = 0; OpenAltar53(); } return; }
            if (_panel69 != 0 || _press69 >= 0 || _sinPanel737) return;
            if (_sel69 == 5) { if (lf || up || dn) _sel69 = _row69; }
            else { if (up) _sel69 = (_sel69 + 4) % 5; if (dn) _sel69 = (_sel69 + 1) % 5; if (rt) { _row69 = _sel69; _sel69 = 5; } }
            if (_sel69 < 5) _row69 = _sel69;
            if (ok) Act69(_sel69);
        }
        void Act69(int i)
        {
            switch (i)
            {
                case 0: Descend51(); break;
                case 1: OpenAltar53(); break;
                case 2: SinOpen737(); break; // t770u: Codex row opens the Infernal Sins collection
                case 3: Msg51("SETTINGS: COMING SOON"); break;
                case 4: Application.Quit(); break;
                case 5: _press69 = 0; HellordAudio.Play("seal_press"); break; // v0.0.61 t611u // soft press brighten, then the Pact panel grows from the seal
            }
        }

        // ---------------- draw (700x392 art space) ----------------
        void DrawMainMenu69()
        {
            float ms = Time.unscaledTime * 1000f; int f = (int)(ms / PT.mm69FrameMs) % NF69;
            float p = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * (ms / PT.mm69FrameMs) / 16f);
            var e = Event.current; bool moved = (e.mousePosition - _lastMouse69).sqrMagnitude > 0.5f; if (e.type == EventType.Repaint || e.type == EventType.MouseMove) { if (moved && _lastMouse69.x > -9) MouseHover69(); _lastMouse69 = e.mousePosition; }
            var bt = T69("base_" + f.ToString("00")); if (bt) GUI.DrawTexture(new Rect(0, 0, 700, 392), bt);
            var rows = T69("rows_" + _row69);
            if (rows) { const int cw = 175, ch = 157, cols = 11; int cx = f % cols, cy = f / cols; GUI.DrawTextureWithTexCoords(new Rect(30, 112, cw, ch), rows, new Rect(cx * cw / (float)rows.width, 1 - (cy + 1) * ch / (float)rows.height, cw / (float)rows.width, ch / (float)rows.height)); }
            // currency, top right (live)
            string cur = "SOULS " + _souls51.ToString("N0") + "   SIGILS " + _sigils51; Tx69(cur, 686 - TW69(cur), 8, U_SAND);
            // live sockets + plate
            int n = SelCount51();
            TxC69("PACT " + n + "/" + MaxSel51(), 613, 213, C69_RED); // plate centre y 217
            // seal hover: ~18% brighten masked to the seal + soft red edge glow, pulsing; press = short extra brighten
            float k = Mathf.SmoothStep(0, 1, _hov69), flash = 0;
            if (_press69 >= 0) { float u = _press69 / PT.mm69PressS; flash = Mathf.Lerp(0.35f, 0f, u); k = Mathf.Max(k, 1); }
            if (k > 0 || flash > 0)
            {
                I69("seal_glow", 525, 96, k * (0.55f + 0.2f * p) / 0.75f);
                I69("seal_hi", 525, 96, k * (0.17f + 0.04f * p) / 0.19f * PT.mm69HoverBright / 0.18f);
                if (flash > 0) { GUI.color = new Color(1, 1, 1, flash); var hi = T69("seal_hi"); if (hi) GUI.DrawTexture(new Rect(525, 96, hi.width, hi.height), hi); GUI.color = Color.white; }
            }
            // t581u: live ring, drawn AFTER the seal hover so signed icons are never washed white.
            // Orb k = the k-th signed Pact node of your Path (bit order): flame = Notable, crown = Keystone, dark = empty. Signed #7/#8 = amber pips by the skull.
            int hovS = _panel69 == 0 ? SockOver69() : -1;
            for (int i = 0; i < Sock69.Length; i++)
            {
                int ni = i < n && _pactPath51 >= 0 ? SelNode69(i) : -1;
                I69(ni < 0 ? "sock_off" : PactData.Nodes[_pactPath51][ni].keystone ? "sock_on_1" : "sock_on_0", Sock69[i].x - 12, Sock69[i].y - 12);
                if (i == hovS) { var a = new Color(0xf7 / 255f, 0x76 / 255f, 0x22 / 255f, 0.85f); float bx = Sock69[i].x - 13, by = Sock69[i].y - 13; R53(bx, by, 5, 1, a); R53(bx, by, 1, 5, a); R53(bx + 21, by, 5, 1, a); R53(bx + 25, by, 1, 5, a); R53(bx, by + 25, 5, 1, a); R53(bx, by + 21, 1, 5, a); R53(bx + 21, by + 25, 5, 1, a); R53(bx + 25, by + 21, 1, 5, a); }
            }
            for (int q = 6; q < MaxSel51(); q++) R53(q == 6 ? 600 : 622, 112, 4, 4, q < n ? U_AMB : U_INK);
            // socket tooltip only (no leader line, never opens the panel)
            int so = _panel69 == 0 ? SockOver69() : -1;
            if (so >= 0)
            {
                float tx = 549 - 154, ty = 96 - 8; R53(tx, ty, 150, 58, U_INK); Ol53(tx, ty, 150, 58, U_DRED);
                int ni = so < n ? SelNode69(so) : -1;
                if (ni >= 0 && _pactPath51 >= 0) { var nd = PactData.Nodes[_pactPath51][ni]; Tx69(Fit69(nd.name, 140), tx + 5, ty + 5, U_AMB); WrapTx69(nd.desc, tx + 5, ty + 17, 140, 3, U_SAND); }
                else { Tx69("EMPTY SOCKET", tx + 5, ty + 5, U_GREY); WrapTx69("SEAL A PACT NODE AT THE SACRIFICE ALTAR", tx + 5, ty + 17, 140, 3, U_SAND); }
            }
            if (_sinPanel737) { DrawSinPanel737(); return; } // t770u Infernal Sins panel (modal)
            if (_panel69 == 0 && _press69 < 0 && SinBtn737(10, 8, 110, 14, "INFERNAL SINS", true)) { SinOpen737(); return; }
            // mouse clicks
            if (_panel69 == 0 && _press69 < 0)
            {
                for (int i = 0; i < 5; i++) if (Click53(30, RowY69[_row69][i] - 4, 200, 22)) { _sel69 = _row69 = i; Act69(i); return; }
                if (so < 0 && Click53(549, 108, 128, 140)) { _sel69 = 5; Act69(5); }
            }
            if (_panelU69 > 0) DrawPactPanel69();
            if (_altarMsgT51 > 0 && !string.IsNullOrEmpty(_altarMsg51)) { float w = TW69(_altarMsg51) + 12; R53(350 - w / 2, 360, w, 14, U_INK); Ol53(350 - w / 2, 360, w, 14, U_DRED); TxC69(_altarMsg51, 350, 363, U_YEL); }
        }
        void MouseHover69()
        {
            if (_panel69 != 0 || _press69 >= 0) return;
            for (int i = 0; i < 5; i++) if (Over51(30, RowY69[_row69][i] - 4, 200, 22)) { _sel69 = _row69 = i; return; }
            if (SealOver69()) _sel69 = 5; else if (_sel69 == 5) _sel69 = _row69; // hover leaves the seal: highlight fades out
        }
        string Fit69(string s, float w) { s = Sanitize(s); int m = Mathf.FloorToInt((w * K69 + 1) / 6); return s.Length > m ? s.Substring(0, Mathf.Max(0, m - 1)) + "." : s; }
        void WrapTx69(string s, float x, float y, float w, int maxL, Color c)
        {
            int m = Mathf.FloorToInt((w * K69 + 1) / 6); var words = Sanitize(s ?? "").Split(' '); string line = ""; int l = 0;
            foreach (var wd in words) { var t = line.Length == 0 ? wd : line + " " + wd; if (t.Length > m && line.Length > 0) { Tx69(l == maxL - 1 ? Fit69(line + "...", w) : line, x, y + l * 11, c); if (++l >= maxL) return; line = wd; } else line = t; }
            if (line.Length > 0) Tx69(line, x, y + l * 11, c);
        }
        // PLACEHOLDER Pact panel (VH remakes the interior later): screen dims, panel grows from the seal, [A] altar, [B] back
        void DrawPactPanel69()
        {
            float u = _panelU69, eu = 1 - (1 - u) * (1 - u) * (1 - u);
            R53(0, 0, 700, 392, new Color(0x18 / 255f, 0x14 / 255f, 0x25 / 255f, 170 / 255f * u));
            float s = Mathf.Lerp(0.08f, 1, eu); var c = Vector2.Lerp(new Vector2(613, 172), new Vector2(350, 200), eu);
            float w = 420 * s, h = 272 * s, x = c.x - w / 2, y = c.y - h / 2;
            R53(x, y, w, h, U_PANEL); Ol53(x, y, w, h, U_DRED, 2); if (u < 1) return;
            TxC69("YOUR DAMNATION PACT", 350, y + 12, U_AMB);
            int n = SelCount51();
            if (_pactPath51 >= 0) TxC69(PactData.PathShort[_pactPath51], 350, y + 28, PactData.PathCol[_pactPath51]);
            TxC69("SEALED " + n + "/" + MaxSel51(), 350, y + 42, C69_RED);
            if (n == 0) TxC69("NO PACT SEALED YET", 350, y + 80, U_GREY);
            for (int k = 0; k < n; k++) { int ni = SelNode69(k); if (ni < 0) break; var nd = PactData.Nodes[_pactPath51][ni]; Tx69((nd.keystone ? "* " : "- ") + Fit69(nd.name, 380), x + 24, y + 62 + k * 14, nd.keystone ? U_YEL : U_SAND); }
            TxC69("PLACEHOLDER PANEL", 350, y + h - 44, U_SLATE);
            string a = "[A] OPEN SACRIFICE ALTAR", b = "[B] BACK"; float ay = y + h - 24, ax = 350 - (TW69(a) + 30 + TW69(b)) / 2, bx = ax + TW69(a) + 30;
            Tx69(a, ax, ay, Over51(ax - 3, ay - 3, TW69(a) + 6, 14) ? U_YEL : U_SAND); Tx69(b, bx, ay, Over51(bx - 3, ay - 3, TW69(b) + 6, 14) ? U_YEL : U_SAND);
            if (Click53(ax - 3, ay - 3, TW69(a) + 6, 14)) { _panel69 = 0; _panelU69 = 0; OpenAltar53(); return; }
            if (Click53(bx - 3, ay - 3, TW69(b) + 6, 14) || (Hit51(0, 0, 700, 392) && !new Rect(x, y, w, h).Contains(Event.current.mousePosition))) { _panel69 = 2; if (Event.current.type == EventType.MouseDown) Event.current.Use(); }
        }
    }
}
