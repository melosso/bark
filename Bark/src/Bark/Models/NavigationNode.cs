namespace Bark.Models;

public sealed record NavigationNode(
    string Title,
    string? Path,
    IReadOnlyList<NavigationNode> Children
)
{
    public NavigationNode(string title, string? path = null, IEnumerable<NavigationNode>? children = null)
        : this(title, path, (children ?? Array.Empty<NavigationNode>()).ToList().AsReadOnly())
    { }
}
