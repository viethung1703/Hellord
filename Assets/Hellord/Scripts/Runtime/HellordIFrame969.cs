using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // t969u/t970u v0.0.83: i-frames are shown on the hero sprite itself (shader Hellord/HeroIFrame, 3 modes) instead of the HurtFX ring.
    public partial class HellordGame
    {
        #pragma warning disable 0414
        bool _atActive969;
#pragma warning restore 0414
        Material _ifMat969; bool _ifOn969; readonly Dictionary<SpriteRenderer, Material> _ifOrig969 = new Dictionary<SpriteRenderer, Material>(), _ifMats987 = new Dictionary<SpriteRenderer, Material>();
        bool IfMatOn987(SpriteRenderer r) => r != null && r.sharedMaterial != null && (r.sharedMaterial == _ifMat969 || r.sharedMaterial.name.StartsWith("HeroIFrame987_"));
        public int iframeFxFrames969; // frames the i-frame FX was active (autotest)
        /// <summary>Hero can't be hurt: real i-frames, test-menu god, or autotest god (autotest god no longer forces _iframe).</summary>
        #if HELLORD_AUTOTEST
        bool HeroInvuln969 => _iframe > 0 || _tmGod || (_atActive969 && _atGod);
#else
        bool HeroInvuln969 => _iframe > 0 || _tmGod;
#endif

        void IfSet969(SpriteRenderer r, bool on)
        {
            if (r == null) return;
#if HELLORD_AUTOTEST
            if (on && AtIfLegacy988) { if (!_ifOrig969.ContainsKey(r)) _ifOrig969[r] = r.sharedMaterial; _ifMat969.SetVector("_TexelPx", Vector4.zero); if (r.sharedMaterial != _ifMat969) r.sharedMaterial = _ifMat969; return; } // t982u: v0.0.86 path (shared mat, _MainTex_TexelSize)
#endif
            if (on)
            {   // t981u v0.0.87: one material per renderer (hero base + each trait strip) with its own texel size; a shared material let the
                // trait strips (896x50) and the hero (576x36) fight over one texel size, and the shader's UV snap then drew the hero as big blocks
                if (!_ifOrig969.ContainsKey(r)) _ifOrig969[r] = r.sharedMaterial;
                if (!_ifMats987.TryGetValue(r, out var mi) || mi == null) { mi = new Material(_ifMat969) { name = "HeroIFrame987_" + r.name }; _ifMats987[r] = mi; }
                mi.CopyPropertiesFromMaterial(_ifMat969);
                var tex = r.sprite ? r.sprite.texture : null; mi.SetVector("_TexelPx", tex ? new Vector4(1f / tex.width, 1f / tex.height, tex.width, tex.height) : Vector4.zero);
                if (r.sharedMaterial != mi) r.sharedMaterial = mi;
            }
            else if (_ifOrig969.TryGetValue(r, out var m)) { r.sharedMaterial = m; _ifOrig969.Remove(r); }
        }
        void UpdateIFrameFX969()
        {
            var T = HellordTuning.I; bool act = _iframe > 0 && mode == Mode.Play;
            if (act) iframeFxFrames969++;
            if (T.iframeShader970 && _ifMat969 == null)
            {
                var sh = Resources.Load<Shader>("HellordShaders/HellordHeroIFrame"); if (sh == null) sh = Shader.Find("Hellord/HeroIFrame");
                if (sh != null) _ifMat969 = new Material(sh) { name = "HeroIFrame969" };
            }
            bool useSh = act && T.iframeShader970 && _ifMat969 != null;
            if (useSh)
            {
                _ifMat969.SetFloat("_Mode", Mathf.Clamp(T.iframeShaderMode970, 0, 2)); _ifMat969.SetFloat("_TimeStep", Mathf.Max(1f, T.iframeShaderFps970));
                _ifMat969.SetFloat("_Strength", Mathf.Clamp01(T.iframeShaderStrength970)); _ifMat969.SetFloat("_K", Mathf.Clamp01(_iframe / Mathf.Max(0.01f, HeroIFrame)));
            }
            if (useSh || _ifOn969)
            {
                IfSet969(_heroBase, useSh); foreach (var l in _heroLayers) IfSet969(l.sr, useSh);
                if (_heroOutline != null && T.iframeShaderMode970 == 1 && useSh) _heroOutline.enabled = false; // B: outline would stay solid while the sprite dissolves
                _ifOn969 = useSh;
            }
            if (act && !useSh)
            {   // fallback: plain dim blink (no white, nothing drawn over the hero)
                bool dim = Mathf.FloorToInt(_iframe / Mathf.Max(0.02f, T.iframeBlinkPeriod969)) % 2 == 1;
                if (dim) { var c = _heroBase.color; float k = T.iframeBlinkDim969; var d = new Color(c.r * k, c.g * k, c.b * k, c.a * T.iframeBlinkAlpha969); _heroBase.color = d; foreach (var l in _heroLayers) l.sr.color = new Color(l.sr.color.r * k, l.sr.color.g * k, l.sr.color.b * k, l.sr.color.a * T.iframeBlinkAlpha969); }
            }
        }

#if HELLORD_AUTOTEST
        // tools/rec_review.sh iframes969 <dir>: hero hit by an imp, i-frames played in each shader mode (clip_iframes_A/B/C.mp4).
        System.Collections.IEnumerator AtIFrames969()
        {
            var T = HellordTuning.I; int oMode = T.iframeShaderMode970; float oMax = maxHp, oArm = armor; string[] nm = { "A", "B", "C" };
            for (int m = 0; m < 3; m++)
            {
                string clip = "iframes969_iframes_" + nm[m]; yield return AtBegin(clip);
                T.iframeShaderMode970 = m; AtClearWorld(); AtSetSkills(); Vector2 c0 = RoomCenterFeet(); AtHeroTo(c0); SnapCamera();
                maxHp = hp = 100000; armor = 0; _atGod = false; _iframe = 0; yield return AtWait(0.3f);
                float ortho0 = _cam.orthographicSize, orthoDev = 0, bW0 = _heroBase.bounds.size.x, bDev = 0;
                int recI = 0, f0 = iframeFxFrames969, h0 = heroHits; bool ringSeen = T.iframeRing969; bool matOn = false; float t = 0, nextHit = 0.25f;
                while (t < 2.6f)
                {
                    yield return null; AtFrameChecks(); t += Time.deltaTime; hp = maxHp;
                    if (t >= nextHit && _iframe <= 0) { HurtHero(5f, "at_iframes969"); nextHit = t + 1.1f; }
                    if (IfMatOn987(_heroBase)) matOn = true;
                    orthoDev = Mathf.Max(orthoDev, Mathf.Abs(_cam.orthographicSize - ortho0)); bDev = Mathf.Max(bDev, Mathf.Abs(_heroBase.bounds.size.x - bW0));
                    AtRecFrame(clip, ref recI);
                }
                int fx = iframeFxFrames969 - f0; yield return AtWait(0.9f);
                bool restored = !IfMatOn987(_heroBase);
                ARes("iframes969:" + nm[m], heroHits - h0 >= 2 && fx > 0 && matOn && restored && !ringSeen && orthoDev < 1e-4f && bDev < 0.07f, $"orthoDev {orthoDev:0.0000} heroW dev {bDev:0.00} hits {heroHits - h0} fxFrames {fx} shader {matOn} restored {restored} ring {ringSeen}");
                AtEnd(clip);
            }
            T.iframeShaderMode970 = oMode; maxHp = oMax; hp = maxHp; armor = oArm; _atGod = true; _iframe = 0; AtClearWorld();
        }


        // t990u: Frost Cleave VH-locked sample_v7 shader smoke + swing shots
        System.Collections.IEnumerator AtFrost990()
        {
            yield return AtBegin("frost990");
            _atGod = true; AtClearWorld(); AtSetSkills((SkillId.FrostCleave, 3));
            AtHeroTo(RoomCenterFeet()); SnapCamera(); yield return AtWait(0.2f);
            // a few imps in a front arc so anyHit holds kick in
            for (int k = 0; k < 5; k++)
            {
                float a = (-40f + k * 20f) * Mathf.Deg2Rad;
                var e = SpawnEnemyAt(EType.Imp, _heroPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3.2f);
                if (e != null) { e.hp = e.maxHp = 40; e.speed = 0.001f; e.dmg = 0; }
            }
            int cleave0 = HFX.CleaveOk; bool okLoad = FrostCleave990.Ok;
            // direct spawn for reliable visual (auto-cast may wait for aim)
            HFX.FrostCleaveAnyHit990 = true; HFX.FrostCleave(_heroPos, Vector2.right, 4.8f, 1f, 115f);
            float t = 0; int shots = 0, recI = 0;
            while (t < 1.0f)
            {
                yield return null; AtFrameChecks(); t += Time.deltaTime; hp = Mathf.Max(hp, maxHp);
                AtRecFrame("frost990", ref recI);
                if (t >= shots * 0.1f && shots < 10) yield return AtShot($"frost990_{shots++:00}");
            }
            // second swing
            HFX.FrostCleaveAnyHit990 = false; HFX.FrostCleave(_heroPos, Vector2.right, 4.8f, 1f, 115f);
            t = 0;
            while (t < 0.6f) { yield return null; t += Time.deltaTime; AtRecFrame("frost990", ref recI); }
            ARes("frost990:shader", HellordTuning.I.frostCleaveShader990 && okLoad && HFX.CleaveOk > cleave0,
                $"okLoad {okLoad} flag {HellordTuning.I.frostCleaveShader990} cleaves {HFX.CleaveOk - cleave0} (total {HFX.CleaveOk})");
            _atGod = true; AtSetSkills(); AtEnd("frost990");
        }

        // t979u v0.0.86: Fire Wall lands at the (fake) cursor when in range, clamped to groundAoeRange716 beyond it.
        System.Collections.IEnumerator AtFireWall986()
        {
            var T = HellordTuning.I; bool oAim = T.autoMouseAim720; T.autoMouseAim720 = true;
            foreach (var nm in new[] { "far_target", "out_of_range" })
            {
                string clip = "firewall986_" + nm; yield return AtBegin(clip);
                AtClearWorld(); AtSetSkills(); Vector2 c0 = RoomCenterFeet() + Vector2.left * 4f; AtHeroTo(c0); SnapCamera(); yield return AtWait(0.3f);
                Vector2 tgt = c0 + new Vector2(6f, 1.5f); ClampToRoom(ref tgt, 6);
                Vector2 mouse = nm == "far_target" ? tgt : c0 + (tgt - c0).normalized * 12f;
                var e = SpawnEnemyAt(EType.Imp, tgt); if (e != null) { e.hp = e.maxHp = 1e5f; e.speed = 0.001f; e.dmg = 0; }
                atMouse986 = mouse; AtSetSkills((SkillId.HellfirePillar, 1));
                float t = 0; int recI = 0; Vector2 wc = Vector2.zero; bool got = false;
                while (t < 2.5f)
                {
                    yield return null; AtFrameChecks(); t += Time.deltaTime; hp = Mathf.Max(hp, maxHp);
                    if (!got && _fw948.Count > 0 && _fw948[0].seg.Count > 0) { wc = (_fw948[0].seg[0].a + _fw948[0].seg[0].b) * 0.5f; got = true; }
                    AtRecFrame(clip, ref recI);
                }
                Vector2 want = mouse; if ((want - c0).magnitude > T.groundAoeRange716) want = c0 + (want - c0).normalized * T.groundAoeRange716; ClampToRoom(ref want, 4);
                float dealt = e != null ? e.maxHp - e.hp : 0;
                bool ok = got && (wc - want).magnitude < 0.35f && (nm != "far_target" || dealt > 0);
                ARes("firewall986:" + nm, ok, $"wall centre {wc} want {want} err {(wc - want).magnitude:0.00} heroDist {(wc - c0).magnitude:0.00} dmg {dealt:0}");
                atMouse986 = null; AtSetSkills(); ClearFireWalls948(); yield return AtWait(0.3f); AtEnd(clip);
            }
            T.autoMouseAim720 = oAim; atMouse986 = null;
        }
#endif
    }
}
