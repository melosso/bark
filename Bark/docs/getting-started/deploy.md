---
title: Deploy
description: Running Bark in production, publishing, Kestrel hardening, and reverse proxies
---

# Deploy

Bark is a single ASP.NET Core process. Deploying it is deploying any other .NET 10 web app: no separate build pipeline, no static output directory to upload, no CDN step required to get started.

## Publish

```bash
cd Bark
dotnet publish src/Bark -c Release -o ./publish
```

This produces a self-contained-ish output folder (still needs the .NET runtime on the target machine unless you add `--self-contained true -r <rid>`). Your `docs/` folder is copied into the publish output automatically. It's wired up as `Content` in `Bark.csproj`, not a compiled resource, so editing Markdown post-publish still works without a rebuild.

```bash
cd publish
dotnet Bark.dll
```

## What's already hardened for you

Production-minded defaults are baked into `Program.cs`, not bolted on with middleware you have to remember to add:

- **Response compression**: Brotli and Gzip, fastest level, enabled for HTTPS too.
- **Kestrel limits**: request body size, header size, max connections, HTTP/2 stream/frame tuning, keep-alive ping settings.
- **Structured logging**: Serilog to console, configured entirely through `appsettings.json`, no code changes needed to adjust log levels per environment.
- **ETag-based caching**: every page response carries a SHA-256 ETag. Clients sending a matching `If-None-Match` get a `304` instead of the full page.
- **Fail-fast port binding**: if a configured port is already in use, Bark logs a clear error and exits instead of letting Kestrel throw an opaque exception mid-startup.

> [!NOTE]  
> None of this requires configuration to get the benefit. It's the difference between "production-minded defaults" and "production-ready out of the box." You still own your deployment topology, but you're not starting from a bare `WebApplication.CreateBuilder()` either.

## Reverse proxy

Bark doesn't terminate TLS itself in most real deployments. That's a job for whatever sits in front of it. A minimal Nginx config:

```nginx
server {
    listen 443 ssl;
    server_name docs.example.com;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_set_header    Host $host;
        proxy_set_header    X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header    X-Forwarded-Proto $scheme;
    }
}
```

If you're behind a reverse proxy, configure [Forwarded Headers Middleware](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) so Bark sees the real client scheme/host. `robots.txt` and `sitemap.xml` both build absolute URLs from the incoming request, so getting this right matters for SEO correctness, not just logging.

## Running as a service

A basic `systemd` unit:

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

Bark holds the entire rendered page set and the search index in memory. For a docs set in the hundreds-of-pages range, that's a non-issue on essentially any VM. If you're hosting tens of thousands of pages, you've outgrown the assumptions this tool was built around. Bark stays narrow on purpose, and "scale to an arbitrary number of pages" isn't a design goal.

The next logical step to visit is [Site Config](../reference/site-config); for the full `appsettings.json` and `config.json` reference.
