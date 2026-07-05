using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class PromoBarHtmlRendererTests
{
    [Fact]
    public void BuildPromoBarHtml_EmptyHtml_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, PromoBarHtmlRenderer.BuildPromoBarHtml(null, "source", "nonce"));
        Assert.Equal(string.Empty, PromoBarHtmlRenderer.BuildPromoBarHtml("", "source", "nonce"));
        Assert.Equal(string.Empty, PromoBarHtmlRenderer.BuildPromoBarHtml("   ", "source", "nonce"));
    }

    [Fact]
    public void BuildPromoBarHtml_RendersContentAndCloseButton()
    {
        var html = PromoBarHtmlRenderer.BuildPromoBarHtml("<p><strong>Sale!</strong></p>", "**Sale!**", "abc123");

        Assert.Contains("class=\"promo-bar\"", html);
        Assert.Contains("class=\"promo-bar-inner\"", html);
        Assert.Contains("<p><strong>Sale!</strong></p>", html);
        Assert.Contains("id=\"promo-bar-close\"", html);
        Assert.Contains("aria-label=\"Dismiss announcement\"", html);
    }

    [Fact]
    public void BuildPromoBarHtml_InlineCheckScript_CarriesNonce()
    {
        var html = PromoBarHtmlRenderer.BuildPromoBarHtml("<p>x</p>", "x", "my-nonce");

        Assert.Contains("<script nonce=\"my-nonce\">", html);
        Assert.Contains("bark-promo-dismissed", html);
        Assert.Contains("promo-dismissed", html);
    }

    [Fact]
    public void BuildPromoBarHtml_PromoId_IsStableForSameSource_AndChangesWithSource()
    {
        var first = PromoBarHtmlRenderer.BuildPromoBarHtml("<p>a</p>", "same source", null);
        var second = PromoBarHtmlRenderer.BuildPromoBarHtml("<p>b</p>", "same source", null);
        var third = PromoBarHtmlRenderer.BuildPromoBarHtml("<p>a</p>", "different source", null);

        static string ExtractId(string html)
        {
            var marker = "data-promo-id=\"";
            var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
            return html[start..html.IndexOf('"', start)];
        }

        Assert.Equal(ExtractId(first), ExtractId(second));
        Assert.NotEqual(ExtractId(first), ExtractId(third));
        Assert.Equal(12, ExtractId(first).Length);
    }

    [Fact]
    public void BuildPromoBarHtml_NoNonce_OmitsNonceAttribute()
    {
        var html = PromoBarHtmlRenderer.BuildPromoBarHtml("<p>x</p>", "x", null);
        Assert.Contains("<script>", html);
        Assert.DoesNotContain("nonce=", html);
    }
}
