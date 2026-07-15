using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Routing;

namespace PodPilot.Application.Tests.Routing;

public class TaskClassifierTests
{
    private readonly TaskClassifier classifier = new();

    [Fact]
    public void Analyze_Detects_Coding_Task()
    {
        var analysis = classifier.Analyze(
            "/v1/chat/completions",
            null,
            "Write a Python function with a class to refactor this bug");

        Assert.Equal(AiTaskType.Coding, analysis.TaskType);
        Assert.True(analysis.EstimatedInputTokens > 0);
        Assert.True(analysis.EstimatedOutputTokens > 0);
    }

    [Fact]
    public void Analyze_Detects_Embeddings_From_Path()
    {
        var analysis = classifier.Analyze("/v1/embeddings", "{\"input\":\"hello\"}", null);
        Assert.Equal(AiTaskType.Embeddings, analysis.TaskType);
        Assert.True(analysis.RequiresEmbeddings);
    }

    [Fact]
    public void Analyze_Detects_Vision_From_Message_Parts()
    {
        var body = """
                   {
                     "model": "gpt-4o",
                     "messages": [
                       {
                         "role": "user",
                         "content": [
                           { "type": "text", "text": "what is in this image" },
                           { "type": "image_url", "image_url": { "url": "https://example.com/a.png" } }
                         ]
                       }
                     ]
                   }
                   """;

        var analysis = classifier.Analyze("/v1/chat/completions", body, null);
        Assert.True(analysis.RequiresVision);
        Assert.Equal(AiTaskType.Vision, analysis.TaskType);
        Assert.Equal("gpt-4o", analysis.RequestedModel);
    }
}
