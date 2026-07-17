using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Deployments.Runtimes;

namespace PodPilot.Application.Tests.Deployments;

public class RuntimeProviderFactoryTests
{
    [Theory]
    [InlineData(InferenceRuntimeKind.Ollama)]
    [InlineData(InferenceRuntimeKind.Vllm)]
    [InlineData(InferenceRuntimeKind.LlamaCpp)]
    public void GetProvider_Resolves_Registered_Runtimes(InferenceRuntimeKind kind)
    {
        var factory = CreateFactory();
        var provider = factory.GetProvider(kind);
        Assert.Equal(kind, provider.Kind);
    }

    [Fact]
    public void GetProvider_Throws_For_Unregistered_Kind()
    {
        var httpFactory = new TestHttpClientFactory();
        var factory = new RuntimeProviderFactory(
        [
            new VllmRuntimeProvider(httpFactory, NullLogger<VllmRuntimeProvider>.Instance),
        ]);

        Assert.Throws<InvalidOperationException>(() => factory.GetProvider(InferenceRuntimeKind.Ollama));
    }

    private static RuntimeProviderFactory CreateFactory()
    {
        var httpFactory = new TestHttpClientFactory();
        var ollamaClient = new Mock<IOllamaClient>().Object;

        return new RuntimeProviderFactory(
        [
            new OllamaRuntimeProvider(
                httpFactory,
                ollamaClient,
                NullLogger<OllamaRuntimeProvider>.Instance),
            new VllmRuntimeProvider(httpFactory, NullLogger<VllmRuntimeProvider>.Instance),
            new LlamaCppRuntimeProvider(httpFactory, NullLogger<LlamaCppRuntimeProvider>.Instance),
        ]);
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
