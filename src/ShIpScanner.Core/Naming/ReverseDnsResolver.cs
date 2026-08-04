using System.Net;

namespace ShIpScanner.Core.Naming;

// 표준 역DNS(PTR) 조회. 도메인/DNS 가 잘 갖춰진 환경에서 PC 이름이 잡히기도 한다.
// NetBIOS 로 못 얻었을 때의 폴백으로 쓴다.
public sealed class ReverseDnsResolver : IHostNameResolver
{
    public async Task<string> ResolveAsync(string ip, CancellationToken ct)
    {
        try
        {
            var entry = await Dns.GetHostEntryAsync(ip, ct);
            var name = entry.HostName ?? "";
            // FQDN(pc1.corp.local) 이면 첫 라벨(pc1)만 취한다.
            int dot = name.IndexOf('.');
            return dot > 0 ? name[..dot] : name;
        }
        catch
        {
            return "";
        }
    }
}
