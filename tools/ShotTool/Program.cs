using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using ShIpScanner.App;
using ShIpScanner.App.ViewModels;
using ShIpScanner.App.Views;
using ShIpScanner.Core.Config;
using ShIpScanner.Core.Scanning;

// ShotTool — Avalonia 를 헤드리스(창 없는 렌더러)로 띄워 화면을 PNG 로 굽는 개발용 도구.
// GUI 세션이 없는 WSL 에서도 UI 를 눈으로 확인할 수 있고, 포트폴리오용 스크린샷도 여기서 만든다.
//
// 사용법:  dotnet run --project tools/ShotTool -- <출력파일> <장면> [폭] [높이]
//   장면: main | scanning | result | manager | settings | firstrun | about
//
// ⚠ 데모 데이터 규칙 — 이 도구가 만드는 화면에는 **실제 운영 대역·실제 PC명을 절대 넣지 않는다.**
//    공개 저장소와 포트폴리오에 그대로 실리므로 항상 예시 대역(192.168.x)과 일반 PC명만 쓴다.
//    (실 대역을 지운 경위는 docs/session-log.md 2026-08-05 블록 참고.)

AppBuilder.Configure<App>()
    .UseSkia()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .SetupWithoutStarting();

var outPath = args.Length > 0 ? args[0] : "shot.png";
var which = args.Length > 1 ? args[1] : "main";
int? width = args.Length > 2 && int.TryParse(args[2], out var w) ? w : null;
int? height = args.Length > 3 && int.TryParse(args[3], out var h) ? h : null;

var vm = new MainViewModel();

// 첫 실행 안내 장면만 "저장된 대역이 없는" 상태가 필요하다. 그 외 장면은 데모 대역을 심는다.
if (which != "firstrun") SeedDemoSubnets(vm);
else vm.LocalIpText = "192.168.0.17";

switch (which)
{
    case "scanning":
        // 스캔이 도는 중간 순간 — 앞쪽 옥텟은 판정이 끝났고 뒤쪽은 아직 흰색(미확인).
        FillBoard(vm, upTo: 138);
        vm.IsScanning = true;
        vm.ScannedCount = 138;
        vm.Log.Add("[192.168.0.x] 대역을 검색합니다.");
        vm.Log.Add("사용 중인 IP 의 호스트 이름(PC명)을 함께 조회합니다.");
        AddDiscoveryLog(vm, take: 3);
        break;

    case "result":
        // 스캔이 끝난 상태 — 254칸이 전부 판정되어 있고 로그에 완료 줄이 찍혔다.
        FillBoard(vm, upTo: 254);
        vm.IsScanning = false;
        vm.ScannedCount = 254;
        AddDiscoveryLog(vm, take: 3);
        vm.Log.Add($"검색 완료 — 사용 중 {vm.AliveCount}대 / 검사 254개.");
        break;
}

Window win = which switch
{
    "manager" => new SubnetManagerWindow { DataContext = vm },
    "firstrun" => new FirstRunWindow { DataContext = vm },
    "settings" => new SettingsWindow { DataContext = vm },
    "about" => new AboutWindow(),
    _ => new MainWindow { DataContext = vm },
};

// 바둑판 254칸을 잘리지 않게 담으려면 기본 창 높이(760)로는 모자라다 — 캡처용으로만 키운다.
if (width is int ww) win.Width = ww;
if (height is int hh) win.Height = hh;

win.Show();
for (int i = 0; i < 8; i++) Dispatcher.UIThread.RunJobs();

var frame = win.CaptureRenderedFrame();
frame?.Save(outPath);
Console.WriteLine($"saved: {outPath} ({frame?.PixelSize})");

// 데모용 대역 3개(예시). 실제 환경도 여러 대역을 오가며 쓰지만, 값은 예시로만 노출한다.
static void SeedDemoSubnets(MainViewModel vm)
{
    vm.Subnets.Clear();
    vm.Subnets.Add(new SubnetDefinition { Base = "192.168.0", Label = "네트워크 A" });
    vm.Subnets.Add(new SubnetDefinition { Base = "192.168.10", Label = "네트워크 B" });
    vm.Subnets.Add(new SubnetDefinition { Base = "192.168.20", Label = "네트워크 C" });
    vm.SelectedSubnet = vm.Subnets[0];
    vm.LocalIpText = "192.168.0.17";
}

// 로그 줄은 바둑판에서 실제로 살아있는 셀에서 뽑는다 — 화면과 로그가 어긋나지 않게.
static void AddDiscoveryLog(MainViewModel vm, int take)
{
    var alive = vm.Cells.Where(c => c.State == HostState.Alive).ToList();
    if (alive.Count == 0) return;

    // 앞·중간·뒤에서 골고루 뽑아 "스캔이 대역 전체를 훑는" 느낌을 준다.
    for (int i = 0; i < take; i++)
    {
        var c = alive[(alive.Count - 1) * i / Math.Max(1, take - 1)];
        var name = string.IsNullOrEmpty(c.HostName) ? "(이름 미확인)" : c.HostName;
        vm.Log.Add($"IP:{c.Ip} >>>> {name} 이(가) 사용 중입니다.");
    }
}

// 바둑판을 결정적(seed 고정)으로 채운다 — 같은 명령이면 항상 같은 그림이 나오도록.
static void FillBoard(MainViewModel vm, int upTo)
{
    var rnd = new Random(7);
    // 한글 PC명은 NetBIOS(CP949) 디코딩이 되는지 보여주는 대목이라 일부러 한글을 섞는다.
    string[] names =
    {
        "공유기", "사무실-PC01", "사무실-PC02", "회의실-PC", "프린터-01", "NAS-01",
        "창고-PC", "안내데스크", "개발-PC01", "개발-PC02", "노트북-01", "복합기-02",
        "스캐너-PC", "CCTV-NVR", "서버-01", "테스트-PC", "교육장-PC", "AP-2층",
        "출입통제기", "키오스크-01", "백업서버", "라벨프린터",
    };
    int ni = 0;
    int alive = 0;

    foreach (var c in vm.Cells)
    {
        if (c.Octet > upTo) break;
        int r = rnd.Next(100);
        if (r < 28)
        {
            c.State = HostState.Alive;
            alive++;
            // 살아있는 호스트 중 일부만 이름이 붙는다 — 실제로도 NetBIOS 응답이 없는 장비가 있다.
            if (r < 12 && ni < names.Length) c.HostName = names[ni++];
        }
        else
        {
            c.State = HostState.Free;
        }
    }
    vm.AliveCount = alive;
}
