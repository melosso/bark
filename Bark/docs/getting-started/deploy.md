---
title: Deploy
description: Docker, Windows/IIS, Linux release, or build from source
---

# Deploy

Pick whichever path fits your environment. Docker is the fastest way to setup your own instance (usually in less than a minute).

## Option A: Docker Compose

Prebuilt images are published to GHCR on every tagged release.

```yaml
services:
  bark:
    image: ghcr.io/melosso/bark:latest
    container_name: bark
    ports:
      - "8080:8080"
    volumes:
      - ./docs:/app/docs
```

Mount your own `docs/` folder (your `.md` files plus an optional `config.json`), then run:

```bash
docker compose up -d
```

Browse to `http://localhost:8080`.

## Option B: Windows / IIS

1. Download the latest `*-Windows_x64.zip` from [Releases](https://github.com/melosso/bark/releases).
2. Extract it to your site folder (for example `C:\inetpub\bark`).
3. In IIS, create a site (or app) pointing at that folder, with the **No Managed Code** .NET CLR version. Bark hosts itself via the ASP.NET Core Module, it doesn't need the CLR to load anything.
4. The zip already includes a `web.config` wired for in-process hosting. No manual edits needed.
5. Install the [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) on the server. This gives IIS the ASP.NET Core Module.
6. Start the site and browse to it.

## Option C: Linux release zip

A self-contained Linux x64 build (`*-Linux_x64.zip`) ships alongside every release if you'd rather run the binary directly without Docker.

> [!WARNING]
> This installation method is currently undocumented. Track [#1](https://github.com/melosso/bark/issues/1) for the write-up. Docker is the supported path on Linux today.

## Option D: Build from source

If you're contributing to Bark itself, or just don't want to pull a container image:

```bash
cd Bark
dotnet publish src/Bark -c Release -o ./publish
```

Your `docs/` folder is copied into the publish output automatically:

```bash
cd publish
dotnet Bark.dll
```

You need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed to publish. The published output still needs the .NET runtime on the target machine unless you add `--self-contained true -r <rid>`.

If you're actively developing Bark's own source rather than just running it, `dotnet watch --project src/Bark` from a clone gives you C#-side hot reload too.

## Option E: Static export (GitHub Pages, etc.)

Bark can crawl its own rendered routes and dump plain HTML/CSS/JS to a folder — no server needed at serve time:

```bash
dotnet publish src/Bark -c Release -o ./publish
cd publish && ./Bark --export ../site --base-url https://you.github.io --base-path /your-repo
```

> [!WARNING]
> Run the binary from inside the publish folder (`cd publish` first). The `docs/` lookup is relative to the current directory, not the executable's location — running it from elsewhere finds an empty/missing `docs` folder and exports a near-empty site.

- `--export <dir>`: writes every page as `<dir>/index.html` or `<dir>/<path>/index.html`, plus `404.html`, `robots.txt`, `llms.txt`, `sitemap.xml`, and a copy of `wwwroot`.
- `--base-url <origin>`: the real public origin, used to rewrite the absolute URLs inside `robots.txt`/`llms.txt`.
- `--base-path </prefix>`: only needed if the site won't be served from domain root (e.g. a GitHub *project* page like `you.github.io/your-repo/` — *user/org* pages at `you.github.io/` don't need this). Prefixes every nav/breadcrumb/pagination link, theme asset URL, and the search/hot-reload API calls so they resolve correctly under the subpath. Can also be set permanently via `Docs:BasePath` in `appsettings.json` for normal reverse-proxy subpath hosting.

> [!NOTE]
> The exported site has no backend: `/api/search` and `/api/build-version` don't exist anymore, so the search box and the live-reload banner silently do nothing. Everything else — nav, breadcrumbs, pagination, theming, dark mode — works identically to the live server.

A working GitHub Actions example lives in `.github/workflows/bark-deployment.yml`. It still needs **Settings → Pages → Source → GitHub Actions** set once per repo before the first deploy will succeed.

## What's already hardened for you

Production-minded defaults are baked into `Program.cs`, not bolted on with middleware you have to remember to add. These apply no matter which option above you picked, since they're compiled into the binary every option runs:

- **Response compression**: Brotli and Gzip, fastest level, enabled for HTTPS too.
- **Kestrel limits**: request body size, header size, max connections, HTTP/2 stream/frame tuning, keep-alive ping settings.
- **Structured logging**: Serilog to console, configured entirely through `appsettings.json`, no code changes needed to adjust log levels per environment.
- **ETag-based caching**: every page response carries a SHA-256 ETag. Clients sending a matching `If-None-Match` get a `304` instead of the full page.
- **Fail-fast port binding**: if a configured port is already in use, Bark logs a clear error and exits instead of letting Kestrel throw an opaque exception mid-startup.

> [!NOTE]  
> None of this requires configuration to get the benefit. It's the difference between "production-minded defaults" and "production-ready out of the box." You still own your deployment topology, but you're not starting from a bare `WebApplication.CreateBuilder()` either.

## Reverse proxy

Bark doesn't terminate TLS itself in most real deployments. That's a job for whatever sits in front of it. A minimal Nginx config in front of the Docker container (port 8080):

```nginx
server {
    listen 443 ssl;
    server_name docs.example.com;

    location / {
        proxy_pass         http://127.0.0.1:8080;
        proxy_set_header    Host $host;
        proxy_set_header    X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header    X-Forwarded-Proto $scheme;
    }
}
```

Building from source and running `dotnet Bark.dll` directly uses whatever port `ASPNETCORE_URLS` or your launch profile configures instead of 8080. Adjust `proxy_pass` to match.

If you're behind a reverse proxy, configure [Forwarded Headers Middleware](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) so Bark sees the real client scheme/host. `robots.txt` and `sitemap.xml` both build absolute URLs from the incoming request, so getting this right matters for SEO correctness, not just logging.

## Running as a service (source builds)

Docker and the IIS zip already manage their own process lifecycle. If you published from source and want it to survive a reboot, a basic `systemd` unit:

```ini
[Unit]
Description=Bark documentation server
After=network.target

[Service]
WorkingDirectory=/srv/bark/publish
ExecStart=/usr/bin/dotnet /srv/bark/publish/Bark.dll
Restart=always
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Hot reload (the `FileSystemWatcher` on `docs/`) keeps working in this setup. You don't need to restart the service to publish a content change, only to ship a code change.

## Sizing expectations

Bark holds the entire rendered page set and the search index *in memory*. For a documentation site in the hundreds-of-pages range, that's a non-issue on essentially any environment. 

But, if you're hosting tens of thousands of pages, you've outgrown the assumptions this tool was built around. 