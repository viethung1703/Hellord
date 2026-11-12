# Hellord — Skill Evolution & Buildcraft Design v0.1 (DESIGN ONLY, for VH approval)

Current pieces: 10 skills (Fireball, Frost Cleave, Storm Ring, Bone Orbit, Curse Rune, Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool), 19 supports (Multi Proj, Pierce, Fork, Bounce, Homing, Bigger Area, Echo, More Orbitals, Burn, Chill, Shock, Bleed, Added Damage, Faster Casting, Executioner, Life Leech, Knockback, Soul Harvest, Explode On Kill), tags (Projectile, Area, Orbital, Hit, Melee, Aura, Zone), skill Lv1–5, support Lv1–5, hero traits, level-up cards.

## 1) Why skill + support feels flat
- **Supports are additive stat sticks.** +1 projectile, +25% area, +dmg. Every skill gets the same kind of bump, so a Fireball build and a Soul Spears build feel the same: more numbers.
- **No interactions between supports.** Burn and Explode On Kill don't know about each other. In PoE the fun is "A turns into B, which triggers C". Here A, B and C are just stacked.
- **No identity change.** Lv5 Fireball is Lv1 Fireball with more balls. The skill never becomes something new.
- **Choices are low-stakes.** Every card is a small upgrade with no trade-off, so there is no "this run I went crazy on X".
- **Scaling is linear.** Level arrays (1,2,3,4,6) plus support levels are easy to predict. There is no multiplicative jump to chase.

## 2) Evolution mechanic options

### A) Evolve Stages with Branches (recommended core)
- A skill at **Lv3** offers a one-time **Branch**: pick 1 of 2 (sometimes 3) evolutions. It changes how the skill behaves, not only its numbers.
- At **Lv5 + a matching support** the skill gets an **Ascension** (VS-style evolve) that is unique to the branch.
- Example, Fireball:
  - Lv3 Branch: **Meteor** (slow, huge AoE crater that leaves burning ground) or **Hellshot** (fast piercing bolt that splits on hit).
  - Lv5 Ascension: Meteor + Burn → **Rain of Ruin** (meteors fall on every burning enemy). Hellshot + Fork → **Cinder Web** (splits chain endlessly between burning targets).
- Example, Bone Orbit:
  - Lv3 Branch: **Bone Storm** (orbit widens and pulses outward) or **Ossuary** (bones fire off as spears at nearest enemies, then regrow).
  - Lv5 Ascension: + Soul Harvest → **Reaper Halo** (each kill adds a bone, up to 3× base).
- 10 skills × 2 branches × ~2 ascensions = ~40 distinct endpoints. Readable: one big moment per skill, shown on a card.

### B) Tag Synergy / Resonance (the "PoE links" layer)
- Every skill and support has **elements and tags**: Fire / Frost / Storm / Blood / Bone / Soul, plus Projectile / Area / etc.
- **Resonance:** having 3+ pieces of the same element in the build turns on a passive rule:
  - Fire 3: Burning enemies explode on death.
  - Fire 5: Explosions apply Burn.
  - Frost 3: Frozen enemies shatter into shards (projectiles that trigger your on-hit supports).
  - Storm 3: Every 10th hit calls lightning on a random target.
  - Blood 3: Overheal becomes a blood shield. At 5, the shield damages nearby enemies.
  - Bone 3: Kills drop bone shards that orbit you briefly.
- **Cross-element combos** (2+2 pairs) give named rules, e.g. Fire+Frost = **Thermal Shock** (hitting a chilled target with fire deals ×2 and removes chill).
- **Support "triggers":** some supports become conditional triggers, e.g. Echo = "when this skill kills, cast it again from the corpse". These are the multiplicative chains.

### C) Corruption / Risk Evolve (spice, later floors)
- At **Shrine / Curse rooms** or after a boss, offer to **Corrupt** a skill: a random outcome from a table shown up front.
  - 50% **Big up:** double projectiles, or an extra branch effect from the other branch.
  - 35% **Twist:** the skill changes element (e.g. Fireball becomes Frost, so it counts toward Frost resonance).
  - 15% **Brick:** −30% damage, but the skill gains a hidden-luck stat for the next corrupt.
