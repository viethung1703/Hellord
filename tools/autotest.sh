#!/bin/bash
# Usage: autotest.sh <unity_project> <version>  -> builds the debug autotest Linux player and runs it under Xvfb (Mesa llvmpipe, real GL)
set -uo pipefail
PROJ="${1:?project}"; VER="${2:?version}"; UNITY=/workspace/unity/6000.3.16f1/Editor/Unity
OUT=/workspace/Hellord/Builds/autotest_${VER}; PL=/workspace/Hellord/Builds/autotest_player_${VER}
source /workspace/unity/dbus.env 2>/dev/null; export DBUS_SESSION_BUS_ADDRESS
mkdir -p "$PROJ/Assets/Hellord" && cp -a /workspace/Hellord/Assets/Hellord/. "$PROJ/Assets/Hellord/"
if [ "${SKIP_BUILD:-0}" != 1 ]; then
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJ" -buildTarget StandaloneLinux64 \
  -executeMethod Hellord.EditorTools.HellordAutoTestBuild.Build -hellordOut "$PL" -logFile "$PL.build.log" || { echo BUILD FAILED; grep -E "error CS|Error" "$PL.build.log" | head -30; exit 1; }
fi
rm -rf "$OUT"; mkdir -p "$OUT"
DISPLAY=${ATDISPLAY:-:99} LIBGL_ALWAYS_SOFTWARE=1 timeout 600 "$PL/Hellord.x86_64" -volume 0 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  -force-glcore -runinbg -logFile "$OUT/player.log" -hellordAutotest "$OUT"; echo "player exit $?"
cat "$OUT/summary.txt" 2>/dev/null || { echo "no summary"; tail -40 "$OUT/player.log"; }
