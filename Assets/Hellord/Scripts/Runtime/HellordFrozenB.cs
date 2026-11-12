using System.Collections.Generic;
using UnityEngine;

// v0.0.44: Frozen Hell floor B (Art/Tiles/FrozenHell, approved 2026-10-28). Sizes, names, weights and timings come from
// Resources/HellordArt/FrozenHell/frozen_tileset.json. Theme 1 rooms get a runtime-composed base (floor 32px variants,
// 48px north wall = 16px cap row + 32px face, 24px side walls, 16px south cap row, AO strip) instead of the recoloured Ox Pit bake.
namespace Hellord
{
    [System.Serializable] public class FzCells { public string file; public int cols, count; public int[] cell; public int frames; public string[] names, rows; public int[] pick_weights, weights; public int[] ms; public float[] pivot; }
    [System.Serializable] public class FzAo { public int[] ao_base, ao_side_L, ao_side_R; }
    [System.Serializable] public class FzAoDef { public string file; public FzAo rects_xywh; }
    [System.Serializable] public class FzTileset { public int tile_size_px; public FzCells floor, wall_cap, wall_north_face, wall_side, hazard_ice_patch; public FzLava lava_pieces; public FzAoDef wall_ao; public FzLavaStates lava_states; }
    [System.Serializable] public class FzLava { public string file; public int[] cell; public string[] rows, cols; }
    [System.Serializable] public class FzLavaState { public int[] cols, ms; }
    [System.Serializable] public class FzLavaStates { public FzLavaState asleep, wake_tell, active, cooldown; }

    /// <summary>v0.0.44: Burning Hell floor G (Art/Tiles/BurningHell, approved 2026-10-28), same composer as Frozen B; lava crack A pieces for the lava hazard.</summary>
    public static class BurningTilesG
    {
        public static readonly RoomTiles Tiles = new RoomTiles("HellordArt/BurningHell/", "burning_tileset", 0, true);
        static Sprite[] _lava; static int _rows, _cols;
        /// <summary>Lava piece sprite for row name and state column (pivot top-left), null when missing.</summary>
        public static Sprite Piece(string row, int col)
        {
            var d = Tiles.Def; if (d == null || d.lava_pieces == null) return null;
            if (_lava == null)
            {
                var lp = d.lava_pieces; var t = Resources.Load<Texture2D>("HellordArt/BurningHell/" + System.IO.Path.GetFileNameWithoutExtension(lp.file)); if (t == null) return null;
                t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; int w = lp.cell[0], h = lp.cell[1]; _cols = t.width / w; _rows = t.height / h; _lava = new Sprite[_rows * _cols];
                for (int r = 0; r < _rows; r++) for (int c = 0; c < _cols; c++) { var sp = Sprite.Create(t, new Rect(c * w, t.height - h * (r + 1), w, h), new Vector2(0, 1), HArt.PPU, 0, SpriteMeshType.FullRect); sp.name = "lavaA_" + r + "_" + c; _lava[r * _cols + c] = sp; }
            }
            int ri = System.Array.IndexOf(d.lava_pieces.rows, row); if (ri < 0 || ri >= _rows) return null;
            return _lava[ri * _cols + Mathf.Clamp(col, 0, _cols - 1)];
        }
        public static FzLavaStates States => Tiles.Def != null ? Tiles.Def.lava_states : null;
    }

    /// <summary>v0.0.44: composes a whole-room base from a 32px floor-variant tileset + Ox Pit style walls (48px north = cap + face, 24px sides, 16px south caps, AO). Shared by Frozen B and Burning G.</summary>
    public class RoomTiles
    {
        readonly string Dir, _json; readonly int _accentFrom; readonly bool _sideAlt;
        FzTileset _ts; bool _tried; static Texture2D _lastTex;
        /// <summary>accentFrom: first floor variant that may never touch another (-1 = tunable frozenFloorAccentFrom, 0 = no rule). sideAlt: side walls alternate _0/_1 down the wall instead of random.</summary>
        public RoomTiles(string dir, string json, int accentFrom, bool sideAlt) { Dir = dir; _json = json; _accentFrom = accentFrom; _sideAlt = sideAlt; }
        public FzTileset Def
        {
            get
            {
                if (_tried) return _ts; _tried = true;
                var ta = Resources.Load<TextAsset>(Dir + _json);
                if (ta == null) { Debug.LogWarning("[Hellord] " + _json + ".json missing"); return null; }
                _ts = JsonUtility.FromJson<FzTileset>(ta.text); return _ts;
            }
        }
        public static string Name(string file) => System.IO.Path.GetFileNameWithoutExtension(file);
        public bool Available => Def != null && Resources.Load<Texture2D>(Dir + Name(Def.floor.file)) != null;

