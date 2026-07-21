using Bark.Services.Extensions;

namespace Bark.Tests;

public sealed class ExtensionHeadRendererTests
{
    [Fact]
    public void Empty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, ExtensionHeadRenderer.Build(ExtensionSet.Empty, "n1"));
    }

    [Fact]
    public void EmitsNonceAsyncDeferSrcAndAttributes()
    {
        var set = new ExtensionSet(
        [
            new ActiveExtension("goatcounter",
                [
                    new ExtensionScript(
                        Src: "https://you.goatcounter.com/count.js",
                        Async: true,
                        Attributes: [new KeyValuePair<string, string>("data-goatcounter", "https://you.goatcounter.com/count")])
                ],
                ["https://you.goatcounter.com"])
        ]);

        var html = ExtensionHeadRenderer.Build(set, "abc123");

        Assert.Contains("nonce=\"abc123\"", html);
        Assert.Contains(" async", html);
        Assert.Contains("src=\"https://you.goatcounter.com/count.js\"", html);
        Assert.Contains("data-goatcounter=\"https://you.goatcounter.com/count\"", html);
    }

    [Fact]
    public void InlineBodyIsEmitted()
    {
        var set = new ExtensionSet(
            [new ActiveExtension("matomo", [new ExtensionScript(Inline: "var _paq=[];")], ["https://a.example.com"])]);

        var html = ExtensionHeadRenderer.Build(set, null);
        Assert.Contains("<script>var _paq=[];</script>", html);
    }
}
