> **SUPERSEDED by Hellord_sins_design_v0.2.md (2026-11-03).** Kept for history; rules below are void.

# Hellord — Sins System v0.1 (DRAFT for VH review — design only, no code, no build)
Status: **DRAFT**. Nothing here is implemented. All numbers are proposals and will become tunables in `HellordTuning.cs` (suffix `_sin`) once VH approves.

## 0. What Sins are
- PoE2-unique-style relics that **last for one run only** (roguelike; no gear, no inventory carry-over).
- Dropped mainly by **bosses** (chance-based), worn in a limited number of **Sin slots** chosen at the **Sin Altar** menu.
- 5 tiers:

| Tier | Colour | Content | Name |
|---|---|---|---|
| Common | grey | 1 stat | generated |
| Cursed | blue | 2 stats | generated |
| Profane | gold | rule-changing effect + trade-off (+1 stat) | hand-named (20) |
| Damned | red | stronger rule change + harsher trade-off | hand-named (14) |
| God-Disliked | black-violet | build-defining rule break + brutal trade-off + "what the gods said" | hand-named (6) |

- Rules reference the **real current content** (HellordData.cs): skills Fireball, Frost Cleave, Storm Ring, Bone Orbit, Curse Rune, Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool, Skeleton Warriors, Skeleton Archers; supports Multi Projectiles, Pierce, Fork, Bounce, Homing, Bigger Area, Echo, More Orbitals, Burn, Chill, Shock, Bleed, Added Damage, Faster Casting, Executioner, Life Leech, Knockback, Soul Harvest, Explode on Kill; tags Projectile, Area, Orbital, Hit, Melee, Aura, Zone, Minion, Chain, Duration, Curse, Physical, Fire, Cold, Lightning, Blood, Bone, Soul; statuses Burn/Chill(3=freeze)/Shock/Bleed/Curse; Dash (Space); XP gems, bones, stats Vitality/Swiftness/Hunger/Hide/Malice.
- **[NEW]** = needs a mechanic that does not exist today. Unmarked = buildable from existing systems (modifiers on existing skill/support/status code).
- ⚠ Overlap: `Hellord_ascendancy_relics_sources_ui_v0.4.md` and the Damnation Pact docs already describe "Relics"/"Pacts". **Proposal: Sins replace the Relics layer of v0.4** (Pacts stay separate). VH to confirm.

## 1. Common / Cursed — generated names + stat pool
Name = `[Sin word] of [Theme]` or `[Theme]'s [Sin word]`. Sin word picks from the stat's family; theme from a 24-word list.
- Sin words by stat family: Gluttony (HP/pickup), Sloth (armor/regen), Wrath (damage), Envy (crit), Greed (XP/pickup), Pride (area/size), Lust (speed/cast speed).
- Themes: the Pyre, the Ossuary, the Gallows, Ash, the Ninth Circle, Rot, the Choir, the Drowned, Cinders, the Butcher, the Martyr, Salt, the Hook, Embers, the Tongue, the Pit, the Crypt, Brimstone, the Flayed, Marrow, the Leper, the Censer, Iron, the Hollow.
- Example: *Gluttony of the Pyre* (+8% max HP), *Wrath's Ossuary* (+6% damage, +4% Bone damage).

**Stat pool** (Common rolls 1 at low range; Cursed rolls 2 at high range; ranges tunable):
| Stat | Common | Cursed |
|---|---|---|
| Max HP | +5–8% | +9–14% |
| Move speed | +3–5% | +6–9% |
| Pickup range | +8–15% | +16–25% |
| Armor | +2–3 | +4–6 |
| Crit chance | +2–3% | +4–6% |
| Damage (all) | +4–6% | +7–10% |
| Damage of one tag (Fire/Cold/Lightning/Physical/Bone/Soul/Blood/Minion/Curse) | +8–12% | +13–20% |
| Cooldown (all) | −3–5% | −6–9% |
| Area | +6–10% | +11–16% |
| Status effect (burn/bleed dmg, chill/shock duration) | +10–15% | +16–25% |
| Dash cooldown | −5–8% | −9–12% |
| XP gain | +5–8% | +9–14% |
| Minion HP | +10–15% | +16–25% |

## 2. The 40 hand-named Sins
Format: **Name** — Effect. *Trade-off.* → Build it enables. [NEW] flags.

