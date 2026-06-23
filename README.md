# Bark

[![License](https://img.shields.io/badge/license-AGPL%203.0-blue)](LICENSE)
[![Last commit](https://img.shields.io/github/last-commit/hawkinslabdev/bark)](https://github.com/hawkinslabdev/bark/commits/main)

**Bark** is a fast, lightweight **Markdown documentation server** built on ASP.NET Core. Drop `.md` files in a folder, get a searchable, navigable docs site with hot reload, no build step, no static site generator.

It renders pages on the fly, watches your `docs/` folder for changes, and rebuilds navigation, search index, and table of contents automatically. One process, one config file, no client-side JavaScript framework required.

---

## Prerequisites

Before deploying Bark, make sure your environment meets the following requirements.

* [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
* If you're running on Windows: Internet Information Services (IIS), if hosting behind it

## Getting Started

### 1. Run it

```bash
cd Bark
dotnet restore
dotnet watch --project src/Bark
```

Browse to `http://localhost:5000`.

### 2. Write docs

Drop Markdown files into `docs/`. Folder structure becomes the navigation tree; `index.md` becomes a section's landing page.

```markdown
---
title: Installation
description: Get Bark running locally
---

# Installation

Run `dotnet watch --project src/Bark` and open the browser.
```

Front matter (`title`, `description`) is optional — Bark falls back to the filename.

### 3. Configure (optional)

Drop a `docs/bark.json` to set brand text, a footer, explicit navigation, and social links:

```json
{
  "brand": "Bark",
  "footer": "Built with Bark · [AGPL-3.0](LICENSE)",
  "nav": [
    {
      "section": "Getting Started",
      "items": [
        { "title": "Installation", "path": "getting-started/installation" }
      ]
    }
  ],
  "socialLinks": [
    { "icon": "github", "url": "https://github.com/example/bark" }
  ]
}
```

When `nav` is present it replaces the auto-generated folder-based navigation. Both content and config are hot-reloaded — no restart needed.

## Features

* **Markdown rendering** — CommonMark + GFM extras (tables, task lists, footnotes, alert blocks, emoji) via Markdig
* **Hot reload** — edit a `.md` file or `bark.json`, see it live
* **Search** — built-in in-memory search index, exposed at `/api/search?q=`
* **Navigation, breadcrumbs, TOC, prev/next pagination** — generated automatically from your folder structure and headings
* **Theming** — CSS variable overrides and dark mode, configurable via `appsettings.json`
* **Production-minded defaults** — response compression, Kestrel limits, structured logging via Serilog, ETag-based caching

## License

Free for open source projects and personal use under the **AGPL 3.0** license. See [LICENSE](LICENSE) for details.

## Contribution

Contributions are welcome. Please submit a PR if you'd like to help improve Bark.
