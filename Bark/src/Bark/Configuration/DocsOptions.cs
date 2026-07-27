namespace Bark.Configuration;

public sealed record DocsOptions
{
    public string RootPath { get; init; } = "docs";
    public string? DefaultPage { get; init; } = "index";
    public bool EnableHotReload { get; init; } = true;
    public string? BasePath { get; init; }

    /// <summary>
    /// Public origin (e.g. <c>https://docs.example.com</c>) for canonical URLs, feeds and robots.txt.
    /// Leave unset only for local use: without it those URLs are built from the caller-supplied Host header.
    /// </summary>
    public string? PublicBaseUrl { get; init; }

    // Static export: pages load a prebuilt search index instead of /api/search.
    public bool IsStaticExport { get; init; }
}
