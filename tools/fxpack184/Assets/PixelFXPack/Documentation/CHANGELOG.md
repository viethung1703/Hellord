# Changelog
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
