# Heaven-Defying sins, revision t755u/t756u. PROPOSAL ONLY: no code, v0.4 not edited
Applies VH's t755u feedback on t754 and the t756u #41 clarification. The older proposals are kept. All numbers are tunables (tag 737), none play-tested. FPS caps from t738 apply.
Style rules: one clear rule change + one clear drawback, tied to a real skill/support/pact, simple to build.

## A. The 16 Heaven-Defying sins
| # | Name | Mod lines | Targets | Status | W |
|---|---|---|---|---|---|
| 35 | Ghost Lantern Procession (Rước Đèn Ma) | ◆ Bone Orbit skulls fly out toward your aim and come back, like boomerangs, hitting both ways<br>− −2 skulls; skulls no longer guard you | Bone Orbit, More Orbitals | NEW | 50 |
| 36 | Firecracker of the Ghost Festival (Pháo Rằm Tháng Bảy) | ◆ Explode on Kill explosions can kill and explode again (1 extra chain step)<br>− That skill −30% damage | Explode on Kill | NEW | 50 |
| 37 | Endless Whip of the Thunder Lord (Lôi Công) | ◆ Chain Lightning jumps without limit (safety cap 30), −15% damage per jump, never hits the same demon twice<br>− Chain Lightning cooldown +60% | Chain Lightning | NEW | 50 |
| 38 | Pearl of the Dragon King (Long Vương Châu) | ◆ Storm Ring is centred on your mouse cursor instead of on you<br>− Storm Ring radius −25% | Storm Ring | NEW | 50 |
| 39 | Cup of the Blood Pool (Huyết Trì) | ◆ Life Leech has no per-second cap<br>− You cannot regenerate and heal pickups do nothing | Life Leech, Blood Whip | NEW | 20 |
| 40 | Meng Po's Untouched Bowl | ◆ Once per run, when you would die, you revive at full HP<br>− Max HP −20% until it has been used | Survival | NEW (simple) | 20 |
| 41 | Ten Thousand Ghost Banner (Vạn Hồn Phiên) | + Kills store souls on the banner (max 20; Soul Harvest kills store 2)<br>◆ At max, all souls are released as free-flying ghosts that roam the whole room, seek demons and fly through them (pierce). They persist 45s, then storing restarts<br>− Soul Spears −30% damage | Soul Spears, Soul Harvest | REVISED (t756u) | 20 |
| 42 | Tongue of the Needle-Throat Ghost (Ngạ Quỷ) | ◆ Blood Whip pulls demons in; demons under 15% HP are devoured (kill + 2% HP heal)<br>+ Each devour: +1 Gluttony stack: +8% body size and +8% skill area (max 10)<br>− −4% move speed per stack; stacks reset each floor | Blood Whip | REVISED | 50 |
| 43 | Blade of the Eight Cold Hells (Bát Hàn) | ◆ Frost Cleave becomes a 360° spin<br>◆ Your Burn becomes Freeze buildup (§B)<br>− 35% less damage; you cannot Burn | Frost Cleave, Chill, Burn | REVISED (needs §B) | 50 |
| 44 | Golden Crow of the Sun (Kim Ô) | ◆ Fireball fires one slow sun that grows +10% per demon passed (max +150%)<br>◆ Multi Projectiles adds +20% size per level instead of projectiles<br>− Fireball cooldown ×2 | Fireball, Multi Projectiles | KEPT | 20 |
| 45 | Lamp of the Nine Nether Springs (Cửu U) | ◆ Cursed demons rise as your minions for 8s (max 10) and cast a small Curse Rune at 40%<br>◆ Curse damage modifiers also apply to Minions<br>− Curse Rune area −40%; can't be Echoed | Curse Rune | KEPT | 20 |
| 46 | Ring of the Burning City (Hỏa Thành) | ◆ Fire Wall becomes a closed ring of fire around you<br>− Can't be aimed; −30% damage | Fire Wall (**DEPENDS on §C option A**) | NEW | 50 |
| 47 | Whirlpool of the Naihe River (Nại Hà) | ◆ Grave Pool sucks demons toward its centre<br>− Grave Pool no longer slows; −20% damage | Grave Pool | NEW | 50 |
| 48 | Ox-Head and Horse-Face Answer the Call | ◆ Skeleton Warriors are replaced by 2 guardians (6× HP, taunt, cleave)<br>+ Guardians grow +15% size per skill level<br>◆ Extra minion count from Grave Muster / Legion of One Hundred = +25% guardian damage instead<br>− Max 2 Skeleton Warriors | Skeleton Warriors, Necromancer pacts | REVISED | 20 |
| 49 | Furnace of Taishang Laojun (Bát Quái Lô) | ◆ Overheat never vents: Fire damage +60% while above 100 heat<br>− Above 100 heat you burn for 2% max HP per second | Overheat (Pyromancer pact) | NEW (simple) | 50 |
| 50 | Heavenly Tribulation (Thiên Kiếp) | ◆ Every Dash calls a lightning bolt where you land<br>− Dash cooldown +50% | Dash, Thunder Step | NEW (one line) | 50 |

