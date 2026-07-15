using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.AiProviders;
using PodPilot.Infrastructure.AiProviders.Providers;

namespace PodPilot.Application.Tests.AiProviders;

public class AiProviderFactoryTests
{
    [Fact]
    public void GetProvider_Returns_Registered_Kind()
    {
        var factory = new AiProviderFactory(
        [
            new OpenAiAiProvider(new TestHttpClientFactory(), new Microsoft.Extensions.Logging.Abstractions.NullLogger<OpenAiAiProvider>()),
            new AnthropicAiProvider(new TestHttpClientFactory(), new Microsoft.Extensions.Logging.Abstractions.NullLogger<AnthropicAiProvider>()),
        ]);

        var provider = factory.GetProvider(AiProviderKind.OpenAi);
        Assert.Equal(AiProviderKind.OpenAi, provider.ProviderKind);
        Assert.Contains(AiProviderKind.Anthropic, factory.GetSupportedKinds());
    }

    [Fact]
    public void GetProvider_Throws_For_Unknown_Kind()
    {
        var factory = new AiProviderFactory(
        [
            new OpenAiAiProvider(new TestHttpClientFactory(), new Microsoft.Extensions.Logging.Abstractions.NullLogger<OpenAiAiProvider>()),
        ]);

        Assert.Throws<InvalidOperationException>(() => factory.GetProvider(AiProviderKind.Groq));
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
