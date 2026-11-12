using System.Collections.Generic;
using UnityEngine;

// v0.7: Ox Pit rooms from the art handoff (export/floor/oxpit_room_lanes.json / oxpit_room_clip.json).
// Static layer (floor, decals, stampede lanes, walls 48px north / 24px sides / 16px south, AO, gate frames and the
// cluster props, all in json draw order) is baked offline by tools/bake_oxpit.py into <room>_base.png (checked pixel-exact
// against the handoff reference PNGs). Runtime adds the animated braziers (4f @ 8fps) and the light overlays
// (warm / brazier / gate glow = additive, shadow pools = multiply x0.55) from <room>_data.json.
// World mapping: room pixel (x, y-down from the top-left) -> world (x/16, -y/16). Room top-left = world origin.
namespace Hellord
{
    [System.Serializable] public class RoomBrazier { public int x, y, f; }
    [System.Serializable] public class RoomLight { public string s; public int x, y, mul; public float k; }
    [System.Serializable] public class RoomGate { public int x, open; }
    [System.Serializable] public class RoomChest { public int x, y, elite; }
    /// <summary>v0.0.26: a decor prop drawn as its own y-sorted sprite (tools/bake_rooms_v26.py). x,y = sprite top-left px, ax,ay = pivot (feet) px in the room, ox,oy = pivot px inside the sprite, k = baked light factor.</summary>
    [System.Serializable] public class RoomProp { public string s; public int x, y, ax, ay, ox, oy; public float k = 1; }
    [System.Serializable] public class RoomData
    {
        public int w, h, wallN, wallS, wallSide; public RoomProp[] props;
        public RoomBrazier[] braziers; public RoomLight[] lights; public RoomGate[] gates;
        // v0.8 (tools/bake_rooms_v08.py): door spot on the N/S walls (x) and E/W walls (y), plus special-room anchors (0 = none)
        public int doorX, doorY, spawnX, spawnY, shopX, shopY, stairsX, stairsY, shrineX, shrineY; public RoomChest[] chests;
    }

    public class HellordRoom
    {
        public const int BaseOrder = -32000, PropOrder = -31000, GateOrder = -30900, LightOrder = -30000;
        public string Name; public RoomData D; public Transform Root;
        readonly List<SpriteRenderer> _braz = new List<SpriteRenderer>(); public readonly List<(SpriteRenderer sr, float k, bool warm)> Lights = new List<(SpriteRenderer, float, bool)>(); Sprite[] _brazF; float _t;
        /// <summary>v0.0.26: y-sorted decor props. order = same scale as YSort (pivot y), lifted above the hero while he overlaps one from behind.</summary>
        public struct PropRef { public SpriteRenderer sr; public int order; public float pivotY, k; public Rect rect; }
        public readonly List<PropRef> PropList = new List<PropRef>(); SpriteRenderer _baseSr; Color _lastGradeKey = new Color(-1, 0, 0, 0);
        static Material _add, _mul; static readonly Dictionary<string, Sprite> _single = new Dictionary<string, Sprite>();

        /// <summary>Room size in world units.</summary>
        public Vector2 Size => new Vector2(D.w, D.h) / HArt.PPU;
        /// <summary>Room rect in world (min = bottom-left).</summary>
        public Rect WorldRect => new Rect(0, -D.h / (float)HArt.PPU, D.w / (float)HArt.PPU, D.h / (float)HArt.PPU);
        public static Vector2 Px(float x, float y) => new Vector2(x / HArt.PPU, -y / HArt.PPU);
        /// <summary>Walkable floor for feet (world): inside the side walls, below the north wall face, above the south wall row.</summary>
        public void Walkable(float marginPx, out Vector2 lo, out Vector2 hi, float northExtraPx = 0)
        {
            lo = Px(D.wallSide + marginPx, D.h - D.wallS - marginPx); hi = Px(D.w - D.wallSide - marginPx, D.wallN + marginPx + northExtraPx);
        }

