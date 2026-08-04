using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using ShIpScanner.App;
using ShIpScanner.App.ViewModels;
using ShIpScanner.App.Views;
using ShIpScanner.Core.Scanning;

AppBuilder.Configure<App>()
    .UseSkia()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .SetupWithoutStarting();

var vm = new MainViewModel();

// demo 인자면 스캔된 것처럼 셀 상태를 채워 색/한글명 렌더를 검증한다.
if (args.Length > 1 && args[1] == "demo")
{
    var rnd = new Random(7);
    string[] names = { "PC-01", "DEV-02", "OFFICE-03", "LAB-04", "FRONT-05", "ADMIN-06" };
    int ni = 0;
    foreach (var c in vm.Cells)
    {
        int r = rnd.Next(100);
        if (r < 35) { c.State = HostState.Alive; if (r < 10) c.HostName = names[ni++ % names.Length]; }
        else c.State = HostState.Free;
    }
    vm.Log.Add("사용하고 있는 IP 의 호스트 이름을 검색하겠습니다.");
    vm.Log.Add("IP:192.168.0.1 >>>> PC-01 이(가) 사용 중입니다.");
    vm.Log.Add("IP:192.168.0.2 >>>> DEV-02 이(가) 사용 중입니다.");
}

var win = new MainWindow { DataContext = vm };
win.Show();
for (int i = 0; i < 6; i++) Dispatcher.UIThread.RunJobs();

var frame = win.CaptureRenderedFrame();
var outPath = args.Length > 0 ? args[0] : "shot.png";
frame?.Save(outPath);
Console.WriteLine($"saved: {outPath} ({frame?.PixelSize})");
