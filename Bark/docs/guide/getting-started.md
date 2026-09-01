---
title: Getting Started
description: Run Bark locally in under a minute
---

# Getting Started

Bark is a documentation server: point it at a folder of Markdown and it serves a full site, navigation, search, and all. This guide gets a copy running locally.

Want to know why it is built this way first? Read [What is Bark?](/guide/what-is-bark). Otherwise, keep going.

## Installation

Docker is the fastest path and the one this guide uses. Without it, [Deploy](/guide/deploy) covers Windows/IIS, a Linux release zip, and building from source.

Create a `docker-compose.yml`:

```yaml
services:
  bark:
    image: ghcr.io/melosso/bark:latest
    container_name: bark
    ports:
      - "8080:8080"
    volumes:
      - ./docs:/app/docs:ro,Z
```

The `./docs` volume is your content: Markdown files plus an optional `config.json`. Bark reads everything from there.

```bash
docker compose up -d
```

Open `http://localhost:8080`. That is the whole setup.

## File structure

```
docs/
├── config.json                      ← Configuration file (optional)
├── index.md                         ← Homepage
├── guide/
│   ├── getting-started.md           ← Served at /guide/getting-started
│   ├── configuration.md
│   ├── routing.md
│   └── deploy.md
└── reference/
    ├── site-config.md
    ├── api-reference.md
    └── sitemap-generation.md
```

Your folder layout becomes the site's navigation and URLs. No index to maintain.

## What's next

- [Configuration](/guide/configuration): `config.json` options, themes, and branding.
- [Routing](/guide/routing): the exact rules for turning a file path into a URL.
- [Using Markdown](/guide/markdown): every Markdown extension Bark supports, with live examples.
- [Frontmatter](/reference/frontmatter-config): every field a page can set.
- [Deploy](/guide/deploy): Docker, IIS, Linux, or source, plus the production defaults Bark comes with.
