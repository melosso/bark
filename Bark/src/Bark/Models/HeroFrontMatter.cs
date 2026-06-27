namespace Bark.Models;

/// <summary><c>image</c> and feature <c>icon</c> are plain strings: a URL or an emoji.</summary>
public sealed record HeroFrontMatter
{
    public string? Name { get; init; }
    public string? Text { get; init; }
    public string? Tagline { get; init; }
    public string? Image { get; init; }
    public List<HeroActionFrontMatter>? Actions { get; init; }
}
