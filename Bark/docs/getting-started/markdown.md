---
title: Markdown
description: Every Markdown extension Bark supports, with live examples
---

# Using Markdown

All pages are primarily written in [Markdown](https://www.markdownguide.org/getting-started/). Everything below renders live on this page; it´s tested and verified.

## Syntax-highlighted code blocks

We're using grammar-based tokenization (using `TextMateSharp`) which works across the ~65 languages bundled with Bark. A few examples:

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

## Line highlighting

Highlight specific lines with `{n,m-o}` in the fence info string:

```ts{2}
function add(a: number, b: number) {
  return a + b; // this line is highlighted
}
```

Or mark a line with a trailing comment. Bark strips the marker and keeps the highlight:

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

```md
::: code-group
```sh [npm]
npm install
```
```sh [pnpm]
pnpm install
```
:::
```

Renders as:

::: code-group
```sh [npm]
npm install
```
```sh [pnpm]
pnpm install
```
:::

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

These are blocks starting and ending with ```:::``` follwed by either `tip`, `warning`, `danger` or `details`. Can be combined with `Click to expand` for expanding blocks.

::: tip
This is a tip container.
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

## Badges

Small inline labels, the kind you'd drop next to a heading to flag "new in 3.0" or an unstable API. Write them as plain HTML, Bark passes unrecognized tags straight through and styles `<badge>` itself:

```md
Search now supports `Ctrl+K` <Badge type="tip">3.0+</Badge>
```

Renders as:

Search now supports `Ctrl+K` <Badge type="tip">3.0+</Badge>

Four types, same colors as the alert blocks above: `info` (blue), `tip` (green, default), `warning` (amber), `danger` (red).

<Badge type="info">info</Badge>
<Badge type="tip">tip</Badge>
<Badge type="warning">warning</Badge>
<Badge type="danger">danger</Badge>

> [!IMPORTANT]
> Always close the tag: `<Badge type="tip">text</Badge>`. A self-closing `<Badge text="x" />` looks reasonable but breaks, HTML has no XML-style self-close for unknown elements, so it silently swallows the rest of the paragraph as its content instead of rendering a badge.

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

A sentence with a footnote.[^1]

[^1]: The footnote text.
