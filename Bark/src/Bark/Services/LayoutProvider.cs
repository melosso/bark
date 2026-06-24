using System.Text;

namespace Bark.Services;

public static class LayoutProvider
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
        bool showScrollIndicator = true)
    {
        var scrollIndicatorHtml = showScrollIndicator ? @"<div id=""scroll-indicator""></div>" : "";
        var faviconHtml = BuildFaviconLink(favicon);
        var descriptionHtml = !string.IsNullOrWhiteSpace(description)
            ? $"<meta name=\"description\" content=\"{HtmlEncode(description)}\">"
            : "";

        var layoutClass = isHomePage ? "layout vp-home-layout" : "layout";
        var sidebarLeftHtml = isHomePage ? "" : $@"
        <aside class=""sidebar-left"" id=""sidebar-left"" aria-label=""Documentation navigation"">
            {mobileTopNavHtml}
            {navigationHtml}
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
        var sidebarRightHtml = isHomePage ? "" : $@"
        <aside class=""sidebar-right"" aria-label=""Table of contents"">
            <div class=""toc-title"">On This Page</div>
            <div class=""toc-list-wrapper"">
                <div class=""toc-indicator"" aria-hidden=""true""></div>
                <ul class=""toc-list"">
                    {tocHtml}
                </ul>
            </div>
        </aside>";
        var contentClass = isHomePage ? "content vp-home-content" : "content";
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
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{HtmlEncode(title)}</title>
    {descriptionHtml}
    {faviconHtml}
    {themeInitScript}
    {themeCss}
    <style>
        :root {{
            --bg-color: #fafafa;
            --sidebar-bg: #f4f4f4;
            --text-color: #1a1a1a;
            --text-muted: #666666;
            --accent: #2e4a36;
            --accent-light: #e8ece9;
            --border: #e5e5e5;
            --code-bg: #f0f0f0;
            --alert-note: #0969da;
            --alert-tip: #1a7f37;
            --alert-important: #8250df;
            --alert-warning: #9a6700;
            --alert-caution: #cf222e;
            --font-sans: system-ui, -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;
            --font-mono: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        }}
        {darkModeMediaQuery}
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        html, body {{
            /* `clip` not `hidden` -- `hidden` forces overflow-y to `auto` too, turning body
               into a scroll container and breaking position: sticky on the sidebars. */
            overflow-x: clip;
        }}
        body {{
            font-family: var(--font-sans);
            background-color: var(--bg-color);
            color: var(--text-color);
            line-height: 1.6;
            -webkit-font-smoothing: antialiased;
        }}
        #scroll-indicator {{
            position: fixed; top: 0; left: 0; height: 3px;
            background-color: var(--accent); width: 0%; z-index: 1101;
            transition: width 0.15s ease;
        }}
        :focus-visible {{
            outline: 2px solid var(--accent);
            outline-offset: 2px;
        }}
        .skip-link {{
            position: absolute; left: -9999px; top: 0; z-index: 1100;
            background: var(--accent); color: #fff; padding: 0.75rem 1.25rem;
            border-radius: 0 0 6px 0; text-decoration: none; font-size: 0.9rem;
        }}
        .skip-link:focus {{
            left: 0;
        }}
        @media (prefers-reduced-motion: reduce) {{
            *, *::before, *::after {{
                animation-duration: 0.01ms !important;
                animation-iteration-count: 1 !important;
                transition-duration: 0.01ms !important;
                scroll-behavior: auto !important;
            }}
        }}
        :root {{ --topbar-height: 57px; }}
        /* z-index scale: sidebar-overlay 1001 < topbar 1002 < mobile drawer 1003 < skip-link 1100
           < scroll-indicator 1101, so the indicator stays visible above the opaque topbar. */
        .icon-btn {{
            display: inline-flex; align-items: center; justify-content: center;
            width: 36px; height: 36px; border-radius: 6px; border: none;
            background: transparent; color: var(--text-muted); cursor: pointer;
            flex-shrink: 0; text-decoration: none;
            transition: color 0.15s ease, background-color 0.15s ease;
        }}
        .icon-btn:hover {{ color: var(--accent); background-color: var(--code-bg); }}
        .icon-btn svg {{ width: 18px; height: 18px; }}
        .topbar {{
            display: flex; align-items: center; justify-content: space-between;
            height: var(--topbar-height); padding: 0 1.5rem;
            background-color: var(--bg-color); border-bottom: 1px solid var(--border);
            position: sticky; top: 0; z-index: 1002;
        }}
        .topbar-left {{
            display: flex; align-items: center; gap: 0.75rem;
        }}
        .topbar-right {{
            display: flex; align-items: center; gap: 1rem;
        }}
        .top-nav {{
            display: flex; align-items: center; gap: 1.5rem; margin-left: 1.5rem; height: 100%;
        }}
        .top-nav-item {{ display: flex; align-items: center; height: 100%; position: relative; }}
        .top-nav-link {{
            display: inline-flex; align-items: center; gap: 0.3rem;
            font-size: 0.9rem; font-weight: 500; color: var(--text-muted);
            text-decoration: none; background: none; border: none; cursor: pointer;
            padding: 0; font-family: inherit;
        }}
        .top-nav-link:hover, .top-nav-link.active {{ color: var(--accent); }}
        .top-nav-chevron {{ width: 14px; height: 14px; transition: transform 0.15s ease; }}
        .top-nav-item.has-dropdown:hover .top-nav-chevron,
        .top-nav-item.has-dropdown:focus-within .top-nav-chevron {{ transform: rotate(180deg); }}
        .top-nav-dropdown-menu {{
            display: none; position: absolute; top: 100%; left: 0; min-width: 180px;
            background-color: var(--bg-color); border: 1px solid var(--border); border-radius: 8px;
            padding: 0.4rem; box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12); z-index: 1003;
        }}
        .top-nav-item.has-dropdown:hover .top-nav-dropdown-menu,
        .top-nav-item.has-dropdown:focus-within .top-nav-dropdown-menu {{ display: block; }}
        .top-nav-dropdown-link {{
            display: flex; align-items: center; justify-content: space-between; gap: 0.5rem;
            padding: 0.45rem 0.6rem; border-radius: 6px;
            font-size: 0.875rem; color: var(--text-color); text-decoration: none;
        }}
        .top-nav-dropdown-link:hover {{ background-color: var(--code-bg); color: var(--accent); }}
        .external-link-icon {{
            display: inline-block; width: 12px; height: 12px; flex-shrink: 0;
            opacity: 0.6; vertical-align: -1px; margin-left: 0.25rem;
        }}
        .mobile-top-nav {{ display: none; }}
        .layout {{
            display: grid;
            grid-template-columns: 270px 1fr 270px;
            min-height: calc(100vh - var(--topbar-height));
        }}
        .sidebar-left {{
            background-color: var(--sidebar-bg);
            border-right: 1px solid var(--border);
            padding: 2.75rem 1.75rem;
            position: sticky; top: var(--topbar-height); align-self: start;
            height: calc(100vh - var(--topbar-height)); overflow-y: auto;
        }}
        .brand a {{
            font-size: 1.1rem; font-weight: 600; letter-spacing: -0.02em;
            color: var(--text-color); text-decoration: none;
        }}
        .brand a:hover {{ color: var(--accent); }}
        .theme-toggle {{
            position: relative; flex-shrink: 0; width: 48px; height: 28px;
            border: 1px solid var(--border); border-radius: 999px; padding: 0;
            background-color: var(--code-bg); cursor: pointer;
            transition: background-color 0.15s ease, border-color 0.15s ease;
        }}
        .theme-toggle:hover {{ border-color: var(--accent); }}
        .theme-toggle-thumb {{
            position: absolute; top: 3px; left: 3px; width: 20px; height: 20px;
            border-radius: 50%; background-color: var(--bg-color);
            box-shadow: 0 1px 2px rgba(0, 0, 0, 0.25);
            display: flex; align-items: center; justify-content: center;
            transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1);
        }}
        .theme-toggle-thumb svg {{ width: 13px; height: 13px; color: var(--accent); }}
        .theme-toggle-thumb .icon-moon {{ display: none; }}
        :root[data-theme=""dark""] .theme-toggle-thumb {{ transform: translateX(20px); }}
        :root[data-theme=""dark""] .theme-toggle-thumb .icon-sun {{ display: none; }}
        :root[data-theme=""dark""] .theme-toggle-thumb .icon-moon {{ display: block; }}
        @media (prefers-color-scheme: dark) {{
            :root:not([data-theme=""light""]) .theme-toggle-thumb {{ transform: translateX(20px); }}
            :root:not([data-theme=""light""]) .theme-toggle-thumb .icon-sun {{ display: none; }}
            :root:not([data-theme=""light""]) .theme-toggle-thumb .icon-moon {{ display: block; }}
        }}
        .sr-only {{
            position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px;
            overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;
        }}
        .search-trigger {{
            display: flex; align-items: center; gap: 0.55rem;
            margin-left: 1rem; padding: 0.4rem 0.65rem;
            border: 1px solid var(--border); border-radius: 8px;
            background-color: var(--sidebar-bg); color: var(--text-muted);
            font-family: inherit; font-size: 0.85rem; cursor: pointer;
            transition: border-color 0.15s ease, color 0.15s ease;
        }}
        .search-trigger:hover {{ border-color: var(--accent); color: var(--text-color); }}
        .search-trigger svg {{ width: 16px; height: 16px; flex-shrink: 0; }}
        .search-trigger-kbd {{
            font-family: var(--font-mono); font-size: 0.7rem;
            border: 1px solid var(--border); border-radius: 4px;
            padding: 0.1rem 0.35rem; background-color: var(--bg-color); color: var(--text-muted);
        }}
        .search-overlay {{
            position: fixed; inset: 0; z-index: 1200;
            background-color: rgba(0, 0, 0, 0.5);
            display: flex; align-items: flex-start; justify-content: center;
            padding: 8vh 1rem 2rem; opacity: 0; transition: opacity 0.15s ease;
        }}
        .search-overlay[hidden] {{ display: none; }}
        .search-overlay.open {{ opacity: 1; }}
        .search-modal {{
            width: 100%; max-width: 720px; max-height: 80vh;
            background-color: var(--bg-color); border: 1px solid var(--border); border-radius: 12px;
            box-shadow: 0 24px 64px rgba(0, 0, 0, 0.3);
            display: flex; flex-direction: column; overflow: hidden;
            transform: translateY(-12px) scale(0.98);
            transition: transform 0.15s ease;
        }}
        .search-overlay.open .search-modal {{ transform: translateY(0) scale(1); }}
        .search-modal-header {{
            display: flex; align-items: center; gap: 0.75rem;
            padding: 1rem 1.25rem; border-bottom: 1px solid var(--border); flex-shrink: 0;
        }}
        .search-modal-header > svg {{ width: 20px; height: 20px; color: var(--text-muted); flex-shrink: 0; }}
        .search-modal-input {{
            flex: 1; min-width: 0; border: none; outline: none; background: transparent;
            color: var(--text-color); font-size: 1.05rem; font-family: var(--font-sans);
        }}
        .search-modal-close {{ flex-shrink: 0; }}
        .search-modal-results {{ flex: 1; overflow-y: auto; padding: 0.5rem; }}
        .search-result-item {{
            display: block; padding: 0.7rem 0.9rem; border-radius: 8px;
            text-decoration: none; transition: background-color 0.1s ease;
        }}
        .search-result-item.active, .search-result-item:hover {{ background-color: var(--accent-light); }}
        .search-result-title {{ font-weight: 500; color: var(--text-color); font-size: 0.9rem; }}
        .search-result-excerpt {{ font-size: 0.8rem; color: var(--text-muted); margin-top: 0.2rem; }}
        .search-highlight {{
            background-color: var(--accent-light); color: var(--accent);
            border-radius: 3px; padding: 0 0.15em; font-weight: 600;
        }}
        .search-result-empty {{ color: var(--text-muted); font-size: 0.85rem; padding: 1rem; text-align: center; }}
        .search-modal-footer {{
            display: flex; gap: 1.25rem; padding: 0.6rem 1.25rem;
            border-top: 1px solid var(--border); font-size: 0.75rem; color: var(--text-muted);
            flex-shrink: 0;
        }}
        .search-modal-footer kbd {{
            font-family: var(--font-mono); border: 1px solid var(--border); border-radius: 4px;
            padding: 0.1rem 0.3rem; background-color: var(--code-bg); margin-right: 0.25rem;
        }}
        @media (max-width: 768px) {{
            .search-trigger-label, .search-trigger-kbd {{ display: none; }}
            .search-trigger {{ min-width: 44px; min-height: 44px; justify-content: center; padding: 0; margin-left: 0.5rem; }}
            .search-modal-close {{ width: 44px; height: 44px; }}
            .search-overlay {{ padding: 0; }}
            .search-modal {{ max-width: 100%; max-height: 100%; height: 100%; height: 100dvh; border-radius: 0; }}
            .search-modal-footer {{ flex-wrap: wrap; row-gap: 0.4rem; }}
        }}
        .nav-group {{
            margin-bottom: 2.25rem;
        }}
        .nav-group-title {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
            color: var(--text-muted); margin-bottom: 1rem; font-weight: 600;
        }}
        .nav-list {{
            list-style: none;
        }}
        .nav-item a {{
            display: block; padding: 0.55rem 0.8rem; line-height: 1.4;
            color: var(--text-muted); text-decoration: none; font-size: 0.9rem;
            border-radius: 6px; margin-left: -0.8rem;
            transition: all 0.15s ease;
        }}
        .nav-item a:hover {{
            color: var(--text-color); background-color: var(--code-bg);
        }}
        .nav-item.active a {{
            color: var(--accent); background-color: var(--accent-light); font-weight: 500;
        }}
        /* .sidebar-group-title stays a plain <div>; <summary> can't be fully de-styled across
           engines, so summary.sidebar-group-summary just wraps it as a click target. */
        /* Each .sidebar-group-items adds 0.9rem left padding; depth compounds via nesting,
           no per-level overrides needed. Root list gets no padding. */
        .sidebar-tree {{ font-size: 0.9rem; }}
        .sidebar-group {{ margin-bottom: 0.25rem; }}
        .sidebar-group-summary {{
            display: block; list-style: none; cursor: pointer;
        }}
        .sidebar-group-summary::-webkit-details-marker {{ display: none; }}
        .sidebar-group-summary::marker {{ content: """"; }}
        .sidebar-group.no-caret > .sidebar-group-title {{ cursor: default; }}
        .sidebar-group-title {{
            display: flex; align-items: center; gap: 0.4rem;
            padding: 0.5rem 0.8rem; border-radius: 6px;
            user-select: none; transition: background-color 0.15s ease;
        }}
        .sidebar-group-summary:hover .sidebar-group-title {{ background-color: var(--code-bg); }}
        /* Only the caret should distinguish colapsible from static groups, not typography */
        .sidebar-group-title h2, .sidebar-group-title h3 {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
            color: var(--text-muted); font-weight: 600; flex: 1; margin: 0;
        }}
        /* Ancestors get a text-color cue only; the highlighted background is reserved for the
           one active leaf link, so a nested active page doesn't stack backgrounds at every level. */
        .sidebar-group-title.has-active h2, .sidebar-group-title.has-active h3 {{ color: var(--accent); }}
        .caret-icon {{
            display: inline-flex; flex-shrink: 0; width: 16px; height: 16px;
            color: var(--text-muted); transition: transform 0.2s ease;
        }}
        .caret-icon svg {{ width: 100%; height: 100%; }}
        details[open] > .sidebar-group-summary .caret-icon {{ transform: rotate(90deg); }}
        .sidebar-group-items {{ padding-left: 0.9rem; margin-bottom: 0.5rem; }}
        .sidebar-tree > .sidebar-group > .sidebar-group-items {{ padding-left: 0; }}
        .sidebar-link {{ margin-bottom: 0.1rem; }}
        /* Top-level entries get a divider between sections; scoped to direct children of
           .sidebar-tree so items inside a group stay tightly packed. */
        .sidebar-tree > .sidebar-group + .sidebar-group,
        .sidebar-tree > .sidebar-group + .sidebar-link,
        .sidebar-tree > .sidebar-link + .sidebar-group,
        .sidebar-tree > .sidebar-link + .sidebar-link {{
            border-top: 1px solid var(--border);
            padding-top: 0.75rem;
            margin-top: 0.75rem;
        }}
        .sidebar-link a {{
            display: block; padding: 0.45rem 0.8rem; line-height: 1.4;
            color: var(--text-muted); text-decoration: none; font-size: 0.875rem;
            border-radius: 6px; transition: all 0.15s ease;
        }}
        .sidebar-link a:hover {{ color: var(--text-color); background-color: var(--code-bg); }}
        .sidebar-link.is-active a {{
            color: var(--accent); background-color: var(--accent-light); font-weight: 500;
        }}
        .main-container {{
            padding: 3rem 4rem;
            max-width: 800px; justify-self: center; width: 100%;
            min-width: 0;
        }}
        .vp-home-layout {{ grid-template-columns: 1fr; }}
        .vp-home-layout .main-container {{ max-width: 100%; padding: 0; }}
        .vp-home-content {{ max-width: 960px; margin: 0 auto; padding: 0 2rem; }}
        .vp-hero {{ text-align: center; padding: 4.5rem 1.5rem 3.5rem; }}
        .vp-hero-image {{ font-size: 4rem; margin-bottom: 1.5rem; line-height: 1; }}
        .vp-hero-image img {{ max-width: 200px; max-height: 200px; }}
        .vp-hero-name {{
            font-size: 2.75rem; font-weight: 700; letter-spacing: -0.02em;
            color: var(--accent); margin-bottom: 0.5rem;
        }}
        .vp-hero-text {{
            font-size: 2rem; font-weight: 600; color: var(--text-color);
            letter-spacing: -0.02em; margin-bottom: 1rem;
        }}
        .vp-hero-tagline {{
            font-size: 1.15rem; color: var(--text-muted); max-width: 540px;
            margin: 0 auto 2rem;
        }}
        .vp-hero-actions {{ display: flex; justify-content: center; gap: 0.9rem; flex-wrap: wrap; }}
        .vp-hero-action {{
            display: inline-flex; align-items: center; padding: 0.65rem 1.4rem;
            border-radius: 8px; font-weight: 600; font-size: 0.95rem; text-decoration: none;
            transition: opacity 0.15s ease, background-color 0.15s ease;
        }}
        .vp-hero-action.brand {{ background-color: var(--accent); color: var(--bg-color); }}
        .vp-hero-action.brand:hover {{ opacity: 0.85; }}
        .vp-hero-action.alt {{
            border: 1px solid var(--border); color: var(--text-color); background: transparent;
        }}
        .vp-hero-action.alt:hover {{ background-color: var(--accent-light); }}
        .vp-features {{
            display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
            gap: 1.25rem; padding: 1rem 1.5rem 4rem;
        }}
        .vp-feature {{
            display: block; padding: 1.5rem; border: 1px solid var(--border);
            border-radius: 10px; background-color: var(--sidebar-bg);
            text-decoration: none; color: inherit; transition: border-color 0.15s ease;
        }}
        a.vp-feature:hover {{ border-color: var(--accent); }}
        .vp-feature-icon {{ font-size: 1.75rem; margin-bottom: 0.75rem; }}
        .vp-feature-title {{ font-size: 1.05rem; font-weight: 600; margin-bottom: 0.4rem; }}
        .vp-feature-details {{ font-size: 0.875rem; color: var(--text-muted); line-height: 1.5; }}
        .page-meta {{
            display: flex; justify-content: space-between; align-items: center; gap: 1rem;
            margin-top: 2.5rem; padding-top: 1rem; border-top: 1px solid var(--border);
            flex-wrap: wrap;
        }}
        .page-meta-right {{ margin-left: auto; }}
        .last-updated {{ font-size: 0.8rem; color: var(--text-muted); }}
        .edit-link {{
            display: inline-flex; align-items: center; gap: 0.35rem;
            font-size: 0.85rem; color: var(--text-muted); text-decoration: none;
        }}
        .edit-link:hover {{ color: var(--accent); }}
        .breadcrumb {{
            display: flex; align-items: center; gap: 0.4rem;
            margin-bottom: 1.5rem; font-size: 0.8rem; flex-wrap: wrap;
        }}
        .breadcrumb a {{
            color: var(--text-muted); text-decoration: none;
            transition: color 0.15s ease;
        }}
        .breadcrumb a:hover {{ color: var(--accent); }}
        .breadcrumb .separator {{ color: var(--text-muted); }}
        .breadcrumb .current {{ color: var(--text-color); font-weight: 500; }}
        .content h1 {{
            font-size: 2.2rem; font-weight: 600; letter-spacing: -0.03em;
            margin-bottom: 1rem; scroll-margin-top: 2rem;
        }}
        .content h2, .content h3, .content h4, .content h5, .content h6 {{
            position: relative;
        }}
        /* Jumping to a heading or footnote via URL hash (TOC links, footnote refs/back-refs)
           should visibly show where you landed, not just scroll there silently. */
        .content h1:target, .content h2:target, .content h3:target,
        .content h4:target, .content h5:target, .content h6:target {{
            animation: bark-target-flash 2s ease-out;
        }}
        .content a.footnote-ref:target,
        .content a.footnote-back-ref:target {{
            background-color: var(--accent-light); outline: 2px solid var(--accent);
            border-radius: 4px; padding: 0 0.2em; scroll-margin-top: 5rem;
        }}
        .content .footnotes li:target {{
            background-color: var(--accent-light); outline: 2px solid var(--accent);
            border-radius: 6px; padding: 0.25rem 0.6rem; margin-left: -0.6rem;
            scroll-margin-top: 5rem;
        }}
        @keyframes bark-target-flash {{
            0%, 40% {{ background-color: var(--accent-light); }}
            100% {{ background-color: transparent; }}
        }}
        @media (prefers-reduced-motion: reduce) {{
            .content h1:target, .content h2:target, .content h3:target,
            .content h4:target, .content h5:target, .content h6:target {{
                animation: none; background-color: var(--accent-light);
            }}
        }}
        .header-anchor {{
            position: absolute; left: -1.2rem; top: 0; bottom: 0;
            display: inline-flex; align-items: center;
            opacity: 0; text-decoration: none; font-weight: 400;
            color: var(--text-muted);
            transition: opacity 0.15s ease, color 0.15s ease;
        }}
        .header-anchor::before {{ content: ""#""; }}
        .header-anchor:hover {{ color: var(--accent); }}
        .content h2:hover .header-anchor, .content h3:hover .header-anchor,
        .content h4:hover .header-anchor, .content h5:hover .header-anchor,
        .content h6:hover .header-anchor, .header-anchor:focus {{
            opacity: 1;
        }}
        .content h2 {{
            font-size: 1.4rem; font-weight: 500; letter-spacing: -0.02em;
            margin-top: 2.5rem; margin-bottom: 1rem; padding-bottom: 0.3rem;
            border-bottom: 1px solid var(--border); scroll-margin-top: 2rem;
        }}
        .content p {{
            color: var(--text-color); margin-bottom: 1.25rem;
            text-decoration-color: var(--border); text-underline-offset: 2px;
        }}
        .content a {{
            color: var(--accent); text-decoration: underline;
            text-decoration-color: var(--border); text-underline-offset: 2px;
            transition: text-decoration-color 0.15s ease;
        }}
        .content a:hover {{
            text-decoration-color: var(--accent);
        }}
        .content ul, .content ol {{
            padding-left: 1.5rem; margin-bottom: 1.25rem;
        }}
        .content li {{
            margin-bottom: 0.4rem;
        }}
        .content li > ul, .content li > ol {{
            margin-top: 0.4rem; margin-bottom: 0;
        }}
        .content hr {{
            border: none; border-top: 1px solid var(--border); margin: 2.5rem 0;
        }}
        .content h3 {{
            font-size: 1.15rem; font-weight: 500; letter-spacing: -0.01em;
            margin-top: 2rem; margin-bottom: 0.75rem; scroll-margin-top: 2rem;
        }}
        .content h4 {{
            font-size: 1rem; font-weight: 500;
            margin-top: 1.5rem; margin-bottom: 0.5rem; scroll-margin-top: 2rem;
        }}
        .content h5, .content h6 {{
            font-size: 0.9rem; font-weight: 600;
            margin-top: 1.25rem; margin-bottom: 0.5rem; scroll-margin-top: 2rem;
        }}
        pre {{
            background-color: var(--code-bg);
            border: 1px solid var(--border);
            border-radius: 8px;
            padding: 1.25rem;
            overflow-x: auto;
            font-family: var(--font-mono);
            font-size: 0.85rem;
            margin: 1.5rem 0;
        }}
        code {{
            font-family: var(--font-mono);
            background-color: var(--code-bg);
            padding: 0.2rem 0.4rem;
            border-radius: 4px;
            font-size: 0.85rem;
        }}
        pre code {{
            padding: 0; background-color: transparent; border-radius: 0;
        }}
        .content h1 code, .content h2 code, .content h3 code,
        .content h4 code, .content h5 code, .content h6 code {{
            background: none; padding: 0; border-radius: 0; font-size: inherit;
        }}
        /* Fenced code block chrome */
        .content div[class^=""language-""] {{
            position: relative;
            margin: 1.5rem 0;
            background-color: var(--code-bg);
            border: 1px solid var(--border);
            border-radius: 8px;
        }}
        .content div[class^=""language-""] pre {{
            margin: 0; border: none; border-radius: 0; padding-top: 2rem;
        }}
        /* Lang badge top-left; Copy/Download buttons (injected by JS) occupy top-right. */
        .content div[class^=""language-""] .lang {{
            position: absolute; top: 0.6rem; left: 1rem; right: auto;
            font-size: 0.7rem; color: var(--text-muted);
            font-family: var(--font-sans); text-transform: lowercase;
            user-select: none; z-index: 1;
        }}
        .content div[class^=""language-""] button.copy {{
            display: none;
        }}
        /* Resolves the --shiki-light/dark vars TextMateSyntaxHighlighter writes per token,
           same prefers-color-scheme + [data-theme] override pattern as the rest of the theme. */
        .shiki, .shiki span {{ color: var(--shiki-light); }}
        .shiki {{ background-color: var(--shiki-light-bg); }}
        @media (prefers-color-scheme: dark) {{
            :root:not([data-theme=""light""]) .shiki, :root:not([data-theme=""light""]) .shiki span {{ color: var(--shiki-dark); }}
            :root:not([data-theme=""light""]) .shiki {{ background-color: var(--shiki-dark-bg); }}
        }}
        :root[data-theme=""dark""] .shiki, :root[data-theme=""dark""] .shiki span {{ color: var(--shiki-dark); }}
        :root[data-theme=""dark""] .shiki {{ background-color: var(--shiki-dark-bg); }}
        .content .line {{ display: inline-block; width: 100%; min-height: 1.4em; }}
        .content .line.highlighted {{
            background-color: var(--accent-light);
            margin: 0 -1.25rem; padding: 0 1.25rem;
            box-shadow: 2px 0 0 var(--accent) inset;
        }}
        .content .line.highlighted.error {{ box-shadow: 2px 0 0 var(--alert-caution) inset; }}
        .content .line.highlighted.warning {{ box-shadow: 2px 0 0 var(--alert-warning) inset; }}
        .content .line.diff {{ margin: 0 -1.25rem; padding: 0 1.25rem; }}
        .content .line.diff.add {{ background-color: rgba(46, 160, 67, 0.15); }}
        .content .line.diff.remove {{ background-color: rgba(248, 81, 73, 0.15); opacity: 0.7; }}
        .content div[class^=""language-""].has-focused-lines .line {{ opacity: 0.5; filter: blur(0.06rem); transition: opacity 0.2s, filter 0.2s; }}
        .content div[class^=""language-""].has-focused-lines .line.has-focus {{ opacity: 1; filter: none; }}
        .content .line-numbers-mode pre {{ padding-left: 2.5rem; }}
        .content .line-numbers-wrapper {{
            position: absolute; top: 1.25rem; left: 0; width: 2rem;
            text-align: right; color: var(--text-muted); font-family: var(--font-mono);
            font-size: 0.85rem; line-height: 1.4em; user-select: none;
        }}
        /* Custom containers: ::: tip / warning / danger / info / details */
        .content .custom-block {{
            margin: 1.25rem 0; padding: 0.1rem 1.25rem;
            border-left: 4px solid var(--border);
            border-radius: 4px; background-color: var(--accent-light);
        }}
        .content .custom-block.tip {{ border-left-color: var(--alert-tip); background-color: color-mix(in srgb, var(--alert-tip) 10%, var(--bg-color)); }}
        .content .custom-block.info {{ border-left-color: var(--alert-note); background-color: color-mix(in srgb, var(--alert-note) 10%, var(--bg-color)); }}
        .content .custom-block.warning {{ border-left-color: var(--alert-warning); background-color: color-mix(in srgb, var(--alert-warning) 10%, var(--bg-color)); }}
        .content .custom-block.danger {{ border-left-color: var(--alert-caution); background-color: color-mix(in srgb, var(--alert-caution) 10%, var(--bg-color)); }}
        .content .custom-block-title {{ font-weight: 700; margin: 0.8rem 0; }}
        .content details.custom-block {{ border-left-color: var(--text-muted); }}
        .content details.custom-block summary {{ font-weight: 700; cursor: pointer; margin: 0.8rem 0; }}
        /* code-group tabs */
        .content .vp-code-group {{ margin: 1.5rem 0; }}
        .content .vp-code-group .tabs {{
            display: flex; gap: 0.25rem; border-bottom: 1px solid var(--border);
        }}
        .content .vp-code-group .tabs input {{ display: none; }}
        .content .vp-code-group .tabs label {{
            padding: 0.5rem 0.9rem; font-size: 0.85rem; color: var(--text-muted);
            cursor: pointer; border-bottom: 2px solid transparent; margin-bottom: -1px;
        }}
        .content .vp-code-group .blocks > div[class^=""language-""] {{ display: none; margin-top: 0; border-top-left-radius: 0; border-top-right-radius: 0; }}
        .content .vp-code-group .blocks > div[class^=""language-""].active {{ display: block; }}
        .content .vp-code-group .tabs label.active-tab {{ color: var(--text-color); border-bottom-color: var(--accent); }}
        .table-wrapper {{
            overflow-x: auto; -webkit-overflow-scrolling: touch;
            margin: 1.5rem 0; border-radius: 6px;
        }}
        .content table {{
            width: 100%; border-collapse: collapse;
            font-size: 0.875rem;
        }}
        .content th, .content td {{
            padding: 0.6rem 1rem; border: 1px solid var(--border);
            text-align: left; vertical-align: top;
        }}
        .content th {{
            background-color: var(--accent-light); font-weight: 600;
            color: var(--text-color);
        }}
        .content tr:nth-child(even) {{
            background-color: var(--code-bg);
        }}
        .code-block-wrapper {{
            position: relative;
        }}
        .code-block-buttons {{
            position: absolute; top: 0.5rem; right: 0.5rem;
            display: flex; gap: 0.25rem; opacity: 0;
            transition: opacity 0.15s ease;
        }}
        .code-block-wrapper:hover .code-block-buttons,
        .code-block-wrapper:focus-within .code-block-buttons {{
            opacity: 1;
        }}
        .code-block-buttons button {{
            background: var(--bg-color); border: 1px solid var(--border);
            border-radius: 4px; padding: 0.25rem 0.5rem;
            font-size: 0.7rem; color: var(--text-muted); cursor: pointer;
            font-family: var(--font-sans); line-height: 1.4;
            transition: color 0.15s ease, border-color 0.15s ease;
        }}
        .code-block-buttons button:hover {{
            color: var(--accent); border-color: var(--accent);
        }}
        .code-block-buttons button.copied {{
            color: var(--accent); border-color: var(--accent);
        }}
        .markdown-alert {{
            padding: 0.75rem 1rem; margin: 1.5rem 0;
            border-left: 4px solid var(--accent);
            border-radius: 0 8px 8px 0;
            background-color: var(--accent-light);
        }}
        .markdown-alert-title {{
            display: flex; align-items: center; gap: 0.5rem;
            font-weight: 600; margin-bottom: 0.25rem;
        }}
        .markdown-alert-title svg {{
            width: 18px; height: 18px; flex-shrink: 0;
            fill: currentColor;
        }}
        .markdown-alert-note {{
            border-left-color: var(--alert-note);
            background-color: color-mix(in srgb, var(--alert-note) 10%, var(--bg-color));
        }}
        .markdown-alert-tip {{
            border-left-color: var(--alert-tip);
            background-color: color-mix(in srgb, var(--alert-tip) 10%, var(--bg-color));
        }}
        .markdown-alert-important {{
            border-left-color: var(--alert-important);
            background-color: color-mix(in srgb, var(--alert-important) 10%, var(--bg-color));
        }}
        .markdown-alert-warning {{
            border-left-color: var(--alert-warning);
            background-color: color-mix(in srgb, var(--alert-warning) 10%, var(--bg-color));
        }}
        .markdown-alert-caution {{
            border-left-color: var(--alert-caution);
            background-color: color-mix(in srgb, var(--alert-caution) 10%, var(--bg-color));
        }}
        .markdown-alert-note .markdown-alert-title svg {{ color: var(--alert-note); }}
        .markdown-alert-tip .markdown-alert-title svg {{ color: var(--alert-tip); }}
        .markdown-alert-important .markdown-alert-title svg {{ color: var(--alert-important); }}
        .markdown-alert-warning .markdown-alert-title svg {{ color: var(--alert-warning); }}
        .markdown-alert-caution .markdown-alert-title svg {{ color: var(--alert-caution); }}
        .markdown-alert > :last-child {{ margin-bottom: 0; }}
        /* Inline badge: <Badge type=""tip"">text</Badge> in raw Markdown. Markdig passes unrecognized
           tags through as raw HTML and lowercases them, so plain CSS on <badge> is enough -- no
           extension needed. Self-closing `<Badge .../>` is NOT supported: HTML has no XML-style
           self-close for unknown elements, so it'd swallow the rest of the paragraph. Always pair
           with a closing tag. */
        badge {{
            display: inline-flex; align-items: center; vertical-align: middle;
            margin: 0 0.3rem; padding: 0.15rem 0.55rem; border-radius: 6px;
            background-color: color-mix(in srgb, var(--alert-tip) 16%, var(--code-bg));
            color: var(--alert-tip); font-family: var(--font-sans);
            font-size: 0.7rem; font-weight: 600; letter-spacing: 0.03em;
            text-transform: uppercase; line-height: 1.5;
        }}
        badge[type=""info""] {{
            background-color: color-mix(in srgb, var(--alert-note) 16%, var(--code-bg));
            color: var(--alert-note);
        }}
        badge[type=""tip""] {{
            background-color: color-mix(in srgb, var(--alert-tip) 16%, var(--code-bg));
            color: var(--alert-tip);
        }}
        badge[type=""warning""] {{
            background-color: color-mix(in srgb, var(--alert-warning) 16%, var(--code-bg));
            color: var(--alert-warning);
        }}
        badge[type=""danger""] {{
            background-color: color-mix(in srgb, var(--alert-caution) 16%, var(--code-bg));
            color: var(--alert-caution);
        }}
        h1 badge, h2 badge, h3 badge, h4 badge {{ font-size: 0.55em; margin-left: 0.5rem; vertical-align: middle; }}
        .pagination {{
            display: flex; justify-content: space-between;
            margin-top: 5rem; padding-top: 2rem;
            border-top: 1px solid var(--border);
        }}
        .pagination-link {{
            text-decoration: none; color: var(--text-muted);
            display: flex; flex-direction: column; gap: 0.25rem;
            transition: color 0.2s ease;
        }}
        .pagination-link:hover {{
            color: var(--accent);
        }}
        .pagination-link .label {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
        }}
        .pagination-link .title {{
            font-size: 1rem; font-weight: 500; color: var(--text-color);
        }}
        .pagination-link:hover .title {{
            color: var(--accent);
        }}
        .pagination-link.next {{
            text-align: right; margin-left: auto;
        }}
        .sidebar-right {{
            padding: 3.5rem 2rem;
            position: sticky; top: var(--topbar-height); align-self: start;
            height: calc(100vh - var(--topbar-height)); overflow-y: auto;
        }}
        .toc-title {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
            color: var(--text-muted); margin-bottom: 1rem; font-weight: 600;
        }}
        /* Containing block for .toc-indicator, which JS positions absolutely regardless of
           .toc-list/.toc-sublist nesting depth. */
        .toc-list-wrapper {{
            position: relative;
        }}
        /* Faint always-visible track; .toc-indicator overlays it and slides to the active item. */
        .toc-list-wrapper::before {{
            content: """"; position: absolute; left: 0; top: 0; bottom: 0; width: 2px;
            border-radius: 2px; background-color: var(--accent-light);
        }}
        .toc-indicator {{
            position: absolute; left: 0; top: 0; width: 2px; border-radius: 2px;
            background-color: var(--accent); opacity: 0; height: 0;
            transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.2s ease, height 0.2s ease;
            will-change: transform;
        }}
        .toc-indicator.visible {{ opacity: 1; }}
        .toc-list {{
            list-style: none; font-size: 0.875rem; padding-left: 0.9rem;
        }}
        .toc-sublist {{
            list-style: none; padding-left: 0.9rem;
        }}
        .toc-item {{
            margin-bottom: 0.1rem;
        }}
        /* Levels differ by indentation and weight/size, not color -- the accent bar is the only color cue. */
        .toc-list > .toc-item > a {{ font-weight: 500; }}
        .toc-list > .toc-item > .toc-sublist > .toc-item > a {{ font-weight: 400; }}
        .toc-list > .toc-item > .toc-sublist > .toc-item > .toc-sublist > .toc-item > a {{
            font-weight: 400; font-size: 0.8rem;
        }}
        .toc-item a {{
            display: block; color: var(--text-muted); line-height: 1.5;
            text-decoration: none; padding: 0.3rem 0.8rem;
            transition: color 0.15s ease;
            overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
        }}
        .toc-item a:hover {{ color: var(--text-color); }}
        .toc-item.active > a {{ color: var(--accent); }}
        .social-links {{
            display: flex; align-items: center; gap: 0.25rem;
        }}
        .content-footer {{
            margin-top: 3rem; padding-top: 1.5rem;
            border-top: 1px solid var(--border);
            font-size: 0.8rem; color: var(--text-muted);
        }}
        .content-footer a {{
            color: var(--accent); text-decoration: none;
        }}
        .content-footer a:hover {{
            text-decoration: underline;
        }}
        .menu-toggle {{
            display: none;
        }}
        .sidebar-overlay {{
            display: none;
        }}
        .toc-inline {{
            display: none;
        }}
        /* Bump touch targets to 44px on coarse-pointer devices, not just by viewport width. */
        @media (hover: none) and (pointer: coarse) {{
            .icon-btn {{ width: 44px; height: 44px; }}
            .nav-item a, .toc-item a {{
                min-height: 44px; display: flex; align-items: center;
            }}
            .code-block-buttons {{
                opacity: 1;
            }}
        }}
        @media (max-width: 1024px) {{
            .layout {{ grid-template-columns: 240px 1fr; }}
            .sidebar-right {{ display: none; }}
            .main-container {{ padding: 2rem 1.5rem; }}
        }}
        @media (min-width: 769px) and (max-width: 1024px) {{
            .toc-inline {{
                display: block; margin-bottom: 2rem;
                border: 1px solid var(--border); border-radius: 8px; padding: 0.5rem 1rem;
            }}
            .toc-inline summary {{
                cursor: pointer; font-size: 0.8rem; font-weight: 600;
                text-transform: uppercase; letter-spacing: 0.05em; color: var(--text-muted);
                padding: 0.5rem 0;
            }}
            .toc-inline .toc-list {{ padding-bottom: 0.5rem; }}
            .toc-inline .toc-item a {{
                padding-left: 0.5rem; border-left: none;
            }}
        }}
        @media (max-width: 768px) {{
            .layout {{ grid-template-columns: 1fr; }}
            .main-container {{ padding: 2rem 1.5rem; }}
            .menu-toggle {{
                display: inline-flex;
            }}
            .top-nav {{ display: none; }}
            .mobile-top-nav {{
                display: block; margin-bottom: 1.25rem; padding-bottom: 1.25rem;
                border-bottom: 1px solid var(--border);
            }}
            .mobile-top-nav-link {{
                display: block; padding: 0.5rem 0; font-size: 0.95rem;
                font-weight: 500; color: var(--text-color); text-decoration: none;
            }}
            .mobile-top-nav-link.active {{ color: var(--accent); }}
            .mobile-top-nav-group summary {{
                padding: 0.5rem 0; font-size: 0.95rem; font-weight: 500;
                color: var(--text-color); cursor: pointer; list-style: none;
            }}
            .mobile-top-nav-group .mobile-top-nav-link {{ padding-left: 1rem; font-weight: 400; }}
            .sidebar-left {{
                position: fixed; top: var(--topbar-height); left: 0;
                height: calc(100vh - var(--topbar-height)); width: 280px;
                max-width: 85vw; z-index: 1003;
                transform: translateX(-100%);
                transition: transform 0.2s ease;
            }}
            .sidebar-left.open {{
                transform: translateX(0);
            }}
            .sidebar-overlay.open {{
                display: block; position: fixed; top: var(--topbar-height); left: 0; right: 0; bottom: 0;
                background: rgba(0, 0, 0, 0.4); z-index: 1001;
            }}
            .nav-item a, .toc-item a {{
                min-height: 44px; display: flex; align-items: center;
            }}
            .search-result-title {{ font-size: 0.95rem; }}
            .search-result-excerpt {{ font-size: 0.8rem; }}
        }}
    </style>
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/katex@0.16.11/dist/katex.min.css"">
    <script defer src=""https://cdn.jsdelivr.net/npm/katex@0.16.11/dist/katex.min.js""></script>
    <script defer src=""https://cdn.jsdelivr.net/npm/katex@0.16.11/dist/contrib/auto-render.min.js""></script>
    <script defer src=""https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js""></script>
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
            <div class=""brand""><a href=""/"">{brandText ?? "Bark"}</a></div>
            <button type=""button"" class=""search-trigger"" id=""search-trigger""
                    aria-haspopup=""dialog"" aria-controls=""search-modal"" aria-label=""Search documentation"">
                <svg viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><circle cx=""11"" cy=""11"" r=""7""/><path d=""M21 21l-4.3-4.3""/></svg>
                <span class=""search-trigger-label"">Search</span>
                <kbd class=""search-trigger-kbd"" id=""search-trigger-kbd"" aria-hidden=""true"">Ctrl K</kbd>
            </button>
        </div>
        {topNavHtml}
        <div class=""topbar-right"">
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
            <div class=""search-modal-footer"" aria-hidden=""true"">
                <span><kbd>&uarr;</kbd><kbd>&darr;</kbd> Navigate</span>
                <span><kbd>Enter</kbd> Select</span>
                <span><kbd>Esc</kbd> Close</span>
            </div>
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
    <script>
        document.addEventListener('DOMContentLoaded', function() {{
            var headings = document.querySelectorAll('.content h1, .content h2, .content h3, .content h4');
            var tocItems = document.querySelectorAll('.toc-item');
            var tocIndicator = document.querySelector('.toc-indicator');
            var tocListWrapper = document.querySelector('.toc-list-wrapper');
            var scrollIndicator = document.getElementById('scroll-indicator');
            var menuToggle = document.getElementById('menu-toggle');
            var sidebarLeft = document.getElementById('sidebar-left');
            var sidebarOverlay = document.getElementById('sidebar-overlay');
            var themeToggle = document.getElementById('theme-toggle');

            if ({(enableLiveReload ? "true" : "false")}) {{
                var currentBuildVersion = {buildVersion};
                setInterval(function() {{
                    fetch('/api/build-version')
                        .then(function(r) {{ return r.json(); }})
                        .then(function(data) {{
                            if (data.version !== currentBuildVersion) {{
                                location.reload();
                            }}
                        }})
                        ['catch'](function() {{}});
                }}, 2000);
            }}

            if (themeToggle) {{
                var prefersDarkQuery = window.matchMedia('(prefers-color-scheme: dark)');

                function isDarkActive() {{
                    var current = document.documentElement.getAttribute('data-theme');
                    return current ? current === 'dark' : prefersDarkQuery.matches;
                }}

                themeToggle.setAttribute('aria-checked', String(isDarkActive()));

                themeToggle.addEventListener('click', function() {{
                    var next = isDarkActive() ? 'light' : 'dark';
                    document.documentElement.setAttribute('data-theme', next);
                    themeToggle.setAttribute('aria-checked', String(next === 'dark'));
                    try {{ localStorage.setItem('bark-theme', next); }} catch (e) {{}}

                    // Mermaid SVGs have their theme's colors baked in at render time, so a CSS
                    // variable flip alone leaves them showing the old theme. Reload is the
                    // simplest reliable way to force a clean re-render with the new theme.
                    if (document.querySelector('.mermaid')) {{
                        location.reload();
                    }}
                }});
            }}

            function closeSidebar() {{
                sidebarLeft.classList.remove('open');
                sidebarOverlay.classList.remove('open');
                menuToggle.setAttribute('aria-expanded', 'false');
                document.documentElement.style.overflow = '';
            }}

            function openSidebar() {{
                sidebarLeft.classList.add('open');
                sidebarOverlay.classList.add('open');
                menuToggle.setAttribute('aria-expanded', 'true');
                // Drawer is position:fixed so it doesn't grow the page, but touch-scroll on it
                // still scrolls <body> underneath without this.
                document.documentElement.style.overflow = 'hidden';
            }}

            menuToggle.addEventListener('click', function() {{
                if (sidebarLeft.classList.contains('open')) {{ closeSidebar(); }} else {{ openSidebar(); }}
            }});
            sidebarOverlay.addEventListener('click', closeSidebar);
            document.addEventListener('keydown', function(e) {{
                if (e.key === 'Escape') closeSidebar();
            }});
            sidebarLeft.querySelectorAll('.nav-item a').forEach(function(link) {{
                link.addEventListener('click', closeSidebar);
            }});

            var touchStartX = null;
            sidebarLeft.addEventListener('touchstart', function(e) {{
                touchStartX = e.touches[0].clientX;
            }}, {{ passive: true }});
            sidebarLeft.addEventListener('touchmove', function(e) {{
                if (touchStartX === null) return;
                var deltaX = e.touches[0].clientX - touchStartX;
                if (deltaX < -40) {{
                    closeSidebar();
                    touchStartX = null;
                }}
            }}, {{ passive: true }});
            sidebarLeft.addEventListener('touchend', function() {{ touchStartX = null; }});

            function updateScrollProgress() {{
                if (!scrollIndicator) return;
                var winScroll = document.documentElement.scrollTop || document.body.scrollTop;
                var height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
                var scrolled = height > 0 ? (winScroll / height) * 100 : 0;
                scrollIndicator.style.width = scrolled + '%';
            }}

            window.addEventListener('scroll', updateScrollProgress);

            // Moves the sliding accent bar to the active item's position. transform/height
            // both transition via CSS, so this only needs to set the target values.
            function updateTocIndicator() {{
                if (!tocIndicator || !tocListWrapper) return;
                var activeLink = tocListWrapper.querySelector('.toc-item.active > a');
                if (!activeLink) {{
                    tocIndicator.classList.remove('visible');
                    return;
                }}
                var wrapperTop = tocListWrapper.getBoundingClientRect().top;
                var linkRect = activeLink.getBoundingClientRect();
                tocIndicator.style.transform = 'translateY(' + (linkRect.top - wrapperTop) + 'px)';
                tocIndicator.style.height = linkRect.height + 'px';
                tocIndicator.classList.add('visible');
            }}

            // toc-item markup renders twice (tablet dropdown + desktop sidebar), so activation
            // is keyed by heading id and applies to every matching copy.
            function setActiveTocId(id) {{
                tocItems.forEach(function(item) {{
                    var link = item.querySelector('a');
                    var matches = link && link.getAttribute('href') === '#' + id;
                    item.classList.toggle('active', !!matches);
                }});
                updateTocIndicator();
            }}

            var observer = new IntersectionObserver(function(entries) {{
                entries.forEach(function(entry) {{
                    if (entry.isIntersecting) {{
                        setActiveTocId(entry.target.getAttribute('id'));
                    }}
                }});
            }}, {{ root: null, rootMargin: '-5% 0px -75% 0px', threshold: 0 }});

            headings.forEach(function(h) {{ observer.observe(h); }});

            // Landing on a #hash scrolls there natively before JS runs, but a heading flush at
            // the top edge sits outside the observer's ~20% rootMargin and never fires. Set it
            // explicitly instead of waiting on the observer.
            if (location.hash) {{
                setActiveTocId(location.hash.slice(1));
            }}
            window.addEventListener('hashchange', function() {{
                if (location.hash) setActiveTocId(location.hash.slice(1));
            }});

            // At the very bottom of the page the last heading can sit above the observer's
            // ~20% rootMargin and never re-fire, leaving the second-to-last item stuck active.
            function checkScrolledToBottom() {{
                var atBottom = window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 2;
                if (atBottom && headings.length > 0) {{
                    setActiveTocId(headings[headings.length - 1].getAttribute('id'));
                }}
            }}

            window.addEventListener('scroll', checkScrolledToBottom, {{ passive: true }});
            window.addEventListener('resize', updateTocIndicator);
            checkScrolledToBottom();

            tocItems.forEach(function(item) {{
                var link = item.querySelector('a');
                if (link) {{
                    link.addEventListener('click', function() {{
                        setActiveTocId(link.getAttribute('href').slice(1));
                    }});
                }}
            }});

            // Search modal: combobox/listbox a11y pattern (aria-activedescendant, roving
            // aria-selected) plus a manual focus trap since <dialog> isn't used.
            var searchTrigger = document.getElementById('search-trigger');
            var searchTriggerKbd = document.getElementById('search-trigger-kbd');
            var searchOverlay = document.getElementById('search-overlay');
            var searchModalInput = document.getElementById('search-modal-input');
            var searchModalResults = document.getElementById('search-modal-results');
            var searchModalClose = document.getElementById('search-modal-close');
            var searchModalStatus = document.getElementById('search-modal-status');
            var searchTimeout;
            var searchActiveIndex = -1;
            var searchLastFocused = null;
            var searchRequestId = 0;
            var searchHintHtml = '<div class=""search-result-empty"" role=""status"">Type to search documentation.</div>';

            if (searchTriggerKbd && /Mac|iPhone|iPad/.test(navigator.platform || '')) {{
                searchTriggerKbd.textContent = '⌘K';
            }}

            function escapeHtml(value) {{
                var div = document.createElement('div');
                div.textContent = value == null ? '' : value;
                return div.innerHTML;
            }}

            function escapeRegExp(value) {{
                return value.replace(/[.*+?^${{}}()|[\]\\]/g, '\\$&');
            }}

            // Escapes first, then wraps matches in the *escaped* string -- query terms never
            // contain markup themselves, so highlighting after escaping can't reopen the hole.
            function highlightMatches(value, terms) {{
                var escaped = escapeHtml(value);
                if (!terms.length) return escaped;
                var pattern = new RegExp('(' + terms.map(escapeRegExp).join('|') + ')', 'ig');
                return escaped.replace(pattern, '<mark class=""search-highlight"">$1</mark>');
            }}

            function getSearchResultItems() {{
                return Array.prototype.slice.call(searchModalResults.querySelectorAll('.search-result-item'));
            }}

            function setSearchActiveIndex(index) {{
                var items = getSearchResultItems();
                if (items.length === 0) {{
                    searchActiveIndex = -1;
                    searchModalInput.removeAttribute('aria-activedescendant');
                    return;
                }}
                searchActiveIndex = (index + items.length) % items.length;
                items.forEach(function(item, i) {{
                    var isActive = i === searchActiveIndex;
                    item.classList.toggle('active', isActive);
                    item.setAttribute('aria-selected', isActive ? 'true' : 'false');
                }});
                var activeItem = items[searchActiveIndex];
                searchModalInput.setAttribute('aria-activedescendant', activeItem.id);
                activeItem.scrollIntoView({{ block: 'nearest' }});
            }}

            function openSearchModal() {{
                searchLastFocused = document.activeElement;
                searchOverlay.hidden = false;
                requestAnimationFrame(function() {{ searchOverlay.classList.add('open'); }});
                document.documentElement.style.overflow = 'hidden';
                searchModalInput.value = '';
                searchModalInput.focus();
                searchModalResults.innerHTML = searchHintHtml;
                searchModalInput.setAttribute('aria-expanded', 'false');
                searchModalInput.removeAttribute('aria-activedescendant');
                searchModalStatus.textContent = '';
                searchActiveIndex = -1;
            }}

            function closeSearchModal() {{
                searchOverlay.classList.remove('open');
                searchOverlay.hidden = true;
                document.documentElement.style.overflow = '';
                if (searchLastFocused && typeof searchLastFocused.focus === 'function') {{
                    searchLastFocused.focus();
                }}
            }}

            function isSearchModalOpen() {{
                return !searchOverlay.hidden;
            }}

            if (searchTrigger) {{
                searchTrigger.addEventListener('click', openSearchModal);
            }}
            searchModalClose.addEventListener('click', closeSearchModal);
            searchOverlay.addEventListener('mousedown', function(e) {{
                if (e.target === searchOverlay) closeSearchModal();
            }});

            document.addEventListener('keydown', function(e) {{
                var isCtrlK = (e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k';
                if (isCtrlK) {{
                    e.preventDefault();
                    isSearchModalOpen() ? closeSearchModal() : openSearchModal();
                    return;
                }}
                if (!isSearchModalOpen()) return;
                if (e.key === 'Escape') {{
                    e.preventDefault();
                    closeSearchModal();
                    return;
                }}
                if (e.key === 'Tab') {{
                    // Manual focus trap -- modal only contains the input and close button as
                    // focusable elements, so Tab/Shift+Tab just toggles between the two.
                    var focusable = [searchModalInput, searchModalClose];
                    var currentIndex = focusable.indexOf(document.activeElement);
                    e.preventDefault();
                    var nextIndex = e.shiftKey
                        ? (currentIndex <= 0 ? focusable.length - 1 : currentIndex - 1)
                        : (currentIndex === -1 || currentIndex === focusable.length - 1 ? 0 : currentIndex + 1);
                    focusable[nextIndex].focus();
                    return;
                }}
                if (e.key === 'ArrowDown') {{
                    e.preventDefault();
                    if (getSearchResultItems().length) setSearchActiveIndex(searchActiveIndex + 1);
                    return;
                }}
                if (e.key === 'ArrowUp') {{
                    e.preventDefault();
                    if (getSearchResultItems().length) setSearchActiveIndex(searchActiveIndex - 1);
                    return;
                }}
                if (e.key === 'Enter') {{
                    var items = getSearchResultItems();
                    if (searchActiveIndex >= 0 && items[searchActiveIndex]) {{
                        e.preventDefault();
                        items[searchActiveIndex].click();
                    }}
                }}
            }});

            // Mouse and keyboard share one active index so a hover never silently
            // disagrees with the last arrow-key position.
            searchModalResults.addEventListener('mouseover', function(e) {{
                var item = e.target.closest('.search-result-item');
                if (!item) return;
                var index = getSearchResultItems().indexOf(item);
                if (index !== -1) setSearchActiveIndex(index);
            }});

            searchModalInput.addEventListener('input', function() {{
                clearTimeout(searchTimeout);
                var query = searchModalInput.value.trim();
                searchActiveIndex = -1;
                searchRequestId += 1;
                if (query.length < 2) {{
                    searchModalResults.innerHTML = searchHintHtml;
                    searchModalInput.setAttribute('aria-expanded', 'false');
                    searchModalStatus.textContent = '';
                    return;
                }}
                searchModalResults.innerHTML = '<div class=""search-result-empty"" role=""status"">Searching&hellip;</div>';
                var requestId = searchRequestId;
                searchTimeout = setTimeout(function() {{
                    fetch('/api/search?q=' + encodeURIComponent(query))
                        .then(function(r) {{ return r.json(); }})
                        .then(function(data) {{
                            if (requestId !== searchRequestId) return; // a newer keystroke superseded this request
                            if (data.length === 0) {{
                                searchModalResults.innerHTML = '<div class=""search-result-empty"" role=""status"">No results found.</div>';
                                searchModalStatus.textContent = 'No results found.';
                            }} else {{
                                var terms = query.split(/\s+/).filter(Boolean);
                                var html = '';
                                data.forEach(function(r, i) {{
                                    html += '<a href=""/' + r.path + '"" class=""search-result-item"" role=""option"" id=""search-result-' + i + '"" aria-selected=""false"" tabindex=""-1"">' +
                                        '<div class=""search-result-title"">' + highlightMatches(r.title, terms) + '</div>' +
                                        (r.excerpt ? '<div class=""search-result-excerpt"">' + highlightMatches(r.excerpt, terms) + '</div>' : '') +
                                        '</a>';
                                }});
                                searchModalResults.innerHTML = html;
                                searchModalStatus.textContent = data.length + (data.length === 1 ? ' result found.' : ' results found.');
                            }}
                            searchModalInput.setAttribute('aria-expanded', 'true');
                        }})
                        ['catch'](function() {{
                            if (requestId !== searchRequestId) return;
                            searchModalResults.innerHTML = '<div class=""search-result-empty"" role=""status"">Something went wrong. Try again.</div>';
                            searchModalStatus.textContent = 'Search failed.';
                        }});
                }}, 200);
            }});

            // Wrap tables so wide ones scroll inside their own box instead of widening the page;
            // a bare overflow-x:auto on the table itself doesn't reliably contain it.
            var tables = document.querySelectorAll('.content table');
            tables.forEach(function(table) {{
                var wrapper = document.createElement('div');
                wrapper.className = 'table-wrapper';
                table.parentNode.insertBefore(wrapper, table);
                wrapper.appendChild(table);
            }});

            // Copy and download buttons for code blocks
            var codeBlocks = document.querySelectorAll('.content pre');
            codeBlocks.forEach(function(pre) {{
                var wrapper = document.createElement('div');
                wrapper.className = 'code-block-wrapper';
                pre.parentNode.insertBefore(wrapper, pre);
                wrapper.appendChild(pre);

                var buttons = document.createElement('div');
                buttons.className = 'code-block-buttons';

                var copyBtn = document.createElement('button');
                copyBtn.textContent = 'Copy';
                copyBtn.addEventListener('click', function() {{
                    var code = pre.querySelector('code');
                    var text = code ? code.textContent : pre.textContent;
                    navigator.clipboard.writeText(text).then(function() {{
                        copyBtn.textContent = 'Copied!';
                        copyBtn.classList.add('copied');
                        setTimeout(function() {{
                            copyBtn.textContent = 'Copy';
                            copyBtn.classList.remove('copied');
                        }}, 2000);
                    }})['catch'](function() {{
                        copyBtn.textContent = 'Failed';
                        setTimeout(function() {{
                            copyBtn.textContent = 'Copy';
                        }}, 2000);
                    }});
                }});
                buttons.appendChild(copyBtn);

                var downloadBtn = document.createElement('button');
                downloadBtn.textContent = 'Download';
                downloadBtn.addEventListener('click', function() {{
                    var code = pre.querySelector('code');
                    var text = code ? code.textContent : pre.textContent;
                    var blob = new Blob([text], {{ type: 'text/plain' }});
                    var url = URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    a.href = url;
                    a.download = 'code.txt';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    URL.revokeObjectURL(url);
                }});
                buttons.appendChild(downloadBtn);

                wrapper.appendChild(buttons);
            }});

            // code-group tab switching
            var codeGroups = document.querySelectorAll('.vp-code-group');
            codeGroups.forEach(function(group) {{
                var inputs = group.querySelectorAll('.tabs input');
                var labels = group.querySelectorAll('.tabs label');
                var blocks = group.querySelectorAll('.blocks > [class^=""language-""]');
                inputs.forEach(function(input, index) {{
                    input.addEventListener('change', function() {{
                        blocks.forEach(function(block, blockIndex) {{
                            block.classList.toggle('active', blockIndex === index);
                        }});
                        labels.forEach(function(label, labelIndex) {{
                            label.classList.toggle('active-tab', labelIndex === index);
                        }});
                    }});
                    if (input.checked) labels[index] && labels[index].classList.add('active-tab');
                }});
            }});

            // KaTeX rendering for Markdig's \(...\) / \[...\] math output
            if (window.renderMathInElement) {{
                document.querySelectorAll('.content').forEach(function(el) {{
                    window.renderMathInElement(el, {{
                        delimiters: [
                            {{ left: '\\\\[', right: '\\\\]', display: true }},
                            {{ left: '\\\\(', right: '\\\\)', display: false }}
                        ]
                    }});
                }});
            }}

            // Mermaid bakes colors into the SVG at render time and ignores CSS variables, so the
            // current theme must be passed in explicitly; a full reload is needed to redraw if
            // the user flips the theme toggle after diagrams are on the page.
            var mermaidBlocks = document.querySelectorAll('.content div[class^=""language-mermaid""]');
            if (mermaidBlocks.length && window.mermaid) {{
                var currentTheme = document.documentElement.getAttribute('data-theme');
                var prefersDarkNow = window.matchMedia('(prefers-color-scheme: dark)').matches;
                var mermaidIsDark = currentTheme ? currentTheme === 'dark' : prefersDarkNow;
                window.mermaid.initialize({{ startOnLoad: false, theme: mermaidIsDark ? 'dark' : 'default' }});
                mermaidBlocks.forEach(function(block) {{
                    var code = block.querySelector('code');
                    if (!code) return;
                    var graphDiv = document.createElement('div');
                    graphDiv.className = 'mermaid';
                    graphDiv.textContent = code.textContent;
                    block.replaceWith(graphDiv);
                }});
                window.mermaid.run();
            }}
        }});
    </script>
</body>
</html>";
    }

    public static string Get404Layout(Func<string?, string> htmlEncode)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
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
        <a href=""/"">Return home</a>
    </div>
</body>
</html>";
    }

    public static string HtmlEncode(string? value) =>
        value != null ? System.Net.WebUtility.HtmlEncode(value) : string.Empty;

    private static string BuildFaviconLink(string? favicon)
    {
        if (string.IsNullOrWhiteSpace(favicon))
            return string.Empty;

        var isUrl = favicon.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || favicon.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || favicon.StartsWith('/');

        if (isUrl)
            return $"<link rel=\"icon\" href=\"{HtmlEncode(favicon)}\">";

        var svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>{favicon}</text></svg>";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return $"<link rel=\"icon\" href=\"data:image/svg+xml;base64,{base64}\">";
    }
}
