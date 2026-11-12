using UnityEngine;
using System.Collections.Generic;

// t1259 v0.0.99 (VH t1259 "ok, apply and build it for me to test"): the Sins screen = approved Art/previews/t1246b/pass3e/cellstone_pass3e mockup.
// Native 480x270 art, integer scale, point filtered, PPU 1. Static scene (backdrop, pillars, animals, talismans, frames with skull keystones, plaques, slot
// frames, title) = ui_t1246_base.png; everything that changes (cells, tabs, buttons, text, icons) is drawn on top from the atlas ui_t1246_parts.png and the
// two hand-pixel fonts in ui_t1246_font.png. Fire, embers, talismans, pillar cracks AND the frame cracks run in ONE compositor (HellordSinFX1246.shader):
// frame cracks use the pillar crack texture/glow and the same clock. Mechanics/save/feel: unchanged (HellordSinFeel1148).
// Default ON for the Sins screen. Fallback flag: -sins1246off (v0.0.98 line screen).
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool T1246On = !System.Environment.CommandLine.Contains("-sins1246off");
        /// <summary>test hooks: pinned clock seconds (&lt;0 = real time), forced integer scale (0 = auto), fixture "mock" = the exact mockup content.</summary>
        public static bool T1246HoverOff; // autotests: ignore the real mouse for the cell hover preview
        int _t46Shown = -9;
        public static float T1246Pin = -1f; public static int T1246Scale = 0; public static string T1246Fix, T1246Force; // T1246Force = "id=state;id=state" forces a control state (tests/screenshots only)
        public static readonly Rect T46No = new Rect(162, 154, 74, 15), T46Yes = new Rect(244, 154, 74, 15);
        public int T46ScaleUsed; public string T46LastState; public bool T46Drawn;
        Texture2D _t46Atlas, _t46Font, _t46Base; bool _t46Tried, _t46Draw, _s84L1246;
        static readonly Color32 V46_VOID = new Color32(24, 20, 37, 255);
        static readonly int[][] Eyes1246 = { new[] { 20, 42 }, new[] { 34, 42 }, new[] { 459, 39 } };
        static Color32 C46(byte[] a) => new Color32(a[0], a[1], a[2], 255);
        static readonly Color32 CK_INK = new Color32(24, 20, 37, 255), CK_RED = new Color32(214, 70, 56, 255);
        float Clk46 => T1246Pin >= 0 ? T1246Pin : Time.unscaledTime;
        float DimX46 => _t46Draw ? -1500 : 0; float DimW46 => _t46Draw ? 4000 : 700; float DimH46 => _t46Draw ? 4000 : 392;
        public bool T1246Ready()
        {
            if (!_t46Tried) { _t46Tried = true; _t46Atlas = UiTex("t1246_parts"); _t46Font = UiTex("t1246_font"); _t46Base = UiTex("t1246_base"); }
            return _t46Atlas && _t46Font && _t46Base && UiTex("t1246_stone_mask") && UiTex("t1246_cracks");
        }

        // ---------- primitives (native px; whole screen px through ScrR56)
        void Spr46(string n, float x, float y)
        {
            if (!Sin1246Data.Atlas.TryGetValue(n, out var a)) return;
            Scr56Begin(out var m); GUI.color = Color.white; float W = _t46Atlas.width, H = _t46Atlas.height;
            GUI.DrawTextureWithTexCoords(ScrR56(x, y, a[2], a[3]), _t46Atlas, new Rect(a[0] / W, 1 - (a[1] + a[3]) / H, a[2] / W, a[3] / H)); GUI.matrix = m;
        }
        void Rect46(float x, float y, float w, float h, Color c) => Fill56(x, y, w, h, c);
        int W46(string s, bool f5)
        {
            if (!f5) { int x = 0; bool last = false; foreach (char ch in s) { if (Sin1246Data.FontT.TryGetValue(ch, out var g)) { x += g[1] + 1; last = true; } else { x += 4; last = false; } } return s.Length > 0 && last ? Mathf.Max(0, x - 1) : x; }
            int w = 0; bool first = true;
            foreach (char ch in s) { if (ch == ' ') { w += 3; first = true; continue; } if (!first) w++; if (Sin1246Data.FontF.TryGetValue(ch, out var g)) w += g[1]; first = false; }
            return w;
        }
        void Run46(string s, bool f5, float x, float y, Color c)
        {
            var map = f5 ? Sin1246Data.FontF : Sin1246Data.FontT; int row0 = f5 ? 8 : 0, rows = f5 ? 5 : 6; float FW = _t46Font.width, FH = _t46Font.height, cx = 0; bool first = true;
            Scr56Begin(out var m); GUI.color = c;
            foreach (char ch in s)
            {
                if (f5) { if (ch == ' ') { cx += 3; first = true; continue; } if (!first) cx++; }
                if (!map.TryGetValue(ch, out var g)) { if (!f5) cx += 4; first = false; continue; }
                GUI.DrawTextureWithTexCoords(ScrR56(x + cx, y, g[1], rows), _t46Font, new Rect(g[0] / FW, 1 - (row0 + rows) / FH, g[1] / FW, rows / FH));
                cx += f5 ? g[1] : g[1] + 1; first = false;
            }
            GUI.color = Color.white; GUI.matrix = m;
        }
        /// <summary>6-row tight font, 1px INK drop shadow (the mockup's text rule).</summary>
        void Tx46(string s, float x, float y, Color c, bool shadow = true) { s = s.ToUpperInvariant(); if (shadow) Run46(s, false, x, y + 1, CK_INK); Run46(s, false, x, y, c); }
        void Tx46C(string s, float cx, float y, Color c) { s = s.ToUpperInvariant(); Tx46(s, Mathf.Round(cx - W46(s, false) / 2f), y, c); }
        void F46C(string s, float cx, float y, Color c) { s = s.ToUpperInvariant(); Run46(s, true, Mathf.Round(cx - W46(s, true) / 2f), y, c); }
        void Wrap46(List<KeyValuePair<string, Color>> outL, string s, int w, Color c)
        {
            string cur = "";
            foreach (var word in s.ToUpperInvariant().Split(' '))
            {
                if (word.Length == 0) continue;
                string t = cur.Length > 0 ? cur + " " + word : word;
                if (cur.Length > 0 && W46(t, false) > w) { outL.Add(new KeyValuePair<string, Color>(cur, c)); cur = word; } else cur = t;
            }
            if (cur.Length > 0) outL.Add(new KeyValuePair<string, Color>(cur, c));
        }
        static int FrameAt46(int[][] fr, float t)
        {
            int tot = 0; foreach (var f in fr) tot += f[1]; int m = (int)(Mathf.Repeat(t * 1000f, tot)); int acc = 0;
            for (int i = 0; i < fr.Length; i++) { acc += fr[i][1]; if (m < acc) return i; } return fr.Length - 1;
        }
        int CellFrame46() { int tot = 0; foreach (int p in Sin1246Data.PulseMs) tot += p; int m = (int)Mathf.Repeat(Clk46 * 1000f, tot), acc = 0; for (int i = 0; i < Sin1246Data.PulseMs.Length; i++) { acc += Sin1246Data.PulseMs[i]; if (m < acc) return i; } return 0; }
        void Icon46(int col, float x, float y)
        {   // 20x20 native px icon from the approved v53 no-plate atlas (whole texel multiple)
            if (col < 0 || _tSinIcons902 == null) return;
            if (!_sinIconsNPTried1153) { _sinIconsNPTried1153 = true; _tSinIconsNP1153 = UiTex("v53_sin_icons_20_noplate"); }
            var atlas = SinIconsNoPlate1153 && _tSinIconsNP1153 != null && _tSinIconsNP1153.width == _tSinIcons902.width ? _tSinIconsNP1153 : _tSinIcons902;
            float cell = Mathf.Max(1, HellordTuning.I.sinIconCell902);
            Scr56Begin(out var m); GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(ScrR56(x, y, 20, 20), atlas, new Rect(col * cell / atlas.width, 0, cell / atlas.width, 1)); GUI.matrix = m;
        }
        void Shadow46(int[] r, bool tab)
        {   // 1px INK drop shadow right + below (tabs: right edge only), never under a glowing part
            float x = r[0], y = r[1], w = r[2], h = r[3];
            Rect46(x + w, y + 1, 1, tab ? h - 1 : h, CK_INK); if (!tab) Rect46(x + 1, y + h, w, 1, CK_INK);
        }
        bool Hov46(string id, int[] r) => Over74(r[0], r[1], r[2], r[3]) && (!Confirm49Open || id.StartsWith("c49"));
        /// <summary>Bottom-row / action / modal button (V2 rim): normal, hover, pressed, selected + glow ring, disabled. Returns the state drawn.</summary>
        string Btn46(string id, int[] r, string cls, string label, bool enabled, bool selected, bool shadow = true)
        {
            bool over = Hov46(id, r); bool rep = Event.current.type == EventType.Repaint;
            if (rep && enabled && over) _f48Hover = id;
            bool pressed = enabled && SinFeel1148On && over && _f48Down == id;
            string st = !enabled ? "disabled" : pressed ? "pressed" : selected ? "selected" : over ? "hover" : "normal";
            if (T1246Fix == "mock") st = id == "pagePREV" ? "disabled" : "normal"; { var fo = Forced46(id); if (fo != null) st = fo; }
            Spr46("btn_" + cls + "_" + st, r[0], r[1]);
            bool glow = st == "hover" || st == "pressed" || st == "selected";
            if (glow) { int lv = st == "hover" ? Sin1246Data.HoverFrames[FrameAt46(Sin1246Data.HoverFrames, Clk46)][0] : Sin1246Data.SelFrames[FrameAt46(Sin1246Data.SelFrames, Clk46)][0]; Spr46("btn_" + cls + "_g" + lv, r[0] - 1, r[1] - 1); }
            else if (shadow) Shadow46(r, false);
            if (SinFeel1148On && _f48Deny == id && Now48 - _f48DenyT < SinFeelTune.DenyButtonS) Wash48(r[0] + 1, r[1] + 1, r[2] - 2, r[3] - 2, A48(new Color(0.84f, 0.27f, 0.22f), SinFeelTune.DenyWash * (1 - (Now48 - _f48DenyT) / SinFeelTune.DenyButtonS)));
            if (label != null) F46C(label, r[0] + r[2] / 2f, r[1] + (r[3] - 5) / 2, st == "disabled" ? (Color)C46(Sin1246Data.C_S5) : (Color)C46(Sin1246Data.C_INK));
            T46LastState = id + ":" + st; return st;
        }
        bool BtnC46(string id, int[] r, string cls, string label, bool enabled, bool selected) { Btn46(id, r, cls, label, enabled, selected); return Btn48(id, r[0], r[1], r[2], r[3], enabled); }
        static string Forced46(string id) { if (string.IsNullOrEmpty(T1246Force)) return null; foreach (var e in T1246Force.Split(';')) { var kv = e.Split('='); if (kv.Length == 2 && kv[0] == id) return kv[1]; } return null; }
        string TabName46(int i, bool sel, bool over) => "tab" + i + "_" + (sel ? "selected" : over ? "hover" : "normal");

        // ---------- the screen
        public void DrawSinPanel1246()
        {
            SinLoad737(); Feel48Begin();
            _s74Click = null; if (_s74Fake.HasValue && Event.current.type == EventType.Repaint) { _s74Click = _s74Fake; _s74Fake = null; }
            var G = GUI.matrix; float W = Screen.width, H = Screen.height, g00 = G.m00; if (g00 <= 0) g00 = 1;
            float fit = Mathf.Min(W / 480f, H / 270f); int s = Mathf.Max(1, Mathf.FloorToInt(fit + 0.001f)); int sr = Mathf.RoundToInt(fit);
            if (sr > s && W / sr >= 432f && H / sr >= 243f) s = sr;          // nearly-fitting screens: integer scale, a few edge px cropped (same rule as the v0.0.98 kit)
            if (T1246Scale > 0) s = T1246Scale; T46ScaleUsed = s;
            float ox = Mathf.Round(W / 2 - s * 240f), oy = Mathf.Round(H / 2 - s * 135f);
            _vs56 = s / (K74 * g00); _vox56 = (G.m03 - ox) * K74 / s; _voy56 = (G.m13 - oy) * K74 / s;
            _full56 = G * (Matrix4x4.Scale(new Vector3(_vs56, _vs56, 1)) * Matrix4x4.Translate(new Vector3(-_vox56, -_voy56, 0))); _k56 = _full56.m00 * K74; _v56 = true; _t46Draw = true; GUI.matrix = _full56;
            try { DrawSinBody1246(); T46Drawn = true; } finally { _v56 = false; _t46Draw = false; GUI.matrix = G; }
        }
        bool Fx1246Bg()
        {
            if (Event.current.type != EventType.Repaint || _s84failed) return false;
            if (_s84 != null && !_s84L1246) DisposeSin84();
            if (_s84 == null) { SinFx84Renderer.Layout1246 = true; _s84 = new SinFx84Renderer(transform); _s84L1246 = true; }
            if (!_s84.Ready) { _s84failed = true; return false; }
            _s84.Frame(Clk46); GUI.color = Color.white;
            Graphics.DrawTexture(new Rect(0, 0, U74(480), U74(270)), _t46Base, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.white, _s84.Composite); return true;
        }
        void DrawSinBody1246()
        {
            var ev = Event.current; bool mock = T1246Fix == "mock";
            if (ev.type == EventType.Repaint) { Scr56Begin(out var m0); GUI.color = V46_VOID; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; GUI.matrix = m0; }
            if (!Fx1246Bg() && ev.type == EventType.Repaint) { Scr56Begin(out var m1); GUI.DrawTexture(ScrR56(0, 0, 480, 270), _t46Base); GUI.matrix = m1; }
            SinEyes1154();
            float t = Clk46;
            // ---- the three sin slots (32x32 frames are in the base)
            int[] sxs = { 184, 224, 264 };
            for (int i = 0; i < 3; i++)
            {
                float x = sxs[i], y = 18; bool unl = i < _sinSlots737 && i < SlotsMax737; if (mock) unl = i < 2;
                if (unl)
                {
                    int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null; if (mock) d = null;
                    if (mock) { Icon46(i == 0 ? 0 : 33, x + 6, y + 6); continue; }
                    if (d != null)
                    {
                        if (_sinSel737 == d.id) Spr46("cell_sel" + CellFrame46(), x + 4, y + 4);
                        Icon46(SinIconCol902(d.name), x + 6, y + 6); if (Hit74(x, y, 32, 32)) _sinSel737 = d.id;
                    }
                    else if (!UnlockReveal48(i, x, y, 32)) Empty46(x, y);
                }
                else if (i < SlotsMax737 || mock)
                {
                    Spr46("lock_glyph", x + 12 + DenyShake48(i), y + 11);
                    if (mock) continue;
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
                    if (LockSocket48(i, x, y, 32, next, afford, sc, gc)) { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f; Unlocked48(i); }
                }
            }
            // ---- currency plaques (frames in the base): icon + label + value centred
            for (int k = 0; k < 2; k++)
            {
                bool sigil = k == 1; var r = sigil ? Sin1246Data.R_sigils : Sin1246Data.R_souls; int n = Shown48(sigil, sigil ? _sigils51 : _souls51); if (mock) n = sigil ? 15 : 1700;
                string txt = (sigil ? "SIGILS " : "SOULS ") + (n >= 10000 ? n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) : n.ToString(System.Globalization.CultureInfo.InvariantCulture));
                string ic = sigil ? "icon_sigil" : "icon_soul"; var ia = Sin1246Data.Atlas[ic]; int tw = W46(txt, false), tot = ia[2] + 5 + tw, sx = r[0] + (r[2] - tot) / 2;
                Spr46(ic, sx, r[1] + (r[3] - ia[3]) / 2);
                Tx46(txt, sx + ia[2] + 5, r[1] + (r[3] - 7) / 2, C46(sigil ? Sin1246Data.C_SIGIL_TXT : Sin1246Data.C_SOUL_TXT));
            }
            if (!mock) Hdr46();   // t1261: readable header on a recessed stone plaque (label muted parchment, numbers bright)
            // ---- tabs (shared bottom baseline; selected is wider and 3px taller)
            var tabs = Sin1246Data.Tabs[Mathf.Clamp(_sinFilter737, 0, 3)];
            for (int i = 0; i < 4; i++)
            {
                var r = tabs[i]; bool sel = _sinFilter737 == i, over = Hov46("tab" + i, r) && !sel; if (mock) { sel = i == 0; over = false; } { var fo = Forced46("tab" + i); if (fo == "hover") over = true; }
                Spr46(TabName46(i, sel, over), r[0], r[1]);
                if (sel || over) { int lv = sel ? Sin1246Data.SelFrames[FrameAt46(Sin1246Data.SelFrames, t)][0] : Sin1246Data.HoverFrames[FrameAt46(Sin1246Data.HoverFrames, t)][0]; Spr46("tab" + i + (sel ? "_sel_g" : "_hov_g") + lv, r[0] - 1, r[1] - 1); }
                else Shadow46(r, true);
                if (rep46(ev) && over) _f48Hover = "tab" + i;
                F46C(Sin1246Data.TabLabels[i], r[0] + r[2] / 2f, r[1] + (r[3] - 5) / 2, C46(Sin1246Data.C_INK));
                if (!mock && Btn48("tab" + i, r[0], r[1], r[2], r[3], true)) { if (!sel) HellordAudio.Play("ui_confirm"); _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; }
            }
            // ---- 8x5 collection (40 per page)
            var list = SinFiltered737(); const int per = 40; int pages = Mathf.Max(1, (list.Count + per - 1) / per); if (mock) pages = 2;
            _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1); _s77Pages = pages; _s77Count = list.Count;
            int pulse = CellFrame46(); int hovShown = -1;
            for (int k = 0; k < per; k++)
            {
                var r = Sin1246Data.Cells[k]; float x = r[0], y = r[1]; int li = _sinPage737 * per + k; var d = li < list.Count ? list[li] : null;
                if (mock)
                {
                    bool locked = k % 9 == 4;
                    string cn = locked ? "cell_locked" : k == 10 ? "cell_sel" + pulse : k == 0 ? "cell_equipped" : k == 3 ? "cell_hover" : "cell_normal"; Spr46(cn, x, y);
                    if (locked) Tx46("?", x + 9, y + 9, C46(Sin1246Data.C_K3)); else Icon46(k, x + 2, y + 2); continue;
                }
                if (d == null) continue;
                bool own = _sinOwned737[d.id], sel = _sinSel737 == d.id, eq = SinEquipped737(d.id), over = (!T1246HoverOff && Over74(x, y, 24, 24) && !Confirm49Open) || Forced46("cell" + k) == "hover";
                if (over) hovShown = d.id;
                Spr46(sel ? "cell_sel" + pulse : !own ? "cell_locked" : eq ? "cell_equipped" : "cell_normal", x, y);
                if (over && !sel) Spr46("cell_hr" + Sin1246Data.HoverFrames[FrameAt46(Sin1246Data.HoverFrames, t)][0], x, y);   // 1px amber hover ring over any cell state (locked/owned/equipped); same flicker as tab/button hover
                if (over && rep46(ev)) _f48Hover = "cell" + k;
                if (own) Icon46(SinIconCol902(d.name), x + 2, y + 2); else Tx46("?", x + 9, y + 9, C46(Sin1246Data.C_K3));
                if (Hit74(x, y, 24, 24)) { _sinSel737 = d.id; _s74Page = 0; }
            }
            // ---- detail panel
            DrawDetail1246(mock, hovShown);   // hovering a cell previews it in the detail panel; leaving reverts to the selected Sin (selection is never written by hover)
            // ---- bottom row: PREV | pager | NEXT | BACK TO MENU (all 15 tall at y235)
            if (BtnC46("pagePREV", Sin1246Data.R_prev, "small", "PREV", _sinPage737 > 0, false)) { _sinPage737--; if (SinFeel1148On) HellordAudio.Play("altar_carousel"); }
            { var r = Sin1246Data.R_pager; Spr46("pager", r[0], r[1]); Shadow46(r, false); F46C((_sinPage737 + 1) + "/" + pages, r[0] + r[2] / 2f, r[1] + (r[3] - 5) / 2, C46(Sin1246Data.C_INK)); }
            if (BtnC46("pageNEXT", Sin1246Data.R_next, "small", "NEXT", _sinPage737 < pages - 1, false)) { _sinPage737++; if (SinFeel1148On) HellordAudio.Play("altar_carousel"); }
            Btn46("back", Sin1246Data.R_back, "secondary", "BACK TO MENU", true, false);
            if (BackBtn48(Sin1246Data.R_back[0], Sin1246Data.R_back[1], Sin1246Data.R_back[2], Sin1246Data.R_back[3])) { SinClose87(); return; }
            Confirm49Draw();
        }
        static bool rep46(Event e) => e.type == EventType.Repaint;

        // ================= t1261: Diablo-2 style detail panel (centred, colour-coded), header plaque, empty-slot label =================
        public static readonly Color32 T61_NAME_MORTAL = new Color32(246, 238, 222, 255), T61_NAME_DEMONIC = new Color32(228, 59, 68, 255), T61_NAME_HD = new Color32(254, 231, 97, 255), T61_BLUE = new Color32(44, 232, 245, 255),
            T61_GREEN = new Color32(99, 199, 77, 255), T61_GREY = new Color32(150, 142, 152, 255), T61_RANGE = new Color32(112, 106, 122, 255), T61_LABEL = new Color32(176, 160, 128, 255);
        const int T61_W = 100, T61_CX = 355;
        public struct Tok61 { public string s; public Color c; public bool f5; public Tok61(string s, Color c, bool f5 = false) { this.s = s; this.c = c; this.f5 = f5; } }
        static bool IsRange61(string t) { int dash = t.IndexOf('-'); if (dash <= 0 || dash >= t.Length - 1) return false; for (int i = 0; i < t.Length; i++) if (i != dash && !(char.IsDigit(t[i]) || t[i] == '.')) return false; return true; }
        int Row61W(List<Tok61> r) { int w = 0; for (int i = 0; i < r.Count; i++) w += W46(r[i].s, r[i].f5) + (i > 0 ? (r[i].f5 ? 4 : 5) : 0); return w; }
        List<List<Tok61>> Wrap61(string text, int w, Color c, bool ranges = false, bool f5 = false)
        {
            var rows = new List<List<Tok61>>(); var cur = new List<Tok61>();
            foreach (var raw in text.ToUpperInvariant().Replace(';', ',').Replace("\u2019", "'").Split(' '))
            {
                string t = raw.Replace("[", "").Replace("]", ""); if (t.Length == 0) continue; var tk = new Tok61(t, ranges && IsRange61(t) ? (Color)T61_RANGE : c, f5);
                cur.Add(tk); if (cur.Count > 1 && Row61W(cur) > w) { cur.RemoveAt(cur.Count - 1); rows.Add(cur); cur = new List<Tok61> { tk }; }
            }
            if (cur.Count > 0) rows.Add(cur); return rows;
        }
        void Row61(List<Tok61> row, float cx, float y) { float x = Mathf.Round(cx - Row61W(row) / 2f); foreach (var t in row) { if (t.f5) { Run46(t.s, true, x, y + 1, CK_INK); Run46(t.s, true, x, y, t.c); x += W46(t.s, true) + 4; } else { Tx46(t.s, x, y, t.c); x += W46(t.s, false) + 5; } } }
        static Color Tier61(SinDef d) => d.tier == SinTier.HeavenDefying ? (Color)T61_NAME_HD : d.tier == SinTier.Demonic ? (Color)T61_NAME_DEMONIC : (Color)T61_NAME_MORTAL;
        /// <summary>The detail text of one Sin as rows (pure: used by the screen and by the all-70-Sins fit test).</summary>
        public void Detail61(int sid, bool own, float[] roll, out List<List<Tok61>> nameRows, out string subtitle, out List<List<Tok61>> rows)
        {
            var d = HSins.All[sid]; nameRows = new List<List<Tok61>>(); rows = new List<List<Tok61>>(); string tier = HSins.TierName[(int)d.tier], no = "NO." + (d.id + 1);
            string[] cand = { tier + " SIN - " + no, tier + " - " + no, tier + " " + no }; subtitle = cand[cand.Length - 1];
            foreach (var cd in cand) if (W46(cd, true) <= T61_W) { subtitle = cd; break; }
            if (own)
            {
                nameRows = Wrap61(d.name, T61_W, Tier61(d)); if (nameRows.Count > 2) nameRows = Wrap61(d.name, T61_W, Tier61(d), false, true);   // 3-row names drop to the small font
                float ql = HSins.Quality(roll); bool perfect = ql >= HellordTuning.I.sinPerfectAt737 && d.mods.Count > 0;
                Color qc = d.mods.Count == 0 ? (Color)T61_GREY : ql >= 0.85f ? (Color)T61_NAME_HD : ql >= 0.45f ? (Color)T61_NAME_MORTAL : (Color)T61_GREY;
                rows.AddRange(Wrap61(HSins.QualityTxt(roll, d), T61_W, qc, false, true));
                foreach (var l0 in HSins.Lines(d, roll)) { string l = l0.StartsWith("- ") ? l0.Substring(2) : l0.StartsWith("+ ") ? l0.Substring(2) : l0; bool neg = l0.StartsWith("-"); rows.AddRange(Wrap61(l, T61_W, neg ? (Color)T61_NAME_DEMONIC : l.StartsWith("EFFECT NOT") ? (Color)T61_GREY : (Color)T61_BLUE, true, true)); }
            }
            else { nameRows = Wrap61("UNKNOWN SIN", T61_W, T61_GREY); rows.AddRange(Wrap61("Not found yet. Sins drop from demons and bosses.", T61_W, T61_GREY, false, true)); }
        }
        void DrawDetail1246(bool mock, int hovShown = -1)
        {
            int sid = hovShown >= 0 && !mock ? hovShown : _sinSel737; if (mock) sid = HSins.All.FindIndex(x => x.name.StartsWith("Lightning Rod") ) ; if (sid != _t46Shown) { _t46Shown = sid; _s74Page = 0; }
            var dr = Sin1246Data.R_dicon; Color dim = C46(Sin1246Data.C_K6); string act = null; bool actOn = false; int actKind = 0, free = -1; List<List<Tok61>> nameRows, rows; string sub = null, status = null; Color statusC = T61_GREY;
            Spr46("cell_normal", dr[0], dr[1]);
            if (sid >= 0)
            {
                var d = HSins.All[sid]; bool own = mock || _sinOwned737[d.id]; float[] roll = null; if (own) { if (mock) { roll = new float[d.mods.Count]; for (int ri = 0; ri < roll.Length; ri++) roll[ri] = 0.91f; } else roll = _sinRoll737[d.id] ?? new float[d.mods.Count]; } Detail61(sid, own, roll, out nameRows, out sub, out rows);
                if (own) Icon46(SinIconCol902(d.name), dr[0] + 2, dr[1] + 2); else Tx46("?", dr[0] + 9, dr[1] + 9, C46(Sin1246Data.C_K3));
                if (mock) { status = "OWNED"; statusC = T61_LABEL; act = "EQUIP"; actOn = true; }
                else if (own)
                {
                    if (SinEquipped737(d.id)) { int sl = 0; for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) sl = i + 1; status = "EQUIPPED - SLOT " + sl; statusC = T61_GREEN; act = "UNEQUIP"; actOn = true; actKind = 1; }
                    else { status = "OWNED"; statusC = T61_LABEL; for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; } act = _sinSlots737 <= 0 ? "NO SLOT" : free < 0 ? "SLOTS FULL" : "EQUIP"; actOn = free >= 0; actKind = 2; }
                }
                else { status = "LOCKED - NOT FOUND"; statusC = T61_NAME_DEMONIC; act = "NOT FOUND"; }
            }
            else
            {
                nameRows = Wrap61("SELECT A SIN FROM THE GRID", T61_W, dim); rows = new List<List<Tok61>>(); sub = "DETAILS"; act = "SELECT SIN";
            }
            // ---- name (tier colour, centred, max 2 rows), muted subtitle, status colour line; the baked divider sits at y143
            int nr = Mathf.Min(3, nameRows.Count); bool nf5 = nr > 0 && nameRows[0][0].f5; int np = nf5 ? 7 : 9, ny = nf5 ? 112 : 113; for (int i = 0; i < nr; i++) Row61(nameRows[i], T61_CX, ny + i * np);
            if (sub != null) Row61(Wrap61(sub, 400, (Color)T61_GREY, false, true)[0], T61_CX, ny + nr * np + (nf5 ? 2 : 1));
            // ---- stats below the divider (blue values, range dim, roll quality coloured), paged when long
            const int y0 = 147, pitch = 8, cap = 5; int capP = rows.Count > cap ? cap - 1 : cap;
            _s74Pages = Mathf.Max(1, (rows.Count + capP - 1) / capP); _s74Page = Mathf.Clamp(_s74Page, 0, _s74Pages - 1); _s74Lines = rows.Count; _s74Cap = capP; _s74Overflow = false;
            for (int i = 0; i < capP; i++) { int li = _s74Page * capP + i; if (li >= rows.Count) break; Row61(rows[li], T61_CX, y0 + i * pitch); }
            if (_s74Pages > 1)
            {
                bool lOn = _s74Page > 0, rOn = _s74Page < _s74Pages - 1; Color on = C46(Sin1246Data.C_T_BODY), off = C46(Sin1246Data.C_K3);
                Tx46C((_s74Page + 1) + "/" + _s74Pages, T61_CX, 180, on); Arrow87(318, 183, -1, lOn ? on : off); Arrow87(391, 183, 1, rOn ? on : off);
                if (Btn48("txtL", 308, 177, 18, 12, lOn)) _s74Page--; if (Btn48("txtR", 384, 177, 18, 12, rOn)) _s74Page++;
            }
            if (status != null) Tx46C(status, T61_CX, 190, statusC);
            if (act != null && BtnC46("action", Sin1246Data.R_equip, "primary", act, actOn, false) && !mock && sid == _sinSel737 && sid >= 0)
            {
                var d = HSins.All[_sinSel737];
                if (actKind == 1) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); if (SinFeel1148On) HellordAudio.Play("ui_back"); }
                else if (free >= 0) { _sinEq737[free] = d.id; SinSave737(); if (SinFeel1148On) HellordAudio.Play("ui_confirm"); }
            }
        }
        /// <summary>Small dark recessed stone plaque right of the tabs: label in muted parchment, numbers bright. 4px clear of tabs, slots and the detail frame.</summary>
        public static readonly int[] T61_HDR = { 285, 54, 131, 17 };
        void Hdr46()
        {
            var r = T61_HDR; float x = r[0], y = r[1], w = r[2], h = r[3];
            Rect46(x, y, w, h, CK_INK); Rect46(x + 1, y + 1, w - 2, h - 2, new Color32(45, 44, 43, 255)); Rect46(x + 2, y + 2, w - 3, h - 3, new Color32(30, 27, 36, 255));
            Rect46(x + 1, y + h - 2, w - 2, 1, new Color32(84, 79, 78, 255)); Rect46(x + w - 2, y + 1, 1, h - 2, new Color32(84, 79, 78, 255));
            var toks = new List<Tok61>();
            if (HdrOverride48(out var hm, out var hcol)) toks.Add(new Tok61(hm.ToUpperInvariant(), hcol == (Color)S74_RED ? (Color)CK_RED : (Color)C46(Sin1246Data.C_SOUL_TXT)));
            else if (SlotsMax737 > 0)
            {
                toks.Add(new Tok61("OWNED", T61_LABEL)); toks.Add(new Tok61(SinOwnedCount737 + "/" + HSins.Count, C46(Sin1246Data.C_SOUL_TXT)));
                toks.Add(new Tok61("SLOTS", T61_LABEL)); toks.Add(new Tok61(_sinSlots737 + "/" + SlotsMax737, C46(Sin1246Data.C_T_BRIGHT)));
            }
            if (toks.Count == 0) return;
            int tw = 0; for (int i = 0; i < toks.Count; i++) tw += W46(toks[i].s, false) + (i > 0 ? (toks.Count == 4 && i == 2 ? 9 : 5) : 0);
            if (tw > (int)w - 8) { string all = ""; foreach (var t in toks) all += (all.Length > 0 ? " " : "") + t.s; F46C(all, x + w / 2, y + (h - 5) / 2, toks[toks.Count - 1].c); return; }
            float cx = Mathf.Round(x + (w - tw) / 2f), ty = y + (h - 7) / 2;
            for (int i = 0; i < toks.Count; i++) { if (i > 0) cx += toks.Count == 4 && i == 2 ? 9 : 5; Tx46(toks[i].s, cx, ty, toks[i].c); cx += W46(toks[i].s, false); }
        }
        // ---- empty Sin slot: faint socket glyph + tiny hand-pixelled 3x5 'EMPTY' (19 px wide) inside the 23 px interior of the 32x32 red frame, >= 2 px margin
        static readonly Dictionary<char, string> Tiny61 = new Dictionary<char, string> { { 'E', "XXXX..XXXX..XXX" }, { 'M', "X.XXXXXXXX.XX.X" }, { 'P', "XXXX.XXXXX..X.." }, { 'T', "XXX.X..X..X..X." }, { 'Y', "X.XX.X.X..X..X." } };
        public static int Tiny61Width(string s) => s.Length * 3 + (s.Length - 1);
        void Tiny61Draw(string s, float x, float y, Color c) { foreach (char ch in s) { var g = Tiny61[ch]; for (int k = 0; k < 15; k++) if (g[k] == 'X') Rect46(x + k % 3, y + k / 3, 1, 1, c); x += 4; } }
        public static int T1261EmptyStyle; // 0 = faint socket glyph + tiny label (default), 1 = tiny label only
        void Empty46(float x, float y)
        {
            Color faint = new Color32(70, 62, 84, 255), lab = new Color32(112, 102, 128, 255); int lw = Tiny61Width("EMPTY"); float lx = x + Mathf.Round((32 - lw) / 2f);
            if (T1261EmptyStyle == 1) { Tiny61Draw("EMPTY", lx, y + 13, lab); return; }
            float cx = x + 16, cy = y + 11;   // diamond socket 9x9 (outline) with a centre pip
            for (int d = 0; d <= 4; d++) { Rect46(cx - d, cy - (4 - d), 1, 1, faint); Rect46(cx + d, cy - (4 - d), 1, 1, faint); Rect46(cx - d, cy + (4 - d), 1, 1, faint); Rect46(cx + d, cy + (4 - d), 1, 1, faint); }
            Rect46(cx, cy, 1, 1, faint); Tiny61Draw("EMPTY", lx, y + 20, lab);
        }
        /// <summary>Modal in the pass3e style (called from Confirm49Draw while this screen draws). Out: the hit rects.</summary>
        void Confirm1246Draw(Rect b, ref Rect yes, ref Rect no, float dy, string title, string cost, string l1, string l2)
        {
            no = T46No; no.y += dy; yes = T46Yes; yes.y += dy;
            Spr46("modal_frame", b.x - 1, b.y - 1);
            Tx46C(title, b.x + b.width / 2, b.y + 11, C46(Sin1246Data.C_T_BRIGHT)); Tx46C(cost, b.x + b.width / 2, b.y + 28, C46(Sin1246Data.C_SOUL_TXT));
            Tx46C(l1, b.x + b.width / 2, b.y + 40, C46(Sin1246Data.C_K6)); Tx46C(l2, b.x + b.width / 2, b.y + 50, C46(Sin1246Data.C_K6));
            Btn46("c49no", new[] { 162, (int)(154 + dy), 74, 15 }, "small", "CANCEL", true, false); Btn46("c49yes", new[] { 244, (int)(154 + dy), 74, 15 }, "small", "UNLOCK", true, true);
        }
    }
}
