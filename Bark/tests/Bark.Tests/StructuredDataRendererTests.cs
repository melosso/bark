using System.Text.Json;
using Bark.Models;
using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class StructuredDataRendererTests
{
    private const string Origin = "https://docs.example.com";
    private const string Url = "https://docs.example.com/guide/install/";

    private static JsonElement ParseGraph(string html)
    {
        var start = html.IndexOf('>') + 1;
        var end = html.LastIndexOf("</script>", StringComparison.Ordinal);
        var json = html[start..end];
        return JsonDocument.Parse(json).RootElement;
    }

    [Fact]
    public void NoCanonical_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, StructuredDataRenderer.BuildJsonLd(null, "T", "d", false, Origin, "", []));
    }

    [Fact]
    public void Home_EmitsWebSite()
    {
        var html = StructuredDataRenderer.BuildJsonLd(Url, "Home", "Welcome", isHomePage: true, Origin, "", [], siteName: "Bark");

        Assert.Contains("application/ld+json", html);
        var graph = ParseGraph(html).GetProperty("@graph");
        Assert.Contains(graph.EnumerateArray(), n => n.GetProperty("@type").GetString() == "WebSite");
    }

    [Fact]
    public void NonHome_EmitsArticleWithDatesAndImage()
    {
        var when = new DateTime(2026, 7, 21, 10, 30, 0, DateTimeKind.Utc);
        var html = StructuredDataRenderer.BuildJsonLd(Url, "Install", "How to", false, Origin, "", [], imageUrl: "https://docs.example.com/og.png", siteName: "Bark", modified: when);

        var article = ParseGraph(html).GetProperty("@graph").EnumerateArray()
            .Single(n => n.GetProperty("@type").GetString() == "Article");
        Assert.Equal("Install", article.GetProperty("headline").GetString());
        Assert.Equal("2026-07-21T10:30:00Z", article.GetProperty("dateModified").GetString());
        Assert.Equal("https://docs.example.com/og.png", article.GetProperty("image").GetString());
    }

    [Fact]
    public void Crumbs_EmitBreadcrumbListWithPositions()
    {
        IReadOnlyList<BreadcrumbItem> crumbs =
        [
            new("Home", "/"),
            new("Guide", "/guide"),
            new("Install", "/guide/install")
        ];

        var html = StructuredDataRenderer.BuildJsonLd(Url, "Install", "d", false, Origin, "", crumbs);

        var list = ParseGraph(html).GetProperty("@graph").EnumerateArray()
            .Single(n => n.GetProperty("@type").GetString() == "BreadcrumbList");
        var items = list.GetProperty("itemListElement");
        Assert.Equal(3, items.GetArrayLength());
        Assert.Equal(1, items[0].GetProperty("position").GetInt32());
        Assert.Equal("https://docs.example.com/guide", items[1].GetProperty("item").GetString());
        Assert.Equal(Url, items[2].GetProperty("item").GetString());
    }

    [Fact]
    public void Nonce_IsEmittedOnScriptTag()
    {
        var html = StructuredDataRenderer.BuildJsonLd(Url, "Home", "d", true, Origin, "", [], nonce: "abc");
        Assert.Contains("<script type=\"application/ld+json\" nonce=\"abc\">", html);
    }

    [Fact]
    public void EmittedJson_IsValid()
    {
        var html = StructuredDataRenderer.BuildJsonLd(Url, "Install", "d", false, Origin, "", [new("Home", "/"), new("Install", "/guide/install")]);
        var root = ParseGraph(html);
        Assert.Equal("https://schema.org", root.GetProperty("@context").GetString());
    }
}
