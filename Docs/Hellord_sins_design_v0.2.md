> **SUPERSEDED by Hellord_sins_design_v0.3.md (2026-11-03).**

# Hellord — Sins System v0.2 (DRAFT for VH review — design only, no code, no build)
Supersedes v0.1. Applies VH decisions t727u, t729u, t731u, t732u (see §11). All numbers are proposals; each becomes a tunable in `HellordTuning.cs` (`sin*`, task tag 733) after VH approves.

## 0. Summary
- **Sins = META collection.** They are unlocked permanently across runs and never lost.
- **Equip before a run** at the Sin Altar (hub screen). Nothing changes mid-run: no satchel, no swaps, no mid-run altar.
- **Max 3 Sin slots**, each unlocked with **Souls + Sigils** (existing meta currency of the Damnation Pact).
- **Unlock source:** each boss kill has a small chance to unlock a sin. Roll the tier first, then a weighted roll of a sin within that tier, so some sins in a tier are much rarer than others.
- **Most sins are stats-only.** Profane/Damned add a MILD special effect plus a small trade-off. Only God-Disliked change builds.
- **Pacts stay.** Relics are cut (t729u); the Relics sections of `Hellord_ascendancy_relics_sources_ui_v0.4.md` and the Relic Codex in `Hellord_meta_ascendancy_menu_v0.5.md` are obsolete.

## 1. Collection size (finite)
| Tier | Colour | Count | Content |
|---|---|---|---|
| Common | grey | 36 | 1 stat |
| Cursed | blue | 24 | 2 stats (higher) |
| Profane | gold | 20 | 1 stat + mild effect + small trade-off |
| Damned | red | 14 | 1 stat + mild-to-moderate effect + trade-off |
| God-Disliked | black-violet | 6 | build-changing effect + heavy trade-off |
| **Total** | | **100** | 60 stat sins (60%) + 40 named specials |

## 2. Meta currency (existing, from code)
Already in `HellordTuning.cs` (v0.0.51 Damnation Pact):
- Souls: `soulsPerKill51`=0.1, `soulsPerBone51`=1, `soulsPerFloor51`=25. Rough estimate ≈ 150–250 Souls per run (NOT measured).
- Sigils: `sigilsPerWin51`=1, `sigilsPerBoss51`=0, so Sigils are only earned on a win.
- Pact prices for reference: Notables 30–80 Souls; Keystones 150 Souls + `pactKeystoneSigils51`=1 Sigil.
- Sins spend the SAME Souls/Sigils wallet as Pacts (proposal), so players choose between Pact nodes and Sin slots.

## 3. Sin slots (max 3)
| Slot | Cost (proposal) | Why |
|---|---|---|
| 1 | 100 Souls, 0 Sigils | Reachable after the first 1–2 runs, so the system shows up early |
| 2 | 400 Souls + 1 Sigil | Needs a first win |
| 3 | 1000 Souls + 3 Sigils | Long-term goal, priced above a Pact Keystone |
- Max 1 God-Disliked equipped (`sinGodMax`=1). Max 2 Damned (`sinDamnedMax`=2).
- Equip/unequip is free at the altar between runs. Locked for the whole run once started.

## 4. Unlock (drop) rates — two-step roll
**Step 1, chance a boss unlocks a sin:**
- Floor bosses: 12% (`sinBossUnlockChance`=0.12).
- Final boss / run win: 25% (`sinFinalBossUnlockChance`=0.25).
- Bosses only. Elites and chests never unlock sins.

**Step 2, tier roll (weights, `sinTierWeights`):**
| Tier | Weight | Effective per floor boss | Effective per final boss |
|---|---|---|---|
| Common | 62 | 7.4% | 15.5% |
| Cursed | 28 | 3.4% | 7.0% |
| Profane | 7 | 0.84% | 1.75% |
| Damned | 2.6 | 0.31% | 0.65% |
| God-Disliked | 0.4 | 0.048% (~1 in 2,100) | 0.10% (~1 in 1,000) |

God-Disliked is meant to be super rare. A dedicated player might see one per 300–600 runs. VH to judge whether that's too rare (see open questions).

**Step 3, sin roll within the tier.** Each sin has its own weight (`sinWeight[]`). Rarity bands: **Frequent 100 / Uncommon 50 / Rare 20 / Very Rare 5**. Inside a tier, a Very Rare sin is 20× rarer than a Frequent one. Weights for the 40 specials are in §6. Stat sins use the pattern in §5.

**Duplicates:** proposal = an already-unlocked result is **rerolled once** within the same tier (`sinDupRerolls`=1). If it is still a duplicate, it **converts to Souls**: Common 15, Cursed 30, Profane 80, Damned 150, God-Disliked 400 plus 1 Sigil (`sinDupSouls[]`). Rare sins stay rare, and a dud roll still pays something. Bad-luck pity (`sinPityBosses`=8): after 8 bosses with no unlock, the next boss unlock chance is doubled.

