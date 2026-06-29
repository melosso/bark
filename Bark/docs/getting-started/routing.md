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

A relative link like `./configuration` also works, but resolves against the page's *URL*, not its location in `docs/`. Since pages serve at directory-style URLs (`/getting-started/getting-started/`, not `/getting-started/getting-started.md`), a relative link from one page to a sibling in the same folder works, but one reaching into a different folder usually doesn't. Root-relative links sidestep the problem entirely, use them by default.

## Using a base path

Some Bark deployments run under a subdirectory prefix rather than at the root of a domain. A GitHub Pages project site is a common example, where your content lives at `username.github.io/your-repo/` instead of `username.github.io/`. Configuring that `/your-repo` segment as a base path tells Bark where to expect incoming requests and where to point generated links.

When a base path is configured, Bark automatically adjusts structured front matter fields such as `hero.image` and any hero or feature action links. Links and images written directly inside your page body, however, are passed through untouched. If your site runs under a base path, it is a good idea to include that prefix yourself when writing body links. For example, with a base path of `/docs`, write `/docs/getting-started/configuration` rather than just `/getting-started/configuration`.

The same consideration applies to images embedded in your content. See [Asset Handling](../assets#base-path) for a detailed look at how images behave in subdirectory deployments.

## What happens on a request

1. The incoming path gets trimmed and lowercased the same way file paths are, so `GET /Getting-Started/Routing/` and `GET /getting-started/routing` hit the same cache entry.
2. An empty path (`/`) resolves to whatever `Docs:DefaultPage` is configured as (`index` by default).
3. No match in the page cache → a 404 page, not an exception.

::: note
Page URLs match their file paths exactly. Want `/quickstart` instead of `/getting-started/getting-started`? Rename the file. That 1:1 relationship between files and links means you always know where a page lives by looking at its URL.
:::

## Navigating Between Pages

You don't write a sidebar by hand unless you want to. If `config.json` doesn't define `sidebar` or `nav`, Bark builds the left navigation straight from your folder tree: sub-folders listed before files, alphabetical within each group. Once you're ready for more control (grouped sections, a header nav with dropdowns, different sidebars per top-level section), that's all in [Site Config](/reference/site-config).
