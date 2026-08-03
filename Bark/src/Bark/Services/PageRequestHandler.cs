using System.Net;
using System.Security.Cryptography;
using System.Text;
using Bark.Configuration;
using Bark.Models;
using Bark.Services.Extensions;
using Bark.Services.Layout;
using Bark.Services.Rendering;
using Bark.Services.Theming;

namespace Bark.Services;

/// <summary>Startup-computed settings the catch-all page route needs on every request</summary>
public sealed record PageRequestSettings(
    string BasePath,
    string? CustomCsp,
    string? AutoCustomCssUrl,
    string? AutoCustomJsUrl,
    string WebRootPath,
    string DocsRootAbsolute,
    string? PublicBaseUrl,
    string? CliTheme = null)
{
    /// <summary>Blank means absent. An empty setting must not count as "configured" and mask a later source.</summary>
    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().TrimEnd('/');

    /// <summary>Precedence: <c>--base-url</c>, then <c>Docs:PublicBaseUrl</c>, then the bare <c>PublicBaseUrl</c> alias.</summary>
    public static string? ResolvePublicBaseUrl(string? cliBaseUrl, string? docsOption, string? alias) =>
        Normalize(cliBaseUrl) ?? Normalize(docsOption) ?? Normalize(alias);

    /// <summary>
    /// Absolute origin for canonical URLs, feeds and sitemaps. <c>PublicBaseUrl</c> wins when set:
    /// the Host header is caller-supplied, and ASP.NET leaves host filtering at <c>*</c> unless
    /// <c>AllowedHosts</c> is configured, so an unconfigured deployment otherwise echoes whatever it is sent.
    /// </summary>
    public string Origin(HttpContext context) =>
        Normalize(PublicBaseUrl) ?? $"{context.Request.Scheme}://{context.Request.Host}";
}

/// <summary>Handles the catch-all documentation page route; lookup, ETag/304, CSP nonce, layout assembly</summary>
public sealed class PageRequestHandler
{
    private readonly DocumentationService _docs;
    private readonly MarkdownService _markdown;
    private readonly DocsOptions _docsOptions;
    private readonly ThemeOptions _themeOptions;
    private readonly PageRequestSettings _settings;
    private readonly ILogger<PageRequestHandler> _logger;
    private readonly string _iconsDir;
    private readonly string? _fallbackIconsDir;

    public PageRequestHandler(
        DocumentationService docs,
        MarkdownService markdown,
        DocsOptions docsOptions,
        ThemeOptions themeOptions,
        PageRequestSettings settings,
        ILogger<PageRequestHandler> logger)
    {
        _docs = docs;
        _markdown = markdown;
        _docsOptions = docsOptions;
        _themeOptions = themeOptions;
        _settings = settings;
        _logger = logger;
        _iconsDir = Path.Combine(settings.WebRootPath, "icons");
        var defaultIconsDir = Path.Combine(AppContext.BaseDirectory, "wwwroot-default", "icons");
        _fallbackIconsDir = Directory.Exists(defaultIconsDir) ? defaultIconsDir : null;
    }

