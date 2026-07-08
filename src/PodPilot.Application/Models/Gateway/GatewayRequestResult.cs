namespace PodPilot.Application.Models.Gateway;

/// <summary>
/// Result of handling a gateway request.
/// </summary>
public sealed class GatewayRequestResult
{
    /// <summary>
    /// Gets or sets whether the request succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets response headers from the upstream proxy.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the error code when unsuccessful.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets or sets the client-facing error message when unsuccessful.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
