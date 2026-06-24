namespace Bark.Models;

public sealed record DocumentationPage(
    string Path,
    string Title,
    string HtmlContent,
    string? Description = null,
    DateTime? LastModified = null,
    IReadOnlyList<HeadingInfo> Headings = default!,
    string? Layout = null,
    bool ShowLastUpdated = true
)
{
    public DocumentationPage(
        string path,
        string title,
        string htmlContent,
        string? description = null,
        DateTime? lastModified = null,
        IEnumerable<HeadingInfo>? headings = null,
        string? layout = null,
        bool showLastUpdated = true
    ) : this(
        path,
        title,
        htmlContent,
        description,
        lastModified,
        (headings ?? Array.Empty<HeadingInfo>()).ToList().AsReadOnly(),
        layout,
        showLastUpdated
    ) { }
}
