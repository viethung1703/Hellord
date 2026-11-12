using UnityEngine;

// t511: VH-approved Sacrifice Altar art (Art/previews/step36_main_menu/v97_sacrifice_all4_paths_review + v95 links + v98 flow).
// Overview = 3-card Path carousel (PixelLab evil busts), Path screen = tier-column node tree with 48px PixelLab icons in 72px (1400-scale) nodes.
// Drawn in the main menu's 700x392 art space. Art baked by tools/altar97_bake.py into Resources/HellordArt/Altar97.
// Rules are the live Pact rules (TryUnlock51 / TrySelect51): one Path per Pact, unlock before select, 6 max, 1 Keystone max, Crest at 4+.
// The YOUR DAMNATION PACT / SEAL PACT screen (_altarScr53 == 2) is still the v0.0.53 one.
// t1179 (VH "the pretty double pacts in one ... choosing pact avatar rolling"): restored to the v0.0.95 look (before step91 stone restyle).
namespace Hellord
{
    public partial class HellordGame
    {
        // (col,row) per node N1..N8,K1,K2. col 4 = KEYSTONE column. Rows 0..2. Hand-placed for short orthogonal links.
        static readonly int[][] Lay97 = {
            new[] { 0,1, 1,0, 1,2, 2,0, 2,2, 2,1, 3,2, 3,1, 4,2, 4,0 }, // Pyromancer: Inferno Heart <- Overheat straight on row 0
            new[] { 0,1, 1,0, 1,2, 2,1, 2,0, 2,2, 3,0, 4,1, 4,2,   0,1, 1,0, 1,2, 2,1, 2,0, 2,2, 3,1, 4,0, 4,2 }, // t539u: Necromancer now slot 1. t536u Necromancer 18 nodes: group A (SUMMON) idx 0..8, group B (BONE SKILLS) idx 9..17, shown one group at a time
            new[] { 0,1, 1,0, 1,1, 1,2, 2,1, 2,0, 2,2, 3,0, 4,0, 4,2 }, // Stormcaller: Elemental Equilibrium <- Static Fork via lane
            new[] { 0,1, 1,0, 1,2, 2,2, 1,1, 2,1, 3,2, 3,1, 4,2, 4,0 }, // Blood Reaver
        };
        static readonly string[] Sub97 = { "OF THE PIT", "", "", "" };
        static readonly Color A_LEATH = new Color32(46, 30, 34, 255), A_STONE = new Color32(34, 30, 44, 255), A_EDGE = new Color32(0x73, 0x3e, 0x39, 255), A_DEEP = new Color32(20, 16, 28, 255);
        int _grp97; // t536u: visible node group on 18-node Paths (0 SUMMON, 1 BONE SKILLS)
        static bool Big97(int p) => PactData.Nodes[p].Length > 10;
        int G097(int p) => Big97(p) ? _grp97 * 9 : 0; int GN97(int p) => Big97(p) ? 9 : PactData.Nodes[p].Length;
        static readonly string[] Grp97 = { "SUMMON", "BONE SKILLS" };
        void SetGrp97(int p, int g) { _grp97 = g; _focus97 = G097(p); }
        int _focus97 = -1, _confirm97 = -1; bool _confirmChoose97; float _slide97;
        readonly System.Collections.Generic.Dictionary<string, Texture2D> _tex97 = new System.Collections.Generic.Dictionary<string, Texture2D>();
        Texture2D T97(string n) { if (_tex97.TryGetValue(n, out var t)) return t; t = Resources.Load<Texture2D>("HellordArt/Altar97/" + n); if (t) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } _tex97[n] = t; return t; }
        void I97(Texture2D t, float x, float y, float w, float h, float a = 1) { if (!t) return; GUI.color = new Color(1, 1, 1, a); GUI.DrawTexture(new Rect(x, y, w, h), t); GUI.color = Color.white; }
        static float TW97(string s, int sc = 1) => (Sanitize(s).Length * 6 * sc - sc) / K69;
        void Tx97(string s, float x, float y, Color c, int sc = 1) { var m = GUI.matrix; GUI.matrix = _base53; Text(Sanitize(s), Mathf.Round(x * K69), Mathf.Round(y * K69), c, true, sc); GUI.matrix = m; }
        void TxC97(string s, float cx, float y, Color c, int sc = 1) => Tx97(s, cx - TW97(s, sc) / 2, y, c, sc);
        bool PathLocked97(int p) { for (int i = 0; i < PactData.Nodes[p].Length; i++) if (Unlocked51(p, i)) return false; return true; }
        int Owned97(int p) { int c = 0; for (int i = 0; i < PactData.Nodes[p].Length; i++) if (Unlocked51(p, i)) c++; return c; }
        bool Taken97(int p, int i) => _pactPath51 == p && (_pactSel51 & Bit(i)) != 0;
        static int NP97 => PactData.Nodes.Length; // t525u: 5 Paths
        static float NX97(int c) => 258 + c * 76;
        static float NY97(int r) => 106 + r * 62;

        void Frame97(float x, float y, float w, float h, Color fill, Color edge, int t = 2)
        { R53(x - 1, y - 1, w + 2, h + 2, A_DEEP); R53(x, y, w, h, edge); R53(x + t, y + t, w - 2 * t, h - 2 * t, fill); R53(x + t, y + t, w - 2 * t, 1, new Color(1, 1, 1, 0.05f)); }
        bool Btn97(float x, float y, float w, string s, bool en = true, string key = null)
        {
            bool ov = en && Over51(x, y, w, 20) && _confirm97 < 0;
            Frame97(x, y, w, 20, ov ? new Color32(92, 40, 48, 255) : en ? A_LEATH : A_STONE, ov ? U_AMB : en ? A_EDGE : U_SLATE);
            float kx = x + 6; if (key != null) { R53(kx, y + 4, 12, 12, en ? U_SAND : U_SLATE); TxC97(key, kx + 6, y + 6, U_INK); }
            TxC97(s, x + w / 2 + (key != null ? 8 : 0), y + 7, en ? (ov ? U_YEL : U_SAND) : U_GREY);
            if (_confirm97 >= 0 || !Hit51(x, y, w, 20)) return false; if (Event.current.type == EventType.MouseDown) Event.current.Use(); if (!en) return false; return true;
        }
        bool BtnHit97(float x, float y, float w) => _confirm97 < 0 && Over51(x, y, w, 20);

