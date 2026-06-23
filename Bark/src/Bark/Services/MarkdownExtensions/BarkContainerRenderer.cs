using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// Renders Markdig's <see cref="CustomContainer"/> (<c>::: name</c> ... <c>:::</c>) blocks with
/// vitepress-compatible markup, ported from vitepress's node/markdown/plugins/containers.ts:
/// tip/info/warning/danger/details custom blocks, plus the code-group container.
/// </summary>
public sealed class BarkContainerRenderer : HtmlObjectRenderer<CustomContainer>
{
    private static readonly Dictionary<string, string> DefaultTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tip"] = "TIP",
        ["info"] = "INFO",
        ["warning"] = "WARNING",
        ["danger"] = "DANGER",
        ["details"] = "Details",
    };

    private readonly BarkCodeBlockRenderer _codeBlockRenderer;

    public BarkContainerRenderer(BarkCodeBlockRenderer codeBlockRenderer)
    {
        _codeBlockRenderer = codeBlockRenderer;
    }

    protected override void Write(HtmlRenderer renderer, CustomContainer obj)
    {
        var name = (obj.Info ?? string.Empty).Trim().ToLowerInvariant();

        if (name == "code-group")
        {
            WriteCodeGroup(renderer, obj);
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            WriteCustomBlock(renderer, obj, "custom-block", "");
            return;
        }

        var defaultTitle = DefaultTitles.TryGetValue(name, out var known)
            ? known
            : char.ToUpperInvariant(name[0]) + name[1..];

        WriteCustomBlock(renderer, obj, name, defaultTitle);
    }

    private static void WriteCustomBlock(HtmlRenderer renderer, CustomContainer obj, string klass, string defaultTitle)
    {
        renderer.EnsureLine();

        var info = (obj.Arguments ?? string.Empty).Trim();
        var hasCustomTitle = !string.IsNullOrEmpty(info);
        var title = hasCustomTitle ? info : defaultTitle;
        var titleClass = hasCustomTitle ? "custom-block-title" : "custom-block-title custom-block-title-default";

        if (klass == "details")
        {
            renderer.Write("<details class=\"details custom-block\"><summary>")
                .WriteEscape(title)
                .Write("</summary>\n");
            renderer.WriteChildren(obj);
            renderer.WriteLine("</details>");
        }
        else
        {
            renderer.Write("<div class=\"").Write(klass).Write(" custom-block\"><p class=\"")
                .Write(titleClass).Write("\">").WriteEscape(title).Write("</p>\n");
            renderer.WriteChildren(obj);
            renderer.WriteLine("</div>");
        }

        renderer.EnsureLine();
    }

    private void WriteCodeGroup(HtmlRenderer renderer, CustomContainer obj)
    {
        renderer.EnsureLine();
        renderer.Write("<div class=\"vp-code-group\"><div class=\"tabs\">");

        var groupId = obj.Line;
        var tabIndex = 0;
        foreach (var child in obj)
        {
            if (child is not FencedCodeBlock fence)
                continue;

            var meta = CodeBlockMeta.Parse(fence.Info, fence.Arguments);
            var title = meta.Title ?? (string.IsNullOrEmpty(meta.Lang) ? "txt" : meta.Lang);
            var tabId = $"tab-{groupId}-{tabIndex}";

            renderer.Write("<input type=\"radio\" name=\"group-").Write(groupId.ToString()).Write("\" id=\"").Write(tabId).Write('"');
            if (tabIndex == 0)
                renderer.Write(" checked");
            renderer.Write("><label data-title=\"").WriteEscape(title).Write("\" for=\"").Write(tabId).Write("\">")
                .WriteEscape(title).Write("</label>");

            tabIndex++;
        }

        renderer.Write("</div><div class=\"blocks\">");

        var isFirst = true;
        foreach (var child in obj)
        {
            if (child is FencedCodeBlock fence)
            {
                _codeBlockRenderer.WriteFenced(renderer, fence, forceActive: isFirst);
                isFirst = false;
            }
            else
            {
                renderer.Render(child);
            }
        }

        renderer.Write("</div></div>");
        renderer.EnsureLine();
    }
}
