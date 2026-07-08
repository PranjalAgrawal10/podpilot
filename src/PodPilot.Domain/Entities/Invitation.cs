using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents an invitation to join an organization.
/// </summary>
public class Invitation : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets or sets the invited email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invitation token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the invitation status.
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>
    /// Gets or sets the role granted upon acceptance.
    /// </summary>
    public OrganizationRole Role { get; set; } = OrganizationRole.Viewer;

    /// <summary>
    /// Gets or sets when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
}
