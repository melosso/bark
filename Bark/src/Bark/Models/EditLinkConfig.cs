namespace Bark.Models;

public class EditLinkConfig
{
    /// <summary><c>:path</c> placeholder is replaced with the page's lowercased URL path plus <c>.md</c>.</summary>
    public string Pattern { get; set; } = string.Empty;

    public string Text { get; set; } = "Edit this page";
}
