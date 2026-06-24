---
title: Edit Link
description: Add an "Edit this page" link via editLink in config.json
---

# Edit Link

Set `editLink` in `docs/config.json` and Bark adds an "Edit this page" link near the bottom of every doc-layout page.

```json
{
  "editLink": {
    "pattern": "https://github.com/melosso/bark/edit/main/docs/:path",
    "text": "Edit this page on GitHub"
  }
}
```

| Field | Type | Default | Description |
|---|---|---|---|
| `pattern` | `string` | none, required | A URL template. The literal text `:path` gets replaced with the current page's path. |
| `text` | `string` | `"Edit this page"` | Link label. |

`:path` resolves to the page's lowercased URL path plus `.md` (`getting-started/configuration` becomes `getting-started/configuration.md`). That matches Bark's own docs, since Bark lowercases every file path it serves.

> [!CAUTION]  
> If your filenames have capital letters (like `Configuration.md`), the edit links may break. To avoid this, always use lowercase (like `configuration.md`) for files in your `docs/` folder.

Removing `editLink` entirely and the server will not render this segment.