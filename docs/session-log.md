---
title: 세션 로그
created: 2026-08-04
updated: 2026-08-04
domain: development
---

# 세션 로그 (최신이 위)

이 파일이 **"언제 무슨 일이 있었나"의 SSOT**다. 세션마다 최상단에 블록 추가.
(커밋/푸시는 사용자가 직접·성긴 단위 — git history 를 이력 SSOT 로 삼지 않는다.)

블록 형식: `## YYYY-MM-DD — 제목` 아래에 **요청/피드백 → 수정 → 검증 → 다음** 순서로 간결하게.

## 2026-08-05 — 엔드포인트 에이전트 착수: 설계 + 통신 코어(Phase 1)

- **요청**: 관리자가 클라이언트 PC 를 제어(원격 접속·업무종료 후 자동 종료, 연장근무 예외).
  에이전트를 각 PC 에 설치(트레이 상주·자동시작), 콘솔에서 설치 여부·상태 조회.
- **결정(AskUserQuestion)**: 저장소 확장(Admin+Agent+Shared) · **LAN 직접** 통신(+설치 시 방화벽/권한) ·
  **Windows RDP** 활용 · 자동 종료 **스케줄+수동 둘 다**. → `docs/adr/0003-endpoint-agent.md`.
- **수정(Phase 1)**: `ShIpScanner.Shared` 신설 — 프로토콜(Status/Shutdown/Reboot/Lock/Message/Extend),
  `CommandServer`(TCP 수신·**AuthToken 인증**·ISystemController 위임), `AgentClient`(상태조회/명령),
  `ShutdownScheduler`(종료시각·경고·연장 순수 로직), `TestSystemController`.
  **안전장치 설계에 못박음**: 인증된 명령만·부드러운 종료(경고→저장유예→연장)·감사.
- **검증**: build 성공, `dotnet test` **17개**(스케줄러·서버/클라이언트 루프백 왕복·**잘못된 토큰→무동작**).
- **다음(Phase 2)**: 에이전트 트레이 앱(Avalonia TrayIcon) + WindowsSystemController + 로컬 스케줄 경고 UI.
  이어서 Phase 3 콘솔 통합(설치여부 표식·명령·RDP), Phase 4 에이전트 설치관리자(방화벽·자동시작).

## 2026-08-05 — 좌상단 메뉴+모달 · UI 정리 · 대역 오타 교정 (v0.5.0)

- **피드백**: (1) 기본 대역 오타 — 95 가 아니라 **192.168.90**. (2) 내 IP~대역관리가 한 줄에 몰려
  어색 → 좌상단 아이콘 메뉴를 눌러 **대역 관리/설정을 모달**로(옛 실행기 느낌). (3) 내 IP 가 너무 흐림.
  (4) 전체 폰트·정렬 정리, 내 IP 적절히 배치.
- **수정**: `Core/Config/ScanSettings`·`ScanSettingsStore`(settings.json). `SubnetStore` 에 오타 세트
  자동 교정(95→90) 마이그레이션. VM 에 스캔 옵션(타임아웃·동시수·이름조회) + `SaveSettings`.
  Views: `SubnetManagerWindow`·`SettingsWindow`·`AboutWindow` 모달 + MainWindow 좌상단 아이콘 MenuFlyout.
  메인은 대역 드롭다운+검색만 남기고 내 IP 를 칩으로, 폰트 통일(기본 13, 설명 14, 제목 18).
- **검증**: build·format clean, `dotnet test` **11개**(대역·설정 클램프·마이그레이션). 헤드리스로
  메인/대역관리/설정/정보 4화면 렌더 확인(대역 80/85/90). 인스톨러 `0.5.0` 바탕화면 교체(0.4.0 삭제).
- **다음**: Windows 실환경에서 메뉴·모달·대역 전환·설정 반영 확인.

## 2026-08-05 — 여러 대역 드롭다운(관리자 선택) (v0.4.0)

- **요청**: 운영 대역이 192.168.80/85/95 세 개다. 이걸 골라 스캔하게 드롭다운으로. 관리자 컨트롤 목적.
- **답변/설계**: 단일 PC 는 자기 대역만 자동 감지 가능(나머지는 라우터 너머라 자동 발견 불가) →
  **편집 가능한 대역 목록 + 드롭다운** 방식 채택. 기본값에 3개 대역 시드, 관리자가 추가/삭제.
- **수정**: `Core/Config/SubnetDefinition`·`SubnetStore`(JSON, `%APPDATA%\sh IP Scanner\subnets.json`).
  `MainViewModel` 에 `Subnets`·`SelectedSubnet`·Add/Remove 커맨드, 선택 변경 시 바둑판 리셋.
  View: 로컬 IP 텍스트박스 → **대역 ComboBox + 추가/삭제**. 내 IP 는 안내로 표시.
