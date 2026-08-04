using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ShIpScanner.Core.Net;

// [개념: static 유틸 클래스] 인스턴스 상태가 필요 없는 순수 함수 모음이다.
// C# 에서는 이런 도우미를 static class 로 묶는다(생성 불가, 메서드도 전부 static).
public static class LocalNetwork
{
    // 현재 PC 의 '주 IPv4' 를 찾는다. 원본 faIpScanner 가 로컬 IP 를 자동으로 잡아 텍스트박스에
    // 채워주던 것과 같은 역할. Up 상태의 비-루프백 인터페이스에서 첫 유효 IPv4 를 고른다.
    public static IPAddress? GetPrimaryIPv4()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue; // IPv4 만
                var b = ua.Address.GetAddressBytes();
                if (b[0] == 127) continue;              // 루프백
                if (b[0] == 169 && b[1] == 254) continue; // APIPA/link-local
                return ua.Address;
            }
        }
        return null;
    }

    // "192.168.80.103" → "192.168.80" (앞 3옥텟). /24 스캔의 기준 대역.
    public static string? GetSubnetBase(IPAddress ip)
    {
        var b = ip.GetAddressBytes();
        return b.Length != 4 ? null : $"{b[0]}.{b[1]}.{b[2]}";
    }
}
