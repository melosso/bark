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
using Bark.Services.MarkdownExtensions;

Directory.CreateDirectory("log");

string? exportDir = null;
string? exportBaseUrl = null;
string? basePathArg = null;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--export" when i + 1 < args.Length: exportDir = args[++i]; break;
        case "--base-url" when i + 1 < args.Length: exportBaseUrl = args[++i]; break;
        case "--base-path" when i + 1 < args.Length: basePathArg = args[++i]; break;
    }
}

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog();

    var docsOptions = builder.Configuration.GetSection("Docs").Get<DocsOptions>() ?? new DocsOptions();
    if (exportDir != null)
        // No server survives the export, so there's nothing for the hot-reload poll baked into
        // the exported HTML to talk to. Disable it instead of baking in a dead endpoint call.
        docsOptions = docsOptions with { EnableHotReload = false };

    builder.Services.AddSingleton(docsOptions);

    var basePath = NormalizeBasePath(basePathArg ?? docsOptions.BasePath);

    // appsettings.json's Docs:Themes wins if present; theme.json is the file-only alternative.
    var themeOptions = builder.Configuration.GetSection("Docs:Themes").Get<ThemeOptions>()
        ?? ThemeJsonLoader.Load(builder.Environment.WebRootPath)
        ?? new ThemeOptions();
    builder.Services.AddSingleton(themeOptions);

    builder.Services.AddSingleton<ISyntaxHighlighter, TextMateSyntaxHighlighter>();
    builder.Services.AddSingleton(sp => new MarkdownService(sp.GetRequiredService<ISyntaxHighlighter>(), basePath));
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

    // Must finish before DocumentationService's StartAsync renders pages.
    await app.Services.GetRequiredService<ISyntaxHighlighter>().InitializeAsync(CancellationToken.None);

    if (basePath.Length > 0)
        app.UsePathBase(basePath);

    app.UseResponseCompression();
    app.UseStaticFiles();

    // Pins routing after static files -- without this, the catch-all "/{**path}" route would
    // match first and StaticFileMiddleware would never get a chance to serve wwwroot assets.
    app.UseRouting();

    // Drop files at wwwroot/theme/custom.{css,js} and they're picked up at startup, no config edit
    // needed. Unlike docs/bark.json, a newly-added file here needs a restart.
    var themeDir = Path.Combine(app.Environment.WebRootPath, "theme");
    var autoCustomCssUrl = File.Exists(Path.Combine(themeDir, "custom.css")) ? $"{basePath}/theme/custom.css" : null;
    var autoCustomJsUrl = File.Exists(Path.Combine(themeDir, "custom.js")) ? $"{basePath}/theme/custom.js" : null;

    app.MapGet("/api/build-version", (HttpContext context, DocumentationService docs) =>
    {
        // "no-store" not just "no-cache" -- the hot-reload poll needs the live value every time.
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        return Results.Ok(new { version = docs.BuildVersion });
    });

    app.MapGet("/api/search", (string? q, DocumentationService docs) =>
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Results.Ok(Array.Empty<SearchResult>());

        var results = docs.Search(q);
        return Results.Ok(results);
    });

    app.MapGet("/robots.txt", (HttpContext context) =>
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var body = $"User-agent: *\nAllow: /\nSitemap: {baseUrl}{basePath}/sitemap.xml\n";
        return Results.Text(body, "text/plain", Encoding.UTF8);
    });

    app.MapGet("/llms.txt", async (DocumentationService docs, HttpContext context) =>
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var pages = await docs.GetAllPagesAsync();
        var config = docs.Config;
        var sb = new StringBuilder();
        sb.AppendLine($"# {config?.Brand ?? "Bark"}");
        sb.AppendLine();
        foreach (var page in pages.OrderBy(p => p.Path))
        {
            var url = page.Path == "index" ? $"{baseUrl}{basePath}/" : $"{baseUrl}{basePath}/{page.Path}/";
            var line = $"- [{page.Title}]({url})";
            if (!string.IsNullOrWhiteSpace(page.Description))
                line += $": {page.Description}";
            sb.AppendLine(line);
        }

        return Results.Text(sb.ToString(), "text/plain", Encoding.UTF8);
    });

    app.MapGet("/sitemap.xml", async (DocumentationService docs) =>
    {
        var pages = await docs.GetAllPagesAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        sb.AppendLine($"  <url><loc>{Href(basePath, "")}</loc><priority>1.0</priority></url>");

        foreach (var page in pages.OrderBy(p => p.Path))
        {
            if (page.Path == "index") continue;
            var lastMod = page.LastModified?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
            sb.AppendLine($"  <url><loc>{Href(basePath, page.Path)}</loc><lastmod>{lastMod}</lastmod><priority>0.8</priority></url>");
        }


        sb.AppendLine("</urlset>");
        return Results.Content(sb.ToString(), "application/xml", Encoding.UTF8);
    });

    app.MapGet("/{**path}", async (string? path, DocumentationService docs, MarkdownService markdown, HttpContext context) =>
    {
        var isRootRequest = path == null || path == "" || path == "/";
        if (isRootRequest)
            path = docsOptions.DefaultPage ?? "index";

        path = (path ?? "").Trim('/');

        var page = await docs.GetPageAsync(path);
        if (page == null && isRootRequest)
            page = await BuildSafeRootPage(docs, basePath);

        if (page == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode, basePath));
            return;
        }

        // Folds inn the BuildVersion (not limited to this page's own HTML) so content edits don't touch this page's content still invalidate its cached ETag.
        var responseBytes = Encoding.UTF8.GetBytes(page.HtmlContent);
        var etagInput = Encoding.UTF8.GetBytes(docs.BuildVersion + ":").Concat(responseBytes).ToArray();
        var etag = Convert.ToBase64String(SHA256.HashData(etagInput)).TrimEnd('=');
        context.Response.Headers.ETag = $"\"{etag}\"";
        context.Response.Headers.CacheControl = "no-cache";

        if (context.Request.Headers.IfNoneMatch.ToString() == $"\"{etag}\"")
        {
            context.Response.StatusCode = 304;
            return;
        }

        var nav = await docs.GetNavigationAsync();
        var config = docs.Config;
        var navHtml = BuildNavigationHtml(nav, path, config, basePath);
        var topNavHtml = BuildTopNavHtml(config?.TopNav, path, basePath);
        var mobileTopNavHtml = BuildMobileTopNavHtml(config?.TopNav, path, basePath);

        var tocHtml = BuildTocHtml(page.Headings);

        var crumbs = docs.GetBreadcrumbs(path);
        var breadcrumbHtml = BuildBreadcrumbHtml(crumbs, page.Title, basePath);

        // Making sure home pages (layout: home) never get prev/next pagination!!
        var isHomePage = page.Layout == "home";
        var paginationHtml = string.Empty;
        if (!isHomePage)
        {
            var orderedPaths = GetOrderedPaths(nav, config, path).Where(p => p != null && p != "index").ToList();
            var currentIndex = orderedPaths.IndexOf(path);
            string? prevPath = currentIndex > 0 ? orderedPaths[currentIndex - 1] : null;
            string? nextPath = currentIndex < orderedPaths.Count - 1 ? orderedPaths[currentIndex + 1] : null;
            string? prevTitle = prevPath != null ? (await docs.GetPageAsync(prevPath))?.Title : null;
            string? nextTitle = nextPath != null ? (await docs.GetPageAsync(nextPath))?.Title : null;

            paginationHtml = BuildPaginationHtml(prevTitle, prevPath, nextTitle, nextPath, basePath);
        }

        var themeCss = ThemeProvider.BuildThemeCss(themeOptions);
        var customCssLink = ThemeProvider.BuildCustomCssLink(themeOptions, autoCustomCssUrl);
        var customJsScript = ThemeProvider.BuildCustomJsScript(autoCustomJsUrl);
        var brandText = config?.Brand ?? ThemeProvider.GetBrandText(themeOptions);
        var combinedThemeCss = themeCss + customCssLink + customJsScript;

        var socialLinksHtml = BuildSocialLinksHtml(config?.SocialLinks);
        var footerHtml = config?.Footer is { } footer
            ? $"<div class=\"content-footer\">{markdown.ToHtml(footer)}</div>"
            : string.Empty;

        var lastUpdatedHtml = !isHomePage && config?.LastUpdated == true && page.ShowLastUpdated && page.LastModified is { } lastModified
            ? $"<div class=\"last-updated\">Last updated: {lastModified:yyyy-MM-dd}</div>"
            : string.Empty;

        const string editLinkIcon = "<svg class=\"edit-link-icon\" viewBox=\"0 0 24 24\" width=\"16\" height=\"16\" fill=\"currentColor\" aria-hidden=\"true\">" +
            "<path d=\"M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z\"/></svg>";

        var editLinkHtml = !isHomePage && config?.EditLink is { Pattern: { Length: > 0 } pattern } editLink
            ? $"<a class=\"edit-link\" href=\"{LayoutProvider.HtmlEncode(pattern.Replace(":path", $"{page.Path}.md"))}\" " +
              $"target=\"_blank\" rel=\"noopener noreferrer\">{editLinkIcon}{LayoutProvider.HtmlEncode(editLink.Text)}</a>"
            : string.Empty;

        var fullHtml = LayoutProvider.GetLayout(
            title: page.Title,
            content: page.HtmlContent,
            navigationHtml: navHtml,
            topNavHtml: topNavHtml,
            mobileTopNavHtml: mobileTopNavHtml,
            tocHtml: tocHtml,
            breadcrumbHtml: breadcrumbHtml,
            paginationHtml: paginationHtml,
            themeCss: combinedThemeCss,
            brandText: brandText,
            enableDarkMode: ThemeProvider.UseDarkMode(themeOptions),
            showScrollIndicator: ThemeProvider.ShowScrollIndicator(themeOptions),
            footerHtml: footerHtml,
            socialLinksHtml: socialLinksHtml,
            enableLiveReload: docsOptions.EnableHotReload,
            buildVersion: docs.BuildVersion,
            favicon: config?.Favicon,
            description: page.Description,
            isHomePage: isHomePage,
            lastUpdatedHtml: lastUpdatedHtml,
            editLinkHtml: editLinkHtml,
            basePath: basePath
        );

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(fullHtml);
    });

    if (exportDir != null)
    {
        await StaticSiteExporter.RunAsync(app, exportDir, exportBaseUrl, CancellationToken.None);
        Log.Information("Static export written to {Dir}", exportDir);
        return;
    }

    var urls = app.Urls.Count > 0
        ? app.Urls.ToArray()
        : (Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
            ?? builder.Configuration["urls"]
            ?? "http://localhost:5000").Split(';');

    foreach (var rawUrl in urls)
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

    Log.Information("Application is hosted on the following URLs:");
    foreach (var url in urls)
    {
        Log.Information("   {Url}", url.Trim());
        Log.Information("");
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
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

static async Task<DocumentationPage> BuildSafeRootPage(DocumentationService docs, string basePath)
{
    var pages = await docs.GetAllPagesAsync();
    var linksHtml = pages.Count > 0
        ? "<ul>" + string.Join("", pages.OrderBy(p => p.Path)
            .Select(p => $"<li><a href=\"{Href(basePath, p.Path)}\">{LayoutProvider.HtmlEncode(p.Title)}</a></li>")) + "</ul>"
        : "<p>No Markdown files found yet. Drop one into your docs folder to get started.</p>";

    var html = $"""
        <h1>No homepage yet</h1>
        <p>Create <code>index.md</code> in your docs folder to set what's shown here.</p>
        {linksHtml}
        """;

    return new DocumentationPage(
        Path: "index",
        Title: "No homepage yet",
        HtmlContent: html,
        Description: null,
        LastModified: null,
        Headings: []
    );
}

static string NormalizeBasePath(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return "";
    var trimmed = "/" + raw.Trim().Trim('/');
    return trimmed == "/" ? "" : trimmed;
}

// Joins basePath + path for use in an href attribute. Empty path means "site root".
// Trailing slash matters: GitHub Pages (and most static hosts) serve "foo/index.html" only
// for a request to "/foo/", not "/foo" -- there's no slash-less directory fallback.
static string Href(string basePath, string path)
{
    var trimmed = path.Trim('/');
    return trimmed.Length == 0
        ? (basePath.Length == 0 ? "/" : $"{basePath}/")
        : $"{basePath}/{LayoutProvider.HtmlEncode(trimmed)}/";
}

static string BuildNavigationHtml(NavigationNode node, string currentPath, BarkConfig? config, string basePath)
{
    if (config?.Sidebar is { Count: > 0 } sidebars)
    {
        var matchedSections = SidebarResolver.Resolve(sidebars, currentPath);
        if (matchedSections is not null)
            return BuildNavFromConfig(matchedSections, currentPath, basePath);
    }

    if (config?.Nav is { Count: > 0 } sections)
        return BuildNavFromConfig(sections, currentPath, basePath);

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
                html.AppendLine($"<a href=\"{Href(basePath, sub.Path ?? "")}\">{LayoutProvider.HtmlEncode(sub.Title)}</a>");
                html.AppendLine("</li>");
            }
            html.AppendLine("</ul>");
            html.AppendLine("</div>");
        }
    }

    html.AppendLine("</div>");
    return html.ToString();
}

