using Bark.Services;

namespace Bark.Tests;

public sealed class MarkdownServiceTests
{
    private readonly MarkdownService _service = new();

    [Fact]
    public void Parse_WithFrontMatter_ExtractsTitle()
    {
        var md = "---\ntitle: My Custom Title\ndescription: A test page\n---\n\n# Content\n";
        var (_, title, description, _) = _service.Parse(md);
        Assert.Equal("My Custom Title", title);
        Assert.Equal("A test page", description);
    }

    [Fact]
    public void Parse_WithoutFrontMatter_UsesDefaultTitle()
    {
        var md = "# Just Content";
        var (_, title, _, _) = _service.Parse(md, "Default Title");
        Assert.Equal("Default Title", title);
    }

    [Fact]
    public void Parse_WithoutFrontMatter_DescriptionIsNull()
    {
        var md = "# Just Content";
        var (_, _, description, _) = _service.Parse(md);
        Assert.Null(description);
    }

    [Fact]
    public void Parse_GeneratesHtml()
    {
        var md = "# Hello";
        var (html, _, _, _) = _service.Parse(md);
        Assert.Contains("<h1", html);
        Assert.Contains("Hello", html);
    }

    [Fact]
    public void Parse_FencedBlockWithTitle_RendersTitleBarAndHidesLang()
    {
        var md = "```json [./appsettings.json]\n{\n  \"Hello\": \"world\"\n}\n```\n";
        var (html, _, _, _) = _service.Parse(md);

        Assert.Contains("class=\"language-json has-title\"", html);
        Assert.Contains("<div class=\"code-title\">./appsettings.json</div>", html);
    }

    [Fact]
    public void Parse_CodeGroupChild_DoesNotRenderTitleBar()
    {
        var md = "::: code-group\n```json [./appsettings.json]\n{}\n```\n:::\n";
        var (html, _, _, _) = _service.Parse(md);

        Assert.DoesNotContain("code-title", html);
    }

    [Fact]
    public void Parse_FourBacktickFence_ToleratesNestedTripleBacktickFences()
    {
        // CommonMark closes a fence on any same/longer same-char line, so nested ``` needs a longer outer fence.
        var md = "````md\n```sh\necho hi\n```\n````\n\nAfter";
        var (html, _, _, _) = _service.Parse(md);
        Assert.Contains("```sh", html);
        Assert.Contains("After", html);
        Assert.DoesNotContain("<p>```", html);
    }

    [Fact]
    public void Parse_ExtractsHeadings()
    {
        var md = """
# Title
## Section One
### Sub Section
## Section Two
""";
        var (_, _, _, headings) = _service.Parse(md);
        Assert.Contains(headings, h => h.Text == "Section One");
        Assert.Contains(headings, h => h.Text == "Sub Section");
        Assert.Contains(headings, h => h.Text == "Section Two");
    }

    [Fact]
    public void Parse_HeadingsHaveSlugifiedIds()
    {
        var md = "## Hello World";
        var (_, _, _, headings) = _service.Parse(md);
        var heading = Assert.Single(headings);
        Assert.Equal("hello-world", heading.Id);
    }

    [Fact]
    public void Parse_EmptyMarkdown_ReturnsEmptyHtml()
    {
        var (html, _, _, _) = _service.Parse("");
        Assert.Empty(html);
    }

    [Fact]
    public void Parse_InvalidFrontMatter_DoesNotThrow()
    {
        var md = "---\ninvalid: : : yaml\n---\n\n# Content\n";
        var ex = Record.Exception(() => _service.Parse(md));
        Assert.Null(ex);
    }

    [Fact]
    public void Parse_CodeBlocksArePreserved()
    {
        var md = """
```csharp
var x = 1;
```
""";
        var (html, _, _, _) = _service.Parse(md);
        Assert.Contains("var x = 1", html);
    }

    [Fact]
    public void Parse_InlineMath_RendersStaticKaTeXHtml()
    {
        var (html, _, _, _) = _service.Parse("$E = mc^2$");
        Assert.Contains("class=\"katex\"", html);
        Assert.Contains("annotation encoding=\"application/x-tex\">E = mc^2</annotation>", html);
        Assert.DoesNotContain(@"\(E = mc^2\)", html);
    }

    [Fact]
    public void Parse_BlockMath_RendersStaticKaTeXHtml()
    {
        var md = """
$$
E = mc^2
$$
""";
        var (html, _, _, _) = _service.Parse(md);
        Assert.Contains("class=\"katex-display\"", html);
        Assert.Contains("annotation encoding=\"application/x-tex\">", html);
    }

    [Fact]
    public void Slugify_ReplacesSpacesWithHyphens()
    {
        var result = MarkdownService.Slugify("Hello World");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public void Slugify_RemovesSpecialChars()
    {
        var result = MarkdownService.Slugify("Hello, World! How's it going?");
        Assert.Equal("hello-world-how-s-it-going", result);
    }

    [Fact]
    public void Slugify_Lowercases()
    {
        var result = MarkdownService.Slugify("HELLO World");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public void Slugify_CollapsesMultipleHyphens()
    {
        var result = MarkdownService.Slugify("Hello   World");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public void Slugify_TrimsTrailingHyphens()
    {
        var result = MarkdownService.Slugify("Hello World!!!");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public void Slugify_EmptyString_ReturnsEmpty()
    {
        var result = MarkdownService.Slugify("");
        Assert.Equal("", result);
    }
}
