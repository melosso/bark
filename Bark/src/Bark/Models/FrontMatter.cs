namespace Bark.Models;

public sealed record FrontMatter
{
    public string? Title { get; init; }
    public string? Description { get; init; }
}
