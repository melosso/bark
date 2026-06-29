namespace Bark.Models;

public sealed record FeatureIconConfig
{
    public string? Src { get; init; }
    public string? Light { get; init; }
    public string? Dark { get; init; }
    public string? Alt { get; init; }
}
