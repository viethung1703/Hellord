# t1481 v2 — diagnosis and research

Feel reference only. No game audio was downloaded, kept, sampled or reused.
YouTube footage was not fetched (policy: if YouTube blocks, do not work around; also no ripping).

## 1. Diagnosis of the in-game captures

Clips (8.00 s, 960×540, 30 fps, mono 44.1 kHz AAC), analysed from extracted WAV:

- `/workspace/Hellord/captures/t1481_fire_basics/t1481_magma_throw_FINAL_FX_real_game.mp4`
- `/workspace/Hellord/captures/t1481_fire_basics/t1481_rolling_magma_rock_FINAL_FX_real_game.mp4`
- `/workspace/Hellord/captures/t1481_fire_basics/t1481_fire_pillar_FINAL_FX_real_game.mp4`

Placeholder clips of the same skills were compared. Stills were grabbed at 8 fps to read glob, boulder and pillar size.

### Mix and music

All three FINAL clips are **digital silence until 0.667 s** (the banner “R 1 OF 4 – FROZEN” is visual only in these recordings). **There is no music bed in these captures**, so loudness was measured against other gameplay SFX (enemy hits, deaths, etc.), not against a score.

| Clip | Integrated | True peak | Action RMS (1.2–6.5 s) | Action peak |
|---|---|---|---|---|
| Molten Spit FINAL | −19.9 LUFS | −5.0 dBFS | −19.1 dBFS | −4.5 dBFS |
| Infernal Boulder FINAL | −17.7 LUFS | −4.0 dBFS | −17.6 dBFS | −1.7 dBFS |
| Fire Pillar FINAL | −16.9 LUFS | −2.9 dBFS | −17.0 dBFS | **+0.1 dBFS (clips)** |

VH prefers a quiet overall mix. Even without music, Fire Pillar already clips in the capture, and the other two sit in the mid-to-high −20s LUFS once many voices stack. v2 therefore **lowers file peaks and hook-gain suggestions**, and puts perceived weight into **spectrum and lower crest**, not into a hotter peak.

### What the visuals are

- **Molten Spit:** small orange globs, about a demon’s head or smaller, spat rapidly in several directions. Impacts are modest fire bursts, roughly 1–2 demon-widths, with little leftover blobs. Many projectiles and bursts are on screen at once under a held click.
- **Infernal Boulder:** a glowing orange rock about the size of the player (not a wagon-sized D2 boulder). Held click can put several rocks on screen. Each rolls through a pack with a short fire trail and knocks demons.
- **Fire Pillar:** a small ground spark/mark, then a tall flame column about 2–3 demons high and one demon wide, which stands for a moment. Several pillars can be active as the player moves.

### Timing and machine-gunning

High-band (2–8 kHz) onset rate in the FINAL mixes, mid-clip (2–7 s):

- Molten Spit: **13.4 events/s**, median gap **57 ms**, 62 gaps under 80 ms.
- Infernal Boulder: also ~13 events/s in the high band (launch + loop grit + hits stacking).
- Fire Pillar: ~14 events/s (telegraphs, erupts, and other SFX).

v1 recommended a 90–110 ms cast retrigger, but the live fire rate is faster than the cast length. Short v1 whooshes therefore chatter as a spray of paper ticks. Weight must live on **land / hit / erupt**, and casts must stay short, rounded and quiet enough to stack.

### What’s wrong with the v1 files (dry spectra)

Measured on the 44.1 kHz mono WAVs themselves:

