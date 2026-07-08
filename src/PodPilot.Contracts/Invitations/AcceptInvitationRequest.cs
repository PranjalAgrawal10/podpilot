namespace PodPilot.Contracts.Invitations;

/// <summary>
/// Request to accept an organization invitation.
/// </summary>
public sealed class AcceptInvitationRequest
{
    /// <summary>
    /// Gets or sets the invitation token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
