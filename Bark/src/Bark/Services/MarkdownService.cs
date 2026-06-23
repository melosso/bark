using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Bark.Models;
using Bark.Services.MarkdownExtensions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Bark.Services;

public sealed partial class MarkdownService
{
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;

    public MarkdownService(ISyntaxHighlighter? syntaxHighlighter = null)
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
            .UseBarkMarkdownExtensions(syntaxHighlighter)
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
    }

    public MarkdownParseResult Parse(
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

                var id = heading.GetAttributes().Id ?? Slugify(headingText);
                headings.Add(new HeadingInfo(headingText, id));
            }
        }

        var html = AddHeadingAnchors(Markdown.ToHtml(markdown, _pipeline));

        if (frontMatter?.Layout == "home")
            html = RenderHomePage(frontMatter) + html;

        return new MarkdownParseResult(
            html,
            frontMatter?.Title ?? defaultTitle,
            frontMatter?.Description,
            headings,
            frontMatter?.Layout,
            frontMatter?.LastUpdated ?? true);
    }

    private static string RenderHomePage(FrontMatter frontMatter)
    {
        var sb = new StringBuilder();
        sb.Append("<div class=\"vp-home\">");

        if (frontMatter.Hero is { } hero)
        {
            sb.Append("<div class=\"vp-hero\">");

            if (!string.IsNullOrWhiteSpace(hero.Image))
            {
                var isUrl = hero.Image.Contains('/') || hero.Image.StartsWith("http", StringComparison.OrdinalIgnoreCase);
                sb.Append("<div class=\"vp-hero-image\">")
                    .Append(isUrl
                        ? $"<img src=\"{WebUtility.HtmlEncode(hero.Image)}\" alt=\"\">"
                        : WebUtility.HtmlEncode(hero.Image))
                    .Append("</div>");
            }

            if (!string.IsNullOrWhiteSpace(hero.Name))
                sb.Append("<h1 class=\"vp-hero-name\">").Append(WebUtility.HtmlEncode(hero.Name)).Append("</h1>");
            if (!string.IsNullOrWhiteSpace(hero.Text))
                sb.Append("<p class=\"vp-hero-text\">").Append(WebUtility.HtmlEncode(hero.Text)).Append("</p>");
            if (!string.IsNullOrWhiteSpace(hero.Tagline))
                sb.Append("<p class=\"vp-hero-tagline\">").Append(WebUtility.HtmlEncode(hero.Tagline)).Append("</p>");

            if (hero.Actions is { Count: > 0 } actions)
            {
                sb.Append("<div class=\"vp-hero-actions\">");
                foreach (var action in actions)
                {
                    var theme = action.Theme == "alt" ? "alt" : "brand";
                    sb.Append("<a class=\"vp-hero-action ").Append(theme).Append("\" href=\"")
                        .Append(WebUtility.HtmlEncode(action.Link ?? "#")).Append("\">")
                        .Append(WebUtility.HtmlEncode(action.Text ?? string.Empty)).Append("</a>");
                }
                sb.Append("</div>");
            }

            sb.Append("</div>");
        }

        if (frontMatter.Features is { Count: > 0 } features)
        {
            sb.Append("<div class=\"vp-features\">");
            foreach (var feature in features)
            {
                var hasLink = !string.IsNullOrWhiteSpace(feature.Link);
                sb.Append(hasLink
                    ? $"<a class=\"vp-feature\" href=\"{WebUtility.HtmlEncode(feature.Link)}\">"
                    : "<div class=\"vp-feature\">");

                if (!string.IsNullOrWhiteSpace(feature.Icon))
                    sb.Append("<div class=\"vp-feature-icon\">").Append(WebUtility.HtmlEncode(feature.Icon)).Append("</div>");
                if (!string.IsNullOrWhiteSpace(feature.Title))
                    sb.Append("<h2 class=\"vp-feature-title\">").Append(WebUtility.HtmlEncode(feature.Title)).Append("</h2>");
                if (!string.IsNullOrWhiteSpace(feature.Details))
                    sb.Append("<p class=\"vp-feature-details\">").Append(WebUtility.HtmlEncode(feature.Details)).Append("</p>");

                sb.Append(hasLink ? "</a>" : "</div>");
            }
            sb.Append("</div>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private static string AddHeadingAnchors(string html) =>
        HeadingRegex().Replace(html, match =>
        {
            var level = match.Groups[1].Value;
            var id = match.Groups[2].Value;
            var inner = match.Groups[3].Value;
            var plainText = TagRegex().Replace(inner, string.Empty);
            return $"<h{level} id=\"{id}\" tabindex=\"-1\">{inner} " +
                   $"<a class=\"header-anchor\" href=\"#{id}\" aria-label=\"Permalink to &quot;{plainText}&quot;\">&#8203;</a></h{level}>";
        });

    [GeneratedRegex(@"<h([2-6]) id=""([^""]+)"">(.*?)</h\1>", RegexOptions.Singleline)]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"<[^>]*>")]
    private static partial Regex TagRegex();

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
