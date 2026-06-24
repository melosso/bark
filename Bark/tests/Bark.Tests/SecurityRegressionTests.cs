using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Bark.Configuration;
using Bark.Services;

namespace Bark.Tests;

public sealed class SecurityRegressionTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DocumentationService _service;

    public SecurityRegressionTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "bark-security-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var options = new DocsOptions
        {
            RootPath = _tempDir,
            DefaultPage = "index",
            EnableHotReload = false
        };
        _service = new DocumentationService(options, new MarkdownService(), NullLogger<DocumentationService>.Instance);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task CreateIndexPage()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "index.md"), "# Home\n");
        await _service.StartAsync(CancellationToken.None);
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..%2f..%2fetc%2fpasswd")]
    [InlineData("getting-started/../../../etc/passwd")]
    public async Task GetPageAsync_PathTraversalAttempt_ReturnsNull(string maliciousPath)
    {
        await CreateIndexPage();

        var page = await _service.GetPageAsync(maliciousPath);

        Assert.Null(page);
    }

    [Theory]
    [InlineData("index\0.md")]
    [InlineData("getting-started\0/../../etc/passwd")]
    public async Task GetPageAsync_NullByteInPath_HandledSafely(string maliciousPath)
    {
        await CreateIndexPage();

        var page = await _service.GetPageAsync(maliciousPath);

        Assert.Null(page);
    }

    [Fact]
    public void MarkdownService_RawHtmlInContent_PassesThroughUnsanitized()
    {
        // Markdig does not sanitize raw HTML by design. Docs are authored content, not
        // untrusted user input, so this is accepted - but it must not change silently.
        var markdown = new MarkdownService();
        var result = markdown.Parse("<script>alert('xss')</script>\n\n# Heading");

        Assert.Contains("<script>alert('xss')</script>", result.Html);
    }

    [Fact]
    public async Task Search_ExcerptIsDecodedPlainText_CallerMustEncodeBeforeDisplay()
    {
        // GetExcerpt intentionally HTML-decodes matched content (see SearchIndex.cs comment) so
        // callers don't double-escape. That means the API contract requires the frontend to
        // HTML-encode the excerpt before rendering - locking this in so it isn't "fixed" into a
        // double-escaping bug later.
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "index.md"),
            "# Home\n\n`<script>alert(1)</script>` marks unsafe code samples.\n");
        await _service.StartAsync(CancellationToken.None);

        var results = _service.Search("script");

        Assert.NotEmpty(results);
        var excerpt = results[0].Excerpt;
        Assert.NotNull(excerpt);
        Assert.Contains("<script>alert(1)</script>", excerpt);
    }

    [Fact]
    public async Task SecurityHeaders_Apply_SetsExpectedHeaders()
    {
        var context = new DefaultHttpContext();

        await SecurityHeaders.Apply(context, () => Task.CompletedTask);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
        Assert.Contains("default-src 'self'", context.Response.Headers["Content-Security-Policy"].ToString());
    }
}
