# Hellord Audio Map - Batch 1 (combat SFX)

All files: 44.1 kHz, 16-bit, mono WAV, generated procedurally by `tools/gen_batch1.py` (numpy/scipy; no samples, no AI audio). **Nothing has been listened to in game yet** - levels are by design (peak target), not by ear.

Loudness: short sounds are zero-padded to 1 s before the BS.1770 measurement (pyloudnorm), so LUFS values are *relative* indicators (lower = quieter), not strict program loudness. Frequent sounds peak ~-12..-14 dBFS; rare/big ones -6..-9 dBFS. Set mixer group volumes on top (SFX ~0.8).

| File | Game event / hook | Variants | Length (s) | Peak dBFS | Loudness | Suggested in-game volume / pitch jitter |
|---|---|---|---|---|---|---|
| `SFX/enemy/enemy_hit_01..04.wav` | Enemy takes damage (every projectile/melee hit) | 4 | 0.09-0.11 | -12.0 | -31.7 LUFS | vol 0.55-0.7, pitch 0.92-1.08, random variant, no repeat |
| `SFX/enemy/enemy_death_01..04.wav` | Normal enemy dies | 4 | 0.20-0.22 | -10.0 | -28.5 LUFS | vol 0.7-0.85, pitch 0.9-1.1; rate-limit ~20/s (voice cap 6) |
| `SFX/enemy/elite_death_01..02.wav` | Elite enemy dies | 2 | 0.66-0.66 | -7.0 | -25.7 LUFS | vol 1.0, pitch 0.95-1.05 |
| `SFX/fireball/fireball_cast_01..04.wav` | Fireball fired | 4 | 0.24-0.28 | -12.0 | -29.5 LUFS | vol 0.55-0.7, pitch 0.93-1.07; rate-limit with fire rate (voice cap 4) |
| `SFX/fireball/fireball_impact_01..04.wav` | Fireball hits / explodes | 4 | 0.40-0.47 | -9.0 | -28.6 LUFS | vol 0.7-0.85, pitch 0.9-1.1; cap 4 simultaneous |
| `SFX/skills/frost_cleave_01..04.wav` | Frost Cleave swing | 4 | 0.17-0.20 | -12.0 | -28.2 LUFS | vol 0.6-0.75, pitch 0.95-1.05 |
| `SFX/skills/storm_ring_01..04.wav` | Storm Ring pulse / zap on enemy | 4 | 0.14-0.18 | -13.0 | -33.3 LUFS | vol 0.5-0.65, pitch 0.92-1.08; rate-limit per ring pulse (voice cap 3) |
| `SFX/skills/bone_orbit_hit_01..04.wav` | Bone Orbit bone strikes an enemy | 4 | 0.09-0.12 | -13.0 | -35.2 LUFS | vol 0.5-0.65, pitch 0.9-1.1; rate-limit ~12/s |
| `SFX/skills/curse_rune_arm_01..02.wav` | Curse Rune placed / arming (before burst) | 2 | 1.25-1.28 | -12.0 | -24.6 LUFS | vol 0.7, pitch 0.97-1.03, play once on placement |
| `SFX/skills/curse_rune_burst_01..03.wav` | Curse Rune detonates | 3 | 0.84-0.89 | -9.0 | -28.2 LUFS | vol 0.8-0.9, pitch 0.93-1.07; has ~0.6 s reverb tail, do not cut it off |
| `SFX/hero/hero_hurt_01..03.wav` | Hero takes damage | 3 | 0.22-0.22 | -9.0 | -28.8 LUFS | vol 0.85-1.0, pitch 0.95-1.05; 0.25 s cooldown (i-frames) |
| `SFX/hero/low_hp_heartbeat_loop.wav` | Hero HP below ~25%: loop while low; fade out on heal | loop | 1.60 | -14.0 | -33.0 LUFS (3 loops) | vol 0.5, no jitter; loop; scale speed 1.0-1.25 with lower HP if wanted |
| `SFX/hero/hero_death_01.wav` | Hero dies | 1 | 1.70-1.70 | -6.0 | -23.2 LUFS | vol 1.0, no jitter; duck other SFX |
| `SFX/pickups/xp_pickup_01..04.wav` | XP blood-drop collected | 4 | 0.07-0.09 | -14.0 | -32.1 LUFS | vol 0.45-0.55, pitch base 0.97-1.03 then combo step (see below); voice cap 4 |

