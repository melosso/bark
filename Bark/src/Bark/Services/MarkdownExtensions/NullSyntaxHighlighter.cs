using System.Linq;

namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// Default <see cref="ISyntaxHighlighter"/>: no tokenization, each line comes back as one plain
/// token. This is today's baseline behavior (escaped text, no token-level color) and is also the
/// fallback every other implementation must degrade to on failure.
/// </summary>
public sealed class NullSyntaxHighlighter : ISyntaxHighlighter
{
    public static readonly NullSyntaxHighlighter Instance = new();

    public SyntaxTheme? Theme => null;

    public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public IReadOnlyList<IReadOnlyList<SyntaxToken>> TokenizeLines(IReadOnlyList<string> lines, string lang) =>
        lines.Select(line => (IReadOnlyList<SyntaxToken>)[new SyntaxToken(line, null, null)]).ToList();
}
