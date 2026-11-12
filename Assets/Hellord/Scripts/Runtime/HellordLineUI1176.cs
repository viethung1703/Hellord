using UnityEngine;

// t1176/t1177/t1178 (VH): "back to the normal UI before please, including pact" / "no fire, sins with simple line ui like pact" / "the one far away".
// DEFAULT = the original simple line UI, from before the stone / fire / Ox-Horse pillar Sins work:
//   Sins  = the t770u Infernal Sins panel (DrawSinPanel737, 2026-11-04; identical to Backups/step64_sins_trial_20261107_0015).
//   Pact  = the v0.0.53 Sacrifice Altar (DrawAltar51: Overview53 / PathScreen53 / YourPact53, before t511 Altar97 art).
// No fire, no stone art, no AI kit, no guardian eyes. Kept from later work (functions only, drawn in the same line style):
//   unlock confirm (t1149 state, 0.30 s + 8 frame input guard, Esc/Enter, click-outside cancel), exact single spend,
//   currency roll-down (Shown48), deny reason, release-activated buttons (drag off cancels), unlock flash, sounds, save format.
// Opt-in fallback to the stone / kit screens: launch with -stoneui (and -sinkit98on for the v0.0.98 AI kit).
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool LineUI1176 = !System.Environment.CommandLine.Contains("-stoneui");
        /// <summary>t1179: the Pact uses the pretty v0.0.95 Altar97 carousel + Necromancer double tree by default; -pactline = the v0.0.53 line altar.</summary>
        public static bool PactLine1176 = System.Environment.CommandLine.Contains("-pactline");
        bool Altar97On => PT.altar97 && !PactLine1176;
        string _l76Down, _l76Hover, _l76Deny; float _l76DenyT = -9; bool _l76HoverSeen;
        static readonly Color L76_PRESS = new Color(0.04f, 0.03f, 0.05f), L76_RED = new Color32(0xe4, 0x3b, 0x44, 255);

        /// <summary>Line button (look of SinBtn737). 1 = activated on release over it, -1 = disabled button clicked (deny), 0 = nothing.</summary>
        int LBtn76(string id, float x, float y, float w, float h, string s, bool on, bool sel = false)
        {
            var e = Event.current; bool live = Gate49 && !_block53; bool ov = live && Over51(x, y, w, h), down = on && ov && _l76Down == id;
            if (e.type == EventType.Repaint)
            {
                R53(x, y, w, h, down ? L76_PRESS : sel ? U_DRED : on && ov ? U_PLUM : U_INK); Ol53(x, y, w, h, on ? U_SLATE : U_LOCK);
                if (_l76Deny == id && Now48 - _l76DenyT < SinFeelTune.DenyButtonS) Ol53(x, y, w, h, A48(L76_RED, 1 - (Now48 - _l76DenyT) / SinFeelTune.DenyButtonS));
                float d = down ? 1 : 0; TxC69(s, x + w / 2 + d, y + (h - 8) / 2 + d, on ? U_SAND : U_GREY);
                if (on && ov) { _l76HoverSeen = true; if (_l76Hover != id) { _l76Hover = id; if (SinFeel1148On) HellordAudio.Play("ui_hover"); } }
            }
            if (!live) return 0;
#if HELLORD_AUTOTEST
            if (e.type == EventType.Repaint && Hit51(x, y, w, h)) { if (on) return 1; LDeny76(id); return -1; } // autotest fake click commits at once
#endif
            if (e.type == EventType.MouseDown && e.button == 0 && ov) { e.Use(); if (on) _l76Down = id; else { LDeny76(id); return -1; } return 0; }
            if (e.rawType == EventType.MouseUp && e.button == 0 && _l76Down == id) { _l76Down = null; if (ov && on) { e.Use(); return 1; } }
            return 0;
        }
        void LDeny76(string id) { _l76Deny = id; _l76DenyT = Now48; HellordAudio.Play("ui_denied"); }
        bool LB76(string id, float x, float y, float w, float h, string s, bool on, bool sel = false) => LBtn76(id, x, y, w, h, s, on, sel) == 1;

        /// <summary>Called once at the end of the line Sins panel: clears stale press/hover state.</summary>
        void LEnd76()
        {
            var e = Event.current;
            if (e.rawType == EventType.MouseUp) _l76Down = null;
            if (e.type == EventType.Repaint) { if (!_l76HoverSeen) _l76Hover = null; _l76HoverSeen = false; }
        }

        /// <summary>Slot unlock moment in line style: white flash, then a gold frame that fades (about 2 s). Plays the unlock sting.</summary>
        void LSlotFx76(int i, float x, float y, float w, float h)
        {
            if (Event.current.type != EventType.Repaint) return; float now = Now48;
            if (SinFeel1148On && i == _f48UnlockSlot)
            {
                float t = now - _f48UnlockT;
                if (t >= 0 && t < SinFeelTune.GlowEndS)
                {
                    if (t >= SinFeelTune.CrackEndS * 0.5f) UnlockSfx48();
                    if (t < SinFeelTune.BreakEndS) R53(x + 1, y + 1, w - 2, h - 2, new Color(1, 1, 1, 0.75f * (1 - t / SinFeelTune.BreakEndS)));
                    float a = t < SinFeelTune.GlowHoldS ? 1 : 1 - (t - SinFeelTune.GlowHoldS) / (SinFeelTune.GlowEndS - SinFeelTune.GlowHoldS);
                    Ol53(x - 1, y - 1, w + 2, h + 2, A48(U_AMB, a), 2);
                }
            }
            if (i == _f48DenySlot && now - _f48DenyST < SinFeelTune.DenyS) { float a = 1 - (now - _f48DenyST) / SinFeelTune.DenyS; Ol53(x - 1, y - 1, w + 2, h + 2, A48(L76_RED, a), 2); }
        }

        public static readonly Rect L76Box = new Rect(230, 140, 240, 96), L76No = new Rect(250, 210, 90, 16), L76Yes = new Rect(360, 210, 90, 16); // UNLOCK on the right; below the slot row (slots end y=72)

        /// <summary>Unlock confirm in the old line style. Same state + guard as t1149/t1151 (Confirm49*), drawn last over the panel.</summary>
        void LConfirm76()
        {
            var ev = Event.current;
            if (_c49Slot < 0) return;
            if (ev.type == EventType.Repaint && _c49Frames < 1000) _c49Frames++;
            var b = L76Box; int sc = _c49Sc, gc = _c49Gc;
            if (ev.type == EventType.Repaint)
            {
                R53(0, 0, 700, 392, new Color(0, 0, 0, 0.6f));
                R53(b.x - 2, b.y - 2, b.width + 4, b.height + 4, U_DRED); R53(b.x, b.y, b.width, b.height, U_PANEL); Ol53(b.x, b.y, b.width, b.height, U_AMB);
                TxC69("UNLOCK SIN SLOT " + (_c49Slot + 1) + "?", 350, b.y + 10, U_SAND);
                TxC69("COST " + Bal49(sc) + " SOULS + " + Bal49(gc) + " SIGIL" + (gc == 1 ? "" : "S"), 350, b.y + 26, U_AMB);
                TxC69("SOULS LEFT: " + Bal49(_souls51 - sc), 350, b.y + 40, U_GREY); TxC69("SIGILS LEFT: " + Bal49(_sigils51 - gc), 350, b.y + 52, U_GREY);
            }
            if (Guard49)
            {   // t1151 input guard: nothing in the dialog can be pressed yet; drop stray presses
                LBtn76Draw("c49no", L76No, "CANCEL"); LBtn76Draw("c49yes", L76Yes, "UNLOCK");
                if (ev.type == EventType.MouseDown || ev.type == EventType.MouseUp) ev.Use();
                if (_l76Down == "c49yes" || _l76Down == "c49no") _l76Down = null;
#if HELLORD_AUTOTEST
                if (ev.type == EventType.Repaint) Hit51(b.x, b.y, b.width, b.height); // eat a fake click inside the box during the guard
#endif
                return;
            }
            _c49Hit = true; bool y1 = false, n1 = false;
            try { n1 = LB76("c49no", L76No.x, L76No.y, L76No.width, L76No.height, "CANCEL", true); if (!n1) y1 = LB76("c49yes", L76Yes.x, L76Yes.y, L76Yes.width, L76Yes.height, "UNLOCK", true); }
            finally { _c49Hit = false; }
            if (y1) { AcceptConfirm49(); return; }
            if (n1) { CancelConfirm49(); return; }
            if (ev.type == EventType.MouseDown && ev.button == 0 && Now48 - _c49T > SinFeelTune.ModalOutsideGraceS && !b.Contains(ev.mousePosition)) { ev.Use(); CancelConfirm49(); return; }
            if (ev.type == EventType.MouseDown && Confirm49Open) ev.Use();
        }
        void LBtn76Draw(string id, Rect r, string s)
        {
            if (Event.current.type != EventType.Repaint) return;
            R53(r.x, r.y, r.width, r.height, U_INK); Ol53(r.x, r.y, r.width, r.height, U_SLATE); TxC69(s, r.x + r.width / 2, r.y + (r.height - 8) / 2, U_SAND);
        }

        /// <summary>The t770u Infernal Sins panel (original line layout) with the kept functions wired in.</summary>
        void DrawSinPanelLine1176()
        {
            SinLoad737();
            bool modal = Confirm49Open; bool blk = _block53; _block53 = blk || modal || !Gate49;
            R53(0, 0, 700, 392, new Color(0, 0, 0, 0.82f)); R53(14, 14, 672, 364, U_PANEL); Ol53(14, 14, 672, 364, U_DRED);
            Tx69("INFERNAL SINS", 24, 22, U_AMB);
            int shS = Shown48(false, _souls51), shG = Shown48(true, _sigils51);
            string cnt = "COLLECTED " + SinOwnedCount737 + "/" + HSins.Count + "    SOULS " + shS + "   SIGILS " + shG; Tx69(cnt, 676 - TW69(cnt), 22, Rolling48 ? U_AMB : U_SAND);
            if (LB76("close", 620, 360, 60, 14, "CLOSE", true)) { _block53 = blk; SinClose87(); return; }
            // slots row
            for (int i = 0; i < 3; i++)
            {
                float x = 24 + i * 222, y = 38; bool unl = i < _sinSlots737 && i < SlotsMax737;
                Ol53(x, y, 212, 34, unl ? U_SLATE : U_LOCK);
                if (unl)
                {
                    int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null;
                    Tx69("SLOT " + (i + 1), x + 4, y + 3, U_GREY);
                    if (d != null) { R53(x + 1, y + 13, 4, 20, HSins.TierCol[(int)d.tier]); Tx69(Fit69(d.name, 150), x + 8, y + 14, U_SAND); if (LB76("rm" + i, x + 166, y + 20, 42, 12, "REMOVE", true)) { _sinEq737[i] = -1; SinSave737(); HellordAudio.Play("ui_back"); } }
                    else Tx69(SinFeel1148On && i == _f48UnlockSlot && Now48 - _f48UnlockT < SinFeelTune.GlowEndS ? "OPEN" : "EMPTY", x + 8, y + 16, SinFeel1148On && i == _f48UnlockSlot && Now48 - _f48UnlockT < SinFeelTune.GlowEndS ? U_AMB : U_GREY);
                }
                else if (i < SlotsMax737)
                {
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
                    Tx69("SLOT " + (i + 1) + " LOCKED", x + 4, y + 3, U_GREY);
                    int r = LBtn76("slot" + i, x + 4, y + 16, 204, 14, "UNLOCK: " + sc + " SOULS + " + gc + " SIGIL" + (gc == 1 ? "" : "S"), next && afford);
                    bool guard = SinFeel1148On && Now48 - _f48UnlockT < SinFeelTune.UnlockGuardS; // never chains into a 2nd purchase
                    if (r == 1 && !guard) { if (SinConfirm1149On) OpenConfirm49(i, sc, gc); else { _c49Slot = i; _c49Sc = sc; _c49Gc = gc; AcceptConfirm49(); } }
                    else if (r == -1 && SinFeel1148On)
                    {
                        _f48DenySlot = i; _f48DenyST = Now48;
                        if (next) Hdr48("NEED " + Num88(sc) + " SOULS + " + Num88(gc) + " SIGIL" + (gc == 1 ? "" : "S"), L76_RED, SinFeelTune.DenyMsgS);
                        else Hdr48("UNLOCK SLOT " + (_sinSlots737 + 1) + " FIRST", L76_RED, SinFeelTune.DenyMsgS);
                    }
                }
                LSlotFx76(i, x, y, 212, 34);
            }
            // filters
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" };
            for (int i = 0; i < 4; i++) if (LB76("f" + i, 24 + i * 96, 78, 92, 14, fl[i], true, _sinFilter737 == i)) { _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; HellordAudio.Play("ui_confirm"); }
            // grid
            var list = SinFiltered737(); const int cols = 4, rows = 13, per = cols * rows; int pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1);
            for (int k = 0; k < per; k++)
            {
                int li = _sinPage737 * per + k; if (li >= list.Count) break; var d = list[li];
                float x = 24 + (k % cols) * 96, y = 98 + (k / cols) * 20; bool own = _sinOwned737[d.id]; var tc = HSins.TierCol[(int)d.tier];
                bool ov = !_block53 && Over51(x, y, 92, 18);
                R53(x, y, 92, 18, _sinSel737 == d.id ? U_PLUM : ov ? U_INK : new Color(0.08f, 0.06f, 0.1f)); Ol53(x, y, 92, 18, own ? tc : U_LOCK);
                Tx69(own ? Fit69(d.name, 84) : "? ? ?", x + 4, y + 5, own ? (SinEquipped737(d.id) ? U_YEL : U_SAND) : U_LOCK);
                if (Click53(x, y, 92, 18)) _sinSel737 = d.id;
            }
            if (pages > 1)
            {
                if (LB76("prev", 24, 360, 40, 14, "PREV", _sinPage737 > 0)) _sinPage737--;
                Tx69((_sinPage737 + 1) + "/" + pages, 72, 363, U_GREY);
                if (LB76("next", 104, 360, 40, 14, "NEXT", _sinPage737 < pages - 1)) _sinPage737++;
            }
            // detail
            float dx = 414, dy = 98; Ol53(dx, dy, 266, 256, U_SLATE);
            if (_sinSel737 >= 0)
            {
                var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id]; var tc = HSins.TierCol[(int)d.tier];
                R53(dx + 1, dy + 1, 264, 4, tc);
                Tx69(HSins.TierName[(int)d.tier] + " SIN  NO." + (d.id + 1), dx + 6, dy + 10, tc);
                if (own)
                {
                    float y = SinWrap737(d.name, dx + 6, dy + 24, 254, U_SAND);
                    if (d.vn != "") { Tx69("(" + d.vn + ")", dx + 6, y + 2, U_GREY); y += 12; }
                    var q = _sinRoll737[d.id]; float ql = HSins.Quality(q); Tx69(HSins.QualityTxt(q, d), dx + 6, y + 2, ql >= ST737.sinPerfectAt737 ? U_YEL : U_SAND); y += 12;
                    y += 4; foreach (var l in HSins.Lines(d, q)) y = SinWrap737(l, dx + 6, y, 254, l.StartsWith("-") ? C69_RED : l.StartsWith("EFFECT NOT") ? U_GREY : U_AMB) + 3;
                    bool eq = SinEquipped737(d.id);
                    if (eq) { if (LB76("act", dx + 6, dy + 232, 120, 16, "UNEQUIP", true)) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); HellordAudio.Play("ui_back"); } }
                    else
                    {
                        int free = -1; for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; }
                        if (LB76("act", dx + 6, dy + 232, 120, 16, _sinSlots737 <= 0 ? "NO SLOT UNLOCKED" : free < 0 ? "SLOTS FULL" : "EQUIP", free >= 0)) { _sinEq737[free] = d.id; SinSave737(); HellordAudio.Play("ui_confirm"); }
                    }
                }
                else { Tx69("UNKNOWN SIN", dx + 6, dy + 24, U_GREY); Tx69("Not found yet. Sins drop from demons and bosses.", dx + 6, dy + 40, U_LOCK); }
            }
            else Tx69("Select a sin.", dx + 6, dy + 10, U_GREY);
            if (HdrOverride48(out var hs, out var hc)) TxC69(hs, 350, 362, hc);
            else if (_sinMsgT737 > 0) TxC69(_sinMsg737, 350, 362, U_AMB);
            _block53 = blk;
            LConfirm76();
            LEnd76();
        }
    }
}
