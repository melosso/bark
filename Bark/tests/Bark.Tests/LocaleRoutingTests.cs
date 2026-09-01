using System.Text.Json;
using Bark.Models;
using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class LocaleRoutingTests
{
    private static Config ConfigWith(params string[] codes)
    {
        var locales = new Dictionary<string, LocaleEntry> { ["root"] = new() { Label = "English", Lang = "en" } };
        foreach (var code in codes)
            locales[code] = new LocaleEntry { Label = code.ToUpperInvariant(), Lang = code };

        return new Config { Locales = locales };
    }

    [Fact]
    public void PathWithoutALocaleSegment_StaysOnTheRootTree()
    {
        var route = LocaleRouting.Resolve(ConfigWith("nl"), "guide/install");

        Assert.Equal("en", route.Code);
        Assert.Equal(string.Empty, route.Prefix);
        Assert.Equal("guide/install", route.ContentPath);
        Assert.Null(route.FallbackPath);
    }

    [Fact]
    public void PathInsideALocaleTree_CarriesTheOriginalAsItsFallback()
    {
        var route = LocaleRouting.Resolve(ConfigWith("nl"), "nl/guide/install");

        Assert.Equal("nl", route.Code);
        Assert.Equal("nl", route.Prefix);
        Assert.Equal("nl/guide/install", route.ContentPath);
        Assert.Equal("guide/install", route.FallbackPath);
    }

    [Fact]
    public void LocaleTreeRoot_ResolvesToTheTreeIndexPage()
    {
        var route = LocaleRouting.Resolve(ConfigWith("nl"), "nl");

        Assert.Equal("nl/", route.ContentPath + "/");
        Assert.Equal("index", route.FallbackPath);
    }

    [Fact]
    public void SegmentThatOnlyLooksLikeALocale_IsTreatedAsContent()
    {
        var route = LocaleRouting.Resolve(ConfigWith("nl"), "de/guide");

        Assert.Equal("en", route.Code);
        Assert.Equal("de/guide", route.ContentPath);
    }

    [Fact]
    public void WithoutConfiguredLocales_EveryPathIsRoot()
    {
        var route = LocaleRouting.Resolve(new Config(), "nl/guide");

        Assert.Equal("nl/guide", route.ContentPath);
        Assert.Null(route.FallbackPath);
    }

    [Theory]
    [InlineData("nl", "guide/install", "nl/guide/install")]
    [InlineData("nl", "index", "nl")]
    [InlineData("", "guide/install", "guide/install")]
    public void LocalizeAddsThePrefix(string prefix, string path, string expected) =>
        Assert.Equal(expected, LocaleRouting.Localize(prefix, path));

    [Theory]
    [InlineData("nl", "nl/guide/install", "guide/install")]
    [InlineData("nl", "nl", "index")]
    [InlineData("", "guide/install", "guide/install")]
    public void DelocalizeRemovesThePrefix(string prefix, string path, string expected) =>
        Assert.Equal(expected, LocaleRouting.Delocalize(prefix, path));

    [Fact]
    public void LangAndLabelComeFromTheConfiguredEntry()
    {
        var config = ConfigWith("nl");

        Assert.Equal("nl", LocaleRouting.LangOf(config, "nl"));
        Assert.Equal("NL", LocaleRouting.LabelOf(config, "nl"));
        Assert.Equal("en", LocaleRouting.LangOf(config, "en"));
        Assert.Equal("English", LocaleRouting.LabelOf(config, "en"));
    }

    [Fact]
    public void RootCodeFollowsTheLocaleSetting()
    {
        var config = JsonSerializer.Deserialize<Config>("""{ "locale": "de" }""", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal("de", LocaleRouting.RootCode(config));
    }

    [Fact]
    public void SwitcherIsEmptyWhenOnlyOneLocaleExists()
    {
        var html = LocaleSwitcherRenderer.Build(new Config(), "en", "guide/install", "", Localization.Default);

        Assert.Equal(string.Empty, html);
    }

    [Fact]
    public void SwitcherLinksEveryLocaleAndMarksTheCurrentOne()
    {
        var html = LocaleSwitcherRenderer.Build(ConfigWith("nl"), "nl", "nl/guide/install", "", Localization.Default);

        Assert.Contains("href=\"/guide/install/\"", html);
        Assert.Contains("href=\"/nl/guide/install/\"", html);
        Assert.Contains("locale-option--current", html);
        Assert.Contains("aria-current=\"true\"", html);
    }

    [Fact]
    public void SwitcherKeepsThePageEvenWhenNoTranslationExists()
    {
        var html = LocaleSwitcherRenderer.Build(ConfigWith("nl"), "en", "guide/install", "", Localization.Default);

        Assert.Contains("href=\"/nl/guide/install/\"", html);
        Assert.DoesNotContain("href=\"/nl/\"", html);
    }

    [Fact]
    public void SwitcherOnAnIndexPageLinksTheTreeRoots()
    {
        var html = LocaleSwitcherRenderer.Build(ConfigWith("nl"), "en", "index", "", Localization.Default);

        Assert.Contains("href=\"/\"", html);
        Assert.Contains("href=\"/nl/\"", html);
    }
}
