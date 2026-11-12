using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.8: one playable Ox Pit (Level 10) floor = 8-11 rooms on a grid from hand-made layouts.
// Rooms: start -> 3-4 fights (survive / hunt / nests) -> boss; side rooms: elite, bone shrine, secret, curse or treasure (+ extra fight).
// Fight rooms lock their doors until the goal is met. Walking into an open door fades (exported fade_room_change) to the next room.
// Only exported art + code-drawn UI. Rooms are baked by tools/bake_rooms_v08.py (Resources/HellordArt/v08_* and rooms_*).
namespace Hellord
{
    public enum RoomType { Start, Survive, Nests, Hunt, Elite, Shrine, Curse, Treasure, Secret, Boss } // same order as icons_room_8 / minimap_icons_5

    public class FloorRoom
    {
        public int gx, gy; public RoomType type; public string art;
        public readonly int[] nb = { -1, -1, -1, -1 };           // N E S W -> room index (-1 none)
        public bool secretLink;                                    // this room's N neighbour is the secret room (cracked wall, not a door)
        public int sizeClass; public bool goalMet; public float goalMetT; public bool visited, cleared, active, locked, accepted, secretOpen, chestPending, rewardTaken, revealed;
        public float goalT; public int goalN, goalHave; public Vector2 chestPos;
        public float[] brkHp53; public bool[] iceBroken53, soulDead53; // pt483u decor state kept across re-entry (this run)
        public bool[] chestOpen; public SupportId[] shop; public int[] price; public bool[] sold;
        public int step = -1, shape, shapeSeed; public bool exitsSet, hostsSecret; // v0.0.42 random path: step on the path, exits assigned on entry, hosts the secret room; 0 rect 1 L 2 plus 3 pillars
        public bool IsFight => type == RoomType.Survive || type == RoomType.Nests || type == RoomType.Hunt || type == RoomType.Elite || type == RoomType.Curse || type == RoomType.Treasure || type == RoomType.Boss;
    }

    public partial class HellordGame
    {
        public bool floorMode = true;
        // Layout legend: S start, 1-4 main-path fights in order, F extra fight (hunt), E elite, H shrine, C curse, T treasure, X secret, B boss, '.' empty.
        static readonly string[][] Layouts =
        {
            new[] { "..X..", ".E23B", "HS1..", "..T.." },              // 9 rooms
            new[] { "B4...", ".32X.", ".C1E.", ".HSF." },              // 11 rooms
            new[] { ".X..", "S12.", "HE3B" },                          // 8 rooms
        };
        static readonly RoomType[] MainFights = { RoomType.Survive, RoomType.Hunt, RoomType.Nests, RoomType.Survive };
        static readonly int[] DX = { 0, 1, 0, -1 }, DY = { -1, 0, 1, 0 };
        static readonly string[] DirName = { "N", "E", "S", "W" };

        public readonly List<FloorRoom> floor = new List<FloorRoom>(12);
        public int curRoom, floorCols, floorRows, layoutIndex = -1; int _fightsCleared; bool _mapRevealed; const float DoorTol = 22f; readonly HashSet<FloorRoom> _debugRevealed = new HashSet<FloorRoom>(); float _roomT, _floorSpawnAcc;

        // floor objects (rebuilt per room)
        class FDoor { public int dir, to; public bool boss, secret; public SpriteRenderer sr, glow; public float lockT, openT; public LwItem lw; public Sprite[] lwF; }
        Transform _floorRoot; readonly List<FDoor> _doors = new List<FDoor>(4); readonly List<SpriteRenderer> _floorSr = new List<SpriteRenderer>();
        SpriteRenderer[] _bigChestSr; SpriteRenderer _altarSr, _stairsSr, _treasureSr; bool _stairsOn;
        Sprite[] _doorF, _doorBossF, _stairsF, _wallSecretF, _chestSmallF, _chestBigF, _nestF; Sprite _altarS;
        Texture2D _tFade, _tShop, _tRoomIcons, _tCard, _tDots, _tNew, _tSkillIcons, _tPause, _tPauseCur;

        // transition
        int _trTo = -1, _trDir; float _trT; bool _trLoaded;
        const float FadeFps = 20f;

        void BuildFloorObjects()
        {
            _floorRoot = new GameObject("FloorObjects").transform;
            _doorF = HArt.Strip("ui_door", 5, 32, 24, new Vector2(16, 23));
            _doorBossF = HArt.Strip("ui_door_boss", 2, 48, 32, new Vector2(24, 31));
            _stairsF = HArt.Strip("ui_stairs", 2, 32, 32, new Vector2(16, 31));
            _wallSecretF = HArt.Strip("ui_wall_secret", 3, 32, 32, new Vector2(16, 31));
            _chestSmallF = HArt.Strip("ui_chest", 3, 16, 16, new Vector2(8, 14));
            _chestBigF = HArt.Strip("rooms_chest_large", 3, 30, 24, new Vector2(15, 23));
            _nestF = HArt.Strip("ui_nest", 3, 48, 56, new Vector2(24, 53)); // v0.0.95 t1019 Bone Maw C
            _altarS = HArt.Strip("ui_altar_curse", 1, 24, 21, new Vector2(12, 20))[0];
        }

        // ================= build =================
        int _mapSeedUses;
        void BuildFloor()
        {
            floor.Clear(); _fightsCleared = 0; _stairsOn = false; _huntsCleared = 0; _mapRevealed = false; _debugRevealed.Clear(); _stairsOn = false;
            _pathMode42 = false; _blocks42.Clear(); _blockAreaU2 = 0; HellordAudio.MusicPitch = HData.At(HellordTuning.I.themeMusicPitch, Theme + 1, 1f); // v0.0.42
            if (HellordTuning.I.randomRoomPath) { BuildPathFloor42(); return; } // v0.0.42: random room path + door choice (the hand-made layouts below are the OFF fallback)
            string[] L; var rstate43 = Random.state; bool gen43 = HellordTuning.I.randomFloorMap; // v0.0.43 random floor map
            if (gen43) { int fs = FloorSeed42(_floorIdx); Random.InitState(fs); L = GenerateLayout43(fs); _genLayout43 = L; layoutIndex = -1; Debug.Log("[Hellord] v0.0.43 floor " + (_floorIdx + 1) + " seed " + _runSeed42 + " map:\n" + string.Join("\n", L)); }
            else {
            int mseed = (int)HellordGame.ArgFPublic("-mapseed", -1); // v0.0.21: '-mapseed N' = reproducible floor (layout + room rolls); differs per N
            if (mseed >= 0) { Random.InitState(mseed * 7919 + _mapSeedUses++); layoutIndex = (mseed + _floorIdx) % Layouts.Length; Debug.Log($"[Hellord] -mapseed {mseed} -> layout {layoutIndex}"); }
            else layoutIndex = (layoutIndex + 1 + Random.Range(0, _floorIdx > 0 ? Layouts.Length - 1 : Layouts.Length)) % Layouts.Length; // varies run to run; a new floor never repeats the layout
            L = Layouts[layoutIndex]; }
            floorRows = L.Length; floorCols = L[0].Length;
            var at = new int[floorCols, floorRows]; int start = 0;
            for (int y = 0; y < floorRows; y++)
                for (int x = 0; x < floorCols; x++)
                {
                    at[x, y] = -1; char c = L[y][x]; if (c == '.') continue;
                    RoomType t = c == 'S' ? RoomType.Start : c == 'E' ? RoomType.Elite : c == 'H' ? RoomType.Shrine : c == 'C' ? RoomType.Curse : c == 'T' ? RoomType.Treasure
                               : c == 'X' ? RoomType.Secret : c == 'B' ? RoomType.Boss : c == 'F' ? RoomType.Hunt : MainFights[(c - '1') % MainFights.Length];
                    var r = new FloorRoom { gx = x, gy = y, type = t }; r.art = PickArt(t, out r.sizeClass);
                    r.cleared = !r.IsFight; // start / shrine / secret have no goal
                    if (t == RoomType.Start) start = floor.Count;
                    if (t == RoomType.Shrine) RollShop(r);
                    at[x, y] = floor.Count; floor.Add(r);
                }
            // links: every orthogonal pair is connected, except the secret room (only to the room below it, through a cracked wall)
            for (int i = 0; i < floor.Count; i++)
            {
                var r = floor[i];
                for (int d = 0; d < 4; d++)
                {
                    int x = r.gx + DX[d], y = r.gy + DY[d]; if (x < 0 || y < 0 || x >= floorCols || y >= floorRows) continue;
                    int j = at[x, y]; if (j < 0) continue; var o = floor[j];
                    bool secretPair = r.type == RoomType.Secret || o.type == RoomType.Secret;
                    if (secretPair && !((r.type == RoomType.Secret && d == 2) || (o.type == RoomType.Secret && d == 0))) continue;
                    r.nb[d] = j; if (o.type == RoomType.Secret && d == 0) r.secretLink = true;
                }
            }
            foreach (var r in floor) if (r.type == RoomType.Secret) { r.chestOpen = new bool[8]; }
            if (gen43) Random.state = rstate43;
            if (FloorIdx == 0) // v0.0.95 t1014u: never hive on floor 1 (hand layouts / gen fallback)
                foreach (var rr in floor) if (rr.type == RoomType.Nests) { rr.type = RoomType.Survive; rr.art = PickArt(RoomType.Survive, out rr.sizeClass); rr.cleared = false; }
            curRoom = -1; EnterRoom(start, -1);
        }
        // v0.0.26: fight rooms come in small / medium / large (weights in HellordTuning); art = v26_<WxH>_a|b baked by tools/bake_rooms_v26.py
        static readonly string[][] SizeArt = { new[] { "v26_640x368", "v26_768x432" }, new[] { "v26_896x512", "v26_1024x576" }, new[] { "v26_1152x656", "v26_1280x736" } };
        static string PickArt(RoomType t, out int cls)
        {
            cls = 0; var T = HellordTuning.I;
            switch (t) { case RoomType.Shrine: return "rooms_shrine_672x384"; case RoomType.Secret: return "rooms_secret_640x360"; case RoomType.Boss: cls = 2; return ClipRoom; }
            if (t != RoomType.Start)
            {
                int min = t == RoomType.Nests ? T.nestsMinSize : t == RoomType.Elite ? T.eliteMinSize : 0;
                float ws = min >= 1 ? 0 : T.roomSmallWeight, wm = min >= 2 ? 0 : T.roomMediumWeight, wl = T.roomLargeWeight, tot = ws + wm + wl;
                if (tot <= 0.0001f) cls = Mathf.Clamp(min, 0, 2);
                else { float rr = Random.value * tot; cls = rr < ws ? 0 : rr < ws + wm ? 1 : 2; }
            }
            return SizeArt[cls][Random.Range(0, 2)] + (Random.value < 0.5f ? "_a" : "_b");
        }
        string RoomLabel(RoomType t)
        {
            switch (t)
            {
                case RoomType.Start: return Theme == 0 ? "OX PIT - LEVEL 10" : FloorName37(); // v0.0.42: theme case RoomType.Survive: return "SURVIVE 60 SECONDS"; case RoomType.Hunt: return "HUNT: KILL 100";
                case RoomType.Nests: return "DESTROY 3 NESTS"; case RoomType.Elite: return "ELITE: KILL THE ELITE AND ITS GUARDS"; case RoomType.Shrine: return "BONE SHRINE: 1-3 BUY OR SWAP";
                case RoomType.Curse: return "CURSE ALTAR: STEP ON IT TO ACCEPT"; case RoomType.Treasure: return "TREASURE: KILL 40"; case RoomType.Secret: return "SECRET ROOM!";
                default: return "BOSS: " + BossName37();
            }
        }
        FloorRoom Cur => curRoom >= 0 && curRoom < floor.Count ? floor[curRoom] : null;
        int BossIndex() { for (int i = 0; i < floor.Count; i++) if (floor[i].type == RoomType.Boss) return i; return -1; }

