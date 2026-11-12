# Hellord — Meta Upgrade Menu & Ascendancy v0.5 (DESIGN ONLY)
> v1.0 RULE UPDATE (VH): 8 nodes per Path (N1–N8) + 2 Keystones; a run selects MAX 6 nodes (max 1 Keystone, counts within the 6; sockets grow 2→4→6). Crest at 4+ from one Path, so effectively 1 Crest per run. Older "8 sockets / 8 per run / 2 Crests" text is superseded. See hellord_damnation_pact_ui_layout_v1.0.md.
Replaces the in-run Pacts from v0.4. Ascendancy now lives in a **between-runs menu**. You unlock nodes with meta currency and equip up to **8 nodes per run**. Relics stay in-run, as found objects with effects and trade-offs (kept from v0.4 §2, adjusted below). There is no item grid and no passive-tree spreadsheet.

## 1. Meta currency: "Souls" (+ rare "Sigils")
**Souls** are the common currency. They are kept after death, with no loss.
| Source | Souls |
|---|---|
| Room cleared | 2 (floor 1) +1 per FloorIdx |
| Floor boss killed | 25 × floor number |
| Elite / Hunt / Survive / Nest room clear | +5 bonus |
| Run end (death or win) | 1 per 100 kills + 10 per floor reached |
| Relic salvage (discard in-run) | +5 |
| First-time discovery (new relic, new boss) | +15 one-time |

**Sigils** are a rare currency used to unlock *Keystone-tier* nodes only.
- Sources: the first kill of each boss (3 total), the floor-4 clear, and Curse room clears (1 per run max).
- Sigils stop the strongest nodes from being bought early with grind alone.

**Sinks:**
- Unlock Ascendancy nodes (main sink).
- Upgrade tab: small account conveniences (below).
- Relic Codex: unlock relics into the drop pool.
- Rerolls bank.

**Pacing target:**
- A first floor-1 death gives about 40 Souls, which buys 1–2 cheap nodes.
- An average early run gives about 80–150 Souls.
- Unlocking the full Ascendancy list takes about 25–35 runs.

## 2. Upgrade Menu ("The Pit Altar", main menu)
### Tabs
1. **Ascendancy:** node list, unlock and equip (up to 8 per run).
2. **Arsenal:** unlock starting-skill choices and supports into the card pool. This expands variety, not power.
3. **Relic Codex:** unlock relics into the in-run drop pool, and view discovered ones.
4. **Comforts (small, capped):**
   - +1 reroll and +1 banish per run.
   - Start-of-run choice of 1 of 3 starting skills.
   - These are capped so power stays mostly in Ascendancy and build choices.
5. **Codex/Stats:** lore, a bestiary and run history.

### Ascendancy structure (PoE2-like clusters, not a big tree)
- **4 Ascendancy Paths (classes of identity), 12 nodes each, 48 total:**
  - **Pyromancer of the Pit:** Fire / Projectile / Burn.
  - **Bone Warden:** Orbital / Zone / Minions.
  - **Stormcaller:** Shock / Chain / Triggers.
  - **Blood Reaver:** Melee / Life / Risk.
- **Node tiers inside each Path:**
  - **Notables (8 per path):** rule-changers. Cost 30–80 Souls.
  - **Keystones (2 per path):** build-defining with a drawback. Cost 150 Souls + 1 Sigil.
  - **Hybrid nodes (2 per path):** require 2 nodes from that path AND 1 from another path. Cost 100 Souls.
- **Prerequisites (PoE2 feel without a tree):**
  - Each node lists "Requires: X" inside its own path. Paths are short chains of 2–3.
  - A Keystone requires 2 Notables from its path.
  - Requirements apply to both unlocking and selecting. You must select the prerequisite in the same loadout.
- **Loadout rule: max 8 selected per run.** No Path lock. You may mix paths, but:
  - Keystones: max 1 selected.
  - **Path Identity bonus:** selecting 4+ nodes from one path grants that path's **Crest**. A Crest is a rule-change, e.g. Pyromancer Crest: "Burning enemies count as Fire for every Fire resonance".
  - This rewards focus vs splash. Two Crests are possible (4+4).
