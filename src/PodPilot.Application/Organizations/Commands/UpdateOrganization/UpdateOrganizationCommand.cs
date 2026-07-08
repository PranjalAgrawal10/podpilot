using MediatR;
using PodPilot.Contracts.Organizations;

namespace PodPilot.Application.Organizations.Commands.UpdateOrganization;

/// <summary>
/// Command to update an organization.
/// </summary>
public sealed class UpdateOrganizationCommand : IRequest<OrganizationResponse>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional logo URL.
    /// </summary>
    public string? Logo { get; set; }
}
