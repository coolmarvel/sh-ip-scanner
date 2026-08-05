using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ShIpScanner.Shared;

namespace ShIpScanner.Agent;

// 에이전트의 두뇌: 명령 서버(콘솔 수신) + 자동 종료 스케줄 감시.
// UI 스레드에서 생성되며, 경고/메시지 창을 직접 띄운다.
public sealed class AgentService
{
    private readonly AgentConfig _config;
    private readonly ShutdownScheduler _scheduler;
    private readonly WindowsSystemController _controller;
    private readonly CommandServer _server;
    private readonly CancellationTokenSource _cts = new();
    private DispatcherTimer? _timer;

    private bool _warningShown;
    private bool _shuttingDown;
    private int _extendsToday;
    private DateTime _dayAnchor = DateTime.Now.Date;

    public AgentService(AgentConfig config)
    {
        _config = config;
        _scheduler = new ShutdownScheduler(config.ShutdownTimeOnly, config.WarnLeadSeconds);

        _controller = new WindowsSystemController(
            nextShutdown: () => _config.ScheduleEnabled ? _scheduler.NextScheduled(DateTime.Now) : null,
            extendedNow: () => _scheduler.DeferredUntil is not null,
            onExtend: minutes => Dispatcher.UIThread.Post(() => ApplyExtend(minutes)),
            onMessage: text => Dispatcher.UIThread.Post(() => ShowMessage(text)));

        _server = new CommandServer(_controller, config.AuthToken, config.Port);
    }

    public AgentStatus Status => _controller.GetStatus();

    // 트레이 메뉴용: 지금 연장.
    public void ExtendNow() => ApplyExtend(_config.ExtendMinutes);

    // 트레이 메뉴용: 상태 요약 문자열.
    public string StatusText()
    {
        var s = Status;
        var next = s.NextShutdown?.ToString("HH:mm") ?? "(자동종료 꺼짐)";
        return $"PC: {s.HostName}\n사용자: {s.UserName}\n버전: {s.Version}\n" +
               $"수신 포트: {_config.Port}\n다음 종료: {next}\n오늘 연장: {_extendsToday}/{_config.MaxExtends}";
    }

    public void Start()
    {
        // 명령 서버 시작(백그라운드).
        _ = Task.Run(() => _server.RunAsync(_cts.Token));

        // 스케줄 감시(15초 간격, UI 스레드).
        if (_config.ScheduleEnabled)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            _timer.Tick += (_, _) => Tick();
            _timer.Start();
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _timer?.Stop();
    }

    private void Tick()
    {
        var now = DateTime.Now;

        // 날짜가 바뀌면 하루치 상태 초기화.
        if (now.Date != _dayAnchor)
        {
            _dayAnchor = now.Date;
            _warningShown = false;
            _extendsToday = 0;
            _scheduler.ResetIfNewDay(now);
        }

        if (_shuttingDown) return;

        if (_scheduler.ShouldShutdownNow(now))
        {
            _shuttingDown = true;
            _ = _controller.ShutdownAsync(60, "업무 시간 종료 — 자동 종료");
            return;
        }

        if (_scheduler.ShouldWarn(now) && !_warningShown)
        {
            _warningShown = true;
            ShowWarning();
        }
    }

    private void ShowWarning()
    {
        bool canExtend = _config.AllowExtend && _extendsToday < _config.MaxExtends;
        var win = new WarningWindow(
            nextShutdown: _scheduler.NextScheduled(DateTime.Now),
            canExtend: canExtend,
            extendMinutes: _config.ExtendMinutes,
            onExtend: () => ApplyExtend(_config.ExtendMinutes),
            onShutdownNow: () =>
            {
                _shuttingDown = true;
                _ = _controller.ShutdownAsync(5, "사용자 요청 종료");
            });
        win.Show();
    }

    private void ApplyExtend(int minutes)
    {
        if (_extendsToday >= _config.MaxExtends) return;
        _extendsToday++;
        _scheduler.Defer(DateTime.Now, minutes);
        _warningShown = false;
        WindowsSystemController.AbortPendingShutdown(); // 예약된 종료 취소
    }

    private void ShowMessage(string text)
    {
        var win = new MessageWindow(text);
        win.Show();
    }
}
