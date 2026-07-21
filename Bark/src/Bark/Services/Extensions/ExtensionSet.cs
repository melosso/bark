namespace Bark.Services.Extensions;

public sealed record ExtensionScript(
    string? Src = null,
    string? Inline = null,
    bool Async = false,
    bool Defer = false,
    IReadOnlyList<KeyValuePair<string, string>>? Attributes = null);

public sealed record ActiveExtension(
    string Name,
    IReadOnlyList<ExtensionScript> Scripts,
    IReadOnlyList<string> CspSources);

public interface IExtensionSource
{
    ExtensionSet Extensions { get; }
}

public sealed record ExtensionSet(
    IReadOnlyList<ActiveExtension> Active,
    IReadOnlyList<string>? Invalid = null)
{
    public static readonly ExtensionSet Empty = new([]);

    public bool IsEmpty => Active.Count == 0;

    public IReadOnlyList<string> Rejected { get; } = Invalid ?? [];

    public IReadOnlyList<string> CspSources { get; } = Active.SelectMany(e => e.CspSources)
        .Distinct(StringComparer.Ordinal).ToArray();

    public string Signature { get; } = string.Join(",", Active.Select(e => e.Name));
}
