using MediatR;
using PodPilot.Contracts.Organizations;

namespace PodPilot.Application.Organizations.Commands.CreateOrganization;

/// <summary>
/// Command to create a new organization.
/// </summary>
public sealed class CreateOrganizationCommand : IRequest<OrganizationResponse>
{
    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional logo URL.
    /// </summary>
    public string? Logo { get; set; }
}
