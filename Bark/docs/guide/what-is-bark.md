---
title: What is Bark?
description: A fast, lightweight Markdown documentation server
---

# What is Bark?

Bark is a documentation server written on .NET. Point it at a folder of Markdown and it serves a full site with navigation, table of contents, breadcrumbs, and search, all from a single process.

Most documentation tools are static site generators: write Markdown, run a build, deploy the output. Bark renders in memory at startup and again whenever a file changes. Save a file and the running site updates. Nothing to build, nothing to deploy but Bark itself.

## How it compares

**Against a wiki.** Confluence stores pages in a database and edits them through a web form, which makes reviewing a change before it goes live awkward. Bark's content is Markdown in a folder. Keep it in git and docs go through the same pull request review as your code.

**Against other generators.** [Hugo](https://github.com/gohugoio/hugo){target="_blank" rel="noopener"}, [MkDocs](https://github.com/mkdocs/mkdocs){target="_blank" rel="noopener"} and [VitePress](https://github.com/vuejs/vitepress){target="_blank" rel="noopener"} are the better fit if you are happy with static hosting and a build step. Bark suits teams already running .NET.

## Performance

Pages are held in memory, so lookups are immediate. Nothing touches disk per request.

- Edits are debounced, so a burst of saves triggers one rebuild.
- Responses have an ETag, so a returning reader gets a 304 instead of the page again.

## Ready to try it out?

Continue to [Getting Started](/guide/getting-started) and have a site running in under a minute.
