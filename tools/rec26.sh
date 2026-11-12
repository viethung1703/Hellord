#!/bin/bash
# rec26.sh TAG SEC -> v0.0.26 (presses i to spawn Gossip Imps, F1 for a level-up = LevelUpFX) + bot, x11grab client area; BOT=0 = hero stands still (no playbot); EXTRA passes extra player args (e.g. -starthp 0.2 -nocrt)
TAG=$1; SEC=${2:-36}; export DISPLAY=:21 HELLORD_DISPLAY=:21; OUT=/workspace/Hellord/Builds/clips; mkdir -p $OUT
/workspace/Hellord/tools/play_linux.sh v0.0.26 -botstate /tmp/bot_$TAG.json -volume 0.2 ${EXTRA} >/dev/null 2>&1 &
GP=$!; for i in $(seq 40); do W=$(xdotool search --pid $GP 2>/dev/null | tail -1); [ -n "$W" ] && break; sleep 0.5; done
sleep 4; xdotool windowmove $W 0 0; xdotool windowfocus $W; sleep 1
X=$(xwininfo -id $W | awk '/Absolute upper-left X/{print $4}'); Y=$(xwininfo -id $W | awk '/Absolute upper-left Y/{print $4}')
ffmpeg -loglevel error -y -f x11grab -framerate 30 -video_size 1280x720 -i $DISPLAY+$X,$Y -t $((SEC+2)) -c:v libx264 -preset veryfast -crf 16 -pix_fmt yuv420p $OUT/${TAG}_raw.mp4 2>/tmp/ff_$TAG.err &
FP=$!
( sleep ${J1:-4}; xdotool key --window $W i; sleep ${J2:-6}; xdotool key --window $W i; sleep ${J3:-3}; xdotool key --window $W F1 ) &
if [ "${BOT:-1}" = 0 ]; then xdotool key --window $W 1; sleep 0.6; xdotool key --window $W Tab; sleep $SEC; else timeout $SEC python3 /workspace/Hellord/tools/playbot.py /tmp/bot_$TAG.json $W $SEC /tmp/bot_$TAG.log >/dev/null 2>&1; fi
xdotool keyup w a s d; wait $FP; kill $GP; cat /tmp/ff_$TAG.err | head -3
