namespace Bark.Services.MarkdownExtensions;

/// <summary>One highlighted run; hex colors for light/dark theme, both null when unhighlighted.</summary>
public readonly record struct SyntaxToken(string Text, string? LightColor, string? DarkColor);
