---
title: Sitemap & Crawlers
description: How Bark generates sitemap.xml, robots.txt, and llms.txt
---

# Sitemap & Crawlers

Three endpoints, all generated from the same in-memory page list, all rebuilt automatically when your docs change. You don't run a separate command to produce any of them.

## `sitemap.xml`

```bash
curl http://localhost:5000/sitemap.xml
```

```xml
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url><loc>/</loc><priority>1.0</priority></url>
  <url><loc>/getting-started/getting-started</loc><lastmod>2026-06-20</lastmod><priority>0.8</priority></url>
  ...
</urlset>
```

The home page gets priority `1.0`; everything else gets `0.8`. `<lastmod>` comes straight from each Markdown file's last-write timestamp on disk: no Git history lookup, no separate metadata to keep in sync. Edit the file, the sitemap reflects it on the next rebuild.

## `robots.txt`

```
User-agent: *
Allow: /
Sitemap: https://your-host/sitemap.xml
```

Generated per-request, not a static file, so the `Sitemap:` line always points at whatever host actually served the request: `http://localhost:5000` in dev, your real domain in production, without you maintaining two versions.

**TIP.** Behind a reverse proxy, this only resolves correctly if forwarded headers are wired up so ASP.NET Core sees the real scheme and host instead of the proxy's internal address. See [Deploy](../getting-started/deploy).

## `llms.txt`

```
# Bark

- [Getting Started](https://your-host/getting-started/getting-started): Get Bark running locally in under a minute
- [Configuration](https://your-host/getting-started/configuration): appsettings.json options, docs/config.json, and theming
...
```

A flat list of every page (title, URL, description) aimed at LLM-based tools and agentic crawlers that would rather read a clean text index than parse rendered HTML and guess at what's navigation chrome versus actual content. This is an emerging convention, not a formal spec, but it costs nothing to support and Bark exposes it for free.

None of these three need configuration. If a page is in your `docs/` folder, it's in all three.
