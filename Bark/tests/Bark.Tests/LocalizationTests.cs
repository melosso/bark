using Microsoft.Extensions.Logging.Abstractions;
using Bark.Configuration;
using Bark.Services;
using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class LocalizationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DocumentationService _service;

    public LocalizationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "bark-locale-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "locale"));
        File.WriteAllText(Path.Combine(_tempDir, "index.md"), "---\ntitle: Home\n---\n\n# Home\n");

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

    private void WriteConfig(string json) => File.WriteAllText(Path.Combine(_tempDir, "config.json"), json);

    private void WriteLocale(string code, string json) =>
        File.WriteAllText(Path.Combine(_tempDir, "locale", $"{code}.json"), json);

    [Fact]
    public async Task ConfiguredLocale_OverridesTheStringsItNames()
    {
        WriteConfig("""{ "locale": "nl" }""");
        WriteLocale("nl", """{ "tocTitle": "Op Deze Pagina" }""");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal("nl", _service.Locales.Root.Code);
        Assert.Equal("Op Deze Pagina", _service.Locales.Root.TocTitle);
    }

    [Fact]
    public async Task KeysTheLocaleOmits_KeepTheirEnglishText()
    {
        WriteConfig("""{ "locale": "nl" }""");
        WriteLocale("nl", """{ "tocTitle": "Op Deze Pagina" }""");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal(Localization.Default.PagerNext, _service.Locales.Root.PagerNext);
    }

    [Fact]
    public async Task EmptyValues_KeepTheirEnglishText()
    {
        WriteConfig("""{ "locale": "nl" }""");
        WriteLocale("nl", """{ "pagerNext": "" }""");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal(Localization.Default.PagerNext, _service.Locales.Root.PagerNext);
    }

    [Fact]
    public async Task CorruptLocaleFile_FallsBackToEnglish()
    {
        WriteConfig("""{ "locale": "nl" }""");
        WriteLocale("nl", "{ not json");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal(Localization.Default.TocTitle, _service.Locales.Root.TocTitle);
    }

    [Fact]
    public async Task MissingLocaleFile_FallsBackToEnglish()
    {
        WriteConfig("""{ "locale": "fr" }""");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal(Localization.Default.TocTitle, _service.Locales.Root.TocTitle);
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("n")]
    [InlineData("nl/../../secret")]
    [InlineData("toolongtobealocale")]
    public async Task LocaleCodeThatIsNotAShortToken_FallsBackToEnglish(string code)
    {
        WriteConfig($$"""{ "locale": "{{code}}" }""");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal("en", _service.Locales.Root.Code);
    }

    [Fact]
    public async Task LangIsUsedWhenLocaleIsAbsent()
    {
        WriteConfig("""{ "lang": "nl" }""");
        WriteLocale("nl", """{ "pagerNext": "Volgende" }""");

        await _service.StartAsync(CancellationToken.None);

        Assert.Equal("Volgende", _service.Locales.Root.PagerNext);
    }
}
