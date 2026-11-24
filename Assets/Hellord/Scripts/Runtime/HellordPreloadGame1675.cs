using System.Collections;
using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    /// <summary>t1675 run loading phase (VH t1675u): Descend (pact sealed) -> LOADING screen (bar) -> the run.
    /// While loading, HellordGame.Update does no gameplay and OnGUI draws only this screen; the run (start pick / chosen basic) is already set up
    /// underneath and starts the frame loading ends. The work itself is the HellordPreload plan (HellordPreload1675.cs); the providers below are
    /// the systems registering with it (each uses that system's own lazy getter, so its cache and the first-use fallback stay as they were).
    /// t1676: also the per-enemy hit / death / corpse art + blood FX, Pb hero pose sets (outline generation), FX934 pool + room light materials.
    /// Loading screen = PLACEHOLDER (existing 5x7 HUD font + pact panel colours, no new art).</summary>
    public partial class HellordGame
    {
        Coroutine _plCo1675; bool _plReg1675; int _plPact1675 = -1;
        /// <summary>True while the loading screen is up (the run is held).</summary>
        public bool Loading1675 => _plCo1675 != null;
        public int PreloadRuns1675 => HellordPreload.Runs;

        /// <summary>Called by every run start from the menu (Descend51 / DfStartRun1555 / DfStartRun1559). Off (preload1675) = the old lazy loads.</summary>
        void PreloadBegin1675(int pact)
        {
            var T = HellordTuning.I; if (T == null || !T.preload1675) return;
            PreloadRegister1675(); PreloadStop1675();
            _plPact1675 = pact; var plan = HellordPreload.Plan(pact);
            _plCo1675 = StartCoroutine(PreloadCo1675(plan, T.preloadBudgetMs1675, T.preloadWarmShaders1675, T.preloadMinSec1675));
        }
        IEnumerator PreloadCo1675(HellordPreload.Ctx plan, float budget, bool warm, float minSec)
        {
            yield return HellordPreload.Run(plan, budget, warm, minSec);
            _plCo1675 = null; _mouseT = -99; // the run starts this frame (no click carried over from the loading screen)
        }
        /// <summary>Finish loading now (same plan, synchronous; tests / tools that need the run ready this frame).</summary>
        public void PreloadFinishNow1675()
        {
            if (_plCo1675 == null) return; PreloadStop1675();
            var T = HellordTuning.I; HellordPreload.RunNow(HellordPreload.Plan(_plPact1675), T == null || T.preloadWarmShaders1675);
        }
        void PreloadStop1675() { if (_plCo1675 != null) { StopCoroutine(_plCo1675); _plCo1675 = null; HellordPreload.Cancel(); } }

        // ---------------------------------------------------------------- providers (one per system)
        void PreloadRegister1675()
        {
            if (_plReg1675) return; _plReg1675 = true;
            HellordPreload.Register("pact basics", PlPactBasics1675);   // Pb strips / glow mats / icons / hero pose sets (+ outlines), Leizu + Zhurong basic icons
            HellordPreload.Register("LeizuFX", PlLeizu1675);            // Leizu kit: LZFX materials, Thunder Hound sheets, kit icons
            HellordPreload.Register("ZhurongFX", PlZhurong1675);        // Zhurong kit: ZFX materials, kit icons, fire basics strips
            HellordPreload.Register("shared FX", PlSharedFx1675);       // Fire / Frost / Nova / PixelAoE / FX934 / boss / room light, pools
            HellordPreload.Register("enemy feel", PlEnemyFeel1676);     // t1676: hit / death / corpse / blood art per enemy sheet
            HellordPreload.Register("hit + hero feedback", PlFeedback1676); // t1676: damage numbers, flash / burn / i-frame materials, drops, cast poses, generated FX sprites
            HellordPreload.Register("audio", PlAudio1675);              // SFX clips: LoadAudioData
            HellordPreload.Register("shaders", PlShaders1675);          // every Resources/HellordShaders shader (temp material) for the warmup
        }

        static string Snake1675(string s) { var b = new System.Text.StringBuilder(); for (int i = 0; i < s.Length; i++) { char ch = s[i]; if (char.IsUpper(ch) && i > 0) b.Append('_'); b.Append(char.ToLowerInvariant(ch)); } return b.ToString(); }
        static string PbFolder1675(string file) { var parts = file.Trim('/').Split('/'); return parts.Length >= 2 ? parts[parts.Length - 2] : ""; }

        void PlPactBasics1675(HellordPreload.Ctx c)
        {
            var raw = DfBasicsRaw1555(c.pact); if (raw == null) return; var ids = new List<SkillId>(raw);
            var folders = new HashSet<string>(); foreach (var id in ids) folders.Add(Snake1675(id.ToString()));
            foreach (var id in ids)
            {
                var sid = id;
                c.Step("icon " + sid, () => { PbIcon1567(sid); ZhCardIcon1625(sid); });
                c.Step("pose " + sid, () => { PoseSetFor1567(sid); if (sid == SkillId.TwinAxeThrow) PoseSetFor1567(sid, true); });
            }
            foreach (var sp in PbArtSpec1567)
            {
                string key = (string)sp[0], file = (string)sp[1]; if (!folders.Contains(PbFolder1675(file))) continue;
                c.Step("strip " + key, () => { var a = PbA1567(key); if (a != null) c.Warm(a.mat); c.Warm(PbBaseMat1567(key)); });
            }
        }
        void PlLeizu1675(HellordPreload.Ctx c)
        {
            if (c.pact != 2) return; var T = TZ;
            c.Step("LZFX materials", () =>
            {
                string dir = T != null && !string.IsNullOrEmpty(T.lzFxDir1669) ? T.lzFxDir1669 : "LeizuFX/Materials/";
                foreach (var m in Resources.LoadAll<Material>(dir.TrimEnd('/'))) { var got = LzFxMat1666(m.name); if (got != null) c.Warm(got); }
            });
            c.Step("Thunder Hound sheets", () => LzHoundAnim1666("idle", "south"));
            c.Step("kit icons", () => { foreach (var k in LzKeys1666) LzIcon1666(k); });
        }
        void PlZhurong1675(HellordPreload.Ctx c)
        {
            if (c.pact != 0) return;
            c.Step("ZFX materials", () => { foreach (var m in Resources.LoadAll<Material>("ZhurongFX/Materials")) { var got = ZhFxMat1625(m.name); if (got != null) c.Warm(got); } });
            c.Step("kit icons", () => { foreach (var k in ZhKeys1618) ZhIcon1618(k); ZhIcon1618("firewall"); ZhIcon1618("hydra"); });
        }
        void PlSharedFx1675(HellordPreload.Ctx c)
        {
            var T = HellordTuning.I;
            c.Step("fire basics strips", () => { if (FireBasicsFX1481.Ok) { } });
            c.Step("frostbolt strips", () => { if (FrostboltFX1432.Ok) { } });
            c.Step("nova strips", () => { if (NovaFX1419.Ok) { } });
            c.Step("frost cleave", () => { if (FrostCleave990.Ok) { } });
            c.Step("pixel AoE 928/930", () => { if (!_sfxTried928) LoadShaderFx928(); });
            c.Step("FX934 pool", () => { Fx934Prewarm1675(8); foreach (var m in Fx934Mats1675()) c.Warm(m); });
            c.Step("fire wall materials", () => { c.Warm(FireWallMat948()); if (_fwEmbMat957 == null) { var sh = Shader.Find("Sprites/Default"); if (sh != null) _fwEmbMat957 = new Material(sh) { name = "fx957_ember" }; } });
            c.Step("Pb hit / heat / disc materials", () => { c.Warm(PbHitMat1567()); c.Warm(PbHeatMat1567()); c.Warm(PbDiscMat1567()); });
            c.Step("boss shader materials", () => { if (Bs950Ok) { } });
            c.Step("room light materials", () => { c.Warm(HellordRoom.LightMatPublic(true)); c.Warm(HellordRoom.LightMatPublic(false)); });
            c.Step("skill light pool", () => SkillLightRunner.I.Prewarm(128));
            c.Step("FX quad pool", () => ZhFxPoolPrewarm1675(T != null ? T.preloadQuadPool1675 : 128));
            c.Step("kit FX warm list", () => { foreach (var m in LzFxMats1675()) c.Warm(m); foreach (var m in ZhFxMats1675()) c.Warm(m); }); // anything a previous run already loaded
        }
        void PlEnemyFeel1676(HellordPreload.Ctx c)
        {
            var T = HellordTuning.I; if (T == null || !T.d2Hooks1434) return;
            c.Step("enemy hit / death art", () =>
            {
                var sheets = new HashSet<string> { "enemy_bloater", "enemy_rice_ghost", "enemy_grinder_monk", "enemy_frost_caster", "enemy_fire_wheel", "enemy_imp_v2_k1", "enemy_imp_v2_k2", "enemy_imp_v2_k3", "enemy_imp_v2_k7",
                    "enemy_wraith_v2_k4", "enemy_wraith_v2_k5", "enemy_wraith_v2", "enemy_frost_brute", "enemy_archer" }; // the D2Sheet variants
                foreach (var kv in EnemyArtMap.Map) { sheets.Add(kv.Value.sheet); foreach (int v in new[] { 1, 2 }) { var s = VarSheet1457(kv.Key, v); if (s != null) sheets.Add(s); } }
                D2Feel1434.Preload1676(sheets);
            });
        }
        /// <summary>t1676: things the ftime1675 asset log showed being built on the first hit / dash / kill / cast (not pact-specific).</summary>
        void PlFeedback1676(HellordPreload.Ctx c)
        {
            c.Step("damage numbers + hit flash", () => { HordeFX.Prewarm(24); c.Warm(HordeMaterials.Flash); c.Warm(HordeMaterials.CritFlash); });
            c.Step("burn FX", InitBurn);
            c.Step("i-frame materials", () => IfPreload1676());
            c.Step("cast poses + cooldown ring", () => { PoseLoad1517(); RingTex1517(); });
            c.Step("drops + projectiles", () => { for (int t = 0; t < 3; t++) HArt.Blood(t); HArt.Bone(true); HArt.Bone(false); foreach (var p in new[] { "proj_fireball", "proj_frost", "proj_storm", "proj_bone", "proj_curse" }) HArt.Proj(p); });
            c.Step("generated FX sprites", () =>
            {   // the radii the kit / basics / tells ask for (others still build on first use, cached)
                foreach (int r in new[] { 4, 8, 13, 18, 24, 40 }) { HellSprites.Disk(r); for (int f = 0; f < 3; f++) HellSprites.FireRing(r, f); }
                foreach (var g in new[] { new Vector2Int(3, 2), new Vector2Int(4, 5), new Vector2Int(5, 4), new Vector2Int(18, 5), new Vector2Int(40, 5) }) PixelFXTextures.Glow(g.x, g.y);
                HellordPreloadTouch1676(HellSprites.Pixel(), HArt.Pixel(), PbDot, PbLineS, PbDisc, PbAxeSpr, PbSoft1567, PbRingSoft1567); // Pb code-drawn sprites (first Spark Spray / Ball Lightning)
            });
        }
        static void HellordPreloadTouch1676(params Sprite[] s) { } // the getters above build + cache; nothing to do with the result
        void PlAudio1675(HellordPreload.Ctx c)
        {
            var T = HellordTuning.I; if (T == null || T.preloadAudioFolders1675 == null || T.preloadAudioFolders1675.Length == 0) return;
            HellordAudio.I.Clips1675(new HashSet<string>(T.preloadAudioFolders1675), c.Audio);
            c.Audio(Resources.Load<AudioClip>("Audio/SFX/firebasics1481/magma_rock_roll_loop")); // LoopStart clip (Rolling Magma Rock)
        }
        void PlShaders1675(HellordPreload.Ctx c)
        {
            foreach (var s in Resources.LoadAll<Shader>("HellordShaders")) c.WarmShader(s);
        }

        // ---------------------------------------------------------------- loading screen (PLACEHOLDER: HUD 5x7 font, pact panel colours)
        void DrawPreload1675()
        {
            if (Event.current.type != EventType.Repaint) return;
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity; GUI.color = HInk; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; GUI.matrix = m;
            float p = Mathf.Clamp01(HellordPreload.Progress); const int W = 280, H = 70; int x = (HW - W) / 2, y = (HH - H) / 2;
            Frame51(x, y, W, H, CPanel51, CEdge51);
            string pact = _plPact1675 >= 0 && _plPact1675 < PactData.PathShort.Length ? PactData.PathShort[_plPact1675] : "";
            string t1 = "DESCENDING" + (pact.Length > 0 ? " - " + pact : ""); Text(t1, x + (W - TextW(t1)) / 2, y + 10, HArt.OutlineCol);
            int bx = x + 20, by = y + 30, bw = W - 40; Rect51(bx - 1, by - 1, bw + 2, 10, CEdge51); Rect51(bx, by, bw, 8, HInk); Rect51(bx, by, Mathf.Round(bw * p), 8, HCyan);
            string t2 = (string.IsNullOrEmpty(HellordPreload.Phase) ? "LOADING" : HellordPreload.Phase) + " " + Mathf.RoundToInt(p * 100f) + "%"; Text(t2, x + (W - TextW(t2)) / 2, y + 48, Color.white);
        }
    }
}
