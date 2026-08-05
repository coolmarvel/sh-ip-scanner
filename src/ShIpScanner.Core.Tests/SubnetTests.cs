using System.Linq;
using ShIpScanner.Core.Config;
using Xunit;

namespace ShIpScanner.Core.Tests;

public class SubnetTests
{
    [Theory]
    [InlineData("192.168.80", true)]
    [InlineData("192.168.0", true)]
    [InlineData("192.168.85", true)]
    [InlineData("192.168.80.1", false)] // 4옥텟은 base 가 아님
    [InlineData("192.168", false)]      // 옥텟 부족
    [InlineData("192.168.256", false)]  // 범위 초과
    [InlineData("a.b.c", false)]
    [InlineData("", false)]
    public void IsValidBase_Works(string input, bool expected)
        => Assert.Equal(expected, SubnetStore.IsValidBase(input));

    [Fact]
    public void Defaults_ContainThreeOperatingSubnets()
    {
        var d = SubnetStore.Defaults();
        Assert.Contains(d, x => x.Base == "192.168.80");
        Assert.Contains(d, x => x.Base == "192.168.85");
        Assert.Contains(d, x => x.Base == "192.168.90");
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

public class SubnetMigrationTests
{
    [Fact]
    public void LegacyTypo_95_Is_Migrated_To_90_On_Load()
    {
        var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sh-ip-migrate-test.json");
        System.IO.File.WriteAllText(tmp,
            "[{\"Base\":\"192.168.80\"},{\"Base\":\"192.168.85\"},{\"Base\":\"192.168.95\"}]");
        var loaded = new SubnetStore(tmp).Load();
        var bases = loaded.Select(s => s.Base).ToList();
        Assert.Contains("192.168.90", bases);
        Assert.DoesNotContain("192.168.95", bases);
        System.IO.File.Delete(tmp);
    }
}
