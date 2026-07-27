using Bark.Services.Layout;

namespace Bark.Services.Rendering;

public static class UrlPaths
{
    /// <summary>
    /// True when a configured nav/sidebar target is an absolute web URL rather than a docs page path.
    /// Deliberately limited to http/https: any other scheme falls through to <see cref="Href"/> and is
    /// rewritten as a relative path, so <c>javascript:</c> targets stay inert.
    /// </summary>
    public static bool IsExternal(string? link) =>
        link is not null
        && (link.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || link.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

    public static string Href(string basePath, string path)
    {
        var trimmed = path.Trim('/');
        return trimmed.Length == 0
            ? (basePath.Length == 0 ? "/" : $"{basePath}/")
            : $"{basePath}/{LayoutProvider.HtmlEncode(trimmed)}/";
    }
}
