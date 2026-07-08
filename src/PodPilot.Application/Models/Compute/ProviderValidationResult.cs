using PodPilot.Domain.Enums;

namespace PodPilot.Application.Models.Compute;

/// <summary>
/// Result of validating provider credentials.
/// </summary>
public sealed class ProviderValidationResult
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
    /// Gets or sets the connection status.
    /// </summary>
    public ProviderConnectionStatus ConnectionStatus { get; init; } = ProviderConnectionStatus.Unknown;

    /// <summary>
    /// Gets or sets optional account information.
    /// </summary>
    public ProviderAccountInfo? AccountInfo { get; init; }

    /// <summary>
    /// Gets or sets available regions.
    /// </summary>
    public IReadOnlyList<ProviderRegionInfo> Regions { get; init; } = [];

    /// <summary>
    /// Gets or sets available GPU types.
    /// </summary>
    public IReadOnlyList<ProviderGpuInfo> Gpus { get; init; } = [];

    /// <summary>
    /// Gets or sets available templates.
    /// </summary>
    public IReadOnlyList<ProviderTemplateInfo> Templates { get; init; } = [];
}
