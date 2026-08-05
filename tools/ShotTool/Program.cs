using Avalonia;
using Avalonia.Controls;
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
        if (r < 35) { c.State = HostState.Alive; if (r < 10) c.HostName = names[ni++ % names.Length]; }
        else c.State = HostState.Free;
    }
    vm.Log.Add("[192.168.80.x] 대역을 검색합니다.");
    vm.Log.Add("IP:192.168.80.1 >>>> PC-01 이(가) 사용 중입니다.");
}

Window win = which switch
{
    "manager" => new SubnetManagerWindow { DataContext = vm },
    "settings" => new SettingsWindow { DataContext = vm },
    "about" => new AboutWindow(),
    _ => new MainWindow { DataContext = vm },
};
win.Show();
for (int i = 0; i < 6; i++) Dispatcher.UIThread.RunJobs();

var frame = win.CaptureRenderedFrame();
frame?.Save(outPath);
Console.WriteLine($"saved: {outPath} ({frame?.PixelSize})");