        void DrawAltar97()
        {
            if (_altarScr53 == 1 && _altarView51 == (int)PactPath.Necromancer)
            {   // t493u/t535u: Necromancer node screen = approved v130b mock-up (HellordAltar130.cs); the old tree/strip/plates are not drawn
                bool md = _confirm97 >= 0; _block53 = md; if (PmActive()) DrawNecroMenu(_altarView51); else DrawNecro130(_altarView51); _block53 = false;   // t1350: menu theme = mockup t1345_B (HellordPactMenu.cs); Stone1246 keeps the v130b screen
                if (_altarMsgT51 > 0 && !string.IsNullOrEmpty(_altarMsg51)) { float w = TW97(_altarMsg51) + 14; Frame97(350 - w / 2, 370, w, 14, A_DEEP, U_AMB, 1); TxC97(_altarMsg51, 350, 374, U_YEL); }
                if (md) DrawConfirm97(); return;
            }
            var bg = T53("altar_bg", true); if (bg) GUI.DrawTexture(new Rect(0, 0, 700, 392), bg);
            R53(0, 0, 700, 392, new Color(0.04f, 0.02f, 0.06f, 0.72f));
            Frame97(64, 12, 572, 368, new Color32(26, 20, 32, 245), A_EDGE, 2); Ol53(68, 16, 564, 360, new Color32(62, 39, 49, 255));
            // currency plates
            Frame97(80, 22, 120, 20, A_DEEP, A_EDGE, 1); I97(T53("soul"), 84, 25, 14, 14); Tx97("SOULS " + _souls51.ToString("N0"), 102, 29, U_AMB);
            Frame97(500, 22, 120, 20, A_DEEP, A_EDGE, 1); I97(T53("sigil"), 504, 25, 14, 14); Tx97("SIGILS " + _sigils51, 522, 29, new Color32(181, 80, 136, 255));
            string title = _altarScr53 == 1 ? PactData.PathShort[_altarView51] + " NODES" : "SACRIFICE ALTAR";
            float tw = TW97(title, 2) + 24; Frame97(350 - tw / 2, 18, tw, 26, new Color32(38, 34, 54, 255), A_EDGE, 2); TxC97(title, 350, 25, U_SAND, 2);
            // YOUR PATH strip
            { float sx = 190, sy = 49, sw = 320; R53(sx, sy, sw, 13, A_DEEP); Ol53(sx, sy, sw, 13, U_DRED);
                int yp = _pactPath51; string a = "YOUR PATH: " + (yp >= 0 ? PactData.PathShort[yp] : "NONE");
                Tx97(a, sx + 6, sy + 3, yp >= 0 ? PactData.PathCol[yp] : U_GREY); float px = sx + 178; Tx97("NODES", px, sy + 3, U_GREY);
                for (int k = 0; k < MaxSel51(); k++) { float bx = px + 38 + k * 8; R53(bx, sy + 3, 6, 7, k < SelCount51() ? U_AMB : U_SLATE); if (k >= SelCount51()) R53(bx + 1, sy + 4, 4, 5, A_DEEP); }
                Tx97(SelCount51() + "/" + MaxSel51(), px + 42 + MaxSel51() * 8, sy + 3, U_SAND); }
            bool modal = _confirm97 >= 0; _block53 = modal;
            if (_altarScr53 == 1) DrawNodes97(); else DrawCarousel97();
            _block53 = false;
            if (_altarMsgT51 > 0 && !string.IsNullOrEmpty(_altarMsg51)) { float w = TW97(_altarMsg51) + 14; Frame97(350 - w / 2, 334, w, 14, A_DEEP, U_AMB, 1); TxC97(_altarMsg51, 350, 338, U_YEL); }
            if (modal) DrawConfirm97();
        }

        // ---------------- overview: Path carousel ----------------
        void DrawCarousel97()
        {
            int v = _altarView51, l = (v + NP97 - 1) % NP97, r = (v + 1) % NP97;
            _slide97 = Mathf.MoveTowards(_slide97, 0, Time.unscaledDeltaTime * 6);
            float off = _slide97 * 30;
            Card97(l, 112 - off * 0.5f, 110, 124, 158, false);
            Card97(r, 464 - off * 0.5f, 110, 124, 158, false);
            Card97(v, 262 + off, 76, 176, 222, true);
            // arrows
            for (int s = 0; s < 2; s++)
            {
                float ax = s == 0 ? 84 : 596, ay = 172; bool ov = Over51(ax, ay, 20, 30) && !_block53;
                Frame97(ax, ay, 20, 30, ov ? new Color32(92, 40, 48, 255) : A_LEATH, ov ? U_AMB : A_EDGE, 1);
                for (int k = 0; k < 7; k++) { float h = 14 - k * 2; R53(s == 0 ? ax + 14 - k : ax + 6 + k, ay + 15 - h / 2, 1, h, ov ? U_YEL : U_AMB); }
                if (Click53(ax, ay, 20, 30)) Scroll97(s == 0 ? -1 : 1);
            }
            if (Click53(112, 110, 124, 158)) Scroll97(-1); else if (Click53(464, 110, 124, 158)) Scroll97(1);
            else if (Click53(262, 76, 176, 222)) { _altarScr53 = 1; _focus97 = -1; }
            // page dots
            for (int p = 0; p < NP97; p++) R53(312 + p * 12, 304, 6, 4, p == v ? U_AMB : U_SLATE); Tx97((v + 1) + "/" + NP97, 376, 302, U_SAND);
            R53(160, 314, 380, 13, A_DEEP); Ol53(160, 314, 380, 13, A_EDGE);
            TxC97("CREST AT " + PT.pactCrestAt51 + "+ NODES FROM ONE PATH. ONE PATH PER PACT", 350, 317, U_SAND);
            if (Btn97(84, 350, 110, "OPEN", true, "A")) { _altarScr53 = 1; _focus97 = -1; }
            if (Btn97(290, 350, 120, "YOUR PACT", true, "Y")) _altarScr53 = 2;
            if (Btn97(506, 350, 110, "BACK", true, "B")) mode = Mode.MainMenu;
        }
        void Scroll97(int d) { _altarView51 = (_altarView51 + d + NP97) % NP97; _slide97 = d; }