## Loop points

- `SFX/hero/low_hp_heartbeat_loop.wav`: loop start = sample 0, loop end = sample 70560 (1.600 s, whole file; file length 70560 samples). Beats are lub at 0.00 s and dub at 0.27 s (one lub-dub pair per 1.6 s = 37.5 pairs/min, slow and ominous); tails are folded around the end so it is seamless (wrap sample step 0/32767). In Unity: `AudioSource.loop = true`, no extra trimming.
- Other sounds are one-shots (`curse_rune_arm` is a ~1 s one-shot meant to be followed by `curse_rune_burst`).

## XP pickup combo pitch

Raise pitch on fast consecutive pickups: if the previous pickup was < 0.5 s ago, `step = min(step + 1, 12)`, else `step = 0`. `AudioSource.pitch = 2^(step/12) * random(0.97, 1.03)` (+1 semitone per pickup, cap +12). The preview ends with a 6-step ascending demo. Because pitch also shortens the clip, keep a voice cap of ~4.

## Notes

- Never play the same variant twice in a row; pick randomly from the variants.
- Suggested voice caps/rate limits are in the table; hits and pickups are the most frequent sounds - keep them low.
- Preview: `previews/batch1_preview.ogg` (order: enemy_hit x2, enemy_death x2, elite_death, fireball_cast x2, fireball_impact x2, frost_cleave x2, storm_ring x2, bone_orbit_hit x2, curse_rune_arm -> burst, hero_hurt x2, heartbeat x3 loops, hero_death, xp pickup ascending then 3 variants). It is rendered at the files' native (quiet) levels; turn the volume up.
- v2 (after first in-game test): `fireball_cast`, `curse_rune_arm`, `curse_rune_burst`, `storm_ring` were redesigned (same names/variant counts); `previews/batch1_redo_preview.ogg` has only these. All other files are unchanged (deterministic seeds).
- Regenerate: `python3 tools/gen_batch1.py` (deterministic seeds).

<!-- MUSIC:START -->
## Music

| File | Game event / hook | BPM | Length | Loudness | Peak | Loop points |
|---|---|---|---|---|---|---|
| `Music/music_floor1_combat.ogg` | Floor 1 room combat music (loop while in combat rooms on floor 1) | 128 | 75.000 s (40 bars, 4/4) | -19.0 LUFS integrated | -3.8 dBFS | start = sample 0, end = sample 3307500 (whole file loops) |

- A phrygian, 44.1 kHz stereo Ogg Vorbis (q6). Structure (8-bar sections): 1 bass+pad+soft drums, 2 + arp & snare, 3 + lead, 4 lead variation + 4-on-the-floor kick, 5 breakdown (bars 33-36) then rebuild into the loop start.
- Seamless: the music is rendered circularly (notes, delay and reverb tails wrap over the start), so the file is one exact loop of 3307500 samples. Decoded length: 3307500 samples. In Unity set `AudioSource.loop = true` and import with *Compression Format: Vorbis*, *Load Type: Streaming* (or Compressed In Memory); do **not** add a gap or fade between repeats. Use `AudioClip.samples == 3307500` as a sanity check.
- Suggested in-game: volume 0.5-0.6 on a Music mixer group (already quiet at about -19 LUFS), duck by ~3-6 dB on level-up/banner stingers. Crossfade into other tracks, not by trimming the loop.
- Preview: `previews/music_floor1_preview.ogg` (the loop plus 30% - the wrap happens at 75.0 s).
- Regenerate: `python3 tools/gen_music_floor1.py`.
<!-- MUSIC:END -->



<!-- BATCH2:START -->
## Batch 2 - event SFX

Same format/rules as Batch 1 (44.1 kHz, 16-bit, mono WAV; procedural via `tools/gen_batch2.py`; nothing heard in game yet). Tuned to A phrygian (A Bb C D E F G) to sit with the floor 1 music. LUFS are zero-padded to 1 s (relative values).