- **검증**: build·format clean, `dotnet test` **9개 통과**(대역 검증·기본값). 헤드리스 렌더로 드롭다운/
  관리 UI 확인. 인스톨러 `0.4.0` 재컴파일 → 바탕화면 교체(0.3.0 삭제).
- **다음**: Windows 실환경에서 대역 전환 스캔 확인. (향후 M5 원격 전원 관리 시 대역별 대상 관리에 활용)

## 2026-08-04 — UI 정정(확대 제거·폭맞춤) + 라이센스 통일 + 발사대 승격 (v0.3.0)

- **피드백**: (1) "확대"는 **창 최대화 버튼**을 뜻한 것 — 내가 넣은 콘텐츠 확대/축소는 오해였으니 제거,
  최소화·최대화·닫기 창 버튼을 원함. (2) **바둑판을 화면 폭에 맞게** 신축. (3) 라이센스 이름은
  **이성현 (SeongHyun Lee)** (pdf-editor 등 참고). (4) 이 라이센스 규칙을 **project-seed 에 문서화**해
  앞으로 언어·프레임워크 무관하게 자동 적용되게.
- **수정(sh-ip-scanner)**:
  - MainWindow: 확대 UI·`LayoutTransformControl` 제거, `MainViewModel` 의 Zoom·ZoomIn/Out 제거.
    `CanResize=True`(OS 최소/최대/닫기). 바둑판 `UniformGrid Columns=15` + 셀 `Stretch` + 가로 스크롤 Disabled → 폭 신축.
  - 라이센스: 루트 `LICENSE` 추가, `installer/LICENSE.txt`(UTF-8 BOM)·`.iss`·csproj·헤더 표기 모두 이성현으로.
    데모/주석의 병원 부서명은 일반 예시로 이미 교체 완료.
- **수정(project-seed 승격)**: `guides/licensing-and-branding.md`(제작자 정체 SSOT + 스택별 적용법) 신설,
  `templates/LICENSE`(자동 인스턴스화, `{{PROJECT_NAME}}`·`{{YEAR}}`) 추가, `INITIALIZE.md` 에 `{{YEAR}}`
  치환 + 7단계(라이센스 적용) 추가, 발사대 `CLAUDE.md` 구조에 반영.
- **검증**: build(0/0)·test(1/1)·format(clean). **헤드리스 렌더**로 확대 제거·폭맞춤·라이센스 표기 확인.
  인스톨러 재컴파일 → `sh-ip-scanner-Setup-0.3.0.exe` 바탕화면 교체(0.2.0 삭제). seed 템플릿 플레이스홀더 무손상 확인.
- **다음**: 사용자 Windows 실환경 설치·스캔 검증(원내망 192.168.80.x, 한글 PC명).

## 2026-08-04 — 실제 UI(바둑판) 구현 + 아이콘/라이센스 인스톨러 v0.2.0

- **피드백**: 루트에 원본 스크린샷 첨부. 실제 UI 는 DataGrid 가 아니라 **254칸 바둑판**(흰→주황=사용중/
  연두=사용가능, 셀에 한글 PC명). 로컬 IP 자동감지(192.168.80.x). 우상단에 **확대 버튼 신규 추가**,
  **아이콘 제작**, 설치 시 **내 이름·라이센스** 명시, **cm병원 관련 배제**. 배경: 전산 관리 목적 +
  미래에 업무종료 후 자동 종료/재부팅 확장.
- **수정**:
  - Core: `LocalNetwork`(IPv4 자동감지) · `SubnetScanner`(async 병렬 핑, SemaphoreSlim/CancellationToken) ·
    `NetBiosNameResolver`(UDP137 NBSTAT + **CP949 한글 디코드**) + `ReverseDnsResolver` 폴백.
  - App: `MainViewModel`(254셀·로그·확대·스캔) · `HostCellViewModel` · `HostStateToBrushConverter` ·
    `MainWindow.axaml`(바둑판/로그창/로컬IP/검색·중지·종료/우상단 확대/라이센스 표기).
  - 아이콘 `appicon.ico/.png`(PIL 로 제작). csproj 에 저작자·저작권·아이콘 메타데이터.
  - 인스톨러: `LICENSE.txt`(저작자 Seonghyun, 설치 동의 페이지) + `.iss` 에 AppPublisher/Copyright/
    VersionInfo/SetupIcon/desktopicon, 코드서명 SignTool 훅 주석. cm병원/falinux 참조는 원래 없음(확인).
  - 피드백 스크린샷 → `docs/feedback-archive/2026-08-04-ui-reference/` 로 이동.