        public static HellordRoom Load(string name)
        {
            var ta = Resources.Load<TextAsset>("HellordArt/" + name + "_data");
            if (ta == null) { Debug.LogWarning("[Hellord] room data missing: " + name); return null; }
            var r = new HellordRoom { Name = name, D = JsonUtility.FromJson<RoomData>(ta.text) };
            if (name == "v26_boss") ShrinkBoss677(r.D); // t677u
            if (LowWallsOn) { r.D.wallSide = HellordTuning.I.lowWallSidePx; r.D.wallS = HellordTuning.I.lowWallBottomPx; }
            r.Root = new GameObject("Room_" + name).transform;
            var bs = FrozenSeed >= 0 ? (TileTheme == 3 ? MortarTiles52.Tiles.BuildBase(r.D, FrozenSeed) : TileTheme == 2 ? BurningTilesG.Tiles.BuildBase(r.D, FrozenSeed) : FrozenTilesB.BuildBase(r.D, FrozenSeed)) : null; r._baseNative = bs != null;
            if (!bs) bs = SingleR(name + "_base");
            if (bs) r._baseSr = MakeSR("base", r.Root, bs, Px(0, 0), BaseOrder, null);
            // t1022u: Ox Pit baked base is 1536x864 — scale the SR so the drawn floor matches ShrinkBoss677 D.w/D.h
            if (name == "v26_boss" && r._baseSr != null && !r._baseNative && BossSX677 < 0.999f)
                r._baseSr.transform.localScale = new Vector3(BossSX677, BossSY677, 1);
            r._brazF = HArt.Strip("oxpit_prop_brazier", 4, 26, 38, Vector2.zero); // t1028u: PNG shadow lip cleared (rows 34-37)
            var TT0 = HellordTuning.I;
            // t1046u: boss room regular lights — skip authored scatter braziers/pools; arena owns the grid
            bool bossRegular = name == "v26_boss" && TT0.bossArenaRegular1046 && TT0.bossArenaLights1006;
            if (!bossRegular)
            {
                foreach (var b in r.D.braziers)
                {
                    // t1027u scale + t1028u: shadow lip removed → solid bowl ends at y33; nudge top-left down 4px so feet sit where the old oval sat
                    float psy = BossSY677 < 0.999f ? BossSY677 : 1f;
                    float by = b.y + 4f * psy;
                    var sr = MakeSR("brazier", r.Root, r._brazF[b.f % 4], Px(b.x, by), PropOrder, null);
                    if (BossSX677 < 0.999f) sr.transform.localScale = new Vector3(BossSX677, BossSY677, 1);
                    r._braz.Add(sr);
                }
            }
            for (int i = 0; i < r.D.lights.Length; i++)
            {
                var L = r.D.lights[i]; var TT = HellordTuning.I;
                if (bossRegular) { /* t1046u: no baked light_brazier / warm pools — regular arena fixtures only */
                    if (L.s.Contains("light_pool_warm") || L.s.Contains("shadow_pool") || L.s.Contains("light_brazier")) continue;
                }
                if (TT.lightsSourcedV46) { /* v0.0.46: no sourceless pools; braziers smaller + weaker */
                    if (L.s.Contains("light_pool_warm") || (TT.lightsDropShadowPools && L.s.Contains("shadow_pool"))) continue;
                    if (L.s.Contains("light_brazier")) { var bl = new RoomLight { s = L.s, x = L.x, y = L.y, mul = L.mul, k = L.k * TT.brazierLightK46 }; var lsr = r.AddLight(bl, LightOrder + i); if (lsr) ScaleAroundCenter46(lsr, TT.brazierLightScale46); continue; } }
                r.AddLight(L, LightOrder + i);
            }
            if (r.D.props != null) foreach (var p in r.D.props) r.AddProp(p, bossRegular);
            return r;
        }
        static readonly Dictionary<int, Sprite> _glow46 = new Dictionary<int, Sprite>();
        /// <summary>v0.0.46: small round glow, smooth falloff quantised to 8 steps with a 4x4 Bayer dither (pixel-art, no hard bands).</summary>
        static Sprite Glow46(int R)
        {
            R = Mathf.Max(4, R); if (_glow46.TryGetValue(R, out var s) && s) return s; int N = R * 2; var c = HellordTuning.I.d2Dark606 ? HellordTuning.I.d2GlowColor606 : HellordTuning.I.sourceGlowColor46; // t606u
            int[] bayer = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 }; var px = new Color[N * N];
            for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
            {
                float dx = (x + 0.5f - R) / R, dy = (y + 0.5f - R) / (R * 0.7f), f = Mathf.Clamp01(1 - Mathf.Sqrt(dx * dx + dy * dy)); f *= f;
                px[y * N + x] = new Color(c.r, c.g, c.b, Mathf.Clamp01(Mathf.Floor(f * 8 + (bayer[(y & 3) * 4 + (x & 3)] + 0.5f) / 16f) / 8f));
            }
            var t = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; t.SetPixels(px); t.Apply();
            return _glow46[R] = Sprite.Create(t, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), HArt.PPU, 0, SpriteMeshType.FullRect);
        }
        /// <summary>v0.0.46: additive glow centred on room px (cx,cy), only for a real light source.</summary>
        public static readonly List<SpriteRenderer> Glow586 = new List<SpriteRenderer>(); // t586u: every source glow (skull torch, lava), used as V2 light sources
        public static SpriteRenderer SourceGlow46(Transform parent, float cx, float cy, int R, float k, int order)
        {
            var sp = Glow46(R); var go = new GameObject("srcglow"); go.transform.SetParent(parent, false); go.transform.position = Px(cx, cy);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sp; sr.sortingOrder = order; var m = LightMat(false); Glow586.Add(sr);
            if (m != null) { sr.sharedMaterial = m; var mpb = new MaterialPropertyBlock(); sr.GetPropertyBlock(mpb); mpb.SetTexture("_MainTex", sp.texture); mpb.SetFloat("_Strength", k); sr.SetPropertyBlock(mpb); }
            else sr.color = new Color(1, 1, 1, 0.5f * k);
            return sr;
        }
        static void ScaleAroundCenter46(SpriteRenderer sr, float k)
        {
            if (!sr || !sr.sprite || k <= 0) return; Vector3 c = sr.bounds.center; sr.transform.localScale = new Vector3(k, k, 1); sr.transform.position += c - sr.bounds.center;
        }
        public SpriteRenderer AddLight(RoomLight l, int order)
        {
            var s = Single(l.s); if (!s) return null;
            var m = LightMat(l.mul != 0); var sr = MakeSR("light_" + l.s, Root, s, Px(l.x, l.y), order, m);
            if (m != null) { var mpb = new MaterialPropertyBlock(); sr.GetPropertyBlock(mpb); mpb.SetTexture("_MainTex", s.texture); mpb.SetFloat("_Strength", l.k); sr.SetPropertyBlock(mpb); Lights.Add((sr, l.k, l.mul == 0 && l.s.IndexOf("gate") < 0)); }
            else sr.color = l.mul != 0 ? new Color(0, 0, 0, 0.55f * l.k) : new Color(1, 1, 1, 0.5f * l.k); // fallback if the shader is missing (not exact)
            return sr;
        }
        static readonly Dictionary<string, Sprite> _propSprites = new Dictionary<string, Sprite>();
        void AddProp(RoomProp p, bool bossRegular1046 = false)
        {
            // t1028u/t1030u: clear baked shadow lips in PNGs; pivot on solid bottom so debris/rocks don't hover over their own oval.
            int ox = p.ox, oy = p.oy;
            if (p.s.Contains("skullpost")) oy = 37;          // was 40
            else if (p.s.Contains("rubble_0")) oy = 6;       // t1030u rock; was 10
            else if (p.s.Contains("rubble_2")) oy = 8;       // t1030u rock cluster; was 12
            else if (p.s.Contains("bonepile")) oy = 9;       // t1030u; was 12
            else if (p.s.Contains("fallen_pillar")) oy = 16; // t1030u; was 19
            else if (p.s.Contains("prop_hay")) oy = 14;      // t1030u; was 16
            string key = p.s + "|" + ox + "|" + oy + "|ns1030" + RampKey;
            if (!_propSprites.TryGetValue(key, out var sp) || !sp)
            {
                var tex = Resources.Load<Texture2D>("HellordArt/" + p.s); if (tex == null) { Debug.LogWarning("[Hellord] missing prop " + p.s); return; }
                tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp;
                if (Ramp != null) tex = V37Recolor.Map(tex, Ramp, PropGain, false); // v0.0.37 placeholder floor recolour
                sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(ox / (float)tex.width, 1f - oy / (float)tex.height), HArt.PPU, 0, SpriteMeshType.FullRect); sp.name = key; _propSprites[key] = sp;
            }
            // t1027u: scale props with ShrinkBoss base so they match the scaled floor
            float psx = BossSX677 < 0.999f ? BossSX677 : 1f, psy = BossSY677 < 0.999f ? BossSY677 : 1f;
            int placeAx = p.ax, placeAy = p.ay; // authored ay = old shadow contact; with oy=37 wood sits there
            int order = placeAy - 432;
            var sr = MakeSR("prop_" + p.s, Root, sp, Px(placeAx, placeAy), order, null); sr.spriteSortPoint = SpriteSortPoint.Pivot;
            if (psx < 0.999f || psy < 0.999f) sr.transform.localScale = new Vector3(psx, psy, 1);
            { var TT = HellordTuning.I; // t1046u: boss regular mode keeps skullpost sprites but no scatter SourceGlow
              if (!bossRegular1046 && TT.lightsSourcedV46 && TT.torchGlow46 && p.s.Contains("skullpost"))
                {
                    float fcx = placeAx + (11 - ox) * psx, fcy = placeAy + (10 - oy) * psy;
                    SourceGlow46(Root, fcx, fcy, TT.d2Dark606 ? TT.d2TorchRadiusPx606 : TT.torchGlowRadiusPx46, TT.d2Dark606 ? TT.d2TorchK606 : TT.torchGlowK46, LightOrder + 300);
                }
            } /* v0.0.46 / t1027u / t1028u / t1046u */
            float w = sp.rect.width / HArt.PPU * psx, h = sp.rect.height / HArt.PPU * psy;
            var rect = new Rect((placeAx - ox * psx) / (float)HArt.PPU, -(placeAy - oy * psy + sp.rect.height * psy) / HArt.PPU, w, h);
            PropList.Add(new PropRef { sr = sr, order = order, pivotY = -placeAy / (float)HArt.PPU, k = p.k, rect = rect });
        }
        /// <summary>v0.0.26: props are drawn above the grade quad, so they get the floor palette x the (normalised) atmosphere grade x their baked light factor as a sprite colour; the base gets the floor palette.</summary>
        public void ApplyGrade(Color floorTint, Color gradeLinear, bool atmosOn)
        {
            var key = new Color(floorTint.r + gradeLinear.r * 7, floorTint.g + gradeLinear.g * 7, floorTint.b + gradeLinear.b * 7, atmosOn ? 1 : 0);
            if (key == _lastGradeKey) return; _lastGradeKey = key;
            if (_baseSr) _baseSr.color = _baseNative ? (TileTheme == 3 ? HellordTuning.I.mortarTint52 : TileTheme == 2 ? HellordTuning.I.burningTilesGTint : HellordTuning.I.frozenTilesBTint) : floorTint;
            float mx = Mathf.Max(0.01f, gradeLinear.r, gradeLinear.g, gradeLinear.b); Color gn = atmosOn ? new Color(Mathf.Pow(gradeLinear.r / mx, 1 / 2.2f), Mathf.Pow(gradeLinear.g / mx, 1 / 2.2f), Mathf.Pow(gradeLinear.b / mx, 1 / 2.2f), 1) : Color.white;
            foreach (var p in PropList) { if (!p.sr) continue; var c = floorTint * gn * p.k; c.a = 1; p.sr.color = c; }
        }
        /// <summary>v0.0.26: the hero stays above actors (sort 32000) but walks BEHIND a prop that is lower on screen than his feet and overlaps him.</summary>
        public void SortAgainstHero(Vector2 feet, int heroOrder, bool on)
        {
            Rect hr = new Rect(feet.x - 0.7f, feet.y, 1.4f, 2.8f);
            for (int i = 0; i < PropList.Count; i++)
            {
                var p = PropList[i]; if (!p.sr) continue;
                int o = on && p.pivotY < feet.y - 0.02f && p.rect.Overlaps(hr) ? heroOrder + 12 : p.order;
                if (p.sr.sortingOrder != o) p.sr.sortingOrder = o;
            }
        }
        public void Tick(float dt)
        {
            _t += dt; int f = (int)(_t * 8);
            for (int i = 0; i < _braz.Count; i++) _braz[i].sprite = _brazF[(f + D.braziers[i].f) % 4];
        }
        public void Destroy() { if (Root) Object.Destroy(Root.gameObject); }

        static SpriteRenderer MakeSR(string n, Transform p, Sprite s, Vector2 pos, int order, Material m)
        {
            var go = new GameObject(n); go.transform.SetParent(p, false); go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = s; sr.sortingOrder = order; if (m != null) sr.sharedMaterial = m; return sr;
        }
        /// <summary>Whole texture as one sprite, pivot top-left.</summary>
        // v0.0.37: floor recolour (set by HellordGame.LoadRoom before Load). Lights are never recoloured.
        /// <summary>v0.0.44: seed for the Frozen floor B composed base (-1 = use the baked _base).</summary>
        /// <summary>t677u/t1022u: boss room scale actually applied (x, y); 1 = original 1536x864. Applies to tiled floors AND baked Ox Pit (base SR scaled to match).</summary>
        public static float BossSX677 = 1, BossSY677 = 1;
        static void ShrinkBoss677(RoomData D)
        {
            float k = HellordTuning.I.bossArenaClamp1006 ? HellordTuning.I.bossRoomScale1006 : HellordTuning.I.bossRoomScale677; BossSX677 = BossSY677 = 1; if (k >= 0.999f) return; // t1022u: also scale Ox Pit (was FrozenSeed<0 skip → clamp-only fake shrink)
            int w = Mathf.Max(640, Mathf.RoundToInt(D.w * k / 32f) * 32), h = Mathf.Max(384, Mathf.RoundToInt(D.h * k / 32f) * 32);
            float sx = w / (float)D.w, sy = h / (float)D.h; BossSX677 = sx; BossSY677 = sy;
            int X(int v) => Mathf.RoundToInt(v * sx); int Y(int v) => Mathf.RoundToInt(v * sy);
            if (D.braziers != null) foreach (var b in D.braziers) { b.x = X(b.x); b.y = Y(b.y); }
            if (D.lights != null) foreach (var l in D.lights) { l.x = X(l.x); l.y = Y(l.y); }
            if (D.props != null) foreach (var p in D.props) { int dx = p.ax - p.x, dy = p.ay - p.y; p.ax = X(p.ax); p.ay = Y(p.ay); p.x = p.ax - dx; p.y = p.ay - dy; }
            if (D.gates != null) foreach (var g in D.gates) g.x = X(g.x);
            if (D.chests != null) foreach (var c in D.chests) { c.x = X(c.x); c.y = Y(c.y); }
            if (D.doorX > 0) D.doorX = X(D.doorX); if (D.doorY > 0) D.doorY = Y(D.doorY); if (D.spawnX > 0) D.spawnX = X(D.spawnX); if (D.spawnY > 0) D.spawnY = Y(D.spawnY);
            Debug.Log($"[Hellord] t677u boss room {D.w}x{D.h} -> {w}x{h}"); D.w = w; D.h = h;
        }
        public static int FrozenSeed = -1; bool _baseNative;
        /// <summary>v0.0.44: which composed tileset FrozenSeed builds (1 Frozen B, 2 Burning G).</summary>
        public static int TileTheme = 1;
        /// <summary>v0.0.44: set by LoadRoom; low walls widen the side walls and the bottom wall (floor x 32..w-33, y 48..h-25).</summary>
        public static bool LowWallsOn;
        public static Color32[] Ramp; public static string RampKey = ""; public static float RampGain = 1.5f, PropGain = 1.15f;
        public static Sprite SingleR(string name)
        {
            if (Ramp == null) return Single(name);
            string key = name + RampKey; if (_single.TryGetValue(key, out var c) && c) return c;
            var s0 = Single(name); if (!s0) return null;
            var t = V37Recolor.Map(s0.texture, Ramp, RampGain, false);
            var s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0, 1), HArt.PPU, 0, SpriteMeshType.FullRect); s.name = key; return _single[key] = s;
        }
        public static Sprite Single(string name)
        {
            if (_single.TryGetValue(name, out var s) && s) return s;
            var t = Resources.Load<Texture2D>("HellordArt/" + name); if (t == null) { Debug.LogWarning("[Hellord] missing " + name); return null; }
            t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp;
            s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0, 1), HArt.PPU, 0, SpriteMeshType.FullRect); s.name = name;
            return _single[name] = s;
        }
        public static Material LightMatPublic(bool mul) => LightMat(mul);
        static Material LightMat(bool mul)
        {
            if (mul ? _mul : _add) return mul ? _mul : _add;
            var sh = Resources.Load<Shader>("HellordShaders/HellordLight"); if (sh == null) sh = Shader.Find("Hellord/LightOverlay");
            if (sh == null || !sh.isSupported) return null;
            var m = new Material(sh) { name = mul ? "HellordLightMul" : "HellordLightAdd" };
            m.SetFloat("_Mode", mul ? 1 : 0);
            m.SetFloat("_SrcBlend", mul ? (float)UnityEngine.Rendering.BlendMode.Zero : (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_DstBlend", mul ? (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha : (float)UnityEngine.Rendering.BlendMode.One);
            if (mul) _mul = m; else _add = m; return m;
        }
    }
}
