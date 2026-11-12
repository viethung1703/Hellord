using System.Collections.Generic;
using UnityEngine;

// v0.0.47: approved 36x36 hero (step31 hero36_final), v14 trait overlays (traits36_v14, anchors_36_v14.json) and the
// step32 chill / freeze art (hero_chill36.json). Art lives in Resources/HellordArt/Hero36. Toggle: HellordTuning.hero36V47.
namespace Hellord
{
    public partial class HellordGame
    {
        static HellordTuning T47 => HellordTuning.I;
        const string Art47 = "Hero36/";
        bool Hero47On => T47.hero36V47 && _tf == 0;
        bool Chill47On => Hero47On && T47.chill36V47;
        /// <summary>Hero-side hit padding multiplier (36 px hero = bigger body).</summary>
        float HeroHitK47 => T47.hero36V47 ? T47.heroHitScale47 : 1f;
        SpriteRenderer _glass47;
        readonly Dictionary<string, int> _sort47 = new Dictionary<string, int>();
        Sprite[] _hero36F;
        bool _combo47, _tall47, _layers47;

        Sprite[] Hero36Frames()
        {
            if (_hero36F == null) { var T = T47; _hero36F = HArt.Strip(Art47 + "hero_36_base", T.hero36Frames, T.hero36CellPx, T.hero36CellPx, T.hero36PivotPx); }
            return _hero36F;
        }
        Sprite[] Over47(string n, int frames) { var T = T47; return HArt.Strip(Art47 + n, frames, T.trait36CellPx.x, T.trait36CellPx.y, T.trait36PivotPx); }
        static bool Has47(string n) => !HArt.Missing(Art47 + n);

        /// <summary>Adds the v14 trait strips for owned skills (supports get none). Called from SetHeroLayers.</summary>
        void AddTraits47()
        {
            var T = T47; _sort47.Clear();
            var own = new HashSet<string>(); foreach (var s in skills) own.Add(s.def.id.ToString());
            _combo47 = own.Contains("Fireball") && own.Contains("HellfirePillar");
            _tall47 = false; foreach (var t in T.chill36TallTraits) if (own.Contains(t)) _tall47 = true;
            if (T.traits36V47)
            {
            void Add(string strip, int sort)
            {
                string n = "hero_36_t_" + strip; if (!Has47(n)) return;
                var f = Over47(n, T.trait36Frames); var sr = NewSR(n, _hero, f[0], 0); _heroLayers.Add((sr, f)); _sort47[n] = sort;
            }
            for (int i = 0; i < T.trait36BehindOrder.Length; i++)
            {
                string t = T.trait36BehindOrder[i]; if (!own.Contains(t)) continue;
                if (t == "HellfirePillar" && _combo47) Add("HellfirePillar_combo_behind", T.trait36BehindSort + i);
                else Add(t + "_behind", T.trait36BehindSort + i);
            }
            for (int i = 0; i < T.trait36FrontOrder.Length; i++)
            {
                string t = T.trait36FrontOrder[i]; if (!own.Contains(t)) continue;
                if (t == "HellfirePillar" && _combo47) continue;
                Add(t + "_front", T.trait36FrontSort + i * T.trait36FrontSortStep);
            }
            }
            _glass47 = null;
            if (_glassOn && T.glassHorns36V47 && Has47("hero_36_glass_horns"))
            {
                int bi = System.Array.IndexOf(T.trait36FrontOrder, T.glassHorns36Below);
                int gs = bi < 0 ? T.trait36FrontSort + T.trait36FrontOrder.Length * T.trait36FrontSortStep : T.trait36FrontSort + bi * T.trait36FrontSortStep - 1;
                var f = Over47("hero_36_glass_horns", T.trait36Frames); _glass47 = NewSR("hero_36_glass_horns", _hero, f[0], 0); _heroLayers.Add((_glass47, f)); _sort47["hero_36_glass_horns"] = gs;
            }
        }
        int LayerSort47(string n) => HArt.HeroLayerSort.TryGetValue(n, out var so) ? so : _sort47.TryGetValue(n, out var s2) ? s2 : 1;