static string BuildNavFromConfig(List<NavEntry> entries, string currentPath, string basePath)
{
    var html = new StringBuilder();
    html.AppendLine("<div class=\"sidebar-tree\">");
    foreach (var entry in entries)
        AppendSidebarEntry(html, entry, currentPath, level: 0, basePath);
    html.AppendLine("</div>");
    return html.ToString();
}

static bool SidebarPathMatches(string entryPath, string currentPath)
{
    var normalized = entryPath.Trim('/').ToLowerInvariant();
    return normalized == currentPath || (normalized.Length == 0 && currentPath == "index");
}

static bool ContainsActiveDescendant(NavEntry entry, string currentPath)
{
    if (entry.Path is not null && SidebarPathMatches(entry.Path, currentPath))
        return true;

    return entry.Items?.Any(child => ContainsActiveDescendant(child, currentPath)) ?? false;
}

static void AppendSidebarEntry(StringBuilder html, NavEntry entry, string currentPath, int level, string basePath)
{
    if (entry.Items is not { Count: > 0 } children)
    {
        var isActive = SidebarPathMatches(entry.Path ?? string.Empty, currentPath);
        var href = Href(basePath, entry.Path ?? string.Empty);
        html.AppendLine(
            $"<div class=\"sidebar-link level-{level}{(isActive ? " is-active" : "")}\">" +
            $"<a href=\"{href}\">{LayoutProvider.HtmlEncode(entry.Title)}</a></div>");
        return;
    }

    var hasActiveDescendant = ContainsActiveDescendant(entry, currentPath);
    var isCollapsible = entry.Collapsed.HasValue;
    var startsOpen = !isCollapsible || entry.Collapsed == false || hasActiveDescendant;
    var headerClass = $"sidebar-group-title level-{level}{(hasActiveDescendant ? " has-active" : "")}";
    var headingTag = level == 0 ? "h2" : "h3";

    const string caretSvg = "<span class=\"caret-icon\"><svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
        "stroke-width=\"2\" aria-hidden=\"true\"><path d=\"M9 6l6 6-6 6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg></span>";

    // .sidebar-group-title stays a plain <div>; UA-default <summary> styling can't be fully overridden, so <summary> only wraps it as a near-invisible click target.
    if (isCollapsible)
        html.AppendLine($"<details class=\"sidebar-group\"{(startsOpen ? " open" : "")}>")
            .AppendLine("<summary class=\"sidebar-group-summary\">")
            .AppendLine($"<div class=\"{headerClass}\"><{headingTag}>{LayoutProvider.HtmlEncode(entry.Title)}</{headingTag}>{caretSvg}</div>")
            .AppendLine("</summary>");
    else
        html.AppendLine("<div class=\"sidebar-group no-caret\">")
            .AppendLine($"<div class=\"{headerClass}\"><{headingTag}>{LayoutProvider.HtmlEncode(entry.Title)}</{headingTag}></div>");

    html.AppendLine("<div class=\"sidebar-group-items\">");
    foreach (var child in children)
        AppendSidebarEntry(html, child, currentPath, level + 1, basePath);
    html.AppendLine("</div>");
    html.AppendLine(isCollapsible ? "</details>" : "</div>");
}

