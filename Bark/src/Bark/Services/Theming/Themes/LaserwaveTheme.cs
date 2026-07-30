namespace Bark.Services.Theming.Themes;

/// <summary>Synthwave violet, hot magenta accent.</summary>
/// <remarks>Full magenta only holds on dark; light mode uses deep berry for contrast.</remarks>
public sealed class LaserwaveTheme : IBarkTheme
{
    public string Name => "laserwave";

    public string Label => "Laserwave";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#faf7fb",
        ["--sidebar-bg"] = "#f2ecf5",
        ["--text-color"] = "#241d2b",
        ["--text-muted"] = "#5f5369",
        ["--accent"] = "#a3186e",
        ["--accent-light"] = "#f4e7f0",
        ["--border"] = "#e3d9e9",
        ["--code-bg"] = "#f4eff7"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#1e1a24",
        ["--sidebar-bg"] = "#27212e",
        ["--text-color"] = "#e6e2e8",
        ["--text-muted"] = "#a599b0",
        ["--accent"] = "#eb64b9",
        ["--accent-light"] = "#33263a",
        ["--border"] = "#3b3145",
        ["--code-bg"] = "#241f2b"
    };

    public string ComponentCss => """
                .bark-feature {
                    border-top-width: 2px;
                    border-top-color: var(--accent);
                    padding-top: 1.5rem;
                }
                .bark-feature-icon {
                    color: var(--accent);
                }
                .bark-feature-title {
                    letter-spacing: -0.01em;
                }
                .bark-hero-name {
                    color: var(--accent);
                    letter-spacing: 0.2em;
                }
                .bark-hero-text {
                    letter-spacing: -0.04em;
                }
        """;
}
