---
title: Routing
description: How a file path becomes a URL in Bark
---

# Routing

In Bark, you do not need to configure any routing files. The system automatically turns your file structure into website URLs. It uses a simple "catch-all" method where every file on your disk is mapped directly to a web address.

## The rules

Bark follows these three simple steps to create a URL from a file:

1. **Remove the extension:** The `.md` part is deleted.
2. **Handle Index files:** If you name a file `index.md`, it becomes the main page for that folder. It does not add "/index" to the URL.
3. **Clean up text:** All URLs are converted to lowercase and extra slashes are removed. This ensures that different ways of typing a path (like capitalized or uncapitalized) always lead to the same page.

## Examples

| File Path | Resulting URL |
|---|---|
| `docs/index.md` | `/` |
| `docs/getting-started/index.md` | `/getting-started` |
| `docs/getting-started/getting-started.md` | `/getting-started/getting-started` |
| `docs/Reference/API-Reference.md` | `/reference/api-reference` |


## What happens on a request

1. The incoming path gets trimmed and lowercased the same way file paths are, so `GET /Getting-Started/Routing/` and `GET /getting-started/routing` hit the same cache entry.
2. An empty path (`/`) resolves to whatever `Docs:DefaultPage` is configured as (`index` by default).
3. No match in the page cache → a 404 page, not an exception.

> [!NOTE]  
> There's no way to give a page a custom URL independent of its file path. If you want `/quickstart` instead of `/getting-started/getting-started`, rename the file. This is a deliberate constraint: one source of truth for "where does this page live" keeps the mental model simple.

## Navigating between pages

You don't write a sidebar by hand unless you want to. If `config.json` doesn't define `sidebar` or `nav`, Bark builds the left navigation straight from your folder tree: sub-folders listed before files, alphabetical within each group. Once you're ready for more control (grouped sections, a header nav with dropdowns, different sidebars per top-level section), that's all in [Site Config](../reference/site-config).