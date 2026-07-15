using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// In-memory registry of AI provider capabilities and defaults.
/// </summary>
public sealed class AiProviderRegistry : IAiProviderRegistry
{
    private static readonly IReadOnlyList<AiProviderKindMetadata> Metadata =
    [
        new() { ProviderKind = AiProviderKind.Ollama, DisplayName = "Ollama", DefaultBaseUrl = "http://localhost:11434/v1", RequiresBaseUrl = false, RequiresApiKey = false, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.Vllm, DisplayName = "vLLM", DefaultBaseUrl = string.Empty, RequiresBaseUrl = true, RequiresApiKey = false, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.LlamaCpp, DisplayName = "llama.cpp", DefaultBaseUrl = string.Empty, RequiresBaseUrl = true, RequiresApiKey = false, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.OpenAi, DisplayName = "OpenAI", DefaultBaseUrl = "https://api.openai.com/v1", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.Anthropic, DisplayName = "Anthropic", DefaultBaseUrl = "https://api.anthropic.com", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = false },
        new() { ProviderKind = AiProviderKind.OpenRouter, DisplayName = "OpenRouter", DefaultBaseUrl = "https://openrouter.ai/api/v1", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.AzureOpenAi, DisplayName = "Azure OpenAI", DefaultBaseUrl = string.Empty, RequiresBaseUrl = true, RequiresApiKey = true, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.GoogleGemini, DisplayName = "Google Gemini", DefaultBaseUrl = "https://generativelanguage.googleapis.com", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = false },
        new() { ProviderKind = AiProviderKind.Groq, DisplayName = "Groq", DefaultBaseUrl = "https://api.groq.com/openai/v1", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.TogetherAi, DisplayName = "Together AI", DefaultBaseUrl = "https://api.together.xyz/v1", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.FireworksAi, DisplayName = "Fireworks AI", DefaultBaseUrl = "https://api.fireworks.ai/inference/v1", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = true },
        new() { ProviderKind = AiProviderKind.DeepInfra, DisplayName = "DeepInfra", DefaultBaseUrl = "https://api.deepinfra.com/v1/openai", RequiresBaseUrl = false, RequiresApiKey = true, IsOpenAiCompatible = true },
    ];

    private readonly IReadOnlyDictionary<AiProviderKind, AiProviderKindMetadata> byKind =
        Metadata.ToDictionary(m => m.ProviderKind);

    /// <inheritdoc />
    public AiProviderKindMetadata GetMetadata(AiProviderKind providerKind)
    {
        if (!byKind.TryGetValue(providerKind, out var metadata))
        {
            throw new InvalidOperationException($"Unknown AI provider kind '{providerKind}'.");
        }

        return metadata;
    }

    /// <inheritdoc />
    public IReadOnlyList<AiProviderKindMetadata> ListMetadata() => Metadata;
}
