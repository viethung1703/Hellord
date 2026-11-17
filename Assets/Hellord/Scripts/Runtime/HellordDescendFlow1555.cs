using UnityEngine;
using System.Collections.Generic;

// t1555 (VH: "go, build the descend pact pick and skill pick flow"): DESCEND on the main menu opens
//   (1) a PACT picker: the 4 pact gods on v8 lacquer cards (name + title frame, 1x avatar, status + basics lines), even spacing, and
//   (2) a SKILL picker: only that pact's basic main attacks (LMB basics, HellordTuning.descendBasics*1555 c lmbBasicIds1517), choose 1;
// then the run starts with that pact and the basic as the first skill (belt slot 1 = the LMB basic, t1517). Back on each step.
// Pact rules: the god SEALED on the Sacrifice Altar is preselected; any unlocked god can be picked. Picking the sealed god applies its nodes exactly as
// before (PactApply51); picking another god starts the run with that god's basic and NO pact nodes (the saved Path / picks / seal are never touched).
// A locked god (no node unlocked) cannot be picked, except on a fresh save with nothing unlocked (descendAllowAllWhenNoneUnlocked1555).
// A god without a basic (Leizu: no lightning basic exists) cannot be picked. Sealing on the altar never starts a run (altar flow untouched).
// Layout (640x360 grid, inside the scene frame interior x 24..616 / y 40..314): equal horizontal gaps (cards + frame edges) and equal vertical gaps
// (frame top -> cards -> buttons -> frame bottom). Mouse (hover selects, click selects / click again or the green button confirms), keys/pad (left/right, A/Enter, B/Esc).
namespace Hellord
{
    public partial class HellordGame
    {
        int _df1555, _dfSel1555, _dfSk1555, _dfOpenF1555 = -9; Vector2 _dfLastM1555 = new Vector2(-99, -99);
        public int DescendStep1555 => _df1555; public int DescendRunPact1555 = -1; public SkillId DescendRunBasic1555;
        public string DescendGaps1555 = "";

        bool DfWanted1555() => PT.descendFlow1555 && HellordUITheme.Active == HellordUITheme.Theme.MenuTheme && AmReady() && PmReady() && NP97 == 4;
        SkillId[] DfBasicsRaw1555(int p) { var T = PT; return p == 0 ? T.descendBasicsZhurong1555 : p == 1 ? T.descendBasicsZhongKui1555 : p == 2 ? T.descendBasicsLeizu1555 : T.descendBasicsXingtian1555; }
        /// <summary>the pact's basics that really are LMB basics (lmbBasicIds1517), ON HOLD ones dropped (or kept as disabled when descendShowOnHold1555).</summary>
        List<SkillId> DfBasics1555(int p, bool withHold)
        {
            var l = new List<SkillId>(); var raw = DfBasicsRaw1555(p); if (raw == null) return l;
            foreach (var id in raw)
            {
                if (PT.lmbBasicIds1517 == null || System.Array.IndexOf(PT.lmbBasicIds1517, id) < 0) continue;
                bool hold = PT.descendOnHold1555 != null && System.Array.IndexOf(PT.descendOnHold1555, id) >= 0;
                if (hold && !(withHold && PT.descendShowOnHold1555)) continue; if (!l.Contains(id)) l.Add(id);
            }
            if (withHold && PT.descendShowOnHold1555 && p == 0 && PT.descendOnHold1555 != null) foreach (var id in PT.descendOnHold1555) if (!l.Contains(id)) l.Add(id);
            return l;
        }
        bool DfHold1555(SkillId id) => PT.descendOnHold1555 != null && System.Array.IndexOf(PT.descendOnHold1555, id) >= 0;
        bool DfNoneUnlocked1555() { for (int p = 0; p < NP97; p++) if (!PathLocked97(p)) return false; return true; }
        bool DfLockLook1555(int p) => PathLocked97(p) && !(PT.descendAllowAllWhenNoneUnlocked1555 && DfNoneUnlocked1555());
        bool DfPickable1555(int p) => !DfLockLook1555(p) && DfBasics1555(p, false).Count > 0;

