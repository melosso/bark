namespace Bark.Services.Theming.Themes;

/// <summary>Pale off-white ground, sage-lime accent with a cyan counterpart, squared corners.</summary>
public sealed class LimelightTheme : IBarkTheme
{
    public string Name => "limelight";

    public string Label => "Limelight";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#f7f8f4",
        ["--sidebar-bg"] = "#eff2e9",
        ["--text-color"] = "#1d201b",
        ["--text-muted"] = "#5b6155",
        ["--accent"] = "#42700e",
        ["--accent-alt"] = "#0b6a86",
        ["--accent-light"] = "#e8eedb",
        ["--border"] = "#dee3d3",
        ["--code-bg"] = "#f1f4ec"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#12140f",
        ["--sidebar-bg"] = "#191c15",
        ["--text-color"] = "#dde1d6",
        ["--text-muted"] = "#969c8c",
        ["--accent"] = "#9fbe3f",
        ["--accent-alt"] = "#67b9d8",
        ["--accent-light"] = "#1f2618",
        ["--border"] = "#292e21",
        ["--code-bg"] = "#181b14"
    };

    public string ComponentCss => """
                .bark-features {
                    gap: 0;
                    border-top: 1px solid var(--border);
                }
                .bark-feature {
                    padding: 1.75rem 1.5rem;
                    border-top: 0;
                    border-left: 1px solid var(--border);
                    border-radius: 0;
                }
                .bark-feature:first-child {
                    border-left: 0;
                    padding-left: 0;
                }
                a.bark-feature:hover {
                    background-color: var(--accent-light);
                }
                .bark-feature-icon {
                    color: var(--accent);
                    border-radius: 0;
                }
                .bark-feature-title {
                    letter-spacing: -0.02em;
                }
                /* The cyan-to-lime pair reads as one mark rather than two accents. */
                .bark-hero {
                    border-bottom: 2px solid transparent;
                    border-image: linear-gradient(90deg, var(--accent-alt), var(--accent)) 1;
                }
                .bark-hero-name {
                    text-transform: uppercase;
                    letter-spacing: 0.12em;
                    color: var(--accent);
                }
                #scroll-indicator {
                    background: linear-gradient(90deg, var(--accent-alt), var(--accent));
                }
                .bark-hero-action.brand,
                .bark-hero-action.alt {
                    border-radius: 0;
                }
        """;
}
