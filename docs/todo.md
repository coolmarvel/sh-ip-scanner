---
title: TODO
created: 2026-08-04
updated: 2026-08-05
domain: development
---

# TODO (미해결·향후 작업만 — 완료분은 session-log 로)

우선순위: **P1** 다음 릴리스에서 다뤄야 함 · **P2** 가까운 로드맵 · **P3** 품질 · **P4** 아이디어.
항목에는 대상 파일 경로와 (있다면) 과거 사고 근거를 함께 적는다.

## P1 — 다음 릴리스에서 다뤄야 함

- [ ] **Windows 실환경 검증** — 인스톨러(`sh-ip-scanner-Setup-1.0.1.exe`) 설치 → **첫 실행
      안내 모달→대역 등록** 플로우 확인 → 원내망 스캔 → 주황/연두 색칠 + 한글 PC명(NetBIOS)
      실제로 잡히는지 확인. 안 잡히면 NetBIOS 타임아웃·방화벽(UDP137) 점검.
- [ ] **public 전환 전 정리** — ① git history 에 실 운영 대역이 남아 있음(history 재작성 또는
      새 레포 이관) ② `docs/feedback-archive/2026-08-04-ui-reference/` 스크린샷에 실 IP·부서
      PC명 포함(삭제 또는 마스킹). 둘 다 끝나기 전에는 public 전환 금지.

## P2 — 가까운 로드맵

- [ ] 스캔 UX 다듬기 — 진행바(N/254), 정렬, 단일 인스턴스(`Mutex`)

## P3 — 품질

- [ ] `Core.Tests` 보강 — `LocalNetwork` 대역 추출, NBSTAT 응답 파서 단위 테스트(고정 바이트 배열로)
- [ ] NetBIOS 실패 시 UX — 이름 미확인 IP 를 나중에 재조회하는 버튼

## P4 — 아이디어

- [ ] MAC 조회(`SendARP` P/Invoke, `IArpResolver`) + 제조사(OUI) — 원래 M2 였으나 후순위 이월
- [ ] 포트 스캔(TCP connect — 원본 `TIdConnectThread` 대응)
- [ ] 결과 CSV/JSON 내보내기 · 코드 서명(Authenticode) 인증서 적용
