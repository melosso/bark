---
title: Routing
description: How a file path becomes a URL in Bark
---

# Routing

Bark uses file-based routing: a page's URL comes straight from its path in `docs/`. There is no route table to maintain, just a catch-all handler mapping the request path onto the page cache.

## From file path to URL

Three steps:

1. **Drop the extension.** `.md` will not be used in the URI.
2. **Collapse `index.md`.** It becomes the page for its folder, with no `/index` suffix.
3. **Lowercase and trim.** Slashes are trimmed and casing normalized, so `/Guide/` and `/guide/` are the same page.

| File path | Resulting URL |
|---|---|
| `docs/index.md` | `/` |
| `docs/guide/index.md` | `/guide` |
| `docs/guide/getting-started.md` | `/guide/getting-started` |
| `docs/Reference/API-Reference.md` | `/reference/api-reference` |

Bark reads from `docs/` relative to the app's working directory. Point it elsewhere with `Docs:RootPath`. See [Site Config](/reference/site-config) for the full option list.

## Linking between pages

Use root-relative links, the same shape Bark generates for navigation and breadcrumbs:

```md
See the [Configuration](/guide/configuration) guide.
```

A relative link such as `./configuration` works too, but it resolves against the page's *URL*, not its location in `docs/`. Because pages serve at directory-style URLs (`/guide/getting-started/`, not `/guide/getting-started.md`), a relative link to a sibling in the same folder is fine, while one reaching into another folder usually is not. Root-relative links avoid the question, so prefer them.

## Using a base path

Some deployments run under a subdirectory rather than at a domain root. A GitHub Pages project site is the common case, serving content at `username.github.io/your-repo/`. Configuring `/your-repo` as the base path tells Bark where requests arrive and where generated links should point.

Bark adjusts structured front matter for you, including `hero.image` and any hero or feature action links. Links and images written in the page body are passed through untouched, so include the prefix yourself when writing them. Under a base path of `/docs`, write `/docs/guide/configuration` rather than `/guide/configuration`.

The same applies to embedded images. See [Asset Handling](/guide/assets#base-path) for how they behave in subdirectory deployments.

## What happens on a request

1. The incoming path is trimmed and lowercased the same way file paths are, so `GET /Guide/Routing/` and `GET /guide/routing` hit one cache entry.
2. An empty path (`/`) resolves to `Docs:DefaultPage`, which is `index` by default.
3. No match in the page cache returns a 404 page, not an exception.

::: note
Page URLs match their file paths exactly. Want `/quickstart` instead of `/guide/getting-started`? Rename the file. That 1:1 relationship means you always know where a page lives by looking at its URL.
:::

## Building the sidebar

You do not write a sidebar by hand unless you want to. With no `sidebar` or `nav` in `config.json`, Bark builds the left navigation from your folder tree, listing sub-folders before files and sorting alphabetically within each group. For grouped sections, a header nav with dropdowns, or a different sidebar per section, see [Configuration](/guide/configuration).
