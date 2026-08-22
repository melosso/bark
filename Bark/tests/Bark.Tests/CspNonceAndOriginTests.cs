using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Bark.Configuration;
using Bark.Services;

namespace Bark.Tests;

public sealed class CspNonceAndOriginTests
{
    [Fact]
    public void Derive_IsStableForTheSameETag()
    {
        // 304 responses reuse a cached body whose nonce was baked in by an earlier 200, so the derive must stay stable for a given ETag within a process.
        Assert.Equal(CspNonce.Derive("abc123"), CspNonce.Derive("abc123"));
    }

    [Fact]
    public void Derive_DiffersPerETag()
    {
        Assert.NotEqual(CspNonce.Derive("abc123"), CspNonce.Derive("abc124"));
    }

    [Fact]
    public void Derive_IsNotAPublicFunctionOfTheETag()
    {
        // Never use a keyless ETag digest for nonces—ETags are public and easily forged.
        const string etag = "abc123";
        var publiclyDerivable = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(etag)), 0, 16);

        Assert.NotEqual(publiclyDerivable, CspNonce.Derive(etag));
    }

    [Fact]
    public void ProcessSalt_IsPresentSoRestartsInvalidateCachedBodies() =>
        Assert.False(string.IsNullOrWhiteSpace(CspNonce.ProcessSalt));

    private static PageRequestSettings Settings(string? publicBaseUrl) =>
        new("", null, "wwwroot/theme", "wwwroot", "/docs", publicBaseUrl);

    private static HttpContext Request(string host, string scheme = "https")
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = scheme;
        context.Request.Host = new HostString(host);
        return context;
    }

    [Fact]
    public void Origin_PrefersConfiguredPublicBaseUrl_OverHostHeader()
    {
        var origin = Settings("https://docs.example.com").Origin(Request("evil.example.com"));

        Assert.Equal("https://docs.example.com", origin);
    }

    [Fact]
    public void Origin_TrimsTrailingSlashFromConfiguredValue()
    {
        var origin = Settings("https://docs.example.com/").Origin(Request("evil.example.com"));

        Assert.Equal("https://docs.example.com", origin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Origin_FallsBackToRequestHost_WhenUnconfigured(string? configured)
    {
        var origin = Settings(configured).Origin(Request("localhost:5000", scheme: "http"));

        Assert.Equal("http://localhost:5000", origin);
    }

    // Binds real configuration, then hands it to the same resolver Program.cs calls.
    private static string? ResolvePublicBaseUrl(Dictionary<string, string?> config, string? cliBaseUrl = null)
    {
        var built = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
        var docs = built.GetSection("Docs").Get<DocsOptions>() ?? new DocsOptions();
        return PageRequestSettings.ResolvePublicBaseUrl(cliBaseUrl, docs.PublicBaseUrl, built["PublicBaseUrl"]);
    }

    [Fact]
    public void PublicBaseUrl_BindsFromTheBareAlias() =>
        Assert.Equal("https://docs.example.com", ResolvePublicBaseUrl(new() { ["PublicBaseUrl"] = "https://docs.example.com" }));

    [Fact]
    public void PublicBaseUrl_BindsFromTheDocsSection() =>
        Assert.Equal("https://docs.example.com", ResolvePublicBaseUrl(new() { ["Docs:PublicBaseUrl"] = "https://docs.example.com" }));

    [Fact]
    public void PublicBaseUrl_DocsSectionWinsOverTheAlias() =>
        Assert.Equal("https://docs.example.com", ResolvePublicBaseUrl(new()
        {
            ["Docs:PublicBaseUrl"] = "https://docs.example.com",
            ["PublicBaseUrl"] = "https://alias.example.com",
        }));

    [Fact]
    public void Origin_TreatsWhitespaceOnlyAsUnset()
    {
        var origin = Settings("   ").Origin(Request("localhost:5000", scheme: "http"));

        Assert.Equal("http://localhost:5000", origin);
    }

    [Fact]
    public void PublicBaseUrl_EmptyDocsSectionDoesNotMaskTheAlias() =>
        Assert.Equal("https://docs.example.com", ResolvePublicBaseUrl(new()
        {
            ["Docs:PublicBaseUrl"] = "",
            ["PublicBaseUrl"] = "https://docs.example.com",
        }));

    [Fact]
    public void PublicBaseUrl_CliBaseUrlWinsOverBothConfigSources() =>
        Assert.Equal("https://cli.example.com", ResolvePublicBaseUrl(new()
        {
            ["Docs:PublicBaseUrl"] = "https://docs.example.com",
            ["PublicBaseUrl"] = "https://alias.example.com",
        }, cliBaseUrl: "https://cli.example.com"));

    [Fact]
    public void PublicBaseUrl_BlankEverywhereResolvesToNull() =>
        Assert.Null(ResolvePublicBaseUrl(new()
        {
            ["Docs:PublicBaseUrl"] = "  ",
            ["PublicBaseUrl"] = "",
        }, cliBaseUrl: ""));

    [Fact]
    public void Normalize_StripsTrailingSlashAndSurroundingWhitespace() =>
        Assert.Equal("https://docs.example.com", PageRequestSettings.Normalize("  https://docs.example.com/  "));
}
