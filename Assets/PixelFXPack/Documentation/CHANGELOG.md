# Changelog
## 2.5.1 - LevelUpFX QC fixes (no red-looking rings, continuous chained dim, shorter wait before the first pick)
- FIX colour: the v2.5.0 cyan ground wave, the rings and the plate border were alpha blends over the red floor; after the CRT they read grey-pink/orange (measured: old wave pixels mean hue 307 deg, only 18% in the cyan band; keysheet resume ring 32% red/orange pixels). The expanding ground wave is REMOVED. Every ring (level-up gold + white, pick ring by kind, skip ring), plate border, glyph and level digit is now an OPAQUE palette colour with an opaque dark 1 px outline and leaves by DISSOLVING (stable per-cell hash, each piece leaves once, no flicker, nothing alpha-blended). Dark outline/plate interior still fade by alpha (hue-neutral). Measured after the CRT (bright effect px, red/orange 330-40 deg): level-up rings 10% -> 2%, pick/resume rings 16% -> 1%, keysheet f141 resume ring 32% -> 1%.
- FIX chained level-up dim: the chained screen now starts at the dim the previous screen left (`Open` reads it internally, no new call) and eases to 1 over new `ChainDimInSeconds` (0.12 s). v2.5.0 mock-up: dim 0.72 -> 1.00 in one frame (14% luma blip in a card-free strip, 26.0 -> 29.7 -> 26.0); v2.5.1: 1.00 on every frame, step 0.0.
- CHANGE timing: `HeroBeatSeconds` 0.45 -> 0.25, new `PickLockSeconds` 0.20 (was a hard-coded 0.25), `DimInSeconds` 0.25 -> 0.20, `CardDelay` 0.10 -> 0.05, `CardStagger` 0.05 -> 0.04, `CardRevealSeconds` 0.18 -> 0.15. Time from level-up to first possible pick: 0.70 s -> 0.45 s (chained 0.20 s, unchanged).
- API: additive only (`PickLockSeconds`, `ChainDimInSeconds`). Same call order and hooks as v2.5.0. 153 effects.
- Mock-up numbers (review/v2.5.1/measurements/): 0 restarts on all 6 screens, 1 hump each; dim luma step max 9.0% of mean (0 frames >10%); resume dim 0 after 100 ms, last effect px gone after 267 ms; top-layer overdraw unchanged from v2.5.0.
- Caveats: not tested in Unity, no FPS measured; the mock-up CRT is an approximation, so absolute hue values will differ a little in the game; the first pick can land while the last card finishes its reveal (about 0.08 s).

