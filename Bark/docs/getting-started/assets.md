---
title: Asset Handling
description: How Bark serves images and other static files referenced from Markdown
---

# Asset Handling

Markdown files reference images, downloads, and other static files all the time. Bark treats these the same way any ASP.NET Core app treats static files: through `wwwroot/`, served by `UseStaticFiles()`. Your `docs/` folder holds content, `wwwroot/` holds assets.

## Where to put files

Put images and downloads under `wwwroot/`, then reference them with a root-relative path:

```markdown
![Architecture diagram](/images/architecture.png)

[Download the sample config](/files/sample-config.json)
```

Files in `wwwroot/` are served directly at their URL path without any build-time processing, hashing, or transformation.

::: tip
Any other files in the `docs/` folder are included in the build output but remain *inaccessible* via HTTP. Since only `wwwroot/` is exposed to the web, any assets placed within the `docs/` directory must be moved to `wwwroot/` to be served.
:::

## Relative paths don't work the way you'd expect

Bark serves pages at directory-style URLs. `getting-started/assets.md` renders at `/getting-started/assets/`, not at a path that mirrors where the file sits in `docs/`. A relative image path like `./diagram.png` resolves against that URL, not against the Markdown file's location on disk, so it almost always 404s. Use a root-relative path instead.

## Base path

If you're running Bark behind `--base-path` (or `Docs:BasePath` in config), root-relative links in **front matter** fields like `hero.image` or feature links get the base path prefixed automatically.

> [!NOTE]
> That auto-prefixing only applies to structured front matter fields, not to regular Markdown body content. An `![](...)` image inside the body of a page is rendered by Markdig as-is. If you're running behind a base path, write the base path into the image URL yourself: `![Logo](/docs/images/logo.png)` instead of `/images/logo.png`.

## External assets

Full URLs work without any rewriting:

```markdown
![Diagram hosted elsewhere](https://cdn.example.com/diagram.png)
```

## Theme assets

Theme overrides are the one asset convention Bark auto-detects: drop `custom.css` or `custom.js` into `wwwroot/theme/` and Bark picks them up at startup, no config edit required. See [Extending Themes](/getting-started/extending-themes) for details. That mechanism is for site-wide styling and scripting, not for per-page content images.
