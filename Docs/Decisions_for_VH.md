## v0.0.95 SHIP (t1024u build) — 2026-11-06 Asia/Saigon

Package: `Builds/Hellord_Prototype_v0.0.95.unitypackage` → `C:\Users\Admin\`.

**In this build**
1. Skeleton L1 floor: War dmg/HP/respawn 9 / 55 / 4.5s; Arc 8 / 32 / 6s (compressed curves).
2. Hive never appears on floor 1.
3. Tall hive Bone Maw C (PixelLab) wired — `ui_nest` 48×56×3, hit radius 1.25 / hitY 1.6.
4. Boss lights: 16 rim + 5 fill (Diablo readability).
5. **Real smaller Ox Pit boss room** (t1022): `ShrinkBoss677` now scales baked Ox Pit too → **960×544** (BossSX 0.625); base SR scaled; no nested camera clamp. Soft runtime scale until Art remakes native art. Frozen/Burning/Horse already used this scale.

Untested on your PC — Linux autotest PASS for bossroom / hive wire / compile.

---
## v0.0.95 (t1019/t1020) — Tall hive Bone Maw C wired (2026-11-06, Asia/Saigon)

### Art
- VH picked PixelLab **C Bone Maw**. Idle from `Art/vh_approved/hive_bone_maw_C/idle.png`.
- `Resources/HellordArt/ui_nest.png` is now a **48×56 × 3** strip (idle/hurt/dead = same idle until Art adds frames). Pivot TL `(24,53)`, PPU 16.
- Old 24×24 strip backed up at `Art/export/ui/nest_old_24x24_strip.png`.

### Collision
- Nest `radius` **0.75→1.25**, `hitY` **0.6→1.6** (matches taller/wider art).

### Already in (t1014/t1016, still no package)
- Skeleton L1 buffs; hive never on floor 1.

### Package
- **Not built** — VH said fix but not build.

## v0.0.95 (t1014u/t1016u) — Skeleton L1 floor + hive never floor 1 (2026-11-06, Asia/Saigon)

### Wired (no package yet — VH "just fix it but not build")
- Skeleton Warriors L1: dmg **6→9**, HP **40→55**, respawn **6→4.5s**; curve compress `{9,12,15,18,21}`.
- Skeleton Archers L1: dmg **5→8**, HP **20→32**, respawn **8→6s**; curve `{8,10,12,14,16}`.
- Hive / Nests: **never on floor 1** (V43 map roll = 0, V42 weight gate, hand-layout strip to Survive).
- Nest spawn: restored `radius=0.75`, `hitY=0.6`, `xp=12` (were stuck in a comment — easier to hit until tall art).
- Tall/big hive art: Hellord Art tasked; not wired until VH approves.

### Not in this pass
- No general enemy HP buff, no nest HP soften on later floors, no package/delivery.

## v0.0.94 (t1012u) — Adventure-balance prototype A–D (2026-11-06, Asia/Saigon)

### Goal
Restore D2-like adventure feel: power grows as punch, not screen-wipe coverage; floors no longer auto-buff the player. VH approved A–D.

### A — Coverage → punch (L1 preserved)
- `skillLevelStep` 0.06 → **0.12** (L5 hit ≈ 1.48×; was 1.24×). `skillMaxSpike` stays 1.
- Fireball shots `[1,1,2,2,3]` (was 1→6); splash `[0.8,0.9,1.0,1.1,1.2]`.
- Storm / Frost reach `[3.2,3.5,3.8,4.1,4.4]`; Frost half-angle `[70,75,80,85,90]` (L5 = 180° forward, not full circle).
- Orbit count `[2,3,3,4,5]`, radius `[2.6,2.8,3.0,3.2,3.4]`.
- Curse runes `[1,1,1,2,2]`, radius `[2.6,2.8,3.0,3.2,3.4]`.
- Same spirit on Chain / Pillar / Whip / Spears / Grave Pool / summons / Fire Wall length growth.
- L1 Fireball still 9.2 raw (base 10 × 0.92) vs 9 HP Imps.

### B — Supports
- Bigger Area max radius ×1.9 → **×1.4** (`[1.15,1.20,1.25,1.30,1.40]`).
- Faster Casting L5 CD ×0.65 → **×0.78**.
- Echo always **1** repeat; dmg `[0.35,0.40,0.45,0.50,0.60]`. Curse Echo special rules unchanged.

### C — No free Intensity combat buffs
- `playerDmgExp49` / `playerAreaExp49` / `playerCdExp49` → **0**.

### D — Enemy HP untouched
- Floor HP growth, boss HP, density, time HP growth left as before.

### Package
- `Builds/Hellord_Prototype_v0.0.94.unitypackage` → VH PC when delivered.
- Not play-tested yet — compare early/mid/late with 1 / 2 / 5 skill builds before further enemy buffs.

## v0.0.92 (t1004u) — Fireball hit-stop off (2026-11-06, Asia/Saigon)

### Problem
Fireball Impact called `HitStopScaled(0.03f)` on every big Fire hit. Higher skill levels / supports fire many projectiles; staggered freezes felt like the game was lagging.

### Fix
- Tuning `fireballHitStop1004 = false` (default). Set true to restore the old freeze.
- Fireball / Fire splash path sets `ProjectileFX.SkipHitStop` before Impact. Screen shake + kick stay.
- Curse / Hellfire Impact hit-stop unchanged unless they go through the same Fire skip path.
- Keeps v0.0.90 light-stamp FPS fix and v0.0.91 Fireball hit FX (no purple Glow disc).

### Package
- `Builds/Hellord_Prototype_v0.0.92.unitypackage` → VH PC `C:\Users\Admin\`.

### Untested
Not play-tested in VH's URP Editor.

# Decisions for Viet Hung
No one waits on these. The team goes with the "assumed for now" default, keeps it easy to change, and keeps working. You decide, and answered items get removed. Only publishing, uploading, deleting work and big style overhauls wait for you.

## Art style
**Palette**
- Q: Which palette rule applies to the art?
- Options: Endesga 32 / snap to PAL14 (design doc) / no strict snap (HA's note)
- Rec: Endesga 32, because it's already your stated rule. (Medium confidence.)
- Assumed for now: Endesga 32

**Hero export**
- Q: Is the current hero file approved? The doc says it's the rejected 80% version, and HA says it's final2. It measures about 18px tall against a target of about 20px.
- Options: Keep it / re-export at 100%
- Rec: Re-export at 100%, because the size is under target.
- Assumed for now: keep the current file until you rule

**Glow level**
- Q: How much CRT glow?
- Options: A / B / C (B for now)
- Rec: B, because it follows your "less glow" note. (Low confidence. Judge it from a clip.)
- Assumed for now: B

## Scope
**Vertical slice**
- Q: Which level is the slice?
- Options: Level 10 Ox Pit / Level 1
- Rec: Ox Pit, because its art and FX are nearly done.
- Assumed for now: Ox Pit

**Unique enemies on Levels 2 and 4**
- Q: Keep Needle Wisp (Level 2) and Shard Bat (Level 4) as the only unique enemies on those levels?
- Options: Yes / add more
- Rec: Yes for now, because that keeps the art queue smaller.
- Assumed for now: yes

**Boss sizes**
- Q: Recheck the other bosses' 64, 96 and 128px sizes against the new size-32 enemies?
- Options: Yes / no
- Rec: Yes, because Ox-Head needed a resize.
- Assumed for now: recheck them

## Progression
**Pacing**
- Q: Which pacing tools?
- Options: Earned support slots / trade-off supports / a Spirit cap on auras and orbitals (any mix)
- Rec: No rec. This is a pure design call.
- Assumed for now: the current system, unchanged

**Level-up heal**
- Q: Heal on level-up?
- Options: Heal 10% / no heal
- Rec: No rec. This is a balance call.
- Assumed for now: the current behaviour

**Rerolls**
- Q: Should rerolls cost bones after the free one?
- Options: Yes / no
- Rec: Yes, because it gives bones another use.
- Assumed for now: one free reroll, then bones

**Moving supports**
- Q: When can supports move between skills?
- Options: Any time mid-run / only at bone shrines
- Rec: Only at shrines, because it makes the choice matter.
- Assumed for now: only at shrines

## Boss room
**Boss entry**
- Q: How does the boss appear when you enter? It currently spawns off-screen at the north centre, about 0.02s after you enter (HT, v0.0.22).
- Options: Spawns on screen with an intro / keep it as it is
- Rec: On screen with an intro, because players should see it before it attacks.
- Assumed for now: the current off-screen spawn

**Layout-1 boss shortcut**
- Q: Keep the shortcut to the boss in layout 1? (Listed as open in the test notes, with no details found yet.)
- Options: Keep / remove
- Assumed for now: keep the current behaviour

**Pillar**
- Q: The fallen pillar is painted into the floor art, so enemies walk over it. Should it be solid?
- Options: Solid / walk-over (current)
- Rec: Solid, because it gives cover from the lane and cone attacks. (Medium confidence.)
- Assumed for now: walk-over (current)

**Arena size**
- Q: The prototype arena is 352x192, and the doc's Ox Pit is 1536x864. Which size?
- Options: Doc size / prototype size / something in between
- Rec: No rec. It needs a playtest.
- Assumed for now: the prototype size (352x192)

## Skins
**Frost skin**
- Q: Owning Frost Cleave currently swaps the hero to the frost skin. Keep that?
- Options: Keep / layer only, no skin swap
- Assumed for now: keep the current behaviour

## Level floors and shrine (HA mock-ups, not exported)
**Art palette in use**
- Q: HA reports you approved the 55% palette blend on 26 Oct. Is that the rule, instead of Endesga 32 or PAL14?
- Assumed for now: the 55% blend

**Level floor details**
- Q: Are these OK? Ox Pit lane tiles are 48x64. L1 has 5 traps against the wall. L2 has curved shear tracks. L4 has silver mirror frames. L8 has glassy cyan ice. L9 has amber oil.
- Assumed for now: as listed (mock-ups for all 19 levels are in Art/previews/step11_level_floors/ALL19_lineup_MOCKUP.png)

**Shop placement**
- Q: Where does the shop go?
- Assumed for now: inside the shrine room, lower right. The use spot's proximity pulse is an existing MM FX.

## FX (Hellord FX)
**Hook chain cooldown**
- Q: How long is the hook's cooldown? The doc gives no number.
- Assumed for now: 4s

**Judge timings**
- Q: Are the Judge's tells OK? Tongue 0.55s, pull 0.6s, slam 0.8s, no tell under 0.25s. Tongue reach 112px, slam radius 40px, 3s stun. Fake warnings are dashed, wobble and have no fill.
- Assumed for now: as listed

## Playtest (HT, v0.0.22)
**Ox-Head phase 1 can be fully dodged**
- Q: A scripted dodger took 0 hits in phase 1 over 3 seeds. Is that OK?
- Options: OK, phase 1 is the learning phase / add an unavoidable-unless-perfect pattern
- Rec: OK for now, because phase 1 teaches the tells. (Low confidence. It's a feel call.)
- Assumed for now: keep it as it is

**Ox-Head horn sweep cone**
- Q: How big is the cone? The doc says 130 degrees and 100px reach, and the 52px boss check says 56-64px.
- Assumed for now: 110 degrees with a 60px reach

**Hook Thrower warning time**
- Q: How much warning before the throw?
- Assumed for now: 0.95s in total (0.45s wind-up plus 0.5s lane tell)

## Build v0.0.23 (Grok Bot, overnight)
**Low-HP warning strength**
- Q: How strong should the red edge pulse be when your HP is low? In v0.0.22 it barely showed (the edges got only about 2% redder).
- Options: keep the faint v0.0.22 pulse / a clearer, brighter red pulse on the screen edges only / stronger still
- Rec: the clearer pulse. It shows on dark floors, the middle of the screen stays untouched, there's no strobing and it beats at most 1.5 times a second.
- Assumed for now: clearer pulse (colour dark brick red, strength 0.35). Tunable in HellordTuning (lowHpPulseColor, lowHpPulseAmt).

**Edge glow and low-HP warning without the CRT filter**
- Q: With the CRT filter off (-nocrt), v0.0.22 had no edge fog and no low-HP warning at all. Should the no-CRT view get them?
- Options: yes, same edge fog and pulse drawn as a simple overlay / no, keep the no-CRT view plain
- Rec: yes. Players who turn the filter off still need the low-HP warning.
- Assumed for now: yes (HellordTuning noCrtEdgeFog = on). It looks a little stronger than with the CRT, but only at the edges.

**New Pixel FX Pack effects (hook chain, Judge, Gossip Imp)**
- Q: Pack 1.9.2 adds effects for the Hook Thrower, the Silver-Tongued Judge and the Gossip Imp. None of these enemies is in the game yet. Should they be built?
- Options: build them next / wait
- Rec: wait for your go. They're new enemies and a boss, which is a design call.
- Assumed for now: the effects are only imported and not used yet.
- Update (v0.0.24): the hook chain and Gossip wind-up are now used by the Hook Thrower (see "Enemies"). The Judge and Gossip Imp effects are still unused.
- Update (v0.0.25): the Gossip Imp's shout ring and scramble effects are now used by the Gossip Imp (see "Enemies"). Only the Judge effects are still unused.
- Update (v0.0.26): the pack is now version 2.5.1. Four new effects are in the game, each with its own on/off switch: level-up, hero hit, doors locking and unlocking, and nests and stairs (see "Build v0.0.26"). Still unused: pickups, skills, chests and shrines, and the Judge.

**Level 11 Stone Crushing FX numbers**
- Q: The doc gives no numbers for these. Are they OK?
- Assumed for now: boulder radius 14px; tells are boulder 0.9s, quake 0.8s, crawler pop 0.5s; 3-4 armor plates; 4s punish window; armor breaks after a set number of hits (luring the boss under boulders is left to code); no "collapse shrinks arena" effect yet

**XP pickup look**
- Q: HA's exported blood-drop pickups are in the game now. Hellord FX made cyan and green gems instead (clearer against the red floor and red enemies, and cheaper to draw in big numbers). Which do you want?
- Options: Keep HA's blood drops / switch to FX gems / blood drops restyled for contrast
- Rec: Restyle the blood drops for contrast, because it keeps the theme and fixes red-on-red. (Medium confidence. It's a taste call.)
- Assumed for now: keep HA's blood drops, and use the FX system for the magnet, merge, pop, bar and level-up

## Enemies (Grok Bot, v0.0.24 Hook Thrower)
All numbers are in HellordTuning under "Hook Thrower". Turn the enemy on with the J key (spawns one near you) or the -hookthrowers launch option.

**Where Hook Throwers appear**
- Q: Level 1 (Liars) doesn't exist yet. Should Hook Throwers show up in the current rooms?
- Options: off unless turned on, like the tongue traps / mixed into every room now
- Assumed for now: off unless turned on (hookThrowersInRooms = false). When on, each spawn has a 4% chance to be a thrower, with at most 2 alive at once.

**Its basic stats**
- Q: The design gives no numbers for HP, speed or damage.
- Assumed for now: 20 HP, slow walk (1.6), keeps 4.5-7.5 tiles away and strafes, throws when you're within 9 tiles, the hook reaches 10 tiles, hits for 8, touching it hurts 6, gives 3 XP. Only 1 hook in the air at a time.

**What a hit does**
- Q: How hard and how far should the chain drag you?
- Options: a short tug / a drag all the way to the thrower / a drag you can fight
- Assumed for now: a drag you can fight. You're pulled at 6 tiles/s and your own movement drops to 35%. The drag stops 1.4 tiles from the thrower or after 1.6 s.

**Breaking free**
- Q: Can you break the chain?
- Assumed for now: yes. Hold away from the thrower. Every 0.25 s of pulling breaks one link, and 3 links snap the chain ("CHAIN SNAPPED"). Killing the thrower also frees you.

**Invincibility after a hit**
- Q: Can a hook catch you while you're still flashing from a hit?
- Assumed for now: no. Hooks pass through you during those frames, so they can't chain-stun you.

**Walls**
- Q: Should walls and statues block the hook?
- Assumed for now: no. The hook flies over everything for now. Blocking it would need a line-of-sight check (it could be added later).

**Art**
- Q: There's no Hook Thrower art yet.
- Options: HA draws a proper sheet / reuse the unused enemy_archer sheet / keep the placeholder
- Rec: HA draws it, using the robe-and-chain look from the design doc.
- Assumed for now: a code-drawn placeholder (purple hooded robe with a chain, marked with a yellow "PH" tag) in the Endesga 32 palette.

### Gossip Imp (Grok Bot, v0.0.25)
All numbers are in HellordTuning under "Gossip Imp". Turn the enemy on with the I key (spawns one near you) or the -gossipimps launch option. The design doc only says "Shout area + player aim is scrambled", so everything below is assumed for now.

**Where Gossip Imps appear**
- Q: Level 1 (Liars) doesn't exist yet. Should Gossip Imps show up in the current rooms?
- Options: off unless turned on, like the Hook Thrower / mixed into every room now
- Assumed for now: off unless turned on (gossipImpsInRooms = false). When on, each spawn has a 4% chance to be an imp, with at most 2 alive at once.

**Its basic stats**
- Q: The design gives no numbers for HP, speed or damage.
- Assumed for now: fragile. 8 HP (about one fireball), walks at 2.2, touching it hurts 4, gives 2 XP. While its shout recharges it hovers 3.5-6 tiles away and strafes. When the shout is ready it walks in (speed 2.8, you walk 4.5) and shouts once you're within 2.4 tiles. It gives up walking in after 4 s.

**The shout**
- Q: How often, how big and how much warning?
- Assumed for now: every 6 s (first one 2 s after it appears). The imp stands still and the warning lasts 0.6 s: a red ring grows on the floor with a bouncing "!". Then the ring goes off, reaching 3 tiles out from the imp. The ring stays where it started, so walking out during the warning dodges it. Only 1 shout warning on screen at a time.

**What a hit does**
- Q: What does "aim is scrambled" mean in play?
- Options: A) mirror: your attacks go to the mirror side (left becomes right; straight up/down flips too) / B) random: each attack is thrown off by 60-150 degrees / C) reverse: every attack goes the opposite way
- Rec: A. You can learn it and still play around it, and it's easy to see what's happening.
- Assumed for now: A, for 2.5 s. It bends every aimed attack (Fireball, Frost Cleave, Curse Rune), including your mouse or stick aim. Storm Ring and Bone Orbit don't aim, so they're unaffected. Three "?" marks circle your head while it lasts (they blink in the last 0.5 s) and "AIM SCRAMBLED" pops up once. Switch with gossipScrambleMode (0 mirror, 1 random, 2 reverse).

**Damage from the shout**
- Q: Should the shout also hurt?
- Options: no damage, status only / a small hit (for example 4)
- Assumed for now: no damage (gossipShoutDamage = 0). The scrambled aim is the punishment.

**Stacking and invincibility**
- Q: What if a second shout lands while you're already scrambled, or while you're flashing from a hit?
- Assumed for now: it does nothing in both cases. It doesn't stack, doesn't reset the timer, and can't hit you during your invincibility frames. Set gossipRefreshOnHit to make a new hit reset the 2.5 s.

**Killing it mid-shout**
- Q: If you kill the imp during its warning, does the shout still go off?
- Assumed for now: no. Killing it cancels the shout, which rewards fast reactions (gossipDeathCancelsShout = on). Changing rooms also clears the scramble.

**Ring covers the imp during the warning**
- Q: The Pixel FX Pack draws the red warning fill on top of characters, so the imp (and anything else inside the ring) is tinted for the 0.6 s warning. Is that OK?
- Options: keep it / ask Hellord FX for a version drawn under characters
- Rec: ask Hellord FX. The ring should sit on the floor.
- Fixed in v0.0.26: the pack (2.3.1) now draws the fill on the floor, under every character, so the imp is no longer tinted. An autotest checks it. The ring edge and "!" still draw on top so the warning stays easy to read. To get the old look back, set HellordTuning gossipFillSortingOrder to 2004.

**Art**
- Q: There's no Gossip Imp art yet.
- Options: HA draws a proper sheet / keep the placeholder
- Rec: HA draws it: a small imp with a huge shouting mouth.
- Assumed for now: a code-drawn placeholder (small pink imp with bat wings and a huge open mouth, marked with a yellow "PH" tag) in the Endesga 32 palette. No AI-generated art.

## Build v0.0.26 (Grok Bot)
Pixel FX Pack is now 2.5.1 (was 1.9.2). Each new effect has its own switch in HellordTuning, under "Pixel FX Pack 2.5.1 modules". Turning a switch off brings back the v0.0.25 look.

**Level-up screen effect (LevelUpFX)**
- Q: Should the level-up screen use the new effect? The hero gets a short glow, then the screen dims and the cards slide in one by one.
- Options: on / off (the old instant screen)
- Assumed for now: on (fxLevelUp). Cards can't be picked for the first 0.45 s, so a key you were already holding doesn't pick by accident. After a pick, the dim fades out over 0.12 s instead of cutting.

**Hero hit effect (HurtFX)**
- Q: When you get hit, should the whole hero still turn red, or use the new effect? The new effect shows gold brackets, a ring and sparks flying away from the hit.
- Options: new effect / old red tint
- Assumed for now: new effect (fxHeroHurt). You also get a silver ring at your feet that shrinks while you can't be hit, and up to 3 hearts over your head below 30% HP. When you die there's a soul flame.
- Not wired yet: the extra screen-edge flash on each hit. It would change the low-HP pulse from v0.0.23, so it waits for your call.

**Door lock effect (DoorFX)**
- Q: HA's doors already animate shut. Should steel bars and a padlock also slam on, with a gold burst when the room clears?
- Options: on / off (HA's door animation only)
- Rec: look at the clip. It's an extra layer on top of HA's doors, and you might find it too busy.
- Assumed for now: on (fxDoors). Doors that are off screen get a small padlock marker at the screen edge.

**Nest and stairs effect (RoomFX)**
- Q: Should destroyed nests get a collapse burst and a dull "dead" seal, and should the stairs get a gold beacon after the boss?
- Assumed for now: on (fxRoom). The animated walk down the stairs is not wired. It would change the floor transition, which is riskier, so it waits.

**Not wired yet (waiting for you)**
- PickupFX: it would take over the XP pickups, and you chose HA's blood drops, so it stays off.
- SkillFX: it would replace all the skill visuals. That's a big style change, so it waits for your go.
- InteractFX (chest, shrine and shop markers, elite marker): skipped this build for time. Rec: wire it next.

## Marketing capture (from HT, v0.0.23)
- True 60fps clips need a GPU machine. My computer renders in software, so clips get about 41 unique fps in combat and about 25 in the boss fight. Options: A) use a GPU machine you provide, B) accept the current clips for drafts. Rec: B for drafts, A before anything is published. Assumed for now: B.
- Scripted or debug captures for no-HUD stills and the missing shots (ice wall shatter, ignite spread, near-death escape). Rec: skip until marketing asks. Assumed for now: skip.

## Hook Thrower (from HT, v0.0.24)
- The weak-link snap rarely matters. Breaking free takes about 0.75s of holding away, but from the usual 4.5u throw distance the pull ends before then. Options: A) keep as is, B) make the snap shorter (about 0.4s), C) start the pull from farther away. Rec: B, low confidence. Assumed for now: A (tunable).

## Gossip Imp (from HT, v0.0.25)
- Time between shouts: the spec says 6s, but in practice it's 7.4-8.8s, because the cooldown starts after the warning, recovery and walk-in. Options: A) keep it and change the doc wording, B) start the cooldown when the shout begins. Rec: A (the imp is rare and gentle). Assumed for now: A.
- Imps (8 HP) usually die before they shout, so the scramble rarely happens (1 hit in 2 runs). Options: A) keep, B) raise HP or shout from farther away. Rec: playtest it yourself first (low confidence). Assumed for now: A.

## Support icons (from HA)
- Support icon style (HA reports VH said "ok" to B on 27 Oct; please confirm): A) gem badge, B) rune tile. Rec: B (A loses the element colours, so fire icons merge), high confidence. Assumed for now: B.
- Burning Ground vs Fire Ground (and Frost Nexus vs Chill): they overlap, and the doc gives Burning Ground no stats. Options: A) keep both, with Burning Ground as a modifier (bigger or longer fire ground), B) drop one (52 supports). Rec: A. Assumed for now: keep both.
- Tags for the 30 new supports aren't in the doc. Is there a Spell or Minion tag (for Spell Echo, Spell Cascade, Unleash)? Assumed for now: HA's tags, with Minion and Curse fixed from the doc text.
- The doc says 54 support icons, but Shatter is counted twice, so it's 53. Fix the doc line L653.

## Silver-Tongued Judge combo (from FX v2.7.0)
- Pull plus slam: the slam warning fills while the pull is still dragging the hero to its centre. A 64px slam over 0.8s can't be walked out of at 72px/s. Options: A) keep it, so it needs a dash, B) start the slam tell after the pull ends, C) shrink the slam to 48px or lengthen it to 1.0s. Rec: B or C so it can be escaped on foot, medium confidence. Assumed for now: C (48px over 1.0s, tunable).

## Enemy look (from QC of HA step15)
- The Gossip Imp's silhouette reads more like a bat (big ears, tiny horns). Options: A) keep it, B) bigger horns, less bat. Rec: B, low confidence. Assumed for now: A.

## Build v0.0.26 (your 9 fixes: assumed defaults, all tunable in HellordTuning)
Everything below was decided by me, not asked. Change any number in the HellordTuning asset (or tell me).

1. Room sizes. Q: what mix of room sizes? Rec/assumed: 45% small, 40% medium, 15% large (`roomSmallWeight/Medium/Large`). Nests and Elite rooms never use the small size (`nestsMinSize`, `eliteMinSize`). Demon count scales with room area: 1.7 demons per 100 sq. units (min 6, max 70), ramping +0.35 per "progress" unit. Goals scale by size (small x0.7, medium x1, large x1.25).
2. "No cameras rendering" in the Editor Game view. Cause: the gameplay camera draws into a low-res RenderTexture (for the CRT look), so no camera drew to the screen. Fix: an extra, enabled display camera that draws nothing. Assumed: this is the only cause. I can't open your Editor, so please check it there.
3. Floor decor. Added contact shadows under standing decor and sorting by pivot (base). Assumed: soft dark ellipse, tunable.
4. Harder. Contact damage x1.6 (`contactDamageMul`), enemy speed x1.1, boss hits x1.4 (`bossDamageMul`), hero invincibility after a hit 0.7 s (was 0.8). Boss phase 1 now: double ring, 11-bullet fans, 3 spiral arms (0.2 s interval), walks at 1.7, bullets x1.1, pattern cooldown 1.05 s / 1.0 s / 0.8 s (phase 1 / 2 / rage). Phase 2 starts at 65% HP (was 60%). Q: is this hard enough? Rec: judge after playing; bot play says a good dodger still takes only 1-2 boss hits.
5. Ranged demons in the normal mix: Hook Thrower and Gossip Imp both ON. Chance a spawn is ranged = 6% + 6% per progress unit + 5% per floor, capped at 30%, split 50/50.
6. Floors. Default 3 (`floorCount`, 1-6). Boss room has stairs; the next floor has a new palette tint and new layout (the same layout can come back). Each floor after the first: enemy HP +35%, enemy damage +25%, density +25%, speed +4%, boss HP +45%, boss damage +20%, goals +20%. Heal 40% on taking the stairs. Beating the last floor's boss is the win. Boss art is reused on every floor (no new art).
7. Room clear. When the goal is met: spawning stops, the room clears when the last demon dies, a "REMAINING N" counter shows. If a straggler survives 45 s it dies (`clearStragglerSeconds`). The old instant clear is `clearInstant` (OFF). Heal 10% on clear.
8. Banners. Important notices (goal reached, room cleared, curse lifted, boss, floor change) are big, centred, on a dark band, held 2.0 s (`bannerHold`). Minor notices stay small toasts.
9. Skill bar. Bottom centre, 3 slots (unlocked at Lv 1 / 5 / 12), cooldown sweep, skill level, support pips, locked slots with a padlock and the unlock level, "FREE SLOT" when empty. F9 debug panel unchanged. Uses your approved art: step14 skill icons 24px with cooldown style A (12 sweep frames, `skillCdFrames`), cooldown digit size `skillCdDigitScale` (1), ready flash 0.15 s (`skillReadyFlash`), support icons from step13 at 20 px (`skillBarSupportPx`, 8-20). Q: is the 20 px support size too large? Assumed: keep 20 (approved size); set 10 for a lower bar. The padlock, sweep masks, digit font and FREE SLOT frame are code-drawn placeholders (PH).

Test changes: the elite-room test now needs surviving 3 s (was 5) with 4+ hits, because the harder contact damage kills a standing hero at ~3.5 s; the rage dodge threshold is 35 s (was 45). The dev flag `-goalscale N` shrinks room goals for recording and testing; it does nothing unless passed.

## Build v0.0.27 (assumed for now)
Your v0.0.26 feedback: too easy, leveling too fast, melee/area skills weak, boss never touches you, only Fireball + supports needed. Everything below was decided by me, not asked. Every number is in the `HellordTuning` asset/class (old value in brackets). Naming used: build folder `Builds/linux_v0.0.27`, package `Builds/Hellord_Prototype_v0.0.27.unitypackage`.

**Important about your tuning asset.** The repo `HellordTuning.asset` is field-less (402 bytes), so the new defaults in code apply. Importing the .unitypackage REPLACES your local asset with that field-less one. If your Unity-saved asset has serialized values, they would override these defaults, so import the package (or delete the old asset) to get the new numbers.

1. Leveling, about 70% fewer level-ups. XP to next level = (5 + 10 x L^2) x `xpCostMul` (1) [was 5 + 4L + L^2/4] (fields `xpBase` 5, `xpPerLevel` 0, `xpQuad` 10). Lv2 needs 15 XP, Lv3 45, Lv4 95, Lv5 165, Lv6 255, Lv10 about 1005. Front-loaded on purpose: a fresh hero gets its first pick after about 15 kills (a steeper start left the test bot at Lv1 and dead in the first room). Expected: about 3-4 level-ups by the end of floor 1 [11-12], about 8 over 3 floors. The gain per level-up is also smaller: max HP +1.5 [+2], damage +0.5% [+1%], speed +0.25% [+0.5%], heal on level-up 6% [10%].
2. Skill slots unlock at Lv 3 and Lv 7 [Lv 5 and Lv 12] (`slot2Level`, `slot3Level`), because levels come slower.
3. Stat cards: max HP +7% [+10%], speed +5% [+8%], armor +4 [+5], crit +4% [+5%].
4. Skill level step: each skill level adds +6% damage [+30%] (`skillLevelStep`); the Lv5 spike is x1.0 [x1.5] (`skillMaxSpike`). Skill levels now mostly grow COVERAGE.
5. Skill damage per hit (new `skillDmgMul`): Fireball x0.92 (not lower: at x0.75 a Lv1 shot, 7.5, no longer one-shots a 9 HP Imp and the early game collapsed), Frost Cleave x0.85, Storm Ring x0.8, Bone Orbit x0.8, Curse Rune x0.8 [all x1].
6. Coverage by skill level (Lv1/2/3/4/5):
   - Fireball: 1/2/3/4/6 projectiles [1], 13 deg spread, splash radius 0.8/1.0/1.25/1.5/1.8 u, splash = 40% of the hit.
   - Frost Cleave: range 2.5/3.3/4.2/5.1/6.2 u [2.3], half-angle 55/75/100/130/180 deg [65] (Lv5 = full circle), knockback 4, chilled targets take x1.25. The pixel pack caps its arc art at 2.5 u, so big swings are drawn as two arcs plus ice bursts on the real edge (the hit area is the real size).
   - Storm Ring: radius 2.2/3.2/4.4/5.6/7.0 u [2.6].
   - Bone Orbit: 2/3/4/5/7 bones [3], orbit radius 1.8/2.5/3.2/3.9/4.6 u [2.0], bone hit radius 0.5 u [0.45].
   - Curse Rune: 1/1/2/2/3 runes [1], radius 1.7/2.2/2.8/3.4/4.2 u [1.8], extra runes scatter within 3.2 u.
7. Supports are upgradable, Lv 1-5. Picking an owned support upgrades it (in level-up cards, chests, shop, swap). Level-up offers always include one upgrade when you own one that fits. The card shows "UPGRADE LV a - b" or "NEW LV 1" plus the effect text; the skill-bar support pip shows a small digit (yellow at Lv5). Per level (Lv1/2/3/4/5):
   - Multi Projectile: +1/+2/+2/+3/+4 projectiles [+2 flat], each x0.88 damage [x0.75].
   - Pierce: 1/2/3/4/5 enemies, 15% falloff per pierce.
   - Fork: 2/3/3/4/5 children, damage x0.55/0.6/0.65/0.7/0.75 [x0.7], 28 deg spread.
   - Bounce: 1-5 bounces, range 4/4.5/5/5.5/6 u.
   - Homing: turn 4/6/8/10/13, range 4/5/6/7/8.5 u.
   - Area: x1.25/1.4/1.55/1.7/1.9 area.
   - Echo: 1/1/2/2/3 repeats at x0.5/0.6/0.6/0.7/0.7 damage.
   - Orbitals: 1/2/3/3/4 extra bones.
   - Burn and Bleed strength: x1.2/1.6/2.0/2.5/3.0, burn lasts +0.5 s per level.
   - Chill: slow time 1/1.3/1.6/1.9/2.3, freeze after 3/3/3/2/2 hits.
   - Shock: bonus damage taken +25/30/36/43/50% (innate shock without the support: +20% [+25%]). Behaviour change: shock now lasts a flat 3 s [3 x strength, 4.5 s with the support].
   - Added Damage: x1.15/1.22/1.3/1.38/1.5 [x1.3]. Faster Cooldown: x0.9/0.84/0.78/0.72/0.65 [x0.75].
   - A support added by old code paths without a level counts as Lv1 (tests still valid).
8. Demons: speed x1.375 [x1.1] (about +25%), boss phase-1 walk speed 2.3 [1.7]. Attack rate x1.4 (`enemyAttackRateMul`): ranged cooldowns tick 40% faster. Demon touches: hero i-frame after a touch is 0.7 / 1.15 = 0.61 s (`contactIFrameDiv`) [0.7 s]; 1.4 (0.5 s) was too lethal in testing. Other damage keeps 0.7 s. Ranged wind-ups slightly faster but still readable: Hook whirl 0.4 s [0.45], Hook lane tell 0.45 s [0.5], Gossip shout tell 0.55 s [0.6].
9. Boss melee can hit (all in `HellordTuning`, `boss*` fields):
   - Horn reach 7.5 u [6.25]; the cone tell is drawn at the real reach.
   - Walk rest between attacks 0.9-1.7 s [1.2-2.2].
   - Wind-up tracking: the boss creeps toward you at 2.6 during the horn wind-up and, halfway, re-aims the cone once (second visible tell, at least 0.4 s).
   - Follow-up swing: if you sidestep the first sweep and are within 8.5 u, 85% chance of one more swing with a 0.5 s tell.
   - New lunge: when you are out of reach (always) or farther than 5 u (55% chance), the boss crouches 0.55 s with a red lane on the floor (locked line), then dashes at speed 17 up to 9 u (1.2 u past you), hit width 1.5 u, damage x0.75 of boss hit. After the dash it always does a follow-up swing (0.5 s tell). Sidestepping the lane dodges the dash; the follow-up is what catches dodgers.
   - Rage speeds all of this up as before.
10. Test changes: slot-unlock autotest uses `slot2Level`/`slot3Level`; the support-add test goes through the new upgrade path.
11. Autotest result for v0.0.27: 165 pass, 2 fail before the fix (`v26_clear:spawning_stops`, `one_left_not_cleared`). Cause: the test's "harmless" ghouls now died in 5 s because the new coverage skills reach them. Test fixed (ghouls get 50000 HP, kill damage 1e7); this is a test change, not a game bug. Not re-run after the fix (compile checked only).

## Build v0.0.36 (assumed for now, all tunable in HellordTuning "v0.0.36" sections)
No one waits on these. Everything below is untested in play; art is PLACEHOLDER only (no AI art).

**New skills (5, Lv1-5, added to the start pick and level-up pools)**
- Chain Lightning (tags Hit + Area): the bolt jumps to the nearest un-hit demon. Jumps 2/3/4/5/7 by level, jump reach 3.2 to 5 u, 8% damage lost per jump, innate Shock. I gave it Area (not Projectile) so Bigger Area widens the jump reach and Echo repeats it, instead of offering Pierce/Fork/Bounce/Homing that a bolt cannot honour.
- Hellfire Pillar (Area + Hit): pillars land on the densest demon cluster near you after a 0.5 s orange rune warning. 1/2/3/4/6 pillars, radius 1.3 to 2.2 u, innate Burn.
- Blood Whip (Melee + Area + Hit): long lash toward your aim (5 to 9.5 u reach, 1/1/2/2/3 lashes fanned 28 degrees), innate Bleed, heals 0.4 to 1 HP per demon hit, max 3 HP per cast.
- Soul Spears (Projectile + Hit): pierce-through spears (1/2/3/4/5 spears, built-in pierce 1/2/2/3/4, small damage loss per pierce). Multi, Pierce, Fork, Bounce and Homing supports all work on it.
- Grave Pool (Area + Zone + Hit): drops a puddle where you stand, but only if you moved 1.4 u since the last one and demons are within 12 u. Radius 1.3 to 2.6 u, lasts 3/4/5/6/8 s, slows demons 30 to 50% and ticks damage every 0.5 s. At most 10 pools alive. Bosses are not slowed.
- Skill damage multipliers for the new five: 0.8 / 0.85 / 0.85 / 0.85 / 0.8 (newSkillDmgMul).

**New supports (5, Lv1-5, no Longer Duration)**
- Executioner (Any): 2.0x to 3.0x damage on demons below 10/15/20/25/30% HP (Lv1 to Lv5). I read "10% up to 30%" as the HP threshold, with a bonus multiplier. Not applied to elites/bosses (executionerSkipBigs).
- Life Leech (Hit): heals 2 to 6% of damage dealt, capped 1 to 3 HP per second.
- Knockback (Hit): extra push of 2 to 6.5 on every hit (bosses ignore it, Grave Pool ticks do not push).
- Soul Harvest (Any): kills by that skill drop +1 to +3 XP and a 5 to 25% chance of a bone.
- Explode on Kill (Hit): a kill by that skill explodes, radius 1.4 to 2.4 u, 35 to 70% of the demon's max HP + 3 (capped at 30). Explosions do not chain. Kills from burn/bleed ticks do not count as skill kills (only direct skill hits).

**Which skill gets a support**
- A support card now says "TO: <skill>" when one skill can take it, or "TO: PICK 1 OF n" when several can (an upgrade of an owned support counts as an option for that skill).
- Picking a support card with several options opens a second pick: one card per skill (name, UPGRADE/NEW level, effect text, slots used), keys 1-n or mouse, ESC goes back to the cards.
- Shop (BuyShop) works the same: with several options you pick the skill before the bones are spent; ESC or walking away cancels.
- Chest uses the same flow. The old IMGUI debug card screen and the autotest still auto-pick the first fitting skill.

**Placeholders**
- Skill icons: existing icons re-tinted with a 2-letter tag (CL, HP, BW, SS, GP).
- Support icons: stand-ins from the approved 20px support sheet: Life Leech (own), Knockback (own), Executioner = Ruthless, Soul Harvest = Thrill of the Kill, Explode on Kill = Detonate. Supports without a 16px card icon now show the 20px icon at 2x on the cards.
- FX are pack effects (chain bolt, rune circle + hellfire nova, pixel beam, soul-tinted bone projectile, tinted aura field). SFX only reuse existing clips; spear/whip/pool casts are silent.

## Build v0.0.37 (assumed for now, all tunable in HellordTuning "v0.0.37" sections; nothing here is tested in game)

**What changed**
- Floors 2 and 3 now look and play differently. Banners read "FLOOR n OF 3 - OX PIT / FROZEN HELL / BURNING HELL".
- Floor 2, Frozen Hell: cold blue rooms; ice patches in fight rooms (you slide, keep momentum for a moment); frost demons (blue Imp/Ghoul/Brute/Splitter, 22%) whose hits slow you for 1.6s; boss is a Frost Ox-Head with a telegraphed blue ice-breath cone added to the cycle (hit = damage + slow).
- Floor 3, Burning Hell: red/orange rooms; lava cracks that sleep, flash a warning, then burn for a few seconds (only in fight rooms, never near the start spot, centre or doors); exploding demons (Imp/Ghoul, 16%) that light a visible fuse when close, slow down, then blast you AND other demons; boss is Horse-Face: pink/bone recolour of Ox-Head, faster tempo, shorter tells, 11u spear thrust line, spear combos and charge combos, shorter stun. It is not just more HP.
- Floor 1 and 2 bosses give a big reward after the death animation: a pick of 3 cards (new skill slot if you have fewer than 3, up to 2 supports with upgrades first, or a skill level) plus bones. The pick cannot be skipped and the stairs are blocked until it is done. The slot card opens a new-skill pick right after.

**Placeholders (all recolour in code, no new art)**
- Room art, props, demons and bosses are re-mapped by brightness onto Endesga 32 ramps (frost blue, fire red/orange, bone/magenta for Horse-Face). Ice patches, lava cracks, fuse rings, the ice cone and the spear lane are flat shapes.
- The slot card uses the fallback room icon. No art exists for it, nor for a new Horse-Face or frost/exploding demon; those need real sprites later.
- SFX reuse existing hooks only (frost cleave, curse rune, fireball impact, chest open, Ox-Head sounds), kept quiet.

**Assumed for now / things to judge when you playtest**
- Because the skill bar has 3 slots and level 7 already unlocks the third, the slot card only shows when you are below level 7. After that the reward is supports and a skill level.
- Frost/exploding chances, slow strength, lava timings and the Horse-Face tempo are guesses. Check that lava is readable before it burns and that ice is not annoying.
- Floor 3's Horse-Face reuses Ox-Head's room and HP growth; its sheets are recoloured on the fly, which may look muddy.

## Build v0.0.38 (test menu; assumed for now; compile checked only, not tested in game)

**What changed**
- New in-game TEST MENU so you can test without beating levels. Open it with **F1** or the small **TEST** button (top left, under the HP bar). It **pauses the game** while open. Close with F1, Esc, or CLOSE. It is in every build for now; set `testMenuEnabled` to false in HellordTuning to remove it for release.
- Controls: mouse click any button, or keyboard: arrows move, Enter / Space press, Tab (or PageUp / PageDown) change tab.
- FLOOR tab: jump to floor 1 / 2 / 3 start; jump to this floor's boss room; jump to a Shop, Treasure, Elite, Curse, Survive, Hunt, Nests or Secret room (if the floor's layout has none, a plain fight room is converted); start room; reveal secret rooms.
- PLAYER tab: +1 level (opens the level-up pick), +5 levels, full heal, god mode, +100 bones, game speed x2.
- SKILLS tab: give any of the 10 skills, level a skill up, remove a skill.
- SUPPORTS tab: pick the target skill, then click a support to give it (only if it fits and has a free slot, else the reason is shown) or to level it up (max 5). Also clear supports and add a skill level for more slots.
- ENEMIES tab: spawn Imp, Ghoul, Brute, Splitter, Frost Imp / Brute, Exploding Imp / Ghoul, Elite, Hook Thrower, Gossip Imp (x1 / x5 / x10); kill all demons; clear room (opens doors); kill the boss.
- BOSS tab: spawn Ox-Head / Frost Ox-Head / Horse-Face (jumps to that floor, then its boss room); kill the boss; open the boss reward pick; boss room of this floor.
- KEYS tab lists these controls and the older debug keys.
- **F1 no longer gives a level** (it used to). Use PLAYER > +1 level.
- Soul Spears, Blood Whip and Grave Pool casts now have a quiet sound: existing clips reused (fireball cast, frost cleave, curse rune), pitched up / down. Pitches: `spearSfxPitch`, `whipSfxPitch`, `poolSfxPitch` in HellordTuning; volumes are low in HellordAudio.

**Assumed for now / limits**
- Giving a skill needs a free slot (max 3 on the bar). If you are below the level that unlocks the slot, the menu unlocks it for you.
- If the menu is opened on the starting-skill pick, any action leaves that pick (and gives Fireball unless you gave a skill yourself).
- x2 speed also speeds up boss tells, so use it for clearing waves, not for judging fairness. God mode only blocks damage.
- Jumping floors builds a fresh random layout and keeps level and skills. Jumping to a cleared room resets that room so its fight starts again.
- Not tested in game: the menu layout on your screen size, mouse clicks through the OnGUI layer, and each jump. Tell me what looks off.

## Build v0.0.42 (pacing pressure; built in v0.0.42, kept; not playtested)

**What changed**
- **No more HP growth over time.** `enemyHpPerMinute` is now 0 (was 0.05). I also found a hidden 5% per minute on boss HP in the code and moved it to a new `bossHpPerMinute` = 0. Enemy and boss HP now grow only per floor. Note this makes bosses a bit softer than in v0.0.41, more so the later in the run you meet them.
- **Shrinking drops** in rooms where demons keep spawning with no kill or time limit. By default that is only the **Nests room** (destroy the 3 nests). In normal rooms nothing changes: they end when every demon is dead. Hunt and Treasure stop spawning at their kill goal, and Survive and Curse end on a timer, so they cannot be farmed forever. They are not listed, but you can add them to `dropDecayRooms`.
- Defaults: after **45 s** in the room, XP and bone drops lose **50% of full value per further 60 s**, down to **10%**. Small drops round randomly, so on average you still get the right amount. Nest rewards themselves (the objective) never shrink.
- Feedback is subtle: smaller drops use the smaller blood-drop sprite. There is no UI or text.
- Tunables in HellordTuning: `dropDecayEnabled`, `dropDecayGraceSeconds`, `dropDecayPerMinute`, `dropDecayMinMul`, `dropDecayRooms`, `bossHpPerMinute`.

**Assumed for now**
- The decay clock is time since you entered the room, and it applies while the room is active (also after all nests are down, until the room clears). Not compiled or played yet.

## Build v0.0.42 (random floors: door-choice path + room shapes REJECTED by VH, off since v0.0.43; only shuffled theme order kept)
VH: "the hell level should random, not linear". All four options are on by default; each one has a switch in HellordTuning ("v0.0.42 Random floors").
- **Theme order is random.** Ox Pit / Frozen Hell / Burning Hell are shuffled per run (`randomFloorOrder`). A theme keeps its boss, hazards, demon variants and palette. Difficulty (HP, goals, density, hazard damage, boss rewards) still grows with DEPTH (1st / 2nd / 3rd floor). `firstFloorAlwaysOxPit` is off by default. Music: there is only one loop, so `themeMusicPitch` lets each theme get its own pitch. It is 1/1/1 by default, which changes nothing.
- **One run seed.** `runSeed` = -1 picks a new seed every run. The `-mapseed N` flag still works. The seed and theme order are logged and shown in the F1 FLOOR tab. Set the seed to replay the same run.
- **Rooms are a path, picked through doors.** Start, then 4-6 steps (`pathRoomsMin/Max`), then Boss. Each cleared room offers 2-3 exits (`doorChoicesMin/Max`), and each exit shows its room's icon and name. Rules:
  - The first room is a plain fight.
  - The last step always offers a Shop, and 1 extra Shop is offered mid-floor.
  - No special type (Nests / Elite / Curse / Treasure / Shop) appears in two steps in a row.
  - Elite / Curse can appear from step 2.
  - Per-floor caps apply.
  - There is a 60% chance of a secret room behind a cracked north wall.
  - You can't go back.
  - The type weights start from the old layout mix.
- **Room shapes.** Fight rooms of about 900 u² and up can be:
  - a plain rectangle,
  - an L (one corner pit),
  - a plus (four corner pits),
  - 1-3 pillar pits.

  Pits never sit near doors, the spawn, the centre, or nest / elite spots, and the walkable area stays inside `roomWalkAreaMin/MaxU2`. Pits are solid for feet; shots fly over them. Spawn density uses the reduced area.
- **Placeholder look (no new art).** Pits are a flat dark fill with the existing fence / skull post props along the edges. Real L / plus room art would need new baked rooms.
- **Old hand-made layouts.** They are kept: turn off `randomRoomPath` to get them back.
- **F1 FLOOR tab.** It has jump-to-depth and jump-to-theme buttons. If the theme isn't in this run, the current depth is re-themed. The BOSS tab buttons pick a theme.

## v0.0.43 - random floor MAP (replaces the v0.0.42 door-choice path)
- VH: "floor will random all rooms position in floor only once when player start the floor". Each floor now generates ONE map from the run seed when it starts (HellordV43Map.cs), then plays exactly like the old hand-made maps: doors between all neighbouring rooms, go back freely, normal minimap, cracked-wall secret room.
- Rules: start near the grid centre; 8-11 rooms grown as branches (15% loop chance); boss = dead end as far from start as possible (min 3 doors, never next to start); 1 shop (prefers the room before the boss / dead ends), 1-2 elites, 0-1 treasure, 0-1 curse, 1 nests, at least 3 plain fights; rooms next to the start are plain fights; elite/curse need 2+ doors from start; secret room 60% above a fight room.
- Kept: shuffled theme order, run seed (F1 FLOOR tab), v0.0.42 pacing (no HP per minute, Nests drop decay).
- Off by default: randomRoomPath (door choice) and the L/plus/pillar shapes (weights 0). randomFloorMap OFF = the 3 old hand-made layouts.
- Untested in play.

## v0.0.44 — Theme bosses + two new demons (approved brief)
- Bosses by theme: Ox Pit = Ox-Head, Frozen Hell = **Ice Gorilla** (replaces Frost Ox-Head), Burning Hell = **Qilin Chariot**. Horse-Face and Frost Ox-Head kept in code, reachable from F1 > BOSS.
- **Chill**: each hit = 1 stack, -15% speed per stack; 3 stacks = frozen 1.0 s, then 3 s immune; decay after 3 s without a hit. Blue tint + pips + frozen overlay.
- **Frost Caster** (~12% of Frozen spawns): keeps 5–8.5 u away, 0.6 s tell, slow ice bolt (+1 Chill, 5 dmg), 2.6 s cooldown.
- **Fire Wheel** (~14% of Burning spawns): 0.7 s spin wind-up + lane tell, rolls up to 12 u / wall, 1.6 s fire trail, 1.2 s vulnerable recovery.
- Ice Gorilla `BLSGRLBG` (enraged 50%: `GBLGSR`): Boulder→7 shards, Leap smash, Belly slide (ice trail), Glacier Slam (16/24 icicles, shadow tells), Roar (+2 Chill).
- Qilin Chariot `CFWCDM` (enraged 50%: double charge): wall-bounce Charge + fire trail, 90° sweeping Breath, 2 bouncing Wheels, Drift ring collapsing with 50° safe gap, 5–7 Meteors.
- All numbers in HellordTuning (v0.0.44 block). Placeholder visuals only — **VH: real art for Gorilla, Qilin, Caster, Wheel requested Oct 28 (VH OK), see Asset requests below.**

## Status Oct 28 (VH)
- Built and delivered: v0.0.41 (per-floor growth: demon HP 25%, dmg 15%, speed 2%, boss HP 20%, boss dmg 15%, boss atk rate 10%), v0.0.42, v0.0.43 (random fixed floor map, shuffled theme order, no HP-per-minute, Nests drop decay).
- v0.0.44 code compiled (0 errors), not packaged, not played: Ice Gorilla (Frozen), Qilin Chariot (Burning), Frost Caster + Chill/freeze, Fire Wheel. Horse-Face kept for a future floor.
- Workflow: VH reviews a skill-set preview before code; batch edits; build only when VH says.

## Asset requests (sent Oct 28 with VH's OK)
Hellord Art (HA): Ice Gorilla boss, Qilin Chariot boss (fire qilin + burning war cart), Frost Caster demon, Fire Wheel demon (roll/spin), Frozen Hell + Burning Hell room tiles/props (currently code recolours), icons for Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool and supports Executioner, Life Leech, Knockback, Soul Harvest, Explode on Kill, hero Chill pips / frozen overlay.
Hellord FX (HFX): Gorilla (icicle fall + shadow, icicle shatter, slam ring, boulder + shards, leap ring, slide ice trail, roar wave), Qilin (charge fire trail, burning wheels, flame breath cone, closing fire ring, meteors), Frost Caster ice bolt + charge glow, Fire Wheel spin sparks + fire trail, chill/freeze on hero, lava cracks, ice patches, the 5 new skills' VFX (currently placeholder).

## v0.0.44 — Pixel FX Pack + Ice Gorilla art wired (VH: "Wire in all FX and the Gorilla art, PixelLab is fine")
- Wired IceGorillaFX, QilinChariotFX, MinionFX, StatusHazardFX, HeroSkillFX (PixelCraft is used inside them) per HELLORD_FX_MAP.md. Toggle `fxV44Pack` (on). Off = plain fallback decals, with no hero-cyan, white freeze disc or amber/orange colours.
- Removed placeholders: lava warning blink, opaque freeze disc + cyan chill pips, hero-cyan chill bursts, RuneCircle pillar tell (+ HellfireNova eruption), Blood beam whip, AuraField grave pool (ticks now run in UpdatePools44), roar Sin(t*20) strobe, cyan slam/leap/roar bursts ("Frost Cleave cyan slam" read as these + the cyan frost-slow burst), #feae34/#f77622 wheel, bounce, trail and drift colours.
- qWheelRadiusU 0.6 → 0.8. New tuning: fxV44Pack, gorillaSheet, gLeapHeightU, gLeapShadowU/PeakMul/Alpha, fxIceSkidEveryFrames, qEmbersS, qWheelTellLenU, qDriftTellS.
- Ice Gorilla uses the PixelLab sheet (Resources/HellordArt/boss_ice_gorilla.png). Shows tell frames during tells, the hit frame on impact and airborne frames in the leap. Toggle `gorillaSheet`. If the sheet is missing, the recoloured Ox-Head is used.
- Compiled with 0 errors. Not packaged and not played yet.

## v0.0.44 status (2026-10-28)
- BUILT: tools/linux_build.sh v0.0.44, 0 compile errors; packaged Builds/Hellord_Prototype_v0.0.44.unitypackage.
- Low walls + per-side doors wired (toggle lowWallsV30; lowWallSidePx=32, lowWallBottomPx=24; lowDoorGlow/lowDoorGlowK). Ox Pit v30d, Frozen/Burning v32, Ox Pit N doors from step21. No rotation/flip. Colliders, door triggers, spawn bounds, hero clamp, hazards follow the new wall thickness.
- New icons wired (toggle newIconsV44): 5 new skills on the bar + level-up cards (24px), Executioner / Soul Harvest / Explode on Kill supports (20px, cells 53-55). Life Leech + Knockback keep old icons. Old tinted stand-ins return when the toggle is off.
- Untested in play: wall art not tinted by grade; edge props may overlap thicker walls; N door sits over the baked N wall; glow centred on the wall band.

## v0.0.45 status (2026-10-28)
- PACKAGED: Builds/Hellord_Prototype_v0.0.45.unitypackage; scripts 0 compile errors (box Linux player step fails on a missing editor dll, unrelated to the game).
- Qilin Chariot invisible: root cause = importer capped every HellordArt texture at 2048 px; the 1440x2736 Qilin sheet shrank, HArt could not slice 171 160x144 cells and drew an 8 px placeholder. Fix: importer keeps sheets >2048 px at real size (AllowBigSheets45, importer version 45); Qilin falls back to recoloured Ox-Head if slicing ever fails. Only the Qilin sheet is >2048.
- HUD (hudCornerV45): skill bar bottom-left; LV/XP left x = HP (hudXpBarX 4); blood/bone under LV (hudCounterLeftPx/TopPx); minimap x1.5 (hudMinimapScale); goal box x2 (hudGoalScale); floor name OX PIT / FROZEN HELL / BURNING HELL right-aligned under the map (hudFloorName, hudFloorNameScale 2, hudFloorNameGapPx 1); FLOOR n/N label off (hudFloorNumber); NO MORE DEMONS banner off (goalMetBanner).
- Untested in play: Qilin reimport in Unity, all HUD positions.

## v0.0.46 status (2026-10-29)
- PACKAGED: Builds/Hellord_Prototype_v0.0.46.unitypackage. Scripts compile with 0 errors. The box Linux player step still fails on the missing editor dll, which is unrelated to the game.
- Boss tells (HellordTells46.cs): HFX design for cones and lanes/charges, MM design for circles and rings. The Pack's own ground tells are hidden (packTellsHideV46). Tunables: tellReleaseMinP46, oxQuakeTellRU.
- MoneyMaker fx_redo integrated (method-level swaps only; other redo files not swapped):
  - Blood Whip = v5 octopus tentacle. Toggle fxClassicWhip46 brings back the old whip.
  - Hellfire Pillar = Task 4 remake: crack tell, squash dome, tall spike, shock ring, lasting 2.2 s. Toggle fxClassicPillar46.
  - Ice patch = Ice v2. Toggle fxClassicIce46.
  - Lava crack = Lava v2 (no red). Toggle fxClassicLava46.
  - Lava tone-down: lavaActiveWidthMul46 (0.8) thins the molten channel; lavaGlowAlphaMul46 (0.6) lowers the floor glow and heat shimmer. Set both to 1 for the MM mock.
- The 1 px grid is isolated in PixelCraft1.cs (Craft1 / CraftDebris1). Only the swapped effects use it; Chain, Spear, Pool, Chill and all other Pack effects keep the 3 px look. Quad caps for HeroSkillFX and StatusHazardFX are now 24000.
- Backups: /workspace/bak_fx_int (box).
- Untested in play: none of the new FX has been viewed in Unity. Previews are from the Python mocks only. Not checked yet: performance at higher quad counts, how lava reads on the red floors after the tone-down, whip and pillar timing against the hit logic, and toggles switched mid-effect.

## v0.0.47 status (2026-10-29)
- 36x36 hero (hero_base_36, pivot 18,35) + v14 trait overlays (56x50, pivot 28,47, json draw order) + step32 chill/freeze (edge overlays, tint on all layers, frozen in/shimmer/break, 9x9 pips at 26/38/45 px). Code: Scripts/Runtime/HellordHero47.cs; art: Resources/HellordArt/Hero36/.
- Toggles: hero36V47 (off = old 32px hero + old hero_up layers + pack chill), traits36V47, chill36V47. Hit pads scale by heroHitScale47 = 1.2.
- Combo: Fireball + HellfirePillar draws HellfirePillar_combo_behind INSTEAD of both Hellfire strips (per VH; anchors json says it replaces only _front).
- Old trait sets are not drawn on the 36px hero. Transformations still use the 32px sheets.
- Glass horns (crit >= baseCrit+0.1): approved 36px step33 art (Hero36/hero_36_glass_horns, 56x50, pivot 28,47, front) replaces hero_up_glass on the 36px hero. Drawn under ChainLightning_front, over the other front traits, hidden while the ice block shows, chill-tinted. Toggle glassHorns36V47.
- Compiled with 0 errors, not playtested. Art issues: StormRing_front is empty; FrostCleave_front touches the right edge; BloodWhip_front touches the bottom; chill art is still _MOCKUP.

## v0.0.48 status (2026-10-29)
- WIRED and compiled (0 error CS). NOT packaged; waiting for VH to say build.
- Burning G floor option B (step35b soft cracked stone): new sheet Resources/HellordArt/BurningHell/burningG_floor_variants_32px_v48.png. It has 13 tiles: base_0-9 (weight 10 each) and crack/scorch/ember accents (weight 2 each). Accents never touch each other. Random flipX is on. The floor block in burning_tileset.json now points to it.
  - All tiles can spawn; the v0.0.46 skip list only applies to the old sheet.
  - Toggle: HellordTuning.burningFloorV48 (off = old v0.0.44 crack sheet). Accent weight: burningFloorAccentWeight.
- Boss music (Hellord Sound drafts) is in Resources/Audio/Music/Boss: Ox-Head 180, Horse-Face P1 140 / P2 155, Ice Gorilla 150, Qilin 160 BPM. The old Frost Ox-Head uses the Ox-Head track.
  - The boss track crossfades in when the boss spawns and back to the floor loop when it dies.
  - Horse-Face P1 -> P2 waits for the next bar line, then does a 0.25 s crossfade.
  - Tunables: bossMusicV48 (off = old floor music), bossMusicBpm[], bossMusicBarSync, bossMusicBeatsPerBar, bossMusicXfadeS, bossMusicPhaseXfadeS, bossMusicVolumeMul.
- Untested in play: the look of the floor in a room, the music switching and loudness, and whether the loops are seamless.

## v0.0.49 status: combat redesign v0.2 first pass (compiled, untested in play)
The whole pass can be switched off with `HellordTuning.combatV49` (unticked = v0.0.48 behaviour). Every number is a v0.0.49 field in HellordTuning.
**Landed**
- Camera aims for about 512×288 (`camTargetPx49`). Rooms are 300–1100 u² and Survive goes up to 1600 u². Density is 2.6 per 100 u².
- Dash: Space, pad B or RB. One dash with a 1.2 s cooldown that moves 3.2 u. It gives 0.25 s of i-frames, breaks a hook drag and shows a cooldown bar under the hero.
- Intensity is 1.5 + 0.15 × floor.
  - Enemy side: it scales enemy cooldowns, bullet counts, damage (^0.5) and boss pattern cooldowns (^0.5).
  - Player side: it scales skill damage (×1.4×I), area (×1.2×I^0.5) and cooldown (I^0.5). Fireball and Soul Spears get +1 projectile at I≥1.5 and +2 at I≥2.
- Mercy valve: losing more than 40% HP within 5 s cuts enemy fire rate by 30% for 4 s.
- Enemy bullets have their own pool (60 max). They do not cancel when the enemy dies. Storm Ring's edge and Frost Cleave erase them.
- Base demon attacks:
  - Imp: lunge, with a red lane tell (split children get one lunge from further away).
  - Ghoul: reach grab, with a ring tell and a 1 s hold that slows you to 30%.
  - Brute: charge, then a cleave, then stays stuck for 1 s.
  - Elite: blinks behind you, does an X-slash, then two offset bullet rings.
  - Frost Caster: 7-bolt fan.
  - Fire Wheel: aims where you are heading and rolls 1.2× faster.
  - Hook: faster, with a longer lane tell.
  - Gossip: radius 5, with a 1 s tell.
- Ranged kits are recolours, 20% of floor spawns, capped at 4 + 0.5 per floor. Each one spawns opposite the previous one, so you get crossfire.
  - Ox: Archer (aim-lock laser) and Imp Gunner (3-shot burst).
  - Frozen: Snowball Imp (grenade that leaves ice and chills), Ice Sniper Wraith (laser shot that chills) and Frost Brute (dash that leaves ice, then a slam).
  - Burning: Ember Spitter (two lava grenades), Flame Wraith (ring with gaps, fired twice) and Imp Gunner.
**Deferred:** Mortar theme and beat logic (Sack Lobber is built but only appears on Ox if `sackOnOx49` is set), FC ice wall, Hell-cat, Hound, Skeleton, Herder, Gossip cone, Archer elite volley, and per-skill reworks (Frost 180°, Orbit bones, …).
**Untested:** none of this has been played yet. Most likely to need tuning: the camera feel, how readable the bullets are (they use a placeholder disc sprite), dash feel on ice, and whether the Archer sheet loads (it falls back to a bone-tinted Imp).

## v0.0.49b: difficulty pass after playtest ("super hard")
New HellordTuning knobs (`*49b`), applied on top of Intensity. Turn `combatV49` off to undo them.
- **Enemy:**
  - `enemyIntensityMul49b` 0.6: enemies only get 60% of the Intensity growth. At floor 0 their I is 1.3 instead of 1.5. The player keeps the full Intensity.
  - `enemyCdMul49b` 1.35: cooldowns are 35% longer. This also applies to boss patterns.
  - `enemyDmgMul49b` 0.7: 30% less damage, including contact hits.
  - `enemyAreaMul49b` 0.8: grenade and patch sizes, grab reach, cleave, X-slash and slam are 20% smaller.
  - `enemyCountMul49b` 0.8: fans, rings and bursts fire 20% fewer bullets.
- **Player:**
  - `playerDmgMul49b` 1.3: skill damage up.
  - `playerAreaMul49b` 1.2: area up, covering Frost reach, Storm Ring, chain range and explosions.
  - `playerProjSpeedMul49b` 1.2: projectile speed up.
  - `playerRangeMul49b` 1.25: projectile lifetime (range) up.
- Not playtested yet.

## v0.0.49d: floor ladder + softer balance
- **49b softened:**
  - Enemy: Intensity share 0.85, cooldown ×1.15, damage ×0.85, area ×0.9, bullet count ×0.9.
  - Player damage, area, projectile speed and range are back to 1.0.
  - The only player buff is Bone Orbit radius ×1.2 (`orbitAreaMul49d`).
- **Floor 1 (FloorIdx 0):**
  - Rooms are 250–650 u² (Survive ×1.3). Density is 2.0 per 100 u², and the enemy Intensity is a flat 1.0.
  - Every enemy just chases you: no lunge, grab, charge or elite blink/rings.
  - Fight rooms 1–2 are melee only (Imp, Ghoul, Brute).
  - From the 3rd fight room on, a few simple ranged enemies can appear (Archer or a 1-shot Imp Gunner): 8% chance, +3% per further room cleared, max 2 alive at once. Hook Throwers and Gossip Imps can appear from then too.
  - From the 5th fight room on, Splitters can appear.
- **Floor 2+:**
  - All patterns and full kits are on (`complexFloor49d`).
  - Rooms are 400–1100 u², and the max grows by 150 u² per floor after floor 2.
  - Density is 2.6 per 100 u², +0.3 per floor after floor 2.
  - Ranged-kit chance is 20%, +5% per floor after floor 2.
- **Knobs:** all `*49d` in HellordTuning. Not playtested.

## v0.0.49e: Mortar enemy art wired (PixelLab v14)
- Sheets are `Resources/HellordArt/enemy_bloater.png`, `enemy_rice_ghost.png` and `enemy_grinder_monk.png`. Each is 38×32, 8×12, pivot (19,27), PPU 16, with the same row layout as Frost Caster.
- Loader is `MortarArt49e`, in `HellordMortarArt49e.cs`: `Frames(kind)`, `Index(kind, anim, dir, t)` and `FlipX`. The SheetDefs are in HArt. The toggle is `mortarSheets49e`.
- These enemies don't spawn anywhere yet: there's no Mortar floor and no debug spawn. No combat behaviour was added.
- Art weak spots, not fixed:
  - All three read smaller than Frost Caster.
  - The Monk's hat is hidden at the top of the overhead-lift cast.
  - The Ghost's cast E frame 4 is soft.
  - Ghost cast_N has 9 frames, so the 9th is cut at 8 columns.

## v0.0.49f — early-game fix (VH: "why player skills still buff / first room still range + many types")
Root causes: (1) floor themes were shuffled with `firstFloorAlwaysOxPit=false`, so floor 1 could be Frozen/Burning; (2) `ApplyVariant37` (+ApplyVariant44) turned melee spawns into Frost Caster / Fire Wheel / frost / bomber variants on any floor; (3) player base buffs playerDmgMul49 1.4 / playerAreaMul49 1.2 × Intensity (1.5 on floor 1) were still active.
- Floor 1 always Ox Pit (`floor1ForceOx49f`). Floors 2+ still shuffled.
- Floor 1 fight rooms 1–3: Imp/Ghoul/Brute only, simple chase (`floor0MeleeOnlyRooms49d`=3). Room 4–5: simple Archer/Gunner kits ramp in (chance 0.08 +0.03/room, max 2). Hook/Gossip/Splitter/variants only from room 6 (`floor0OtherTypesRooms49d`=5, `floor0VariantRooms49f`=5). Rooms 250–650, density 2.0. Elite patterns off on floor 1 (Complex49).
- Player: all broad multipliers = 1 on floor 1 (`playerScaleFloor49f`=1); from floor 2 player scaling uses relative Intensity (I/1.5 → floor 2 ≈1.1). Only kept buff: Bone Orbit AoE ×1.2.
- Caveat: an Elite room on floor 1 still spawns the Elite (chase only) + Brute/Ghoul guards.

## v0.0.49g — Fireball started with 2 projectiles
Cause: the build VH played (v0.0.49d package) used player Intensity 1.5 on floor 1, and `playerCountAtI49.x`=1.5 gave +1 projectile to Fireball and Soul Spears (plus dmg ×2.1, area ×1.47, cd ÷1.22). 49f already set floor-1 player Intensity to 1.0; 49g hardens it: `playerCountFloor49g`=1 → no Intensity projectiles before floor 2, and player statics reset in Clear49. Start values now = design Lv1: Fireball 1 proj, Soul Spears 1, Bone Orbit 2 bones (radius 2.6×1.2 approved), Chain 2 jumps, Storm radius 3.2, dmg/area/cd ×1.

## v0.0.49h — fewer demons per room, ramp by floor
New ladder (`density49hOn`): floor 1 1.4 (was 2.0), floor 2 1.8 (was 2.6), +0.3 per floor after. Effective with floorDensityGrowth ×(1+0.25·FloorIdx): F1 1.4 (2.0), F2 2.25 (3.25), F3 3.15 (4.35), F4 4.2 (5.6). Within-room ramp (+35% over progress) unchanged.

## v0.0.49i — smaller floor-1 rooms, demon caps, hive nerf, gentler HP
Root cause of ~60 demons in room 1: (a) `firstHuntGoal`=60 kill goal; (b) floor-1 room max 650 u² was smaller than the smallest fight art (703 u²), so all 8 rerolls failed and a random art (up to 3234 u²) was kept; Nests/Elite forced medium+ (≥1484 u²).
- Rooms F1: max 720 u² (only 640x368 = 703 u²); Survive/Nests/Elite ≤1044 (also 768x432 = 1035). Failed rerolls now fall back to a fitting / smallest art.
- Alive caps: F1 12 +2 per cleared fight room, max 20; F2 26, +6/floor. Kill-goal caps: F1 15 +2/room max 25; F2 40, +10/floor. Elite guards F1 4 (+2/floor; was 8).
- Hives: HP 150 ×(1+0.08·rooms)×(1+0.15·floor) (was 260 ×(1+0.35·rooms)); 1 demon / 3.5 s (was 2 / 2.5 s), stop at 80% of alive cap (was 200).
- Demon HP per floor +15% (was +25%). No per-room demon HP ramp exists (enemyHpPerMinute 0); per-room sponginess came from hives +35%/room and the reverted player buffs.

## v0.0.49j — floor 1 theme shuffles again
`floor1ForceOx49f` = false (49f forced Ox Pit; wrong). With `randomFloorOrder` on and `firstFloorAlwaysOxPit` off, floor 1 is Ox Pit / Frozen / Burning at random (no repeat across blocks). On any theme, floor 1 keeps: melee-only rooms 1–3, no theme demon variants (frost, bomber, Frost Caster, Fire Wheel) until 5 cleared fight rooms, no Frost Brute / Elite patterns. Theme hazards, tiles, boss and music still follow the theme. All 49g/49h/49i changes unchanged.

## v0.0.50 — Ambient hell screams (wired, compiled, not packaged)
- 6 clips (Resources/Audio/SFX/ambient/hell_scream_01..06) play off-screen on room enter: 20% chance, 1.5–5 s delay, global cooldown 20–40 s, one at a time, no immediate repeat, pitch 0.92–1.08, volume 0.3–0.5 × SFX, random side pan ≤0.6.
- Silent in boss rooms / while boss alive; any playing scream fades out (0.5 s) when boss music starts.
- Tunables (HellordTuning, v0.0.50 header): ambientScreamsOn50, screamChance50, screamCooldownMin50/Max50, screamVolMin50/Max50, screamPitchMin50/Max50, screamPanMax50, screamDelayMin50/Max50, screamMuteInBoss50, screamBossFadeS50.

## Damnation Pact UI v1.1 (focus pass, design only)
- New doc hellord_damnation_pact_ui_layout_v1.1.md and step56 mockups. No legend: three states are shown by shape and symbol. Big SELECTED n/6, one primary button per screen, one hint line, details go in an [X] drawer. Rules are unchanged (8 nodes per Path, max 6, Keystone uses a slot, 2 modes).

## UI v1.2 (step57, design only)
- Pact UI is now a compact 312x196 panel over the dimmed scene. The detail panel is a strip under the grid. The n/6 badge breaks out of the panel corner.
- The Main Menu is an atmospheric hell scene: citadel, lava, braziers, skulls, a pixel HELLORD title, plaque buttons, and a ritual-circle Pact preview. Doc: hellord_damnation_pact_ui_layout_v1.2.md.

## UI v1.3 (step58, design only)
- Overview Path cards shrunk to a 2x2 grid of 92x36 cards (about 64% less area) in a 200x124 panel. Uses the real Paths (Pyromancer of the Pit, Bone Warden, Stormcaller, Blood Reaver). Path focus panel is 272x170.

## UI v1.4 (step59, design only)
- Scalable overview: a 2-column grid of the small 92x36 cards with up to 3 rows (6 Paths). At 7+ Paths it pages 6 at a time (LB/RB plus dots). Unreleased Paths show as '?' teaser cards.

## UI v1.5 (step60, design only)
- Each Path has a 24 px icon (flame, skull+bones, storm bolt, blood drop+claw). Overview chip now reads 'n in pact' and the hint says 'Mix Paths · up to 6 nodes'. SELECT button reads 'ADD TO PACT'. The confirm screen shows a mixed loadout and Crest status.

## Damnation Pact LOCKED rules (VH) — doc v1.6, design only
- **One Pact Path at a time:** no mixing Paths. The player can add, remove or change nodes, or switch Path, between runs. Switching Path clears the Pact after a confirm.
- **Max 6 selected:** a node must be unlocked (Souls) before it can be selected.
- Unchanged: Keystone uses 1 of 6, Crest at 4+, two modes.
- The step60 mockups are approved as placeholder; HA polish comes later (do not ask HA yet). 'Mix Paths' copy is removed, and the mixed mockup (06) is marked INVALID.

## v0.0.51 — Damnation Pact in the game (code)
- New `Scripts/Runtime/HellordPact.cs` (data, save, UI, run hooks). New modes `Mode.MainMenu`, `Mode.Altar`.
- Boot goes to the **Main Menu**: DESCEND (Space), SACRIFICE ALTAR, QUIT. It shows the current Pact plus Souls and Sigils. After death or win, R returns to the menu. Autotest and botstate runs skip the menu. `pactMainMenu51=false` restores the old flow.
- **Sacrifice Altar → Damnation Pact tab** (placeholder IMGUI, step60/v1.6 layout):
  - Path list with icons on the left. The 8 Notables + 2 Keystones from catalog v0.6 sit on the right.
  - UNLOCK and SELECT are separate modes (buttons or TAB). In Select mode, a locked node shows "UNLOCK FIRST".
  - The Pact bar has 6 slots and a Crest at 4+. SWITCH PACT asks for confirmation and clears the Pact. CLEAR PACT is also available.
- **Rules:**
  - One Path per Pact; max 6; 1 Keystone max (it uses a slot).
  - Unlock costs Souls (v0.6 costs). A Keystone also costs 1 Sigil.
  - Unlock requirements: N-prereq, "or" prereqs, and K1 needs 2 Notables unlocked.
- **Hybrids are not offered.** Every v0.6 Hybrid needs a node from a second Path, which the single-Path rule forbids. **Decision for VH:** redesign Hybrids as same-Path combos, or drop them.
- **Meta save:** PlayerPrefs `hellord.pact51.*` (souls, sigils, unlock masks, path, selected mask).
- **Souls:** paid on death or win as bones×1 + kills×0.1 + 25/floor, plus 100 on a win. Sigils: 1 per win (per-boss is a tunable, 0 by default). Earnings appear on the end screen.
- **LIVE effects:**
  - Path passive per selected node: Pyro +4% inc dmg, Bone +1.5 armor, Storm +2% speed, Blood +6 max HP.
  - Crest at 4+: Pyro +15% dmg, Bone +6 armor, Storm +8% crit, Blood +30 HP.
  - Signature skill ×1.25: Kindling→Fireball, Hellfire Lance→Pillar, Marrow Launch→Bone Orbit, Grave Anchor→Grave Pool, Conduit→Chain Lightning, Storm Capacitor→Storm Ring, Whip Reach→Blood Whip, Sanguine Spears→Soul Spears.
  - Red Feast: +1 HP per kill.
  - Last Rites: cheat death once per run, back to 35% HP.
  - Martyr: +40% dmg below 35% HP.
  - Inferno Heart: +40 HP, +4 armor.
- **STUBBED (Path passive only; TODO):** the rest of the nodes' real mechanics (zones, minions, triggers, heat, blood magic, and so on). The Altar labels them "PATH PASSIVE ONLY".
- Tunables are in `HellordTuning` under the header "v0.0.51: Damnation Pact".

## v0.0.52 — Torture floors + Mortar B floor (code; not packaged)
- **Mortar B = skin, not a 4th Theme.** Ox-logic depths (never floor 1) roll `mortarChance52` (0.5) per run seed → packed-bran tiles (`Resources/HellordArt/MortarHell/mortar_tileset.json`, TileTheme 3) + mortar low walls/doors (`LowWalls/lowwalls_mortar.json`, `LowWalls.Mortar52`). Ox enemies/boss/hazards unchanged (Bloater / Rice Ghost / Grinder Monk not yet added to Mortar spawn tables).
- **Mortar props (live):** pit + animated pestle + rope beam, sacks, spilled sacks, husk piles, broken bowls, chopsticks, animated rice patches. Contact shadows are drawn under them.
- **Set pieces (live, `HellordTorture52.cs`):** v131 transparent renders (devices + humans only, no demons, no floor) repacked as 4x4 grids: Ox spiked grind-wheel trough, Burning G cauldrons (only on Burning G tiles), Mortar rice-mill (human in bowl under pestle, rope haulers). `tortureRoomChance52` per non-boss room. Each piece is placed only where it fits clear of doors, spawn and the hero.
- **Frozen ice blocks (live):** `iceBlocks52` (3) per fight room. The cyan glow is drawn in code. The 1st fireball hit (projectile or kind-0 blast) cracks a block and the 2nd shatters it (5-frame shatter, no regrow by default). Only fireballs count unless `iceAnyProj52`.
- **Stubbed:** watching demons near devices (none spawned; the rule is kept because no demon touches a device), the Monk hauling the rope (the B render has humans hauling), shootable poor souls on Mortar, collision on devices (walk-through), Mortar enemy roster and boss.
- Tunables: `// v0.0.52 torture floors` in HellordTuning. Compile 0 error CS. No package made.

