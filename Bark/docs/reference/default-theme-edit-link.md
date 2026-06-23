---
title: Edit Link
description: Add an "Edit this page" link via editLink in config.json
---

# Edit Link

Set `editLink` in `docs/config.json` and Bark adds an "Edit this page" link near the bottom of every doc-layout page.

```json
{
  "editLink": {
    "pattern": "https://github.com/hawkinslabdev/bark/edit/main/docs/:path",
    "text": "Edit this page on GitHub"
  }
}
```

| Field | Type | Default | Description |
|---|---|---|---|
| `pattern` | `string` | none, required | A URL template. The literal text `:path` gets replaced with the current page's path. |
| `text` | `string` | `"Edit this page"` | Link label. |

`:path` resolves to the page's lowercased URL path plus `.md` (`getting-started/configuration` becomes `getting-started/configuration.md`). That matches Bark's own docs, since Bark lowercases every file path it serves.

> [!IMPORTANT]  
> If your actual filenames use mixed case (`Configuration.md` instead of `configuration.md`), the generated edit link will be wrong, since Bark only knows the URL path, not the original on-disk filename casing. Keep your `docs/` filenames lowercase and this isn't a problem.

Skip `editLink` entirely and Bark renders nothing. There's no default GitHub URL guessed from your git remote, the way some tools attempt.

Home pages never show an edit link. See [Layout](default-theme-layout).
