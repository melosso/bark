using Bark.Services.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bark.Tests;

public sealed class ExtensionLoaderTests : IDisposable
{
    private readonly string _dir =
        Path.Combine(Path.GetTempPath(), "bark-ext-" + Guid.NewGuid().ToString("N"));

    public ExtensionLoaderTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, true);
    }

    private ExtensionSet Load(string json)
    {
        File.WriteAllText(Path.Combine(_dir, ExtensionLoader.FileName), json);
        return ExtensionLoader.Load(_dir, NullLogger.Instance);
    }

    [Fact]
    public void MissingFile_ReturnsEmpty()
    {
        var set = ExtensionLoader.Load(_dir, NullLogger.Instance);
        Assert.True(set.IsEmpty);
    }

    [Fact]
    public void MalformedJson_ReturnsEmpty()
    {
        var set = Load("{ not valid json");
        Assert.True(set.IsEmpty);
    }

    [Fact]
    public void AllDisabled_ReturnsEmpty()
    {
        var set = Load("""{"extensions":{"plausible":{"enabled":false,"domain":"example.com"}}}""");
        Assert.True(set.IsEmpty);
    }

    [Fact]
    public void Plausible_Valid_ProducesDeferredScriptWithDataDomain()
    {
        var set = Load("""{"extensions":{"plausible":{"enabled":true,"domain":"example.com","url":"https://plausible.io"}}}""");

        var ext = Assert.Single(set.Active);
        Assert.Equal("plausible", ext.Name);
        var script = Assert.Single(ext.Scripts);
        Assert.Equal("https://plausible.io/js/script.js", script.Src);
        Assert.True(script.Defer);
        Assert.Contains(script.Attributes!, a => a is { Key: "data-domain", Value: "example.com" });
        Assert.Contains("https://plausible.io", set.CspSources);
    }

    [Fact]
    public void Matomo_Valid_EmitsInlineQueueThenTracker()
    {
        var set = Load("""{"extensions":{"matomo":{"enabled":true,"url":"https://a.example.com","siteId":"7"}}}""");

        var ext = Assert.Single(set.Active);
        Assert.Equal("matomo", ext.Name);
        Assert.Equal(2, ext.Scripts.Count);
        Assert.Contains("disableCookies", ext.Scripts[0].Inline);
        Assert.Contains("'setSiteId','7'", ext.Scripts[0].Inline);
        Assert.Equal("https://a.example.com/matomo.js", ext.Scripts[1].Src);
    }

    [Fact]
    public void Matomo_DisableCookiesFalse_OmitsDirective()
    {
        var set = Load("""{"extensions":{"matomo":{"enabled":true,"url":"https://a.example.com","siteId":"7","disableCookies":false}}}""");
        Assert.DoesNotContain("disableCookies", Assert.Single(set.Active).Scripts[0].Inline);
    }

    [Fact]
    public void Medama_And_GoatCounter_Valid()
    {
        var set = Load("""{"extensions":{"medama":{"enabled":true,"url":"https://m.example.com"},"goatcounter":{"enabled":true,"url":"https://you.goatcounter.com"}}}""");
        Assert.Equal(2, set.Active.Count);
        Assert.Contains(set.Active, e => e.Name == "medama");
        Assert.Contains(set.Active, e => e.Name == "goatcounter");
    }

    [Fact]
    public void Liwan_Valid_EmitsModuleTrackerWithEntityAndApi()
    {
        var set = Load("""{"extensions":{"liwan":{"enabled":true,"url":"https://liwan.example.com","entity":"my-site"}}}""");

        var ext = Assert.Single(set.Active);
        Assert.Equal("liwan", ext.Name);
        var script = Assert.Single(ext.Scripts);
        Assert.Equal("https://liwan.example.com/tracker.js", script.Src);
        Assert.Contains(script.Attributes!, a => a is { Key: "type", Value: "module" });
        Assert.Contains(script.Attributes!, a => a is { Key: "data-entity", Value: "my-site" });
        Assert.Contains(script.Attributes!, a => a is { Key: "data-api", Value: "https://liwan.example.com/api/event" });
        Assert.Contains("https://liwan.example.com", set.CspSources);
    }

    [Fact]
    public void Liwan_MissingEntity_IsRejected()
    {
        var set = Load("""{"extensions":{"liwan":{"enabled":true,"url":"https://liwan.example.com"}}}""");
        Assert.True(set.IsEmpty);
        Assert.Contains("liwan", set.Rejected);
    }

    [Fact]
    public void InvalidUrl_IsRejectedAndNamed()
    {
        var set = Load("""{"extensions":{"medama":{"enabled":true,"url":"ftp://bad"}}}""");
        Assert.True(set.IsEmpty);
        Assert.Contains("medama", set.Rejected);
    }

    [Fact]
    public void Matomo_NonNumericSiteId_IsRejected()
    {
        var set = Load("""{"extensions":{"matomo":{"enabled":true,"url":"https://a.example.com","siteId":"abc"}}}""");
        Assert.True(set.IsEmpty);
        Assert.Contains("matomo", set.Rejected);
    }
}
