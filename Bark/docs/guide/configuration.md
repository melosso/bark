---
title: Configuration
description: appsettings.json options, docs/config.json, and theming
---

# Configuration

Bark splits configuration into two files:

- **`appsettings.json`**: host-level concerns. Where the docs folder lives, whether hot reload is on, theme colors. Set per deployment, and applied on restart.
- **`docs/config.json`**: content-level concerns. Site title and metadata, brand text, navigation, footer, social links. Set per project and hot-reloaded alongside your Markdown, so no restart is needed.

That split means a content editor never needs deploy access just to fix a typo in the brand name.

This page covers what you will touch first. For the field-by-field list, see [Site Config](/reference/site-config). If you run Bark in a container, [Environment Variables](/guide/environment-variables) gives the equivalent variable names.

## `appsettings.json`

These settings belong to the `Docs` section:

| Setting | Default | Description |
|---|---|---|
| `RootPath` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `index` | Page served at `/`. |
| `EnableHotReload` | `true` | Watch `*.md` and `config.json` for changes and rebuild in the background. |
| `BasePath` | `null` | Prefix for every internal link and asset URL. Set this when Bark is served from a subpath instead of the domain root. |

```json
{
  "Docs": {
    "RootPath": "../../docs",
    "DefaultPage": "index",
    "EnableHotReload": true,
    "BasePath": "/your-repo"
  }
}
```

::: tip
`BasePath` matters most for [static export](/guide/deploy#option-e-static-export-github-pages-etc), where the `--base-path` CLI flag usually replaces it entirely. Set it here instead when the live server sits behind a reverse proxy that mounts Bark under a subpath.
:::

Colors, fonts, and your own CSS or JS are a separate concern, covered in [Themes](/guide/themes).

## `docs/config.json`

This file covers your site's title, HTML metadata, navigation, footer, and social links. Navigation has the most moving parts, so it gets the space below. For everything else, see [Site Config](/reference/site-config).

Navigation has three levels of control, and they mix:

1. **Do nothing.** With no `nav`, `sidebar`, or `topNav`, Bark builds the left sidebar from your folder structure.
2. **One flat sidebar** (`nav`) for the whole site. Good for small doc sets that need no header bar.
3. **A header nav with dropdowns** (`topNav`) plus **a different sidebar per section** (`sidebar`, keyed by path prefix). This is what most multi-section sites want.

```json
{
  "brand": "Bark",
  "topNav": [
    { "text": "Home", "link": "/" },
    { "text": "Guide", "link": "/guide/getting-started" },
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
    "/guide/": [
      {
        "title": "Introduction",
        "items": [
          { "title": "Getting Started", "path": "guide/getting-started" },
          { "title": "Configuration", "path": "guide/configuration" },
          { "title": "Routing", "path": "guide/routing" },
          { "title": "Deploy", "path": "guide/deploy" }
        ]
      }
    ],
    "/reference/": [
      {
        "title": "Reference",
        "items": [
          { "title": "Site Config", "path": "reference/site-config" },
          { "title": "API Reference", "path": "reference/api-reference" },
          { "title": "Sitemap & Crawlers", "path": "reference/sitemap-generation" }
        ]
      }
    ]
  },
  "socialLinks": [
    { "icon": "github", "url": "https://github.com/melosso/bark", "title": "GitHub" }
  ]
}
```

A `topNav` item is either a direct link (`text` plus `link`) or a dropdown (`text` plus `items`, no `link`). Those are the only two shapes.

`sidebar` keys are path prefixes, and the longest match for the current page wins. That lets `/guide/` and `/guide/advanced/` coexist, with the more specific key taking over for pages beneath it.

::: tip
When `sidebar` is present it takes priority over `nav` for any page matching one of its prefixes. `nav`, when present at all, fully replaces the auto-generated folder navigation on every page. Neither one merges with the folder tree. Leave both out to let Bark build navigation from your folders.
:::

`footer` is rendered as Markdown, so links and formatting work as expected. For social links, an `icon` of `"github"` or `"mastodon"` renders as an inline SVG; any other value renders as plain text.
