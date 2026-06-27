---
title: Site Config
description: Full reference for appsettings.json and docs/config.json
---

# Site Config

This page lists every option Bark reads, grouped by the file it belongs to. If you are looking for a narrative walkthrough instead, see [Configuration](/getting-started/configuration).

## `appsettings.json`: `Docs`

These are host-level settings, applied per deployment. Any changes here require an app restart to take effect. If you are deploying with Docker, each of these can be supplied as an environment variable instead. See [Environment Variables](/getting-started/environment-variables).

| Option | Type | Default | Description |
|---|---|---|---|
| `RootPath` | `string` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `string` | `index` | Page served at `/`. |
| `EnableHotReload` | `bool` | `true` | Watch `*.md` and `config.json` for changes and rebuild in the background. Disable in production if you publish content as part of your deploy and don't want a `FileSystemWatcher` running. |
| `BasePath` | `string?` | `null` | Prefix every internal link, theme asset URL, and API call with this path segment. Use it when Bark isn't served from the domain root, for example a GitHub Pages project page at `you.github.io/your-repo/` or a reverse proxy mounting Bark under `/docs`. A CLI `--base-path` flag overrides this at runtime, which is how [static export](/getting-started/deploy#option-e-static-export-github-pages-etc) sets it without touching config. |

## Theming

The quickest approach is to drop `custom.css`, `custom.js`, or `theme.json` into `wwwroot/theme/` for filesystem-based theming. Please note that these files require an app restart to take effect, unlike your `docs/` content. For deployment-level control, you can set `Docs:Themes` in `appsettings.json` instead. When `Docs:Themes` is present, it takes full priority over `theme.json`. The two are not merged field by field.

See [Extending Themes](/getting-started/extending-themes).

## `appsettings.json`: `Docs:Themes`

All fields in this section are optional. Each one is either a CSS variable override or a theme toggle, and anything you leave unset falls back to Bark's default palette. The same fields, written in camelCase, are also accepted in `wwwroot/theme/theme.json`.

| Option | Type | Maps to | Description |
|---|---|---|---|
| `PrimaryColor` | `string` | `--primary-color` | Accent color used for links and highlights. |
| `BgColor` | `string` | `--bg-color` | Page background. |
| `SidebarBg` | `string` | `--sidebar-bg` | Sidebar background, can differ from the page background. |
| `TextColor` | `string` | `--text-color` | Primary text color. |
| `TextMuted` | `string` | `--text-muted` | Secondary text: descriptions, timestamps, muted labels. |
| `BorderColor` | `string` | `--border` | Hairline borders throughout the layout. |
| `CodeBg` | `string` | `--code-bg` | Background for inline code and fenced code blocks. |
| `AccentLight` | `string` | `--accent-light` | Light tint of the accent color, used for active/highlighted states. |
| `FontSans` | `string` | `--font-sans` | Body font stack. |
| `FontMono` | `string` | `--font-mono` | Code font stack. |
| `CustomCssUrl` | `string` | n/a | Injects an extra `<link rel="stylesheet">`, loaded after Bark's built-in styles so it can override them. Takes priority over an auto-detected `wwwroot/theme/custom.css` if both are present. |
| `BrandText` | `string` | n/a | Sidebar brand label. `config.json`'s `brand` takes priority if set; if `brand` is absent, `config.json`'s `title` is used instead. |
| `DarkMode` | `bool` | n/a | Toggles the `prefers-color-scheme: dark` variant and the in-page dark mode switch. Default `true`. |
| `ShowScrollIndicator` | `bool` | n/a | Toggles the thin scroll-progress bar pinned to the top of the viewport. Default `true`. |

## `docs/config.json`: site metadata

For a full walkthrough of the HTML head fields below, see [HTML Metadata](/reference/site-metadata).

| Option | Type | Description |
|---|---|---|
| `title` | `string?` | Site name. Appended to every page title as `Page Title \| Site Name`. See [HTML Metadata](/reference/site-metadata). |
| `titleTemplate` | `string?` | Custom title pattern. Use `:title` and `:siteName` as placeholders. For example, `":title · :siteName"` produces `Getting Started · Bark`. Overrides the default suffix format when set. |
| `description` | `string?` | Site-wide fallback `<meta name="description">`. Per-page frontmatter description takes priority. |
| `lang` | `string?` | `lang` attribute on `<html>`. Defaults to `"en"`. |
| `head` | `HeadTag[]?` | Extra tags injected into `<head>` on every page. Useful for Open Graph, canonical links, and structured data. |
| `brand` | `string?` | Sidebar/header brand label. Falls back to `title` if unset, then to `Docs:Themes:BrandText`. |
| `brandImage` | `string?` | An image URL or path to display alongside the brand text in the header. Placed to the left of the brand label. |
| `footer` | `string?` | Rendered as Markdown inside the page footer. Links and formatting work. See [Footer](default-theme-footer). |
| `favicon` | `string?` | A URL/path to an icon file, or a single emoji character to use as an inline SVG favicon. |
| `lastUpdated` | `bool` | Site-wide toggle for the "Last updated" stamp. Off by default. See [Last Updated Timestamp](default-theme-last-updated). |
| `editLink` | `EditLinkConfig?` | "Edit this page" link near the pagination footer. See [Edit Link](default-theme-edit-link). |