        void DfOpen1555()
        {
            PactLoad51(); _df1555 = 1; _dfOpenF1555 = Time.frameCount; _dfSk1555 = 0;
            int s = _pactSealed51 && _pactPath51 >= 0 && DfPickable1555(_pactPath51) ? _pactPath51 : -1;   // default: the god sealed on the altar
            if (s < 0) for (int k = 0; k < NP97; k++) { int p = (Mathf.Max(0, _pactPath51) + k) % NP97; if (DfPickable1555(p)) { s = p; break; } }
            _dfSel1555 = Mathf.Max(0, s); HellordAudio.Play("ui_confirm");
        }
        void DfBack1555() { HellordAudio.Play("ui_back"); if (_df1555 == 2) { _df1555 = 1; _dfOpenF1555 = Time.frameCount; } else { _df1555 = 0; _sinBackFrame87 = Time.frameCount; } }
        void DfConfirm1555()
        {
            if (_df1555 == 1)
            {
                if (!DfPickable1555(_dfSel1555)) { HellordAudio.Play("ui_denied"); return; }
                if (!PT.descendSkillScreen1555) { DfStartRun1559(_dfSel1555); return; }   // t1559: the run starts, the old start pick filters to this pact
                _df1555 = 2; _dfOpenF1555 = Time.frameCount; var l = DfBasics1555(_dfSel1555, true); _dfSk1555 = 0; while (_dfSk1555 < l.Count - 1 && DfHold1555(l[_dfSk1555])) _dfSk1555++; HellordAudio.Play("ui_confirm"); return;
            }
            var ls = DfBasics1555(_dfSel1555, true); if (_dfSk1555 < 0 || _dfSk1555 >= ls.Count || DfHold1555(ls[_dfSk1555])) { HellordAudio.Play("ui_denied"); return; }
            DfStartRun1555(_dfSel1555, ls[_dfSk1555]);
        }
        /// <summary>starts the run exactly like Descend51 (sting, StartPick, pact apply), then takes the chosen basic as the start pick (Choose: skills[0] = belt slot 1).</summary>
        void DfStartRun1555(int p, SkillId id)
        {
            _df1555 = 0; DescendRunPact1555 = p; DescendRunBasic1555 = id;
            HellordAudio.Play("pact_seal_descend"); mode = Mode.StartPick;
            if (p == _pactPath51) PactApply51();                           // your Path: its (sealed) nodes apply as before
            else { SinRecalc737(); PactLoad51(); _pactActive51 = false; }   // another god: its basic only, no pact nodes; the saved Path is untouched
            if (skills.Count > 0) { skills.Clear(); SetHeroLayers(); }
            Choose(new Card { kind = CardKind.NewSkill, skill = HData.Skill(id) });
            Debug.Log("[Hellord] t1555 descend: pact " + PactData.PathShort[p] + " basic " + id + " pactNodes " + (_pactActive51 ? SelCount51() : 0));
        }
        /// <summary>t1559: the pact picker starts the run; the old start pick (BuildStartCards / DrawCardsB 'CHOOSE YOUR STARTING SKILL') then offers only this pact's basics.
        /// Nodes: the chosen god = your altar Path -> its tree picks apply (even unsealed, descendApplyChosenNodes1559; the saved seal is restored at once, never saved);
        /// another god has no stored picks -> no nodes. A Path with 0 picks -> no nodes (PactApply51 needs picks).</summary>
        int _dfRunPick1559 = -1; public int DescendInGamePick1559 => _dfRunPick1559;
        void DfStartRun1559(int p)
        {
            _df1555 = 0; DescendRunPact1555 = p; _dfRunPick1559 = p;
            HellordAudio.Play("pact_seal_descend"); mode = Mode.StartPick;
            if (p == _pactPath51) { bool s = _pactSealed51; if (PT.descendApplyChosenNodes1559) _pactSealed51 = true; PactApply51(); _pactSealed51 = s; }
            else { SinRecalc737(); PactLoad51(); _pactActive51 = false; }
            if (skills.Count > 0) { skills.Clear(); SetHeroLayers(); }
            BuildStartCards();
            Debug.Log("[Hellord] t1559 descend: pact " + PactData.PathShort[p] + " pactNodes " + (_pactActive51 ? SelCount51() : 0) + " startCards " + cards.Count);
        }
        /// <summary>t1559: the old start cards, filtered to the run pact's basics (mapping order). Empty after a pact ban -> the unfiltered basics (never an empty pick).</summary>
        bool DfStartCards1559()
        {
            if (_dfRunPick1559 < 0 || !PT.descendFlow1555 || PT.descendSkillScreen1555) return false;
            cards.Clear(); var l = DfBasics1555(_dfRunPick1559, false);
            foreach (var id in l) { var d = HData.Skill(id); if (!PactBanned53(d)) cards.Add(new Card { kind = CardKind.NewSkill, skill = d }); }
            if (cards.Count == 0) foreach (var id in l) cards.Add(new Card { kind = CardKind.NewSkill, skill = HData.Skill(id) });
            warning = null; return cards.Count > 0;
        }
        void DfMove1555(int d)
        {
            if (_df1555 == 1) { for (int k = 1; k <= NP97; k++) { int p = (_dfSel1555 + d * k + 4 * NP97) % NP97; if (DfPickable1555(p)) { if (p != _dfSel1555) HellordAudio.Play("ui_hover"); _dfSel1555 = p; return; } } }
            else { var l = DfBasics1555(_dfSel1555, true); int n = l.Count; for (int k = 1; k <= n; k++) { int i = (_dfSk1555 + d * k + 4 * n) % n; if (!DfHold1555(l[i])) { if (i != _dfSk1555) HellordAudio.Play("ui_hover"); _dfSk1555 = i; return; } } }
        }
        /// <summary>Update: keys / pad (called from PactTick51 instead of MenuTick69 while the flow is open).</summary>
        void DfTick1555()
        {
            if (Time.frameCount <= _dfOpenF1555) return;                    // the key that opened this step must not confirm it too
            bool lf = Nav69(2), rt = Nav69(3), ok = K69Down(4), back = K69Down(5); var mv = HInput.Move(); _navY69 = mv.y; _navX69 = mv.x;
            if (back) { DfBack1555(); return; }
            if (lf) DfMove1555(-1); if (rt) DfMove1555(1);
            if (ok) { AmKick("dfok"); DfConfirm1555(); }
        }

