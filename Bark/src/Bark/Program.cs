using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Bark.Configuration;
using Bark.Models;
using Bark.Services;

Directory.CreateDirectory("log");

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog();

    var docsOptions = builder.Configuration.GetSection("Docs").Get<DocsOptions>() ?? new DocsOptions();
    builder.Services.AddSingleton(docsOptions);

    var themeOptions = builder.Configuration.GetSection("Docs:Themes").Get<ThemeOptions>();
    builder.Services.AddSingleton(themeOptions ?? new ThemeOptions());

    builder.Services.AddSingleton<MarkdownService>();
    builder.Services.AddSingleton<DocumentationService>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<DocumentationService>());

    builder.Services.AddResponseCompression(opts =>
    {
        opts.EnableForHttps = true;
        opts.Providers.Add<BrotliCompressionProvider>();
        opts.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(opts =>
    {
        opts.Level = CompressionLevel.Fastest;
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(opts =>
    {
        opts.Level = CompressionLevel.Fastest;
    });

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.AddServerHeader = false;
        serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
        serverOptions.Limits.MaxRequestHeadersTotalSize = 32 * 1024;
        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
        serverOptions.Limits.MaxConcurrentConnections = 1000;
        serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
        serverOptions.Limits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
        serverOptions.Limits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
        serverOptions.Limits.Http2.MaxStreamsPerConnection = 100;
        serverOptions.Limits.Http2.MaxFrameSize = 16 * 1024;
        serverOptions.Limits.Http2.InitialConnectionWindowSize = 128 * 1024;
        serverOptions.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(30);
        serverOptions.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromSeconds(60);
    });

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole(opts => opts.FormatterName = "simple");
    builder.Logging.AddSimpleConsole(opts => opts.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ");

    builder.Services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

    LogApplicationBanner();

    var app = builder.Build();

    app.UseResponseCompression();
    app.UseStaticFiles();

    app.MapGet("/api/search", (string? q, DocumentationService docs) =>
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Results.Ok(Array.Empty<SearchResult>());

        var results = docs.Search(q);
        return Results.Ok(results);
    });

    app.MapGet("/sitemap.xml", async (DocumentationService docs) =>
    {
        var pages = await docs.GetAllPagesAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        sb.AppendLine("  <url><loc>/</loc><priority>1.0</priority></url>");

        foreach (var page in pages.OrderBy(p => p.Path))
        {
            if (page.Path == "index") continue;
            var lastMod = page.LastModified?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
            sb.AppendLine($"  <url><loc>/{LayoutProvider.HtmlEncode(page.Path)}</loc><lastmod>{lastMod}</lastmod><priority>0.8</priority></url>");
        }

        sb.AppendLine("</urlset>");
        return Results.Content(sb.ToString(), "application/xml", Encoding.UTF8);
    });

    app.MapGet("/{**path}", async (string? path, DocumentationService docs, MarkdownService markdown, HttpContext context) =>
    {
        if (path == null || path == "" || path == "/")
            path = docsOptions.DefaultPage ?? "index";

        path = path.Trim('/');

        var page = await docs.GetPageAsync(path);
        if (page == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode));
            return;
        }

        var responseBytes = Encoding.UTF8.GetBytes(page.HtmlContent);
        var etag = Convert.ToBase64String(SHA256.HashData(responseBytes)).TrimEnd('=');
        context.Response.Headers.ETag = $"\"{etag}\"";
        context.Response.Headers.CacheControl = "no-cache";

        if (context.Request.Headers.IfNoneMatch.ToString() == $"\"{etag}\"")
        {
            context.Response.StatusCode = 304;
            return;
        }

        var nav = await docs.GetNavigationAsync();
        var config = docs.Config;
        var navHtml = BuildNavigationHtml(nav, path, config);

        var tocHtml = string.Join("", page.Headings.Select(h =>
            $"<li class=\"toc-item\"><a href=\"#{LayoutProvider.HtmlEncode(h.Id)}\">{LayoutProvider.HtmlEncode(h.Text)}</a></li>"));

        var crumbs = docs.GetBreadcrumbs(path);
        var breadcrumbHtml = BuildBreadcrumbHtml(crumbs, page.Title);

        var orderedPaths = FlattenNavigation(nav).Where(p => p != null && p != "index").ToList();
        var currentIndex = orderedPaths.IndexOf(path);
        string? prevPath = currentIndex > 0 ? orderedPaths[currentIndex - 1] : null;
        string? nextPath = currentIndex < orderedPaths.Count - 1 ? orderedPaths[currentIndex + 1] : null;
        string? prevTitle = prevPath != null ? (await docs.GetPageAsync(prevPath))?.Title : null;
        string? nextTitle = nextPath != null ? (await docs.GetPageAsync(nextPath))?.Title : null;

        var paginationHtml = BuildPaginationHtml(prevTitle, prevPath, nextTitle, nextPath);

        var themeCss = ThemeProvider.BuildThemeCss(themeOptions);
        var customCssLink = ThemeProvider.BuildCustomCssLink(themeOptions);
        var brandText = config?.Brand ?? ThemeProvider.GetBrandText(themeOptions);
        var combinedThemeCss = themeCss + customCssLink;

        var socialLinksHtml = BuildSocialLinksHtml(config?.SocialLinks);
        var footerHtml = config?.Footer is { } footer
            ? $"<div class=\"content-footer\">{markdown.ToHtml(footer)}</div>"
            : string.Empty;

        var fullHtml = LayoutProvider.GetLayout(
            title: page.Title,
            content: page.HtmlContent,
            navigationHtml: navHtml,
            tocHtml: tocHtml,
            breadcrumbHtml: breadcrumbHtml,
            paginationHtml: paginationHtml,
            themeCss: combinedThemeCss,
            brandText: brandText,
            enableDarkMode: ThemeProvider.UseDarkMode(themeOptions),
            footerHtml: footerHtml,
            socialLinksHtml: socialLinksHtml
        );

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(fullHtml);
    });

    var urlsToCheck = app.Urls.Count > 0
        ? app.Urls
        : (Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
            ?? builder.Configuration["urls"]
            ?? "http://localhost:5000").Split(';');

    foreach (var rawUrl in urlsToCheck)
    {
        if (!Uri.TryCreate(rawUrl.Trim(), UriKind.Absolute, out var uri)) continue;
        if (uri.Scheme is not ("http" or "https")) continue;

        try
        {
            var address = uri.Host is "localhost" or "0.0.0.0" or "*" or "+"
                ? IPAddress.Loopback
                : IPAddress.Parse(uri.Host);

            using var probe = new TcpListener(address, uri.Port);
            probe.Start();
            probe.Stop();
        }
        catch (SocketException)
        {
            Log.Fatal("Port {Port} is already in use. Stop the existing process and try again.", uri.Port);
            return;
        }
    }

    var urls = app.Urls;
    if (urls.Count > 0)
    {
        Log.Information("Application is hosted on the following URLs:");
        foreach (var url in urls)
        {
            Log.Information("   {Url}", url);
        }
    }
    else
    {
        var serverUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
            ?? builder.Configuration["urls"]
            ?? "http://localhost:5000";
        Log.Information("Application is hosted on: {Urls}", serverUrls.Replace(";", "; "));
    }

    app.Lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("");
        Log.Information("Application shutting down...");
        Log.CloseAndFlush();
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal("");
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}

