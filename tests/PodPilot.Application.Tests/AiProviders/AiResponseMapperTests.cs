using System.Text;
using System.Text.Json;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.AiProviders;

namespace PodPilot.Application.Tests.AiProviders;

public class AiResponseMapperTests
{
    private readonly AiResponseMapper mapper = new();

    [Fact]
    public void ToOpenAiChatCompletionJson_Includes_Content_And_Usage()
    {
        var json = mapper.ToOpenAiChatCompletionJson(new AiChatResponse
        {
            Id = "chatcmpl-1",
            Model = "gpt-4o",
            Content = "Hello world",
            FinishReason = "stop",
            PromptTokens = 3,
            CompletionTokens = 2,
            TotalTokens = 5,
            ProviderKind = AiProviderKind.OpenAi,
        });

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Hello world", doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString());
        Assert.Equal(5, doc.RootElement.GetProperty("usage").GetProperty("total_tokens").GetInt32());
    }

    [Fact]
    public void ToOpenAiEmbeddingsJson_Serializes_Vectors()
    {
        var json = mapper.ToOpenAiEmbeddingsJson(new AiEmbeddingResponse
        {
            Model = "text-embedding-3-small",
            Embeddings = [[0.1f, 0.2f]],
            TotalTokens = 4,
        });

        using var doc = JsonDocument.Parse(json);
        Assert.Equal(0.1f, doc.RootElement.GetProperty("data")[0].GetProperty("embedding")[0].GetSingle());
    }

    [Fact]
    public async Task WriteOpenAiStreamChunkAsync_Writes_Done_Marker()
    {
        await using var stream = new MemoryStream();
        await mapper.WriteOpenAiStreamChunkAsync(stream, "gpt-4o", null, isDone: true);
        var text = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("data: [DONE]", text);
    }
}
