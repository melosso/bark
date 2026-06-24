using System.Text;
using Bark.Models;

namespace Bark.Services.Rendering;

public static class BreadcrumbHtmlRenderer
{
    public static string BuildBreadcrumbHtml(IReadOnlyList<BreadcrumbItem> crumbs, string currentTitle, string basePath)
    {
        var html = new StringBuilder();
        for (var i = 0; i < crumbs.Count - 1; i++)
        {
            var crumb = crumbs[i];
            html.Append($"<a href=\"{UrlPaths.Href(basePath, crumb.Path ?? "")}\">{LayoutProvider.HtmlEncode(crumb.Title)}</a>");
            html.Append("<span class=\"separator\">/</span>");
        }
        html.Append($"<span class=\"current\">{LayoutProvider.HtmlEncode(currentTitle)}</span>");
        return html.ToString();
    }
}
