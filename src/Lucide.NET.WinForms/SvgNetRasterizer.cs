using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using Svg;

namespace Lucide.NET.WinForms;

internal sealed class SvgNetRasterizer : ISvgRasterizer
{
    public Bitmap Rasterize(string svg, int size, float dpi)
    {
        Trace.WriteLine("Lucide.NET.WinForms: rasterizing SVG.");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var document = SvgDocument.Open<SvgDocument>(stream);
        if (document is null)
        {
            throw new InvalidOperationException("Failed to parse SVG content.");
        }

        var safeDpi = dpi <= 0 ? 96f : dpi;
        document.Ppi = (int)Math.Round(safeDpi);
        var bitmap = document.Draw(size, size);
        bitmap.SetResolution(safeDpi, safeDpi);
        return bitmap;
    }
}
