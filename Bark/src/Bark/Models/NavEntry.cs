namespace Bark.Models;

/// <summary>A leaf link (<see cref="Path"/> set) or a group header (<see cref="Items"/> set, nesting allowed).</summary>
public class NavEntry
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Leaf link target. Null/omitted when this entry is a group header.</summary>
    public string? Path { get; set; }

    /// <summary>
    /// Collapse behavior for a group (ignored on leaf entries):
    /// <c>null</c>/omitted -- not collapsible, always expanded
    /// <c>false</c> -- collapsible, expanded by default.
    /// <c>true</c> -- collapsible, collapsed by default.
    /// A group containing the current page always renders expanded regardless of this setting.
    /// </summary>
    public bool? Collapsed { get; set; }

    /// <summary>Child entries (links and/or nested groups). Null/empty marks this a leaf link.</summary>
    public List<NavEntry>? Items { get; set; }
}
