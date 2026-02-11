namespace Lucide.NET;

public enum LucideLineCap
{
    Round,
    Butt,
    Square
}

public enum LucideLineJoin
{
    Round,
    Miter,
    Bevel
}

public sealed class LucideOptions
{
    public float StrokeWidth { get; set; } = 2f;
    public bool UseCurrentColor { get; set; } = true;
    public LucideLineCap LineCap { get; set; } = LucideLineCap.Round;
    public LucideLineJoin LineJoin { get; set; } = LucideLineJoin.Round;
    public bool IncludeXmlns { get; set; } = true;
    public string? Stroke { get; set; }

    internal bool IsDefault =>
        StrokeWidth.Equals(2f) &&
        UseCurrentColor &&
        LineCap == LucideLineCap.Round &&
        LineJoin == LucideLineJoin.Round &&
        IncludeXmlns &&
        string.IsNullOrEmpty(Stroke);
}
