namespace PodPilot.Contracts.Gateway;

/// <summary>
/// Request to create a gateway route.
/// </summary>
public sealed class CreateGatewayRouteRequest
{
    /// <summary>
    /// Gets or sets the target pod identifier.
    /// </summary>
    public Guid GpuPodId { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the default route.
    /// </summary>
    public bool IsDefault { get; set; }
}