### Profane (gold) — 20
1. **The Ninth Betrayal** — Soul Spears return to you after max range, hitting again on the way back. *You take +15% damage.* → Pierce/Multi Projectiles spear lane builds. [NEW: return path]
2. **Mother's Last Curse** — Curses explode when they expire (not only on death), 60% of Curse Rune hit. *−20% max HP.* → Curse Rune + Bigger Area + Echo carpet.
3. **Gluttonous Saint's Jaw** — Each kill heals 1 HP. *Pickup range −40%.* → Frost Cleave / Blood Whip melee in the crowd.
4. **The Ash Widow's Veil** — Burning enemies spread Burn to one nearby enemy on death. *Your Cold damage −50%.* → Fireball/Hellfire Pillar + Burn support pure-fire.
5. **Choirboy's Severed Tongue** — Chain Lightning +3 jumps; each jump +8% dmg instead of falling off. *Chain Lightning cooldown +40%.* → Lightning chain + Shock amplify.
6. **Leper King's Crown** — Bleeding enemies take +30% from Physical. *You cannot crit.* → Bone Orbit + Blood Whip + Bleed.
7. **Thrice-Drowned Lantern** — Grave Pool puddles chill (3 chills = freeze). *Grave Pool damage −40%.* → Grave Pool as control + Frost Cleave shatter.
8. **Salt in the Wound** — Executioner threshold +10% HP on Bleeding/Burning targets. *−10% damage to enemies above 50% HP.* → Finisher builds with any DoT.
9. **The Hook-Mother's Knuckle** — Knockback pulls enemies toward you instead. *−20% move speed.* → Storm Ring / Frost Cleave vacuum. [NEW: pull vector]
10. **Brimstone Rosary** — Each Fire skill cast reduces other Fire skills' cooldown by 0.2s. *Non-Fire skills +25% cooldown.* → Mono-fire rotation.
11. **The Butcher's Apology** — Blood Whip heal per hit doubled. *Blood Whip can't crit and you lose 1 HP/s while above 80% HP.* → Self-sustain whip.
12. **Marrow Psalter** — Soul Harvest bone drops +100%; bones grant +1% damage for 10s (stacks 20). *XP gain −15%.* → Bone-stacking necro. [NEW: bone buff stacks]
13. **The Censer of Rot** — Storm Ring also applies Bleed. *Storm Ring radius −20%.* → Hybrid Shock+Bleed aura.
14. **Gallows Bell** — Every 10th kill casts Curse Rune at the kill spot for free. *Curse Rune is removed from your belt; it only fires via the Bell.* → Trigger-curse with high clear speed. [NEW: on-kill trigger]
15. **Hollow Martyr's Ribs** — Bone Orbit +2 skulls. *Skulls orbit 30% faster but radius −25% (must hug enemies).* → Close-range orbit tank.
16. **Ember-Eaten Psalm** — Fork children inherit Burn and explode for small fire area. *Fork max children −1.* → Fireball + Fork shotgun.
17. **The Flayed Saint's Mirror** — Echo repeats also Echo once more at 30% dmg. *Echo skill cooldown +30%.* → Curse Rune/Hellfire Pillar repeat carpets.
18. **Crypt-Warden's Oath** — Skeleton Warriors taunt bullets in a radius (block more). *You move 10% slower while any Warrior is alive.* → Minion wall.
19. **The Pyre-Child's Toy** — Dash leaves a Hellfire Pillar at the start point. *Dash cooldown +30%.* → Dash-kite fire. [NEW: on-dash trigger]
20. **Tithe of the Lesser Hand** — +1 support gem cap on your first skill slot. *Your other skills −10% damage.* → One-skill hyper builds. [NEW: socket cap change]

