---
title: Home
description: Bark — a fast, lightweight Markdown documentation server for .NET
---

# Welcome to Bark 🐾

Let's be real: every team has a docs folder somewhere that's either a graveyard of stale Confluence pages or a static-site-generator build pipeline nobody wants to touch after the person who set it up left. Bark exists because "drop a `.md` file, get a docs site" shouldn't require a build step, a JavaScript framework, or three new YAML configs you'll forget the syntax for by next quarter.

Bark is a single ASP.NET Core process. Point it at a folder of Markdown, and it renders pages on the fly — navigation, search index, table of contents, and breadcrumbs all rebuilt automatically the moment a file changes on disk.

### What I Realize:

Most "docs as code" setups optimize for the wrong thing — they treat documentation like a deploy artifact (build it, ship it, cache-bust it) instead of like the living thing it actually is. Bark treats the filesystem as the source of truth and reacts to it in real time, the same way you'd want your dev server to react to a saved file.

> **The 80% Truth:** Nobody updates docs that require a rebuild step to see take effect.

## Features

- **Markdown rendering** — CommonMark + GitHub-flavored extras (tables, task lists, footnotes, alert blocks, emoji) via Markdig.
- **Hot reload** — edit a `.md` file or `bark.json`, see it live. No restart.
- **Search** — built-in in-memory inverted index, exposed at `/api/search?q=`.
- **Navigation, breadcrumbs, TOC, prev/next pagination** — generated automatically from your folder structure and headings.
- **Theming** — CSS variable overrides and dark mode toggle, configured via `appsettings.json`.
- **Production-minded defaults** — response compression, Kestrel limits, structured logging via Serilog, ETag-based caching, a sitemap at `/sitemap.xml`.

### What I Learned:

A docs server doesn't need to be clever to be good. It needs to never be the reason a teammate didn't update a page. Bark's whole design is built around removing friction between "I changed something" and "the docs reflect it" — that's it.

---

Keep reading: [Installation](getting-started/installation) to get it running, then [Configuration](getting-started/configuration) to make it yours.

**P.S.** If you came here expecting a Jamstack-generator comparison table, you won't find one in these docs — Bark isn't trying to out-feature Docusaurus. It's intentionally narrow: render Markdown fast, reload instantly, stay out of your way. Specialization beats a kitchen-sink feature list every time.
