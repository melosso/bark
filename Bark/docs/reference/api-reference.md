---
title: API Reference
description: HTTP routes Bark exposes
---

# API Reference

This document defines the HTTP routes Bark exposes. The surface area exposed is tiny.

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

## `GET /raw/{path}`

Returns the raw Markdown source for the given documentation page as a file download. This is what the [page controls](/reference/site-config#pagecontrolsconfig) "Download markdown" action links to.

```bash
curl -O http://localhost:5000/raw/getting-started/getting-started
```

The path follows the same normalization rules as the page route: case-insensitive, no trailing slash needed. If the path does not match a known page, the endpoint returns `404 Not Modified`.

This endpoint is rate-limited to 30 requests per minute per IP address, the same policy as `/api/search`.

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
| Keywords (frontmatter) | 4 |
| Heading | 3 |
| Body text | 1 |

The index is an in-memory inverted index, rebuilt in full (not incrementally) every time the docs rebuild.

This endpoint is rate-limited to 30 requests per minute per IP address. Requests over that threshold receive a `429 Too Many Requests` response.

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
