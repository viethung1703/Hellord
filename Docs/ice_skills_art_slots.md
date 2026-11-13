# Ice skills (t1393u): art / VFX slots

Skills: Frostbolt (id 12), Ice Nova (13), Frost Bomb (14), Hurricane (15). Code: `Assets/Hellord/Scripts/Runtime/HellordIceSkills.cs`
(the whole "ICE ART & VFX SLOTS" region + `IceArt` class at the top). Tuning: `HellordTuning` block "Ice skills (t1393u)".

## Hellord Art (HA): icons
`Assets/Hellord/Resources/HellordArt/IceSkills/` (PNG, point filter, no compression, same import settings as `ui_k98_*.png`)
| file | size | notes |
|---|---|---|
| icon_frostbolt_24.png | 24x24 | framed tile in the style of `ui_v51k_skill_icons_24` (dark outline, light top-left edge, fill #2a64a8) |
| icon_icenova_24.png | 24x24 | same frame |
| icon_frostbomb_24.png | 24x24 | same frame |
| icon_hurricane_24.png | 24x24 | same frame |
Placeholders are already there (drawn by hand in Endesga 32 blues). Overwrite the files; nothing else to change. Cards, skill bar and support-target pick draw them at 1x (bar) and 2x (cards).

## Hellord FX (HFX): fixed-size strips (optional, drop in and they replace the placeholder automatically)
Horizontal strips, frames left to right, 16 px per world unit, never rescaled. Missing file = placeholder.
| file | frame | frames | fps | loop | what |
|---|---|---|---|---|---|
| fx_frostbolt_orb.png | 16x16 | 4 | 12 | yes | orb in flight, round (drawn unrotated), centred |
| fx_frostbolt_burst.png | 48x48 | 6 | 16 | once | burst on wall / end of life, centred |
| fx_chill_ground.png | 32x32 | 3 | 6 | yes | flat chill patch left behind by the orb (ground layer) |
| fx_frostbomb_crystal.png | 24x32 | 4 | 8 | yes | crystal while it ticks; pivot = bottom centre (feet) |

## Hellord FX (HFX): scalable effects = code hooks (they get the LIVE radius, so no pixel art is ever resampled)
Edit these methods in `HellordIceSkills.cs`, same style as `HFX.FrostCleave` / `FrostCleave990`:
- `IceVfxNovaCast(c, rMax)` once; `IceVfxNova(c, r, rMax, t01)` every frame while the ring expands (0.38 s, r 3.2-4.5 u).
- `IceVfxBombDrop(p)` once; `IceVfxBombCrystal(p, life01, age)` every frame (2 s); `IceVfxBombPulse(p, r)` every 0.5 s; `IceVfxBombBurst(p, r)` once (r 2.0-2.9 u).
- `IceVfxHurricaneStart(r, secs)` once (returns a HellFXRunner.Job or null); `IceVfxHurricane(c, r, age, fade)` every frame (4-6 s, r 2.4-3.4 u, follows the hero).
- `IceVfxCast`, `IceVfxOrbHit`, `IceVfxOrbBurst`, `IceVfxChillGround`, `IceVfxOrb` for the Frostbolt pieces if a strip is not enough.
Palette used by the placeholders: Endesga 32 #124e89 #0099db #2ce8f5 #c0cbdc #ffffff on #181425.

## Shared status (reusable)
`IceChill(enemy, seconds, build)` / `IceFreeze(enemy, seconds)`: chill = x0.65 move speed + cool tint (existing rule), duration refreshes;
`build` stacks, at `iceFreezeAt` (3) the demon freezes `iceFreezeSeconds` (0.9 s; bosses 40%), then needs `iceFreezeGraceBuild` (2) more before it can freeze again.
Frozen demons that die shatter (existing 35% max-HP blast).