| File | Game event / hook | Variants | Length (s) | Peak dBFS | Loudness | Suggested in-game volume / pitch jitter |
|---|---|---|---|---|---|---|
| `SFX/events/levelup_sting.wav` | Player levels up (before card choice) | 1 | 1.50-1.50 | -9.0 | -25.2 LUFS | vol 0.8, no jitter, duck music ~4 dB for 1.5 s |
| `SFX/events/ui_card_select_01..03.wav` | Hover/select a level-up card | 3 | 0.06-0.07 | -17.0 | -39.4 LUFS | vol 0.5, pitch 0.95-1.05 (random variant on each hover) |
| `SFX/events/ui_upgrade_confirm_01..02.wav` | Confirm the chosen upgrade card | 2 | 0.60-0.60 | -12.0 | -28.3 LUFS | vol 0.7, pitch 0.97-1.03 |
| `SFX/events/room_cleared_sting.wav` | Last enemy of a room dies | 1 | 1.50-1.50 | -9.0 | -24.4 LUFS | vol 0.7, no jitter; play at the same time as door_unlock (delay unlock 0.2 s) |
| `SFX/events/door_unlock_01..02.wav` | Room doors unlock after clear | 2 | 1.20-1.20 | -10.0 | -30.9 LUFS | vol 0.7, pitch 0.95-1.05 |
| `SFX/events/banner_whoosh_floor.wav` | Floor-change banner appears | 1 | 1.20-1.20 | -10.0 | -25.3 LUFS | vol 0.6, no jitter |
| `SFX/events/banner_whoosh_boss.wav` | Boss-incoming banner appears | 1 | 1.20-1.20 | -9.0 | -24.2 LUFS | vol 0.7, no jitter; start with the banner slide-in |
| `SFX/events/chest_open_01..02.wav` | Chest opened | 2 | 1.20-1.20 | -11.0 | -29.7 LUFS | vol 0.7, pitch 0.95-1.05 |
| `SFX/events/shrine_use_01..02.wav` | Shrine activated | 2 | 2.00-2.00 | -11.0 | -25.1 LUFS | vol 0.7, pitch 0.97-1.03 |
| `SFX/events/shop_purchase_01..02.wav` | Item bought in shop | 2 | 0.50-0.50 | -13.0 | -27.9 LUFS | vol 0.6, pitch 0.95-1.05 |

Notes: `ui_card_select_01..03` are tiny variants - pick randomly; `door_unlock_01..02` differ in grind roughness; `chest_open_01..02` have different glimmer notes; `shrine_use_01..02` use different chime pitches; `shop_purchase_01..02` differ in coin pitch. `banner_whoosh_floor` = floor change, `banner_whoosh_boss` = boss incoming. Preview: `previews/batch2_preview.ogg`.
<!-- BATCH2:END -->

<!-- BATCH3:START -->
## Batch 3 - enemy and boss SFX

Same format/rules as earlier batches (44.1 kHz, 16-bit, mono WAV; procedural via `tools/gen_batch3.py`; nothing heard in game yet). Boss peaks <= -6 dBFS. LUFS are zero-padded to 1 s (relative values).

