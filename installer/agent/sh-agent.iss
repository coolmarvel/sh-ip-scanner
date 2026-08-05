; sh Agent — 클라이언트 PC 설치용 (기본 설치본)
; 저작자/라이센스: 이성현 (SeongHyun Lee).
; NOTE: 로그온 자동시작 + 방화벽 인바운드 허용은 보안 민감 항목이라 이 스크립트에서 분리했다.
;       필요한 [Registry]/[Run] 라인은 docs/guides/agent-install-firewall.md 참고(사용자 승인 후 추가).

#define MyAppName "sh Agent"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "SeongHyun Lee"
#define MyAppExeName "ShIpScanner.Agent.exe"

[Setup]
AppId={{B7E2A9C1-6F4D-4E8A-9C33-2A1E5D7F0B22}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppCopyright=Copyright (C) 2026 SeongHyun Lee
LicenseFile=LICENSE.txt
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=sh-agent-Setup-{#MyAppVersion}
OutputDir=Output
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\..\src\ShIpScanner.Agent\Assets\agenticon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright=Copyright (C) 2026 SeongHyun Lee
VersionInfoProductName={#MyAppName}
VersionInfoVersion={#MyAppVersion}

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Files]
Source: "..\..\publish-agent\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "지금 sh Agent 실행"; Flags: nowait postinstall skipifsilent
