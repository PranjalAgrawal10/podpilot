using PodPilot.Application.Models;

namespace PodPilot.Application.Tests.Models;

public class ModelReferenceParserTests
{
    [Theory]
    [InlineData("llama3:latest", "llama3", "latest")]
    [InlineData("mistral", "mistral", "latest")]
    [InlineData("qwen2.5:7b", "qwen2.5", "7b")]
    public void Parse_SplitsNameAndTag(string input, string name, string tag)
    {
        var (parsedName, parsedTag) = ModelReferenceParser.Parse(input);
        Assert.Equal(name, parsedName);
        Assert.Equal(tag, parsedTag);
    }

    [Fact]
    public void ToReference_BuildsFullName()
    {
        Assert.Equal("llama3", ModelReferenceParser.ToReference("llama3", "latest"));
        Assert.Equal("llama3:7b", ModelReferenceParser.ToReference("llama3", "7b"));
        Assert.Equal("llama3", ModelReferenceParser.ToReference("llama3", string.Empty));
    }

    [Theory]
    [InlineData("llama3:7b", "7B")]
    [InlineData("mistral-nemo-12b", "12B")]
    public void ExtractParameters_FindsParameterLabel(string modelName, string expected)
    {
        var result = ModelReferenceParser.ExtractParameters(null, modelName);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractQuantization_FindsKnownFormat()
    {
        var result = ModelReferenceParser.ExtractQuantization(null, "llama3-q4_k_m");
        Assert.Equal("Q4_K_M", result);
    }
}
