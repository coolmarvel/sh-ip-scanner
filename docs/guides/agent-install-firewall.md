---
title: 에이전트 설치관리자 — 자동시작·방화벽 허용 (보안 민감, 승인 필요)
created: 2026-08-05
updated: 2026-08-05
domain: packaging
---

# 에이전트 설치 시 자동시작·방화벽 허용

에이전트가 콘솔의 명령을 받으려면 (1) **로그온 시 자동 실행**되고 (2) **방화벽에서 수신 포트가 허용**
되어야 한다. 이 두 항목은 "부팅 지속성 + 인바운드 방화벽 개방 + 원격 종료"라는 조합이라 보안 도구가
악성코드 패턴으로 오인하기 쉽다 — 본인이 소유·관리하는 사내망 자산관리 용도임이 전제다.

> **상태(2026-08-05)**: 관리자 승인에 따라 **아래 라인이 `installer/agent/sh-agent.iss`(0.2.0)에 반영됨.**
> 이 문서는 근거·대안·수동 적용 참고용으로 유지한다.

## 반영 방법

`installer/agent/sh-agent.iss` 에 아래 섹션을 추가하면 된다(포트 기본 47101).

### 로그온 자동 시작 (레지스트리 Run)
```
[Registry]
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; \
    ValueName: "shAgent"; ValueData: """{app}\ShIpScanner.Agent.exe"""; Flags: uninsdeletevalue
```
- HKLM Run = 모든 사용자 로그온 시 자동 실행. 제거 시 값 삭제.

### 방화벽 인바운드 허용 + 실행/정리
```
[Run]
Filename: "{sys}\netsh.exe"; \
    Parameters: "advfirewall firewall add rule name=""sh Agent"" dir=in action=allow protocol=TCP localport=47101"; \
    Flags: runhidden; StatusMsg: "방화벽 규칙 추가 중..."

[UninstallRun]
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""sh Agent"""; Flags: runhidden
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im ShIpScanner.Agent.exe"; Flags: runhidden
```
- 설치관리자는 `PrivilegesRequired=admin` 이라 netsh/HKLM 쓰기가 가능하다.
- 제거 시 방화벽 규칙 삭제 + 실행 중 에이전트 종료.

## 대안 (더 안전/투명)
- **자동시작**: Run 키 대신 **작업 스케줄러 로그온 트리거** 또는 `{commonstartup}` 바로가기.
- **방화벽**: 조직 GPO(그룹 정책)로 일괄 허용 규칙 배포 → 설치관리자에서 netsh 를 빼는 방법.
- **다중 대역**: 콘솔 PC 에서 타 대역(192.168.85/90) 에이전트에 도달하려면 라우팅/대역 간 방화벽 허용 확인.

## 안전 점검
- 공유 토큰(AuthToken)을 기본값 `change-me` 에서 **반드시 변경**하고 콘솔 설정과 일치시킨다.
- 종료 명령은 에이전트에서 경고→저장유예→연장 절차를 거친다(즉시 강제 종료 아님).
