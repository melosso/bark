---
title: Deploy
description: Docker, Windows/IIS, Linux release, or build from source
---

# Deploy

Pick whichever path fits your environment. Docker is the fastest.

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

Your `docs/` folder is copied into the publish output automatically. It's wired up as `Content` in `Bark.csproj`, not a compiled resource, so editing Markdown post-publish still works without a rebuild.

```bash
cd publish
dotnet Bark.dll
```

You need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed to publish. The published output still needs the .NET runtime on the target machine unless you add `--self-contained true -r <rid>`.

If you're actively developing Bark's own source rather than just running it, `dotnet watch --project src/Bark` from a clone gives you C#-side hot reload too. See the [CLI reference](../reference/cli) for the full command list.

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

Bark holds the entire rendered page set and the search index in memory. For a docs set in the hundreds-of-pages range, that's a non-issue on essentially any VM or container. If you're hosting tens of thousands of pages, you've outgrown the assumptions this tool was built around. Bark stays narrow on purpose, and "scale to an arbitrary number of pages" isn't a design goal.

The next logical step to visit is [Site Config](../reference/site-config), for the full `appsettings.json` and `config.json` reference.
