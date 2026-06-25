---
title: Badge
description: Small inline labels for flagging new features, version requirements, or unstable APIs
---

# Badge

A small pill-shaped label you drop inline, next to a heading, after a word, wherever you need to flag "new", "deprecated", or "requires 3.0+" without breaking the sentence.

```md
## Some heading <Badge type="tip">3.0+</Badge>
```

Bark has no client-side framework in the loop, so this is plain HTML: write `<Badge type="...">text</Badge>` directly in your Markdown, and Bark's stylesheet renders it. Markdig passes unrecognized tags through as raw HTML, and HTML lowercases tag names on parse, so the actual rendered element is `<badge>`. You never need to think about that. Always write the closing tag.

## Types

| `type` | Color | Maps to |
|---|---|---|
| `tip` | Green | `--alert-tip` (default if `type` is omitted) |
| `info` | Blue | `--alert-note` |
| `warning` | Amber | `--alert-warning` |
| `danger` | Red | `--alert-caution` |

Same four colors as [Alerts](/getting-started/markdown#alerts), so a badge and an alert block referring to the same kind of thing always match.

<Badge type="tip">tip</Badge>
<Badge type="info">info</Badge>
<Badge type="warning">warning</Badge>
<Badge type="danger">danger</Badge>

## Usage

Inline, anywhere text can go:

```md
Supports `Ctrl+K` <Badge type="tip">3.0+</Badge> on every page.
```

Right after a heading, the most common placement:

```md
## Customization <Badge type="warning">beta</Badge>
```

> [!WARNING]
> Self-closing syntax (`<Badge text="x" />`) is not supported and will break your page. HTML has no XML-style self-close for unknown elements: a stray `/>` opens an unclosed `<badge>` tag, which then silently swallows the rest of the paragraph as its content instead of rendering a label. Always write `<Badge type="...">text</Badge>` with an explicit closing tag.
