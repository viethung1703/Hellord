using System.Collections.Generic;
using UnityEngine;

// t1567: HFX / HA strips for the pact LMB basics (Resources/HellordArt/Skills1567/<skill>/<file>_strip.png + *_glow_strip.png twins).
// Sizes, frames, ms, pivots and glow gain / tint are copied from each folder's README.md (HFX t1567, Leizu set delivered 2:43 PM).
// dir16 strips: 16 rows, row 0 = east, clockwise on screen (y down). Glow = additive mask through the FrostGlow shader (same as the t1481 fire basics).
// A key whose file is missing (or pbArt1567 off) returns null and the skill draws its code placeholder instead.
namespace Hellord
{
    public partial class HellordGame
    {
        sealed class PbArtS { public Sprite[,] f, g; public int n, rows; public float[] ms; public Material mat; }
        const uint PbCy = 0x2ce8f5, PbSt = 0xc0cbdc, PbRd = 0xe43b44, PbYl = 0xfee761, PbPk = 0xf6757a;
        // key, file (under Skills1567/), frame w, h, frames, rows, pivot x, y (px from top-left), ms per frame (float or float[]), glow gain (0 = none), glow tint
        static readonly object[][] PbArtSpec1567 =
        {
            new object[] { "ss_spark",  "spark_spray/spark_spray_spark_loop", 20, 19, 8, 1, 9, 9, 50f, 0.55f, PbCy },
            new object[] { "ss_trail",  "spark_spray/spark_spray_trail", 7, 6, 4, 1, 3, 2, 50f, 0.35f, PbCy },
            new object[] { "ss_hit",    "spark_spray/spark_spray_hit", 42, 37, 6, 1, 21, 16, 40f, 0.60f, PbSt },
            new object[] { "ss_fizzle", "spark_spray/spark_spray_fizzle", 19, 16, 6, 1, 9, 6, 50f, 0.45f, PbCy },
            new object[] { "bl_spawn",  "ball_lightning/ball_lightning_spawn", 30, 28, 6, 1, 14, 14, 40f, 0.60f, PbCy },
            new object[] { "bl_orb",    "ball_lightning/ball_lightning_orb_loop", 32, 31, 12, 1, 15, 15, 50f, 0.60f, PbCy },
            new object[] { "bl_fade",   "ball_lightning/ball_lightning_fade", 36, 36, 8, 1, 17, 17, 50f, 0.55f, PbCy },
            new object[] { "bl_shadow", "ball_lightning/ball_lightning_shadow", 9, 5, 1, 1, 4, 2, 1000f, 0f, PbCy },
            new object[] { "bl_zaphit", "ball_lightning/ball_lightning_zap_hit", 33, 29, 6, 1, 17, 13, 40f, 0.50f, PbSt },
            new object[] { "bl_zap16",  "ball_lightning/ball_lightning_zap_L16_dir16", 41, 40, 4, 16, 20, 19, 40f, 0.45f, PbSt },
            new object[] { "bl_zap24",  "ball_lightning/ball_lightning_zap_L24_dir16", 57, 56, 4, 16, 28, 27, 40f, 0.45f, PbSt },
            new object[] { "bl_zap32",  "ball_lightning/ball_lightning_zap_L32_dir16", 73, 72, 4, 16, 36, 36, 40f, 0.45f, PbSt },
            new object[] { "bl_zap40",  "ball_lightning/ball_lightning_zap_L40_dir16", 89, 89, 4, 16, 44, 44, 40f, 0.45f, PbSt },
            new object[] { "bl_zap48",  "ball_lightning/ball_lightning_zap_L48_dir16", 105, 105, 4, 16, 52, 52, 40f, 0.45f, PbSt },
            new object[] { "tj_flight", "thunder_javelin/thunder_javelin_flight_dir16", 51, 51, 4, 16, 25, 25, 50f, 0.55f, PbCy },
            new object[] { "tj_impact", "thunder_javelin/thunder_javelin_impact", 50, 44, 7, 1, 23, 19, 40f, 0.70f, PbSt },
            new object[] { "tj_bolthit","thunder_javelin/thunder_javelin_bolt_hit", 31, 26, 6, 1, 15, 11, 40f, 0.50f, PbSt },
            new object[] { "tj_fork32", "thunder_javelin/thunder_javelin_fork_L32_dir16", 73, 72, 5, 16, 36, 36, 40f, 0.45f, PbSt },
            new object[] { "tj_fork48", "thunder_javelin/thunder_javelin_fork_L48_dir16", 104, 105, 5, 16, 52, 52, 40f, 0.45f, PbSt },
            // Xingtian set (HFX 2:50 PM)
            new object[] { "xc_arc",    "headsmans_chop/headsmans_chop_arc_dir16", 78, 58, 6, 16, 39, 27, 33f, 0.35f, PbSt },
            new object[] { "xc_wave",   "headsmans_chop/headsmans_chop_blood_wave", 56, 44, 16, 1, 27, 24, 33f, 0.30f, PbRd },
            new object[] { "xc_spark",  "headsmans_chop/headsmans_chop_hit_spark_dir16", 42, 43, 7, 16, 21, 21, 33f, 0.45f, PbYl },
            new object[] { "xc_skid",   "headsmans_chop/headsmans_chop_skid_dust", 23, 7, 11, 1, 15, 5, 40f, 0f, PbSt },
            new object[] { "xs_impact", "earth_splitting_slam/earth_splitting_slam_impact", 43, 34, 14, 1, 21, 20, 33f, 0.45f, PbYl },
            new object[] { "xs_crack",  "earth_splitting_slam/earth_splitting_slam_crack_decal", 51, 41, 6, 1, 26, 19, 1000f, 0.40f, PbRd },
            new object[] { "xs_erupt",  "earth_splitting_slam/earth_splitting_slam_eruption", 51, 48, 14, 1, 25, 27, 33f, 0.40f, PbRd },
            new object[] { "xs_dust1",  "earth_splitting_slam/earth_splitting_slam_dust_v1", 21, 11, 12, 1, 9, 8, 40f, 0f, PbSt },
            new object[] { "xs_dust2",  "earth_splitting_slam/earth_splitting_slam_dust_v2", 23, 12, 12, 1, 11, 8, 40f, 0f, PbSt },
            new object[] { "xa_spin",   "twin_axe_throw/twin_axe_throw_axe_spin", 24, 24, 16, 1, 12, 12, 33f, 0.25f, PbSt },
            new object[] { "xa_smear",  "twin_axe_throw/twin_axe_throw_spin_smear", 20, 20, 16, 1, 10, 10, 33f, 0f, PbSt },
            new object[] { "xa_trail",  "twin_axe_throw/twin_axe_throw_trail_dir16", 49, 49, 4, 16, 24, 24, 50f, 0f, PbSt },
            new object[] { "xa_spark",  "twin_axe_throw/twin_axe_throw_hit_spark_dir16", 42, 44, 7, 16, 21, 22, 33f, 0.45f, PbYl },
            new object[] { "xa_catch",  "twin_axe_throw/twin_axe_throw_catch_flash", 25, 25, 6, 1, 12, 12, 33f, 0.40f, PbYl },
            new object[] { "xp_chop",   "phantom_axes/phantom_axes_chop_dir16", 53, 44, 16, 16, 27, 34, 33f, 0.40f, PbPk },
            new object[] { "xp_flash",  "phantom_axes/phantom_axes_hit_flash", 29, 26, 6, 1, 13, 12, 33f, 0.45f, PbPk },
        };
        static readonly Dictionary<string, PbArtS> _pbArt1567 = new Dictionary<string, PbArtS>(); static bool _pbArtTried1567; static Shader _pbGlowSh1567;
        public static string PbArtStatus1567 = "";
        static PbArtS PbA1567(string key)
        {
            var T = HellordTuning.I; if (T == null || !T.pbArt1567) return null;
            if (!_pbArtTried1567)
            {
                _pbArtTried1567 = true; int got = 0, glow = 0; _pbGlowSh1567 = Resources.Load<Shader>("HellordShaders/HellordFrostGlow"); if (_pbGlowSh1567 == null) _pbGlowSh1567 = Shader.Find("Hellord/FrostGlow1430");
                bool glowOk = T.pbGlow1567 && _pbGlowSh1567 != null && _pbGlowSh1567.isSupported;
                foreach (var sp in PbArtSpec1567)
                {
                    string k = (string)sp[0], file = (string)sp[1]; int fw = (int)sp[2], fh = (int)sp[3], n = (int)sp[4], rows = (int)sp[5]; float px = (int)sp[6], py = (int)sp[7]; float gain = (float)sp[9]; uint tint = (uint)sp[10]; object ms = sp[8];
                    PbSpecOv1567(k, ref file, ref fw, ref fh, ref n, ref rows, ref px, ref py, ref ms, ref gain, ref tint); // t1572: remap + spec.json (v2 drop-in)
                    var t = PbTex1567(file + "_strip"); if (t == null || t.width < fw * n || t.height < fh * rows) continue;
                    var a = new PbArtS { n = n, rows = rows, f = PbGrid1567(t, fw, fh, n, rows, px, py), ms = ms is float[] arr ? arr : new[] { (float)ms } }; got++;
                    var tc = new Color32((byte)(tint >> 16), (byte)(tint >> 8), (byte)tint, 255);
                    if (glowOk && gain > 0f) { var tg = PbTex1567(file + "_glow_strip"); if (tg != null && tg.width >= fw * n && tg.height >= fh * rows) { a.g = PbGrid1567(tg, fw, fh, n, rows, px, py);
                        if (T.pbGlowMode1567 == 1) a.mat = PbAddMat1567(tc, T.pbGlowAddInt1567 * gain / 0.45f, T.pbGlowAddBands1567); // plain additive (pre-blurred v2 masks)
                        if (a.mat == null) { a.mat = new Material(_pbGlowSh1567) { name = "pbglow1567_" + k }; a.mat.SetColor("_Tint", tc); a.mat.SetFloat("_Gain", T.pbGlowGain1567 * gain / 0.45f); a.mat.SetFloat("_Add", T.pbGlowAdd1567); }
                        glow++; } }
                    _pbArt1567[k] = a;
                }
                PbArtStatus1567 = "pbArt1567: " + got + "/" + PbArtSpec1567.Length + " strips from " + PbArtRoot1567 + ", spec.json overrides " + _pbSpecOv1567 + ", glow " + (glowOk ? glow + " masks (mode " + T.pbGlowMode1567 + ")" : "OFF"); Debug.Log("[Hellord] " + PbArtStatus1567);
            }
            return _pbArt1567.TryGetValue(key, out var r) ? r : null;
        }
        static Texture2D PbTex1567(string f) { var t = Resources.Load<Texture2D>("HellordArt/" + PbArtRoot1567 + "/" + f); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        static Sprite[,] PbGrid1567(Texture2D t, int fw, int fh, int cols, int rows, float px, float py)
        {
            var g = new Sprite[rows, cols]; for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++) g[r, c] = Sprite.Create(t, new Rect(c * fw, t.height - (r + 1) * fh, fw, fh), new Vector2(px / fw, 1f - py / fh), HArt.PPU, 0, SpriteMeshType.FullRect); return g;
        }
        /// <summary>frame index for an age (s); -1 = a once-strip is finished</summary>
        static int PbAF1567(PbArtS a, float age, bool loop)
        {
            if (age < 0f) return -1; float t = age * 1000f; int n = a.n;
            if (a.ms.Length == 1) { int i = Mathf.FloorToInt(t / Mathf.Max(1f, a.ms[0])); return loop ? i % n : (i < n ? i : -1); }
            float tot = 0f; for (int i = 0; i < n && i < a.ms.Length; i++) tot += a.ms[i]; if (loop && tot > 0f) t %= tot;
            for (int i = 0; i < n && i < a.ms.Length; i++) { if (t < a.ms[i]) return i; t -= a.ms[i]; } return loop ? n - 1 : -1;
        }
        static int PbRow16(Vector2 v) { float a = Mathf.Atan2(-v.y, v.x) * Mathf.Rad2Deg; if (a < 0f) a += 360f; return Mathf.RoundToInt(a / 22.5f) & 15; }
        /// <summary>puts one frame (+ its glow twin, additive, one order above). False when the key has no art.</summary>
        bool PbArtPut1567(string key, int row, float age, bool loop, Vector2 p, int order, bool flip = false, float sc = 1f)
        {
            var a = PbA1567(key); if (a == null) return false; int fi = PbAF1567(a, age, loop); if (fi < 0) return true; row = Mathf.Clamp(row, 0, a.rows - 1);
            PbPut1567(a.f[row, fi], p, 0f, sc, sc, Color.white, order, flip);
            if (a.g != null && a.mat != null) PbPut1567(a.g[row, fi], p, 0f, sc, sc, Color.white, order + 1, flip, a.mat);
            return true;
        }
        /// <summary>puts a given frame index (code-stepped strips: crack stages, axe spin) with an alpha</summary>
        bool PbArtPutF1567(string key, int row, int fi, Vector2 p, int order, bool flip = false, float alpha = 1f, float sc = 1f)
        {
            var a = string.IsNullOrEmpty(key) ? null : PbA1567(key); if (a == null) return false; row = Mathf.Clamp(row, 0, a.rows - 1); fi = ((fi % a.n) + a.n) % a.n; var c = new Color(1f, 1f, 1f, alpha);
            PbPut1567(a.f[row, fi], p, 0f, sc, sc, c, order, flip); if (a.g != null && a.mat != null) PbPut1567(a.g[row, fi], p, 0f, sc, sc, c, order + 1, flip, a.mat);
            return true;
        }
        static float PbArtLen1567(string key) { var a = PbA1567(key); if (a == null) return 0f; float t = 0f; for (int i = 0; i < a.n; i++) t += a.ms[Mathf.Min(i, a.ms.Length - 1)]; return t / 1000f; }

