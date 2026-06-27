namespace Bark.Models;

public class TopNavItem
{
    public string Text { get; set; } = string.Empty;

    /// <summary>Direct link. Null when this item is a dropdown (see <see cref="Items"/>).</summary>
    public string? Link { get; set; }

    /// <summary>Dropdown children. Null/empty for a plain link item.</summary>
    public List<TopNavItem>? Items { get; set; }
}
