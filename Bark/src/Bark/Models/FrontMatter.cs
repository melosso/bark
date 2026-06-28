namespace Bark.Models;

public sealed record FrontMatter
{
    public string? Title { get; init; }
    public string? Description { get; init; }

    /// <summary>Set to <c>"home"</c> to render <see cref="Hero"/>/<see cref="Features"/> instead of standard docs chrome.</summary>
    public string? Layout { get; init; }

    public HeroFrontMatter? Hero { get; init; }
    public List<FeatureFrontMatter>? Features { get; init; }

    public List<string>? Keywords { get; init; }

    /// <summary>Per-page override for <c>Config.LastUpdated</c>. <c>false</c> hides the
    /// "Last updated" stamp on this page even when the site-wide setting is on.</summary>
    public bool? LastUpdated { get; init; }

    /// <summary>Set to <c>false</c> to hide prev/next pagination links on this page.</summary>
    public bool? Pagination { get; init; }

    /// <summary>When set, the page issues a 307 redirect to this URL instead of rendering.
    /// Root-relative paths (starting with <c>/</c>) are prefixed with the configured base path.
    /// Absolute URLs are used as-is.</summary>
    public string? Redirect { get; init; }

    /// <summary>Content creation date (ISO 8601). Used as the "Last updated" display value
    /// when <see cref="Updated"/> is absent. Overrides file system mtime.</summary>
    public DateTime? Date { get; init; }

    /// <summary>Last-modified date (ISO 8601). Takes priority over <see cref="Date"/> and
    /// file system mtime for the "Last updated" display.</summary>
    public DateTime? Updated { get; init; }
}
