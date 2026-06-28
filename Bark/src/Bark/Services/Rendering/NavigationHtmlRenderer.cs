using System.Text;
using Bark.Models;
using Bark.Services.Layout;

namespace Bark.Services.Rendering;

public static class NavigationHtmlRenderer
{
    public static string BuildNavigationHtml(NavigationNode node, string currentPath, Config? config, string basePath)
    {
        if (config?.Sidebar is { Count: > 0 } sidebars)
        {
            var matchedSections = SidebarResolver.Resolve(sidebars, currentPath);
            if (matchedSections is not null)
                return BuildNavFromConfig(matchedSections, currentPath, basePath);
        }

        if (config?.Nav is { Count: > 0 } sections)
            return BuildNavFromConfig(sections, currentPath, basePath);

        if (node.Children.Count == 0) return string.Empty;

        var html = new StringBuilder();
        html.AppendLine("<div class=\"nav-groups\">");

        foreach (var child in node.Children)
        {
            if (child.Children.Count > 0)
            {
                html.AppendLine("<div class=\"nav-group\">");
                var displayTitle = ToDisplayName(child.Title);
                html.AppendLine($"<div class=\"nav-group-title\">{LayoutProvider.HtmlEncode(displayTitle)}</div>");
                html.AppendLine("<ul class=\"nav-list\">");
                foreach (var sub in child.Children.OrderBy(c => c.Title))
                {
                    var isActive = sub.Path == currentPath;
                    html.AppendLine($"<li class=\"nav-item{(isActive ? " active" : "")}\">");
                    html.AppendLine($"<a href=\"{UrlPaths.Href(basePath, sub.Path ?? "")}\">{LayoutProvider.HtmlEncode(sub.Title)}</a>");
                    html.AppendLine("</li>");
                }
                html.AppendLine("</ul>");
                html.AppendLine("</div>");
            }
        }

        html.AppendLine("</div>");
        return html.ToString();
    }

