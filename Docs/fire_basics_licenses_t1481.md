# t1481 fire basics — source licences

Every source file actually used in a rendered output is listed below.
Licence was checked on each file's own OpenGameArt, NPS, or Internet Archive page.
Only CC0 / public-domain material was used. Raw sources live in `_sources/`.
This pack does not contain copyrighted or game-ripped audio. D2 / D2R / PoE 2 fire skills were used as feel references from public descriptions only; no game audio was sampled.
Procedural layers (whoosh, crackle, rumble, crunch, wet thud, fire roar) are synthesised in `tools/gen_fire_basics_t1481.py`.

## Policy notes

- Freesound was not used. No account or API key was created.
- Skipped (Freesound derivatives, even when the OGA page is tagged CC0, because the upstream file was not independently verified without a login): themightyglider *Catching fire*, *Moving Boulder*, *Breaking Rock*.
- Skipped (mixed Freesound / not clearly CC0 on the item page): congusbongus *Footsteps on different surfaces* (CC-BY / mixed Freesound authors); Jute *Foot walking step sounds* (based on pdsounds.org).
- Skipped (7z packs and/or third-party Freesound extracts): Fantozzi's Footsteps; *42 Snow and Gravel Footsteps* (Corsica_S extracts, permission claimed for CC0).
- Skipped in the mix (page-checked PD, downloaded, not used in any output): NPS Maple Fire late-afternoon clip (the page notes grasshopper hops); NPS Beehive Geyser (distant, very quiet field recording, not a usable close roar). They remain under `_sources/` for later inspection.
- Spring Spring *Synthesized Fire Sound Effect* was not used; fire textures come from recorded CC0/PD fire plus our own procedural layers.
- NPS Yellowstone recordings are U.S. government works in the public domain. Credit the National Park Service (and the named recordists) where appropriate; attribution is not required.
- USFWS alligator hiss is a U.S. government work in the public domain. Credit the U.S. Fish and Wildlife Service if possible; attribution is not required.

## Sources used

### 80 CC0 creature SFX

- Author: rubberduck
- URL: https://opengameart.org/content/80-cc0-creature-sfx
- Licence: CC0 1.0 (OpenGameArt page). Copies of spit/burble/cough/breath also live in d2feel_t1449/_sources/.
- Files:
  - `rubberduck_80_cc0_creature_sfx/burble_01.ogg` → `magma_throw_land_03.wav`, `magma_throw_splat_03.wav`
  - `rubberduck_80_cc0_creature_sfx/burble_02.ogg` → `magma_throw_land_01.wav`, `magma_throw_splat_01.wav`
  - `rubberduck_80_cc0_creature_sfx/spit_01.ogg` → `magma_throw_land_01.wav`, `magma_throw_splat_02.wav`
  - `rubberduck_80_cc0_creature_sfx/spit_02.ogg` → `magma_throw_land_02.wav`
  - `rubberduck_80_cc0_creature_sfx/spit_03.ogg` → `magma_throw_land_03.wav`, `magma_throw_splat_04.wav`

### 80 CC0 creture SFX #2

- Author: rubberduck
- URL: https://opengameart.org/content/80-cc0-creture-sfx-2
- Licence: CC0 1.0 (OpenGameArt page). Title spelling is as on the OGA page. Copies of slime files also live in d2feel_t1449/_sources/.
- Files:
  - `rubberduck_80_cc0_creature_sfx_2/slime_01.ogg` → `magma_throw_land_02.wav`, `magma_throw_splat_03.wav`
  - `rubberduck_80_cc0_creature_sfx_2/slime_02.ogg` → `magma_throw_splat_02.wav`
  - `rubberduck_80_cc0_creature_sfx_2/slime_03.ogg` → `magma_throw_land_04.wav`
  - `rubberduck_80_cc0_creature_sfx_2/slime_07.ogg` → `magma_throw_splat_01.wav`
  - `rubberduck_80_cc0_creature_sfx_2/slime_08.ogg` → `magma_throw_land_04.wav`, `magma_throw_splat_04.wav`

