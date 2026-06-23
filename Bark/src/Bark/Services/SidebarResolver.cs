using Bark.Models;

namespace Bark.Services;

/// <summary>
/// Resolves which sidebar (vitepress's path-prefix-keyed <c>sidebar</c> config) applies to a
/// given page path: the longest matching prefix wins, with an empty-prefix entry (<c>"/"</c>)
/// acting as a catch-all.
/// </summary>
public static class SidebarResolver
{
    public static List<NavEntry>? Resolve(IReadOnlyDictionary<string, List<NavEntry>> sidebars, string currentPath)
    {
        var normalizedPath = currentPath.Trim('/').ToLowerInvariant();
        List<NavEntry>? best = null;
        var bestPrefixLength = -1;

        foreach (var (key, sections) in sidebars)
        {
            var prefix = key.Trim('/').ToLowerInvariant();
            var isMatch = prefix.Length == 0
                || normalizedPath == prefix
                || normalizedPath.StartsWith(prefix + "/", StringComparison.Ordinal);

            if (isMatch && prefix.Length > bestPrefixLength)
            {
                best = sections;
                bestPrefixLength = prefix.Length;
            }
        }

        return best;
    }
}
