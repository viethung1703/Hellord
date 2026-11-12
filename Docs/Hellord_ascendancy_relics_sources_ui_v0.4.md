# Hellord — Ascendancy Passives, Relics, Sources & UI v0.4 (DESIGN ONLY)

> **t729u: RELICS CUT by VH. Relic sections below are obsolete; replaced by Sins (Hellord_sins_design_v0.1.md). Pacts stay.**

Replaces v0.3 items/tree. No item slots grid, no inventory, no passive tree. There are two new layers on top of Skills/Supports + Branch/Ascend (v0.1):
- **Pacts:** ascendancy-like unique passives that change rules.
- **Relics:** few, special, often with trade-offs.

## 1. Pacts (ascendancy-like unique passives)
**Concept:** you make a Pact with one of 4 Hell Lords. Each Lord has 6 Pact passives, 24 in total. Every Pact changes HOW a build works, not a stat.

### Rules
- **You get 4 Pact points per run:**
  - 1 after the floor-1 boss.
  - 1 after the floor-2 boss.
  - 1 after the floor-3 boss.
  - 1 bonus from a Pact Shrine.
- **Choosing a Lord:**
  - The first point chooses your Lord: pick 1 of 3 Lords offered. That Lord is locked for the run.
  - Later points pick 1 of 3 Pacts from that Lord.
  - The 4th (bonus) point may take 1 Pact from ANY Lord. This is the "splash" pick.
- **Tiers:** each Lord's 6 Pacts are split into a Minor tier (3) and a Major tier (3). A Major tier Pact needs 1 owned Minor tier Pact first. That gives the PoE Ascendancy ladder feel.
- **Respec:** none freely. A Pact Shrine can swap 1 Pact once per run for HP or Hellfire. Not in v1.

### The 4 Lords and example Pacts
**Asmodeus — Lord of Excess (Projectile / many-hits)**
- *Minor:*
  - **Overflow:** Projectile skills fire +2 projectiles, each −30% dmg.
  - **Ricochet Sin:** projectiles that hit a wall split.
  - **Hungry Barrage:** every 10th cast is a full-circle volley.
- *Major:*
  - **Fireball Legion:** Fireball becomes a turret-like stream: 0.1 s interval, tiny balls.
  - **Orbit Launch:** Bone Orbit bones fire outward every 3 s, so orbitals become projectiles and gain Pierce/Fork.
  - **Endless Spears:** Soul Spears no longer expire; they circle back.

**Belphegor — Lord of Sloth (Zone / Aura / standing still)**
- *Minor:*
  - **Rooted:** standing still 1 s gives +60% area and dmg for zones.
  - **Lazy Orbit:** Bone Orbit stops rotating and becomes a fixed ward that blocks enemies.
  - **Heavy Air:** auras slow by 25%.
- *Major:*
  - **Grave Kingdom:** Grave Pool is permanent under you and follows at walking speed.
  - **Pillar Garden:** Hellfire Pillars persist 4 s and link with fire walls.
  - **Storm Throne:** Storm Ring radius doubles while still, and drops to 0 while moving.

**Mammon — Lord of Greed (economy / scaling over time)**
- *Minor:*
  - **Tithe:** each kill gives +0.02% dmg permanently for the run; capped.
  - **Golden Souls:** XP orbs also heal.
  - **Extra Card:** level-ups show 4 options.
- *Major:*
  - **Hoarder:** you may hold 2 extra skills, but all skill Lv max is 4.
  - **Interest:** unspent rerolls grow ×1.5 each floor.
  - **Midas Curse:** Chain Lightning turns killed enemies into gold bombs.

**Satan — Lord of Wrath (melee / risk / blood)**
- *Minor:*
  - **Blood Price:** skills cost 1% HP and deal +50%.
  - **Close Wrath:** +40% dmg to enemies within melee range.
  - **Spite:** taking a hit casts Frost Cleave around you.
- *Major:*
  - **Whip Storm:** Blood Whip hits all directions, with Leech doubled.
  - **Martyr:** below 30% HP every skill is Empowered.
  - **Wrathborn:** Frost Cleave and Blood Whip trigger Hellfire Pillar on crit.

### How Pacts reshape kits
- Each Major tier Pact names 1 or 2 skills and changes their behaviour type. Examples:
  - Orbit → Projectile.
  - Pool → Aura.
  - Whip → AoE.
- Supports keep working through the new tags. This is the build-changing core, for example Orbit Launch + Fork + Pierce.
- The Pact Lord also biases the level-up card pool toward its tags, about +30% weight, so the build forms naturally.

## 2. Relics (special effects, some trade-offs)
- **Slots:** 3 Relic slots, plus 1 Cursed slot unlocked by a Curse room. Max 4 relics.
  - No affixes, rarity tiers or rolls: each relic is 1 fixed handcrafted effect.
  - Total pool about 30.
- **Types:**
  - **Boon relics (about 15):** pure positive, modest. Examples:
    - **Ember Tooth:** Burn spreads on death.
    - **Bone Bell:** +1 orbital.
    - **Storm Jar:** every 5th hit Shocks.
    - **Thief's Hand:** pickup radius +80%.
  - **Trade-off relics (about 10):** strong, with a clear cost. Examples:
    - **Glass Horn:** ×1.6 dmg, max HP −40%.
    - **Heavy Crown:** +1 skill slot, move speed −15%.
    - **Hourglass of Sloth:** cooldowns −30%, and you can't dash.
    - **Chained Echo:** Echo triggers twice, and skills cast in a fixed order.
    - **Blood Moon:** enemies +30% HP and you get 2 level-ups per level.
  - **Cursed relics (about 5):** Cursed slot only, from Curse rooms. Build-defining with a heavy drawback. Examples:
    - **Eye of the Pit:** all skills become Fire, other elements are disabled.
    - **Lich Heart:** no healing, and kills restore shield.
