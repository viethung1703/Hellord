# Hellord: Sacrifice Altar / Damnation Pact UI Layout v1.0 (CLARITY FIRST). DESIGN + MOCKUPS ONLY
- **Supersedes:** v0.7, v0.8 and v0.9.
- **Naming:** hub = Sacrifice Altar. System/screen = Damnation Pact. Run confirm = Your Damnation Pact. No player-facing "Ascendancy".
- **Mockups:** `Art/previews/step55_damnation_pact_clear/` (480×270, ×3 nearest-neighbour).

## 0. Rules (VH, locked)
- **Per Path:**
  - **8 nodes, N1–N8.**
  - **2 Keystones**, which are separate from the 8.
- **Selection per run:** **max 6** nodes total. At most **1 Keystone**, and it uses one of the 6 slots.
  - The old "8 sockets + Keystone socket" is gone.
  - Socket growth over the meta: **2 → 4 → 6**.
- **Hybrids:** they also use a slot.
- **Crest:** granted at 4+ selected nodes from one Path. With 6 slots that means **max 1 Crest** per run in practice. A theoretical 2nd would need 4+4, which is impossible.
- **Modes:** keep TWO modes, UNLOCK and SELECT. Each mode has one primary action.

## 1. Clarity rules
1. **Fixed visual language with a legend on every Path screen:**
   - LOCKED: grey fill, plus the price.
   - OWNED: empty dark tile with a path-colour border, "[ ] owned".
   - SELECTED: gold fill, "[x] SELECTED".
   - NEEDS PREREQ: red border, "! needs N4".
   - Text always repeats the state, so colour is never the only cue.
2. **Always-visible count and hint:**
   - **SELECTED n/6** sits in a big counter at the top right.
   - The **mode hint line** sits next to the mode toggle:
     - UNLOCK: "Spend Souls: own forever"
     - SELECT: "Pick up to 6 for this run"
3. **One action per mode:**
   - The bottom detail bar names the focused node and shows its prereq in plain words ("Needs: N4 Overheat (owned)").
   - It has ONE button: UNLOCK 70 S, or SELECT 4/6 / REMOVE.
   - The run loadout strip (6 sockets) and the Crest progress line ("CREST: Pyromancer 3/4") are always at the bottom.

## 2. Screens
### 01 Overview (4 large Path cards, 2×2)
Each card shows:
- the sigil and Path name (Pyromancer of the Pit, Bone Warden, Stormcaller, Blood Reaver)
- a row of 8 pips, one per node, in the same state colours
- "x/8 owned · y selected"
- "A: open"

The screen also has a HYBRIDS > link, the SELECTED n/6 counter and the loadout strip.

### 02 Path Focus: UNLOCK mode
- The legend row.
- **All 8 nodes in a 4×2 grid** (108×40). Each tile shows "N# Name" plus a state line. There's no pager.
- The Keystones row: 2 wide bars with "max 1 per run".
- The detail bar with **UNLOCK 70 S**.

### 03 Path Focus: SELECT mode
- The same grid.
- Locked tiles are shown but can't be chosen.
- The detail bar shows **SELECT 4/6**.

### 04 SELECT invalid
- A selected node whose prereq isn't selected gets a red border and "! N4 not selected".
- The bottom line shows "! N7 needs N4 selected".
- DESCEND is grey.

### 05 Your Damnation Pact
- 6 cards (3×2), with the Keystone marked KEY.
- A Crest banner.
- EDIT / BEGIN buttons.

## 3. Layout (native px)
| Region | Rect |
|---|---|
| Header: Sacrifice Altar / Damnation Pact + context, currencies | 0,0,480,28 |
| SELECTED n/6 counter | 384,18,84,26 |
| Mode toggle + hint | 12,30,~370,16 |
| Legend | 12,52 |
| Node grid 4×2 | 12,68,456,88 |
| Keystones | 12,168,456,28 |
| Detail bar + action | 12,204,456,26 |
| Run loadout (6) + Crest + DESCEND | 0,236,480,34 |

Growth: if a Path ever has more than 8 nodes, add a second 4×2 page ("Tier 2 >"). Paths beyond 4 page the overview, 4 per page.

## 4. Critical question for VH
Should a Keystone use one of the 6 slots (the default here), or get a separate 7th Keystone-only slot?

## Mockups
- /workspace/Hellord/Art/previews/step55_damnation_pact_clear/01_overview_MOCKUP.png
- /workspace/Hellord/Art/previews/step55_damnation_pact_clear/02_path_focus_unlock_MOCKUP.png
- /workspace/Hellord/Art/previews/step55_damnation_pact_clear/03_path_focus_select_MOCKUP.png
- /workspace/Hellord/Art/previews/step55_damnation_pact_clear/04_path_focus_select_invalid_MOCKUP.png
- /workspace/Hellord/Art/previews/step55_damnation_pact_clear/05_your_damnation_pact_MOCKUP.png
