using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.52 torture floors (VH confirmed). Prop art + animation only; demons never operate devices.
//  * Mortar B floor: a SKIN on Ox-logic depths (Theme stays 0 so every Theme-indexed array keeps working):
//    packed bran tiles (Resources/HellordArt/MortarHell/mortar_tileset.json), mortar low walls (lowwalls_mortar.json),
//    rice-mill props (pits + pestle + rope beam, rice patches, sacks, husk piles, bowls, chopsticks).
//  * Set pieces (Resources/HellordArt/Torture, 4x4 grids of 16 frames, v131 transparent renders, devices + humans only):
//    Ox spiked grind-wheel trough, Burning G hellfire cauldrons, Mortar B rice-mill (bowl under pestle, rope haulers).
//  * Frozen: humans in glowing ice blocks; a player fireball (proj or blast) cracks a block, the 2nd hit shatters it.
namespace Hellord
{
    public partial class HellordGame
    {
        // ---------- Mortar skin ----------
        /// <summary>v0.0.52: this depth is an Ox-logic floor drawn with the Mortar B skin (never floor 1).</summary>
        public bool Mortar52
        {
            get
            {
                var T = HellordTuning.I; return floorMode && T.mortarFloor52 && MortarTiles52.Available && ThemeSlot53 == 3; // t495u: Mortar = its own theme slot (was a random skin roll on Ox depths)
            }
        }

        // ---------- set pieces ----------
        class Torture52 { public SpriteRenderer sr, sh; public Sprite[] f; public float t; }
        class IceBlock52 { public SpriteRenderer sr, glow, sh; public Vector2 c; public Rect col; public int hits; public float t, regrowT; public bool broken; } // pt484u: col = its _blocks42 footprint
        readonly List<Torture52> _tort52 = new List<Torture52>(); readonly List<IceBlock52> _iceB52 = new List<IceBlock52>();
        static readonly Dictionary<string, Sprite[]> _tortF52 = new Dictionary<string, Sprite[]>();
        static Sprite _shadow52, _glow52;
        const string TortDir52 = "HellordArt/Torture/";

