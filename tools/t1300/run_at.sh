#!/bin/bash
# usage: run_at.sh <scenario> <outdir> [extra player args]   (SKIP_BUILD=1 reuses the player). t1300: own player dir (v0.0.99 / v0.1.0 baseline players untouched).
S=$1; OUT=$2; shift 2
PROJ=/workspace/HellordProject; PL=/workspace/Hellord/Builds/autotest_player_t1300
source /workspace/unity/dbus.env 2>/dev/null; export DBUS_SESSION_BUS_ADDRESS
cp -a /workspace/Hellord/Assets/Hellord/. $PROJ/Assets/Hellord/
if [ "${SKIP_BUILD:-0}" != 1 ]; then /workspace/unity/6000.3.16f1/Editor/Unity -batchmode -nographics -quit -projectPath $PROJ -buildTarget StandaloneLinux64 -executeMethod Hellord.EditorTools.HellordAutoTestBuild.Build -hellordOut $PL -logFile $PL.build.log || { echo BUILD FAILED; grep -E "error CS|error:" $PL.build.log | sort -u | head -40; exit 1; }; fi
D=${ATDISPLAY:-:31}; pgrep -f "Xvfb $D " >/dev/null || { Xvfb $D -screen 0 1920x1200x24 -ac +extension GLX +render -noreset >/dev/null 2>&1 & sleep 2; }
rm -rf $OUT; mkdir -p $OUT
DISPLAY=$D LIBGL_ALWAYS_SOFTWARE=1 timeout ${ATTIMEOUT:-900} $PL/Hellord.x86_64 -volume 0 -screen-fullscreen 0 -screen-width ${W:-1280} -screen-height ${H:-720} -force-glcore -runinbg -logFile $OUT/player.log -hellordAutotest $OUT -atOnly $S "$@" >/dev/null 2>&1; echo "player exit $?"; cat $OUT/summary.txt
