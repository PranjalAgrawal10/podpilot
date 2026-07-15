using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Together AI provider.</summary>
public sealed class TogetherAiAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="TogetherAiAiProvider"/> class.</summary>
    public TogetherAiAiProvider(IHttpClientFactory httpClientFactory, ILogger<TogetherAiAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.TogetherAi;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.together.xyz/v1";
}
