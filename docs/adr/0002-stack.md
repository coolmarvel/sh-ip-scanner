---
title: ADR-0002 스택 결정 — C# / .NET 8 / Avalonia / Inno Setup
created: 2026-08-04
status: accepted
---

# ADR-0002: 스택 결정 — C# / .NET 8 / Avalonia (MVVM) / Inno Setup

## 상태

Accepted

## 맥락

`sh IP Scanner` 는 두 가지 목적을 동시에 가진다 (→ `docs/brief.md`):

1. **결과물** — 기존에 쓰던 포터블 툴 `faIpScanner.exe` 처럼 LAN 대역을 스윕해 살아있는
   호스트의 IP·MAC·호스트명을 보여주는 Windows 데스크톱 스캐너.
2. **학습** — 개발자 본인이 지금까지 써온 스택(**TypeScript + React + Electron**)에서 벗어나
   **새 언어를 하나 제대로 배우는 것**. 그 언어로 **C#** 을 택했다.

그래서 이 결정서는 단순한 "무슨 기술을 쓸까"가 아니라 **"왜 C#을 배우기로 했는가"** 를 함께 남긴다.

### 벤치마킹 대상 분석 (faIpScanner.exe 정적 분석 결과)

우리가 재현할 원본을 먼저 이해했다. 바이너리 정적 분석으로 확인한 사실:

| 항목 | 내용 |
|---|---|
| 형태 | 네이티브 **Win32 GUI 앱**, x86(32-bit), PE32, 설치 없는 단일 exe(약 592KB) |
| 제작 언어 | **Delphi(오브젝트 파스칼)** — `TThread`·`TCustomActionList`·`TFindHostNameThread`·`TCheckIPThread`·`TIdConnectThread`(Indy 라이브러리) 같은 VCL/Indy `T`-클래스가 결정적 증거 |
| 살아있음 판별 | `WSAStartup`(Winsock) + `TCheckIPThread` 로 대역을 **멀티스레드 스윕**, `TIdConnectThread`(Indy)로 TCP 연결 프로빙 |
| MAC 조회 | **`SendARP`**(iphlpapi.dll) — LAN 세그먼트의 살아있는 호스트 MAC 해석. `cmd /c arp -d` 로 ARP 캐시 초기화 |
| 호스트명 | `TFindHostNameThread` + `gethostname` 역조회 |
| 기타 | `CreateFileMappingA/OpenFileMappingA` 공유 메모리 → 단일 인스턴스 실행 방지(뮤텍스 대용) |

**핵심 통찰**: 원본이 하던 일은 (1) 병렬 핑/연결 스윕, (2) `SendARP` 로 MAC, (3) 역DNS 호스트명이다.
이 세 가지는 **C#/.NET 표준 라이브러리 + Win32 P/Invoke 로 거의 1:1 재현**할 수 있다.
즉 원본을 배우기 교재로 삼기에 C#은 궁합이 아주 좋다.

## 결정

아래 스택으로 확정한다. (언어를 제외한 GUI/설치 항목은 사용자가 선택함.)

| 축 | 선택 | 한 줄 이유 |
|---|---|---|
| **언어** | **C# 12** | 배우고 싶은 새 언어 + 네트워크/시스템 프로그래밍 표준 라이브러리가 강함 |
| **런타임** | **.NET 8 (LTS)** | 장기 지원, 크로스플랫폼, 최신 툴링. (개발 환경에 SDK 8.0.423 설치 확인) |
| **GUI** | **Avalonia UI 11 + MVVM** | XAML+MVVM 정석 아키텍처 학습, WPF와 호환되는 문법, 크로스플랫폼 여지 |
| **MVVM 라이브러리** | **CommunityToolkit.Mvvm** | `ObservableObject`·`RelayCommand` 로 보일러플레이트 최소화, MS 공식 |
| **핑** | `System.Net.NetworkInformation.Ping` | 관리자 권한 없이 ICMP 핑, 비동기 지원 |
| **MAC** | Win32 **`SendARP`** via **P/Invoke** | 원본과 동일 기법. `IArpResolver` 인터페이스로 추상화(플랫폼 종속 격리) |
| **호스트명** | `Dns.GetHostEntryAsync` | 표준 역DNS |
| **동시성** | `async`/`await` + `SemaphoreSlim` + `CancellationToken` | 원본의 스레드풀을 .NET 비동기로 재현 (학습 핵심) |
| **설치** | **Inno Setup** | 무료·간단, 결과가 진짜 `Setup.exe`, 코드서명 없이 동작 |

### 프로젝트 구조 (계획)

