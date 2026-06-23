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
        string? socialLinksHtml = null)
    {
        var darkModeMediaQuery = enableDarkMode ? @"@media (prefers-color-scheme: dark) {
            :root {
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
                --alert-caution: #e65030;
            }
        }" : "";

        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{HtmlEncode(title)}</title>
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
            background-color: var(--accent); width: 0%; z-index: 1000;
            transition: width 0.15s ease;
        }}
        :focus-visible {{
            outline: 2px solid var(--accent);
            outline-offset: 2px;
        }}
        .layout {{
            display: grid;
            grid-template-columns: 260px 1fr 220px;
            min-height: 100vh;
        }}
        .sidebar-left {{
            background-color: var(--sidebar-bg);
            border-right: 1px solid var(--border);
            padding: 2.5rem 1.5rem;
            position: sticky; top: 0; height: 100vh; overflow-y: auto;
        }}
        .brand a {{
            font-size: 1.1rem; font-weight: 600; letter-spacing: -0.02em;
            color: var(--text-color); text-decoration: none;
        }}
        .brand a:hover {{ color: var(--accent); }}
        .brand {{
            margin-bottom: 2.5rem;
        }}
        .search-box {{
            width: 100%; padding: 0.5rem 0.75rem;
            border: 1px solid var(--border); border-radius: 6px;
            background-color: var(--bg-color); color: var(--text-color);
            font-family: var(--font-sans); font-size: 0.8rem;
            margin-bottom: 2rem; outline: none;
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
        .nav-group {{
            margin-bottom: 2rem;
        }}
        .nav-group-title {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
            color: var(--text-muted); margin-bottom: 0.75rem; font-weight: 600;
        }}
        .nav-list {{
            list-style: none;
        }}
        .nav-item a {{
            display: block; padding: 0.4rem 0.75rem;
            color: var(--text-muted); text-decoration: none; font-size: 0.9rem;
            border-radius: 6px; margin-left: -0.75rem;
            transition: all 0.15s ease;
        }}
        .nav-item a:hover {{
            color: var(--text-color); background-color: var(--code-bg);
        }}
        .nav-item.active a {{
            color: var(--accent); background-color: var(--accent-light); font-weight: 500;
        }}
        .main-container {{
            padding: 3rem 4rem;
            max-width: 800px; justify-self: center; width: 100%;
        }}
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
        .content h2 {{
            font-size: 1.4rem; font-weight: 500; letter-spacing: -0.02em;
            margin-top: 2.5rem; margin-bottom: 1rem; padding-bottom: 0.3rem;
            border-bottom: 1px solid var(--border); scroll-margin-top: 2rem;
        }}
        .content p {{
            color: var(--text-color); margin-bottom: 1.25rem;
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
        .code-block-wrapper:hover .code-block-buttons {{
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
            padding: 3rem 1.5rem;
            position: sticky; top: 0; height: 100vh; overflow-y: auto;
        }}
        .toc-title {{
            font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em;
            color: var(--text-muted); margin-bottom: 0.75rem; font-weight: 600;
        }}
        .toc-list {{
            list-style: none;
        }}
        .toc-item a {{
            display: block; font-size: 0.8rem; color: var(--text-muted);
            text-decoration: none; padding: 0.25rem 0;
            transition: color 0.15s ease, border-color 0.15s ease;
            border-left: 2px solid transparent; padding-left: 0.5rem;
            overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
        }}
        .toc-item a:hover {{
            color: var(--text-color);
        }}
        .toc-item.active a {{
            color: var(--accent); border-left-color: var(--accent); font-weight: 500;
        }}
        .social-links {{
            display: flex; gap: 0.75rem; margin-top: 2rem;
        }}
        .social-links a {{
            color: var(--text-muted); text-decoration: none;
            transition: color 0.15s ease;
        }}
        .social-links a:hover {{ color: var(--accent); }}
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
        @media (max-width: 1024px) {{
            .layout {{ grid-template-columns: 240px 1fr; }}
            .sidebar-right {{ display: none; }}
        }}
        @media (max-width: 768px) {{
            .layout {{ grid-template-columns: 1fr; }}
            .sidebar-left {{ display: none; }}
            .main-container {{ padding: 2rem 1.5rem; }}
        }}
    </style>
</head>
<body>
    <div id=""scroll-indicator""></div>
    <div class=""layout"">
        <aside class=""sidebar-left"">
            <div class=""brand""><a href=""/"">{brandText ?? "Bark"}</a></div>
            <input type=""text"" class=""search-box"" id=""search-input""
                   placeholder=""Search..."" autocomplete=""off"">
            <div class=""search-results"" id=""search-results""></div>
            {navigationHtml}
            {socialLinksHtml}
        </aside>
        <main class=""main-container"">
            <nav class=""breadcrumb"">
                {breadcrumbHtml}
            </nav>
            <article class=""content"">
                {content}
                {paginationHtml}
                {footerHtml}
            </article>
        </main>
        <aside class=""sidebar-right"">
            <div class=""toc-title"">On This Page</div>
            <ul class=""toc-list"">
                {tocHtml}
            </ul>
        </aside>
    </div>
    <script>
        document.addEventListener('DOMContentLoaded', function() {{
            var headings = document.querySelectorAll('.content h1, .content h2');
            var tocItems = document.querySelectorAll('.toc-item');
            var scrollIndicator = document.getElementById('scroll-indicator');
            var searchInput = document.getElementById('search-input');
            var searchResults = document.getElementById('search-results');

            function updateScrollProgress() {{
                var winScroll = document.documentElement.scrollTop || document.body.scrollTop;
                var height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
                var scrolled = height > 0 ? (winScroll / height) * 100 : 0;
                scrollIndicator.style.width = scrolled + '%';
            }}

            window.addEventListener('scroll', updateScrollProgress);

            var observer = new IntersectionObserver(function(entries) {{
                entries.forEach(function(entry) {{
                    if (entry.isIntersecting) {{
                        var id = entry.target.getAttribute('id');
                        tocItems.forEach(function(item) {{
                            var link = item.querySelector('a');
                            if (link && link.getAttribute('href') === '#' + id) {{
                                item.classList.add('active');
                            }} else {{
                                item.classList.remove('active');
                            }}
                        }});
                    }}
                }});
            }}, {{ root: null, rootMargin: '-5% 0px -75% 0px', threshold: 0 }});

            headings.forEach(function(h) {{ observer.observe(h); }});

            tocItems.forEach(function(item) {{
                var link = item.querySelector('a');
                if (link) {{
                    link.addEventListener('click', function() {{
                        tocItems.forEach(function(i) {{ i.classList.remove('active'); }});
                        item.classList.add('active');
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
                    return;
                }}
                searchTimeout = setTimeout(function() {{
                    fetch('/api/search?q=' + encodeURIComponent(query))
                        .then(function(r) {{ return r.json(); }})
                        .then(function(data) {{
                            if (data.length === 0) {{
                                searchResults.innerHTML = '<div class=""search-result-item"" style=""color:var(--text-muted);font-size:0.8rem;padding:0.75rem;"">No results found.</div>';
                            }} else {{
                                var html = '';
                                data.forEach(function(r) {{
                                    html += '<a href=""/' + r.path + '"" class=""search-result-item"">' +
                                        '<div class=""search-result-title"">' + r.title + '</div>' +
                                        (r.excerpt ? '<div class=""search-result-excerpt"">' + r.excerpt + '</div>' : '') +
                                        '</a>';
                                }});
                                searchResults.innerHTML = html;
                            }}
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
}