## 5. Stat sins (60, fixed list, named by pattern)
Name = `[Sin word] of [Theme]`. Each stat family has a fixed sin word, and each sin's theme is fixed, so the list is finite and collectible.
- Sin words: Gluttony = HP / pickup, Sloth = armor, Wrath = damage, Envy = crit, Greed = XP, Pride = area, Lust = speed / cooldown.
- **Common (36):** 12 stats × 3 themes, with weights 100/50/20 so the 3rd theme is the rarer high roll.
- **Cursed (24):** 12 pairs of 2 stats × 2 themes, with weights 100/20.

| # | Stat | Common value (3 themes) | Cursed (stat used in a pair) |
|---|---|---|---|
| 1 | Max HP | +4 / +6 / +8% (Gluttony of the Pyre / of Rot / of the Ninth Circle) | +10% |
| 2 | Move speed | +3 / +4 / +5% (Lust of Ash / of the Hook / of Embers) | +6% |
| 3 | Pickup range | +8 / +12 / +16% (Gluttony of the Tongue / the Pit / the Hollow) | +18% |
| 4 | Armor | +2 / +3 / +4 (Sloth of Iron / the Crypt / Salt) | +5 |
| 5 | Crit chance | +2 / +3 / +4% (Envy of the Gallows / Cinders / the Martyr) | +5% |
| 6 | Damage, all | +3 / +4 / +6% (Wrath of the Butcher / Brimstone / the Flayed) | +7% |
| 7 | Fire damage | +6 / +9 / +12% (Wrath of the Pyre-Bell / the Censer / the Brand) | +14% |
| 8 | Cold damage | +6 / +9 / +12% (Wrath of the Drowned / Hoarfrost / the Frozen Choir) | +14% |
| 9 | Lightning damage | +6 / +9 / +12% (Wrath of the Storm-Bell / the Spire / the Struck) | +14% |
| 10 | Physical/Bone/Blood damage | +6 / +9 / +12% (Wrath of Marrow / the Ossuary / the Leper) | +14% |
| 11 | Cooldown | −2 / −3 / −5% (Lust of the Choir / the Gallop / the Hour) | −6% |
| 12 | Minion damage + HP | +6 / +9 / +12% (Pride of the Grave / the Legion / the Barrow) | +14% |

- Cursed pairs, proposed (each pair has 2 themes): HP+Armor, HP+Speed, Damage+Crit, Fire+Area, Cold+Cooldown, Lightning+Crit, Physical+Bleed effect, Minion+HP, Pickup+XP, Speed+Dash cooldown, Area+Duration, Curse/Soul damage+Cooldown.
- Every value goes in `sinStatValues[]`. Every name is generated, with no hand writing needed.

## 6. The 40 named specials (milder rewrite)
Profane and Damned effects are now **mild** (roughly +5–15% power, local to one skill or support). Their trade-offs are small. God-Disliked stay build-changing.
[NEW] = needs a mechanic that does not exist yet. Weight = drop weight within its tier.

### Profane (gold) — 20 · each also gives +1 Common-range stat
| # | Name | Effect (mild) | Trade-off | Weight | NEW? |
|---|---|---|---|---|---|
| 1 | The Ninth Betrayal | Soul Spears +1 pierce | −5% Soul Spear speed | 100 | |
| 2 | Mother's Last Curse | Curse Rune explosions +15% area | Curse Rune cooldown +5% | 100 | |
| 3 | Gluttonous Saint's Jaw | 3% chance per kill to heal 1 HP | −10% pickup range | 100 | |
| 4 | The Ash Widow's Veil | Burn lasts +1s | −5% Cold damage | 100 | |
| 5 | Choirboy's Severed Tongue | Chain Lightning +1 jump | Chain Lightning −5% damage | 100 | |
| 6 | Leper King's Crown | +8% damage to Bleeding enemies | −2% crit chance | 50 | |
| 7 | Thrice-Drowned Lantern | Grave Pool slows +10% more | Grave Pool −8% damage | 50 | |
| 8 | Salt in the Wound | Executioner threshold +3% HP | −3% damage to enemies above 50% HP | 50 | |
| 9 | The Hook-Mother's Knuckle | Knockback +20% | −3% move speed | 100 | |
| 10 | Brimstone Rosary | Fire skills −5% cooldown | Non-Fire skills +3% cooldown | 50 | |
| 11 | The Butcher's Apology | Blood Whip heal per hit +25% | Blood Whip −5% damage | 50 | |
| 12 | Marrow Psalter | Soul Harvest bone chance +3% | −3% XP gain | 50 | |
| 13 | The Censer of Rot | Storm Ring hits have a 10% chance to Bleed | Storm Ring radius −5% | 20 | |
| 14 | Gallows Bell | Every 40th kill drops a free Curse Rune at that spot | Curse Rune cooldown +10% | 20 | [NEW: on-kill trigger] |
| 15 | Hollow Martyr's Ribs | Bone Orbit +1 skull at skill level 3+ | Orbit radius −8% | 20 | |
| 16 | Ember-Eaten Psalm | Fork children +10% damage | Fork shots −5% speed | 50 | |
| 17 | The Flayed Saint's Mirror | Echo repeats +5% damage | Echo skill cooldown +5% | 50 | |
| 18 | Crypt-Warden's Oath | Skeleton Warriors +15% HP | −3% move speed | 100 | |
| 19 | The Pyre-Child's Toy | Dash leaves a small burn patch (2s) | Dash cooldown +8% | 5 | [NEW: on-dash patch] |
| 20 | Tithe of the Lesser Hand | First skill slot +6% damage | Other skills −2% damage | 20 | |

