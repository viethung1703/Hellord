using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    /// <summary>t1625: one named shader parameter for a Zhurong FX hook (float or vector).</summary>
    public struct ZP
    {
        public string n; public Vector4 v; public bool vec;
        public static ZP F(string n, float f) => new ZP { n = n, v = new Vector4(f, 0, 0, 0) };
        public static ZP V(string n, Vector2 v) => new ZP { n = n, v = new Vector4(v.x, v.y, 0, 0), vec = true };
        public static ZP V4(string n, Vector4 v) => new ZP { n = n, v = v, vec = true };
    }

    // =====================================================================================================================
    // t1625: FX route for HFX's fx_v2 shaders (Art/zhurong_t1618/fx_v2/spec.md), VH-approved for every skill but Molten Shell (removed).
    // Shipped in Resources/ZhurongFX (Shaders + Materials + Textures with their fixed-GUID metas; no Molten Shell / Volcanic Orb / WIP
    // Hydra / War Cry files). Switch: HellordTuning.zhFxV2_1625 (on). A missing material = that effect's procedural placeholder.
    //  - Every Zhurong effect calls ZhFx1625(material, pivot, rect px (left, bottom, width, height relative to the pivot),
    //    sorting order, _FxTime = seconds since spawn, _Seed, then the effect's drive values from the spec: _Progress / _Spawn /
    //    _Ignite / _Cool / _Form / _Fade / _Pulse / _Crack / _Intensity / _Dir / _Vel / ... ).
    //  - When the switch is on and Resources/ZhurongFX/Materials/<material>.mat exists (Hydra / War Cry: drop HFX's finished
    //    shader + materials + metas into the same folders; the hooks already use HFX's material names), it draws a
    //    MeshRenderer quad covering the rect with one MaterialPropertyBlock (_Origin, _FxTime, _Seed + the drive values), plus the
    //    "<material>_Glow" twin at order + 1 if present, and returns true -> the caller skips its placeholder.
    //  - ZP.F("_HeroSprite", 1) = pass the hero's current frame (_MainTex, _SpriteRect with flipX as a negative width, _SpritePx).
    //  - Not hooked yet (placeholders / existing HFX strips stay): the spec's ParticleSystem debris, the heat-haze quads and the
    //    ground-light quads; Vermilion Bird's long trail quad.
    //  - ZhFxLogOn1625 (autotests) records the last drive values per material so tests can check the hooks without the art.
    // =====================================================================================================================
    public partial class HellordGame
    {
        [System.NonSerialized] public bool ZhFxLogOn1625; [System.NonSerialized] public int ZhFxQuads1625; // quads drawn by the shader route (total)
        readonly Dictionary<string, float> _zfxLog1625 = new Dictionary<string, float>(), _zfxMax1625 = new Dictionary<string, float>(); readonly HashSet<string> _zfxSeen1625 = new HashSet<string>();
        readonly Dictionary<string, Material> _zfxMat1625 = new Dictionary<string, Material>();
        readonly List<MeshRenderer> _zfxR1625 = new List<MeshRenderer>(); int _zfxN1625; Transform _zfxRoot1625; Mesh _zfxQuad1625; MaterialPropertyBlock _zfxMpb1625;

        /// <summary>Last logged value of par on mat (NaN = never drawn). Vectors log .x as par and .y as par + ".y".</summary>
        public float ZhFxLast1625(string mat, string par) => _zfxLog1625.TryGetValue(mat + "|" + par, out var v) ? v : float.NaN;
        /// <summary>Largest logged value of par on mat (NaN = never drawn).</summary>
        public float ZhFxMax1625(string mat, string par) => _zfxMax1625.TryGetValue(mat + "|" + par, out var v) ? v : float.NaN;
        public bool ZhFxSeen1625(string mat) => _zfxSeen1625.Contains(mat);
        public void ZhFxLogReset1625() { _zfxLog1625.Clear(); _zfxMax1625.Clear(); _zfxSeen1625.Clear(); }

        Material ZhFxMat1625(string name)
        {
            if (_zfxMat1625.TryGetValue(name, out var m)) return m;
            m = Resources.Load<Material>("ZhurongFX/Materials/" + name); return _zfxMat1625[name] = m;
        }

        /// <summary>Draw (or only log) one spec FX quad. Returns true when the shader drew it (caller skips the placeholder).</summary>
        bool ZhFx1625(string mat, Vector2 pivot, float l, float b, float w, float h, int order, float fxTime, float seed, params ZP[] ps)
        {
            if (ZhFxLogOn1625)
            {
                _zfxSeen1625.Add(mat); _zfxLog1625[mat + "|_FxTime"] = fxTime;
                if (ps != null) foreach (var p in ps) { string k = mat + "|" + p.n; _zfxLog1625[k] = p.v.x; if (!_zfxMax1625.TryGetValue(k, out var mx) || p.v.x > mx) _zfxMax1625[k] = p.v.x; if (p.vec) _zfxLog1625[mat + "|" + p.n + ".y"] = p.v.y; }
            }
            if (TZ == null || !TZ.zhFxV2_1625) return false;
            var m = ZhFxMat1625(mat); if (m == null) return false;
            ZhFxQuad1625(m, pivot, l, b, w, h, order, fxTime, seed, ps);
            var g = ZhFxMat1625(mat + "_Glow"); if (g != null) ZhFxQuad1625(g, pivot, l, b, w, h, order + 1, fxTime, seed, ps);
            return true;
        }
        void ZhFxQuad1625(Material m, Vector2 pivot, float l, float b, float w, float h, int order, float fxTime, float seed, ZP[] ps)
        {
            if (_zfxRoot1625 == null)
            {
                _zfxRoot1625 = new GameObject("ZhurongFX1625").transform; _zfxMpb1625 = new MaterialPropertyBlock();
                _zfxQuad1625 = new Mesh { name = "zfx_quad" }; _zfxQuad1625.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f), new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
                _zfxQuad1625.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) }; _zfxQuad1625.triangles = new[] { 0, 2, 1, 2, 3, 1 }; _zfxQuad1625.RecalculateBounds();
            }
            if (_zfxN1625 >= _zfxR1625.Count) { var go = new GameObject("zfx"); go.transform.SetParent(_zfxRoot1625, false); go.AddComponent<MeshFilter>().sharedMesh = _zfxQuad1625; _zfxR1625.Add(go.AddComponent<MeshRenderer>()); }
            ZhFxQuads1625++; var r = _zfxR1625[_zfxN1625++]; r.enabled = true; r.sharedMaterial = m; r.sortingOrder = order;
            const float U = 1f / 16f; var tr = r.transform; tr.position = new Vector3(pivot.x + (l + w * 0.5f) * U, pivot.y + (b + h * 0.5f) * U, 0f); tr.localScale = new Vector3(w * U, h * U, 1f);
            var mpb = _zfxMpb1625; mpb.Clear(); mpb.SetVector("_Origin", new Vector4(pivot.x, pivot.y, 0, 0)); mpb.SetFloat("_FxTime", fxTime); mpb.SetFloat("_Seed", seed);
            if (ps != null) foreach (var p in ps)
            {
                if (p.n == "_HeroSprite") { ZhFxHeroSprite1625(mpb); continue; }
                if (p.vec) mpb.SetVector(p.n, p.v); else mpb.SetFloat(p.n, p.v.x);
            }
            r.SetPropertyBlock(mpb);
        }
        void ZhFxHeroSprite1625(MaterialPropertyBlock mpb)
        {
            var sp = _heroBase != null ? _heroBase.sprite : null; if (sp == null || sp.texture == null) return; var t = sp.texture; var rc = sp.textureRect;
            Vector4 uv = new Vector4(rc.x / t.width, rc.y / t.height, rc.width / t.width, rc.height / t.height); if (_heroBase.flipX) { uv.x += uv.z; uv.z = -uv.z; }
            mpb.SetTexture("_MainTex", t); mpb.SetVector("_SpriteRect", uv); mpb.SetVector("_SpritePx", new Vector4(sp.rect.width, sp.rect.height, sp.pivot.x, sp.pivot.y));
        }
        /// <summary>True when the shader route will draw mat (switch on + material present): one-shot placeholders / HFX strips are skipped.</summary>
        bool ZhFxLive1625(string mat) => TZ != null && TZ.zhFxV2_1625 && ZhFxMat1625(mat) != null;
        void ZhFxBegin1625() { _zfxN1625 = 0; }
        void ZhFxEnd1625() { for (int i = _zfxN1625; i < _zfxR1625.Count; i++) if (_zfxR1625[i].enabled) _zfxR1625[i].enabled = false; }
        int ZhHeroOrd1625 => _heroBase != null ? _heroBase.sortingOrder : YSort(_heroPos.y);
        static float Z01(float x) => Mathf.Clamp01(x);
    }
}
