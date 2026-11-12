using System.Collections.Generic;
using UnityEngine;

// v0.0.42: RANDOM FLOORS (VH: "the hell level should random, not linear").
//  1. Floor THEME order is shuffled per run (Ox Pit / Frozen / Burning). Theme = boss, hazards, demon variants, palette, music pitch.
//     Difficulty keeps using DEPTH (FloorIdx). Theme is read through 'Theme' everywhere the old code meant "which hell".
//  2. Each floor is a random PATH of rooms built from a weighted pool (HellordTuning "Random floors").
//  3. A cleared room offers 2-3 exits; each shows the type of the room behind it. Walking into one picks it. Boss = final node.
//  4. Fight rooms get a random shape: plain rectangle, L (one corner pit), plus (four corner pits) or pillar pits inside.
//     Pits are solid for feet (hero + demons), projectiles fly over them. Placeholder look: flat dark fill + existing fence / skull post props.
// One run seed drives all of it (HellordTuning.runSeed, -1 = random; always logged and shown in the F1 FLOOR tab).
namespace Hellord
{
    public partial class HellordGame
    {
        // ================= run seed + theme order =================
        int _runSeed42 = -1; int[] _themeOrder42;
        const int ThemeSlots42 = 12;
        public int RunSeed42 => _runSeed42;
        /// <summary>v0.0.42: which hell this floor is (0 Ox Pit, 1 Frozen, 2 Burning). Depth is FloorIdx.</summary>
        /// t495u: the run order holds 4 floor themes (0 Ox Pit, 1 Frozen, 2 Burning, 3 Mortar). Mortar plays on Ox rules (hazards, palette, Ox enemy pool),
        /// so 'Theme' (the gameplay hell) reads Mortar as 0; ThemeSlot53 / Mortar52 tell the look, name, music, boss and Mortar enemies apart.
        public int Theme => floorMode ? ThemeOf53(ThemeAt(_floorIdx)) : 0;
        public int ThemeSlot53 => floorMode ? ThemeAt(_floorIdx) : 0;
        static int ThemeOf53(int slot) => slot == 3 ? 0 : slot;
        int ThemeAt(int depth) { if (_themeOrder42 == null) PlanRun42(); return _themeOrder42[Mathf.Abs(depth) % ThemeSlots42]; }
        string FloorLabel53(int slot, bool longName) => slot == 3 ? (longName ? "MORTAR HELL" : "MORTAR") : longName ? new[] { "OX PIT", "FROZEN HELL", "BURNING HELL" }[Mathf.Clamp(slot, 0, 2)] : ThemeNames42[Mathf.Clamp(slot, 0, 2)];
        static readonly string[] ThemeNames42 = { "OX PIT", "FROZEN", "BURNING" };

        /// <summary>v0.0.42: new run: pick the run seed and the theme of every depth.</summary>
        void PlanRun42()
        {
            var T = HellordTuning.I; int s = T.runSeed;
            if (s < 0) { int ms = (int)ArgFPublic("-mapseed", -1); s = ms >= 0 ? ms : (System.Environment.TickCount & 0x7fffffff) % 1000000; }
            _runSeed42 = s; var rng = new System.Random(s);
            _themeOrder42 = new int[ThemeSlots42];
            int nt = T.mortarFloor52 && MortarTiles52.Available ? 4 : 3; // t495u: Mortar is a 4th theme (art missing / toggle off = 3)
            for (int b = 0; b < ThemeSlots42 / nt; b++)
            {
                int[] blk = nt == 4 ? new[] { 0, 1, 2, 3 } : new[] { 0, 1, 2 };
                if (T.randomFloorOrder)
                {
                    for (int i = nt - 1; i > 0; i--) { int j = rng.Next(i + 1); int t = blk[i]; blk[i] = blk[j]; blk[j] = t; }
                    if (b > 0 && blk[0] == _themeOrder42[b * nt - 1]) { int t = blk[0]; blk[0] = blk[1]; blk[1] = t; } // no theme twice in a row
                }
                for (int i = 0; i < nt; i++) _themeOrder42[b * nt + i] = blk[i];
            }
            Debug.Log("[Hellord] v0.0.42 run seed " + s + " -> themes " + ThemeOrderText42());
        }
        string ThemeOrderText42()
        {
            if (_themeOrder42 == null) return "?"; var sb = new System.Text.StringBuilder();
            for (int d = 0; d < FloorCount; d++) { if (d > 0) sb.Append(" > "); sb.Append(FloorLabel53(_themeOrder42[d % ThemeSlots42], false)); }
            return sb.ToString();
        }
        int FloorSeed42(int depth) { if (_themeOrder42 == null) PlanRun42(); unchecked { return _runSeed42 * 7919 + depth * 104729 + 17; } }
        /// <summary>v0.0.42 test menu: the depth that has this theme (inside floorCount). If none, the current depth is re-themed.</summary>
        int DepthForTheme42(int theme)
        {
            if (_themeOrder42 == null) PlanRun42();
            for (int d = 0; d < FloorCount; d++) if (_themeOrder42[d % ThemeSlots42] == theme) return d;
            int cur = Mathf.Clamp(FloorIdx, 0, FloorCount - 1); _themeOrder42[cur % ThemeSlots42] = theme; return cur;
        }

