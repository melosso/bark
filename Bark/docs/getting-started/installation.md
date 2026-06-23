---
title: Installation
description: Get Bark running locally in under a minute
---

# Installation

Let's be real: if "getting started" takes more than two commands, half your team bounces before they write a single page. Bark's install is deliberately boring.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- If hosting on Windows behind IIS, make sure IIS is installed — Bark itself doesn't need it for local dev.

## Run it

```bash
cd Bark
dotnet restore
dotnet watch --project src/Bark
```

Open `http://localhost:5000`. That's the whole onboarding flow.

`dotnet watch` gives you hot reload on the C# side too — but you mostly won't need it, because Bark already watches `docs/**/*.md` and `docs/bark.json` on its own `FileSystemWatcher` and rebuilds in the background (changes are debounced ~300ms so a flurry of editor saves doesn't trigger a rebuild storm).

For a plain run without the C# watcher:

```bash
dotnet run --project src/Bark
```

### What I Learned:

> **The 80% Truth:** If your "quick start" needs a wiki page to explain it, it's not quick.

## Where docs live

Markdown files go in `docs/` at the repo root (configurable — see [Configuration](configuration)). The folder tree becomes the navigation tree; `index.md` in any folder becomes that folder's landing page instead of a separate `/folder/index` URL.

```
docs/
├── bark.json              ← optional, see Configuration
├── index.md                ← served at /
├── getting-started/
│   ├── installation.md     ← served at /getting-started/installation
│   └── configuration.md
└── api/
    └── reference.md
```

Front matter (`title`, `description`) is optional. Bark falls back to the filename when it's missing:

```markdown
---
title: Installation
description: Get Bark running locally
---

# Installation
...
```

Next: [Configuration](configuration) — brand text, nav overrides, theming, social links.
