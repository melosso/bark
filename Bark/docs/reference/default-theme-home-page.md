---
title: Homepage
description: layout:home frontmatter, hero and features
---

# Homepage

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
      link: https://github.com/melosso/bark
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
| `icon` | `string` | no | An emoji, short text, inline SVG string (`<svg>...</svg>`), or image URL shown above the title. Paths starting with `/` or `http(s)://` render as `<img>` tags; inline SVG strings are rendered directly. |
| `iconImage` | `FeatureIconConfig` | no | A themed icon object for separate light and dark variants. Takes priority over `icon` when both are set. |
| `title` | `string` | yes | Card heading. |
| `details` | `string` | yes | Card body text. |
| `link` | `string` | no | If set, the whole card becomes a link. |

**`FeatureIconConfig`**

| Field | Type | Description |
|---|---|---|
| `src` | `string?` | A single image URL used in both light and dark mode. |
| `light` | `string?` | Image URL shown in light mode. Pair with `dark` for full theming. |
| `dark` | `string?` | Image URL shown in dark mode. Pair with `light` for full theming. |
| `alt` | `string?` | Alt text for the image. Defaults to an empty string, treating the icon as decorative. |

> [!NOTE]  
> `hero.image` accepts a URL or a single emoji/character and does not currently support the `iconImage` format.

Content written below the front matter (regular Markdown) still renders, directly beneath the features grid. Use it for a short paragraph or an extra call-to-action that doesn't fit the hero/features shape.
