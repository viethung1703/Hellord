using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hellord
{
    // t982u v0.0.88: pipeline-agnostic hero size probe. Renders the gameplay camera into its low-res RT twice in the same frame
    // (hero shown / hero hidden) and measures the bbox of the pixels the hero actually changed. Works the same under Built-in and
    // URP 2D, so it catches anything that DRAWS the hero bigger (shader UVs, wrong texel size, camera, RT) - not just transform bounds.
    public partial class HellordGame
    {
        // t982u v0.0.88: the ONLY code path found that draws the hero bigger on screen (render probe: all skills / i-frames / URP 2D = same px): the t636u test wheel-zoom (camScrollZoom636) was live while the Manual belt "PICK A SLOT" prompt is up
        // (mode stays Play there). That prompt only appears from the 2nd skill on, so scrolling to choose a slot zoomed the whole view in:
        // hero (and world) drawn bigger, nothing else changed. The wheel is now ignored there; middle-click resets; every change toasts + logs.
        public int wheelBlocked988; internal float? atWheel988; // autotest: fake wheel delta for one frame
        float Wheel988(UnityEngine.InputSystem.Mouse ms) { if (atWheel988.HasValue) { float v = atWheel988.Value; atWheel988 = null; return v; } return ms != null ? ms.scroll.ReadValue().y : 0f; }
        bool WheelBlocked988() => _slotPick716 != null || _ctrlOpen716 || _tmOpen || _tp != null || MouseOverBelt716();
        // Runtime watch (all builds): logs once whenever the hero's on-screen pixel scale changes, with the cause, so a Console line proves it.
        float _scrK988 = -1; int _scrW988, _scrH988;
        void HeroScaleWatch988()
        {
            if (_cam == null || _heroBase == null) return;
            float k = ViewPxH / (2f * _cam.orthographicSize) / HArt.PPU * _heroBase.transform.lossyScale.y;
            if (_scrK988 > 0 && Mathf.Abs(k - _scrK988) > 0.01f)
                Debug.Log($"[Hellord] t982u hero screen scale {_scrK988:0.00} -> {k:0.00} px/art-px | wheelStep {_camStep636} rt {(_rt ? _rt.width + "x" + _rt.height : "-")} win {_scrW988}x{_scrH988} -> {Screen.width}x{Screen.height} ortho {_cam.orthographicSize:0.000} heroScale {_heroBase.transform.lossyScale.y:0.00} layers {_heroLayers.Count} iframe {_iframe > 0}");
            _scrK988 = k; _scrW988 = Screen.width; _scrH988 = Screen.height;
        }
#if HELLORD_AUTOTEST
        static readonly bool AtIfLegacy988 = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-atIfLegacy") >= 0;
        Texture2D _probe988;
        Color32[] Grab988()
        {
            _cam.Render(); var prev = RenderTexture.active; RenderTexture.active = _rt;
            if (_probe988 == null || _probe988.width != _rt.width || _probe988.height != _rt.height) _probe988 = new Texture2D(_rt.width, _rt.height, TextureFormat.RGBA32, false);
            _probe988.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0); _probe988.Apply(false); RenderTexture.active = prev; return _probe988.GetPixels32();
        }
        /// <summary>Drawn hero bbox in RT px (w,h). onlyBase = hide trait layers too. png = save the crop.</summary>
        RectInt HeroDrawn988(bool onlyBase, string png = null)
        {
            if (_rt == null) return new RectInt();
            var all = _hero.GetComponentsInChildren<SpriteRenderer>(); var was = new bool[all.Length];
            var layerSet = new HashSet<SpriteRenderer>(); foreach (var l in _heroLayers) if (l.sr) layerSet.Add(l.sr);
            for (int i = 0; i < all.Length; i++) { was[i] = all[i].enabled; if (onlyBase && layerSet.Contains(all[i])) all[i].enabled = false; }
            var a = Grab988();
            for (int i = 0; i < all.Length; i++) all[i].enabled = false;
            var b = Grab988();
            for (int i = 0; i < all.Length; i++) all[i].enabled = was[i];
            int W = _rt.width, H = _rt.height, x0 = W, y0 = H, x1 = -1, y1 = -1;
            var hs = _cam.WorldToViewportPoint(_hero.position); int hx = Mathf.RoundToInt(hs.x * W), hy = Mathf.RoundToInt(hs.y * H);
            for (int y = Mathf.Max(0, hy - 24); y < Mathf.Min(H, hy + 56); y++) for (int x = Mathf.Max(0, hx - 40); x < Mathf.Min(W, hx + 40); x++)
            {
                var p = a[y * W + x]; var q = b[y * W + x];
                if (Mathf.Abs(p.r - q.r) + Mathf.Abs(p.g - q.g) + Mathf.Abs(p.b - q.b) > 6) { if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y; }
            }
            if (x1 < 0) return new RectInt();
            if (png != null)
            {
                int cx = Mathf.Max(0, x0 - 8), cy = Mathf.Max(0, y0 - 8), cw = Mathf.Min(W - cx, x1 - x0 + 17), ch = Mathf.Min(H - cy, y1 - y0 + 17);
                var t = new Texture2D(cw, ch, TextureFormat.RGBA32, false); var px = new Color32[cw * ch];
                for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++) px[y * cw + x] = a[(cy + y) * W + cx + x];
                t.SetPixels32(px); t.Apply(); File.WriteAllBytes(png, t.EncodeToPNG()); Destroy(t);
            }
            return new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }
        static string R988(RectInt r) => r.width + "x" + r.height;
        System.Collections.IEnumerator AtSize988()
        {
            string clip = "size988_all_skills"; yield return AtBegin(clip); LoadCtrl716(); bool oMan = Manual716; Manual716 = true;
            string dir = Path.Combine(_atOut, "size988"); Directory.CreateDirectory(dir);
            AtClearWorld(); AtSetSkills(); AtHeroTo(RoomCenterFeet()); SnapCamera(); maxHp = hp = 100000; _atGod = false; _iframe = 0;
            yield return AtWait(0.4f);
            var rows = new List<SkillId[]>();
            foreach (SkillId s in System.Enum.GetValues(typeof(SkillId))) rows.Add(new[] { s });
            rows.Add(new[] { SkillId.SkeletonArchers, SkillId.SoulSpears }); rows.Add(new[] { SkillId.SoulSpears, SkillId.SkeletonArchers });
            rows.Add(new[] { SkillId.Fireball, SkillId.HellfirePillar }); rows.Add(new[] { SkillId.FrostCleave, SkillId.StormRing });
            rows.Add(new[] { SkillId.Fireball, SkillId.FrostCleave, SkillId.StormRing, SkillId.SoulSpears, SkillId.ChainLightning });
            RectInt base0 = HeroDrawn988(true, Path.Combine(dir, "start.png")); var all0 = HeroDrawn988(false);
            ALog($"size988 start base {R988(base0)} all {R988(all0)} ortho {_cam.orthographicSize:0.000} rt {_rt.width}x{_rt.height} pipeline {(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline ? UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name : "Built-in")}");
            int bad = 0; var sb = new System.Text.StringBuilder();
            foreach (var row in rows)
            {
                AtClearWorld(); AtSetSkills(); _iframe = 0; yield return AtWait(0.15f);
                foreach (var id in row) { mode = Mode.LevelUp; Choose(new Card { kind = CardKind.NewSkill, skill = HData.Skill(id) }); if (_slotPick716 != null) { for (int i = 0; i < BeltN716; i++) if (_slot716[i] == null) { PutSlot716(i); break; } } mode = Mode.Play; yield return AtWait(0.1f); }
                AtClearWorld(); _iframe = 0; yield return AtWait(0.2f);
                string nm = string.Join("+", row); float orthoN = _cam.orthographicSize;
                var bN = HeroDrawn988(true); var aN = HeroDrawn988(false, Path.Combine(dir, nm + "_normal.png"));
                HurtHero(5f, "at_size988"); yield return null; yield return null; yield return null;
                if (!IfMatOn987(_heroBase)) { _iframe = HeroIFrame; yield return null; yield return null; }
                bool ifOn = IfMatOn987(_heroBase);
                var bI = HeroDrawn988(true); var aI = HeroDrawn988(false, Path.Combine(dir, nm + "_iframe.png"));
                // same frame, i-frame materials temporarily removed -> exact A/B of the shader alone
                IfSet969(_heroBase, false); foreach (var l in _heroLayers) IfSet969(l.sr, false); var aIN = HeroDrawn988(false); IfSet969(_heroBase, true); foreach (var l in _heroLayers) IfSet969(l.sr, true);
                bool ok = Mathf.Abs(bN.width - base0.width) <= 3 && Mathf.Abs(bN.height - base0.height) <= 2 && Mathf.Abs(bI.width - bN.width) <= 3 && Mathf.Abs(bI.height - bN.height) <= 2
                          && Mathf.Abs(aI.height - aIN.height) <= 2 /* all-layer WIDTH is info only: the hit's damage number sits beside the trait layers and leaks into the diff */ && Mathf.Abs(orthoN - _cam.orthographicSize) < 1e-4f && ifOn;
                if (!ok) bad++;
                string line = $"{nm,-60} base {R988(bN)} all {R988(aN)} | iframe base {R988(bI)} all {R988(aI)} (same frame no-shader {R988(aIN)}) layers {_heroLayers.Count} ortho {orthoN:0.00}/{_cam.orthographicSize:0.00} if {ifOn} {(ok ? "OK" : "BAD")}";
                ALog("size988 " + line); sb.AppendLine(line);
                _iframe = 0; yield return AtWait(0.9f);
            }
            // wheel during the 2nd-skill "PICK A SLOT" prompt (the VH repro) vs plain play
            {
                var T = HellordTuning.I; bool oZ = T.camScrollZoom636; T.camScrollZoom636 = true; _camStep636 = -1;
                AtClearWorld(); AtSetSkills(); _iframe = 0; yield return AtWait(0.2f);
                mode = Mode.LevelUp; Choose(new Card { kind = CardKind.NewSkill, skill = HData.Skill(SkillId.SkeletonArchers) }); mode = Mode.Play; if (_slotPick716 != null) PutSlot716(0);
                mode = Mode.LevelUp; Choose(new Card { kind = CardKind.NewSkill, skill = HData.Skill(SkillId.SoulSpears) }); mode = Mode.Play;
                bool pick = _slotPick716 != null; yield return AtWait(0.1f);
                float k0 = RtSY636; var h0 = HeroDrawn988(true); int wb0 = wheelBlocked988;
                for (int i = 0; i < 3; i++) { atWheel988 = 120f; yield return null; }
                float k1 = RtSY636; var h1 = HeroDrawn988(true, Path.Combine(dir, "wheel_during_slotpick.png"));
                ARes("size988:wheel_blocked_in_slot_pick", pick && _camStep636 < 0 && Mathf.Abs(k1 - k0) < 1e-3f && wheelBlocked988 - wb0 >= 3, $"slotPick {pick} step {_camStep636} screen px/rt px {k0:0.00}->{k1:0.00} hero rt {R988(h0)}->{R988(h1)} blocked {wheelBlocked988 - wb0}");
                if (_slotPick716 != null) PutSlot716(1); yield return AtWait(0.1f);
                for (int i = 0; i < 2; i++) { atWheel988 = 120f; yield return null; yield return null; }
                float k2 = RtSY636; var h2 = HeroDrawn988(true, Path.Combine(dir, "wheel_in_play.png"));
                ALog($"size988 wheel in plain play (still allowed, test feature): step {_camStep636} rt {_rt.width}x{_rt.height} hero on screen {h0.height * k0:0}px -> {h2.height * k2:0}px (x{k2 / k0:0.00})");
                ARes("size988:wheel_in_play_is_the_grow", _camStep636 >= 0 && k2 > k0 * 1.05f, $"plain-play wheel zooms the view: hero screen h {h0.height * k0:0}px -> {h2.height * k2:0}px");
                _camStep636 = -1; T.camScrollZoom636 = oZ; yield return AtWait(0.2f);
            }
            File.WriteAllText(Path.Combine(dir, "table.txt"), sb.ToString());
            ARes("size988:drawn_hero_px", bad == 0, $"rows {rows.Count} bad {bad} start base {R988(base0)}");
            _atGod = true; _iframe = 0; AtSetSkills(); Manual716 = oMan; AtEnd(clip);
        }
#endif
    }
}
