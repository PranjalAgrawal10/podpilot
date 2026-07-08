namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Filters hop-by-hop headers when proxying gateway responses.
/// </summary>
public static class GatewayHeaderFilter
{
    private static readonly HashSet<string> SkipHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailers",
        "Transfer-Encoding",
        "Upgrade",
        "Content-Length",
    };

    /// <summary>
    /// Determines whether a header should be skipped when copying upstream headers.
    /// </summary>
    public static bool ShouldSkip(string headerName) => SkipHeaders.Contains(headerName);
}
