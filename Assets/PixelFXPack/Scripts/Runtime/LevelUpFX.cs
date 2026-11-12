using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// #151-153 LevelUpFX (v2.5.1): level-up juice in three beats, on UNSCALED time so it keeps stepping while the game's level-up pause (mode != Play) is up.
    /// Reward language only (gold #fee761, white, cyan #2ce8f5 for support, steel #8b9bb4, dark outlines). Never a hazard tell: no red or orange, no filled areas, no chevrons, no blink, no loop, nothing pulses,
    /// no full-screen flash, all features at least 3 px. v2.5.1: every coloured part is OPAQUE with a dark outline and fades by DISSOLVING (cells leave once), never by alpha, because an alpha blend over the red floor
    /// turned gold/cyan into orange/pink after the CRT. The big expanding cyan ground wave was dropped (it read as an enemy AoE). Pooled static runner, int handles (-1 when full), 2 draw calls.
    /// 151 LevelUp    beat (a) + (b): Open(feet, level) when the card screen opens. The game's mode freeze already stops the sim, so the world stands still for HeroBeatSeconds (0.25 s) UNDIMMED:
    ///                an opaque gold and an opaque white 3 px ground ring (dark outline, dissolve out in 0.3 s) around the hero, 6 gold/white sparkles beside (never over) the hero and a gold-bordered plate above
    ///                the head (star + level number). After the beat DimK eases 0 -> 1 over 0.20 s (instead of the old 1-frame hard dim) and Reveal / SlidePx stagger the cards in. PickLockSeconds (0.20) after the beat
    ///                CardsReady turns true, so the first pick is possible 0.45 s after the level-up. A chained screen (a pick within 0.25 s of the previous one: several pending levels) skips the hero beat; its dim starts
    ///                at the value the previous screen left (so no jump) and eases to 1 over ChainDimInSeconds (0.12 s).
    /// 152 Picked     beat (c): Picked(h, kind, feet) the frame a card is chosen. 0.30 s: an opaque ground ring (gold = new skill / skill level, cyan = support, steel = stat, steel = skip), a 28x26 plate with a kind glyph
    ///                above the head (star / plus / gem / shield; none for Skip) and two sparkles. The plate is gone by 0.18 s and the ring dissolves by 0.30 s, so the game is readable at once.
    /// 153 Resume     DimK(h) after Picked ramps 1 -> 0 over ResumeFadeSeconds (0.12 s, 4 frames) so the world un-dims in steps of ~25% instead of one 69% jump; set it to 0 for the old hard cut. The sim runs from the first frame.
    /// Call order in the game: Open (OpenLevelUp) -> every OnGUI: DimK / Reveal / SlidePx -> Picked (Choose / SkipCards) -> keep drawing DimK until it returns 0. SetHero(feet) every frame so Picked follows a moving hero.
    /// </summary>
    public enum LevelPick { NewSkill = 0, SkillLevel = 1, Support = 2, Stat = 3, Skip = 4 }

    public static class LevelUpFX
    {
        public static int SortingOrder = 70; public static string SortingLayer = "Default";
        public static int GroundSortingOrder = -20000; public static string GroundSortingLayer = "Default";
        public static Material MaterialOverride; public static int MaxItems = 16;
        /// <summary>Hero sprite height in px above the feet (Hellord hero 17); the plates float 4 px above it.</summary>
        public static float HeroHeadPx = 18f;
        /// <summary>Unscaled seconds the frozen world stays undimmed with the hero beat playing before the dim starts. 0 = no hero beat (dim starts at once).</summary>
        public static float HeroBeatSeconds = 0.25f;
        /// <summary>Unscaled seconds for the dim to ease in after the hero beat (0 = hard dim like before).</summary>
        public static float DimInSeconds = 0.20f;
        /// <summary>Unscaled seconds for the dim to ease out after a pick (0 = hard cut).</summary>
        public static float ResumeFadeSeconds = 0.12f;
        /// <summary>Card reveal: delay after the hero beat (the dim is ~1/3 in by then), seconds between cards, seconds per card, slide distance in px.</summary>
        public static float CardDelay = 0.05f, CardStagger = 0.04f, CardRevealSeconds = 0.15f, CardSlidePx = 10f;
        /// <summary>Seconds after the hero beat before a card can be picked (default 0.20: total 0.45 s from the level-up to the first pick; the first card is fully in by then).</summary>
        public static float PickLockSeconds = 0.20f;
        /// <summary>Chained screen (several pending levels): seconds for the dim to ease from where the previous screen left it up to 1. Keeps the dim continuous between the two screens.</summary>
        public static float ChainDimInSeconds = 0.12f;
        static LevelUpRunner R => LevelUpRunner.I;
        public static void Prewarm() => R.Prewarm();
        public static void SetGroundSorting(string layerName, int order) { GroundSortingLayer = layerName; GroundSortingOrder = order; R.ApplySorting(); }
        /// <summary>Hero feet in world units, every frame (Picked effects follow the hero).</summary>
        public static void SetHero(Vector2 feet) => R.SetHero(feet * HellFX.PPU);
        /// <summary>Beats (a) + (b), when the level-up card screen opens. feet = hero feet in world units, level = the level the screen is for (1..99 shown). Returns a handle, -1 if the pool is full
        /// (then DimK(-1) = 1, Reveal(-1, i) = 1, SlidePx = 0, CardsReady = true: the screen simply opens at once with no effect).</summary>
        public static int Open(Vector2 feet, int level) => R.OpenScreen(feet * HellFX.PPU, level);
        /// <summary>Dim strength 0..1 to multiply into the game's card-screen dim alpha. Returns 1 for h = -1 (fallback), 0 for a finished or unknown handle. Keep calling it after Picked until it reaches 0.</summary>
        public static float DimK(int h) => R.DimK(h);
        /// <summary>Card i (0-based) reveal 0..1 (use it as the card GUI alpha; skip drawing at 0; card -1 = title, same timing as card 0); 1 = settled. Returns 1 for h = -1.</summary>
        public static float Reveal(int h, int card) => R.Reveal(h, card);
        /// <summary>Whole px to add to the card's y while it slides in (CardSlidePx -> 0, ease-out). 0 when settled or h = -1.</summary>
        public static int SlidePx(int h, int card) => R.SlidePx(h, card);
        /// <summary>False for the first HeroBeatSeconds + PickLockSeconds (0.45 s) of a normal screen (0.2 s chained) so a mashed key cannot pick a card that is not drawn yet. True for h = -1.</summary>
        public static bool CardsReady(int h) => R.CardsReady(h);
        /// <summary>Beat (c): call when a card is picked (or the screen is skipped) at the same frame the game goes back to Play. h = the Open handle (may be -1). Returns the pick effect handle, -1 if full.</summary>
        public static int Picked(int h, LevelPick kind, Vector2 feet) => R.Picked(h, (int)kind, feet * HellFX.PPU);
        public static void Kill(int h) => R.Kill(h);
        public static void Clear() => R.ClearAll();
    }

    public class LevelUpRunner : MonoBehaviour
    {
        static LevelUpRunner _i;
        public static LevelUpRunner I { get { if (_i == null) { var go = new GameObject("LevelUpFX"); DontDestroyOnLoad(go); _i = go.AddComponent<LevelUpRunner>(); } return _i; } }
        /* k 0 = screen (hero beat, dim state), k 1 = pick feedback. pickT < 0 = not picked yet. */
        struct It { public bool used, chained, follow; public byte k; public int sub, level; public Vector2 p; public float t, pickT, dimAtPick, carry; public ushort gen; }
        It[] _it; int[] _free; int _freeN; Vector2 _hero; bool _heroSet; float _lastPickAt = -99f;
        struct Q { public float x, y, w, h; public Color32 c; }
        class Layer
        {
            public Q[] q; public int n, last; public Mesh mesh; public Vector3[] vtx; public Color32[] col; public MeshRenderer mr;
            public Layer(int cap, GameObject go, string name, Material m, int order, string layer)
            {
                q = new Q[cap]; vtx = new Vector3[cap * 4]; col = new Color32[cap * 4]; var uv = new Vector2[cap * 4]; var tri = new int[cap * 6];
                for (int i = 0; i < cap; i++) { int v = i * 4, t = i * 6; tri[t] = v; tri[t + 1] = v + 1; tri[t + 2] = v + 2; tri[t + 3] = v; tri[t + 4] = v + 2; tri[t + 5] = v + 3;
                    uv[v] = new Vector2(.25f, .25f); uv[v + 1] = new Vector2(.25f, .75f); uv[v + 2] = new Vector2(.75f, .75f); uv[v + 3] = new Vector2(.75f, .25f); }
                mesh = new Mesh { name = name }; mesh.MarkDynamic(); mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                go.AddComponent<MeshFilter>().sharedMesh = mesh; mr = go.AddComponent<MeshRenderer>(); if (m != null) mr.sharedMaterial = m;
                mr.sortingOrder = order; mr.sortingLayerName = layer; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                mesh.vertices = vtx; mesh.uv = uv; mesh.colors32 = col; mesh.triangles = tri;
            }
            public void Upload(float u)
            {
                for (int b = 0; b < n; b++)
                {
                    int v = b * 4; ref var s = ref q[b]; float x0 = s.x * u, y0 = s.y * u, x1 = (s.x + s.w) * u, y1 = (s.y + s.h) * u;
                    vtx[v] = new Vector3(x0, y0, 0); vtx[v + 1] = new Vector3(x0, y1, 0); vtx[v + 2] = new Vector3(x1, y1, 0); vtx[v + 3] = new Vector3(x1, y0, 0);
                    col[v] = col[v + 1] = col[v + 2] = col[v + 3] = s.c;
                }
                for (int z = n; z < last; z++) { int v = z * 4; vtx[v] = vtx[v + 1] = vtx[v + 2] = vtx[v + 3] = Vector3.zero; }
                mesh.vertices = vtx; mesh.colors32 = col; mesh.RecalculateBounds(); last = n; n = 0; mr.enabled = last > 0;
            }
        }
        Layer _top, _gnd; bool _g;
        const int TopQ = 1536, GndQ = 2048;
        static readonly Color32 Ko = new Color32(0x18, 0x14, 0x25, 255), Pl = new Color32(0x26, 0x2b, 0x44, 255), Wh = new Color32(255, 255, 255, 255),
            Y = new Color32(0xfe, 0xe7, 0x61, 255), Cy = new Color32(0x2c, 0xe8, 0xf5, 255), Lt = new Color32(0x8b, 0x9b, 0xb4, 255);
        /* 8x7 glyphs, 2 px cells: Y gold, W white, C cyan, L steel */
        static readonly string[][] Glyph = {
            new[] { "...YY...", "...YY...", "YYYYYYYY", ".YYWWYY.", "..YWWY..", ".YY..YY.", ".Y....Y." },
            new[] { "...YY...", "...YY...", "...YY...", "YYYYYYYY", "YYYYYYYY", "...YY...", "...YY..." },
            new[] { "...CC...", "..CCCC..", ".CCWWCC.", "CCCWWCCC", ".CCCCCC.", "..CCCC..", "...CC..." },
            new[] { "LLLLLLLL", "LWWLLLLL", "LWLLLLLL", ".LLLLLL.", ".LLLLLL.", "..LLLL..", "...LL..." } };
        /* 3x5 digits, 3 px cells (X = on) */
        static readonly string[][] Digit = {
            new[] { "XXX", "X.X", "X.X", "X.X", "XXX" }, new[] { ".X.", "XX.", ".X.", ".X.", "XXX" }, new[] { "XXX", "..X", "XXX", "X..", "XXX" }, new[] { "XXX", "..X", "XXX", "..X", "XXX" },
            new[] { "X.X", "X.X", "XXX", "..X", "..X" }, new[] { "XXX", "X..", "XXX", "..X", "XXX" }, new[] { "XXX", "X..", "XXX", "X.X", "XXX" }, new[] { "XXX", "..X", "..X", "..X", "..X" },
            new[] { "XXX", "X.X", "XXX", "X.X", "XXX" }, new[] { "XXX", "X.X", "XXX", "..X", "XXX" } };

        public void Prewarm()
        {
            if (_top != null) return;
            int cap = Mathf.Clamp(LevelUpFX.MaxItems, 4, 4096); _it = new It[cap]; _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) _free[_freeN++] = i;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "PFX LevelUp White", hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = Wh; tex.SetPixels32(px); tex.Apply(false, true);
            var m = LevelUpFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "LevelUpFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] LevelUpFX: no sprite shader found; set LevelUpFX.MaterialOverride.");
            _top = new Layer(TopQ, gameObject, "LevelUpFX", m, LevelUpFX.SortingOrder, LevelUpFX.SortingLayer);
            var g = new GameObject("LevelUpFX Ground"); g.transform.SetParent(transform, false);
            _gnd = new Layer(GndQ, g, "LevelUpFX Ground", m, LevelUpFX.GroundSortingOrder, LevelUpFX.GroundSortingLayer);
        }
        public void ApplySorting()
        {
            if (_top == null) return;
            _top.mr.sortingLayerName = LevelUpFX.SortingLayer; _top.mr.sortingOrder = LevelUpFX.SortingOrder;
            _gnd.mr.sortingLayerName = LevelUpFX.GroundSortingLayer; _gnd.mr.sortingOrder = LevelUpFX.GroundSortingOrder;
        }
        public void SetHero(Vector2 p) { _hero = p; _heroSet = true; }
        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && _it != null && i < _it.Length && _it[i].used && _it[i].gen == (ushort)(h >> 16); }
        int Alloc(byte k, Vector2 p)
        {
            if (_top == null) Prewarm();
            if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ushort g = (ushort)(_it[i].gen + 1);
            _it[i] = new It { used = true, k = k, p = p, gen = g, pickT = -1f };
            return Hd(i);
        }
        public void Kill(int h) { if (!Get(h, out int i)) return; _it[i].used = false; _free[_freeN++] = i; }
        public void ClearAll() { if (_it == null) return; for (int i = 0; i < _it.Length; i++) if (_it[i].used) { _it[i].used = false; _free[_freeN++] = i; } _top.n = 0; _gnd.n = 0; }

        public int OpenScreen(Vector2 p, int level)
        {
            if (_top == null) Prewarm();
            float carry = 0f;
            for (int i = 0; i < _it.Length; i++) if (_it[i].used && _it[i].k == 0) { carry = Mathf.Max(carry, DimOf(ref _it[i])); Kill(Hd(i)); }
            int h = Alloc(0, p); if (h < 0) return -1; int s = h & 0xFFFF;
            _it[s].level = Mathf.Clamp(level, 1, 99); _it[s].chained = Time.unscaledTime - _lastPickAt < 0.25f || LevelUpFX.HeroBeatSeconds <= 0f;
            _it[s].carry = _it[s].chained ? carry : 0f;
            return h;
        }
        /* all timings are seconds since Open; a chained screen has no hero beat, the dim is already 1 and the cards reveal fast */
        float BeatT(ref It it) => it.chained ? 0f : LevelUpFX.HeroBeatSeconds;
        static float Sm(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }
        float DimOf(ref It it)
        {
            if (it.pickT >= 0f) { float f = LevelUpFX.ResumeFadeSeconds; return f <= 0f ? 0f : it.dimAtPick * Mathf.Clamp01(1f - it.pickT / f); }
            if (it.chained) { float c = LevelUpFX.ChainDimInSeconds; return c <= 0f ? 1f : Mathf.Lerp(it.carry, 1f, Sm(it.t / c)); }
            float d = LevelUpFX.DimInSeconds; return d <= 0f ? (it.t >= BeatT(ref it) ? 1f : 0f) : Sm((it.t - BeatT(ref it)) / d);
        }
        public float DimK(int h) { if (h < 0) return 1f; if (!Get(h, out int i)) return 0f; return DimOf(ref _it[i]); }
        float RevealOf(ref It it, int card)
        {
            float st = it.chained ? 0.02f : LevelUpFX.CardStagger, dur = it.chained ? 0.1f : LevelUpFX.CardRevealSeconds;
            float k = (it.t - BeatT(ref it) - (it.chained ? 0f : LevelUpFX.CardDelay) - st * Mathf.Max(0, card)) / Mathf.Max(0.01f, dur); return Mathf.Clamp01(k);
        }
        public float Reveal(int h, int card) { if (!Get(h, out int i) || _it[i].k != 0) return 1f; return RevealOf(ref _it[i], card); }
        public int SlidePx(int h, int card) { if (!Get(h, out int i) || _it[i].k != 0) return 0; float k = RevealOf(ref _it[i], card); float e = 1f - (1f - k) * (1f - k); return Mathf.RoundToInt(LevelUpFX.CardSlidePx * (1f - e)); }
        public bool CardsReady(int h) { if (!Get(h, out int i) || _it[i].k != 0) return true; return _it[i].t >= (_it[i].chained ? 0.2f : BeatT(ref _it[i]) + LevelUpFX.PickLockSeconds); }
        public int Picked(int h, int kind, Vector2 feet)
        {
            if (Get(h, out int i) && _it[i].k == 0 && _it[i].pickT < 0f) { _it[i].dimAtPick = DimOf(ref _it[i]); _it[i].pickT = 0f; _it[i].p = feet; }
            _lastPickAt = Time.unscaledTime;
            int ph = Alloc(1, feet); if (ph < 0) return -1; int s = ph & 0xFFFF; _it[s].sub = Mathf.Clamp(kind, 0, 4); _it[s].follow = _heroSet; return ph;
        }

        /* primitives (pixel units, snapped); _g selects the under-characters layer */
        void A(float x, float y, float w, float h, Color32 c)
        {
            if (c.a == 0) return; var L = _g ? _gnd : _top; if (L.n >= L.q.Length) return;
            L.q[L.n++] = new Q { x = Mathf.Round(x), y = Mathf.Round(y), w = w, h = h, c = c };
        }
        void Dot(float cx, float cy, int s, Color32 c) => A(Mathf.Round(cx) - (s >> 1), Mathf.Round(cy) - (s >> 1), s, s, c);
        /* stable 0..1 hash of two ints (same value every frame for the same cell), used to dissolve opaque parts instead of alpha-blending them over the red floor */
        static float H(int a, int b) { unchecked { uint x = (uint)(a * 73856093) ^ (uint)(b * 19349663); x ^= x >> 13; x *= 1274126177u; x ^= x >> 16; return (x & 0xFFFFu) / 65536f; } }
        static Color32 Fa(Color32 c, float a) { c.a = (byte)(Mathf.Clamp01(a) * 255); return c; }
        /* ring: OPAQUE 3 px dots on an ellipse with an opaque dark 1 px outline (5 px backing), step ~2 px along the arc. It fades by dissolving whole 32nds of the arc (keep 1 -> 0, each piece leaves once), never by alpha, so the colour stays pure gold/white/cyan/steel over any floor. */
        void Ring(Vector2 c, float rx, float ys, int s, Color32 col, float keep, int seed)
        {
            if (rx < 1 || keep <= 0f) return; int n = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI * rx * (0.5f + 0.5f * ys) * 2f / 2f), 8, 220);
            for (int pass = 0; pass < 2; pass++)
                for (int j = 0; j < n; j++)
                {
                    if (H(j * 32 / n + seed * 37, 5) >= keep) continue; float a = 2 * Mathf.PI * j / n;
                    if (pass == 0) Dot(c.x + Mathf.Cos(a) * rx, c.y + Mathf.Sin(a) * rx * ys, s + 2, Ko); else Dot(c.x + Mathf.Cos(a) * rx, c.y + Mathf.Sin(a) * rx * ys, s, col);
                }
        }
        /* opaque rect made of cells of about 3 px (never thinner), each present while keep > hash(cell, seed) */
        void Cells(float x, float y, int w, int h, Color32 col, float keep, int seed)
        {
            if (keep <= 0f) return; int nx = Mathf.Max(1, w / 3), ny = Mathf.Max(1, h / 3);
            for (int ix = 0; ix < nx; ix++) for (int iy = 0; iy < ny; iy++)
            { if (H(ix + seed * 53, iy + 11) >= keep) continue; int x0 = ix * w / nx, x1 = (ix + 1) * w / nx, y0 = iy * h / ny, y1 = (iy + 1) * h / ny; A(x + x0, y + y0, x1 - x0, y1 - y0, col); }
        }
        /* plus sparkle: 3x3 core, 3x3 arms at size 1 (9x9 total), core only at size 0.5 */
        void Spark(Vector2 p, float k, Color32 col)
        {
            if (k <= 0.05f) return; Color32 o = Fa(Ko, 0.8f);
            if (k > 0.6f)
            {
                Dot(p.x, p.y, 5, o); Dot(p.x + 3, p.y, 5, o); Dot(p.x - 3, p.y, 5, o); Dot(p.x, p.y + 3, 5, o); Dot(p.x, p.y - 3, 5, o);
                Dot(p.x + 3, p.y, 3, col); Dot(p.x - 3, p.y, 3, col); Dot(p.x, p.y + 3, 3, col); Dot(p.x, p.y - 3, 3, col); Dot(p.x, p.y, 3, Wh);
            }
            else { Dot(p.x, p.y, 5, o); Dot(p.x, p.y, 3, col); }
        }
        static Color32 GlyphCol(char ch) { switch (ch) { case 'Y': return Y; case 'W': return Wh; case 'C': return Cy; case 'L': return Lt; default: return default; } }
        /* plate: 1 px dark outline, 3 px border, dark interior; origin = bottom-left of the border rect. The dark parts fade by alpha (they are dark, so no hue change); the coloured border, glyph and digits dissolve (opaque, keep = al). */
        void PlateBox(float x, float y, int w, int h, Color32 border, float al, int seed)
        {
            A(x - 1, y - 1, w + 2, h + 2, Fa(Ko, al)); A(x + 3, y + 3, w - 6, h - 6, Fa(Pl, al));
            Cells(x, y, w, 3, border, al, seed); Cells(x, y + h - 3, w, 3, border, al, seed + 1); Cells(x, y + 3, 3, h - 6, border, al, seed + 2); Cells(x + w - 3, y + 3, 3, h - 6, border, al, seed + 3);
        }
        void GlyphAt(float x, float y, int g, float al)
        {
            for (int r = 0; r < 7; r++) for (int col = 0; col < 8; col++)
            { char ch = Glyph[g][r][col]; if (ch == '.') continue; if (H((col >> 1) + 17 * g, (r >> 1) + 97) >= al) continue; A(x + col * 2, y + (6 - r) * 2, 2, 2, GlyphCol(ch)); }
        }
        void Number(float x, float y, int n, float al)
        {
            string s = n.ToString();
            for (int d = 0; d < s.Length; d++)
            {
                var gl = Digit[s[d] - '0']; float dx = x + d * 12;
                for (int r = 0; r < 5; r++) { int c0 = -1; for (int c = 0; c <= 3; c++) { bool on = c < 3 && gl[r][c] == 'X'; if (on && c0 < 0) c0 = c; if (!on && c0 >= 0) { if (H(d * 7 + c0 * 3 + c + 31, r + 211) < al) A(dx + c0 * 3, y + (4 - r) * 3, (c - c0) * 3, 3, Wh); c0 = -1; } } }
            }
        }

        void LateUpdate()
        {
            if (_top == null) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f), u = 1f / HellFX.PPU;
            for (int i = 0; i < _it.Length; i++)
            {
                if (!_it[i].used) continue; ref var it = ref _it[i]; it.t += dt;
                if (it.k == 0)
                {
                    if (it.pickT >= 0f) { it.pickT += dt; if (it.pickT >= Mathf.Max(LevelUpFX.ResumeFadeSeconds, 0.1f)) { Kill(Hd(i)); continue; } }
                    DrawScreen(ref it);
                }
                else
                {
                    if (it.t >= 0.30f) { Kill(Hd(i)); continue; }
                    if (it.follow) it.p = _hero;
                    DrawPick(ref it);
                }
            }
            _top.Upload(u); _gnd.Upload(u);
        }

        /* beat (a) + (b): everything is a one-shot. Ground parts (rings) sort under characters; the plate + sparkles are above but beside / over the hero's head only. */
        void DrawScreen(ref It it)
        {
            float t = it.t; Vector2 p = it.p;
            if (!it.chained)
            {
                _g = true;
                if (t < 0.30f) { float k = t / 0.30f, e = 1f - (1f - k) * (1f - k); Ring(new Vector2(p.x, p.y + 1), 8 + 36 * e, 0.6f, 3, Y, Mathf.Clamp01(1.4f * (1f - k)), 1); }
                if (t >= 0.06f && t < 0.38f) { float k = (t - 0.06f) / 0.32f, e = 1f - (1f - k) * (1f - k); Ring(new Vector2(p.x, p.y + 1), 6 + 50 * e, 0.6f, 3, Wh, Mathf.Clamp01(1.4f * (1f - k)), 2); }
                _g = false;
                for (int j = 0; j < 6; j++)
                {
                    float st = 0.04f + 0.05f * j, k = (t - st) / 0.55f; if (k < 0f || k > 1f) continue;
                    float side = (j & 1) == 0 ? -1f : 1f, off = 12f + 4f * (j >> 1);
                    Spark(new Vector2(p.x + side * off, p.y + 6 + 22 * k + 3 * (j >> 1)), Mathf.Sin(k * Mathf.PI), (j & 1) == 0 ? Y : Wh);
                }
            }
            float pa = it.pickT >= 0f ? Mathf.Clamp01(1f - it.pickT / 0.1f) : 1f;
            if (!it.chained)
            {
                float al = Mathf.Min(1f, t / 0.12f) * pa; float rise = (1f - Mathf.Min(1f, t / 0.12f)) * 6f; if (t > 1.1f) al *= Mathf.Clamp01(1f - (t - 1.1f) / 0.25f);
                if (al > 0.01f)
                {
                    int dg = it.level >= 10 ? 2 : 1; float iw = 3 + 16 + 4 + dg * 9 + (dg - 1) * 3 + 3, w = iw + 6, h = 28;
                    float x = Mathf.Round(p.x - w * 0.5f), y = Mathf.Round(p.y + LevelUpFX.HeroHeadPx + 4 - rise);
                    PlateBox(x, y, (int)w, (int)h, Y, al, 1);
                    GlyphAt(x + 6, y + 3 + 4, 0, al);
                    Number(x + 6 + 16 + 4, y + 3 + 3, it.level, al);
                }
            }
        }

        /* beat (c): 0.30 s. Ring (ground layer, 0.30 s) + plate (0.18 s) + 2 sparkles (0.27 s). */
        void DrawPick(ref It it)
        {
            float t = it.t, k = t / 0.30f; Vector2 p = it.p; int s = it.sub;
            Color32 rc = s == 2 ? Cy : s == 3 ? Lt : s == 4 ? Lt : Y;
            _g = true; Ring(new Vector2(p.x, p.y + 1), 6 + 24 * Mathf.Sqrt(k), 0.6f, 3, rc, Mathf.Clamp01(1.4f * (1f - k)), 3); _g = false;
            if (s == 4) return;
            float al = t < 0.08f ? 1f : Mathf.Clamp01(1f - (t - 0.08f) / 0.10f), rise = 4f * Mathf.Min(1f, t / 0.15f);
            float x = Mathf.Round(p.x - 14), y = Mathf.Round(p.y + LevelUpFX.HeroHeadPx + 4 + rise);
            PlateBox(x, y, 28, 26, s == 0 || s == 1 ? Y : s == 2 ? Cy : Lt, al, 5);
            GlyphAt(x + 6, y + 6, s, al);
            for (int j = 0; j < 2; j++) { float kk = (t - 0.03f * j) / 0.27f; if (kk < 0f || kk > 1f) continue; Spark(new Vector2(p.x + (j == 0 ? -20 : 20), p.y + 8 + 14 * kk + 4 * j), Mathf.Sin(kk * Mathf.PI), j == 0 ? Y : Wh); }
        }
    }
}
