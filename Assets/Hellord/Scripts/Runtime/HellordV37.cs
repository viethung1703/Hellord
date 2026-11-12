using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

// v0.0.37: Frozen Hell (floor 2) and Burning Hell (floor 3). Everything here is PLACEHOLDER: no new art, existing room art / demons /
// bosses are recoloured in code onto Endesga 32 ramps, hazards are drawn with generated flat shapes. All numbers: HellordTuning "v0.0.37".
namespace Hellord
{
    /// <summary>Gradient-map recolour (luminance -> ramp) of existing sprites. Textures are not readable, so they go through a RenderTexture first.</summary>
    public static class V37Recolor
    {
        static Color32 H(int rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
        // Endesga 32 hexes only (ramps dark -> light)
        public static readonly Color32[] Frost = { H(0x181425), H(0x262b44), H(0x3a4466), H(0x124e89), H(0x0099db), H(0x2ce8f5), H(0xc0cbdc), H(0xffffff) };
        public static readonly Color32[] Fire = { H(0x181425), H(0x3e2731), H(0x733e39), H(0xa22633), H(0xe43b44), H(0xf77622), H(0xfeae34), H(0xfee761) };
        public static readonly Color32[] Bone = { H(0x181425), H(0x3e2731), H(0x68386c), H(0xb55088), H(0xf6757a), H(0xe8b796), H(0xead4aa), H(0xffffff) };

        public static Color32[] RampFor(int floorIdx) => floorIdx == 1 ? Frost : floorIdx == 2 ? Fire : null;

        internal static Color32[] Pixels(Texture src, out int w, out int h)
        {
            w = src.width; h = src.height;
            if (src is Texture2D t2 && t2.isReadable) return t2.GetPixels32();
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active; Graphics.Blit(src, rt); RenderTexture.active = rt;
            var tmp = new Texture2D(w, h, TextureFormat.RGBA32, false, false); tmp.ReadPixels(new Rect(0, 0, w, h), 0, 0); tmp.Apply();
            RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
            var px = tmp.GetPixels32(); Object.Destroy(tmp); return px;
        }

        /// <summary>New point-filtered texture: every pixel's luminance x gain picks a colour from the ramp (quantize = nearest ramp colour, else blended).</summary>
        public static Texture2D Map(Texture src, Color32[] ramp, float gain, bool quantize)
        {
            var px = Pixels(src, out int w, out int h); int n = ramp.Length;
            var lut = new Color32[256];
            for (int i = 0; i < 256; i++)
            {
                float t = Mathf.Clamp01(i / 255f * gain) * (n - 1);
                if (quantize) lut[i] = ramp[Mathf.RoundToInt(t)];
                else { int a = Mathf.Min(n - 2, (int)t); lut[i] = Color32.Lerp(ramp[a], ramp[a + 1], t - a); }
            }
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i]; if (c.a == 0) continue;
                int l = (int)((0.299f * c.r + 0.587f * c.g + 0.114f * c.b) + 0.5f); var m = lut[Mathf.Clamp(l, 0, 255)]; px[i] = new Color32(m.r, m.g, m.b, c.a);
            }
            var o = new Texture2D(w, h, TextureFormat.RGBA32, false, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            o.SetPixels32(px); o.Apply(false, true); return o;
        }

        /// <summary>Recolours a sheet; sprites that share a texture share one recoloured texture. Same rects / pivots / PPU.</summary>
        public static Sprite[] Sprites(Sprite[] src, Color32[] ramp, float gain, bool quantize)
        {
            var outS = new Sprite[src.Length]; var texMap = new Dictionary<Texture, Texture2D>();
            for (int i = 0; i < src.Length; i++)
            {
                var s = src[i]; if (!s) continue;
                if (!texMap.TryGetValue(s.texture, out var nt)) texMap[s.texture] = nt = Map(s.texture, ramp, gain, quantize);
                var ns = Sprite.Create(nt, s.rect, new Vector2(s.pivot.x / s.rect.width, s.pivot.y / s.rect.height), s.pixelsPerUnit, 0, SpriteMeshType.FullRect); ns.name = s.name + "_v37"; outS[i] = ns;
            }
            return outS;
        }
    }

    public partial class HellordGame
    {
        static HellordTuning T37 => HellordTuning.I;
        // ---------- state ----------
        struct Ice37 { public Vector2 c; public float r; public int fx; public SpriteRenderer sr; }
        struct Crack37 { public Vector2[] p; public float off; public int fx; public SpriteRenderer[] sr; public string[] rows; public int po; }
        readonly List<Ice37> _ice = new List<Ice37>(); readonly List<Crack37> _cracks = new List<Crack37>();
        Vector2 _heroVel37; float _iceCarryT, _heroSlowT, _bossRewardT; bool _slotSkillPending, _bossReward37;
        string _chestTitle, _chestSub;
        readonly List<int> _bombQ = new List<int>();
        readonly Dictionary<int, Sprite[]> _variantF = new Dictionary<int, Sprite[]>();
        Sprite[] _bossFBase, _bossRageFBase, _bossFFrost, _bossRageFFrost, _bossFHorse, _bossRageFHorse, _bossFFire44, _bossRageFFire44;
        char[] _cycle37 = "CSTCH".ToCharArray(); int _spearLeft; bool _spear, _spearThenCharge; float _iceDeg, _iceTotal, _iceTickT;

