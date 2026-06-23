---
title: Routing
description: How a file path becomes a URL in Bark
---

# Routing

There's no router config to write. Bark has exactly one route, a catch-all (`GET /{**path}`), and derives every page's URL straight from its file path on disk. You name files, Bark handles the rest.

## The rules

| File | URL |
|---|---|
| `docs/index.md` | `/` |
| `docs/getting-started/getting-started.md` | `/getting-started/getting-started` |
| `docs/getting-started/index.md` | `/getting-started` |
| `docs/Reference/API-Reference.md` | `/reference/api-reference` |

Plainly:

1. Drop the `.md` extension.
2. A file named `index.md` becomes its **parent folder's** URL, not a literal `/index` segment.
3. Everything is lowercased and slashes are trimmed. `/Getting-Started/Routing/` and `getting-started/routing` resolve to the same page.

This logic lives in one place, `PagePath.FromFile()`, so it's consistent everywhere Bark needs a URL from a file: the page cache, the sitemap, search results, breadcrumbs.

## What happens on a request

1. The incoming path gets trimmed and lowercased the same way file paths are, so `GET /Getting-Started/Routing/` and `GET /getting-started/routing` hit the same cache entry.
2. An empty path (`/`) resolves to whatever `Docs:DefaultPage` is configured as (`index` by default).
3. No match in the page cache → a 404 page, not an exception.

> [!NOTE]  
> There's no way to give a page a custom URL independent of its file path. If you want `/quickstart` instead of `/getting-started/getting-started`, rename the file. This is a deliberate constraint, not a missing feature: one source of truth for "where does this page live" keeps the mental model simple.

## Navigating between pages

You don't write a sidebar by hand unless you want to. If `config.json` doesn't define `sidebar` or `nav`, Bark builds the left navigation straight from your folder tree: sub-folders listed before files, alphabetical within each group. Once you're ready for more control (grouped sections, a header nav with dropdowns, different sidebars per top-level section), that's all in [Site Config](../reference/site-config).