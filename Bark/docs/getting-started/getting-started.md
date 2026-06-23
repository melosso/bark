---
title: Getting Started
description: Get Bark running locally in under a minute
---

# Getting Started

In a nutshell, Bark takes a folder of Markdown and turns it into a searchable, navigable docs site: no build step, no static-site generator, no JavaScript framework to wire up. Just want to see it running? Keep reading, you're two commands away.

Bark is a single ASP.NET Core process. Point it at a folder of Markdown, and it renders pages on the fly. Navigation, search index, table of contents, and breadcrumbs rebuild automatically the moment a file changes on disk. You edit, you save, you see it.

## What you get

- **Markdown rendering**: CommonMark + GitHub-flavored extras (tables, task lists, footnotes, alert blocks, emoji), plus VitePress-style custom containers, code groups, line highlighting, and math.
- **Hot reload**: edit a `.md` file or `config.json`, see it live. No restart.
- **Search**: built-in in-memory inverted index, exposed at `/api/search?q=`.
- **Navigation, breadcrumbs, TOC, prev/next pagination**: generated automatically from your folder structure and headings.
- **Theming**: CSS variable overrides and dark mode toggle, configured via `appsettings.json`.
- **Production-minded defaults**: response compression, Kestrel limits, structured logging via Serilog, ETag-based caching, plus `sitemap.xml`/`robots.txt`/`llms.txt`.

Bark stays narrow on purpose: render Markdown fast, reload instantly, stay out of your way.

## Run it

Docker is the fastest path. Create a `docker-compose.yml`:

```yaml
services:
  bark:
    image: ghcr.io/hawkinslabdev/bark:latest
    container_name: bark
    ports:
      - "8080:8080"
    volumes:
      - ./docs:/app/docs
```

Mount your own `docs/` folder (your `.md` files plus an optional `config.json`), then run:

```bash
docker compose up -d
```

Open `http://localhost:8080`. That's the whole onboarding flow.

Not on Docker? See [Deploy](deploy) for the Windows/IIS and Linux release-zip paths, plus building Bark from source yourself if you'd rather not pull a container image.

> [!NOTE] 
> Hot reload works the same regardless of how you run Bark. It watches `docs/**/*.md` and `docs/config.json` with its own `FileSystemWatcher` and rebuilds in the background, debounced ~300ms so a flurry of editor saves doesn't trigger a rebuild storm. The browser refreshes itself once the rebuild lands. On an external or network-mounted drive, filesystem change notifications can occasionally fire without real content changes. Bark hashes the rebuilt content and only reloads the browser when something actually changed, so this doesn't show up as a problem in practice.

## Where docs live

Markdown files go in `docs/` at the repo root by default (configurable, see [Site Config](../reference/site-config)). The folder tree becomes the navigation tree unless you override it in `config.json`. `index.md` in any folder becomes that folder's landing page.

```
docs/
├── config.json                      ← optional, see Configuration
├── index.md                         ← served at /
├── getting-started/
│   ├── getting-started.md           ← served at /getting-started/getting-started
│   ├── configuration.md
│   ├── routing.md
│   └── deploy.md
└── reference/
    ├── site-config.md
    ├── cli.md
    ├── api-reference.md
    └── sitemap-generation.md
```

Front matter (`title`, `description`) is optional. Bark falls back to the filename, or to a nav-configured title if you've set one in `config.json`, when it's missing:

```markdown
---
title: Getting Started
description: Get Bark running locally
---

# Getting Started
...
```

