using UnityEngine;
using System.Collections.Generic;

// t493u (VH via Hellord Art): Necromancer Pact Altar = v130b MOCK-UP
// (Art/previews/step36_main_menu/v130b_necromancer_currency_top_right_respaced, scripts/cur_130b.py).
// Everything is laid out in the mock's 1280x720 pixel space and scaled to the 700x392 altar space (S130).
// Text uses the mock's own fonts (Pixelify Sans Bold; digits in Press Start 2P) at the mock's pixel sizes,
// drawn in screen space so sizes match the mock 1:1 relative to the frame.
// Static art (frame, header bar, sidebar column, tree panel + rings, silent hub) is baked by tools/necro130_bake.py.
namespace Hellord
{
    public partial class HellordGame
    {
        const float S130 = 700f / 1280f;
        static readonly Color N_INK = new Color32(0x18, 0x14, 0x25, 255), N_NAVY = new Color32(0x26, 0x2b, 0x44, 255), N_SLATE = new Color32(0x3a, 0x44, 0x66, 255),
            N_GREY = new Color32(0x5a, 0x69, 0x88, 255), N_LG = new Color32(0x8b, 0x9b, 0xb4, 255), N_PALE = new Color32(0xc0, 0xcb, 0xdc, 255),
            N_LEA2 = new Color32(0x73, 0x3e, 0x39, 255), N_WARM = new Color32(0xb8, 0x6f, 0x50, 255), N_TAN = new Color32(0xe4, 0xa6, 0x72, 255),
            N_SAND = new Color32(0xea, 0xd4, 0xaa, 255), N_AMB = new Color32(0xfe, 0xae, 0x34, 255), N_RED = new Color32(0xa2, 0x26, 0x33, 255),
            N_BRED = new Color32(0xe4, 0x3b, 0x44, 255), N_GRN = new Color32(0x63, 0xc7, 0x4d, 255), N_SIG = new Color32(181, 80, 136, 255), N_DARK = new Color32(40, 34, 50, 255);
        // v130b node positions (mock px), index order A1..A7, AK, AK2, B1..B7, BK, BK2
        static readonly Vector2[] V130 = {
            new Vector2(753,372), new Vector2(664,246), new Vector2(664,498), new Vector2(636,372), new Vector2(542,131), new Vector2(542,613), new Vector2(394,372), new Vector2(511,372), new Vector2(768,124),
            new Vector2(925,372), new Vector2(1023,246), new Vector2(1023,498), new Vector2(1055,372), new Vector2(1158,131), new Vector2(1158,613), new Vector2(1206,543), new Vector2(1191,372), new Vector2(940,128) };
        static readonly Vector2 HUB130 = new Vector2(835, 372);
        static Vector2 TP130(int i) => V130[i] * S130;
        static readonly string[] DESC130 = { "Raise the dead and command bone.", "Summon minions and empower fire and bone skills.", "Unlock nodes, then select up to 8 (at most 2 Keystones)." };

