using System.Globalization;

namespace Lucide.NET;

public static class Lucide
{
    private static readonly ILucideIconSource Source = new EmbeddedLucideIconSource();
    private static readonly Lazy<IReadOnlyList<string>> Names = new(() =>
        LucideAliases.Map.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray());

    public static string GetSvg(LucideIconKind kind, LucideOptions? options = null)
    {
        var svg = Source.GetSvg(kind);
        return ApplyOptions(svg, options);
    }

    public static bool TryResolve(string name, out LucideIconKind kind)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            kind = default;
            return false;
        }

        return LucideAliases.Map.TryGetValue(name, out kind);
    }

    public static IReadOnlyList<string> GetAllNames() => Names.Value;

    private static string ApplyOptions(string svg, LucideOptions? options)
    {
        if (options is null || options.IsDefault)
        {
            return svg;
        }

        var tagStart = svg.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
        if (tagStart < 0)
        {
            return svg;
        }

        var tagEnd = svg.IndexOf('>', tagStart);
        if (tagEnd < 0)
        {
            return svg;
        }

        var tag = svg.Substring(tagStart, tagEnd - tagStart + 1);
        var updated = tag;

        var strokeWidth = options.StrokeWidth.ToString(CultureInfo.InvariantCulture);
        updated = SetAttribute(updated, "stroke-width", strokeWidth);

        updated = SetAttribute(updated, "stroke-linecap", LineCapToString(options.LineCap));
        updated = SetAttribute(updated, "stroke-linejoin", LineJoinToString(options.LineJoin));

        if (!string.IsNullOrWhiteSpace(options.Stroke))
        {
            updated = SetAttribute(updated, "stroke", options.Stroke!);
        }
        else if (options.UseCurrentColor)
        {
            updated = SetAttribute(updated, "stroke", "currentColor");
        }

        if (!options.IncludeXmlns)
        {
            updated = RemoveAttribute(updated, "xmlns");
        }

        if (updated == tag)
        {
            return svg;
        }

        return svg.Substring(0, tagStart) + updated + svg.Substring(tagEnd + 1);
    }

    private static string SetAttribute(string tag, string attribute, string value)
    {
        var pattern = attribute + "=\"";
        var index = tag.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            var valueStart = index + pattern.Length;
            var valueEnd = tag.IndexOf('"', valueStart);
            if (valueEnd > valueStart)
            {
                return tag.Substring(0, valueStart) + value + tag.Substring(valueEnd);
            }
        }

        var insertPos = tag.LastIndexOf('>');
        if (insertPos <= 0)
        {
            return tag;
        }

        var insertion = $" {attribute}=\"{value}\"";
        return tag.Insert(insertPos, insertion);
    }

    private static string RemoveAttribute(string tag, string attribute)
    {
        var pattern = attribute + "=\"";
        var index = tag.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return tag;
        }

        var valueStart = index + pattern.Length;
        var valueEnd = tag.IndexOf('"', valueStart);
        if (valueEnd < 0)
        {
            return tag;
        }

        var removeStart = index;
        if (removeStart > 0 && tag[removeStart - 1] == ' ')
        {
            removeStart--;
        }

        return tag.Remove(removeStart, valueEnd - removeStart + 1);
    }

    private static string LineCapToString(LucideLineCap lineCap)
    {
        return lineCap switch
        {
            LucideLineCap.Butt => "butt",
            LucideLineCap.Square => "square",
            _ => "round"
        };
    }

    private static string LineJoinToString(LucideLineJoin lineJoin)
    {
        return lineJoin switch
        {
            LucideLineJoin.Miter => "miter",
            LucideLineJoin.Bevel => "bevel",
            _ => "round"
        };
    }
}
