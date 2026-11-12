using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    // t930u: shader batch 3 (VH-approved) on top of the t928u PixelAoE pool.
    //  - Grave Pool merge: all live grave pools (and, separately, Ghost Mushroom pools) draw as ONE quad with a min(d_i/R_i) field -> no seams.
    //  - #40 Burning City hybrid (kind 3): low shader ember band + opt A hand tongues, back half on the ground, front half over the actors.
    //  - #41 Ghost Mushrooms v2 (kind 5): lilac/slate pool, thin mycelium threads, rising spores.
    //  - #14 Death Bell (kind 6): replaces the purple pentagram telegraph; fires the same RuneBurst+CurseZone callback at the same point (k = 0.6).
    //  - #36 Ghost Banner (kind 7): bone-white wisp per ghost projectile instead of the Soul Spears sprite.
    // Draw-only. All gameplay (radius, damage, timing, homing) stays in the old code paths. Each piece has its own switch; off = old FX.
    public partial class HellordGame
    {
        Material _sfxMat930City, _sfxMat930Mush, _sfxMat930Bell, _sfxMat930Ghost;
        bool _sfxMush930; // set by SinMushroom818 around its GravePoolZone call
        readonly SpriteRenderer[] _sfxClu930 = new SpriteRenderer[2];
        readonly Vector4[] _sfxPools930 = new Vector4[16];
        static readonly int P930Pools = Shader.PropertyToID("_Pools"), P930Count = Shader.PropertyToID("_PoolCount"), P930Frame = Shader.PropertyToID("_Frame"), P930Fb = Shader.PropertyToID("_Fb"), P930Dir = Shader.PropertyToID("_Dir");

        void ShaderFxSpawn930(SFx928 f, Vector2 at, float rU)
        {
            var T = HellordTuning.I; float sq = Mathf.Max(0.1f, T.fxSquash928); f.sr.gameObject.SetActive(true);
            f.m.SetFloat(P928Seed, Random.value * 6.28f); f.m.SetFloat(P928T0, Random.value * 10f);
            if (f.kind == 7)
            {
                f.sr.sortingOrder = T.fxGhostOrder930; f.m.SetFloat(P928Seed, Random.value * 3f);
                float s = (T.fxGhostLenPx930 + 12f) / HArt.PPU; f.sr.transform.localScale = new Vector3(s * 2f, s * 2f, 1); // centred on the head, covers any direction
                f.sr.transform.position = at; f.m.SetVector(P928Center, at); return;
            }
            float rPx = rU * HArt.PPU; f.m.SetFloat(P928Radius, rPx); f.m2.SetFloat(P928Radius, rPx); f.m.SetFloat("_Side", 0); f.m2.SetFloat("_Side", 1);
            f.sr.sortingOrder = T.fxShaderOrder928; f.sr2.sortingOrder = f.kind == 3 ? T.fxCityFrontOrder930 : T.fxBellFrontOrder930;
            float padX = (f.kind == 3 ? T.sin40W818 * 0.3f : 0f) + 0.4f, padY = f.kind == 3 ? 1.6f : 0.4f; // tongues reach ~22 px up
            foreach (var sr in new[] { f.sr, f.sr2 }) { sr.gameObject.SetActive(true); sr.enabled = true; sr.transform.localScale = new Vector3(rU * 2f + padX * 2f, rU * 2f * sq + padY * 2f, 1); }
            if (f.kind == 3) { f.m.SetFloat("_BandW", T.sin40W818 * HArt.PPU * 0.6f / 2f); f.m2.SetFloat("_BandW", T.sin40W818 * HArt.PPU * 0.6f / 2f); }
            ShaderFxPlace930(f, at);
        }
        void ShaderFxPlace930(SFx928 f, Vector2 at)
        {
            f.at = at; var T = HellordTuning.I;
            Vector2 qc = f.kind == 3 ? at + Vector2.up * 0.6f : at; // city quad leans up so the tongue tips fit
            f.sr.transform.position = qc; f.m.SetVector(P928Center, at);
            if (f.sr2) { f.sr2.transform.position = qc; f.m2.SetVector(P928Center, at); }
        }
        void ShaderFxApply930(SFx928 f)
        {
            var T = HellordTuning.I;
            if (f.kind == 7)
            {
                var p = f.proj; if (p == null || !p.alive || p.sin818 != 2) { f.on = false; f.sr.gameObject.SetActive(false); return; }
                f.at = p.pos; f.sr.transform.position = p.pos; f.m.SetVector(P928Center, p.pos); f.m.SetVector(P930Dir, p.vel.sqrMagnitude > 1e-6f ? p.vel.normalized : Vector2.right); return;
            }
            if (f.kind == 6)
            {
                float fr = f.age * 30f; f.m.SetFloat(P930Frame, fr); f.m.SetFloat("_TelF", Mathf.Max(1f, f.flare / Mathf.Max(0.05f, T.fxBellFlareK930) * 30f));
                if (f.onCast != null && f.age >= f.flare) { var a = f.onCast; f.onCast = null; a(); } // same k=0.6 point as HellFX.RuneCircle
                float fb = f.onCast == null ? (f.age - f.flare) * 30f : -1f; f.m2.SetFloat(P930Fb, fb); f.m.SetFloat(P930Fb, fb);
                return;
            }
            if (f.kind == 3) { var c = _heroPos + Vector2.up * 0.3f; ShaderFxPlace930(f, c); f.m.SetFloat(P928Radius, f.r * HArt.PPU); f.m2.SetFloat(P928Radius, f.r * HArt.PPU); }
        }
        float PoolGrow930(SFx928 f)
        {
            var T = HellordTuning.I; float g = Mathf.Min(1f, 0.4f + 0.6f * f.age / Mathf.Max(0.01f, T.fxZoneGrowS928));
            float left = f.life - f.age; if (left < T.fxZoneFadeS928) g = Mathf.Min(g, Mathf.Clamp01(left / Mathf.Max(0.01f, T.fxZoneFadeS928)));
            return g;
        }
        void ShaderFxMerge930(int kind, int slot)
        {
            var T = HellordTuning.I; var cl = _sfxClu930[slot];
            if (!T.fxGraveMerge930 || !_sfxOk928)
            {
                if (cl) cl.gameObject.SetActive(false);
                foreach (var f in _sfx928) if (f.on && f.kind == kind && f.merged) { f.merged = false; f.sr.enabled = true; }
                return;
            }
            int n = 0; float x0 = 1e9f, y0 = 1e9f, x1 = -1e9f, y1 = -1e9f, sq = Mathf.Max(0.1f, T.fxSquash928);
            foreach (var f in _sfx928)
            {
                if (!f.on || f.kind != kind) continue;
                if (n >= 16) { if (f.merged) { f.merged = false; f.sr.enabled = true; } continue; } // overflow -> own quad (old single-pool path)
                float g = PoolGrow930(f), rPx = f.r * HArt.PPU * Mathf.Max(g, 0.05f) * (g >= 1f ? 1f : 0.9f);
                _sfxPools930[n++] = new Vector4(f.at.x, f.at.y, rPx, 0); f.merged = true; f.sr.enabled = false;
                x0 = Mathf.Min(x0, f.at.x - f.r); x1 = Mathf.Max(x1, f.at.x + f.r); y0 = Mathf.Min(y0, f.at.y - f.r * sq); y1 = Mathf.Max(y1, f.at.y + f.r * sq);
            }
            if (n == 0) { if (cl) cl.gameObject.SetActive(false); return; }
            if (cl == null)
            {
                var src = kind == 1 ? _sfxMat928Grave : _sfxMat930Mush;
                cl = new GameObject("fx930_cluster_" + kind).AddComponent<SpriteRenderer>(); cl.sprite = _sfxQuad928; cl.sharedMaterial = new Material(src); _sfxClu930[slot] = cl;
                cl.sharedMaterial.SetFloat(P928Grow, 1f); cl.sharedMaterial.SetFloat(P928EdgeW, T.fxGraveEdgeW928);
            }
            for (int i = n; i < 16; i++) _sfxPools930[i] = Vector4.zero;
            cl.gameObject.SetActive(true); cl.sortingOrder = T.fxShaderOrder928 + slot;
            cl.transform.position = new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, 0); cl.transform.localScale = new Vector3(x1 - x0 + 0.6f, y1 - y0 + 0.6f, 1);
            var m = cl.sharedMaterial; m.SetVectorArray(P930Pools, _sfxPools930); m.SetFloat(P930Count, n); m.SetFloat(P928Radius, 16f);
        }
        /// <summary>#14 Death Bell telegraph. Returns false when the shader is off (caller uses HellFX.RuneCircle).</summary>
        bool ShaderBell930(Vector2 at, int radiusPx, float seconds, System.Action onCast)
        {
            if (!ShaderFx928(6)) return false; var T = HellordTuning.I;
            float flare = seconds * T.fxBellFlareK930; float life = Mathf.Max(seconds + 4f / 30f, flare + 7f / 30f);
            int id = ShaderFxSpawn928(6, at, radiusPx / (float)HArt.PPU, life); if (id < 0) return false;
            var f = _sfx928[id]; f.flare = flare; f.onCast = onCast; ShaderFxApply930(f); return true;
        }
        /// <summary>#36 ghost projectile look. Hides the spear FX/sprite of p and follows it.</summary>
        void ShaderGhost930(Proj p)
        {
            if (p == null || !ShaderFx928(7)) return;
            foreach (var o in _sfx928) if (o.on && o.kind == 7 && o.proj == p) { o.on = false; o.sr.gameObject.SetActive(false); } // pooled proj reused
            int id = ShaderFxSpawn928(7, p.pos, 1f, 999f); if (id < 0) return;
            _sfx928[id].proj = p; if (p.fx44 >= 0) { HeroSkillFX.SpearEnd(p.fx44); p.fx44 = -1; } if (p.trail != null) { HellFXRunner.I.Kill(p.trail); p.trail = null; } if (p.sr) p.sr.enabled = false;
            ShaderFxApply930(_sfx928[id]);
        }
        int _sfxCity930 = -1;
        /// <summary>#40 ring: keep one shader ring alive while _sinRingT818 runs. Returns true when the shader draws it.</summary>
        bool ShaderCity930(float R, float left)
        {
            if (!ShaderFx928(3)) return false;
            if (_sfxCity930 < 0 || _sfxCity930 >= _sfx928.Count || !_sfx928[_sfxCity930].on || _sfx928[_sfxCity930].kind != 3) _sfxCity930 = ShaderFxSpawn928(3, _heroPos + Vector2.up * 0.3f, R, left + 0.05f);
            if (_sfxCity930 < 0) return false;
            var f = _sfx928[_sfxCity930]; f.r = R; f.life = f.age + Mathf.Max(0.05f, left); return true;
        }
    }
}
