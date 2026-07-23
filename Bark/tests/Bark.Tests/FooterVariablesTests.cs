using Bark.Services;

namespace Bark.Tests;

public sealed class FooterVariablesTests
{
    [Fact]
    public void ExpandFooterVariables_Year_ReplacesWithCurrentUtcYear()
    {
        var result = PageRequestHandler.ExpandFooterVariables("© {year}", "Bark", null);
        Assert.Equal($"© {DateTime.UtcNow.Year}", result);
    }

    [Fact]
    public void ExpandFooterVariables_Brand_ReplacesWithBrandText()
    {
        var result = PageRequestHandler.ExpandFooterVariables("Built with {brand}", "Bark", null);
        Assert.Equal("Built with Bark", result);
    }

    [Fact]
    public void ExpandFooterVariables_Title_ReplacesWithSiteTitle()
    {
        var result = PageRequestHandler.ExpandFooterVariables("{title} docs", "Bark", "My Site");
        Assert.Equal("My Site docs", result);
    }

    [Fact]
    public void ExpandFooterVariables_NullTitle_ReplacesWithEmpty()
    {
        var result = PageRequestHandler.ExpandFooterVariables("{title}", "Bark", null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExpandFooterVariables_AllVariables_ReplacesEach()
    {
        var result = PageRequestHandler.ExpandFooterVariables("© {year} {brand} — {title}", "Melosso", "Bark Docs");
        Assert.Equal($"© {DateTime.UtcNow.Year} Melosso — Bark Docs", result);
    }

    [Fact]
    public void ExpandFooterVariables_NoVariables_ReturnsUnchanged()
    {
        var result = PageRequestHandler.ExpandFooterVariables("Built with Bark", "Bark", "Site");
        Assert.Equal("Built with Bark", result);
    }
}
