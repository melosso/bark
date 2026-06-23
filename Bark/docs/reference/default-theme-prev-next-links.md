---
title: Prev / Next Links
description: How Bark derives the pagination links at the bottom of each page
---

# Prev / Next Links

Bark renders a "previous page" and "next page" link at the bottom of every doc-layout page, derived entirely from your navigation order. You don't write these by hand.

## How the order is determined

Bark flattens your sidebar (or auto-generated nav tree, if you haven't configured one) into a single ordered list of pages, then looks up the current page's position in that list. Whatever comes immediately before becomes "prev." Whatever comes immediately after becomes "next." First page gets no "prev." Last page gets no "next."

This means reordering your `sidebar`/`nav` entries in `config.json` directly reorders pagination, no separate configuration needed.

## Link text

The link text is the target page's title (front matter `title`, or its filename/nav-configured fallback, the same resolution order as everywhere else). There's no separate "pagination label" distinct from the page's actual title.

> [!NOTE]  
> Bark doesn't currently support per-page `prev`/`next` overrides or a `false` value to suppress one side, unlike vitepress's frontmatter `prev: false` / `next: { text, link }`. If a page's auto-derived neighbor is wrong for your structure, fix it by reordering your `sidebar` config instead. That's the only lever available today.

## Home pages

`layout: home` pages never show pagination. See [Layout](default-theme-layout).
