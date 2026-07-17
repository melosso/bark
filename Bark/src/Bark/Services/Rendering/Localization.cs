using System.Text;
using System.Text.Json;
using Bark.Models;

namespace Bark.Services.Rendering;

/// <summary>Server-side UI string table. English defaults are the floor; docs/locale/{code}.json
/// overrides them per key. Swapped atomically on content reload. Never served to clients.</summary>
public sealed class Localization
{
    private static readonly JsonSerializerOptions LocaleJsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false
    };

    private static readonly Dictionary<string, string> Defaults = new(StringComparer.Ordinal)
    {
        ["skipToContent"] = "Skip to content",
        ["menuToggle"] = "Toggle navigation menu",
        ["themeToggle"] = "Toggle dark mode",
        ["onThisPage"] = "On this page",
        ["breadcrumbHome"] = "Home",
        ["paginationPrevious"] = "Previous",
        ["paginationNext"] = "Next",
        ["copyCode"] = "Copy code",
        ["pageOptions"] = "Page options",
        ["copyPage"] = "Copy page",
        ["viewMarkdown"] = "View as Markdown",
        ["copyRssUrl"] = "Copy RSS feed URL",
        ["searchAria"] = "Search documentation",
        ["searchPlaceholder"] = "Search documentation...",
        ["searchClose"] = "Close search",
        ["searchResultsAria"] = "Search results",
        ["searchNavigate"] = "Navigate",
        ["searchSelect"] = "Select",
        ["searchEsc"] = "Close",
        ["searchSearching"] = "Searching",
        ["searchNoResults"] = "No results found.",
        ["searchResultSingular"] = "result found.",
        ["searchResultPlural"] = "results found.",
        ["searchError"] = "Something went wrong. Try again.",
        ["searchFailed"] = "Search failed.",
        ["notFoundTitle"] = "Page Not Found",
        ["notFoundMessage"] = "The page you're looking for doesn't exist.",
        ["notFoundHome"] = "Return home",
    };

    private readonly IReadOnlyDictionary<string, string> _map;

    private Localization(IReadOnlyDictionary<string, string> map) => _map = map;

    public static Localization Default { get; } = new(Defaults);

    private static volatile Localization _current = Default;

    public static Localization Current
    {
        get => _current;
        set => _current = value;
    }

    private string this[string key] =>
        _map.TryGetValue(key, out var v) ? v
        : Defaults.TryGetValue(key, out var d) ? d
        : key;

    public string SkipToContent => this["skipToContent"];
    public string MenuToggle => this["menuToggle"];
    public string ThemeToggle => this["themeToggle"];
    public string OnThisPage => this["onThisPage"];
    public string BreadcrumbHome => this["breadcrumbHome"];
    public string PaginationPrevious => this["paginationPrevious"];
    public string PaginationNext => this["paginationNext"];
    public string CopyCode => this["copyCode"];
    public string PageOptions => this["pageOptions"];
    public string CopyPage => this["copyPage"];
    public string ViewMarkdown => this["viewMarkdown"];
    public string CopyRssUrl => this["copyRssUrl"];
    public string SearchAria => this["searchAria"];
    public string SearchPlaceholder => this["searchPlaceholder"];
    public string SearchClose => this["searchClose"];
    public string SearchResultsAria => this["searchResultsAria"];
    public string SearchNavigate => this["searchNavigate"];
    public string SearchSelect => this["searchSelect"];
    public string SearchEsc => this["searchEsc"];
    public string SearchSearching => this["searchSearching"];
    public string SearchNoResults => this["searchNoResults"];
    public string SearchResultSingular => this["searchResultSingular"];
    public string SearchResultPlural => this["searchResultPlural"];
    public string SearchError => this["searchError"];
    public string SearchFailed => this["searchFailed"];
    public string NotFoundTitle => this["notFoundTitle"];
    public string NotFoundMessage => this["notFoundMessage"];
    public string NotFoundHome => this["notFoundHome"];

    // Overlays docs/locale/{code}.json on the defaults. Missing file: silent. Corrupt/unknown keys: warn.
    public static Localization From(string docsPath, Config? config, ILogger logger)
    {
        var code = ResolveCode(config);
        var path = Path.Combine(docsPath, "locale", $"{code}.json");
        if (!File.Exists(path))
            return Default;

        var filename = Path.GetFileName(path);

        Dictionary<string, string?>? raw;
        try
        {
            var json = File.ReadAllText(path);
            raw = JsonSerializer.Deserialize<Dictionary<string, string?>>(json, LocaleJsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning("Locale file {Filename} is invalid. Falling back to default strings. Reason: {Message}", filename, ex.Message);
            return Default;
        }

        if (raw is null || raw.Count == 0)
            return Default;

        var map = new Dictionary<string, string>(Defaults, StringComparer.Ordinal);
        var deadKeys = new List<string>();
        foreach (var (key, value) in raw)
        {
            if (!Defaults.ContainsKey(key))
            {
                deadKeys.Add(key);
                continue;
            }
            if (!string.IsNullOrEmpty(value))
                map[key] = value;
        }

        if (deadKeys.Count > 0)
            logger.LogWarning("Locale file {Filename} has unknown keys (no such string, ignored): {Keys}",
                filename, string.Join(", ", deadKeys.Order()));

        return new Localization(map);
    }

    private static string ResolveCode(Config? config)
    {
        var raw = config?.Locale?.Code ?? config?.Lang ?? "en";
        return IsValidCode(raw) ? raw.ToLowerInvariant() : "en";
    }

    // Guard the filename: locale codes are short tokens, never paths.
    private static bool IsValidCode(string s)
    {
        if (s.Length is < 2 or > 12) return false;
        foreach (var c in s)
            if (!char.IsAsciiLetterOrDigit(c) && c != '-') return false;
        return true;
    }

    // Escape for inlining inside a quoted JS string literal.
    public static string JsEncode(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var sb = new StringBuilder(value.Length + 8);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '\'': sb.Append("\\'"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '<': sb.Append("\\u003C"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
}
