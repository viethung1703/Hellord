using UnityEngine;

// v0.0.21: all balance numbers Viet Hung wants to tune live here (Resources/HellordTuning.asset, editable in the Inspector).
// Missing asset -> defaults below (identical to the values hard-coded before v0.0.21). Change numbers in the asset, not in code.
namespace Hellord
{
    [CreateAssetMenu(menuName = "Hellord/Tuning", fileName = "HellordTuning")]
    public class HellordTuning : ScriptableObject
    {
        static HellordTuning _i;
        public static HellordTuning I { get { if (_i == null) { _i = Resources.Load<HellordTuning>("HellordTuning"); if (_i == null) _i = CreateInstance<HellordTuning>(); } return _i; } }

        [Header("Hero")]
        public float heroIFrame = 0.7f; // v0.0.26: was 0.8 (harder)
        [Tooltip("A single non-boss hit can take at most this fraction of max HP")] public float nonBossHitCap = 0.2f;
        public float roomEnterGrace = 1.5f;

        [Header("Rooms")]
        public int firstHuntGoal = 60;

        [Header("Boss")]
        public float bossHp = 1200f; // v0.0.40: VH still too tanky (v0.0.39 1800, was 2500)
        public float bossBulletDamage = 9f; // v0.0.26: was 8
        [Tooltip("Pattern warning lead (PatternWarnFX #113) before every boss bullet pattern")] public float bossPatternLead = 0.45f;

        [Header("Burn (BurnFX #110)")]
        [Tooltip("Burn DPS = hit damage x this")] public float burnDpsFromHit = 0.3f;
        public float burnDuration = 3f;
        public float burnTickInterval = 0.5f;

        [Header("Tongue trap (Level 1 Tongue Pulling, VH-approved design)")]
        [Tooltip("Spawn traps automatically in fight rooms (off until Level 1 exists; debug: T key or -traps)")] public bool tongueTrapsInRooms = false;
        public float tongueTriggerRadiusPx = 24f;
        public float tongueTellSeconds = 0.4f;
        [Tooltip("Strike radius around the slit, world units (1u = 16px)")] public float tongueStrikeRadiusU = 1f;
        [Tooltip("Lash length toward the locked point, world units")] public float tongueLashLengthU = 2f;
        [Tooltip("Lash half-width, world units")] public float tongueLashHalfWidthU = 0.35f;
        public float tongueActiveSeconds = 0.15f;
        public float tongueRetractSeconds = 0.17f;
        public float tongueDamage = 12f;
        public float tongueCooldown = 3f;
        public bool tongueHitsEnemies = false;
        public float tongueEnemyDamage = 40f;
        public int tongueMinPerRoom = 3, tongueMaxPerRoom = 6;
        [Tooltip("Min distance from doors and the hero spawn, world units")] public float tongueDoorSpawnClearU = 3f;
        [Tooltip("Traps go within this many px of the walkable edge (never in the open middle)")] public float tongueEdgeBandPx = 40f;
        public float tongueMinSpacingPx = 48f;
        [Tooltip("PLACEHOLDER art (HA mock-up, not approved). Swap by assigning a new sheet here (same 80x80, 9 cols x 8 rows E,SE,S,SW,W,NW,N,NE, pivot = slit centre).")]
        public Texture2D tongueSheet;
        [Tooltip("Used when tongueSheet is empty: Resources path of the placeholder sheet")] public string tongueSheetResource = "HellordPlaceholder/L01_tongue_trap_8dir_sheet_MOCKUP";
        public int tongueFramePx = 80;
        [Header("Hook Thrower (v0.0.24, Level 1 Liars enemy; PLACEHOLDER art; debug: J key or -hookthrowers)")]
        [Tooltip("Mix Hook Throwers into normal fight-room spawns (v0.0.26: ON; spawn weights ramp with progress, see Ranged demons)")] public bool hookThrowersInRooms = true;
        [Tooltip("Chance per normal floor spawn to be a Hook Thrower (when enabled)")][Range(0, 1)] public float hookSpawnChance = 0.04f;
        [Tooltip("Max Hook Throwers alive at once in a room")] public int hookMaxAlive = 2;
        [Tooltip("Max hooks in the air / dragging at once (all throwers)")] public int hookMaxActive = 1;
        public float hookHp = 20f, hookMoveSpeed = 1.6f, hookContactDamage = 6f; public int hookXp = 3;
        [Tooltip("Body radius, world units")] public float hookRadiusU = 0.45f;
        [Tooltip("Keeps this distance band to the hero (world units): walks in if farther, backs off if closer")] public float hookPreferMinU = 4.5f, hookPreferMaxU = 7.5f;
        [Tooltip("Throws only when the hero is within this many world units")] public float hookThrowRangeU = 9f;
        [Tooltip("Chain reach, art px (FX range; 160px = 10u)")] public float hookRangePx = 160f;
        [Tooltip("Delay after spawning before the first throw, seconds")] public float hookFirstDelay = 1.1f; // v0.0.27: was 1.5
        [Tooltip("Re-arm time after a hook ends (hit, miss or snap), seconds")] public float hookCooldown = 3f; // v0.0.27: was 4
        [Tooltip("Whirl wind-up over the thrower's head, seconds (part 1 of the warning)")] public float hookWindupSeconds = 0.4f; // v0.0.27: was 0.45
        [Tooltip("Lane tell on the floor after the wind-up, seconds (part 2 of the warning; aim is locked here)")] public float hookLaneTellSeconds = 0.45f; // v0.0.27: was 0.5
        [Tooltip("Hook flight speed, art px / s")] public float hookSpeedPx = 480f; // v0.0.27: was 420
        [Tooltip("Chain lane width, art px (also the hit width)")] public float hookWidthPx = 6f;
        [Tooltip("Hero hurt radius vs the hook tip, world units")] public float hookHeroRadiusU = 0.4f;
        [Tooltip("Damage when the hook lands")] public float hookDamage = 8f;
        [Tooltip("Pull speed toward the thrower, world units / s")] public float hookPullSpeed = 6f;
        [Tooltip("Pull ends when the hero is this close to the thrower, world units")] public float hookReleaseDistU = 1.4f;
        [Tooltip("Pull ends after this long at most, seconds")] public float hookMaxDragSeconds = 1.6f;
        [Tooltip("Hero's own movement while dragged (multiplier)")][Range(0, 1)] public float hookDragMoveMul = 0.35f;
        [Tooltip("Struggle: holding away from the thrower this long adds one break pip")] public float hookStruggleSecondsPerPip = 0.25f;
        [Tooltip("Pips needed to snap the chain")] public int hookBreakPips = 3;
        [Header("Gossip Imp (v0.0.25, Level 1 Liars enemy; PLACEHOLDER art; debug: I key or -gossipimps)")]
        [Tooltip("Mix Gossip Imps into normal fight-room spawns (v0.0.26: ON; spawn weights ramp with progress, see Ranged demons)")] public bool gossipImpsInRooms = true;
        [Tooltip("Chance per normal floor spawn to be a Gossip Imp (when enabled)")][Range(0, 1)] public float gossipSpawnChance = 0.04f;
        [Tooltip("Max Gossip Imps alive at once in a room")] public int gossipMaxAlive = 2;
        [Tooltip("Max shout rings telegraphing at once (all imps)")] public int gossipMaxShouts = 1;
        [Tooltip("Fragile: dies to about one fireball")] public float gossipHp = 8f, gossipMoveSpeed = 2.2f, gossipContactDamage = 4f; public int gossipXp = 2;
        [Tooltip("Body radius, world units")] public float gossipRadiusU = 0.4f;
        [Tooltip("Hovers in this distance band to the hero while its shout recharges (world units)")] public float gossipPreferMinU = 3.5f, gossipPreferMaxU = 6f;
        [Tooltip("When the shout is ready it walks in and starts shouting once the hero is this close (world units, inside the ring)")] public float gossipShoutTriggerU = 2.4f;
        [Tooltip("Walk-in speed while the shout is ready (hero walks 4.5)")] public float gossipApproachSpeed = 2.8f;
        [Tooltip("Gives up walking in after this long and re-arms (seconds)")] public float gossipApproachMaxSeconds = 4f;
        [Tooltip("Delay after spawning before the first shout, seconds")] public float gossipFirstDelay = 1.5f; // v0.0.27: was 2
        [Tooltip("Time between shouts, seconds")] public float gossipCooldown = 4.5f; // v0.0.27: was 6
        [Tooltip("Shout wind-up (red ring grows + '!'), seconds: the warning. Walk out of the ring to dodge.")] public float gossipShoutTellSeconds = 0.55f; // v0.0.27: was 0.6
        [Tooltip("Shout ring radius, world units (FX px = x16)")] public float gossipShoutRadiusU = 3f;
        [Tooltip("Extra hit slack for the hero's feet vs the ring edge, world units")] public float gossipHeroRadiusU = 0.25f;
        [Tooltip("Stands still this long after the shout, seconds")] public float gossipRecoverSeconds = 0.3f; // v0.0.27: was 0.4
        [Tooltip("How long the hero's aim stays scrambled, seconds")] public float gossipScrambleSeconds = 2.5f;
        [Tooltip("0 = mirror left/right (near-vertical aim flips up/down too), 1 = random big offset per attack, 2 = opposite direction")] public int gossipScrambleMode = 0;
        [Tooltip("Mode 1: random offset range, degrees (either side)")] public float gossipScrambleMinDeg = 60f, gossipScrambleMaxDeg = 150f;
        [Tooltip("Mode 0: below this |x| of the aim direction, mirror also flips up/down so straight-up targets are missed too")] public float gossipMirrorMinX = 0.35f;
        [Tooltip("Damage when the shout lands (0 = status only)")] public float gossipShoutDamage = 0f;
        [Tooltip("A shout that lands while already scrambled refreshes the timer (off = no stacking, no refresh)")] public bool gossipRefreshOnHit = false;
        [Tooltip("Killing the imp during its wind-up cancels the shout")] public bool gossipDeathCancelsShout = true;
        [Tooltip("Height of the '?' status icons over the hero's feet, art px")] public float gossipScrambleHeadPx = 30f;
        [Tooltip("v0.0.26: sorting order of the shout warning FILL (pack 2.3.1). Must stay under every character (characters are about -1300..0; floor/props/lights -32000..-29000). -20000 = pack default. Put 2004 to get the old over-characters look")] public int gossipFillSortingOrder = -20000;

        [Header("Pixel FX Pack 2.5.1 modules (v0.0.26) - each one can be switched off on its own")]
        [Tooltip("LevelUpFX: hero level-up beat, ground ring, cards slide in one by one, short pick lock (0.45 s) and a gentle dim ramp back to play. Off = v0.0.25 card screen + old burst")] public bool fxLevelUp = true;
        [Tooltip("HurtFX: gold hit brackets/ring/sparks on the hero instead of the whole-sprite red tint, a silver shrinking i-frame ellipse at the feet, heart pips over the head under 30% HP, soul-flame on death. Off = v0.0.25 red tint")] public bool fxHeroHurt = true;
        [Tooltip("DoorFX: steel bars + padlock slam onto doors when a fight room seals, gold unlock burst + off-screen door markers when it clears. Drawn on top of HA's door art")] public bool fxDoors = true;
        [Tooltip("RoomFX: nest collapse burst + dull 'dead' seal marker on destroyed nests, gold beacon on the stairs after the boss")] public bool fxRoom = true;
        [Tooltip("Sorting order of HurtFX (hero is 32000, so the pack default 61 would hide under him)")] public int fxHurtSortingOrder = 32020;
        [Tooltip("Sorting order of LevelUpFX sparkles/plate (above the hero); its ground ring stays at -20000 under characters")] public int fxLevelUpSortingOrder = 32010;

        [Header("Hell atmosphere (v0.0.22, debug: H toggles, -noatmos starts off)")]
        public bool atmosEnabled = true;
        [Tooltip("Floor/prop grade, linear-space multiplier (1 = unchanged). Drawn UNDER gameplay sprites, so hero/enemies/bullets/pickups are never graded.")] public Color gradeTint = new Color(1.15f, 0.88f, 0.74f, 1f);
        [Tooltip("Additive shadow lift, linear units (keeps deep shadows from crushing)")] public Color shadowLift = new Color(0.006f, 0.0016f, 0.0008f, 1f);
        [Tooltip("Flicker depth, fraction of brightness (comfort cap 0.08)")][Range(0, 0.08f)] public float flickerAmp = 0.05f;
        [Tooltip("Flicker base frequency Hz (comfort cap 3; second component = x1.85)")][Range(0.1f, 1.6f)] public float flickerHz = 0.7f;
        [Tooltip("Banded fire pool under each brazier: additive strength")] public float firePoolStrength = 0.32f;
        public int firePoolRadiusPx = 44;
        public Color firePoolColor = new Color(0.97f, 0.46f, 0.13f, 1f);  // Endesga #f77622
        [Tooltip("Existing room warm/brazier lights get the same flicker; strength multiplier")] public float roomLightBoost = 0.9f;
        [Tooltip("Embers + ash on screen at once")] public int emberCount = 26; public int ashCount = 16;
        public float emberRise = 0.9f, ashRise = 0.25f;
        public Color emberColor = new Color(0.996f, 0.682f, 0.204f, 0.85f); // #feae34
        public Color ashColor = new Color(0.353f, 0.412f, 0.533f, 0.45f);   // #5a6988
        [Tooltip("Red edge fog (CRT pass, screen edges only)")][Range(0, 1)] public float edgeFogAmt = 0.38f;
        [Range(0, 1)] public float edgeFogInner = 0.45f;
        public Color edgeFogColor = new Color(0.24f, 0.035f, 0.035f, 1f);  // dark ember red, never saturated full-screen
        [Tooltip("Heat haze: glow-layer wobble at the edges, art px")][Range(0, 2)] public float hazeAmpPx = 0.8f;
        [Header("v0.0.29 Audio")][Tooltip("Master SFX volume (multiplies each sound). -volume launch flag still scales everything via AudioListener")][Range(0, 1)] public float sfxVolume = 0.35f;
        [Tooltip("v0.0.33 music volume, on/off, and volume multiplier while paused or on the level-up screen")] public float musicVolume = 0.3f; public bool musicEnabled = true; public float musicPausedMul = 0.5f;
        [Tooltip("v0.0.61 t611u: menu music volume (MainMenu / Altar loops)")] public float menuMusicVol611 = 0.6f; // v0.0.61
        [Tooltip("v0.0.61: crossfade main menu <-> altar loop (s)")] public float menuMusicXfadeS611 = 1.5f; // v0.0.61
        [Tooltip("v0.0.61: menu music fade-in / fade-out (s); fade-out runs on Descend")] public float menuMusicFadeInS611 = 1.5f, menuMusicFadeOutS611 = 1.0f; // v0.0.61
        [Tooltip("v0.0.61: Dead/Won stinger volume and wait before the menu loop fades in (s)")] public float menuStingerVol611 = 0.7f, menuStingerHoldS611 = 3f; // v0.0.61
        [Tooltip("v0.0.61: UI SFX volumes (AUDIO_MAP menu section)")] public float uiHoverVol611 = 0.55f /* v0.0.63 t615u */, uiConfirmVol611 = 0.6f, uiBackVol611 = 0.55f, uiDeniedVol611 = 0.55f, sealPressVol611 = 0.65f, altarCarouselVol611 = 0.5f, pactUnlockVol611 = 0.65f, pactSignVol611 = 0.65f, pactRemoveVol611 = 0.55f, pathSwitchVol611 = 0.65f, sealDescendVol611 = 0.75f, cardRerollVol611 = 0.55f, cardSkipVol611 = 0.5f, restartVol611 = 0.65f; // v0.0.61
        [Tooltip("v0.0.61: ui_hover rate limit (s)")] public float uiHoverGap611 = 0.08f; // v0.0.61; v0.0.63 t615u 80 ms
        [Tooltip("v0.0.61: UI hooks keep playing while LevelUp/Chest pause the voice pool (fix: card SFX were muted)")] public bool uiUnpaused611 = true; // v0.0.61
        [Tooltip("v0.0.62 t613u: UI/menu SFX gain, independent of sfxVolume. Final = hook vol x this.")] public float uiSfxGain612 = 1.6f;
        public bool sfxEnabled = true;
        [Tooltip("Low-HP pulse (edge only)")] public bool lowHpPulse = true; [Range(0, 1)] public float lowHpThreshold = 0.3f;
        [Range(0, 0.5f)] public float lowHpPulseAmt = 0.35f; [Range(0.2f, 1.5f)] public float lowHpPulseHz = 0.8f;
        [Tooltip("v0.0.23: with -nocrt, draw the same edge fog + low-HP pulse as a GUI overlay (else -nocrt has no low-HP warning)")] public bool noCrtEdgeFog = true;
        [Tooltip("v0.0.23: low-HP pulse colour (sRGB, edges only). Brighter than the edge fog so the warning reads on dark floors")] public Color lowHpPulseColor = new Color(0.62f, 0.07f, 0.04f, 1f);

        [Header("Rooms v0.0.26: size mix, density, clear rule")]
        [Tooltip("Weights for picking a fight room's size class (small 640x368 / 768x432, medium 896x512 / 1024x576, large 1152x656 / 1280x736). The start room is always small, shrine/secret keep their own art, the boss room stays 1536x864")] public float roomSmallWeight = 0.45f, roomMediumWeight = 0.40f, roomLargeWeight = 0.15f;
        [Tooltip("Smallest size class allowed for Nests and Elite rooms (0 small, 1 medium, 2 large): three nests / elite + guards need space")] [Range(0, 2)] public int nestsMinSize = 1, eliteMinSize = 1;
        [Tooltip("Demons alive at once per 100 square world units of walkable floor, at the start of a room (density scales with room AREA)")] public float densityPer100Units = 1.7f;
        public float densityMin = 6f, densityMax = 70f;
        [Tooltip("Extra density per 'progress' unit (minutes in room/60 + 0.4 per cleared fight)")] public float densityRampPerProgress = 0.35f;
        [Tooltip("Per-room-type density multipliers")] public float surviveDensityMul = 1f, huntDensityMul = 1.15f, treasureDensityMul = 1.5f, curseDensityMul = 1.8f, nestsDensityMul = 0.45f;
        [Tooltip("The spawner refills the gap between alive and target over this many seconds, clamped to the rates below")] public float spawnRefillSeconds = 4f, spawnRateMin = 0.6f, spawnRateMax = 4.5f;
        [Tooltip("Spawns appear at least this far (world units) from the hero, so small rooms never drop a demon on top of him")] public float spawnMinHeroDistance = 9f;
        [Tooltip("Goal values: Survive/Curse seconds, kill goals (Hunt after the first, Treasure), Elite guards")] public float surviveSeconds = 60f, curseSeconds = 45f; public int huntGoal = 100, treasureGoal = 40, eliteGuards = 8;
        [Tooltip("Kill goals are multiplied by this per size class (small / medium / large)")] public float goalMulSmall = 0.7f, goalMulMedium = 1f, goalMulLarge = 1.25f;
        [Tooltip("OLD behaviour: the moment the goal is met every remaining demon dies and the room clears. OFF by default: spawning stops and the room clears when the last demon is killed")] public bool clearInstant = false;
        [Tooltip("Safety net: after the goal is met, if demons are still alive after this many seconds they are burst away and the room clears (0 = never, you must kill them all)")] public float clearStragglerSeconds = 45f;
        [Tooltip("Heal (fraction of max HP) when a room clears / when you take the stairs to the next floor")] [Range(0, 1)] public float healOnClear = 0.10f, healOnFloorChange = 0.40f;

        [Header("Floors v0.0.26")]
        [Tooltip("Per floor after the first: hunt/treasure kill goals grow by this fraction; boss attack cadence speeds up by floorBossPatternRate")] public float floorGoalGrowth = 0.2f, floorBossPatternRate = 0.10f; // v0.0.41 VH (was 0.12)
        [Tooltip("Floors per run; each is a set of rooms ending in the boss + stairs. t495u: 4 = every theme (Ox Pit / Frozen / Burning / Mortar) once per run, shuffled")] [Range(1, 8)] public int floorCount = 4;
        [Tooltip("Per floor after the first: enemy HP / contact damage / density / speed grow by this fraction; boss HP / damage by their own")] public float floorEnemyHpGrowth = 0.25f, floorEnemyDmgGrowth = 0.15f, floorDensityGrowth = 0.25f, floorSpeedGrowth = 0.02f, floorBossHpGrowth = 0.2f, floorBossDmgGrowth = 0.15f; // v0.0.41 VH (was 0.35,0.25,0.25,0.04,0.3,0.2)
        [Tooltip("Floor palette: multiplies the room art + props (<= 1). Index = floor. Floor 1 is the untouched art. These reuse the existing room art, no new art")] public Color[] floorTints = { new Color(1f, 1f, 1f, 1f), new Color(0.92f, 0.97f, 1f, 1f), new Color(1f, 0.93f, 0.88f, 1f) }; // v0.0.37: near white - the room art is recoloured to a floor palette (Endesga 32 ramps) in code instead
        [Tooltip("Multiplies the hell-atmosphere grade per floor (so the palette also reads under the grade quad)")] public Color[] floorGradeMul = { new Color(1f, 1f, 1f, 1f), new Color(0.85f, 1.0f, 1.2f, 1f), new Color(1.12f, 0.85f, 0.7f, 1f) };

        [Header("Pressure v0.0.26 (everything harder)")]
        [Tooltip("Multiplies demon contact damage")] public float contactDamageMul = 1.6f;
        [Tooltip("Demon HP grows by this fraction per minute of run time (0 = off, per-floor growth only; was 0.05)")] public float enemyHpPerMinute = 0f; // v0.0.42: VH "5% per minute is crazy" (was 0.05). 0 = enemy HP only grows per floor
        public float enemySpeedMul = 1.15f; // v0.0.39: VH +15% (was 1.25) // v0.0.28: VH asked +25% (v0.0.27 was 1.375)
        [Tooltip("Elite HP before time and floor growth")] public float eliteHp = 360f; // v0.0.28: was 600 (VH: elites too tanky)
        [Tooltip("Multiplies every boss hit (horn, stomp, herd, bullets)")] public float bossDamageMul = 1.4f;
        [Tooltip("Boss phase 1: seconds between bullet patterns (was 1.5), phase 2 (was 1.35), bloodrage (was 1.2)")] public float bossP1PatternCooldown = 1.05f, bossP2PatternCooldown = 1.0f, bossRagePatternCooldown = 0.8f;
        [Tooltip("Boss phase 1 also uses the double ring (was phase 2 only), fires wider fans and 3 spiral arms")] public bool bossP1DoubleRing = true; public int bossP1FanCount = 11, bossP1SpiralArms = 3; public float bossP1SpiralInterval = 0.2f;
        [Tooltip("Boss bullet speed multiplier, boss walk speed in phase 1 (was 1.3), phase 2 starts below this HP fraction (was 0.6)")] public float bossBulletSpeedMul = 1.1f, bossP1Speed = 2.3f /* v0.0.27: was 1.7 */, bossPhase2At = 0.65f;

        [Header("Pacing pressure v0.0.42 - shrinking drops")]
        [Tooltip("Boss HP grows by this fraction per minute of run time (0 = off; was a hidden 0.05)")] public float bossHpPerMinute = 0f; // v0.0.42: VH no time-based HP growth, only per floor
        [Tooltip("Shrinking drops: in the listed room types, demon XP/bone drops shrink the longer you stay (stops farming endless spawns). Other rooms are untouched")] public bool dropDecayEnabled = true; // v0.0.42
        [Tooltip("Seconds in the room before drops start to shrink")] public float dropDecayGraceSeconds = 45f; // v0.0.42
        [Tooltip("After the grace time, drops lose this fraction of full value per further 60 s (0.5 = -50% a minute)")] [Range(0, 5)] public float dropDecayPerMinute = 0.5f; // v0.0.42
        [Tooltip("Lowest drop multiplier the decay reaches (0.1 = 10%)")] [Range(0, 1)] public float dropDecayMinMul = 0.1f; // v0.0.42
        [Tooltip("Room types the decay applies to. Default: Nests (the only room where demons keep spawning with no time or kill limit). Survive and Curse are timed, Hunt/Treasure stop spawning at their goal, so they are not listed")] public RoomType[] dropDecayRooms = { RoomType.Nests }; // v0.0.42

        [Header("Balance v0.0.27 - demons")]
        [Tooltip("Demon attack frequency: ranged cooldowns tick this much faster (contact i-frame is separate: contactIFrameDiv)")] public float enemyAttackRateMul = 1.2f; // v0.0.39: VH +20% (was 1.4)
        [Tooltip("Contact-hit i-frame divisor: after a demon touch the hero is safe for heroIFrame / this (0.7 / 1.15 = 0.61 s). 1 = v0.0.26 (0.7 s). Was tied to enemyAttackRateMul (0.5 s) in the first v0.0.27 try, which killed a fresh hero in about 20 s")] public float contactIFrameDiv = 1.15f;

        [Header("Balance v0.0.27 - boss melee gap closers")]
        [Tooltip("Horn sweep reach, world units (was 6.25). The cone tell is drawn at this size too")] public float bossHornReachU = 7.5f;
        [Tooltip("Seconds the boss rests between attacks while walking (was 1.2 .. 2.2)")] public float bossWalkRestMin = 0.9f, bossWalkRestMax = 1.7f;
        [Tooltip("Sweep tracking: halfway through the wind-up the cone RE-AIMS once at the hero (a second, visible tell flash) and the boss creeps toward him meanwhile")] public bool bossSweepRetrack = true;
        [Tooltip("Boss creep speed during a sweep wind-up, world units / s")] public float bossSweepCreepSpeed = 2.6f;
        [Tooltip("Re-aimed wind-up length (never below this, so the tell stays readable), seconds")] public float bossRetrackTellMin = 0.4f;
        [Tooltip("Follow-up swing: if the first sweep misses and the hero is within this many world units, swing again (chance), with a shorter but still visible tell")] public float bossFollowupRangeU = 8.5f, bossFollowupChance = 0.85f, bossFollowupTell = 0.5f;
        [Tooltip("Lunge (telegraphed short dash, ends in a follow-up sweep): used instead of a sweep when the hero is farther than the sweep reach, and sometimes when he is farther than bossLungeMinDistU")] public float bossLungeChance = 0.55f, bossLungeMinDistU = 5f;
        [Tooltip("Lunge wind-up (red lane on the floor), seconds")] public float bossLungeTell = 0.55f;
        [Tooltip("Lunge dash speed (world units / s), longest dash (world units), overshoot past the hero's marked spot, hit lane half-width (world units), damage as a fraction of boss contact damage")] public float bossLungeSpeed = 17f, bossLungeMaxU = 9f, bossLungeOvershootU = 1.2f, bossLungeHalfWidthU = 1.5f, bossLungeDmgMul = 0.75f;

        [Header("Balance v0.0.27 - levelling and growth")]
        [Tooltip("XP needed per level = (xpBase + xpPerLevel x L + xpQuad x L^2) x xpCostMul. v0.0.26 was (5 + 4L + L^2/4) x 1. The v0.0.27 curve gives about 70% fewer level-ups (front-loaded: Lv2 after about 15 XP so a fresh hero is not helpless) by the end of floor 1 (about 4 instead of about 12), then 3-4 more per floor")] public float xpBase = 5f, xpPerLevel = 0f, xpQuad = 10f, xpCostMul = 1f;
        [Tooltip("Player level that unlocks skill slot 2 and 3 (was 5 and 12)")] public int slot2Level = 3, slot3Level = 7;
        [Tooltip("t818u: player level that unlocks skill slot 4 and 5 (the belt holds 5 skills in 1 row)")] public int slot4Level818 = 12, slot5Level818 = 18;
        [Tooltip("t818u: hard cap on skills held (Auto unlocks + boss slot cards). Belt shows this many cells (max 8).")] public int skillSlotsMax818 = 5;
        [Tooltip("t1057u: level-up card choices offered (was hardcoded 3). Sin #25 can still add extra.")] [Range(2, 5)] public int levelUpCardChoices1057 = 4;
        [Tooltip("t1058u: bones charged per level-up reroll after the free one. PROVISIONAL prototype (skip awards +10).")] public int levelUpRerollBones1057 = 15;
        [Tooltip("t1058u: free rerolls granted when a level-up screen opens (then bones)")] public int levelUpFreeRerolls1057 = 1;
        [Tooltip("Per level-up: max HP (was 2), skill damage fraction (was 0.01), move speed fraction (was 0.005), heal fraction of max HP (was 0.1)")] public float levelMaxHp = 1.5f, levelIncDmg = 0.005f, levelSpeed = 0.0025f, levelHeal = 0.06f;
        [Tooltip("Skill LEVEL adds only a little damage per hit now (coverage is what grows): damage = 1 + step x (level - 1) (was 0.3); a max-level skill gets the spike multiplier (was 1.5, now none)")] public float skillLevelStep = 0.12f /* t1012u adventure A: was 0.06 */, skillMaxSpike = 1f;
        [Tooltip("Stat cards. Vitality max HP fraction (was 0.10), Swiftness move speed (was 0.08), Hide armor (was 5), Malice crit chance (was 0.05)")] public float statHpFrac = 0.07f, statSpeed = 0.05f, statArmor = 4f, statCrit = 0.04f;

