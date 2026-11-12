using System.Collections.Generic;
using UnityEngine;

namespace PixelFXPack.Demo
{
    /// <summary>
    /// v1.1 showcase demo, built entirely from code (sprites are hand-placed ASCII pixel maps, Endesga 32 palette - no AI art).
    /// Pixel-perfect camera with integer scaling (480x270 reference), card layout, auto-playing effect previews.
    /// Click a card to trigger its effect, click the particle buttons, or click empty space for an explosion.
    /// UI is IMGUI so it works with both input backends. Drop on an empty GameObject in any scene and press Play.
    /// </summary>
    [AddComponentMenu("Pixel FX/Demo/Pixel FX Demo")]
    public class PixelFXDemo : MonoBehaviour
    {
        public float pixelsPerUnit = 16f;
        [Tooltip("Optional. If empty, the material in Resources/PixelFX is used (it guarantees the shader ships in builds).")]
        public Material uberMaterial;
        [Tooltip("Reference resolution in art pixels; the camera picks the largest whole-number zoom that fits.")]
        public Vector2Int referenceResolution = new Vector2Int(480, 270);

        internal static readonly string[] Knight =
        {
            "......kkk.......",
            ".....krrRk......",
            "....kkkrRkk.....",
            "...kswssSSDk....",
            "..kswsssSSDDk...",
            "..kssssssSDDk...",
            "..kSkkkkkkkDk.k.",
            "..kSkkckkckDkkwk",
            "..kSSSkkkSSDkkSk",
            "...kDDDDDDdk.kSk",
            "..knbbbybbnnkkSk",
            ".kskbbyyybnkskSk",
            ".kSDkbbybnnkDkyk",
            "..kk.knnnnnkkoOk",
            "....kDdkkDdk.kk.",
            "....kkkk.kkkk...",
        };
        internal static readonly string[] Slime =
        {
            "......kkkk......",
            "....kkLLllkk....",
            "...kLwwLllllk...",
            "..kLwLllllllgk..",
            "..kLllllllllgk..",
            ".kllkkllllkklgk.",
            ".kllkwklllkwkgk.",
            ".klllkkllllkkggk",
            ".kglllllrrllgggk",
            ".kgglllllllggGGk",
            ".kGggggggggggGGk",
            "..kGGGGGGGGGGhk.",
            "...kkkkkkkkkkk..",
        };
        internal static readonly string[] Coin =
        {
            "....kkkkk...",
            "..kkyyyyokk.",
            ".kywyyyyyook",
            ".kwyyooyyyOk",
            "kyyyoykoyyOk",
            "kyyyoykoyyOk",
            "kyyyoykoyyOk",
            "kyyyoykoyyOk",
            ".kyyyookyyOk",
            ".kyoyyyyyOOk",
            "..kkoOOOOkk.",
            "....kkkkk...",
        };

        internal static readonly Dictionary<char, Color32> Pal = new Dictionary<char, Color32>
        {
            { 'k', new Color32(24, 20, 37, 255) },
            { 'K', new Color32(38, 43, 68, 255) },
            { 'w', new Color32(255, 255, 255, 255) },
            { 's', new Color32(192, 203, 220, 255) },
            { 'S', new Color32(139, 155, 180, 255) },
            { 'D', new Color32(90, 105, 136, 255) },
            { 'd', new Color32(58, 68, 102, 255) },
            { 'r', new Color32(228, 59, 68, 255) },
            { 'R', new Color32(162, 38, 51, 255) },
            { 'y', new Color32(254, 231, 97, 255) },
            { 'o', new Color32(254, 174, 52, 255) },
            { 'O', new Color32(247, 118, 34, 255) },
            { 'B', new Color32(184, 111, 80, 255) },
            { 'l', new Color32(99, 199, 77, 255) },
            { 'g', new Color32(62, 137, 72, 255) },
            { 'G', new Color32(38, 92, 66, 255) },
            { 'h', new Color32(25, 60, 62, 255) },
            { 'L', new Color32(184, 240, 140, 255) },
            { 'b', new Color32(0, 153, 219, 255) },
            { 'n', new Color32(18, 78, 137, 255) },
            { 'c', new Color32(44, 232, 245, 255) }
        };

