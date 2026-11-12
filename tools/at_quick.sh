#!/bin/bash
# v0.0.23: at_quick.sh <scenario: atmos|dodge|hook|gossip (v0.0.25, +fill_under_imp v0.0.26)|fx26 (v0.0.26)> [out=/tmp/at_<scenario>] -> rebuild debug autotest player (no FX sync needed) and run one scenario
set -uo pipefail; S=${1:?scenario}; OUT=${2:-/tmp/at_$S}; PROJ=/workspace/HellordProject; PL=/workspace/Hellord/Builds/autotest_player_${ATVER:-v0.0.26}
source /workspace/unity/dbus.env 2>/dev/null; export DBUS_SESSION_BUS_ADDRESS
cp -a /workspace/Hellord/Assets/Hellord/. $PROJ/Assets/Hellord/
if [ "${SKIP_BUILD:-0}" != 1 ]; then /workspace/unity/6000.3.16f1/Editor/Unity -batchmode -nographics -quit -projectPath $PROJ -buildTarget StandaloneLinux64 -executeMethod Hellord.EditorTools.HellordAutoTestBuild.Build -hellordOut $PL -logFile $PL.build.log || { grep -E "error CS" $PL.build.log | head; exit 1; }; fi
rm -rf $OUT; mkdir -p $OUT
DISPLAY=${ATDISPLAY:-:23} LIBGL_ALWAYS_SOFTWARE=1 timeout 400 $PL/Hellord.x86_64 -volume 0 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -force-glcore -runinbg -logFile $OUT/player.log -hellordAutotest $OUT -atOnly $S >/dev/null 2>&1; echo "player exit $?"; cat $OUT/summary.txt
