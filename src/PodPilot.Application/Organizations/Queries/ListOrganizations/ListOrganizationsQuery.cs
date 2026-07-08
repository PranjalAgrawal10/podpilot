using MediatR;
using PodPilot.Contracts.Organizations;

namespace PodPilot.Application.Organizations.Queries.ListOrganizations;

/// <summary>
/// Query to list organizations for the current user.
/// </summary>
public sealed class ListOrganizationsQuery : IRequest<IReadOnlyList<OrganizationResponse>>
{
}