        // ================= random room path =================
        bool _pathMode42;
        readonly List<List<int>> _steps42 = new List<List<int>>();
        readonly HashSet<int> _shopSteps42 = new HashSet<int>();

        /// <summary>v0.0.42: start -> N steps of 2-3 candidate rooms -> boss. Rooms are made up front (seeded), doors are assigned on entry.</summary>
        void BuildPathFloor42()
        {
            var T = HellordTuning.I; var st = Random.state; Random.InitState(FloorSeed42(_floorIdx));
            _pathMode42 = true; layoutIndex = -1; _steps42.Clear(); _shopSteps42.Clear();
            int lo = Mathf.Max(1, T.pathRoomsMin), hi = Mathf.Max(lo, T.pathRoomsMax);
            int mid = Random.Range(lo, hi + 1);
            int cMin = Mathf.Clamp(T.doorChoicesMin, 1, 3), cMax = Mathf.Clamp(Mathf.Max(cMin, T.doorChoicesMax), 1, 3);
            // forced shop offers: the last step before the boss + extra mid-floor steps (never next to another shop step)
            if (T.shopBeforeBoss) _shopSteps42.Add(mid);
            for (int k = 0; k < T.extraShopOffers; k++)
                for (int tries = 0; tries < 12; tries++)
                {
                    int s = Random.Range(2, mid); if (s < 2 || s >= mid) break;
                    if (_shopSteps42.Contains(s) || _shopSteps42.Contains(s - 1) || _shopSteps42.Contains(s + 1)) continue;
                    _shopSteps42.Add(s); break;
                }
            var counts = new Dictionary<RoomType, int>();
            AddPathRoom42(RoomType.Start, 0, 2);
            var prev = new List<RoomType> { RoomType.Start };
            for (int s = 1; s <= mid; s++)
            {
                int n = Random.Range(cMin, cMax + 1); var types = new List<RoomType>(3);
                if (_shopSteps42.Contains(s) && Count42(counts, RoomType.Shrine) < Mathf.Max(1, T.maxShopPerFloor)) types.Add(RoomType.Shrine);
                while (types.Count < n) types.Add(PickPathType42(s, mid, prev, types, counts));
                Shuffle(types);
                int[] rows = n == 1 ? new[] { 2 } : n == 2 ? new[] { 1, 3 } : new[] { 1, 2, 3 };
                for (int k = 0; k < types.Count; k++) { counts[types[k]] = Count42(counts, types[k]) + 1; AddPathRoom42(types[k], s, rows[k]); }
                prev = types;
            }
            AddPathRoom42(RoomType.Boss, mid + 1, 2);
            // optional secret room behind the cracked north wall of one fight room
            if (Random.value < T.secretRoomChance)
            {
                var hosts = new List<int>();
                for (int s = 1; s <= mid; s++) foreach (int i in _steps42[s]) { var r = floor[i]; if (r.IsFight && r.type != RoomType.Boss) hosts.Add(i); }
                if (hosts.Count > 0)
                {
                    int h = hosts[Random.Range(0, hosts.Count)]; var host = floor[h];
                    var sec = new FloorRoom { gx = host.gx, gy = 0, type = RoomType.Secret, step = -1, exitsSet = true, shapeSeed = Random.Range(0, int.MaxValue) };
                    sec.art = PickArt(RoomType.Secret, out sec.sizeClass); sec.cleared = true; sec.chestOpen = new bool[8];
                    int si = floor.Count; floor.Add(sec); sec.nb[2] = h; host.nb[0] = si; host.secretLink = true; host.hostsSecret = true;
                }
            }
            floorCols = mid + 2; floorRows = 4;
            Random.state = st;
            HellordAudio.MusicPitch = HData.At(T.themeMusicPitch, Theme + 1, 1f);
            Debug.Log("[Hellord] v0.0.42 floor " + (_floorIdx + 1) + " (" + FloorLabel53(ThemeSlot53, false) + ") seed " + _runSeed42 + ": " + mid + " steps, " + floor.Count + " rooms");
            curRoom = -1; EnterRoom(0, -1);
        }
        static int Count42(Dictionary<RoomType, int> c, RoomType t) => c.TryGetValue(t, out var n) ? n : 0;
        void AddPathRoom42(RoomType t, int step, int row)
        {
            while (_steps42.Count <= step) _steps42.Add(new List<int>(3));
            var r = new FloorRoom { gx = step, gy = row, type = t, step = step };
            _genFloor49 = FloorIdx; r.art = PickArt42(t, out r.sizeClass); r.cleared = !r.IsFight;
            if (t == RoomType.Shrine) RollShop(r);
            r.shapeSeed = Random.Range(0, int.MaxValue); r.shape = PickShape42(r);
            _steps42[step].Add(floor.Count); floor.Add(r);
        }
        static bool Special42(RoomType t) => t != RoomType.Survive && t != RoomType.Hunt;
        RoomType PickPathType42(int s, int mid, List<RoomType> prev, List<RoomType> same, Dictionary<RoomType, int> counts)
        {
            var T = HellordTuning.I;
            RoomType[] pool = { RoomType.Survive, RoomType.Hunt, RoomType.Nests, RoomType.Elite, RoomType.Curse, RoomType.Treasure, RoomType.Shrine };
            float[] w = { T.weightSurvive, T.weightHunt, T.weightNests, T.weightElite, T.weightCurse, T.weightTreasure, T.weightShop };
            int[] cap = { 999, 999, T.maxNestsPerFloor, T.maxElitePerFloor, T.maxCursePerFloor, T.maxTreasurePerFloor, T.maxShopPerFloor };
            float tot = 0;
            for (int i = 0; i < pool.Length; i++)
            {
                var t = pool[i]; float wi = Mathf.Max(0f, w[i]);
                if (FloorIdx == 0 && t == RoomType.Nests) wi = 0; // v0.0.95 t1014u: never hive on floor 1
                if (s == 1 && T.firstRoomPlainFight && t != RoomType.Survive && t != RoomType.Hunt && t != RoomType.Nests) wi = 0;
                if ((t == RoomType.Elite || t == RoomType.Curse) && s < T.eliteMinStep) wi = 0;
                if (t == RoomType.Shrine && (s < 2 || _shopSteps42.Contains(s + 1) || _shopSteps42.Contains(s - 1))) wi = 0;
                if (T.distinctDoorTypes && same.Contains(t)) wi = 0;
                if (Special42(t) && prev.Contains(t)) wi = 0;                 // no same special type twice in a row on any path
                int got = Count42(counts, t); foreach (var x in same) if (x == t) got++;
                if (got >= cap[i]) wi = 0;
                w[i] = wi; tot += wi;
            }
            if (tot <= 0.0001f) return !same.Contains(RoomType.Survive) ? RoomType.Survive : !same.Contains(RoomType.Hunt) ? RoomType.Hunt : RoomType.Survive;
            float rr = Random.value * tot;
            for (int i = 0; i < pool.Length; i++) { if (rr < w[i]) return pool[i]; rr -= w[i]; }
            return RoomType.Survive;
        }
        /// <summary>Walkable area (world units^2) of a v26 room art from its name ("v26_WxH_a"); -1 if unknown.</summary>
        static float ArtAreaU2_42(string art)
        {
            if (string.IsNullOrEmpty(art) || !art.StartsWith("v26_")) return -1;
            var p = art.Split('_'); if (p.Length < 2) return -1; var wh = p[1].Split('x');
            if (wh.Length != 2 || !int.TryParse(wh[0], out int w) || !int.TryParse(wh[1], out int h)) return -1;
            return (w - 48) / 16f * ((h - 64) / 16f); // 24 px side walls, 48 north, 16 south (all v26 fight rooms)
        }
        /// <summary>PickArt, re-rolled (max 8 times) while the walkable area is outside roomWalkAreaMin/Max.</summary>
        static string PickArt42(RoomType t, out int cls)
        {
            var T = HellordTuning.I; string a = PickArt(t, out cls);
            for (int k = 0; k < 8; k++)
            {
                float ar = ArtAreaU2_42(a); if (ar < 0 || (ar >= RoomMin49(t) && ar <= RoomMax49(t))) break;
                a = PickArt(t, out cls);
            }
            if (T.combatV49 && T.roomFitFallback49i) { float ar = ArtAreaU2_42(a); if (ar >= 0 && (ar < RoomMin49(t) || ar > RoomMax49(t))) a = FitArt49i(t, a, ref cls); } // v0.0.49i
            return a;
        }

