using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.AiProviders.Providers;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Resolves AI provider implementations by kind.
/// </summary>
public sealed class AiProviderFactory : IAiProviderFactory
{
    private readonly IReadOnlyDictionary<AiProviderKind, IAiProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiProviderFactory"/> class.
    /// </summary>
    public AiProviderFactory(IEnumerable<IAiProvider> providers)
    {
        this.providers = providers.ToDictionary(p => p.ProviderKind);
    }

    /// <inheritdoc />
    public IAiProvider GetProvider(AiProviderKind providerKind)
    {
        if (!providers.TryGetValue(providerKind, out var provider))
        {
            throw new InvalidOperationException($"AI provider kind '{providerKind}' is not registered.");
        }

        return provider;
    }

    /// <inheritdoc />
    public IReadOnlyList<AiProviderKind> GetSupportedKinds() =>
        providers.Keys.OrderBy(k => (int)k).ToList();
}
