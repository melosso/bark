---
title: Global Meta Tags
description: Configuring page titles, meta descriptions, the HTML lang attribute, and arbitrary head tags in docs/config.json
---

# Global Meta Tags

When search engines index your site, social platforms generate previews, and assistive technologies announce your page to readers, the metadata in `<head>` is the first thing they see. 

Bark gives you a comfortable set of controls for all of it directly inside `docs/config.json`, so the same file that shapes your navigation also shapes how your content appears to the wider web.

## Page Titles

By default, each page takes its title from its frontmatter `title` field. Adding a `title` at the site level turns that into a suffix pattern, so your visitors always know which documentation site they are reading.

```json
{
  "title": "Bark"
}
```

A page called "Getting Started" becomes `Getting Started | Bark` in the browser tab automatically. If no site `title` is set, the page title is used on its own, exactly as it was before.

If you would like more control over the separator or the order, `titleTemplate` lets you define the pattern yourself using two placeholders:

| Placeholder | Replaced with |
|---|---|
| `:title` | The current page's title |
| `:siteName` | The value of `title` |

```json
{
  "title": "Bark",
  "titleTemplate": ":title · :siteName"
}
```

A page called "Configuration" would then produce `Configuration · Bark`. You can place the placeholders in any order and use any separator character you like.

::: info
If you set `titleTemplate` without setting `title`, the `:siteName` placeholder resolves to an empty string. Setting `titleTemplate` to a fixed string, such as `"Bark Docs"`, is entirely valid if you want every tab to show the same title regardless of the current page.
:::

## Site Description

The `description` field sets a site-wide `<meta name="description">` fallback. Any page that provides its own `description` in frontmatter will use that value instead, so this setting only applies to pages where no frontmatter description has been supplied.

```json
{
  "description": "A fast, lightweight documentation server for ASP.NET Core."
}
```

This is a practical way to ensure every page carries at least some meta description, without having to add frontmatter to every single file in your docs.

## Language

The `lang` field sets the `lang` attribute on the root `<html>` element. It defaults to `"en"` if left unset, which is appropriate for most English-language documentation sites. For sites written in another language, or for multi-audience sites where a specific language tag helps screen readers and search engines, it is recommended to set it explicitly.

```json
{
  "lang": "fr"
}
```

Language tag values follow the [BCP 47 standard](https://www.ietf.org/rfc/bcp/bcp47.txt). Common values include `"en"`, `"fr"`, `"de"`, `"ja"`, `"zh-CN"`, and `"pt-BR"`.

## Automatic Canonical and Social Meta

Bark automatically emits a canonical link and Open Graph / Twitter Card tags on every page. You do not need to configure any of these manually.

The canonical URL is derived from the request scheme, host, and page path. A page at `/getting-started/installation/` on `https://docs.example.com` produces:

```html
<link rel="canonical" href="https://docs.example.com/getting-started/installation/">
```

Open Graph and Twitter Card tags are generated from the same canonical URL, the page's own `title` and `description`, and your site settings:

```html
<meta property="og:type" content="article">
<meta property="og:title" content="Installation">
<meta property="og:url" content="https://docs.example.com/getting-started/installation/">
<meta property="og:site_name" content="Bark">
<meta property="og:locale" content="en">
<meta property="og:description" content="How to install Bark.">
<meta name="twitter:description" content="How to install Bark.">
<meta property="og:image" content="https://docs.example.com/site-og.png">
<meta name="twitter:image" content="https://docs.example.com/site-og.png">
<meta name="twitter:card" content="summary_large_image">
<meta name="twitter:title" content="Installation">
```

A few notes on how these are filled in:

- `og:site_name` comes from your `brand` or `title`, and `og:locale` from `lang`.
- The home page (`/`) uses `og:type` of `website`; all other pages use `article`, and article pages also carry an `article:modified_time`.
- Description tags are omitted when the page has no description.

### Social Preview Image

The preview image is resolved for each page in order: the page's own frontmatter `image`, then a site-wide `image` in `config.json`, then your `brandImage`. A relative path such as `/og.png` is turned into an absolute URL against the request origin for you.

```json
{
  "image": "/site-og.png"
}
```

When an image is found, Bark also upgrades the Twitter card to `summary_large_image`. Without one, it stays at `summary` and the image tags are simply omitted. To set a preview per page, add `image` to that page's frontmatter (see [Frontmatter Config](/reference/frontmatter-config)).

### Structured Data (JSON-LD)

Bark also emits a `application/ld+json` block so search engines can read your pages as structured data. The home page is described as a `WebSite`, and every other page as an `Article` with its title, description, image, and modified date. When a page has breadcrumbs, a `BreadcrumbList` is included as well.

There is nothing to configure. The block reflects the live request, and it carries the page nonce so it passes a strict Content Security Policy.

::: info
Canonical, Open Graph, Twitter Card, and JSON-LD are all generated from the live request, so they stay correct on their own. If you add matching tags through the `head` array below, yours appear alongside the automatic ones rather than replacing them.
:::

## Extra Head Tags

For anything that falls outside the fields above, the `head` array lets you inject arbitrary tags into `<head>` on every page. This is particularly useful for structured data, site-name Open Graph metadata, or any third-party initialization snippets.

Each entry in the array is an object with three fields:

| Field | Type | Description |
|---|---|---|
| `tag` | `string` | The HTML tag name, for example `"meta"`, `"link"`, or `"script"`. |
| `attrs` | `Record<string, string>?` | Attribute key-value pairs. Values are HTML-encoded automatically. |
| `content` | `string?` | Inner HTML for tags that wrap content, such as `<script>` or `<style>`. Void elements like `<meta>` and `<link>` do not use this field. |

Here is a practical example that adds supplementary Open Graph fields and a structured data block:

```json
{
  "head": [
    { "tag": "meta", "attrs": { "property": "og:site_name", "content": "Bark" } },
    { "tag": "meta", "attrs": { "property": "og:image", "content": "https://bark.example.com/og-image.png" } },
    {
      "tag": "script",
      "attrs": { "type": "application/ld+json" },
      "content": "{\"@context\":\"https://schema.org\",\"@type\":\"WebSite\",\"name\":\"Bark\"}"
    }
  ]
}
```

Bark recognizes `<meta>`, `<link>`, and `<base>` as void elements and renders them without a closing tag. Every other tag receives an opening tag, the `content` value if provided, and a closing tag.

::: info
All entries in `head` are added to every page on your site. For per-page metadata such as individual descriptions and custom titles, please see [Frontmatter Config](/reference/frontmatter-config).
:::

## A Complete Example

```json
{
  "title": "Bark",
  "titleTemplate": ":title | :siteName",
  "description": "A fast, lightweight documentation server for ASP.NET Core.",
  "lang": "en",
  "head": [
    { "tag": "meta", "attrs": { "property": "og:site_name", "content": "Bark" } },
    {
      "tag": "script",
      "attrs": { "type": "application/ld+json" },
      "content": "{\"@context\":\"https://schema.org\",\"@type\":\"WebSite\",\"name\":\"Bark\"}"
    }
  ]
}
```

Bark generates the canonical link, the full Open Graph and Twitter Card set (including `og:site_name`, `og:locale`, and `og:image`), and a JSON-LD block automatically on every page, so those are intentionally absent from this example. The `head` array is for anything beyond that, such as third-party verification tags or your own extra structured data.
