---
title: Configuration
description: appsettings.json options, docs/config.json, and theming
---

# Configuration

Bark splits configuration into two files:

- **`appsettings.json`**: host-level concerns. Where the docs folder lives, whether hot reload is on, theme colors. Set per deployment, and applied on restart.
- **`docs/config.json`**: content-level concerns. Site title and metadata, brand text, navigation, footer, social links. Set per project and hot-reloaded alongside your Markdown, so no restart is needed.

That split means a content editor never needs deploy access just to fix a typo in the brand name.

This page covers what you will touch first. For the field-by-field list, see [Site Config](/reference/site-config). If you run Bark in a container, [Environment Variables](/guide/environment-variables) gives the equivalent variable names.

## `appsettings.json`

These settings belong to the `Docs` section:

| Setting | Default | Description |
|---|---|---|
| `RootPath` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `index` | Page served at `/`. |
| `EnableHotReload` | `true` | Watch `*.md` and `config.json` for changes and rebuild in the background. |
| `BasePath` | `null` | Prefix for every internal link and asset URL. Set this when Bark is served from a subpath instead of the domain root. |

```json
{
  "Docs": {
    "RootPath": "../../docs",
    "DefaultPage": "index",
    "EnableHotReload": true,
    "BasePath": "/your-repo"
  }
}
```

