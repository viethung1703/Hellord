# Hellord: Damnation Pact UI v1.6 (LOCKED RULES: single-Path Pact). DESIGN DOC ONLY
- **Supersedes the rules in v1.5.** v1.5 wrongly allowed mixing Paths.
- **Layout and mockups:** step60 (`Art/previews/step60_pact_icons_clarity/`) is **APPROVED AS PLACEHOLDER**. Hellord Art will polish the icons and art later. Do not involve HA until VH says so.

## LOCKED rules (VH)
1. **One Pact Path at a time.**
   - A Pact belongs to exactly 1 Path.
   - Nodes from different Paths can **never** be in the same Pact.
   - Between runs (meta) the player can add, remove or change nodes, or switch the Pact's Path, at any time.
   - Every selected node must belong to the chosen Path.
2. **Max 6 nodes selected.**
   - A node must be **unlocked first** (Souls, UNLOCK mode) before it can be selected.
   - Locked nodes cannot be selected. In SELECT mode the cursor skips them.
3. **Keystone:** max 1, and it uses 1 of the 6 slots.
4. **Crest:** earned at 4+ selected nodes from the Pact's Path.
5. **Modes:** UNLOCK and SELECT remain two separate modes.
6. **Naming:** the hub is the Sacrifice Altar, and the system is named Damnation Pact. No player-facing "Ascendancy".
7. **Path structure:** 8 nodes per Path plus 2 Keystones. Real Paths: Pyromancer of the Pit, Bone Warden, Stormcaller, Blood Reaver.
8. **Relics:** deferred.
9. **Overview:** the scalable overview from v1.4 stays (2-column grid, 3 rows, pages of 6).

## UI behaviour under the single-Path rule
- **Overview:**
  - Only the **active Pact Path** card shows the yellow "n in pact" chip and yellow pips.
  - The other cards show "x/8 owned" or "locked".
  - The active card gets a "PACT" marker. Placeholder: the yellow chip plus a white focus outline.
- **SELECT on a different Path:**
  - If the Pact is empty, ADD TO PACT sets that Path as the Pact Path.
  - If the Pact already has nodes from another Path, the button reads **SWITCH PACT** and asks for confirmation: "Switch Pact to Bone Warden? Clears 4 nodes." [A] Yes / [B] No.
  - Nodes are never mixed.
- **Counter:** SELECTED n/6 = the number of nodes from the Pact Path.

## Copy corrections vs v1.5
| Where | v1.5 (wrong) | v1.6 |
|---|---|---|
| Overview hint | "Mix Paths · up to 6 nodes in your Pact" | **"One Path per Pact · up to 6 unlocked nodes"** |
| SELECT hint | "Add to Pact · mix Paths · 6 max" | **"Add unlocked nodes · 6 max"** |
| Confirm screen | "6 slots · any Paths · 1 Keystone max" | **"6 slots · one Path · 1 Keystone max"** |
| Locked node in SELECT | — | button greyed out: **"UNLOCK FIRST"** |

- The other copy is unchanged: ADD TO PACT / REMOVE FROM PACT, "Pact full 6/6: remove one to swap", and the Crest line.

## Mockup status (step60)
| Mockup | Status | Note |
|---|---|---|
| 01_overview_4paths | placeholder OK, with a copy fix | It shows "in pact" on 3 Paths (2+1+1). Valid would be only one Path with chips, e.g. Pyromancer 4 in pact = 4/6. Its hint uses the old copy. |
| 02_overview_paged | placeholder OK | Same chip and hint correction as 01. |
| 03_path_unlock, 04_path_select | placeholder OK | Replace the SELECT hint with the v1.6 copy. |
| 05_your_pact_crest | **NOT ALLOWED** | It contains a Bone Warden N1. A valid Pact would be all Pyromancer nodes, e.g. N1–N5 + KEY1. |
| 06_your_pact_mixed | **INVALID / NOT ALLOWED** | It mixes Pyro, Bone and Blood. Kept only as a "what not to do" reference. |