static string BuildTopNavHtml(List<TopNavItem>? topNav, string currentPath, string basePath)
{
    if (topNav is null || topNav.Count == 0)
        return string.Empty;

    var html = new StringBuilder();
    html.AppendLine("<nav class=\"top-nav\" aria-label=\"Main navigation\">");
    foreach (var item in topNav)
        AppendTopNavItem(html, item, currentPath, isMobile: false, basePath);
    html.AppendLine("</nav>");
    return html.ToString();
}

static string BuildMobileTopNavHtml(List<TopNavItem>? topNav, string currentPath, string basePath)
{
    if (topNav is null || topNav.Count == 0)
        return string.Empty;

    var html = new StringBuilder();
    html.AppendLine("<nav class=\"mobile-top-nav\" aria-label=\"Main navigation\">");
    foreach (var item in topNav)
        AppendTopNavItem(html, item, currentPath, isMobile: true, basePath);
    html.AppendLine("</nav>");
    return html.ToString();
}

static void AppendTopNavItem(StringBuilder html, TopNavItem item, string currentPath, bool isMobile, string basePath)
{
    if (item.Items is { Count: > 0 } children)
    {
        if (isMobile)
        {
            html.AppendLine("<details class=\"mobile-top-nav-group\">");
            html.AppendLine($"<summary>{LayoutProvider.HtmlEncode(item.Text)}</summary>");
            foreach (var child in children)
                AppendTopNavLink(html, child, currentPath, "mobile-top-nav-link", basePath);
            html.AppendLine("</details>");
        }
        else
        {
            html.AppendLine("<div class=\"top-nav-item has-dropdown\">");
            html.AppendLine($"<button type=\"button\" class=\"top-nav-link\">{LayoutProvider.HtmlEncode(item.Text)} " +
                "<svg class=\"top-nav-chevron\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" aria-hidden=\"true\"><path d=\"M6 9l6 6 6-6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg></button>");
            html.AppendLine("<div class=\"top-nav-dropdown-menu\">");
            foreach (var child in children)
                AppendTopNavLink(html, child, currentPath, "top-nav-dropdown-link", basePath);
            html.AppendLine("</div>");
            html.AppendLine("</div>");
        }
        return;
    }

    AppendTopNavLink(html, item, currentPath, isMobile ? "mobile-top-nav-link" : "top-nav-link", basePath, wrapInItemDiv: !isMobile);
}

