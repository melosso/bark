---
title: Pagination
description: How Bark derives the pagination links at the bottom of each page
---

# Pagination

Bark renders a "previous page" and "next page" link at the bottom of every document page, derived entirely from your navigation order. You don't write these by hand, nor can you adjust the logic of these from the Markdown files.

## How the order is determined

### Using config file (recommended)

If you have configured your `config.json`, then Bark will inherit this structure as your logic for the pagination. This means reordering your `sidebar`/`nav` entries in `config.json` directly reorders pagination.

### Without configuration

Without configuration, Bark flattens your sidebar (or auto-generated navigation tree, if you haven't configured one) into a single ordered list of pages; then looks up the current page's position in that list. Whatever comes immediately before or after becomes "prev." or "next." respectively.

## Link text

The link text is the target page's title (front matter `title`, or its filename/nav-configured fallback, the same resolution order as everywhere else). There's no separate "pagination label" distinct from the page's actual title.

::: note
Bark doesn't currently support per-page `prev`/`next` overrides or a `false` value to suppress one side. If a page's auto-derived neighbor is wrong for your structure, fix it by reordering your `sidebar` config instead. That's the only lever available today.
:::

## Home pages

`layout: home` pages never show pagination. See [Layout](default-theme-layout).
