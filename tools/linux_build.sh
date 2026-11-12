#!/bin/bash
# Usage: linux_build.sh <unity_project_path> <version e.g. v0.0.16>
# Headless Linux64 release player -> /workspace/Hellord/Builds/linux_<version>/
# Requires an activated license (~/.local/share/unity3d/Unity/Unity_lic.ulf or Unity Licensing Client entitlement via Hub).
set -euo pipefail
[ -f /workspace/unity/dbus.env ] && source /workspace/unity/dbus.env
PROJ="${1:?project path}"; VER="${2:?version}"
UNITY=/workspace/unity/6000.3.16f1/Editor/Unity
OUT=/workspace/Hellord/Builds/linux_${VER}
LOG=/workspace/Hellord/Builds/linux_${VER}_build.log
mkdir -p "$OUT"
# Sync Hellord sources into the project (skip if building /workspace/Hellord itself)
if [ "$(realpath "$PROJ")" != "/workspace/Hellord" ]; then
  mkdir -p "$PROJ/Assets/Hellord" && cp -a /workspace/Hellord/Assets/Hellord/. "$PROJ/Assets/Hellord/"
  [ -d /workspace/Hellord/Assets/WebGLTemplates ] && mkdir -p "$PROJ/Assets/WebGLTemplates" && cp -a /workspace/Hellord/Assets/WebGLTemplates/. "$PROJ/Assets/WebGLTemplates/"
  [ -d /workspace/Hellord/Assets/PixelFXPack ] && mkdir -p "$PROJ/Assets/PixelFXPack" && cp -a /workspace/Hellord/Assets/PixelFXPack/. "$PROJ/Assets/PixelFXPack/"
fi
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJ" -buildTarget Linux64 \
  -executeMethod Hellord.EditorTools.HellordLinuxBuild.Build -hellordOut "$OUT" -logFile "$LOG" || { echo "BUILD FAILED, see $LOG"; tail -40 "$LOG"; exit 1; }
grep "\[HellordBuild\]" "$LOG" || true
echo "Play: /workspace/Hellord/tools/play_linux.sh $VER"