- **Unlock vs select:** unlocking is permanent (spend Souls). Selecting is free, done at run start, and can change every run.

## 3. What an Ascendancy node IS
**Rule:** every node changes a rule, a conversion, a trigger, or a trade-off. A flat "+X% dmg" alone is not allowed. If a node has a number, it comes with a behaviour change or a cost.

Example nodes:

**Pyromancer of the Pit**
- *Kindling:* Fireball leaves a burning patch on impact. Patches count as Zone, so Grave Pool / Zone supports apply.
- *Cinder Split:* when a Burning enemy dies, its Burn jumps to the 2 nearest enemies.
- *Overheat (Notable):* skills don't have cooldowns. Instead they build Heat, and at 100 Heat you vent a nova and cast nothing for 1.5 s.
- *Keystone — Ash Covenant:* all your damage is Fire; Chill/Shock supports become Burn variants; you can't freeze or shock.

**Bone Warden**
- *Marrow Launch:* every 3 s, Bone Orbit bones fire outward and return. Orbitals gain the Projectile tag, so Fork, Pierce and Multiple Projectiles apply.
- *Ossuary Ward:* orbitals block enemy projectiles and break after 3 blocks, then regrow.
- *Bone Legion:* every 25th kill raises a skeleton minion (up to 5) that copies your Orbital supports.
- *Keystone — Sloth Throne:* while standing still, Zones and Orbitals ×2 radius; while moving they shrink to half.

**Stormcaller**
- *Conduit:* Chain Lightning can chain to your own orbitals and zones, re-triggering their hit.
- *Storm Capacitor:* Storm Ring no longer pulses; it stores charge and releases on dash.
- *Cast on Shock:* shocking an enemy triggers your lowest-level skill. Each trigger costs Energy, and Energy refills from kills.
- *Keystone — Unbound Chains:* triggers cost no Energy; each trigger costs 1 HP.

**Blood Reaver**
- *Leech Convert:* Life Leech overheals into a Blood Shield that explodes when it breaks.
- *Price of Wrath:* skills can be cast at 2× speed and cost HP; toggle with the dash key hold.
- *Whip Reach:* Blood Whip hits in a full circle, and its attack rate is halved.
- *Keystone — Martyr:* max HP halved; below 30% HP every skill is Empowered and leeches.

**Hybrid examples**
- *Burning Bones* (Pyro + Bone): orbitals ignite, and Burn deaths regrow bones.
- *Blood Lightning* (Storm + Blood): chains heal you per jump, and every chain costs 0.5% HP.

**Why 8 of 48 makes many builds:**
- C(48,8) is about 377 million raw loadouts. With prerequisites and the 1-keystone cap, it is still tens of thousands of distinct identities.
- Nodes convert tags. Each loadout therefore changes which supports and level-up cards are good, so the *in-run* card choices diverge too.
- Unlocked-only selection means each player's collection shapes their builds over time.

## 4. Relics (in-run, found)
- **Run-only** (lost at run end). The **Relic Codex** (meta) decides which relics can drop. Unlocking costs Souls, and discovering a relic also unlocks it.
- **Slots:** 3, plus 1 Cursed slot from a Curse room. Each relic is a fixed, handcrafted effect with no affixes. Pool is about 30.
- **Types:**
  - **Boon:** modest, e.g. *Ember Tooth* (Burn spreads on death).
  - **Trade-off:** e.g. *Glass Horn* (×1.6 dmg, −40% max HP), *Heavy Crown* (+1 skill slot, −15% move), *Blood Moon* (enemies +30% HP, double level-ups).
  - **Cursed:** e.g. *Eye of the Pit* (everything Fire, other elements off).
- **Interaction with Ascendancy:** relics carry tags. Some relics **resonate** with a selected Path and show a "Resonant" gold border on the pick card. Examples:
  - *Ember Tooth* + Pyromancer's *Cinder Split* gives a double spread.
  - *Glass Horn* + *Martyr* means a permanent low-HP Empower. That is risky but huge.
  - When at least 4 nodes from a path are selected, the relic pick screen weights resonant relics higher.
- **Sources:**
  - Boss: pick 1 of 3.
  - Treasure room: pick 1 of 2.
  - Elite/Hunt/Survive: 25% chance of a relic chest.
  - Nests: shards, 3 shards make a pick.
  - Curse room: a Cursed relic.
