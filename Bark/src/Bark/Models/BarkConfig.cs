namespace Bark.Models;

public class BarkConfig
{
    public string? Brand { get; set; }
    public string? Footer { get; set; }
    public List<NavSection>? Nav { get; set; }
    public List<SocialLink>? SocialLinks { get; set; }
}

public class NavSection
{
    public string Section { get; set; } = string.Empty;
    public List<NavItem> Items { get; set; } = [];
}

public class NavItem
{
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class SocialLink
{
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}
