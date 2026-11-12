# Pounding Mortar (Level 12, Wasteful): design preview v0.1 (2026-10-29)
Status: preview for VH to approve or edit. No code, art or build yet. Every number is a proposal and becomes a HellordTuning tunable.

## 1. Floor, theme, palette, place in a run
- **Name:** POUNDING MORTAR (HUD: "MORTAR"). Banner subline: "PESTLES POUND TO THE BEAT. RICE SLOWS YOU. STEP OFF THE MARK."
- **Theme:** a hell rice-mill for people who wasted food. Stone mortar pits sunk in the floor, giant wooden pestles on rope beams, spilled rice, sacks and husk piles. Sinners pound rice forever.
- **Palette (Endesga 32):**
  - **Floor:** warm straw and tan stone.
  - **Mortars:** cool grey stone with dark rims.
  - **Pestles:** wood brown.
  - **Rice:** off-white, with a pale yellow highlight.
  - **Accent:** Horse-Face purple/magenta. It's already the v46 tell colour for Horse-Face, so beat rings and boss tells read as "this floor's danger".
  - **Why this palette:** it's light-floored, so it doesn't look like Ox Pit (brown/orange), Frozen (blue) or Burning (red). The catch is that off-white rice must not look like XP pickups (see Q4).
- **Place in a run:**
  - It's a 4th theme in the shuffled pool, never first (Ox Pit stays first).
  - Difficulty still comes from depth (FloorIdx) like the others. The floor's own identity is **rhythm**: Ox Pit teaches dodging lanes, Frozen and Burning teach where to stand, and Mortar teaches **when** to move.
- **Code groundwork (later):**
  - Theme arrays sized 3 → N.
  - Shuffle blocks of 3 → N.
  - Theme-1/else-2 switches (boss kind, banner, tiles, LowWalls.Level) → a lookup.

## 2. Room types and layout feel
- **Room types:** all 10 kept (Start, Survive, Nests, Hunt, Elite, Shrine, Curse, Treasure, Secret, Boss).
- **Fight rooms:**
  - **Mortar pits:** 2-4 per room. Each is a 3x3u round stone bowl. Its rim is solid for feet like the v42 pits; projectiles fly over.
  - **Pestle per mortar:** each mortar has one pestle that pounds on the room beat (see section 3).
  - **No-pit zone:** pits stay ≥3u from doors, spawn and room centre, the same rule as lava cracks.
  - **Room shapes:** the v42 shapes (rect / L / plus / pillar) still apply, with mortars placed in the open floor.
- **Rice on the floor:** loose rice patches, 2-4 per room, 1.5-2.5u blobs. Slam bursts add more patches that fade.
- **Props (decoration, not hazards):** sacks, husk piles, broken bowls, a rope beam overhead (top layer, no collision), chopsticks stuck in rice, a shrine bowl on the Shrine altar.
- **Start / Treasure / Shrine / Secret:** no pestles, just props, so non-fight rooms stay calm like on other floors.
- **Survive room twist:** the beat speeds up over 60 s (BPM ramp), which makes it the floor's signature room.
- **Boss room:** one huge central mortar the boss stands in or around (see section 5).

## 3. Hazards
### 3a. Pestle slam (main hazard)
- **Room beat:** one BPM per room, base 80 BPM (0.75 s per beat). A pestle slams every 4 beats, which is 3.0 s.
  - Pestles in a room are staggered across the 4 beats, so something is always moving.
  - The 4th beat is accented with a louder thud.
- **Tell:** uses the existing ground-decal tell plus stomp-quake ring style.
  1. **Beat −2 (1.5 s before):** a dim purple ring fills the mortar mouth plus a 1u margin. The pestle starts to rise.
  2. **Beat −1 (0.75 s before):** the ring turns bright and pulses with the beat. The pestle is at the top.
  3. **Slam:** impact flash, damage disc, and a short quake ring that spreads.
  - Tells never drop under bp46MinTellS (0.5 s).
