namespace Bark.Models;

public sealed record FrontMatter
{
    public string? Title { get; init; }
    public string? Description { get; init; }

    /// <summary>Set to <c>"home"</c> to render <see cref="Hero"/>/<see cref="Features"/> instead of standard docs chrome.</summary>
    public string? Layout { get; init; }

    public HeroFrontMatter? Hero { get; init; }
    public List<FeatureFrontMatter>? Features { get; init; }

    /// <summary>Per-page override for <c>BarkConfig.LastUpdated</c>. <c>false</c> hides the
    /// "Last updated" stamp on this page even when the site-wide setting is on.</summary>
    public bool? LastUpdated { get; init; }
}

/// <summary><c>image</c> and feature <c>icon</c> are plain strings: a URL or an emoji.</summary>
public sealed record HeroFrontMatter
{
    public string? Name { get; init; }
    public string? Text { get; init; }
    public string? Tagline { get; init; }
    public string? Image { get; init; }
    public List<HeroActionFrontMatter>? Actions { get; init; }
}

public sealed record HeroActionFrontMatter
{
    /// <summary><c>"brand"</c> (filled) or <c>"alt"</c> (outline). Defaults to <c>"brand"</c>.</summary>
    public string? Theme { get; init; }
    public string? Text { get; init; }
    public string? Link { get; init; }
}

public sealed record FeatureFrontMatter
{
    public string? Icon { get; init; }
    public string? Title { get; init; }
    public string? Details { get; init; }
    public string? Link { get; init; }
}
