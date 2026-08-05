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
    SetSchedule,   // 자동 종료 일정 설정(관리자만) — 종료시각·사용여부·자체연장 허용
}

// 콘솔 → 에이전트 요청. AuthToken 이 맞아야 실행한다(인증).
public sealed class CommandRequest
{
    public CommandType Type { get; set; }
    public string AuthToken { get; set; } = "";
    public string? Text { get; set; }         // Message 내용
    public int DelaySeconds { get; set; } = 60; // Shutdown/Reboot 경고 유예
    public int ExtendMinutes { get; set; } = 60; // ExtendSession 연장 시간

    // SetSchedule 용(관리자가 종료 일정을 푸시). 종료시각은 에이전트가 스스로 못 바꾼다.
    public bool ScheduleEnabled { get; set; } = true;
    public string? ShutdownTime { get; set; } // "HH:mm"
    public bool AllowExtend { get; set; } = true; // 연장근무 자체연기 허용 여부(관리자 결정)
}

// 에이전트의 현재 상태(콘솔이 목록에 표시).
public sealed class AgentStatus
{
    public string HostName { get; set; } = "";
    public string Version { get; set; } = "";
    public string UserName { get; set; } = "";
    public DateTime? NextShutdown { get; set; }
    public bool ExtendedNow { get; set; } // 지금 연장 상태인지

    // 관리자가 설정한 일정(에이전트는 표시만).
    public string ShutdownTime { get; set; } = "";
    public bool ScheduleEnabled { get; set; }
    public bool AllowExtend { get; set; }
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