        Font _pix130, _ps130; GUIStyle _st130;
        void Fonts130()
        {
            if (_st130 != null) return;
            _pix130 = Resources.Load<Font>("Fonts/PixelifySans-Bold"); _ps130 = Resources.Load<Font>("Fonts/PressStart2P-Regular");
            _st130 = new GUIStyle { wordWrap = false, clipping = TextClipping.Overflow, padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0), alignment = TextAnchor.UpperLeft };
        }
        // runs: words in Pixelify, digits in Press Start 2P (as cur_130b txt()); ps = whole string in Press Start 2P
        List<(string s, Font f, int fs)> Runs130(string s, float sz, bool ps, float k)
        {
            var r = new List<(string, Font, int)>(); int fw = Mathf.Max(1, Mathf.RoundToInt(sz * k));
            if (ps) { r.Add((s, _ps130, fw)); return r; }
            int fd = Mathf.Max(1, Mathf.RoundToInt((sz >= 20 ? 16 : 8) * k)); int a = 0;
            for (int i = 1; i <= s.Length; i++)
            {
                bool end = i == s.Length; bool d0 = char.IsDigit(s[a]);
                if (end || char.IsDigit(s[i]) != d0) { r.Add((s.Substring(a, i - a), d0 ? _ps130 : _pix130, d0 ? fd : fw)); a = i; }
            }
            return r;
        }
        float Wid130(string s, float sz, bool ps = false)
        {
            Fonts130(); float k = GUI.matrix.m00 * S130, w = 0; if (k <= 0) return 0;
            foreach (var r in Runs130(s, sz, ps, k)) { _st130.font = r.f; _st130.fontSize = r.fs; w += _st130.CalcSize(new GUIContent(r.s)).x; }
            return w / k; // mock px
        }
        // ah: 0 left, 1 centre, 2 right. mid: vertical centre. outline: INK 1px halo (node labels)
        void MT(string s, float mx, float my, float sz, Color c, int ah = 0, bool ps = false, bool mid = false, bool outline = false)
        {
            if (string.IsNullOrEmpty(s)) return; Fonts130(); if (!_pix130 || !_ps130) { Tx97(s, mx * S130, my * S130, c); return; }
            var m = GUI.matrix; float k = m.m00 * S130; var runs = Runs130(s, sz, ps, k);
            float W = 0, H = 0; var sizes = new List<Vector2>();
            foreach (var r in runs) { _st130.font = r.f; _st130.fontSize = r.fs; var v = _st130.CalcSize(new GUIContent(r.s)); sizes.Add(v); W += v.x; H = Mathf.Max(H, v.y); }
            Vector3 p = m.MultiplyPoint3x4(new Vector3(mx * S130, my * S130, 0));
            float x = p.x - (ah == 1 ? W / 2 : ah == 2 ? W : 0), y = p.y - (mid ? H / 2 : 0);
            GUI.matrix = Matrix4x4.identity; var gc = GUI.color; GUI.color = Color.white; float o = Mathf.Max(1, Mathf.Round(k));
            for (int pass = outline ? 0 : 1; pass < 2; pass++)
            {
                float xx = x;
                for (int i = 0; i < runs.Count; i++)
                {
                    var r = runs[i]; _st130.font = r.f; _st130.fontSize = r.fs; float yy = y + (H - sizes[i].y) * 0.75f;
                    if (pass == 0) { _st130.normal.textColor = QualitySettings.activeColorSpace == ColorSpace.Linear ? N_INK.gamma : N_INK; /* t1179: keep Linear text fix */ foreach (var d in new[] { new Vector2(o, o), new Vector2(-o, o), new Vector2(o, -o), new Vector2(-o, -o), new Vector2(0, 2 * o) }) GUI.Label(new Rect(xx + d.x, yy + d.y, sizes[i].x + 4, sizes[i].y + 4), r.s, _st130); }
                    else { _st130.normal.textColor = QualitySettings.activeColorSpace == ColorSpace.Linear ? c.gamma : c; GUI.Label(new Rect(xx, yy, sizes[i].x + 4, sizes[i].y + 4), r.s, _st130); }
                    xx += sizes[i].x;
                }
            }
            GUI.color = gc; GUI.matrix = m;
        }
        void RM(float x0, float y0, float x1, float y1, Color c) => R53(x0 * S130, y0 * S130, (x1 - x0) * S130, (y1 - y0) * S130, c);
        void BoxM(float x0, float y0, float x1, float y1, Color fill, Color edge, float w)
        {   // t576u: transparent fill = outline only (before, the edge rect showed through as a solid square: the white "selected" block)
            if (fill.a <= 0.001f) { RM(x0, y0, x1, y0 + w, edge); RM(x0, y1 - w, x1, y1, edge); RM(x0, y0 + w, x0 + w, y1 - w, edge); RM(x1 - w, y0 + w, x1, y1 - w, edge); return; }
            RM(x0, y0, x1, y1, edge); RM(x0 + w, y0 + w, x1 - w, y1 - w, fill); }
        void SegM(Vector2 a, Vector2 b, Color c, float w)
        {
            a *= S130; b *= S130; Vector2 d = b - a; float len = d.magnitude; if (len < 0.5f) return; float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; w *= S130;
            var m = GUI.matrix; GUI.matrix = m * Matrix4x4.TRS(new Vector3(a.x, a.y, 0), Quaternion.Euler(0, 0, ang), Vector3.one); R53(0, -w / 2, len, w, c); GUI.matrix = m;
        }
        // square (k=false) or diamond (k=true) of radius r centred at mock (x,y)
        void ShapeM(float x, float y, float r, bool k, Color fill, Color edge, float w)
        {
            if (!k) { BoxM(x - r, y - r, x + r, y + r, fill, edge, w); return; }
            float cx = x * S130, cy = y * S130, h = r / 1.41421f;
            var m = GUI.matrix; GUI.matrix = m * Matrix4x4.TRS(new Vector3(cx, cy, 0), Quaternion.Euler(0, 0, 45), Vector3.one) * Matrix4x4.Translate(new Vector3(-cx, -cy, 0));
            BoxM(x - h, y - h, x + h, y + h, fill, edge, w * 1.0f); GUI.matrix = m;
        }
        static List<string> Wrap130(string s, int n)
        {
            var o = new List<string>(); string l = "";
            foreach (var w in s.Split(' ')) { if (l.Length > 0 && l.Length + 1 + w.Length > n) { o.Add(l); l = w; } else l = l.Length == 0 ? w : l + " " + w; }
            if (l.Length > 0) o.Add(l); return o;
        }
        static readonly string[] SKULL130 = { "..XXXXXXXX..", ".XXXXXXXXXX.", "XXXXXXXXXXXX", "XXXXXXXXXXXX", "XEEEXXXXEEEX", "XEEEXXXXEEEX", "XXXXXNNXXXXX", ".XXXXNNXXXX.", "..XXXXXXXX..", "..X.X..X.X..", "..XXXXXXXX..", "...X.X.X.X.." };
        void SkullM(float cx, float cy, float u, Color b, Color e)
        { float ox = cx - 6 * u, oy = cy - 6 * u; for (int r = 0; r < 12; r++) for (int q = 0; q < 12; q++) { char ch = SKULL130[r][q]; if (ch == '.') continue; RM(ox + q * u, oy + r * u, ox + q * u + u, oy + r * u + u, ch == 'X' ? b : ch == 'E' ? e : N_INK); } }

