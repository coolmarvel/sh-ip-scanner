using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ShIpScanner.Agent;

// 종료 경고창. 남은 시간을 카운트다운하고, 연장/즉시 종료를 제공한다.
public partial class WarningWindow : Window
{
    private DateTime _nextShutdown;
    private readonly Action _onExtend;
    private readonly Action _onShutdownNow;
    private DispatcherTimer? _timer;

    public WarningWindow(DateTime nextShutdown, bool canExtend, int extendMinutes,
                         Action onExtend, Action onShutdownNow)
    {
        InitializeComponent();
        _nextShutdown = nextShutdown;
        _onExtend = onExtend;
        _onShutdownNow = onShutdownNow;

        ExtendButton.IsEnabled = canExtend;
        ExtendButton.Content = canExtend ? $"연장 ({extendMinutes}분)" : "연장 불가";

        UpdateCountdown();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => UpdateCountdown();
        _timer.Start();
    }

    private void UpdateCountdown()
    {
        var remain = _nextShutdown - DateTime.Now;
        if (remain <= TimeSpan.Zero)
        {
            CountdownText.Text = "종료 중…";
            _timer?.Stop();
            return;
        }
        CountdownText.Text = $"남은 시간 {(int)remain.TotalMinutes:D2}:{remain.Seconds:D2}";
    }

    private void OnExtend(object? sender, RoutedEventArgs e)
    {
        _onExtend();
        _timer?.Stop();
        Close();
    }

    private void OnShutdownNow(object? sender, RoutedEventArgs e)
    {
        _timer?.Stop();
        _onShutdownNow();
        Close();
    }
}
