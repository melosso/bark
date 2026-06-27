using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Bark.Configuration;
using Bark.Models;

namespace Bark.Services;

public sealed partial class DocumentationService : IHostedService, IDisposable
{
    private readonly DocsOptions _options;
    private readonly MarkdownService _markdown;
    private readonly ILogger<DocumentationService> _logger;
    private FileSystemWatcher? _watcher;
    private FileSystemWatcher? _configWatcher;
    private readonly CancellationTokenSource _shutdownCts = new();

    private readonly ConcurrentDictionary<string, DocumentationPage> _pageCache = new();
    private readonly SearchIndex _searchIndex = new();
    private NavigationNode? _navigation;
    private BarkConfig? _docsConfig;
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

    public BarkConfig? Config => _docsConfig;
    public long BuildVersion { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await BuildAsync(cancellationToken);

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

            _ = FileWatcherConsumerAsync(_shutdownCts.Token);

            _logger.LogInformation("Hot reload enabled, watching {DocsPath}", docsPath);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdownCts.Cancel();
        _watcher?.Dispose();
        _configWatcher?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _shutdownCts.Cancel();
        _shutdownCts.Dispose();
        _watcher?.Dispose();
        _configWatcher?.Dispose();
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

                await RebuildAsync();
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

    private async Task RebuildAsync()
    {
        try
        {
            await _buildLock.WaitAsync();
            try
            {
                _pageCache.Clear();
                await BuildAsync(CancellationToken.None);
            }
            finally
            {
                _buildLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild documentation");
        }
    }

    private async Task BuildAsync(CancellationToken cancellationToken)
    {
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
                defaultTitle = !string.IsNullOrEmpty(dir) ? Path.GetFileName(dir) : "Home";
            }

            if (navTitlesByPath.TryGetValue(pagePath, out var navTitle))
                defaultTitle = navTitle;

            var parsed = _markdown.Parse(content, defaultTitle);

            var html = WrapTables(parsed.Html);
            var lastModified = File.GetLastWriteTimeUtc(file);

            var page = new DocumentationPage(
                Path: pagePath,
                Title: parsed.Title ?? defaultTitle,
                HtmlContent: html,
                Description: parsed.Description,
                LastModified: lastModified,
                Headings: parsed.Headings,
                Layout: parsed.Layout,
                ShowLastUpdated: parsed.ShowLastUpdated
            );

            _pageCache[pagePath] = page;
            pages.Add(page);
        }

        var configPath = Path.Combine(docsPath, "config.json");
        if (File.Exists(configPath))
            hashInput.Append(await File.ReadAllTextAsync(configPath, cancellationToken));

        var contentHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput.ToString())));

        // FileSystemWatcher fires spurious events on some filesystems; only bump BuildVersion
        // (which the client polls and reloads on) when content actually changed.
        if (contentHash == _lastContentHash)
        {
            _navigation = BuildNavigation(docsPath, pages);
            _searchIndex.Build(pages);
            _docsConfig = config;
            _logger.LogDebug("Rebuilt documentation but content is unchanged, skipping version bump");
            return;
        }

        _lastContentHash = contentHash;
        _navigation = BuildNavigation(docsPath, pages);
        _searchIndex.Build(pages);
        _docsConfig = config;
        BuildVersion++;
        _logger.LogInformation("Built documentation with {PageCount} pages", pages.Count);
    }

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

    public async Task<NavigationNode> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        await _buildLock.WaitAsync(cancellationToken);
        try
        {
            return _navigation ?? new NavigationNode("Root", null, Array.Empty<NavigationNode>());
        }
        finally
        {
            _buildLock.Release();
        }
    }

    public async Task<DocumentationPage?> GetPageAsync(string path, CancellationToken cancellationToken = default)
    {
        path = path.Trim('/').ToLowerInvariant();
        if (string.IsNullOrEmpty(path))
            path = _options.DefaultPage ?? "index";

        if (_pageCache.TryGetValue(path, out var page))
            return page;

        return null;
    }

    public async Task<IReadOnlyList<DocumentationPage>> GetAllPagesAsync(CancellationToken cancellationToken = default)
    {
        await _buildLock.WaitAsync(cancellationToken);
        try
        {
            return _pageCache.Values.ToImmutableList();
        }
        finally
        {
            _buildLock.Release();
        }
    }

    public IReadOnlyList<SearchResult> Search(string query)
    {
        return _searchIndex.Search(query);
    }

    public IReadOnlyList<BreadcrumbItem> GetBreadcrumbs(string path)
    {
        path = path.Trim('/').ToLowerInvariant();
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(s => !s.Equals("index", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var crumbs = new List<BreadcrumbItem> { new("Home", "/") };

        var accumulated = "";
        foreach (var segment in segments)
        {
            accumulated = string.IsNullOrEmpty(accumulated) ? segment : $"{accumulated}/{segment}";

            if (_pageCache.TryGetValue(accumulated, out var page))
            {
                crumbs.Add(new BreadcrumbItem(page.Title, $"/{accumulated}"));
            }
            else
            {
                var title = segment.Replace('-', ' ').Replace('_', ' ');
                if (title.Length > 0)
                    title = char.ToUpperInvariant(title[0]) + title[1..];
                crumbs.Add(new BreadcrumbItem(title, null));
            }
        }

        return crumbs;
    }

    private static Dictionary<string, string> BuildNavTitleLookup(BarkConfig? config)
    {
        var lookup = new Dictionary<string, string>();

        if (config?.Nav is { } nav)
            CollectNavTitles(nav, lookup);

        if (config?.Sidebar is { } sidebar)
            foreach (var entries in sidebar.Values)
                CollectNavTitles(entries, lookup);

        return lookup;
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

    private static BarkConfig? LoadConfig(string docsPath)
    {
        var configPath = Path.Combine(docsPath, "config.json");
        if (!File.Exists(configPath))
            return null;

        try
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<BarkConfig>(json, new JsonSerializerOptions
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
