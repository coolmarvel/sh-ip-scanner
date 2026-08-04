namespace ShIpScanner.Core.Naming;

// [개념: 인터페이스] '어떻게'가 아니라 '무엇을 할 수 있는가'만 정의하는 계약.
// 이름 조회 방법(역DNS / NetBIOS …)을 갈아끼울 수 있게 추상화한다 → 플랫폼 종속 격리에도 쓴다.
public interface IHostNameResolver
{
    // IP 의 호스트명(PC 이름)을 조회. 실패하면 빈 문자열을 반환한다(예외 대신).
    Task<string> ResolveAsync(string ip, CancellationToken ct);
}