        // t526: Bone Warden v107 whole-bust portrait. Focused + unlocked = PixelLab idle loop (jaw locked shut); otherwise static base. Other Paths unchanged.
        string Av97(int p, bool locked, bool focused)
        {
            if (locked) return "avatar_" + p + "_lock";
            int nf = p == (int)PactPath.Necromancer ? 8 : 6; // t539u: Necromancer uses the v107 bone bust (8 frames, slot 1 art); Pyro/Storm/Blood v103 = 6
            if (focused && nf > 0 && PT.boneIdleFrameMs97 > 0) return "avatar_" + p + "_idle" + ((int)(Time.unscaledTime * 1000f / PT.boneIdleFrameMs97) % nf);
            return "avatar_" + p;
        }
        void Card97(int p, float x, float y, float w, float h, bool centre)
        {
            bool yours = _pactPath51 == p, locked = PathLocked97(p); Color pc = PactData.PathCol[p];
            float a = centre ? 1 : 0.55f;
            Frame97(x, y, w, h, centre ? new Color32(52, 34, 38, 255) : new Color32(34, 30, 44, 255), centre ? (yours ? (Color)new Color32(0xbe, 0x4a, 0x2f, 255) : A_EDGE) : locked ? U_SLATE : A_EDGE, centre ? 3 : 2);
            float ps = centre ? 128 : 96, px = x + (w - ps) / 2, py = y + (centre ? 18 : 10);
            R53(px - 2, py - 2, ps + 4, ps + 4, A_DEEP); R53(px, py, ps, ps, centre ? new Color32(70, 40, 44, 255) : new Color32(44, 40, 58, 255));
            I97(T97(Av97(p, locked, centre)), px, py, ps, ps, centre ? 1 : 0.8f);
            if (locked) I97(T53("lock"), px + ps - 18, py + ps - 18, 14, 14);
            float ny = py + ps + 6;
            if (centre)
            {
                Frame97(x + 10, ny, w - 20, 18, A_DEEP, A_EDGE, 1); TxC97(PactData.PathShort[p], x + w / 2, ny + 5, locked ? U_GREY : U_SAND);
                if (Sub97[p] != "") TxC97(Sub97[p], x + w / 2, ny + 22, U_GREY);
                float dy = ny + 34; int nN = PactData.Nodes[p].Length; float dsp = nN > 10 ? 5.6f : 10; for (int i = 0; i < nN; i++) { float bx = x + w / 2 - dsp * nN / 2 + i * dsp + (PactData.Nodes[p][i].keystone ? 2 : 0) + (nN > 10 && i >= 9 ? 3 : 0); bool t = Taken97(p, i), u = Unlocked51(p, i); R53(bx, dy, 7, 7, t ? U_AMB : u ? A_EDGE : U_SLATE); R53(bx + 2, dy + 2, 3, 3, t ? U_YEL : u ? (Color)pc : A_DEEP); }
                TxC97(yours ? SelCount51() + "/" + MaxSel51() + " IN PACT" : locked ? "LOCKED" : Owned97(p) + "/" + PactData.Nodes[p].Length + " OWNED", x + w / 2, dy + 12, yours ? U_AMB : U_GREY);
            }
            else
            {
                R53(x + 8, ny, w - 16, 14, A_DEEP); TxC97(PactData.PathShort[p], x + w / 2, ny + 4, new Color(U_SAND.r, U_SAND.g, U_SAND.b, a));
                TxC97(yours ? "YOUR PATH" : locked ? "LOCKED" : Owned97(p) + "/" + PactData.Nodes[p].Length + " OWNED", x + w / 2, ny + 19, yours ? U_AMB : U_GREY);
                if (!yours && !locked) TxC97("NOT CHOSEN", x + w / 2, y + 2, U_GREY);
                R53(x, y, w, h, new Color(0.03f, 0.02f, 0.05f, 0.25f)); // neighbours dimmer
            }
            if (yours) Ribbon97(x + w / 2, y - 6, centre ? w + 20 : w - 8);
        }
        void Ribbon97(float cx, float y, float w)
        {
            R53(cx - w / 2 - 4, y + 3, 6, 12, new Color32(0x73, 0x17, 0x2d, 255)); R53(cx + w / 2 - 2, y + 3, 6, 12, new Color32(0x73, 0x17, 0x2d, 255));
            R53(cx - w / 2, y, w, 14, U_DRED); R53(cx - w / 2, y + 13, w, 1, new Color32(0x73, 0x17, 0x2d, 255)); TxC97("YOUR PATH", cx, y + 4, U_SAND);
        }

