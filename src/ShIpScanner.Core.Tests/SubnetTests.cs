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
        Assert.Contains(d, x => x.Base == "192.168.95");
    }
}
