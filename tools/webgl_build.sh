#!/bin/bash
# Usage: webgl_build.sh <unity_project_path> <version e.g. v0.0.13>
# Headless WebGL (brotli, Hellord template) -> /workspace/Hellord/Builds/web_<version>/
# Requires an activated license (~/.local/share/unity3d/Unity/Unity_lic.ulf or Unity Licensing Client entitlement via Hub).
set -euo pipefail
PROJ="${1:?project path}"; VER="${2:?version}"
UNITY=/workspace/unity/6000.3.16f1/Editor/Unity
OUT=/workspace/Hellord/Builds/web_${VER}
LOG=/workspace/Hellord/Builds/web_${VER}_build.log
mkdir -p "$OUT"
# Sync Hellord sources into the project (skip if building /workspace/Hellord itself)
if [ "$(realpath "$PROJ")" != "/workspace/Hellord" ]; then
  mkdir -p "$PROJ/Assets/Hellord" && cp -a /workspace/Hellord/Assets/Hellord/. "$PROJ/Assets/Hellord/"
  [ -d /workspace/Hellord/Assets/WebGLTemplates ] && mkdir -p "$PROJ/Assets/WebGLTemplates" && cp -a /workspace/Hellord/Assets/WebGLTemplates/. "$PROJ/Assets/WebGLTemplates/"
  [ -d /workspace/Hellord/Assets/PixelFXPack ] && mkdir -p "$PROJ/Assets/PixelFXPack" && cp -a /workspace/Hellord/Assets/PixelFXPack/. "$PROJ/Assets/PixelFXPack/"
fi
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJ" -buildTarget WebGL \
  -executeMethod Hellord.EditorTools.HellordWebGLBuild.Build -hellordOut "$OUT" -logFile "$LOG" || { echo "BUILD FAILED, see $LOG"; tail -40 "$LOG"; exit 1; }
grep "\[HellordBuild\]" "$LOG" || true
echo "Serve: python3 /workspace/uploads/serve_br.py $OUT"
