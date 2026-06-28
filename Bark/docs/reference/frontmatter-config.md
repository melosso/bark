---
title: Frontmatter Config
description: Every field you can set in a page's YAML frontmatter
---

# Frontmatter Config

Every Markdown file accepts an optional YAML frontmatter block at the top. Bark falls back to sensible defaults for anything you skip.

```yaml
---
title: Configuration
description: appsettings.json options, docs/config.json, and theming
---
```

## Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `title` | `string` | filename or nav-configured title | Page title. Shows in the browser tab, breadcrumbs, and pagination links. |
| `description` | `string` | none | Meta description. Also shows under the title in search results and `llms.txt`. |
| `layout` | `string` | none | Set to `home` to render a hero and features grid instead of standard docs chrome. See [homepage](default-theme-home-page). |
| `hero` | `object` | none | Hero content. Only used when `layout: home`. |
| `features` | `array` | none | Feature cards. Only used when `layout: home`. |
| `keywords` | `string[]` | none | A list of keywords for the page. Emitted as `<meta name="keywords">` in the page head (capped at 20 entries) and also indexed by Bark's search at a higher weight than body text. |
| `lastUpdated` | `bool` | none | Set to `false` to hide the "Last updated" stamp on this page, overriding the site-wide setting. See [Last Updated Timestamp](default-theme-last-updated). |

`title` and `description` work everywhere. `layout`, `hero`, and `features` only do anything on a page that sets `layout: home`. `lastUpdated` only matters when the site-wide toggle is on.

> [!NOTE]  
> Bark doesn't currently support per-page `prev`/`next` overrides or a `sidebar: false` toggle. Pagination always derives from your nav order, and every page that resolves to a `.md` file gets a sidebar. See [Prev / Next Links](default-theme-prev-next-links) for the workaround.

## Title fallback order

When `title` is missing, Bark picks one in this order:

1. The filename, title-cased (`getting-started.md` becomes "Getting Started").
2. If the file is `index.md`, the parent folder name instead.
3. If `config.json`'s `nav`/`sidebar` configures a title for this page's path, that title wins over both of the above.

Front matter always wins when it's set. Run `dotnet test --filter "FullyQualifiedName~DocumentationServiceTests"` if you want to see the exact fallback behavior covered by tests.
