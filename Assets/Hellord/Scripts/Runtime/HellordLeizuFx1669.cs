using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1669 (VH GO): LeizuFX sub-effects pass. Wires every optional part of Art/fx_leizu_t1666/spec.md that t1666 left out:
    //  sky strikes (DrumStrike / Decree / Judgement / Tribunal: Spark, Bolt, Hit, Ground|Splash), DrumStrike_Pips, Hound
    //  Arrive / Aura / Bite, LiveWire_Hit / Overload, HeavenArc_Link / Node, JudgementWarp Out / In / Flash / Arc / Haze,
    //  Judgement_Arc, ShockStatus, MirrorDazzled, GroundingWard_Zap, StormStride_Trail, Thunderclap_Flash / Haze,
    //  DrumBoom_Haze, StaticField_Core, Conductivity_Jump.
    //  DATA-DRIVEN (v2 swap = data only):
    //   * material folder: TZ.lzFxDir1669 (default "LeizuFX/Materials/"); a name missing there loads from TZ.lzFxDirFallback1669.
    //     Renamed v2 materials: TZ.lzFxRename1669 = "LZFX_Old=LZFX_New;...". The _Glow twin follows the base name.
    //   * one-shots / links: LzFxDefs1669 rows (material, pivot offset px, rect px, life, delay, stagger, layer, progress curve, haze radii).
    //   * sky strikes: LzStrikeDefs1669 rows (material prefix, sky height px, lives, rects, spark yes/no).
    //  FX ONLY: nothing here changes damage, timing or targeting. A missing material = the old placeholder draws, as before.
    //  Built-in pipeline note: the *_Haze materials grab the URP 2D sorting-layer texture; their built-in fallback draws nothing.
    // =====================================================================================================================
    public partial class HellordGame
    {
        enum LzL { Ground, Over, Over1, Haze } // FbGround+YSort+40 | YSort+60 | YSort+61 | FbGround+YSort+45 (after the ground layer)
        class LzFxDef
        {
            public string mat; public Vector2 off; public Vector4 rc; public float life, delay, stagger, plife, p0, r0, r1; public LzL layer; public bool bolt;
        }
        static LzFxDef LzD(string mat, float ox, float oy, float l, float b, float w, float h, float life, LzL layer, float delay = 0f) => new LzFxDef { mat = mat, off = new Vector2(ox, oy), rc = new Vector4(l, b, w, h), life = life, layer = layer, delay = delay };
        static LzFxDef LzB(string mat, float ox, float oy, float life, float delay = 0f, float stagger = 0f) => new LzFxDef { mat = mat, off = new Vector2(ox, oy), life = life, delay = delay, stagger = stagger, layer = LzL.Over, bolt = true };
        /// <summary>spec.md tables (rects in art px from the pivot, lives s). Change a row = change the effect; v2 names go through lzFxRename1669.</summary>
        static readonly Dictionary<string, LzFxDef> LzFxDefs1669 = new Dictionary<string, LzFxDef>
        {
            // 1 Thunder Drum Strikes: the clap (beat 3)
            { "clapFlash",  LzD("LZFX_Thunderclap_Flash", 0, 4, -28, -26, 56, 50, 0.3f, LzL.Over) },
            { "clapHaze",   new LzFxDef { mat = "LZFX_Thunderclap_Haze", rc = new Vector4(-48, -28, 96, 56), life = 0.4f, layer = LzL.Haze, r0 = 6, r1 = 40 } },
            // 3 Live Wire: per tick contact flash (_Progress = a/0.27 + 0.3 over 0.1 s)
            { "wireHit",    new LzFxDef { mat = "LZFX_LiveWire_Hit", rc = new Vector4(-12, -12, 24, 22), life = 0.1f, plife = 0.27f, p0 = 0.3f, layer = LzL.Over1 } },
            // 4 Thunder Hound
            { "houndArrive", LzD("LZFX_Hound_Arrive", 0, 0, -16, -10, 32, 40, 0.35f, LzL.Over) },
            { "houndBite",  LzD("LZFX_Hound_Bite", 14, 10, -22, -18, 44, 34, 0.3f, LzL.Over) }, // off.x mirrored by facing
            // 6 Grounding Ward: zap to the attacker (off.x mirrored toward it)
            { "wardZap",    LzB("LZFX_GroundingWard_Zap", 9, 10, 0.3f) },
            // 7 Storm Stride footfall afterimage (rect from the segment)
            { "strideTrail", LzD("LZFX_StormStride_Trail", 0, 0, -8, -6, 16, 34, 0.5f, LzL.Ground) },
            // 9 Mark of Judgement payoff arcs (0.12 + 0.06 i)
            { "judgeArc",   LzB("LZFX_Judgement_Arc", 0, 0, 0.35f, 0.12f, 0.06f) },
            // 10 Conductivity spread
            { "condJump",   LzB("LZFX_Conductivity_Jump", 0, 0, 0.35f) },
            // 11 Ministry Drum boom haze (R 45)
            { "boomHaze",   new LzFxDef { mat = "LZFX_DrumBoom_Haze", rc = new Vector4(-52, -30, 104, 60), life = 0.35f, layer = LzL.Haze, r0 = 6, r1 = 44 } },
            // 12 Static Field core flash
            { "sfCore",     LzD("LZFX_StaticField_Core", 0, 10, -22, -22, 44, 40, 0.27f, LzL.Over) },
            // 14 Heaven-Splitting Arc: link k starts at 0.06 k, node flash at each target
            { "arcLink",    LzB("LZFX_HeavenArc_Link", 0, 0, 0.4f, 0f, 0.06f) },
            { "arcNode",    LzD("LZFX_HeavenArc_Node", 0, 0, -20, -20, 40, 36, 0.27f, LzL.Over1) },
            // 16 Judgement Warp (spec timeline: out 0, in +0.2, impact +0.4; R 48 = 3 u)
            { "jwOut",      LzD("LZFX_JudgementWarp_Out", 0, 0, -16, -10, 32, 46, 0.4f, LzL.Over) },
            { "jwIn",       LzD("LZFX_JudgementWarp_In", 0, 0, -16, -10, 32, 64, 0.4f, LzL.Over, 0.2f) },
            { "jwFlash",    LzD("LZFX_JudgementWarp_Flash", 0, 8, -32, -30, 64, 56, 0.35f, LzL.Over, 0.4f) },
            { "jwGround",   LzD("LZFX_JudgementWarp", 0, 0, -59, -37, 118, 78, 0.7f, LzL.Ground, 0.4f) },
            { "jwArc",      LzB("LZFX_JudgementWarp_Arc", 0, 2, 0.3f, 0.4f, 0.04f) },
            { "jwHaze",     new LzFxDef { mat = "LZFX_JudgementWarp_Haze", rc = new Vector4(-60, -36, 120, 72), life = 0.45f, delay = 0.4f, layer = LzL.Haze, r0 = 8, r1 = 48 } },
        };
        class LzStrikeDef { public string pre, ground; public float height, boltLife, hitLife, groundLife; public Vector4 hitRc, groundRc; public bool spark; }
        /// <summary>spec.md "Shared recipe: sky strike" (v2: every sky height 220 px = full screen). Bolt pivot = strike + (-6 + 12 seed, height) px, _TargetPx to strike + (0,1); Hit at +(0,3); Spark at +(0,2).</summary>
        static readonly Dictionary<string, LzStrikeDef> LzStrikeDefs1669 = new Dictionary<string, LzStrikeDef>
        {
            { "DrumStrike", new LzStrikeDef { pre = "LZFX_DrumStrike", ground = "_Ground", height = 220, boltLife = 0.33f, hitLife = 0.27f, groundLife = 0.45f, hitRc = new Vector4(-18, -18, 36, 32), groundRc = new Vector4(-24, -18, 48, 40), spark = true } },
            { "Decree",     new LzStrikeDef { pre = "LZFX_Decree", ground = "_Ground", height = 220, boltLife = 0.3f, hitLife = 0.27f, groundLife = 0.4f, hitRc = new Vector4(-16, -16, 32, 30), groundRc = new Vector4(-23, -17, 46, 38) } },
            { "Judgement",  new LzStrikeDef { pre = "LZFX_Judgement", ground = "_Ground", height = 220, boltLife = 0.4f, hitLife = 0.27f, groundLife = 0.6f, hitRc = new Vector4(-28, -26, 56, 50), groundRc = new Vector4(-33, -23, 66, 50) } },
            { "Tribunal",   new LzStrikeDef { pre = "LZFX_Tribunal", ground = "_Splash", height = 220, boltLife = 0.33f, hitLife = 0.27f, groundLife = 0.45f, hitRc = new Vector4(-20, -20, 40, 36), groundRc = new Vector4(-27, -19, 54, 42) } },
        };

        // ---------------------------------------------------------------- material lookup (folder + rename map from tuning)
        string _lzMatKey1669; Dictionary<string, string> _lzRename1669;
        string LzFxName1669(string n)
        {
            if (_lzRename1669 == null || _lzRename1669.Count == 0) return n; if (_lzRename1669.TryGetValue(n, out var r)) return r;
            if (n.EndsWith("_Glow") && _lzRename1669.TryGetValue(n.Substring(0, n.Length - 5), out var b)) return b + "_Glow"; return n;
        }
        Material LzFxLoad1669(string n)
        {
            var T = TZ; string dir = T != null && !string.IsNullOrEmpty(T.lzFxDir1669) ? T.lzFxDir1669 : "LeizuFX/Materials/", fb = T != null ? T.lzFxDirFallback1669 : null, ren = T != null ? T.lzFxRename1669 : null;
            string key = dir + "|" + fb + "|" + ren;
            if (key != _lzMatKey1669)
            {   // folder / rename changed (F1 tuning or a v2 drop): reload everything
                _lzMatKey1669 = key; _lzMat1666.Clear(); _lzRename1669 = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(ren)) foreach (var p in ren.Split(';')) { int e = p.IndexOf('='); if (e > 0) _lzRename1669[p.Substring(0, e).Trim()] = p.Substring(e + 1).Trim(); }
            }
            var m = Resources.Load<Material>(dir + LzFxName1669(n));
            if (m == null && !string.IsNullOrEmpty(fb) && fb != dir) m = Resources.Load<Material>(fb + n);
            return m;
        }
        void LzFxKeyCheck1669()
        {   // cheap per-frame check so a changed folder drops the cache before the next lookup
            var T = TZ; if (T == null) return; string dir = !string.IsNullOrEmpty(T.lzFxDir1669) ? T.lzFxDir1669 : "LeizuFX/Materials/";
            if (_lzMatKey1669 != null && _lzMatKey1669 != dir + "|" + T.lzFxDirFallback1669 + "|" + T.lzFxRename1669) { _lzMatKey1669 = null; _lzMat1666.Clear(); }
        }

        // ---------------------------------------------------------------- players
        int LzOrd1669(LzL l, Vector2 p) => l == LzL.Ground ? FbGround + YSort(p.y) + 40 : l == LzL.Haze ? FbGround + YSort(p.y) + 45 : l == LzL.Over1 ? YSort(p.y) + 61 : YSort(p.y) + 60;
        /// <summary>one-shot quad from LzFxDefs1669 (pivot p + row offset px, mirrored when flipX). sc scales rect + haze radii (radius supports). False = no material (keep the placeholder).</summary>
        bool LzPlay1669(string key, Vector2 p, float sc = 1f, float delay = 0f, bool flipX = false, Vector4? rc = null, ZP[] ps = null)
        {
            if (!LzFxDefs1669.TryGetValue(key, out var d) || d.bolt || !LzFxLive1666(d.mat)) return false;
            Vector2 at = p + new Vector2(flipX ? -d.off.x : d.off.x, d.off.y) / 16f; Vector4 r = rc ?? d.rc * Mathf.Max(0.25f, sc);
            _lzFx1666.Add(new LzFxO { kind = -1, p = at, life = d.life, mat = d.mat, rc = r, ord = LzOrd1669(d.layer, at), ps = ps, delay = d.delay + delay, plife = d.plife, p0 = d.p0, r0 = d.r0 * sc, r1 = d.r1 * sc, haze = d.layer == LzL.Haze });
            return true;
        }
        /// <summary>endpoint link from LzFxDefs1669 (a + row offset px, x mirrored toward b): i-th of a series starts at delay + stagger i.</summary>
        bool LzLink1669(string key, Vector2 a, Vector2 b, int i = 0)
        {
            if (!LzFxDefs1669.TryGetValue(key, out var d) || !d.bolt || !LzFxLive1666(d.mat)) return false;
            Vector2 at = a + new Vector2(b.x < a.x ? -d.off.x : d.off.x, d.off.y) / 16f;
            _lzFx1666.Add(new LzFxO { kind = -2, p = at, q = b, life = d.life, mat = d.mat, ord = YSort(Mathf.Min(at.y, b.y)) + 60, delay = d.delay + d.stagger * i, seed = Mathf.Repeat(0.41f + i * 0.173f + a.x * 0.29f, 1f) });
            return true;
        }
        /// <summary>sky strike recipe at p (set = LzStrikeDefs1669 key). False = no bolt material (caller keeps LzSky1666).</summary>
        bool LzStrike1669(string set, Vector2 p)
        {
            if (!LzStrikeDefs1669.TryGetValue(set, out var s) || !LzFxLive1666(s.pre + "_Bolt")) return false;
            const float U = 1f / 16f; float seed = Mathf.Repeat(p.x * 0.731f + p.y * 0.377f + runTime * 3.1f, 1f); int over = YSort(p.y) + 60;
            _lzFx1666.Add(new LzFxO { kind = -2, p = p + new Vector2(-6f + 12f * seed, s.height) * U, q = p + new Vector2(0f, 1f) * U, life = s.boltLife, mat = s.pre + "_Bolt", ord = over, seed = seed });
            if (LzFxLive1666(s.pre + "_Hit")) _lzFx1666.Add(new LzFxO { kind = -1, p = p + new Vector2(0f, 3f) * U, life = s.hitLife, mat = s.pre + "_Hit", rc = s.hitRc, ord = over + 2 });
            if (LzFxLive1666(s.pre + s.ground)) _lzFx1666.Add(new LzFxO { kind = -1, p = p, life = s.groundLife, mat = s.pre + s.ground, rc = s.groundRc, ord = FbGround + YSort(p.y) + 40 });
            if (s.spark && LzFxLive1666(s.pre + "_Spark")) _lzFx1666.Add(new LzFxO { kind = -1, p = p + new Vector2(0f, 2f) * U, life = 0.14f, mat = s.pre + "_Spark", rc = new Vector4(-8, -8, 16, 16), ord = over + 4 }); // the -0.14 s pre-flash, played with the impact (gameplay has no lead-in)
            return true;
        }
        /// <summary>Storm Stride footfall afterimage every 0.18 s while moving (segment = feet 0.18 s ago -> now).</summary>
        Vector2 _lzTrailP1669; float _lzTrailT1669 = -9f;
        void LzStrideTrail1669()
        {
            if (LzStrideT1666 <= 0f) { _lzTrailT1669 = -9f; return; }
            if (_lzTrailT1669 < 0f) { _lzTrailP1669 = _heroPos; _lzTrailT1669 = runTime; return; }
            if (runTime - _lzTrailT1669 < 0.18f) return; Vector2 a = _lzTrailP1669; _lzTrailP1669 = _heroPos; _lzTrailT1669 = runTime;
            Vector2 d = (_heroPos - a) * 16f; if (d.magnitude < 2f) return; float dx = Mathf.Round(d.x), dy = Mathf.Round(d.y);
            LzPlay1669("strideTrail", a, 1f, 0f, false, new Vector4(Mathf.Min(0f, dx) - 8f, Mathf.Min(0f, dy) - 6f, Mathf.Abs(dx) + 16f, Mathf.Abs(dy) + 34f), new[] { ZP.V("_TargetPx", new Vector2(dx, dy)) });
        }
        /// <summary>ApplyStatus shock: Leizu kit + LZFX_ShockStatus present = the LeizuFX overlay replaces the legacy crackle (true).</summary>
        bool LzShockFx1669(Enemy e)
        {
            if (!LzOn1666 || !LzFxLive1666("LZFX_ShockStatus")) return false;
            e.lzShockAt1669 = runTime; e.lzShockDur1669 = Mathf.Min(e.shockT, HellordTuning.I.shockFxSec948); return true;
        }
        /// <summary>per-enemy body overlays: shock (spec 17), Mirror Flash dazzle (spec 8).</summary>
        void LzDrawOverlays1669()
        {
            bool sh = LzFxLive1666("LZFX_ShockStatus"), dz = LzFxLive1666("LZFX_MirrorDazzled"); if (!sh && !dz) return;
            for (int i = 0; i < maxEnemies; i++)
            {
                var e = _en[i]; if (!e.alive || e.dying) continue; float sa = runTime - e.lzShockAt1669; bool s1 = sh && sa >= 0f && sa < e.lzShockDur1669, d1 = dz && e.lzDazzT1669 > runTime; if (!s1 && !d1) continue;
                Vector2 bs = BodySize1595(e) * 16f; float w = Mathf.Round(Mathf.Clamp(bs.x, 8f, 64f)), h = Mathf.Round(Mathf.Clamp(bs.y, 10f, 96f)); int o = (e.sr != null ? e.sr.sortingOrder : YSort(e.pos.y)) + 1;
                if (s1) LzFx1666("LZFX_ShockStatus", e.pos, -(w * 0.5f + 7f), -6f, w + 14f, h + 14f, o, sa, (e.id * 0.137f) % 1f, ZP.V("_Body", new Vector4(w, h, 0, 0)), ZP.F("_Intensity", Mathf.Clamp01((e.lzShockDur1669 - sa) / 0.3f)));
                if (d1) LzFx1666("LZFX_MirrorDazzled", e.pos, -14f, h - 4f, 28f, 16f, o + 2, e.lzDazzS1669 - (e.lzDazzT1669 - runTime), (e.id * 0.211f) % 1f, ZP.V("_Body", new Vector4(w, h, 0, 0)) /* v2.1: orbit on this type's head */, ZP.F("_Intensity", Mathf.Clamp01((e.lzDazzT1669 - runTime) / 0.2f)));
            }
        }
    }
}
