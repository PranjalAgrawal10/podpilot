namespace PodPilot.Domain.Enums;

/// <summary>
/// Status of an organization membership.
/// </summary>
public enum MemberStatus
{
    /// <summary>
    /// Active member with full access per role.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Invited but not yet accepted.
    /// </summary>
    Invited = 1,

    /// <summary>
    /// Suspended from the organization.
    /// </summary>
    Suspended = 2,
}