        enum NS130 { Taken, Owned, Full, Avail, Locked, KMax, NeedSign }
        NS130 State130(int p, int i)
        {
            var n = PactData.Nodes[p][i]; bool yours = _pactPath51 == p;
            if (Taken97(p, i)) return NS130.Taken;
            if (!ReqMet51(p, n)) return NS130.Locked;
            if (!Unlocked51(p, i)) return NS130.Avail;
            if (!SignReq130(p, n)) return NS130.NeedSign; // t580u: parents must be SIGNED first
            if (n.keystone && yours && KeyCount53() >= MaxKeys51()) return NS130.KMax;
            if (yours && SelCount51() >= MaxSel51()) return NS130.Full;
            return NS130.Owned;
        }

        // t580u: SIGN needs the same req graph as UNLOCK, but with parents SIGNED (in the Pact), not just unlocked.
        // "A2/A3" = any one parent signed; "2A"/"2B" = any 2 non-Keystone notables of that group signed.
        bool SignReq130(int p, PactNode n)
        {
            if (string.IsNullOrEmpty(n.req)) return true; var ns = PactData.Nodes[p];
            if (AnyReq51(n.req)) { int c = 0; for (int i = 0; i < ns.Length; i++) if (!ns[i].keystone && (n.req[1] == 'N' || ns[i].id[0] == n.req[1]) && Taken97(p, i)) c++; return c >= 2; }
            foreach (var r in n.req.Split('/')) { int ix = NodeIx51(p, r); if (ix >= 0 && Taken97(p, ix)) return true; }
            return false;
        }
        // REMOVE is blocked while a signed child would lose its last signed parent (keeps the signed set a valid path).
        int RemoveBlock130(int p, int f)
        {
            if (!Taken97(p, f)) return -1; var ns = PactData.Nodes[p]; _pactSel51 &= ~Bit(f); int bad = -1;
            for (int i = 0; i < ns.Length; i++) if (i != f && Taken97(p, i) && !SignReq130(p, ns[i])) { bad = i; break; }
            _pactSel51 |= Bit(f); return bad;
        }
        void DrawNecro130(int p)
        {
            var nodes = PactData.Nodes[p]; bool yours = _pactPath51 == p, locked = PathLocked97(p);
            if (yours) { bool ch = false, again = true; while (again) { again = false; for (int i = 0; i < nodes.Length; i++) if (Taken97(p, i) && !SignReq130(p, nodes[i])) { _pactSel51 &= ~Bit(i); ch = again = true; } } if (ch) { if (_pactSel51 == 0) _pactPath51 = -1; PactSave51(); Msg51("UNSIGNED NODES WITHOUT A SIGNED PARENT"); } } // t580u: fix old saves
            if (_focus97 < 0 || _focus97 >= nodes.Length) { _focus97 = 0; for (int i = 0; i < nodes.Length; i++) if (Unlocked51(p, i) && !Taken97(p, i)) { _focus97 = i; break; } }
            var bg = T97("necro130_bg"); if (bg) GUI.DrawTexture(new Rect(0, 0, 700, 394), bg); else R53(0, 0, 700, 392, N_INK);
            // header: title + path, SOULS / SIGILS top-right (one PS2P 12px string each: label and number same size)
            MT("SACRIFICE ALTAR", 32, 26, 22, N_SAND); MT("PACT PATH  ·  NECROMANCER  -  THE GRAVECALLER", 250, 30, 16, N_TAN);
            string s2 = "SIGILS " + _sigils51, s1 = "SOULS " + _souls51; float x2 = 1244 - Wid130(s2, 12, true), x1 = x2 - 22 - Wid130(s1, 12, true);
            MT(s1, x1, 32, 12, N_AMB, 0, true); MT(s2, x2, 32, 12, N_SIG, 0, true);
            // bust card: v107 Bone Warden whole bust (idle loop)
            BoxM(30, 76, 166, 212, N_SLATE, N_WARM, 3); RM(34, 80, 162, 208, new Color32(58, 68, 102, 255));
            I97(T97(Av97(p, locked, true)), 34 * S130, 80 * S130, 128 * S130, 128 * S130);
            MT("NECROMANCER", 176, 82, 18, N_SAND);
            int dl = 0; foreach (var para in DESC130) foreach (var l in Wrap130(para.ToUpperInvariant(), 22)) MT(l, 176, 103 + 12 * dl++, 10, N_PALE);
            // YOUR PACT / PICK n/8 (all picks incl. Keystones)
            var tk = new List<int>(); if (yours) for (int i = 0; i < nodes.Length; i++) if (Taken97(p, i)) tk.Add(i);
            int n = tk.Count, kc = 0; foreach (int i in tk) if (nodes[i].keystone) kc++; int mx = MaxSel51(p);
            RM(24, 206, 314, 236, new Color32(43, 30, 40, 255));
            MT("YOUR PACT", 32, 212, 15, N_SAND); MT("PICK", 126, 215, 10, N_LG, 0, true); MT(n + "/" + mx, 176, 215, 16, N_SAND, 0, true);
            for (int j = 0; j < mx; j++) { float x = 244 + j * 8; BoxM(x, 216, x + 6, 227, j < n ? N_WARM : N_DARK, N_INK, 1); }
            // pact slots: icons only (no ids, no costs)
            for (int s = 0; s < mx; s++)
            {
                float x = 34 + (s % 4) * 68, y = 248 + (s / 4) * 68; BoxM(x, y, x + 57, y + 57, N_INK, s < n ? N_WARM : N_LEA2, 3);
                if (s < n) { int i = tk[s]; I97(T97("icon_4_" + i), (x + 4.5f) * S130, (y + 4.5f) * S130, 48 * S130, 48 * S130); if (Click53((x) * S130, y * S130, 57 * S130, 57 * S130)) _focus97 = i; }
                else MT("—", x + 28, y + 28, 14, N_SLATE, 1, false, true);
            }
            // Gravecaller Crest: passive only (nothing Keystone-related here)
            bool crest = yours && n >= PT.pactCrestAt51;
            BoxM(30, 384, 308, 474, crest ? new Color32(62, 39, 49, 255) : N_INK, crest ? N_TAN : N_SLATE, 3);
            BoxM(42, 405, 82, 453, crest ? N_LEA2 : N_NAVY, crest ? N_TAN : N_SLATE, 2); SkullM(62, 429, 2, crest ? N_SAND : N_GREY, crest ? N_BRED : N_RED);
            MT("GRAVECALLER CREST", 94, 392, 13, crest ? N_SAND : N_GREY);
            MT(crest ? "ACTIVE" : "LOCKED · " + n + "/" + PT.pactCrestAt51 + " NODES", 94, 410, 12, crest ? N_GRN : N_GREY);
            MT("UNDEAD MINIONS +15% DMG,", 94, 428, 11, crest ? N_TAN : N_SLATE); MT("+15% LIFE. BONE SKILLS +15% DMG", 94, 442, 11, crest ? N_TAN : N_SLATE);
            // detail panel (cost lives here only)
            BoxM(30, 486, 308, 692, N_NAVY, N_SLATE, 2);
            {
                int f = Mathf.Clamp(_focus97, 0, nodes.Length - 1); var nd = nodes[f]; var st = State130(p, f);
                MT(nd.name, 40, 494, 16, nd.keystone ? N_AMB : N_SAND);
                MT((nd.keystone ? "KEYSTONE · " : "NOTABLE · ") + (f < 9 ? "SUMMON" : "BONE"), 40, 514, 11, N_LG);
                string req = string.IsNullOrEmpty(nd.req) ? "NONE (ROOT)" : AnyReq51(nd.req) ? "ANY 2 " + (nd.req[1] == 'A' ? "SUMMON" : "BONE") + " NOTABLES" : ReqNames130(p, nd.req);
                Color N_ORG = new Color32(0xf7, 0x76, 0x22, 255);
                Color sc = st == NS130.Locked ? (Color)new Color32(90, 105, 136, 255) : st == NS130.Avail ? N_ORG : st == NS130.Owned || st == NS130.NeedSign ? N_LG : st == NS130.Taken ? N_AMB : N_BRED;
                string stt = st == NS130.Locked ? "LOCKED" : st == NS130.Avail ? "AVAILABLE - NOT UNLOCKED" : st == NS130.Owned ? "UNLOCKED - READY TO SIGN" : st == NS130.NeedSign ? "UNLOCKED - SIGN PARENT FIRST" : st == NS130.Taken ? "SIGNED - IN YOUR PACT" : st == NS130.KMax ? "KEYSTONE SLOT FULL " + kc + "/" + PT.necroMaxKeystones549 : "PACT FULL " + n + "/" + mx;
                RM(40, 535, 49, 544, sc); MT(stt, 54, 532, 12, sc);
                int cs = NodeCost51(nd), sg = nd.keystone ? PT.pactKeystoneSigils51 : 0; bool afford = _souls51 >= cs && _sigils51 >= sg;
                if (st == NS130.Locked) MT("NEEDS: " + req, 40, 550, 10, N_BRED);
                else if (st == NS130.NeedSign) MT("SIGN FIRST: " + req, 40, 550, 10, N_BRED);
                else if (st == NS130.Avail) MT("COST  " + cs + " SOULS" + (sg > 0 ? "  + " + sg + " SIGIL" : ""), 40, 550, 11, afford ? N_AMB : N_BRED);
                else MT("PICK " + n + "/" + mx + (nd.keystone ? "   KEYSTONE SLOT " + kc + "/" + PT.necroMaxKeystones549 : ""), 40, 550, 11, N_LG);
                RM(40, 568, 298, 569, N_SLATE);
                int li = 0; foreach (var l in Wrap130(nd.desc, 26)) { if (li >= 7) break; MT(l, 40, 576 + 15 * li++, 13, N_PALE); }
                bool ob = Over51(250 * S130, 492 * S130, 52 * S130, 14 * S130) && !_block53; MT("BACK", 298, 495, 10, ob ? N_AMB : N_GREY, 2);
                if (Click53(250 * S130, 492 * S130, 52 * S130, 14 * S130)) _altarScr53 = 0;
                _act140 = st == NS130.Avail ? 1 : st == NS130.Owned ? 2 : st == NS130.Taken ? 3 : 0; _actOn140 = st != NS130.Avail || afford; _actF140 = f;
            }
            Tree130(p, nodes, yours);
            Btn140(p);
            // KEYSTONE SLOT k/2: floats top-right of the TREE (Keystones only), away from the Crest and the header currency
            { float x0 = 926; int km = PT.necroMaxKeystones549; Color c = kc > 0 ? N_AMB : N_LG;
              MT("KEYSTONE SLOT", x0 + 12, 79, 9, c, 0, true); MT(kc + "/" + km, x0 + 142, 76, 16, c, 0, true);
              for (int j = 0; j < km; j++) ShapeM(x0 + 228 + j * 26, 84, 9, true, j < kc ? N_AMB : N_DARK, j < kc ? N_SAND : N_SLATE, 1);
              MT("MAX " + km, x0 + 282, 80, 8, N_LG, 0, true); }
        }
        // v140: ONE action button for the focused node, centred in the tree's empty bottom-middle (128x34 @ x835 y622). Other actions hidden.
        int _act140, _actF140; bool _actOn140;
        void Btn140(int p)
        {
            if (_act140 == 0) return; float W = 128, h = 34, x0 = 835 - W / 2, y0 = 622;
            string lab = _act140 == 1 ? "UNLOCK" : _act140 == 2 ? "SIGN" : "REMOVE";
            bool on = _actOn140, ov = on && Over51(x0 * S130, y0 * S130, W * S130, h * S130) && !_block53;
            if (on)
            {
                Color col = _act140 == 3 ? (Color)N_RED : (Color)N_LEA2; if (ov) col = Color.Lerp(col, N_WARM, 0.35f);
                BoxM(x0, y0, x0 + W, y0 + h, col, N_AMB, 2); RM(x0 + 2, y0 + 2, x0 + W - 2, y0 + 3, N_WARM); RM(x0 + 2, y0 + h - 3, x0 + W - 2, y0 + h - 2, new Color32(62, 39, 49, 255));
            }
            else BoxM(x0, y0, x0 + W, y0 + h, new Color32(24, 20, 37, 255), N_SLATE, 2);
            MT(lab, x0 + W / 2, y0 + h / 2, 12, on ? N_SAND : (Color)new Color32(90, 105, 136, 255), 1, true, true);
            if (Click53(x0 * S130, y0 * S130, W * S130, h * S130)) { if (!on) { Msg51("NOT ENOUGH SOULS / SIGILS"); return; } string l2; bool en; PrimaryLab97(p, _actF140, out l2, out en); if (en) Primary97(p, _actF140); else Msg51(l2); }
        }
        string ReqNames130(int p, string req) { var o = new List<string>(); foreach (var r in req.Split('/')) { int i = NodeIx51(p, r); o.Add(i >= 0 ? PactData.Nodes[p][i].name : r); } return string.Join(" / ", o); }

