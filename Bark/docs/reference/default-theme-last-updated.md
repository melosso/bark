---
title: Last Updated Timestamp
description: Showing when a page was last changed
---

# Last Updated Timestamp

Bark can show a "Last updated" stamp at the bottom of every page. By default the date comes from the Markdown file's last-write time on disk, though you can pin a specific date in frontmatter when the filesystem timestamp is not reliable.

## Turn it on

```json
{
  "lastUpdated": true
}
```

in `docs/config.json`. Off by default.

## Per-page override

Setting `lastUpdated: false` in a page's frontmatter hides the stamp on that page regardless of the site-wide setting. This is useful for pages that are intentionally evergreen, such as a glossary or a license page, where a timestamp might give the impression of staleness even though the content has not changed.

```yaml
---
lastUpdated: false
---
```

## Pinning a date

File system timestamps are not always meaningful. When a deployment tool downloads or copies your files fresh, it tends to stamp every file with today's date rather than the date the content was written. Moving or renaming a file has the same effect. The `date` and `updated` frontmatter fields let you record the actual date directly alongside the content, so the stamp reflects when the page was written rather than when the file last landed on disk.

```yaml
---
date: 2025-03-01
updated: 2025-06-28
---
```

When `updated` is present it takes priority. When only `date` is set, that value is used. When neither is set, Bark falls back to the file's last-write time as before. A reasonable convention is to set `date` when a page is first published and then add or update `updated` when the content changes significantly.

See [Frontmatter Config](/reference/frontmatter-config#dates) for the full field reference.
