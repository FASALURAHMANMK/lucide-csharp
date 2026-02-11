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
        var svg = Lucide.GetSvg(LucideIconKind.CircleAlert);

        Assert.Contains("<svg", svg);
        Assert.Contains("xmlns=", svg);
        Assert.Contains("</svg>", svg);

        var containsAnyShape =
            svg.Contains("<path", System.StringComparison.Ordinal) ||
            svg.Contains("<circle", System.StringComparison.Ordinal) ||
            svg.Contains("<line", System.StringComparison.Ordinal) ||
            svg.Contains("<polyline", System.StringComparison.Ordinal) ||
            svg.Contains("<polygon", System.StringComparison.Ordinal) ||
            svg.Contains("<rect", System.StringComparison.Ordinal);

        Assert.True(containsAnyShape);
    }
}