        // ================= exits (door choice) =================
        List<int> NextStep42(FloorRoom r) => r.step >= 0 && r.step + 1 < _steps42.Count ? _steps42[r.step + 1] : null;
        /// <summary>v0.0.42: on the first entry, the room's exits = the next step's rooms, on random sides (never the side we came in).
        /// The north side of a secret host stays the cracked wall; a secret host is never reached through a south door (we'd arrive at its cracked wall).</summary>
        void AssignExits42(FloorRoom r, int arrive)
        {
            if (!_pathMode42 || r.exitsSet) return; r.exitsSet = true;
            var next = NextStep42(r); if (next == null || next.Count == 0) return;
            var rng = new System.Random(r.shapeSeed ^ 0x5bd1e995);
            var dirs = new List<int> { 0, 1, 2, 3 }; if (arrive >= 0) dirs.Remove(arrive); if (r.hostsSecret) dirs.Remove(0);
            for (int i = dirs.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); int t = dirs[i]; dirs[i] = dirs[j]; dirs[j] = t; }
            if (arrive >= 0) { int fwd = (arrive + 2) % 4; if (dirs.Remove(fwd)) dirs.Insert(0, fwd); } // straight ahead is always one of the exits
            foreach (int c in next)
            {
                int pick = -1;
                for (int k = 0; k < dirs.Count && pick < 0; k++) if (!(floor[c].hostsSecret && dirs[k] == 2)) pick = dirs[k];
                if (pick < 0) continue; dirs.Remove(pick); r.nb[pick] = c;
            }
        }
        static string DoorLabel42(RoomType t)
        {
            switch (t)
            {
                case RoomType.Survive: return "SURVIVE"; case RoomType.Hunt: return "HUNT"; case RoomType.Nests: return "NESTS"; case RoomType.Elite: return "ELITE";
                case RoomType.Shrine: return "SHOP"; case RoomType.Curse: return "CURSE"; case RoomType.Treasure: return "TREASURE"; case RoomType.Boss: return "BOSS"; default: return "?";
            }
        }
        /// <summary>v0.0.42: icon (icons_room_8) + name of the room behind every exit door. Placeholder - no new art.</summary>
        void DrawDoorLabels42(FloorRoom r)
        {
            if (!_pathMode42 || !HellordTuning.I.doorLabels || _room == null || mode != Mode.Play) return;
            var D = _room.D;
            foreach (var d in _doors)
            {
                if (d.secret || d.to < 0 || d.to >= floor.Count) continue;
                var t = floor[d.to].type; string s = DoorLabel42(t); var p = DoorPivotPx(d.dir);
                // pull the label into the room so it is never under a wall / off the view edge
                float lx = d.dir == 1 ? p.x - 40 : d.dir == 3 ? p.x + 40 : p.x, ly = d.dir == 0 ? D.wallN + 30 : d.dir == 2 ? p.y - 34 : p.y - 30;
                var c = RoomToCanvas(lx, ly); float w = 10 + TextW(s), x = Mathf.Clamp(c.x - w / 2, 4, 636 - w), y = Mathf.Clamp(c.y, 50, 340);
                GUI.color = r.locked ? new Color(1, 1, 1, 0.45f) : Color.white;
                if (_tRoomIcons != null) Blit(_tRoomIcons, (int)Mathf.Min((int)t, 7) * 8, 0, 8, 8, x, y - 1);
                GUI.color = Color.white;
                Text(s, x + 10, y, r.locked ? new Color32(0x8b, 0x9b, 0xb4, 255) : t == RoomType.Boss ? new Color32(0xe4, 0x3b, 0x44, 255) : (Color32)HCyan);
            }
        }
        /// <summary>v0.0.42 path minimap: visited rooms, the exits on offer, the boss at the end. Columns = steps.</summary>
        void DrawPathMinimap42()
        {
            float mw = floorCols * 10 + 9, mh = floorRows * 8 + 9, mx = 636 - mw, my = 4;
            Nine(_tPanel, 0, 48, 40, 5, 5, 4, 4, mx, my, mw, mh);
            if (HellordTuning.I.hudFloorNumber) { string fl = "FLOOR " + (FloorIdx + 1) + "/" + FloorCount + " " + FloorLabel53(ThemeSlot53, false); Text(fl, 636 - TextW(fl), my + mh + 3 + FloorLblDy45(), Theme == 0 ? Color.white : (Color)new Color32(0xfe, 0xe7, 0x61, 255)); }
            var cur = Cur; var next = cur != null ? NextStep42(cur) : null;
            for (int i = 0; i < floor.Count; i++)
            {
                var r = floor[i]; bool offered = next != null && next.Contains(i) && ExitShown42(cur, i);
                bool shown = r.type == RoomType.Secret ? (r.visited || r.revealed) : (r.visited || offered || r.type == RoomType.Boss || _mapRevealed);
                if (!shown) continue;
                float cx = mx + 5 + r.gx * 10, cy = my + 5 + r.gy * 8;
                int cell = i == curRoom ? 0 : r.visited ? (r.cleared ? 1 : 2) : 2;
                Blit(_tCells, cell * 9, 0, 9, 7, cx, cy);
                Blit(_tIcons5, (int)r.type * 5, 0, 5, 5, cx + 2, cy + 1);
            }
        }
        bool ExitShown42(FloorRoom r, int idx) { if (r == null) return false; for (int d = 0; d < 4; d++) if (r.nb[d] == idx) return true; return false; }

