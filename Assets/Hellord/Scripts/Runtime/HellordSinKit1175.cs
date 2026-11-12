using UnityEngine;
using System.Collections.Generic;

// t1175 v0.0.98 (VH "try this and build i let see"): Sins screen wired to AI KIT v7
// (Art/previews/t1162_ai_ui_kit/v7/MOCKUP_NOT_RUNTIME_AI_KIT_v7_interaction_x3.png).
// v2 layout (native 480x270 px, gutters/inner padding >= 4), AI-stone 9-slices ui_k98_* (Resources/HellordArt),
// AI-painted button states (v7 sheet C: normal/hover/selected/pressed/disabled), cream labels, carved glowing label
// for selected/pressed, Sin slots with the v2 red frames, ember pip. Mechanics/feel/save: unchanged (HellordSinFeel1148).
// Default ON. Fallback: -sinkit98off (v0.0.97 layout). -sinkit98fit = non-integer fit scale instead of integer scale.
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool Kit98On = System.Environment.CommandLine.Contains("-sinkit98on"); // t1176: opt-in only (VH rejected the v0.0.98 kit); needs -stoneui too
        public static bool Kit98IntScale = !System.Environment.CommandLine.Contains("-sinkit98fit");
        public static readonly Rect K98Modal = new Rect(140, 92, 200, 86), K98No = new Rect(158, 154, 78, 14), K98Yes = new Rect(244, 154, 78, 14);
        static readonly Rect K98Tabs = new Rect(69, 89, 82, 14), K98Prev = new Rect(78, 116, 38, 14), K98Next = new Rect(244, 116, 38, 14),
            K98Act = new Rect(304, 202, 96, 14), K98Back = new Rect(304, 220, 96, 14);
        public static Rect K98Tab(int i) => new Rect(69 + i * 86, 89, 82, 14);
        public static Rect K98Tile(int k) => new Rect(78 + (k % 8) * 26, 134 + (k / 8) * 26, 22, 22);
        public static Rect K98Slot(int i) => new Rect(69 + i * 40, 49, 36, 36);
        static readonly Color K98_CREAM = new Color32(232, 218, 186, 255), K98_WARM = new Color32(240, 214, 176, 255), K98_EDGE = new Color32(128, 26, 22, 255),
            K98_DIAG = new Color32(70, 16, 12, 255), K98_PIP = new Color32(162, 38, 51, 255), K98_WELL = new Color32(15, 15, 14, 255), K98_VOID = new Color32(10, 9, 8, 255);
        public int K98Scale; public bool K98Integer; bool _k98Draw; public string K98LastState; // diagnostics for tests
        readonly Dictionary<string, Texture2D> _k98 = new Dictionary<string, Texture2D>();
        Texture2D K98T(string n) { if (_k98.TryGetValue(n, out var t)) return t; t = UiTex("k98_" + n); _k98[n] = t; return t; }
        public bool Kit98Ready => K98T("btn_normal") && K98T("panel_frame") && K98T("slot_locked");

        /// <summary>Integer view scale: every native art px = s whole screen px (point filtered). Falls back to fit scale when the integer
        /// scale would hide the content (visible < 420x236 native) or would leave it tiny.</summary>
        bool K98ViewScale(Matrix4x4 G)
        {
            if (!Kit98IntScale) return false;
            float W = Screen.width, H = Screen.height, fit = Mathf.Min(W / View56W, H / (View56W * 392f / 700f));
            int s = Mathf.Max(1, Mathf.RoundToInt(fit)); if (W / s < 420 || H / s < 236) s = Mathf.Max(1, Mathf.FloorToInt(fit));
            if (s < 2 || s < fit * 0.7f) return false;
            float g00 = G.m00, g03 = G.m03, g13 = G.m13; if (g00 <= 0) return false;
            float ox = Mathf.Round(W / 2 - s * View56Axis), oy = Mathf.Round(H / 2 - s * View56CY);
            _vs56 = s / (K74 * g00); _vox56 = (g03 - ox) * K74 / s; _voy56 = (g13 - oy) * K74 / s; K98Scale = s; K98Integer = true; return true;
        }

        // ---- drawing primitives (whole screen px)
        void Nine98(string n, int l, int t, int r, int b, float x, float y, float w, float h)
        {
            var tex = K98T(n); if (!tex || w <= 0 || h <= 0) return;
            Scr56Begin(out var m); var d = ScrR56(x, y, w, h); float kx = d.width / w, ky = d.height / h; int W = tex.width, H = tex.height;
            var cs = new List<Vector4>(); var rs = new List<Vector4>(); // (src, srcLen, dst, dstLen) in native px
            cs.Add(new Vector4(0, l, 0, l)); int mw = W - l - r; for (float u = l; u < w - r - 0.01f; u += mw) cs.Add(new Vector4(l, Mathf.Min(mw, w - r - u), u, Mathf.Min(mw, w - r - u))); cs.Add(new Vector4(W - r, r, w - r, r));
            rs.Add(new Vector4(0, t, 0, t)); int mh = H - t - b; for (float v = t; v < h - b - 0.01f; v += mh) rs.Add(new Vector4(t, Mathf.Min(mh, h - b - v), v, Mathf.Min(mh, h - b - v))); rs.Add(new Vector4(H - b, b, h - b, b));
            GUI.color = Color.white;
            foreach (var c in cs) { if (c.y <= 0) continue; float x0 = d.x + Mathf.Round(c.z * kx), x1 = d.x + Mathf.Round((c.z + c.w) * kx);
                foreach (var rr in rs) { if (rr.y <= 0) continue; float y0 = d.y + Mathf.Round(rr.z * ky), y1 = d.y + Mathf.Round((rr.z + rr.w) * ky);
                    GUI.DrawTextureWithTexCoords(new Rect(x0, y0, x1 - x0, y1 - y0), tex, new Rect(c.x / W, 1 - (rr.x + rr.y) / H, c.y / W, rr.y / H)); } }
            GUI.matrix = m;
        }
        float Fp98 => 1f / (K69 * K74 * _txS56);       // one font px in native px
        float Th98 => 7 * Fp98;                         // glyph height in native px
        void Tx98(string s, float x, float y, string mode)
        {
            if (mode != "on" && mode != "pressed") { Tx74(s, x, y, mode == "hover" ? K98_WARM : mode == "disabled" ? (Color)S74_DIM : K98_CREAM); return; }
            float px = U74(x), py = U74(y); Tx56Map(ref px, ref py); var m = GUI.matrix; GUI.matrix = _base53; float X = Mathf.Round(px * K69), Y = Mathf.Round(py * K69); string ss = Sanitize(s);
            for (int i = 0; i < 4; i++) Text(ss, X + (i & 1) * 2 - 1, Y + (i >> 1) * 2 - 1, K98_DIAG, false, 1);   // carved glow: deep-red diagonals,
            Text(ss, X - 1, Y, K98_EDGE, false, 1); Text(ss, X + 1, Y, K98_EDGE, false, 1); Text(ss, X, Y - 1, K98_EDGE, false, 1); Text(ss, X, Y + 1, K98_EDGE, false, 1); // ember edge,
            Text(ss, X, Y, K98_CREAM, false, 1); GUI.matrix = m;                                                        // cream core
        }
        void Lbl98(string s, Rect r, float faceH, string mode, float dy = 0) => Tx98(s, r.x + (r.width - TW74(s)) / 2, r.y + (faceH - Th98) / 2 + dy, mode);
        void TxC98(string s, float cx, float y, Color c) => Tx74(s, cx - TW74(s) / 2, y, c);
        string BtnState98(string id, Rect r, bool enabled, bool selected)
        {
            bool over = Over74(r.x, r.y, r.width, r.height) && (!Confirm49Open || id.StartsWith("c49"));
            if (!enabled) return "disabled";
            if (Event.current.type == EventType.Repaint && over) _f48Hover = id;
            if (SinFeel1148On && over && _f48Down == id) return "pressed";
            return selected ? "on" : over ? "hover" : "normal";
        }
        /// <summary>Draws the AI-painted state face + label (no code outlines). Returns the state drawn.</summary>
        string DrawBtn98(string id, Rect r, string label, bool enabled, bool selected, bool arrow = false)
        {
            string st = BtnState98(id, r, enabled, selected);
            Nine98("btn_" + st, 7, 0, 7, 0, r.x, r.y, r.width, r.height);
            if (SinFeel1148On && _f48Deny == id && Now48 - _f48DenyT < SinFeelTune.DenyButtonS) Wash48(r.x + 1, r.y + 1, r.width - 2, 9, A48(new Color(0.84f, 0.27f, 0.22f), SinFeelTune.DenyWash * (1 - (Now48 - _f48DenyT) / SinFeelTune.DenyButtonS)));
            float dy = st == "pressed" ? 1 : 0;
            if (label != null)
            {
                if (!arrow) Lbl98(label, r, 10, st, dy);
                else
                {
                    float bw = TW74(label) + 6, bx = r.x + (r.width - bw) / 2, ty = r.y + (10 - Th98) / 2 + dy; Color ac = st == "hover" ? K98_WARM : K98_CREAM;
                    for (int j = 0; j < 3; j++) Box74(bx + 1 + j * 0.67f, ty + Th98 / 2 - 0.33f - j * 0.67f, 0.67f, (2 * j + 1) * 0.67f, ac);
                    Tx98(label, bx + 6, ty, st);
                }
            }
            if (id == "pageNEXT" || id == "action" || id == "back" || id.StartsWith("tab")) K98LastState = id + ":" + st;
            return st;
        }
        bool Btn98(string id, Rect r, string label, bool enabled, bool selected) { DrawBtn98(id, r, label, enabled, selected); return Btn48(id, r.x, r.y, r.width, r.height, enabled); }
        void Icon98(SinDef d, float cx, float cy, float nativeSize)
        {
            int ic = SinIconCol902(d.name); if (ic < 0 || _tSinIcons902 == null) return;
            if (!_sinIconsNPTried1153) { _sinIconsNPTried1153 = true; _tSinIconsNP1153 = UiTex("v53_sin_icons_20_noplate"); }
            var atlas = SinIconsNoPlate1153 && _tSinIconsNP1153 != null && _tSinIconsNP1153.width == _tSinIcons902.width ? _tSinIconsNP1153 : _tSinIcons902;
            float cell = Mathf.Max(1, HellordTuning.I.sinIconCell902); int mul = Mathf.Max(1, Mathf.RoundToInt(nativeSize / 20f * _k56)); float sz = 20 * mul; // whole texel multiple
            Scr56Begin(out var m); var c = Scr56(cx, cy); GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(new Rect(Mathf.Round(c.x - sz / 2), Mathf.Round(c.y - sz / 2), sz, sz), atlas, new Rect(ic * cell / atlas.width, 0, cell / atlas.width, 1));
            GUI.matrix = m;
        }
        void Wrap98(List<KeyValuePair<string, Color>> outL, string s, float w, Color c)
        {
            string cur = "";
            foreach (var word in Sanitize(s).Split(' '))
            {
                string t = cur.Length > 0 ? cur + " " + word : word;
                if (cur.Length > 0 && TW74(t) > w) { outL.Add(new KeyValuePair<string, Color>(cur, c)); cur = word; } else cur = t;
            }
            if (cur.Length > 0) outL.Add(new KeyValuePair<string, Color>(cur, c));
        }

        // ---- the screen
        void DrawSinPanelK98()
        {
            _full56 = _bgFull56; _vox56 = _bgVox56; GUI.matrix = _full56; _k98Draw = true;
            try { DrawSinK98Body(); } finally { _k98Draw = false; }
        }
        void DrawSinK98Body()
        {
            // symmetric dark frame outside the clean art window (x10..469, y7..262), only visible when the integer scale shows more than the art
            Fill56(-600, -400, 610, 1100, K98_VOID); Fill56(469, -400, 600, 1100, K98_VOID); Fill56(10, -400, 459, 407, K98_VOID); Fill56(10, 262, 459, 400, K98_VOID);
            // ---- slots
            for (int i = 0; i < 3; i++)
            {
                var r = K98Slot(i); float x = r.x, y = r.y, s = r.width; bool unl = i < _sinSlots737 && i < SlotsMax737;
                if (unl)
                {
                    int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null; bool sel = d != null && _sinSel737 == d.id;
                    Nine98(sel ? "slot_selected" : "slot_normal", 0, 0, 0, 0, x, y, s, s);
                    if (d != null) { Icon98(d, x + s / 2, y + s / 2, 20); if (Hit74(x, y, s, s)) _sinSel737 = d.id; }
                    else if (!UnlockReveal48(i, x, y, s)) Lbl74("EMPTY", x, y, s, s, S74_DIM, 2);
                }
                else if (i < SlotsMax737)
                {
                    Nine98("slot_locked", 0, 0, 0, 0, x + DenyShake48(i), y, s, s);
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
                    if (LockSocket48(i, x, y, s, next, afford, sc, gc)) { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f; Unlocked48(i); }
                }
            }
            // ---- currency + cost plaques
            _currencyFit91 = true;
            for (int k = 0; k < 2; k++)
            {
                bool sigil = k == 1; float x = sigil ? 301 : 189, y = 49, w = 108, h = 16; Nine98("plaque_frame", 3, 3, 3, 3, x, y, w, h);
                var t = T53(sigil ? "sigil" : "soul"); if (t) { Scr56Begin(out var m); GUI.color = Color.white; GUI.DrawTexture(ScrR56(x + 5, y + 3.5f, 9, 9), t); GUI.matrix = m; }
                string lb = sigil ? "SIGILS" : "SOULS"; int n = Shown48(sigil, sigil ? _sigils51 : _souls51); string v = n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                float ty = y + (h - Th98) / 2, tx = x + 18; Tx74(lb, tx, ty, K98_CREAM); Tx74(v, tx + TW74(lb) + 4, ty, Rolling48 ? K98_WARM : K98_CREAM);
            }
            { float x = 189, y = 69, w = 220, h = 16; Nine98("plaque_frame", 3, 3, 3, 3, x, y, w, h); float ty = y + (h - Th98) / 2;
              string slots = "SLOTS " + _sinSlots737 + "/" + SlotsMax737, req; Color rc = K98_CREAM;
              if (SlotsMax737 <= 0) req = "NO SLOTS AVAILABLE"; else if (_sinSlots737 >= SlotsMax737) req = "ALL OPEN - READY TO EQUIP";
              else { int sc = Arr737(ST737.sinSlotSoulCost737, _sinSlots737, 100), gc = Arr737(ST737.sinSlotSigilCost737, _sinSlots737, 1); req = "NEXT " + sc.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " SOULS + " + gc.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " SIGILS"; if (TW74(slots) + 10 + TW74(req) > w - 12) req = CostLine88(sc, gc); }
              if (HdrOverride48(out var hs, out var hc)) { req = hs; rc = hc == (Color)S74_RED ? (Color)S74_RED : K98_WARM; }
              Tx74(slots, x + 6, ty, K98_CREAM); Tx74(req, x + w - 6 - TW74(req), ty, rc); }
            // ---- tabs
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" };
            for (int i = 0; i < 4; i++) { bool sel = _sinFilter737 == i; if (Btn98("tab" + i, K98Tab(i), fl[i], true, sel)) { if (!sel) HellordAudio.Play("ui_confirm"); _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; } }
            // ---- collection panel: page rail on a stone plaque, 8x4 sockets
            Nine98("panel_frame", 5, 5, 5, 5, 69, 107, 222, 136); Nine98("panel_frame", 5, 5, 5, 5, 295, 107, 114, 136);
            var list = SinFiltered737(); const int per = 32; int pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1); _s77Pages = pages; _s77Count = list.Count;
            Nine98("plaque_frame", 3, 3, 3, 3, 120, 116, 120, 14);
            string pc = "PAGE " + (_sinPage737 + 1) + " OF " + pages + "  OWNED " + SinOwnedCount737 + "/" + HSins.Count; TxC98(pc, 180, 116 + (14 - Th98) / 2, K98_CREAM);
            if (Btn98("pagePREV", K98Prev, "PREV", _sinPage737 > 0, false)) { _sinPage737--; if (SinFeel1148On) HellordAudio.Play("altar_carousel"); }
            if (Btn98("pageNEXT", K98Next, "NEXT", _sinPage737 < pages - 1, false)) { _sinPage737++; if (SinFeel1148On) HellordAudio.Play("altar_carousel"); }
            for (int k = 0; k < per; k++)
            {
                var r = K98Tile(k); int li = _sinPage737 * per + k; var d = li < list.Count ? list[li] : null; bool sel = d != null && _sinSel737 == d.id;
                Nine98(sel ? "tile_selected" : "tile_normal", 0, 0, 0, 0, r.x, r.y, r.width, r.height);
                if (d == null) continue;
                if (_sinOwned737[d.id]) Icon98(d, r.x + 11, r.y + 11, 40f / 3); else Lbl74("?", r.x, r.y, r.width, r.height, S74_DIM, 0);
                if (SinEquipped737(d.id)) Fill56(r.x + r.width - 6, r.y + 3, 2, 2, K98_PIP);
                if (Hit74(r.x, r.y, r.width, r.height)) { _sinSel737 = d.id; _s74Page = 0; }
            }
            // ---- detail column: header plaque, text well, action, back
            Nine98("plaque_frame", 3, 3, 3, 3, 304, 116, 96, 18);
            Nine98("well_frame", 6, 6, 6, 6, 304, 138, 96, 60); Fill56(310, 144, 84, 48, K98_WELL);
            string act = null; bool actOn = false; int actKind = 0, free = -1; var lines = new List<KeyValuePair<string, Color>>(); const float tx0 = 312, tw = 80;
            if (_sinSel737 >= 0)
            {
                var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id];
                if (own) Icon98(d, 308 + 20f / 3, 125, 40f / 3); else Lbl74("?", 304, 116, 18, 18, S74_DIM, 0);
                string tier = HSins.TierName[(int)d.tier] + " NO." + (d.id + 1); Tx74(tier, 326, 116 + (18 - Th98) / 2, S74_RED);
                if (own)
                {
                    Wrap98(lines, d.name, tw, K98_CREAM); var q = _sinRoll737[d.id]; Wrap98(lines, HSins.QualityTxt(q, d), tw, K98_CREAM);
                    foreach (var l in HSins.Lines(d, q)) Wrap98(lines, l, tw, l.StartsWith("-") ? S74_RED : l.StartsWith("EFFECT NOT") ? (Color)S74_DIM : K98_CREAM);
                    if (SinEquipped737(d.id)) { act = "UNEQUIP"; actOn = true; actKind = 1; }
                    else { for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; } act = _sinSlots737 <= 0 ? "NO SLOT UNLOCKED" : free < 0 ? "SLOTS FULL" : "EQUIP"; actOn = free >= 0; actKind = 2; }
                }
                else { Wrap98(lines, "UNKNOWN SIN", tw, K98_CREAM); Wrap98(lines, "Not found yet. Sins drop from demons and bosses.", tw, S74_DIM); act = "NOT FOUND"; }
            }
            else { Tx74("DETAILS", 310, 116 + (18 - Th98) / 2, S74_DIM); Wrap98(lines, "Select a sin from the collection to read it here.", tw, S74_DIM); act = "SELECT A SIN"; }
            const float lh = 8, ly0 = 146; int cap = 6, capP = lines.Count > cap ? cap - 1 : cap;
            _s74Pages = Mathf.Max(1, (lines.Count + capP - 1) / capP); _s74Page = Mathf.Clamp(_s74Page, 0, _s74Pages - 1); _s74Lines = lines.Count; _s74Cap = capP; _s74Overflow = false;
            for (int i = 0; i < capP; i++) { int li = _s74Page * capP + i; if (li >= lines.Count) break; Tx74(lines[li].Key, tx0, ly0 + i * lh, lines[li].Value); }
            int shown = Mathf.Min(capP, lines.Count - _s74Page * capP);
            if (_s74Pages <= 1 && _sinSel737 >= 0 && shown <= 4)
            {   // short text: the sin large in the free space (if it fits) + its state (mockup)
                var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id]; float iy = ly0 + shown * lh + 2 + 20f / 3;
                if (shown <= 3) { if (own) Icon98(d, 352, iy, 40f / 3); else Lbl74("?", 345, iy - 7, 14, 14, S74_DIM, 0); }
                int eqSlot = -1; for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) eqSlot = i;
                string st = !own ? "NOT FOUND" : eqSlot >= 0 ? "EQUIPPED - SLOT " + (eqSlot + 1) : "OWNED"; TxC98(st, 352, 190 - Th98, eqSlot >= 0 ? K98_CREAM : (Color)S74_DIM);
            }
            if (_s74Pages > 1)
            {
                string tp = "TEXT " + (_s74Page + 1) + "/" + _s74Pages; TxC98(tp, 352, 186, K98_CREAM);
                bool lOn = _s74Page > 0, rOn = _s74Page < _s74Pages - 1;
                Arrow87(318, 189, -1, lOn ? K98_CREAM : (Color)S74_DIM); Arrow87(386, 189, 1, rOn ? K98_CREAM : (Color)S74_DIM);
                if (Btn48("txtL", 310, 184, 18, 10, lOn)) _s74Page--; if (Btn48("txtR", 376, 184, 18, 10, rOn)) _s74Page++;
            }
            if (act != null && Btn98("action", K98Act, act, actOn, actOn))
            {
                var d = HSins.All[_sinSel737];
                if (actKind == 1) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); if (SinFeel1148On) HellordAudio.Play("ui_back"); }
                else if (free >= 0) { _sinEq737[free] = d.id; SinSave737(); if (SinFeel1148On) HellordAudio.Play("ui_confirm"); }
            }
            DrawBtn98("back", K98Back, "BACK TO MENU", true, false, true);
            if (BackBtn48(K98Back.x, K98Back.y, K98Back.width, K98Back.height)) { SinClose87(); return; }
            Confirm49Draw();
        }
        /// <summary>Modal in the kit style (called from Confirm49Draw while the kit screen draws). Out: the hit rects.</summary>
        void Confirm98Draw(Rect b, ref Rect yes, ref Rect no, float dy, string title, string cost, string l1, string l2)
        {
            yes = K98Yes; yes.y += dy; no = K98No; no.y += dy;
            Nine98("modal_frame", 5, 5, 5, 5, b.x, b.y, b.width, b.height);
            Nine98("plaque_frame", 3, 3, 3, 3, b.x + 10, b.y + 24, b.width - 20, 34);
            TxC98(title, b.x + b.width / 2, b.y + 11, K98_CREAM);
            TxC98(cost, b.x + b.width / 2, b.y + 29, K98_CREAM); TxC98(l1, b.x + b.width / 2, b.y + 39, S74_DIM); TxC98(l2, b.x + b.width / 2, b.y + 48, S74_DIM);
            DrawBtn98("c49no", no, "CANCEL", true, false); DrawBtn98("c49yes", yes, "UNLOCK", true, true);
        }
    }
}