        // ---------------------------------------------------------------- layout
        int DfPactH1555() { int P = PT.descendPad1555; return P + 35 + P + 70 + P + 7 + PT.descendLead1555 + 7 + P; }
        int DfIconF1555() => 24 * Mathf.Clamp(PT.descendIconScale1555, 1, 3) + 6;
        static string DfShort1555(string d) { if (string.IsNullOrEmpty(d)) return ""; int i = d.IndexOf(". "); return i > 0 ? d.Substring(0, i + 1) : d; }
        List<string> DfWrap1555(string s, int w)
        {
            var o = new List<string>(); string line = ""; foreach (var wd in Sanitize(s).Split(' ')) { if (wd.Length == 0) continue; string t = line.Length == 0 ? wd : line + " " + wd; if (MtW(t, false) > w && line.Length > 0) { o.Add(line); line = wd; } else line = t; }
            if (line.Length > 0) o.Add(line); return o;
        }
        int DfTextW1555() => PT.descendCardW1555 - 2 * PT.descendPad1555 - 2 * PT.descendTextInset1555;
        /// <summary>vertical: 3 equal gaps (frame top, between cards and buttons, frame bottom) around the card height h + the 19 px buttons; remainder (if any) goes to the bottom gap.</summary>
        void DfVert1555(int h, out int cy, out int by, out int g, out int rem) { int inH = PT.descendInY1_1555 - PT.descendInY0_1555 + 1; g = (inH - h - 19) / 3; rem = (inH - h - 19) - 3 * g; cy = PT.descendInY0_1555 + g; by = cy + h + g; }
        /// <summary>horizontal: n cards, equal gaps; with n = 4 the frame edges get the same gap (593 = 4x132 + 5x13). Fewer cards keep that gap and centre the row.</summary>
        void DfHorz1555(int n, out int x0, out int g, out int ml, out int mr)
        {
            int W = PT.descendCardW1555, inW = PT.descendInX1_1555 - PT.descendInX0_1555 + 1; int g4 = (inW - NP97 * W) / (NP97 + 1); g = g4;
            int tot = n * W + (n - 1) * g; x0 = n == NP97 ? PT.descendInX0_1555 + g4 + ((inW - NP97 * W - (NP97 + 1) * g4) / 2) : PT.descendInX0_1555 + (inW - tot) / 2;
            ml = x0 - PT.descendInX0_1555; mr = PT.descendInX1_1555 - (x0 + tot - 1);
        }

