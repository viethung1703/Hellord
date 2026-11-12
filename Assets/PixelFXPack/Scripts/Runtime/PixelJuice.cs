using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pixel FX Pack v1.2 "juice" helpers: hit-stop, screen shake, afterimages, shockwave ring, slash arc, pixel glow, impact frame.
// Researched from Vlambeer "Art of Screenshake", Dead Cells crit freeze + slow-mo, Celeste shake. UNTESTED IN EDITOR - see README.
namespace PixelFXPack
{
    /// <summary>Hidden runner so static helpers can start coroutines.</summary>
    internal class PixelJuiceRunner : MonoBehaviour
    {
        static PixelJuiceRunner _i;
        public static PixelJuiceRunner I
        {
            get
            {
                if (_i == null) { var go = new GameObject("PixelJuice"); go.hideFlags = HideFlags.HideInHierarchy; _i = go.AddComponent<PixelJuiceRunner>(); }
                return _i;
            }
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }

    /// <summary>Hit-stop ("sleep"): freeze Time.timeScale for a few frames, then an optional slow-mo ease back (Dead Cells crit style).</summary>
    public static class HitStop
    {
        static Coroutine _co; static float _restore = 1f;
        public static bool Active { get; private set; }
        /// <param name="freezeSeconds">Real-time freeze, e.g. 0.05 (3 frames) for a hit, 0.1 for a kill.</param>
        /// <param name="slowScale">Time scale during slow-mo after the freeze (1 = none).</param>
        /// <param name="slowSeconds">Real-time slow-mo length, eased back to normal.</param>
        public static void Play(float freezeSeconds, float slowScale = 1f, float slowSeconds = 0f)
        {
            var r = PixelJuiceRunner.I;
            if (_co != null) r.StopCoroutine(_co); else _restore = Time.timeScale;
            _co = r.StartCoroutine(Co(freezeSeconds, slowScale, slowSeconds));
        }
        static IEnumerator Co(float f, float s, float sd)
        {
            Active = true; Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(f);
            float t = 0;
            while (t < sd) { t += Time.unscaledDeltaTime; Time.timeScale = Mathf.Lerp(s, _restore, t / sd); yield return null; }
            Time.timeScale = _restore; Active = false; _co = null;
        }
    }

    /// <summary>Camera shake in whole art pixels with decay, plus directional kick. Put on the camera (works with a pixel-perfect camera).</summary>
    [AddComponentMenu("Pixel FX/Screen Shake")]
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Main;
        public float pixelsPerUnit = 16f;
        [Tooltip("Accessibility: 0 disables shake (Celeste ships a toggle).")] [Range(0, 1)] public float intensity = 1f;
        float _amp, _dur, _t; Vector2 _kick; Vector3 _base; bool _hasBase;
        void OnEnable() { Main = this; }
        /// <summary>Random shake of up to <paramref name="pixels"/> art pixels, decaying over <paramref name="seconds"/> (unscaled, so it runs during hit-stop).</summary>
        public void Shake(float pixels, float seconds) { if (pixels >= _amp * (1 - _t / Mathf.Max(_dur, 1e-4f))) { _amp = pixels; _dur = seconds; _t = 0; } }
        /// <summary>Directional kick (Vlambeer "camera kick"), e.g. opposite to the shot.</summary>
        public void Kick(Vector2 pixels) { _kick += pixels; }
        public static void Add(float pixels, float seconds) { if (Main != null) Main.Shake(pixels, seconds); }
        void LateUpdate()
        {
            if (!_hasBase) { _base = transform.localPosition; _hasBase = true; }
            float u = 1f / pixelsPerUnit, dt = Time.unscaledDeltaTime;
            _t += dt;
            float a = _t < _dur ? _amp * (1 - _t / _dur) * intensity : 0;
            Vector2 off = new Vector2(Mathf.Round(Random.Range(-a, a)), Mathf.Round(Random.Range(-a, a))) + new Vector2(Mathf.Round(_kick.x), Mathf.Round(_kick.y)) * intensity;
            _kick = Vector2.Lerp(_kick, Vector2.zero, 1 - Mathf.Exp(-20 * dt));
            transform.localPosition = _base + (Vector3)(off * u);
        }
        /// <summary>Call if you move the camera yourself (sets the un-shaken position).</summary>
        public void SetBasePosition(Vector3 p) { _base = p; _hasBase = true; }
    }

