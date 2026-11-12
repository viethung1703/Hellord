using UnityEngine;
using System.Collections.Generic;

// t1300 (VH t1296/t1297/t1299): the MENU-THEME Sins screen = approved mockup Art/previews/t1287_redraw/D2_MORTA_v6 (D2 Morta frame, lava background at 55 % dim, 26 px flames in 32 px shafts).
// Selected with -uitheme menu (HellordUITheme). Stone1246 (v0.1.0) stays the default and is untouched. Everything is drawn in the 640x360 grid at an INTEGER scale
// (3x at 1080p, 2x at 720p) from exported layers (tools/t1300/mt_export.py): mt_bg (AI lava background, dimmed, palette-snapped), mt_scene (static frame: arch, pillars, panels, title, sconce straps),
// mt_atlas (cells, niches, buttons, bowl, coals, 2x16 main-menu flame frames, firelight overlays, halo). Text uses the mockup's hand-pixelled glyph sets (MenuThemeData), built to small textures at runtime.
// Sconce fire = the main menu's fire60 flame (16 frames, 90 ms each, per-side phase offset) burning behind the bowl's front rim with a 3-frame coal bed, runtime embers (rise, drift, split, cool, smooth fade),
// pre-baked stone firelight levels that only move stone ramp steps (texture is kept) + a dithered halo on the background. Fallback flags: -mtfireoff (static fire), -uitheme stone (default).
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool MtFireOff = System.Environment.CommandLine.Contains("-mtfireoff");
        /// <summary>test/capture hooks: pinned clock (&lt;0 = real), forced integer scale (0 = auto), forced control states "tab1=hover;cell5=hover;equip=hover;back=hover;slot1=hover", fake click in native 640x360 px.</summary>
        public static float MenuThemePin = -1f; public static int MenuThemeScale; public static string MenuThemeForce; public static bool MenuThemeIgnoreMouse;
        public Vector2? MenuThemeFake; public int MenuThemeScaleUsed; public bool MenuThemeDrawn; public string MenuThemeLastHover;
        public int MenuThemeAskSlot => _mtAsk; public int MenuThemeStatPages => _mtPages; public int MenuThemeFlameFrame, MenuThemeLightLevel; public int MenuThemeEmberCount;

        Texture2D _mtBg, _mtScene, _mtAtlas; bool _mtTried; int _mtS; float _mtOx, _mtOy; bool _mtRep;
        readonly Dictionary<string, Texture2D> _mtTxt = new Dictionary<string, Texture2D>();
        string _mtDown, _mtHover; int _mtAsk = -1; float _mtAskT = -9; string _mtMsg; string _mtMsgCol = "AMBER"; float _mtMsgUntil = -9; int _mtShown = -9, _mtPage, _mtPages = 1; bool _mtFakeUsed;
        static readonly Color32 MT_INK = new Color32(24, 20, 37, 255);

        static Color32 MtC(string n) { return MenuThemeData.Pal[n]; }
        static Color32[] MtRamp(params string[] cols) { var r = new Color32[cols.Length]; for (int i = 0; i < cols.Length; i++) r[i] = MtC(cols[i]); return r; }
        static string[] MtRep(string a, int na, string b = null, int nb = 0, string c = null, int nc = 0) { var l = new List<string>(); for (int i = 0; i < na; i++) l.Add(a); for (int i = 0; i < nb; i++) l.Add(b); for (int i = 0; i < nc; i++) l.Add(c); return l.ToArray(); }
        static readonly Color32[] MT_TAB_N = MtRamp(MtRep("BONE", 6, "CLAY", 3)), MT_TAB_H = MtRamp(MtRep("AMBER", 4, "ORANGE", 5)), MT_TAB_S = MtRamp(MtRep("PINK", 2, "RED", 4, "DRED", 3));
        static readonly Color32[][] MT_NAME = { MtRamp(MtRep("BONE", 4, "CLAY", 3, "TERRA", 2)), MtRamp(MtRep("AMBER", 3, "ORANGE", 4, "TERRA", 2)), MtRamp(MtRep("YELLOW", 5, "AMBER", 4)) };   // by SinTier (Mortal, Demonic, Heaven-Defying)
        static readonly Color32[] MT_NAME_DIM = MtRamp(MtRep("DIM", 9));

        bool MtReady()
        {
            if (!_mtTried) { _mtTried = true; _mtBg = MtTex("mt_bg"); _mtScene = MtTex("mt_scene"); _mtAtlas = MtTex("mt_atlas"); }
            return _mtBg && _mtScene && _mtAtlas;
        }
        static Texture2D MtTex(string n) { var t = Resources.Load<Texture2D>("HellordArt/MenuTheme/" + n); if (t) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        float MtClock() { return MenuThemePin >= 0 ? MenuThemePin : Time.unscaledTime; }

        // ---------------------------------------------------------------- drawing primitives (whole screen px, GUI.matrix = identity)
        void MtBlit(Texture2D t, int sx, int sy, int sw, int sh, int dx, int dy, float a = 1f, Color? tint = null)
        {
            if (!_mtRep || a <= 0.002f) return;
            int cx0 = Mathf.Max(0, dx), cy0 = Mathf.Max(0, dy), cx1 = Mathf.Min(640, dx + sw), cy1 = Mathf.Min(360, dy + sh); if (cx1 <= cx0 || cy1 <= cy0) return;
            int ssx = sx + (cx0 - dx), ssy = sy + (cy0 - dy), ssw = cx1 - cx0, ssh = cy1 - cy0;
            var r = new Rect(_mtOx + cx0 * _mtS, _mtOy + cy0 * _mtS, ssw * _mtS, ssh * _mtS);
            var uv = new Rect(ssx / (float)t.width, 1f - (ssy + ssh) / (float)t.height, ssw / (float)t.width, ssh / (float)t.height);
            var c = tint ?? Color.white; c.a *= a; GUI.color = c; GUI.DrawTextureWithTexCoords(r, t, uv); GUI.color = Color.white;
        }
        void MtSpr(string n, int x, int y, float a = 1f, Color? tint = null) { var r = MenuThemeData.Atlas[n]; MtBlit(_mtAtlas, r[0], r[1], r[2], r[3], x, y, a, tint); }
        void MtFill(int x, int y, int w, int h, Color c)
        {
            if (!_mtRep || w <= 0 || h <= 0) return; int cx0 = Mathf.Max(0, x), cy0 = Mathf.Max(0, y), cx1 = Mathf.Min(640, x + w), cy1 = Mathf.Min(360, y + h); if (cx1 <= cx0 || cy1 <= cy0) return;
            GUI.color = c; GUI.DrawTexture(new Rect(_mtOx + cx0 * _mtS, _mtOy + cy0 * _mtS, (cx1 - cx0) * _mtS, (cy1 - cy0) * _mtS), Texture2D.whiteTexture); GUI.color = Color.white;
        }
        void MtBrackets(int x, int y, int w, int h)   // hover brackets: 3 px ORANGE arms, 2 px outside the control (as in the mockup)
        {
            var o = (Color)MtC("ORANGE");
            int[,] cs = { { x - 2, y - 2, 1, 1 }, { x + w + 1, y - 2, -1, 1 }, { x - 2, y + h + 1, 1, -1 }, { x + w + 1, y + h + 1, -1, -1 } };
            for (int k = 0; k < 4; k++) for (int i = 0; i < 3; i++) { MtFill(cs[k, 0] + cs[k, 2] * i, cs[k, 1], 1, 1, o); MtFill(cs[k, 0], cs[k, 1] + cs[k, 3] * i, 1, 1, o); }
        }
        // ---------------------------------------------------------------- text (hand-pixelled glyph sets, cached textures)
        static string[] MtGlyph(bool disp, char c) { var d = disp ? MenuThemeData.Disp : MenuThemeData.Small; if (d.TryGetValue(c, out var g)) return g; return d.TryGetValue('?', out var q) ? q : null; }
        public static int MtW(string s, bool disp)
        {
            int space = disp ? 4 : 3, w = 0, n = 0;
            foreach (char c in s) { if (n > 0) w++; n++; if (c == ' ') { w += space; continue; } var g = MtGlyph(disp, c); if (g == null) continue; int gw = 0; foreach (var r in g) gw = Mathf.Max(gw, r.Length); w += gw; }
            return w;
        }
        /// <summary>true when every character of s (after Sanitize) has a glyph of its own (no '?' substitution).</summary>
        public static bool MtCovered(string s, bool disp) { var d = disp ? MenuThemeData.Disp : MenuThemeData.Small; foreach (char c in s) if (c != ' ' && !d.ContainsKey(c)) return false; return true; }
        Texture2D MtTextTex(string s, bool disp, Color32[] rows, bool shadow, string key)
        {
            string k = (disp ? "D|" : "S|") + key + (shadow ? "|sh|" : "|") + s; if (_mtTxt.TryGetValue(k, out var tx) && tx) return tx;
            if (_mtTxt.Count > 900) { foreach (var v in _mtTxt.Values) if (v) Destroy(v); _mtTxt.Clear(); }
            int gh = disp ? 9 : 7, space = disp ? 4 : 3, w = MtW(s, disp), W = w + 2, H = gh + 2; var px = new Color32[W * H]; var deep = MtC("DEEP"); int cx = 0, n = 0;
            void P(int x, int y, Color32 c) { if (x < 0 || y < 0 || x >= W || y >= H) return; px[(H - 1 - y) * W + x] = c; }
            for (int pass = 0; pass < 2; pass++)
            {
                cx = 0; n = 0;
                foreach (char ch in s)
                {
                    if (n > 0) cx++; n++; if (ch == ' ') { cx += space; continue; }
                    var g = MtGlyph(disp, ch); if (g == null) continue; int gw = 0; foreach (var r in g) gw = Mathf.Max(gw, r.Length);
                    for (int j = 0; j < g.Length; j++) for (int i = 0; i < g[j].Length; i++)
                    {
                        if (g[j][i] != '#') continue;
                        if (pass == 0) { if (disp) { for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) P(1 + cx + i + dx, 1 + j + dy, deep); } else if (shadow) P(1 + cx + i + 1, 1 + j + 1, deep); }
                        else P(1 + cx + i, 1 + j, rows[Mathf.Min(j, rows.Length - 1)]);
                    }
                    cx += gw;
                }
            }
            tx = new Texture2D(W, H, TextureFormat.RGBA32, false); tx.filterMode = FilterMode.Point; tx.wrapMode = TextureWrapMode.Clamp; tx.SetPixels32(px); tx.Apply(false); _mtTxt[k] = tx; return tx;
        }
        void MtDrawTex(Texture2D t, int x, int y) { if (!_mtRep) return; MtBlit(t, 0, 0, t.width, t.height, x - 1, y - 1); }
        /// <summary>SMALL 5x7 glyphs (palette colour name), optional 1 px DEEP drop shadow; (x,y) = top-left of the glyph box.</summary>
        void MtTx(string s, int x, int y, string col, bool shadow = false) { if (s.Length == 0) return; s = Sanitize(s); MtDrawTex(MtTextTex(s, false, new[] { MtC(col) }, shadow, col), x, y); }
        void MtTxC(string s, int cx, int y, string col, bool shadow = false) { s = Sanitize(s); MtTx(s, cx - MtW(s, false) / 2, y, col, shadow); }
        void MtDisp(string s, int x, int y, Color32[] rows, string key) { if (s.Length == 0) return; s = Sanitize(s); MtDrawTex(MtTextTex(s, true, rows, false, key), x, y); }
        // ---------------------------------------------------------------- input (native 640x360 coordinates)
        Vector2 MtMouse() { var m = Event.current.mousePosition; return new Vector2((m.x - _mtOx) / _mtS, (m.y - _mtOy) / _mtS); }
        static bool MtIn(Vector2 p, int x, int y, int w, int h) { return p.x >= x && p.y >= y && p.x < x + w && p.y < y + h; }
        static string MtForced(string id) { if (string.IsNullOrEmpty(MenuThemeForce)) return null; foreach (var e in MenuThemeForce.Split(';')) { var kv = e.Split('='); if (kv.Length == 2 && kv[0] == id) return kv[1]; } return null; }
        bool MtOver(string id, int x, int y, int w, int h, bool allowModal = false)
        {
            if (MtForced(id) == "hover") return true;
            if (MenuThemeIgnoreMouse || T1246HoverOff) return false; if (_mtAsk >= 0 && !allowModal) return false;
            return MtIn(MtMouse(), x, y, w, h);
        }
        /// <summary>true when the control was clicked: mouse released over the control that got the press (drag-off cancels), or a fake click (tests) inside its rect.</summary>
        bool MtClick(string id, int x, int y, int w, int h, bool enabled = true, bool allowModal = false)
        {
            var ev = Event.current; if (!enabled) return false; if (_mtAsk >= 0 && !allowModal) return false;
            if (MenuThemeFake.HasValue) { if (ev.type == EventType.Repaint && !_mtFakeUsed && MtIn(MenuThemeFake.Value, x, y, w, h)) { _mtFakeUsed = true; return true; } return false; }
            if (MenuThemeIgnoreMouse) return false;
            bool inside = MtIn(MtMouse(), x, y, w, h);
            if (ev.type == EventType.MouseDown && ev.button == 0 && inside) { _mtDown = id; ev.Use(); }
            else if (ev.type == EventType.MouseUp && ev.button == 0 && _mtDown == id) { _mtDown = null; ev.Use(); return inside; }
            return false;
        }
        bool MtPressed(string id, bool over) { return over && _mtDown == id; }
        // ---------------------------------------------------------------- controls
        bool MtButton(string id, int x, int y, int w, int h, string label, bool enabled, string cls, bool primary, bool allowModal = false)
        {
            bool over = enabled && MtOver(id, x, y, w, h, allowModal); bool press = MtPressed(id, over);
            string st = !enabled ? "disabled" : over ? "hover" : "normal"; string spr = primary ? (cls == "ps" ? "btn_ps_" : "btn_p_") + (enabled ? "normal" : "disabled") : "btn_" + cls + "_" + st;
            if (over && _mtRep) MenuThemeLastHover = id;
            MtSpr(spr, x, y); if (over) MtBrackets(x, y, w, h);
            string lab = Sanitize(label); int tx = x + (w - MtW(lab, false)) / 2 + (press ? 1 : 0), ty = y + (h - 7) / 2 + (press ? 1 : 0);
            if (enabled) MtTx(lab, tx, ty, "BONE", true); else MtTx(lab, tx, ty, "DIM");
            return MtClick(id, x, y, w, h, enabled, allowModal);
        }
        void MtIcon(int col, int x, int y)
        {
            if (col < 0 || _tSinIcons902 == null) return;
            if (!_sinIconsNPTried1153) { _sinIconsNPTried1153 = true; _tSinIconsNP1153 = UiTex("v53_sin_icons_20_noplate"); }
            var atlas = SinIconsNoPlate1153 && _tSinIconsNP1153 != null && _tSinIconsNP1153.width == _tSinIcons902.width ? _tSinIconsNP1153 : _tSinIcons902; int cell = Mathf.Max(1, Mathf.RoundToInt(HellordTuning.I.sinIconCell902));
            MtBlit(atlas, col * cell, 0, cell, atlas.height, x, y);
        }
        static string MtTierName(SinDef d) { return HSins.TierName[(int)d.tier]; }
        int MtTierIdx(SinDef d) { return d.tier == SinTier.HeavenDefying ? 2 : d.tier == SinTier.Demonic ? 1 : 0; }
        void MtMsg(string s, string col, float sec) { _mtMsg = s; _mtMsgCol = col; _mtMsgUntil = MtClock() + sec; }

        /// <summary>Called first from the Sins ESC handling: the unlock confirm owns Esc while it is open. Returns true when Esc was used.</summary>
        public bool MenuThemeEscape()
        {
            if (HellordUITheme.Active != HellordUITheme.Theme.MenuTheme || _mtAsk < 0) return false;
            if (HInput.Down(HKey.Escape)) { _mtAsk = -1; HellordAudio.Play("ui_back"); return true; }
            return false;
        }

        // ================================================================ the screen
        bool DrawSinPanelMenuTheme()
        {
            if (!MtReady()) return false;
            SinLoad737(); var G = GUI.matrix; _mtRep = Event.current.type == EventType.Repaint;
            float W = Screen.width, H = Screen.height, fit = Mathf.Min(W / 640f, H / 360f); int s = Mathf.Max(1, Mathf.FloorToInt(fit + 0.001f)), sr = Mathf.RoundToInt(fit);
            if (sr > s && W / sr >= 576f && H / sr >= 324f) s = sr; if (MenuThemeScale > 0) s = MenuThemeScale;
            _mtS = s; MenuThemeScaleUsed = s; _mtOx = Mathf.Round(W / 2 - s * 320f); _mtOy = Mathf.Round(H / 2 - s * 180f);
            GUI.matrix = Matrix4x4.identity;
            try { MtBody(); MenuThemeDrawn = true; } finally { GUI.matrix = G; }
            return true;
        }
        void MtBody()
        {
            var ev = Event.current; float t = MtClock(); if (_mtRep) { _mtFakeUsed = false; MenuThemeLastHover = null; MenuThemeEmberCount = 0; }
            if (_mtRep)
            {
                GUI.color = MT_INK; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
                MtBlit(_mtBg, 0, 0, 640, 360, 0, 0);
                MtFire(t);
            }
            // ---- three sin slots (niches 28x32 at x 256/306/356, y 74)
            int[] nxs = { 256, 306, 356 }; const int NY = 74; string hdr = null; string hdrCol = "DIM";
            for (int i = 0; i < 3; i++)
            {
                int x = nxs[i]; bool unl = i < _sinSlots737 && i < SlotsMax737; string id = "slot" + i;
                if (unl)
                {
                    int sid = _sinEq737[i]; var d = sid >= 0 ? HSins.All[sid] : null; bool over = MtOver(id, x, NY, 28, 32);
                    MtSpr("niche_normal", x, NY); if (d != null) MtIcon(SinIconCol902(d.name), x + 4, NY + 10); if (over) { MtBrackets(x, NY, 28, 32); if (_mtRep) MenuThemeLastHover = id; }
                    if (d != null && MtClick(id, x, NY, 28, 32)) { _sinSel737 = d.id; HellordAudio.Play("ui_confirm"); }
                }
                else
                {
                    MtSpr("niche_locked", x, NY); bool can = i < SlotsMax737; bool over = can && MtOver(id, x, NY, 28, 32);
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
                    if (can && i == 2) { MtSpr("icon_soul", 394, 87); MtTx(sc.ToString(System.Globalization.CultureInfo.InvariantCulture), 404, 86, afford && next ? "YELLOW" : "DIM"); }
                    if (over) { MtBrackets(x, NY, 28, 32); if (_mtRep) MenuThemeLastHover = id; hdr = next ? "UNLOCK SLOT " + (i + 1) + ": " + sc + " SOULS + " + gc + " SIGIL" + (gc == 1 ? "" : "S") : "UNLOCK SLOT " + (_sinSlots737 + 1) + " FIRST"; hdrCol = next && afford ? "AMBER" : "HOT"; }
                    if (can && MtClick(id, x, NY, 28, 32))
                    {
                        if (!next) { MtMsg("UNLOCK SLOT " + (_sinSlots737 + 1) + " FIRST", "HOT", 1.6f); HellordAudio.Play("ui_back"); }
                        else if (!afford) { MtMsg("NEED " + sc + " SOULS + " + gc + " SIGIL" + (gc == 1 ? "" : "S"), "HOT", 1.8f); HellordAudio.Play("ui_back"); }
                        else { _mtAsk = i; _mtAskT = Time.unscaledTime; HellordAudio.Play("ui_confirm"); }
                    }
                }
            }
            // ---- currency + slot/owned headers
            MtSpr("icon_soul", 52, 87); MtTx("SOULS " + _souls51, 64, 86, "YELLOW");
            { string sg = "SIGILS " + _sigils51; int iw = MenuThemeData.Atlas["icon_sigil"][2]; int ix = 588 - iw; MtTx(sg, ix - 4 - MtW(sg, false), 86, "PINK"); MtSpr("icon_sigil", ix, 87); }
            if (_mtMsg != null && MtClock() < _mtMsgUntil) { hdr = _mtMsg; hdrCol = _mtMsgCol; }
            MtTxC(hdr ?? "SLOTS " + _sinSlots737 + "/" + SlotsMax737, 320, 110, hdr != null ? hdrCol : "DIM");
            { string ow = "OWNED " + SinOwnedCount737 + "/" + HSins.Count; MtTx(ow, 588 - MtW(ow, false), 122, "DIM"); }
            // ---- tabs
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" }; int tx = 52, TY = 121;
            for (int i = 0; i < 4; i++)
            {
                int w = MtW(fl[i], true); bool sel = _sinFilter737 == i; string id = "tab" + i; bool over = !sel && MtOver(id, tx - 6, TY - 1, w + 12, 17);
                MtDisp(fl[i], tx, TY, sel ? MT_TAB_S : over ? MT_TAB_H : MT_TAB_N, sel ? "S" : over ? "H" : "N");
                if (sel && _mtRep)
                {
                    for (int k = -3; k < w + 3; k++) { MtFill(tx + k, TY + 12, 1, 1, MtC("RED")); MtFill(tx + k, TY + 13, 1, 1, MtC("BLOOD")); MtFill(tx + k, TY + 14, 1, 1, MtC("DEEP")); }
                    foreach (int ax in new[] { tx - 6, tx + w + 3 }) { MtFill(ax + 1, TY + 11, 1, 1, MtC("RED")); MtFill(ax, TY + 12, 1, 1, MtC("RED")); MtFill(ax + 2, TY + 12, 1, 1, MtC("RED")); MtFill(ax + 1, TY + 12, 1, 1, MtC("PINK")); MtFill(ax + 1, TY + 13, 1, 1, MtC("RED")); }
                }
                if (over && _mtRep) MenuThemeLastHover = id;
                if (MtClick(id, tx - 6, TY - 1, w + 12, 17, !sel)) { _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; HellordAudio.Play("ui_confirm"); }
                tx += w + 22;
            }
            // ---- 8x5 collection (cells 28 px at pitch 30)
            var list = SinFiltered737(); const int per = 40; int pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1); _s77Pages = pages; _s77Count = list.Count;
            int pulse = Mathf.Clamp((int)((0.5f - 0.5f * Mathf.Cos(Mathf.Repeat(t / 2.4f, 1f) * 6.2831853f)) * 2.9999f), 0, 2); int hovShown = -1;
            for (int k = 0; k < per; k++)
            {
                int x = 58 + (k % 8) * 30, y = 153 + (k / 8) * 30, li = _sinPage737 * per + k; var d = li < list.Count ? list[li] : null;
                if (d == null) { MtSpr("cell_empty", x, y); continue; }
                bool own = _sinOwned737[d.id], sel = _sinSel737 == d.id, eq = SinEquipped737(d.id), over = MtOver("cell" + k, x, y, 28, 28);
                MtSpr(sel ? "cell_sel" + pulse : !own ? "cell_locked" : eq ? "cell_equipped" : "cell_normal", x, y);
                if (own) MtIcon(SinIconCol902(d.name), x + 4, y + 4); else if (sel) MtSpr("lock_glyph", x + 9, y + 9);
                if (over) { hovShown = d.id; MtBrackets(x, y, 28, 28); if (_mtRep) MenuThemeLastHover = "cell" + k; }
                if (MtClick("cell" + k, x, y, 28, 28)) { _sinSel737 = d.id; HellordAudio.Play("ui_confirm"); }
            }
            MtDetail(hovShown);
            // ---- bottom row: PREV | pager | NEXT | BACK TO MENU
            if (MtButton("pagePREV", 87, 320, 60, 20, "PREV", _sinPage737 > 0, "s", false)) { _sinPage737--; HellordAudio.Play("altar_carousel"); }
            MtSpr("counter", 155, 320); { string pg = (_sinPage737 + 1) + "/" + pages; MtTx(pg, 155 + (44 - MtW(pg, false)) / 2, 320 + (20 - 7) / 2, "BONE"); }
            if (MtButton("pageNEXT", 207, 320, 60, 20, "NEXT", _sinPage737 < pages - 1, "s", false)) { _sinPage737++; HellordAudio.Play("altar_carousel"); }
            if (MtButton("back", 387, 320, 136, 20, "BACK TO MENU", true, "b", false)) { SinClose87(); return; }
            MtModal();
            if (_mtRep && MenuThemeFake.HasValue) MenuThemeFake = null;
            if (ev.type == EventType.MouseUp) _mtDown = null;
        }
        // ---------------------------------------------------------------- detail panel (D2 style: tier-ramp name, DIM sub, divider, ROLL line, coloured stats, status, EQUIP)
        struct MtRow { public string s, col; public MtRow(string s, string col) { this.s = s; this.col = col; } }
        List<string> MtWrapPx(string text, int maxw)
        {
            var o = new List<string>(); string cur = "";
            foreach (var word in Sanitize(text).Split(' ')) { if (word.Length == 0) continue; string t = cur.Length > 0 ? cur + " " + word : word; if (cur.Length > 0 && MtW(t, false) > maxw) { o.Add(cur); cur = word; } else cur = t; }
            if (cur.Length > 0) o.Add(cur); return o;
        }
        /// <summary>Detail text rows of one Sin (pure: used by the screen and by the all-70 fit test). Rows already wrapped to the panel width.</summary>
        public void MtDetailRows(int sid, bool own, float[] roll, out List<string> name, out string sub, out List<string[]> rows)
        {
            var d = HSins.All[sid]; name = new List<string>(); rows = new List<string[]>(); sub = MtTierName(d) + " SIN - NO." + (d.id + 1);
            if (!own) { name = MtWrapPx("UNKNOWN SIN", 200); rows.Add(new[] { "NOT FOUND YET. SINS DROP FROM DEMONS AND BOSSES.", "DIM" }); return; }
            name = MtWrapPx(d.name, 200);
            float ql = HSins.Quality(roll); bool perfect = ql >= HellordTuning.I.sinPerfectAt737 && d.mods.Count > 0;
            foreach (var w in MtWrapPx(HSins.QualityTxt(roll, d), 236)) rows.Add(new[] { w, d.mods.Count == 0 ? "DIM" : perfect ? "YELLOW" : "AMBER" });
            foreach (var l0 in HSins.Lines(d, roll)) { string l = l0.StartsWith("- ") || l0.StartsWith("+ ") ? l0.Substring(2) : l0; bool neg = l0.StartsWith("-"); foreach (var w in MtWrapPx(l, 236)) rows.Add(new[] { w, neg ? "HOT" : l.StartsWith("EFFECT NOT") ? "DIM" : "CYAN" }); }
        }
        void MtDetail(int hovShown)
        {
            int sid = hovShown >= 0 ? hovShown : _sinSel737; if (sid != _mtShown) { _mtShown = sid; _mtPage = 0; }
            const int IX = 337, IY = 161, DIVY = 197, EY = 269; int pulse = Mathf.Clamp((int)((0.5f - 0.5f * Mathf.Cos(Mathf.Repeat(MtClock() / 2.4f, 1f) * 6.2831853f)) * 2.9999f), 0, 2);
            MtSpr("cell_sel" + pulse, IX, IY);
            string act = "SELECT SIN", status = null, statusCol = "BONE"; bool actOn = false; int actKind = 0, free = -1; _mtPages = 1;
            if (sid < 0) { MtDisp("SELECT A SIN", 373, IY, MT_NAME_DIM, "dim"); MtTx("FROM THE GRID", 373, IY + 13, "DIM"); }
            else
            {
                var d = HSins.All[sid]; bool own = _sinOwned737[d.id]; float[] roll = own ? (_sinRoll737[d.id] ?? new float[d.mods.Count]) : null;
                MtDetailRows(sid, own, roll, out var name, out var sub, out var rows);
                if (own) MtIcon(SinIconCol902(d.name), IX + 4, IY + 4); else MtSpr("lock_glyph", IX + 9, IY + 9);
                bool small = name.Count > 2; int ny = IY; var ramp = own ? MT_NAME[MtTierIdx(d)] : MT_NAME_DIM;
                foreach (var ln in name) { if (small) { MtTx(ln, 373, ny + 1, own ? (d.tier == SinTier.HeavenDefying ? "YELLOW" : d.tier == SinTier.Demonic ? "AMBER" : "BONE") : "DIM", true); ny += 9; } else { MtDisp(ln, 373, ny, ramp, own ? "t" + MtTierIdx(d) : "dim"); ny += 12; } }
                MtTx(sub, 373, ny + 1, "DIM");
                int pitch = rows.Count <= 4 ? 11 : 9, cap = rows.Count <= 4 ? 4 : 5; int capP = rows.Count > cap ? cap - 1 : cap; _mtPages = Mathf.Max(1, (rows.Count + capP - 1) / capP); _mtPage = Mathf.Clamp(_mtPage, 0, _mtPages - 1);
                for (int i = 0; i < capP; i++) { int ri = _mtPage * capP + i; if (ri >= rows.Count) break; MtTx(rows[ri][0], IX, 207 + i * pitch, rows[ri][1]); }
                if (_mtPages > 1)
                {
                    string pg = (_mtPage + 1) + "/" + _mtPages; MtTx(pg, 555 - MtW(pg, false) / 2, 246, "BONE");
                    bool lOn = _mtPage > 0, rOn = _mtPage < _mtPages - 1; MtTx("<", 523, 246, lOn ? "BONE" : "DIM"); MtTx(">", 584, 246, rOn ? "BONE" : "DIM");
                    if (MtClick("txtL", 517, 240, 19, 15, lOn)) _mtPage--; if (MtClick("txtR", 576, 240, 19, 15, rOn)) _mtPage++;
                }
                if (own)
                {
                    if (SinEquipped737(d.id)) { int sl = 0; for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) sl = i + 1; status = "EQUIPPED - SLOT " + sl; statusCol = "GREEN"; act = "UNEQUIP"; actOn = true; actKind = 1; }
                    else { status = "OWNED"; for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; } act = _sinSlots737 <= 0 ? "NO SLOT" : free < 0 ? "SLOTS FULL" : "EQUIP"; actOn = free >= 0; actKind = 2; }
                }
                else { status = "LOCKED - NOT FOUND"; statusCol = "RED"; act = "NOT FOUND"; }
            }
            if (status != null) MtTx(status, IX, 259, statusCol);
            // the divider sits in mt_scene; the EQUIP button is 236x24 (primary red, disabled = flat)
            if (MtButton("action", IX, EY, 236, 24, act, actOn, "p", true) && sid >= 0 && sid == _sinSel737)
            {
                var d = HSins.All[_sinSel737];
                if (actKind == 1) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); HellordAudio.Play("ui_back"); }
                else if (free >= 0) { _sinEq737[free] = d.id; SinSave737(); HellordAudio.Play("ui_confirm"); }
            }
        }
        // ---------------------------------------------------------------- unlock confirm (asks first; one debit + one save per confirm)
        void MtModal()
        {
            if (_mtAsk < 0) return; int i = _mtAsk; var ev = Event.current;
            int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1);
            const int BX = 208, BY = 138, BW = 224, BH = 84;
            MtFill(0, 0, 640, 360, new Color(0, 0, 0, 0.62f));
            MtFill(BX - 2, BY - 2, BW + 4, BH + 4, MtC("DEEP")); MtFill(BX - 1, BY - 1, BW + 2, BH + 2, MtC("B2")); MtFill(BX, BY, BW, BH, MtC("DEEP")); MtFill(BX + 1, BY + 1, BW - 2, BH - 2, MtC("INK"));
            string ti = "UNLOCK SLOT " + (i + 1) + "?"; MtDisp(ti, BX + (BW - MtW(ti, true)) / 2, BY + 10, MT_TAB_N, "N");
            string co = sc + " SOULS + " + gc + " SIGIL" + (gc == 1 ? "" : "S"); MtTxC(co, BX + BW / 2, BY + 30, "AMBER"); MtTxC("THIS CANNOT BE UNDONE.", BX + BW / 2, BY + 42, "DIM");
            bool guard = Time.unscaledTime - _mtAskT < 0.3f;
            if (MtButton("askno", BX + 28, BY + 58, 60, 20, "CANCEL", !guard, "s", false, true)) { _mtAsk = -1; HellordAudio.Play("ui_back"); return; }
            if (MtButton("askyes", BX + BW - 88, BY + 58, 60, 20, "UNLOCK", !guard, "ps", true, true) || (!guard && ev.type == EventType.KeyDown && (ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.KeypadEnter)))
            {
                if (_souls51 >= sc && _sigils51 >= gc && i == _sinSlots737) { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); MtMsg("SIN SLOT " + (i + 1) + " UNLOCKED", "AMBER", 2.4f); HellordAudio.Play("pact_unlock"); }
                _mtAsk = -1; if (ev.type == EventType.KeyDown) ev.Use();
            }
        }

        // ================================================================ sconce fire
        static uint MtHash(uint a, uint b, uint c) { unchecked { uint h = a * 374761393u + b * 668265263u + c * 2246822519u; h = (h ^ (h >> 13)) * 1274126177u; return h ^ (h >> 16); } }
        static float MtRnd(uint a, uint b, uint c) { return (MtHash(a, b, c) & 0xFFFFFF) / 16777216f; }
        static float MtSmooth(float a, float b, float x) { x = Mathf.Clamp01((x - a) / (b - a)); return x * x * (3 - 2 * x); }
        static readonly string[] MT_EMBER = { "YELLOW", "AMBER", "ORANGE", "RED", "DRED" };
        void MtFire(float t)
        {
            const int RIM = 214; int[] cxs = { 21, 619 }; string[] sd = { "L", "R" }; int fi = MtFireOff ? 0 : Mathf.FloorToInt(t / 0.09f);
            // smooth flicker 0..1 (sum of incommensurate sines per side)
            float[] n = new float[2];
            for (int s = 0; s < 2; s++) { float ph = s * 1.7f; n[s] = MtFireOff ? 0.5f : Mathf.Clamp01(0.5f + 0.25f * Mathf.Sin(t * 5.3f + ph) + 0.15f * Mathf.Sin(t * 11.1f + ph * 2.1f) + 0.10f * Mathf.Sin(t * 2.1f + ph * 0.7f)); }
            var orange = (Color)MtC("ORANGE");
            for (int s = 0; s < 2; s++) { int cx = cxs[s]; MtSpr("halo", cx - 40, RIM - 18 - 56, Mathf.Lerp(0.13f, 0.27f, n[s]), orange); }
            MtBlit(_mtScene, 0, 0, 640, 360, 0, 0);
            for (int s = 0; s < 2; s++)
            {
                int lv = Mathf.Clamp(Mathf.RoundToInt(n[s] * (MenuThemeData.LightLevels - 1)), 0, MenuThemeData.LightLevels - 1); string ln = "light" + sd[s] + lv; if (s == 0) MenuThemeLightLevel = lv;
                var lp = MenuThemeData.LightPos[ln]; MtSpr(ln, lp[0], lp[1]);
            }
            for (int s = 0; s < 2; s++)
            {
                int cx = cxs[s], f = (fi + s * 5) % MenuThemeData.FlameFrames; if (s == 0) MenuThemeFlameFrame = f;
                MtSpr("flame" + sd[s] + f.ToString("00"), cx - 13, RIM + 1 - MenuThemeData.FlameH);   // behind the heap and the bowl's front rim
                MtSpr("coal" + ((fi + s * 2) % 3), cx - 13, RIM - 3); MtSpr("bowl", cx - 16, RIM);
            }
            if (!MtFireOff) for (int s = 0; s < 2; s++) MtEmbers(s, cxs[s], RIM, t);
        }
        /// <summary>Tiny embers: spawn inside the flame, rise ~40-60 px with a slow sway, half of them split in two near mid-life, cool YELLOW -> DRED, fade in/out smoothly (alpha), 1 px.</summary>
        void MtEmbers(int side, int cx, int rim, float t)
        {
            for (int j = 0; j < 8; j++)
            {
                uint a = (uint)(side * 16 + j); float per = 1.7f + 1.1f * MtRnd(a, 0, 99), ph = t / per + MtRnd(a, 0, 98); int cyc = Mathf.FloorToInt(ph); float f = ph - cyc; uint c = (uint)(cyc + 1000);
                float x0 = cx + (MtRnd(a, c, 1) - 0.5f) * 14f, y0 = rim - 16 - MtRnd(a, c, 2) * 12f, rise = 40f + 22f * MtRnd(a, c, 3), sway = 1.4f + 1.6f * MtRnd(a, c, 4), wind = (MtRnd(a, c, 5) - 0.5f) * 7f;
                bool split = MtRnd(a, c, 6) < 0.55f; float x = x0 + Mathf.Sin((f * 1.1f + MtRnd(a, c, 7)) * 6.2831853f) * sway + wind * f, y = y0 - rise * f;
                float al = Mathf.Clamp01(f / 0.07f) * (1f - MtSmooth(0.5f, 1f, f)) * 0.95f; string col = MT_EMBER[f < 0.10f ? 0 : f < 0.28f ? 1 : f < 0.50f ? 2 : f < 0.75f ? 3 : 4];
                var cc = (Color)MtC(col); cc.a = al;
                if (split && f > 0.42f)
                {
                    float sp = (f - 0.42f) / 0.58f * 3.2f; MenuThemeEmberCount += 2;
                    MtFill(Mathf.RoundToInt(x - sp), Mathf.RoundToInt(y + sp * 0.3f), 1, 1, cc); MtFill(Mathf.RoundToInt(x + sp), Mathf.RoundToInt(y - sp * 0.25f), 1, 1, cc);
                }
                else { MenuThemeEmberCount++; MtFill(Mathf.RoundToInt(x), Mathf.RoundToInt(y), 1, 1, cc); }
            }
        }
    }
}