### Damned (red) — 14 · each also gives +1 Cursed-range stat
| # | Name | Effect (mild–moderate) | Trade-off | Weight | NEW? |
|---|---|---|---|---|---|
| 21 | Hymn for the Unbaptised | Skeleton Archers' arrows apply Burn | Archers −10% attack speed | 50 | |
| 22 | The Seventh Seal, Broken | Projectiles gain +1 Bounce | Projectiles −8% damage | 100 | |
| 23 | Saint Agony's Nails | Bleed +20% damage while you stand still | −5% move speed | 100 | [NEW: standing-still check] |
| 24 | Wedding Ring of the Pit-Bride | Shock lasts +1s | Take +5% damage while shocked enemies are within 2u | 50 | |
| 25 | The Glutton's Second Stomach | +1 card option on every 5th level-up | −5% XP gain | 20 | |
| 26 | The Unwept Child | Cursed enemies' death explosion +20% damage | Explosion within 1.5u deals 2 damage to you | 50 | |
| 27 | Frostbitten Hand of Judas | Frozen enemies take +15% damage | Fire damage −10% | 100 | |
| 28 | Lantern of the Last Shepherd | Minions rise again 25% faster | Your own skills −5% damage | 50 | |
| 29 | The Scaffold's Kiss | Executioner kills refund 10% of the skill's cooldown | Executioner damage −5% | 20 | |
| 30 | Ninefold Lie | Chain Lightning first bolt splits into 2 at 50% damage | Chain Lightning cooldown +10% | 5 | [NEW: split bolt] |
| 31 | Heart of the Burned Abbess | Burn on a target can stack twice | Burn duration −20% | 20 | [NEW: burn stacks] |
| 32 | The Debt-Collector's Ledger | +3% damage per boss killed this run (max +9%) | Boss HP +8% | 20 | |
| 33 | Ossuary Throne | Every 6th Bone Orbit skull hit fires a Soul Spear outward | Bone Orbit −8% damage | 5 | [NEW] |
| 34 | Whisper of the Hanged | Life Leech cap per second +25% | Other healing −15% | 100 | |

### God-Disliked (black-violet) — 6 · build-changing, no stat
| # | Name | Effect | Trade-off | Weight | NEW? |
|---|---|---|---|---|---|
| 35 | What Even Heaven Refused | Your minions also cast your non-minion skills (each skeleton picks one, at 60% damage) | Your own casts −50% damage | 50 | [NEW: minion casts skill] |
| 36 | The Eighth Sorrow of God | Every 4th cast of any skill repeats 2 more times | Max HP −35% | 100 | [NEW: global echo] |
| 37 | Tongue that Named the Unnameable | All hits apply Burn, Chill, Shock and Bleed | −30% damage; enemies +10% speed | 20 | [NEW: multi-status] |
| 38 | The Crown No Angel Wore | Storm Ring and Bone Orbit +4% area per second without taking a hit (max +80%) | Getting hit resets it and stuns you 0.4s | 50 | [NEW: growth stack, stun] |
| 39 | Lucifer's Unpaid Tithe | Each EMPTY sin slot: +25% damage and +1 projectile | Can't equip Common/Cursed sins with it | 20 | |
| 40 | The Last Prayer, Reversed | Once per floor, revive at 50% HP and blast the screen | Max HP −1% per 10s alive (resets on revive) | 5 | [NEW: revive] |

What the gods said:
- #35: "We cast it down once. You picked it up."
- #36: "It wept, and we let it."
- #37: "Some words should stay unsaid."
- #38: "It was made for a head that never bowed."
- #39: "He still owes us. Now you do."
- #40: "We answered. You will wish we had not."

