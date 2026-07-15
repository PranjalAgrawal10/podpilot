namespace PodPilot.Domain.Entities;

/// <summary>
/// Encrypted credentials for an AI inference provider.
/// </summary>
public class AiProviderCredential : Common.AuditableEntity
{
    /// <summary>Gets or sets the AI provider identifier.</summary>
    public Guid AiProviderId { get; set; }

    /// <summary>Gets or sets the encrypted API key.</summary>
    public string EncryptedApiKey { get; set; } = string.Empty;

    /// <summary>Gets the AI provider.</summary>
    public AiInferenceProvider AiProvider { get; set; } = null!;
}