        static readonly string[] FloorNames37 = { "OX PIT", "FROZEN HELL", "BURNING HELL" };
        string FloorName37() => FloorLabel53(ThemeSlot53, true); // v0.0.42: theme, not depth
        bool IsHorse => BossKind44 == 2; bool IsFrostOx => BossKind44 == 1; // v0.0.42: the boss follows the floor THEME (random order), not the depth
        string BossName37() => IsGorilla ? "ICE GORILLA" : IsQilin ? "QILIN CHARIOT" : IsHorse ? "HORSE-FACE" : IsFrostOx ? "FROST OX-HEAD" : "OX-HEAD";

        void ResetV37()
        {
            _ice.Clear(); _cracks.Clear(); _heroVel37 = Vector2.zero; _iceCarryT = _heroSlowT = _bossRewardT = 0; _slotSkillPending = _bossReward37 = false; _chestTitle = _chestSub = null;
            HData.BonusSlots = 0; Z37Clear(); ClearV44();
        }

        // ---------- ground decal drawer (pooled, immediate mode: Z37Begin ... draws ... Z37End each frame in Play) ----------
        const int Z37Order = -29500;
        readonly List<SpriteRenderer> _z37 = new List<SpriteRenderer>(); int _z37n; Transform _z37Root;
        Sprite _z37Disc, _z37Ring; readonly Dictionary<int, Sprite> _z37Wedge = new Dictionary<int, Sprite>();
        static Texture2D Z37Tex(int w, int h, System.Func<int, int, bool> fn)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; var px = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = fn(x, y) ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            t.SetPixels32(px); t.Apply(false, true); return t;
        }
        void Z37Init()
        {
            if (_z37Root) return; _z37Root = new GameObject("Z37_Decals").transform;
            var d = Z37Tex(64, 64, (x, y) => { float dx = x - 31.5f, dy = y - 31.5f; return dx * dx + dy * dy <= 32f * 32f; });
            _z37Disc = Sprite.Create(d, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
            var r = Z37Tex(64, 64, (x, y) => { float dx = x - 31.5f, dy = y - 31.5f, q = dx * dx + dy * dy; return q <= 32f * 32f && q >= 28f * 28f; });
            _z37Ring = Sprite.Create(r, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
        }
        Sprite Z37WedgeSprite(float halfDeg)
        {
            int key = Mathf.RoundToInt(halfDeg); if (_z37Wedge.TryGetValue(key, out var s)) return s;
            float lim = Mathf.Cos(key * Mathf.Deg2Rad);
            var t = Z37Tex(128, 128, (x, y) => { float dx = x - 63.5f, dy = y - 63.5f, m = Mathf.Sqrt(dx * dx + dy * dy); return m <= 64f && m > 0.5f && dx / m >= lim; });
            s = Sprite.Create(t, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect); return _z37Wedge[key] = s;
        }
        void Z37Begin() { Z37Init(); _z37n = 0; }
        void Z37End() { Tells46End(Time.deltaTime); Z37Hide(); }
        void Z37Hide() { for (int i = _z37n; i < _z37.Count; i++) if (_z37[i].enabled) _z37[i].enabled = false; }
        void Z37Clear() { _z37n = 0; Tells46Clear(); Z37Hide(); }
        SpriteRenderer Z37SR(Sprite s, Vector2 pos, Vector2 scale, float deg, Color c, int sub)
        {
            Z37Init(); if (_z37n >= _z37.Count) { var g = new GameObject("z"); g.transform.SetParent(_z37Root, false); var n = g.AddComponent<SpriteRenderer>(); _z37.Add(n); }
            var sr = _z37[_z37n++]; sr.sprite = s; sr.color = c; sr.sortingOrder = Z37Order + sub; sr.enabled = true;
            var tr = sr.transform; tr.position = new Vector3(pos.x, pos.y, 0); tr.rotation = Quaternion.Euler(0, 0, deg); tr.localScale = new Vector3(scale.x, scale.y, 1); return sr;
        }
        void ZDisc(Vector2 c, float r, Color col, float flat = 1f, int sub = 0) => Z37SR(_z37Disc, c, new Vector2(r * 2f, r * 2f * flat), 0, col, sub);
        void ZRing(Vector2 c, float r, Color col, float flat = 1f, int sub = 1) => Z37SR(_z37Ring, c, new Vector2(r * 2f, r * 2f * flat), 0, col, sub);
        void ZWedge(Vector2 o, float deg, float halfDeg, float reach, Color col, int sub = 0) => Z37SR(Z37WedgeSprite(halfDeg), o, new Vector2(reach * 2f, reach * 2f), deg, col, sub);
        /// <summary>Rotated box from a to b, width in world units.</summary>
        void ZSeg(Vector2 a, Vector2 b, float w, Color col, int sub)
        {
            Vector2 d = b - a; float len = d.magnitude; if (len < 0.01f) return;
            Z37SR(HArt.Pixel(), (a + b) * 0.5f, new Vector2((len + w * 0.5f) * HArt.PPU, w * HArt.PPU), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, col, sub);
        }
        static Color E32(int rgb, float a = 1f) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, a);

        // ---------- per-room hazards ----------
        void BuildHazards37(FloorRoom r)
        {
            foreach (var o in _ice) if (o.sr) Object.Destroy(o.sr.gameObject);
            foreach (var c in _cracks) if (c.sr != null) foreach (var q in c.sr) if (q) Object.Destroy(q.gameObject);
            _ice.Clear(); _cracks.Clear(); _heroVel37 = Vector2.zero; _iceCarryT = 0; Z37Clear();
            EnsureFX44(); StatusHazardFX.Clear(); _chillH44 = -1; ClearV44(); // v0.0.44: room change drops every pack effect (chill handle is re-made next frame)
            if (!floorMode || r == null || !r.IsFight || r.type == RoomType.Boss || _room == null) return;
            int fl = Theme; if (fl < 1) return; var T = T37; // v0.0.42: hazard kind = theme
            _room.Walkable(40f, out var lo, out var hi); if (hi.x < lo.x) { var t = lo; lo = hi; hi = t; } if (hi.y < lo.y) { float t = lo.y; lo.y = hi.y; hi.y = t; }
            var D = _room.D; float area = (D.w / (float)HArt.PPU) * (D.h / (float)HArt.PPU);
            var avoid = new List<Vector2> { _heroPos, HellordRoom.Px(D.w / 2f, D.h / 2f + 16f), HellordRoom.Px(D.spawnX > 0 ? D.spawnX : D.w / 2f, D.spawnY > 0 ? D.spawnY : D.h / 2f + 40f) };
            for (int d = 0; d < 4; d++) if (r.nb[d] >= 0 || (d == 0 && r.secretLink)) { var a = ArrivePx(d); var p = DoorPivotPx(d); avoid.Add(HellordRoom.Px(a.x, a.y)); avoid.Add(HellordRoom.Px(p.x, p.y)); }
            var rng = new System.Random(curRoom * 7919 + fl * 104729 + floor.Count);
            float F() => (float)rng.NextDouble();
            bool Free(Vector2 p, float extra) { foreach (var a in avoid) if ((a - p).magnitude < T.hazardSafeRadiusU + extra) return false; return p.x >= lo.x && p.x <= hi.x && p.y >= lo.y && p.y <= hi.y; }
            if (fl == 1)
            {
                int n = Mathf.Clamp(Mathf.RoundToInt(area / Mathf.Max(50f, T.icePatchAreaU2)), T.icePatchMin, T.icePatchMax);
                for (int i = 0, tries = 0; i < n && tries < 400; tries++)
                {
                    float rad = Mathf.Lerp(T.iceRadiusMinU, T.iceRadiusMaxU, F()); var c = new Vector2(Mathf.Lerp(lo.x, hi.x, F()), Mathf.Lerp(lo.y, hi.y, F()));
                    if (!Free(c, rad * 0.7f)) continue; bool ov = false; foreach (var o in _ice) if ((o.c - c).magnitude < o.r + rad) ov = true; if (ov) continue;
                    var sr = FrozenIce44(c, rad);
                    _ice.Add(new Ice37 { c = c, r = rad, sr = sr, fx = sr == null && T44.fxV44Pack ? StatusHazardFX.IcePatch(c, rad) : -1 }); i++;
                }
            }
            else if (HellordTuning.I.burningTilesG && BurningTilesG.Piece("H_0", 0) != null)
            {
                int n = Mathf.Clamp(Mathf.RoundToInt(area / Mathf.Max(50f, T.lavaAreaU2)), T.lavaMin, T.lavaMax);
                LavaGridG44(D, n, rng, Free);
            }
            else
            {
                int n = Mathf.Clamp(Mathf.RoundToInt(area / Mathf.Max(50f, T.lavaAreaU2)), T.lavaMin, T.lavaMax);
                for (int i = 0, tries = 0; i < n && tries < 400; tries++)
                {
                    int segs = 3 + (int)(F() * 2.99f); var pts = new Vector2[segs + 1]; pts[0] = new Vector2(Mathf.Lerp(lo.x, hi.x, F()), Mathf.Lerp(lo.y, hi.y, F()));
                    float ang = F() * 360f; bool ok = Free(pts[0], 0.8f);
                    for (int k = 1; k <= segs && ok; k++) { ang += (F() - 0.5f) * 80f; pts[k] = pts[k - 1] + new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)) * Mathf.Lerp(1.5f, 2.6f, F()); ok = Free(pts[k], 0.8f); }
                    if (!ok) continue;
                    _cracks.Add(new Crack37 { p = pts, off = F() * (T.lavaDormantS + T.lavaWarnS + T.lavaActiveS), fx = T44.fxV44Pack ? StatusHazardFX.LavaCrack(pts, T.lavaWidthU) : -1 }); i++;
                }
            }
        }

        /// <summary>v0.0.44: Burning G lava cracks: random walk on the 32px floor grid, pieces join at tile-edge midpoints, both ends capped with end pieces, one pulse offset (0-5) per crack.</summary>
        void LavaGridG44(RoomData D, int n, System.Random rng, System.Func<Vector2, float, bool> free)
        {
            var T = HellordTuning.I; const int G = 32; int gx0 = (D.wallSide + G - 1) / G, gx1 = (D.w - D.wallSide) / G - 1, gy1 = (D.h - D.wallS - D.wallN) / G - 1;
            if (gx1 < gx0 || gy1 < 0) return;
            var used = new HashSet<int>(); int[] dx = { 0, 1, 0, -1 }, dy = { -1, 0, 1, 0 };
            Vector2 Ctr(int x, int y) => HellordRoom.Px(x * G + G / 2f, D.wallN + y * G + G / 2f);
            bool Ok(int x, int y) => x >= gx0 && x <= gx1 && y >= 0 && y <= gy1 && !used.Contains(y * 1000 + x) && free(Ctr(x, y), 0.9f);
            for (int i = 0, tries = 0; i < n && tries < 300; tries++)
            {
                int len = T.lavaGLenMin + rng.Next(Mathf.Max(1, T.lavaGLenMax - T.lavaGLenMin + 1)); len = Mathf.Max(2, len);
                int x = gx0 + rng.Next(gx1 - gx0 + 1), y = rng.Next(gy1 + 1); if (!Ok(x, y)) continue;
                var cx = new List<int> { x }; var cy = new List<int> { y }; var loc = new HashSet<int> { y * 1000 + x }; int dir = rng.Next(4);
                while (cx.Count < len)
                {
                    int nd = rng.NextDouble() < T.lavaGTurnChance ? (dir + (rng.Next(2) == 0 ? 1 : 3)) % 4 : dir;
                    int ax = x + dx[nd], ay = y + dy[nd];
                    if (!Ok(ax, ay) || loc.Contains(ay * 1000 + ax)) { nd = dir; ax = x + dx[nd]; ay = y + dy[nd]; if (!Ok(ax, ay) || loc.Contains(ay * 1000 + ax)) break; }
                    dir = nd; x = ax; y = ay; cx.Add(x); cy.Add(y); loc.Add(y * 1000 + x);
                }
                if (cx.Count < 2) continue;
                int m = cx.Count; var rows = new string[m]; var pts = new Vector2[m]; var srs = new SpriteRenderer[m];
                for (int k = 0; k < m; k++)
                {
                    bool N = false, S = false, E = false, W = false;
                    for (int j = k - 1; j <= k + 1; j += 2)
                    {
                        if (j < 0 || j >= m) continue; int ddx = cx[j] - cx[k], ddy = cy[j] - cy[k];
                        if (ddx == 1) E = true; else if (ddx == -1) W = true; else if (ddy == 1) S = true; else N = true;
                    }
                    rows[k] = W && E ? "H_" + rng.Next(3) : N && S ? "V_" + rng.Next(2) : W && N ? "bend_W_N" : W && S ? "bend_W_S" : E && N ? "bend_E_N" : E && S ? "bend_E_S" : E ? "end_E" : W ? "end_W" : N ? "end_N" : "end_S";
                    pts[k] = Ctr(cx[k], cy[k]);
                    var go = new GameObject("lavaA_" + rows[k]); go.transform.SetParent(_room.Root, false); go.transform.position = HellordRoom.Px(cx[k] * G, D.wallN + cy[k] * G);
                    var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = BurningTilesG.Piece(rows[k], 0); sr.sortingOrder = HellordRoom.BaseOrder + 110; srs[k] = sr;
                    if (T.lightsSourcedV46 && T.lavaGlow46) HellordRoom.SourceGlow46(go.transform, cx[k] * G + G / 2f, D.wallN + cy[k] * G + G / 2f, T.lavaGlowRadiusPx46, T.lavaGlowK46, HellordRoom.LightOrder + 290); /* v0.0.46: lava cell glow, destroyed with the cell */
                }
                foreach (var c in loc) used.Add(c);
                _cracks.Add(new Crack37 { p = pts, sr = srs, rows = rows, po = rng.Next(6), off = (float)rng.NextDouble() * LavaGCycle44(), fx = -1 }); i++;
            }
        }
        static float LavaGTellS44() { var st = BurningTilesG.States; float j = st != null && st.wake_tell != null ? st.wake_tell.ms.Sum44() / 1000f : 0.39f; return HellordTuning.I.lavaGTellS > 0 ? HellordTuning.I.lavaGTellS : j; }
        static float LavaGCoolS44() { var st = BurningTilesG.States; return st != null && st.cooldown != null ? st.cooldown.ms.Sum44() / 1000f : 0.36f; }
        static float LavaGCycle44() => HellordTuning.I.lavaDormantS + LavaGTellS44() + HellordTuning.I.lavaActiveS + LavaGCoolS44();
        /// <summary>State (0 asleep, 1 tell, 2 active, 3 cooldown) and sheet column for a grid crack at phase ph.</summary>
        static int LavaGCol44(float ph, int po, out int st)
        {
            var T = HellordTuning.I; var S = BurningTilesG.States; float tell = LavaGTellS44(), cool = LavaGCoolS44();
            if (ph < T.lavaDormantS) { st = 0; return 0; }
            ph -= T.lavaDormantS;
            if (ph < tell) { st = 1; int[] ms = S != null && S.wake_tell != null ? S.wake_tell.ms : new[] { 160, 130, 100 }; float k = ms.Sum44() / 1000f / Mathf.Max(0.01f, tell); return 1 + Mathf.Clamp(MsCol44(ms, ph * k, false), 0, 2); }
            ph -= tell;
            if (ph < T.lavaActiveS) { st = 2; int[] ms = S != null && S.active != null ? S.active.ms : new[] { 120, 120, 120, 120, 120, 120 }; return 4 + (MsCol44(ms, ph, true) + po) % 6; }
            ph -= T.lavaActiveS; st = 3;
            int[] cm = S != null && S.cooldown != null ? S.cooldown.ms : new[] { 120, 120, 120 }; return 2 - Mathf.Clamp(MsCol44(cm, Mathf.Min(ph, cool - 0.001f), false), 0, 2);
        }

        Sprite[] _iceF44;
        /// <summary>v0.0.44: Frozen floor B ice patch sprite (64x28, 4f glint loop from the json) stretched over the slide ellipse, floor-level under every sprite. Null = tileset off / missing.</summary>
        SpriteRenderer FrozenIce44(Vector2 c, float rad)
        {
            var T = HellordTuning.I; if (!T.frozenTilesB || Theme != 1 || _room == null) return null;
            _iceF44 = FrozenTilesB.IceFrames(out _); if (_iceF44 == null || _iceF44.Length == 0) return null;
            var go = new GameObject("frozen_ice_patch"); go.transform.SetParent(_room.Root, false); go.transform.position = c;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _iceF44[0]; sr.sortingOrder = HellordRoom.BaseOrder + 100; sr.color = new Color(1, 1, 1, T.frozenIceAlpha);
            var b = _iceF44[0].bounds.size; float k = Mathf.Max(0.05f, T.frozenIceScale);
            go.transform.localScale = new Vector3(2f * rad * k / Mathf.Max(0.01f, b.x), 2f * rad * 0.7f * k / Mathf.Max(0.01f, b.y), 1);
            return sr;
        }
        bool OnIce37(Vector2 p) { foreach (var o in _ice) { float dx = (p.x - o.c.x) / o.r, dy = (p.y - o.c.y) / (o.r * 0.7f); if (dx * dx + dy * dy < 1f) return true; } return false; }
        float HeroSlowMul37() => _heroSlowT > 0 ? T37.frostSlowMul : 1f;
        void ApplyFrostSlow37(float secs)
        {
            bool fresh = _heroSlowT <= 0; _heroSlowT = Mathf.Max(_heroSlowT, secs);
            if (fresh) { HordeFX.CheapBurst(HeroCenter, E32(0xc0cbdc), 8); HellordAudio.Play("frost_cleave"); }
        }
        /// <summary>Hero velocity: direct on normal ground, low acceleration on ice, short carry after leaving ice.</summary>
        Vector2 HeroVel37(Vector2 mv, float spd, float dt)
        {
            Vector2 want = mv * spd;
            if (_ice.Count > 0 && OnIce37(_heroPos)) { _iceCarryT = T37.iceCarryS; _heroVel37 = Vector2.MoveTowards(_heroVel37, want, T37.iceAccelU * dt); }
            else if (_iceCarryT > 0) { _iceCarryT -= dt; _heroVel37 = Vector2.MoveTowards(_heroVel37, want, T37.iceCarryAccelU * dt); }
            else _heroVel37 = want;
            return _heroVel37;
        }
        void HeroVelClamp37(Vector2 before, float dt)
        {
            if (_heroVel37 == Vector2.zero) return; Vector2 exp = before + _heroVel37 * dt;
            if (Mathf.Abs(_heroPos.x - exp.x) > 1e-3f) _heroVel37.x = 0; if (Mathf.Abs(_heroPos.y - exp.y) > 1e-3f) _heroVel37.y = 0;
        }

        void UpdateHazards37(float dt)
        {
            if (_heroSlowT > 0) _heroSlowT -= dt; UpdateV44(dt);
            if (_slotSkillPending && mode == Mode.Play) { _slotSkillPending = false; OpenSlotSkillPick37(); }
            if (_bossRewardT > 0) { _bossRewardT -= dt; if (_bossRewardT <= 0) { _bossRewardT = 0; OpenBossReward37(); } }
            var T = T37; bool grace = _roomT < T.hazardGraceS;
            if (_ice.Count > 0 && mode == Mode.Play && T44.fxV44Pack && (_iceCarryT > 0 || OnIce37(_heroPos)) && _heroVel37.sqrMagnitude > 0.5f && Time.frameCount % Mathf.Max(1, T44.fxIceSkidEveryFrames) == 0) StatusHazardFX.IceSkid(_heroPos, _heroVel37);
            if (_ice.Count > 0 && _iceF44 != null) { var fr = _iceF44[Mathf.Clamp(FrozenTilesB.IceFrame(_roomT), 0, _iceF44.Length - 1)]; foreach (var o in _ice) if (o.sr && o.sr.sprite != fr) o.sr.sprite = fr; }
            foreach (var o in _ice)
            {
                if (o.fx >= 0 || o.sr) continue;
                ZDisc(o.c, o.r, E32(0xc0cbdc, 0.42f), 0.7f, 0); ZDisc(o.c, o.r * 0.72f, E32(0x0099db, 0.18f), 0.7f, 1); ZRing(o.c, o.r, E32(0xffffff, 0.55f), 0.7f, 2);
                ZSeg(o.c + new Vector2(-o.r * 0.3f, o.r * 0.1f), o.c + new Vector2(-o.r * 0.05f, o.r * 0.22f), 0.09f, E32(0xffffff, 0.8f), 3);
                ZSeg(o.c + new Vector2(o.r * 0.2f, -o.r * 0.18f), o.c + new Vector2(o.r * 0.45f, -o.r * 0.05f), 0.09f, E32(0xffffff, 0.8f), 3);
            }
            if (_cracks.Count > 0)
            {
                float cyc = T.lavaDormantS + T.lavaWarnS + T.lavaActiveS; float half = T.lavaWidthU * 0.5f + 0.3f;
                foreach (var c in _cracks)
                {
                    if (c.sr != null)
                    {
                        int gst = 0, col = grace ? 0 : LavaGCol44((_roomT + c.off) % LavaGCycle44(), c.po, out gst);
                        for (int k = 0; k < c.sr.Length; k++) if (c.sr[k]) c.sr[k].sprite = BurningTilesG.Piece(c.rows[k], col);
                        if (gst == 2 && !grace && mode == Mode.Play)
                            for (int k = 0; k < c.p.Length - 1; k++)
                                if (DistSeg37(_heroPos, c.p[k], c.p[k + 1]) < T.lavaGHitHalfU) { HurtHero(T.lavaDmg * (1 + T.floorEnemyDmgGrowth * FloorIdx), "lava"); break; }
                        continue;
                    }
                    float ph = (_roomT + c.off) % cyc; int st = grace ? 0 : ph < T.lavaDormantS ? 0 : ph < T.lavaDormantS + T.lavaWarnS ? 1 : 2;
                    float wp = (ph - T.lavaDormantS) / Mathf.Max(0.01f, T.lavaWarnS);
                    if (c.fx >= 0) StatusHazardFX.LavaSet(c.fx, st == 0 ? LavaState.Dormant : st == 1 ? LavaState.Warning : LavaState.Active, st == 1 ? Mathf.Clamp01(wp) : st == 2 ? Mathf.Clamp01((ph - T.lavaDormantS - T.lavaWarnS) / Mathf.Max(0.01f, T.lavaActiveS)) : 0f);
                    else for (int k = 0; k < c.p.Length - 1; k++)
                    {
                        Vector2 a = c.p[k], b = c.p[k + 1];
                        ZSeg(a, b, T.lavaWidthU * 1.15f, E32(0x181425, 0.75f), 0);
                        if (st == 0) ZSeg(a, b, T.lavaWidthU * 0.3f, E32(0x733e39, 1f), 1);
                        else if (st == 1) ZSeg(a, b, T.lavaWidthU * (0.3f + 0.4f * wp), E32(0xa22633), 1);
                        else { ZSeg(a, b, T.lavaWidthU, E32(0xe43b44), 1); ZSeg(a, b, T.lavaWidthU * 0.6f, E32(0xf77622), 2); ZSeg(a, b, T.lavaWidthU * 0.25f, E32(0xfee761), 3); }
                    }
                    if (st == 2 && mode == Mode.Play)
                        for (int k = 0; k < c.p.Length - 1; k++)
                            if (DistSeg37(_heroPos, c.p[k], c.p[k + 1]) < half) { HurtHero(T.lavaDmg * (1 + T.floorEnemyDmgGrowth * FloorIdx), "lava"); break; }
                }
            }
        }
        static float DistSeg37(Vector2 p, Vector2 a, Vector2 b) { Vector2 ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude)); return (p - (a + ab * t)).magnitude; }

        // ---------- demon variants (1 = frost, 2 = exploding) ----------
        Sprite[] VariantFrames37(EType t, int variant)
        {
            int key = (int)t * 4 + variant; if (_variantF.TryGetValue(key, out var f) && f != null && f.Length > 0 && f[0]) return f;
            f = V37Recolor.Sprites(_eSprites[t], variant == 1 ? V37Recolor.Frost : V37Recolor.Fire, T37.spriteRecolorGain, true); _variantF[key] = f; return f;
        }
        void ApplyVariant37(Enemy e)
        {
            if (e == null || !floorMode) return; if (V49 && FloorIdx == 0 && FightsCleared49() < T49.floor0VariantRooms49f) return; var T = T37; int fl = Theme; // v0.0.42: variant = theme
            bool basic = e.type == EType.Imp || e.type == EType.Ghoul || e.type == EType.Brute || e.type == EType.Splitter;
            if (!basic) return;
            if (ApplyMortar53(e)) return; // v0.0.53 torture gaps: Mortar floor demons
            if (ApplyVariant44(e)) return;
            if (fl == 1 && Random.value < T.frostChance) { e.variant = 1; e.maxHp *= T.frostHpMul; e.hp = e.maxHp; e.speed *= T.frostSpeedMul; }
            else if (fl == 2 && (e.type == EType.Imp || e.type == EType.Ghoul) && Random.value < T.bomberChance) { e.variant = 2; e.maxHp *= T.bomberHpMul; e.hp = e.maxHp; e.speed *= T.bomberSpeedMul; e.fuseT = -1; }
            else return;
            e.frames = VariantFrames37(e.type, e.variant); e.sr.sprite = e.frames[0];
        }

        /// <summary>Exploding demon: lights its fuse near the hero, creeps, flashes + shows the blast ring, then bursts. Returns true if it exploded (enemy done this frame).</summary>
        bool BomberThink37(Enemy e, ref Vector2 v, float dist, float dt, out Color tint)
        {
            var T = T37; tint = Color.white;
            if (e.fuseT < 0) { if (dist < T.bomberTriggerU) { e.fuseT = 0; HellordAudio.Play("curse_rune_arm"); } return false; }
            if (e.freezeT <= 0) e.fuseT += dt; v *= T.bomberFuseMoveMul; float p = Mathf.Clamp01(e.fuseT / Mathf.Max(0.1f, T.bomberFuseS));
            bool on = Mathf.Sin(e.fuseT * (12f + 24f * p)) > 0; tint = on ? new Color(1f, 0.4f, 0.3f) : Color.white;
            ZDisc(e.pos, T.bomberRadiusU * p, E32(0xe43b44, 0.28f), 1f, 0); ZRing(e.pos, T.bomberRadiusU, on ? E32(0xfee761, 0.9f) : E32(0xf77622, 0.8f), 1f, 1);
            if (e.fuseT < T.bomberFuseS) return false;
            var c = e.pos; float R = T.bomberRadiusU;
            HordeFX.CheapBurst(c + Vector2.up * 0.5f, E32(0xf77622), 22); HordeFX.CheapBurst(c + Vector2.up * 0.5f, E32(0xfee761), 10); ScreenShake.Add(2f, 0.2f); HellordAudio.Play("fireball_impact");
            if ((_heroPos - c).magnitude < R + 0.3f * HeroHitK47) HurtHero(T.bomberHeroDmg * (1 + (T.soften679 ? T.enemyDmgGrowth679 : T.floorEnemyDmgGrowth) * FloorIdx), "bomber");
            e.fuseT = -1; e.variant = 0;
            Query(c, R, _bombQ);
            for (int i = 0; i < _bombQ.Count; i++) { var o = _en[_bombQ[i]]; if (o == e || !o.alive || o.dying || o.boss || o.nest || (o.pos - c).magnitude > R + o.radius) continue; Damage(o, T.bomberEnemyDmg, false, false); }
            if (e.alive) KillEnemy(e);
            return true;
        }

        // ---------- boss look / cycle ----------
        void SetBossLook37()
        {
            if (_bossFBase == null) { _bossFBase = _bossF; _bossRageFBase = _bossRageF; }
            var T = T37; int fl = BossKind44; // v0.0.44: boss look = boss kind (0 ox, 1/3 frost ramp, 2 bone ramp, 4 fire ramp)
            if (fl == 1 || fl == 3)
            {
                if (_bossFFrost == null) { _bossFFrost = V37Recolor.Sprites(_bossFBase, V37Recolor.Frost, T.spriteRecolorGain, true); _bossRageFFrost = V37Recolor.Sprites(_bossRageFBase, V37Recolor.Frost, T.spriteRecolorGain, true); }
                _bossF = _bossFFrost; _bossRageF = _bossRageFFrost;
            }
            else if (fl == 4)
            {
                if (_bossFFire44 == null) { _bossFFire44 = V37Recolor.Sprites(_bossFBase, V37Recolor.Fire, T.spriteRecolorGain, true); _bossRageFFire44 = V37Recolor.Sprites(_bossRageFBase, V37Recolor.Fire, T.spriteRecolorGain, true); }
                _bossF = _bossFFire44; _bossRageF = _bossRageFFire44;
            }
            else if (fl == 2)
            {
                if (MortarBoss53) { var hf = HorseFrames53() ?? HArt.Sheet("boss_horseface_pestle"); if (_bossFPestle53 == null) { _bossFPestle53 = hf; _bossRageFPestle53 = V37Recolor.Sprites(hf, V37Recolor.Fire, T.spriteRecolorGain, true); } _bossF = _bossFPestle53; _bossRageF = _bossRageFPestle53; } // v0.0.53 torture gaps
                else { if (_bossFHorse == null) { _bossFHorse = V37Recolor.Sprites(_bossFBase, V37Recolor.Bone, T.spriteRecolorGain, true); _bossRageFHorse = V37Recolor.Sprites(_bossRageFBase, V37Recolor.Bone, T.spriteRecolorGain, true); }
                _bossF = _bossFHorse; _bossRageF = _bossRageFHorse; }
            }
            else { _bossF = _bossFBase; _bossRageF = _bossRageFBase; }
            _boss.frames = _bossF; _bossBody.sprite = _bossF[0];
            string s = fl == 3 ? T44.gorillaCycle : fl == 4 ? T44.qilinCycle : fl == 1 ? T.bossCycleFloor2 : fl == 2 ? T.bossCycleFloor3 : T.bossCycleFloor1; SetupBoss44(); s = (s ?? "").ToUpperInvariant().Replace(" ", "");
            _cycle37 = s.Length > 0 ? s.ToCharArray() : "CSTCH".ToCharArray(); _spear = false; _spearLeft = 0; _spearThenCharge = false;
        }
        void StartCycleAttack37(char c, float dist)
        {
            if (BossKind44 >= 3) { StartAtk44(c); return; }
            _spear = false; _spearLeft = 0; _spearThenCharge = false; _spearFollow = false; var T = T37;
            switch (c)
            {
                case 'S': StartAttack(PickAttack(BS.Sweep, dist)); break;
                case 'T': StartAttack(BS.Stomp); break;
                case 'H': StartAttack(BS.Herd); break;
                case 'L': StartAttack(BS.LungeTell); break;
                case 'I': StartIce37(); break;
                case 'P': _spear = true; _spearLeft = 1; StartAttack(BS.LungeTell); break;
                case 'X': _spear = true; _spearLeft = 1; _spearThenCharge = true; StartAttack(BS.LungeTell); break;
                case 'K': _spear = true; _spearLeft = Mathf.Max(2, T.horseComboThrusts); StartAttack(BS.LungeTell); break;
                default: StartAttack(BS.ChargeTell); break;
            }
        }
        float IceOriginY => 0.4f;
        void StartIce37()
        {
            var T = T37; _iceTotal = Mathf.Max(0.5f, T.bossIceTellS / Rage); AimIce37(); _iceTickT = 0; _squash = -0.12f;
            Play("horn_windup", true); HellordAudio.Stop("oxhead_horn_windup"); HellordAudio.Play("oxhead_horn_windup"); Go(BS.IceTell, _iceTotal);
        }
        Vector2 IceOrigin37() => _boss.pos + Vector2.up * IceOriginY;
        void AimIce37() { Vector2 d = _heroPos - IceOrigin37(); _iceDeg = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; }
        bool InIce37(Vector2 p) { var T = T37; Vector2 o = IceOrigin37(), d = p - o; if (d.magnitude > T.bossIceReachU) return false; return Vector2.Angle(new Vector2(Mathf.Cos(_iceDeg * Mathf.Deg2Rad), Mathf.Sin(_iceDeg * Mathf.Deg2Rad)), d) < T.bossIceHalfAngleDeg; }
        void UpdateIce37(Enemy e, float dt)
        {
            var T = T37; Vector2 o = IceOrigin37();
            if (_bs == BS.IceTell)
            {
                _squash = Mathf.MoveTowards(_squash, -0.12f, dt);
                if (_bsT > _iceTotal * (1f - T.bossIceAimLock)) AimIce37();
                float p = 1f - Mathf.Clamp01(_bsT / _iceTotal); bool locked = _bsT <= _iceTotal * (1f - T.bossIceAimLock);
                if (T.crispTellsV46) TellWedge46(o, _iceDeg, T.bossIceHalfAngleDeg, T.bossIceReachU, E32(0x2ce8f5), p, 0);
                else
                {
                    ZWedge(o, _iceDeg, T.bossIceHalfAngleDeg, T.bossIceReachU, E32(0x0099db, 0.12f + 0.08f * (locked ? 1 : 0)), 0);
                    ZWedge(o, _iceDeg, T.bossIceHalfAngleDeg, T.bossIceReachU * p, E32(0x2ce8f5, 0.22f), 1);
                }
                if (_bsT <= 0)
                {
                    HellordAudio.Stop("oxhead_horn_windup"); HellordAudio.Play("frost_cleave"); Play("roar", true); _squash = 0.12f; ScreenShake.Add(1.5f, 0.2f); Go(BS.IceBreath, T.bossIceBreathS);
                }
            }
            else
            {
                float f = Mathf.Clamp01(_bsT / Mathf.Max(0.1f, T.bossIceBreathS));
                ZWedge(o, _iceDeg, T.bossIceHalfAngleDeg, T.bossIceReachU, E32(0xc0cbdc, 0.35f + 0.25f * f), 0); ZWedge(o, _iceDeg, T.bossIceHalfAngleDeg * 0.5f, T.bossIceReachU, E32(0xffffff, 0.3f), 1);
                _iceTickT -= dt;
                if (_iceTickT <= 0 && InIce37(_heroPos))
                {
                    _iceTickT = 0.25f; int hh = heroHits; HurtHero(T.bossIceDmg, "boss_ice", true); if (heroHits != hh) ApplyFrostSlow37(T.bossIceSlowS);
                }
                if (Random.value < 0.5f) { float a = (_iceDeg + (Random.value * 2f - 1f) * T.bossIceHalfAngleDeg) * Mathf.Deg2Rad; HordeFX.CheapBurst(o + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(1f, T.bossIceReachU), E32(0xc0cbdc), 2); }
                if (_bsT <= 0) Go(BS.Recover, 0.7f);
            }
        }
        /// <summary>After a spear thrust ends: next thrust of the combo, a charge, or recover.</summary>
        void SpearEnd37()
        {
            _spearLeft--;
            if (_spearLeft > 0) { _spearFollow = true; StartAttack(BS.LungeTell); return; }
            _spear = false;
            if (_spearThenCharge) { _spearThenCharge = false; StartAttack(BS.ChargeTell); return; }
            Go(BS.Recover, 0.45f);
        }

        // ---------- boss reward ----------
        static readonly StatDef SlotStat37 = new StatDef { id = StatId.Slot, name = "New Skill Slot", desc = "Unlocks your next skill slot" };
        void FloorBossRewardStart37() { if (FloorIdx < FloorCount - 1) _bossRewardT = Mathf.Max(0.2f, T37.bossRewardDelayS); }
        bool BossRewardPending37 => _bossRewardT > 0 || _slotSkillPending;
        void OpenBossReward37()
        {
            var T = T37; var pick = new List<Card>(); bool slot = HData.SlotsUnlocked(level) < HellordTuning.I.skillSlotsMax818;
            var sups = new List<Card>(); var upg = new List<Card>();
            foreach (var sd in HData.Supports) if (skills.Exists(s => s.CanSocket(sd) == null)) { var sc = new Card { kind = CardKind.Support, support = sd }; if (skills.Exists(s => s.CanUpgrade(sd))) upg.Add(sc); else sups.Add(sc); }
            Shuffle(upg); Shuffle(sups); upg.AddRange(sups);
            var lvl = new List<Card>(); for (int i = 0; i < skills.Count; i++) if (skills[i].level < HData.MaxLv(skills[i].def)) lvl.Add(new Card { kind = CardKind.SkillLevel, skill = skills[i].def, skillIndex = i }); Shuffle(lvl);
            if (slot) pick.Add(new Card { kind = CardKind.Stat, stat = SlotStat37 });
            for (int i = 0; i < upg.Count && i < 2 && pick.Count < T.bossRewardCards; i++) pick.Add(upg[i]);
            if (lvl.Count > 0 && pick.Count < T.bossRewardCards) pick.Add(lvl[0]);
            for (int i = 2; i < upg.Count && pick.Count < T.bossRewardCards; i++) pick.Add(upg[i]);
            if (pick.Count == 0) { bones += T.bossRewardBones; Toast("BOSS REWARD: +" + T.bossRewardBones + " BONES"); return; }
            cards.Clear(); cards.AddRange(pick); warning = null; _bossReward37 = true; _chestTitle = "BOSS REWARD"; _chestSub = "TAKE 1  -  " + "THE " + BossName37() + "'S SPOILS" /* v0.0.42: theme boss name */;
            HellordAudio.Play("chest_open"); HitStop.Play(0.05f); mode = Mode.Chest;
        }
        void OpenSlotSkillPick37()
        {
            var news = new List<Card>(); foreach (var d in HData.Skills) if (!skills.Exists(s => s.def == d)) news.Add(new Card { kind = CardKind.NewSkill, skill = d });
            if (news.Count == 0 || skills.Count >= HData.SlotsUnlocked(level)) return;
            Shuffle(news); cards.Clear(); for (int i = 0; i < Mathf.Min(3, news.Count); i++) cards.Add(news[i]);
            warning = null; _bossReward37 = true; _chestTitle = "NEW SKILL SLOT"; _chestSub = "PICK THE SKILL FOR IT"; mode = Mode.Chest;
        }
    }
}
