namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// Theme metadata for the `&lt;pre class="shiki shiki-themes {light} {dark}"&gt;` wrapper that
/// <see cref="BarkCodeBlockRenderer"/> emits when a real <see cref="ISyntaxHighlighter"/> (e.g.
/// <see cref="TextMateSyntaxHighlighter"/>) is active. <see cref="ISyntaxHighlighter.Theme"/>
/// returns <c>null</c> for <see cref="NullSyntaxHighlighter"/>, which keeps the renderer on the
/// plain `&lt;pre&gt;&lt;code&gt;` path with no theme attributes.
/// </summary>
public readonly record struct SyntaxTheme(
    string LightName,
    string DarkName,
    string LightForeground,
    string DarkForeground,
    string LightBackground,
    string DarkBackground);
