using MediatR;
using PodPilot.Contracts.Invitations;

namespace PodPilot.Application.Invitations.Commands.InviteMember;

/// <summary>
/// Command to invite a user to an organization.
/// </summary>
public sealed class InviteMemberCommand : IRequest<InvitationResponse>
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the email address to invite.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role to grant upon acceptance.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
