namespace PodPilot.Application.Models.Gateway;

/// <summary>
/// Options for proxying a gateway request.
/// </summary>
public sealed class GatewayProxyOptions
{
    /// <summary>
    /// Gets or sets the upstream base URL.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public string Method { get; init; } = "POST";

    /// <summary>
    /// Gets or sets request headers to forward.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the request body stream.
    /// </summary>
    public Stream? Body { get; init; }

    /// <summary>
    /// Gets or sets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Gets or sets the forward timeout.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(10);
}
