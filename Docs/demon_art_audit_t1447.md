# Demon / enemy art audit (t1447)

> **t1454 / t1457 update:** PixelLab batches 1-3 (+ the batch 4 Ox-Head re-roll) now give hit / death / permanent corpse to Shambler, Wraith, Splitter, Split Child, Hook Thrower, Gossip Imp, Frost Caster, Fire Wheel, Bloater, Rice Ghost, Grinder Monk, Frost Brute, Archer, every frost / bomber / range-kit recolour, and the four bosses. Death44 / Death53 and the boss deaths now spawn corpses. See `d2feel_assets.md` (t1454 / t1457). Still open: attack / idle poses (batch 4 not integrated), code-drawn Hook Thrower / Gossip Imp sheets, unused sheets.


Source of truth: code in `Assets/Hellord/Scripts/Runtime` (HArt.cs `EnemyArtMap` / `Sheets`, HellordGame.cs, HellordV37/V44/V49.cs, HellordTorture53.cs, HellordMortarArt49e.cs, HellordBoss*.cs, HellordD2Feel1434.cs), the sheets in `Resources/HellordArt`, and `Art/EXPORT_MANIFEST_2026-10-26.md`. Written for Hellord Art. Nothing here was changed in-game by this audit.

## Legend

- **old** = the original export sheet (`enemy_*` 4-direction walk x 4 frames at 12 fps, S/E/N/W at frames 0/4/8/12, plus one 8-frame death strip at frames 16-23 that has **no facing**). No idle, attack or hit frame. The hit flash is a material swap.
- **d2feel (HA)** = t1434/t1435 art from Hellord Art, `Resources/HellordArt/Enemies/<type>_{stagger,death,corpse}_<S|E|N|W>` plus `BloodFX/` pools and FX strips. 4 directions.
- **d2anim (PL)** = t1446 PixelLab hit / death / corpse (`*_pl`), toggle `d2anim1446`. Only **imp_v2** and **brute**, 4 directions.
- **own sheet** = a bespoke sheet with its own rows (Frost Caster, Fire Wheel, Mortar demons, bosses).
- Recolour = code tint of an existing sheet (`V37Recolor`), no new art.
- "Fallback corpse" = what the d2feel layer uses when a type has no own art: the base ETypes art, regardless of variant.

## 1. Gameplay ETypes (EnemyArtMap)

| Type | Sheet | Walk | Idle | Attack | Hit | Death | Corpse | Source in use |
|---|---|---|---|---|---|---|---|---|
| **Imp** | enemy_imp_v2 (31x38) | 4 dir x 4 | none (walk frame 0 when still) | none (contact damage; lunge = walk frames + code telegraph) | **PL hit** 4 dir (+HA stagger fallback) | **PL death** 4 dir with POP frame; HA death 4 dir behind it; old strip | PL corpse 4 dir + pool + decal | **d2anim (PL) + d2feel (HA) + old walk** |
| **Ghoul** (shambler_v2, 22x39) | | 4 x 4 | none | none (grab = code ring telegraph) | HA stagger 4 dir | HA death 4 dir | HA corpse 4 dir + pool + decal | **d2feel (HA) + old walk**. No PL art. |
| **Brute** (enemy_brute 47x50) | | 4 x 4 | none | none (step + cleave = code wedge telegraph) | **PL hit** 4 dir | **PL death** 4 dir; HA death | PL corpse 4 dir + pool + decal | **d2anim (PL) + d2feel (HA) + old walk** |
| **Elite** (wraith_v2 24x41) | | 4 x 4 | none (floats) | none (blink + X-slash + ring = code FX) | HA stagger 4 dir | HA death 4 dir | HA corpse 4 dir, **no pool / no decal** | **d2feel (HA) + old walk** |
| **Splitter** (60x40) | | 4 x 4 | none | none | HA stagger 4 dir | HA death 4 dir (children spawn at frame 4) | one **non-directional** corpse png (`splitter_corpse`) + pool + decal | **d2feel (HA) + old walk** |
| **SplitChild** (27x25) | | 4 x 4 | none | none (lunge as Imp) | HA stagger 4 dir | HA death 4 dir | one non-directional corpse png | **d2feel (HA) + old walk** |
| **HookThrower** | **PLACEHOLDER**, code-drawn (`HookThrowerArt.Placeholder`, 24x34) | 4 x 4 | none | none (wind-up whirl + chain FX only; no throw pose) | HA stagger art exists but **code skips stagger** for this type | HA death 4 dir (placeholder body in the death sheet frames 16-23) | HA corpse 4 dir + pool | **placeholder + d2feel (HA)**. Needs a real sheet. |
| **GossipImp** | **PLACEHOLDER**, code-drawn (`GossipImpArt.Placeholder`, 24x28) | 4 x 4 | none | none (shout = ring FX only; no shout pose) | HA stagger art exists but **code skips stagger** | HA death 4 dir | HA corpse 4 dir + pool | **placeholder + d2feel (HA)**. Needs a real sheet. |
| **Nest** (enemy flag, `_nestF` 3 frames) | code / floor art, 2 states + dead | n/a | idle + hit flash | spawner (no attack) | hit frame | `nest_remains` floor sprite | the remains sprite (persistent already) | old |

