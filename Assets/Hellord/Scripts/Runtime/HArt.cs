using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.6: hero = approved final2 (full size, ~22px) rebuilt as 16-frame strips; enemies/boss/hazards/UI from the 2026-10-26 export.
// v0.3: loads the Hellord Art export (Art/export, manifest v1) from Resources/HellordArt and slices the horizontal strips
// at runtime with Sprite.Create. PPU = 16 everywhere (hero 32px = 2 units; 384x216 target = 24 x 13.5 units).
// Every sheet's frame size / pivot / frame ranges are copied from manifest.json into the tables below.
// If a sheet is missing, the code-drawn placeholder is used instead (nothing breaks).
namespace Hellord
{
    /// <summary>Strip layout from the art manifest. Pivot is in pixels measured from the TOP-LEFT of a frame (manifest convention).</summary>
    public struct SheetDef
    {
        public string file; public int w, h, frames; public Vector2 pivotPx;
        public SheetDef(string file, int w, int h, int frames, float px, float py) { this.file = file; this.w = w; this.h = h; this.frames = frames; pivotPx = new Vector2(px, py); }
    }

    /// <summary>ONE place to change which enemy sheet each gameplay type uses. Swap e.g. Elite to enemy_knight + a tint here.</summary>
    public static class EnemyArtMap
    {
        public struct Entry { public string sheet; public Color tint; public float scale; }
        public static readonly Dictionary<EType, Entry> Map = new Dictionary<EType, Entry>
        {
            // v0.6: Hellord Art export 2026-10-26 (PixelLab size-32 roster, native size, 4-direction walk + 8-frame death).
            { EType.Imp,        new Entry { sheet = "enemy_imp_v2",      tint = Color.white, scale = 1f } },
            { EType.Ghoul,      new Entry { sheet = "enemy_shambler_v2", tint = Color.white, scale = 1f } },
            { EType.Brute,      new Entry { sheet = "enemy_brute",       tint = Color.white, scale = 1f } },
            { EType.Elite,      new Entry { sheet = "enemy_wraith_v2",   tint = Color.white, scale = 1f } }, // floating wraith; shadow baked in
            { EType.Splitter,   new Entry { sheet = "enemy_splitter",    tint = Color.white, scale = 1f } },
            { EType.SplitChild, new Entry { sheet = "enemy_split_child", tint = Color.white, scale = 1f } },
            { EType.HookThrower, new Entry { sheet = "PLACEHOLDER_hook_thrower", tint = Color.white, scale = 1f } }, // v0.0.24: code-drawn (HookThrowerArt)
            { EType.GossipImp,  new Entry { sheet = "PLACEHOLDER_gossip_imp", tint = Color.white, scale = 1f } }, // v0.0.25: code-drawn (GossipImpArt)
        };
    }