        static readonly Color32 BgCol = new Color32(24, 20, 37, 255), BgDot = new Color32(38, 43, 68, 255);
        static readonly Color32 CardCol = new Color32(38, 43, 68, 255), CardEdge = new Color32(58, 68, 102, 255), CardHi = new Color32(90, 105, 136, 255);
        static readonly Color32 StripCol = new Color32(24, 20, 37, 255), Accent = new Color32(254, 174, 52, 255);
        const int ArtScale = 3, CardW = 88, CardH = 100, StripH = 16, Gap = 6;

        class Tile
        {
            public string label, hint; public PixelFX fx; public SpriteRenderer card; public Transform art;
            public Vector3 center; public System.Action click; public System.Action<float> tick; public int baseY; public bool bob = true;
        }
        readonly List<Tile> _tiles = new List<Tile>();
        readonly List<(string label, Vector3 pos, System.Action<Vector2> fire)> _buttons = new List<(string, Vector3, System.Action<Vector2>)>();
        Camera _cam; int _scale = 1;
        Sprite _slime, _knight, _coin, _card, _cardHover, _panel;
        Texture2D _flat, _flatHover, _flatAccent;
        Material _mat;
        float U => 1f / pixelsPerUnit;

        void Start()
        {
            _mat = uberMaterial != null ? uberMaterial : PixelFXMaterials.Shared;
            if (_mat == null) Debug.LogError("[PixelFX Demo] Uber shader missing: effects will not show. Re-import Assets/PixelFXPack/Resources/PixelFX/PixelFX_Uber.mat.");
            _slime = PixelSpriteFactory.FromRows(Slime, Pal, pixelsPerUnit, 3, "Slime");
            _knight = PixelSpriteFactory.FromRows(Knight, Pal, pixelsPerUnit, 3, "Knight");
            _coin = PixelSpriteFactory.FromRows(Coin, Pal, pixelsPerUnit, 3, "Coin");
            _card = MakeCard(CardW, CardH, CardEdge, "Card");
            _cardHover = MakeCard(CardW, CardH, Accent, "CardHover");
            _panel = MakeCard(CardW * 5 + Gap * 4, 30, CardEdge, "Panel", false);

            _cam = Camera.main;
            if (_cam == null) { _cam = new GameObject("Main Camera").AddComponent<Camera>(); _cam.tag = "MainCamera"; }
            _cam.orthographic = true; _cam.transform.position = new Vector3(0, 0, -10);
            _cam.clearFlags = CameraClearFlags.SolidColor; _cam.backgroundColor = BgCol;
            UpdateCamera();

            var bg = new GameObject("Background").AddComponent<SpriteRenderer>();
            bg.sprite = MakeBackground(referenceResolution.x * 2, referenceResolution.y * 2); bg.sortingOrder = -20;

            var amber = new Color32(254, 231, 97, 255);
            // Row 1 (card centres in art pixels)
            float r1 = 58, r2 = -48;
            Add("OUTLINE", "outer, 1-4 px", _slime, Col(0), r1, fx => { fx.outlineSize = 1; fx.outlineColor = Color.white; },
                (t, time) => t.fx.outlineColor = Step(time, 0.35f) ? (Color)new Color32(255, 255, 255, 255) : (Color)amber,
                t => t.fx.outlineSize = t.fx.outlineSize >= 2 ? 1 : 2);
            Add("INNER OUTLINE", "rim light", _knight, Col(1), r1, fx => { fx.outlineSize = 1; fx.innerOutline = true; fx.outlineColor = amber; },
                (t, time) => t.fx.outlineColor = Step(time, 0.35f) ? (Color)amber : (Color)new Color32(44, 232, 245, 255), null);
            Add("HIT FLASH", "+ sparks", _slime, Col(2), r1, fx => { var h = fx.gameObject.AddComponent<HitFlash>(); h.blinks = 2; h.duration = 0.16f; },
                Every(1.4f, t => Hit(t)), t => Hit(t));
            Add("DISSOLVE", "pixel burn", _knight, Col(3), r1, fx => { var d = fx.gameObject.AddComponent<DissolveFX>(); d.cellSize = 1; d.duration = 0.7f; fx.dissolveEdgeWidth = 0.1f; },
                Every(1.3f, t => ToggleDissolve(t)), t => ToggleDissolve(t), bob: false);
            Add("PALETTE SWAP", "LUT recolour", _slime, Col(4), r1, fx =>
            {
                var ps = fx.gameObject.AddComponent<PaletteSwap>();
                ps.enabled = false;
                ps.sourceColors = new Color[] { (Color)Pal['L'], (Color)Pal['l'], (Color)Pal['g'], (Color)Pal['G'], (Color)Pal['h'] };
                ps.palettes = new[]
                {
                    new PaletteSwap.Palette { name = "Fire", colors = C(new Color32(254, 231, 97, 255), new Color32(247, 118, 34, 255), new Color32(228, 59, 68, 255), new Color32(162, 38, 51, 255), new Color32(62, 39, 49, 255)) },
                    new PaletteSwap.Palette { name = "Ice", colors = C(new Color32(255, 255, 255, 255), new Color32(44, 232, 245, 255), new Color32(0, 153, 219, 255), new Color32(18, 78, 137, 255), new Color32(24, 20, 37, 255)) },
                    new PaletteSwap.Palette { name = "Void", colors = C(new Color32(246, 117, 122, 255), new Color32(181, 80, 136, 255), new Color32(104, 56, 108, 255), new Color32(62, 39, 49, 255), new Color32(24, 20, 37, 255)) },
                };
                ps.enabled = true;
            }, Every(0.9f, t => t.fx.GetComponent<PaletteSwap>().Next()), t => t.fx.GetComponent<PaletteSwap>().Next());

            // Row 2
            Add("SILHOUETTE", "flat colour", _knight, Col(0), r2, fx => { fx.silhouetteColor = new Color32(44, 232, 245, 200); },
                (t, time) => t.fx.silhouetteAmount = Step(time, 0.6f) ? 1 : 0, null);
            Add("WAVE", "wobble / heat", _slime, Col(1), r2, fx => { fx.waveAmplitude = 1; fx.waveFrequency = 0.9f; fx.waveSpeed = 6; }, null,
                t => t.fx.waveAmplitude = t.fx.waveAmplitude >= 2 ? 1 : 2);
            Add("SHINE", "sweep", _coin, Col(2), r2, fx => { var s = fx.gameObject.AddComponent<ShineSweep>(); s.loopInterval = 0.9f; s.sweepTime = 0.4f; fx.shineWidth = 0.18f; },
                null, t => { t.fx.GetComponent<ShineSweep>().Play(); PixelParticles.Burst(PixelBurstSettings.Sparkle(), t.art.position); });
            int[] pix = { 1, 2, 3, 4, 3, 2 };
            Add("PIXELATE", "mosaic", _knight, Col(3), r2, null, (t, time) => t.fx.pixelateSize = pix[(int)(time / 0.22f) % pix.Length], null);
            Add("DROP SHADOW", "hard offset", _coin, Col(4), r2, fx => { fx.shadowOffset = new Vector2Int(2, -2); fx.shadowColor = new Color32(24, 20, 37, 200); },
                (t, time) => { int k = (int)(time / 0.25f) % 4; t.fx.shadowOffset = new Vector2Int(1 + (k == 2 ? 1 : 0) + (k > 0 ? 1 : 0), -1 - (k > 0 ? 1 : 0) - (k == 2 ? 1 : 0)); }, null, bob: false);

            // Particle panel
            var panel = new GameObject("ParticlePanel").AddComponent<SpriteRenderer>();
            panel.sprite = _panel; panel.sortingOrder = -10; panel.transform.position = new Vector3(0, -117, 0) * U;
            string[] names = { "HIT SPARKS", "DUST", "EXPLOSION", "SPARKLE" };
            System.Func<PixelBurstSettings>[] presets = { PixelBurstSettings.HitSparks, PixelBurstSettings.Dust, PixelBurstSettings.Explosion, PixelBurstSettings.Sparkle };
            for (int i = 0; i < 4; i++)
            {
                var f = presets[i];
                _buttons.Add((names[i], new Vector3(-174 + i * 116, -117, 0) * U, p => PixelParticles.Burst(f(), p + Vector2.up * (12 * U))));
            }

            PixelParticles.Instance.pixelsPerUnit = pixelsPerUnit / ArtScale; // particle pixels match the 3x art pixels
            PixelParticles.Instance.sortingOrder = 100;

            _flat = Flat(new Color32(24, 20, 37, 255)); _flatHover = Flat(new Color32(58, 68, 102, 255)); _flatAccent = Flat(Accent);
        }

