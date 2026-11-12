using System.Collections.Generic;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Settings for one pixel burst (hit sparks, dust, explosion...).</summary>
    [System.Serializable]
    public class PixelBurstSettings
    {
        public int count = 12;
        public Vector2 speed = new Vector2(1.5f, 4f);
        [Range(0, 360)] public float spreadDegrees = 360f;
        public float directionDegrees = 90f;
        public Vector2 lifetime = new Vector2(0.2f, 0.45f);
        public float gravity = 0f;
        public float drag = 2f;
        [Tooltip("Particle size in pixels (1 = a single pixel).")] public Vector2Int sizePixels = new Vector2Int(1, 2);
        public Gradient colorOverLife = DefaultGradient(Color.white, new Color(1f, 0.8f, 0.2f));
        [Tooltip("Color steps over life; >0 gives a stepped retro look.")] public int colorSteps = 4;
        public bool shrink = true;
        [Tooltip("v1.2: draw with the additive material so overlapping sparks glow.")] public bool additive = false;
        [Tooltip("v1.2: sideways sway (px/s) for embers.")] public float sway = 0f;

        public static Gradient DefaultGradient(Color a, Color b)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, 1) },
                      new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
            return g;
        }

        // v1.1: juicier presets (more particles, stepped Endesga-style colour ramps).
        public static PixelBurstSettings HitSparks() => new PixelBurstSettings
        { count = 16, speed = new Vector2(4, 10), lifetime = new Vector2(0.12f, 0.32f), drag = 7, sizePixels = new Vector2Int(1, 2), colorSteps = 3,
          colorOverLife = Grad3(Color.white, new Color32(254, 231, 97, 255), new Color32(247, 118, 34, 255)) };

        public static PixelBurstSettings Dust() => new PixelBurstSettings
        { count = 12, speed = new Vector2(0.6f, 2.2f), spreadDegrees = 150, directionDegrees = 90, lifetime = new Vector2(0.35f, 0.8f),
          gravity = -0.8f, drag = 3, sizePixels = new Vector2Int(1, 3), colorSteps = 3,
          colorOverLife = Grad3(new Color32(234, 212, 170, 255), new Color32(194, 133, 105, 255), new Color32(115, 62, 57, 255)) };

        public static PixelBurstSettings Explosion() => new PixelBurstSettings
        { count = 56, speed = new Vector2(1.5f, 8f), lifetime = new Vector2(0.3f, 0.9f), gravity = -3f, drag = 3.5f,
          sizePixels = new Vector2Int(1, 4), colorSteps = 5,
          colorOverLife = Grad4(Color.white, new Color32(254, 231, 97, 255), new Color32(247, 118, 34, 255), new Color32(62, 39, 49, 255)) };

        /// <summary>Coin / pickup twinkle: upward gold and white pixels.</summary>
        public static PixelBurstSettings GlowSparks() => new PixelBurstSettings
        { count = 22, speed = new Vector2(4f, 11f), spreadDegrees = 140f, directionDegrees = 0f, lifetime = new Vector2(0.15f, 0.45f), drag = 6f,
          sizePixels = new Vector2Int(1, 2), colorOverLife = Grad4(Color.white, new Color(1f, 0.91f, 0.38f), new Color(1f, 0.68f, 0.2f), new Color(0.97f, 0.46f, 0.13f)), additive = true };
        public static PixelBurstSettings Embers() => new PixelBurstSettings
        { count = 1, speed = new Vector2(0.6f, 1.4f), spreadDegrees = 30f, directionDegrees = 90f, lifetime = new Vector2(0.8f, 1.6f), drag = 0f,
          sizePixels = new Vector2Int(1, 1), colorOverLife = Grad4(new Color(1f, 0.91f, 0.38f), new Color(1f, 0.68f, 0.2f), new Color(0.97f, 0.46f, 0.13f), new Color(0.89f, 0.23f, 0.27f)), shrink = false, additive = true, sway = 1.5f };
        public static PixelBurstSettings Sparkle() => new PixelBurstSettings
        { count = 14, speed = new Vector2(1.5f, 4.5f), spreadDegrees = 100, directionDegrees = 90, lifetime = new Vector2(0.35f, 0.7f),
          gravity = -2f, drag = 2.5f, sizePixels = new Vector2Int(1, 2), colorSteps = 3,
          colorOverLife = Grad3(Color.white, new Color32(254, 231, 97, 255), new Color32(44, 232, 245, 255)) };

        static Gradient Grad4(Color a, Color b, Color c, Color d)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, 0.2f), new GradientColorKey(c, 0.55f), new GradientColorKey(d, 1) },
                      new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.8f), new GradientAlphaKey(0, 1) });
            return g;
        }

        static Gradient Grad3(Color a, Color b, Color c)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, 0.3f), new GradientColorKey(c, 1) },
                      new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.75f), new GradientAlphaKey(0, 1) });
            return g;
        }
    }

    /// <summary>
    /// Tiny pooled pixel particle system: square SpriteRenderers snapped to the pixel grid.
    /// Use PixelParticles.Burst(PixelBurstSettings.HitSparks(), position). No ParticleSystem setup needed.
    /// </summary>
    [AddComponentMenu("Pixel FX/Pixel Particles (Manager)")]
    public class PixelParticles : MonoBehaviour
    {
        [Tooltip("Pixels per world unit; match your sprites' import PPU.")] public float pixelsPerUnit = 16f;
        public string sortingLayerName = "Default";
        public int sortingOrder = 50;
        [Min(16)] public int maxParticles = 512;

        struct P { public SpriteRenderer sr; public Vector2 pos, vel; public float age, life, gravity, drag; public int size; public PixelBurstSettings s; }

        static PixelParticles _instance;
        readonly List<P> _live = new List<P>();
        readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();
        static Sprite _pixel;
        Material _defaultMat;

        public static PixelParticles Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<PixelParticles>();
                    if (_instance == null) _instance = new GameObject("PixelParticles").AddComponent<PixelParticles>();
                }
                return _instance;
            }
        }

        static Sprite PixelSprite
        {
            get
            {
                if (_pixel != null) return _pixel;
                var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PixelFX 1px" };
                t.SetPixel(0, 0, Color.white); t.Apply();
                _pixel = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                return _pixel;
            }
        }

        void Awake() { if (_instance == null) _instance = this; }
        void OnDestroy() { if (_instance == this) _instance = null; }

        public static void Burst(PixelBurstSettings s, Vector2 position) { Instance.Emit(s, position); }

        // v1.4 horde budget: once the pool is more than budgetStart full, new bursts are scaled down linearly
        // (to a minimum of 1 particle) so 300+ simultaneous deaths never stall on particle spawning.
        [Tooltip("v1.4: scale bursts down when the pool is this full (0..1). 1 = off.")] [Range(0.1f, 1f)] public float budgetStart = 0.6f;
        public static bool autoBudget = true;
        public int LiveCount => _live.Count;
        /// <summary>How many of <paramref name="requested"/> particles the budget allows right now.</summary>
        public int Budget(int requested)
        {
            if (!autoBudget || requested <= 1) return requested;
            float fill = _live.Count / (float)maxParticles;
            if (fill <= budgetStart) return requested;
            float k = 1f - Mathf.Clamp01((fill - budgetStart) / Mathf.Max(0.01f, 1f - budgetStart));
            return Mathf.Max(1, Mathf.RoundToInt(requested * k));
        }

        public void Emit(PixelBurstSettings s, Vector2 position)
        {
            if (s == null) return;
            int n = Budget(s.count);
            for (int i = 0; i < n && _live.Count < maxParticles; i++)
            {
                float ang = (s.directionDegrees + Random.Range(-s.spreadDegrees, s.spreadDegrees) * 0.5f) * Mathf.Deg2Rad;
                var p = new P
                {
                    sr = Get(), pos = position,
                    vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(s.speed.x, s.speed.y),
                    life = Random.Range(s.lifetime.x, s.lifetime.y), gravity = s.gravity, drag = s.drag,
                    size = Random.Range(s.sizePixels.x, s.sizePixels.y + 1), s = s
                };
                var am = s.additive ? PixelFXMaterials.Additive : null;
                p.sr.sharedMaterial = am != null ? am : _defaultMat;
                _live.Add(p);
            }
        }

        SpriteRenderer Get()
        {
            SpriteRenderer sr = _pool.Count > 0 ? _pool.Pop() : null;
            if (sr == null)
            {
                var go = new GameObject("px");
                go.transform.SetParent(transform, false);
                sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PixelSprite;
            }
            sr.sortingLayerName = sortingLayerName; sr.sortingOrder = sortingOrder;
            if (_defaultMat == null) _defaultMat = sr.sharedMaterial;
            sr.gameObject.SetActive(true);
            return sr;
        }

        void Update()
        {
            float dt = Time.deltaTime, unit = 1f / Mathf.Max(1f, pixelsPerUnit);
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var p = _live[i];
                p.age += dt;
                if (p.age >= p.life || p.sr == null)
                {
                    if (p.sr != null) { p.sr.gameObject.SetActive(false); _pool.Push(p.sr); }
                    _live.RemoveAt(i); continue;
                }
                p.vel += Vector2.up * (p.gravity * dt);
                p.vel *= 1f / (1f + p.drag * dt);
                p.pos += p.vel * dt;
                if (p.s.sway != 0f) p.pos.x += Mathf.Sin(p.age * 6f + p.life * 17f) * p.s.sway * dt;
                float k = p.age / p.life;
                float kc = p.s.colorSteps > 0 ? Mathf.Floor(k * p.s.colorSteps) / p.s.colorSteps : k;
                p.sr.color = p.s.colorOverLife.Evaluate(kc);
                int size = p.s.shrink ? Mathf.Max(1, Mathf.CeilToInt(p.size * (1f - kc))) : p.size;
                // snap to pixel grid
                var snapped = new Vector2(Mathf.Round(p.pos.x / unit) * unit, Mathf.Round(p.pos.y / unit) * unit);
                p.sr.transform.position = new Vector3(snapped.x, snapped.y, 0);
                p.sr.transform.localScale = new Vector3(size * unit, size * unit, 1);
                _live[i] = p;
            }
        }
    }

    /// <summary>Drop-in component: fire a preset burst from code or a UnityEvent.</summary>
    [AddComponentMenu("Pixel FX/Pixel Burst Emitter")]
    public class PixelBurstEmitter : MonoBehaviour
    {
        public enum Preset { HitSparks, Dust, Explosion, Custom, Sparkle }
        public Preset preset = Preset.HitSparks;
        public PixelBurstSettings custom = new PixelBurstSettings();
        public bool playOnEnable = false;

        void OnEnable() { if (playOnEnable) Play(); }

        public void Play() { Play(transform.position); }

        public void Play(Vector2 at)
        {
            PixelBurstSettings s;
            switch (preset)
            {
                case Preset.Dust: s = PixelBurstSettings.Dust(); break;
                case Preset.Explosion: s = PixelBurstSettings.Explosion(); break;
                case Preset.Custom: s = custom; break;
                case Preset.Sparkle: s = PixelBurstSettings.Sparkle(); break;
                default: s = PixelBurstSettings.HitSparks(); break;
            }
            PixelParticles.Burst(s, at);
        }
    }
}
