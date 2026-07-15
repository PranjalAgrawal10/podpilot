using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>vLLM OpenAI-compatible provider.</summary>
public sealed class VllmAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="VllmAiProvider"/> class.</summary>
    public VllmAiProvider(IHttpClientFactory httpClientFactory, ILogger<VllmAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.Vllm;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "http://localhost:8000/v1";
}
