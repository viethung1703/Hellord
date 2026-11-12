> **SUPERSEDED by Hellord_sins_design_v0.4.md (t749u, 2026-11-04).**

# Hellord — Sins System v0.3 (FINAL DESIGN DRAFT for VH — design only, no code, no build)
Supersedes v0.2. Applies VH decisions t727u, t729u, t731u, t732u, t733u, t734u, t735u, t736. All numbers are tunables (`HellordTuning.cs`, tag 737) and none of them have been play-tested.

## Changelog
- t737u (2026-11-03): Eastern-lore rename (VH: "this hell base on easten lore so westen name is not fit"). All 40 specials and the 6 Heaven-Forsaken quotes are renamed. Tier display names: Common→Mortal, Cursed→Hungry-Ghost, Profane→Yama-Judged, Damned→Avīci, God-Disliked→Heaven-Forsaken. Stat pattern now uses Three Poisons / Ten Evil Deeds + Diyu places. Effects, trade-offs, weights and numbers are UNCHANGED. Tunable/code ids keep their internal names (e.g. `sinDamnedMax737`, `sinGodMax737`).
- Lore consistency: existing floors are Ox Pit / Frozen Hell / Burning Hell; bosses are Ox-Head, Frost Ox-Head, Horse-Face, Qilin Chariot, Ice Gorilla; the Ten Yama Kings come from Core Combat v0.2.8. Names reuse these.
- System-name options (default stays **Sins**, VH's choice): **Karma / Nghiệp** (Buddhist, also a Vietnamese word), **Yin Debts / Nợ Âm** (debts owed to the underworld, which fits the ledger theme), **Ledger Marks** (entries in the Book of Life and Death).

## 0. Rules
- **Sins panel (menu):** every sin that ever drops is saved there permanently. Before a run, the player equips up to the unlocked slot count.
- **Mid-run drop:**
  - If an unlocked slot is free, the sin auto-equips into it (toast).
  - If every slot is full, the game pauses and offers a choice: swap it with one equipped sin, or keep the current sins. Either way the new sin is saved to the panel.
- **Slots:** max 3, unlocked with Souls + Sigils. Souls come from the same wallet as Pacts.
- **Pacts stay. Relics are cut.**
- **Tiers:** Mortal / Hungry-Ghost = stat sins (60 total). Yama-Judged 20, Avīci 14, Heaven-Forsaken 6. 100 sins in total.

## 1. Drop rates (VH t734u / t735u)
| Source | Normal (stat) | Rare (Yama-Judged/Avīci) | Heaven-Forsaken |
|---|---|---|---|
| Any demon kill | 1/1000 | 1/10000 | — |
| Boss kill | 1/20 | 1/100 | 1/100 |

- **Roll order per kill:** Heaven-Forsaken (bosses only), then Rare, then Normal. At most **1 sin per kill**; the first success wins. Rolls are independent per kill.
- **Normal splits by tier weight:** Mortal 70 / Hungry-Ghost 30 (`sinNormalTierW737`).
- **Rare split (proposal):** Yama-Judged 70 / Avīci 30 (`sinRareTierW737`). Any demon gives Avīci 1/33,000; a boss gives Avīci 0.3%.
- **Within a tier:** per-sin weight (Frequent 100 / Uncommon 50 / Rare 20 / Very Rare 5), as in v0.2 (t732u).
- **Estimate (unmeasured):** 1000–1500 kills + 3–4 bosses per run. That gives about 1.2–1.7 normal sins per run, a rare sin every ~6–8 runs, and a Heaven-Forsaken every ~25–35 runs.

## 2. Duplicates
- **Duplicate check:** a roll that lands on an already-collected sin rerolls once within the same tier (`sinDupRerolls737`=1), so new sins come up more often.
- **Still a duplicate, and the sin is NOT equipped this run:** it is offered like a fresh drop (auto-equip or swap choice). It isn't saved twice. Nothing is converted.
- **Still a duplicate, and the sin is already equipped:** it converts to Souls (Mortal 15 / Hungry-Ghost 30 / Yama-Judged 80 / Avīci 150 / Heaven-Forsaken 400 + 1 Sigil) (`sinDupSouls737`, `sinDupGodSigils737`).
- **Full tier:** if every sin in the tier is collected, the duplicate goes straight to the rules above.

## 3. Slots and currency
- **Sigils now:** 1 per boss (`sigilsPerBoss51` 0→1, t736), plus 1 per win. That's about 3–5 Sigils per run.
- **Souls estimate:** about 150–250 per run (estimate).

| Slot | Souls | Sigils | Roughly reachable |
|---|---|---|---|
| 1 | 100 | 1 | after run 1 |
| 2 | 500 | 4 | about run 3–4 |
| 3 | 1200 | 10 | about run 7–9 |

- **Equip limits:** max 1 Heaven-Forsaken and max 2 Avīci equipped (kept from v0.2).
- **Souls wallet:** shared with Pacts (the default from t736 while VH hasn't decided).

## 4. Stat sins (60, fixed list, named by pattern)
Name = `[Karma word] of [Hell place]` (Eastern: Buddhist Three Poisons + Ten Evil Deeds; places from Diyu, the Ten Courts, the 18 levels and Vietnamese folk hell). Each stat family has a fixed sin word, and each sin's theme is fixed, so the list is finite and collectible.
- Karma words: Greed (Tham) = HP / pickup, Delusion (Si) = armor, Hatred (Sân) = damage, Slander = crit, Theft = XP, Wrong View = area, Killing = speed / cooldown, Idle Speech = minions.
- **Mortal (36):** 12 stats × 3 themes, with weights 100/50/20 so the 3rd theme is the rarer high roll.
- **Hungry-Ghost (24):** 12 pairs of 2 stats × 2 themes, with weights 100/20.

| # | Stat | Mortal value (3 themes) | Hungry-Ghost (stat used in a pair) |
|---|---|---|---|
| 1 | Max HP | +4 / +6 / +8% (Greed of the Ox Pit / of the Hungry Ghosts / of the Eighteenth Level) | +10% |
| 2 | Move speed | +3 / +4 / +5% (Killing of Naihe Bridge / of the Hook Court / of the Embers) | +6% |
| 3 | Pickup range | +8 / +12 / +16% (Greed of the Needle-Throat / the Offering Table / the Empty Bowl) | +18% |
| 4 | Armor | +2 / +3 / +4 (Delusion of the Iron Bed / the Stone Mill / the Bronze Pillar) | +5 |
| 5 | Crit chance | +2 / +3 / +4% (Slander of the Tongue Court / the Scissors / the Karma Mirror) | +5% |
| 6 | Damage, all | +3 / +4 / +6% (Hatred of the Sawing Court / the Sea of Fire / the Flaying Court) | +7% |
| 7 | Fire damage | +6 / +9 / +12% (Hatred of the Burning Hell / the Joss Fire / the Oil Cauldron) | +14% |
| 8 | Cold damage | +6 / +9 / +12% (Hatred of the Frozen Hell / the Ice Lotus / the Cold Yin Wind) | +14% |
| 9 | Lightning damage | +6 / +9 / +12% (Hatred of the Thunder Court / the Lightning Talisman / the Struck) | +14% |
| 10 | Physical/Bone/Blood damage | +6 / +9 / +12% (Hatred of the Knife Mountain / the Blood Pool / the Bone Field) | +14% |
| 11 | Cooldown | −2 / −3 / −5% (Killing of the Gong / the Horse-Face Gallop / the Last Hour) | −6% |
| 12 | Minion damage + HP (Idle Speech) | +6 / +9 / +12% (Idle Speech of the Grave / the Yin Army / the Unburied) | +14% |

- Hungry-Ghost pairs, proposed (each pair has 2 themes): HP+Armor, HP+Speed, Damage+Crit, Fire+Area, Cold+Cooldown, Lightning+Crit, Physical+Bleed effect, Minion+HP, Pickup+XP, Speed+Dash cooldown, Area+Duration, Curse/Soul damage+Cooldown.
- Every value goes in `sinStatValues[]`. Every name is generated, with no hand writing needed.


## 5. Named specials — Yama-Judged and Avīci (strength between v0.1 and v0.2; Avīci stronger than Yama-Judged)
### Yama-Judged (gold) — 20 · each also gives +1 Mortal-range stat
| # | Name | Effect | Trade-off | Weight | NEW? |
|---|---|---|---|---|---|
| 1 | Spear of the Tongue-Ripping Court | Soul Spears +2 pierce | −10% Soul Spear speed | 100 |  |
| 2 | Meng Po's Bitter Ladle | Curse Rune explosions +25% area and +10% damage | Curse Rune cooldown +10% | 100 |  |
| 3 | Mouth of the Needle-Throat Ghost | 6% chance per kill to heal 1 HP | −20% pickup range | 100 |  |
| 4 | Veil of the Sea of Fire | Burn lasts +2s; burning enemies take +5% damage | −15% Cold damage | 100 |  |
| 5 | Thunder Drum of the Fifth Court | Chain Lightning +2 jumps | Chain Lightning −10% damage | 100 |  |
| 6 | Hook of the Blood Pool | +15% Physical damage to Bleeding enemies | −4% crit chance | 50 |  |
| 7 | Lantern of the Naihe Ferry | Grave Pool slows +20% more; hits chill | Grave Pool −15% damage | 50 |  |
| 8 | Brush of the Book of Life and Death | Executioner threshold +5% HP on Burning/Bleeding targets | −6% damage to enemies above 50% HP | 50 |  |
| 9 | Ox-Head's Iron Fork | Knockback +40%, knocked enemies take +5% damage for 1s | −6% move speed | 100 |  |
| 10 | Cinder Beads of the Burning Hell | Fire skills −10% cooldown | Non-Fire skills +6% cooldown | 50 |  |
| 11 | Cleaver of the Sawing Court | Blood Whip heal per hit +50% | Blood Whip −10% damage | 50 |  |
| 12 | Bone Abacus of the Karma Ledger | Soul Harvest bone chance +6%, +1 XP per bone | −6% XP gain | 50 |  |
| 13 | Incense of the Ghost Festival | Storm Ring hits have 20% chance to Bleed | Storm Ring radius −10% | 20 |  |
| 14 | Death Bell of White Impermanence | Every 25th kill drops a free Curse Rune at that spot | Curse Rune cooldown +15% | 20 | [NEW: on-kill trigger] |
| 15 | Ribs of the Knife Mountain | Bone Orbit +1 skull | Orbit radius −15% | 20 |  |
| 16 | Joss Paper of the Burned Offering | Fork children +20% damage and Burn | Fork shots −10% speed | 50 |  |
| 17 | Shard of the Karma Mirror | Echo repeats +12% damage | Echo skill cooldown +10% | 50 |  |
| 18 | Oath of the Yin Soldiers | Skeleton Warriors +30% HP and block +1 bullet | −5% move speed | 100 |  |
| 19 | Fire Wheel of Nezha's Ghost | Dash leaves a burn patch (3s, 40% Fireball damage/s) | Dash cooldown +15% | 5 | [NEW: on-dash patch] |
| 20 | Hell Bank Note of the First Hand | First skill slot +12% damage | Other skills −5% damage | 20 |  |

### Avīci (red) — 14 · each also gives +1 Hungry-Ghost-range stat
| # | Name | Effect | Trade-off | Weight | NEW? |
|---|---|---|---|---|---|
| 21 | Dirge of the Unburied (Cô Hồn) | Skeleton Archers' arrows Burn and +15% damage | Archers −15% attack speed | 50 |  |
| 22 | The Broken Talisman of Zhong Kui | Projectiles gain +2 Bounce | Projectiles −15% damage | 100 |  |
| 23 | Nails of the Bronze Pillar | Bleed +40% damage while you stand still | −10% move speed | 100 | [NEW: standing-still check] |
| 24 | Red Thread of the Ghost Bride | Shock +2s and +10% shock effect | Take +10% damage while a shocked enemy is within 2u | 50 |  |
| 25 | Second Belly of the Hungry Ghost | +1 card option on every 3rd level-up | −10% XP gain | 20 |  |
| 26 | The Unmourned Child (Vong Nhi) | Cursed enemies' death explosion +40% damage, +15% area | Explosions within 1.5u deal 4 damage to you | 50 |  |
| 27 | Hand of the Frozen Hell | Frozen enemies take +30% damage; frozen kills shatter (small cold area) | Fire damage −25% | 100 |  |
| 28 | Black Impermanence's Chain | Minions rise again 50% faster, +15% minion damage | Your own skills −12% damage | 50 |  |
| 29 | Blade of the Beheading Platform | Executioner kills refund 20% of the skill's cooldown | Executioner damage −10% | 20 |  |
| 30 | Nine-Forked Lie of the Tongue Court | Chain Lightning first bolt splits into 3 at 50% damage | Chain Lightning cooldown +20% | 5 | [NEW: split bolt] |
| 31 | Heart of the Oil Cauldron | Burn stacks up to 3 | Burn duration −30% | 20 | [NEW: burn stacks] |
| 32 | Debt Ledger of the Ten Courts | +6% damage per boss killed this run (max +24%) | Boss HP +15% | 20 |  |
| 33 | Bone Throne of the Pit of Ox-Head | Every 4th Bone Orbit skull hit fires a Soul Spear outward | Bone Orbit −15% damage | 5 | [NEW] |
| 34 | Breath of the Hanged Ghost (Ma Thắt Cổ) | Life Leech cap per second +50%; overheal becomes a shield (max 10% HP) | Other healing −30% | 100 | [NEW: shield] |

### Heaven-Forsaken (UNCHANGED from v0.2) (black-violet) — 6 · build-changing, no stat
| # | Name | Effect | Trade-off | Weight | NEW? |
|---|---|---|---|---|---|
| 35 | What the Jade Emperor Struck Out | Your minions also cast your non-minion skills (each skeleton picks one, at 60% damage) | Your own casts −50% damage | 50 | [NEW: minion casts skill] |
| 36 | The Nineteenth Level | Every 4th cast of any skill repeats 2 more times | Max HP −35% | 100 | [NEW: global echo] |
| 37 | Tongue That Spoke Yama's True Name | All hits apply Burn, Chill, Shock and Bleed | −30% damage; enemies +10% speed | 20 | [NEW: multi-status] |
| 38 | Crown Refused at the Gate of Heaven | Storm Ring and Bone Orbit +4% area per second without taking a hit (max +80%) | Getting hit resets it and stuns you 0.4s | 50 | [NEW: growth stack, stun] |
| 39 | Debt Unpaid to the Tenth King | Each EMPTY sin slot: +25% damage and +1 projectile | Can't equip Mortal/Hungry-Ghost sins with it | 20 | |
| 40 | The Soup Meng Po Would Not Pour | Once per floor, revive at 50% HP and blast the screen | Max HP −1% per 10s alive (resets on revive) | 5 | [NEW: revive] |

What the gods said:
- #35: "Its name was struck from the Book of Life and Death. You wrote it back."
- #36: "There are eighteen levels. We never dug the nineteenth. You did."
- #37: "Even the Ten Kings do not say that name aloud."
- #38: "It was forged for one who never kneeled before the Jade Emperor."
- #39: "The Tenth King keeps every ledger. Yours is still open."
- #40: "You crossed Naihe Bridge and refused the soup. Now you remember everything."


[NEW] count: **12** (7 Yama-Judged/Avīci: #14, 19, 23, 30, 31, 33, 34; 5 Heaven-Forsaken: #35–38, 40).

## 6. UI screens
**A. Sins panel (main menu → "Sins", next to Pact)**
```
+---------------------------------- SINS ----------------------------------------+
| EQUIPPED: [1 ◆ Spear of the Tongue-Ripping Court] [2 ◇ Hatred of the Sea of Fire] [3 🔒 1200 Souls·10✦] |
| Wallet: 640 Souls · 5 Sigils                                   [Unlock slot]   |
|--------------------------------------------------------------------------------|
| COLLECTION 23/100   [All][Stat][Yama][Avīci][Heaven]   ? = not found (tier col)|
| ◇◇◇◇◆◆?◆◈??◇◇? ...                                                              |
|--------------------------------------------------------------------------------|
| SPEAR OF THE TONGUE-RIPPING COURT · Profane · Frequent                                        |
| + Soul Spears +2 pierce   − −10% Soul Spear speed   + +5% max HP               |
| First found: floor 2, run #14             [Enter] Equip/Unequip  [Esc] Back    |
+--------------------------------------------------------------------------------+
```
**B. Mid-run drop**
- A free slot means a toast plus a slot flash ("Sin Confessed: X → Slot 2").
- Full slots open a pause card: `NEW: X (effect/trade-off)` | `[1][2][3] swap with…` | `[Esc] Keep current — saved to Sins`. It uses the t718u input guard (delay + release) so the pick key can't double-fire.
**C. Pause (Tab)** shows the equipped sins with their effects, read-only.
**D. Run-end** lists new sins, duplicate conversions, Souls and Sigils.

## 7. Build plan (no code yet)
- **Data, in a new `HellordSins737.cs`:** `enum SinTier`; `SinDef` (id, name, tier, weight, stat ids/values, effect key, trade-off key, isNew); a static `Sins[100]` table. Stat-sin names come from the pattern in §4.
- **Rolls:** `RollSinOnKill(bool boss)`, hooked from the existing kill/boss-death path (`HellordGame` kill handler, `HellordBoss` death). It runs a two-step weighted pick with dup reroll.
- **Effects:** `SinMods` aggregate, applied where the stats/supports already read their multipliers:
  - `HellordSkills` (damage, cooldown, pierce, bounce, chains, orbit count)
  - `HellordBurn` (burn duration and stacks)
  - `HellordSummon561` (minions)
  - the dash code in `HellordV49`
  - `HellordCards` (card option count)
  - Each named special is one switch-case keyed by id. [NEW] mechanics are separate tasks.
- **Save:** PlayerPrefs, prefix `PK+"sin_"`, the same style as `HellordPact.cs`:
  - `sin_owned` (100-bit string), `sin_eq0..2`, `sin_slots`, `sin_found_<id>` (run# and floor, optional).
  - Run-time equipped state lives in `HellordGame`.
  - Souls and Sigils use the existing `_souls51` and `_sigils51`.
- **UI:**
  - Sins panel tab in `HellordMainMenu69` / `HellordScreens`.
  - Drop toast and swap card in `HellordUI`.
  - Pause list in `HellordHud`.
  - Test menu tab in `HellordTestMenu`: grant a sin, give all, reset, and force-drop rates.
- **Tuning:** every field in §8 goes in `HellordTuning.cs`, plus `sigilsPerBoss51`=1.
- **Hellord Art (needs VH approval before wiring):**
  - 5 tier frames (grey/blue/gold/red/black-violet), 24px plus a large card size.
  - Icons: 7 stat-family icons tinted per tier for the stat sins, 40 unique icons for the specials, and an undiscovered "?" silhouette.
  - Sins panel mockup and drop/swap card mockup.
- **Hellord FX (needs VH approval):**
  - Sin drop orb/glow in the tier colour, with a Heaven-Forsaken special (violet pillar + screen tint).
  - Equip flash on the slot.
  - On-world FX for the [NEW] sins: burn patch, split bolt, shield, revive blast.
- **Suggested phases:**
  1. Data, save, drops, Sins panel and stat sins.
  2. The 27 Yama-Judged/Avīci sins that need no new mechanics, plus Debt Unpaid to the Tenth King.
  3. The [NEW] mechanics and the remaining Heaven-Forsaken.
  4. Art and FX swap-in after approval.

## 8. Tunables (tag 737)
- `sinEnabled737`=true
- `sinDemonNormalChance737`=0.001; `sinDemonRareChance737`=0.0001
- `sinBossNormalChance737`=0.05; `sinBossRareChance737`=0.01; `sinBossGodChance737`=0.01; `sinMaxPerKill737`=1
- `sinNormalTierW737`={70,30}; `sinRareTierW737`={70,30}; `sinRarityBand737`={100,50,20,5}; `sinWeight737[100]`
- `sinDupRerolls737`=1; `sinDupSouls737`={15,30,80,150,400}; `sinDupGodSigils737`=1
- `sinSlotSoulCost737`={100,500,1200}; `sinSlotSigilCost737`={1,4,10}; `sinSlotsMax737`=3; `sinGodMax737`=1; `sinDamnedMax737`=2
- `sigilsPerBoss51`=1 (change from 0)
- `sinSwapPause737`=true; `sinSwapDelay737`=0.3 (t718u-style guard)
- `sinStatValues737[]` (table §4); `sinProfaneStatTier737`=Mortal; `sinDamnedStatTier737`=Hungry-Ghost
- **Per named special:** one field per number in §5. Examples:
  - `sin01Pierce`=2, `sin01SpeedMul`=0.9
  - `sin02AreaMul`=1.25, `sin02DmgMul`=1.1, `sin02CdMul`=1.1
  - `sin05Jumps`=2, `sin05DmgMul`=0.9
  - `sin22Bounce`=2, `sin22DmgMul`=0.85
  - `sin27FrozenTaken`=0.3, `sin27FireMul`=0.75
  - `sin32PerBoss`=0.06, `sin32Max`=0.24, `sin32BossHp`=1.15
  - `sin36EveryN`=4, `sin36Repeats`=2, `sin36HpMul`=0.65
  - `sin39PerEmpty`=0.25, `sin40ReviveHp`=0.5, `sin40DecayPer10s`=0.01
  - and so on.

## 9. VH decisions applied
- t727u: Pacts stay.
- t729u: Relics are cut.
- t731u: Sins are a meta collection, with 3 slots bought with Souls + Sigils.
- t732u: each sin has its own weight inside its tier.
- t733u: there's a Sins panel, mid-run drops auto-equip or offer a swap, and every drop is saved.
- t734u/t735u: drop rates as in §1.
- t736: bosses give 1 Sigil, rare effects are a bit stronger, and the Souls wallet is shared by default.
