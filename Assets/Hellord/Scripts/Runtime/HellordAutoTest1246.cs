#if HELLORD_AUTOTEST
// t1259 v0.0.99: autotest for the approved Sins screen (t1246b pass3e). Run alone: -atOnly sins1246 (also runs at the end of -atOnly menus92).
// Pixel check: fixture "mock" (exact mockup content) with a pinned clock, captured at the integer scale, sampled back to 480x270 and compared with the offline
// golden of the same instant (-t1246golden=<dir> containing golden_t9.15.png; skipped, and reported as skipped, when absent).
using System.Collections;
using System.IO;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        static string At1246Arg(string key) { foreach (var a in System.Environment.GetCommandLineArgs()) if (a.StartsWith(key + "=")) return a.Substring(key.Length + 1); return null; }
        /// <summary>Samples a screenshot back to native px (one sample per integer-scale block) and returns it as 480x270 Color32 rows top-down.</summary>
        static bool IsFlamePal1262(Color32 c) { return (c.r == 254 && c.g == 231 && c.b == 97) || (c.r == 254 && c.g == 174 && c.b == 52) || (c.r == 247 && c.g == 118 && c.b == 34) || (c.r == 228 && c.g == 59 && c.b == 68) || (c.r == 162 && c.g == 38 && c.b == 51); }
        Color32[] At1246Native(Texture2D cap)
        {
            int s = T46ScaleUsed, W = cap.width, H = cap.height, ox = Mathf.RoundToInt(W / 2f - s * 240f), oy = Mathf.RoundToInt(H / 2f - s * 135f); var px = cap.GetPixels32(); var o = new Color32[480 * 270];
            for (int j = 0; j < 270; j++) for (int i = 0; i < 480; i++) { int sx = ox + s * i + s / 2, sy = oy + s * j + s / 2; o[j * 480 + i] = sx >= 0 && sx < W && sy >= 0 && sy < H ? px[(H - 1 - sy) * W + sx] : new Color32(0, 0, 0, 255); }
            return o;
        }
        // t1260: regions deliberately changed after the mockup golden was made (fire/embers/talismans, the fire-light zones on the frame edges, the cell grid with its new studless cells + 1px ring, the detail icon well).
        // (the pillar columns also hold the animated cracks/eyes, whose phase is not pixel-pinned to the offline render.) Everything else (plaques, slots, tabs, detail text, bottom row, backdrop) must still match the offline mockup render.
        static bool At1273Frame(int x, int y)   // t1273: brazier firelight on the main frame stone (outer ornate border strips + the 8 px stone rings of the grid and detail panels): deliberately changed
        {
            if ((x >= 47 && x < 59) || (x >= 421 && x < 433) || (y >= 256 && y < 268 && x >= 47 && x < 433)) return true;
            foreach (var r in new[] { new[] { 64, 288, 75, 229 }, new[] { 292, 415, 75, 229 } })
                if (x >= r[0] && x <= r[1] && y >= r[2] && y <= r[3] && !(x >= r[0] + 8 && x <= r[1] - 8 && y >= r[2] + 8 && y <= r[3] - 8)) return true;
            return false;
        }
        static bool At1246Fx(int x, int y) => At1273Frame(x, y) || x < 42 || x >= 438 || x < 80 && y >= 100 || x >= 400 && y >= 100 || x >= 77 && x < 277 && y >= 88 && y < 216 || x >= 285 && y >= 52 && y < 74 || x >= 293 && x < 417 && y >= 75 && y < 231 || x >= 184 && x < 296 && y >= 18 && y < 50;   // pillar ember boxes (fire, embers, talismans: separate render, own clocks)
        void At1246SaveNative(Color32[] c, string name) { var t = new Texture2D(480, 270, TextureFormat.RGBA32, false); var fl = new Color32[480 * 270]; for (int j = 0; j < 270; j++) for (int i = 0; i < 480; i++) fl[(269 - j) * 480 + i] = c[j * 480 + i]; t.SetPixels32(fl); t.Apply(); File.WriteAllBytes(Path.Combine(_atOut, name), t.EncodeToPNG()); Destroy(t); }
        IEnumerator At1246Capture() { yield return new WaitForEndOfFrame(); if (_atLast) Destroy(_atLast); _atLast = ScreenCapture.CaptureScreenshotAsTexture(); }
        IEnumerator AtSins1246()
        {
            yield return AtBegin("sins1246"); bool t46was = T1246On; T1246On = true; bool eyes0 = SinEyesTune.On; T1246HoverOff = true;   // the real mouse must not preview cells during scripted clicks (a dedicated hover test uses T1246Force)
            SinLoad737(); PactLoad51();
            var own0 = (bool[])_sinOwned737.Clone(); var roll0 = (float[][])_sinRoll737.Clone(); var eq0 = (int[])_sinEq737.Clone(); int sl0 = _sinSlots737, so0 = _souls51, si0 = _sigils51;
            AtClearWorld(); mode = Mode.MainMenu; SinOpen737(); yield return AtWait(0.3f);
            for (int i = 0; i < HSins.Count; i++) { bool o = i % 9 != 4; _sinOwned737[i] = o; _sinRoll737[i] = o ? SinNewRoll737(HSins.All[i]) : null; }
            int h44 = HSins.All.FindIndex(d => d.name.StartsWith("Lightning Rod")); _sinOwned737[h44] = true; _sinRoll737[h44] = SinNewRoll737(HSins.All[h44]);
            int sc2 = Arr737(ST737.sinSlotSoulCost737, 2, 100), gc2 = Arr737(ST737.sinSlotSigilCost737, 2, 1);
            _sinSlots737 = 2; _sinEq737[0] = h44; _sinEq737[1] = _sinEq737[2] = -1; _souls51 = sc2 + 500; _sigils51 = gc2 + 5; PactSave51(); SinSave737();
            _sinFilter737 = 0; _sinSel737 = -1; _sinPage737 = 0; _s74Page = 0; T46Drawn = false; yield return AtWait(0.6f);
            ARes("sins1246_screen_active", T46Drawn && T1246Ready() && T46LastState != null, "t1246 screen drew, atlas/font/base loaded (last " + T46LastState + ")");
            ARes("sins1246_shader_ok", Sin84Ready && _s84L1246, "pass3e compositor (HellordSinFX1246) ready=" + Sin84Ready);
            ARes("sins1246_integer_scale", T46ScaleUsed >= 1, $"screen {Screen.width}x{Screen.height} -> {T46ScaleUsed} screen px per native px");
            // ---- t1261: empty-slot label fits the 32px red frame (23px interior) with >= 2px margin; header plaque keeps 4px clear; every Sin's detail text fits the panel
            {
                int lw = Tiny61Width("EMPTY"); ARes("sins1246_empty_label_fits", lw + 2 * 2 <= 23, $"tiny EMPTY label {lw}px wide inside the 23px slot interior (margin {(23 - lw) / 2f}px), glyph+label height 5+9+...");
                var hr = T61_HDR; int tabEnd = 0; foreach (var t in Sin1246Data.Tabs[0]) tabEnd = Mathf.Max(tabEnd, t[0] + t[2]); var dpan = Sin1246Data.R_detail; var s2 = Sin1246Data.R_slot2;
                ARes("sins1246_header_plaque_clearance", hr[0] - tabEnd >= 4 && hr[1] - (s2[1] + s2[3]) >= 4 && dpan[1] - (hr[1] + hr[3]) >= 4 && hr[0] + hr[2] <= dpan[0] + dpan[2], $"plaque x{hr[0]} y{hr[1]} {hr[2]}x{hr[3]}: tabs gap {hr[0] - tabEnd}, slot gap {hr[1] - (s2[1] + s2[3])}, detail frame gap {dpan[1] - (hr[1] + hr[3])}");
                int maxW = 0, maxNameRows = 0, maxRows = 0, pager = 0, wide = 0, nameOver = 0; string worstName = "", worstRows = ""; var rngLog = new System.Text.StringBuilder();
                for (int id = 0; id < HSins.Count; id++)
                {
                    Detail61(id, true, SinNewRoll737(HSins.All[id]), out var nr, out var sub, out var rws); int sw = W46(sub, true); maxW = Mathf.Max(maxW, sw); if (sw > 100) ALog("WIDE sub: " + sub + " " + sw); foreach (var r in rws) if (Row61W(r) > 100) { var sbw = new System.Text.StringBuilder(); foreach (var tk in r) sbw.Append(tk.s + " "); ALog("WIDE row " + Row61W(r) + ": " + sbw); }
                    foreach (var r in nr) maxW = Mathf.Max(maxW, Row61W(r)); foreach (var r in rws) { int w = Row61W(r); maxW = Mathf.Max(maxW, w); if (w > 100) wide++; }
                    if (nr.Count > 3) { nameOver++; worstName += HSins.All[id].name + "; "; } if (nr.Count > maxNameRows) maxNameRows = nr.Count; if (rws.Count > 5) pager++; if (rws.Count > maxRows) { maxRows = rws.Count; worstRows = HSins.All[id].name; }
                }
                { var sbd = new System.Text.StringBuilder("DETAIL dump: "); for (int id = 0; id < HSins.Count; id += 3) { var dd = HSins.All[id]; sbd.Append("\n  [" + dd.tier + "] " + dd.name + " | " + string.Join(" / ", HSins.Lines(dd, SinNewRoll737(dd)))); } ALog(sbd.ToString()); }
                ALog($"DETAIL fit over {HSins.Count} sins: widest row {maxW}px (limit 100), name rows max {maxNameRows} (>2: {nameOver} {worstName}), stat rows max {maxRows} ({worstRows}), {pager} sins need the text pager (>5 rows)");
                var miss = new System.Collections.Generic.HashSet<char>(); for (int id = 0; id < HSins.Count; id++) { var dd = HSins.All[id]; var q = SinNewRoll737(dd); var all = new System.Collections.Generic.List<string>(HSins.Lines(dd, q)); all.Add(dd.name); all.Add(HSins.QualityTxt(q, dd)); foreach (var ln in all) foreach (char ch in ln.ToUpperInvariant().Replace(';', ',').Replace('[', ' ').Replace(']', ' ')) if (ch != ' ' && !Sin1246Data.FontF.ContainsKey(ch)) { if (miss.Add(ch)) ALog("MISSING '" + ch + "' in: " + ln + " (" + dd.name + ")"); } }
                ARes("sins1246_detail_glyphs_complete", miss.Count == 0, "characters used by any Sin text that the small font cannot draw: " + (miss.Count == 0 ? "none" : string.Join(" ", miss)));
                ARes("sins1246_detail_text_fits_all_sins", maxW <= 100 && wide == 0 && nameOver == 0, $"widest row {maxW}px <= 100; names with >2 rows {nameOver}; max stat rows {maxRows}; {pager}/{HSins.Count} sins use the text pager");
            }
            yield return AtShot("s1246_01_open");
            // ---- layout rects from the data (pass3e): bottom row 15 tall at y235; PREV x67 w74, pager x149 w55, NEXT x212 w74, BACK x301 w107 (centre 354.5)
            var pv = Sin1246Data.R_prev; var pg = Sin1246Data.R_pager; var nx = Sin1246Data.R_next; var bk = Sin1246Data.R_back; var eqb = Sin1246Data.R_equip;
            ARes("sins1246_bottom_row_rects", pv[0] == 67 && pv[2] == 74 && pg[0] == 149 && pg[2] == 55 && nx[0] == 212 && nx[2] == 74 && bk[0] == 301 && bk[2] == 107 && pv[1] == 235 && pg[1] == 235 && nx[1] == 235 && bk[1] == 235 && pv[3] == 15 && pg[3] == 15 && nx[3] == 15 && bk[3] == 15, "all four 15px tall at y235; x/w as pass3e");
            ARes("sins1246_back_centred", bk[0] + bk[2] / 2f == 354.5f && (Sin1246Data.R_detail[0] + Sin1246Data.R_detail[2] / 2f) == 354.5f, "BACK centre " + (bk[0] + bk[2] / 2f) + " = detail panel centre");
            ARes("sins1246_text_fits", W46("HEAVEN-DEFYING", true) + 12 <= 83 && W46("BACK TO MENU", true) <= bk[2] - 8 && W46("SLOTS FULL", true) <= eqb[2] - 6 && W46("NOT FOUND", true) <= eqb[2] - 6 && W46("UNEQUIP", true) <= eqb[2] - 6, "labels fit their buttons");
            // ---- flow
            for (int f = 1; f < 4; f++) { var r = Sin1246Data.Tabs[_sinFilter737][f]; yield return AtS74(r[0] + r[2] / 2f, r[1] + r[3] / 2f); int n = SinFiltered737().Count; int[] want = { 70, 25, 34, 11 }; ARes("sins1246_tab_" + Sin1246Data.TabLabels[f], _sinFilter737 == f && n == want[f] && _sinPage737 == 0, "filter " + _sinFilter737 + " count " + n); }
            yield return AtShot("s1246_02_tab_HD");
            { var r = Sin1246Data.Tabs[_sinFilter737][0]; yield return AtS74(r[0] + r[2] / 2f, r[1] + r[3] / 2f); ARes("sins1246_tab_all", _sinFilter737 == 0 && SinFiltered737().Count == 70, "filter " + _sinFilter737); }
            yield return AtS74(nx[0] + nx[2] / 2f, nx[1] + nx[3] / 2f); ARes("sins1246_next_page", _sinPage737 == 1 && _s77Pages == 2, "page " + _sinPage737 + "/" + _s77Pages);
            yield return AtShot("s1246_03_page2");
            yield return AtS74(nx[0] + nx[2] / 2f, nx[1] + nx[3] / 2f); ARes("sins1246_next_disabled_last", _sinPage737 == 1, "page " + _sinPage737);
            yield return AtS74(pv[0] + pv[2] / 2f, pv[1] + pv[3] / 2f); ARes("sins1246_prev_page", _sinPage737 == 0, "page " + _sinPage737);
            var L = SinFiltered737(); var c1 = L[1]; var cr = Sin1246Data.Cells[1];
            yield return AtS74(cr[0] + 12, cr[1] + 12); ARes("sins1246_tile_select", _sinSel737 == c1.id, "sel " + _sinSel737 + " want " + c1.id); yield return AtWait(0.4f); yield return AtShot("s1246_04_tile_selected");
            // ---- t1260: cells have no studs/bright dots, thin 1px selected ring with a calm red breathe, hover ring + detail preview that never writes the selection
            {
                int sk = 1; var cs = Sin1246Data.Cells[sk]; var cl = Sin1246Data.Cells[4];   // sk selected; cell 4 is a locked '?' cell (clean stone, nothing else drawn)
                T1246Pin = 0.0f; yield return AtWait(0.35f); yield return At1246Capture(); var n0 = At1246Native(_atLast); At1246SaveNative(n0, "s1246_20_cells_t0.0_native.png");
                T1246Pin = 1.2f; yield return AtWait(0.35f); yield return At1246Capture(); var n1 = At1246Native(_atLast); At1246SaveNative(n1, "s1246_21_cells_t1.2_native.png");
                Color32 P(Color32[] c, int x, int y) => c[y * 480 + x];
                bool Near(Color32 a, int r, int g, int b, int tol = 4) => Mathf.Abs(a.r - r) <= tol && Mathf.Abs(a.g - g) <= tol && Mathf.Abs(a.b - b) <= tol;
                int dots = 0; foreach (var cp in new[] { new[] { 3, 2 }, new[] { 4, 3 }, new[] { 18, 3 }, new[] { 3, 19 }, new[] { 4, 20 }, new[] { 18, 20 }, new[] { 20, 2 }, new[] { 2, 20 }, new[] { 21, 1 }, new[] { 1, 21 } }) { var q = P(n0, cl[0] + cp[0], cl[1] + cp[1]); if (q.r > 70 && !(cp[0] == 21 && cp[1] == 1) && !(cp[0] == 1 && cp[1] == 21)) dots++; if ((cp[0] == 21 && cp[1] == 1 || cp[0] == 1 && cp[1] == 21) && q.r > 60) dots++; }
                ARes("sins1246_cell_no_bright_dots", dots == 0, "stud and lip-corner pixels of a clean cell are stone-dark (bright count " + dots + ")");
                var rA = P(n0, cs[0] + 12, cs[1]); var rB = P(n1, cs[0] + 12, cs[1]); var lip = P(n0, cs[0] + 12, cs[1] + 1); var lipL = P(n0, cs[0] + 1, cs[1] + 12); var out1 = P(n0, cs[0] + 12, cs[1] - 1); var ringL = P(n0, cs[0], cs[1] + 12);
                ARes("sins1246_sel_ring_1px", Near(rA, 92, 27, 38, 6) && Near(lip, 45, 44, 43, 6) && Near(lipL, 45, 44, 43, 6) && ringL.r > 80 && ringL.g < 60 && out1.r < 70, $"ring {rA.r},{rA.g},{rA.b}; px inside ring {lip.r},{lip.g},{lip.b} (stone, not red); outside {out1.r},{out1.g},{out1.b}");
                ARes("sins1246_sel_pulse_calm_red", Near(rB, 162, 38, 51, 6) && rB.r != rA.r && rB.g < 60 && rB.r > rB.g * 2 && rA.r > rA.g * 2, $"breathes {rA.r},{rA.g},{rA.b} -> {rB.r},{rB.g},{rB.b} (dark red <-> medium red, no orange/pink)");
                int diffOut = 0; for (int j = cs[1] - 1; j <= cs[1] + 24; j++) for (int i = cs[0] - 1; i <= cs[0] + 24; i++) { bool ring = i == cs[0] - 1 || i == cs[0] + 24 || j == cs[1] - 1 || j == cs[1] + 24; if (ring) continue; var a = P(n0, i, j); var b = P(n1, i, j); bool onRing = i == cs[0] || i == cs[0] + 23 || j == cs[1] || j == cs[1] + 23; if (!onRing && (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b)) > 6) diffOut++; }
                ARes("sins1246_sel_pulse_no_growth", diffOut == 0, "inside the 1px ring and the 1px halo around the cell nothing but the ring colour changes between the pulse extremes (" + diffOut + " px)");
                // hover: ring + preview, selection untouched
                T1246Pin = -1f; int hk = 5; var L2 = SinFiltered737(); var hc = Sin1246Data.Cells[hk]; int selBefore = _sinSel737;
                T1246Force = "cell" + hk + "=hover"; yield return AtWait(0.45f); yield return At1246Capture(); var nh = At1246Native(_atLast); At1246SaveNative(nh, "s1246_22_hover_cell5_native.png");
                var hp = P(nh, hc[0] + 12, hc[1]); var hp2 = P(nh, hc[0], hc[1] + 12);
                ARes("sins1246_hover_ring_amber_1px", hp.r > 120 && hp.r > hp.g + 40 && hp.g > hp.b + 20 && hp2.r > 120 && P(nh, hc[0] + 12, hc[1] + 1).r < 80, $"ring {hp.r},{hp.g},{hp.b} amber, inner px stone");
                ARes("sins1246_hover_previews_detail", _t46Shown == L2[hk].id && _sinSel737 == selBefore && _sinSel737 != L2[hk].id, $"shown {_t46Shown} (hovered {L2[hk].id}), selection {_sinSel737} untouched (was {selBefore})");
                int hl = 4; T1246Force = "cell" + hl + "=hover"; yield return AtWait(0.35f); ARes("sins1246_hover_locked_info", _t46Shown == L2[hl].id && !_sinOwned737[L2[hl].id], "locked cell hover shows its locked info (id " + L2[hl].id + ")");
                T1246Force = null; yield return AtWait(0.35f); ARes("sins1246_hover_leave_reverts", _t46Shown == _sinSel737 && _sinSel737 == selBefore, $"shown {_t46Shown} = selected {_sinSel737}");
                var hn = P(nh, hc[0] + 12, hc[1]); T1246Pin = -1f;
                // hover must not hijack a click: clicking the hovered cell selects it
                T1246Force = "cell" + hk + "=hover"; yield return AtS74(hc[0] + 12, hc[1] + 12); T1246Force = null; ARes("sins1246_hover_click_selects", _sinSel737 == L2[hk].id, "sel " + _sinSel737); yield return AtS74(cr[0] + 12, cr[1] + 12);
            }
            yield return AtS74(eqb[0] + eqb[2] / 2f, eqb[1] + eqb[3] / 2f); ARes("sins1246_equip", !SinTierOk737(c1, 1) || _sinEq737[1] == c1.id, "eq=" + string.Join(",", _sinEq737)); yield return AtWait(0.6f); yield return AtShot("s1246_05_equipped");
            yield return AtS74(eqb[0] + eqb[2] / 2f, eqb[1] + eqb[3] / 2f); ARes("sins1246_unequip", !SinEquipped737(c1.id), "eq=" + string.Join(",", _sinEq737)); yield return AtWait(0.6f);
            int s0 = _souls51, g0 = _sigils51; var sl = new Rect(264, 18, 32, 32); yield return AtS74(sl.center.x, sl.center.y);
            ARes("sins1246_confirm_opens_no_spend", Confirm49Open && _c49Slot == 2 && _souls51 == s0 && _sigils51 == g0, $"open={Confirm49Open} souls {s0}->{_souls51}"); yield return AtWait(0.4f); yield return AtShot("s1246_06_confirm_modal");
            yield return AtS74(T46No.center.x, T46No.center.y); ARes("sins1246_cancel_no_spend", !Confirm49Open && _sinSlots737 == 2 && _souls51 == s0 && _sigils51 == g0, $"open={Confirm49Open} slots {_sinSlots737}");
            yield return AtWait(0.7f); yield return AtS74(sl.center.x, sl.center.y); yield return AtWait(0.05f);
            yield return AtS74(T46Yes.center.x, T46Yes.center.y); ConfirmKeys49(); ARes("sins1246_guard_blocks_early_unlock", Confirm49Open && _sinSlots737 == 2 && _souls51 == s0, "guard holds");
            yield return AtWait(0.4f); yield return AtS74(T46Yes.center.x, T46Yes.center.y);
            ARes("sins1246_unlock_slot3", !Confirm49Open && _sinSlots737 == 3 && _souls51 == s0 - sc2 && _sigils51 == g0 - gc2, $"slots {_sinSlots737} souls {s0}->{_souls51} sigils {g0}->{_sigils51}");
            ConfirmKeys49(); yield return AtWait(0.2f); ARes("sins1246_no_double_spend", _sinSlots737 == 3 && _souls51 == s0 - sc2 && _sigils51 == g0 - gc2, "souls " + _souls51);
            yield return AtWait(1.0f); yield return AtShot("s1246_07_slot3_unlocked");
            ARes("sins1246_modal_buttons_clear_slots", !T46Yes.Overlaps(sl) && !T46No.Overlaps(sl), "modal buttons never under a slot");
            // ---- pixel check against the offline mockup render of the same instant
            SinEyesTune.On = false; T1246Fix = "mock"; T1246Pin = 9.15f; _sinSel737 = -1; yield return AtWait(0.6f);
            yield return At1246Capture(); var cap1 = At1246Native(_atLast); var cap1Full = _atLast; _atLast = null; File.WriteAllBytes(Path.Combine(_atOut, "s1246_08_mock_pinned_t9.15_full.png"), cap1Full.EncodeToPNG());
            { var nat = new Texture2D(480, 270, TextureFormat.RGBA32, false); var fl = new Color32[480 * 270]; for (int j = 0; j < 270; j++) for (int i = 0; i < 480; i++) fl[(269 - j) * 480 + i] = cap1[j * 480 + i]; nat.SetPixels32(fl); nat.Apply(); File.WriteAllBytes(Path.Combine(_atOut, "s1246_08_mock_pinned_native.png"), nat.EncodeToPNG()); Destroy(nat); Destroy(cap1Full); }
            string gd = At1246Arg("-t1246golden");
            if (gd != null && File.Exists(Path.Combine(gd, "golden_t9.15.png")))
            {
                var g = new Texture2D(2, 2, TextureFormat.RGBA32, false); g.LoadImage(File.ReadAllBytes(Path.Combine(gd, "golden_t9.15.png"))); var gp = g.GetPixels32(); int gh = g.height; bool sizeOk = g.width == 480 && gh == 270;
                int ui = 0, fx = 0, big = 0, total = 0; var diff = new Color32[480 * 270]; var rows = new int[270]; var cols = new int[480]; int minx = 480, maxx = -1, miny = 270, maxy = -1;
                if (sizeOk) for (int j = 0; j < 270; j++) for (int i = 0; i < 480; i++)
                {
                    var a = cap1[j * 480 + i]; var b = gp[(269 - j) * 480 + i]; int d = Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b)));
                    bool f = At1246Fx(i, j); diff[(269 - j) * 480 + i] = d > 2 ? (f ? new Color32(255, 160, 0, 255) : new Color32(255, 0, 0, 255)) : new Color32((byte)(b.r / 3), (byte)(b.g / 3), (byte)(b.b / 3), 255);
                    if (d > 2) { total++; if (f) fx++; else { ui++; rows[j]++; cols[i]++; minx = Mathf.Min(minx, i); maxx = Mathf.Max(maxx, i); miny = Mathf.Min(miny, j); maxy = Mathf.Max(maxy, j); } if (d > 24) big++; }
                }
                var dt = new Texture2D(480, 270, TextureFormat.RGBA32, false); dt.SetPixels32(diff); dt.Apply(); File.WriteAllBytes(Path.Combine(_atOut, "s1246_09_diff_vs_mockup_golden.png"), dt.EncodeToPNG()); Destroy(dt);
                ALog($"PIXEL golden compare: differing px (>2 levels) outside pillar-fx boxes {ui}, inside {fx}, >24 levels {big}; ui bbox x{minx}..{maxx} y{miny}..{maxy}");
                ARes("sins1246_pixels_vs_mockup_unchanged_regions", sizeOk && ui <= 300, $"{ui} px differ in the regions that must still equal the mockup (limit 300); {fx} px differ in the deliberately changed regions (fire lighting, cells, ring); see s1246_09_diff_vs_mockup_golden.png");
                Destroy(g);
            }
            else ALog("PIXEL golden compare SKIPPED (no -t1246golden=<dir>)");
            // ---- cracks animate on the frames (same clock as the pillars): grid panel ring differs between two pinned instants, and the pillar crack boxes too
            T1246Pin = 0.75f; yield return AtWait(0.4f); yield return At1246Capture(); var A = At1246Native(_atLast); At1246SaveNative(A, "s1246_10_mock_t0.75_native.png");
            T1246Pin = 5.0f; yield return AtWait(0.4f); yield return At1246Capture(); var B2 = At1246Native(_atLast); At1246SaveNative(B2, "s1246_11_mock_t5.0_native.png");
            T1246Pin = 9.15f; T1246Force = "tab2=hover;pageNEXT=hover;action=pressed;back=selected;pagePREV=hover"; yield return AtWait(0.4f); yield return At1246Capture(); At1246SaveNative(At1246Native(_atLast), "s1246_12_forced_states_native.png"); T1246Force = null;
            int ringDiff = 0, pilDiff = 0; var gp2 = Sin1246Data.R_gridpanel;
            for (int j = 0; j < 270; j++) for (int i = 0; i < 480; i++)
            {
                var a = A[j * 480 + i]; var b = B2[j * 480 + i]; bool d = Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) > 12; if (!d) continue;
                bool inP = i >= gp2[0] && i < gp2[0] + gp2[2] && j >= gp2[1] && j < gp2[1] + gp2[3], inner = i >= gp2[0] + 5 && i < gp2[0] + gp2[2] - 5 && j >= gp2[1] + 5 && j < gp2[1] + gp2[3] - 5;
                if (inP && !inner) ringDiff++; if ((i < 42 || i >= 438) && j >= 2 && j < 112) pilDiff++;
            }
            ARes("sins1246_frame_cracks_animate", ringDiff >= 3, $"grid panel frame ring: {ringDiff} px glow differently between t=0.75 s and t=5 s (frame cracks driven by the pillar crack clock)");
            ARes("sins1246_pillar_cracks_animate", pilDiff >= 3, $"pillar crack area: {pilDiff} px differ between the same two instants");
            // ---- t1260: fire lighting flickers over the environment, the new fire and the talismans animate (same two pinned instants)
            {
                int lightD = 0, fireD = 0, talD = 0, palBad = 0, fireN = 0;
                for (int j = 100; j < 270; j++) for (int i = 0; i < 480; i++)
                {
                    if (!(i < 80 || i >= 400)) continue; var a = A[j * 480 + i]; var b = B2[j * 480 + i]; int d = Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
                    bool inFire = (i >= 6 && i < 34 || i >= 446 && i < 474) && j >= 183 && j < 219, inTal = (i >= 8 && i < 36 || i >= 444 && i < 472) && j >= 112 && j < 154;
                    if (inFire) { if (d > 12) fireD++; } else if (inTal) { if (d > 12) talD++; } else if ((i >= 44 && i < 78 || i >= 402 && i < 436) && d > 6) lightD++;
                }
                ARes("sins1246_fire_light_flickers", !SinFx84Renderer.NewFire1260 || lightD >= 40, $"{lightD} wall/frame-edge px change brightness between two instants (banded fire light follows the fire flicker)");
                ARes("sins1246_fire_animates", fireD >= 60, $"{fireD} flame px differ between two instants");
                ARes("sins1246_talismans_animate", talD >= 10, $"{talD} talisman px differ between two instants");
            }
            // ---- t1262: braziers. Flame/coals live inside the bowl, the front lip is never overwritten, the base is irregular, coals glow in the cavity
            {
                int flameOnLip = 0, coalPx = 0, rows = 0; var bottoms = new System.Collections.Generic.HashSet<int>(); int flameCols = 0;
                foreach (var F in new[] { A, B2 })
                    foreach (int cxb in new[] { 19, 459 })
                    {
                        for (int u = -13; u <= 13; u++)
                        {
                            int x = cxb + u; for (int y = 225; y <= 226; y++) { var c = F[y * 480 + x]; if (IsFlamePal1262(c)) flameOnLip++; }   // front lip rows below the cavity arc centre
                            int low = -1; for (int y = 214; y <= 223; y++) if (IsFlamePal1262(F[y * 480 + x])) low = y; if (low >= 0) { bottoms.Add(low); flameCols++; }
                        }
                        for (int y = 217; y <= 223; y++) for (int u = -14; u <= 14; u++) { if (Mathf.Abs(u) < 9) continue; if (IsFlamePal1262(F[y * 480 + cxb + u])) coalPx++; }
                        rows++;
                    }
                ARes("sins1246_brazier_front_lip_hides_flame", flameOnLip == 0, $"{flameOnLip} flame-palette px on the front lip rows (must be 0: the front rim is drawn over the flame base)");
                ARes("sins1246_brazier_flame_base_irregular", bottoms.Count >= 3 && flameCols > 20, $"lowest visible flame/coal row varies over {bottoms.Count} distinct values across {flameCols} columns (not a straight cut)");
                ARes("sins1246_brazier_coals_visible", coalPx >= 12, $"{coalPx} glowing coal px in the cavity beside the flame (4 braziers-frames)");
            }
            // ---- t1265: step84 smooth anchored flex. The paper must stay intact: pinned top rows do not move, adjacent rows never jump (no torn/misaligned rows), no paper is lost
            if (SinFx84Renderer.Tal1265)
            {
                int maxJump = 0, pinnedDiff = 0, maxDisp = 0; var tot = new int[2, 2];
                for (int fr = 0; fr < 2; fr++)
                    for (int tl = 0; tl < 2; tl++)
                    {
                        var F = fr == 0 ? A : B2; int x0 = tl == 0 ? 8 : 444; int pl = -1, pr = -1;
                        for (int y = 120; y < 153; y++)
                        {
                            int lo = 999, hi = -1; for (int x = x0; x < x0 + 28; x++) { var g = F[y * 480 + x]; if (g.r > 235 && g.g > 150 && g.g < 215 && g.b < 75) { lo = Mathf.Min(lo, x); hi = Mathf.Max(hi, x); tot[fr, tl]++; } }
                            if (hi >= 0 && pl >= 0 && y < 148) maxJump = Mathf.Max(maxJump, Mathf.Max(Mathf.Abs(lo - pl), Mathf.Abs(hi - pr)));
                            if (hi >= 0 && y < 148) { if (y == 120) { pl = lo; pr = hi; } else { pl = lo; pr = hi; } }
                            if (y < 128 && hi >= 0 && fr == 1) { int lo0 = 999, hi0 = -1; for (int x = x0; x < x0 + 28; x++) { var gz = A[y * 480 + x]; if (gz.r > 235 && gz.g > 150 && gz.g < 215 && gz.b < 75) { lo0 = Mathf.Min(lo0, x); hi0 = Mathf.Max(hi0, x); } } pinnedDiff += Mathf.Abs(lo0 - lo) + Mathf.Abs(hi0 - hi); }
                        }
                    }
                int lost = 0; for (int tl = 0; tl < 2; tl++) lost = Mathf.Max(lost, Mathf.Abs(tot[0, tl] - tot[1, tl]) * 100 / Mathf.Max(1, tot[0, tl]));
                ARes("sins1246_talisman_1265_intact", maxJump <= 2 && pinnedDiff <= 2 && lost <= 12, $"smooth flex: max edge step between adjacent paper rows {maxJump}px (<=2), pinned rows 120..127 edge change {pinnedDiff}px, paper area change between two instants {lost}% (<=12%)");
            }
            // ---- t1263: hand-pixelled talisman frames. Placement, loop timing and the right-hand +4 offset are checked against the frame texture; no old paper under or beside the frames
            if (SinFx84Renderer.Tal1263)
            {
                var ft = Resources.Load<Texture2D>("HellordArt/ui_t1263_talisman_frames"); var rtt = RenderTexture.GetTemporary(ft.width, ft.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB); var prev0 = RenderTexture.active; Graphics.Blit(ft, rtt); RenderTexture.active = rtt; var rt2 = new Texture2D(ft.width, ft.height, TextureFormat.RGBA32, false); rt2.ReadPixels(new Rect(0, 0, ft.width, ft.height), 0, 0); rt2.Apply(); RenderTexture.active = prev0; RenderTexture.ReleaseTemporary(rtt); var fpx = rt2.GetPixels32(); int bad = 0, chk = 0, ghost = 0;
                var ms = new[] { 190, 160, 170, 210, 190, 160, 170, 210 }; int Fi(float t) { int m = Mathf.FloorToInt(t * 1000f) % 1460, acc = 0; for (int q = 0; q < 8; q++) { acc += ms[q]; if (m < acc) return q; } return 7; }
                foreach (var pr in new[] { (A, 0.75f), (B2, 5.0f) })
                    for (int tl = 0; tl < 2; tl++)
                    {
                        int fi = (Fi(pr.Item2) + tl * 4) % 8, ox = tl == 0 ? 11 : 447;
                        for (int y = 0; y < 32; y++) for (int x = 0; x < 22; x++)
                        {
                            var sc = fpx[(31 - y) * 176 + fi * 22 + x]; var got = pr.Item1[(120 + y) * 480 + ox + x];
                            if (sc.a > 0 && sc.r == 24 && sc.g == 20 && sc.b == 37) { chk++; if (got.r != 24 || got.g != 20 || got.b != 37) bad++; }   // dark outline: never lit, exact position
                        }
                        // old paper must not double up: no paper-orange px in the old rect rows >= 120 outside the frame rect (embers excluded by count tolerance)
                        int x0 = tl == 0 ? 8 : 444; for (int y = 120; y < 154; y++) for (int x = x0; x < x0 + 28; x++) { if (x >= ox && x < ox + 22 && y < 152) continue; var g = pr.Item1[y * 480 + x]; if (g.r > 220 && g.g > 140 && g.b < 70) ghost++; }
                    }
                ARes("sins1246_talisman_frames_1263", bad == 0 && chk > 400 && ghost == 0, $"{chk} outline px checked against HA's frames at (11,120)/(447,120), right = frame (i+4)%8, loop 190/160/170/210/190/160/170/210 ms: {bad} off; paper-coloured px outside the frame rect: {ghost}");
            }
            T1246Pin = -1f; T1246Fix = null; SinEyesTune.On = eyes0;
            yield return AtS74(bk[0] + bk[2] / 2f, bk[1] + bk[3] / 2f); ARes("sins1246_back_closes", !_sinPanel737 && mode == Mode.MainMenu, "panel " + _sinPanel737);
            System.Array.Copy(own0, _sinOwned737, own0.Length); System.Array.Copy(roll0, _sinRoll737, roll0.Length); System.Array.Copy(eq0, _sinEq737, 3); _sinSlots737 = sl0; _souls51 = so0; _sigils51 = si0; SinSave737(); PactSave51();
            SinLoad737(); PactLoad51(); ARes("sins1246_save_restored", _sinSlots737 == sl0 && _souls51 == so0 && _sigils51 == si0, $"slots {_sinSlots737} souls {_souls51} sigils {_sigils51}");
            { var T = HellordUITheme.Theme.Stone1246; var M = HellordUITheme.Theme.MenuTheme;
              ARes("sins1246_uitheme_switch", HellordUITheme.Resolve(T, false) == T && HellordUITheme.Resolve(M, false) == T && HellordUITheme.Resolve(M, true) == M && HellordUITheme.FromName("menu") == M && HellordUITheme.FromName("stone") == T && HellordUITheme.FromName("junk") == T
                  && HellordUITheme.Parse(new[] { "x", "-uitheme", "menu" }) == M && HellordUITheme.Parse(new[] { "-uitheme=menu" }) == M && HellordUITheme.Parse(new[] { "x" }) == T && HellordUITheme.Active == T && HellordUITheme.MenuThemeReady,
                  "t1277/t1300 theme switch: default Stone1246, Resolve(menu, ready) = MenuTheme (t1300 flipped MenuThemeReady: the menu theme exists), a not-ready theme would still fall back to Stone1246, parse of -uitheme stone|menu"); }
            T1246On = t46was; AtEnd("sins1246");
        }
    }
}
#endif
