namespace Bark.Services.Theming.Themes;

/// <summary>Warm paper, forest green, numbered rule-lines.</summary>
public sealed class ForestLedgerTheme : IBarkTheme
{
    public string Name => "forest-ledger";

    public string Label => "Forest Ledger";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#faf7f0",
        ["--sidebar-bg"] = "#f3efe4",
        ["--text-color"] = "#221e17",
        ["--text-muted"] = "#655c4e",
        ["--accent"] = "#3f5d3c",
        ["--accent-light"] = "#e6ece0",
        ["--border"] = "#ddd4c2",
        ["--code-bg"] = "#f2ecdf"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#1a1712",
        ["--sidebar-bg"] = "#221e17",
        ["--text-color"] = "#ece5d7",
        ["--text-muted"] = "#a39885",
        ["--accent"] = "#8fb07f",
        ["--accent-light"] = "#26301f",
        ["--border"] = "#332d23",
        ["--code-bg"] = "#221d16"
    };

    public string ComponentCss => """
                .bark-features {
                    display: block;
                    border: 0;
                    border-radius: 0;
                    counter-reset: bark-feature;
                }
                .bark-feature {
                    display: grid;
                    grid-template-columns: 2.5rem 1fr;
                    gap: 0 1.5rem;
                    padding: 1.75rem 0;
                    border: 0;
                    border-top: 1px solid var(--border);
                }
                .bark-feature:first-child {
                    border-top: 0;
                }
                .bark-feature-title,
                .bark-feature-details {
                    grid-column: 2;
                }
                .bark-feature::before {
                    counter-increment: bark-feature;
                    content: counter(bark-feature, decimal-leading-zero);
                    font-family: var(--font-mono);
                    font-size: 0.8rem;
                    color: var(--accent);
                    padding-top: 0.35rem;
                }
                .bark-feature-icon {
                    display: none;
                }
                .bark-feature-title {
                    font-size: 1.15rem;
                }
        """;
}
