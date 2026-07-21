using System.Text.Json.Serialization;

namespace Bark.Models;

public sealed class ExtensionsFile
{
    public ExtensionsSection? Extensions { get; set; }
}

public sealed class ExtensionsSection
{
    public MatomoOptions? Matomo { get; set; }
    public PlausibleOptions? Plausible { get; set; }
    public MedamaOptions? Medama { get; set; }
    public GoatCounterOptions? GoatCounter { get; set; }
    public LiwanOptions? Liwan { get; set; }
}

public sealed class MatomoOptions
{
    public bool Enabled { get; set; }
    public string? Url { get; set; }
    public string? SiteId { get; set; }

    [JsonPropertyName("site_id")]
    public string? SiteIdAlias { get; set; }

    public bool DisableCookies { get; set; } = true;
}

public sealed class PlausibleOptions
{
    public bool Enabled { get; set; }
    public string? Domain { get; set; }
    public string? Url { get; set; }
    public string? Script { get; set; }
}

public sealed class GoatCounterOptions
{
    public bool Enabled { get; set; }
    public string? Url { get; set; }
}

public sealed class MedamaOptions
{
    public bool Enabled { get; set; }
    public string? Url { get; set; }
}

public sealed class LiwanOptions
{
    public bool Enabled { get; set; }
    public string? Url { get; set; }
    public string? Entity { get; set; }
}
