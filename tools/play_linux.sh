#!/bin/bash
# Usage: play_linux.sh [version=v0.0.19] [extra args...]   -> windowed 1280x720 on the agent desktop (DISPLAY :10, same as box Chrome)
# v0.0.21: per-run log path: LOG=/path/run.log play_linux.sh VER ...   or   play_linux.sh VER "<extra args>" /path/run.log [more args]
#          (3rd arg is the log path when it does not start with '-' and arg 2 is a quoted arg group or empty, so '-botstate /x.json' is never mistaken for a log). Always passes -runinbg (keeps running unfocused).
VER="${1:-v0.0.19}"; DIR=/workspace/Hellord/Builds/linux_${VER}
export DISPLAY="${HELLORD_DISPLAY:-:10}"
EXTRA=("${@:2}")
if [ -z "$LOG" ] && [ $# -ge 3 ] && [[ "$3" != -* ]] && { [[ "$2" != -* ]] || [[ "$2" == *" "* ]]; }; then LOG="$3"; EXTRA=(); [ -n "$2" ] && read -r -a EXTRA <<< "$2"; EXTRA+=("${@:4}"); fi
LOG="${LOG:-$DIR/player.log}"; mkdir -p "$(dirname "$LOG")"
cd "$DIR" && exec ./Hellord.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -window-mode windowed -volume 0.25 -runinbg -logFile "$LOG" "${EXTRA[@]}"
