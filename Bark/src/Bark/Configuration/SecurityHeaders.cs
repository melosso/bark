namespace Bark.Configuration;

public static class SecurityHeaders
{
    // Inline <style>/<script> blocks (LayoutProvider.Styles.cs/.Scripts.cs), KaTeX/Mermaid CDN assets
    // (LayoutProvider.cs), and code-group tab icons (CodeGroupIconOptions) need jsdelivr.
    // Point CodeGroupIconOptions.BaseUrl at a local path instead to drop this img-src dependency.
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https://cdn.jsdelivr.net; " +
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
