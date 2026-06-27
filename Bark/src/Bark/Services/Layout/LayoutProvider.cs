using System.Text;

namespace Bark.Services;

public static partial class LayoutProvider
{
    public static string GetLayout(
        string title,
        string content,
        string navigationHtml,
        string tocHtml,
        string breadcrumbHtml,
        string paginationHtml,
        string? themeCss = null,
        string? brandText = null,
        bool enableDarkMode = true,
        string? footerHtml = null,
        string? socialLinksHtml = null,
        bool enableLiveReload = false,
        long buildVersion = 0,
        string? favicon = null,
        string? description = null,
        string? topNavHtml = null,
        string? mobileTopNavHtml = null,
        bool isHomePage = false,
        string? lastUpdatedHtml = null,
        string? editLinkHtml = null,
        bool showScrollIndicator = true,
        string basePath = "",
        string lang = "en",
        string? headTagsHtml = null)
    {
        var scrollIndicatorHtml = showScrollIndicator ? @"<div id=""scroll-indicator""></div>" : "";
        var faviconHtml = BuildFaviconLink(favicon, basePath);
        var homeHref = basePath.Length == 0 ? "/" : $"{basePath}/";
        var descriptionHtml = !string.IsNullOrWhiteSpace(description)
            ? $"<meta name=\"description\" content=\"{HtmlEncode(description)}\">"
            : "";

        var layoutClass = isHomePage ? "layout bark-home-layout" : "layout";
        var mobileSocialHtml = !string.IsNullOrWhiteSpace(socialLinksHtml)
            ? $@"<div class=""sidebar-social-links"">{socialLinksHtml}</div>"
            : "";
        var sidebarLeftHtml = $@"
        <aside class=""sidebar-left"" id=""sidebar-left"" aria-label=""Documentation navigation"">
            {mobileTopNavHtml}
            {navigationHtml}
            {mobileSocialHtml}
        </aside>";
        var breadcrumbAndTocHtml = isHomePage ? "" : $@"
            <nav class=""breadcrumb"" aria-label=""Breadcrumb"">
                {breadcrumbHtml}
            </nav>
            <details class=""toc-inline"">
                <summary>On this page</summary>
                <ul class=""toc-list"">
                    {tocHtml}
                </ul>
            </details>";
        var sidebarRightHtml = isHomePage ? "" : string.IsNullOrWhiteSpace(tocHtml)
            ? $@"
        <aside class=""sidebar-right"" aria-label=""Page info"">
            <div class=""toc-title"">{HtmlEncode(title)}</div>
        </aside>"
            : $@"
        <aside class=""sidebar-right"" aria-label=""Table of contents"">
            <div class=""toc-title"">On This Page</div>
            <div class=""toc-list-wrapper"">
                <div class=""toc-indicator"" aria-hidden=""true""></div>
                <ul class=""toc-list"">
                    {tocHtml}
                </ul>
            </div>
        </aside>";
        var contentClass = isHomePage ? "content bark-home-content" : "content";
        // Home pages never show "last updated" or prev/next pagination, regardless of caller input.
        var paginationBlock = isHomePage ? "" : paginationHtml;
        var lastUpdatedBlock = isHomePage ? "" : lastUpdatedHtml;
        var editLinkBlock = isHomePage ? "" : editLinkHtml;
        var pageMetaBlock = string.IsNullOrEmpty(editLinkBlock) && string.IsNullOrEmpty(lastUpdatedBlock)
            ? ""
            : $@"<div class=""page-meta""><div class=""page-meta-left"">{editLinkBlock}</div><div class=""page-meta-right"">{lastUpdatedBlock}</div></div>";

        const string darkVars = @"
                --bg-color: #0b0b0b;
                --sidebar-bg: #121212;
                --text-color: #e5e5e5;
                --text-muted: #a0a0a0;
                --accent: #6b8e74;
                --accent-light: #1c241f;
                --border: #222222;
                --code-bg: #161616;
                --alert-note: #2f81f7;
                --alert-tip: #3fb950;
                --alert-important: #a371f7;
                --alert-warning: #d4a72c;
                --alert-caution: #f85149;";

        var darkModeMediaQuery = enableDarkMode
            ? $@"@media (prefers-color-scheme: dark) {{
            :root:not([data-theme=""light""]) {{{darkVars}
            }}
        }}
        :root[data-theme=""dark""] {{{darkVars}
        }}"
            : "";

        var themeInitScript = enableDarkMode
            ? @"<script>(function(){try{var t=localStorage.getItem('bark-theme');if(t==='dark'||t==='light')document.documentElement.setAttribute('data-theme',t);}catch(e){}})();</script>"
            : "";

        var themeToggleHtml = enableDarkMode
            ? @"<button type=""button"" class=""theme-toggle"" id=""theme-toggle"" role=""switch"" aria-checked=""false"" aria-label=""Toggle dark mode"">
                <span class=""theme-toggle-thumb"">
                    <svg class=""icon-sun"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><circle cx=""12"" cy=""12"" r=""4""/><path d=""M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41""/></svg>
                    <svg class=""icon-moon"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" aria-hidden=""true""><path d=""M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z""/></svg>
                </span>
            </button>"
            : "";

        return $@"
