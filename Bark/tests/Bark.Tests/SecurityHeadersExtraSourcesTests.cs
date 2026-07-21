using Bark.Configuration;

namespace Bark.Tests;

public sealed class SecurityHeadersExtraSourcesTests
{
    [Fact]
    public void NoOrigins_ReturnsCspUnchanged()
    {
        Assert.Equal(SecurityHeaders.DefaultCsp, SecurityHeaders.WithExtraSources(SecurityHeaders.DefaultCsp, []));
    }

    [Fact]
    public void AddsOriginToScriptConnectImgAndFrameOnly()
    {
        var csp = SecurityHeaders.WithExtraSources(SecurityHeaders.DefaultCsp, ["https://plausible.io"]);

        var directives = csp.Split(';').Select(d => d.Trim()).ToArray();

        Assert.Contains(directives, d => d.StartsWith("script-src ") && d.Contains("https://plausible.io"));
        Assert.Contains(directives, d => d.StartsWith("connect-src ") && d.Contains("https://plausible.io"));
        Assert.Contains(directives, d => d.StartsWith("img-src ") && d.Contains("https://plausible.io"));
        Assert.Contains(directives, d => d.StartsWith("frame-src ") && d.Contains("https://plausible.io"));
        Assert.Contains(directives, d => d.StartsWith("style-src ") && !d.Contains("https://plausible.io"));
    }

    [Fact]
    public void ComposesWithBuildNonceCsp_OriginSurvivesInScriptSrc()
    {
        var widened = SecurityHeaders.WithExtraSources(SecurityHeaders.DefaultCsp, ["https://plausible.io"]);
        var final = SecurityHeaders.BuildNonceCsp(widened, "nonce123");

        var scriptSrc = final.Split(';').Select(d => d.Trim()).Single(d => d.StartsWith("script-src "));
        Assert.Contains("https://plausible.io", scriptSrc);
        Assert.Contains("'nonce-nonce123'", scriptSrc);
        Assert.DoesNotContain("'unsafe-inline'", scriptSrc);
    }
}
