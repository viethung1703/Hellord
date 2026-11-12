# Pixel FX Pack v1.4.1 (Unity 6, 2D)

**48 effects + 6 pixel particle presets** - full list with one-line descriptions in `EFFECTS.md`.

Pixel-perfect sprite effects + tiny pixel particles. Code-only, no art dependencies; every demo sprite is generated from code.

## Status - please read
- C# scripts: compile-checked against Unity 6000.0 engine assemblies outside the Editor (0 errors, 0 warnings).
- Shaders: hand-written HLSL, **NOT yet compiled or viewed in the Unity Editor** as of v1.4.1. Treat as preview until a later update says "Editor-verified".
- Godot 4 port: **not run in Godot**, and it only covers the v1.0 sprite + CRT shaders (none of the v1.2/v1.3 additions).

## Requirements
- Unity 6000.0+. URP (2D Renderer or Universal Renderer) recommended. The Sprite Uber shader is pipeline-agnostic (UnityCG) and should also run in Built-in.
- The Sprite Uber shader targets shader model 3.5 (since v1.3; very old GLES2-only devices are not supported).
- The CRT and Screen FX shaders need URP 17 (Unity 6) Full Screen Pass Renderer Feature.

## Quick start
1. Import the package. Open `Demo/PixelFXShowcase.unity` and press Play: a looping knight-vs-slime moment shows every effect. Keys: 1 dash, 2 slash, 3 finisher, Space auto-loop, S shake on/off, **G gallery** (15 animated gameplay vignettes that together use all 111 effects, each labelled with its effects; Left/Right to step). (The v1.1 card grid is still available: add `PixelFXDemo` to an empty object.)
2. On your own sprite: add **Pixel FX (Sprite)** (`PixelFX`). It assigns a shared material with `PixelFX/Sprite Uber` automatically.
3. Add helpers as needed and call them from code:
```csharp
GetComponent<PixelFXPack.HitFlash>().Play();
GetComponent<PixelFXPack.DissolveFX>().PlayOut();
GetComponent<PixelFXPack.ShineSweep>().Play();
GetComponent<PixelFXPack.PaletteSwap>().Set(1);
PixelFXPack.PixelParticles.Burst(PixelFXPack.PixelBurstSettings.Explosion(), transform.position);
```

## Sprite import settings (important for pixel-perfect results)
Filter Mode **Point**, Compression **None**, Mip Maps **off**, Mesh Type **Full Rect**.
Outer outline and drop shadow draw only inside the sprite rect: leave 1-4 px of transparent padding around each sprite (or atlas padding >= outline size).

## Effects (all in one shader, all off by default, stackable)
| Effect | PixelFX fields | Notes |
|---|---|---|
| Outline outer/inner | outlineSize (px), innerOutline, outlineDiagonals | 4-way or 8-way |
| Hit flash | flashAmount, flashColor | `HitFlash.Play()` with blinks |
| Dissolve (pixel step) | dissolveAmount, dissolveCell, edge color/width | `DissolveFX.PlayOut/PlayIn`, stepped |
| Palette swap (LUT) | paletteLUT, paletteRow | LUT: width = colors (<=16), row 0 at TOP = source colors. Import LUT Point/None/no mips |
| Silhouette | silhouetteAmount, silhouetteColor | flat color fill |
| See-through | `SeeThrough` component | walls need a SpriteMask; silhouette shows inside masks |
| Wave / wobble | waveAmplitude (px), frequency, speed | whole-texel steps |
| Shine sweep | shinePosition, width, slant | `ShineSweep` auto-loops |
| Pixelate | pixelateSize | block size in texels |
| Drop shadow | shadowOffset (px), shadowColor | needs padding |
| CRT / scanlines | `PixelFX/CRT Fullscreen (URP)` | full-screen, see below |

## CRT setup (URP)
1. Create a material from `PixelFX/CRT Fullscreen (URP)`.
2. Select your Renderer asset (e.g. Renderer2D) > Add Renderer Feature > **Full Screen Pass Renderer Feature**.
3. Pass Material = your CRT material. Injection Point: After Rendering Post Processing.

