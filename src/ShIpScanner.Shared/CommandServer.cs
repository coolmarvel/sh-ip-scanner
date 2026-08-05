using System.Net;
using System.Net.Sockets;

namespace ShIpScanner.Shared;

// 에이전트측 TCP 명령 수신 서버.
// 접속 → 한 줄(JSON) 요청 읽기 → 인증 → ISystemController 로 위임 → 응답 한 줄 쓰기.
// [안전장치] AuthToken 이 일치하지 않으면 어떤 동작도 하지 않고 거부한다.
public sealed class CommandServer
{
    private readonly ISystemController _controller;
    private readonly string _authToken;
    private readonly int _port;
    private TcpListener? _listener;

    public CommandServer(ISystemController controller, string authToken, int port = AgentProtocol.DefaultPort)
    {
        _controller = controller;
        _authToken = authToken;
        _port = port;
    }

    // 실제 바인딩된 포트(테스트에서 0 을 주면 OS 가 정한 포트를 확인할 때 사용).
    public int Port => ((IPEndPoint)_listener!.LocalEndpoint).Port;

    public void Start()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
    }

    // 접속을 계속 받는 루프. 취소되면 종료.
    public async Task RunAsync(CancellationToken ct)
    {
        if (_listener is null) Start();
        using var reg = ct.Register(() => _listener!.Stop());
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient client;
                try { client = await _listener!.AcceptTcpClientAsync(ct); }
                catch (OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }

                _ = HandleAsync(client, ct); // 연결마다 비동기 처리(대기하지 않음)
            }
        }
        finally
        {
            _listener?.Stop();
        }
    }

    // 한 연결에서 1요청/1응답 처리.
    public async Task HandleAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream) { AutoFlush = true })
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null) return;

            CommandResponse response;
            try
            {
                var req = AgentProtocol.Deserialize<CommandRequest>(line);
                response = req is null
                    ? CommandResponse.Fail("잘못된 요청")
                    : await DispatchAsync(req);
            }
            catch (Exception ex)
            {
                response = CommandResponse.Fail(ex.Message);
            }

            await writer.WriteLineAsync(AgentProtocol.Serialize(response));
        }
    }

    private async Task<CommandResponse> DispatchAsync(CommandRequest req)
    {
        // 인증 실패는 즉시 거부(어떤 동작도 하지 않음).
        if (req.AuthToken != _authToken)
            return CommandResponse.Fail("인증 실패");

        switch (req.Type)
        {
            case CommandType.Status:
                return CommandResponse.Success(_controller.GetStatus());
            case CommandType.Shutdown:
                await _controller.ShutdownAsync(req.DelaySeconds, req.Text ?? "관리자 종료");
                return CommandResponse.Success(_controller.GetStatus());
            case CommandType.Reboot:
                await _controller.RebootAsync(req.DelaySeconds, req.Text ?? "관리자 재부팅");
                return CommandResponse.Success(_controller.GetStatus());
            case CommandType.Lock:
                await _controller.LockAsync();
                return CommandResponse.Success(_controller.GetStatus());
            case CommandType.Message:
                await _controller.ShowMessageAsync(req.Text ?? "");
                return CommandResponse.Success(_controller.GetStatus());
            case CommandType.ExtendSession:
                await _controller.ExtendAsync(req.ExtendMinutes);
                return CommandResponse.Success(_controller.GetStatus());
            case CommandType.SetSchedule:
                await _controller.SetScheduleAsync(req.ScheduleEnabled, req.ShutdownTime ?? "", req.AllowExtend);
                return CommandResponse.Success(_controller.GetStatus());
            default:
                return CommandResponse.Fail("알 수 없는 명령");
        }
    }
}
