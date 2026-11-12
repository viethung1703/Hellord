#!/bin/bash
# t945u: DEFAULT for review clips. rec_review.sh <scenario e.g. shaderfx934> <outDir> [fps=60]
# Runs Builds/autotest_player with -atRec <fps>: Time.captureFramerate fixes the game step to 1/fps and every frame is saved,
# so clips are smooth no matter how slow llvmpipe renders. Encodes clip_<name>.mp4 at fps, crop 640x360 around centre, x2 nearest.
# Only scenarios that call AtRecFrame produce frames (currently shaderfx934).
set -uo pipefail; S=${1:?scenario}; D=${2:?outDir}; FPS=${3:-60}; T=/tmp/at_rec_$S
rm -rf $T; mkdir -p $T $D
DISPLAY=${ATDISPLAY:-:99} LIBGL_ALWAYS_SOFTWARE=1 timeout 3000 /workspace/Hellord/Builds/autotest_player/Hellord.x86_64 -volume 0 -screen-fullscreen 0 \
  -screen-width 1280 -screen-height 720 -force-glcore -runinbg -logFile $T/player.log -hellordAutotest $T -atOnly $S -atRec $FPS >/dev/null 2>&1
cat $T/summary.txt
for r in $T/rec_*; do n=$(basename $r); n=${n#rec_${S}_}
  ffmpeg -loglevel error -y -framerate $FPS -i $r/f_%05d.png -vf "crop=640:360:320:160,scale=1280:720:flags=neighbor" \
    -c:v libx264 -crf 16 -preset slow -pix_fmt yuv420p -r $FPS $D/clip_$n.mp4 && echo "$D/clip_$n.mp4"; done