static void AppendTopNavLink(StringBuilder html, TopNavItem item, string currentPath, string cssClass, string basePath, bool wrapInItemDiv = false)
{
    var link = item.Link ?? "#";
    var isExternal = link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                      link.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    var normalizedLink = isExternal ? link : Href(basePath, link);
    var isActive = !isExternal &&
        link.Trim('/').Equals(currentPath.Trim('/'), StringComparison.OrdinalIgnoreCase);
    var activeClass = isActive ? " active" : "";
    var relAttr = isExternal ? " target=\"_blank\" rel=\"noopener noreferrer\"" : "";

    const string externalIcon = "<svg class=\"external-link-icon\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
        "stroke-width=\"2\" aria-hidden=\"true\"><path d=\"M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>" +
        "<path d=\"M15 3h6v6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/><path d=\"M10 14 21 3\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>";

    if (wrapInItemDiv)
        html.AppendLine("<div class=\"top-nav-item\">");

    html.AppendLine(
        $"<a href=\"{LayoutProvider.HtmlEncode(normalizedLink)}\" class=\"{cssClass}{activeClass}\"{relAttr}>" +
        $"{LayoutProvider.HtmlEncode(item.Text)}{(isExternal ? externalIcon : "")}</a>");

    if (wrapInItemDiv)
        html.AppendLine("</div>");
}

