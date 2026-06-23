using Microsoft.Extensions.Logging.Abstractions;
using Bark.Configuration;
using Bark.Models;
using Bark.Services;

namespace Bark.Tests;

public sealed class DocumentationServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DocumentationService _service;
    private readonly DocsOptions _options;
    private readonly MarkdownService _markdown;

    public DocumentationServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "bark-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        Directory.CreateDirectory(Path.Combine(_tempDir, "getting-started"));

        _options = new DocsOptions
        {
            RootPath = _tempDir,
            DefaultPage = "index",
            EnableHotReload = false
        };
        _markdown = new MarkdownService();
        _service = new DocumentationService(_options, _markdown, NullLogger<DocumentationService>.Instance);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task CreateTestFiles()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "index.md"),
            "---\ntitle: Home\ndescription: Welcome page\n---\n\n# Home Page\n\nWelcome to the docs.\n");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "getting-started", "installation.md"),
            "---\ntitle: Installation\n---\n\n# Installation Guide\n\nHow to install.\n");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "getting-started", "configuration.md"),
            "---\ntitle: Configuration\n---\n\n# Configuration Guide\n\nHow to configure.\n");
    }

    [Fact]
    public async Task StartAsync_BuildsPageCache()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var home = await _service.GetPageAsync("index");
        Assert.NotNull(home);
        Assert.Equal("Home", home!.Title);

        var install = await _service.GetPageAsync("getting-started/installation");
        Assert.NotNull(install);
        Assert.Equal("Installation", install!.Title);
    }

    [Fact]
    public async Task GetPageAsync_ReturnsNull_ForMissingPage()
    {
        await _service.StartAsync(CancellationToken.None);
        var page = await _service.GetPageAsync("nonexistent");
        Assert.Null(page);
    }

    [Fact]
    public async Task GetPageAsync_NormalizesPath()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var page = await _service.GetPageAsync("/Getting-Started/Installation/");
        Assert.NotNull(page);
        Assert.Equal("Installation", page!.Title);
    }

    [Fact]
    public async Task GetNavigationAsync_ReturnsTree()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var nav = await _service.GetNavigationAsync();
        Assert.NotNull(nav);

        var gettingStarted = nav.Children.FirstOrDefault(c => c.Title == "getting-started");
        Assert.NotNull(gettingStarted);
        Assert.Equal(2, gettingStarted!.Children.Count);
    }

    [Fact]
    public async Task GetAllPagesAsync_ReturnsAllPages()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var pages = await _service.GetAllPagesAsync();
        Assert.Equal(3, pages.Count);
    }

    [Fact]
    public async Task GetBreadcrumbs_ReturnsHomeAndSegments()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var crumbs = _service.GetBreadcrumbs("getting-started/installation");
        Assert.Equal(3, crumbs.Count);
        Assert.Equal("Home", crumbs[0].Title);
        Assert.Equal("Getting started", crumbs[1].Title);
        Assert.Equal("Installation", crumbs[2].Title);
    }

    [Fact]
    public async Task GetBreadcrumbs_RootPath_ReturnsOnlyHome()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var crumbs = _service.GetBreadcrumbs("index");
        Assert.Single(crumbs);
        Assert.Equal("Home", crumbs[0].Title);
    }

    [Fact]
    public async Task Search_ReturnsResults_AfterBuild()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var results = _service.Search("installation");
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Path == "getting-started/installation");
    }

    [Fact]
    public async Task Search_EmptyResult_ForNoMatch()
    {
        await CreateTestFiles();
        await _service.StartAsync(CancellationToken.None);

        var results = _service.Search("zzzzz_no_match_zzzzz");
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_ReturnsEmpty_BeforeBuild()
    {
        var results = _service.Search("anything");
        Assert.Empty(results);
    }
}
