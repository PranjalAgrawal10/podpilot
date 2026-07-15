using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>llama.cpp OpenAI-compatible provider.</summary>
public sealed class LlamaCppAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="LlamaCppAiProvider"/> class.</summary>
    public LlamaCppAiProvider(IHttpClientFactory httpClientFactory, ILogger<LlamaCppAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.LlamaCpp;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "http://localhost:8080/v1";
}
