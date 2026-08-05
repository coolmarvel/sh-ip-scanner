---
title: Plan 0002 — 엔드포인트 에이전트 (관리자 콘솔 + 클라이언트 에이전트)
created: 2026-08-05
updated: 2026-08-05
---

# Plan 0002 — 엔드포인트 에이전트

> 설계·결정은 `docs/adr/0003-endpoint-agent.md`. 상태: `[ ]`미착수 `[~]`진행 `[x]`완료 `[?]`결정필요.
> 큰 기능이라 **단계(Phase)로 쪼갠다**. 각 단계가 검증 가능한 산출물을 낸다.

## Phase 1 — 통신 프로토콜 + 에이전트 코어 (검증 가능한 뼈대) `[x]`

- [x] ADR-0003 결정 기록
- [x] `ShIpScanner.Shared` 프로젝트: 프로토콜(CommandType·CommandRequest·AgentStatus·CommandResponse),
      `AgentProtocol`(포트 47101·JSON 프레이밍), `ISystemController`+`TestSystemController`
- [x] `CommandServer`(에이전트측 TCP 수신·인증·위임) + `AgentClient`(콘솔측 상태조회/명령)
- [x] `ShutdownScheduler`(다음 종료시각·경고창·연장 판정 — 순수 로직)
- [x] 단위 테스트 17개: 스케줄러(경고/종료/연장), 서버↔클라이언트 라운드트립(루프백+TestController), **토큰 거부→무동작**

## Phase 2 — 클라이언트 에이전트(트레이 앱) `[x]`

- [x] `ShIpScanner.Agent` — Avalonia **TrayIcon**(창 없음) 상주, 트레이 메뉴(상태/연장/종료)
- [x] `WindowsSystemController` — 종료/재부팅(shutdown.exe)·잠금(LockWorkStation)·메시지·연장
- [x] 로컬 스케줄 종료: `WarningWindow` 카운트다운 → 저장 유예 → **연장근무자 연기** → 종료
- [x] `AgentConfig`(포트·공유토큰·종료시각·연장허용·최대연장) 로컬 저장(%APPDATA%\sh Agent)
- [x] 에이전트 아이콘(틸 모니터+전원) + 라이센스

## Phase 3 — 관리자 콘솔 통합 `[x]`

- [x] 스캔에 **에이전트 프로브**(AgentClient) → 셀 우상단 **파란 배지** + 로그 "에이전트 vX"
- [x] 셀 **더블클릭 → PC 제어 창**: 종료·재부팅·잠금·메시지·연장 + **RDP 접속**(`mstsc /v:<ip>`)
- [x] 설정에 에이전트 옵션(공유토큰·포트·탐지 여부)
- [ ] (향후) 다중 선택 일괄 종료(연장근무 제외), 감사 로그

## Phase 4 — 에이전트 설치관리자 `[x]`

- [x] `installer/agent/sh-agent.iss` — 관리자 권한 설치 + 라이센스 페이지 + 아이콘
- [x] **방화벽 인바운드 허용(netsh) + 로그온 자동시작(HKLM Run)** — 사용자 승인 후 반영(0.2.0).
      제거 시 방화벽 규칙 삭제 + 에이전트 종료. 배경: `docs/guides/agent-install-firewall.md`
- [ ] 무인 설치 옵션(다수 PC 배포용, `/VERYSILENT` + 토큰/종료시각 파라미터)

## 추가 완료 — 관리자 일정 푸시 + 콘솔 명칭

- [x] **종료 시각은 관리자만 설정**: `SetSchedule` 명령 추가(Shared). 에이전트는 표시·연장(허용 시)만.
      콘솔 PC 제어 창에 일정 섹션(사용여부·종료시각·자체연장 허용) + 전송. 에이전트 트레이는 확인만.
- [x] 콘솔 명칭 **sh IP Scanner → sh Manager**(에이전트 `sh Agent` 와 짝). 어셈블리명 ShIpScanner.* 유지.

## 열린 결정 `[?]`

- `[?]` 공유 토큰 배포 방법(설치 시 주입 vs 콘솔에서 설정). MVP는 설치 파라미터.
- `[?]` 연장근무 예외를 사용자 자가 연기로 할지, 관리자 승인/화이트리스트로 할지. MVP는 자가 연기(N회/최대시간 제한).
- `[?]` 다중 대역 방화벽·라우팅 정책 확인(콘솔 PC에서 타 대역 에이전트 도달 가능한지).
