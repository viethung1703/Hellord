# Hellord: whole-roster enemy combat redesign v0.1 (design only, not wired)

**Why:** VH said most enemies have boring attack patterns. Today almost everything walks into you and deals contact damage. The only real exceptions are Hook Thrower, Gossip Imp, Frost Caster, Fire Wheel and the bomber variant.

**Goal:** every enemy forces one clear decision:
- **WHERE** to stand,
- **WHEN** to move, or
- **WHAT** to kill first.

## 0. Rules for every pattern
- **Tells:**
  - The tell sits on the ground and lasts at least 0.5 s (bp46MinTellS).
  - The tell colour is the floor's colour: Ox Pit orange, Frozen ice-blue, Burning lava-red, Mortar Horse-Face purple. Elites always use white-gold, so they read as "elite" on any floor.
  - Damage is given as a multiple of Imp contact (×1.0).
- **Tiers:**
  - **HP:** T1 = Imp, T2 = Ghoul, T3 = Brute, T4 = Elite.
  - **Speed:** S ≤0.7× Imp, M = Imp, F ≥1.3× Imp.
- **Contact damage:** it stays, but drops to ×0.5 on every non-fodder enemy. The real damage comes from the pattern, so getting hit means you misread a tell, not that you got body-blocked.
- **Beat:** only Pounding Mortar has a room beat. On other floors, patterns use fixed timings instead (1 "beat" = 0.75 s, so the same tuning still works).
- **Tools reused:**
  - shapes: lanes (Horse-Face spear / Ox charge), cones (Ox sweep), rings (Ox stomp quake), sky-drop discs
  - the bomber fuse ring, chill / freeze, lava cracks, ice patches, mortar pestles, rice slow, the hook tether (Hook Thrower)
  - the projectile system (Frost Caster bolts)

## 1. Inventory (what exists today)

| Enemy | Status in code / art | Today's behaviour |
|---|---|---|
| Imp | coded (EType.Imp, enemy_imp_v2) | walks in, contact |
| Ghoul (shambler) | coded (EType.Ghoul, enemy_shambler_v2) | walks in, contact |
| Brute | coded (EType.Brute) | slow walk-in, big contact |
| Elite (wraith) | coded (EType.Elite, enemy_wraith_v2), timed spawn | tougher walk-in |
| Splitter + Split Child | coded | walks in; splits into children on death |
| Hook Thrower | coded v0.0.24 (placeholder art, Level 1) | hook throw + pull |
| Gossip Imp | coded v0.0.25 (placeholder art, Level 1) | shout warning, then blast |
| Frost variant (v37) | coded variant 1 | recoloured base, chill on hit |
| Exploding / Bomber variant (v37) | coded variant 2 | fuse ring near the hero, explodes |
| Frost Caster | coded v0.0.44 (PixelLab sheet) | ice bolts, chill stacks |
| Fire Wheel | coded v0.0.44 | rolls at you |
| Archer, Hell-cat, Herder, Calf | sheets defined in HArt, **not spawned as enemies** (Calf / oxen exist as Ox-Head herd hazard) | — |
| Skeleton, Hound | art PNGs only, no code | — |
| Boss minions | none as enemies; bosses use hazards (oxen herd, icicles, meteors) | — |
| Mortar set v0.1 (8) | design only | see Pounding_Mortar_enemies_combat_v0.1.md |

## 2. Roster table (redesign)