        // one-shot strips (hits, fizzles, zaps, forks, trails...)
        class PbFx { public string key; public int row, order; public Vector2 p; public float t, sc = 1f; public bool flip; }
        readonly List<PbFx> _pbFx = new List<PbFx>();
        bool PbFxOnce1567(string key, Vector2 p, int order, int row = 0, float delayS = 0f, bool flip = false, float sc = 1f)
        {
            if (string.IsNullOrEmpty(key) || PbA1567(key) == null) return false; _pbFx.Add(new PbFx { key = key, p = p, order = order, row = row, t = -delayS, flip = flip, sc = sc }); return true;
        }
        void UpdateDrawPbFx1567(float dt, bool draw)
        {
            for (int i = _pbFx.Count - 1; i >= 0; i--)
            {
                var x = _pbFx[i]; x.t += dt; var a = PbA1567(x.key); if (a == null || (x.t >= 0f && PbAF1567(a, x.t, false) < 0)) { _pbFx.RemoveAt(i); continue; }
                if (draw && x.t >= 0f) PbArtPut1567(x.key, x.row, x.t, false, x.p, x.order, x.flip, x.sc);
            }
        }
        /// <summary>which art keys are loaded (for the report / test log)</summary>
        public static string PbArtKeys1567() { var sb = new System.Text.StringBuilder(); foreach (var sp in PbArtSpec1567) sb.Append((string)sp[0]).Append(PbA1567((string)sp[0]) != null ? "=ART " : "=missing "); return sb.ToString(); }
    }
}
namespace Hellord
{
    // t1567 HA hero frames: one pose set per skill (body / blink / walk / aura / spark, holds, release index) replacing the t1528 tail cast for that
    // skill. null set = the t1528 castB tail cast (Spark Spray / Ball Lightning recolour through castLightningIds1567). The set is picked on the cast.
    public partial class HellordGame
    {
        sealed class PoseSet1567 { public Sprite[] body, bodyOl, blink, walk, walkOl, aura, spark; public float[] holds; public int release; public bool noWalk, noSpark; }
        PoseSet1567 _pose1567; readonly Dictionary<string, PoseSet1567> _poseSets1567 = new Dictionary<string, PoseSet1567>();
        float[] PHolds1567 => _pose1567 != null ? _pose1567.holds : HellordTuning.I.castHoldMs1528;
        int PRel1567 => _pose1567 != null ? _pose1567.release : HellordTuning.I.castReleaseIdx1528;
        Sprite[] PBody1567 => _pose1567 != null ? _pose1567.body : _castBody1528;
        Sprite[] PBodyOl1567 => _pose1567 != null ? _pose1567.bodyOl : _castBodyOl1528;
        Sprite[] PBlink1567 => _pose1567 != null ? _pose1567.blink : _castBlink1528;
        Sprite[] PWalk1567 => _pose1567 != null ? (_pose1567.noWalk ? null : _pose1567.walk) : _castWalk1528;
        Sprite[] PWalkOl1567 => _pose1567 != null ? _pose1567.walkOl : _castWalkOl1528;
        /// <summary>the HA pose set for a skill (null = the default tail cast)</summary>
        PoseSet1567 PoseSetFor1567(SkillId id, bool catchPose = false)
        {
            var T = HellordTuning.I; if (!T.pbHeroPoses1567) return null;
            switch (id)
            {
                case SkillId.ThunderJavelin: return PoseSet1567Load("jav", HArt.Missing(Art47 + T.poseJavBody1567) ? T.castBodySheet1528 : T.poseJavBody1567, HArt.Missing(Art47 + T.poseJavBlink1567) ? T.castBlinkSheet1528 : T.poseJavBlink1567, T.castWalk1528 ? T.castWalkSheet1528 : null, T.castAuraLightningSheet1567, T.castSparkLightningSheet1567, T.poseJavHolds1567, T.poseJavRelease1567, false);
                case SkillId.HeadsmansChop: return PoseSet1567Load("chop", "hero_chop_body_36", "hero_chop_blink_36", "hero_chop_walk_36", "fx_chop_aura_blood_44", null, T.poseChopHolds1567, T.poseChopRelease1567, false);
                case SkillId.EarthSplittingSlam: return PoseSet1567Load("slam", "hero_slam_body_36", "hero_slam_blink_36", null, "fx_slam_aura_blood_44", null, T.poseSlamHolds1567, T.poseSlamRelease1567, true);
                case SkillId.TwinAxeThrow: return catchPose ? PoseSet1567Load("catch", "hero_throwcatch_body_36", "hero_throwcatch_blink_36", "hero_throwcatch_walk_36", "fx_throwcatch_aura_blood_44", null, T.poseCatchHolds1567, T.poseCatchRelease1567, false)
                                                          : PoseSet1567Load("throw", "hero_throw_body_36", "hero_throw_blink_36", "hero_throw_walk_36", "fx_throw_aura_blood_44", null, T.poseThrowHolds1567, T.poseThrowRelease1567, false);
            }
            return null;
        }
        PoseSet1567 PoseSet1567Load(string key, string body, string blink, string walk, string aura, string spark, float[] holds, int release, bool noWalk)
        {
            if (_poseSets1567.TryGetValue(key, out var ps)) return ps; var T = HellordTuning.I; int c = T.hero36CellPx; var pv = T.hero36PivotPx; int n = Mathf.Max(1, holds.Length);
            ps = null;
            if (!HArt.Missing(Art47 + body))
            {
                ps = new PoseSet1567 { holds = holds, release = Mathf.Clamp(release, 0, n - 1), noWalk = noWalk || walk == null, noSpark = spark == null };
                ps.body = HArt.Strip(Art47 + body, n, c, c, pv); ps.bodyOl = HArt.HeroOutline(Art47 + body, c, c, pv.x, pv.y, n);
                if (blink != null && !HArt.Missing(Art47 + blink)) ps.blink = HArt.Strip(Art47 + blink, 1, c, c, pv);
                if (walk != null && !HArt.Missing(Art47 + walk)) { int wn = 8 * (n + 1); ps.walk = HArt.Strip(Art47 + walk, wn, c, c, pv); ps.walkOl = HArt.HeroOutline(Art47 + walk, c, c, pv.x, pv.y, wn); }
                if (aura != null && !HArt.Missing(Art47 + aura)) { var t = Resources.Load<Texture2D>("HellordArt/" + Art47 + aura); int an = t != null ? Mathf.Max(1, t.width / 44) : 4; ps.aura = HArt.Strip(Art47 + aura, an, 44, 44, T.castAuraPivotPx1528); }
                if (spark != null && !HArt.Missing(Art47 + spark)) ps.spark = HArt.Strip(Art47 + spark, 3, 13, 13, new Vector2(6.5f, 6.5f));
            }
            _poseSets1567[key] = ps; return ps;
        }
        /// <summary>Twin Axe catch: play the 2-frame catch pose from its first frame</summary>
        void HeroCatchPose1567(SkillInst s)
        {
            var T = HellordTuning.I; if (!T.heroPose1517) return; var ps = PoseSetFor1567(SkillId.TwinAxeThrow, true); if (ps == null) return;
            _pose1567 = ps; _poseLight1567 = false; _poseFrost1528 = false; _poseKind1517 = AttackPoseKind1517.Cast; _poseT1517 = 0f; _preWind1528 = false; _poseCalls1517++; CatchPoses1567++;
        }
        public int CatchPoses1567;

