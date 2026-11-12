using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // t928u: VH-approved PixelAoE shader FX (Fireball blast, Curse zone veins, Grave Pool liquid).
    // Draw-only: gameplay radius / damage / ticks stay in the old code paths. Off (HellordTuning.fxShaderOn) = old FX.
    public partial class HellordGame
    {
        class SFx928 { public SpriteRenderer sr; public Material m; public int kind; public Vector2 at; public float r, age, life, pulse; public bool on; public Proj proj; public System.Action onCast; public float flare = -1; public SpriteRenderer sr2; public Material m2; public bool merged; } // t930u: proj (ghost), onCast/flare (bell), sr2 front half (city, bell)
        readonly List<SFx928> _sfx928 = new List<SFx928>();
        Material _sfxMat928Curse, _sfxMat928Grave, _sfxMat928Fire; Sprite _sfxQuad928; bool _sfxTried928, _sfxOk928;
        static readonly int P928Center = Shader.PropertyToID("_Center"), P928Radius = Shader.PropertyToID("_RadiusPx"), P928Grow = Shader.PropertyToID("_Grow"), P928Life = Shader.PropertyToID("_Life"),
            P928Pulse = Shader.PropertyToID("_Pulse"), P928Seed = Shader.PropertyToID("_Seed"), P928T0 = Shader.PropertyToID("_T0"), P928EdgeW = Shader.PropertyToID("_EdgeW");
        public static string ShaderFxStatus928 = "not loaded";

        bool ShaderFx928(int kind)
        {
            var T = HellordTuning.I; if (!T.fxShaderOn) return false;
            if (kind == 0 && !T.fxShaderCurse928) return false; if (kind == 1 && !T.fxShaderGrave928) return false; if (kind == 2 && !T.fxShaderFireball928) return false;
            if (kind == 3 && !T.fxShaderCity930) return false; if (kind == 5 && !T.fxShaderMush930) return false; if (kind == 6 && !T.fxShaderBell930) return false; if (kind == 7 && !T.fxShaderGhost930) return false; if (kind == 4 || kind > 7) return false;
            if (!_sfxTried928) LoadShaderFx928();
            return _sfxOk928;
        }
        Material LoadMat928(string n, Shader sh)
        {
            var m = Resources.Load<Material>("HellordFX928/" + n);
            if (m == null && sh != null) m = new Material(sh) { name = n };
            if (m != null && m.shader != null && !m.shader.isSupported && sh != null && sh.isSupported) m.shader = sh;
            return m;
        }
        void Tex928(Material m, string prop, string res) { if (m == null || string.IsNullOrEmpty(res)) return; var t = Resources.Load<Texture2D>("HellordFX928/" + res); if (t == null) { Debug.LogWarning("[t928u] missing texture HellordFX928/" + res); return; } bool ramp = prop == "_RampTex"; t.filterMode = ramp ? FilterMode.Point : FilterMode.Bilinear; t.wrapMode = ramp ? TextureWrapMode.Clamp : TextureWrapMode.Repeat; m.SetTexture(prop, t); }
        void LoadShaderFx928()
        {
            _sfxTried928 = true; var T = HellordTuning.I;
            var sh = Resources.Load<Shader>("HellordShaders/HellordPixelAoE"); if (sh == null) sh = Shader.Find("Hellord/PixelAoE");
            if (sh == null || !sh.isSupported) { ShaderFxStatus928 = sh == null ? "shader missing" : "shader not supported"; Debug.LogWarning("[t928u] PixelAoE " + ShaderFxStatus928 + " - old FX used"); return; }
            _sfxMat928Curse = LoadMat928("fx928_curse", sh); _sfxMat928Grave = LoadMat928("fx928_grave", sh); _sfxMat928Fire = LoadMat928("fx928_fireball", sh);
            foreach (var m in new[] { _sfxMat928Curse, _sfxMat928Grave, _sfxMat928Fire }) { Tex928(m, "_NoiseTex", T.fxNoiseTex928); Tex928(m, "_CellTex", T.fxCellTex928); m.SetFloat("_TimeStep", T.fxTimeStep928); m.SetFloat("_PPU", HArt.PPU); }
            Tex928(_sfxMat928Curse, "_RampTex", T.fxRampCurse928); Tex928(_sfxMat928Grave, "_RampTex", T.fxRampGrave928); Tex928(_sfxMat928Fire, "_RampTex", T.fxRampFire928);
            var c = _sfxMat928Curse; c.SetFloat("_Mode", 0); c.SetFloat("_CellPx", T.fxCurseCellPx928); c.SetFloat("_Warp", T.fxCurseWarp928); c.SetFloat("_EdgeNoise", T.fxCurseEdgeNoise928); c.SetFloat("_VeinW", T.fxCurseVeinW928); c.SetFloat("_Squash", T.fxSquash928); c.SetVector("_Flow", T.fxCurseFlow928);
            var g = _sfxMat928Grave; g.SetFloat("_Mode", 1); g.SetFloat("_CellPx", T.fxGraveCellPx928); g.SetFloat("_Warp", T.fxGraveWarp928); g.SetFloat("_EdgeNoise", T.fxGraveEdgeNoise928); g.SetFloat("_NGain", T.fxGraveNGain928); g.SetFloat("_Squash", T.fxSquash928); g.SetVector("_Flow", T.fxGraveFlow928); g.SetVector("_Scroll", T.fxGraveScroll928);
            _sfxMat928Fire.SetFloat("_Mode", 2); _sfxMat928Fire.SetFloat("_RampCount", 8); // t988u: force 8-slot fire ramp (mat default / Properties was 6 → white clamp)
            // t930u batch3 materials (runtime-built from the same shader; ramps in HellordFX928/fx930_ramp_*)
            System.Func<string, float, Material> mk = (rp, mode) => { var m = new Material(sh) { name = "fx930_" + mode }; Tex928(m, "_NoiseTex", T.fxNoiseTex928); Tex928(m, "_CellTex", T.fxCellTex928); Tex928(m, "_RampTex", rp); m.SetFloat("_TimeStep", T.fxTimeStep928); m.SetFloat("_PPU", HArt.PPU); m.SetFloat("_Mode", mode); m.SetFloat("_Squash", T.fxSquash928); return m; };
            _sfxMat930City = mk(T.fxRampCity930, 3); _sfxMat930City.SetFloat("_RampCount", 6); _sfxMat930City.SetFloat("_FlameH", T.fxCityBandH930);
            _sfxMat930Mush = mk(T.fxRampMush930, 5); _sfxMat930Mush.SetFloat("_RampCount", 8); _sfxMat930Mush.SetFloat("_CellPx", T.fxGraveCellPx928); _sfxMat930Mush.SetFloat("_Warp", T.fxGraveWarp928); _sfxMat930Mush.SetFloat("_EdgeNoise", T.fxGraveEdgeNoise928); _sfxMat930Mush.SetFloat("_EdgeW", T.fxGraveEdgeW928); _sfxMat930Mush.SetFloat("_NGain", T.fxGraveNGain928); _sfxMat930Mush.SetVector("_Flow", T.fxMushFlow930); _sfxMat930Mush.SetVector("_Scroll", T.fxGraveScroll928);
            _sfxMat930Bell = mk(T.fxRampBell930, 6); _sfxMat930Bell.SetFloat("_RampCount", 6);
            _sfxMat930Ghost = mk(T.fxRampGhost930, 7); _sfxMat930Ghost.SetFloat("_RampCount", 7); _sfxMat930Ghost.SetFloat("_RadiusPx", T.fxGhostLenPx930); _sfxMat930Ghost.SetFloat("_BandW", T.fxGhostWidthPx930); _sfxMat930Ghost.SetFloat("_FlameH", T.fxGhostWavePx930); _sfxMat930Ghost.SetFloat("_WFreq", T.fxGhostWaveFreq930); _sfxMat930Ghost.SetFloat("_Gain", T.fxGhostGain930);
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255); tex.SetPixels32(px); tex.Apply();
            _sfxQuad928 = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f); // 1 world unit quad
            _sfxOk928 = true; ShaderFxStatus928 = "ok"; Debug.Log("[t928u] PixelAoE shader FX loaded (" + sh.name + ")");
        }
        /// <summary>kind 0 curse, 1 grave, 2 fireball burst. Returns a handle (index) or -1.</summary>
        int ShaderFxSpawn928(int kind, Vector2 at, float rU, float life)
        {
            if (!ShaderFx928(kind)) return -1;
            SFx928 f = null; int id = -1;
            for (int i = 0; i < _sfx928.Count; i++) if (!_sfx928[i].on && _sfx928[i].kind == kind) { f = _sfx928[i]; id = i; break; }
            if (f == null)
            {
                var src = kind == 0 ? _sfxMat928Curse : kind == 1 ? _sfxMat928Grave : kind == 3 ? _sfxMat930City : kind == 5 ? _sfxMat930Mush : kind == 6 ? _sfxMat930Bell : kind == 7 ? _sfxMat930Ghost : _sfxMat928Fire;
                var sr = new GameObject("fx928_" + kind).AddComponent<SpriteRenderer>(); sr.sprite = _sfxQuad928; sr.sharedMaterial = new Material(src);
                f = new SFx928 { sr = sr, m = sr.sharedMaterial, kind = kind }; _sfx928.Add(f); id = _sfx928.Count - 1;
                if (kind == 3 || kind == 6) { var s2 = new GameObject("fx930_" + kind + "_front").AddComponent<SpriteRenderer>(); s2.sprite = _sfxQuad928; s2.sharedMaterial = new Material(src); s2.sharedMaterial.SetFloat("_Side", 1); f.sr2 = s2; f.m2 = s2.sharedMaterial; }
            }
            var T = HellordTuning.I; f.on = true; f.at = at; f.r = rU; f.age = 0; f.life = Mathf.Max(0.05f, life); f.pulse = 0; f.proj = null; f.onCast = null; f.flare = -1; f.merged = false; f.sr.enabled = true;
            if (kind == 3 || kind == 6 || kind == 7) { ShaderFxSpawn930(f, at, rU); return id; }
            f.sr.gameObject.SetActive(true); f.sr.sortingOrder = kind == 2 ? 62 : T.fxShaderOrder928; // burst sits under the pack impact flash (64)
            if (kind == 2 && T.fxFireFix988) f.m.SetFloat("_RampCount", 8);
            float sq = kind == 2 ? 1f : Mathf.Max(0.1f, T.fxSquash928);
            f.sr.transform.position = new Vector3(at.x, at.y, 0); f.sr.transform.localScale = new Vector3(rU * 2f + 0.5f, rU * 2f * sq + 0.5f, 1);
            f.m.SetVector(P928Center, at); f.m.SetFloat(P928Radius, rU * HArt.PPU); f.m.SetFloat(P928Seed, Random.value * 6.28f); f.m.SetFloat(P928T0, Random.value * 10f);
            ShaderFxApply928(f); return id;
        }
        void ShaderFxMove928(int id, Vector2 at) { if (id < 0 || id >= _sfx928.Count || !_sfx928[id].on) return; var f = _sfx928[id]; f.at = at; f.sr.transform.position = at; f.m.SetVector(P928Center, at); }
        void ShaderFxPulse928(int id) { if (id >= 0 && id < _sfx928.Count && _sfx928[id].on) _sfx928[id].pulse = 0.5f; }
        void ShaderFxEnd928(int id) { if (id >= 0 && id < _sfx928.Count && _sfx928[id].on) { var f = _sfx928[id]; f.life = Mathf.Min(f.life, f.age + HellordTuning.I.fxZoneFadeS928); } }
        void ShaderFxApply928(SFx928 f)
        {
            var T = HellordTuning.I;
            if (f.kind == 2) { f.m.SetFloat(P928Life, Mathf.Clamp01(f.age / f.life)); return; }
            float g = Mathf.Min(1f, 0.4f + 0.6f * f.age / Mathf.Max(0.01f, T.fxZoneGrowS928));
            float left = f.life - f.age; if (left < T.fxZoneFadeS928) g = Mathf.Min(g, Mathf.Clamp01(left / Mathf.Max(0.01f, T.fxZoneFadeS928)));
            f.m.SetFloat(P928Grow, g);
            if (f.kind == 0) f.m.SetFloat(P928Pulse, f.pulse);
            else f.m.SetFloat(P928EdgeW, T.fxGraveEdgeW928 / Mathf.Max(g, 0.3f));
        }
        void ShaderFxTick928(float dt)
        {
            var T = HellordTuning.I;
            for (int i = 0; i < _sfx928.Count; i++)
            {
                var f = _sfx928[i]; if (!f.on) continue;
                f.age += dt; f.pulse = Mathf.Max(0, f.pulse - dt * 0.5f / Mathf.Max(0.01f, T.fxCursePulseS928));
                if (f.age >= f.life) { f.on = false; f.sr.gameObject.SetActive(false); if (f.sr2) f.sr2.gameObject.SetActive(false); continue; }
                if (f.kind == 3 || f.kind == 6 || f.kind == 7) ShaderFxApply930(f); else ShaderFxApply928(f);
            }
            ShaderFxMerge930(1, 0); ShaderFxMerge930(5, 1); // t930u merged pool clusters
        }
        public string ShaderFxDebug928()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var f in _sfx928) { if (!f.on) continue; var m = f.m; System.Func<string, string> tn = k => { var t = m.GetTexture(k); return t ? t.name + " " + t.width + "x" + t.height : "NULL"; };
                sb.Append($"[k{f.kind} at {f.at} r {f.r:0.00} pos {f.sr.transform.position} scl {f.sr.transform.localScale} order {f.sr.sortingOrder} layer {f.sr.sortingLayerName} vis {f.sr.isVisible} sh {m.shader.name}/{m.shader.isSupported} ramp {tn("_RampTex")} noise {tn("_NoiseTex")} cell {tn("_CellTex")} grow {m.GetFloat("_Grow"):0.00} life {m.GetFloat("_Life"):0.00} hero {_heroPos}] "); }
            return sb.ToString();
        }
        public int ShaderFxActive928() { int n = 0; foreach (var f in _sfx928) if (f.on) n++; return n; }
    }
}