## Game-feel kit (v1.2)
```csharp
using PixelFXPack;
HitStop.Play(0.06f);                       // hit pause; HitStop.Play(0.11f, 0.3f, 0.4f) = freeze, then slow-mo ease back
ScreenShake.Add(3, 0.2f);                  // whole-pixel shake (put ScreenShake on the camera; intensity 0 = accessibility off)
ScreenShake.Main.Kick(new Vector2(2, 0));  // directional camera kick
ImpactFrame.Play(Camera.main, 0.04f, attackerFX, targetFX); // black frame, white silhouettes
PixelFXSpawn.Slash(pos);                   // procedural 4-frame slash arc (additive)
PixelFXSpawn.Shockwave(pos, 24);           // expanding glow ring
PixelFXSpawn.LightPop(pos, Color.white);   // short light flash
PixelParticles.Burst(PixelBurstSettings.GlowSparks(), pos); // additive sparks; also Embers()
```
- `AfterimageTrail` (set `emitting = true` while dashing), `PixelGlow` (banded, dithered fake light with flicker; no 2D lights needed).
- `PixelFX` new fields: `rimAmount/rimColor/rimDirection` (pixel rim light), `emissionAmount/emissionColor`, `dissolveEdgeGlow` (pair with URP HDR + Bloom for real glow).
- New shader `PixelFX/Additive` (shipped via `Resources/PixelFX/PixelFX_Additive.mat`).

## v1.3 additions
New `PixelFX` fields (all 0 = off): `windAmount/windSpeed`, `glitchAmount`, `motionBlur/motionBlurDirection`, `chromaticOffset`,
`hologramAmount/hologramColor`, `frozenAmount`, `burnAmount/burnColor`, `pulseAmount/pulseColor/pulseSpeed`,
`gradientMapAmount/gradientDark/Mid/Light`, `greyscale`, `sepia`, `negative`, `rainbow/rainbowSpeed`, `hueShift`, `ditherFade`.
```csharp
using PixelFXPack;
PixelScreenFX.Shockwave(Camera.main, hitPos);          // real screen distortion (needs Screen FX pass, below)
PixelScreenFX.ChromaticPulse(3, 0.25f);
PixelScreenFX.SetHeatHaze(new Rect(0.2f, 0, 0.3f, 0.4f), 2f); // viewport rect, amplitude in screen px; 0 = off
GetComponent<InvincibilityBlink>().Play(1.5f);
GetComponent<SquashStretch>().Punch(-0.3f);           // put on the feet-pivot root
DamageNumbers.Spawn(pos, 42, new Color32(255,255,255,255), crit: true);
PixelVFX.Lightning(from, to, Color.cyan);
PixelVFX.TeleportBeam(feet, Color.cyan);  PixelVFX.LevelUp(feet);  PixelVFX.Heal(feet);
var aura = PixelVFX.BuffAura(transform, Color.yellow); // Destroy(aura) to stop
```
`WaterReflection`: add to a SpriteRenderer with a feet pivot, set `waterLineY`.

### Screen FX setup (URP)
Renderer asset > Add Renderer Feature > **Full Screen Pass Renderer Feature** > Pass Material = `Resources/PixelFX/PixelFX_ScreenFX.mat`,
Requirements = Color. It costs one full-screen pass and does nothing until `PixelScreenFX` calls set its values. You can stack it with the CRT feature.
Optional: `PixelScreenFX.SetPixelSnap(scale)` quantises distortion to your pixel scale for a chunkier look.

## Pixel particles
`PixelParticles` is a pooled, pixel-snapped SpriteRenderer particle manager (auto-created on first use). Presets: `HitSparks()`, `Dust()`, `Explosion()`, or tweak a `PixelBurstSettings`. `PixelBurstEmitter` lets you fire one from a UnityEvent. Set `pixelsPerUnit` to your sprites' PPU.

## Builds
`PixelFX` uses `Shader.Find`. For player builds add `PixelFX/Sprite Uber` to Project Settings > Graphics > Always Included Shaders, or assign a material using it to your sprites (then nothing is looked up).

## Builds (WebGL, mobile)
The shader ships via `Resources/PixelFX/PixelFX_Uber.mat`. Keep that file (or assign a material using "PixelFX/Sprite Uber" to your sprites); otherwise Unity strips the shader from builds and effects disappear.

