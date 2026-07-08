namespace PodPilot.Application.Models.Compute;

/// <summary>
/// Template information from a compute provider.
/// </summary>
public sealed class ProviderTemplateInfo
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
