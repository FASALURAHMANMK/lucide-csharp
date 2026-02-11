using System.Windows.Media;

namespace Lucide.NET.Wpf.Tests;

public class WpfCacheTests
{
    [Fact]
    public void Cache_ReturnsSameInstanceForSameKey()
    {
        StaTestRunner.Run(() =>
        {
            var stroke = Brushes.Black;
            var first = new LucideIcon
            {
                Kind = LucideIconKind.House,
                Stroke = stroke,
                StrokeThickness = 2d,
                Size = 24d
            };

            var second = new LucideIcon
            {
                Kind = LucideIconKind.House,
                Stroke = stroke,
                StrokeThickness = 2d,
                Size = 24d
            };

            var firstSource = first.Source;
            var secondSource = second.Source;

            Assert.NotNull(firstSource);
            Assert.Same(firstSource, secondSource);
        });
    }
}
