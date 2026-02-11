using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string ExpectedCompression = "gzip";

var argsMap = ParseArgs(args);
if (!argsMap.TryGetValue("--input", out var inputArg) || string.IsNullOrWhiteSpace(inputArg))
{
    Fail("Missing required --input <folder> argument.");
}

if (!argsMap.TryGetValue("--output", out var outputArg) || string.IsNullOrWhiteSpace(outputArg))
{
    Fail("Missing required --output <repo_root> argument.");
}

var compressArg = argsMap.TryGetValue("--compress", out var compress) ? compress : ExpectedCompression;
if (!string.Equals(compressArg, ExpectedCompression, StringComparison.OrdinalIgnoreCase))
{
    Fail($"Unsupported compression '{compressArg}'. Only '{ExpectedCompression}' is supported.");
}

var inputPath = Path.GetFullPath(inputArg!);
var outputRoot = Path.GetFullPath(outputArg!);

if (!Directory.Exists(inputPath))
{
    Fail($"Input folder not found: {inputPath}");
}

if (!Directory.Exists(outputRoot))
{
    Fail($"Output root not found: {outputRoot}");
}

var svgFiles = Directory.EnumerateFiles(inputPath, "*.svg", SearchOption.AllDirectories)
    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (svgFiles.Length == 0)
{
    Fail($"No .svg files found under {inputPath}");
}

var iconDefs = new List<IconDefinition>(svgFiles.Length);
var normalizedMap = new Dictionary<string, string>(StringComparer.Ordinal);

foreach (var file in svgFiles)
{
    var name = Path.GetFileNameWithoutExtension(file);
    var normalized = NormalizeName(name);
    if (normalizedMap.TryGetValue(normalized, out var existing))
    {
        Fail($"Duplicate normalized name '{normalized}' from '{existing}' and '{name}'.");
    }

    normalizedMap[normalized] = name;
    iconDefs.Add(new IconDefinition(name, normalized, file));
}

iconDefs.Sort((a, b) => StringComparer.Ordinal.Compare(a.Normalized, b.Normalized));

var generatedDir = Path.Combine(outputRoot, "src", "Lucide.NET.Core", "Generated");
var resourcesDir = Path.Combine(outputRoot, "src", "Lucide.NET.Core", "Resources");
Directory.CreateDirectory(generatedDir);
Directory.CreateDirectory(resourcesDir);

var entries = new List<IndexEntry>(iconDefs.Count);
using var blobStream = new MemoryStream();

foreach (var icon in iconDefs)
{
    var svgText = File.ReadAllText(icon.Path, Encoding.UTF8);
    var compressed = CompressSvg(svgText);
    if (blobStream.Position > int.MaxValue)
    {
        Fail("Binary blob exceeds 2GB; offsets would overflow 32-bit index.");
    }

    var offset = (int)blobStream.Position;
    blobStream.Write(compressed, 0, compressed.Length);
    entries.Add(new IndexEntry(offset, compressed.Length));
}

var blobPath = Path.Combine(resourcesDir, "lucide_icons.bin");
File.WriteAllBytes(blobPath, blobStream.ToArray());

var enumPath = Path.Combine(generatedDir, "LucideIconKind.g.cs");
var aliasesPath = Path.Combine(generatedDir, "LucideAliases.g.cs");
var indexPath = Path.Combine(generatedDir, "LucideIconIndex.g.cs");

File.WriteAllText(enumPath, EmitEnum(iconDefs), Encoding.UTF8);
File.WriteAllText(aliasesPath, EmitAliases(iconDefs), Encoding.UTF8);
File.WriteAllText(indexPath, EmitIndex(iconDefs, entries), Encoding.UTF8);

var report = new GeneratorReport
{
    Input = inputPath,
    Output = outputRoot,
    Compression = ExpectedCompression,
    IconCount = iconDefs.Count,
    Icons = iconDefs.Select(icon => new GeneratorIcon
    {
        Name = icon.Name,
        Normalized = icon.Normalized
    }).ToArray()
};

var reportPath = Path.Combine(outputRoot, "generator-report.json");
var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(reportPath, reportJson, Encoding.UTF8);

Console.WriteLine($"Generated {iconDefs.Count} icons.");
Console.WriteLine($"- {enumPath}");
Console.WriteLine($"- {aliasesPath}");
Console.WriteLine($"- {indexPath}");
Console.WriteLine($"- {blobPath}");
Console.WriteLine($"- {reportPath}");

static Dictionary<string, string> ParseArgs(string[] args)
{
    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (!arg.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            map[arg] = args[i + 1];
            i++;
        }
        else
        {
            map[arg] = string.Empty;
        }
    }

    return map;
}

