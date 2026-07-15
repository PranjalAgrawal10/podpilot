using System.Text;
using System.Text.Json;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Maps internal responses to OpenAI-compatible JSON payloads.
/// </summary>
public sealed class AiResponseMapper : IAiResponseMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <inheritdoc />
    public string ToOpenAiChatCompletionJson(AiChatResponse response)
    {
        var payload = new
        {
            id = string.IsNullOrWhiteSpace(response.Id) ? $"chatcmpl-{Guid.NewGuid():N}" : response.Id,
            @object = "chat.completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = response.Model,
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new { role = "assistant", content = response.Content },
                    finish_reason = response.FinishReason ?? "stop",
                },
            },
            usage = new
            {
                prompt_tokens = response.PromptTokens ?? 0,
                completion_tokens = response.CompletionTokens ?? 0,
                total_tokens = response.TotalTokens ?? ((response.PromptTokens ?? 0) + (response.CompletionTokens ?? 0)),
            },
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    /// <inheritdoc />
    public string ToOpenAiEmbeddingsJson(AiEmbeddingResponse response)
    {
        var data = response.Embeddings
            .Select((embedding, index) => new
            {
                @object = "embedding",
                index,
                embedding,
            })
            .ToArray();

        var payload = new
        {
            @object = "list",
            data,
            model = response.Model,
            usage = new
            {
                prompt_tokens = response.TotalTokens ?? 0,
                total_tokens = response.TotalTokens ?? 0,
            },
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    /// <inheritdoc />
    public async Task WriteOpenAiStreamChunkAsync(
        Stream responseStream,
        string model,
        string? contentDelta,
        bool isDone,
        CancellationToken cancellationToken = default)
    {
        if (isDone)
        {
            var done = Encoding.UTF8.GetBytes("data: [DONE]\n\n");
            await responseStream.WriteAsync(done, cancellationToken);
            await responseStream.FlushAsync(cancellationToken);
            return;
        }

        var chunk = JsonSerializer.Serialize(
            new
            {
                id = $"chatcmpl-{Guid.NewGuid():N}",
                @object = "chat.completion.chunk",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                model,
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        delta = new { content = contentDelta ?? string.Empty },
                        finish_reason = (string?)null,
                    },
                },
            },
            JsonOptions);

        var bytes = Encoding.UTF8.GetBytes($"data: {chunk}\n\n");
        await responseStream.WriteAsync(bytes, cancellationToken);
        await responseStream.FlushAsync(cancellationToken);
    }
}
