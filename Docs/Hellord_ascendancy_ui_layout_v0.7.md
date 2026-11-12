# Hellord — Sacrifice Altar & Ascendancy UI Layout v0.7 (DESIGN + MOCKUPS ONLY)
> v1.0 RULE UPDATE (VH): 8 nodes per Path (N1–N8) + 2 Keystones; a run selects MAX 6 nodes (max 1 Keystone, counts within the 6; sockets grow 2→4→6). Crest at 4+ from one Path, so effectively 1 Crest per run. Older "8 sockets / 8 per run / 2 Crests" text is superseded. See hellord_damnation_pact_ui_layout_v1.0.md.
> SUPERSEDED: the system is now named **Damnation Pact**; see hellord_damnation_pact_ui_layout_v0.9.md.
> v0.8 note: the Ascendancy panel layout in §2 is superseded by Hellord_ascendancy_ui_layout_v0.8.md (Overview → Path Focus → Hybrid Drawer).
> Naming: the between-run hub is the **Sacrifice Altar** (name chosen by VH). Structure: **Game menu** (§1) → **Sacrifice Altar** hub with tabs (§2, §5) → **Ascendancy panel** = its ASCEND tab (§2) → Your Pact confirm (§3) → Run/HUD (§4).
Sources: v0.5 (meta menu) and v0.6 (node catalog, with no free nodes). Relics are deferred: their tab is shown greyed.
- **Mockups:** `Art/previews/step51_altar_tabbar/` (480×270 native, saved ×3 nearest-neighbour).
- **Palette:** dark hell UI, Endesga-ish. bg #181425, panel #262b44, edge #3a4466, red #e43b44, gold #fee761, orange #f77622, bone #c0cbdc, storm #0099db, blood #a22633, sigil purple #68386c.

## 0. Global rules
- **Base resolution:** 480×270, the same as the game. Menus use the same pixel grid at integer scale, and there's no fractional zoom.
- **Text:** minimum 8 px pixel font. Body text is at most 22 chars per line inside node detail.
- **Input:** mouse, keyboard and pad.
  - **Keys:**
    - A/Enter/LMB = act (unlock or select).
    - Y/RMB = info pin.
    - Tab/LB-RB = switch tab.
    - X = switch Unlock/Select mode.
    - Start = Descend.
  - Every action has a visible button. Hover is never required, so the menu works for mobile-ish or pad-only play.
- **Hit targets:** at least 20×20 px at native res, so they're readable when scaled.

## 1. Screen: Main Menu (mockup 01)
**Regions:**
1. **Title row (0–14 px):** HELLORD on the left, Souls (gold square + number) and Sigils (purple diamond + SG n) on the right. There are no Altar tabs on the main menu; the Altar is entered with the SACRIFICE ALTAR > button.
2. **Center menu (150–330 x):**
   - Buttons in order: DESCEND (start run, red, default focus), SACRIFICE ALTAR, CODEX, SETTINGS, QUIT.
3. **Left card: Loadout preview.**
   - Shows the 8 sockets (filled or empty), with the Keystone socket in gold.
   - Shows the active Crest(s).
   - Has an [EDIT IN ALTAR] button.
4. **Right card: Last run.**
   - Shows the floor reached, kills, and Souls earned.
   - Lists new discoveries.
   - A "NEXT UNLOCK →" button opens the Altar with the cheapest affordable node focused.

**States:**
- On the first launch, the Altar button shows a lock with the text "Reach floor 1 boss".
- On the first launch, the left card says "Ascendancy locked".

## 2. Screen: Sacrifice Altar — Ascendancy tab (mockups 02 and 03)
### 2.1 Regions (native px)
| Region | Rect (x,y,w,h) | Content |
|---|---|---|
| Title row | 0,0,480,14 | SACRIFICE ALTAR (left) · Souls + Sigils (right) |
| Tab row | 0,15,480,14 | 5 equal tabs, 96 px each: ASCEND · ARSENAL · RELICS (greyed + lock) · COMFORT · CODEX |
| Mode bar | 0,31,480,11 | [UNLOCK MODE] [SELECT MODE] (100 px each) + key hints (LB/RB tab, X mode, A act, Y info) |
| Path columns | 6..332, 44, 4×74, 170 | 4 columns, coloured border per path |
| Hybrid lane | between columns | gold diamonds (6) linking adjacent / far paths |
| Detail panel | 336,44,140,170 | focused node info + primary action button |
| Loadout bar | 0,216,480,54 | 8 sockets, Keystone socket, Crest slots, errors, presets, DESCEND |