### Air whoosh

- Author: pyranostudios
- URL: https://opengameart.org/content/air-whoosh
- Licence: CC0 1.0 (OpenGameArt page). Author says they made this nunchuk-style whoosh loop.
- Files:
  - `pyranostudios_air_whoosh/whoosh2.wav` → `fire_pillar_erupt_01.wav`, `fire_pillar_erupt_02.wav`, `fire_pillar_erupt_03.wav`, `fire_pillar_erupt_04.wav`, `magma_throw_cast_01.wav`, `magma_throw_cast_02.wav`, `magma_throw_cast_03.wav`, `magma_throw_cast_04.wav`, `magma_throw_cast_05.wav`

### Animal sounds from nature

- Author: U.S. Fish and Wildlife Service (field recordings; Internet Archive item animalsounds1)
- URL: https://archive.org/details/animalsounds1
- Licence: Public domain (U.S. government work). The item says recordings may be used without prior permission; credit the U.S. Fish and Wildlife Service if possible. Attribution is not required. A copy of the alligator hiss also lives in d2feel_t1449/_sources/.
- Files:
  - `usfws_animal_sounds/usfws_25alligatorhiss.mp3` → `fire_pillar_telegraph_01.wav`, `fire_pillar_telegraph_02.wav`, `fire_pillar_telegraph_03.wav`, `magma_rock_stop_01.wav`, `magma_rock_stop_02.wav`, `magma_rock_stop_03.wav`

### Different steps on wood, stone, leaves, gravel and mud

- Author: TinyWorlds
- URL: https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud
- Licence: CC0 1.0 (OpenGameArt page). Zip of 8 short step recordings; gravel, stone and mud are used.
- Files:
  - `tinyworlds_different_steps/unpacked/gravel.ogg` → `magma_rock_hit_01.wav`, `magma_rock_hit_02.wav`, `magma_rock_hit_03.wav`, `magma_rock_hit_04.wav`, `magma_rock_launch_01.wav`, `magma_rock_launch_02.wav`, `magma_rock_launch_03.wav`, `magma_rock_launch_04.wav`, `magma_rock_roll_loop.wav`, `magma_rock_stop_01.wav`, `magma_rock_stop_02.wav`, `magma_rock_stop_03.wav`
  - `tinyworlds_different_steps/unpacked/mud02.ogg` → `magma_throw_splat_01.wav`, `magma_throw_splat_02.wav`, `magma_throw_splat_03.wav`, `magma_throw_splat_04.wav`
  - `tinyworlds_different_steps/unpacked/stone01.ogg` → `magma_rock_hit_01.wav`, `magma_rock_hit_02.wav`, `magma_rock_hit_03.wav`, `magma_rock_hit_04.wav`, `magma_rock_stop_01.wav`, `magma_rock_stop_02.wav`, `magma_rock_stop_03.wav`

### Fire Crackling

- Author: AntumDeluge
- URL: https://opengameart.org/content/fire-crackling
- Licence: CC0 1.0 (OpenGameArt page). A popping and crackling fireplace.
- Files:
  - `antumdeluge_fire_crackling/fire-1.wav` → `fire_pillar_erupt_01.wav`, `fire_pillar_erupt_02.wav`, `fire_pillar_erupt_03.wav`, `fire_pillar_erupt_04.wav`, `fire_pillar_telegraph_01.wav`, `fire_pillar_telegraph_02.wav`, `fire_pillar_telegraph_03.wav`, `magma_rock_launch_01.wav`, `magma_rock_launch_02.wav`, `magma_rock_launch_03.wav`, `magma_rock_launch_04.wav`, `magma_rock_roll_loop.wav`

### Fireplace Sound loop

