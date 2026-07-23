using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Bark.Configuration;
using Bark.Serialization;

namespace Bark.Services;

public static class StaticSiteExporter
{
    public static async Task RunAsync(WebApplication app, string outputDir, string? baseUrl, CancellationToken cancellationToken)
    {
        app.Urls.Clear();
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync(cancellationToken);

        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.First();

        using var client = new HttpClient { BaseAddress = new Uri(address) };

        var docs = app.Services.GetRequiredService<DocumentationService>();
        var pages = await docs.GetAllPagesAsync(cancellationToken);

        Directory.CreateDirectory(outputDir);

        var originPrefix = address.TrimEnd('/');
        var publicPrefix = string.IsNullOrEmpty(baseUrl) ? null : baseUrl.TrimEnd('/');

        foreach (var page in pages)
        {
            var requestPath = page.Path == "index" ? "/" : $"/{page.Path}";
            var html = await client.GetStringAsync(requestPath, cancellationToken);
            if (publicPrefix is not null)
                html = html.Replace(originPrefix, publicPrefix);
            var targetFile = page.Path == "index"
                ? Path.Combine(outputDir, "index.html")
                : Path.Combine(outputDir, page.Path, "index.html");
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            await File.WriteAllTextAsync(targetFile, html, cancellationToken);
        }

        foreach (var extra in new[] { "robots.txt", "llms.txt", "sitemap.xml" })
        {
            var content = await client.GetStringAsync($"/{extra}", cancellationToken);
            if (publicPrefix is not null)
                content = content.Replace(originPrefix, publicPrefix);
            await File.WriteAllTextAsync(Path.Combine(outputDir, extra), content, cancellationToken);
        }

        var notFoundResponse = await client.GetAsync("/__bark_export_404__", cancellationToken);
        var notFoundHtml = await notFoundResponse.Content.ReadAsStringAsync(cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "404.html"), notFoundHtml, cancellationToken);

        // Search has no server on a static host, so ship the prebuilt index the client queries directly.
        var searchIndex = docs.GetSearchIndexExport();
        var searchJson = JsonSerializer.Serialize(searchIndex, BarkJsonContext.Default.SearchIndexExport);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "search-index.json"), searchJson, cancellationToken);
        app.Logger.LogInformation(
            "Static search index written: {Docs} docs, {Bytes:N0} bytes",
            searchIndex.Docs.Count, System.Text.Encoding.UTF8.GetByteCount(searchJson));

        CopyStaticAssets(app.Environment.WebRootPath, outputDir);

        // The /assets route is served from docs/assets at runtime, outside wwwroot; mirror it into the export.
        var assetsDir = Path.Combine(Path.GetFullPath(app.Services.GetRequiredService<DocsOptions>().RootPath), "assets");
        CopyStaticAssets(assetsDir, Path.Combine(outputDir, "assets"));

        await app.StopAsync(cancellationToken);
    }

    private static void CopyStaticAssets(string sourceRoot, string outputDir)
    {
        if (!Directory.Exists(sourceRoot)) return;

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, file);
            var dest = Path.Combine(outputDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }
}