static string ToDisplayName(string name)
{
    if (string.IsNullOrEmpty(name)) return name;
    var display = name.Replace('-', ' ').Replace('_', ' ');
    return char.ToUpperInvariant(display[0]) + display[1..];
}

// Excludes the page's own H1; everything else collapses onto a 3-level scale (deeper headings
// flatten onto level 3) so the TOC never grows a fourth visual indent.
static string BuildTocHtml(IReadOnlyList<HeadingInfo> headings)
{
    var items = headings.Where(h => h.Level >= 2).ToList();
    if (items.Count == 0)
        return string.Empty;

    var minLevel = items.Min(h => h.Level);
    var roots = BuildTocTree(items, minLevel);

    var html = new StringBuilder();
    foreach (var root in roots)
        AppendTocNode(html, root);
    return html.ToString();
}

static List<TocNode> BuildTocTree(IReadOnlyList<HeadingInfo> items, int minLevel)
{
    var roots = new List<TocNode>();
    var stack = new List<(int Depth, TocNode Node)>();

    foreach (var heading in items)
    {
        var depth = Math.Min(heading.Level - minLevel + 1, 3);
        var node = new TocNode(heading);

        while (stack.Count > 0 && stack[^1].Depth >= depth)
            stack.RemoveAt(stack.Count - 1);

        if (stack.Count == 0)
            roots.Add(node);
        else
            stack[^1].Node.Children.Add(node);

        stack.Add((depth, node));
    }

    return roots;
}

static void AppendTocNode(StringBuilder html, TocNode node)
{
    html.Append("<li class=\"toc-item\">");
    html.Append($"<a href=\"#{LayoutProvider.HtmlEncode(node.Heading.Id)}\">{LayoutProvider.HtmlEncode(node.Heading.Text)}</a>");

    if (node.Children.Count > 0)
    {
        html.Append("<ul class=\"toc-sublist\">");
        foreach (var child in node.Children)
            AppendTocNode(html, child);
        html.Append("</ul>");
    }

    html.Append("</li>");
}

