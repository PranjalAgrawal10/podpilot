using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>OpenAI API provider.</summary>
public sealed class OpenAiAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="OpenAiAiProvider"/> class.</summary>
    public OpenAiAiProvider(IHttpClientFactory httpClientFactory, ILogger<OpenAiAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.OpenAi;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.openai.com/v1";
}
