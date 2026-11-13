using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.6: Hellord Art export wired (v2 enemies + 4-dir walk + death anims, Splitter, Club Jailer boss), final2 hero, 640x360.
// Hellord prototype v0.3 (art export + Pixel FX Pack v1.5.1 wiring). One component builds the whole run at runtime: camera rig, hero, horde, skills, pickups, UI.
// Enemies use a hash grid + distance checks (no physics). FX come from Pixel FX Pack (import it first).
namespace Hellord
{
    public enum EType { Imp, Ghoul, Brute, Elite, Splitter, SplitChild, HookThrower, GossipImp } // v0.0.24: HookThrower, v0.0.25: GossipImp (Level 1, placeholder art)
    public class Enemy
    {
        public int v44Fx = -1, v44Tr = -1; public byte m53; public int calc53, eqEl53 = -1, bst53; public bool calcB53; public float eqT53, bstore53, stun53, brit536; // t536u Brittle Mark expiry (runTime) // v0.0.53 pact node state (Calcify, Equilibrium, Hemorrhage). public byte m53: mortar demon kind+1 (0 = none) // v0.0.44 MinionFX handles (tell / fire trail), -1 = none
        public int id; public EType type; public bool alive, elite; public Vector2 pos, push; public float hp, maxHp, speed, radius, dmg, animT;
        public SpriteRenderer sr; public Transform tr; public Sprite[] frames; public int xp, cx, cy; public float hitT; public PixelFX fx; public int shockH = -1, bleedH = -1, burnH = -1, chillH = -1; public bool boss;
        public float hitY, deathT; public bool dying, splitDone, nest; public int walkRow; // v0.6: hurt-circle height (units above feet), death anim, facing row
        public float tint948Until, tint948Amt; public Color tint948Col; /* t948u brief status tint */ public float knock818; public int burnStk818; /* t818u sin #9 knock mark (runTime expiry) / #31 burn stacks */ public float slowT, slowAmt; /* v0.0.36 Grave Pool slow */ public float iceBuild; /* t1393u shared chill->freeze buildup */ public float shockAmp = 0.25f; public float burnT, burnDps, chillT, shockT, bleedT, bleedDps, curseT, freezeT, dotTick, orbitCd, auraCd; public int chillHits;
        public byte variant; public float fuseT = -1; /* v0.0.37: 1 frost, 2 exploding */ public float hookCd; public int hookWind = -1, hookH = -1, strafe = 1; public Vector2 hookPrevTip, hookAim; public bool hookTipOk; // v0.0.24 Hook Thrower
        public int v44St, v44Face; public float v44T, v44HurtT, v44CastT, v44AnimT; public byte k49; public int s49, n49; public float t49, cd49; public Vector2 a49, b49; /* v0.0.49 patterns */ public bool v44West; public Vector2 v44Dir, v44Prev; public bool v44Hit; /* v0.0.44 Frost Caster / Fire Wheel */ public float gossipCd, gossipT; public int gossipSt, gossipH = -1; public Vector2 gossipC; // v0.0.25 Gossip Imp (state 0 hover, 1 approach, 2 shouting, 3 recover)
    }
    public class Proj
    {
        public bool alive; public Vector2 pos, vel; public float dmg, life, explodeR, animT; public int pierce, bounce, lastHit = -1, forkN = 2; public bool fork, homing, ret53, emp53, bone53; public float homingTurn = 6, homingR = 5, bounceR = 4, forkDmg = 0.55f;
        public byte sin818; public int sinN818; /* t818u: 1 burning fork child, 2 ghost spear, 3 sun */ public SkillInst s; public SpriteRenderer sr; public Sprite[] frames; public HellFXRunner.Job trail; public int fx44 = -1, lg667 = -1; // t667u SkillLightFX flight glow
    }
    public class Pickup { public bool alive, magnet, bone; public Vector2 pos; public float speed, animT; public int value; public SpriteRenderer sr; public Sprite[] frames; public HellFXRunner.Job job; public System.Action<HellFXRunner.Job> onEnd; }
    struct Blast { public Vector2 pos; public float r, dmg; public int kind; public int skip; }   // kind 0 fire,1 overload,2 shatter,3 curse

    public partial class HellordGame : MonoBehaviour
    {
        [Header("Tuning (proposals)")]
        public float baseMaxHp = 100, baseSpeed = 4.5f, basePickup = 2.2f, baseCrit = 0.05f;
        public int maxEnemies = 500;
        public float eliteEvery = 120f, firstElite = 90f;

        // --- runtime state ---
        public enum Mode { StartPick, Play, LevelUp, Chest, Dead, Transition, Won, MainMenu, Altar } // v0.0.51 MainMenu/Altar
        public Mode mode = Mode.StartPick;
        public float hp, maxHp, incDmg, speedMul = 1, pickupMul = 1, armor, crit;
        public int level = 1, xp, kills, bones, blood, pendingLevels; // v0.7: blood = total blood (xp) picked up, for the HUD counter
        public float runTime; public bool autoAim; public string toast; float toastT;
        public readonly List<SkillInst> skills = new List<SkillInst>(3);

        Transform _hero, _camRig; Camera _cam; SpriteRenderer _heroBase; readonly List<(SpriteRenderer sr, Sprite[] f)> _heroLayers = new List<(SpriteRenderer, Sprite[])>();
        Sprite[] _heroBaseF, _heroFrostF; int _tf; bool _glassOn; float _heroAnim, _hurtT, _iframe; Vector2 _heroPos, _lastDir = Vector2.right, _lastMouse; float _mouseT = -99;

        Enemy[] _en; Proj[] _pr; Pickup[] _pk; int _nextEnemy;
        readonly List<Blast> _blasts = new List<Blast>(64);
        readonly List<int> _q = new List<int>(256);
        float _spawnAcc, _nextElite; Vector2 _chestPos; bool _chestAlive; SpriteRenderer _chestSr;
        readonly List<SpriteRenderer> _orbs = new List<SpriteRenderer>(); float _orbAngle;
        HellFXRunner.Job _aura; float _auraR; bool _auraCur818;
        readonly Dictionary<EType, Sprite[]> _eSprites = new Dictionary<EType, Sprite[]>();

        // hash grid
        const int Buckets = 4096; const float Cell = 1.5f; int[] _head = new int[Buckets]; int[] _next;

        void Awake()
        {
            FireWallDefs948(); // t948u
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-runinbg") >= 0) { Application.runInBackground = true; Debug.Log("[Hellord] -runinbg: runInBackground = true"); } // v0.0.21
            // v0.0.17: '-volume <0..1>' command-line arg sets the master volume (stream-friendly playtests); no arg = full volume
            { var a = System.Environment.GetCommandLineArgs(); int vi = System.Array.IndexOf(a, "-volume");
              if (vi >= 0 && vi + 1 < a.Length && float.TryParse(a[vi + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float vol))
              { AudioListener.volume = Mathf.Clamp01(vol); Debug.Log($"[Hellord] -volume {AudioListener.volume:0.##}"); } }
            Application.targetFrameRate = 60;
            HFX.Setup();
            EnsureFX26(); // v0.0.26 pack 2.5.1 modules
            EnsureGlobalLight();
            BuildCamera(); BuildHero(); BuildPools();
            foreach (EType t in System.Enum.GetValues(typeof(EType))) _eSprites[t] = HArt.Enemy(t); // mapping: EnemyArtMap (HArt.cs)
            ResetRun();
            PactBoot51(); // v0.0.51 damnation pact: main menu on boot
        }

        static void EnsureGlobalLight()
        {
            // URP 2D: Sprite-Lit materials need a 2D light. Add a Global Light 2D via reflection (no URP assembly reference needed).
            System.Type t = null;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies()) { t = a.GetType("UnityEngine.Rendering.Universal.Light2D"); if (t != null) break; }
            if (t == null || FindAnyObjectByType(t) != null) return;
            var go = new GameObject("Global Light 2D (Hellord)"); var c = go.AddComponent(t);
            var p = t.GetProperty("lightType"); if (p != null && p.CanWrite) { try { p.SetValue(c, System.Enum.Parse(p.PropertyType, "Global")); } catch (System.Exception) { } }
        }

