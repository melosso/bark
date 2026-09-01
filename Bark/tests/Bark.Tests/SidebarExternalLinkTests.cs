using Bark.Models;
using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class SidebarExternalLinkTests
{
    private static List<NavEntry> Sidebar() =>
    [
        new NavEntry
        {
            Title = "Guide",
            Items =
            [
                new NavEntry { Title = "Intro", Path = "guide/intro" },
                new NavEntry { Title = "GitHub", Path = "https://github.com/hawkinslabdev/bark" },
                new NavEntry { Title = "Outro", Path = "guide/outro" },
            ],
        },
    ];

    [Fact]
    public void ExternalPath_RenderedVerbatim_NotRewrittenAsDocsPath()
    {
        var html = NavigationHtmlRenderer.BuildNavFromConfig(Sidebar(), "guide/intro", basePath: "/docs");

        Assert.Contains("href=\"https://github.com/hawkinslabdev/bark\"", html);
        Assert.DoesNotContain("/docs/https:", html);
    }

    [Fact]
    public void ExternalPath_OpensInNewTab_WithNoopenerAndIcon()
    {
        var html = NavigationHtmlRenderer.BuildNavFromConfig(Sidebar(), "guide/intro", basePath: "");

        Assert.Contains("target=\"_blank\" rel=\"noopener noreferrer\"", html);
        Assert.Contains("external-link-icon", html);
    }

    [Fact]
    public void InternalPath_StillRewrittenWithBasePath()
    {
        var html = NavigationHtmlRenderer.BuildNavFromConfig(Sidebar(), "guide/intro", basePath: "/docs");

        Assert.Contains("href=\"/docs/guide/intro/\"", html);
        Assert.Contains("sidebar-link level-1 is-active", html);
    }

    [Fact]
    public void ExternalPath_NeverMarkedActive()
    {
        var html = NavigationHtmlRenderer.BuildNavFromConfig(
            [new NavEntry { Title = "GitHub", Path = "https://github.com/hawkinslabdev/bark" }],
            currentPath: "https://github.com/hawkinslabdev/bark",
            basePath: "");

        Assert.DoesNotContain("is-active", html);
    }

    [Fact]
    public void ExternalPath_ExcludedFromPrevNextOrder()
    {
        var order = NavigationHtmlRenderer.FlattenNavEntries(Sidebar());

        Assert.Equal(["guide/intro", "guide/outro"], order);
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("HTTP://example.com", true)]
    [InlineData("guide/intro", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    // javascript: stays "internal" on purpose so Href rewrites it into an inert relative path.
    [InlineData("javascript:alert(1)", false)]
    public void IsExternal_MatchesOnlyWebSchemes(string? link, bool expected) =>
        Assert.Equal(expected, UrlPaths.IsExternal(link));

    [Fact]
    public void JavascriptScheme_RewrittenAsRelativePath_NotExecutable()
    {
        var html = NavigationHtmlRenderer.BuildNavFromConfig(
            [new NavEntry { Title = "Bad", Path = "javascript:alert(1)" }],
            currentPath: "index",
            basePath: "");

        Assert.DoesNotContain("href=\"javascript:", html);
        Assert.Contains("href=\"/javascript:alert(1)/\"", html);
    }
}
