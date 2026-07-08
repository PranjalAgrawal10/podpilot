using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Represents a compute provider configured for an organization.
/// </summary>
public class ComputeProvider : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the owning organization identifier.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the internal provider name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public ProviderType ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the display name shown in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default region identifier.
    /// </summary>
    public string? DefaultRegion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether credentials were validated.
    /// </summary>
    public bool IsValidated { get; set; }

    /// <summary>
    /// Gets or sets when credentials were last validated.
    /// </summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>
    /// Gets the owning organization.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Gets the encrypted credential for this provider.
    /// </summary>
    public ProviderCredential? Credential { get; set; }

    /// <summary>
    /// Gets the current health snapshot.
    /// </summary>
    public ProviderHealth? Health { get; set; }

    /// <summary>
    /// Gets cached regions for this provider.
    /// </summary>
    public ICollection<ProviderRegion> Regions { get; set; } = [];

    /// <summary>
    /// Gets cached GPUs for this provider.
    /// </summary>
    public ICollection<ProviderGpu> Gpus { get; set; } = [];

    /// <summary>
    /// Gets health check history entries.
    /// </summary>
    public ICollection<ProviderHealthHistory> HealthHistory { get; set; } = [];
}
