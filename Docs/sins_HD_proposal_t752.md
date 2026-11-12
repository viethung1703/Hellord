# Heaven-Defying sins: build-linked redesign proposal (t752u). PROPOSAL ONLY, not in v0.4 yet, no code
VH t752u: Heaven-Defying (HD) sins must tie to the build (skills, supports, pacts). Some should transform a skill into a new version of itself, like PoE2 uniques that change a gem.
Covers all 16 HD slots (#35–50). The original 6 were reviewed against the same rule: 2 kept, 4 rewritten.

## Real names found in code
- **Skills (HellordData.cs, 12):** Fireball, Frost Cleave, Storm Ring, Bone Orbit, Curse Rune, Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool, Skeleton Warriors, Skeleton Archers.
- **Supports (19):** Multi Projectiles, Pierce, Fork, Bounce, Homing, Bigger Area, Echo, More Orbitals, Burn, Chill, Shock, Bleed, Added Damage, Faster Casting, Executioner, Life Leech, Knockback, Soul Harvest, Explode on Kill.
- **Pact paths (HellordPact.cs):** Pyromancer, Necromancer, Stormcaller, Blood Reaver.
  - **Pyromancer:** Kindling, Cinder Split, Scorched Trail, Overheat, Meteor Pact, Ember Orbit, Hellfire Lance, Pyre Feast; keystones Ash Covenant, Inferno Heart.
  - **Necromancer, minion nodes:** Grave Muster, Iron Marrow, Hollow Haste, Quick Grave, Grave Edge, Marrow Shot, Ossuary Order; keystones Crypt King, Legion of One Hundred.
  - **Necromancer, bone nodes:** Marrow Bite, Splintering, Brittle Mark, Dry Ossuary, Grave Reach, Calcify, Marrow Harvest; keystones Ossuary Tyrant, Grave Colossus.
  - **Stormcaller:** Conduit, Storm Capacitor, Cast on Shock, Static Fork, Overcharge, Lightning Rod, Cast on Kill, Thunder Step; keystones Unbound Chains, Elemental Equilibrium.
  - **Blood Reaver:** Leech Convert, Price of Wrath, Whip Reach, Hemorrhage, Red Feast, Frenzy Chain, Sanguine Spears, Last Rites; keystones Martyr, Blood Magic.
- **Stats:** Vitality, Swiftness, Hunger, Hide, Malice.
- **Duplicates to avoid:**
  - The t750u #42 (HP-cost casting) duplicates the Blood Magic keystone, so it is dropped.
  - The t750u #48 (dash hop) overlaps Thunder Step, so it is dropped.

## Review of the original 6
- **#35 Jade Emperor (any minion casts any skill):** too generic. Rewritten to target Skeleton Archers + Fireball.
- **#36 Nineteenth Level (global echo, −35% HP):** generic, and a t738 FPS risk. Rewritten as a transform of the Echo support.
- **#37 Yama's True Name (all hits apply all statuses):** generic. Rewritten as a Chain Lightning transform.
- **#38 Crown Refused (Storm Ring/Bone Orbit grow while unhit):** already skill-specific. KEEP.
- **#39 Debt Unpaid (empty sin slots = power):** a Sins-layer build hook, not generic. KEEP.
- **#40 Meng Po soup (revive):** generic. Re-tied to the Last Rites pact node.

## Proposal (16)
| # | Name | Targets | New behaviour | Trade-off | NEW? | W |
|---|---|---|---|---|---|---|
| 35 | What the Jade Emperor Struck Out | Skeleton Archers + Fireball | Archers fire your Fireball, with its supports, at 50% damage instead of arrows | You no longer cast Fireball yourself; Archer count −1 | yes | 50 |
| 36 | The Nineteenth Level | Echo (support) | Echo becomes Delayed Echo: 1 repeat only (all skills), 1.5s later at the same spot, +80% damage | Skill linked to Echo: cooldown +30%. FPS-safe: 1 repeat, t738 caps kept | yes | 50 |
| 37 | Tongue That Spoke Yama's True Name | Chain Lightning | Becomes Judgment Chain: no jumps, locks on one demon as a beam, damage ramps +20%/s (max +200%), applies Burn/Chill/Shock/Bleed | Single target only; Fork/Bounce/Static Fork do nothing on it | yes | 20 |
| 38 | Crown Refused at the Gate of Heaven (KEEP) | Storm Ring + Bone Orbit | +4% area per second without being hit (max +80%) | A hit resets it and stuns you 0.4s | yes | 50 |
| 39 | Debt Unpaid to the Tenth King (KEEP) | Sin slots | Each empty sin slot: +25% damage and +1 projectile | Can't equip Mortal sins with it | no | 20 |
| 40 | The Soup Meng Po Would Not Pour | Last Rites (Blood Reaver pact) | Last Rites works once per FLOOR (not room) and the revive blasts the screen; without the node, gives a 1×/run revive at 30% | Max HP −1% per 10s alive (resets on revive) | yes | 5 |
| 41 | Ten Thousand Ghost Banner (Vạn Hồn Phiên) | Soul Spears + Soul Harvest | Kills store souls on a banner (max 20). Soul Spears instead launches ALL stored souls as homing spears in a volley | Cooldown ×2.5; nothing fires with 0 souls | yes | 20 |
| 42 | Tongue of the Needle-Throat Ghost (Ngạ Quỷ) | Blood Whip | Whip becomes a grabbing tongue: pulls demons to you; demons under 15% HP are devoured (instant kill + 2% HP heal) | Range −40%; Knockback support disabled on it | yes | 50 |
| 43 | Blade of the Eight Cold Hells (Bát Hàn) | Frost Cleave + Chill | Cleave becomes a 360° spin; 1 chill = freeze; frozen demons shatter on death (small Cold area) | Damage −35%; frozen demons take −50% Fire damage | yes | 50 |
| 44 | Golden Crow of the Sun (Kim Ô) | Fireball + Multi Projectiles | Fireball becomes one slow sun that grows +10% per demon it passes (max +150%) and explodes at range end; each Multi Projectiles level = +20% size instead of more shots | Always 1 projectile; cooldown +100% | yes | 20 |
| 45 | Lamp of the Nine Nether Springs (Cửu U) | Curse Rune | Cursed demons rise as your minions for 8s (max 10) and cast a small Curse Rune at 40% | Curse Rune area −40%; can't link Echo to Curse Rune | yes | 20 |
| 46 | Fire That Burns Heaven (Phần Thiên) | Hellfire Pillar (+ Hellfire Lance pact) | Pillars become a marching wall: 5 pillars walk from you toward the aim point and leave burning ground | No longer auto-targets clusters; Hellfire Lance node disabled while equipped | yes | 50 |
| 47 | River of Forgetting (Vong Xuyên) | Grave Pool + dash | Pools merge into one river along your dash path (8s); demons inside forget you (no attacks for 1s, every 3s) | Grave Pool −30% damage; only 1 river at a time | yes | 50 |
| 48 | Ox-Head and Horse-Face Answer the Call | Skeleton Warriors | Warriors are replaced by 2 giant guardians, Ox-Head and Horse-Face: taunt, 6× HP, cleave | Max 2 minions from this skill; Grave Muster and Legion of One Hundred give no extra count to them | yes | 20 |
| 49 | Iron Contract of Yama's Court | Pact keystones | You may seal 2 keystones from different paths in one run | All non-keystone pact nodes give half effect; boss HP +20% | yes | 20 |
| 50 | Heavenly Tribulation (Thiên Kiếp) | Storm Ring + Stormcaller | Storm Ring stops ticking and charges instead; at full (8s), tribulation lightning strikes inside the ring for 500% (Storm Capacitor: dash triggers it early) | The strike also hits you for 15% max HP unless you dash inside 1s of the warning | yes | 50 |

- **Coverage:**
  - All 12 skills are targeted.
  - Supports targeted: Echo, Multi Projectiles, Chill, Soul Harvest, Knockback; Fork/Bounce indirectly.
  - Pacts targeted: Last Rites, Hellfire Lance, keystones, Stormcaller/Storm Capacitor, Grave Muster/Legion.
- **NEW mechanics:** 15 of 16 (all except #39). Expect most of this work in Sins phase 3.
- **FPS (t738):** #36 caps at 1 repeat. #45 max 10 raised. #41 max 20 souls. #46 caps the burn ground at 5 patches. #50 is one strike every 8s.

## Tunables (tag 737), new or changed
- `sin35ArcherDmg`=0.5, `sin35ArcherCountDelta`=-1
- `sin36Delay`=1.5, `sin36Repeats`=1, `sin36DmgMul`=1.8, `sin36CdMul`=1.3
- `sin37RampPerSec`=0.2, `sin37RampMax`=2.0
- `sin38AreaPerSec`=0.04, `sin38Max`=0.8, `sin38Stun`=0.4
- `sin40ReviveHp`=0.5, `sin40NoNodeHp`=0.3, `sin40DecayPer10s`=0.01
- `sin41MaxSouls`=20, `sin41CdMul`=2.5
- `sin42Pull`=3.0, `sin42DevourHp`=0.15, `sin42Heal`=0.02, `sin42RangeMul`=0.6
- `sin43DmgMul`=0.65, `sin43FireTakenMul`=0.5, `sin43ShatterR`=1.2
- `sin44GrowPerHit`=0.1, `sin44GrowMax`=1.5, `sin44SizePerMulti`=0.2, `sin44CdMul`=2.0
- `sin45RaiseSec`=8, `sin45MaxRaised`=10, `sin45RuneDmg`=0.4, `sin45AreaMul`=0.6
- `sin46Pillars`=5, `sin46StepSec`=0.25, `sin46BurnPatchMax`=5
- `sin47RiverSec`=8, `sin47ForgetSec`=1, `sin47ForgetEvery`=3, `sin47DmgMul`=0.7
- `sin48HpMul`=6, `sin48Count`=2
- `sin49Keystones`=2, `sin49NodeMul`=0.5, `sin49BossHp`=1.2
- `sin50Charge`=8, `sin50DmgMul`=5.0, `sin50SelfDmg`=0.15, `sin50Warn`=1.0
- Replaces the t750u `sin41*`–`sin50*` fields and the old `sin35*`–`sin37*`/`sin40*` fields.

## Questions for VH
1. Is the rewrite of the original #35, #36, #37 and #40 OK? Keep #38 and #39?
2. Should #49 (two keystones) be allowed? It is the strongest pact interaction.
3. For #48, is it OK to use Ox-Head and Horse-Face (also boss names) as your minions?
