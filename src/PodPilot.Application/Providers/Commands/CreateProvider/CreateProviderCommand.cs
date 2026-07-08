using MediatR;
using PodPilot.Contracts.Providers;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Providers.Commands.CreateProvider;

/// <summary>
/// Creates a compute provider for the current organization.
/// </summary>
public sealed class CreateProviderCommand : IRequest<ProviderResponse>
{
    /// <summary>
    /// Gets or sets the internal name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public ProviderType ProviderType { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the default region.
    /// </summary>
    public string? DefaultRegion { get; init; }

    /// <summary>
    /// Gets or sets the provider API key.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}