        // ================= room data helpers (room px, y down) =================
        int DoorX => _room != null && _room.D.doorX > 0 ? _room.D.doorX : (_room != null ? _room.D.w / 2 : 0);
        int DoorY => _room != null && _room.D.doorY > 0 ? _room.D.doorY : (_room != null ? (_room.D.h + 32) / 2 : 0);
        Vector2 HeroPx => new Vector2(_heroPos.x * HArt.PPU, -_heroPos.y * HArt.PPU);
        Vector2 DoorPivotPx(int d)
        {
            var D = _room.D;
            if (LowWallsOn44) switch (d) { case 0: return new Vector2(DoorX, D.wallN); case 1: return new Vector2(D.w - D.wallSide, DoorY); case 2: return new Vector2(DoorX, D.h - D.wallS); default: return new Vector2(D.wallSide, DoorY); }
            switch (d) { case 0: return new Vector2(DoorX, D.wallN); case 1: return new Vector2(D.w - 24, DoorY); case 2: return new Vector2(DoorX, D.h - 24); default: return new Vector2(24, DoorY); }
        }
        Vector2 ArrivePx(int d)
        {
            var D = _room.D;
            switch (d) { case 0: return new Vector2(DoorX, D.wallN + 20); case 1: return new Vector2(D.w - D.wallSide - 20, DoorY); case 2: return new Vector2(DoorX, D.h - D.wallS - 20); default: return new Vector2(D.wallSide + 20, DoorY); }
        }

        // ================= enter / leave =================
        void LeaveRoom()
        {
            var r = Cur; _ice.Clear(); _cracks.Clear(); Z37Clear(); // v0.0.37
            // stray enemies are removed (the floor is room-based); bones bank, blood is capped (v0.0.9)
            foreach (var e in _en) { if (e.boss) continue; if (e.alive || e.dying || e.nest) { DetachFx(e); StopStatuses(e); } e.alive = false; e.dying = false; e.nest = false; e.sr.gameObject.SetActive(false); }
            foreach (var p in _pr) KillProj(p); ClearHooks(); FX26RoomUnload();
            int leftXp = 0;
            foreach (var p in _pk)
            {
                if (!p.alive) continue; var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j);
                p.alive = false; p.sr.gameObject.SetActive(false); if (p.bone) bones += p.value; else leftXp += p.value;
            }
            // v0.0.9: was AddXp(all) -> several level-ups at the door. Bones still bank in full; left-behind blood is capped at
            // half of the current level's requirement and can never push past one level-up.
            if (leftXp > 0) { int need = HData.XpToNext(level) - xp; int give = Mathf.Min(leftXp, Mathf.Max(1, HData.XpToNext(level) / 2)); if (pendingLevels > 0) give = Mathf.Min(give, Mathf.Max(0, need - 1)); AddXp(Mathf.Max(0, give)); }
            _blasts.Clear();
            if (_chestAlive && r != null) { r.chestPending = true; r.chestPos = _chestPos; _chestAlive = false; _chestSr.gameObject.SetActive(false); }
            if (r != null && r.active && r.type != RoomType.Boss) { r.active = false; r.locked = false; } // only via F4 (debug): an unfinished goal restarts on re-entry
        }

        void EnterRoom(int idx, int exitDir)
        {
            if (curRoom >= 0) LeaveRoom(); PactRoomEnter53(); // v0.0.53 pact: Last Rites per room, room bones
            curRoom = idx; var r = floor[idx]; Debug.Log($"[HB] EnterRoom {idx} {r.type} cleared {r.cleared} exitDir {exitDir} t {runTime:0.0} bossAlive {BossAlive}");
            if (_room == null || _room.Name != r.art || r.type != RoomType.Boss) LoadRoom(r.art);
            int arrive = exitDir < 0 ? -1 : (exitDir + 2) % 4;
            AssignExits42(r, arrive); // v0.0.42 door choice
            if (arrive < 0)
            {
                if (r.type == RoomType.Boss) { for (int d = 0; d < 4; d++) if (r.nb[d] >= 0) arrive = d; }
                _heroPos = arrive >= 0 ? HellordRoom.Px(ArrivePx(arrive).x, ArrivePx(arrive).y)
                         : HellordRoom.Px(_room.D.spawnX > 0 ? _room.D.spawnX : _room.D.w / 2, _room.D.spawnY > 0 ? _room.D.spawnY : _room.D.h / 2 + 40);
            }
            else { var a = ArrivePx(arrive); _heroPos = HellordRoom.Px(a.x, a.y); }
            if (r.type == RoomType.Boss && !r.cleared && (arrive == 0 || exitDir < 0 || _pathMode42)) _heroPos = HellordRoom.Px(BossHeroStartX, BossHeroStartY); // v0.0.9: F4 teleport used the last neighbour's door (often the far-right wall) // keep the v0.7 arena start (boss stands on the north lane)
            ClampToRoom(ref _heroPos, HeroMarginPx); _hero.position = _heroPos; SnapCamera();
            _iframe = Mathf.Max(_iframe, RoomEnterGrace); // v0.0.16: entry grace - nothing can hit the hero in the first 1.5 s of a room
            HellordAudio.RoomEnterScream(r.type == RoomType.Boss || BossAlive); // v0.0.50 ambient screams
            bool firstVisit = !r.visited; r.visited = true; _roomT = 0; _floorSpawnAcc = 0; UpdateRoomGrade();
            BuildRoomObjects(r); BuildBlocks42(r); BuildHazards37(r); BuildTorture52(r); // v0.0.52 torture floors; v0.0.42 room shape pits
            ClearTongueTraps(); if ((HellordTuning.I.tongueTrapsInRooms || ArgTraps) && r.IsFight && r.type != RoomType.Boss) SpawnTongueTraps(); // v0.0.21 Level 1 hazard (off by default)
            if (r.chestPending) { r.chestPending = false; _chestAlive = true; _chestPos = r.chestPos; _chestSr.transform.position = _chestPos; _chestSr.sortingOrder = YSort(_chestPos.y); _chestSr.gameObject.SetActive(true); }
            if (!r.cleared && r.type != RoomType.Curse) StartGoal(r);
            if (r.cleared && r.IsFight) Toast("CLEARED");
            else if (r.type == RoomType.Start) { if (firstVisit) PostBanner("FLOOR " + (FloorIdx + 1) + " OF " + FloorCount + " - " + FloorName37(), Theme == 0 ? "LEVEL 10" : Theme == 1 ? "ICE SLIDES YOU. FROST CASTERS CHILL, 3 STACKS FREEZE" : "LAVA CRACKS BURN. FIRE WHEELS ROLL, SOME DEMONS EXPLODE", BannerKind.Floor); }
            else if (r.type == RoomType.Boss) { /* the boss banner comes from SpawnBoss */ }
            else if (HellordTuning.I.bannerRoomGoal) PostBanner(GoalTitle(r), GoalSub(r), BannerKind.Info);
            else Toast(GoalTitle(r));
        }

