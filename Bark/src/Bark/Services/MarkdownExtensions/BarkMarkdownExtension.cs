using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Mathematics;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Bark.Models;

namespace Bark.Services.MarkdownExtensions;

public sealed class BarkMarkdownExtension(
    ISyntaxHighlighter syntaxHighlighter,
    CodeGroupIconOptions? codeGroupIcons = null,
    string basePath = "",
    MathRenderer? mathRenderer = null) : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer htmlRenderer)
            return;

        var codeBlockRenderer = new BarkCodeBlockRenderer(syntaxHighlighter);
        htmlRenderer.ObjectRenderers.ReplaceOrAdd<CodeBlockRenderer>(codeBlockRenderer);
        htmlRenderer.ObjectRenderers.ReplaceOrAdd<HtmlCustomContainerRenderer>(
            new BarkContainerRenderer(codeBlockRenderer, codeGroupIcons ?? new CodeGroupIconOptions(), basePath));

        if (mathRenderer != null)
        {
            htmlRenderer.ObjectRenderers.ReplaceOrAdd<HtmlMathInlineRenderer>(new BarkMathInlineRenderer(mathRenderer));
            htmlRenderer.ObjectRenderers.ReplaceOrAdd<HtmlMathBlockRenderer>(new BarkMathBlockRenderer(mathRenderer));
        }
    }
}

public sealed class BarkMathInlineRenderer(MathRenderer mathRenderer) : HtmlObjectRenderer<MathInline>
{
    protected override void Write(HtmlRenderer renderer, MathInline obj) =>
        renderer.Write(mathRenderer.RenderToHtml(obj.Content.ToString(), displayMode: false));
}

public sealed class BarkMathBlockRenderer(MathRenderer mathRenderer) : HtmlObjectRenderer<MathBlock>
{
    protected override void Write(HtmlRenderer renderer, MathBlock obj)
    {
        renderer.EnsureLine();
        renderer.WriteLine(mathRenderer.RenderToHtml(obj.Lines.ToString(), displayMode: true));
    }
}

public static class BarkMarkdownExtensions
{
    public static MarkdownPipelineBuilder UseBarkMarkdownExtensions(
        this MarkdownPipelineBuilder pipeline,
        ISyntaxHighlighter? syntaxHighlighter = null,
        CodeGroupIconOptions? codeGroupIcons = null,
        string basePath = "",
        MathRenderer? mathRenderer = null)
    {
        pipeline.UseCustomContainers();
        pipeline.Extensions.AddIfNotAlready(
            new BarkMarkdownExtension(
                syntaxHighlighter ?? NullSyntaxHighlighter.Instance,
                codeGroupIcons,
                basePath,
                mathRenderer));
        return pipeline;
    }
}