        [Header("Balance v0.0.27 - skill damage per hit (lower) and coverage per skill level (much bigger)")]
        [Tooltip("Damage multiplier per skill (all lower than v0.0.26's 1), order Fireball, Frost Cleave, Storm Ring, Bone Orbit, Curse Rune. Fireball is 0.92 so a Lv1 shot (9.2) still one-shots an Imp (9 HP); at 0.75 imps took 2 hits and the early game collapsed")] public float[] skillDmgMul = { 0.92f, 0.85f, 0.8f, 0.8f, 0.8f };
        [Tooltip("Fireball by skill level Lv1..Lv5: projectiles per cast (was 1), fan angle between projectiles (deg), explosion radius (world units, was 0.8), explosion damage fraction (was 0.4)")] public int[] fbProjectiles = { 1, 1, 2, 2, 3 } /* t1012u A */; public float fbSpreadDeg = 13f; public float[] fbSplashRadiusU = { 0.8f, 0.9f, 1.0f, 1.1f, 1.2f } /* t1012u A */; public float fbSplashFrac = 0.4f;
        [Tooltip("Frost Cleave by skill level: reach in world units (was 2.3), half-angle of the swing in degrees (was 65; 180 = full circle), knock-back (was 4), damage bonus against chilled / frozen demons")] public float[] frostRangeU = { 3.2f, 3.5f, 3.8f, 4.1f, 4.4f } /* t1012u A */; public float[] frostHalfAngle = { 70f, 75f, 80f, 85f, 90f } /* t1012u A: L5 stays 180° forward, not full circle */; public float frostKnock = 4f, frostChilledBonus = 1.25f;
        [Tooltip("Storm Ring by skill level: ring radius in world units (was 2.6)")] public float[] stormRadiusU = { 3.2f, 3.5f, 3.8f, 4.1f, 4.4f } /* t1012u A */;
        [Tooltip("Bone Orbit by skill level: skulls (was 3), orbit radius (was 2.0 u), hit radius per skull (was 0.45 u)")] public int[] orbitCount = { 2, 3, 3, 4, 5 } /* t1012u A */; public float[] orbitRadiusU = { 2.6f, 2.8f, 3.0f, 3.2f, 3.4f } /* t1012u A */; public float orbitHitRadiusU = 0.65f /* v0.0.31: was 0.5 */;
        [Tooltip("Curse Rune by skill level: runes per cast, scattered around the aim point (was 1), rune radius in world units (was 1.8), how far the extra runes scatter (world units)")] public int[] curseRunes = { 1, 1, 1, 2, 2 } /* t1012u A */; public float[] curseRadiusU = { 2.6f, 2.8f, 3.0f, 3.2f, 3.4f } /* t1012u A */; public float curseScatterU = 3.2f;

        [Header("Balance v0.0.27 - support levels Lv1..Lv5 (picking an owned support upgrades it)")]
        [Tooltip("Multi Projectiles: extra projectiles (was +2 flat), damage multiplier of the skill (was 0.75)")] public int[] supMultiExtra = { 1, 2, 2, 3, 4 }; public float multiProjDmgMul = 0.88f;
        [Tooltip("Pierce: enemies pierced (was 2), damage lost per pierce (was 0.15)")] public int[] supPierce = { 1, 2, 3, 4, 5 }; public float pierceFalloff = 0.15f;
        [Tooltip("Fork: children on first hit (was 2), child damage fraction (was 0.7), fork spread between children (deg)")] public int[] supForkChildren = { 2, 3, 3, 4, 5 }; public float[] supForkDmg = { 0.55f, 0.6f, 0.65f, 0.7f, 0.75f }; public float forkSpreadDeg = 28f;
        [Tooltip("Bounce: bounces (was 2) and search radius for the next demon (was 4)")] public int[] supBounce = { 1, 2, 3, 4, 5 }; public float[] supBounceRangeU = { 4f, 4.5f, 5f, 5.5f, 6f };
        [Tooltip("Homing: turn speed (was 6) and seek range in world units (was 5)")] public float[] supHomingTurn = { 4f, 6f, 8f, 10f, 13f }; public float[] supHomingRangeU = { 4f, 5f, 6f, 7f, 8.5f };
        [Tooltip("Bigger Area: area multiplier (was 1.4)")] public float[] supAreaMul = { 1.15f, 1.20f, 1.25f, 1.30f, 1.40f } /* t1012u B */;
        [Tooltip("Echo: repeats (was 1) and damage of each repeat (was 0.7)")] public int[] supEchoCount = { 1, 1, 1, 1, 1 } /* t1012u B */; public float[] supEchoDmg = { 0.35f, 0.40f, 0.45f, 0.50f, 0.60f } /* t1012u B */;
        [Tooltip("More Orbitals: extra skulls (was +2)")] public int[] supOrbitals = { 1, 2, 3, 3, 4 };
        [Tooltip("Burn / Bleed strength multiplier (burn: dps = hit x burnDpsFromHit x this; bleed: 25% of hit x this) (was 1.5 as innate with support) and extra seconds of Burn per level above 1")] public float[] supBurnMul = { 1.2f, 1.6f, 2.0f, 2.5f, 3.0f }; public float[] supBleedMul = { 1.2f, 1.6f, 2.0f, 2.5f, 3.0f }; public float burnExtraSecondsPerLevel = 0.5f;
        [Tooltip("Chill: chill duration multiplier (was 1), chills needed to freeze (was 3)")] public float[] supChillTime = { 1f, 1.3f, 1.6f, 1.9f, 2.3f }; public int[] supChillFreezeHits = { 3, 3, 3, 2, 2 };
        [Tooltip("Shock: extra damage taken by shocked demons (was 0.25 at every level). Base value is used when a skill is innately shocking and has no Shock support")] public float shockBaseAmp = 0.2f; public float[] supShockAmp = { 0.25f, 0.3f, 0.36f, 0.43f, 0.5f };
        [Tooltip("Added Damage: damage multiplier (was 1.3 flat)")] public float[] supAddedDmg = { 1.15f, 1.22f, 1.3f, 1.38f, 1.5f };
        [Tooltip("Faster Casting: cooldown multiplier (was 0.75 flat)")] public float[] supFasterCd = { 0.90f, 0.87f, 0.84f, 0.81f, 0.78f } /* t1012u B */;

        [Header("v0.0.36 - new skills (all PLACEHOLDER visuals; low damage per hit, coverage grows per skill level Lv1..Lv5)")]
        [Tooltip("Damage multiplier for the 5 new skills, order Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool (same role as skillDmgMul for the first five)")] public float[] newSkillDmgMul = { 0.8f, 0.85f, 0.85f, 0.85f, 0.8f };
        [Tooltip("Chain Lightning by skill level: jumps after the first target, jump reach (world units), first-target search radius around the hero (world units), damage kept per jump (1 = no falloff)")] public int[] chainJumps = { 2, 2, 3, 3, 4 } /* t1012u A spirit */; public float[] chainJumpRangeU = { 3.2f, 3.4f, 3.6f, 3.8f, 4.0f } /* t1012u A spirit */; public float chainFirstRangeU = 8f, chainJumpDmgKeep = 0.92f;
        [Tooltip("Chain Lightning bolt: color, seconds the bolt stays, delay between hops")] public Color chainColor = new Color(0.7f, 0.9f, 1f); public float chainBoltSeconds = 0.22f, chainHopDelay = 0.03f;
        [Tooltip("Hellfire Pillar by skill level: pillars per cast, pillar radius (world units), how far from the hero a pillar may land (world units)")] public int[] pillarCount = { 1, 1, 2, 2, 3 } /* t1012u A spirit */; public float[] pillarRadiusU = { 1.3f, 1.4f, 1.5f, 1.6f, 1.7f } /* t1012u A spirit */; public float pillarRangeU = 9f;
        [Tooltip("Hellfire Pillar: warning time before a pillar erupts (s), extra delay between pillars of one cast (s), how many random demons are sampled per pillar to find the densest cluster, damage multiplier of the eruption")] public float pillarTellSeconds = 0.5f, pillarStaggerSeconds = 0.12f; public int pillarSamples = 10; public float pillarDmgMul = 1.6f;
        [Tooltip("Blood Whip by skill level: reach (world units), half-width of the lash (world units), lashes per cast, fan angle between lashes (deg)")] public float[] whipReachU = { 5f, 5.5f, 6f, 6.5f, 7f } /* t1012u A spirit */; public float[] whipHalfWidthU = { 0.6f, 0.65f, 0.7f, 0.75f, 0.8f } /* t1012u A spirit */; public int[] whipLashes = { 1, 1, 1, 2, 2 } /* t1012u A spirit */; public float whipFanDeg = 28f;
        [Tooltip("Blood Whip: delay between lashes (s), knock-back of a hit, HP healed per demon hit by skill level, heal cap per cast (HP)")] public float whipLashDelay = 0.14f, whipKnock = 1.5f; public float[] whipHealPerHit = { 0.4f, 0.5f, 0.6f, 0.8f, 1f }; public float whipHealCapPerCast = 3f;
        [Tooltip("Soul Spears by skill level: spears per cast, built-in pierce (extra from the Pierce support is added), fan angle between spears (deg); speed (world units/s); damage lost per pierce as a fraction of the normal pierce falloff")] public int[] spearCount = { 1, 1, 2, 2, 3 } /* t1012u A spirit */; public int[] spearPierce = { 1, 1, 2, 2, 3 } /* t1012u A spirit */; public float spearFanDeg = 9f, spearSpeed = 14f, spearFalloffMul = 0.5f; [Tooltip("Soul Spears placeholder tint (the bone projectile sprite is recoloured)")] public Color spearTint = new Color(0.6f, 0.85f, 1f);
        [Tooltip("Grave Pool by skill level: pool radius (world units), seconds a pool lasts, damage tick interval (s), slow strength (0.3 = 30% slower), most pools alive at once, hero must move this far (world units) before the next pool drops")] public float[] gravePoolRadiusU = { 1.3f, 1.4f, 1.5f, 1.6f, 1.8f } /* t1012u A spirit */; public float[] gravePoolSeconds = { 3f, 3.5f, 4f, 4.5f, 5f } /* t1012u A spirit */; public float graveTickSeconds = 0.5f; public float[] graveSlow = { 0.3f, 0.35f, 0.4f, 0.45f, 0.5f }; public int graveMaxPools = 10; public float graveMinSpacingU = 1.4f;
        [Tooltip("Grave Pool colour (placeholder look: AuraField tint) and slow duration refreshed per tick (s)")] public Color gravePoolColor = new Color(0.3f, 0.85f, 0.4f, 0.4f); public float graveSlowRefresh = 0.6f;

        [Header("v0.0.36 - new supports Lv1..Lv5")]
        [Tooltip("Executioner: demons below this fraction of max HP take the bonus (Lv1 10% .. Lv5 30%), damage multiplier below the threshold. Bosses and elites are not affected when executionerSkipBigs is on")] public float[] supExecThreshold = { 0.10f, 0.15f, 0.20f, 0.25f, 0.30f }; public float[] supExecDmg = { 2.0f, 2.2f, 2.4f, 2.7f, 3.0f }; public bool executionerSkipBigs = true;
        [Tooltip("Life Leech: fraction of the damage dealt that heals you, most HP healed per second")] public float[] supLeechFrac = { 0.02f, 0.03f, 0.04f, 0.05f, 0.06f }; public float[] supLeechCapPerSec = { 1f, 1.5f, 2f, 2.5f, 3f };
        [Tooltip("Knockback: extra push on every hit (world units of impulse, same scale as the skills' own knock-back). Elites and bosses ignore pushes")] public float[] supKnockback = { 2f, 3f, 4f, 5f, 6.5f };
        [Tooltip("Soul Harvest: extra XP dropped by a kill (blood value), chance of one extra bone")] public int[] supHarvestXp = { 1, 1, 2, 2, 3 }; public float[] supHarvestBoneChance = { 0.05f, 0.08f, 0.12f, 0.18f, 0.25f };
        [Tooltip("Explode on Kill: explosion radius (world units), damage as a fraction of the killed demon's max HP (+ flat), flat damage")] public float[] supExplodeRadiusU = { 1.4f, 1.6f, 1.8f, 2.1f, 2.4f }; public float[] supExplodeDmg = { 0.35f, 0.4f, 0.5f, 0.6f, 0.7f }; public float explodeFlatDmg = 3f; [Tooltip("Explode on Kill: most damage one explosion can deal (so elites do not nuke the room)")] public float explodeMaxDmg = 30f;

        [Header("Ranged demons v0.0.26 (Hook Thrower + Gossip Imp in the normal mix)")]
        [Tooltip("Chance a normal spawn is a ranged demon = base + ramp x progress + perFloor x floor, capped. Split between Hook Thrower and Gossip Imp by hookShare")] public float rangedBaseChance = 0.06f, rangedRampPerProgress = 0.06f, rangedPerFloor = 0.05f, rangedMaxChance = 0.30f, hookShare = 0.5f;

        [Header("Notices (banners) v0.0.26")]
        [Tooltip("Seconds an important banner stays fully shown (room cleared, curse lifted, boss, floor change)")] public float bannerHold = 2.0f;
        public float bannerFade = 0.25f;
        [Tooltip("Show the objective banner when a fight room starts")] public bool bannerRoomGoal = true; public float bannerGoalHold = 1.6f;
        [Tooltip("Dark band opacity behind banner text")] [Range(0.3f, 1)] public float bannerBandAlpha = 0.9f;

        [Header("Skill bar v0.0.26")]
        public bool skillBarEnabled = true;
        [Tooltip("Approved skill-bar art: cooldown number size (integer scale of the 3x5 digit font; 1 = as approved)")] [Range(1, 4)] public int skillCdDigitScale = 1;
        [Tooltip("Support pip icon size in HUD pixels (the approved support icons are 20x20 = 20 here; smaller squeezes them)")] [Range(8, 20)] public int skillBarSupportPx = 20;
        [Tooltip("Cooldown sweep frames (approved art uses 12)")] [Range(4, 48)] public int skillCdFrames = 12;
        [Tooltip("Ready flash length in seconds (approved art: 0.15)")] public float skillReadyFlash = 0.15f;

        [Header("Camera / sorting v0.0.26")]
        [Tooltip("Add an always-on display camera so the Editor Game view never says 'No cameras rendering' (the gameplay camera draws into a low-res texture)")] public bool displayCamera = true;
        [Tooltip("Hero walks behind decor props that are lower on screen than his feet (y-sort by sprite pivot)")] public bool heroBehindProps = true;


        [Header("v0.0.37 Floor themes (all recolours are PLACEHOLDER, built in code from the existing art, Endesga 32 ramps)")]
        [Tooltip("Recolour the room base art + props per floor (floor 2 = frost ramp, floor 3 = fire ramp). Floor 1 is never touched. Off = old plain tint only")] public bool roomRecolor = true;
        [Tooltip("Brightness gain before the room art is mapped onto the floor ramp. Index = floor (floor 1 unused). Higher = lighter room")] public float[] roomRecolorGain = { 1f, 1.7f, 1.55f };
        [Tooltip("Same gain for props (torches, pillars...) - they are brighter than the floor, so lower values keep them from blowing out")] public float propRecolorGain = 1.15f;
        [Tooltip("Brightness gain for recoloured demons (frost / exploding variants) and the recoloured bosses before they are mapped onto their ramp")] public float spriteRecolorGain = 1.35f;
        [Tooltip("Hazards (ice, lava) never spawn inside this radius (world units) of a door, the hero's arrival point, the room centre or the spawn spot")] public float hazardSafeRadiusU = 5.5f;
        [Tooltip("Seconds after entering a room during which ice/lava cracks stay harmless (lava stays dormant)")] public float hazardGraceS = 3f;

        [Header("v0.0.37 Floor 2 - Frozen Hell")]
        [Tooltip("Ice patches in fight rooms: one patch per this many square world units of room, min and max count")] public float icePatchAreaU2 = 400f; public int icePatchMin = 2, icePatchMax = 10;
        [Tooltip("Ice patch radius range (world units; the patch is drawn 30% flatter than it is wide - the hit shape is the drawn ellipse)")] public float iceRadiusMinU = 2.0f, iceRadiusMaxU = 3.4f;
        [Tooltip("ON ICE the hero's velocity only changes by this much per second (world units / s^2): lower = more slippery. Walk speed is ~4.5 u/s")] public float iceAccelU = 5f;
        [Tooltip("After stepping OFF ice the hero keeps the slide for this many seconds with this accel (world units / s^2), then control is back to normal")] public float iceCarryS = 0.35f, iceCarryAccelU = 14f;
        [Tooltip("Chance a demon spawned on floor 2 is a FROST variant (blue, hits slow you)")] [Range(0, 1)] public float frostChance = 0.22f;
        [Tooltip("Frost demon multipliers: max HP, move speed")] public float frostHpMul = 1.25f, frostSpeedMul = 0.92f;
        [Tooltip("A hit from a frost demon (or the ice breath) slows the hero: move speed multiplier while slowed, and seconds (refreshed by every hit)")] [Range(0.1f, 1f)] public float frostSlowMul = 0.55f; public float frostSlowS = 1.6f;
        [Tooltip("Frost Ox-Head attack order. C charge, S sweep (lunges at range), T stomp, H herd, L lunge, I ICE BREATH, P spear thrust, X spear thrust then charge, K spear combo. Loops")] public string bossCycleFloor1 = "CSTCH", bossCycleFloor2 = "CISTCIH", bossCycleFloor3 = "PXSKTC";
        [Tooltip("Ice breath cone: telegraph seconds (the aim stops following you after the lock fraction of the tell), breath duration, reach (world units), half angle (degrees)")] public float bossIceTellS = 1.15f, bossIceBreathS = 0.9f, bossIceReachU = 9f, bossIceHalfAngleDeg = 32f; [Range(0.1f, 1f)] public float bossIceAimLock = 0.55f;
        [Tooltip("Ice breath damage per hit (hero i-frames cap the rate) and the hero slow seconds it adds")] public float bossIceDmg = 9f, bossIceSlowS = 2f;

        [Header("v0.0.37 Floor 3 - Burning Hell")]
        [Tooltip("Lava cracks in fight rooms: one crack per this many square world units, min and max count, crack width (world units)")] public float lavaAreaU2 = 450f; public int lavaMin = 2, lavaMax = 9; public float lavaWidthU = 1.0f;
        [Tooltip("Lava cycle seconds: dormant (dark crack) -> warning (glowing, harmless) -> active (burns). Each crack starts at a random point of the cycle")] public float lavaDormantS = 2.2f, lavaWarnS = 1.3f, lavaActiveS = 1.8f;
        [Tooltip("Lava damage per hit (hero i-frames cap the rate); grows with floorEnemyDmgGrowth like demon damage")] public float lavaDmg = 7f;
        [Tooltip("Chance a demon spawned on floor 3 is an EXPLODING variant (Imp / Ghoul based)")] [Range(0, 1)] public float bomberChance = 0.16f;
        [Tooltip("Exploding demon multipliers: max HP, move speed (speed while the fuse burns = bomberFuseMoveMul of this)")] public float bomberHpMul = 0.8f, bomberSpeedMul = 1.15f, bomberFuseMoveMul = 0.4f;
        [Tooltip("The fuse lights when the hero is within trigger radius (world units); it burns this many seconds, then explodes in this radius (shown as a growing ring)")] public float bomberTriggerU = 2.6f, bomberFuseS = 1.0f, bomberRadiusU = 2.4f;
        [Tooltip("Explosion damage to the hero (grows with floor damage growth) and to every other non-boss demon in the radius (flat)")] public float bomberHeroDmg = 14f, bomberEnemyDmg = 45f;
        [Tooltip("Horse-Face tempo: multiplies every boss speed, tell and rest timer (1 = Ox-Head pace; bloodrage stacks on top)")] public float horseTempo = 1.25f;
        [Tooltip("Spear thrust: locked narrow lane length, half width (world units), tell seconds, dash speed (world units / s), damage as a fraction of boss contact damage")] public float horseSpearLenU = 11f, horseSpearHalfWidthU = 0.75f, horseSpearTellS = 0.6f, horseSpearSpeed = 36f, horseSpearDmgMul = 1.1f;
        [Tooltip("Spear combo (K): thrusts in a row, and the tell seconds of every thrust after the first")] public int horseComboThrusts = 3; public float horseComboTellS = 0.38f;
        [Tooltip("Horse-Face charge: tell seconds, wall-stun seconds (Ox-Head is 3 s - Horse-Face recovers faster)")] public float horseChargeTellS = 0.6f, horseStunS = 2f;

        [Header("v0.0.37 Boss rewards (floor 1 and floor 2 bosses)")]
        [Tooltip("Seconds after the boss dies before the reward pick opens (lets the death animation play)")] public float bossRewardDelayS = 1.8f;
        [Tooltip("Reward cards offered (NEW SKILL SLOT if not maxed, rare support / upgrade, skill level)")] [Range(2, 4)] public int bossRewardCards = 3;
        [Tooltip("Bones paid when the reward pick finds nothing to offer")] public int bossRewardBones = 40;

        [Header("v0.0.38 Test menu (designer tool) and new skill sounds")]
        [Tooltip("Master switch of the in-game TEST MENU (F1 or the TEST button, top right). Turn OFF for a release build")] public bool testMenuEnabled = true;
        [Tooltip("Test menu: demons spawned this far from the hero (world units), bones the +BONES button gives, game speed of the x2 toggle")] public float testSpawnDistU = 5f; public int testBones = 100; public float testFastSpeed = 2f;
        [Tooltip("pt483u: broken / damaged decor, ice blocks and damned souls stay as left when you leave a room and come back (same run)")] public bool decorPersist53 = true;
        [Tooltip("pt483u: destroyed decor drops loot like a demon kill (never attacks)")] public bool brkDrops53 = true;
        [Tooltip("pt483u: xp souls dropped by small decor / ice / damned soul when destroyed")] public int brkDropXp53 = 2;
        [Tooltip("pt483u: xp souls dropped by a torture set piece / pit when destroyed (split into 4 drops)")] public int brkDropXpBig53 = 12;
        [Tooltip("pt483u: chance a small decor drops 1 bone")] [Range(0, 1)] public float brkDropBoneChance53 = 0.25f;
        [Tooltip("pt483u: bones dropped by a torture set piece / pit")] public int brkDropBonesBig53 = 3;
        [Tooltip("pt483u: shrine (support shop) rooms use their baked base so the altar statue shows on themed floors (Frozen / Burning / Mortar)")] public bool shrineBakedBase53 = true;
        [Tooltip("Test menu PACT tab: Set souls button sets Souls to this amount (v0.0.53 pt480u)")] public int testSoulsSet53 = 99999;
        [Tooltip("Test menu PACT tab: Set sigils button sets Sigils to this amount (v0.0.53 pt480u)")] public int testSigilsSet53 = 99;
        [Tooltip("Absolute pitch of the reused clips: Soul Spears (fireball cast), Blood Whip (frost cleave), Grave Pool (curse rune). Volumes are fixed low in HellordAudio")] public float spearSfxPitch = 1.35f, whipSfxPitch = 1.25f, poolSfxPitch = 0.8f;

        [Header("v0.0.42 Random floors - theme order and run seed")]
        [Tooltip("Shuffle the floor THEMES (Ox Pit / Frozen Hell / Burning Hell) per run. Each theme keeps its boss, hazards, demon variants, palette and music pitch. Difficulty still grows with DEPTH (1st / 2nd / 3rd floor). OFF = the old fixed order")] public bool randomFloorOrder = true; // v0.0.42
        [Tooltip("Run seed. -1 = a new random seed every run (it is printed to the log and shown in the F1 FLOOR tab). Any value >= 0 replays the same theme order, room paths, room sizes and shapes. The '-mapseed N' launch flag is used when this is -1")] public int runSeed = -1; // v0.0.42
        [Tooltip("Music pitch per theme: Ox Pit, Frozen, Burning (only one music loop exists; 1 = unchanged)")] public float[] themeMusicPitch = { 1f, 1f, 1f }; // v0.0.42

        [Header("v0.0.42 UNUSED (v0.0.43) - room path and door choice, replaced by the random floor map")]
        [Tooltip("ON = each floor is a random PATH: start -> rooms chosen through doors -> boss. OFF = the old hand-made grid layouts")] public bool randomRoomPath = false; // v0.0.43: OFF + unused by default (VH "not this type of random"); ON brings back the v0.0.42 door-choice path
        [Tooltip("Rooms between the start room and the boss room on one path (min / max, rolled per floor). The old layouts made you clear 3-4 fights + optional side rooms")] public int pathRoomsMin = 4, pathRoomsMax = 6; // v0.0.42
        [Tooltip("Exits offered when a room is cleared (min / max, rolled per step, max 3). Each exit shows the type of the room behind it")] [Range(1, 3)] public int doorChoicesMin = 2; [Range(1, 3)] public int doorChoicesMax = 3; // v0.0.42
        [Tooltip("The first room after the start is always a plain fight (Survive / Hunt / Nests)")] public bool firstRoomPlainFight = true; // v0.0.42
        [Tooltip("The last step before the boss always offers a SHOP (bone shrine) exit")] public bool shopBeforeBoss = true; // v0.0.42
        [Tooltip("Extra steps (mid floor) that are forced to offer a SHOP exit")] [Range(0, 3)] public int extraShopOffers = 1; // v0.0.42
        [Tooltip("Exits of one room never show the same room type twice (if the pool allows)")] public bool distinctDoorTypes = true; // v0.0.42
        [Tooltip("Room type weights for the random exits: Survive, Hunt, Nests, Elite, Curse, Treasure, Shop. Tuned from the old layouts (2 survive, 1-2 hunt, 1 nests, 1 elite, 1 shrine, curse or treasure)")] public float weightSurvive = 1f, weightHunt = 1f, weightNests = 0.7f, weightElite = 0.45f, weightCurse = 0.35f, weightTreasure = 0.4f, weightShop = 0.25f; // v0.0.42
        [Tooltip("Max rooms of a type OFFERED per floor: Nests, Elite, Curse, Treasure, Shop (forced shops count)")] public int maxNestsPerFloor = 2, maxElitePerFloor = 2, maxCursePerFloor = 1, maxTreasurePerFloor = 2, maxShopPerFloor = 3; // v0.0.42
        [Tooltip("Elite / Curse rooms can only be offered from this step on (1 = right after the start room)")] public int eliteMinStep = 2; // v0.0.42
        [Tooltip("Chance a floor hides a SECRET room behind a cracked north wall of one fight room on the path")] [Range(0, 1)] public float secretRoomChance = 0.6f; // v0.0.42
        [Tooltip("Show the room type (icon + name) above every exit door")] public bool doorLabels = true; // v0.0.42

        [Header("v0.0.42 UNUSED (v0.0.43) - room shapes, only used by the old door-choice path")]
        [Tooltip("Shape weights for fight rooms: plain rectangle, L (one corner is a pit), plus (all four corners are pits), pillar blocks (1-3 pits inside). Room size / aspect still comes from the small / medium / large weights")] public float shapeRectWeight = 1f, shapeLWeight = 0f, shapePlusWeight = 0f, shapePillarsWeight = 0f; // v0.0.43: off (were 0.8 / 0.5 / 0.8)
        [Tooltip("Rooms with less walkable area than this (world units^2) stay plain rectangles (the 640x368 room is ~700)")] public float shapeMinAreaU2 = 900f; // v0.0.42
        [Tooltip("Corner pit size as a fraction of the walkable width / height (min / max; kept below 0.45 so the plus shape stays connected)")] [Range(0.05f, 0.45f)] public float cornerCutMin = 0.22f; [Range(0.05f, 0.45f)] public float cornerCutMax = 0.36f; // v0.0.42
        [Tooltip("Pillar pits per room (min / max) and their size in room px (width min / max, height min / max)")] public int pillarsMin = 1, pillarsMax = 3; public int pillarWMin = 32, pillarWMax = 64, pillarHMin = 24, pillarHMax = 48; // v0.0.42
        [Tooltip("Free gap (room px) kept between a pillar pit and the walls / other pits, so every part of the room stays reachable")] public int pillarGapPx = 40; // v0.0.42
        [Tooltip("No pit within this many room px of a door, the arrival spot, the spawn, the room centre (chest / altar / stairs) or a nest / elite spot")] public int blockAvoidPadPx = 30; // v0.0.42
        [Tooltip("Walkable area limits (world units^2): room art outside them is re-rolled, and pits are dropped if the area would fall below the min (keeps densityPer100Units sane)")] public float roomWalkAreaMinU2 = 500f, roomWalkAreaMaxU2 = 3400f; // v0.0.42
        [Tooltip("Pit fill colour (placeholder, code-drawn; edges get the existing fence / skull post props)")] public Color blockFillColor = new Color(0.05f, 0.025f, 0.035f, 1f); // v0.0.42
        [Tooltip("Put existing fence / skull post props along pit edges")] public bool blockEdgeProps = true; // v0.0.42

        [Header("v0.0.43 Random floor map (generated once when a floor starts, then a normal fixed map)")]
        [Tooltip("ON = every floor gets a new random map from the run seed (room positions + room types), then it plays like the old hand-made maps: doors, go back freely, minimap. OFF = the 3 old hand-made layouts")] public bool randomFloorMap = true; // v0.0.43
        [Tooltip("Grid size the map is grown on (columns / rows). Row 0 is kept for secret rooms")] public int mapGridW = 7, mapGridH = 6; // v0.0.43
        [Tooltip("Normal rooms on a floor, start room included, boss and secret room not included (min / max, rolled per floor). Old layouts had 7-10")] public int mapRoomsMin = 8, mapRoomsMax = 11; // v0.0.43
        [Tooltip("Chance a new room may touch 2 rooms (makes a loop). 0 = pure tree with branches and dead ends")] [Range(0, 1)] public float mapLoopChance = 0.15f; // v0.0.43
        [Tooltip("Boss room is a dead end at least this many doors from the start (it is placed as far as possible)")] public int mapBossMinDist = 3; // v0.0.43
        [Tooltip("Rooms per floor, x = min, y = max: Shops (bone shrine), Elites, Treasure, Curse, Nests")] public Vector2Int mapShops = new Vector2Int(1, 1), mapElites = new Vector2Int(1, 2), mapTreasures = new Vector2Int(0, 1), mapCurses = new Vector2Int(0, 1), mapNests = new Vector2Int(1, 1); // v0.0.43
        [Tooltip("At least this many plain fight rooms (Survive / Hunt / Nests) stay on the floor; specials are skipped if needed")] public int mapMinFights = 3; // v0.0.43
        [Tooltip("Elite / Curse rooms need at least this many doors from the start (rooms next to the start are always plain fights)")] public int mapEliteMinDist = 2; // v0.0.43
        [Tooltip("Weights of the plain fight rooms: Survive vs Hunt")] public float mapSurviveWeight = 1f, mapHuntWeight = 1f; // v0.0.43
        [Tooltip("Score bonus for putting special rooms in dead ends (0 = anywhere, 1 = strongly dead ends)")] public float mapDeadEndBias = 0.6f; // v0.0.43
        [Tooltip("Score bonus for putting the shop right before the boss room")] public float mapShopNearBossBias = 0.5f; // v0.0.43
        [Tooltip("Secret room chance per floor uses secretRoomChance above (behind the cracked north wall of a fight room)")] public bool mapSecretUsesSecretChance = true; // v0.0.43 info only
    
