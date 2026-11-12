# Hellord: Sacrifice Altar / Damnation Pact UI Layout v0.9 (SPARSE). DESIGN + MOCKUPS ONLY
> v1.0 RULE UPDATE (VH): 8 nodes per Path (N1–N8) + 2 Keystones; a run selects MAX 6 nodes (max 1 Keystone, counts within the 6; sockets grow 2→4→6). Crest at 4+ from one Path, so effectively 1 Crest per run. Older "8 sockets / 8 per run / 2 Crests" text is superseded. See hellord_damnation_pact_ui_layout_v1.0.md.
- **Supersedes:** v0.7 and v0.8 layout.
- **Still valid from v0.8:** the node rules, the growth rules and the Unlock/Select logic.
- **Naming:**
  - The hub is **Sacrifice Altar**.
  - The system and its tab are **Damnation Pact**.
  - The run confirm screen is **Your Damnation Pact**.
  - Paths: Pyromancer of the Pit, Bone Warden, Stormcaller, Blood Reaver.
  - "Ascendancy" never appears in player-facing text.
- **Mockups:** `Art/previews/step54_damnation_pact_sparse/` (480×270, ×3 nearest-neighbour).

## 1. Sparse rules (hard limits at 480×270)
1. **Interactive cards on screen:** at most 6, plus the bottom bar.
   - Overview: 4 path cards (2×2).
   - Path Focus: 6 nodes (3×2).
   - Hybrids: 2 cards.
2. **Card sizes:** path cards 192×80; node tiles 120×44; hybrid cards 192×140.
3. **Gaps:** at least 16 px between cards. Outer margin is 40 px left and right.
4. **Text per card:** 2 lines at most, a name plus one status ("IN PACT", "owned", "70 S", or "! needs X").
5. **Chrome:** no boxed panels around groups.
6. **Top:** two text lines, with no tab boxes.
   - Line 1: "SACRIFICE ALTAR" (dim).
   - Line 2: "DAMNATION PACT / context" (red).
   - Currencies sit at the top right as text only. Other altar tabs are reached with LB/RB, with a dim hint on screen.
7. **Detail:** a single collapsible bottom sheet (400×36) with name, one effect line and one action button. Y expands it to full text with tags and requirements. It is hidden when nothing is focused.
8. **Loadout bar:** 1 thin line, about 24 px tall.
   - 8 small 12 px sockets, a Keystone diamond and a "6/8" counter.
   - One red error line, shown only when there is an error.
   - DESCEND on the right.
   - Crests show only on the confirm screen and as a glow on the Keystone diamond.
9. **Modes:** the Unlock/Select toggle is gone from the screen. The focused node's single action button is context-aware:
   - Locked and affordable: UNLOCK n.
   - Owned: SELECT or REMOVE.
   - Locked and unaffordable: NEED n.
   - UNLOCK always asks for confirmation. This changes the v0.8 default; VH can revert it.

## 2. Screens
- **01 Overview:**
  - 2×2 path cards, each with a sigil, name and "x of 8 owned". A "* new" marker appears only when something is affordable.
  - "HYBRIDS (n) >" is a text link.
- **02 Path Focus, unlock:**
  - 6 nodes per page.
  - Keystones and later tiers are on page 2, reached with "v more".
  - The bottom sheet shows the focused node.
- **03 Path Focus, invalid:** the node shows "! needs Overheat" in red with a red border. The same reason appears in the bar, and DESCEND is greyed.
- **04 Hybrid drawer:** 2 large cards per page, each with a diamond, a name, two path chips and its status. Pager "< 1/3 >".
- **05 Growth:** paths 5 and up go to page 2 of the overview, 4 cards per page, with pager dots. Nodes 9 and up go to more 6-node pages inside Path Focus.
- **06 Your Damnation Pact:**
  - 8 node cards (4×2), with empty sockets shown as "empty".
  - The Crest line.
  - EDIT and BEGIN buttons.

## 3. Removed to reduce crowding
- The boxed 5-tab bar, replaced by the text title and LB/RB.
- The Unlock/Select toggle bar, replaced by the context button.
- The always-on right detail panel (166 px), replaced by a collapsible bottom sheet.
- Tags, requirement chips and "reshapes" icons, which now appear only in the expanded sheet.
- Progress bars, keystone counters and "in loadout" counts on path cards, replaced by one "x of 8 owned" line.
- Tier labels and the third node row. Keystones move to page 2.
- The Hybrid strip of 6 diamonds, replaced by a text link. The drawer shows 2 cards instead of 6.
- In the loadout bar: the 26 px sockets, the "LOADOUT" label, Crest slots, the PRESET button and the KEY box. Presets move to Y on DESCEND / the confirm screen.

## 4. Critical question for VH
Use one context button in place of the Unlock/Select modes? This is the default here; unlocking always asks for confirmation.

## Mockups
- /workspace/Hellord/Art/previews/step54_damnation_pact_sparse/01_overview_2x2_MOCKUP.png
- /workspace/Hellord/Art/previews/step54_damnation_pact_sparse/02_path_focus_unlock_MOCKUP.png
- /workspace/Hellord/Art/previews/step54_damnation_pact_sparse/03_path_focus_select_invalid_MOCKUP.png
- /workspace/Hellord/Art/previews/step54_damnation_pact_sparse/04_hybrid_drawer_MOCKUP.png
- /workspace/Hellord/Art/previews/step54_damnation_pact_sparse/05_growth_page2_MOCKUP.png
- /workspace/Hellord/Art/previews/step54_damnation_pact_sparse/06_your_damnation_pact_MOCKUP.png
