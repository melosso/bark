---
title: Search
description: Bark's built-in search, no third-party service required
---

# Search

Search is built in. The search box in the sidebar talks to `/api/search`, backed by an in-memory (inverted) index that rebuilds automatically every time your configuration changes.

## How it ranks results

| Match location | Weight |
| --- | --- |
| Title | 10 |
| Description | 5 |
| Heading | 3 |
| Body text | 1 |

A query matching a page's title outranks one that only matches buried body text. That is the entire ranking model: weighted term matches.

## Calling it directly

```bash
curl "http://localhost:5000/api/search?q=hot+reload"
```

Queries under 2 characters return an empty array rather than the whole index. See [API Reference](api-reference) for the full response shape.

## What's not here

We've chosen to keep Bark lightweight. There is no "Ask AI" panel, nor any analytics dashboard. If you need full-text search across a documentation servicer larger than a few hundred pages, or you want fuzzy/typo-tolerant matching, Bark's in-memory index isn't built for that scale. With our customisation options you can adjust the search box's markup plain HTML, easy to swap out from source or with custom JS.
