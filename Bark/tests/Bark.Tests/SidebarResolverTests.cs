using Bark.Models;
using Bark.Services;

namespace Bark.Tests;

public sealed class SidebarResolverTests
{
    [Fact]
    public void Resolve_PicksMatchingPrefix()
    {
        var sidebars = new Dictionary<string, List<NavEntry>>
        {
            ["/guide/"] = [new NavEntry { Title = "Guide" }],
            ["/reference/"] = [new NavEntry { Title = "Reference" }],
        };

        var result = SidebarResolver.Resolve(sidebars, "guide/getting-started");

        Assert.NotNull(result);
        Assert.Equal("Guide", result![0].Title);
    }

    [Fact]
    public void Resolve_ExactPrefixMatch_NoTrailingSegment()
    {
        var sidebars = new Dictionary<string, List<NavEntry>>
        {
            ["/guide/"] = [new NavEntry { Title = "Guide" }],
        };

        var result = SidebarResolver.Resolve(sidebars, "guide");

        Assert.NotNull(result);
        Assert.Equal("Guide", result![0].Title);
    }

    [Fact]
    public void Resolve_PicksLongestMatchingPrefix()
    {
        var sidebars = new Dictionary<string, List<NavEntry>>
        {
            ["/guide/"] = [new NavEntry { Title = "Guide" }],
            ["/guide/advanced/"] = [new NavEntry { Title = "Advanced Guide" }],
        };

        var result = SidebarResolver.Resolve(sidebars, "guide/advanced/topic");

        Assert.NotNull(result);
        Assert.Equal("Advanced Guide", result![0].Title);
    }

    [Fact]
    public void Resolve_EmptyPrefixActsAsCatchAll_ButLosesToMoreSpecific()
    {
        var sidebars = new Dictionary<string, List<NavEntry>>
        {
            ["/"] = [new NavEntry { Title = "Default" }],
            ["/guide/"] = [new NavEntry { Title = "Guide" }],
        };

        Assert.Equal("Guide", SidebarResolver.Resolve(sidebars, "guide/intro")![0].Title);
        Assert.Equal("Default", SidebarResolver.Resolve(sidebars, "something-else")![0].Title);
    }

    [Fact]
    public void Resolve_NoMatch_ReturnsNull()
    {
        var sidebars = new Dictionary<string, List<NavEntry>>
        {
            ["/guide/"] = [new NavEntry { Title = "Guide" }],
        };

        Assert.Null(SidebarResolver.Resolve(sidebars, "reference/endpoints"));
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var sidebars = new Dictionary<string, List<NavEntry>>
        {
            ["/Guide/"] = [new NavEntry { Title = "Guide" }],
        };

        Assert.NotNull(SidebarResolver.Resolve(sidebars, "GUIDE/Intro"));
    }
}
