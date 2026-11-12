# Hellord play bot (tools/playbot.py, v2)

Plays the **real Linux player** with xdotool key holds (WASD, Tab, 1-5). It sends **no debug keys**.
It reads the read-only `-botstate <file>` JSON snapshot the game writes about 20 times a second (HellordBotState.cs).
The bot never changes game code, difficulty or design.

## Single run
```
DISPLAY=:12 ./Hellord.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -window-mode windowed -botstate /tmp/bs.json &
python3 tools/playbot.py /tmp/bs.json <WINDOWID> [maxSeconds] [logfile] [--seed N] [--stats out.json] [--shots DIR] [--boss-wait-max 90] [--display :12]
```
The old positional CLI still works (run19.sh). The display comes from `--display`, then `$DISPLAY`, then `:10`. In v1 it was hard-coded to `:10`.

## Batch (recommended)
```
python3 tools/playbot_batch.py --build linux_v0.0.20 --seeds 1 2 3 4 --jobs 2 --display :12 --out Builds/playbot_v0019_work/batchX [--maxt 900] [--rec] [--nocrt]
```
- Each job starts its own Xvfb (`:12`, `:13`, …) unless that display already exists. It does not touch `:10`, which the other playtests use.
  It does not need a window manager.
- Each run writes `sN/bot.log`, `sN/stats.json`, `sN/player.log`, and `sN/shots/*.png` (window-cropped x11grab).
  With `--rec` it also writes `sN/clip.mkv` (window-cropped, 15 fps).
- The output folder gets `results.json`, `results.csv` and `summary.md`.
- `--seed` seeds only the **bot's** RNG. The game picks its floor layout (1 of 3) with an unseeded RNG and has no seed arg,
  so the layout is recorded per run (`layout` in stats.json) instead.
- True `-batchmode` is not usable, because input goes through X11 key events and needs a window. Xvfb is the headless setup.

## Per-run stats (stats.json)
`result` (won / dead / timeout / boss_no_spawn / stale / bot_crash), `run_s` (game runTime), `rooms_cleared` + `n_rooms_cleared`,
`death_cause` + `death_room` (from the hit that landed in the last 2 s before death: boss_bullet, boss_<state>, <enemy type>, elite_<type>),
`hits_total`, `hp_lost_total`, `hits_per_room` (one entry per room visit: type, hits, hp_lost, enter/cleared run time),
`boss_enter_run_s`, `boss_spawn_latency_s` (from entering the boss room until a boss shows in botstate), `boss_ttk_s` (from first boss sighting until the room clears),
`boss_hits`, `stuck_events` + `stuck_log`, `shop_buys`, `level`, `kills`, `layout`, `anomalies`.

## Behaviour changes in v2
1. **Stuck detection and recovery.** If keys are held but the hero travels less than 0.8 u in 2 s, the bot side-steps at an angle for 0.9 s, alternating sides.
   After 3 such events within 12 s it walks to the room centre for 2.5 s and then tries again.
   In combat, wall-slide drops the force component that pushes into a wall. Every event is logged with its room and position,
   and the 1st, 5th and 10th events save a screenshot.
2. **Pathing.** BFS over the floor graph picks the next door. The bot first walks to a waypoint 2.5 u inside the door on the door's axis,
   then pushes straight through (v1 pushed diagonally and hugged walls). There is a detour around the shrine statue.
   The shop approach gives up after 25 s. Stairs are at the exact room centre (v1 aimed 1 u off).
3. **Boss room.** If the room is uncleared and there is no boss in botstate, the bot holds the south-centre arena start spot instead of wandering.
   It logs `BOSS-WAIT` and takes screenshots at 2, 10, 30 and 60 s. After `--boss-wait-max` s it ends the run as `boss_no_spawn`.
   Every run logs the boss spawn latency.
4. Each hit is logged with a cause classification. The run aborts if botstate goes stale for 20 s.
   The bot writes stats even if it crashes.
