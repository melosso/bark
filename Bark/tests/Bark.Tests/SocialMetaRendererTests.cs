using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class SocialMetaRendererTests
{
    private const string Url = "https://docs.example.com/guide/install/";

    [Fact]
    public void NoCanonical_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, SocialMetaRenderer.BuildSocialMeta(null, "T", "d", false));
    }

    [Fact]
    public void Basic_NoImage_UsesSummaryCard()
    {
        var html = SocialMetaRenderer.BuildSocialMeta(Url, "Install", "How to install", isHomePage: false);

        Assert.Contains("<meta property=\"og:type\" content=\"article\">", html);
        Assert.Contains("<meta property=\"og:title\" content=\"Install\">", html);
        Assert.Contains($"<meta property=\"og:url\" content=\"{Url}\">", html);
        Assert.Contains("<meta property=\"og:description\" content=\"How to install\">", html);
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary\">", html);
        Assert.DoesNotContain("og:image", html);
    }

    [Fact]
    public void Home_UsesWebsiteType()
    {
        var html = SocialMetaRenderer.BuildSocialMeta(Url, "Home", null, isHomePage: true);
        Assert.Contains("<meta property=\"og:type\" content=\"website\">", html);
    }

    [Fact]
    public void WithImage_EmitsImageTagsAndLargeCard()
    {
        var html = SocialMetaRenderer.BuildSocialMeta(Url, "Install", "d", false, imageUrl: "https://docs.example.com/og.png");

        Assert.Contains("<meta property=\"og:image\" content=\"https://docs.example.com/og.png\">", html);
        Assert.Contains("<meta name=\"twitter:image\" content=\"https://docs.example.com/og.png\">", html);
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary_large_image\">", html);
    }

    [Fact]
    public void SiteNameAndLocaleAndModified_AreEmitted()
    {
        var when = new DateTime(2026, 7, 21, 10, 30, 0, DateTimeKind.Utc);
        var html = SocialMetaRenderer.BuildSocialMeta(Url, "Install", "d", false, siteName: "Bark", locale: "en", modified: when);

        Assert.Contains("<meta property=\"og:site_name\" content=\"Bark\">", html);
        Assert.Contains("<meta property=\"og:locale\" content=\"en\">", html);
        Assert.Contains("<meta property=\"article:modified_time\" content=\"2026-07-21T10:30:00Z\">", html);
    }

    [Fact]
    public void Home_OmitsArticleModifiedTime()
    {
        var when = new DateTime(2026, 7, 21, 10, 30, 0, DateTimeKind.Utc);
        var html = SocialMetaRenderer.BuildSocialMeta(Url, "Home", "d", isHomePage: true, modified: when);
        Assert.DoesNotContain("article:modified_time", html);
    }
}
