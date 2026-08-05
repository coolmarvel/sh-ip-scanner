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

## Phase 2 — 클라이언트 에이전트(트레이 앱) `[ ]`

- [ ] `ShIpScanner.Agent` — Avalonia **TrayIcon**(창 없음) 상주, 부팅/로그온 자동 실행
- [ ] `WindowsSystemController` — 종료/재부팅/잠금/메시지(P/Invoke·shutdown.exe·rundll32)
- [ ] 로컬 스케줄 종료: 트레이 경고 카운트다운 → 저장 유예 → **연장근무자 연기** → 종료
- [ ] 에이전트 설정(포트·공유토큰·종료시각·연장허용) 로컬 저장

## Phase 3 — 관리자 콘솔 통합 `[ ]`

- [ ] 스캔에 **에이전트 프로브** 추가 → 셀에 "에이전트 설치됨 vX / 미설치" 표식
- [ ] PC 선택 → 명령(종료·재부팅·잠금·메시지·연장) 전송, **RDP 접속**(`mstsc /v:<ip>`)
- [ ] 대상 다중 선택(대역 전체/연장근무 제외 일괄 종료)
- [ ] 감사 로그(누가·언제·무슨 명령)

## Phase 4 — 에이전트 설치관리자 `[ ]`

- [ ] `installer/agent/*.iss` — 관리자 권한 설치, **방화벽 인바운드 허용(netsh)**, **자동시작 등록**,
      공유토큰·종료시각 설정 주입
- [ ] 무인 설치 옵션(다수 PC 배포용, `/VERYSILENT` + 파라미터)

## 열린 결정 `[?]`

- `[?]` 공유 토큰 배포 방법(설치 시 주입 vs 콘솔에서 설정). MVP는 설치 파라미터.
- `[?]` 연장근무 예외를 사용자 자가 연기로 할지, 관리자 승인/화이트리스트로 할지. MVP는 자가 연기(N회/최대시간 제한).
- `[?]` 다중 대역 방화벽·라우팅 정책 확인(콘솔 PC에서 타 대역 에이전트 도달 가능한지).
