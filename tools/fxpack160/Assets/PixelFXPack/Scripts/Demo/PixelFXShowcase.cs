using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelFXPack.Demo
{
    /// <summary>
    /// v1.2/v1.3 showcase (v1.3: 2x characters, scaled slash, G = gallery of every effect): a looping game moment built entirely in code (hand-placed pixel maps + procedural textures, no AI art).
    /// Moody night ruins with parallax and torch glow; the knight dashes (afterimages), slashes the slime (slash arc, hit-stop,
    /// impact frame, white flash, glow sparks, shockwave, shake, knockback), kills it (glowing dissolve + embers) and coins burst out.
    /// Keys: 1 dash, 2 slash, 3 finisher, Space toggles auto-loop, S toggles shake, G gallery (arrows step). IMGUI buttons do the same.
    /// UNTESTED IN EDITOR - written against the Unity 6 API and compile-checked only. See README "Known limits".
    /// </summary>
    [AddComponentMenu("Pixel FX/Demo/Pixel FX Showcase")]
    public partial class PixelFXShowcase : MonoBehaviour
    {
        public float pixelsPerUnit = 16f;
        public Material uberMaterial;
        [Tooltip("Art resolution; the camera uses the largest whole-number zoom that fits.")]
        public Vector2Int referenceResolution = new Vector2Int(240, 135);
        public bool autoLoop = true;
        [Tooltip("v1.3: character scale in art pixels (2 = each sprite pixel is 2x2).")] public int characterScale = 2;

        const int GY = 105; // ground line in art pixels (y down, like the mock-up)
        float U => 1f / pixelsPerUnit;
        Vector3 P(float px, float py) => new Vector3((px - referenceResolution.x * 0.5f) * U, (referenceResolution.y * 0.5f - py) * U, 0);

        static Color32 C(string h) => new Color32(System.Convert.ToByte(h.Substring(0, 2), 16), System.Convert.ToByte(h.Substring(2, 2), 16), System.Convert.ToByte(h.Substring(4, 2), 16), 255);
        static readonly Color32 INK = C("181425"), NAVY = C("262b44"), SLATE = C("3a4466"), STEEL = C("5a6988"), PURP = C("3e2731"), PLUM = C("68386c"), MAG = C("b55088"),
            CYAN = C("2ce8f5"), ORANGE = C("f77622"), AMBER = C("feae34"), YEL = C("fee761"), MOON = C("f6d6bd"), MOOND = C("c28569"), MOSS = C("3e8948");
        static readonly int[,] Bayer = { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } };

        Camera _cam; int _scale = 1; Vector3 _camBase;
        Transform _knight, _slime; SpriteRenderer _knightSR, _slimeSR; PixelFX _knightFX, _slimeFX; AfterimageTrail _trail;
        readonly List<(Transform t, float factor, float baseX)> _parallax = new List<(Transform, float, float)>();
        Sprite _coin; readonly List<Coin> _coins = new List<Coin>();
        class Coin { public Transform t; public SpriteRenderer sr; public Vector2 pos, vel; public float age, phase; }
        float _kx, _slx, _slvx, _squash, _flash, _stretch; bool _dead; Coroutine _loop; bool _busy; float _emberT;
        string _caption = "";

        void Start()
        {
            Application.targetFrameRate = 60;
            _cam = Camera.main;
            if (_cam == null) { _cam = new GameObject("Main Camera").AddComponent<Camera>(); _cam.tag = "MainCamera"; }
            _cam.orthographic = true; _cam.clearFlags = CameraClearFlags.SolidColor; _cam.backgroundColor = INK;
            var shake = _cam.GetComponent<ScreenShake>();
            if (shake == null) shake = _cam.gameObject.AddComponent<ScreenShake>();
            shake.pixelsPerUnit = pixelsPerUnit;
            UpdateCamera();
            var pp = PixelParticles.Instance; pp.pixelsPerUnit = pixelsPerUnit; pp.sortingOrder = 80;
            BuildWorld();
            ResetMoment();
            if (autoLoop) _loop = StartCoroutine(Loop());
        }

        // ---------------- world building (procedural textures) ----------------
        SpriteRenderer Layer(string name, Color32[] px, int w, int h, Vector3 pos, int order, Vector2? pivot = null)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = name };
            t.SetPixels32(px); t.Apply();
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(t, new Rect(0, 0, w, h), pivot ?? new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sr.sortingOrder = order; sr.transform.position = pos; sr.transform.SetParent(transform, true);
            return sr;
        }
        static Color32 Lerp(Color32 a, Color32 b, float k) => Color32.Lerp(a, b, k);
        static Color32 Mul(Color32 a, float k) => new Color32((byte)Mathf.Min(255, a.r * k), (byte)Mathf.Min(255, a.g * k), (byte)Mathf.Min(255, a.b * k), a.a);

        void BuildWorld()
        {
            int W = referenceResolution.x, H = referenceResolution.y, W2 = W * 2;
            var rnd = new System.Random(7);
            // sky: ordered-dither gradient + stars + moon (y down in our loops; texture rows are bottom-up)
            var sky = new Color32[W * H];
            var stops = new (float, Color32)[] { (0, INK), (.35f, PURP), (.62f, PLUM), (.8f, Lerp(PLUM, MAG, .7f)), (1.01f, Lerp(PLUM, MAG, .7f)) };
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                float t = y / (float)H; int i = 0; while (t >= stops[i + 1].Item1) i++;
                float k = (t - stops[i].Item1) / (stops[i + 1].Item1 - stops[i].Item1);
                sky[(H - 1 - y) * W + x] = k > Bayer[y & 3, x & 3] / 16f ? stops[i + 1].Item2 : stops[i].Item2;
            }
            for (int s = 0; s < 45; s++) { int x = rnd.Next(W), y = rnd.Next(60); sky[(H - 1 - y) * W + x] = Mul(new Color32(255, 255, 255, 255), new[] { .5f, .8f, 1f }[rnd.Next(3)]); }
            for (int y = 19; y < 42; y++) for (int x = 37; x < 60; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(48, 30));
                if (d < 11) sky[(H - 1 - y) * W + x] = (x - 48 > 3 && y > 27) ? MOOND : MOON;
            }
            Layer("Sky", sky, W, H, Vector3.zero, -100);
            var moonGlow = new GameObject("MoonGlow").AddComponent<PixelGlow>();
            moonGlow.transform.position = P(48, 30); moonGlow.color = new Color(0.96f, 0.84f, 0.74f, 0.35f); moonGlow.radiusPixels = 26; moonGlow.flicker = 0; moonGlow.sortingOrder = -99; moonGlow.pixelsPerUnit = pixelsPerUnit;

            // parallax ridges + pillars (tileable-ish, 2x screen width)
            int[] Ridge(int seed, float b, float amp, float fr)
            {
                var r = new System.Random(seed); var ph = new float[4]; for (int i = 0; i < 4; i++) ph[i] = (float)r.NextDouble() * 6;
                var h = new int[W2];
                for (int x = 0; x < W2; x++) { float v = b; for (int i = 0; i < 4; i++) v += amp / (i + 1) * Mathf.Sin(x * fr * (i + 1.7f) + ph[i]); h[x] = Mathf.RoundToInt(v); }
                return h;
            }
            void RidgeLayer(string n, int[] h, Color32 col, int order, float factor)
            {
                var px = new Color32[W2 * H];
                for (int x = 0; x < W2; x++) for (int y = h[x]; y < H; y++) px[(H - 1 - y) * W2 + x] = col;
                var sr = Layer(n, px, W2, H, new Vector3(W * 0.5f * U, 0, 0), order);
                _parallax.Add((sr.transform, factor, sr.transform.position.x));
            }
            RidgeLayer("FarRidge", Ridge(1, 72, 14, .02f), Mul(PLUM, .55f), -90, .25f);
            RidgeLayer("MidRidge", Ridge(2, 86, 10, .045f), Mul(PURP, .9f), -80, .5f);
            var pil = new Color32[W2 * H]; var pc = Mul(NAVY, .75f);
            for (int px0 = 10; px0 < W2; px0 += 70)
            {
                int top = 40 + (px0 * 37) % 30;
                for (int y = top - 4; y < GY; y++)
                {
                    int x0 = y < top ? px0 - 2 : px0, x1 = y < top ? px0 + 14 : px0 + 12;
                    for (int x = Mathf.Max(0, x0); x < Mathf.Min(W2, x1); x++) pil[(H - 1 - y) * W2 + x] = pc;
                }
                if ((px0 / 70) % 2 == 0)
                    for (int x = px0 + 12; x < Mathf.Min(W2, px0 + 58); x++) { int yv = (int)(top + 8 - 10 * Mathf.Sin((x - px0 - 12) / 46f * Mathf.PI)); for (int y = yv; y < yv + 5; y++) pil[(H - 1 - y) * W2 + x] = pc; }
            }
            var ps = Layer("Pillars", pil, W2, H, new Vector3(W * 0.5f * U, 0, 0), -70); _parallax.Add((ps.transform, .75f, ps.transform.position.x));
            // ground bricks with moss lip
            int GH = H - GY; var gp = new Color32[W2 * GH];
            for (int y = 0; y < GH; y++) for (int x = 0; x < W2; x++)
            {
                int row = y / 6, bx = (x + (row % 2) * 8) / 16; bool edge = y % 6 == 0 || (x + (row % 2) * 8) % 16 == 0;
                Color32 b = (bx * 7 + row * 3) % 5 != 0 ? SLATE : Mul(STEEL, .8f);
                Color32 c = edge ? Mul(INK, 1.4f) : Mul(b, 1.25f * (1 - y / (float)GH * .5f));
                if (y == 0) c = Mul(C("63c74d"), .55f); else if (y == 1) c = Mul(MOSS, .6f);
                gp[(GH - 1 - y) * W2 + x] = c;
            }
            var g = Layer("Ground", gp, W2, GH, new Vector3(W * 0.5f * U, P(0, GY).y, 0), -60, new Vector2(0.5f, 1f)); _parallax.Add((g.transform, 1f, g.transform.position.x));
            // torch pillar + flame + glow + embers
            var tp = new Color32[12 * (GY - 68)];
            for (int y = 0; y < GY - 68; y++) for (int x = 0; x < 12; x++) if (y < 3 || (x >= 2 && x < 10)) tp[(GY - 68 - 1 - y) * 12 + x] = y < 3 ? Mul(STEEL, .7f) : Mul(NAVY, .9f);
            Layer("TorchPillar", tp, 12, GY - 68, P(195, 68), -50, new Vector2(0.5f, 1f));
            var torch = new GameObject("TorchLight").AddComponent<PixelGlow>(); torch.transform.position = P(195, 64);
            torch.color = new Color(0.97f, 0.46f, 0.13f, 0.55f); torch.radiusPixels = 60; torch.sortingOrder = -40; torch.pixelsPerUnit = pixelsPerUnit;
            var flame = new GameObject("TorchFlame").AddComponent<PixelGlow>(); flame.transform.position = P(195, 63);
            flame.color = new Color(1f, 0.91f, 0.38f, 1f); flame.radiusPixels = 5; flame.bands = 3; flame.flicker = 0.3f; flame.sortingOrder = 85; flame.pixelsPerUnit = pixelsPerUnit;
            var vign = new GameObject("Vignette").AddComponent<SpriteRenderer>(); // dark edges (alpha-blended)
            var vp = new Color32[W * H];
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                float v = (x - W / 2f) * (x - W / 2f) / (W * .7f * W * .7f) + (y - H / 2f) * (y - H / 2f) / (H * .75f * H * .75f);
                float a = Mathf.Clamp01(v - 0.05f) * 0.9f; a = Mathf.Floor(a * 6 + Bayer[y & 3, x & 3] / 16f) / 6;
                vp[(H - 1 - y) * W + x] = new Color32(INK.r, INK.g, INK.b, (byte)(a * 255));
            }
            var vt = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; vt.SetPixels32(vp); vt.Apply();
            vign.sprite = Sprite.Create(vt, new Rect(0, 0, W, H), new Vector2(.5f, .5f), pixelsPerUnit); vign.sortingOrder = 500; vign.transform.SetParent(_cam.transform, false); vign.transform.localPosition = new Vector3(0, 0, 10);

            // characters: root at the feet so squash/stretch keeps them grounded
            _knightSR = Character("Knight", PixelFXDemo.Knight, out _knight, out _knightFX, 10);
            _slimeSR = Character("Slime", PixelFXDemo.Slime, out _slime, out _slimeFX, 9);
            _knightFX.rimAmount = 0.6f; _knightFX.rimColor = new Color(1f, 0.68f, 0.2f, 1); _knightFX.rimDirection = new Vector2Int(1, 1);
            _slimeFX.rimAmount = 0.5f; _slimeFX.rimColor = new Color(1f, 0.68f, 0.2f, 1); _slimeFX.rimDirection = new Vector2Int(1, 1);
            _slimeFX.dissolveEdgeColor = new Color(1f, 0.68f, 0.2f, 1); _slimeFX.dissolveEdgeWidth = 0.12f; _slimeFX.dissolveEdgeGlow = 2.5f;
            _trail = _knightSR.gameObject.AddComponent<AfterimageTrail>(); _trail.interval = 0.03f; _trail.lifetime = 0.22f;
            BuildGallery();
            _coin = PixelSpriteFactory.FromRows(PixelFXDemo.Coin, PixelFXDemo.Pal, pixelsPerUnit, 1, "Coin");
        }

        SpriteRenderer Character(string n, string[] rows, out Transform root, out PixelFX fx, int order)
        {
            root = new GameObject(n).transform; root.SetParent(transform, false);
            var s = PixelSpriteFactory.FromRows(rows, PixelFXDemo.Pal, pixelsPerUnit, 2, n);
            var sr = new GameObject(n + "Sprite").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(root, false);
            int h = rows.Length + 4; sr.transform.localPosition = new Vector3(0, (Mathf.Floor(h * 0.5f) - 2) * U, 0);
            sr.sprite = s; sr.sortingOrder = order;
            if (uberMaterial != null) sr.sharedMaterial = uberMaterial;
            sr.color = new Color(0.95f, 0.9f, 1f); // slight moonlit tint
            fx = sr.gameObject.AddComponent<PixelFX>();
            return sr;
        }

        // ---------------- moment ----------------
        void ResetMoment()
        {
            _kx = 36; _slx = 168; _slvx = 0; _dead = false; _squash = 0; _flash = 0;
            _slimeFX.dissolveAmount = 0; _slimeSR.enabled = true;
            foreach (var c in _coins) if (c.t) Destroy(c.t.gameObject);
            _coins.Clear();
        }

        IEnumerator Loop()
        {
            while (true)
            {
                ResetMoment(); _caption = "";
                yield return Spawn();
                yield return new WaitForSeconds(0.35f);
                yield return Dash();
                yield return new WaitForSeconds(0.12f);
                yield return Attack(false);
                yield return new WaitForSeconds(0.7f);
                yield return Attack(true);
                yield return new WaitForSeconds(2.6f);
            }
        }
        IEnumerator Spawn()
        {
            _caption = "spawn: dissolve-in";
            float t = 0; while (t < 0.4f) { t += Time.deltaTime; _slimeFX.dissolveAmount = 1 - Mathf.Floor(t / 0.4f * 10) / 10; _squash = -0.5f * (1 - t / 0.4f); yield return null; }
            _slimeFX.dissolveAmount = 0;
            PixelParticles.Burst(PixelBurstSettings.Dust(), (Vector2)P(_slx, GY));
        }
        IEnumerator Dash()
        {
            _busy = true; _caption = "dash: afterimages + stretch";
            float target = Mathf.Min(_slx - 34, _kx + 110), start = _kx, t = 0, d = 0.32f;
            _trail.emitting = true;
            while (t < d)
            {
                t += Time.deltaTime; float k = Mathf.Clamp01(t / d);
                _kx = Mathf.Lerp(start, target, 1 - (1 - k) * (1 - k) * (1 - k)); _stretch = k < .85f ? .18f : 0;
                if (Random.value < .35f) PixelParticles.Burst(PixelBurstSettings.Dust(), (Vector2)P(_kx - 12, GY));
                yield return null;
            }
            _stretch = 0; _trail.emitting = false; _busy = false;
        }
        IEnumerator Attack(bool kill)
        {
            if (_dead) yield break;
            _busy = true; _caption = kill ? "finisher: impact frame, hit-stop + slow-mo, shockwave, dissolve" : "hit: slash arc, hit-stop, flash, sparks, shake, knockback";
            _kx = Mathf.Max(_kx, _slx - 40);
            PixelFXSpawn.Slash(P(_kx + 14, GY - 16), false, 0.03f, 16, 70, pixelsPerUnit); // v1.3: arc sized to the 2x knight
            PixelFXSpawn.LightPop(P(_kx + 24, GY - 16), new Color(0.17f, 0.91f, 0.96f, 0.6f), 28, 0.15f, 45, pixelsPerUnit);
            _kx += 3;
            yield return new WaitForSeconds(0.06f);
            Vector3 hit = P(_slx - 12, GY - 12);
            ImpactFrame.Play(_cam, kill ? 0.06f : 0.035f, _knightFX, _slimeFX);
            HitStop.Play(kill ? 0.11f : 0.06f, kill ? 0.3f : 1f, kill ? 0.4f : 0f);
            ScreenShake.Add(kill ? 5 : 3, kill ? 0.35f : 0.2f);
            if (ScreenShake.Main) ScreenShake.Main.Kick(new Vector2(kill ? 3 : 2, 0));
            var sp = PixelBurstSettings.GlowSparks(); if (kill) sp.count = 34;
            PixelParticles.Burst(sp, (Vector2)hit);
            var deb = PixelBurstSettings.Dust(); deb.gravity = -20; deb.directionDegrees = 45; deb.spreadDegrees = 70; deb.speed = new Vector2(2, 5);
            PixelParticles.Burst(deb, (Vector2)hit);
            PixelFXSpawn.Shockwave(hit, kill ? 30 : 20, kill ? 0.4f : 0.3f, 60, pixelsPerUnit);
            PixelFXSpawn.LightPop(hit, new Color(1, 1, 1, 0.8f), 40, 0.2f, 46, pixelsPerUnit);
            _flash = 1; _squash = 1; _slvx = kill ? 70 : 140; // px/s knockback
            if (kill) { _dead = true; StartCoroutine(Die()); }
            _busy = false;
        }
        IEnumerator Die()
        {
            float t = 0, d = 0.85f; bool coinsOut = false;
            yield return new WaitForSeconds(0.08f);
            while (t < d)
            {
                t += Time.deltaTime; _slimeFX.dissolveAmount = Mathf.Floor(Mathf.Clamp01(t / d) * 26) / 26;
                if (Random.value < .6f) { var e = PixelBurstSettings.Embers(); e.count = 2; PixelParticles.Burst(e, (Vector2)P(_slx + Random.Range(-12f, 12f), GY - Random.Range(2f, 22f))); }
                if (!coinsOut && t > d * .15f) { coinsOut = true; SpawnCoins(9); }
                yield return null;
            }
            _slimeSR.enabled = false;
        }
        void SpawnCoins(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var sr = new GameObject("Coin").AddComponent<SpriteRenderer>(); sr.sprite = _coin; sr.sortingOrder = 12; sr.transform.SetParent(transform, false);
                if (uberMaterial != null) sr.sharedMaterial = uberMaterial;
                var fx = sr.gameObject.AddComponent<PixelFX>(); fx.emissionAmount = 0.15f; fx.emissionColor = new Color(1f, 0.85f, 0.4f);
                var glow = new GameObject("glow").AddComponent<SpriteRenderer>(); glow.transform.SetParent(sr.transform, false);
                glow.sprite = PixelFXTextures.Glow(8, 3, pixelsPerUnit); glow.color = new Color(1f, 0.8f, 0.3f, 0.35f); glow.sortingOrder = 11;
                if (PixelFXMaterials.Additive) glow.sharedMaterial = PixelFXMaterials.Additive;
                _coins.Add(new Coin { t = sr.transform, sr = sr, pos = new Vector2(_slx + Random.Range(-8f, 8f), GY - 14), vel = new Vector2(Random.Range(-70f, 70f), Random.Range(-170f, -110f)), phase = Random.value * 4 });
            }
        }

        // ---------------- per-frame ----------------
        void Update()
        {
            float dt = Time.deltaTime;
            GalleryUpdate(dt);
            _slx += _slvx * dt; _slvx *= Mathf.Exp(-9 * dt);
            _squash = Mathf.MoveTowards(_squash, 0, dt * 5);
            _flash = Mathf.Max(0, _flash - dt * 7);
            _slimeFX.flashAmount = _flash > 0.4f ? 1 : 0; // stepped: full white, then off
            float sq = _squash > 0 ? _squash * .35f : _squash;
            float cs = characterScale;
            _slime.localScale = new Vector3((1 + sq) * cs, (1 - sq * .85f) * cs, 1);
            _knight.localScale = new Vector3((1 + _stretch) * cs, cs / (1 + _stretch), 1);
            _knight.position = Snap(P(_kx, GY)); _slime.position = Snap(P(_slx, GY));
            foreach (var c in _coins)
            {
                c.age += dt; c.vel.y += 420 * dt; c.pos += c.vel * dt;
                if (c.pos.y > GY - 6) { c.pos.y = GY - 6; c.vel.y *= -0.45f; c.vel.x *= 0.7f; }
                c.t.position = Snap(P(c.pos.x, c.pos.y));
                int f = (int)((Time.time * 10 + c.phase)) % 4; c.t.localScale = new Vector3(new[] { 1f, .66f, .33f, .66f }[f] * cs, cs, 1);
                if (Random.value < dt * 2.5f) PixelParticles.Burst(PixelBurstSettings.Sparkle(), (Vector2)P(c.pos.x, c.pos.y - 6));
            }
            _emberT += dt; if (_emberT > 0.08f) { _emberT = 0; PixelParticles.Burst(PixelBurstSettings.Embers(), (Vector2)P(195 + Random.Range(-2f, 2f), 66)); }
            // camera follows the knight a little; parallax layers move by factor
            float camx = _gallery ? 0 : (_kx - 36) * 0.3f * U;
            if (ScreenShake.Main) ScreenShake.Main.SetBasePosition(_camBase + new Vector3(camx, 0, 0));
            foreach (var (t, f, bx) in _parallax) t.position = new Vector3(Mathf.Round((bx + camx * (1 - f)) / U) * U, t.position.y, t.position.z);
        }
        Vector3 Snap(Vector3 v) => new Vector3(Mathf.Round(v.x / U) * U, Mathf.Round(v.y / U) * U, 0);

        void UpdateCamera()
        {
            int sw = Screen.width, sh = Screen.height;
            _scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(sw / (float)referenceResolution.x, sh / (float)referenceResolution.y)));
            _cam.orthographicSize = sh / (2f * pixelsPerUnit * _scale);
            float ox = (sw / _scale) % 2 == 1 ? 0.5f : 0f, oy = (sh / _scale) % 2 == 1 ? 0.5f : 0f;
            _camBase = new Vector3(ox * U, oy * U, -10); _cam.transform.position = _camBase;
        }
        int _lw, _lh;
        void LateUpdate() { if (Screen.width != _lw || Screen.height != _lh) { _lw = Screen.width; _lh = Screen.height; UpdateCamera(); } }

        // ---------------- UI (IMGUI: works with either input backend) ----------------
        void Manual(IEnumerator r)
        {
            if (_gallery) return;
            if (_busy) return;
            if (_loop != null) { StopCoroutine(_loop); _loop = null; autoLoop = false; }
            if (_dead && !_slimeSR.enabled) { ResetMoment(); StartCoroutine(Spawn()); return; }
            StartCoroutine(r);
        }
        void ToggleAuto()
        {
            autoLoop = !autoLoop;
            if (autoLoop && _loop == null) _loop = StartCoroutine(Loop());
            if (!autoLoop && _loop != null) { StopCoroutine(_loop); _loop = null; }
        }
        void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Alpha1) Manual(Dash());
                if (e.keyCode == KeyCode.Alpha2) Manual(Attack(false));
                if (e.keyCode == KeyCode.Alpha3) Manual(Attack(true));
                if (e.keyCode == KeyCode.Space) ToggleAuto();
                if (e.keyCode == KeyCode.G) ToggleGallery();
                if (_gallery && e.keyCode == KeyCode.RightArrow) GalleryStep(1);
                if (_gallery && e.keyCode == KeyCode.LeftArrow) GalleryStep(-1);
                if (e.keyCode == KeyCode.S && ScreenShake.Main) ScreenShake.Main.intensity = ScreenShake.Main.intensity > 0 ? 0 : 1;
            }
            float s = Mathf.Max(1, _scale * 0.5f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1));
            GUILayout.BeginArea(new Rect(8, 8, Screen.width / s - 16, 60));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_gallery ? "G: Fight" : "G: Gallery")) ToggleGallery();
            if (!_gallery && GUILayout.Button("1 Dash")) Manual(Dash());
            if (GUILayout.Button("2 Slash")) Manual(Attack(false));
            if (GUILayout.Button("3 Finisher")) Manual(Attack(true));
            if (GUILayout.Button(autoLoop ? "Space: Auto ON" : "Space: Auto OFF")) ToggleAuto();
            if (ScreenShake.Main && GUILayout.Button(ScreenShake.Main.intensity > 0 ? "S: Shake ON" : "S: Shake OFF")) ScreenShake.Main.intensity = ScreenShake.Main.intensity > 0 ? 0 : 1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label(_gallery ? "" : _caption);
            GUILayout.EndArea();
            GalleryGUI(s);
        }
    }
}
