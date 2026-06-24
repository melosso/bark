namespace Bark.Services.MarkdownExtensions;

/// <summary>Theme metadata for the highlighted `&lt;pre&gt;` wrapper; null when no real highlighter is active.</summary>
public readonly record struct SyntaxTheme(
    string LightName,
    string DarkName,
    string LightForeground,
    string DarkForeground,
    string LightBackground,
    string DarkBackground);
