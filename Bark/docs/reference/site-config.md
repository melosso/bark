---
title: Site Config
description: Full reference for appsettings.json and docs/config.json
---

# Site Config

Every option Bark reads, grouped by file. If you want the narrative walkthrough instead, see [Configuration](/getting-started/configuration).

## `appsettings.json`: `Docs`

Host-level. Set per deployment, requires a restart to change.

| Option | Type | Default | Description |
|---|---|---|---|
| `RootPath` | `string` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `string` | `index` | Page served at `/`. |
| `EnableHotReload` | `bool` | `true` | Watch `*.md` and `config.json` for changes and rebuild in the background. Disable in production if you publish content as part of your deploy and don't want a `FileSystemWatcher` running. |
| `BasePath` | `string?` | `null` | Prefix every internal link, theme asset URL, and API call with this path segment. Use it when Bark isn't served from the domain root, for example a GitHub Pages project page at `you.github.io/your-repo/` or a reverse proxy mounting Bark under `/docs`. A CLI `--base-path` flag overrides this at runtime, which is how [static export](/getting-started/deploy#option-e-static-export-github-pages-etc) sets it without touching config. |

## Theming

Short version: drop `custom.css` / `custom.js` / `theme.json` into `wwwroot/theme/` for filesystem-only theming, no restart-free hot reload (these need an app restart, unlike `docs/`), or set `Docs:Themes` below for deployment-level config. If `Docs:Themes` is present in `appsettings.json` at all, it wins outright over `theme.json`, the two aren't merged field-by-field.

See [Extending Themes](/getting-started/extending-themes).

## `appsettings.json`: `Docs:Themes`

Optional. Every field is a CSS variable override or theme toggle. Anything left unset falls back to Bark's default palette. The same fields, in camelCase, work in `wwwroot/theme/theme.json`.

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
| `BrandText` | `string` | n/a | Sidebar brand label. `config.json`'s `brand` takes priority if both are set. |
| `DarkMode` | `bool` | n/a | Toggles the `prefers-color-scheme: dark` variant and the in-page dark mode switch. Default `true`. |
| `ShowScrollIndicator` | `bool` | n/a | Toggles the thin scroll-progress bar pinned to the top of the viewport. Default `true`. |

## `docs/config.json`: site metadata

| Option | Type | Description |
|---|---|---|
| `brand` | `string?` | Sidebar/header brand label. Overrides `Docs:Themes:BrandText`. |
| `footer` | `string?` | Rendered as Markdown inside the page footer. Links and formatting work. See [Footer](default-theme-footer). |
| `favicon` | `string?` | A URL/path to an icon file, or a single emoji character to use as an inline SVG favicon. |
| `lastUpdated` | `bool` | Site-wide toggle for the "Last updated" stamp. Off by default. See [Last Updated Timestamp](default-theme-last-updated). |
| `editLink` | `EditLinkConfig?` | "Edit this page" link near the pagination footer. See [Edit Link](default-theme-edit-link). |

## `docs/config.json`: navigation

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
  "brand": "Bark",
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

| Field | Type | Description |
|---|---|---|
| `icon` | `string` | `"github"` and `"mastodon"` render as inline SVGs. Anything else renders as plain text. |
| `url` | `string` | Link target. Opens in a new tab. |
| `title` | `string?` | Accessible label / tooltip. Falls back to `icon` if omitted. |

## Limitations

Bark is designed to be straightforward, so it avoids complex configuration overhead. You will not find a `markdown` options object, a way to pass through your own bundler, or an API for build hooks, simply because there is no client-side bundler to manage.

Essential Markdown features like custom containers, code groups, math support, and line highlighting are baked in and always active. If you need to customize or extend these features beyond their defaults, you will need to modify the underlying source code, as these cannot be adjusted via configuration files.