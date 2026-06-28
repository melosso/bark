using System.Text;
using Bark.Models;
using Bark.Services.Layout;

namespace Bark.Services.Rendering;

public static class SocialLinksHtmlRenderer
{
    public static string BuildSocialLinksHtml(List<SocialLink>? links, string iconsDir)
    {
        if (links is not { Count: > 0 }) return string.Empty;

        var html = new StringBuilder();
        html.AppendLine("<div class=\"social-links\">");
        foreach (var link in links)
        {
            var iconSvg = IconProvider.InlineSvg(link.Icon, iconsDir);
            var icon = iconSvg.Length > 0
                ? iconSvg
                : $"<span style=\"font-size:0.9rem\" aria-hidden=\"true\">{LayoutProvider.HtmlEncode(link.Icon)}</span>";

            var tooltip = link.Title ?? link.Icon;
            var label = $"{tooltip} (opens in new tab)";
            html.AppendLine($"<a href=\"{LayoutProvider.HtmlEncode(link.Url)}\" class=\"icon-btn\" target=\"_blank\" rel=\"noopener noreferrer\" title=\"{LayoutProvider.HtmlEncode(tooltip)}\" aria-label=\"{LayoutProvider.HtmlEncode(label)}\">{icon}</a>");
        }
        html.AppendLine("</div>");
        return html.ToString();
    }
}