        [Header("v0.0.44 Chill stacks / Freeze on the hero (Frost Caster bolts, Gorilla roar)")]
        [Tooltip("Hero move speed lost per Chill stack (0.15 = -15% per stack)")] [Range(0, 0.5f)] public float heroChillSlowPerStack = 0.15f; // v0.0.44
        [Tooltip("Stacks that FREEZE the hero (stacks then clear)")] public int heroChillFreezeAt = 3; // v0.0.44
        [Tooltip("Seconds the hero is frozen (cannot move, skills still fire)")] public float heroFrozenS = 1.0f; // v0.0.44
        [Tooltip("Seconds of freeze immunity after a freeze ends (no stacks gained)")] public float heroFreezeImmuneS = 3f; // v0.0.44
        [Tooltip("All Chill stacks fall off after this many seconds without a new stack")] public float heroChillDecayS = 3f; // v0.0.44

        [Header("v0.0.44 Frost Caster (Frozen Hell ranged demon, Imp based)")]
        [Tooltip("Chance a basic Imp/Ghoul spawned on a Frozen floor becomes a Frost Caster (rolled before the frost variant)")] [Range(0, 1)] public float frostCasterChance = 0.12f; // v0.0.44
        [Tooltip("HP multiplier and move speed multiplier vs the base demon")] public float frostCasterHpMul = 1.6f, frostCasterSpeedMul = 0.9f; // v0.0.44
        [Tooltip("Preferred distance band from the hero (units): backs off inside min, walks in beyond max")] public float frostCasterMinU = 5f, frostCasterMaxU = 8.5f; // v0.0.44
        [Tooltip("Seconds between shots (first shot after a random 0..this delay)")] public float frostCasterCooldownS = 2.6f; // v0.0.44
        [Tooltip("Wind-up tell before a shot (aim line + glow)")] public float frostCasterTellS = 0.6f; // v0.0.44
        [Tooltip("Ice bolt speed (units/s), radius (units), life (s)")] public float frostBoltSpeed = 5.5f, frostBoltRadiusU = 0.35f, frostBoltLifeS = 3f; // v0.0.44
        [Tooltip("Ice bolt damage (before floor growth). Each hit also adds 1 Chill stack")] public float frostBoltDmg = 5f; // v0.0.44

        [Header("v0.0.44 Fire Wheel (Burning Hell demon, Brute based)")]
        [Tooltip("Chance a basic Brute/Ghoul spawned on a Burning floor becomes a Fire Wheel (rolled before the exploding variant)")] [Range(0, 1)] public float fireWheelChance = 0.14f; // v0.0.44
        [Tooltip("HP and wander speed multipliers vs the base demon")] public float fireWheelHpMul = 1.0f, fireWheelSpeedMul = 1.0f; // v0.0.44
        [Tooltip("Starts its wind-up when the hero is within this distance (units)")] public float fireWheelTriggerU = 7f; // v0.0.44
        [Tooltip("Wind-up seconds: spins in place, the lane tell shows where it will roll")] public float fireWheelWindS = 0.7f; // v0.0.44
        [Tooltip("Roll speed (units/s) and max roll distance (units)")] public float fireWheelRollSpeed = 14f, fireWheelRollMaxU = 12f; // v0.0.44
        [Tooltip("Roll hit damage (before floor growth) and lane half-width (units)")] public float fireWheelDmg = 14f, fireWheelHalfWidthU = 0.7f; // v0.0.44
        [Tooltip("Recovery stun after the roll (stands still, easy to hit) and cooldown before the next wind-up")] public float fireWheelRecoverS = 1.2f, fireWheelCooldownS = 2.5f; // v0.0.44
        [Tooltip("Fire trail left by a roll: seconds it burns, radius per blob, damage per touch")] public float fireTrailS = 1.6f, fireTrailRadiusU = 0.45f, fireTrailDmg = 4f; // v0.0.44

        [Header("v0.0.44 Ice Gorilla (Frozen Hell boss). G glacier slam, B boulder, L leap, S belly slide, R roar")]
        [Tooltip("Attack order (loops). G Glacier Slam, B Boulder Throw, L Leap Smash, S Belly Slide, R Chest-Beat Roar")] public string gorillaCycle = "BLSGRLBG"; // v0.0.44
        [Tooltip("Attack order after enrage (Glacier Slam twice per loop)")] public string gorillaCycleEnraged = "GBLGSR"; // v0.0.44
        [Tooltip("Enrage at this HP fraction")] [Range(0, 1)] public float gorillaEnrageAt = 0.5f; // v0.0.44
        [Tooltip("Glacier Slam: rear-up tell s, shockwave ring radius (units), ring damage mul (x boss dmg)")] public float gSlamTellS = 0.9f, gSlamRingU = 3.2f, gSlamRingDmgMul = 0.9f; // v0.0.44
        [Tooltip("Glacier Slam icicles: count (enraged count), fraction aimed at/around the hero, spread around the hero (units)")] public int gIcicles = 16, gIciclesEnraged = 24; [Range(0, 1)] public float gIcicleAimedFrac = 0.4f; public float gIcicleAimSpreadU = 3f; // v0.0.44
        [Tooltip("Icicle: shadow tell s, stagger between icicles s, radius (units), damage mul (x boss dmg), seconds it stays as an obstacle")] public float gIcicleTellS = 0.8f, gIcicleStaggerS = 0.06f, gIcicleRadiusU = 0.7f, gIcicleDmgMul = 0.7f, gIcicleStayS = 1.5f; // v0.0.44
        [Tooltip("Boulder Throw: wind-up s, flight s, landing radius (units), damage mul, shards in the fan, fan half-angle deg, shard speed, shard damage mul")] public float gBoulderTellS = 0.8f, gBoulderFlightS = 0.8f, gBoulderRadiusU = 1.6f, gBoulderDmgMul = 1.1f; public int gShards = 7; public float gShardFanDeg = 50f, gShardSpeed = 9f, gShardDmgMul = 0.5f; // v0.0.44
        [Tooltip("Leap Smash: crouch s, air s (shadow tracks the hero), last part of the air time where the spot is locked s, landing ring radius (units), damage mul")] public float gLeapCrouchS = 0.5f, gLeapAirS = 1.1f, gLeapLockS = 0.4f, gLeapRingU = 2.6f, gLeapDmgMul = 1.2f; // v0.0.44
        [Tooltip("Belly Slide: lane tell s, speed, lane half-width (units), damage mul, ice trail life s, trail slow s per touch")] public float gSlideTellS = 0.8f, gSlideSpeed = 16f, gSlideHalfWidthU = 1.1f, gSlideDmgMul = 1f, gSlideTrailS = 3.5f, gSlideTrailSlowS = 0.6f; // v0.0.44
        [Tooltip("Chest-Beat Roar: tell s, ring radius (units), Chill stacks added")] public float gRoarTellS = 0.8f, gRoarRadiusU = 5f; public int gRoarStacks = 2; // v0.0.44
        [Tooltip("Rest after each Gorilla attack (s)")] public float gRecoverS = 0.8f; // v0.0.44

        [Header("v0.0.44 Qilin Chariot (Burning Hell boss). C charge, W wheels, F breath, D drift circle, M meteor hooves")]
        [Tooltip("Attack order (loops)")] public string qilinCycle = "CFWCDM"; // v0.0.44
        [Tooltip("Enrage at this HP fraction: Blazing Charge becomes a double charge")] [Range(0, 1)] public float qilinEnrageAt = 0.5f; // v0.0.44
        [Tooltip("Blazing Charge: lane tell s, re-aim tell s (2nd charge when enraged), speed, damage mul, wall stagger s")] public float qChargeTellS = 0.85f, qChargeRetellS = 0.45f, qChargeSpeed = 17f, qChargeDmgMul = 1.2f, qChargeStaggerS = 0.9f; // v0.0.44
        [Tooltip("Wheel Launch: tell s, wheels, spread deg, speed, radius (units), wall bounces, damage mul")] public float qWheelTellS = 0.7f; public int qWheels = 2; public float qWheelSpreadDeg = 25f, qWheelSpeed = 8f, qWheelRadiusU = 0.8f; public int qWheelBounces = 3; public float qWheelDmgMul = 0.8f; // v0.0.44
        [Tooltip("Flame Breath: tell s, breath s, reach (units), flame half-angle deg, total sweep deg, damage mul per tick (0.25 s)")] public float qBreathTellS = 0.8f, qBreathS = 2.1f, qBreathReachU = 10.5f, qBreathHalfDeg = 20f, qBreathSweepDeg = 120f, qBreathDmgMul = 0.45f; // t986u VH: bigger/longer Flame Breath (was 1.4s / 7u / 14deg / 90deg)
        [Tooltip("Drift Circle: ring radius around the hero (units), drive s, ring burn width (units), collapse s, safe gap deg, damage mul")] public float qDriftRadiusU = 6f, qDriftDriveS = 1.6f, qDriftWidthU = 0.6f, qDriftCollapseS = 1.4f, qDriftGapDeg = 50f, qDriftDmgMul = 0.8f; // v0.0.44
        [Tooltip("Meteor Hooves: rear tell s, fireballs min / max, tell circle s, radius (units), spread around the hero (units), damage mul")] public float qMeteorTellS = 0.6f; public int qMeteorMin = 5, qMeteorMax = 7; public float qMeteorFallS = 0.9f, qMeteorRadiusU = 1.2f, qMeteorSpreadU = 4f, qMeteorDmgMul = 0.9f; // v0.0.44
        [Tooltip("Rest after each Qilin attack (s)")] public float qRecoverS = 0.7f; // v0.0.44

        [Header("v0.0.44 Pixel FX Pack hooks (IceGorillaFX, QilinChariotFX, MinionFX, StatusHazardFX, HeroSkillFX) + Ice Gorilla art")]
        [Tooltip("Use the Pixel FX Pack for v0.0.44 bosses, Frost Caster / Fire Wheel, Chill, ice / lava hazards and the 5 newer hero skills. Off = flat placeholder decals (hero-colour placeholders are gone either way)")] public bool fxV44Pack = true; // v0.0.44
        [Tooltip("Ice Gorilla uses the PixelLab sheet (Resources/HellordArt/boss_ice_gorilla). Off or sheet missing = recoloured Ox-Head")] public bool gorillaSheet = true; // v0.0.44
        [Tooltip("Leap Smash: peak height of the Gorilla body above its shadow (units)")] public float gLeapHeightU = 5f; // v0.0.44
        [Tooltip("Leap Smash: ground shadow radius (units) at take-off / at peak (shrinks while high), and its alpha")] public float gLeapShadowU = 1.4f, gLeapShadowPeakMul = 0.6f, gLeapShadowAlpha = 0.45f; // v0.0.44
        [Tooltip("Ice skid puff: emit every N frames while the hero slides on ice")] public int fxIceSkidEveryFrames = 3; // v0.0.44
        [Tooltip("Qilin Chariot: seconds between cosmetic ember puffs on the boss while it attacks (0 = off)")] public float qEmbersS = 0.25f; // v0.0.44
        [Tooltip("Drift Circle: tell before the drive starts (s, divided by the enrage speed-up)")] public float qDriftTellS = 0.6f; // v0.0.44
        [Tooltip("Wheel Launch tell lane length (units, capped by the wall)")] public float qWheelTellLenU = 8f; // v0.0.44
        [Tooltip("Ice Gorilla: min seconds between hurt-row flinches (only outside attacks, so it never cancels a tell)")] public float gHurtAnimCooldownS = 1.5f; // v0.0.44
        [Tooltip("Ice Gorilla: extra seconds the last death frame is held before the body disappears")] public float gDeathHoldS = 0.6f; // v0.0.44
        [Tooltip("Frost Caster uses the PixelLab sheet (Resources/HellordArt/enemy_frost_caster). Off or sheet missing = recoloured base demon")] public bool frostCasterSheet = true; // v0.0.44
        [Tooltip("Fire Wheel uses its side-view sheet (Resources/HellordArt/enemy_fire_wheel) and stops spinning the transform. Off or sheet missing = recoloured base demon + rotation")] public bool fireWheelSheet = true; // v0.0.44
        [Tooltip("Fire Wheel: roll-row playback speed while it wanders (1 = 60 ms frames like the attack roll)")] public float fireWheelWanderAnimMul = 0.5f; // v0.0.44
        [Tooltip("Qilin Chariot uses the PixelLab sheet (Resources/HellordArt/boss_qilin_chariot). Off or sheet missing = recoloured Ox-Head")] public bool qilinSheet = true; // v0.0.44
        [Tooltip("Qilin mouth offset from the feet (units) for the fire-breath origin: facing S, facing E (x mirrored for W), facing N")] public Vector2 qMouthS = new Vector2(0f, 3.8f), qMouthE = new Vector2(3.7f, 3.2f), qMouthN = new Vector2(0f, 4.5f); // v0.0.44
        [Tooltip("Qilin sheet: stomp hit frame (S/E, N). Frames before it stretch over the Meteor Hooves tell, so meteors spawn on this frame")] public int qStompHitF = 6, qStompHitFN = 6; // v0.0.44
        [Tooltip("Qilin sheet: breath release frame (S/E, N). Frames before it stretch over the breath tell; release frames play across the breath")] public int qBreathReleaseF = 4, qBreathReleaseFN = 4; // v0.0.44
        [Tooltip("Meteor Hooves: seconds after the stomp hit before the attack ends (plays the settle frames; sheet native is 0.5)")] public float qMeteorAfterS = 0.6f; // v0.0.44
        [Header("v0.0.44 Frozen Hell floor B tileset (Resources/HellordArt/FrozenHell, sizes / weights / ms from frozen_tileset.json)")]
        [Tooltip("Frozen Hell (theme 1) rooms use the floor B tileset: composed floor + 48px north wall + sides + AO, and the animated ice patch sprite on the slide hazard. Off or art missing = recoloured Ox Pit bake + FX-pack / flat ice")] public bool frozenTilesB = true; // v0.0.44
        [Tooltip("Frozen floor B: first variant index counted as an accent (json order; 0-3 = B_orig, 4-8 = B_new). Accents never touch another accent")] public int frozenFloorAccentFrom = 4; // v0.0.44
        [Tooltip("Frozen floor B: random flipX per 32px floor tile")] public bool frozenFloorFlipX = true; // v0.0.44
        [Tooltip("Frozen floor B: north face pick weights in sheet order (plain_0, plain_1, damage, icicle, chain). Default = plain 70%, others 10% each")] public int[] frozenFaceWeights = { 35, 35, 10, 10, 10 }; // v0.0.44
        [Tooltip("Frozen floor B: sprite colour on the composed base (white = native palette, ignores the theme floor tint)")] public Color frozenTilesBTint = Color.white; // v0.0.44
        [Tooltip("Frozen floor B ice patch: sprite size multiplier vs the hazard ellipse (1 = 64x28 art stretched to the slide area), and alpha")] public float frozenIceScale = 1f, frozenIceAlpha = 1f; // v0.0.44
            [Tooltip("Qilin Chariot: min seconds between hurt-row flinches (never during an attack or stun)")] public float qHurtAnimCooldownS = 1.5f; // v0.0.44
        [Tooltip("Qilin Chariot: extra seconds the last death frame (corpse) is held before the body disappears")] public float qDeathHoldS = 0.6f; // v0.0.44
            [Tooltip("Burning Hell (theme 2) rooms use the Burning G tileset (composed floor + walls + AO) and lava crack A grid pieces on the lava hazard. Off or art missing = old bake + FX-pack / line lava.")] public bool burningTilesG = true; // v0.0.44
        [Tooltip("Random flipX on Burning G floor tiles.")] public bool burningFloorFlipX = true; // v0.0.44
        [Tooltip("Burning G floor: never pick the off-style variants in burningFloorSkipVariants (soft/grainy/shaded tiles that don't match the crack tiles). Off = all 11 variants like v0.0.44")] public bool burningFloorMatchV46 = true; // v0.0.46
        [Tooltip("Burning G floor variant indexes (json order, 0-10) skipped when burningFloorMatchV46 is on. 1 = soft no-outline slabs, 3 = grainy scratched, 7 = blurred blobs, 9 = dark shaded slab")] public int[] burningFloorSkipVariants = { 1, 3, 7, 9 }; // v0.0.46
        [Tooltip("Burning G floor: use the v0.0.48 soft cracked stone sheet (step35b, 13 tiles: base_0-9 + crack/scorch/ember accents, all can spawn). Off = old v0.0.44 crack sheet (burningFloorMatchV46 / burningFloorSkipVariants apply only then)")] public bool burningFloorV48 = true; // v0.0.48
        [Tooltip("v0.0.48 Burning floor: pick weight for each accent tile (crack, scorch, ember) vs 10 for each base tile. 0 = no accents")] public int burningFloorAccentWeight = 2; // v0.0.48
        [Tooltip("v0.0.48: each boss plays its own music track (Audio/Music/Boss). Off = old floor 1 combat loop through boss fights")] public bool bossMusicV48 = true; // v0.0.48
        [Tooltip("v0.0.48 boss track BPM (bar-line sync now, pestle sync later): Ox-Head, Horse-Face P1, Horse-Face P2, Ice Gorilla, Qilin Chariot")] public float[] bossMusicBpm = { 180f, 140f, 155f, 150f, 160f }; // v0.0.48
        [Tooltip("v0.0.48: Horse-Face P1 -> P2 switch waits for the next bar line of the P1 track. Off = switch on the phase-change frame")] public bool bossMusicBarSync = true; // v0.0.48
        [Tooltip("v0.0.48: beats per bar for the bar-line sync")] public int bossMusicBeatsPerBar = 4; // v0.0.48
        [Tooltip("v0.0.48: crossfade seconds floor <-> boss music")] public float bossMusicXfadeS = 1.0f; // v0.0.48
        [Tooltip("v0.0.48: crossfade seconds boss -> boss (Horse-Face P1 -> P2 hard tempo change). 0 = hard cut on the bar line")] public float bossMusicPhaseXfadeS = 0.25f; // v0.0.48
        [Tooltip("v0.0.48: boss track volume multiplier on top of musicVolume")] public float bossMusicVolumeMul = 1.0f; // v0.0.48
        [Tooltip("Lights only from real sources: drop the room json's sourceless oxpit_light_pool_warm pools (and shadow pools, see lightsDropShadowPools). Off = v0.0.45 lighting")] public bool lightsSourcedV46 = true; // v0.0.46
        [Tooltip("With lightsSourcedV46: also drop the sourceless dark oxpit_shadow_pool blotches")] public bool lightsDropShadowPools = true; // v0.0.46
        [Tooltip("With lightsSourcedV46: strength x scale of each brazier light sprite (smaller, consistent glow)")] public float brazierLightK46 = 0.7f, brazierLightScale46 = 0.75f; // v0.0.46
        [Tooltip("With lightsSourcedV46: brazier fire pool radius px, additive strength, smooth dithered falloff instead of 4 hard bands")] public int firePoolRadiusPx46 = 26; // v0.0.46
        [Tooltip("With lightsSourcedV46: brazier fire pool additive strength")] public float firePoolStrength46 = 0.2f; // v0.0.46
        [Tooltip("With lightsSourcedV46: fire pool uses a smooth Bayer-dithered falloff (no visible bands)")] public bool firePoolSmooth46 = true; // v0.0.46
        [Tooltip("With lightsSourcedV46: no red edge fog vignette (it is not anchored to any source). Low-HP edge pulse is a warning and stays")] public bool lightsNoEdgeFog46 = true; // v0.0.46
        [Tooltip("With lightsSourcedV46: small dithered glow on each skull torch prop (oxpit_prop_skullpost flame)")] public bool torchGlow46 = true; // v0.0.46
        [Tooltip("Skull torch glow radius px and additive strength")] public int torchGlowRadiusPx46 = 16; public float torchGlowK46 = 0.22f; // v0.0.46
        [Tooltip("With lightsSourcedV46: small dithered glow under each Burning lava crack cell")] public bool lavaGlow46 = true; // v0.0.46
        [Tooltip("Lava cell glow radius px and additive strength")] public int lavaGlowRadiusPx46 = 18; public float lavaGlowK46 = 0.14f; // v0.0.46
        [Tooltip("Lava / torch glow colour")] public Color sourceGlowColor46 = new Color(0.97f, 0.46f, 0.13f, 1f); // v0.0.46
        [Header("t586u V2 light mask (step38v): white light on hero + sources, full floor colour")]
        [Tooltip("V2 light mask on: floors keep full colour (d2Ambient606 skipped), orange glow sprites and the vignette are hidden, and a per-frame light mask (hero pool + torch pools, white) darkens everything outside the light")] public bool v2Light586 = true;
        [Tooltip("Ambient light outside every pool (0 = black edges)")][Range(0f, 0.3f)] public float v2Amb586 = 0.03f;
        [Tooltip("Hero pool (legacy view fractions; used only when v2HeroWorld1041=false): flat to r0, fade to r1 of view width")] public float v2HeroR0_586 = 0.22f, v2HeroR1_586 = 0.45f;
        [Tooltip("t1041u: hero pool in world units so zoom/full-room does not inflate coverage. Calibrated to old 0.22/0.45 at ref view width 40wu (zoom2 @1280x720)")] public bool v2HeroWorld1041 = true;
        [Tooltip("t1041u: hero flat / fade radii in world units (1u=16px)")] public float v2HeroR0Wu1041 = 8.8f, v2HeroR1Wu1041 = 18f;
        [Tooltip("Torch/brazier/lava pool radius (fraction of view width; 0.10 = 64 art px at 640x360) and strength before saturation 1-exp(-sum)")] public float v2TorchR586 = 0.10f, v2TorchK586 = 1.4f;
        [Tooltip("White additive core on each source: radius as a fraction of the torch radius, and strength")] public float v2CoreR586 = 0.07f, v2CoreK586 = 0.08f; // t1034u: dim firefly white cores (was 0.18/0.4 → 0.10/0.16 still hot)
        [Tooltip("Vertical distance scale (the mock-up uses 1.3: pools are a bit flatter than circles)")] public float v2YScale586 = 1.3f;
        [Tooltip("Light mask resolution divisor vs the low-res world (4 -> 160x90 at 640x360, bilinear)")][Range(1, 8)] public int v2MaskDiv586 = 4;
        [Tooltip("t660u: player projectiles and archer arrows emit light in the V2 layer")] public bool v2ProjLight660 = true;
        [Tooltip("t991u: stamp torch/proj lights into the mask (O(S*r^2)) instead of O(W*H*S). Same look; big FPS win when casting Fireball")] public bool v2LightStamp991 = true;
        [Tooltip("t991u: max dynamic skill light sources (projectiles+orbs+flashes) stamped per frame. 0 = old 48. Torches uncapped")] public int v2DynLightCap991 = 12;
        [Tooltip("v0.0.64 t674u: player projectile light weight (x torch K); was 0.5, -25%")] public float v2ProjK660 = 0.38f;
        [Tooltip("t669u: SkillLightFX v2.14.0 pixel lights (PAUSED by VH, keep false)")] public bool skillLightFX667 = false;
        [Tooltip("v0.0.64 t674u: Bone Orbit orb light weight (x torch K); was 0.35, -23%")] public float v2OrbK661 = 0.27f;
        // ===== t677u (VH feedback on v0.0.65)
        [Tooltip("t677u: CRT pass master switch (scanlines/mask/curvature/bloom). Launch arg -nocrt also turns it off.")] public bool crtEnabled677 = true;
        [Tooltip("t991u: run the 2 phosphor-glow Blits (off = CRT scanlines/mask/curve stay, glow=0 — FPS win when G=0 or for testing)")] public bool crtGlowBlit991 = true;
        [Tooltip("t991u: build phosphor glow at half RT resolution (bilinear). Same look at less fill-rate")] public bool crtGlowHalf991 = true;
        [Tooltip("t677u: above this screen height (px) the CRT is reduced. 1440 = 4K/3840x2160 gets no scanlines/curvature.")] public int crtMaxHeight677 = 1440;
        [Tooltip("t677u: above crtMaxHeight677: 0 = CRT shader fully off (plain upscale), 1 = keep bloom/fog/vignette but no scanlines/RGB mask/curvature/chroma")][Range(0, 1)] public int crtHiResMode677 = 1;
        [Tooltip("t677u: max extra light skill flashes/projectiles can add on top of hero+torch light (0..1). Keeps skill light a local glow, not a white disc.")][Range(0, 1)] public float skillLightCap677 = 0.35f;
        [Tooltip("t677u: how much the hero's own light suppresses skill light/tint where the hero already lights the floor (1 = no skill light on top of full hero light)")][Range(0, 1)] public float skillHeroDamp677 = 0.7f;
        [Tooltip("t677u: max impact flashes added per frame (big AoE hits used to add one per enemy)")] public int flashPerFrame677 = 4;
        // ===== t738u (VH: full curse build + Echo = huge rune, FPS drop)
        [Tooltip("t738u: Curse Rune area bonuses (Bigger Area, Intensity, Sloth Throne, Grave Reach...) ADD instead of multiply")] public bool curseAreaAdditive738 = true;
        [Tooltip("t738u: max total Curse Rune area multiplier over its level radius")] public float curseAreaMaxMul738 = 1.7f;
        [Tooltip("t738u: hard cap on Curse Rune radius in world units (Lv5 base is 4.6)")] public float curseRadiusMaxU738 = 7.0f;
        [Tooltip("t738u: max Echo repeats for Curse Rune (Echo Lv5 is 3 for other skills)")] public int curseEchoMax738 = 1;
        [Tooltip("t738u: Echo repeat of Curse Rune: radius x this")] public float curseEchoRadiusMul738 = 0.75f;
        [Tooltip("t738u: Echo repeat of Curse Rune: damage x this (on top of Echo dmg)")] public float curseEchoDmgMul738 = 0.8f;
        [Tooltip("t738u: Echo repeat of Curse Rune: runes placed (main cast places 1-3)")] public int curseEchoRunes738 = 1;
        [Tooltip("t738u: max lingering curse zones alive at once (extra runes still burst, no zone)")] public int curseZoneMax738 = 6;
        [Tooltip("t738u: curse zone damage tick seconds (was 0.4 = 1.6 s / 4)")] public float curseZoneTick738 = 0.5f;
        [Tooltip("t738u: rune/zone sprite radius snapped to this many px so new radii don't build new textures every floor")] public int curseRadiusPxStep738 = 8;
        [Tooltip("t738u: max cursed-death explosion visuals per frame (damage still applies to all)")] public int curseBlastFxPerFrame738 = 4;
        [Tooltip("t677u: impact flash strength multiplier for big AoE skills (Hellfire Pillar, Storm Ring)")][Range(0, 1)] public float aoeFlashMul677 = 0.4f;
        [Tooltip("t677u: skill-coloured sources get no white additive core (white core = torches/braziers only)")] public bool noSkillCore677 = true;
        [Tooltip("t677u: Mortar floors only: floor grade multiplier (1 = old look). Other floors unchanged.")] public float mortarGradeK677 = 0.72f;
        [Tooltip("t677u: Mortar floors only: V2 ambient multiplier (1 = old)")] public float mortarAmbK677 = 0.5f;
        [Tooltip("t677u: boss room size factor (1 = old 1536x864). Applies on tiled floors (Frozen/Burning/Mortar); the Ox Pit baked boss art keeps its size.")][Range(0.5f, 1f)] public float bossRoomScale677 = 0.75f;
        [Tooltip("v0.0.64 t674u: SkillLightFX.Intensity applied when the pixel-light pack is enabled (1 = pack default; -25%)")][Range(0, 1)] public float skillLightIntensity674 = 0.75f;
        [Tooltip("v0.0.64 t674u: element tint strength of skill lights; was 0.55, -24%")][Range(0, 1)] public float v2TintK661 = 0.42f;
        [Tooltip("v0.0.64 t674u: impact flash peak weight (x torch K); was 0.9, -24%")] public float v2FlashK660 = 0.68f;
        [Tooltip("t660u: impact flash duration s")] public float v2FlashS660 = 0.15f;
        [Tooltip("Colour of the dark outside the light")] public Color v2DarkColor586 = new Color(0f, 0f, 0f, 1f);
        [Header("t606u Diablo 2 darkness (v0.0.61)")]
        [Tooltip("Master switch for the t606u D2 darkness pass. Off = v0.0.60 lighting")] public bool d2Dark606 = true; // v0.0.61 t606u
        [Tooltip("Ambient darkness: multiplies the floor/prop grade (gameplay sprites never graded). 1 = v0.0.60 brightness, lower = darker")][Range(0.2f, 1f)] public float d2Ambient606 = 0.30f; // t571u M 0.42 -> 0.30 // v0.0.61 t606u 0.6 -> t607u D target 0.42
        [Tooltip("Shadow lift multiplier under the darkness pass (lower = deeper blacks; 0 lets shadows crush)")][Range(0f, 1f)] public float d2ShadowLiftMul606 = 0.25f; // v0.0.61 t606u 0.4 -> t607u D 0.25
        [Tooltip("Hero light radius vignette: darkness strength at the screen edge (camera follows the hero, so it is a light circle around him)")][Range(0f, 1f)] public float d2VignetteAmt606 = 0.90f; // t571u M 0.82 -> 0.90 (edges stay dark) // v0.0.61 t606u 0.7 -> t607u D 0.82
        [Tooltip("Vignette inner radius (0..1 of half-screen, where the dark starts). Lower = tighter light circle")][Range(0f, 1f)] public float d2VignetteInner606 = 0.34f; // t571u M 0.22 -> 0.34 (hero light ~190 art px) // v0.0.61 t606u 0.3 -> t607u D 0.22 (tight hero light)
        [Tooltip("Vignette colour: Endesga #181425 near-black purple")] public Color d2VignetteColor606 = new Color(0.094f, 0.078f, 0.145f, 1f); // v0.0.61 t606u
        [Tooltip("Brazier fire pool radius px with the darkness pass (was firePoolRadiusPx46 = 26)")] public int d2FirePoolRadiusPx606 = 100; // t571u M 18 -> 100 // v0.0.61 t606u 20 -> t607u D 18
        [Tooltip("Brazier fire pool additive strength with the darkness pass (was firePoolStrength46 = 0.2)")] public float d2FirePoolK606 = 0.30f; // t571u M 0.36 -> 0.30 (bigger pool, keep units readable) // v0.0.61 t606u 0.28 -> t607u D 0.36 (hot core vs darker ambient)
        [Tooltip("Skull torch glow radius px / strength with the darkness pass (was 16 / 0.22)")] public int d2TorchRadiusPx606 = 100; public float d2TorchK606 = 0.26f; // t571u M radius 12 -> 100, k 0.36 -> 0.26 // v0.0.61 t606u 0.30 -> t607u D 0.36
        [Tooltip("Room warm/brazier light sprite strength multiplier with the darkness pass (was roomLightBoost 0.9)")] public float d2RoomLightBoost606 = 0.7f; // v0.0.61 t606u 0.75 -> t607u D 0.7
        [Tooltip("Torch/lava glow colour with the darkness pass: warmer orange (was #f77622-ish 0.97,0.46,0.13)")] public Color d2GlowColor606 = new Color(0.97f, 0.40f, 0.10f, 1f); // v0.0.61 t606u
        [Tooltip("Ember mote alpha multiplier (fewer bright specks washing the dark)")][Range(0f, 1f)] public float d2EmberAlpha606 = 0.6f; // v0.0.61 t606u 0.7 -> t607u D 0.6
        [Tooltip("Dark backing box behind the hell floor name. Off = text only with its 1px shadow")] public bool hudFloorNameBg = false; // v0.0.46
        [Tooltip("Tint on the composed Burning G room base (white = art as drawn).")] public Color burningTilesGTint = Color.white; // v0.0.44
        [Tooltip("Burning G lava crack length in 32px cells, min (end pieces included, >= 2).")] public int lavaGLenMin = 3; // v0.0.44
        [Tooltip("Burning G lava crack length in 32px cells, max.")] public int lavaGLenMax = 6; // v0.0.44
        [Tooltip("Chance per step that a Burning G crack turns 90 degrees (makes a bend).")] [Range(0, 1)] public float lavaGTurnChance = 0.3f; // v0.0.44
        [Tooltip("Burning G wake/tell length in seconds before a crack goes active (0 = json 390 ms). Wake frames stretch to fill it.")] public float lavaGTellS = 0f; // v0.0.44
        [Tooltip("Burning G lava hit distance (units) from the crack centre line while active.")] public float lavaGHitHalfU = 0.55f; // v0.0.44
        [Tooltip("Low walls + per-side doors (Ox Pit v30d, Frozen/Burning v32): 32px side walls, 24px bottom wall, each door side its own drawing (no rotation). Off = old 24/16px walls and the rotated ui_door.")] public bool lowWallsV30 = true; // v0.0.44
        [Tooltip("Side wall thickness in room px when low walls are on (art is 32 wide).")] public int lowWallSidePx = 32; // v0.0.44
        [Tooltip("Bottom wall thickness in room px when low walls are on (art is 24 tall).")] public int lowWallBottomPx = 24; // v0.0.44
        [Tooltip("Soft additive glow (door_glow_soft_64x64) on open low-wall doors.")] public bool lowDoorGlow = true; // v0.0.44
        [Tooltip("Strength of the open-door glow.")] [Range(0, 2)] public float lowDoorGlowK = 0.8f; // v0.0.44
        [Tooltip("Approved step26 icons: own 24px tiles for Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool and 20px Executioner, Soul Harvest, Explode on Kill. Off = old tinted stand-ins with 2-letter tags.")] public bool newIconsV44 = true; // v0.0.44
        [Tooltip("HUD corners: skill bar (3 skill+support panels, FREE SLOT included) bottom-left, blood/bone counters bottom-right. Off = old centred bar + counters bottom-left")] public bool hudCornerV45 = true; // v0.0.45
        [Tooltip("Skill bar gap from the canvas left edge, 640x360 canvas px")] public float hudSkillBarLeftPx = 4; // v0.0.45
        [Tooltip("Skill bar gap from the canvas bottom edge, 640x360 canvas px")] public float hudSkillBarBottomPx = 4; // v0.0.45
        [Tooltip("Goal box (top centre) scale, whole numbers only so pixels stay crisp. 2 = 88+ x 26 px")] public float hudGoalScale = 2; // v0.0.45
        [Tooltip("Show the hell floor name (OX PIT / FROZEN HELL / BURNING HELL) under the minimap")] public bool hudFloorName = true; // v0.0.45
        [Tooltip("Hell floor name scale (whole number, crisp). Drawn right-aligned under the minimap and its FLOOR label")] public float hudFloorNameScale = 1; // v0.0.46 (was 2 in v0.0.45)
        [Tooltip("Gap between the minimap frame bottom and the floor name backing, canvas px. The FLOOR n/N label moves below the name")] public float hudFloorNameGapPx = 1; // v0.0.45
        [Tooltip("Show the FLOOR n/N label under the minimap. Off by default: the big floor name replaces it")] public bool hudFloorNumber = false; // v0.0.45
        [Tooltip("Show the NO MORE DEMONS COMING banner when the goal is met. Off by default: it covered enemies mid-fight")] public bool goalMetBanner = false; // v0.0.45
        [Tooltip("LV / XP bar left edge, 640x360 canvas px. 4 = same left x as the HP widget (old 17)")] public float hudXpBarX = 4; // v0.0.45
        [Tooltip("Blood/bone counters left edge, 640x360 canvas px (sits under the LV / XP bar)")] public float hudCounterLeftPx = 4; // v0.0.45
        [Tooltip("Blood/bone counters top edge, 640x360 canvas px (XP bar ends at y 30)")] public float hudCounterTopPx = 33; // v0.0.45
        [Tooltip("Minimap + FLOOR label scale around the top-right corner (1 = old size). 1.5 = 3 screen px per map px at 2x zoom")] public float hudMinimapScale = 1.5f; // v0.0.45
        [Header("v0.0.46 per-boss projectile patterns")]
        [Tooltip("Each boss fires its own pattern set (Ox Horn Toss / Stomp Quake / Bull Rush Wake, Gorilla Icicle Rain / Boulder Roll / Shard Fan / slam spikes, Qilin Hoof Meteors / Wheel Spokes / flame-trail fireballs / rage Fire Wheels, Frost Ox puffs, Horse-Face lasso). Off = the old shared ring / fan / double ring / spiral cycle")] public bool bossPatternsV46 = true; // v0.0.46
        [Tooltip("Seconds between patterns: phase 1, phase 2, rage (divided by the floor pattern-rate growth)")] public float bp46CdP1 = 3.2f, bp46CdP2 = 2.6f, bp46CdRage = 2.0f; // v0.0.46
        [Tooltip("Rage tempo: tells and the pattern cooldown are multiplied by this (tells never go under bp46MinTellS)")] public float bp46RageTempo = 0.85f; // v0.0.46
        [Tooltip("Shortest allowed tell (s) for any pattern")] public float bp46MinTellS = 0.5f; // v0.0.46
        [Tooltip("Most different pattern types allowed on screen at once")] public int bp46MaxTypes = 2; // v0.0.46
        [Tooltip("Hero width (units) used for the dodge-gap rule, and the gap multiplier (ring / lasso gaps are at least width x mul at the hero's distance)")] public float bp46HeroWidthU = 1.0f, bp46GapMul = 1.5f; // v0.0.46
        [Tooltip("Hit radius of a normal pattern bullet (units) and damage multiplier vs bossBulletDamage")] public float bp46BulletR = 0.3f, bp46DmgMul = 1f; // v0.0.46
        [Tooltip("Pattern bullets die after this many seconds")] public float bp46LifeS = 8f; // v0.0.46
        [Tooltip("OX Horn Toss: chunks phase 1 / phase 2, chunk speed, spread deg between chunks, tell s, max throw range (units), shards per chunk, shard speed, chunk hit radius")] public int oxTossChunks = 3, oxTossChunksP2 = 5; public float oxTossSpeed = 3.5f, oxTossSpreadDeg = 25f, oxTossTellS = 0.6f, oxTossRangeU = 7f; public int oxTossShards = 4; public float oxTossShardSpeed = 4.5f, oxTossChunkR = 0.45f; // v0.0.46
        [Tooltip("OX Stomp Quake: rings phase 1 / phase 2, bullets per ring, ring speed, seconds between rings, gap deg facing the hero, tell s, rage gap rotation deg per ring")] public int oxQuakeRings = 2, oxQuakeRingsP2 = 3, oxQuakeBullets = 26; public float oxQuakeSpeed = 4f, oxQuakeEveryS = 0.35f, oxQuakeGapDeg = 60f, oxQuakeTellS = 0.7f, oxQuakeRageRotDeg = 35f; // v0.0.46
        [Tooltip("OX Bull Rush Wake: seconds between ember drops while charging, embers per side, sideways speed, ember life s, rage homing acceleration toward the hero (units/s2)")] public float oxWakeEveryS = 0.15f; public int oxWakePerSide = 2; public float oxWakeSpeed = 2f, oxWakeLifeS = 2.5f, oxWakeRageHome = 2.5f; // v0.0.46
        [Tooltip("GORILLA Icicle Rain: min / max icicles, phase 2 count (staggered grid), scatter radius around the hero, grid spacing, shadow grow s (tell), impact radius, rage second-wave delay s")] public int gRainMin = 6, gRainMax = 8, gRainP2 = 10; public float gRainScatterU = 3.2f, gRainGridU = 2.2f, gRainGrowS = 0.9f, gRainR = 0.8f, gRainWave2S = 0.5f; // v0.0.46
        [Tooltip("GORILLA Boulder Roll: speed, radius, wall bounces before shattering, shatter shards, shard speed, tell s, phase 2 V half-angle deg, rage shards per bounce")] public float gRollSpeed = 4f, gRollR = 0.75f; public int gRollBounces = 2, gRollShards = 6; public float gRollShardSpeed = 4.5f, gRollTellS = 0.8f, gRollVDeg = 20f; public int gRollRageBounceShards = 4; // v0.0.46
        [Tooltip("GORILLA Shard Fan: shards (rage count), fan width deg, speed, tell s, phase 2 mirrored fans offset deg each side of the hero")] public int gFanN = 5, gFanNRage = 7; public float gFanDeg = 30f, gFanSpeed = 7f, gFanTellS = 0.5f, gFanMirrorDeg = 35f; // v0.0.46
        [Tooltip("GORILLA Glacier Slam spikes: count, ring radius (units), life s, block radius (units), damage mul if the hero is on one when it rises")] public int gSpikeN = 8; public float gSpikeRingU = 3.5f, gSpikeLifeS = 1f, gSpikeR = 0.45f, gSpikeDmgMul = 0.5f; // v0.0.46
        [Tooltip("QILIN Hoof Meteors: count phase 1 / phase 2 (X), tell s (orange circle), stagger s, impact radius, puddle life s, puddle damage per tick mul, puddle tick s, X arm spacing (units), rage follow interval s")] public int qMetN = 4, qMetNP2 = 6; public float qMetTellS = 0.8f, qMetStaggerS = 0.15f, qMetR = 1f, qMetPuddleS = 1.5f, qMetPuddleDmgMul = 0.35f, qMetPuddleTickS = 0.5f, qMetXSpacingU = 1.8f, qMetRageEveryS = 0.3f; // v0.0.46
        [Tooltip("QILIN Wheel Spokes: spokes phase 1 / phase 2, fireballs per spoke, spacing along a spoke (units), rotation deg/s, outward speed, tell s, rage reverses rotation after this many s")] public int qSpokeN = 6, qSpokeNP2 = 8, qSpokeBalls = 4; public float qSpokeStepU = 0.6f, qSpokeDegS = 40f, qSpokeSpeed = 3f, qSpokeTellS = 0.6f, qSpokeRageRevS = 1.5f; // v0.0.46
        [Tooltip("QILIN Flame Trail Charge: fireball volleys per charge, fireballs per side per volley, sideways speed, seconds between volleys")] public int qTrailVolleys = 3, qTrailPerSide = 1; public float qTrailSpeed = 3f, qTrailEveryS = 0.2f; // v0.0.46
        [Tooltip("QILIN rage: every Nth pattern is a summon of Fire Wheel demons instead of bullets; wheels per summon; summon tell s; max Fire Wheels alive")] public int qRageSummonEvery = 2, qRageWheels = 2; public float qRageSummonTellS = 0.6f; public int qRageWheelsMax = 4; // v0.0.46
        [Tooltip("FROST OX Frost Puffs: puffs per cone, cone width deg, speed, puff radius, tell s, phase 2 / rage waves, seconds between waves, chill stacks on hit")] public int foPuffN = 7; public float foPuffDeg = 50f, foPuffSpeed = 2.2f, foPuffR = 0.45f, foPuffTellS = 0.6f; public int foPuffWavesP2 = 2; public float foPuffWaveS = 0.4f; public int foPuffChill = 1; // v0.0.46
        [Tooltip("HORSE-FACE Lasso: beads per arm, lasso radius around the hero (units), closing speed deg/s, closed gap deg (min, raised to the dodge-gap rule), tell s, bead life after closing s")] public int hLassoBeads = 9; public float hLassoR = 4.5f, hLassoDegS = 70f, hLassoGapDeg = 40f, hLassoTellS = 0.6f, hLassoHoldS = 0.6f; // v0.0.46
        [Tooltip("Shared boss attack scheduler: a v0.0.46 pattern only starts while the boss walks (never during a melee move, its tell or its recovery), and a melee move waits until the pattern's tell and volley are done. Off = patterns may overlap melee like before")] public bool bossSchedV46 = true; // v0.0.46
        [Tooltip("Scheduler: seconds the boss must have been walking after a melee move ends before a pattern may start")] public float bossGapAfterMeleeS = 0.6f; // v0.0.46
        [Tooltip("Scheduler: seconds after a pattern's volley is fired before the next melee move may start (the bullets are still flying)")] public float bossGapAfterPatternS = 0.9f; // v0.0.46
        [Tooltip("Scheduler: the walk timer is topped up to at least this many seconds when a pattern ends, so melee never fires on the same frame")] public float bossMeleeMinWalkS = 0.4f; // v0.0.46
        [Tooltip("Pattern riders that fire DURING a melee move (Ox Bull Rush Wake while charging, Qilin Flame Trail while charging). Off by default with the scheduler, since they are what made melee feel crowded")] public bool bossChargeRidersV46 = true; // v0.0.46
        [Tooltip("Legacy Ox stomp add-on: the PatternWarnFX ring burst of bullets after the stomp shockwave (replaced by Stomp Quake). Only used when bossPatternsV46 is off or this is on")] public bool oldStompBurstV46 = false; // v0.0.46
        [Tooltip("Legacy Ice Gorilla slam icicles (replaced by Icicle Rain). Only used when bossPatternsV46 is off or this is on")] public bool gorillaSlamIciclesV46 = false; // v0.0.46
        [Tooltip("Legacy Qilin Hoof Meteors melee move (replaced by the Hoof Meteors pattern). Only used when bossPatternsV46 is off or this is on")] public bool qilinOldMeteorsV46 = false; // v0.0.46
        [Tooltip("Crisp pixel boss tells: 1px dark outline + 1px boss-colour rim, dithered fill that grows over the tell, white flash at the end, built at the game's 16 px/unit and snapped to the pixel grid. v0.0.46 spec tells: cones and lanes / charges use the Hellord FX design, circles and rings the MoneyMaker design. Also hides the soft Pixel FX Pack tell glows for the slam / roar / wheels / drift / charge. Off = old scaled soft decals")] public bool crispTellsV46 = true; // v0.0.46
        [Tooltip("v0.0.46 tells: hide the Pixel FX Pack's own ground tells for Qilin breath, Gorilla boulder, icicles and meteors (their projectiles / fire still play), so the spec tells draw there instead")] public bool packTellsHideV46 = true; // v0.0.46
        [Tooltip("v0.0.46 tells: a tell that stops being drawn at or past this progress plays its release (hit flash / ring wave / lane streak); earlier = cancelled, removed at once")] public float tellReleaseMinP46 = 0.9f; // v0.0.46
        [Tooltip("Ox Quake: radius (units) of the closing-ring tell (MoneyMaker spec ring R = 112 px = 7 units)")] public float oxQuakeTellRU = 7f; // v0.0.46
        [Tooltip("Qilin breath: share of the mouth's height offset that is treated as ground depth when the cone is projected to the floor (0 = cone starts at the feet, 1 = at the mouth sprite)")] public float qBreathGroundYMul46 = 0.35f; // v0.0.46
        [Tooltip("QILIN on-screen size: body sprite scale (1 = the raw 160x144 PixelLab cell, ~2x the Ice Gorilla). Hitbox, mouth and FX offsets scale with it")] public float qilinScale46 = 0.6f; // v0.0.46
        [Tooltip("QILIN hit radius and hurt-centre height (units) at scale 1; multiplied by qilinScale46")] public float qilinHitRadiusAt1 = 2f, qilinHitYAt1 = 2.5f; // v0.0.46
        [Tooltip("FX redo: draw the ORIGINAL 3 px Hellfire Pillar instead of MoneyMaker's Task 4 remake (crack tell, squash dome, tall spike, shock ring)")] public bool fxClassicPillar46 = false; // v0.0.46
        [Tooltip("FX redo: draw the ORIGINAL 3 px Blood Whip instead of MoneyMaker's v5 octopus tentacle")] public bool fxClassicWhip46 = false; // v0.0.46
        [Tooltip("FX redo: draw the ORIGINAL 3 px ice patch instead of MoneyMaker's Ice v2 (frost spread, facets, sheen, shards)")] public bool fxClassicIce46 = false; // v0.0.46
        [Tooltip("FX redo: draw the ORIGINAL 3 px lava crack instead of MoneyMaker's Lava v2 (navy seam, heating warning, molten channel)")] public bool fxClassicLava46 = false; // v0.0.46
        [Tooltip("Lava v2 ACTIVE: width multiplier of the bright molten channel (1 = MoneyMaker mock, lower = thinner; visual only, the hit width is lavaWidth)")] [Range(0.3f, 1.5f)] public float lavaActiveWidthMul46 = 0.8f; // v0.0.46
        [Tooltip("Lava v2 ACTIVE: alpha multiplier of the orange floor glow wash and heat shimmer (1 = MoneyMaker mock, 0 = off)")] [Range(0f, 2f)] public float lavaGlowAlphaMul46 = 0.6f; // v0.0.46

