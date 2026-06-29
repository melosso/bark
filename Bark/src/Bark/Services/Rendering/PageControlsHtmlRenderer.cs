using System.Text;
using Bark.Models;
using Bark.Services.Layout;

namespace Bark.Services.Rendering;

public static class PageControlsHtmlRenderer
{
    public static string BuildPageControlsHtml(
        DocumentationPage page,
        PageControlsConfig? config,
        string basePath)
    {
        if (config is null) return string.Empty;
        if (!config.DownloadMarkdown && config.OpenInEditor is null) return string.Empty;

        var sb = new StringBuilder();
        sb.Append("<div class=\"page-controls\">");
        sb.Append("<button type=\"button\" class=\"page-controls-toggle icon-btn\" " +
                  "aria-expanded=\"false\" aria-haspopup=\"true\" aria-label=\"Page options\">" +
                  "<svg viewBox=\"0 0 24 24\" fill=\"currentColor\" aria-hidden=\"true\" width=\"16\" height=\"16\">" +
                  "<circle cx=\"12\" cy=\"5\" r=\"1.5\"/><circle cx=\"12\" cy=\"12\" r=\"1.5\"/><circle cx=\"12\" cy=\"19\" r=\"1.5\"/>" +
                  "</svg></button>");
        sb.Append("<div class=\"page-controls-menu\" hidden role=\"menu\">");

        if (config.DownloadMarkdown && page.OriginalRelativePath is { } relPath)
        {
            var rawHref = LayoutProvider.HtmlEncode($"{basePath}/raw/{relPath.TrimStart('/')}");
            var fileName = LayoutProvider.HtmlEncode(Path.GetFileName(relPath));
            sb.Append($"<a class=\"page-controls-item\" role=\"menuitem\" href=\"{rawHref}\" download=\"{fileName}\">")
              .Append("<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" aria-hidden=\"true\" width=\"14\" height=\"14\">")
              .Append("<path d=\"M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4\"/><polyline points=\"7 10 12 15 17 10\"/><line x1=\"12\" y1=\"15\" x2=\"12\" y2=\"3\"/>")
              .Append("</svg>Download markdown</a>");
        }

        if (config.OpenInEditor is { Template.Length: > 0 } editor && page.OriginalRelativePath is { } editPath)
        {
            var href = LayoutProvider.HtmlEncode(editor.Template.Replace("{path}", editPath));
            var label = LayoutProvider.HtmlEncode(editor.Label);
            sb.Append($"<a class=\"page-controls-item\" role=\"menuitem\" href=\"{href}\" target=\"_blank\" rel=\"noopener noreferrer\">")
              .Append("<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" aria-hidden=\"true\" width=\"14\" height=\"14\">")
              .Append("<path d=\"M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7\"/>")
              .Append("<path d=\"M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z\"/>")
              .Append($"</svg>{label}</a>");
        }

        sb.Append("</div></div>");
        return sb.ToString();
    }
}
