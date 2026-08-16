namespace Bark.Models;

public sealed record PageControlsConfig
{
    public bool DownloadMarkdown { get; init; }
    public bool SubscribeRss { get; init; }
    public PageControlsEditLinkConfig? EditLink { get; init; }
}

public sealed record PageControlsEditLinkConfig
{
    public string Label { get; init; } = "Edit this page";
    /// <summary>Optional inline SVG string. If null, falls back to a generic external-link icon.</summary>
    public string? Icon { get; init; }
}
