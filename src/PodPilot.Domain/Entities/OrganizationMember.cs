using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a user's membership in an organization.
/// </summary>
public class OrganizationMember : Common.AuditableEntity
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
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the member's role within the organization.
    /// </summary>
    public OrganizationRole Role { get; set; } = OrganizationRole.Viewer;

    /// <summary>
    /// Gets or sets when the user joined the organization.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the membership status.
    /// </summary>
    public MemberStatus Status { get; set; } = MemberStatus.Active;

    /// <summary>
    /// Gets or sets a value indicating whether the membership is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
