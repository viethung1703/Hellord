#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1675 first-use lag test: in a fresh process, a real Descend (sealed pact via the altar button) -> loading step -> run,
    /// then every kit skill and basic is cast twice (same setup) and the longest frame (ms, wall clock between frames) in each cast window is logged.
    /// PASS per skill = first cast max frame &lt;= 2 x second cast max + 8 ms. '-preload1675 0' = the old lazy path (the "before" numbers).
    /// ftime1675_lz = Leizu (pact 2), ftime1675_zh = Zhurong (pact 0). loadclip1675 = the same Leizu start with -atRec, a few first casts only.</summary>
    public partial class HellordGame
    {
        readonly System.Diagnostics.Stopwatch _ftSw1675 = new System.Diagnostics.Stopwatch(); double _ftLast1675; List<float> _ftWin1675; string _ftTag1675 = "";
        IEnumerator FtF1675(int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return ZF1618(1); double now = _ftSw1675.Elapsed.TotalMilliseconds; float ms = (float)(now - _ftLast1675); if (_ftWin1675 != null) _ftWin1675.Add(ms); if (ms > 150f && _ftLast1675 > 0) ALog($"t1675 spike {ms:0} ms [{_ftTag1675}] frame {Time.frameCount}"); _ftLast1675 = now;
                pendingLevels = 0; if (mode != Mode.Play) mode = Mode.Play;
                foreach (var p in _pk) if (p.alive) { var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j); p.alive = false; p.sr.gameObject.SetActive(false); }
            }
        }
        /// <summary>Loaded-object counts (textures, materials, sprites, loaded audio clips, game objects): a rise during a cast = something was loaded / built lazily.</summary>
        static int[] FtCounts1675()
        {
            int au = 0; foreach (var a in Resources.FindObjectsOfTypeAll<AudioClip>()) if (a.loadState == AudioDataLoadState.Loaded) au++;
            return new[] { Resources.FindObjectsOfTypeAll<Texture2D>().Length, Resources.FindObjectsOfTypeAll<Material>().Length, Resources.FindObjectsOfTypeAll<Sprite>().Length, au, Resources.FindObjectsOfTypeAll<GameObject>().Length };
        }
        static HashSet<int> FtIds1675() { var h = new HashSet<int>(); foreach (var o in Resources.FindObjectsOfTypeAll<Texture2D>()) h.Add(o.GetInstanceID()); foreach (var o in Resources.FindObjectsOfTypeAll<Material>()) h.Add(o.GetInstanceID()); foreach (var o in Resources.FindObjectsOfTypeAll<AudioClip>()) if (o.loadState == AudioDataLoadState.Loaded) h.Add(o.GetInstanceID()); return h; }
        static string FtNew1675(HashSet<int> before)
        {   // names of textures / materials / loaded clips that appeared (what was loaded or built lazily)
            var l = new List<string>();
            foreach (var o in Resources.FindObjectsOfTypeAll<Texture2D>()) if (!before.Contains(o.GetInstanceID())) l.Add("tex:" + o.name);
            foreach (var o in Resources.FindObjectsOfTypeAll<Material>()) if (!before.Contains(o.GetInstanceID())) l.Add("mat:" + o.name);
            foreach (var o in Resources.FindObjectsOfTypeAll<AudioClip>()) if (o.loadState == AudioDataLoadState.Loaded && !before.Contains(o.GetInstanceID())) l.Add("clip:" + o.name);
            return string.Join(" ", l.Count > 14 ? l.GetRange(0, 14) : l) + (l.Count > 14 ? " +" + (l.Count - 14) : "");
        }
        static string FtDelta1675(int[] a, int[] b) { string[] n = { "tex", "mat", "spr", "clip", "go" }; var s = new List<string>(); for (int i = 0; i < a.Length; i++) if (b[i] != a[i]) s.Add(n[i] + (b[i] - a[i] > 0 ? "+" : "") + (b[i] - a[i])); return s.Count == 0 ? "none" : string.Join(" ", s); }
        IEnumerator FtPress1675(int which, int frames) { if (which == 0) HInput.AtDash = true; else if (which == 1) HInput.AtRmb = true; else HInput.AtUlt = true; yield return FtF1675(1); HInput.AtDash = HInput.AtRmb = HInput.AtUlt = false; yield return FtF1675(frames); }
        IEnumerator FtHold1675(int hold, int after) { HInput.AtLmb = true; yield return FtF1675(hold); HInput.AtLmb = false; yield return FtF1675(after); }

        IEnumerator AtFtime1675(int pact, bool clipRun)
        {
            string scen = clipRun ? "loadclip1675" : pact == 2 ? "ftime1675_lz" : "ftime1675_zh", clip = clipRun ? scen : null; var T = HellordTuning.I;
            var av = System.Environment.GetCommandLineArgs(); int ai = System.Array.IndexOf(av, "-preload1675"); if (ai >= 0 && ai + 1 < av.Length) T.preload1675 = av[ai + 1] != "0";
            ai = System.Array.IndexOf(av, "-preloadWarm1675"); if (ai >= 0 && ai + 1 < av.Length) T.preloadWarmShaders1675 = av[ai + 1] != "0";
            if (System.Array.IndexOf(av, "-preloadNoAudio1675") >= 0) T.preloadAudioFolders1675 = new string[0];
            bool pre = T.preload1675; int runs0 = HellordPreload.Runs;
            _ftSw1675.Restart(); _ftLast1675 = 0;
            yield return ZhStart1618(scen, clip, pact == 2 ? SkillId.ThunderDrumStrikes : SkillId.Fireball, pact);
            if (clip != null) _zClip = clip;
            bool loaded = HellordPreload.Runs > runs0;
            ARes(scen + ":loading", pre ? loaded && HellordPreload.LastPact == pact && !Loading1675 : !loaded, pre ? "preload ON: " + HellordPreload.LastReport : "preload OFF (lazy first use, the old path)");
            int m0 = T.lzMove1666, s0 = T.lzSpecial1666, u0 = T.lzUlt1666; var L = new List<Enemy>(); Vector2 H0 = _heroPos; T.kitSlotOverride1639 = true;
            Enemy Pack(EType t, Vector2 at, float hpv = 40f) => ZMk1618(L, t, at, 0f, hpv);
            void Basic(SkillId id) { var s = new SkillInst { def = HData.Skill(id) }; if (skills.Count > 0) skills[0] = s; else skills.Add(s); }
            void Ring(int n, float r, float hpv) { for (int i = 0; i < n; i++) Pack(i == 0 ? EType.Brute : EType.Imp, H0 + new Vector2(3.5f + Mathf.Cos(i * 1.3f) * r, Mathf.Sin(i * 1.3f) * r * 0.7f), hpv); }
            var rows = new List<string>(); int fails = 0; var csv = new System.Text.StringBuilder("skill,first_max_ms,second_max_ms,first_mean_ms,second_mean_ms,first_new_assets,second_new_assets,first_delta,second_delta\n"); int lazyFirst = 0;
            // quiet baseline
            _ftLast1675 = _ftSw1675.Elapsed.TotalMilliseconds; _ftTag1675 = "baseline"; _ftWin1675 = new List<float>(); yield return FtF1675(60); var bl = new List<float>(_ftWin1675); bl.Sort(); float med = bl[bl.Count / 2]; _ftWin1675 = null;
            ARes(scen + ":baseline", true, $"quiet frame median {med:0.0} ms, max {bl[bl.Count - 1]:0.0} ms");
            IEnumerator Two(string name, System.Action setup, System.Func<IEnumerator> cast)
            {
                float[] mx = new float[2], mean = new float[2]; var dl = new string[2]; var nload = new int[2];
                for (int pass = 0; pass < (clipRun ? 1 : 2); pass++) // the clip shows first casts only
                {
                    _ftTag1675 = name + "#" + (pass + 1);
                    ZKill1618(L); _heroPos = H0; SnapCamera(); ZhMoveCd = ZhSpecCd = ZhUltCd = 0f; _dashCd49 = 0f; if (skills.Count > 0) skills[0].cd = 0f; atMouse986 = H0 + new Vector2(3.5f, 0f);
                    setup(); yield return FtF1675(6);
                    var ids0 = FtIds1675(); var c0 = FtCounts1675(); _ftWin1675 = new List<float>(); yield return cast(); var w = _ftWin1675; _ftWin1675 = null; var c1 = FtCounts1675(); ALog($"t1675 new during {name}#{pass + 1}: {FtNew1675(ids0)}");
                    dl[pass] = FtDelta1675(c0, c1); nload[pass] = (c1[0] - c0[0]) + (c1[1] - c0[1]) + (c1[2] - c0[2]) + (c1[3] - c0[3]); // tex + mat + sprite + clip (game objects churn with pooled FX, reported only)
                    float s = 0; foreach (var f in w) { mx[pass] = Mathf.Max(mx[pass], f); s += f; } mean[pass] = w.Count > 0 ? s / w.Count : 0f;
                    HInput.AtLmb = false; AtMove1396 = Vector2.zero; yield return FtF1675(12);
                }
                bool ok = clipRun || mx[0] <= 2f * mx[1] + 8f; if (!ok) fails++;
                ARes(scen + ":" + name, ok, $"first {mx[0]:0.0} ms, second {mx[1]:0.0} ms (means {mean[0]:0.0}/{mean[1]:0.0}) | new during cast 1: {dl[0]}" + (clipRun ? "" : $", cast 2: {dl[1]}"));
                csv.Append($"{name},{mx[0]:0.0},{mx[1]:0.0},{mean[0]:0.0},{mean[1]:0.0},{nload[0]},{nload[1]},{dl[0]},{dl[1]}\n"); lazyFirst += nload[0];
            }
            if (pact == 2)
            {
                T.lzMove1666 = T.lzSpecial1666 = T.lzUlt1666 = 0; ClearLz1666(); LzRunReset1666();
                var basics = clipRun ? new[] { SkillId.ThunderDrumStrikes, SkillId.ThunderHound, SkillId.SparkSpray } : new[] { SkillId.ThunderDrumStrikes, SkillId.ThunderLash, SkillId.LiveWire, SkillId.ThunderHound, SkillId.SparkSpray, SkillId.BallLightning, SkillId.ThunderJavelin };
                foreach (var id in basics) { var b = id; yield return Two(b.ToString(), () => { Basic(b); Ring(5, 1f, 40f); }, () => FtHold1675(b == SkillId.ThunderHound ? 3 : 36, 30)); }
                for (int k = 1; k <= (clipRun ? 1 : 4); k++) { int kk = k; yield return Two("lzMove" + kk, () => { Basic(SkillId.ThunderDrumStrikes); T.lzMove1666 = kk; Ring(4, 1.2f, 40f); }, () => { AtMove1396 = Vector2.right; return FtPress1675(0, 40); }); }
                T.lzMove1666 = 0;
                for (int k = 1; k <= (clipRun ? 1 : 4); k++) { int kk = k; yield return Two("lzSpecial" + kk, () => { Basic(SkillId.ThunderDrumStrikes); T.lzSpecial1666 = kk; Ring(5, 1f, 300f); if (L.Count > 0) atMouse986 = L[0].pos; }, () => FtPress1675(1, 50)); }
                T.lzSpecial1666 = 0;
                if (!clipRun) for (int k = 1; k <= 4; k++)
                {
                    int kk = k;
                    yield return Two("lzUlt" + kk, () =>
                    {
                        Basic(SkillId.ThunderDrumStrikes); T.lzUlt1666 = kk; LzRunReset1666(); Ring(6, 1.2f, 300f);
                        if (kk == 1) LzCombos1666 = T.dcNeed1666; else if (kk == 2) LzInfusions1666 = T.haNeed1666; else if (kk == 3) { LzEnergy1666 = T.trNeed1666; foreach (var e in L) ApplyStatus(e, Status.Shock, 1f, 1f, skills[0]); }
                        else { var el = Pack(EType.Brute, H0 + new Vector2(4f, 0.4f), 1000f); el.elite = true; el.hp = el.maxHp * (T.jwCull1666 - 0.05f); }
                        if (L.Count > 0) atMouse986 = L[0].pos;
                    }, () => FtPress1675(2, 90));
                }
                T.lzUlt1666 = 0;
                T.lzMove1666 = m0; T.lzSpecial1666 = s0; T.lzUlt1666 = u0; ClearLz1666(); LzRunReset1666();
            }
            else
            {
                foreach (var id in new[] { SkillId.Fireball, SkillId.RollingMagmaRock, SkillId.FirePillar, SkillId.FireWall, SkillId.Hydra }) { var b = id; yield return Two(b.ToString(), () => { Basic(b); Ring(5, 1f, 40f); }, () => FtHold1675(36, 30)); }
                for (int k = 1; k <= 3; k++) { int kk = k; yield return Two("zhMove" + kk, () => { Basic(SkillId.Fireball); T.kitMove1618 = kk; Ring(4, 1.2f, 40f); }, () => { AtMove1396 = Vector2.right; return FtPress1675(0, 40); }); }
                for (int k = 1; k <= 5; k++) { int kk = k; yield return Two("zhSpecial" + kk, () => { Basic(SkillId.Fireball); T.kitSpecial1618 = kk; Ring(5, 1f, 300f); if (L.Count > 0) atMouse986 = L[0].pos; }, () => FtPress1675(1, 50)); }
                for (int k = 1; k <= 4; k++)
                {
                    int kk = k;
                    yield return Two("zhUlt" + kk, () =>
                    {
                        Basic(SkillId.Fireball); T.kitUlt1618 = kk; Ring(6, 1.2f, 300f); if (kk == 1) ZhSunMeter1618 = T.swMeter1618; else if (kk == 4) ZhNearMiss1618 = T.vbNeed1618; else if (kk == 3) foreach (var e in L) ApplyStatus(e, Status.Burn, 1f, 1f, skills[0]);
                        if (L.Count > 0) atMouse986 = L[0].pos;
                    }, () => FtPress1675(2, 90));
                }
            }
            if (_atOut != null) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, scen + "_frametimes.csv"), csv.ToString());
            ARes(scen + ":summary", fails == 0, $"preload {(pre ? "ON" : "OFF")}, {fails} skills with a first-cast spike (> 2x second + 8 ms); assets loaded / built during first casts {lazyFirst}; csv {scen}_frametimes.csv");
            if (clipRun) ARes(scen + ":clip", _r1561 > 0 || _atRecFps <= 0, $"frames {_r1561}");
            ZKill1618(L); _zClip = null; ZhEnd1618(scen, clip);
        }
    }
}
#endif
