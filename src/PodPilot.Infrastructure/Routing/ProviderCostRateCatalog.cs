using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Routing;

/// <summary>
/// Default token pricing catalog by provider kind.
/// </summary>
public sealed class ProviderCostRateCatalog : IProviderCostRateCatalog
{
    private static readonly IReadOnlyDictionary<AiProviderKind, (decimal Input, decimal Output)> Rates =
        new Dictionary<AiProviderKind, (decimal, decimal)>
        {
            [AiProviderKind.Ollama] = (0m, 0m),
            [AiProviderKind.Vllm] = (0.1m, 0.2m),
            [AiProviderKind.LlamaCpp] = (0.1m, 0.2m),
            [AiProviderKind.Groq] = (0.2m, 0.5m),
            [AiProviderKind.OpenRouter] = (1.5m, 4m),
            [AiProviderKind.Anthropic] = (3m, 15m),
            [AiProviderKind.OpenAi] = (2.5m, 10m),
            [AiProviderKind.AzureOpenAi] = (2.5m, 10m),
            [AiProviderKind.GoogleGemini] = (1.25m, 5m),
            [AiProviderKind.TogetherAi] = (1m, 3m),
            [AiProviderKind.FireworksAi] = (1m, 3m),
            [AiProviderKind.DeepInfra] = (1m, 3m),
        };

    /// <inheritdoc />
    public decimal GetInputCostPerMillion(AiProviderKind providerKind) =>
        Rates.TryGetValue(providerKind, out var rates) ? rates.Input : 1m;

    /// <inheritdoc />
    public decimal GetOutputCostPerMillion(AiProviderKind providerKind) =>
        Rates.TryGetValue(providerKind, out var rates) ? rates.Output : 3m;
}