        // ---------------- Path nodes panel ----------------
        void DrawNodes97()
        {
            int p = _altarView51; var nodes = PactData.Nodes[p]; Color pc = PactData.PathCol[p]; var L = Lay97[p];
            bool yours = _pactPath51 == p, locked = PathLocked97(p);
            if (_focus97 < 0) { _focus97 = G097(p); for (int i = G097(p); i < G097(p) + GN97(p); i++) if (Unlocked51(p, i) && !Taken97(p, i)) { _focus97 = i; break; } }
            if (_focus97 >= nodes.Length) _focus97 = 0; if (Big97(p)) _grp97 = _focus97 / 9; if (Big97(p) && _focus97 < 0) _focus97 = 0; int g0 = G097(p), gN = GN97(p);
            // left portrait card
            Frame97(80, 72, 128, 152, new Color32(52, 34, 38, 255), yours ? (Color)new Color32(0xbe, 0x4a, 0x2f, 255) : locked ? U_SLATE : A_EDGE, 3);
            R53(94, 84, 100, 100, A_DEEP); I97(T97(Av97(p, locked, true)), 96, 86, 96, 96);
            Frame97(88, 190, 112, 14, A_DEEP, A_EDGE, 1); TxC97(PactData.PathShort[p], 144, 194, locked ? U_GREY : U_SAND);
            TxC97(yours ? SelCount51() + "/" + MaxSel51() + " IN PACT" : locked ? "LOCKED" : Owned97(p) + "/" + PactData.Nodes[p].Length + " OWNED", 144, 210, yours ? U_AMB : U_GREY);
            if (yours) Ribbon97(144, 66, 120);
            Frame97(80, 230, 128, 40, A_DEEP, U_DRED, 1);
            TxC97(yours ? "TAKEN " + SelCount51() + "/" + MaxSel51() : locked ? "PATH LOCKED" : "NOT YOUR PATH", 144, 238, yours ? U_AMB : locked ? U_GREY : U_SAND);
            TxC97(yours ? "YOUR PATH" : locked ? "UNLOCK " + nodes[0].name : _pactPath51 >= 0 ? "PACT: " + PactData.PathShort[_pactPath51] : "NO PATH CHOSEN", 144, 252, U_GREY);
            if (Big97(p)) Tree115(p, nodes, pc); else {
            // tree box
            Frame97(216, 64, 404, 210, new Color32(24, 22, 36, 255), A_EDGE, 1);
            string[] tiers = { "TIER I", "TIER II", "TIER III", "TIER IV", "KEYSTONE" };
            for (int c = 0; c < 5; c++) TxC97(tiers[c], NX97(c), 70, U_GREY);
            if (Big97(p)) for (int g = 0; g < 2; g++)
            {   // t536u group toggle (also Tab)
                float bx = 216 + g * 92, by = 50, bw = 88; bool on = _grp97 == g;
                R53(bx, by, bw, 13, on ? (Color)pc : A_DEEP); Ol53(bx, by, bw, 13, on ? Color.white : A_EDGE, 1);
                TxC97((g == 0 ? "A " : "B ") + Grp97[g], bx + bw / 2, by + 3, on ? U_INK : U_GREY);
                if (Click53(bx, by, bw, 13) && !on) SetGrp97(p, g);
            }
            if (Big97(p)) TxC97("TAB: SWITCH GROUP", 470, 54, U_GREY);
            // links first (under nodes)
            for (int i = g0; i < g0 + gN; i++)
            {
                string req = nodes[i].req; if (string.IsNullOrEmpty(req)) continue;
                int cc = L[i * 2], cr = L[i * 2 + 1];
                if (AnyReq51(req))
                {   // keystone needing ANY 2 nodes: dashed stub + label, no fake parent line
                    float sy = NY97(cr), sx = NX97(cc) - 24; Color dc = ReqMet51(p, nodes[i]) ? A_EDGE : U_SLATE;
                    for (int k = 0; k < 4; k++) R53(sx - 22 + k * 6, sy - 1, 4, 2, dc); R53(sx - 26, sy - 3, 4, 6, dc);
                    continue;
                }
                foreach (var rq in req.Split('/'))
                {
                    int pi = NodeIx51(p, rq); if (pi < 0) continue; int pcol = L[pi * 2], prow = L[pi * 2 + 1];
                    bool on = Taken97(p, pi) && Taken97(p, i), un = Unlocked51(p, pi) && Unlocked51(p, i);
                    Link97(L, pcol, prow, cc, cr, nodes[i].keystone, on ? (Color)pc : un ? (Color)new Color32(0x9e, 0x5a, 0x45, 255) : (Color)new Color32(0x4a, 0x50, 0x6a, 255));
                }
            }
            for (int i = g0; i < g0 + gN; i++)
            {
                float cx = NX97(L[i * 2]), cy = NY97(L[i * 2 + 1]); bool key = nodes[i].keystone, un = Unlocked51(p, i), tk = Taken97(p, i), avail = !un && ReqMet51(p, nodes[i]);
                Color edge = tk ? pc : un ? (Color)new Color32(0x9e, 0x5a, 0x45, 255) : avail ? (Color)new Color32(0x73, 0x3e, 0x39, 255) : (Color)new Color32(0x4a, 0x50, 0x6a, 255);
                if (key)
                {
                    var m = GUI.matrix; GUI.matrix = m * Matrix4x4.TRS(new Vector3(cx, cy, 0), Quaternion.Euler(0, 0, 45), Vector3.one) * Matrix4x4.Translate(new Vector3(-cx, -cy, 0));
                    R53(cx - 17, cy - 17, 34, 34, A_DEEP); R53(cx - 16, cy - 16, 32, 32, edge); R53(cx - 13, cy - 13, 26, 26, un ? (Color)new Color32(40, 26, 30, 255) : (Color)new Color32(30, 30, 44, 255));
                    if (_focus97 == i) Ol53(cx - 20, cy - 20, 40, 40, Color.white);
                    GUI.matrix = m;
                    I97(T97("icon_" + (p == (int)PactPath.Necromancer ? 4 : p) + "_" + i + (un ? "" : "_dim")), cx - 12, cy - 12, 24, 24);
                }
                else
                {
                    R53(cx - 19, cy - 19, 38, 38, A_DEEP); R53(cx - 18, cy - 18, 36, 36, edge); R53(cx - 15, cy - 15, 30, 30, un ? (Color)new Color32(40, 26, 30, 255) : (Color)new Color32(30, 30, 44, 255));
                    I97(T97("icon_" + (p == (int)PactPath.Necromancer ? 4 : p) + "_" + i + (un ? "" : "_dim")), cx - 12, cy - 12, 24, 24);
                }
                if (_focus97 == i && !key) { R53(cx - 22, cy - 22, 6, 2, Color.white); R53(cx - 22, cy - 22, 2, 6, Color.white); R53(cx + 16, cy - 22, 6, 2, Color.white); R53(cx + 20, cy - 22, 2, 6, Color.white); R53(cx - 22, cy + 20, 6, 2, Color.white); R53(cx - 22, cy + 16, 2, 6, Color.white); R53(cx + 16, cy + 20, 6, 2, Color.white); R53(cx + 20, cy + 16, 2, 6, Color.white); }
                if (tk) R53(cx + 12, cy - 18, 6, 6, U_YEL);
                if (!un) I97(T53("lock"), cx + 9, cy - 22, 10, 10);
                // label (2 lines max)
                string nm = nodes[i].name; string l1 = nm, l2 = ""; int sp = nm.Length > 10 ? nm.LastIndexOf(' ', Mathf.Min(nm.Length - 1, 11)) : -1; if (sp > 0) { l1 = nm.Substring(0, sp); l2 = nm.Substring(sp + 1); }
                float ly = cy + (key ? 27 : 21); Color lc = tk ? U_YEL : un ? U_SAND : U_GREY;
                TxC97(l1, cx, ly, lc); if (l2 != "") TxC97(l2, cx, ly + 8, lc);
                if (AnyReq51(nodes[i].req)) TxC97(nodes[i].req[1] == 'N' ? "ANY 2 NODES" : "ANY 2 " + nodes[i].req[1] + " NODES", cx, cy - 37, ReqMet51(p, nodes[i]) ? U_AMB : U_GREY);
                if (Click53(cx - 20, cy - 20, 40, 40)) _focus97 = i;
            }
            }
            // description panel
            int f = Mathf.Clamp(_focus97, 0, nodes.Length - 1); var n = nodes[f]; bool fu = Unlocked51(p, f), fs = Taken97(p, f), reqOk = ReqMet51(p, n);
            Frame97(80, 278, 540, 66, A_DEEP, pc, 1);
            Tx97(n.name, 88, 284, U_SAND, 2);
            string st = fs ? "TAKEN" : fu ? "UNLOCKED" : reqOk ? "AVAILABLE" : "LOCKED"; Tx97(st, 612 - TW97(st), 286, fs ? U_YEL : fu ? U_AMB : U_GREY);
            WrapTx69(n.desc, 88, 302, 520, 2, U_SAND);
            int cost = NodeCost51(n);
            string l3 = fu ? (n.live ? (n.partial ? "LIVE: PARTIAL" : "LIVE IN GAME") : "NOT IN GAME YET") : (n.req != "" ? ReqText51(n) + ".  " : "") + "COST " + cost + " SOULS" + (n.keystone ? " + " + PT.pactKeystoneSigils51 + " SIGILS" : "");
            Tx97(l3, 88, 322, fu ? (n.live ? HCyan : U_GREY) : reqOk ? U_AMB : new Color32(0xe4, 0x3b, 0x44, 255));
            if (n.keystone) Tx97("KEYSTONE: " + (Big97(p) ? PT.necroMaxKeystones549 : PT.pactMaxKeystones51) + " MAX PER PACT", 88, 333, U_GREY);
            // legend
            float gx = 234; R53(gx, 354, 9, 9, pc); Tx97("TAKEN", gx + 13, 356, U_GREY); R53(gx + 52, 354, 9, 9, new Color32(0x9e, 0x5a, 0x45, 255)); Tx97("UNLOCKED", gx + 65, 356, U_GREY); I97(T53("lock"), gx + 128, 353, 10, 10); Tx97("LOCKED", gx + 141, 356, U_GREY);
            // buttons
            string lab; bool en; PrimaryLab97(p, f, out lab, out en);
            if (Btn97(84, 350, 140, lab, en, "A")) Primary97(p, f);
            if (yours) { if (Btn97(470, 350, 74, "PACT", true, "Y")) _altarScr53 = 2; }
            else if (locked) Btn97(440, 350, 104, "LOCKED", false);
            else if (Btn97(440, 350, 104, "CHOOSE PATH", true, "Y")) ChoosePath97(p);
            if (Btn97(550, 350, 66, "BACK", true, "B")) _altarScr53 = 0;
        }


