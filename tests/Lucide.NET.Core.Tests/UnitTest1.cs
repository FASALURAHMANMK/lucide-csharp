namespace Lucide.NET.Core.Tests;

public class CoreIconTests
{
    [Theory]
    [InlineData("shopping-cart", LucideIconKind.ShoppingCart)]
    [InlineData("ShoppingCart", LucideIconKind.ShoppingCart)]
    public void TryResolve_MatchesExpected(string name, LucideIconKind expected)
    {
        var result = Lucide.TryResolve(name, out var kind);

        Assert.True(result);
        Assert.Equal(expected, kind);
    }

    [Fact]
    public void GetSvg_ReturnsSvgMarkup()
    {
        var svg = Lucide.GetSvg(LucideIconKind.AlertCircle);

        Assert.Contains("<svg", svg);
        Assert.Contains("path", svg);
    }
}
