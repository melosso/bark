namespace Bark.Models;

/// <summary>
/// Result of <see cref="Services.MarkdownService.Parse"/>. Provides a 4-element
/// <see cref="Deconstruct(out string, out string?, out string?, out List{HeadingInfo})"/> for
/// backward compatibility with existing call sites that only destructure
/// Html/Title/Description/Headings; new code that needs <see cref="Layout"/> or
/// <see cref="ShowLastUpdated"/> should use the record's properties directly.
/// </summary>
public sealed record MarkdownParseResult(
    string Html,
    string? Title,
    string? Description,
    List<HeadingInfo> Headings,
    string? Layout,
    bool ShowLastUpdated)
{
    public void Deconstruct(out string html, out string? title, out string? description, out List<HeadingInfo> headings)
    {
        html = Html;
        title = Title;
        description = Description;
        headings = Headings;
    }
}
