namespace ShIpScanner.Core.Config;

// 스캔 옵션(설정 모달에서 조정, %APPDATA%\sh IP Scanner\settings.json 저장).
public sealed class ScanSettings
{
    public int TimeoutMs { get; set; } = 1000;   // 핑 타임아웃(ms)
    public int MaxParallel { get; set; } = 128;  // 동시 스캔 개수
    public bool ResolveNames { get; set; } = true; // PC명(호스트명) 조회 여부
}
