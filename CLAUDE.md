# CLAUDE.md — sh IP Scanner 작업 가이드

이 파일은 **세션이 바뀌어도 맥락을 즉시 복구**하기 위한 진입점이다. Claude Code는 세션 시작 시
이 파일을 자동으로 읽는다. (도구 중립 절대 규칙은 `AGENTS.md`.)

> [project-seed](https://github.com/coolmarvel/project-seed) 에서 2026-08-04 에 생성됨.
> `<!-- TODO(kickoff) -->` 가 남아 있으면 킥오프가 끝나지 않은 것이다 — 채우기 전에 기능 작업을 시작하지 않는다.

## 🟢 세션 시작 부팅 프로토콜 (매 세션 첫 작업 전에 반드시 수행)

새 세션에서 작업을 시작하면, **코드를 건드리기 전에** 순서대로:

1. `docs/session-log.md` 읽기 — 마지막으로 무엇을 했고 지금 어디쯤인지 (**진행 이력 SSOT**)
2. `docs/todo.md` 읽기 — 남은 일 (P1~P4)
3. 최근 `docs/plans/*.md` 1개 읽기 — 진행 중 기능의 설계 의도
4. 사용자 피드백 확인: **프로젝트 루트에 놓인 스크린샷(`*.png`/`*.jpg`)이나 `FEEDBACK.md` = 미처리 피드백**으로 읽는다. 반영한 뒤에는 `docs/feedback-archive/YYYY-MM-DD-*/` 로 옮긴다. (개인 학습 툴이라 채널은 단순하게 — 필요 시 사용자와 재확정)
5. `git status` 에 모르는 변경이 있으면 session-log 와 대조 — 다른 세션의 흔적일 수 있다. 출처 불명이면 사용자에게 확인.

## 🔴 변경 후 자동 규칙 (사용자가 매번 요청하지 않아도 수행)

1. 코드를 바꾸면 **같은 턴에** `docs/session-log.md`(최상단 블록 추가)·`docs/todo.md`
   (+릴리스급이면 `docs/changelog.md`)를 갱신한다. 문서 규칙은 `docs/writing-guide.md`.
2. 검증을 통과하기 전에는 커밋 메시지 작성/산출물 전달을 하지 않는다:
   `dotnet build ShIpScanner.sln -c Debug && dotnet test ShIpScanner.sln && dotnet format ShIpScanner.sln --verify-no-changes`
   (셋 다 통과가 전제. `dotnet` 이 PATH 에 없으면 `export PATH="$HOME/.dotnet:$PATH"`.)
3. 버전을 판단해 올린다 (아래 "버전 정책").
4. 산출물 전달: v1 은 로컬 실행(`dotnet run --project src/ShIpScanner.App`) 안내가 기본. 릴리스는 `dotnet publish` + Inno Setup 으로 `Setup.exe` 를 굽는다(→ `docs/guides/packaging.md`, M4).

## 버전 정책 (semver `MAJOR.MINOR.PATCH`)

**MINOR 승격은 사용자만 선언한다.** 에이전트가 판단해서 올리지 않는다.

- **PATCH** — 기본값. 다듬는 중인 기능의 수정 하나 반영할 때마다 +1.
- **MINOR** — 사용자가 "이 기능은 더 수정할 게 없다, 넘어가자"라고 선언한 그 시점에만.
- **MAJOR** — 대규모 재설계/호환 깨짐. 사용자와 상의.

## 커밋 컨벤션

Conventional Commits — `<type>: <한국어 제목>` + 리스트형 본문. **커밋/푸시는 사용자가 직접**
(에이전트는 요청 시 커밋 메시지만 작성). 메시지 끝에 `Co-Authored-By` 트레일러.

type: `feat` `fix` `refactor` `chore` `docs` `style` `test` `perf` `ci` `build` `revert` `init` `remove` `rename` `hotfix`

## 협업 규칙

- **진행 이력의 SSOT 는 `docs/session-log.md`** — git history 가 아니다 (커밋이 성긴 단위라서).
- 세션이 끊겨도 이어서 작업할 수 있게 **모든 진행 사항을 `docs/` 에 파일로 기록**한다.
- 설계 논쟁이 생기면 `docs/brief.md`(왜/무엇 SSOT)로 돌아와 판정한다. 브리프 밖 기능은 스코프 확인 먼저.
- `.env` 는 직접 수정하지 않는다 — `.env.example` 수정 또는 사용자에게 요청 (훅이 차단함).
- 규칙이 미확정이면 이 파일에 `<!-- TODO -->` 로 남기고, 확정되는 순간 채운다.

## 이 프로젝트가 뭔가

사내망 PC 를 파악·관리하는 **관리자 콘솔 + 클라이언트 에이전트** 모음(저장소 `sh-pc-manager`,
내부 솔루션명 `ShIpScanner.*` 유지). 콘솔("sh IP Scanner")은 대역을 핑 스윕해 IP·PC명을 바둑판으로
보여주고(벤치마킹: `faIpScanner`), 에이전트("sh Agent")는 각 PC 트레이에 상주해 업무종료 자동 종료·
콘솔 명령(종료/재부팅/잠금/메시지/연장·RDP)을 수행한다. C#/.NET 8 + Avalonia. **결과물 + C# 학습**이
동시 목적이라 개념 주석을 풍부하게 단다. 왜/무엇: `docs/brief.md`, 스택: `docs/adr/0002-stack.md`,
에이전트 아키텍처: `docs/adr/0003-endpoint-agent.md`.

## 문서 인덱스 (docs/)

| 파일 | 용도 |
|---|---|
| `docs/writing-guide.md` | **문서 지배 규칙** (SSOT·frontmatter·코드 1:1 대조). 문서 쓰기 전 필독 |
| `docs/brief.md` | 프로젝트 **왜/무엇 SSOT** — 킥오프 산출물 |
| `docs/session-log.md` | 세션별 진행 이력. **"언제 무슨 일" SSOT** (최신이 위) |
| `docs/todo.md` | 미해결·향후 작업만 (P1~P4). 완료분은 session-log 로 |
| `docs/changelog.md` | 릴리스 단위 사람용 요약 |
| `docs/adr/*.md` | 구조적 결정 기록 — 왜 이렇게 했는가 (`NNNN-kebab.md`) |
| `docs/plans/*.md` | 앞으로 만들 것 — 기능 단위 구현 계획 (역할 구분은 `plans/README.md`) |
| `docs/guides/*.md` | 현재 구현된 동작·코드 위치 (기능별) |
| `docs/feedback-archive/` | 처리 완료한 사용자 피드백 보관소 |

## 자주 쓰는 명령

```bash
# 최초 1회: dotnet 이 PATH 에 없으면
export PATH="$HOME/.dotnet:$PATH"

# 앱 실행 (GUI — Windows/데스크톱 세션 필요. 헤드리스 WSL 에선 창이 안 뜸)
dotnet run --project src/ShIpScanner.App

# 검증 3종 (커밋 전 전부 통과)
dotnet build ShIpScanner.sln -c Debug
dotnet test  ShIpScanner.sln
dotnet format ShIpScanner.sln --verify-no-changes

# 릴리스 게시 + 인스톨러 (검증됨 — 상세는 docs/guides/packaging.md)
dotnet publish src/ShIpScanner.App/ShIpScanner.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
cd installer && WINEDEBUG=-all wine "C:\\Program Files\\Inno Setup 6\\ISCC.exe" sh-ip-scanner.iss
cp installer/Output/sh-ip-scanner-Setup-*.exe /mnt/c/Users/user/Desktop/
```

## 코드 지도 (수정 시 어디를 보나)

| 위치 | 역할 |
|---|---|
| `src/ShIpScanner.Core/Net/` | `LocalNetwork` — 주 IPv4 자동 감지 + `/24` 대역 추출 |
| `src/ShIpScanner.Core/Config/` | `SubnetDefinition`·`SubnetStore`(대역 목록, subnets.json, 95→90 마이그레이션) · `ScanSettings`·`ScanSettingsStore`(스캔 옵션, settings.json) — 둘 다 `%APPDATA%\sh IP Scanner\` |
| `src/ShIpScanner.Core/Scanning/` | `SubnetScanner`(병렬 핑 스윕) · `PingOutcome`(record) · `HostState`(enum) |
| `src/ShIpScanner.Core/Naming/` | `IHostNameResolver` + `NetBiosNameResolver`(UDP137, CP949 한글) · `ReverseDnsResolver` · `CompositeHostNameResolver` |
| `src/ShIpScanner.App/ViewModels/` | `MainViewModel`(상태·커맨드·스캔 오케스트레이션) · `HostCellViewModel`(셀) |
| `src/ShIpScanner.App/Views/` | `MainWindow.axaml`(바둑판, 좌상단 아이콘 MenuFlyout) + 모달 `SubnetManagerWindow`·`SettingsWindow`·`AboutWindow`. 창 최소/최대/닫기는 OS 타이틀바(CanResize) |
| `src/ShIpScanner.App/Converters/` | `HostStateToBrushConverter`(상태→색) |
| `src/ShIpScanner.App/Assets/` | `appicon.ico/.png`(바둑판+돋보기 아이콘) |
| `src/ShIpScanner.Shared/` | **콘솔↔에이전트 공통** — Protocol(명령/상태) · `CommandServer`·`AgentClient` · `ShutdownScheduler` · `ISystemController` |
| `src/ShIpScanner.Agent/` | **클라이언트 에이전트**(트레이) — `App`(트레이) · `AgentService`(서버+스케줄) · `WindowsSystemController` · `WarningWindow`·`MessageWindow` · `AgentConfig` |
| `src/ShIpScanner.App/Views/PcControlWindow` | 콘솔의 PC 제어 창(더블클릭) — 종료/재부팅/잠금/메시지/연장/RDP |
| `src/ShIpScanner.Core.Tests/` | xUnit — 대역·설정·마이그레이션 + 에이전트 프로토콜(스케줄러·서버/클라이언트·인증) |
| `installer/` | 콘솔 `sh-ip-scanner.iss` / 에이전트 `agent/sh-agent.iss` · `LICENSE.txt`(BOM) · 루트 `LICENSE` |
| `tools/ShotTool/` | (개발용) Avalonia 헤드리스로 UI 를 PNG 캡처 — 솔루션·배포에 미포함 |

**새 기능 추가 = ① `Core` 에 로직·모델 (+ `Core.Tests` 에 테스트) → ② `App/ViewModels` 에 상태·커맨드 바인딩 → ③ `App/Views` XAML 에 화면 → ④ 검증 3종 통과.**
디자인 근거는 스크린샷: `docs/feedback-archive/2026-08-04-ui-reference/`. 색 규칙: 흰=미확인·주황=사용중·연두=사용가능.

> 함정 박제(어기면 재발):
> - **App 은 net8.0 고정** — Avalonia 템플릿이 net10.0 을 잡으면 SDK8 로 빌드 불가. 마찬가지로 **Avalonia 는 11.2.x** (12.x 는 SDK8 의 Roslyn 4.11 과 소스제너레이터 비호환).
> - `[ObservableProperty]` 는 **필드 기반**(`private string _x;`)으로 쓴다 — C# 13 의 `partial property` 문법은 SDK8(C#12)에서 안 됨.
> - `SendARP` 등 Win32 P/Invoke 는 **Windows 전용** — 반드시 `IArpResolver` 뒤로 격리하고 비-Windows 폴백을 둔다.

## 하네스 (Harness Engineering)

지시문이 아니라 시스템으로 제약한다. 도입 근거: `docs/adr/0001-harness-engineering.md`.

| 축 | 내용 |
|---|---|
| Hooks (`scripts/hooks/`) | `env-guard.sh`(.env 편집 차단) · `git-add-guard.sh`(`git add -A/.`·.env staging 차단) · `format.sh`(`.cs`/`.axaml` 저장 시 `dotnet format`) · `build-check.sh`(`.cs`/`.axaml`/`.csproj` 저장 시 백그라운드 `dotnet build`, 실패 시 에이전트 통지) |
| Commands (`.claude/commands/`) | `/review-security` · `/deploy-check` (내장 `/code-review` 로 diff 리뷰. 스택 전용 커맨드는 추후 필요 시) |
| MCP (`.mcp.json`) | `context7`(라이브러리 문서 — Avalonia/.NET API 조회에 유용) · `playwright`(브라우저 QA — **이 데스크톱 앱엔 미사용**) |
| Skills | (없음. .NET 전용 스킬 도입 시 skills-lock.json + scripts/install-skills.sh 방식으로 락) |