| Enemy | Role | HP / Spd | Main threat | Counter | One-line pattern |
|---|---|---|---|---|---|
| Imp | fodder swarmer | T1 / F | flanking pack | AoE | packs of 3-5 pick **different sides** and dive in staggered 0.3 s apart |
| Ghoul | fodder blocker | T2 / S | grab and slow | kill or dash out | on contact it **grabs** (×0.5 slow, 1.5 s); 2+ grabs stack |
| Brute | heavy | T3 / S | overhead cleave cone | step to the side / behind | stops, raises its club (1 beat), 120° cone ×2.0; stuck 1 s after |
| Elite (Wraith) | priority duelist | T4 / M | blink + cross slash | bait the blink, then punish | blinks behind the hero, 0.6 s X-tell, cross slash ×2; elite affix (see §3) |
| Splitter | split puzzle | T2 / S | children come out **aimed** | kill near walls / in AoE | on death fires 3 children as a short fan toward the hero |
| Split Child | fodder | T1 / F | dash | AoE | one short lunge on spawn (0.4 s lane), then swarm |
| Hook Thrower | puller | T2 / S | hook to danger | dash breaks it / sidestep the lane | lane tell 1 s, the hook pulls you 3u toward hazards if any are nearby |
| Gossip Imp | area denier | T1 / M | delayed cone blast | leave the cone, kill first | "whisper" marks a 90° cone 1.5 s ahead, then the blast plus a 2 s Silence (no skills) |
| Frost variant | modifier | +0 | chill on hit | kill at range | leaves an **ice slick** trail (patch 2 s) |
| Bomber variant | modifier | +0 | fuse burst | kill at range or walk away | 4-count fuse ring, damages demons too |
| Frost Caster | ranged zoner | T1 / M | **ice wall + bolts** | flank through the gap | casts a 5u ice wall (blocks the hero), then 3 bolts through its gap |
| Fire Wheel | lane charger | T3 / F | bouncing roll | stand off its lane | locks a lane, rolls and **bounces off walls once**, leaving a lava streak |
| Archer (new spawn) | sniper | T1 / S | charged arrow | break line of sight / strafe | 1.2 s laser line, then a fast pierce arrow; volley ×3 when an Elite is near |
| Hell-cat (new spawn) | hit-and-run | T1 / F | pounce | step aside on the hop | circles at 4u, crouches 0.6 s, then a 4u pounce lane; retreats after |
| Herder (new spawn) | summoner | T2 / S | calls a Calf stampede | kill it first | every 6 s marks a lane at the room edge; 3 calves run it 1 s later |
| Calf (herd only) | hazard runner | T1 / F | stampede lane | leave the lane | runs straight and is gone at the wall; not a chaser |
| Skeleton (new) | revive pest | T1 / M | rises again | finish the bone pile | dies into a bone pile; reassembles in 4 s unless hit or burned |
| Hound (new) | pack leash | T2 / F | howl buff | kill the hound first | howls (ring tell): nearby demons get +30% speed for 3 s |
| Mortar set (8) | see §4 | | | | from Pounding_Mortar_enemies_combat_v0.1 |

## 3. Per-enemy patterns (steps + tell)

**Fodder (Imp, Split Child, Calf, Skeleton)** is meant to die fast in AoE. Fodder may keep full contact damage, but must have one behaviour twist.

**Imp: flank packs**
1. Imps spawn in packs of 3-5.
2. Each picks a side (N/E/S/W of the hero) and orbits to it.
3. They dive in one by one, 0.3 s apart.
- **Tell:** a quick red eye-flash before each dive.
- **Decision:** move so they can't surround you, and kill the first diver.

**Ghoul: grabber**
1. On contact it grabs: ×0.5 hero speed for 1.5 s plus a hand sprite.
2. Each extra grab adds −10% speed, down to a floor of ×0.3.
3. A dash breaks all grabs.
- **Tell:** arms raised when within 1u.
- **Decision:** don't wade through a Ghoul pack; use your dash on purpose.

**Brute: cleave**
1. It walks toward you. Within 3u it stops and raises its club for 1 beat.
2. Cone tell: 120°, 3u.
3. It swings for ×2.0 and is **stuck** 1 s after (takes ×1.3 damage).
- **Decision:** step behind it, then punish.

**Elite (Wraith): blink duelist + affix**
1. It blinks to 2u behind the hero (0.3 s puff tell at the destination).
2. An X-shaped slash tell (0.6 s), then the slash for ×2.0.
3. Cooldown 4 s. In between it hovers and fires 1 slow orb.
- **Affix** (one random per Elite):
  - **Frozen:** ice ring on death.
  - **Molten:** lava trail.
  - **Herald:** buffs nearby demons +20% damage, shown as a gold aura.
  - **Vampire:** heals from hits.
- **Decision:** priority target. Read the puff and turn.

**Splitter: aimed split**
1. It walks in.
2. On death, a 0.4 s tell shows 3 short lanes fanning toward the hero.
3. The children lunge down those lanes, then swarm normally.
- **Decision:** kill it at range or angled away from you.

