using System.Linq;
using ShIpScanner.Core.Config;
using Xunit;

namespace ShIpScanner.Core.Tests;

public class SubnetTests
{
    [Theory]
    [InlineData("192.168.0", true)]
    [InlineData("10.0.1", true)]
    [InlineData("172.16.5", true)]
    [InlineData("192.168.0.1", false)] // 4옥텟은 base 가 아님
    [InlineData("192.168", false)]     // 옥텟 부족
    [InlineData("192.168.256", false)] // 범위 초과
    [InlineData("a.b.c", false)]
    [InlineData("", false)]
    public void IsValidBase_Works(string input, bool expected)
        => Assert.Equal(expected, SubnetStore.IsValidBase(input));

    // 기본 대역을 하드코딩하지 않는다 — 파일이 없으면 빈 목록(=첫 실행)이어야 한다.
    [Fact]
    public void Load_WithoutFile_ReturnsEmpty()
    {
        var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sh-ip-no-such-file.json");
        System.IO.File.Delete(tmp);
        Assert.Empty(new SubnetStore(tmp).Load());
    }

    // 손상/무효 항목은 걸러내고 유효 항목만 남긴다(전부 무효면 빈 목록).
    [Fact]
    public void Load_FiltersInvalidAndDuplicateEntries()
    {
        var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sh-ip-sanitize-test.json");
        System.IO.File.WriteAllText(tmp,
            "[{\"Base\":\"192.168.0\"},{\"Base\":\"192.168.0\"},{\"Base\":\"bad\"},{\"Base\":\"10.0.1\"}]");
        var loaded = new SubnetStore(tmp).Load();
        Assert.Equal(new[] { "192.168.0", "10.0.1" }, loaded.Select(s => s.Base));
        System.IO.File.Delete(tmp);
    }
}

public class ScanSettingsTests
{
    [Fact]
    public void Store_Clamps_OutOfRange_Values()
    {
        var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sh-ip-settings-test.json");
        var store = new ScanSettingsStore(tmp);
        store.Save(new ScanSettings { TimeoutMs = 99999, MaxParallel = 99999, ResolveNames = false });
        var loaded = store.Load();
        Assert.InRange(loaded.TimeoutMs, 100, 10000);
        Assert.InRange(loaded.MaxParallel, 1, 512);
        Assert.False(loaded.ResolveNames);
        System.IO.File.Delete(tmp);
    }
}
