namespace PodPilot.Domain.Entities;

/// <summary>
/// Stores encrypted credentials for a compute provider.
/// </summary>
public class ProviderCredential : Common.AuditableEntity
{
    /// <summary>
    /// Gets or sets the compute provider identifier.
    /// </summary>
    public Guid ComputeProviderId { get; set; }

    /// <summary>
    /// Gets or sets the encrypted API key payload.
    /// </summary>
    public string EncryptedApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets the compute provider.
    /// </summary>
    public ComputeProvider ComputeProvider { get; set; } = null!;
}