## 2. Variants and themed demons

These all reuse a base ETypes and mostly **bypass the d2feel death, so they leave no corpse** (see section 4).

| Variant | How it is built | Walk | Idle | Attack | Hit | Death | Corpse / pool | Source in use |
|---|---|---|---|---|---|---|---|---|
| **Frost demon** (`variant 1`, floor theme 1) | recolour of the base type | base walk | none | contact + frost slow | base | base death (recolour not applied to d2feel art, so the HA corpse comes out in the **normal colours**) | base type's HA corpse | old / recolour + d2feel (wrong colour) |
| **Bloater / exploding demon** (`variant 2`, Burning floor, Imp or Ghoul recolour; Mortar `m53` kind Bloater = `enemy_bloater`) | `BomberThink37` fuse + explosion | base walk (variant 2) / **enemy_bloater** 8 cols x 12 rows (m53) | idle S/E/N 8f (m53 only) | cast S/E/N 8f in sheet (**not used by code**: fuse is a tint pulse) | hurt S/E 6f (m53) | m53: death row S 8f, only S | none for m53 (body vanishes after the anim). Plain variant 2: HA corpse of the base type, in normal colours | m53 = **own sheet** (v14 PixelLab). variant 2 = recolour. |
| **Frost Caster** (`variant 3`; ranged frost bolt) | `enemy_frost_caster` 38x32 x 8 x 12 | rows 3-5 S/E/N (W = E flipped) | rows 0-2 S/E/N | cast rows 6-8 S/E/N, used | hurt rows 9-10 S/E (N uses S) | row 11, **S only**, ice shatter, last frame held | **none** (sprite is hidden after the anim) | **own sheet** (PixelLab v14). No d2feel stagger / death / corpse / pool. |
| **Fire Wheel** (`variant 4`; rolling charge) | `enemy_fire_wheel` 40x40 x 8 x 5 | row 0 roll (E, W = flip; no N / S) | frame 0 of the roll row | spin wind-up row 1, roll row 0, dizzy row 2 | hurt row 3 | row 4, E only | **none** | **own sheet** (side view only). No d2feel. |
| **Rice Ghost** (Mortar `m53` kind 2, Imp) | `enemy_rice_ghost` (same 38x32 layout) | S/E/N | S/E/N | cast rows exist (unused) | hurt S/E | death row S | **none** | **own sheet** (PixelLab v14); no d2feel |
| **Grinder Monk** (Mortar `m53` kind 3, Brute) | `enemy_grinder_monk` (same layout) | S/E/N | S/E/N | cast rows exist (unused) | hurt S/E | death S, 7f | **none** | **own sheet**; no d2feel |
| **Mortar m53 (general)** | `ApplyMortar53`: Ghoul = Bloater, Imp = Rice Ghost, Brute = Grinder Monk, Mortar theme only | see rows above | | contact damage only | | `Death53` | none | own sheets |
| **Imp range kits** (`k49` 1-3, 7; Floor 1+) | Imp recoloured (fire / frost / bone) | Imp walk | none | gun burst / lob / ember: **code projectiles, no cast pose** | Imp stagger (HA) | PL death if the Imp art applies (**tinted wrongly**: PL art is the red imp) | Imp PL corpse (red) | recolour only |
| **Elite range kits** (`k49` 4-5) | Elite recoloured (fire / frost) | Elite walk | none | laser / FWR: code lines | HA stagger | HA death | HA corpse (white wraith) | recolour only |
| **Frost Brute** (`k49` 6, Frost floor) | Brute recoloured frost | Brute walk | none | dash + slam: code zones | PL hit | PL death + PL corpse (**grey brute colours**) | PL | recolour of PL |
| **Archer** (`k49` 8) | `enemy_archer` 38x38 x 24 (old export) | 4 x 4 | none | laser tell only (no shoot frames) | none | the d2feel layer looks up the base type (Imp), so it dies and leaves the **Imp** corpse, not an archer one | Imp corpse (wrong body) | **old** walk only |

