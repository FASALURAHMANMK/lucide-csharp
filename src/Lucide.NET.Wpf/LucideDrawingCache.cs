using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace Lucide.NET.Wpf;

internal static class LucideDrawingCache
{
    private static readonly ConcurrentDictionary<CacheKey, Lazy<DrawingImage>> Cache = new();

    public static DrawingImage GetOrCreate(LucideIconKind kind, Brush stroke, double strokeThickness, double size)
    {
        var color = GetColor(stroke);
        var key = new CacheKey(kind, strokeThickness, color, size);

        return Cache.GetOrAdd(key, _ => new Lazy<DrawingImage>(() => CreateDrawing(kind, strokeThickness, color))).Value;
    }

    private static DrawingImage CreateDrawing(LucideIconKind kind, double strokeThickness, Color color)
    {
        Trace.WriteLine($"Lucide.NET.Wpf: parsing SVG for {kind}.");
        var options = new LucideOptions
        {
            StrokeWidth = (float)strokeThickness,
            UseCurrentColor = false,
            Stroke = ColorToString(color)
        };

        var svg = Lucide.GetSvg(kind, options);
        var settings = new WpfDrawingSettings
        {
            IncludeRuntime = false,
            TextAsGeometry = true,
            OptimizePath = true
        };

        var reader = new FileSvgReader(settings, false);
        using var textReader = new StringReader(svg);
        var drawing = reader.Read(textReader);
        if (drawing is null)
        {
            throw new InvalidOperationException("Failed to parse SVG content.");
        }

        if (drawing.CanFreeze)
        {
            drawing.Freeze();
        }

        var image = new DrawingImage(drawing);
        if (image.CanFreeze)
        {
            image.Freeze();
        }

        return image;
    }

    private static Color GetColor(Brush stroke)
    {
        if (stroke is SolidColorBrush solid)
        {
            return solid.Color;
        }

        return Colors.Black;
    }

    private static string ColorToString(Color color)
    {
        return color.A == byte.MaxValue
            ? string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)
            : string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
    }

    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        public CacheKey(LucideIconKind kind, double strokeThickness, Color color, double size)
        {
            Kind = kind;
            StrokeThickness = strokeThickness;
            Color = color;
            Size = size;
        }

        public LucideIconKind Kind { get; }
        public double StrokeThickness { get; }
        public Color Color { get; }
        public double Size { get; }

        public bool Equals(CacheKey other)
        {
            return Kind == other.Kind
                   && StrokeThickness.Equals(other.StrokeThickness)
                   && Color.Equals(other.Color)
                   && Size.Equals(other.Size);
        }

        public override bool Equals(object? obj)
        {
            return obj is CacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)Kind;
                hash = (hash * 397) ^ StrokeThickness.GetHashCode();
                hash = (hash * 397) ^ Color.GetHashCode();
                hash = (hash * 397) ^ Size.GetHashCode();
                return hash;
            }
        }
    }
}
