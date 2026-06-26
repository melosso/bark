using System.Text;
using Bark.Models;

namespace Bark.Services.Rendering;

public static class HeadTagHtmlRenderer
{
    private static readonly HashSet<string> VoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "meta", "link", "base"
    };

    public static string BuildHeadTagsHtml(List<HeadTag>? tags)
    {
        if (tags is null or { Count: 0 }) return string.Empty;

        var sb = new StringBuilder();
        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag.Tag)) continue;

            sb.Append('<').Append(tag.Tag);

            if (tag.Attrs is { Count: > 0 } attrs)
            {
                foreach (var (key, value) in attrs)
                    sb.Append(' ').Append(key).Append("=\"")
                      .Append(System.Net.WebUtility.HtmlEncode(value)).Append('"');
            }

            if (VoidElements.Contains(tag.Tag))
            {
                sb.AppendLine(">");
            }
            else
            {
                sb.Append('>');
                if (tag.Content is not null)
                    sb.Append(tag.Content);
                sb.Append("</").Append(tag.Tag).AppendLine(">");
            }
        }

        return sb.ToString();
    }
}
