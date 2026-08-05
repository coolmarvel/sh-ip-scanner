# sh IP Scanner

로컬 네트워크(LAN)의 서브넷을 훑어 **살아있는 호스트의 IP·PC명을 바둑판으로 보여주는 Windows
데스크톱 스캐너**. 기존 포터블 툴 `faIpScanner` 를 벤치마킹해 **C# / .NET 8 / Avalonia(MVVM)** 로
재현하며, 개발자 본인이 C# 을 학습하는 것이 함께하는 목적이다.

## 무엇을 하나

- 로컬 IP 를 자동 감지해 그 `/24` 대역(예: `192.168.80.1~254`)을 **병렬 핑 스윕**
- 결과를 **254칸 바둑판**으로 표시 — 흰=미확인 · 주황=사용 중 · 연두=사용 가능
- 사용 중인 IP 의 **PC명(호스트명)** 을 NetBIOS(한글 지원) + 역DNS 로 조회
- 용도: 개인이 관리하는 네트워크의 **전산 자산 파악**

## 개발

```bash
# .NET 8 SDK 필요
dotnet build ShIpScanner.sln            # 빌드
dotnet test  ShIpScanner.sln            # 테스트
dotnet run --project src/ShIpScanner.App  # 실행 (Windows/데스크톱 세션)
```

빌드·검증·패키징 상세는 `docs/guides/packaging.md`, 설계 배경은 `docs/brief.md` 와
`docs/adr/0002-stack.md` 참고. 에이전트 작업 규칙은 `CLAUDE.md` / `AGENTS.md`.

## 설치본

`docs/guides/packaging.md` 절차로 자체포함 게시 후 Inno Setup 으로 `Setup.exe` 를 굽는다.

---

© 2026 이성현 (SeongHyun Lee). All rights reserved. 라이센스: `LICENSE` (UNLICENSED · 개인 저작물).
