using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace ShIpScanner.Agent;

public partial class App : Application
{
    private AgentService? _service;
    private TrayIcon? _tray;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 창 없이 트레이에만 상주 — 마지막 창이 닫혀도 앱이 종료되지 않게.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var config = AgentConfig.Load();
            _service = new AgentService(config);
            _service.Start();

            SetupTray(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupTray(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var icon = new WindowIcon(AssetLoader.Open(new Uri("avares://ShIpScanner.Agent/Assets/agenticon.ico")));

        var menu = new NativeMenu();

        var statusItem = new NativeMenuItem("상태 보기");
        statusItem.Click += (_, _) => new MessageWindow(_service?.StatusText() ?? "").Show();
        menu.Add(statusItem);

        var extendItem = new NativeMenuItem("지금 연장");
        extendItem.Click += (_, _) => _service?.ExtendNow();
        menu.Add(extendItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("에이전트 종료");
        exitItem.Click += (_, _) => { _service?.Stop(); desktop.Shutdown(); };
        menu.Add(exitItem);

        _tray = new TrayIcon
        {
            Icon = icon,
            ToolTipText = "sh Agent — PC 관리",
            Menu = menu,
            IsVisible = true,
        };

        TrayIcon.SetIcons(this, new TrayIcons { _tray });
    }
}