        // ---------------------------------------------------------------- held axe overlay (HA hero_axe_held_overlay_36, 16 f = base frames 0-15)
        SpriteRenderer _axeOv1567; Sprite[] _axeOvF1567; bool _axeOvTried1567;
        bool AxeBasic1567(SkillId id) => id == SkillId.HeadsmansChop || id == SkillId.EarthSplittingSlam || id == SkillId.TwinAxeThrow || id == SkillId.PhantomAxes;
        void AxeOverlay1567(int f, bool flip, int ord, bool pose)
        {
            var T = HellordTuning.I; var b = _basic1517; bool on = T.axeHeldOverlay1567 && Hero47On && b != null && AxeBasic1567(b.def.id) && !pose && _pbAxe == null;
            if (on && !_axeOvTried1567) { _axeOvTried1567 = true; string n = Art47 + "hero_axe_held_overlay_36"; if (!HArt.Missing(n)) _axeOvF1567 = HArt.Strip(n, 16, T.hero36CellPx, T.hero36CellPx, T.hero36PivotPx); }
            if (on && _axeOvF1567 != null && _axeOv1567 == null && _hero != null) { _axeOv1567 = new GameObject("axeHeld1567").AddComponent<SpriteRenderer>(); _axeOv1567.transform.SetParent(_hero, false); if (_heroBase != null) _axeOv1567.sortingLayerID = _heroBase.sortingLayerID; }
            if (_axeOv1567 == null) return; on = on && _axeOvF1567 != null && f >= 0 && f < _axeOvF1567.Length; _axeOv1567.enabled = on;
            if (on) { _axeOv1567.sprite = _axeOvF1567[f]; _axeOv1567.flipX = flip; _axeOv1567.sortingOrder = ord + 1; }
        }
    }
}
