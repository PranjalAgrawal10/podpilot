using PodPilot.Application.Models.AiProviders;

namespace PodPilot.Application.Common.Interfaces;

/// <summary>
/// Normalizes provider-specific chat requests into a common internal model.
/// </summary>
public interface IAiRequestMapper
{
    /// <summary>
    /// Maps raw OpenAI-compatible chat JSON into the internal request model.
    /// </summary>
    AiChatRequest MapFromOpenAiChatJson(string json, string? fallbackModel = null);

    /// <summary>
    /// Maps raw Anthropic messages JSON into the internal request model.
    /// </summary>
    AiChatRequest MapFromAnthropicMessagesJson(string json, string? fallbackModel = null);

    /// <summary>
    /// Maps raw Gemini generateContent JSON into the internal request model.
    /// </summary>
    AiChatRequest MapFromGeminiJson(string json, string? fallbackModel = null);

    /// <summary>
    /// Maps raw Ollama chat JSON into the internal request model.
    /// </summary>
    AiChatRequest MapFromOllamaChatJson(string json, string? fallbackModel = null);
}