### Damned (red) — 14
21. **Hymn for the Unbaptised** — Skeleton Archers shoot your Fireball (copies supports). *Archers' own arrows are removed; you cannot cast Fireball.* → Summoner-caster. [NEW: minion casts player skill]
22. **The Seventh Seal, Broken** — All Projectile skills gain Bounce Lv3 for free. *Projectiles −35% damage.* → Pinball clear.
23. **Saint Agony's Nails** — Your Bleeds never expire while you stand still. *Move speed −15%.* → Stationary bleed turret (Bone Orbit/Whip).
24. **Wedding Ring of the Pit-Bride** — Shocked enemies chain 30% of hits taken to one nearby enemy. *You are Shocked for 1s whenever hit.* → Shock spread with Storm Ring + Chain Lightning. [NEW: shock on player]
25. **The Glutton's Second Stomach** — Every XP gem picked has 5% chance to instantly level a random owned skill. *Level-up cards show 1 fewer option.* → Gamble skill depth.
26. **The Unwept Child** — Cursed enemies explode twice. *You take 5 damage whenever a cursed enemy explodes within 2u of you.* → Curse + Explode on Kill chain clears.
27. **Frostbitten Hand of Judas** — Frozen enemies shatter on death for 50% max-HP cold area. *Fire damage −60%.* → Frost Cleave + Chill freeze-shatter. 
28. **Lantern of the Last Shepherd** — Minions +50% damage and rise instantly after dying. *Your own skills −30% damage.* → Pure summoner.
29. **The Scaffold's Kiss** — Executioner kills refund 50% of that skill's cooldown. *Executioner threshold can't be raised by any source except this.* → Execute chaining.
30. **Ninefold Lie** — Multi Projectiles and Fork apply to Chain Lightning (extra bolts). *All projectiles −20% speed and range.* → Lightning storm. [NEW: support tag bypass]
31. **Heart of the Burned Abbess** — Your burns stack (up to 5). *Burn duration −40%.* → DoT fire stacking. [NEW: burn stacks]
32. **The Debt-Collector's Ledger** — Each boss killed this run gives +10% damage permanently (run). *Boss HP +25%.* → Snowball runs.
33. **Ossuary Throne** — Bone Orbit skulls become Soul Spears that fire outward when they hit (then respawn). *Bone Orbit cooldown ×2.* → Orbit-launcher. [NEW]
34. **Whisper of the Hanged** — Life Leech heals over cap, overflow becomes a shield (max 30% HP). *No natural healing from any other source.* → Leech tank. [NEW: shield]

### God-Disliked (black-violet) — 6
35. **What Even Heaven Refused** — Your minions cast ALL your skills (each skeleton picks one). *You can never cast skills yourself; you need ≥1 Minion skill to use it.* → Ultimate necromancer. [NEW]
   - *Gods said:* "We cast it down once. You picked it up."
36. **The Eighth Sorrow of God** — Every 3rd cast of any skill is cast 3 more times (Echo-like, all skills incl. projectiles). *Max HP halved.* → Glass-cannon everything. [NEW: global echo]
   - *Gods said:* "It wept, and we let it."
37. **Tongue that Named the Unnameable** — All your damage counts as every element: applies Burn, Chill, Shock and Bleed at once. *−40% damage; enemies +20% speed.* → Every status support stacks multiplicatively. [NEW: multi-element]
   - *Gods said:* "Some words should stay unsaid."
38. **The Crown No Angel Wore** — Your Auras (Storm Ring) and Orbitals (Bone Orbit) grow +5% area per second without taking damage. *Getting hit resets it and stuns you 0.5s.* → Untouchable dodge build. [NEW: growth stack, player stun]
   - *Gods said:* "It was made for a head that never bowed."
39. **Lucifer's Unpaid Tithe** — Each Sin slot left EMPTY gives +35% damage and +1 projectile. *You cannot equip Common/Cursed Sins.* → Minimalist one-Sin runs.
   - *Gods said:* "He still owes us. Now you do."
40. **The Last Prayer, Reversed** — On death, revive once per floor at full HP and kill everything on screen. *Your max HP drops 1% per 10s alive (resets on revive).* → Death-cycling berserker. [NEW: revive]
   - *Gods said:* "We answered. You will wish we had not."


## 3. Drop tables (per boss kill, tunable)
- Chance a boss drops a Sin: floor 1 60%, floor 2 75%, floor 3+ 90% (`sinBossDropChance`).
- Elite drop: 8% (Common/Cursed only) (`sinEliteDropChance`).
- Tier weights given a drop (boss): Common 45 / Cursed 30 / Profane 18 / Damned 6.5 / God-Disliked 0.5 (`sinTierWeights`). God-Disliked only from bosses, +0.25 per floor (`sinGodPerFloor`).
- Duplicate rule: no duplicate named Sins in one run.
- Pity: after 3 bosses without Profane+, next drop guarantees Profane (`sinPityBosses`).

## 4. Slot rules
- Start with 3 Sin slots (`sinSlotsStart`=3); each boss killed +1, max 6 (`sinSlotsMax`).
- Max 1 God-Disliked worn (`sinGodMax`=1); max 2 Damned (`sinDamnedMax`=2).
- Picked-up Sins go to the **Sin Satchel** (unworn, max 8, `sinSatchelMax`). Swap any time at the Sin Altar (no cost), or proposal: swapping out a Damned+/God costs 10% HP ("you can't just un-sin").

