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
