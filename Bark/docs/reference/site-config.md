---
title: Site Config
description: Full reference for appsettings.json and docs/config.json
---

# Site Config

Bark draws from two separate configuration files, and understanding why helps you decide where a setting belongs.

1. **Environment settings.** Found in  `appsettings.json` or your [.env](/getting-started/environment-variables) file is for deployment-level concerns: where your Markdown files live, whether hot-reload is on, and what base path the server sits at. 
2. **Site configuration.** Found in `docs/config.json` lives alongside your content and controls everything your readers see, from the site name and navigation structure to the footer and social links.

That distinction means you can check `config.json` into the same repository as your docs and deploy the server without touching it.

If you are looking for a narrative walkthrough of these settings rather than a field-by-field reference, [Configuration](/getting-started/configuration) walks through the most common setups step by step.

## `appsettings.json`: `Docs`

These settings apply at the host level and take effect at startup, so a restart is needed after changing them. If your deployment runs Bark in a container or through an orchestration platform, every field here can be provided as an environment variable instead. See [Environment Variables](/getting-started/environment-variables) for how the naming convention works.

| Option | Type | Default | Description |
|---|---|---|---|
| `RootPath` | `string` | `docs` | Path to the Markdown files directory, relative to the app's working directory. |
| `DefaultPage` | `string` | `index` | Page served at `/`. |
| `EnableHotReload` | `bool` | `true` | Watch `*.md` and `config.json` for changes and rebuild in the background. It is recommended to disable this in production if you publish content as part of your deploy rather than editing files on the running server. |
| `BasePath` | `string?` | `null` | Prefix every internal link, theme asset URL, and API call with this path segment. This is the setting to reach for when Bark is not served from the domain root, for example a GitHub Pages project page at `you.github.io/your-repo/` or a reverse proxy mounting Bark under `/docs`. A CLI `--base-path` flag overrides this value at runtime, which is how [static export](/getting-started/deploy#option-e-static-export-github-pages-etc) adjusts it without requiring a config edit. |
| `ContentSecurityPolicy` | `string?` | `null` | A custom `Content-Security-Policy` header value. When provided, this replaces Bark's built-in default entirely rather than extending it. It is recommended to leave this unset unless you have a specific reason to override the default policy, such as allowing an external font host. Bark's default policy disallows inline scripts and styles that do not carry its per-request nonce, restricts every fetch directive to `'self'`, and disables framing. |

## `appsettings.json`: `Docs:Themes`

**About Theming**

There are two ways to apply your own theme, and they complement each other nicely. Dropping files into `wwwroot/theme/` is the quickest path: a `custom.css` file there is loaded after Bark's built-in styles, a `custom.js` file is injected before the closing `</body>`, and a `theme.json` file accepts CSS variable overrides in a structured format. These are picked up at startup without any configuration changes, which works well when you can edit the filesystem directly.

If you would rather track theme settings in version control alongside the rest of your infrastructure, the `Docs:Themes` section of `appsettings.json` offers the same options. When `Docs:Themes` is present it takes full priority over `theme.json`, so the two are not merged.

See [Extending Themes](/getting-started/extending-themes) for a practical walkthrough of both approaches.

**Configuration**

Every field here is optional. Each one is either a CSS variable override or a theme feature toggle, and anything you leave unset simply falls back to Bark's default palette and behavior. The same fields, written in camelCase, are accepted in `wwwroot/theme/theme.json` if you prefer the filesystem approach.

| Option | Type | Maps to | Description |
|---|---|---|---|
| `PrimaryColor` | `string` | `--primary-color` | Accent color used for links and highlights. |
| `BgColor` | `string` | `--bg-color` | Page background. |
| `SidebarBg` | `string` | `--sidebar-bg` | Sidebar background, which can differ from the page background. |
| `TextColor` | `string` | `--text-color` | Primary text color. |
| `TextMuted` | `string` | `--text-muted` | Secondary text: descriptions, timestamps, muted labels. |
| `BorderColor` | `string` | `--border` | Hairline borders throughout the layout. |
| `CodeBg` | `string` | `--code-bg` | Background for inline code and fenced code blocks. |
| `AccentLight` | `string` | `--accent-light` | Light tint of the accent color, used for active and highlighted states. |
| `FontSans` | `string` | `--font-sans` | Body font stack. |
| `FontMono` | `string` | `--font-mono` | Code font stack. |
| `CustomCssUrl` | `string` | n/a | Injects an extra `<link rel="stylesheet">`, loaded after Bark's built-in styles. Takes priority over an auto-detected `wwwroot/theme/custom.css` if both are present. |
| `BrandText` | `string` | n/a | Sidebar brand label. `config.json`'s `brand` field takes priority if set; if `brand` is absent, `config.json`'s `title` is used as a fallback. |
| `DarkMode` | `bool` | n/a | Enables the `prefers-color-scheme: dark` variant and the in-page dark mode switch. Defaults to `true`. |
| `ShowScrollIndicator` | `bool` | n/a | Shows the thin scroll-progress bar pinned to the top of the viewport. Defaults to `true`. |

## `docs/config.json`: site metadata

These settings shape how your site presents itself to readers, search engines, and social platforms. For a deeper look at the HTML head fields specifically, see [HTML Metadata](/reference/site-metadata).

| Option | Type | Description |
|---|---|---|
| `title` | `string?` | Site name. Appended to every page title as `Page Title \| Site Name`. See [HTML Metadata](/reference/site-metadata). |
| `titleTemplate` | `string?` | Custom title pattern using `:title` and `:siteName` as placeholders. For example, `":title · :siteName"` produces `Getting Started · Bark`. Overrides the default suffix format when set. |
| `description` | `string?` | Site-wide fallback `<meta name="description">`. Per-page frontmatter descriptions take priority over this value. |
| `lang` | `string?` | `lang` attribute on `<html>`. Defaults to `"en"`. |
| `head` | `HeadTag[]?` | Extra tags injected into `<head>` on every page. Useful for structured data, supplementary Open Graph fields like `og:image`, or third-party initialization snippets. Bark generates canonical links and basic Open Graph tags automatically, so those do not need to be listed here. |
| `brand` | `string?` | Sidebar and header brand label. Falls back to `title` if unset, then to `Docs:Themes:BrandText`. |
| `brandImage` | `string?` | An image URL or path to display alongside the brand label in the header, placed to the left of the text. |
| `footer` | `string?` | Rendered as Markdown inside the page footer. Links and inline formatting are fully supported. See [Footer](../default-theme-footer). |
| `favicon` | `string?` | A URL or path to an icon file, or a single emoji character to use as an inline SVG favicon. |
| `lastUpdated` | `bool` | Site-wide toggle for the "Last updated" timestamp. Off by default. When enabled, the date shown for each page comes from the file's last-modified time on disk unless the page sets `date` or `updated` in its frontmatter, which takes priority. See [Last Updated Timestamp](../default-theme-last-updated) and [Frontmatter Config](/reference/frontmatter-config#dates). |
| `editLink` | `EditLinkConfig?` | "Edit this page" link displayed near the pagination footer. See [Edit Link](../default-theme-edit-link). |
| `pageControls` | `PageControlsConfig?` | Per-page action menu shown in the breadcrumb bar. When configured, a small button appears that opens a dropdown with actions like downloading the page's Markdown source or opening it in an external editor. |

**`HeadTag`**

| Field | Type | Description |
|---|---|---|
| `tag` | `string` | HTML tag name, for example `"meta"`, `"link"`, or `"script"`. |
| `attrs` | `Record<string, string>?` | Attribute key-value pairs. Values are HTML-encoded automatically. |
| `content` | `string?` | Inner content for non-void tags. For `<script>` and `<style>` the value is written as-is; for any other tag it is HTML-encoded. Void elements (`<meta>`, `<link>`, `<base>`) do not use this field. |

## `docs/config.json`: navigation

There are three navigation building blocks you can combine depending on the shape of your documentation. `topNav` populates the horizontal bar at the top of the page, which works well for top-level sections or external links. `sidebar` lets you define a different sidebar for each section by mapping path prefixes to nav trees; the longest matching prefix wins. `nav` is a simpler alternative that shows the same sidebar on every page, which suits smaller sites that do not need path-scoped sidebars. When both `sidebar` and `nav` are present, `sidebar` takes priority for any page whose path it matches.

| Option | Type | Description |
|---|---|---|
| `topNav` | `TopNavItem[]?` | Header nav bar. See [Nav](../default-theme-nav). |
| `sidebar` | `Record<string, NavEntry[]>?` | Path-prefix-keyed sidebars. The longest matching prefix for the current page wins; an empty-prefix key (`"/"`) acts as a catch-all. Takes priority over `nav`. See [Sidebar](../default-theme-sidebar). |
| `nav` | `NavEntry[]?` | Single flat sidebar, shared by every page. Ignored for any page that matches a `sidebar` prefix. |

**`TopNavItem`**

| Field | Type | Description |
|---|---|---|
| `text` | `string` | Label shown in the nav bar. |
| `link` | `string?` | Direct link. Omit this field to make the item a dropdown instead. |
| `items` | `TopNavItem[]?` | Dropdown children. Omit `link` when using this field. |

A `TopNavItem` is either a direct link (`text` + `link`) or a dropdown (`text` + `items`), but not both at the same time.

**`NavEntry`**

| Field | Type | Description |
|---|---|---|
| `title` | `string` | Link text or group heading. |
| `path` | `string?` | Leaf link target. Omit this to make the entry a group. |
| `items` | `NavEntry[]?` | Child entries. Set this (and omit `path`) to create a group. Groups nest to any depth. |
| `collapsed` | `bool?` | Group-only. Omitting this field means the group is not collapsible. `false` means collapsible and starts expanded. `true` means collapsible and starts collapsed. |

**`EditLinkConfig`**

| Field | Type | Description |
|---|---|---|
| `pattern` | `string` | URL template with a `:path` placeholder, which Bark replaces with the file's path relative to your docs root, preserving original casing. |
| `text` | `string` | Link label. Defaults to `"Edit this page"`. |

**`PageControlsConfig`**

| Field | Type | Description |
|---|---|---|
| `downloadMarkdown` | `bool` | When `true`, the menu includes a "Download markdown" link that serves the raw `.md` source for the current page via `GET /raw/{path}`. |
| `openInEditor` | `OpenInEditorConfig?` | When set, the menu includes a link that opens the current page's source file in an external editor using a deep-link URL. |

**`OpenInEditorConfig`**

| Field | Type | Description |
|---|---|---|
| `template` | `string` | URL template with a `{path}` placeholder, replaced by the page's `.md` path relative to the docs root. For example, `"vscode://file/{path}"` opens the file in VS Code, and `"vscodium://file/{path}"` opens it in VSCodium. |
| `label` | `string` | Menu item label. Defaults to `"Open in editor"`. |

**Full example**, matching the structure used throughout this documentation site:

```json
{
  "title": "Bark",
  "description": "A fast, lightweight Markdown documentation server built on .NET.",
  "lang": "en",
  "brand": "Bark",
  "brandImage": "/brand-image.svg",
  "footer": "Built with Bark · [AGPL-3.0](LICENSE)",
  "favicon": "🌳",
  "lastUpdated": true,
  "editLink": {
    "pattern": "https://github.com/melosso/bark/edit/main/docs/:path",
    "text": "Edit this page on GitHub"
  },
  "topNav": [
    { "text": "Home", "link": "/" },
    { "text": "Guide", "link": "/getting-started/getting-started" },
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
    "/getting-started/": [
      {
        "title": "Introduction",
        "collapsed": false,
        "items": [
          { "title": "Getting Started", "path": "getting-started/getting-started" },
          { "title": "Configuration", "path": "getting-started/configuration" }
        ]
      }
    ],
    "/reference/": [
      {
        "title": "Reference",
        "items": [
          { "title": "Site Config", "path": "reference/site-config" },
          {
            "title": "Customisation",
            "items": [
              { "title": "Nav", "path": "reference/default-theme-nav" },
              { "title": "Sidebar", "path": "reference/default-theme-sidebar" }
            ]
          }
        ]
      }
    ]
  },
  "socialLinks": [
    { "icon": "github", "url": "https://github.com/melosso/bark", "title": "GitHub" },
    { "icon": "mastodon", "url": "https://fosstodon.org/@example", "title": "Mastodon" }
  ],
  "pageControls": {
    "downloadMarkdown": true,
    "openInEditor": {
      "template": "vscode://file/{path}",
      "label": "Open in VS Code"
    }
  }
}
```

## `docs/config.json`: social links

Social links appear in the top-right area of the header on desktop and fold into the mobile sidebar on smaller screens. Each entry needs at minimum an `icon` name and a `url`.

| Field | Type | Description |
|---|---|---|
| `icon` | `string` | `"github"` and `"mastodon"` render as inline SVGs. Any other value renders as plain text, which works well for short labels like `"npm"` or `"discord"`. |
| `url` | `string` | Link target. Opens in a new tab. |
| `title` | `string?` | Accessible label and tooltip text. Falls back to the `icon` value if omitted. |

## CLI flags

When launching Bark directly from the terminal rather than through Docker or a process manager, a small set of flags let you adjust runtime behavior without editing config files. These are most commonly used during the [static export](/getting-started/deploy#option-e-static-export-github-pages-etc) workflow, but they work just as well against the live server.

| Flag | Overrides | Description |
|---|---|---|
| `--export <dir>` | | Writes a static HTML export to the given directory and exits. Disables hot reload automatically. |
| `--base-url <origin>` | | The public origin used when building absolute URLs in `robots.txt` and `llms.txt`. |
| `--base-path </prefix>` | `Docs:BasePath` | Prefixes all links, theme assets, and API routes with this path segment. Particularly useful for GitHub Pages project sites or reverse proxies mounting Bark under a subpath. |

## What Bark does not configure

Bark keeps its configuration surface intentionally small. There are no build pipeline options, bundler settings, or lifecycle hooks, because Bark runs without a client-side build step. Features like math rendering, syntax highlighting, and custom containers are part of the core and are always active.

If you find yourself needing custom asset pipelines or structural layout changes, the recommended approach is to fork the source and modify it directly. Bark is designed to be a solid starting point you can adapt, not a platform you configure into a different shape through files alone.