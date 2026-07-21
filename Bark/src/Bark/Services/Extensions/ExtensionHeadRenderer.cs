using System.Text;
using System.Text.Encodings.Web;

namespace Bark.Services.Extensions;

public static class ExtensionHeadRenderer
{
    public static string Build(ExtensionSet extensions, string? nonce)
    {
        if (extensions.IsEmpty)
            return string.Empty;

        var nonceAttr = nonce is { Length: > 0 }
            ? $" nonce=\"{HtmlEncoder.Default.Encode(nonce)}\""
            : string.Empty;

        var sb = new StringBuilder(512);
        foreach (var script in extensions.Active.SelectMany(e => e.Scripts))
        {
            sb.Append("<script").Append(nonceAttr);

            if (script.Async) sb.Append(" async");
            if (script.Defer) sb.Append(" defer");

            if (script.Src is { Length: > 0 } src)
                sb.Append(" src=\"").Append(HtmlEncoder.Default.Encode(src)).Append('"');

            if (script.Attributes is { Count: > 0 } attributes)
                foreach (var (key, value) in attributes)
                    sb.Append(' ').Append(key).Append("=\"").Append(HtmlEncoder.Default.Encode(value)).Append('"');

            sb.Append('>').Append(script.Inline).Append("</script>").AppendLine();
        }

        return sb.ToString();
    }
}