Notes:
- #35 avoids skeleton casters. #37 avoids a beam. #39 doesn't scale with sin slots.
- #40 in plain words: one extra life per run, but you're weaker until you use it.
- **FPS:**
  - #41 is at most 20 pooled ghosts on screen; they can't be stored again while the swarm is out.
  - #36 chains only 1 extra step, inside the t738 limit of 4 death-explosion visuals per frame.
  - #37 has a 30-jump cap.
  - #42 caps body size at +80%.

## B. Freeze mechanic proposal (player side, PoE2-style)
- The code already has a chill-count freeze (Frost Cleave "3 chills = freeze"; Chill Lv4 freezes in 2). VH can't see it in play, so this replaces it with a visible system.
- **Buildup:** every Cold hit adds buildup = damage ÷ (demon max HP × `freezeThresholdMul737`). A bar shows under demons, and it decays after 2s without cold hits.
- **Frozen:** at 100%, the demon is frozen solid for 1.5s (no move or attack, ice tint) and takes +25% damage.
- **Shatter:** killing a frozen demon causes a small Cold burst (40% of the hit, 1u) and leaves no corpse. Shatter visuals obey the t738 per-frame cap.
- **Bosses:** buildup ×0.3, freeze lasts half as long, and each freeze raises that boss's threshold +50%.
- **Hooks:** Chill support and Frost Cleave add buildup. #43 turns Burn into buildup.

## C. Hellfire Pillar replacement
- **A. Fire Wall (Hỏa Tường), recommended**
  - Places a wall of fire across your aim direction. Demons crossing it Burn.
  - Bigger Area = longer. Echo = a second wall behind. Burn support = stronger. Hellfire Lance pact = the wall marches forward.
- **B. Fire Dragon (Hỏa Long)**
  - A fire serpent weaves toward the aim point and leaves a burning trail.
  - Has the Projectile tag, so Pierce/Fork/Multi work (Multi = more dragons).
- **C. Heaven's Fire Rain (Thiên Hỏa)**
  - After a warning, 3–5 meteors fall in the aim area.
  - Bigger Area = wider spread. Echo = a second rain. Overlaps the Meteor Pact node.

## Tunables (tag 737), replacing earlier sin35–50 fields
- `sin35SkullDelta`=-2, `sin35Range`=6
- `sin36ChainSteps`=1, `sin36DmgMul`=0.7
- `sin37JumpCap`=30, `sin37JumpFalloff`=0.15, `sin37CdMul`=1.6
- `sin38RadiusMul`=0.75
- `sin40HpMulBefore`=0.8
- `sin41MaxSouls`=20, `sin41HarvestSouls`=2, `sin41SwarmSec`=45, `sin41GhostSpeed`=5, `sin41GhostDmgFrac`=0.3, `sin41HitCdPerDemon`=0.5, `sin41SpearMul`=0.7
- `sin42DevourHp`=0.15, `sin42Heal`=0.02, `sin42SizePerStack`=0.08, `sin42AreaPerStack`=0.08, `sin42MaxStacks`=10, `sin42SpeedPerStack`=0.04, `sin42ResetPerFloor`=true
- `sin43DmgMul`=0.65
- `freezeThresholdMul737`=1.0, `freezeSec737`=1.5, `freezeTaken737`=0.25, `freezeDecaySec737`=2, `freezeBossMul737`=0.3, `freezeBossDimin737`=0.5, `shatterFrac737`=0.4
- `sin44*` / `sin45*` as in t754
- `sin46DmgMul`=0.7
- `sin47PullForce`=2.5, `sin47DmgMul`=0.8
- `sin48HpMul`=6, `sin48SizePerLvl`=0.15, `sin48DmgPerCount`=0.25
- `sin49FireMul`=1.6, `sin49SelfBurn`=0.02
- `sin50BoltDmgMul`=2.0, `sin50DashCdMul`=1.5

## Questions for VH
1. Which Pillar replacement (A/B/C)?
2. Should Freeze replace the current chill-count freeze?
3. Is the #42 reset per floor OK?
4. Is a #41 swarm length of 45s OK (range 30–60)?
