using System.Text.Json;

namespace ShIpScanner.Core.Config;

// 스캔 옵션을 JSON 으로 저장/로드. 위치는 SubnetStore 와 같은 %APPDATA%\sh IP Scanner\settings.json.
public sealed class ScanSettingsStore
{
    private readonly string _path;

    public ScanSettingsStore(string? path = null) => _path = path ?? DefaultPath();

    public static string DefaultPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "sh IP Scanner");
        return Path.Combine(dir, "settings.json");
    }

    public ScanSettings Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var s = JsonSerializer.Deserialize<ScanSettings>(File.ReadAllText(_path));
                if (s != null) return Clamp(s);
            }
        }
        catch { }
        return new ScanSettings();
    }

    public void Save(ScanSettings s)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(Clamp(s), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    // 비정상 값 방지(느려지거나 리소스 폭주하지 않게).
    private static ScanSettings Clamp(ScanSettings s)
    {
        s.TimeoutMs = Math.Clamp(s.TimeoutMs, 100, 10000);
        s.MaxParallel = Math.Clamp(s.MaxParallel, 1, 512);
        return s;
    }
}
