using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Resolves AI provider implementations by kind.
/// </summary>
public interface IAiProviderFactory
{
    /// <summary>
    /// Gets the AI provider implementation for the specified kind.
    /// </summary>
    IAiProvider GetProvider(AiProviderKind providerKind);

    /// <summary>
    /// Gets all registered provider kinds.
    /// </summary>
    IReadOnlyList<AiProviderKind> GetSupportedKinds();
}
