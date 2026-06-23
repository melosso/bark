---
title: API Reference
description: HTTP routes Bark exposes
---

# API Reference

Bark is a minimal-API app — three routes, no MVC, no controllers. That's the whole surface area.

## `GET /{path}`

Returns the rendered HTML page for the given documentation path.

**Example:** `GET /getting-started/installation`

What happens on each request:

1. Look up the pre-rendered page from the in-memory cache (built at startup, rebuilt on file change).
2. Compute a SHA-256 `ETag` of the HTML. If `If-None-Match` matches, respond `304 Not Modified`.
3. Build nav, breadcrumbs, table of contents, and prev/next pagination for the response.
4. Stitch it all into one HTML string and return it.

Unknown paths return a styled 404 page, not a bare status code.

### What I Learned:

> **The 80% Truth:** ETag support is the cheapest performance win nobody bothers to add to their own internal tools — until the docs site starts feeling sluggish under load.

## `GET /api/search`

| Query param | Required | Notes |
|---|---|---|
| `q` | yes | Search term. Queries under 2 characters return an empty array. |

```bash
curl "http://localhost:5000/api/search?q=hot+reload"
```

Returns a JSON array of search results, ranked by a weighted score:

| Match location | Weight |
|---|---|
| Title | 10 |
| Description | 5 |
| Heading | 3 |
| Body text | 1 |

The index is an in-memory inverted index, rebuilt in full (not incrementally) every time the docs rebuild — fine at the scale this tool is meant for, and simpler than maintaining incremental diffing logic for marginal gains.

## `GET /sitemap.xml`

Returns a standard XML sitemap covering every known page, `<lastmod>` populated from each file's last-write time on disk. Useful for search engine indexing if you're hosting public docs.

---

**P.S.** No GraphQL, no versioned `/v1/` prefix, no OpenAPI spec generator bolted on. Three routes is the entire backend surface — and that's the point. A docs server doesn't need an API platform; it needs to serve Markdown fast and stay legible enough that you can read the whole route table in thirty seconds.
