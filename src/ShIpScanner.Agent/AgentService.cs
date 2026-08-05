using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ShIpScanner.Shared;

namespace ShIpScanner.Agent;

// 에이전트의 두뇌: 명령 서버(콘솔 수신) + 자동 종료 스케줄 감시.
// UI 스레드에서 생성되며, 경고/메시지 창을 직접 띄운다.
// [정책] 종료 '시각'은 관리자만 설정한다(SetSchedule 명령). 에이전트는 표시·연장(허용 시)만.
public sealed class AgentService
{
    private readonly AgentConfig _config;
    private ShutdownScheduler _scheduler; // 관리자 일정 변경 시 교체되므로 readonly 아님
    private readonly WindowsSystemController _controller;
    private readonly CommandServer _server;
    private readonly CancellationTokenSource _cts = new();
    private DispatcherTimer? _timer;

    private static readonly string AppVersion =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0";

    private bool _warningShown;
    private bool _shuttingDown;
    private int _extendsToday;
    private DateTime _dayAnchor = DateTime.Now.Date;

    public AgentService(AgentConfig config)
    {
        _config = config;
        _scheduler = new ShutdownScheduler(config.ShutdownTimeOnly, config.WarnLeadSeconds);

        _controller = new WindowsSystemController(
            status: BuildStatus,
            onExtend: minutes => Dispatcher.UIThread.Post(() => ApplyExtend(minutes)),
            onMessage: text => Dispatcher.UIThread.Post(() => ShowMessage(text)),
            onSetSchedule: (enabled, time, allowExtend) =>
                Dispatcher.UIThread.Post(() => ApplySchedule(enabled, time, allowExtend)));

        _server = new CommandServer(_controller, config.AuthToken, config.Port);
    }

    public AgentStatus Status => BuildStatus();

    // 콘솔에 보낼 전체 상태(관리자 일정 포함, 에이전트는 표시만).
    private AgentStatus BuildStatus() => new()
    {
        HostName = Environment.MachineName,
        UserName = Environment.UserName,
        Version = AppVersion,
        NextShutdown = _config.ScheduleEnabled ? _scheduler.NextScheduled(DateTime.Now) : null,
        ExtendedNow = _scheduler.DeferredUntil is not null,
        ShutdownTime = _config.ShutdownTime,
        ScheduleEnabled = _config.ScheduleEnabled,
        AllowExtend = _config.AllowExtend,
    };

    // 트레이 메뉴용: 지금 연장(관리자가 허용한 경우에만).
    public void ExtendNow() => ApplyExtend(_config.ExtendMinutes);

    // 트레이 메뉴용: 상태 요약(읽기 전용).
    public string StatusText()
    {
        var s = BuildStatus();
        var next = s.NextShutdown?.ToString("HH:mm") ?? "(자동종료 꺼짐)";
        var ext = _config.AllowExtend ? $"연장 가능 (오늘 {_extendsToday}/{_config.MaxExtends})" : "연장은 관리자에게 요청";
        return $"PC: {s.HostName}\n사용자: {s.UserName}\n버전: {s.Version}\n수신 포트: {_config.Port}\n" +
               $"종료 시각(관리자 설정): {_config.ShutdownTime}\n다음 종료: {next}\n{ext}";
    }

    public void Start()
    {
        // 명령 서버 시작(백그라운드).
        _ = Task.Run(() => _server.RunAsync(_cts.Token));

        // 스케줄 감시(15초 간격, UI 스레드). 항상 돌되 Tick 에서 사용여부를 확인.
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    public void Stop()
    {
        _cts.Cancel();
        _timer?.Stop();
    }

    private void Tick()
    {
        var now = DateTime.Now;

        if (now.Date != _dayAnchor)
        {
            _dayAnchor = now.Date;
            _warningShown = false;
            _extendsToday = 0;
            _scheduler.ResetIfNewDay(now);
        }

        if (!_config.ScheduleEnabled || _shuttingDown) return;

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
        // 관리자가 자체 연장을 막았거나 한도 초과면 무시.
        if (!_config.AllowExtend || _extendsToday >= _config.MaxExtends) return;
        _extendsToday++;
        _scheduler.Defer(DateTime.Now, minutes);
        _warningShown = false;
        WindowsSystemController.AbortPendingShutdown();
    }

    // 관리자(콘솔)가 종료 일정을 푸시. 에이전트는 받아서 적용·저장만 한다.
    private void ApplySchedule(bool enabled, string shutdownTime, bool allowExtend)
    {
        _config.ScheduleEnabled = enabled;
        if (TimeOnly.TryParse(shutdownTime, out _)) _config.ShutdownTime = shutdownTime;
        _config.AllowExtend = allowExtend;
        _config.Save();

        _scheduler = new ShutdownScheduler(_config.ShutdownTimeOnly, _config.WarnLeadSeconds);
        _warningShown = false;
        _shuttingDown = false;
    }

    private void ShowMessage(string text) => new MessageWindow(text).Show();
}
