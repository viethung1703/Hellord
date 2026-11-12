> **REJECTED by VH, Oct 31 2026: Paths only.** No Combos, no hybrids, no prebuilt kits. Damnation Pact = pick ONE Path per run, then pick up to 6 of its nodes (max 1 Keystone, Crest at 4+). Players find their own builds. This doc is kept for history only.

# Hellord — Damnation Pact: Hybrid Trade-offs v1.0 (DESIGN ONLY, for VH approval)
Base: Hellord_ascendancy_nodes_full_v0.6.md (v1.0 rule: max 6 nodes, max 1 Keystone, Crest at 4+ from one Path). No code, no version bump.

## 0. VH intent
Like PoE ascendancy, a Pact is a real choice:
- **Pure Pact:** go deep in 1 Path and use its Keystone and Crest.
- **Hybrid Pact:** fuse 2 Paths into one very strong combo, and give up depth, Keystone and Crest to get it.
Picking a Hybrid closes off the rest of both Paths for that run. You can never "have it all".

## 1. What a Hybrid is (new definition)
- A Hybrid is a **Pact Seal**: a two-Path fusion node that takes the place of a Keystone. It is not a free extra node.
- Each Hybrid belongs to exactly one Path pair (6 pairs, 6 Hybrids). You choose it **instead of** going deep in one Path and **instead of** a Keystone.
- Choosing a Hybrid sets the run's **Pact mode = Hybrid (Home + Partner)**. Every other Path, and every other Hybrid, is locked for that run.
- Home/Partner: either parent Path can be Home. The Home Path gets 3 sockets and the Partner Path gets 2 (see §2).

## 2. Loadout rules
Both modes keep **max 6 selected** and the same socket growth (2 → 4 → 6).

| | Pure Pact | Hybrid Pact |
|---|---|---|
| Paths used | 1 | exactly 2 (one pair) |
| Nodes | up to 6 from that Path | Hybrid (1) + Home up to 3 (incl. N1) + Partner up to 2 (incl. N1) = 6 |
| Keystone | max 1 (its own Path) | **none** (the Hybrid uses the gold Keystone socket) |
| Crest | yes at 4+ | **no**: you can't reach 4 in either Path |
| Hybrids | none | exactly 1, and only that pair's |
| Third Path | no | no |

- **Mixing Notables + Hybrid:** yes. That is the whole build: Hybrid + 2 Home Notables (after N1) + 1 Partner Notable (after N1).
- **Keystones from both Paths in a Hybrid run:** no, and not even one (see §5).
- **Deep nodes:** in Hybrid mode, Home N5–N8 and Partner N3–N8 can still be picked if their chain is met. The socket caps (Home 3, Partner 2) are what limit you.
- **Mode lock:** the first time you pick a Partner N1 or a Hybrid during a run, the Pact confirm screen says "HYBRID PACT: <Home> + <Partner>. Keystones and Crests sealed." Then the mode locks.

## 3. Prerequisites redesign
The old requirement ("2 Home nodes + Partner N1 selected") forced 2 Paths into a one-Path loadout. It is replaced by an **exclusive Hybrid branch per pair**.

**Meta unlock (Souls, done once):**
- **Cost:** 100 S + 1 Sigil, the same weight as a Keystone because it is a Keystone-tier power.
- **Requirements:** you must have *unlocked* (bought, not selected) Home N1, Partner N1, and any 1 Notable in either parent Path.
- **Placement:** each Hybrid shows as a diamond between its 2 columns (unchanged UI). It has a single owner pair, so it is no longer "reachable from both paths" in a confusing way.

**Run selection (exclusive):**
- **Selection order:** to select a Hybrid, Home N1 and Partner N1 must both be selected, and the run must not already hold a Keystone.
- **Lock when a Hybrid is selected:** all Keystones, all other Paths and all other Hybrids grey out with "Sealed by <Hybrid>".
- **Lock when a Keystone is selected:** all Hybrids and every other Path's N1 grey out with "Sealed by <Keystone>".
- **Early runs (2–4 sockets):** you may select Home N1 + Partner N1 before the Hybrid, so a 3-socket Hybrid build is possible at 4 sockets.

**Optional, stronger exclusivity (for VH to decide, Q1):** buying a Hybrid at the meta level permanently seals the other 2 Hybrids that share its Home Path, until a respec costing Souls.

## 4. The 6 Hybrids
The power target is "super strong". Each Hybrid ≈ one Keystone + the value of a Crest, because it replaces both. All effects below build on v0.6 and are tuned up to that level.

### H-PB Burning Bones (Pyromancer + Bone Warden)
- **Effect:** Orbitals ignite on every hit. When a Burning enemy dies, a broken bone regrows instantly. **Seal bonus:** +1 orbital bone, and orbitals deal +30% damage to Burning enemies.
- **Trade-off text:** "Your bones burn forever, but the Pit's flame and the Ossuary's throne are closed to you."
- **What you give up:** Ash Covenant / Inferno Heart / Sloth Throne / Necro Covenant; Pit-Born Flame + Ossuary Lord Crests; Pyro N5–N8 depth beyond 2 Notables.
- **Required:** PY-N1 + BW-N1 selected; meta-unlock as in §3.
- **Power:** high, easy, and great for clearing. Watch the regrow loop at 6+ bones (cap it at 1 regrow per 0.2 s).

