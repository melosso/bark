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
            <input type=""text"" class=""search-box"" id=""search-input""
                   placeholder=""Search..."" autocomplete=""off""
                   role=""combobox"" aria-expanded=""false"" aria-controls=""search-results""
                   aria-autocomplete=""list"" aria-label=""Search documentation"">
            <div class=""search-results"" id=""search-results"" role=""listbox""></div>
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
        // Home pages never show "last updated" or prev/next pagination, regardless of what the
        // caller passes in -- vitepress's default theme has the same rule for the home layout.
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
                --alert-note: #7dab86;
                --alert-tip: #a07da3;
                --alert-important: #a3a07d;
                --alert-warning: #e6a020;
                --alert-caution: #e65030;";

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
            ? @"<button type=""button"" class=""theme-toggle icon-btn"" id=""theme-toggle"" aria-label=""Toggle dark mode"">
                <svg class=""icon-sun"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" aria-hidden=""true""><circle cx=""12"" cy=""12"" r=""4""/><path d=""M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41""/></svg>
                <svg class=""icon-moon"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" aria-hidden=""true""><path d=""M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z""/></svg>
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
            --alert-note: #6b8e74;
            --alert-tip: #8b6b8e;
            --alert-important: #8e8b6b;
            --alert-warning: #cc8800;
            --alert-caution: #cc3300;
            --font-sans: system-ui, -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;
            --font-mono: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        }}
        {darkModeMediaQuery}
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
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
           < scroll-indicator 1101. The indicator sits above everything, including the topbar
           it's pinned alongside at top:0 -- it was previously below the topbar (1000 < 1002)
           and invisible behind its opaque background the entire time. */
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
            grid-template-columns: 270px 1fr 250px;
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
        .theme-toggle .icon-moon {{ display: none; }}
        :root[data-theme=""dark""] .theme-toggle .icon-sun {{ display: none; }}
        :root[data-theme=""dark""] .theme-toggle .icon-moon {{ display: block; }}
        @media (prefers-color-scheme: dark) {{
            :root:not([data-theme=""light""]) .theme-toggle .icon-sun {{ display: none; }}
            :root:not([data-theme=""light""]) .theme-toggle .icon-moon {{ display: block; }}
        }}
        .search-box {{
            width: 100%; padding: 0.65rem 0.85rem;
            border: 1px solid var(--border); border-radius: 6px;
            background-color: var(--bg-color); color: var(--text-color);
            font-family: var(--font-sans); font-size: 0.8rem;
            margin-bottom: 2.25rem; outline: none;
            transition: border-color 0.15s ease;
        }}
        .search-box:focus {{
            border-color: var(--accent);
        }}
        .search-results {{
            display: none; margin-bottom: 1rem;
            border: 1px solid var(--border); border-radius: 6px;
            background-color: var(--bg-color); overflow: hidden;
        }}
        .search-results.visible {{ display: block; }}
        .search-result-item {{
            display: block; padding: 0.6rem 0.75rem;
            text-decoration: none; border-bottom: 1px solid var(--border);
            transition: background-color 0.1s ease;
        }}
        .search-result-item:last-child {{ border-bottom: none; }}
        .search-result-item:hover {{ background-color: var(--accent-light); }}
        .search-result-title {{ font-weight: 500; color: var(--text-color); font-size: 0.85rem; }}
        .search-result-excerpt {{ font-size: 0.75rem; color: var(--text-muted); margin-top: 0.2rem; }}
        .search-result-empty {{ color: var(--text-muted); font-size: 0.8rem; padding: 0.75rem; }}
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
        /* Recursive, optionally-collapsible sidebar (explicit config.json nav/sidebar).
           .sidebar-group-title is always a plain <div> -- never styled directly on <summary>,
           since browsers apply enough UA-default styling to <summary> (list-item display,
           native marker box) that no class override fully neutralizes it across engines.
           summary.sidebar-group-summary is a near-invisible click target that just wraps
           that div, so the collapsible and static variants render identically. */
        /* Single indent system: every .sidebar-group-items adds 0.9rem of left padding, and
           since groups nest inside .sidebar-group-items, depth compounds naturally -- no
           per-level overrides needed on the rows themselves. Level 0 (the root list) gets no
           padding, so the first visible indent step only shows up once you're actually inside
           a group. */
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
        /* Collapsible and static groups use identical heading typography -- only the presence
           of a caret should tell them apart, not weight/case/color. */
        .sidebar-group-title h2, .sidebar-group-title h3 {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
            color: var(--text-muted); font-weight: 600; flex: 1; margin: 0;
        }}
        /* Active-branch ancestors get a text-color cue only. The stronger accent-tint
           background is reserved for the one actual active leaf link (.sidebar-link.is-active),
           so a deeply nested active page doesn't stack a highlighted background at every level. */
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
        /* vitepress-compatible fenced code block chrome */
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
        /* Lang badge sits top-left; the legacy Copy/Download buttons (injected by JS into a
           .code-block-wrapper around <pre>) occupy top-right, so the two never overlap. */
        .content div[class^=""language-""] .lang {{
            position: absolute; top: 0.6rem; left: 1rem; right: auto;
            font-size: 0.7rem; color: var(--text-muted);
            font-family: var(--font-sans); text-transform: lowercase;
            user-select: none; z-index: 1;
        }}
        .content div[class^=""language-""] button.copy {{
            display: none;
        }}
        /* TextMateSyntaxHighlighter writes --shiki-light/--shiki-dark per token and
           --shiki-light-bg/--shiki-dark-bg on <pre>, but nothing reads them without this:
           same light/dark resolution pattern as the rest of the theme (system preference via
           prefers-color-scheme, overridable per-page via the [data-theme] toggle). */
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
        /* vitepress custom containers (::: tip / warning / danger / info / details) */
        .content .custom-block {{
            margin: 1.25rem 0; padding: 0.1rem 1.25rem;
            border-left: 4px solid var(--border);
            border-radius: 4px; background-color: var(--accent-light);
        }}
        .content .custom-block.tip {{ border-left-color: var(--alert-tip); }}
        .content .custom-block.info {{ border-left-color: var(--alert-note); }}
        .content .custom-block.warning {{ border-left-color: var(--alert-warning); background-color: rgba(230, 160, 32, 0.08); }}
        .content .custom-block.danger {{ border-left-color: var(--alert-caution); background-color: rgba(230, 80, 48, 0.08); }}
        .content .custom-block-title {{ font-weight: 700; margin: 0.8rem 0; }}
        .content details.custom-block {{ border-left-color: var(--text-muted); }}
        .content details.custom-block summary {{ font-weight: 700; cursor: pointer; margin: 0.8rem 0; }}
        /* vitepress code-group tabs */
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
        .content table {{
            width: 100%; border-collapse: collapse; margin: 1.5rem 0;
            font-size: 0.875rem; display: block; overflow-x: auto;
            max-width: 100%;
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
        .markdown-alert-note {{ border-left-color: var(--alert-note); }}
        .markdown-alert-tip {{ border-left-color: var(--alert-tip); }}
        .markdown-alert-important {{ border-left-color: var(--alert-important); }}
        .markdown-alert-warning {{ border-left-color: var(--alert-warning); }}
        .markdown-alert-caution {{ border-left-color: var(--alert-caution); }}
        .markdown-alert-note .markdown-alert-title svg {{ color: var(--alert-note); }}
        .markdown-alert-tip .markdown-alert-title svg {{ color: var(--alert-tip); }}
        .markdown-alert-important .markdown-alert-title svg {{ color: var(--alert-important); }}
        .markdown-alert-warning .markdown-alert-title svg {{ color: var(--alert-warning); }}
        .markdown-alert-caution .markdown-alert-title svg {{ color: var(--alert-caution); }}
        .markdown-alert > :last-child {{ margin-bottom: 0; }}
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
        /* The sliding accent bar (.toc-indicator) is positioned absolutely within this wrapper
           and translated to the active item's offset by JS, so it needs to be the bar's
           containing block regardless of how deep .toc-list/.toc-sublist nesting goes. */
        .toc-list-wrapper {{
            position: relative;
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
        /* Levels differentiate through indentation (above, via .toc-sublist) and font weight/size,
           not color, since the active accent bar is the one color cue that should stand out. */
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
        /* Touch targets: bump icon buttons and list items to 44px on any coarse-pointer device,
           regardless of viewport width, so tablets (769-1024px) aren't missed by width-only rules. */
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
        </div>
        {topNavHtml}
        <div class=""topbar-right"">
            {socialLinksHtml}
            {themeToggleHtml}
        </div>
    </header>
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
            var searchInput = document.getElementById('search-input');
            var searchResults = document.getElementById('search-results');
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
                themeToggle.addEventListener('click', function() {{
                    var current = document.documentElement.getAttribute('data-theme');
                    var prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
                    var isDark = current ? current === 'dark' : prefersDark;
                    var next = isDark ? 'light' : 'dark';
                    document.documentElement.setAttribute('data-theme', next);
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
            }}

            function openSidebar() {{
                sidebarLeft.classList.add('open');
                sidebarOverlay.classList.add('open');
                menuToggle.setAttribute('aria-expanded', 'true');
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

            // The same toc-item markup is rendered twice (the tablet inline <details> dropdown
            // and the desktop right sidebar), so activation is keyed by heading id, not by a
            // single DOM node, and applies to every copy that matches.
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

            // IntersectionObserver's rootMargin only watches the top ~20% of the viewport, so
            // once you're scrolled to the very bottom of the page, the last heading can sit
            // above that zone (nothing left to scroll it down into view) and never re-fires an
            // intersection event. Without this, the second-to-last item stays stuck active
            // forever once you reach the bottom.
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

            var searchTimeout;
            searchInput.addEventListener('input', function() {{
                clearTimeout(searchTimeout);
                var query = searchInput.value.trim();
                if (query.length < 2) {{
                    searchResults.classList.remove('visible');
                    searchResults.innerHTML = '';
                    searchInput.setAttribute('aria-expanded', 'false');
                    return;
                }}
                searchTimeout = setTimeout(function() {{
                    fetch('/api/search?q=' + encodeURIComponent(query))
                        .then(function(r) {{ return r.json(); }})
                        .then(function(data) {{
                            if (data.length === 0) {{
                                searchResults.innerHTML = '<div class=""search-result-empty"" role=""status"">No results found.</div>';
                            }} else {{
                                var html = '';
                                data.forEach(function(r) {{
                                    html += '<a href=""/' + r.path + '"" class=""search-result-item"" role=""option"">' +
                                        '<div class=""search-result-title"">' + r.title + '</div>' +
                                        (r.excerpt ? '<div class=""search-result-excerpt"">' + r.excerpt + '</div>' : '') +
                                        '</a>';
                                }});
                                searchResults.innerHTML = html;
                            }}
                            searchInput.setAttribute('aria-expanded', 'true');
                            searchResults.classList.add('visible');
                        }});
                }}, 200);
            }});

            document.addEventListener('click', function(e) {{
                if (!e.target.closest('.sidebar-left')) {{
                    searchResults.classList.remove('visible');
                }}
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

            // vitepress code-group tab switching
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

            // Mermaid diagrams: ```mermaid fenced blocks. Mermaid bakes colors into the
            // rendered SVG at render time, it doesn't react to CSS variables, so it needs the
            // current effective theme passed in explicitly, and a re-render (full reload, the
            // simplest reliable way to force mermaid to redraw with new colors) when the user
            // flips the theme toggle after diagrams are already on the page.
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
