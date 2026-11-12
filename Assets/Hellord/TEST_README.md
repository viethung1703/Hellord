# v0.0.11 - NaN error flood and vanishing sprites

**Root cause 1: the console flood** (`transform.localScale ... 't' is not valid`, position `{NaN,NaN,0}`, `FromEulerRad` asserts). This one is certain from reading the code. The "t" objects are pooled Pixel FX Pack TellFX slots. A pooled effect slot kept its old `a0` value when it was reused. `DustPuff` writes NaN into `a0` (that means "no direction"). When the boss charge later reused that slot for `SpeedLines`, the speed value stayed NaN forever. NaN fails every comparison, so the early-out never fired, and each frame 5 streaks with NaN length, position and rotation were pushed into Transforms. The charge effect loops, so this ran every frame and slowed the Editor.
Fix (pack TellFX.cs, shipped inside this package):
- `Alloc` now resets a0/a1/rad/w/spd/last.
- `SpeedLines` zeroes a0.
- `Put`, `Seg`, `RBox` and `TickSpeed` refuse non-finite values.

**Root cause 2: enemies or the boss invisible or half-drawn after being hit or getting a status.** This is the most likely cause, found by reading the code. I could not reproduce it in a headless web run. Elites, the boss, and up to 8 burning, chilled or cursed enemies get a `PixelFX` component. Every frame it read the renderer's property block and wrote it back. Unity's SpriteRenderer keeps the sprite's texture (`_MainTex`) in that block, so a stale texture got re-applied. When the sprite switched to another sheet (for example the boss normal sheet to the bloodrage sheet), the new frame was cut from the old sheet. The result was an empty or partial sprite, such as only a head.
Fix:
- `PixelFX.Apply` now always writes the current sprite's texture.
- `DetachFx` re-seeds the sprite after clearing the block.

Please check:
1. Play to the boss (Shift+4). Fight until rage at 25% HP and use every skill.
2. The console should have no NaN or "is not valid" errors.
3. The boss and elites should stay fully drawn when hit, burning, chilled or enraged.
4. If any sprite still vanishes, tell me which enemy, which skill, and whether it had a status effect.

# v0.0.10 - skill visuals fix

**Root cause (Fireball invisible):** in `HellordSkills.cs` SpawnProj, `p.sr.gameObject.SetActive(true);` sat at the end of a `// v0.7: ...` comment on the same line, so it never ran. The pooled projectile renderers are created inactive, so every fireball (and every fork/split child) flew, hit and dealt damage while staying invisible. Only the impact puffs showed. This bug has been there since v0.7, so it is in v0.8, v0.0.9 and the web build.
**Not the cause:** the Pixel FX Pack shaders work under URP 2D. Their passes are untagged, which URP treats as SRPDefaultUnlit, and their materials ship in Resources. In a Wine run of the v0.8 player, the Storm Ring aura (HellFX) rendered correctly. Player.log shows no shader or material errors.

Fixes:
- The SetActive call now runs on its own line. As a guard, a live projectile is re-activated every frame.
- New **Shift+9 / F9 FX self-check** line: `FX: proj visible/alive, orbs, hellfx jobs, aura`. A red `FX WARN` appears if Fireball fired but no projectile is visible, the orbit has 0 visible orbs, Storm Ring has no aura, or Curse Rune cast with no HellFX jobs alive.

Please check: pick each of the 5 skills, open Shift+9, and confirm "FX check OK" and that the effect can be seen.

# Hellord prototype v0.8 — Ox Pit floor (grid rooms, doors, minimap, shrine, secret room)

**Status: compile-checked only** (Unity 6000.0 DLLs, Input System + legacy; 0 errors, 0 warnings). Never run in the Editor/Play mode. Please play-test in Unity 6000.3.16f1. Mock-ups: `Builds/v08_preview/` (composites from exported art, NOT screenshots).

