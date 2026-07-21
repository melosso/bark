---
title: Extensions
description: Enabling privacy-friendly analytics like Matomo, Plausible, Medama, and GoatCounter through docs/extensions.json
---

# Extensions

Extensions are small, built-in integrations that Bark wires up for you. You describe the one you want, and Bark injects its script, keeps your Content Security Policy in step, and reloads on save. The current family covers privacy-friendly analytics, so you can measure your traffic while respecting your readers.

## What's Supported

The following extensions are supported:

| Extension | Type | Required keys |
| --- | --- | --- |
| [Matomo](#matomo) | Analytics, self-hosted | `url`, `siteId` |
| [Plausible](#plausible) | Analytics, hosted or self-hosted | `domain` |
| [Medama](#medama) | Analytics, self-hosted | `url` |
| [GoatCounter](#goatcounter) | Analytics, hosted or self-hosted | `url` |
| [Liwan](#liwan) | Analytics, self-hosted | `url`, `entity` |

## How Extensions Work

Extensions live in one optional file, `extensions.json`, next to your `config.json` in the `docs/` folder. Keeping them separate leaves your main config focused on content and navigation.

```json
{
  "extensions": {
    "plausible": {
      "enabled": true,
      "domain": "docs.example.com"
    }
  }
}
```

Every extension stays off until you set `enabled` to `true`. An empty or absent file simply means no analytics run. Because Bark reads this file in memory alongside your Markdown, a saved change is picked up right away through hot reload. There is no build step to wait on.

Bark also verifies each enabled extension before it goes live. If a setting looks off, that extension is left inactive and a warning is written to your startup log. This way a small typo cannot push a broken tracker to your visitors.

::: info
Analytics scripts talk to an outside server, so Bark widens your Content Security Policy to allow the origin you configure. Each injected script also carries the page's security nonce. You are welcome to keep a strict CSP: the extension you enable is added to it for you.
:::

## Available Extensions

You can pick whichever tool fits your workflow. Running more than one at a time is fine if you are comparing them.

### Matomo

[Matomo](https://matomo.org) is a self-hosted analytics platform for teams that want to own their data. Bark configures it in cookieless mode by default, which keeps you clear of consent prompts. You can turn that off if your setup calls for it.

```json
{
  "extensions": {
    "matomo": {
      "enabled": true,
      "url": "https://analytics.example.com",
      "siteId": "1",
      "disableCookies": true
    }
  }
}
```

| Field | Description |
|---|---|
| `enabled` | Set to `true` to activate the extension. |
| `url` | The base URL of your Matomo install. |
| `siteId` | The numeric site id Matomo assigned. The `site_id` spelling works too. |
| `disableCookies` | Cookieless tracking, on by default. Set to `false` for standard cookies. |

### Plausible

[Plausible](https://plausible.io) is a lightweight, cookie-free service. It comes hosted or self-hosted, and needs no consent banner.

```json
{
  "extensions": {
    "plausible": {
      "enabled": true,
      "domain": "docs.example.com",
      "url": "https://plausible.io",
      "script": "script.js"
    }
  }
}
```

| Field | Description |
|---|---|
| `enabled` | Set to `true` to activate the extension. |
| `domain` | The site domain registered in Plausible. Comma-separate several for a shared script. |
| `url` | The base URL of your install. Defaults to `https://plausible.io`, so you only need it when self-hosting. |
| `script` | The script variant under `/js/`, such as `script.outbound-links.js`. Defaults to `script.js`. |

### Medama

[Medama](https://github.com/medama-io/medama) is a self-hosted, privacy-first server with a light footprint. It needs only the address it lives at.

```json
{
  "extensions": {
    "medama": {
      "enabled": true,
      "url": "https://medama.example.com"
    }
  }
}
```

| Field | Description |
|---|---|
| `enabled` | Set to `true` to activate the extension. |
| `url` | The base URL of your Medama install. |

### GoatCounter

[GoatCounter](https://www.goatcounter.com) is an easygoing option, offered as a free hosted service or self-hosted. It suits personal sites and smaller projects.

```json
{
  "extensions": {
    "goatcounter": {
      "enabled": true,
      "url": "https://you.goatcounter.com"
    }
  }
}
```

| Field | Description |
|---|---|
| `enabled` | Set to `true` to activate the extension. |
| `url` | The base URL of your GoatCounter site. |

### Liwan

[Liwan](https://liwan.dev) is a self-hosted, privacy-friendly analytics server written in Rust. It is small and stores its data in a single file. Point it at your instance and name the entity you track.

```json
{
  "extensions": {
    "liwan": {
      "enabled": true,
      "url": "https://liwan.example.com",
      "entity": "my-website"
    }
  }
}
```

| Field | Description |
|---|---|
| `enabled` | Set to `true` to activate the extension. |
| `url` | The base URL of your Liwan instance. |
| `entity` | The entity id you configured in Liwan for this site. |

## A Complete Example

Here is an `extensions.json` listing all four providers, each switched off. Keep it as a template and flip `enabled` to `true` on the one you choose.

```json
{
  "extensions": {
    "matomo": {
      "enabled": false,
      "url": "https://analytics.example.com",
      "siteId": "1",
      "disableCookies": true
    },
    "plausible": {
      "enabled": false,
      "domain": "example.com",
      "url": "https://plausible.io",
      "script": "script.js"
    },
    "medama": {
      "enabled": false,
      "url": "https://medama.example.com"
    },
    "goatcounter": {
      "enabled": false,
      "url": "https://you.goatcounter.com"
    },
    "liwan": {
      "enabled": false,
      "url": "https://liwan.example.com",
      "entity": "my-website"
    }
  }
}
```

Save the file with an extension enabled, then reload a page and view its source. The analytics script sits in the `<head>`, carrying the page nonce. From here, the [Site Config](/reference/site-config) reference shows how `extensions.json` sits alongside the rest of your setup.
