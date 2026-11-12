using System;
using System.Collections.Generic;
using UnityEngine;

// Pixel FX Pack v1.4 "Horde" toolkit: batch-friendly hit flash + death dissolve, pooled damage numbers, cheap death bursts.
// Design notes (UNTESTED IN EDITOR - see README "Performance"):
//  * A MaterialPropertyBlock per renderer (what PixelFX/HitFlash use) breaks sprite batching. With 300+ enemies that means
//    300+ draw calls. The horde path instead swaps renderer.sharedMaterial between a handful of SHARED materials
//    (base, flash, 8 quantised dissolve steps). Renderers that share sprite texture + material can batch.
//  * Everything is updated from ONE MonoBehaviour Update with plain lists - no per-enemy components, no coroutines, no
//    per-hit allocations once pools are warm.
//  * Damage numbers use one shared glyph atlas texture, so all numbers batch together too.
namespace PixelFXPack
{
    /// <summary>Shared materials for the horde path. All derive from PixelFXMaterials.Shared (the Uber material).</summary>
    public static class HordeMaterials
    {
        public const int DissolveSteps = 8;
        static Material _flash, _crit;
        static Material[] _dissolve;
        static Color _edge = new Color(1f, 0.45f, 0.1f, 1f);
        static readonly int ID_Flash = Shader.PropertyToID("_FlashAmount"), ID_FlashColor = Shader.PropertyToID("_FlashColor"),
            ID_Diss = Shader.PropertyToID("_DissolveAmount"), ID_Edge = Shader.PropertyToID("_DissolveEdgeColor"),
            ID_EdgeW = Shader.PropertyToID("_DissolveEdgeWidth"), ID_Glow = Shader.PropertyToID("_DissolveEdgeGlow"),
            ID_Cell = Shader.PropertyToID("_DissolvePixel");

        /// <summary>The base material every horde enemy should use (assign it, or call HordeFX.Prepare).</summary>
        public static Material Base => PixelFXMaterials.Shared;

        static Material Clone(string name)
        {
            var b = Base; if (b == null) return null;
            var m = new Material(b) { name = name, hideFlags = HideFlags.DontSave };
            m.SetFloat(ID_Flash, 0); m.SetFloat(ID_Diss, 0);
            return m;
        }
        /// <summary>Solid white flash material (hit frame).</summary>
        public static Material Flash { get { if (_flash == null) { _flash = Clone("Horde Flash"); if (_flash) { _flash.SetFloat(ID_Flash, 1); _flash.SetColor(ID_FlashColor, Color.white); } } return _flash; } }
        /// <summary>Hot red-gold flash for crits.</summary>
        public static Material CritFlash { get { if (_crit == null) { _crit = Clone("Horde CritFlash"); if (_crit) { _crit.SetFloat(ID_Flash, 1); _crit.SetColor(ID_FlashColor, new Color(1f, 0.85f, 0.3f)); } } return _crit; } }
        /// <summary>Dissolve step 0..DissolveSteps-1 (amount rises with step).</summary>
        public static Material Dissolve(int step)
        {
            if (_dissolve == null) _dissolve = new Material[DissolveSteps];
            step = Mathf.Clamp(step, 0, DissolveSteps - 1);
            if (_dissolve[step] == null)
            {
                var m = Clone("Horde Dissolve " + step);
                if (m != null)
                {
                    m.SetFloat(ID_Diss, (step + 1f) / (DissolveSteps + 1f));
                    m.SetColor(ID_Edge, _edge); m.SetFloat(ID_EdgeW, 0.12f); m.SetFloat(ID_Glow, 2.5f); m.SetFloat(ID_Cell, 1f);
                }
                _dissolve[step] = m;
            }
            return _dissolve[step];
        }
        /// <summary>Change the dissolve edge colour for every bucket (e.g. hell orange, soul cyan).</summary>
        public static void SetDissolveEdge(Color c)
        {
            _edge = c;
            if (_dissolve == null) return;
            foreach (var m in _dissolve) if (m) m.SetColor(ID_Edge, c);
        }
    }

