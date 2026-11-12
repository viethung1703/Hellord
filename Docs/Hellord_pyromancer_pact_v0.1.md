# Hellord — Pyromancer of the Pit Pact v0.1 (t541u, DESIGN ONLY, awaiting VH approval)
Template = Necromancer: 18 nodes (Group A BURN 9, Group B FIRE SKILLS 9), each group 7 Notables + 2 Keystones.
Pick max 8 per run, max 1 Keystone. Unlock (Souls; Keystone 150 Souls + 1 Sigil) then select. Crest at 4+ selected.
Rule: no node names a specific skill. "Fire skill" = any skill with the Fire tag. "Burn" = the Burn status from any source.
Old 10-node Pyromancer (Kindling, Cinder Split, Overheat, Meteor Pact, Ember Orbit, Pyre Feast, Ash Covenant, Inferno Heart...) used as inspiration only.

## Group A — BURN (the status)
| Id | Name | Souls | Req | Effect |
|---|---|---|---|---|
| A1 | KINDLING | 35 | – | Fire hits always Burn. Burn lasts +1s |
| A2 | SEARING BRAND | 40 | A1 | Burn deals +30% damage |
| A3 | BANKED COALS | 40 | A1 | Burn stacks up to 3 times (each stack full damage, own timer) |
| A4 | CINDER SPLIT | 60 | A2/A3 | Burning enemies that die spread their Burn to 2 enemies within 3m |
| A5 | PYRE FEAST | 50 | A2 | Burn kills heal 1% max HP (max 5% per second) |
| A6 | EMBERFALL | 60 | A3 | Burn kills release 2 embers that seek nearby enemies (30% of the Burn's DPS as fire hit; embers can Burn) |
| A7 | SCORCHED EARTH | 80 | A5/A6 | Burn kills leave burning ground (2m, 3s) that Burns enemies standing in it |
| AK | INFERNO HEART | 150 + 1 Sigil | A4 | You are always burning: 3m aura Burns enemies; enemies that hit you are Burned. You take 1% max HP/s fire damage and all healing −50% |
| AK2 | ASH COVENANT | 150 + 1 Sigil | 2A | All your damage counts as fire and can Burn; Burn +40% damage. You can no longer Chill or Freeze (cold effects are lost) |

## Group B — FIRE SKILLS (any skill with the Fire tag)
| Id | Name | Souls | Req | Effect |
|---|---|---|---|---|
| B1 | HELLBORN FLAME | 35 | – | Fire skills +20% damage |
| B2 | WILDFIRE | 40 | B1 | Fire skills +25% area, projectile size and beam width |
| B3 | FURNACE STEP | 40 | B1 | After you dash, your next fire skill cast within 2s deals +40% damage |
| B4 | KILN | 60 | B2/B3 | Fire skills −20% cooldown |
| B5 | FLASHPOINT | 50 | B2 | Fire hits against Burning enemies deal +25% damage |
| B6 | CATACLYSM | 60 | B3 | Every 6th fire skill cast is Empowered: +100% damage, +50% area |
| B7 | MELTDOWN | 70 | B4 | Fire hits Melt the enemy: +5% damage taken from fire per stack, max 5 stacks, 4s |
| BK | OVERHEAT | 150 + 1 Sigil | B4 | Fire skills have no cooldown. Each fire cast builds Heat (scaled by its old cooldown). At 100 Heat: fire nova around you, then fire skills are locked for 3s |
| BK2 | SOLAR HUBRIS | 150 + 1 Sigil | 2B | Fire skills +60% damage and +30% area. Non-fire skills −50% damage |

Req notation: "A2/A3" = either; "2A" = any 2 Group A Notables selected; Keystones need one in their own group.

## Crest (4+ selected): PYRE OF THE PIT
"CREST: BURNING DEATHS EXPLODE (2M FIRE BURST), FIRE DMG +15%"
- Any Burning enemy that dies bursts for fire damage = 3x its Burn DPS in 2m; the burst can Burn.

## Path flavor
Pyromancer of the Pit — the pit-fire made flesh. Group A makes everything burn longer and wider; Group B makes every fire skill hit harder. Old per-node "+4% inc dmg" passive is DROPPED (Necromancer has no per-node stat either) unless VH wants it back.

## Open
- Keep the per-node +inc damage passive? (default: drop, like Necromancer)
- Numbers are first-pass; tune after apply.
