---
title: Deploy
description: Docker, Windows/IIS, Linux release, or build from source
---

# Deploy

These guides assume you already have a documentation folder with content in it. See [Getting Started](/guide/getting-started) if you don't. Pick whichever path matches your environment. Docker is the fastest, usually under a minute from a blank folder to a running site.

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
      - ./docs:/app/docs:ro,Z
    environment:
      PublicBaseUrl: https://docs.example.com
      AllowedHosts: docs.example.com
```

Mount your own `docs/` folder (your `.md` files plus an optional `config.json`), set `PublicBaseUrl` to the origin you serve from, then run:

```bash
docker compose up -d
```

Browse to `http://localhost:8080`.

### Keeping `docs/` in sync with Git

Bark can clone and pull `docs/` for you, entirely from env vars: no manual `git clone` step, no credentials baked into a remote URL.

```yaml [docker-compose.yml]
services:
  bark:
    image: ghcr.io/melosso/bark:latest
    env_file: .env
    volumes:
      - ./docs:/app/docs:Z
```

```bash [.env]
GIT_ENABLED=true
GIT_URL=https://github.com/you/your-docs.git
GIT_USERNAME=you
GIT_PASSWORD=your-token
GIT_CRON=*/5 * * * *
```

Off by default. If `docs/` isn't already a checkout, Bark clones `GIT_URL` into it on startup; either way it then runs `git pull --ff-only` on the `GIT_CRON` schedule (standard 5-field cron expression), picked up by the existing file watcher. A failed clone or pull logs a warning and never takes the site down. Needs write access to `docs/`, no `:ro` (Bark's container also runs as a non-root user, so the mount needs to be writable by it).

`GIT_USERNAME`/`GIT_PASSWORD` are sent as an HTTP Basic auth header per git invocation, never written into the remote URL or `.git/config`. For a token-only host (e.g. GitHub PAT), set `GIT_PASSWORD` to the token and leave `GIT_USERNAME` as any non-empty value.

The ASP.NET Core convention `Git__Enabled`/`Git__Url`/`Git__Username`/`Git__Password`/`Git__Cron` also works and takes precedence if both are set.

## Option B: Windows / IIS

