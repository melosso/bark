using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Bark.Services.MarkdownExtensions;

/// <summary>Replaces Markdig's default HTML renderers for code blocks and custom containers with Bark's.</summary>
public sealed class BarkMarkdownExtension(ISyntaxHighlighter syntaxHighlighter) : IMarkdownExtension
{
    // Custom-containers/math are enabled via UseCustomContainers()/UseMathematics() in
    // UseBarkMarkdownExtensions() instead, since adding extensions from inside another
    // extension's Setup(pipeline) would mutate the list being iterated over.
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
            new BarkContainerRenderer(codeBlockRenderer));
    }
}

public static class BarkMarkdownExtensions
{
    /// <summary>
    /// Enables custom containers (<c>::: tip/warning/details/code-group</c>), math (<c>$...$</c>/<c>$$...$$</c>),
    /// and fenced-code-block conventions: <c>{1,3}</c> line highlighting, <c>[!code highlight]</c> notation,
    /// <c>:line-numbers</c>, and <c>[title]</c> tab titles.
    /// </summary>
    public static MarkdownPipelineBuilder UseBarkMarkdownExtensions(
        this MarkdownPipelineBuilder pipeline,
        ISyntaxHighlighter? syntaxHighlighter = null)
    {
        pipeline.UseCustomContainers();
        pipeline.UseMathematics();
        pipeline.Extensions.AddIfNotAlready(
            new BarkMarkdownExtension(syntaxHighlighter ?? NullSyntaxHighlighter.Instance));
        return pipeline;
    }
}
