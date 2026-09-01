using System.Text.Json;
using System.Text.RegularExpressions;
using Bark.Services.Rendering;

namespace Bark.Tests;

public sealed class LocaleCoverageTests
{
    private static readonly string LocaleDir = Path.Combine(AppContext.BaseDirectory, "docs", "locale");

    private static readonly Regex PlaceholderPattern = new(@"\{\d+\}", RegexOptions.Compiled);

    public static TheoryData<string> LocaleFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in Directory.GetFiles(LocaleDir, "*.json"))
            data.Add(Path.GetFileName(path));
        return data;
    }

    private static Dictionary<string, JsonElement> ReadRaw(string file) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(Path.Combine(LocaleDir, file)))!;

    private static Dictionary<string, string> Read(string file) =>
        ReadRaw(file)
            .Where(pair => pair.Value.ValueKind == JsonValueKind.String)
            .ToDictionary(pair => pair.Key, pair => pair.Value.GetString()!, StringComparer.Ordinal);

    [Fact]
    public void EveryShippedLanguageHasALocaleFile()
    {
        var codes = Directory.GetFiles(LocaleDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f)!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Superset(new HashSet<string> { "en", "nl" }, codes);
    }

    [Theory]
    [MemberData(nameof(LocaleFiles))]
    public void LocaleCoversEveryStringKey(string file)
    {
        var translated = Read(file);
        var missing = Localization.Keys.Where(k => !translated.ContainsKey(k)).Order().ToArray();

        Assert.True(missing.Length == 0, $"{file} is missing: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(LocaleFiles))]
    public void LocaleCarriesNoUnknownKey(string file)
    {
        var translated = Read(file);
        var known = Localization.Keys.ToHashSet(StringComparer.Ordinal);
        var stray = translated.Keys.Where(k => !known.Contains(k)).Order().ToArray();

        Assert.True(stray.Length == 0, $"{file} has keys no longer in the table: {string.Join(", ", stray)}");
    }

    [Theory]
    [MemberData(nameof(LocaleFiles))]
    public void LocaleKeepsThePlaceholdersOfTheEnglishSource(string file)
    {
        var english = Read("en.json");
        var translated = Read(file);

        foreach (var (key, value) in translated)
        {
            if (!english.TryGetValue(key, out var source))
                continue;

            var expected = PlaceholderPattern.Matches(source).Select(m => m.Value).Order().ToArray();
            var actual = PlaceholderPattern.Matches(value).Select(m => m.Value).Order().ToArray();

            Assert.True(expected.SequenceEqual(actual),
                $"{file} key {key} has placeholders [{string.Join(", ", actual)}], expected [{string.Join(", ", expected)}]");
        }
    }

    [Theory]
    [MemberData(nameof(LocaleFiles))]
    public void ConfigLabelsAreNonEmptyStrings(string file)
    {
        if (!ReadRaw(file).TryGetValue(Localization.LabelSection, out var section))
            return;

        Assert.Equal(JsonValueKind.Object, section.ValueKind);

        var blank = section.EnumerateObject()
            .Where(label => label.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(label.Value.GetString()))
            .Select(label => label.Name)
            .Order()
            .ToArray();

        Assert.True(blank.Length == 0, $"{file} has blank config labels: {string.Join(", ", blank)}");
    }

    [Theory]
    [MemberData(nameof(LocaleFiles))]
    public void LocaleHasNoEmptyValue(string file)
    {
        var empty = Read(file).Where(pair => string.IsNullOrWhiteSpace(pair.Value)).Select(pair => pair.Key).Order().ToArray();

        Assert.True(empty.Length == 0, $"{file} has empty values: {string.Join(", ", empty)}");
    }
}
