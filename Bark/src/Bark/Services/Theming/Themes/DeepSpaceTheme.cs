namespace Bark.Services.Theming.Themes;

/// <summary>Near-black navy, periwinkle accent. Darkest built-in.</summary>
public sealed class DeepSpaceTheme : IBarkTheme
{
    public string Name => "deep-space";

    public string Label => "Deep Space";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#f7f8fc",
        ["--sidebar-bg"] = "#eceff8",
        ["--text-color"] = "#131832",
        ["--text-muted"] = "#4d5578",
        ["--accent"] = "#2f4fa8",
        ["--accent-light"] = "#e6eaf7",
        ["--border"] = "#d5dbee",
        ["--code-bg"] = "#eef1f9"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#070b1a",
        ["--sidebar-bg"] = "#0d1327",
        ["--text-color"] = "#dbe2f5",
        ["--text-muted"] = "#8b95bb",
        ["--accent"] = "#7aa2f7",
        ["--accent-light"] = "#141c38",
        ["--border"] = "#202a4a",
        ["--code-bg"] = "#0a1022"
    };

    public string ComponentCss => """
                .bark-features {
                    gap: 0;
                    border: 1px solid var(--border);
                    border-radius: 14px;
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
                .bark-feature-icon {
                    color: var(--accent);
                }
                .bark-hero-name {
                    color: var(--accent);
                    letter-spacing: 0.18em;
                }
        """;
}
