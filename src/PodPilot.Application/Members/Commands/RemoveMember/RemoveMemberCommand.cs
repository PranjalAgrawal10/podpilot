using MediatR;

namespace PodPilot.Application.Members.Commands.RemoveMember;

/// <summary>
/// Command to remove a member from an organization.
/// </summary>
public sealed class RemoveMemberCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the membership identifier.
    /// </summary>
    public Guid MemberId { get; set; }
}