## What's new in v0.8
- **Floor:** each run picks one of 3 hand-made layouts (9, 11 or 8 rooms). The rooms are Start, Survive, Hunt, Nests, Curse, Treasure, Elite, Shrine, Secret and Boss. Room art is baked by `tools/bake_rooms_v08.py`.
- **Doors:** every pair of orthogonal neighbours is connected. In a fight room the doors play the close animation and stay locked until the goal is done. Push into an open door: 0.25 s fade out, change room, 0.25 s fade in. You arrive 20 px inside the opposite door. When you leave a room, pickups are auto-collected and leftover enemies are removed.
- **Room goals:**

  | Room | Goal |
  |---|---|
  | Survive | 60 s |
  | Hunt | 100 kills at 1.6x spawn rate |
  | Treasure | 40 kills, then a chest (+20 bones and a support pick) |
  | Nests | kill 3 nests (each spawns 2 enemies every 2.5 s) |
  | Elite | the elite and 8 guards, then the elite chest = pick 1 of 3 supports |
  | Curse (optional) | step on the altar: -20% max HP, 45 s at 2x rate, then +30 bones and a support pick |
  | Boss | Club Jailer. When it dies the stairs appear; step on them = WIN screen |

- **Secret room:** clear the room below it, then push the cracked north wall once to break it and again to walk in. It has 30x24 chests (15–25 bones each) and an elite chest (support pick). It is hidden on the minimap until found.
- **Bone shrine shop:** 3 supports priced 20–40 bones. Keys 1–3 buy when you are within about 110 px of the shrine. A support fills a free slot, or swaps out the last support of a skill it fits.
- **Minimap** (top right) shows the layout: current, cleared and seen rooms, plus unknown neighbours. **Goal box** (top centre) shows the time, kill count or nests left, and SHOP/CLEAR.
- **640x360 screens:** level-up/card pick (hover lifts a card; click or keys 1–N; Esc = skip for +10 bones), death and win. All are drawn with the ui_ art and the pixel font.
- Difficulty rises with time in the room and with the number of fight rooms cleared.

## Keys (changes in bold)
F1 level-up · F2 elite · F3 transformation · **F4 teleport to boss room** · F5/F6/F7 juice · **F8 clear current room** (was room swap) · F9 debug text · **F10 reveal map** · K kill counter · Tab aim · **1–3 shrine buy** · **Esc skip cards (+10 bones)** · R restart (death/win).

## Checklist per room type
- [ ] **Start:** no enemies, all doors open, the minimap shows the neighbours.
- [ ] **Survive:** doors lock on entry and the timer counts down from 60. At 0 the doors open and CLEAR shows.
- [ ] **Hunt:** spawns are visibly denser, the kill counter reaches 100, then the room clears.
- [ ] **Nests:** 3 nests take damage and spawn enemies near themselves. The dead nest stays as remains. The counter shows "N LEFT".
- [ ] **Treasure:** after 40 kills the chest appears. Walking onto it gives +20 bones and opens the card screen.
- [ ] **Elite:** the elite and its guards spawn. The room clears when all are dead. The chest gives 3 support cards.
- [ ] **Curse:** doors stay open until you step on the altar. Max HP drops by 20%, then 45 s at a hard rate, then the reward. You can also just skip it.
- [ ] **Shrine:** stand near the shrine, offers show, 1–3 buy. The price shows red if you are too poor. Check that SWAP really replaced a support (F9).
- [ ] **Secret:** cracked wall → broken → enter. Chests open on touch. The south door leads back.
- [ ] **Boss:** the boss door looks different. The fight is the same as v0.7. After the kill the stairs show; step on them = WIN, then R restarts.
- [ ] **General:** the fade never gets stuck. No way out of a locked room (walls/doors). The camera stays inside small rooms. The cards, death and win screens are readable at 2x/3x.

## Known limits / simplified
- The transition is a fade, not a camera pan. Leftover enemies are deleted on room change.
- A support picked from the cards goes to the first skill that fits. The shop uses the shrine room icon (there is no support icon art).
- The boss room reuses `oxpit_room_clip`. The secret room's north hole is decorative. If you leave an unfinished goal through F4, it restarts on re-entry.
- No new art. Untested in Unity: door placement, the room-wall art lining up with the door sprites, and the balance numbers are all best guesses.

---

# Hellord prototype v0.7 — Ox Pit room, room camera, HUD Option B

**Status: compile-checked only** (Unity 6000.0 DLLs, Input System + legacy). Never run in the Editor/Play mode. Please play-test in Unity 6000.3.16f1.

