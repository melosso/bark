using System.Security.Cryptography;
using System.Text;

namespace Bark.Services.Rendering;

public static class PromoBarHtmlRenderer
{
    /// <summary>Builds the announcement bar; dismissal is keyed on a hash of the source markdown so editing the promo re-shows it</summary>
    public static string BuildPromoBarHtml(string? promoHtml, string promoSource, string? nonce)
    {
        if (string.IsNullOrWhiteSpace(promoHtml))
            return string.Empty;

        var promoId = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(promoSource)))[..12];
        var nonceAttr = nonce is { Length: > 0 } ? $" nonce=\"{nonce}\"" : "";

        // Inline check runs before the bar paints; dismissed users get no flash, no-JS users still see the bar
        var checkScript =
            $"<script{nonceAttr}>(function(){{try{{if(localStorage.getItem('bark-promo-dismissed')==='{promoId}')" +
            "document.documentElement.classList.add('promo-dismissed');}catch(e){}})();</script>";

        return checkScript +
            $"<div class=\"promo-bar\" id=\"promo-bar\" data-promo-id=\"{promoId}\" role=\"region\" aria-label=\"Announcement\">" +
            "<div class=\"promo-bar-inner\">" +
            $"<div class=\"promo-bar-content\">{promoHtml}</div>" +
            "<button type=\"button\" class=\"promo-bar-close icon-btn\" id=\"promo-bar-close\" aria-label=\"Dismiss announcement\">" +
            "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" aria-hidden=\"true\"><path d=\"M18 6L6 18M6 6l12 12\"/></svg>" +
            "</button></div></div>";
    }
}
