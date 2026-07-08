namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Routes gateway requests to the appropriate GPU pod.
/// </summary>
public interface IGatewayRouter
{
    /// <summary>
    /// Resolves the target pod for a gateway request.
    /// </summary>
    Task<Models.Gateway.GatewayRouteResult> ResolveAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken = default);
}