### 2.1b Top bar redesign (v0.7b)
- **Old problem:** the long SACRIFICE ALTAR title, 5 tabs and both currencies were packed into one 18 px bar. The tabs got pushed right and crowded.
- **New layout:** two thin rows.
  - Row 1 (14 px) is identity and wallet: the title on the left, currencies on the right.
  - Row 2 (14 px) is navigation: 5 equal-width 96 px tabs that fill the whole width.
  - The Unlock/Select mode bar sits under the tabs (11 px).
- **Why this layout:**
  - Equal tabs give stable pad focus (LB/RB) and big touch targets.
  - Each label fits with room to spare (7 chars max = 42 px out of 96).
  - The currencies never move.
  - The title can be long without pushing anything.
- **Rejected alternatives:**
  - A vertical tab strip would eat about 60 px of width the 4 path columns need.
  - 3 tabs plus overflow would hide Comfort/Codex behind an extra click.
- **Cost:** about 10 px of extra height, taken from the path columns (176 → 170). Tile spacing goes from 30 to 29 px.
- **Tab states:**
  - Active: red fill, gold border.
  - Idle: panel fill.
  - RELICS: dim text plus a lock box, not selectable.
  - Later, a tab with something to unlock shows a 3 px gold dot (top-right).

### 2.2 Path column layout
- **Header:** the path name in its path colour.
- **Grid:** a 2-wide × 4-high grid of Notable tiles (24×20).
  - The left column holds the main chain: N1 → N2 → N4 → ...
  - The right column holds side branches.
  - Thin lines between tiles show prerequisites.
  - N1 sits at the top-left as the entry node. It has a Souls price, like every other node.
- **Keystone seals:** 2 sigil-diamond seals at the bottom, purple with a gold rim and a "K".
  - Locked seals show "SG" plus the Souls price.
  - The label "SG seal" sits under them.
- **Hybrid diamonds:** placed on the borders between columns.
  - Non-adjacent pairs (Pyro–Storm, Pyro–Blood, Bone–Blood) use a small diamond in the lane, with a coloured dot for each linked path.
  - Hovering a diamond highlights both parent paths.