**`HeadTag`**

| Field | Type | Description |
|---|---|---|
| `tag` | `string` | HTML tag name, e.g. `"meta"`, `"link"`, `"script"`. |
| `attrs` | `Record<string, string>?` | Attribute key-value pairs. Values are HTML-encoded automatically. |
| `content` | `string?` | Inner HTML for non-void tags (`<script>`, `<style>`). Void elements (`<meta>`, `<link>`, `<base>`) ignore this field. |

## `docs/config.json`: navigation

These fields control your header bar, sidebars, and legacy flat navigation. You can use them together or independently depending on the shape of your docs.

| Option | Type | Description |
|---|---|---|
| `topNav` | `TopNavItem[]?` | Header nav bar. See [Nav](default-theme-nav). |
| `sidebar` | `Record<string, NavEntry[]>?` | Path-prefix-keyed sidebars. The longest matching prefix for the current page wins; an empty-prefix key (`"/"`) acts as a catch-all. Takes priority over `nav`. See [Sidebar](default-theme-sidebar). |
| `nav` | `NavEntry[]?` | Legacy single flat sidebar, shared by every page. Ignored for any page that matches a `sidebar` prefix. |

**`TopNavItem`**

| Field | Type | Description |
|---|---|---|
| `text` | `string` | Label shown in the nav bar. |
| `link` | `string?` | Direct link. Omit this to make the item a dropdown instead. |
| `items` | `TopNavItem[]?` | Dropdown children. Omit `link` when this is set. |

A `TopNavItem` is either a link (`text` + `link`) or a dropdown (`text` + `items`). Not both.

**`NavEntry`**

| Field | Type | Description |
|---|---|---|
| `title` | `string` | Link text or group heading. |
| `path` | `string?` | Leaf link target. Omit it to make this entry a group. |
| `items` | `NavEntry[]?` | Child entries. Set this (and omit `path`) to make this entry a group. Groups nest to any depth. |
| `collapsed` | `bool?` | Group-only. Omitted means not collapsible. `false` means collapsible, starts expanded. `true` means collapsible, starts collapsed. |

**`EditLinkConfig`**

| Field | Type | Description |
|---|---|---|
| `pattern` | `string` | URL template with a `:path` placeholder. |
| `text` | `string` | Link label. Default `"Edit this page"`. |

**Full example**, matching the structure used throughout this docs site:

```json
{
  "title": "Bark",
  "description": "A fast, lightweight Markdown documentation server built on .NET.",
  "lang": "en",
  "brand": "Bark",
  "brandImage": "/brand-image.svg",
  "footer": "Built with Bark · [AGPL-3.0](LICENSE)",
  "favicon": "🌳",
  "lastUpdated": true,
  "editLink": {
    "pattern": "https://github.com/melosso/bark/edit/main/docs/:path",
    "text": "Edit this page on GitHub"
  },
  "topNav": [
    { "text": "Home", "link": "/" },
    { "text": "Guide", "link": "/getting-started/getting-started" },
    { "text": "Reference", "link": "/reference/site-config" },
    {
      "text": "More",
      "items": [
        { "text": "GitHub", "link": "https://github.com/melosso/bark" },
        { "text": "Releases", "link": "https://github.com/melosso/bark/releases" }
      ]
    }
  ],
  "sidebar": {
    "/getting-started/": [
      {
        "title": "Introduction",
        "collapsed": false,
        "items": [
          { "title": "Getting Started", "path": "getting-started/getting-started" },
          { "title": "Configuration", "path": "getting-started/configuration" }
        ]
      }
    ],
    "/reference/": [
      {
        "title": "Reference",
        "items": [
          { "title": "Site Config", "path": "reference/site-config" },
          {
            "title": "Customisation",
            "items": [
              { "title": "Nav", "path": "reference/default-theme-nav" },
              { "title": "Sidebar", "path": "reference/default-theme-sidebar" }
            ]
          }
        ]
      }
    ]
  },
  "socialLinks": [
    { "icon": "github", "url": "https://github.com/melosso/bark", "title": "GitHub" },
    { "icon": "mastodon", "url": "https://fosstodon.org/@example", "title": "Mastodon" }
  ]
}
```

## `docs/config.json`: social links

Social links appear in the top-right area of the header and in the mobile sidebar. Each entry needs at minimum an icon name and a URL.

| Field | Type | Description |
|---|---|---|
| `icon` | `string` | `"github"` and `"mastodon"` render as inline SVGs. Anything else renders as plain text. |
| `url` | `string` | Link target. Opens in a new tab. |
| `title` | `string?` | Accessible label / tooltip. Falls back to `icon` if omitted. |

## Limitations

Bark is designed to be straightforward, so it avoids complex configuration overhead. You will not find a `markdown` options object, a way to pass through your own bundler, or an API for build hooks, simply because there is no client-side bundler to manage.

Essential Markdown features like custom containers, code groups, math support, and line highlighting are baked in and always active. If you would like to customize or extend these features beyond their defaults, you would need to modify the underlying source code, as they cannot be adjusted via configuration files.