- **Damage:**
  - **Impact disc:** radius 1.8u, 14 damage, normal i-frames, the same 20% max-HP cap as the tongue trap.
  - **Quake ring:** 1 ring, speed 4u/s, alive 0.6 s, 6 damage, with a 60° gap facing away from the nearest door. It's a graze punish, and you can jump it like Ox Stomp Quake.
  - **Demons:** they take 40 damage if under the pestle (an optional lure tactic, like the tongue trap's "hits enemies optional").
- **Rice burst:** each slam throws 1 fresh rice patch (r 1.2u, lasts 6 s) 2-3u from the mortar. The patch lands on a beat.

### 3b. Rice (slow)
- **Slow:** standing on rice gives ×0.75 move speed. It lasts while you stand on it plus 0.3 s after.
  - It doesn't stack with chill; the stronger slow wins.
  - Rice is ground only and doesn't affect dodge i-frames.
- **Bloater interaction:** a Bloater's death spill can wet the rice, making a sticky patch at ×0.6 for 3 s (optional, Q3).
- **Fire skills:** Fireball, Hellfire Pillar and lava burn rice patches away in 0.5 s, which is the player's counterplay.

### 3c. Optional second hazard (Q2): grinding stone
- One rolling millstone per Hunt or Elite room.
- It moves on a straight rail between two walls, crossing the room once every 8 beats.
- Damage: 12. It pushes demons.
- It's skipped if Q2 says "pestles only".

## 4. Enemy roster
| Enemy | New / reuse | Role on this floor |
|---|---|---|
| Imp | reuse | swarm filler |
| Splitter | reuse | punishes AoE in tight lanes between mortars |
| Archer (spitter stand-in) | reuse sheet `enemy_archer` | ranged pressure while you wait for the beat; shots fly over mortars |
| **Bloater** | **new common** (in the design's 12-enemy roster, not made yet) | slow tank; on death it bursts into a sticky rice/gruel puddle (×0.6 slow, 3 s) and 6 damage in 1.5u |
| **Rice Ghost** | **new unique** (~18% of Imp/Ghoul spawns, like the Frost Caster roll) | floats over rice and pits, phases through walls of mortars. Every 4 beats it spits a 3-grain rice fan (slow-on-hit 1 s). Low HP |
| **Grinder Monk** | **new unique** (~12% of Brute/Ghoul spawns, like the Fire Wheel roll) | heavy melee carrying a mini-pestle. On the accented beat it slams a 1.5u disc (0.6 s tell); between slams it walks. Elite rooms: 2 guards are Monks |
- Ghoul and Brute stay as base rolls. The Rice Ghost and Grinder Monk are their floor variants, replacing frost or exploding variants.
- Hook Throwers and Gossip Imps stay off (they belong to Level 1).

## 5. Boss: Horse-Face (Pounding Mortar)
- **Arena:**
  - Boss room size as for Ox-Head.
  - A ring of 4 corner mortars with pestles on the room beat, at 90 BPM in phase 1 and 100 BPM in phase 2.
  - A central open floor with no rice at the start; rice comes from the boss's moves.
- **Base:** the existing Horse-Face: horseTempo 1.25, spear thrust lane (11u, half width 0.75u, tell 0.6 s, speed 36), spear combo ×3 (later tells 0.38 s), charge (tell 0.6 s, wall stun 2 s). HP uses the Ox-Head growth × 1.0.
- **The boss gets a pestle** (a long wooden pestle instead of the spear). The spear-thrust code is reused as the **Pestle Thrust**, the same lane hitbox with the art swapped.
- **Patterns:**
  1. **Horn Toss** (v46 id 1, reuse): bone chunks that split, 3 chunks in P1 and 5 in P2.
  2. **Lasso** (v46 id 11, reuse): bead ring closes around the hero (9 beads/arm, r 4.5u, 70°/s, gap rule kept).
  3. **Pestle Thrust** (reuse spear thrust + combo), renamed.
  4. **NEW Pestle Combo:**
     - 3 overhead slams ON THE BEAT (0.6 s apart at 100 BPM).
     - Each slam is a 2.2u disc at a locked point: first on the hero, the 2nd and 3rd lead your move direction by 1.5u.
     - Each disc shows its own beat ring tell (≥0.5 s). Damage ×1.2 contact.
     - Each slam leaves a rice patch.
  5. **NEW Spin:**
     - 1.2 s wind-up (purple circle tell r 3u).
     - Then a 2 s spin that moves toward the hero at 2.5u/s with a 3u damage disc, flinging 12 rice grains outward (slow-on-hit).
     - It ends dizzy for 1.0 s (a free hit window).
  6. **NEW Rice Scatter:**
     - Throws 5 rice sacks in a fan (sky-drop kind 2 reused, burst on landing), each tell r 1.5u, 0.8 s.
     - Each makes a rice patch for 8 s.
     - It sets up the next Pestle Combo.
- **Phases:**
  - **P1 (100-60% HP):** cycle of Pestle Thrust, Horn Toss, Pestle Combo, Rice Scatter.
  - **P2 (60-25% HP):** adds Spin and Lasso. The corner pestles go to 100 BPM. Horn Toss uses 5 chunks.
  - **P3 Frenzy Pound (≤25% HP, once, then repeats every ~15 s):**
    - The boss leaps into the centre mortar.
    - The room beat drops to a clear 72 BPM with a loud metronome.
    - The boss then slams 3 times on the beat. Each slam targets where the hero stands 1 beat earlier.
  - **Stun rule:**
    - If the hero takes **0 slam hits across all 3 slams**, the boss's pestle sticks in the mortar: **3.0 s stun, ×2 damage taken**, the same idea as the Ox-Head wall stun.
    - If any slam hits, there's no stun, and it repeats after the normal cooldown.
    - Corner pestles pause during Frenzy Pound, so only the boss's slams count.
- **Rage:** the v46 rage tempo of 0.85 applies to its tells and cooldowns. It never drops below 0.5 s.
- **Reward:** the standard boss reward pick.

## 6. Proposed tunables (all go in HellordTuning, `// v0.0.4x`)
| Tunable | Proposal |
|---|---|
| mortarBpm / bossBpmP1 / P2 / frenzyBpm | 80 / 90 / 100 / 72 |
| survive BPM ramp | 80 → 110 over 60 s |
| pestleEveryBeats | 4 |
| mortarsPerRoom | 2-4 (by room size) |
| mortarRadiusU | 1.5 |
| pit clearance from doors/spawn/centre | 3u |
| tell beats | 2 (dim at −2, bright at −1) |
| tell floor | bp46MinTellS 0.5 s |
| slam disc r / dmg | 1.8u / 14 |
| quake ring speed / life / dmg / gap | 4u/s / 0.6 s / 6 / 60° |
| slam dmg to demons | 40 |
| HP cap per hit | 20% |
| rice patches per room | 2-4 |
| rice patch radius | 1.5-2.5u |
| rice from a slam | r 1.2u, lasts 6 s |
| rice slow / linger | ×0.75 / 0.3 s |
| sticky puddle | ×0.6, 3 s |
| fire clears rice | 0.5 s |
| millstone | every 8 beats, 12 dmg |
| Rice Ghost / Grinder Monk chance | 18% / 12% |
| Monk slam r / tell | 1.5u / 0.6 s |
| Ghost fan | 3 grains, 1 s slow |
| Bloater burst r / dmg | 1.5u / 6 |
| Pestle Combo slams / disc r / lead / dmg | 3 / 2.2u / 1.5u / ×1.2 |
| Spin wind-up / time / speed / r / grains / dizzy | 1.2 s / 2 s / 2.5u/s / 3u / 12 / 1.0 s |
| Rice Scatter sacks / tell r / tell s / patch life | 5 / 1.5u / 0.8 s / 8 s |
| phase thresholds | 60% / 25% |
| Frenzy repeat | 15 s |
| Frenzy stun / damage-taken mul | 3.0 s / ×2 |
| horseTempo | 1.25 (unchanged) |

## 7. Art and effects asks (to send to Hellord Art / Hellord FX; not sent yet)
**Hellord Art (sprites):**
1. **Horse-Face boss sheet:** the real sheet replacing the recolour. Same row layout as Ox-Head plus: pestle-overhead slam, spin loop, dizzy, leap-in, stuck-pestle stun, and the rice-sack throw. Holds a wooden pestle.
2. **Mortar room tiles:** base tiles plus LowWalls set "mortar" (NE/NW/SE/SW, wall E/S/W, door and boss door N/E/S/W, glow), matching the frozen and burning sets.
3. **Mortar + pestle prop:** a 3x3u stone bowl (top-down, rim reads as solid), plus a pestle with rise/top/slam frames (about 6 frames) and an overhead rope beam.
4. **Rice patch decals:** 3 sizes, plus 3 fade frames.
5. **Decoration props:** sacks, husk pile, broken bowl, chopsticks.
6. **Enemies:** Rice Ghost and Grinder Monk sheets (same row layout as Frost Caster: idle/walk/cast S/E/N, hurt, death). Bloater sheet (common enemy).
7. **Room icon** for the floor banner / minimap if needed.

**Hellord FX (effects):**
1. **Beat ring tell:** dim fill → bright pulse synced to the beat, in Horse-Face purple, scalable from 1.5 to 3u.
2. **Slam impact:** dust, cracks and rice spray, plus a small screen shake hook.
3. **Quake ring** for this floor (a recolour of the stomp quake is fine).
4. **Rice burst:** grains flying and settling into a patch.
5. **Spin:** wind circle and a grain trail.
6. **Frenzy Pound:** metronome flash and a "stuck pestle" stun sparkle.
7. **Sticky gruel puddle** (Bloater).
8. **Fire-burns-rice** puff.

**Audio (if available):** a beat thud with an accent on beat 4, and a metronome tick for Frenzy Pound.

## 8. Open questions for VH
1. **Beat source:** A) fixed per-room BPM in code (simplest), or B) synced to the floor's music track (needs a music loop at a known BPM).
2. **Second hazard:** A) pestles + rice only, or B) also the rolling millstone in Hunt and Elite rooms.
3. **Bloater:** A) a full new common enemy used on later floors too, or B) a Mortar-only "Gruel Bloater" variant to save art.
4. **Rice vs XP pickups:** rice is off-white and blood drops are red, but HA's XP gems/drops may change. A) rice off-white, or B) rice tinted pale yellow-green to stay well clear of pickups.
5. **Frenzy Pound rule:** A) any slam hit cancels the stun (strict, as above), or B) 2 of 3 dodged still stuns, for a shorter 2 s.
