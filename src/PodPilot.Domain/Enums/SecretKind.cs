namespace PodPilot.Domain.Enums;

/// <summary>
/// Classification of a managed secret.
/// </summary>
public enum SecretKind
{
    /// <summary>Compute provider API key.</summary>
    ProviderApiKey = 0,

    /// <summary>AI inference provider credential.</summary>
    AiProviderCredential = 1,

    /// <summary>JWT signing key material.</summary>
    JwtSigningKey = 2,

    /// <summary>Database password.</summary>
    DatabasePassword = 3,

    /// <summary>Redis password.</summary>
    RedisPassword = 4,

    /// <summary>Generic secret.</summary>
    Generic = 5,
}
