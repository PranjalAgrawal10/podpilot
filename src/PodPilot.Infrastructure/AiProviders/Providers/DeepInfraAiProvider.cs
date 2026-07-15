using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>DeepInfra OpenAI-compatible provider.</summary>
public sealed class DeepInfraAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="DeepInfraAiProvider"/> class.</summary>
    public DeepInfraAiProvider(IHttpClientFactory httpClientFactory, ILogger<DeepInfraAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.DeepInfra;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.deepinfra.com/v1/openai";
}