    /// <summary>
    /// Central horde FX system. Usage (no components needed on enemies):
    ///   HordeFX.Hit(enemyRenderer, damage, crit);   // flash + pooled number
    ///   HordeFX.Kill(enemyRenderer, onDone);        // dissolve + cheap burst; onDone returns it to YOUR pool
    /// </summary>
    [AddComponentMenu("Pixel FX/Horde FX (System)")]
    public class HordeFX : MonoBehaviour
    {
        [Tooltip("Pixels per world unit; match your sprites' import PPU.")] public float pixelsPerUnit = 16f;
        [Tooltip("Max damage numbers alive. Oldest is recycled when exceeded.")] [Min(4)] public int maxNumbers = 48;
        [Tooltip("v1.8.3: hits on the same target less than this long after its last hit merge into one rising total (0 = off).")] [Min(0)] public float numberMergeWindow = 0.4f;
        [Tooltip("v1.8.4: draw each number on a dark backing plate so drops / sparks / pops underneath never read as extra digits.")] public bool numberPlate = true;
        [Tooltip("v1.8.4: seconds a kill burst (or AddNumberObstacle) blocks number placement.")] [Min(0)] public float numberObstacleSeconds = 0.15f;
        [Tooltip("v1.8.3: layout step (px x2) when a new number would overlap a live one; numbers with no free slot are dropped.")] [Min(0)] public float numberStackPx = 5f;
        [Tooltip("Max simultaneous dissolves. Extra deaths pop instantly (burst only).")] [Min(4)] public int maxDissolves = 160;
        [Tooltip("Hit flash length (s).")] public float flashSeconds = 0.07f;
        [Tooltip("Death dissolve length (s).")] public float dissolveSeconds = 0.32f;
        [Tooltip("Pixels per cheap death burst (budgeted further by PixelParticles).")] [Range(0, 16)] public int burstPixels = 5;
        public Color burstColor = new Color(1f, 0.55f, 0.15f);
        public int numberSortingOrder = 95;

        static HordeFX _i;
        public static HordeFX I
        {
            get
            {
                if (_i == null) { _i = FindAnyObjectByType<HordeFX>(); if (_i == null) _i = new GameObject("HordeFX").AddComponent<HordeFX>(); }
                return _i;
            }
        }
        void Awake() { if (_i == null) _i = this; }
        void OnDestroy() { if (_i == this) _i = null; }

        struct FlashJob { public SpriteRenderer sr; public float until; }
        struct DieJob { public SpriteRenderer sr; public float t; public Action<SpriteRenderer> done; }
        class Num { public Transform root; public SpriteRenderer[] glyphs; public int len, value, key; public float age, life, lastHit; public Vector3 pos; public bool crit; public SpriteRenderer plate; }
        struct Obstacle { public Vector2 p; public float r, until; }
        readonly List<Obstacle> _obs = new List<Obstacle>();
        static Sprite _plateSprite;

        readonly List<FlashJob> _flash = new List<FlashJob>(256);
        readonly List<DieJob> _die = new List<DieJob>(256);
        readonly List<Num> _nums = new List<Num>(64);
        readonly Stack<Num> _numPool = new Stack<Num>();
        PixelBurstSettings _burst; Color _burstCol = new Color(-1, 0, 0);

        public int ActiveFlashes => _flash.Count;
        public int ActiveDissolves => _die.Count;
        public int ActiveNumbers => _nums.Count;

        // ---------- static API ----------
        /// <summary>Put a renderer on the shared horde base material (do this once when you spawn enemies).</summary>
        public static void Prepare(SpriteRenderer sr) { if (sr && HordeMaterials.Base) sr.sharedMaterial = HordeMaterials.Base; }
        /// <summary>Flash + damage number.</summary>
        public static void Hit(SpriteRenderer sr, int damage, bool crit = false, bool showNumber = true)
        {
            var s = I; s.DoFlash(sr, crit);
            if (showNumber && sr) s.DoNumber(sr.transform.position + Vector3.up * (8f / s.pixelsPerUnit), damage, crit, sr.GetInstanceID());
        }
        public static void Flash(SpriteRenderer sr, bool crit = false) => I.DoFlash(sr, crit);
        public static void Number(Vector3 pos, int value, bool crit = false) => I.DoNumber(pos, value, crit, 0);
        /// <summary>v1.8.2: numbers with the same non-zero <paramref name="mergeKey"/> (e.g. target instance id) inside numberMergeWindow merge into one total.</summary>
        public static void Number(Vector3 pos, int value, bool crit, int mergeKey) => I.DoNumber(pos, value, crit, mergeKey);
        /// <summary>Death: quantised dissolve on shared materials + cheap pixel burst. <paramref name="onDone"/> receives the renderer
        /// (restored to the base material) so you can return it to your pool. If null the GameObject is deactivated.</summary>
        /// <summary>v1.8.4: keep damage numbers clear of a drop / pickup / spark at <paramref name="pos"/> (radius in px) for <paramref name="seconds"/> (&lt;0 = numberObstacleSeconds).</summary>
        public static void AddNumberObstacle(Vector3 pos, float radiusPx = 3f, float seconds = -1f) => I.DoObstacle(pos, radiusPx, seconds);
        void DoObstacle(Vector3 pos, float radiusPx, float seconds)
        {
            if (_obs.Count >= 64) _obs.RemoveAt(0);
            _obs.Add(new Obstacle { p = pos, r = radiusPx / pixelsPerUnit, until = Time.time + (seconds < 0 ? numberObstacleSeconds : seconds) });
        }
        public static void Kill(SpriteRenderer sr, Action<SpriteRenderer> onDone = null) => I.DoKill(sr, onDone);
        /// <summary>Cheapest possible death pop: a few budgeted pixels, no dissolve.</summary>
        public static void CheapBurst(Vector3 pos, Color c, int pixels = 5) => I.DoBurst(pos, c, pixels);

