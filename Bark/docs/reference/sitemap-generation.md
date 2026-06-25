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

Because this is generated dynamically for every request rather than being served as a static file, the `Sitemap:` line always points to the specific host that handled the request. Whether you are working locally at `http://localhost:5000` or browsing your live site in production, it points to the correct location automatically, so you never have to manage multiple versions of the file.

> [!NOTE] 
> Behind a reverse proxy, this only resolves correctly if forwarded headers are wired up so ASP.NET Core sees the real scheme and host instead of the proxy's internal address. See [Deploy](/getting-started/deploy).

## `llms.txt`

```
# Bark

- [Getting Started](https://your-host/getting-started/getting-started): Get Bark running locally in under a minute
- [Configuration](https://your-host/getting-started/configuration): appsettings.json options, docs/config.json, and theming
...
```

This file lists every page with its title, URL, and a description. It helps AI agents read your site. These tools often struggle to ignore your site's navigation and layout when parsing HTML, so this file gives them a clean index instead. 

None of these three files require any setup. If you put a page in your `docs/` folder, Bark includes it in all of them.