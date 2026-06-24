---
title: Nav
description: The header navigation bar, configured via topNav in config.json
---

# Nav

The header nav bar comes from `topNav` in `docs/config.json`. Skip it entirely and Bark renders the topbar without one, just the brand and search box.

```json
{
  "topNav": [
    { "text": "Home", "link": "/" },
    { "text": "Guide", "link": "/getting-started/getting-started" },
    { "text": "Reference", "link": "/reference/" },
    {
      "text": "More",
      "items": [
        { "text": "GitHub", "link": "https://github.com/melosso/bark" },
        { "text": "Releases", "link": "https://github.com/melosso/bark/releases" }
      ]
    }
  ]
}
```

## Item shapes

Every `topNav` entry is one of two shapes:

| Shape | Fields | Renders as |
|---|---|---|
| Link | `text`, `link` | A direct link. |
| Dropdown | `text`, `items` | A button that opens a menu of links on hover or focus. |

Set `link` for a plain link. Set `items` (an array of the same two shapes) for a dropdown. Don't set both on the same entry.

Bark detects external links automatically: anything starting with `http://` or `https://` opens in a new tab, gets `rel="noopener noreferrer"`, and shows a small external-link icon. Internal links (anything else) get normalized to an absolute path and participate in active-link highlighting.

## Active state

A `topNav` link gets the `active` class when its `link` matches the current page's path exactly. Dropdown triggers don't get an active state. Bark doesn't currently highlight a dropdown as active when one of its children matches the current page.

## Mobile

The header nav bar hides below 768px width. Its items reappear at the top of the mobile sidebar drawer instead, dropdowns rendered as native `<details>` disclosures so they work without any JavaScript.

There's no separate mobile-specific config. Whatever you put in `topNav` drives both.