- **검증**: `dotnet build`(0/0)·`dotnet test`(1/1)·`dotnet format`(clean). **Avalonia 헤드리스로 UI 를
  PNG 캡처**해 초기 흰 바둑판 + 스캔 상태(주황/연두 + 한글명 렌더)를 눈으로 확인. 인스톨러 재컴파일
  (wine ISCC) → `sh-ip-scanner-Setup-0.2.0.exe` 바탕화면 교체(0.1.0 삭제).
- **다음**: 사용자가 Windows 에서 인스톨러 설치→실제 원내망(192.168.80.x) 스캔으로 PC명 조회 확인.
  이후 다듬기(진행바·정렬) 또는 미래 M5(원격 전원 관리) 논의.

## 2026-08-04 — 하네스 자동화 + v0.1.0 인스톨러 선검증

- **요청**: 권한·환경 변수·훅으로 dotnet 빌드가 자동으로 돌게 설정(project-seed 에도 반영),
  모델 fable 고정, 그리고 바탕화면에 설치 프로그램 굽기.
- **수정**:
  - `dotnet` 을 `~/.local/bin/dotnet` 심링크로 PATH 에 상시 노출 (모든 셸에서 plain `dotnet` 동작).
  - `.claude/settings.json`: `model: fable` 고정, `Bash(dotnet …)` 서브커맨드 권한 허용,
    env(`DOTNET_ROOT`·텔레메트리 옵트아웃), **`build-check.sh` 훅 신설** — `.cs/.axaml/.csproj`
    저장 시 백그라운드 `dotnet build`, 실패하면 에이전트를 깨움(asyncRewake, exit 2).
    project-seed 저장소에도 동일한 model/env/권한 settings 생성(훅 제외).
  - **M4 선검증**: `dotnet publish`(win-x64 자체포함, 89MB) → Inno Setup 6.7.3 을 wine 에 헤드리스
    설치 → `installer/sh-ip-scanner.iss` 작성(한국어 UI) → wine ISCC 컴파일 성공(31MB).
- **검증**: build-check 훅 3경로(정상 0/스킵 0/고장 2) 파이프테스트 통과, settings JSON 2건 구조 검증,
  `Setup.exe` 산출 후 `/mnt/c/Users/user/Desktop/sh-ip-scanner-Setup-0.1.0.exe` 복사 확인.
- **다음**: 사용자가 Windows 에서 인스톨러 실행해 M0 창 확인. 이후 M1(대역 파싱+핑 스윕) 착수.
  주의: 새 hooks/settings 는 다음 세션(또는 `/hooks` 열기)부터 적용된다.

## 2026-08-04 — 킥오프 완료

- **요청**: TS/React/Electron 대신 **C# 을 배우면서** 만드는 프로젝트. 기존 포터블 툴
  `faIpScanner.exe` 처럼 IP 스캔을 하는 Windows 데스크톱 앱. 이름 **sh IP Scanner**. 설계도(청사진)와
  C# 채택 근거를 문서로 잘 남길 것.
- **벤치마킹 분석**: `faIpScanner.exe` 정적 분석 → **Delphi(VCL/Indy) 네이티브 Win32 x86 GUI**.
  핵심 동작 = 멀티스레드 서브넷 스윕 + `SendARP`(MAC) + 역DNS(호스트명). wine 로 구동은 되나
  헤드리스 WSL 이라 GUI 창은 미표시.
- **결정(사용자 선택)**: 언어 **C#/.NET 8** · GUI **Avalonia(MVVM)** · 설치 **Inno Setup** ·
  v1 범위 **코어 재현**(살아있는 IP + MAC + 호스트명) · 학습 밀도 **개념 주석 풍부하게**.
  근거는 `docs/adr/0002-stack.md`(왜 C#·왜 Avalonia·왜 Inno Setup).
- **산출물**: `docs/brief.md`(confirmed) · `docs/adr/0002-stack.md` · `docs/plans/0001-mvp.md`(M0~M4) ·
  스캐폴드(`ShIpScanner.sln` + Core/App/Tests 3 프로젝트) · 패키징 가이드(.NET/Inno 버전으로 교체).
- **스캐폴드 이슈 해결**: Avalonia 템플릿이 net10.0/Avalonia 12 를 잡아 SDK8 로 빌드 실패 →
  **net8.0 + Avalonia 11.2.2** 로 고정, `[ObservableProperty]` 는 필드 기반으로 수정, Program.cs 의
  v12 API(`WithDeveloperTools`) 제거.
- **검증**: `dotnet build`(0/0) · `dotnet test`(1/1) · `dotnet format --verify-no-changes`(clean) 통과.
- **하네스**: `format.sh`(dotnet format) PostToolUse 연결. `.gitignore` .NET 보강.
- **다음**: M1 — `IpRange` 파싱 + 병렬 핑 스윕(async/SemaphoreSlim/CancellationToken) + 결과 DataGrid.
  첫 커밋은 사용자가 직접(`init:` 메시지 전달됨).
