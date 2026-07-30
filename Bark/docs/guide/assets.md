---
title: Asset Handling
description: How Bark serves images and other static files referenced from Markdown
---

# Asset Handling

Markdown content lives in `docs/`. Images and other static files live in `wwwroot/`, which is the folder Bark actually serves over HTTP.

## Where to put files

Place assets under `wwwroot/` and reference them with a root-relative path:

```markdown
![Architecture diagram](/images/architecture.png)

[Download the sample config](/files/sample-config.json)
```

Files in `wwwroot/` are served at their URL path with no build-time processing, hashing, or transformation.

::: tip
Files left in `docs/` are copied to the build output but stay unreachable over HTTP. Move an asset to `wwwroot/` to expose it.
:::

## Relative paths

Bark serves pages at directory-style URLs. This means that `guide/assets.md` is rendered as `/guide/assets/`, instead of mirroring its location on disk with the file extension.

A relative image path such as `./diagram.png` resolves against that URL, not the file's folder, which usually produces a 404. Use root-relative paths instead.

## Base path

Behind a `--base-path` (or `Docs:BasePath`), Bark auto-prefixes root-relative links in structured front matter fields such as `hero.image` and feature links.

::: note
That prefixing covers front matter only. An `![](...)` in the page body is rendered as written, so include the prefix yourself: under a base path of `/docs`, write `![Logo](/docs/images/logo.png)` rather than `/images/logo.png`.
:::

## External assets

Assets can come from anywhere:

```markdown
![Diagram hosted elsewhere](https://cdn.example.com/diagram.png)
```

## Theme assets

`custom.css` and `custom.js` in `wwwroot/theme/` are picked up at startup with no configuration edit. See [Themes](/guide/themes).