## What's new in v0.7
- **Ox Pit rooms (pixel-exact from the room json):** normal play happens in `oxpit_room_lanes` (1536x864). The boss fight moves you to `oxpit_room_clip` (1536x864). Each room has a baked floor+walls+props image plus `<room>_data.json`, made by `tools/bake_oxpit.py`. Both were checked against the reference renders (max diff 1/255). The 5 braziers are animated sprites. There are 28 lights: warm pools, brazier light and gate glow are **additive**, shadow pools are **multiply** (new `Hellord/LightOverlay` shader).
- **Walls block movement:** north wall 48 px, side walls 24 px, south cap 16 px. This applies to the hero, enemies and the boss. Enemies spawn only on the floor.
- **Boss room:** the red test walls are gone. The bone drop-gate sits on the north gate at room px (960,0).
  - Boss north limit retuned: feet stop 16 px below the wall base, so the head may overlap the wall face (3/4 view).
  - North-wall stun impact lands on the wall base.
  - **Call the Herd:** the gate shakes, then opens with an additive glow, and 3 oxen run **south out of the gate** down the x=984 lane. Stand off that lane.
  - The horde is cleared when the boss arrives.
- **Camera:** on each axis, if the room is not bigger than the view it is centred and still; otherwise it follows the hero, clamped to the room. Pixel-snapped, integer zoom as before.
- **HUD Option B** (640x360 canvas, same integer zoom), laid out from `hud_layout_optionB_640x360.json`:
  - HP widget, XP/level bar, goal box (survive timer), minimap (2 cells: lanes room → boss room), blood/bone counters.
  - Boss bar under the goal box.
  - All text uses the 5x7 pixel font.
  - No low-HP pulse: the "low" frame is shown below 30%.
- **Gore Charge tell** redrawn in the TellFX v1.8.4 *outline* style: 1 px dark ring, thin crimson wash, gold clock front, chevrons, white blink at the end. It is no longer a filled red strip.

## Keys
F1 level-up · F2 elite · F3 transformation · F4 boss · F5/F6/F7 juice toggles · **F8 swap room (lanes/boss)** · **F9 debug text + skill slots** · **K kill counter on/off (off by default)** · Tab aim.

## What to test
1. Walk into every wall (hero, horde, boss charge). Do you ever clip into the wall art or get stuck on a prop? Props have **no collision** yet.
2. Camera at the room edges: no black outside the room, no jitter, still pixel-crisp at 2x and 3x?
3. Lights: do the warm pools and shadows look like the mock-ups? Is the shader missing on your render pipeline (pink, or flat white/black boxes)?
4. Boss: does the boss stop at a believable spot under the north wall? Is the herd lane fair? Does the gate glow show?
5. HUD: is it readable at 1080p (3x) and 720p (2x)? Do the numbers line up in their boxes?
6. Red imps around the red hero: can you always find yourself?

## Known limits / untested
- Never run. The light shader is a plain built-in-pipeline CG shader. On URP it may fail (there is a code fallback: flat alpha tint, not exact).
- Props are baked into the floor image, so actors always draw **over** props (no y-sort against them) and lights draw over props.
- The oxen run on a fixed straight lane. Cards and the death screen still use the old 960x540 IMGUI.
- HP widget tick marks are the art's 4 segments (25% each), not every 5 HP.


---

# Hellord prototype v0.5 — Pixel FX Pack v1.8.4 test build

**Status: compile-checked only** (built against Unity 6000.0 DLLs, both Input System and legacy input). Not yet run in the Editor/Play mode. Please play-test in Unity 6000.3.16f1.

