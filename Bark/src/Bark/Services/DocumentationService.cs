using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Bark.Configuration;
using Bark.Models;
using Bark.Services.Extensions;
using Bark.Services.Rendering;

namespace Bark.Services;

public sealed partial class DocumentationService : IHostedService, IDisposable, IExtensionSource
{
    private readonly DocsOptions _options;
    private readonly MarkdownService _markdown;
    private readonly ILogger<DocumentationService> _logger;
    private FileSystemWatcher? _watcher;
    private FileSystemWatcher? _configWatcher;
    private FileSystemWatcher? _extensionsWatcher;
    private FileSystemWatcher? _localeWatcher;
    private readonly CancellationTokenSource _shutdownCts = new();

    // All read state lives in one immutable snapshot swapped atomically after a full build; readers never see half-built state
    private sealed record ContentSnapshot(
        IReadOnlyDictionary<string, DocumentationPage> Pages,
        NavigationNode Navigation,
        IReadOnlyDictionary<string, string> NavTitles,
        Config? Config,
        SearchIndex SearchIndex,
        ExtensionSet Extensions);

    private static readonly ContentSnapshot EmptySnapshot = new(
        ImmutableDictionary<string, DocumentationPage>.Empty,
        new NavigationNode("Root", null, Array.Empty<NavigationNode>()),
        ImmutableDictionary<string, string>.Empty,
        null,
        new SearchIndex(),
        ExtensionSet.Empty);