**Split Child:** one lunge (above), then fodder chaser.

**Hook Thrower: hazard hook**
1. It keeps 5-7u away.
2. Lane tell: 1 s, dashed line to solid line.
3. The hook flies; on hit it pulls the hero 3u toward the Thrower, **or toward the nearest active hazard** (lava crack, ice patch, mortar about to slam) within 4u.
4. A dash breaks it.
- **Decision:** sidestep the lane, and keep away from hazards when it's alive.

**Gossip Imp: rumour cone**
1. It "whispers": a 90° cone tell, 4u, 1.5 s.
2. Blast for ×1.0 plus **Silence** for 2 s (skills paused, auto-attack only).
3. It reuses the existing shout warning and blast sounds.
- **Decision:** leave the cone or kill it first.

**Frost variant (any base):** keeps chill on hit, and also leaves **ice slick** footprints (2 s, slide). Its death spawns a small ice patch.

**Bomber variant (any base):**
1. Within 2.6u it plants and starts a 4-count fuse ring (the existing bomber fuse).
2. It bursts for ×2.0 and **also hurts demons**.
- **Decision:** kite it into packs.

**Frost Caster: ice wall + bolts**
1. It casts a 5u ice wall perpendicular to the hero line, 2u in front of the hero. Tell: 0.8 s blue line. The wall blocks the hero, not projectiles.
2. It then fires 3 bolts in a narrow fan through the wall's end gap (chill stacks).
3. The wall melts after 4 s, or 1 s faster per fire hit.
- **Decision:** go around the correct end, or burn the wall.

**Fire Wheel: bounce roll**
1. It revs up for 0.8 s; the lane tell shows the full path **including one wall bounce**.
2. It rolls fast, bounces once, and leaves a lava streak (3 s).
3. It is dizzy for 1.2 s after.
- **Decision:** read the bounce, and don't stand where the second lane goes.

**Archer: charged sniper (spawns on Ox Pit / Mortar)**
1. It keeps 8u away. A thin laser line locks after 1.2 s.
2. A fast pierce arrow for ×1.5. Pillars and walls block it.
3. If an Elite is within 6u, it fires 3 arrows with a 0.25 s gap.
- **Decision:** break line of sight or strafe off the line.

**Hell-cat: pounce (Burning / Ox Pit)**
1. It circles at 4u.
2. Crouches for 0.6 s with a short 4u lane tell.
3. Pounces for ×1.3 and leaves a small lava paw-print.
4. It hops back 3u, then repeats.
- **Decision:** sidestep, then hit it during the hop back.

**Herder: stampede caller (Ox Pit / Mortar)**
1. It keeps away.
2. Every 6 s it whistles: a room-edge lane tell 1u wide, 1 s.
3. 3 Calves stampede down the lane, ×1.0 each, and hit demons too.
- **Decision:** kill the Herder first, or use the stampede on packs.

**Calf:** a stampede projectile only. It runs straight, never chases, and is gone at the wall.

**Skeleton: revive pest (any floor, new)**
1. Normal walk-in fodder.
2. On death it becomes a bone pile and rebuilds in 4 s at full HP.
3. One hit, or any fire/ice status, on the pile destroys it for good.
- **Decision:** clean up the piles.

**Hound: howler (Frozen / Burning, new)**
1. It runs with packs.
2. Every 8 s it howls: a ring tell r 5u, 0.6 s. Demons inside get +30% speed for 3 s.
- **Decision:** it's a priority target; kill it before the pack arrives.

**Pounding Mortar set (from v0.1, unchanged):**

| Enemy | Pattern |
|---|---|
| Bloater (Fuse Sack) | 4-beat fuse, pestle-triggered |
| Rice Ghost (Grain Weaver) | rice lines + beat phase |
| Grinder Monk | 3-pound cross march + ring with a safe gap |
| Husk Runner | beat dash lane, trips on rice / rims |
| Rope Hauler | tether toward a slamming mortar |
| Lid Bearer | shield, back weak point |
| Rice Mimic | XP-bait ambush |
| Drum Acolyte | +15% BPM + beat rings |

## 4. Fodder vs priority targets