        // ---------------- chill / freeze ----------------
        SpriteRenderer _edge47, _frz47, _bar47; readonly SpriteRenderer[] _pip47 = new SpriteRenderer[3];
        Sprite[] _edge1F, _edge2F, _inF, _shF, _brF, _pipF;
        bool _wasFrozen47; float _frzT47, _brkT47 = 99f; int _lastStacks47; float _popT47; int _popIdx47 = -1;

        void BuildChill47()
        {
            if (_edge47 != null || !_hero) return; var T = T47; int c = T.chill36EdgeCellPx;
            _edge1F = HArt.Strip(Art47 + "hero_36_chill1_edge", T.hero36Frames, c, c, T.hero36PivotPx);
            _edge2F = HArt.Strip(Art47 + "hero_36_chill2_edge", T.hero36Frames, c, c, T.hero36PivotPx);
            _inF = Over47("hero_36_frozen_in", T.chill36FreezeInMs.Length);
            _shF = Over47("hero_36_frozen_shimmer", T.chill36ShimmerMs.Length);
            _brF = Over47("hero_36_frozen_break", T.chill36BreakMs.Length);
            int pc = T.chill36PipCellPx; _pipF = HArt.Strip(Art47 + "hero_36_chill_pips", T.chill36PipFrames, pc, pc, new Vector2(pc * 0.5f, pc * 0.5f));
            _edge47 = NewSR("chill36_edge", _hero, null, 0); _frz47 = NewSR("chill36_frozen", _hero, null, 0);
            int bw = T.chill36BarPx.x, bh = T.chill36BarPx.y; var tex = new Texture2D(bw, bh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "chill36_bar" };
            var px = new Color32[bw * bh]; Color32 bc = T.chill36BarColor;
            for (int y = 0; y < bh; y++) for (int x = 0; x < bw; x++) { bool corner = (x == 0 || x == bw - 1) && (y == 0 || y == bh - 1); px[y * bw + x] = corner ? new Color32(0, 0, 0, 0) : bc; }
            tex.SetPixels32(px); tex.Apply(false, false);
            _bar47 = NewSR("chill36_bar", _hero, Sprite.Create(tex, new Rect(0, 0, bw, bh), new Vector2(0.5f, 0f), HArt.PPU, 0, SpriteMeshType.FullRect), 0);
            for (int i = 0; i < 3; i++) _pip47[i] = NewSR("chill36_pip" + i, _hero, null, 0);
        }
        static int StepFrame47(int[] ms, float t, bool loop, out bool done)
        {
            int total = 0; foreach (var d in ms) total += d; int m = (int)(t * 1000f); done = !loop && m >= total;
            if (total <= 0) return 0; if (loop) m %= total; else if (m >= total) return ms.Length - 1;
            for (int i = 0; i < ms.Length; i++) { if (m < ms[i]) return i; m -= ms[i]; }
            return ms.Length - 1;
        }
        /// <summary>True while the hero and traits must hold frame 0 (frozen, or break-out cracks still on the block).</summary>
        bool HoldFrame47 => Chill47On && (HeroFrozen44 || _brkT47 * 1000f < BreakFreeMs47());
        float BreakFreeMs47() { float s = 0; var ms = T47.chill36BreakMs; for (int i = 0; i < Mathf.Min(T47.chill36BreakFreeFrame, ms.Length); i++) s += ms[i]; return s; }

