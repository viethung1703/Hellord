# Changelog
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
