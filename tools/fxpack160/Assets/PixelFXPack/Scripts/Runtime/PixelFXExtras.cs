using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pixel FX Pack v1.3 additions: screen distortion driver, invincibility blink, water reflection, squash & stretch,
// pixel damage numbers, and code-spawned VFX (lightning, teleport beam, level-up, heal, buff aura).
// UNTESTED IN THE UNITY EDITOR at v1.3.0 - compiled against Unity API stubs only. See README "Known limitations".
namespace PixelFXPack
{
    /// <summary>
    /// Drives PixelFX/Screen FX (URP) through global shader values: screen shockwave, screen chromatic aberration, heat haze.
    /// Requires the Full Screen Pass Renderer Feature using PixelFX_ScreenFX.mat (see README). Without it these calls do nothing visible.
    /// </summary>
    public static class PixelScreenFX
    {
        static readonly int ID_Shock = Shader.PropertyToID("_PFX_Shock");
        static readonly int ID_ShockWidth = Shader.PropertyToID("_PFX_ShockWidth");
        static readonly int ID_Chroma = Shader.PropertyToID("_PFX_Chroma");
        static readonly int ID_HeatRect = Shader.PropertyToID("_PFX_HeatRect");
        static readonly int ID_Heat = Shader.PropertyToID("_PFX_Heat");
        static readonly int ID_Snap = Shader.PropertyToID("_PFX_PixelSnap");
        static Coroutine _shock, _chroma;

        /// <summary>Quantise distortion offsets to N screen pixels (0 = smooth). Default 0.</summary>
        public static void SetPixelSnap(float screenPixels) => Shader.SetGlobalFloat(ID_Snap, screenPixels);

        /// <summary>Radial distortion ring expanding from a world position.</summary>
        public static void Shockwave(Camera cam, Vector3 worldPos, float strength = 1f, float seconds = 0.45f, float maxRadius = 0.6f, float width = 0.08f)
        {
            if (cam == null) return;
            Vector3 v = cam.WorldToViewportPoint(worldPos);
            if (_shock != null) PixelJuiceRunner.I.StopCoroutine(_shock);
            _shock = PixelJuiceRunner.I.StartCoroutine(CoShock(new Vector2(v.x, v.y), strength, seconds, maxRadius, width));
        }
        static IEnumerator CoShock(Vector2 c, float s, float d, float r, float w)
        {
            Shader.SetGlobalFloat(ID_ShockWidth, w);
            float t = 0;
            while (t < d)
            {
                t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / d), e = 1 - (1 - k) * (1 - k);
                Shader.SetGlobalVector(ID_Shock, new Vector4(c.x, c.y, e * r, s * (1 - k)));
                yield return null;
            }
            Shader.SetGlobalVector(ID_Shock, Vector4.zero);
            _shock = null;
        }

        /// <summary>Full-screen RGB split that decays (big hits, damage taken).</summary>
        public static void ChromaticPulse(float pixels = 3f, float seconds = 0.25f)
        {
            if (_chroma != null) PixelJuiceRunner.I.StopCoroutine(_chroma);
            _chroma = PixelJuiceRunner.I.StartCoroutine(CoChroma(pixels, seconds));
        }
        static IEnumerator CoChroma(float p, float d)
        {
            float t = 0;
            while (t < d) { t += Time.unscaledDeltaTime; Shader.SetGlobalFloat(ID_Chroma, Mathf.Round(p * (1 - t / d))); yield return null; }
            Shader.SetGlobalFloat(ID_Chroma, 0); _chroma = null;
        }

        /// <summary>Constant chromatic offset (0 = off).</summary>
        public static void SetChromatic(float pixels) => Shader.SetGlobalFloat(ID_Chroma, pixels);

