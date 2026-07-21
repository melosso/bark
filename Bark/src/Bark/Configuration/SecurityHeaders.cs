using Microsoft.AspNetCore.Builder;

namespace Bark.Configuration;

public static class SecurityHeaders
{
    public const string DefaultCsp =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "style-src-attr 'unsafe-inline'; " +
        "style-src-elem 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'";

    public static Task Apply(HttpContext context, Func<Task> next) =>
        Apply(context, next, DefaultCsp);

    public static Task Apply(HttpContext context, Func<Task> next, string contentSecurityPolicy)
    {
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers.ContentSecurityPolicy = contentSecurityPolicy;
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";

        if (context.Request.IsHttps)
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        return next();
    }

    private static readonly string[] WidenedDirectives =
        ["script-src ", "connect-src ", "img-src ", "frame-src "];

    public static string WithExtraSources(string csp, IReadOnlyList<string> origins)
    {
        if (origins.Count == 0)
            return csp;

        var extra = " " + string.Join(' ', origins);
        var directives = csp.Split(';');
        var seen = new bool[WidenedDirectives.Length];

        for (var i = 0; i < directives.Length; i++)
        {
            var trimmed = directives[i].TrimStart();
            for (var d = 0; d < WidenedDirectives.Length; d++)
            {
                if (trimmed.StartsWith(WidenedDirectives[d], StringComparison.Ordinal))
                {
                    directives[i] = directives[i].TrimEnd() + extra;
                    seen[d] = true;
                    break;
                }
            }
        }

        var result = new List<string>(directives);
        for (var d = 0; d < WidenedDirectives.Length; d++)
            if (!seen[d])
                result.Add(" " + WidenedDirectives[d].TrimEnd() + extra);

        return string.Join(";", result);
    }

    public static string BuildNonceCsp(string baseCsp, string nonce)
    {
        var noncePart = $"'nonce-{nonce}'";
        var directives = baseCsp.Split(';');
        for (var i = 0; i < directives.Length; i++)
        {
            var trimmed = directives[i].TrimStart();
            if ((trimmed.StartsWith("script-src ") || (trimmed.StartsWith("style-src ") && !trimmed.StartsWith("style-src-attr")))
                && trimmed.Contains("'unsafe-inline'"))
                directives[i] = directives[i].Replace("'unsafe-inline'", noncePart);
        }
        return string.Join(";", directives);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, string? contentSecurityPolicy = null)
    {
        var csp = contentSecurityPolicy ?? SecurityHeaders.DefaultCsp;
        return app.Use((context, next) => SecurityHeaders.Apply(context, next, csp));
    }
}
