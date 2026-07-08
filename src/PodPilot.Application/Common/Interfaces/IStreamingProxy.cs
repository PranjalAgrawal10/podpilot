using PodPilot.Application.Models.Gateway;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Proxies streaming HTTP requests to inference backends.
/// </summary>
public interface IStreamingProxy
{
    /// <summary>
    /// Forwards a request and streams the response to the output stream.
    /// </summary>
    Task<GatewayProxyResult> ForwardAsync(
        GatewayProxyOptions options,
        Stream responseBody,
        Func<GatewayProxyResult, Task>? onResponseHeaders = null,
        CancellationToken cancellationToken = default);
}
