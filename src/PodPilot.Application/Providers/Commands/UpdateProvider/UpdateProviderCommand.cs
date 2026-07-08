using MediatR;
using PodPilot.Contracts.Providers;

namespace PodPilot.Application.Providers.Commands.UpdateProvider;

/// <summary>
/// Updates a compute provider.
/// </summary>
public sealed class UpdateProviderCommand : IRequest<ProviderResponse>
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Gets or sets the internal name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets or sets an optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the default region.
    /// </summary>
    public string? DefaultRegion { get; init; }

    /// <summary>
    /// Gets or sets a new API key for rotation.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool? IsEnabled { get; init; }
}