```
sh-ip-scanner/
├── ShIpScanner.sln
├── src/
│   ├── ShIpScanner.Core/           # UI 없는 순수 스캔 로직 (테스트·재사용 쉽게 분리)
│   │   ├── Scanning/               #   IpRange, HostResult, ScanEngine (async 병렬 스윕)
│   │   ├── Arp/                    #   IArpResolver, WindowsArpResolver(P/Invoke), NullArpResolver
│   │   └── Naming/                 #   호스트명 역조회
│   ├── ShIpScanner.App/            # Avalonia GUI (MVVM: Views + ViewModels)
│   │   ├── ViewModels/             #   MainWindowViewModel (ObservableCollection<HostRow>)
│   │   ├── Views/                  #   MainWindow.axaml
│   │   └── Program.cs / App.axaml
│   └── ShIpScanner.Core.Tests/     # xUnit — IpRange 파싱 등 순수 로직 단위 테스트
└── installer/                      # Inno Setup 스크립트(.iss) — v1 후반
```

> **왜 Core / App 분리?** 스캔 로직을 UI에서 떼어놓으면 (1) 단위 테스트가 쉽고, (2) 나중에 CLI나
> 다른 UI를 붙이기 쉽다. "관심사 분리"라는 개념을 프로젝트 구조로 배우는 첫 실습이다.

## 근거

### 왜 C# 인가 (이 프로젝트의 핵심 질문)

이미 TS+React+Electron 으로 GUI 실행기를 만들 수 있는데도 굳이 C#을 택한 이유:

1. **"진짜 정적 타입 언어 + 시스템 프로그래밍"을 배우려는 목적.**
   TypeScript는 JS 위의 타입 레이어라 런타임엔 타입이 없다. C#은 **컴파일·런타임 모두 강타입**이고,
   `struct`(값 타입) vs `class`(참조 타입), 제네릭, `Span<T>`, P/Invoke 같은 **JS/TS엔 없는 개념**을
   정면으로 배우게 된다. 새 언어를 배우는 목적에 가장 큰 학습 이득.

2. **문법이 TS와 가까워 진입 장벽은 낮고, 개념 차이는 크다 — 학습 효율이 좋다.**
   중괄호 문법, `async`/`await`, 람다, LINQ(≈ 배열 메서드 체이닝)는 TS 경험이 그대로 전이된다.
   그 위에서 클래스·인터페이스·값 타입·네이티브 상호운용 같은 **새 뼈대**만 얹으면 되니,
   완전 낯선 언어(Rust 등)보다 "배우면서 결과물도 낸다"가 현실적이다.

3. **네트워크/시스템 표준 라이브러리가 이 프로젝트에 딱 맞는다.**
   `System.Net.NetworkInformation.Ping`(ICMP 핑), `System.Net.Sockets`, `Dns` 역조회가 기본 제공되고,
   **`SendARP` 같은 Win32 API를 P/Invoke로 직접 호출**할 수 있다. 원본 faIpScanner가 하던 일을
   거의 그대로 옮길 수 있다 — 벤치마킹→재현→이해의 학습 루프에 최적.

4. **결과물의 "결"이 Electron보다 맞다 — 가벼운 네이티브 툴.**
   원본은 592KB 단일 exe로 순식간에 뜨는 가벼운 유틸이었다. Electron은 Chromium+Node를 통째로
   번들해 수십~수백 MB에 시작도 느리다. .NET 데스크톱 앱은 훨씬 가볍고 빠르며, 필요하면
   자체포함/트리밍/AOT로 더 줄일 수 있다. "가벼운 스캐너"라는 목표에 C#/.NET이 더 어울린다.

5. **데스크톱 GUI가 1급 시민 — 학습 ROI가 높다.**
   대안 언어들과 비교하면:

   | 후보 | 데스크톱 GUI | 시스템/네트워크 | 학습 곡선 | 판정 |
   |---|---|---|---|---|
   | **C# / .NET** | WPF·WinForms·**Avalonia**·MAUI 등 성숙 | 표준 라이브러리 + P/Invoke 강함 | TS 경험 전이로 완만 | ✅ 채택 |
   | Rust | egui·Slint 등 아직 미성숙 | 매우 강함 | 소유권 등으로 가파름 | 학습+결과 동시엔 부담 |
   | Go | Fyne 등 제한적, 약함 | 강함(단 ICMP는 권한 이슈) | 완만 | GUI가 약해 이 프로젝트엔 부적합 |
   | C++/Delphi | 강함(원본이 이 길) | 강함 | 메모리 관리 부담 | 배움의 현대성·안전성에서 밀림 |
   | TS/Electron | 강함(웹기술) | Node로 우회 필요, 무거움 | 이미 아는 것 | **새로 배우는 목적에 부적합** |

   C#은 GUI·네트워크·학습곡선·툴링(Visual Studio / JetBrains Rider / `dotnet` CLI)·전이성
   (기업 백엔드·데스크톱·Unity 게임)에서 고르게 높다. 배워두면 써먹을 곳이 넓다.

