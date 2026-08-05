using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using ShIpScanner.App;
using ShIpScanner.App.ViewModels;
using ShIpScanner.App.Views;
using ShIpScanner.Core.Scanning;
using ShIpScanner.Agent;

AppBuilder.Configure<ShIpScanner.App.App>()
    .UseSkia()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .SetupWithoutStarting();

var outPath = args.Length > 0 ? args[0] : "shot.png";
var which = args.Length > 1 ? args[1] : "main";

var vm = new MainViewModel();
if (which == "demo")
{
    var rnd = new Random(7);
    string[] names = { "PC-01", "DEV-02", "OFFICE-03", "LAB-04", "FRONT-05", "ADMIN-06" };
    int ni = 0;
    foreach (var c in vm.Cells)
    {
        int r = rnd.Next(100);
        if (r < 35) { c.State = HostState.Alive; if (r < 10) c.HostName = names[ni++ % names.Length]; if (r < 18) { c.AgentInstalled = true; c.AgentVersion = "0.1.0"; } }
        else c.State = HostState.Free;
    }
    vm.Log.Add("[192.168.80.x] 대역을 검색합니다.");
    vm.Log.Add("IP:192.168.80.1 >>>> PC-01 이(가) 사용 중입니다.");
}

Window win = which switch
{
    "warning" => new WarningWindow(DateTime.Now.AddMinutes(5), true, 60, () => {}, () => {}),
    "message" => new MessageWindow("전산팀 공지: 오늘 19시에 보안 패치가 적용됩니다. 업무 저장 후 대기해 주세요."),
    "manager" => new SubnetManagerWindow { DataContext = vm },
    "settings" => new SettingsWindow { DataContext = vm },
    "about" => new AboutWindow(),
    "control" => new ShIpScanner.App.Views.PcControlWindow("192.168.80.24", "관리부-PC01", true, "0.1.0", "change-me", 47101),
    _ => new MainWindow { DataContext = vm },
};
win.Show();
for (int i = 0; i < 6; i++) Dispatcher.UIThread.RunJobs();

var frame = win.CaptureRenderedFrame();
frame?.Save(outPath);
Console.WriteLine($"saved: {outPath} ({frame?.PixelSize})");
