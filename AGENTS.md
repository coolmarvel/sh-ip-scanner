# AGENTS.md — sh IP Scanner

이 파일은 **모든 AI 코딩 에이전트**(Claude Code, Codex, Cursor, Copilot 등)를 위한 진입점이다.
도구에 상관없이 아래 규칙을 따른다. Claude Code 전용 상세(부팅 프로토콜·하네스)는 `CLAUDE.md`.

## 이 프로젝트가 무엇인가

LAN 서브넷을 병렬 핑 스윕해 살아있는 호스트의 **IP·MAC·호스트명**을 표로 보여주는 **Windows
데스크톱 IP 스캐너**. 기존 포터블 툴 `faIpScanner.exe`(Delphi)를 벤치마킹해 **C# / .NET 8 /
Avalonia(MVVM) / Inno Setup** 으로 재현한다. 결과물과 **C# 학습**이 동시 목적 — 코드에 개념 주석을
풍부하게 단다. 상세: `docs/brief.md`(왜/무엇), `docs/adr/0002-stack.md`(스택 근거).

## 절대 규칙 (위반 금지)

1. **작업 전에 읽는다**: `docs/session-log.md`(진행 이력 SSOT) → `docs/todo.md` → 최근 `docs/plans/*` 1개.
2. **코드를 바꾸면 같은 턴에 문서를 갱신한다**: session-log 최상단 블록 + todo. 규칙은 `docs/writing-guide.md`.
3. **검증 없이 전달하지 않는다**: `dotnet build ShIpScanner.sln -c Debug && dotnet test ShIpScanner.sln && dotnet format ShIpScanner.sln --verify-no-changes` 통과가 커밋 메시지 작성의 전제.
4. **`.env` 를 직접 수정하지 않는다.** `.env.example` 수정 또는 사용자에게 요청.
5. **`git add -A` / `git add .` 금지.** 파일을 지정해서 stage 한다. **커밋/푸시는 사용자가 직접.**
6. **스택·구조 변경은 ADR 로**: 구조적 결정은 `docs/adr/NNNN-*.md` 에 근거와 함께 기록한다.
7. **버전의 MINOR 승격은 사용자 선언이 있을 때만.**
8. **브리프가 판정 기준**: 설계 논쟁·스코프 논쟁은 `docs/brief.md` 로 돌아와 판정한다.

## 커밋 컨벤션

Conventional Commits — `<type>: <한국어 제목>` + 리스트형 본문 + `Co-Authored-By` 트레일러.
type: feat, fix, refactor, chore, docs, style, test, perf, ci, build, revert, init, remove, rename, hotfix.

## 더 읽을 것

- `CLAUDE.md` — 부팅 프로토콜, 변경 후 자동 규칙, 코드 지도, 하네스 상세
- `docs/brief.md` — 왜/무엇 SSOT · `docs/writing-guide.md` — 문서 규칙