static string NormalizeName(string name)
{
    var parts = Regex.Split(name, "[^A-Za-z0-9]+")
        .Where(part => !string.IsNullOrWhiteSpace(part));

    var builder = new StringBuilder();
    foreach (var part in parts)
    {
        var span = part.AsSpan();
        if (span.Length == 0)
        {
            continue;
        }

        var first = span[0];
        if (builder.Length == 0 && !char.IsLetter(first) && first != '_')
        {
            builder.Append('N');
        }

        builder.Append(char.ToUpperInvariant(first));
        if (span.Length > 1)
        {
            builder.Append(span.Slice(1).ToString().ToLowerInvariant());
        }
    }

    var result = builder.Length == 0 ? "Icon" : builder.ToString();
    if (!char.IsLetter(result[0]) && result[0] != '_')
    {
        result = "N" + result;
    }

    return result;
}

static byte[] CompressSvg(string svgText)
{
    var bytes = Encoding.UTF8.GetBytes(svgText);
    using var memory = new MemoryStream();
    using (var gzip = new GZipStream(memory, CompressionLevel.Optimal, leaveOpen: true))
    {
        gzip.Write(bytes, 0, bytes.Length);
    }

    return memory.ToArray();
}

static string EmitEnum(IReadOnlyList<IconDefinition> icons)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated />");
    builder.AppendLine("namespace Lucide.NET;");
    builder.AppendLine();
    builder.AppendLine("public enum LucideIconKind");
    builder.AppendLine("{");
    for (var i = 0; i < icons.Count; i++)
    {
        builder.Append("    ");
        builder.Append(icons[i].Normalized);
        builder.Append(" = ");
        builder.Append(i.ToString());
        builder.AppendLine(",");
    }

    builder.AppendLine("}");
    return builder.ToString();
}

static string EmitAliases(IReadOnlyList<IconDefinition> icons)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated />");
    builder.AppendLine("using System;");
    builder.AppendLine("using System.Collections.Generic;");
    builder.AppendLine();
    builder.AppendLine("namespace Lucide.NET;");
    builder.AppendLine();
    builder.AppendLine("public static class LucideAliases");
    builder.AppendLine("{");
    builder.AppendLine("    public static readonly IReadOnlyDictionary<string, LucideIconKind> Map =");
    builder.AppendLine("        new Dictionary<string, LucideIconKind>(StringComparer.OrdinalIgnoreCase)");
    builder.AppendLine("        {");
    foreach (var icon in icons)
    {
        builder.AppendLine($"            [\"{icon.Name}\"] = LucideIconKind.{icon.Normalized},");
        if (!string.Equals(icon.Name, icon.Normalized, StringComparison.OrdinalIgnoreCase))
        {
            builder.AppendLine($"            [\"{icon.Normalized}\"] = LucideIconKind.{icon.Normalized},");
        }
    }

    builder.AppendLine("        };");
    builder.AppendLine("}");
    return builder.ToString();
}

static string EmitIndex(IReadOnlyList<IconDefinition> icons, IReadOnlyList<IndexEntry> entries)
{
    var builder = new StringBuilder();
    builder.AppendLine("// <auto-generated />");
    builder.AppendLine("namespace Lucide.NET;");
    builder.AppendLine();
    builder.AppendLine("public enum LucideCompression : byte");
    builder.AppendLine("{");
    builder.AppendLine("    None = 0,");
    builder.AppendLine("    Gzip = 1,");
    builder.AppendLine("}");
    builder.AppendLine();
    builder.AppendLine("public readonly struct LucideIconIndexEntry");
    builder.AppendLine("{");
    builder.AppendLine("    public LucideIconIndexEntry(int offset, int length, LucideCompression compression)");
    builder.AppendLine("    {");
    builder.AppendLine("        Offset = offset;");
    builder.AppendLine("        Length = length;");
    builder.AppendLine("        Compression = compression;");
    builder.AppendLine("    }");
    builder.AppendLine();
    builder.AppendLine("    public int Offset { get; }");
    builder.AppendLine("    public int Length { get; }");
    builder.AppendLine("    public LucideCompression Compression { get; }");
    builder.AppendLine("}");
    builder.AppendLine();
    builder.AppendLine("public static class LucideIconIndex");
    builder.AppendLine("{");
    builder.AppendLine("    public const LucideCompression Compression = LucideCompression.Gzip;");
    builder.AppendLine($"    public const int Count = {icons.Count};");
    builder.AppendLine("    public static readonly LucideIconIndexEntry[] Entries = new[]");
    builder.AppendLine("    {");
    for (var i = 0; i < entries.Count; i++)
    {
        var entry = entries[i];
        builder.AppendLine($"        new LucideIconIndexEntry({entry.Offset}, {entry.Length}, LucideCompression.Gzip),");
    }

    builder.AppendLine("    };");
    builder.AppendLine("}");
    return builder.ToString();
}

static void Fail(string message)
{
    Console.Error.WriteLine(message);
    Environment.Exit(1);
}

internal readonly record struct IconDefinition(string Name, string Normalized, string Path);

internal readonly record struct IndexEntry(int Offset, int Length);

internal sealed class GeneratorReport
{
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Compression { get; set; } = string.Empty;
    public int IconCount { get; set; }
    public GeneratorIcon[] Icons { get; set; } = Array.Empty<GeneratorIcon>();
}

internal sealed class GeneratorIcon
{
    public string Name { get; set; } = string.Empty;
    public string Normalized { get; set; } = string.Empty;
}
