---
title: Bark
description: Bark, a fast, lightweight Markdown documentation server
layout: home
hero:
  name: Documentation, served
  text: Markdown in, docs site out.
  tagline: Point Bark at a folder of Markdown files and it serves the whole site. There is no build step.
  actions:
    - theme: brand
      text: Get Started
      link: what-is-bark
    - theme: alt
      text: View on GitHub
      link: https://github.com/melosso/bark
features:
  - icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 4h9l5 5v11H4z"/><path d="M13 4v5h5"/><path d="M8 13h7M8 16.5h5"/></svg>'
    title: Content first
    details: Write Markdown, save the file, and the running site updates. Navigation, table of contents, breadcrumbs, and search come from your folders.
  - icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M13 3 5 13.5h5.5L11 21l8-10.5h-5.5z"/></svg>'
    title: Built on .NET 10
    details: One Kestrel process serves the site, with hot reload while you write. Deploy it with Docker, IIS, systemd, or export static HTML.
  - icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20a8 8 0 1 1 8-8"/><path d="M12 12l5-3.5"/><circle cx="12" cy="12" r="1.4"/></svg>'
    title: Fast by default
    details: Pages are rendered once and held in memory, compressed on the way out, and returned as a 304 when nothing has changed.
---
