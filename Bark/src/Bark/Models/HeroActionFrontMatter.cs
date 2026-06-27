namespace Bark.Models;

public sealed record HeroActionFrontMatter
{
    /// <summary><c>"brand"</c> (filled) or <c>"alt"</c> (outline). Defaults to <c>"brand"</c>.</summary>
    public string? Theme { get; init; }
    public string? Text { get; init; }
    public string? Link { get; init; }
}
