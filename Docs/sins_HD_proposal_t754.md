# Heaven-Defying sins: PoE2-unique-pattern remake (t754u). PROPOSAL ONLY, not in v0.4, no code
VH t754u: "research POE 2 unique item, and remake these HD". The t752 file is kept for comparison. All numbers are tunables (tag 737), none play-tested. FPS rules from t738 apply.

## Sources (read 2026-11-04)
- Full PoE2 unique list with mod text: https://mobalytics.gg/poe-2/unique-items
- https://poe2db.tw/us/Atziris_Disdain · https://poe2dat.com/en/uniques/kaoms-heart · https://www.exile.pub/poe2/items/uniques
- poe2wiki.net returned 403 to the fetch tool, so it was not used.

## Part 1. What makes PoE2 uniques build-defining (≈35 uniques studied)
| # | Pattern | Real examples (item → mod → build it enables) |
|---|---|---|
| P1 | **Trigger / granted skill**: a free skill fires on an event | Dusk Vigil → "Trigger Ember Fusillade on casting a Spell" (spell-spam fire caster) · Earthbound → "Trigger Spark on killing a Shocked Enemy" (shock-clear chain) · Corpsewade → "Trigger Decompose every 1.2 metres travelled" (movement = damage) · Choir of the Storm → Lightning Bolt on crit |
| P2 | **Conversion**: damage/element/ailment changes type, so other scaling now applies | Blueflame Bracers → "100% of Fire Damage Converted to Cold" (fire skills with freeze scaling) · Valako's Vice → Fire→Lightning · Voltaxic Rift → Lightning→Chaos, "Chaos damage also contributes to Shock" · The Three Dragons → Fire shocks, Cold ignites, Lightning freezes |
| P3 | **"X also applies to Y" / "counts as"** rule-breaks | Trenchtimbre → "Minion Attack Speed modifiers also affect you" · Chober Chaber → "Minion Damage modifiers also affect you" (summoner-scaled melee) · Crown of Eyes → spell damage applies to attacks · Gravebind → "Enemies killed by anyone count as killed by you" |
| P4 | **Hard drawback as enabler**: "No X / Cannot X" pays for a huge payoff | Kaom's Heart → "+1500 Life, You have no Spirit" · Tyranny's Grip → Strikes splash, "Cannot use Projectile Attacks" · Quecholli → "Crit chance is 100%, Crits do not deal extra damage" · Veil of the Night → +50% Life, no Elemental Resistances |
| P5 | **Stat-per-stat / ramp scaling**: gain X per Y | Pillar of the Caged God → area per 10 Int, speed per 10 Dex, damage per 10 Str · Rathpith Globe → Spells +6% damage per 100 max Life (cost 6% Life) · Chernobog's Pillar → 1% damage as Fire per 1% Block · The Vile Knight → +4% damage per second the enemy has been in Presence (max 200%) |
| P6 | **Conditional state that breaks when hit/acting** | Dustbloom → "Gain 1 Fragile Regrowth each second, lose all when Hit" · Hyrri's Ire → Evasion doubled if not Hit Recently · Lioneye's Glare → attacks "Repeat +2 times if no enemies are in your Presence" |
| P7 | **Resource/cost swap** | The Burden of Shadows → "Skill Mana Costs Converted to Life Costs; +1% Chaos per 3 Life Cost" · Mask of the Sanguimancer → Blood Magic · Tangletongue → 10% Mana cost as Life |
| P8 | **Skill-gem/support rewrite**: changes how a skill or support behaves | Sire of Shards → "Spells fire 4 additional Projectiles in a circle" (any spell becomes a nova) · Slivertongue → "Arrows Fork; Arrows Pierce all targets after Forking" · Idol of Uldurn → "Skills have +1 to Limit" · Solus Ipse → 2 extra Lineage supports · Killjoy → "On-Kill Effects happen twice" |

**Takeaways for Hellord:**
- The best uniques combine 2 patterns: a rule-break plus a hard drawback (P2+P4 or P8+P4).
- Mod lines are short and literal, and the drawback is a plain line, not hidden.
- Most uniques name a specific mechanic (Spark, Fork, Minion Attack Speed), never just "+damage".

