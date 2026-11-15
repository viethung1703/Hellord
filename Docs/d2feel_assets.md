# D2 hit / death feel: asset slots (t1434)

All hooks are optional. Every lookup returns "none" when the file is missing and the game falls back to the old visual/sound (or a tiny code placeholder). Everything sits behind `HellordTuning.d2feel1434` (master) and `d2Hooks1434` (assets); per-feature toggles are listed in the Tuning tooltips (`d2Stagger1434`, `d2Knock1434`, `d2Flash1434`, `d2Death1434`, `d2Overkill1434`, `d2Frozen1434`, `d2NoHitShake1434`). Code: `Scripts/Runtime/HellordD2Feel1434.cs`.

`<key>` = sheet name without `enemy_` / `PLACEHOLDER_`: `imp_v2` (Imp), `shambler_v2` (Ghoul), `brute`, `wraith_v2` (Elite), `splitter`, `split_child`, `hook_thrower`, `gossip_imp`. Direction suffix `{south,east,north,west}` = the walk rows 0/4/8/12 (facing at the moment of the hit/death). A file without the suffix is used for all four directions. Sprites load from `Resources/HellordArt/`, point filter, 16 PPU, pivot = bottom centre (x = floor(w/2), 1 px above the bottom row = the feet point).

## Art (`Assets/Hellord/Resources/HellordArt/`)
| File | Format | Used for | Status |
|---|---|---|---|
| `Enemies/<key>_stagger_<dir>.png` | horizontal strip, **2 frames**, same cell as the enemy sheet (e.g. imp 62x38), plays once over the stagger time (0.20-0.24 s, about 10 fps) | stagger pose; missing = recoil offset + squash | delivered for all 8 |
| `Enemies/<key>_death_<dir>.png` | strip, **3 frames @ 12 fps** (`d2DeathFps1434`), frame may be wider than the standing frame, last frame = corpse pose | replaces the old 8-frame ember death when the toggle is on; missing = old frames trimmed to 0.3 s (`d2DeathS1434`) | delivered for all 8 |
| `Enemies/<key>_corpse_<dir>.png` (blobs: `splitter_corpse.png`, `split_child_corpse.png`) | single image, any size | corpse that stays 3-5 s then fades; missing = last death frame | delivered for all 8 |
| `BloodFX/blood_pool_s.png` (28x14), `BloodFX/blood_pool_m.png` (40x20) | single image, centre pivot, drawn about 3 px above the feet, under the corpse | `_m` for Brute + Splitter, `_s` for the rest, **none for the wraith (Elite)**. Per-enemy override: `BloodFX/pool_<key>.png`. Missing = a small dark code pool (only when there is no corpse art either, `d2Placeholders1434`) | delivered |
| `BloodFX/blood_hit_v1..v3_strip.png` | 24x20, 8 frames, pivot (6,10) on the chest (= feet + `hitY`), 30 fps, base direction east, flipX when the hit comes from the east side | blood spurt on **every** non-DoT, non-killing hit (random variant) | delivered (FX t1435) |
| `BloodFX/hit_spark_strip.png` | 22x22, 6 frames, pivot (6,11), 30 fps | on a stagger (replaces the 3-pixel placeholder) | delivered |
| `BloodFX/blood_death_v1..v3_strip.png` | 56x44, 12 frames, pivot (20,18) chest, 30 fps; the last frame is held on the ground (under the corpse) for the corpse life, then fades | kill spray (normal kills) | delivered |
| `BloodFX/blood_decal_v1..v3_strip.png` | 40x20, 6 stages, pivot (20,10) on the floor point. Stage 0 at landing, 1 after 1.5 s, then 2-5 in equal steps over the corpse fade (0.25 s each), no loop | `d2DecalMode1434`: 0 = HA pool only, **1 = decal replaces the pool (default)**, 2 = both. None for the wraith | delivered |
| `BloodFX/frozen_shatter2_strip.png` | 72x72, 21 frames, pivot (36,36) chest, 30 fps. Frames 0-1 flash, 2-5 statue (code tints the pose 60% toward c0cbdc), enemy sprite hidden at frame 6, 6-20 mist + shards | frozen kill; replaces `HFX.Shatter` while present (the shatter blast damage is unchanged) | delivered |
| `BloodFX/gib_burst_strip.png` | 72x56, 14 frames, pivot (36,26); last frame held on the ground for the corpse life | overkill >= `d2GibMul1434` (4x remaining HP), non-elite: body hidden at frame 0, no corpse, decal left, plays with `gore_splat` | delivered |
| `BloodFX/burn_death_strip.png` | 48x56, 16 frames, pivot (24,36) | kill while burning (`burnT > 0` or the lethal hit was a Burn skill / Burn support); played over the normal collapse, corpse tinted 55% toward charred; replaces the blood_death spray | delivered |