### 왜 Avalonia 인가 (WPF / WinForms / MAUI 대신)

사용자가 선택했고, 그 선택이 학습 목적에 부합하는 이유:

- **Avalonia**: XAML + MVVM 이라는 **정석 .NET 데스크톱 아키텍처**를 배운다. 문법이 WPF와 거의 동일해
  WPF 지식으로도 전이되고, 크로스플랫폼(Win/mac/Linux)이라 나중 여지가 열려 있다.
  단점: 생태계가 WPF보다 작고, 일부 네이티브 통합은 직접 해야 한다(이 프로젝트의 ARP가 대표 예).
- **WinForms**: 가장 쉽고 드래그앤드롭 디자이너가 있어 원본 VCL과 감각이 비슷하지만, **MVVM/데이터
  바인딩 학습엔 부적합**한 옛 방식이다. "정석 아키텍처를 배운다"는 목적과 어긋난다.
- **WPF**: 훌륭하지만 Windows 전용이고 최신 .NET에서 사실상 유지보수 모드다. Avalonia가 상위호환 학습재.
- **MAUI**: 모바일 중심이라 데스크톱 LAN 스캐너엔 과하다.

> **트레이드오프 하나를 명시**: Avalonia는 크로스플랫폼이지만 우리의 MAC 조회(`SendARP`)는 **Windows 전용**이다.
> 그래서 `IArpResolver` 인터페이스로 감싸고 `WindowsArpResolver`(P/Invoke)만 우선 구현한다.
> 비-Windows에선 빈 결과(`NullArpResolver`)로 우아하게 낮춘다. "플랫폼 종속을 인터페이스로 격리"하는
> 것 자체가 좋은 학습 소재다.

### 왜 Inno Setup 인가 (MSIX / WiX / 단일 exe 대신)

사용자가 "설치하는 앱"을 원했고(원본은 포터블이었지만 이번엔 설치형 학습), Inno Setup을 택한 이유:

- **Inno Setup**: 무료, 스크립트(.iss)가 단순, 결과가 누구나 아는 **`Setup.exe` 마법사**라 배포·학습이 직관적이다.
  **코드 서명 없이도** 동작한다(개인 학습 툴엔 충분). 국내외 사용 사례가 많아 자료가 풍부하다.
- **MSIX**: 모던 표준이고 클린 설치/제거·자동 업데이트에 유리하지만 **코드 서명 인증서가 사실상 필수**라
  초기 셋업이 번거롭다.
- **WiX / MSI**: 가장 정통 엔터프라이즈 방식이지만 XML 러닝커브가 가파르다.
- **자체포함 단일 exe**: 배포는 가장 간단하나 "설치하는 앱"이라는 이번 목표와 어긋난다(원본이 이미 포터블).

## 결과

**긍정**

- 학습 목적 최적화: 정적 타입·비동기·P/Invoke·MVVM 을 하나의 실전 프로젝트에서 모두 만난다.
- 결과물이 가볍고 빠르다(네이티브 데스크톱). 원본의 "가벼운 유틸" 결을 잇는다.
- 스캔 로직(Core)과 UI(App) 분리로 테스트·확장이 쉽다.
- 배운 스택의 전이성이 넓다(백엔드·데스크톱·게임).

**부정 / 리스크**

- **플랫폼 종속**: `SendARP`/ARP 경로는 Windows 전용. 인터페이스로 격리하지만, 크로스플랫폼 MAC 조회는
  별도 과제로 남는다(범위 밖).
- **Avalonia 생태계**가 WPF보다 작아, 막히면 자료가 상대적으로 적을 수 있다.
- **.NET SDK 의존**: 개발엔 SDK가 필요하다(설치 완료). 배포는 자체포함 옵션으로 런타임 미설치 PC도 커버 가능.
- **권한**: ICMP 핑(`Ping` 클래스)은 관리자 권한이 필요 없다. 다만 방화벽/네트워크 정책에 따라 일부 호스트가
  핑에 무응답일 수 있어, 판별 신뢰도는 원본과 마찬가지로 100%는 아니다(설계상 한계로 수용).

## 관련 문서

- `docs/brief.md` — 왜/무엇 SSOT
- `docs/plans/0001-mvp.md` — 이 결정을 따라 만들 마일스톤
- `docs/adr/0001-harness-engineering.md` — 하네스 도입 근거
