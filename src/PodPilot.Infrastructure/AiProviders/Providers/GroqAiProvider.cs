using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Groq OpenAI-compatible provider.</summary>
public sealed class GroqAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="GroqAiProvider"/> class.</summary>
    public GroqAiProvider(IHttpClientFactory httpClientFactory, ILogger<GroqAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.Groq;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.groq.com/openai/v1";
}
