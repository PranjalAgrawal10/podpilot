using PodPilot.Infrastructure.AiProviders;

namespace PodPilot.Application.Tests.AiProviders;

public class AiRequestMapperTests
{
    private readonly AiRequestMapper mapper = new();

    [Fact]
    public void MapFromOpenAiChatJson_Parses_Messages_And_Options()
    {
        var json = """
            {
              "model": "gpt-4o",
              "temperature": 0.2,
              "max_tokens": 100,
              "stream": true,
              "messages": [
                { "role": "system", "content": "Be brief" },
                { "role": "user", "content": "Hello" }
              ]
            }
            """;

        var request = mapper.MapFromOpenAiChatJson(json);
        Assert.Equal("gpt-4o", request.Model);
        Assert.Equal(0.2, request.Temperature);
        Assert.Equal(100, request.MaxTokens);
        Assert.True(request.Stream);
        Assert.Equal("Be brief", request.SystemPrompt);
        Assert.Single(request.Messages);
        Assert.Equal("user", request.Messages[0].Role);
        Assert.Equal("Hello", request.Messages[0].Content);
    }

    [Fact]
    public void MapFromAnthropicMessagesJson_Parses_System_And_Messages()
    {
        var json = """
            {
              "model": "claude-3-5-sonnet-20241022",
              "system": "You are helpful",
              "messages": [{ "role": "user", "content": "Hi" }]
            }
            """;

        var request = mapper.MapFromAnthropicMessagesJson(json);
        Assert.Equal("claude-3-5-sonnet-20241022", request.Model);
        Assert.Equal("You are helpful", request.SystemPrompt);
        Assert.Equal("Hi", request.Messages[0].Content);
    }

    [Fact]
    public void MapFromOllamaChatJson_Uses_Prompt_Fallback()
    {
        var json = """{ "model": "llama3", "prompt": "Say hi", "stream": false }""";
        var request = mapper.MapFromOllamaChatJson(json);
        Assert.Equal("llama3", request.Model);
        Assert.Equal("Say hi", request.Messages[0].Content);
        Assert.False(request.Stream);
    }
}
