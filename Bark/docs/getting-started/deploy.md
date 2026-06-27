---
title: Deploy
description: Docker, Windows/IIS, Linux release, or build from source
---

# Deploy

These guides assume you already have a `docs/` folder with content in it, see [Getting Started](getting-started) if you don't. Pick whichever path matches your environment. Docker is the fastest, usually under a minute from a blank folder to a running site.

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

A self-contained Linux x64 build (`*-Linux_x64.zip`) ships alongside every release. This is a convenient option if you prefer running the binary directly without Docker.

1. Download the latest `*-Linux_x64.zip` from [Releases](https://github.com/melosso/bark/releases).
2. Extract it to your server (for example `/srv/bark`):

```bash
mkdir -p /srv/bark && unzip Bark-*-Linux_x64.zip -d /srv/bark
```

3. Prepare your `docs/` directory with your Markdown content and optional `config.json`:

```bash
mkdir -p /srv/bark/docs
# Place your .md files in /srv/bark/docs
```

4. Run the binary:

```bash
cd /srv/bark && ./Bark
```

> [!NOTE]
> The binary expects a `docs/` folder relative to the current working directory. If your content lives elsewhere, set `Docs:RootPath` in `appsettings.json` or pass it as an environment variable. See [Environment Variables](/getting-started/environment-variables) for the full list.

5. Browse to `http://localhost:8080`.

For a production setup, you can configure Bark as a systemd service (see [Running as a service](#running-as-a-service-source-builds) below).

## Option D: Build from source

If you're contributing to Bark itself, or don't want to pull a container image:

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

If you're actively developing Bark's own source rather than running it, `dotnet watch --project src/Bark` from a clone gives you C#-side hot reload too.

## Option E: Static export (GitHub Pages, etc.)

You can skip the server entirely and export plain HTML, CSS, and JS for any static host: GitHub Pages, Netlify, or a plain web server. This path requires cloning the repository and compiling Bark yourself.

```bash
dotnet publish src/Bark -c Release -o ./publish
cd publish && ./Bark --export ./output --base-url https://you.github.io --base-path /your-repo
```

When you run the binary, it accepts these CLI flags:

| Flag | Purpose |
|---|---|
| `--export <dir>` | Writes every page, plus `404.html`, `robots.txt`, `llms.txt`, `sitemap.xml`, and `wwwroot` to the given directory. |
| `--base-url <origin>` | The real public origin used for absolute URLs in `robots.txt` and `llms.txt`. |
| `--base-path </prefix>` | Required when the site lives under a subpath, such as a GitHub project page (`you.github.io/your-repo/`). This flag overrides `Docs:BasePath` at runtime. See [Site Config](/reference/site-config). |

> [!NOTE]
> Run the binary from inside the publish folder (`cd publish` first). The `docs/` lookup is relative to the current directory, not the executable's location. The `--export` flag also disables hot reload, so there is no `/api/build-version` polling. Search still renders but fails gracefully without a backend.

<br>

A working GitHub Actions example lives in `.github/workflows/bark-deployment.yml`. It still needs **Settings → Pages → Source → GitHub Actions** set once per repo before the first deploy succeeds.

## Production-ready

Bark is configured for production stability out of the box. The following optimizations are pre-configured and active by default:

* **Automatic Compression**: All web traffic uses Brotli or Gzip compression (including secure HTTPS traffic) to significantly reduce page load times and save bandwidth.
* **Built-in DoS Protection**: Safety limits are pre-configured to protect the server from resource exhaustion. This includes strict limits on request body sizes, header sizes, maximum simultaneous connections, and keep-alive timeouts.
* **Production Logging**: Logs are routed directly to the console. You can easily adjust how detailed these logs are for different environment if necessary.
* **Smart Caching (ETags)**: Every page includes a unique fingerprint (SHA-256 ETag). If a user's browser already has the current version of a page then the server responds with "304 Not Modified" status, saving resources.

::: note
**What does this mean for you?** Performance and security optimizations are enabled automatically. You only need to configure your external infrastructure, such as your domain, firewall, and SSL certificates.
:::

## Reverse Proxy Setup

Bark is designed to sit behind a dedicated web server or load balancer that handles SSL/TLS certificates and external traffic encryption.

If you are using Docker, Bark listens internally on port 8080. Below is a minimal Nginx configuration to safely route external traffic to your Bark container:

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