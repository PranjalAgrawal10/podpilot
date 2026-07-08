namespace PodPilot.Contracts.Gateway;

/// <summary>
/// Gateway route response.
/// </summary>
public sealed class GatewayRouteResponse
{
    /// <summary>
    /// Gets or sets the route identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the pod name.
    /// </summary>
    public string PodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the default route.
    /// </summary>
    public bool IsDefault { get; set; }
}
