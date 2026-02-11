using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lucide.NET.Wpf;

public sealed class LucideIcon : Image
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(LucideIconKind),
        typeof(LucideIcon),
        new FrameworkPropertyMetadata(default(LucideIconKind), OnVisualPropertyChanged));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke),
        typeof(Brush),
        typeof(LucideIcon),
        new FrameworkPropertyMetadata(Brushes.Black, OnVisualPropertyChanged));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness),
        typeof(double),
        typeof(LucideIcon),
        new FrameworkPropertyMetadata(2d, OnVisualPropertyChanged));

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size),
        typeof(double),
        typeof(LucideIcon),
        new FrameworkPropertyMetadata(0d, OnVisualPropertyChanged));

    public LucideIcon()
    {
        Stretch = Stretch.Uniform;
        UpdateSource();
    }

    public LucideIconKind Kind
    {
        get => (LucideIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LucideIcon icon)
        {
            icon.UpdateSource();
        }
    }

    private void UpdateSource()
    {
        var stroke = Stroke ?? Brushes.Black;
        var thickness = StrokeThickness <= 0 ? 2d : StrokeThickness;
        var size = ResolveSize();

        if (Size > 0)
        {
            Width = Size;
            Height = Size;
        }

        Source = LucideDrawingCache.GetOrCreate(Kind, stroke, thickness, size);
    }

    private double ResolveSize()
    {
        if (Size > 0)
        {
            return Size;
        }

        var width = double.IsNaN(Width) ? 0d : Width;
        var height = double.IsNaN(Height) ? 0d : Height;
        return Math.Max(width, height);
    }
}