| File | Game event / hook | Variants | Length (s) | Peak dBFS | Loudness | Suggested in-game volume / pitch jitter |
|---|---|---|---|---|---|---|
| `SFX/enemies/hook_windup_rattle_01..02.wav` | Hook Thrower begins wind-up (telegraph) | 2 | 0.62-0.74 | -13.0 | -28.4 LUFS | vol 0.6, pitch 0.95-1.05 |
| `SFX/enemies/hook_chain_throw_01..03.wav` | Hook Thrower throws the hook/chain | 3 | 0.32-0.39 | -12.0 | -27.5 LUFS | vol 0.6, pitch 0.93-1.07 |
| `SFX/enemies/hook_hit_01..03.wav` | Hook connects with the hero | 3 | 0.28-0.33 | -10.0 | -27.3 LUFS | vol 0.8, pitch 0.95-1.05; play with hero_hurt |
| `SFX/enemies/gossip_shout_warning_01..02.wav` | Gossip Imp starts shout telegraph | 2 | 0.46-0.51 | -11.0 | -25.4 LUFS | vol 0.6, pitch 0.9-1.1 (per imp) |
| `SFX/enemies/gossip_shout_blast_01..02.wav` | Gossip Imp shout blast fires | 2 | 0.39-0.44 | -9.0 | -25.0 LUFS | vol 0.75, pitch 0.95-1.05; cap 2 simultaneous |
| `SFX/boss/oxhead_roar_01..02.wav` | Ox-Head intro / enrage roar | 2 | 1.70-1.70 | -7.0 | -22.0 LUFS | vol 0.9, pitch 0.97-1.03 |
| `SFX/boss/oxhead_horn_windup_01..02.wav` | Ox-Head horn attack telegraph | 2 | 0.76-0.87 | -12.0 | -21.9 LUFS | vol 0.8, no jitter; length ~ attack telegraph, trim/fade by code if shorter |
| `SFX/boss/oxhead_horn_strike_01..03.wav` | Ox-Head horn attack lands | 3 | 0.75-0.75 | -7.0 | -28.8 LUFS | vol 0.9, pitch 0.96-1.04 |
| `SFX/boss/oxhead_stomp_windup_01..02.wav` | Ox-Head stomp telegraph | 2 | 0.67-0.72 | -10.0 | -25.8 LUFS | vol 0.8, pitch 0.97-1.03 |
| `SFX/boss/oxhead_stomp_strike_01..03.wav` | Ox-Head stomp lands (shockwave) | 3 | 1.40-1.40 | -6.0 | -23.7 LUFS | vol 1.0, pitch 0.95-1.05; shake camera with it |
| `SFX/boss/oxhead_lunge_dash_01..02.wav` | Ox-Head lunge dash | 2 | 0.69-0.70 | -9.0 | -21.9 LUFS | vol 0.8, pitch 0.95-1.05 |
| `SFX/boss/oxhead_phase_change.wav` | Ox-Head phase change (HP threshold) | 1 | 2.20-2.20 | -6.0 | -22.2 LUFS | vol 1.0, no jitter; duck music ~6 dB, other SFX ~3 dB |

Notes: pick a random variant each time, never the same twice in a row. Reverb tails on boss sounds are cut with fades at the length shown. Preview: `previews/batch3_preview.ogg`.
<!-- BATCH3:END -->





<!-- MUSIC_OPTIONS:START -->
## Floor music OPTIONS — DRAFT (not locked, not heard in game)

Generator: `tools/gen_music_options.py` (fixed seeds). All: OGG 44.1 kHz stereo q6, whole file loops (loop start 0, end = file length), kick on every quarter note with downbeat accent, lowpassed 6–7 kHz. Hazard sync: beat = quarter note, beat 1 of bar = sample 0 + n·bar.

| Floor | Opt | File | BPM | Bars | Length | Loop end (smp) | LUFS | Peak dBFS | Beat period | Bar period | Vibe |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Pounding Mortar | A | `Music/options/music_mortar_optA_80bpm.ogg` | 80 | 16 | 48.000 s | 2116800 | -19.0 | -4.2 | 750.00 ms / 33075 smp | 3000.0 ms / 132300 smp | Slow, crushing E-phrygian grind; stone pestle thump on beat 4, kick on every beat |
| Pounding Mortar | B | `Music/options/music_mortar_optB_90bpm.ogg` | 90 | 16 | 42.667 s | 1881600 | -19.0 | -5.4 | 666.67 ms / 29400 smp | 2666.7 ms / 117600 smp | Boss-P1 tempo D-phrygian march; driving 8th bass, clap 2/4, pestle thud beat 4 |
| Frozen Hell | A | `Music/options/music_frozen_optA_100bpm.ogg` | 100 | 16 | 38.400 s | 1693440 | -19.0 | -5.1 | 600.00 ms / 26460 smp | 2400.0 ms / 105840 smp | Cold B harmonic-minor; glassy offbeat ticks, hollow kick, slow icy pad |
| Frozen Hell | B | `Music/options/music_frozen_optB_110bpm.ogg` | 110 | 16 | 34.909 s | 1539491 | -19.0 | -4.8 | 545.45 ms / 24054.5 smp | 2181.8 ms / 96218.2 smp | Eerie F#-phrygian pulse; brittle ice hats, glass bells, music-box arp |
| Burning Hell | A | `Music/options/music_burning_optA_140bpm.ogg` | 140 | 20 | 34.286 s | 1512000 | -19.0 | -6.6 | 428.57 ms / 18900 smp | 1714.3 ms / 75600 smp | Hot D phrygian-dominant drive; rolling 16th saw bass, snare 2/4, tom fills |
| Burning Hell | B | `Music/options/music_burning_optB_150bpm.ogg` | 150 | 24 | 38.400 s | 1693440 | -19.0 | -7.1 | 400.00 ms / 17640 smp | 1600.0 ms / 70560 smp | Aggressive C#-phrygian gallop; distorted bass gallop, ghost snares, fiery riff |
| Ox Pit | A | `Music/options/music_oxpit_optA_128bpm.ogg` | 128 | 16 | 30.000 s | 1323000 | -19.0 | -6.1 | 468.75 ms / 20671.9 smp | 1875.0 ms / 82687.5 smp | Ox Pit A-phrygian, same palette as current track but four-on-the-floor throughout |