## Hell survivor / horde games (v1.4)
Made for Vampire Survivors-style games (top-down / three-quarter, hundreds of enemies). Quick use:
```csharp
HordeFX.Prepare(enemySr);                    // once per pooled enemy: shared batched material
HordeFX.Hit(enemySr, dmg, crit);             // flash + pooled number
HordeFX.Kill(enemySr, sr => pool.Return(sr)); // batched dissolve + cheap burst, then back to your pool
var drop = HellFX.BloodDrop(pos, tier);   // or HellFX.BonePickup(pos, big), or HellFX.XPGem(pos) for non-hell games
pickups.Add(drop);  HellFX.MagnetAll(pickups, player, 48);  HellFX.BloodStream(pos, player);
HellFX.HellfireNova(player.position, 40);  HellFX.ChainLightning(points, HellPalette.Soul);
HellFX.DemonEvolve(playerFx, () => sr.sprite = lordSprite);
```
Full list with numbers 55-74 in EFFECTS.md.

### v1.5 projectile weapons
`ProjectileFX.Fireball(pos, dir)`, `BoneShard`, `SoulOrb`, `CurseSkull`, `HellfireSpear`, `LightningBolt`, `MagicMissile`, `ReturningBlade`, `Beam`, `Bouncing`, `Splitting`, plus `Spread` / `Ring` / `SpiralStep` / `HomingVolley`. Set `ProjectileFX.OnHit = (collider, damage, element) => ...` for damage and `ProjectileFX.FindTarget` for homing/bounce. Enemies need a Collider2D. Numbers 75-91 in EFFECTS.md.

**v1.5.1 chains & poison:** `ChainFX.ChainLightning(origin, jumps, rangePx)` (jump chain + hit spark, `OnChainHit`), `ChainFX.ChainWhip(origin, fromDeg, toDeg, lengthPx)`, `ChainFX.PoisonPuddle(pos, radiusPx, seconds)` / `StopPuddle(handle)`. Numbers 92-94. Open `Demo/HordeStressTest.unity` to stress test.

**v1.6.0 frost & reactions:** `ReactionFX.FrostCleave(origin, dirDeg, radiusPx)` (`OnCleaveHit`), `ReactionFX.ShockStatus(target, seconds)` / `BleedStatus(target, seconds)` (handle + `StopStatus(handle)`, shared `ReactionFXRunner.StatusCap`), `ReactionFX.Overload(pos, radiusPx)` (`OnOverloadHit`), `ReactionFX.FrostShatter(pos, shards)`, `ReactionFX.EliteChestOpen(pos)`. Numbers 95-100. Call `ReactionFXRunner.I.Prewarm()` at load.

**v1.8.3:** damage numbers merge per burst and never overlap.

**v1.8.2:** smaller impact frames and stomp star; damage numbers merge per target and stagger (`HordeFX.numberMergeWindow`, `numberStackPx`).

**v1.8.1 juice pass:** hit-stop, directional kicks, impact frames, juicier stun, new `WallImpact` / `SpeedLines` / `BossHit` (see CHANGELOG). Set `TellFX.JuiceHitStop=false` if your game owns `Time.timeScale`.

**v1.8.0 Ox Pit rework:** same API, new research-driven looks (see `research/oxpit_v18_references.md`). Burn/Chill now tint the target SpriteRenderer (set `StatusFX.TintTargets = false` if your code drives `SpriteRenderer.color`).

**v1.7.0 Ox Pit slice:** `TellFX.LaneTelegraph(from, to, widthPx, seconds)`, `ConeTelegraph(origin, dirDeg, arcDeg, rangePx, seconds)`, `StompRings(pos, rings, maxRadiusPx, tellSeconds)` (`OnTellFire(handle, index)`), `StunStars(target, seconds)`, `PunishGlow(target, seconds)`, `DustPuff(pos)`, `DustTrail(target, seconds)`, `Stampede(from, to, widthPx, tellSeconds, oxen)` (`OnStampedeHit`), `BloodrageTrail(target, seconds)`, plus `TellFX.Stop` / `IsAlive`. Also `StatusFX.BurnStatus(target, seconds)`, `ChillStatus(target, seconds, frozen)`, `SetFrozen`, `StopStatus` (these share StatusCap). Numbers 101-111. Call `TellFXRunner.I.Prewarm()` at load.