- Author: PagDev
- URL: https://opengameart.org/content/fireplace-sound-loop
- Licence: CC0 1.0 (OpenGameArt page). Author says credit is not required, but always appreciated.
- Files:
  - `pagdev_fireplace_sound_loop/fire.wav` → `fire_pillar_telegraph_01.wav`, `fire_pillar_telegraph_02.wav`, `fire_pillar_telegraph_03.wav`, `magma_rock_roll_loop.wav`, `magma_rock_stop_01.wav`, `magma_rock_stop_02.wav`, `magma_rock_stop_03.wav`, `magma_throw_cast_01.wav`, `magma_throw_cast_02.wav`, `magma_throw_cast_03.wav`, `magma_throw_cast_04.wav`, `magma_throw_cast_05.wav`, `magma_throw_splat_01.wav`, `magma_throw_splat_02.wav`, `magma_throw_splat_03.wav`, `magma_throw_splat_04.wav`

### Flare ignition

- Author: qubodup
- URL: https://opengameart.org/content/flare-ignition
- Licence: CC0 1.0 (OpenGameArt page). Author-recorded matchstick ignition, sped up and normalised.
- Files:
  - `qubodup_flare_ignition/ignition.flac` → `magma_throw_cast_02.wav`, `magma_throw_cast_04.wav`

### Stone, rock or wood moved sound

- Author: pauliuw
- URL: https://opengameart.org/content/stone-rock-or-wood-moved-sound
- Licence: CC0 1.0 (OpenGameArt page).
- Files:
  - `pauliuw_stone_rock_or_wood_moved/stone_rock_or_wood_moved.mp3` → `magma_rock_hit_01.wav`, `magma_rock_hit_02.wav`, `magma_rock_hit_03.wav`, `magma_rock_hit_04.wav`, `magma_rock_launch_01.wav`, `magma_rock_launch_02.wav`, `magma_rock_launch_03.wav`, `magma_rock_launch_04.wav`, `magma_rock_roll_loop.wav`, `magma_rock_stop_01.wav`, `magma_rock_stop_02.wav`, `magma_rock_stop_03.wav`

### Swishes Sound Pack

- Author: artisticdude
- URL: https://opengameart.org/content/swishes-sound-pack
- Licence: CC0 1.0 (OpenGameArt page). Author-recorded with a camcorder, clothes hanger and sticks of wood for the FOSS RPG Summoning Wars.
- Files:
  - `artisticdude_swishes/unpacked/swishes/swish-1.wav` → `magma_throw_cast_05.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-11.wav` → `magma_throw_cast_03.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-12.wav` → `magma_throw_cast_02.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-13.wav` → `magma_throw_cast_01.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-2.wav` → `magma_throw_cast_04.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-6.wav` → `magma_rock_launch_04.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-7.wav` → `fire_pillar_erupt_02.wav`, `magma_rock_launch_01.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-8.wav` → `fire_pillar_erupt_03.wav`, `magma_rock_launch_02.wav`
  - `artisticdude_swishes/unpacked/swishes/swish-9.wav` → `fire_pillar_erupt_01.wav`, `fire_pillar_erupt_04.wav`, `magma_rock_launch_03.wav`

### Yellowstone Sound Library — Artists Paintpots

- Author: National Park Service & MSU Acoustic Atlas (recordist Jennifer Jerrett)
- URL: https://www.nps.gov/yell/learn/photosmultimedia/sounds-artistpaintpots.htm
- Licence: Public domain (U.S. government work). Files may be used without limitation; credit the National Park Service where appropriate. Attribution is not required.
- Files:
  - `nps_yellowstone_artist_paintpots/artist_paintpots.mp3` → `magma_throw_land_03.wav`, `magma_throw_splat_02.wav`, `magma_throw_splat_04.wav`

### Yellowstone Sound Library — Black Growler Steam Vent

- Author: National Park Service (recordist Jennifer Jerrett)
- URL: https://www.nps.gov/yell/learn/photosmultimedia/sounds-blackgrowler.htm
- Licence: Public domain (U.S. government work). The stereo (non-binaural) Black Growler clip is used. Files may be used without limitation; credit the National Park Service where appropriate. Attribution is not required.
- Files:
  - `nps_yellowstone_black_growler/black_growler.mp3` → `fire_pillar_telegraph_01.wav`, `fire_pillar_telegraph_02.wav`, `fire_pillar_telegraph_03.wav`

