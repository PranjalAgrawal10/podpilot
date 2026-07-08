namespace PodPilot.Contracts.Organizations;

/// <summary>
/// Request to update an organization.
/// </summary>
public sealed class UpdateOrganizationRequest
{
    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional logo URL.
    /// </summary>
    public string? Logo { get; set; }
}
