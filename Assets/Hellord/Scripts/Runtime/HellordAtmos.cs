using System.Collections.Generic;
using UnityEngine;

// v0.0.22 hell atmosphere (Viet Hung: "more hell like with the light and atmosphere but do not hurt players' eyes").
// Everything tunable in HellordTuning ("Hell atmosphere"). H toggles it, -noatmos starts with it off.
//  * Warm ember grade: camera quad at sorting order -30500 = after room base/props, before room lights and ALL gameplay sprites
//    (gameplay YSort orders are about -432..+400), so the hero, enemies, bullets, warnings and pickups are never graded.
//  * Banded + Bayer-dithered fire pools under every brazier (additive, HellordLight shader), plus gentle flicker on warm room lights.
//  * Flicker: two slow sines (flickerHz, x1.85), depth flickerAmp (capped 0.08) -> no strobe, < 3 Hz.
//  * Ash + embers: small pool of 1-2 art-px sprites drifting up, drawn under gameplay.
//  * Red edge fog, heat haze (glow layer only) and the low-HP edge pulse live in HellordCRT.shader (edges only, never full screen).
// Pipeline-agnostic: plain sprites + UnityCG shaders, no 2D lights / volumes / renderer features.
namespace Hellord
{
    public partial class HellordGame
    {
        public static bool atmosOn = true; // set from HellordTuning.atmosEnabled / -noatmos in AtmosInit (Resources.Load is not allowed in a static initializer)
        public float atmosLabelT;
        SpriteRenderer _gradeMul, _gradeAdd; Material _atmosMul, _atmosAdd; static Sprite _px, _poolSprite;
        HellordRoom _atmosRoom; readonly List<(SpriteRenderer sr, float phase)> _firePools = new List<(SpriteRenderer, float)>();
        struct Mote { public Transform t; public SpriteRenderer sr; public Vector2 p; public float life, age, sway, ph, rise; public bool ember; }
        readonly List<Mote> _motes = new List<Mote>(); Transform _atmosRoot; MaterialPropertyBlock _mpb;

