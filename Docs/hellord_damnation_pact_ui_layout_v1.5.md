# Hellord: Damnation Pact UI v1.5 (Path ICONS + multi-Path Pact clarity). DESIGN + MOCKUPS ONLY
> **⚠ RULES SUPERSEDED by v1.6.** A Pact is ONE Path only; mixing Paths is NOT allowed. See hellord_damnation_pact_ui_layout_v1.6.md.
Builds on v1.4, keeping its scalable 2-column grid and pages of 6. Mockups: `Art/previews/step60_pact_icons_clarity/`. Generator: `v15_mockups.py`.

## Loadout rule (what the player chooses)
- The player does NOT choose one Path.
- Each run has ONE Damnation Pact, a loadout of up to **6 nodes total, mixed across any Paths**.
- Max **1 Keystone**, and it uses 1 of the 6 slots.
- A **Crest** is earned with 4+ nodes from one Path.
- Owned (unlocked in UNLOCK mode) is permanent. "In pact" is per run.

## Overview card meaning
- **Pips:** 8 per Path.
  - Yellow = this Path's nodes currently in your Pact.
  - Path colour = owned, not in the Pact.
  - Dark = locked.
- **Chip:** "**n in pact**" replaces the ambiguous "picked". If none are in the Pact, it shows "x/8 owned" or "locked".
- **Counter:** the global **SELECTED n/6** badge equals the sum of all "in pact" chips (example: 2+1+0+1 = 4/6).

## Copy changes
| Where | Old | New |
|---|---|---|
| Overview hint | "[A] Open [Y] Your Pact [B] Back" | **"Mix Paths · up to 6 nodes in your Pact"** |
| SELECT button | "SELECT 3/6" | **"ADD TO PACT"** (REMOVE FROM PACT when the node is already in) |
| SELECT hint | "pick up to 6 for this run" | **"Add to Pact · mix Paths · 6 max"** |
| Full Pact | "6/6 full…" | "Pact full 6/6: remove one to swap" |
| Confirm screen | — | **"6 slots · any Paths · 1 Keystone max"**, plus the Crest status ("✦ X CREST active" / "No Crest yet · 4+ from one Path") |

## Path icons (placeholder pixel art; final art later)
- **Size:** 24 px on overview cards (the old letter ring was 16 px). Cards grew only slightly, from 92x36 to 96x40. 16 px in the Path focus header, 20 px in Pact slots.
- **Pyromancer of the Pit:** an orange flame with a yellow core.
- **Bone Warden:** a bone-white skull over crossed bones.
- **Stormcaller:** a grey storm cloud with a cyan lightning bolt.
- **Blood Reaver:** a red blood drop with claw slashes.
- **Locked Paths:** the same silhouette in slate.
- **Future or unreleased Paths:** a "?" card.
- **Pact slots:** show the source Path's icon, a Path-colour border and a short tag, so a mixed loadout is readable at a glance. The Keystone slot is a diamond.

## Mockups (480x270, x3 NN, MOCKUP)
- 01_overview_4paths_MOCKUP.png: icons; 2+1+0+1 in pact = 4/6.
- 02_overview_paged_MOCKUP.png: 6 cards with page dots and LB/RB.
- 03_path_unlock_MOCKUP.png, 04_path_select_MOCKUP.png ("ADD TO PACT").
- 05_your_pact_crest_MOCKUP.png: Pyromancer-heavy with Crest active.
- 06_your_pact_mixed_MOCKUP.png: Pyro x2, Bone x1, Blood x3 (including the Keystone), so no Crest.
