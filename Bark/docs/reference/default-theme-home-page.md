---
title: Home Page
description: layout:home frontmatter, hero and features
---

# Home Page

Set `layout: home` in a page's front matter to swap the normal docs chrome (sidebar, table of contents, breadcrumbs) for a hero section and a features grid. That's exactly what `index.md` on this site uses.

```yaml
---
layout: home
hero:
  name: Bark
  text: Markdown in, docs site out.
  tagline: Drop .md files in a folder. Get a searchable, navigable docs site.
  actions:
    - theme: brand
      text: Get Started
      link: /getting-started/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/hawkinslabdev/bark
features:
  - icon: ⚡
    title: Hot reload
    details: Edit a file, see it live.
---
```

## `hero`

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | `string` | no | Small heading above `text`, typically the product name. |
| `text` | `string` | no | The big headline. |
| `tagline` | `string` | no | Supporting subtext below `text`. |
| `image` | `string` | no | A URL (rendered as `<img>`) or a single emoji/character (rendered as text) shown above the headline. |
| `actions` | `HeroAction[]` | no | Call-to-action buttons. |

**`HeroAction`**

| Field | Type | Required | Description |
|---|---|---|---|
| `theme` | `"brand"` \| `"alt"` | no | `brand` is the filled button, `alt` is the outline button. Defaults to `brand`. |
| `text` | `string` | yes | Button label. |
| `link` | `string` | yes | Where the button goes. Internal paths and external URLs both work. |

## `features`

An array of cards rendered in a responsive grid below the hero.

| Field | Type | Required | Description |
|---|---|---|---|
| `icon` | `string` | no | An emoji or short text shown above the title. |
| `title` | `string` | yes | Card heading. |
| `details` | `string` | yes | Card body text. |
| `link` | `string` | no | If set, the whole card becomes a link. |

> [!NOTE]  
> This is a deliberately simplified version of VitePress's home page schema. `hero.image`/`features[].icon` only take a plain string here, not the light/dark themeable-image object VitePress supports, and there's no `markdownStyles` toggle. If you need per-theme hero artwork, that's a real gap.

Content written below the front matter (regular Markdown) still renders, directly beneath the features grid. Use it for a short paragraph or an extra call-to-action that doesn't fit the hero/features shape.