## Performance (v1.4) - honest numbers
**No FPS has been measured yet; the pack has not been run in Unity.** The numbers below are design targets/estimates, not results. Profile a development build on your target device with the stress scene before you rely on them.
- Design: enemies sharing one sprite texture + `HordeFX` materials should batch into a few draw calls; hit flash/dissolve swap shared materials instead of using MaterialPropertyBlocks; numbers use one atlas; one Update loop, no per-enemy components.
- Expected (estimate): mid-range desktop 300-500 enemies with effects at 60 FPS; mid-range phone around 200-300 at 60 FPS; WebGL lower. Particle-heavy moments (nova + meteors) are the most likely drops.
- Budgets: `HordeFX.maxNumbers` 48, `maxDissolves` 160, `HellFXRunner.maxSprites` 600, `PixelParticles.autoBudget` thins emits above 60% of max.
- Tips: put all enemy frames in ONE texture/atlas (separate textures break batching); keep Screen FX (heat haze, shockwave) optional on low-end; cap pickups (the stress scene caps them at 150).

## Known limits (v1.4.0)
- v1.6: ReactionFX has not been run in the Unity Editor yet (compile-checked against 6000.0.85f1 DLLs only). Prewarm creates 2304 inactive SpriteRenderers. Rotated stretched strips (cleave band, bolts, light column) are not perfectly pixel-grid aligned. Cleave hits max 16 colliders per swing, only while sweeping; Overload checks at most 32 colliders once, at call time. Over StatusCap or when all 48 slots are busy, effects return false/-1 and are skipped. EliteChestOpen does not hide your chest sprite. The v1.6 preview clip is a Python mock-up.
- v1.8.1: WallImpact, Stomp and the cone set `Time.timeScale` briefly (via `HitStop`); disable with `TellFX.JuiceHitStop`. Stun wobble moves a child SpriteRenderer's localPosition (restored on end). 2 more cached materials.
- v1.8: Prewarm creates 2304 inactive SpriteRenderers (48 x 48). Burn/Chill write `SpriteRenderer.color` while active and restore it on end; only one status tints a target at a time. PunishGlow/BloodrageTrail outlines use 2 extra cached materials (boss only). Not run in the Editor.
- v1.7: TellFX/StatusFX have not been run in the Unity Editor (compile-checked only). Prewarm creates 1536 inactive SpriteRenderers. Tell times are clamped to 0.5-1 s. Rotated lane and cone strips are not perfectly pixel-grid aligned. The stampede ox is a placeholder silhouette. `OnStampedeHit` is a convenience overlap check (max 16 colliders per stampede); game code should do the real hit tests. Over StatusCap, or when all 48 slots are busy, calls return -1/false. The v1.7 preview clip is a Python mock-up.
- v1.4: batched dissolve uses 8 shared steps, so it looks slightly steppier than the per-sprite dissolve. Spawning effects with callbacks allocates small closures (GC). `PixelSpriteFactory.FromRows` makes one texture per sprite, which does NOT batch; pack enemies into one atlas. Heat Distortion and the nova screen shockwave need the Screen FX full-screen pass. The horde preview video is a Python mock-up, not a Unity capture.
- Unlit only: does not receive URP 2D Lights. Lighting in the showcase is faked with additive `PixelGlow` sprites (works everywhere). Emission/edge-glow values above 1 only bloom if the camera has HDR + a Bloom volume.
- `PixelFXSpawn.Shockwave` is a glow ring. The real distortion version is `PixelScreenFX.Shockwave` and needs the Screen FX full-screen pass.
- Burn assumes a non-atlased sprite (UVs 0..1); in an atlas the burn front follows atlas UVs.
- Wind Sway moves vertices: with sprite atlases / Tight mesh it still works but sway strength follows UV height.
- `HitStop` changes `Time.timeScale`; scripts that must keep running during hit-stop should use unscaled time.
- The v1.2/v1.3 preview GIF/MP4 files are Python mock-ups of the intended look, not a Unity capture.
- Uses MaterialPropertyBlocks: per-sprite values, but SRP Batcher does not batch these sprites (fine for typical 2D counts).
- No Shader Graph versions, no UI (Image) support, no custom material inspector yet.
- Palette match is nearest-color within tolerance, max 16 colors per row.
- Tilemaps: use the shader on a TilemapRenderer material; PixelFX component is for SpriteRenderer only.

## AI disclosure
Code and documentation were written with AI assistance and reviewed by the publisher. No AI-generated art or audio: all visuals are generated by code at runtime.

Support: see the store page. License: see LICENSE.txt.