- **Fodder** (low HP, AoE food; pattern is a twist, not a test): Imp, Split Child, Ghoul, Skeleton, Calf, Husk Runner.
- **Pressure** (you dodge them while doing other things): Brute, Fire Wheel, Hell-cat, Grinder Monk, Lid Bearer, Bloater, Frost / Bomber variants.
- **Priority** (kill first, or the room gets much worse): Elite, Herder, Hound, Hook Thrower, Gossip Imp, Frost Caster, Archer, Rope Hauler, Drum Acolyte, Rice Ghost.
- **Room budget:**
  - At most 2 priority targets alive at once, or 3 in Elite rooms.
  - At most 1 hero-moving enemy (Hook Thrower or Rope Hauler) at a time.
  - At most 1 Silence source.

## 5. How the Mortar enemies fit the global roster

They are **floor variants of global roles**, the same way Frost Caster and Fire Wheel are the Imp/Brute rolls on Frozen and Burning:

| Mortar enemy | Global role |
|---|---|
| Husk Runner | Mortar Hell-cat / charger |
| Lid Bearer | Mortar Brute |
| Grinder Monk | Mortar heavy |
| Rope Hauler | Mortar Hook Thrower (Hook Thrower stays off Mortar) |
| Rice Ghost | Mortar ranged zoner (like Frost Caster) |
| Bloater | Mortar Bomber |
| Drum Acolyte | Mortar Hound-type buffer |
| Rice Mimic | Mortar-only special |

Floor rolls:

| Floor | Roles |
|---|---|
| Ox Pit | Imp/Ghoul/Brute/Splitter + Archer, Herder, Hell-cat, Hook Thrower, Gossip Imp |
| Frozen | Frost Caster, Hound, frost variant, Skeleton |
| Burning | Fire Wheel, Hell-cat, Hound, bomber variant, Skeleton |
| Mortar | the 8 Mortar enemies + Imp/Splitter/Archer fodder |

## 6. Art implications (for Hellord Art later, not sent)

All new or reworked sheets use the Frost Caster layout: 38×32 cells, 8×12, PPU 16, pivot (19,27). Rows: idle / walk / attack S/E/N, hurt S/E, death S. Larger bodies (Brute, Lid Bearer) may need a bigger cell; Hellord Art should confirm.

**Needs new or changed attack rows:**

| Enemy | Art needed |
|---|---|
| Brute | club raise + cleave swing row; "stuck" recover frames |
| Ghoul | grab/reach row; hand-grip overlay sprite (16×16) |
| Elite / Wraith | blink-out / blink-in frames; cross slash row |
| Imp | small crouch-dive row (can reuse walk frames + squash) |
| Frost Caster | wall-cast row (existing cast can stretch); ice wall sprite 16px segments |
| Fire Wheel | rev-up frames + dizzy frames (side-view sheet) |
| Archer | draw/aim hold + release row (sheet exists, check rows) |
| Hell-cat | crouch + pounce + hop-back rows |
| Herder | whistle row |
| Calf | fine as is |
| Skeleton | bone-pile collapse + reassemble rows (new sheet) |
| Hound | howl row (new sheet) |
| Hook Thrower / Gossip Imp | still placeholder art; need real sheets in the same layout |
| Mortar set | as listed in the Mortar v0.1 doc |

**FX** can reuse the v46 tells: lanes, cones, rings, X-slash, laser line, wall-bounce lane, ice-wall line, silence icon, grab hand, howl ring.

**No new art:** Split Child (behaviour only), Calf, the Bloater (v14 sheet works), and the frost/bomber variants (recolour + decals).

## 7. Open questions for VH
1. **Contact damage at ×0.5** for pattern enemies, so damage comes from tells. OK, or keep full contact?
2. **Silence (Gossip Imp)** and **hero pull (Hook Thrower / Rope Hauler)**: are these fun, or too punishing for a horde game?
3. **Elite affixes** (Frozen / Molten / Herald / Vampire): do you want random affixes now, or keep Elite simple?
4. **New spawns:** Archer, Hell-cat, Herder, Skeleton, Hound. Which do you want first? My pick is Brute cleave + Elite blink + Hell-cat + Herder.
5. **Order of work:** start with the base enemies (Imp/Ghoul/Brute/Elite/Splitter, used every run), or with Mortar?