        static Color[] C(params Color32[] c) { var r = new Color[c.Length]; for (int i = 0; i < c.Length; i++) r[i] = c[i]; return r; }
        static float Col(int i) => (i - 2) * (CardW + Gap);
        static bool Step(float t, float period) => ((int)(t / period) & 1) == 0;

        static System.Action<Tile, float> Every(float interval, System.Action<Tile> a)
        {
            float next = 0.6f;
            return (t, time) => { if (time >= next) { next = time + interval; a(t); } };
        }

        void Hit(Tile t)
        {
            t.fx.GetComponent<HitFlash>().Play();
            PixelParticles.Burst(PixelBurstSettings.HitSparks(), t.art.position);
            _shake[t] = 0.12f;
        }
        readonly Dictionary<Tile, float> _shake = new Dictionary<Tile, float>();

        static void ToggleDissolve(Tile t)
        {
            var d = t.fx.GetComponent<DissolveFX>();
            if (t.fx.dissolveAmount > 0.5f) d.PlayIn(); else d.PlayOut();
        }

        void Add(string label, string hint, Sprite sprite, float x, float y, System.Action<PixelFX> setup, System.Action<Tile, float> auto, System.Action<Tile> click, bool bob = true)
        {
            var card = new GameObject("Card " + label).AddComponent<SpriteRenderer>();
            card.sprite = _card; card.sortingOrder = -10; card.transform.position = new Vector3(x, y, 0) * U;

            var go = new GameObject(label);
            int artY = Mathf.RoundToInt(y + 14);
            go.transform.position = new Vector3(x, artY, 0) * U;
            go.transform.localScale = new Vector3(ArtScale, ArtScale, 1); // whole-number scale only: stays pixel-perfect
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (_mat != null) sr.sharedMaterial = _mat;
            var fx = go.AddComponent<PixelFX>();
            setup?.Invoke(fx);
            var t = new Tile { label = label, hint = hint, fx = fx, card = card, art = go.transform, center = card.transform.position, baseY = artY, bob = bob };
            if (auto != null) t.tick = time => auto(t, time);
            if (click != null) t.click = () => click(t);
            _tiles.Add(t);
        }

