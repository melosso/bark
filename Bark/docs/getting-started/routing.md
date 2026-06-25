---
title: Routing
description: How a file path becomes a URL in Bark
---

# Routing

Bark uses file-based routing: the URL for a page is derived directly from its path in `docs/`. There's no route table to maintain, just a single catch-all handler that maps a request path straight onto the page cache.

## File-Based Routing

Bark turns a file path into a URL in three steps:

1. **Drop the extension.** `.md` goes away.
2. **Collapse `index.md`.** It becomes the page for its folder, no `/index` suffix.
3. **Lowercase and trim.** Slashes get trimmed, casing gets normalized, so `/Getting-Started/` and `/getting-started/` resolve to the same page.

| File Path | Resulting URL |
|---|---|
| `docs/index.md` | `/` |
| `docs/getting-started/index.md` | `/getting-started` |
| `docs/getting-started/getting-started.md` | `/getting-started/getting-started` |
| `docs/Reference/API-Reference.md` | `/reference/api-reference` |

## Root Directory

By default Bark reads from `docs/`, relative to the app's working directory. Point it elsewhere with `Docs:RootPath` in `appsettings.json`, see [Site Config](/reference/site-config) for the full option list.

## Linking Between Pages

Use root-relative links between pages, the same shape Bark generates for navigation and breadcrumbs:

```md
See the [Configuration](/getting-started/configuration) guide.
```

> [!WARNING]
> Links and images in your page body aren't rewritten for `BasePath`. If you run Bark behind a base path (a GitHub Pages project page, a reverse-proxy subpath), write that prefix into the link yourself: `/docs/getting-started/configuration` instead of `/getting-started/configuration`. Only structured front matter fields (`hero.image`, hero/feature links) get the prefix added automatically. See [Asset Handling](assets#base-path) for the same caveat applied to images.

A relative link like `./configuration` also works, but resolves against the page's *URL*, not its location in `docs/`. Since pages serve at directory-style URLs (`/getting-started/getting-started/`, not `/getting-started/getting-started.md`), a relative link from one page to a sibling in the same folder works, but one reaching into a different folder usually doesn't. Root-relative links sidestep the problem entirely, use them by default.

## What happens on a request

1. The incoming path gets trimmed and lowercased the same way file paths are, so `GET /Getting-Started/Routing/` and `GET /getting-started/routing` hit the same cache entry.
2. An empty path (`/`) resolves to whatever `Docs:DefaultPage` is configured as (`index` by default).
3. No match in the page cache → a 404 page, not an exception.

::: note
Page URLs match their file paths exactly. Want `/quickstart` instead of `/getting-started/getting-started`? Rename the file. That 1:1 relationship between files and links means you always know where a page lives by looking at its URL.
:::

## Navigating Between Pages

You don't write a sidebar by hand unless you want to. If `config.json` doesn't define `sidebar` or `nav`, Bark builds the left navigation straight from your folder tree: sub-folders listed before files, alphabetical within each group. Once you're ready for more control (grouped sections, a header nav with dropdowns, different sidebars per top-level section), that's all in [Site Config](/reference/site-config).
