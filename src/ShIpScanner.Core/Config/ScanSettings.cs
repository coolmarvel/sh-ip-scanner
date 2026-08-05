namespace ShIpScanner.Core.Config;

// 스캔 옵션(설정 모달에서 조정, %APPDATA%\sh Manager\settings.json 저장).
public sealed class ScanSettings
{
    public int TimeoutMs { get; set; } = 1000;   // 핑 타임아웃(ms)
    public int MaxParallel { get; set; } = 128;  // 동시 스캔 개수
    public bool ResolveNames { get; set; } = true; // PC명(호스트명) 조회 여부

    // 에이전트 연동(콘솔↔에이전트 공유). 토큰은 에이전트와 반드시 같아야 명령이 동작한다.
    public string AgentToken { get; set; } = "change-me";
    public int AgentPort { get; set; } = 47101;
    public bool DetectAgents { get; set; } = true; // 스캔 시 에이전트 설치 여부도 조회
}
