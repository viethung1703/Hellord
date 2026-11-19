using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using PixelFXPack;

// t1567 (t1572/t1573 steering): effect plumbing for the pact LMB basics, so HFX's v2 strips drop in without code:
//  * art root folder (pbArtRoot1567), key -> file remap (pbArtFileMap1567), event -> key map (pbFxEvents1567) and per-folder spec.json overrides
//    (HFX's spec.json format: w / h / n / rows / piv / ms / glowset) read on load;
//  * shader hooks: additive glow material (FrostGlow tap shader or plain AddGlow1567), overbright hit-flash pass (HitFlash1567), optional heat-wave
//    distortion (HeatWave1567, URP opaque / sorting-layer texture or built-in GrabPass), light flash (existing t661 light mask + optional additive disc),
//    screen shake per skill. Everything is in HellordTuning (t1567 FX section).
//  * Earth-Splitting Slam v2: PoE 2 style FORWARD fissure line (slamForward1567) travelling ahead in the aim direction.
namespace Hellord
{
    public partial class HellordGame
    {
        // ---------------------------------------------------------------- data-driven hookup
        static Dictionary<string, string> _pbEvMap1567;
        /// <summary>art key for an effect event ("slam.segment" ...): pbFxEvents1567 "event=key" wins, "event=" / "event=none" = no art (code placeholder)</summary>
        static string PbEv1567(string ev, string def)
        {
            var T = HellordTuning.I; if (_pbEvMap1567 == null) { _pbEvMap1567 = new Dictionary<string, string>(); if (T != null && T.pbFxEvents1567 != null) foreach (var s in T.pbFxEvents1567) { int i = s.IndexOf('='); if (i > 0) _pbEvMap1567[s.Substring(0, i).Trim()] = s.Substring(i + 1).Trim(); } }
            if (_pbEvMap1567.TryGetValue(ev, out var k)) return k == "none" ? "" : k; return def;
        }
        static string PbArtPath1567(string file) => file.StartsWith("/") ? file.Substring(1) : PbFolderRoot1567(file) + "/" + file; // "/x/y" = under HellordArt directly
        /// <summary>t1578: per-folder art root (pbArtFolderRoot1567 "folder=root", e.g. spark_spray=Skills1567_v2), else pbArtRoot1567</summary>
        static string PbFolderRoot1567(string file)
        {
            var T = HellordTuning.I; int sl = file.IndexOf('/'); if (T == null || T.pbArtFolderRoot1567 == null || sl <= 0) return PbArtRoot1567; string fo = file.Substring(0, sl);
            foreach (var m in T.pbArtFolderRoot1567) { int i = m.IndexOf('='); if (i > 0 && m.Substring(0, i).Trim() == fo) return m.Substring(i + 1).Trim(); }
            return PbArtRoot1567;
        }
        /// <summary>t1578: the {...} body of "stem": { ... } with nested braces skipped; only the top level is returned (nested blocks blanked)</summary>
        static string PbSpecBlock1567(string txt, string stem)
        {
            var m = Regex.Match(txt, "\"" + Regex.Escape(stem) + "\"\\s*:\\s*\\{"); if (!m.Success) return null; int i0 = m.Index + m.Length, d = 1; var sb = new System.Text.StringBuilder(); bool str = false;
            for (int i = i0; i < txt.Length; i++)
            {
                char c = txt[i]; if (c == '"' && txt[i - 1] != '\\') str = !str;
                if (!str) { if (c == '{') d++; else if (c == '}') { d--; if (d == 0) return sb.ToString(); } }
                if (d == 1 && !(c == '}' && !str)) sb.Append(c);
            }
            return null;
        }
        static string PbArtRoot1567 { get { var T = HellordTuning.I; return T != null && !string.IsNullOrEmpty(T.pbArtRoot1567) ? T.pbArtRoot1567 : "Skills1567"; } }
        static int _pbSpecOv1567;
        /// <summary>key -> file remap + spec.json override (HFX format) for one spec row</summary>
        static void PbSpecOv1567(string key, ref string file, ref int fw, ref int fh, ref int n, ref int rows, ref float px, ref float py, ref object ms, ref float gain, ref uint tint)
        {
            var T = HellordTuning.I; if (T == null) return;
            if (T.pbArtFileMap1567 != null) foreach (var m in T.pbArtFileMap1567) { int i = m.IndexOf('='); if (i > 0 && m.Substring(0, i).Trim() == key) file = m.Substring(i + 1).Trim(); }
            if (!T.pbArtSpecJson1567) return; int sl = file.LastIndexOf('/'); string folder = sl > 0 ? file.Substring(0, sl) : "", stem = sl >= 0 ? file.Substring(sl + 1) : file;
            var ta = Resources.Load<TextAsset>("HellordArt/" + PbArtPath1567((folder.Length > 0 ? folder + "/" : "") + "spec")); if (ta == null) return;
            string b = PbSpecBlock1567(ta.text, stem); if (b == null) return; // t1578: brace-matched (v2 Leizu blocks nest "shader": {..})
            int I(string f, int d) { var x = Regex.Match(b, "\"" + f + "\"\\s*:\\s*(\\d+)"); return x.Success ? int.Parse(x.Groups[1].Value) : d; }
            fw = I("w", fw); fh = I("h", fh); n = I("n", n); rows = I("rows", rows);
            var pv = Regex.Match(b, "\"piv\"\\s*:\\s*\\[\\s*([\\d.]+)\\s*,\\s*([\\d.]+)"); if (pv.Success) { px = float.Parse(pv.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture); py = float.Parse(pv.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture); }
            var mm = Regex.Match(b, "\"ms\"\\s*:\\s*(\\[[^\\]]*\\]|[\\d.]+)");
            if (mm.Success) { string v = mm.Groups[1].Value; if (v.StartsWith("[")) { var parts = v.Trim('[', ']').Split(','); var arr = new List<float>(); foreach (var p in parts) if (float.TryParse(p.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f)) arr.Add(f); if (arr.Count > 0) ms = arr.ToArray(); } else if (float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f1)) ms = f1; }
            var gs = Regex.Match(b, "\"glowset\"\\s*:\\s*\\[\\s*([\\d.]+)\\s*,\\s*[\\d.]+\\s*,\\s*\"#?([0-9a-fA-F]{6})\"");
            if (gs.Success) { gain = float.Parse(gs.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture); tint = System.Convert.ToUInt32(gs.Groups[2].Value, 16); }
            else if (Regex.IsMatch(b, "\"glow_mask\"\\s*:\\s*false")) gain = 0f;
            _pbSpecOv1567++;
        }

