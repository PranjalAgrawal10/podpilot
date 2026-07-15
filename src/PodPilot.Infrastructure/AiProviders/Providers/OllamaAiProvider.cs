using Microsoft.Extensions.Logging;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders.Providers;

/// <summary>Ollama OpenAI-compatible provider.</summary>
public sealed class OllamaAiProvider : OpenAiCompatibleAiProvider
{
    /// <summary>Initializes a new instance of the <see cref="OllamaAiProvider"/> class.</summary>
    public OllamaAiProvider(IHttpClientFactory httpClientFactory, ILogger<OllamaAiProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    /// <inheritdoc />
    public override AiProviderKind ProviderKind => AiProviderKind.Ollama;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "http://localhost:11434/v1";

    /// <inheritdoc />
    protected override System.Net.Http.Headers.AuthenticationHeaderValue? BuildAuthHeader(
        Application.Models.AiProviders.AiProviderConnection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.ApiKey))
        {
            return null;
        }

        return base.BuildAuthHeader(connection);
    }
}
