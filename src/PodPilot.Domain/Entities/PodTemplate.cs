namespace PodPilot.Domain.Entities;

/// <summary>
/// Snapshot of a provider template used when creating a pod.
/// </summary>
public class PodTemplate
{
    /// <summary>
    /// Gets or sets the provider template identifier.
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the container image name.
    /// </summary>
    public string? ImageName { get; set; }

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }
}
