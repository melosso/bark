---
title: HTML Metadata
description: Configuring page titles, meta descriptions, the HTML lang attribute, and arbitrary head tags in docs/config.json
---

# HTML Metadata

When search engines index your site, social platforms generate previews, and assistive technologies announce your page to readers, the metadata in `<head>` is the first thing they see. Bark gives you a comfortable set of controls for all of it directly inside `docs/config.json`, so the same file that shapes your navigation also shapes how your content appears to the wider web.

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

> [!NOTE]
> If you set `titleTemplate` without setting `title`, the `:siteName` placeholder resolves to an empty string. Setting `titleTemplate` to a fixed string, such as `"Bark Docs"`, is entirely valid if you want every tab to show the same title regardless of the current page.

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

## Extra Head Tags

For anything that falls outside the fields above, the `head` array lets you inject arbitrary tags into `<head>` on every page. This is particularly useful for Open Graph metadata, canonical links, structured data, or any third-party initialization snippets.

Each entry in the array is an object with three fields:

| Field | Type | Description |
|---|---|---|
| `tag` | `string` | The HTML tag name, for example `"meta"`, `"link"`, or `"script"`. |
| `attrs` | `Record<string, string>?` | Attribute key-value pairs. Values are HTML-encoded automatically. |
| `content` | `string?` | Inner HTML for tags that wrap content, such as `<script>` or `<style>`. Void elements like `<meta>` and `<link>` do not use this field. |

Here is a practical example that adds Open Graph metadata and a structured data block:

```json
{
  "head": [
    { "tag": "meta", "attrs": { "property": "og:type", "content": "website" } },
    { "tag": "meta", "attrs": { "property": "og:site_name", "content": "Bark" } },
    {
      "tag": "script",
      "attrs": { "type": "application/ld+json" },
      "content": "{\"@context\":\"https://schema.org\",\"@type\":\"WebSite\",\"name\":\"Bark\"}"
    }
  ]
}
```

Bark recognizes `<meta>`, `<link>`, and `<base>` as void elements and renders them without a closing tag. Every other tag receives an opening tag, the `content` value if provided, and a closing tag.

> [!NOTE]
> All entries in `head` are added to every page on your site. For per-page metadata such as individual descriptions and custom titles, please see [Frontmatter Config](/reference/frontmatter-config).

## A Complete Example

```json
{
  "title": "Bark",
  "titleTemplate": ":title | :siteName",
  "description": "A fast, lightweight documentation server for ASP.NET Core.",
  "lang": "en",
  "head": [
    { "tag": "meta", "attrs": { "property": "og:type", "content": "website" } },
    { "tag": "link", "attrs": { "rel": "canonical", "href": "https://bark.example.com" } }
  ]
}
```