        static float SizeGoalMul(int cls) { var T = HellordTuning.I; return cls <= 0 ? T.goalMulSmall : cls == 1 ? T.goalMulMedium : T.goalMulLarge; }
        static readonly float ClipGoalScale = Mathf.Clamp(ArgF("-goalscale", 1f), 0.05f, 5f); // dev/clip flag: scales every kill/survive goal (default 1 = no change)
        int HuntGoalFor(FloorRoom r) => Mathf.Max(5, Mathf.RoundToInt((_huntsCleared == 0 ? FirstHuntGoal : HellordTuning.I.huntGoal) * SizeGoalMul(r.sizeClass) * ClipGoalScale * (1 + HellordTuning.I.floorGoalGrowth * FloorIdx)));
        int EliteGuardCount() => V49 ? Mathf.Min(HellordTuning.I.eliteGuards + 2 * FloorIdx, T49.eliteGuardsMax49i + 2 * FloorIdx) : HellordTuning.I.eliteGuards + 2 * FloorIdx; // v0.0.49i
        string GoalTitle(FloorRoom r)
        {
            var T = HellordTuning.I;
            switch (r.type)
            {
                case RoomType.Survive: return "SURVIVE " + Mathf.RoundToInt(T.surviveSeconds * SizeGoalMul(r.sizeClass) * (FloorIdx == 0 ? T.surviveF1SecondsMul53 : 1f)) + " SECONDS";
                case RoomType.Hunt: return "HUNT: KILL " + HuntGoalFor(r);
                case RoomType.Treasure: return "TREASURE: KILL " + Mathf.RoundToInt(T.treasureGoal * SizeGoalMul(r.sizeClass) * (1 + T.floorGoalGrowth * FloorIdx));
                case RoomType.Start: return "FLOOR " + (FloorIdx + 1);
                default: return RoomLabel(r.type);
            }
        }
        string GoalSub(FloorRoom r) => null;
        void StartGoal(FloorRoom r)
        {
            var T = HellordTuning.I;
            r.active = true; r.locked = true; r.goalHave = 0; r.goalMet = false; r.goalMetT = 0;
            foreach (var d in _doors) d.lockT = 0;
            FX26Lock(); // v0.0.26 DoorFX
            var D0 = _room.D; float wkW = D0.w - 2 * D0.wallSide, wkH = D0.h - D0.wallN - D0.wallS;
            var c = new Vector2(D0.w / 2, D0.h / 2 + 16); float gm = SizeGoalMul(r.sizeClass) * ClipGoalScale, fg = 1 + T.floorGoalGrowth * FloorIdx;
            switch (r.type)
            {
                case RoomType.Survive: r.goalT = T.surviveSeconds * gm * (FloorIdx == 0 ? T.surviveF1SecondsMul53 : 1f); break; // v0.0.53 O1
                case RoomType.Hunt: r.goalN = Mathf.Min(HuntGoalFor(r), KillGoalCap49i); break;
                case RoomType.Treasure: r.goalN = Mathf.Min(Mathf.RoundToInt(T.treasureGoal * gm * fg), KillGoalCap49i); break;
                case RoomType.Curse: r.goalT = T.curseSeconds; break;
                case RoomType.Nests:
                    r.goalN = 3;
                    var w = D0.w; var h = D0.h;
                    SpawnNest(new Vector2(w * 0.25f, h * 0.35f)); SpawnNest(new Vector2(w * 0.75f, h * 0.35f)); SpawnNest(new Vector2(w * 0.5f, h * 0.75f));
                    break;
                case RoomType.Elite:
                {
                    float dy = Mathf.Min(120f, wkH * 0.2f), rx = Mathf.Min(70f, wkW * 0.14f), ry = Mathf.Min(50f, wkH * 0.17f); int guards = EliteGuardCount();
                    var el = SpawnEnemyAt(EType.Elite, HellordRoom.Px(c.x, c.y - dy));
                    for (int i = 0; i < guards; i++) { float a = i / (float)guards * Mathf.PI * 2; SpawnEnemyAt(i % 2 == 0 ? EType.Brute : EType.Ghoul, HellordRoom.Px(c.x + Mathf.Cos(a) * rx, c.y - dy + Mathf.Sin(a) * ry)); }
                    r.goalN = guards + 1; break;
                }
                case RoomType.Boss: SpawnBoss(); break;
            }
        }

