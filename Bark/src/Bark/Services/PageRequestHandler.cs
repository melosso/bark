using System.Net;
using System.Security.Cryptography;
using System.Text;
using Bark.Configuration;
using Bark.Models;
using Bark.Services.Layout;
using Bark.Services.Rendering;

namespace Bark.Services;

/// <summary>Startup-computed settings the catch-all page route needs on every request</summary>
public sealed record PageRequestSettings(
    string BasePath,
    string? CustomCsp,
    string? AutoCustomCssUrl,
    string? AutoCustomJsUrl,
    string WebRootPath,
    string DocsRootAbsolute);

/// <summary>Handles the catch-all documentation page route; lookup, ETag/304, CSP nonce, layout assembly</summary>
public sealed class PageRequestHandler
{
    private readonly DocumentationService _docs;
    private readonly MarkdownService _markdown;
    private readonly DocsOptions _docsOptions;
    private readonly ThemeOptions _themeOptions;
    private readonly PageRequestSettings _settings;
    private readonly string _iconsDir;
    private readonly string? _fallbackIconsDir;

    public PageRequestHandler(
        DocumentationService docs,
        MarkdownService markdown,
        DocsOptions docsOptions,
        ThemeOptions themeOptions,
        PageRequestSettings settings)
    {
        _docs = docs;
        _markdown = markdown;
        _docsOptions = docsOptions;
        _themeOptions = themeOptions;
        _settings = settings;
        _iconsDir = Path.Combine(settings.WebRootPath, "icons");
        var defaultIconsDir = Path.Combine(AppContext.BaseDirectory, "wwwroot-default", "icons");
        _fallbackIconsDir = Directory.Exists(defaultIconsDir) ? defaultIconsDir : null;
    }