<!DOCTYPE html>
<html lang=""{HtmlEncode(lang)}"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{HtmlEncode(title)}</title>
    {descriptionHtml}
    {faviconHtml}
    {headTagsHtml}
    {themeInitScript}
    {themeCss}
    {GetStyles(darkModeMediaQuery)}
    <link rel=""stylesheet"" href=""{basePath}/css/katex.min.css"">
    <script defer src=""{basePath}/js/mermaid.min.js""></script>
</head>
<body>
    <a href=""#main-content"" class=""skip-link"">Skip to content</a>
    {scrollIndicatorHtml}
    <header class=""topbar"">
        <div class=""topbar-left"">
            <button type=""button"" class=""menu-toggle icon-btn"" id=""menu-toggle""
                    aria-expanded=""false"" aria-controls=""sidebar-left"" aria-label=""Toggle navigation menu"">
                <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" aria-hidden=""true"">
                    <path d=""M3 6h18M3 12h18M3 18h18"" stroke-linecap=""round""/>
                </svg>
            </button>
            <div class=""brand""><a href=""{homeHref}"">{brandText ?? "Bark"}</a></div>
            <button type=""button"" class=""search-trigger"" id=""search-trigger""
                    aria-haspopup=""dialog"" aria-controls=""search-modal"" aria-label=""Search documentation"">
                <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><circle cx=""11"" cy=""11"" r=""7""/><path d=""M21 21l-4.3-4.3""/></svg>
                <span class=""search-trigger-label"">Search</span>
                <kbd class=""search-trigger-kbd"" id=""search-trigger-kbd"" aria-hidden=""true"">Ctrl K</kbd>
            </button>
        </div>
        {topNavHtml}
        <div class=""topbar-right"">
            <button type=""button"" class=""search-trigger-mobile icon-btn"" id=""search-trigger-mobile""
                    aria-haspopup=""dialog"" aria-controls=""search-modal"" aria-label=""Search documentation"">
                <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><circle cx=""11"" cy=""11"" r=""7""/><path d=""M21 21l-4.3-4.3""/></svg>
            </button>
            {socialLinksHtml}
            {themeToggleHtml}
        </div>
    </header>
    <div class=""search-overlay"" id=""search-overlay"" hidden>
        <div class=""search-modal"" id=""search-modal"" role=""dialog"" aria-modal=""true"" aria-labelledby=""search-modal-label"">
            <h2 id=""search-modal-label"" class=""sr-only"">Search documentation</h2>
            <div class=""search-modal-header"">
                <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><circle cx=""11"" cy=""11"" r=""7""/><path d=""M21 21l-4.3-4.3""/></svg>
                <input type=""search"" class=""search-modal-input"" id=""search-modal-input""
                       placeholder=""Search documentation..."" autocomplete=""off"" enterkeyhint=""search""
                       role=""combobox"" aria-expanded=""false"" aria-controls=""search-modal-results""
                       aria-autocomplete=""list"" aria-label=""Search documentation"">
                <button type=""button"" class=""search-modal-close icon-btn"" id=""search-modal-close"" aria-label=""Close search"">
                    <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><path d=""M18 6L6 18M6 6l12 12""/></svg>
                </button>
            </div>
            <div class=""search-modal-results"" id=""search-modal-results"" role=""listbox"" aria-label=""Search results""></div>
            <div class=""sr-only"" id=""search-modal-status"" role=""status"" aria-live=""polite""></div>
            <ul class=""DocSearch-Commands"" aria-hidden=""true"">
                <li>
                    <kbd class=""DocSearch-Commands-Key""><svg width=""20"" height=""20"" aria-label=""Arrow down"" viewBox=""0 0 24 24"" role=""img""><g fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""1.4""><path d=""M12 5v14""></path><path d=""m19 12-7 7-7-7""></path></g></svg></kbd>
                    <kbd class=""DocSearch-Commands-Key""><svg width=""20"" height=""20"" aria-label=""Arrow up"" viewBox=""0 0 24 24"" role=""img""><g fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""1.4""><path d=""m5 12 7-7 7 7""></path><path d=""M12 19V5""></path></g></svg></kbd>
                    <span class=""DocSearch-Label"">Navigate</span>
                </li>
                <li>
                    <kbd class=""DocSearch-Commands-Key""><svg width=""20"" height=""20"" aria-label=""Enter key"" viewBox=""0 0 24 24"" role=""img""><g fill=""none"" stroke=""currentColor"" stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""1.4""><polyline points=""9 10 4 15 9 20""></polyline><path d=""M20 4v7a4 4 0 0 1-4 4H4""></path></g></svg></kbd>
                    <span class=""DocSearch-Label"">Select</span>
                </li>
                <li>
                    <kbd class=""DocSearch-Commands-Key""><span class=""DocSearch-Escape-Key"">ESC</span></kbd>
                    <span aria-label=""Escape key"" class=""DocSearch-Label"">Close</span>
                </li>
            </ul>
        </div>
    </div>
    <div class=""sidebar-overlay"" id=""sidebar-overlay""></div>
    <div class=""{layoutClass}"">
        {sidebarLeftHtml}
        <main class=""main-container"" id=""main-content"" tabindex=""-1"">
            {breadcrumbAndTocHtml}
            <article class=""{contentClass}"">
                {content}
                {pageMetaBlock}
                {paginationBlock}
                {footerHtml}
            </article>
        </main>
        {sidebarRightHtml}
    </div>
    {GetScripts(enableLiveReload, buildVersion, basePath)}
