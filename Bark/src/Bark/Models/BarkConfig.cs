namespace Bark.Models;

public class BarkConfig
{
    public string? Title { get; set; }
    public string? TitleTemplate { get; set; }
    public string? Description { get; set; }
    public string? Lang { get; set; }
    public List<HeadTag>? Head { get; set; }

    public string? Brand { get; set; }
    public string? Footer { get; set; }
    public string? Favicon { get; set; }

    /// <summary>Flat sidebar. Superseded by <see cref="Sidebar"/> when present.</summary>
    public List<NavEntry>? Nav { get; set; }

    /// <summary>Path-prefix-keyed sidebars, e.g. <c>{"/guide/": [...]}</c>. Longest matching prefix wins, falls back to <see cref="Nav"/>.</summary>
    public Dictionary<string, List<NavEntry>>? Sidebar { get; set; }

    /// <summary>Header nav bar. Each item is a direct link or a dropdown with <see cref="TopNavItem.Items"/>.</summary>
    public List<TopNavItem>? TopNav { get; set; }

    public List<SocialLink>? SocialLinks { get; set; }

    /// <summary>Site-wide "Last updated" stamp toggle (uses file mtime). Off by default; a page can opt out via <c>lastUpdated: false</c>.</summary>
    public bool LastUpdated { get; set; }

    /// <summary>"Edit this page" link near the pagination footer. Null disables it.</summary>
    public EditLinkConfig? EditLink { get; set; }
}

public class EditLinkConfig
{
    /// <summary><c>:path</c> placeholder is replaced with the page's lowercased URL path plus <c>.md</c>.</summary>
    public string Pattern { get; set; } = string.Empty;

    public string Text { get; set; } = "Edit this page";
}

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

public class TopNavItem
{
    public string Text { get; set; } = string.Empty;

    /// <summary>Direct link. Null when this item is a dropdown (see <see cref="Items"/>).</summary>
    public string? Link { get; set; }

    /// <summary>Dropdown children. Null/empty for a plain link item.</summary>
    public List<TopNavItem>? Items { get; set; }
}

public class SocialLink
{
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}

public record HeadTag
{
    public string Tag { get; init; } = string.Empty;
    public Dictionary<string, string>? Attrs { get; init; }
    public string? Content { get; init; }
}