        // ---------- impl ----------
        void DoFlash(SpriteRenderer sr, bool crit)
        {
            if (!sr) return;
            var m = crit ? HordeMaterials.CritFlash : HordeMaterials.Flash;
            if (m == null) return;
            for (int i = 0; i < _flash.Count; i++) if (_flash[i].sr == sr) { var f = _flash[i]; f.until = Time.time + flashSeconds; _flash[i] = f; sr.sharedMaterial = m; return; }
            for (int i = 0; i < _die.Count; i++) if (_die[i].sr == sr) return; // already dying
            sr.sharedMaterial = m;
            _flash.Add(new FlashJob { sr = sr, until = Time.time + flashSeconds });
        }

        void DoKill(SpriteRenderer sr, Action<SpriteRenderer> onDone)
        {
            if (!sr) return;
            for (int i = _flash.Count - 1; i >= 0; i--) if (_flash[i].sr == sr) _flash.RemoveAt(i);
            for (int i = 0; i < _die.Count; i++) if (_die[i].sr == sr) return;
            DoBurst(sr.transform.position, burstColor, burstPixels);
            DoObstacle(sr.transform.position, 4f, -1f);
            if (_die.Count >= maxDissolves || HordeMaterials.Dissolve(0) == null) { Finish(sr, onDone); return; }
            sr.sharedMaterial = HordeMaterials.Flash != null ? HordeMaterials.Flash : sr.sharedMaterial; // 1 white frame first
            _die.Add(new DieJob { sr = sr, t = 0, done = onDone });
        }

        void Finish(SpriteRenderer sr, Action<SpriteRenderer> onDone)
        {
            if (!sr) return;
            if (HordeMaterials.Base) sr.sharedMaterial = HordeMaterials.Base;
            if (onDone != null) onDone(sr); else sr.gameObject.SetActive(false);
        }