    public static class HArt
    {
        public const float PPU = 16f;
        // ---- manifest tables ----
        public const int HeroIdle0 = 0, HeroIdleN = 8, HeroWalk0 = 8, HeroWalkN = 8; public const float HeroIdleFps = 8, HeroWalkFps = 12;
        // v0.6 enemy layout (all v2 sheets): walk_south 0-3, walk_east 4-7, walk_north 8-11, walk_west 12-15 @12fps, death 16-23 @12fps.
        // No hit frame: the hit flash is a material swap (HordeFX Flash), so SpriteRenderer.color (our tint owner) is untouched.
        public const int EnemyWalkN = 4, EnemyDeath0 = 16, EnemyDeathN = 8, SplitFrame = 4; public const float EnemyWalkFps = 12, EnemyDeathFps = 12;
        /// <summary>Walk row start for a move direction (S=0, E=4, N=8, W=12).</summary>
        public static int WalkRow(Vector2 v) => Mathf.Abs(v.x) >= Mathf.Abs(v.y) ? (v.x >= 0 ? 4 : 12) : (v.y >= 0 ? 8 : 0);
        public const float PickupFps = 6, ProjFps = 14;
        static readonly Dictionary<string, SheetDef> Sheets = new Dictionary<string, SheetDef>
        {
            // v0.6 export (frame w/h, frames, pivot px from top-left = json "pivot", real feet)
            { "enemy_imp_v2",      new SheetDef("enemy_imp_v2",      31, 38, 24, 15.5f, 37) },
            { "boss_ice_gorilla",  new SheetDef("boss_ice_gorilla",  80, 80, 128, 40f, 74) }, // v0.0.44 PixelLab sheet, 8 cols x 16 rows, feet 6px above the cell bottom
            { "enemy_frost_caster", new SheetDef("enemy_frost_caster", 38, 32, 96, 19f, 27) },
            { "enemy_bloater", new SheetDef("enemy_bloater", 38, 32, 96, 19f, 27) }, // v0.0.49e PixelLab v14 Mortar, Frost Caster layout
            { "enemy_rice_ghost", new SheetDef("enemy_rice_ghost", 38, 32, 96, 19f, 27) }, // v0.0.49e PixelLab v14 Mortar, Frost Caster layout
            { "enemy_grinder_monk", new SheetDef("enemy_grinder_monk", 38, 32, 96, 19f, 27) }, // v0.0.49e PixelLab v14 Mortar, Frost Caster layout // v0.0.44 PixelLab Frost Caster, 8 cols x 12 rows, feet 5px above the cell bottom
            { "enemy_fire_wheel",   new SheetDef("enemy_fire_wheel",   40, 40, 40, 20f, 36) }, // v0.0.44 Fire Wheel, 8 cols x 5 rows, side view, ground 4px above the cell bottom
            { "boss_qilin_chariot", new SheetDef("boss_qilin_chariot", 160, 144, 171, 80f, 136) }, // v0.0.44 VH PixelLab object 27bb782f, 9 cols x 19 rows (13-18 hurt/stun/death S/E), wheel line 8px above the cell bottom
            { "enemy_shambler_v2", new SheetDef("enemy_shambler_v2", 22, 39, 24, 11f, 38) },
            { "enemy_wraith_v2",   new SheetDef("enemy_wraith_v2",   24, 41, 24, 12f, 40) },
            { "enemy_brute",       new SheetDef("enemy_brute",       47, 50, 24, 23.5f, 49) },
            { "enemy_splitter",    new SheetDef("enemy_splitter",    60, 40, 24, 30f, 39) },
            { "enemy_split_child", new SheetDef("enemy_split_child", 27, 25, 24, 13.5f, 24) },
            // exported but no matching gameplay yet (kept for later): archer, hellcat, calf, herder, ox north/south
            { "enemy_archer",      new SheetDef("enemy_archer",      38, 38, 24, 19f, 37) },
            { "enemy_hellcat",     new SheetDef("enemy_hellcat",     46, 30, 24, 23f, 29) },
            { "enemy_calf",        new SheetDef("enemy_calf",        34, 34, 54, 17f, 33) },
            { "enemy_herder",      new SheetDef("enemy_herder",      38, 35, 41, 19f, 34) },
            { "hazard_ox_run_east",  new SheetDef("hazard_ox_run_east",  40, 30, 6, 20f, 29) },
            { "hazard_ox_run_north", new SheetDef("hazard_ox_run_north", 30, 40, 6, 15f, 39) },
            { "hazard_ox_run_south", new SheetDef("hazard_ox_run_south", 30, 40, 6, 15f, 39) },
            { "gate_bone_dropgate",  new SheetDef("gate_bone_dropgate",  48, 48, 7, 24f, 47) },
            { "boss_oxhead_clubjailer",           new SheetDef("boss_oxhead_clubjailer",           72, 72, 80, 36, 66) },
            { "boss_oxhead_clubjailer_bloodrage", new SheetDef("boss_oxhead_clubjailer_bloodrage", 72, 72, 80, 36, 66) },
            { "boss_horseface_pestle",            new SheetDef("boss_horseface_pestle",            96, 96, 106, 48, 88) }, // t501u: PixelLab v138 (21x6 grid); HorseFrames53 remaps to Ox slots // v0.0.53 torture gaps: Mortar boss (MOCK-UP art, ox-head slot layout)
            { "xp_blood_small",  new SheetDef("xp_blood_small",  12, 12, 4, 6, 9) },
            { "xp_blood_medium", new SheetDef("xp_blood_medium", 12, 12, 4, 6, 9) },
            { "xp_blood_large",  new SheetDef("xp_blood_large",  12, 12, 4, 6, 9) },
            { "bone_shard",      new SheetDef("bone_shard",      12, 12, 4, 6, 9) },
            { "bone_skull",      new SheetDef("bone_skull",      12, 12, 4, 6, 10) },
        };
        // hero: all layers 32x32, 16 frames, pivot (16,30). Sort offset relative to the base (manifest draw order; base = 0).
        public static readonly Dictionary<string, int> HeroLayerSort = new Dictionary<string, int>
        {
            { "hero_up_wings", -1 }, { "hero_up_frost", 1 }, { "hero_up_toxic", 1 }, { "hero_up_bleed", 2 }, { "hero_up_chains", 3 },
            { "hero_up_glass", 4 }, { "hero_up_hellfire", 5 }, { "hero_up_shock", 6 }, { "hero_up_curse", 7 },
        };