- One corrupt per skill. Optional. This is the "unpredictable" knob.

### Not recommended now
- **Weapon / skill merge** (fusing two skills into one): high art cost and balance risk. Maybe later as a single "Forbidden Fusion" boss reward.
- **Free-form socket grids / passive tree:** too much menu for a survivor game.

## 3) Many builds, unpredictable power, no boring early game
- **Early (floor 1):** only Lv1–2 skills and simple supports. The first Branch card appears around the 3rd–4th level-up, so the first evolve lands about 3–5 minutes in. The early game stays clean, with a guaranteed "wow" moment.
- **Variety comes from:** branch (2) × ascension (2) × resonance element mix (6) × corruption roll. Two Fireball runs can end as Rain of Ruin + Fire 5, or Frost-corrupted Hellshot + Thermal Shock.
- **Unpredictability:**
  - Branch and ascension cards show up on level-up only when the prerequisites are met, so what you see depends on the run.
  - Resonance thresholds create power spikes rather than linear growth.
  - Corruption adds gamble moments.
- **Readable:** each evolution is ONE named card with a 1-line effect and a small preview GIF/icon. Resonance shows as element pips on the HUD (e.g. Fire ●●●○○).
- **Power curve:** supports stay modest. Branch gives about ×1.5 effective, Ascension about ×2–3, Resonance 3/5 about ×1.3 / ×1.8, and triggers multiply. Late floors reach "screen-clear" builds, while floor 1 stays near baseline.

## 4) Tie to the floor ladder and Intensity
| Floor | Unlocks | Feel |
|---|---|---|
| 1 | Skill Lv1–3, first Branch at Lv3, Resonance tier 3 visible but rarely reached | Simple, learn the skill |
| 2 | Ascensions possible (Lv5 + support), Resonance 3, Shrine corrupts | First build identity |
| 3+ | Resonance 5, cross-element combos, boss reward = guaranteed Ascension or Corruption choice | Crazy builds, matches enemy Intensity |
- **Gating:** use the existing `FloorIdx` and `FightsCleared49()` gates, the same way the enemy ladder works. Player power then comes from **build choices** instead of flat Intensity multipliers, which can stay ~1 (49f/49g direction).
- **Enemy Intensity** keeps rising per floor, and evolutions are the answer to it. That makes the build feel necessary.

## 5) Auto-attack (VS-friendly) vs pause screen
- **Stays auto, no extra input:** all skills fire automatically, resonance and triggers are passive, branch effects just happen.
- **Level-up card screen (already pauses):** branch picks, ascension cards and support picks all appear as cards. No new screen is needed. A branch card shows 2 options side by side.
- **Short build screen (paused, between rooms or at a Shrine):** a "Build" panel listing skills, branch, supports, resonance pips and corrupt option. It is read-only mid-run; actions happen only at Shrines (corrupt, swap a support). No inventory juggling mid-fight.

## 6) Art and UI needs (high level)
- Per skill: 2 branch VFX variants + 2 ascension VFX (≈40 VFX sets total; can ship 3 skills first).
- Branch and ascension card frames (gold / red), element icons (6), HUD resonance pips.
- Corruption shrine visual + roll animation; a "corrupted" tint on skill icons.
- A small build-summary panel (pause menu), with skill icons showing a branch badge.
- SFX stinger for evolve / ascend / resonance tier.

## 7) Open questions for VH (max 5)
1. **Do we need this mechanic? (yes / no)** My recommendation: yes. It is the main fix for flat scaling.
2. **Which approach:** A only, A + B (recommended), or A + B + C (adds corruption gambles)?
3. **Pilot scope:** start with 3 skills (Fireball, Bone Orbit, Chain Lightning) fully evolved before doing all 10?
4. **Branch timing:** at skill Lv3 (first evolve about 3–5 min in), or later (Lv4 / floor 2)?
5. **Corruption risk:** allow a real "brick" outcome (negative), or only twist/upgrade outcomes?