        // ---------------------------------------------------------------- shader materials
        static Shader _pbAddSh1567, _pbFlashSh1567, _pbHeatSh1567; static Material _pbFlashDiscMat1567, _pbHitMat1567, _pbHeatMat1567;
        static Shader PbShader1567(ref Shader cache, string res, string find) { if (cache == null) { cache = Resources.Load<Shader>("HellordShaders/" + res); if (cache == null) cache = Shader.Find(find); } return cache != null && cache.isSupported ? cache : null; }
        /// <summary>plain additive glow material (AddGlow1567) for a glow mask; null when the shader is missing</summary>
        static Material PbAddMat1567(Color tint, float intensity, float bands)
        {
            var sh = PbShader1567(ref _pbAddSh1567, "HellordAddGlow1567", "Hellord/AddGlow1567"); if (sh == null) return null;
            var m = new Material(sh) { name = "pbAdd1567" }; m.SetColor("_Tint", tint); m.SetFloat("_Intensity", intensity); m.SetFloat("_Bands", bands); return m;
        }
        Material PbHitMat1567()
        {
            var T = HellordTuning.I; if (_pbHitMat1567 == null) { var sh = PbShader1567(ref _pbFlashSh1567, "HellordHitFlash1567", "Hellord/HitFlash1567"); if (sh == null) return null; _pbHitMat1567 = new Material(sh) { name = "pbHit1567" }; }
            _pbHitMat1567.SetColor("_FlashColor", T.pbHitFlashCol1567); _pbHitMat1567.SetFloat("_Overbright", T.pbHitFlashOver1567); _pbHitMat1567.SetFloat("_Flash", 1f); return _pbHitMat1567;
        }
        Material PbDiscMat1567()
        {
            var T = HellordTuning.I; if (_pbFlashDiscMat1567 == null) { _pbFlashDiscMat1567 = PbAddMat1567(Color.white, 1f, 0f); if (_pbFlashDiscMat1567 == null) return null; }
            _pbFlashDiscMat1567.SetFloat("_Intensity", T.pbFlashDiscInt1567); _pbFlashDiscMat1567.SetFloat("_Bands", T.pbFlashBands1567); return _pbFlashDiscMat1567;
        }
        Material PbHeatMat1567()
        {
            var T = HellordTuning.I; if (_pbHeatMat1567 == null) { var sh = PbShader1567(ref _pbHeatSh1567, "HellordHeatWave1567", "Hellord/HeatWave1567"); if (sh == null) return null; _pbHeatMat1567 = new Material(sh) { name = "pbHeat1567" }; }
            _pbHeatMat1567.SetFloat("_Strength", T.pbHeatPx1567); _pbHeatMat1567.SetFloat("_Speed", T.pbHeatSpeed1567); _pbHeatMat1567.SetFloat("_UseSortingLayerTex", T.pbHeatSortingTex1567 ? 1f : 0f); return _pbHeatMat1567;
        }
        static Sprite _pbSoft1567, _pbRingSoft1567;
        static Sprite PbSoft1567 => _pbSoft1567 != null ? _pbSoft1567 : (_pbSoft1567 = PbMake1567(32, 32, (x, y) => { float d = new Vector2(x - 15.5f, y - 15.5f).magnitude / 16f; float a = Mathf.Clamp01(1f - d); return new Color(1, 1, 1, a * a); }, 0.5f, 0.5f));
        static Sprite PbRingSoft1567 => _pbRingSoft1567 != null ? _pbRingSoft1567 : (_pbRingSoft1567 = PbMake1567(32, 32, (x, y) => { float d = new Vector2(x - 15.5f, y - 15.5f).magnitude / 16f; float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.7f) / 0.3f); return new Color(1, 1, 1, a); }, 0.5f, 0.5f));

        // ---------------------------------------------------------------- hooks: light flash, screen shake, hit flash, heat wave
        class PbTimed { public Vector2 p; public float t, dur, r, w; public Color c; public Enemy e; }
        readonly List<PbTimed> _pbDiscs1567 = new List<PbTimed>(), _pbHitFl1567 = new List<PbTimed>(), _pbHeat1567 = new List<PbTimed>();
        public int PbFlashes1567, PbShakes1567, PbHitFlashes1567, PbHeats1567;
        static T1 PbAt1567<T1>(T1[] a, int i, T1 d) => a != null && a.Length > 0 ? a[Mathf.Clamp(i, 0, a.Length - 1)] : d;
        /// <summary>light flash for skill ix at p (weight x mul): the t661 light mask flash + an optional additive disc (AddGlow1567)</summary>
        void PbFlash1567(int ix, Vector2 p, float mul = 1f)
        {
            var T = HellordTuning.I; if (!T.pbLightFlash1567) return; float w = PbAt1567(T.pbFlashW1567, ix, 0f) * mul; if (w <= 0f) return; var c = PbAt1567(T.pbFlashCol1567, ix, Color.white);
            PbFlashes1567++; Flash661(p, new Color(c.r, c.g, c.b, w));
            if (T.pbFlashDisc1567 && _pbDiscs1567.Count < 24) _pbDiscs1567.Add(new PbTimed { p = p, dur = Mathf.Max(0.02f, PbAt1567(T.pbFlashS1567, ix, 0.12f)), r = PbAt1567(T.pbFlashRU1567, ix, 1f) * Mathf.Sqrt(mul), c = c, w = Mathf.Clamp01(w) });
        }
        /// <summary>screen shake for skill ix (pbShakePx1567 art px over pbShakeS1567 s) x mul</summary>
        void PbShake1567(int ix, float mul = 1f) { var T = HellordTuning.I; if (!T.pbShake1567) return; float px = PbAt1567(T.pbShakePx1567, ix, 0f) * mul; if (px <= 0f) return; PbShakes1567++; ScreenShake.Add(px, PbAt1567(T.pbShakeS1567, ix, 0.08f)); }
        /// <summary>overbright hit pass over the demon's own sprite for pbHitFlashS1567</summary>
        void PbHitFlash1567(Enemy e, int ix)
        {
            var T = HellordTuning.I; if (!T.pbHitFlash1567 || e == null || e.sr == null || !PbAt1567(T.pbHitFlashOn1567, ix, true)) return;
            foreach (var h in _pbHitFl1567) if (h.e == e) { h.t = 0f; return; } if (_pbHitFl1567.Count < 48) { _pbHitFl1567.Add(new PbTimed { e = e, dur = Mathf.Max(0.02f, T.pbHitFlashS1567) }); PbHitFlashes1567++; }
        }
        /// <summary>optional heat-wave ring (HeatWave1567) at p growing to rU over dur</summary>
        void PbHeat1567(Vector2 p, float rU, float dur) { var T = HellordTuning.I; if (!T.pbHeatWave1567 || _pbHeat1567.Count >= 8 || PbHeatMat1567() == null) return; _pbHeat1567.Add(new PbTimed { p = p, r = rU, dur = Mathf.Max(0.05f, dur) }); PbHeats1567++; }
        void UpdateDrawPbHooks1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbDiscs1567.Count - 1; i >= 0; i--)
            {
                var d = _pbDiscs1567[i]; d.t += dt; if (d.t >= d.dur) { _pbDiscs1567.RemoveAt(i); continue; } var m = PbDiscMat1567(); if (m == null) continue;
                float k = d.t / d.dur, a = (1f - k) * (1f - k) * d.w, sc = d.r * 2f * HArt.PPU / 32f; PbPut1567(PbSoft1567, d.p + Vector2.up * (T.pbFlashLiftPx1567 / HArt.PPU), 0f, sc, sc * T.pbFlashYSc1567, new Color(d.c.r, d.c.g, d.c.b, a), YSort(d.p.y) + 70, false, m);
            }
            var hm = _pbHitFl1567.Count > 0 ? PbHitMat1567() : null;
            for (int i = _pbHitFl1567.Count - 1; i >= 0; i--)
            {
                var h = _pbHitFl1567[i]; h.t += dt; var e = h.e; if (h.t >= h.dur || e == null || e.sr == null || !e.alive || !e.sr.enabled) { _pbHitFl1567.RemoveAt(i); continue; } if (hm == null) continue;
                float a = 1f - h.t / h.dur; var tr = e.sr.transform; PbPut1567(e.sr.sprite, tr.position, 0f, tr.lossyScale.x, tr.lossyScale.y, new Color(1, 1, 1, a), e.sr.sortingOrder + 1, e.sr.flipX, hm);
                var last = _pbSr1567[_pbN1567 - 1]; if (last != null) last.sortingLayerID = e.sr.sortingLayerID;
            }
            for (int i = _pbHeat1567.Count - 1; i >= 0; i--)
            {
                var h = _pbHeat1567[i]; h.t += dt; if (h.t >= h.dur) { _pbHeat1567.RemoveAt(i); continue; } var m = PbHeatMat1567(); if (m == null) continue;
                float k = h.t / h.dur, sc = Mathf.Lerp(0.3f, 1f, 1f - (1f - k) * (1f - k)) * h.r * 2f * HArt.PPU / 32f; PbPut1567(PbRingSoft1567, h.p, 0f, sc, sc * 0.6f, new Color(1, 1, 1, 1f - k), YSort(h.p.y) + 80, false, m);
            }
        }
        /// <summary>art scale so a strip drawn for artPx reaches the hit radius hitU (pbArtMatchHit1567; 1 when off)</summary>
        static float PbMatch1567(float hitU, float artPx) { var T = HellordTuning.I; if (!T.pbArtMatchHit1567 || artPx <= 0f) return 1f; float s = hitU * HArt.PPU / artPx; if (T.pbArtScaleSnap1567 > 0f) s = Mathf.Max(T.pbArtScaleSnap1567, Mathf.Round(s / T.pbArtScaleSnap1567) * T.pbArtScaleSnap1567); return s; }

        // ---------------------------------------------------------------- Earth-Splitting Slam v2: forward fissure line (PoE 2 Earthshatter / Sunder ref)
        // The axe hits slamAheadU ahead; a fissure front runs from slamLineStartU to slamLineLenU along the aim at slamLineSpeedU, slamLineWU wide,
        // hitting each demon once as it passes (knock-back sideways + forward). Every slamSegU it leaves a crack segment (slows, erupts after
        // eruptDelayS; each demon is hit by the eruption once per fissure). At most crackCap1567 fissures: a new slam removes the oldest.
        class PbFis { public bool v2, stopDone; public int row; public float frontPx, sq; public Vector2 a, dir; public float len, w, t, front, dmg; public int id, segs; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(), eruptHit = new HashSet<int>(); }
        readonly List<PbFis> _pbFis = new List<PbFis>(); int _pbFisId1567;
        public int SlamLineHits1567; public float SlamLineMaxAlong1567, SlamLineLen1567;
        bool CastSlamLine1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; if (SlamV2On1567) return CastSlamV2_1567(s, dmg); // t1576 HFX v2 (approved)
            Vector2 dir = PbAim1567(); Vector2 hero0 = _heroPos; float area = Mathf.Sqrt(s.AreaMul); Vector2 at = _heroPos + dir * T.slamAheadU1567;
            Vector2 a = _heroPos + dir * T.slamLineStartU1567; float len = T.slamLineLenU1567 * (T.slamLineAreaLen1567 ? area : 1f), w = T.slamLineWU1567 * area;
            for (float d = 0f; d <= len; d += 0.25f) { Vector2 p = a + dir * d, q = p; ClampToRoom(ref q, 4); if ((q - p).sqrMagnitude > 0.01f) { len = Mathf.Max(0.5f, d); break; } } // stops at walls
            while (_pbFis.Count >= Mathf.Max(1, T.crackCap1567)) { var old = _pbFis[0]; _pbFis.RemoveAt(0); _pbCracks.RemoveAll(k => k.fis == old); }
            var f = new PbFis { a = a, dir = dir, len = len, w = w, dmg = dmg, s = s, id = _pbFisId1567++ }; _pbFis.Add(f);
            SlamImpacts1567++; SlamCracks1567 = _pbFis.Count; SlamCracksPeak1567 = Mathf.Max(SlamCracksPeak1567, _pbFis.Count); SlamLastAt1567 = at; SlamLastHeroMove1567 = (_heroPos - hero0).magnitude; SlamLineLen1567 = len;
            string ik = PbEv1567("slam.impact", "xs_impact"); if (ik.Length > 0 && PbFxOnce1567(ik, at, YSort(at.y) + 4, 0, 0f, false, PbMatch1567(w * 0.5f, T.slamImpactArtPx1567))) SlamDust1567(at, T.slamDustPx1567, 1);
            else PbPop1567(at + Vector2.up * 0.2f, T.slamRingS1567, PbDust, 10, w * 0.4f);
            PbShake1567(4); PbFlash1567(4, at); PbHeat1567(at, w, 0.35f);
            UpdateFis1567(0f); PbCasts1567[4]++; HellordAudio.Play("oxhead_stomp_strike", PbPitch1567(4)); return true;
        }
        void UpdateFis1567(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pbFis.Count - 1; i >= 0; i--)
            {
                var f = _pbFis[i]; f.t += dt; if (f.v2) { UpdateSlamV2_1567(f, i); continue; } f.front = Mathf.Min(f.len, f.t * T.slamLineSpeedU1567); float hw = f.w * 0.5f;
                if (f.front > 0f || f.t == 0f)
                {
                    Vector2 mid = f.a + f.dir * (f.front * 0.5f); Query(mid, f.front * 0.5f + hw + 1f, _pbQ); Vector2 nrm = new Vector2(-f.dir.y, f.dir.x);
                    foreach (int ei in _pbQ)
                    {
                        var e = _en[ei]; if (!PbOk(e) || f.hit.Contains(ei)) continue; Vector2 v = e.pos - f.a; float al = Vector2.Dot(v, f.dir), pp = Vector2.Dot(v, nrm);
                        if (al < -T.slamLineBackU1567 || al > f.front + e.radius * 0.5f || Mathf.Abs(pp) > hw + e.radius * 0.5f) continue; f.hit.Add(ei);
                        PbHit1567(e, f.dmg, f.s, nrm * (Mathf.Sign(pp == 0f ? 1f : pp) * T.slamLineKnockSide1567) + f.dir * T.slamLineKnockFwd1567); SlamLineHits1567++; SlamLineMaxAlong1567 = Mathf.Max(SlamLineMaxAlong1567, al);
                    }
                }
                float seg = Mathf.Max(0.25f, T.slamSegU1567);
                while (f.segs * seg <= f.front + 0.001f && f.segs * seg <= f.len)
                {
                    float d0 = f.segs * seg; Vector2 c = f.a + f.dir * Mathf.Min(f.len, d0 + seg * 0.5f);
                    _pbCracks.Add(new PbCrack { p = c, life = T.crackLifeS1567, dmg = f.dmg * T.eruptMul1567, s = f.s, seed = f.id * 31 + f.segs, fis = f, r = Mathf.Max(hw, seg * 0.6f), rot = Mathf.Atan2(f.dir.y, f.dir.x) * Mathf.Rad2Deg });
                    string sk = PbEv1567("slam.segment", "xs_impact"); if (sk.Length > 0) PbFxOnce1567(sk, c, YSort(c.y) + 4, 0, 0f, (f.segs & 1) == 1, PbMatch1567(hw, T.slamImpactArtPx1567)); else PbPop1567(c + Vector2.up * 0.2f, T.slamRingS1567, PbDust, 8, hw * 0.8f);
                    if ((f.segs & 1) == 0) SlamDust1567(c, T.slamDustPx1567 * hw * HArt.PPU / 24f, 1);
                    f.segs++;
                }
                if (f.front >= f.len && !_pbCracks.Exists(k => k.fis == f)) _pbFis.RemoveAt(i);
            }
            if (T.slamForward1567) SlamCracks1567 = _pbFis.Count;
        }
        void ClearFis1567() { _pbFis.Clear(); _pbDiscs1567.Clear(); _pbHitFl1567.Clear(); _pbHeat1567.Clear(); }
        /// <summary>placeholder draw of the fissure line while the front travels (v1 strips have no line art): dark seam + rubble dots</summary>
        void DrawFis1567()
        {
            var T = HellordTuning.I;
            foreach (var f in _pbFis)
            {
                if (f.v2) { DrawSlamV2_1567(f); continue; }
                if (f.front <= 0f || f.t > T.crackLifeS1567) continue; Vector2 nrm = new Vector2(-f.dir.y, f.dir.x); int ord = FbGround + YSort(f.a.y) + 3; float a = Mathf.Clamp01((T.crackLifeS1567 - f.t) / 0.5f);
                if (PbEv1567("slam.seam", "").Length > 0) continue; // v2 seam strip owns it
                int n = Mathf.Max(2, Mathf.RoundToInt(f.front * HArt.PPU / 3f)); Vector2 prev = f.a;
                for (int q = 1; q <= n; q++) { float d = f.front * q / n; Vector2 p = f.a + f.dir * d + nrm * ((((q * 7 + f.id) % 5) - 2) * 0.06f); PbLine1567(prev, p, new Color(PbCrackCol.r, PbCrackCol.g, PbCrackCol.b, 0.9f * a), ord, 2); prev = p; }
            }
        }
    }
}

