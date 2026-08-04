#!/bin/bash
# PostToolUse hook (asyncRewake): C# 소스/프로젝트 파일이 바뀌면 백그라운드로 dotnet build 를 돌려
# 컴파일이 깨졌으면 에이전트를 깨워 알린다 (exit 2 = 실패 통지). ADR-0001 하네스 참고.
set -uo pipefail

FILE_PATH=$(python3 -c "
import json, os
data = json.loads(os.environ.get('CLAUDE_TOOL_INPUT', '{}'))
print(data.get('file_path', ''))
" 2>/dev/null) || exit 0

# 빌드에 영향 주는 파일만 대상 (.cs / .axaml / .csproj). 문서 등은 조용히 통과.
case "$FILE_PATH" in
  *.cs|*.axaml|*.csproj) ;;
  *) exit 0 ;;
esac

DOTNET_BIN=""
if command -v dotnet >/dev/null 2>&1; then DOTNET_BIN="dotnet"
elif [ -x "$HOME/.dotnet/dotnet" ]; then DOTNET_BIN="$HOME/.dotnet/dotnet"
else exit 0; fi

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SLN="$REPO_ROOT/ShIpScanner.sln"
[ -f "$SLN" ] || exit 0

OUT=$(DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 "$DOTNET_BIN" build "$SLN" -c Debug 2>&1)
if [ $? -ne 0 ]; then
  echo "dotnet build FAILED after editing $FILE_PATH:"
  echo "$OUT" | grep -E "error|Build FAILED" | head -15
  exit 2   # asyncRewake: 에이전트를 깨워 빌드 실패를 알린다
fi
exit 0
