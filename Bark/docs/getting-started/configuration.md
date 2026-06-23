---
title: Configuration
description: appsettings.json options, docs/bark.json, and theming
---

# Configuration

Bark splits configuration into two places, and the split is intentional, not accidental sprawl:

- **`appsettings.json`** — host-level concerns: where the docs folder lives, whether hot reload is on, theme colors. Things an ops person changes per deployment.
- **`docs/bark.json`** — content-level concerns: brand text, explicit navigation, footer, social links. Things a docs author changes per project. Hot-reloaded along with your Markdown — no restart needed.

### What I Realize:

Mixing infra config and content config in one file is how you end up with a `config.yml` nobody trusts to touch without asking the one person who understands it. Keeping them separate means a content editor never needs repo deploy permissions just to fix a typo in the brand name.

## `appsettings.json` — `Docs` section

| Setting | Default | Description |
|---|---|---|
| `RootPath` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `index` | Page served at `/`. |
| `EnableHotReload` | `true` | Watch `*.md` and `bark.json` for changes and rebuild in the background. |

```json
{
  "Docs": {
    "RootPath": "../../docs",
    "DefaultPage": "index",
    "EnableHotReload": true
  }
}
```

## `appsettings.json` — `Docs:Themes` section

Optional. Every field is a CSS variable override; anything left blank falls back to Bark's default palette.

| Setting | Maps to | Notes |
|---|---|---|
| `PrimaryColor` | `--primary-color` | |
| `BgColor` | `--bg-color` | |
| `SidebarBg` | `--sidebar-bg` | |
| `TextColor` | `--text-color` | |
| `TextMuted` | `--text-muted` | |
| `BorderColor` | `--border` | |
| `CodeBg` | `--code-bg` | |
| `AccentLight` | `--accent-light` | |
| `FontSans` | `--font-sans` | |
| `FontMono` | `--font-mono` | |
| `CustomCssUrl` | — | Injects an extra `<link rel="stylesheet">`, loaded after the built-in styles so it can override them. |
| `BrandText` | — | Overrides the sidebar brand label. `bark.json`'s `brand` takes priority over this if both are set. |
| `DarkMode` | — | `true`/`false`. Toggles the `prefers-color-scheme: dark` variant. Default `true`. |

```json
{
  "Docs": {
    "Themes": {
      "PrimaryColor": "#2e4a36",
      "DarkMode": true,
      "CustomCssUrl": "/css/custom.css"
    }
  }
}
```

## `docs/bark.json` — content config

```json
{
  "brand": "Bark",
  "footer": "Built with Bark · [AGPL-3.0](LICENSE)",
  "nav": [
    {
      "section": "Getting Started",
      "items": [
        { "title": "Installation", "path": "getting-started/installation" },
        { "title": "Configuration", "path": "getting-started/configuration" }
      ]
    },
    {
      "section": "API",
      "items": [
        { "title": "Reference", "path": "api/reference" }
      ]
    }
  ],
  "socialLinks": [
    { "icon": "github", "url": "https://github.com/hawkinslabdev/bark", "title": "GitHub" },
    { "icon": "mastodon", "url": "https://fosstodon.org/@example", "title": "Mastodon" }
  ]
}
```

**Important:** when `nav` is present, it *fully replaces* the auto-generated folder-based navigation — it does not merge with it. Leave `nav` out entirely if you want Bark to build the nav tree from your folder structure instead.

`footer` is rendered as Markdown, so links and formatting work. `socialLinks[].icon` of `"github"` or `"mastodon"` get inline SVGs; anything else renders as plain text.

> **The 80% Truth:** Config that requires a redeploy to fix a broken link is worse than no config at all.

Next: [API Reference](../api/reference) — the routes Bark exposes, including search and the sitemap.