static string BuildNavigationHtml(NavigationNode node, string currentPath, BarkConfig? config = null)
{
    if (config?.Nav is { Count: > 0 } sections)
        return BuildNavFromConfig(sections, currentPath);

    if (node.Children.Count == 0) return string.Empty;

    var html = new StringBuilder();
    html.AppendLine("<div class=\"nav-groups\">");

    foreach (var child in node.Children)
    {
        if (child.Children.Count > 0)
        {
            html.AppendLine("<div class=\"nav-group\">");
            var displayTitle = ToDisplayName(child.Title);
            html.AppendLine($"<div class=\"nav-group-title\">{LayoutProvider.HtmlEncode(displayTitle)}</div>");
            html.AppendLine("<ul class=\"nav-list\">");
            foreach (var sub in child.Children.OrderBy(c => c.Title))
            {
                var isActive = sub.Path == currentPath;
                html.AppendLine($"<li class=\"nav-item{(isActive ? " active" : "")}\">");
                html.AppendLine($"<a href=\"/{LayoutProvider.HtmlEncode(sub.Path)}\">{LayoutProvider.HtmlEncode(sub.Title)}</a>");
                html.AppendLine("</li>");
            }
            html.AppendLine("</ul>");
            html.AppendLine("</div>");
        }
    }

    html.AppendLine("</div>");
    return html.ToString();
}

static string BuildNavFromConfig(List<NavSection> sections, string currentPath)
{
    var html = new StringBuilder();
    html.AppendLine("<div class=\"nav-groups\">");

    foreach (var section in sections)
    {
        html.AppendLine("<div class=\"nav-group\">");
        html.AppendLine($"<div class=\"nav-group-title\">{LayoutProvider.HtmlEncode(section.Section)}</div>");
        html.AppendLine("<ul class=\"nav-list\">");
        foreach (var item in section.Items)
        {
            var isActive = item.Path == currentPath;
            html.AppendLine($"<li class=\"nav-item{(isActive ? " active" : "")}\">");
            html.AppendLine($"<a href=\"/{LayoutProvider.HtmlEncode(item.Path)}\">{LayoutProvider.HtmlEncode(item.Title)}</a>");
            html.AppendLine("</li>");
        }
        html.AppendLine("</ul>");
        html.AppendLine("</div>");
    }

    html.AppendLine("</div>");
    return html.ToString();
}

static string ToDisplayName(string name)
{
    if (string.IsNullOrEmpty(name)) return name;
    var display = name.Replace('-', ' ').Replace('_', ' ');
    return char.ToUpperInvariant(display[0]) + display[1..];
}