    public static string BuildNavFromConfig(List<NavEntry> entries, string currentPath, string basePath)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class=\"sidebar-tree\">");
        foreach (var entry in entries)
            AppendSidebarEntry(html, entry, currentPath, level: 0, basePath);
        html.AppendLine("</div>");
        return html.ToString();
    }

    public static bool SidebarPathMatches(string entryPath, string currentPath)
    {
        var normalized = entryPath.Trim('/').ToLowerInvariant();
        return normalized == currentPath || (normalized.Length == 0 && currentPath == "index");
    }

    public static bool ContainsActiveDescendant(NavEntry entry, string currentPath)
    {
        if (entry.Path is not null && SidebarPathMatches(entry.Path, currentPath))
            return true;

        return entry.Items?.Any(child => ContainsActiveDescendant(child, currentPath)) ?? false;
    }

    public static void AppendSidebarEntry(StringBuilder html, NavEntry entry, string currentPath, int level, string basePath)
    {
        if (entry.Items is not { Count: > 0 } children)
        {
            var isActive = SidebarPathMatches(entry.Path ?? string.Empty, currentPath);
            var href = UrlPaths.Href(basePath, entry.Path ?? string.Empty);
            html.AppendLine(
                $"<div class=\"sidebar-link level-{level}{(isActive ? " is-active" : "")}\">" +
                $"<a href=\"{href}\">{LayoutProvider.HtmlEncode(entry.Title)}</a></div>");
            return;
        }

        var hasActiveDescendant = ContainsActiveDescendant(entry, currentPath);
        var isCollapsible = entry.Collapsed.HasValue;
        var startsOpen = !isCollapsible || entry.Collapsed == false || hasActiveDescendant;
        var headerClass = $"sidebar-group-title level-{level}{(hasActiveDescendant ? " has-active" : "")}";
        var headingTag = level == 0 ? "h2" : "h3";

        const string caretSvg = "<span class=\"caret-icon\"><svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
            "stroke-width=\"2\" aria-hidden=\"true\"><path d=\"M9 6l6 6-6 6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg></span>";

        // .sidebar-group-title stays a plain <div>; UA-default <summary> styling can't be fully overridden, so <summary> only wraps it as a near-invisible click target.
        if (isCollapsible)
            html.AppendLine($"<details class=\"sidebar-group\"{(startsOpen ? " open" : "")}>")
                .AppendLine("<summary class=\"sidebar-group-summary\">")
                .AppendLine($"<div class=\"{headerClass}\"><{headingTag}>{LayoutProvider.HtmlEncode(entry.Title)}</{headingTag}>{caretSvg}</div>")
                .AppendLine("</summary>");
        else
            html.AppendLine("<div class=\"sidebar-group no-caret\">")
                .AppendLine($"<div class=\"{headerClass}\"><{headingTag}>{LayoutProvider.HtmlEncode(entry.Title)}</{headingTag}></div>");

        html.AppendLine("<div class=\"sidebar-group-items\">");
        foreach (var child in children)
            AppendSidebarEntry(html, child, currentPath, level + 1, basePath);
        html.AppendLine("</div>");
        html.AppendLine(isCollapsible ? "</details>" : "</div>");
    }

    public static string BuildTopNavHtml(List<TopNavItem>? topNav, string currentPath, string basePath)
    {
        if (topNav is null || topNav.Count == 0)
            return string.Empty;

        var html = new StringBuilder();
        html.AppendLine("<nav class=\"top-nav\" aria-label=\"Main navigation\">");
        foreach (var item in topNav)
            AppendTopNavItem(html, item, currentPath, isMobile: false, basePath);
        html.AppendLine("</nav>");
        return html.ToString();
    }

    public static string BuildMobileTopNavHtml(List<TopNavItem>? topNav, string currentPath, string basePath)
    {
        if (topNav is null || topNav.Count == 0)
            return string.Empty;

        var html = new StringBuilder();
        html.AppendLine("<nav class=\"mobile-top-nav\" aria-label=\"Main navigation\">");
        foreach (var item in topNav)
            AppendTopNavItem(html, item, currentPath, isMobile: true, basePath);
        html.AppendLine("</nav>");
        return html.ToString();
    }

    public static void AppendTopNavItem(StringBuilder html, TopNavItem item, string currentPath, bool isMobile, string basePath)
    {
        if (item.Items is { Count: > 0 } children)
        {
            if (isMobile)
            {
                html.AppendLine("<details class=\"mobile-top-nav-group\">");
                html.AppendLine($"<summary>{LayoutProvider.HtmlEncode(item.Text)}</summary>");
                foreach (var child in children)
                    AppendTopNavLink(html, child, currentPath, "mobile-top-nav-link", basePath);
                html.AppendLine("</details>");
            }
            else
            {
                html.AppendLine("<div class=\"top-nav-item has-dropdown\">");
                html.AppendLine($"<button type=\"button\" class=\"top-nav-link\" aria-expanded=\"false\" aria-haspopup=\"true\">{LayoutProvider.HtmlEncode(item.Text)} " +
                    "<svg class=\"top-nav-chevron\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" aria-hidden=\"true\"><path d=\"M6 9l6 6 6-6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg></button>");
                html.AppendLine("<div class=\"top-nav-dropdown-menu\">");
                foreach (var child in children)
                    AppendTopNavLink(html, child, currentPath, "top-nav-dropdown-link", basePath);
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }
            return;
        }

        AppendTopNavLink(html, item, currentPath, isMobile ? "mobile-top-nav-link" : "top-nav-link", basePath, wrapInItemDiv: !isMobile);
    }

    public static void AppendTopNavLink(StringBuilder html, TopNavItem item, string currentPath, string cssClass, string basePath, bool wrapInItemDiv = false)
    {
        var link = item.Link ?? "#";
        var isExternal = link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                          link.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        var normalizedLink = isExternal ? link : UrlPaths.Href(basePath, link);
        var isActive = !isExternal &&
            link.Trim('/').Equals(currentPath.Trim('/'), StringComparison.OrdinalIgnoreCase);
        var activeClass = isActive ? " active" : "";
        var relAttr = isExternal ? " target=\"_blank\" rel=\"noopener noreferrer\"" : "";

        const string externalIcon = "<svg class=\"external-link-icon\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
            "stroke-width=\"2\" aria-hidden=\"true\"><path d=\"M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>" +
            "<path d=\"M15 3h6v6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/><path d=\"M10 14 21 3\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>";

        if (wrapInItemDiv)
            html.AppendLine("<div class=\"top-nav-item\">");

        html.AppendLine(
            $"<a href=\"{LayoutProvider.HtmlEncode(normalizedLink)}\" class=\"{cssClass}{activeClass}\"{relAttr}>" +
            $"{LayoutProvider.HtmlEncode(item.Text)}{(isExternal ? externalIcon : "")}</a>");

        if (wrapInItemDiv)
            html.AppendLine("</div>");
    }

    public static string ToDisplayName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var display = name.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(display[0]) + display[1..];
    }

    // Prev/next must walk pages in the same order as whatever sidebar is actually showing for this
    // page, not always the auto-generated folder tree -- same precedence as BuildNavigationHtml.
    public static List<string?> GetOrderedPaths(NavigationNode node, Config? config, string currentPath)
    {
        if (config?.Sidebar is { Count: > 0 } sidebars)
        {
            var matchedSections = SidebarResolver.Resolve(sidebars, currentPath);
            if (matchedSections is not null)
                return FlattenNavEntries(matchedSections);
        }

        if (config?.Nav is { Count: > 0 } sections)
            return FlattenNavEntries(sections);

        return FlattenNavigation(node);
    }

    public static List<string?> FlattenNavigation(NavigationNode node)
    {
        var list = new List<string?>();
        foreach (var child in node.Children)
        {
            if (child.Path != null)
                list.Add(child.Path);
            if (child.Children.Count > 0)
                list.AddRange(FlattenNavigation(child));
        }
        return list;
    }

    public static List<string?> FlattenNavEntries(List<NavEntry> entries)
    {
        var list = new List<string?>();
        foreach (var entry in entries)
        {
            if (entry.Path != null)
                list.Add(entry.Path.Trim('/'));
            if (entry.Items is { Count: > 0 } children)
                list.AddRange(FlattenNavEntries(children));
        }
        return list;
    }
}