    private static string? ResolveSocialImage(string? image, string origin, string basePath)
    {
        if (string.IsNullOrWhiteSpace(image))
            return null;
        if (image.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || image.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return image;
        var path = image.StartsWith('/') ? image : "/" + image;
        return $"{origin}{basePath}{path}";
    }

    internal static string ComputeETag(string origin, long buildVersion, string html)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes($"{CspNonce.ProcessSalt}:{buildVersion}:{origin}:"));
        hash.AppendData(Encoding.UTF8.GetBytes(html));
        return Convert.ToBase64String(hash.GetHashAndReset()).TrimEnd('=');
    }

    /// <summary>Absolute front-matter redirects need the target host declared in <c>config.json</c>'s <c>redirectHosts</c>; anything else is an open redirect on the docs domain.</summary>
    internal static bool IsAllowedRedirectHost(string target, string requestHost, IReadOnlyList<string>? allowedHosts)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri))
            return false;

        if (uri.Host.Equals(requestHost, StringComparison.OrdinalIgnoreCase))
            return true;

        return allowedHosts is { Count: > 0 }
            && allowedHosts.Any(h => uri.Host.Equals(h.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    internal static string ExpandFooterVariables(string footer, string brand, string? title) =>
        footer
            .Replace("{year}", DateTime.UtcNow.Year.ToString())
            .Replace("{brand}", brand)
            .Replace("{title}", title ?? string.Empty);

    public async Task HandleAsync(string? path, HttpContext context)
    {
        var basePath = _settings.BasePath;

        var isRootRequest = path == null || path == "" || path == "/";
        if (isRootRequest)
            path = _docsOptions.DefaultPage ?? "index";

        path = (path ?? "").Trim('/');

        var config = _docs.SiteConfig;

        // Per request, not at startup: config.json hot-reloads, appsettings does not.
        var theme = ThemeRegistry.Resolve(_settings.CliTheme ?? _themeOptions.Name ?? config?.Theme);

        var page = await _docs.GetPageAsync(path, context.RequestAborted);
        if (page == null && isRootRequest)
            page = await BuildSafeRootPage(_docs, basePath, context.RequestAborted);

        if (page == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(LayoutProvider.Get404Layout(LayoutProvider.HtmlEncode, basePath, config?.Lang ?? "en", theme));
            return;
        }

        if (page.Redirect is { Length: > 0 } redirectTarget)
        {
            var isAbsolute = redirectTarget.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || redirectTarget.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (isAbsolute && !IsAllowedRedirectHost(redirectTarget, context.Request.Host.Host, config?.RedirectHosts))
            {
                _logger.LogWarning(
                    "Page {Page} redirects to {Target}, whose host is not listed in config.json redirectHosts; redirect ignored",
                    page.Path, redirectTarget);
            }
            else
            {
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
        }

        // Folds BuildVersion, CspNonce.ProcessSalt, and Origin into the ETag to invalidate the cache on edits, process restarts, or host changes.
        var origin = _settings.Origin(context);
        var etag = ComputeETag(origin, _docs.BuildVersion, page.HtmlContent);
        context.Response.Headers.ETag = $"\"{etag}\"";
        context.Response.Headers.CacheControl = "no-cache";
        if (_settings.PublicBaseUrl is null)
            context.Response.Headers.Vary = "Host";

        var extensions = _docs.Extensions;

        var nonce = CspNonce.Derive(etag);
        var hasMermaid = page.HtmlContent.Contains("class=\"mermaid\"", StringComparison.Ordinal);
        var baseCsp = SecurityHeaders.WithExtraSources(_settings.CustomCsp ?? SecurityHeaders.DefaultCsp, extensions.CspSources);
        var pageCsp = SecurityHeaders.BuildNonceCsp(baseCsp, nonce);
        context.Response.Headers.ContentSecurityPolicy = hasMermaid
            ? SecurityHeaders.WithInlineStyleElements(pageCsp)
            : pageCsp;

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
            ? $"<div class=\"content-footer\">{_markdown.ToHtml(ExpandFooterVariables(footer, brandText, config.Title))}</div>"
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
        var isLocalRequest = LocalRequest.IsLocal(context);
        var pageControlsHtml = !isHomePage
            ? PageControlsHtmlRenderer.BuildPageControlsHtml(page, config?.PageControls, config?.EditLink, basePath, _settings.DocsRootAbsolute, pageControlsEditIcon, isLocalRequest)
            : string.Empty;

        var promoBarHtml = config?.Promo is { Length: > 0 } promoSource
            ? PromoBarHtmlRenderer.BuildPromoBarHtml(_markdown.ToHtml(promoSource), promoSource, nonce)
            : string.Empty;

        var feedUrl = $"{origin}{basePath}/feed.xml";
        var rssDiscoveryHtml = $"<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{LayoutProvider.HtmlEncode(config?.Brand ?? config?.Title ?? "RSS Feed")}\" href=\"{LayoutProvider.HtmlEncode(feedUrl)}\">";

        var pageSegment = page.Path == "index" ? string.Empty : $"{page.Path}/";
        var rawPath = $"{basePath}/{pageSegment}".TrimStart('/');
        var canonicalUrl = $"{origin}/{rawPath}";

        var metaDescription = string.IsNullOrEmpty(page.Description) ? config?.Description : page.Description;
        var socialImageUrl = ResolveSocialImage(page.Image ?? config?.Image ?? config?.BrandImage, origin, basePath);
        var siteName = config?.Brand ?? config?.Title;
        var modified = isHomePage ? null : page.LastModified;

        var socialMetaHtml = SocialMetaRenderer.BuildSocialMeta(
            canonicalUrl, page.Title, metaDescription, isHomePage, socialImageUrl, siteName, config?.Lang ?? "en", modified);
        var structuredDataHtml = StructuredDataRenderer.BuildJsonLd(
            canonicalUrl, page.Title, metaDescription, isHomePage, origin, basePath, crumbs, socialImageUrl, siteName, modified, nonce);

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
            staticSearch: _docsOptions.IsStaticExport,
            buildVersion: _docs.BuildVersion,
            favicon: config?.Favicon,
            description: metaDescription,
            isHomePage: isHomePage,
            lastUpdatedHtml: lastUpdatedHtml,
            editLinkHtml: editLinkHtml,
            basePath: basePath,
            lang: config?.Lang ?? "en",
            headTagsHtml: HeadTagHtmlRenderer.BuildHeadTagsHtml(config?.Head) + ExtensionHeadRenderer.Build(extensions, nonce),
            keywordsHtml: keywordsHtml,
            canonicalUrl: canonicalUrl,
            nonce: nonce,
            hasMath: page.HtmlContent.Contains("class=\"katex\"", StringComparison.Ordinal),
            hasMermaid: hasMermaid,
            pageControlsHtml: pageControlsHtml,
            rssDiscoveryHtml: rssDiscoveryHtml,
            promoBarHtml: promoBarHtml,
            socialMetaHtml: socialMetaHtml,
            structuredDataHtml: structuredDataHtml,
            theme: theme
        );

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(fullHtml);
    }

    private static async Task<DocumentationPage> BuildSafeRootPage(DocumentationService docs, string basePath, CancellationToken cancellationToken)
    {
        var pages = await docs.GetAllPagesAsync(cancellationToken);
        var linksHtml = pages.Count > 0
            ? "<p>In the meantime, here are the pages you have already written:</p><ul>" + string.Join("", pages.OrderBy(p => p.Path)
                .Select(p => $"<li><a href=\"{UrlPaths.Href(basePath, p.Path)}\">{LayoutProvider.HtmlEncode(p.Title)}</a></li>")) + "</ul>"
            : "<p>It looks like there are not any Markdown files just yet. Whenever you are ready, you can simply drop a <code>.md</code> file into your docs folder, and it will appear here instantly, with no build step to wait on.</p>";

        var html = $"""
            <h1>
                Let's set up your homepage
            </h1>
            <p>
                You are up and running. Whenever you would like to choose what appears on this page, you can create an <code>index.md</code> file inside your docs folder, and it will be rendered here as your homepage automatically.
            </p>
            {linksHtml}
            """;

        return new DocumentationPage(
            Path: "index",
            Title: "Let's set up your homepage",
            HtmlContent: html,
            Description: null,
            LastModified: null,
            Headings: []
        );
    }
}
