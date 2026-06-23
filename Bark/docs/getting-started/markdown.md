---
title: Markdown Cheatsheet
description: Every Markdown extension Bark supports, with live examples
---

# Markdown Cheatsheet

Everything below renders live on this page. It's not a static screenshot. If something here looks broken, the renderer is broken, not the doc.

## Syntax-highlighted code blocks

Real grammar-based tokenization (TextMateSharp, the same engine family Shiki wraps), not a regex-based approximation. Works across the ~65 languages bundled with Bark. A few examples:

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

Use the real language id (`csharp`, not `c#`). See [CLI](../reference/cli) if you're wondering why the shorthand doesn't work the same.

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

## Custom containers

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

> [!NOTE]
> A GitHub-style alert block.

> [!WARNING]
> Another one, different color.