Notes:
- Pounding Mortar: extra pestle-thud layer on beat 4 of every bar (design doc: accented 4th beat = slam), kick accent on beat 1. Survive ramp 80→110 needs runtime pitch (shifts key ~+5.5 st) or a separate ramped render; Frenzy 72 BPM = optA at pitch 0.9.
- Ox Pit: current `music_floor1_combat.ogg` has a kick on every beat only in bars 25–32 (+37–40 rebuild); optA is a four-on-the-floor alternative.
- Previews: `previews/music_options_<floor>_preview.ogg` = optA, 1 s gap, optB (Ox Pit: optA, gap, first 30 s of current track).
<!-- MUSIC_OPTIONS:END -->

<!-- BOSS_MUSIC:START -->
## Boss-fight music — DRAFT (not locked, not heard in game)

Generator: `tools/gen_music_boss.py` (fixed seeds, renderer from gen_music_options.py). OGG 44.1 kHz stereo q6; whole file loops (loop start 0, end = file length). Kick on every quarter note, accented downbeat; beat 1 of bar k = k·bar samples. Previews: `previews/music_boss_<name>_preview.ogg` (Horse-Face P2: `music_boss_horseface_p2_preview.ogg`). v1 files kept as `*_v1_backup.ogg` (oxhead 140, horseface 90/100); v2 fun versions as `*_fun_backup.ogg`; v3 dark 160/112/126 BPM as `*_dark_backup.ogg`; v4 180/140/155 as `*_pre_hype_backup.ogg`; v5 no-hook hype as `*_nohook_backup.ogg`; Horse-Face v6 + Ice Gorilla 120 BPM as `*_prehook_backup.ogg`; Horse-Face/Ice Gorilla v7 as `*_weakhook_backup.ogg`; Ox-Head v6 + Qilin v2 as `*_prehook_backup.ogg`. All five bosses = v8 top-line 3-note horn hook remake (riff call/response bars 1-4, b2 variation 5-8, doubled hook 9-16, tritone development 17-24 -> surge into wrap; drone, dissonance, chug + 8-bar build/riser/impact, double-kick bursts, power stabs; no lead/arp). = loop + 30% (wrap audible), 1 s fade.

