# Pounding Mortar: enemy combat redesign v0.1 (design only, not wired)

**Goal:** every Mortar enemy forces a different decision: where to stand, when to move, what to kill first. The room beat is 80 BPM, so one beat is 0.75 s and a bar is 4 beats (3.0 s); the 4th beat is the accent. Mortar pestles, rice slow (×0.75) and the beat are the floor's tools, and each enemy uses at least one of them.

**Shared rules**
- Every tell lasts at least 0.5 s (bp46MinTellS) and uses Horse-Face purple (the v46 tell colour) on the ground.
- "On beat" means the strike lands exactly on a beat, so the player can learn the rhythm.
- Damage is given as a multiple of a normal demon contact hit (×1.0 = Imp contact).
- Tiers:
  - **HP:** T1 = Imp, T2 = Ghoul, T3 = Brute, T4 = elite-only.
  - **Speed:** S = slow (≤0.7× Imp), M = Imp speed, F = fast (≥1.3×).

---

## 1. The three v14 enemies, made dangerous

### 1a. Bloater: "Fuse Sack" (keep the art, change the role)
- **Role:** a walking timed bomb that punishes standing still and turns kills into area denial.
- **HP / speed:** T2 HP, S speed.
- **Pattern:**
  1. It walks toward the hero. Within 3u it **plants** and starts a 4-beat fuse. A beat counter (4, 3, 2, 1) pulses on its belly and a purple ring r 2.5u grows each beat.
  2. On the accent beat it **bursts**: ×2.0 damage in r 2.5u, plus a sticky gruel patch (×0.6 slow, 4 s).
  3. Killed early, it bursts **instantly** at half radius (r 1.5u, ×1.0), so killing it in melee range still costs you.
  4. If a pestle slam hits it, it bursts at full radius **on that pestle's beat**. Players can lure it under a pestle as a chain trap that also hits nearby demons (it damages demons too, ×2.0).
- **Tell:** a growing ring, a 4-3-2-1 belly pulse and a hissing fuse sound on each beat.
- **Rice / mortar / beat:** the fuse counts room beats. Its gruel adds stronger slow. Pestles detonate it.
- **Spawn weight:** 14 (common).

### 1b. Rice Ghost: "Grain Weaver" (keep the art, replace the spray with lanes)
- **Role:** a ranged area-closer that makes **lines** of rice and turns the room into a maze.
- **HP / speed:** T1 HP, M speed. It floats over rice and pits.
- **Pattern:**
  1. It keeps 6-8u away and hovers.
  2. Every 2 bars it **draws a rice line**: a thin purple lane tell 8u long × 0.8u wide aimed through the hero, lasting 2 beats.
  3. On the next beat grains land along the lane: ×0.6 damage on hit plus a **rice strip** (×0.75 slow, 6 s).
  4. On the accent it **phases** for 1 beat. It is untargetable and blinks 3u sideways, so you have to time your hits between phases.
- **Tell:** the lane fills from end to end over 2 beats, and the ghost raises its arms (its cast row).
- **Rice / mortar / beat:** it creates rice terrain. Fire skills burn the strips. The phase is beat-locked.
- **Spawn weight:** 10 (it replaces about 18% of Imp/Ghoul rolls as before).

### 1c. Grinder Monk: "Pestle Monk" (keep the art, make the slam a moving threat)
- **Role:** a heavy that chases you with a **three-hit ground-pound walk** and safe gaps you have to read.
- **HP / speed:** T3 HP, S speed.
- **Pattern:**
  1. It walks. When within 4u it **marches 3 steps toward the hero, one pound per beat**.
  2. Each pound is a 1.5u disc at its feet plus a **cross of 4 short shock arms** (3u long, 0.6u wide). The diagonals are safe.
  3. The 3rd pound lands on the accent and makes a full ring r 3u, with a 1u safe gap facing away from the nearest mortar.
  4. It then stops, panting, for 1.5 s. Hits during this window do ×1.5 damage.
- **Tell:** the cross outline appears 1 beat before each pound, and the ring outline appears 2 beats before.
- **Rice / mortar / beat:** each pound spills a small rice puff (r 0.8u, 3 s). If its ring overlaps a mortar, that pestle slams early in sync, doubling the threat near pits.
- **Spawn weight:** 7 (it replaces about 12% of Brute/Ghoul rolls).

---

## 2. New Mortar enemy types

