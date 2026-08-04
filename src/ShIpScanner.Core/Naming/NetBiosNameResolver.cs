using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ShIpScanner.Core.Naming;

// NetBIOS Node Status(NBSTAT) 질의로 Windows PC 의 '컴퓨터 이름' 을 얻는다.
// [배경] 원본 faIpScanner 가 한글 PC명(예: "관리부-PC01")을 보여준 방식이 이것이다.
// 동작: UDP 137 로 노드 상태를 물으면, 상대 PC 가 자신이 가진 NetBIOS 이름 목록을 응답한다.
//       그 중 UNIQUE + 접미사 0x00(Workstation) 이름이 곧 컴퓨터 이름이다. 관리자 권한 불필요.
// [주의] 한글 이름은 로컬 코드페이지(CP949)로 인코딩되어 오므로 그 인코딩으로 디코드한다.
public sealed class NetBiosNameResolver : IHostNameResolver
{
    private readonly int _timeoutMs;
    private readonly Encoding _encoding;

    static NetBiosNameResolver()
    {
        // .NET Core 는 CP949 를 기본 탑재하지 않는다 — 공급자를 등록해야 GetEncoding(949) 가 된다.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public NetBiosNameResolver(int timeoutMs = 400)
    {
        _timeoutMs = timeoutMs;
        Encoding enc;
        try { enc = Encoding.GetEncoding(949); }   // 한글(CP949)
        catch { enc = Encoding.ASCII; }            // 최후 폴백
        _encoding = enc;
    }

    public async Task<string> ResolveAsync(string ip, CancellationToken ct)
    {
        try
        {
            using var udp = new UdpClient();
            var ep = new IPEndPoint(IPAddress.Parse(ip), 137);

            byte[] req = BuildNodeStatusRequest();
            await udp.SendAsync(req, req.Length, ep);

            // 타임아웃 있는 수신: 응답이 늦으면 포기(사용 중이 아닌 IP 는 응답이 없다).
            var recvTask = udp.ReceiveAsync();
            var done = await Task.WhenAny(recvTask, Task.Delay(_timeoutMs, ct));
            if (done != recvTask) return "";

            return ParseNodeStatusResponse(recvTask.Result.Buffer);
        }
        catch
        {
            return "";
        }
    }

    // NBSTAT 질의 패킷: 12바이트 헤더 + '*' 와일드카드 이름 + 타입/클래스.
    private static byte[] BuildNodeStatusRequest()
    {
        var m = new List<byte>(50);
        m.AddRange(new byte[] { 0x00, 0x00 }); // Transaction ID
        m.AddRange(new byte[] { 0x00, 0x00 }); // Flags
        m.AddRange(new byte[] { 0x00, 0x01 }); // Questions = 1
        m.AddRange(new byte[] { 0x00, 0x00 }); // Answer RRs
        m.AddRange(new byte[] { 0x00, 0x00 }); // Authority RRs
        m.AddRange(new byte[] { 0x00, 0x00 }); // Additional RRs
        // 질문 이름: '*'(0x2A) 를 16바이트로 채운 뒤 반옥텟 인코딩 → "CKAAAA…A"(32자)
        m.Add(0x20);
        m.AddRange(Encoding.ASCII.GetBytes("CKAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
        m.Add(0x00);
        m.AddRange(new byte[] { 0x00, 0x21 }); // Type = NBSTAT
        m.AddRange(new byte[] { 0x00, 0x01 }); // Class = IN
        return m.ToArray();
    }

    // 응답 파싱: 헤더(12) + 되돌아온 질문 이름(34) + type(2)+class(2)+ttl(4)+rdlen(2) 뒤부터 RDATA.
    // RDATA = [이름 개수(1)] + 개수만큼 [이름 15바이트][접미사 1][플래그 2].
    private string ParseNodeStatusResponse(byte[] r)
    {
        int idx = 12 + 34 + 2 + 2 + 4 + 2;
        if (r.Length <= idx) return "";

        int count = r[idx];
        idx++;

        for (int i = 0; i < count; i++)
        {
            if (idx + 18 > r.Length) break;

            string name = _encoding.GetString(r, idx, 15).TrimEnd(' ', '\0');
            byte suffix = r[idx + 15];
            int flags = (r[idx + 16] << 8) | r[idx + 17];
            bool isGroup = (flags & 0x8000) != 0;
            idx += 18;

            // Workstation 서비스(접미사 0x00) + UNIQUE(그룹 아님) 이름 = 컴퓨터 이름.
            if (suffix == 0x00 && !isGroup && name.Length > 0)
                return name;
        }
        return "";
    }
}
