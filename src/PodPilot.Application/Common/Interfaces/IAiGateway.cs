using PodPilot.Application.Models.Gateway;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Orchestrates AI gateway request handling.
/// </summary>
public interface IAiGateway
{
    /// <summary>
    /// Handles a proxied gateway request end-to-end.
    /// </summary>
    Task<GatewayRequestResult> HandleRequestAsync(
        GatewayAuthContext auth,
        string path,
        string method,
        IReadOnlyDictionary<string, string> headers,
        Stream requestBody,
        Stream responseBody,
        GatewayErrorFormat errorFormat,
        string? correlationId,
        Func<GatewayProxyResult, Task>? onResponseHeaders = null,
        CancellationToken cancellationToken = default);
}
