using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;
using Bark.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Bark.Services;

public sealed class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseAutoIdentifiers()
            .UsePipeTables()
            .UseTaskLists()
            .UseDefinitionLists()
            .UseFootnotes()
            .UseEmojiAndSmiley()
            .UseMediaLinks()
            .UseListExtras()
            .UseGridTables()
            .UseAutoLinks()
            .UseAlertBlocks()
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
    }

    public (string Html, string? Title, string? Description, List<HeadingInfo> Headings) Parse(
        string markdown,
        string? defaultTitle = null)
    {
        var document = Markdown.Parse(markdown, _pipeline);

        var frontMatter = ParseFrontMatter(document);

        var headings = new List<HeadingInfo>();
        foreach (var block in document.Descendants())
        {
            if (block is HeadingBlock heading)
            {
                var headingText = ExtractInlineText(heading.Inline);
                if (string.IsNullOrEmpty(headingText))
                    continue;

                var id = Slugify(headingText);
                headings.Add(new HeadingInfo(headingText, id));
            }
        }

        var html = Markdown.ToHtml(markdown, _pipeline);

        return (html, frontMatter?.Title ?? defaultTitle, frontMatter?.Description, headings);
    }

    private FrontMatter? ParseFrontMatter(MarkdownDocument document)
    {
        if (document.FirstOrDefault() is not YamlFrontMatterBlock yamlBlock)
            return null;

        var yaml = yamlBlock.Lines.ToString().Trim();
        if (string.IsNullOrWhiteSpace(yaml))
            return null;

        try
        {
            return _yamlDeserializer.Deserialize<FrontMatter>(yaml);
        }
        catch (YamlException)
        {
            return null;
        }
    }

    private static string ExtractInlineText(ContainerInline? container)
    {
        if (container == null) return string.Empty;
        var sb = new StringBuilder();
        var inline = container.FirstChild;
        while (inline != null)
        {
            if (inline is LiteralInline lit)
                sb.Append(lit.Content);
            else if (inline is CodeInline code)
                sb.Append(code.Content);
            else if (inline is ContainerInline child)
                sb.Append(ExtractInlineText(child));
            inline = inline.NextSibling;
        }
        return sb.ToString().Trim();
    }

    public string ToHtml(string markdown) =>
        Markdown.ToHtml(markdown, _pipeline);

    public static string Slugify(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var builder = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
            {
                builder.Append(char.ToLowerInvariant(c));
            }
            else if (char.IsWhiteSpace(c) || c is '.' or ',' or '/' or '\\' or '+' or '~' or '#'
                     or '(' or ')' or '[' or ']' or '{' or '}' or '*' or '&' or '^' or '%'
                     or '$' or '!' or '@' or '`' or '\'' or '"' or ':' or ';' or '|'
                     or '<' or '>' or '?' or '=' or '~')
            {
                if (builder.Length > 0 && builder[^1] != '-')
                    builder.Append('-');
            }
        }

        while (builder.Length > 0 && builder[^1] == '-')
            builder.Length--;

        return builder.ToString();
    }
}
