using MediatR;
using PodPilot.Contracts.Auth;

namespace PodPilot.Application.Organizations.Commands.SwitchOrganization;

/// <summary>
/// Command to switch the current organization context.
/// </summary>
public sealed class SwitchOrganizationCommand : IRequest<AuthResponse>
{
    /// <summary>
    /// Gets or sets the target organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }
}