        void Tree130(int p, PactNode[] nodes, bool yours)
        {
            Color lit = N_AMB, open = new Color32(168, 134, 108, 255), lk = new Color32(86, 76, 96, 255);
            // connect lines (thin), lock -> open -> lit
            for (int pass = 0; pass < 3; pass++)
            {
                for (int r0 = 0; r0 < 18; r0 += 9) { int k = Taken97(p, r0) ? 2 : 1; if (k == pass) SegM(HUB130, V130[r0], k == 2 ? lit : open, 1.6f); }
                for (int i = 0; i < 18; i++)
                {
                    string req = nodes[i].req; if (string.IsNullOrEmpty(req) || AnyReq51(req)) continue;
                    foreach (var rq in req.Split('/')) { int pi = NodeIx51(p, rq); if (pi < 0) continue; int k = Taken97(p, pi) && Taken97(p, i) ? 2 : Unlocked51(p, pi) ? 1 : 0; if (k == pass) SegM(V130[pi], V130[i], k == 2 ? lit : k == 1 ? open : lk, 1.6f); }
                }
            }
            // OR tags
            foreach (int i in new[] { 3, 6, 12 }) if (nodes[i].req.IndexOf('/') > 0)
            { var v = V130[i]; float cx = v.x + (i < 9 ? 44 : -44); BoxM(cx - 15, v.y - 9, cx + 15, v.y + 9, N_INK, N_AMB, 2); MT("OR", cx, v.y, 11, N_AMB, 1, false, true); }
            // any-2 gates
            for (int g = 0; g < 2; g++)
            {
                int kid = g * 9 + 8, cnt = 0; for (int j = 0; j < 7; j++) if (Unlocked51(p, g * 9 + j)) cnt++;
                Vector2 pl = new Vector2(g == 0 ? 768 : 940, 208), kp = V130[kid];
                Color col = Taken97(p, kid) ? N_AMB : cnt >= 2 ? (Color)new Color32(214, 170, 128, 255) : (Color)new Color32(96, 84, 104, 255);
                Vector2 a = new Vector2(pl.x, pl.y - 14), b = new Vector2(kp.x, kp.y + 28);
                for (int s = 0; s < 6; s += 2) SegM(Vector2.Lerp(a, b, s / 6f), Vector2.Lerp(a, b, (s + 1) / 6f), col, 1.6f);
                BoxM(pl.x - 66, pl.y - 14, pl.x + 66, pl.y + 14, N_INK, col, 2);
                MT("ANY 2 " + (g == 0 ? "SUMMON" : "BONE") + " NOTABLES", pl.x, pl.y - 11, 9, col, 1);
                for (int j = 0; j < 7; j++) { float dx = pl.x - 42 + j * 14; BoxM(dx - 4, pl.y + 2, dx + 4, pl.y + 10, Unlocked51(p, g * 9 + j) ? N_AMB : N_DARK, col, 1); }
            }
            // silent hub (bone/rune/skull art, no text)
            bool any = false; for (int i = 0; i < 18; i++) if (Taken97(p, i)) any = true;
            I97(T97(any ? "necro130_hub_on" : "necro130_hub_off"), HUB130.x * S130 - 27.5f, HUB130.y * S130 - 27.5f, 55, 55);
            // nodes
            for (int i = 0; i < 18; i++)
            {
                var v = V130[i]; float x = v.x, y = v.y; bool k = nodes[i].keystone; float r0 = k ? 26 : 19; var st = State130(p, i);
                Color lc, sc; string sub; float ic = 34 * S130; Texture2D icon = T97("icon_4_" + i), icd = T97("icon_4_" + i + "_dim");
                if (st == NS130.Taken)
                {
                    ShapeM(x, y, r0 + 3, k, new Color(1f, 0.68f, 0.2f, 0.18f), new Color(1f, 0.68f, 0.2f, 0.18f), 0);
                    ShapeM(x, y, r0 + 1, k, new Color32(115, 62, 57, 255), N_AMB, 2);
                    I97(icon, x * S130 - ic / 2, y * S130 - ic / 2, ic, ic);
                    float cx = x + r0 - 2, cy = y - r0 + 2; BoxM(cx - 9, cy - 9, cx + 9, cy + 9, N_GRN, N_INK, 2); SegM(new Vector2(cx - 5, cy), new Vector2(cx - 1, cy + 4), N_INK, 3); SegM(new Vector2(cx - 1, cy + 4), new Vector2(cx + 6, cy - 5), N_INK, 3);
                    lc = N_SAND; sc = N_GRN; sub = "SIGNED";
                }
                else if (st == NS130.Owned || st == NS130.Full || st == NS130.NeedSign)
                { ShapeM(x, y, r0, k, N_SLATE, N_GREY, 2); I97(icon, x * S130 - ic / 2, y * S130 - ic / 2, ic, ic, 0.75f); lc = N_LG; sc = N_LG; sub = st == NS130.Owned ? "UNLOCKED" : "UNLOCKED · PACT FULL"; }
                else if (st == NS130.Avail)
                { ShapeM(x, y, r0, k, N_NAVY, N_WARM, 2); I97(icd, x * S130 - ic / 2, y * S130 - ic / 2, ic, ic, 0.85f); lc = N_LG; sc = N_AMB; sub = "AVAILABLE"; }
                else
                {
                    bool km = st == NS130.KMax; ShapeM(x, y, r0, k, new Color32(20, 16, 28, 255), km ? N_RED : (Color)new Color32(48, 38, 56, 255), 2);
                    I97(icd, x * S130 - ic / 2, y * S130 - ic / 2, ic, ic, 0.5f);
                    Color pc = km ? N_BRED : (Color)new Color32(90, 105, 136, 255); RM(x - 7, y + 2, x + 7, y + 13, pc); RM(x - 5, y - 6, x - 3, y + 2, pc); RM(x + 3, y - 6, x + 5, y + 2, pc); RM(x - 5, y - 6, x + 5, y - 4, pc); RM(x - 1, y + 5, x + 1, y + 9, N_INK);
                    lc = new Color32(70, 74, 100, 255); sc = km ? N_BRED : lc; sub = km ? "MAX 2 KEYSTONES" : "LOCKED";
                }
                if (_focus97 == i) { float r = r0 + 5; Color fc = new Color32(0xf7, 0x76, 0x22, 255); for (int sx = -1; sx <= 1; sx += 2) for (int sy = -1; sy <= 1; sy += 2) { RM(Mathf.Min(x + sx * r, x + sx * (r - 6)), y + sy * r - 1, Mathf.Max(x + sx * r, x + sx * (r - 6)), y + sy * r + 1, fc); RM(x + sx * r - 1, Mathf.Min(y + sy * r, y + sy * (r - 6)), x + sx * r + 1, Mathf.Max(y + sy * r, y + sy * (r - 6)), fc); } } // v140: small orange corner brackets (no white)
                // label: no plate, INK halo only (v130b SIDE/LDX/LDY offsets)
                float ly = y + (k ? r0 + 6 : r0 + 7), lx = x; int an = 1;
                if (i == 7 || i == 16) ly += 16; if (i == 16) lx -= 6;
                if (i == 1 || i == 2 || i == 10 || i == 11) { bool left = i < 9; an = left ? 2 : 0; lx = x + (left ? -(r0 + 8) : r0 + 8); ly = y - 14; }
                MT(nodes[i].name, lx, ly, 12, lc, an, false, false, true); MT(sub, lx, ly + 15, 10, sc, an, false, false, true);
                if (Click53((x - r0) * S130, (y - r0) * S130, 2 * r0 * S130, 2 * r0 * S130)) _focus97 = i;
            }
        }
    }
}