static string BuildBreadcrumbHtml(IReadOnlyList<BreadcrumbItem> crumbs, string currentTitle, string basePath)
{
    var html = new StringBuilder();
    for (var i = 0; i < crumbs.Count - 1; i++)
    {
        var crumb = crumbs[i];
        html.Append($"<a href=\"{Href(basePath, crumb.Path ?? "")}\">{LayoutProvider.HtmlEncode(crumb.Title)}</a>");
        html.Append("<span class=\"separator\">/</span>");
    }
    html.Append($"<span class=\"current\">{LayoutProvider.HtmlEncode(currentTitle)}</span>");
    return html.ToString();
}

static string BuildPaginationHtml(string? prevTitle, string? prevPath, string? nextTitle, string? nextPath, string basePath)
{
    var html = new StringBuilder();
    html.AppendLine("<nav class=\"pagination\">");

    if (prevPath != null)
    {
        var prevUrl = prevPath == "index" ? Href(basePath, "") : Href(basePath, prevPath);
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
        var nextUrl = nextPath == "index" ? Href(basePath, "") : Href(basePath, nextPath);
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

// Prev/next must walk pages in the same order as whatever sidebar is actually showing for this
// page, not always the auto-generated folder tree -- same precedence as BuildNavigationHtml.
static List<string?> GetOrderedPaths(NavigationNode node, BarkConfig? config, string currentPath)
{
    if (config?.Sidebar is { Count: > 0 } sidebars)
    {
        var matchedSections = SidebarResolver.Resolve(sidebars, currentPath);
        if (matchedSections is not null)
            return FlattenNavEntries(matchedSections);
    }

    if (config?.Nav is { Count: > 0 } sections)
        return FlattenNavEntries(sections);

    return FlattenNavigation(node);
}

static List<string?> FlattenNavEntries(List<NavEntry> entries)
{
    var list = new List<string?>();
    foreach (var entry in entries)
    {
        if (entry.Path != null)
            list.Add(entry.Path.Trim('/'));
        if (entry.Items is { Count: > 0 } children)
            list.AddRange(FlattenNavEntries(children));
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
            "github" => "<svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z\"/></svg>",
            "mastodon" => "<svg viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"currentColor\" aria-hidden=\"true\"><path d=\"M23.268 5.313c0-3.491-2.292-4.51-2.292-4.51C19.528.247 17.648 0 12 0S4.472.247 3.024.803c0 0-2.292 1.019-2.292 4.51 0 1.129-.023 2.48.013 3.927.108 4.28.82 8.505 4.944 10.448 1.904.898 3.538 1.087 4.855.96 2.386-.23 3.727-.85 3.727-.85l-.08-1.768s-1.707.537-3.623.47c-1.89-.064-3.89-.205-4.197-2.526a4.777 4.777 0 01-.042-.708s1.88.458 4.27.566c1.448.065 2.806-.085 4.188-.25 2.64-.316 4.95-1.96 5.254-3.459.461-2.257.421-5.326.421-5.326zM19.74 13.41h-2.207V8.63c0-1.14-.48-1.718-1.44-1.718-1.062 0-1.594.687-1.594 2.044v2.96h-2.19V8.956c0-1.357-.532-2.044-1.594-2.044-.96 0-1.44.578-1.44 1.719v4.78H7.245V8.488c0-1.14.291-2.047.874-2.719.601-.672 1.389-1.017 2.363-1.017 1.13 0 1.986.434 2.547 1.302l.55.922.549-.922c.561-.868 1.417-1.302 2.547-1.302.974 0 1.762.345 2.363 1.017.583.672.874 1.578.874 2.719z\"/></svg>",
            _ => $"<span style=\"font-size:0.9rem\" aria-hidden=\"true\">{LayoutProvider.HtmlEncode(link.Icon)}</span>"
        };
        var tooltip = link.Title ?? link.Icon;
        var label = $"{tooltip} (opens in new tab)";
        html.AppendLine($"<a href=\"{LayoutProvider.HtmlEncode(link.Url)}\" class=\"icon-btn\" target=\"_blank\" rel=\"noopener noreferrer\" title=\"{LayoutProvider.HtmlEncode(tooltip)}\" aria-label=\"{LayoutProvider.HtmlEncode(label)}\">{icon}</a>");
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

sealed record TocNode(HeadingInfo Heading)
{
    public List<TocNode> Children { get; } = [];
}