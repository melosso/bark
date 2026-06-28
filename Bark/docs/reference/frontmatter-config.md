---
title: Frontmatter Config
description: Every field you can set in a page's YAML frontmatter
---

# Frontmatter Config

Frontmatter is [widely used](https://www.markdownlang.com/advanced/frontmatter.html){target="_blank" rel="noopener"} and documented way to add metadata to your Markdown documents. Every Markdown file accepts an optional YAML frontmatter block at the top. Bark falls back to sensible defaults for anything you skip, so a minimal file with no frontmatter at all is completely valid and a reasonable starting point.

```yaml
---
title: Configuration
description: appsettings.json options, docs/config.json, and theming
---
```

::: note
Bark reads frontmatter keys using camelCase (`lastUpdated`, not `last_updated`). Fields that do not match the expected name are quietly ignored.
:::

## Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `title` | `string` | filename or nav-configured title | Page title. Shows in the browser tab, breadcrumbs, and pagination links. |
| `description` | `string` | none | Meta description. Also shows under the title in search results and `llms.txt`. |
| `layout` | `string` | none | Set to `home` to render a hero and features grid instead of standard docs chrome. See [Home Page](/reference/default-theme-home-page). |
| `hero` | `object` | none | Hero content. Only used when `layout: home`. |
| `features` | `array` | none | Feature cards. Only used when `layout: home`. |
| `keywords` | `string[]` | none | A list of keywords for the page. Emitted as `<meta name="keywords">` in the page head (capped at 20 entries) and also indexed by Bark's search at a higher weight than body text. |
| `lastUpdated` | `bool` | inherits site-wide setting | Set to `false` to hide the "Last updated" stamp on this page, overriding the site-wide setting. See [Last Updated Timestamp](/reference/default-theme-last-updated). |
| `pagination` | `bool` | `true` | Set to `false` to hide the previous and next page links at the bottom of this page. Useful for standalone landing pages or pages that do not fit naturally into a linear reading order. |
| `toc` | `bool` | `true` | Set to `false` to hide the table of contents on this page. |
| `redirect` | `string` | none | When set, Bark issues a redirect to the given URL instead of rendering the page. See [Redirects](#redirects) below. |
| `date` | `string` (ISO 8601) | none | Content creation date. Overrides the file system timestamp for the "Last updated" display when `updated` is not also set. |
| `updated` | `string` (ISO 8601) | none | Last-modified date. Takes priority over `date` and the file system timestamp for the "Last updated" display. |

`title` and `description` work on every page. `layout`, `hero`, and `features` only apply when `layout: home` is set. `lastUpdated`, `pagination`, and `toc` only have a visible effect when their respective site-wide features are active.

::: tip
Bark does not currently support a per-page `sidebar: false` toggle. Every page that resolves to a `.md` file receives a sidebar automatically. See [Sidebar](/reference/default-theme-sidebar) for options around structuring and scoping sidebar navigation.
:::

## Redirects

The `redirect` field is a convenient way to forward readers from an old URL to a new one without breaking existing bookmarks or inbound links. When Bark serves a page that has `redirect` set, it issues a temporary (307) redirect to the target URL instead of rendering any content, so the body of the `.md` file does not matter.

Root-relative paths (starting with `/`) are automatically prefixed with your configured base path, so `redirect: /guide/getting-started` will resolve correctly whether Bark is served from the domain root or from a subpath like `/docs`. Absolute URLs starting with `http://` or `https://` are passed through exactly as written.

```yaml
---
redirect: /guide/getting-started
---
```

A particularly useful pattern is page renaming. If you move `old-name.md` to `new-name.md`, you can leave the old file in place with a `redirect` pointing to the new location. Readers who bookmarked or linked to the old URL will arrive at the right page without noticing the change.

## Dates

By default, the "Last updated" display at the bottom of a page reflects the file's last-modified time on disk. This works well when your content and your files stay in sync, but it can produce surprising results when files are moved, renamed in bulk, or downloaded fresh by a deployment tool that stamps everything with today's date.

The `date` and `updated` fields let you pin the displayed date directly in the frontmatter. When `updated` is present it takes priority over `date`, and either one overrides the file's last-modified time completely.

```yaml
---
date: 2025-03-01
updated: 2025-06-28
---
```

A sensible convention is to set `date` when a page is first written and then set or update `updated` whenever you revise the content significantly. Pages that omit both fields continue to use the file's last-modified time as before, so you can adopt explicit dates incrementally, one page at a time, without touching anything else.

## Title fallback order

When `title` is missing from frontmatter, Bark picks one in this order:

1. The filename, title-cased (`getting-started.md` becomes "Getting Started").
2. If the file is `index.md`, the parent folder name is used instead.
3. If `config.json`'s `nav` or `sidebar` configures a title for this page's path, that title takes priority over the filename.

A `title` set in frontmatter always overrides all of the above.
