---
title: Getting Started
description: Run Bark locally in under a minute
---

# Getting Started

Bark makes it easy to turn your Markdown folders into a fully searchable documentation site. You can get started right away without needing to manage complex configurations or build processes.

Simply follow this guide to get your site up and running. Once configured, the application automatically updates your navigation, table of contents, and search index every time you save your files.

## Key Features

- **Markdown support**: CommonMark, GitHub-flavored features (tables, task lists, alerts), and advanced additions like code groups, line highlighting, and math.
- **Hot reload**: Edit your configuration and see your changes instantly
- **Built-in Search**: Includes an in-memory index and exposed REST API for integration
- **Production-ready**: Includes response compression, Kestrel limits, Serilog logging, ETag-based caching

More features like full customisation and automatic `sitemap.xml`/`robots.txt`/`llms.txt` generation are noteworthy too.

## Run it

Docker is the fastest way to get started. Create a `docker-compose.yml` file:

```yaml
services:
  bark:
    image: ghcr.io/melosso/bark:latest
    container_name: bark
    ports:
      - "8080:8080"
    volumes:
      - ./docs:/app/docs
```

Map your `docs/` folder (containing your Markdown files and optional `config.json`), then run:

```bash
docker compose up -d
```

Your site will be available at `http://localhost:8080`.

Not using Docker? See [Deploy](../deploy) for different configurations (e.g. IIS on Windows Server).

## File Structure

By default the server looks for Markdown files in the `docs/` folder. The folder structure determines your site navigation, which can be overridden in `config.json`. An `index.md` file serves as the landing page for any folder.

```
docs/
├── config.json                      ← Configuration file (optional)
├── index.md                         ← Homepage
├── getting-started/
│   ├── getting-started.md           ← Served at /getting-started/getting-started
│   ├── configuration.md
│   ├── routing.md
│   └── deploy.md
└── reference/
    ├── site-config.md
    ├── api-reference.md
    └── sitemap-generation.md
```

Front matter, which is the block of metadata at the very top of your Markdown file, is optional. If missing, Bark uses the filename or the navigation title defined in `config.json`:

```markdown
---
title: Getting Started
description: Get Bark running locally
---

# Getting Started
...
```