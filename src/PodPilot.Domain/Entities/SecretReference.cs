using PodPilot.Domain.Enums;

namespace PodPilot.Domain.Entities;

/// <summary>
/// Reference to an organization secret stored in a backend (never plaintext).
/// </summary>
public class SecretReference : Common.AuditableEntity
{
    /// <summary>Gets or sets the organization identifier.</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Gets or sets the secret name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the secret kind.</summary>
    public SecretKind SecretKind { get; set; }

    /// <summary>Gets or sets the backend kind.</summary>
    public SecretBackendKind BackendKind { get; set; } = SecretBackendKind.LocalEncrypted;

    /// <summary>Gets or sets the backend locator (vault path / ARN / local id).</summary>
    public string BackendLocator { get; set; } = string.Empty;

    /// <summary>Gets or sets the locally encrypted value when using local backend.</summary>
    public string? EncryptedValue { get; set; }

    /// <summary>Gets or sets when the secret expires.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Gets or sets when the secret was last rotated.</summary>
    public DateTime? LastRotatedAt { get; set; }

    /// <summary>Gets or sets when the secret was last accessed.</summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>Gets or sets whether the secret is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets a version stamp for rotation.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Gets the organization.</summary>
    public Organization Organization { get; set; } = null!;
}
