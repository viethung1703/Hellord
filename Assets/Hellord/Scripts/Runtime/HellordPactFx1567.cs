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
        static string PbArtRoot1567 { get { var T = HellordTuning.I; return T != null && !string.IsNullOrEmpty(T.pbArtRoot1567) ? T.pbArtRoot1567 : "Skills1567"; } }
        static int _pbSpecOv1567;
        /// <summary>key -> file remap + spec.json override (HFX format) for one spec row</summary>
        static void PbSpecOv1567(string key, ref string file, ref int fw, ref int fh, ref int n, ref int rows, ref float px, ref float py, ref object ms, ref float gain, ref uint tint)
        {
            var T = HellordTuning.I; if (T == null) return;
            if (T.pbArtFileMap1567 != null) foreach (var m in T.pbArtFileMap1567) { int i = m.IndexOf('='); if (i > 0 && m.Substring(0, i).Trim() == key) file = m.Substring(i + 1).Trim(); }
            if (!T.pbArtSpecJson1567) return; int sl = file.LastIndexOf('/'); string folder = sl > 0 ? file.Substring(0, sl) : "", stem = sl >= 0 ? file.Substring(sl + 1) : file;
            var ta = Resources.Load<TextAsset>("HellordArt/" + PbArtRoot1567 + "/" + (folder.Length > 0 ? folder + "/" : "") + "spec"); if (ta == null) return;
            var mb = Regex.Match(ta.text, "\"" + Regex.Escape(stem) + "\"\\s*:\\s*\\{([^{}]*)\\}"); if (!mb.Success) return; string b = mb.Groups[1].Value;
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
        class PbFis { public Vector2 a, dir; public float len, w, t, front, dmg; public int id, segs; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(), eruptHit = new HashSet<int>(); }
        readonly List<PbFis> _pbFis = new List<PbFis>(); int _pbFisId1567;
        public int SlamLineHits1567; public float SlamLineMaxAlong1567, SlamLineLen1567;
        bool CastSlamLine1567(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Vector2 dir = PbAim1567(); Vector2 hero0 = _heroPos; float area = Mathf.Sqrt(s.AreaMul); Vector2 at = _heroPos + dir * T.slamAheadU1567;
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
                var f = _pbFis[i]; f.t += dt; f.front = Mathf.Min(f.len, f.t * T.slamLineSpeedU1567); float hw = f.w * 0.5f;
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
                if (f.front <= 0f || f.t > T.crackLifeS1567) continue; Vector2 nrm = new Vector2(-f.dir.y, f.dir.x); int ord = FbGround + YSort(f.a.y) + 3; float a = Mathf.Clamp01((T.crackLifeS1567 - f.t) / 0.5f);
                if (PbEv1567("slam.seam", "").Length > 0) continue; // v2 seam strip owns it
                int n = Mathf.Max(2, Mathf.RoundToInt(f.front * HArt.PPU / 3f)); Vector2 prev = f.a;
                for (int q = 1; q <= n; q++) { float d = f.front * q / n; Vector2 p = f.a + f.dir * d + nrm * ((((q * 7 + f.id) % 5) - 2) * 0.06f); PbLine1567(prev, p, new Color(PbCrackCol.r, PbCrackCol.g, PbCrackCol.b, 0.9f * a), ord, 2); prev = p; }
            }
        }
    }
}