        /// <summary>Ox-Head Club Jailer animations (boss_oxhead_clubjailer.json): first frame, per-frame durations_ms, loop. E anims flip X for west.</summary>
        public struct BossAnim { public int from; public int[] ms; public bool loop; public BossAnim(int f, bool l, params int[] d) { from = f; loop = l; ms = d; } }
        public static readonly Dictionary<string, BossAnim> BossAnims = new Dictionary<string, BossAnim>
        {
            { "idle", new BossAnim(0, true, 150, 150, 150, 150, 150, 150) },
            { "move_S", new BossAnim(6, true, 83, 83, 83, 83, 83, 83, 83, 83) },
            { "move_E", new BossAnim(14, true, 83, 83, 83, 83, 83, 83, 83, 83) },
            { "move_N", new BossAnim(22, true, 83, 83, 83, 83, 83, 83, 83, 83) },
            { "gore_windup_S", new BossAnim(30, false, 140, 140, 260) },
            { "gore_loop_S", new BossAnim(33, true, 70, 70, 70, 70) },
            { "gore_windup_E", new BossAnim(37, false, 140, 140, 260) },
            { "gore_loop_E", new BossAnim(40, true, 70, 70, 70, 70) },
            { "horn_windup", new BossAnim(44, false, 130, 130, 240) },
            { "horn_strike", new BossAnim(47, false, 70, 90, 160) },
            { "stomp_windup", new BossAnim(50, false, 150, 260) },
            { "stomp_strike", new BossAnim(52, false, 120, 180) },
            { "roar", new BossAnim(54, false, 120, 150, 300, 150) },
            { "hurt", new BossAnim(58, false, 70, 110) },
            { "stun_loop", new BossAnim(60, true, 160, 160, 160, 160) },
            { "phase_change", new BossAnim(64, false, 150, 150, 200, 90, 90, 300) },
            { "death", new BossAnim(70, false, 120, 120, 120, 120, 120, 120, 120, 120, 120, 600) },
        };
        /// <summary>Frame index inside an anim after t seconds (holds the last frame when not looping); done = past the end.</summary>
        public static int BossFrame(BossAnim a, float t, out bool done)
        {
            int total = 0; foreach (var d in a.ms) total += d; int ms = (int)(t * 1000f); done = !a.loop && ms >= total;
            if (a.loop) ms %= total; else if (ms >= total) return a.from + a.ms.Length - 1;
            for (int i = 0; i < a.ms.Length; i++) { if (ms < a.ms[i]) return a.from + i; ms -= a.ms[i]; }
            return a.from + a.ms.Length - 1;
        }
        /// <summary>Gate B frames: closed 0, shake 1-2 (14fps loop), open 3-6 (12fps), close = 5,4,3,0.</summary>
        public const int GateClosed = 0, GateShake0 = 1, GateOpen0 = 3, GateOpenN = 4;