| Boss | File | BPM | Bars | Length | Loop end (smp) | LUFS | Peak dBFS | Beat period | Bar period | Event hook | Vibe |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Ice Gorilla | `Music/boss/music_boss_icegorilla_150bpm.ogg` | 150 | 24 | 38.400 s | 1693440 | -19.0 | -4.7 | 400.00 ms / 17640 smp | 1600.0 ms / 70560 smp | Ice Gorilla boss fight (Frozen Hell boss room) | HOOK v8: 3-note icy horn B-F-C (high, tritone drop, b2 sink) with glass ping, solo bars 1-2, returns every 4 bars louder; under it B-phrygian chest-beat riff call/response (2 pounding hits -> tritone climb -> b2 crash), evolves per 4 bars; cold hell: frozen drone, dissonant glass shards (b2/tritone), icy shatter swells, chug, 8-bar surge/impact |
| Qilin Chariot | `Music/boss/music_boss_qilin_160bpm.ogg` | 160 | 24 | 36.000 s | 1587600 | -19.0 | -5.3 | 375.00 ms / 16537.5 smp | 1500.0 ms / 66150 smp | Qilin Chariot boss fight (Burning Hell boss room) | HOOK v8: 3-note top-line horn B-C-E (half-step rise, then plunge to root) solo bars 1-2, returns every 4 bars louder (b2 shift, octave, tritone harmony, double-time, power chords); burning hell: E-phrygian 16th fire chug, distorted drone, ghost snares, 8-bar surge/impact |
| Ox-Head | `Music/boss/music_boss_oxhead_180bpm.ogg` | 180 | 24 | 32.000 s | 1411200 | -19.0 | -5.2 | 333.33 ms / 14700 smp | 1333.3 ms / 58800 smp | Ox-Head boss fight (Ox Pit boss room) | HOOK v8: 3-note top-line horn A-C-Bb (climb a minor third, crush down a half step) solo bars 1-2, returns every 4 bars louder (b2 shift, octave, tritone harmony, double-time, power chords); under it A-phrygian riff call/response (b2+tritone), evolves per 4 bars; hype surge: distorted b2/tritone drone, chugging low 8th bass, war drums, low dissonant horn swells |
| Horse-Face (P1) | `Music/boss/music_boss_horseface_140bpm.ogg` | 140 | 24 | 41.143 s | 1814400 | -19.0 | -4.5 | 428.57 ms / 18900 smp | 1714.3 ms / 75600 smp | Horse-Face phase 1 (Pounding Mortar boss room P1) | HOOK v8: 3-note top-line horn Eb-D-Ab (dum-dum-HOWL) solo bars 1-2, returns every 4 bars louder (octave, tritone harmony, double-time); under it D-phrygian hammer riff (3 blows on the root -> tritone howl, answer an octave up), evolves per 4 bars; crushing: grinding drone, low chug, pestle thud beat 4, iron clangs, dissonant choir-like pad |
| Horse-Face (P2) | `Music/boss/music_boss_horseface_p2_155bpm.ogg` | 155 | 24 | 37.161 s | 1638813 | -19.0 | -5.2 | 387.10 ms / 17071 smp | 1548.4 ms / 68283.9 smp | Horse-Face phase 2 (Pounding Mortar boss room P2) | P2 HOOK v8: same Eb-D-Ab horn hook + hammer riff, harder from bar 1, 16th chug, denser drums, rising tritone horn, more distortion |

Not covered: Frost Ox-Head (legacy, replaced by Ice Gorilla; debug-only), King Yama / Ten Kings (design doc only, not in code).
<!-- BOSS_MUSIC:END -->

<!-- HELL_SCREAMS:START -->
## Ambient hell screams (distant damned) - WIRED in v0.0.50

Event hook: **ambient hell scream / distant damned** (rooms & corridors). Procedural (`tools/gen_hell_screams.py`, deterministic seeds). 44.1 kHz 16-bit mono WAV in `SFX/ambient/`. Preview: `previews/hell_screams_preview.ogg` (all variants, 1.2 s gaps).

| File | Variant | Length | Peak dBFS | LUFS |
|---|---|---|---|---|
| `SFX/ambient/hell_scream_01.wav` | damned soul wail | 2.40 s | -16.0 | -29.8 |
| `SFX/ambient/hell_scream_02.wav` | demon howl (low, growly) | 2.50 s | -15.0 | -30.2 |
| `SFX/ambient/hell_scream_03.wav` | distant female-like wail, very far | 2.50 s | -18.0 | -31.7 |
| `SFX/ambient/hell_scream_04.wav` | tortured shriek (short, harsh) | 1.30 s | -17.0 | -29.1 |
| `SFX/ambient/hell_scream_05.wav` | low moan rising to scream | 2.50 s | -15.0 | -27.7 |
| `SFX/ambient/hell_scream_06.wav` | chorus of the damned (3 far voices) | 2.50 s | -17.0 | -31.7 |

Suggested use: pick a random variant (no immediate repeat), play as 2D/3D ambient at **0.3-0.5 volume** of the SFX bus (well under combat SFX), random pitch 0.92-1.08, random pan/position off-screen. Chance ~15-25% when entering a room, plus a global cooldown of 20-40 s (never more than 1 at a time; suppress during boss fights or duck under combat).
<!-- HELL_SCREAMS:END -->

<!-- MENU_AUDIO:START -->
## Menu music + menu/altar UI SFX - DRAFT, not heard/wired

### MUSIC menu (`tools/gen_menu_music.py`)
Seamless whole-file loops (circular render), OGG 44.1 kHz stereo q6. Previews: `previews/music_menu_main_preview.ogg`, `previews/music_menu_altar_preview.ogg` (loop + 30%).

