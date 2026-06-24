---
title: Extending Themes
description: Override colors, ship your own CSS and JS, or hand the whole theme folder to a designer
---

# Extending Themes

Bark ships one built-in theme. You don't pick from a gallery of themes, and there's no plugin system for swapping in a different layout engine. What you get instead is three escalating levels of control, from "change one color" to "replace every line of CSS":

1. **CSS variables**, for palette and font tweaks.
2. **A `custom.css` / `custom.js` drop-in**, for anything a variable can't reach.
3. **`Docs:Themes` in `appsettings.json**`, for deployment-level overrides that don't live in the filesystem.

Just want dark mode off or your brand color in place? Skip to [CSS variables](#css-variables).

## The theme folder

Drop your custom files into the right place:

| File | Effect |
|---|---|
| `wwwroot/theme/custom.css` | Loaded last, after every built-in style. Plain selectors win without `!important`. |
| `wwwroot/theme/custom.js` | Loaded with `defer` on every page. |
| `wwwroot/theme/theme.json` | CSS variable overrides and toggles, as plain JSON. See [CSS variables](#css-variables) for the field list. |

> [!IMPORTANT]
> New files in `wwwroot/theme/` need an application restart to take effect.

## CSS variables

Bark's layout reads its colors and fonts from CSS variables. Override the ones you care about and leave the rest:

| Variable | Default (light) | Controls |
|---|---|---|
| `--primary-color` | `#2e4a36` | Links, highlights, the active nav indicator. |
| `--bg-color` | `#fafafa` | Page background. |
| `--sidebar-bg` | `#f4f4f4` | Sidebar background. |
| `--text-color` | `#1a1a1a` | Primary text. |
| `--text-muted` | `#666666` | Descriptions, timestamps, muted labels. |
| `--border` | `#e5e5e5` | Hairline borders throughout the layout. |
| `--code-bg` | `#f0f0f0` | Inline code and fenced code blocks. |
| `--accent-light` | `#e8ece9` | Light tint of the accent, used for active/highlighted states. |
| `--font-sans` | system stack | Body font. |
| `--font-mono` | system stack | Code font. |

Set them with `theme.json` (no config edit) or `Docs:Themes` in `appsettings.json` (no filesystem write). Field names map 1:1 to the variables above, in PascalCase:

::: code-group

```json [wwwroot/theme/theme.json]
{
  "primaryColor": "#7c3aed",
  "fontSans": "'Inter', system-ui, sans-serif",
  "darkMode": true
}
```

```json [appsettings.json]
{
  "Docs": {
    "Themes": {
      "PrimaryColor": "#7c3aed",
      "FontSans": "'Inter', system-ui, sans-serif",
      "DarkMode": true
    }
  }
}
```

:::

> [!IMPORTANT]
> If `Docs:Themes` exists in `appsettings.json` at all, it wins outright over `theme.json`. Bark doesn't merge the two field-by-field, it picks one source and uses it. Pick `theme.json` for filesystem-only workflows, `appsettings.json` for everything else.

Two more fields round out the toggle list:

| Field | Type | Default | Effect |
|---|---|---|---|
| `DarkMode` | `bool` | `true` | Toggles the `prefers-color-scheme: dark` variant and the in-page dark mode switch. |
| `ShowScrollIndicator` | `bool` | `true` | The thin progress bar pinned to the top of the viewport while you scroll. |

## Escape hatches

CSS variables cover palette and fonts. For anything else, layout tweaks, hiding an element, animating something, reach for `custom.css` and `custom.js` directly:

`wwwroot/theme/custom.css`:

```css
/* Bark's CSS variables don't expose border-radius, so override the rule directly. */
.search-trigger {
  border-radius: 999px;
}
```

`wwwroot/theme/custom.js`:

```js
document.addEventListener('DOMContentLoaded', () => {
  console.log('Custom theme JS loaded.');
});
```

`custom.css` loads after Bark's own stylesheet, so a plain selector overrides the built-in one without a specificity fight. `custom.js` runs with `defer`, after the DOM is parsed but before Bark's own inline script finishes setting up search, the sidebar, and dark mode. If you need to run after those are ready, listen for `DOMContentLoaded` like the example above.

> [!TIP]
> Need a CSS file hosted somewhere other than `wwwroot/theme/`, a CDN, a different path? Set `CustomCssUrl` in `Docs:Themes` or `theme.json` instead. It takes priority over the auto-detected `custom.css` if both exist.

## Limitations

Bark uses one standard design for every page. Because of this, you cannot change the header, add new interactive features to the sidebar, or give different pages their own unique look.

If you need a major change to the layout, you can sometimes use custom CSS and JavaScript files to alter the appearance. However, if you need a completely different structure, you would need to modify the underlying code of the application itself. We have made this a deliberate design choice: we prioritize a single, high-quality, and easy-to-maintain design over a complex system that is difficult for most users to manage.