using System.Text.Json;

namespace ShIpScanner.Core.Config;

// 스캔 대역 목록을 JSON 으로 저장/로드한다.
// 저장 위치: %APPDATA%\sh IP Scanner\subnets.json (사용자가 쓰기 가능한 표준 위치 — Program Files 아님).
// 관리자는 앱에서 대역을 추가/삭제하거나, 이 파일을 직접 편집해 여러 대역을 관리한다.
public sealed class SubnetStore
{
    private readonly string _path;

    public SubnetStore(string? path = null) => _path = path ?? DefaultPath();

    public static string DefaultPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "sh IP Scanner");
        return Path.Combine(dir, "subnets.json");
    }

    public List<SubnetDefinition> Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var list = JsonSerializer.Deserialize<List<SubnetDefinition>>(File.ReadAllText(_path));
                if (list is { Count: > 0 }) return Sanitize(list);
            }
        }
        catch
        {
            // 파일 손상 등은 기본값으로 폴백(앱이 죽지 않게).
        }
        return Defaults();
    }

    public void Save(IEnumerable<SubnetDefinition> subnets)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(subnets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
        catch
        {
            // 저장 실패는 조용히 무시(다음 실행 때 기본값). 필요 시 로깅 추가.
        }
    }

    // 중복 제거 + 유효성 필터.
    private static List<SubnetDefinition> Sanitize(List<SubnetDefinition> list)
    {
        var seen = new HashSet<string>();
        var result = new List<SubnetDefinition>();
        foreach (var s in list)
        {
            var b = (s?.Base ?? "").Trim();
            if (!IsValidBase(b) || !seen.Add(b)) continue;
            result.Add(new SubnetDefinition { Base = b, Label = (s!.Label ?? "").Trim() });
        }
        return result.Count > 0 ? result : Defaults();
    }

    // 첫 실행 기본 대역 — 운영 중인 3개 대역. 관리자가 앱/파일에서 자유롭게 바꾼다.
    public static List<SubnetDefinition> Defaults() => new()
    {
        new SubnetDefinition { Base = "192.168.80" },
        new SubnetDefinition { Base = "192.168.85" },
        new SubnetDefinition { Base = "192.168.95" },
    };

    // "192.168.80" 형식(3옥텟, 각 0~255) 검증.
    public static bool IsValidBase(string b)
    {
        if (string.IsNullOrWhiteSpace(b)) return false;
        var parts = b.Split('.');
        if (parts.Length != 3) return false;
        foreach (var p in parts)
            if (!int.TryParse(p, out var n) || n < 0 || n > 255) return false;
        return true;
    }
}
