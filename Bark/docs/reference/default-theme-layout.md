---
title: Layout
description: The two page layouts Bark renders and what each one includes
---

# Layout

Bark has two layouts. You don't pick a layout directly. Front matter's `layout` field switches between them.

## Doc layout (default)

Every page without `layout: home` in its front matter gets the doc layout:

- Header nav bar (if `topNav` is configured).
- Left sidebar (auto-generated, or from `nav`/`sidebar` config).
- Breadcrumbs.
- A collapsible "On this page" table of contents on mobile, plus a fixed right-hand TOC on desktop.
- Your rendered Markdown content.
- "Edit this page" link (if `editLink` is configured).
- "Last updated" stamp (if enabled).
- Prev/next pagination, derived from your nav order.
- The footer (if `footer` is configured).

```yaml
---
title: Configuration
---
```

No `layout` field needed. This is the default.

## Home layout

```yaml
---
layout: home
hero:
  name: Bark
  text: Markdown in, docs site out.
---
```

Set `layout: home` and Bark drops the sidebar, breadcrumbs, and TOC entirely, replacing your content with a full-width hero section and features grid. See [homepage](default-theme-home-page) for the full `hero`/`features` schema.

> [!NOTE]  
> Home pages never show "Edit this page", "Last updated", or pagination links, regardless of config. This is intentional: a landing page isn't part of the linear reading order pagination assumes, and there's nothing to "edit" in the sense those links imply.