- **Discarding:** you can discard a relic at a between-floor screen. A discarded relic gives a reroll token, so there is no treadmill.

## 3. Sources
| Source | Pact point | Relic | Notes |
|---|---|---|---|
| Floor boss (each floor) | ✔ guaranteed | ✔ pick 1 of 3 boon/trade-off | the main progression beat |
| Pact Shrine room (Shrine variant) | ✔ 1/run bonus (splash) | — | appears guaranteed on floor 2 or 3 |
| Treasure room | — | ✔ pick 1 of 2 | 1 per floor max |
| Curse room | — | ✔ Cursed relic + unlocks the Cursed slot | opt-in risk |
| Elite / Hunt / Survive clear | — | 25% Relic chest; pity guarantees 1 per floor | |
| Nests destroyed (all in room) | — | Relic shard (3 shards = relic pick) | rewards nest play |
| Corruption altar (Shrine) | — | turns 1 relic into a Trade-off version ×1.5 effect | optional |

- **Pity:**
  - Every floor guarantees at least 1 relic before the boss. If no relic has dropped by the second-to-last room, the room-clear chest is forced to be a relic.
  - Pacts are guaranteed by bosses, so they are never starved.
- **Early game (floor 1):**
  - The first relic is a Boon only, pick 1 of 2, at about room 3.
  - The first Pact comes at the floor-1 boss, about 8–10 minutes in.
  - Trade-off relics appear from floor 2.
- **Late game (floor 3–4):**
  - 3 or 4 Pacts and 3 or 4 relics, including at least 1 trade-off.

## 4. UI
- **Pact screen (after the boss, paused):**
  - The first time, 3 Lord banners, each with sigil art, its theme tags and 2 sample Pacts.
  - Afterwards, 3 Pact cards from your Lord, shown on a small 2-tier ladder (Minor → Major) so the next Major tier Pacts are visible.
  - Art: 4 Lord sigils (M) plus card frames (S).
- **Relic pick:** the same card UI as level-up, with a different frame colour: boon gold, trade-off purple with a red "COST" line, cursed black/red. If slots are full, the pick screen shows your relics so you can choose one to replace. This happens only on pause screens.
- **HUD:**
  - A small row under the XP bar: Lord sigil + Pact pips (up to 4) + relic icons (up to 4).
  - A relic icon flashes when it procs.
  - Nothing else is added mid-fight.
- **Build panel (pause menu, Tab):** 3 columns.
  - Skills: icon, level, branch and linked supports.
  - Pacts.
  - Relics.
  - Hovering shows the full text. A "Tags you scale" bar summary.
  - Read-only.
- **Between-floor screen:** a summary of the floor, with:
  - discarding a relic for a reroll token
  - using the Corruption altar result
  - a preview of the next floor's theme and its guaranteed Pact Shrine
- **Codex (main menu):** all Lords, Pacts and relics revealed on discovery, to drive replay.

## 5. How this scales builds without a tree or item grid
- **Multiplier layers:**
  - Skills/Supports and Branch/Ascend set the base and the form.
  - Pacts set rule changes and tag conversion, which makes supports apply in new ways, plus biased card weighting.
  - Relics are discrete multipliers and procs, often big with costs.
- **Scale comes from conversions × supports,** not from stacked %. For example, Orbit Launch turns orbitals into projectiles, so Multiple Projectiles, Fork and Pierce all now multiply bone damage.
- **Combinations:** 4 Lords × about 20 Pact sets per Lord × C(30,3–4) relic sets × branches gives thousands of runs. Each run still reads as one identity, e.g. "Sloth Pool-Aura with Glass Horn".
- **Rough power target, for tuning:**
  - Pacts about ×2–3.
  - Relics about ×1.5–2.5.
  - On top of skills about ×5.
  - Retune floorEnemyHpGrowth when pilot data comes in.

## 6. Pilot order
1. **v0.0.50 — Relics v1:**
   - 3 slots, 12 relics (8 boon, 4 trade-off).
   - Sources: boss pick, Treasure, Elite chest, plus pity.
   - Relic card frame and HUD row.
   - Cheapest; immediate feel.
2. **v0.0.51 — Pacts v1:**
   - 2 Lords (Asmodeus, Belphegor) × 6 Pacts.
   - Boss grants, Pact screen, Build panel.
3. **v0.0.52:** the other 2 Lords, Pact Shrine splash, Curse room + Cursed slot.
4. **v0.0.53:** Branch/Ascend skills (v0.1), Nest shards, Corruption altar, Codex.

## Open questions for VH (max 5)
1. Lock to 1 Lord per run with 1 splash, or allow free mixing of all Pacts?
2. 3 relic slots + 1 Cursed: OK, or fewer/more?
3. Should Pacts unlock only at bosses (slower, about 1 per floor), or should Shrine rooms on floor 1 give one earlier?
4. Theme: Hell Lords / Seven Sins naming OK, or another lore frame?
5. Should some Pacts/relics be meta-unlocked over runs (Codex progression), or all available from run 1?
