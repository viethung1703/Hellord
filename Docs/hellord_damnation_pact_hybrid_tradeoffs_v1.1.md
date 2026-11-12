> **REJECTED by VH, Oct 31 2026: Paths only.** No Combos, no hybrids, no prebuilt kits. Damnation Pact = pick ONE Path per run, then pick up to 6 of its nodes (max 1 Keystone, Crest at 4+). Players find their own builds. This doc is kept for history only.

# Hellord — Damnation Pact: Hybrid Seals v1.1 (DESIGN ONLY, for VH approval)
Supersedes v1.0 (the Home 3 + Partner 2 multi-Path mix is REMOVED). Base: Hellord_ascendancy_nodes_full_v0.6.md. No code, no version bump.

## 0. Locked rules (VH)
- Every run picks exactly ONE Pact choice: **one Path** OR **one Hybrid Seal**. There are no multi-Path socket loadouts, and Notables from two Paths are never mixed.
- **Pure Pact:** 1 Path, up to 6 nodes from that Path only, max 1 Keystone, Crest at 4+.
- **Hybrid Seal:** the Seal is the build's identity. You don't climb any Path tree that run.

## 1. Chosen model: (B) the Hybrid Seal IS your one Pact choice
At the start of a run, the Pact screen offers 4 Paths + the Seals you have unlocked, all **side by side as equal choices**. Choosing a Seal means:
- **0 Path nodes:** no N1–N8, no Keystone and no Crest from any Path. The sockets don't hold Path nodes.
- **Instead, the Seal has 3 fixed Ranks** that light up as your sockets grow (2 → 4 → 6):
  - **Rank I** (socket 2): the core fusion effect.
  - **Rank II** (socket 4): the Seal's "Crest", a passive that rounds out the build.
  - **Rank III** (socket 6): the Seal's "Keystone", a big build-defining power with a drawback.
- **No choices inside a Seal.** That is the trade-off:
  - A Pure Path offers **flexibility**: 6 picks out of 10, your own Keystone, and tuning for the run.
  - A Seal offers **guaranteed fusion power** but a **fixed script**. You never touch the parent Paths' Notables, Keystones or Crests that run.
- **Why B over A and C:**
  - **A (Seal only, flat)** gives no growth with sockets, so the Seal feels dead in the mid-game.
  - **C (Home Path + Seal, deep nodes banned)** brings back the two-Path confusion and feels like a nerfed Path.
  - **B** keeps ONE choice per run, uses the same 2/4/6 socket growth players already know, and maps cleanly to PoE ("pick your ascendancy").
- **The Pact pairs are flavour only.** They set what the Seal's skills and supports care about (Orbital+Burn, Chain+Burn and so on). They are **not** requirements to slot anything.

### Loadout summary
| | Pure Path | Hybrid Seal |
|---|---|---|
| Choice per run | 1 Path | 1 Seal |
| Node picks | up to 6 from that Path (player chooses) | none: Ranks I/II/III auto at 2/4/6 sockets |
| Keystone | max 1, from that Path | **banned** (Rank III is the Seal's own Keystone) |
| Crest | that Path's Crest at 4+ | **banned** (Rank II is the Seal's own Crest) |
| Other Paths / Seals | locked | locked |
| Respec mid-run | no | no |

## 2. Unlock (meta)
- **Cost:** 120 Souls + 1 Sigil for each Seal. That is a little more than a Keystone, because it is a whole build.
- **Requirement:** you must have earned **both parent Crests at least once** in earlier runs (i.e. finished a run with 4+ nodes in each parent Path). You have to master both Paths before you can fuse them, like PoE's "you have to know the class".
- Unlocking a Seal does not use or lock any Path nodes you own. It only adds a choice to the Pact screen.
- **UI:** Seals sit in a row under the 4 Path columns as diamonds, each linked to its 2 parent columns. Locked Seals show their Crest requirement.

## 3. The 6 Hybrid Seals
Every Seal shows: **"Seal chosen: no Path nodes, no Keystone, no Crest this run."**

### Burning Bones — Seal of the Kiln Ossuary (Pyro × Bone)
- **Rank I:** orbitals ignite on hit. A Burning kill regrows a broken bone instantly (max 1 per 0.2 s).
- **Rank II:** +1 orbital bone. Orbitals deal +30% damage to Burning enemies, and Burn spreads its remaining duration on kill.
- **Rank III (Keystone-type):** all your projectiles become orbiting embers for 0.5 s before they launch. **Drawback:** −25% projectile speed.
- **Trade-off text:** "Your bones burn forever. You will never walk the Pit or the Ossuary to their ends."
- **You give up:** all Pyro and Bone Notables, Ash Covenant, Inferno Heart, Sloth Throne, Necro Covenant, Pit-Born Flame, Ossuary Lord.
- **Power:** high. It's an easy, strong clear build and the best Seal to start with.

