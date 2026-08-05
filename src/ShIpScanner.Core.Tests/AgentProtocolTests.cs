using System;
using System.Threading;
using System.Threading.Tasks;
using ShIpScanner.Shared;
using Xunit;

namespace ShIpScanner.Core.Tests;

public class ShutdownSchedulerTests
{
    private static DateTime At(int h, int m) => new(2026, 8, 5, h, m, 0);

    [Fact]
    public void Warns_Within_Lead_Window()
    {
        var s = new ShutdownScheduler(new TimeOnly(19, 0), warnLeadSeconds: 600); // 10분 전 경고
        Assert.False(s.ShouldWarn(At(18, 49))); // 11분 전 → 아직
        Assert.True(s.ShouldWarn(At(18, 55)));   // 5분 전 → 경고
    }

    [Fact]
    public void Shuts_Down_At_Or_After_Target()
    {
        var s = new ShutdownScheduler(new TimeOnly(19, 0));
        Assert.False(s.ShouldShutdownNow(At(18, 59)));
        Assert.True(s.ShouldShutdownNow(At(19, 0)));
        Assert.True(s.ShouldShutdownNow(At(21, 0)));
    }

    [Fact]
    public void Defer_Postpones_This_Cycle()
    {
        var s = new ShutdownScheduler(new TimeOnly(19, 0));
        s.Defer(At(18, 55), 60);                 // 19:00 → 20:00 로 연기
        Assert.False(s.ShouldShutdownNow(At(19, 5))); // 아직 안 끔
        Assert.True(s.ShouldShutdownNow(At(20, 0)));   // 연장 시각 도달
        Assert.Equal(At(20, 0), s.NextScheduled(At(19, 30)));
    }
}

public class AgentServerClientTests
{
    private const string Token = "secret-123";

    private static async Task<(CommandServer server, int port, CancellationTokenSource cts, Task run)> StartAsync(ISystemController ctrl)
    {
        var server = new CommandServer(ctrl, Token, port: 0); // 0 = OS 가 빈 포트 선택
        server.Start();
        var cts = new CancellationTokenSource();
        var run = server.RunAsync(cts.Token);
        await Task.Delay(50); // 리스너 준비 대기
        return (server, server.Port, cts, run);
    }

    [Fact]
    public async Task Status_RoundTrip_Works()
    {
        var ctrl = new TestSystemController { Status = new AgentStatus { HostName = "PC-1", Version = "1.0", UserName = "u" } };
        var (_, port, cts, _) = await StartAsync(ctrl);

        var status = await AgentClient.GetStatusAsync("127.0.0.1", Token, port, timeoutMs: 1500);

        Assert.NotNull(status);
        Assert.Equal("PC-1", status!.HostName);
        cts.Cancel();
    }

    [Fact]
    public async Task Shutdown_Command_Invokes_Controller()
    {
        var ctrl = new TestSystemController();
        var (_, port, cts, _) = await StartAsync(ctrl);

        var resp = await AgentClient.SendAsync("127.0.0.1",
            new CommandRequest { Type = CommandType.Shutdown, AuthToken = Token, DelaySeconds = 30 }, port, 1500);

        Assert.NotNull(resp);
        Assert.True(resp!.Ok);
        Assert.Equal(1, ctrl.ShutdownCalls);
        cts.Cancel();
    }

    [Fact]
    public async Task Wrong_Token_Is_Rejected_And_Does_Nothing()
    {
        var ctrl = new TestSystemController();
        var (_, port, cts, _) = await StartAsync(ctrl);

        var resp = await AgentClient.SendAsync("127.0.0.1",
            new CommandRequest { Type = CommandType.Shutdown, AuthToken = "WRONG", DelaySeconds = 30 }, port, 1500);

        Assert.NotNull(resp);
        Assert.False(resp!.Ok);
        Assert.Equal(0, ctrl.ShutdownCalls); // 인증 실패 → 아무 동작 안 함
        cts.Cancel();
    }
}
