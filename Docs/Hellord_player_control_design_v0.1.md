# Hellord: Player Control Design v0.1 (Manual mode SHELVED — t720u)

**Status: APPROVED by VH (t716u, 2026-11-02) — in build (compiled, not play-tested).**

**t720u (2026-11-03): VH chose Option 1 — Auto skills + mouse aim. Manual mode, key badges, slot pick and the Controls page are shelved (code kept behind manualControl720=false, showKeyBadges720=false). The 2x4 skill belt stays.**

Status: draft from VH decisions on Nov 2–3, 2026 (UTC+7), updated with t714u–t715u. No code has been written. Do not implement until VH approves.

## 1. Control model
- New option: player-controlled skills (PoE2-like). The player aims and casts each skill instead of the skills auto-firing.
- Movement is the player's choice in settings:
  - **Mouse mode:** click-to-move.
  - **WASD mode:** keyboard movement, with the mouse used for aiming.
- An Auto/Manual toggle stays, for testing and fun only. Manual is the main design.
- **Aim:** always at the mouse cursor, in both movement modes.
- **Firing:**
  - Press: most skills cast once per press.
  - Hold: channelled skills keep going while the key is held.
  - Auto: orbit and summon skills (e.g. Bone Orbit, minions) run on their own and don't need a key.
- **Dash:** Space, PoE2-style direction. In mouse mode it goes toward the cursor; in WASD mode it goes in the movement direction. The cooldown is unchanged.

## 2. Resources
- No mana. Skills are limited by cooldowns only.
- Skills and supports are still gained from level-up cards, as now.
- **No basic attack.** The player only has skills and starts with 1 skill in slot 1.
- **New skill card:** the player picks which slot it goes in. Picking a filled slot replaces the skill that was there.

## 3. Skill bar
- Position: bottom-left of the screen, a 2x4 "belt" with 8 slots.
- Icons: native 24 px skill icons, no scaling.
- Supports: each skill has 4 support-gem slots. Each support has its own unique glyph. Hovering a skill shows a tooltip that lists its supports by number.
- Key badge: a cream keycap on the icon's top-left corner, showing the current binding.
  - Art reference: Hellord Art step39j V1, `/workspace/Hellord/Art/previews/step39j_skill_bar_B_simple_keys/`.
- Mouse slots show a mouse sign plus L or R instead of a keycap letter.

## 4. Controls page (rebinding)
- Mockup: step40, `/workspace/Hellord/Art/previews/step40_controls_page/`.
- Every action can be rebound, and the badges on the skill bar always show the current binding.
- If a new key is already used by another action, the page offers to swap the two bindings.
- A "Reset to default" button restores the defaults below.

## 5. Default bindings
| Action | Mouse-move mode | WASD mode |
|---|---|---|
| Skill slots 1–8 | LMB, RMB, Q, W, E, R, T, F | LMB, RMB, Q, E, R, F, 1, 2 |
| Move | LMB click-to-move | W A S D |
| Dash | Space | Space |
| Interact | G | G |
| Inventory | I | I |
| Map | Tab | Tab |
| Pause | Esc | Esc |

Left-click (PoE2 rules, resolved in t715u):
- **Mouse mode:**
  - LMB on the ground moves.
  - LMB on an enemy uses the LMB skill.
  - Shift+LMB casts the LMB skill in place without moving.
- **WASD mode:** LMB is just skill slot 1.

## 6. Open questions
Resolved: auto mode stays as a test/fun toggle; no basic attack; left-click behaviour is decided (section 5).
1. Final art approval of the skill bar (step39j V1) and the Controls page (step40).
