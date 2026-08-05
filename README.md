# sh PC Manager

사내망 PC를 파악·관리하는 **관리자 콘솔 + 클라이언트 에이전트** 모음 (Windows, C#/.NET 8/Avalonia).
IP 스캐너로 시작해 원격 전원 관리까지 확장 중. (저장소명 `sh-pc-manager`, 내부 솔루션명 `ShIpScanner.*`.)

## 구성

- **관리자 콘솔 (sh IP Scanner)** — 대역을 병렬 핑 스윕해 살아있는 **IP·PC명**을 254칸 바둑판으로
  표시(흰=미확인·주황=사용중·연두=사용가능). 여러 대역 드롭다운 관리, 설정/정보 모달.
  PC명은 NetBIOS(한글) + 역DNS. 스캔 결과에 **에이전트 설치 배지**, PC **더블클릭 → 제어 창**.
- **클라이언트 에이전트 (sh Agent)** — 각 PC에 설치되어 **트레이 상주**. 업무시간 종료 시
  **자동 종료**(경고→저장유예→연장근무 연기). 콘솔의 인증된 명령(종료·재부팅·잠금·메시지·연장) 수신.
- **통신** — LAN 직접(TCP, 공유 토큰 인증). 원격 접속은 Windows RDP(`mstsc`).

## 개발

```bash
# .NET 8 SDK 필요
dotnet build ShIpScanner.sln                     # 전체 빌드
dotnet test  ShIpScanner.sln                     # 테스트
dotnet run --project src/ShIpScanner.App         # 콘솔 실행
dotnet run --project src/ShIpScanner.Agent       # 에이전트 실행(트레이)
```

- 설계·결정: `docs/adr/` (0002 스택·왜 C#, 0003 에이전트 아키텍처)
- 계획: `docs/plans/` (0001 스캐너 MVP, 0002 에이전트 Phase 1~4)
- 패키징: `docs/guides/packaging.md`, 에이전트 자동시작·방화벽: `docs/guides/agent-install-firewall.md`
- 작업 규칙: `CLAUDE.md` / `AGENTS.md`

## 보안 / 용도

본인이 소유·관리하는 사내망의 정당한 **자산·전원 관리** 도구다. 에이전트는 **인증된 명령만** 수락하고,
종료는 항상 **부드럽게**(경고·저장유예·연장) 처리한다.

---

© 2026 이성현 (SeongHyun Lee). All rights reserved. 라이센스: `LICENSE` (UNLICENSED · 개인 저작물).
