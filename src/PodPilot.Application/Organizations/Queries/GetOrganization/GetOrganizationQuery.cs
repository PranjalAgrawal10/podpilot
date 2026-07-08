using MediatR;
using PodPilot.Contracts.Organizations;

namespace PodPilot.Application.Organizations.Queries.GetOrganization;

/// <summary>
/// Query to get an organization by identifier.
/// </summary>
public sealed class GetOrganizationQuery : IRequest<OrganizationResponse>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }
}