Each FX name is looked up once; a missing file or a wrong size (logged once) just disables that effect. The sizes/pivots are the `FxDefs` table in `HellordD2Feel1434.cs`.

Not covered (keep their own hurt/death rows or old death): Frost Caster, Fire Wheel, Bloater, Rice Ghost, Grinder Monk (variant >= 3 / mortar `m53`), bosses, nests. Hook Thrower and Gossip Imp get death/corpse/pool art but **no stagger** (their AI moves/telegraphs through their own code).

## Sound (`Assets/Hellord/Resources/Audio/SFX/hitdeath/`, hook table in `HellordAudio.Awake`)
Files `<hook>_01 ... _09.wav` (random variant, never the same twice in a row, pitch jitter). Wired for the 34-file `d2feel_t1434` pack:

| Hook | When | Vol | Pitch | Cap | Gap |
|---|---|---|---|---|---|
| `impact_flesh` | every non-DoT hit incl. the killing hit (replaces `enemy_hit`) | 0.62-0.78 | 0.92-1.08 | 6 | 35 ms |
| `pain_imp` (Imp, Gossip Imp) / `pain_ghoul` (Ghoul, Splitter, SplitChild) / `pain_brute` (Brute, Hook Thrower) / `pain_wraith` (Elite) | on a stagger only; never on the killing blow, frozen or DoT | 0.5-0.6 | 0.94-1.06 | 3 per hook | 100 ms |
| `death_scream` | death start (replaces `enemy_death`; elites also keep `elite_death`) | 0.78-0.9 | 0.94-1.06 | 4 | 50 ms |
| `gore_splat` | on top of the scream for an overkill / crit kill | 0.62-0.76 | 0.92-1.10 | 3 | 60 ms |
| `ice_shatter` | frozen kill, at the shatter moment (no scream, no gore) | 0.72-0.85 | 0.95-1.05 | 3 | 60 ms |

Fallback: if a hook has no clips, `impact_flesh` falls back to `enemy_hit` and `death_scream` to `enemy_death`; the others are simply silent. Toggle off = the old sounds.

## Behaviour summary (all values in HellordTuning, `d2*1434`)
Stagger when damage >= maxHP/16: 50% at 1-2x, 75% at 2-4x, 100% at 4x+ (elites: chance x0.5, duration x0.7); 0.20-0.24 s no move/attack/contact damage; per-enemy 0.2 s cooldown after it ends. Knockback 8-16 px by hit size, ease-out over 0.18 s (elites x0.35). Never when frozen, on the killing blow, DoT, bosses. Hit flash 33 ms (global `HordeFX.flashSeconds`). Overkill (dmg >= 2x remaining HP, or a crit kill) slides the body 24-40 px over 0.26 s. Corpses: 3-5 s, 1 s fade, cap 60 (oldest fades first, hard cap 76), cleared on room change / run reset. Frozen kill: 2 flash frames, 4 statue frames (40 ms each), then the existing shatter; no death anim, no corpse. No shake / big-hit hitstop on normal hits; elite kill = 0.05 s hitstop (no slow-mo, no shake), boss kill = 0.06 s hitstop.

