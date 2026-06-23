---
title: Last Updated Timestamp
description: Showing when a page was last changed
---

# Last Updated Timestamp

Bark can show a "Last updated" stamp at the bottom of every page, pulled from the Markdown file's last-write time on disk.

**TIP.** This is the one deliberate difference from VitePress: VitePress reads the timestamp from `git log -1`, so it needs full git history (and breaks under shallow clones in CI unless you fix `fetch-depth`). Bark reads the filesystem directly instead. No git dependency, no shallow-clone gotcha, at the cost of the timestamp reflecting "last time this file was written to disk" rather than "last git commit that touched it." For most deployments those are the same moment anyway.

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