        [Header("v0.0.47 36 px hero, trait overlays v14, chill / freeze 36 (Resources/HellordArt/Hero36)")]
        [Tooltip("Use the approved 36x36 hero (hero36_final) with v14 trait overlays and the step32 chill art. Off = the old 32x32 hero_base, old hero_up layers and the Pixel FX Pack chill pips / ice shell")] public bool hero36V47 = true; // v0.0.47
        [Tooltip("36 px hero: show the v14 trait overlays for owned skills (needs hero36V47)")] public bool traits36V47 = true; // v0.0.47
        [Tooltip("36 px hero: use the step32 chill edge overlays, tint, pips and frozen block (needs hero36V47). Off = the Pixel FX Pack chill visuals")] public bool chill36V47 = true; // v0.0.47
        [Tooltip("36 px hero: hero hit radius multiplier (applied to the hero-side padding of every enemy / bullet / hazard hit check). 1 = old 32 px hitbox")] [Range(0.5f, 2f)] public float heroHitScale47 = 1.2f; // v0.0.47
        [Tooltip("36 px hero: frame size in px (hero_base_36 is 576x36 = 16 frames of 36x36)")] public int hero36CellPx = 36; // v0.0.47
        [Tooltip("36 px hero: frame count (0-7 idle, 8-15 walk)")] public int hero36Frames = 16; // v0.0.47
        [Tooltip("36 px hero: pivot in px from the TOP-LEFT of a frame (bottom-centre = 18,35)")] public Vector2 hero36PivotPx = new Vector2(18, 35); // v0.0.47
        [Tooltip("Trait / frozen overlay frame size in px (56x50)")] public Vector2Int trait36CellPx = new Vector2Int(56, 50); // v0.0.47
        [Tooltip("Trait / frozen overlay pivot in px from the TOP-LEFT (28,47 = hero pivot; hero sits at 10,12 in the cell)")] public Vector2 trait36PivotPx = new Vector2(28, 47); // v0.0.47
        [Tooltip("Trait overlay frame count (same frame index as the hero)")] public int trait36Frames = 16; // v0.0.47
        [Tooltip("BEHIND strips (under the hero), bottom-most first (anchors_36_v14.json draw_order_behind)")] public string[] trait36BehindOrder = { "GravePool", "HellfirePillar", "Fireball", "StormRing" }; // v0.0.47
        [Tooltip("FRONT strips (over the hero), bottom-most first (anchors_36_v14.json draw_order_front)")] public string[] trait36FrontOrder = { "GravePool", "HellfirePillar", "Fireball", "StormRing", "FrostCleave", "SoulSpears", "BoneOrbit", "CurseRune", "ChainLightning", "BloodWhip" }; // v0.0.47
        [Tooltip("Sorting offset of the bottom-most BEHIND strip vs the hero base (below the hero outline at -2)")] public int trait36BehindSort = -20; // v0.0.47
        [Tooltip("Sorting offset of the bottom-most FRONT strip vs the hero base (the chill edge overlay sits at +1)")] public int trait36FrontSort = 2; // v0.0.47
        [Tooltip("Traits that raise the chill pip bar to the 'tall' height")] public string[] chill36TallTraits = { "Fireball", "HellfirePillar", "ChainLightning", "SoulSpears", "StormRing" }; // v0.0.47
        [Tooltip("Chill edge overlay sorting offset vs the hero base (over the hero, under FRONT traits)")] public int chill36EdgeSort = 1; // v0.0.47
        [Tooltip("Frozen block sorting offset vs the hero base (above the hero and all traits)")] public int chill36FrozenSort = 40; // v0.0.47
        [Tooltip("Chill pips + bar sorting offset vs the hero base")] public int chill36PipSort = 50; // v0.0.47
        [Tooltip("Chill tint colour (RGB 120,210,255), applied to the hero and all trait layers")] public Color chill36Tint = new Color(120f / 255f, 210f / 255f, 1f, 1f); // v0.0.47
        [Tooltip("Chill tint amount by stack 0,1,2,3 (3 = also used while frozen)")] public float[] chill36TintByStack = { 0f, 0.22f, 0.4f, 0.55f }; // v0.0.47
        [Tooltip("Chill edge overlay frame size in px (36x36, same frames and pivot as the hero)")] public int chill36EdgeCellPx = 36; // v0.0.47
        [Tooltip("Frozen freeze-in frame times (ms), 5 frames, not held")] public int[] chill36FreezeInMs = { 50, 50, 50, 60, 90 }; // v0.0.47
        [Tooltip("Frozen shimmer frame times (ms), 4 frames, looped for the rest of the freeze")] public int[] chill36ShimmerMs = { 250, 90, 90, 90 }; // v0.0.47
        [Tooltip("Break-out frame times (ms), 7 frames, played when the freeze ends")] public int[] chill36BreakMs = { 60, 60, 70, 50, 60, 70, 90 }; // v0.0.47
        [Tooltip("Break-out frames before this index still hold the hero on frame 0 (3 = cracks on block for 0-2, shards only from 3)")] public int chill36BreakFreeFrame = 3; // v0.0.47
        [Tooltip("Chill pip cell size in px (9x9, frames: empty, full, pop, max)")] public int chill36PipCellPx = 9; // v0.0.47
        [Tooltip("Chill pip frame count")] public int chill36PipFrames = 4; // v0.0.47
        [Tooltip("Chill pip centre-to-centre spacing in px")] public int chill36PipSpacingPx = 7; // v0.0.47
        [Tooltip("Chill pip bar size in px (dark rounded bar drawn in code behind the pips)")] public Vector2Int chill36BarPx = new Vector2Int(27, 11); // v0.0.47
        [Tooltip("Chill pip bar colour")] public Color chill36BarColor = new Color(0x18 / 255f, 0x14 / 255f, 0x25 / 255f, 0.85f); // v0.0.47
        [Tooltip("Pip bar BOTTOM, px above the hero pivot, with no tall trait")] public int chill36PipBottomBasePx = 26; // v0.0.47
        [Tooltip("Pip bar BOTTOM, px above the hero pivot, with any tall trait")] public int chill36PipBottomTallPx = 38; // v0.0.47
        [Tooltip("Pip bar BOTTOM, px above the hero pivot, with the Fireball + Hellfire Pillar combo crown")] public int chill36PipBottomComboPx = 45; // v0.0.47
        [Tooltip("Pip 'pop' frame time (ms) after a stack is gained")] public int chill36PipPopMs = 150; // v0.0.47
        [Tooltip("36 px hero: draw the approved 36 px glass horns (step33, Hero36/hero_36_glass_horns) when crit >= baseCrit + 0.1. Off = the old 32 px hero_up_glass (shown on the 32 px hero only)")] public bool glassHorns36V47 = true; // v0.0.47
        [Tooltip("Glass horns: the FRONT trait they draw directly UNDER (json combo rule: lightning horns + arc stay on top). Unknown name = over all front traits")] public string glassHorns36Below = "ChainLightning"; // v0.0.47
        [Tooltip("Glass horns: hide while the frozen ice block (freeze-in / shimmer / break-out) is drawn")] public bool glassHorns36HideFrozen = true; // v0.0.47
        [Tooltip("Front trait sort step (front i sorts at trait36FrontSort + i * step; 2 leaves a slot for the glass horns)")] public int trait36FrontSortStep = 2; // v0.0.47
        [Tooltip("Keep the Pixel FX Pack freeze-immunity ring at the feet while the 36 px chill art is on (it has no immunity art)")] public bool chill36KeepPackImmune = true; // v0.0.47
            // ================= v0.0.49 combat redesign v0.2 (first pass) =================
        [Header("v0.0.49 Combat v0.2")]
        [Tooltip("Master switch for the v0.0.49 combat pass (camera, rooms, density, dash, Intensity, mercy, enemy patterns, range kits). Off = v0.0.48 behaviour")] public bool combatV49 = true; // v0.0.49
        [Tooltip("t636u TEST: mouse wheel steps the camera view through camZoomSteps636 during Play (wheel up = zoom in / smaller view, wheel down = zoom out / bigger view). Off = normal auto integer zoom")] public bool camScrollZoom636 = true; // v0.0.63 t636u
        [Tooltip("t636u TEST: show the 'CAM WxH xScale SAFE/UNSAFE' readout at the bottom-centre during Play")] public bool camResHud636 = true; // v0.0.63 t636u
        [Tooltip("t636u TEST: wheel steps, small to big (art px view). The world is stretched to the window, so SAFE only when the window is an exact integer multiple with the same aspect; the readout computes that live")] public Vector2[] camZoomSteps636 = { new Vector2(320, 180), new Vector2(400, 225), new Vector2(480, 270), new Vector2(500, 280), new Vector2(600, 340), new Vector2(640, 360), new Vector2(720, 400), new Vector2(800, 450), new Vector2(850, 480), new Vector2(960, 540), new Vector2(1000, 560), new Vector2(1280, 720) }; // v0.0.63 t636u: safe 16:9 + unsafe mixed by size
        [Tooltip("t637u TEST: SAFE (true) / UNSAFE (false) label for each camZoomSteps636 entry, same order. The CAM readout shows this flag as-is (not recomputed from the window)")] public bool[] camZoomSafe636 = { true, true, true, false, false, true, false, true, false, true, false, true }; // v0.0.63 t637u
        [Tooltip("Camera: the integer zoom is the smallest that keeps the view within this many art px (512x288 at 1920x1080 = zoom 4 = 480x270 view). Old value 768x432")] public Vector2 camTargetPx49 = new Vector2(640f, 360f); // v0.0.49 512x288 -> t642u 640x360 (1080p zoom 3, 720p zoom 2)
        [Tooltip("Combat room walk area limits, world units^2: room art outside is re-rolled (old 500..3400)")] public float roomWalkMinU2_49 = 300f, roomWalkMaxU2_49 = 1100f; // v0.0.49
        [Tooltip("Survive rooms may be larger, up to this walk area (world units^2)")] public float roomSurviveMaxU2_49 = 1500f; // t571u 1600 -> 1500 (biggest survive art = 896x512, 1484 u2) // v0.0.49
        [Tooltip("Demons alive per 100 u^2 of walk area at room start (old densityPer100Units 1.7)")] public float density49 = 2.6f; // v0.0.49
        [Header("v0.0.49 Dash (Space / pad East or RB)")]
        [Tooltip("Dash cooldown, seconds (one dash, no charges)")] public float dashCooldownS49 = 1.2f; // v0.0.49
        [Tooltip("Dash distance, world units")] public float dashDistU49 = 3.2f; // v0.0.49
        [Tooltip("Dash travel time, seconds")] public float dashTimeS49 = 0.16f; // v0.0.49
        [Tooltip("Invulnerability from dash start, seconds")] public float dashIFrameS49 = 0.25f; // v0.0.49
        [Tooltip("A dash snaps an active Hook chain (sets the hook to break)")] public bool dashBreaksHook49 = true; // v0.0.49
        [Header("v0.0.49 Intensity (mutual scaling)")]
        [Tooltip("combatIntensity at floor 0")] public float intensityBase49 = 1.5f; // v0.0.49
        [Tooltip("Intensity added per FloorIdx")] public float intensityPerFloor49 = 0.15f; // v0.0.49
        [Tooltip("Enemy pattern cooldown is divided by I^this")] public float enemyCdExp49 = 1f; // v0.0.49
        [Tooltip("Enemy bullets per burst are multiplied by I^this")] public float enemyBulletsExp49 = 1f; // v0.0.49
        [Tooltip("Enemy pattern damage is multiplied by I^this")] public float enemyDmgExp49 = 0.5f; // v0.0.49
        [Tooltip("Boss pattern cooldown is divided by I^this")] public float bossCdExp49 = 0.5f; // v0.0.49
        [Tooltip("Player skill damage is multiplied by I^this")] public float playerDmgExp49 = 0f; // t1012u C: was 1 (no free floor dmg)
        [Tooltip("Player skill area / radius is multiplied by I^this")] public float playerAreaExp49 = 0f; // t1012u C: was 0.5
        [Tooltip("Player skill cooldowns are divided by I^this")] public float playerCdExp49 = 0f; // t1012u C: was 0.5
        [Tooltip("Extra Fireball / Soul Spear projectiles: +1 at I >= first value, +2 at I >= second value")] public Vector2 playerCountAtI49 = new Vector2(1.5f, 2f); // v0.0.49
        [Tooltip("Base player aggression: flat skill damage multiplier (before Intensity)")] public float playerDmgMul49 = 1f; // v0.0.49f was 1.4
        [Tooltip("Base player aggression: flat skill area multiplier (before Intensity)")] public float playerAreaMul49 = 1f; // v0.0.49f was 1.2
        [Header("v0.0.49 Mercy valve")]
        [Tooltip("Window, seconds, for counting damage taken")] public float mercyWindowS49 = 5f; // v0.0.49
        [Tooltip("Mercy starts when damage in the window exceeds this fraction of max HP")] public float mercyHpFrac49 = 0.4f; // v0.0.49
        [Tooltip("Mercy lasts this long, seconds")] public float mercyDurationS49 = 4f; // v0.0.49
        [Tooltip("Enemy fire rate multiplier while mercy is on (0.7 = -30%)")] public float mercyFireMul49 = 0.7f; // v0.0.49
        [Header("v0.0.49 Enemy bullets / tells")]
        [Tooltip("Max enemy bullets alive (readability + perf)")] public int bulletMax49 = 60; // v0.0.49
        [Tooltip("Enemy bullet life, seconds")] public float bulletLifeS49 = 4f; // v0.0.49
        [Tooltip("Hero hit radius vs enemy bullets, world units (times the hero hit scale)")] public float bulletHeroR49 = 0.35f; // v0.0.49
        [Tooltip("Bullet sprite sorting order (over characters)")] public int bulletSort49 = 2003; // v0.0.49
        [Tooltip("Lead aim: 0 = aim at the hero, 1 = full lead on the hero's velocity")] public float leadAim49 = 0.7f; // v0.0.49
        [Tooltip("Storm Ring erases enemy bullets within this band of its radius, world units")] public float stormEraseBandU49 = 0.7f; // v0.0.49
        [Tooltip("Frost Cleave erases enemy bullets in its cone")] public bool frostEraseBullets49 = true; // v0.0.49
        [Header("v0.0.49 Imp lunge (also split children)")]
        [Tooltip("Lunge trigger range, units")] public float impLungeRangeU49 = 3f; // v0.0.49
        [Tooltip("Lane tell, seconds")] public float impLungeTellS49 = 0.4f; // v0.0.49
        [Tooltip("Lunge distance, units")] public float impLungeDistU49 = 2.5f; // v0.0.49
        [Tooltip("Lunge travel time, seconds")] public float impLungeTimeS49 = 0.2f; // v0.0.49
        [Tooltip("Lunge cooldown, seconds")] public float impLungeCdS49 = 2.5f; // v0.0.49
        [Header("v0.0.49 Ghoul reach grab")]
        [Tooltip("Grab reach, units")] public float ghoulGrabReachU49 = 1.5f; // v0.0.49
        [Tooltip("Arms-up tell, seconds")] public float ghoulGrabTellS49 = 0.5f; // v0.0.49
        [Tooltip("Hold time on a hit, seconds")] public float ghoulHoldS49 = 1f; // v0.0.49
        [Tooltip("Hero move multiplier while held")] public float ghoulHoldMoveMul49 = 0.3f; // v0.0.49
        [Tooltip("Grab damage multiplier vs contact damage")] public float ghoulGrabDmgMul49 = 1.2f; // v0.0.49
        [Tooltip("Grab cooldown, seconds")] public float ghoulGrabCdS49 = 3f; // v0.0.49
        [Header("v0.0.49 Brute charge + cleave")]
        [Tooltip("Starts within this range, units")] public float bruteRangeU49 = 4.5f; // v0.0.49
        [Tooltip("Lane tell, seconds")] public float bruteTellS49 = 0.5f; // v0.0.49
        [Tooltip("Charge step distance, units")] public float bruteStepU49 = 3f; // v0.0.49
        [Tooltip("Charge step time, seconds")] public float bruteStepS49 = 0.25f; // v0.0.49
        [Tooltip("Cleave cone tell after the step, seconds")] public float bruteCleaveTellS49 = 0.25f; // v0.0.49
        [Tooltip("Cleave reach, units")] public float bruteCleaveU49 = 3f; // v0.0.49
        [Tooltip("Cleave half angle, degrees (60 = 120 cone)")] public float bruteCleaveHalfDeg49 = 60f; // v0.0.49
        [Tooltip("Cleave damage multiplier vs contact damage")] public float bruteCleaveDmgMul49 = 2f; // v0.0.49
        [Tooltip("Stuck after the cleave, seconds")] public float bruteStuckS49 = 1f; // v0.0.49
        [Tooltip("Cooldown, seconds")] public float bruteCdS49 = 3.5f; // v0.0.49
        [Header("v0.0.49 Elite wraith blink + X-slash + rings")]
        [Tooltip("Cooldown between blink cycles, seconds")] public float eliteCdS49 = 5f; // v0.0.49
        [Tooltip("Blink destination tell, seconds")] public float eliteBlinkTellS49 = 0.6f; // v0.0.49
        [Tooltip("Blink lands this far behind the hero, units")] public float eliteBehindU49 = 2.5f; // v0.0.49
        [Tooltip("X-slash lane tell, seconds")] public float eliteSlashTellS49 = 0.45f; // v0.0.49
        [Tooltip("X-slash lane length and half width, units")] public float eliteSlashLenU49 = 5f, eliteSlashHalfWU49 = 0.45f; // v0.0.49
        [Tooltip("X-slash damage multiplier vs contact damage")] public float eliteSlashDmgMul49 = 1.3f; // v0.0.49
        [Tooltip("Ring bullets (before Intensity), speed u/s, 2nd ring delay s, 2nd ring rotation deg")] public int eliteRingN49 = 8; // v0.0.49
        public float eliteRingSpeed49 = 8f, eliteRingDelayS49 = 0.3f, eliteRingRotDeg49 = 22f; // v0.0.49
        [Header("v0.0.49 Upgrades to existing demons")]
        [Tooltip("Frost Caster bolt fan count (before Intensity), spread deg, bolt speed u/s")] public int fcFanN49 = 7; // v0.0.49
        public float fcFanDeg49 = 50f, fcBoltSpeed49 = 9f; // v0.0.49
        [Tooltip("Fire Wheel roll speed multiplier; roll is lead-aimed")] public float fwSpeedMul49 = 1.2f; // v0.0.49
        [Tooltip("Hook Thrower: hook speed (art px/s, 384 = 24 u/s), lane tell seconds")] public float hookSpeedPx49 = 384f, hookLaneTellS49 = 0.6f; // v0.0.49
        [Tooltip("Gossip Imp: shout tell seconds, radius units, scramble (silence) seconds")] public float gossipTellS49 = 1f, gossipRadiusU49 = 5f, gossipScrambleS49 = 1.5f; // v0.0.49
        [Header("v0.0.49 Range kits (recolours)")]
        [Tooltip("Chance per floor spawn to be a range-kit demon")] public float rangeKitChance49 = 0.2f; // v0.0.49
        [Tooltip("Max range-kit demons alive: base + per FloorIdx")] public int rangeKitMax49 = 4; // v0.0.49
        public float rangeKitMaxPerFloor49 = 0.5f; // v0.0.49
        [Tooltip("Range demons keep this distance band to the hero, units")] public float rangeMinU49 = 4.5f, rangeMaxU49 = 7.5f; // v0.0.49
        [Tooltip("Crossfire: a new range demon spawns on the far side from the last one (offset jitter deg), at this distance from the hero")] public float crossJitterDeg49 = 40f, crossDistU49 = 6.5f; // v0.0.49
        [Tooltip("Chance a Brute on a Frozen floor becomes a Frost Brute")] public float frostBruteChance49 = 0.35f; // v0.0.49
        [Tooltip("Sack Lobber also rolls on Ox floors (Mortar floor not wired yet)")] public bool sackOnOx49 = false; // v0.0.49
        [Tooltip("Imp Gunner: burst count (before Intensity), gap s, speed u/s, stop tell s, cooldown s, damage")] public int gunBurst49 = 3; // v0.0.49
        public float gunGapS49 = 0.12f, gunSpeed49 = 12f, gunTellS49 = 0.4f, gunCdS49 = 2.5f, gunDmg49 = 5f; // v0.0.49
        [Tooltip("Archer: laser total s, tracking part s, arrow speed u/s, cooldown s, damage")] public float arLaserS49 = 0.8f, arTrackS49 = 0.5f, arSpeed49 = 26f, arCdS49 = 3f, arDmg49 = 8f; // v0.0.49
        [Tooltip("Ice Sniper: laser total s, tracking part s, shard speed u/s, cooldown s, damage, chill stacks")] public float isLaserS49 = 1f, isTrackS49 = 0.6f, isSpeed49 = 28f, isCdS49 = 3.5f, isDmg49 = 10f; // v0.0.49
        public int isChill49 = 2; // v0.0.49
        [Tooltip("Grenades (Snowball / Ember / Sack): flight s, landing radius u, cooldown s, damage, ground patch life s")] public float gnFlightS49 = 0.8f, gnRadiusU49 = 2f, gnCdS49 = 3f, gnDmg49 = 7f, gnPatchS49 = 3f; // v0.0.49
        [Tooltip("Ember Spitter landing radius, units (2 embers per volley)")] public float emberRadiusU49 = 1.8f; // v0.0.49
        [Tooltip("Lava patch tick damage")] public float lavaDmg49 = 4f; // v0.0.49
        [Tooltip("Flame Wraith ring: bullets (before Intensity), gaps, speed u/s, 2nd ring delay s, rotation deg, cooldown s, damage")] public int fwrRingN49 = 16, fwrGaps49 = 4; // v0.0.49
        public float fwrSpeed49 = 8f, fwrDelayS49 = 0.4f, fwrRotDeg49 = 11f, fwrCdS49 = 3.5f, fwrDmg49 = 6f, fwrTellS49 = 0.5f; // v0.0.49
        [Tooltip("Frost Brute: dash lane tell s, dash length u, dash time s, slick life s, slam ring r u, cooldown s")] public float fbTellS49 = 0.7f, fbDashU49 = 6f, fbDashS49 = 0.35f, fbSlickS49 = 3f, fbSlamR49 = 2f, fbCdS49 = 4f; // v0.0.49
        [Tooltip("Hero hit damage multiplier of the Frost Brute slam vs contact")] public float fbSlamDmgMul49 = 1.5f; // v0.0.49
    
