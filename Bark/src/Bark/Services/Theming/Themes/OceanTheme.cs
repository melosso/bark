namespace Bark.Services.Theming.Themes;

/// <summary>Cool blue-grey paper, deep harbour accent, tinted feature band.</summary>
public sealed class OceanTheme : IBarkTheme
{
    public string Name => "ocean";

    public string Label => "Ocean";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#f6fafc",
        ["--sidebar-bg"] = "#e9f1f6",
        ["--text-color"] = "#0d2130",
        ["--text-muted"] = "#48626f",
        ["--accent"] = "#0a6382",
        ["--accent-light"] = "#e0edf4",
        ["--border"] = "#cfe0ea",
        ["--code-bg"] = "#edf4f8"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#0a1620",
        ["--sidebar-bg"] = "#0f1e2b",
        ["--text-color"] = "#dfeaf2",
        ["--text-muted"] = "#8ba6b9",
        ["--accent"] = "#4fb3d9",
        ["--accent-light"] = "#12293a",
        ["--border"] = "#1d3242",
        ["--code-bg"] = "#0d1c28"
    };

    public string ComponentCss => """
                .bark-features {
                    gap: 0;
                    background-color: var(--accent-light);
                    border-radius: 10px;
                    overflow: hidden;
                }
                .bark-feature {
                    padding: 1.75rem;
                    border-top: 0;
                }
                a.bark-feature:hover {
                    background-color: var(--sidebar-bg);
                }
                .bark-feature-icon {
                    color: var(--accent);
                }
                .bark-hero-name {
                    color: var(--accent);
                }
        """;
}
