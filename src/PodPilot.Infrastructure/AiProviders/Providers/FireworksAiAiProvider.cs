using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Fireworks AI provider.</summary>
public sealed class FireworksAiAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="FireworksAiAiProvider"/> class.</summary>
    public FireworksAiAiProvider(IHttpClientFactory httpClientFactory, ILogger<FireworksAiAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.FireworksAi;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.fireworks.ai/inference/v1";
}