        // ================= per-room objects =================
        SpriteRenderer FloorSR(string n, Sprite s, Vector2 px, int order, float rotZ = 0)
        {
            var sr = NewSR(n, _floorRoot, s, order); sr.transform.position = HellordRoom.Px(px.x, px.y); sr.transform.rotation = Quaternion.Euler(0, 0, rotZ); _floorSr.Add(sr); return sr;
        }
        void BuildRoomObjects(FloorRoom r)
        {
            foreach (var s in _floorSr) if (s) Destroy(s.gameObject); _floorSr.Clear(); _doors.Clear(); _bigChestSr = null; _altarSr = _stairsSr = _treasureSr = null;
            int order = HellordRoom.PropOrder + 50; float[] rot = { 0, -90, 180, 90 };
            for (int d = 0; d < 4; d++)
            {
                int to = r.nb[d]; if (to < 0) continue;
                var fd = new FDoor { dir = d, to = to, boss = floor[to].type == RoomType.Boss || r.type == RoomType.Boss, secret = r.secretLink && d == 0 };
                var p = DoorPivotPx(d);
                if (fd.secret) fd.sr = FloorSR("wall_secret", _wallSecretF[r.secretOpen ? 2 : 1], new Vector2(DoorX, _room.D.wallN), order);
                else if (LowWallsOn44 && LowDoor44(fd, order)) { }
                else if (fd.boss) fd.sr = FloorSR("door_boss", _doorBossF[0], d == 0 ? new Vector2(p.x, p.y) : p, order, rot[d]);
                else fd.sr = FloorSR("door", _doorF[0], p, order, rot[d]);
                fd.lockT = 99; _doors.Add(fd);
            }
            var D = _room.D;
            if (LowWallsOn44) BuildLowWalls44();
            if (r.type == RoomType.Secret && D.chests != null)
            {
                _bigChestSr = new SpriteRenderer[D.chests.Length];
                for (int i = 0; i < D.chests.Length; i++) { var c = D.chests[i]; bool op = r.chestOpen[i]; _bigChestSr[i] = FloorSR("chest_large", _chestBigF[op ? 1 : c.elite != 0 ? 2 : 0], new Vector2(c.x, c.y), YSort(-c.y / (float)HArt.PPU)); }
            }
            if (r.type == RoomType.Curse && !r.accepted && !r.cleared) _altarSr = FloorSR("altar_curse", _altarS, new Vector2(D.w / 2, D.h / 2 + 20), YSort(-(D.h / 2 + 20) / (float)HArt.PPU));
            if (r.type == RoomType.Treasure && r.cleared) _treasureSr = FloorSR("chest", _chestSmallF[r.rewardTaken ? 1 : 0], new Vector2(D.w / 2, D.h / 2 + 16), YSort(-(D.h / 2 + 16) / (float)HArt.PPU));
            if (r.type == RoomType.Boss && _stairsOn) ShowStairs();
            FX26RegisterDoors(); // v0.0.26 DoorFX
        }
        // ================= v0.0.44 low walls + per-side doors =================
        bool LowWallsOn44 => HellordTuning.I.lowWallsV30 && _room != null && LowWalls.Available(Theme);
        static readonly string[] Side44 = { "N", "E", "S", "W" };
        /// <summary>Door strip for this side from the level's json (its own drawing, no rotation). False = art missing, use the old rotated door.</summary>
        bool LowDoor44(FDoor fd, int order)
        {
            string lv = LowWalls.Level(Theme), key = (fd.boss ? "boss_" : "door_") + Side44[fd.dir];
            var it = LowWalls.Item(lv, key); var f = LowWalls.Frames(lv, key); if (it == null || f == null || f.Length == 0) return false;
            var D = _room.D; int dx = DoorX, dy = DoorY;
            fd.lw = it; fd.lwF = f; fd.sr = FloorSR(key, f[0], LowWalls.DoorTopLeft(it, fd.dir, D, dx, dy), order);
            var g = LowWalls.Frames(lv, "glow"); var gi = LowWalls.Item(lv, "glow");
            if (HellordTuning.I.lowDoorGlow && g != null && g.Length > 0 && gi != null)
            {
                Vector2 c = fd.dir == 0 ? new Vector2(dx, D.wallN / 2f) : fd.dir == 2 ? new Vector2(dx, D.h - D.wallS / 2f) : fd.dir == 1 ? new Vector2(D.w - D.wallSide / 2f, dy) : new Vector2(D.wallSide / 2f, dy);
                int gx = gi.pivot != null && gi.pivot.Length > 1 ? gi.pivot[0] : gi.w / 2, gy = gi.pivot != null && gi.pivot.Length > 1 ? gi.pivot[1] : gi.h / 2;
                var gs = FloorSR(key + "_glow", g[0], new Vector2(c.x - gx, c.y - gy), order + 1);
                var m = HellordRoom.LightMatPublic(false);
                if (m != null) { gs.sharedMaterial = m; var mpb = new MaterialPropertyBlock(); gs.GetPropertyBlock(mpb); mpb.SetTexture("_MainTex", g[0].texture); mpb.SetFloat("_Strength", HellordTuning.I.lowDoorGlowK); gs.SetPropertyBlock(mpb); }
                else gs.color = new Color(1, 1, 1, 0.5f * HellordTuning.I.lowDoorGlowK);
                gs.enabled = false; fd.glow = gs;
            }
            return true;
        }
        /// <summary>Side walls (x 0..wallSide, from wallN down), bottom wall (last wallS rows), NW/NE/SW/SE corners; tiles are clipped around the side and south door strips so the doors sit in the wall.</summary>
        void BuildLowWalls44()
        {
            var D = _room.D; string lv = LowWalls.Level(Theme); int order = HellordRoom.BaseOrder + 200, W = D.w, H = D.h;
            int[] gapA = { -1, -1, -1, -1 }, gapB = { -2, -2, -2, -2 };
            foreach (var fd in _doors)
            {
                if (fd.lw == null) continue; var tl = LowWalls.DoorTopLeft(fd.lw, fd.dir, D, DoorX, DoorY);
                if (fd.dir == 1 || fd.dir == 3) { gapA[fd.dir] = (int)tl.y; gapB[fd.dir] = (int)tl.y + fd.lw.h - 1; }
                else if (fd.dir == 2) { gapA[2] = (int)tl.x; gapB[2] = (int)tl.x + fd.lw.w - 1; }
            }
            void Tile(string key, Vector2 tl) { var f = LowWalls.Frames(lv, key); if (f != null && f.Length > 0) FloorSR("lowwall_" + key, f[0], tl, order); }
            void ClipTile(string key, int x, int y, int x0, int x1, int y0, int y1) { var s = LowWalls.Clip(lv, key, x0, x1, y0, y1); if (s) FloorSR("lowwall_" + key, s, new Vector2(x + x0, y + y0), order); }
            foreach (int side in new[] { 3, 1 })
            {
                string key = side == 3 ? "wall_W" : "wall_E"; var it = LowWalls.Item(lv, key); if (it == null) continue;
                int x = side == 3 ? 0 : W - it.w, yEnd = H - D.wallS;
                for (int y = D.wallN; y < yEnd; y += it.h)
                {
                    int last = Mathf.Min(it.h - 1, yEnd - 1 - y), a = gapA[side] - y, b = gapB[side] - y;
                    if (b < 0 || a > last) { if (last == it.h - 1) Tile(key, new Vector2(x, y)); else ClipTile(key, x, y, 0, it.w - 1, 0, last); continue; }
                    if (a > 0) ClipTile(key, x, y, 0, it.w - 1, 0, a - 1);
                    if (b < last) ClipTile(key, x, y, 0, it.w - 1, b + 1, last);
                }
            }
            var si = LowWalls.Item(lv, "wall_S");
            if (si != null)
            {
                int y = H - si.h, xEnd = W - D.wallSide;
                for (int x = D.wallSide; x < xEnd; x += si.w)
                {
                    int last = Mathf.Min(si.w - 1, xEnd - 1 - x), a = gapA[2] - x, b = gapB[2] - x;
                    if (b < 0 || a > last) { if (last == si.w - 1) Tile("wall_S", new Vector2(x, y)); else ClipTile("wall_S", x, y, 0, last, 0, si.h - 1); continue; }
                    if (a > 0) ClipTile("wall_S", x, y, 0, a - 1, 0, si.h - 1);
                    if (b < last) ClipTile("wall_S", x, y, b + 1, last, 0, si.h - 1);
                }
            }
            var nw = LowWalls.Item(lv, "NW"); var ne = LowWalls.Item(lv, "NE"); var sw = LowWalls.Item(lv, "SW"); var se = LowWalls.Item(lv, "SE");
            if (nw != null) Tile("NW", new Vector2(0, 0));
            if (ne != null) Tile("NE", new Vector2(W - ne.w, 0));
            if (sw != null) Tile("SW", new Vector2(0, H - sw.h));
            if (se != null) Tile("SE", new Vector2(W - se.w, H - se.h));
        }
        void ShowStairs()
        {
            var D = _room.D; _stairsSr = FloorSR("stairs", _stairsF[1], new Vector2(D.w / 2, D.h / 2 + 16), HellordRoom.PropOrder + 60);
            FX26Stairs(HellordRoom.Px(D.w / 2, D.h / 2 + 16)); // v0.0.26 RoomFX beacon
        }

        // ================= update =================
        void UpdateFloor(float dt)
        {
            var r = Cur; if (r == null) return;
            _roomT += dt; UpdateHazards37(dt); UpdateTorture52(dt); // v0.0.52
            UpdateDoorsVisual(dt, r);
            var hp = HeroPx; var mv = HInput.Move(); var D = _room.D;
            // doors
            if (!r.locked)
                foreach (var d in _doors)
                {
                    bool push = false;
                    switch (d.dir)
                    {
                        case 0: push = mv.y > 0.5f && hp.y <= D.wallN + HeroMarginPx + 2 && Mathf.Abs(hp.x - DoorX) < DoorTol; break;
                        case 2: push = mv.y < -0.5f && hp.y >= D.h - D.wallS - HeroMarginPx - 2 && Mathf.Abs(hp.x - DoorX) < DoorTol; break;
                        case 1: push = mv.x > 0.5f && hp.x >= D.w - D.wallSide - HeroMarginPx - 2 && Mathf.Abs(hp.y - DoorY) < DoorTol; break;
                        case 3: push = mv.x < -0.5f && hp.x <= D.wallSide + HeroMarginPx + 2 && Mathf.Abs(hp.y - DoorY) < DoorTol; break;
                    }
                    if (!push) continue;
                    if (d.secret && !r.secretOpen)
                    {
                        r.secretOpen = true; floor[d.to].revealed = true; d.sr.sprite = _wallSecretF[2];
                        ScreenShake.Add(3, 0.25f); TellFX.DustPuff(HellordRoom.Px(DoorX, D.wallN), 8, 14); Toast("SECRET FOUND! WALK IN");
                        _bumpCd = 0.6f; break;
                    }
                    if (_bumpCd > 0) break;
                    StartTransition(d.to, d.dir); return;
                }
            if (_bumpCd > 0) _bumpCd -= dt;
            if (r.active) UpdateGoal(r, dt);
            // curse altar: step on it to accept
            if (r.type == RoomType.Curse && !r.accepted && !r.cleared && _altarSr && (hp - new Vector2(D.w / 2, D.h / 2 + 20)).magnitude < 14)
            {
                r.accepted = true; HellordAudio.Play("shrine_use"); maxHp = Mathf.Round(maxHp * 0.8f); hp_Clamp(); Destroy(_altarSr.gameObject); _altarSr = null;
                HellFX.HellfireNova(_heroPos, 30); ScreenShake.Add(3, 0.3f); PostBanner("CURSED: -20% MAX HP", "SURVIVE " + Mathf.RoundToInt(HellordTuning.I.curseSeconds) + " SECONDS", BannerKind.Danger); StartGoal(r);
            }
            // treasure chest
            if (_treasureSr && !r.rewardTaken && (hp - new Vector2(D.w / 2, D.h / 2 + 16)).magnitude < 14)
            { r.rewardTaken = true; _treasureSr.sprite = _chestSmallF[1]; bones += 20; HFX.Chest(HellordRoom.Px(D.w / 2, D.h / 2 + 16), true); HellordAudio.Play("chest_open"); Toast("TREASURE: +20 BONES"); OpenChest(); return; }
            // secret chests
            if (_bigChestSr != null)
                for (int i = 0; i < _bigChestSr.Length; i++)
                {
                    var c = D.chests[i]; if (r.chestOpen[i] || (hp - new Vector2(c.x, c.y)).magnitude > 18) continue;
                    r.chestOpen[i] = true; _bigChestSr[i].sprite = _chestBigF[1]; HFX.Chest(HellordRoom.Px(c.x, c.y), true);
                    if (c.elite != 0) { OpenChest(); return; }
                    int b = Random.Range(15, 26); bones += b; Toast("+" + b + " BONES");
                }
            // shrine
            if (r.type == RoomType.Shrine) UpdateShop(r, hp);
            // boss stairs
            if (_stairsSr && !BossRewardPending37 && (hp - new Vector2(D.w / 2, D.h / 2 + 16)).magnitude < 14)
            {
                if (FloorIdx + 1 < FloorCount) { _trTo = -2; _trDir = 0; _trT = 0; _trLoaded = false; mode = Mode.Transition; } // v0.0.26: next floor
                else mode = Mode.Won;
            }
        }
        float _bumpCd;
        void hp_Clamp() { hp = Mathf.Min(hp, maxHp); }

