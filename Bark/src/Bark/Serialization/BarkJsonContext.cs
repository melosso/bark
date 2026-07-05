using System.Text.Json.Serialization;
using Bark.Models;

namespace Bark.Serialization;

// Source-generated JSON metadata for API response types; no runtime reflection, AOT-ready; unknown types fall back to reflection via the resolver chain
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<PageSummary>))]
[JsonSerializable(typeof(BuildVersionResponse))]
[JsonSerializable(typeof(IReadOnlyList<SearchResult>))]
[JsonSerializable(typeof(SearchResult[]))]
internal sealed partial class BarkJsonContext : JsonSerializerContext;