New-mechanic count: 10 sins are [NEW] (was 18 in v0.1). Most specials now reuse existing stats, supports and status code.

## 7. Sin Altar (pre-run hub screen)
Opened from the hub/main menu before "Descend" (next to the Pact screen).
```
+--------------------------- SIN ALTAR (before the run) ----------------------------+
| SLOTS:  [1 ◆ Gold: The Ninth Betrayal]  [2 ◇ Grey: Wrath of Brimstone]  [3 🔒]     |
|         Slot 3: 1000 Souls + 3 Sigils   [Unlock]        Wallet: 640 Souls · 2 ✦   |
|-----------------------------------------------------------------------------------|
| COLLECTION 23/100     Filter: [All][Stat][Profane][Damned][God]  Sort: [Tier]      |
|  ◇ ◇ ◇ ◇ ◇ ◆ ◆ ? ? ◆ ◈ ? ? ? ...   (? = undiscovered silhouette + tier colour)     |
|-----------------------------------------------------------------------------------|
| Selected: THE NINTH BETRAYAL (Profane · Uncommon)                                 |
|  + Soul Spears +1 pierce     − −5% Soul Spear speed     + +4% max HP               |
|  Found: floor 2 boss, run #14                                                     |
| [Enter] Equip/Unequip    [Esc] Back    [Space] DESCEND                             |
+-----------------------------------------------------------------------------------+
```
- During a run, Tab → pause shows the 3 worn sins read-only.
- On unlock (after the boss): a short "A Sin Has Been Confessed" toast plus a card with the tier colour. The sin can be equipped next run. Duplicates show "→ +80 Souls".
- Run-end screen: lists newly unlocked sins with Souls/Sigils.

## 8. Example builds (equipped before the run, 3 slots)
1. **Soul Lancer:** Soul Spears + Pierce + Multi Projectiles. Sins: The Ninth Betrayal, The Seventh Seal Broken, Wrath of Brimstone. Spears pierce and bounce a little more, a small nudge rather than a new build.
2. **Funeral Carpet:** Curse Rune + Bigger Area + Echo. Sins: Mother's Last Curse, The Unwept Child, The Flayed Saint's Mirror. Curse explosions get bigger and hit harder, but stay out of range of your own blasts.
3. **Unholy Choir (God-Disliked):** Skeleton Warriors/Archers + Fireball + Fork. Sins: What Even Heaven Refused, Lantern of the Last Shepherd, Crypt-Warden's Oath. Skeletons cast Fireball; you dodge and position.
4. **Lone Tithe (God-Disliked):** any projectile build. Sins: Lucifer's Unpaid Tithe and nothing else, giving +50% damage and +2 projectiles from 2 empty slots.

## 9. Tunables for HellordTuning.cs (proposed, tag 733)
- `sinEnabled733`=true
- `sinSlotSoulCost733`={100,400,1000}; `sinSlotSigilCost733`={0,1,3}; `sinSlotsMax733`=3; `sinGodMax733`=1; `sinDamnedMax733`=2
- `sinBossUnlockChance733`=0.12; `sinFinalBossUnlockChance733`=0.25; `sinPityBosses733`=8; `sinPityMul733`=2
- `sinTierWeights733`={62,28,7,2.6,0.4}
- `sinRarityBand733`={100,50,20,5}; `sinWeight733[100]` (per-sin weights, §5/§6)
- `sinDupRerolls733`=1; `sinDupSouls733`={15,30,80,150,400}; `sinDupGodSigils733`=1
- `sinStatValues733[]` (table §5); `sinProfaneStatTier733`=Common; `sinDamnedStatTier733`=Cursed
- Per-special values: `sin01Pierce`=1, `sin01SpeedMul`=0.95; `sin02AreaMul`=1.15, `sin02CdMul`=1.05; … `sin36EveryN`=4, `sin36Repeats`=2, `sin36HpMul`=0.65; `sin38GrowPerSec`=0.04, `sin38Max`=0.8, `sin38Stun`=0.4; `sin39PerEmpty`=0.25; `sin40ReviveHp`=0.5, `sin40DecayPer10s`=0.01. Every number in §6 gets a field.

## 10. Open questions
See Decisions_for_VH.md → "t733 Sins v0.2".

## 11. VH decisions applied
- t727u: Pacts stay.
- t729u: Relics cut entirely.
- t731u: Sins are meta, equipped pre-run, no mid-run changes, max 3 slots (Souls + Sigils), small boss unlock chance, better sins are milder, God-Disliked are build-changing.
- t732u: per-sin weight within each tier.

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
