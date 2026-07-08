namespace PodPilot.Application.Models.Gateway;

/// <summary>
/// Result of a proxied gateway request.
/// </summary>
public sealed class GatewayProxyResult
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Gets or sets response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets whether the response was streamed.
    /// </summary>
    public bool IsStreaming { get; init; }

    /// <summary>
    /// Gets or sets forward latency in milliseconds.
    /// </summary>
    public int ForwardLatencyMs { get; set; }
}
