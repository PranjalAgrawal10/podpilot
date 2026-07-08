namespace PodPilot.Contracts.Providers;

/// <summary>
/// Provider template response.
/// </summary>
public sealed class ProviderTemplateResponse
{
    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    public string TemplateId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the container image name.
    /// </summary>
    public string? ImageName { get; init; }

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; init; }
}