        void BuildCamera()
        {
            _camRig = new GameObject("CameraRig").transform;
            _cam = Camera.main;
            if (_cam == null) { var cg = new GameObject("Main Camera"); cg.tag = "MainCamera"; _cam = cg.AddComponent<Camera>(); }
            _cam.transform.SetParent(_camRig, false); _cam.transform.localPosition = new Vector3(0, 0, -10);
            _cam.orthographic = true; _cam.clearFlags = CameraClearFlags.SolidColor; _cam.backgroundColor = new Color32(14, 6, 9, 255);
            var sh = _cam.GetComponent<ScreenShake>(); if (sh == null) sh = _cam.gameObject.AddComponent<ScreenShake>(); sh.pixelsPerUnit = HArt.PPU;
            // v0.0.26: sprites sort by Y (HA: Sprite Sort Point = Pivot, Transparency Sort Axis = (0,1,0)). Our explicit sortingOrders already y-sort; this makes ties resolve the same way.
            _cam.transparencySortMode = TransparencySortMode.CustomAxis; _cam.transparencySortAxis = new Vector3(0, 1, 0);
            UnityEngine.Rendering.GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis; UnityEngine.Rendering.GraphicsSettings.transparencySortAxis = new Vector3(0, 1, 0);
            BuildDisplayCamera();
            EnsureOneAudioListener(); // v0.0.30: code-built cameras had no listener
            LoadRoom(LanesRoom); // v0.7: Ox Pit room replaces the tiled floor_style14 ground
        }
        /// <summary>v0.0.26: the gameplay camera renders into a low-res RenderTexture (targetTexture), and OnGUI upscales that texture through the CRT
        /// pass. With no camera targeting the screen, the Editor Game view reports "No cameras rendering" (the build never shows that overlay). This
        /// extra enabled camera (nothing in its culling mask, solid background) renders to the display every frame, which satisfies the Game view
        /// and gives the window a clean background; the CRT picture is still drawn over it in OnGUI.</summary>
        /// <summary>v0.0.30: exactly one AudioListener, on the gameplay camera (removes extras, adds one if missing).</summary>
        void EnsureOneAudioListener()
        {
            foreach (var l in FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (l.gameObject != _cam.gameObject) Destroy(l);
            if (_cam.GetComponent<AudioListener>() == null) _cam.gameObject.AddComponent<AudioListener>();
        }
        Camera _displayCam;
        void BuildDisplayCamera()
        {
            if (!HellordTuning.I.displayCamera || _displayCam != null) return;
            var go = new GameObject("Hellord Display Camera"); go.transform.SetParent(_camRig, false); go.transform.localPosition = new Vector3(0, 0, -10);
            _displayCam = go.AddComponent<Camera>(); _displayCam.orthographic = true; _displayCam.cullingMask = 0; _displayCam.clearFlags = CameraClearFlags.SolidColor;
            _displayCam.backgroundColor = new Color32(14, 6, 9, 255); _displayCam.depth = _cam.depth + 1; _displayCam.targetTexture = null; _displayCam.enabled = true;
            var ls = go.GetComponent<AudioListener>(); if (ls) Destroy(ls);
        }
        void BuildHero()
        {
            _hero = new GameObject("Hero").transform;
            _heroBaseF = HArt.Hero("hero_base"); _heroFrostF = HArt.Hero("hero_skin_frost");
            _heroOutline = NewSR("outline", _hero, null, HeroSort - 2); // 1px #FFDB9E @0.6 (built at runtime from the base layer)
            _heroBase = NewSR("base", _hero, _heroBaseF[0], HeroSort);
            _heroCenterT = new GameObject("HeroCenter").transform; _heroCenterT.SetParent(_hero, false); _heroCenterT.localPosition = new Vector3(0, 0.8f, 0);
        }
        // v0.6: hero drawn above FX and the horde (fixed high sorting order; walls 30000, pack FX below this).
        const int HeroSort = 32000; SpriteRenderer _heroOutline;
        void SetHeroLayers()
        {
            foreach (var l in _heroLayers) if (l.sr) { _ifOrig969.Remove(l.sr); if (_ifMats987.TryGetValue(l.sr, out var im)) { if (im) Destroy(im); _ifMats987.Remove(l.sr); } Destroy(l.sr.gameObject); } // t981u: drop the dead layers' i-frame materials
            _heroLayers.Clear();
            _glassOn = crit >= baseCrit + 0.1f;
            if (_tf > 0) return; // transformations replace the base and hide all upgrade layers
            if (Hero47On) { AddTraits47(); return; }
            void Add(string n) { var f = HArt.Hero(n); _heroLayers.Add((NewSR(n, _hero, f[0], 0), f)); }
            foreach (var s in skills)
                switch (s.def.id)
                {
                    case SkillId.Fireball: Add("hero_up_hellfire"); break;
                    case SkillId.FrostCleave: Add("hero_up_frost"); break;
                    case SkillId.StormRing: Add("hero_up_shock"); break;
                    case SkillId.BoneOrbit: Add("hero_up_bleed"); break;
                    case SkillId.CurseRune: Add("hero_up_curse"); break;
                }
            if (_glassOn) Add("hero_up_glass"); // crit horns after 2+ Crit stat picks
        }
        static SpriteRenderer NewSR(string n, Transform parent, Sprite s, int order)
        {
            var go = new GameObject(n); var sr = go.AddComponent<SpriteRenderer>(); go.transform.SetParent(parent, false); sr.sprite = s; sr.sortingOrder = order; return sr;
        }
        void BuildPools()
        {
            var root = new GameObject("Horde").transform;
            _en = new Enemy[maxEnemies]; _next = new int[maxEnemies];
            for (int i = 0; i < maxEnemies; i++)
            {
                var sr = NewSR("enemy", root, null, 0); HordeFX.Prepare(sr); sr.gameObject.SetActive(false);
                _en[i] = new Enemy { id = i, sr = sr, tr = sr.transform };
            }
            BuildBossSlot(root);
            var pr = new GameObject("Projectiles").transform; _pr = new Proj[400];
            for (int i = 0; i < _pr.Length; i++) { var sr = NewSR("proj", pr, null, 0); sr.gameObject.SetActive(false); _pr[i] = new Proj { sr = sr }; }
            var pk = new GameObject("Pickups").transform; _pk = new Pickup[700];
            for (int i = 0; i < _pk.Length; i++) { var sr = NewSR("pickup", pk, null, 0); sr.gameObject.SetActive(false); var pp = new Pickup { sr = sr }; pp.onEnd = j => OnPickupJobEnd(pp, j); _pk[i] = pp; }
            _chestSr = NewSR("chest", null, HArt.Strip("ui_chest", 3, 16, 16, new Vector2(8, 14))[2], 0); _chestSr.gameObject.SetActive(false); // v0.8: exported chest art (elite_closed frame)
            BuildFloorObjects();
        }

        public void ResetRun()
        {
            FX26Reset();
            foreach (var e in _en) { DetachFx(e); StopStatuses(e); e.alive = false; e.dying = false; e.sr.gameObject.SetActive(false); }
            foreach (var p in _pr) KillProj(p);
            ResetV36(); ResetV37(); ResetV44();
            foreach (var p in _pk) { var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j); p.alive = false; p.sr.gameObject.SetActive(false); }
            foreach (var o in _orbs) Destroy(o.gameObject); _orbs.Clear(); KillOrbit();
            if (_aura != null) { _aura.life = -1; _aura = null; }
            ResetBoss(); ClearHooks();
            _chestAlive = false; _chestSr.gameObject.SetActive(false); _blasts.Clear();
            skills.Clear(); SetHeroLayers();
            maxHp = hp = baseMaxHp; if (ArgStartHp > 0) hp = maxHp * ArgStartHp; incDmg = 0; speedMul = 1; pickupMul = 1; armor = 0; crit = baseCrit;
            level = 1; xp = 0; kills = 0; bones = 0; blood = 0; pendingLevels = 0; runTime = 0; _spawnAcc = 0; _nextElite = firstElite;
            _floorIdx = 0; PlanRun42(); ClearBanners(); _sbLastLevel = 0; _heroPos = HeroStart; BuildFloor(); // v0.8: new Ox Pit floor (loads the start room and places the hero)
            _hero.position = _heroPos; _iframe = 0; SnapCamera();
            mode = Mode.StartPick; BuildStartCards();
        }

