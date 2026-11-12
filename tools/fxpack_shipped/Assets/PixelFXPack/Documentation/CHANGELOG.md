# Changelog
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
