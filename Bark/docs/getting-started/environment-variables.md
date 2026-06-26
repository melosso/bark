---
title: Environment Variables
description: Configuring Bark via environment variables for Docker and container deployments
---

# Environment Variables

If you are running Bark inside a Docker container or deploying it through an orchestration platform, environment variables are often the most convenient way to pass configuration in. ASP.NET Core's built-in configuration system picks them up automatically, so no code changes are needed.

## How the Mapping Works

ASP.NET Core translates environment variables into configuration keys by replacing double underscores (`__`) with the colon (`:`) used to separate nested sections. This means any setting you would normally write in `appsettings.json` can be provided as an environment variable instead.

For example, `Docs:RootPath` becomes `Docs__RootPath`, and `Docs:Themes:PrimaryColor` becomes `Docs__Themes__PrimaryColor`.

## `Docs` Settings

| Environment variable | Equivalent `appsettings.json` key | Default |
|---|---|---|
| `Docs__RootPath` | `Docs:RootPath` | `docs` |
| `Docs__DefaultPage` | `Docs:DefaultPage` | `index` |
| `Docs__EnableHotReload` | `Docs:EnableHotReload` | `true` |
| `Docs__BasePath` | `Docs:BasePath` | _(none)_ |

## `Docs:Themes` Settings

| Environment variable | Equivalent `appsettings.json` key |
|---|---|
| `Docs__Themes__BrandText` | `Docs:Themes:BrandText` |
| `Docs__Themes__PrimaryColor` | `Docs:Themes:PrimaryColor` |
| `Docs__Themes__BgColor` | `Docs:Themes:BgColor` |
| `Docs__Themes__SidebarBg` | `Docs:Themes:SidebarBg` |
| `Docs__Themes__TextColor` | `Docs:Themes:TextColor` |
| `Docs__Themes__TextMuted` | `Docs:Themes:TextMuted` |
| `Docs__Themes__BorderColor` | `Docs:Themes:BorderColor` |
| `Docs__Themes__CodeBg` | `Docs:Themes:CodeBg` |
| `Docs__Themes__AccentLight` | `Docs:Themes:AccentLight` |
| `Docs__Themes__FontSans` | `Docs:Themes:FontSans` |
| `Docs__Themes__FontMono` | `Docs:Themes:FontMono` |
| `Docs__Themes__CustomCssUrl` | `Docs:Themes:CustomCssUrl` |
| `Docs__Themes__DarkMode` | `Docs:Themes:DarkMode` |
| `Docs__Themes__ShowScrollIndicator` | `Docs:Themes:ShowScrollIndicator` |

## Server URL and Port

Kestrel's listening address is controlled by the standard `ASPNETCORE_URLS` variable:

```bash
ASPNETCORE_URLS=http://+:8080
```

You can specify multiple addresses separated by a semicolon:

```bash
ASPNETCORE_URLS=http://+:8080;https://+:8443
```

## Docker Examples

**`docker run`:**

```bash
docker run \
  -e Docs__RootPath=/docs \
  -e Docs__BasePath=/my-repo \
  -e ASPNETCORE_URLS=http://+:8080 \
  -v /path/to/your/docs:/docs \
  -p 8080:8080 \
  your-bark-image
```

**`docker-compose.yml`:**

```yaml
services:
  docs:
    build: .
    ports:
      - "8080:8080"
    environment:
      ASPNETCORE_URLS: http://+:8080
      Docs__RootPath: /docs
      Docs__BasePath: /my-repo
      Docs__EnableHotReload: "false"
    volumes:
      - ./docs:/docs
```

It is recommended to set `Docs__EnableHotReload` to `false` in production container deployments. Because your docs are typically baked into the image or mounted as a read-only volume at startup, the filesystem watcher has nothing meaningful to watch.