        static Sprite[] Grid52(string file, int w, int h, int cols, int n, int row0, Vector2 piv)
        {
            string k = file + "|" + row0 + "|" + n; if (_tortF52.TryGetValue(k, out var f)) return f;
            var t = Resources.Load<Texture2D>(TortDir52 + file); f = null;
            if (t != null)
            {
                t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; var l = new List<Sprite>();
                for (int i = 0; i < n; i++)
                {
                    int cx = (i % cols) * w, cy = (row0 + i / cols) * h; if (cx + w > t.width || cy + h > t.height) break;
                    var s = Sprite.Create(t, new Rect(cx, t.height - cy - h, w, h), piv, HArt.PPU, 0, SpriteMeshType.FullRect); s.name = file + "_" + i; l.Add(s);
                }
                f = l.Count > 0 ? l.ToArray() : null;
            }
            _tortF52[k] = f; return f;
        }
        static Sprite Soft52(ref Sprite cache, int w, int h, Color c, float pow)
        {
            if (cache) return cache; var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f) / w * 2 - 1, dy = (y + 0.5f) / h * 2 - 1, d = Mathf.Sqrt(dx * dx + dy * dy);
                px[y * w + x] = new Color(c.r, c.g, c.b, d >= 1 ? 0 : c.a * Mathf.Pow(1 - d, pow));
            }
            t.SetPixels(px); t.Apply(false, true); cache = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), HArt.PPU); return cache;
        }
        SpriteRenderer SR52(string n, Sprite s, Vector2 p, int order)
        {
            var go = new GameObject(n); go.transform.SetParent(_room.Root, false); go.transform.position = p;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = s; sr.sortingOrder = order; return sr;
        }
        /// <summary>Tight contact shadow under a prop: dark ellipse wPx wide.</summary>
        SpriteRenderer Shadow52(Vector2 p, float wPx, int order)
        {
            var s = Soft52(ref _shadow52, 32, 12, new Color(0, 0, 0, 1), 0.6f); var sr = SR52("shadow52", s, p, order - 1);
            sr.color = new Color(1, 1, 1, HellordTuning.I.tortureShadowA52); sr.transform.localScale = new Vector3(wPx / 32f, Mathf.Max(0.5f, wPx / 32f * 0.35f), 1); return sr;
        }
        int Ord52(Vector2 p) => HellordRoom.PropOrder + HellordTuning.I.tortureSort52 + Mathf.Clamp(Mathf.RoundToInt(-p.y * 4f), -400, 400);

        void ClearTorture52()
        {
            SaveBrk53(); // pt483u: remember broken / damaged decor of the room we leave
            foreach (var t in _tort52) { if (t.sr) Object.Destroy(t.sr.gameObject); if (t.sh) Object.Destroy(t.sh.gameObject); }
            foreach (var b in _iceB52) { if (b.sr) Object.Destroy(b.sr.gameObject); if (b.glow) Object.Destroy(b.glow.gameObject); if (b.sh) Object.Destroy(b.sh.gameObject); }
            _tort52.Clear(); _iceB52.Clear(); ClearTorture53();
        }

        /// <summary>v0.0.52: called after BuildHazards37 on room entry.</summary>
        void BuildTorture52(FloorRoom r)
        {
            ClearTorture52(); var T = HellordTuning.I;
            if (!T.decorEnabled582) { ClearBrk53(); return; } // t582u: ALL torture decor / set pieces / watchers / rope monk / breakable props off (kill-switch)
            if (!floorMode || r == null || _room == null || r.type == RoomType.Boss) return;
            bool skin = Mortar52; int th = Theme; bool mortar = skin || (th == 0 && T.oxTortureDecor53); // t495u: Mortar floors = Mortar set piece + props; Ox Pit = torture_ox + props on the old Ox look
            _room.Walkable(48f, out var lo, out var hi); if (hi.x < lo.x) { var t = lo; lo = hi; hi = t; } if (hi.y < lo.y) { float t = lo.y; lo.y = hi.y; hi.y = t; }
            var D = _room.D;
            var avoid = new List<Vector2> { HellordRoom.Px(D.w / 2f, D.h / 2f + 16f), HellordRoom.Px(D.spawnX > 0 ? D.spawnX : D.w / 2f, D.spawnY > 0 ? D.spawnY : D.h / 2f + 40f) };
            for (int d = 0; d < 4; d++) if (r.nb[d] >= 0 || (d == 0 && r.secretLink)) { var a = ArrivePx(d); var p = DoorPivotPx(d); avoid.Add(HellordRoom.Px(a.x, a.y)); avoid.Add(HellordRoom.Px(p.x, p.y)); }
            foreach (var o in _ice) avoid.Add(o.c);
            var rng = new System.Random(curRoom * 6151 + _floorIdx * 92821 + floor.Count * 31 + 5);
            float F() => (float)rng.NextDouble();
            // footprint test: pivot bottom-centre, w x h world units must fit the walkable box and stay clear of doors/spawn/hero
            bool Fits(Vector2 p, float w, float h, float pad)
            {
                if (p.x - w / 2 < lo.x || p.x + w / 2 > hi.x || p.y < lo.y || p.y + h > hi.y) return false;
                var ctr = p + Vector2.up * h * 0.4f; float rr = Mathf.Max(w, h) * 0.5f + pad;
                foreach (var a in avoid) if ((a - ctr).magnitude < rr) return false; return true;
            }
            bool Spot(float w, float h, float pad, out Vector2 at)
            {
                for (int k = 0; k < 120; k++) { var p = new Vector2(Mathf.Lerp(lo.x, hi.x, F()), Mathf.Lerp(lo.y, hi.y, F())); if (Fits(p, w, h, pad)) { at = p; return true; } }
                at = default; return false;
            }
            void Reserve(Vector2 p, float w, float h) { for (int i = -1; i <= 1; i++) for (int j = 0; j <= 2; j++) avoid.Add(p + new Vector2(i * w * 0.4f, j * h * 0.4f)); }

            if (T.torture52 && F() < T.tortureRoomChance52)
            {
                string file = null; int w = 0, h = 0;
                if (skin) { file = "torture_b"; w = 458; h = 222; }
                else if (th == 0) { file = "torture_ox"; w = 433; h = 156; }
                else if (th == 2 && T.burningTilesG && BurningTilesG.Tiles.Available) { file = "torture_burning"; w = 242; h = 201; }
                if (file != null)
                {
                    var f = Grid52(file, w, h, 4, 16, 0, new Vector2(0.5f, 0.04f)); float wu = w / (float)HArt.PPU, hu = h / (float)HArt.PPU;
                    if (f != null && Spot(wu, hu * 0.6f, T.tortureSetPad52, out var at))
                    {
                        var o = Ord52(at); var tp = new Torture52 { f = f, sr = SR52(file, f[0], at, o), sh = Shadow52(at + Vector2.up * 0.25f, w * 0.8f, o) };
                        tp.t = F() * 2f; _tort52.Add(tp); Reserve(at, wu, hu * 0.6f);
                        // v0.0.53 torture gaps: solid footprint + watching demons (+ the rope-hauling Grinder Monk on B)
                        var dc = Collide53(at, w * T.tortureColW53, h * T.tortureColH53); Watchers53(at, w * 0.8f, h, file, F);
                        var bowl = Collide53(at, w * T.tortureBowlW53, h * T.tortureBowlH53, h * T.tortureColH53); // v0.0.53 B17: blocks standing inside the bowl
                        var db = AddBrk53(tp.sr, BrkRect53(at, wu * T.brkDeviceHitW53, hu * T.brkDeviceHitH53), T.brkDeviceHp53, file == "torture_burning" ? Brk53Fire : Brk53Wood, dc, true, tp.sh); // v0.0.53 breakable device (humans baked in -> gore on break)
                        if (db != null) db.col2 = bowl;
                        int nl = _watchLines53.Count;
                        if (file == "torture_b") RopeMonk53(at, o);
                        if (db != null) for (int li = nl; li < _watchLines53.Count; li++) db.parts.Add(_watchLines53[li]); // v0.0.53 B16: rope goes with the device
                    }
                }
            }
            if (th == 1 && T.iceBlocks52 > 0 && (r.IsFight || T.iceBlocksAnyRoom52))
            {
                var idle = Grid52("torture_iceblock", 83, 72, 16, 16, 0, new Vector2(39f / 83f, 1f - 52f / 72f));
                // v0.0.53 torture gaps: the v131 Frozen GROUP scene (torture_frozen 381x219) - 5 blocks in its exact layout, each one still shatterable
                bool grouped = false;
                Vector2 g0 = default;
                if (idle != null && T.iceGroup53 && F() < T.iceGroupChance53 && Spot(381f / HArt.PPU, 219f / HArt.PPU * 0.7f, T.tortureSetPad52, out g0))
                {
                    grouped = true; Reserve(g0, 381f / HArt.PPU, 219f / HArt.PPU * 0.7f);
                    float[,] lay = { { 70, 94 }, { 130, 145 }, { 190, 78 }, { 250, 145 }, { 310, 94 } }; // block feet, px from the scene's top-left
                    for (int i = 0; i < 5; i++)
                    {
                        var at = g0 + new Vector2((lay[i, 0] - 190.5f) / HArt.PPU, (219f - lay[i, 1]) / HArt.PPU - 2f);
                        var o = Ord52(at); var b = new IceBlock52 { c = at, t = F() * 1.4f };
                        b.glow = SR52("ice52_glow", Soft52(ref _glow52, 48, 48, new Color(0.45f, 0.9f, 1f, 1f), 1.6f), at + Vector2.up * 1.6f, o - 2);
                        b.glow.transform.localScale = Vector3.one * T.iceGlowScale52;
                        b.sh = Shadow52(at + Vector2.up * 0.1f, 60, o); b.sr = SR52("ice52", idle[0], at, o);
                        _iceB52.Add(b); Collide53(at, 36 * T.tortureColW53 / 0.8f * 0.8f, 10);
                    }
                    Watchers53(g0 + Vector2.up * 1.5f, 381f * 0.8f, 219f, "torture_frozen", F);
                }
                if (idle != null && !grouped)
                    for (int i = 0; i < T.iceBlocks52; i++)
                    {
                        if (!Spot(83f / HArt.PPU, 72f / HArt.PPU * 0.5f, 0.6f, out var at)) break;
                        var o = Ord52(at); var b = new IceBlock52 { c = at, t = F() * 1.4f };
                        b.glow = SR52("ice52_glow", Soft52(ref _glow52, 48, 48, new Color(0.45f, 0.9f, 1f, 1f), 1.6f), at + Vector2.up * 1.6f, o - 2);
                        b.glow.transform.localScale = Vector3.one * T.iceGlowScale52;
                        b.sh = Shadow52(at + Vector2.up * 0.1f, 60, o); b.sr = SR52("ice52", idle[0], at, o);
                        _iceB52.Add(b); Reserve(at, 83f / HArt.PPU, 72f / HArt.PPU * 0.5f); b.col = Collide53(at, 36, 10); // v0.0.53 solid (pt484u: kept so the shatter can drop it)
                    }
            }
            if (mortar && T.mortarProps52) BuildMortarProps52(r, F, Spot, Reserve);
            if (mortar && (r.IsFight || T.soulsAnyRoom53)) Souls53(F, Spot); // v0.0.53 shootable damned souls
            RestoreBrk53(r); // pt483u: same layout every visit (seeded, hero pos no longer in it) -> re-apply broken / damaged state
        }

        void BuildMortarProps52(FloorRoom r, System.Func<float> F, SpotFn52 spot, System.Action<Vector2, float, float> reserve)
        {
            var T = HellordTuning.I;
            // pit + pestle on its rope beam (animated), only in fight rooms with room to spare
            if (r.IsFight && F() < T.mortarPitChance52)
            {
                var pit = Grid52("m_mortar_pit_48x48_2f", 48, 48, 2, 2, 0, new Vector2(0.5f, 0.1f)); var pes = Grid52("m_mortar_pestle_48x96_6f", 48, 96, 6, 6, 0, new Vector2(0.5f, 0.05f));
                var beam = Grid52("m_rope_beam_96x16_1f", 96, 16, 1, 1, 0, new Vector2(0.5f, 0.5f));
                if (pit != null && pes != null && spot(6f, 6f, 0.5f, out var at))
                {
                    int o = Ord52(at);
                    var tpit = new Torture52 { f = pit, sr = SR52("m_pit", pit[0], at, o - 3), sh = Shadow52(at + Vector2.up * 0.3f, 50, o - 3) }; _tort52.Add(tpit);
                    var tpes = new Torture52 { f = pes, sr = SR52("m_pestle", pes[0], at + Vector2.up * 0.4f, o), t = F() }; _tort52.Add(tpes);
                    SpriteRenderer bsr = null; if (beam != null) { bsr = SR52("m_beam", beam[0], at + Vector2.up * 6.2f, o + 1); bsr.transform.SetParent(_room.Root, true); }
                    reserve(at, 3f, 6f); var pc = Collide53(at, 40, 16); // v0.0.53 solid pit
                    AddBrk53(tpit.sr, BrkRect53(at, 3f, 4.5f), T.brkPitHp53, Brk53Wood, pc, false, tpit.sh, tpes.sr, bsr); // v0.0.53 breakable pit + pestle + beam
                }
            }
            string[] deco = { "m_deco_sack_20x22_1f", "m_deco_sack_spilled_20x22_1f", "m_deco_husk_pile_28x16_1f", "m_deco_broken_bowl_24x16_1f", "m_deco_chopsticks_20x12_1f" };
            int[,] dsz = { { 20, 22 }, { 20, 22 }, { 28, 16 }, { 24, 16 }, { 20, 12 } };
            int nd = T.mortarDeco52 + (int)(F() * 3);
            for (int i = 0; i < nd; i++)
            {
                int k = (int)(F() * deco.Length) % deco.Length; int w = dsz[k, 0], h = dsz[k, 1];
                var f = Grid52(deco[k], w, h, 1, 1, 0, new Vector2(0.5f, 0.1f)); if (f == null || !spot(w / 16f, h / 16f, 0.3f, out var at)) continue;
                int o = Ord52(at); var sr = SR52(deco[k], f[0], at, o); sr.flipX = F() < 0.5f; var td = new Torture52 { f = f, sr = sr, sh = Shadow52(at + Vector2.up * 0.1f, w * 0.9f, o) }; _tort52.Add(td); reserve(at, w / 16f, h / 16f);
                AddBrk53(sr, BrkRect53(at, Mathf.Max(1f, w / 16f), Mathf.Max(0.9f, h / 16f), 0.2f), T.brkDecoHp53, k <= 2 ? Brk53Grain : Brk53Wood, default, false, td.sh); // v0.0.53 breakable decor
            }
            string[] rice = { "m_rice_patch_S_16x12_4f", "m_rice_patch_M_24x16_4f", "m_rice_patch_L_40x24_4f" }; int[,] rsz = { { 16, 12 }, { 24, 16 }, { 40, 24 } };
            for (int i = 0; i < T.mortarRice52; i++)
            {
                int k = (int)(F() * 3) % 3; int w = rsz[k, 0], h = rsz[k, 1];
                var f = Grid52(rice[k], w, h, 4, 4, 0, new Vector2(0.5f, 0.5f)); if (f == null || !spot(w / 16f, h / 16f, 0.2f, out var at)) continue;
                var sr = SR52(rice[k], f[0], at, HellordRoom.BaseOrder + 90); _tort52.Add(new Torture52 { f = f, sr = sr, t = F() * 2f });
                AddBrk53(sr, BrkRect53(at, w / 16f, Mathf.Max(0.8f, h / 16f), h / 32f), T.brkRiceHp53, Brk53Grain, default); // v0.0.53 rice patch scatters
            }
        }
        delegate bool SpotFn52(float w, float h, float pad, out Vector2 at);

        void UpdateTorture52(float dt)
        {
            UpdateTorture53(dt); if (_tort52.Count == 0 && _iceB52.Count == 0) return; var T = HellordTuning.I; float fps = Mathf.Max(1f, T.tortureFps52);
            foreach (var t in _tort52) { if (t.f == null || t.f.Length < 2 || !t.sr) continue; t.t += dt; t.sr.sprite = t.f[(int)(t.t * fps) % t.f.Length]; }
            var idle = Grid52("torture_iceblock", 83, 72, 16, 16, 0, new Vector2(39f / 83f, 1f - 52f / 72f));
            var crk = Grid52("torture_iceblock", 83, 72, 16, 16, 1, new Vector2(39f / 83f, 1f - 52f / 72f));
            var shat = Grid52("torture_iceblock", 83, 72, 16, 5, 2, new Vector2(39f / 83f, 1f - 52f / 72f));
            foreach (var b in _iceB52)
            {
                if (!b.sr) continue; b.t += dt;
                if (b.broken)
                {
                    int k = (int)(b.t * T.iceShatterFps52);
                    if (shat != null && k < shat.Length) b.sr.sprite = shat[k]; else if (b.sr.enabled) { b.sr.enabled = false; if (b.sh) b.sh.enabled = false; }
                    if (b.glow) b.glow.color = new Color(1, 1, 1, Mathf.Max(0, 0.6f - b.t * 2f));
                    if (T.iceRegrow52 > 0 && (b.regrowT += dt) >= T.iceRegrow52) { b.broken = false; b.hits = 0; b.t = 0; b.regrowT = 0; b.sr.enabled = true; if (b.sh) b.sh.enabled = true; if (b.col.width > 0 && !_blocks42.Exists(k => k.col == b.col)) _blocks42.Add(new Block42 { vis = b.col, col = b.col }); } // pt484u: solid again only when it regrows
                    continue;
                }
                var set = b.hits > 0 ? crk : idle; if (set != null) b.sr.sprite = set[(int)(b.t * fps) % set.Length];
                if (b.glow) b.glow.color = new Color(1, 1, 1, T.iceGlowA52 * (0.75f + 0.25f * Mathf.Sin(b.t * 3.1f)));
            }
        }

        bool IceHit52(Vector2 p, float extra)
        {
            var T = HellordTuning.I; bool any = false;
            foreach (var b in _iceB52)
            {
                if (b.broken || !b.sr) continue; if ((b.c + Vector2.up * T.iceHitY52 - p).magnitude > T.iceHitR52 + extra) continue;
                b.hits++; any = true;
                if (b.hits >= Mathf.Max(1, T.iceHitsToBreak52)) { b.broken = true; b.t = 0; b.regrowT = 0; DropCol53(b.col); HellordAudio.Play("fireball_impact"); BrkDrop53(b.c + Vector2.up * 0.3f, false); if (T.breakables53) BreakFX53(b.c + Vector2.up * T.iceHitY52, Brk53Ice, T.brkIceMul53, 2.5f); } // v0.0.53 shatter burst
                if (T.iceOneBlockPerHit52) break;
            }
            return any;
        }
        /// <summary>v0.0.52: projectile vs ice blocks (fireballs only unless iceAnyProj52). True = projectile consumed.</summary>
        bool TortureProjHit52(Proj p)
        {
            if (p == null || p.s == null || p.s.def == null) return false;
            if (_souls53.Count > 0 && SoulHit53(p.pos, 0f, 12f)) { ProjectileFX.Impact(p.pos, ProjElement.Fire, p.vel); return true; } // v0.0.53: any projectile hits souls
            if (BrkHit53(p.pos, 0f, p.dmg, false)) { ProjectileFX.Impact(p.pos, ProjElement.Fire, p.vel); return true; } // v0.0.53 breakables: any projectile
            if (_iceB52.Count == 0) return false;
            if (!HellordTuning.I.iceAnyProj52 && p.s.def.id != SkillId.Fireball) return false;
            if (!IceHit52(p.pos, 0f)) return false; ProjectileFX.Impact(p.pos, ProjElement.Fire, p.vel); return true;
        }
        /// <summary>v0.0.52: fireball explosions (blast kind 0) crack ice blocks in radius.</summary>
        void TortureBlast52(Vector2 pos, float r, int kind) { BrkHit53(pos, r, HellordTuning.I.brkBlastDmg53, true); if (_souls53.Count > 0) SoulHit53(pos, r, 12f); if (_iceB52.Count > 0 && (kind == 0 || HellordTuning.I.iceAnyProj52)) IceHit52(pos, r); }
    }

    /// <summary>v0.0.52: Mortar B packed-bran tiles (Art/previews/step34_pounding_mortar/tiles/mortarB).</summary>
    public static class MortarTiles52
    {
        public static readonly RoomTiles Tiles = new RoomTiles("HellordArt/MortarHell/", "mortar_tileset", 0, true);
        public static bool Available => Tiles.Available;
    }
}
