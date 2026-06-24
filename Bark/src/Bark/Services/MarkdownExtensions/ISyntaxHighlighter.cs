namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// Tokenizes whole blocks (not single lines, since grammars carry state across lines) for syntax
/// highlighting. Must never throw -- return one plain <see cref="SyntaxToken"/> per line instead.
/// </summary>
public interface ISyntaxHighlighter
{
    /// <summary>One-time setup (e.g. loading grammars/themes), called once at startup.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Theme metadata for the `&lt;pre&gt;` wrapper; null means no real highlighting.</summary>
    SyntaxTheme? Theme { get; }

    /// <summary>Tokenizes <paramref name="lines"/> for <paramref name="lang"/>; must not throw.</summary>
    IReadOnlyList<IReadOnlyList<SyntaxToken>> TokenizeLines(IReadOnlyList<string> lines, string lang);
}
