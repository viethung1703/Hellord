# Hellord menu audio map (audit for Hellord Sound, 2026-11-01)
Source: Assets/Hellord/Scripts/Runtime. Code audit only. No audio added, nothing play-tested.

## 0. Game modes (HellordGame.Mode)
`MainMenu, Altar, StartPick, Play, LevelUp, Chest, Transition, Dead, Won`.
- Boot: `PactBoot51()` (HellordPact.cs) goes to **MainMenu** when `pactMainMenu51` is on and the game was not started with `-hellordAutotest`. Otherwise it goes straight to **StartPick**.
- The menus are IMGUI (OnGUI), drawn in `HellordUI.cs` (dispatch) → `DrawPact51()` (menu and altar) or `DrawCardsB / DrawDeathB / DrawWonB` (HellordScreens.cs).
- Menu logic runs in `PactTick51()` (HellordPact.cs).
- There are NO Unity scenes per menu. Everything is one scene with a mode switch, so music and SFX must hook on **mode changes**.

## 1. MAIN MENU (HellordMainMenu69.cs, v69 art, `PT.mainMenu69` = on)
These are the rows, top to bottom (`Rows69`), handled by `Act69(i)`:

| # | Label | Action |
|---|---|---|
| 0 | DESCEND | `Descend51()` → mode StartPick (choose starting skill), and applies the Pact |
| 1 | SACRIFICE ALTAR | `OpenAltar53()` → mode Altar |
| 2 | CODEX | `Msg51("CODEX: COMING SOON")` (toast only) |
| 3 | SETTINGS | `Msg51("SETTINGS: COMING SOON")` (toast only; no settings menu exists) |
| 4 | QUIT | `Application.Quit()` |
| 5 | Pact SEAL (right side, not a row) | soft press brighten for `mm69PressS` = 0.45 s, then `OpenAltar53()` (`mm69SealOpensAltar`). If that is off, a PLACEHOLDER "YOUR DAMNATION PACT" panel opens ([A] OPEN SACRIFICE ALTAR, [B] BACK, click outside = close) |

Also on the main menu:
- 6 socket orbs show a tooltip on hover only.
- The top-right shows the Souls and Sigils count.
- Fallback when `mainMenu69` is off: `DrawMainMenu51` (old hex buttons; Space = Descend).

## 2. SUBMENUS
**Sacrifice Altar** (mode Altar; HellordAltar97.cs plus Altar130 for the Necromancer tree; `_altarScr53` is the screen):
- **scr 0, Path carousel**: 4 paths (PYROMANCER, NECROMANCER, STORMCALLER, BLOOD REAVER).
  - Left/right or the side arrows: `Scroll97(±1)`.
  - Buttons: OPEN [A] → scr 1; YOUR PACT [Y] (or Tab) → scr 2; BACK [B] → MainMenu.
  - Clicking the centre portrait → scr 1.
- **scr 1, Node tree** of the viewed path.
  - Directional keys move the node focus; clicking a node focuses it.
  - Primary button [A]: UNLOCK <cost> / SIGN / REMOVE / LOCKED (`Primary97`; a failure shows a `Msg51` toast such as "NOT ENOUGH SOULS / SIGILS").
  - Second button: PACT [Y] → scr 2, or CHOOSE PATH [Y] (`ChoosePath97`), or LOCKED.
  - BACK [B] → scr 0.
- **Confirm dialog** (`_confirm97`, shown when switching path while nodes are signed): YES (`ConfirmYes97`, wipes the pact and switches path) / NO. Keys: confirm = yes, back = no.
- **scr 2, YOUR DAMNATION PACT** (`DrawYourPact53`, HellordPact.cs):
  - Slots, crest line.
  - SEAL PACT button / Space → `Descend51()` (seal and descend into the run).
  - EDIT / Esc → scr 0.
- An old placeholder altar exists (`DrawAltar51`, when `altar97` is off), with the same flow and YES/NO switch-path.

**StartPick** ("HELLORD / CHOOSE YOUR STARTING SKILL", `DrawCardsB`): pick with keys 1–n or a mouse click → mode Play.

**LevelUp** ("LEVEL N! / PICK ONE", `OpenLevelUp()` HellordCards.cs):
- Keys 1–5 or click: pick.
- SPACE: reroll (`Reroll`).
- ESC: skip for +10 bones (`SkipCards`).
- For support cards with several valid skills, a **Target pick** sub-screen opens (`OpenTargetPick`, HellordV36.cs): keys 1–n or click to pick, ESC to go back.

**Chest** (CHEST / "TAKE 1 SUPPORT", or a boss reward "NEW SKILL SLOT"): same card UI. ESC skips (not allowed on a boss reward).