        class Src { public Color32[] px; public int w, h; }
        Src Load(string file)
        {
            var t = Resources.Load<Texture2D>(Dir + Name(file)); if (t == null) return null;
            var px = V37Recolor.Pixels(t, out int w, out int h); return new Src { px = px, w = w, h = h };
        }

        /// <summary>Copy src rect (top-left px) into dst at (dx, dy) top-left px; y-down coords, alpha-over blend.</summary>
        static void Blit(Color32[] dst, int W, int H, Src s, int sx, int sy, int w, int h, int dx, int dy, bool flip)
        {
            if (s == null) return;
            for (int y = 0; y < h; y++)
            {
                int ty = dy + y; if (ty < 0 || ty >= H) continue; int syy = sy + y; if (syy < 0 || syy >= s.h) continue;
                int drow = (H - 1 - ty) * W, srow = (s.h - 1 - syy) * s.w;
                for (int x = 0; x < w; x++)
                {
                    int tx = dx + x; if (tx < 0 || tx >= W) continue; int sxx = sx + (flip ? w - 1 - x : x); if (sxx < 0 || sxx >= s.w) continue;
                    var c = s.px[srow + sxx]; if (c.a == 0) continue;
                    if (c.a == 255) { dst[drow + tx] = c; continue; }
                    var d = dst[drow + tx]; float a = c.a / 255f;
                    dst[drow + tx] = new Color32((byte)(d.r + (c.r - d.r) * a), (byte)(d.g + (c.g - d.g) * a), (byte)(d.b + (c.b - d.b) * a), (byte)Mathf.Max(d.a, c.a));
                }
            }
        }
        static int Pick(System.Random r, int[] wts, int from, int to)
        {
            int sum = 0; for (int i = from; i < to; i++) sum += Mathf.Max(0, wts[i]); if (sum <= 0) return from;
            int k = r.Next(sum); for (int i = from; i < to; i++) { k -= Mathf.Max(0, wts[i]); if (k < 0) return i; }
            return to - 1;
        }
        int Cap(string n) { var a = Def.wall_cap.names; for (int i = 0; i < a.Length; i++) if (a[i] == n) return i; return 0; }

