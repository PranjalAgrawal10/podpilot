namespace PodPilot.Contracts.Invitations;

/// <summary>
/// Organization invitation response.
/// </summary>
public sealed class InvitationResponse
{
    /// <summary>
    /// Gets or sets the invitation identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the invited email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role granted upon acceptance.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invitation status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the invitation token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
