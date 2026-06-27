using System.Text;
using Bark.Services.Layout;

namespace Bark.Services.Rendering;

public static class PaginationHtmlRenderer
{
    public static string BuildPaginationHtml(string? prevTitle, string? prevPath, string? nextTitle, string? nextPath, string basePath)
    {
        var html = new StringBuilder();
        html.AppendLine("<nav class=\"pagination\">");

        if (prevPath != null)
        {
            var prevUrl = prevPath == "index" ? UrlPaths.Href(basePath, "") : UrlPaths.Href(basePath, prevPath);
            html.AppendLine($"<a href=\"{prevUrl}\" class=\"pagination-link prev\">");
            html.AppendLine("<span class=\"label\">Previous</span>");
            html.AppendLine($"<span class=\"title\">{LayoutProvider.HtmlEncode(prevTitle)}</span>");
            html.AppendLine("</a>");
        }
        else
        {
            html.AppendLine("<span></span>");
        }

        if (nextPath != null)
        {
            var nextUrl = nextPath == "index" ? UrlPaths.Href(basePath, "") : UrlPaths.Href(basePath, nextPath);
            html.AppendLine($"<a href=\"{nextUrl}\" class=\"pagination-link next\">");
            html.AppendLine("<span class=\"label\">Next</span>");
            html.AppendLine($"<span class=\"title\">{LayoutProvider.HtmlEncode(nextTitle)}</span>");
            html.AppendLine("</a>");
        }
        else
        {
            html.AppendLine("<span></span>");
        }

        html.AppendLine("</nav>");
        return html.ToString();
    }
}