## What's new
- Bundles **Pixel FX Pack v1.8.4** (same GUIDs as before, so it upgrades the old pack in place; no duplicate scripts). New pack files: `TellFX.cs`, `Documentation/HELLORD_FX_MAP.md`.
- **Damage numbers:** every hit now shows a number; repeated hits on one enemy merge into one rising total (HordeFX mergeKey). Drops register as number obstacles so numbers don't cover pickups.
- **BossHit sparks** on every non-DoT hit on elites and the boss.
- **Big-hit juice:** hits ≥60 dmg (or crits >40) add a screen kick + 35 ms hit-stop (rate-limited to once per 0.15 s).
- **Burn / Chill status FX** (flame crown, frost glint, ice shards when frozen). Tint conflict fixed: `StatusFX.TintTargets = false`; the game keeps owning the enemy colour tint.
- **TEST BOSS – Ox-Head (placeholder block art, not final)**: spawns once at 1:00 or on F4. A red test arena locks you in while it lives (fodder spawns at 40%).
  - Gore Charge: lane telegraph → charge with speed lines + dust → slams the wall: wall impact, stun stars + punish glow, **3 s stun, takes 2× damage**.
  - Horn Sweep: cone telegraph, then a hit.
  - Stomp: 3 expanding rings — step over the ring gaps.
  - Call the Herd: a stampede lane across the arena at your height — move off the lane.
  - Bloodrage under 50% HP: red trail, 1.4× faster and shorter tells.

## Keys
F1 level-up · F2 elite · F3 transformation · **F4 spawn test boss** · **F5 juice hit-stop on/off** · **F6 stun wobble on/off** · **F7 impact frames on/off** (states shown on the HUD).

## What to test
1. Do the merged numbers read well in a big horde, and do they stay clear of drops?
2. Does the big-hit kick/hit-stop feel good, or is it too much? Compare with F5 off.
3. Burn/Chill overlays: is the enemy tint correct, with no flicker between our tint and the pack's?
4. Boss: are the tells readable and fair? Does the wall stun feel rewarding? Try F6/F7 during the stun.
5. Performance with a full horde plus the boss.

## Known risks / untested
- Never run in Play mode: boss hit boxes, timings and arena size are first guesses.
- Burn/Chill overlay sizes use the enemy radius; they may sit wrong on some sprites.
- Hit-stop is short and rate-limited, but it could stack with other HitStop calls.
- No arena walls outside the boss fight; the arena is drawn around wherever you were when the boss spawned.
- Not done: world dim, camera punch-in (would break pixel-perfect), screen shockwave, hero recoil.
- Hero art is unchanged.

---

## v0.4 — Pixel FX Pack v1.6.0 + ReactionFX

- Bundles **Pixel FX Pack v1.6.0**. It uses the same GUIDs as v1.5.1, all 44 old assets match and the only addition is `ReactionFX.cs`, so importing it over v1.5.1 upgrades in place.
- Effects now come from ReactionFX: Frost Cleave arc, Frost Shatter on frozen deaths, Shock status, Bleed status, Overload explosion and Elite Chest Open.
- If a ReactionFX call fails because the pool or cap is full, the v0.3 stand-ins play instead.
- All damage is still our own code. The ReactionFX hit hooks are not used because they need Collider2D and Hellord enemies have no colliders.
- Shock and Bleed statuses are stopped on enemy death, on despawn (more than 30 units away), on pool reuse, when they expire, when Overload consumes a shock, and on run reset.
- **Known limit:** Overload returns 0 both when it fails and when nothing was hit, so we can't tell if it failed. The small pooled chain zaps always play with it.

**Please check:**
1. Frost Cleave shows an ice arc.
2. Frozen kills shatter.
3. Shocked enemies have zaps and bleeding enemies drip.
4. When a horde of more than 32 is shocked, the extra enemies show the old zaps.
5. Burn plus Shock causes an Overload blast with a shake.
6. Opening the elite chest plays a gold column.
7. No FX is left floating after an enemy dies or despawns, or after you press R to reset.

# Hellord Prototype v0.3: test notes

**v0.3:** the Hellord Art export (manifest v1) is now in the game, and Pixel FX Pack v1.5.1 effects are wired in following HELLORD_FX_MAP.md.
- **Art:** hero base, skins, upgrade layers and transformations; enemies, pickups, projectiles and the floor. PPU is **16** everywhere. Sheets are point-filtered and uncompressed, sliced at runtime from the manifest tables in `HArt.cs`, with code placeholders if a sheet is missing.
- **Hero:** idle uses frames 0-7 at 8fps, walk uses frames 8-15 at 12fps.
  - Each owned skill adds its upgrade layer: Fireball→hellfire, Frost Cleave→frost, Storm Ring→shock, Bone Orbit→bleed, Curse Rune→curse. 2 or more Crit picks add the glass horns.
  - Owning Frost Cleave uses the frost skin.
  - **F3 (debug)** cycles the transformations infernal → lich → bloodfiend → none. Transformations hide all upgrade layers.
