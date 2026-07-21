using System.Text.Json;
using System.Text.Json.Serialization;
using Bark.Models;

namespace Bark.Services.Rendering;

public static class StructuredDataRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string BuildJsonLd(
        string? canonicalUrl,
        string title,
        string? description,
        bool isHomePage,
        string origin,
        string basePath,
        IReadOnlyList<BreadcrumbItem> crumbs,
        string? imageUrl = null,
        string? siteName = null,
        DateTime? modified = null,
        string? nonce = null)
    {
        if (string.IsNullOrEmpty(canonicalUrl))
            return string.Empty;

        var graph = new List<object>();

        if (isHomePage)
        {
            graph.Add(new Dictionary<string, object?>
            {
                ["@type"] = "WebSite",
                ["name"] = siteName ?? title,
                ["url"] = canonicalUrl,
                ["description"] = description
            });
        }
        else
        {
            var iso = modified?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            var article = new Dictionary<string, object?>
            {
                ["@type"] = "Article",
                ["headline"] = title,
                ["description"] = description,
                ["mainEntityOfPage"] = canonicalUrl,
                ["image"] = imageUrl,
                ["datePublished"] = iso,
                ["dateModified"] = iso,
                ["publisher"] = string.IsNullOrEmpty(siteName)
                    ? null
                    : new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = siteName }
            };
            graph.Add(article);
        }

        if (crumbs.Count > 1)
        {
            var items = new List<object>(crumbs.Count);
            for (var i = 0; i < crumbs.Count; i++)
            {
                var crumb = crumbs[i];
                var url = i == crumbs.Count - 1
                    ? canonicalUrl
                    : crumb.Path is { Length: > 0 } p
                        ? $"{origin}{basePath}{(p.StartsWith('/') ? p : "/" + p)}"
                        : null;

                items.Add(new Dictionary<string, object?>
                {
                    ["@type"] = "ListItem",
                    ["position"] = i + 1,
                    ["name"] = crumb.Title,
                    ["item"] = url
                });
            }

            graph.Add(new Dictionary<string, object?>
            {
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = items
            });
        }

        if (graph.Count == 0)
            return string.Empty;

        var doc = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = graph
        };

        var json = JsonSerializer.Serialize(doc, JsonOptions);
        var nonceAttr = nonce is { Length: > 0 } ? $" nonce=\"{nonce}\"" : string.Empty;
        return $"    <script type=\"application/ld+json\"{nonceAttr}>{json}</script>\n";
    }
}
