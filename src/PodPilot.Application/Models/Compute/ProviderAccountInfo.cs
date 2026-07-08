namespace PodPilot.Application.Models.Compute;

/// <summary>
/// Account information returned by a compute provider.
/// </summary>
public sealed class ProviderAccountInfo
{
    /// <summary>
    /// Gets or sets the provider account identifier.
    /// </summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the account email.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets the account display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets or sets the account balance if available.
    /// </summary>
    public decimal? Balance { get; init; }

    /// <summary>
    /// Gets or sets the balance currency code.
    /// </summary>
    public string? Currency { get; init; }
}
