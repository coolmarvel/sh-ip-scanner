using System.Net.Sockets;

namespace ShIpScanner.Shared;

// 콘솔측 클라이언트 — 특정 IP 의 에이전트에 접속해 상태조회/명령을 보낸다.
public static class AgentClient
{
    // 요청 1건 전송 후 응답 수신. 접속 실패/타임아웃이면 null(=에이전트 없음/무응답).
    public static async Task<CommandResponse?> SendAsync(
        string ip, CommandRequest req, int port = AgentProtocol.DefaultPort, int timeoutMs = 800)
    {
        using var client = new TcpClient();
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            await client.ConnectAsync(ip, port, cts.Token);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            using var reader = new StreamReader(stream);

            await writer.WriteLineAsync(AgentProtocol.Serialize(req).AsMemory(), cts.Token);
            var line = await reader.ReadLineAsync(cts.Token);
            return line is null ? null : AgentProtocol.Deserialize<CommandResponse>(line);
        }
        catch
        {
            return null; // 접속 불가·타임아웃·오류 = 에이전트 미설치/무응답으로 취급
        }
    }

    // 편의: 상태만 조회. 에이전트가 있으면 AgentStatus, 없으면 null.
    public static async Task<AgentStatus?> GetStatusAsync(
        string ip, string authToken, int port = AgentProtocol.DefaultPort, int timeoutMs = 800)
    {
        var resp = await SendAsync(ip, new CommandRequest { Type = CommandType.Status, AuthToken = authToken }, port, timeoutMs);
        return resp is { Ok: true } ? resp.Status : null;
    }
}