        void UpdateDoorsVisual(float dt, FloorRoom r)
        {
            foreach (var d in _doors)
            {
                if (d.secret) continue;
                if (r.locked) { d.lockT += dt; d.openT = 0; } else d.lockT = 0;
                if (!r.locked) d.openT += dt;
                if (d.lw != null && d.lwF != null)
                {
                    int fi = d.boss ? LowWalls.FrameIdx(d.lw, r.locked ? "locked" : "open") : LowWalls.NormalFrame(d.lw, r.locked, d.lockT, d.openT);
                    d.sr.sprite = d.lwF[Mathf.Clamp(fi < 0 ? (r.locked ? 0 : d.lwF.Length - 1) : fi, 0, d.lwF.Length - 1)];
                    if (d.glow) d.glow.enabled = !r.locked;
                    continue;
                }
                if (d.boss) { d.sr.sprite = _doorBossF[r.locked ? 0 : 1]; continue; }
                int f = !r.locked ? 0 : Mathf.Min(4, (int)(d.lockT * 12)); // close 0->3 @12fps then locked 4
                d.sr.sprite = _doorF[f];
            }
        }

        static int FirstHuntGoal => HellordTuning.I.firstHuntGoal; const float FirstHuntMul = 1.15f; int _huntsCleared;
        void UpdateGoal(FloorRoom r, float dt)
        {
            var T = HellordTuning.I; float m = _roomT / 60f + 0.4f * _fightsCleared;
            if (r.goalMet) { CheckClear(r, dt); return; } // v0.0.26: goal reached -> spawning stopped, clear the rest
            switch (r.type)
            {
                case RoomType.Survive: FloorSpawn(dt, m, T.surviveDensityMul * (FloorIdx == 0 ? T.surviveF1DensityMul53 : 1f)); /* v0.0.53 O1 */ r.goalT -= dt; if (r.goalT <= 0) GoalMet(r); break;
                case RoomType.Hunt:
                {   // first hunt eases in over 15 s (v0.0.9)
                    float mul = T.huntDensityMul * (_huntsCleared == 0 ? Mathf.Clamp01(0.5f + _roomT / 30f) : 1f);
                    FloorSpawn(dt, m, mul); if (r.goalHave >= r.goalN) GoalMet(r); break;
                }
                case RoomType.Treasure: FloorSpawn(dt, m, T.treasureDensityMul); if (r.goalHave >= r.goalN) GoalMet(r); break;
                case RoomType.Curse: FloorSpawn(dt, m + 0.5f, T.curseDensityMul); r.goalT -= dt; if (r.goalT <= 0) GoalMet(r); break;
                case RoomType.Nests: FloorSpawn(dt, m, T.nestsDensityMul); if (r.goalHave >= r.goalN) GoalMet(r); break;
                case RoomType.Elite: if (_roomT > 1 && RemainingCount() == 0) ClearRoom(r); break;
            }
        }
        /// <summary>v0.0.26: the goal is met. Spawning stops; the room clears when the remaining demons are dead (or instantly if clearInstant).</summary>
        void GoalMet(FloorRoom r)
        {
            if (r.goalMet || r.cleared) return;
            if (r.type == RoomType.Hunt) _huntsCleared++;
            if (HellordTuning.I.clearInstant || RemainingCount() == 0) { ClearRoom(r, true); return; }
            r.goalMet = true; r.goalMetT = 0; ScreenShake.Add(1.5f, 0.15f);
            if (HellordTuning.I.goalMetBanner) PostBanner("NO MORE DEMONS COMING", "KILL THE REST TO OPEN THE DOORS", BannerKind.Info); /* v0.0.45: off by default, it covered enemies mid-fight */
        }
        void CheckClear(FloorRoom r, float dt)
        {
            r.goalMetT += dt; int left = RemainingCount();
            if (left == 0 && r.goalMetT > 0.25f) { ClearRoom(r); return; }
            float st = HellordTuning.I.clearStragglerSeconds; if (st > 0 && r.goalMetT >= st) ClearRoom(r, true, true); // safety net: stuck / unreachable demons
        }
        int AliveCount() { if (_aliveFrame == Time.frameCount) return _aliveCache; int n = 0; foreach (var e in _en) if (e.alive && !e.boss) n++; _aliveFrame = Time.frameCount; _aliveCache = n; return n; }
        int _aliveFrame = -1, _aliveCache; // v0.0.9 perf: one 500-slot scan per frame (spawn loops bump the cache)
        /// <summary>v0.0.26: demons still to kill for the clear = alive + dying splitters that will still split.</summary>
        public int RemainingCount() { int n = 0; foreach (var e in _en) { if (e.boss) continue; if (e.alive || (e.dying && e.type == EType.Splitter && !e.splitDone)) n++; } return n; }
        /// <summary>v0.0.26: walkable room area in world units^2 (the density controller scales with this).</summary>
        public float RoomArea() { if (_room == null) return 600f; var D = _room.D; return Mathf.Max(100f, (D.w - 2 * D.wallSide) / 16f * ((D.h - D.wallN - D.wallS) / 16f) - _blockAreaU2); } // v0.0.42: minus shape pits
        public float FloorTargetAlive(float m, float mul)
        {
            var T = HellordTuning.I;
            float d = (T.combatV49 ? Density49 : T.densityPer100Units) * RoomArea() / 100f * (1 + T.densityRampPerProgress * m) * (1 + T.floorDensityGrowth * FloorIdx) * mul;
            return Mathf.Min(Mathf.Clamp(d, T.densityMin, T.densityMax), AliveCap49i); // v0.0.49i hard cap
        }
        void FloorSpawn(float dt, float m, float mul)
        {
            var T = HellordTuning.I; float target = FloorTargetAlive(m, mul); float deficit = target - AliveCount();
            if (deficit < 1f) { _floorSpawnAcc = Mathf.Min(_floorSpawnAcc, 0.99f); return; }
            _floorSpawnAcc += Mathf.Clamp(deficit / Mathf.Max(0.5f, T.spawnRefillSeconds), T.spawnRateMin, T.spawnRateMax) * dt;
            int guard = 0;
            while (_floorSpawnAcc >= 1 && guard++ < 8)
            {
                _floorSpawnAcc -= 1; if (AliveCount() >= T.densityMax + 40) continue; _aliveCache++;
                float rc = Mathf.Min(T.rangedMaxChance, T.rangedBaseChance + T.rangedRampPerProgress * m + T.rangedPerFloor * FloorIdx); // v0.0.26: ranged demons ramp with progress
                if (Random.value < rc && !Floor0NoOthers49) { bool hookFirst = Random.value < T.hookShare; if (hookFirst ? (TryFloorHookThrower(1f) || TryFloorGossipImp(1f)) : (TryFloorGossipImp(1f) || TryFloorHookThrower(1f))) continue; }
                float rr = Random.value; EType t = rr < 0.55f - m * 0.05f ? EType.Imp : rr < 0.85f ? EType.Ghoul : rr < 0.94f || m < 0.5f ? EType.Brute : EType.Splitter;
                if (t == EType.Splitter && Floor0NoOthers49) t = EType.Imp; // v0.0.49d
                if (TryRangeKit49()) continue;
                var e49 = SpawnEnemyAt(t, null); ApplyVariant37(e49); ApplyVariant49(e49); // v0.0.37: frost / exploding variants
            }
        }

