using System.Globalization;
using System.Text.RegularExpressions;
using Bark.Services.Layout;
using Bark.Services.Theming;

namespace Bark.Tests;

public sealed class ThemeRegistryTests
{
    [Fact]
    public void Resolve_NullOrBlank_ReturnsDefault()
    {
        Assert.Same(ThemeRegistry.Default, ThemeRegistry.Resolve(null));
        Assert.Same(ThemeRegistry.Default, ThemeRegistry.Resolve("   "));
    }

    [Fact]
    public void Resolve_UnknownName_FallsBackToDefaultWithoutThrowing()
    {
        Assert.Same(ThemeRegistry.Default, ThemeRegistry.Resolve("no-such-theme"));
    }

    [Theory]
    [InlineData("forest-ledger")]
    [InlineData("FOREST-LEDGER")]
    [InlineData("  Forest-Ledger  ")]
    public void Resolve_IsCaseAndWhitespaceInsensitive(string name)
    {
        Assert.Equal("forest-ledger", ThemeRegistry.Resolve(name).Name);
    }

    [Fact]
    public void All_NamesAreUniqueAndKebabCase()
    {
        var names = ThemeRegistry.All.Select(t => t.Name).ToList();
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(names, n => Assert.Matches("^[a-z][a-z0-9-]*$", n));
    }

    [Fact]
    public void Default_IsTheDefaultTheme() => Assert.Equal("default", ThemeRegistry.Default.Name);
}

public sealed class ThemeTokenTests
{
    public static TheoryData<string> ThemeNames() => [.. ThemeRegistry.All.Select(t => t.Name)];

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void RequiredPaletteKeys_PresentInBothModes(string name)
    {
        var theme = ThemeRegistry.Resolve(name);
        foreach (var key in IBarkTheme.RequiredPaletteKeys)
        {
            Assert.True(theme.LightTokens.ContainsKey(key), $"{name} light is missing {key}");
            Assert.True(theme.DarkTokens.ContainsKey(key), $"{name} dark is missing {key}");
        }
    }

    /// <summary>A literal declared in one mode bleeds into the other; alias values re-resolve, so only literals need parity.</summary>
    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void EveryLiteralColourHasBothModes(string name)
    {
        var theme = ThemeRegistry.Resolve(name);
        var lightLiterals = theme.LightTokens.Where(t => t.Value.StartsWith('#')).Select(t => t.Key);
        var darkLiterals = theme.DarkTokens.Where(t => t.Value.StartsWith('#')).Select(t => t.Key);

        Assert.Empty(lightLiterals.Except(theme.DarkTokens.Keys));
        Assert.Empty(darkLiterals.Except(theme.LightTokens.Keys));
    }
}

public sealed class ThemeContrastTests
{
    public static TheoryData<string> ThemeNames() => [.. ThemeRegistry.All.Select(t => t.Name)];

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void LightMode_MeetsContrastFloor(string name) =>
        AssertContrastFloor(ThemeRegistry.Resolve(name), dark: false);

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void DarkMode_MeetsContrastFloor(string name) =>
        AssertContrastFloor(ThemeRegistry.Resolve(name), dark: true);

    private static void AssertContrastFloor(IBarkTheme theme, bool dark)
    {
        var tokens = dark
            ? Merge(theme.LightTokens, theme.DarkTokens)
            : theme.LightTokens;

        var bg = tokens["--bg-color"];
        var mode = dark ? "dark" : "light";

        AssertRatio(tokens["--text-color"], bg, 4.5, $"{theme.Name}/{mode} body text");
        AssertRatio(tokens["--text-muted"], bg, 4.5, $"{theme.Name}/{mode} muted text");
        AssertRatio(tokens["--accent"], bg, 4.5, $"{theme.Name}/{mode} accent");
        AssertRatio(tokens["--border"], bg, 1.2, $"{theme.Name}/{mode} hairline border");
        AssertRatio(tokens["--text-color"], tokens["--sidebar-bg"], 4.5, $"{theme.Name}/{mode} text on sidebar");
        AssertRatio(tokens["--text-color"], tokens["--code-bg"], 4.5, $"{theme.Name}/{mode} text on code");
        AssertRatio(tokens["--text-color"], tokens["--accent-light"], 4.5, $"{theme.Name}/{mode} text on accent tint");

        // Only when both are literals; otherwise they alias tokens the assertions above already cover.
        if (tokens.TryGetValue("--promo-bg", out var promoBg) && promoBg.StartsWith('#')
            && tokens.TryGetValue("--promo-text", out var promoText) && promoText.StartsWith('#'))
            AssertRatio(promoText, promoBg, 4.5, $"{theme.Name}/{mode} promo bar");
    }

