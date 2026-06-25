namespace Bark.Configuration;

public static class SecurityHeaders
{
    // Inline <style>/<script> blocks (LayoutProvider.Styles.cs/.Scripts.cs) and KaTeX/Mermaid CDN
    // assets (LayoutProvider.cs) require 'unsafe-inline' and jsdelivr. Code-group tab icons
    // (CodeGroupIconOptions) are vendored locally under wwwroot/icons by default, so img-src stays
    // self-only -- only widen it if BaseUrl is deliberately pointed at a CDN.
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "font-src 'self' data: https://cdn.jsdelivr.net; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'";

    public static Task Apply(HttpContext context, Func<Task> next)
    {
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers.ContentSecurityPolicy = ContentSecurityPolicy;
        return next();
    }
}
