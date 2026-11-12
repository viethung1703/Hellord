# Hellord — Infernal Sins v0.4 (DESIGN DRAFT for VH — design only, no code, no build)
Supersedes v0.3. Applies VH decisions t746u–t749u on top of v0.3 (t727u–t738u). All numbers are tunables (`HellordTuning.cs`, tag 737); none play-tested.

## Changelog
- t765u: the swap card pauses the game. t767u: #45 spokes have a fixed path each; #45 visuals proposed. t768u: global Minion tag rule. t769u: charmed Animals are not Minions.
- t764u: Samsara Wheel option A (Six Paths) locked as HD #45 (Bone Orbit). HD 11, total 95.
- t762u/t763u: Meng Po removed from HD (shelved). Fire Wall locked as the Hellfire Pillar replacement (§5b), so Ring of the Burning City is active. Freeze locked (§5c) and Eight Cold Hells locked. Chain Lightning HD = Lightning Rod (with self-strike). Samsara Wheel effect remade (3 options pending). HD 10 locked (#35–44 renumbered), total 94.
- t757u–t761u: Heaven-Defying (HD) rule: each one transforms one skill or pact path (keeps that skill's card/slot, level-ups, supports and pacts). HD count is now VARIABLE: 9 locked (#35–43, renumbered). Tiers: Mortal 50 / Demonic 34 / HD 9 = 93 sins. More HD will come with new skills. Cut 6; #43 Eight Cold Hells is MAYBE; Bone Orbit and Chain Lightning remakes have 2 options each.
- t751u: no tier equip limits; dup Souls 20/100/400 + 1 Sigil approved.
- t750u: tiers now 50 Mortal / 34 Demonic / 16 Heaven-Defying. Cut 10 Mortal singles (middle theme of stats 1–10). Added 10 new Heaven-Defying #41–50 (9 need new mechanics).
- t746u: system name is **Infernal Sins** (short "Sins" in UI is OK).
- t747u/t749u: only **3 tiers**: **Mortal** (normal) / **Demonic** (rare) / **Heaven-Defying** (myth; formerly God-Disliked / Heaven-Forsaken).
  - Mortal 60 = old Common+Cursed (Mortal + Hungry-Ghost stat sins). Demonic 34 = old Profane+Damned (Yama-Judged + Avīci). Heaven-Defying 6 = old top tier. Still 100 sins.
  - The 70/30 sub-splits inside normal and rare are removed; per-sin weights inside each tier are kept as-is.
  - Duplicate Souls collapse to 3 values (20 / 100 / 400 + 1 Sigil).
  - Equip limit proposal: max 1 Heaven-Defying, no Demonic cap (PENDING VH).
  - All individual Eastern sin names kept unchanged. No sin name contained an old tier word ("Hungry Ghost" in #25 and "of the Hungry Ghosts" are lore places, not tier labels, so they stay).
- Previous (t737u): Eastern-lore rename of all sins; v0.3 history is in the v0.3 doc.

## 0. Rules
- **Infernal Sins panel (menu):** every sin that ever drops is saved there permanently. Before a run, the player equips up to the unlocked slot count.
- **Mid-run drop:**
  - If an unlocked slot is free, the sin auto-equips into it (toast).
  - If every slot is full, the game pauses and offers a choice: swap it with one equipped sin, or keep the current sins. Either way the new sin is saved to the panel.
- **Slots:** max 3, unlocked with Souls + Sigils. Souls come from the same wallet as Pacts.
- **Pacts stay. Relics are cut.**
- **Tag rule (t768u, global):** anything tagged **Minion** gets every minion buff (pacts, supports, sins). Sin-created minions are tagged Minion: #45 Asura minions, #39 risen demons, #42 Ox-Head/Horse-Face guardians (#45 Animal-path charmed demons are NOT Minions and get no minion buffs, t769u). Necromancer minion pacts apply to all of them. Their FPS caps still apply.
- **Tiers (3, t757u):** Mortal 50 (stat sins) · Demonic 34 (named specials with trade-offs) · Heaven-Defying VARIABLE (skill transforms; 11 locked now, so 95 in total). More HD will be added with new skills/pacts; the HD count is not fixed. Collection UI shows N/total dynamically.

## 1. Drop rates (VH t734u / t735u)
| Source | Mortal (normal) | Demonic (rare) | Heaven-Defying (myth) |
|---|---|---|---|
| Any demon kill | 1/1000 | 1/10000 | — |
| Boss kill | 1/20 | 1/100 | 1/100 |

- **Roll order per kill:** Heaven-Defying (bosses only), then Demonic, then Mortal. At most **1 sin per kill**; the first success wins. Rolls are independent per kill.
- **No sub-splits** (removed t747u). A tier roll picks one sin from that whole tier by per-sin weight (Frequent 100 / Uncommon 50 / Rare 20 / Very Rare 5; t732u).
- Side effect: old weights are kept, so the former stronger sins (ex-Hungry-Ghost pairs, ex-Avīci) are now relatively more common than under 70/30. Mortal pool (t750u): 26 singles (10×120 + 2×170 = 1540) + 24 pairs (1440); a pair sin is ~48% of Mortal drops (was 30% under 70/30). Demonic: ex-Avīci weights 690 vs ex-Yama 1185, so ex-Avīci is ~37% of Demonic drops (was 30%).
- **Estimate (unmeasured):** 1000–1500 kills + 3–4 bosses per run. That gives about 1.2–1.7 normal sins per run, a rare sin every ~6–8 runs, and a Heaven-Defying every ~25–35 runs.

## 2. Duplicates
- **Duplicate check:** a roll that lands on an already-collected sin rerolls once within the same tier (`sinDupRerolls737`=1), so new sins come up more often.
- **Still a duplicate, and the sin is NOT equipped this run:** it is offered like a fresh drop (auto-equip or swap choice). It isn't saved twice. Nothing is converted.
- **Still a duplicate, and the sin is already equipped:** it converts to Souls (Mortal 20 / Demonic 100 / Heaven-Defying 400 + 1 Sigil; approved t751u) (`sinDupSouls737`, `sinDupHeavenDefyingSigils737`).
- **Full tier:** if every sin in the tier is collected, the duplicate goes straight to the rules above.

## 3. Slots and currency
- **Sigils now:** 1 per boss (`sigilsPerBoss51` 0→1, t736), plus 1 per win. That's about 3–5 Sigils per run.
- **Souls estimate:** about 150–250 per run (estimate).

| Slot | Souls | Sigils | Roughly reachable |
|---|---|---|---|
| 1 | 100 | 1 | after run 1 |
| 2 | 500 | 4 | about run 3–4 |
| 3 | 1200 | 10 | about run 7–9 |

- **Equip limits (t751u, VH approved):** NONE. Any mix of tiers in the 3 slots, including 3 Heaven-Defying (`sinHeavenDefyingMax737`=3, `sinDemonicMax737`=3 = no cap).
- **Souls wallet:** shared with Pacts (the default from t736 while VH hasn't decided).

## 4. Mortal — stat sins (50, fixed list, named by pattern)
Name = `[Karma word] of [Hell place]` (Eastern: Buddhist Three Poisons + Ten Evil Deeds; places from Diyu, the Ten Courts, the 18 levels and Vietnamese folk hell). Each stat family has a fixed sin word, and each sin's theme is fixed, so the list is finite and collectible.
- Karma words: Greed (Tham) = HP / pickup, Delusion (Si) = armor, Hatred (Sân) = damage, Slander = crit, Theft = XP, Wrong View = area, Killing = speed / cooldown, Idle Speech = minions.
- **Mortal singles (26, t750u — SUPERSEDED by t784u §4b: 13 rolled singles):** stats 1–10 have 2 themes (weights 100/20; middle theme cut t750u), stats 11–12 keep 3 themes (100/50/20).
- **Mortal pairs (24, ex-Hungry-Ghost):** 12 pairs of 2 stats × 2 themes, with weights 100/20.

| # | Stat | Single value (3 themes) | Pair value (stat used in a pair) |
|---|---|---|---|
| 1 | Max HP | +4 / +8% (Greed of the Ox Pit / of the Eighteenth Level) | +10% |
| 2 | Move speed | +3 / +5% (Killing of Naihe Bridge / of the Embers) | +6% |
| 3 | Pickup range | +8 / +16% (Greed of the Needle-Throat / the Empty Bowl) | +18% |
| 4 | Armor | +2 / +4 (Delusion of the Iron Bed / the Bronze Pillar) | +5 |
| 5 | Crit chance | +2 / +4% (Slander of the Tongue Court / the Karma Mirror) | +5% |
| 6 | Damage, all | +3 / +6% (Hatred of the Sawing Court / the Flaying Court) | +7% |
| 7 | Fire damage | +6 / +12% (Hatred of the Burning Hell / the Oil Cauldron) | +14% |
| 8 | Cold damage | +6 / +12% (Hatred of the Frozen Hell / the Cold Yin Wind) | +14% |
| 9 | Lightning damage | +6 / +12% (Hatred of the Thunder Court / the Struck) | +14% |
| 10 | Physical/Bone/Blood damage | +6 / +12% (Hatred of the Knife Mountain / the Bone Field) | +14% |
| 11 | Cooldown | −2 / −3 / −5% (Killing of the Gong / the Horse-Face Gallop / the Last Hour) | −6% |
| 12 | Minion damage + HP (Idle Speech) | +6 / +9 / +12% (Idle Speech of the Grave / the Yin Army / the Unburied) | +14% |

- Pairs, proposed (each pair has 2 themes): HP+Armor, HP+Speed, Damage+Crit, Fire+Area, Cold+Cooldown, Lightning+Crit, Physical+Bleed effect, Minion+HP, Pickup+XP, Speed+Dash cooldown, Area+Duration, Curse/Soul damage+Cooldown.
- Every value goes in `sinStatValues[]`. Every name is generated, with no hand writing needed.


### 4b. ROLLED STATS (VH t784u/t785u, LOCKED; built in code, NOT play-tested)
PoE2-style farming: every sin's numbers roll when it drops. Each owned sin stores its own roll (q 0..1 per number).
- **Mortal singles: ONE sin per stat (13)**, weak/strong duplicates removed. Rolls (tunable `sinSingleMin737/Max737`):
  Greed of the Ox Pit HP +8..20% · Killing of Naihe Bridge Speed +2..7% · Greed of the Needle-Throat Pickup +8..22% · Delusion of the Iron Bed Armor +1..5 · Slander of the Tongue Court Crit +2..6% · Hatred of the Sawing Court Damage +3..9% · Hatred of the Burning Hell Fire +6..18% · Hatred of the Frozen Hell Cold +6..18% · Hatred of the Thunder Court Lightning +6..18% · Hatred of the Knife Mountain Phys/Bone/Blood +6..18% · Killing of the Gong Cooldown −2..7% · Idle Speech of the Grave Minion +6..18% · Wrong View of the Nine Springs Area +5..14% (new single).
- **Mortal pairs: 24 kept**, each stat rolls on its own (`sinPairMin737/Max737`), e.g. HP 6..14%, Armor 3..7, Fire 9..19%.
- **Mortal now 37** (13 + 24). **Total 82** (37 + 34 + 11). Counter shows X/82.
- **Demonic / HD:** every number in the effect line rolls base×0.7..1.3 (Demonic) / ×0.8..1.25 (HD) (tunable). Drawbacks fixed.
- **Display:** PoE style `+14% MAX HP [8-20]`, plus "ROLL: 73% OF MAX" or "ROLL: PERFECT" (≥ `sinPerfectAt737` 0.995).
- **Duplicates:** reroll-once stays. Owned sin dropped again → compare the average normalized roll. Higher = REPLACES the stored roll (live if equipped) + **BETTER ROLL! card** (tier frame, OLD vs NEW lines, quality %, sound hook `sinBetterSfx737`, 4s, no pause). Equal/worse → Souls 20/100/400 (+1 Sigil HD), toast "WORSE ROLL -> +X SOULS". (Replaces the old "not equipped dup = fresh drop" rule.)
- **Save v2:** `sin_v`=2, `sin_owned` bitstring (82), `sin_r<id>` = comma floats. Old v1 saves: owned/equipped cleared, unlocked slots kept.

### 4c. t818u — Mortal twins merged (VH t818u, 2026-11-04 UTC+7) — SUPERSEDES the "24 pairs" lines above
- VH: two-stat sins must not have twins; rerolling already gives variety. Each combo now exists ONCE (rolled per stat, `sinPairMin737/Max737`; drop weight per combo `sinPairW737`, default 100 each; old 100/20 theme weights removed).
- **Mortal 25** (13 singles + 12 pairs). **Total 70** (25 / 34 / 11). Counter shows X/70 (dynamic).
- Kept names (one per combo):
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
- **Save v3:** v2 saves migrate (ids: singles same, twin pair 13+2k+t -> 13+k, Demonic/HD id-12). Owned twins merge to the kept sin with the better average roll; if both twins were equipped, one slot is freed. v1 still cleared (slots kept).
- **Proposed new Mortal pairs to refill toward ~50 (doc only, not code):**: Speed+Pickup ("Killing and Greed of the Ghost Market"), Armor+Minion ("Delusion of the Yin Army"), Crit+Phys ("Slander of the Knife Mountain"), Cold+Area ("Hatred of the Ice Field"), Fire+Cooldown ("Killing of the Oil Cauldron"), Lightning+Speed ("Killing of the Thunder Road"), HP+Pickup ("Greed of the Offering Table"), Damage+Area ("Hatred of the Wide Court"), Curse/Soul+Area ("Wrong View of Meng Po's Soup"), Bleed+Crit ("Slander of the Blood Pool"), Minion+Cooldown ("Idle Speech of the Gong"), XP+Area ("Theft of the Lantern Road"), Armor+Speed ("Delusion of the Bronze Road"). 13 more → Mortal 50.

## 5. Demonic (34) and Heaven-Defying (16)
### Demonic (red) — 34 · #1–20 (ex-Yama-Judged) give +1 single-range stat; #21–34 (ex-Avīci, stronger) give +1 pair-range stat
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

*(ex-Avīci, stronger)*
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

### Heaven-Defying (black-violet) — VARIABLE count: 11 locked (#35–45) · skill transforms
**Rule (VH t757u):** every Heaven-Defying (HD) sin targets one specific skill or pact path and transforms it for the run.
- The sin-skill keeps the original card and slot. Level-ups, supports and pacts still apply, but its behaviour changes.
- If the targeted skill isn't in the run, the sin is dormant (greyed icon, "Needs <skill>").
- More HD will be added as skills and pacts grow (VH plans many ice skills). Not play-tested.

| # | Name | Targets | Effect (◆ transform, + bonus) | Drawback | W | Status |
|---|---|---|---|---|---|---|
| 35 | Pearl of the Dragon King (Long Vương Châu) | Storm Ring | ◆ Storm Ring is centred on your mouse cursor instead of on you | Radius −25% | 50 | LOCKED |
| 36 | Ten Thousand Ghost Banner (Vạn Hồn Phiên) | Soul Spears (+ Soul Harvest) | + Kills store souls (max 20; Soul Harvest kills store 2)<br>◆ At max, all souls are released as free-flying ghosts that roam the room, seek demons and fly through them for 45s<br>+ Souls keep stacking during the swarm | Soul Spears −30% damage | 20 | LOCKED |
| 37 | Tongue of the Needle-Throat Ghost (Ngạ Quỷ) | Blood Whip | ◆ Blood Whip pulls demons in; demons under 15% HP are devoured (kill + 2% HP heal)<br>+ Gluttony: 1 stack per 100 devoured (max 10). Each stack gives +8% body size and +8% skill area. Stacks persist the whole run | −4% move speed per stack | 50 | LOCKED |
| 38 | Golden Crow of the Sun (Kim Ô) | Fireball | ◆ Fireball fires one slow sun that grows +10% per demon passed (max +150%)<br>◆ Multi Projectiles adds +20% size per level instead of projectiles | Fireball cooldown ×2 | 20 | LOCKED |
| 39 | Lamp of the Nine Nether Springs (Cửu U) | Curse Rune | ◆ Cursed demons rise as your minions for 8s (max 10) and cast a small Curse Rune at 40%<br>◆ Curse damage modifiers also apply to minions | Curse Rune area −40%; can't be Echoed | 20 | LOCKED |
| 40 | Ring of the Burning City (Hỏa Thành) | Fire Wall (§5b) | ◆ Fire Wall becomes a closed ring of fire that moves with you | Can't be aimed; −30% damage | 50 | LOCKED, active (Fire Wall locked t763u) |
| 41 | Ghost Mushrooms of Fengdu (Nấm Quỷ Phong Đô) | Grave Pool | ◆ Grave Pool becomes demon mushrooms: you drop one every 3s; after a 1.5s fuse it bursts for area damage + poison DoT + slow | You lose 3% of current HP per mushroom dropped | 50 | LOCKED |
| 42 | Ox-Head and Horse-Face Answer the Call (Ngưu Đầu Mã Diện) | Skeleton Warriors | ◆ Skeleton Warriors become 2 guardians (6× HP, taunt, cleave)<br>+ Guardians grow +15% size per skill level<br>◆ Extra minion count from Grave Muster / Legion of One Hundred becomes +25% damage per extra | Max 2 Skeleton Warriors | 20 | LOCKED |
| 43 | Blade of the Eight Cold Hells (Bát Hàn) | Frost Cleave | ◆ Frost Cleave becomes a 360° spin<br>◆ Your Burn becomes Freeze buildup (§5c) | −35% damage; you cannot Burn | 50 | LOCKED t763u |
| 44 | Lightning Rod of the Heavenly Tribulation (Dẫn Lôi Thiên Kiếp) | Chain Lightning | ◆ Every chain bounces back through you (demon → you → demon …). Each pass gives +1 Thunder charge (max 20)<br>+ At 20 charges, a Tribulation bolt strikes ALL demons on screen | Each pass costs 0.5% max HP. The full strike also hits YOU (15% max HP, tunable) | 20 | LOCKED t762u |

| 45 | Samsara Wheel of Six Rings (Lục Đạo Luân) | Bone Orbit | ◆ Bone Orbit becomes one huge 6-spoke bone wheel (radius 4u), one spoke per realm. A demon killed by a spoke is reborn into that path: Deva heals you 1% · Asura becomes a 6s minion · Human drops XP ×2 · Animal is charmed and attacks other demons · Hungry Ghost devours the nearest weak demon · Hell explodes for 40%<br>◆ More Orbitals = +10% wheel spin speed instead of skulls | −25% damage; slow spin (1 turn per 2s) | 20 | LOCKED t764u |

**#45 visuals (proposed, art pending VH approval; Art not contacted):**
- Hub: a bone dharma wheel.
- Spokes:
  - Deva: pale gold bone + halo.
  - Asura: red jagged bone.
  - Human: ivory bone + prayer bead.
  - Animal: horned/tusked bone with a green glow.
  - Hungry Ghost: thin cracked bone with a mouth.
  - Hell: charred black bone dripping fire.
- t767u: each spoke always gives its own path (not random).
**#44 visuals (VH):**
- The player looks more scorched as charges rise: smoke at 5, sparks at 10, blackened skin and crackling at 15, glowing cracks at 19.
- At 20, the bolt hits the screen and the player, then the look resets.
- → **Art/FX request for Hellord Art/FX (queued, not contacted):** a 4-stage scorch overlay on the player, the Tribulation full-screen bolt, and a self-hit flash. Must obey the t738 per-frame caps.

**Cut / removed:**
- t757u–t761u: Firecracker, Blood Pool, Taishang Furnace, Heavenly Tribulation (Dash), and the old Bone Orbit/Chain Lightning versions.
- t762u: Meng Po's Untouched Bowl, moved to Shelved ideas (§11).

[NEW] count: 7 Demonic (#14, 19, 23, 30, 31, 33, 34) + all 11 Heaven-Defying.

### 5b. Fire Wall skill (LOCKED t763u, replaces Hellfire Pillar)
- **Behaviour:**
  - Auto-casts a wall of fire across your aim direction, centred 3u ahead. Wall length 5u, thickness 0.6u, lasts 4s.
  - Demons crossing or standing in it take fire damage per 0.5s tick and get Burn.
  - Fire tag + Area tag. Hellfire Pillar's slot, card and level-ups move to Fire Wall.
- **Scaling per level:** +12% damage, +0.4u length, +0.3s duration. At Lv5 and Lv8, +1 wall segment angled 30°, making a V.
- **Supports:**
  - Bigger Area: longer and thicker.
  - Echo: a second wall 1.5u behind (counts against the t738 Echo caps).
  - Burn: stronger, longer Burn.
  - Added Damage / Faster Casting: as normal.
  - Knockback: the wall pushes demons back.
  - Executioner / Explode on Kill: work on wall kills.
  - Projectile supports (Multi Projectiles, Pierce, Fork, Bounce, Homing): no effect.
- **Pacts:**
  - Hellfire Lance (Pyromancer): the wall marches forward 2u/s instead of staying still.
  - Overheat and Meteor Pact apply unchanged.
  - Ring of the Burning City (#40) turns the wall into a moving ring around you.
- **Tunables (tag 737):** `fireWallLen737`=5, `fireWallThick737`=0.6, `fireWallDist737`=3, `fireWallSec737`=4, `fireWallTick737`=0.5, `fireWallDmgPerLvl737`=0.12, `fireWallLenPerLvl737`=0.4, `fireWallSecPerLvl737`=0.3, `fireWallVLevels737`={5,8}, `fireWallVAngle737`=30, `fireWallEchoGap737`=1.5, `fireWallLanceSpeed737`=2, `fireWallMaxLive737`=6.

### 5c. Freeze system (LOCKED t763u, replaces the hidden chill-count freeze)
- **Buildup:** every Cold hit adds buildup = damage ÷ (demon max HP × `freezeThresholdMul737`). A visible bar under demons decays after `freezeDecaySec737`=2s.
- **Frozen:** at 100%, the demon is frozen for `freezeSec737`=1.5s and takes +`freezeTaken737`=25% damage.
- **Shatter:** killing a frozen demon causes a Cold burst (`shatterFrac737`=0.4, 1u) under the t738 per-frame cap.
- **Bosses:** buildup ×`freezeBossMul737`=0.3, half the freeze duration, and +`freezeBossDimin737`=50% threshold after each freeze.
- **Sources:** Chill support, Frost Cleave and #43. This replaces the "3 chills = freeze" rule and the Chill Lv4 rule. This is the base system for future ice skills.

## 6. UI screens
**A. Infernal Sins panel (main menu → "Sins", next to Pact)**
```
+------------------------------ INFERNAL SINS -----------------------------------+
| EQUIPPED: [1 ◆ Spear of the Tongue-Ripping Court] [2 ◇ Hatred of the Sea of Fire] [3 🔒 1200 Souls·10✦] |
| Wallet: 640 Souls · 5 Sigils                                   [Unlock slot]   |
|--------------------------------------------------------------------------------|
| COLLECTION 23/95    [All][Mortal][Demonic][Heaven-Defying]   ? = not found (tier col)|
| ◇◇◇◇◆◆?◆◈??◇◇? ...                                                              |
|--------------------------------------------------------------------------------|
| SPEAR OF THE TONGUE-RIPPING COURT · Demonic · Frequent                                        |
| + Soul Spears +2 pierce   − −10% Soul Spear speed   + +5% max HP               |
| First found: floor 2, run #14             [Enter] Equip/Unequip  [Esc] Back    |
+--------------------------------------------------------------------------------+
```
**B. Mid-run drop**
- A free slot means a toast plus a slot flash ("Sin Confessed: X → Slot 2").
- Full slots open a pause card: `NEW: X (effect/trade-off)` | `[1][2][3] swap with…` | `[Esc] Keep current — saved to Sins`. It uses the t718u input guard (delay + release) so the pick key can't double-fire.
**C. Pause (Tab)** shows the equipped sins with their effects, read-only.
**D. Run-end** lists new sins, duplicate conversions, Souls and Sigils.

## 7. Build plan — PHASE 1 BUILT (t770u, 2026-11-04 05:07 UTC+7; compiled 0 errors, NOT play-tested)
**Phase 1 status:** `HellordSins737.cs` holds all 95 sins (50 Mortal live, 34 Demonic + 11 HD data only, "EFFECT NOT YET IMPLEMENTED", excluded from drops unless `sinDropUnimplemented737`). Drops, dupes, slots, Souls/Sigils, swap card (pause + 0.3s guard), Sins panel (main menu INFERNAL SINS button or Codex row), test-menu SINS tab (cheats off by default) are in. Mortal pair names are proposed placeholders; pair 2nd-stat values (Area 10, Duration 10, Bleed 14, XP 8, Dash cd 8, Curse/Soul 14) are proposals; Duration has no hook yet (no effect). Minion-tag rule skipped. Demonic "+1 stat" left TBD.

### Original plan
- **Data, in a new `HellordSins737.cs`:** `enum SinTier { Mortal, Demonic, HeavenDefying }`; `SinDef` (id, name, tier, weight, stat ids/values, effect key, trade-off key, isNew); a static `Sins[100]` table. Stat-sin names come from the pattern in §4.
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
  - 3 tier frames (grey/red/black-violet), 24px plus a large card size.
  - Icons: 7 stat-family icons tinted per tier for the stat sins, 50 unique icons for the specials, and an undiscovered "?" silhouette.
  - Sins panel mockup and drop/swap card mockup.
- **Hellord FX (needs VH approval):**
  - Sin drop orb/glow in the tier colour, with a Heaven-Defying special (violet pillar + screen tint).
  - Equip flash on the slot.
  - On-world FX for the [NEW] sins: burn patch, split bolt, shield, revive blast.
- **Suggested phases:**
  1. Data, save, drops, Sins panel and stat sins.
  2. The 27 Demonic sins that need no new mechanics, plus Debt Unpaid to the Tenth King.
  3. The [NEW] mechanics and the remaining Heaven-Defying.
  4. Art and FX swap-in after approval.

## 8. Tunables (tag 737)
- `sinEnabled737`=true
- `sinDemonNormalChance737`=0.001; `sinDemonRareChance737`=0.0001
- `sinBossNormalChance737`=0.05; `sinBossRareChance737`=0.01; `sinBossHeavenDefyingChance737`=0.01; `sinMaxPerKill737`=1
- (removed: `sinNormalTierW737`, `sinRareTierW737`) `sinRarityBand737`={100,50,20,5}; `sinWeight737[100]`
- `sinDupRerolls737`=1; `sinDupSouls737`={20,100,400}; `sinDupHeavenDefyingSigils737`=1
- `sinSlotSoulCost737`={100,500,1200}; `sinSlotSigilCost737`={1,4,10}; `sinSlotsMax737`=3; `sinHeavenDefyingMax737`=3; `sinDemonicMax737`=3 (=no cap, t751u)
- `sigilsPerBoss51`=1 (change from 0)
- `sinSwapPause737`=true; `sinSwapDelay737`=0.3 (t718u-style guard)
- `sinStatValues737[]` (table §4); `sinDemonicStatRange737`={single for #1–20, pair for #21–34}; `sinTierCount737`=3
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

- **Heaven-Defying tunables (t763u numbering; replace all earlier sin35–50 fields, incl. sin36/39/40 examples above):**
  - `sinHDDormantIfSkillMissing737`=true
  - `sin35RadiusMul`=0.75
  - `sin36MaxSouls`=20, `sin36HarvestSouls`=2, `sin36SwarmSec`=45 (30–60), `sin36GhostSpeed`=5, `sin36GhostDmgFrac`=0.3, `sin36HitCdPerDemon`=0.5, `sin36StackDuringSwarm`=true, `sin36MaxLiveGhosts`=40, `sin36SpearMul`=0.7
  - `sin37DevourHp`=0.15, `sin37Heal`=0.02, `sin37DevourPerStack`=100, `sin37MaxStacks`=10, `sin37SizePerStack`=0.08, `sin37AreaPerStack`=0.08, `sin37SpeedPerStack`=0.04, `sin37Persist`=run
  - `sin38GrowPerHit`=0.10, `sin38GrowMax`=1.5, `sin38MultiSizePerLvl`=0.2, `sin38CdMul`=2.0
  - `sin39RaiseSec`=8, `sin39MaxRaised`=10, `sin39RuneDmg`=0.4, `sin39AreaMul`=0.6
  - `sin40RingRadius`=3, `sin40DmgMul`=0.7
  - `sin41DropEvery`=3, `sin41FuseSec`=1.5, `sin41BurstRadius`=2, `sin41PoisonDps`=0.15, `sin41PoisonSec`=4, `sin41Slow`=0.3, `sin41HpCostCurrent`=0.03, `sin41MaxLive`=8
  - `sin42Guardians`=2, `sin42HpMul`=6, `sin42SizePerLvl`=0.15, `sin42DmgPerCount`=0.25
  - `sin43DmgMul`=0.65, `sin43BurnToFreeze`=true (freeze*737 in §5c)
  - `sin44MaxCharges`=20, `sin44HpPerPass`=0.005, `sin44StrikeDmgMul`=3.0, `sin44SelfStrikeHp`=0.15, `sin44ScorchStages`={5,10,15,19}
  - `sin45Radius`=4, `sin45TurnSec`=2, `sin45DmgMul`=0.75, `sin45SpinPerOrbital`=0.10, `sin45DevaHeal`=0.01, `sin45DevaHealPerSec`=0.03 (heal cap), `sin45AsuraSec`=6, `sin45AsuraDmg`=0.3, `sin45MaxAsura`=6, `sin45HumanXpMul`=2, `sin45CharmSec`=4, `sin45MaxCharmed`=8, `sin45CharmBossImmune`=true, `sin45GhostDevourHp`=0.2, `sin45GhostRange`=3, `sin45HellBurst`=0.4, `sin45HellRadius`=1.5, `sin45HellBurstsPerFrame`=4 (t738 cap shared), `sin45PathMode`=spoke (each spoke = fixed path)
  - `sinHeavenDefyingW737` weights in §5

## 9. VH decisions applied
- t727u: Pacts stay.
- t729u: Relics are cut.
- t731u: Sins are a meta collection, with 3 slots bought with Souls + Sigils.
- t732u: each sin has its own weight inside its tier.
- t733u: there's a Sins panel, mid-run drops auto-equip or offer a swap, and every drop is saved.
- t734u/t735u: drop rates as in §1.
- t736: bosses give 1 Sigil, rare effects are a bit stronger, and the Souls wallet is shared by default.
- t746u–t749u: name Infernal Sins; 3 tiers Mortal / Demonic / Heaven-Defying; sub-splits removed; dup Souls 20/100/400.
- t750u/t751u: 50/34 counts; no tier limits.
- t765u: swap card pauses. t767u: #45 fixed path per spoke. t768u: Minion-tag rule.
- t764u: Samsara Wheel A locked as #45; HD 11, total 95.
- t762u/t763u: Meng Po shelved; Fire Wall, Freeze, Eight Cold Hells and Lightning Rod locked; HD 10.
- t757u–t761u: HD = skill transforms, variable count; 9 locked; cuts; #38 per-100-devour stacks persist the whole run; #37 stacks during the swarm; #42 mushrooms.

## 10. Open questions for VH
0. ~~(t750u) approve 10 new HD~~ superseded by t757u–t761u.
1. ~~Equip limit~~ resolved t751u: no tier limits.
2. ~~Dup Souls~~ approved t751u.
3. With 70/30 removed, ex-pair Mortal sins drop ~48% of the time and ex-Avīci ~37% of Demonic (was 30%). Keep, or lower their weights?
4. ~~Swap card pauses?~~ YES, it pauses (t765u). Still open from v0.3: Phase 1 go?

## 11. Shelved ideas (not in the game; may return)
- **Meng Po's Untouched Bowl** (ex-HD, t762u): once per run, revive at full HP; max HP −20% until it has been used. VH may reuse it as a big keystone node in a future PoE2-style passive tree.

---
## t818u implementation notes (v0.0.69, 2026-11-04 UTC+7)
- All Demonic (#1-34) and Heaven-Defying (#35-45) effects are now live (`HellordTuning.sinDemonicHdLive818`). Code is in `Scripts/Runtime/HellordSinFx818.cs`. Hooks are spread across Game/Skills/V36/Necro/Summon/Pact/Cards.
- Fixed drawbacks: `sinDraw818[n-1]`. Demonic bonus stat ranges: `sinDemBonusStat818`. Every other number is a `sin*818` field in HellordTuning.
- The Duration stat now works: `SinDuration737 = clamp(1+dur, 0.1, sinDurationMax818)`. It scales Burn/Shock/Bleed/Curse status, Curse rune life and Grave pool time.
- Simplifications vs this doc: #37 body growth skipped; #40 is a fire ring on Hellfire Pillar (there is no Fire Wall skill); #41 uses Bleed as the poison stand-in; #42 has no taunt; #45 has no Asura/Animal spokes; #27 shatter was already baseline, so only the multiplier is added; #44's self-strike goes through HurtHero.
- HUD: a sins frame (3 code-drawn slots) sits above the 1-row belt. When an HD sin triggers, a glint runs around the frame (`sinGlintS818`/`sinGlintCd818`).