        public static float AtmosFlicker(float t, float phase)
        {
            var T = HellordTuning.I; float h = Mathf.Min(T.flickerHz, 1.6f);
            return 0.6f * Mathf.Sin(2 * Mathf.PI * h * t + phase) + 0.4f * Mathf.Sin(2 * Mathf.PI * h * 1.85f * t + 1.3f + phase * 1.7f);
        }
        static Sprite PxSprite()
        {
            if (_px) return _px; var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; t.SetPixel(0, 0, Color.white); t.Apply();
            return _px = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);   // 1 world unit, scaled per use
        }
        // Banded radial pool (4 bands) with a 4x4 Bayer dither across each band edge: pixel-art light, no smooth gradient.
        static Sprite PoolSprite()
        {
            if (_poolSprite) return _poolSprite; var T = HellordTuning.I; int R = Mathf.Max(8, T.d2Dark606 ? T.d2FirePoolRadiusPx606 : T.lightsSourcedV46 ? T.firePoolRadiusPx46 : T.firePoolRadiusPx), N = R * 2; bool sm = T.lightsSourcedV46 && T.firePoolSmooth46;
            int[] bayer = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 }; float[] edge = { 0.32f, 0.55f, 0.78f, 1f }; float[] a = { 1f, 0.62f, 0.34f, 0.14f };
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var c = T.firePoolColor; var px = new Color[N * N];
            for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
            {
                float dx = (x + 0.5f - R) / R, dy = (y + 0.5f - R) / (R * 0.62f); float r = Mathf.Sqrt(dx * dx + dy * dy);   // squashed: floor perspective
                float th = (bayer[(y & 3) * 4 + (x & 3)] + 0.5f) / 16f * 0.09f; int b = 0; while (b < 4 && r > edge[b] - 0.045f + th) b++;
                if (sm) { float f = Mathf.Clamp01(1 - r); f *= f; float q = Mathf.Floor(f * 8 + (bayer[(y & 3) * 4 + (x & 3)] + 0.5f) / 16f) / 8f; px[y * N + x] = new Color(c.r, c.g, c.b, Mathf.Clamp01(q)); continue; } /* v0.0.46: smooth dithered falloff */
                px[y * N + x] = b >= 4 ? new Color(c.r, c.g, c.b, 0) : new Color(c.r, c.g, c.b, a[b]);
            }
            tex.SetPixels(px); tex.Apply(); return _poolSprite = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), HArt.PPU, 0, SpriteMeshType.FullRect);
        }
        Material AtmosMat(bool add)
        {
            var sh = Resources.Load<Shader>("HellordShaders/HellordAtmos"); if (sh == null || !sh.isSupported) { Debug.LogWarning("[Hellord] atmos shader missing"); return null; }
            var m = new Material(sh) { name = add ? "HellordAtmosAdd" : "HellordAtmosMul", hideFlags = HideFlags.DontSave };
            m.SetFloat("_Mode", add ? 1 : 0); m.SetFloat("_SrcBlend", add ? 1 : 2); m.SetFloat("_DstBlend", add ? 1 : 3); return m;
        }
        void AtmosInit()
        {
            if (_atmosRoot) return; _mpb = new MaterialPropertyBlock(); atmosOn = HellordTuning.I.atmosEnabled && System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-noatmos") < 0;
            _atmosRoot = new GameObject("HellAtmos").transform;
            _atmosMul = AtmosMat(false); _atmosAdd = AtmosMat(true);
            _gradeMul = MakeQuad("grade_mul", _atmosMul, -30500); _gradeAdd = MakeQuad("grade_lift", _atmosAdd, -30499);
            var T = HellordTuning.I; var rnd = new System.Random(2222);
            for (int i = 0; i < T.emberCount + T.ashCount; i++)
            {
                var go = new GameObject(i < T.emberCount ? "ember" : "ash"); go.transform.SetParent(_atmosRoot, false);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = PxSprite(); sr.sortingOrder = -29000;
                var m = new Mote { t = go.transform, sr = sr, ember = i < T.emberCount }; float s = (m.ember && rnd.NextDouble() < 0.5 ? 2 : 1) / (float)HArt.PPU; go.transform.localScale = new Vector3(s, s, 1);
                m.age = (float)rnd.NextDouble() * 5; m.life = 0; _motes.Add(m);
            }
            Debug.Log("[Hellord] hell atmosphere " + (atmosOn ? "ON" : "OFF") + " (H toggles)");
        }
        SpriteRenderer MakeQuad(string n, Material m, int order)
        {
            var go = new GameObject(n); go.transform.SetParent(_atmosRoot, false); var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = PxSprite(); sr.sortingOrder = order; if (m) sr.sharedMaterial = m; return sr;
        }
        void AtmosRoomChanged()
        {
            foreach (var p in _firePools) if (p.sr) Destroy(p.sr.gameObject); _firePools.Clear(); _atmosRoom = _room; if (_room == null || _room.D.braziers == null) return;
            var add = HellordRoom.LightMatPublic(false); int i = 0;
            foreach (var b in _room.D.braziers)
            {
                var go = new GameObject("firepool"); go.transform.SetParent(_room.Root, false); go.transform.position = HellordRoom.Px(b.x + 13, b.y - 4);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = PoolSprite(); sr.sortingOrder = HellordRoom.LightOrder - 1; if (add) sr.sharedMaterial = add;
                _firePools.Add((sr, i++ * 2.1f));
            }
        }
        void UpdateAtmos()
        {
            if (_cam == null) return; AtmosInit(); var T = HellordTuning.I;
            if (HInput.Down(HKey.Atmos)) { atmosOn = !atmosOn; atmosLabelT = 2f; Debug.Log("[Hellord] hell atmosphere " + (atmosOn ? "ON" : "OFF")); }
            if (_atmosRoom != _room) AtmosRoomChanged();
            float t = Time.time, amp = Mathf.Min(T.flickerAmp, 0.08f); bool lin = QualitySettings.activeColorSpace == ColorSpace.Linear;
            // grade quads cover the camera view
            float hh = _cam.orthographicSize, hw = hh * _cam.aspect; Vector3 cp = _cam.transform.position; cp.z = 0;
            foreach (var q in new[] { _gradeMul, _gradeAdd }) { if (!q) continue; q.enabled = atmosOn; q.transform.position = cp; q.transform.localScale = new Vector3(hw * 2 + 2, hh * 2 + 2, 1); }
            if (atmosOn && _gradeMul) {
                float g = 1 + amp * 0.4f * AtmosFlicker(t, 0); Vector4 tint = (Vector4)FloorGradeLinear() * g, lift = T.shadowLift * (T.d2Dark606 ? T.d2ShadowLiftMul606 : 1f);
                if (!lin) { tint = new Vector4(Mathf.Pow(tint.x, 1 / 2.2f), Mathf.Pow(tint.y, 1 / 2.2f), Mathf.Pow(tint.z, 1 / 2.2f), 1); lift = new Vector4(Mathf.LinearToGammaSpace(lift.x), Mathf.LinearToGammaSpace(lift.y), Mathf.LinearToGammaSpace(lift.z), 1); }
                _gradeMul.GetPropertyBlock(_mpb); _mpb.SetVector("_Tint", tint); _gradeMul.SetPropertyBlock(_mpb);
                _gradeAdd.GetPropertyBlock(_mpb); _mpb.SetVector("_Tint", lift); _gradeAdd.SetPropertyBlock(_mpb);
            }
            // fire pools + warm room lights: gentle flicker (off -> pools hidden, room lights back to their authored strength)
            foreach (var p in _firePools) { if (!p.sr) continue; p.sr.enabled = atmosOn; if (!atmosOn) continue; p.sr.GetPropertyBlock(_mpb); _mpb.SetTexture("_MainTex", p.sr.sprite.texture); _mpb.SetFloat("_Strength", (T.d2Dark606 ? T.d2FirePoolK606 : T.lightsSourcedV46 ? T.firePoolStrength46 : T.firePoolStrength) * (1 + amp * AtmosFlicker(t, p.phase))); p.sr.SetPropertyBlock(_mpb); }
            if (_room != null) for (int i = 0; i < _room.Lights.Count; i++)
            {
                var l = _room.Lights[i]; if (!l.sr || !l.warm) continue; float k = atmosOn ? l.k * (T.d2Dark606 ? T.d2RoomLightBoost606 : T.roomLightBoost) * (1 + amp * AtmosFlicker(t, i * 1.37f)) : l.k;
                l.sr.GetPropertyBlock(_mpb); _mpb.SetFloat("_Strength", k); l.sr.SetPropertyBlock(_mpb);
            }
            Hide586(); // t586u
            // ash + embers (world space, snapped to the art-pixel grid, slow fade in/out: no blinking)
            float dt = Time.deltaTime; Vector2 c = cp;
            for (int i = 0; i < _motes.Count; i++)
            {
                var m = _motes[i]; m.sr.enabled = atmosOn; if (!atmosOn) continue;
                m.age += dt;
                if (m.age >= m.life) { m.age = 0; m.life = Random.Range(3.5f, 7f); m.p = c + new Vector2(Random.Range(-hw, hw), Random.Range(-hh - 1, hh * 0.6f)); m.sway = Random.Range(0.15f, 0.45f); m.ph = Random.value * 6.3f; m.rise = (m.ember ? T.emberRise : T.ashRise) * Random.Range(0.7f, 1.3f); }
                m.p += new Vector2(Mathf.Sin(m.ph + m.age * 0.9f) * m.sway * dt + (m.ember ? 0 : 0.12f * dt), m.rise * dt);
                float u = m.age / m.life, fade = Mathf.Clamp01(Mathf.Min(u, 1 - u) * 4f); var col = m.ember ? T.emberColor : T.ashColor; col.a *= fade * (m.ember && T.d2Dark606 ? T.d2EmberAlpha606 : 1f);
                m.sr.color = col; m.t.position = new Vector3(Snap(m.p.x), Snap(m.p.y), 0); _motes[i] = m;
            }
        }
        Color FogCol606() { var T = HellordTuning.I; return T.d2Dark606 ? T.d2VignetteColor606 : T.edgeFogColor; } // t606u
        float FogInner606() { var T = HellordTuning.I; return T.d2Dark606 ? T.d2VignetteInner606 : T.edgeFogInner; } // t606u
        /// <summary>Base edge fog strength (0 when the atmosphere is off).</summary>
        float AtmosFogAmt() => atmosOn && HellordTuning.I.v2Light586 ? 0f : atmosOn && HellordTuning.I.d2Dark606 ? HellordTuning.I.d2VignetteAmt606 : atmosOn && !(HellordTuning.I.lightsSourcedV46 && HellordTuning.I.lightsNoEdgeFog46) ? HellordTuning.I.edgeFogAmt : 0; /* v0.0.46 */
        /// <summary>v0.0.23: low-HP edge pulse 0..lowHpPulseAmt, its own brighter ember red (lowHpPulseColor). Before, the pulse only pushed
        /// the edges further toward the dark fog colour, which on the dark floor edges changed ~5/255 and could not be seen.</summary>
        float AtmosPulse()
        {
            var T = HellordTuning.I; float hp01 = maxHp > 0 ? hp / maxHp : 1;
            if (!atmosOn || !T.lowHpPulse || hp01 >= T.lowHpThreshold || mode != Mode.Play) return 0;
            return T.lowHpPulseAmt * (0.5f - 0.5f * Mathf.Cos(2 * Mathf.PI * Mathf.Min(T.lowHpPulseHz, 1.5f) * Time.time)) * (1 - hp01 / Mathf.Max(0.01f, T.lowHpThreshold) * 0.5f);
        }
        /// <summary>CRT-pass atmosphere uniforms (edge fog, haze, low-HP pulse). Zeroed when the atmosphere is off.</summary>
        void SetAtmosCrt(Material crt)
        {
            var T = HellordTuning.I;
            crt.SetVector("_FogCol", (Vector4)FogCol606()); crt.SetFloat("_FogAmt", AtmosFogAmt()); crt.SetFloat("_FogInner", FogInner606());
            crt.SetVector("_PulseCol", (Vector4)T.lowHpPulseColor); crt.SetFloat("_PulseAmt", AtmosPulse());
            crt.SetFloat("_Haze", atmosOn ? T.hazeAmpPx : 0); crt.SetFloat("_AtmosT", Time.time);
        }
        // v0.0.23: -nocrt fallback. Without the CRT pass there was no edge fog and, more importantly, no low-HP edge pulse.
        // Same shape as the CRT fog (alpha = edgeE^2, centre 0), drawn over the world but under the HUD. HellordTuning.noCrtEdgeFog.
        static Texture2D _fogTex; static float _fogTexInner = -1;
        void DrawAtmosEdgeNoCrt(Rect r)
        {
            var T = HellordTuning.I; if (!atmosOn || !T.noCrtEdgeFog) return; float a = Mathf.Clamp01(AtmosFogAmt()), pa = Mathf.Clamp01(AtmosPulse()); if (a <= 0.001f && pa <= 0.001f) return;
            if (_fogTex == null || _fogTexInner != FogInner606())
            {
                const int W = 160, H = 90; _fogTexInner = FogInner606(); if (_fogTex == null) _fogTex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                var px = new Color[W * H];
                for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { float u = (x + 0.5f) / W * 2 - 1, v = (y + 0.5f) / H * 2 - 1; float e = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(_fogTexInner, 1.25f, Mathf.Sqrt(u * u + v * v))); px[y * W + x] = new Color(1, 1, 1, e * e); }
                _fogTex.SetPixels(px); _fogTex.Apply();
            }
            var oc = GUI.color; var fc = FogCol606(); GUI.color = new Color(fc.r, fc.g, fc.b, a); GUI.DrawTexture(r, _fogTex, ScaleMode.StretchToFill, true);
            if (pa > 0.001f) { var pc = T.lowHpPulseColor; GUI.color = new Color(pc.r, pc.g, pc.b, pa); GUI.DrawTexture(r, _fogTex, ScaleMode.StretchToFill, true); }
            GUI.color = oc;
        }
    }
}
