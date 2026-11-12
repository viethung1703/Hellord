# Final review checklist for VH (started 4 Nov 2026, 23:18 UTC+7)
VH's ask (t855u, HM chat): do the remaining list step by step, HA does art, FX redoes the fake-looking FX, he reviews everything at the end. Nothing packaged or published without VH.
Status: TODO / WIP / QC-PASS / QC-FAIL (back to maker) / READY FOR VH. All art and FX are MOCK-UPS until VH OKs.

## HM (code), in this order
- [ ] 1. 5-skill belt HUD: 1 row, cooldown number centred in the icon, separate sins frame left-aligned above the belt, HD glint around the frame border. WIP
- [ ] 2. Duration stat really changes skill durations. WIP
- [ ] 3. Demonic and Heaven-Defying sin effects (Docs/Hellord_sins_design_v0.4.md). WIP
- [ ] 4. Echo card text for Curse fixed. WIP
- [ ] 5. Compiles with 0 errors; untested-in-Editor list written down.

## Hellord Art, in this order
- [ ] 1. Crow legs (H38 leg redo). WIP
- [ ] 2. Icons D33, D24, H44, H41. TODO
- [ ] 3. Ghost Mushroom sprite (waiting on VH's OK). TODO
- [ ] 4. Skill visuals and icons for all skills. Skill icons: WIRED (in game). Sin icons (70, v53 sheet): WIRED v0.0.74, VH approved. Sins frame + HD running light: WIRED v0.0.74/v0.0.75, VH approved, seen in real-build capture t928u.

## Hellord FX
- [ ] 1. Audit of current skill and combat FX, with a list of the weak ones. WIP
- [ ] 2. Redo the weak FX in batches (pixel style, Endesga 32, banded light). PARTIALLY WIRED (t928u, v0.0.76): shader FX for Fireball blast, Curse zone veins, Grave Pool liquid are in game behind `fxShaderOn` (old FX = fallback). Grave Pool overlapping edges still being fixed by Hellord FX (textures swappable by name in HellordTuning). Captures: Builds/autotest_v0.0.76_shaderfx928/, crops in Builds/t928u_review/.
- [ ] 3. v2.8.0 HudJuiceFX (skill bar and banners, built earlier, not QC'd yet). Check it still fits the new 5-skill belt. TODO

## Older open items for VH to know about
- HT v0.0.53 bugs B01 to B18 and balance note O1 (Builds/linux_v0053_playtest/). HM has the full report.
- HA step15 Hook Thrower and Gossip Imp sheets: failed QC, waiting on VH's keep-or-redraw pick.

## Blockers
- none yet

## t930u (v0.0.78): shader batch 3
- [ ] Grave Pool overlap draws as one shape (t930u_review/grave_pool_*_poolarea_zoom4x.png)
- [ ] #40 Burning City hybrid: tongues on the band, front half over the actors (burning_city_*)
- [ ] #41 Ghost Mushrooms v2: lilac pool with threads and spores (ghost_mushrooms_*)
- [ ] #14 Death Bell: toll rings and gong, then the plum zone, with damage still at the same moment (death_bell_*)
- [ ] #36 Ghost Banner: wisps with ink eyes instead of the spear (ghost_banner_*)
- [ ] Try it on a real GPU and Vulkan (only llvmpipe has been tested)
- [ ] Soul Spears v2: make the head pointier (VH note), still pending