| File | BPM | Bars | Length | Type | LUFS | Peak dBFS | Use |
|---|---|---|---|---|---|---|---|
| `Music/music_menu_main.ogg` | 56 | 16 | 68.57 s | loop | -19.0 | -5.2 | v3 main menu (calmer): consonant D drone (root/5th) gently breathing, soft bowed metal, low chant in 5ths, gentle minor-mode temple bells every 2 bars, one soft drum per 4 bars; no horror dissonance |
| `Music/music_menu_altar.ogg` | 50 | 12 | 57.60 s | loop | -19.0 | -5.0 | v3 altar (calmer, slightly more ritual): D drone with faint tritone shade, low chant, soft gong every 4 bars, sparse soft frame drum, minor-mode bells |
| `Music/music_stinger_dead.ogg` | - | - | 4.50 s | one-shot | -20.0 | -8.8 | Dead stinger (one-shot, before menu track): falling bells + low choir |
| `Music/music_stinger_won.ogg` | - | - | 4.50 s | one-shot | -20.0 | -8.3 | Won stinger (one-shot): rising bells + dark D-minor swell |

Suggested: music bus 0.6-0.7 for loops; crossfade main <-> altar over 1.5 s; stingers at 0.7, then fade the menu loop in after ~3 s.

### UI menu hooks (`tools/gen_menu_sfx.py`)
44.1 kHz 16-bit mono WAV, for `HellordAudio.Play("<hook>")` -> `Resources/Audio/SFX/<folder>/<hook>_NN`. Preview: `previews/menu_sfx_preview.ogg` (all, in order, 0.5 s gaps).

| Hook | Folder | Files | Event | Lengths | Peak dBFS | Suggested vol |
|---|---|---|---|---|---|---|
| `ui_hover` | ui | 4 (`_01`-`_04`) | menu item hover/focus (soft v2) | 0.24-0.25 s | -26 | 0.35 |
| `ui_confirm` | ui | 2 (`_01`-`_02`) | menu confirm / open submenu | 1.10-1.14 s | -21 | 0.6 |
| `ui_card_select` | events | 3 (`_01`-`_03`) | card / altar node focus-select click (soft v2; overrides batch2 file) | 0.35-0.35 s | -22 | 0.45 |
| `ui_back` | ui | 2 (`_01`-`_02`) | back / close submenu | 0.98-1.04 s | -19 | 0.55 |
| `ui_denied` | ui | 2 (`_01`-`_02`) | locked / COMING SOON (Msg51) | 0.82-0.87 s | -19 | 0.55 |
| `seal_press` | ui | 2 (`_01`-`_02`) | pact seal press | 1.50-1.53 s | -17 | 0.65 |
| `altar_carousel` | ui | 3 (`_01`-`_03`) | altar path carousel L/R | 1.02-1.04 s | -20 | 0.5 |
| `pact_unlock` | ui | 2 (`_01`-`_02`) | pact unlocked | 1.49-1.60 s | -20 | 0.65 |
| `pact_sign` | ui | 2 (`_01`-`_02`) | pact signed | 1.40-1.45 s | -20 | 0.65 |
| `pact_remove` | ui | 2 (`_01`-`_02`) | pact removed | 1.08-1.09 s | -19 | 0.55 |
| `path_switch` | ui | 2 (`_01`-`_02`) | path switch confirm YES | 1.68-1.73 s | -17 | 0.65 |
| `pact_seal_descend` | ui | 2 (`_01`-`_02`) | SEAL PACT / Descend into game | 2.37-2.39 s | -15 | 0.75 |
| `card_reroll` | ui | 2 (`_01`-`_02`) | level-up card reroll | 1.00-1.11 s | -19 | 0.55 |
| `card_skip` | ui | 2 (`_01`-`_02`) | level-up card skip | 0.90-0.94 s | -20 | 0.5 |
| `restart` | end | 2 (`_01`-`_02`) | Dead/Won R restart | 1.97-2.04 s | -17 | 0.65 |

Random variant per play (no immediate repeat). Hover: rate-limit ~80 ms. v2 (VH): new menu loops; softer ui_hover, ui_confirm, ui_card_select (node/card click), pact_unlock, pact_sign. v1 kept as `*_v1_backup.wav/.ogg`; v2 scary menu loops as `*_scary_backup.ogg`, loops now v3 calmer. Note: rerunning `gen_batch2.py` would restore the old harsh `SFX/events/ui_card_select_*`.
<!-- MENU_AUDIO:END -->
