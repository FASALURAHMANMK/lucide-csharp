using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Lucide.NET.WinForms;

public static class LucideWinForms
{
    private static readonly ISvgRasterizer Rasterizer = new SvgNetRasterizer();
    private static readonly ConcurrentDictionary<CacheKey, Lazy<Bitmap>> Cache = new();

    public static Bitmap ToBitmap(LucideIconKind kind, int size, Color strokeColor, float strokeWidth = 2f, float dpi = 96f)
    {
        size = Math.Max(1, size);
        strokeWidth = strokeWidth <= 0 ? 2f : strokeWidth;
        dpi = dpi <= 0 ? 96f : dpi;

        var key = new CacheKey(kind, size, strokeColor.ToArgb(), strokeWidth, dpi);
        return Cache.GetOrAdd(key, _ => new Lazy<Bitmap>(() => Render(kind, size, strokeColor, strokeWidth, dpi))).Value;
    }

    public static Icon ToIcon(LucideIconKind kind, int size, Color strokeColor, float strokeWidth = 2f, float dpi = 96f)
    {
        var bitmap = ToBitmap(kind, size, strokeColor, strokeWidth, dpi);
        var hIcon = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(hIcon);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static Bitmap Render(LucideIconKind kind, int size, Color strokeColor, float strokeWidth, float dpi)
    {
        var options = new LucideOptions
        {
            StrokeWidth = strokeWidth,
            UseCurrentColor = false,
            Stroke = ColorToString(strokeColor)
        };

        var svg = Lucide.GetSvg(kind, options);
        return Rasterizer.Rasterize(svg, size, dpi);
    }

    private static string ColorToString(Color color)
    {
        return color.A == byte.MaxValue
            ? string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)
            : string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        public CacheKey(LucideIconKind kind, int size, int color, float strokeWidth, float dpi)
        {
            Kind = kind;
            Size = size;
            Color = color;
            StrokeWidth = strokeWidth;
            Dpi = dpi;
        }

        public LucideIconKind Kind { get; }
        public int Size { get; }
        public int Color { get; }
        public float StrokeWidth { get; }
        public float Dpi { get; }

        public bool Equals(CacheKey other)
        {
            return Kind == other.Kind
                   && Size == other.Size
                   && Color == other.Color
                   && StrokeWidth.Equals(other.StrokeWidth)
                   && Dpi.Equals(other.Dpi);
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
                hash = (hash * 397) ^ Size;
                hash = (hash * 397) ^ Color;
                hash = (hash * 397) ^ StrokeWidth.GetHashCode();
                hash = (hash * 397) ^ Dpi.GetHashCode();
                return hash;
            }
        }
    }
}
