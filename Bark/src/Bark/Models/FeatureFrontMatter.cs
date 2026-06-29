namespace Bark.Models;

public sealed record FeatureFrontMatter
{
    public string? Icon { get; init; }
    public FeatureIconConfig? IconImage { get; init; }
    public string? Title { get; init; }
    public string? Details { get; init; }
    public string? Link { get; init; }
}
