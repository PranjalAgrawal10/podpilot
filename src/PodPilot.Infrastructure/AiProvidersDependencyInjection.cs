using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.AiProviders;
using PodPilot.Infrastructure.AiProviders.Providers;
using PodPilot.Infrastructure.BackgroundServices;

namespace PodPilot.Infrastructure;

/// <summary>
/// AI providers dependency injection extensions.
/// </summary>
public static class AiProvidersDependencyInjection
{
    /// <summary>
    /// Registers AI provider engine services.
    /// </summary>
    public static IServiceCollection AddAiProviders(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddHttpClient(nameof(OpenAiCompatibleAiProvider));
        services.AddHttpClient(nameof(AnthropicAiProvider));
        services.AddHttpClient(nameof(GeminiAiProvider));

        services.AddSingleton<IAiProvider, OllamaAiProvider>();
        services.AddSingleton<IAiProvider, VllmAiProvider>();
        services.AddSingleton<IAiProvider, LlamaCppAiProvider>();
        services.AddSingleton<IAiProvider, OpenAiAiProvider>();
        services.AddSingleton<IAiProvider, OpenRouterAiProvider>();
        services.AddSingleton<IAiProvider, AzureOpenAiAiProvider>();
        services.AddSingleton<IAiProvider, GroqAiProvider>();
        services.AddSingleton<IAiProvider, TogetherAiAiProvider>();
        services.AddSingleton<IAiProvider, FireworksAiAiProvider>();
        services.AddSingleton<IAiProvider, DeepInfraAiProvider>();
        services.AddSingleton<IAiProvider, AnthropicAiProvider>();
        services.AddSingleton<IAiProvider, GeminiAiProvider>();

        services.AddSingleton<IAiProviderFactory, AiProviderFactory>();
        services.AddSingleton<IAiProviderRegistry, AiProviderRegistry>();
        services.AddSingleton<IAiRequestMapper, AiRequestMapper>();
        services.AddSingleton<IAiResponseMapper, AiResponseMapper>();

        services.AddScoped<IAiProviderService, AiProviderService>();
        services.AddScoped<IAiInferenceRouter, AiInferenceRouter>();
        services.AddScoped<IAiInferenceDispatcher, AiInferenceDispatcher>();
        services.AddScoped<IAiFailoverService, AiFailoverService>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IAiProviderNotificationService, NoOpAiProviderNotificationService>();
        }
        else
        {
            services.AddScoped<IAiProviderNotificationService, AiProviderNotificationService>();
            services.AddHostedService<AiProviderHealthWorker>();
        }

        return services;
    }
}