        void ClearRoom(FloorRoom r, bool wasInstant = false, bool forced = false)
        {
            if (r.cleared) return;
            var T = HellordTuning.I;
            r.active = false; r.locked = false; r.cleared = true; r.goalMet = true; if (r.type != RoomType.Boss) _fightsCleared++;
            FX26Unlock(); // v0.0.26 DoorFX
            if (r.type != RoomType.Boss) { HellordAudio.Play("room_cleared_sting"); HellordAudio.PlayDelayed("door_unlock", 0.2f); } // v0.0.34
            foreach (var e in _en) { if (e.boss) continue; if (e.alive) { KillEnemy(e); e.splitDone = true; } else if (e.dying) e.splitDone = true; } // v0.0.9: no splitter children after the room is clear
            ScreenShake.Add(2, 0.2f);
            PactRoomClear53(); if (T.healOnClear > 0 && hp > 0 && !PactNoRegen53) { float add = Mathf.Min(maxHp - hp, maxHp * T.healOnClear); if (add >= 1f) { hp += add; Toast("+" + Mathf.RoundToInt(add) + " HP"); } }
            switch (r.type)
            {
                case RoomType.Curse: bones += 30; PostBanner("CURSE LIFTED", "+30 BONES AND A SUPPORT", BannerKind.Good); OpenChest(); break;
                case RoomType.Treasure: _treasureSr = FloorSR("chest", _chestSmallF[0], new Vector2(_room.D.w / 2, _room.D.h / 2 + 16), YSort(-(_room.D.h / 2 + 16) / (float)HArt.PPU)); PostBanner("ROOM CLEARED", "A CHEST APPEARS", BannerKind.Good); break;
                default: PostBanner("ROOM CLEARED", "THE DOORS ARE OPEN", BannerKind.Good); break;
            }
        }

        // ---- kills / nests ----
        void FloorKill(Enemy e) { var r = Cur; if (floorMode && r != null && r.active && (r.type == RoomType.Hunt || r.type == RoomType.Treasure)) r.goalHave++; }
        void SpawnNest(Vector2 px)
        {
            var e = SpawnEnemyAt(EType.Imp, HellordRoom.Px(px.x, px.y)); if (e == null) return;
            e.nest = true; e.frames = _nestF; e.maxHp = e.hp = V49 ? T49.nestHp49i * (1 + T49.nestHpPerFight49i * _fightsCleared) * (1 + T49.nestHpPerFloor49i * FloorIdx) : 260f * (1 + 0.35f * _fightsCleared); e.speed = 0; e.radius = 1.25f; e.hitY = 1.6f; e.dmg = 0; e.xp = 12; // v0.0.95 t1019 Bone Maw: taller art + larger hit (base ~40px)
            e.sr.sprite = _nestF[0]; e.sr.color = Color.white; e.tr.localScale = Vector3.one; e.animT = 1f + Random.value; e.tr.position = e.pos; e.sr.sortingOrder = YSort(e.pos.y);
        }
        void UpdateNest(Enemy e, float dt)
        {
            e.hitT -= dt; e.sr.sprite = _nestF[e.hitT > 0 ? 1 : 0]; e.sr.sortingOrder = YSort(e.pos.y); e.tr.position = e.pos;
            e.animT -= dt;
            if (e.animT <= 0) { e.animT = V49 ? T49.nestSpawnEvery49i : 2.5f; int nn = V49 ? T49.nestSpawnN49i : 2, cap = V49 ? Mathf.RoundToInt(AliveCap49i * T49.nestAliveCapMul49i) : 200; if (AliveCount() < cap) for (int i = 0; i < nn; i++, _aliveCache++) SpawnEnemyAt(Random.value < 0.6f ? EType.Imp : EType.Ghoul, e.pos + Random.insideUnitCircle.normalized * 1.3f); }
        }
        void OnNestKilled(Enemy e)
        {
            DetachFx(e); StopStatuses(e); e.nest = false; e.sr.gameObject.SetActive(false);
            FloorSR("nest_remains", _nestF[2], new Vector2(e.pos.x * HArt.PPU, -e.pos.y * HArt.PPU), YSort(e.pos.y) - 30); // dead frame stays as a remain
            HordeFX.CheapBurst(e.pos + Vector2.up * 0.6f, new Color(0.55f, 0.06f, 0.08f), 10); ScreenShake.Add(2, 0.15f);
            FX26NestDead(e.pos); // v0.0.26 RoomFX
            DropPickup(e.pos, false, e.xp, false); DropPickup(e.pos + Random.insideUnitCircle * 0.5f, true, 3, false); // v0.0.42: nest rewards are the objective, never decayed
            var r = Cur; if (r != null && r.type == RoomType.Nests) { r.goalHave++; Toast($"NEST DESTROYED {r.goalHave}/{r.goalN}"); }
        }

        // ---- boss ----
        void FloorBossKilled()
        {
            var r = Cur; if (!floorMode || r == null || r.type != RoomType.Boss) return;
            r.active = false; r.locked = false; r.cleared = true; _stairsOn = true; ShowStairs(); FX26Unlock();
            bool more = FloorIdx + 1 < FloorCount;
            PostBanner(BossName37() + " DOWN", more ? "A REWARD COMES FIRST, THEN THE STAIRS TO FLOOR " + (FloorIdx + 2) : "THE STAIRS ARE OPEN", BannerKind.Good, HellordTuning.I.bannerHold + 0.5f); FloorBossRewardStart37();
        }

        // ================= transition (fade_room_change: out 0->4, load, in 4->0 @20fps) =================
        public float trLoadMs, trLoadMsMax;
        void StartTransition(int to, int dir) { _trTo = to; _trDir = dir; _trT = 0; _trLoaded = false; mode = Mode.Transition; }
        void UpdateTransition()
        {
            _trT += Mathf.Min(Time.unscaledDeltaTime, 1f / FadeFps); // v0.0.17: a load hitch can't skip the fade frames
            if (!_trLoaded && _trT >= 5f / FadeFps)
            {
                _trLoaded = true; var sw = System.Diagnostics.Stopwatch.StartNew(); if (_trTo == -2) AdvanceFloor(); else EnterRoom(_trTo, _trDir);
                trLoadMs = (float)sw.Elapsed.TotalMilliseconds; trLoadMsMax = Mathf.Max(trLoadMsMax, trLoadMs);
            }
            if (_trT >= 10f / FadeFps + 0.05f) { mode = Mode.Play; _trTo = -1; }
        }
        /// <summary>v0.0.26: the stairs lead to the next floor: new layout, new palette, tougher demons, a partial heal.</summary>
        void AdvanceFloor()
        {
            LeaveRoom(); curRoom = -1; ResetBoss(); ClearBBullets(); ClearBanners();
            if (_jumpFloor >= 0) { _floorIdx = _jumpFloor; _jumpFloor = -1; } else _floorIdx++; // v0.0.38: test menu can jump to any floor
            var T = HellordTuning.I; if (T.healOnFloorChange > 0 && hp > 0 && !PactNoRegen53) hp = Mathf.Min(maxHp, hp + maxHp * T.healOnFloorChange);
            BuildFloor(); _iframe = Mathf.Max(_iframe, 2f);
            Debug.Log($"[HB] FLOOR {_floorIdx + 1}/{FloorCount} built: layout {layoutIndex}, rooms {floor.Count}");
        }
        int FadeFrame()
        {
            if (mode != Mode.Transition) return -1;
            int k = (int)(_trT * FadeFps); return k < 5 ? k : Mathf.Max(0, 9 - k);
        }

