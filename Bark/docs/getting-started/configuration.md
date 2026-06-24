---
title: Configuration
description: appsettings.json options, docs/config.json, and theming
---

# Configuration

Bark splits configuration into two places by design:

- **`appsettings.json`**: host-level concerns, like where the docs folder lives, whether hot reload is on, theme colors. Set per deployment.
- **`docs/config.json`**: content-level concerns, like brand text, navigation, footer, social links. Set per project, and hot-reloaded along with your Markdown. No restart needed.

Keeping infra config and content config apart means a content editor never needs deploy permissions just to fix a typo in the brand name. The full field-by-field reference for both files lives in [Site Config](../reference/site-config). This page is the narrative version, with the parts you'll actually touch first.

## `appsettings.json`: `Docs` section

| Setting | Default | Description |
|---|---|---|
| `RootPath` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `index` | Page served at `/`. |
| `EnableHotReload` | `true` | Watch `*.md` and `config.json` for changes and rebuild in the background. |

```json
{
  "Docs": {
    "RootPath": "../../docs",
    "DefaultPage": "index",
    "EnableHotReload": true
  }
}
```

Want to change colors, fonts, or ship your own CSS/JS? That's a separate concern from the settings above, covered in [Customization](customization).

## `docs/config.json`: navigation

You get three levels of control over navigation, and you can mix them:

1. **Do nothing.** No `nav`, `sidebar`, or `topNav` in `config.json`: Bark builds the left sidebar from your folder structure automatically.
2. **One flat sidebar** (`nav`) for the whole site. Good for small doc sets that don't need a header bar.
3. **A header nav with dropdowns** (`topNav`) plus **a different sidebar per section** (`sidebar`, keyed by path prefix). This is the setup most multi-section docs sites want.

```json
{
  "brand": "Bark",
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
        "section": "Introduction",
        "items": [
          { "title": "Getting Started", "path": "getting-started/getting-started" },
          { "title": "Configuration", "path": "getting-started/configuration" },
          { "title": "Routing", "path": "getting-started/routing" },
          { "title": "Deploy", "path": "getting-started/deploy" }
        ]
      }
    ],
    "/reference/": [
      {
        "section": "Reference",
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

A `topNav` item is either a direct link (`text` + `link`) or a dropdown (`text` + `items`, no `link`), exactly the two shapes shown above. `sidebar` keys are path prefixes: whichever key is the **longest match** for the page you're viewing wins, so `/getting-started/` and `/getting-started/advanced/` can coexist, with the more specific one taking over for pages under it.

> [!TIP]
> When `sidebar` is present, it takes priority over `nav` for any page matching one of its prefixes. `nav`, when present at all, fully replaces the auto-generated folder-based navigation for every page. Neither merges with the folder tree. Leave both out if you want Bark to build navigation from your folders.

`footer` is rendered as Markdown, so links and formatting work. `socialLinks[].icon` of `"github"` or `"mastodon"` get inline SVGs; anything else renders as plain text.