### 2a. Husk Runner: charge lanes
- **Role:** a fast lane-charger that punishes standing in a line with it.
- **HP / speed:** T1 HP, F speed.
- **Pattern:**
  1. It stops and paws the ground. A lane tell 10u × 1u, aimed at the hero, locks after 2 beats.
  2. On the beat it dashes the lane (×1.2 damage, knockback).
  3. If its lane crosses a **rice patch** it slows, skids, falls and is stunned for 1 s (a free kill).
  4. If it hits a **mortar rim** it stuns for 2 s and drops a rice puff.
- **Tell:** a straight purple lane that brightens on the beat before the dash.
- **Rice / mortar / beat:** rice and mortars are its counters, so the player can bait it into them.
- **Spawn weight:** 12, in packs of 2-3 that dash on alternate beats.

### 2b. Rope Hauler: tether and pull
- **Role:** a control unit that pulls you into danger.
- **HP / speed:** T2 HP, S speed. It stays at 5-7u.
- **Pattern:**
  1. It whirls a rope. After a 1-bar tell (a dashed line to the hero that turns solid), it **hooks** the hero.
  2. The tether lasts 2 beats and drags the hero 3u toward it, or **toward the nearest mortar if that mortar slams within the next 2 beats**.
  3. A dash breaks the tether. Damaging the Hauler by 15% of its HP also breaks it.
  4. Cooldown is 3 bars.
- **Tell:** a dashed line, then a solid line, plus a creaking rope sound on each beat.
- **Rice / mortar / beat:** it directly weaponises the pestles. Rice under the hero halves the pull distance (rice as an anchor).
- **Spawn weight:** 6. At most 1 per room, never in Start rooms.
- **Note:** this is the only enemy that moves the hero, so its tell must be the clearest one on the floor.

### 2c. Lid Bearer: shield and weak point
- **Role:** a front-shield tank that makes you reposition instead of standing and shooting.
- **HP / speed:** T3 HP, S speed.
- **Pattern:**
  1. It holds a big mortar lid facing the hero. Frontal projectiles are **blocked** (they deflect and spark).
  2. It turns slowly at 90°/s, so circling around it exposes its back, which takes ×2 damage.
  3. Every bar on the accent it **slams the lid down** in a frontal cone (90°, 3u, ×1.5). For 1 beat after, it is fully exposed.
  4. A pestle slam on it knocks the lid away for 3 s.
- **Tell:** the cone outline appears 1 beat before; the lid glints.
- **Rice / mortar / beat:** pestles break the shield. Rice slows your circling, which raises the risk.
- **Spawn weight:** 6. It pairs well with Rice Ghost or Archer behind it.

### 2d. Rice Mimic: bait lure
- **Role:** an ambush that punishes greedy pickup routing.
- **HP / speed:** T1 HP, M speed (only once awake).
- **Pattern:**
  1. It sits as an off-white **rice pile shaped like a fat XP pickup**. It is subtly different: a faint purple shimmer every 4 beats. It doesn't move.
  2. When the hero comes within 1.5u it **snaps**: a 1-beat bite (×1.5) in r 1.5u, then it turns into a fast chaser for 4 s before resettling.
  3. Killed, it drops **real** bonus XP (×2 of an Imp).