## PixelLab hit / death (t1446, `d2anim1446`, imp_v2 + brute only)
Files `Enemies/<key>_{hit,death,corpse}_{south,east,north,west}_pl.png` (AI-generated, VH's one-off exception; source `Art/d2feel_t1444_pixellab/NOTES.txt`). Hit: 4 frames @ `d2HitFps1446` (15), pivot (w/2, h-1); played on stagger (the stagger is stretched to >= 0.267 s so the whole anim shows). Death: 7 frames @ `d2DeathFps1446` (20). **POP = frame 2**: white silhouette flash (HordeFX Flash material swap, re-armed during the frame), and `blood_death` / `gib_burst` / `burn_death`, `death_scream`, `gore_splat` fire there, not at the kill. **Slam = frame 4**: blood pool / decal appears. Frame 6 = corpse pose; the `_corpse_*_pl` image takes over after the anim (pivots: brute E 21.5, N 22.5, W 24.5, the rest w/2). Gib: body hidden after the POP frame, decal at the POP, no corpse. Other enemies keep the d2feel (HA) art. Toggle off = the d2feel collapse.

## t1447: persistent corpses and per-family voices

- `d2CorpsePersist1447` (default on): corpses, landed blood and pools stay for the whole floor instead of fading. Decals stop at the dried stage. `d2CorpseCap1447` (default 400): the cap for corpses and, separately, for landed-blood sprites. Only the oldest beyond the cap are removed (a 0.4 s fade if visible, instantly if from another room). Corpses are tagged per room (`FloorIdx*1000 + room`): other rooms are hidden, other floors are dropped, `ResetRun` clears everything. Sort order stays under the living enemies and nothing collides.
- `d2sfx1447` (default **on** since t1457; was off): per-family `death_<imp|ghoul|brute|wraith|caster>_NN`, `attack_<family>_NN`, `aggro_<family>_NN` hooks, folder `Resources/Audio/SFX/d2sfx1447/` (no clips committed yet; hooks stay silent and the t1434 `death_scream` set plays). Death: shared cap 4, overlap allowed. Attack: cap 6 per family, the newest replaces the oldest of that family, 300 ms retrigger, brute-type voices 65%. Aggro: shared cap 3, per-unit timer (Imp 4-6 s, most 5-7 s, Ghoul / Grinder Monk 10-14 s, Fire Wheel 8-12 s, bosses 18-28 s). Families: imp, ghoul, brute, wraith, frost_caster, fire_wheel, bloater, rice_ghost, grinder_monk, oxhead, horseface, icegorilla, qilin.
- Audit of every demon's art: `Docs/demon_art_audit_t1447.md`.

### t1449 recorded-vocal voices (CC0, trial)

`Resources/Audio/SFX/d2sfx1447/` now holds the 48 t1449 WAVs: `pain_/attack_/aggro_` for imp, ghoul, brute and `death_` for imp, ghoul, brute, wraith, caster. With `d2sfx1447` on, `pain_<imp|ghoul|brute>` plays through the hook `pain1447_<fam>` (instead of the t1434 `pain_*`; other families keep the old pain), and the attack / aggro / death hooks play these clips. Families without clips (wraith / caster attack and aggro, bloater, rice ghost, grinder monk, fire wheel, bosses) stay silent for attack / aggro and use the t1434 / mapped death sounds. t1434 impact, gore and ice are untouched. The toggle stays **off** by default. Sources and licences: `Docs/d2sfx_licenses_t1449.md`. The raw `_sources/` stay in the Art repo (not committed).
Volumes (before `sfxVolume`): death 0.9-1.0, attack 0.95-1.0, pain 0.75-0.85, aggro 0.55-0.65.

## t1454 / t1457: PixelLab batches 1-3 wired (`d2anim1446`)

Same hit (4 f @ 15 fps) / death (7 f @ 20 fps, POP = f2 white flash + blood/gib/scream/gore, pool or decal at f4, last frame = permanent corpse that works with `d2CorpsePersist1447`) pipeline as imp / brute, for every demon below. Files `Enemies/<key>_{hit,death,corpse}_{south,east,north,west}_pl.png`, nearest neighbour, PPU 16.

- **Art key per enemy** (`D2Sheet(e)` in `HellordD2Feel1434.cs`): Mortar `m53` 1/2/3 = `bloater` / `rice_ghost` / `grinder_monk`; `variant` 3 / 4 = `frost_caster` / `fire_wheel` (Fire Wheel ships E / W only, S / N use E); `k49` 1/2/3/7 = `imp_v2_k1/k2/k3/k7`, 4/5 = `wraith_v2_k4/k5`, 6 = `frost_brute`, 8 = `archer`; `variant` 1 / 2 = `<base>_var1` / `_var2` (imp_v2, shambler_v2, brute, splitter; var2 imp + shambler only). Missing strips fall back to the base type. Hook Thrower / Gossip Imp (`hook_thrower`, `gossip_imp`) now stagger with the PL hit.
- **Pivots**: hit / death strips x = w/2, y = the sheet's ground row (`D2Pl1454.GroundPx`: 1 px above the bottom, 5 for the 38x32 cells, 4 for the wheel, 6/8 for the boss cells). Corpse pivots are the death pivot minus the crop origin, measured by exact pixel matching of each corpse against the last death frame (`D2Pl1454` table, 111 entries, 0 mismatching pixels; brute E 21.5 / N 22.5 / W 24.5 as before).
- **Ghosts** (`wraith_v2*`, `rice_ghost`): `BloodFX/ectoplasm_pool_s.png` pool, no blood burst, no red decal, no red hit blood.
- **Corpses for the mortar demons and the sheet-death demons**: with PL art they get the normal PL death. Without it (`d2anim1446` off) `Death44` (Frost Caster / Fire Wheel) and `Death53` (Bloater / Rice Ghost / Grinder Monk) now leave the last death frame as a corpse (`D2CorpseFromSprite1457`), plus pool / decal; frozen deaths are skipped.
- **Bosses** (`UpdateBossDeath`): `boss_oxhead_clubjailer` (batch 4 re-roll, 72x72 cell pivot (36,66), corpse 72x51 pivot (36,49)), `boss_horseface_pestle` (SE strip, flipped for W), `boss_ice_gorilla` (one pose for every facing), `boss_qilin_chariot` (S and E, E flipped for W). The PL strip drives the boss body sprite (POP at f2 with the white flash, pool + decal at f4, permanent corpse at the end of the strip); the original sheet-death timing still decides when the boss is removed. No PL art / toggle off: `D2BossCorpseFallback1457` leaves the last body frame + pool / decal.
- **Attack voices**: one per unit per `d2AtkVoiceCd1457` (1.5 s) so a brute swinging every 0.6 s no longer barks every swing.
- Not integrated: batch 4 attack / idle strips, Hook Thrower / Gossip Imp art (still code-drawn), unused sheets (hellcat, calf, herder, hound, skeleton), split_child frost / fire.

### t1457 phase-2 voices (CC0, `d2sfx1447` now default on)

`Resources/Audio/SFX/d2sfx1447/` also holds the 171 phase-2 WAVs (`Audio/d2feel_t1449/phase2`, licences in `Docs/d2sfx_licenses_t1449.md`). Voice family per enemy (`D2Fam1447`): Ghoul = `shambler`, Splitter / Split Child = `splitter`, Hook Thrower = `hook_thrower`, Gossip Imp = `gossip_imp`, Archer (k49 8) = `archer`, Elite / Flame Wraith = `wraith`, Frost Caster, Snowball Imp and Ice Sniper = `frost_caster`, Fire Wheel, Bloater (and the exploding variant), Rice Ghost, Grinder Monk, Imp, Brute (also Frost Brute) as before, bosses `oxhead` / `horseface` / `icegorilla` / `qilin`. Hooks: `pain1447_<fam>`, `attack_<fam>`, `aggro_<fam>`, `death_<fam>` (boss deaths play at `OnBossKilled`). A family without its own clip falls back to the phase-1 family (`D2LegacyFam1447`: bloater / shambler / splitter -> ghoul, grinder_monk / hook_thrower -> brute, rice_ghost -> wraith, frost_caster / fire_wheel -> caster, gossip_imp / archer -> imp). Caps, replace-oldest and gaps are unchanged; `d2AtkVoiceCd1457` (1.5 s) limits each unit's attack voice.