static string BuildBreadcrumbHtml(IReadOnlyList<BreadcrumbItem> crumbs, string currentTitle)
{
    var html = new StringBuilder();
    for (var i = 0; i < crumbs.Count - 1; i++)
    {
        var crumb = crumbs[i];
        html.Append($"<a href=\"{LayoutProvider.HtmlEncode(crumb.Path)}\">{LayoutProvider.HtmlEncode(crumb.Title)}</a>");
        html.Append("<span class=\"separator\">/</span>");
    }
    html.Append($"<span class=\"current\">{LayoutProvider.HtmlEncode(currentTitle)}</span>");
    return html.ToString();
}

static string BuildPaginationHtml(string? prevTitle, string? prevPath, string? nextTitle, string? nextPath)
{
    var html = new StringBuilder();
    html.AppendLine("<nav class=\"pagination\">");

    if (prevPath != null)
    {
        var prevUrl = prevPath == "index" ? "/" : $"/{LayoutProvider.HtmlEncode(prevPath)}";
        html.AppendLine($"<a href=\"{prevUrl}\" class=\"pagination-link prev\">");
        html.AppendLine("<span class=\"label\">Previous</span>");
        html.AppendLine($"<span class=\"title\">{LayoutProvider.HtmlEncode(prevTitle)}</span>");
        html.AppendLine("</a>");
    }
    else
    {
        html.AppendLine("<span></span>");
    }

    if (nextPath != null)
    {
        var nextUrl = nextPath == "index" ? "/" : $"/{LayoutProvider.HtmlEncode(nextPath)}";
        html.AppendLine($"<a href=\"{nextUrl}\" class=\"pagination-link next\">");
        html.AppendLine("<span class=\"label\">Next</span>");
        html.AppendLine($"<span class=\"title\">{LayoutProvider.HtmlEncode(nextTitle)}</span>");
        html.AppendLine("</a>");
    }
    else
    {
        html.AppendLine("<span></span>");
    }

    html.AppendLine("</nav>");
    return html.ToString();
}

static List<string?> FlattenNavigation(NavigationNode node)
{
    var list = new List<string?>();
    foreach (var child in node.Children)
    {
        if (child.Path != null)
            list.Add(child.Path);
        if (child.Children.Count > 0)
            list.AddRange(FlattenNavigation(child));
    }
    return list;
}

static string BuildSocialLinksHtml(List<SocialLink>? links)
{
    if (links is not { Count: > 0 }) return string.Empty;

    var html = new StringBuilder();
    html.AppendLine("<div class=\"social-links\">");
    foreach (var link in links)
    {
        var icon = link.Icon.ToLowerInvariant() switch
        {
            "github" => "<svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"currentColor\"><path d=\"M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z\"/></svg>",
            "mastodon" => "<svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"currentColor\"><path d=\"M23.268 5.313c0-3.491-2.292-4.51-2.292-4.51C19.528.247 17.648 0 12 0S4.472.247 3.024.803c0 0-2.292 1.019-2.292 4.51 0 1.129-.023 2.48.013 3.927.108 4.28.82 8.505 4.944 10.448 1.904.898 3.538 1.087 4.855.96 2.386-.23 3.727-.85 3.727-.85l-.08-1.768s-1.707.537-3.623.47c-1.89-.064-3.89-.205-4.197-2.526a4.777 4.777 0 01-.042-.708s1.88.458 4.27.566c1.448.065 2.806-.085 4.188-.25 2.64-.316 4.95-1.96 5.254-3.459.461-2.257.421-5.326.421-5.326zM19.74 13.41h-2.207V8.63c0-1.14-.48-1.718-1.44-1.718-1.062 0-1.594.687-1.594 2.044v2.96h-2.19V8.956c0-1.357-.532-2.044-1.594-2.044-.96 0-1.44.578-1.44 1.719v4.78H7.245V8.488c0-1.14.291-2.047.874-2.719.601-.672 1.389-1.017 2.363-1.017 1.13 0 1.986.434 2.547 1.302l.55.922.549-.922c.561-.868 1.417-1.302 2.547-1.302.974 0 1.762.345 2.363 1.017.583.672.874 1.578.874 2.719z\"/></svg>",
            _ => $"<span style=\"font-size:0.9rem\">{LayoutProvider.HtmlEncode(link.Icon)}</span>"
        };
        var tooltip = link.Title ?? link.Icon;
        html.AppendLine($"<a href=\"{LayoutProvider.HtmlEncode(link.Url)}\" target=\"_blank\" rel=\"noopener noreferrer\" title=\"{LayoutProvider.HtmlEncode(tooltip)}\">{icon}</a>");
    }
    html.AppendLine("</div>");
    return html.ToString();
}

void LogApplicationBanner()
{
    Log.Information("");
    Log.Information("Bark - A simple documentation server built with ASP.NET Core");
    Log.Information("");
}