</body>
</html>";
    }

    public static string Get404Layout(Func<string?, string> htmlEncode, string basePath = "", string lang = "en")
    {
        var homeHref = basePath.Length == 0 ? "/" : $"{basePath}/";
        return $@"
<!DOCTYPE html>
<html lang=""{htmlEncode(lang)}"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Page Not Found</title>
    <style>
        :root {{
            --bg-color: #fafafa;
            --text-color: #1a1a1a;
            --text-muted: #666666;
            --accent: #2e4a36;
            --font-sans: system-ui, -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;
        }}
        @media (prefers-color-scheme: dark) {{
            :root {{
                --bg-color: #0b0b0b;
                --text-color: #e5e5e5;
                --text-muted: #a0a0a0;
                --accent: #6b8e74;
            }}
        }}
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        body {{
            font-family: var(--font-sans);
            background-color: var(--bg-color);
            color: var(--text-color);
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            line-height: 1.6;
        }}
        .not-found {{
            text-align: center;
        }}
        .not-found h1 {{
            font-size: 4rem;
            font-weight: 600;
            letter-spacing: -0.03em;
            margin-bottom: 0.5rem;
        }}
        .not-found p {{
            color: var(--text-muted);
            margin-bottom: 2rem;
        }}
        .not-found a {{
            color: var(--accent);
            text-decoration: none;
            font-weight: 500;
        }}
        .not-found a:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class=""not-found"">
        <h1>404</h1>
        <p>The page you're looking for doesn't exist.</p>
        <a href=""{homeHref}"">Return home</a>
    </div>
</body>
</html>";
    }

    public static string HtmlEncode(string? value) =>
        value != null ? System.Net.WebUtility.HtmlEncode(value) : string.Empty;

    private static string BuildFaviconLink(string? favicon, string basePath = "")
    {
        if (string.IsNullOrWhiteSpace(favicon))
            return string.Empty;

        var isRootRelative = favicon.StartsWith('/');
        var isUrl = favicon.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || favicon.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || isRootRelative;

        if (isUrl)
        {
            var href = isRootRelative ? $"{basePath}{favicon}" : favicon;
            return $"<link rel=\"icon\" href=\"{HtmlEncode(href)}\">";
        }

        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>{favicon}</text></svg>";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return $"<link rel=\"icon\" href=\"data:image/svg+xml;base64,{base64}\">";
    }
}