## Part 2. The 16 Heaven-Defying sins, remade
Mod-line style: `+` = boon, `−` = drawback, `◆` = rule change. All 12 skills are targeted, plus supports (Echo, Fork, Multi Projectiles, Chill/Shock/Burn, Explode on Kill, Life Leech, Knockback) and pacts (Overheat, Hellfire Lance, Last Rites, Blood Magic, Grave Muster, Thunder Step, keystones).

| # | Name | Mod lines | Pattern | NEW? | W |
|---|---|---|---|---|---|
| 35 | What the Jade Emperor Struck Out | ◆ Skeleton Archers fire your Fireball (with its supports) instead of arrows<br>◆ Minion Damage modifiers also apply to that Fireball<br>− You cannot cast Fireball yourself | P3 + P1 + P4 | yes | 50 |
| 36 | The Nineteenth Level | ◆ Echo repeats once only, 1.5s later at the same spot, for +80% damage<br>◆ Echo can support Minion skills (repeats their summon pulse)<br>− +30% cooldown on Echo-linked skills | P8 | yes | 50 |
| 37 | Tongue That Spoke Yama's True Name | ◆ Chain Lightning no longer chains; it becomes a beam on one demon<br>+ +20% damage per second on the same target (max +200%)<br>− Fork, Bounce and Static Fork have no effect on it | P8 + P5 + P4 | yes | 20 |
| 38 | Crown Refused at the Gate of Heaven | + Storm Ring and Bone Orbit gain 4% area per second while not hit (max +80%)<br>◆ At max, Bone Orbit skulls inflict Shock and Storm Ring inflicts Bleed<br>− Being hit resets it and stuns you for 0.4s | P6 + P2 | yes | 50 |
| 39 | Debt Unpaid to the Tenth King | + Per EMPTY sin slot: +25% damage and +1 projectile<br>− Cannot equip Mortal sins | P5 + P4 | no | 20 |
| 40 | The Soup Meng Po Would Not Pour | ◆ Last Rites resets each floor instead of each room; reviving blasts the screen<br>◆ Without Last Rites: one revive per run at 30% HP<br>− Lose 1% max HP per 10s alive (resets on revive) | P8(pact) + P4 | yes | 5 |
| 41 | Ten Thousand Ghost Banner (Vạn Hồn Phiên) | ◆ Kills store souls (max 20); Soul Spears fires one homing spear per stored soul<br>◆ Soul Harvest kills store 2 souls<br>− Soul Spears cooldown ×2.5; fires nothing with 0 souls | P1 + P5 + P4 | yes | 20 |
| 42 | Tongue of the Needle-Throat Ghost (Ngạ Quỷ) | ◆ Blood Whip pulls demons to you instead of pushing them (Knockback is reversed)<br>◆ Demons pulled below 15% HP are devoured: killed, and you heal 2% HP<br>− Whip range −40%; Life Leech has no effect on it | P2 + P3 + P4 | yes | 50 |
| 43 | Blade of the Eight Cold Hells (Bát Hàn) | ◆ Frost Cleave becomes a 360° spin<br>◆ Burn applied by your hits counts as Chill instead (fire builds freeze)<br>− 35% less damage; you cannot Burn enemies | P8 + P2 + P4 | yes | 50 |
| 44 | Golden Crow of the Sun (Kim Ô) | ◆ Fireball fires one slow sun that grows +10% per demon passed (max +150%)<br>◆ Each Multi Projectiles level adds +20% size instead of projectiles<br>− Fireball cooldown ×2 | P8 + P3 | yes | 20 |
| 45 | Lamp of the Nine Nether Springs (Cửu U) | ◆ Cursed demons rise as your minions for 8s (max 10) and cast a small Curse Rune at 40%<br>◆ Curse damage modifiers also apply to Minions<br>− Curse Rune area −40%; Curse Rune cannot be Echoed | P1 + P3 + P4 | yes | 20 |
| 46 | Fire That Burns Heaven (Phần Thiên) | ◆ Hellfire Pillar becomes a marching wall: 5 pillars walk toward your aim and leave burning ground (max 5 patches)<br>◆ Overheat heat also raises Pillar damage (+1% per heat)<br>− No longer targets clusters; Hellfire Lance is disabled | P8 + P5(pact) + P4 | yes | 50 |
| 47 | River of Forgetting (Vong Xuyên) | ◆ Grave Pool becomes one river along your dash path (8s)<br>◆ Demons inside forget you: no attacks for 1s, every 3s<br>− Grave Pool −30% damage; only one river at a time | P1 + P8 | yes | 50 |
| 48 | Ox-Head and Horse-Face Answer the Call | ◆ Skeleton Warriors are replaced by 2 guardians (6× HP, taunt, cleave)<br>◆ Each Grave Muster or Legion count you would gain = +25% guardian damage instead<br>− Max 2 Skeleton Warriors | P8 + P3 + P4 | yes | 20 |
| 49 | Iron Contract of Yama's Court | ◆ You may seal 2 keystones from different pact paths<br>− Non-keystone pact nodes give 50% effect<br>− Boss HP +20% | P4 (keystone-like) | yes | 20 |
| 50 | Heavenly Tribulation (Thiên Kiếp) | ◆ Storm Ring stops ticking and charges; after 8s, lightning strikes the whole ring for 500%<br>◆ Thunder Step / Storm Capacitor dash triggers the strike early<br>− The strike hits you for 15% max HP unless you dash within 1s of the warning | P6 + P1 + P4 | yes | 50 |