## 3. Art present in `Resources/HellordArt` but not used by an enemy

| Sheet | Contents | Status |
|---|---|---|
| `enemy_hellcat` (46x30, 24f) | walk 4 dir, death | **loaded nowhere**; no gameplay |
| `enemy_calf` (34x34, 54f) | walk 4 dir, windup E/W 7f, charge E/W 4f, stun E/W 4f, death 8f | **loaded nowhere** (Calf B, skull mask) |
| `enemy_herder` (38x35, 41f) | walk 4 dir, whip E/W/S, death | **loaded nowhere** (haste-buff support that never attacks) |
| `enemy_hound` (520x21), `enemy_skeleton` (360x24) | sheets only, **not in `HArt.Sheets`** | not loadable, not used |
| `enemy_knight`, `enemy_imp`, `enemy_shambler`, `enemy_wraith` (v1) | in the Art export only, not in the repo | superseded by `_v2` |
| `Summon561/skel_warrior_*` / `skel_archer_*` | idle / walk / attack / death (warrior), idle / walk / shoot (archer), S/E/N | **hero's summons** (not enemies), own sheets, no hit / corpse |
| `Torture/damned_rice_16x24_8f` | Damned soul prop (idle / scream / flail) | environment prop in Mortar rooms, not an enemy |
| `hazard_ox_run_*` | ox stampede hazard, 6f, E/N/S | hazard, no death / hit |

## 4. Bosses

| Boss | Sheet | Idle / walk | Attacks | Hit | Death | Corpse | Source |
|---|---|---|---|---|---|---|---|
| **Ox-Head Club Jailer** (kind 0; kind 1 frost ox legacy) | `boss_oxhead_clubjailer` (72x72, 80f) + `_bloodrage` | idle 6f, move S/E/N 8f | gore windup + loop S/E, horn windup / strike, stomp windup / strike, roar, phase change | hurt 2f | death 10f, one direction | none (boss root hides after the anim) | own sheet (PixelLab); stun is code sway; blood-rage cracks faint |
| **Horse-Face** (kind 2, Mortar boss) | `boss_horseface_pestle` 96x96 x 106 (remapped into the Ox-Head 80 slots) | idle, walk S / SE / N (E has no pestle) | spin, overhead slam, rice-sack throw, leap | dizzy-recoil frames reused | death 7f + hold (SE only) | none | own sheet, **mock-up-quality remap** (hurt reuses dizzy, stun reuses stuck-pestle, phase change = dizzy) |
| **Ice Gorilla** (kind 3, Frost floor) | `boss_ice_gorilla` 80x80, 8 x 16 | idle, walk S/E/N | slam, throw S/E/N, leap, slide S/E/N, roar | hurt (S for all facings) | death 8f, S for all facings, hold | none | own sheet (PixelLab) |
| **Qilin Chariot** (kind 4, Burning floor) | `boss_qilin_chariot` 160x144, 9 x 19 | idle S, drive E/N/S | charge S/E/N, stomp S/E/N, breath S/E/N | hurt S/E | death S/E, hold | none | own sheet (PixelLab object) |
| Bosses: d2feel | stagger, knockback, blood, pools: all **skipped for bosses** (`!e.boss`) | | | | | | |

## 5. D2-layer coverage matrix (what exists vs what is missing)

Legend: Y = present for all 4 directions; S = south only; - = missing.

