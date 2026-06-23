namespace Bark.Models;

public class BarkConfig
{
    public string? Brand { get; set; }
    public string? Footer { get; set; }
    public string? Favicon { get; set; }

    /// <summary>
    /// Legacy single flat sidebar (all pages share one nav tree). Superseded by
    /// <see cref="Sidebar"/> when present; kept for backward compatibility.
    /// </summary>
    public List<NavEntry>? Nav { get; set; }

    /// <summary>
    /// Path-prefix-keyed sidebars, e.g. <c>{"/guide/": [...], "/reference/": [...]}</c> --
    /// vitepress's multi-sidebar convention. The longest matching prefix for the current page
    /// wins; falls back to <see cref="Nav"/>, then to the auto-generated folder tree.
    /// </summary>
    public Dictionary<string, List<NavEntry>>? Sidebar { get; set; }

    /// <summary>
    /// Header nav bar (vitepress's <c>themeConfig.nav</c>). Each item is either a direct link
    /// (<see cref="TopNavItem.Link"/> set) or a dropdown with <see cref="TopNavItem.Items"/>.
    /// </summary>
    public List<TopNavItem>? TopNav { get; set; }

    public List<SocialLink>? SocialLinks { get; set; }

    /// <summary>
    /// Site-wide toggle for the "Last updated" stamp, shown using each page's file last-write
    /// time (vitepress uses git history instead; Bark uses the filesystem since it has no git
    /// dependency). Off by default. A page can opt out individually via <c>lastUpdated: false</c>
    /// in its front matter even when this is on.
    /// </summary>
    public bool LastUpdated { get; set; }

    /// <summary>
    /// "Edit this page" link near the pagination footer. Null disables the feature entirely.
    /// </summary>
    public EditLinkConfig? EditLink { get; set; }
}

public class EditLinkConfig
{
    /// <summary>
    /// URL template with a <c>:path</c> placeholder, replaced with the page's lowercased URL
    /// path plus <c>.md</c> (e.g. <c>getting-started/configuration.md</c>). Example:
    /// <c>https://github.com/org/repo/edit/main/docs/:path</c>.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    public string Text { get; set; } = "Edit this page";
}

/// <summary>
/// A sidebar entry: either a leaf link (<see cref="Path"/> set, <see cref="Items"/> empty) or a
/// group header (<see cref="Items"/> set). Groups nest recursively, matching vitepress's sidebar
/// schema.
/// </summary>
public class NavEntry
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Leaf link target. Null/omitted when this entry is a group header.</summary>
    public string? Path { get; set; }

    /// <summary>
    /// Collapse behavior for a group (ignored on leaf entries):
    /// <c>null</c>/omitted -- not collapsible, always expanded, no toggle caret.
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