        /// <summary>Per frame after the hero sprites are set: chill tint, edge overlay, frozen block, pips. f = hero frame, ord = hero sort.</summary>
        void UpdateChill47(float dt, int f, bool flip, int ord)
        {
            if (!Chill47On) { if (_glass47) _glass47.enabled = true; if (_edge47) { _edge47.enabled = _frz47.enabled = _bar47.enabled = false; foreach (var p in _pip47) p.enabled = false; } return; }
            BuildChill47(); var T = T47; bool frozen = HeroFrozen44;
            if (frozen && !_wasFrozen47) { _frzT47 = 0; _brkT47 = 99f; }
            if (!frozen && _wasFrozen47) _brkT47 = 0;
            _wasFrozen47 = frozen; _frzT47 += dt; _brkT47 += dt;
            int st = frozen ? 3 : Mathf.Clamp(_chill44, 0, 3);
            float amt = T.chill36TintByStack != null && T.chill36TintByStack.Length > st ? T.chill36TintByStack[st] : 0f;
            if (amt > 0f && !(_hurtT > 0 && !T.fxHeroHurt))
            {
                var c = Color.Lerp(Color.white, T.chill36Tint, amt); _heroBase.color = c; foreach (var l in _heroLayers) l.sr.color = c;
            }
            else foreach (var l in _heroLayers) l.sr.color = Color.white;
            var ef = st >= 2 ? _edge2F : st == 1 ? _edge1F : null;
            _edge47.enabled = ef != null && !frozen; if (_edge47.enabled) { _edge47.sprite = ef[Mathf.Clamp(f, 0, ef.Length - 1)]; _edge47.flipX = flip; _edge47.sortingOrder = ord + T.chill36EdgeSort; }
            Sprite fs = null;
            if (frozen)
            {
                float inS = 0; foreach (var d in T.chill36FreezeInMs) inS += d / 1000f;
                fs = _frzT47 < inS ? _inF[StepFrame47(T.chill36FreezeInMs, _frzT47, false, out _)] : _shF[StepFrame47(T.chill36ShimmerMs, _frzT47 - inS, true, out _)];
            }
            else { int bi = StepFrame47(T.chill36BreakMs, _brkT47, false, out bool done); if (!done) fs = _brF[bi]; }
            _frz47.enabled = fs != null; if (_glass47) _glass47.enabled = !(fs != null && T.glassHorns36HideFrozen); if (fs != null) { _frz47.sprite = fs; _frz47.flipX = flip; _frz47.sortingOrder = ord + T.chill36FrozenSort; }
            // pips: count up with pop frame on gain; MAX on all three during freeze-in; hidden otherwise
            int ns = Mathf.Clamp(_chill44, 0, 3);
            if (ns > _lastStacks47) { _popIdx47 = ns - 1; _popT47 = T.chill36PipPopMs / 1000f; }
            _lastStacks47 = ns; _popT47 -= dt;
            float inS2 = 0; foreach (var d in T.chill36FreezeInMs) inS2 += d / 1000f;
            bool showMax = frozen && _frzT47 < inS2, show = (ns > 0 || showMax) && _pipF != null && _pipF.Length >= 4;
            _bar47.enabled = show;
            if (show)
            {
                float bottom = (_combo47 && T.traits36V47 ? T.chill36PipBottomComboPx : _tall47 && T.traits36V47 ? T.chill36PipBottomTallPx : T.chill36PipBottomBasePx) / HArt.PPU;
                _bar47.transform.localPosition = new Vector3(0, bottom, 0); _bar47.sortingOrder = ord + T.chill36PipSort;
                float cy = bottom + T.chill36BarPx.y * 0.5f / HArt.PPU;
                for (int i = 0; i < 3; i++)
                {
                    var p = _pip47[i]; p.enabled = true; p.sortingOrder = ord + T.chill36PipSort + 1;
                    p.transform.localPosition = new Vector3((i - 1) * T.chill36PipSpacingPx / HArt.PPU, cy, 0);
                    p.sprite = _pipF[showMax ? 3 : i < ns ? (i == _popIdx47 && _popT47 > 0 ? 2 : 1) : 0];
                }
            }
            else foreach (var p in _pip47) p.enabled = false;
        }
    }
}
