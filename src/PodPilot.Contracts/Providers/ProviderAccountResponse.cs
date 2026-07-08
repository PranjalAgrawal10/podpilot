namespace PodPilot.Contracts.Providers;

/// <summary>
/// Provider account response.
/// </summary>
public sealed class ProviderAccountResponse
{
    /// <summary>
    /// Gets or sets the account identifier.
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
    /// Gets or sets the account balance.
    /// </summary>
    public decimal? Balance { get; init; }

    /// <summary>
    /// Gets or sets the balance currency code.
    /// </summary>
    public string? Currency { get; init; }
}
