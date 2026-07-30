using Bark.Services.Theming.Themes;

namespace Bark.Services.Theming;

/// <summary>Built-in themes. To add your own, implement <see cref="IBarkTheme"/> and add a line to <see cref="All"/>.</summary>
public static class ThemeRegistry
{
    public static IReadOnlyList<IBarkTheme> All { get; } =
    [
        new DefaultTheme(),
        new ForestLedgerTheme(),
        new SignalDarkTheme(),
        new BlueprintGridTheme(),
        new OceanTheme(),
        new DeepSpaceTheme(),
        new SolarizedTheme(),
        new LaserwaveTheme(),
        new LimelightTheme()
    ];

    public static IBarkTheme Default { get; } = All[0];

    /// <summary>Unknown names warn and fall back to the default; a typo must never take a site down.</summary>
    public static IBarkTheme Resolve(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Default;

        var trimmed = name.Trim();
        foreach (var theme in All)
        {
            if (string.Equals(theme.Name, trimmed, StringComparison.OrdinalIgnoreCase))
                return theme;
        }

        Serilog.Log.Warning(
            "Unknown theme {Theme}; falling back to {Default}. Available: {Available}",
            trimmed, Default.Name, string.Join(", ", All.Select(t => t.Name)));
        return Default;
    }
}
