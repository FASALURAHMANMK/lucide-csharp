using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace Lucide.NET.Wpf;

[MarkupExtensionReturnType(typeof(DrawingImage))]
public sealed class LucideImageExtension : MarkupExtension
{
    public LucideImageExtension()
    {
    }

    public LucideImageExtension(LucideIconKind kind)
    {
        Kind = kind;
    }

    [ConstructorArgument("Kind")]
    public LucideIconKind Kind { get; set; }

    public Brush? Brush { get; set; }

    public double StrokeThickness { get; set; } = 2d;

    public double Size { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var stroke = Brush ?? Brushes.Black;
        var thickness = StrokeThickness <= 0 ? 2d : StrokeThickness;
        var size = Size > 0 ? Size : 0d;
        return LucideDrawingCache.GetOrCreate(Kind, stroke, thickness, size);
    }
}
