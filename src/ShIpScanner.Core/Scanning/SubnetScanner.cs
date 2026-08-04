using System.Net.NetworkInformation;

namespace ShIpScanner.Core.Scanning;

// 서브넷 /24 (기준.1 ~ 기준.254) 를 병렬로 핑 스윕한다.
// [학습 핵심] 원본 faIpScanner 의 TCheckIPThread(스레드를 254개 굴리던 방식) 를,
// .NET 에서는 스레드 대신 async/await + SemaphoreSlim 으로 재현한다.
//   - Task: '언젠가 끝날 작업'을 나타내는 값. 스레드보다 가볍다.
//   - SemaphoreSlim: 동시에 도는 작업 수를 제한하는 '입장권 N장'짜리 게이트.
//   - CancellationToken: 사용자가 '중지'를 누르면 협조적으로 멈추게 하는 신호.
public sealed class SubnetScanner
{
    private readonly int _maxParallel;
    private readonly int _timeoutMs;

    public SubnetScanner(int maxParallel = 128, int timeoutMs = 1000)
    {
        _maxParallel = maxParallel;
        _timeoutMs = timeoutMs;
    }

    // subnetBase 예: "192.168.80". 각 IP 결과를 progress 로 즉시 흘려보낸다(점진적 표시).
    public async Task ScanAsync(string subnetBase, IProgress<PingOutcome> progress, CancellationToken ct)
    {
        using var gate = new SemaphoreSlim(_maxParallel);
        var tasks = new List<Task>(254);

        for (int octet = 1; octet <= 254; octet++)
        {
            ct.ThrowIfCancellationRequested();
            await gate.WaitAsync(ct); // 입장권 대기(동시 실행 수 제한)

            int o = octet; // 클로저 캡처용 지역 복사
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    progress.Report(await PingOneAsync(subnetBase, o));
                }
                finally
                {
                    gate.Release(); // 입장권 반납
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<PingOutcome> PingOneAsync(string subnetBase, int octet)
    {
        string ip = $"{subnetBase}.{octet}";
        using var ping = new Ping();
        try
        {
            // ICMP 핑은 관리자 권한이 필요 없다.
            var reply = await ping.SendPingAsync(ip, _timeoutMs);
            bool alive = reply.Status == IPStatus.Success;
            return new PingOutcome(octet, ip, alive, alive ? reply.RoundtripTime : 0);
        }
        catch
        {
            // 네트워크 오류 등은 '응답 없음'으로 취급.
            return new PingOutcome(octet, ip, false, 0);
        }
    }
}
