using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Mathematics;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Bark.Models;

namespace Bark.Services.MarkdownExtensions;

/// <summary>Replaces Markdig's default HTML renderers for code blocks, custom containers, and math</summary>
public sealed class BarkMarkdownExtension(
    ISyntaxHighlighter syntaxHighlighter,
    MathRenderer mathRenderer,
    CodeGroupIconOptions? codeGroupIcons = null,
    string basePath = "") : IMarkdownExtension
{
    // // Math and Custom-containers must be registered in UseBarkMarkdownExtensions() to avoid modifying the pipeline collection during Setup() !
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

        htmlRenderer.ObjectRenderers.ReplaceOrAdd<HtmlMathInlineRenderer>(new BarkMathInlineRenderer(mathRenderer));
        htmlRenderer.ObjectRenderers.ReplaceOrAdd<HtmlMathBlockRenderer>(new BarkMathBlockRenderer(mathRenderer));
    }
}

/// <summary>Server-side renders inline <c>$...$</c> math to static KaTeX HTML</summary>
public sealed class BarkMathInlineRenderer(MathRenderer mathRenderer) : HtmlObjectRenderer<MathInline>
{
    protected override void Write(HtmlRenderer renderer, MathInline obj) =>
        renderer.Write(mathRenderer.RenderToHtml(obj.Content.ToString(), displayMode: false));
}

/// <summary>Server-side renders block <c>$$...$$</c> math to static KaTeX HTML</summary>
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
    /// <summary>
    /// Enables custom containers (<c>::: tip/warning/details/code-group</c>); math (<c>$...$</c>/<c>$$...$$</c>,
    /// rendered server-side via <see cref="MathRenderer"/>); fenced-code-block conventions:
    /// <c>{1,3}</c> line highlighting, <c>[!code highlight]</c> notation, <c>:line-numbers</c>, <c>[title]</c> tab titles
    /// </summary>
    public static MarkdownPipelineBuilder UseBarkMarkdownExtensions(
        this MarkdownPipelineBuilder pipeline,
        ISyntaxHighlighter? syntaxHighlighter = null,
        MathRenderer? mathRenderer = null,
        CodeGroupIconOptions? codeGroupIcons = null,
        string basePath = "")
    {
        pipeline.UseCustomContainers();
        pipeline.UseMathematics();
        pipeline.Extensions.AddIfNotAlready(
            new BarkMarkdownExtension(syntaxHighlighter ?? NullSyntaxHighlighter.Instance, mathRenderer ?? new MathRenderer(), codeGroupIcons, basePath));
        return pipeline;
    }
}