### H-PS Plasma Storm (Pyromancer + Stormcaller)
- **Effect:** Shocking a Burning enemy turns it into Plasma: Burn ticks chain 1 jump of lightning. **Seal bonus:** Plasma jumps also apply Burn (they spread), and Shock lasts +50% on Burning enemies.
- **Trade-off text:** "Fire and storm become one; you will never wear the Tempest's Eye."
- **What you give up:** all 4 related Keystones (Ash Covenant, Inferno Heart, Unbound Chains, Elemental Equilibrium) and both Crests. The free-trigger economy (Eye of the Tempest) is gone.
- **Required:** PY-N1 + ST-N1.
- **Power:** very high in big packs; it scales with enemy density. It needs a limit of 1 chain per tick per enemy.

### H-BB Blood Marrow (Bone Warden + Blood Reaver)
- **Effect:** Bleeding enemies hit by orbitals lose 1 bone's worth of HP as Bone damage, and orbital LifeLeech is ×2. **Seal bonus:** orbital hits apply Bleed.
- **Trade-off text:** "The marrow drinks, but you will never be a Martyr nor a Lord of bones."
- **What you give up:** Sloth Throne / Necro Covenant / Martyr / Blood Magic; Ossuary Lord + Crimson Pact Crests (no Blood Shield).
- **Required:** BW-N1 + BR-N1.
- **Power:** high sustain with medium damage. It is the safest Hybrid and a good first one.

### H-BS Grave Conduit (Bone Warden + Stormcaller)
- **Effect:** Chain Lightning can chain through your minions and zones, and each one re-hits around itself. **Seal bonus:** +1 chain, and every minion or zone counts as Shocked-target bait.
- **Trade-off text:** "The dead carry your storm; the Tempest and the Throne are sealed."
- **What you give up:** 4 Keystones and both Crests. It also cannot use Necro Covenant, so its minions are only Legion/Pool sources.
- **Required:** BW-N1 + ST-N1.
- **Power:** high, but it depends on setup (zones and minions must exist). Its weak start is balanced by a strong late game.

### H-SB Blood Lightning (Stormcaller + Blood Reaver)
- **Effect:** each chain jump heals 0.5 HP, and each cast costs 0.5% max HP. On low-HP enemies, jumps apply Bleed. **Seal bonus:** while you are under 50% HP, chains get +1 jump.
- **Trade-off text:** "Your blood rides the lightning; Blood Magic and the Eye are forsaken."
- **What you give up:** Unbound Chains / Elemental Equilibrium / Martyr / Blood Magic; Eye of the Tempest + Crimson Pact Crests.
- **Required:** ST-N1 + BR-N1.
- **Power:** medium-high and skill-based, with a risky low-HP style. Without Martyr it needs the Seal bonus to feel strong.

### H-BP Crimson Pyre (Blood Reaver + Pyromancer)
- **Effect:** Burn on a Bleeding enemy cauterizes it. The enemy stops Bleeding, takes all the stored Bleed as Fire damage at once, and you heal 1 HP. **Seal bonus:** cauterize explodes in a 1.5 m area for 50% of that damage.
- **Trade-off text:** "Blood and fire purge each other; no Covenant, no Pact crest."
- **What you give up:** Martyr / Blood Magic / Ash Covenant / Inferno Heart; Crimson Pact + Pit-Born Flame Crests.
- **Required:** BR-N1 + PY-N1.
- **Power:** very high single-target and boss burst, so it's the boss-killer Hybrid. Cap the stored-Bleed burst at about 3× the hit.

## 5. Keystones vs Hybrid
**Recommendation: BAN.** A Hybrid run has 0 Keystones, and a Keystone run has 0 Hybrids. They share the gold socket.
- **Why:** this is the PoE-style "fork in the road". A Keystone means one Path's identity at its peak; a Hybrid means two Paths fused. Allowing both makes the Hybrid a free bonus, which is exactly what VH does not want.
- **Alternatives considered, and why they were rejected:**
  - *Allow 1 Keystone + Hybrid at a cost of 2 sockets:* too strong once you have 6 sockets, and it blurs the choice.
  - *Allow 2 Keystones:* breaks the "max 1 Keystone" rule and the balance.
- Banish-style Keystones (Necro Covenant, Blood Magic) also stay Pure-only, which avoids strange combos such as a skill-banish Keystone + Grave Conduit.

## 6. Crest interaction
- **Pure Pact:** unchanged. Crest at 4+ nodes from that Path.
- **Hybrid Pact:** **no Crest.** The caps (3 Home + 2 Partner + Hybrid) make 4 from one Path impossible, and the Hybrid node does not count toward any Path's count.
- The Hybrid's **Seal bonus** (§4) stands in for the missing Crest, so a Hybrid run still gets its "complete build" moment once the Hybrid is selected.
- **UI:** in Hybrid mode, the Crest slot shows the Hybrid's glyph (a fused diamond) instead of a Path Crest. Text: "SEALED: <Hybrid>".

## 7. Open questions for VH (max 5)
1. **Exclusivity scope:** is a Hybrid exclusive only *per run* (recommended), or does buying one at the meta level also permanently seal other Hybrids or Paths until a respec?
2. **Socket split:** Home 3 / Partner 2 (recommended), or a free 5 spread across the pair with max 3 per Path?
3. **Keystone ban:** is a full ban OK, or do you want one special "Hybrid Keystone" per pair later (a 7th tier)?
4. **Seal bonus as Crest replacement:** OK, or should Hybrid runs have no Crest-like bonus at all, to make the trade-off harsher?
5. **Meta cost:** is 100 S + 1 Sigil per Hybrid right, and should Hybrids unlock only after the player has owned a Crest once (late game)?
