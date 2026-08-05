namespace ShIpScanner.Shared;

// 콘솔↔에이전트가 주고받는 명령 종류.
public enum CommandType
{
    Status,        // 상태 조회(호스트명·버전·사용자·다음 종료시각)
    Shutdown,      // 종료(경고 후)
    Reboot,        // 재부팅
    Lock,          // 화면 잠금
    Message,       // 안내 메시지 표시
    ExtendSession, // 연장(자동 종료 연기)
}

// 콘솔 → 에이전트 요청. AuthToken 이 맞아야 실행한다(인증).
public sealed class CommandRequest
{
    public CommandType Type { get; set; }
    public string AuthToken { get; set; } = "";
    public string? Text { get; set; }         // Message 내용
    public int DelaySeconds { get; set; } = 60; // Shutdown/Reboot 경고 유예
    public int ExtendMinutes { get; set; } = 60; // ExtendSession 연장 시간
}

// 에이전트의 현재 상태(콘솔이 목록에 표시).
public sealed class AgentStatus
{
    public string HostName { get; set; } = "";
    public string Version { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTime? NextShutdown { get; set; }
    public bool ExtendedNow { get; set; } // 지금 연장 상태인지
}

// 에이전트 → 콘솔 응답.
public sealed class CommandResponse
{
    public bool Ok { get; set; }
    public string? Error { get; set; }
    public AgentStatus? Status { get; set; }

    public static CommandResponse Fail(string error) => new() { Ok = false, Error = error };
    public static CommandResponse Success(AgentStatus? status = null) => new() { Ok = true, Status = status };
}
