using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Context for scheduling a gateway request.
/// </summary>
public sealed class ScheduleRequestContext
{
    /// <summary>
    /// Gets or sets the authentication context.
    /// </summary>
    public GatewayAuthContext Auth { get; init; } = null!;

    /// <summary>
    /// Gets or sets the upstream path.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets request headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the buffered request body.
    /// </summary>
    public MemoryStream Body { get; init; } = null!;

    /// <summary>
    /// Gets or sets the response stream.
    /// </summary>
    public Stream ResponseBody { get; init; } = null!;

    /// <summary>
    /// Gets or sets the error format.
    /// </summary>
    public GatewayErrorFormat ErrorFormat { get; init; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets or sets the client request identifier.
    /// </summary>
    public string? ClientRequestId { get; init; }

    /// <summary>
    /// Gets or sets a callback for response headers.
    /// </summary>
    public Func<GatewayProxyResult, Task>? OnResponseHeaders { get; init; }
}