        // ---------------- t544: Necromancer ONE PoE2-style tree (v115 unlock-edges layout; v113 states; v114 lines) ----------------
        // v115 mock coords (1594-wide canvas, B mirrors A around the START hub at x 797), mapped into the 404x210 tree box.
        static readonly Vector2[] V115 = {
            new Vector2(715,384), new Vector2(640,276), new Vector2(640,492), new Vector2(570,384), new Vector2(545,178), new Vector2(545,590), new Vector2(372,384), new Vector2(480,384), new Vector2(705,118),
            new Vector2(879,384), new Vector2(954,276), new Vector2(954,492), new Vector2(1024,384), new Vector2(1049,178), new Vector2(1049,590), new Vector2(1160,530), new Vector2(1114,384), new Vector2(889,118) };
        static Vector2 TM115(Vector2 v) => new Vector2(240 + (v.x - 372) * 0.42f, 82 + (v.y - 118) * 0.35f);
        static Vector2 TP115(int i) => TM115(V115[i]);
        static readonly Color C_LIT115 = new Color32(0xfe, 0xae, 0x34, 255), C_BONE115 = new Color32(214, 170, 128, 255), C_GREY115 = new Color32(96, 84, 104, 255), C_RED115 = new Color32(0xe4, 0x3b, 0x44, 255);
        void Seg115(Vector2 a, Vector2 b, Color c, float w)
        {
            Vector2 d = b - a; float len = d.magnitude; if (len < 1) return; float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            var m = GUI.matrix; GUI.matrix = m * Matrix4x4.TRS(new Vector3(a.x, a.y, 0), Quaternion.Euler(0, 0, ang), Vector3.one);
            R53(0, -w / 2 - 1, len, w + 2, new Color32(12, 10, 18, 255)); R53(0, -w / 2, len, w, c); if (c == C_LIT115) R53(0, -0.5f, len, 1, new Color32(255, 226, 170, 255));
            GUI.matrix = m;
        }
        void Tree115(int p, PactNode[] nodes, Color pc)
        {
            Frame97(216, 46, 404, 228, new Color32(24, 22, 36, 255), A_EDGE, 1);
            TxC97("SUMMON", 250, 52, U_GREY); TxC97("BONE", 590, 52, U_GREY); int kc = _pactPath51 == p ? KeyCount53() : 0; TxC97("PICK " + (_pactPath51 == p ? SelCount51() : 0) + "/" + MaxSel51() + "   KEYSTONE SLOT " + kc + "/" + PT.necroMaxKeystones549, 418, 52, kc > 0 ? U_AMB : U_GREY);
            Vector2 hub = TM115(new Vector2(797, 384));
            // links (under nodes): lit = both taken, bone = parent owned, grey = parent not owned
            for (int pass = 0; pass < 3; pass++)
            {
                for (int s0 = 0; s0 < 2; s0++) { int r0 = s0 * 9; int k = Taken97(p, r0) ? 2 : 1; if (k == pass) Seg115(hub, TP115(r0), k == 2 ? C_LIT115 : C_BONE115, k == 2 ? 3 : 2); }
                for (int i = 0; i < 18; i++)
                {
                    string req = nodes[i].req; if (string.IsNullOrEmpty(req) || AnyReq51(req)) continue;
                    foreach (var rq in req.Split('/'))
                    {
                        int pi = NodeIx51(p, rq); if (pi < 0) continue;
                        int k = Taken97(p, pi) && Taken97(p, i) ? 2 : Unlocked51(p, pi) ? 1 : 0; if (k != pass) continue;
                        Seg115(TP115(pi), TP115(i), k == 2 ? C_LIT115 : k == 1 ? C_BONE115 : C_GREY115, k == 2 ? 3 : 2);
                    }
                }
            }
            // OR tags on two-parent children (A4, A7, B4)
            for (int i = 0; i < 18; i++) if (!nodes[i].keystone && nodes[i].req.IndexOf('/') > 0)
            {
                Vector2 c = TP115(i); float ox = i < 9 ? 20 : -20; R53(c.x + ox - 8, c.y - 5, 16, 10, U_INK); Ol53(c.x + ox - 8, c.y - 5, 16, 10, C_LIT115, 1); TxC97("OR", c.x + ox, c.y - 2, C_LIT115);
            }
            // ANY-2 keystone gates: pill with 7 pips + dashed line to the Keystone (no fake parent)
            for (int g = 0; g < 2; g++)
            {
                int kid = g * 9 + 8; Vector2 kp = TP115(kid), pill = TM115(new Vector2(g == 0 ? 705 : 889, 212)); int n = 0; for (int j = 0; j < 7; j++) if (Unlocked51(p, g * 9 + j)) n++;
                Color col = Taken97(p, kid) ? C_LIT115 : n >= 2 ? C_BONE115 : C_GREY115;
                for (float t = 0; t < 1f; t += 0.25f) { Vector2 a = Vector2.Lerp(new Vector2(pill.x, pill.y - 5), new Vector2(kp.x, kp.y + 14), t), b = Vector2.Lerp(new Vector2(pill.x, pill.y - 5), new Vector2(kp.x, kp.y + 14), t + 0.12f); Seg115(a, b, col, 2); }
                R53(pill.x - 28, pill.y - 5, 56, 11, U_INK); Ol53(pill.x - 28, pill.y - 5, 56, 11, col, 1);
                for (int j = 0; j < 7; j++) R53(pill.x - 24 + j * 7, pill.y - 2, 5, 5, Unlocked51(p, g * 9 + j) ? C_LIT115 : new Color32(40, 34, 50, 255));
                TxC97("ANY 2", pill.x, pill.y + 8, col);
            }
            // START hub
            R53(hub.x - 13, hub.y - 13, 26, 26, A_DEEP); Ol53(hub.x - 13, hub.y - 13, 26, 26, C_BONE115, 1); TxC97("START", hub.x, hub.y - 3, U_SAND);
            // nodes: Taken = lit amber + glow + check, Owned = grey frame, Locked = dark + padlock, Keystone blocked = red
            bool keyTaken = _pactPath51 == p && KeyCount53() >= MaxKeys51();
            for (int i = 0; i < 18; i++)
            {
                Vector2 c = TP115(i); float cx = c.x, cy = c.y; bool key = nodes[i].keystone, un = Unlocked51(p, i), tk = Taken97(p, i), blocked = key && !tk && keyTaken;
                Color edge = tk ? C_LIT115 : blocked ? C_RED115 : un ? new Color32(150, 146, 160, 255) : new Color32(56, 58, 76, 255);
                float h = 13; // half size
                if (tk) { R53(cx - h - 4, cy - h - 4, 2 * h + 8, 2 * h + 8, new Color(1f, 0.68f, 0.2f, 0.25f)); R53(cx - h - 2, cy - h - 2, 2 * h + 4, 2 * h + 4, new Color(1f, 0.68f, 0.2f, 0.45f)); }
                if (key)
                {
                    var m = GUI.matrix; GUI.matrix = m * Matrix4x4.TRS(new Vector3(cx, cy, 0), Quaternion.Euler(0, 0, 45), Vector3.one) * Matrix4x4.Translate(new Vector3(-cx, -cy, 0));
                    R53(cx - 12, cy - 12, 24, 24, A_DEEP); R53(cx - 11, cy - 11, 22, 22, edge); R53(cx - 9, cy - 9, 18, 18, un ? new Color32(40, 26, 30, 255) : new Color32(22, 22, 32, 255));
                    if (_focus97 == i) Ol53(cx - 15, cy - 15, 30, 30, Color.white);
                    GUI.matrix = m;
                }
                else
                {
                    R53(cx - h - 1, cy - h - 1, 2 * h + 2, 2 * h + 2, A_DEEP); R53(cx - h, cy - h, 2 * h, 2 * h, edge); R53(cx - h + 2, cy - h + 2, 2 * h - 4, 2 * h - 4, un ? new Color32(40, 26, 30, 255) : new Color32(22, 22, 32, 255));
                    if (_focus97 == i) Ol53(cx - h - 3, cy - h - 3, 2 * h + 6, 2 * h + 6, Color.white);
                }
                I97(T97("icon_4_" + i + (un ? "" : "_dim")), cx - 10, cy - 10, 20, 20);
                if (tk) { R53(cx + 7, cy - 15, 9, 9, new Color32(0x3e, 0x89, 0x48, 255)); TxC97("V", cx + 11.5f, cy - 13, Color.white); }
                if (!un) I97(T53("lock"), cx - 5, cy - 5, 10, 10);
                if (blocked) { R53(cx - 10, cy - 1, 20, 2, C_RED115); TxC97(KeyMaxLab51, cx, cy - 22, C_RED115); }
                string nm = nodes[i].name; if (nm.Length > 13) { int sp = nm.LastIndexOf(' ', 12); if (sp > 0) nm = nm.Substring(0, sp); }
                Color lc = tk ? U_YEL : blocked ? C_RED115 : un ? U_SAND : U_GREY;
                TxC97(nm, cx, cy + (key ? 17 : 15), lc);
                if (Click53(cx - 14, cy - 14, 28, 28)) _focus97 = i;
            }
        }

