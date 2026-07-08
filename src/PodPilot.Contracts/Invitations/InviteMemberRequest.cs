namespace PodPilot.Contracts.Invitations;

/// <summary>
/// Request to invite a user to an organization.
/// </summary>
public sealed class InviteMemberRequest
{
    /// <summary>
    /// Gets or sets the email address to invite.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role to grant upon acceptance.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