    [Header("v0.0.49b: enemy nerf / player buff (on top of Intensity)")]
    [Tooltip("Enemy attack cooldowns are multiplied by this (>1 = slower fire / pattern rate).")] public float enemyCdMul49b = 1.15f; // v0.0.49d softened
    [Tooltip("Enemy damage multiplier for all v0.0.49 attacks, Frost Caster bolts and enemy contact hits.")] public float enemyDmgMul49b = 0.85f; // v0.0.49d softened
    [Tooltip("Enemy AoE radius multiplier: grenades, patches, grab reach, cleave, slash, slam.")] public float enemyAreaMul49b = 0.9f; // v0.0.49d softened
    [Tooltip("Enemy projectile count multiplier (fans, rings, bursts), applied after Intensity.")] public float enemyCountMul49b = 0.9f; // v0.0.49d softened
    [Tooltip("Scales only the enemy share of Intensity: I_enemy = 1 + (I - 1) x this. Player side keeps full Intensity.")] public float enemyIntensityMul49b = 0.85f; // v0.0.49d softened
    [Tooltip("Player skill damage multiplier.")] public float playerDmgMul49b = 1f; // v0.0.49d softened
    [Tooltip("Player skill AoE multiplier (all AreaMul users: Frost Cleave reach, Storm Ring, chain range, explosions).")] public float playerAreaMul49b = 1f; // v0.0.49d softened
    [Tooltip("Player projectile speed multiplier.")] public float playerProjSpeedMul49b = 1f; // v0.0.49d softened
    [Tooltip("Player projectile lifetime (range) multiplier.")] public float playerRangeMul49b = 1f; // v0.0.49d softened

    [Header("t679u: floor 2+ enemy softening (VH: too hard from floor 2)")]
    [Tooltip("t679u master switch. Off = old 49b/49d ladder.")] public bool soften679 = true;
    [Tooltip("t679u: enemy Intensity on floor N (FloorIdx>=1) = 1 + this x FloorIdx (old: 1 + (1.5 + 0.15 x idx - 1) x 0.85 = 1.55 on floor 2).")] public float enemyIPerFloor679 = 0.12f;
    [Tooltip("t679u: enemy bullet count multiplier is capped at this (fans, rings, bursts).")] public float enemyBulletMaxMul679 = 1.25f;
    [Tooltip("t679u: enemy fire-rate speed-up (cooldown divider) is capped at this.")] public float enemyCdDivMax679 = 1.3f;
    [Tooltip("t679u: enemy damage growth per floor (replaces floorEnemyDmgGrowth 0.15 for demon contact + attacks).")] public float enemyDmgGrowth679 = 0.08f;
    [Tooltip("t679u: density added per floor after floor 2 (replaces densityPerFloor49h 0.3).")] public float densityPerFloor679 = 0.2f;
    [Tooltip("t679u: Frost Caster (ice mage) volley = exactly this many bolts: leftmost, middle, rightmost of the fan. Not scaled by Intensity. 0 = old scaled fan.")] public int fcVolley679 = 3;

    [Header("v0.0.49d: floor ladder (floor 1 = FloorIdx 0)")]
    [Tooltip("Bone Orbit radius multiplier (the only early player buff).")] public float orbitAreaMul49d = 1.2f; // v0.0.49d
    [Tooltip("Floor 1 enemy-side Intensity (replaces base on floor 1 only). Floor 2+ uses intensityBase49 + perFloor x FloorIdx.")] public float floor0EnemyIntensity49d = 1.0f; // v0.0.49d
    [Tooltip("Floor 1: melee-only fight rooms before ranged kits / Hook / Gossip may appear (cleared fight rooms).")] public int floor0MeleeOnlyRooms49d = 3; // v0.0.49f was 2
    [Tooltip("Floor 1: cleared fight rooms before Splitters and other non-basic types appear.")] public int floor0OtherTypesRooms49d = 5; // v0.0.49f was 4
    [Tooltip("Floor 1: ranged-kit chance once unlocked; it ramps by floor0RangeRamp49d per further cleared room.")] public float floor0RangeChance49d = 0.08f; // v0.0.49d
    [Tooltip("Floor 1: extra ranged-kit chance per cleared fight room after unlock.")] public float floor0RangeRamp49d = 0.03f; // v0.0.49d
    [Tooltip("Floor 1: max ranged kits alive at once.")] public int floor0RangeMax49d = 2; // v0.0.49d
    [Tooltip("Floor 1: Imp Gunner burst size (simple pattern).")] public int floor0GunBurst49d = 1; // v0.0.49d
    [Tooltip("Floor 1 walk area min (u2).")] public float floor0RoomMinU2_49d = 250f; // v0.0.49d
    [Tooltip("Floor 1 walk area max (u2); Survive uses this x1.3.")] public float floor0RoomMaxU2_49d = 650f; // v0.0.49d
    [Tooltip("Floor 1 density per 100 u2.")] public float floor0Density49d = 2.0f; // v0.0.49d
    [Tooltip("Floor 2+ walk area min (u2).")] public float floorNRoomMinU2_49d = 400f; // v0.0.49d
    [Tooltip("Floor 2+ walk area max grows by this per floor after floor 2 (u2).")] public float roomMaxPerFloor49d = 60f; // t571u 150 -> 60 (deep floors no longer grow into 1024x576/1152x656 rooms) // v0.0.49d
    [Tooltip("Floor 2+ density per 100 u2 added per floor after floor 1.")] public float densityPerFloor49d = 0.3f; // v0.0.49d
    [Tooltip("Floor 2+ ranged-kit chance added per floor after floor 1 (more types in one room).")] public float rangeKitPerFloor49d = 0.05f; // v0.0.49d
    [Tooltip("First FloorIdx where complex patterns run (Imp lunge, Ghoul grab, Brute charge, Elite blink/rings, multi-kits). 1 = floor 2.")] public int complexFloor49d = 1; // v0.0.49d

    [Header("v0.0.49e: Mortar enemy art (PixelLab v14)")]
    [Tooltip("Bloater / Rice Ghost / Grinder Monk use the PixelLab sheets in Resources/HellordArt. Off or sheet missing = null (caller falls back).")] public bool mortarSheets49e = true; // v0.0.49e

