using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.Routing;

namespace PodPilot.Infrastructure;

/// <summary>
/// Intelligent routing dependency injection extensions.
/// </summary>
public static class RoutingDependencyInjection
{
    /// <summary>
    /// Registers intelligent model router and cost optimizer services.
    /// </summary>
    public static IServiceCollection AddIntelligentRouting(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddSingleton<ITaskClassifier, TaskClassifier>();
        services.AddScoped<IProviderSelector, ProviderSelector>();
        services.AddScoped<IModelRouter, ModelRouter>();
        services.AddScoped<IRoutingPolicy, RoutingPolicyService>();
        services.AddScoped<ICostEstimator, CostEstimator>();
        services.AddScoped<ILatencyPredictor, LatencyPredictor>();
        services.AddScoped<IAvailabilityScorer, AvailabilityScorer>();
        services.AddScoped<IRoutingEngine, RoutingEngine>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IRoutingNotificationService, NoOpRoutingNotificationService>();
        }
        else
        {
            services.AddScoped<IRoutingNotificationService, RoutingNotificationService>();
        }

        return services;
    }
}
