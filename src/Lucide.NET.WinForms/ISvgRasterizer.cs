using System.Drawing;

namespace Lucide.NET.WinForms;

internal interface ISvgRasterizer
{
    Bitmap Rasterize(string svg, int size, float dpi);
}
