using System;
using System.IO;
using System.Text.Json;
using ShIpScanner.Shared;

namespace ShIpScanner.Agent;

// 에이전트 설정(설치 시 주입/이후 편집). %APPDATA%\sh Agent\agent.json 에 저장.
public sealed class AgentConfig
{
    public int Port { get; set; } = AgentProtocol.DefaultPort;
    public string AuthToken { get; set; } = "change-me";  // 콘솔과 공유하는 인증 토큰
    public bool ScheduleEnabled { get; set; } = true;      // 자동 종료 사용
    public string ShutdownTime { get; set; } = "19:00";    // 종료 시각(HH:mm)
    public int WarnLeadSeconds { get; set; } = 600;         // 종료 10분 전 경고
    public bool AllowExtend { get; set; } = true;           // 연장근무 연기 허용
    public int ExtendMinutes { get; set; } = 60;            // 1회 연기 시간
    public int MaxExtends { get; set; } = 3;                // 하루 최대 연기 횟수

    public TimeOnly ShutdownTimeOnly =>
        TimeOnly.TryParse(ShutdownTime, out var t) ? t : new TimeOnly(19, 0);

    public static string Dir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sh Agent");
    public static string Path_ => System.IO.Path.Combine(Dir, "agent.json");

    public static AgentConfig Load()
    {
        try
        {
            if (File.Exists(Path_))
            {
                var c = JsonSerializer.Deserialize<AgentConfig>(File.ReadAllText(Path_));
                if (c != null) return c;
            }
        }
        catch { }
        return new AgentConfig();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(Path_, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
