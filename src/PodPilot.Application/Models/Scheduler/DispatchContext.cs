using PodPilot.Application.Models.Gateway;

namespace PodPilot.Application.Models.Scheduler;

/// <summary>
/// Context for dispatching a scheduled request.
/// </summary>
public sealed class DispatchContext
{
    /// <summary>
    /// Gets or sets the gateway request identifier.
    /// </summary>
    public Guid RequestId { get; init; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; init; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Gets or sets the API key identifier.
    /// </summary>
    public Guid? ApiKeyId { get; init; }

    /// <summary>
    /// Gets or sets the assigned pod identifier.
    /// </summary>
    public Guid PodId { get; init; }

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
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets request headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the request body.
    /// </summary>
    public Stream Body { get; init; } = Stream.Null;

    /// <summary>
    /// Gets or sets the response body stream.
    /// </summary>
    public Stream? ResponseBody { get; init; }

    /// <summary>
    /// Gets or sets the resolved model name.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Gets or sets the attempt number.
    /// </summary>
    public int AttemptNumber { get; init; } = 1;

    /// <summary>
    /// Gets or sets a callback for response headers.
    /// </summary>
    public Func<GatewayProxyResult, Task>? OnResponseHeaders { get; init; }
}
