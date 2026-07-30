namespace Bark.Services.Theming.Themes;

/// <summary>Mint tints, bordered feature grid, tinted icon chips.</summary>
public sealed class BlueprintGridTheme : IBarkTheme
{
    public string Name => "blueprint-grid";

    public string Label => "Blueprint Grid";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#ffffff",
        ["--sidebar-bg"] = "#f7f9f8",
        ["--text-color"] = "#101a16",
        ["--text-muted"] = "#55645c",
        ["--accent"] = "#1f5f52",
        ["--accent-light"] = "#eef1ee",
        ["--border"] = "#dbe3df",
        ["--code-bg"] = "#f4f7f6"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#0f1412",
        ["--sidebar-bg"] = "#151b19",
        ["--text-color"] = "#e3ebe7",
        ["--text-muted"] = "#90a099",
        ["--accent"] = "#5fbfa6",
        ["--accent-light"] = "#17221f",
        ["--border"] = "#232d29",
        ["--code-bg"] = "#131a18"
    };

    public string ComponentCss => """
                .bark-features {
                    gap: 0;
                    border: 1px solid var(--border);
                    border-radius: 12px;
                    overflow: hidden;
                }
                .bark-feature {
                    display: block;
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
                .bark-feature-details {
                    margin-top: 0.4rem;
                }
                .bark-feature-icon {
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    width: 2.5rem;
                    height: 2.5rem;
                    border-radius: 8px;
                    background-color: var(--accent-light);
                    color: var(--accent);
                    font-size: 1.15rem;
                    margin-bottom: 1rem;
                }
                .bark-feature-icon svg,
                .bark-feature-icon img {
                    width: 1.25rem;
                    height: 1.25rem;
                    stroke: currentColor;
                    fill: none;
                }
        """;
}
