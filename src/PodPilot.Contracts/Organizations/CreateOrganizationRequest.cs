namespace PodPilot.Contracts.Organizations;

/// <summary>
/// Request to create a new organization.
/// </summary>
public sealed class CreateOrganizationRequest
{
    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional logo URL.
    /// </summary>
    public string? Logo { get; set; }
}
