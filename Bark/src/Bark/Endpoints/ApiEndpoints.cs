using Microsoft.AspNetCore.Http.HttpResults;
using Bark.Configuration;
using Bark.Models;
using Bark.Services;

namespace Bark.Endpoints;

internal static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        api.MapGet("/search", Search).RequireRateLimiting(RateLimitPolicies.Search);
        api.MapGet("/pages", GetPages).RequireRateLimiting(RateLimitPolicies.Search);
        // NOT rate-limited; the hot-reload script polls this every few seconds
        api.MapGet("/build-version", GetBuildVersion);
        return app;
    }

    internal static Ok<IReadOnlyList<SearchResult>> Search(string? q, DocumentationService docs)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return TypedResults.Ok<IReadOnlyList<SearchResult>>(Array.Empty<SearchResult>());

        return TypedResults.Ok(docs.Search(q));
    }

    // Public page metadata only; no OriginalRelativePath or other server file paths
    internal static async Task<Ok<List<PageSummary>>> GetPages(DocumentationService docs, CancellationToken cancellationToken)
    {
        var pages = await docs.GetAllPagesAsync(cancellationToken);
        var items = pages
            .OrderBy(p => p.Path)
            .Select(p => new PageSummary(p.Path, p.Title, p.Description, p.LastModified))
            .ToList();
        return TypedResults.Ok(items);
    }

    internal static Ok<BuildVersionResponse> GetBuildVersion(HttpContext context, DocumentationService docs)
    {
        // "no-store" not just "no-cache"; the hot-reload poll needs the live value every time.
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        return TypedResults.Ok(new BuildVersionResponse(docs.BuildVersion));
    }
}
