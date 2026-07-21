using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bark.Tests;

public sealed class ExtensionsWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DocsDir { get; } =
        Path.Combine(Path.GetTempPath(), "bark-ext-int-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(DocsDir);
        File.WriteAllText(Path.Combine(DocsDir, "index.md"),
            "---\ntitle: Home\n---\n\n# Welcome\n\nHello.\n");
        File.WriteAllText(Path.Combine(DocsDir, "extensions.json"),
            """{"extensions":{"plausible":{"enabled":true,"domain":"example.com","url":"https://plausible.io"}}}""");

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

public sealed class ExtensionsIntegrationTests : IClassFixture<ExtensionsWebApplicationFactory>
{
    private readonly ExtensionsWebApplicationFactory _factory;

    public ExtensionsIntegrationTests(ExtensionsWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task EnabledPlausible_InjectsScriptAndWidensCsp()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("https://plausible.io/js/script.js", html);
        Assert.Contains("data-domain=\"example.com\"", html);

        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        var scriptSrc = csp.Split(';').Select(d => d.Trim()).Single(d => d.StartsWith("script-src "));
        Assert.Contains("https://plausible.io", scriptSrc);
    }
}
