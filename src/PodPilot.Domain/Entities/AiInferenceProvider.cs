using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Organization-scoped AI inference provider configuration.
/// </summary>
public class AiInferenceProvider : Common.AuditableEntity
{
    /// <summary>Gets or sets the owning organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the internal name (unique per organization).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the provider kind.</summary>
    public AiProviderKind ProviderKind { get; set; }

    /// <summary>Gets or sets the base URL override (required for self-hosted providers).</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Gets or sets an optional Azure/resource deployment name.</summary>
    public string? DeploymentName { get; set; }

    /// <summary>Gets or sets an optional API version (Azure OpenAI).</summary>
    public string? ApiVersion { get; set; }

    /// <summary>Gets or sets a value indicating whether the provider is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether credentials were validated.</summary>
    public bool IsValidated { get; set; }

    /// <summary>Gets or sets when credentials were last validated.</summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>Gets or sets the priority used for routing (lower is preferred).</summary>
    public int Priority { get; set; }

    /// <summary>Gets the owning organization.</summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>Gets the encrypted credential.</summary>
    public AiProviderCredential? Credential { get; set; }

    /// <summary>Gets the current health snapshot.</summary>
    public AiProviderHealth? Health { get; set; }

    /// <summary>Gets catalog models for this provider.</summary>
    public ICollection<AiProviderModel> Models { get; set; } = [];

    /// <summary>Gets routing policies that reference this provider.</summary>
    public ICollection<AiRoutingPolicy> RoutingPolicies { get; set; } = [];
}
