using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Lucide.NET;

public sealed class EmbeddedLucideIconSource : ILucideIconSource
{
    private static readonly Lazy<byte[]> Blob = new(LoadBlob);
    private readonly ConcurrentDictionary<LucideIconKind, string> _cache = new();

    public string GetSvg(LucideIconKind kind)
    {
        if (_cache.TryGetValue(kind, out var cached))
        {
            return cached;
        }

        var svg = LoadSvg(kind);
        _cache.TryAdd(kind, svg);
        return svg;
    }

    private static string LoadSvg(LucideIconKind kind)
    {
        var index = (int)kind;
        if (index < 0 || index >= LucideIconIndex.Entries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), $"Unknown icon kind: {kind}");
        }

        var entry = LucideIconIndex.Entries[index];
        if (entry.Compression != LucideCompression.Gzip)
        {
            throw new NotSupportedException($"Unsupported compression: {entry.Compression}");
        }

        var blob = Blob.Value;
        if (entry.Offset < 0 || entry.Length < 0 || entry.Offset + entry.Length > blob.Length)
        {
            throw new InvalidOperationException("Lucide icon index is out of range for the embedded blob.");
        }

        using var sourceStream = new MemoryStream(blob, entry.Offset, entry.Length, writable: false);
        using var gzip = new GZipStream(sourceStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        return reader.ReadToEnd();
    }

    private static byte[] LoadBlob()
    {
        var assembly = typeof(EmbeddedLucideIconSource).GetTypeInfo().Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("lucide_icons.bin", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Embedded resource 'lucide_icons.bin' was not found. Run the generator to produce icon data.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException("Failed to open embedded resource 'lucide_icons.bin'.");
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