### Yellowstone Sound Library — Fire (Maple Fire)

- Author: National Park Service (recordist Jennifer Jerrett)
- URL: https://www.nps.gov/yell/learn/photosmultimedia/sounds-fire.htm
- Licence: Public domain (U.S. government work). The Yellowstone Sound Library says files may be downloaded and used without limitation; credit the National Park Service where appropriate. Attribution is not required.
- Files:
  - `nps_yellowstone_maple_fire/maple_fire_10am.mp3` → `fire_pillar_erupt_01.wav`, `fire_pillar_erupt_02.wav`, `fire_pillar_erupt_03.wav`, `fire_pillar_erupt_04.wav`, `magma_rock_hit_01.wav`, `magma_rock_hit_02.wav`, `magma_rock_hit_03.wav`, `magma_rock_hit_04.wav`, `magma_throw_land_01.wav`, `magma_throw_land_02.wav`, `magma_throw_land_03.wav`, `magma_throw_land_04.wav`
  - `nps_yellowstone_maple_fire/maple_fire_12pm.mp3` → `fire_pillar_erupt_01.wav`, `fire_pillar_erupt_02.wav`, `fire_pillar_erupt_03.wav`, `fire_pillar_erupt_04.wav`, `magma_rock_launch_01.wav`, `magma_rock_launch_02.wav`, `magma_rock_launch_03.wav`, `magma_rock_launch_04.wav`, `magma_rock_roll_loop.wav`

### Yellowstone Sound Library — Fountain Paint Pot

- Author: National Park Service (recordist Jennifer Jerrett)
- URL: https://www.nps.gov/yell/learn/photosmultimedia/sounds-fountainpaintpots.htm
- Licence: Public domain (U.S. government work). Files may be used without limitation; credit the National Park Service where appropriate. Attribution is not required.
- Files:
  - `nps_yellowstone_fountain_paintpot/fountain_paintpot.mp3` → `magma_throw_land_01.wav`, `magma_throw_land_02.wav`, `magma_throw_land_04.wav`, `magma_throw_splat_01.wav`, `magma_throw_splat_03.wav`

### Yellowstone Sound Library — Fumaroles

- Author: National Park Service (recordist Jennifer Jerrett)
- URL: https://www.nps.gov/yell/learn/photosmultimedia/sounds-fumaroles.htm
- Licence: Public domain (U.S. government work). Files may be used without limitation; credit the National Park Service where appropriate. Attribution is not required.
- Files:
  - `nps_yellowstone_fumaroles/fumarole_large.mp3` → `fire_pillar_erupt_01.wav`, `fire_pillar_erupt_02.wav`, `fire_pillar_erupt_03.wav`, `fire_pillar_erupt_04.wav`, `fire_pillar_telegraph_02.wav`, `magma_rock_roll_loop.wav`, `magma_throw_land_01.wav`, `magma_throw_land_02.wav`, `magma_throw_land_03.wav`, `magma_throw_land_04.wav`
  - `nps_yellowstone_fumaroles/fumaroles_small.mp3` → `fire_pillar_telegraph_01.wav`, `fire_pillar_telegraph_03.wav`

### Yellowstone Sound Library — The Dragon's Mouth

- Author: National Park Service (recordist David Restivo)
- URL: https://www.nps.gov/yell/learn/photosmultimedia/sounds-dragonsmouth.htm
- Licence: Public domain (U.S. government work). Files may be used without limitation; credit the National Park Service where appropriate. Attribution is not required.
- Files:
  - `nps_yellowstone_dragons_mouth/dragons_mouth.mp3` → `magma_throw_land_02.wav`, `magma_throw_land_03.wav`, `magma_throw_land_04.wav`