- **Pity:** at least 1 relic per floor before the boss.
- **Floor 1:** Boon only. Trade-offs appear from floor 2.

## 5. UI
- **Pit Altar (main menu):**
  - Left: tabs.
  - Center for Ascendancy: 4 Path columns. Each column is a vertical list of nodes with lines showing prerequisites. It reads as a short chain, not a tree.
  - **Node states:**
    - Locked (greyed, with price shown).
    - Unlockable (glow).
    - Unlocked.
    - Selected (filled, red gem).
  - Hovering shows a detail panel with the rule text, tags affected, and a "Pairs well with" list of supports/relics.
  - Top bar shows currencies (Souls, Sigils) and the **Loadout bar: 8 gem sockets**, 1 marked Keystone. Crest indicators appear when 4 nodes from one path are selected.
  - **Presets:** 3 saved loadouts.
- **Run-start confirm:**
  - After choosing the starting skill, a "Your Pact" screen shows the 8 selected nodes + Crest(s) as cards, with "Begin" / "Edit".
  - Invalid loadouts are blocked with a reason: a missing prerequisite or 2 Keystones.
- **In-run HUD:**
  - Under the XP bar: a compact row of 8 tiny node glyphs + Crest icon + relic icons (up to 4).
  - Glyphs flash when the node procs (e.g. *Cast on Shock*).
  - Heat/Energy meters appear only if a selected node uses them.
- **Pause / Build panel (Tab):** 3 columns.
  - Skills + supports.
  - Ascendancy (8 nodes, read-only during the run).
  - Relics.
  - Plus a "Tags you scale" summary.
- **Run end screen:**
  - Souls/Sigils earned with a breakdown.
  - New discoveries.
  - A "Next unlock" suggestion button straight into the Altar.

## 6. Early game & new players
- **First run:** the Ascendancy tab is locked, with a tooltip "Reach floor 1 boss". It unlocks after the first boss attempt or after run 2. So the first run is pure Vampire Survivors-style play.
- **Starter unlocks:** each path's first node is free, granted on unlock. Loadout slots grow over time:
  - 2 sockets at start.
  - 4 after the first boss kill.
  - 6 after the floor-2 boss.
  - 8 after the floor-3 boss (or at a Souls milestone).
  - This way the "max 8" is earned.
- **Floor 1 stays safe:**
  - Nodes change rules, not raw numbers.
  - Keystones need Sigils, which come from boss first-kills.
  - Enemy scaling is unchanged by meta. Optionally a "Heat/Torment" difficulty can be added later for veterans.
- Comforts upgrades are capped, so long-time players don't flatten floor 1.

## 7. Pilot order
1. **v0.0.50 — Meta core:**
   - Souls earn/save.
   - Run end screen.
   - Pit Altar with the Ascendancy tab: 2 Paths (Pyromancer, Bone Warden) × 6 nodes, no Keystones yet.
   - Loadout sockets with prerequisites; HUD glyph row.
2. **v0.0.51 — Relics v1 in-run:** 3 slots, 12 relics, sources + pity. Relic Codex tab.
3. **v0.0.52:**
   - The other 2 Paths.
   - Keystones + Sigils.
   - Crests (4-node bonus).
   - Socket growth 2→8.
   - Presets.
4. **v0.0.53:**
   - Hybrid nodes.
   - Arsenal tab (unlock skills and supports into the pool).
   - Branch/Ascend skill evolutions from v0.1 as in-run card upgrades.
   - Curse/Cursed relics.

## Open questions for VH (max 5)
1. Sockets: start at 2 and grow to 8, or all 8 available once nodes are unlocked?
2. Free mixing across all 4 Paths (with the Crest bonus at 4+), or force 1 main Path + splash?
3. Should Keystones need a separate rare currency (Sigils), or Souls only?
4. Comforts tab (rerolls, start-skill choice): allowed, or keep meta power purely in Ascendancy?
5. Path names/theme (Pyromancer / Bone Warden / Stormcaller / Blood Reaver) OK, or tie to the 3 floor themes (Ox Pit / Frozen / Burning)?
