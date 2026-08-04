namespace ShIpScanner.Core.Naming;

// 여러 리졸버를 순서대로 시도해 첫 성공을 반환한다.
// 기본 순서: NetBIOS(한글 PC명, 원본과 동일) → 실패 시 역DNS.
public sealed class CompositeHostNameResolver : IHostNameResolver
{
    private readonly IHostNameResolver[] _resolvers;

    public CompositeHostNameResolver(params IHostNameResolver[] resolvers) => _resolvers = resolvers;

    public async Task<string> ResolveAsync(string ip, CancellationToken ct)
    {
        foreach (var r in _resolvers)
        {
            var name = await r.ResolveAsync(ip, ct);
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        return "";
    }
}
