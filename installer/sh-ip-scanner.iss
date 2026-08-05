; sh IP Scanner — Inno Setup 스크립트 (docs/guides/packaging.md 참고)
; 저작자/라이센스: 이성현 (SeongHyun Lee). 설치 시 라이센스 동의 페이지에 이름·서명이 표시된다.

#define MyAppName "sh IP Scanner"
#define MyAppVersion "0.4.0"
#define MyAppPublisher "SeongHyun Lee"
#define MyAppExeName "ShIpScanner.App.exe"

[Setup]
; AppId 는 이 앱을 유일하게 식별(업그레이드·제거에 사용). 임의 GUID 고정.
AppId={{9F3C6B54-3E77-4B9C-9A21-5C1D7B0A2E10}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppCopyright=Copyright (C) 2026 SeongHyun Lee
; 설치 마법사 라이센스 페이지에 내 이름·라이센스 전문이 표시된다.
LicenseFile=LICENSE.txt
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=sh-ip-scanner-Setup-{#MyAppVersion}
OutputDir=Output
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\src\ShIpScanner.App\Assets\appicon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
; VersionInfo* 는 Setup.exe 자체 속성(자세히)에 저작자/저작권을 새긴다.
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright=Copyright (C) 2026 SeongHyun Lee
VersionInfoProductName={#MyAppName}
VersionInfoVersion={#MyAppVersion}

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "바탕화면에 바로가기 생성"; GroupDescription: "추가 아이콘:"

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{#MyAppName} 실행"; Flags: nowait postinstall skipifsilent

; ── 코드 서명(Authenticode) 참고 ──────────────────────────────
; 코드 서명 인증서를 보유하면 아래처럼 SignTool 을 등록해 Setup.exe/exe 에 서명할 수 있다.
; (인증서가 있어야 SmartScreen '알 수 없는 게시자' 경고가 사라진다.)
; [Setup] 에: SignTool=mysign
; 컴파일 옵션에: /Smysign="signtool sign /a /fd sha256 /tr http://timestamp.digicert.com /td sha256 $f"
