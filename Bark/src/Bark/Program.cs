using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System.Threading.RateLimiting;
using Bark.Configuration;
using Bark.Models;
using Bark.Services;
using Bark.Services.Layout;
using Bark.Services.MarkdownExtensions;
using Bark.Services.Rendering;

Directory.CreateDirectory("log");

var cliArgs = CliArguments.Parse(args);
var exportDir = cliArgs.ExportDir;
var exportBaseUrl = cliArgs.ExportBaseUrl;

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

    var basePath = NormalizeBasePath(cliArgs.BasePath ?? docsOptions.BasePath);

    // appsettings.json's Docs:Themes wins if present; theme.json is the file-only alternative.
    var themeOptions = builder.Configuration.GetSection("Docs:Themes").Get<ThemeOptions>()
        ?? ThemeJsonLoader.Load(builder.Environment.WebRootPath)
        ?? new ThemeOptions();
    builder.Services.AddSingleton(themeOptions);

    var codeGroupIconOptions = builder.Configuration.GetSection("Docs:CodeGroupIcons").Get<CodeGroupIconOptions>()
        ?? new CodeGroupIconOptions();
    builder.Services.AddSingleton(codeGroupIconOptions);

    builder.Services.AddSingleton<ISyntaxHighlighter, TextMateSyntaxHighlighter>();
    builder.Services.AddSingleton<MathRenderer>();
    builder.Services.AddSingleton(sp => new MarkdownService(
        sp.GetRequiredService<ISyntaxHighlighter>(), basePath,
        sp.GetRequiredService<CodeGroupIconOptions>(),
        sp.GetRequiredService<MathRenderer>()));
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

    builder.WebHost.ConfigureKestrel(KestrelHardening.Configure);

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole(opts => opts.FormatterName = "simple");
    builder.Logging.AddSimpleConsole(opts => opts.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ");

    builder.Services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("search-limit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    LogApplicationBanner();

    var app = builder.Build();

    // Must finish before DocumentationService's async renders the pages
    await app.Services.GetRequiredService<ISyntaxHighlighter>().InitializeAsync(CancellationToken.None);

    if (basePath.Length > 0)
        app.UsePathBase(basePath);

    var customCspRaw = builder.Configuration["Docs:ContentSecurityPolicy"];
    var customCsp = string.IsNullOrWhiteSpace(customCspRaw) ? null : customCspRaw;
    app.UseSecurityHeaders(customCsp);
    app.UseResponseCompression();

    var defaultWebRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot-default");
    if (Directory.Exists(defaultWebRoot))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new CompositeFileProvider(
                new PhysicalFileProvider(app.Environment.WebRootPath),
                new PhysicalFileProvider(defaultWebRoot)
            )
        });
    }
    else
    {
        app.UseStaticFiles();
    }

    app.UseRouting();
    app.UseRateLimiter();

    // Drop files at wwwroot/theme/custom.{css,js} and they're picked up at startup, no config edit needed. Does NOT support hot rloading.
    var themeDir = Path.Combine(app.Environment.WebRootPath, "theme");
    try { Directory.CreateDirectory(themeDir); } catch (IOException) { }
    var autoCustomCssUrl = File.Exists(Path.Combine(themeDir, "custom.css")) ? $"{basePath}/theme/custom.css" : null;
    var autoCustomJsUrl = File.Exists(Path.Combine(themeDir, "custom.js")) ? $"{basePath}/theme/custom.js" : null;

    app.MapGet("/api/build-version", (HttpContext context, DocumentationService docs) =>
    {
        // "no-store" not just "no-cache"; the hot-reload poll needs the live value every time.
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
    }).RequireRateLimiting("search-limit");

    app.MapGet("/robots.txt", (HttpContext context) =>
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var body = $"User-agent: *\nAllow: /\nSitemap: {baseUrl}{basePath}/sitemap.xml\n";
        return Results.Text(body, "text/plain", Encoding.UTF8);
    });

    app.MapGet("/llms.txt", async (DocumentationService docs, HttpContext context) =>
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var pages = await docs.GetAllPagesAsync(context.RequestAborted);
        var config = docs.SiteConfig;
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

    app.MapGet("/sitemap.xml", async (DocumentationService docs, HttpContext context) =>
    {
        var pages = await docs.GetAllPagesAsync(context.RequestAborted);
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        sb.AppendLine($"  <url><loc>{UrlPaths.Href(basePath, "")}</loc><priority>1.0</priority></url>");

        foreach (var page in pages.OrderBy(p => p.Path))
        {
            if (page.Path == "index") continue;
            var lastMod = page.LastModified?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
            sb.AppendLine($"  <url><loc>{UrlPaths.Href(basePath, page.Path)}</loc><lastmod>{lastMod}</lastmod><priority>0.8</priority></url>");
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

        var config = docs.SiteConfig;

        var page = await docs.GetPageAsync(path, context.RequestAborted);
        if (page == null && isRootRequest)
            page = await BuildSafeRootPage(docs, basePath, context.RequestAborted);

        if (page == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode, basePath, config?.Lang ?? "en"));
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

        var nav = await docs.GetNavigationAsync(context.RequestAborted);
        var navHtml = NavigationHtmlRenderer.BuildNavigationHtml(nav, path, config, basePath);
        var topNavHtml = NavigationHtmlRenderer.BuildTopNavHtml(config?.TopNav, path, basePath);
        var mobileTopNavHtml = NavigationHtmlRenderer.BuildMobileTopNavHtml(config?.TopNav, path, basePath);

        var tocHtml = TocHtmlRenderer.BuildTocHtml(page.Headings);

        var crumbs = await docs.GetBreadcrumbsAsync(path, context.RequestAborted);
        var breadcrumbHtml = BreadcrumbHtmlRenderer.BuildBreadcrumbHtml(crumbs, page.Title, basePath);

        var isHomePage = page.Layout == "home";
        var paginationHtml = string.Empty;
        if (!isHomePage)
        {
            var orderedPaths = NavigationHtmlRenderer.GetOrderedPaths(nav, config, path).Where(p => p != null && p != "index").ToList();
            var currentIndex = orderedPaths.IndexOf(path);
            string? prevPath = currentIndex > 0 ? orderedPaths[currentIndex - 1] : null;
            string? nextPath = currentIndex < orderedPaths.Count - 1 ? orderedPaths[currentIndex + 1] : null;
            string? prevTitle = prevPath != null ? (await docs.GetPageAsync(prevPath, context.RequestAborted))?.Title : null;
            string? nextTitle = nextPath != null ? (await docs.GetPageAsync(nextPath, context.RequestAborted))?.Title : null;

            paginationHtml = PaginationHtmlRenderer.BuildPaginationHtml(prevTitle, prevPath, nextTitle, nextPath, basePath);
        }

        var themeCss = ThemeProvider.BuildThemeCss(themeOptions);
        var customCssLink = ThemeProvider.BuildCustomCssLink(themeOptions, autoCustomCssUrl, basePath);
        var customJsScript = ThemeProvider.BuildCustomJsScript(themeOptions, autoCustomJsUrl, basePath);
        var brandText = config?.Brand ?? config?.Title ?? ThemeProvider.GetBrandText(themeOptions);
        var brandImage = config?.BrandImage;
        var combinedThemeCss = themeCss + customCssLink + customJsScript;

        var iconsDir = Path.Combine(app.Environment.WebRootPath, "icons");
        var defaultIconsDir = Path.Combine(AppContext.BaseDirectory, "wwwroot-default", "icons");
        var fallbackIconsDir = Directory.Exists(defaultIconsDir) ? defaultIconsDir : null;
        var socialLinksHtml = await SocialLinksHtmlRenderer.BuildSocialLinksHtmlAsync(config?.SocialLinks, iconsDir, fallbackIconsDir);
        var footerHtml = config?.Footer is { } footer
            ? $"<div class=\"content-footer\">{markdown.ToHtml(footer)}</div>"
            : string.Empty;

        var lastUpdatedHtml = !isHomePage && config?.LastUpdated == true && page.ShowLastUpdated && page.LastModified is { } lastModified
            ? $"<div class=\"last-updated\">Last updated: {lastModified:yyyy-MM-dd}</div>"
            : string.Empty;

        const string editLinkIcon = "<svg class=\"edit-link-icon\" viewBox=\"0 0 24 24\" width=\"16\" height=\"16\" fill=\"currentColor\" aria-hidden=\"true\">" +
            "<path d=\"M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z\"/></svg>";

        var editPath = page.OriginalRelativePath ?? $"{page.Path}.md";
        var encodedEditPath = string.Join("/", editPath.Split('/').Select(Uri.EscapeDataString));
        var editLinkHtml = !isHomePage && config?.EditLink is { Pattern: { Length: > 0 } pattern } editLink
            ? $"<a class=\"edit-link\" href=\"{LayoutProvider.HtmlEncode(pattern.Replace(":path", encodedEditPath))}\" " +
              $"target=\"_blank\" rel=\"noopener noreferrer nofollow\">{editLinkIcon}{LayoutProvider.HtmlEncode(editLink.Text)}</a>"
            : string.Empty;

        var keywordsHtml = page.Keywords is { Count: > 0 } kw
            ? $"<meta name=\"keywords\" content=\"{LayoutProvider.HtmlEncode(string.Join(", ", kw.Take(20)))}\">"
            : string.Empty;

        var pageSegment = page.Path == "index" ? string.Empty : $"{page.Path}/";
        var rawPath = $"{basePath}/{pageSegment}".TrimStart('/');
        var canonicalUrl = $"{context.Request.Scheme}://{context.Request.Host}/{rawPath}";

        var nonce = context.Items["csp-nonce"] as string ?? string.Empty;

        var fullHtml = LayoutProvider.GetLayout(
            title: PageTitleRenderer.ComputeTitle(page.Title, config),
            content: page.HtmlContent,
            navigationHtml: navHtml,
            topNavHtml: topNavHtml,
            mobileTopNavHtml: mobileTopNavHtml,
            tocHtml: tocHtml,
            breadcrumbHtml: breadcrumbHtml,
            paginationHtml: paginationHtml,
            themeCss: combinedThemeCss,
            brandText: brandText,
            brandImage: brandImage,
            enableDarkMode: ThemeProvider.UseDarkMode(themeOptions),
            showScrollIndicator: ThemeProvider.ShowScrollIndicator(themeOptions),
            footerHtml: footerHtml,
            socialLinksHtml: socialLinksHtml,
            enableLiveReload: docsOptions.EnableHotReload,
            buildVersion: docs.BuildVersion,
            favicon: config?.Favicon,
            description: string.IsNullOrEmpty(page.Description) ? config?.Description : page.Description,
            isHomePage: isHomePage,
            lastUpdatedHtml: lastUpdatedHtml,
            editLinkHtml: editLinkHtml,
            basePath: basePath,
            lang: config?.Lang ?? "en",
            headTagsHtml: HeadTagHtmlRenderer.BuildHeadTagsHtml(config?.Head),
            keywordsHtml: keywordsHtml,
            canonicalUrl: canonicalUrl,
            nonce: nonce
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

    if (!PortAvailabilityChecker.TryEnsureUrlsAvailable(urls, out var conflictingPort))
    {
        Log.Fatal("Port {Port} is already in use. Stop the existing process and try again.", conflictingPort);
        return;
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

static async Task<DocumentationPage> BuildSafeRootPage(DocumentationService docs, string basePath, CancellationToken cancellationToken)
{
    var pages = await docs.GetAllPagesAsync(cancellationToken);
    var linksHtml = pages.Count > 0
        ? "<ul>" + string.Join("", pages.OrderBy(p => p.Path)
            .Select(p => $"<li><a href=\"{UrlPaths.Href(basePath, p.Path)}\">{LayoutProvider.HtmlEncode(p.Title)}</a></li>")) + "</ul>"
        : "<p>No Markdown files found yet. Drop one into your docs folder to get started.</p>";

    var html = $"""
        <h1>
            No homepage yet
        </h1>
        <p>
            Create <code>index.md</code> in your docs folder to set what's shown here.
        </p>
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

void LogApplicationBanner()
{
    Log.Information("");
    Log.Information("Bark - Your fast documentation server built on .NET");
    Log.Information("");
}
