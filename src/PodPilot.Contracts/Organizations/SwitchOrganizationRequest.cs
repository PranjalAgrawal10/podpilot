namespace PodPilot.Contracts.Organizations;

/// <summary>
/// Request to switch the current organization context.
/// </summary>
public sealed class SwitchOrganizationRequest
{
    /// <summary>
    /// Gets or sets the target organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }
}
