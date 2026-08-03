using System.Security.Cryptography;
using System.Text;

namespace Bark.Configuration;

/// <summary>
/// Derives a secure, unpredictable CSP nonce from the ETag.
/// Keeps nonces unique per response while ensuring 304 Not Modified responses stay consistent.
/// </summary>
public static class CspNonce
{
    private static readonly byte[] Key = RandomNumberGenerator.GetBytes(32);

    public static readonly string ProcessSalt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    public static string Derive(string etag) =>
        Convert.ToBase64String(HMACSHA256.HashData(Key, Encoding.UTF8.GetBytes(etag)), 0, 16);
}

public static class SecurityHeaders
{
    public const string DefaultCsp =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "style-src-attr 'unsafe-inline'; " +
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
        context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        // same-site, not same-origin: docs images and fonts are routinely embedded from a sibling
        // host (blog.example.com pulling docs.example.com/assets), which same-origin would break.
        context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-site";
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

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

    /// <summary>
    /// Puts the per-response nonce into <c>script-src</c>/<c>style-src</c>. Swaps out
    /// <c>'unsafe-inline'</c> where it is present; where it is not — an operator who hardened
    /// <c>Docs:ContentSecurityPolicy</c> — the nonce is appended instead. Appending matters:
    /// silently skipping it would block Bark's own inline scripts and push the operator back
    /// to a policy weaker than the default.
    /// </summary>
    public static string BuildNonceCsp(string baseCsp, string nonce)
    {
        var noncePart = $"'nonce-{nonce}'";
        var directives = baseCsp.Split(';');
        for (var i = 0; i < directives.Length; i++)
        {
            var trimmed = directives[i].TrimStart();
            if (!trimmed.StartsWith("script-src ") && !trimmed.StartsWith("style-src "))
                continue;

            directives[i] = trimmed.Contains("'unsafe-inline'")
                ? directives[i].Replace("'unsafe-inline'", noncePart)
                : directives[i].TrimEnd() + " " + noncePart;
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
