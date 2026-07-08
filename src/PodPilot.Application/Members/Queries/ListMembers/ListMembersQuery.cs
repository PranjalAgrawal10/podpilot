using MediatR;
using PodPilot.Contracts.Members;

namespace PodPilot.Application.Members.Queries.ListMembers;

/// <summary>
/// Query to list members of an organization.
/// </summary>
public sealed class ListMembersQuery : IRequest<IReadOnlyList<MemberResponse>>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }
}
