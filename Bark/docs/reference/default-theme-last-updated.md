---
title: Last Updated Timestamp
description: Showing when a page was last changed
---

# Last Updated Timestamp

Bark can show a "Last updated" stamp at the bottom of every page, pulled from the Markdown file's last-write time on disk.

## Turn it on

```json
{
  "lastUpdated": true
}
```

in `docs/config.json`. Off by default.

## Per-page override

```yaml
---
lastUpdated: false
---
```

Useful for pages that are intentionally evergreen (a glossary, a license page) where a stale-looking timestamp would be misleading even though the content hasn't actually changed.

That's the entire feature surface: one site-wide switch, one front-matter override.
