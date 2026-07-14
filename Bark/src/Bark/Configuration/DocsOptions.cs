namespace Bark.Configuration;

public sealed record DocsOptions
{
    public string RootPath { get; init; } = "docs";
    public string? DefaultPage { get; init; } = "index";
    public bool EnableHotReload { get; init; } = true;
    public string? BasePath { get; init; }

    // Static export: pages load a prebuilt search index instead of /api/search.
    public bool IsStaticExport { get; init; }
}
