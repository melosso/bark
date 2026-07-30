---
title: Themes
description: Override colors, ship your own CSS and JS, or hand the whole theme folder to a designer
---

# Themes

Bark ships nine built-in themes and four escalating levels of control, from "pick a different look" to "replace every line of CSS":

1. **A built-in theme**, one word in `config.json`.
2. **CSS variables**, for palette and font tweaks on top of it.
3. **A `custom.css` / `custom.js` drop-in**, for anything a variable cannot reach.
4. **Your own theme class**, to ship a look as code.

Just want dark mode off, or your brand color in place? Skip to [CSS variables](#css-variables).

## Picking a theme

Set `theme` in `docs/config.json`:

```json
{
  "theme": "forest-ledger"
}
```

That is the whole opt-in. Leave it out and you get `default`. The value hot-reloads with the rest of `config.json`, so you can flip between themes without restarting.

| Name | Look |
|---|---|
| `default` | Off-white ground, centred hero under a green kicker, deep green accent, flat monochrome outline icons under a hairline rule. |
| `forest-ledger` | Warm paper tones and forest green, with features set as numbered rule-lines. |
| `signal-dark` | Deep charcoal and an amber accent, with a divided feature row. |
| `blueprint-grid` | Mint tints, features drawn as one bordered grid with tinted icon chips. |
| `ocean` | Cool blue-grey paper and a deep harbour accent, with the feature row on a tinted band. |
| `deep-space` | Near-black navy and a periwinkle accent. The darkest built-in, with a soft-cornered feature grid. |
| `solarized` | Solarized base tones: warm paper by day, deep teal by night, square corners throughout. |
| `laserwave` | Synthwave violet with a hot magenta accent and a 2px accent rule above each feature. |
| `limelight` | Pale off-white with a sage-lime accent and a cyan counterpart. Square corners and a gradient rule under the hero. |

Every theme carries a full light **and** dark palette, so the light/dark toggle behaves the same whichever you pick. The dark-sounding names are not dark-only: `signal-dark` has a paper-toned light mode that swaps its amber for bronze, and `laserwave` and `deep-space` have daylight palettes too.

An unrecognised name logs a warning and falls back to `default`. A typo will never take your site down.

Two other places can set it, for when `config.json` is the wrong home:

| Source | Wins over | Use it for |
|---|---|---|
| `--theme <name>` CLI flag | everything | previewing a theme, or exporting a themed static site |
| `Docs:Themes:Name` in `appsettings.json` | `config.json` | pinning a theme per deployment |
| `theme` in `docs/config.json` | nothing | the normal case |

```bash
dotnet run --project src/Bark -- --theme signal-dark
dotnet run --project src/Bark -- --export ./out --theme blueprint-grid
```

### What a theme controls

Colors, fonts, icon treatment, card treatment, hero styling. That is the whole list.

A theme cannot change your navigation, content, URLs, or front matter. Switching themes never reshuffles a sidebar or breaks a link, which is why it is safe to try one on a live site.

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

Bark's layout reads its colors and fonts from CSS variables. The active theme supplies a full set; override the ones you care about and leave the rest.

Defaults below are the `default` theme's light values:

| Variable | Default (light) | Controls |
|---|---|---|
| `--primary-color` | `#1f6b4a` | Links, highlights, the active nav indicator. Also sets `--accent`. |
| `--bg-color` | `#fafafa` | Page background. |
| `--sidebar-bg` | `#f2f4f2` | Sidebar background. |
| `--text-color` | `#1a1d1f` | Primary text. |
| `--text-muted` | `#565f59` | Descriptions, timestamps, muted labels. |
| `--border` | `#e2e7e2` | Hairline borders throughout the layout. |
| `--code-bg` | `#f4f6f4` | Inline code and fenced code blocks. |
| `--search-bg` | `#f4f6f4` | The boxed search field in the header. Falls back to `--sidebar-bg` when a theme leaves it unset. |
| `--accent-light` | `#eef1ee` | Accent-tinted surface: active nav rows, hover fills. Carries no text-contrast duty, so a theme can put a saturated color here. |
| `--promo-bg` / `--promo-text` | `--accent-light` / `--accent` | The announcement bar above the header. Aliases by default, so they follow your accent unless you set them. |
| `--font-sans` | system stack | Body font. |
| `--font-mono` | system stack | Code font. |

> [!IMPORTANT]
> These overrides apply to **both** light and dark mode, because one value cannot be right for two grounds. Set `--bg-color` and you get that background in dark mode too. To change a single mode, use `custom.css` with a `:root[data-theme="dark"]` selector, or write your own theme.

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
> If `Docs:Themes` exists in `appsettings.json` at all, it wins outright over `theme.json`. Bark does not merge the two field by field, it picks one source. Use `theme.json` for filesystem-only workflows and `appsettings.json` for everything else.

Two more fields round out the toggle list:

| Field | Type | Default | Effect |
|---|---|---|---|
| `DarkMode` | `bool` | `true` | Toggles the `prefers-color-scheme: dark` variant and the in-page dark mode switch. |
| `ShowScrollIndicator` | `bool` | `true` | The thin progress bar pinned to the top of the viewport while you scroll. |

## Escape hatches

CSS variables cover palette and fonts. For layout tweaks, hiding an element, or animating something, reach for `custom.css` and `custom.js` directly.

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

`custom.css` loads after Bark's own stylesheet, so a plain selector wins without a specificity fight. `custom.js` runs with `defer`, after the DOM is parsed but before Bark's inline script finishes setting up search, the sidebar, and dark mode. To run after those are ready, listen for `DOMContentLoaded` as above.

> [!TIP]
> Need a CSS file hosted elsewhere, such as a CDN or a different path? Set `CustomCssUrl` in `Docs:Themes` or `theme.json`. It takes priority over an auto-detected `custom.css` if both exist.

## Writing your own theme

If a look is worth keeping, ship it as code instead of a pile of CSS overrides. A theme is one class implementing `IBarkTheme`, in its own file under `src/Bark/Services/Theming/Themes/`:

```csharp
namespace Bark.Services.Theming.Themes;

public sealed class MidnightTheme : IBarkTheme
{
    public string Name => "midnight";

    public string Label => "Midnight";

    public IReadOnlyDictionary<string, string> LightTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#ffffff",
        ["--sidebar-bg"] = "#f7f8fb",
        ["--text-color"] = "#12151f",
        ["--text-muted"] = "#5a6072",
        ["--accent"] = "#3a4bb8",
        ["--accent-light"] = "#eceefa",
        ["--border"] = "#dfe2ec",
        ["--code-bg"] = "#f3f5fa"
    };

    public IReadOnlyDictionary<string, string> DarkTokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["--bg-color"] = "#0c0e16",
        ["--sidebar-bg"] = "#12141f",
        ["--text-color"] = "#e6e8f2",
        ["--text-muted"] = "#969cb4",
        ["--accent"] = "#8b98f0",
        ["--accent-light"] = "#181c2e",
        ["--border"] = "#232739",
        ["--code-bg"] = "#12151f"
    };

    public string ComponentCss => """
                .bark-feature {
                    border-radius: 0;
                }
        """;
}
```

Then add one line to `ThemeRegistry.All`:

```csharp
public static IReadOnlyList<IBarkTheme> All { get; } =
[
    new DefaultTheme(),
    new ForestLedgerTheme(),
    new SignalDarkTheme(),
    new BlueprintGridTheme(),
    new OceanTheme(),
    new DeepSpaceTheme(),
    new SolarizedTheme(),
    new LaserwaveTheme(),
    new MidnightTheme()
];
```

`"theme": "midnight"` now works in `config.json`.

Three rules the test suite enforces for you:

- **Both modes, always.** Every literal color in `LightTokens` needs a counterpart in `DarkTokens`, or the light value bleeds into dark mode.
- **The eight palette keys are required.** `--bg-color`, `--sidebar-bg`, `--text-color`, `--text-muted`, `--accent`, `--accent-light`, `--border`, `--code-bg`. Everything else (alert hues, fonts, shadows, alias variables) comes from `ThemeDefaults` and only needs declaring when you want it different.
- **Contrast is checked.** Text, muted text and accent must clear 4.5:1 against the background in both modes. Run `dotnet test --filter ThemeContrastTests` and it reports the exact ratio it measured.

`ComponentCss` is appended after the entire built-in stylesheet, inside the same `<style>` element, so plain selectors win without `!important`.

## Limitations

Themes change how Bark looks, not how it is built. You cannot restructure the header, add interactive features to the sidebar, or give individual pages their own layout.

Custom CSS and JavaScript stretch this a fair way, but a genuinely different structure means changing Bark's own code. That is deliberate: one well-maintained layout beats a plugin system that the maintainers cannot easily debug.