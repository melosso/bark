using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

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

        foreach (var page in pages)
        {
            var requestPath = page.Path == "index" ? "/" : $"/{page.Path}";
            var html = await client.GetStringAsync(requestPath, cancellationToken);
            var targetFile = page.Path == "index"
                ? Path.Combine(outputDir, "index.html")
                : Path.Combine(outputDir, page.Path, "index.html");
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            await File.WriteAllTextAsync(targetFile, html, cancellationToken);
        }

        foreach (var extra in new[] { "robots.txt", "llms.txt", "sitemap.xml" })
        {
            var content = await client.GetStringAsync($"/{extra}", cancellationToken);
            // These bodies embed the loopback origin as their "absolute" base URL; swap it for
            // the real public origin so the exported files are correct once deployed.
            if (!string.IsNullOrEmpty(baseUrl))
                content = content.Replace(address.TrimEnd('/'), baseUrl.TrimEnd('/'));
            await File.WriteAllTextAsync(Path.Combine(outputDir, extra), content, cancellationToken);
        }

        var notFoundResponse = await client.GetAsync("/__bark_export_404__", cancellationToken);
        var notFoundHtml = await notFoundResponse.Content.ReadAsStringAsync(cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "404.html"), notFoundHtml, cancellationToken);

        CopyStaticAssets(app.Environment.WebRootPath, outputDir);

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