        /// <summary>Whole-room base sprite (pivot top-left like the Ox Pit bake). Null if the art or json is missing.</summary>
        public Sprite BuildBase(RoomData D, int seed)
        {
            var ts = Def; if (ts == null) return null; var T = HellordTuning.I;
            bool burnOld = Dir.Contains("Burning") && !T.burningFloorV48; /* v0.0.48: false = old v0.0.44 crack sheet */
            var fl = Load(burnOld ? "burningG_floor_variants_32px.png" : ts.floor.file); if (fl == null) return null;
            var cap = Load(ts.wall_cap.file); var face = Load(ts.wall_north_face.file); var side = Load(ts.wall_side.file); var ao = Load(ts.wall_ao.file);
            int W = D.w, H = D.h, ft = ts.tile_size_px > 0 ? ts.tile_size_px : 32; var px = new Color32[W * H];
            var rng = new System.Random(seed);
            // floor: weighted variants, accents (index >= T.frozenFloorAccentFrom) never touch another accent (8-neighbour, already-placed cells)
            var wts = ts.floor.pick_weights != null && ts.floor.pick_weights.Length > 0 ? ts.floor.pick_weights : ts.floor.weights;
            if (burnOld) wts = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            else if (Dir.Contains("Burning")) { wts = (int[])wts.Clone(); if (wts.Length > 10) for (int i = 10; i < wts.Length; i++) wts[i] = Mathf.Max(0, T.burningFloorAccentWeight); }
            if (burnOld && T.burningFloorMatchV46 && T.burningFloorSkipVariants != null) { wts = (int[])wts.Clone(); foreach (int sv in T.burningFloorSkipVariants) if (sv >= 0 && sv < wts.Length) wts[sv] = 0; } /* v0.0.46: only matching crack variants */
            int nv = burnOld ? 11 : Mathf.Min(wts.Length, ts.floor.count > 0 ? ts.floor.count : ts.floor.names.Length), cols = Mathf.Max(1, ts.floor.cols);
            int acc = _accentFrom < 0 ? Mathf.Clamp(T.frozenFloorAccentFrom, 1, nv) : _accentFrom == 0 ? nv : Mathf.Clamp(_accentFrom, 1, nv);
            if (Dir.Contains("Burning") && !burnOld) acc = Mathf.Clamp(10, 1, nv); /* v0.0.48: crack/scorch/ember accents never touch */
            int gx = (W + ft - 1) / ft, gy = (H - D.wallN + ft - 1) / ft; var g = new int[gx * gy];
            for (int y = 0; y < gy; y++) for (int x = 0; x < gx; x++)
            {
                int v = Pick(rng, wts, 0, nv);
                if (v >= acc)
                {
                    bool near = false;
                    for (int oy = -1; oy <= 0; oy++) for (int ox = -1; ox <= 1; ox++)
                    {
                        if (oy == 0 && ox >= 0) continue; int nx = x + ox, ny = y + oy; if (nx < 0 || ny < 0 || nx >= gx) continue;
                        if (g[ny * gx + nx] >= acc) near = true;
                    }
                    if (near) v = Pick(rng, wts, 0, acc);
                }
                g[y * gx + x] = v; bool flip = (_accentFrom < 0 ? T.frozenFloorFlipX : T.burningFloorFlipX) && rng.Next(2) == 1;
                Blit(px, W, H, fl, (v % cols) * ft, (v / cols) * ft, ft, ft, x * ft, D.wallN + y * ft, flip);
            }
            // AO under the walls (drawn on the floor before the walls)
            if (ao != null && ts.wall_ao.rects_xywh != null)
            {
                var r = ts.wall_ao.rects_xywh;
                if (r.ao_base != null && r.ao_base.Length == 4) for (int x = D.wallSide; x < W - D.wallSide; x += r.ao_base[2]) Blit(px, W, H, ao, r.ao_base[0], r.ao_base[1], Mathf.Min(r.ao_base[2], W - D.wallSide - x), r.ao_base[3], x, D.wallN, false);
                if (r.ao_side_L != null && r.ao_side_L.Length == 4) for (int y = D.wallN; y < H - D.wallS; y += r.ao_side_L[3]) Blit(px, W, H, ao, r.ao_side_L[0], r.ao_side_L[1], r.ao_side_L[2], Mathf.Min(r.ao_side_L[3], H - D.wallS - y), D.wallSide, y, false);
                if (r.ao_side_R != null && r.ao_side_R.Length == 4) for (int y = D.wallN; y < H - D.wallS; y += r.ao_side_R[3]) Blit(px, W, H, ao, r.ao_side_R[0], r.ao_side_R[1], r.ao_side_R[2], Mathf.Min(r.ao_side_R[3], H - D.wallS - y), W - D.wallSide - r.ao_side_R[2], y, false);
            }
            int cw = ts.wall_cap.cell != null && ts.wall_cap.cell.Length == 2 ? ts.wall_cap.cell[0] : 16, ch = ts.wall_cap.cell != null && ts.wall_cap.cell.Length == 2 ? ts.wall_cap.cell[1] : 16;
            // north face: plain / damage / icicle / chain by tunable weights (json: plain 70%, others 10% each)
            if (face != null)
            {
                var fwts = ts.wall_north_face.weights != null && ts.wall_north_face.weights.Length > 0 ? ts.wall_north_face.weights : T.frozenFaceWeights;
                int fw = ts.wall_north_face.cell[0], fh = ts.wall_north_face.cell[1], fy = D.wallN - fh, nf = Mathf.Min(face.w / Mathf.Max(1, fw), fwts.Length);
                for (int x = 0; x < W; x += fw) Blit(px, W, H, face, Pick(rng, fwts, 0, nf) * fw, 0, fw, fh, x, fy, false);
            }
            // side walls 24x16: L_0/L_1 on the west, R_0/R_1 on the east
            if (side != null)
            {
                int sw = ts.wall_side.cell[0], sh = ts.wall_side.cell[1];
                for (int y = ch, k = 0; y < H - D.wallS; y += sh, k++)
                {
                    int a = _sideAlt ? k & 1 : rng.Next(2), b = _sideAlt ? k & 1 : rng.Next(2);
                    Blit(px, W, H, side, a * sw, 0, sw, Mathf.Min(sh, H - D.wallS - y), 0, y, false);
                    Blit(px, W, H, side, (2 + b) * sw, 0, sw, Mathf.Min(sh, H - D.wallS - y), W - sw, y, false);
                }
            }
            // cap rows: north row (cap_S faces the wall face below), south row (cap_N faces the floor above), corners
            if (cap != null)
            {
                int cS = Cap("cap_S"), cN = Cap("cap_N"), cSE = Cap("cap_SE"), cSW = Cap("cap_SW"), cNE = Cap("cap_NE"), cNW = Cap("cap_NW");
                int a1 = System.Array.IndexOf(ts.wall_cap.names, "cap_S_alt1"), a2 = System.Array.IndexOf(ts.wall_cap.names, "cap_S_alt2");
                var cyc = a1 >= 0 && a2 >= 0 ? new[] { cS, a1, a2 } : new[] { cS };
                for (int x = 0, k = 0; x < W; x += cw, k++)
                {
                    int top = x == 0 ? cSE : x + cw >= W ? cSW : cyc[k % cyc.Length], bot = x == 0 ? cNE : x + cw >= W ? cNW : cN;
                    Blit(px, W, H, cap, top * cw, 0, cw, ch, x, 0, false);
                    for (int y = H - D.wallS; y < H; y += ch) Blit(px, W, H, cap, bot * cw, 0, cw, ch, x, y, false);
                }
            }
            if (_lastTex) Object.Destroy(_lastTex);
            var t = new Texture2D(W, H, TextureFormat.RGBA32, false, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "frozen_floorB_base" };
            t.SetPixels32(px); t.Apply(false, true); _lastTex = t;
            var s = Sprite.Create(t, new Rect(0, 0, W, H), new Vector2(0, 1), HArt.PPU, 0, SpriteMeshType.FullRect); s.name = t.name; return s;
        }

    }

