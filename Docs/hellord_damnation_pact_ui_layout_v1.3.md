# Hellord: Damnation Pact UI v1.3 (SMALL cards). DESIGN + MOCKUPS ONLY
Supersedes the v1.2 pact screens. The v1.2 Main Menu is kept. Mockups: `Art/previews/step58_pact_cards_small/`. Generator: `v13_mockups.py`.

## Rules (unchanged)
- Sacrifice Altar hub, Damnation Pact name, no "Ascendancy".
- 8 nodes per Path plus 2 Keystones. Max 6 selected. A Keystone uses a slot.
- UNLOCK and SELECT modes. Compact panel over the dimmed scene.

## Paths (real names; Wrath, Greed, Plague and Sorrow were placeholders)
| Path | Colour | Short tag |
|---|---|---|
| Pyromancer of the Pit | orange | PYROM |
| Bone Warden | bone | BONE |
| Stormcaller | blue | STORM |
| Blood Reaver | red | BLOOD |

## Overview: small cards
- **Panel:** 200x124 px (v1.2 was 312x196, about 40% of the area), with wide empty margins of the hell scene around it.
- **Cards:** a 2x2 grid of 92x36 px cards. v1.2 cards were 70x130 px, so each card is about 64% smaller in area and 72% shorter.
- **Each card holds:**
  - an emblem ring with the Path's initial
  - the Path name on 2 lines
  - 8 pips (yellow = picked, Path colour = owned, dark = locked)
  - a "n picked" chip, or x/8, or "locked"
- **Locked Paths:** the whole card turns slate grey.
- **Picked colour:** now YEL (#fee761), which separates it from the orange/bone Path colours. This fixes the v1.2 nit.

## Path focus: tightened
- **Panel:** 272x170 (was 312x196).
- **Tiles:** nodes and Keystones are 22 px (were 26).
- **Detail strip:** 48 px tall (was 56).
- **Title:** shortened to "PYROMANCER". The full name stays in the breadcrumb.
- **Counter:** the big n/6 badge is kept.

## Confirm screen
- Same 272x170 panel.
- 6 picks with short Path tags, a Crest line, and SEAL PACT.

## Mockups
- 00_main_menu_MOCKUP.png (copied unchanged from step57)
- 01_overview_MOCKUP.png
- 02_path_unlock_MOCKUP.png
- 03_path_select_MOCKUP.png
- 04_select_full_MOCKUP.png
- 05_your_damnation_pact_MOCKUP.png
