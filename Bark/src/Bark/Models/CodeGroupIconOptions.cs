namespace Bark.Models;

/// <summary>Bound from `Docs:CodeGroupIcons` (appsettings.json), not bark.json -- pipeline is built once at startup.</summary>
public sealed record CodeGroupIconOptions
{
    public bool Enabled { get; init; } = true;

    public string BaseUrl { get; init; } = "https://cdn.jsdelivr.net/npm/simple-icons@13/icons";

    public string Format { get; init; } = "svg";

    public Dictionary<string, string>? Overrides { get; init; }
}
