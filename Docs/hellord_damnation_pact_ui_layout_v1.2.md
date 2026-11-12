# Hellord: Damnation Pact UI v1.2 (COMPACT pact + atmospheric Main Menu). DESIGN + MOCKUPS ONLY
Supersedes the v1.1 layout. The v1.1 clarity rules still apply. Mockups: `Art/previews/step57_pact_compact_menu/` (480x270, x3 NN, Endesga 32 palette). Generator: `v12_mockups.py`.

## Rules (unchanged)
- Sacrifice Altar hub. The system is named Damnation Pact. No player-facing "Ascendancy".
- 8 nodes per Path plus 2 Keystones. Max 6 selected per run. A Keystone uses a slot (max 1).
- UNLOCK and SELECT are two modes. Relics are deferred.

## Flow
Main Menu → SACRIFICE ALTAR → Damnation Pact overview → Path → Your Damnation Pact → SEAL PACT → run.
- DESCEND starts a run with the last sealed Pact.

## 1. Compact Pact panel
- **Panel:** one 312x196 panel, centred over the dimmed hell scene. It replaces the full-screen dashboard. The breadcrumb sits above the panel.
- **Header:** one 20 px row with the title and the UNLOCK/SELECT toggle.
- **Counter:** **SELECTED n/6** sits in a badge that breaks out of the top-right corner, so it stays big without wasting a row.
- **Path screen:**
  - 8 nodes as 26 px tiles in 2 rows of 4.
  - A divider, then the 2 Keystone diamonds ("1 max").
  - The detail panel is a strip below the grid, not a side column. It holds:
    - the name
    - a one-line effect
    - a requirement or status
    - the primary button
    - an [X] details prompt
- **Overview:** 4 compact Path cards. Each card shows:
  - a sigil
  - the name
  - an 8-pip bar (gold = picked, Path colour = owned, dark = locked)
  - "x/8 owned"
  - a "n picked" chip
- **Confirm:** 6 slot tiles in one row, a Crest line, and a SEAL PACT plaque.
- **States** are unchanged:
  - LOCKED: grey with a padlock
  - OWNED: dark with a Path-colour border
  - SELECTED: gold with a check
  - Keystones are diamonds
- **When full:** the button greys out to PACT FULL, and the hint turns red: "6/6 full: remove one to swap".

## 2. Main Menu (Hellord flavour)
- **Background:**
  - a sky gradient from ink to blood
  - jagged spire silhouettes
  - a black citadel with lit windows
  - a lava river with an ember glow
  - drifting ember pixels
  - skull piles in the foreground
  - 2 burning braziers with light pools
- **Title:** pixel HELLORD in red with an ink outline and highlight. Horned corners frame it, it sits on a red glow, and the tagline "survive the descent" runs under it.
- **Buttons:**
  - DESCEND is a large blade-shaped plaque with an ember glow, a skull cursor and an arrow.
  - The others are smaller hex plaques: Sacrifice Altar, Codex, Settings, Quit.
- **Currencies:** a plaque in the top right: ✦ Souls and ◆ Sigils.
- **Pact preview:** a ritual circle with a pentagram. It has 6 slot orbs, lit for each picked node, and "PACT 4/6" in the centre. This reads as the Pact, not as a menu.
- **Animation notes** (for implementation later):
  - ember particles rise
  - lava scrolls
  - brazier flames flicker
  - title glow pulses
  - the DESCEND plaque has a heat-shimmer hover
  - Pact orbs pulse

## Mockups
- 00_main_menu_MOCKUP.png
- 01_overview_MOCKUP.png
- 02_path_unlock_MOCKUP.png
- 03_path_select_MOCKUP.png
- 04_select_full_MOCKUP.png (invalid, optional)
- 05_your_damnation_pact_MOCKUP.png

## Known nit
- Greed's Path colour (amber) is close to the "picked" gold. In art polish, shift Greed toward a deeper gold or use a different picked colour.