- **Casts** peak at −16 dBFS but RMS is only −30 to −32 (crest 14–16 dB). Variants 01–03 have **0.2–1.1% of energy below 150 Hz** and almost no air above 4 kHz (they were low-passed around 4.4–5.2 kHz “to avoid fatigue”). They are mid-only paper whooshes. When 13 of them fire each second they hiss and click, and they never sound like a molten spit.
- **Lands** are too short (0.28–0.40 s) with a high spectral centroid (**4150–4700 Hz**). They read as a sizzle tick, not a heavy molten burst. Crest is still ~14–15 dB, so any sub energy is a spike, not a body, and there is almost no sizzle tail.
- **Splats** are wet and short, with little fire.
- **Launches** have **0% energy above 4 kHz** — a muted woof with no fire crackle.
- **Roll loop** is **99.3% energy below 150 Hz**, with no mid grit and no crackle (0% above 4 kHz). Under anything else it becomes vague bass, not a grinding fiery rock. (Integrated LUFS of −38.8 on the 1.6 s file was an ebur128 gating artefact; RMS is −22, but the spectrum is still wrong.)
- **Hits** have crest 16–18 dB and RMS around −29 to −31: tick-like crunches, no body.
- **Stops** are worse: crest 23–26 dB, RMS −36 to −39. The crumble is almost inaudible after the peak.
- **Telegraph** has almost no sub (1–2% below 150 Hz) and a 4100–4500 Hz centroid: a thin hiss, not a ground rumble.
- **Erupt** centroids sit at **4750–5010 Hz**, crest 15–19 dB. In the capture, stacked erupts **clip at 0 dBFS**. The column looks like a volcanic vent; the file sounds like a short harsh sizzle.

**Summary:** v1 over-corrected for held-fire fatigue by stripping low end, fire body, tails and mid grit, then peaked the remaining tick at a normal SFX level. In game that is thin, spiky, and, with the real 13/s fire rate, fatiguing in a different way. Fire Pillar is also off — too thin in the telegraph, too harsh and too loud in the erupt — and was revised in v2.

## 2. Research (public sources only)

### PoE 2 Molten Blast

Mechanic (ExileDB / Fextralife, skill gem text, patch 0.3.0 / 0.5.5):

- “Gouge molten rock from the ground and fling it at the target.”
- The projectile **explodes on collision** (radius 1.6 m) and **scatters eight shrapnel projectiles in a cone behind the impact**.
- Attack speed is 75% of base; dual wielding fires two projectiles at 15% less attack speed.

Implied layers (from the skill’s own beats, not from ripped audio):

1. **Cast / gouge-and-throw** — a short scoop of rock plus a punchy fling, not only a cloth whoosh.
2. **Projectile whoosh** — a brief travel body (in Hellord the glob is small and very fast, so this stays short and lives in the cast).
3. **Heavy explosion** — the satisfying beat: low thump, fire body, debris.
4. **Shrapnel impacts** — several smaller hits / drips (our `magma_throw_splat_*`).

Official skill-audio practice, *Skill Sound Design in Path of Exile 2*, Dominic Downing (GGG), 18 Jun 2025, <https://www.pathofexile.com/forum/view-thread/3794832>:

- Skills that **repeat in quick succession** (Barrage and similar) have to be tested in those states.
- “Combat in Path of Exile is chaotic, so we always ask: what is most important for the player to hear? Sound should **guide, not suppress**.”
- Mixing tools: ducking, distance attenuation, a priority system.
- Projectile example from the same article (soul bolts on Last Lament, as a method, not a sound to copy): initial shot layers, a travel loop if needed, and an impact “made up of vocal textures, **debris, and various thick impacts**,” with pitch and gain variation.
- A later in-game mixing pass balances loudness and “how big or small things feel.”

