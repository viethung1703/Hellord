# Hellord — Sacrifice Altar Damnation Pact Panel v0.8 (SPACIOUS + GROWTH) — DESIGN + MOCKUPS ONLY
> v1.0 RULE UPDATE (VH): 8 nodes per Path (N1–N8) + 2 Keystones; a run selects MAX 6 nodes (max 1 Keystone, counts within the 6; sockets grow 2→4→6). Crest at 4+ from one Path, so effectively 1 Crest per run. Older "8 sockets / 8 per run / 2 Crests" text is superseded. See hellord_damnation_pact_ui_layout_v1.0.md.
> SUPERSEDED by hellord_damnation_pact_ui_layout_v0.9.md (sparse layout). v0.7 is also superseded.
This replaces v0.7 §2, the 4-columns-at-once panel. The rest of v0.7 still applies, with these unchanged:
- the main menu
- the title row and 5 equal tabs
- the Your Pact screen
- the HUD
- the node states table
- the art slots

Mockups: `Art/previews/step52_altar_spacious/` (480×270, saved ×3 nearest-neighbour).

## 1. Why change
In v0.7, 4 columns × 10 nodes plus the hybrids were all on one screen. That's about 42 targets, with 24×20 tiles and no room for names.

It can't grow either. A 5th path or a 12th Notable has no space.

## 2. Recommended default: Overview → Path Focus (two levels) + Hybrid Drawer
### 2.1 Level 1: ALL PATHS overview (mockup 01)
- **Path cards:** one large card per path (110×120), in a pager of 4 per page.
  - Each card shows the path sigil, "Notables x/N" with a progress bar, "Keystone x/2", and "In loadout n".
  - When something can be bought, the card shows a "* n unlockable" hint.
- **Hybrid strip:** shows hybrids as "x/N unlocked". Pressing A opens the drawer.
- **Navigation:** LB/RB or "< PAGE 1/2 >" pages through paths. A on a card opens Path Focus.
- **No node tiles here:** the overview is pure orientation.

### 2.2 Level 2: PATH FOCUS (mockups 02 and 03)
- **One path at a time.** "< PYROMANCER >" in the header cycles to the other paths (LB/RB). B goes back to the overview.
- **Nodes are grouped by TIER rows (I, II, III, …)** instead of a dependency web. Each row holds 3 wide tiles (90×26) showing:
  - an icon
  - the full name
  - a status line: IN LOADOUT / OWNED / price / "! NEED N4"
- **Keystones:** a purple row at the bottom.
- **Scrolling:** the list scrolls vertically ("v scroll") when a path has more than 3 tiers.
- **Detail panel:** stays on the right (166×176). It shows the full text, tags, REQ chips, the invalid reason and the main button.
- **Unlock / Select:** both stay as small toggles (86 px each) under the tabs.
  - Keep two modes for now. It's safer for Souls.
  - The one-mode alternative: A selects, and if the node isn't owned the panel offers "UNLOCK 60 S" first. This is noted as a test option.

### 2.3 Hybrid Drawer (mockup 04)
- **Layout:** a full-panel grid of hybrid cards (148×76, 3×2 per page). Each card shows the diamond, the name, two path-colour chips, the cost or OWNED, and its requirement.
- **Filters:** [ALL] [MY PATHS] [UNLOCKABLE].
- **Growth:** it pages as hybrids grow (15 pairs with 6 paths).

### 2.4 Loadout bar (all levels, bottom 44 px)
- **Sockets:** "LOADOUT 6/8", with 8 sockets of 20×20. The KEY socket is 24×24 with a gold border.
- **Crests:** 2 slots. An empty slot shows progress, e.g. "2/4".
- **Error line:** one line of red text.
- **PRESET** and **DESCEND:** DESCEND is grey when the loadout is invalid.
- **Always visible.** You can see the build from anywhere, and selecting a socket jumps to that node's Path Focus.

## 3. Growth rules
| Growth | How the UI absorbs it |
|---|---|
| Paths 4 → 6 | Overview pages (4/page) or a compact 3×2 grid (mockup 05, 148×80 cards). With 7 or more paths, go 3×2 per page plus the pager. Path Focus is unchanged. |
| Nodes 42 → 80+ | Each path gets more tiers (I–V). Path Focus scrolls vertically, 3 tiers visible. Tile size never shrinks. |
| More Notables per tier | Up to 3 per row; with 4 or more, the row wraps to a second line within the tier. |
| More Keystones | The Keystone row scrolls horizontally (2 visible). |
| More hybrids | The drawer pages 6 per page, with filters. |
| New node types (e.g. Crest variants) | New row type in Path Focus; the overview card adds one counter line. |

Hard limits: at least 20 px targets, at least 8 px text, 12 or fewer interactive targets on screen (excluding the loadout bar).

## 4. Alternatives considered
| Option | Pro | Con | Verdict |
|---|---|---|---|
| v0.7 4 columns all-at-once | see everything | crowded; no growth | rejected |
| 2 paths per page (2+2) | comparison | still cramped at 6 paths, half-width tiles | rejected |
| Free zoom / pan tree (PoE) | flashy, infinite | bad on pad; text unreadable at 480×270 | later, maybe for an overview-only map |
| Vertical tab strip of paths | always-visible paths | eats 60 px width | rejected |
| **Overview + Path Focus + Drawer** | big tiles, full names, scales | 1 extra click to reach nodes | **default** |

Fixing the extra click:
- "NEXT UNLOCK →" on the main menu opens Path Focus directly.
- Selecting a loadout socket jumps to Path Focus.
- The last-viewed path is remembered.

## 5. Tabs
Keep the 5 equal tabs. If more meta tabs come later, put DAMNATION PACT, ARSENAL and COMFORT as primaries and move CODEX and RELICS into a "MORE" tab, keeping equal width.

## 6. Critical question for VH (1)
Two modes (Unlock/Select) or one mode (A = select, auto-offer unlock)? The default is two modes.

## Mockups
- /workspace/Hellord/Art/previews/step52_altar_spacious/01_overview_MOCKUP.png
- /workspace/Hellord/Art/previews/step52_altar_spacious/02_path_focus_unlock_MOCKUP.png
- /workspace/Hellord/Art/previews/step52_altar_spacious/03_path_focus_select_invalid_MOCKUP.png
- /workspace/Hellord/Art/previews/step52_altar_spacious/04_hybrid_drawer_MOCKUP.png
- /workspace/Hellord/Art/previews/step52_altar_spacious/05_overview_6paths_growth_MOCKUP.png
