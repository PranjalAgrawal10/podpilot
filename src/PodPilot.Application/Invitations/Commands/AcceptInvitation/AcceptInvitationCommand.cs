using MediatR;
using PodPilot.Contracts.Members;

namespace PodPilot.Application.Invitations.Commands.AcceptInvitation;

/// <summary>
/// Command to accept an organization invitation.
/// </summary>
public sealed class AcceptInvitationCommand : IRequest<MemberResponse>
{
    /// <summary>
    /// Gets or sets the invitation token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