        // ================= debug =================
        void FloorTeleportBoss()
        {
            int b = BossIndex(); if (b < 0) return;
            if (b == curRoom) { if (!BossAlive && !Cur.cleared) SpawnBoss(); else Toast("Already in the boss room"); return; }
            StartTransition(b, -1);
        }
        void FloorClearRoom()
        {
            var r = Cur; if (r == null) return;
            if (r.type == RoomType.Boss) { if (BossAlive) Damage(_boss, _boss.hp + 1, false, true); return; }
            if (r.type == RoomType.Curse && !r.accepted) { r.accepted = true; if (_altarSr) { Destroy(_altarSr.gameObject); _altarSr = null; } }
            if (r.cleared) { Toast("Room already clear"); return; }
            if (r.type == RoomType.Nests) foreach (var e in _en) if (e.alive && e.nest) KillEnemy(e);
            ClearRoom(r); Toast("DEBUG: room cleared");
        }
        void FloorRevealMap() { _mapRevealed = !_mapRevealed; foreach (var r in floor) if (r.type == RoomType.Secret) { if (_mapRevealed && !r.revealed) { r.revealed = true; _debugRevealed.Add(r); } else if (!_mapRevealed && _debugRevealed.Remove(r)) r.revealed = false; } Toast(_mapRevealed ? "DEBUG: map revealed" : "Map reveal off"); }

        // ================= bone shrine =================
        void RollShop(FloorRoom r)
        {
            var ids = new List<SupportId>(); foreach (var s in HData.Supports) ids.Add(s.id); Shuffle(ids);
            r.shop = new SupportId[3]; r.price = new int[3]; r.sold = new bool[3];
            for (int i = 0; i < 3; i++) { r.shop[i] = ids[i]; r.price[i] = 20 + Random.Range(0, 5) * 5; }
        }
        /// <summary>v0.0.19: the shrine statue is baked into the room art (drawn under every actor), so the hero could stand on it and be
        /// drawn over it. Its footprint (statue + horns + pedestal, room px, y down) is now solid: the hero's feet are pushed out to the nearest edge.</summary>
        void BlockShrineStatue(ref Vector2 pos)
        {
            var r = Cur; if (r == null || r.type != RoomType.Shrine || _room == null) return; var D = _room.D;
            float sx = D.shrineX > 0 ? D.shrineX : D.w / 2, sy = D.shrineY > 0 ? D.shrineY : D.h / 2;
            float x0 = sx - 28, x1 = sx + 28, y0 = sy - 96, y1 = sy + 3; var p = new Vector2(pos.x * HArt.PPU, -pos.y * HArt.PPU);
            if (p.x <= x0 || p.x >= x1 || p.y <= y0 || p.y >= y1) return;
            float dl = p.x - x0, dr = x1 - p.x, du = p.y - y0, dd = y1 - p.y, m = Mathf.Min(Mathf.Min(dl, dr), Mathf.Min(du, dd));
            if (m == dd) p.y = y1; else if (m == dl) p.x = x0; else if (m == dr) p.x = x1; else p.y = y0;
            pos = new Vector2(p.x / HArt.PPU, -p.y / HArt.PPU);
        }
        bool NearShrine(Vector2 hp) { var D = _room.D; float sx = D.shrineX > 0 ? D.shrineX : D.w / 2, sy = D.shrineY > 0 ? D.shrineY : D.h / 2; return (hp - new Vector2(sx, sy + 40)).magnitude < 110; }
        /// <summary>v0.0.19: an unsold offer that fits none of the hero's skills is swapped for one that does (checked every shop frame,
        /// since skills change after the stock was rolled). Only if no support fits anything does 'NO FIT' remain.</summary>
        void RefitShop(FloorRoom r)
        {
            if (r.shop == null) return;
            for (int i = 0; i < 3; i++)
            {
                if (r.sold[i] || ShopTarget(HData.Support(r.shop[i]), out _) >= 0) continue;
                var c = new List<SupportId>();
                foreach (var s in HData.Supports) if (System.Array.IndexOf(r.shop, s.id) < 0 && ShopTarget(s, out _) >= 0) c.Add(s.id);
                if (c.Count > 0) r.shop[i] = c[Random.Range(0, c.Count)];
            }
        }
        void UpdateShop(FloorRoom r, Vector2 hp)
        {
            RefitShop(r);
            if (_tp != null) { if (!NearShrine(hp)) _tp = null; else UpdateTargetKeys(); return; } // v0.0.36: second pick open - it owns keys 1-n / ESC
            if (!NearShrine(hp)) return;
            HKey[] keys = { HKey.One, HKey.Two, HKey.Three };
            for (int i = 0; i < 3; i++) if (HInput.Down(keys[i])) BuyShop(r, i);
        }
        /// <summary>Returns the target skill and whether it is a swap (replaces that skill's last support). -1 = fits nowhere.</summary>
        int ShopTarget(SupportDef sd, out bool swap) { var l = ShopTargets(sd, out swap); return l.Count > 0 ? l[0] : -1; }
        /// <summary>v0.0.36: every skill the shop support could go to (upgrade or open slot first, else swap candidates).</summary>
        List<int> ShopTargets(SupportDef sd, out bool swap)
        {
            swap = false; var l = SupportTargets(sd); if (l.Count > 0) return l; // v0.0.27: an owned support is upgraded
            for (int i = 0; i < skills.Count; i++) { var s = skills[i]; if (HData.Fits(sd, s.def) && !s.Has(sd.id) && s.sup.Count > 0) { swap = true; l.Add(i); } }
            return l;
        }
        void BuyShop(FloorRoom r, int i)
        {
            if (r.sold[i]) { Toast("SOLD OUT"); return; }
            var sd = HData.Support(r.shop[i]); var tl = ShopTargets(sd, out bool swap);
            if (tl.Count == 0) { Toast("NO SKILL CAN TAKE " + sd.name); return; }
            if (bones < r.price[i]) { Toast("NOT ENOUGH BONES"); return; }
            if (tl.Count > 1) { OpenTargetPick(sd, tl, swap, tIx => { if (!r.sold[i] && bones >= r.price[i]) CompleteShopBuy(r, i, sd, tIx, swap); }); return; } // v0.0.36: several skills can take it -> player chooses
            CompleteShopBuy(r, i, sd, tl[0], swap);
        }
        void CompleteShopBuy(FloorRoom r, int i, SupportDef sd, int t, bool swap)
        {
            bones -= r.price[i]; r.sold[i] = true; HellordAudio.Play("shop_purchase"); var s = skills[t];
            if (swap) { var old = HData.Support(s.sup[s.sup.Count - 1]); s.sup.RemoveAt(s.sup.Count - 1); s.AddSupport(sd.id); Toast($"SWAPPED {old.name} FOR {sd.name}"); }
            else { bool up = s.Has(sd.id); s.AddSupport(sd.id); Toast(up ? $"{sd.name} UPGRADED TO LV{s.Lvl(sd.id)}" : $"{sd.name} SOCKETED IN {s.def.name}"); }
            HellFX.LevelUpBurst(_hero.position);
        }

        // ================= 640 canvas drawing (called from OnGUI after HudBegin) =================
        void FloorTex()
        {
            if (_tFade != null) return;
            _tFade = UiTex("fade_room_change"); _tShop = UiTex("shop_box"); _tRoomIcons = UiTex("icons_room_8"); _tCard = UiTex("levelup_card"); _tDots = UiTex("levelup_level_dots");
            _tNew = UiTex("levelup_new_badge"); _tSkillIcons = UiTex("icons_skill_16"); _tPause = UiTex("pause_box"); _tPauseCur = UiTex("pause_cursor");
        }
        /// <summary>Room px -> 640 canvas px (camera centre is canvas (320,180)).</summary>
        Vector2 RoomToCanvas(float px, float py) => new Vector2(Mathf.Round(px - _camRig.position.x * HArt.PPU + 320), Mathf.Round(py + _camRig.position.y * HArt.PPU + 180));

        void DrawGoalB()
        {
            var r = Cur; if (r == null) return; string t = null; int fr = 0;
            if (r.active)
                switch (r.type)
                {
                    case RoomType.Survive: case RoomType.Curse: { float s = Mathf.Max(0, r.goalT); t = $"{(int)(s / 60)}:{(int)(s % 60):00}"; fr = 0; break; }
                    case RoomType.Hunt: case RoomType.Treasure: t = $"{Mathf.Min(r.goalHave, r.goalN)}/{r.goalN}"; fr = 1; break;
                    case RoomType.Nests: t = $"{r.goalHave}/{r.goalN}"; fr = 2; break;
                    case RoomType.Elite: t = $"{RemainingCount()} LEFT"; fr = 1; break;
                }
            if (r.active && r.goalMet && r.type != RoomType.Elite) { t = $"LEFT {RemainingCount()}"; fr = 1; } // v0.0.26: goal met, kill the rest
            else if (r.type == RoomType.Shrine) { t = "SHOP"; fr = 1; }
            else if (r.type == RoomType.Curse && !r.cleared) { t = "CURSE?"; fr = 2; }
            else if (r.IsFight && r.cleared && r.type != RoomType.Boss) { t = "CLEAR"; fr = 1; }
            if (t == null || (r.type == RoomType.Boss && BossAlive)) return;
            int gwid = Mathf.Max(44, 13 + TextW(t) + 4); float gx = 320 - gwid / 2;
            Nine(_tGoal, fr * 44, 44, 13, 12, 2, 3, 2, gx, 4, gwid, 13); Text(t, gx + 13, 7, Color.white);
        }

