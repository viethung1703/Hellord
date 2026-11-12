# Hellord — Buildcraft v0.3: Items + Passive Tree Lite (DESIGN ONLY)
Builds on v0.1 (Branch/Ascend) and the v0.2 ideas menu (Relics 4.1, Uniques 4.2, Constellation 8.1, Keystones 8.2, Card rarity 12.1, Resonance 2.1, Triggers 2.3).

## 1. Why evolve/ascend alone feels flat (the PoE lesson)
- **There is only one scaling axis.** Skill levels and evolutions are all drawn from the same level-up card pool. Every run ends up near the same power curve, and builds converge on "max the best skill".
- **Power in PoE comes from 3 independent sources that multiply:** gems (what you do) × items (big flat and unique rule-changers) × passive tree (% increases, keystones). Ascendancy is only the cherry on top. Take two of those away and the build is mostly decided.
- **There are no persistent decisions outside the level-up pop-up.** Items give you loot moments: a reason to kill Elites, open chests and take risk rooms. The tree gives you a plan between floors.
- **Additive stacking goes flat.** +10% +10% +10% from cards is linear. Separate buckets (skill "more" × item "more" × passive "increased") give a curve that steepens late, which is the feeling VH wants.
- **Conclusion:** Hellord needs 3 layers, and each layer must feed a different multiplier bucket and a different decision moment.

## 2. Item system (survivor-friendly)
**Core rule:** no inventory grid and no mid-fight menus. Items are picked up by walking over them and auto-equip into fixed slots. Swapping happens at the next room-clear screen.

### 2.1 Slots (5 total)
- **Weapon Sigil (1):** changes base attack behaviour or a tag. Example: "Projectile skills +1 Fork", or "Melee skills leave a burning trail".
- **Relics (3):** passive tag or condition effects.
- **Heart (1):** a defensive or sustain rule. Example: "Life Leech also overheals into a shield".

### 2.2 Rarity
| Rarity | Content | Source |
|---|---|---|
| Common (white) | 1 stat (+dmg %, +area %, +move) | normal drops, chests |
| Magic (blue) | 2 stats, tag-scoped ("Orbital skills +20% dmg") | Elites, Treasure room |
| Rare (yellow) | 3 stats + 1 small rule | Elite guards, Hunt, Nests |
| Unique (orange) | fixed build-defining rule | bosses, Curse/Shrine rooms, Survive |
| Corrupted (red) | Unique or Rare + a corruption twist (big up or a drawback) | Shrine altar (v0.2 6.1) |

### 2.3 Tag synergy (the key)
Every affix references a **tag** that already exists (Projectile, Area, Orbital, Hit, Melee, Aura, Zone) or an **element** (Fire, Frost, Storm, Blood, Bone, Soul). It can also reference a **support** ("Supports of type Burn count as +1 level").

Example uniques:
- **Crown of the Pit:** Orbitals count as Projectiles, so they gain Fork/Pierce. (Bone Orbit + Fork build.)
- **Ashen Heart:** Burning enemies that die cast Fireball at the nearest enemy. (A trigger without a support slot.)
- **Glacial Brand:** Chill becomes Freeze at 5 stacks. Frozen enemies shatter for Area dmg.
- **Whip of Seven Sins:** Blood Whip hits apply every element you have Resonance in.
- **Echo Skull:** Echo support repeats twice, and your max skills are reduced by 1. (Trade-off.)
- **Grave Censer:** Grave Pool follows you as an Aura.

Rule: about 30% of uniques are **tag-converters**, the most build-generating kind. Effects like "X counts as Y" open PoE-style combinations with no new art.

### 2.4 Drop sources & pacing
- **Floor 1:** commons/magics only. 1 guaranteed Relic choice (pick 1 of 2) after room 3. Boss gives 1 Unique, picked 1 of 3.
- **Floor 2+:** Rare/Unique chance scales with FloorIdx. Elites always drop Magic+. Treasure room gives a pick-1-of-3 Rare.
- **Risk rooms** (Curse, Survive, Hunt) have higher rarity. This gives the room types a loot purpose.
- **Pickup flow:** if the slot is free, the item auto-equips. If slots are full, the item goes into a **Satchel** (max 3) and you choose a swap on the room-clear screen. Unclaimed items are salvaged into Sins (see 3.2).

### 2.5 Early vs late
- **Early:** the first items are simple stats, and a tooltip names the tag. The player learns what tags are without reading a sheet.
- **Late:** uniques and corrupted items rewrite rules. Expected late kit: about 2 uniques, 2 rares and 1 corrupted.

## 3. Passive tree LITE ("Infernal Constellation")
**Where:** at the floor transition screen (between floors), plus a pause-menu view. Allocation happens only between floors or on room clear, never live.

### 3.1 Shape
- About **40 nodes total**, in 3 wings from the center:
  - **Wrath** (damage / crit / projectiles)
  - **Gluttony** (sustain / area / orbitals and auras)
  - **Pride** (speed / cooldown / triggers and summons)
- **Node types:**
  - Small: +6% to a tag. About 24 of these.
  - Notable: +20% or a mini rule. About 10.
  - **Keystone:** 1 per wing tip plus 3 hidden ones, 6 total. You can own at most 1 or 2 per run.
