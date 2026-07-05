using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bark.Tests;

/// <summary>Boots the real app in-memory against a temp docs dir; real routing, middleware, rate limiter, CSP and ETag flow</summary>
public sealed class BarkWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DocsDir { get; } =
        Path.Combine(Path.GetTempPath(), "bark-integration-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(DocsDir);
        File.WriteAllText(Path.Combine(DocsDir, "index.md"),
            "---\ntitle: Home\ndescription: Welcome\n---\n\n# Welcome\n\nHello world.\n");
        Directory.CreateDirectory(Path.Combine(DocsDir, "guide"));
        File.WriteAllText(Path.Combine(DocsDir, "guide", "install.md"),
            "---\ntitle: Install\ndescription: Setup guide\n---\n\n# Install\n\nInstallation instructions here.\n");
        File.WriteAllText(Path.Combine(DocsDir, "config.json"),
            """{"promo": "**Big news!** See the [changelog](/guide/install)"}""");

        // Port 0 keeps the pre-bind port probe from colliding with a running dev server
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

public sealed class IntegrationTests : IClassFixture<BarkWebApplicationFactory>
{
    private readonly BarkWebApplicationFactory _factory;

    public IntegrationTests(BarkWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Root_Returns200_WithHtmlAndSecurityHeaders()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("nonce-", csp);
        Assert.NotNull(response.Headers.ETag);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello world.", html);
    }

    [Fact]
    public async Task Page_SecondRequestWithETag_Returns304()
    {
        var client = _factory.CreateClient();
        var first = await client.GetAsync("/guide/install");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var etag = first.Headers.ETag;
        Assert.NotNull(etag);

        var request = new HttpRequestMessage(HttpMethod.Get, "/guide/install");
        request.Headers.IfNoneMatch.Add(etag!);
        var second = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    [Fact]
    public async Task PromoBar_RendersMarkdown_WithDismissWiring()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/");

        Assert.Contains("class=\"promo-bar\"", html);
        Assert.Contains("<strong>Big news!</strong>", html);
        Assert.Contains("href=\"/guide/install", html);
        Assert.Contains("data-promo-id=\"", html);
        Assert.Contains("bark-promo-dismissed", html);
    }

    [Fact]
    public async Task UnknownPage_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/no/such/page");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApiPages_ReturnsSummaries_WithoutServerFilePaths()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/pages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"path\":\"guide/install\"", json);
        Assert.Contains("\"title\":\"Install\"", json);
        // Contract; the response must never leak OriginalRelativePath or any server file path
        Assert.DoesNotContain(".md", json);
        Assert.DoesNotContain("originalRelativePath", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApiSearch_FindsPage_AndShortQueryReturnsEmpty()
    {
        var client = _factory.CreateClient();

        var hit = await client.GetStringAsync("/api/search?q=installation");
        Assert.Contains("guide/install", hit);

        var shortQuery = await client.GetStringAsync("/api/search?q=a");
        Assert.Equal("[]", shortQuery);
    }

    [Fact]
    public async Task ApiBuildVersion_IsNotCached()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/build-version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"version\":", json);
    }

    [Fact]
    public async Task Raw_ValidPage_ReturnsMarkdownAttachment()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/raw/guide/install");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/markdown", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Installation instructions here.", body);
    }

    [Theory]
    [InlineData("/raw/..%2Fappsettings.json")]
    [InlineData("/raw/..%2f..%2fappsettings.json")]
    [InlineData("/raw/%2e%2e/appsettings.json")]
    [InlineData("/raw/nonexistent")]
    public async Task Raw_TraversalOrUnknownPath_Returns404(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        // 400 is also acceptable; the framework may reject encoded dot-segments before routing
        Assert.True(
            response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest,
            $"Expected 404/400 for {url}, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Seo_RobotsSitemapLlmsFeed_AllRespond()
    {
        var client = _factory.CreateClient();

        var robots = await client.GetStringAsync("/robots.txt");
        Assert.Contains("Sitemap:", robots);

        var sitemap = await client.GetStringAsync("/sitemap.xml");
        Assert.Contains("<urlset", sitemap);
        Assert.Contains("guide/install", sitemap);

        var llms = await client.GetStringAsync("/llms.txt");
        Assert.Contains("[Install]", llms);

        var feed = await client.GetAsync("/feed.xml");
        Assert.Equal(HttpStatusCode.OK, feed.StatusCode);
        Assert.Contains("<rss", await feed.Content.ReadAsStringAsync());
    }
}

/// <summary>Own factory instance; burning the rate-limit budget must not starve shared-fixture tests (no remote IP on TestServer, all requests share one partition)</summary>
public sealed class RateLimitIntegrationTests
{
    [Fact]
    public async Task ApiSearch_OverLimit_Returns429()
    {
        using var factory = new BarkWebApplicationFactory();
        var client = factory.CreateClient();

        var lastStatus = HttpStatusCode.OK;
        for (var i = 0; i < 35; i++)
        {
            var response = await client.GetAsync("/api/search?q=install");
            lastStatus = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastStatus);
    }
}
