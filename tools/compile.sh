set -e; cd /workspace/Hellord/tools/build
for d in pfx rt_legacy rt_is ed; do echo "== $d"; ~/.dotnet/dotnet build $d/c.csproj -nologo -v q 2>&1 | grep -E "error|warning|Build succeeded|Warn|Error" | sort -u | head -40; done