## 5. Sin Altar menu (text wireframe)
Opens: automatically after picking up a Sin (pause), or from pause menu (Tab) → "Sins".
```
+------------------------------ SIN ALTAR -------------------------------+
|  WORN (3/4)                                                             |
|  [◆ Gold: The Ninth Betrayal] [◆ Red: Unwept Child] [◇ Grey: Sin..] [ ] |
|                                                                        |
|  SATCHEL (5/8)                                                          |
|  [Cursed] [Common] [Profane] [God-Disliked✦] [Common]                   |
|                                                                        |
|  ── Selected: THE NINTH BETRAYAL (Profane) ─────────────────────────── |
|  + Soul Spears return after max range, hitting again.                  |
|  − You take +15% damage.                                              |
|  Works with: Soul Spears ✓ owned | Pierce ✓ | Multi Projectiles —       |
|                                                                        |
|  [E] Wear / Swap   [X] Shatter (+XP)   [Esc] Back to the Pit           |
+------------------------------------------------------------------------+
```
- "Works with" shows owned skills/supports matching the Sin (helps build-around).
- Shatter: destroy a satchel Sin for XP (`sinShatterXp` by tier).
- Tier frame colours from the tier table; art via Hellord Art mockup first (needs VH approval).

## 6. Codex (PROPOSAL)
- First pickup of each named Sin records it in the Sin Codex (meta, persistent): name, effect, first floor found.
- Undiscovered named Sins show as "???". Optional: discovered Sins get +weight in future runs (`sinCodexWeightBonus`). Proposal only — VH to decide if any meta carries over.

## 7. Example builds
1. **Soul Pinball** — Soul Spears + Pierce + Multi Projectiles; Sins: The Ninth Betrayal + The Seventh Seal, Broken. Spears bounce, pierce and return — screen fills with lines. Weakness: +15% damage taken, −35% projectile damage → needs Malice/Cursed damage stats.
2. **Funeral Carpet** — Curse Rune + Bigger Area + Echo, Grave Pool; Sins: Mother's Last Curse + The Unwept Child + The Flayed Saint's Mirror. Curses explode on expire and twice on death, Echo repeats. Weakness: low HP, self-damage when close → kite with Dash.
3. **Unholy Choir (God-Disliked)** — Skeleton Archers + Skeleton Warriors + Fireball + Fork; Sins: What Even Heaven Refused + Lantern of the Last Shepherd + Crypt-Warden's Oath. Minions cast Fireball with Fork; you only move and dodge.
4. **Shatter Saint** — Frost Cleave + Chill + Grave Pool + Executioner; Sins: Frostbitten Hand of Judas + Thrice-Drowned Lantern + The Scaffold's Kiss. Puddles freeze, frozen kills shatter, executes refund cooldown. Weakness: no fire damage at all.

## 8. Tunables to add to HellordTuning.cs later (proposed names, task tag _sin)
- `sinEnabled`=true (master switch)
- `sinBossDropChance`={0.6,0.75,0.9}; `sinEliteDropChance`=0.08
- `sinTierWeights`={45,30,18,6.5,0.5}; `sinGodPerFloor`=0.25; `sinPityBosses`=3
- `sinSlotsStart`=3; `sinSlotsMax`=6; `sinSlotsPerBoss`=1; `sinGodMax`=1; `sinDamnedMax`=2; `sinSatchelMax`=8
- `sinSwapHpCost`=0.1 (Damned+ only, proposal); `sinShatterXp`={5,10,25,50,100}
- `sinCommonStatRanges[]`, `sinCursedStatRanges[]` (table §1); `sinCursedStatCount`=2; `sinProfaneBonusStat`=1
- One value block per named Sin, e.g. `sin01ReturnDmg`=1.0, `sin01DmgTaken`=0.15; `sin02ExplodeFrac`=0.6, `sin02HpLoss`=0.2; `sin05Jumps`=3, `sin05JumpDmg`=0.08, `sin05CdMul`=1.4; `sin22BounceLv`=3, `sin22DmgMul`=0.65; `sin36EveryN`=3, `sin36Repeats`=3, `sin36HpMul`=0.5; `sin38GrowPerSec`=0.05, `sin38Stun`=0.5; `sin39PerEmpty`=0.35; `sin40HpDecayPer10s`=0.01 … (every number in §2 gets a field)
- `sinCodexEnabled`=false (proposal); `sinCodexWeightBonus`=0.2

## 9. Open questions for VH
See Decisions_for_VH.md (t726u Sins draft).


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