        // ================= main loop =================
        /// <summary>v0.0.23 debug/clip only: '-starthp 0.12' starts each run at that HP fraction (to film the low-HP edge pulse). Off unless passed.</summary>
        static readonly float ArgStartHp = ParseStartHp();
        static float ParseStartHp() { var a = System.Environment.GetCommandLineArgs(); int i = System.Array.IndexOf(a, "-starthp"); return i >= 0 && i + 1 < a.Length && float.TryParse(a[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f) ? Mathf.Clamp01(f) : 0f; }
        void LateUpdate() { UpdateAtmos(); } // v0.0.22 hell atmosphere (HellordAtmos.cs), after the camera moved
        void Update()
        {
            if (toastT > 0) toastT -= Time.unscaledDeltaTime;
            UpdateBanners(Time.unscaledDeltaTime);
            PmTick(); PactTick51(); MenuAudioTick611(); // v0.0.61 t611u menu audio; v0.0.51 damnation pact (souls payout, menu keys)
            if (_rageBannerT > 0) _rageBannerT -= Time.unscaledDeltaTime; // v0.0.16: real time (hit-stop no longer freezes it), 3 s
            BenchTick(); BotStateTick(); HookPauseTick();
            if (HInput.Down(HKey.Tab)) { autoAim = !autoAim; Toast(autoAim ? "Aim: AUTO (nearest enemy)" : "Aim: MOUSE / STICK (hybrid)"); }
            UpdateCardKeys(); UpdateFX26();
            HellordAudio.BossTrack = BossTrack48(); /* v0.0.48 boss music */ HellordAudio.FloorTheme53 = !floorMode ? 0 : Mortar52 ? 3 : Mathf.Clamp(Theme, 0, 2); HellordAudio.FloorKey53 = floorMode ? _floorIdx : 0; // v0.0.53 per-floor music
            HellordAudio.TickHeart(maxHp > 0 && hp > 0 && hp / maxHp < HellordTuning.I.lowHpThreshold, mode == Mode.Play, mode == Mode.LevelUp || mode == Mode.Chest || Time.timeScale == 0 || _tmOpen); // v0.0.29 heartbeat follows the low-HP pulse
            if (TestMenuUpdate()) { UpdateCamera(); return; } // v0.0.38: test menu open = game paused
            if (mode == Mode.Transition) { UpdateTransition(); UpdateCamera(); return; }
            if (mode == Mode.Won && HInput.Down(HKey.R)) { HellordAudio.Play("restart"); /* v0.0.61 */ ResetRun(); PactAfterReset51(); return; }
            if (mode != Mode.Play) { UpdateCamera(); return; }
            if (CtrlBlock716()) { UpdateCamera(); return; }
            if (SinSwapTick737()) { UpdateCamera(); return; } // t770u: sin swap card pauses the run // t716u: Controls page / level-up slot pick pause the run
            // v0.0.38: F1 now opens the TEST MENU (PLAYER > +1 level replaces the old F1 level-up)
            if (HInput.Down(HKey.F2)) SpawnEnemy(EType.Elite);
            if (HInput.Down(HKey.F4)) FloorTeleportBoss(); // v0.8: teleport to the boss room (starts the fight there)
            if (HInput.Down(HKey.F5)) { TellFX.JuiceHitStop = !TellFX.JuiceHitStop; Toast("Juice hit-stop: " + (TellFX.JuiceHitStop ? "ON" : "OFF")); }
            if (HInput.Down(HKey.F6)) { TellFX.StunWobble = !TellFX.StunWobble; Toast("Stun wobble: " + (TellFX.StunWobble ? "ON" : "OFF")); }
            if (HInput.Down(HKey.F7)) { TellFX.ImpactFrames = !TellFX.ImpactFrames; Toast("Impact frames: " + (TellFX.ImpactFrames ? "ON" : "OFF")); }
            if (HInput.Down(HKey.F8)) FloorClearRoom(); // v0.8: was 'swap room'; now clears the current room's goal
            if (HInput.Down(HKey.F10)) FloorRevealMap();
            if (Free716("N") && HInput.Down(HKey.SkillCycle)) DebugGrantSkill(); // v0.0.14: Shift+0 grants/levels the next skill (test visuals)
            if (HInput.Down(HKey.F9)) showDebug = !showDebug;
            if (Free716("G") && HInput.Down(HKey.Glow)) { int gi = System.Array.FindIndex(GlowSteps, g => Mathf.Abs(g - crtGlow) < 0.01f); crtGlow = GlowSteps[(gi + 1) % GlowSteps.Length]; glowLabelT = 2f; Debug.Log("[Hellord] crt glow " + crtGlow); } // v0.0.21: G / typed g
            if (Free716("J") && HInput.Down(HKey.HookThrower)) Toast(SpawnHookThrowerNearHero() != null ? "HOOK THROWER (debug, PLACEHOLDER art)" : "no free enemy slot"); // v0.0.24: J / typed j
            if (Free716("I") && HInput.Down(HKey.GossipImp)) Toast(SpawnGossipImpNearHero() != null ? "GOSSIP IMP (debug, PLACEHOLDER art)" : "no free enemy slot"); // v0.0.25: I / typed i
            if (Free716("T") && HInput.Down(HKey.Traps)) Toast("TONGUE TRAPS (debug, placeholder art): " + SpawnTongueTraps()); // v0.0.21: T / typed t
            if (Free716("K") && HInput.Down(HKey.K)) { showKills = !showKills; Toast("Kill counter: " + (showKills ? "ON" : "OFF")); }
            if (HInput.Down(HKey.F3)) { _tf = (_tf + 1) % 4; SetHeroLayers(); Toast(_tf == 0 ? "Transformation: none" : "Transformation (debug): " + TfSheets[_tf]); }
            float dt = Mathf.Min(Time.deltaTime, 1f / 20f); if (dt <= 0) { UpdateCamera(); return; }
            runTime += dt; if (hp > maxHp) hp = maxHp; // v0.0.14 safety clamp
            Z37Begin(); UpdateHero(dt); if (floorMode) UpdateFloor(dt); else { UpdateSpawner(dt); UpdateBossSpawn(); } RebuildGrid(); UpdateEnemies(dt); UpdateSkills(dt); PactTick53(dt); UpdateProjectiles(dt); UpdateIce(dt); UpdateOrbLight667(); UpdateBBullets(dt); Update49(dt); UpdateTongueTraps(dt); UpdateHooks(dt); UpdateGossip(dt); ProcessBlasts(); UpdatePickups(dt);
            if (_room != null) _room.Tick(dt); if (_lane != null) _lane.Tick(dt); Z37End();
            UpdateCamera();
            if (pendingLevels > 0 && mode == Mode.Play) OpenLevelUp();
        }

        /// <summary>v0.0.9: one integer zoom for every room, picked from the screen so the view is never bigger than a normal
        /// 768x432 room (was floor(min(W/640,H/360)): a big browser canvas got zoom 1-2 with a view larger than the small rooms,
        /// so they sat static in the middle while the 1536x864 boss room scrolled and looked zoomed in). 1280x720 / 1920x1080 -> 640x360.</summary>
        public static int CamZoom() { var T = HellordTuning.I; Vector2 tp = T.combatV49 ? T.camTargetPx49 : new Vector2(768f, 432f); return Mathf.Max(1, Mathf.Max(Mathf.CeilToInt(Screen.width / Mathf.Max(64f, tp.x)), Mathf.CeilToInt(Screen.height / Mathf.Max(36f, tp.y)))); } // v0.0.49 camTargetPx49
        public static bool AtFullRoomCam1023; // t1023u: autotest full-room overview (fit WorldRect)
        bool _atSavedLowRes1023;
        void UpdateCamera()
        {
            if (AtFullRoomCam1023 && _room != null && _cam != null)
            {
                // keep low-res RT (game draws via OnGUI blit); size to window, ortho fits whole room
                int rw = Screen.width, rh = Screen.height;
                if (_rt == null || _rt.width != rw || _rt.height != rh)
                {
                    if (_rt != null) { _cam.targetTexture = null; _rt.Release(); Destroy(_rt); }
                    _rt = new RenderTexture(rw, rh, 24) { filterMode = FilterMode.Point, name = "HellordFullRoom1023" }; _rt.Create(); _cam.targetTexture = _rt;
                }
                _rtZoom = 1;
                Rect wr = _room.WorldRect;
                float needH = wr.height * 0.5f + 0.75f;
                float needW = (wr.width * 0.5f + 0.75f) / Mathf.Max(0.01f, _cam.aspect);
                _cam.orthographicSize = Mathf.Max(needH, needW);
                _camRig.position = new Vector3(Snap(wr.center.x), Snap(wr.center.y), 0);
                return;
            }
            int zoom = CamZoom(); CamScroll636();
            _cam.orthographicSize = Screen.height / (2f * HArt.PPU * zoom);
            UpdateLowRes(zoom); HeroScaleWatch988();
            Vector3 target = CamTarget(CamFocus());
            var p = Vector3.Lerp(_camRig.position, target, 1 - Mathf.Exp(-10 * Time.unscaledDeltaTime));
            p = CamTarget(p); // v0.7: never show outside the room while lerping
            _camRig.position = new Vector3(Snap(p.x), Snap(p.y), 0);
            if (BossAlive && mode == Mode.Play) { bool off = BossOffView(); _bossOffT = off ? _bossOffT + Time.unscaledDeltaTime : 0; } else _bossOffT = 0;
        }
        /// <summary>v0.0.9: during the boss fight the camera leans 45% toward the boss, but the hero always stays 40 px inside the view.</summary>
        /// <summary>v0.0.16: ONE off-screen test shared by the banner (_bossOffT) and the arrow (was 0..1 vs 0.02..0.98 -> banner without arrow).</summary>
        public bool BossOffView() { var v = _cam.WorldToViewportPoint(_boss.pos + Vector2.up * 1.5f); return v.x < 0.02f || v.x > 0.98f || v.y < 0.02f || v.y > 0.98f; }
        // v0.0.16 boss camera: the raw hero/boss midpoint swung with every boss charge and flipped to hero-clamped
        // framing whenever the pair stopped fitting (seen as the view 'alternating'). Now: the framed point is the boss BODY
        // (feet..head), the focus is low-pass filtered (time constant ~0.45 s) with a 0.6 u dead-zone, and only the hero
        // margin is a hard constraint (applied after smoothing, so the hero can never leave the view).
        Vector2 _camF; bool _camFInit; public float camFocusJump; // harness: largest per-frame focus step
        Vector2 CamWant(Vector2 h, float hh, float hw)
        {
            float u = HudPxToWorld(), top = (20 + HArt.HpBarH + 34) * u;            // boss bar + name band at the top of the HUD
            Vector2 bFeet = _boss.pos, bHead = _boss.pos + Vector2.up * (BossHeadY + 0.4f);
            Vector2 f = Vector2.Lerp(h + Vector2.up * 0.8f, (bFeet + bHead) * 0.5f, 0.5f);
            // keep the boss's head below the boss bar when the pair fits vertically
            f.y = Mathf.Max(f.y, bHead.y - (hh - top));
            return f;
        }
        Vector3 CamFocus() => CamFocusStep(Time.unscaledDeltaTime, false);
        Vector3 CamFocusStep(float dt, bool snap)
        {
            Vector2 h = _hero.position; if (!BossAlive || _cam == null) { _camFInit = false; return h; }
            float hh = _cam.orthographicSize, hw = hh * _cam.aspect, mx = CamMarginX(), my = CamMarginY();
            Vector2 want = CamWant(h, hh, hw), prev = _camF;
            if (snap || !_camFInit) { _camF = want; _camFInit = true; }
            else
            {
                Vector2 d = want - _camF; float m = d.magnitude, dead = 0.6f;
                if (m > dead) _camF += d * (1 - dead / m) * (1 - Mathf.Exp(-dt / 0.45f));
            }
            Vector2 f = _camF;
            // the boss body point stays inside the view (soft framing lag never hides a boss that fits)
            Vector2 bb = _boss.pos + Vector2.up * 1.5f; float bm = 1.6f;
            f.x = Mathf.Clamp(f.x, bb.x - (hw - bm), bb.x + (hw - bm)); f.y = Mathf.Clamp(f.y, bb.y - (hh - bm), bb.y + (hh - bm));
            f = HeroSafeClamp(f, h, hw, hh);
            _camF = f; // the hard hero clamp also moves the filter state (no snap-back)
            if (!snap) camFocusJump = Mathf.Max(camFocusJump, (f - prev).magnitude);
            return f;
        }
        /// <summary>v0.0.17: hero priority - the hero stays inside the HUD-safe part of the view (clear of the boss bar band, the
        /// debug/skill column when shown, and the screen edge margins). Applied last, after boss framing and room clamping.</summary>
        Vector2 HeroSafeClamp(Vector2 f, Vector2 h, float hw, float hh)
        {
            float mx = CamMarginX(), my = CamMarginY(), u = HudPxToWorld();
            float left = mx, right = mx, bottom = _bossEntryT > 0 && BossAlive ? 1.4f : Mathf.Max(2.5f, hh * 0.22f), /* v0.0.17: only the small counter strip is at the bottom */ top = Mathf.Max(my, (20 + HArt.HpBarH + 14) * u + 1.6f);
            if (showDebug) left = Mathf.Max(mx, hw * 2 * 0.17f + 1.0f); // debug text + skill slots column (150/960 of the width)
            f.x = Mathf.Clamp(f.x, h.x - (hw - right), h.x + (hw - left));
            f.y = Mathf.Clamp(f.y, h.y + 1.3f + top - hh, h.y + (hh - bottom)); // v0.0.18: top/bottom were swapped (hero head below the HUD band, feet above the bottom strip)
            return f;
        }
        public bool HeroInSafeView()
        {
            if (_cam == null || _hero == null) return true; Vector2 c = _camRig.position, h = _hero.position;
            float hh = _cam.orthographicSize, hw = hh * _cam.aspect; var g = HeroSafeClamp(c, h, hw, hh);
            return (g - c).sqrMagnitude < 0.6f * 0.6f;
        }
        float CamMarginX() => Mathf.Max(2.5f, _cam.orthographicSize * _cam.aspect * 0.22f);
        float CamMarginY() => Mathf.Max(2.5f, _cam.orthographicSize * 0.3f);
        /// <summary>v0.0.15: HUD blocks in 640x360 canvas px (HP/XP top-left, minimap/goal top-right, counters bottom-left).</summary>
        static readonly Rect[] _hudBlocks = { new Rect(0, 0, 124, 34), new Rect(500, 0, 140, 46), new Rect(0, 342, 132, 18) };
        static readonly Rect[] _hudBlocks45 = { new Rect(0, 0, 124, 34), new Rect(500, 0, 140, 46), new Rect(0, 30, 132, 18) };
        Rect[] HudBlocks()
        {
            if (!HellordTuning.I.hudCornerV45) return _hudBlocks;
            float ms = Mathf.Clamp(HellordTuning.I.hudMinimapScale, 1f, 3f); _hudBlocks45[1] = new Rect(640 - 140 * ms, 0, 140 * ms, 46 * ms); return _hudBlocks45;
        } /* v0.0.45: counters moved bottom-right */
        /// <summary>World units per HUD canvas pixel.</summary>
        float HudPxToWorld() { float z = Mathf.Max(1f, Mathf.Min(Mathf.Floor(Screen.width / 640f), Mathf.Floor(Screen.height / 360f))); return z / (HArt.PPU * CamZoom()); }
        void SnapCamera() { if (_camRig && _hero) { var p = CamTarget(CamFocusStep(0, true)); _camRig.position = new Vector3(Snap(p.x), Snap(p.y), 0); } }
        /// <summary>v0.7 camera rule: an axis where the room is smaller than (or equal to) the view stays fixed on the room centre;
        /// otherwise follow, clamped so the view never leaves the room. Uses the real view (integer zoom can show more than 640x360).</summary>
        Vector3 CamTarget(Vector3 want)
        {
            if (_room == null || _cam == null) return want;
            // t1022u: after Ox Pit real scale, WorldRect IS the fight room — skip nested arena cam clamp (felt like sticky/slow camera)
            Rect r = (_bossArenaOn1006 && BossAlive && HellordRoom.BossSX677 >= 0.999f) ? BossArenaCamRect1006() : _room.WorldRect; float hh = _cam.orthographicSize, hw = hh * _cam.aspect;
            // v0.0.15: the view may overscroll past the room edge by the HUD band so a hero hugging a wall/corner is never under the HUD
            float u = HudPxToWorld(), padT = 40 * u, padB = 20 * u, padX = 10 * u;
            float x = r.width <= hw * 2 ? r.center.x : Mathf.Clamp(want.x, r.xMin + hw - padX, r.xMax - hw + padX);
            float y = r.height <= hh * 2 ? r.center.y : Mathf.Clamp(want.y, r.yMin + hh - padB, r.yMax - hh + padT);
            if (BossAlive && _hero) { var g = HeroSafeClamp(new Vector2(x, y), _hero.position, hw, hh); x = g.x; y = g.y; } // v0.0.17: hero beats room clamp
            return new Vector3(x, y, 0);
        }

        // ---- v0.7 rooms ----
        const string LanesRoom = "oxpit_room_lanes", ClipRoom = "v26_boss";
        static readonly Vector2 HeroStartPx = new Vector2(768, 640); // lanes room: south of the altar ring, clear of props
        Vector2 HeroStart => HellordRoom.Px(HeroStartPx.x, HeroStartPx.y);
        HellordRoom _room; public bool showKills, showDebug;
        void LoadRoom(string name)
        {
            { var TR = HellordTuning.I; var rp = floorMode && TR.roomRecolor ? V37Recolor.RampFor(Theme) : null; HellordRoom.Ramp = rp; HellordRoom.RampKey = rp == null ? "" : "|f" + Theme; HellordRoom.RampGain = HData.At(TR.roomRecolorGain, Theme + 1, 1.5f); /* v0.0.42: palette = theme */ HellordRoom.PropGain = TR.propRecolorGain; } // v0.0.37
            HellordRoom.FrozenSeed = floorMode && Theme == 1 && HellordTuning.I.frozenTilesB && FrozenTilesB.Available ? Mathf.Abs(curRoom * 7919 + FloorIdx * 104729 + name.Length * 31 + 17) : -1;
            HellordRoom.LowWallsOn = HellordTuning.I.lowWallsV30 && LowWalls.Available(floorMode ? Theme : 0);
            HellordRoom.TileTheme = Theme == 2 && HellordTuning.I.burningTilesG && BurningTilesG.Tiles.Available ? 2 : 1;
            LowWalls.Mortar52 = Mortar52; if (LowWalls.Mortar52) { HellordRoom.TileTheme = 3; HellordRoom.LowWallsOn = HellordTuning.I.lowWallsV30 && LowWalls.Available(0); } // v0.0.52 torture floors: Mortar B skin
            if (floorMode && HellordRoom.TileTheme >= 2) HellordRoom.FrozenSeed = Mathf.Abs(curRoom * 7919 + FloorIdx * 104729 + name.Length * 31 + 17);
            if (name.StartsWith("rooms_shrine") && HellordTuning.I.shrineBakedBase53) HellordRoom.FrozenSeed = -1; // pt483u: the altar statue is baked into rooms_shrine_*_base; the composed theme floor dropped it
            var r = HellordRoom.Load(name); if (r == null) return;
            if (_room != null) _room.Destroy(); _room = r; _gateGlow = null; // glow lived under the old room root
            if (_hero) { ClampToRoom(ref _heroPos, HeroMarginPx); if (_hero) _hero.position = _heroPos; }
            if (_en != null) foreach (var e in _en) if (e.alive && !e.boss) ClampToRoom(ref e.pos, 6);
            SnapCamera();
        }
        const float HeroMarginPx = 6;
        /// <summary>Walls block movement: keep feet inside the walkable floor (24px side walls, 48px north wall, 16px south row).</summary>
        void ClampToRoom(ref Vector2 p, float marginPx)
        {
            if (_room == null) return; _room.Walkable(marginPx, out var lo, out var hi);
            p = new Vector2(Mathf.Clamp(p.x, lo.x, hi.x), Mathf.Clamp(p.y, lo.y, hi.y));
            PushOutBlocks42(ref p, marginPx); // v0.0.42 room shape pits
        }
        /// <summary>Sorting for y-sorted actors: same spread as v0.6 (room y 0..864px maps to roughly -432..432).</summary>
        static int YSort(float y) => -(int)(y * 16) - 432;
        public const int RefW = 640, RefH = 360;
        static float Snap(float v) => Mathf.Round(v * HArt.PPU) / HArt.PPU;

        // ================= hero =================
        void UpdateHero(float dt)
        {
            var mv = Manual716 ? Move716(dt) : HInput.Move(); // t716u
            Vector2 heroP0 = _heroPos; Dash49(mv, dt); _heroPos += HeroVel37(mv, baseSpeed * speedMul * HookMoveMul() * HeroSlowMul37() * HeroChillMul44() * HoldMul49(), dt) * dt; // v0.0.24: slowed while a hook drags you; v0.0.37: ice + frost slow
            if (mv.sqrMagnitude > 0.01f) _lastDir = mv.normalized;
            ClampToRoom(ref _heroPos, HeroMarginPx); ClampToArena(ref _heroPos); BlockShrineStatue(ref _heroPos); BlockSpikes46(ref _heroPos); HeroVelClamp37(heroP0, dt);
            _hero.position = new Vector3(Snap(_heroPos.x), Snap(_heroPos.y), 0);
            bool moving = mv.sqrMagnitude > 0.01f; _heroAnim += dt;
            int f = moving ? HArt.HeroWalk0 + (int)(_heroAnim * HArt.HeroWalkFps) % HArt.HeroWalkN : HArt.HeroIdle0 + (int)(_heroAnim * HArt.HeroIdleFps) % HArt.HeroIdleN;
            if ((crit >= baseCrit + 0.1f) != _glassOn || Hero47On != _layers47) { _layers47 = Hero47On; SetHeroLayers(); }
            if (HoldFrame47) f = 0;
            Sprite[] baseF = _tf > 0 ? HArt.Hero(TfSheets[_tf]) : Hero47On ? Hero36Frames() : _heroBaseF; // v0.0.15: no frost-blue skin swap when Frost Cleave is owned (hero stays the red final2 demon)
            _heroBase.sprite = baseF[f];
            bool flip = AimDir(false).x < 0;
            int ord = HeroSort;
            _heroBase.flipX = flip; _heroBase.sortingOrder = ord;
            var ol = Hero47On ? HArt.HeroOutline(Art47 + "hero_36_base", T47.hero36CellPx, T47.hero36CellPx, T47.hero36PivotPx.x, T47.hero36PivotPx.y) : HArt.HeroOutline(_tf > 0 ? TfSheets[_tf] : baseF == _heroFrostF ? "hero_skin_frost" : "hero_base");
            _heroOutline.enabled = ol != null; if (ol != null) { _heroOutline.sprite = ol[f]; _heroOutline.flipX = flip; _heroOutline.sortingOrder = ord - 2; }
            foreach (var l in _heroLayers) { l.sr.sprite = l.f[f]; l.sr.flipX = flip; l.sr.sortingOrder = ord + LayerSort47(l.sr.name); }
            if (_room != null) { _room.SortAgainstHero(_heroPos, HeroSort, HellordTuning.I.heroBehindProps); UpdateRoomGrade(); }
            _iframe -= dt; _hurtT -= dt;
            _heroBase.color = _hurtT > 0 && !HellordTuning.I.fxHeroHurt ? new Color(1, 0.4f, 0.4f) : _heroSlowT > 0 ? new Color(0.62f, 0.85f, 1f) : Color.white; // v0.0.26: HurtFX replaces the whole-sprite red tint
            UpdateChill47(dt, f, flip, ord); UpdateIFrameFX969(); Bs950Frame();
            var m = HInput.MouseScreen(); if ((m - _lastMouse).sqrMagnitude > 4) { _mouseT = Time.unscaledTime; _lastMouse = m; }
        }

        /// <summary>Hybrid aim: mouse/stick while the player is aiming, otherwise nearest enemy, otherwise move direction.</summary>
        Vector2 AimDir(bool preferEnemy = true) { var d = AimDirRaw(preferEnemy); return preferEnemy ? ScrambleAim(d) : d; } // v0.0.25: Gossip Imp scramble bends attack aim only (not hero facing)
        Vector2 AimDirRaw(bool preferEnemy)
        {
            if (CurAim720 && mode == Mode.Play) { var dm = MouseWorld() - HeroCenter; return dm.sqrMagnitude > 0.01f ? dm.normalized : _lastDir; } // t716u: Manual = always the cursor
            if (!autoAim)
            {
                var st = HInput.AimStick(); if (st.sqrMagnitude > 0.09f) return st.normalized;
                if (Time.unscaledTime - _mouseT < 1.5f || HInput.MouseHeld()) { var d = MouseWorld() - _heroPos; if (d.sqrMagnitude > 0.01f) return d.normalized; }
            }
            if (preferEnemy) { int n = Nearest(_heroPos, 9f, -1); if (n >= 0) return (_en[n].pos + Vector2.up * 0.5f - _heroPos - Vector2.up * 0.8f).normalized; }
            return _lastDir;
        }
        bool Aiming() => CurAim720 || !autoAim && (HInput.AimStick().sqrMagnitude > 0.09f || Time.unscaledTime - _mouseT < 1.5f || HInput.MouseHeld());
        internal Vector2? atMouse986; // t979u autotest: fake cursor (world)
        Vector2 MouseWorld() { if (atMouse986.HasValue) return atMouse986.Value; var m = HInput.MouseScreen(); var w = _cam.ViewportToWorldPoint(new Vector3(m.x / ViewPxW, m.y / ViewPxH, 10)); return w; }
        // ---- v0.0.15 perf: render the world at 1 texel per art pixel (Screen/zoom) and point-upscale it in OnGUI.
        // The camera zoom is an integer, so the picture is identical, but the GPU shades zoom^2 fewer pixels
        // (9x fewer at 1080p) - the big cost in software-rendered Chrome (SwiftShader / llvmpipe).
        public bool lowResRender = true; RenderTexture _rt; int _rtZoom = 1;
        float ViewPxW => _rt != null ? _rt.width * RtSX636 : Screen.width;
        float ViewPxH => _rt != null ? _rt.height * RtSY636 : Screen.height;
        // ===== t636u TEST: mouse-wheel camera view steps (incl. deliberately UNSAFE sizes) + on-screen CAM readout
        int _camStep636 = -1; // -1 = normal auto integer zoom (640x360 at 1080p since t642u) until VH scrolls
        bool CamTest636 => _camStep636 >= 0 && lowResRender && HellordTuning.I.camScrollZoom636;
        float RtSX636 => CamTest636 ? Screen.width / (float)_rt.width : _rtZoom;
        float RtSY636 => CamTest636 ? Screen.height / (float)_rt.height : _rtZoom;
        void CamScroll636()
        {
            var T = HellordTuning.I; if (!T.camScrollZoom636 || mode != Mode.Play || T.camZoomSteps636 == null || T.camZoomSteps636.Length == 0) return;
            var ms = UnityEngine.InputSystem.Mouse.current; float d = Wheel988(ms);
            if (ms != null && ms.middleButton.wasPressedThisFrame && _camStep636 >= 0) { _camStep636 = -1; Toast("VIEW ZOOM RESET (AUTO)"); return; } // t982u: middle-click = back to the normal view
            if (Mathf.Abs(d) < 0.01f) return;
            if (WheelBlocked988()) { wheelBlocked988++; return; } // t982u: the wheel no longer zooms the camera while picking a belt slot / on the Controls page / test menu
            int prevStep = _camStep636;
            if (_camStep636 < 0) { _camStep636 = 0; for (int i = 0; i < T.camZoomSteps636.Length; i++) if ((int)T.camZoomSteps636[i].x == 640 && (int)T.camZoomSteps636[i].y == 360) _camStep636 = i; }
            _camStep636 = Mathf.Clamp(_camStep636 + (d > 0 ? -1 : 1), 0, T.camZoomSteps636.Length - 1); // wheel up = zoom in (smaller view)
            if (_camStep636 != prevStep) { var st = T.camZoomSteps636[_camStep636]; Toast($"VIEW ZOOM {(int)st.x}x{(int)st.y} (MOUSE WHEEL) - MIDDLE-CLICK = RESET"); Debug.Log($"[Hellord] t982u view zoom by mouse wheel: step {prevStep} -> {_camStep636} ({(int)st.x}x{(int)st.y}) - hero and world drawn bigger/smaller"); }
        }
        GUIStyle _camSt636;
        void DrawCamHud636()
        {
            var T = HellordTuning.I; if (!T.camResHud636 || !T.camScrollZoom636 || mode != Mode.Play || _rt == null) return;
            int si = CamTest636 ? _camStep636 : -1; // t637u: label from the step list flag, never from the window
            if (si < 0 && T.camZoomSteps636 != null) for (int k = 0; k < T.camZoomSteps636.Length; k++) if ((int)T.camZoomSteps636[k].x == _rt.width && (int)T.camZoomSteps636[k].y == _rt.height) { si = k; break; }
            bool known = si >= 0 && T.camZoomSafe636 != null && si < T.camZoomSafe636.Length; bool ok = known && T.camZoomSafe636[si];
            string lab = known ? (ok ? "SAFE" : "UNSAFE") : "AUTO";
            string txt = $"CAM {_rt.width}x{_rt.height} {lab}{(CamTest636 ? "" : " (auto)")}   [wheel]  win {Screen.width}x{Screen.height}";
            if (_camSt636 == null) _camSt636 = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            _camSt636.normal.textColor = !known ? Color.white : ok ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.35f, 0.3f);
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity; var sz = _camSt636.CalcSize(new GUIContent(txt));
            GUI.Box(new Rect((Screen.width - sz.x) / 2, Screen.height - sz.y - 8, sz.x, sz.y), txt, _camSt636); GUI.matrix = m;
        }
        void UpdateLowRes(int zoom)
        {
            if (!lowResRender) { if (_rt != null) { _cam.targetTexture = null; _rt.Release(); Destroy(_rt); _rt = null; } return; }
            int rw = Mathf.CeilToInt(Screen.width / (float)zoom), rh = Mathf.CeilToInt(Screen.height / (float)zoom);
            if (CamTest636) { var st = HellordTuning.I.camZoomSteps636[Mathf.Min(_camStep636, HellordTuning.I.camZoomSteps636.Length - 1)]; rw = Mathf.Max(16, (int)st.x); rh = Mathf.Max(9, (int)st.y); } // t636u: exact test size, stretched to the window
            if (_rt == null || _rt.width != rw || _rt.height != rh)
            {
                if (_rt != null) { _cam.targetTexture = null; _rt.Release(); Destroy(_rt); }
                _rt = new RenderTexture(rw, rh, 24) { filterMode = FilterMode.Point, name = "HellordLowRes" }; _rt.Create(); _cam.targetTexture = _rt;
            }
            _rtZoom = zoom; _cam.orthographicSize = rh / (2f * HArt.PPU);
        }
        // v0.0.19 CRT: scanlines / RGB mask / curvature / vignette / phosphor glow, applied while upscaling the low-res world.
        // Pipeline-agnostic (Resources/HellordShaders/HellordCRT.shader), so no URP renderer feature is needed. '-nocrt' disables it.
        public static float crtGlow = ArgF("-crtglow", 0.3f); static readonly float[] GlowSteps = { 0f, 0.15f, 0.3f, 0.45f, 0.7f }; public float glowLabelT; // v0.0.20: bloom amount (HA preview = 0.7)
        public static float ArgFPublic(string k, float d) => ArgF(k, d);
        static float ArgF(string k, float d) { var a = System.Environment.GetCommandLineArgs(); int i = System.Array.IndexOf(a, k); float v; return i >= 0 && i + 1 < a.Length && float.TryParse(a[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) ? v : d; }
        public static bool crtEnabled = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-nocrt") < 0;
        static Material _crtMat; static bool _crtTried; static RenderTexture _glowA, _glowB;
        static bool CrtHiRes677 => Screen.height > HellordTuning.I.crtMaxHeight677; // t677u
        static void CrtRes677(Material m) { bool hi = CrtHiRes677; m.SetFloat("_Scan", hi ? 0 : 0.32f); m.SetFloat("_Mask", hi ? 0 : 0.12f); m.SetFloat("_Curve", hi ? 0 : 0.06f); m.SetFloat("_Chroma", hi ? 0 : 0.35f); }
        static Material CrtMat
        {
            get
            {
                if (!crtEnabled || !HellordTuning.I.crtEnabled677 || (CrtHiRes677 && HellordTuning.I.crtHiResMode677 == 0)) return null; if (_crtMat != null || _crtTried) return _crtMat; _crtTried = true;
                var sh = Resources.Load<Shader>("HellordShaders/HellordCRT");
                if (sh == null || !sh.isSupported) { Debug.LogWarning("[Hellord] CRT shader missing/unsupported -> plain upscale"); return null; }
                Debug.Log("[Hellord] CRT pass ON"); return _crtMat = new Material(sh) { name = "HellordCRT", hideFlags = HideFlags.DontSave };
            }
        }
        void DrawLowResWorld()
        {
            if (_rt == null || Event.current.type != EventType.Repaint) return;
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity; float w = _rt.width * RtSX636, h = _rt.height * RtSY636; // t636u: = zoom normally
            var r = new Rect(0, Screen.height - h, w, h); var crt = CrtMat;
            if (crt != null) { CrtRes677(crt); crt.SetVector("_SrcSize", new Vector4(_rt.width, _rt.height, 0, 0));
                // t991u: skip / half-res phosphor glow Blits (2 full-res ARGBHalf Blits were #2 FPS cost after the V2 light mask)
                bool doGlow = crtGlow > 0.001f && HellordTuning.I.crtGlowBlit991;
                if (doGlow) {
                    int gw = _rt.width, gh = _rt.height; if (HellordTuning.I.crtGlowHalf991) { gw = Mathf.Max(8, gw / 2); gh = Mathf.Max(8, gh / 2); }
                    if (_glowA == null || _glowA.width != gw || _glowA.height != gh) {
                        if (_glowA != null) { _glowA.Release(); _glowB.Release(); }
                        _glowA = new RenderTexture(gw, gh, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                        _glowB = new RenderTexture(gw, gh, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                    }
                    var prev = RenderTexture.active;
                    bool lin = QualitySettings.activeColorSpace == ColorSpace.Linear;
                    crt.SetVector("_BlurDir", new Vector4(1, 0, lin ? 1 : 0, 0)); Graphics.Blit(_rt, _glowA, crt, 1);
                    crt.SetVector("_BlurDir", new Vector4(0, 1, 0, 0)); Graphics.Blit(_glowA, _glowB, crt, 1);
                    RenderTexture.active = prev;
                    crt.SetTexture("_GlowTex", _glowB);
                } else { crt.SetTexture("_GlowTex", Texture2D.blackTexture); }
                crt.SetFloat("_GlowAmt", doGlow ? crtGlow : 0f); SetAtmosCrt(crt); Graphics.DrawTexture(r, _rt, crt, 0); }
            else { GUI.DrawTexture(r, _rt, ScaleMode.StretchToFill, false); DrawAtmosEdgeNoCrt(r); }
            DrawLight586(r); // t586u V2 light mask over the world, under the HUD
            GUI.matrix = m;
            DrawCamHud636(); // t636u
        }
        /// <summary>World point -> screen pixel (bottom-left origin) of the final upscaled picture.</summary>
        public Vector2 WorldToScreenPx(Vector2 w) { var v = _cam.WorldToViewportPoint(w); return new Vector2(v.x * ViewPxW, v.y * ViewPxH); }
        Vector2 HeroCenter => _heroPos + Vector2.up * 0.8f;
        static readonly string[] TfSheets = { null, "hero_tf_infernal", "hero_tf_lich", "hero_tf_bloodfiend" };

        // v0.0.16 fairness: 0.8 s i-frames (was 0.45 -> Elite 20 + Brutes 14 = ~44 DPS, 104 HP gone in ~2.3 s with no readable drop),
        // a non-boss hit never takes more than 20% max HP, every hit pops a number over the hero, and the last hits are logged.
        public static float HeroIFrame => HellordTuning.I.heroIFrame; public static float NonBossHitCap => HellordTuning.I.nonBossHitCap; public static float RoomEnterGrace => HellordTuning.I.roomEnterGrace; // v0.0.21: HellordTuning
        public float lastHitDmg, lastHitTime = -99; public string lastHitSrc = ""; public int heroHits;
        void HurtHero(float dmg, string src = "?", bool boss = false, bool contact = false)
        {
            if (HeroInvuln969) return; // v0.0.38: test-menu god mode
            if (contact && !boss && V49) dmg *= T49.enemyDmgMul49b; // v0.0.49b
            if (!boss && FloorIdx == 0 && Cur != null && Cur.type == RoomType.Survive) dmg *= HellordTuning.I.surviveF1DmgMul53; // v0.0.53 O1
            if (boss) dmg *= HellordTuning.I.bossDamageMul * (1 + HellordTuning.I.floorBossDmgGrowth * FloorIdx); // v0.0.26
            dmg *= SinHurtMul818(); /* t818u #24 */ dmg *= 100f / (100f + armor * 4f); if (!boss) dmg = Mathf.Min(dmg, maxHp * NonBossHitCap);
            dmg = PactHurt53(dmg, contact); if (dmg < 0) return; // v0.0.53 pact: steam miss / shield / frenzy / inferno
            hp -= dmg; _iframe = contact ? HeroIFrame / Mathf.Max(0.5f, HellordTuning.I.contactIFrameDiv) : HeroIFrame; _hurtT = 0.12f; // v0.0.27: demon touches land more often
            Mercy49(dmg); lastHitDmg = dmg; lastHitTime = runTime; lastHitSrc = src; heroHits++;
            HordeFX.Number(HeroCenter + Vector2.up * 1.2f, Mathf.Max(1, Mathf.RoundToInt(dmg)), true, 7771);
            if (showDebug) Debug.Log($"[Hurt] {src} -{dmg:0.#} hp={hp:0.#}/{maxHp:0} t={runTime:0.00}");
            ScreenShake.Add(2, 0.15f);
            if (hp <= 0 && PactCheatDeath51()) { } // v0.0.53 Last Rites sets hp itself (1 HP, Empower, invuln)
            if (hp <= 0) { hp = 0; mode = Mode.Dead; HitStop.Play(0.15f, 0.3f, 0.5f); HellordAudio.HeroDeath(); } else HellordAudio.Play("hero_hurt"); // v0.0.29
            FX26HeroHit(dmg); // v0.0.26 HurtFX
        }

        // ================= spawning =================
        void UpdateSpawner(float dt)
        {
            float minute = runTime / 60f;
            float rate = 1.2f + 1.6f * minute + 0.35f * minute * minute; // enemies / second
            _spawnAcc += rate * dt * (BossAlive ? 0.4f : 1f); // fewer fodder during the test boss
            while (_spawnAcc >= 1) { _spawnAcc -= 1; float r = Random.value; EType t = r < 0.55f - minute * 0.05f ? EType.Imp : r < 0.85f ? EType.Ghoul : r < 0.94f || minute < 0.5f ? EType.Brute : EType.Splitter; SpawnEnemy(t); }
            if (runTime >= _nextElite) { _nextElite += eliteEvery; SpawnEnemy(EType.Elite); Toast("An ELITE has appeared!"); }
        }
        void SpawnEnemy(EType t) => SpawnEnemyAt(t, null);
        /// <summary>v0.7: ring just outside the view, but only on walkable floor; if the ring is outside the room (view clamped at a wall),
        /// fall back to any floor point at least ~half a view away from the hero.</summary>
        Vector2 SpawnPoint(float rad)
        {
            Vector2 c = _camRig.position; Vector2 lo = c, hi = c; if (_room != null) _room.Walkable(8, out lo, out hi);
            float minD = HellordTuning.I.spawnMinHeroDistance, minD2 = minD * minD;
            for (int k = 0; k < 10; k++)
            {
                float a = Random.value * Mathf.PI * 2; var p = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rad;
                if (_room == null || (p.x >= lo.x && p.x <= hi.x && p.y >= lo.y && p.y <= hi.y && (p - _heroPos).sqrMagnitude >= minD2)) return p;
            }
            // v0.0.26 (small rooms are smaller than the view): any floor point at least minD from the hero, else the farthest of a few tries
            Vector2 best = new Vector2(Random.Range(lo.x, hi.x), Random.Range(lo.y, hi.y)); float bd = (best - _heroPos).sqrMagnitude;
            for (int k = 0; k < 14 && bd < minD2; k++) { var p = new Vector2(Random.Range(lo.x, hi.x), Random.Range(lo.y, hi.y)); float d = (p - _heroPos).sqrMagnitude; if (d > bd) { bd = d; best = p; } }
            return best;
        }
        Enemy SpawnEnemyAt(EType t, Vector2? at)
        {
            Enemy e = null;
            for (int k = 0; k < maxEnemies; k++) { var c = _en[(_nextEnemy + k) % maxEnemies]; if (c.id != BossSlot && !c.alive && !c.sr.gameObject.activeSelf) { e = c; _nextEnemy = (c.id + 1) % maxEnemies; break; } }
            if (e == null) return null;
            float half = _cam.orthographicSize * _cam.aspect, rad = Mathf.Sqrt(half * half + _cam.orthographicSize * _cam.orthographicSize) + 1.5f;
            e.pos = at ?? SpawnPoint(rad);
            var TT = HellordTuning.I; int fl = FloorIdx; float hpMul = (1 + TT.enemyHpPerMinute * (runTime / 60f)) * (1 + (TT.combatV49 ? TT.floorEnemyHpGrowth49i : TT.floorEnemyHpGrowth) * fl); // v0.0.49i; v0.0.26: BaseHP x (1 + g x minute) x (1 + floor growth x floor)
            HookCleanup(e); e.type = t; e.elite = t == EType.Elite; e.frames = _eSprites[t];
            switch (t)
            {
                // v0.6 radii / hurt heights retuned for the size-32 art (proposals): imp 38px, shambler 39px, brute 50px, wraith 41px, splitter 60x40, child 27x25
                case EType.Imp: e.maxHp = 9; e.speed = 3.0f; e.radius = 0.5f; e.hitY = 1.1f; e.dmg = 5; e.xp = 1; break;
                case EType.Ghoul: e.maxHp = 24; e.speed = 2.0f; e.radius = 0.45f; e.hitY = 1.15f; e.dmg = 8; e.xp = 2; break;
                case EType.Brute: e.maxHp = 70; e.speed = 1.3f; e.radius = 0.9f; e.hitY = 1.45f; e.dmg = 14; e.xp = 5; break;
                case EType.Splitter: e.maxHp = 40; e.speed = 1.2f; e.radius = 1.1f; e.hitY = 1.1f; e.dmg = 10; e.xp = 3; break;
                case EType.SplitChild: e.maxHp = 10; e.speed = 2.6f; e.radius = 0.55f; e.hitY = 0.7f; e.dmg = 5; e.xp = 1; break;
                case EType.HookThrower: InitHookThrower(e); break; // v0.0.24: numbers in HellordTuning "Hook Thrower"
                case EType.GossipImp: InitGossipImp(e); break; // v0.0.25: numbers in HellordTuning "Gossip Imp"
                default: e.maxHp = TT.eliteHp; /* v0.0.28: was 600 */ e.speed = 1.7f; e.radius = 0.7f; e.hitY = 1.3f; e.dmg = 12; /* v0.0.16: was 20 */ e.xp = 25; break;
            }
            e.dmg *= TT.contactDamageMul * (1 + (TT.soften679 ? TT.enemyDmgGrowth679 : TT.floorEnemyDmgGrowth) * fl); e.speed *= TT.enemySpeedMul * (1 + TT.floorSpeedGrowth * fl); // v0.0.26 pressure
            e.maxHp *= hpMul; e.hp = e.maxHp; e.alive = true; e.animT = Random.value * 4; e.dying = false; e.splitDone = false; e.nest = false; e.deathT = 0; e.walkRow = 0; e.variant = 0; e.m53 = 0; e.k49 = 0; e.s49 = 0; e.n49 = 0; e.t49 = 0; e.cd49 = 0.8f + Random.value * 1.5f; e.fuseT = -1; e.v44St = 0; e.v44T = 0; e.tr.rotation = Quaternion.identity;
            e.burnT = e.chillT = e.shockT = e.bleedT = e.curseT = e.freezeT = e.orbitCd = e.auraCd = e.slowT = e.stun53 = e.brit536 = 0; e.chillHits = 0; e.iceBuild = 0; e.push = Vector2.zero;
            e.sr.sprite = e.frames[0]; e.sr.color = Color.white; e.sr.flipX = false; e.sr.enabled = true; e.sr.gameObject.SetActive(true); HordeFX.Prepare(e.sr);
            StopStatuses(e); e.tr.localScale = Vector3.one * EnemyArtMap.Map[t].scale; e.tr.position = e.pos; e.hitT = 0;
            return e;
        }

        // ================= grid =================
        static int Bucket(int cx, int cy) => ((cx * 73856093) ^ (cy * 19349663)) & (Buckets - 1);
        void RebuildGrid()
        {
            for (int i = 0; i < Buckets; i++) _head[i] = -1;
            for (int i = 0; i < maxEnemies; i++)
            {
                var e = _en[i]; if (!e.alive) continue;
                e.cx = Mathf.FloorToInt(e.pos.x / Cell); e.cy = Mathf.FloorToInt(e.pos.y / Cell);
                int b = Bucket(e.cx, e.cy); _next[i] = _head[b]; _head[b] = i;
            }
        }
        /// <summary>Fills _q-like list with alive enemies whose body circle overlaps (p, r).</summary>
        void Query(Vector2 p, float r, List<int> outList)
        {
            outList.Clear(); float rr = r + 1.0f;
            int x0 = Mathf.FloorToInt((p.x - rr) / Cell), x1 = Mathf.FloorToInt((p.x + rr) / Cell), y0 = Mathf.FloorToInt((p.y - rr) / Cell), y1 = Mathf.FloorToInt((p.y + rr) / Cell);
            for (int cx = x0; cx <= x1; cx++) for (int cy = y0; cy <= y1; cy++)
                for (int i = _head[Bucket(cx, cy)]; i >= 0; i = _next[i])
                {
                    var e = _en[i]; if (!e.alive || e.cx != cx || e.cy != cy) continue;
                    float d = r + e.radius; if ((e.pos + Vector2.up * e.hitY - p).sqrMagnitude <= d * d) outList.Add(i);
                }
        }
        int Nearest(Vector2 p, float maxR, int skip)
        {
            int best = -1; float bd = maxR * maxR;
            for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (!e.alive || i == skip) continue; float d = (e.pos - p).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
            return best;
        }

        // ================= enemies =================
        void UpdateEnemies(float dt)
        {
            for (int i = 0; i < maxEnemies; i++)
            {
                var e = _en[i]; if (e.dying) { UpdateDeath(e, dt); continue; }
                if (!e.alive) { if (e.sr.enabled && !e.boss && e.id != BossSlot) { e.sr.enabled = false; e.sr.gameObject.SetActive(false); } continue; } // v0.0.9: sweep orphaned corpses (grey sprites after F8)
                e.burnT -= dt; e.chillT -= dt; e.shockT -= dt; e.bleedT -= dt; e.curseT -= dt; e.freezeT -= dt; e.orbitCd -= dt; e.auraCd -= dt; e.slowT -= dt; e.stun53 -= dt;
                // DoTs tick every 0.5s
                e.dotTick -= dt;
                if (e.dotTick <= 0) { e.dotTick = 0.5f; float d = e.bleedT > 0 ? e.bleedDps : 0; /* v0.0.21: burn damage comes from BurnFX.OnTick */ if (d > 0) { Damage(e, d * 0.5f, false, true); if (!e.alive) continue; } }
                if (e.boss) { e.hitT -= dt; UpdateBoss(e, dt); UpdateStatusFx(e); UpdateStatusHandles(e); continue; }
                if (e.nest) { UpdateNest(e, dt); continue; }
                Vector2 to = _heroPos - e.pos; float dist = to.magnitude;
                float sp = e.freezeT > 0 || e.stun53 > 0 ? 0 : e.speed * (e.chillT > 0 ? 0.65f : 1f) * (e.slowT > 0 ? 1f - e.slowAmt : 1f);
                Vector2 v = dist > 0.01f ? to / dist * sp : Vector2.zero;
                if (_nmin536.Count > 0 && e.type != EType.HookThrower && e.type != EType.GossipImp) { Vector2 g = EnemyGoal584(e.pos, dist) - e.pos; if (g.sqrMagnitude > 0.0001f && g != to) v = g.normalized * sp; } // t584u: demons chase a closer minion
                if (e.type == EType.HookThrower) v = HookThrowerMove(e, to, dist, sp);
                else if (e.type == EType.GossipImp) v = GossipImpMove(e, to, dist, sp);
                Color v37t = Color.white; if (e.stun53 > 0) { v = Vector2.zero; } else { if (e.variant == 2 && BomberThink37(e, ref v, dist, dt, out v37t)) continue; if (e.variant >= 3) Think44(e, ref v, to, dist, sp, dt, ref v37t); else if (V49) Think49(e, ref v, to, dist, sp, dt, ref v37t); } // v0.0.53 Calcify hard stun skips Think (no attacks) // v0.0.37 exploding demon
                // cheap separation against same-cell neighbours
                Vector2 sep = Vector2.zero; int n = 0;
                for (int j = _head[Bucket(e.cx, e.cy)]; j >= 0 && n < 6; j = _next[j])
                {
                    if (j == i) continue; var o = _en[j]; if (!o.alive) continue; Vector2 dd = e.pos - o.pos; float m2 = dd.sqrMagnitude, rr = e.radius + o.radius;
                    if (m2 < rr * rr && m2 > 1e-5f) { sep += dd / Mathf.Sqrt(m2) * (rr - Mathf.Sqrt(m2)); n++; }
                }
                e.pos += (v + e.push) * dt + sep * Mathf.Min(1, 8 * dt);
                e.push = Vector2.Lerp(e.push, Vector2.zero, 1 - Mathf.Exp(-8 * dt));
                ClampToRoom(ref e.pos, 6); if (_bossArenaOn1006) ClampToArena(ref e.pos); // v0.7 walls; t1006u arena
                // far away -> respawn closer (keeps density around the player)
                if (dist > (floorMode ? 60 : 30) && !e.elite) { StopStatuses(e); DetachFx(e); e.alive = false; e.sr.gameObject.SetActive(false); SpawnEnemy(e.type); continue; }
                if (e.stun53 <= 0 && dist < e.radius + 0.4f * HeroHitK47) { int hh37 = heroHits; _hurtBy53 = e; HurtHero(e.dmg, e.type.ToString(), false, true); if (e.variant == 1 && heroHits != hh37) ApplyFrostSlow37(HellordTuning.I.frostSlowS); } // v0.0.37 frost demon
                if (e.freezeT <= 0) e.animT += dt * (HArt.EnemyWalkFps * sp / Mathf.Max(0.5f, e.speed));
                e.hitT -= dt;
                if (v.sqrMagnitude > 0.01f) e.walkRow = HArt.WalkRow(v); // 4-direction walk rows (west is its own row, no flip)
                e.sr.sprite = e.frames[e.walkRow + (int)e.animT % HArt.EnemyWalkN]; e.sr.flipX = false; if (e.variant >= 3) Animate44(e, v, to, dt); if (e.m53 > 0) Animate53(e, v, dt); // hit flash = HordeFX material swap (no hit frame)
                var tint = EnemyArtMap.Map[e.type].tint * v37t;
                if (!HordeFXBusy(e)) SetCol(e.sr, tint * (e.fx != null ? (e.curseT > 0 ? new Color(0.8f, 0.6f, 1f) : Color.white) : e.curseT > 0 ? new Color(0.8f, 0.6f, 1f) : !IceFX3.Ok ? (e.freezeT > 0 ? new Color(0.6f, 0.9f, 1f) : e.chillT > 0 ? new Color(0.8f, 0.92f, 1f) : Color.white) : Color.white)); // t1409u: with the v3 ice shader the chill/freeze tint is done in-shader (no whole-sprite colour)
                UpdateStatusFx(e);
                if (e.bleedT > 0 && e.bleedH < 0 && e.dotTick > 0.45f) HFX.Bleed(e.pos + Vector2.up * e.hitY); // stand-in when BleedStatus failed
                UpdateStatusHandles(e);
                e.tr.position = new Vector3(Snap(e.pos.x), Snap(e.pos.y), 0); int so = YSort(e.pos.y); if (e.sr.sortingOrder != so) e.sr.sortingOrder = so;
            }
        }
        static void SetCol(SpriteRenderer r, Color c) { if (r.color != c) r.color = c; } // v0.0.9 perf: no renderer dirtying when unchanged
        static bool HordeFXBusy(Enemy e) => false; // flash uses a shared material swap, so tinting via color is compatible

        // ================= damage =================
        float SkillDamage(SkillInst s) => SkillDamage0(s) * PDmg49(); // v0.0.49 aggression x Intensity
        float SkillDamage0(SkillInst s) { var m = HellordTuning.I.skillDmgMul; int ix = (int)s.def.id; return s.def.baseDmg * HData.LevelMul(s.level) * (1 + incDmg) * s.More() * (m != null && ix >= 0 && ix < m.Length ? m[ix] : NewSkillDmgMul(ix)) * PactSkillMul51(s) * SinSkillMul737(s); } // v0.0.51 pact // v0.0.27: per-skill damage multiplier (all below 1)

        /// <summary>Hit with a skill: applies supports' statuses and reactions.</summary>
        void Hit(Enemy e, float dmg, SkillInst s, Vector2 knock, bool tick = false)
        {
            if (!e.alive) return; HitLight661(e, s, tick); // t661u elemental hit light
            dmg = SupportsBeforeHit(e, dmg, s, ref knock, tick); dmg = PactPreHit53(e, dmg, s, tick); dmg = SinPreHit818(e, dmg, s, tick); // t818u sins // v0.0.53 pact // v0.0.36 Executioner / Knockback / Life Leech
            bool burnNow = e.burnT > 0, shockNow = e.shockT > 0;
            // v0.0.27: status strength comes from the support's LEVEL (was a flat 1.5 for a matching innate support, 1 for the others)
            ApplyStatus(e, s.def.innate, dmg, s.sup.Contains(SupportIdFor(s.def.innate)) ? StatusK(s, SupportIdFor(s.def.innate)) : 1f, s);
            if (s.def.innate != Status.Burn && s.Has(SupportId.Burn)) ApplyStatus(e, Status.Burn, dmg, StatusK(s, SupportId.Burn), s);
            if (s.def.innate != Status.Chill && s.Has(SupportId.Chill)) ApplyStatus(e, Status.Chill, dmg, StatusK(s, SupportId.Chill), s);
            if (s.def.innate != Status.Shock && s.Has(SupportId.Shock)) ApplyStatus(e, Status.Shock, dmg, StatusK(s, SupportId.Shock), s);
            if (s.def.innate != Status.Bleed && s.Has(SupportId.Bleed)) ApplyStatus(e, Status.Bleed, dmg, StatusK(s, SupportId.Bleed), s);
            // Reaction: Burn + Shock = Overload explosion
            if ((burnNow && e.shockT > 0 && !shockNow) || (shockNow && e.burnT > 0 && !burnNow) || (burnNow && shockNow && Random.value < 0.15f))
            {
                e.burnT = 0; e.shockT = 0; HFX.Stop(ref e.shockH);
                _blasts.Add(new Blast { pos = e.pos + Vector2.up * 0.5f, r = 1.8f, dmg = dmg * 2.5f + 10, kind = 1, skip = -1 });
            }
            if (!e.elite) e.push += knock;
            Damage(e, dmg, !tick, tick); PactPostHit53(e, dmg, s, tick); SinPostHit818(e, dmg, s, tick); // t818u sins // v0.0.53 pact
            if (!e.alive) OnSkillKill(e, s); // v0.0.36 Soul Harvest / Explode on Kill
        }
        static SupportId SupportIdFor(Status st) => st == Status.Burn ? SupportId.Burn : st == Status.Chill ? SupportId.Chill : st == Status.Shock ? SupportId.Shock : st == Status.Bleed ? SupportId.Bleed : SupportId.AddedDamage;
        float StatusK(SkillInst s, SupportId id)
        {
            var T = HellordTuning.I; int l = s.Lvl(id);
            switch (id) { case SupportId.Burn: return HData.At(T.supBurnMul, l, 1.2f); case SupportId.Bleed: return HData.At(T.supBleedMul, l, 1.2f); case SupportId.Chill: return HData.At(T.supChillTime, l, 1f); default: return 1f; }
        }
        void ApplyStatus(Enemy e, Status st, float dmg, float k, SkillInst src = null)
        {
            var TT = HellordTuning.I; if (st == Status.Burn && SinOn818(43)) st = Status.Chill; /* t818u #43 Endless Winter */ if (PactStatus53(e, ref st, ref dmg, ref k, src)) return; // v0.0.53 pact
            float sd = SinDuration737; // t818u Duration stat
            switch (st)
            {
                case Status.Burn: { float nb = dmg * HellordTuning.I.burnDpsFromHit * k; if (e.burnT <= 0) e.burnStk818 = 0; e.burnDps = SinBurnDps818(e, nb); e.burnT = (TT.burnDuration + (src != null && src.Has(SupportId.Burn) ? TT.burnExtraSecondsPerLevel * (src.Lvl(SupportId.Burn) - 1) : 0f)) * sd * SinBurnDurMul818() + SinBurnAddS818(); IgniteBurn(e); break; } // t818u #4/#31 + Duration
                case Status.Chill: e.chillT = 2 * k; if (++e.chillHits >= (src != null && src.Has(SupportId.Chill) ? HData.At(TT.supChillFreezeHits, src.Lvl(SupportId.Chill), 3) : 3)) { e.chillHits = 0; e.freezeT = 1.2f; } if (!StatusFX.IsAlive(e.chillH)) e.chillH = HFX.ChillStatus(e); StatusFX.SetFrozen(e.chillH, e.freezeT > 0); break;
                case Status.Shock:
                    e.shockT = 3 * sd * SinShockDur818(); e.shockAmp = (src != null && src.Has(SupportId.Shock) ? HData.At(TT.supShockAmp, src.Lvl(SupportId.Shock), 0.25f) : TT.shockBaseAmp) * SinShockAmp818(); // t818u #24 + Duration
                    if (HFX.Alive(e.shockH)) { HFX.Stop(ref e.shockH); }
                    e.shockH = HFX.ShockStatus(e.tr, Mathf.Min(e.shockT, HellordTuning.I.shockFxSec948), e.radius); Tint948(e, HellordTuning.I.shockTint948, HellordTuning.I.shockTintS948, new Color(0.6f, 0.85f, 1f)); // t948u snappier crackle + brief tint
                    if (e.shockH < 0) HFX.Shock(e.pos + Vector2.up * e.hitY); // stand-in
                    break;
                case Status.Bleed:
                    Tint948(e, HellordTuning.I.bleedTint948, HellordTuning.I.bleedTintS948, new Color(1f, 0.15f, 0.15f)); e.bleedT = 3 * sd; e.bleedDps = dmg * 0.25f * k * _sinBleedMul737 * SinBleedStill818(); // t818u #23 + Duration
                    if (!HFX.Alive(e.bleedH)) e.bleedH = HFX.BleedStatus(e.tr, 3f, e.radius);
                    break;
                case Status.Curse: e.curseT = 4 * k * sd; break; // t818u Duration
            }
        }
        void Damage(Enemy e, float dmg, bool canCrit, bool dot, bool noNumber = false)
        {
            if (!e.alive) return;
            if (e.shockT > 0) dmg *= 1f + e.shockAmp;
            if (e.boss && BossStunned) dmg *= 2f; // punish window
            bool c = canCrit && Random.value < crit; if (c) dmg *= 2f;
            e.hp -= dmg;
            if (!dot) e.hitT = 0.08f;
            int shown = Mathf.Max(1, Mathf.RoundToInt(dmg));
            // v0.5: every hit shows a number; HordeFX (pack v1.8.4) merges hits per target into one rising total, keeps them apart and on a plate.
            if (noNumber) { } // v0.0.21: BurnFX draws its own tick number
            else if (e.fx != null) HordeFX.Number(e.pos + Vector2.up * (e.boss ? BossHeadY : e.hitY * 1.8f), shown, c, e.sr.GetInstanceID()); // Uber material: flash via PixelFX
            else HordeFX.Hit(e.sr, shown, c, true);
            if (!dot) HitShards934(e.pos + Vector2.up * e.hitY, c); // t934u mode 19
            if (!dot) { HitJuice(e, dmg, c); HellordAudio.Play("enemy_hit"); } // v0.0.29 (rate-limited 0.04 s, cap 4)
            if (e.hp <= 0) KillEnemy(e);
        }
        static void StopStatuses(Enemy e) { HFX.Stop(ref e.shockH); HFX.Stop(ref e.bleedH); StopBurn(e, false); HFX.StopTell(ref e.chillH); }
        static void UpdateStatusHandles(Enemy e)
        {
            if (e.shockT <= 0 && e.shockH >= 0) HFX.Stop(ref e.shockH);
            if (e.bleedT <= 0 && e.bleedH >= 0) HFX.Stop(ref e.bleedH);
            if (e.burnT <= 0 && e.burnH >= 0) StopBurn(e, true);
            if (e.chillH >= 0) { if (e.chillT <= 0 && e.freezeT <= 0) HFX.StopTell(ref e.chillH); else StatusFX.SetFrozen(e.chillH, e.freezeT > 0); }
        }
        void KillEnemy(Enemy e)
        {
            e.alive = false; if (e.type == EType.HookThrower || e.type == EType.GossipImp) HookCleanup(e);
            EndMinionFx44(e); // v0.0.44
            if (e.nest) { OnNestKilled(e); return; }
            PactKill53(e); // v0.0.53 pact (reads statuses before they stop)
            if (mode != Mode.Dead && mode != Mode.Won) kills++; FloorKill(e); SinOnKill737(e); // v0.0.53 B14: no kills counted after death (HUD vs death screen mismatch)
            Vector2 c = e.pos + Vector2.up * e.hitY;
            if (e.freezeT > 0) _blasts.Add(new Blast { pos = c, r = 1.5f, dmg = (e.maxHp * 0.35f + 5) * SinShatterMul818(), kind = 2, skip = e.id }); // t818u #27
            if (e.curseT > 0) { _blasts.Add(new Blast { pos = c, r = 1.7f * SinCurseBlastR818(), dmg = (e.maxHp * 0.5f + 6) * SinCurseBlastDmg818(), kind = 3, skip = e.id }); SinCurseKill818(e, c); } // t818u #26 / #39
            SinKill818(e); // t818u #3 #14 #36
            DropPickup(e.pos, false, e.xp);
            if (Random.value < (e.type == EType.Brute ? 0.5f : 0.08f)) DropPickup(e.pos + Random.insideUnitCircle * 0.4f, true, 1);
            if (e.freezeT > 0) HFX.Shatter(c);
            DetachFx(e); StopStatuses(e);
            if (e.elite)
            {
                HitStop.Play(0.09f, 0.4f, 0.35f); ScreenShake.Add(5, 0.35f); HellFX.HellfireNova(c, 40);
                _chestAlive = true; _chestPos = e.pos; _chestSr.transform.position = _chestPos; _chestSr.gameObject.SetActive(true); _chestSr.sortingOrder = YSort(e.pos.y); HFX.Chest(_chestPos, false);
                for (int i = 0; i < 6; i++) DropPickup(e.pos + Random.insideUnitCircle * 1.2f, i % 2 == 0, 3);
            }
            if (e.elite) HellordAudio.Play("elite_death"); else if (!e.boss) HellordAudio.Play("enemy_death"); // v0.0.29
            if (e.boss) { _pactBossKills51++; _sinBossK818++; OnBossKilled(e); return; } // t818u #32 // v0.0.51 sigils
            // v0.6: play the sheet's 8-frame death (12fps), then hide (last frame is empty). Same burst + number obstacle as HordeFX.Kill.
            if (HellordTuning.I.fxDeathBurst934) DeathBurst934(e.pos + Vector2.up * e.hitY); else HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, new Color(0.55f, 0.06f, 0.08f), 5); HordeFX.AddNumberObstacle(e.pos, 4f);
            HordeFX.Prepare(e.sr); e.sr.color = EnemyArtMap.Map[e.type].tint; e.dying = true; e.deathT = 0; e.splitDone = false;
            e.sr.sprite = e.frames[HArt.EnemyDeath0];
        }
        void UpdateDeath(Enemy e, float dt)
        {
            if (e.boss) { UpdateBossDeath(e, dt); return; }
            if (e.variant >= 3 && Death44(e, dt)) return;
            if (e.m53 > 0 && Death53(e, dt)) return; // v0.0.53
            e.deathT += dt; int k = (int)(e.deathT * HArt.EnemyDeathFps);
            if (e.type == EType.Splitter && !e.splitDone && k >= HArt.SplitFrame)
            {   // Viet Hung's rule: 2 children at death frame index 4, -13px / +13px on X from the pivot
                e.splitDone = true;
                for (int s = -1; s <= 1; s += 2) { var c = SpawnEnemyAt(EType.SplitChild, e.pos + new Vector2(s * 13f / HArt.PPU, 0)); if (c != null) c.push = new Vector2(s * 1.5f, 0); }
            }
            if (k >= HArt.EnemyDeathN || e.deathT > 2f) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); return; }
            e.sr.sprite = e.frames[HArt.EnemyDeath0 + k];
        }
        readonly List<int> _blastQ = new List<int>(64); readonly List<Vector3> _blastPts = new List<Vector3>(4);
        void ProcessBlasts()
        {
            int guard = 0, cfx738 = 0;
            while (_blasts.Count > 0 && guard++ < 48)
            {
                var b = _blasts[_blasts.Count - 1]; _blasts.RemoveAt(_blasts.Count - 1);
                switch (b.kind)
                {
                    case 0: TortureBlast52(b.pos, b.r, 0); ProjectileFX.Impact(b.pos, ProjElement.Fire, Vector2.zero); HellordAudio.Play("fireball_impact"); break;
                    case 1: break; // overload: drawn below once targets are known
                    case 2: HordeFX.CheapBurst(b.pos, IceFX3.Ok ? IceArt.Cyan : new Color(0.7f, 0.95f, 1f), IceFX3.Ok ? 4 : 12); break; // shatter visual already played in KillEnemy (HFX.Shatter)
                    case 3: if (cfx738++ < HellordTuning.I.curseBlastFxPerFrame738) { ProjectileFX.Impact(b.pos, ProjElement.Curse, Vector2.zero); HordeFX.CheapBurst(b.pos, new Color(0.75f, 0.4f, 1f), 10); } break; // t738u cap visuals
                    case 4: HordeFX.CheapBurst(b.pos, new Color(1f, 0.55f, 0.15f), 14); break; // v0.0.36 Explode on Kill (no shake)
                }
                var list = _blastQ; Query(b.pos, b.r, list); if (b.kind != 0) SkillArea53(b.pos, b.r, b.dmg, 20 + Mathf.Clamp(b.kind, 0, 3)); // v0.0.53 every explosion hits decor
                if (b.kind == 1) { _blastPts.Clear(); foreach (int i in list) if (_blastPts.Count < 3 && i != b.skip) _blastPts.Add(_en[i].pos + Vector2.up * _en[i].hitY); HFX.Overload(b.pos, b.r, _blastPts); }
                foreach (int i in list) { if (i == b.skip) continue; var e = _en[i]; if (b.kind == 3) e.curseT = 0; Damage(e, b.dmg, false, false); }
            }
        }

