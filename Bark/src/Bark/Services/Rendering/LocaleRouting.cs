using Bark.Models;

namespace Bark.Services.Rendering;

public sealed record LocaleRoute(string Code, string Prefix, string ContentPath, string? FallbackPath);

public static class LocaleRouting
{
    public const string RootKey = "root";

    public static string RootCode(Config? config) => Config.ResolveLocale(config)?.Code ?? "en";

    public static IReadOnlyList<string> TreeCodes(Config? config) =>
        config?.Locales is null
            ? []
            : config.Locales.Keys
                .Where(key => !key.Equals(RootKey, StringComparison.OrdinalIgnoreCase))
                .Select(key => key.ToLowerInvariant())
                .Order()
                .ToArray();

    public static string LocaleOf(string path, IReadOnlyList<string> localeCodes, string rootLocale)
    {
        var slash = path.IndexOf('/');
        var head = slash < 0 ? path : path[..slash];
        return localeCodes.Contains(head, StringComparer.OrdinalIgnoreCase) ? head.ToLowerInvariant() : rootLocale;
    }

    public static LocaleEntry? EntryOf(Config? config, string code)
    {
        if (config?.Locales is null)
            return null;

        var key = code == RootCode(config) ? RootKey : code;
        return config.Locales.TryGetValue(key, out var entry) ? entry : null;
    }

    public static string LangOf(Config? config, string code) =>
        EntryOf(config, code)?.Lang is { Length: > 0 } lang ? lang : code;

    public static string LabelOf(Config? config, string code) =>
        EntryOf(config, code)?.Label is { Length: > 0 } label ? label : code;

    public static LocaleRoute Resolve(Config? config, string path)
    {
        var trimmed = path.Trim('/');
        var rootCode = RootCode(config);
        var trees = TreeCodes(config);
        if (trees.Count == 0)
            return new LocaleRoute(rootCode, string.Empty, trimmed, null);

        var firstSlash = trimmed.IndexOf('/');
        var firstSegment = (firstSlash < 0 ? trimmed : trimmed[..firstSlash]).ToLowerInvariant();
        if (firstSegment.Length == 0 || !trees.Contains(firstSegment))
            return new LocaleRoute(rootCode, string.Empty, trimmed, null);

        var rest = firstSlash < 0 ? string.Empty : trimmed[(firstSlash + 1)..];
        var content = trimmed;
        var fallback = rest.Length == 0 ? "index" : rest;

        return new LocaleRoute(firstSegment, firstSegment, content, fallback);
    }

    public static string Localize(string prefix, string path)
    {
        var trimmed = path.Trim('/');
        if (prefix.Length == 0)
            return trimmed;

        if (trimmed.Length == 0 || trimmed.Equals("index", StringComparison.OrdinalIgnoreCase))
            return prefix;

        var alreadyLocalized = trimmed.Equals(prefix, StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith($"{prefix}/", StringComparison.OrdinalIgnoreCase);

        return alreadyLocalized ? trimmed : $"{prefix}/{trimmed}";
    }

    public static string Delocalize(string prefix, string path)
    {
        var trimmed = path.Trim('/');
        if (prefix.Length == 0 || !trimmed.StartsWith(prefix, StringComparison.Ordinal))
            return trimmed;

        var rest = trimmed[prefix.Length..].TrimStart('/');
        return rest.Length == 0 ? "index" : rest;
    }
}
