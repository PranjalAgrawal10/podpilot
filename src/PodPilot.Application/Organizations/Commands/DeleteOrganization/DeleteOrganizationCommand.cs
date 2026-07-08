using MediatR;

namespace PodPilot.Application.Organizations.Commands.DeleteOrganization;

/// <summary>
/// Command to delete an organization.
/// </summary>
public sealed class DeleteOrganizationCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }
}
