using System.Text.Json;
using Bark.Models;

namespace Bark.Tests;

public sealed class LocaleConfigTests
{
    [Theory]
    [InlineData("""{ "locale": "en" }""", "en")]
    [InlineData("""{ "locale": "nl" }""", "nl")]
    [InlineData("""{ "locale": { "code": "fr" } }""", "fr")]
    [InlineData("""{ "lang": "nl" }""", "nl")]
    [InlineData("""{ "locale": "en", "lang": "nl" }""", "en")]
    public void Parses_AllLocaleForms(string json, string expectedCode)
    {
        var config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Equal(expectedCode, Config.ResolveLocale(config)?.Code);
    }

    [Fact]
    public void NoLocaleConfig_ResolvesNull()
    {
        var config = JsonSerializer.Deserialize<Config>("""{ "title": "x" }""");

        Assert.Null(Config.ResolveLocale(config));
    }
}
