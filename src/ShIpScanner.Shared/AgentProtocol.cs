using System.Text.Json;

namespace ShIpScanner.Shared;

// 전송 규약 상수·직렬화. 한 줄(개행 구분) JSON 으로 1요청/1응답을 주고받는다.
// 단순·디버그 용이. (프레이밍이 개행이라 페이로드에 개행이 없도록 JSON 은 한 줄로 직렬화한다.)
public static class AgentProtocol
{
    public const int DefaultPort = 47101;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false, // 한 줄 유지(개행 프레이밍과 충돌 방지)
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
