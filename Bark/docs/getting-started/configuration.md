---
title: Configuration
---

# Configuration

Bark is configured via `appsettings.json` or environment variables.

## DocsOptions

| Setting | Default | Description |
|---------|---------|-------------|
| `RootPath` | `docs` | Path to the markdown files directory |
| `DefaultPage` | `index` | Default page when visiting `/` |
| `EnableHotReload` | `true` | Watch for file changes and rebuild |

## Example

```json
{
  "Docs": {
    "RootPath": "docs",
    "DefaultPage": "index",
    "EnableHotReload": true
  }
}
```
