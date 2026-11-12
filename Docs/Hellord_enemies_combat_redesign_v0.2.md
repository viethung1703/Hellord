# Hellord: whole-roster combat redesign v0.2 (design only, not wired)

**Changes from v0.1** (based on VH's feedback [pt381u]):
1. The camera moves closer and rooms get smaller, so fights are denser.
2. Every floor gets a clear **melee kit AND range kit** with varied attack types: grenade, burst, a single fast shot, and AoE cones, rings and lines.
3. **Anti-kiting:** patterns are built so that constantly running in circles stops being a free win.
4. **Bullet-hell power fantasy, both ways:** enemies hit harder and more often, and the player's skills scale up just as hard. There is one shared "intensity" knob that raises both sides.

Each change is tagged:
- **[TUNE]**: numbers or tunables only.
- **[CODE]**: new behaviour code.
- **[ART]**: new animation rows or sprites for Hellord Art.

## 1. Camera and room scale

**What happens now** (`HellordGame.CamZoom`):
- The view targets ≤768×432 px. At 1080p the zoom is 3, which gives a **640×360 px view = 40×22.5 world units**. The hero is 2u tall.
- Rooms may have up to **3400 u² of walkable area** (`roomWalkAreaMaxU2`), which is bigger than the screen, so the camera scrolls.
- Demons fire from 6-8u away and their shots cross 10-20u of open floor. A moving hero simply walks out of the way.

**Proposal:**

| Knob | Now | v0.2 | Tag |
|---|---|---|---|
| Camera target view | ≤768×432 → 640×360 at 1080p | **≤512×288 → 480×270 at 1080p (zoom 4), 30×16.9u** | [TUNE] + small [CODE]: make the 768/432 divisor in CamZoom a tunable `camTargetPx` |
| Hero on screen | 2u of 22.5u height (9%) | 2u of 16.9u (12%) | — |
| Room walk area | 500-3400 u² | **300-1100 u²** (combat rooms ≈ 1-1.5 screens) | [TUNE] roomWalkAreaMinU2 / MaxU2 |
| Spawn density | 1.7 / 100u² | **2.6 / 100u²** (similar count, half the space → about 2× denser) | [TUNE] densityPer100Units |
| Boss room | 1536×864 px art (96×54u) | keep the art; camera clamps to a **48×27u arena** (walls from the room collider), or use a 1024×576 crop | [CODE] arena bounds; [ART] optional smaller boss rooms |

**Room shapes and hazards:**
- **v42 shapes (rect / L / plus / pillar):** they still work. At the smaller size there are at most **2 pillars** and plus-arms 6u wide.
- **Pits:** at most 2 per room. The ≥3u no-hazard rule around doors and spawns stays.
- **Mortar:** **2 mortars** per combat room (was 2-4) and 1-2 rice patches; with 4 pestles everything would be covered.
- **Burning:** at most 2 lava cracks, each about 60% of its current length.
- **Frozen:** 1-2 ice patches.
- **Survive:** keeps the bigger size (up to 1600 u²). It is the one "arena" room.
- **Pre-baked rooms (v26_* base art, 1024-1280px):** use the **640/768-wide variants only** in combat; the large ones go to Survive and boss.
  - Tag: [TUNE]/[CODE] in the picker.
  - The v26 640×368 and 768×432 variants already exist.

**Result:** a demon at the screen edge is ≤9u from the hero instead of ≤20u, so shot flight time roughly halves before anything else changes.

## 2. Anti-kiting rules (global)

The current problem: a projectile aimed at where the hero *is* misses a hero who keeps moving. Every range kit must use at least one of these:

1. **Lead aim:** aim where the hero will be (`hero pos + vel × t_flight × lead`), with lead 0.6-0.8. Partial lead stays dodgeable when you change direction. [CODE]
2. **Fast single shot:** 22-28 u/s (now about 8-12). A ≥0.6 s laser-line tell before it fires keeps it fair. [TUNE]+[CODE]
3. **Area over aim:** grenades and AoE land in a **disc that covers the hero's path**. A 2-2.5u disc placed with lead can't be outrun with a straight run. [CODE]
4. **Burst walls:** fans or rings of 8-24 bullets with **gaps you must aim for** instead of just running. Bullet speed is 7-10 u/s so they can be read. [CODE] (reuse the boss bullet layer `BossBullets` / P46)
5. **Lingering denial:** fire, ice, rice and gas zones (3-6 s) shrink the space you can circle in. [CODE] (reuse hazard decals)
6. **Melee closers:** every melee kit has a **gap-closer** (dash, leap, pull, or a short-range grab with a 1.5u reach). Melee no longer just walks after you. [CODE]
7. **Crossfire rule:** the spawn director puts ranged demons on **two different sides** of the hero, so circling one way runs you into the other. [CODE] in spawn placement

**Readability floor:**
- Every tell lasts ≥0.5 s. A fast shot always shows a laser line.
- Grenades show their landing disc for their full flight time (0.7-0.9 s).
- Bullet patterns use the floor's tell colour with a white core, and never the XP or hero colours.
- At most **60 enemy bullets on screen** (performance and readability).

## 3. Mutual power scaling: "Intensity"

**One knob, `combatIntensity` (1.0 now, 1.5 at v0.2 launch), scales both sides.** [TUNE]+[CODE]

| Side | What Intensity multiplies |
|---|---|
| Enemy | pattern frequency (cooldown ÷ I), bullets per burst (× I), pattern damage (× √I), elite spawn rate |
| Player | skill damage (× I), area/radius (× √I), projectile count (+1 at I≥1.5, +2 at I≥2), cooldown ÷ √I, pickup radius × √I |

**Depth scaling:** Intensity rises **+0.15 per floor** (FloorIdx), so floor 4 is about 2.0, which is the bullet-hell zone. Levels and traits are expected to keep the player on top.

**Player aggression direction** (skill tuning only, not a full redesign):
- **Base damage up about 40%, AoE radius up about 20%.** Kills come in bursts, so the screen clears and refills.
- **Fireball:** splash on by default; at level 3+ it forks into 3 on hit.
- **Frost Cleave:** 180° arc. The 3rd swing fires an ice wave that **erases enemy bullets** it passes.
- **Storm Ring:** 2 rings. Each ring **deletes bullets** it touches, so it's a defensive tool.
- **Bone Orbit:** +2 bones; bones block bullets.
- **Hellfire Pillar / Curse Rune / Grave Pool / Soul Spears:** +1 cast each, cooldowns −25%.
- **Bullet-cancel rule:** killing a ranged demon **pops its bullets in flight into XP sparks**. It's a big power-fantasy payoff for killing priority targets first. [CODE]
- **Dash:** 2 charges (was 1), i-frames 0.25 s. Dashing through a bullet grants **+10% damage for 2 s** ("graze" reward). [CODE]
- **Trait and support knobs** (all [TUNE]):
  - level-up pool weights shift toward +Projectiles, +Area and Explode on Kill
  - `supExplodeRadiusU` +25%

**Safety valve:** if the hero takes more than 40% of max HP within 5 s, enemy fire rate drops 30% for 4 s. It's invisible "mercy" that keeps the game crazy but not cheap. [CODE]

## 4. Melee and range kits per floor

Every floor has **2-3 melee and 2-3 range enemies**, plus fodder. Each range kit includes at least 2 of: grenade, burst, fast shot, AoE.

| Floor | Melee kit | Range kit | Fodder |
|---|---|---|---|
| Ox Pit (orange) | Brute (cleave + charge), Hell-cat (pounce), Ghoul (grab) | Archer (**fast shot**), Gossip Imp (**AoE cone**), Herder (**line**: calf stampede) | Imp, Splitter |
| Frozen (ice blue) | Frost Brute (**ice-dash**), Hound (pack leap), Skeleton (revive) | Frost Caster (**burst** fan + ice wall), Snowball Imp (**grenade**, lob), Ice Sniper Wraith (**fast** shard) | Imp, frost variant |
| Burning (lava red) | Fire Wheel (bounce roll), Hell-cat (lava pounce), Brute bomber (plants) | Ember Spitter (**grenade** that leaves lava), Flame Wraith (**ring burst** 16), Imp Gunner (**3-round burst**) | Imp, bomber variant |
| Mortar (purple) | Grinder Monk (pound march), Husk Runner (dash), Lid Bearer (shield + cone) | Rice Ghost (**line** + burst), Sack Lobber (**grenade**), Drum Acolyte (**ring** AoE), Bloater (fuse AoE) | Imp, Rice Mimic |

New names (Snowball Imp, Ice Sniper Wraith, Ember Spitter, Flame Wraith, Imp Gunner, Frost Brute, Sack Lobber) are **recolour/reuse variants** of existing sheets first, so they don't block on art. [CODE] + recolour; [ART] later.

## 5. Roster table (v0.2)

Bullet speeds are in u/s. HP tiers: T1 Imp, T2 Ghoul, T3 Brute, T4 Elite. Speed: S / M / F.

| Enemy | Floor(s) | Role | HP / Spd | Type | One-line pattern |
|---|---|---|---|---|---|
| Imp | all | fodder | T1 / F | melee | flank packs; dive with **0.4 s lunge** (gap close) |
| Imp Gunner | Burning, Ox | range | T1 / M | **burst** | 3-round burst, lead-aimed, 12 u/s, every 2.5 s |
| Ghoul | Ox, Frozen | melee | T2 / S | grab | **1.5u reach grab** (tell 0.5 s), slow + hold 1 s |
| Brute | Ox, Burning | melee | T3 / S | cone + dash | 3u **charge step** then 120° cleave; stuck 1 s |
| Frost Brute | Frozen | melee | T3 / S | dash | 6u ice-dash leaving a slick lane, then a slam ring r 2u |
| Hell-cat | Ox, Burning | melee | T1 / F | leap | pounce lane 5u, **lead-aimed**; back-hop |
| Hound | Frozen, Burning | melee | T2 / F | pack leap | 3 hounds leap in **staggered 0.3 s**, from 3 angles |
| Skeleton | Frozen | melee | T1 / M | revive | bone pile reassembles in 4 s unless finished |
| Elite Wraith | all | priority | T4 / M | blink + burst | blink behind, X-slash; between slashes an 8-bullet ring |
| Splitter | Ox | fodder | T2 / S | aimed split | children fan-lunge at the hero on death |
| Hook Thrower | Ox | range ctrl | T2 / S | fast line | **24 u/s** hook, laser tell 0.6 s, pulls toward hazards |
| Gossip Imp | Ox | range AoE | T1 / M | cone | 90° 5u cone, 1.0 s tell, Silence 1.5 s |
| Archer | Ox, Mortar | range | T1 / S | **fast shot** | 0.8 s laser then a **26 u/s** pierce arrow; with an Elite near, a 3-volley |
| Herder | Ox | range line | T2 / S | line AoE | calf stampede lane 1u, **through the hero's lead point** |
| Frost Caster | Frozen | range | T1 / M | **burst** + wall | ice wall, then a **7-bolt fan** (9 u/s) through the gap |
| Snowball Imp | Frozen | range | T1 / M | **grenade** | lobs a snowball: disc r 2u, 0.8 s flight, leaves an ice patch 3 s |
| Ice Sniper Wraith | Frozen | range | T2 / M | **fast shot** | 1.0 s laser, a 28 u/s shard, chill ×2 stacks |
| Fire Wheel | Burning | melee | T3 / F | lane | lead-aimed roll, 1 wall bounce, lava streak |
| Ember Spitter | Burning | range | T1 / S | **grenade** | 2 lobbed embers (r 1.8u), each leaves lava 3 s |
| Flame Wraith | Burning | range | T2 / M | **ring burst** | 16-bullet ring with **4 gaps**, 2nd ring rotated 11° 0.4 s later |
| Bomber variant | all | AoE | +0 | fuse | 3-count fuse (was 4), hits demons too |
| Frost variant | Frozen | modifier | +0 | trail | ice slick trail + death ice patch |
| Bloater (Fuse Sack) | Mortar | AoE | T2 / S | fuse | 4-beat fuse; a pestle detonates it |
| Rice Ghost | Mortar | range | T1 / M | line + **burst** | rice line, then a 6-grain fan on the accent beat; phases |
| Grinder Monk | Mortar | melee | T3 / S | pound | 3 beat pounds (cross arms) + ring with a safe gap |
| Husk Runner | Mortar | melee | T1 / F | dash | lead-aimed beat dash; trips on rice or rims |
| Rope Hauler | Mortar | range ctrl | T2 / S | fast line | **24 u/s** tether toward a mortar about to slam |
| Lid Bearer | Mortar | melee | T3 / S | shield cone | blocks the front; lid-slam cone on the accent |
| Sack Lobber (new) | Mortar | range | T1 / S | **grenade** | lobs rice sacks onto **the next pestle's target** or the hero's lead point |
| Drum Acolyte | Mortar | range AoE | T1 / S | ring | +15% BPM; dash-able beat ring every 2 bars |
| Rice Mimic | Mortar | ambush | T1 / M | bite | XP-bait pile, 1.5u snap |
| Calf | Ox (herd) | hazard | T1 / F | lane | stampede only |

## 6. Per-enemy pattern updates (v0.1 → v0.2)

Only the changes are listed. Unchanged parts are in v0.1 and the Mortar v0.1 doc.

- **Imp:** adds a 0.4 s lunge (2.5u lane tell 0.4 s) when within 3u, so melee lands on a moving hero. Packs come at 3 angles.
- **Imp Gunner (new, Imp recolour):** stops for 0.4 s and shows a 3-dot muzzle tell, then 3 bullets 0.12 s apart at 12 u/s with lead 0.7. It walks 1 s, then repeats.
- **Ghoul:** the grab now has **reach 1.5u** with a 0.5 s arms-up tell, so circling at close range still gets caught.
- **Brute:** before the cleave it takes a **3u charge step** (0.5 s lane tell). Cone 120° 3u, ×2.0, stuck 1 s.
- **Frost Brute (new, Brute recolour):**
  1. Ice-dash 6u (0.7 s lane tell); the lane becomes a 3 s ice slick.
  2. At the end, a slam ring r 2u.
- **Hell-cat:** the pounce lane is **lead-aimed** (0.6) and 5u long. Lava paw on Burning.
- **Hound:** packs of 3 leap from 3 angles, staggered 0.3 s (circling doesn't escape all three). Howl buff as in v0.1.
- **Elite Wraith:**
  1. Blink + X-slash as in v0.1.
  2. **Between slashes:** 8-bullet ring (8 u/s), then a 2nd ring rotated 22° after 0.3 s.
  3. Affixes as in v0.1.
- **Archer:** the laser tell drops to 0.8 s and arrow speed rises to **26 u/s**, so it hits anything still on the line. The laser **tracks for the first 0.5 s, then locks**.
- **Hook Thrower / Rope Hauler:** hook 24 u/s, laser tell 0.6 s, tracks then locks like the Archer. Pull 3u. A dash breaks it.
- **Gossip Imp:** cone tell 1.0 s (was 1.5), 5u, Silence 1.5 s.
- **Herder:** the stampede lane passes through the hero's **lead point**, not current position.
- **Frost Caster:**
  1. Wall as in v0.1.
  2. The bolt fan is **7 bolts at 9 u/s**, spread 50°, aimed at the wall gap and the hero.
- **Snowball Imp (new, Imp recolour):**
  1. A lob arc grenade; the landing disc tell (r 2u) is shown for the full 0.8 s flight.
  2. It lands at the lead point.
  3. Hits for ×1.0 + chill, and leaves an ice patch for 3 s.
- **Ice Sniper Wraith (new, wraith recolour):** 1.0 s laser (tracks 0.6 s), 28 u/s shard, ×1.5 + 2 chill stacks.
- **Fire Wheel:** lead-aimed roll, wall bounce, lava streak (as v0.1). Speed +20%.
- **Ember Spitter (new, Imp or Gossip recolour):** 2 lobbed embers in sequence, one at the current position and one at the lead point. Each leaves lava for 3 s.
- **Flame Wraith (new, wraith recolour):**
  1. A 16-bullet ring with 4 gaps (8 u/s).
  2. A 2nd ring 0.4 s later rotated 11°; the gaps move, so you must step into a gap.
- **Bomber variant:** 3-count fuse (2.25 s at 0.75 s/beat).
- **Rice Ghost:** rice line plus a **6-grain fan on the accent beat** (8 u/s).
- **Sack Lobber (new Mortar range):**
  1. Lobs 1 sack per bar onto **the spot the next pestle will slam**, or the hero's lead point (rice + ×1.0).
  2. This **teaches the beat**: where the sack lands, danger follows.

## 7. Bullet-hell density targets (Intensity 1.5)

| Room | Enemy bullets on screen (avg / peak) | Player kills/min target |
|---|---|---|
| Hunt / Fight | 12 / 30 | 90-140 |
| Elite | 20 / 45 | 110 |
| Survive | 25 / 60 (cap) | 160+ |
| Boss | per boss patterns, +25% bullets vs v0.0.46 | — |

Boss pattern bullets (`BossBullets` / P46) also get **+25% bullets and ÷1.2 cooldowns** under the same Intensity knob, so bosses and rooms rise together. [TUNE]

## 8. What is tuning vs code vs art

- **[TUNE] only:**
  - camera target (once it's a tunable), room walk area, density, hazard counts
  - projectile speeds, tell times
  - player skill damage/radius/count/cooldown, dash charges, trait weights
  - Intensity values and boss bullet multipliers
- **[CODE]:**
  - `camTargetPx` tunable, boss arena clamp
  - lead-aim helper, laser-tracking tell
  - lob/grenade projectile with landing disc
  - burst/ring emitters for regular demons (reuse BossBullets)
  - crossfire spawn placement
  - bullet cancel on kill, bullets erased by player skills, graze reward
  - mercy valve
  - the `combatIntensity` system
  - each enemy's state machine (Imp lunge, Ghoul reach-grab, Brute charge-cleave, Wraith blink + rings, etc.)
- **[ART]** (later, Frost Caster layout 38×32, 8×12, pivot 19,27):
  - attack rows for: Brute charge-cleave, Ghoul reach-grab, Wraith blink + cast, Hell-cat pounce, Archer aim/release, Herder whistle
  - new sheets: Skeleton (pile/reassemble), Hound (howl/leap), Sack Lobber
  - the Mortar set per the Mortar doc
  - bullet sprites: ice shard, ember, rice grain, sack, snowball (8×8 to 16×16)
  - the 7 recolour variants work first with the existing sheets + V37Recolor and get their own art only if VH likes them

## 9. Mortar set fit

The Mortar enemies stay as in the Mortar v0.1 doc. They are the purple melee/range kit, with these v0.2 changes:
- **Sack Lobber** added as the grenade unit.
- **Rice Ghost** gains a burst.
- **Hook speed** raised on the Rope Hauler.
- **Room size** is 2 mortars per room.

The beat still drives everything on Mortar; at 80 BPM the tells (1 beat = 0.75 s) already meet the readability floor.

## 10. Open questions for VH (max 5)
1. **Camera:** is 480×270 at 1080p (about 25% closer, zoom 4) the right amount, or do you want even closer (420×236)?
2. **Intensity 1.5 at launch, +0.15 per floor:** is that the right level of "crazy", or should floor 1 already start at bullet-hell density?
3. **Bullet cancel on kill + skills that erase bullets:** OK as the player's main power answer to bullet hell?
4. **Dash 2 charges + graze reward:** yes, or keep 1 dash?
5. **New recolour enemies first** (Imp Gunner, Snowball Imp, Ember Spitter, Flame Wraith, Ice Sniper, Frost Brute, Sack Lobber) using existing sprites, with art later? Or wait for real art before wiring?


## t679u — floor 2+ softening + ice mage 3-bolt volley (2026-11-02, UTC+7), compile only, NOT play-tested
VH: from floor 2 up enemy attacks are too much; the ice (Frost Caster) mage fires far too many bolts, 3 is enough (left edge, right edge, middle).
Cause: enemy Intensity jumped from 1.0 on floor 1 to 1.55 on floor 2 (1 + (1.5 + 0.15 x idx - 1) x 0.85). Bullet counts, fire rate and attack damage all scale with it, and floor damage growth is +15%/floor on top.
New HellordTuning tunables (soften679 = master switch): enemyIPerFloor679 0.12, enemyBulletMaxMul679 1.25, enemyCdDivMax679 1.3, enemyDmgGrowth679 0.08, densityPerFloor679 0.2, fcVolley679 3.
| Floor | enemy I | bullet mul | fire-rate x | attack dmg x | contact dmg growth | density /100u2 | ice mage bolts |
|---|---|---|---|---|---|---|---|
| 1 | 1.00 -> 1.00 | 0.90 -> 0.90 | 0.87 -> 0.87 | 0.85 -> 0.85 | 1.00 -> 1.00 | 1.4 -> 1.4 | 6 -> 3 |
| 2 | 1.55 -> 1.12 | 1.40 -> 1.01 | 1.35 -> 0.97 | 1.22 -> 0.97 | 1.15 -> 1.08 | 1.8 -> 1.8 | 10 -> 3 |
| 3 | 1.68 -> 1.24 | 1.51 -> 1.12 | 1.46 -> 1.08 | 1.43 -> 1.10 | 1.30 -> 1.16 | 2.1 -> 2.0 | 11 -> 3 |
| 4 | 1.81 -> 1.36 | 1.63 -> 1.22 | 1.57 -> 1.18 | 1.66 -> 1.23 | 1.45 -> 1.24 | 2.4 -> 2.2 | 11 -> 3 |
| 5 | 1.94 -> 1.48 | 1.74 -> 1.25 (cap) | 1.68 -> 1.29 | 1.89 -> 1.37 | 1.60 -> 1.32 | 2.7 -> 2.4 | 12 -> 3 |
The 3 bolts spread across fcFanDeg49 = 50 deg (-25, 0, +25). Player-side Intensity, HP growth, lava and boss scaling are unchanged. Compile: 0 errors.
