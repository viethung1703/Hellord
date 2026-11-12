# Hellord Sins UI Feel Research — t1150

**For:** Viet Hung · Hellord (Unity 6 pixel survivor, Endesga 32, carved-stone Sins screen)  
**Date:** 2026-11-09 (Asia/Saigon)  
**Trigger:** t1148 "UI feel, buttons feel are bad" / unlock unnoticeable; t1149 "where is ask player before unlock?"; t1150 "please research for best UI feel bro"  
**Scope:** Research + concrete Sins-screen spec. **No code changes in this pass.**

---

## 1. Sources (cited)

| Source | Why it matters |
|---|---|
| [Juice it or lose it — Jonasson & Purho (GDC Europe)](https://www.youtube.com/watch?v=Fy0aCDmgnxg) | Juice = max output for min input; tween/ease, squash-stretch, flash white, particles, sound, stagger. |
| [The Art of Screenshake — Jan Willem Nijman / Vlambeer](https://www.youtube.com/watch?v=AJdEqssNZ-U) · [write-up](https://infovore.org/archives/2013/10/22/making-game-feel) | Hitstop ~10–20 ms; permanence; layered impact cues; shake as spice not default. |
| [Feedback: how a game tells the player it heard them — Hadjoudj Idris](https://h-idris.com/blog/game-feedback-design.html) | Three questions (heard / worked / changed); ≤100 ms ack; 100–300 ms result; layered cues; when modals are right. |
| [Game Feel & Juice latency budgets — Game Design Lab](https://teemo.dev/game-design/concepts/live-ops-events/systems/game-feel-and-juice/) | Touch→visible ≤50–100 ms; claim stack timings; counter/bar rules; no menu screen-shake; skippable reveal. |
| [Interactive Component States — asobi.gg](https://asobi.gg/interactive-component-states/) | Hover subtle (~15%), press stronger (~30%), focus distinct for pad/keyboard, disabled mute. |
| [Press vs release activation — GameDev.SE](https://gamedev.stackexchange.com/questions/14390/in-a-gui-should-input-be-handled-when-the-button-is-pressed-or-released) | Mouse: activate on **release** over same control (cancel by drag-off). Gamepad/keyboard: activate on **press**. |
| [Hold to Confirm — Design Atlas](https://design.solhun.com/en/interaction/hold-to-confirm/) | Hold-to-confirm for irreversible / high-stakes only (~0.5–1.1 s fill + "release to cancel"). Everyday = modal. |
| [Fictional-currency confirm — UX StackExchange](https://ux.stackexchange.com/questions/24476/should-i-ask-users-to-confirm-before-making-a-purchase-with-a-fictional-currency) | Confirm any spend that has no undo / real grind weight. |
| [Balatro juice stack](https://blakecrosley.com/guides/design/balatro) | Layer animation + counter roll + particles + audio; celebration ∝ significance. |
| [Slay the Spire UI notes](https://www.cloudfallstudios.com/blog/2018/2/20/flash-thoughts-slay-the-spires-ui) | Never block next input on animation; massive hitboxes; exaggerated SFX on meaningful acts. |
| [Pixel-art UI rules — Pixeli](https://pixeli.pebbly.space/knowledge/rules/83-ui-and-icons) | Shared bevel kit; hover = +1 value; pressed = **invert bevel + 1 px down-right**; same silhouette; HUD juice quiet. |
| [Pixel render stack](https://tigerabrodi.blog/2d-rendering-concepts-a-reference) | Integer scale, nearest-neighbour, pixel snap; no subpixel UI motion. |
| [Game UI Database](https://gameuidatabase.com/index.php?set=0) | Reference library for menu/button state patterns across shipped games. |
| Player reaction: [Diablo IV Reliquary friction](https://us.forums.blizzard.com/en/d4/t/reliquary-unlock-method-is-ridiculous/221077) · [ResetEra hold-to-confirm](https://www.resetera.com/threads/why-do-games-these-days-require-you-to-hold-a-button-to-make-certain-menu-confirmations.552202/) | Fake transaction delays + hold on routine unlocks = annoyance. Don't copy. |

**Game reference takeaways (for Sins tone):**
- **Hades:** themed, highly animated menus; distinct hover / confirm / back / deny sounds per interface.
- **Slay the Spire:** snappy card motion; input never waits on FX; big targets.
- **Balatro:** multi-layer juice on rare big moments; counters *roll*; hover lift on interactive cards (not on Hellord collection tiles).
- **Dead Cells:** menus stay out of the way of action; low friction.
- **Hollow Knight:** restrained, tactile, thematic — juice matches material, not flashy UI chrome.
- **Vampire Survivors:** HUD quiet; meaning via colour; options to disable flash/shake.
- **Celeste:** physical block-like menu motion; hover scale on selectable rows.
- **Diablo / PoE shop:** players demand a confirm that restates item + cost; they hate fake wait spinners on free currency.

---

## 2. Core principles (apply to every Sins control)

1. **Three answers per action** (Idris):  
   - *Did you hear me?* → press state + sound in **≤100 ms**.  
   - *Did it work?* → success/deny within **100–300 ms**.  
   - *What changed?* → currency, slot state, header text update after.
2. **Ack ≠ result.** Never wait for the unlock animation to show the press.
3. **Activate on mouse-up** (same control); **activate on pad/key press**. Drag-off cancels mouse.
4. **Juice ∝ importance.** Hover quiet · click medium · unlock loud · collection tiles silent (VH directive).
5. **Pixel-safe motion only:** integer px offsets, nearest filter, no fractional scale punches on stone faces. Prefer bevel invert + 1–2 px press-in over `scale 0.92`.
6. **Mute test:** every state must still read with SFX off.
7. **Skip / never trap:** unlock ceremony ≤ ~1.2 s and skippable by click/confirm.

---

## 3. Pixel-art constraints (Hellord / Endesga 32)

| Do | Don't |
|---|---|
| Press = invert bevel + label/face **+1 px down-right** (Pixeli) | Subpixel `transform.scale` on stone buttons |
| Hover = +1 face value step **or** gold 1–2 px rim (already in carved kit) | Soft glow blooms that leave the palette |
| Flash = Endesga white / warm gold / blood red for **1–2 frames**, then ease | Full-screen white strobes (>2 luminance flips / s) |
| Shake = ±1–2 **integer** px on the **control**, ≤120 ms | Camera / full UI shake on every click |
| Particles = 4–8 hard pixel sparks (palette colours) | Soft GPU particles / motion blur |
| Same silhouette across Normal / Hover / Pressed / Disabled | Layout shift when pressing (causes label ghosting) |

**Hellord material kit (keep):** stone face, gold emboss, firelight wash on hover, carved-in dark wash on press. Collection tiles: **no** hover animation (locked VH rule).

---

## 4. Timing cheatsheet (ms @ 60 fps)

| Event | Duration | Ease | Notes |
|---|---|---|---|
| Hover enter wash/rim | **0–80** | Instant or ease-out | Must feel immediate |
| Press-in (bevel invert + 1 px) | **0–60** | Snap | Visible while held |
| Mouse-up click flash (gold rim) | **80–140** | Ease-out | Ack of commit |
| Deny flash + micro-shake | **350–450** | Linear fade | ±1–2 px, 3–4 kicks |
| Tab switch | **120–180** | Ease-out | Content appears immediately; frame moves |
| Modal open / close | **150 / 100** | Ease-out / ease-in | Fade-out faster than in |
| Currency count | **250–600** hard cap | Ease-out | Fixed duration; step size scales with Δ |
| Unlock ceremony total | **900–1200** | Staged | Skippable from frame 1 |
| Hitstop (unlock impact only) | **16–33** (1–2 frames) | — | UI-local freeze, not `Time.timeScale` combat |

---

## 5. Prioritized Sins-screen spec

Priority order matches VH pain: **confirm → button feel → unlock ceremony → deny → currency → sound → pad focus → anti-overjuice**.

### P0 — Ask before unlock (VH t1149)

**Use a stone modal, not hold-to-confirm.**  
Reasons: cost is grind-weight (Souls+Sigils, no undo); modal can restate cost + after-balance; hold-to-confirm trains muscle memory and feels like mobile MTX when used on routine unlocks (D4 / ResetEra backlash). Hold reserved only if VH later wants a "skip confirm" option for repeated unlocks.

**Modal copy (English only):**
```
UNLOCK SIN SLOT 2?
Cost:  500 Souls  +  4 Sigils
After:  XXX Souls   ·   Y Sigils

[ UNLOCK ]    [ CANCEL ]
```
- Show **current → after** numbers (PoE/Diablo player demand).  
- If cannot afford: disable UNLOCK, show short red reason (`NOT ENOUGH SOULS` / `NOT ENOUGH SIGILS`), play `ui_denied`.  
- Esc / East / click-outside / CANCEL → close, no spend.  
- Enter / South / UNLOCK → begin unlock ceremony, then debit + save once.  
- Double-click guard: ignore further unlock clicks for **250 ms** after confirm (Game Design Lab).  
- Autotest path: keep a flag to skip modal when `-hellordAutotest` (or existing fake-click path).

### P0 — Button feel (tabs, BACK, modal buttons, afford-slot affordance)

| State | Visual (pixel-safe) | Sound | Timing |
|---|---|---|---|
| **Normal** | Stone face, default bevel | — | — |
| **Hover / Focus** | Warm firelit wash (~α 0.18–0.22) + gold 1–2 px rim; **or** +1 face value | `ui_hover` (existing, 4 variants) once on enter | ≤80 ms |
| **Pressed (held)** | Bevel **inverted**; face+label **+1 px down-right**; dark wash α≈0.45 | optional soft `ui_confirm` pitch-down tick at down (or silent; prefer silent until up) | Instant |
| **Click (release commit)** | Gold rim punch 80–140 ms; optional 1-frame Endesga white flash on rim only | `ui_confirm` | Ack ≤100 ms |
| **Disabled** | Desaturate / darker face; **no** hover/press | `ui_denied` if clicked | — |
| **Deny (enabled but refused)** | Red wash α≈0.25 + red double rim; control shakes ±1–2 px × 3–4 over 350–450 ms | `ui_denied` | 350–450 ms |

**Rules:**
- Mouse: down shows Pressed; **up over same id** fires action; up outside cancels (already correct on BACK — extend to all Sins controls + modal).  
- Pad/keyboard: focus ring = same gold rim as hover; **South/Enter fires on press**.  
- Draw state **before** label so text never doubles (known BACK ghost bug).  
- Hitboxes ≥ visual + 2–4 px pad; no layout shift between states.

### P0 — Unlock reveal ceremony (anticipation → action → reward)

Triggered only after modal confirm. Total **~1.0 s**, skippable.

| Phase | t (ms) | Visual | Audio |
|---|---|---|---|
| Anticipation | 0–200 | Slot cracks (1–2 px hairline cracks toward centre); brief darken; currency numbers highlight | low stone grind / reuse quiet `pact_unlock` pre-roll or new `ui_unlock_wind` |
| Action | 200–280 | Hitstop 1–2 frames on the **slot only**; gold flash 1 frame; ±2 px kick | `pact_unlock` (existing) |
| Break | 280–550 | Stone shards (4–6 palette pixels) fly out 8–16 px, fade; face opens | shard tick (can layer `pact_unlock` var 2) |
| Reward | 550–1000 | Slot face → open gold state; label **OPEN**; header `SIN SLOT n UNLOCKED` 1.2–1.6 s; currency counts down | `ui_confirm` or short sting; currency ticks |
| Settle | 1000+ | Rest open state; header fades | — |

**Must be unmistakable even muted:** crack → flash → open gold + header text.  
**Do not** rely on header alone (currently easy to miss / clip under BACK). Prefer header in the Sins title band, single instance, no duplicate draw.

### P1 — Denied feedback

| Case | Visual | Sound | Text |
|---|---|---|---|
| Can't afford | Red wash + shake on slot/button | `ui_denied` | Header or toast: `NEED 500 SOULS + 4 SIGILS` |
| Wrong order (slot 3 before 2) | Same | `ui_denied` | `UNLOCK SLOT 2 FIRST` |
| Click disabled control | Same, shorter 300 ms | `ui_denied` | — |

Shake = local control only, ±1–2 px integer, 350–450 ms. No camera shake.

### P1 — Currency counter

- On spend: numbers **roll down** over **250–600 ms** (cap; big spends use bigger steps, not longer time) — Balatro / live-ops pattern.  
- Flash the spent currency icon/label gold→stone once.  
- Show modal **after** preview (current → after) so the roll confirms what the modal promised.  
- Tabular / fixed-width digits to avoid jitter.

### P1 — Sound layering (Hellord Sound checklist)

Existing clips under `Audio/SFX/ui/` and hooks already mapped in `Hellord_menu_audio_map.md`:

| Cue | Hook / clip | When | Status |
|---|---|---|---|
| Hover tick | `ui_hover` (01–04) | Focus/hover enters a **new** control | **Exists** — wire on all Sins controls; one-shot per enter |
| Confirm / click | `ui_confirm` (01–02) | Successful mouse-up / pad confirm on buttons + modal UNLOCK | **Exists** — ensure Sins uses it |
| Back | `ui_back` (01–02) | BACK / Cancel / Esc close | **Exists** |
| Denied | `ui_denied` (01–02) | Can't afford, wrong order, disabled click | **Exists** |
| Unlock | `pact_unlock` (01–02) | Unlock ceremony Action beat | **Exists** |
| Modal open | *(gap)* `ui_modal_open` or reuse `ui_confirm` quieter | Modal appears | **Need from Hellord Sound** (or reuse) |
| Unlock wind / crack | *(gap)* `ui_unlock_wind` | Anticipation 0–200 ms | **Optional new** |
| Currency tick | *(gap)* `ui_currency_tick` | During count-down, every N units, max ~8 ticks | **Optional new** |
| Slot open settle | reuse `ui_upgrade_confirm` or `shop_purchase` | Reward phase | Prefer reuse over new |

**Rules for Sound:**
- Pitch jitter ±1–2 semitones on hover/confirm so repeats don't fatigue.  
- Min gap on `ui_hover` ~40–60 ms so scrubbing doesn't machine-gun.  
- UI voices must play while menus pause the game (`Hellord_menu_audio_map.md` already flags paused-pool risk).  
- Play muted: every beat above still has a visual twin.

### P2 — Keyboard / gamepad focus

- Always exactly one focused control on Sins when using pad/keyboard.  
- Focus visual = hover gold rim (asobi Focus = 100% layer).  
- D-pad / stick: move among BACK, tabs, next unlockable slot, modal buttons.  
- South / Enter = confirm (press). East / Esc = back/cancel.  
- Mouse move steals hover; last input device wins (standard hybrid).

### P2 — Tabs

- Hover/focus rim like buttons.  
- Selected tab = persistent embossed/gold state (not the same as hover).  
- Switch ≤180 ms; **content swaps instantly**, frame eases (Idris: never animate the thing the player waits for).

### P3 — Anti-overjuice (keep Hellord identity)

- **No** hover animation on collection tiles (VH).  
- **No** screen/camera shake on menu clicks.  
- **No** particles on hover or ordinary clicks — only on unlock break.  
- **No** fake 2 s "transaction verifying" delay (D4 Reliquary anti-pattern).  
- Celebration ranks: hover < click < deny < unlock. Never equalize.  
- Honour a future "reduce UI motion" toggle: snaps to end states, keeps colour/sound.

---

## 6. Concrete per-control checklist (implementation order)

1. **Modal confirm** for slot unlock (cost + after-balance + Unlock/Cancel).  
2. **Unify press model:** all Sins buttons = mouse-up activate; pad = press. Fix BACK label ghost (state drawn once, before label).  
3. **Hover / press / click** visuals per §5 on BACK, tabs, modal buttons, locked slots (afford vs unafford).  
4. **Deny** red flash + local shake + clear header reason.  
5. **Unlock ceremony** crack → hitstop flash → shards → OPEN + single header.  
6. **Currency roll-down** after debit.  
7. **Wire existing SFX**; request only modal/currency/wind gaps from Hellord Sound.  
8. **Pad focus** ring + nav.  
9. **videoReview** normal-speed capture before claiming feel is fixed; no package until VH says build.

---

## 7. What "best UI feel" means for Hellord (one paragraph)

Best feel here is not Balatro fireworks or Hades pageantry on every click. It is **stone that answers instantly**: a gold rim when you aim, a carved-in face while you hold, a commit flash when you release, a clear ask before spending grind currency, and an unlock you cannot miss even with the sound off — crack, flash, open gold, header, currency counting down — while collection tiles stay still and the Endesga palette stays honest. Maximum signal, minimum chrome, pixel-integer motion.

---

## 8. Top 8 recommendations (for VH)

1. **Ask before unlock with a stone modal** that shows cost and after-balance; Cancel/Esc safe. No hold-to-confirm for slot unlocks.  
2. **Ack in ≤100 ms:** hover rim + press-in must appear the same frame as input; never wait on the unlock FX.  
3. **Mouse-up to activate, press for pad;** drag-off cancels. Extend the BACK pattern to every Sins control.  
4. **Pixel press recipe:** invert bevel + 1 px down-right (not subpixel scale).  
5. **Unlock ceremony ~1 s, skippable:** crack → 1–2 frame hitstop/flash → shards → OPEN + one header. Must read muted.  
6. **Denied = red flash + ±1–2 px shake + plain reason text** (`UNLOCK SLOT 2 FIRST` / `NEED X SOULS`).  
7. **Currency rolls down** after spend (≤600 ms); modal preview matches the final numbers.  
8. **Layer existing Hellord Sound hooks** (`ui_hover`, `ui_confirm`, `ui_back`, `ui_denied`, `pact_unlock`); only add modal/currency ticks if needed — and keep collection tiles quiet.

---

*Research only. Next implementation pass should follow §6 order, then normal-speed videoReview, then ask VH before any v0.0.97 package.*
