using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Bark.Models;

namespace Bark.Services;

public sealed partial class SearchIndex
{
    private readonly ConcurrentDictionary<string, List<(string Path, int Score)>> _invertedIndex = new();
    private readonly ConcurrentDictionary<string, DocumentationPage> _pages = new();
    private volatile bool _isBuilt;

    public void Build(IEnumerable<DocumentationPage> pages)
    {
        _invertedIndex.Clear();
        _pages.Clear();

        foreach (var page in pages)
        {
            _pages[page.Path] = page;
            IndexPage(page);
        }

        _isBuilt = true;
    }

    private void IndexPage(DocumentationPage page)
    {
        var titleTerms = Tokenize(page.Title);
        var descTerms = page.Description is { Length: > 0 } ? Tokenize(page.Description) : [];
        var headingTerms = page.Headings.SelectMany(h => Tokenize(h.Text));
        var bodyTerms = Tokenize(GetPlainText(page.HtmlContent));

        var allTerms = new HashSet<string>(titleTerms, StringComparer.OrdinalIgnoreCase);
        allTerms.UnionWith(descTerms);
        allTerms.UnionWith(headingTerms);
        allTerms.UnionWith(bodyTerms);

        foreach (var term in allTerms)
        {
            var score = 0;
            if (titleTerms.Contains(term, StringComparer.OrdinalIgnoreCase))
                score += 10;
            if (descTerms.Contains(term, StringComparer.OrdinalIgnoreCase))
                score += 5;
            if (headingTerms.Any(t => string.Equals(t, term, StringComparison.OrdinalIgnoreCase)))
                score += 3;
            if (bodyTerms.Contains(term, StringComparer.OrdinalIgnoreCase))
                score += 1;

            _invertedIndex.AddOrUpdate(
                term.ToLowerInvariant(),
                _ => [(page.Path, score)],
                (_, list) =>
                {
                    var newList = new List<(string Path, int Score)>(list) { (page.Path, score) };
                    return newList;
                });
        }
    }

    public IReadOnlyList<SearchResult> Search(string query)
    {
        if (!_isBuilt || string.IsNullOrWhiteSpace(query))
            return Array.Empty<SearchResult>();

        var terms = Tokenize(query);
        if (terms.Count == 0)
            return Array.Empty<SearchResult>();

        var scores = new Dictionary<string, (int Score, string? Excerpt)>(StringComparer.OrdinalIgnoreCase);

        foreach (var term in terms)
        {
            if (!_invertedIndex.TryGetValue(term.ToLowerInvariant(), out var matches))
                continue;

            foreach (var (path, termScore) in matches)
            {
                if (!_pages.TryGetValue(path, out var page))
                    continue;

                var excerpt = GetExcerpt(page.HtmlContent, term);
                if (scores.TryGetValue(path, out var existing))
                    scores[path] = (existing.Score + termScore, existing.Excerpt ?? excerpt);
                else
                    scores[path] = (termScore, excerpt);
            }
        }

        return scores
            .OrderByDescending(kv => kv.Value.Score)
            .ThenBy(kv => kv.Key)
            .Select(kv =>
            {
                var page = _pages[kv.Key];
                return new SearchResult(
                    Path: page.Path,
                    Title: page.Title,
                    Description: page.Description,
                    Excerpt: kv.Value.Excerpt);
            })
            .ToList();
    }

    private static string? GetExcerpt(string html, string term)
    {
        // Decode entities first -- callers HTML-encode for display, double-escaping otherwise.
        var plainText = System.Net.WebUtility.HtmlDecode(HtmlTagRegex().Replace(html, " "));
        var index = plainText.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return null;

        var start = Math.Max(0, index - 60);
        var length = Math.Min(plainText.Length - start, 160);
        var excerpt = plainText.AsSpan(start, length).Trim().ToString();
        if (start > 0) excerpt = "..." + excerpt;
        if (start + length < plainText.Length) excerpt += "...";
        return excerpt;
    }

    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var words = WordSplitRegex().Split(text);
        var result = new List<string>(words.Length);
        foreach (var word in words)
        {
            var trimmed = word.Trim();
            if (trimmed.Length > 0)
                result.Add(trimmed.ToLowerInvariant());
        }
        return result;
    }

    private static string GetPlainText(string html) =>
        HtmlTagRegex().Replace(html, " ");

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\W+")]
    private static partial Regex WordSplitRegex();
}