## v0.0.53 — torture gaps finished (VH: "finish ALL gaps from v0.0.52")
- **Device collision**: set pieces (bottom band `tortureColW53`×`tortureColH53`), mortar pits, every ice block and watchers are solid via `_blocks42` (hero + enemies pushed out). `tortureCollide53` toggles.
- **Watching demons**: `watchers53`=2 idle demons beside each set piece facing it (B: Bloater+Rice Ghost; Ox: Rice Ghost/Grinder Monk/Bloater; Frozen: Bloater/Rice Ghost/Frost Caster; Burning: Rice Ghost/Frost Caster/Bloater). Decor only — never on or operating a device.
- **Rope Monk**: on the Mortar B set piece a Grinder Monk (cast row, facing W) hauls a rope line up to the big pulley. Only demon touching a device. Position `ropeMonkX53/Y53`.
- **Damned souls (Mortar)**: 3–4 per fight room (`damned_rice_16x24_8f`), idle/flail/scream/push anim; any projectile or blast hits: white flash + "-12", HP bar, 36 HP = 3 hits; death = squash frame + blood burst, fades after 2.5 s. No XP.
- **Mortar demons**: on Mortar floors 70% of Imp→Rice Ghost (fast, frail), Ghoul→Bloater (exploding bomber logic), Brute→Grinder Monk (tanky). Full walk/idle/hurt/death from their sheets.
- **Mortar boss**: Horse-Face pestle warden (`boss_horseface_pestle`, ox-head slot layout) on the Spear Warden kit; rage = fire recolor. ⚠ art is the code-made MOCK-UP and has no real bloodrage sheet.
- **Frozen group scene**: 70% of Frozen fight rooms place 5 ice blocks in the exact v131 group-scene layout (+watchers); each block still cracks/shatters. The baked `torture_frozen` composite is not drawn (it can't shatter per block).
- Kept: Ox no pit-fall; demons don't operate Ox/Frozen/Burning devices. Compile v0.0.53: 0 `error CS`.

## v0.0.53 (same version, UI fix) — Main menu + Sacrifice Altar now match the agreed mockups
- Was wrong: main menu was a plain text list (no castle/lava bg, no hex DESCEND, no Codex/Settings, no Souls/Sigils chip, no PACT n/6 ring); Altar was a debug list + text grid (no Path cards/icons, no SELECTED n/6 box, no UNLOCK/SELECT tabs, no detail strip, no Your Pact / SEAL PACT screen).
- Now: art baked from the approved mockup code (tools/pact_ui_v53.py -> Resources/HellordArt/PactUI, 38 PNGs). Main menu = step57 layout. Altar = step60 Overview cards -> Path screen (8 nodes + 2 Keystones, UNLOCK/SELECT tabs, detail strip, greyed UNLOCK FIRST / PACT FULL) -> Your Damnation Pact (6 slots, Crest line, SEAL PACT). v1.6 copy + switch-Path confirm. Rules unchanged (one Path, max 6, unlock before select, Keystone uses a slot).
- Codex / Settings buttons are placeholders ("COMING SOON"). No player-facing "Ascendancy". No version bump.

## v0.0.53 (same version) — Shootable torture decor + break FX
- Every projectile now hits torture props. Explosions hit everything in their radius. Each hit makes a white flash, a damage number, a squash-wobble and material chips. A kill plays a break burst, a short squash-pop and leaves a remnant decal. The prop's collision footprint is removed.
- Shootable props: Ox / Mortar B / Burning set pieces (their humans are baked into the sprite, so they die with the device and you get gore plus debris, or embers on Burning); mortar pit + pestle + beam; sacks / husk piles (grain); bowls / chopsticks (debris); rice patches; watching demons and the rope Monk (blood + embers + curse pop; the rope disappears with him). Souls and ice blocks keep their own rules but now get a stronger death / shatter burst.
- Tunables: HellordTuning `breakables53` and `brk*53` (HP per class, hit box, damage multiplier, blast damage, particle counts, shake, decal). Broken props come back when you re-enter the room. No version bump.

## v0.0.53 – Tester report B01–B18 + O1 (no version bump)
- **B01** Hints used runs of spaces that `Sanitize` collapses → now `KEY: ACTION  /  KEY: ACTION` separators (main menu, path screen, Your Pact).
- **B02** Path grid nodes show the Path icon (dim when locked); lock / check moved to a corner overlay.
- **B03** YOUR PACT: slots show the id only; full "ID NAME" list (2 cols × 3 rows) under the Seal button.
- **B04** "PATH PASSIVE ONLY FOR NOW" removed (blank unless LIVE).
- **B05** YES on the switch-Path confirm now also adds the node you clicked.
- **B06** ADD TO PACT shows disabled "1 KEYSTONE MAX" when a keystone is already in the pact.
- **B07** SELECTED n/6 on a Path screen shows 0 when browsing a Path that isn't the sealed one.
- **B08** Hint now says "ESC OR EDIT: BACK TO PATHS" (matches what Esc does).
- **B09** New editor script `Scripts/Editor/HellordPlayerSettings53.cs` sets companyName "Hellord" (if default/empty) and productName → save path `.../Hellord/Hellord`.
- **B10** TEST F1 button moved to bottom-left (was over the bones counter).
- **B11** Pact ring PNGs regenerated with an opaque inner disc (`tools/pact_ui_v53.py`) so castle windows no longer show through.
- **B12** F1 menu selection = yellow frame + dark fill (no green-on-yellow); green "on" tint skipped while selected.
- **B13/B18** "PH" placeholder tags removed from Gossip Imp and Hook Thrower sprites (skill-icon 2-letter tags untouched).
- **B14** Kills no longer count after death/win; soul payout recomputes (delta) while the end screen is up, so payout = formula on the shown stats.
- **B15** Set-piece breaks: `brkBigMul53` 1.6→2.4, gore bursts + blood decals across the width, rubble chunks, hit-stop, extra shake, 0.45 s darkening collapse. Tunables `brkBigGoreBursts53, brkBigGoreMul53, brkRubble53, brkBigHitStop53, brkBigShake53, brkBigCollapseS53`.
- **B16** Rope line(s) of torture_b now belong to the device breakable and vanish with it.
- **B17** Second solid band over the device bowl (`tortureBowlW53` 0.5, `tortureBowlH53` 0.25), removed on break.
- **O1** Floor 1 Survive: `surviveF1DensityMul53` 0.7, `surviveF1SecondsMul53` 0.8, `surviveF1DmgMul53` 0.75 (non-boss hits).

## v0.0.53 — playtest fixes (decor collision, AoE vs breakables, floor/boss music) — 2026-10-30
- **Decor no longer blocks walking.** Root cause: `Collide53` added walk-blocks for every torture device, B17 bowl, ice block, pit and watcher. `HellordTuning.tortureCollide53` now defaults to **false**. Room-shape corner cuts and pillars in `_blocks42` are unchanged and still act as cover.
- **AoE skills now hit breakables.** Root cause: every skill hits through `Hit(Enemy…)` on `Query()` results, which only returns demons. Only projectiles and blast kind 0 called the breakable hooks. New `SkillArea53(c,r,dmg,key)` damages breakables, tortured souls and ice. It is called from:
  - Frost Cleave, Storm Ring, Rune Burst, curse zone ticks, Bone Orbit (both loops)
  - Hellfire Pillar, Blood Whip (sampled along the lash)
  - Grave Pool ticks
  - every blast kind
  - Chain Lightning: arcs into up to `brkChainJumps53` props, and can fire when only props are near

  Each prop has a per-skill cooldown `brkAoeCd53` (0.25 s). New tunables: `brkAoe53`, `brkAoeCd53`, `brkAoeDmgMul53`, `brkChainJumps53`.
- **"Decor becomes a big enemy".** No code turns a prop into an enemy. The decor watchers and rope monk around torture devices are drawn with the same live sheets as the Mortar demons (bloater, rice ghost, grinder monk) and the frost caster. On Mortar floors the Brute-sized grinder spawns with that exact art, so it looks like the prop came alive. Fix: watchers and the rope monk are tinted `watchTint53` and no longer collide. Not fully closed: new art for the watchers would remove the confusion completely.
- **Floor music.** The draft options are copied into `Resources/Audio/Music/Floor/`. Playlists:
  - Ox Pit: floor1_combat + oxpit_optA
  - Frozen: optA + optB
  - Burning: optA + optB
  - Pounding Mortar: optA + optB

  Each floor picks a track from a per-run shuffle. With `musicRotate53` the track rotates after `musicLoopsPerTrack53` (2) loops. Turn it all off with `floorMusic53=false`.
- **Boss music**:
  - Ox-Head: 180 bpm track, wired. No P2 track.
  - Horse-Face P1/P2: 140/155 bpm, wired.
  - Ice Gorilla: 150 bpm, wired. No P2 track.
  - Qilin: 160 bpm, wired. No P2 track.
  - Mortar Horse-Face Pestle: reuses the Horse-Face P1/P2 tracks. No dedicated track exists.

### v0.0.53 — follow-up (VH: "keep collision but make sure player can break them with any skill")
- Collision is back on by default: `tortureCollide53 = true`. Devices, bowls, ice, pits and watchers block the hero and demons again. Breaking a prop removes its block.
- Every skill still hits props (`SkillArea53` plus the existing projectile and blast hooks), with the 0.25 s per-skill cooldown on each prop.
- The collision blocks only push hero and demon feet. Projectiles, areas, beams and chains are not stopped, so attacks connect on and through solid props.
- Fireball, Frost Cleave, Blood Whip, Soul Spears and Grave Pool used to hold fire when no demon was near. They now also fire when a prop or tortured soul is in range (`BrkNear53`).

## v0.0.53 (pt461u) — Pact nodes now really run in game + Restart fix
- **Restart fixed:** Restart now calls `ResetRun(); PactAfterReset51();`, so the Pact state resets and the Pact is re-applied.
- **UI honesty:** the `live = live || sk >= 0` rule and the flat ×1.25 "signature skill" proxy are gone. The detail line now shows LIVE, LIVE: PARTIAL, or NOT IN GAME YET.
- **New file `HellordPact53.cs`:** all 40 nodes plus the 4 Crests (one Path per Pact; hybrids still unavailable). Hooks are in Hit/PreHit/PostHit, ApplyStatus, KillEnemy, HurtHero, UpdateSkills, projectiles, pools, dash, and room enter/clear. Every number is a `*53` tunable in HellordTuning, with a Tooltip.
- **Simplifications (marked PARTIAL in the UI):**
  - Pyre Feast: Steam's 50% miss chance applies to contact hits only.
  - Marrow Launch: no Fork/Homing.
  - Bone Legion: skeletons are simple sprites that copy Orbital/Melee supports.
  - Rune Ossuary: your own hero orbit stays.
  - Stun = full slow.
- **Design gaps, filled with these values:**
  - Overheat: Fire cooldown ×0.3, heat decays 15/s. The doc says "no cooldown", which would mean a cast every frame.
  - Blood Magic: cooldown ×0.35.
  - Energy regen: 0. The doc gives kills only; tune with `energyRegen53`.
  - Inferno Heart: the old max HP / armor proxy has been removed.
- Crest stat bonuses are still applied, and the Crest text now names the Crest mechanic.

## v0.0.53 pt466u — Damnation Pact PARTIAL nodes completed (no version bump)
- Pyre Feast: Steam is a full blind cloud. Enemy hits from inside it miss 50% of the time, and enemy bullets born inside it have a 50% chance to fizzle. Non-contact hits with no known attacker are blamed on the nearest enemy within 12.
- Marrow Launch: the bones are real projectiles, so Fork and Homing apply. They fly out, then come back to the hero. The orbit is hidden while they are in flight.
- Bone Legion: skeletons copy Bleed, Knockback and LifeLeech supports, up to 8, from all skills when Necro Covenant is taken.
- Rune Ossuary: the whole orbit moves to the rune, and the hero's own orbit is hidden while the rune holds it.
- Calcify: a new stun53 field gives a hard 1s stun (tunable calcifyStunS53) with no movement or attacks. Bosses are immune.
- Overheat and Blood Magic: cooldown multiplier is now 0 (tunable), with a 0.1s minimum cast gap (pactMinCastGap53) for frame safety.
- Hybrids are NOT implemented. The one-Path-per-Pact rule stays.

## v0.0.53 — PixelLab watcher demons wired (no version bump)
- Art: the 4 combined 8f strips (watcher_{imp,warden,overseer,hauler}_8f_38x32.png) are copied into Resources/HellordArt/Torture/. Frames are 38x32, pivot (19,31), feet-tight 16px shadow, no tint.
- Animations: Imp idle 0-5 + cheer_hop 6-7; Warden idle 0-1,5-7 + point 2-4; Overseer idle 0-3,6-7 + cheer_fists 4-5. A cheer/point burst plays about every watchActEvery53 (4s) for watchActS53 (0.9s). Hauler loops rope_haul 0-7.
- Placement: Ox = Imp/Warden/Overseer; Frozen group = Warden/Imp/Overseer; Burning = Overseer/Warden/Imp. All of them watch only; none operate a device, and there is no Hauler on Ox. Mortar B = Overseer + Imp watching, and the Blindfold Rope Hauler on the main rope only (replaces the Grinder Monk; flip tunable ropeHaulerFlip53). No Warden on Mortar B.
- The Bloater/Rice Ghost/Grinder Monk/Frost Caster sheets are no longer used for watchers. Combat enemies with HP bars keep their real enemy sheets. Watchers and the Hauler are still shootable breakables, and the rope snaps with the Hauler.

## Oct 31 2026: Damnation Pact = Paths only (VH LOCKED)
- No Combos, no Hybrids, no prebuilt kits. Each run you pick ONE Path and choose its nodes (max 6, max 1 Keystone, Crest at 4+). Players find their own builds, PoE-ascendancy style.
- The Hybrid/Combo design docs v1.0, v1.1 and simple v1.2 are banner-marked REJECTED. The step61 Path-vs-Combo mockups are reference only.
- Code: the game never had a Combo/Hybrid picker. Only one comment in HellordPact.cs was updated. No version bump.

## pt477u — Pact nodes re-verified, fix + build (v0.0.53, no bump)
- Checked the code paths: Descend/SEAL → PactApply51 applies the path stat bonus and every selected node. Death R and the Restart button → ResetRun → PactAfterReset51 → menu → Descend → applied again, with no double-stacking (ResetRun resets stats).
- All 40 nodes (4 paths × N1–N8, K1, K2) and all 4 Crests have live hooks, and each hook function has a caller in the game loop. No Combo/Hybrid UI is left in the code.
- Fix: the "free trigger every 4th" Crest bonus fired for any path's Crest. It is now gated to the Stormcaller path only.
- Known approximation: Pyre Feast steam attribution. When the attacker is unknown, the hit is blamed on the nearest enemy.
- Built with 0 errors and 2 warnings. Packaged to C:\Users\Admin\Hellord_Prototype_v0.0.53.unitypackage. NOT play-tested.

## pt480u — Test menu: SOUL + RESET SAVE (v0.0.53, no bump)
- Added a new **PACT** tab to the F1 TEST menu, between BOSS and KEYS. It shows the current Souls and Sigils.
  - Souls buttons: +100 souls, +1000 souls, Set souls = 99999 (`testSoulsSet53`), Souls = 0.
  - Sigils buttons: +1 sigil, +10 sigils, Set sigils = 99 (`testSigilsSet53`), Sigils = 0.
  - Every change saves to PlayerPrefs immediately.
- **RESET PACT SAVE...** needs two clicks: click it, then "CLICK AGAIN TO WIPE PACT SAVE", or press "Cancel reset". It deletes `hellord.pact51.` souls, sigils, path, sel and unlock0–3, then reloads (Souls go back to `startSouls51`). It also switches the current run's Pact off.
- F1 and the small "TEST F1" button now also work on the main menu and the Altar. There the menu opens on the PACT tab, and only PACT and KEYS are available because the other tabs need a run. While it is open, the menu ignores Space/Descend and clicks do not pass through to the screen underneath.
- Compiled with 0 errors. No package. Not play-tested.

## pt482u — BUILD v0.0.53 (with PACT test tab)
- Package: Builds/Hellord_Prototype_v0.0.53.unitypackage (21,094,121 bytes, 601 entries), MD5 437a663795b40bd7c87bfa93b9d060ad. Copied to C:\Users\Admin\. Includes the F1 PACT tab (Souls/Sigils, Reset Pact Save). Compiled with 0 errors. Not play-tested.

## pt483u — v0.0.53 playtest fixes (no version bump, no package)
1. **Decor reset on re-entry.** Cause: the torture/Mortar decor layout seed included the hero's entry position (`avoid` list), and no state was stored per room. Fix: the layout is now purely seeded. `ClearTorture52` saves breakable hp, ice broken and soul dead flags into `FloorRoom` (`brkHp53/iceBroken53/soulDead53`). `BuildTorture52` re-applies them silently: rubble plus decal, no FX, no drops. Toggle: `decorPersist53`.
2. **Decor "breaks pixels / zooms out".** Cause: a non-uniform `localScale` wobble (±6%) on hit, and a 1.25x / 0.2x squash on death, applied to pixel-art sprites up to ~458 px wide. Fix: scale is never changed now. A hit is a 1 px whole-pixel shake. Death is a whole-pixel sink plus darken and fade.
3. **Decor drops.** `BrkDrop53` gives xp souls plus a bone chance on break, ice shatter and damned-soul death. Tunables: `brkDrops53`, `brkDropXp53`, `brkDropXpBig53`, `brkDropBoneChance53`, `brkDropBonesBig53`. Decor still never attacks.
4. **Support altar invisible.** Cause: the Bone Shrine statue is baked into `rooms_shrine_672x384_base.png`. On Frozen/Burning/Mortar floors, `LoadRoom` builds a composed tileset base instead, so the statue was dropped. Fix: shrine rooms keep their baked base, recoloured by the theme ramp. Toggle: `shrineBakedBase53`.
Not play-tested.

## pt484u / t485u — broken decor still blocked movement; floor questions
- **Cause:** ice blocks added a solid footprint (`Collide53`) to `_blocks42`, but the rect was thrown away, so shattering the ice never removed it. Rubble stayed solid for the hero and demons, which share `_blocks42`. Other breakables removed theirs through an exact float `Rect ==` match, which was fragile.
- **Fix:** new `DropCol53` (tolerant match). It runs on `Break53`, on the restored-broken path (`BreakSilent53` / `RestoreBrk53`, including ice) and on ice shatter. Ice gets its collision back only when it regrows. Decals, drops and the no-attack rule are unchanged.
- **Screenshot:** OX PIT (Theme 0, sandy Ox tiles, not the Mortar skin), Survive room (fight room: live through the timer).
- **Why Ox Pit can appear twice:** `PlanRun42` shuffles the themes in blocks of 3, and floor 1 is forced to Ox (`floor1ForceOx49f`). With `floorCount = 3`, the theme order gives each theme once per run; it can only repeat if `floorCount` > 3. Mortar torture floors use the Ox theme (Theme 0) with a different skin, so the HUD would also say OX PIT. Each new run starts on Ox again. Not play-tested.

## t487u — floor 1 not pinned to Ox Pit
- Removed `firstFloorAlwaysOxPit` and `floor1ForceOx49f` (tuning fields and code in `PlanRun42`). Both were already OFF by default.
- Each run shuffles Ox / Frozen / Burning per block of 3, floor 1 included. Same theme is never on two floors in a row across blocks.

## t494u — keep the old Ox Pit look, torture decor only
- **Removed as a floor look:** the Mortar B skin. `mortarFloor52` now defaults OFF, so `Mortar52` is false. That turns off the Mortar tile base, the Mortar low walls / doors (`TileTheme 3`), the Mortar music key and the Horse-Face boss swap (Ox floors get the old Ox-Head boss). The code is still there behind the toggle.
- **Kept on Ox Pit rooms** (new `oxTortureDecor53`, ON): the `torture_ox` set piece with watchers, mortar pit + pestle, sacks / husks / bowls, rice patches, damned souls, breakables with loot, and rubble that does not block. Rice Ghost / Bloater / Grinder Monk extras still spawn on Ox floors 2+.
- Frozen and Burning unchanged. Not play-tested.

## t495u — Mortar Hell is a 4th floor theme
- The run order now shuffles 4 themes: Ox Pit / Frozen / Burning / Mortar (`PlanRun42`, blocks of 4, no theme twice in a row). `floorCount` changed from 3 to 4, so each theme appears once per run, in random order, floor 1 included.
- `mortarFloor52` is ON again. `Mortar52` now means "this floor's slot is Mortar", not a random skin roll on an Ox depth. If it is OFF or the Mortar art is missing, the run uses 3 themes.
- **Mortar floors:** Mortar tiles, Mortar low walls / doors, Mortar music key, the `torture_b` set piece and props, the Horse-Face boss, and Bloater / Rice Ghost / Grinder Monk (these are now Mortar-only). Gameplay runs on Ox rules underneath (`Theme` reads 0).
- **Ox Pit:** the old tiles, walls, doors and Ox-Head boss, plus torture decor only (`oxTortureDecor53`: `torture_ox`, pit, sacks, rice, souls, watchers).
- HUD, minimap name, start banner and the test menu now show MORTAR HELL / MORTAR. Frozen and Burning are unchanged. Not play-tested.

## t501u — Horse-Face (PixelLab v138) wired as the Mortar Hell boss (2026-10-31, UTC+7)
- Art: the v138 106-frame 96x96 strip was repacked into a 21x6 grid (2016x576, to stay under the WebGL 2048 px limit). It overwrites `Resources/HellordArt/boss_horseface_pestle.png`, which was the old Ox-Head recolor mock-up. Added `boss_horseface_rice_sack.png` (5 cells of 52x46). The SheetDef is now 96x96, 106 frames, pivot (48,88), ppu 16.
- Only Mortar Hell (`MortarBoss53`) uses it. Ox Pit keeps Ox-Head. Floor 3 without Mortar still uses the Bone recolor.
- New file `HellordHorseFace53.cs`. `HorseFrames53` remaps the frames into the Ox-Head slot layout so the existing state machine drives them:
  - idle→idle SE, move_S→walk_S, move_E→walk_SE, move_N→walk_N
  - gore→spin_loop, horn→pestle_overhead_slam, stomp→rice_sack_throw
  - roar→leap_in, hurt→hurt, stun→stuck_pestle_stun, phase→dizzy, death→death
  - The rage look is still the fire recolor.
- Hitboxes and projectile:
  - The horn strike also hits inside the JSON pestle circle: offset (18,4) px from the pivot, r 16, mirrored when facing west.
  - On the stomp strike (throw frame 3) a rice sack spawns at the JSON hand offset. It arcs up, comes down on the hero's position, then does a hit check, dust and shake.
  - Tunables: `horseSlamR53`, `horseSackSpeed53`, `horseSackArc53`, `horseSackHitR53`, `horseSackDmg53`.
- Known limitations:
  - Attacks are drawn facing SE only; the sprite is flipped for west.
  - walk_E and hurt have no pestle, so walk_E is not used and hurt keeps the gap.
  - leap_in plays as the roar with no engine jump arc yet.
  - spin_loop has no dedicated ~30 px sweep hitbox; it relies on the existing charge contact.
  - The body hurtbox is still the Ox-Head one, not the JSON rect [-13,-46,26,46].
  - In death the body sits left of the pivot.
  - Colours are not strictly Endesga.
  - The stomp rings still fire around the boss as well as the sack.
- Compile v0.0.53: 0 errors, Succeeded. Nothing was play-tested.

## t502u — Horse-Face fixes + Mortar B check (2026-10-31, UTC+7)
- Horse-Face (only in Mortar Hell, via the new `AnimateHorse53` called after AnimateBoss):
  - **Leap-in:** the intro roar or leap gets an engine jump arc: sin arc, `horseLeapH53` 2.5 u over `horseLeapS53` 0.6 s.
  - **spin_loop:** a dedicated sweep hitbox: 30 px circle around the body centre while gore_loop plays, 10 dmg every 0.5 s (`horseSpinR53/Dmg53/Cd53`). The charge contact still applies too.
  - **hurt:** the hurt slot now uses dizzy recoil frames 75/77, which have the pestle. Hurt 44-49 has no pestle. walk_E still uses walk_SE.
  - **death:** `horseDeathDx53` = -5 px, mirrored for west. I measured the lying-body pixel mass in death frames 54-57, and it sits about +5 px right of the pivot, not left as HA's note said. The bbox incl. the pestle is centred. The shift is tunable.
  - **Body hurtbox:** the JSON rect [-13,-46,26,46] is used: radius 13 px, centre 23 px up (`horseJsonBody53`).
  - **Stomp:** with `horseNoRings53` the stomp is the rice sack only. There is no ring damage and no bullet burst, and the ring tell is a 20 px foot cue that still times the throw.
  - **Spawn:** the body local position is reset on spawn.
- Mortar B check:
  - **Tiles:** `Resources/HellordArt/MortarHell` is byte-identical to `tiles/mortarB/*`, and `mortar_tileset.json` points at the mortarB_* files.
  - **Low walls:** all 16 `LowWalls/mortar_*.png` are byte-identical to `lowwalls/mortarB/strips`. The glow, S wall, and SE/SW corners are also identical in A/C.
  - **Theme routing:** Mortar52 gives TileTheme 3 and the "mortar" low-wall level. Ox Pit, Frozen and Burning are untouched. No code change was needed.
- Compile v0.0.53: 0 errors, Succeeded. Nothing was play-tested.

## t503u — Main Menu v69 wired (VH-locked art), v0.0.53, no version bump (2026-10-31, UTC+7)
- **Art source:** `Art/previews/step36_main_menu/v69_pact_seal_hover_simple/`. `tools/main_menu_v69.py` bakes it with the v69 scripts unchanged into `Resources/HellordArt/MainMenu69/` (59 files, 3.2 MB):
  - `mm69_base_00..47`: the 700x392 bg with the 16-frame braziers (right fire seated in the bowl), plus the exact v49 title with glow breathe, eye pulse and ash motes. It has no shimmer sweep and no v57 bottoms/fangs. The old throne has no seat, no glow circle and no blue dots.
  - `mm69_rows_0..4`: the text-only v11 rows with the skull cursor and v49 'S' blood select. There is one 48-cell atlas per selected row.
  - `mm69_seal_hi` / `mm69_seal_glow`: the seal masked brighten and the red edge halo.
  - `mm69_sock_on_0/1`, `mm69_sock_off`, and `mm69.json` (layout).
- **Live data drawn by the game:** the SOULS/SIGILS currency at the top right, the "PACT n/6" plate, socket fill from the sealed node count, and the socket tooltip (the sealed node's name and desc, or EMPTY SOCKET).
- **Seal:**
  - Hover or focus fades in a pulsing ~18% brighten plus a red edge glow, and fades out on leave.
  - Click or confirm gives a soft press (0.45 s), then the screen dims and the panel grows from the seal. [B]/Esc shrinks it back. [A] opens the Sacrifice Altar.
  - Socket hover shows a tooltip only.
  - The hit area is 128x140 at (549,108).
- **Navigation:**
  - Up/Down (arrows/WASD/d-pad/stick) moves through the rows. Right moves to the seal; Left/Up/Down moves back.
  - Enter/Space/pad A confirms. Esc/Backspace/pad B goes back. Mouse hover selects.
- **Rows:** DESCEND, SACRIFICE ALTAR (opens the Altar), CODEX and SETTINGS ("COMING SOON" toast), QUIT.
- **Removed from the outer menu:** hex buttons, the little king, the Space hint and the pact list.
- **PLACEHOLDER:** the Pact panel interior is plain: path, SEALED n/6, node list, [A]/[B]. The Sacrifice Altar interior is unchanged. VH remakes both.
- **Code:**
  - New file `HellordMainMenu69.cs`.
  - `HellordPact.cs` hooks it in DrawPact51 and PactTick51.
  - Tunables in `HellordTuning.cs` (with tooltips): `mainMenu69` (off = old menu), `mm69FrameMs`, `mm69HoverFadeS`, `mm69HoverBright`, `mm69PressS`, `mm69PanelOpenS`, `mm69PanelCloseS`.
- **Build:** v0.0.53 compile succeeded with 0 errors. **Not play-tested.**

## t507u: In-game preview of the v69 menu, plus 2 fixes (2026-10-31, UTC+7)
- **Captures:** real screenshots from the running v0.0.53 Linux player on an Xvfb screen (1280x720), driven with xdotool. Saved in `Art/previews/step36_main_menu/hm_ingame_preview/`.
- **Fix 1:** the "PACT n/6" text now sits inside the plate (y 213). It was drawn below the plate.
- **Fix 2:** the `seal_hi` hover mask was 12 px low (clip69 used y120, the seal is drawn at y108), which gave a doubled seal with a hard cut. It is re-baked aligned to the seal, with the cut-off moved to y212.
- **Known nits, not fixed:**
  - Sanitize strips "[ ]", so the panel shows "A OPEN SACRIFICE ALTAR / B BACK".
  - On hover the seal highlight covers the empty socket rings.

## t508u — Combat clip recorded (2026-10-31, UTC+7)
- **New launch flag `-botmenu`:** used together with `-botstate`, it keeps the main menu up for bot runs, so a recorded run can start with a real DESCEND click. Without it, `-botstate` still skips the menu as before. Code: `HellordPact.cs` `PactMenuOn51`. v0.0.53 rebuilt: 0 errors, no version bump.
- **Clip:** `Art/previews/step36_main_menu/hm_gameplay_clip/04_floor1_combat_menu_descend_to_room_clear.mp4`, 86.6 s.
  - DESCEND was clicked with the mouse, and the starting skill was picked with key 1 (Curse Rune).
  - From there, `tools/playbot.py` played Start → door → a Survive room on Frozen Hell (floor 1) until the room was cleared.

## t511 — Sacrifice Altar redesign (v0.0.53, no bump/package) — 2026-10-31 UTC+7
- Main-menu seal now opens the Sacrifice Altar (`PT.mm69SealOpensAltar`).
- New `HellordAltar97.cs` (toggle `PT.altar97`): 3-Path carousel with evil PixelLab busts, YOUR PATH strip/ribbon, NOT CHOSEN/LOCKED; tier-column node tree (48px icons in 72px nodes, orthogonal links, keystones solid-from-parent or dashed "ANY 2 NODES"), large description, TAKE NODE / REMOVE / CHOOSE PATH (locked Path shows LOCKED).
- Art baked by `tools/altar97_bake.py` → Resources/HellordArt/Altar97 (new Stormcaller demon bust, new Thunder Step / Whip Reach icons).
- All pact rules, Seal/Back and YOUR PACT kept; Ascendancy never shown.
- Not play-tested: gamepad, final keystone re-layout screenshots. YOUR PACT screen still old style; a few link detours remain.

## t525u — Necromancer (The Gravecaller) Pact Path wired (2026-10-31, UTC+7) — build v0.0.53, no version bump
- Necromancer added as the 5th Damnation Pact Path (`PactPath.Necromancer`, index 4, colour #5ac54f). It uses the same rules as the other four Paths: unlock before select, one Path per Pact, max 6 nodes, max 1 Keystone, Crest at 4+, Souls/Sigils. The 8 Notables and 2 Keystones use the names, costs and reqs from `Hellord_necromancer_summoner_skills_v0.2.md` §9. Crest text: "UNDEAD MINIONS +15% DMG, +15% LIFE".
- Altar97 carousel now loops through 5 Paths (dots and "x/5"). The Necromancer tree layout puts Crypt King on row 1, straight from Quick Grave. Legion of One Hundred uses the "ANY 2 NODES" stub.
- **All 10 Necromancer node effects are STUBS (live=false), and so is the Crest.** The reason is that no minion summon skills (Skeleton Warriors/Archers, Wolf, Bear) exist in code yet, so there is nothing for the nodes to buff. The only minions in the game are Bone Warden's Bone Legion skeletons, and those can't be used together with Necromancer (one Path per Pact). I did not wire Crypt King's "-50% non-minion skill damage" on purpose: on its own it would be a pure penalty. All of these go live once the four minion skills are built.
- Art is a **PLACEHOLDER**: the avatar and node icons are Bone Warden art tinted green (`tools/altar97_bake.py`, files `icon_4_*`, `avatar_4*`). Real art is still needed.
- Unlocks saved under the prefs key `hellord.pact51.unlock4`. The old 2x2 legacy altar grid (DrawAltar51) still shows only 4 Paths. Altar97 replaced it.
- Summoner (Beastbinder) Path not wired, as instructed.
- Compiled with 0 errors. **Not play-tested:** no in-game screenshot or test of the 5th carousel page.

## t526 — Bone Warden v107 whole-bust portrait in Altar97 (2026-11-01, UTC+7) — build v0.0.53, no version bump
- `avatar_1.png` now uses the v107 base bust (`base/bone_body_closed_64.png`) and replaces the old skull-only portrait. `avatar_1_lock.png` is re-dimmed from the new bust. `avatar_1_idle0..7.png` are cut from `bone_idle_strip_64.png`. All of this is baked by `tools/altar97_bake.py`.
- Unfocused card: static base bust, no animation. Focused card (carousel centre and the Path-nodes screen portrait): the 8-frame PixelLab idle loop only, with no select gesture and the mouth kept shut (jaw lock is from the v107 art). Locked: static dimmed bust.
- New tunable `PT.boneIdleFrameMs97` = 180 ms per frame (0 = static).
- Other Paths unchanged. The Necromancer placeholder art still comes from the old Bone Warden skull art.
- Weak spots from v107 not fixed: eye fire dims in frames 3–5, wisps barely visible, possible robe/pauldron pixel crawl, a possible 1px jaw seam, about a 360 ms hold at the loop point, navy hood / lumpy right pauldron / faint smile line.
- Compiled with 0 errors. Not watched in-game in real time.

## t527 — Altar97 portrait idle for all 4 Paths (2026-11-01, UTC+7) — build v0.0.53, no version bump
- Pyromancer, Stormcaller and Blood Reaver now use the v102 closed-mouth bases (`v102_portrait_closed_mouth_anim/base/{pyro,storm,blood}_closed_64.png`) as the static `avatar_{0,2,3}.png`. The `_lock` versions are re-dimmed from those bases.
- Their idle frames come from the v103 idle strips (`v103_portrait_idle_only_select/portrait_anim/*_idle_strip_64.png`, 6 frames each) and are saved as `avatar_{p}_idleN.png`.
- Bone Warden keeps the v107 whole-bust portrait (8 frames).
- Same rule for all four: an unfocused card shows the static still; a focused, unlocked card plays the idle loop only. There is no select gesture, and the mouth stays closed (the strips are already mouth-locked).
- Locked Paths (Blood Reaver until it is unlocked) show the static dimmed still, so the open v103 question is answered as "locked does not animate".
- `PT.boneIdleFrameMs97` (180 ms) now sets the speed for all four Paths.
- Necromancer stays on its placeholder art with no animation.
- v102/v103 weak spots still apply: the Pyromancer eye glow may read as blinking, the idle is subtle, and the busts are softer than v86.
- Compiled with 0 errors. Not watched in-game.

## t536u — Necromancer Pact applied in game (2026-10-31 18:23 UTC+7)
- Necromancer Path now has the 18 approved nodes (Group A SUMMON: A1..A7, AK CRYPT KING, AK2 LEGION OF ONE HUNDRED; Group B BONE SKILLS: B1..B7, BK OSSUARY TYRANT, BK2 GRAVE COLOSSUS). Pick max 8 (`PT.necroMaxSelect536`), 1 Keystone, Crest at 4+. Other 4 Paths unchanged (10 nodes, pick 6).
- Pact code generalised: requirements by node id ("A2/A3"), "2A"/"2B" = any 2 Notables of that group, Keystone = id contains K, node counts no longer fixed at 10.
- Altar97: Necromancer tree shows one group at a time; toggle with the A SUMMON / B BONE SKILLS tabs above the tree or Tab. Card dots show 18. Icons are PLACEHOLDERS (green-tinted Bone icons for A, plain Bone icons for B), not HA v111 art.
- BONE SKILLS = Bone Orbit, Soul Spears, Grave Pool (`PT.necroBoneSkills536`, editable). VH to confirm this list.
- No summon skill exists yet. Minions come from an F1 TEST spawn (3 skeleton warriors + 2 skeleton archers, tunable) and from A7 Ossuary Order. Minions have life; demons touching them hurt them; dead ones respawn (6 s base). Placeholder look = bone sprite (archers green-tinted).
- Partial: B5 Grave Reach and BK2 Grave Colossus scale AREA only (projectile range / melee reach not scaled yet). Minion bullet-blocking not wired (baseline, needs the real summon skill).
- OPEN: Bone Warden is still a separate Path in game (doc says merged into Necromancer). Removing it needs VH's call on existing saves.
- Compiled v0.0.53 with 0 errors. Not play-tested.

## t539u — Bone Warden removed as a Path (Nov 1, 2026, UTC+7)
- DONE (closes the t536u OPEN item): `PactPath.BoneWarden` deleted. Enum is now Pyromancer=0, Necromancer=1, Stormcaller=2, BloodReaver=3. Altar carousel shows 4 cards (1/4..4/4): Pyromancer, Necromancer, Stormcaller, Blood Reaver.
- Removed: Bone Warden name/colour/passive/crest/10-node list from PactData, its Altar layout, its +armor-per-node Pact stat. Old Bone Warden node code in HellordPact53/V36 is DORMANT (bound to a never-active `BoneWardenGone539` constant) — reference only, can be deleted later.
- Art: Necromancer card uses the v107 bone bust (`avatar_1`, 8-frame idle) and its own node icons `icon_4_*`. No new art wired; HA v111 mockup NOT used.
- Save migration (one-time, key `mig539`): BW Bone Legion -> Necro A7 Ossuary Order, BW Calcify -> B6, BW Marrow Harvest -> B7 (stay unlocked). All other unlocked BW nodes refunded (Souls at their cost, +1 Sigil per Keystone). Old Necro unlocks (unlock4) move to slot 1. Saved Path BW -> Necromancer with selection cleared; saved Necro -> Necromancer, selection kept.
- Build v0.0.53: 0 error CS. NOT play-tested; migration not tested against a real old save.
- OPEN for VH: OK with Necromancer sitting 2nd in the carousel (took Bone Warden's slot)? OK with refund rather than more node carry-overs?

## t541u — Pyromancer Pact remake DESIGN (Nov 1, 2026, UTC+7)
- Docs/Hellord_pyromancer_pact_v0.1.md: 18 nodes (A BURN / B FIRE SKILLS), pick 8, 1 Keystone, Crest at 4+. No skill names targeted. Design only, NOT applied — waiting VH approval.

## t544/t546u — Necromancer Altar art wired (Nov 1, 2026, UTC+7)
- Necromancer Altar screen is now ONE PoE2-style tree (v115 unlock-edges layout, B mirrors A around START). Old A/B group tabs + Tab key removed for Necromancer.
- Unlock edges = VH t543u (A4<A2|A3, A7<A5|A6, B4<B2|B3, AK<A4, BK<B4, AK2/BK2 = any 2 notables of own group via pill with 7 pips + dashed line, no fake parent). OR tags on A4/A7/B4.
- Lines: amber lit = both ends taken, bone = parent owned, grey = parent not owned. Nodes: Taken amber+glow+check, Owned grey frame, Locked dark+padlock, Keystone blocked red + "MAX 1 KEYSTONE".
- 4 Keystones in the tree, max 1 taken. Header: "PICK n/8   KEYSTONE SLOT k/1" (slot counter, not tree total). All "1 KEYSTONE MAX" copy -> "MAX 1 KEYSTONE".
- Portrait = v107 Bone Warden whole-bust idle (avatar_1). v112 lich bust NOT used. Node icons = v112 PixelLab placeholders (icon_4_0..17, bake in tools/altar97_bake.py).
- Build v0.0.53 0 error CS. NOT play-tested / no screenshot verified (label overlap at 640x360 possible).

## t549u — Necromancer: max 2 Keystones per run
- Tree still has 4 Keystones (Crypt King, Legion of One Hundred, Ossuary Tyrant, Grave Colossus); a run may take 2. New tunable `necroMaxKeystones549 = 2` (HellordTuning); other Paths stay at `pactMaxKeystones51 = 1`.
- Altar header: "KEYSTONE SLOT k/2" (taken/max). With 2 taken, remaining Keystones show blocked "MAX 2 KEYSTONES"; swap toast "MAX 2 KEYSTONES: REMOVE A KEYSTONE TO SWAP".
- Keystones still consume pick slots (8 total). Necro design doc's "1 Keystone" is superseded.
- HA: v115 mockup copy should read KEYSTONE SLOT x/2.
- Compiled v0.0.53, 0 error CS. NOT play-tested.

## t551 — Necromancer Altar art applied (VH via HA; refs v115/v117/v118/v121, v112 icons, v107 bust)
- Sidebar (Necromancer only, `NecroSide551`): Bone Warden v107 bust card + Path blurb "NECROMANCER / Raise the dead and command bone. / Summon minions and empower fire and bone skills. / Unlock nodes, then select up to 8 (at most 2 Keystones)." No stats-style "18 NODES / PICK MAX 8..." text.
- Below it: Gravecaller Crest box, PASSIVE ONLY (at 4 nodes: minions +15% dmg/life, bone skills +15% dmg). Nothing Keystone-related in or beside it.
- KEYSTONE SLOT k/2 stays in the tree header top-right ("PICK n/8   KEYSTONE SLOT k/2"), away from the Crest. PICK counts all picks incl. Keystones (max 8); necroMaxKeystones549 = 2.
- Node names: drawn straight on the tree, no plate; one word per line; the crowded centre row (node indices 0, 7, 9, 16) puts labels above to stop overlap.
- Kept: one tree, v115 unlock edges, Taken/Owned/Locked/Blocked states, v112 placeholder icons.
- Compiled v0.0.53: 0 errors. Not play-tested, no screenshot — label spacing is estimated from coordinates.

## t493u — Necromancer Pact Altar = v130b mock-up (2026-11-01, UTC+7)
- New `HellordAltar130.cs`: when the Altar shows the Necromancer, it draws the v130b mock-up (Art/previews/step36_main_menu/v130b_necromancer_currency_top_right_respaced) in mock pixel coordinates, scaled 700/1280.
- Text uses the mock's fonts and sizes: Pixelify Sans Bold for words, Press Start 2P for digits. These are TTF fonts in Resources/Fonts, set to hinted raster.
- The static art is baked by tools/necro130_bake.py: necro130_bg and the silent hub on/off.
- Bust: v107 Bone Warden. The card shows the Path description exactly as given (no v112 lich).
- Header: SOULS n and SIGILS n sit top-right, each as one PS2P 12 string, so label and number are the same size.
- Sidebar order: YOUR PACT / PICK n/8 bar, then the 8 icon-only slots (no ids, no costs), then the Gravecaller Crest (passive only, unlocks at 4), then the detail panel. The cost appears only in the detail action line.
- Keystones: KEYSTONE SLOT k/2 floats at the top-right of the tree panel, with k/max taken from necroMaxKeystones549 = 2. PICK counts Keystones inside the 8.
- Tree: one tree on the v130b coordinates (Legion and Grave Colossus sit further right), using the v115 unlock edges.
  - Connect lines are thin, with no border. Taken nodes get a small outline and glow.
  - The 4 node states plus the Keystone-limit state are drawn.
  - Labels have no plates. There is no banner, no hub text and no KEYSTONE·SUMMON·BONE strip.
- Input: the mock has no buttons, so the detail action line is clickable (Primary97) and a small "[B] BACK" link sits at the bottom of the detail panel. Pad and keyboard navigation now use the v130b positions.
- The build compiles with 0 errors. It has NOT been play-tested and there is no screenshot.

## t561u — Necromancer skeleton summon skills (2026-11-01, UTC+7)
- **Two new player skills.** `SkillId.SkeletonWarriors` and `SkeletonArchers` are tagged Minion, plus Melee or Projectile, plus Hit. They are in the normal level-up card pool for every run and in the F1 Skills tab.
- **Max level 5** for these two skills (`summonMaxLv561`), the same as every other skill.
- **Behaviour:** the skeletons appear beside the player as soon as the skill is owned (PoE2 style, no recast). Each one that dies respawns beside the player after its own timer. Level-ups apply at once.
- **Numbers (Lv1–5, placeholders):**
  - Warriors: count 2,3,3,4,5; damage 6,9,12,15,18; respawn 6, 5.5, 5, 5, 4.5 s; life 40 (+10% per level); attack every 0.9 s.
  - Archers: count 1,2,3,3,4; damage 5,7,9,11,14; respawn 8, 7.5, 7, 6, 5.5 s; life 20 (+10% per level); shot every 1.2 s, range 8 m; idle in a 3.5 m ring around the player.
- **Damage formula:** skill damage × (1 + 50% of the player's increased damage) × Necromancer node, Crest and Intensity multipliers.
- **Leash:** targets within 7 m of the player; teleport back beyond 10 m.
- **Minion rules:**
  - The global minion cap is 12 (`minionCap561`). AK2 Legion of One Hundred adds +4.
  - Necromancer nodes A1–AK2 apply: Grave Muster +1 per type, Iron Marrow, Hollow Haste, Quick Grave, Grave Edge, Marrow Shot, Crypt King and Legion.
  - BK Ossuary Tyrant still means no minions.
  - Covenant (BW K2) no longer bans Skeleton Archers, because they are minions.
- **Bullet block (baseline):** minions destroy the enemy bullets that touch them and take the bullet's damage. This covers Shot44 (frost bolt and ice shard) and Ox-Head boss bullets. Other bullet systems are NOT covered yet.
- **F1 test menu:** the old TEST skeletons toggle is replaced by:
  - "give Skeleton Warriors + Skeleton Archers (Lv1)";
  - "+1 level both summon skills".
  - "Kill my minions" is kept.
- **Art:** placeholders only. Both types use the existing enemy_skeleton strip with different tints (bone white vs grave green), a sand foot ring, and a drawn bow stick for archers. Dedicated warrior and archer skeleton sets are MISSING. No art was generated.
- **Not wired (stubbed):** supports copying to minions, the Echo double-respawn, the bone-dust rise animation, minion hit SFX at -6 dB, and the Legion kill counter for minion kills. Wolf and Bear are not done (by design).
- The build compiles with 0 errors. Nothing has been play-tested.

## t534u — Skeleton Warrior and Archer art wired into the summon skills (2026-11-01, UTC+7)
- **Correction to t561u:** those code edits had gone into /workspace/HellordProject. `linux_build.sh` overwrites that folder from /workspace/Hellord/Assets on every build, so the earlier "0 errors" result did not include them. They are now applied to the source tree (/workspace/Hellord/Assets) and compile. New file: `HellordSummon561.cs`.
- **Art:** the approved step37n exports were copied to `Resources/HellordArt/Summon561/`: 10 warrior strips, 7 archer strips, and both pivots_sizes.json files. Frame size and pivot come from the json:
  - Warrior: 42x38, pivot (20,35).
  - Archer: 38x39, pivot (19,35).
  - Both at PPU 16, point filter (set by HellordArtImporter). Size is tunable with `skelArtScale561`.
- **Animations live:**
  - Warrior: idle S/E/N at 8 fps; walk S/E/N at 12 fps; attack S/E at 12 fps; death S/E at 10 fps, followed by a 0.4 s fade.
  - Archer: idle S/N only at 8 fps (sideways facing uses S or N, no East); walk S/E/N at 12 fps; shoot S/E at 12 fps with the brown wood bow (step37e).
  - West is East mirrored. Attacks toward the north use the South attack/shoot row.
- **Missing art:**
  - attack/shoot North rows (warrior and archer);
  - archer death (placeholder: a 0.35 s sink and fade of the idle frame);
  - summon skill icons (they use the generic icon 8);
  - hero-pick visual.
- The old tinted enemy_skeleton placeholder is removed. The old Spr53 placeholder remains only as a fallback if a sheet fails to load.
- FX (arrow trail, hit, spawn, death puffs) is left to HFX. Current FX is unchanged: a CheapBurst on spawn and death, and a green line trail for archer shots.
- Compiles with 0 error CS. The only 2 warnings are old ones (`_bench`, `_hoverNode51`). Not play-tested.

## t561u FX — Skeleton Warrior and Archer combat FX wired (2026-11-01, UTC+7)
- **New file:** `HellordSummonFx561.cs`. The FX are drawn at runtime from the SPEC.md/NOTES.md timings and Endesga colours. No mp4s or PNGs ship; it is all drawn with Z37 primitives at 1 px = 1/16 m.
- **Warrior v2:**
  - Downward slash smear on damage frame 0, over 2 frames: a high arc, then a low arc, with the thick head and white tip at the bottom.
  - The attack anim joins at swing frame 4, as in the mock-up.
  - Impact: 8-ray star 6→4 px, a 1-frame ring, 3 chips, held for 3 frames.
  - Grave Edge: bone link line to the 2nd target, plus a 0.7× star.
  - Walk dust: a 0.35 s stepped puff 11 px behind the heel on foot contacts (walk frames 0 and 3).
- **Archer v7:**
  - Damage stays on shoot frame 0. The visual release is on frame 6 (+0.5 s): a release flash at the bow.
  - The 19 px normal bone arrow (shaft, triangular head with white tip, two feathers, #181425 outline, 3 px smear behind the nock) flies for 0.1 s to the target's current position.
  - On arrival: star, ring, 3 splinters, and the arrow stays stuck in the target for 0.4 s.
  - A6 pierce: a 2 px arrow with a solid lane.
  - No aim dots. The old green Tr536 tracer is removed.
  - Death: the idle frame sinks in 3 stepped 6 px steps over 0.35 s, with no alpha.
- **Shared:**
  - Spawn: a 0.7 s bone/ivory grave ring (dashed ring closes over 0.3 s while 6 motes converge, then a white frame, then puffs). This replaces the green CheapBurst.
  - Death: a white frame, 9 bone shards and a dashed dust ring.
  - Blink-back (more than 10 m away): a burst at the old spot, then motes gathering, a white frame and puffs at the new spot.
  - Bullet-block spark: a 4-ray bone cross over 3 frames.
  - Frozen floors use dust #8b9bb4, as the NOTES ask.
- **DAMAGE TIMING IS STILL OPEN (VH):** the code still damages on attack/shoot frame 0, and only the archer's visuals wait for f6. The arrow lands about 0.6 s after the damage number. Options: move damage to f6, or shorten the draw. Not changed.
- **Not done:**
  - the 1-frame white flash on the target;
  - the pierce stars on each target it passes;
  - per-floor colour variants beyond the Frozen dust (all floors use the same bone palette, as in the mock-ups).
- Compiles with 0 error CS. The 2 warnings are old ones. Not play-tested.

## t535u/t536u — Necromancer Pact panel restored to v130b mock-up (2026-11-01 00:04, UTC+7)
- **What was wrong:** the t493u v130b work had been edited in HellordProject, not in the source tree. linux_build.sh copies the source tree over the project on every build, so the Altar97 hook was overwritten, and the game fell back to the old tree (START hub, SUMMON/BONE headers, PICK x/6 strip).
- **Fix:** these files are now in the source tree (/workspace/Hellord/Assets/Hellord): HellordAltar130.cs, Resources/Fonts PixelifySans-Bold + PressStart2P-Regular, and Altar97 necro130_bg / necro130_hub_on / necro130_hub_off.
- DrawAltar97 now draws only DrawNecro130 for the Necromancer node screen. The old header, plates, YOUR PATH strip and DrawNodes97 tree are skipped. Pad and keyboard navigation use the v130b positions (TP130).
- All 12 mock-up points are as listed under t493u. The "ANY 2 SUMMON/BONE NOTABLES" Keystone-gate plates stay, because they are in the v130b mock-up (cur_130b.py line 119).
- Font metas (fontRenderingMode 2) exist only in the project, which is fine because the build does not overwrite metas.
- Build v0.0.53: 0 error CS. NOT play-tested, no screenshot.

## t574u — Versioning rule + v0.0.54 build (2026-11-01 07:10, UTC+7)
- **Rule:** the version stays the same while code and design are being iterated. EVERY build or package must increase the version (0.0.53 → 0.0.54 → …). Never re-ship an existing version number.
- The version lives only in the build inputs (`linux_build.sh <proj> vX` and `build_package.py vX`). No runtime or Tuning version constant exists; the `…53` field suffixes are feature tags, not the version.
- **v0.0.54:**
  - Linux build succeeded: 0 error CS, 0 warnings.
  - `Builds/Hellord_Prototype_v0.0.54.unitypackage`, MD5 931f0696d6fa2e9bed8d6bc2d402cf8b.
  - Includes the restored Necromancer v130b Altar from the source tree: HellordAltar130.cs, the Altar97 hook, the two fonts and the three necro130 PNGs.
  - Copied to C:\Users\Admin\ on DESKTOP-FN5KPPL. PC MD5 matches. NOT play-tested.

## t576u — Necromancer Altar: real UNLOCK/TAKE/REMOVE button + no white selection (2026-11-01 07:30, UTC+7)
- **Root cause of the white block:** the focus ring was drawn with BoxM using a transparent fill. That paints the edge rectangle solid, so the selected node became a pale square. BoxM now draws an outline only when the fill is transparent, and the focus ring is a thin dim-amber outline.
- **Detail panel button** sits at the bottom of the panel, mock y 640–670: a framed button labelled [A] UNLOCK / [A] TAKE INTO PACT / [A] REMOVE FROM PACT.
  - It is greyed when it can't be used: LOCKED / PACT FULL n/8 / KEYSTONE SLOTS FULL.
  - The sub-line underneath reads:
    - "COSTS n SOULS (+1 SIGIL for Keystones)"
    - "USES 1 PICK (+1 KEYSTONE SLOT)"
    - "FREES 1 PICK · STAYS UNLOCKED"
    - "NEEDS <req>"
  - Click or pad A both work. [B] BACK moved to the top-right of the detail panel.
- **Node states:**
  - Taken: amber 2px border, warm fill, soft amber glow, green check, amber name.
  - Owned: slate with a grey border and grey name.
  - Available: navy with an orange border.
  - Locked / Keystone-limit: dark with a padlock (red when it's the Keystone limit).
- The v130b layout is unchanged.
- Build v0.0.54 (iteration, no bump): 0 error CS. NOT play-tested, no screenshot.

## v140 — Necromancer Pact single-action UI (2026-11-01 07:55, UTC+7)
- Source: Art/previews/step36_main_menu/v140_necromancer_pact_single_action (cur_140.py). Code: HellordAltar130.cs (Btn140 plus a new detail panel).
- **One action button, 128×34, centred in the tree's empty bottom-middle** at mock x835 y622 (between Marrow Shot and Calcify).
  - Available → UNLOCK (wine + amber rim). It is drawn dim only when you can't afford it, as in cur_140 btn(afford).
  - Unlocked → SIGN (wine + amber).
  - Signed → REMOVE (blood red + amber).
  - Locked, Keystone slot full or Pact full → no button.
  - Labels are mouse-only and centred, in Press Start 2P 12.
- **Detail panel, per cur_140 panel():** name, NOTABLE/KEYSTONE · SUMMON/BONE, a coloured state square + state text, then one of:
  - NEEDS (locked)
  - COST n SOULS (+1 SIGIL) (available; red if you can't afford it)
  - PICK n/8 (+ KEYSTONE SLOT k/2)
  Then a rule and the description (wrap 26, up to 7 lines). No buttons, no keyboard hints. A small mouse "BACK" link sits top-right. Esc / pad B still go back; pad A still runs the same action.
- **Node wording:** SIGNED / UNLOCKED / AVAILABLE / LOCKED / MAX 2 KEYSTONES. TAKE and OWNED are gone from this screen. The focus marker is small orange corner brackets (no white).
- The v130b layout is unchanged.
- Build v0.0.54 (iteration): 0 error CS. NOT play-tested, no screenshot.

## t579u — Build v0.0.55 (2026-11-01 07:50, UTC+7)
- Version bumped 0.0.54 → 0.0.55 (build and package arguments; there is no in-code version constant).
- Linux build: 0 error CS, 0 warnings.
- `Builds/Hellord_Prototype_v0.0.55.unitypackage`, MD5 f844e1d28754e9dc7980ac54d9aab4f9.
- Includes the Necromancer Pact v140 single-action UI: HellordAltar130.cs with Btn140 and UNLOCK/SIGN/REMOVE, no TAKE/EQUIP. Also the fonts and the necro130 art.
- Copied to C:\Users\Admin\ on DESKTOP-FN5KPPL. PC MD5 matches. NOT play-tested.

## t580u — SIGN needs signed parents (01 Nov 2026, UTC+7)
- You can SIGN a node only if all of these hold: it is UNLOCKED, a parent is SIGNED by the same req graph ('/' means any one parent; "2A"/"2B" means any 2 Notables of that group signed), PICK has room (8 max), and for a Keystone, KEYSTONE SLOT has room (2 max). If any check fails, the SIGN button is hidden. A node that is unlocked but has no signed parent shows "UNLOCKED - SIGN PARENT FIRST" and "SIGN FIRST: <req>".
- The rule is also enforced in the logic (Primary97/PrimaryLab97), so pad A and the mouse are both blocked.
- REMOVE is blocked while another signed node depends on this node as its only signed parent (message: "REMOVE <child> FIRST").
- Fix for old saves: when the tree opens, any signed node with no signed parent is unsigned. Toast: "UNSIGNED NODES WITHOUT A SIGNED PARENT".
- UNLOCK still checks the req graph against unlocked parents. Built v0.0.55 (no version bump, not packaged). Not play-tested.

## t581u — Main-menu Damnation Pact seal shows the live Pact (01 Nov 2026, UTC+7)
- The seal art has 6 ring orbs. All 6 are now live; before this change only the top 2 were. Going clockwise from the top-left, orb k shows the k-th signed node of your Path in node order: flame = Notable, crown = Keystone, dark = empty. Signed nodes #7 and #8 show as 2 amber pips next to the skull.
- PACT n/8 uses SelCount51(), the real number of signed nodes. It matches the orbs and pips, and old invalid saves are cleaned up first by the t580u check.
- The orbs are drawn after the seal hover brighten, so they never get washed white. The hovered orb gets thin orange (f77622) corner brackets, not a white outline.
- Built v0.0.55 (no version bump, not packaged). Not play-tested.

## t582u — Minion max level, smarter minions, decor removed (01 Nov 2026, UTC+7)
1. **Summon max level.** This is 5 now; see t587u.
2. **Minion AI (HellordNecro536 tick).**
   - Target pick (`PickT582`):
     - focus: keep the current target while it is alive and inside the leash;
     - otherwise the demon nearest the hero within `minionThreatR582` = 3m;
     - otherwise the minion's own nearest within `minionAggroR582` = 6m, still inside the leash.
   - Warriors:
     - chase at speed x`minionChaseMul582` = 1.15 to a flank slot around the target (spread by minion index), so they don't clump in its centre;
     - swing when in reach (reach + half the enemy radius);
     - go home when there is no target.
   - Archers:
     - hold `archerStandoff582` = 3.5m;
     - back off at `archerKiteSpeed582` = 4 m/s when a target is closer than `archerKiteR582` = 2.2m (they never leave the leash; at the edge they slide back to the ring);
     - step in when out of range;
     - shoot only within range;
     - ring the hero at 3.5m when idle.
   - Both:
     - spacing push of `minionSepR582` = 0.6m between minions and `minionHeroGap582` = 0.8m from the hero (strength `minionSepPush582` = 6);
     - there is no jitter when they are home;
     - the existing leash (7m) and teleport (10m) are unchanged.
   - There is no pathfinding (none existed), so minions can still walk into walls.
3. **Decor.** `decorEnabled582` = false by default. `BuildTorture52` clears and returns, so no torture set pieces, mortar pit, sacks, rice, ice blocks, damned souls, watchers, rope monk or breakable decor spawn. That means no decor collision and no decor "enemies". Set it to true to bring them back.
- Built v0.0.55 (no version bump, not packaged). Not play-tested.

## t584u — Minions can be attacked, walls block them, no walking through the hero (01 Nov 2026, UTC+7)
- **Enemies vs minions.** Already in the game before t584u: demons touching a minion hurt it (contact dps x minionHurtMul536), and enemy bullets that hit a minion's body are blocked and hurt it (Block561). What was missing is aggro: demons only ever chased the hero. NEW: a normal demon chases the nearest minion that is within `enemyMinionAggro584` = 3.5m AND closer than the hero. Bosses, HookThrower and GossipImp keep their own movement. Contact damage to the hero still uses the real hero distance.
- **Walls.** Minions now use the same ClampToRoom(…, 6px) as demons: room walls, room-shape pits and blocks. They slide along the edge. Teleport-back is clamped too. There is no navmesh, so they can get stuck behind a corner until they teleport.
- **Hero.** The soft spacing push goes up from 6 to 10 (`minionSepPush582`). There is also a hard minimum distance `minionHeroHard584` = 0.6m: a minion inside it is pushed out to the edge every frame, so minions can't walk through the hero.
- Built v0.0.55 (no version bump, not packaged). Not play-tested.

## t585 — Summon Skeleton skill icons wired (01 Nov 2026, UTC+7)
- VH approved the step37p icons (`Art/previews/step37p_summon_skill_icons_pixellab/summon_skeleton_{warriors,archers}_24.png`). They are now slots 10 and 11 of the approved 24px sheet `Assets/Hellord/Resources/HellordArt/ui_v44_skill_icons_24.png`, which grows from 276x24 to 332x24 (28px stride, x=280 Warriors, x=308 Archers). The old sheet is backed up as `ui_v44_skill_icons_24_BEFORE_276.png` in the preview folder.
- No code change: the skill bar (`(int)SkillId * 28`), the level-up cards and the skill screen already read the sheet by SkillId for ids ≥ ChainLightning. The generic icon 8 is gone for both summons.
- Known weak spots ship as they are (the shield rim clips, the archer string is thin). Not play-tested.

## t587u / t588u — Summon skills max Lv5 (01 Nov 2026, UTC+7)
- `summonMaxLv561` = 5, the same as every other skill (`HData.MaxSkillLevel`).
- Warriors: count 2,3,3,4,5; damage 6,9,12,15,18; respawn 6, 5.5, 5, 5, 4.5 s.
- Archers: count 1,2,3,3,4; damage 5,7,9,11,14; respawn 8, 7.5, 7, 6, 5.5 s.
- Life +10% per level. The cap of 12 minions (+4 with AK2) still applies.
- `Docs/Hellord_necromancer_summoner_skills_v0.2.md` tables are Lv1–5 only. All older summon max-level text was removed from the docs, the tuning tooltips and the code comments.
- Built v0.0.55 (no version bump, not packaged). Not play-tested.

## t589u — v0.0.56 build + package + delivery (01 Nov 2026, UTC+7)
- The version goes from v0.0.55 to v0.0.56 (t574u rule). The Linux build has 0 errors. The package is `Builds/Hellord_Prototype_v0.0.56.unitypackage` (848 entries).
- Includes:
  - t580u SIGN-needs-signed-parents;
  - t581u live seal orbs;
  - the v140 Necro Pact UI;
  - t582u smarter minion AI and decor off;
  - t584u enemy aggro on minions, wall clamp and hero separation;
  - t585 summon icons;
  - t587u/t588u summons max Lv5;
  - the existing combat FX.
- Copied to PC 686c6c3d `C:\Users\Admin\Hellord_Prototype_v0.0.56.unitypackage`. MD5 on the box and on the PC both 172ba5acc5fce76913a579eb01e6815a (match). No SendToAgent. Not play-tested.

## t590u / t591u / t592u — Minion FSM rewrite, PoE-researched (01 Nov 2026, UTC+7)
- Research is in `Docs/Hellord_poe2_minion_ai_research.md`. Sources: PoE Wiki Minion and Convocation pages, PoE2DB, Myrtana PoE2 wiki, PoE2Path, Reddit, Steam, PoE forum.
- The old hug-follow code (`Home536` ring at 1.8m, PickT582 inside a 7m leash) is replaced by `Brain590` in HellordNecro536.cs. Every tick it re-checks the state:
  - **ROAM:** no enemy. Wander to random points on a 4–6m ring around the hero (`minionRoamMin590`/`Max590`), at 0.55x speed, waiting 1.5–3.5s between points.
  - **SEEK:** the target is (1) whoever just hit the hero, (2) the nearest demon within 3.5m of the hero, else (3) the demon nearest the minion within `minionSeekR590` = 8.5m of the hero. Warriors skip a target that already has 3+ warriors. Warriors run at 1.15x speed to a flank slot. Archers move to 5m standoff and shoot while closing in.
  - **FIGHT:** warriors swing in reach. They go back to SEEK only when 0.35m beyond reach, so they don't jitter. Archers fight within 90% of 8m range, kite back at 4 m/s when a demon is within 2.5m (or strafe if a wall or the leash is behind them), and shoot.
  - **RETURN:** the target is gone, or the minion is more than 7.5m from the hero (`minionReturnR590`). It jogs back to a ring point, then switches to ROAM.
  - **UNSTUCK:** no progress (under 0.25m) for 0.8s gives a new point and drops the target. Stuck for 2s means it blinks to the ring.
- Kept: wall clamp, the 0.6m hard gap from the hero plus soft spacing, enemy aggro on minions, 10m teleport, summons max Lv5.
- Not play-tested.

## t593u — v0.0.57 build + package (01 Nov 2026, UTC+7)
- Version bumped 0.0.56 → 0.0.57. linux_build v0.0.57: Succeeded, 0 error CS, 0 warnings.
- Package: Builds/Hellord_Prototype_v0.0.57.unitypackage (848 entries, 24,859,065 bytes), MD5 195ea6afef088fc605826cad959c9d0f.
- Contents: PoE2-researched minion FSM (ROAM/SEEK/FIGHT/RETURN/UNSTUCK), Lv5 summons, decor off, enemy aggro on minions, wall and player separation, SIGN parents fix, seal orbs, v140 Necro Pact UI, summon skill icons, combat FX.
- Delivered to PC 686c6c3d at C:\Users\Admin\. The PC MD5 matches the box MD5. Not play-tested.

## t594u — Archer arrows are real projectiles, damage on impact (01 Nov 2026, UTC+7)
- Shoot590 no longer deals damage. The Archer FX v7 bone arrow (19px: shaft, head, 2 feathers, 3px smear, release flash) spawns on shoot f6 (`archerReleaseS594` = 0.5s), then flies at `archerArrowSpeed594` = 12 m/s. Max flight time is `archerArrowLife594` = 1.5s.
- Damage, hit reaction, impact star, splinters and the stuck arrow are all triggered when the arrow tip actually reaches the enemy (ArrowStep594 in HellordSummonFx561.cs).
- Normal arrows home on their target. If the target dies mid-flight, the arrow retargets the nearest demon within `archerRetargetR594` = 2m. Otherwise it flies on to the last point and lands without damage.
- A6 Marrow Shot (every 3rd arrow) is a lane arrow out to max range. It hits each demon once, at the moment the tip passes it, and draws a lane trail behind the tip.
- Warrior damage stays on frame 0, synced with the slash FX. This supersedes the old "archer damage stays on frame 0" rule.
- Build v0.0.57: 0 error CS. Not play-tested.

## t601u — v0.0.58 build + package (01 Nov 2026, UTC+7)
- Version bumped 0.0.57 → 0.0.58. Build: Succeeded, 0 error CS, 0 warnings.
- Package: Builds/Hellord_Prototype_v0.0.58.unitypackage (848 entries, 24,859,585 bytes), MD5 36f8125ee32cb554d247a4894c347b4a.
- Includes the t594u archer arrow (travels, damage on impact) on top of everything in v0.0.57. Warrior melee still lands on frame 0.
- Delivered to PC 686c6c3d at C:\Users\Admin\. Not play-tested.

## t602u — Skeleton anim / AI anti-thrash (01 Nov 2026, UTC+7)
- **Causes:**
  - Skel561 picked walk vs idle, and the facing direction, from the per-frame position change. Separation pushes, hero projection, wall clamp and demon contact all nudged minions by tiny amounts, so walk/idle and S/E/N flipped every frame.
  - The attack anim re-read the live facing every frame, so it could turn mid-swing.
  - FIGHT↔SEEK flipped whenever knockback pushed the target just past reach (0.35m dead zone, no minimum time in the state).
  - Warriors moved during the swing.
- **Fixes:**
  - The anim now follows intent: walk only while the brain is stepping, held for `minionWalkHold602` = 0.15s.
  - Facing comes from the brain, with `minionDirHyst602` = 1.35 hysteresis on the E vs N/S choice. Left/right only flips on a clear 30% x component.
  - Attack/shoot anim locks: facing is frozen when the attack starts, and the anim plays to completion. Warriors don't move for `warAtkLock602` = 0.45s, and FIGHT can't be left during `warAtkLock602` / `arcAtkLock602` (0.45/0.55s).
  - FIGHT→SEEK needs the target to be 0.6m beyond reach (`minionFightExit602`) AND at least 0.4s in the state (`minionStateMin602`). SEEK re-evaluates only after 0.1s.
  - Move dead zone `minionMoveDead602` = 0.2m.
  - Hurt is a 0.1s red tint flash with a 0.35s cooldown. It never resets the anim.
- Kept: the travelling archer arrow with damage on impact, the warrior frame-0 hit with slash, wall clamp, separation, Lv5, the PoE2 FSM. Build v0.0.58: 0 error CS. Not play-tested.

## t603u — v0.0.59 build + package (01 Nov 2026, UTC+7)
- Version bumped 0.0.58 → 0.0.59. Build: Succeeded, 0 error CS, 0 warnings.
- Package: Builds/Hellord_Prototype_v0.0.59.unitypackage (848 entries, 24,860,812 bytes), MD5 29c8ba278b49ae87d4a1079cf113819c.
- Adds the t602u skeleton anim lock, facing hysteresis and state buffers on top of everything in v0.0.58 (arrow damage on impact, PoE2 minion AI and the rest).
- Delivered to PC 686c6c3d at C:\Users\Admin\. The PC MD5 matches the box. Not play-tested.

## t604u — CRASH fix: IndexOutOfRange in Brain590 (01 Nov 2026, UTC+7)
- **Root cause:** HellordNecro536.cs:190, `var e = _en[t];` in `case F590` (the same pattern is in `case S590`).
  - The t602u attack lock keeps a minion in FIGHT/SEEK for 0.45–0.55s after a swing or shot starts.
  - If the target died during that window, PickT590 returned t = -1. The transition code no longer switched the minion to RETURN because the lock blocked it. So the switch ran `_en[-1]`.
- **Fix:**
  - Before the switch: if the state is SEEK or FIGHT and the target is not alive, clear `m.tgt`. While the lock is still running, the minion just stands still (the swing anim finishes). After the lock, it goes to RETURN.
  - Swing590 and Shoot590 return early on an invalid target.
  - `LiveEn594` checks the null array, bounds and null entries.
  - PickT590 bounds-checks `m.tgt` against `_en.Length`.
  - Arrow homing already used LiveEn594.
  - The respawn queues are capped at 16 and other list accesses are index-safe (reviewed, unchanged).
- Build v0.0.59: 0 error CS. Not play-tested.

## v0.0.60 build + package (t604u crash fix delivered) (01 Nov 2026, UTC+7)
- Version bumped 0.0.59 → 0.0.60. The version is passed as a build/package argument; the source code has no version string to edit.
- Build: Succeeded, 0 error CS, 0 warnings.
- Package: Builds/Hellord_Prototype_v0.0.60.unitypackage (848 entries, 24,861,014 bytes), MD5 59a83d04d8bea01360a2e250668ed93b. Includes the t604u Brain590 dead-target fix.
- Delivered to PC 686c6c3d at C:\Users\Admin\. Not play-tested.

## t605u – Arrow visuals, white line, summon levels (compile only, v0.0.60 build arg)
- **White line**: two off-white full-alpha lines: (1) the A6 Marrow Shot pierce "lane" (2px #ead4aa line from bow to arrow tip for the whole flight), (2) Splinter (node A11) chain lines (near-white RGB .95/.92/.82, alpha 1, 0.06 m thick). Lane removed (replaced with a 4–8px fading tan smear behind the nock); Splinter lines now bone-tan, 60% alpha, 1px, 0.08 s.
- **Arrow**: redrawn as a clean 14px pixel bone arrow from code primitives (no AI art): #181425 outline, #c28569 tan shaft (no longer near-white), ivory/peach head (white tip pixel removed), red #e43b44/#a22633 fletching for readability on dark floors. Stuck arrow sits shallower and fades out at 75% life. Impact damage (t594u) unchanged.
- **Levels**: already stored per skill (`SkillInst.level` on each skill in `skills`; level cards use their own skillIndex; new skills start Lv1). The extra warriors were A7 Ossuary Order temp warriors (up to 3, every 20 kills) which spawned even before you owned Skeleton Warriors, plus +1 from A1. Fix: Ossuary only spawns while Skeleton Warriors is owned; status line now shows "warriors N+T temp, archers M". Warrior Lv1 = 2 (+1 with A1).

## t606u – Diablo 2 darkness (compile only, v0.0.60 build arg; tunables tagged v0.0.61)
All behind `d2Dark606` (HellordTuning, header "t606u Diablo 2 darkness"); off = v0.0.60 look. Gameplay sprites (hero, enemies, bullets, pickups) are never graded, so they still read on the darker floor.
- Ambient: floor/prop grade × `d2Ambient606` 0.6 (was 1.0). Shadow lift × `d2ShadowLiftMul606` 0.4 (deeper blacks).
- Hero light circle: the screen-edge fog (off since v0.0.46) returns as a dark vignette: `d2VignetteAmt606` 0.7 (was 0), inner `d2VignetteInner606` 0.3 (was edgeFogInner 0.45), colour Endesga #181425 (was ember red). Low-HP red pulse unchanged.
- Torch pools tighter + warmer: brazier pool radius 26→20 px, strength 0.20→0.28; skull torch glow 16→12 px, 0.22→0.30; glow colour (0.97,0.46,0.13)→(0.97,0.40,0.10); room light sprites ×0.9→×0.75; embers alpha ×0.7.
- Banded/Bayer-dithered pools and Endesga ramps untouched.

## t556u / t607u – Arena lighting retuned toward step38c option D (compile only, v0.0.60 build arg)
Target: Art/previews/step38c_arena_lighting_real/D_darker_tight_MOCKUP.png (ambient 1.2 ramp steps, brazier r90 / hero r70 at x2, near-black purple surround, small warm brazier pools, units/flames/HUD bright). Same `d2Dark606` switch (off = v0.0.60 lighting). Before (t606u) -> after (D):
- d2Ambient606 0.60 -> 0.42; d2ShadowLiftMul606 0.40 -> 0.25
- d2VignetteAmt606 0.70 -> 0.82; d2VignetteInner606 0.30 -> 0.22
- d2FirePoolRadiusPx606 20 -> 18; d2FirePoolK606 0.28 -> 0.36; d2TorchK606 0.30 -> 0.36 (radius 12 kept)
- d2RoomLightBoost606 0.75 -> 0.70; d2EmberAlpha606 0.70 -> 0.60
Units stay ungraded (brighter than floor); HUD/flames unaffected. Gap vs mockup: engine darkens with a grade multiply + screen vignette, not the mockup's per-pixel Endesga palette-ramp LUT with Bayer bands; true palette-shift lighting needs the light-RT + LUT shader (not built).

## Menu audio audit (Hellord Sound), 2026-11-01
- Wrote `Docs/Hellord_menu_audio_map.md`, covering the menus, navigation, existing audio hooks and recommended attach points for music and SFX. No code changes, no audio assets, no build or package.
- Key findings:
  - Main menu, altar and end screens have zero audio.
  - There is no menu music; the music runs only in Play and the paused-in-run states.
  - There is no pause menu.
  - The UI SFX on the level-up/chest screens may be swallowed by the pool pause.

## t611u menu music + UI SFX (v0.0.61), 2026-11-01
- Copied the assets into Resources/Audio:
  - `Music/music_menu_main`, `music_menu_altar`, `music_stinger_dead` and `music_stinger_won`.
  - `SFX/ui/*` (14 hooks). `restart_01-02` also went into `ui/` so Play resolves it.
- Menu music is a separate pair of crossfading sources (`HellordAudio.TickMenu611`) and does not touch the floor/boss logic. `HellordGame.MenuAudioTick611` sets `MenuMusic611` each frame:
  - MainMenu → main loop; Altar → altar loop (1.5 s crossfade).
  - Dead/Won → stinger once, then the main loop fades in after 3 s.
  - StartPick, Play and everything else → fade out over 1 s. Floor music still starts at Play, as before.
- The UI SFX are driven by frame-to-frame changes in menu state (so mouse, keyboard and pad all sound), plus direct calls: Msg51 → ui_denied (except UNLOCKED / YOUR PATH), Descend51 → pact_seal_descend, seal → seal_press, Reroll, SkipCards and R restart.
- Fix: UI hooks are no longer paused by the voice-pool pause on LevelUp/Chest (`uiUnpaused611`).
- Volumes come from the AUDIO_MAP MENU_AUDIO section and are exposed as `HellordTuning *611` tunables.
- Not play-tested.

## t613u — v0.0.62 menu audio "silent" fix (2026-11-01, UTC+7)
- Root cause (code side): every UI/menu hook was multiplied by the global sfxVolume (0.35) on top of its own hook vol (0.4–0.75), on clips that peak at only -24..-15 dBFS. Hover therefore came out at about -41 dBFS and confirm/back at about -31 dBFS, which is effectively inaudible over a normal listening level.
- Fix: UI hooks now use their own gain `uiSfxGain612` (default 1.6, tunable in HellordTuning, independent of sfxVolume; sfxEnabled=false still mutes them). This makes them about 13 dB louder. Card/event hooks that share the `ui` flag also get louder.
- Robustness: `TickMenu611` (menu music) now runs before the heart-clip early return in TickHeart.
- Diagnostics kept: Console lines `[HellordAudio] v0.0.62 menu hooks: ...`, `first play <hook> vol=`, `menu music -> main`.
- PC finding: on VH's project the HellordTuning.cs.meta guid (6777087a…) does not match the HellordTuning.asset script guid (7d1e0c5a…), which logs "referenced script missing". The game falls back to code defaults, so this is harmless, but asset tweaks will not apply.
- NOT play-tested in-game with audio output. The box has no working audio capture.

## t614u — supports vs Necromancer skeleton minions (2026-11-01, UTC+7)
- Finding: the level-up and boss cards DO let you socket supports on Skeleton Warriors (tags Minion|Melee|Hit) and Skeleton Archers (Minion|Projectile|Hit). Before this fix the supports did nothing. Every minion hit (Swing590 → Hit; Shoot590 → FxArcShot561 → arrow impact → Hit) used `NSk536()`, a dummy SkillInst with no supports, so socketed supports were never read. NDmg536 and NRate536 also ignored them. The cards were wasted.
- Fix (v0.0.62):
  - NSk536 now returns the real summon SkillInst, so Hit() applies its supports: Burn/Chill/Shock/Bleed (+ Burn+Shock Overload), Executioner, Knockback, Life Leech (heals the hero), Soul Harvest and Explode on Kill.
  - Added Damage multiplies NDmg536 (NSupDmg614).
  - Faster Casting shortens the minion attack interval in NRate536 (NSupRate614).
- Still NOT wired (offered for Archers but do nothing): Multi Projectiles, Pierce, Fork, Bounce, Homing. Bone arrows are single, fixed-target Fx561 shots. Marrow Shot (Necro A6) is the only pierce. Proposal: either wire them or exclude Minion skills from Projectile supports. VH to decide.
- Player-only by design: Bigger Area, Echo, More Orbitals. Their tags never fit summons, so they are never offered.
- Global passives already applied before: incDmg × minionPlayerDmgShare561, PDmg49, Necro crest and Necro pact nodes (A1–A6, Crypt, Legion, Hollow Haste, Iron Marrow). The Crit stat is not applied to minions. Targeting (PickT590) is unaffected by any support.
- Untested in-game; compile only.

## t615u — menu audio v2 (v0.0.63, 2026-11-01, UTC+7)
- VH disliked the v1 menu music, hover and node clicks. Hellord Sound made v2 files at the same paths; the v1 files are kept as *_v1_backup under Audio/ and are not copied into Resources.
- Copied into Resources: Music/music_menu_main.ogg and music_menu_altar.ogg; SFX/ui ui_hover_01-04, ui_confirm_01-02, pact_unlock_01-02, pact_sign_01-02; SFX/events ui_card_select_01-03 (softer card/altar node focus). gen_batch2.py is a manual generator and is not run by the build, so it cannot restore the old clicks unless someone runs it by hand.
- Measured peaks: hover -26, confirm -21, card_select -22, sign/unlock -20 dBFS; music about -18 dB mean.
- Levels: uiSfxGain612 stays 1.6. Hover volume 0.4 → 0.55 to make up for the quieter v2 peak (final peak about -27 dBFS). Hover rate limit 60 → 80 ms (uiHoverGap611).
- Untested by ear in-game.

## t560u — Mortar map → approved packed-bran Mortar B (2026-11-01, UTC+7, in source only)
- In-game MortarHell was using the rejected peach/orange mortarB sheets. It now uses the VH-approved darker olive-gold packed bran from step34 v95.
- Tiles copied into Resources/HellordArt/MortarHell, same filenames and sizes: mortarB_floor_variants_32px (256x64), wall_cap_16px, wall_north_face_16x32, wall_side_24x16. mortar_tileset.json is unchanged except for its note, which wrongly called the old sheet packed bran and has been corrected.
- Low walls copied from v95 lowwalls/mortarB/strips into Resources/HellordArt/LowWalls/mortar_*.png, mapped by role. All 16 sheet sizes match the game files.
  - Corners: NE/NW/SE/SW ← lowwall_corner_*.
  - Doors: door_E/N/S/W ← door_*; boss_E/N/S/W ← door_boss_*.
  - Glow ← door_glow_soft.
  - Walls: wall_E/S/W ← lowwall_E/S/W.
- lowwalls_mortar.json is unchanged. Props and torture are unchanged.
- No build or package until VH says build. Not checked in-game yet.

## t615u addendum — calmer menu music (v0.0.63, 2026-11-01, UTC+7)
- VH found the v2 menu music too scary. Hellord Sound replaced it with calmer mystic loops: main 56 BPM / 68.6 s, altar 50 BPM / 57.6 s, both -19 LUFS. The scary versions are kept in Audio/ as *_scary_backup.ogg and are not copied into Resources.
- Hover, confirm and pact SFX v2 are unchanged. v0.0.63 had not been delivered yet, so it was rebuilt under the same version number.
- This package also contains the t560u packed-bran Mortar tiles that are already in source, because it is the menu-music rebuild VH asked for.
- Untested by ear in-game.
- VH approved the calm music (21:28 UTC+7). The packaged music_menu_main and music_menu_altar have the same MD5 as the approved files in Audio/Music, so v0.0.63 (MD5 ecfb2ed8…) was shipped unchanged, with no rebuild.

## t622u — Projectile supports now affect Skeleton Archer arrows (2026-11-01, UTC+7)
- Supports are read from the real Skeleton Archers skill (`NSkill561(1)`). The numbers are the same tuning arrays the player projectiles use. t614u Added Damage and Faster Casting are kept, and no support is hidden.
- **Multi Projectiles**: each shot fires +`supMultiExtra[lvl]` extra arrows at other demons near the target (or at the same target if it is alone). Every arrow does × `multiProjDmgMul` (0.88). Marrow lane shots fan out ±8°.
- **Pierce**: after the first hit, the arrow keeps flying straight for `supPierce[lvl]` more demons, losing `pierceFalloff` (15%) damage per pierce. The Marrow Shot lane arrow still pierces without limit.
- **Fork**: on the first hit, the arrow splits once into `supForkChildren[lvl]` children in a fan (`forkSpreadDeg`), each doing `supForkDmg[lvl]` × damage. A child stops at its first hit and does not fork again.
- **Bounce**: when pierce is used up (or there is none), the arrow jumps to the nearest demon it hasn't hit within `supBounceRangeU[lvl]`, `supBounce[lvl]` times.
- **Homing**: a lost target is re-sought within `supHomingRangeU[lvl]`. Straight arrows (pierce or fork children) curve toward the nearest demon they haven't hit. Marrow lane arrows stay straight.
- Arrow lifetime resets on each pierce or bounce so chains aren't cut short. Fork children are capped by a 400-fx limit.
- Code: `HellordSummonFx561.cs` (`ArrowSup622`, `ArcMulti622`, `ArrowOnHit622`, `NearUnhit622`, `ArrowStep594` hooks) and `HellordNecro536.cs` (`Shoot590`).
- Compile: 0 `error CS`. **Not play-tested.**

## t623u: Skill/support tags like PoE2 (2026-11-01, UTC+7), PROPOSAL, docs only
- Research: `Docs/Hellord_poe2_skill_support_tags_research.md`. In PoE, visible tags categorise skills, but compatibility comes from the support's "Supports X / Cannot support Y" (hidden skill types). Minion supports act on what the minions do.
- Design: `Docs/Hellord_skill_support_tags_design_v0.1.md`. Hellord keeps one tag set for both display and matching. It proposes element, Duration, Curse and Chain tags, plus `requiresAny` / `requiresAll` / `excludes` / `category` on supports. Minion skills are tagged by what the minions do, so the t622u archer projectile supports are the first tag-driven minion case.
- Existing code already has `STag`, `SupportDef.needs` and `CanSocket`. **No code changed, nothing compiled, nothing locked**; waiting for VH's answers to the open questions in the design doc.

## t626u: Skill/support tags implemented (2026-11-02, UTC+7). VH approved design v0.1
- **Tags:** `STag` gets Chain, Duration, Curse, Strike, Physical, Fire, Cold, Lightning, Blood, Bone, Soul.
- **Support fields:** `SupportDef` gets `requiresAll`, `excludes` and `category`. `needs` is the requires-any list (`requiresAny`).
- **Match rule:** `HData.Fits` now requires any one of the needed tags, all of the requires-all tags, and none of the excluded tags.
  - `CanSocket` says which tags are missing or blocking, e.g. "needs Projectile" or "can't support Minion skills".
  - It enforces category no-stack only when a category is set.
- **Retagged skills** per the design table. Echo now excludes Minion (no minion skill has Area today, so nothing changes in practice). Other support requirements are unchanged.
- **UI:**
  - Pixel cards: a new skill card shows tag chips in its last lines, which may trim long descriptions. A support card shows a "NEEDS X [NOT Y]" line.
  - Old GUI cards: readable tags, plus "Needs ... (not ...)".
- Compile 0 `error CS` (v0.0.63, no package). **Not play-tested.**

## t628u: Safe resolutions audit (2026-11-02, UTC+7), audit only, no code change
- **How it works today:**
  - Art is 16 PPU.
  - Integer zoom `z = max(ceil(W/512), ceil(H/288))` (`camTargetPx49` = 512×288).
  - The world renders into a point-filtered RenderTexture of ceil(W/z) × ceil(H/z) and is drawn at exactly ×z. The world is always an integer scale. When W or H is not divisible by z, it crops up to z−1 screen px at the right and top edges.
  - The view is not fixed. It follows the window aspect, with no letterbox.
- **Same 480×270 view as 1080p:** 960×540 (×2), 1440×810 (×3), 1920×1080 (×4), 2400×1350 (×5), 2880×1620 (×6), 3840×2160 (×8).
- **Not 1080p-identical:**
  - 1280×720 gives 427×240 (×3, crop 1 px).
  - 2560×1440 gives 512×288 (×5).
  - Non-16:9 sizes change the field of view.
- **HUD/cards:** they use a non-integer scale `min(W/960, H/540)`, so UI pixels are uneven unless the window is a whole multiple of 960×540 (960×540, 1920×1080, 2880×1620, 3840×2160).
- **Safe zoom-out:** raise `camTargetPx49` by whole multiples, e.g. 640×360 gives ×3 at 1080p and ×6 at 4K.

## t636u: Mouse-wheel camera zoom test + CAM readout (2026-11-02, UTC+7)
- **Wheel controls (Play only):** wheel up = zoom in (smaller view), wheel down = zoom out (bigger view).
  - Until you scroll, the normal auto integer zoom is used (480×270 at 1080p).
  - The first scroll starts from 480×270.
- **Steps, small to big:** 320×180, 400×225, 480×270, 500×280 (U), 600×340 (U), 640×360, 720×400 (U), 800×450, 850×480 (U), 960×540, 1000×560 (U), 1280×720. U = deliberately unsafe.
- **How test steps render:** a test step renders exactly that size and stretches it to fill the window.
  - SAFE/UNSAFE is computed live: SAFE only if the window is an exact whole-number multiple with the same ratio. So 640×360 is SAFE at 1920×1080 (×3), but UNSAFE at 1366×768.
  - Wrong-ratio steps show "WRONG RATIO".
- **Readout:** at the bottom-centre, e.g. `CAM 640x360 x3 SAFE [wheel] win 1920x1080`. Green = safe, red = unsafe.
- **Tunables** in `HellordTuning.cs`: `camScrollZoom636`, `camResHud636`, `camZoomSteps636`.
- Code is in `HellordGame.cs` (`CamScroll636`, `DrawCamHud636`, and the render-texture size and stretch).
- Compile 0 `error CS`. **Not play-tested.**

## t637u: CAM readout SAFE/UNSAFE comes from the step list (2026-11-02, UTC+7)
- New tunable `camZoomSafe636` (bool per step). The label is no longer recomputed from the window size.
- HUD: `CAM 640x360 SAFE   [wheel]  win 1920x1080`. Green = SAFE, red = UNSAFE.
  - Before the first scroll it shows `(auto)`. It uses the list flag if the auto size is in the list, otherwise `AUTO` in white.
- SAFE: 320×180, 400×225, 480×270, 640×360, 800×450, 960×540, 1280×720.
- UNSAFE: 500×280, 600×340, 720×400, 850×480, 1000×560.
- Compile 0 `error CS`. Not play-tested.

## t638u: BUILD v0.0.64 (2026-11-02, UTC+7)
- Compile 0 `error CS`. Package `Builds/Hellord_Prototype_v0.0.64.unitypackage` (885 entries), MD5 `c63014ef2b9b4b2d9090a9b65de99a54`.
- Delivered to `C:\Users\Admin\Hellord_Prototype_v0.0.64.unitypackage`; the PC MD5 matches.
- Includes t614 minion supports, t622 archer projectile supports, t626 tags plus chips, t636/t637 cam wheel plus CAM SAFE/UNSAFE HUD, menu audio v2, the calm music, and the Mortar tiles.
- Only the audio log string was bumped to v0.0.64. Not play-tested.

## t564u – real floor lighting refs for Art (2026-11-01 UTC+7)
- Added autotest-only scenario `-atOnly floorrefs` (HellordAutoTest.cs, AtFloorRefs564): jumps to Ox Pit / Mortar / Frozen / Burning, clears enemies, 2 shots + 60-frame walk per floor at 1920x1080 (480x270 cam).
- Output: Art/previews/step38d_floor_lighting_refs/ (8 PNG, 5 MP4, NOTES.md). No package, no version bump, nothing copied to PC. Software-GL capture, not a hand play-test.

## t642u – try 640x360 default camera (2026-11-02 UTC+7)
- HellordTuning.camTargetPx49 512x288 -> 640x360. Integer zoom unchanged: 1920x1080 -> zoom 3 = 640x360, 1280x720 -> zoom 2 = 640x360, 2560x1440 -> zoom 4 = 640x360. CAM HUD shows "CAM 640x360 SAFE (auto)".
- First mouse-wheel step now starts from 640x360 (was 480x270). Steps list unchanged. Revert: set camTargetPx49 back to 512x288. Compile only, not play-tested.
- t642u compile: tools/linux_build.sh v0.0.64 -> Succeeded, 0 error CS.

## t645u – floor lighting refs at camera view 640x360 (2026-11-02 UTC+7)
- Added an autotest-only arg `-atCam W H` (only in the floorrefs scenario; sets camTargetPx49 at runtime and leaves the tuning default alone). Captured 4 floors at rt 640x360 -> Art/previews/step38d_floor_lighting_refs/cam640/. NOTES.md updated. No package or copy to the PC. Software-GL capture, not play-tested.

## t571u – arena lighting option M + smaller biggest rooms (2026-11-02 UTC+7)
- Lighting (HellordTuning, all floors): d2Ambient606 0.42->0.30; d2VignetteAmt606 0.82->0.90; d2VignetteInner606 0.22->0.34 (hero light ~190 art px at 640x360); d2TorchRadiusPx606 12->100, d2TorchK606 0.36->0.26; d2FirePoolRadiusPx606 18->100, d2FirePoolK606 0.36->0.30. Vignette colour stays #181425. Strengths were lowered so the larger pools don't wash out units.
- Rooms: roomSurviveMaxU2_49 1600->1500, roomMaxPerFloor49d 150->60 (floor 2+ max = base + per-floor x (floor-2)). Survive rooms: biggest art was 1024x576 from floor 5; now 896x512 until floor 9 (1024x576 only from floor 10). Fight rooms: 896x512 used to appear from floor 5 and 1024x576 from floor 8; now 768x432 until floor 8 and 896x512 from floor 9. Floor 1, boss rooms and the art itself are unchanged, so doors and walls are untouched.
- Autotest: added an `-atCombat` flag to floorrefs. Preview is in Art/previews/step38m_preview/.
- Compile: linux_build.sh v0.0.64 Succeeded, 0 error CS. No package, no version bump, nothing copied to the PC. Not play-tested.

## t586u — Lighting option V2 (white light layer) — 2026-11-01 21:41 UTC+7
- Replaced global darkening (d2Ambient floor multiply + vignette) with a screen light layer: L = amb + (1-amb)*union(hero flat->fade, torches 1-exp(-sum k(1-d/r)^1.3)); black alpha 1-L, white cores. Toggle: HellordTuning.v2Light586.
- Floor colours preserved (no grey/desat). Preview: Art/previews/step38v_preview/.
- Mask is applied over the whole world incl. enemies/props (no per-layer lighting). 1/4-res CPU mask (160x90) bilinear-upscaled each frame. No wall occlusion/shadow casting. No per-floor exposure (art target 40 not implemented). White cores are alpha-blended, not true additive. Torch pools are subtle in-game. Not play-tested by a human; autotest only.

## t659u — Projectile HFX audit — 2026-11-01 22:00 UTC+7
- Player Proj (Fireball, Soul Spears, Meteor-pact fireball, Marrow Launch bones): flight sprite+HellFX trail (spear: HeroSkillFX spear when fxV44Pack), impact ProjectileFX.Impact, pierce/fork/bounce sparks present. Fireball CastFlash yes; spear CastFlash only when fxV44Pack off; Meteor-pact + Marrow Launch no muzzle.
- Missing: fireball splash radius has no blast ring; no fizzle on expiry; homing has no FX; trail job 2 s vs proj life 1.6*PRange49 (can drop early); breakable/ice hits always Fire impact.
- Archer arrows: release, arrow+smear flight, star+burst impact, smaller star on pierce, stuck arrow on miss. Fork/bounce had no own FX.
- Trivial fixes wired: player sparks/impact now use skill element (Soul spear, Bone orbit, else Fire) instead of always Fire; arrows get ProjectileFX.HitSplit/HitBounce (Bone) on fork/bounce. Compile 0 error CS. Not play-tested, no capture.

## t660u — Projectile LIGHT FX audit + wire — 2026-11-01 22:39 UTC+7
- Before: NO projectile light. V2 light layer (HellordLight586) only took fire pools, warm room lights, Glow586 torches/lava + hero. Projectiles/arrows were darkened by the overlay like everything else; their HellFX trail/glow sprites are just sprites under the dark mask. No impact flash light.
- Wired: per-source weight in the light layer; alive player Proj (fireball, soul spear, meteor fireball, Marrow bones) = light k 0.5x torch; flying archer arrow tips 0.25x; impact flash on every player-proj hit and arrow hit, 0.9x decaying over 0.15 s (max 32). Tunables v2ProjLight660, v2ProjK660, v2ArrowK660, v2FlashK660, v2FlashS660.
- Gap: light layer is white-only, so no element tint (fire warm / soul / bone); boss/enemy bullets not lights; extra CPU per source. Compile 0 error CS. Not play-tested, no capture.

## t591u / t661u — Skill light list (elemental only, element-tinted) — 2026-11-01 22:46 UTC+7
- YES (light): Fireball + Hellfire Pillar (warm 1,.55,.2); Frost Cleave (ice .5,.8,1); Storm Ring + Chain Lightning (cool .7,.82,1); Bone Orbit incl. orbs + Marrow Launch bones (bone 1,.92,.75); Soul Spears (soul .55,1,.85); Curse Rune (purple .75,.4,1); Grave Pool (necro green .45,1,.45). Holy: no holy skill exists.
- NO: Skeleton Warriors, Skeleton Archers (incl. Marrow Shot lane arrows), Blood Whip (Blood/Physical melee, not in VH's YES list — flag for VH), basic/physical/DOT ticks, boss/enemy bullets.
- Wiring: elemental projectiles = flight light (v2ProjK660 0.5); Bone Orbit orbs v2OrbK661 0.35; every non-tick Hit() by a YES skill = coloured impact flash (v2FlashK660 0.9, 0.15 s, max 32); muzzle flash on Fireball + Soul Spear cast. t660u arrow light/flash removed.
- Colour: light layer now draws a third tint texture (weighted avg of tinted source colours, alpha (1-exp(-sum))*v2TintK661 0.55) between darkness and white cores; torches/hero stay white.
- Gaps: tint is alpha-blended, not additive; area skills (Storm Ring aura, Grave Pool / Curse Rune zones, Hellfire Pillar column, Chain Lightning arcs) only light at hit flashes, no persistent zone/aura glow; no Meteor flight light; CPU cost. Compile 0 error CS. Not play-tested, no capture.

## t667u — Hellord FX SkillLightFX (v2.14.0 wip) wired — 2026-11-01 22:53 UTC+7
- Copied /workspace/PixelFXPack/wip/v2.14.0_lights/SkillLightFX.cs -> Assets/Hellord/Scripts/Runtime/SkillLightFX667.cs (namespace PixelFXPack, unchanged; PixelFXPack folder untouched).
- Flight glow (Proj.lg667: Hook on SpawnProj, GlowMove each frame, GlowEnd in KillProj): fireball_flight, soul_spears_flight, marrow_launch_flight. Bone Orbit: one bone_orbit_glow per live orb (UpdateOrbLight667). Muzzle: fireball_muzzle, soul_spears_muzzle. Impact: all 9 *_impact hooks from Hit() (non-tick, YES skills only, cap 24/frame). SkillLightFX.Clear on ClearV44 (run reset).
- Kept t661u V2 tint flashes too: SkillLightFX sprites are world sprites (order 60) so the V2 dark overlay draws over them; the V2 tint is what actually lifts the darkness.
- NO: basic, Warrior melee, Archer arrows, Blood Whip, holy. Compile 0 error CS. Not run in Unity.

## t669u / t670u — SkillLightFX v2.14.0 visuals PAUSED — 2026-11-01 22:55 UTC+7
- All SkillLightFX Hook calls (flight glow, orb glow, muzzle, impact) now go through SL667(), gated by HellordTuning.skillLightFX667 = false -> returns -1, so GlowMove/GlowEnd are no-ops and no pixel-light sprites spawn. SkillLightFX667.cs stays in Assets (unused) for the redo.
- Still ON: t661u element-tinted flashes + projectile/orb light in the V2 light layer (HellordLight586), which push back the darkness.
- Waiting on: Hellord FX research redo + in-game previews using real skills. Compile 0 error CS. Not run in Unity.

## t674u — Skill light ~25% dimmer — 2026-11-01 23:19 UTC+7
- V2 light layer (live now): v2ProjK660 0.5->0.38, v2OrbK661 0.35->0.27, v2TintK661 0.55->0.42, v2FlashK660 0.9->0.68. Colours/hooks/flash length (0.15 s) unchanged.
- SkillLightFX pack (still gated off, skillLightFX667=false): new skillLightIntensity674 = 0.75 sets SkillLightFX.Intensity on every hook, so it is dimmer when re-enabled. Assets copy is v2.14.0; v2.15.0 wip exists but is NOT integrated (same Intensity API).
- Compile 0 error CS. Not play-tested.

## t675u — BUILD v0.0.65 (2026-11-02, UTC+7)
- Compiled with linux_build.sh v0.0.65: Succeeded, 0 errors, 0 warnings (the script also writes a Linux build, as before).
- Packaged as Hellord_Prototype_v0.0.65.unitypackage (887 entries, 28.9 MB). MD5 5ab38a805def97532ea6f87086d1c532.
- Delivered to C:\Users\Admin\Hellord_Prototype_v0.0.65.unitypackage. The PC MD5 matches.
- skillLightFX667 stays false (SkillLightFX visuals paused). skillLightIntensity674 = 0.75.
- No central version constant exists; the version comes from the build and package script argument.
- NOT play-tested.

## t677u — VH feedback on v0.0.65 (2026-11-02, UTC+7), source + compile only, NOT play-tested
- CRT: new tunables crtEnabled677 (true), crtMaxHeight677 (1440), crtHiResMode677 (1). Above 1440p (e.g. 3840x2160) scanlines, RGB mask, curvature and chroma are set to 0; bloom, fog and vignette stay. Mode 0 = plain upscale with no CRT. At 1440p or below the CRT looks the same as before.
- Light stacking (old): darkness = amb + screen(hero, 1-exp(-sum of torches + projectiles + flashes)). Skill sources were in the same sum as the torches, so they didn't multiply, but they saturated to full light even next to the hero. Then two more layers were drawn on top: the tint (alpha up to 0.42) and a WHITE additive core per source, which included every impact flash. A Pillar or Storm Ring hit on N enemies added N flashes and N white cores, which caused the white disc.
- Fix: skill light is a separate term, capped at skillLightCap677 0.35 and scaled by (1 - heroLight*skillHeroDamp677 0.7), and the tint is scaled the same way. Skill sources get no white core (noSkillCore677). At most 4 flashes are added per frame (flashPerFrame677). Pillar and Storm Ring flashes are x0.4 (aoeFlashMul677).
- Mortar only: mortarGradeK677 0.72 (floor grade, before 1.0) and mortarAmbK677 0.5 (V2 ambient 0.03 -> 0.015). Other floors are unchanged.
- Boss room: bossRoomScale677 0.75, 1536x864 -> 1152x640 (rounded to 32 px). Braziers, lights, props, doors, gate, herd lane, hero and boss starts are scaled. This only works on tiled floors (Frozen/Burning/Mortar); the Ox Pit's baked boss art keeps its original size.
- Compile: 0 errors, 2 warnings.

## t678u — BUILD v0.0.66 (2026-11-02, UTC+7)
- Includes the t677u fixes (CRT at high res, skill-light cap, darker Mortar, smaller boss room).
- Compile: 0 errors. Packaged Hellord_Prototype_v0.0.66.unitypackage (887 entries). MD5 faad0c5d9220fa6c87297a3691540adf.
- Delivered to C:\Users\Admin\. NOT play-tested.


## t679u — floor 2+ softening + ice mage 3-bolt volley (2026-11-02, UTC+7), compile only, NOT play-tested
VH: from floor 2 up enemy attacks are too much; the ice (Frost Caster) mage fires far too many bolts, 3 is enough (left edge, right edge, middle).
Cause: enemy Intensity jumped from 1.0 on floor 1 to 1.55 on floor 2 (1 + (1.5 + 0.15 x idx - 1) x 0.85). Bullet counts, fire rate and attack damage all scale with it, and floor damage growth is +15%/floor on top.
New HellordTuning tunables (soften679 = master switch): enemyIPerFloor679 0.12, enemyBulletMaxMul679 1.25, enemyCdDivMax679 1.3, enemyDmgGrowth679 0.08, densityPerFloor679 0.2, fcVolley679 3.
| Floor | enemy I | bullet mul | fire-rate x | attack dmg x | contact dmg growth | density /100u2 | ice mage bolts |
|---|---|---|---|---|---|---|---|
| 1 | 1.00 -> 1.00 | 0.90 -> 0.90 | 0.87 -> 0.87 | 0.85 -> 0.85 | 1.00 -> 1.00 | 1.4 -> 1.4 | 6 -> 3 |
| 2 | 1.55 -> 1.12 | 1.40 -> 1.01 | 1.35 -> 0.97 | 1.22 -> 0.97 | 1.15 -> 1.08 | 1.8 -> 1.8 | 10 -> 3 |
| 3 | 1.68 -> 1.24 | 1.51 -> 1.12 | 1.46 -> 1.08 | 1.43 -> 1.10 | 1.30 -> 1.16 | 2.1 -> 2.0 | 11 -> 3 |
| 4 | 1.81 -> 1.36 | 1.63 -> 1.22 | 1.57 -> 1.18 | 1.66 -> 1.23 | 1.45 -> 1.24 | 2.4 -> 2.2 | 11 -> 3 |
| 5 | 1.94 -> 1.48 | 1.74 -> 1.25 (cap) | 1.68 -> 1.29 | 1.89 -> 1.37 | 1.60 -> 1.32 | 2.7 -> 2.4 | 12 -> 3 |
The 3 bolts spread across fcFanDeg49 = 50 deg (-25, 0, +25). Player-side Intensity, HP growth, lava and boss scaling are unchanged. Compile: 0 errors.

## t680u: player control design decisions (Nov 2–3, 2026, UTC+7), DOCS ONLY, DRAFT, not approved to build
- Doc: Docs/Hellord_player_control_design_v0.1.md (DRAFT).
- Player-controlled skills option (PoE2-like). Movement choice: WASD or mouse click-to-move.
- No mana: cooldowns plus level-up cards.
- Skill bar: bottom-left 2x4 belt, native 24 px icons. 4 support gems per skill with unique glyphs and a numbered hover tooltip.
- Key badge: cream keycap on the icon's top-left corner (step39j V1, Art/previews/step39j_skill_bar_B_simple_keys/). Mouse slots show a mouse sign plus L/R.
- Controls page for rebinding (step40, Art/previews/step40_controls_page/): badges show the current binding, conflicts offer a swap, reset to default.
- Defaults: mouse-move mode LMB RMB Q W E R T F; WASD mode LMB RMB Q E R F 1 2. Dash Space, Interact G, Inventory I, Map Tab, Pause Esc.
- OPEN: whether auto-attack mode stays as an option; basic attack; final art approval of the bar and the Controls page.
- No code changed, no build.

## t714u–t715u: player control decisions, part 2 (Nov 3, 2026, UTC+7), DOCS ONLY, DRAFT, not approved to build
- Firing: press for most skills, hold for channels, automatic for orbit and summon skills. Aim is always at the mouse cursor.
- NO basic attack: skills only, and the player starts with 1 skill in slot 1.
- The Auto/Manual toggle stays, for testing and fun only.
- New skill card: the player picks the slot; picking a filled slot replaces the skill.
- Dash on Space with PoE2 direction (toward the cursor in mouse mode, movement direction in WASD mode). Cooldown unchanged.
- Left-click (resolves the t680u open question), PoE2 rules:
  - Mouse mode: LMB on the ground moves, LMB on an enemy uses the LMB skill, Shift+click casts in place.
  - WASD mode: LMB is just skill slot 1.
- Still open: final art approval of the skill bar (step39j V1) and the Controls page (step40).
- Doc updated: Docs/Hellord_player_control_design_v0.1.md. No code, no build.

## t716u — Player control build (2026-11-02, UTC+7)
- Built per player_control_design_v0.1 (now APPROVED / in build). Compiles: 0 error CS. NOT play-tested.
- Manual is the default (toggle on the Controls page). Auto code path is unchanged.
- The Controls page opens with O (both modes), or with Esc/Pause in Manual, because there is no pause menu yet.
- Interact, Inventory and Map can be rebound but do nothing yet.
- The Manual skill cap is 8 with no level gating. Replacing a skill deletes the old one and its supports.
- Storm Ring is press-to-pulse in Manual. Chain Lightning's first hit is the enemy nearest the cursor.
- Belt gems reuse the existing support icons. There is no summon-count tab.
- Switching from Manual (more than 3 skills) to Auto: all skills fire, but the old bar shows only 3.
- Manual as default may break autotests or bots that expect auto-fire.
- Debug letter keys turn off when a binding uses that letter.
- A/S/D bound to skills in WASD mode also moves the hero; this is not guarded.
- Casting does not stop movement.

## t717u — Build v0.0.67 (2026-11-03, UTC+7)
- Packaged t716u player control as Hellord_Prototype_v0.0.67.unitypackage (888 entries). Compile: Succeeded, 0 error CS. Not play-tested.
- MD5 88DCE34B8BE37809627A9D04489CAE6A, matches on C:\Users\Admin\ (Get-FileHash).
- No in-game version string exists in code (version comes from build arg). The "v0.0.64 t674u" tooltips are change-history labels for when those values were tuned, so I left them as they are.

## t718u — Fix: second skill card replaced the first (2026-11-03, UTC+7)
- Root cause: in Manual, slot 1 is bound to LMB. Picking the card with LMB opened the slot pick, and that same LMB press counted at once as "press slot 1", so the new skill went into slot 1 and the old skill was removed. Auto mode was not affected (OfferSlot716 returns at once outside Manual).
- Fix (HellordControl716.cs): the slot pick now waits slotPickDelay718 (0.3 s) AND for every slot key/LMB/Enter/Esc to be released before it accepts input.
- The highlighted slot is the first EMPTY one; Enter puts the skill there. Esc = first empty slot.
- An empty slot is filled with one press. A FULL slot needs a second press on the same slot (red frame + toast), via slotReplaceConfirm718 = true.
- Compiled v0.0.67 tag, 0 error CS. Not play-tested. No package.

## t720u — Option 1: Auto skills + mouse aim (2026-11-03, UTC+7)
- manualControl720=false: Auto is forced, even if a saved Manual setting exists. No hotkey casting, no slot pick (new skill = next empty belt slot), O no longer opens the Controls page. Manual code is kept.
- autoMouseAim720=true: projectiles aim at the cursor. Ground AoE (Curse Rune, Grave Pool, Hellfire Pillar) lands at the cursor clamped to groundAoeRange716 (7). I chose always-cursor, NOT a nearest-enemy fallback, so the player controls the placement. Downside: AoE can fire onto empty ground. Chain Lightning's first hit = enemy nearest the cursor. Self-centred skills unchanged.
- Grave Pool in auto+mouse aim skips the old "walked spacing / no demon" checks (it drops at the cursor on each cooldown).
- Movement: WASD/arrows as before. Click-to-move is OFF in Auto (it only exists in the Manual path).
- skillBelt720=true: the 2x4 belt (icons, cooldown, gems, hover tooltip) replaces the old 3-skill bar. It shows all owned skills (the Auto cap is still SlotsUnlocked). showKeyBadges720=false hides keycaps.
- Dash on Space unchanged. Compiled v0.0.67 tag, 0 error CS. Not play-tested.

## t726u — Sins system DRAFT v0.1 (2026-11-03, UTC+7) — design only, no code, no build
- Doc: Docs/Hellord_sins_design_v0.1.md. 40 hand-named Sins (20 Profane / 14 Damned / 6 God-Disliked) + generated Common/Cursed, drop tables, slots, Sin Altar wireframe, 4 example builds, tunables list.
- Open questions:
  1. Do Sins replace the "Relics" layer in Ascendancy/Relics v0.4? Do Pacts stay?
  2. Codex: should anything carry over between runs (record only vs. +drop weight)?
  3. Slots: 3 start, +1 per boss, max 6 — OK? Limits of 1 God-Disliked / 2 Damned?
  4. Swapping out Damned/God Sins costs HP, or free?
  5. Drop rates: boss 60/75/90%, God-Disliked 0.5% — too rare / too common?
  6. 18 Sins need new mechanics ([NEW]: return projectiles, on-kill/on-dash triggers, minions casting skills, burn stacks, shield, revive…). Which first, or cut them from v1?
  7. Can elites drop Common/Cursed, or bosses only?
  8. Rename any Sins you don't like.


## VH decision t727u (Nov 3)
- Pacts STAY. Sins do not replace anything; Sins are a NEW, additional build layer on top of existing systems (Pacts, Relics, skills/supports, cards).

## VH decision t728u (Nov 3)
- Relics and Sins are DIFFERENT systems; do not merge. Relics design (v0.4) stays as-is, unbuilt.
- Sins use ONLY VH's Sins design (t725u). Sins can add stats; MOST sins are stats-only (Common/Cursed). Special-effect sins are the rare minority.

## VH decision t729u (Nov 3) — supersedes t728u relic note
- Relics are CUT entirely (they are just another version of Sins). Only Sins (VH design) + Pacts. Relics sections in Hellord_ascendancy_relics_sources_ui_v0.4.md are obsolete.

## VH decision t731u (Nov 3) — SUPERSEDES slot/drop rules above
- Sins are META: a permanent collection unlocked across runs. Player equips chosen sins BEFORE a run (from the beginning); no slot changes during a run.
- Max 3 sin slots, unlocked with Souls + Sigils (meta currency).
- Unlock source: beating a boss = small chance to drop (unlock) a sin. Better sins = very small chance; may have special effects but must NOT be too impactful. God-Disliked = super small chance; build-changing.
- Old rules (3 start/+1 per boss/max 6, satchel, mid-run Sin Altar swaps, 60-90% drop rates) are VOID. Pending: exact rates, Soul+Sigil costs, rebalance Profane/Damned effects to be milder.

## VH decision t732u (Nov 3)
- Within the same tier, each sin has its OWN drop weight (some much rarer than others). Drop = roll tier, then weighted roll of sin within tier. All weights tunable.

## t733 — Sins v0.2 DRAFT (2026-11-03, UTC+7) — design only, no code, no build
- Doc: Docs/Hellord_sins_design_v0.2.md (v0.1 marked superseded). 100-sin meta collection: 36 Common + 24 Cursed stat sins (pattern-named) + 20 Profane + 14 Damned (now mild) + 6 God-Disliked.
- Rates: boss 12% unlock (final boss 25%); tier weights 62/28/7/2.6/0.4; per-sin weights 100/50/20/5. Duplicates reroll once, then convert to Souls.
- Slot costs: 100 Souls / 400 Souls + 1 Sigil / 1000 Souls + 3 Sigils. Shared wallet with Pacts.
- Open questions:
  1. God-Disliked ≈ 1 in 1,000–2,100 bosses. Too rare? Alternatives: raise the tier weight, or guarantee a first God-Disliked after N wins.
  2. Slot costs OK? Should slot 1 be free?
  3. Sigils are only earned on a win (sigilsPerBoss51=0). Keep, or give 1 Sigil per boss?
  4. Duplicates: reroll + Souls, or Souls only?
  5. Shared Souls wallet with Pacts, or a separate currency?
  6. 60 stat sins: is that the right size? Is the Common 3-theme / Cursed 2-theme layout OK?
  7. Are the Profane/Damned effects now mild enough, or too weak to be worth a slot?
  8. Limits of max 1 God-Disliked and max 2 Damned equipped: keep?
  9. 10 sins still need new mechanics. Cut them for v1?
  10. Should elites/chests ever unlock sins? Currently bosses only.

## VH decision t733u (Nov 3)
- Every dropped sin is saved permanently to the Sins panel in the menu.
- Mid-run: if a sin drops and the player has a free unlocked slot, it auto-equips into that slot.
- If all unlocked slots are filled, player gets a choice to swap the new sin with one equipped sin, or keep current (new sin still saved to the panel).
- Overrides v0.2 "no mid-run changes" rule. Pre-run equip from the Sins panel still applies.

## VH decision t734u (Nov 3) — replaces v0.2 drop rates
- Normal (stat) sins: drop from ANY demon at 1/1000. Rare (better) sins: any demon at 1/10000.
- God-Disliked: bosses only, 1/100 per boss.
- HM proposal (pending VH): bosses also roll normal 1/20 and rare 1/100 so boss kills feel rewarding.
- Estimate (unmeasured): ~1000-1500 kills + 3-4 bosses per run => ~1-1.5 normal/run, rare every ~7-10 runs, God-Disliked every ~25-35 runs.

## VH decision t735u (Nov 3)
- APPROVED: bosses also roll normal 1/20 and rare 1/100 (plus God-Disliked 1/100).

## VH decision t736 (Nov 3)
- Bosses also give 1 Sigil.
- Rare (Profane/Damned) sin effects: make a bit stronger than v0.2 mild versions.
- Currency: VH skipped; default = share Souls wallet with Pacts (pending).

## t737 — Sins v0.3 FINAL DESIGN DRAFT (2026-11-03, UTC+7) — design only, no code, no build
- Doc: Docs/Hellord_sins_design_v0.3.md (v0.2 superseded). Applies t733u–t736.
- Drops: demons 1/1000 normal, 1/10000 rare; bosses 1/20 normal, 1/100 rare, 1/100 God-Disliked. Max 1 sin per kill. Normal splits Common 70 / Cursed 30; rare splits Profane 70 / Damned 30.
- Duplicates: reroll once in the tier. If it's still a duplicate: when not equipped, offer it as a drop (equip/swap); when already equipped, convert to Souls.
- Slots: 100 Souls + 1 Sigil / 500 + 4 / 1200 + 10. sigilsPerBoss51 0→1. Shared Souls wallet with Pacts.
- The 34 Profane/Damned sins are rewritten between v0.1 and v0.2 strength, with Damned stronger. God-Disliked are unchanged. 12 sins need new mechanics.
- Open questions:
  1. Is the Profane 70 / Damned 30 split OK?
  2. Duplicate rule OK (offer if not equipped, Souls if equipped)?
  3. Slot costs OK with ~3–5 Sigils per run?
  4. Shared Souls wallet confirmed?
  5. Keep the equip limits of max 1 God-Disliked and 2 Damned, given mid-run auto-equip? (A drop beyond a limit only offers a swap.)
  6. Should the mid-run swap card pause the game? (Proposed: yes.)
  7. Phase 1 build scope: stat sins + panel first?
  8. Which [NEW]-mechanic sins should be cut from v1?

## t737u — Sins renamed to Eastern lore (2026-11-03, UTC+7) — doc only
- Hellord_sins_design_v0.3.md edited in place. All 40 specials and the 6 quotes are now Diyu / Ten Courts / Meng Po / Naihe / Impermanence / Vietnamese folk. Effects and numbers unchanged.
- Tiers proposed: Mortal / Hungry-Ghost / Yama-Judged / Avīci / Heaven-Forsaken. Stat sins: [Three Poisons / Ten Evil Deeds word] of [hell place].
- Open: approve tier names? Keep "Sins", or switch to Karma (Nghiệp) / Yin Debts (Nợ Âm) / Ledger Marks?

## t738u: Curse Rune AoE nerf + FPS (code, untested in Unity play)
- Root cause size: radius = level radius (4.6u Lv5) x Bigger Area (1.9) x Intensity^0.5 (~1.27 late floors) x Sloth Throne (2.0 while still) x Grave Reach/Colossus, all MULTIPLIED and uncapped: ~11u without pact, ~22u with Sloth. Echo Lv5 = 3 repeats x 3 runes = 12 full-size bursts + zones per cast.
- Root cause FPS: each rune spawns pentagram + glow + 16 embers + a 1.6 s aura zone (ticks Query + embers) with no cap; ~12 zones overlapping; every cursed death adds an explosion with Impact + burst (not capped); radius changes every floor build new big textures (Disk/Pentagram cached per exact px).
- Fix (HellordTuning, tag 738): additive area (1 + sum of bonuses), max x1.7, hard cap 7.0u (Lv5 max now ~7.0u vs 11-22u); Echo on Curse max 1 repeat, 0.75x radius, 0.8x extra dmg, 1 rune; max 6 live curse zones; zone tick 0.5 s; radius px snapped to 8 px; max 4 cursed-death explosion visuals per frame (damage unchanged).
- VH to decide: are 1.7x / 7u / 1 echo the right feel?

## t749u — Infernal Sins v0.4: 3 tiers (2026-11-04, UTC+7) — doc only, no code
- Doc: Docs/Hellord_sins_design_v0.4.md (v0.3 superseded). VH t746u–t749u.
- System name: Infernal Sins. Tiers: Mortal (normal, 60) / Demonic (rare, 34) / Heaven-Defying (myth, 6, bosses only).
- Drop rates unchanged (demons 1/1000, 1/10000; bosses 1/20, 1/100, 1/100). 70/30 sub-splits removed; per-sin weights kept.
- Dup Souls: 20 / 100 / 400 + 1 Sigil. Sin names unchanged.
- Open: equip limit max 1 Heaven-Defying, no Demonic cap? Dup values OK? Reweight ex-stronger sins now that splits are gone? Swap card pause? Phase 1 go?

## t750u — Infernal Sins tier counts 50 / 34 / 16 (2026-11-04, UTC+7) — doc only, no code
- v0.4 edited in place. Mortal 60→50: cut the middle theme of stat singles 1–10 (every stat still present). Heaven-Defying 6→16: new #41–50, 9 need new mechanics (#46 Iron Contract does not). Tunables sin41–sin50 added (tag 737).
- Mortal two-stat sins now ~48% of Mortal drops (weights kept).
- Open: approve the new 10 and the cuts; #49 Nine-Coffin Echo FPS risk (t738 caps kept).

## t751u — Infernal Sins: no tier limits, dup Souls approved (2026-11-04, UTC+7) — doc only, no code
- No tier equip limits: sinHeavenDefyingMax737=3, sinDemonicMax737=3 (no cap). 3 Heaven-Defying at once is allowed.
- Duplicate Souls 20 / 100 / 400 + 1 Sigil (Heaven-Defying) approved.

## t757u–t761u — Heaven-Defying (HD) sins: skill transforms, variable count (2026-11-04, UTC+7) — doc only, no code
- Rule: every HD sin targets one skill or pact path and transforms it for the run. It keeps the card/slot; level-ups, supports and pacts still apply. If the skill is missing from the run, the sin is dormant.
- Counts: Mortal 50 / Demonic 34 / HD variable (9 locked), so 93 total. More HD will come with new skills and pacts.
- Locked (renumbered #35–43):
  - #35 Pearl of the Dragon King (Storm Ring)
  - #36 Meng Po's Untouched Bowl (survival, open Q)
  - #37 Ten Thousand Ghost Banner (Soul Spears; stacks during the swarm)
  - #38 Needle-Throat Ghost (Blood Whip; 1 stack per 100 devoured, max 10, persists the whole run)
  - #39 Golden Crow (Fireball)
  - #40 Lamp of the Nine Nether Springs (Curse Rune)
  - #41 Ring of the Burning City (Fire Wall ring that moves with you; needs Pillar option A)
  - #42 Ghost Mushrooms of Fengdu (Grave Pool; NEW)
  - #43 Ox-Head and Horse-Face (Skeleton Warriors)
- Maybe: Eight Cold Hells (Frost Cleave; needs Freeze).
- Cut: Firecracker, Blood Pool, Taishang Furnace, Heavenly Tribulation, and the t754/t755 Bone Orbit/Chain Lightning versions.
- Remake options pending: Bone Orbit BO-A Bone Mill / BO-B Samsara Wheel; Chain Lightning CL-A Living Spark / CL-B Lightning Rod.
- Open: Pillar A/B/C; Meng Po skill-target exception.

## t762u/t763u — HD finalise round (2026-11-04, UTC+7) — doc only, no code
- Meng Po's Untouched Bowl removed from HD and shelved (possible future passive-tree keystone).
- Fire Wall LOCKED as the Hellfire Pillar replacement (spec in v0.4 §5b). Ring of the Burning City is active.
- Freeze system LOCKED (visible buildup bar, replaces the chill-count freeze; base for future ice skills). Eight Cold Hells LOCKED.
- Chain Lightning HD = Lightning Rod of the Heavenly Tribulation: the player is progressively scorched; at 20 charges the bolt hits all demons AND the player (sin44SelfStrikeHp=0.15). Art/FX request queued for Hellord Art/FX (not contacted).
- Bone Orbit HD keeps the name Samsara Wheel of Six Rings; the effect is pending: SW-A Six Paths / SW-B Rebirth / SW-C Karma Grinder.
- HD = 10 locked (#35–44); totals Mortal 50 / Demonic 34 / HD 10 = 94.

## t764u — Samsara Wheel locked (2026-11-04, UTC+7) — doc only, no code
- Option A (Six Paths) is locked as HD #45 Samsara Wheel of Six Rings (Bone Orbit). HD 11; total 95.
- FPS caps: max 6 Asura minions, 8 charmed, Hell bursts share the t738 4/frame cap, Deva heal cap 3%/s, bosses immune to charm.
- Open: path per spoke vs random per kill; whether Asura minions count for Necromancer pacts.

## t765u/t767u/t768u (2026-11-04, UTC+7) — doc only, no code
- The swap card PAUSES the game.
- #45 Samsara Wheel: each spoke always gives its own path (`sin45PathMode`=spoke).
- #45 visuals proposed (art pending VH; Art/FX not contacted): Deva gold + halo, Asura red jagged, Human ivory + bead, Animal horned green glow, Hungry Ghost cracked with mouth, Hell charred dripping fire, hub = bone dharma wheel.
- Global tag rule: anything tagged Minion gets all minion buffs (pacts, supports, sins). Asura minions, Lamp risen demons and Ox-Head/Horse-Face guardians are Minions, so Necromancer pacts apply. FPS caps are kept.

## t769u (2026-11-04, UTC+7)
- #45 Animal-path charmed demons are NOT Minions (no minion buffs).


## t770u — Infernal Sins Phase 1 built (2026-11-04, UTC+7)
- VH "go" (pt770u). Code: new `HellordSins737.cs`; hooks in HellordTuning (737 block, `sigilsPerBoss51` 0 -> 1), HellordGame (kill roll, tag dmg, XP, bleed, swap pause), HellordData (CdMul/AreaMul), HellordV49 (dash cd), HellordSummon561 (minion HP/dmg), HellordPact (run reset/apply), HellordMainMenu69 (panel), HellordUI (swap card), HellordTestMenu (SINS tab).
- Compiled v0.0.68 Linux, 0 errors. NOT packaged/delivered (waiting for VH "build"). NOT play-tested.
- Open for VH: Mortal pair names (placeholders) and pair 2nd-stat values; both themes of a pair share values; Duration sin stat has no effect yet; Demonic "+1 single/pair stat" which stat; equip only between runs except mid-run auto-equip/swap.

## t784u/t785u — ROLLED STATS for all sins (2026-11-04, UTC+7)
- VH: rolled numbers on every sin (PoE2 farming); player must clearly notice a better roll replacing the old one.
- Built: 13 Mortal singles (one per stat, roll range) + 24 pairs (each stat rolls) = 37 Mortal; total 82. Demonic/HD numbers roll (drawbacks fixed). Ranges/quality shown in Sins panel. Duplicate: better average roll replaces + BETTER ROLL card; worse/equal -> Souls toast. Save v2 (old sin save cleared, slots kept). Cheats: Force perfect roll, Grant duplicate of equipped sin.
- Compiled v0.0.68, 0 errors. Not packaged. Not play-tested.
- Open: refill Mortal to ~50 with the 13 proposed pairs (doc §4b)? Some Demonic numbers are "lower is better" (e.g. "every 25th kill") but roll like the others for now.

## t818u — Mortal pair twins merged (2026-11-04, UTC+7)
- VH: "2 stats should reroll the same" -> no twin pairs. 24 pairs -> 12 unique rolled pairs. Mortal 25, total 70 (25/34/11).
- Kept names:
  - HP+Armor: Greed and Delusion of the Iron City
  - HP+Speed: Greed and Killing of the Ghost Road
  - Damage+Crit: Hatred and Slander of the Judge's Desk
  - Fire+Area: Hatred and Wrong View of the Burning City
  - Cold+Cooldown: Hatred and Killing of the Eight Cold Hells
  - Lightning+Crit: Hatred and Slander of the Storm Gate
  - Physical+Bleed: Hatred of the Blood Pool
  - Minion+HP: Idle Speech and Greed of the Ghost Festival
  - Pickup+XP: Greed and Theft of the Hungry Ghost
  - Speed+Dash cooldown: Killing of the Black Wind
  - Area+Duration: Wrong View of the Wheel of Rebirth
  - Curse/Soul+Cooldown: Hatred and Killing of the Hanged Ghost
- Save v2 -> v3 migration: owned twins map to the kept sin, better roll kept; Demonic/HD ids shift by -12. Tunable `sinPairW737` (12) replaces `sinPairWCommon737/WRare737`.
- Compiled v0.0.68 Linux, 0 errors. NOT packaged. Migration and drops NOT play-tested.
- Open: icon sheet (step49) still shows twin pairs sharing one icon; now 1 icon per pair, so nothing to redraw. Names are VH-renamable.

## t818u (v0.0.69, 2026-11-04 UTC+7) — decisions taken, please confirm
1. Belt: 1 row of up to 5 cells (`skillSlotsMax818`). No key badges. CD number centred on the icon. Sins frame 3 x 20px slots (`sinHudSlot818`), `sinHudGap818`=3px above the belt, left-aligned to it.
2. Duration stat is live. Max multiplier is `sinDurationMax818`=2. The "(NO EFFECT YET)" text is removed.
3. Demonic/HD sins are live, with these simplifications: no taunt (#42), no body growth (#37), Bleed stands in for poison (#41), Fire Wall became a Hellfire Pillar ring (#40), no Asura/Animal spokes (#45).
4. Echo on Curse Rune: the card now says "repeats Nx: 1 smaller rune (75% size, X% dmg)" (uses curseEcho*738).
5. A test menu button "SIN SELF-TEST (t818u)" checks sin data and ids.

## t899u (v0.0.70, 2026-11-05 UTC+7) — wired
1. Skill icon sheet: your approved step51k sheet is in the game (Blood Whip = R2 tentacle on a dark red frame; Curse Rune = step51f D; Grave Pool = step51e A). File: `Resources/HellordArt/ui_v51k_skill_icons_24.png`; the old v44 sheet stays as a fallback. Compiled, NOT play-tested, NOT packaged.
2. H38 crow icon (step50p, your final): NOT wired. The game has no sin icon sheet yet: the sins frame above the belt draws each sin as a coloured box with a text abbreviation. Putting icons in needs a new sin icon sheet and drawing code. Assumed for now: wait for your go to add sin icons (H38 first, others stay text until drawn).

## t902u — Sin icons in the sins frame (v0.0.71, 2026-11-05, UTC+7) — compiled, NOT play-tested, NOT packaged
- New sheet `Resources/HellordArt/ui_v52_sin_icons_20.png` (20px cells) + `ui_v52_sin_icons_20.json` mapping. Column 0 = H38 Golden Crow of the Sun, your step50p final file copied pixel-exact.
- `DrawSinHud818` draws the icon inside the existing tier frame when the sin is listed in `sinIconNames902` (matched by sin name); every other sin keeps its text label.
- Tunables (HellordTuning): `sinIconsOn902`, `sinIconNames902`, `sinIconPx902` (20), `sinIconOffX902`/`sinIconOffY902` (0), `sinIconCell902` (20).
- Only H38 is in: no other sin icon has your approval in the docs/notes (step49–step50d sets are marked MOCK-UP / not approved; step50l D33, D24, H44, H41 unreviewed). Q: which sin icons should go next? Assumed: add each one only after you OK it.
- Preview: `Art/previews/step52_sin_sheet/sins_frame_v0071_4x_MOCKUP.png` (PIL replica of the HUD, not a game screenshot).

## t902u follow-up — Sins frame back to your approved step49d/step50p slot24 design + HD running light (v0.0.72, 2026-11-05, UTC+7) — compiled 0 errors, NOT play-tested, NOT packaged
- Why v0.0.71 looked different: the game's t818u sins frame was a code-drawn box, never the step44f/step49d slot24 mock-up you approved. Fixed now.
- Sprites are baked by the same mock-up helper code (`Art/previews/step52b_sin_slots/make52b.py`, step44f `slot24` + step48 `plate` + step44f `sins()` frame): `Resources/HellordArt/ui_v52_sin_slots_24.png` (row 0: Mortal / Demonic / HD / empty / locked; row 1: 23 HD running-light overlays) + `ui_v52_sin_frame.png` (84x32) + `ui_v52_sin_slots_24.json`.
- Check: base HD slot + crow at +2,+2 = `H38_VH_final_slot24.png` pixel for pixel; all 23 glint frames and the M/D slots equal the helper output (0 mismatches).
- HD running light loops like the step44e mock-up: 4px per 0.05 s clockwise (~1.15 s round), then 0.7 s pause. The one-shot trigger glint around the whole frame stays.
- Tunables: `sinSlotSprites903` (false = old v0.0.71 drawing), `sinRunLight903`, `sinSlotCell903` 24, `sinSlotOrigin903` 4, `sinSlotStride903` 26, `sinIconInset903` 2, `sinFrameOffX903` 0, `sinRunFrameS903` 0.05, `sinRunPauseS903` 0.7, `sinRunFrames903` 23.
- Q: should the running light loop all the time (now) or only after an HD trigger as in the step44e mock-up? Assumed: loop always, as you asked.
- Preview: `Art/previews/step52b_sin_slots/sins_frame_v0072_running_light_4x_MOCKUP.gif` / `.mp4` and `_4x_MOCKUP.png`.

## t903u — No text in sin slots (v0.0.73, 2026-11-05, UTC+7) — compiled 0 errors, NOT play-tested, NOT packaged
- Slots without an icon (and empty/locked slots) show only their slot sprite, no abbreviation. Tunable `sinSlotText903` = false (true brings the text back).
- HD running light loops always (VH confirmed); the previous Q is closed.
- Preview: `Art/previews/step52b_sin_slots/sins_frame_v0073_4x_MOCKUP.png` / `sins_frame_v0073_running_light_4x_MOCKUP.mp4`.

## t912u - all 70 sin icons wired (v0.0.74)
- VH: "approve all". New sheet `Resources/HellordArt/ui_v53_sin_icons_20.png` (1400x20, 70 cols, pixel-exact paste of the step53 sources + H38 VH final; all were 20x20, no fitting). Column map in `ui_v53_sin_icons_20.json`.
- `HellordTuning.sinIconNames902` now lists all 70 names in `HSins.All` order, taken from `HellordSins737.cs` (0 mismatches vs source filenames after normalising). Game loads v53, falls back to v52 (that sheet only has H38's column, so in fallback other sins show no icon).
- Sin self-test now flags any icon name that is not a sin and any sin without an icon.
- Preview: `Art/previews/step53_sin_review/sins_frame_v0074_samples_4x_MOCKUP.png`.
- Untested: not run in-game/play mode; self-test not executed; icon alignment on non-H38 slot bases only seen in mockup.

## t914u - HD running light runs continuously (v0.0.75)
- VH: running light must never stop. `sinRunPauseS903` default 0.7 -> 0 (still tunable).
- Checked the 23 overlay frames (`ui_v52_sin_slots_24.png` row 1): none blank (12-14 px each); the light travels clockwise around the border and frame 22 (left edge) leads straight into frame 0 (top-left corner), so the lap wraps seamlessly. No frames skipped; `sinRunFrames903` stays 23.
- Preview: `Art/previews/step53_sin_review/sins_frame_v0075_running_light_4x_MOCKUP.mp4` (MOCKUP, 3 laps, 3.45s, loops seamlessly).
- Untested in-game.

## t928u - shader FX wired: Fireball, Curse zone, Grave Pool (v0.0.76)
- New shader `Resources/HellordShaders/HellordPixelAoE.shader` ("Hellord/PixelAoE"): CG port of the approved Python samples (curse v2 veins with no ring, grave v3 with bubbles, fireball burst). Plain UnityCG, same convention as HellordLight/Atmos. Compiles in the real project, 0 errors.
- Materials + textures in `Resources/HellordFX928/` (fx928_curse/grave/fireball.mat, noise, cell, 3 ramps). The importer rule for this folder: Texture2D, no mips, no compression; noise/cell Bilinear+Repeat linear; ramps Point+Clamp.
- Hooks are visual only (no mechanics or damage changed):
  - Fireball: the shader blast fills `fbSplashRadiusU` (the real splash radius) on impact. It replaces only the old nova puffs; flash, ring, sparks and shake stay (`fxFireKeepNova928`).
  - Curse zone: the shader veins replace the AuraField sprite. AuraField still drives the ticks and damage, and each tick pulses the veins.
  - Grave Pool: the shader pool replaces HeroSkillFX.GravePool. It follows Grave Anchor moves.
- Tunables (HellordTuning): `fxShaderOn` (master; off = old FX), `fxShaderFireball928/Curse928/Grave928`, texture names `fxNoiseTex928`, `fxCellTex928`, `fxRampCurse928/Grave928/Fire928` (to swap the grave textures, drop a new png in Resources/HellordFX928 and change the name), plus all the shader params, grow/fade times, pulse, sorting order and fps.
- Autotest: new `-atOnly shaderfx928` scenario. It runs the t818u SIN SELF-TEST headlessly, writes `sin_selftest818.txt`, captures the sins frame (3 icons, 1 slot + 2 locked, empty) and 10 frames each of Fireball, Curse Rune and Grave Pool. `Builds/autotest_player` was rebuilt from the real project.
- Fixed during the test: the shader textures were imported as Cubemaps, so the shader drew white or nothing. The importer now forces Texture2D. Also fixed a .mat YAML error.
- Known: the autotest frame check `hero_sprite_not_base_final2` fails in every scenario, including the old fx26. This was already the case and is not caused by this change; the check is out of date with the current hero art.
- Untested: real GPU (only Mesa llvmpipe), WebGL, the look at full game speed, and the Grave Pool overlapping edges (Hellord FX is still fixing them).

## t930u: shader batch 3 wired (v0.0.78)
- The build is **linux_v0.0.78**. The linux_v0.0.77 build is kept but broken: the PixelAoE shader failed on Vulkan because a variable was named `half`, which is a reserved word. It was renamed. v0.0.78 builds with errors=0 and no shader errors.
- **Grave Pool merge:** all live Grave Pools draw as one cluster quad using a min(d/R) field, so overlapping pools show no seams. Ghost Mushroom pools get their own cluster. If there are more than 16 pools, the extra ones fall back to their own quad. Switch: `fxGraveMerge930`.
- **#40 Burning City:** opt A hand tongues on a low shader ember band. The back half is drawn under the actors and the front half over them (`fxCityFrontOrder930`). The old ZSeg ring runs only when the shader is off. Tunables: `fxShaderCity930` and `fxCityBandH930`.
- **#41 Ghost Mushrooms v2:** a lilac/slate pool with thin mycelium threads and rising spores. The burst colour is `fxMushBurstCol930`. Tunables: `fxShaderMush930` and `fxMushFlow930`. The mode-4 spore-burst shader and the hand-drawn pop were not added.
- **#14 Death Bell:** bone-white toll rings and a white gong, then the plum curse zone. RuneBurst+CurseZone still fire at k=0.6 of 0.6 s (`fxBellFlareK930`), so timing and damage are unchanged. With the shader off, the old RuneCircle is used. The code was moved into `SinBell818(at)` without changing what it does.
- **#36 Ghost Banner:** bone-white wisps with ink eyes follow the spear projectile, and the spear sprite and trail are hidden. All mechanics are still the spear's. Tunables: `fxShaderGhost930` and `fxGhost*930`.
- **Soul Spears:** SKIPPED. v2 is waiting on VH. VH's note to apply next: "the spear should more pointing like a spear, it head is too round now".
- **Autotest:** `-atOnly shaderfx928` now also runs `shaderfx930_{burning_city,ghost_mushrooms,death_bell,ghost_banner,grave_merge}` with 10 shots each. The sins are triggered directly, and the Burning City capture deals no damage. Run: Builds/autotest_v0.0.78_shaderfx930/. Crops: Builds/t930u_review/.
- **Cleanup:** removed the autotest frame folders autotest_v0.0.20 to v0.0.27 (older than v0.0.75). No builds were touched.
- **Untested:**
  - a real GPU (only llvmpipe/glcore has been tried; the Vulkan compile is clean but has not been run), and WebGL
  - full-speed feel and real sin triggers in play (the 25th kill, the soul count)
  - pooled projectile reuse for ghosts over a long run
  - the merge at more than 16 pools
  - the grave_merge capture, where all pools land at the same fixed autotest aim point, so they overlap completely. The partial-overlap merge is shown in grave_pool_*_poolarea crops.

## v0.0.79 (t934u) — shader FX batch + Nezha Fire Wheel
- **Nezha Fire Wheel (sin 19)**: dash now rolls a shader fire wheel that hits + knocks back enemies and leaves a burn trail. Damage scales with hero damage, size with Area, trail time with Duration. Tunables in HellordTuning (t934u block): sin19Wheel934, sin19WheelR934=1.0, sin19TrailW934=0.6, sin19TrailS934=3, sin19Knock934=0.6, sin19ReHit934=0.66, sin19WheelMul934=1 — **numbers NOT confirmed by VH, please tune**.
- Sins-frame one-shot glint removed (sinFrameGlint934=false); HD slot running light kept.
- New shader versions: Soul Spears v2 head, Hellfire Pillar v2 (PixelShock ripple + chips, no ring), Nine-Forked Lie split nodes, Tribulation v2, Death Bell v2 ripple, Golden Crow sun v2, Death Burst (11 chunks + 2-frame pop), Hit shards (crit-only by default). All fx* tunables in HellordTuning.
- Blood Tentacle: blood-red palette only (fxBloodRed934). The draft's mode-9 shader strip was NOT ported.
- Pipeline note: the game actually runs **Built-in** (the referenced URP asset is missing), so "URP Opaque Texture" doesn't apply; PixelShock uses a GrabPass and draws between floor and units (sort -19980).
- Hero "placeholder box": the hero sprite is the real hero_36_base art (check fixed, passes); the white marks are the existing hero ring/trait overlays, not a placeholder.
- Untested: Nezha feel/balance, Vulkan/WebGL runtime, real-GPU look (tested on software GL only), Death Bell v2 in live play.

## t945u — smooth review clips (autotest `-atRec`)
- Review clips are now recorded with `-atRec 60`: the game advances exactly 1/60 s per frame and every frame is saved, so clips play smoothly at 60 fps even though the test machine renders slowly (software GL). Crop 640x360 around the action, scaled x2 nearest-neighbour.
- Test-only (autotest player); no gameplay change. Script: `tools/rec_review.sh <scenario> <outDir>`.
- Nezha wheel scenario in rec mode: hero chains real dashes across the room (3 right, 3 left), no teleports.

## v0.0.80 (t948u) - decisions / open questions
- Fire Wall replaces Hellfire Pillar (spec v0.4 §5b), toggle `fireWallOn948` (off = old Pillar). Wall across aim 3u ahead, len 5u +0.4/lvl, thick 0.6u, 4s +0.6/lvl, tick 0.5s at 0.35x damage. Lv5 adds a 30° forward V (3 segments). Lv8 second V unreachable (max level 5). Icon still the Pillar icon. NOT confirmed by VH: tick dmg mul 0.35, #40 Burning City ring x0.7 with Fire Wall.
- Fire Wall aims like other skills: at the cursor when `autoMouseAim720` is on (default), else nearest enemy.
- Blood Whip dark-red FX; hit tint now 0.3 for 0.1s (was 0.5 for 3s). Bleed droplet overlay unchanged (still lasts the bleed).
- Lightning: tint 0.25 for 0.1s; shock FX 0.35s max (was the whole shock).
- Boss telegraphs (stomp rings, cones, pattern warns, puddles) sort at -19985 = under units. Stampede/Bloodrage/Punish left as is.

## v0.0.81 (t957u) - Blood Whip colour fix, Fire Wall shader wired, Horse-Face foot rings removed
- **Whip "white af" cause:** the project renders in Linear colour space. HeroSkillFX draws the whip as a vertex-colour mesh (Sprites/Default), and vertex colours are not gamma-converted, so the sRGB hex colours came out brightened on screen (#a22633 looked like ~#d06e80, #e43b44 like ~#f6a9c1 pale pink/white). A runtime dump confirmed the mesh only held 181425 / 3e2731 / a22633 / e43b44. An older toggle bug that could leave the cream/magenta palette on is also fixed (ApplyWhip is now a pure function of the flags).
- **Fix:** the whip palette is converted to linear (`CL()` in HeroSkillFX.ApplyWhip), so the screen shows the exact Endesga colours. Every whip part (body, outline, underside, sucker row, glint, splat, crown, puddle, droplets, hit flash, debris, heal motes) uses only the 3 tuning colours. No white anywhere.
- **Tuning:** `whipColDark957=0x3e2731, whipColMid957=0xa22633, whipColHi957=0xe43b44`. Mechanics unchanged.
- **Open question:** other HeroSkillFX / StatusHazardFX skills have the same linear brightening (approved looks were judged that way, so they are left alone). Say if you want them corrected too.
- **Fire Wall:** HellordPixelFireWall shader wired (fx948 ramp Point/Clamp, fx928_noise128 Bilinear/Repeat, uncompressed, no mips). `fireWallFlameH948=22, fireWallTongue948=0.889`, `_Scorch=min(1,age/fireWallScorchS957=0.133)`, quad padded down `fireWallPadDownU957=0.3`u, 1px embers at sort `fireWallEmberOrder957=-19988` (`fireWallShader957` / `fireWallEmbers957` toggles; falls back to PixelAoE mode 8 if the shader is missing).
- **Horse-Face:** the 20px StompRings at the feet during windup are removed behind `horseNoFootRings957=true`. Strike timing and the sack throw are unchanged.
- Clips: Builds/t957u_review/clip_blood_whip.mp4, clip_firewall_l1.mp4, clip_firewall_l5.mp4, clip_horseface_windup.mp4 (60fps). No package, no publish.

## v0.0.82 (t965u) - whip value contrast, white circle under the hero identified
- **Whip "all red, weird":** v0.0.81 mapped every whip part onto 3 reds, so it read as one flat blob. It now has value roles (all Endesga, linear-converted): outline + handle `181425`, body `3e2731`, underside band `a22633`, lash edge / sucker row `e43b44`, a 1px travelling warm glint `f77622` on the lash, splat core 1px glint. Hit flash = `feae34` core + `f77622` corners; heal motes = `feae34` core + `e43b44` trail, so both read apart from the body. No white or pale pink. Tunables: `whipRoles965` (off = v0.0.81 flat reds), `whipOutline965, whipBody965, whipUnder965, whipEdge965, whipGlint965, whipFlashCore965, whipFlashRim965, whipMoteCore965, whipMoteTrail965`. Mechanics unchanged. The flat red on *enemies* in the first hit frames is the separate hit tint (0.3 for 0.1 s), unchanged.
- **White circle under the hero:** it is **HurtFX #138 "IFrames"** (PixelFXPack HurtFX.cs `HurtRunner.IFrames`, mesh object `HurtFX`, Sprites/Default, sort order 32020, silver `c0cbdc` with dark backing): a rotating dashed ellipse at the feet that shrinks while the hero is invulnerable after being hit (heroIFrame 0.7 s). It is a real gameplay indicator, so it is **kept**. Its front arcs sort above the hero (32020 > 32000), which made the "white bands across the hero".
- **Why it was in every clip:** the autotest runs in god mode and sets `_iframe = 1` every frame (HellordAutoTest AtFrameChecks), so the i-frame ring showed permanently in all review captures. In real play it only appears for 0.7 s after a hit. Open question: hide it in review captures (autotest-only), make it subtler, or sort it under the hero?
- Proof: renderer dump `Builds/t965u_review/feet_renderers.txt` (3 renderers near the feet: Hero/base 32000, Hero/outline 31998, HurtFX 32020); hiding each one in turn (`review965_feet_hide00..02.png`) only removes the circle when HurtFX is hidden. Labelled crop: `hero_feet_circle_labelled.png`.
- Clip: `Builds/t965u_review/clip_blood_whip.mp4` (60 fps, autotest `review965`), contact sheet `whip_contact.png`. Linux build v0.0.82 built; no package, no publish.

## v0.0.83 (t969u / t970u)
- **I-frames:** the HurtFX silver ring is OFF (`iframeRing969=false`). I-frames now show as a shader on the hero sprite (`Hellord/HeroIFrame`, Endesga palette, pixel-snapped, 12 fps step). `iframeShaderMode970` picks the look: 0 = A soul shimmer + 1px ember outline, 1 = B dither dissolve, 2 = C heat/scanline wobble with a dark-red tint. Nothing is drawn over the hero, and there is no white. Clips: `Builds/t969u_review/clip_iframes_A/B/C.mp4`. **VH: pick A, B or C.**
- **I-frame look DECIDED (t972u, 2026-11-05):** VH picked **C** (heat/scanline wobble, dark-red tint). `iframeShaderMode970` default is now **2** in HellordTuning.cs. Source-only change; not yet in a build.
- **Autotest god mode** is now its own flag (`HeroInvuln969`). It no longer forces `_iframe` every frame.
- **Boss shader FX** (fx_audit/boss_shader/wire), behind `bossShaderTells950` (ON):
  - Tells46 ground tells (circle, drop, lane, cone and ring) now run on PixelBossWarn, drawn on the same quads with the same sorting under the boss. Hitboxes and timings are unchanged.
  - Horse-Face tells use the wind grain.
  - Ox/Gorilla/Qilin/Horse `FireBB` bullets are drawn as PixelBossOrb quads (Horse uses the vortex). Their motion is the same linear velocity as before.
  - The Horse rice sack gets the slate tint plus the wind-wrap quads.
  - Skipped as asked: Frost Ox, the Gorilla Shard Fan and the Flame Breath v2 numbers. The vertical mirroring is left as authored.
  - Not wired yet: the PatternWarnFX bullet-pattern warnings (the Ox stomp/radial polygon rings), the EB49/P46 projectiles, PixelBossGround modes 0–8, and the PARTICLES.md CPU particles.
  - Clips: `Builds/t970u_boss_review/clip_ox/gorilla/qilin/horseface.mp4`.
- **Fix:** `UpdateP46` used to throw "Collection was modified" when the Ox toss shards split mid-loop. It now uses an index loop.

## v0.0.84 (t974u) - boss telegraphs: last old rings moved onto the shader
- **Why every boss showed the same warning in the v0.0.83 clips:** the `boss970` review forced a **Stomp** on all four bosses. Stomp still drew the old **TellFX.StompRings** (crimson polygon rim, gold clock ring, star, white/gold shock rings), so every clip opened with that ring. Gorilla and Qilin never stomp in real play; they use their own attacks (StartAtk44). In live play PatternWarnFX was already unused (`bossPatternsV46=true`), and the per-boss pattern tells were already on the shader.
- **Stomp (Ox, and any non-Horse boss that stomps):** StompRings is replaced by PixelBossWarn (`bossStompShader984`, ON), in the Ox Stomp v3 mock-up look.
  - Windup: a dissolve-grain ellipse at the 7 u reach (y × 0.6) fills over 0.8 s.
  - Strike: one dissolve wave band (±0.6 u) per shock ring. It follows the real hitbox radius (7 u × t/0.55 s), with a Shock934 ripple and a dust puff.
  - Colours are per boss: Ox orange, Gorilla ice, Qilin fire, Horse wind. Drawn under the boss (sort -19985).
  - The strike beat is the same as before (0.8 s, then rings every 0.3 s). Squash, strike anim/audio, kick/hit-stop/shake, hitbox, damage and timings are unchanged.
  - Note: the old ring was capped at 96 px (6 u) while the hitbox reached 7 u. The new band shows the true 7 u.
- **PatternWarnFX** (radial burst, ring, fan, spiral, stomp burst warns) → self-timed PixelBossWarn quads for the same lead (`bossPatternWarnShader984`, ON): bands around the muzzle and a wedge for the fan. These only appear if the old pattern cycle is switched back on (`bossPatternsV46=false` or `oldStompBurstV46=true`). Bullets are unchanged.
- **Unchanged:** Horse-Face still has no foot rings (`horseNoFootRings957`). Frost Ox keeps the old look, because it was skipped in the shader work.
- **Autotest `boss974`** runs each boss's own attacks: Ox stomp + quake, Gorilla Glacier Slam ×2, Qilin breath + hoof meteors, Horse-Face spin + sack. It counts frames where old visuals are alive. Result: StompRings frames 0 and PatternWarnFX frames 0 for all 4 bosses, 0 console errors, PASS.
- Clips: `Builds/t974u_boss_review/clip_ox_stomp_quake.mp4`, `clip_gorilla_slam.mp4`, `clip_qilin_breath_meteors.mp4`, `clip_horseface_spin.mp4` (60 fps). Proof: `proof/before_v083_top_vs_after_v084_bottom_t0.4s.png` and `proof/contact_*.png`.
- Linux build v0.0.84. No package, no publish.

## v0.0.85 (t975u) - boss warnings darker / less shiny
- VH (t975u): v0.0.84 boss warnings "too bright and shiny". Cause: PixelBossWarn v3/wind grain quantised up to the top ramp slots (Ox/Qilin `fee761` yellow + `ffffff` white, Gorilla `c0cbdc`/`ffffff`, Horse wind `ffffff`) and filled almost the whole shape as progress rose, so the Ox windup became a dense bright-yellow disc.
- Fix: new `_Dim` on `Hellord/PixelBossWarn` (tuning `bossWarnDim985`, on; off = v0.0.84 look). Interior: sparser grain (+0.22 threshold), only the two darkest ramp slots (Ox/Qilin `3e2731`/`a22633`, Gorilla `181425`/`124e89`). Edge: capped at `e43b44` red / `0099db` ice, with `feae34` / `8b9bb4` only on sparse noise; never yellow `fee761` or white. Horse wind: slightly sparser, never white (max `c0cbdc` on edge, `8b9bb4` inside).
- Applies to every PixelBossWarn user: Ox stomp windup + wave bands, Gorilla / Qilin / Horse-Face tells, pattern warns. Frost Ox unchanged. Mechanics, timings, hit sizes unchanged.
- Clips (60 fps): `Builds/t975u_boss_review/clip_*.mp4`. Proof: `proof/before_v084_top_vs_after_v085_bottom_*.png`, `proof/autotest_summary.txt` (boss974 PASS, 0 errors, StompRings / PatternWarnFX 0 frames).
- Linux build v0.0.85. No package, no publish.

## t978u — BUILD v0.0.85 (2026-11-05, UTC+7)
- Packaged with tools/build_package.py as Hellord_Prototype_v0.0.85.unitypackage (934 entries, 29.1 MB / 29102256 bytes). MD5 48fc2982e6f0ce00b4ca168e0a6e4216.
- Delivered to C:\Users\Admin\Hellord_Prototype_v0.0.85.unitypackage. The PC MD5 matches.
- Includes the darker boss warning FX (v0.0.85). Not play-tested beyond the earlier autotests. Not published anywhere.

## v0.0.86 (t979u) - i-frame zoom fix + Fire Wall at the cursor (2026-11-05, UTC+7)
- **I-frame "zoom out":** the only thing that changes during `_iframe` is the hero material swap to `Hellord/HeroIFrame` (look C kept). Camera ortho size, camera rig and hero transform are never touched by i-frame code. The shader's vertex stage called `UnityPixelSnap`, which snaps to `_ScreenParams`; under URP 2D with the low-res RT target that is not the render target size, so the hero quad jumped in scale/position (reads as a zoom). Fix: removed `UnityPixelSnap` (same vertex path as Sprites-Default); per-texel snapping in the fragment stays, so look C is unchanged. Autotest `iframes969` now also checks camera ortho size deviation (0.0000) and hero sprite width deviation (0.00) in A/B/C. Not reproducible on the box (built-in pipeline) - please confirm in the Editor.
- **Fire Wall aims at the cursor:** with cursor aim on (`autoMouseAim720` / Manual), the wall centre = `GroundAim716()` (cursor, clamped to `groundAoeRange716` = 7 u and the room, same helper as Hellfire Pillar / Curse Rune / Grave Pool). Wall stays across the hero->cursor line. Echo wall sits `fireWallEchoGap737` closer. Toggle `fireWallCursor986` (on; off = old 3 u ahead). Length/thickness/tick/damage unchanged. Card text: "Wall of flame at your cursor (in range), across your aim."
- Other skills: projectiles already use the cursor direction (AimDir), Pillar/Rune/Pool already land at the cursor, Chain Lightning picks the enemy nearest the cursor. Fire Wall was the only fixed-offset placement found.
- Autotests: `firewall986` (far target 6.2 u: wall centre error 0.00, damage dealt; out of range: clamped to 7.00 u) PASS; `review948` Fire Wall L1/L3/L5 PASS (cursor aim off there); `iframes969` PASS; 0 console errors.
- Clips (60 fps): `Builds/t979u_review/clip_iframes_C.mp4`, `clip_far_target.mp4`, `clip_out_of_range.mp4`.
- Linux build v0.0.86. Package `Hellord_Prototype_v0.0.86.unitypackage` (934 entries, 29104620 bytes, MD5 82876114673c48c746fd8939b1418e55) delivered to C:\Users\Admin\, PC MD5 matches. Not published.

## v0.0.87 (t981u) - hero grows after the 2nd skill (Archers then Spears) (2026-11-06, UTC+7)
- Report: hero normal at start, bigger after picking the 2nd skill (Skeleton Archers, then Soul Spears).
- What changes on that pick: Archers add no hero layer; Soul Spears adds the first trait strip (`hero_36_t_SoulSpears_front`, 896x50 texture, 56x50 frames) on top of the hero (576x36 texture). Hero transform scale, sprite PPU (16), camera ortho and RT size never change (autotest measured all of them).
- Likely cause (Editor-only, not reproducible on the box's built-in Linux player): while i-frames are on, every hero renderer shared ONE `HeroIFrame` material, and the shader snapped UVs with `_MainTex_TexelSize`. Under URP / SRP batcher that value is not reliably filled per SpriteRenderer, and with two different textures (hero + trait) on one material it can be wrong, so the UVs snap to a few huge texels and the hero draws as a bigger block. With no trait layer (start) only one texture used the material, which is why it looked fixed at the start of the run.
- Fix: one i-frame material per hero renderer (base + each trait strip), each given its own texel size `_TexelPx` from C# (the shader uses it instead of `_MainTex_TexelSize`). Materials of destroyed trait layers are cleaned up on skill change. Look C, timings and mechanics are unchanged.
- Autotest `size987` (new): Archers then Soul Spears through the real card pick (Manual slot pick), 2 hits each for i-frames; hero width world 2.250 / 72 screen px all the way (dev 0.000), 65 i-frame frames with correct per-layer texel size, no shared material. `iframes969` A/B/C still pass. Clip `Builds/t981u_review/clip_size987_archer_then_spear.mp4`.
- Package `Hellord_Prototype_v0.0.87.unitypackage` (935 entries, 29108557 bytes, MD5 b5e9a7bc9f9d9a055eace372bd435052) delivered to C:\Users\Admin\, PC MD5 matches. Not published. NOT tested in VH's Editor/URP - please confirm.

## v0.0.88 (t982u) - hero "gets bigger": all skills traced (2026-11-06, UTC+7)
- **New probe (`HellordSize988.cs`, autotest `-atOnly size988`):** renders the game camera twice in the same frame (hero shown / hero hidden) and measures the pixels the hero really draws. Run on the box under **Built-in** AND under a **URP 2D Renderer** copy of the project.
- **Result:** all 12 skills, 4 first→second pairs (incl. Archers→Spears and Spears→Archers) and a 5-skill set, normal and during i-frames, plus the old v0.0.86 shared-material shader path: the hero body is always 28×24/25 RT px. Camera ortho 11.25 and RT 640×360 never change. Only the trait overlays add pixels (Spears +8 px tall, Frost blade +12 px wide); those are the same with or without i-frames. Crops are in `Builds/at88_urp/size988/`.
- **The only code path that draws the hero bigger** is the t636u test **mouse-wheel view zoom** (`camScrollZoom636`). Wheel up = smaller view stretched to the window, so hero and world get bigger: measured 50 → 62–64 px on screen (×1.29 at 500×280).
  - It was live during the Manual **"PICK A SLOT FOR …"** prompt, because the game stays in Play there. That prompt only appears from the **2nd skill** on, which matches your Archer→Spear report.
- **Fix:**
  - The wheel no longer zooms while a slot pick, the Controls page, or the test menu is open, or while the cursor is over the skill belt.
  - **Middle-click** resets the view to auto.
  - Every wheel zoom now shows a toast (`VIEW ZOOM 600x340 (MOUSE WHEEL) - MIDDLE-CLICK = RESET`).
  - **Diagnostic:** every change in the hero's on-screen scale writes a Console line `[Hellord] t982u hero screen scale A -> B ...` with the wheel step, RT, window and ortho. If you see the hero grow again, send that line. No line means the game itself did not rescale anything (e.g. the Editor Game-view Scale slider).
- I-frame look C is unchanged. The v0.0.87 per-layer materials are kept.
- Autotests: size988 PASS on Built-in and URP 2D; iframes969 A/B/C and size987 PASS on Built-in. **Not tested in your Editor.** Plain-play wheel zoom still works (test feature); set `camScrollZoom636` off in the Tuning asset to remove it completely.

## v0.0.89 (t986u / t988u) — clear leftover FX (except Frost Ox) + FPS + Fireball fix (2026-11-06, UTC+7)
VH (t984–t986): "except frost ox, apply others" / "why still left? just make sure nothing are left". t988u: FPS drops + Fireball hit looks weird.

### Checklist
| Item | Status |
|---|---|
| Frost Cleave / Frost Blade shader (sample_v7 lock) | **DONE** — `frost_cleave_wire` → `Hellord/PixelFrostCleave` + `HellordFrostCleave990`. Flag `frostCleaveShader990` ON. Wind-up `frostCleaveWindUp990` OFF (damage is instant). |
| Qilin Flame Breath bigger/longer | **DONE** — reach 10.5u, sweep 120°, half 20°, duration 2.1s. PixelBossGround mode 3 flame during active phase (`bossBreathGround986`). |
| Gorilla Shard Fan at feet | **DONE** — tell + shots from `_boss.pos` (`gFanAtFeet986`). Gorilla KCone now uses PixelBossWarn (no longer skipped). |
| Soul Spears pointed head | **DONE** — `fxSpearV2_934` already on; tip sharpened HL 13 / HW 2.7 / WideAt 7. |
| Fire Wall icon | **NO ASSET** — still Hellfire Pillar cell on the step51k sheet. No approved Fire Wall-specific icon in Art/previews. |
| Boss/ground leftovers | **PARTIAL** — breath Mode 3 wired; orb/warn/sack already live. Other PixelBossGround modes (heat/lava/frost/wind/core/flash/spike/dot) and PARTICLES.md debris still unused (no live call sites demanding them this turn). EB49/P46 projectiles already on PixelBossOrb when `bossShaderTells950`. |
| Frost Ox | **SKIPPED** (only skip). |
| Preview vs game quality | See below. |
| FPS drops (t988u) | **DONE** — PixelShock concurrent cap `fxShockMax988=3` (GrabPass is a full-screen copy while any shock is live); Fire Wall embers sparser (`fireWallEmberPxPer988=8`, max 48). |
| Fireball hit weird/broken (t988u) | **DONE** — centre-relative noise UVs; force `_RampCount=8`; skip pack Glow+soft Ring over the shader blast (`fxFireSkipPackFlash988`). |

### Quality: why FX look worse than the 4× MOCKUP previews
1. **Previews are 4× nearest-neighbour zooms** of a 640×360 canvas; in-game you see 1× (or CRT-scaled). Same pixels look "softer / thinner" at 1×.
2. **Un-wired skills still showed old FrostArcFX / ReactionFX** thin rims while previews showed the thick sample_v7 smear — Frost Cleave was the biggest gap; now wired.
3. **Linear colour space**: vertex-colour meshes (HeroSkillFX) need `Color.linear` (already fixed for Blood Whip). Shader ramps are sampled as textures (sRGB on) so they match Endesga when imported correctly.
4. **Cubemap import** historically made PixelAoE white/empty (fixed by HellordArtImporter → Texture2D). Fireball white wash was the pack Glow on top of the shader + RampCount defaulting to 6.
5. **Irreducible**: mock hit-stop holds, per-enemy frost crust on sprite outlines, and some debris are CPU/particle approximations (PARTICLES.md), not 1:1 shader.

### Autotest (box Built-in player, 2026-11-06 Asia/Saigon)
- `frost990`: PASS — okLoad True, flag True, cleaves 3 (total 4). Clips: `Builds/t986u_review/frost990_{00..09}.png`, `frost990.mp4`, `frost990.gif`.
- `fireball`: PASS — visual_pixels casts 4, skill-colour +412 vs baseline.
- `firewall986`: PASS — far_target + out_of_range (ember NRE fixed: Embers948 arrays restored).
- `boss974`: PASS — ox / gorilla / qilin breath / horseface (shader tells, no old StompRings/PatternWarnFX).

### Package
- `Builds/Hellord_Prototype_v0.0.89.unitypackage` (MD5 `0caec4b6c7cc9f6ee55cbf6a337befb9`, 29124982 bytes) + linux `Builds/linux_v0.0.89/`.
- Delivered to VH PC `C:\Users\Admin\` (package + .md5 + checklist + frost990 mp4 + Decisions).

### Untested
Not play-tested in VH's Editor/URP. Autotests run on the box Built-in player. Frost Cleave wind-up left off on purpose.

## v0.0.91 (t1001u / t1002u) — Fireball hit + URP GrabPass gate (2026-11-06, Asia/Saigon)

### Cause (Fireball purple disc)
The hit looked like a purple dithered bubble because (1) PixelAoE mode-2 painted a radial filled disc (old ramp had ink/greyblue soot; even warm remakes still read as a soft bubble under CRT), and (2) pack soft Glow / HellfireNova LightPop added another translucent Glow disc on top. Neither path uses GrabPass; the disc was soft additive Glow + radial AoE, not fire puffs.

### Fix
- Fireball splash default: **HellfireNova** flame ring + white core + sparks/embers, sized to splash radius. **No** PixelAoE radial disc (`fxShaderFireball928=false`). **No** soft Glow/Ring wash (`fxFireSkipPackFlash988`). **No** LightPop Glow on that nova.
- Warm Endesga fire ramp kept for optional shader path; mode-2 also discards dark fill if re-enabled later.
- **t1002u URP:** `Shock934` (PixelShock GrabPass) is **skipped when `currentRenderPipeline != null`**. GrabPass shocks are Broken-in-URP until a URP-safe rewrite. Fireball hit path never used GrabPass.
- FPS stamp/caps from v0.0.90 kept.

### Untested
Not play-tested in VH's URP Editor. Box Linux autotest `fireball1001` PASS (nova path, warm fire). Clip: `Builds/t1001u_review/clip_fireball_hit.mp4` (may be from slightly earlier nova build).

## t1022u — Real smaller Ox Pit boss room (Nov 6)
VH: boss room still not smaller — only weird slower camera. Confirmed: t1006 was movement/camera clamp inside still-huge 1536×864 baked `v26_boss`.
**Fix (no package yet):** `ShrinkBoss677` now also scales Ox Pit (~0.62 → ~960×544); baked base SpriteRenderer scales to match; arena/camera use the scaled walkable room (no nested clamp). Soft runtime scale until Hellord Art remakes native ~960×540 Ox Pit.

## t1026u — boss lights need visible sources (2026-11-06 Asia/Saigon)

**VH:** "you added light but without light source, it look weird"

**What was wrong:** `PlaceBossArenaLights1006` spawned only `SourceGlow46` additive discs (16 rim + 5 fill). No torch/brazier prop under them, so light floated in empty air.

**Choice A (shipped):** floor `oxpit_prop_brazier` (existing 4-frame strip, same as room braziers) at every rim+fill glow, flame centre aligned to the glow. Rim inset 0.55 wu so props sit on floor. Animated @ 8fps. 21 props = 21 lights.

**Choice B (alt if VH wants):** `oxpit_prop_skullpost` on rim walls + braziers on fill only.

**Files:** `HellordBossRoom1006.cs`, assert in `HellordAutoTest.cs` (`bossroom1006:light_sources`).

**Preview:** `Docs/previews/t1026_boss_lights_with_braziers.mp4` (+ stills `t1026_boss_lights_0{0,2,5}.png`). Compile PASS. No package shipped.

## t1027u — floating skullposts / light props (2026-11-06 Asia/Saigon)

**VH:** circled gap under ox skullpost feet (empty air above dirt; shadow detached). "these look like flying still not fix"

**Root cause (baked skullposts, not only our t1026 braziers):**
After t1022 ShrinkBoss677, `v26_boss_base` SR is `localScale`d (~0.625×0.630) to 960×544, but runtime props (`AddProp` skullposts, fences, …) and room braziers stayed **1:1**. Oversized posts sat on tiny scaled contact shadows → looked floating. Secondary: skullpost wood ends at sprite y37 while pivot oy=40 (baked shadow), so wood sat ~3px above the contact point.

**Fix:**
1. `HellordRoom.AddProp`: scale props by `BossSX677/BossSY677`; skullpost `placeAy += round(4*psy)` so wood meets floor; flame glow offsets scale with sprite.
2. Room braziers: same `localScale` (top-left already ShrinkBoss-scaled → feet map correctly when sprite scales).
3. Arena lights (`PlaceBossBrazierGrounded1027`): feet on floor point; glow at flame tip only (do not lift prop to glow). Scale with BossS*.

**Before → after (north skullpost example):**
- Shrink: ay 94 → 59 (sy=0.630). Scale was (1,1) → now (0.625, 0.630). placeAy 59 → 62 (+3 seat).
- Visual height 42px → ~26.5px (matches floor). Measured post feet gap in zoomed shot: 0–1 screen px (was multi-px float).

**Previews:** `Docs/previews/t1027_boss_props_grounded.mp4`, `t1027_skull_close.png` (320×180 zoom), `t1027_brazier_feet.png`, `t1027_full_05.png`, `t1027_meas_post_*.png`. Autotest PASS (`skull_scaled` 0.630). No package.

## t1028u — floating was the SHADOW (2026-11-06 Asia/Saigon)

**VH:** "the problem is it's shadow bro… still look like flying"

**Root cause:** Baked contact-shadow lips in the prop PNGs (not a runtime drop-shadow GO):
- `oxpit_prop_skullpost.png` rows **y38–41**: dark semi-transparent oval (w13–15) under 4px wood; JSON pivot **oy=40** sat in the lip → oval read as hovering blob.
- `oxpit_prop_brazier.png` rows **y34–37**: same lip under the bowl.

**Fix (no new Hellord Art):**
1. Cleared shadow rows to alpha 0 in both Resources PNGs (backups `*.bak_t1028`).
2. Skullpost pivot **oy 40 → 37** (wood bottom) at authored `ay` — feet = solid post, no oval.
3. Room/arena braziers: seat solid bottom (y33); +4*psy top-left nudge / bh=34 so bowl sits where the old lip was.

**Previews:** `Docs/previews/t1028_no_float_shadow.mp4`, `t1028_sprite_before_after.png`, `t1028_feet_planted_4x.png`, `t1028_skull_close.png`. Autotest PASS. No package.

## t1029u — detached oval was FLOOR-BAKED contact shadow (2026-11-06 Asia/Saigon)

**VH:** "?" + screenshot still circling dark oval on dirt under/near skullpost (PC still on v0.0.95).

**Exact source:** `tools/bake_rooms_v26.py` paints a soft two-band ellipse (`#181425`) into **`v26_boss_base.png` under every prop + brazier** ("so nothing floats"). After props scale separately / prop PNG lip cleared (t1028), that floor oval sits half-below the feet → classic detached blob. Not a runtime DropShadow GO. Prop PNG lip remains cleared (t1028).

**Fix:** Erased all bake contact ellipses from `v26_boss_base.png` (76 props + 5 braziers) via ring-median fill. Backup `v26_boss_base.png.bak_t1029`.

**Previews:** `Docs/previews/t1029_base_shadow_before_after.png`, `t1029_no_floor_shadow.mp4`, `t1029_skull_close.png`, `t1029_feet_no_oval_4x.png`. Autotest PASS. No package.

**Note for VH:** PC package is still v0.0.95 — this floor erase is box-only until a new package ships.

## t1030u — flying rock (not skullpost)

**VH:** circled a small grey/brown rock cluster with a detached dark oval under it ("the rock under it look like flying af").

**Cause:** `oxpit_prop_rubble_0` / `oxpit_prop_rubble_2` (and same-bake debris) ship with a baked soft shadow lip in the PNG; pivot sat in the lip so the solid rock hovered above its own oval. Floor contact ellipses under rubble were already erased in t1029 — verified healed (center≈ring).

**Fix:**
1. Cleared shadow lips in PNGs (backups `*.bak_t1030`):
   - `oxpit_prop_rubble_0` y≥7 (solid bottom y6; was oy=10)
   - `oxpit_prop_rubble_2` y≥9 (solid bottom y8; was oy=12)
   - `oxpit_prop_bonepile` y≥10 (oy 12→9)
   - `oxpit_prop_fallen_pillar` y≥17 (oy 19→16)
   - `oxpit_prop_hay` y≥15 (oy 16→14)
2. `HellordRoom.AddProp`: override oy to solid bottoms + sprite key `ns1030`; already scales with `BossSX677`/`BossSY677`.
3. Floor ovals: no further erase needed (t1029 covered all props including rubble/bonepile).

**VH circled:** rock cluster = `oxpit_prop_rubble_2` (6 placements) / also `rubble_0` (2). Same float pattern on bonepile/hay/fallen_pillar — fixed same pass.

**Previews:** `Docs/previews/t1030_rock_*`, sprite before/after boards, auto-test `bossroom1006_rock_close` / `_rock_feet_crop`.