    /// <summary>Afterimage / dash trail: spawns additive tinted copies of the sprite that fade out (Katana ZERO / Dead Cells dash).</summary>
    [AddComponentMenu("Pixel FX/Afterimage Trail")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class AfterimageTrail : MonoBehaviour
    {
        public bool emitting = false;
        [Tooltip("Seconds between ghosts.")] public float interval = 0.025f;
        public float lifetime = 0.25f;
        public Gradient color = DefaultGradient();
        public bool additive = true;
        SpriteRenderer _sr; float _acc;
        readonly List<(SpriteRenderer sr, float age)> _live = new List<(SpriteRenderer, float)>();
        readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();
        static Gradient DefaultGradient()
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(0.17f, 0.91f, 0.96f), 0), new GradientColorKey(new Color(0f, 0.6f, 0.86f), 0.5f), new GradientColorKey(new Color(0.15f, 0.17f, 0.27f), 1) },
                      new[] { new GradientAlphaKey(0.8f, 0), new GradientAlphaKey(0f, 1) });
            return g;
        }
        void Awake() { _sr = GetComponent<SpriteRenderer>(); }
        public void Burst(int count = 1) { for (int i = 0; i < count; i++) Spawn(); }
        void Update()
        {
            float dt = Time.deltaTime;
            if (emitting) { _acc += dt; while (_acc >= interval) { _acc -= interval; Spawn(); } }
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var (sr, age) = _live[i]; age += dt;
                if (age >= lifetime) { sr.gameObject.SetActive(false); _pool.Push(sr); _live.RemoveAt(i); continue; }
                float k = age / lifetime; k = Mathf.Floor(k * 4) / 4; // stepped fade reads more "pixel"
                sr.color = color.Evaluate(k);
                _live[i] = (sr, age);
            }
        }
        void Spawn()
        {
            if (_sr == null || _sr.sprite == null) return;
            var sr = _pool.Count > 0 ? _pool.Pop() : new GameObject("ghost").AddComponent<SpriteRenderer>();
            sr.gameObject.SetActive(true);
            sr.transform.SetPositionAndRotation(transform.position, transform.rotation);
            sr.transform.localScale = transform.lossyScale;
            sr.sprite = _sr.sprite; sr.flipX = _sr.flipX; sr.flipY = _sr.flipY;
            sr.sortingLayerID = _sr.sortingLayerID; sr.sortingOrder = _sr.sortingOrder - 1;
            var m = additive ? PixelFXMaterials.Additive : null;
            if (m != null) sr.sharedMaterial = m;
            sr.color = color.Evaluate(0);
            _live.Add((sr, 0));
        }
        void OnDestroy() { foreach (var l in _live) if (l.sr) Destroy(l.sr.gameObject); foreach (var p in _pool) if (p) Destroy(p.gameObject); }
    }

    /// <summary>Procedural pixel textures (no art files): banded glow discs, rings, slash crescents.</summary>
    public static class PixelFXTextures
    {
        static readonly float[,] Bayer = { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } };
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        static Sprite Make(string key, int w, int h, System.Func<int, int, Color> f, float ppu)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = key };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            t.SetPixels(px); t.Apply();
            return Cache[key] = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
        /// <summary>Radial glow with banded, Bayer-dithered falloff (pixel-art light pool). White; tint via SpriteRenderer.color.</summary>
        public static Sprite Glow(int radius, int bands = 5, float ppu = 16f) => Make($"glow{radius}_{bands}_{ppu}", radius * 2, radius * 2, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
            float f = Mathf.Clamp01(1 - d); f *= f;
            f = Mathf.Floor(f * bands + Bayer[y & 3, x & 3] / 16f * 0.9f) / bands;
            return new Color(1, 1, 1, f);
        }, ppu);
        /// <summary>1-px ring of the given radius (scale it up over time for a shockwave).</summary>
        public static Sprite Ring(int radius, int thickness = 2, float ppu = 16f) => Make($"ring{radius}_{thickness}_{ppu}", radius * 2 + 2, radius * 2 + 2, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius + 1, radius + 1));
            return Mathf.Abs(d - radius + thickness * 0.5f) <= thickness * 0.5f ? Color.white : Color.clear;
        }, ppu);
        /// <summary>Slash crescent frame (0..frames-1): white core, cyan rim, tapering ends. Pivot centre = swing origin.</summary>
        public static Sprite Slash(int frame, int frames = 4, int radius = 18, float ppu = 16f) => Make($"slash{frame}_{frames}_{radius}_{ppu}", radius * 2 + 8, radius * 2 + 8, (x, y) =>
        {
            float cx = radius + 4, cy = radius + 4, dx = x + 0.5f - cx, dy = y + 0.5f - cy;
            float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg, d = Mathf.Sqrt(dx * dx + dy * dy);
            float prog = Mathf.Min(1f, (frame + 1f) / Mathf.Max(1, frames - 1));
            float a0 = 110f, a1 = a0 - 200f * prog;
            if (ang > a0 || ang < a1 || dx < -6) return Color.clear;
            float tp = Mathf.Clamp01((a0 - ang) / (200f * prog));
            float thick = 4.5f * Mathf.Sin(tp * Mathf.PI) + 0.5f;
            if (frame == frames - 1) thick *= 0.45f; // last frame thins out
            float e = Mathf.Abs(d - radius);
            if (e < thick * 0.45f) return Color.white;
            if (e < thick) return new Color(0.17f, 0.91f, 0.96f, 1);
            return Color.clear;
        }, ppu);
    }

    /// <summary>Static spawners for one-shot additive effects.</summary>
    public static class PixelFXSpawn
    {
        static Material Add => PixelFXMaterials.Additive;
        static SpriteRenderer NewSR(string name, Sprite s, Vector3 pos, Color c, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.sprite = s; sr.color = c; sr.sortingOrder = order; sr.transform.position = pos;
            if (Add != null) sr.sharedMaterial = Add;
            return sr;
        }
        /// <summary>Expanding white ring that fades (shockwave). Real screen distortion needs a grab-pass; this is the glow-ring version.</summary>
        public static void Shockwave(Vector3 pos, float maxRadiusPx = 24, float seconds = 0.35f, int order = 60, float ppu = 16f)
        {
            var sr = NewSR("Shockwave", PixelFXTextures.Ring(8, 1, ppu), pos, Color.white, order);
            PixelJuiceRunner.I.StartCoroutine(CoRing(sr, maxRadiusPx / 8f, seconds));
        }
        static IEnumerator CoRing(SpriteRenderer sr, float maxScale, float d)
        {
            float t = 0;
            while (t < d && sr)
            {
                t += Time.deltaTime; float k = Mathf.Clamp01(t / d), e = 1 - (1 - k) * (1 - k);
                // integer-ish scale steps keep the ring chunky
                sr.transform.localScale = Vector3.one * Mathf.Max(0.25f, Mathf.Round(e * maxScale * 4) / 4);
                sr.color = new Color(1, 1, 1, Mathf.Floor((1 - k) * 4) / 4);
                yield return null;
            }
            if (sr) Object.Destroy(sr.gameObject);
        }
        /// <summary>Plays the 4-frame procedural slash arc at <paramref name="pos"/>; flip for left-facing swings.</summary>
        public static void Slash(Vector3 pos, bool flip = false, float frameSeconds = 0.033f, int radiusPx = 18, int order = 70, float ppu = 16f)
        {
            var sr = NewSR("Slash", PixelFXTextures.Slash(0, 4, radiusPx, ppu), pos, Color.white, order);
            sr.flipX = flip;
            PixelJuiceRunner.I.StartCoroutine(CoSlash(sr, frameSeconds, radiusPx, ppu));
        }
        static IEnumerator CoSlash(SpriteRenderer sr, float fs, int r, float ppu)
        {
            for (int f = 0; f < 4 && sr; f++) { sr.sprite = PixelFXTextures.Slash(f, 4, r, ppu); yield return new WaitForSeconds(fs); }
            for (int f = 0; f < 3 && sr; f++) { sr.color = new Color(1, 1, 1, 1 - (f + 1) / 4f); yield return new WaitForSeconds(fs); }
            if (sr) Object.Destroy(sr.gameObject);
        }
        /// <summary>Short additive light pop (muzzle-flash / impact light).</summary>
        public static void LightPop(Vector3 pos, Color c, int radiusPx = 24, float seconds = 0.15f, int order = 40, float ppu = 16f)
        {
            var sr = NewSR("LightPop", PixelFXTextures.Glow(radiusPx, 5, ppu), pos, c, order);
            PixelJuiceRunner.I.StartCoroutine(CoFade(sr, seconds, c));
        }
        static IEnumerator CoFade(SpriteRenderer sr, float d, Color c)
        {
            float t = 0;
            while (t < d && sr) { t += Time.deltaTime; float k = Mathf.Floor(Mathf.Clamp01(t / d) * 4) / 4; sr.color = new Color(c.r, c.g, c.b, c.a * (1 - k)); yield return null; }
            if (sr) Object.Destroy(sr.gameObject);
        }
    }

    /// <summary>Fake pixel light: additive banded glow sprite with optional flicker. Works with or without URP 2D lights.</summary>
    [AddComponentMenu("Pixel FX/Pixel Glow")]
    public class PixelGlow : MonoBehaviour
    {
        public Color color = new Color(0.97f, 0.46f, 0.13f, 0.8f);
        [Range(2, 128)] public int radiusPixels = 40;
        [Range(2, 8)] public int bands = 5;
        [Range(0, 0.5f)] public float flicker = 0.12f;
        public float pixelsPerUnit = 16f;
        public int sortingOrder = 30;
        SpriteRenderer _sr; float _seed;
        void OnEnable()
        {
            if (_sr == null) { _sr = new GameObject("glow").AddComponent<SpriteRenderer>(); _sr.transform.SetParent(transform, false); }
            _sr.sprite = PixelFXTextures.Glow(radiusPixels, bands, pixelsPerUnit); _sr.sortingOrder = sortingOrder;
            if (PixelFXMaterials.Additive != null) _sr.sharedMaterial = PixelFXMaterials.Additive;
            _seed = Random.value * 10;
        }
        void Update()
        {
            float t = Time.time + _seed;
            float f = 1 + flicker * (Mathf.Sin(t * 23) * 0.6f + Mathf.Sin(t * 37) * 0.4f);
            _sr.color = new Color(color.r, color.g, color.b, color.a * Mathf.Clamp01(f));
            float s = Mathf.Round(f * 8) / 8; _sr.transform.localScale = new Vector3(s, s, 1);
        }
    }

    /// <summary>Impact frame (anime/Katana ZERO style): 1-3 frames of black screen with the given sprites drawn as white silhouettes.</summary>
    public static class ImpactFrame
    {
        static SpriteRenderer _bg;
        public static void Play(Camera cam, float seconds, params PixelFX[] subjects)
        {
            if (cam == null) return;
            if (_bg == null)
            {
                _bg = new GameObject("ImpactFrameBG").AddComponent<SpriteRenderer>();
                _bg.sprite = PixelSpriteFactory.Rect(4, 4, new Color32(24, 20, 37, 255), 4f, "ImpactBG");
                _bg.sortingOrder = 900;
            }
            PixelJuiceRunner.I.StartCoroutine(Co(cam, seconds, subjects));
        }
        static IEnumerator Co(Camera cam, float d, PixelFX[] subs)
        {
            float h = cam.orthographicSize * 2, w = h * cam.aspect;
            _bg.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0);
            _bg.transform.localScale = new Vector3(w * 1.2f, h * 1.2f, 1);
            _bg.gameObject.SetActive(true);
            var orders = new int[subs.Length]; var sil = new float[subs.Length]; var silc = new Color[subs.Length];
            for (int i = 0; i < subs.Length; i++)
            {
                if (!subs[i]) continue; var sr = subs[i].SpriteRenderer;
                orders[i] = sr.sortingOrder; sil[i] = subs[i].silhouetteAmount; silc[i] = subs[i].silhouetteColor;
                sr.sortingOrder = 901; subs[i].silhouetteColor = Color.white; subs[i].silhouetteAmount = 1; subs[i].Apply();
            }
            yield return new WaitForSecondsRealtime(d);
            _bg.gameObject.SetActive(false);
            for (int i = 0; i < subs.Length; i++)
            {
                if (!subs[i]) continue;
                subs[i].SpriteRenderer.sortingOrder = orders[i]; subs[i].silhouetteAmount = sil[i]; subs[i].silhouetteColor = silc[i]; subs[i].Apply();
            }
        }
    }
}
