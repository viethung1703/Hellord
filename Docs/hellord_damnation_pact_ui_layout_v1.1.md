# Hellord: Sacrifice Altar / Damnation Pact UI Layout v1.1 (FOCUS). DESIGN + MOCKUPS ONLY
Supersedes v1.0 (step55). Mockups: `Art/previews/step56_damnation_pact_focus/` (480x270, x3 NN). Player-facing name: Damnation Pact. Never "Ascendancy".

## 0. Rules (locked, unchanged)
- Hub: Sacrifice Altar. Each Path has 8 nodes (N1-N8) and 2 Keystones.
- The player SELECTS at most 6 per run. A Keystone uses 1 of the 6 slots (VH default). Max 1 Keystone per run.
- Two modes, UNLOCK and SELECT. Each has one primary action.
- Relics are deferred.

## 1. Clarity principles
1. **Read:**
   - Type is at least 8 px at 480x270. Titles 16 px, the counter 14 px.
   - Light text on a near-black background.
   - Each screen shows at most about 12 words, not counting node names.
2. **Focus:**
   - One screen has one job and one big primary button, inside the detail panel.
   - The mode toggle and the counter are the only chrome.
   - The breadcrumb is small and dim.
3. **Understand:** three states. Each is told apart by shape plus symbol, not colour alone, so no legend is needed.
   - LOCKED: grey tile with a padlock and a dim label.
   - OWNED: empty dark tile with a thick Path-colour border.
   - SELECTED: gold fill with a check mark.
   - Keystones are diamonds and follow the same three states.
4. **Count:**
   - **SELECTED n/6** is always in the top right, in a big framed counter. It turns red at 6/6 when the player tries to add a 7th.
5. **Hint line:**
   - One sentence in a bottom bar. It changes with the mode or error.
   - Button prompts appear only where an action exists.

## 2. Screens
1. **Overview** (01):
   - Four large Path cards. Each card shows:
     - a sigil
     - the name
     - "x/8 owned"
     - a gold "n selected" chip, or "locked"
   - Hint: "Choose a Path · [A] Open [Y] Your Pact".
2. **Path focus, UNLOCK** (02):
   - Node grid, 2 rows of 4, with the Keystone diamonds in a column on the right.
   - Detail panel: name, one-line effect, requirement.
   - Primary button: **UNLOCK cost**.
   - Hint: "UNLOCK mode: spend souls to own nodes forever".
3. **Path focus, SELECT** (03):
   - Same grid and panel as UNLOCK.
   - Primary button: **SELECT (n+1/6)**. On a node that is already selected, it reads **REMOVE**.
   - Hint: "SELECT mode: pick up to 6 for this run".
   - Locked nodes are unfocusable here: the cursor skips them.
4. **Invalid / full** (04):
   - At 6/6 the counter turns red and the button greys out to "PACT FULL".
   - Hint in red: "6/6 full: deselect one to swap".
   - No popup.
5. **Your Damnation Pact** (05):
   - Six large gold tiles, each with its Path name in that Path's colour.
   - A single line for an active Crest.
   - One big **SEAL PACT** button.
   - Hint: "[A] Seal & descend [B] Back to edit".

## 3. Progressive disclosure (hidden by default)
- **[X] Details** opens a drawer with the full stat text, synergies, prerequisites and Hybrid notes.
- Crest rules, socket growth (2→4→6) and the soul balance show only when relevant:
  - the soul balance shows only in UNLOCK, inside the button cost
  - socket growth shows as a tooltip on the counter
- The tutorial overlay appears on the first visit only, with 3 cards: Unlock, Select, Seal.

## 4. Removed vs v1.0
- The on-screen legend block. States are self-evident now.
- Per-tile text labels such as "[ ] owned", "[x] SELECTED" and "! needs N4". The prereq now appears only in the detail panel.
- The Path summary sidebar, the Crest progress bar and the Hybrid list on the Path screen.
- The soul balance in SELECT mode, plus the extra count lines on overview cards beyond owned and selected.
- Multi-line hints. Only one line remains.
- The secondary buttons row. Only the primary button and the [X] Details prompt remain.

## 5. Main Menu (added; mockup 00)
- **Flow:**
  - **Main Menu → Sacrifice Altar (Damnation Pact) → Path Focus → Your Damnation Pact → SEAL → Run.**
  - DESCEND on the Main Menu starts a run straight away, using the last sealed Pact.
- **Layout:**
  - **Title:** HELLORD.
  - **Currencies:** Souls and Sigils, top right. They show only here and in the UNLOCK cost.
  - **Buttons, stacked:** DESCEND (primary, red), SACRIFICE ALTAR, CODEX, SETTINGS, QUIT.
  - **Right panel:** a read-only "YOUR PACT n/6" preview with 6 slots.
  - **Hint:** "[A] Descend · Pact: Sacrifice Altar".
- **The Main Menu is not the Altar:**
  - The Altar is a separate screen, reached only through its button.
  - Altar screens carry the breadcrumb "Main Menu › Sacrifice Altar …".
  - [B] on the Altar returns to the Main Menu.

## 6. Mockup list (step56_damnation_pact_focus/)
- 00_main_menu_MOCKUP.png
- 01_overview_MOCKUP.png: Sacrifice Altar, Damnation Pact overview with 4 Paths.
- 02_path_focus_unlock_MOCKUP.png
- 03_path_focus_select_MOCKUP.png
- 04_select_full_invalid_MOCKUP.png
- 05_your_damnation_pact_MOCKUP.png: confirm screen, then SEAL PACT starts the run.
- Generator: v11_mockups.py
