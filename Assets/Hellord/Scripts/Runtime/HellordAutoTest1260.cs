#if HELLORD_AUTOTEST
// t1260: preview capture scenario (NOT a pass/fail test): -atOnly preview1260 -t1260mode=stills|video. Pins the Sins clock and drives hover through T1246Force,
// because the mouse cannot be faked; the click uses the same _s74Fake path as the other tests. Frames/shots go to the autotest output dir.
using System.Collections;
using System.IO;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        static bool i4(int v) { return v >= 0; }
        IEnumerator At1277Shot(string dir, string name) { yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(dir, name + ".png"), cap.EncodeToPNG()); Destroy(cap); ALog("t1277 shot " + name); }
        IEnumerator AtPreview1260()
        {
            yield return AtBegin("preview1260"); bool t46was = T1246On; T1246On = true; T1246HoverOff = true;
            int rw = int.Parse(At1246Arg("-t1260w") ?? "1280"), rh = int.Parse(At1246Arg("-t1260h") ?? "720"); Screen.SetResolution(rw, rh, false); yield return AtWait(0.8f);
            SinLoad737(); PactLoad51();
            var own0 = (bool[])_sinOwned737.Clone(); var roll0 = (float[][])_sinRoll737.Clone(); var eq0 = (int[])_sinEq737.Clone(); int sl0 = _sinSlots737, so0 = _souls51, si0 = _sigils51;
            AtClearWorld(); mode = Mode.MainMenu; SinOpen737(); yield return AtWait(0.3f);
            for (int i = 0; i < HSins.Count; i++) { bool o = i % 9 != 4; _sinOwned737[i] = o; _sinRoll737[i] = o ? SinNewRoll737(HSins.All[i]) : null; }
            _sinSlots737 = 2; _sinEq737[0] = 3; _sinEq737[1] = _sinEq737[2] = -1; _souls51 = 1700; _sigils51 = 15; _sinFilter737 = 0; _sinPage737 = 0; _s74Page = 0;
            var L = SinFiltered737(); _sinSel737 = L[1].id; _sinOwned737[3] = true; if (_sinRoll737[3] == null) _sinRoll737[3] = SinNewRoll737(HSins.All[3]); yield return AtWait(0.6f);
            string m2 = At1246Arg("-t1260mode") ?? "stills";
            if (m2 == "tal")
            {
                // t1263: native captures. tal_fK = the 8 loop frames (pinned mid-frame), tvf/NNNN = 100 fps native frames of N loops (1460 ms each)
                T1246Force = null; int[] st = { 0, 190, 350, 520, 730, 920, 1080, 1250 }; float t0 = 29.2f;   // 29.2 s = 20 loops
                for (int k = 0; k < 8; k++) { T1246Pin = t0 + (st[k] + 30) / 1000f; yield return AtWait(0.2f); yield return At1246Capture(); At1246SaveNative(At1246Native(_atLast), "tal_f" + k + ".png"); }
                int loops = int.Parse(At1246Arg("-t1263loops") ?? "3"); int N = loops * 146; var dir = Path.Combine(_atOut, "tvf"); Directory.CreateDirectory(dir);
                for (int i = 0; i < N; i++) { T1246Pin = t0 + i / 100f + 0.003f; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); var nat = At1246Native(cap); Destroy(cap); At1246SaveNative(nat, "tvf/" + i.ToString("0000") + ".png"); }
            }
            else if (m2 == "t1267")
            {
                // t1267: A/B/C talisman options, tiny native crops (both talismans: x 4..40 + 440..476, y 108..158) at 30 fps; boards/video are composed by tools/t1246/t1267_board.py
                T1246Force = null; int N = int.Parse(At1246Arg("-t1267n") ?? "480"); float t0 = float.Parse(At1246Arg("-t1267t0") ?? "40"); int was = SinFx84Renderer.Tal1267;
                string only = At1246Arg("-t1267only");
                for (int md = 1; md <= 3; md++)
                {
                    if (only != null && only != md.ToString()) continue;
                    SinFx84Renderer.Tal1267 = md; var dir = Path.Combine(_atOut, "m" + md); Directory.CreateDirectory(dir);
                    for (int i = 0; i < N; i++)
                    {
                        T1246Pin = t0 + i / 30f + 0.002f; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); var nat = At1246Native(cap); Destroy(cap);
                        var t = new Texture2D(72, 50, TextureFormat.RGBA32, false); var fl = new Color32[72 * 50];
                        for (int y = 0; y < 50; y++) for (int x = 0; x < 72; x++) { int sx = x < 36 ? 4 + x : 440 + (x - 36); int sy = 108 + y; fl[(49 - y) * 72 + x] = nat[sy * 480 + sx]; }
                        t.SetPixels32(fl); t.Apply(); File.WriteAllBytes(Path.Combine(dir, i.ToString("0000") + ".png"), t.EncodeToPNG()); Destroy(t);
                    }
                }
                SinFx84Renderer.Tal1267 = was;
            }
            else if (m2 == "t1270")
            {
                // t1270: t1265 (native-pixel flex) vs t1270 (screen-resolution flex) at the REAL output scale (-t1260w/-t1260h, integer scale s), 60 fps. Saves tiny crops (both talismans, art x 4..40 + 440..476, y 108..158, at s x) for both variants; -t1270full saves full JPG frames of the t1270 variant.
                T1246Force = null; int N = int.Parse(At1246Arg("-t1270n") ?? "480"); float t0 = float.Parse(At1246Arg("-t1270t0") ?? "40"); bool was = SinFx84Renderer.Tal1270; bool full = At1246Arg("-t1270full") != null; int S = T46ScaleUsed;
                ALog("t1270 scale " + S + " screen " + Screen.width + "x" + Screen.height);
                for (int v = 0; v < 2; v++)
                {
                    SinFx84Renderer.Tal1270 = v == 1; var dir = Path.Combine(_atOut, v == 0 ? "old" : "new"); Directory.CreateDirectory(dir); var fdir = Path.Combine(_atOut, "full"); if (full && v == 1) Directory.CreateDirectory(fdir);
                    for (int i = 0; i < N; i++)
                    {
                        T1246Pin = t0 + i / 60f + 0.0005f; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture();
                        int W = cap.width, H = cap.height, ox = Mathf.RoundToInt(W / 2f - S * 240f), oy = Mathf.RoundToInt(H / 2f - S * 135f); var px = cap.GetPixels32();
                        int cw = 36 * S, ch = 50 * S; var t = new Texture2D(cw * 2, ch, TextureFormat.RGBA32, false); var fl = new Color32[cw * 2 * ch];
                        for (int y = 0; y < ch; y++) for (int x = 0; x < cw * 2; x++) { int ax = x < cw ? 4 * S + x : 440 * S + (x - cw); int sx = ox + ax, sy = oy + 108 * S + y; fl[(ch - 1 - y) * cw * 2 + x] = px[(H - 1 - sy) * W + sx]; }
                        t.SetPixels32(fl); t.Apply(); File.WriteAllBytes(Path.Combine(dir, i.ToString("0000") + ".png"), t.EncodeToPNG()); Destroy(t);
                        if (full && v == 1) File.WriteAllBytes(Path.Combine(fdir, i.ToString("0000") + ".jpg"), cap.EncodeToJPG(96));
                        Destroy(cap);
                    }
                }
                SinFx84Renderer.Tal1270 = was;
            }
            else if (m2 == "t1272")
            {
                // t1272: embers t1271 (v0) vs t1272 (v1) at the REAL output scale, 60 fps: crops of the LEFT brazier column; -t1270full = full JPG frames of v1.
                T1246Force = null; int N = int.Parse(At1246Arg("-t1272n") ?? "480"); float t0 = float.Parse(At1246Arg("-t1272t0") ?? "40"); bool was = SinFx84Renderer.Emb1272; bool full = At1246Arg("-t1270full") != null; int S = T46ScaleUsed;
                ALog("t1272 scale " + S + " screen " + Screen.width + "x" + Screen.height);
                for (int v = 0; v < 2; v++)
                {
                    SinFx84Renderer.Emb1272 = v == 1; var dir = Path.Combine(_atOut, v == 0 ? "old" : "new"); Directory.CreateDirectory(dir); var fdir = Path.Combine(_atOut, "full"); if (full && v == 1) Directory.CreateDirectory(fdir);
                    for (int i = 0; i < N; i++)
                    {
                        T1246Pin = t0 + i / 60f + 0.0005f; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture();
                        int W = cap.width, H = cap.height, ox = Mathf.RoundToInt(W / 2f - S * 240f), oy = Mathf.RoundToInt(H / 2f - S * 135f); var px = cap.GetPixels32();
                        int cw = 44 * S, ch = 120 * S; var tx = new Texture2D(cw, ch, TextureFormat.RGBA32, false); var fl = new Color32[cw * ch];
                        for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++) { int sx = ox + x, sy = oy + 108 * S + y; fl[(ch - 1 - y) * cw + x] = px[(H - 1 - sy) * W + sx]; }
                        tx.SetPixels32(fl); tx.Apply(); File.WriteAllBytes(Path.Combine(dir, i.ToString("0000") + ".png"), tx.EncodeToPNG()); Destroy(tx);
                        if (full && v == 1) File.WriteAllBytes(Path.Combine(fdir, i.ToString("0000") + ".jpg"), cap.EncodeToJPG(96));
                        Destroy(cap);
                    }
                }
                SinFx84Renderer.Emb1272 = was;
            }
            else if (m2 == "t1271")
            {
                // t1271: embers old (t1262) vs new at the REAL output scale, 60 fps. Crops of the LEFT brazier column (art x 0..44, y 108..228) for both variants; -t1270full = full JPG frames of the new variant.
                T1246Force = null; int N = int.Parse(At1246Arg("-t1271n") ?? "480"); float t0 = float.Parse(At1246Arg("-t1271t0") ?? "40"); bool was = SinFx84Renderer.Emb1271; bool full = At1246Arg("-t1270full") != null; int S = T46ScaleUsed;
                ALog("t1271 scale " + S + " screen " + Screen.width + "x" + Screen.height);
                for (int v = 0; v < 2; v++)
                {
                    if (v == 0 && At1246Arg("-t1271skipold") != null) continue; SinFx84Renderer.Emb1271 = v == 1; var dir = Path.Combine(_atOut, v == 0 ? "old" : "new"); Directory.CreateDirectory(dir); var fdir = Path.Combine(_atOut, "full"); if (full && v == 1) Directory.CreateDirectory(fdir);
                    for (int i = 0; i < N; i++)
                    {
                        T1246Pin = t0 + i / 60f + 0.0005f; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture();
                        int W = cap.width, H = cap.height, ox = Mathf.RoundToInt(W / 2f - S * 240f), oy = Mathf.RoundToInt(H / 2f - S * 135f); var px = cap.GetPixels32();
                        int cw = 44 * S, ch = 120 * S; var t = new Texture2D(cw, ch, TextureFormat.RGBA32, false); var fl = new Color32[cw * ch];
                        for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++) { int sx = ox + x, sy = oy + 108 * S + y; fl[(ch - 1 - y) * cw + x] = px[(H - 1 - sy) * W + sx]; }
                        t.SetPixels32(fl); t.Apply(); File.WriteAllBytes(Path.Combine(dir, i.ToString("0000") + ".png"), t.EncodeToPNG()); Destroy(t);
                        if (full && v == 1) File.WriteAllBytes(Path.Combine(fdir, i.ToString("0000") + ".jpg"), cap.EncodeToJPG(96));
                        Destroy(cap);
                    }
                }
                SinFx84Renderer.Emb1271 = was;
            }
            else if (m2 == "t1273")
            {
                // t1273: brazier firelight on the main frame. before/after stills (pinned instant) at the REAL output scale + full-screen JPG frames of the "on" variant (60 fps).
                T1246Force = null; int N = int.Parse(At1246Arg("-t1273n") ?? "480"); float t0 = float.Parse(At1246Arg("-t1273t0") ?? "40"); bool was = SinFx84Renderer.FrameLight1273; int S = T46ScaleUsed;
                ALog("t1273 scale " + S + " screen " + Screen.width + "x" + Screen.height);
                foreach (float tp in new[] { 44.17f, 41.9f })
                    for (int v = 0; v < 2; v++)
                    {
                        SinFx84Renderer.FrameLight1273 = v == 1; T1246Pin = tp; yield return AtWait(0.4f); yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(_atOut, "still_" + (v == 1 ? "on" : "off") + "_t" + tp.ToString("0.00") + ".png"), cap.EncodeToPNG()); Destroy(cap);
                    }
                SinFx84Renderer.FrameLight1273 = true; var fdir = Path.Combine(_atOut, "full"); Directory.CreateDirectory(fdir);
                for (int i = 0; i < N; i++) { T1246Pin = t0 + i / 60f + 0.0005f; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(fdir, i.ToString("0000") + ".jpg"), cap.EncodeToJPG(96)); Destroy(cap); }
                SinFx84Renderer.FrameLight1273 = was;
            }
            else if (m2 == "braz")
            {
                // t1262: braziers at three flicker instants (native 480x270 saves), no hover; composed into before/after boards by tools/t1246/t1262_board.py
                T1246Force = null; int bi = 0;
                foreach (var tt in new[] { 41.00f, 41.37f, 41.83f, 42.20f }) { T1246Pin = tt; yield return AtWait(0.3f); yield return At1246Capture(); At1246SaveNative(At1246Native(_atLast), "braz_" + bi + ".png"); bi++; }
                if (At1246Arg("-t1262full") != null) { T1246Pin = 41.37f; yield return AtWait(0.3f); yield return At1246Capture(); File.WriteAllBytes(Path.Combine(_atOut, "braz_full.png"), _atLast.EncodeToPNG()); }
            }
            else if (m2 == "burn")
            {
                int N = int.Parse(At1246Arg("-t1262n") ?? "132"); float FPS = float.Parse(At1246Arg("-t1262fps") ?? "24"); var dir = Path.Combine(_atOut, "vf"); Directory.CreateDirectory(dir); float t0 = 40.0f; T1246Force = null;
                for (int i = 0; i < N; i++) { T1246Pin = t0 + i / FPS; yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(dir, i.ToString("0000") + ".png"), cap.EncodeToPNG()); Destroy(cap); }
            }
            else if (m2 == "t1261")
            {
                // t1261: full screen, empty-slot variants, and the detail panel for chosen Sins (native 480x270 saves; boards are composed by tools/t1246/t1261_board.py)
                T1246Pin = 7.3f; T1246Force = "cell5=hover"; yield return AtWait(0.4f); yield return At1246Capture(); File.WriteAllBytes(Path.Combine(_atOut, "t1261_full.png"), _atLast.EncodeToPNG());
                ALog("t1261 scale " + T46ScaleUsed + " screen " + Screen.width + "x" + Screen.height);
                T1246Force = null; foreach (int es in new[] { 0, 1 }) { T1261EmptyStyle = es; yield return AtWait(0.3f); yield return At1246Capture(); At1246SaveNative(At1246Native(_atLast), "t1261_empty_style" + es + ".png"); }
                T1261EmptyStyle = 0;
                int mortal = -1, dem = -1, hd = -1; for (int i = 0; i < HSins.Count; i++) { if (i % 9 == 4 || i == 3) continue; int t = (int)HSins.All[i].tier; if (t == 0 && mortal < 0 && i >= 5) mortal = i; else if (t == 1 && dem < 0) dem = i; else if (t == 2 && hd < 0) hd = i; }
                int fw = -1; for (int i = 0; i < HSins.Count; i++) if (HSins.All[i].name.StartsWith("Fire Wheel")) fw = i; if (i4(fw)) _sinOwned737[fw] = true;
                var cases = new[] { ("mortal", mortal, 0), ("demonic", dem, 0), ("hd", hd, 0), ("locked", 4, 0), ("equipped", 3, 0), ("longest", fw, 0), ("longest_p2", fw, 1) };
                foreach (var cs in cases)
                {
                    if (cs.Item2 < 0) { ALog("no sin for " + cs.Item1); continue; }
                    if (_sinOwned737[cs.Item2] && _sinRoll737[cs.Item2] == null) _sinRoll737[cs.Item2] = SinNewRoll737(HSins.All[cs.Item2]);
                    _sinSel737 = cs.Item2; _s74Page = 0; yield return AtWait(0.35f); _s74Page = cs.Item3; yield return AtWait(0.2f); yield return At1246Capture(); At1246SaveNative(At1246Native(_atLast), "t1261_detail_" + cs.Item1 + ".png");
                    ALog("t1261 case " + cs.Item1 + " sin " + cs.Item2 + " " + HSins.All[cs.Item2].name + " tier " + HSins.All[cs.Item2].tier);
                }
            }
            else if (m2 == "t1277")
            {
                // t1277: MAIN MENU reference pack for the art designer (capture only). stills/ = real build at -t1260w x -t1260h; clip/ = 60 fps jpg frames (Time.captureFramerate 60)
                SinClose87(); yield return AtWait(0.5f); mode = Mode.MainMenu; _panel69 = 0; _press69 = -1; _sel69 = _row69 = 0; _souls51 = 1700; _sigils51 = 15;
                _pactPath51 = (int)PactPath.Necromancer; _pactSel51 = 0xB5; string sd = Path.Combine(_atOut, "stills"); Directory.CreateDirectory(sd);
                ALog("t1277 screen " + Screen.width + "x" + Screen.height + " seal opens altar " + PT.mm69SealOpensAltar);
                for (int r = 0; r < 5; r++) { _sel69 = _row69 = r; yield return AtWait(0.45f); yield return At1277Shot(sd, "menu_row" + r + "_selected"); }
                _sel69 = _row69 = 0; yield return AtWait(0.4f); yield return At1277Shot(sd, "menu_idle");
                _sel69 = 5; yield return AtWait(0.9f); yield return At1277Shot(sd, "menu_seal_hover");
                Act69(5); yield return AtWait(0.12f); yield return At1277Shot(sd, "menu_seal_pressed");
                yield return AtWait(1.2f); yield return At1277Shot(sd, "menu_after_seal_press_mode_" + mode + "_scr" + _altarScr53);
                if (mode == Mode.Altar) { _altarScr53 = 0; _altarView51 = (int)PactPath.Necromancer; yield return AtWait(0.4f); yield return At1277Shot(sd, "pact_altar_overview"); _altarScr53 = 1; yield return AtWait(0.6f); yield return At1277Shot(sd, "pact_necromancer_nodes"); _altarScr53 = 1; _altarView51 = (int)PactPath.Pyromancer; yield return AtWait(0.6f); yield return At1277Shot(sd, "pact_pyromancer_nodes"); }
                mode = Mode.MainMenu; _panel69 = 1; _panelU69 = 1; _press69 = -1; yield return AtWait(0.5f); yield return At1277Shot(sd, "menu_pact_panel_placeholder"); _panel69 = 0; _panelU69 = 0; yield return AtWait(0.3f);
                if (At1246Arg("-t1277clip") != null)
                {
                    var cd = Path.Combine(_atOut, "clip"); Directory.CreateDirectory(cd); _sel69 = _row69 = 0; Time.captureFramerate = 60; int N = int.Parse(At1246Arg("-t1277n") ?? "480");
                    for (int i = 0; i < N; i++)
                    {
                        if (i == 90) _sel69 = _row69 = 1; else if (i == 140) _sel69 = _row69 = 2; else if (i == 190) _sel69 = _row69 = 3; else if (i == 240) _sel69 = _row69 = 4; else if (i == 290) _sel69 = _row69 = 0; else if (i == 340) _sel69 = 5; else if (i == 420) Act69(5);
                        yield return new WaitForEndOfFrame(); var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(cd, i.ToString("0000") + ".jpg"), cap.EncodeToJPG(95)); Destroy(cap);
                    }
                    Time.captureFramerate = 0; ALog("t1277 clip frames " + N + " mode after " + mode);
                }
                mode = Mode.MainMenu; _altarScr53 = 0; _panel69 = 0; _panelU69 = 0; _press69 = -1; _sel69 = _row69 = 0;
            }
            else if (m2 == "stills")
            {
                T1246Pin = 7.3f; T1246Force = "cell5=hover"; yield return AtWait(0.4f); yield return At1246Capture(); File.WriteAllBytes(Path.Combine(_atOut, "preview_full.png"), _atLast.EncodeToPNG());
                ALog("preview scale " + T46ScaleUsed + " screen " + Screen.width + "x" + Screen.height);
                var nat = At1246Native(_atLast); At1246SaveNative(nat, "preview_native.png");
                foreach (var tt in new[] { 1.0f, 1.25f, 1.5f, 1.75f }) { T1246Pin = tt + 20f; yield return AtWait(0.3f); yield return At1246Capture(); At1246SaveNative(At1246Native(_atLast), "preview_native_t" + (tt + 20f).ToString("0.00") + ".png"); }
            }
            else
            {
                const int N = 192; const float FPS = 24f; var dir = Path.Combine(_atOut, "vf"); Directory.CreateDirectory(dir); float t0 = 40.0f; var c = Sin1246Data.Cells;
                for (int i = 0; i < N; i++)
                {
                    T1246Pin = t0 + i / FPS;
                    T1246Force = i >= 48 && i < 84 ? "cell11=hover" : i >= 84 && i < 122 ? "cell14=hover" : i >= 150 && i < 184 ? "cell6=hover" : null;
                    if (i == 122) _s74Fake = new Vector2(c[14][0] + 12, c[14][1] + 12);
                    yield return new WaitForEndOfFrame();
                    var cap = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(dir, i.ToString("0000") + ".png"), cap.EncodeToPNG()); Destroy(cap);
                }
                ALog("video frames " + N + " at scale " + T46ScaleUsed + " screen " + Screen.width + "x" + Screen.height + " sel after click " + _sinSel737);
            }
            T1246Pin = -1f; T1246Force = null;
            System.Array.Copy(own0, _sinOwned737, own0.Length); System.Array.Copy(roll0, _sinRoll737, roll0.Length); System.Array.Copy(eq0, _sinEq737, 3); _sinSlots737 = sl0; _souls51 = so0; _sigils51 = si0; SinSave737(); PactSave51();
            SinLoad737(); PactLoad51(); T1246On = t46was; T1246HoverOff = false; AtEnd("preview1260");
        }
    }
}
#endif