        void Update()
        {
            UpdateCamera();
            float time = Time.time;
            foreach (var t in _tiles)
            {
                t.tick?.Invoke(time);
                int dy = t.bob && Step(time + t.center.x * 0.01f, 0.45f) ? 1 : 0; // classic 1-art-pixel idle bob
                int dx = 0;
                if (_shake.TryGetValue(t, out float s) && s > 0) { _shake[t] = s - Time.deltaTime; dx = ((int)(s * 60) & 1) == 0 ? ArtScale : -ArtScale; }
                t.art.position = new Vector3(t.center.x / U + dx, t.baseY + dy * ArtScale, 0) * U;
            }
        }

        void UpdateCamera()
        {
            if (_cam == null) return;
            int sw = Screen.width, sh = Screen.height;
            _scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(sw / (float)referenceResolution.x, sh / (float)referenceResolution.y)));
            _cam.orthographicSize = sh / (2f * pixelsPerUnit * _scale);
            // half-texel offset on odd screen sizes keeps texels aligned to screen pixels
            float ox = (sw / _scale) % 2 == 1 ? 0.5f : 0f, oy = (sh / _scale) % 2 == 1 ? 0.5f : 0f;
            _cam.transform.position = new Vector3(ox * U, oy * U, -10);
        }

        // ---------- IMGUI ----------
        GUIStyle _title, _sub, _label, _hint, _btn;

        void OnGUI()
        {
            if (_cam == null) return;
            int s = _scale;
            if (_title == null || _title.fontSize != 16 * s)
            {
                _title = new GUIStyle(GUI.skin.label) { fontSize = 16 * s, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
                _title.normal.textColor = Color.white;
                _sub = new GUIStyle(_title) { fontSize = 7 * s, fontStyle = FontStyle.Normal };
                _sub.normal.textColor = new Color32(139, 155, 180, 255);
                _label = new GUIStyle(GUI.skin.label) { fontSize = 7 * s, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow };
                _label.normal.textColor = Color.white;
                _hint = new GUIStyle(_label) { fontSize = 6 * s, fontStyle = FontStyle.Normal, alignment = TextAnchor.UpperCenter };
                _hint.normal.textColor = new Color32(139, 155, 180, 255);
                _btn = new GUIStyle(_label) { border = new RectOffset(0, 0, 0, 0) };
                _btn.normal.background = _flat; _btn.hover.background = _flatHover; _btn.active.background = _flatAccent;
                _btn.hover.textColor = Color.white; _btn.active.textColor = new Color32(24, 20, 37, 255);
            }

            Rect top = PixelRect(-232, 135, 464, 26);
            GUI.Label(new Rect(top.x, top.y, top.width, top.height * 0.62f), "PIXEL FX PACK", _title);
            GUI.Label(new Rect(top.x, top.y + top.height * 0.58f, top.width, top.height * 0.42f), "10 sprite effects  |  pixel particles  |  click a card, a button, or empty space", _sub);

            Vector2 mouse = Event.current.mousePosition;
            bool used = false;
            foreach (var t in _tiles)
            {
                Rect card = PixelRect(t.center.x / U - CardW / 2f, t.center.y / U + CardH / 2f, CardW, CardH);
                bool hover = card.Contains(mouse);
                t.card.sprite = hover ? _cardHover : _card;
                Rect strip = PixelRect(t.center.x / U - CardW / 2f + 2, t.center.y / U - CardH / 2f + StripH + 10, CardW - 4, StripH);
                Shadowed(strip, t.label, _label);
                Rect hint = PixelRect(t.center.x / U - CardW / 2f, t.center.y / U - CardH / 2f + 11, CardW, 10);
                GUI.Label(hint, t.click != null ? t.hint + "  - click" : t.hint, _hint);
                if (t.click != null && GUI.Button(card, GUIContent.none, GUIStyle.none)) { t.click(); used = true; }
            }
            foreach (var b in _buttons)
            {
                Rect r = PixelRect(b.pos.x / U - 52, b.pos.y / U + 9, 104, 18);
                if (GUI.Button(r, b.label, _btn)) { b.fire(b.pos); used = true; }
            }
            if (!used && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector3 w = _cam.ScreenToWorldPoint(new Vector3(mouse.x, Screen.height - mouse.y, 0));
                PixelParticles.Burst(PixelBurstSettings.Explosion(), w);
            }
        }

        static void Shadowed(Rect r, string text, GUIStyle st)
        {
            var c = st.normal.textColor;
            st.normal.textColor = new Color32(24, 20, 37, 255);
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, st);
            st.normal.textColor = c;
            GUI.Label(r, text, st);
        }

        /// <summary>Rect in GUI space from art-pixel coords (x,y = top-left, y up).</summary>
        Rect PixelRect(float x, float yTop, float w, float h)
        {
            Vector3 a = _cam.WorldToScreenPoint(new Vector3(x, yTop, 0) * U);
            return new Rect(Mathf.Round(a.x), Mathf.Round(Screen.height - a.y), w * _scale, h * _scale);
        }

        // ---------- procedural UI sprites ----------
        Sprite MakeCard(int w, int h, Color32 edge, string name, bool strip = true)
        {
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool corner = (x == 0 || x == w - 1) && (y == 0 || y == h - 1);
                    if (corner) continue; // 1-px rounded corners
                    Color32 c = CardCol;
                    if (strip && y < StripH + 12) c = StripCol;
                    if (y == h - 2 && x > 0 && x < w - 1) c = CardHi; // top highlight
                    if (x == 0 || x == w - 1 || y == 0 || y == h - 1) c = edge;
                    px[y * w + x] = c;
                }
            return PixelSpriteFactory.FromPixels(px, w, h, pixelsPerUnit, name);
        }

        Sprite MakeBackground(int w, int h)
        {
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = (x % 8 == 0 && y % 8 == 0) ? BgDot : BgCol;
            return PixelSpriteFactory.FromPixels(px, w, h, pixelsPerUnit, "Background");
        }

        static Texture2D Flat(Color32 c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            t.SetPixel(0, 0, c); t.Apply(); return t;
        }
    }
}
