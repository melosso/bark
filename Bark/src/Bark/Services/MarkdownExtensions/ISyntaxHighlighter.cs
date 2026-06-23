namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// Tokenizes whole code blocks for syntax highlighting. A full block (not single lines) is
/// passed in because grammar-based tokenizers carry state across lines (e.g. multi-line
/// comments/strings), so lines cannot be tokenized independently. Implementations must never
/// throw for an unsupported or unrecognized language -- return one plain <see cref="SyntaxToken"/>
/// per line (both colors <c>null</c>) instead, so <see cref="BarkCodeBlockRenderer"/> always has
/// a safe fallback to plain-escaped-text rendering.
/// </summary>
public interface ISyntaxHighlighter
{
    /// <summary>
    /// Performs one-time setup (e.g. loading grammars/themes). Called once at application
    /// startup, never per-request or per-rebuild.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Theme metadata for the `&lt;pre&gt;` wrapper. <c>null</c> means "no real highlighting" --
    /// the renderer falls back to a plain `&lt;pre&gt;&lt;code&gt;` with no theme attributes.
    /// </summary>
    SyntaxTheme? Theme { get; }

    /// <summary>
    /// Tokenizes <paramref name="lines"/> (in source order) for <paramref name="lang"/>. Must
    /// return exactly one entry per input line, each covering that line's full text; must not
    /// throw.
    /// </summary>
    IReadOnlyList<IReadOnlyList<SyntaxToken>> TokenizeLines(IReadOnlyList<string> lines, string lang);
}
