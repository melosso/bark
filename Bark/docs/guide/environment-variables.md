---
title: Environment Variables
description: Configuring Bark via environment variables for Docker and container deployments
---

# Environment Variables

When you are running Bark inside a Docker container or deploying through an orchestration platform, environment variables are often the most convenient way to pass configuration in. ASP.NET Core's built-in configuration system picks them up automatically, so no code changes or extra config files are needed on your end.

Every setting you would normally write in `appsettings.json` can be provided this way. For the full list of available keys, see [Site Config](/reference/site-config).

## How the mapping works

ASP.NET Core translates environment variable names into configuration keys by replacing double underscores (`__`) with the colon (`:`) used to separate nested sections. This keeps the naming consistent with what you would write directly in `appsettings.json`.

For example, `Docs:RootPath` becomes `Docs__RootPath`, and `Docs:Themes:PrimaryColor` becomes `Docs__Themes__PrimaryColor`.

## Public base URL

Set `PublicBaseUrl` to the origin your site is actually served from:

```bash
PublicBaseUrl=https://docs.example.com
```

Bark uses it to build the absolute URLs in `robots.txt`, `llms.txt`, the RSS feed, and the `canonical` and `og:url` tags. Left unset, those URLs are built from the incoming request's `Host` header, which the caller controls. That is fine locally, but on a public host anyone can request your `robots.txt` with a forged `Host` and get a `Sitemap:` line pointing somewhere else. If a CDN caches that response, the forged copy is what your visitors and crawlers get.

Combine it with the standard `AllowedHosts` variable so requests for other hostnames are rejected outright rather than merely ignored:

```bash
AllowedHosts=docs.example.com
```

`Docs__PublicBaseUrl` works too, if you prefer keeping it alongside the other `Docs__` settings. Exporting a static site with `--base-url` sets the same value, so you only need this when running Bark as a server.

## Server URL and port

Kestrel's listening address is controlled by the standard `ASPNETCORE_URLS` variable:

```bash
ASPNETCORE_URLS=http://+:8080
```

You can specify multiple addresses separated by a semicolon if your deployment needs to listen on more than one port:

```bash
ASPNETCORE_URLS=http://+:8080;https://+:8443
```

## Examples

Docker is a common deployment target, so the examples below show both a `docker run` one-liner and a Compose file. The same variables work in any environment that supports standard process environment variables.

**`docker run`:**

```bash
docker run \
  -e Docs__RootPath=/docs \
  -e Docs__BasePath=/my-repo \
  -e PublicBaseUrl=https://docs.example.com \
  -e AllowedHosts=docs.example.com \
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
      PublicBaseUrl: https://docs.example.com
      Docs__EnableHotReload: "false"
      AllowedHosts: docs.example.com
    volumes:
      - ./docs:/docs
```