    private static Dictionary<string, string> Merge(
        IReadOnlyDictionary<string, string> baseTokens,
        IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(baseTokens, StringComparer.Ordinal);
        foreach (var (key, value) in overrides)
            merged[key] = value;
        return merged;
    }

    private static void AssertRatio(string foreground, string background, double floor, string label)
    {
        var ratio = ContrastRatio(foreground, background);
        Assert.True(ratio >= floor, $"{label}: {foreground} on {background} is {ratio:F2}:1, needs {floor}:1");
    }

    internal static double ContrastRatio(string a, string b)
    {
        var (high, low) = (RelativeLuminance(a), RelativeLuminance(b));
        if (low > high)
            (high, low) = (low, high);
        return (high + 0.05) / (low + 0.05);
    }

    private static double RelativeLuminance(string hex)
    {
        var value = hex.TrimStart('#');
        if (value.Length == 3)
            value = string.Concat(value.Select(c => new string(c, 2)));

        var r = Channel(value[..2]);
        var g = Channel(value.Substring(2, 2));
        var b = Channel(value.Substring(4, 2));
        return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);

        static double Channel(string pair)
        {
            var srgb = int.Parse(pair, NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
            return srgb <= 0.03928 ? srgb / 12.92 : Math.Pow((srgb + 0.055) / 1.055, 2.4);
        }
    }
}

public sealed partial class ThemeCssIntegrityTests
{
    /// <summary>Set inline by the syntax highlighter on the elements that read them, not by any theme.</summary>
    private static readonly string[] ExternallyDefined =
        ["--shiki-light", "--shiki-dark", "--shiki-light-bg", "--shiki-dark-bg"];

    public static TheoryData<string> ThemeNames() => [.. ThemeRegistry.All.Select(t => t.Name)];

    /// <summary>A theme dropping a variable the base stylesheet reads renders as an unstyled element, not an error.</summary>
    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void EveryReferencedVariableIsDefined(string name)
    {
        var css = RenderStyleBlock(ThemeRegistry.Resolve(name));

        var defined = DefinitionPattern().Matches(css).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        var referenced = ReferencePattern().Matches(css).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        referenced.ExceptWith(ExternallyDefined);
        referenced.ExceptWith(defined);

        Assert.Empty(referenced);
    }

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void ThemeCssStaysInsideTheSingleNoncedStyleElement(string name)
    {
        var html = Render(ThemeRegistry.Resolve(name), nonce: "test-nonce");
        Assert.Equal(1, StylePattern().Count(html));
        Assert.Contains("<style nonce=\"test-nonce\">", html, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void ThemeClassIsOnTheHtmlElement(string name)
    {
        var html = Render(ThemeRegistry.Resolve(name));
        Assert.Contains($"class=\"theme-{name}\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public void DarkModeDisabled_EmitsNoDarkBlock()
    {
        var css = ThemeCssBuilder.BuildTokenCss(ThemeRegistry.Default, enableDarkMode: false);
        Assert.DoesNotContain("prefers-color-scheme", css, StringComparison.Ordinal);
        Assert.DoesNotContain("data-theme", css, StringComparison.Ordinal);
    }

    [Fact]
    public void DarkModeEnabled_EmitsBothOsQueryAndExplicitToggle()
    {
        var css = ThemeCssBuilder.BuildTokenCss(ThemeRegistry.Default, enableDarkMode: true);
        Assert.Contains("@media (prefers-color-scheme: dark)", css, StringComparison.Ordinal);
        Assert.Contains(":root[data-theme=\"dark\"]", css, StringComparison.Ordinal);
    }

    private static string Render(IBarkTheme theme, string? nonce = null) =>
        LayoutProvider.GetLayout(
            title: "Test",
            content: "<p>body</p>",
            navigationHtml: "",
            tocHtml: null,
            breadcrumbHtml: "",
            paginationHtml: "",
            nonce: nonce,
            theme: theme);

    private static string RenderStyleBlock(IBarkTheme theme)
    {
        var html = Render(theme);
        var match = StyleBlockPattern().Match(html);
        Assert.True(match.Success, "layout emitted no <style> block");
        return match.Groups[1].Value;
    }

    [GeneratedRegex(@"(--[a-z0-9-]+)\s*:")]
    private static partial Regex DefinitionPattern();

    [GeneratedRegex(@"var\((--[a-z0-9-]+)")]
    private static partial Regex ReferencePattern();

    [GeneratedRegex(@"<style[^>]*>")]
    private static partial Regex StylePattern();

    [GeneratedRegex(@"<style[^>]*>(.*?)</style>", RegexOptions.Singleline)]
    private static partial Regex StyleBlockPattern();
}
