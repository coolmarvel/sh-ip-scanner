; sh Agent — 클라이언트 PC 설치용 (관리자 승인: 로그온 자동시작 + 방화벽 인바운드 허용 포함)
; 저작자/라이센스: 이성현 (SeongHyun Lee). 용도: 본인이 관리하는 사내망 PC 전원·자산 관리.
; 배경/대안: docs/guides/agent-install-firewall.md

#define MyAppName "sh Agent"
#define MyAppVersion "0.2.0"
#define MyAppPublisher "SeongHyun Lee"
#define MyAppExeName "ShIpScanner.Agent.exe"
#define AgentPort "47101"

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

[Registry]
; 로그온 시 자동 시작(모든 사용자). 제거 시 값 삭제.
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; \
    ValueName: "shAgent"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue

[Run]
; 방화벽 인바운드 허용(에이전트 포트) — 관리자 콘솔이 접속할 수 있게.
Filename: "{sys}\netsh.exe"; \
    Parameters: "advfirewall firewall add rule name=""sh Agent"" dir=in action=allow protocol=TCP localport={#AgentPort}"; \
    Flags: runhidden; StatusMsg: "방화벽 규칙 추가 중..."
; 설치 직후 바로 실행(트레이 상주).
Filename: "{app}\{#MyAppExeName}"; Description: "지금 sh Agent 실행"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""sh Agent"""; Flags: runhidden
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im {#MyAppExeName}"; Flags: runhidden
