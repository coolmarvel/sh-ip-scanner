---
title: 패키징 가이드 — .NET 자체포함 게시 + Inno Setup (Windows)
created: 2026-08-04
updated: 2026-08-04
domain: packaging
---

# 패키징 가이드 — sh Manager (Windows 설치본)

> **파이프라인 검증 완료 (2026-08-04, v0.1.0)** — 아래 절차로 실제 `Setup.exe` 를 구워 바탕화면에
> 전달했다. WSL(리눅스)에서도 전 과정이 돌아간다: `dotnet publish` 는 크로스 게시, Inno Setup 컴파일은
> **wine** 으로 실행 (`~/.wine/drive_c/Program Files/Inno Setup 6/ISCC.exe`, 헤드리스 OK).
> 발사대의 원본 가이드(`project-seed/guides/desktop-packaging.md`)는 Electron 기준 — 개념 참고용.

## 목표 산출물

- `sh-manager-Setup-<버전>.exe` — 더블클릭하면 설치 마법사가 뜨는 Windows 인스톨러(Inno Setup).
- 설치본은 **자체포함(self-contained)** — 대상 PC에 .NET 런타임이 없어도 실행되도록 런타임을 동봉한다.

## 공통 규칙 (발사대에서 승격된 것)

- **릴리스 전 검증 통과가 먼저다**: `dotnet build -c Release` + `dotnet test` + `dotnet format --verify-no-changes`.
- 산출물 폴더(`publish/`, `installer/Output/`)는 `.gitignore` 대상 — **설치 파일을 git 에 커밋하지 않는다.**
- 공개 배포 시 자산 파일명은 **ASCII** 로 (`sh-manager-Setup-1.0.0.exe`). 한글 파일명 금지(URL/도구 호환).
- 업로드 스크립트는 **버전을 파라미터로** 받게 만든다(릴리스마다 스크립트 복사본이 쌓이지 않도록).

## 1단계 — 자체포함 게시 (dotnet publish)

App 프로젝트를 Windows x64 자체포함으로 게시한다:

```bash
dotnet publish src/ShIpScanner.App/ShIpScanner.App.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o publish/win-x64
```

- `-r win-x64` — 런타임 식별자(RID). 대상 아키텍처. (arm64 도 필요하면 `win-arm64` 로 한 벌 더.)
- `--self-contained true` — .NET 런타임을 함께 담아 런타임 미설치 PC 에서도 실행.
- `-p:PublishSingleFile=true` — 실행 파일을 하나로 묶음(배포 단순화). 필요 시 `PublishTrimmed` 로 용량↓.
- (선택) 소스 보호: .NET 은 IL 이라 디컴파일이 쉽다. 배포본 보호가 필요하면 obfuscator(예: Obfuscar)를
  게시 후 단계에 넣는다 — 원본 faIpScanner 가 네이티브라 분석이 어려웠던 것과의 트레이드오프.

## 2단계 — Inno Setup 스크립트 (installer/sh-manager.iss)

`publish/win-x64` 산출물을 인스톨러로 굽는 `.iss` 스크립트를 작성한다(초안):

```ini
[Setup]
AppName=sh Manager
AppVersion=1.0.0
DefaultDirName={autopf}\sh Manager
DefaultGroupName=sh Manager
OutputBaseFilename=sh-manager-Setup-1.0.0
OutputDir=Output
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\sh Manager"; Filename: "{app}\ShIpScanner.App.exe"
Name: "{autodesktop}\sh Manager"; Filename: "{app}\ShIpScanner.App.exe"

[Run]
Filename: "{app}\ShIpScanner.App.exe"; Description: "실행"; Flags: nowait postinstall skipifsilent
```

빌드 (WSL 에서 wine 으로 — 검증된 실제 명령):

```bash
cd installer
WINEDEBUG=-all wine "C:\\Program Files\\Inno Setup 6\\ISCC.exe" sh-manager.iss
# 결과물: installer/Output/sh-manager-Setup-<버전>.exe
cp Output/sh-manager-Setup-<버전>.exe /mnt/c/Users/user/Desktop/   # 바탕화면 전달
```

(Windows 에서 직접 할 때는 `iscc.exe installer\sh-manager.iss`.) 실제 스크립트는
`installer/sh-manager.iss` — 한국어 설치 UI(`Korean.isl`), 바탕화면/시작메뉴 바로가기 포함.

## 3단계 — 검증

- 인스톨러로 설치 → 바탕화면/시작메뉴 바로가기 → 실행 → 빈 창(및 이후 스캔 UI) 확인.
- 다른(런타임 미설치) PC 에서도 실행되는지 확인(자체포함 검증).

## 열린 항목

- `[?]` 코드 서명 인증서 — 없으면 SmartScreen 경고가 뜬다. 개인 학습 배포엔 수용, 공개 배포 시 재검토.
- `[?]` arm64 동시 배포 여부 — 필요 시 `win-arm64` 게시본을 별도 인스톨러로.
- `[?]` 관리자 권한 필요 여부 — ICMP 핑은 불필요. 설치 위치(`{autopf}`)에 따라 설치 시 권한만 요구.