**Shrine shop** (in Play): the purchase uses the target pick above.

**Dead** ("YOU DIED", `DrawDeathB`) and **Won** ("RUN COMPLETE", `DrawWonB`):
- Show the run stats and the souls and sigils earned.
- The only action is **R = RESTART**: `ResetRun()` + `PactAfterReset51()` → back to MainMenu.

**Transition**: a floor fade between floors. No input.

**Test menu F1** (HellordTestMenu.cs, `testMenuEnabled`; also a "TEST F1" button bottom-left):
- Available in Play, StartPick, MainMenu and Altar. It opens on the PACT tab when you are on the menu.
- Tabs: FLOOR, PLAYER, SKILLS, SUPPORTS, ENEMIES, BOSS, PACT, KEYS (Unity GUI.Button lists).
- While it is open the game is paused (the `TestMenuUpdate` early return).

**Map**: M / F10 calls `FloorRevealMap()` (a reveal, not a menu).

**Pause menu**: DOES NOT EXIST. No Esc-pause in Play. The only "pause" states are the LevelUp/Chest screens, the open test menu and `Time.timeScale == 0` from HitStop.

## 3. NAVIGATION / INPUT
- **Menu and altar** use `K69Down(which)` + `Nav69()` (HellordMainMenu69.cs). Each works with both the new Input System and legacy input.

  | Action | Keyboard | Gamepad / other |
  |---|---|---|
  | Up / Down / Left / Right | arrows or WASD | d-pad; left stick edge at ±0.6 |
  | Confirm | Enter / NumpadEnter / Space | South (A) / Start (legacy: JoystickButton0) |
  | Back | Esc / Backspace | East (B) (legacy: JoystickButton1) |

  - The main menu rows wrap around with up/down. Right moves to the seal; left, up or down moves back to the rows.
  - Mouse: hovering a row selects it (`MouseHover69`), clicking activates it (`Click53`). Hovering the seal brightens it.
  - Altar Tab = YOUR PACT.
- **Cards**: keys 1–5, Space (reroll), Esc (skip/back). Mouse hover highlights a card; a click picks it, but only after the 0.45 s slide-in lock (`FX26CardsLocked`). No d-pad navigation for cards.
- **End screens**: the R key only.
- **Test menu**: F1 toggles it; the rest is mouse only.

## 4. EXISTING AUDIO HOOKS
**System** (HellordAudio.cs, one DontDestroyOnLoad object):
- `HellordAudio.Play(hook, pitch)` plays from 24 pooled 2D voices. Clips come from `Resources/Audio/SFX/<folder>/<hook>_NN`.
- Per-hook volume and pitch ranges, a cap and a minimum gap are registered in `Awake()` via `Add(...)`.
- Also: `Stop`, `PlayDelayed`, `DuckMusic(db, dur)`, `XpPickup`, `HeroDeath`, `TickHeart` (heartbeat, and it also drives `TickMusic`), `RoomEnterScream`.
- Switches: `HellordTuning.sfxEnabled`, `sfxVolume` 0.35, `musicEnabled`, `musicVolume` 0.3, `musicPausedMul` 0.5.

**UI/menu hooks that exist today:**

| Hook (clip folder events/) | Where it is called | When |
|---|---|---|
| `ui_card_select` (3 variants) | HellordScreens.cs `DrawCardsB` line ~52 | mouse hover moves onto a new card (StartPick, LevelUp, Chest) |
| `ui_card_select` | HellordCards.cs `UpdateCardKeys` | key 1–5 card pick |
| `ui_card_select` | HellordV36.cs target pick: hover (~250) and pick (`PickTarget`, ~231) | |
| `ui_upgrade_confirm` | HellordCards.cs `Choose()` (~line 102) | every card choice applied |
| `levelup_sting` + `DuckMusic(4 dB, 1.5 s)` | HellordCards.cs `OpenLevelUp()` | level-up screen opens |
| `chest_open` | HellordV37.cs ~469, HellordFloor.cs 397, HellordGame.cs 801 | chest / boss reward screen |
| `shop_purchase` | HellordFloor.cs 661 | shrine shop buy |
| `shrine_use` | HellordFloor.cs 392 | |
| `banner_whoosh_floor` / `banner_whoosh_boss` | HellordV26Systems.cs ~42 | floor and boss banners |
| `room_cleared_sting`, `door_unlock` (delay 0.2 s) | HellordFloor.cs 509 | |
| `hero_death` (via `HeroDeath()`, which stops all voices) | HellordGame.cs 482 | entering Dead |

