namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// A single highlighted text run within a code line. <see cref="LightColor"/>/<see cref="DarkColor"/>
/// are hex colors (e.g. <c>#abcdef</c>) for the light/dark theme, mirroring Shiki's dual-theme
/// output (<c>--shiki-light</c>/<c>--shiki-dark</c> CSS vars). Both are <c>null</c> for plain,
/// unhighlighted text (e.g. when no tokenizer is available for the block's language).
/// </summary>
public readonly record struct SyntaxToken(string Text, string? LightColor, string? DarkColor);
