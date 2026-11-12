# Decisions — v0.0.93 (t1006u boss room)

Assumed for now (you said go all three):

1. **Smaller arena** — `bossArenaClamp1006=true`, play clamped to ~960×540 art px (~2 screens) inside the boss room. Camera also clamps to that arena so you can’t scroll into black. Ox Pit baked 1536×864 art is unchanged; Hellord Art still needs a smaller Ox Pit boss room later. Tiled floors use `bossRoomScale1006=0.62`.

2. **Lights** — `bossArenaLights1006=true`: 8 soft torch glows on the arena rim (`bossArenaLightRPx1006` / `bossArenaLightK1006`). Dark Diablo ambient kept; fight space lit.

3. **Trash adds** — `bossAdds1006=true`: 6 at start, then waves of 3 every 3.5s, cap 12 (`bossAddsCap1006`). Imp/Ghoul/Brute with theme variants. Softened HP/dmg vs normal rooms.

Toggles all on HellordTuning. Untested in your URP Editor. FPS light stamp from v0.0.90 kept.
