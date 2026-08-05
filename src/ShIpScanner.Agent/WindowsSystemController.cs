using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ShIpScanner.Shared;

namespace ShIpScanner.Agent;

// 실제 Windows 제어 구현. 종료/재부팅은 shutdown.exe, 잠금은 user32!LockWorkStation.
// [주의] Windows 전용 — 다른 OS 에선 호출되지 않는다(에이전트는 Windows 배포 대상).
public sealed class WindowsSystemController : ISystemController
{
    private readonly Func<DateTime?> _nextShutdown;   // 스케줄러에서 다음 종료시각 제공
    private readonly Func<bool> _extendedNow;
    private readonly Action<int> _onExtend;           // 콘솔의 연장 명령을 스케줄러에 반영
    private readonly Action<string> _onMessage;       // 안내 메시지를 UI 로 표시

    public WindowsSystemController(Func<DateTime?> nextShutdown, Func<bool> extendedNow,
                                   Action<int> onExtend, Action<string> onMessage)
    {
        _nextShutdown = nextShutdown;
        _extendedNow = extendedNow;
        _onExtend = onExtend;
        _onMessage = onMessage;
    }

    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    public AgentStatus GetStatus() => new()
    {
        HostName = Environment.MachineName,
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0",
        UserName = Environment.UserName,
        NextShutdown = _nextShutdown(),
        ExtendedNow = _extendedNow(),
    };

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
