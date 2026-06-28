namespace Bark.Services.Layout;

public static partial class LayoutProvider
{
    private static string GetScripts(bool enableLiveReload, long buildVersion, string basePath, string? nonce = null) => $@"    <script{GetNonceAttr(nonce)}>
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
                    fetch('{basePath}/api/build-version')
                        .then(function(r) {{ return r.json(); }})
                        .then(function(data) {{
                            if (data.version !== currentBuildVersion) {{
                                location.reload();
                            }}
                        }})
                        ['catch'](function() {{}});
                }}, 5000);
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

            if (sidebarLeft) {{
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
            }}

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
                searchModalResults.innerHTML = '';
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
            var searchTriggerMobile = document.getElementById('search-trigger-mobile');
            if (searchTriggerMobile) {{
                searchTriggerMobile.addEventListener('click', openSearchModal);
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
                    searchModalResults.innerHTML = '';
                    searchModalInput.setAttribute('aria-expanded', 'false');
                    searchModalStatus.textContent = '';
                    return;
                }}
                searchModalResults.innerHTML = '<div class=""search-result-empty"" role=""status"">Searching&hellip;</div>';
                var requestId = searchRequestId;
                searchTimeout = setTimeout(function() {{
                    fetch('{basePath}/api/search?q=' + encodeURIComponent(query))
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
                                    html += '<a href=""{basePath}/' + r.path + '/"" class=""search-result-item"" role=""option"" id=""search-result-' + i + '"" aria-selected=""false"" tabindex=""-1"">' +
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

            // Wrap wide tables in scroll container; overflow-x:auto on table itself doesn't reliably contain it.
            var tables = document.querySelectorAll('.content table');
            tables.forEach(function(table) {{
                var wrapper = document.createElement('div');
                wrapper.className = 'table-wrapper';
                table.parentNode.insertBefore(wrapper, table);
                wrapper.appendChild(table);
            }});


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
                    var langBlock = pre.closest('[class^=""language-""]');
                    a.download = (langBlock && langBlock.dataset.filename) || 'code.txt';
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    URL.revokeObjectURL(url);
                }});
                buttons.appendChild(downloadBtn);

                wrapper.appendChild(buttons);
            }});

            var codeGroups = document.querySelectorAll('.bark-code-group');
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

            // Mermaid bakes colors into the SVG at render time and ignores CSS variables, so the current theme must be passed in explicitly; a full reload is needed to redraw..
            var mermaidBlocks = document.querySelectorAll('.mermaid');
            if (mermaidBlocks.length && window.mermaid) {{
                var currentTheme = document.documentElement.getAttribute('data-theme');
                var prefersDarkNow = window.matchMedia('(prefers-color-scheme: dark)').matches;
                var mermaidIsDark = currentTheme ? currentTheme === 'dark' : prefersDarkNow;
                window.mermaid.initialize({{ theme: mermaidIsDark ? 'dark' : 'default' }});
                window.mermaid.run();
            }}
        }});
    </script>
";
}
