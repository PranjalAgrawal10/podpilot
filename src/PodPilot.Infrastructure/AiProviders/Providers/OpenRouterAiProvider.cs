using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>OpenRouter provider.</summary>
public sealed class OpenRouterAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="OpenRouterAiProvider"/> class.</summary>
    public OpenRouterAiProvider(IHttpClientFactory httpClientFactory, ILogger<OpenRouterAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.OpenRouter;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://openrouter.ai/api/v1";
}
