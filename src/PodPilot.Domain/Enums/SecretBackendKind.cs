namespace PodPilot.Domain.Enums;

/// <summary>
/// Backing store for organization secrets.
/// </summary>
public enum SecretBackendKind
{
    /// <summary>Local encrypted storage in PodPilot database.</summary>
    LocalEncrypted = 0,

    /// <summary>Azure Key Vault.</summary>
    AzureKeyVault = 1,

    /// <summary>AWS Secrets Manager.</summary>
    AwsSecretsManager = 2,

    /// <summary>HashiCorp Vault.</summary>
    HashiCorpVault = 3,
}
