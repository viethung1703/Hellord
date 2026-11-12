#if HELLORD_AUTOTEST
// t991u/t992u: wall-clock FPS probe (idle / L1 Fireball / combat / A-B toggles). Real Time.realtimeSinceStartup, NOT captureFramerate.
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PixelFXPack;
using Unity.Profiling;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        struct FpsSamp991
        {
            public string name; public int frames; public float sec, avgFps, minFps, p1Fps, worstMs;
            public long gcAvg, dcAvg, bcAvg; public int srMax, hellfxMax, shockMax, sfxMax, burnMax, lightSrcMax;
            public float lightMsAvg; public string note;
        }
        readonly List<FpsSamp991> _fps991 = new List<FpsSamp991>();

        IEnumerator AtFps991()
        {
            yield return AtBegin("fps991");
            Screen.SetResolution(1280, 720, false); yield return AtFrames(10);
            mode = Mode.Play; autoAim = true; showDebug = false; _mouseT = -99; _atGod = true; maxHp = hp = 100000;
            ProjectileFX.HitStopScale = 0f; // measure render cost, not juice freezes
            var T = HellordTuning.I;
            bool light0 = T.v2Light586, projL0 = T.v2ProjLight660, shaderFb0 = T.fxShaderFireball928, shock0 = T.fxShock934, crt0 = crtEnabled, atmos0 = atmosOn, stamp0 = T.v2LightStamp991;
            int dyn0 = T.v2DynLightCap991;
            float glow0 = crtGlow;
            ALog($"fps991 gfx {SystemInfo.graphicsDeviceName} / {SystemInfo.graphicsDeviceVersion} lowRes={lowResRender} crt={crtEnabled} light={T.v2Light586} projLight={T.v2ProjLight660} fbShader={T.fxShaderFireball928}");

            // 1) idle empty room, no skills
            yield return AtFpsRun991("idle_noskill", null, 0, 25f, null);

            // 2) idle with static enemies (no skills) — AI/sprites baseline
            yield return AtFpsRun991("idle_imps12", null, 12, 20f, null);

            // 3) L1 Fireball only (VH case) — stamp+dynCap ON (post-fix defaults)
            yield return AtFpsRun991("fb_l1", new[] { (SkillId.Fireball, 1) }, 10, 25f, "stamp+dynCap12");

            // 3b) L1 Fireball, stamp OFF (legacy O(W*H*S) — pre-fix baseline)
            T.v2LightStamp991 = false; T.v2DynLightCap991 = 0; Light586MsReset();
            yield return AtFpsRun991("fb_l1_legacy_mask", new[] { (SkillId.Fireball, 1) }, 10, 20f, "stamp=false dynCap=0");
            T.v2LightStamp991 = stamp0; T.v2DynLightCap991 = dyn0;

            // 4) L1 Fireball, light mask OFF
            T.v2Light586 = false; Light586MsReset();
            yield return AtFpsRun991("fb_l1_nolight", new[] { (SkillId.Fireball, 1) }, 10, 20f, "v2Light586=false");
            T.v2Light586 = light0;

            // 5) L1 Fireball, projectile/flash lights OFF (hero+torch mask stays)
            T.v2ProjLight660 = false; Light586MsReset();
            yield return AtFpsRun991("fb_l1_noprojlight", new[] { (SkillId.Fireball, 1) }, 10, 20f, "v2ProjLight660=false");
            T.v2ProjLight660 = projL0;

            // 6) L1 Fireball, CRT OFF
            crtEnabled = false;
            yield return AtFpsRun991("fb_l1_nocrt", new[] { (SkillId.Fireball, 1) }, 10, 20f, "crt=false");
            crtEnabled = crt0;

            // 7) L1 Fireball, shader blast OFF (pack Impact/Nova path)
            T.fxShaderFireball928 = false;
            yield return AtFpsRun991("fb_l1_noshaderblast", new[] { (SkillId.Fireball, 1) }, 10, 20f, "fxShaderFireball928=false");
            T.fxShaderFireball928 = shaderFb0;

            // 8) early combat: FB+Frost L1, 16 imps
            yield return AtFpsRun991("early_fb_frost_l1", new[] { (SkillId.Fireball, 1), (SkillId.FrostCleave, 1) }, 16, 20f, null);

            // 9) dense combat: FB3+Frost3+Storm3, 40 enemies
            yield return AtFpsRun991("dense_skills3_en40", new[] { (SkillId.Fireball, 3), (SkillId.FrostCleave, 3), (SkillId.StormRing, 3) }, 40, 20f, null);

            // 10) dense + light OFF
            T.v2Light586 = false; Light586MsReset();
            yield return AtFpsRun991("dense_nolight", new[] { (SkillId.Fireball, 3), (SkillId.FrostCleave, 3), (SkillId.StormRing, 3) }, 40, 15f, "v2Light586=false");
            T.v2Light586 = light0;

            // 11) dense + CRT OFF
            crtEnabled = false;
            yield return AtFpsRun991("dense_nocrt", new[] { (SkillId.Fireball, 3), (SkillId.FrostCleave, 3), (SkillId.StormRing, 3) }, 40, 15f, "crt=false");
            crtEnabled = crt0;

            // 12) PixelShock stress (Fire Wall / Pillar path) — GrabPass
            yield return AtFpsRun991("shock_firewall_l3", new[] { (SkillId.HellfirePillar, 3) }, 12, 18f, "FireWall/Pillar shocks");

            // 13) shock OFF
            T.fxShock934 = false;
            yield return AtFpsRun991("shock_firewall_noshock", new[] { (SkillId.HellfirePillar, 3) }, 12, 15f, "fxShock934=false");
            T.fxShock934 = shock0;

            // restore
            T.v2Light586 = light0; T.v2ProjLight660 = projL0; T.fxShaderFireball928 = shaderFb0; T.fxShock934 = shock0;
            T.v2LightStamp991 = stamp0; T.v2DynLightCap991 = dyn0;
            crtEnabled = crt0; atmosOn = atmos0; crtGlow = glow0; ProjectileFX.HitStopScale = 1f;

            var sb = new StringBuilder();
            sb.AppendLine($"Hellord FPS probe t991u/t992u  {System.DateTime.Now:yyyy-MM-dd HH:mm}  {Screen.width}x{Screen.height}");
            sb.AppendLine($"gfx {SystemInfo.graphicsDeviceName} / {SystemInfo.graphicsDeviceVersion}");
            sb.AppendLine($"NOTE: box = software GL (llvmpipe). Absolute FPS << VH PC; use relative deltas.");
            sb.AppendLine();
            sb.AppendLine($"{"name",-28} {"avg",6} {"min",6} {"p1%",6} {"worstMs",8} {"gcB/f",7} {"dc",5} {"SR",5} {"HFX",4} {"shk",4} {"sfx",4} {"brn",4} {"src",4} {"lightMs",8} note");
            foreach (var s in _fps991)
                sb.AppendLine($"{s.name,-28} {s.avgFps,6:0.0} {s.minFps,6:0.0} {s.p1Fps,6:0.0} {s.worstMs,8:0.0} {s.gcAvg,7} {s.dcAvg,5} {s.srMax,5} {s.hellfxMax,4} {s.shockMax,4} {s.sfxMax,4} {s.burnMax,4} {s.lightSrcMax,4} {s.lightMsAvg,8:0.00} {s.note}");
            string path = Path.Combine(_atOut, "fps991.txt");
            File.WriteAllText(path, sb.ToString());
            ALog("wrote " + path);
            ARes("fps991:wrote_table", true, path);
            foreach (var s in _fps991) ARes("fps991:" + s.name, true, $"avg {s.avgFps:0.0} min {s.minFps:0.0} p1 {s.p1Fps:0.0} lightMs {s.lightMsAvg:0.00} src {s.lightSrcMax} {s.note}");
            AtEnd("fps991");
        }

        IEnumerator AtFpsRun991(string name, (SkillId id, int lvl)[] skillsOrNull, int enemies, float sec, string note)
        {
            AtClearWorld(); AtHeroTo(RoomCenterFeet()); SnapCamera(); AtKeepPlay();
            if (skillsOrNull == null || skillsOrNull.Length == 0) AtSetSkills();
            else AtSetSkills(skillsOrNull);
            if (enemies > 0) AtRing(enemies, 3.5f, EType.Imp, true, 1e6f);
            foreach (var s in skills) s.cd = 0;
            Light586MsReset();
            yield return AtWait(0.8f); // settle
            using var gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            using var dc = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            using var bc = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            var frameMs = new List<float>(2048);
            float t0 = Time.realtimeSinceStartup, t = 0; int frames = 0;
            long gcSum = 0, dcSum = 0, bcSum = 0; int srMax = 0, hfxMax = 0, shkMax = 0, sfxMax = 0, burnMax = 0, srcMax = 0;
            double worst = 0;
            while (t < sec)
            {
                float a0 = Time.realtimeSinceStartup;
                yield return null; AtFrameChecks(); AtKeepPlay();
                // keep skills firing + tanky imps alive
                foreach (var s in skills) if (s.cd > 0.05f) s.cd = 0;
                int alive = 0; foreach (var e in _en) if (e.alive && !e.boss) { alive++; e.hp = e.maxHp; e.dmg = 0; }
                if (enemies > 0 && alive < enemies * 0.6f) AtRing(enemies - alive, 3.5f, EType.Imp, true, 1e6f);
                float dt = Time.realtimeSinceStartup - a0; t = Time.realtimeSinceStartup - t0;
                frames++; frameMs.Add(dt * 1000f); worst = System.Math.Max(worst, dt * 1000);
                gcSum += gc.LastValue; dcSum += dc.LastValue; bcSum += bc.LastValue;
                srcMax = Mathf.Max(srcMax, Light586SrcN);
                hfxMax = Mathf.Max(hfxMax, HellFXRunner.I != null ? HellFXRunner.I.Alive : 0);
                shkMax = Mathf.Max(shkMax, CountShock934());
                sfxMax = Mathf.Max(sfxMax, ShaderFxActive928());
                burnMax = Mathf.Max(burnMax, BurnFX.ActiveCount);
                if ((frames % 45) == 0)
                {
                    int s = 0; foreach (var r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) if (r.enabled && r.gameObject.activeInHierarchy) s++;
                    srMax = Mathf.Max(srMax, s);
                }
            }
            frameMs.Sort();
            float sum = 0; foreach (var m in frameMs) sum += m;
            float avgMs = sum / Mathf.Max(1, frames);
            float minFps = 1000f / Mathf.Max(0.001f, frameMs[frameMs.Count - 1]);
            int p1i = Mathf.Clamp(Mathf.FloorToInt(frameMs.Count * 0.99f), 0, frameMs.Count - 1);
            float p1Fps = 1000f / Mathf.Max(0.001f, frameMs[p1i]);
            var samp = new FpsSamp991
            {
                name = name, frames = frames, sec = t, avgFps = 1000f / Mathf.Max(0.001f, avgMs), minFps = minFps, p1Fps = p1Fps, worstMs = (float)worst,
                gcAvg = gcSum / Mathf.Max(1, frames), dcAvg = dcSum / Mathf.Max(1, frames), bcAvg = bcSum / Mathf.Max(1, frames),
                srMax = srMax, hellfxMax = hfxMax, shockMax = shkMax, sfxMax = sfxMax, burnMax = burnMax, lightSrcMax = srcMax,
                lightMsAvg = Light586MsAvg, note = note ?? ""
            };
            _fps991.Add(samp);
            ALog($"FPS {name}: avg {samp.avgFps:0.0} min {samp.minFps:0.0} p1 {samp.p1Fps:0.0} lightMs {samp.lightMsAvg:0.00} srcMax {srcMax} shock {shkMax} sfx {sfxMax} burn {burnMax} {note}");
            File.AppendAllText(Path.Combine(_atOut, "perf.txt"), $"fps991 {name}: avg {samp.avgFps:0.0} min {samp.minFps:0.0} p1 {samp.p1Fps:0.0} lightMs {samp.lightMsAvg:0.00} src {srcMax} gc {samp.gcAvg} dc {samp.dcAvg} {note}\n");
        }
    }
}
#endif
