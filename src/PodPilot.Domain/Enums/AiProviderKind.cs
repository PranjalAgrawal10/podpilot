namespace PodPilot.Domain.Enums;

/// <summary>
/// Supported AI inference provider kinds.
/// </summary>
public enum AiProviderKind
{
    /// <summary>Ollama (local or remote).</summary>
    Ollama = 0,

    /// <summary>vLLM OpenAI-compatible server.</summary>
    Vllm = 1,

    /// <summary>llama.cpp OpenAI-compatible server.</summary>
    LlamaCpp = 2,

    /// <summary>OpenAI API.</summary>
    OpenAi = 3,

    /// <summary>Anthropic Claude API.</summary>
    Anthropic = 4,

    /// <summary>OpenRouter.</summary>
    OpenRouter = 5,

    /// <summary>Azure OpenAI.</summary>
    AzureOpenAi = 6,

    /// <summary>Google Gemini.</summary>
    GoogleGemini = 7,

    /// <summary>Groq.</summary>
    Groq = 8,

    /// <summary>Together AI.</summary>
    TogetherAi = 9,

    /// <summary>Fireworks AI.</summary>
    FireworksAi = 10,

    /// <summary>DeepInfra.</summary>
    DeepInfra = 11,
}
