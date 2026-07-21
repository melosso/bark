using System.Text.Json;
using Bark.Models;

namespace Bark.Services.Extensions;

public static partial class ExtensionLoader
{
    public const string FileName = "extensions.json";

    private static readonly char[] UnsafeUrlChars = ['\'', '"', '<', '>', '\\', ' '];

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static ExtensionSet Load(string docsPath, ILogger logger)
    {
        var path = Path.Combine(docsPath, FileName);
        if (!File.Exists(path))
            return ExtensionSet.Empty;

        ExtensionsFile? file;
        try
        {
            file = JsonSerializer.Deserialize<ExtensionsFile>(File.ReadAllText(path), JsonOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "{File} could not be read; all extensions stay disabled", FileName);
            return ExtensionSet.Empty;
        }

        if (file?.Extensions is not { } section)
            return ExtensionSet.Empty;

        var rejects = new Rejections(logger);

        var active = new List<ActiveExtension>();
        AddIfVerified(active, BuildMatomo(section.Matomo, rejects));
        AddIfVerified(active, BuildPlausible(section.Plausible, rejects));
        AddIfVerified(active, BuildMedama(section.Medama, rejects));
        AddIfVerified(active, BuildGoatCounter(section.GoatCounter, rejects));
        AddIfVerified(active, BuildLiwan(section.Liwan, rejects));

        return new ExtensionSet(active, rejects.Names);
    }

    private static void AddIfVerified(List<ActiveExtension> active, ActiveExtension? extension)
    {
        if (extension is not null)
            active.Add(extension);
    }

    private sealed class Rejections(ILogger logger)
    {
        private readonly List<string> _names = [];

        public IReadOnlyList<string> Names => _names;

        public void Add(string name, string reason)
        {
            logger.LogWarning("Extension {Extension} is enabled but was not activated: {Reason}", name, reason);
            _names.Add(name);
        }
    }

    private static ActiveExtension? Reject(string name, string reason, Rejections rejects)
    {
        rejects.Add(name, reason);
        return null;
    }

    internal static bool TryBaseUrl(string? raw, out string baseUrl, out string origin)
    {
        baseUrl = origin = string.Empty;

        var trimmed = Coalesce(raw);
        if (trimmed is null || trimmed.IndexOfAny(UnsafeUrlChars) >= 0)
            return false;

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme is not ("http" or "https"))
            return false;

        if (uri.UserInfo.Length > 0 || uri.Query.Length > 0 || uri.Fragment.Length > 0)
            return false;

        origin = uri.GetLeftPart(UriPartial.Authority);
        baseUrl = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        return baseUrl.Length > 0;
    }

    private static string? Coalesce(params string?[] values)
    {
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        return null;
    }
}