        void Link97(int[] L, int c1, int r1, int c2, int r2, bool toKey, Color col)
        {
            float x1 = NX97(c1) + 18, y1 = NY97(r1), x2 = NX97(c2) - (toKey ? 18 : 18), y2 = NY97(r2);
            float g1 = NX97(c1) + 38 + (r1 - 1) * 6, g2 = NX97(c2) - 38 + (r2 - 1) * 6;
            if (c2 == c1 + 1) { HSeg97(x1, g1, y1, col); VSeg97(g1, y1, y2, col); HSeg97(g1, x2, y2, col); return; }
            if (r1 == r2 && RowFree97(L, r2, c1 + 1, c2 - 1)) { HSeg97(x1, x2, y1, col); return; }
            if (RowFree97(L, r2, c1 + 1, c2 - 1)) { HSeg97(x1, g1, y1, col); VSeg97(g1, y1, y2, col); HSeg97(g1, x2, y2, col); return; }
            if (RowFree97(L, r1, c1 + 1, c2 - 1)) { HSeg97(x1, g2, y1, col); VSeg97(g2, y1, y2, col); HSeg97(g2, x2, y2, col); return; }
            // lane between rows (just above the lower row's nodes, below labels)
            int lr = Mathf.Max(r1, r2); if (lr == r1 && lr == r2) lr = Mathf.Min(2, r1 + 1); float ly = NY97(lr) - 25;
            HSeg97(x1, g1, y1, col); VSeg97(g1, y1, ly, col); HSeg97(g1, g2, ly, col); VSeg97(g2, ly, y2, col); HSeg97(g2, x2, y2, col);
        }
        bool RowFree97(int[] L, int r, int ca, int cb) { int g0 = G097(_altarView51), gN = GN97(_altarView51); for (int i = 0; i < L.Length / 2; i++) if (i >= g0 && i < g0 + gN && L[i * 2 + 1] == r && L[i * 2] >= ca && L[i * 2] <= cb) return false; return true; }
        void HSeg97(float a, float b, float y, Color c) { float x = Mathf.Min(a, b), w = Mathf.Abs(b - a) + 2; R53(x - 1, y - 1, w, 2, c); }
        void VSeg97(float x, float a, float b, Color c) { float y = Mathf.Min(a, b), h = Mathf.Abs(b - a) + 2; R53(x - 1, y - 1, 2, h, c); }

