using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ShIpScanner.Shared;

namespace ShIpScanner.Agent;

// 실제 Windows 제어 구현. 종료/재부팅은 shutdown.exe, 잠금은 user32!LockWorkStation.
// 상태 구성·연장·메시지·일정설정은 AgentService 가 넘겨준 콜백으로 위임한다.
// [주의] Windows 전용 — 다른 OS 에선 호출되지 않는다(에이전트는 Windows 배포 대상).
public sealed class WindowsSystemController : ISystemController
{
    private readonly Func<AgentStatus> _status;
    private readonly Action<int> _onExtend;
    private readonly Action<string> _onMessage;
    private readonly Action<bool, string, bool> _onSetSchedule;

    public WindowsSystemController(Func<AgentStatus> status, Action<int> onExtend,
                                   Action<string> onMessage, Action<bool, string, bool> onSetSchedule)
    {
        _status = status;
        _onExtend = onExtend;
        _onMessage = onMessage;
        _onSetSchedule = onSetSchedule;
    }

    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    public AgentStatus GetStatus() => _status();

    public Task ShutdownAsync(int delaySeconds, string reason)
    {
        RunShutdown($"/s /t {Math.Max(0, delaySeconds)} /c \"{Sanitize(reason)}\"");
        return Task.CompletedTask;
    }

    public Task RebootAsync(int delaySeconds, string reason)
    {
        RunShutdown($"/r /t {Math.Max(0, delaySeconds)} /c \"{Sanitize(reason)}\"");
        return Task.CompletedTask;
    }

    public Task LockAsync()
    {
        try { LockWorkStation(); } catch { }
        return Task.CompletedTask;
    }

    public Task ShowMessageAsync(string text)
    {
        _onMessage(text);
        return Task.CompletedTask;
    }

    public Task ExtendAsync(int minutes)
    {
        _onExtend(minutes);
        return Task.CompletedTask;
    }

    public Task SetScheduleAsync(bool enabled, string shutdownTime, bool allowExtend)
    {
        _onSetSchedule(enabled, shutdownTime, allowExtend);
        return Task.CompletedTask;
    }

    // 예약된 종료가 있으면 취소(연장/저장을 위해).
    public static void AbortPendingShutdown()
    {
        try { RunShutdown("/a"); } catch { }
    }

    private static void RunShutdown(string args)
    {
        var psi = new ProcessStartInfo("shutdown", args) { CreateNoWindow = true, UseShellExecute = false };
        Process.Start(psi);
    }

    private static string Sanitize(string s) => s.Replace("\"", "'").Replace("\r", " ").Replace("\n", " ");
}
