# PoE / PoE2 minion AI research for Hellord (t591u/t592u, 01 Nov 2026 UTC+7)

GGG has never published exact AI numbers (aggro ranges and so on). All of the findings below come from wikis, community testing and forum posts. The numbers in the Hellord column are our own picks.

## Findings (cited)
1. **Minions have an aggro range and act on their own inside it.** Skeletons ignore the player's actions and aggressively attack enemies within their aggro range, which is "much larger" than that of Raging Spirits. Source: PoE Wiki, *Minion › Minion Targeting* (https://pathofexile.fandom.com/wiki/Minion).
2. **Aggressive vs Defensive.** Aggressive minions attack anything in aggro range and do NOT move with the player. Defensive minions stay close and only attack enemies near the player. Source: PoE Wiki, *Minion › Aggressive and Defensive*.
3. **Spread targets.** Raging Spirits first try to attack targets that other Raging Spirits are not already attacking. Source: PoE Wiki, *Minion*.
4. **Focus on the player's target / threat.** Attacking an enemy with a skill makes minions prioritise that target. Signal Prey forces focus fire on one enemy. Source: PoE Wiki, *Minion* (Signal Prey).
5. **PoE2: each minion type has its own aggression range and skill logic, and some have aggression options.** You can manually direct minions to move or attack. Source: PoE2 mechanics wiki mirror, Myrtana (https://www.myrtana.net/en/path-of-exile-2/wiki/mechaniky/minions). The PoE2 skeleton line has an explicitly "aggressive" Reaver, a "long-ranged" Sniper and a frontline Warrior. Source: PoE2DB *Minions* (https://poe2db.tw/Minions), PoE2Path minion guide (https://poe2path.com/guides/minion-army-build/).
6. **Players want minions ahead of them, engaging first.** With click-to-move, minions "move ahead of me … engaging with mobs screens away". With WASD they lag behind and players feel they have to babysit them. Source: r/PathOfExile2 "Minion AI with Click to move VS WASD" (https://www.reddit.com/r/PathOfExile2/comments/1i16n40/). Direct Minions is used to wall enemies off and to make minions "aggro mobs before you". Source: Steam PoE2 discussion "Tips for Minion players".
7. **Leash and stuck problems.** In PoE2, minions die (and revive) when they get too far away. Players complain they get stuck behind bridges and doors and ask for automatic teleport-to-player and an "unstuck" button. Source: PoE forum "Need Convocation for Minions Please!" (https://www.pathofexile.com/forum/view-thread/3656860). PoE1 Convocation recalls following minions by teleport. Source: PoE Wiki *Convocation*.
8. **After a move order, minions stop fighting for 1–2 s.** The community treats this as bad, because the minions get beaten up while idle. Source: Steam "Tips for Minion players".

## What Hellord COPIES
- Skeletons use the **aggressive** profile: they leave the player and fight anything within a large seek radius (#1, #2, #6).
- **Target priority:** the demon currently hitting the hero, then demons in the hero's face, then the nearest demon inside the seek radius (#4).
- **Spread:** warriors avoid a target that already has 3 or more warriors on it, and use flank slots around their target (#3).
- Ranged skeletons fight at long range; melee skeletons hold the line (#5).
- **Never idle while being hit:** state is re-evaluated every tick, with no post-move freeze (#8).

## What Hellord ADAPTS (top-down survivor, WASD, no navmesh)
- There are no commands (no Direct Minions). Instead of forcing focus with a command, the hero's attacker gets top priority.
- **Leash:** we teleport instead of killing the minion (this is the fix players asked for in #7). Teleport at 10m (`minionTeleport561`). The seek radius is 8.5m around the hero, so a fight never drags a minion past the teleport distance.
- **Idle:** with no enemy, minions ROAM a 4–6m ring around the hero, wandering to a new point every 1.5–3.5s at slow speed, instead of hugging the hero. This replaces PoE's "follow" because WASD otherwise leaves them behind (#6).
- **Unstuck:** if a minion makes no progress for 0.8s it picks a new point and drops the target. If it is still stuck at 2s, it blinks to the ring (#7).
- **Archers:** they shoot from up to 8m, stop at about 5m when closing in, back off or strafe when a demon gets within 2.5m, and shoot while moving.