        void PrimaryLab97(int p, int f, out string lab, out bool en)
        {
            var n = PactData.Nodes[p][f]; bool fu = Unlocked51(p, f), fs = Taken97(p, f);
            if (!fu) { bool ok = ReqMet51(p, n); lab = ok ? "UNLOCK " + NodeCost51(n) : "LOCKED"; en = ok; return; }
            if (fs) { int rb = p == (int)PactPath.Necromancer ? RemoveBlock130(p, f) : -1; lab = rb >= 0 ? "REMOVE " + PactData.Nodes[p][rb].name + " FIRST" : "REMOVE"; en = rb < 0; return; }
            if (p == (int)PactPath.Necromancer && !SignReq130(p, n)) { lab = "SIGN PARENT FIRST: " + n.req; en = false; return; } // t580u
            if (_pactPath51 == p && SelCount51() >= MaxSel51()) { lab = "PACT FULL"; en = false; return; }
            if (n.keystone && _pactPath51 == p && KeyCount53() >= MaxKeys51()) { lab = KeyMaxLab51; en = false; return; }
            lab = "TAKE NODE"; en = true;
        }
        void Primary97(int p, int f)
        {
            if (!Unlocked51(p, f)) { TryUnlock51(p, f); return; }
            if (_pactPath51 >= 0 && _pactPath51 != p && SelCount51() > 0) { _confirm97 = p; _confirmChoose97 = false; return; }
            if (p == (int)PactPath.Necromancer) { string lab; bool en; PrimaryLab97(p, f, out lab, out en); if (!en) { Msg51(lab); return; } } // t580u: SIGN needs parents signed; REMOVE keeps the path valid
            TrySelect51(p, f);
        }
        void ChoosePath97(int p)
        {
            if (PathLocked97(p)) { Msg51("PATH LOCKED: UNLOCK A NODE FIRST"); return; }
            if (_pactPath51 >= 0 && _pactPath51 != p && SelCount51() > 0) { _confirm97 = p; _confirmChoose97 = true; return; }
            _pactPath51 = p; _pactSel51 = 0; PactSave51(); Msg51("YOUR PATH: " + PactData.PathShort[p]);
        }
        void DrawConfirm97()
        {
            R53(0, 0, 700, 392, new Color(0, 0, 0, 0.6f)); float x = 220, y = 150, w = 260, h = 80; int np = _confirm97;
            Frame97(x, y, w, h, A_LEATH, U_AMB, 2);
            TxC97("SWITCH PACT TO " + PactData.PathShort[np] + "?", 350, y + 14, U_SAND); TxC97("CLEARS " + SelCount51() + " NODES FROM " + PactData.PathShort[_pactPath51], 350, y + 28, U_GREY);
            int c = _confirm97; _confirm97 = -1; // let buttons take clicks
            bool yes = Btn97(x + 24, y + 50, 90, "YES"), no = !yes && Btn97(x + w - 114, y + 50, 90, "NO");
            _confirm97 = c;
            if (yes) ConfirmYes97(); else if (no || (Hit51(0, 0, 700, 392) && !new Rect(x, y, w, h).Contains(Event.current.mousePosition))) _confirm97 = -1;
        }
        void ConfirmYes97() { int np = _confirm97; _confirm97 = -1; _pactPath51 = np; _pactSel51 = 0; PactSave51(); if (!_confirmChoose97 && _altarView51 == np && _focus97 >= 0) TrySelect51(np, _focus97); else Msg51("YOUR PATH: " + PactData.PathShort[np]); }

