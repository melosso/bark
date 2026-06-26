---
title: Asset Handling
description: How Bark serves images and other static files referenced from Markdown
---

# Asset Handling

When writing documentation, you will often need to reference images or other files. You can keep your Markdown content in the `docs/` folder, while placing your static assets in the `wwwroot/` folder to make them accessible to the web. 

## Where to put files

Sometimes you may need to provide static assets like images or downloadable files alongside your documentation. Place these files under the `wwwroot/` directory, and reference them using a root-relative path:

```markdown
![Architecture diagram](/images/architecture.png)

[Download the sample config](/files/sample-config.json)
```

Files placed in `wwwroot/` are served directly at their URL path without any build-time processing, hashing, or transformation.

::: tip
Any files left in the `docs/` folder are included in the build output, but they remain inaccessible via HTTP. If you would like an asset to be exposed to the web, ensure it is moved to the `wwwroot/` directory.
:::

## Relative paths

Because Bark serves pages at directory-style URLs, you might notice that relative paths behave a bit differently than you expect. For example, `getting-started/assets.md` is rendered at `/getting-started/assets/`, rather than a path that mirrors the file's location on disk.

A relative image path like `./diagram.png` will resolve against that URL, which can sometimes lead to a 404 error. In this case, we recommend using a root-relative path instead.

## Base path

If your setup requires running Bark behind a `--base-path` (or `Docs:BasePath` in config), the application will auto-prefix the base path for root-relative links in structured **front matter** fields, such as `hero.image` or feature links.

::: note
Keep in mind that this auto-prefixing only applies to structured front matter fields, and not to your regular Markdown body content. An `![](...)` image inside the body of a page is rendered exactly as-is. If you are running behind a base path, you will need to write the base path into the image URL yourself (e.g., `![Logo](/docs/images/logo.png)` instead of `/images/logo.png`).
:::

## External assets

You can, of course, reference assets from external sources if you prefer:

```markdown
![Diagram hosted elsewhere](https://cdn.example.com/diagram.png)
```

## Theme assets

If you would like to apply theme overrides, Bark auto-detects files like `custom.css` or `custom.js`. Drop them into the `wwwroot/theme/` folder and Bark will pick them up at startup without requiring any configuration edits. See [Extending Themes](/getting-started/extending-themes) for more details on this process. N
