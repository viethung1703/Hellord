# Hellord icon style guide (step48, research-based) — PROPOSAL, not approved by VH

Sources: Slynyrd Pixelblog 26 (UI basics), 47/59 (tiny sprites, "outlines only where needed for readability");
Lospec "Pixel Art Outlines Part 2: Using Color" (selective/colored outlines, any two touching colours must be distinct at 1x);
Wayline "Readable icons" (silhouette first, judge at 1x, 1px gap between shapes); Saint11 (Pedro Medeiros) tips on hue-shifted ramps,
clusters and pillow shading; common practice in Brotato / Halls of Torment / Vampire Survivors / Dead Cells / Darkest Dungeon / PoE item icons
(one object, big, lit from one side, strong rim against a dark plate).

## Rules (all are code-checkable)
1. **Silhouette first.** Fill the shape in one colour at 1x. If it is not recognisable, stop and redesign; shading will not save it.
2. **Size and space.** Icon art box 20x20 inside the 24px slot; the drawn object incl. outline fits 16-18px, centred by bounding box. Min stroke 2px (3px for the main form). No 1px lines except highlights/outline.
3. **Max 2 elements** (main object + one modifier). The modifier is ≤ 1/3 of the area and sits bottom-right or top-right, separated by a 1px black gap.
4. **One light, top-left.** Every form gets: highlight (1-4 px, top-left), light, mid, shadow (bottom-right). No pillow shading (dark rim all round + light centre).
5. **Ramps of 3-4 Endesga colours, hue-shifted:** shadows go toward red/purple/blue, highlights toward yellow/white. Fire: wine→red→orange→amber→yellow→white. Blood: wine→red→pink→white. Steel: dslate→slate→grey→lgrey→white. Wood: plum→brown→clay→tan. Spirit: navy→blue→cyan/lgrey→white.
6. **Selective outline (automatic in code):** outline pixels on the lit (top/left) side use the dark end of the object's ramp; on the shadow (bottom/right) side use black #181425. Outline must be darker than both object and plate.
7. **Plate.** All icons sit on the same ink (#262b44) plate with clipped corners; never on pure black (black outlines vanish there).
8. **Value contrast.** The object's lightest value must be ≥ 2 steps above the plate; avoid dark-on-dark objects (the old hook/banner/drum problem). Test: convert to greyscale at 1x.
9. **Clean clusters.** No single stray pixels, no jaggies on curves (use 1-2-3 step lines), no dithering at 20px, ≤ 10 colours per icon incl. outline.
10. **Same-stat sins:** keep the main shape, change ONE small detail in the modifier corner (colour or tiny glyph), never the silhouette.
11. **Tier = frame, not icon.** Mortal/Demonic/Heaven-Defying is shown only by the slot frame (and HD glint), so the art stays the same style across tiers.

## Audit of current icons (step44f/45/46 "NOW")
- All: flat fills with no light direction → read as stickers; outlines are uniform; several sit on pure black.
- Fireball: OK silhouette, but the core is small and the tail is noisy streaks.
- Chain Lightning: 4 tiny nodes + 1px links = no silhouette at 1x (reads as a "share" glyph).
- HP heart: good silhouette, no light → flat.
- Area: pink disc + orange dot reads as a button; no "radius" meaning.
- HP+Armor: shield is 5px, a smudge.
- #5 Drum: cream/brown blob, low contrast between skin and body; bolt OK.
- #6 Hook: 1px grey line on black → disappears at 1x.
- #36 Banner: grey flag on grey pole, no face/ghost cue; low value range.

## Fixes applied in step48 (see Art/previews/step48_icon_research/new.py)
- Fireball: comet diagonal, white-hot core bottom-right, tail tapers to 1px top-left.
- Chain Lightning: two lit orbs joined by a 2-3px zigzag bolt with white core.
- HP: same heart + highlight top-left, wine shadow bottom-right.
- Area: shaded ring (lit top-left) + bright centre dot = "radius".
- HP+Armor: smaller heart + 9x10 two-tone steel shield, black gap between them.
- #5 Drum: tan/cream skin, rust bands, lit-left wood body, yellow bolt striking the skin.
- #6 Hook: 3px steel hook with highlight edge, red barb tip, blood drop.
- #36 Banner: spear-tipped pole + white/blue ghost-face flag with tattered wisps.

## Workflow for new icons
Draw as an ASCII grid of palette letters (no outline) → `layer(grid, rim)` adds the selective outline → `stack()` centres →
`plate()` → check at 1x and in greyscale → check against neighbours in the bar.
