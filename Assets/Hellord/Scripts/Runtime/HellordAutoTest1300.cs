#if HELLORD_AUTOTEST
// t1300: autotest + capture scenario for the menu-theme Sins screen (HellordMenuTheme.cs).
//   -atOnly menutheme1300 [-mtmock=<V6 clean 640x360 png>] [-mtmode=stills|video]   (also run with -screen-width/-height 1280x720 and 1920x1080 for the integer-scale checks)
// Input comes from the screen's own fake-click hook (native 640x360 px) and force strings, the same way as the stone tests; the real mouse is ignored (reported as untested).
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        IEnumerator At1300Click(float x, float y) { MenuThemeFake = new Vector2(x, y); float t = 0; while (MenuThemeFake.HasValue && t < 3f) { t += Time.unscaledDeltaTime; yield return null; } yield return null; yield return null; }
        IEnumerator At1300Cap() { yield return new WaitForEndOfFrame(); if (_atLast) Destroy(_atLast); _atLast = ScreenCapture.CaptureScreenshotAsTexture(); }
        /// <summary>screenshot -> native 640x360 Color32 rows top-down (one sample per integer-scale block)</summary>
        Color32[] At1300Native(Texture2D cap)
        {
            int s = MenuThemeScaleUsed, W = cap.width, H = cap.height, ox = Mathf.RoundToInt(W / 2f - s * 320f), oy = Mathf.RoundToInt(H / 2f - s * 180f); var px = cap.GetPixels32(); var o = new Color32[640 * 360];
            for (int j = 0; j < 360; j++) for (int i = 0; i < 640; i++) { int sx = ox + s * i + s / 2, sy = oy + s * j + s / 2; o[j * 640 + i] = sx >= 0 && sx < W && sy >= 0 && sy < H ? px[(H - 1 - sy) * W + sx] : new Color32(0, 0, 0, 255); }
            return o;
        }
        /// <summary>true when the screenshot is an exact nearest-neighbour s-fold enlargement of its native 640x360 grid (every block uniform)</summary>
        int At1300BlockErrors(Texture2D cap)
        {
            int s = MenuThemeScaleUsed, W = cap.width, H = cap.height, ox = Mathf.RoundToInt(W / 2f - s * 320f), oy = Mathf.RoundToInt(H / 2f - s * 180f); var px = cap.GetPixels32(); int bad = 0;
            for (int j = 0; j < 360; j += 3) for (int i = 0; i < 640; i += 3)
            {
                if (j >= 340 && i >= 430) continue; // dev-player 'Development Build' watermark (Unity's own, native-res text)
                int bx = ox + s * i, by = oy + s * j; if (bx < 0 || by < 0 || bx + s > W || by + s > H) continue; var c0 = px[(H - 1 - by) * W + bx];
                for (int dy = 0; dy < s; dy++) for (int dx = 0; dx < s; dx++) { var c = px[(H - 1 - (by + dy)) * W + bx + dx]; if (c.r != c0.r || c.g != c0.g || c.b != c0.b) bad++; }
            }
            return bad;
        }
        static Color32[] At1300LoadPng(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null; var t = new Texture2D(2, 2, TextureFormat.RGBA32, false); t.LoadImage(File.ReadAllBytes(path)); if (t.width != 640 || t.height != 360) return null;
            var p = t.GetPixels32(); var o = new Color32[640 * 360]; for (int j = 0; j < 360; j++) for (int i = 0; i < 640; i++) o[j * 640 + i] = p[(359 - j) * 640 + i]; Destroy(t); return o;
        }
        static bool At1300Warm(Color32 c) { return c.r > 150 && c.b < 120 && c.r > c.g + 20; }
        IEnumerator AtMenuTheme1300()
        {
            yield return AtBegin("menutheme1300"); { string rs = At1246Arg("-mtres"); if (rs == "1080") { Screen.SetResolution(1920, 1080, false); yield return AtWait(0.6f); } } string mode1300 = At1246Arg("-mtmode"); var themeWas = HellordUITheme.Requested; bool hoverWas = T1246HoverOff;
            HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; T1246HoverOff = true; MenuThemeIgnoreMouse = false; MenuThemePin = -1f; MenuThemeForce = null;
            SinLoad737(); PactLoad51();
            var own0 = (bool[])_sinOwned737.Clone(); var roll0 = (float[][])_sinRoll737.Clone(); var eq0 = (int[])_sinEq737.Clone(); int sl0 = _sinSlots737, so0 = _souls51, si0 = _sigils51;
            AtClearWorld(); mode = Mode.MainMenu;
            for (int i = 0; i < HSins.Count; i++) { bool o = i % 9 != 4; _sinOwned737[i] = o; _sinRoll737[i] = o ? SinNewRoll737(HSins.All[i]) : null; }
            for (int i = 0; i < 3; i++) _sinEq737[i] = -1; _sinSlots737 = 2; _souls51 = 1700; _sigils51 = 15; _sinFilter737 = 0; SinSave737(); PactSave51();
            var dEq = HSins.All[0]; _sinEq737[0] = dEq.id; var dEq2 = HSins.All[10]; _sinEq737[1] = dEq2.id; SinOpen737(); _sinSel737 = HSins.All[25].id; yield return AtWait(0.4f);
            ARes("mt1300_active_and_drawn", HellordUITheme.Active == HellordUITheme.Theme.MenuTheme && MenuThemeDrawn && _sinPanel737, $"theme {HellordUITheme.Active}, drawn {MenuThemeDrawn}, scale {MenuThemeScaleUsed} at {Screen.width}x{Screen.height}");
            int expS = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(Screen.width / 640f, Screen.height / 360f) + 0.001f));
            ARes("mt1300_integer_scale", MenuThemeScaleUsed >= 1 && (MenuThemeScaleUsed == expS || MenuThemeScaleUsed == Mathf.RoundToInt(Mathf.Min(Screen.width / 640f, Screen.height / 360f))), $"scale {MenuThemeScaleUsed} (floor fit {expS}) at {Screen.width}x{Screen.height}");
            string outDir = _atOut; MenuThemePin = 12.34f; yield return AtWait(0.3f); yield return At1300Cap();
            ARes("mt1300_pixel_grid", At1300BlockErrors(_atLast) == 0, $"every sampled {MenuThemeScaleUsed}x{MenuThemeScaleUsed} block uniform (exact nearest-neighbour enlargement): {At1300BlockErrors(_atLast)} off");
            var A = At1300Native(_atLast); File.WriteAllBytes(Path.Combine(outDir, "mt_screen_" + Screen.width + "x" + Screen.height + ".png"), _atLast.EncodeToPNG());
            { var mock = At1300LoadPng(At1246Arg("-mtmock"));
              if (mock == null) ALog("mock png not given (-mtmock): pixel comparison skipped");
              else
              {   // regions that must equal the approved mockup exactly: background + arch + title (rows 0..69, x 48..591), panel top frames and the rail row; fire/embers/light live at x < 48 and x >= 592.
                  int bad = 0, chk = 0; for (int y = 0; y < 70; y++) for (int x = 48; x < 592; x++) { var a = A[y * 640 + x]; var m = mock[y * 640 + x]; chk++; if (a.r != m.r || a.g != m.g || a.b != m.b) bad++; }
                  for (int y = 138; y < 150; y++) for (int x = 48; x < 592; x++) { var a = A[y * 640 + x]; var m = mock[y * 640 + x]; chk++; if (a.r != m.r || a.g != m.g || a.b != m.b) bad++; }
                  ARes("mt1300_matches_mockup_static", bad == 0 && chk > 0, $"{chk} px of background + arch + title + panel tops compared with D2_MORTA_v6 dimB (mockup): {bad} differ");
                  // whole-screen: pixels that differ from the mockup are only where something is live (fire columns, text/icons: reported as a number)
                  int diff = 0; for (int i = 0; i < 640 * 360; i++) { var a = A[i]; var m = mock[i]; if (a.r != m.r || a.g != m.g || a.b != m.b) diff++; }
                  ALog($"t1300 whole-screen px differing from the mockup: {diff} of {640 * 360} (live fire/embers/light, runtime text and game icons)");
                  var heat = new Color32[640 * 360]; var hd = new Texture2D(640, 360, TextureFormat.RGBA32, false); for (int j = 0; j < 360; j++) for (int i = 0; i < 640; i++) { var a = A[j * 640 + i]; var m = mock[j * 640 + i]; bool d = a.r != m.r || a.g != m.g || a.b != m.b; heat[(359 - j) * 640 + i] = d ? new Color32(255, 0, 80, 255) : new Color32((byte)(m.r / 3), (byte)(m.g / 3), (byte)(m.b / 3), 255); }
                  hd.SetPixels32(heat); hd.Apply(); File.WriteAllBytes(Path.Combine(outDir, "mt_diff_vs_mockup_640x360.png"), hd.EncodeToPNG()); Destroy(hd);
                  var nt = new Texture2D(640, 360, TextureFormat.RGBA32, false); var nf = new Color32[640 * 360]; for (int j = 0; j < 360; j++) for (int i = 0; i < 640; i++) nf[(359 - j) * 640 + i] = A[j * 640 + i]; nt.SetPixels32(nf); nt.Apply(); File.WriteAllBytes(Path.Combine(outDir, "mt_native_640x360.png"), nt.EncodeToPNG()); Destroy(nt);
              } }
            // ---- fire: animated, fits the shaft, embers rise; parity numbers vs the main menu torch are measured offline (tools/t1300/measure_parity.py) on the 3x capture
            { var F0 = At1300Native(_atLast); int warm = 0, outside = 0; for (int y = 160; y < 214; y++) for (int x = 0; x < 44; x++) { var c = F0[y * 640 + x]; if (At1300Warm(c)) { warm++; if (x < 21 - 13 || (x >= 21 + 13 && x < 44)) outside++; } }
              MenuThemePin = 12.34f + 0.09f * 3; yield return AtWait(0.15f); yield return At1300Cap(); var F1 = At1300Native(_atLast); int moved = 0; for (int y = 160; y < 214; y++) for (int x = 8; x < 34; x++) { var a = F0[y * 640 + x]; var b = F1[y * 640 + x]; if (a.r != b.r || a.g != b.g || a.b != b.b) moved++; }
              ARes("mt1300_fire_animates", moved > 60 && warm > 150, $"left sconce: {warm} warm px above the bowl; {moved} px change between two instants 0.27 s apart; flame frame {MenuThemeFlameFrame}, light level {MenuThemeLightLevel}, {MenuThemeEmberCount} embers live");
              ARes("mt1300_fire_inside_shaft", outside <= 8, $"warm px outside the 26 px flame column x 8..33 (embers excluded by tolerance): {outside}"); }
            // ---- hover preview: forcing hover on a cell previews it in the detail panel, leaving reverts to the selected Sin (selection is never written)
            { int sel0 = _sinSel737; MenuThemeForce = "cell5=hover"; yield return AtWait(0.2f); int pv = _mtShown; string lh = MenuThemeLastHover; var list0 = SinFiltered737(); MenuThemeForce = null; yield return AtWait(0.2f);
              ARes("mt1300_hover_preview", lh == "cell5" && pv == list0[5].id && _mtShown == sel0 && _sinSel737 == sel0, $"hover cell5 -> preview {pv} (cell5 = {list0[5].id}), after leaving shows {_mtShown}, selection {_sinSel737} (was {sel0})"); }
            // ---- click a cell (fake click in native px) selects it; click a tab filters; paging; equip / unequip saves; back closes
            { var list0 = SinFiltered737(); int k = 3; yield return At1300Click(58 + (k % 8) * 30 + 14, 153 + (k / 8) * 30 + 14); ARes("mt1300_cell_select", _sinSel737 == list0[k].id, $"selected {_sinSel737}, cell {k} = {list0[k].id}"); }
            { int tx = 52; for (int i = 0; i < 2; i++) tx += MtW(new[] { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" }[i], true) + 22; int w2 = MtW("DEMONIC", true); yield return At1300Click(tx + w2 / 2, 129);
              var l2 = SinFiltered737(); bool allD = _sinFilter737 == 2 && l2.Count > 0 && _sinPage737 == 0; foreach (var d in l2) if (d.tier != SinTier.Demonic) allD = false; ARes("mt1300_tab_filter", allD, $"filter {_sinFilter737}, {l2.Count} Sins, all demonic {allD}, page {_sinPage737}, selection cleared {_sinSel737 == -1}");
              yield return At1300Click(52 + MtW("ALL", true) / 2, 129); ARes("mt1300_tab_all", _sinFilter737 == 0 && SinFiltered737().Count == HSins.Count, $"back to ALL: {SinFiltered737().Count}/{HSins.Count}"); }
            { _sinPage737 = 0; yield return AtWait(0.1f); yield return At1300Click(207 + 30, 330); int p1 = _sinPage737; yield return At1300Click(87 + 30, 330); int p0 = _sinPage737; yield return At1300Click(87 + 30, 330); ARes("mt1300_paging", p1 == 1 && p0 == 0 && _sinPage737 == 0, $"NEXT -> page {p1}, PREV -> {p0}, PREV at first page stays {_sinPage737} (disabled)"); }
            { int fid = -1; for (int i = 0; i < HSins.Count; i++) { var d = HSins.All[i]; if (_sinOwned737[d.id] && !SinEquipped737(d.id) && SinTierOk737(d, 2)) { fid = d.id; break; } }
              _sinSlots737 = 3; _sinFilter737 = 0; _sinPage737 = 0; _sinSel737 = fid; yield return AtWait(0.2f); int free = -1; for (int i = 0; i < 3; i++) if (_sinEq737[i] < 0) { free = i; break; }
              yield return At1300Click(337 + 118, 269 + 12); bool eq1 = SinEquipped737(fid); SinLoad737(); bool saved1 = SinEquipped737(fid);
              yield return AtWait(0.2f); yield return At1300Click(337 + 118, 269 + 12); SinLoad737(); bool gone = !SinEquipped737(fid);
              ARes("mt1300_equip_unequip_saves", fid >= 0 && free >= 0 && eq1 && saved1 && gone, $"sin {fid}: equip -> {eq1} (reloaded from save: {saved1}), UNEQUIP -> removed {gone}"); _sinSlots737 = 2; _sinEq737[2] = -1; SinSave737(); }
            // ---- locked slot: the confirm asks first, cancel debits nothing, confirm debits exactly once
            { int sc = Arr737(ST737.sinSlotSoulCost737, 2, 100), gc = Arr737(ST737.sinSlotSigilCost737, 2, 1); int s0 = _souls51, g0 = _sigils51; _sinSlots737 = 2; yield return AtWait(0.2f);
              yield return At1300Click(356 + 14, 74 + 16); bool asked = MenuThemeAskSlot == 2 && _souls51 == s0; yield return AtWait(0.4f); yield return At1300Click(208 + 28 + 30, 138 + 58 + 10); bool canceled = MenuThemeAskSlot == -1 && _souls51 == s0 && _sinSlots737 == 2;
              yield return At1300Click(356 + 14, 74 + 16); yield return AtWait(0.4f); yield return At1300Click(208 + 224 - 88 + 30, 138 + 58 + 10); bool done = MenuThemeAskSlot == -1 && _souls51 == s0 - sc && _sigils51 == g0 - gc && _sinSlots737 == 3;
              yield return AtWait(0.2f); yield return At1300Click(356 + 14, 74 + 16); bool once = _souls51 == s0 - sc && MenuThemeAskSlot == -1; PactLoad51(); bool savedOnce = _souls51 == s0 - sc && _sinSlots737 == 3;
              ARes("mt1300_unlock_confirm_once", asked && canceled && done && once && savedOnce, $"asks first {asked}, cancel keeps {s0} souls {canceled}, confirm debits {sc} souls + {gc} sigil once {done}, no second charge {once}, saved {savedOnce}"); }
            // ---- all 70 Sins fit the detail panel (<= 3 name rows, stat rows paginate), and every character has a hand-pixelled glyph
            { int maxRows = 0, over4 = 0, name3 = 0, uncovered = 0, n = 0; var miss = new HashSet<char>(); string firstMiss = "";
              for (int i = 0; i < HSins.Count; i++)
              {
                  var d = HSins.All[i]; MtDetailRows(d.id, true, SinNewRoll737(d), out var nm, out var sub, out var rows); n++; maxRows = Mathf.Max(maxRows, rows.Count); if (rows.Count > 4) over4++; if (nm.Count > 2) name3++; if (nm.Count > 3) uncovered += 100;
                  foreach (var l in nm) foreach (char ch in l) if (ch != ' ' && !MenuThemeData.Disp.ContainsKey(ch) && !MenuThemeData.Small.ContainsKey(ch)) { miss.Add(ch); firstMiss += d.name + ";"; }
                  foreach (var r in rows) foreach (char ch in r[0]) if (ch != ' ' && !MenuThemeData.Small.ContainsKey(ch)) miss.Add(ch);
                  foreach (char ch in sub) if (ch != ' ' && !MenuThemeData.Small.ContainsKey(ch)) miss.Add(ch);
              }
              string ms = ""; foreach (var ch in miss) ms += ch; ARes("mt1300_all_sins_fit_and_glyphs", n == HSins.Count && uncovered == 0 && miss.Count == 0, $"{n} Sins: max {maxRows} stat rows ({over4} need > 4 rows -> small pitch / paging), {name3} names use the small font (3+ rows), characters without a glyph: '{ms}'"); }
            // ---- default is still Stone1246; flag parse
            ARes("mt1300_default_is_stone", HellordUITheme.Parse(new[] { "x" }) == HellordUITheme.Theme.Stone1246 && HellordUITheme.Resolve(HellordUITheme.Theme.Stone1246, true) == HellordUITheme.Theme.Stone1246 && HellordUITheme.Parse(new[] { "-uitheme", "menu" }) == HellordUITheme.Theme.MenuTheme && HellordUITheme.MenuThemeReady, "no flag -> Stone1246; -uitheme menu -> MenuTheme; MenuThemeReady = true");
            if (mode1300 == "stills" || mode1300 == "video") yield return At1300Capture(mode1300, outDir);
            // ---- BACK closes
            _sinSel737 = HSins.All[25].id; yield return AtWait(0.1f); yield return At1300Click(387 + 68, 330); ARes("mt1300_back_closes", !_sinPanel737 && mode == Mode.MainMenu, $"panel {_sinPanel737} mode {mode}");
            System.Array.Copy(own0, _sinOwned737, own0.Length); System.Array.Copy(roll0, _sinRoll737, roll0.Length); System.Array.Copy(eq0, _sinEq737, 3); _sinSlots737 = sl0; _souls51 = so0; _sigils51 = si0; SinSave737(); PactSave51(); SinLoad737(); PactLoad51();
            ARes("mt1300_save_restored", _sinSlots737 == sl0 && _souls51 == so0 && _sigils51 == si0, $"slots {_sinSlots737} souls {_souls51} sigils {_sigils51}");
            MenuThemePin = -1f; MenuThemeForce = null; MenuThemeFake = null; HellordUITheme.Requested = themeWas; T1246HoverOff = hoverWas; AtEnd("menutheme1300");
        }
        /// <summary>runtime captures (3 s of pinned-clock frames at 30 fps for the video; stills at distinct instants), cropped sconce frames, 8x crops are cut offline from these PNGs.</summary>
        IEnumerator At1300Capture(string kind, string outDir)
        {
            var sd = Path.Combine(outDir, "capture"); Directory.CreateDirectory(sd); _sinSel737 = HSins.All[25].id; _sinFilter737 = 0; _sinPage737 = 0; _sinSlots737 = 2; MenuThemeForce = null;
            MenuThemePin = 12.34f; yield return AtWait(0.3f); yield return At1300Cap(); File.WriteAllBytes(Path.Combine(sd, "mt_runtime_" + Screen.width + "x" + Screen.height + "_still.png"), _atLast.EncodeToPNG());
            MenuThemeForce = "cell12=hover;back=hover"; yield return AtWait(0.2f); yield return At1300Cap(); File.WriteAllBytes(Path.Combine(sd, "mt_runtime_" + Screen.width + "x" + Screen.height + "_hover.png"), _atLast.EncodeToPNG()); MenuThemeForce = null;
            int N = kind == "video" ? int.Parse(At1246Arg("-mtn") ?? "120") : 12; var fd = Path.Combine(sd, "frames"); Directory.CreateDirectory(fd);
            for (int i = 0; i < N; i++)
            {
                MenuThemePin = 20f + i / 30f; yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(fd, i.ToString("0000") + ".png"), cap.EncodeToPNG()); Destroy(cap);
            }
            ALog("t1300 capture frames " + N + " in " + fd); MenuThemePin = 12.34f;
        }
    }
}
#endif