- Each wing ends at a keystone, and the wings are linked at the edges so hybrids are possible.

### 3.2 Currency: Sins
- **In-run:**
  - Each floor cleared gives 3 Sins.
  - Boss kills give +2.
  - Salvaged items give +1 each.
  - Expected total by floor 4: about 18 points, so you fill about 45% of the tree. That forces identity.
- **Meta (optional, later):** a persistent unlock of keystones and wing starts. This is not a power grind; it widens choice instead.

### 3.3 Example keystones
- **Glass Hellion:** ×1.8 damage, and Max HP is fixed at 1 plus a regenerating shield.
- **Legion:** +2 max skills, and all skills deal −25%.
- **Elemental Equilibrium:** hitting with an element makes enemies take +40% damage from *other* elements. This feeds Resonance and Reactions.
- **Tyrant's Patience:** skills fire half as often, and each cast is ×2.5 and Empowered.
- **Unbound Chains:** triggers have no Energy cost but cause 1 HP of self-damage per proc.
- **Soul Furnace:** the Hellfire meter fills from damage taken as well as kills.

### 3.4 How the tree multiplies with evolve and items
- Tree nodes use the **"increased"** bucket: additive within the tree.
- Items use the **"more"** bucket: each Unique or Rare rule is a separate multiplier.
- Evolve/Ascend changes the **form** and the base damage.
- Keystones **change which bucket matters**.
  - Example: Tyrant's Patience makes Cast Cadence and Echo stronger. Elemental Equilibrium makes multi-element items stronger.

## 4. How the three layers stack
**Final DPS** = Base(skill Lv × evolved form) × (1 + Σ increased [cards + tree]) × Π more [supports at breakpoints, uniques, keystone] × Hits/sec (cast rate, projectiles, echoes, triggers) × Conditional (resonance, reactions, marks)

- **Rough late power,** a target for tuning:
  - Skills: about ×5.
  - Tree: about ×2.
  - Items: about ×3.
  - Keystone and resonance: about ×1.5.
  - Total: about ×45 versus floor 1.
- Enemy floorEnemyHpGrowth then needs to be retuned. The current 0.15/floor is set for card-only scaling.
- **How builds multiply:**
  - About 20 Lv3 branches × 6 element sets × about 15 tag-converter uniques × 6 keystones gives thousands of distinct combinations.
  - Readable archetypes include:
    - "Orbital-Projectile Bone Fork"
    - "Frozen shatter Cleave"
    - "Trigger-chain Storm"
    - "Tyrant slow nukes"
    - "Legion many-skill"
- **Floor gates prevent early-game explosions:**
  - Rarity is floor-gated.
  - Keystones are available from Sin 6 on, which is roughly floor 2.
  - Tree points arrive only between floors.

## 5. VS readability: what pauses vs what auto-applies
| Event | Behaviour |
|---|---|
| Level-up card (skills/supports/branch) | PAUSE (as now), and a Rarity frame tells you its weight |
| Item pickup, free slot | AUTO-equip + a 1.5 s toast at the screen edge (icon + one line) |
| Item pickup, slots full | goes to the Satchel. Choose at room clear, never mid-fight |
| Relic/Unique pick 1 of 3 (boss, Treasure) | PAUSE (after the room is cleared) |
| Passive tree points | between floors only (floor transition screen); pause menu is read-only + undo for the current floor |
| Triggers / reactions / resonance procs | auto, with distinct small VFX and colour per element; cap on-screen proc VFX |
| Corruption | at the Shrine altar only, with a confirm |

Readability rules:
- At most 1 new mechanic glyph on the HUD per layer.
- Build summary on pause: "Tags you scale" bars.
- Projectile and colour palettes stay tied to element.

## 6. Pilot order (recommendation)
1. **v0.0.50 — Items v1:**
   - 5 slots, Common/Magic/Rare.
   - About 8 tag-converter Uniques.
   - Boss pick 1 of 3, Satchel + room-clear swap.
   - Fastest felt-scaling gain. Mostly icons + text (art S/M).
2. **v0.0.51 — Card rarity + Banish/Reroll** (v0.2 12.1/6.3). Cheap; it steers builds toward items.
3. **v0.0.52 — Constellation Lite:**
   - About 25 nodes at first, 3 keystones, Sins currency.
   - The floor transition screen is the only new UI.
4. **v0.0.53 — Branch at Lv3 + Ascension** (v0.1 A) on 3 skills (Fireball, Bone Orbit, Chain Lightning), then the rest.
5. **v0.0.54+ — Resonance/Reactions, Triggers, Corrupted items,** retune of the enemy HP curve.

Why items first: VH's point is scaling. Items add a new multiplier bucket and loot excitement with the least code risk, and they reuse existing tags.

## 7. Open questions for VH (max 5)
1. Should items persist only within a run (roguelite), or should some stash/meta unlocks exist between runs?
2. 5 slots (Sigil + 3 Relics + Heart) OK, or simpler: 4 Relics only?
3. Tree between floors only, or also a point per X levels mid-run (with pause)?
4. How strong should late game be? About ×40–50 power over floor 1, with enemies scaling to match. Or do you prefer a flatter curve?
5. Should Corrupted/drawback items and keystones be in the first pilot, or kept for later?
