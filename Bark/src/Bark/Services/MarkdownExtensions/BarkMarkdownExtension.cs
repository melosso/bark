using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Bark.Services.MarkdownExtensions;

/// <summary>
/// Replaces Markdig's default HTML renderers for code blocks and custom containers with
/// vitepress-compatible ones. Registered via <see cref="BarkMarkdownExtensions.UseBarkMarkdownExtensions"/>,
/// which also enables Markdig's built-in custom-containers and math extensions that this
/// extension's renderers build on top of.
/// </summary>
public sealed class BarkMarkdownExtension(ISyntaxHighlighter syntaxHighlighter) : IMarkdownExtension
{
    // Parsing-side setup (custom containers, math) is enabled directly via UseCustomContainers()/
    // UseMathematics() in UseBarkMarkdownExtensions() rather than here, since adding extensions
    // from inside another extension's Setup(pipeline) would mutate the list MarkdownPipelineBuilder
    // is currently iterating over.
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
    /// Enables vitepress-compatible markdown syntax: custom containers (<c>::: tip</c>,
    /// <c>::: warning</c>, <c>::: details</c>, <c>::: code-group</c>, ...), math (<c>$...$</c> /
    /// <c>$$...$$</c>), and vitepress's fenced-code-block conventions (line highlighting via
    /// <c>{1,3}</c>, <c>[!code highlight]</c> notation comments, <c>:line-numbers</c>, and
    /// <c>[title]</c> tab titles).
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