        void DoBurst(Vector3 pos, Color c, int pixels)
        {
            if (pixels <= 0) return;
            if (_burst == null)
                _burst = new PixelBurstSettings { speed = new Vector2(1.2f, 3f), spreadDegrees = 360, lifetime = new Vector2(0.18f, 0.34f),
                    gravity = -3f, drag = 4, sizePixels = new Vector2Int(1, 2), colorSteps = 3, shrink = true, additive = true };
            _burst.count = pixels;
            if (c != _burstCol) { _burstCol = c; _burst.colorOverLife = Grad(Color.white, c, new Color(c.r * 0.4f, c.g * 0.2f, c.b * 0.2f, 0f)); }
            PixelParticles.Burst(_burst, pos);
        }
        static Gradient Grad(Color a, Color b, Color c)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(a, 0), new GradientColorKey(b, 0.35f), new GradientColorKey(c, 1) },
                      new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
            return g;
        }

        // ----- pooled numbers on one glyph atlas -----
        static Texture2D _atlas; static Sprite[] _glyphW, _glyphY; const string Glyphs = "0123456789x!";
        static readonly string[] Font = {
            "111101101101111","010110010010111","111001111100111","111001111001111","101101111001001",
            "111100111001111","111100111101111","111001001001001","111101111101111","111101111001111",
            "000101010101000","010010010000010" };
        void BuildAtlas()
        {
            if (_atlas != null) return;
            int gw = 5, gh = 7, n = Glyphs.Length; // 3x5 glyph + 1px outline, two colour rows
            _atlas = new Texture2D(gw * n, gh * 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PFX Horde Glyphs" };
            var px = new Color32[_atlas.width * _atlas.height];
            var ol = new Color32(20, 8, 12, 255);
            Color32[] fills = { new Color32(255, 255, 255, 255), new Color32(255, 214, 64, 255) };
            for (int row = 0; row < 2; row++)
                for (int i = 0; i < n; i++)
                    for (int y = 0; y < gh; y++) for (int x = 0; x < gw; x++)
                    {
                        bool ink = Ink(i, x - 1, y - 1), near = false;
                        for (int dy = -1; dy <= 1 && !near; dy++) for (int dx = -1; dx <= 1; dx++) if (Ink(i, x - 1 + dx, y - 1 + dy)) { near = true; break; }
                        px[(row * gh + y) * _atlas.width + i * gw + x] = ink ? fills[row] : near ? ol : new Color32(0, 0, 0, 0);
                    }
            _atlas.SetPixels32(px); _atlas.Apply();
            _glyphW = new Sprite[n]; _glyphY = new Sprite[n];
            for (int i = 0; i < n; i++)
            {
                _glyphW[i] = Sprite.Create(_atlas, new Rect(i * gw, 0, gw, gh), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                _glyphY[i] = Sprite.Create(_atlas, new Rect(i * gw, gh, gw, gh), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
        }
        static bool Ink(int g, int x, int y) { if (x < 0 || y < 0 || x > 2 || y > 4) return false; return Font[g][(4 - y) * 3 + x] == '1'; }

        void DoNumber(Vector3 pos, int value, bool crit, int key)
        {
            BuildAtlas();
            Num n;
            if (key != 0 && numberMergeWindow > 0)
                for (int i = _nums.Count - 1; i >= 0; i--)
                {
                    var m = _nums[i];
                    if (m.key != key || m.age - m.lastHit > numberMergeWindow) continue;
                    // v1.8.3: one total per burst - crits bump the same number (yellow + pop), never spawn a second one
                    m.value += Mathf.Abs(value); m.crit |= crit; m.lastHit = m.age; m.life = Mathf.Max(m.life, m.age + (m.crit ? 0.45f : 0.35f));
                    SetText(m); return;
                }
            if (_nums.Count >= maxNumbers) { n = _nums[0]; _nums.RemoveAt(0); }
            else n = _numPool.Count > 0 ? _numPool.Pop() : NewNum();
            n.value = Mathf.Abs(value); n.key = key; n.age = 0; n.lastHit = 0; n.life = crit ? 0.75f : 0.5f; n.crit = crit;
            SetText(n);
            if (!Place(n, pos)) { n.root.gameObject.SetActive(false); _numPool.Push(n); return; } // no free spot: drop (never overlap)
            n.root.gameObject.SetActive(true);
            _nums.Add(n);
        }
        // v1.8.3 layout: boxes cover the whole rise path (+ crit pop), candidates go up then sideways.
        static readonly Vector2[] _slots = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(-1, 1), new Vector2(1, 1), new Vector2(0, 2), new Vector2(-1, 2), new Vector2(1, 2) };
        Rect Box(Num n, Vector3 p)
        {
            float u = 1f / pixelsPerUnit, sc = n.crit ? 2f : 1.4f, w = (n.len * 4 + 1) * sc * u, h = 7 * sc * u, rise = (n.crit ? 12 : 8) * u;
            return new Rect(p.x - w * 0.5f, p.y - u, w, h + rise + u);
        }
        bool Place(Num n, Vector3 pos)
        {
            float u = 1f / pixelsPerUnit, sx = (n.len * 4 + 3) * u, sy = Mathf.Max(numberStackPx, 1) * 2 * u;
            for (int k = 0; k < _slots.Length; k++)
            {
                var p = pos + new Vector3(_slots[k].x * sx, _slots[k].y * sy, 0); var b = Box(n, p); bool ok = true;
                for (int i = 0; i < _nums.Count && ok; i++) if (_nums[i] != n && b.Overlaps(Box(_nums[i], _nums[i].pos))) ok = false;
                for (int i = 0; i < _obs.Count && ok; i++) if (_obs[i].until > Time.time && b.Overlaps(new Rect(_obs[i].p.x - _obs[i].r, _obs[i].p.y - _obs[i].r, _obs[i].r * 2, _obs[i].r * 2))) ok = false;
                if (ok) { n.pos = p; return true; }
            }
            return false;
        }
        void SetText(Num n)
        {
            string s = n.value.ToString();
            if (n.crit) s += "!";
            if (s.Length > n.glyphs.Length) s = s.Substring(0, n.glyphs.Length);
            n.len = s.Length;
            var set = n.crit ? _glyphY : _glyphW;
            for (int i = 0; i < n.glyphs.Length; i++)
            {
                var g = n.glyphs[i];
                if (i < n.len) { g.enabled = true; g.sprite = set[Glyphs.IndexOf(s[i])]; g.transform.localPosition = new Vector3((i - (n.len - 1) * 0.5f) * 4f / pixelsPerUnit, 0, 0); g.color = Color.white; }
                else g.enabled = false;
            }
            if (n.plate)
            {
                n.plate.enabled = numberPlate;
                float u = 1f / pixelsPerUnit; // 2px padding around the outlined glyph run
                n.plate.transform.localScale = new Vector3((n.len * 4 + 1 + 2) * u * pixelsPerUnit, (7 + 2) * u * pixelsPerUnit, 1);
                n.plate.color = new Color(0.078f, 0.031f, 0.047f, 0.9f);
            }
        }
        Num NewNum()
        {
            var n = new Num { root = new GameObject("num").transform, glyphs = new SpriteRenderer[6] };
            n.root.SetParent(transform, false);
            if (_plateSprite == null)
            {
                var tx = new Texture2D(1, 1) { filterMode = FilterMode.Point, name = "PFX Number Plate" }; tx.SetPixel(0, 0, Color.white); tx.Apply();
                _plateSprite = Sprite.Create(tx, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            n.plate = new GameObject("plate").AddComponent<SpriteRenderer>();
            n.plate.sprite = _plateSprite; n.plate.transform.SetParent(n.root, false); n.plate.sortingOrder = numberSortingOrder - 1;
            for (int i = 0; i < 6; i++)
            {
                var g = new GameObject("g").AddComponent<SpriteRenderer>();
                g.transform.SetParent(n.root, false); g.sortingOrder = numberSortingOrder;
                n.glyphs[i] = g;
            }
            return n;
        }

        void Update()
        {
            float now = Time.time, dt = Time.deltaTime;
            for (int i = _flash.Count - 1; i >= 0; i--)
            {
                var f = _flash[i];
                if (!f.sr) { _flash.RemoveAt(i); continue; }
                if (now >= f.until) { if (HordeMaterials.Base) f.sr.sharedMaterial = HordeMaterials.Base; _flash.RemoveAt(i); }
            }
            for (int i = _die.Count - 1; i >= 0; i--)
            {
                var d = _die[i];
                if (!d.sr) { _die.RemoveAt(i); continue; }
                d.t += dt;
                float k = d.t / Mathf.Max(0.01f, dissolveSeconds);
                if (k >= 1f) { _die.RemoveAt(i); Finish(d.sr, d.done); continue; }
                if (d.t > 0.04f) d.sr.sharedMaterial = HordeMaterials.Dissolve(Mathf.FloorToInt(k * HordeMaterials.DissolveSteps));
                _die[i] = d;
            }
            for (int i = _obs.Count - 1; i >= 0; i--) if (_obs[i].until <= Time.time) _obs.RemoveAt(i);
            float unit = 1f / pixelsPerUnit;
            for (int i = _nums.Count - 1; i >= 0; i--)
            {
                var n = _nums[i];
                n.age += dt;
                if (n.age >= n.life) { n.root.gameObject.SetActive(false); _nums.RemoveAt(i); _numPool.Push(n); continue; }
                float k = n.age / n.life;
                // pop up fast, hang, then fall slightly - snapped to the pixel grid
                float rise = (1 - (1 - Mathf.Min(1, k * 3)) * (1 - Mathf.Min(1, k * 3))) * (n.crit ? 12 : 8);
                float pop = n.age - n.lastHit; // v1.8.3: re-pop on every merged hit
                float sc = n.crit ? (pop < 0.06f ? 2f : 1.5f) : (pop < 0.05f ? 1.4f : 1f);
                var p = n.pos + new Vector3(0, rise * unit, 0);
                n.root.position = new Vector3(Mathf.Round(p.x / unit) * unit, Mathf.Round(p.y / unit) * unit, 0);
                n.root.localScale = new Vector3(sc, sc, 1);
                if (k > 0.7f) { float a = Mathf.Floor((1 - (k - 0.7f) / 0.3f) * 3) / 3f; for (int g = 0; g < n.len; g++) n.glyphs[g].color = new Color(1, 1, 1, a); if (n.plate) n.plate.color = new Color(0.078f, 0.031f, 0.047f, 0.9f * a); }
            }
        }
    }
}
