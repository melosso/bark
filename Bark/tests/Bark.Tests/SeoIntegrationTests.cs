using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bark.Tests;

public sealed class SeoWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DocsDir { get; } =
        Path.Combine(Path.GetTempPath(), "bark-seo-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(DocsDir);
        File.WriteAllText(Path.Combine(DocsDir, "index.md"),
            "---\ntitle: Home\ndescription: Welcome\nlayout: home\n---\n\n# Welcome\n");
        Directory.CreateDirectory(Path.Combine(DocsDir, "guide"));
        File.WriteAllText(Path.Combine(DocsDir, "guide", "install.md"),
            "---\ntitle: Install\ndescription: How to install\nimage: /custom-og.png\n---\n\n# Install\n");
        File.WriteAllText(Path.Combine(DocsDir, "config.json"),
            """{"brand":"Bark","image":"/site-og.png"}""");

        builder.UseSetting("urls", "http://127.0.0.1:0");
        builder.UseSetting("Docs:RootPath", DocsDir);
        builder.UseSetting("Docs:EnableHotReload", "false");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Directory.Exists(DocsDir))
            Directory.Delete(DocsDir, true);
    }
}

public sealed class SeoIntegrationTests : IClassFixture<SeoWebApplicationFactory>
{
    private readonly SeoWebApplicationFactory _factory;

    public SeoIntegrationTests(SeoWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Home_EmitsWebSiteJsonLdAndSiteImage()
    {
        var html = await _factory.CreateClient().GetStringAsync("/");

        Assert.Contains("<meta property=\"og:type\" content=\"website\">", html);
        Assert.Contains("<meta property=\"og:site_name\" content=\"Bark\">", html);
        Assert.Contains("application/ld+json", html);
        Assert.Contains("\"WebSite\"", html);
        Assert.Contains("/site-og.png", html);
    }

    [Fact]
    public async Task Page_UsesFrontmatterImageArticleAndBreadcrumbs()
    {
        var html = await _factory.CreateClient().GetStringAsync("/guide/install");

        Assert.Contains("<meta property=\"og:type\" content=\"article\">", html);
        Assert.Contains("/custom-og.png", html);
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary_large_image\">", html);
        Assert.Contains("\"Article\"", html);
        Assert.Contains("\"BreadcrumbList\"", html);
    }
}
