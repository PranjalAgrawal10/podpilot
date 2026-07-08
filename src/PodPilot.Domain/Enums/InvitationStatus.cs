namespace PodPilot.Domain.Enums;

/// <summary>
/// Status of an organization invitation.
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitation is pending acceptance.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Invitation was accepted.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Invitation was revoked.
    /// </summary>
    Revoked = 2,

    /// <summary>
    /// Invitation expired before acceptance.
    /// </summary>
    Expired = 3,
}
