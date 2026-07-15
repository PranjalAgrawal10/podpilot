using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PodPilot.Application.Common;

namespace PodPilot.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for enterprise security events.
/// </summary>
[Authorize]
public sealed class SecurityHub : Hub
{
    /// <summary>Gets the organization group name.</summary>
    public static string GetOrganizationGroupName(Guid organizationId) => $"org-{organizationId}";

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
