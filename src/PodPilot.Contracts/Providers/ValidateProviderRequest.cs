namespace PodPilot.Contracts.Providers;

/// <summary>
/// Request to validate provider credentials.
/// </summary>
public sealed class ValidateProviderRequest
{
    /// <summary>
    /// Gets or sets an optional API key to validate instead of the stored credential.
    /// </summary>
    public string? ApiKey { get; set; }
}
