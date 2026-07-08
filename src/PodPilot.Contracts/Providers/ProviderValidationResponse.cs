namespace PodPilot.Contracts.Providers;

/// <summary>
/// Provider validation response.
/// </summary>
public sealed class ProviderValidationResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether validation succeeded.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets or sets a user-facing validation message.
    /// </summary>
    public string? Message => ErrorMessage;

    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public string ConnectionStatus { get; init; } = "Unknown";

    /// <summary>
    /// Gets or sets optional account information.
    /// </summary>
    public ProviderAccountResponse? AccountInfo { get; init; }

    /// <summary>
    /// Gets or sets optional account information for clients using account alias.
    /// </summary>
    public ProviderAccountResponse? Account => AccountInfo;

    /// <summary>
    /// Gets or sets available regions.
    /// </summary>
    public IReadOnlyList<ProviderRegionResponse> Regions { get; init; } = [];

    /// <summary>
    /// Gets or sets available GPU types.
    /// </summary>
    public IReadOnlyList<ProviderGpuResponse> Gpus { get; init; } = [];

    /// <summary>
    /// Gets or sets available templates.
    /// </summary>
    public IReadOnlyList<ProviderTemplateResponse> Templates { get; init; } = [];
}
