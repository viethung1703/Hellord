# Hellord QC Checklist
Kept by Hellord Producer. Every item is checked against this list before it goes to Hellord Maker or Viet Hung.
Add a line each time Viet Hung approves or rejects something, with the date.

## Gameplay
- [ ] Matches its gameplay brief.
- [ ] Danger is clearly telegraphed before it hits, and there is a readable escape gap.

## Readability
- Persistent markers (elite etc.) must be findable at a glance at 1x in a dense horde at all times, not only during a pulse; overhead plates must not cover props (InteractFX v2.4.0).
- Warning fills (tells and areas) draw UNDER characters and never tint an enemy (Gossip shout v0.0.25).
- [ ] Hero skills never use enemy-tell language (closing circles, filled lanes, blinking), never share a colour with enemy types, and never hide tells near the hero at max level.
- [ ] Reads at game scale, under the CRT filter, and in motion. Judge it from a moving clip, not a still.
- [ ] Nothing smaller than 3px. The CRT darkens every third row, so 2px detail reads as 1px.
- [ ] Icons and pips are at least 8x6 (icons at least 5x7), bright, with a dark outline or plate.
- [ ] Warning fills are not red-on-red: a red hero or enemy inside them must stay readable, and the hero draws above FX.
- [ ] Not too dark. Dark visuals have been rejected.

## Style
- [ ] Uses the Endesga 32 palette and is consistent with approved art. (OPEN: the design doc says snap to PAL14, and HA's 26 Oct note says don't snap to a strict palette. Waiting on Viet Hung.)
- [ ] The CRT matches HA's preview filter, with less glow.
- [ ] Burn does not tint the whole enemy. Reference: MM's flames-and-embers BurnFX (approved).

## Source
- [ ] No AI art, except PixelLab sprites that HA has cleaned up.

## Approval log
- MM flames-and-embers BurnFX: approved.
- Whole-enemy burn tint: rejected.
- StunBar #114 v1.8.8: accepted (per HM, 27 Oct 2026).
- Support icons style B (rune tile): HA reports VH OK, 27 Oct (not yet confirmed directly).
- Skill bar icons with cooldown style A (tiny 3x5 corner digit, 1px outline, no plate): HA reports VH OK, 27 Oct. VH's pick overrides my 3px concern for this HUD digit.
