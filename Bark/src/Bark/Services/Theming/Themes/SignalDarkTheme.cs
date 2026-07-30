namespace Bark.Services.Theming.Themes;

/// <summary>Deep charcoal, amber accent, divided feature row.</summary>
/// <remarks>Amber is a highlighter on paper; light mode uses bronze and never as a fill.</remarks>
public sealed class SignalDarkTheme : IBarkTheme
{
    public string Name => "signal-dark";

    public string Label => "Signal Dark";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#faf9f6",
        ["--sidebar-bg"] = "#f1f0ea",
        ["--text-color"] = "#1a1a15",
        ["--text-muted"] = "#5c5a4e",
        ["--accent"] = "#7d6114",
        ["--accent-light"] = "#f0ede1",
        ["--border"] = "#e2dfd4",
        ["--code-bg"] = "#f2f0e9"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#14140f",
        ["--sidebar-bg"] = "#1b1b14",
        ["--text-color"] = "#ebe7d6",
        ["--text-muted"] = "#9d9781",
        ["--accent"] = "#e0c65f",
        ["--accent-light"] = "#2a2617",
        ["--border"] = "#2f2d20",
        ["--code-bg"] = "#1c1b14"
    };

    public string ComponentCss => """
                .bark-features {
                    gap: 0;
                }
                .bark-feature {
                    padding: 2.5rem 1.5rem 0;
                    border-top: 0;
                    border-left: 1px solid var(--border);
                }
                .bark-feature:first-child {
                    border-left: 0;
                    padding-left: 0;
                }
                .bark-hero {
                    border-bottom: 1px solid var(--border);
                }
                .bark-hero-name {
                    font-family: var(--font-mono);
                    letter-spacing: -0.01em;
                }
        """;
}
