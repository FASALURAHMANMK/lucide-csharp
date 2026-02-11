namespace Lucide.NET.WinForms.Tests;

public class WinFormsCacheTests
{
    [Fact]
    public void ToBitmap_ReturnsCachedInstanceForSameKey()
    {
        var first = LucideWinForms.ToBitmap(LucideIconKind.CircleAlert, 24, System.Drawing.Color.Black);
        var second = LucideWinForms.ToBitmap(LucideIconKind.CircleAlert, 24, System.Drawing.Color.Black);

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.Equal(24, first.Width);
        Assert.Equal(24, first.Height);
    }
}
