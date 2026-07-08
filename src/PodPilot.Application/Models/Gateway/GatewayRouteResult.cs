using PodPilot.Domain.Entities;

namespace PodPilot.Application.Models.Gateway;

/// <summary>
/// Routing decision for a gateway request.
/// </summary>
public sealed class GatewayRouteResult
{
    /// <summary>
    /// Gets or sets the target pod.
    /// </summary>
    public GpuPod Pod { get; init; } = null!;

    /// <summary>
    /// Gets or sets the resolved model name.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Gets or sets the Ollama base URL.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;
}
