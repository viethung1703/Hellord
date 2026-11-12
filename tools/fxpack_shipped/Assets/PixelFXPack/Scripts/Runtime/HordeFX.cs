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
        class Num { public Transform root; public SpriteRenderer[] glyphs; public int len; public float age, life; public Vector3 pos; public bool crit; }

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
            if (showNumber && sr) s.DoNumber(sr.transform.position + Vector3.up * (8f / s.pixelsPerUnit), damage, crit);
        }
        public static void Flash(SpriteRenderer sr, bool crit = false) => I.DoFlash(sr, crit);
        public static void Number(Vector3 pos, int value, bool crit = false) => I.DoNumber(pos, value, crit);
        /// <summary>Death: quantised dissolve on shared materials + cheap pixel burst. <paramref name="onDone"/> receives the renderer
        /// (restored to the base material) so you can return it to your pool. If null the GameObject is deactivated.</summary>
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

        void DoNumber(Vector3 pos, int value, bool crit)
        {
            BuildAtlas();
            Num n;
            if (_nums.Count >= maxNumbers) { n = _nums[0]; _nums.RemoveAt(0); }
            else n = _numPool.Count > 0 ? _numPool.Pop() : NewNum();
            string s = Mathf.Abs(value).ToString();
            if (crit) s += "!";
            if (s.Length > n.glyphs.Length) s = s.Substring(0, n.glyphs.Length);
            n.len = s.Length; n.age = 0; n.life = crit ? 0.75f : 0.5f; n.crit = crit;
            n.pos = pos + new Vector3(UnityEngine.Random.Range(-3, 4), 0, 0) / pixelsPerUnit;
            var set = crit ? _glyphY : _glyphW;
            for (int i = 0; i < n.glyphs.Length; i++)
            {
                var g = n.glyphs[i];
                if (i < n.len) { g.enabled = true; g.sprite = set[Glyphs.IndexOf(s[i])]; g.transform.localPosition = new Vector3((i - (n.len - 1) * 0.5f) * 4f / pixelsPerUnit, 0, 0); g.color = Color.white; }
                else g.enabled = false;
            }
            n.root.gameObject.SetActive(true);
            _nums.Add(n);
        }
        Num NewNum()
        {
            var n = new Num { root = new GameObject("num").transform, glyphs = new SpriteRenderer[6] };
            n.root.SetParent(transform, false);
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
            float unit = 1f / pixelsPerUnit;
            for (int i = _nums.Count - 1; i >= 0; i--)
            {
                var n = _nums[i];
                n.age += dt;
                if (n.age >= n.life) { n.root.gameObject.SetActive(false); _nums.RemoveAt(i); _numPool.Push(n); continue; }
                float k = n.age / n.life;
                // pop up fast, hang, then fall slightly - snapped to the pixel grid
                float rise = (1 - (1 - Mathf.Min(1, k * 3)) * (1 - Mathf.Min(1, k * 3))) * (n.crit ? 12 : 8);
                float sc = n.crit ? (k < 0.12f ? 2f : 1.5f) : (k < 0.1f ? 1.4f : 1f);
                var p = n.pos + new Vector3(0, rise * unit, 0);
                n.root.position = new Vector3(Mathf.Round(p.x / unit) * unit, Mathf.Round(p.y / unit) * unit, 0);
                n.root.localScale = new Vector3(sc, sc, 1);
                if (k > 0.7f) { float a = Mathf.Floor((1 - (k - 0.7f) / 0.3f) * 3) / 3f; for (int g = 0; g < n.len; g++) n.glyphs[g].color = new Color(1, 1, 1, a); }
            }
        }
    }
}
