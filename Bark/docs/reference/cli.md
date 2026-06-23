---
title: CLI
description: The dotnet commands you'll use to run, build, and test Bark
---

# CLI

Bark doesn't ship its own CLI binary. There's no `bark dev` or `bark build` to learn. It's a regular .NET 10 web project, so the `dotnet` CLI is the whole interface. Run everything from the `Bark/` directory, where `Bark.slnx` lives.

## `dotnet restore`

```bash
dotnet restore
```

Pulls NuGet dependencies. You need this once after cloning, and again any time `Directory.Packages.props` changes.

## `dotnet watch`

```bash
dotnet watch --project src/Bark
```

Starts the dev server with hot reload on both sides: `dotnet watch` recompiles and restarts on C# changes, and Bark's own `FileSystemWatcher` rebuilds the page cache on Markdown/`config.json` changes independently, without a restart. Serves on `http://localhost:5000` by default.

## `dotnet run`

```bash
dotnet run --project src/Bark
```

Same server, no C# hot reload. Use this when you're only editing content and don't want a `dotnet watch` process recompiling on every save. In practice it rarely matters, since Markdown changes never trigger a recompile either way.

## `dotnet build`

```bash
dotnet build
```

Compiles the solution. Mostly useful for a fast correctness check before you commit, or in CI.

## `dotnet test`

```bash
dotnet test
```

Runs the full xUnit suite in `tests/Bark.Tests`.

**Filter to one class:**

```bash
dotnet test --filter "FullyQualifiedName~MarkdownServiceTests"
```

**Filter to one test:**

```bash
dotnet test --filter "FullyQualifiedName~MarkdownServiceTests.MethodName"
```

`--filter` matches on the fully qualified test name, so partial matches (`~`) work for both class-level and method-level filtering. Handy when you're iterating on one feature and don't want to wait for the whole suite.

## `dotnet publish`

```bash
dotnet publish src/Bark -c Release -o ./publish
```

Builds a production output folder, `docs/` content included. See [Deploy](../getting-started/deploy) for what to do with it next.

---

That's the complete list. No plugin system, no command you need to discover through `--help` that isn't already standard `dotnet` tooling.
