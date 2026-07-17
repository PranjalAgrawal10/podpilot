using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Deployments.Runtimes;

/// <summary>
/// Resolves inference runtime providers by kind.
/// </summary>
public sealed class RuntimeProviderFactory : IRuntimeProviderFactory
{
    private readonly IReadOnlyDictionary<InferenceRuntimeKind, IRuntimeProvider> providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeProviderFactory"/> class.
    /// </summary>
    public RuntimeProviderFactory(IEnumerable<IRuntimeProvider> providers)
    {
        this.providers = providers.ToDictionary(p => p.Kind);
    }

    /// <inheritdoc />
    public IRuntimeProvider GetProvider(InferenceRuntimeKind kind)
    {
        if (providers.TryGetValue(kind, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"No runtime provider registered for {kind}.");
    }
}