| Demon | Walk | Idle | Attack anim | Hit | Death | Corpse | Pool / decal | FX strips | PL (d2anim) |
|---|---|---|---|---|---|---|---|---|---|
| Imp | Y | - | - | Y PL | Y PL + HA | Y PL | Y | Y | **Y** |
| Ghoul | Y | - | - | Y HA stagger | Y HA | Y | Y | Y | - |
| Brute | Y | - | - | Y PL | Y PL + HA | Y PL | Y | Y | **Y** |
| Elite (wraith) | Y | - | - | Y HA | Y HA | Y | - | Y | - |
| Splitter | Y | - | - | Y HA | Y HA | one png | Y | Y | - |
| SplitChild | Y | - | - | Y HA | Y HA | one png | Y | Y | - |
| HookThrower | placeholder | - | - (code FX only) | stagger art, disabled | Y HA | Y | Y | Y | - |
| GossipImp | placeholder | - | - (code FX only) | stagger art, disabled | Y HA | Y | Y | Y | - |
| Frost Caster | own (S/E/N) | own | own | own (S/E) | own, S | **-** | **-** | - | - |
| Fire Wheel | own (E only) | frame 0 | own | own | own, E | **-** | **-** | - | - |
| Bloater (m53) | own | own | cast rows unused | own | own, S | **-** | **-** | - | - |
| Rice Ghost (m53) | own | own | cast rows unused | own | own, S | **-** | **-** | - | - |
| Grinder Monk (m53) | own | own | cast rows unused | own | own, S | **-** | **-** | - | - |
| Archer (k49 8) | old | - | - | - | Imp art (wrong body) | Imp art | Y | Y | - |
| All 4 bosses | own | own | own | own | own | **-** | **-** | - | - |

## 6. What is missing, in priority order for Hellord Art

1. **No corpse for Frost Caster, Fire Wheel, Bloater, Rice Ghost, Grinder Monk or any boss.** Their sheet death plays, then the sprite is hidden. These types need a corpse frame per direction (or "last death frame stays") and a pool/decal. The persistence code from this task (`d2CorpsePersist1447`) keeps whatever corpse the d2feel layer spawns, so these demons will keep nothing until the art and a hook exist. Code side: `Death44` / `Death53` do not call `D2SpawnCorpse1434`.
2. **No attack art for any ETypes demon**: Imp, Ghoul, Brute, Elite, Splitter, SplitChild, HookThrower, GossipImp attack with a code telegraph and contact damage only. Minimum set: an attack windup + strike per direction for each. The Hook Thrower needs a throw pose, the Gossip Imp a shout pose.
3. **No idle for the base ETypes** (the sprite freezes on walk frame 0).
4. **Hook Thrower and Gossip Imp are code-drawn placeholders** (walk and death body). They need real 4-direction sheets in the PixelLab style. The HA stagger / death / corpse already exist for them, but the stagger is switched off in code.
5. **PixelLab (d2anim) hit/death/corpse exists only for Imp and Brute.** Missing: Ghoul, Elite, Splitter, SplitChild, plus every variant above.
6. **Variants are recolours** (frost demon, bomber, imp / elite range kits, Frost Brute): they show the **base colours** at death and as corpses (d2feel / PL art is looked up by ETypes). Either tinted corpse / death strips per variant, or code tint at spawn. Archer is the old 24-frame sheet with no shoot / hit / corpse.
7. **Single-direction death rows**: Frost Caster, Bloater, Rice Ghost, Grinder Monk, Fire Wheel death, Gorilla hurt / death, Horse-Face death. The base ETypes old death strip also has no facing.
8. **Elite has no pool or decal** (white wraith, so possibly intended; confirm).
9. **Existing but unused art**: hellcat, calf (windup / charge / stun), herder (whip), hound, skeleton. No gameplay uses them yet.
10. **Mortar demon cast rows (rows 6-8) exist but are never played**: Bloater, Rice Ghost and Grinder Monk attack by contact only.
11. **Boss polish**: Ox-Head stun is a code sway and phase change reuses the roar; Horse-Face is a remapped mock-up (hurt / stun / phase reuse other frames); bosses have no hit-spark / blood / stagger layer.

## 7. Voices (for the sound pass)

The family keys used by the `d2sfx1447` hooks (default off, no clips committed yet): `imp, ghoul, brute, wraith (Elite), frost_caster, fire_wheel, bloater, rice_ghost, grinder_monk, oxhead, horseface, icegorilla, qilin`. Death hooks use 5 groups (`death_imp/ghoul/brute/wraith/caster`, variants fall back as: bloater -> ghoul, grinder -> brute, rice_ghost -> wraith, frost_caster / fire_wheel -> caster).
