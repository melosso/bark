namespace Bark.Services.Theming.Themes;

/// <summary>White ground, green accent, flat outline icons. Used when none is configured.</summary>
public sealed class DefaultTheme : IBarkTheme
{
    public string Name => "default";

    public string Label => "Default";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#fafafa",
        ["--sidebar-bg"] = "#f2f4f2",
        ["--text-color"] = "#1a1d1f",
        // Spec's lighter #75807a reads 3.6:1 on white, under the floor for body copy.
        ["--text-muted"] = "#565f59",
        ["--accent"] = "#1f6b4a",
        ["--accent-light"] = "#eef1ee",
        ["--border"] = "#e2e7e2",
        ["--code-bg"] = "#f4f6f4",
        ["--search-bg"] = "#f4f6f4"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#0e100f",
        ["--sidebar-bg"] = "#141615",
        ["--text-color"] = "#e6e9e7",
        ["--text-muted"] = "#99a29c",
        ["--accent"] = "#4fb187",
        ["--accent-light"] = "#17211c",
        ["--border"] = "#262b28",
        ["--code-bg"] = "#151817",
        ["--search-bg"] = "#151817"
    };

    /// <summary>Empty: the base stylesheet is this theme.</summary>
    public string ComponentCss => string.Empty;
}
