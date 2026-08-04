---
title: TODO
created: 2026-08-04
updated: 2026-08-04
domain: development
---

# TODO (미해결·향후 작업만 — 완료분은 session-log 로)

우선순위: **P1** 다음 릴리스에서 다뤄야 함 · **P2** 가까운 로드맵 · **P3** 품질 · **P4** 아이디어.
항목에는 대상 파일 경로와 (있다면) 과거 사고 근거를 함께 적는다.

## P1 — 다음 릴리스에서 다뤄야 함

- [ ] **Windows 실환경 검증** — 인스톨러(`sh-ip-scanner-Setup-0.2.0.exe`) 설치 → 원내망(192.168.80.x)
      스캔 → 주황/연두 색칠 + 한글 PC명(NetBIOS) 실제로 잡히는지 확인. 안 잡히면 NetBIOS 타임아웃·
      방화벽(UDP137) 점검. (M1~M3 은 코드·헤드리스 렌더까지 완료, 실측만 남음)

## P2 — 가까운 로드맵

- [ ] 스캔 UX 다듬기 — 진행바(N/254), 정렬, 단일 인스턴스(`Mutex`)
- [ ] **M5(미래) 원격 전원 관리** — 업무 종료 후 대상 PC 자동 종료/재부팅(WMI/RPC, 권한·안전장치). 브리프 미래 확장.

## P3 — 품질

- [ ] `Core.Tests` 보강 — `LocalNetwork` 대역 추출, NBSTAT 응답 파서 단위 테스트(고정 바이트 배열로)
- [ ] NetBIOS 실패 시 UX — 이름 미확인 IP 를 나중에 재조회하는 버튼

## P4 — 아이디어

- [ ] MAC 조회(`SendARP` P/Invoke, `IArpResolver`) + 제조사(OUI) — 원래 M2 였으나 후순위 이월
- [ ] 포트 스캔(TCP connect — 원본 `TIdConnectThread` 대응)
- [ ] 결과 CSV/JSON 내보내기 · 코드 서명(Authenticode) 인증서 적용