    private volatile ContentSnapshot _snapshot = EmptySnapshot;
    private string? _lastContentHash;
    private readonly SemaphoreSlim _buildLock = new(1, 1);
    private readonly Channel<FileSystemEventArgs> _fileChannel =
        Channel.CreateBounded<FileSystemEventArgs>(new BoundedChannelOptions(256)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    private bool _disposed;

    public DocumentationService(
        DocsOptions options,
        MarkdownService markdown,
        ILogger<DocumentationService> logger)
    {
        _options = options;
        _markdown = markdown;
        _logger = logger;
    }

    public Config? SiteConfig => _snapshot.Config;
    public ExtensionSet Extensions => _snapshot.Extensions;
    public long BuildVersion { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await RebuildAsync(cancellationToken);

        if (_options.EnableHotReload)
        {
            var docsPath = Path.GetFullPath(_options.RootPath);
            if (!Directory.Exists(docsPath))
                Directory.CreateDirectory(docsPath);

            _watcher = new FileSystemWatcher(docsPath)
            {
                IncludeSubdirectories = true,
                Filter = "*.md",
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;

            _configWatcher = new FileSystemWatcher(docsPath)
            {
                Filter = "config.json",
                EnableRaisingEvents = true
            };
            _configWatcher.Changed += OnFileChanged;
            _configWatcher.Created += OnFileChanged;
            _configWatcher.Deleted += OnFileChanged;
            _configWatcher.Renamed += OnFileRenamed;

            _extensionsWatcher = new FileSystemWatcher(docsPath)
            {
                Filter = ExtensionLoader.FileName,
                EnableRaisingEvents = true
            };
            _extensionsWatcher.Changed += OnFileChanged;
            _extensionsWatcher.Created += OnFileChanged;
            _extensionsWatcher.Deleted += OnFileChanged;
            _extensionsWatcher.Renamed += OnFileRenamed;

            // The main watcher filters *.md, so locale JSON needs its own watcher.
            var localeDir = Path.Combine(docsPath, "locale");
            if (Directory.Exists(localeDir))
            {
                _localeWatcher = new FileSystemWatcher(localeDir)
                {
                    Filter = "*.json",
                    EnableRaisingEvents = true
                };
                _localeWatcher.Changed += OnFileChanged;
                _localeWatcher.Created += OnFileChanged;
                _localeWatcher.Deleted += OnFileChanged;
                _localeWatcher.Renamed += OnFileRenamed;
            }

            _ = FileWatcherConsumerAsync(_shutdownCts.Token);

            _logger.LogInformation("Hot reload enabled, watching {DocsPath}", docsPath);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdownCts.Cancel();
        _watcher?.Dispose();
        _configWatcher?.Dispose();
        _extensionsWatcher?.Dispose();
        _localeWatcher?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _shutdownCts.Cancel();
        _shutdownCts.Dispose();
        _watcher?.Dispose();
        _configWatcher?.Dispose();
        _extensionsWatcher?.Dispose();
        _localeWatcher?.Dispose();
        _buildLock.Dispose();
        _disposed = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _fileChannel.Writer.TryWrite(e);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _fileChannel.Writer.TryWrite(e);
    }

    private async Task FileWatcherConsumerAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var __ in _fileChannel.Reader.ReadAllAsync(ct))
            {
                await Task.Delay(300, ct);

                while (_fileChannel.Reader.TryRead(out _)) { }

                try
                {
                    await RebuildAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rebuild documentation");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File watcher consumer failed");
        }
    }

    private async Task RebuildAsync(CancellationToken cancellationToken)
    {
        await _buildLock.WaitAsync(cancellationToken);
        try
        {
            await BuildAsync(cancellationToken);
        }
        finally
        {
            _buildLock.Release();
        }
    }

    // Caller must hold _buildLock; builds a complete snapshot off to the side, then swaps it in
    private async Task BuildAsync(CancellationToken cancellationToken)
    {
        IconProvider.ClearCache();
        var docsPath = Path.GetFullPath(_options.RootPath);
        if (!Directory.Exists(docsPath))
        {
            _logger.LogWarning("Docs directory does not exist: {Path}", docsPath);
            return;
        }

        // Loaded up front so title fallback can consult config.json before the filename.
        var config = LoadConfig(docsPath);
        var navTitlesByPath = BuildNavTitleLookup(config);

        // Sorted for deterministic hashing, regardless of FS enumeration order.
        var allFiles = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories).Order().ToArray();
        var pages = new List<DocumentationPage>();
        var pageMap = new Dictionary<string, DocumentationPage>();
        var hashInput = new StringBuilder();

        foreach (var file in allFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(docsPath, file);
            var pagePath = PagePath.FromFile(relativePath);

            var content = await File.ReadAllTextAsync(file, cancellationToken);
            hashInput.Append(relativePath).Append('\0').Append(content).Append('\0');

            var defaultTitle = Path.GetFileNameWithoutExtension(relativePath);
            if (defaultTitle.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                var dir = Path.GetDirectoryName(relativePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    var dirName = Path.GetFileName(dir)!;
                    var spaced = dirName.Replace('-', ' ').Replace('_', ' ');
                    defaultTitle = spaced.Length > 0 ? char.ToUpperInvariant(spaced[0]) + spaced[1..] : dirName;
                }
                else
                {
                    defaultTitle = "Home";
                }
            }

            if (navTitlesByPath.TryGetValue(pagePath, out var navTitle))
                defaultTitle = navTitle;

            var normalizedRelativePath = relativePath.Replace('\\', '/');
            var parsed = _markdown.Parse(content, defaultTitle, filePath: normalizedRelativePath);

            var html = WrapTables(parsed.Html);
            var lastModified = parsed.FrontmatterDate ?? File.GetLastWriteTimeUtc(file);

            var page = new DocumentationPage(
                Path: pagePath,
                Title: parsed.Title ?? defaultTitle,
                HtmlContent: html,
                Description: parsed.Description,
                LastModified: lastModified,
                Headings: parsed.Headings,
                Layout: parsed.Layout,
                ShowLastUpdated: parsed.ShowLastUpdated,
                OriginalRelativePath: normalizedRelativePath,
                Keywords: parsed.Keywords,
                ShowPagination: parsed.ShowPagination,
                Redirect: parsed.Redirect,
                ShowToc: parsed.ShowToc,
                Image: parsed.Image
            );

            pageMap[pagePath] = page;
            pages.Add(page);
        }

        var configPath = Path.Combine(docsPath, "config.json");
        if (File.Exists(configPath))
            hashInput.Append(await File.ReadAllTextAsync(configPath, cancellationToken));

        var extensions = ExtensionLoader.Load(docsPath, _logger);
        var extensionsPath = Path.Combine(docsPath, ExtensionLoader.FileName);
        if (File.Exists(extensionsPath))
            hashInput.Append(await File.ReadAllTextAsync(extensionsPath, cancellationToken));

        // Fold locale files into the hash so an edit bumps BuildVersion and drives live reload.
        var localeDir = Path.Combine(docsPath, "locale");
        if (Directory.Exists(localeDir))
            foreach (var f in Directory.GetFiles(localeDir, "*.json").Order())
                hashInput.Append(Path.GetFileName(f)).Append('\0')
                         .Append(await File.ReadAllTextAsync(f, cancellationToken)).Append('\0');

        var contentHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput.ToString())));

        var searchIndex = new SearchIndex();
        searchIndex.Build(pages);

        var snapshot = new ContentSnapshot(
            pageMap,
            BuildNavigation(docsPath, pages),
            navTitlesByPath,
            config,
            searchIndex,
            extensions);

        _snapshot = snapshot;

        // Prevent unnecessary client reloads from spurious file events by verifying content changes!
        if (contentHash == _lastContentHash)
        {
            _logger.LogDebug("Rebuilt documentation but content is unchanged, skipping version bump");
            return;
        }

        _lastContentHash = contentHash;
        BuildVersion++;
        _logger.LogInformation("Built documentation with {PageCount} pages", pages.Count);

        LogDeadLinks(pages, pageMap);
    }

    private void LogDeadLinks(List<DocumentationPage> pages, Dictionary<string, DocumentationPage> pageMap)
    {
        var deadSources = new HashSet<string>();
        foreach (var page in pages)
        {
            foreach (Match match in HrefRegex().Matches(page.HtmlContent))
            {
                var href = match.Groups[1].Value;
                if (ShouldSkipHref(href))
                    continue;

                var resolved = ResolveHref(page.Path, href);
                if (resolved.Length == 0 || pageMap.ContainsKey(resolved))
                    continue;

                deadSources.Add(page.Path);
            }
        }

        if (deadSources.Count > 0)
        {
            var list = string.Join(", ", deadSources.Order());
            _logger.LogWarning("Dead internal links found in: {Sources}", list);
        }
    }

    private static string ResolveHref(string pagePath, string href)
    {
        var fragIdx = href.IndexOf('#');
        var pathOnly = fragIdx >= 0 ? href[..fragIdx] : href;

        if (pathOnly.StartsWith('/'))
            return pathOnly.Trim('/').ToLowerInvariant();

        var basePath = pagePath == "index" ? "" : pagePath;
        var combined = $"{basePath}/{pathOnly}";
        var segments = new List<string>();
        foreach (var seg in combined.Split('/'))
        {
            if (seg == "..")
            {
                if (segments.Count > 0)
                    segments.RemoveAt(segments.Count - 1);
            }
            else if (seg != "." && seg != "")
                segments.Add(seg);
        }
        return string.Join("/", segments).ToLowerInvariant();
    }

    private static bool ShouldSkipHref(string href)
    {
        return href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || href.StartsWith("//")
            || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || href.StartsWith("#")
            || href == "/";
    }

    [GeneratedRegex(@"<a\s[^>]*href=""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HrefRegex();

    private NavigationNode BuildNavigation(string docsPath, IEnumerable<DocumentationPage> pages)
    {
        var pageMap = pages.ToDictionary(p => p.Path);
        return BuildNodeFromDirectory(docsPath, docsPath, pageMap);
    }

    private NavigationNode BuildNodeFromDirectory(string basePath, string currentDir, Dictionary<string, DocumentationPage> pageMap)
    {
        var relativePath = Path.GetRelativePath(basePath, currentDir).Replace('\\', '/');
        var title = Path.GetFileName(currentDir);
        if (relativePath == ".")
            title = "Home";

        var children = new List<NavigationNode>();

        foreach (var subDir in Directory.GetDirectories(currentDir))
        {
            var node = BuildNodeFromDirectory(basePath, subDir, pageMap);
            if (node.Children.Count > 0 || pageMap.Values.Any(p =>
                p.Path.StartsWith(Path.GetRelativePath(basePath, subDir).Replace('\\', '/').ToLowerInvariant())))
            {
                children.Add(node);
            }
        }

        foreach (var file in Directory.GetFiles(currentDir, "*.md"))
        {
            var pagePath = PagePath.FromFile(Path.GetRelativePath(basePath, file));

            if (pageMap.TryGetValue(pagePath, out var page))
            {
                children.Add(new NavigationNode(page.Title, page.Path, Array.Empty<NavigationNode>()));
            }
        }

        children = children.OrderBy(c => c.Path == null ? 0 : 1)
                           .ThenBy(c => c.Title)
                           .ToList();

        return new NavigationNode(title, null, children);
    }

    public Task<NavigationNode> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_snapshot.Navigation);
    }

    public ValueTask<DocumentationPage?> GetPageAsync(string path, CancellationToken cancellationToken = default)
    {
        path = path.Trim('/').ToLowerInvariant();
        if (string.IsNullOrEmpty(path))
            path = _options.DefaultPage ?? "index";

        _snapshot.Pages.TryGetValue(path, out var page);
        return ValueTask.FromResult(page);
    }

    public Task<IReadOnlyList<DocumentationPage>> GetAllPagesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DocumentationPage> pages = _snapshot.Pages.Values.ToImmutableList();
        return Task.FromResult(pages);
    }

    public IReadOnlyList<SearchResult> Search(string query)
    {
        return _snapshot.SearchIndex.Search(query);
    }

    public SearchIndexExport GetSearchIndexExport()
    {
        return _snapshot.SearchIndex.ExportSnapshot();
    }

    public Task<IReadOnlyList<BreadcrumbItem>> GetBreadcrumbsAsync(string path, CancellationToken cancellationToken = default)
    {
        path = path.Trim('/').ToLowerInvariant();
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(s => !s.Equals("index", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var crumbs = new List<BreadcrumbItem> { new("Home", "/") };

        var snapshot = _snapshot;
        var accumulated = "";
        foreach (var segment in segments)
        {
            accumulated = string.IsNullOrEmpty(accumulated) ? segment : $"{accumulated}/{segment}";

            if (snapshot.Pages.TryGetValue(accumulated, out var page))
                crumbs.Add(new BreadcrumbItem(page.Title, $"/{accumulated}"));
            else if (snapshot.NavTitles.TryGetValue(accumulated, out var navTitle))
                crumbs.Add(new BreadcrumbItem(navTitle, null));
            else
            {
                var title = segment.Replace('-', ' ').Replace('_', ' ');
                if (title.Length > 0)
                    title = char.ToUpperInvariant(title[0]) + title[1..];
                crumbs.Add(new BreadcrumbItem(title, null));
            }
        }

        return Task.FromResult<IReadOnlyList<BreadcrumbItem>>(crumbs);
    }

    private static Dictionary<string, string> BuildNavTitleLookup(Config? config)
    {
        var lookup = new Dictionary<string, string>();

        if (config?.Nav is { } nav)
            CollectNavTitles(nav, lookup);

        if (config?.Sidebar is { } sidebar)
            foreach (var entries in sidebar.Values)
                CollectNavTitles(entries, lookup);

        if (config?.TopNav is { } topNav)
            foreach (var item in topNav)
                CollectTopNavTitles(item, lookup);

        return lookup;
    }

    private static void CollectTopNavTitles(TopNavItem item, Dictionary<string, string> lookup)
    {
        if (!string.IsNullOrEmpty(item.Link) && !string.IsNullOrEmpty(item.Text))
            lookup[item.Link.Trim('/').ToLowerInvariant()] = item.Text;

        if (item.Items is { Count: > 0 } children)
            foreach (var child in children)
                CollectTopNavTitles(child, lookup);
    }

    private static void CollectNavTitles(List<NavEntry> entries, Dictionary<string, string> lookup)
    {
        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.Path))
                lookup[entry.Path.Trim('/').ToLowerInvariant()] = entry.Title;

            if (entry.Items is { Count: > 0 } children)
                CollectNavTitles(children, lookup);
        }
    }

    [GeneratedRegex(@"<table[^>]*>[\s\S]*?</table>", RegexOptions.IgnoreCase)]
    private static partial Regex TableRegex();

    private static string WrapTables(string html) =>
        TableRegex().Replace(html, m => $"<div class=\"table-wrapper\">{m.Value}</div>");

    private static Config? LoadConfig(string docsPath)
    {
        var configPath = Path.Combine(docsPath, "config.json");
        if (!File.Exists(configPath))
            return null;

        try
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}
