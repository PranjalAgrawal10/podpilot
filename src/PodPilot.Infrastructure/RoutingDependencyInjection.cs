using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.AiProviders;
using PodPilot.Infrastructure.Routing;
using PodPilot.Infrastructure.Routing.Planners;
using PodPilot.Infrastructure.Routing.Strategies;

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
        services.AddSingleton<IProviderCostRateCatalog, ProviderCostRateCatalog>();
        services.AddSingleton<IModelScorer, ModelScorer>();

        services.AddSingleton<IRoutingWeightStrategy, LowestCostWeightStrategy>();
        services.AddSingleton<IRoutingWeightStrategy, LowestLatencyWeightStrategy>();
        services.AddSingleton<IRoutingWeightStrategy, HighestAccuracyWeightStrategy>();
        services.AddSingleton<IRoutingWeightStrategy, PolicyConfiguredWeightStrategy>();
        services.AddSingleton<IRoutingWeightResolver, RoutingWeightResolver>();

        services.AddScoped<IProviderSelector, ProviderSelector>();
        services.AddScoped<IModelRouter, ModelRouter>();
        services.AddScoped<IRoutingPolicy, RoutingPolicyService>();
        services.AddScoped<ICostEstimator, CostEstimator>();
        services.AddScoped<ILatencyPredictor, LatencyPredictor>();
        services.AddScoped<IAvailabilityScorer, AvailabilityScorer>();
        services.AddScoped<IRoutingCandidateEnricher, RoutingCandidateEnricher>();
        services.AddScoped<IRoutingDecisionStore, RoutingDecisionStore>();
        services.AddScoped<ILegacyAiInferenceRouter, LegacyAiInferenceRouter>();

        // More specific planners first so ProviderPriority wins when applicable.
        services.AddScoped<IRoutePlanner, ProviderPriorityRoutePlanner>();
        services.AddScoped<IRoutePlanner, ScoredRoutePlanner>();

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