- Removed from t752: Blood Magic-style HP casting (duplicates the existing keystone) and dash-hop (overlaps Thunder Step).
- [NEW] count: 15 of 16 (all except #39).
- **FPS (t738):**
  - #36 allows 1 repeat.
  - #45 max 10 raised minions; Curse cannot be Echoed.
  - #41 max 20 souls, which are pooled projectiles.
  - #46 max 5 burn patches.
  - #50 one strike per 8s.
  - #35 fireballs count toward the existing projectile budget.

## Tunables (tag 737), replacing the sin35–50 fields in t750/t752
- `sin35ArcherDmg`=0.5
- `sin36Delay`=1.5, `sin36Repeats`=1, `sin36DmgMul`=1.8, `sin36CdMul`=1.3, `sin36AllowMinion`=true
- `sin37RampPerSec`=0.2, `sin37RampMax`=2.0
- `sin38AreaPerSec`=0.04, `sin38Max`=0.8, `sin38Stun`=0.4
- `sin39PerEmptyDmg`=0.25, `sin39PerEmptyProj`=1
- `sin40ReviveHp`=0.5, `sin40NoNodeHp`=0.3, `sin40DecayPer10s`=0.01
- `sin41MaxSouls`=20, `sin41HarvestSouls`=2, `sin41CdMul`=2.5
- `sin42Pull`=3.0, `sin42DevourHp`=0.15, `sin42Heal`=0.02, `sin42RangeMul`=0.6
- `sin43DmgMul`=0.65
- `sin44GrowPerHit`=0.1, `sin44GrowMax`=1.5, `sin44SizePerMulti`=0.2, `sin44CdMul`=2.0
- `sin45RaiseSec`=8, `sin45MaxRaised`=10, `sin45RuneDmg`=0.4, `sin45AreaMul`=0.6
- `sin46Pillars`=5, `sin46StepSec`=0.25, `sin46PatchMax`=5, `sin46DmgPerHeat`=0.01
- `sin47RiverSec`=8, `sin47ForgetSec`=1, `sin47ForgetEvery`=3, `sin47DmgMul`=0.7
- `sin48HpMul`=6, `sin48Count`=2, `sin48DmgPerCount`=0.25
- `sin49Keystones`=2, `sin49NodeMul`=0.5, `sin49BossHp`=1.2
- `sin50Charge`=8, `sin50DmgMul`=5.0, `sin50SelfDmg`=0.15, `sin50Warn`=1.0

## Questions for VH
1. Is the PoE2-style mod wording right for the in-game Sin card?
2. Is #49 (two keystones) OK?
3. Is it OK for #43 to remove your ability to Burn, which kills any fire build?