        // ================= room shapes (pits) =================
        struct Block42 { public Rect vis, col; }
        readonly List<Block42> _blocks42 = new List<Block42>(8); float _blockAreaU2;
        int PickShape42(FloorRoom r)
        {
            var T = HellordTuning.I;
            if (!r.IsFight || r.type == RoomType.Boss) return 0;
            float ar = ArtAreaU2_42(r.art); if (ar < 0 || ar < T.shapeMinAreaU2) return 0;
            float a = Mathf.Max(0, T.shapeRectWeight), b = Mathf.Max(0, T.shapeLWeight), c = Mathf.Max(0, T.shapePlusWeight), d = Mathf.Max(0, T.shapePillarsWeight), tot = a + b + c + d;
            if (tot <= 0.0001f) return 0; float rr = Random.value * tot;
            return rr < a ? 0 : rr < a + b ? 1 : rr < a + b + c ? 2 : 3;
        }
        /// <summary>v0.0.42: build this room's pits (after BuildRoomObjects, which clears the floor sprites). Same room = same pits (seeded).</summary>
        void BuildBlocks42(FloorRoom r)
        {
            _blocks42.Clear(); _blockAreaU2 = 0;
            if (!_pathMode42 || r == null || r.shape == 0 || _room == null || !r.IsFight || r.type == RoomType.Boss) return;
            var T = HellordTuning.I; var D = _room.D; var rng = new System.Random(r.shapeSeed);
            float L = D.wallSide, R = D.w - D.wallSide, Tp = D.wallN, B = D.h - D.wallS, WW = R - L, WH = B - Tp;
            var avoid = new List<Vector2>(16);
            for (int d = 0; d < 4; d++) { avoid.Add(DoorPivotPx(d)); avoid.Add(ArrivePx(d)); }
            avoid.Add(new Vector2(D.spawnX > 0 ? D.spawnX : D.w / 2, D.spawnY > 0 ? D.spawnY : D.h / 2 + 40));
            avoid.Add(new Vector2(D.w / 2, D.h / 2 + 16)); avoid.Add(new Vector2(D.w / 2, D.h / 2 + 20));
            avoid.Add(new Vector2(D.w * 0.25f, D.h * 0.35f)); avoid.Add(new Vector2(D.w * 0.75f, D.h * 0.35f)); avoid.Add(new Vector2(D.w * 0.5f, D.h * 0.75f)); // nest spots
            avoid.Add(new Vector2(D.w / 2, D.h / 2 + 16 - Mathf.Min(120f, WH * 0.2f))); // elite spot
            avoid.Add(HeroPx);
            float pad = Mathf.Max(0, T.blockAvoidPadPx);
            float cutLo = Mathf.Clamp(T.cornerCutMin, 0.05f, 0.45f), cutHi = Mathf.Clamp(Mathf.Max(T.cornerCutMin, T.cornerCutMax), 0.05f, 0.45f);
            if (r.shape == 1 || r.shape == 2)
            {
                int first = rng.Next(4);
                for (int k = 0; k < (r.shape == 1 ? 1 : 4); k++)
                {
                    int corner = (first + k) % 4; bool left = corner == 0 || corner == 3, top = corner == 0 || corner == 1;
                    float fw = cutLo + (float)rng.NextDouble() * (cutHi - cutLo), fh = cutLo + (float)rng.NextDouble() * (cutHi - cutLo);
                    for (int shrink = 0; shrink < 4; shrink++, fw *= 0.8f, fh *= 0.8f)
                    {
                        float cw = Mathf.Round(WW * fw / 16f) * 16f, ch = Mathf.Round(WH * fh / 16f) * 16f; if (cw < 32 || ch < 32) break;
                        var vis = new Rect(left ? L : R - cw, top ? Tp : B - ch, cw, ch);
                        if (HitsAvoid42(vis, avoid, pad)) continue;
                        // the collision box runs into the walls so the only free edges face the room
                        var col = Rect.MinMaxRect(left ? 0 : vis.xMin, top ? 0 : vis.yMin, left ? vis.xMax : D.w, top ? vis.yMax : D.h);
                        _blocks42.Add(new Block42 { vis = vis, col = col }); break;
                    }
                }
            }
            else if (r.shape == 3)
            {
                int pl0 = Mathf.Max(0, T.pillarsMin); int n = rng.Next(pl0, Mathf.Max(pl0, T.pillarsMax) + 1); float gap = Mathf.Max(24, T.pillarGapPx);
                for (int k = 0; k < n; k++)
                    for (int tries = 0; tries < 30; tries++)
                    {
                        int wl0 = Mathf.Max(16, T.pillarWMin), hl0 = Mathf.Max(16, T.pillarHMin);
                        float w = rng.Next(wl0, Mathf.Max(wl0, T.pillarWMax) + 1), h = rng.Next(hl0, Mathf.Max(hl0, T.pillarHMax) + 1);
                        float x0 = L + gap, x1 = R - gap - w, y0 = Tp + gap, y1 = B - gap - h; if (x1 <= x0 || y1 <= y0) break;
                        var vis = new Rect(Mathf.Round(x0 + (float)rng.NextDouble() * (x1 - x0)), Mathf.Round(y0 + (float)rng.NextDouble() * (y1 - y0)), w, h);
                        if (HitsAvoid42(vis, avoid, pad)) continue;
                        bool clash = false; foreach (var o in _blocks42) { var g = new Rect(o.vis.x - gap, o.vis.y - gap, o.vis.width + 2 * gap, o.vis.height + 2 * gap); if (g.Overlaps(vis)) { clash = true; break; } }
                        if (clash) continue;
                        _blocks42.Add(new Block42 { vis = vis, col = vis }); break;
                    }
            }
            foreach (var b in _blocks42) _blockAreaU2 += b.vis.width / 16f * (b.vis.height / 16f);
            float walk = WW / 16f * (WH / 16f);
            if (walk - _blockAreaU2 < T.roomWalkAreaMinU2) { _blocks42.Clear(); _blockAreaU2 = 0; return; } // keep density sane
            foreach (var b in _blocks42) DrawBlock42(b.vis, L, R, Tp, B);
            if (_hero) { ClampToRoom(ref _heroPos, HeroMarginPx); _hero.position = _heroPos; }
        }
        static bool HitsAvoid42(Rect vis, List<Vector2> avoid, float pad)
        {
            var g = new Rect(vis.x - pad, vis.y - pad, vis.width + 2 * pad, vis.height + 2 * pad);
            foreach (var p in avoid) if (g.Contains(p)) return true; return false;
        }
        void DrawBlock42(Rect v, float L, float R, float Tp, float B)
        {
            var T = HellordTuning.I;
            var fill = FloorSR("pit42", HArt.Pixel(), new Vector2(v.xMin, v.yMax), HellordRoom.PropOrder - 500);
            fill.transform.localScale = new Vector3(v.width, v.height, 1); fill.color = T.blockFillColor; // 1 px sprite, pivot bottom-left
            if (!T.blockEdgeProps) return;
            var fence = HellordRoom.SingleR("oxpit_prop_fence_mid"); var fence2 = HellordRoom.SingleR("oxpit_prop_fence_broken"); var post = HellordRoom.SingleR("oxpit_prop_skullpost");
            int n = 0;
            // horizontal edges facing the room: fences (32x22, pivot top-left)
            float[] hy = { v.yMin, v.yMax }; bool[] hOpen = { v.yMin > Tp + 1, v.yMax < B - 1 };
            for (int e = 0; e < 2; e++)
            {
                if (!hOpen[e] || fence == null) continue;
                for (float x = v.xMin; x + 32 <= v.xMax + 0.5f; x += 32)
                {
                    var s = (n++ % 3 == 2 && fence2 != null) ? fence2 : fence; float baseY = hy[e] + 2;
                    FloorSR("pit42_fence", s, new Vector2(x, baseY - 22), YSort(-baseY / HArt.PPU));
                }
            }
            // vertical edges facing the room: skull posts (24x42)
            float[] vx = { v.xMin, v.xMax }; bool[] vOpen = { v.xMin > L + 1, v.xMax < R - 1 };
            for (int e = 0; e < 2; e++)
            {
                if (!vOpen[e] || post == null) continue;
                for (float y = v.yMin + 40; y <= v.yMax + 0.5f; y += 40)
                    FloorSR("pit42_post", post, new Vector2(vx[e] - 12, y - 42), YSort(-y / HArt.PPU));
            }
        }
        /// <summary>v0.0.42: feet (hero / demons, world pos) are pushed out of the pits to the nearest free edge inside the room.</summary>
        void PushOutBlocks42(ref Vector2 pos, float marginPx)
        {
            if (_blocks42.Count == 0 || _room == null) return; var D = _room.D;
            var p = new Vector2(pos.x * HArt.PPU, -pos.y * HArt.PPU);
            float wl = D.wallSide + marginPx, wr = D.w - D.wallSide - marginPx, wt = D.wallN + marginPx, wb = D.h - D.wallS - marginPx;
            for (int pass = 0; pass < 2; pass++)
            {
                bool moved = false;
                foreach (var b in _blocks42)
                {
                    var c = new Rect(b.col.x - marginPx, b.col.y - marginPx, b.col.width + 2 * marginPx, b.col.height + 2 * marginPx);
                    if (!c.Contains(p)) continue;
                    Vector2 best = p; float bd = float.MaxValue;
                    Vector2[] cand = { new Vector2(c.xMin - 0.01f, p.y), new Vector2(c.xMax + 0.01f, p.y), new Vector2(p.x, c.yMin - 0.01f), new Vector2(p.x, c.yMax + 0.01f) };
                    foreach (var q in cand)
                    {
                        if (q.x < wl || q.x > wr || q.y < wt || q.y > wb) continue;
                        float dd = (q - p).sqrMagnitude; if (dd < bd) { bd = dd; best = q; }
                    }
                    if (bd < float.MaxValue) { p = best; moved = true; }
                }
                if (!moved) break;
            }
            pos = new Vector2(p.x / HArt.PPU, -p.y / HArt.PPU);
        }
    }
}