### Plasma Storm — Seal of the Violet Tempest (Pyro × Storm)
- **Rank I:** Shocking a Burning enemy makes it Plasma: Burn ticks chain 1 lightning jump (max 1 per tick per enemy).
- **Rank II:** Plasma jumps also apply Burn, and Shock lasts +50% on Burning enemies.
- **Rank III:** every 4th trigger is free, and free triggers apply Burn + Shock. **Drawback:** your fire and lightning hits deal −20% damage to targets that aren't Burning or Shocked.
- **Trade-off text:** "Fire and storm become one; you will never wear the Tempest's Eye or the Pit's crown."
- **You give up:** all Pyro and Storm Notables, 4 Keystones, 2 Crests.
- **Power:** very high in big packs and weaker on a lone boss.

### Blood Marrow — Seal of the Drinking Bone (Bone × Blood)
- **Rank I:** orbital hits apply Bleed. Bleeding enemies hit by orbitals lose 1 bone's worth of HP.
- **Rank II:** orbital LifeLeech ×2. Overheal becomes a Blood Shield (max 20% HP).
- **Rank III:** each bone that breaks bursts and heals you 2% max HP. **Drawback:** bones regrow 40% slower.
- **Trade-off text:** "The marrow drinks; you will never be Martyr nor Lord of bones."
- **You give up:** all Bone and Blood Notables, Sloth Throne, Necro Covenant, Martyr, Blood Magic, both Crests.
- **Power:** high sustain with medium damage. It's the safest Seal.

### Grave Conduit — Seal of the Buried Storm (Bone × Storm)
- **Rank I:** Chain Lightning can chain through your minions and zones, and each one re-hits around itself.
- **Rank II:** +1 chain. Every zone you create spawns 1 temporary bone that can be chained.
- **Rank III:** every 20 kills raise a skeleton conduit (max 3, 15 s) that casts your Chain Lightning. **Drawback:** your own Chain Lightning casts have −1 jump.
- **Trade-off text:** "The dead carry your storm; Tempest and Throne are sealed."
- **You give up:** all Bone and Storm Notables, 4 Keystones, 2 Crests.
- **Power:** high once it's set up. It starts slow and is strong late.

### Blood Lightning — Seal of the Red Bolt (Storm × Blood)
- **Rank I:** each chain jump heals 0.5 HP, and each cast costs 0.5% max HP. Jumps on low-HP enemies apply Bleed.
- **Rank II:** under 50% HP, chains get +1 jump and +20% damage.
- **Rank III:** chain casts have no cooldown and cost 2% max HP each instead (0.1 s minimum gap). **Drawback:** you can't heal above 80% HP.
- **Trade-off text:** "Your blood rides the lightning; Blood Magic and the Eye are forsaken."
- **You give up:** all Storm and Blood Notables, 4 Keystones, 2 Crests.
- **Power:** medium-high. It's a risky, skill-based glass-cannon build.

### Crimson Pyre — Seal of Cautery (Blood × Pyro)
- **Rank I:** Burn on a Bleeding enemy cauterizes it: all stored Bleed is dealt at once as Fire damage (capped at 3× the hit), and you heal 1 HP.
- **Rank II:** each cauterize explodes in a 1.5 m area for 50% of its damage.
- **Rank III:** a cauterize on an elite or boss marks it for 4 s, and your hits on a marked target apply Bleed + Burn. **Drawback:** Burn on enemies that aren't Bleeding deals −30% damage.
- **Trade-off text:** "Blood and fire purge each other; no Covenant, no Pact crest."
- **You give up:** all Blood and Pyro Notables, Martyr, Blood Magic, Ash Covenant, Inferno Heart, both Crests.
- **Power:** very high single-target. It's the boss-killer Seal.

## 4. Keystone ban
**Full ban.** A Seal run can't select any Path Keystone, and a Path run can't select any Seal. Rank III is the Seal's own Keystone-level power, with its own drawback, so a Seal never stacks on top of a Keystone.

## 5. Crest ban
**Full ban.** A Seal run gets no Path Crest, because it has 0 Path nodes. Rank II stands in for it.
- **UI:** in a Seal run, the Crest slot shows the Seal glyph with the text "SEALED".

## 6. Balance target
A Seal at Rank III should equal roughly a 6-node Path + Keystone + Crest in raw power. It is weaker in flexibility, because there are no picks inside it.
- Tune by comparing clear time and boss time against the 4 sample builds in v0.6.

## 7. Open questions for VH (max 5)
1. Is the Seal unlock condition OK ("own both parent Crests once" + 120 S + 1 Sigil), or should it be Souls only?
2. Should Seal Ranks stay fully fixed (recommended), or give 1 small choice at Rank II (2 options)?
3. Are the Rank III drawbacks OK, or should Seals have no drawbacks because they already lose flexibility?
4. Should the Pact screen show Seals alongside the 4 Paths from the start, or only after the first Seal is unlocked?
5. Do Seals need their own Pact pixel-art glyphs (6 icons + 6 Rank frames) for v0.0.54, or should they reuse the v0.6 hybrid glyphs?