    public static class FrozenTilesB
    {
        public static readonly RoomTiles Tiles = new RoomTiles("HellordArt/FrozenHell/", "frozen_tileset", -1, false);
        public static bool Available => Tiles.Available;
        public static FzTileset Def => Tiles.Def;
        public static Sprite BuildBase(RoomData D, int seed) => Tiles.BuildBase(D, seed);
        const string Dir = "HellordArt/FrozenHell/";
        static Sprite[] _ice; static float[] _iceS;

        /// <summary>Ice patch hazard frames (centre pivot) and their seconds, from the json.</summary>
        public static Sprite[] IceFrames(out float[] secs)
        {
            secs = _iceS; if (_ice != null) return _ice; var ts = Def; if (ts == null || ts.hazard_ice_patch == null) return null;
            var h = ts.hazard_ice_patch; var t = Resources.Load<Texture2D>(Dir + RoomTiles.Name(h.file)); if (t == null) return null;
            t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp;
            int w = h.cell[0], hh = h.cell[1], n = Mathf.Min(h.frames, t.width / w); var piv = h.pivot != null && h.pivot.Length == 2 ? new Vector2(h.pivot[0], h.pivot[1]) : new Vector2(0.5f, 0.5f);
            _ice = new Sprite[n]; _iceS = new float[n];
            for (int i = 0; i < n; i++) { _ice[i] = Sprite.Create(t, new Rect(i * w, t.height - hh, w, hh), piv, HArt.PPU, 0, SpriteMeshType.FullRect); _ice[i].name = "frozen_ice_" + i; _iceS[i] = (h.ms != null && i < h.ms.Length ? h.ms[i] : 160) / 1000f; }
            secs = _iceS; return _ice;
        }
        public static int IceFrame(float t)
        {
            if (_iceS == null || _iceS.Length == 0) return 0; float tot = 0; foreach (var s in _iceS) tot += s; if (tot <= 0) return 0;
            t %= tot; for (int i = 0; i < _iceS.Length; i++) { t -= _iceS[i]; if (t < 0) return i; }
            return _iceS.Length - 1;
        }
    }
}
