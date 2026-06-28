using System.Text.RegularExpressions;

namespace Bark.Services.Rendering;

public static partial class IconProvider
{
    private static readonly Dictionary<string, string?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public static string InlineSvg(string iconName, string iconsDir)
    {
        if (_cache.TryGetValue(iconName, out var cached))
            return cached ?? string.Empty;

        var slug = Slugify(iconName);
        var filePath = Path.Combine(iconsDir, $"{slug}.svg");

        if (!File.Exists(filePath))
        {
            _cache[iconName] = null;
            return string.Empty;
        }

        var svg = File.ReadAllText(filePath);
        svg = StripFillAttr().Replace(svg, "");
        svg = svg.Replace("<svg", "<svg fill=\"currentColor\" aria-hidden=\"true\"");
        _cache[iconName] = svg;
        return svg;
    }

    public static void ClearCache() => _cache.Clear();

    private static string Slugify(string name) =>
        SlugRegex().Replace(name.ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex(@"\s+fill=""[^""]*""")]
    private static partial Regex StripFillAttr();

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex SlugRegex();
}