        // ================= pickups =================
        void DropPickup(Vector2 pos, bool bone, int value, bool decay = true)
        {
            if (decay) { float dm = DropDecayMul(); if (dm < 0.999f) { value = StochRound(value * dm); if (value <= 0) return; } } // v0.0.42 shrinking drops
            HordeFX.AddNumberObstacle(pos, 3f); // v0.5: damage numbers lay out clear of fresh drops
            foreach (var p in _pk)
            {
                if (p.alive) continue;
                if (p.job != null) continue;
                bool skull = bone && value > 1; int tier = HArt.BloodTier(value);
                p.alive = true; p.bone = bone; p.value = value; p.pos = pos; p.magnet = false; p.speed = 0; p.animT = Random.value;
                p.frames = bone ? HArt.Bone(skull) : HArt.Blood(tier);
                // pooled HellFX pickup (bob + collect splat/chips) using OUR art; null when the 600-sprite budget is full
                p.job = HellFX.Pickup(p.frames[0], pos, bone ? (skull ? 21 : 20) : 10 + tier, 120f, 20);
                if (p.job != null) { p.job.end = p.onEnd; p.sr.gameObject.SetActive(false); return; }
                p.sr.sprite = p.frames[0];
                p.sr.transform.position = pos; p.sr.sortingOrder = YSort(pos.y) - 20; p.sr.gameObject.SetActive(true);
                return;
            }
        }
        void UpdatePickups(float dt)
        {
            float pr = basePickup * pickupMul;
            Vector2 hc = _heroPos + Vector2.up * 0.5f;
            foreach (var p in _pk)
            {
                if (!p.alive) continue;
                p.animT += dt * HArt.PickupFps;
                if (p.job != null)
                {
                    if (!p.magnet) { p.job.sr.sprite = p.frames[(int)p.animT & 3]; if (((Vector2)p.job.a - hc).sqrMagnitude < pr * pr) { p.magnet = true; HellFX.Magnet(p.job, _heroCenterT); } }
                    continue; // collection is reported by OnPickupJobEnd
                }
                p.sr.sprite = p.frames[(int)p.animT & 3];
                Vector2 d = hc - p.pos; float m = d.magnitude;
                if (!p.magnet && m < pr) { p.magnet = true; p.speed = -2f; }
                if (p.magnet) { p.speed += 30 * dt; p.pos += d / Mathf.Max(m, 0.001f) * p.speed * dt; p.sr.transform.position = p.pos; }
                if (m < 0.45f)
                {
                    p.alive = false; p.sr.gameObject.SetActive(false);
                    if (p.bone) { bones += p.value; SinBone818(p.value); } else AddXp(p.value);
                }
            }
            if (_chestAlive && (hc - _chestPos).magnitude < 1f) { _chestAlive = false; _chestSr.gameObject.SetActive(false); HFX.Chest(_chestPos, true); HellordAudio.Play("chest_open"); OpenChest(); }
        }
        void AddXp(int v) { v = SinXp737(v); blood += v; xp += v; while (xp >= HData.XpToNext(level)) { xp -= HData.XpToNext(level); GainLevel(); } }
        void GainLevel()
        {
            level++; pendingLevels++;
            var T = HellordTuning.I; maxHp += T.levelMaxHp; incDmg += T.levelIncDmg; speedMul += T.levelSpeed; hp = Mathf.Min(maxHp, hp + maxHp * T.levelHeal); // v0.0.27: smaller per-level gains (tuning)
            if (!HellordTuning.I.fxLevelUp) HellFX.LevelUpBurst(_hero.position); // v0.0.26: LevelUpFX plays its own beat when the screen opens
            int sn818 = level == T.slot2Level ? 2 : level == T.slot3Level ? 3 : level == T.slot4Level818 ? 4 : level == T.slot5Level818 ? 5 : 0; if (sn818 > 0 && sn818 <= T.skillSlotsMax818) Toast($"Skill slot {sn818} unlocked!");
        }
        int _dbgSkill;
        /// <summary>v0.0.14 debug (Shift+0 / F12): cycles Fireball, Frost Cleave, Storm Ring, Bone Orbit, Curse Rune. Owned -> +1 level;
        /// free slot -> add; all slots full -> replaces the last slot. Lets every skill's visual be checked on demand.</summary>
        void DebugGrantSkill()
        {
            // v0.0.15: always moves to the NEXT skill the hero does not own (old version spent presses levelling owned skills and
            // silently swapped slot 3, so Bone Orbit / Curse Rune looked like they never arrived). Full loadout -> slot 3 is replaced.
            var defs = HData.Skills; SkillDef d = null;
            for (int n = 0; n < defs.Length; n++) { var c = defs[_dbgSkill++ % defs.Length]; if (skills.FindIndex(s => s.def == c) < 0) { d = c; break; } }
            if (d == null) return;
            var inst = new SkillInst { def = d, level = 2 };
            if (skills.Count < HellordTuning.I.skillSlotsMax818) skills.Add(inst); else skills[skills.Count - 1] = inst;
            SetHeroLayers();
            var sb = new System.Text.StringBuilder("Debug skill: " + d.name + " Lv2 [");
            for (int i = 0; i < skills.Count; i++) sb.Append(i > 0 ? ", " : "").Append(skills[i].def.name);
            Toast(sb.Append("]").ToString());
        }
        // v0.0.15 perf bench (web only, inert unless the page URL contains "bench"): 60 imps + 3 skills, god mode, logs FPS every 5 s.
        // "benchfull" turns the low-res world render off for an A/B. Used to measure software-Chrome FPS; players never hit it.
        int _bench = -1; float _benchT; int _benchF;
        void BenchTick()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_bench < 0) { string u = Application.absoluteURL ?? ""; _bench = u.Contains("bench") ? 1 : 0; if (u.Contains("benchfull")) lowResRender = false; }
            if (_bench == 0) return;
            if (mode == Mode.StartPick || mode == Mode.LevelUp) { skills.Clear(); foreach (var id in new[] { SkillId.Fireball, SkillId.FrostCleave, SkillId.StormRing }) skills.Add(new SkillInst { def = HData.Skill(id), level = 3 }); SetHeroLayers(); pendingLevels = 0; mode = Mode.Play; _mouseT = -99; autoAim = true; }
            if (mode != Mode.Play) return;
            _iframe = 1f; hp = maxHp; int a = 0; foreach (var e in _en) if (e.alive) a++;
            for (int i = a; i < 60; i++) SpawnEnemy(EType.Imp);
            _benchT += Time.unscaledDeltaTime; _benchF++;
            if (_benchT >= 5f) { Debug.Log($"[Bench] lowRes={lowResRender} {Screen.width}x{Screen.height} enemies={a} fps={_benchF / _benchT:0.0}"); _benchT = 0; _benchF = 0; }
#endif
        }
        public void Toast(string s) { toast = s; toastT = 2.5f; }
    }
}
