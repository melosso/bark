using Bark.Services;

namespace Bark.Tests;

public sealed class LayoutProviderTests
{
    [Fact]
    public void GetLayout_ContainsTitle()
    {
        var html = LayoutProvider.GetLayout(
            "Test Title", "<p>content</p>",
            "<nav>nav</nav>", "<li>toc</li>",
            "<a href='/'>Home</a>", "<nav>pagination</nav>",
            null);

        Assert.Contains("Test Title", html);
    }

    [Fact]
    public void GetLayout_ContainsContent()
    {
        var html = LayoutProvider.GetLayout(
            "Title", "<p>Hello World</p>",
            "<nav>nav</nav>", "<li>toc</li>",
            "<a href='/'>Home</a>", "<nav>pagination</nav>",
            null);

        Assert.Contains("Hello World", html);
    }

    [Fact]
    public void GetLayout_ContainsNavigation()
    {
        var html = LayoutProvider.GetLayout(
            "Title", "<p>content</p>",
            "<ul><li>Nav Item</li></ul>", "<li>toc</li>",
            "<a href='/'>Home</a>", "<nav>pagination</nav>",
            null);

        Assert.Contains("Nav Item", html);
    }

    [Fact]
    public void GetLayout_ContainsToc()
    {
        var html = LayoutProvider.GetLayout(
            "Title", "<p>content</p>",
            "<nav>nav</nav>", "<li>Section One</li>",
            "<a href='/'>Home</a>", "<nav>pagination</nav>",
            null);

        Assert.Contains("Section One", html);
    }

    [Fact]
    public void GetLayout_ContainsBreadcrumbs()
    {
        var html = LayoutProvider.GetLayout(
            "Title", "<p>content</p>",
            "<nav>nav</nav>", "<li>toc</li>",
            "<a href='/'>Home</a> / <a href='/page'>Page</a>",
            "<nav>pagination</nav>",
            null);

        Assert.Contains("Home", html);
        Assert.Contains("/page", html);
    }

    [Fact]
    public void GetLayout_ContainsPagination()
    {
        var html = LayoutProvider.GetLayout(
            "Title", "<p>content</p>",
            "<nav>nav</nav>", "<li>toc</li>",
            "<a href='/'>Home</a>",
            "<a href='/next' class=\"pagination-link next\">Next</a>",
            null);

        Assert.Contains("Next", html);
        Assert.Contains("/next", html);
    }

    [Fact]
    public void GetLayout_ContainsThemeCss_WhenProvided()
    {
        var themeCss = "<style>:root { --primary-color: red; }</style>";
        var html = LayoutProvider.GetLayout(
            "Title", "<p>content</p>",
            "<nav>nav</nav>", "<li>toc</li>",
            "<a href='/'>Home</a>", "<nav>pagination</nav>",
            themeCss);

        Assert.Contains("--primary-color: red;", html);
    }

    [Fact]
    public void GetLayout_HtmlEncodesTitle()
    {
        var html = LayoutProvider.GetLayout(
            "Test <script>alert('xss')</script>", "<p>content</p>",
            "<nav>nav</nav>", "<li>toc</li>",
            "<a href='/'>Home</a>", "<nav>pagination</nav>",
            null);

        Assert.Contains("Test &lt;script&gt;", html);
        Assert.Contains("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", html);
    }

    [Fact]
    public void Get404Layout_Contains404()
    {
        var html = LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode);
        Assert.Contains("404", html);
    }

    [Fact]
    public void Get404Layout_ContainsReturnHomeLink()
    {
        var html = LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode);
        Assert.Contains("Return home", html);
        Assert.Contains("href=\"/\"", html);
    }

    [Fact]
    public void Get404Layout_DoesNotContainUserContent()
    {
        var html = LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode);
        Assert.DoesNotContain("<p>content</p>", html);
    }

    [Fact]
    public void HtmlEncode_EncodesHtml()
    {
        var result = LayoutProvider.HtmlEncode("<script>alert('xss')</script>");
        Assert.Equal("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;", result);
    }

    [Fact]
    public void HtmlEncode_NullReturnsEmpty()
    {
        Assert.Equal("", LayoutProvider.HtmlEncode(null));
    }

    [Fact]
    public void HtmlEncode_EmptyReturnsEmpty()
    {
        Assert.Equal("", LayoutProvider.HtmlEncode(""));
    }
}