- **Enemies:** walk frames 0-7 at 10fps, flipped when moving left, frame 8 on hit. Death still uses the HordeFX dissolve.
  - Mapping: imp→imp, ghoul→shambler, brute→knight, elite→wraith. It lives in `EnemyArtMap` (`HArt.cs`), the one place to swap it (for example elite→tinted knight). Balance is unchanged.
- **Pickups:** blood small/medium/large for XP 1/5/25, plus bone shard / skull, animated at 6fps. They run on HellFX pooled pickups (bob, magnet, collect splat), and fall back to our own sprites when the 600-sprite FX budget is full.
- **Projectiles:** art at 14fps, rotated to velocity, with HellFX trail and the pack's impacts. The floor is the tiled `floor_style14`.
- **Camera:** pixel-perfect integer zoom of a 384×216 target. No CRT.
- **FX:**
  - Storm Ring uses pooled ChainFX bolts.
  - Bone Orbit visuals are `HellFX.OrbitWeapons`, recast only on level or support change, with hits from our own logic at the orbit positions.
  - Curse Rune uses RuneCircle plus a lingering AuraField zone.
  - Burn, chill and freeze statuses show via PixelFX burn/frozen/pulse on elites and up to 8 other enemies.
  - Shock zaps, bleed drips, Overload (nova + chain), the Shatter burst and the chest drop/open combos all go through `HFX` (`HellordFXHooks.cs`).
- **Art status:** imp, shambler, wraith and hound are flagged **ai_generated** in the manifest and are used here only for testing, as approved. Medium/large blood and the bone skull are marked "not yet reviewed".
- **Old mock-up PNGs:** v0.2's files (`imp.png`, `ghoul.png`, `brute.png`, `elite.png`, `hero_face_*`, `hero_back_bleed`, `hero_head_hellfire`, `proj_fire`) are no longer used. You can delete them from `Assets/Hellord/Resources/HellordArt` after import.

# (v0.2 notes below still apply)

**v0.2 fix:** v0.1 failed to compile (`HellFXRunner` not found) because the project had an old Pixel FX Pack (v1.0/1.1, no HellFX/HordeFX/ProjectileFX). v0.2 bundles Pixel FX Pack v1.5.1 with the same file IDs, so importing it upgrades the pack in place. The Burst `Hellord.Editor` error was a side effect and should disappear.

This is a throwaway gameplay prototype. All art is the existing Hellord mock-up sprites (v6 hero layers, v7 enemies). There is **no AI-generated art**, and the code falls back to placeholder sprites if a sprite is missing.

## Import order (Unity 6000.0+ / tested compile target 6000.0.85f1, URP 2D project)
1. Import `Hellord_Prototype_v0.3.unitypackage` (import **all** items). It contains Pixel FX Pack v1.5.1, so no separate pack import is needed; an older pack in the project is overwritten in place.
2. No need to delete `Assets/Hellord` first (same file IDs; v0.2 overwrites v0.1). If errors persist, delete `Assets/Hellord`, reimport.
3. Open `Assets/Hellord/Scenes/Hellord_Prototype.unity` and press Play.
   Alternatively, use the menu **Hellord ▸ Create Prototype Scene** (or *…and Play*).
- Both input backends work: the old Input Manager, the new Input System, or Both. Gamepad support needs the Input System package.

## Controls
| Action | Keyboard/Mouse | Gamepad |
|---|---|---|
| Move | WASD / arrow keys | Left stick |
| Aim | Mouse (hold/move) | Right stick |
| Toggle Auto ⇄ Mouse aim | Tab | – |
| Pick card | 1–5 or click | (mouse) |
| Reroll (1 free per level-up) | Space | – |
| Restart after death | R / button | – |
| **Debug:** grant a level-up | F1 | |
| **Debug:** spawn an elite | F2 | |
| **Debug:** cycle hero transformation | F3 | |