::: tip
`BasePath` matters most for [static export](/guide/deploy#option-e-static-export-github-pages-etc), where the `--base-path` CLI flag usually replaces it entirely. Set it here instead when the live server sits behind a reverse proxy that mounts Bark under a subpath.
:::

Colors, fonts, and your own CSS or JS are a separate concern, covered in [Themes](/guide/themes).

## `docs/config.json`

This file covers your site's title, HTML metadata, navigation, footer, and social links. Navigation has the most moving parts, so it gets the space below. For everything else, see [Site Config](/reference/site-config).

Navigation has three levels of control, and they mix:

1. **Do nothing.** With no `nav`, `sidebar`, or `topNav`, Bark builds the left sidebar from your folder structure.
2. **One flat sidebar** (`nav`) for the whole site. Good for small doc sets that need no header bar.
3. **A header nav with dropdowns** (`topNav`) plus **a different sidebar per section** (`sidebar`, keyed by path prefix). This is what most multi-section sites want.

```json
{
  "brand": "Bark",
  "topNav": [
    { "text": "Home", "link": "/" },
    { "text": "Guide", "link": "/guide/getting-started" },
    { "text": "Reference", "link": "/reference/site-config" },
    {
      "text": "More",
      "items": [
        { "text": "GitHub", "link": "https://github.com/melosso/bark" },
        { "text": "Releases", "link": "https://github.com/melosso/bark/releases" }
      ]
    }
  ],
  "sidebar": {
    "/guide/": [
      {
        "title": "Introduction",
        "items": [
          { "title": "Getting Started", "path": "guide/getting-started" },
          { "title": "Configuration", "path": "guide/configuration" },
          { "title": "Routing", "path": "guide/routing" },
          { "title": "Deploy", "path": "guide/deploy" }
        ]
      }
    ],
    "/reference/": [
      {
        "title": "Reference",
        "items": [
          { "title": "Site Config", "path": "reference/site-config" },
          { "title": "API Reference", "path": "reference/api-reference" },
          { "title": "Sitemap & Crawlers", "path": "reference/sitemap-generation" }
        ]
      }
    ]
  },
  "socialLinks": [
    { "icon": "github", "url": "https://github.com/melosso/bark", "title": "GitHub" }
  ]
}
```

A `topNav` item is either a direct link (`text` plus `link`) or a dropdown (`text` plus `items`, no `link`). Those are the only two shapes.

`sidebar` keys are path prefixes, and the longest match for the current page wins. That lets `/guide/` and `/guide/advanced/` coexist, with the more specific key taking over for pages beneath it.

::: tip
When `sidebar` is present it takes priority over `nav` for any page matching one of its prefixes. `nav`, when present at all, fully replaces the auto-generated folder navigation on every page. Neither one merges with the folder tree. Leave both out to let Bark build navigation from your folders.
:::

`footer` is rendered as Markdown, so links and formatting work as expected. For social links, an `icon` of `"github"` or `"mastodon"` renders as an inline SVG; any other value renders as plain text.

## `docs/locale/`

Every piece of text Bark puts on the page itself, the search modal, the table of contents heading, the pager labels, the 404 page, reads from a string table. English is built in. To translate it, point `locale` at a language code and drop a file next to the others:

```json
{
  "locale": "nl"
}
```

That reads `docs/locale/nl.json`. The object form works too, if you prefer it: `"locale": { "code": "nl" }`. Leave `locale` out and Bark falls back to `lang`, then to English.

To start a translation, copy `docs/locale/en.json`, which lists every key Bark knows about, and translate the values:

```json
{
  "searchPlaceholder": "Documentatie doorzoeken...",
  "tocTitle": "Op Deze Pagina",
  "pagerNext": "Volgende"
}
```

A few rules make this safe to do halfway:

- Any key you leave out keeps its English text. A partial file is fine and renders as a mix.
- Placeholders like `{0}` carry a value from the page. Keep them exactly as they appear in the English source.
- Keys Bark does not know are ignored, and it logs a warning naming them, which is usually a typo.
- A file that is not valid JSON is skipped whole, with a warning, and the interface stays English.

Locale files hot reload like any other content. Save one and the running site picks it up, no restart. They are never served to a browser or listed by the API; the only thing that reads them is the string table.

Bark ships `en.json` and `nl.json`. One language is active at a time, for the whole site.

## Translated pages

The string table covers the interface. To translate the pages themselves, give each language a directory and name it in `config.json`:

```json
{
  "locales": {
    "root": { "label": "English", "lang": "en" },
    "nl": { "label": "Nederlands", "lang": "nl" }
  }
}
```

`root` names the untranslated tree, the one that already lives at the top of `docs/`. Every other key is both a directory name and a locale code, so `docs/nl/guide/install.md` serves at `/nl/guide/install/` and reads its interface strings from `docs/locale/nl.json`.

```
docs/
  index.md                  → /
  guide/install.md          → /guide/install/
  nl/
    index.md                → /nl/
    guide/install.md        → /nl/guide/install/
```

Each tree gets its own navigation, its own search index, and its own `<html lang>`. A language switcher appears in the header as soon as a second language is configured. Because `sidebar` keys are path prefixes, a `/nl/` key gives that tree its own sidebar with no extra machinery.

A page with `layout: home` never shows a translation notice. A landing page is a shop window, not a document, and a banner across the top of it reads as a fault rather than a courtesy.

You do not have to translate everything before you ship. A page that exists in English but not yet in Dutch is served at its Dutch URL, in the Dutch interface, with a notice above the content linking to the English original. Its `canonical` points at the original, and it stays out of the Dutch search index, so an untranslated page never competes with the page it came from.

Translations also go stale. When the original changes after the translation was written, Bark compares the two timestamps and shows a quieter notice inviting a comparison. Nothing is hidden and nothing is silently wrong.

::: tip
One language is active per request, decided by the URL. Bark never redirects readers based on their browser language: that would fight shared caches, static export, and the reader's own choice of link.
:::

### Drafting a translation with LibreTranslate

Bark can draft a translated tree for you from a self-hosted [LibreTranslate](https://libretranslate.com/) instance. It is a one-off generator, not a runtime service: it writes real Markdown files that you then own, review, and edit.

```bash
dotnet run --project src/Bark -- --translate nl --translate-endpoint http://localhost:5000
```

Flags:

| Flag | Meaning |
|---|---|
| `--translate <code>` | Target locale, and the directory it writes into (`docs/nl/`) |
| `--translate-endpoint <url>` | LibreTranslate base URL, default `http://localhost:5000` |
| `--translate-from <code>` | Source language, defaults to the site's own `locale` |
| `--translate-api-key <key>` | Sent as `api_key` when your instance requires one |
| `--translate-overwrite` | Retranslate files that already exist, which is off by default so your edits survive |

What it leaves alone: fenced and indented code, inline code spans, link and image targets, HTML blocks, container markers, and every front matter field except `title` and `description`. Headings keep an explicit `{#anchor}` matching the original slug, so links into a translated page still land in the right place.

Every generated file carries `machineTranslated: true` in its front matter, which renders a notice inviting corrections. Remove the flag once a person has read the page.

::: warning
Machine translation is a starting point, not a finished page. Review before you announce a language, and treat the flag as a to-do list rather than a badge.
:::

### Translating the menus

`config.json` holds text too: menu labels, sidebar titles, the brand, the footer, the promo bar, the edit-link label. That text stays in `config.json`, written once, in one language. The translations live in the locale file, as a dictionary keyed by the original text:

```json
{
  "tocTitle": "Op Deze Pagina",
  "config": {
    "Getting Started": "Aan de slag",
    "Configuration": "Configuratie",
    "Edit this page on GitHub": "Bewerk deze pagina op GitHub",
    "Built with Bark.": "Gebouwd met Bark."
  }
}
```

The structure of your navigation is never duplicated. Add a page to the sidebar and every language picks it up; only its label needs a line in each `config` block. Anything absent from the dictionary renders in the original language, so a half-translated menu is a mix, not a gap.

Markdown values work the same way. Use the exact source string as the key, links and all, and the translation is rendered as Markdown just like the original:

```json
"config": {
  "Built with Bark · [EUPL-1.2](/LICENSE).": "Gebouwd met Bark · [EUPL-1.2](/LICENSE)."
}
```

`config` is a reserved key inside a locale file. Every other key is an interface string, and Bark warns about names it does not recognise.

### Drafting a translation with LibreTranslate

Bark can draft a translated tree for you from a self-hosted [LibreTranslate](https://libretranslate.com/) instance. It is a one-off generator, not a runtime service: it writes real Markdown files that you then own, review, and edit.

```bash
dotnet run --project src/Bark -- --translate nl --translate-endpoint http://localhost:5000
```

Flags:

| Flag | Meaning |
|---|---|
| `--translate <code>` | Target locale, and the directory it writes into (`docs/nl/`) |
| `--translate-endpoint <url>` | LibreTranslate base URL, default `http://localhost:5000` |
| `--translate-from <code>` | Source language, defaults to the site's own `locale` |
| `--translate-api-key <key>` | Sent as `api_key` when your instance requires one |
| `--translate-overwrite` | Retranslate files that already exist, which is off by default so your edits survive |

What it leaves alone: fenced and indented code, inline code spans, link and image targets, HTML blocks, container markers, and every front matter field except `title` and `description`. Headings keep an explicit `{#anchor}` matching the original slug, so links into a translated page still land in the right place.

Every generated file carries `machineTranslated: true` in its front matter, which renders a notice inviting corrections. Remove the flag once a person has read the page.

::: warning
Machine translation is a starting point, not a finished page. Review before you announce a language, and treat the flag as a to-do list rather than a badge.
:::

### Translating the menus

`config.json` holds text too: menu labels, sidebar titles, the brand, the footer. A locale entry can override `topNav`, `nav`, `sidebar`, `brand`, `footer` and `promo`, and anything it leaves out falls back to the top-level value.

```json
{
  "brand": "Docs",
  "topNav": [{ "text": "Guide", "link": "/guide/install" }],
  "sidebar": {
    "/guide/": [
      { "title": "Guide", "items": [{ "title": "Install", "path": "guide/install" }] }
    ]
  },
  "locales": {
    "root": { "label": "English", "lang": "en" },
    "nl": {
      "label": "Nederlands",
      "lang": "nl",
      "brand": "Documentatie",
      "footer": "Geschreven in het Nederlands.",
      "topNav": [{ "text": "Handleiding", "link": "/guide/install" }],
      "sidebar": {
        "/guide/": [
          { "title": "Handleiding", "items": [{ "title": "Installeren", "path": "guide/install" }] }
        ]
      }
    }
  }
}
```

Write the paths exactly as you write them in the root config, without the locale prefix. Bark adds it, so `guide/install` becomes `/nl/guide/install/` for a reader in the Dutch tree, and sidebar keys like `/guide/` keep matching. External links are never rewritten.

A locale that overrides nothing inherits every menu from the top level, in the original language. That is the right default for a site whose navigation is mostly product names.
