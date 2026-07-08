using MediatR;
using PodPilot.Contracts.Members;

namespace PodPilot.Application.Members.Commands.UpdateMemberRole;

/// <summary>
/// Command to update a member's organization role.
/// </summary>
public sealed class UpdateMemberRoleCommand : IRequest<MemberResponse>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the membership identifier.
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Gets or sets the new role.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