A GGG video titled “Working on Molten Blast | Path of Exile 2” exists (<https://www.youtube.com/watch?v=xdboNcCX-p4>) but was **not downloaded or measured**.

### D2 / D2R Druid Molten Boulder

Public mechanic and flavour (d2r.org, Amazon Basin, diablo2.io, diablo2.diablowiki.net, Fextralife, Blizzard patch 2.4 notes):

- Flavour: “a huge, rolling mass of magma, bowling over smaller enemies in its path before **bursting into fiery shards**.”
- Launch a boulder of flaming-hot magma; it rolls ~2 s in D2R (~14 yards), leaves a **fire trail**, knocks back small/medium targets, and **explodes** on expiry or when it hits something it cannot push, leaving ~18 lingering fires for ~1.4 s.
- The original Diablo Wiki write-up jokes that “you’ll need to supply your own bowling-pin sound effect” — i.e. the skill is meant to feel like a fiery bowling ball with a real detonation at the end.

Implied layers:

1. **Launch** — weight plus ignition, a shove not a puff.
2. **Roll loop** — heavy grinding rumble **with crackling fire** (and grit), for the whole travel.
3. **Crunch on enemies** — body impacts as it bowls through a pack.
4. **Stop / crumble / burst** — a real end: debris plus a lingering fire sizzle, not a tick.

Hellord’s Infernal Boulder is **smaller** (about player-sized) and can have several rocks at once under a held click, so the same layers apply at a smaller scale, and the loop/hit must stay quieter when stacked.

### D2R fire skills and Fire Pillar

Public flavour and mechanics (skill wikis / diabloforfans Fissure page):

- **Firestorm** — falling, swirling flames; a downward whoosh plus crackle.
- **Fire Wall / Blaze** — a continuous roaring / crackling wall of flame.
- **Inferno** — a sustained gout of flame (stream), not a single tick.
- **Fissure** — “open volcanic vents below your enemies”; repeated ground eruptions.
- **Volcano** — a roaring volcanic vent, lava and rock, longer presence.

Hellord’s Fire Pillar **looks like a Fissure / Volcano vent**: a ground mark, then a standing flame column. v1’s telegraph was a thin hiss and the erupt was a short harsh sizzle that clipped in the capture. v2 revises both hooks: a low rumble/crackle buildup, then a rising roar with sub, fire body, debris and a sizzle tail, at a **quieter peak** so stacked columns do not clip.

Reddit discussions of D2R “weak / not heavy enough” remaster hits (e.g. r/Diablo, Aug 2021) were read as a general warning — lacking low-end body is a common complaint — not as a source of audio.

### Layer recipe taken from the above (original synthesis only)

| Beat | Weight / sub | Fire body / crackle | Debris / grit | Sizzle tail |
|---|---|---|---|---|
| Spit cast | small low-mid punch only | hint | hint of grit | no |
| Spit land / explosion | yes, main | yes | yes | yes |
| Spit splat | small | hint | little | short |
| Boulder launch | yes | yes | rock grit | short |
| Boulder roll loop | mid-low rumble, not sub-only | continuous crackle | grinding grit | — |
| Boulder hit | yes | burst | crunch | short |
| Boulder stop | yes | dying fire | crumble | yes |
| Pillar telegraph | low rumble | crackle buildup | — | — |
| Pillar erupt | yes | roar + crackle | debris | yes |

## 3. v2 loudness plan (from the capture, not from file peak alone)

- Keep the **overall mix quiet**, as VH prefers.
- Put the **weight in land, hit and erupt**, not in the 13/s cast.
- Lower crest (more RMS for the same or a **lower** peak) so files sound heavier without stacking to 0 dBFS.
- Suggested file peaks: casts about **−18 dBFS**, splats / telegraph about **−17**, loop / launch / stop about **−16**, land / hit / erupt about **−15** (erupt was −12 in v1 and clipped in game).
- Suggest modest hook volumes in FILES.md (casts ~0.5–0.65, loop ~0.3–0.45, erupt ~0.55–0.7).

## Sources cited

- GGG, “Skill Sound Design in Path of Exile 2,” Dominic Downing, 18 Jun 2025, <https://www.pathofexile.com/forum/view-thread/3794832>
- ExileDB, Molten Blast, <https://exiledb.com/gems/molten-blast>
- Fextralife, Molten Blast, <https://pathofexile2.wiki.fextralife.com/Molten_Blast>
- d2r.org, “Molten Boulder - How It Works in D2R,” <https://d2r.org/mechanics/molten-boulder>
- Amazon Basin Wiki, Molten Boulder, <https://www.theamazonbasin.com/wiki/index.php?title=Molten_Boulder>
- diablo2.io, Molten Boulder, <https://diablo2.io/skills/molten-boulder-t4123.html>
- Diablo Wiki, Molten Boulder, <https://diablo2.diablowiki.net/Molten_Boulder>
- Fextralife, Molten Boulder, <https://diablo2.wiki.fextralife.com/Molten_Boulder>
- diabloforfans, Fissure, <https://diabloforfans.com/skill/fissure>
- In-game captures listed above (mix / timing / spectrum / stills only)
