namespace Bark.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class LocaleOptions
{
    public string? Code { get; set; }
}

public sealed class LocaleOptionsConverter : JsonConverter<LocaleOptions>
{
    public override LocaleOptions? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new LocaleOptions { Code = reader.GetString() };

        return JsonSerializer.Deserialize<LocaleOptions>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, LocaleOptions value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}