1. Install the [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/10.0){target="_blank" rel="noopener"} on the server. This is what gives IIS the ASP.NET Core Module.
2. Download the latest `*-Windows_x64.zip` from [Releases](https://github.com/melosso/bark/releases){target="_blank" rel="noopener"} and extract it to your site folder, for example `C:\inetpub\bark`.
3. In IIS, create a site pointing at that folder with the **No Managed Code** .NET CLR version. Bark hosts itself through the ASP.NET Core Module and needs nothing from the CLR.
4. Start the site and browse to it.

The zip ships a `web.config` wired for in-process hosting, so no manual edits are needed.

## Option C: Linux release zip

A self-contained Linux x64 build ships alongside every release.

1. Download the latest `*-Linux_x64.zip` from [Releases](https://github.com/melosso/bark/releases){target="_blank" rel="noopener"} and extract it:

```bash
mkdir -p /srv/bark && unzip Bark-*-Linux_x64.zip -d /srv/bark
mkdir -p /srv/bark/docs   # your .md files go here
```

2. Run it, then browse to `http://localhost:8080`:

```bash
cd /srv/bark && ./Bark
```

::: note
The binary looks for `docs/` relative to the current working directory, not the executable. If your content lives elsewhere, set `Docs:RootPath`. See [Environment Variables](/guide/environment-variables).
:::

To survive a reboot, see [Running as a service](#running-as-a-service-source-builds).

## Option D: Build from source

For contributing to Bark itself, or to avoid pulling a container image. Needs the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0){target="_blank" rel="noopener"}.

```bash
cd Bark
dotnet publish src/Bark -c Release -o ./publish
cd publish && dotnet Bark.dll
```

Your `docs/` folder is copied into the publish output automatically. The target machine still needs the .NET runtime unless you add `--self-contained true -r <rid>`.

Developing Bark's own source rather than running it? `dotnet watch --project src/Bark` gives you C#-side hot reload too.

## Option E: Static export (GitHub Pages, etc.)

Skip the server entirely and export plain HTML, CSS, and JS for any static host. This path requires cloning the repository and compiling Bark yourself.

```bash
dotnet publish src/Bark -c Release -o ./publish
cd publish && ./Bark --export ./output --base-url https://you.github.io --base-path /your-repo
```

| Flag | Purpose |
|---|---|
| `--export <dir>` | Writes every page, plus `404.html`, `robots.txt`, `llms.txt`, `sitemap.xml`, and `wwwroot` to the given directory. |
| `--base-url <origin>` | The real public origin used for absolute URLs in `robots.txt` and `llms.txt`. |
| `--base-path </prefix>` | Required when the site lives under a subpath, such as a GitHub project page (`you.github.io/your-repo/`). Overrides `Docs:BasePath` at runtime. See [Site Config](/reference/site-config). |

::: note
Run the binary from inside the publish folder (`cd publish` first), because the `docs/` lookup is relative to the current directory. `--export` also disables hot reload, so there is no `/api/build-version` polling. Search still renders but fails gracefully without a backend.
:::

A working GitHub Actions example lives in `.github/workflows/bark-deployment.yml`. It needs **Settings → Pages → Source → GitHub Actions** set once per repo before the first deploy succeeds.

## What you get by default

No flags required for any of this:

* **Compression.** Brotli or Gzip on all traffic, including HTTPS.
* **DoS limits.** Caps on request body size, header size, simultaneous connections, and keep-alive timeouts.
* **Console logging.** Verbosity is adjustable per environment.
* **[ETags](https://en.wikipedia.org/wiki/HTTP_ETag){target="_blank" rel="noopener"}.** Every page carries a SHA-256 fingerprint, so an unchanged page returns `304 Not Modified`.

What's left to you is external: domain, firewall, and SSL certificates.

## Reverse proxy setup

Bark expects to sit behind a web server or load balancer that terminates TLS. Under Docker it listens on port 8080:

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

Running `dotnet Bark.dll` from a source build uses whatever port `ASPNETCORE_URLS` or your launch profile sets instead, so adjust `proxy_pass` to match.

Configure [Forwarded Headers Middleware](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer){target="_blank" rel="noopener"} so Bark sees the real client scheme and host. `robots.txt` and `sitemap.xml` build absolute URLs from the incoming request, so this affects SEO correctness, not just logging.

## Running as a service (source builds)

Docker and the IIS zip manage their own process lifecycle. For a source build:

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

Hot reload keeps working here. Restart the service to ship a code change, not a content change.

## Going to production

Set `PublicBaseUrl` to the origin you actually serve from and `AllowedHosts` to the matching hostname. In containers, also turn off hot reload, since documentation is usually baked into the image or mounted read-only, so the filesystem watcher has nothing to do.

```yaml
    environment:
      PublicBaseUrl: https://docs.example.com
      Docs__EnableHotReload: "false"
      AllowedHosts: docs.example.com
```

`PublicBaseUrl` is a security setting, not a convenience one. Without it, the absolute URLs in `robots.txt`, `llms.txt`, the RSS feed and your `canonical`/`og:url` tags are built from the request's `Host` header, which the caller controls. Someone can request your `robots.txt` with a forged `Host` and get back a `Sitemap:` line pointing at their own site; if a CDN caches that response, the forged copy is what your visitors and crawlers get. `AllowedHosts` closes the same gap from the other side, rejecting hostnames you don't serve rather than quietly answering them.

See [Environment variables](/guide/environment-variables/) for the full list.

## Sizing expectations

Bark holds the entire rendered page set and the search index in memory. For a site in the hundreds of pages, that's a non-issue anywhere. At tens of thousands of pages, you've outgrown the assumptions this tool was built around.