## 2.5.0 - LevelUpFX (level-up juice in three beats, #151-153)
- New `LevelUpFX` (Runtime/LevelUpFX.cs): static API, pooled `It` struct runner (16 items), int handles (-1 when full; every call accepts -1 and then the screen simply opens with no effect), unscaled time (keeps stepping while the game's level-up pause is up), no per-frame alloc, 2 draw calls (top mesh order 70 + ground mesh -20000), `Prewarm()`.
- #151 beat (a)+(b) `Open(heroFeet, level)` when `OpenLevelUp` runs: the world is already frozen by the mode, so it stands UNDIMMED for `HeroBeatSeconds` (0.45 s) while a gold + white 3 px ground ring, 6 sparkles beside the hero (never over it), a gold-bordered plate (star + level number, up to 2 digits) above the head and a thin cyan ground wave (alpha 0.4, under characters) play. Then `DimK` eases the card-screen dim 0 -> 1 over `DimInSeconds` (0.25 s; was a 1-frame hard 69% cut), and `Reveal` / `SlidePx` stagger the title and cards in (10 px slide, 0.05 s stagger, starting 0.10 s after the beat). `CardsReady` locks the pick until beat + 0.25 s (about 0.7 s; chained screens 0.2 s).
- #152 beat (c) `Picked(h, LevelPick kind, heroFeet)` when a card is chosen / skipped: 0.30 s ground ring by kind (gold new skill / skill level, cyan support, steel stat, grey skip), a 28x26 kind plate (star / plus / gem / shield, none for Skip) above the head that is gone by 0.18 s, 2 sparkles.
- #153 resume: `DimK(h)` keeps returning a ramp 1 -> 0 over `ResumeFadeSeconds` (0.12 s, 4 frames, about 25% steps) after `Picked`, so the world un-dims while the sim already runs. The game must keep calling DimK in Play-mode OnGUI until it returns 0 (see HELLORD_FX_MAP.md).
- Chained level-ups (a pick within 0.25 s of the previous): no hero beat, dim already 1, fast card reveal.
- Rules kept: Endesga 32 only, no red or orange (no hazard language, no chevrons, no blink, no loops), every part is a one-shot with one smooth fade (no sawtooth restarts), nothing under 3 px, no full-screen flash (the only screen-wide change is the dim ramp), the sprite is never tinted, nothing above 1 Hz. Tunables: `HeroBeatSeconds` (0 = no beat), `DimInSeconds`, `ResumeFadeSeconds` (0 = old hard cut), `CardDelay`, `CardStagger`, `CardRevealSeconds`, `CardSlidePx`, `HeroHeadPx`, `SetGroundSorting`.
- Mock-up numbers (review/v2.5.0/measurements/measure_lu_report.txt): 0 restarts on all 6 screens; one smooth hump per effect; sim max frame-to-frame change 20-22% of peak (CRT png 29-36%, mp4 18-24%, the ring's onset frame); screen luminance step 8.8% of mean (old hard dim 31.5%), 0 frames over 10% (old: 12); after resume the dim is gone in 100 ms and the last effect pixels in 267 ms (under 0.3 s).
- Caveats: the 0.45 s hero beat adds time before the cards (tunable); the pick plate can cover enemies above the hero for up to 0.18 s after resume in a very dense spot; the game files are not edited by the pack (hook plan in HELLORD_FX_MAP.md is assumed-for-now). Not tested in Unity, no FPS measured. Mock-up only (review/v2.5.0/).

## 2.4.2 - InteractFX motion-review fixes (strobing, ring under characters, lower shrine plate)
- FIX strobing #147/#148: measured cause = the sim, not frame-skipping or encoding. The elite ripple restarted every 1.4 s (alpha 0.01 -> 0.89 in ONE frame, the ripple quad area x alpha swung 9.5 -> 433 between neighbouring frames), the use-spot ripple restarted every 0.9 s (1.1 Hz, same hard restart), the use-spot ring breathed 0.35 <-> 0.75 (2:1) at 0.8 Hz, plates bobbed in 1-2 px integer steps and the sparkles switched shape. The CRT scanline/fringe amplified it (cyan count cv 9% sim -> 16% CRT -> 20% mp4); the mp4 (crf 18) loses about 45% of the cyan pixels but adds no flicker. 0 duplicate frames in 855.
- Now steady: no sawtooth ripples, no sparkles, no bob. The elite has a fixed outer ground ring (alpha 0.60 +/- 0.12, 0.5 Hz smooth sine, floor 67%). Use-spot ring breath is 0.72 +/- 0.10 at 0.5 Hz (floor 76% of peak) with a steady outer ring in the zone (0.55 +/- 0.12 at 0.5 Hz). Fades (near/ready) slowed from 0.2 s / 0.125 s to 0.33 s so a hero crossing the zone edge cannot flicker the plate. Nothing above 0.5 Hz.
- FIX #147 draw order: the elite halo discs, the expanding ripple and (use-spot) ripple were on the top mesh, so they washed over the hero and enemies. Halo, ground ring and outer ring now all draw on the ground mesh (-20000, under characters). Only the plate + pin stay on top (a pin and a 32x30 plate are needed readable over the horde). Mock-up: top-layer pixels drawn over horde sprites 245 -> 67 per frame (2-7 s, plate+pin only).
- FIX #148 shrine/shop plate lift: `PlateLiftPx` defaults are now Chest 0, Shrine 73, Shop 64 (were 0 / 83 / 68). Shrine: the plate (24 px wide inner box) nests in the V of the statue's raised hands, its bottom edge 94 px above the spot centre, the skull top about 4 px under it. Shop: the plate bottom sits on the awning's top edge (85 px above the spot centre). API unchanged: `SetPlateLift`, `UseSpot(..., liftPx)`.
- Cost: `GndQ` 1536 -> 4096 and `TopQ` 4096 -> 3072 (a mark is about 304 ground + 63 top quads: about 13 simultaneous elite markers). Still 2 draw calls, pooled, no per-frame alloc, -1 handles when full; overflow quads are dropped silently.
- Not tested in Unity, no FPS measured. Mock-up only (review/v2.4.2/).

## 2.4.1 - InteractFX QC fixes (always-lit elite marker, use-spot plate lift)
- FIX #147: the elite marker was only findable during its ripple (small dark plate blended into the floor at 1x in a horde). It is now a steady beacon, lit at all times: 32x30 plate with a 3 px cyan border and dark outline (inner 24x22, icon 16x14), a steady cyan halo (two soft discs) behind it, a 3 px cyan pin down to the head, and a steady 3 px cyan ground ring under the elite (ground layer, under characters). The ripple and two sparkles stay (0.8 Hz). Sprite is never tinted; cyan only, no red/orange, no chevrons, no blink. Plate centre is about 66 px above the feet.
- FIX #148: the use-spot plate sat on the statue's face/torso (shrine) and the awning (shop). New per-kind lift: `InteractFX.PlateLiftPx` (defaults Chest 0, Shrine 83, Shop 68, measured from the HA props: shrine 84 px tall, awning 70 px), `InteractFX.SetPlateLift(kind, px)` and `UseSpot(centre, kind, liftPx)` (omit or NaN = per-kind default). Plate centre = spot centre + 32 + lift.
- Cost: `TopQ` raised 3072 -> 4096 (a mark is about 250 quads on the top layer; about 16 elites at once fit). Still 2 draw calls, pooled, no per-frame alloc, -1 handles when full.
- Not tested in Unity, no FPS measured. Mock-up only (review/v2.4.1/).

## 2.4.0 - InteractFX (elite marker + chest/shrine/shop use-spot + open bursts, #147-150)
- New `InteractFX` (Runtime/InteractFX.cs), static API with int handles (-1 when the 64-item pool is full), pooled, no per-frame alloc, unscaled time, 2 draw calls (top mesh + ground mesh).
- #147 `EliteMark(feet)` / `MoveMark(h, feet)` / `Kill(h)`: a 22x20 dark plate above the head with an 8x7 purple-chest-and-cyan-gem icon (the game's own elite-chest colours), a 3 px cyan ripple every 1.4 s and two sparkles. Sits ABOVE the sprite, never tints it.
- #148 `UseSpot(centre, UseKind)` + `SetHero(feet)` + `InRange(h)`: far = cyan motes; near (40 px) = 3 px cyan ground ring (under characters, -20000) that breathes; in the zone (30x16 px, hysteresis) = white-cyan ring + ripple + a kind plate (chest / rune eye / coin) rising above. Cyan only, so it never reads as a hazard tell.
- #149 `OpenChest(pos, elite)`: white pop, 8-10 yellow/white rays, yellow ring (+ cyan ring for elite), coin fountain (elite: 12 with every 3rd a cyan gem), sparkles. `OnOpened(kind, pos)` fires at 0.12 s (kind 0 chest, 1 elite chest, 2 shrine, 3 shop) for SFX/loot.
- #150 `ShrineUsed(pos)` (white star pop, cyan rings, orbiting cyan/white diamonds, rising motes) and `Purchase(pos)` (yellow ring, 5 coins, 2 sparkles).
- Rules kept: no red/orange fills, no danger borders, no chevrons, no blink, nothing over 3 Hz, no full-screen flash, nothing under 3 px, plate icons on dark plates.
- Not tested in Unity, no FPS measured.

## 2.3.1 - GossipHookFX Shout fill under characters
- FIX: the Shout warning fill (and the burst flash) was drawn above characters, so the imp turned pink during the 0.6 s tell. They now draw on a second child renderer at sorting order -20000 (above floor/props/atmos, below every character `YSort` order).
- New API: `GossipHookFX.SetFillSorting(layerName, order)`, statics `FillSortingLayer` ("Default"), `FillSortingOrder` (-20000), `ShoutFillColor` (#f6757a a158), `ShoutEdgePx` (3; 1 = old look).
- The rim, growth edge, burst ring and "!" stay above characters on purpose: thin lines cannot tint a sprite, and they must cross the horde to stay readable. Rim and growth edge are now 3 px (dark backing 5 px) instead of 1 px, because 1 px vanished under CRT scanlines in a 120-enemy horde.
- Cost: 2 draw calls for Shout instead of 1 (fill mesh + edge mesh). Pooled, no per-frame alloc, no game change needed (defaults apply).
## 2.3.0 - RoomFX (nest destroyed + stairs/descend, #142-146)
- New `RoomFX` (Runtime/RoomFX.cs): `NestDestroyed(pos)` = collapse burst (#142: debris drops in, gold ring + sparks out) + persistent `NestDeadMarker` (#143: cold slate broken seal + slow grey embers, reads "off").
- `StairsOpen(pos)` (#144): cyan ring + gold light posts reveal, then a calm beacon (gold brackets 0.8 Hz, bobbing down-chevron, cyan motes).
- #145 off-screen stairs marker: gold stairs glyph on a dark plate + cyan arrow, `EdgeInsetPx` 40, `EdgeScale` 2 (same rules as DoorFX).
- `Descend(pos, beacon)` (#146): gold column, then a dark iris (#181425, never white) to black; `OnDescendBeat(0..3)`, `OnTransition(pos)` at 0.9 s to load the next floor; `DescendReveal(pos)` irises back open (0.5 s). Also `OnNestCollapsed(handle, pos)`.
- Gold/cyan reward + exit language only: no orange fills, no crawling borders, nothing closes in, nothing over 3 Hz. Pooled (64 items), one draw call, unscaled time.

## 2.2.1 - HurtFX hit read
- #137 sparks are bright gold/white with a dark outline, at least 3 px, 8-13 px long.
- #137 punch: gold brackets pop 1.35x for 2 frames, plus a 0.12 s gold ring with white core (3-5 px stroke, 8 -> 22 px). No screen flash.

## 2.2.0 - HurtFX (hero damage + death, #137-141)
- New `HurtFX`: `Hit(hero, source, dmg, heroPixelFX)` 1-2 frame white silhouette outline + 6 directional sparks; `IFrames` shrinking ring; `LowHpPips` 8x6 heart pips; `LowHpEdge`/`OnEdgeBump` feed the existing CRT low-HP pulse; `Death` impact -> shatter -> soul -> done.
- Callbacks `OnHit`, `OnEdgeBump`, `OnDeathBeat` with timing: hit HitStop 0.05 s + SFX at beat 0; death HitStop(0.15,0.3,0.5), SFX at 0 / 0.12 / 0.45 s, UI at 1.6 s.
- Pooled (64 items, 2048 quads), one draw call, unscaled time, no flash/flicker > 3 Hz.

## 2.1.1 - SkillFX QC fixes
- Fireball (#132): 13 px amber/gold head (#feae34 rim, #fee761 core), 6-step comet trail; no white/silver.
- Bone Orbit (#135): #63c74d green glow rim + green trail (60/80 deg) so skulls never read as skeleton/ghost enemies.
- Curse Rune (#136): glyphs rotate, arcs radiate OUTWARD during the fuse; nothing closes in.
- Clear zone: `SkillFX.SetHero(pos)`, `ClearZonePx` (26) / `ClearZoneAlpha` (0.3) fade all SkillFX near the hero; Lv5 crowns use skill hues; fewer white sparks.
- Hero signature revised: white core is no longer universal (accent only).

## 2.1.0 – 2026-10-27 (Hero skill FX)
- New `SkillFX` (Runtime/SkillFX.cs), 5 effects #132-136, 1 pooled mesh (1 draw call), no per-frame alloc, int handles (-1 when the pool is full).
- #132 Fireball: gold-white head + silver streak (FireballSpawn/Move/End), ring-only burst (FireballBurst).
- #133 Frost Cleave: cyan tapered crescent that sweeps the arc, plus ice motes.
- #134 Storm Ring: 3-5 rotating deep-blue arc segments (never a full ring) + StormTick sparks + StormArc zigzag bolts.
- #135 Bone Orbit: 11x11 skulls with 3x3 eyes and silver orbit trails (immediate, per frame) + BoneHit.
- #136 Curse Rune: glyph circle draws in (arm), 3 white arcs close in (fuse), violet burst -> OnRuneBurst; CurseMark over cursed enemies.
- Shared HERO SIGNATURE: white core + silver #c0cbdc rim + dark outline, strokes only, continuous motion, never red/orange fills, bright filled borders, chevrons or blinking.
- Levels 1-5: Lv3 adds a layer, Lv5 adds a white spark crown; size comes from the radius/arc you pass.

## 2.0.1 – 2026-10-27 (PickupFX external drop sprites)
- #128 PickupFX.SetDropSprites(kind, Sprite[] frames, fps) / SetDropSprite(kind, sprite) / ClearDropSprites(kind): draw a kind with your own art (e.g. a game's blood-drop pickups) instead of the code gem. Code gems stay the default.
- Still pooled: all sprite pickups share ONE extra mesh (3 draw calls total). All frames must share one texture (SpriteAtlas, Full Rect packing, or one sheet); different textures are repacked once at registration if Read/Write is enabled, otherwise a warning and the kind keeps the code gem.
- PickupFX.SetKindColor(kind, color): trail/pop colour per kind (also the small/medium code gem body). Use white/bone for red art so motion never reads as enemy red.
- Colour note: red blood-drop art (#c0102a / #ff6070) is only 62-73 RGB from hero #ff2448 and enemy #e43b44 and sits on a red floor; see review/v201.

## 2.0.0 – 2026-10-27 (Pickups + Level-up: PickupFX)
- NEW PickupFX #128-131 (Runtime/PickupFX.cs). Reward language only: cyan/green gems, a pale-yellow large gem, bone-white bones, no red, no hazard stripes, no blinking fills, nothing thinner than 3 px.
- #128 Drop(pos, kind, value): 5x5 plus-shaped gem (3 px arms, 7x7 dark outline) or 9x5 bone, 1 px bob, no glint. Large gem: 7x7 faceted octagon in pale yellow #fee761 with a #feae34 facet, a 3x3 white core and a 3x3 sparkle every 1.6 s (contrast: #fee761 vs #feae34 = 73, vs orange #f77622 = 130, white core vs #f77622 = 260 RGB; old #feae34 gem vs #f77622 was only 59). Hundreds at once in ONE pooled mesh (~6 quads each, MaxPickups 600). Pool full -> merges value into the nearest pickup and upgrades its tier (VS/Brotato-style cap); MergeWhenFull=false returns -1.
- #129 Magnet: SetTarget(hero) each frame; AutoMagnetRadiusPx 40, Magnet(h), MagnetAll(r), Vacuum(). Hop out 4 px (0.08 s) then accelerate (cap 420 px/s) with a 2-dot low-alpha trail; collect pop 0.18 s (white core, ring <= 10 px, 4 sparks).
- #130 Optional XP bar (ShowBar): 6 px bar (3 px highlight) on a dark plate inset 24 px from the view top; white leading-edge flash on gain, gold for 0.4 s on level-up. SetBar(fill01).
- #131 LevelUp(feet, level): 0.5 s, LevelUpAlpha 0.5: white + gold 3 px rings (squashed 0.6), 10 rising 3x3 sparks, 11x13 stepped up-arrow (3 px parts) on a dark plate. No pillar, no screen flash.
- SFX: OnCollect(kind, value) for every pickup; OnPickupSfx(kind, pitch, batched) rate-limited to 1 per 0.045 s, pitch +1 semitone per emitted SFX within ChainWindow 0.35 s, capped at +12 (pitch 2.0); OnBarFull, OnLevelUp(level).
- 2 draw calls total: pickups + trails at SortingOrder 20 (under enemies), pops/level-up/bar at SortingOrderTop 74. Old HellFX XPGem/BonePickup/Magnet/LevelUpBurst kept unchanged for back-compat (MAJOR bump = new default pickup path; see README upgrade notes).

## 1.9.9 – 2026-10-27 (DoorFX fixes)
- #127 Off-screen door markers: padlock now 2x (22x26 glyph on a 30x34 plate, 3 px cyan/gold frame, extra dark outline), centred EdgeInsetPx = 40 px in from the screen edge so it sits outside the darkest CRT vignette and clear of top/bottom debug-text strips. New tunables: EdgeInsetPx, EdgeScale (2), SameWallGapPx (60).
- #127 Off-screen door on the same wall as a visible door: its marker slides along the edge (SameWallGapPx away from the visible door) and always carries a short 3 px arrow (shaft + chevron) pointing at the off-screen door, so it reads as a second exit.
- #127 Unlock chevrons start on the outer side of the wall line and only ever move outward (-normal) through the doorway, never toward the hero.

## 1.9.8 – 2026-10-27 (Door Lock / Room Cleared: DoorFX)
- NEW DoorFX #127 (Runtime/DoorFX.cs): combat-room door lock and room-cleared unlock, visual only. Pooled, 1 draw call, no per-frame alloc, int handles (-1 when pool full), Endesga 32, no sprite tint, nothing thinner than 3 px.
- Sealed language, deliberately NOT a hazard tell: steel bars (#c0cbdc/#8b9bb4) slam in with a white threshold flash and grey dust, an 11x13 padlock on a dark plate with a cyan frame, a cyan #0099db 3 px seal seam with a slow travelling glint. No red, no hazard stripes, no blinking.
- Unlock = reward: padlock pops open in gold and lifts, bars retract, gold 3 px ring + 12 gold/white sparks, 3 outward gold chevrons through each doorway; optional ClearBurst(centre) gold ring at the hero.
- Off-screen doors get an edge marker (padlock on dark plate + 3 px chevron toward the door; cyan while locked, gold while opening). Uses Camera.main (orthographic).
- API: RegisterDoor(centre, inwardNormal, width), LockAll(), UnlockAll(), ClearBurst(centre), GetState, Unregister, Clear. Callbacks OnLock, OnUnlock, OnDoorLocked(door), OnDoorUnlocked(door) with SFX beats documented in README.
- Level 12 (MortarFX) cancelled; draft kept in wip/v198_mortar/, not in the package.

## 1.9.7 – 2026-10-27 (StoneFX fixes, full StoneFX set)
- #123 Throw arc: 3x3 yellow dots with a 1 px dark outline every 3 px, plus a solid white tracer running hand -> target. (The v1.9.6 clip did show 2x2 dots; under the CRT scanlines they read about 1 px tall.)
- #124 Quake Line: fill is now bright yellow-orange #feae34 at 60 % (was orange #f77622 at 40 %), inner border gold, default width 20 px (was 16). Clear of enemy and hero reds even where enemies cover the border.
- #125 Rock Armor: pips 8x6 px, bright (#c0cbdc + white top edge) with a 1 px dark outline (JudgeFX #116 size); cracked pips go dark. Head offset 100 px (Warden is 96 px tall). If the pips would be within 24 px of the camera's top edge they move under the feet. Punish window unchanged.
- #123 Drop and #126 Pop unchanged (passed QC). Level 12 work is paused and moved to wip/ (not in the package).

## 1.9.6 – 2026-10-27 (Level 11 Stone Crushing: StoneFX)
- NEW StoneFX (#123-#126), same runner pattern as OxPitFX: pooled, 1 draw call, no per-frame alloc, int handles (-1 when pool full), pixel-snapped fills, Endesga 32, no sprite tint. Built to the producer QC rules: bright border on every danger area, fills clear of the hero's red (orange/dark only), a visible wind-up on every attack, icons 5x7 on dark plates.
- #123 BoulderDrop(spot) - level hazard "boulders fall on shadows" + Stone Warden ceiling fall: growing dark shadow inside a crawling yellow/dark hazard border, falling boulder as wind-up, white blink last 15 %; impact 0.35 s (white ring, dust ring, pebbles, cracks), OnStrike. BoulderThrow(boss, spot): hand glow wind-up, dotted yellow arc, orange 45 % landing circle with 2 px yellow/orange border.
- #124 QuakeLine(from, to, 16 px): stomp burst wind-up, orange band + bright border, racing crack; strike 0.45 s erupts stone spikes along the line.
- #125 RockArmor(boss, plates) + ArmorHit(h): plate pips on dark plates above the head (84 px); the last hit breaks the armor (shard burst) and opens a 4 s punish window (pulsing yellow corner brackets + 5x7 broken-shield icon). OnArmorBreak / OnArmorRestore.
- #126 DisguisePop(pos): Rock Crawler - jittering pebbles, tightening dashed yellow/orange ring, 5x7 '!' on a dark plate; pop 0.35 s with dust ring and chips.
- Pre-release QC fixes: throw arc = 2x2 yellow dots on dark 4x4 backing + moving white tracer; crawler pop icon 7x9 bold "!" on a 9x11 dark plate (5x7 "!" made bold, fills its cell); pop ring 20->12 px, 2 px thick, white blink + star in the last 25 % before the pop.
- Slab Bearer sparks skipped (already exist). Compile-checked; not tested in the Unity Editor.

## 1.9.5 – 2026-10-27 (Ox Pit fixes, full OxPitFX set)
- #119 Gore lane: fill is now orange (45 %) instead of dark red, so the red hero (#ff2448) no longer camouflages; bright 2 px border (yellow outer, orange inner, white blink in the last 15 %).
- #119 Calf lane: same bright border; chevrons scaled down (3 px) and every 10 px so it holds up in the horde.
- #122 Herder whip: 2 px yellow/gold lash with a white tip, plus a wind-up flash (star + ring) at the herder's hand. Haste keeps the ">>" icon; the speed streaks are replaced by a ground-ellipse colour rim (12x5 px, crawling yellow/gold dashes) drawn in the FX mesh - no sprite tint, still 1 draw call.
- All OxPitFX fills are pixel-snapped (convex fills rasterised into whole-pixel row spans), so they don't shimmer when rotated.
- #120 Horn sweep and #121 Stampede unchanged (passed QC).

## 1.9.4 – 2026-10-27 (Ox Pit, Level 10)
- NEW OxPitFX (#119-#122). Pooled, 1 draw call (translucent fills + outlines in one mesh), no per-frame alloc, int handles (-1 when pool full). Endesga 32 only; sprites are never tinted.
- #119 GoreLane(feet, target, calf, wallEnd): 40 px Ox-Head lane (0.75 s, 160 px) or calf lane (24 px, 80 px, 0.58 s = 7 wind-up frames at 12 fps, with kicked-back dust). Red fill grows to a 3 px end cap, chevrons march, rim blinks white in the last 15 %; wallEnd adds a yellow wall-hit cue. Strike: white edge flash + streaks; OnStrike fires.
- #120 HornSweep(feet, facing, sweepDir): 110 deg / 60 px cone from the horns (+38 px above the feet); the fill sweeps in the swing direction (0.7 s), then a 0.2 s slash trail.
- #121 StampedeLane(gate, dir): 40 px x 320 px yellow/dark hazard-striped lane with marching hoof marks and a bone-tooth gate bar (1.0 s) - styled apart from the red/orange boss lanes.
- #122 Whip(herder, ally) + Haste(ally, 3 s): wavy lash lands at 0.25 s (whip frame 3), then a ">>" icon on a dark plate + speed streaks at the feet; blinks in the last 0.5 s.
- PAL14 check: colour distances logged (see EFFECTS); main clash = red gore fill vs red-skinned hero/imps.

## 1.9.3 – 2026-10-27 (QC fixes only)
- Judge Kit #116: the stun bar now sits above the "STUN" badge, above the head (was over the face/eyes).
- Hook Chain #115: weak link is a screen-aligned gold diamond, 7 px pulsing to 9 px, white core, gold edge, opaque 1 px dark outline (was 5-7 px with no outline).
- Gossip Imp #117: Shout fill is a brighter pink (62 %) with a white growth edge (was dim red 45 %). Scramble "?" icons are bold 5x7 on opaque 7x9 dark plates (outlined), orbit radius 13 px.
- Hook Thrower #118: WindupSeconds 0.45 s + bright HookChainFX lane tell 0.5 s (orange edges, chevrons, white end blink) = 0.95 s total (assumed default).
- Tidy: EFFECTS.md header now says 118 effects; unused colour constant (and the unused old 3x5 '?' glyph) removed from GossipHookFX; README GossipHookFX example fixed (static calls, no .Instance).

## 1.9.2 – 2026-10-27
- NEW GossipHookFX (#117 Gossip Imp Shout + Scramble, #118 Hook Thrower wind-up). Pooled, 1 draw call, no per-frame alloc, int handles (-1 when pool full).
- Shout(center, r=48px, tell=0.6s): real-style red fill + solid orange rim + bouncing "!"; Burst 0.3s fires OnShout.
- Scramble(victim, 2.5s): orbiting "?" on dark plates + two counter-rotating foot arrows; blinks last 0.5s. Game owns the actual input scrambling.
- HookWindup(thrower, target, 0.4s): spinning hook, ready ring, aim chevrons, then calls HookChainFX.Throw with a 0.5s lane tell (0.9s total), fires OnHookThrown.
- Defaults are assumptions – tune in inspector.

## 1.9.1 (2026-10-27)
- Judge Kit #116, gavel meter: each pip is now 8x6 px with a 1 px dark outline, gold when intact (bright on red floors) and steel with a dark crack when broken. It is more readable at 1x.
- Judge Kit #116, stun beat: it now uses the same look as Boss Stun Bar #114. A "STUN" badge (3x5 letters on a dark plate, blinking yellow/white), 3 big 7x7 stars orbiting with front/back depth, and a 32x3 px draining bar with marching gold hazard stripes and a dark frame. Everything blinks in the last 0.5 s.
- Hook Chain #115: the weak link is now a gold diamond with a white core that pulses between radius 2 and 3, faster with each break pip, so it stays visible during fast drags through hordes.
- Compile-checked against Unity 6000.0.85f1 DLLs; not tested in the Unity Editor; no FPS measured.

## 1.9.0 (2026-10-27)
- New effect #116 Judge Kit (`JudgeFX`): tongue-whip lane, hook-pull line, gavel-slam area, real vs fake warnings, 5-hit gavel break and 3 s stun beat. 116 effects.
- Hook Chain: `DefaultCooldown` is 4 s (change it per game).
- Compile-checked against Unity 6000.0.85f1 DLLs; not tested in the Unity Editor; no FPS measured.

## 1.8.9 (2026-10-27)
- NEW #115 Hook Chain (`HookChainFX`, Runtime/HookChainFX.cs): a chain-hook hazard that drags the player. TELL (0.7 s): lane exactly as wide as the hook hitbox (dotted rust edges), hollow hook ghost at max range, steel chevrons marching outward, pulsing anchor ring; white blink + 2 px wind-up recoil in the last 15%. FLY (420 px/s): 1-frame white smear, anchor release flash, alternating steel links and speed pixels. MISS: clank sparks, then retract. DRAG: 1-frame dark contrast disc + latch ring, taut chain with links and rust/white chevrons marching toward the anchor, break pips above the victim, sideways escape arrows and a weak middle link that glows and shakes more per pip. SNAP: burst at the middle link, anchor half whips back, 6 links fall. RELEASE: reels in over 0.15 s.
- API: `HookChainFX.Throw(anchor, target, rangePx=160, tell=0.7f, speedPx=420, widthPx=6, breakCount=3)` -> handle (-1 if pool full); `TryGetTip`, `Latch(h, victim, offsetPx)`, `AddBreak(h)` (true = snapped), `Release`, `SetAnchor`, `GetPhase`, `Break01`, `Cancel`, `IsAlive`, `Clear`, `ActiveCount`, `Prewarm(16)`; callbacks `OnLaunch`, `OnMiss`, `OnSnap`; `MaterialOverride`, `SortingOrder` (72) / `SortingLayer`. Visual only: the game owns collision and movement. One pooled mesh, one 4x4 white texture, one material (1 draw call), no per-frame allocations. The sprite is never tinted.
- 115 effects. Compile-checked against Unity 6000.0.85f1 DLLs; nothing tested in the Unity Editor, no FPS measured.
## 1.8.8 (2026-10-27)
- NEW #114 Boss Stun Bar (`StunBarFX`, Runtime/StunBarFX.cs): shows on the boss HP bar that the boss is STUNNED / VULNERABLE without looking like lost HP. It never uses red and never changes the fill. It is an overlay texture (bar + 14 px each side / 14 above / 5 below), point filtered and redrawn only while active. Intro pop (0.25 s): white flash on the track, border bulges 1 px out, a "STUN" badge overshoots up (-3, +2, +1, 0 px), gold stripes wipe in, 6-spark burst. Sustain: gold diagonal hazard stripes (55%) march at 16 px/s over the WHOLE track (full and empty parts), border blinks yellow/white at 4 Hz, a glint runs along the top edge every 0.6 s, and 2 stars + 1 birdie (same motif as in-world StunStars #104) orbit the badge. Optional timer: 1 px yellow line under the bar shrinking to the centre, blinking in the last 25%. End recovery (0.3 s): stripes wipe out left to right, border flashes white once and fades, badge drops 3 px and fades, stars fly off.
- API: `StunBarFX` component on the boss HP bar: `Begin(float duration = 0, bool showTimer = true)`, `BeginWithStars(float duration, Transform bossOverride = null, bool showTimer = true)` (also starts in-world StunStars #104 with the same duration), `End(bool instant = false)`, `IsStunned`, `IsVisible`, `Remaining01`, `OnStunEnd`, `Redraw()`; fields `barWidthPx` (160), `barHeightPx` (6), `fillOriginLocal`, `createSpriteRenderer`, `sortingLayer`/`sortingOrder` (500), `pixelsPerUnit`, `boss`, `bossHeadYPx`; UI: `Texture`, `TextureSizePx`, `FillPivot01`. SpriteRenderer bars: a child SpriteRenderer is created automatically. UI bars: assign `Texture` to a RawImage (no UnityEngine.UI dependency in the package).
- Research: `research/stunbar_v188_references.md`. Mock-up: `review/v188/`. 114 effects. Nothing tested in the Unity Editor.

## 1.8.7 (2026-10-27)
- REBUILT #110 Burn Status as `BurnFX` (Runtime/BurnFX.cs). The enemy stays fully visible and is NOT tinted: flame tongues on a BACK renderer (`BurnFX.BackSortingOrder`, default -1, must be below your enemies) rise from the head and lick out at the shoulders; 2-3 tiny 3x3 flame licks hop over the upper body and 1 px embers rise (front renderer, `FrontSortingOrder` 62). Every damage tick: flames flare one ramp step hotter for 0.13 s, 4 sparks pop, and a small outlined gold number floats 8 px up ABOVE the head (white for 2 frames). End: flames shrink 0.2 s, then 3 violet-grey smoke puffs rise and shrink. Sized for 25-41 px enemies (width/height from SpriteRenderer bounds or passed in).
- New API: `BurnFX.Ignite(Transform target, float seconds = 3f, int tickDamage = 1, float tickInterval = 0.5f, float widthPx = 0, float heightPx = 0, float feetYPx = NaN)` -> handle; `BurnFX.OnTick` (handle, target, damage); `BurnFX.Tick(handle, dmg)`; `BurnFX.Extinguish(handle, smoke = true)`; `IsAlive`, `Find`, `Clear`, `Prewarm(256)`, `ActiveCount`. Damage is synced via OnTick; re-igniting a burning target keeps the higher damage and longer time (no stacking). All burns share 2 meshes + 1 atlas + 1 material (2 draw calls); pool grows to 4096; horde LOD above `LodThreshold` (64) burns.
- `StatusFX.BurnStatus` keeps its signature and now routes to BurnFX (handles carry `StatusFX.BurnFlag` = 1<<30; `StopStatus`/`IsAlive` route them). CHANGES: `color` and `order` are ignored; no tint flicker any more (`BurnTintA/B` unused); burns no longer count toward `ReactionFXRunner.StatusCap` or TellFXRunner's 48 slots; BurnStatus has no damage numbers (tickDamage 0) - use BurnFX.Ignite for numbers. Optional `BurnFX.TickTint` (off by default) = 2-frame warm multiply per tick.
- Research: `research/burn_v187_references.md`. Mock-up: `review/v187/`. Still 113 effects (#110 upgraded in place). Nothing tested in the Unity Editor.

## 1.8.6 (2026-10-27)
- NEW #113 PatternWarnFX (Runtime/PatternWarnFX.cs): 0.45s boss pattern telegraph for Ring, RadialBurst, AimedFan and Spiral. It shows the origin (shrinking sigil + seed orb), the exact bullet directions (hollow ghost bullets at the locked angles) and the pattern shape (dotted ring, marching dashed spokes, fan wedge + chevrons, spiral arm trails + spin arrow). Blinks white for the last 15%, then fires BossBulletFX #112 (Ring / Spread / timed spiral) with a white flash + lime shock ring. Dark danger wash on a second renderer UNDER the bullets (sorting 69); the telegraph itself is at sorting 71 with dark 3x3 backing per pixel. Pooled structs, one mesh + one wash mesh, no per-frame allocations.
- TickLane outline: removed the back end cap added in 1.8.5 (the approved v1.8 mock-up is open at the attacker); chevrons resized to the mock-up's proportions.

## 1.8.5
- New #112 **Boss Bullet** (`BossBulletFX`, Runtime/BossBulletFX.cs): enemy projectile for Hellord at 640x360 / 16 PPU, PAL14. Round 7px (Small) or 11px (Large) orb: white core, PAL14 'tox' lime ring, 1px #100010 rim, ~5 Hz core pulse, 2-frame spawn flash, 3-frame despawn pop. Distinct in shape (round vs elongated) and hue (lime, unused by ProjectileFX #75-91) from player shots. All bullets are quads in one dynamic mesh with one 108x12 atlas and one material: 1 draw call, no per-bullet GameObjects, no per-frame allocation after `Prewarm` (default capacity 256). Research: `research/bossbullet_v185_references.md`. 112 effects.
- Fix (reported by Hellord Maker): `TellFX` lane telegraph "outline" was a filled box at 75% under the wash (solid dark-red strip). It is now a true 1px border (2 long sides + 2 end caps) just outside the rim, as in the approved v1.8 lane mock-up.

## 1.8.4
- TellFX: impact frames are now a single white silhouette frame (≈2 frames @60fps). The second, black silhouette frame was removed — on dark arenas it read as a solid black hole over the boss (stun stars + wall impact).
- HordeFX: numbers get a dark backing plate (`numberPlate`, default on) and obstacle-aware placement — kill bursts block their spot for `numberObstacleSeconds` (0.15s); new `HordeFX.AddNumberObstacle(pos, radiusPx, seconds)` for drops / pickups / sparks.
- Damage numbers (Hellord rule): numbers must sit on a dark backing plate and avoid live drops / pop / spark fx so fragments never read as extra digits.

## 1.8.3 - 2026-10-25
- HordeFX damage numbers: one total per burst. Hits on the same target merge while less than `numberMergeWindow` (0.4s) has passed since its last hit; crits bump the same number (yellow glyphs + 2x pop), never a second number. Life extends with each hit.
- Overlap avoidance: each new number is checked against the bounding boxes (including rise path and pop scale) of all live numbers and tries up/sideways slots; if none is free it is dropped. Visible count still capped by `maxNumbers`.
- Mock-up: `review/v183/oxpit_gameplay_v1.8.3_MOCKUP.mp4` (boss shows one rising total per burst; bat numbers merged and laid out; the render checks every frame for text overlaps: 0 found).
## 1.8.2 - 2026-10-25
- Review fixes; everything else as in 1.8.1.
- Impact frames (WallImpact, StunStars): silhouette is now the target's own size (1.03-1.04x, was up to 1.15x), one white frame then one black frame, and only one impact-frame pair per target at a time (WallImpact + StunStars no longer stack). BossHit flash: 1 frame, 1.03x. Mock-up: no world dim, boss hit flash is a 1px white outline.
- Stomp impact star: about half the size (1.0 -> 0.7x, was 2 -> 1.4x), gold/orange instead of white, 0.05s instead of 0.067s; shockwave ring is gold at 70% alpha. StunStars star 1.6 -> 1x, WallImpact star 2 -> 1.2x.
- HordeFX damage numbers: hits on the same SpriteRenderer within `numberMergeWindow` (0.25s) merge into one rising total; new numbers get +-4px x jitter and a 5px vertical stagger per nearby number (max 3, `numberStackPx`). New overload `HordeFX.Number(pos, value, crit, mergeKey)`.
- Mock-up: `review/v182/oxpit_gameplay_v1.8.2_MOCKUP.mp4`.
## 1.8.1 - 2026-10-25
- Juice pass on the Ox Pit (research: `research/oxpit_v18_references.md`, section v1.8.1). Old API unchanged; 111 effects.
- Tells: a "tell complete" pop (glint + 1px ring) when Lane/Cone/Stomp fire; directional screen kick (lane backwards, cone along aim, stomp down); stomp landing adds a 0.05s hit-stop, 2-step impact star and a fast white shockwave ring.
- Stun Stars rework: 2-frame white then black squashed impact silhouette, 6 pebbles, 3 squash/stretch stars + 2 flapping birdies on a tilted ellipse, cracked-horn sparks every 0.45s, 1-1.5px wobble (only when the SpriteRenderer is on a child transform; position restored on end).
- New APIs: `TellFX.WallImpact(pos, normal, target, strength, order)`, `TellFX.SpeedLines(target, seconds, lengthPx, widthPx, minSpeedPx, order)`, `TellFX.BossHit(target, hitDir, hitPoint, crit, order)` (max 6 alive).
- New switches: `TellFX.JuiceHitStop`, `TellFX.StunWobble`, `TellFX.ImpactFrames` (default true). New sprite `TellSprites.Birdie[2]`.
- Hit-stop uses `HitStop` (Time.timeScale) and respects `ProjectileFX.HitStopScale`; 2 more cached materials (solid white / black).
- Showcase: Gore Charge shows SpeedLines + WallImpact. Mock-up: `review/v181/oxpit_gameplay_v1.8.1_MOCKUP.mp4`.
## 1.8.0 - 2026-10-25
- Ox Pit visual rework (#101-111), based on research (`research/oxpit_v18_references.md`). API signatures unchanged; still 111 effects.
- Tells (#101 Lane, #102 Cone, #103 Stomp) share one look:
  - a dark outline, a thin wash and a 1px #e43b44 rim;
  - a #fee761 clock front that reaches the impact edge at 85% of the tell, then the rim blinks white twice;
  - a 1-2 frame white flash on release.
- Lane: chevrons and a wind-up glint. Cone: white/gold horn-swing smear band. Stomp: a ring that closes in during the tell; opaque shock rings with dust riding them; ground cracks and pebbles.
- Stun Stars: impact star and bouncing pebbles on the wall hit. Punish Glow: a blinking 1px gold silhouette outline plus 3 countdown pips (falls back to a ring when the target has no SpriteRenderer).
- Stampede: no lane fill. Marching dashed borders, entry chevrons clamped to the camera view, and tremor pebbles; only oxen and dust show during the run.
- Bloodrage: a red outline flicker on the boss, nostril steam, and flame-flipbook hoof patches (scorch, ignite, burn, die down, smoke).
- Burn / Chill: a vertex tint on the target SpriteRenderer (original colour restored) plus 1-2 tiny sprites (flame crown and ember; glint; ice shards when frozen). The v1.7.1 sprite copies are gone. New `StatusFX.TintTargets` and tint colours (`BurnTintA/B`, `ChillTint`, `FrozenTint`).
- Dust: stepped L->M->S puffs with drag (no alpha fade).
- New public members:
  - `TellSprites.FlameBook[4]`, `FlameSmall[4]`, `Glint`, `Pebble`, `Chevron`, `IceShard`, `Scorch`, `Impact` (atlas is now 128x48)
  - `TellFX.Wash`, `Clock`, `ScorchCol`, `Steam`, `DustShade`, `Orange`, `BlinkFrom`
- `TellFXRunner.Slots` raised from 32 to 48, so Prewarm now creates 2304 SpriteRenderers.
- Boss outlines (Punish, Bloodrage) use 2 cached solid-colour clones of `PixelFX_Uber` (`_FlashAmount=1`).
- Status: C# compile-checked against Unity 6000.0.85f1 (0 warnings, warnings treated as errors). NOT run in the Unity Editor. No FPS measured. The preview (`review/v18/oxpit_gameplay_MOCKUP.mp4`) is a Python mock-up.

## 1.7.1 - 2026-10-25
- TellFX enemy telegraph defaults (#101 Lane, #102 Cone, #103 Stomp Rings, #108 Stampede tell) now use Endesga red `#e43b44` for the ground mark (`TellFX.TellRed`) and `#feae34` for the flash core (`TellFX.TellHot`), replacing crimson/fire. No API, sizing or timing changes.
- Status: C# compile-checked against Unity 6 (0 warnings); NOT run in the Unity Editor.
### Visual polish (TellFX)
- Lane / Cone / Stampede telegraphs: dark outline, semi-transparent #e43b44 fill, 2px #feae34 edge that fills toward impact (stampede lanes no longer solid blocks).
- StompRings tell & waves and PunishGlow ring: solid segmented rings, smooth fades (less dotted).
- Dust (#106/#107): smooth alpha fade + drift, no popping.
- BloodrageTrail: varied, flickering fire patches over scorch, ignite/burn/die-down.
- BurnStatus: small rising, cooling flame tongues + embers. ChillStatus: sprite-shaped outline/tint copies of the target SpriteRenderer + frost flecks (no box; Crust fallback if no SpriteRenderer).
- Behaviour: Stampede `color` now tints only the oxen; tell always uses TellRed.
- Mock-up re-rendered on dark hell stone; left-edge caption clipping fixed. Hero sizing unchanged.

## 1.7.0 - 2026-10-25
- Hellord Level 10 "Ox Pit" vertical slice: 11 new effects (111 total), sized for 240x135 art / ~15px hero. In `TellFX` (Runtime/TellFX.cs): #101 Lane Telegraph, #102 Cone Telegraph, #103 Stomp Rings, #104 Stun Stars, #105 Punish-Window Glow, #106 Dust Puff, #107 Dust Trail, #108 Stampede, #109 Bloodrage Trail. In `StatusFX`: #110 Burn Status and #111 Chill Status (with `SetFrozen`).
- `TellFXRunner`: 48 instances x 32 preallocated sprites, one 64x32 atlas, shared materials, no per-spawn allocations. Tells are clamped to 0.5-1 s and use the #69 crimson/fire pair. Hooks are optional (`OnTellFire`, `OnStampedeHit`); the effects are visual only.
- Burn and Chill are pooled overlays that do not touch the target's material, so HordeFX batching is kept. They share `ReactionFXRunner.StatusCap` with Shock and Bleed. New `ReactionFXRunner.SharedStatusAlive`.
- Gallery vignette 15 "Ox Pit". New `HELLORD_FX_PLAN.md`.
- Status: C# compile-checked against Unity 6000.0.85f1 (0 warnings). NOT run in the Unity Editor. No FPS measured. The preview clip is a Python mock-up.
## 1.6.0 - 2026-10-25
- 6 new effects (100 total), sized for 240x135 art / ~15px hero, in `ReactionFX` (Runtime/ReactionFX.cs): #95 Frost Cleave (tintable pooled arc, `OnCleaveHit`), #96 Shock Status, #97 Overload (Burn+Shock fire nova + electric burst, shake/hit-stop via ProjectileFX scales, `OnOverloadHit`), #98 Bleed Status, #99 Frost Shatter, #100 Elite Chest Open.
- `ReactionFXRunner`: 48 instances x 48 preallocated sprites, one 64x32 atlas, shared additive + default materials, no per-spawn allocations. Statuses attach to a Transform, return a handle (`StopStatus`, `IsAlive`) and share `ReactionFXRunner.StatusCap` (32). Statuses do not touch the target's material (horde batching stays intact).
- Gallery vignette 14 "Frost & reactions".
## 1.5.1 - 2026-10-25
- 3 new effects (94 total), sized for 240x135 art / ~15px hero: #92 Chain Lightning Jump (`ChainFX.ChainLightning` auto-targets Collider2Ds, up to 6 hops, hit spark, `OnChainHit`), #93 Chain Whip Sweep (`ChainFX.ChainWhip`), #94 Poison Puddle (`ChainFX.PoisonPuddle` / `StopPuddle`, bubbling loop, `OnPuddleTick`). Pooled by `ChainFXRunner` (fixed slots, preallocated sprites, single atlas, shared materials).
- #62 `HellFX.ChainLightning` now forwards to the pooled ChainFX bolt: no per-call allocations, adds hit sparks (same signature; max 7 points).
- Gallery vignette 13 "Chains & poison".
- Status: C# compile-checked against Unity 6 (0 warnings); NOT run in the Unity Editor; no FPS measured.
## 1.5.0 - 2026-10-25
- 17 new projectile-weapon effects (91 total): fireball, blood bolt, bone shard, soul orb, curse skull, hellfire spear, lightning bolt, homing magic missile, returning blade, piercing beam, bouncing, splitting, cast flash, element trails, element impacts, pierce/bounce/split hit FX, spread/ring/spiral/homing pattern helpers. `ProjectileFX` API, pooled by `ProjectileFXRunner` (1024 preallocated projectiles, no per-spawn allocation, shared materials).
- Gallery vignette 12 "Hellord projectiles".
- Status: C# compile-checked against Unity 6 (0 warnings); NOT run in the Unity Editor; no FPS measured.
## 1.4.1 - 2026-10-24
- Hell pickups replace XP gems in the hell kit: `HellFX.BloodDrop` (small/medium/large blood drops with glossy highlight, splat + drips on pickup), `HellFX.BonePickup` (bone shard / skull), `HellFX.BloodStream` absorb stream, `HellFX.Pickup` for custom sprites. `XPGem` stays as a generic gem preset for non-hell games.
- New `HellPalette.BloodGradient()` / `BoneGradient()`; new sprites `HellSprites.BloodDrop(tier)` / `Bone(big)`.
- Stress scene and gallery vignette 11 now drop blood and bones and use blood streams.
- Status unchanged: C# compile-checked against Unity 6 (0 warnings); NOT run in the Unity Editor; no FPS measured.
## 1.4.0 - 2026-10-24
- 20 new "hell survivor" effects for top-down horde games (74 total): XP gems, magnet pull, level-up burst, choice glow, orbit weapons, aura field, projectile trails, chain lightning, hellfire nova, pentagram rune circle, meteor rain, lava ground, soul stream, spawn portals, boss telegraph, crit pop, kill-streak pop, demon evolution, heat distortion, hell palette. `HellFX` static API, pooled by `HellFXRunner` (600 sprites).
- Horde toolkit: `HordeFX` batched hit flash and dissolve (shared materials, no MaterialPropertyBlocks), pooled damage numbers on one glyph atlas, cheap 5-pixel death burst; `PixelParticles` auto particle budget.
- New `Demo/HordeStressTest.unity`: 400+ enemies dying with effects, FPS / min-FPS readout, toggles.
- Gallery: power-up vignette now has a real star pickup; waterside slime is clearly visible (outline, bubbles); windy meadow uses dense tall grass with stronger rolling gusts; new vignette 11 "Hellord horde".
- Status: C# compile-checked against Unity 6 assemblies (0 warnings). NOT run in the Unity Editor; shaders not compiled in Unity; no FPS has been measured yet.
## 1.3.1 - 2026-10-24
- Gallery mode (G) rebuilt as 10 animated gameplay vignettes (loot pickup, slime combo, dash finisher, status effects, power-up, waterside, windy meadow camp, ruins teleport, boss fight, game over). Together they use all 54 effects, and each vignette lists its effects on screen. Left/Right steps between vignettes.
- New code-made knight animation frames (idle, run x4, jump, fall, attack x3, hurt, dead) plus grass, gem and crown props in Demo/PixelFXAnimFrames.cs. Hand-placed rows and a procedural sword; no AI art.
- No runtime or shader changes.
- Status: C# compile-checked against Unity 6 assemblies; NOT run in the Unity Editor. Shaders still not compiled in Unity.
## 1.3.0 - 2026-10-24
- 21 new effects (48 effects + 6 particle presets total, listed in EFFECTS.md).
- Sprite shader: wind sway, glitch, motion blur, chromatic aberration, hologram, frozen, burn, tint pulse, gradient map, greyscale, sepia, negative, rainbow, hue shift, dither fade. Shader target raised to 3.5.
- New full-screen shader PixelFX/Screen FX (URP): screen shockwave distortion, screen chromatic aberration, heat haze, driven by `PixelScreenFX`. Material in Resources/PixelFX/PixelFX_ScreenFX.mat.
- New components/helpers: InvincibilityBlink, WaterReflection, SquashStretch, DamageNumbers (3x5 pixel font), PixelVFX (Lightning, TeleportBeam, LevelUp, Heal, BuffAura).
- Showcase: characters 2x larger, slash arc resized to match, new gallery mode (G) that cycles every effect with a numbered label.
- Status: C# compile-checked against Unity 6 assemblies; shaders NOT yet compiled or run in the Unity Editor.
## 1.2.0 - 2026-10-24
- New showcase scene (Demo/PixelFXShowcase.unity) replaces the card grid: night ruins with parallax, dithered sky, moon and torch glow, embers; knight dashes (afterimages), slashes (slash arc, hit-stop, impact frame, white flash, glow sparks, shockwave, shake + kick, knockback), finisher (freeze + slow-mo, glowing dissolve with embers, coin burst with sparkle). Auto-loops; keys/buttons trigger moments.
- Game-feel kit: HitStop, ScreenShake (whole-pixel, accessibility intensity), ImpactFrame, AfterimageTrail, PixelGlow, PixelFXSpawn (Slash, Shockwave, LightPop), PixelFXTextures (procedural glow/ring/slash).
- Shader: rim light, emission, dissolve edge glow. New PixelFX/Additive shader + Resources material.
- Particles: additive option, sway, GlowSparks() and Embers() presets.
- Status: C# compile-checked against Unity 6 assemblies; NOT yet run in the Unity Editor.
## 1.1.0 - 2026-10-24
- FIX: effects were invisible in player builds (WebGL etc.): the Uber shader was stripped because it was only loaded via Shader.Find. Added Resources/PixelFX/PixelFX_Uber.mat (always included) and PixelFXMaterials now loads it first.
- FIX: ShineSweep.Play() stopped the automatic loop. FIX: procedural sprites now pivot on a whole texel (odd-sized sprites sat half a pixel off-grid).
- New showcase demo: hand-placed 16px knight/slime/coin (Endesga 32 palette, no AI art), pixel-perfect camera with integer zoom (480x270 ref), card layout, auto-playing previews, idle bob, hit shake, click-anywhere explosions.
- Juicier particle presets + new Sparkle preset; PixelSpriteFactory.FromPixels().
- Status: C# compile-checked; still needs an Editor/WebGL check of v1.1.0.
## 1.0.0 - 2026-10-24
- First release: Sprite Uber shader (outline outer/inner, hit flash, pixel dissolve, palette LUT, silhouette, wave, shine, pixelate, drop shadow), CRT full-screen (URP), C# helpers, pooled pixel particles (hit sparks, dust, explosion), code-built demo, Godot 4 port (bonus, untested).
- Status: C# compile-checked; shaders not yet verified in the Unity Editor.

**Hook Chain review pass (1.8.9):** new `Dormant` phase. After Retract, Snap or Release the hook hangs from its anchor for `DefaultCooldown` (4 s by default; set it per game) with 8 orange refill ticks, blinks white in its last 0.5 s as a re-arm warning, then shows a ready glint, then the slot frees and `OnReady(handle)` fires. Query it with `IsDormant(h)`. The chain's weak middle link is now always marked in gold and flickers faster with each break pip. Chain links get a 1px highlight so they read on red cave floors and inside 100+ enemy hordes.
