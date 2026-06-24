using System.Text;
using Bark.Models;

namespace Bark.Services.Rendering;

public static class SocialLinksHtmlRenderer
{
    public static string BuildSocialLinksHtml(List<SocialLink>? links)
    {
        if (links is not { Count: > 0 }) return string.Empty;

        var html = new StringBuilder();
        html.AppendLine("<div class=\"social-links\">");
        foreach (var link in links)
        {
            var icon = link.Icon.ToLowerInvariant() switch
            {
                "github" => "<svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z\"/></svg>",
                "mastodon" => "<svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M23.268 5.313c0-3.491-2.292-4.51-2.292-4.51C19.528.247 17.648 0 12 0S4.472.247 3.024.803c0 0-2.292 1.019-2.292 4.51 0 1.129-.023 2.48.013 3.927.108 4.28.82 8.505 4.944 10.448 1.904.898 3.538 1.087 4.855.96 2.386-.23 3.727-.85 3.727-.85l-.08-1.768s-1.707.537-3.623.47c-1.89-.064-3.89-.205-4.197-2.526a4.777 4.777 0 01-.042-.708s1.88.458 4.27.566c1.448.065 2.806-.085 4.188-.25 2.64-.316 4.95-1.96 5.254-3.459.461-2.257.421-5.326.421-5.326zM19.74 13.41h-2.207V8.63c0-1.14-.48-1.718-1.44-1.718-1.062 0-1.594.687-1.594 2.044v2.96h-2.19V8.956c0-1.357-.532-2.044-1.594-2.044-.96 0-1.44.578-1.44 1.719v4.78H7.245V8.488c0-1.14.291-2.047.874-2.719.601-.672 1.389-1.017 2.363-1.017 1.13 0 1.986.434 2.547 1.302l.55.922.549-.922c.561-.868 1.417-1.302 2.547-1.302.974 0 1.762.345 2.363 1.017.583.672.874 1.578.874 2.719z\"/></svg>",
                _ => $"<span style=\"font-size:0.9rem\" aria-hidden=\"true\">{LayoutProvider.HtmlEncode(link.Icon)}</span>"
            };
            var tooltip = link.Title ?? link.Icon;
            var label = $"{tooltip} (opens in new tab)";
            html.AppendLine($"<a href=\"{LayoutProvider.HtmlEncode(link.Url)}\" class=\"icon-btn\" target=\"_blank\" rel=\"noopener noreferrer\" title=\"{LayoutProvider.HtmlEncode(tooltip)}\" aria-label=\"{LayoutProvider.HtmlEncode(label)}\">{icon}</a>");
        }
        html.AppendLine("</div>");
        return html.ToString();
    }
}