**MAIN MENU / ALTAR / END SCREENS: ZERO audio calls.** There is no sound in HellordMainMenu69.cs, HellordAltar97.cs, HellordAltar130.cs, HellordPact.cs, HellordPact53.cs or HellordTestMenu.cs. Hover, confirm, back, seal press, unlock, sign, path switch, toasts and restart are all silent.

**Music (TickMusic, HellordAudio.cs)**: two crossfading looping sources.
- Floor playlists per theme (`FloorPlaylists53`: Ox Pit, Frozen, Burning, Mortar; rotation `musicRotate53`). Boss tracks via `BossTrack` (5 clips, bar-synced switches).
- It plays only when `inRun = (mode == Play) || paused`, where paused = LevelUp, Chest, `timeScale == 0` or the test menu open (`TickHeart` call at HellordGame.cs:204). In those paused states the volume is ×0.5.
- In **MainMenu, Altar, StartPick, Transition, Dead and Won** the target volume is 0, so it fades out over about 1 s and stops.
- **There is no menu music track and no menu music code.** The existing music files are Music/music_floor1_combat, Music/Floor/* (8) and Music/Boss/* (5).

## 5. RECOMMENDED ATTACH POINTS
**Music:**
- **Menu music start**: entering MainMenu, at `PactBoot51()` and `PactAfterReset51()` (HellordPact.cs 101–102), at the Altar BACK paths (`mode = Mode.MainMenu` in HellordAltar97.cs 106/387, HellordPact.cs 230/336), and on restart from Dead/Won.
  - Cleanest single point: in `TickMusic`/`TickHeart`, add a `menu` flag (mode == MainMenu || Altar, optionally StartPick) that selects a menu track on the same crossfade sources. `TickHeart` already receives the mode state from HellordGame.cs:204, so pass `mode == Mode.MainMenu || mode == Mode.Altar`.
  - Altar: keep the menu track playing (same loop) or crossfade to an altar variant when `_altarScr53` changes.
- **Menu music stop**: `Descend51()` (HellordPact.cs 191; mode becomes StartPick). Crossfade to the floor loop, or fade out over StartPick and let the floor music start at mode Play (current behaviour).
- **Death/Won**: currently silent after the `hero_death` sting and the fade. Optional: a short death or victory stinger, or the menu track after it, in `DrawEndBox` mode / at the `mode = Mode.Dead` (HellordGame.cs 482) and `mode = Mode.Won` (HellordFloor.cs 413) sites.

**SFX (new hooks to register in `HellordAudio.Awake`):**

| Proposed hook | Attach point |
|---|---|
| `ui_hover` (menu row move) | `MenuTick69` when `_sel69` changes by keys/stick; `MouseHover69` when `_sel69` changes; altar `_focus97` changes in `AltarTick97` and on click |
| `ui_confirm` | `Act69(i)` cases 0, 1 and 5; altar OPEN/YOUR PACT buttons (`Btn97`) |
| `ui_back` | `K69Down(5)` paths: `AltarTick97` back, `AltarBack53`, the Btn97 BACK / EDIT buttons, pact panel close (`_panel69 = 2`), target-pick ESC (HellordV36.cs 236), confirm NO |
| `ui_denied` | `Msg51(...)` (CODEX / SETTINGS coming soon, NOT ENOUGH SOULS, LOCKED). `Msg51` is one function in HellordPact.cs 147, so a single hook covers them all |
| `seal_press` | main menu seal `Act69(5)` (`_press69 = 0`, 0.45 s brighten) |
| `altar_carousel` | `Scroll97(d)` (HellordAltar97.cs 108) |
| `pact_unlock` / `pact_sign` / `pact_remove` | inside `Primary97` / `TryUnlock51` / `TrySelect51` |
| `path_switch` | `ConfirmYes97` / `ChoosePath97`; confirm dialog open = `_confirm97 = p` |
| `pact_seal_descend` (big sting) | `Descend51()`, which all descend paths reach (DESCEND row, SEAL PACT, Space) |
| `card_reroll` | `Reroll()`; `card_skip` = `SkipCards()` |
| `restart` | `UpdateCardKeys` R on Dead (HellordCards.cs 111) and HellordGame.cs 207 on Won |
| test menu | dev only; recommend no audio |

**Notes for Sound:**
- Menus run on unscaled time. `HellordAudio` already uses `Time.unscaledTime`.
- Pooled voices get paused when `paused` is true (`TickHeart`), so menu SFX in MainMenu/Altar play normally (`paused` is false there).
- The level-up/chest screens pause the voice pool. **Current behaviour: `ui_card_select` / `ui_upgrade_confirm` played while paused may be paused immediately on the same frame (HellordAudio.cs:246 pauses every pooled voice while paused).** This is worth checking in-game; a dedicated unpaused UI voice would fix it.
