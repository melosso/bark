---
title: Search
description: Bark's built-in search, no third-party service required
---

# Search

Search is built in. There's no Algolia account to create, no API key to manage, no separate indexing build step. The search box in the sidebar talks to `/api/search`, backed by an in-memory inverted index that rebuilds automatically every time your docs change.

## How it ranks results

| Match location | Weight |
|---|---|
| Title | 10 |
| Description | 5 |
| Heading | 3 |
| Body text | 1 |

A query matching a page's title outranks one that only matches buried body text. That's the entire ranking model: weighted term matches, no fuzzy matching, no semantic search, no external dependency.

## Calling it directly

```bash
curl "http://localhost:5000/api/search?q=hot+reload"
```

Queries under 2 characters return an empty array rather than the whole index. See [API Reference](api-reference) for the full response shape.

## What's not here

No search-result keyboard shortcut (`Cmd+K`), no "Ask AI" panel, no search analytics dashboard. If you need full-text search across a docs set larger than a few hundred pages, or you want fuzzy/typo-tolerant matching, Bark's in-memory index isn't built for that scale. Bring your own Algolia/Meilisearch integration at that point: the search box's markup is plain HTML, easy to swap out.
