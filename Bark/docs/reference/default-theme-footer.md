---
title: Footer
description: The content footer, configured via footer in config.json
---

# Footer

Set `footer` in `docs/config.json` and Bark renders it at the bottom of every page's content, below the pagination links.

```json
{
  "footer": "Built with Bark · [AGPL-3.0](https://github.com/melosso/bark/blob/main/LICENSE)"
}
```

`footer` is a Markdown string, not plain text. Links, bold, inline code, all of it works:

```json
{
  "footer": "Copyright 2026. Questions? [Open an issue](https://github.com/melosso/bark/issues)."
}
```

Skip `footer` and Bark renders nothing there. There's no default placeholder text to remove.

::: note Rendering the footer
The footer renders once per page, not once per site. There's no separate "footer-only" content area independent of the per-page Markdown pipeline, so anything you put here goes through the same renderer as your docs content (full support for links, code spans, and emphasis; no headings or fenced code blocks, since those don't make sense in a one-line footer).
:::

Home pages (`layout: home`) render the footer too, below the features grid. This can be hidden with [custom CSS](/getting-started/extending-themes) rules.