        /// <summary>Boss HP bar states (216x16): 0 full, 1 hit, 2 phase, 3 bloodrage, 4 rage, 5 empty. 14px skull cap + 200x10 bar.</summary>
        public static Texture2D HpBar() { var t = Resources.Load<Texture2D>("HellordArt/ui_boss_hpbar_oxhead"); if (t) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        public const int HpBarW = 216, HpBarH = 16, HpBarStates = 6, HpBarCap = 14, HpBarFill = 200;

        // v0.6 hero readability: 1px warm cream outline (#FFDB9E) at 0.6 strength, generated from the layer's own pixels (needs Read/Write,
        // set by HellordArtImporter for hero_*). Returns null when the texture is not readable (outline is then skipped).
        public static readonly Color32 OutlineCol = new Color32(0xFF, 0xDB, 0x9E, 255), OutlineInk = new Color32(0x18, 0x14, 0x25, 230); // v0.0.9: full-strength cream + dark outer ring (was cream @0.6)
        public static Sprite[] HeroOutline(string layer, int W = 32, int H = 32, float pivX = 16, float pivY = 30)
        {
            string key = layer + "__outline"; if (_cache.TryGetValue(key, out var c)) return c;
            var src = Resources.Load<Texture2D>("HellordArt/" + layer); Sprite[] res = null;
            if (src != null && src.isReadable && src.width >= W * 16 && src.height >= H)
            {
                const int N = 16; var px = src.GetPixels32(); int tw = src.width, oy = src.height - H;
                var o = new Color32[W * N * H];
                for (int f = 0; f < N; f++) for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
                {
                    bool Op(int xx, int yy) => xx >= 0 && xx < W && yy >= 0 && yy < H && px[(oy + yy) * tw + f * W + xx].a > 127;
                    if (Op(x, y)) continue;
                    if (Op(x - 1, y) || Op(x + 1, y) || Op(x, y - 1) || Op(x, y + 1)) o[y * W * N + f * W + x] = OutlineCol;
                }
                for (int f = 0; f < N; f++) for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
                {   // second pass: 1px ink ring outside the cream ring (8-neighbour so corners close)
                    int i0 = y * W * N + f * W + x; if (o[i0].a != 0) continue;
                    bool Op(int xx, int yy) => xx >= 0 && xx < W && yy >= 0 && yy < H && px[(oy + yy) * tw + f * W + xx].a > 127;
                    if (Op(x, y)) continue;
                    bool Cr(int xx, int yy) => xx >= 0 && xx < W && yy >= 0 && yy < H && o[yy * W * N + f * W + xx].a == 255;
                    for (int dy = -1; dy <= 1 && o[i0].a == 0; dy++) for (int dx = -1; dx <= 1; dx++) if (Cr(x + dx, y + dy)) { o[i0] = OutlineInk; break; }
                }
                var t = new Texture2D(W * N, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                t.SetPixels32(o); t.Apply(false, true);
                res = new Sprite[N]; var piv = new Vector2(pivX / W, (H - pivY) / H);
                for (int i = 0; i < N; i++) res[i] = Sprite.Create(t, new Rect(i * W, 0, W, H), piv, PPU, 0, SpriteMeshType.FullRect);
            }
            _cache[key] = res; return res;
        }

        static readonly Dictionary<string, Sprite[]> _cache = new Dictionary<string, Sprite[]>();
        public static bool Missing(string name) => Resources.Load<Texture2D>("HellordArt/" + name) == null;
        public static Sprite[] Strip(string name, int frames, int w, int h, Vector2 pivotPxFromTop)
        {
            if (_cache.TryGetValue(name, out var s)) return s;
            var tex = Resources.Load<Texture2D>("HellordArt/" + name);
            // v0.0.9: a sheet is either one horizontal strip or a row-major grid (top row first) <= 2048 px wide (WebGL texture limit;
            // tools/grid_boss_sheets.py re-packs the 5760 px boss strips into 28x3).
            int cols = tex != null ? tex.width / w : 0, rows = tex != null ? tex.height / h : 0;
            if (tex == null || cols * rows < frames || cols == 0 || rows == 0)
            {
                if (tex != null) Debug.LogWarning($"[Hellord] {name}: {tex.width}x{tex.height} cannot hold {frames} frames of {w}x{h}; using placeholder.");
                else Debug.LogWarning($"[Hellord] art '{name}' missing in Resources/HellordArt; using placeholder.");
                s = Fallback(name, frames); _cache[name] = s; return s;
            }
            tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp;
            s = new Sprite[frames];
            var piv = new Vector2(pivotPxFromTop.x / w, (h - pivotPxFromTop.y) / h);
            for (int i = 0; i < frames; i++) { int cx = i % cols, cy = i / cols; s[i] = Sprite.Create(tex, new Rect(cx * w, tex.height - h * (cy + 1), w, h), piv, PPU, 0, SpriteMeshType.FullRect); s[i].name = name + "_" + i; }
            _cache[name] = s; return s;
        }
        public static Sprite[] Sheet(string name) { var d = Sheets[name]; return Strip(name, d.frames, d.w, d.h, d.pivotPx); }
        static Sprite[] Fallback(string name, int frames)
        {
            var pal = new Dictionary<char, Color32> { { 'k', new Color32(20, 8, 12, 255) }, { 'r', new Color32(178, 24, 32, 255) }, { 'o', new Color32(240, 120, 30, 255) }, { 'y', new Color32(255, 222, 96, 255) } };
            string[] rows = { "..k..k..", ".krkkrk.", "krrrrrrk", "kryrryrk", "krrrrrrk", ".krrrrk.", ".kr..rk.", ".kk..kk." };
            var sp = PixelSpriteFactory.FromRows(rows, pal, PPU, 0, name);
            var a = new Sprite[frames]; for (int i = 0; i < frames; i++) a[i] = sp; return a;
        }
        public static Sprite[] Hero(string layer) => Strip(layer, 16, 32, 32, new Vector2(16, 30));
        public static Sprite[] Enemy(EType t) => EnemyArtMap.Map[t].sheet == "PLACEHOLDER_hook_thrower" ? HookThrowerArt.Placeholder() : EnemyArtMap.Map[t].sheet == "PLACEHOLDER_gossip_imp" ? GossipImpArt.Placeholder() : Sheet(EnemyArtMap.Map[t].sheet);
        /// <summary>XP blood tier from value (manifest xp_value 1/5/25): 0 small, 1 medium, 2 large.</summary>
        public static int BloodTier(int xp) => xp >= 25 ? 2 : xp >= 5 ? 1 : 0;
        public static Sprite[] Blood(int tier) => Sheet(tier >= 2 ? "xp_blood_large" : tier == 1 ? "xp_blood_medium" : "xp_blood_small");
        public static Sprite[] Bone(bool skull) => Sheet(skull ? "bone_skull" : "bone_shard");
        /// <summary>Projectile strips: 16x16, 4 frames, pivot (8,8), art faces +X.</summary>
        public static Sprite[] Proj(string name) => Strip(name, 4, 16, 16, new Vector2(8, 8));

        static Sprite _ground, _chest, _px;
        public static Sprite Pixel() => _px ? _px : (_px = PixelSpriteFactory.Rect(1, 1, new Color32(255, 255, 255, 255), PPU, "hpx"));
        /// <summary>floor_style14 (96x96 seamless) tiled; procedural placeholder when missing.</summary>
        public static Sprite Ground()
        {
            if (_ground) return _ground;
            var t = Resources.Load<Texture2D>("HellordArt/floor_style14");
            if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Repeat; return _ground = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect); }
            const int N = 64; var px = new Color32[N * N];
            for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
            {
                uint h = (uint)(x * 374761393 + y * 668265263); h = (h ^ (h >> 13)) * 1274126177; int v = (int)(h & 255);
                bool seam = (x % 16 == 0) || ((y + (x / 16 % 2) * 8) % 16 == 0);
                var c = new Color32(28, 16, 20, 255);
                if (v < 18) c = new Color32(36, 20, 24, 255); else if (v > 248) c = new Color32(52, 22, 26, 255);
                if (seam) c = new Color32(20, 10, 14, 255);
                px[y * N + x] = c;
            }
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            tex.SetPixels32(px); tex.Apply();
            _ground = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
            return _ground;
        }
        public static Sprite Chest()
        {
            if (_chest) return _chest;
            var pal = new Dictionary<char, Color32> { { 'k', new Color32(20, 8, 12, 255) }, { 'b', new Color32(110, 50, 30, 255) }, { 'g', new Color32(255, 200, 60, 255) }, { 'w', new Color32(255, 246, 220, 255) } };
            string[] rows = { ".kkkkkkkkkk.", "kbbbbbbbbbbk", "kbbbbggbbbbk", "kggggwwggggk", "kbbbbggbbbbk", "kbbbbbbbbbbk", "kbbbbbbbbbbk", ".kkkkkkkkkk." };
            return _chest = PixelSpriteFactory.FromRows(rows, pal, PPU, 0, "chest");
        }
    }
}