        /// <summary>Heat haze inside a viewport rect (0..1). amplitude in screen pixels; 0 disables.</summary>
        public static void SetHeatHaze(Rect viewportRect, float amplitudePixels)
        {
            Shader.SetGlobalVector(ID_HeatRect, new Vector4(viewportRect.xMin, viewportRect.yMin, viewportRect.xMax, viewportRect.yMax));
            Shader.SetGlobalFloat(ID_Heat, amplitudePixels);
        }
        public static void ClearAll()
        {
            Shader.SetGlobalVector(ID_Shock, Vector4.zero); Shader.SetGlobalFloat(ID_Chroma, 0); Shader.SetGlobalFloat(ID_Heat, 0);
        }
    }

    /// <summary>Classic i-frame blink: toggles the sprite on/off (or flashes white) at a fixed rate for a duration.</summary>
    [AddComponentMenu("Pixel FX/Invincibility Blink")]
    public class InvincibilityBlink : MonoBehaviour
    {
        public float blinksPerSecond = 12f;
        [Tooltip("If set, flash white via PixelFX instead of hiding the sprite.")] public bool flashInsteadOfHide = false;
        public bool IsBlinking { get; private set; }
        SpriteRenderer _sr; PixelFX _fx; Coroutine _co;

        void Awake() { _sr = GetComponent<SpriteRenderer>(); _fx = GetComponent<PixelFX>(); }
        public void Play(float seconds)
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Co(seconds));
        }
        IEnumerator Co(float d)
        {
            IsBlinking = true;
            float t = 0;
            while (t < d)
            {
                bool on = Mathf.FloorToInt(t * blinksPerSecond) % 2 == 0;
                if (flashInsteadOfHide && _fx != null) _fx.flashAmount = on ? 0 : 1;
                else if (_sr != null) _sr.enabled = on;
                t += Time.deltaTime; yield return null;
            }
            Stop();
        }
        public void Stop()
        {
            if (_co != null) { StopCoroutine(_co); _co = null; }
            if (_sr != null) _sr.enabled = true;
            if (_fx != null && flashInsteadOfHide) _fx.flashAmount = 0;
            IsBlinking = false;
        }
        void OnDisable() { Stop(); }
    }

    /// <summary>Water reflection: a flipped, tinted, rippling copy of the sprite mirrored about a water line (world Y).</summary>
    [AddComponentMenu("Pixel FX/Water Reflection")]
    public class WaterReflection : MonoBehaviour
    {
        public float waterLineY = 0f;
        public Color tint = new Color(0.4f, 0.7f, 1f, 0.45f);
        [Range(0, 3)] public float rippleAmplitude = 1f;
        public float rippleSpeed = 4f;
        public int sortingOffset = -1;
        SpriteRenderer _src, _dst; PixelFX _fx;

        void OnEnable()
        {
            _src = GetComponent<SpriteRenderer>();
            if (_src == null) return;
            var go = new GameObject("Reflection");
            go.transform.SetParent(transform, false);
            _dst = go.AddComponent<SpriteRenderer>();
            _fx = go.AddComponent<PixelFX>();
        }
        void OnDisable() { if (_dst != null) Destroy(_dst.gameObject); _dst = null; }
        void LateUpdate()
        {
            if (_src == null || _dst == null) return;
            _dst.sprite = _src.sprite; _dst.flipX = _src.flipX; _dst.flipY = !_src.flipY;
            _dst.color = tint; _dst.sortingLayerID = _src.sortingLayerID; _dst.sortingOrder = _src.sortingOrder + sortingOffset;
            // mirror world position about the water line
            Vector3 p = transform.position;
            // flipY mirrors the sprite about its pivot; with a feet pivot on the water line this reads as a true reflection
            _dst.transform.position = new Vector3(p.x, 2 * waterLineY - p.y, p.z);
            _dst.transform.localScale = Vector3.one;
            _fx.waveAmplitude = rippleAmplitude; _fx.waveSpeed = rippleSpeed; _fx.waveFrequency = 0.9f;
        }
    }

    /// <summary>Squash and stretch "punch" on a transform (jump, land, hit). Keeps the feet planted if the pivot is at the feet.</summary>
    [AddComponentMenu("Pixel FX/Squash Stretch")]
    public class SquashStretch : MonoBehaviour
    {
        Vector3 _base; Coroutine _co;
        void Awake() { _base = transform.localScale; }
        /// <summary>amount &gt; 0 stretches tall, &lt; 0 squashes wide. Springs back over seconds.</summary>
        public void Punch(float amount = -0.3f, float seconds = 0.18f)
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Co(amount, seconds));
        }
        IEnumerator Co(float a, float d)
        {
            float t = 0;
            while (t < d)
            {
                t += Time.deltaTime; float k = Mathf.Clamp01(t / d);
                float s = a * Mathf.Cos(k * Mathf.PI * 1.5f) * (1 - k);
                transform.localScale = new Vector3(_base.x * (1 - s * 0.6f), _base.y * (1 + s), _base.z);
                yield return null;
            }
            transform.localScale = _base; _co = null;
        }
    }

    /// <summary>Pixel damage / heal numbers using a built-in 3x5 pixel font. Pops up, hangs, then falls and blinks out.</summary>
    public static class DamageNumbers
    {
        static readonly Dictionary<char, string> Font = new Dictionary<char, string>
        {
            {'0',"111101101101111"},{'1',"010110010010111"},{'2',"111001111100111"},{'3',"111001111001111"},{'4',"101101111001001"},
            {'5',"111100111001111"},{'6',"111100111101111"},{'7',"111001010010010"},{'8',"111101111101111"},{'9',"111101111001111"},
            {'x',"000101010101000"},{'+',"000010111010000"},{'-',"000000111000000"},{'!',"010010010000010"},
        };
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Builds (and caches) a sprite for text with a 1px dark outline. Only 0-9 + - ! x are supported.</summary>
        public static Sprite TextSprite(string text, Color32 fill, float ppu = 16f)
        {
            string key = text + fill.ToString() + ppu;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            int w = text.Length * 4 + 1, h = 7;
            var px = new Color32[w * h];
            var ink = new bool[w * h];
            for (int i = 0; i < text.Length; i++)
            {
                if (!Font.TryGetValue(text[i], out var g)) continue;
                for (int gy = 0; gy < 5; gy++) for (int gx = 0; gx < 3; gx++)
                    if (g[gy * 3 + gx] == '1') ink[(5 - gy) * w + 1 + i * 4 + gx] = true;
            }
            var outline = new Color32(20, 12, 28, 255);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                if (ink[y * w + x]) { px[y * w + x] = fill; continue; }
                bool n = false;
                for (int dy = -1; dy <= 1 && !n; dy++) for (int dx = -1; dx <= 1; dx++)
                {
                    int xx = x + dx, yy = y + dy;
                    if (xx >= 0 && yy >= 0 && xx < w && yy < h && ink[yy * w + xx]) { n = true; break; }
                }
                px[y * w + x] = n ? outline : new Color32(0, 0, 0, 0);
            }
            return Cache[key] = PixelSpriteFactory.FromPixels(px, w, h, ppu, "Dmg_" + text);
        }

        public static void Spawn(Vector3 pos, int value, Color32 color, int order = 90, float ppu = 16f, bool crit = false)
        {
            string s = (value > 0 && color.g > color.r ? "+" : "") + value + (crit ? "!" : "");
            var sr = new GameObject("DamageNumber").AddComponent<SpriteRenderer>();
            sr.sprite = TextSprite(s, color, ppu); sr.sortingOrder = order; sr.transform.position = pos;
            if (PixelFXMaterials.Shared != null) sr.sharedMaterial = PixelFXMaterials.Shared;
            PixelJuiceRunner.I.StartCoroutine(Co(sr, crit ? 2 : 1, ppu));
        }
        static IEnumerator Co(SpriteRenderer sr, int scale, float ppu)
        {
            Vector3 p0 = sr.transform.position; float t = 0, d = 0.8f;
            float vx = Random.Range(-0.6f, 0.6f);
            while (t < d && sr)
            {
                t += Time.deltaTime;
                float pop = t < 0.08f ? 1.5f : 1f;
                sr.transform.localScale = Vector3.one * scale * pop;
                float y = 1.2f * Mathf.Sin(Mathf.Min(t / 0.5f, 1.4f) * Mathf.PI * 0.5f) - Mathf.Max(0, t - 0.5f) * 2f;
                Vector3 p = p0 + new Vector3(vx * t, y, 0);
                sr.transform.position = new Vector3(Mathf.Round(p.x * ppu) / ppu, Mathf.Round(p.y * ppu) / ppu, p.z);
                sr.enabled = t < 0.6f || Mathf.FloorToInt(t * 30) % 2 == 0;
                yield return null;
            }
            if (sr) Object.Destroy(sr.gameObject);
        }
    }

    /// <summary>Code-spawned VFX built from procedural pixel sprites (no prefabs, no art files).</summary>
    public static class PixelVFX
    {
        static Sprite _px;
        static Sprite Px(float ppu) => _px != null && Mathf.Approximately(_px.pixelsPerUnit, ppu) ? _px : (_px = PixelSpriteFactory.Rect(1, 1, new Color32(255, 255, 255, 255), ppu, "PFX_Pixel"));
        static SpriteRenderer Quad(string name, Vector3 pos, Vector2 sizePx, Color c, int order, float ppu, bool additive = true)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.sprite = Px(ppu); sr.color = c; sr.sortingOrder = order;
            sr.transform.position = pos; sr.transform.localScale = new Vector3(sizePx.x, sizePx.y, 1);
            var m = additive ? PixelFXMaterials.Additive : PixelFXMaterials.Shared;
            if (m != null) sr.sharedMaterial = m;
            return sr;
        }
        static IEnumerator DestroyAfter(GameObject go, float s) { yield return new WaitForSeconds(s); if (go) Object.Destroy(go); }

        /// <summary>Jagged pixel lightning bolt between two points; re-rolls its shape a few times then fades.</summary>
        public static void Lightning(Vector3 from, Vector3 to, Color color, float seconds = 0.3f, int order = 80, float ppu = 16f)
        {
            var root = new GameObject("Lightning");
            PixelJuiceRunner.I.StartCoroutine(CoLightning(root, from, to, color, seconds, order, ppu));
        }
        static IEnumerator CoLightning(GameObject root, Vector3 a, Vector3 b, Color c, float d, int order, float ppu)
        {
            float t = 0; int frame = 0;
            while (t < d && root)
            {
                foreach (Transform ch in root.transform) Object.Destroy(ch.gameObject);
                int steps = Mathf.Max(2, Mathf.RoundToInt(Vector3.Distance(a, b) * ppu / 4f));
                Vector3 prev = a;
                Vector3 perp = Vector3.Cross((b - a).normalized, Vector3.forward);
                for (int i = 1; i <= steps; i++)
                {
                    Vector3 p = Vector3.Lerp(a, b, i / (float)steps);
                    if (i < steps) p += perp * Random.Range(-3, 4) / ppu;
                    DrawLine(root.transform, prev, p, frame % 2 == 0 ? Color.white : c, order, ppu);
                    prev = p;
                }
                frame++;
                float wait = 0.05f; t += wait;
                yield return new WaitForSeconds(wait);
            }
            if (root) Object.Destroy(root);
        }
        static void DrawLine(Transform parent, Vector3 a, Vector3 b, Color c, int order, float ppu)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Vector3.Distance(a, b) * ppu));
            for (int i = 0; i <= n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                p = new Vector3(Mathf.Round(p.x * ppu) / ppu, Mathf.Round(p.y * ppu) / ppu, a.z);
                Quad("bolt", p, Vector2.one, c, order, ppu).transform.SetParent(parent, true);
            }
        }

        /// <summary>Vertical teleport beam: column grows down, flashes, narrows to a line, with rising sparks.</summary>
        public static void TeleportBeam(Vector3 feet, Color color, float heightPx = 64, float seconds = 0.6f, int order = 75, float ppu = 16f)
        {
            PixelJuiceRunner.I.StartCoroutine(CoBeam(feet, color, heightPx, seconds, order, ppu));
            var sp = PixelBurstSettings.Sparkle(); sp.directionDegrees = 90; sp.spreadDegrees = 30; sp.count = 16;
            PixelParticles.Burst(sp, feet);
        }
        static IEnumerator CoBeam(Vector3 feet, Color c, float h, float d, int order, float ppu)
        {
            var core = Quad("BeamCore", feet, new Vector2(4, 1), Color.white, order + 1, ppu);
            var glow = Quad("BeamGlow", feet, new Vector2(10, 1), c, order, ppu);
            float t = 0;
            while (t < d && core)
            {
                t += Time.deltaTime; float k = Mathf.Clamp01(t / d);
                float grow = Mathf.Clamp01(k / 0.25f);
                float width = k < 0.6f ? 1 : 1 - (k - 0.6f) / 0.4f;
                float hh = Mathf.Round(h * grow);
                foreach (var sr in new[] { core, glow })
                {
                    float w = sr == core ? Mathf.Max(1, Mathf.Round(4 * width)) : Mathf.Max(0, Mathf.Round(10 * width));
                    sr.transform.localScale = new Vector3(w, hh, 1);
                    sr.transform.position = feet + new Vector3(0, (h - hh * 0.5f) / ppu, 0);
                }
                glow.color = new Color(c.r, c.g, c.b, (Mathf.FloorToInt(t * 30) % 2 == 0 ? 0.9f : 0.6f) * width);
                yield return null;
            }
            if (core) Object.Destroy(core.gameObject);
            if (glow) Object.Destroy(glow.gameObject);
        }

        /// <summary>Level-up: light pillar, expanding ring, rising sparkles and a gold light pop.</summary>
        public static void LevelUp(Vector3 feet, int order = 75, float ppu = 16f)
        {
            Color gold = new Color(1f, 0.85f, 0.3f, 1);
            TeleportBeam(feet, gold, 48, 0.7f, order, ppu);
            PixelFXSpawn.Shockwave(feet + Vector3.up * 0.5f, 28, 0.5f, order + 2, ppu);
            PixelFXSpawn.LightPop(feet + Vector3.up * 1f, gold, 32, 0.4f, order - 5, ppu);
            PixelJuiceRunner.I.StartCoroutine(CoRise(feet, gold, 24, 1.0f, 12, order + 3, ppu));
        }

        /// <summary>Heal: green plus-shaped pixels rising around the target and a soft green glow.</summary>
        public static void Heal(Vector3 feet, int order = 75, float ppu = 16f)
        {
            Color green = new Color(0.39f, 0.9f, 0.4f, 1);
            PixelFXSpawn.LightPop(feet + Vector3.up * 1f, green, 24, 0.5f, order - 5, ppu);
            PixelJuiceRunner.I.StartCoroutine(CoRise(feet, green, 10, 0.9f, 16, order, ppu, plus: true));
        }

        static IEnumerator CoRise(Vector3 feet, Color c, int count, float d, float spreadPx, int order, float ppu, bool plus = false)
        {
            var parts = new List<(SpriteRenderer sr, Vector3 p0, float delay, float speed)>();
            for (int i = 0; i < count; i++)
            {
                Vector3 p0 = feet + new Vector3(Random.Range(-spreadPx, spreadPx) / ppu, Random.Range(0, 8) / ppu, 0);
                var sr = Quad(plus ? "HealPlus" : "Rise", p0, Vector2.one, c, order, ppu);
                if (plus)
                {
                    sr.sprite = PlusSprite(ppu); sr.transform.localScale = Vector3.one;
                }
                parts.Add((sr, p0, Random.Range(0, d * 0.4f), Random.Range(1.5f, 3f)));
            }
            float t = 0;
            while (t < d)
            {
                t += Time.deltaTime;
                foreach (var q in parts)
                {
                    if (!q.sr) continue;
                    float lt = Mathf.Max(0, t - q.delay);
                    Vector3 p = q.p0 + Vector3.up * lt * q.speed;
                    q.sr.transform.position = new Vector3(Mathf.Round(p.x * ppu) / ppu, Mathf.Round(p.y * ppu) / ppu, p.z);
                    float a = lt <= 0 ? 0 : Mathf.Floor((1 - Mathf.Clamp01(lt / (d - q.delay))) * 4) / 4;
                    q.sr.color = new Color(c.r, c.g, c.b, a);
                }
                yield return null;
            }
            foreach (var q in parts) if (q.sr) Object.Destroy(q.sr.gameObject);
        }
        static Sprite _plus;
        static Sprite PlusSprite(float ppu)
        {
            if (_plus != null && Mathf.Approximately(_plus.pixelsPerUnit, ppu)) return _plus;
            var w = new Color32(255, 255, 255, 255); var o = new Color32(0, 0, 0, 0);
            var px = new Color32[] { o, w, o, w, w, w, o, w, o };
            return _plus = PixelSpriteFactory.FromPixels(px, 3, 3, ppu, "PFX_Plus");
        }

        /// <summary>Looping buff aura: pixels orbit the target and a ground ring pulses. Destroy the returned object to stop.</summary>
        public static GameObject BuffAura(Transform target, Color color, float radiusPx = 10, int count = 8, int order = 74, float ppu = 16f)
        {
            var root = new GameObject("BuffAura");
            root.transform.SetParent(target, false);
            var aura = root.AddComponent<PixelBuffAura>();
            aura.Init(color, radiusPx, count, order, ppu);
            return root;
        }
    }

    /// <summary>Runtime behaviour behind PixelVFX.BuffAura (orbiting pixels, front/back sorting, pulsing ring).</summary>
    public class PixelBuffAura : MonoBehaviour
    {
        readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>();
        SpriteRenderer _ring; Color _c; float _r, _ppu; int _order;
        public void Init(Color c, float radiusPx, int count, int order, float ppu)
        {
            _c = c; _r = radiusPx; _order = order; _ppu = ppu;
            var px = PixelSpriteFactory.Rect(1, 1, new Color32(255, 255, 255, 255), ppu, "PFX_AuraPixel");
            for (int i = 0; i < count; i++)
            {
                var sr = new GameObject("dot").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(transform, false); sr.sprite = px; sr.color = c;
                if (PixelFXMaterials.Additive != null) sr.sharedMaterial = PixelFXMaterials.Additive;
                _dots.Add(sr);
            }
            _ring = new GameObject("ring").AddComponent<SpriteRenderer>();
            _ring.transform.SetParent(transform, false);
            _ring.sprite = PixelFXTextures.Ring(Mathf.RoundToInt(radiusPx), 1, ppu);
            _ring.transform.localScale = new Vector3(1, 0.35f, 1);
            if (PixelFXMaterials.Additive != null) _ring.sharedMaterial = PixelFXMaterials.Additive;
            _ring.sortingOrder = order - 20;
        }
        void Update()
        {
            float t = Time.time;
            for (int i = 0; i < _dots.Count; i++)
            {
                float a = t * 3f + i * Mathf.PI * 2 / _dots.Count;
                float x = Mathf.Cos(a) * _r, y = 8 + Mathf.Sin(a) * _r * 0.35f + Mathf.Sin(t * 4 + i) * 2 + (i % 3) * 6;
                _dots[i].transform.localPosition = new Vector3(Mathf.Round(x) / _ppu, Mathf.Round(y) / _ppu, 0);
                _dots[i].sortingOrder = Mathf.Sin(a) > 0 ? _order - 20 : _order + 20; // behind when on the far side
            }
            float pulse = Mathf.Floor((0.5f + 0.5f * Mathf.Sin(t * 5)) * 3) / 3;
            _ring.color = new Color(_c.r, _c.g, _c.b, 0.3f + 0.5f * pulse);
        }
    }
}
