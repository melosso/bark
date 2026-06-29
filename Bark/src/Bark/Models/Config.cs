namespace Bark.Models;

public class Config
{
    public string? Title { get; set; }
    public string? TitleTemplate { get; set; }
    public string? Description { get; set; }
    public string? Lang { get; set; }
    public List<HeadTag>? Head { get; set; }

    public string? Brand { get; set; }
    public string? BrandImage { get; set; }
    public string? Footer { get; set; }
    public string? Favicon { get; set; }

    /// <summary>Flat sidebar. Superseded by <see cref="Sidebar"/> when present.</summary>
    public List<NavEntry>? Nav { get; set; }

    /// <summary>Path-prefix-keyed sidebars, e.g. <c>{"/guide/": [...]}</c>. Longest matching prefix wins, falls back to <see cref="Nav"/>.</summary>
    public Dictionary<string, List<NavEntry>>? Sidebar { get; set; }

    /// <summary>Header nav bar. Each item is a direct link or a dropdown with <see cref="TopNavItem.Items"/>.</summary>
    public List<TopNavItem>? TopNav { get; set; }

    public List<SocialLink>? SocialLinks { get; set; }

    public PageControlsConfig? PageControls { get; set; }

    /// <summary>Site-wide "Last updated" stamp toggle (uses file mtime). Off by default; a page can opt out via <c>lastUpdated: false</c>.</summary>
    public bool LastUpdated { get; set; }

    /// <summary>"Edit this page" link near the pagination footer. Null disables it.</summary>
    public EditLinkConfig? EditLink { get; set; }
}
