using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common;

namespace PodPilot.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time gateway events.
/// </summary>
[Authorize]
public sealed class GatewayHub : Hub
{
    /// <summary>
    /// Gets the SignalR group name for an organization.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <returns>The group name.</returns>
    public static string GetOrganizationGroupName(Guid organizationId) => $"gateway-org-{organizationId}";

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var organizationId = Context.User?.FindFirst(ApplicationConstants.OrganizationIdClaim)?.Value;
        if (Guid.TryParse(organizationId, out var orgId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetOrganizationGroupName(orgId));
        }

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var organizationId = Context.User?.FindFirst(ApplicationConstants.OrganizationIdClaim)?.Value;
        if (Guid.TryParse(organizationId, out var orgId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetOrganizationGroupName(orgId));
        }

        await base.OnDisconnectedAsync(exception);
    }
}
