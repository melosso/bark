---
title: Table of Contents
description: The "On This Page" outline Bark builds from each page's headings
---

# Table of Contents

The server renders an "On this page" outline in the right-hand column of every document page, dynamically built from that page's own headings. You do not write it by hand nor have the option to customize it.

## Content

Every `##` and deeper heading becomes an entry. The page's own `#` heading is left out, since it is the title rather than a section of the page. Nesting stops at three levels, so a `#####` heading sits at the same indent as a `###` one rather than growing a fourth step.

A page with no subheadings at all still gets a single entry linking to its `#` heading, rather than an empty box. Anchors come from the same slug generation as the heading IDs, so an outline link always resolves to the heading it names.

## Disabling per page

Set `toc: false` in a page's frontmatter and the column disappears, letting the content take the full width.

```yaml
---
toc: false
---
```

There is no site-wide equivalent. The outline is derived from content that already exists, so pages that do not want one are the exception rather than the rule.

## Rendering

On screens wider than 1024px the outline is a sticky column beside your content. Between 769px and 1024px it collapses into an "On this page" disclosure above the content, using a native `<details>` element so it opens without JavaScript. Screens narrower than that, it is not rendered at all.


## Home pages

Pages with `layout: home` never show an outline, regardless of `toc`. See [Layout](/reference/default-theme-layout).