        // ---------------- keys / pad ----------------
        void AltarTick97()
        {
            bool up = Nav69(0), dn = Nav69(1), lf = Nav69(2), rt = Nav69(3), ok = K69Down(4), back = K69Down(5);
            var mv = HInput.Move(); _navX69 = mv.x; _navY69 = mv.y;
            if (_confirm97 >= 0) { if (ok) ConfirmYes97(); else if (back) _confirm97 = -1; return; }
            if (back) { if (_altarScr53 == 1) _altarScr53 = 0; else mode = Mode.MainMenu; return; }
            if (HInput.Down(HKey.Tab) && _altarScr53 == 0) { _altarScr53 = 2; return; }
            if (_altarScr53 == 0)
            {
                if (lf) Scroll97(-1); else if (rt) Scroll97(1); else if (ok) { _altarScr53 = 1; _focus97 = -1; }
                return;
            }
            int p = _altarView51, f = Mathf.Max(0, _focus97); var L = Lay97[p];
            // t544: one tree, no group Tab
            if (ok) { string lab; bool en; PrimaryLab97(p, f, out lab, out en); if (en) Primary97(p, f); else Msg51(lab); return; }
            int dx = lf ? -1 : rt ? 1 : 0, dy = up ? -1 : dn ? 1 : 0; if (dx == 0 && dy == 0) return;
            int best = -1; float bd = 1e9f;
            if (Big97(p)) { for (int i = 0; i < 18; i++) { if (i == f) continue; Vector2 a = p == (int)PactPath.Necromancer ? NP130(i) : TP115(i), b = p == (int)PactPath.Necromancer ? NP130(f) : TP115(f); float ex = a.x - b.x, ey = a.y - b.y; float along = dx != 0 ? ex * dx : ey * dy, side = dx != 0 ? Mathf.Abs(ey) : Mathf.Abs(ex); if (along <= 0) continue; float d = along + side * 2.5f; if (d < bd) { bd = d; best = i; } } if (best >= 0) _focus97 = best; return; }
            for (int i = G097(p); i < G097(p) + GN97(p); i++)
            {
                if (i == f) continue; float ex = L[i * 2] - L[f * 2], ey = L[i * 2 + 1] - L[f * 2 + 1];
                float along = dx != 0 ? ex * dx : ey * dy, side = dx != 0 ? Mathf.Abs(ey) : Mathf.Abs(ex); if (along <= 0) continue;
                float d = along + side * 2.5f; if (d < bd) { bd = d; best = i; }
            }
            if (best >= 0) _focus97 = best;
        }
    }
}