namespace Hellord
{
    // t1576 Earth-Splitting Slam v2 (HFX skills_t1567_v2/earth_splitting_slam, VH approved except the dust layer): one baked dir16 fissure per slam.
    // All strips spawn at P = feet + aim x 9 px (axe bite). Hitbox = the tearing crack: a band along the row direction, front = 8 px + 10 px/f
    // (to 112 ground px, ~0.35 s), width 28 ground px, both through the art's ground squash (screen y = ground y x 0.7) so the band lies on the drawn
    // crack; plus an r = 10 px circle at P on impact. Hit-stop 100 ms on frame 1. Cracks slow until the fade; the aftershock is optional (off).
    public partial class HellordGame
    {
        Material _pbShockMat1567; public int SlamHitStops1567, SlamV2Casts1567, SlamV2ImpactHits1567; public bool SlamV2Last1567; public float SlamV2L1567, SlamV2W1567;
        bool SlamV2On1567 { get { var T = HellordTuning.I; return T.slamV2Art1567 && PbA1567(PbEv1567("slam.v2.decal", "xs2_decal")) != null && PbA1567(PbEv1567("slam.v2b.rocks", "xs2b_rocks")) != null; } } // t1591: rocks_v2b only (v2 rocks deleted)
        static Vector2 PbRowDir1567(int row) { float a = row * 22.5f * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(a), -Mathf.Sin(a)); } // row 0 = east, clockwise on screen
        /// <summary>screen length of 1 ground px along screen direction d (art ground squash: screen y = ground y x sq)</summary>
        static float PbSq1567(Vector2 d, float sq) { if (sq <= 0f || sq >= 1f) return 1f; return 1f / Mathf.Sqrt(d.x * d.x + (d.y / sq) * (d.y / sq)); }
        bool CastSlamV2_1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 aim = PbAim1567(); Vector2 hero0 = _heroPos; float area = Mathf.Sqrt(s.AreaMul); int row = PbRow16(aim); Vector2 rd = PbRowDir1567(row), nrm = new Vector2(-rd.y, rd.x);
            float sq = T.slamV2Squash1567 ? T.slamV2GroundSquash1567 : 1f, kL = PbSq1567(rd, sq), kW = PbSq1567(nrm, sq);
            Vector2 P = _heroPos + aim * (T.slamV2BitePx1567 / HArt.PPU);
            float L = T.slamV2LenPx1567 * kL / HArt.PPU * (T.slamLineAreaLen1567 ? area : 1f), W = T.slamV2WPx1567 * kW / HArt.PPU * area;
            float len = L; for (float d = 0f; d <= L; d += 0.25f) { Vector2 p = P + rd * d, q = p; ClampToRoom(ref q, 4); if ((q - p).sqrMagnitude > 0.01f) { len = Mathf.Max(0.5f, d); break; } } // hitbox stops at walls (the baked art does not)
            while (_pbFis.Count >= Mathf.Max(1, T.crackCap1567)) { var old = _pbFis[0]; _pbFis.RemoveAt(0); _pbCracks.RemoveAll(k => k.fis == old); }
            var f = new PbFis { v2 = true, row = row, sq = kL, a = P, dir = rd, len = len, w = W, dmg = dmg, s = s, id = _pbFisId1567++ }; _pbFis.Add(f);
            SlamImpacts1567++; SlamV2Casts1567++; SlamV2Last1567 = true; SlamV2L1567 = len; SlamV2W1567 = W; SlamCracks1567 = _pbFis.Count; SlamCracksPeak1567 = Mathf.Max(SlamCracksPeak1567, _pbFis.Count);
            SlamLastAt1567 = P; SlamLastHeroMove1567 = (_heroPos - hero0).magnitude; SlamLineLen1567 = len;
            // impact circle (r = 10 px at P)
            float ir = T.slamV2ImpactRPx1567 / HArt.PPU * area; Query(P, ir + 1f, _pbQ);
            foreach (int ei in _pbQ) { var e = _en[ei]; if (!PbOk(e) || f.hit.Contains(ei)) continue; Vector2 v = e.pos - P; float m = v.magnitude; if (m > ir + e.radius * 0.5f) continue; f.hit.Add(ei); PbHit1567(e, dmg, s, (m > 0.05f ? v / m : rd) * T.slamKnock1567); SlamV2ImpactHits1567++; }
            if (T.pbShake1567 && T.slamV2ShakePx1567 > 0f) { PbShakes1567++; ScreenShake.Add(T.slamV2ShakePx1567, T.slamV2ShakeS1567); }
            PbFlash1567(4, P); UpdateSlamV2_1567(f, _pbFis.Count - 1); PbCasts1567[4]++; HellordAudio.Play("oxhead_stomp_strike", PbPitch1567(4)); return true;
        }
        void UpdateSlamV2_1567(PbFis f, int idx)
        {
            var T = HellordTuning.I; float hw = f.w * 0.5f;
            if (!f.stopDone && f.t > 0f) { f.stopDone = true; if (T.slamV2HitStopS1567 > 0f) { HitStop.Play(T.slamV2HitStopS1567); SlamHitStops1567++; } } // hold frame 1 (hero + enemies + FX) for 3 frames
            f.frontPx = Mathf.Min(T.slamV2LenPx1567, T.slamV2FrontStartPx1567 + T.slamV2FrontPxF1567 * 30f * f.t); f.front = Mathf.Min(f.len, f.frontPx * f.sq / HArt.PPU);
            Vector2 nrm = new Vector2(-f.dir.y, f.dir.x); Query(f.a + f.dir * (f.front * 0.5f), f.front * 0.5f + hw + 1f, _pbQ);
            foreach (int ei in _pbQ)
            {
                var e = _en[ei]; if (!PbOk(e) || f.hit.Contains(ei)) continue; Vector2 v = e.pos - f.a; float al = Vector2.Dot(v, f.dir), pp = Vector2.Dot(v, nrm);
                if (al < -T.slamLineBackU1567 || al > f.front + e.radius * 0.5f || Mathf.Abs(pp) > hw + e.radius * 0.5f) continue; f.hit.Add(ei);
                PbHit1567(e, f.dmg, f.s, nrm * (Mathf.Sign(pp == 0f ? 1f : pp) * T.slamLineKnockSide1567) + f.dir * T.slamLineKnockFwd1567); SlamLineHits1567++; SlamLineMaxAlong1567 = Mathf.Max(SlamLineMaxAlong1567, al);
            }
            float seg = Mathf.Max(0.25f, T.slamSegU1567);
            while (f.segs * seg <= f.front + 0.001f && f.segs * seg <= f.len)
            {   // invisible slow segments under the baked decal (aftershock = optional delayed damage)
                Vector2 c = f.a + f.dir * Mathf.Min(f.len, f.segs * seg + seg * 0.5f);
                _pbCracks.Add(new PbCrack { p = c, life = T.slamV2LifeS1567 - f.t, dmg = f.dmg * T.eruptMul1567, s = f.s, seed = f.id * 31 + f.segs, fis = f, r = Mathf.Max(hw, seg * 0.6f), erupted = !T.slamV2Aftershock1567, eruptAt = Mathf.Max(0.01f, T.slamV2AftershockS1567 - f.t) });
                f.segs++;
            }
            if (f.t >= T.slamV2LifeS1567 && !_pbCracks.Exists(k => k.fis == f)) _pbFis.RemoveAt(idx);
        }
        // banded rocks (Y-sort split): [row, frame, band] sub-sprites + the band's bottom (px below the pivot)
        class PbBands1567 { public Sprite[,,] B, G; public float[,] bot; public int bands; } readonly Dictionary<PbArtS, PbBands1567> _xsBands1567 = new Dictionary<PbArtS, PbBands1567>(); // t1578: per strip (v2 rocks, v2b rocks, v2b debris)
        PbBands1567 SlamV2Bands1567(PbArtS a, int bands)
        {
            if (_xsBands1567.TryGetValue(a, out var c0) && c0.bands == bands) return c0; var bc = new PbBands1567 { bands = bands, B = new Sprite[a.rows, a.n, bands], G = a.g != null ? new Sprite[a.rows, a.n, bands] : null, bot = new float[a.rows, bands] }; _xsBands1567[a] = bc;
            for (int r = 0; r < a.rows; r++) for (int c = 0; c < a.n; c++) for (int b = 0; b < bands; b++)
            {
                var sp = a.f[r, c]; Rect R = sp.rect; float h = R.height, y0 = Mathf.Floor(h * b / bands), y1 = Mathf.Floor(h * (b + 1) / bands); // from the top
                var sub = new Rect(R.x, R.y + h - y1, R.width, y1 - y0); var pv = new Vector2(sp.pivot.x / R.width, (sp.pivot.y - (h - y1)) / (y1 - y0));
                bc.B[r, c, b] = Sprite.Create(sp.texture, sub, pv, HArt.PPU, 0, SpriteMeshType.FullRect); bc.bot[r, b] = sp.pivot.y - (h - y1);
                if (bc.G != null) { var g = a.g[r, c]; bc.G[r, c, b] = Sprite.Create(g.texture, new Rect(g.rect.x, g.rect.y + h - y1, g.rect.width, y1 - y0), pv, HArt.PPU, 0, SpriteMeshType.FullRect); }
            }
            return bc;
        }
        void DrawSlamV2_1567(PbFis f)
        {
            var T = HellordTuning.I; float t = f.t; int F = Mathf.FloorToInt(t * 30f + 0.0001f), row = f.row; Vector2 P = f.a; int top = YSort(P.y - T.slamV2TopU1567);
            bool after = T.slamV2Aftershock1567; float aS = T.slamV2AftershockS1567; int aF = Mathf.RoundToInt(aS * 30f); var st = T.slamV2StageF1567;
            // ground decal: decal_dir16 f0-23 (crack tears + cools), then code-stepped stages; aftershock: tell 1/2, replay decal f12-23 x0.8
            string dk = PbEv1567("slam.v2.decal", "xs2_decal"), sk = PbEv1567("slam.v2.stages", "xs2_stages"); int gO = FbGround + YSort(P.y) - 100;
            if (F < 24) PbArtPut1567(dk, row, t, false, P, gO);
            else if (after && F >= aF && F < aF + 12) { _pbGlowMul1567 = T.slamV2AfterGlow1567; PbArtPutF1567(dk, row, 12 + (F - aF), P, gO); _pbGlowMul1567 = 1f; }
            else
            {
                int stg = after && F >= aF - 9 && F < aF ? 1 + ((F - (aF - 9)) / Mathf.Max(1, T.crackBlinkF1567)) % 2 : F < PbAt1567(st, 0, 54) ? 0 : F < PbAt1567(st, 1, 114) ? 3 : F < PbAt1567(st, 2, 129) ? 4 : F < PbAt1567(st, 3, 144) ? 5 : -1;
                if (stg >= 0) PbArtPutF1567(sk, row, stg, P, gO);
            }
            // one-shot layers at impact (and again at the optional aftershock, glow x0.8)
            for (int pass = 0; pass < (after ? 2 : 1); pass++)
            {
                float lt = pass == 0 ? t : t - aS; if (lt < 0f) continue; _pbGlowMul1567 = pass == 0 ? 1f : T.slamV2AfterGlow1567;
                {   // t1578 rocks_v2b (Y-sorted, glow) -> v2c debris just after (rocks key + 2, glow) -> v2c debris shadow on the ground (after the decal, under actors) -> v2c dust smoke +10
                    // t1591: the rejected v2 rocks / dust and v2b debris / dust strips are deleted; always the v2c dirt set
                    SlamV2Rocks1567(f, lt, row, P, PbEv1567("slam.v2b.rocks", "xs2b_rocks"), 0);
                    SlamV2Rocks1567(f, lt, row, P, PbEv1567("slam.v2c.debris", "xs2c_debris"), 2);
                    if (T.slamV2bShadow1567) PbArtPut1567(PbEv1567("slam.v2c.shadow", "xs2c_dshadow"), row, lt, false, P, gO + 5);
                    if (T.slamV2bDust1567) PbArtPut1567(PbEv1567("slam.v2c.dust", "xs2c_dust"), row, lt, false, P, top + 10);
                }
                PbArtPut1567(PbEv1567("slam.v2.sparks", "xs2_sparks"), row, lt, false, P, top + 20);
                if (pass == 0) PbArtPut1567(PbEv1567("slam.v2.flash", "xs2_flash"), 0, lt, false, P, top + 30);
                if (T.pbHeatWave1567 && T.slamV2Shock1567) SlamV2Shock1567(row, lt, P, top + 40);
                _pbGlowMul1567 = 1f;
            }
        }
        void SlamV2Rocks1567(PbFis f, float lt, int row, Vector2 P, string rk, int dO)
        {
            var T = HellordTuning.I; var a = string.IsNullOrEmpty(rk) ? null : PbA1567(rk); if (a == null) return; int fi = PbAF1567(a, lt, false); if (fi < 0) return; row = Mathf.Clamp(row, 0, a.rows - 1);
            bool vert = (row >= 2 && row <= 6) || (row >= 10 && row <= 14); int bands = vert ? Mathf.Max(1, T.slamV2RockBands1567) : 1;
            if (bands <= 1)
            {   // spec sort key: P.y + 40 px (toward the camera) for rows 2-6, else P.y
                int o = YSort(P.y - (row >= 2 && row <= 6 ? T.slamV2RockKeyPx1567 / HArt.PPU : 0f)) + dO; PbPut1567(a.f[row, fi], P, 0f, 1f, 1f, Color.white, o);
                if (a.g != null && a.mat != null) PbPut1567(a.g[row, fi], P, 0f, 1f, 1f, new Color(1, 1, 1, _pbGlowMul1567), o + 1, false, a.mat); return;
            }
            var bc = SlamV2Bands1567(a, bands);
            for (int b = 0; b < bands; b++)
            {   // each horizontal band Y-sorts at its own bottom edge, so a demon standing inside the fissure goes in front of the far slabs and behind the near ones
                int o = YSort(P.y - bc.bot[row, b] / HArt.PPU) + dO; PbPut1567(bc.B[row, fi, b], P, 0f, 1f, 1f, Color.white, o);
                if (bc.G != null && a.mat != null) PbPut1567(bc.G[row, fi, b], P, 0f, 1f, 1f, new Color(1, 1, 1, _pbGlowMul1567), o + 1, false, a.mat);
            }
        }
        void SlamV2Shock1567(int row, float lt, Vector2 P, int order)
        {   // optional distortion: the shock mask through HeatWave1567 in gradient mode (uv offset = normalize(grad(mask)) x mask x strength)
            var a = PbA1567(PbEv1567("slam.v2.shock", "xs2_shock")); var hm = PbHeatMat1567(); if (a == null || hm == null) return; if (_pbShockMat1567 == null) _pbShockMat1567 = new Material(hm) { name = "pbShock1567" }; var m = _pbShockMat1567; m.SetFloat("_UseSortingLayerTex", HellordTuning.I.pbHeatSortingTex1567 ? 1f : 0f); int fi = PbAF1567(a, lt, false); if (fi < 0) return;
            m.SetFloat("_Mode", 1f); m.SetFloat("_Strength", HellordTuning.I.slamV2ShockPx1567); PbPut1567(a.f[Mathf.Clamp(row, 0, a.rows - 1), fi], P, 0f, 1f, 1f, Color.white, order, false, m); PbHeats1567++;
        }
    }
}
