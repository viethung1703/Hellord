using UnityEngine;

// step64 TRIAL (VH t1091/t1092): Sins panel drawn on AI-generated demon-scroll art (Art/previews/step64_generated_scroll_ui).
// Reversible: set SinArt64On = false (falls back to DrawSinPanel737 unchanged) or delete this file + ui_s64_* textures.
// Mechanics (owned/equip/unequip/unlock/filter/save) reuse the same 737 fields/helpers; only drawing + hit rects differ.
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool SinArt64On = false; // step74: superseded by HellordSinsArt74 (stone guardian)
        Texture2D _s64bg, _s64tl, _s64tm, _s64tr; bool _s64tried;
        static readonly Color S64_INK = new Color32(0x2e, 0x1b, 0x12, 255), S64_RED = new Color32(0x86, 0x12, 0x12, 255), S64_DIM = new Color32(0x6e, 0x58, 0x44, 255),
            S64_SAND = new Color32(0xf4, 0xe0, 0xb8, 255), S64_GOLD = new Color32(0xff, 0xd8, 0x5a, 255);
        const float S64 = 700f / 1280f; // generated art is 1280x720 = 1 texel per screen px at 1280x720
        static float Q64(float v) => v * S64;
        static readonly int[] S64_GX = { 143, 200, 257, 314, 371, 428, 484, 541, 598, 654, 711, 768 }; // baked cell left edges (px)
        static readonly int[] S64_GY = { 279, 331, 382, 433, 484, 535 };
        const int S64_CW = 51, S64_CH = 48;

        bool SinArt64Ready()
        {
            if (!_s64tried) { _s64tried = true; _s64bg = UiTex("s64_bg"); _s64tl = UiTex("s64_tab_l"); _s64tm = UiTex("s64_tab_m"); _s64tr = UiTex("s64_tab_r"); }
            return _s64bg && _s64tl && _s64tm && _s64tr;
        }
        void Tex64(Texture2D t, float px, float py, float pw, float ph) { GUI.DrawTexture(new Rect(Q64(px), Q64(py), Q64(pw), Q64(ph)), t); }
        void Box64(float px, float py, float pw, float ph, Color c) => R53(Q64(px), Q64(py), Q64(pw), Q64(ph), c);
        bool Hit64(float px, float py, float pw, float ph)
        {   // autotest hook: _s64Fake = click position in art px, consumed once on next repaint (exercises the same hit rects/logic)
            if (_s64Click.HasValue && new Rect(px, py, pw, ph).Contains(_s64Click.Value)) { _s64Click = null; return true; }
            return Click53(Q64(px), Q64(py), Q64(pw), Q64(ph));
        }
        public Vector2? _s64Fake; Vector2? _s64Click;
        bool Over64(float px, float py, float pw, float ph) => Over51(Q64(px), Q64(py), Q64(pw), Q64(ph));
        void TxC64(string s, float pcx, float py, Color c) => TxC69(s, Q64(pcx), Q64(py), c);
        void Icon64(SinDef d, float px, float py, int scale, Color tint)
        {
            int ic = SinIconCol902(d.name); if (ic < 0 || _tSinIcons902 == null) { TxC64(Fit69(d.name, 40), px + 10 * scale, py + 6 * scale, S64_INK); return; }
            float cell = Mathf.Max(1, HellordTuning.I.sinIconCell902), tw = _tSinIcons902.width; GUI.color = tint;
            GUI.DrawTextureWithTexCoords(new Rect(Q64(px), Q64(py), Q64(20 * scale), Q64(20 * scale)), _tSinIcons902, new Rect(ic * cell / tw, 0, cell / tw, 1)); GUI.color = Color.white;
        }
        void Outline64(float px, float py, float pw, float ph, Color c, int t = 3) { Box64(px, py, pw, t, c); Box64(px, py + ph - t, pw, t, c); Box64(px, py, t, ph, c); Box64(px + pw - t, py, t, ph, c); }

        void DrawSinPanel64()
        {
            SinLoad737();
            _s64Click = null; if (_s64Fake.HasValue && Event.current.type == EventType.Repaint) { _s64Click = _s64Fake; _s64Fake = null; }
            R53(0, 0, 700, 392, new Color(0, 0, 0, 0.82f)); GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, 0, 700, 392), _s64bg);
            if (Hit64(1025, 65, 75, 77)) { _sinPanel737 = false; return; } // baked close seal
            // ---- equip slots (baked frames 104px) ----
            _s64NextCost = null; int[] sx = { 170, 295, 420 }; const int sy = 100, ss = 104;
            for (int i = 0; i < 3; i++)
            {
                float x = sx[i]; bool unl = i < _sinSlots737 && i < SlotsMax737;
                if (unl)
                {
                    int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null;
                    if (d != null) { Icon64(d, x + 22, sy + 22, 3, Color.white); if (_sinSel737 == d.id) Outline64(x + 10, sy + 10, ss - 20, ss - 20, S64_RED); if (Hit64(x, sy, ss, ss)) _sinSel737 = d.id; }
                    else TxC64("EMPTY", x + ss / 2, sy + 46, S64_DIM);
                }
                else if (i < SlotsMax737)
                {
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc, can = next && afford;
                    Box64(x + 12, sy + 12, ss - 24, ss - 24, new Color(0.12f, 0.06f, 0.05f, can && Over64(x, sy, ss, ss) ? 0.55f : 0.72f));
                    TxC64("LOCKED", x + ss / 2, sy + 36, S64_SAND);
                    TxC64(can ? "CLICK" : next ? "NEED" : "", x + ss / 2, sy + 56, can ? S64_GOLD : S64_SAND);
                    if (next) { _s64NextCost = sc + " SOULS + " + gc + " SIGIL" + (gc == 1 ? "" : "S"); _s64NextOk = afford; }
                    if (can && Hit64(x, sy, ss, ss)) { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f; }
                }
            }
            // wallet / collection (no heading, per VH t1080)
            Tx69("COLLECTED " + SinOwnedCount737 + "/" + HSins.Count, Q64(560), Q64(108), S64_INK);
            Tx69("SOULS " + _souls51 + "  SIGILS " + _sigils51, Q64(560), Q64(134), S64_INK);
            if (_s64NextCost != null) { Tx69("NEXT SLOT COSTS", Q64(560), Q64(164), S64_DIM); Tx69(_s64NextCost, Q64(560), Q64(184), _s64NextOk ? S64_RED : S64_DIM); }
            // ---- filters: actual game filters, tab caps unstretched, 1px middle column tiled ----
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" }; float tx = 144;
            for (int i = 0; i < 4; i++)
            {
                float w = Mathf.Ceil(TW69(fl[i]) / S64) + 44; bool sel = _sinFilter737 == i, ov = Over64(tx, 217, w, 51);
                GUI.color = sel ? Color.white : ov ? new Color(0.92f, 0.9f, 0.88f) : new Color(0.8f, 0.78f, 0.76f);
                Tex64(_s64tl, tx, 217, 24, 51); Tex64(_s64tm, tx + 24, 217, w - 48, 51); Tex64(_s64tr, tx + w - 24, 217, 24, 51); GUI.color = Color.white;
                if (sel) Box64(tx + 14, 256, w - 28, 3, S64_RED);
                TxC64(fl[i], tx + w / 2, 233, sel ? S64_RED : S64_INK);
                if (Hit64(tx, 217, w, 51)) { _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; }
                tx += w + 10;
            }
            // ---- collection grid: baked 12x6 cells ----
            var list = SinFiltered737(); int cols = S64_GX.Length, per = cols * S64_GY.Length, pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1);
            for (int k = 0; k < per; k++)
            {
                int li = _sinPage737 * per + k; if (li >= list.Count) break; var d = list[li];
                float x = S64_GX[k % cols], y = S64_GY[k / cols]; bool own = _sinOwned737[d.id], eq = SinEquipped737(d.id);
                if (Over64(x, y, S64_CW, S64_CH)) Box64(x + 3, y + 3, S64_CW - 6, S64_CH - 6, new Color(0.55f, 0.1f, 0.08f, 0.18f));
                if (own) Icon64(d, x + 5, y + 4, 2, Color.white); else TxC64("?", x + S64_CW / 2f, y + 18, S64_DIM);
                if (eq) { Box64(x + S64_CW - 13, y + 4, 9, 9, S64_GOLD); Outline64(x + S64_CW - 13, y + 4, 9, 9, S64_INK, 1); }
                if (_sinSel737 == d.id) Outline64(x, y, S64_CW, S64_CH, S64_RED);
                if (Hit64(x, y, S64_CW, S64_CH)) _sinSel737 = d.id;
            }
            if (pages > 1)
            {
                if (_sinPage737 > 0 && Hit64(143, 592, 40, 30)) _sinPage737--; TxC64("<", 163, 600, _sinPage737 > 0 ? S64_INK : S64_DIM);
                TxC64((_sinPage737 + 1) + "/" + pages, 220, 600, S64_INK);
                if (_sinPage737 < pages - 1 && Hit64(257, 592, 40, 30)) _sinPage737++; TxC64(">", 277, 600, _sinPage737 < pages - 1 ? S64_INK : S64_DIM);
            }
            // ---- details (inside baked detail panel 900..1145 x 165..500) ----
            const float dx = 906, dw = 234; string act = null; bool actOn = false; int actKind = 0;
            if (_sinSel737 >= 0)
            {
                var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id]; var tc = HSins.TierCol[(int)d.tier];
                if (own) Icon64(d, dx, 170, 2, Color.white); else TxC64("?", dx + 20, 182, S64_DIM);
                Tx69(HSins.TierName[(int)d.tier], Q64(dx + 52), Q64(170), S64_RED); Tx69("NO. " + (d.id + 1) + " OF " + HSins.Count, Q64(dx + 52), Q64(192), S64_DIM);
                float y = Q64(220), X = Q64(dx), W = Q64(dw);
                if (own)
                {
                    y = Wrap64(d.name, X, y, W, S64_RED);
                    if (d.vn != "") { y = Wrap64("(" + d.vn + ")", X, y + 1, W, S64_DIM); }
                    var q = _sinRoll737[d.id]; float ql = HSins.Quality(q); y = Wrap64(HSins.QualityTxt(q, d), X, y + 2, W, ql >= ST737.sinPerfectAt737 ? S64_RED : S64_INK);
                    y += 2; foreach (var l in HSins.Lines(d, q)) y = Wrap64(l, X, y, W, l.StartsWith("-") ? S64_RED : l.StartsWith("EFFECT NOT") ? S64_DIM : S64_INK) + 1;
                    _s64DetailBottom = y / S64;
                    if (SinEquipped737(d.id)) { act = "UNEQUIP"; actOn = true; actKind = 1; }
                    else
                    {
                        int free = -1; for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; }
                        act = _sinSlots737 <= 0 ? "NO SLOT UNLOCKED" : free < 0 ? "SLOTS FULL" : "EQUIP"; actOn = free >= 0; actKind = 2; _s64Free = free;
                    }
                }
                else { y = SinWrap737("UNKNOWN SIN", X, y, W, S64_INK); SinWrap737("Not found yet. Sins drop from demons and bosses.", X, y + 4, W, S64_DIM); _s64DetailBottom = 0; }
            }
            else { SinWrap737("Select a sin.", Q64(dx), Q64(246), Q64(dw), S64_DIM); _s64DetailBottom = 0; }
            // ribbons (baked): top = sin action, bottom = BACK (closes)
            if (act != null)
            {
                TxC64(act, 1018, 522, actOn ? S64_SAND : new Color(0.75f, 0.6f, 0.55f));
                if (actOn && Hit64(912, 508, 213, 44))
                {
                    var d = HSins.All[_sinSel737];
                    if (actKind == 1) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); }
                    else if (_s64Free >= 0) { _sinEq737[_s64Free] = d.id; SinSave737(); }
                }
            }
            TxC64("BACK", 1018, 570, S64_SAND);
            if (Hit64(912, 556, 213, 44)) { _sinPanel737 = false; return; }
            if (_sinMsgT737 > 0) TxC64(_sinMsg737, 480, 600, S64_RED);
        }
        float Wrap64(string s, float x, float y, float w, Color c)
        {   // like SinWrap737 but 9-unit line height for the narrow scroll detail column
            int m = Mathf.Max(8, Mathf.FloorToInt((w * K69 + 1) / 6)); string cur = "";
            foreach (var word in Sanitize(s).Split(' '))
            {
                if (cur.Length > 0 && cur.Length + 1 + word.Length > m) { Tx69(cur, x, y, c); y += 10; cur = word; }
                else cur = cur.Length > 0 ? cur + " " + word : word;
            }
            if (cur.Length > 0) { Tx69(cur, x, y, c); y += 10; }
            return y;
        }
        string _s64NextCost; bool _s64NextOk; float _s64DetailBottom; int _s64Free = -1;
    }
}
