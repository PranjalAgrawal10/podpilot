using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// In-memory registry of AI provider capabilities and defaults.
/// </summary>
public interface IAiProviderRegistry
{
    /// <summary>
    /// Gets metadata for a provider kind.
    /// </summary>
    AiProviderKindMetadata GetMetadata(AiProviderKind providerKind);

    /// <summary>
    /// Lists metadata for all supported provider kinds.
    /// </summary>
    IReadOnlyList<AiProviderKindMetadata> ListMetadata();
}
