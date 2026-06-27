namespace Bark.Models;

public sealed record FrontMatter
{
    public string? Title { get; init; }
    public string? Description { get; init; }

    /// <summary>Set to <c>"home"</c> to render <see cref="Hero"/>/<see cref="Features"/> instead of standard docs chrome.</summary>
    public string? Layout { get; init; }

    public HeroFrontMatter? Hero { get; init; }
    public List<FeatureFrontMatter>? Features { get; init; }

    /// <summary>Per-page override for <c>Config.LastUpdated</c>. <c>false</c> hides the
    /// "Last updated" stamp on this page even when the site-wide setting is on.</summary>
    public bool? LastUpdated { get; init; }
}
