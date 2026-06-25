---
title: API Reference
description: HTTP routes Bark exposes
---

# API Reference

Every HTTP route Bark exposes. The whole surface area fits on one page, read it in a minute.

## `GET /{path}`

Returns the rendered HTML page for the given documentation path.

```bash
curl http://localhost:5000/getting-started/getting-started/
```

What happens on each request:

1. Look up the pre-rendered page from the in-memory cache (built at startup, rebuilt on file change).
2. Compute a SHA-256 `ETag` of the HTML. If the request's `If-None-Match` matches, respond `304 Not Modified` instead of re-sending the page.
3. Build navigation, breadcrumbs, table of contents, and prev/next pagination for the response.
4. Stitch it all into one HTML string and return it.

Unknown paths return a 404 page rather than an exception.

## `GET /api/search`

Returns a JSON array of search results, ranked by a weighted score.

```bash
curl "http://localhost:5000/api/search?q=hot+reload"
```

| Query param | Required | Notes |
|---|---|---|
| `q` | yes | Search term. Queries under 2 characters return an empty array. |

| Match location | Weight |
|---|---|
| Title | 10 |
| Description | 5 |
| Heading | 3 |
| Body text | 1 |

The index is an in-memory inverted index, rebuilt in full (not incrementally) every time the docs rebuild.

## `GET /api/build-version`

Returns an integer that increments every time the docs content actually changes, not on every filesystem event (see [Getting Started](/getting-started/getting-started) for why that distinction matters).

```json
{ "version": 4 }
```

The dev-mode hot-reload script polls this endpoint and reloads the browser when it sees a new value. You probably won't call this directly, but it's there if you want to build your own "content changed" hook.

## `GET /sitemap.xml`

Returns a standard XML sitemap covering every known page, `<lastmod>` populated from each file's last-write time on disk.

## `GET /robots.txt`

Returns a `robots.txt` with a `Sitemap:` line built from the actual request host, correct behind a reverse proxy as long as forwarded headers are configured.

```
User-agent: *
Allow: /
Sitemap: https://your-host/sitemap.xml
```

See [Deploy](/getting-started/deploy) for the forwarded-headers setup.

## `GET /llms.txt`

Returns a plain-text index of every page (title, URL, and description), formatted for LLM crawlers and agentic tools that prefer a flat, low-noise summary over crawling rendered HTML.

See [Sitemap & Crawlers](/reference/sitemap-generation) for how these three endpoints fit together.
