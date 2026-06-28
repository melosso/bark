---
title: Pagination
description: How Bark derives the pagination links at the bottom of each page
---

# Pagination

Bark renders a "previous page" and "next page" link at the bottom of every document page, derived entirely from your navigation order. You do not write these links by hand. The order comes from your `config.json` structure, or from the auto-generated navigation tree when no configuration file is present.

## How the order is determined

### Using config file (recommended)

If you have configured your `config.json`, then Bark will inherit this structure as your logic for the pagination. This means reordering your `sidebar`/`nav` entries in `config.json` directly reorders pagination.

### Without configuration

Without configuration, Bark flattens your sidebar (or auto-generated navigation tree, if you haven't configured one) into a single ordered list of pages; then looks up the current page's position in that list. Whatever comes immediately before or after becomes "prev." or "next." respectively.

## Link text

The link text is the target page's title (front matter `title`, or its filename/nav-configured fallback, the same resolution order as everywhere else). There's no separate "pagination label" distinct from the page's actual title configured by [Frontmatter](/reference/frontmatter-config).

## Disabling per page

If a particular page does not fit naturally into a linear reading sequence, you can hide its pagination links by setting `pagination: false` in the page's frontmatter. Changelog entries, standalone landing pages, and reference tables are common candidates for this.

```yaml
---
pagination: false
---
```

Only the links on that page are affected. The page itself still participates in the broader nav order, so adjacent pages continue to link to and from it normally.

## Home pages

Pages configured with `layout: home` never show pagination regardless of any other setting. See [Layout](/reference/default-theme-layout).