How aiming works: while you move the mouse or push the right stick, skills aim there. Otherwise they target the nearest enemy. Tab locks aiming to Mouse mode.

## What to look for
- **Run start:** pick 1 of the 5 skills: Fireball, Frost Cleave, Storm Ring, Bone Orbit, Curse Rune. The hero sprite layers change with the skills you own.
- **Skill levels:** max Lv5. Each level opens one support slot (up to 4). Lv5 gives a ×1.5 damage spike.
- **Skill slots:** slot 2 opens at player level 5, slot 3 at level 12. The HUD shows locked slots.
- **Supports:** cards show which skills accept them. Skills that can't take a support are greyed out with a ⚠ reason, e.g. a wrong tag, no open slot, or a conflicting support.
- **Projectile order:** Pierce, then Fork, then Bounce.
- **Damage formula:** base(level) × (1+Σincreased) × Π more × crit.
- **Reactions:** Burn + Shock causes **Overload** (an explosion). Hitting a frozen enemy causes **Shatter**.
- **Level-up:** stats are applied automatically, then you get 3 cards. One card is always a skill-level card, and you get 1 free reroll.
- **Enemies:** imp (fast), ghoul and brute (tanky). HP = Base × (1 + 0.05 × minutes).
- **Elites:** about every 2 minutes. An elite kill gives hitstop, shake and a nova, and drops a chest (pick 1 of 3 supports).
- **Pickups:** blood XP drops with a magnet, plus bones.
- **Juice:** hit flash, damage numbers, dissolve on death, and Pixel FX Pack effects.
- **Performance:** the top-right counter shows FPS and live enemies. Push past 300 enemies (survive or press F2) and watch the frame rate.

## Implemented vs stubbed
- **Implemented:** everything listed above.
- **Stubbed:**
  - No meta-progression. Bones are only counted.
  - No audio.
  - No pause menu.
  - The UI is IMGUI placeholder.
  - Balance numbers are guesses.
  - Only some supports have bespoke visuals.

## Known risks / untested
- The code was compiled against the Unity 6000.0.85f1 DLLs plus the shipped Pixel FX Pack v1.5.1 scripts, as separate assemblies matching the asmdef split (Unity 6000.3 DLLs are not available here) with **0 errors and 0 warnings**, but it has **never been run inside the Editor**. Expect tuning and visual issues.
- **Lighting:** a Global Light 2D is created via reflection. If sprites look black, add a Global Light 2D yourself.
- **Scene file:** the ready scene was hand-built from the pack's demo template. If it fails to open, use the menu instead.
- **Sprite pivots and frame slicing** are set in code and may be off by a pixel.
- **Card clicks** go through IMGUI. If a click doesn't register, use the number keys.
- **HitStop** changes `Time.timeScale`. The game pauses through its own mode flag, so the two shouldn't conflict, but that hasn't been verified.

## v0.6 (Hellord Art export integration)
- Enemies: imp_v2 / shambler_v2 / wraith_v2 / brute; new Splitter (spawns after 0:30, splits into 2 children 27x25 at death frame 4, ±13px). 4-dir walk rows, code hit-flash (material swap, StatusFX tint off), death anim then sprite hidden. Pivots from json.
- Hero: final2 (previews/hero_s14_final2) idle+walk, runtime 1px #FFDB9E outline @0.6, sorted above FX.
- Camera reference 640x360 @16 PPU.
- Boss Ox-Head Club Jailer: 72x72 sheet (pivot 36,66), anim state machine, Bloodrage sheet at 25% HP, death anim. Retune: hurt circle 24px up r≈19px, gore lane 40px, horn sweep origin ~38px reach 60px, north margin 52px with wall-stun at head. New 216x16 HP bar (normal/hit/phase/bloodrage/stunned/empty).
- Ox Pit: drop gate on north wall (shakes on herd tell, opens on stampede, closes on recover); stampede uses hazard_ox_run_east.
- Unused art: archer, hellcat, calf, herder (no gameplay yet), ox run N/S, boss hurt anim, old hound/skeleton.
- Test: F4 spawns boss. Check outline readability, splitter split timing, gore lane vs contact, gate placement.