        bool RoomShown(int i)
        {
            var r = floor[i];
            if (r.type == RoomType.Secret) return r.visited || r.revealed;
            if (r.visited || _mapRevealed) return true;
            for (int d = 0; d < 4; d++) { int j = r.nb[d]; if (j >= 0 && floor[j].visited && floor[j].type != RoomType.Secret) return true; }
            return false;
        }
        void DrawMinimapB()
        {
            if (floor.Count == 0) return;
            if (_pathMode42) { DrawPathMinimap42(); return; } // v0.0.42
            float mw = floorCols * 10 + 9, mh = floorRows * 8 + 9, mx = 636 - mw, my = 4;
            Nine(_tPanel, 0, 48, 40, 5, 5, 4, 4, mx, my, mw, mh);
            if (HellordTuning.I.hudFloorNumber) { string fl = "FLOOR " + (FloorIdx + 1) + "/" + FloorCount + " " + FloorLabel53(ThemeSlot53, false); Text(fl, 636 - TextW(fl), my + mh + 3 + FloorLblDy45(), FloorIdx == 0 ? Color.white : new Color32(0xfe, 0xe7, 0x61, 255)); } // v0.0.26
            for (int i = 0; i < floor.Count; i++)
            {
                var r = floor[i]; if (!RoomShown(i)) continue;
                float cx = mx + 5 + r.gx * 10, cy = my + 5 + r.gy * 8;
                int cell = i == curRoom ? 0 : r.visited ? (r.cleared ? 1 : 2) : (_mapRevealed || r.revealed) ? 2 : 3;
                Blit(_tCells, cell * 9, 0, 9, 7, cx, cy);
                if (cell != 3) Blit(_tIcons5, (int)r.type * 5, 0, 5, 5, cx + 2, cy + 1);
                if (r.nb[1] >= 0 && RoomShown(r.nb[1])) Blit(_tCorr, 0, 0, 1, 1, cx + 9, cy + 3);
                if (r.nb[2] >= 0 && RoomShown(r.nb[2]) && (floor[r.nb[2]].type != RoomType.Secret)) Blit(_tCorr, 0, 0, 1, 1, cx + 4, cy + 7);
                if (r.type == RoomType.Secret && r.nb[2] >= 0) Blit(_tCorr, 0, 0, 1, 1, cx + 4, cy + 7);
            }
        }

        void DrawFloorWorldUI()
        {
            FloorTex(); var r = Cur; if (r == null || _room == null) return;
            DrawDoorLabels42(r); // v0.0.42 exit labels
            if (r.type == RoomType.Shrine && r.shop != null && mode == Mode.Play || r.type == RoomType.Shrine && mode == Mode.Transition) DrawShop(r);
            if (r.type == RoomType.Curse && !r.accepted && !r.cleared && _altarSr)
            {
                var p = RoomToCanvas(_room.D.w / 2, _room.D.h / 2 - 8); string s = "STEP ON: -20% MAX HP"; Text(s, p.x - TextW(s) / 2, p.y - 14, new Color32(0xe4, 0x3b, 0x44, 255));
                string s2 = "SURVIVE " + Mathf.RoundToInt(HellordTuning.I.curseSeconds) + "S FOR A SUPPORT + 30 BONES"; Text(s2, p.x - TextW(s2) / 2, p.y - 4, Color.white);
            }
        }
        void DrawShop(FloorRoom r)
        {
            var D = _room.D; float sy = D.shopY > 0 ? D.shopY : 244, cx = D.shrineX > 0 ? D.shrineX : D.w / 2;
            float[] w = new float[3]; float total = 0;
            for (int i = 0; i < 3; i++) { w[i] = ShopCardW(r, i); total += w[i]; }
            total += 2 * 6; float x = cx - total / 2; bool near = NearShrine(HeroPx);
            for (int i = 0; i < 3; i++)
            {
                var p = RoomToCanvas(x, sy); var sd = HData.Support(r.shop[i]); int t = ShopTarget(sd, out bool swap);
                GUI.color = r.sold[i] ? new Color(1, 1, 1, 0.45f) : bones < r.price[i] ? new Color(0.62f, 0.62f, 0.7f, 0.8f) : Color.white;
                Nine(_tShop, 0, 56, 30, 5, 5, 4, 4, p.x, p.y, w[i], 30); GUI.color = Color.white;
                Blit(_tRoomIcons, 5 * 8, 0, 8, 8, p.x + 5, p.y + 6);          // shrine icon (no support icon art exists)
                Text(ShopName(r, i), p.x + 16, p.y + 6, r.sold[i] ? new Color32(0x8b, 0x9b, 0xb4, 255) : Color.white);
                if (!r.sold[i])
                {
                    Blit(_tCIcons, 8, 0, 8, 8, p.x + 16, p.y + 17);
                    Text(r.price[i].ToString(), p.x + 26, p.y + 18, bones >= r.price[i] ? Color.white : (Color)new Color32(0xe4, 0x3b, 0x44, 255));
                    string tag = t < 0 ? "NO FIT" : swap ? "SWAP" : "BUY";
                    bool afford = bones >= r.price[i]; Color dim = new Color32(0x5a, 0x69, 0x88, 255); // v0.0.19: unaffordable = greyed tag, not active cyan
                    Text(tag, p.x + w[i] - 5 - TextW(tag), p.y + 18, t < 0 ? (Color)new Color32(0xe4, 0x3b, 0x44, 255) : !afford ? dim : swap ? (Color)new Color32(0xfe, 0xe7, 0x61, 255) : HCyan);
                }
                Text((i + 1).ToString(), p.x + w[i] / 2 - 2, p.y - 10, near ? HCyan : (Color)new Color32(0x8b, 0x9b, 0xb4, 255));
                x += w[i] + 6;
            }
            var b = RoomToCanvas(cx, sy + 34); string h = near ? "PRESS 1-3 TO BUY. SEVERAL FIT = YOU PICK THE SKILL. SWAP REPLACES THE LAST SUPPORT" : "COME CLOSER TO TRADE BONES";
            Text(h, b.x - TextW(h) / 2, b.y, new Color32(0x8b, 0x9b, 0xb4, 255));
        }
        // v0.0.16: card wide enough for the name AND 'icon price  BUY/SWAP/NO FIT' (price used to be drawn under the tag)
        string ShopTag(FloorRoom r, int i) { int t = ShopTarget(HData.Support(r.shop[i]), out bool swap); return t < 0 ? "NO FIT" : swap ? "SWAP" : "BUY"; }
        float ShopCardW(FloorRoom r, int i) => Mathf.Max(56, 16 + TextW(ShopName(r, i)) + 5, r.sold[i] ? 0 : 26 + TextW(r.price[i].ToString()) + 6 + TextW("NO FIT") + 5);
        public bool ShopLayoutOk(FloorRoom r, out string why)
        {
            why = ""; bool ok = true;
            for (int i = 0; i < 3; i++) { if (r.sold[i]) continue; float w = ShopCardW(r, i), pe = 26 + TextW(r.price[i].ToString()), ts = w - 5 - TextW(ShopTag(r, i)); why += $"card{i} w{w} priceEnd{pe} tagStart{ts}; "; if (pe + 3 > ts) ok = false; }
            return ok;
        }
        string ShopName(FloorRoom r, int i) => r.sold[i] ? "SOLD" : Sanitize(HData.Support(r.shop[i]).name);

        void DrawFadeB()
        {
            int f = FadeFrame(); if (f < 0) return;
            if (f >= 4) { GUI.color = HInk; var m = GUI.matrix; GUI.matrix = Matrix4x4.identity; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.matrix = m; GUI.color = Color.white; return; }
            // v0.0.9: tile the whole screen, not just the centred 640x360 canvas (a big browser window showed a dark rectangle)
            float z = GUI.matrix.m00 > 0 ? GUI.matrix.m00 : 1, x0 = -GUI.matrix.m03 / z, y0 = -GUI.matrix.m13 / z;
            float x1 = (Screen.width - GUI.matrix.m03) / z, y1 = (Screen.height - GUI.matrix.m13) / z;
            x0 = Mathf.Floor(x0 / 20) * 20; y0 = Mathf.Floor(y0 / 20) * 20;
            for (float y = y0; y < y1; y += 20) for (float x = x0; x < x1; x += 20) Blit(_tFade, f * 20, 0, 20, 20, x, y);
        }
    }
}
