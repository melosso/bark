using Bark.Models;
using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class PageTitleRendererTests
{
    [Fact]
    public void ComputeTitle_NoConfig_ReturnsPageTitle()
    {
        Assert.Equal("My Page", PageTitleRenderer.ComputeTitle("My Page", null));
    }

    [Fact]
    public void ComputeTitle_TitleOnly_AppendsSiteName()
    {
        var config = new BarkConfig { Title = "My Site" };
        Assert.Equal("My Page | My Site", PageTitleRenderer.ComputeTitle("My Page", config));
    }

    [Fact]
    public void ComputeTitle_TitleTemplate_SubstitutesTokens()
    {
        var config = new BarkConfig { Title = "Docs", TitleTemplate = ":title — :siteName" };
        Assert.Equal("Getting Started — Docs", PageTitleRenderer.ComputeTitle("Getting Started", config));
    }

    [Fact]
    public void ComputeTitle_TitleTemplate_NoSiteName_LeavesTokenEmpty()
    {
        var config = new BarkConfig { TitleTemplate = ":title · :siteName" };
        Assert.Equal("Page · ", PageTitleRenderer.ComputeTitle("Page", config));
    }

    [Fact]
    public void ComputeTitle_TitleTemplate_PageTitleContainsSiteNameToken_NoDoubleSubstitution()
    {
        var config = new BarkConfig { Title = "Bark", TitleTemplate = ":title | :siteName" };
        Assert.Equal("My :siteName Page | Bark", PageTitleRenderer.ComputeTitle("My :siteName Page", config));
    }

    [Fact]
    public void ComputeTitle_TitleTemplateNoTokens_ReturnTemplateVerbatim()
    {
        var config = new BarkConfig { TitleTemplate = "Always This Title" };
        Assert.Equal("Always This Title", PageTitleRenderer.ComputeTitle("Ignored", config));
    }
}
