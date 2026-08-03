using System.Net;

namespace Bark.Configuration;

/// <summary>
/// Decides whether a request really came from the machine Bark runs on, for controls that expose
/// server-side detail (the "Open in editor" links carry the absolute docs root path).
/// </summary>
/// <remarks>
/// A loopback <c>RemoteIpAddress</c> alone is not enough: the usual production shape is a reverse
/// proxy on the same host forwarding to Kestrel over loopback, which makes every visitor look local.
/// Bark registers no forwarded-headers middleware, so the connection IP is the proxy's either way.
/// The request must therefore also carry no proxy hop markers and address Bark by a loopback host.
/// </remarks>
public static class LocalRequest
{
    private static readonly string[] ProxyHeaders =
        ["X-Forwarded-For", "X-Forwarded-Host", "X-Forwarded-Proto", "Forwarded", "X-Real-IP"];

    public static bool IsLocal(HttpContext context)
    {
        if (context.Connection.RemoteIpAddress is not { } remoteIp || !IPAddress.IsLoopback(remoteIp))
            return false;

        foreach (var header in ProxyHeaders)
            if (context.Request.Headers.ContainsKey(header))
                return false;

        return IsLoopbackHost(context.Request.Host.Host);
    }

    private static bool IsLoopbackHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("::1", StringComparison.Ordinal)
        || (IPAddress.TryParse(host, out var parsed) && IPAddress.IsLoopback(parsed));
}