### 2.3 Node tile states
| State | Fill | Border | Overlay |
|---|---|---|---|
| Locked (can't afford / prereq not unlocked) | near-black | grey edge | price "40" in grey |
| Unlockable (affordable + prereqs unlocked) | panel | path colour, slow pulse | price in gold |
| Unlocked (owned, not selected) | panel | path colour | glyph icon |
| Selected (in loadout) | path colour | gold | glyph icon, socket number |
| Prereq missing (selected but prereq not selected) | panel | red | red "!" |
| Keystone sealed | purple diamond | grey | "SG" + lock |
| Keystone available | purple diamond | gold pulse | "K" |
| Keystone selected | gold diamond | white | "K" |

Colour is never the only signal: each state also has an icon, number or "!", so it stays readable for colour-blind players.

### 2.4 Detail panel (right)
- **Header:** the node name and ID, e.g. KINDLING PY-N1, in the path colour.
- **Tier and cost:** e.g. "Notable · 30 S", or "Keystone · 150 S + 1 SG".
- **Effect text:** 3–5 lines, wrapped.
- **TAGS chips:** Zone, Burn, Orbital, ...
- **REQ chips:**
  - Green = met.
  - Red = missing. Clicking a red chip focuses that node.
- **RESHAPES:** icons of the affected skills and supports.
- **Primary button (bottom), by context:**
  - Unlock mode: "UNLOCK 30 S" in red. Greyed with "NEED 12 S" when you can't afford it.
  - Select mode: "SELECT (n/8)" in green, or "REMOVE" when the node is already selected.
- **Pin:** Y pins the panel so you can compare nodes. A second press shows a 2-node compare.

### 2.5 Modes
**UNLOCK mode (default when you have Souls ≥ the cheapest unlockable price)**
- Only unlockable tiles pulse.
- Pressing the button spends currency, then a confirm pops up: "Unlock Kindling for 30 Souls? [YES] [NO]". It's permanent and can't be refunded.
- Feedback: a 0.4 s ember burst, the tile flips to Unlocked, and the currency counts down.

**SELECT mode**
- Locked tiles dim to 40%.
- Clicking an unlocked tile puts it into the next free socket.
- Clicking a selected tile removes it.
- Keystones only go into the gold Keystone socket, at most 1. Selecting a second Keystone swaps it, after a prompt.

### 2.6 Loadout bar (bottom 54 px)
- **"LOADOUT" label + 8 sockets** (26×26).
  - Sockets 1–7 take Notables or Hybrids.
  - Socket 8 is the **Keystone socket**, with a gold border and the label "KEY". It may also hold a Notable if no Keystone is chosen.
  - Locked sockets (meta growth 2/4/6/8) show a chain icon and the unlock condition.
  - Each filled socket shows the node glyph on its path-colour fill.
- **CRESTS:** 2 slots.
  - A slot lights up in the path colour with initials (PY/BW/ST/BR) when 4 or more nodes from one path are selected.
  - A progress hint appears under an empty slot: "BW 2/4".
- **Error line (red, 1 line):** shows the first invalid reason. Pad users can cycle through the reasons. Possible reasons:
  - "! Hybrid Grave Conduit needs ST-N1 selected"
  - "! N4 needs N2 selected"
  - "! Only 1 Keystone"
  - "! Socket locked (reach floor 2 boss)"
- **PRESET 1 2 3:** save or load whole loadouts.
- **DESCEND:**
  - Green when the loadout is valid. Grey and disabled when it's invalid, with the hover/focus text "Fix 1 issue".
  - Empty sockets are allowed: a run with fewer than 8 nodes is valid.

### 2.7 Navigation flow
Main Menu → Sacrifice Altar (Ascend tab) → DESCEND → **Run-start confirm** → Run.

Pressing B/Esc anywhere goes back one step (from the Altar it returns to the Main Menu). The Altar auto-saves selections.

## 3. Screen: Run-start confirm, "YOUR PACT" (mockup 04)
- **Node cards:** a row of 8 cards (52×80), with the Keystone card gold-framed. Each card shows its glyph, name and tier.
- **Crest card(s):** name plus a 1-line effect.
- **Meters box:** "METERS THIS RUN: HEAT/ENERGY/FRENZY", listed only when selected nodes use them, so the player knows what new HUD bars to expect.
- **Buttons:** [EDIT] goes back to the Altar. [BEGIN] in red is the default focus.

## 4. In-run HUD (mockup 05)
- **Glyph row:** under the HP and XP bars, 8 glyphs (8×8) in socket order. The Keystone glyph has a purple tint, and a Crest glyph follows at the end.
  - A glyph flashes gold for 0.2 s when its node procs, e.g. Cast on Shock.
- **Node meters:** Heat/Energy/Frenzy bars (60×4) appear under the glyph row only if a selected node uses them. Never more than 2 are shown.
- **Not shown in-run:** no node text, no menus, no changes.
- **Pause → Build panel:** the Ascendancy column lists the 8 nodes with full text. It's read-only.

## 5. Other Altar tabs (layout stubs)
- **ARSENAL:** a grid of skills and supports with a lock or price, using the same tile states and detail panel.
- **COMFORT:** a short vertical list of capped upgrades, each with a level pip bar.
- **RELICS:** greyed with the label "Coming later".
- **CODEX:** list on the left, entry detail on the right.
- **Shared:** all tabs reuse the Top bar, the Detail panel and the tile states. That's 1 widget set to build.

## 6. Glyph and art slots
- **Node glyphs:** 42 at 16×16. In-run HUD versions are 8×8 downscaled hand-cleaned.
- **Crest icons:** 4 at 16×16. Keystone seal frame 24×16 ×3 states.
- **Path headers:** 4 (text + 1 sigil each, 12×12).
- **Frames:**
  - Tile frame states: 8.
  - Socket frame: normal, keystone and locked.
  - Diamond: 3 states.
- **Animation:**
  - Ember burst on unlock (6 frames).
  - Pulse on unlockable tiles (palette cycle, no extra frames).

## 7. Defaults proposed (no VH decision needed unless he objects)
- Unlock and Select are two explicit modes, not one combined click, to avoid spending Souls by accident.
- Socket 8 is the Keystone socket but can hold a Notable.
- Hybrid diamonds sit in the lanes between columns, not in a separate row.
- Detail panel on the right, always visible. No floating tooltips.

## 8. Critical question for VH (1)
1. Keep two modes (Unlock / Select) as tabs, or use one mode where the first click unlocks and later clicks select? The default proposal is two modes, because it's safer for Souls.

## Mockups
- `/workspace/Hellord/Art/previews/step51_altar_tabbar/01_main_menu_MOCKUP.png`
- `/workspace/Hellord/Art/previews/step51_altar_tabbar/02_ascendancy_unlock_MOCKUP.png`
- `/workspace/Hellord/Art/previews/step51_altar_tabbar/03_ascendancy_select_invalid_MOCKUP.png`
- `/workspace/Hellord/Art/previews/step51_altar_tabbar/04_run_start_confirm_MOCKUP.png`
- `/workspace/Hellord/Art/previews/step51_altar_tabbar/05_inrun_hud_glyphs_MOCKUP.png`