- **Tell:** a purple shimmer on the accent beat, plus a short mouth-open frame 0.5 s before the bite.
- **Rice / mortar / beat:** it lives on rice and uses the rice-vs-XP colour confusion as a feature (see Q4 in the design preview).
- **Spawn weight:** 5, at most 2 per room. Never in Survive rooms (they're too chaotic).

### 2e. Drum Acolyte: beat-caller and support (optional 5th)
- **Role:** a priority-target buffer that changes the beat itself.
- **HP / speed:** T1 HP, S speed. It keeps away.
- **Pattern:**
  1. It beats a small drum. While it is alive, the room BPM is **+15%**, which shortens every tell and pestle cycle.
  2. Every 2 bars, on the accent, it sends out a **beat ring**: an expanding ring 0.5u thick, ×0.8 damage, jumpable with a dash (i-frames).
  3. Killing it returns the room to normal BPM with a satisfying "record slow" sound sting.
- **Tell:** the ring starts as a bright pulse on its drum 1 beat before release.
- **Rice / mortar / beat:** it **is** the beat. Every pestle in the room speeds up while it lives.
- **Spawn weight:** 4, at most 1 per room. Elite and Survive rooms only (Survive already ramps the BPM, so it stacks).

---

## 3. Roster summary

| Enemy | Role | HP / speed | Main threat | Counter | Weight |
|---|---|---|---|---|---|
| Bloater (Fuse Sack) | timed bomb | T2 / S | 4-beat fuse burst | kill early at range / lure under a pestle | 14 |
| Rice Ghost (Grain Weaver) | lane maker | T1 / M | rice line + phase | burn rice, hit between phases | 10 |
| Grinder Monk (Pestle Monk) | pound march | T3 / S | cross + ring pounds | diagonals, punish the pant | 7 |
| Husk Runner | lane charger | T1 / F | beat dash | bait into rice or a mortar rim | 12 |
| Rope Hauler | tether | T2 / S | pull into a pestle | dash or burst damage to break | 6 |
| Lid Bearer | shield | T3 / S | lid cone | circle to the back, pestle | 6 |
| Rice Mimic | ambush | T1 / M | bite | read the shimmer | 5 |
| Drum Acolyte | buffer | T1 / S | +15% BPM, rings | kill first, dash rings | 4 |

Imp, Splitter and Archer stay as base fodder at their existing weights.

**Room caps:** at most 3 beat-locked attackers of the new types per wave. Hauler, Acolyte and Mimic have per-room limits as noted above.

---

## 4. Art ask list for Hellord Art

**Format for all sheets:** 38×32 cells, 8 columns × 12 rows, PPU 16, pivot (19,27), the same as Frost Caster and v14. Rows:
- 0-2: idle S/E/N
- 3-5: walk S/E/N
- 6-8: attack/cast S/E/N
- 9-10: hurt S/E
- 11: death S

W is a flip of E. Use the Endesga 32 palette.

**Keep v14 as the base, with these additions:**
- **Bloater:** keep the sheet. Add a **plant/fuse row**: a 4-frame swell with belly glow pulses, one per beat. Put it in the cast rows (6-8) and drop "spray". Add a burst death.
- **Rice Ghost:** keep the sheet. The cast row becomes arms-raised "weave", and the ghost needs a **phase-out** look (2-4 frames of fade/dither). The phase can reuse hurt_E if frames are tight.
- **Grinder Monk:** keep the sheet. The attack rows become a **3-step pound march**: a pestle raised overhead, then a slam, with a strong silhouette. Add a 2-4 frame **pant/exhausted** loop (it can take the hurt_E row if needed).

**New sheets:**
1. **Husk Runner:** a small wiry sinner covered in rice husks.
   - Attack rows: paw/charge (frames 0-3 paw, 4-7 dash).
   - Needs a trip-and-stunned pose for the death row or an extra frame.
   - Size: a small body in the 38×32 cell.
2. **Rope Hauler:** a hunched mill worker with a rope coil.
   - Attack rows: whirl and throw.
   - Separate sprite: a rope/hook projectile strip, 16×16, 4 frames.
3. **Lid Bearer:** a broad sinner holding a round stone mortar lid as a shield.
   - Attack rows: a lid slam down.
   - Needs a "lid knocked off" variant. Ideally 3 extra rows (idle/walk/hurt without the lid). If that's too much, use a separate 24×24 lid prop.
   - **May need a 48×40 cell.** Hellord Art should confirm the size.
4. **Rice Mimic:** a rice pile that opens into a mouth.
   - Rows 0-2: dormant pile (1 direction is enough, so use S for all three).
   - Rows 3-5: skitter.
   - Rows 6-8: bite.
   - Must look different from XP at a glance (a purple shimmer frame) but still clearly "rice".
   - Small: about 24×20 drawn inside the 38×32 cell.
5. **Drum Acolyte (optional):** a thin monk with a hand drum.
   - Attack rows: a drum strike with a ring-start pulse.
   - Idle loop timed to 8 frames per bar.

**FX asks (ground tells, can reuse the v46 tell set):**
- a 4-3-2-1 fuse ring
- a rice-line lane fill
- the cross-pound arms
- a dashed-to-solid tether line
- a lid cone
- a drum beat ring
- a gruel puddle

---

## 5. Open questions for VH
1. Should the **Rope Hauler pull** exist? It's very dangerous and the only hero-moving enemy. If not, I'd swap it for a "Sack Thrower" that lobs bags into safe gaps.
2. Is the **Rice Mimic** OK, using the rice-looks-like-XP problem on purpose, or do you want rice clearly different from XP and no mimic?
3. **Drum Acolyte** changing the BPM: fun or too chaotic? Should it be Elite/Survive only, or cut?
4. How many new types for the first pass: **all 5**, or 3 (suggested: Husk Runner, Lid Bearer, Rice Mimic) plus the 3 reworked v14 enemies?
5. Should the **Bloater burst hurt demons** (the lure-under-pestle chain trick), or only the hero?
