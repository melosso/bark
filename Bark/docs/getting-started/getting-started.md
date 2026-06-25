---
title: Getting Started
description: Run Bark locally in under a minute
---

# Getting Started

Bark is a documentation server: point it at a folder of Markdown and it serves a full site, navigation, search, and all. This guide gets a copy running locally.

Want to know why it's built this way before diving in? Read [What is Bark?](/what-is-bark). Just want it running? Keep reading.

## Prerequisites

Docker is the fastest path and the one this guide uses. No Docker? [Deploy](../deploy) covers Windows/IIS, a Linux release zip, and building from source instead.

## Installation

Create a `docker-compose.yml`:

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

The `./docs` volume is your content: Markdown files plus an optional `config.json`. Bark reads everything from there.

## Up and running

```bash
docker compose up -d
```

Open `http://localhost:8080`. That's the whole setup.

## File structure

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

Your folder layout becomes your site's navigation and URLs automatically. The exact rules for turning a file path into a URL live in [Routing](./routing); every front matter field a page can set lives in [Frontmatter Config](../reference/frontmatter-config).

## What's next

- [Configuration](./configuration): `config.json` options, themes, and branding.
- [Using Markdown](./markdown): every Markdown extension Bark supports, with live examples.
- [Routing](./routing): how file paths map to URLs.
- [Deploy](./deploy): Docker, IIS, Linux, or building from source, plus the production defaults Bark ships with.