    [Header("v0.0.49f: early-game fix (floor 1 melee start, no early player buff)")]
    [Tooltip("First FloorIdx where player-side Intensity scaling turns on (1 = floor 2). Before it all player multipliers are 1.")] public int playerScaleFloor49f = 1; // v0.0.49f
    [Tooltip("Player Intensity is relative to intensityBase49: Ip = Intensity / base, so floor 2 is about 1.1, not 1.65.")] public bool playerRelIntensity49f = true; // v0.0.49f
    [Tooltip("Floor 1: no theme variants (frost / bomber / Frost Caster / Fire Wheel) before this many cleared fight rooms.")] public int floor0VariantRooms49f = 5; // v0.0.49f
    [Tooltip("v0.0.49g: first FloorIdx where Intensity may add extra projectiles (Fireball / Soul Spears). 1 = floor 2. Before it, projectiles come only from skill level and supports.")] public int playerCountFloor49g = 1; // v0.0.49g
    [Tooltip("v0.0.49h: use the 49h density ladder below (fewer demons per room, growing per floor).")] public bool density49hOn = true; // v0.0.49h
    [Tooltip("Floor 1 demons per 100 u2 of walk area at room start (49d was 2.0).")] public float floor0Density49h = 1.4f; // v0.0.49h
    [Tooltip("Floor 2 demons per 100 u2 (49d used density49 2.6).")] public float floor2Density49h = 1.8f; // v0.0.49h
    [Tooltip("Density added per floor after floor 2 (49d was 0.3).")] public float densityPerFloor49h = 0.3f; // v0.0.49h
    [Header("v0.0.49i: smaller floor-1 rooms, demon caps, hive nerf, gentler HP")]
    [Tooltip("Floor 1 walk-area max (u2). Smallest fight art is 703 u2 (640x368), so 720 = smallest rooms only. 49d was 650, which no art fit, so the reroll kept random huge rooms.")] public float floor0RoomMaxU2_49i = 720f; // v0.0.49i
    [Tooltip("Floor 1 Survive / Nests / Elite may use up to this x the max (1.45 -> 1044 u2, allows 768x432).")] public float floor0BigRoomMul49i = 1.45f; // v0.0.49i
    [Tooltip("When 8 rerolls fail, pick a fitting art (or the smallest) instead of keeping an oversized one.")] public bool roomFitFallback49i = true; // v0.0.49i
    [Tooltip("Floor 1: max demons alive at once in the 1st fight room.")] public int aliveCapF0Start49i = 12; // v0.0.49i
    [Tooltip("Floor 1: alive cap added per cleared fight room.")] public int aliveCapF0PerRoom49i = 2; // v0.0.49i
    [Tooltip("Floor 1: alive cap never above this.")] public int aliveCapF0Max49i = 20; // v0.0.49i
    [Tooltip("Floor 2: max demons alive at once.")] public int aliveCapF2_49i = 26; // v0.0.49i
    [Tooltip("Alive cap added per floor after floor 2.")] public int aliveCapPerFloor49i = 6; // v0.0.49i
    [Tooltip("Floor 1: Hunt / Treasure kill goal in the 1st fight room (was firstHuntGoal 60).")] public int killGoalF0Start49i = 15; // v0.0.49i
    [Tooltip("Floor 1: kill goal added per cleared fight room.")] public int killGoalF0PerRoom49i = 2; // v0.0.49i
    [Tooltip("Floor 1: kill goal never above this.")] public int killGoalF0Max49i = 25; // v0.0.49i
    [Tooltip("Floor 2: kill goal cap (Hunt was 100 x size x floor growth).")] public int killGoalF2_49i = 40; // v0.0.49i
    [Tooltip("Kill goal cap added per floor after floor 2.")] public int killGoalPerFloor49i = 10; // v0.0.49i
    [Tooltip("Elite room guards cap on floor 1 (+2 per floor). Old eliteGuards 8.")] public int eliteGuardsMax49i = 4; // v0.0.49i
    [Tooltip("Hive (nest) base HP (was 260).")] public float nestHp49i = 150f; // v0.0.49i
    [Tooltip("Hive HP added per cleared fight room (was 0.35 = +35% each room).")] public float nestHpPerFight49i = 0.08f; // v0.0.49i
    [Tooltip("Hive HP added per floor.")] public float nestHpPerFloor49i = 0.15f; // v0.0.49i
    [Tooltip("Seconds between hive spawns (was 2.5).")] public float nestSpawnEvery49i = 3.5f; // v0.0.49i
    [Tooltip("Demons per hive spawn (was 2).")] public int nestSpawnN49i = 1; // v0.0.49i
    [Tooltip("Hives stop spawning when alive demons reach this x the room alive cap (was 200 flat).")] public float nestAliveCapMul49i = 0.8f; // v0.0.49i
    [Tooltip("Demon HP added per floor (old floorEnemyHpGrowth 0.25).")] public float floorEnemyHpGrowth49i = 0.15f; // v0.0.49i
    [Header("v0.0.50: ambient hell screams (distant damned)")]
    [Tooltip("v0.0.50: master switch for random distant screams on room enter.")] public bool ambientScreamsOn50 = true; // v0.0.50 ambient screams
    [Tooltip("Chance (0-1) to play a scream when entering a room, if the cooldown is over.")] [Range(0f, 1f)] public float screamChance50 = 0.2f; // v0.0.50 ambient screams
    [Tooltip("Global cooldown after a scream: minimum seconds (random between min and max).")] public float screamCooldownMin50 = 20f; // v0.0.50 ambient screams
    [Tooltip("Global cooldown after a scream: maximum seconds.")] public float screamCooldownMax50 = 40f; // v0.0.50 ambient screams
    [Tooltip("Volume min as a fraction of the SFX bus (sfxVolume).")] [Range(0f, 1f)] public float screamVolMin50 = 0.3f; // v0.0.50 ambient screams
    [Tooltip("Volume max as a fraction of the SFX bus.")] [Range(0f, 1f)] public float screamVolMax50 = 0.5f; // v0.0.50 ambient screams
    [Tooltip("Pitch min (random per play).")] public float screamPitchMin50 = 0.92f; // v0.0.50 ambient screams
    [Tooltip("Pitch max.")] public float screamPitchMax50 = 1.08f; // v0.0.50 ambient screams
    [Tooltip("Random stereo pan up to +-this, so the scream sounds off-screen / to the side (0 = centre).")] [Range(0f, 1f)] public float screamPanMax50 = 0.6f; // v0.0.50 ambient screams
    [Tooltip("Delay after room enter before the scream starts, seconds (min..max random). Keeps it off the door/banner sounds.")] public float screamDelayMin50 = 1.5f; // v0.0.50 ambient screams
    [Tooltip("Max delay after room enter, seconds.")] public float screamDelayMax50 = 5f; // v0.0.50 ambient screams
    [Tooltip("Boss fights: no new screams, and a playing scream fades out over screamBossFadeS50.")] public bool screamMuteInBoss50 = true; // v0.0.50 ambient screams
    [Tooltip("Fade-out seconds for a scream cut by a boss fight.")] public float screamBossFadeS50 = 0.5f; // v0.0.50 ambient screams
    [Header("v0.0.51: Damnation Pact (Sacrifice Altar meta)")]
    [Tooltip("Boot into the Main Menu (Descend / Sacrifice Altar / Quit). Off = old behaviour (straight to the starting-skill pick). Autotest/botstate runs always skip the menu.")] public bool pactMainMenu51 = true; // v0.0.51 damnation pact
    [Tooltip("t511: Sacrifice Altar uses the VH-approved v97 art (Path carousel + PixelLab node tree). Off = v0.0.53 placeholder altar.")] public bool altar97 = true;
    [Tooltip("t526/t527: Altar Path portrait idle loop (all 4 Paths), ms per frame. Plays only on the focused, unlocked Path card. 0 = static base bust.")] public float boneIdleFrameMs97 = 180f;
    [Tooltip("t511: clicking the main-menu Pact seal opens the Sacrifice Altar (same as the SACRIFICE ALTAR row). Off = old placeholder Pact panel.")] public bool mm69SealOpensAltar = true;
    [Header("t503u Main Menu v69 (VH-locked art)")]
    [Tooltip("Use the VH-locked v69 main menu art (v49 title, skull-stair bg, text rows + skull/blood select, Damnation Pact seal). Off = old v0.0.51 hex-button menu.")] public bool mainMenu69 = true;
    [Tooltip("Menu animation frame time in ms (art baked at 90 ms, 48 frames).")] public float mm69FrameMs = 90f;
    [Tooltip("Seconds for the seal hover/focus highlight to fade in or out.")] public float mm69HoverFadeS = 0.18f;
    [Tooltip("Seal hover brighten amount (0.18 = ~18% brighter, masked to the seal silhouette).")] public float mm69HoverBright = 0.18f;
    [Tooltip("Seconds of the soft press brighten on the seal before the Pact panel opens (~5 frames at 90 ms).")] public float mm69PressS = 0.45f;
    [Tooltip("Seconds for the Pact panel to grow out of the seal.")] public float mm69PanelOpenS = 0.63f;
    [Tooltip("Seconds for the Pact panel to shrink back into the seal on [B]/Esc.")] public float mm69PanelCloseS = 0.45f;
    [Tooltip("Max nodes selected in one Pact (Keystone uses one of these slots). VH locked: 6.")] public int pactMaxSelect51 = 6; // v0.0.51 damnation pact
    [Tooltip("Max Keystones in one Pact.")] public int pactMaxKeystones51 = 1; // v0.0.51 damnation pact
    [Tooltip("Selected nodes from the Pact Path needed for its Crest.")] public int pactCrestAt51 = 4; // v0.0.51 damnation pact
    [Tooltip("Global multiplier on all node Soul costs (catalog v0.6 costs x this).")] public float pactCostMul51 = 1f; // v0.0.51 damnation pact
    [Tooltip("Sigils needed to unlock a Keystone (on top of its Soul cost).")] public int pactKeystoneSigils51 = 1; // v0.0.51 damnation pact
    [Tooltip("Souls earned per bone picked up in a run (paid at death / run complete).")] public float soulsPerBone51 = 1f; // v0.0.51 damnation pact
    [Tooltip("Souls earned per kill.")] public float soulsPerKill51 = 0.1f; // v0.0.51 damnation pact
    [Tooltip("Souls earned per floor cleared.")] public int soulsPerFloor51 = 25; // v0.0.51 damnation pact
    [Tooltip("Bonus Souls for a completed run.")] public int soulsWinBonus51 = 100; // v0.0.51 damnation pact
    [Tooltip("Sigils earned for a completed run.")] public int sigilsPerWin51 = 1; // v0.0.51 damnation pact
    [Tooltip("Sigils earned per boss killed (0 = only on win).")] public int sigilsPerBoss51 = 1; // v0.0.51 damnation pact; t737/t770u Infernal Sins: 0 -> 1 (bosses give 1 Sigil)
    [Tooltip("Start-of-save Souls (new save only).")] public int startSouls51 = 0; // v0.0.51 damnation pact
    [Tooltip("Path passive per selected node: Pyromancer +inc damage.")] public float pyroIncDmgPerNode51 = 0.04f; // v0.0.51 damnation pact
    [Tooltip("Path passive per selected node: Bone Warden +armor.")] public float boneArmorPerNode51 = 1.5f; // v0.0.51 damnation pact
    [Tooltip("Path passive per selected node: Stormcaller +move speed (fraction).")] public float stormSpeedPerNode51 = 0.02f; // v0.0.51 damnation pact
    [Tooltip("Path passive per selected node: Blood Reaver +max HP.")] public float bloodHpPerNode51 = 6f; // v0.0.51 damnation pact
    [Tooltip("Crest (4+ selected) Pyromancer: +inc damage.")] public float crestPyroIncDmg51 = 0.15f; // v0.0.51 damnation pact
    [Tooltip("Crest Bone Warden: +armor.")] public float crestBoneArmor51 = 6f; // v0.0.51 damnation pact
    [Tooltip("Crest Stormcaller: +crit chance.")] public float crestStormCrit51 = 0.08f; // v0.0.51 damnation pact
    [Tooltip("Crest Blood Reaver: +max HP.")] public float crestBloodHp51 = 30f; // v0.0.51 damnation pact
    [Tooltip("Signature notables (live proxy): damage multiplier on the node's linked skill (e.g. Kindling -> Fireball).")] public float pactSkillMul51 = 1.25f; // v0.0.51 damnation pact
    [Tooltip("Blood Reaver N5 Red Feast: HP healed per kill.")] public float redFeastHeal51 = 1f; // v0.0.51 damnation pact
    [Tooltip("Blood Reaver N8 Last Rites: cheat death once per run, back to this HP fraction.")] public float lastRitesHeal51 = 0.35f; // v0.0.51 damnation pact
    [Tooltip("Pyromancer K2 Inferno Heart (live proxy): +max HP / +armor.")] public float infernoHeartHp51 = 40f; // v0.0.51 damnation pact
    [Tooltip("t495u: Mortar Hell is a 4th floor theme in the run shuffle (Mortar tiles, low walls / doors, music, Horse-Face boss, Bloater / Rice Ghost / Grinder Monk). OFF = 3 themes. Ox Pit always keeps its own look")] public bool mortarFloor52 = true;
    [Tooltip("t494u: Ox Pit rooms get the torture decor (set pieces, mortar pit, sacks, rice, damned souls, watchers, breakables + loot) on the old Ox Pit look")] public bool oxTortureDecor53 = true; // v0.0.52 torture floors
    [Tooltip("Chance an eligible Ox depth uses the Mortar B skin.")] public float mortarChance52 = 0.5f; // v0.0.52 torture floors
    [Tooltip("Tint on the composed Mortar B base.")] public Color mortarTint52 = Color.white; // v0.0.52 torture floors
    [Tooltip("Mortar rooms: scatter rice-mill props (pit+pestle, sacks, husks, bowls, chopsticks, rice patches).")] public bool mortarProps52 = true; // v0.0.52 torture floors
    [Tooltip("Mortar fight rooms: chance of a pit + animated pestle.")] public float mortarPitChance52 = 0.6f; // v0.0.52 torture floors
    [Tooltip("Mortar rooms: base deco prop count (+0..2 random).")] public int mortarDeco52 = 4; // v0.0.52 torture floors
    [Tooltip("Mortar rooms: rice patch decals per room.")] public int mortarRice52 = 5; // v0.0.52 torture floors
    [Tooltip("Spawn torture set pieces (Ox grind-wheel, Burning G cauldrons, Mortar rice-mill).")] public bool torture52 = true; // v0.0.52 torture floors
    [Tooltip("Chance a non-boss room gets a torture set piece (if it fits).")] public float tortureRoomChance52 = 0.55f; // v0.0.52 torture floors
    [Tooltip("Extra clearance (units) around set pieces from doors/spawn/hero.")] public float tortureSetPad52 = 1.5f; // v0.0.52 torture floors
    [Tooltip("Torture/prop animation fps (art: 90 ms = 11).")] public float tortureFps52 = 11f; // v0.0.52 torture floors
    [Tooltip("Sorting offset over HellordRoom.PropOrder for torture props.")] public int tortureSort52 = 500; // v0.0.52 torture floors
    [Tooltip("Contact shadow alpha under torture props.")] public float tortureShadowA52 = 0.55f; // v0.0.52 torture floors
    [Tooltip("Frozen rooms: ice blocks with humans per room (0 = off).")] public int iceBlocks52 = 3; // v0.0.52 torture floors
    [Tooltip("Ice blocks also in non-fight rooms.")] public bool iceBlocksAnyRoom52 = false; // v0.0.52 torture floors
    [Tooltip("Ice block hit radius (units) for fireballs.")] public float iceHitR52 = 1.6f; // v0.0.52 torture floors
    [Tooltip("Ice block hit centre height above pivot (units).")] public float iceHitY52 = 1.4f; // v0.0.52 torture floors
    [Tooltip("Hits to shatter (1st cracks).")] public int iceHitsToBreak52 = 2; // v0.0.52 torture floors
    [Tooltip("One block per hit (else every block in radius).")] public bool iceOneBlockPerHit52 = true; // v0.0.52 torture floors
    [Tooltip("Any projectile/blast cracks ice (false = fireball only).")] public bool iceAnyProj52 = false; // v0.0.52 torture floors
    [Tooltip("Shatter animation fps (5 frames).")] public float iceShatterFps52 = 12f; // v0.0.52 torture floors
    [Tooltip("Seconds until a shattered block regrows (0 = never).")] public float iceRegrow52 = 0f; // v0.0.52 torture floors
    [Tooltip("Cyan glow alpha behind ice blocks.")] public float iceGlowA52 = 0.55f; // v0.0.52 torture floors
    [Tooltip("Cyan glow scale.")] public float iceGlowScale52 = 1.6f; // v0.0.52 torture floors
    [Tooltip("Torture set pieces, mortar pits, ice blocks and watchers are solid (hero + enemies are pushed out).")] public bool tortureCollide53 = true; // v0.0.53 VH: props stay solid; every skill can still hit/break them
        [Tooltip("v0.0.53: tint on decor watchers (they share the live Mortar demon / frost caster sheets, so untinted they read as enemies).")] public Color watchTint53 = new Color(0.55f, 0.5f, 0.6f, 1f);
        [Tooltip("v0.0.53: per-floor music playlists (Ox Pit / Frozen / Burning / Mortar A+B tracks); false = old single loop.")] public bool floorMusic53 = true;
        [Tooltip("v0.0.53: rotate to the theme's next track after musicLoopsPerTrack53 loops.")] public bool musicRotate53 = true;
        [Tooltip("v0.0.53: loops of a floor track before it rotates.")] public int musicLoopsPerTrack53 = 2;
        [Tooltip("v0.0.53: AoE/melee/beam/orbit/pool/chain/explosion skills also damage breakables, souls and ice.")] public bool brkAoe53 = true;
        [Tooltip("v0.0.53: per-breakable cooldown (s) per skill so ticking areas don't shred props in one frame.")] public float brkAoeCd53 = 0.25f;
        [Tooltip("v0.0.53: damage multiplier for AoE hits on breakables (on top of brkDmgMul53).")] public float brkAoeDmgMul53 = 1f;
        [Tooltip("v0.0.53: extra Chain Lightning jumps into breakables after demons.")] public int brkChainJumps53 = 2; // v0.0.53 torture gaps
    [Tooltip("Set-piece collision width as a fraction of the sprite width (bottom band).")] public float tortureColW53 = 0.62f; // v0.0.53 torture gaps
    [Tooltip("Set-piece collision depth as a fraction of the sprite height (bottom band).")] public float tortureColH53 = 0.22f; // v0.0.53 torture gaps
    [Tooltip("Watching demons beside each set piece (idle, facing the device, never operating it). 3 = one also stands in front.")] public int watchers53 = 2; // v0.0.53 torture gaps
    [Tooltip("Gap in px between the device footprint and its watchers.")] public float watchGapPx53 = 10f; // v0.0.53 torture gaps
    [Tooltip("Watcher idle animation fps.")] public float watchFps53 = 7f; // v0.0.53 torture gaps
    [Tooltip("Seconds (avg) between a watcher's cheer/point bursts (Imp cheer_hop, Warden point, Overseer cheer_fists).")] public float watchActEvery53 = 4f; // v0.0.53 PixelLab watchers
    [Tooltip("How long one cheer/point burst lasts, seconds.")] public float watchActS53 = 0.9f; // v0.0.53 PixelLab watchers
    [Tooltip("Flip the Blindfold Rope Hauler (art faces SE); on = pulls west toward the mortar pulley.")] public bool ropeHaulerFlip53 = true; // v0.0.53 PixelLab watchers
    [Tooltip("Mortar B set piece: the Blindfold Rope Hauler hauls the rope (the only demon on a device).")] public bool ropeMonk53 = true; // v0.0.53 torture gaps
    [Tooltip("Rope Monk X, px from the torture_b sprite's top-left.")] public float ropeMonkX53 = 150f; // v0.0.53 torture gaps
    [Tooltip("Rope Monk feet Y, px from the torture_b sprite's top-left.")] public float ropeMonkY53 = 168f; // v0.0.53 torture gaps
    [Tooltip("Shootable damned souls per Mortar fight room (+0..1 random).")] public int souls53 = 3; // v0.0.53 torture gaps
    [Tooltip("Souls also in non-fight Mortar rooms.")] public bool soulsAnyRoom53 = false; // v0.0.53 torture gaps
    [Tooltip("Damned soul HP.")] public float soulHp53 = 36f; // v0.0.53 torture gaps
    [Tooltip("Damage per hit to a soul (MOCK-UP shows -12 per fireball = 3 hits).")] public float soulDmg53 = 12f; // v0.0.53 torture gaps
    [Tooltip("Soul hit radius (world units).")] public float soulHitR53 = 0.8f; // v0.0.53 torture gaps
    [Tooltip("Soul idle/flail animation fps.")] public float soulFps53 = 6f; // v0.0.53 torture gaps
    [Tooltip("Seconds the squashed soul stays before fading.")] public float soulCorpseS53 = 2.5f; // v0.0.53 torture gaps
    [Tooltip("Torture decor, devices, watchers and the rope Monk are shootable and break with an FX burst.")] public bool breakables53 = true; // v0.0.53 torture breakables
    [Tooltip("HP of a torture set piece (Ox / Mortar B / Burning). Its baked-in humans die with it.")] public float brkDeviceHp53 = 160f; // v0.0.53 torture breakables
    [Tooltip("Set-piece hit box width, fraction of the sprite width.")] public float brkDeviceHitW53 = 0.7f; // v0.0.53 torture breakables
    [Tooltip("Set-piece hit box height, fraction of the sprite height (from the feet).")] public float brkDeviceHitH53 = 0.6f; // v0.0.53 torture breakables
    [Tooltip("HP of the mortar pit + pestle + beam.")] public float brkPitHp53 = 90f; // v0.0.53 torture breakables
    [Tooltip("HP of small Mortar decor (sacks, husk piles, bowls, chopsticks).")] public float brkDecoHp53 = 20f; // v0.0.53 torture breakables
    [Tooltip("HP of rice patches.")] public float brkRiceHp53 = 8f; // v0.0.53 torture breakables
    [Tooltip("HP of watching demons and the rope Monk.")] public float brkDemonHp53 = 45f; // v0.0.53 torture breakables
    [Tooltip("Projectile damage multiplier vs breakables.")] public float brkDmgMul53 = 1f; // v0.0.53 torture breakables
    [Tooltip("Damage an explosion deals to every breakable in its radius.")] public float brkBlastDmg53 = 30f; // v0.0.53 torture breakables
    [Tooltip("Show damage numbers on breakables.")] public bool brkNumbers53 = true; // v0.0.53 torture breakables
    [Tooltip("Seconds of squash-wobble after a hit.")] public float brkWobbleS53 = 0.15f; // v0.0.53 torture breakables
    [Tooltip("Seconds of the squash-pop before a broken object disappears.")] public float brkPopS53 = 0.12f; // v0.0.53 torture breakables
    [Tooltip("Chip particles per hit.")] public int brkChip53 = 5; // v0.0.53 torture breakables
    [Tooltip("Debris particles per break puff.")] public int brkDebrisN53 = 22; // v0.0.53 torture breakables
    [Tooltip("Grain / rice particles per break puff.")] public int brkGrainN53 = 26; // v0.0.53 torture breakables
    [Tooltip("Blood particles per break puff.")] public int brkBloodN53 = 24; // v0.0.53 torture breakables
    [Tooltip("Ember particles per break puff (demons, Burning device).")] public int brkEmberN53 = 18; // v0.0.53 torture breakables
    [Tooltip("Global break-FX particle multiplier (0 = no break FX).")] public float brkFxMul53 = 1f; // v0.0.53 torture breakables
    [Tooltip("Extra particle multiplier for set pieces.")] public float brkBigMul53 = 2.4f; // v0.0.53 torture breakables
    [Tooltip("Break-burst multiplier on a soul's death.")] public float brkSoulMul53 = 0.6f; // v0.0.53 torture breakables
    [Tooltip("Break-burst multiplier on an ice block shatter.")] public float brkIceMul53 = 1f; // v0.0.53 torture breakables
    [Tooltip("Screen shake (art px) on a break, 0 = none.")] public float brkShake53 = 1.5f; // v0.0.53 torture breakables
    [Tooltip("Leave a debris / blood / grain decal where an object broke.")] public bool brkDecal53 = true; // v0.0.53 torture breakables
    [Tooltip("Set-piece break: blood bursts + gore decals spread across the device width (baked-in humans).")] public int brkBigGoreBursts53 = 4; // v0.0.53 B15
    [Tooltip("Set-piece break: particle multiplier of each gore burst.")] public float brkBigGoreMul53 = 1.3f; // v0.0.53 B15
    [Tooltip("Set-piece break: rubble chunks left on the floor (0 = none).")] public int brkRubble53 = 26; // v0.0.53 B15
    [Tooltip("Set-piece break: hit-stop seconds (0 = none).")] public float brkBigHitStop53 = 0.07f; // v0.0.53 B15
    [Tooltip("Set-piece break: extra screen shake (art px).")] public float brkBigShake53 = 4f; // v0.0.53 B15
    [Tooltip("Set-piece break: seconds of the shaking, darkening collapse before it disappears.")] public float brkBigCollapseS53 = 0.45f; // v0.0.53 B15
    [Tooltip("Floor 1 Survive rooms: enemy spawn density multiplier (new / idle players died here).")] public float surviveF1DensityMul53 = 0.7f; // v0.0.53 O1
    [Tooltip("Floor 1 Survive rooms: survive-timer multiplier.")] public float surviveF1SecondsMul53 = 0.8f; // v0.0.53 O1
    [Tooltip("Floor 1 Survive rooms: damage multiplier on non-boss hits to the hero.")] public float surviveF1DmgMul53 = 0.75f; // v0.0.53 O1
    [Tooltip("Second solid band over the device bowl/body, width as a fraction of the sprite width (0 = off).")] public float tortureBowlW53 = 0.5f; // v0.0.53 B17
    [Tooltip("Height of that bowl band as a fraction of the sprite height (sits right above the floor band).")] public float tortureBowlH53 = 0.25f; // v0.0.53 B17
    [Tooltip("Mortar floors swap basic demons for Rice Ghost (Imp), Bloater (Ghoul, exploding) and Grinder Monk (Brute).")] public bool mortarEnemies53 = true; // v0.0.53 torture gaps
    [Tooltip("Chance a basic Imp/Ghoul/Brute on a Mortar floor becomes its Mortar demon.")] public float mortarEnemyChance53 = 0.7f; // v0.0.53 torture gaps
    [Tooltip("Rice Ghost speed multiplier.")] public float riceGhostSpeed53 = 1.25f; // v0.0.53 torture gaps
    [Tooltip("Rice Ghost HP multiplier.")] public float riceGhostHp53 = 0.8f; // v0.0.53 torture gaps
    [Tooltip("Bloater HP multiplier (it explodes like the bomber).")] public float bloaterHp53 = 1.4f; // v0.0.53 torture gaps
    [Tooltip("Bloater speed multiplier.")] public float bloaterSpeed53 = 0.85f; // v0.0.53 torture gaps
    [Tooltip("Grinder Monk HP multiplier.")] public float grinderHp53 = 1.6f; // v0.0.53 torture gaps
    [Tooltip("Grinder Monk speed multiplier.")] public float grinderSpeed53 = 0.9f; // v0.0.53 torture gaps
    [Tooltip("Grinder Monk contact damage multiplier.")] public float grinderDmg53 = 1.25f; // v0.0.53 torture gaps
    [Tooltip("Mortar floor boss = Horse-Face pestle warden (boss_horseface_pestle, MOCK-UP art) on the Spear Warden kit.")] public bool mortarBoss53 = true;
    [Tooltip("t501u Horse-Face: pestle slam impact circle radius in px (JSON r16), centred (18,4) px from the pivot, mirrored for west")] public float horseSlamR53 = 16f;
    [Tooltip("t501u Horse-Face: rice sack flight speed (units/s); flight time clamped 0.45-1.4 s")] public float horseSackSpeed53 = 9f;
    [Tooltip("t501u Horse-Face: rice sack arc height (units) above the straight line")] public float horseSackArc53 = 2.5f;
    [Tooltip("t501u Horse-Face: rice sack landing hit radius (units)")] public float horseSackHitR53 = 1.2f;
    [Tooltip("t501u Horse-Face: rice sack landing damage")] public float horseSackDmg53 = 14f;
    [Tooltip("t502u Horse-Face: leap-in (intro roar) engine jump height in units (art has little height); 0 = off")] public float horseLeapH53 = 2.5f;
    [Tooltip("t502u Horse-Face: leap-in arc duration in seconds")] public float horseLeapS53 = 0.6f;
    [Tooltip("t502u Horse-Face: spin_loop sweep hitbox radius in px around the body centre")] public float horseSpinR53 = 30f;
    [Tooltip("t502u Horse-Face: spin sweep damage per hit")] public float horseSpinDmg53 = 10f;
    [Tooltip("t502u Horse-Face: seconds between spin sweep hits")] public float horseSpinCd53 = 0.5f;
    [Tooltip("t502u Horse-Face: death sprite x shift in px (SE facing, mirrored west); measured lying-body mass sits ~5 px right of the pivot")] public float horseDeathDx53 = -5f;
    [Tooltip("t502u Horse-Face: use the JSON body rect [-13,-46,26,46] as the hurtbox (radius 13 px, centre 23 px up)")] public bool horseJsonBody53 = true;
    [Tooltip("t502u Horse-Face: stomp = rice sack throw only (no shockwave ring damage/bullet burst; the ring tell shrinks to a 20 px foot cue)")] public bool horseNoRings53 = true;
    [Tooltip("t957u Horse-Face: hide the 20 px StompRings foot cue during the stomp windup (sack throw timing kept, 0.8 s). Off = t502u foot rings")] public bool horseNoFootRings957 = true; // v0.0.53 torture gaps
    [Tooltip("Frozen: place ice blocks in the v131 group-scene layout (5 shatterable blocks).")] public bool iceGroup53 = true; // v0.0.53 torture gaps
    [Tooltip("Chance a Frozen fight room uses the group layout instead of scattered blocks.")] public float iceGroupChance53 = 0.7f; // v0.0.53 torture gaps
    [Tooltip("Blood Reaver K1 Martyr (live proxy): +inc damage while below 35% HP.")] public float martyrIncDmg51 = 0.4f; // v0.0.51 damnation pact
    [Tooltip("Empowered damage multiplier (Last Rites, Overcharge triggers, Martyr low HP, Scorched Trail).")] public float empowerMul53 = 1.5f; // v0.0.53 pact nodes
    [Tooltip("BR N8 Last Rites: Empowered seconds after cheating death.")] public float lastRitesEmpS53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("BR N8 Last Rites: invulnerable seconds.")] public float lastRitesInvulnS53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("BR K1 Martyr: max HP multiplier.")] public float martyrMaxHpMul53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("BR N8 Last Rites: max HP multiplier.")] public float lastRitesMaxHpMul53 = 0.9f; // v0.0.53 pact nodes
    [Tooltip("BW K1 Sloth Throne: move speed multiplier.")] public float slothSpeedMul53 = 0.9f; // v0.0.53 pact nodes
    [Tooltip("Pyro K2 Inferno Heart: healing multiplier.")] public float infernoHealMul53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("BR N1 Leech Convert: overheal shield cap (fraction of max HP).")] public float leechShieldCap53 = 0.2f; // v0.0.53 pact nodes
    [Tooltip("BR Crest: overheal shield cap (fraction of max HP).")] public float crestBloodShieldCap53 = 0.3f; // v0.0.53 pact nodes
    [Tooltip("BR K2 Blood Magic: leech multiplier.")] public float bloodMagicLeechMul53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("BR K1 Martyr: below this HP fraction you are Empowered + leech.")] public float martyrLow53 = 0.3f; // v0.0.53 pact nodes
    [Tooltip("BR K1 Martyr: above this HP fraction damage is reduced.")] public float martyrHigh53 = 0.7f; // v0.0.53 pact nodes
    [Tooltip("BR K1 Martyr: damage multiplier above the high threshold.")] public float martyrHighMul53 = 0.8f; // v0.0.53 pact nodes
    [Tooltip("BR K1 Martyr: leech fraction of hit damage while low.")] public float martyrLeech53 = 0.05f; // v0.0.53 pact nodes
    [Tooltip("BR K1 Martyr: max HP leeched per second.")] public float martyrLeechCapPerS53 = 8.0f; // v0.0.53 pact nodes
    [Tooltip("BR K2 Blood Magic: HP cost per cast (fraction of max HP) for the fastest skill.")] public float bloodMagicCostMin53 = 0.005f; // v0.0.53 pact nodes
    [Tooltip("BR K2 Blood Magic: HP cost per cast for the slowest skill.")] public float bloodMagicCostMax53 = 0.02f; // v0.0.53 pact nodes
    [Tooltip("BR K2 Blood Magic: cooldown multiplier (design: 0 = no cooldown; HP cost is the limit).")] public float bloodMagicCdMul53 = 0.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4 Overheat: Fire skill cooldown multiplier (design: 0 = no cooldown; heat is the limit).")] public float overheatCdMul53 = 0.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N7 Hellfire Lance: Pillar fire-rate multiplier.")] public float lanceRateMul53 = 0.7f; // v0.0.53 pact nodes
    [Tooltip("BR N3 Whip Reach: Blood Whip cooldown multiplier.")] public float whipReachCdMul53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4: heat per Fireball.")] public float heatFireball53 = 15.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4: heat per Pillar.")] public float heatPillar53 = 30.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4: fire lockout seconds after venting.")] public float overheatLockS53 = 1.5f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4: vent radius (world units).")] public float overheatVentR53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4: vent damage x the skill's hit.")] public float overheatVentMul53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N4: heat lost per second.")] public float heatDecay53 = 15.0f; // v0.0.53 pact nodes
    [Tooltip("BW Crest: max temporary orbit bones from zone casts.")] public int crestBoneMax53 = 6; // v0.0.53 pact nodes
    [Tooltip("BW Crest: seconds per temporary bone.")] public float crestBoneLife53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("BR N2 Price of Wrath: HP cost per cast (fraction of max HP).")] public float wrathHpCost53 = 0.01f; // v0.0.53 pact nodes
    [Tooltip("BR Crest: delay between extra melee recasts.")] public float crestBloodRecastGap53 = 0.12f; // v0.0.53 pact nodes
    [Tooltip("Storm N4 Static Fork: min bounces given to forking projectiles.")] public int staticForkBounce53 = 2; // v0.0.53 pact nodes
    [Tooltip("Storm N4 Static Fork: damage multiplier.")] public float staticForkDmgMul53 = 0.75f; // v0.0.53 pact nodes
    [Tooltip("Pyro N3 Scorched Trail: half-width of the dash fire line.")] public float trailWidth53 = 0.6f; // v0.0.53 pact nodes
    [Tooltip("BR N7 Sanguine Spears: heal per enemy pierced.")] public float spearHealPerPierce53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("Pyro N1 Kindling: patch radius.")] public float kindlingR53 = 1.2f; // v0.0.53 pact nodes
    [Tooltip("Pyro N1 Kindling: patch seconds.")] public float kindlingLife53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N1 Kindling: burn hit value x Fireball damage.")] public float kindlingDmg53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("Pyro N5 Meteor Pact: every Nth Fireball is a meteor.")] public int meteorEvery53 = 6; // v0.0.53 pact nodes
    [Tooltip("Pyro N5: meteor radius.")] public float meteorR53 = 2.2f; // v0.0.53 pact nodes
    [Tooltip("Pyro N5: meteor damage x Fireball.")] public float meteorDmgMul53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N5: meteor fall delay seconds.")] public float meteorDelay53 = 0.6f; // v0.0.53 pact nodes
    [Tooltip("Pyro N7: beam length.")] public float lanceLen53 = 10.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N7: beam half-width.")] public float lanceWidth53 = 0.45f; // v0.0.53 pact nodes
    [Tooltip("BW N3 Grave Anchor: pool follow speed x hero base speed.")] public float anchorSpeed53 = 0.6f; // v0.0.53 pact nodes
    [Tooltip("BW N2 Ossuary Ward: orbit radius multiplier.")] public float ossuaryOrbitMul53 = 0.85f; // v0.0.53 pact nodes
    [Tooltip("BW N2 Ossuary Ward: seconds for a broken bone to regrow.")] public float ossuaryRegrow53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N8 Thunder Step: blink distance.")] public float thunderStepU53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N8 Thunder Step: base damage to enemies on the path.")] public float thunderStepDmg53 = 6.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N3: fire line seconds.")] public float trailLife53 = 1.5f; // v0.0.53 pact nodes
    [Tooltip("Pyro N3: dash cooldown multiplier.")] public float trailDashCdMul53 = 1.2f; // v0.0.53 pact nodes
    [Tooltip("Storm N2 Capacitor: release damage per charge x Storm Ring hit.")] public float capDmgPerCharge53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("BW N8 Wall of the Dead: wall length.")] public float wallLen53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("BW N8: wall seconds.")] public float wallLife53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("BW N8: shatter damage x Bone Orbit hit.")] public float wallDmgMul53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("BW N6 Calcify: next hit multiplier after the stun.")] public float calcifyNextMul53 = 1.5f; // v0.0.53 pact nodes
    [Tooltip("BW N6 Calcify: stun seconds.")] public float calcifyStunS53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("Storm K2 Equilibrium: same element multiplier.")] public float eqSameMul53 = 0.7f; // v0.0.53 pact nodes
    [Tooltip("Storm K2 Equilibrium: other element multiplier.")] public float eqOtherMul53 = 1.4f; // v0.0.53 pact nodes
    [Tooltip("Storm K2 Equilibrium: seconds an element mark lasts.")] public float eqWindow53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N6 Ember Orbit: ember hit x the triggering hit.")] public float emberDmgMul53 = 0.6f; // v0.0.53 pact nodes
    [Tooltip("BR N6 Frenzy: Blood Nova radius.")] public float frenzyNovaR53 = 3.5f; // v0.0.53 pact nodes
    [Tooltip("BR N6 Frenzy: nova damage x the melee hit.")] public float frenzyNovaMul53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("BR N4 Hemorrhage: burst damage x stored bleed.")] public float hemorrhageMul53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro K1 Ash Covenant: Smolder slow.")] public float smolderSlow53 = 0.2f; // v0.0.53 pact nodes
    [Tooltip("Pyro N8: steam cloud radius.")] public float steamR53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N8: steam cloud seconds.")] public float steamLife53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N2 Cinder Split: jump radius.")] public float cinderR53 = 4.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N6: max embers.")] public int emberMax53 = 6; // v0.0.53 pact nodes
    [Tooltip("Pyro N6: ember seconds.")] public float emberLife53 = 5.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N8 Pyre Feast: heal per burning kill.")] public float pyreHeal53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("BW N4 Bone Legion: kills per skeleton.")] public int legionKills53 = 25; // v0.0.53 pact nodes
    [Tooltip("BW N4: max skeletons.")] public int legionMax53 = 5; // v0.0.53 pact nodes
    [Tooltip("BW K2 Necro Covenant: max skeletons.")] public int necroLegionMax53 = 12; // v0.0.53 pact nodes
    [Tooltip("BW N4: skeleton seconds.")] public float legionLife53 = 20.0f; // v0.0.53 pact nodes
    [Tooltip("BW N4: skeleton hit x Bone Orbit hit.")] public float legionDmgMul53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("Storm: Energy per kill.")] public float energyPerKill53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro N8: contact-hit miss chance inside steam.")] public float steamMiss53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("Storm N3 Cast on Shock: Energy cost.")] public float trigShockCost53 = 20.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N7 Cast on Kill: Energy cost.")] public float trigKillCost53 = 35.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N5 Overcharge: triggers stop below this Energy.")] public float overchargeStop53 = 20.0f; // v0.0.53 pact nodes
    [Tooltip("Storm K1 Unbound Chains: HP per trigger.")] public float unboundHp53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N3: internal cooldown.")] public float trigShockIcd53 = 0.3f; // v0.0.53 pact nodes
    [Tooltip("Storm N7: internal cooldown.")] public float trigKillIcd53 = 0.6f; // v0.0.53 pact nodes
    [Tooltip("Storm N1 Conduit: max extra chains per cast.")] public int conduitMax53 = 8; // v0.0.53 pact nodes
    [Tooltip("Storm N1 Conduit: extra chain damage x the chain hit.")] public float conduitDmgMul53 = 0.7f; // v0.0.53 pact nodes
    [Tooltip("Pyro K2 Inferno Heart: self-burn per second (fraction of max HP, cannot kill).")] public float infernoSelfBurn53 = 0.01f; // v0.0.53 pact nodes
    [Tooltip("Pyro K2: burn aura radius.")] public float infernoAuraR53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Pyro K2: aura damage per 0.5 s (before inc damage).")] public float infernoAuraDmg53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("BW K1 Sloth Throne: zone/orbit area while still.")] public float slothStillArea53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("BW K1: area while moving.")] public float slothMoveArea53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("BW K1: zone/orbit cooldown multiplier while still.")] public float slothStillCd53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("BW Crest/N7: extra bone hit x Bone Orbit hit.")] public float extraBoneDmg53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("BW N1 Marrow Launch: seconds between launches.")] public float launchEvery53 = 3.0f; // v0.0.53 pact nodes
    [Tooltip("BW N1: out-and-back seconds.")] public float launchTime53 = 0.8f; // v0.0.53 pact nodes
    [Tooltip("BW N1: launch distance.")] public float launchDist53 = 6.0f; // v0.0.53 pact nodes
    [Tooltip("Storm: Energy regen per second (design: kills only = 0).")] public float energyRegen53 = 0.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N6 Lightning Rod: strike range.")] public float rodRange53 = 6.0f; // v0.0.53 pact nodes
    [Tooltip("Storm N6: strike x skill hit.")] public float rodDmgMul53 = 1.0f; // v0.0.53 pact nodes
    [Tooltip("BR N2: dash hold seconds to toggle.")] public float wrathHoldS53 = 0.3f; // v0.0.53 pact nodes
    [Tooltip("BR N2: cooldown multiplier while on.")] public float wrathCdMul53 = 0.5f; // v0.0.53 pact nodes
    [Tooltip("BR N5 Red Feast: heal per orb.")] public float redFeastHeal53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("BR N1: shield break nova damage x shield cap.")] public float shieldBurstMul53 = 2.0f; // v0.0.53 pact nodes
    [Tooltip("Overheat / Blood Magic: minimum seconds between casts of one skill when its cooldown is 0 (frame safety, not a cooldown).")] public float pactMinCastGap53 = 0.1f; // v0.0.53 pact nodes
    // ---- t536u Necromancer Pact (18 nodes, 2 groups). Docs/Hellord_necromancer_summoner_skills_v0.2.md sec 9 ----
    [Tooltip("Necromancer (18-node Path): max nodes in one Pact (Keystone uses a slot). VH: 8.")] public int necroMaxSelect536 = 8; // v0.0.53
    [Tooltip("Necromancer (18-node Path): max Keystones taken in one Pact (tree has 4). VH t549u: 2.")] public int necroMaxKeystones549 = 2; // v0.0.53
    [Tooltip("SkillId numbers that count as BONE SKILLS for Group B (3 BoneOrbit, 8 SoulSpears, 9 GravePool). Minion hits never count.")] public int[] necroBoneSkills536 = { 3, 8, 9 }; // v0.0.53
    [Tooltip("B1 Marrow Bite: bone skill damage x.")] public float marrowBiteMul536 = 1.2f; // v0.0.53
    [Tooltip("B2 Splintering: number of shards per bone hit.")] public int splinterN536 = 3; // v0.0.53
    [Tooltip("B2 Splintering: shard damage x the hit.")] public float splinterDmg536 = 0.25f; // v0.0.53
    [Tooltip("B2 Splintering: shard reach (m) to other enemies.")] public float splinterR536 = 3f; // v0.0.53
    [Tooltip("B2 Splintering: min seconds between shatters (perf / orbit spam guard).")] public float splinterIcd536 = 0.12f; // v0.0.53
    [Tooltip("B3 Brittle Mark: damage taken x while marked.")] public float brittleMul536 = 1.1f; // v0.0.53
    [Tooltip("B3 Brittle Mark: mark duration (s).")] public float brittleS536 = 3f; // v0.0.53
    [Tooltip("B4 Dry Ossuary: bone skill cooldown x.")] public float dryOssuaryCd536 = 0.8f; // v0.0.53
    [Tooltip("B5 Grave Reach: bone skill area x (radius-type sizes).")] public float graveReachArea536 = 1.25f; // v0.0.53
    [Tooltip("B6 Calcify: chill stacks for a stun.")] public int calcifyStacks536 = 4; // v0.0.53
    [Tooltip("B6 Calcify: stun seconds (bosses immune).")] public float calcifyStun536 = 1f; // v0.0.53
    [Tooltip("B7 Marrow Harvest: kills per bone charge.")] public int harvestKills536 = 10; // v0.0.53
    [Tooltip("B7 Marrow Harvest: max charges.")] public int harvestMax536 = 4; // v0.0.53
    [Tooltip("B7 Marrow Harvest: damage per charge spent on the next bone hit.")] public float harvestPer536 = 0.25f; // v0.0.53
    [Tooltip("BK Ossuary Tyrant: bone damage x.")] public float tyrantMul536 = 1.6f; // v0.0.53
    [Tooltip("BK Ossuary Tyrant: 2nd hit x the first.")] public float tyrantEcho536 = 0.4f; // v0.0.53
    [Tooltip("BK2 Grave Colossus: bone damage x.")] public float colossusMul536 = 1.4f; // v0.0.53
    [Tooltip("BK2 Grave Colossus: bone area x.")] public float colossusArea536 = 1.5f; // v0.0.53
    [Tooltip("BK2 Grave Colossus: bone cooldown x.")] public float colossusCd536 = 1.4f; // v0.0.53
    [Tooltip("Necro Crest (4+ nodes): minion damage and life x.")] public float necroCrestMinion536 = 1.15f; // v0.0.53
    [Tooltip("Necro Crest (4+ nodes): bone skill damage x.")] public float necroCrestBone536 = 1.15f; // v0.0.53
    [Tooltip("TEST minions: skeleton warriors (melee) given by the F1 test spawn.")] public int testWarriors536 = 3; // v0.0.53
    [Tooltip("TEST minions: skeleton archers (ranged) given by the F1 test spawn.")] public int testArchers536 = 2; // v0.0.53
    [Tooltip("Minion cap (all minions incl. temporary).")] public int minionCap536 = 10; // v0.0.53
    [Tooltip("Minion base hit damage (x hero inc. damage and Intensity).")] public float minionDmg536 = 9f; // v0.0.53
    [Tooltip("Minion base life.")] public float minionHp536 = 60f; // v0.0.53
    [Tooltip("Minion life lost per second while touching a demon, x demon contact damage.")] public float minionHurtMul536 = 1.0f; // v0.0.53
    [Tooltip("Melee minion attack interval (s).")] public float minionMeleeRate536 = 0.8f; // v0.0.53
    [Tooltip("Ranged minion attack interval (s).")] public float minionShotRate536 = 1.1f; // v0.0.53
    [Tooltip("Ranged minion range (m).")] public float minionRange536 = 6f; // v0.0.53
    [Tooltip("Undead minion respawn time (s) after dying.")] public float minionRespawn536 = 6f; // v0.0.53
    [Tooltip("A2 Iron Marrow: minion life x.")] public float ironMarrowHp536 = 1.4f; // v0.0.53
    [Tooltip("A3 Hollow Haste: minion attack speed x.")] public float hollowHaste536 = 1.2f; // v0.0.53
    [Tooltip("A4 Quick Grave: respawn time x.")] public float quickGrave536 = 0.7f; // v0.0.53
    [Tooltip("A5 Grave Edge: melee minion damage x (plus 1 extra target).")] public float graveEdgeMul536 = 1.25f; // v0.0.53
    [Tooltip("A5 Grave Edge: extra target reach from the main target (m).")] public float graveEdgeR536 = 1.4f; // v0.0.53
    [Tooltip("A6 Marrow Shot: extra ranged minion range (m).")] public float marrowShotRange536 = 2f; // v0.0.53
    [Tooltip("A7 Ossuary Order: kills per temp skeleton.")] public int ossuaryKills536 = 20; // v0.0.53
    [Tooltip("A7 Ossuary Order: max temp skeletons at once.")] public int ossuaryMax536 = 3; // v0.0.53
    [Tooltip("A7 Ossuary Order: temp skeleton lifetime (s).")] public float ossuaryLife536 = 15f; // v0.0.53
    [Tooltip("AK Crypt King: minion damage x.")] public float cryptDmg536 = 1.6f; // v0.0.53
    [Tooltip("AK Crypt King: minion life x.")] public float cryptHp536 = 1.4f; // v0.0.53
    [Tooltip("AK Crypt King: your non-minion skill damage x.")] public float cryptSelf536 = 0.5f; // v0.0.53
    [Tooltip("AK2 Legion of One Hundred: extra minion cap.")] public int legionCap536 = 4; // v0.0.53
    [Tooltip("AK2 Legion of One Hundred: damage x per minion.")] public float legionDmg536 = 0.8f; // v0.0.53
    [Header("t561u Summon Skeleton Warriors / Archers (placeholder numbers, Necro v0.2 doc)")]
    [Tooltip("Max level of the two summon skills = 5, same as every other skill (HData.MaxSkillLevel).")] public int summonMaxLv561 = 5; // v0.0.53
    [Tooltip("Global minion cap (all summons + temp minions). AK2 Legion adds legionCap536.")] public int minionCap561 = 12; // v0.0.53
    [Tooltip("Skeleton Warriors: count per skill level 1..5.")] public int[] skelWarCount561 = { 2, 2, 3, 3, 4 } /* t1012u A spirit */; // v0.0.53
    [Tooltip("Skeleton Warriors: hit damage per level 1..5.")] public float[] skelWarDmg561 = { 9, 12, 15, 18, 21 }; // v0.0.95 t1014u L1 floor + compress
    [Tooltip("Skeleton Warriors: respawn time (s) per level 1..5.")] public float[] skelWarResp561 = { 4.5f, 5.5f, 5, 5, 4.5f }; // v0.0.95 t1014u L1 respawn
    [Tooltip("Skeleton Warriors: base life.")] public float skelWarHp561 = 55f; // v0.0.95 t1014u
    [Tooltip("Skeleton Warriors: attack interval (s).")] public float skelWarRate561 = 0.9f; // v0.0.53
    [Tooltip("Skeleton Warriors: move speed (m/s).")] public float skelWarSpeed561 = 5.5f; // v0.0.53
    [Tooltip("Skeleton Warriors: melee reach (m, centre to centre).")] public float skelWarReach561 = 0.9f; // v0.0.53
    [Tooltip("Skeleton Archers: count per level 1..5.")] public int[] skelArcCount561 = { 1, 1, 2, 2, 3 } /* t1012u A spirit */; // v0.0.53
    [Tooltip("Skeleton Archers: arrow damage per level 1..5.")] public float[] skelArcDmg561 = { 8, 10, 12, 14, 16 }; // v0.0.95 t1014u L1 floor + compress
    [Tooltip("Skeleton Archers: respawn time (s) per level 1..5.")] public float[] skelArcResp561 = { 6f, 7.5f, 7, 6, 5.5f }; // v0.0.95 t1014u L1 respawn
    [Tooltip("Skeleton Archers: base life.")] public float skelArcHp561 = 32f; // v0.0.95 t1014u
    [Tooltip("Skeleton Archers: shot interval (s).")] public float skelArcRate561 = 1.2f; // v0.0.53
    [Tooltip("Skeleton Archers: shot range (m).")] public float skelArcRange561 = 8f; // v0.0.53
    [Tooltip("Skeleton Archers: idle ring radius around the player (m).")] public float skelArcRing561 = 3.5f; // v0.0.53
    [Tooltip("Summon life +x per skill level above 1.")] public float skelHpPerLv561 = 0.1f; // v0.0.53
    [Tooltip("Share of the hero's increased damage that minions get.")] public float minionPlayerDmgShare561 = 0.5f; // v0.0.53
    [Tooltip("Minions only pick targets within this distance of the player (m).")] public float minionLeash561 = 7f; // v0.0.53
    [Tooltip("Minions farther than this from the player teleport back (m).")] public float minionTeleport561 = 10f; // v0.0.53
    [Tooltip("Bullet block radius of a minion body (m).")] public float minionBlockR561 = 0.45f; // v0.0.53
    [Tooltip("Blocked bullet damage taken by the minion x bullet damage.")] public float minionBulletHurt561 = 1f; // v0.0.53
    [Header("t590u/t591u minion FSM (research: Docs/Hellord_poe2_minion_ai_research.md) - (ROAM / SEEK / FIGHT / RETURN / RECOVER)")]
    [Tooltip("Minions seek any demon within this distance of the hero (m). Teleport-back stays at minionTeleport561.")] public float minionSeekR590 = 8.5f;
    [Tooltip("Idle roam ring around the hero: inner radius (m).")] public float minionRoamMin590 = 4f; // v0.0.56 t591u
    [Tooltip("Idle roam ring around the hero: outer radius (m).")] public float minionRoamMax590 = 6f; // v0.0.56 t591u
    [Tooltip("No enemy and farther than this from the hero -> RETURN to the ring (m).")] public float minionReturnR590 = 7.5f; // v0.0.56 t591u
    [Tooltip("Roam: wait between new roam points, min (s).")] public float minionRoamWaitMin590 = 1.5f;
    [Tooltip("Roam: wait between new roam points, max (s).")] public float minionRoamWaitMax590 = 3.5f;
    [Tooltip("Roam walk speed x skelWarSpeed561.")] public float minionRoamSpeed590 = 0.55f;
    [Tooltip("RECOVER: no progress for this long -> pick a new point / drop the target (s).")] public float minionStuckS590 = 0.8f;
    [Tooltip("RECOVER: still stuck after this long -> blink to the roam ring (s).")] public float minionStuckBlinkS590 = 2f;
    [Header("t582u decor")]
    [Tooltip("t582u kill-switch: torture decor, set pieces, mortar pit, sacks, rice, damned souls, watchers, rope monk and ALL breakable decor props. Off = none spawn (no collision, no decor enemies).")] public bool decorEnabled582 = false;
    [Header("t582u minion AI (PoE2-style)")]
    [Tooltip("t584u: demons switch to chasing a minion within this distance if it is closer than the hero (m). 0 = never.")] public float enemyMinionAggro584 = 3.5f;
    [Tooltip("t584u: hard minimum distance between a minion and the hero (m).")] public float minionHeroHard584 = 0.6f;
    [Tooltip("Demons within this distance of the hero are attacked first (m).")] public float minionThreatR582 = 3.5f; // t590u
    [Tooltip("Minions pick their own nearest target within this distance of themselves (m), still inside the leash.")] public float minionAggroR582 = 6f;
    [Tooltip("Warrior chase speed x skelWarSpeed561.")] public float minionChaseMul582 = 1.15f;
    [Tooltip("Archer preferred standoff distance to its target (m).")] public float archerStandoff582 = 5f; // v0.0.56 t591u
    [Tooltip("Archer backs off when a target is closer than this (m).")] public float archerKiteR582 = 2.5f; // t590u
    [Tooltip("Archer back-off speed (m/s).")] public float archerKiteSpeed582 = 4f;
    [Tooltip("t594u: archer arrow release delay after the shot starts = shoot f6 at 12 fps (s).")] public float archerReleaseS594 = 0.5f; // v0.0.57 t594u
    [Tooltip("t594u: bone arrow travel speed (m/s). Damage lands when the arrow reaches the enemy.")] public float archerArrowSpeed594 = 12f; // v0.0.57 t594u
    [Tooltip("t594u: max arrow flight time before it vanishes (s).")] public float archerArrowLife594 = 1.5f; // v0.0.57 t594u
    [Tooltip("t594u: if the target dies mid-flight the arrow retargets the nearest demon within this distance of it (m), else flies on to the last point.")] public float archerRetargetR594 = 2f; // v0.0.57 t594u
    [Header("t602u minion anim / AI anti-thrash")]
    [Tooltip("Warrior: after a swing starts it is locked (no move, stays in FIGHT) this long (s).")] public float warAtkLock602 = 0.45f; // v0.0.58 t602u
    [Tooltip("Archer: after a shot starts it stays in FIGHT this long (draw + release f6) (s).")] public float arcAtkLock602 = 0.55f; // v0.0.58 t602u
    [Tooltip("Minimum time in a state before FIGHT may drop back to SEEK (s).")] public float minionStateMin602 = 0.4f; // v0.0.58 t602u
    [Tooltip("FIGHT -> SEEK only when the target is this far beyond reach (m) (dead zone).")] public float minionFightExit602 = 0.6f; // v0.0.58 t602u
    [Tooltip("Do not step toward a goal closer than this (m): kills micro-steps.")] public float minionMoveDead602 = 0.2f; // v0.0.58 t602u
    [Tooltip("Walk anim keeps playing this long after the last intended step (s): no walk/idle flicker.")] public float minionWalkHold602 = 0.15f; // v0.0.58 t602u
    [Tooltip("Facing hysteresis: switch E <-> N/S only when the new axis is this many times stronger.")] public float minionDirHyst602 = 1.35f; // v0.0.58 t602u
    [Tooltip("Hurt tint flash length (s). Anim is never reset by damage.")] public float minionHurtFlash602 = 0.1f; // v0.0.58 t602u
    [Tooltip("Min gap between hurt flashes (s) so per-tick contact damage doesn't strobe.")] public float minionHurtCd602 = 0.35f; // v0.0.58 t602u
    [Tooltip("Minion-minion spacing radius (m).")] public float minionSepR582 = 0.6f;
    [Tooltip("Minions keep this gap from the hero (m).")] public float minionHeroGap582 = 0.8f;
    [Tooltip("Strength of the spacing push.")] public float minionSepPush582 = 10f; // t584u: was 6
    [Tooltip("Art: world height scale of skeleton summon sprites (PPU 16 base).")] public float skelArtScale561 = 1f; // v0.0.53
    [Header("t716u: player control (Manual mode, skill belt, Controls page)")]
    [Tooltip("t716u: first launch (no saved choice) starts in MANUAL control. Saved choice in PlayerPrefs wins.")] public bool manualDefault716 = true;
    [Tooltip("t716u: first launch movement = mouse click-to-move (false = WASD). Saved choice wins.")] public bool mouseMoveDefault716 = true;
    [Tooltip("t716u: holding a press-to-cast key re-casts each time the cooldown is ready (PoE2 feel). Off = one cast per press.")] public bool holdRepeat716 = true;
    [Tooltip("t716u: Manual mode skill cap (new-skill cards show while you own fewer). Auto keeps the old 3-slot unlocks.")] public int manualSkillCap716 = 5; // t818u: was 8
        [Tooltip("t718u: seconds after a new-skill card before the slot pick accepts input (all slot keys must also be released)")] public float slotPickDelay718 = 0.3f;
        [Header("t720u: Auto + mouse aim")]
        [Tooltip("t720u: Manual control (skills on keys, slot pick, Controls page). OFF = shelved; skills auto-fire")] public bool manualControl720 = false;
        [Tooltip("t720u: Auto mode aims at the cursor: projectiles toward it, ground AoE at it (clamped to groundAoeRange716)")] public bool autoMouseAim720 = true;
        [Tooltip("t720u: draw keycap / mouse badges on the skill belt")] public bool showKeyBadges720 = false;
        [Tooltip("t720u: use the 2x4 skill belt instead of the old 3-skill bar")] public bool skillBelt720 = true;
        [Tooltip("t718u: a FULL slot needs a second press to be replaced")] public bool slotReplaceConfirm718 = true;
    [Tooltip("t716u: ground AoE (Curse Rune, Hellfire Pillar, Grave Pool) lands at the cursor, clamped to this range from the hero (m).")] public float groundAoeRange716 = 7f;
    [Tooltip("t979u v0.0.86: Fire Wall lands at the cursor (clamped to groundAoeRange716) when cursor aim is on; off = v0.0.85 fixed fireWallDist737 ahead.")] public bool fireWallCursor986 = true;
    [Tooltip("t716u: Hellfire Pillar extra pillars scatter this far around the cursor point (m).")] public float pillarScatter716 = 1.4f;
    [Tooltip("t716u: Chain Lightning first target = enemy nearest the cursor within this radius of the cursor (m); none = nearest to hero.")] public float chainCursorPick716 = 2.5f;
    [Tooltip("t716u: mouse mode: LMB counts as 'on an enemy' when one is within this radius of the cursor (m).")] public float enemyPickR716 = 0.9f;
    [Tooltip("t716u: mouse mode: stop walking this close to the click point (m).")] public float moveStopDist716 = 0.2f;
    [Tooltip("t716u: Blood Whip is a channel (cast while held). Off = press-to-cast like the rest.")] public bool whipChannel716 = true;
    [Tooltip("t716u belt: left / bottom margin (HUD px, 640x360).")] public float beltX716 = 4f, beltBottom716 = 4f;
    [Tooltip("t716u belt: cell size + gap (HUD px). 39j V1 mock: 58x42.")] public float beltCellW716 = 58f, beltCellH716 = 42f, beltGap716 = 2f;
    [Tooltip("t716u belt: support gem size (HUD px, 2x2 grid).")] public float beltGem716 = 10f;
    [Tooltip("t716u belt: show the numbered support tooltip on mouse hover.")] public bool beltTooltip716 = true;

    [Header("t818u: Demonic + Heaven-Defying sin EFFECTS (Docs/Hellord_sins_design_v0.4.md sec 5). Rolled numbers come from the sin's effect text; everything else is here. NOT play-tested.")]
    [Tooltip("t818u: master switch: Demonic/HD sins are live (effects + drops). Off = old 'not implemented' behaviour (no effect, drops follow sinDropUnimplemented737).")] public bool sinDemonicHdLive818 = true;
    [Tooltip("t818u: drawback size per sin #1..#45 (index = doc number - 1). Fractions unless noted: #26 = flat dmg to you, #38 = Fireball cooldown multiplier, #42 = max warriors, #44 = max HP fraction per pass.")]
    public float[] sinDraw818 = { 0.10f, 0.10f, 0.20f, 0.15f, 0.10f, 0.04f, 0.15f, 0.06f, 0.06f, 0.06f, 0.10f, 0.06f, 0.10f, 0.15f, 0.15f, 0.10f, 0.10f, 0.05f, 0.15f, 0.05f, 0.15f, 0.15f, 0.10f, 0.10f, 0.10f, 4f, 0.25f, 0.12f, 0.10f, 0.20f, 0.30f, 0.15f, 0.15f, 0.30f,
        0.25f, 0.30f, 0.04f, 2f, 0.40f, 0.30f, 0.03f, 2f, 0.35f, 0.005f, 0.25f };
    [Tooltip("t818u: Demonic '+1 stat' per sin #1..#34 (SinStat index: 0 MaxHp 1 Speed 2 Pickup 3 Armor 4 Crit 5 Dmg 6 Fire 7 Cold 8 Lightning 9 Phys 10 Cooldown 11 Minion 12 Area 13 Duration 14 Bleed 15 Xp 16 DashCd 17 Curse/Soul). #1-20 roll in the Mortal SINGLE range, #21-34 in the PAIR range.")]
    public int[] sinDemBonusStat818 = { 17, 17, 0, 6, 8, 9, 17, 5, 3, 6, 9, 15, 8, 17, 9, 6, 10, 11, 16, 5, 11, 12, 14, 8, 15, 17, 7, 11, 5, 8, 6, 0, 9, 0 };
    [Tooltip("t818u: #24 shocked-enemy radius around you (u); #26 explosion radius that hurts you (u)")] public float sin24R818 = 2f, sin26R818 = 1.5f;
    [Tooltip("t818u: #9 knocked enemies: the Knockback support marks them (seconds come from the roll)")] public bool sin9NeedsKnockSupport818 = true;
    [Tooltip("t818u: #18 seconds per free bullet block window")] public float sin18BlockWindowS818 = 1f;
    [Tooltip("t818u: #19 burn patch radius (u), tick (s)")] public float sin19R818 = 1.1f, sin19Tick818 = 0.5f;
    [Tooltip("t818u: #23 'standing still' = hero moved less than this (u/s)")] public float sin23StillSpeed818 = 0.2f;
    [Tooltip("t818u: #7 Grave Pool hits chill with this strength (chill seconds multiplier)")] public float sin7ChillK818 = 1f;
    [Tooltip("t818u: #36 ghosts: spears per wave, seconds between waves, pierce, flight seconds, homing turn")] public int sin36PerWave818 = 2, sin36Pierce818 = 30; public float sin36Interval818 = 0.6f, sin36Life818 = 4f, sin36Turn818 = 5f;
    [Tooltip("t818u: #37 pull strength (push units per hit), max Gluttony stacks, seconds before stacks fall off")] public float sin37Pull818 = 3f, sin37StackDecayS818 = 8f; public int sin37StackMax818 = 10;
    [Tooltip("t818u: #38 sun: speed multiplier, damage growth per demon passed, splash radius growth (u), max growth steps, life (s)")] public float sin38Speed818 = 0.35f, sin38GrowDmg818 = 0.08f, sin38GrowR818 = 0.08f, sin38Life818 = 5f; public int sin38MaxGrow818 = 25;
    [Tooltip("t818u: #39 max risen demons (temp minions) at once")] public int sin39MaxRisen818 = 10;
    [Tooltip("t818u: #40 ring of fire (on Hellfire Pillar): radius, band width (u), seconds, tick (s)")] public float sin40R818 = 2.6f, sin40W818 = 0.7f, sin40Secs818 = 4f, sin40Tick818 = 0.3f;
    [Tooltip("t818u: #41 mushroom: radius (u), pool seconds, burst damage x Grave Pool damage")] public float sin41R818 = 1.4f, sin41Secs818 = 3f, sin41Burst818 = 1.5f;
    [Tooltip("t818u: #42 guardian cleave radius around the struck demon (u); +damage per warrior over the cap")] public float sin42CleaveR818 = 1.6f, sin42ExtraDmg818 = 0.25f, sin42CleaveMul818 = 0.6f;
    [Tooltip("t818u: #44 Tribulation bolt: damage x Chain hit, radius (u), self hit (fraction of max HP)")] public float sin44TribMul818 = 8f, sin44TribR818 = 3f, sin44TribSelf818 = 0.15f;
    [Tooltip("t818u: #45 wheel: radius x, spin x")] public float sin45RMul818 = 2.2f, sin45Spin818 = 0.5f;
    [Tooltip("t818u HUD: sins frame (3 slots) above the belt: slot px, gap to the belt px")] public float sinHudSlot818 = 20f, sinHudGap818 = 3f;
    [Tooltip("t902u v0.0.71 sin icons: sheet columns (sin name per 20px column of ui_v52_sin_icons_20), icon px, offset inside slot; sins not listed keep the text label")] public string[] sinIconNames902 = { "Greed of the Ox Pit", "Killing of Naihe Bridge", "Greed of the Needle-Throat", "Delusion of the Iron Bed", "Slander of the Tongue Court", "Hatred of the Sawing Court", "Hatred of the Burning Hell", "Hatred of the Frozen Hell", "Hatred of the Thunder Court", "Hatred of the Knife Mountain", "Killing of the Gong", "Idle Speech of the Grave", "Wrong View of the Nine Springs", "Greed and Delusion of the Iron City", "Greed and Killing of the Ghost Road", "Hatred and Slander of the Judge's Desk", "Hatred and Wrong View of the Burning City", "Hatred and Killing of the Eight Cold Hells", "Hatred and Slander of the Storm Gate", "Hatred of the Blood Pool", "Idle Speech and Greed of the Ghost Festival", "Greed and Theft of the Hungry Ghost", "Killing of the Black Wind", "Wrong View of the Wheel of Rebirth", "Hatred and Killing of the Hanged Ghost", "Spear of the Tongue-Ripping Court", "Meng Po's Bitter Ladle", "Mouth of the Needle-Throat Ghost", "Veil of the Sea of Fire", "Thunder Drum of the Fifth Court", "Hook of the Blood Pool", "Lantern of the Naihe Ferry", "Brush of the Book of Life and Death", "Ox-Head's Iron Fork", "Cinder Beads of the Burning Hell", "Cleaver of the Sawing Court", "Bone Abacus of the Karma Ledger", "Incense of the Ghost Festival", "Death Bell of White Impermanence", "Ribs of the Knife Mountain", "Joss Paper of the Burned Offering", "Shard of the Karma Mirror", "Oath of the Yin Soldiers", "Fire Wheel of Nezha's Ghost", "Hell Bank Note of the First Hand", "Dirge of the Unburied", "The Broken Talisman of Zhong Kui", "Nails of the Bronze Pillar", "Red Thread of the Ghost Bride", "Second Belly of the Hungry Ghost", "The Unmourned Child", "Hand of the Frozen Hell", "Black Impermanence's Chain", "Blade of the Beheading Platform", "Nine-Forked Lie of the Tongue Court", "Heart of the Oil Cauldron", "Debt Ledger of the Ten Courts", "Bone Throne of the Pit of Ox-Head", "Breath of the Hanged Ghost", "Pearl of the Dragon King", "Ten Thousand Ghost Banner", "Tongue of the Needle-Throat Ghost", "Golden Crow of the Sun", "Lamp of the Nine Nether Springs", "Ring of the Burning City", "Ghost Mushrooms of Fengdu", "Ox-Head and Horse-Face Answer the Call", "Blade of the Eight Cold Hells", "Lightning Rod of the Heavenly Tribulation", "Samsara Wheel of Six Rings" };
    public bool sinIconsOn902 = true; public float sinIconPx902 = 20f, sinIconOffX902 = 0f, sinIconOffY902 = 0f, sinIconCell902 = 20f;
    [Tooltip("t903 v0.0.72 sins frame sprites (step49d/step50p slot24): on/off, slot cell px, slot origin in frame, slot stride, icon inset, frame x offset vs belt; HD running light: on, frames, s per frame (mockup 0.05 = 4px/frame), pause between loops s")] public bool sinSlotSprites903 = true, sinRunLight903 = true;
    public float sinSlotCell903 = 24f, sinSlotOrigin903 = 4f, sinSlotStride903 = 26f, sinIconInset903 = 2f, sinFrameOffX903 = 0f, sinRunFrameS903 = 0.05f, sinRunPauseS903 = 0f; /* t914u: was 0.7, VH wants continuous run */ public int sinRunFrames903 = 23;
    // t928u shader FX (Hellord/PixelAoE). Off = old pack FX. Gameplay radius/damage unchanged; the shader only draws.
    [Tooltip("t928u: master switch for the PixelAoE shader FX (Fireball blast, Curse zone veins, Grave Pool liquid). Off = old FX.")] public bool fxShaderOn = true;
    [Tooltip("t928u/t1001u: per-skill. Fireball shader disc OFF by default (VH: purple bubble). Curse/Grave stay on.")] public bool fxShaderFireball928 = false, fxShaderCurse928 = true, fxShaderGrave928 = true;
    [Tooltip("t928u: Resources names (HellordFX928/...) so textures can be swapped without code. Empty = keep the material's texture.")] public string fxNoiseTex928 = "fx928_noise128", fxCellTex928 = "fx928_cell128", fxRampCurse928 = "fx928_ramp_curse", fxRampGrave928 = "fx928_ramp_grave", fxRampFire928 = "fx928_ramp_fireball";
    [Tooltip("t928u: Fireball burst life in seconds (14 frames at 30 fps in the approved sample)")] public float fxFireLifeS928 = 0.467f;
    [Tooltip("t928u: Fireball shader also keeps the old big-impact nova puffs (off = shader blast replaces them; flash, ring, sparks, shake stay)")] public bool fxFireKeepNova928 = false;
    [Tooltip("t928u: zone spawn grow time / fade time (s) for curse and grave dissolve")] public float fxZoneGrowS928 = 0.25f, fxZoneFadeS928 = 0.3f;
    [Tooltip("t928u: curse tick pulse decay (s)")] public float fxCursePulseS928 = 0.13f;
    [Tooltip("t928u: sorting order of the shader ground quads (pack ground FX = -20000)")] public int fxShaderOrder928 = -19990;
    [Tooltip("t928u: curse shader params")] public float fxCurseCellPx928 = 1.2f, fxCurseWarp928 = 0.35f, fxCurseEdgeNoise928 = 0.35f, fxCurseVeinW928 = 0.11f, fxSquash928 = 0.75f; public Vector2 fxCurseFlow928 = new Vector2(0.02f, 0.012f);
    [Tooltip("t928u: grave shader params")] public float fxGraveCellPx928 = 1f, fxGraveWarp928 = 0.45f, fxGraveEdgeNoise928 = 0.22f, fxGraveEdgeW928 = 0.16f, fxGraveNGain928 = 2.2f; public Vector2 fxGraveFlow928 = new Vector2(0.03f, 0.02f), fxGraveScroll928 = new Vector2(0.02f, 0.01f);
    [Tooltip("t928u: shader stepped animation fps")] public float fxTimeStep928 = 12f;
    [Tooltip("t930u batch3 switches: Grave Pool merged field, #40 Burning City hybrid, #41 Ghost Mushrooms v2, #14 Death Bell, #36 Ghost Banner wisps. Off = previous FX.")] public bool fxGraveMerge930 = true, fxShaderCity930 = true, fxShaderMush930 = true, fxShaderBell930 = true, fxShaderGhost930 = true;
    [Tooltip("t930u ramps (Resources/HellordFX928/...)")] public string fxRampCity930 = "fx930_ramp_city", fxRampMush930 = "fx930_ramp_mush", fxRampBell930 = "fx930_ramp_bell", fxRampGhost930 = "fx930_ramp_ghost";
    [Tooltip("t930u #40: shader ember band flame height px (hand tongues stand on it)")] public float fxCityBandH930 = 6f;
    [Tooltip("t930u #40/#14: sorting order of the FRONT half (near-side tongues, gong shock); back half uses fxShaderOrder928")] public int fxCityFrontOrder930 = 1999, fxBellFrontOrder930 = 1999;
    [Tooltip("t930u #41: mushroom pool flow, spawn puff colour")] public Vector2 fxMushFlow930 = new Vector2(0.02f, 0.03f); public Color fxMushBurstCol930 = new Color(0.75f, 0.8f, 0.86f);
    [Tooltip("t930u #14: telegraph point where RuneBurst + CurseZone fire (0..1 of 0.6 s). Must stay 0.6 = same as HellFX.RuneCircle.")] public float fxBellFlareK930 = 0.6f;
    [Tooltip("t930u #36: ghost wisp length/width/wave px, wave freq, tail dissolve gain, sorting order")] public float fxGhostLenPx930 = 26f, fxGhostWidthPx930 = 3.4f, fxGhostWavePx930 = 2.2f, fxGhostWaveFreq930 = 0.3f, fxGhostGain930 = 0.8f; public int fxGhostOrder930 = 2001;
    [Tooltip("t903u: text abbreviation inside sin slots without an icon (VH: off)")] public bool sinSlotText903 = false;
    [Tooltip("t818u HUD: HD trigger glint: seconds for one lap, min seconds between glints")] public float sinGlintS818 = 0.8f, sinGlintCd818 = 1.2f;
    [Tooltip("t934u VH: one-shot trigger glint around the whole sins frame. Off (VH t934u) - the HD slot running light stays")] public bool sinFrameGlint934 = false;
    [Tooltip("t818u: Duration stat (Mortal pair 'Wheel of Rebirth') scales burn/shock/bleed/curse times, curse zones, grave pools and sin patches. Cap.")] public float sinDurationMax818 = 2f;

    [Header("t737 / t770u: INFERNAL SINS phase 1 (Docs/Hellord_sins_design_v0.4.md). NOT play-tested.")]
    [Tooltip("737: master switch for sin drops + effects")] public bool sinEnabled737 = true;
    [Tooltip("737: any demon kill: Mortal chance / Demonic chance")] public float sinDemonNormalChance737 = 0.001f, sinDemonRareChance737 = 0.0001f;
    [Tooltip("737: boss kill: Mortal / Demonic / Heaven-Defying chance (roll order HD > Demonic > Mortal, first hit wins)")] public float sinBossNormalChance737 = 0.05f, sinBossRareChance737 = 0.01f, sinBossHeavenDefyingChance737 = 0.01f;
    [Tooltip("737: OFF = sins whose effect is not implemented yet (all Demonic + Heaven-Defying in phase 1) are left out of drops. ON = they drop (collectable, do nothing when equipped)")] public bool sinDropUnimplemented737 = false;
    [Tooltip("737: a roll that lands on an owned sin rerolls this many times inside its tier")] public int sinDupRerolls737 = 1;
    [Tooltip("737 t784u: duplicate with a worse/equal roll converts to Souls: Mortal / Demonic / Heaven-Defying")] public int[] sinDupSouls737 = { 20, 100, 400 };
    [Tooltip("737: extra Sigils for an equipped Heaven-Defying duplicate")] public int sinDupHeavenDefyingSigils737 = 1;
    [Tooltip("737: slot unlock costs (slot 1,2,3): Souls / Sigils")] public int[] sinSlotSoulCost737 = { 100, 500, 1200 }, sinSlotSigilCost737 = { 1, 4, 10 };
    [Tooltip("737: max sin slots")] public int sinSlotsMax737 = 3;
    [Tooltip("737: equip caps per tier (3 = no cap, t751u)")] public int sinHeavenDefyingMax737 = 3, sinDemonicMax737 = 3;
    [Tooltip("737: full-slot swap card pauses the run (t765u)")] public bool sinSwapPause737 = true;
    [Tooltip("737: swap card ignores input for this long + until keys are released (t718u guard)")] public float sinSwapDelay737 = 0.3f;
    [Tooltip("737 t784u ROLLED STATS: Mortal single sins (13, one per stat) roll range MIN, in order MaxHp, Speed, Pickup, Armor, Crit, Dmg, Fire, Cold, Lightning, Phys, Cooldown, Minion, Area. Fractions (0.08 = +8%), armor = flat, cooldown = reduction")] public float[] sinSingleMin737 = { 0.08f, 0.02f, 0.08f, 1f, 0.02f, 0.03f, 0.06f, 0.06f, 0.06f, 0.06f, 0.02f, 0.06f, 0.05f };
    [Tooltip("737 t784u: Mortal single roll range MAX, same order")] public float[] sinSingleMax737 = { 0.20f, 0.07f, 0.22f, 5f, 0.06f, 0.09f, 0.18f, 0.18f, 0.18f, 0.18f, 0.07f, 0.18f, 0.14f };
    [Tooltip("737 t784u: Mortal single weights (13), same order")] public int[] sinSingleW737 = { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };
    [Tooltip("737 t784u: Mortal pair roll MIN per stat (SinStat order: MaxHp, Speed, Pickup, Armor, Crit, Dmg, Fire, Cold, Lightning, Phys, Cooldown, Minion, Area, Duration, Bleed, Xp, DashCd, CurseSoul). Each stat of a pair rolls on its own")] public float[] sinPairMin737 = { 0.06f, 0.04f, 0.12f, 3f, 0.03f, 0.04f, 0.09f, 0.09f, 0.09f, 0.09f, 0.04f, 0.09f, 0.06f, 0.06f, 0.09f, 0.05f, 0.05f, 0.09f };
    [Tooltip("737 t784u: Mortal pair roll MAX per stat, same order")] public float[] sinPairMax737 = { 0.14f, 0.08f, 0.24f, 7f, 0.07f, 0.10f, 0.19f, 0.19f, 0.19f, 0.19f, 0.08f, 0.19f, 0.14f, 0.14f, 0.19f, 0.11f, 0.11f, 0.19f };
    [Tooltip("737 t784u: Demonic numeric effect lines roll from base x LOW to base x HIGH (base = doc sec 5 number). Drawbacks fixed")] public float sinRollLowDemonic737 = 0.7f, sinRollHighDemonic737 = 1.3f;
    [Tooltip("737 t784u: Heaven-Defying numeric effect lines roll range (base x LOW .. x HIGH). Drawbacks fixed")] public float sinRollLowHD737 = 0.8f, sinRollHighHD737 = 1.25f;
    [Tooltip("737 t784u: BETTER ROLL notice card: seconds on screen (does not pause)")] public float sinBetterCardS737 = 4f;
    [Tooltip("737 t784u: sound hook played for a BETTER ROLL (falls back silently if the hook has no clip)")] public string sinBetterSfx737 = "ui_confirm";
    [Tooltip("737 t784u: roll quality shown as PERFECT at or above this (0..1)")] public float sinPerfectAt737 = 0.995f;
    [Tooltip("818 t818u: Mortal pair drop weights (12, one per combo, order: HP+Armor, HP+Speed, Dmg+Crit, Fire+Area, Cold+Cooldown, Lightning+Crit, Phys+Bleed, Minion+HP, Pickup+XP, Speed+DashCd, Area+Duration, Curse+Cooldown)")] public int[] sinPairW737 = { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };
    [Tooltip("737: Demonic #1-34 weights (doc sec 5)")] public int[] sinDemonicW737 = { 100, 100, 100, 100, 100, 50, 50, 50, 100, 50, 50, 50, 20, 20, 20, 50, 50, 100, 5, 20, 50, 100, 100, 50, 20, 50, 100, 50, 20, 5, 20, 20, 5, 100 };
    [Tooltip("737: Heaven-Defying #35-45 weights (doc sec 5)")] public int[] sinHeavenDefyingW737 = { 50, 20, 50, 20, 20, 50, 50, 20, 50, 20, 20 };
    [Tooltip("737 DEBUG: sin cheats (test menu SINS tab: grant sins/Sigils, force drops). OFF by default; also turned on by the -sincheats launch arg or the SINS tab button")] public bool sinDebugCheats737 = false;
    [Tooltip("737 DEBUG: with cheats on + Force drops, every kill rolls with this chance per tier")] public float sinDebugForceChance737 = 1f;
    // ---------------- t934u v0.0.79 ----------------
    [Tooltip("t934u #19 Nezha Fire Wheel: ON = rolling wheel hits along the dash + burn trail (OFF = old t818u burn patch)")] public bool sin19Wheel934 = true;
    [Tooltip("t934u #19 wheel diameter (u, x Fireball area), trail width (u, x area), trail life (s, x Duration), knockback (u), per-enemy re-hit lockout (s), wheel hit = Fireball damage x this (NOT confirmed by VH)")] public float sin19WheelR934 = 1.0f, sin19TrailW934 = 0.6f, sin19TrailS934 = 3f, sin19Knock934 = 0.6f, sin19ReHit934 = 0.66f, sin19WheelMul934 = 1f;
    [Tooltip("t934u #19 FX: shader wheel (mode 11) + trail (mode 21). Off = flat ZSeg trail. Spin rad/s, wheel lift above feet (u), wheel squash")] public bool fxNezha934 = true; public float fxNezhaSpin934 = 14f, fxNezhaY934 = 0.35f, fxNezhaSquash934 = 0.55f;
    [Tooltip("t934u Soul Spears v2 sharp head (off = old round head): length px, half width px, widest at px")] public bool fxSpearV2_934 = true; public float fxSpearHL934 = 13f, fxSpearHW934 = 2.7f, fxSpearWideAt934 = 7f; // t986u: longer tip, shoulders closer to tip (VH: head too round)
    [Tooltip("t934u Blood Whip tentacle blood-red palette (VH approved batch 4; off = old magenta). NOTE: palette only, the CPU tentacle is kept - the mode 9 shader strip is not ported")] public bool fxBloodRed934 = true;
    [Tooltip("t934u shader quad pool cap")] public int fxPool934 = 96;
    [Tooltip("t934u/t1002u PixelShock ripple (GrabPass). OFF under URP automatically — GrabPass is Built-in only and errors in URP 2D. sorting order / squash")] public bool fxShock934 = true; public int fxShockOrder934 = -19980; public float fxShockSquash934 = 0.55f;
    [Tooltip("t934u Hellfire Pillar: GPU tell+crust (mode 10; off = CPU marks). Impact ripple radius 0.6->1.4 x r over 0.16 s, amp px, band px, crust life s")] public bool fxPillarGround934 = true; public float fxPillarShockR0_934 = 0.6f, fxPillarShockR1_934 = 1.4f, fxPillarShockS934 = 0.16f, fxPillarShockAmp934 = 3f, fxPillarShockW934 = 5f, fxPillarCrustS934 = 2.2f;
    [Tooltip("t934u Nine-Forked split node (mode 13): on, radius px, life s")] public bool fxNineFork934 = true; public float fxNodeRPx934 = 6f, fxNodeS934 = 0.3f;
    [Tooltip("t934u Tribulation v2: ripple R px, frames, amp px, band px, squash; scorch life s; chips")] public bool fxTrib934 = true; public float fxTribShockRPx934 = 48f, fxTribShockF934 = 9f, fxTribShockAmp934 = 3.5f, fxTribShockW934 = 6f, fxTribSquash934 = 0.55f, fxTribScorchS934 = 1.2f; public int fxTribChips934 = 10;
    [Tooltip("t934u Death Bell v2 (off = t930u shader rings): toll inward tremor amp/band px, gong frames/amp/band px, enemy jolt px, motes, chips")] public bool fxBell934 = true; public float fxBellTollAmp934 = 1.5f, fxBellTollW934 = 4f, fxBellGongF934 = 9f, fxBellGongAmp934 = 3.5f, fxBellGongW934 = 6f, fxBellJoltPx934 = 3f; public int fxBellMotes934 = 26, fxBellChips934 = 12;
    [Tooltip("t934u Golden Crow v2 sun (mode 15; off = old sprite): disc radius px at scale 1, pierce flash s, sorting order")] public bool fxSun934 = true; public float fxSunRPx934 = 5f, fxSunFlashS934 = 0.066f; public int fxSunOrder934 = 1990;
    [Tooltip("t934u Death Burst gore (mode 17; off = old CheapBurst): radius px, life s, pop frames")] public bool fxDeathBurst934 = true; public float fxGoreRPx934 = 14f, fxGoreS934 = 0.5f, fxGorePopF934 = 2f;
    [Tooltip("t934u Hit shards (mode 19, no ring): on, also on non-crit hits, max per 0.05 s, life s, sorting order")] public bool fxHitShards934 = true, fxHitShardsNormal934 = false; public int fxHitShardCap934 = 6; public float fxHitShardS934 = 0.2f; public int fxHitOrder934 = 32005;
    // ---- t948u (v0.0.80) ----
    [Tooltip("t948u Blood Whip FX in dark blood red (Endesga 0xe43b44/0xa22633/0x732e3a) instead of white/cream glints")] public bool fxWhipDark948 = true;
    [Tooltip("t957u Blood Whip colours, Endesga 0xRRGGBB: dark (outline/back, chips, puddle), mid (body, droplets, motes), highlight (underside, glint, hit flash). No white anywhere on the whip")] public int whipColDark957 = 0x3e2731, whipColMid957 = 0xa22633, whipColHi957 = 0xe43b44;
    [Tooltip("t965u Blood Whip value roles (Endesga): outline 181425, body 3e2731, underside band a22633, lash edge/sucker row e43b44, 1px travelling glint f77622, hit flash core feae34 + corners f77622, heal mote core feae34 + trail e43b44. Off = v0.0.81 flat 3 reds")] public bool whipRoles965 = true;
    public int whipOutline965 = 0x181425, whipBody965 = 0x3e2731, whipUnder965 = 0xa22633, whipEdge965 = 0xe43b44, whipGlint965 = 0xf77622, whipFlashCore965 = 0xfeae34, whipFlashRim965 = 0xf77622, whipMoteCore965 = 0xfeae34, whipMoteTrail965 = 0xe43b44;
    [Tooltip("t948u Bleed / Shock body tint: brief partial flash instead of a 3 s full pulse. Strength 0..1 and seconds")] public float bleedTint948 = 0.3f, bleedTintS948 = 0.1f, shockTint948 = 0.25f, shockTintS948 = 0.1f;
    [Tooltip("t948u Shock status crackle overlay seconds (was the full 3 s shock duration; the debuff itself still lasts 3 s)")] public float shockFxSec948 = 0.35f;
    [Tooltip("t948u Boss telegraph sorting order (above the floor -29000, below units ~ -1300..0): PatternWarnFX bullet warnings, TellFX stomp rings / horn cone, Ox-Head meteor puddle")] public int bossWarnOrder948 = -19985;
    [Tooltip("t948u Fire Wall replaces Hellfire Pillar (same slot, card, level-ups). Off = old Pillar code")] public bool fireWallOn948 = true;
    [Tooltip("t948u Fire Wall (design v0.4 sec 5b): length u, thickness u, distance ahead u, seconds, tick s")] public float fireWallLen737 = 5f, fireWallThick737 = 0.6f, fireWallDist737 = 3f, fireWallSec737 = 4f, fireWallTick737 = 0.5f;
    [Tooltip("t948u Fire Wall per level: +damage (replaces generic level step), +length u, +seconds")] public float fireWallDmgPerLvl737 = 0.12f, fireWallLenPerLvl737 = 0.2f /* t1012u A spirit: was 0.4 */, fireWallSecPerLvl737 = 0.2f /* t1012u A spirit: was 0.3 */;
    [Tooltip("t948u Fire Wall: levels that add a V segment, V angle deg, Echo wall gap u (behind), Hellfire Lance march speed u/s, max live walls")] public int[] fireWallVLevels737 = { 5, 8 }; public float fireWallVAngle737 = 30f, fireWallEchoGap737 = 1.5f, fireWallLanceSpeed737 = 2f; public int fireWallMaxLive737 = 6;
    [Tooltip("t948u Fire Wall damage per tick = skill dmg x this (Burn applies every tick via the innate status); knockback push u (Knockback support only)")] public float fireWallTickDmgMul948 = 0.35f, fireWallKnock948 = 0.6f;
    [Tooltip("t948u Fire Wall FX: shader flame band height px, tongue height mul, sorting order (ground, under units)")] public float fireWallFlameH948 = 22f, fireWallTongue948 = 0.889f; public int fireWallOrder948 = -19989; // t957u: 22 / 0.889 = approved fw.py mock-up (tongues 22..38 px)
    [Tooltip("t957u Fire Wall: use the approved PixelFireWall shader (off = t948u PixelAoE mode 8 band); scorch fill time s; extra quad pad below the band u; embers on, ember sorting order, embers per wall px")] public bool fireWallShader957 = true; public float fireWallScorchS957 = 0.1333f, fireWallPadDownU957 = 0.3f; public bool fireWallEmbers957 = true; public int fireWallEmberOrder957 = -19988; public float fireWallEmberPxPer957 = 4f;
    // ---- t969u/t970u (v0.0.83) ----
    [Header("t950/t970u boss shader FX (VH-approved wire pkg)")] public bool bossShaderTells950 = true;
    [Tooltip("t974u Stomp tell on PixelBossWarn (dissolve windup ellipse + wave bands on the real hitbox radius) instead of TellFX.StompRings (not Horse-Face, not Frost Ox)")] public bool bossStompShader984 = true;
    [Tooltip("t974u PatternWarnFX bullet-pattern warnings (radial / ring / fan / spiral / stomp burst) drawn as self-timed PixelBossWarn quads instead")] public bool bossPatternWarnShader984 = true;
    [Tooltip("t975u VH: boss warnings too bright/shiny. PixelBossWarn _Dim: sparser interior in the 2 darkest ramp slots, edge capped at red/blue (no yellow/white top). Off = v0.0.84 look")] public bool bossWarnDim985 = true;
        [Tooltip("t969u HurtFX #138 silver i-frame ring at the feet (drawn over the hero). Off = VH t969u 'not good'")] public bool iframeRing969 = false;
    [Tooltip("t970u I-frames shown by a shader on the hero sprite itself (Hellord/HeroIFrame). Off = plain blink fallback")] public bool iframeShader970 = true;
    [Tooltip("t970u I-frame shader mode: 0 = A soul shimmer + ember outline, 1 = B dither dissolve flicker, 2 = C scanline heat wobble (default: VH picked C at t972u)")] public int iframeShaderMode970 = 2;
    [Tooltip("t970u I-frame shader: stepped fps, strength 0..1; blink fallback period s and dim (0..1 brightness, alpha)")] public float iframeShaderFps970 = 12f, iframeShaderStrength970 = 1f, iframeBlinkPeriod969 = 0.07f, iframeBlinkDim969 = 0.35f, iframeBlinkAlpha969 = 0.6f;
    [Tooltip("t948u #40 Ring of the Burning City with Fire Wall: damage mul (-30%)")] public float sin40WallDmgMul948 = 0.7f;
    // ---- t986u / t988u / t989u ----
    [Header("t990u Frost Cleave VH-locked sample_v7 (frost_cleave_wire)")]
    [Tooltip("t990u: PixelFrostCleave smear from fx_audit/frost_cleave_wire. Off = FrostArcFX + CheapBurst")] public bool frostCleaveShader990 = true;
    [Tooltip("t990u: wind-up blade (5f). OFF — damage is instant at cast, so wind-up would lag the hit. Needs VH OK to enable")] public bool frostCleaveWindUp990 = false;
    [Tooltip("t990u: sorting order (front FX above units)")] public int frostCleaveOrder990 = 60;
    [Tooltip("t986u Gorilla Shard Fan originates at boss feet (not BBMuzzle +1.6 chest)")] public bool gFanAtFeet986 = true;
    [Tooltip("t986u wire PixelBossGround breath flame (mode 3) during Qilin Flame Breath active phase")] public bool bossBreathGround986 = true;
    [Tooltip("t988u FPS: max simultaneous PixelShock GrabPass quads (0 = uncapped). Named GrabPass is a full-screen copy whenever any shock is live")] public int fxShockMax988 = 3;
    [Tooltip("t988u FPS: Fire Wall embers per wall px (was 4; higher = fewer embers)")] public float fireWallEmberPxPer988 = 8f;
    [Tooltip("t988u/t1001u Fireball: skip pack Glow+soft Ring on splash (HellfireNova+sparks stay). Soft Glow was the purple/pink disc under CRT")] public bool fxFireSkipPackFlash988 = true;
    [Tooltip("t988u/t1001u Fireball: force ramp count 8 + centre-relative noise UVs; ramp is warm Endesga only (no ink/greyblue purple disc)")] public bool fxFireFix988 = true;
    [Tooltip("t1004u Fireball Impact hit-stop (0.03s). OFF default — many Fireballs stacked freezes and felt like lag. Shake stays.")] public bool fireballHitStop1004 = false;
    // ---- t1006u boss room: smaller arena + lights + trash adds ----
    [Tooltip("t1006u: clamp boss fight to a smaller arena inside the room (Diablo-style)")] public bool bossArenaClamp1006 = true;
    [Tooltip("t1006u: arena width/height in art px (camera ~480x270; ~2 screens = 960x540)")] public float bossArenaWPx1006 = 960f, bossArenaHPx1006 = 540f;
    [Tooltip("t1006u: also tighten tiled-floor bossRoomScale677 (Ox Pit baked art uses clamp only)")][Range(0.5f, 1f)] public float bossRoomScale1006 = 0.62f;
    [Tooltip("t1006u: place soft torch glows around the arena rim")] public bool bossArenaLights1006 = true;
    [Tooltip("t1006u legacy: unused when bossArenaRegular1046 (kept for old builds)")] public int bossArenaLightN1006 = 16; // legacy perimeter count
    [Tooltip("t1006u: torch glow radius px / strength for arena rim lights")] public int bossArenaLightRPx1006 = 100; public float bossArenaLightK1006 = 0.48f; // t1021u
    [Tooltip("t1021u/t1041u: fill glows — L/R first. Keep 2 for symmetric side fills")] public int bossArenaFillN1021 = 2;
    [Tooltip("t1021u/t1041u: fill glow radius px / strength")] public int bossArenaFillRPx1021 = 100; public float bossArenaFillK1021 = 0.28f;
    [Tooltip("t1046u: mirrored wall-row rim + suppress authored v26_boss scatter lights (skull glows, baked braziers)")] public bool bossArenaRegular1046 = true;
    [Tooltip("t1046u: evenly spaced lights on each long wall (N/S), mirrored")] public int bossArenaRimLong1046 = 4;
    [Tooltip("t1046u: evenly spaced lights on each short wall (E/W), mirrored")] public int bossArenaRimShort1046 = 3;
    [Tooltip("t1046u: corner margin as fraction of that wall length (keeps end lights off corners)")] public float bossArenaRimMargin1046 = 0.12f;
    [Tooltip("t1046u: inset from wall into floor (world units)")] public float bossArenaRimInset1046 = 0.55f;
    [Tooltip("t1006u: spawn theme trash during boss fight")] public bool bossAdds1006 = true;
    [Tooltip("t1006u: max alive non-boss trash in boss room")] public int bossAddsCap1006 = 12;
    [Tooltip("t1006u: seconds between add waves")] public float bossAddsEvery1006 = 3.5f;
    [Tooltip("t1006u: trash spawned at fight start")] public int bossAddsStart1006 = 6;
    [Tooltip("t1006u: trash per wave")] public int bossAddsWave1006 = 3;
    // ===================== t1393u ICE SKILLS (Frostbolt, Ice Nova, Frost Bomb, Hurricane) + shared chill / freeze =====================
    [Header("Ice skills (t1393u)")]
    [Tooltip("Damage multiplier, order Frostbolt, Ice Nova, Frost Bomb, Hurricane (same role as skillDmgMul)")] public float[] iceDmgMul = { 0.85f, 0.9f, 1f, 1f };
    [Tooltip("t1410u: CPU pixel-art ice VFX (HellordIceFX3). Off = the old chill/freeze tint + pack overlay and no ice effect sprites (visual only).")] public bool iceVfx1410 = true;
    [Tooltip("Shared CHILL -> FREEZE: build points a demon needs to freeze (each ice hit adds its skill's 'build'), seconds frozen, and extra build needed after a freeze before it can freeze again. Bosses freeze 40% as long")] public float iceFreezeAt = 3f, iceFreezeSeconds = 0.9f, iceFreezeGraceBuild = 2f;
    [Tooltip("Frostbolt by skill level: orbs per cast, orb speed (units/s), burst radius (units), chill-ground seconds, cooldown multiplier (applied on top of the 1.25s base)")] public int[] frostbOrbs = { 1, 1, 2, 2, 3 }; public float[] frostbSpeedU = { 5.5f, 5.5f, 6f, 6f, 6.5f }, frostbBurstU = { 1.1f, 1.2f, 1.3f, 1.4f, 1.6f }, frostbTrailSeconds = { 1.5f, 1.8f, 2f, 2.3f, 2.6f }, frostbCdMul = { 1f, 0.9f, 0.9f, 0.8f, 0.75f };
    [Tooltip("Frostbolt: target search range (units), fan angle between orbs (deg), orb hit radius (units), orb life (s), knock-back per hit, burst damage vs the orb hit (x)")] public float frostbRangeU = 10f, frostbSpreadDeg = 16f, frostbRadiusU = 0.55f, frostbLifeS = 2.4f, frostbKnock = 0.4f, frostbBurstMul = 0.6f;
    [Tooltip("Frostbolt chill: seconds + build per orb hit / burst; chill-ground: spacing of patches (units), patch radius (units), max patches alive, tick (s), chill seconds + build per tick")] public float frostbChillS = 2f, frostbBuild = 0.7f, frostbTrailGapU = 0.5f, frostbPatchU = 0.8f, frostbPatchTickS = 0.3f, frostbPatchChillS = 0.8f, frostbPatchBuild = 0.12f; public int frostbMaxPatches = 40;
    [Tooltip("Ice Nova by skill level: radius (units), pulses per cast, cooldown multiplier (on top of the 3.0s base)")] public float[] novaRadiusU = { 3.2f, 3.5f, 3.8f, 4.1f, 4.5f }, novaCdMul = { 1f, 0.95f, 0.9f, 0.85f, 0.8f }; public int[] novaPulses = { 1, 1, 1, 1, 2 };
    [Tooltip("Ice Nova: ring expand time (s), knock-back close to the hero / at the rim, chill seconds + build per hit, 2nd pulse damage (x) and gap (s)")] public float novaExpandS = 0.38f, novaKnockClose = 10f, novaKnockFar = 3f, novaChillS = 2.5f, novaBuild = 1.5f, novaPulse2Mul = 0.7f, novaPulseGapS = 0.45f;
    [Tooltip("Frost Bomb by skill level: blast radius (units), bombs per cast, seconds ticking, cooldown multiplier (on top of the 4.5s base)")] public float[] bombRadiusU = { 2f, 2.2f, 2.4f, 2.6f, 2.9f }, bombSeconds = { 2f, 2f, 2f, 2f, 2f }, bombCdMul = { 1f, 0.95f, 0.9f, 0.9f, 0.85f }; public int[] bombCount = { 1, 1, 1, 2, 2 };
    [Tooltip("Frost Bomb: how far from the hero a bomb may land (units), cluster samples, blast damage (x skill dmg), pulse damage (x), pulse radius (x blast radius), pulse interval (s), chill seconds + build per pulse, bonus vs chilled/frozen (x), knock-back, freeze seconds on detonation")] public float bombRangeU = 9f, bombBlastMul = 1f, bombPulseDmgMul = 0.08f, bombPulseRadiusMul = 1.1f, bombPulseS = 0.5f, bombChillS = 1.2f, bombPulseBuild = 0.8f, bombChilledBonus = 1.3f, bombKnock = 3f, bombFreezeS = 1f; public int bombSamples = 6;
    [Tooltip("Hurricane by skill level: radius (units), seconds, cooldown multiplier (on top of the 9.0s base)")] public float[] hurrRadiusU = { 2.4f, 2.6f, 2.8f, 3f, 3.4f }, hurrSeconds = { 4f, 4.5f, 5f, 5.5f, 6f }, hurrCdMul = { 1f, 1f, 0.92f, 0.85f, 0.8f };
    [Tooltip("Hurricane: start range (units), tick (s), chill seconds + build per tick")] public float hurrRangeU = 6f, hurrTickS = 0.4f, hurrChillS = 1.2f, hurrBuild = 0.35f;
}


}
