---
title: What is Bark?
description: A fast, lightweight Markdown documentation server
---

# What is Bark?

Bark is a documentation server written on .NET. You write your content in Markdown, point Bark at the folder, and it serves a full documentation site: navigation, a table of contents, breadcrumbs, and search, all built in. Bark runs as a single process and serves your pages directly.

<div class="tip custom-block">

Just want to try it out? Skip to the [Quickstart](../getting-started/getting-started).

</div>

## Why Bark exists

Most documentation tools are static site generators. You write Markdown, run a build, and that build creates a folder of HTML files you then deploy somewhere. That is a fine workflow, but it puts a build pipeline between you editing a page and someone reading it.

Bark skips that step, as it reads your Markdown and renders the site in memory when it starts, and again whenever a file changes. Save a file, and the running site updates right away. There is nothing to build and nothing to deploy other than Bark itself.

## Bark vs. wikis

A wiki like Confluence stores pages in a database and edits them through a web form. That's convenient until you want to review a change before it goes live, or track who changed what and why. Reviewing wiki edits the way you review code isn't really possible.

Bark's content is just Markdown files in a folder. Keep them in git, and documentation changes go through the same pull request review as everything else you write.

## Bark vs. other site generators

Tools like [Hugo](https://github.com/gohugoio/hugo){target="_blank" rel="noopener"}, [MkDocs](https://github.com/mkdocs/mkdocs){target="_blank" rel="noopener"}, and [VitePress](https://github.com/vuejs/vitepress){target="_blank" rel="noopener"} are great if you're happy deploying to static hosting and don't mind a build step. Bark is especially interesting for teams already running .NET.

## Performance

Bark is designed to be incredibly fast by keeping everything in your computer's memory rather than reading from the hard drive every time someone visits your site.

**While you are writing:**<br>
Bark keeps a close eye on your files. If you make changes, it waits for a brief moment to ensure you are done saving before it updates your site. This ensures that even if you save several times in quick succession, it only updates once, saving time and resources.

**For your readers:**<br>
Visitors enjoy nearly instant load times. Because all the information is ready in memory, looking up a page is essentially immediate. Additionally, Bark uses smart caching; if a reader visits a page they have already seen, the site quickly confirms nothing has changed, avoiding the need to download the full page again.

## Ready to try it out?

Continue to [Getting Started](/getting-started/getting-started) and have a site running in under a minute.
