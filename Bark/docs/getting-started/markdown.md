---
title: Markdown
description: Every Markdown extension Bark supports, with live examples
---

# Using Markdown

All pages are primarily written in [Markdown](https://www.markdownguide.org/getting-started/). Everything below renders live on this page; it´s tested and verified.

## Syntax-highlighted code blocks

We're using grammar-based tokenization[^1] which works across the ~65 languages bundled with Bark. A few examples:

```csharp
public sealed record Order(string Id, decimal Total)
{
    public bool IsLarge => Total > 1000m;
}
```

```python
def fibonacci(n: int) -> int:
    if n < 2:
        return n
    return fibonacci(n - 1) + fibonacci(n - 2)
```

```java
public class Greeter {
    public String greet(String name) {
        return "Hello, " + name + "!";
    }
}
```

```rust
fn main() {
    let nums = vec![1, 2, 3];
    println!("{:?}", nums.iter().sum::<i32>());
}
```

```sql
SELECT id, title FROM pages WHERE published = true ORDER BY title;
```

Use the real language id (`csharp`, not `c#`); the shorthand doesn't work the same.

## Title bars

Add `[filename]` to the fence info string to show a title bar instead of the language badge:

````md
```json [./appsettings.json]
{
    "Hello": "world"
}
```
````

Renders as:

```json [./appsettings.json]
{
    "Hello": "world"
}
```

## Line highlighting

Highlight specific lines with `{n,m-o}` in the fence info string:

```ts{2}
function add(a: number, b: number) {
  return a + b; // this line is highlighted
}
```

Or mark a line with a ==trailing== comment. Bark strips the marker and keeps the highlight:

```ts
const cache = new Map();
const ttlMs = 60_000; // [!code highlight]
```

## Diff notation

```ts
const port = 3000; // [!code --]
const port = process.env.PORT ?? 3000; // [!code ++]
```

## Focus

```ts
function setup() {
  loadConfig(); // [!code focus]
  startServer();
}
```

## Line numbers

```ts:line-numbers
const a = 1;
const b = 2;
const c = a + b;
```

## Code groups

This example:

````md
::: code-group
```sh [npm]
npm install
```
```sh [pnpm]
pnpm install
```
```sh [c# icon:sharp]
dotnet restore
```
:::
````

Renders as:

::: code-group
```sh [npm]
npm install
```
```sh [pnpm]
pnpm install
```
```sh [c# icon:sharp]
dotnet restore
```
:::

Use `[label]` for the tab title. If the label matches a name on [Simple Icons](https://simpleicons.org/), its icon will display automatically. You can a specific icon with `[label icon:slug]`, e.g. `[csharp icon:dotnet]`.

## Alerts

Use these for quick highlights within your prose. These can be spawned with the syntax `> [!TYPE]`:

> [!NOTE]
> Important context.

> [!TIP]
> Helpful advice.

> [!IMPORTANT]
> Required for success.

> [!WARNING]
> Immediate attention needed.

> [!CAUTION]
> Potential negative risks.

## Custom containers

These are blocks starting and ending with ```:::``` follewed by either `note`, `tip`, `warning`, `danger` or `details`. Can be combined with `Click to expand` for expanding blocks.

::: note
This is a note container.
:::

::: tip
This is a tip container.
:::

::: info
This is an info container.
:::

::: warning
This is a warning container.
:::

::: danger
This is a danger container.
:::

::: details Click to expand
Hidden content goes here.
:::

Custom containers are sugar over a plain `<div class="TYPE custom-block">`. Markdig passes raw HTML straight through, so you can write the div yourself when you need something the shorthand can't do, like a one-off inline style:

```md
<div class="tip custom-block">

Just want to try it out? Skip to the [Quickstart](./getting-started).

</div>
```

Renders as:

<div class="tip custom-block">

Just want to try it out? Skip to the [Quickstart](./getting-started).

</div>

Though make sure to leave a blank line after the opening `<div>` and before the closing `</div>`. Without it, the Markdown renderer treats the inside as raw HTML instead of Markdown and your `[links](...)` won't render.

## Badges

Small inline labels, the kind you'd drop next to a heading to flag "new in 3.0" or an unstable API. Write them as plain HTML, Bark passes unrecognized tags straight through and styles `<badge>` itself:

```md
Hello world <Badge type="tip">3.0+</Badge>
```

Renders as:

Hello world <Badge type="tip">3.0+</Badge>

Four types, same colors as the alert blocks above: `info` (blue), `tip` (green, default), `warning` (amber), `danger` (red).

<Badge type="info">info</Badge>
<Badge type="tip">tip</Badge>
<Badge type="warning">warning</Badge>
<Badge type="danger">danger</Badge>

::: danger
Always close the tag: `<Badge type="tip">text</Badge>`. A self-closing `<Badge text="x" />` looks reasonable but breaks, HTML has no XML-style self-close for unknown elements, so it silently swallows the rest of the paragraph as its content instead of rendering a badge.
:::

## Math

Inline: $E = mc^2$

Block:

$$
\sum_{i=1}^{n} i = \frac{n(n+1)}{2}
$$

## Standard Markdown

Tables, task lists, footnotes, and alert blocks all work too:

| Feature | Supported |
|---|---|
| Tables | ✅ |
| Task lists | ✅ |
| Footnotes | ✅ |

- [x] Done
- [ ] Not done yet

A sentence with a footnote.[^2]

[^1]: Using TextMateSharp found [here](https://github.com/danipen/TextMateSharp){target="_blank" rel="noopener"}.
[^2]: The footnote text.

## Link attributes

Append `{target="_blank" rel="noopener"}` after a link to open it in a new tab:

```md
[Bark on GitHub](https://github.com/org/bark){target="_blank" rel="noopener"}
```
