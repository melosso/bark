namespace Bark.Services.Theming.Themes;

/// <summary>Solarized base tones: warm paper, deep teal night.</summary>
/// <remarks>Grounds and dark mode are canonical. Light text/accent are not: base00-02 either miss 4.5:1 or read blue.</remarks>
public sealed class SolarizedTheme : IBarkTheme
{
    public string Name => "solarized";

    public string Label => "Solarized";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#fdf6e3",
        ["--sidebar-bg"] = "#eee8d5",
        ["--text-color"] = "#3a372f",
        ["--text-muted"] = "#5f5a4d",
        ["--accent"] = "#0f6795",
        ["--accent-light"] = "#e9e2cd",
        ["--border"] = "#ddd6c1",
        ["--code-bg"] = "#eee8d5"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#002b36",
        ["--sidebar-bg"] = "#073642",
        ["--text-color"] = "#d6e2e2",
        ["--text-muted"] = "#8fa3a3",
        ["--accent"] = "#4aa3dc",
        ["--accent-light"] = "#0a3a47",
        ["--border"] = "#12414e",
        ["--code-bg"] = "#04303c"
    };

    public string ComponentCss => """
                .bark-features {
                    gap: 0;
                    border: 1px solid var(--border);
                    border-radius: 4px;
                    overflow: hidden;
                }
                .bark-feature {
                    padding: 1.75rem;
                    border-top: 0;
                    border-left: 1px solid var(--border);
                }
                .bark-feature:first-child {
                    border-left: 0;
                }
                a.bark-feature:hover {
                    background-color: var(--accent-light);
                }
                .bark-hero-action.brand {
                    border-radius: 4px;
                }
                .bark-hero-action.alt {
                    border-radius: 4px;
                }
        """;
}
