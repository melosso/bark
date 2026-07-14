---
title: Sidebar
description: The left navigation tree, auto-generated or fully configured via sidebar in config.json
---

# Sidebar

Bark builds the left sidebar one of three ways, in priority order:

1. **`sidebar` in `config.json`**, matched by path prefix. Highest priority.
2. **`nav` in `config.json`**, one flat tree shared by every page. Used only when no `sidebar` prefix matches.
3. **Your folder structure**, auto-generated. Used when neither `sidebar` nor `nav` is set.

Most projects start with option 3 and graduate to option 1 once they have more than one logical section (a guide and a reference, for example).

## Multi-sidebar config

```json
{
  "sidebar": {
    "/getting-started/": [
      {
        "title": "Introduction",
        "collapsed": false,
        "items": [
          { "title": "Getting Started", "path": "getting-started/getting-started" },
          { "title": "Configuration", "path": "getting-started/configuration" }
        ]
      }
    ],
    "/reference/": [
      {
        "title": "Reference",
        "items": [
          { "title": "Site Config", "path": "reference/site-config" }
        ]
      }
    ]
  }
}
```

Each key is a path prefix. Bark picks whichever key is the **longest match** for the page you're viewing, so `/getting-started/` and `/getting-started/advanced/` can both exist, the more specific one winning for pages under it. An empty-string key (`""` or `/`) acts as a catch-all for anything not matched by a more specific prefix.

## Entries

Every entry in a sidebar array is either a link or a group, and groups nest to any depth:

| Field | Type | Description |
|---|---|---|
| `title` | `string` | Link text or group heading. |
| `path` | `string` | Leaf link target. Omit it to make this entry a group. |
| `items` | `array` | Child entries. Set this (and omit `path`) to make this entry a group. |
| `collapsed` | `bool` | Group-only. See below. |

If you leave the `title` off a group, its links render together as one heading-less cluster. You can read more about this in [Grouping links without a heading](#grouping-links-without-a-heading) further down.

## Collapse behavior

`collapsed` controls whether a group gets a toggle caret and what state it starts in:

| Value | Behavior |
|---|---|
| omitted | Not collapsible. Always expanded |
| `false` | Collapsible, starts expanded. |
| `true` | Collapsible, starts collapsed. |

A group containing the page you're currently on always renders expanded. Collapsing is implemented with native `<details>`/`<summary>`, so it works with JavaScript disabled and doesn't need any client-side state.

> [!NOTE] 
> Use static (no `collapsed` field) groups for reference material someone scans top to bottom, like this site's `/reference/` sidebar. Use collapsible groups for a guide with more sections than fit comfortably on screen at once.

## Grouping links without a heading

Sometimes you have a handful of standalone links that belong together, yet none of them really calls for a section heading above it. Rather than leaving them as separate top-level links, where each one sits on its own under a divider, you can gather them into a group and simply leave the `title` off. Bark then renders the links as one tight cluster, quietly skipping the uppercase heading that a titled group would show.

```json
{
  "sidebar": {
    "": [
      {
        "items": [
          { "title": "Config & API Reference", "path": "reference/site-config" },
          { "title": "Changelog", "path": "more/changelog" }
        ]
      }
    ]
  }
}
```

This is convenient for a closing set of reference or housekeeping links at the bottom of a guide sidebar. The cluster is still set apart from the section above it by a gentle divider, so your grouping stays clear without adding extra visual weight. A heading-less group is always expanded and is not collapsible, since there is no title to click.