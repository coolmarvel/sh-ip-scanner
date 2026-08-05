namespace ShIpScanner.Shared;

// 에이전트가 PC 에 실제로 수행하는 동작의 추상화(플랫폼 종속 격리 + 테스트 가능).
// Windows 구현은 에이전트 프로젝트의 WindowsSystemController(P/Invoke·shutdown.exe),
// 테스트는 TestSystemController(실제 동작 없이 호출 기록).
public interface ISystemController
{
    AgentStatus GetStatus();
    Task ShutdownAsync(int delaySeconds, string reason);
    Task RebootAsync(int delaySeconds, string reason);
    Task LockAsync();
    Task ShowMessageAsync(string text);
    Task ExtendAsync(int minutes);
}

// 테스트/개발용 — 실제로 끄지 않고 호출만 기록한다.
public sealed class TestSystemController : ISystemController
{
    public AgentStatus Status { get; set; } = new() { HostName = "TEST-PC", Version = "test", UserName = "tester" };
    public int ShutdownCalls { get; private set; }
    public int RebootCalls { get; private set; }
    public int LockCalls { get; private set; }
    public int ExtendCalls { get; private set; }
    public string? LastMessage { get; private set; }

    public AgentStatus GetStatus() => Status;
    public Task ShutdownAsync(int delaySeconds, string reason) { ShutdownCalls++; return Task.CompletedTask; }
    public Task RebootAsync(int delaySeconds, string reason) { RebootCalls++; return Task.CompletedTask; }
    public Task LockAsync() { LockCalls++; return Task.CompletedTask; }
    public Task ShowMessageAsync(string text) { LastMessage = text; return Task.CompletedTask; }
    public Task ExtendAsync(int minutes) { ExtendCalls++; return Task.CompletedTask; }
}
