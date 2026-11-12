# Hellord: Damnation Pact UI v1.4 (SCALABLE overview). DESIGN + MOCKUPS ONLY
Builds on v1.3 (small cards). The Main Menu is from step57. Mockups: `Art/previews/step59_pact_scalable/`. Generator: `v14_mockups.py`.

## Rules (unchanged)
- Sacrifice Altar hub, Damnation Pact name, no "Ascendancy".
- Real Paths: Pyromancer of the Pit, Bone Warden, Stormcaller, Blood Reaver.
- 8 nodes per Path plus 2 Keystones. Max 6 selected. A Keystone uses a slot.
- UNLOCK and SELECT modes. SELECTED n/6 badge.

## Recommended growth pattern: 2-column grid, up to 3 rows, then pages
- **Cards:** stay at the step58 size, 92x36. Never larger.
- **Panel height grows only with rows:**
  - 4 Paths = 2x2 (panel 200x120)
  - 5–6 Paths = 2x3 (panel 200x160)
- **7+ Paths:** pages of 6.
  - The panel stays a fixed 2x3 height.
  - Page dots sit under the grid, with LB/RB (or ◀ ▶ / mouse wheel) to change page.
  - A partial last page keeps the same panel size.
- **Order:** owned or picked Paths first, then locked, then "future" teasers. The cursor wraps across pages with the d-pad.
- **Teaser card:** a not-yet-released Path shows a slate "?" card that reads "locked". Do not show unreleased names.
- **Path focus and Confirm are unaffected:** each opens one Path, and Confirm lists only the 6 picks. Paging is overview-only.
- **Why this pattern:**
  - It keeps the step58 card size and readability.
  - There is no horizontal scroll to hide cards.
  - One overview layout covers 4, 6 and 12+ Paths.
  - List+detail was rejected: it duplicates Path Focus.

## Mockups (480x270, x3 NN, MOCKUP)
- 01_overview_4paths_MOCKUP.png: today, 2x2.
- 02_overview_6paths_2x3_MOCKUP.png
- 03_overview_8paths_page1_MOCKUP.png: paged, dots and LB/RB.
- 04_overview_8paths_page2_MOCKUP.png
- 05_path_unlock_MOCKUP.png, 06_path_select_MOCKUP.png, 07_select_full_MOCKUP.png, 08_your_damnation_pact_MOCKUP.png: unchanged from v1.3. Path Focus is still fine.