        // ---------------------------------------------------------------- draw (OnGUI, mode MainMenu)
        void DrawDescendFlow1555()
        {
            var G = GUI.matrix; _mtRep = Event.current.type == EventType.Repaint;
            float W = Screen.width, H = Screen.height, fit = Mathf.Min(W / 640f, H / 360f); int s = Mathf.Max(1, Mathf.FloorToInt(fit + 0.001f)), sr = Mathf.RoundToInt(fit);
            if (sr > s && W / sr >= 576f && H / sr >= 324f) s = sr; if (MenuThemeScale > 0) s = MenuThemeScale;
            _mtS = s; MenuThemeScaleUsed = s; _mtOx = Mathf.Round(W / 2 - s * 320f); _mtOy = Mathf.Round(H / 2 - s * 180f);
            GUI.matrix = Matrix4x4.identity;
            try { DfBody1555(); }
            catch (System.Exception e) { Debug.LogError("[Hellord] t1555 descend flow failed, falling back to the old DESCEND: " + e); _df1555 = 0; PT.descendFlow1555 = false; Descend51(); }
            finally { GUI.matrix = G; GUI.color = Color.white; }
        }
        void DfBody1555()
        {
            float t = MtClock();
            if (_mtRep)
            {
                GUI.color = MT_INK; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
                PmBackdrop(t);
                // the altar scene without its title (x 235..403, y 8..20) and without the old side frames (x108..211 / 428..531, y104..223); veil redrawn there
                MtBlit(_amScene, 0, 0, 230, 104, 0, 0); MtBlit(_amScene, 410, 0, 230, 104, 410, 0); MtBlit(_amScene, 230, 22, 180, 82, 230, 22);
                MtBlit(_amScene, 0, 224, 640, 136, 0, 224);
                MtBlit(_amScene, 0, 104, 108, 120, 0, 104); MtBlit(_amScene, 212, 104, 216, 120, 212, 104); MtBlit(_amScene, 532, 104, 108, 120, 532, 104);
                AmBlitS(_amScene, 60, 150, 1, 1, 108, 104, 104, 120); AmBlitS(_amScene, 60, 150, 1, 1, 428, 104, 104, 120);
            }
            AmTxAt("SOULS " + _souls51, 144, 11, "YELLOW");
            { string sg = "SIGILS " + _sigils51; AmTxAt(sg, AltarMenuData.SigilIx - 4 - MtW(Sanitize(sg), false), 11, "PINK"); }
            string title = _df1555 == 1 ? "CHOOSE YOUR PACT" : "CHOOSE YOUR BASIC ATTACK"; { string ts = Sanitize(title); MtDisp(ts, 320 - MtW(ts, true) / 2, PT.descendTitleY1555, MT_TAB_N, "N"); }
            var m = MtMouse(); bool moved = (m - _dfLastM1555).sqrMagnitude > 0.25f && _dfLastM1555.x > -98; if (Event.current.type == EventType.Repaint) _dfLastM1555 = m;
            int cy, by, g, rem; string okLab;
            if (_df1555 == 1) { DfPactCards1555(moved, out cy, out by, out g, out rem); okLab = "CHOOSE"; }
            else { DfSkillCards1555(moved, out cy, out by, out g, out rem); okLab = "DESCEND"; }
            if (_df1555 == 0) return;   // a card click started the run / went back
            bool okOn = _df1555 == 1 ? DfPickable1555(_dfSel1555) : true;
            if (DfBtn1555("dfok", "open", PT.descendOkX1555, by, okLab, okOn)) { DfConfirm1555(); }
            else if (_df1555 != 0 && AmBtn("dfback", "back", PT.descendBackX1555, by, 105, 19, true)) DfBack1555();
            if (_mtRep)
            {
                PmDomes(t); float boost; PmBursts(t, out boost);
                float a = Mathf.Clamp(0.94f + 0.04f * Mathf.Sin(t * 6.1f) + 0.03f * Mathf.Sin(t * 11.7f) + 0.30f * boost, 0f, 1.4f); MtBlit(_amGlow, 0, 0, 640, 360, 0, 0, a);
            }
        }
        /// <summary>the altar's green OPEN TREE button with its label repainted (the 2-px dithered body is copied over the old label, same column parity) and a new label drawn.</summary>
        bool DfBtn1555(string id, string spr, int x, int y, string label, bool enabled)
        {
            bool over = enabled && MtOver(id, x, y, 105, 19), pr = MtPressed(id, over); int lv = AmLevel(id, over, pr);
            int dw = 2 * lv, dh = 2 * (lv / 2), ox = x - dw / 2, oy = y - dh / 2 + (lv < 0 ? 1 : 0); string nm = spr + (lv >= 1 ? "_h_" : "_n_") + AmD(dw) + "_" + AmD(dh);
            float al = enabled ? 1f : 0.5f; AmSpr(nm, ox, oy, al);
            int[] r; if (_amAtlas != null && AltarMenuData.Atlas.TryGetValue(nm, out r) && _mtRep)
            {
                int rw = r[2], rh = r[3], s0 = rw - 17;
                for (int c = 20; c < rw - 6; c++) { int sc = ((c - s0) & 1) == 0 ? s0 : s0 + 1; GUI.color = new Color(1, 1, 1, al); AmBlitS(_amAtlas, r[0] + sc, r[1], 1, rh, ox + c, oy, 1, rh); }
                GUI.color = Color.white; string ls = Sanitize(label); int lx = ox + 19 + (rw - 19 - 3 - MtW(ls, false)) / 2; MtTx(ls, lx, oy + (rh - 7) / 2, enabled ? "BONE" : "DIM", true);
            }
            if (over) MtBrackets(ox, oy, 105 + dw, 19 + dh);
            return MtClick(id, x, y, 105, 19, enabled);
        }
        /// <summary>lacquer two-line block (HA t1552 frame 2) with free text: name BONE, title CLAY (selected) / TERRA.</summary>
        void DfLacquer1555(int x0, int top, int w, string nm, string tl, string tc)
        {
            AmNine(T97("pact_frame_lacquer"), V6Frame, 25, 25, 15, 5, 64, 22, x0, top, w, 35);
            AmTxIn(nm, x0, 0, w, 0, "BONE", top + PT.pactV6NameDy); AmTxIn(tl, x0, 0, w, 0, tc, top + PT.pactV6TitleDy);
        }
        void DfCardBg1555(int x, int y, int w, int h, bool sel, bool dis)
        {
            var t = V2Tex; if (sel) AmNine(t, V6CardC, 25, 24, 20, 10, 25, 21, x, y, w, h); else AmNine(t, dis ? V4CardL : V4CardN, 10, 10, 16, 14, 65, 16, x, y, w, h);
        }
        void DfPactCards1555(bool moved, out int cy, out int by, out int g, out int rem)
        {
            int W = PT.descendCardW1555, P = PT.descendPad1555, Hc = DfPactH1555(), x0, gh, ml, mr; DfVert1555(Hc, out cy, out by, out g, out rem); DfHorz1555(NP97, out x0, out gh, out ml, out mr);
            DescendGaps1555 = $"pact: cards {W}x{Hc} gapX {gh} frameL {ml} frameR {mr} | gapY top {g} mid {g} bottom {g + rem} (card y {cy}..{cy + Hc - 1}, buttons y {by}..{by + 18}) | pad {P}";
            bool none = DfNoneUnlocked1555();
            for (int p = 0; p < NP97; p++)
            {
                int x = x0 + p * (W + gh), y = cy; bool lk = DfLockLook1555(p), can = DfPickable1555(p), sel = p == _dfSel1555, seal = _pactSealed51 && _pactPath51 == p;
                DfCardBg1555(x, y, W, Hc, sel && can, !can);
                DfLacquer1555(x + P, y + P, W - 2 * P, PactData.PathShort[p], PactData.PathTitle[p], sel ? "CLAY" : "TERRA");
                int ay = y + P + 35 + P, ax = x + (W - 70) / 2;
                AmAtlas(V2Tex, lk ? V2AvL : V2AvN, ax, ay); AmBlitN(T97(Av97(p, lk, PT.pactGods1544 && sel && can)), ax + 3, ay + 3, 1, can ? 1f : 0.55f);
                if (lk) AmSpr("lock", ax + 3 + 49, ay + 3 + 49); if (seal) AmSpr("chip", x + 4, y + 4);
                int n = DfBasics1555(p, false).Count; string st, sc;
                if (seal) { st = "SEALED " + SelCount51() + "/" + MaxSel51(p) + " NODES"; sc = "AMBER"; }
                else if (lk) { st = "LOCKED"; sc = "DIM"; }
                else if (PathLocked97(p) && none) { st = "NO PACT NODES"; sc = "TERRA"; }
                else if (!PT.descendSkillScreen1555 && PT.descendApplyChosenNodes1559 && _pactPath51 == p) { st = "PACT " + SelCount51() + "/" + MaxSel51(p) + " NODES"; sc = SelCount51() > 0 ? "AMBER" : "TERRA"; }   // t1559: your altar Path's picks apply
                else if (!PT.descendSkillScreen1555) { st = "NO NODES PICKED"; sc = "TERRA"; }
                else { st = "UNSEALED: NO NODES"; sc = "TERRA"; }
                string bl = n == 0 ? "NO BASIC ATTACK YET" : n == 1 ? "1 BASIC ATTACK" : n + " BASIC ATTACKS";
                int ty = ay + 70 + P; AmTxIn(st, x, 0, W, 0, sc, ty); AmTxIn(bl, x, 0, W, 0, n == 0 ? "DIM" : can ? "BONE" : "DIM", ty + 7 + PT.descendLead1555);
                string id = "dfp" + p; bool ov = can && MtOver(id, x, y, W, Hc);
                if (ov && moved && p != _dfSel1555) { _dfSel1555 = p; HellordAudio.Play("ui_hover"); }
                if (ov || (sel && can)) MtBrackets(x, y, W, Hc);
                if (MtClick(id, x, y, W, Hc, true)) { if (!can) HellordAudio.Play("ui_denied"); else if (p == _dfSel1555) { AmKick("dfok"); DfConfirm1555(); return; } else { _dfSel1555 = p; HellordAudio.Play("ui_hover"); } }
            }
        }
        /// <summary>skill card text lines (short description wrapped to the text width), max line count L, the leading (descendLead1555 unless another 2..5 makes the three vertical gaps exactly equal) and the card height.</summary>
        void DfSkillLayout1555(List<SkillId> l, out List<List<string>> lines, out int L, out int lead, out int Hs)
        {
            int P = PT.descendPad1555, IF = DfIconF1555(), tw = DfTextW1555(); lines = new List<List<string>>(); L = 1;
            foreach (var id in l) { var d = HData.Skill(id); var ws = DfWrap1555(DfHold1555(id) ? "ON HOLD" : DfShort1555(d.desc), tw); lines.Add(ws); L = Mathf.Max(L, ws.Count); }
            lead = PT.descendLead1555; int inH = PT.descendInY1_1555 - PT.descendInY0_1555 + 1, LL = L;
            System.Func<int, int> HsOf = ld => P + 35 + P + IF + P + LL * 7 + (LL - 1) * ld + P;
            if ((inH - HsOf(lead) - 19) % 3 != 0 && L > 1) for (int ld = 2; ld <= 5; ld++) if ((inH - HsOf(ld) - 19) % 3 == 0) { lead = ld; break; }
            Hs = HsOf(lead);
        }
        int DfSkillH1555() { List<List<string>> a; int L, ld, h; DfSkillLayout1555(DfBasics1555(_dfSel1555, true), out a, out L, out ld, out h); return h; }
        void DfSkillCards1555(bool moved, out int cy, out int by, out int g, out int rem)
        {
            var l = DfBasics1555(_dfSel1555, true); int n = Mathf.Max(1, l.Count), W = PT.descendCardW1555, P = PT.descendPad1555, IF = DfIconF1555();
            List<List<string>> lines; int L, lead, Hs0; DfSkillLayout1555(l, out lines, out L, out lead, out Hs0);
            int Hs = Hs0, x0, gh, ml, mr; DfVert1555(Hs, out cy, out by, out g, out rem); DfHorz1555(n, out x0, out gh, out ml, out mr);
            DescendGaps1555 = $"skill: cards {W}x{Hs} n {l.Count} gapX {gh} frameL {ml} frameR {mr} | gapY top {g} mid {g} bottom {g + rem} (card y {cy}..{cy + Hs - 1}, buttons y {by}..{by + 18}) | pad {P} lead {lead}";
            for (int i = 0; i < l.Count; i++)
            {
                var id = l[i]; var d = HData.Skill(id); bool hold = DfHold1555(id), sel = i == _dfSk1555 && !hold; int x = x0 + i * (W + gh), y = cy;
                DfCardBg1555(x, y, W, Hs, sel, hold);
                var si = new SkillInst { def = d }; float cd = d.cooldown * si.CdMul;
                DfLacquer1555(x + P, y + P, W - 2 * P, d.name, hold ? "ON HOLD" : "COOLDOWN " + cd.ToString("0.0#") + "S", sel ? "CLAY" : "TERRA");
                int fy = y + P + 35 + P, fx = x + (W - IF) / 2, sc = (IF - 6) / 24;
                AmNine(V2Tex, hold ? V2AvL : V2AvN, 4, 4, 4, 4, 35, 35, fx, fy, IF, IF);
                DfIcon1555(id, fx + 3, fy + 3, sc, hold ? 0.45f : 1f);
                var ws = lines[i]; int bh = ws.Count * 7 + (ws.Count - 1) * lead, zone = L * 7 + (L - 1) * lead, ty = fy + IF + P + (zone - bh) / 2;
                for (int k = 0; k < ws.Count; k++) AmTxIn(ws[k], x, 0, W, 0, hold ? "DIM" : sel ? "BONE" : "CLAY", ty + k * (7 + lead));
                string cid = "dfs" + i; bool ov = !hold && MtOver(cid, x, y, W, Hs);
                if (ov && moved && i != _dfSk1555) { _dfSk1555 = i; HellordAudio.Play("ui_hover"); }
                if (ov || sel) MtBrackets(x, y, W, Hs);
                if (MtClick(cid, x, y, W, Hs, true)) { if (hold) HellordAudio.Play("ui_denied"); else if (i == _dfSk1555) { AmKick("dfok"); DfConfirm1555(); return; } else { _dfSk1555 = i; HellordAudio.Play("ui_hover"); } }
            }
        }
        /// <summary>the game's own 24 px skill icon (fire basics: their t1481 icons; others: the v51k belt sheet column id*28), integer scale.</summary>
        void DfIcon1555(SkillId id, int x, int y, int s, float a)
        {
            GUI.color = new Color(1, 1, 1, a);
            if (IsFb1481(id)) { var ft = FbIcon1481(id); if (ft != null) AmBlitS(ft, 0, 0, 24, 24, x, y, 24 * s, 24 * s); }
            else if (IceArt.IsIce(id)) { var it = IceArt.Icon24(id); if (it != null) AmBlitS(it, 0, 0, 24, 24, x, y, 24 * s, 24 * s); }
            else { SkillBarTex(); if (_tSkill24 != null) { int c = _icons44 ? (int)id : BarTile(id); AmBlitS(_tSkill24, c * 28, 0, 24, 24, x, y, 24 * s, 24 * s); } }
            GUI.color = Color.white;
        }
    }
}