    // ETag-based nonce: persists across restarts and updates automatically when content changes
    private static string NonceFromETag(string etag) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(etag)), 0, 16);

    public async Task HandleAsync(string? path, HttpContext context)
    {
        var basePath = _settings.BasePath;

        var isRootRequest = path == null || path == "" || path == "/";
        if (isRootRequest)
            path = _docsOptions.DefaultPage ?? "index";

        path = (path ?? "").Trim('/');

        var config = _docs.SiteConfig;

        var page = await _docs.GetPageAsync(path, context.RequestAborted);
        if (page == null && isRootRequest)
            page = await BuildSafeRootPage(_docs, basePath, context.RequestAborted);

        if (page == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode, basePath, config?.Lang ?? "en"));
            return;
        }

        if (page.Redirect is { Length: > 0 } redirectTarget)
        {
            var isAbsolute = redirectTarget.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || redirectTarget.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            string resolvedRedirect;
            if (isAbsolute)
            {
                resolvedRedirect = redirectTarget;
            }
            else
            {
                var trimmed = redirectTarget.Trim('/');
                resolvedRedirect = trimmed.Length == 0
                    ? (basePath.Length == 0 ? "/" : $"{basePath}/")
                    : (basePath.Length == 0 ? $"/{trimmed}/" : $"{basePath}/{trimmed}/");
            }

            context.Response.Redirect(resolvedRedirect, permanent: false);
            return;
        }

        // Folds in the BuildVersion (not limited to this page's own HTML) so content edits that don't touch this page's content still invalidate its cached ETag.
        var responseBytes = Encoding.UTF8.GetBytes(page.HtmlContent);
        var etagInput = Encoding.UTF8.GetBytes(_docs.BuildVersion + ":").Concat(responseBytes).ToArray();
        var etag = Convert.ToBase64String(SHA256.HashData(etagInput)).TrimEnd('=');
        context.Response.Headers.ETag = $"\"{etag}\"";
        context.Response.Headers.CacheControl = "no-cache";

        var nonce = NonceFromETag(etag);
        context.Response.Headers.ContentSecurityPolicy =
            SecurityHeaders.BuildNonceCsp(_settings.CustomCsp ?? SecurityHeaders.DefaultCsp, nonce);

        if (context.Request.Headers.IfNoneMatch.ToString() == $"\"{etag}\"")
        {
            context.Response.StatusCode = 304;
            return;
        }

        var nav = await _docs.GetNavigationAsync(context.RequestAborted);
        var navHtml = NavigationHtmlRenderer.BuildNavigationHtml(nav, path, config, basePath);
        var topNavHtml = NavigationHtmlRenderer.BuildTopNavHtml(config?.TopNav, path, basePath);
        var mobileTopNavHtml = NavigationHtmlRenderer.BuildMobileTopNavHtml(config?.TopNav, path, basePath);

        var tocHtml = page.ShowToc ? TocHtmlRenderer.BuildTocHtml(page.Headings) : null;

        var crumbs = await _docs.GetBreadcrumbsAsync(path, context.RequestAborted);
        var breadcrumbHtml = BreadcrumbHtmlRenderer.BuildBreadcrumbHtml(crumbs, page.Title, basePath);

        var isHomePage = page.Layout == "home";
        var paginationHtml = string.Empty;
        if (!isHomePage && page.ShowPagination)
        {
            var orderedPaths = NavigationHtmlRenderer.GetOrderedPaths(nav, config, path).Where(p => p != null && p != "index").ToList();
            var currentIndex = orderedPaths.IndexOf(path);
            string? prevPath = currentIndex > 0 ? orderedPaths[currentIndex - 1] : null;
            string? nextPath = currentIndex < orderedPaths.Count - 1 ? orderedPaths[currentIndex + 1] : null;
            string? prevTitle = prevPath != null ? (await _docs.GetPageAsync(prevPath, context.RequestAborted))?.Title : null;
            string? nextTitle = nextPath != null ? (await _docs.GetPageAsync(nextPath, context.RequestAborted))?.Title : null;

            paginationHtml = PaginationHtmlRenderer.BuildPaginationHtml(prevTitle, prevPath, nextTitle, nextPath, basePath);
        }

        var themeCss = ThemeProvider.BuildThemeCss(_themeOptions);
        var customCssLink = ThemeProvider.BuildCustomCssLink(_themeOptions, _settings.AutoCustomCssUrl, basePath);
        var customJsScript = ThemeProvider.BuildCustomJsScript(_themeOptions, _settings.AutoCustomJsUrl, basePath);
        var brandText = config?.Brand ?? config?.Title ?? ThemeProvider.GetBrandText(_themeOptions);
        var brandImage = config?.BrandImage;
        var combinedThemeCss = themeCss + customCssLink + customJsScript;

        var socialLinksHtml = await SocialLinksHtmlRenderer.BuildSocialLinksHtmlAsync(config?.SocialLinks, _iconsDir, _fallbackIconsDir);
        var footerHtml = config?.Footer is { } footer
            ? $"<div class=\"content-footer\">{_markdown.ToHtml(footer)}</div>"
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

        var pageControlsEditIcon = !isHomePage && config?.PageControls?.EditLink?.Icon is { Length: > 0 } pcIconName
            ? await IconProvider.InlineSvgAsync(pcIconName, _iconsDir, _fallbackIconsDir)
            : null;
        var isLocalRequest = context.Connection.RemoteIpAddress is { } remoteIp && IPAddress.IsLoopback(remoteIp);
        var pageControlsHtml = !isHomePage
            ? PageControlsHtmlRenderer.BuildPageControlsHtml(page, config?.PageControls, config?.EditLink, basePath, _settings.DocsRootAbsolute, pageControlsEditIcon, isLocalRequest)
            : string.Empty;

        var promoBarHtml = config?.Promo is { Length: > 0 } promoSource
            ? PromoBarHtmlRenderer.BuildPromoBarHtml(_markdown.ToHtml(promoSource), promoSource, nonce)
            : string.Empty;

        var feedUrl = $"{context.Request.Scheme}://{context.Request.Host}{basePath}/feed.xml";
        var rssDiscoveryHtml = $"<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{LayoutProvider.HtmlEncode(config?.Brand ?? config?.Title ?? "RSS Feed")}\" href=\"{LayoutProvider.HtmlEncode(feedUrl)}\">";

        var pageSegment = page.Path == "index" ? string.Empty : $"{page.Path}/";
        var rawPath = $"{basePath}/{pageSegment}".TrimStart('/');
        var canonicalUrl = $"{context.Request.Scheme}://{context.Request.Host}/{rawPath}";

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
            enableDarkMode: ThemeProvider.UseDarkMode(_themeOptions),
            showScrollIndicator: ThemeProvider.ShowScrollIndicator(_themeOptions),
            footerHtml: footerHtml,
            socialLinksHtml: socialLinksHtml,
            enableLiveReload: _docsOptions.EnableHotReload,
            buildVersion: _docs.BuildVersion,
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
            nonce: nonce,
            hasMath: page.HtmlContent.Contains("class=\"katex\"", StringComparison.Ordinal),
            hasMermaid: page.HtmlContent.Contains("class=\"mermaid\"", StringComparison.Ordinal),
            pageControlsHtml: pageControlsHtml,
            rssDiscoveryHtml: rssDiscoveryHtml,
            promoBarHtml: promoBarHtml
        );

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(fullHtml);
    }

    private static async Task<DocumentationPage> BuildSafeRootPage(DocumentationService docs, string basePath, CancellationToken cancellationToken)
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
}
