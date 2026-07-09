using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.BackgroundServices;
using PodPilot.Infrastructure.Orchestrator;
using PodPilot.Infrastructure.Scheduler;
using PodPilot.Infrastructure.Services;
using StackExchange.Redis;

namespace PodPilot.Infrastructure;

/// <summary>
/// Orchestrator dependency injection extensions.
/// </summary>
public static class OrchestratorDependencyInjection
{
    /// <summary>
    /// Registers orchestrator services.
    /// </summary>
    public static IServiceCollection AddOrchestrator(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<IPodPoolManager>(sp => CreateWithRedis<PodPoolManager>(sp));
        services.AddScoped<ILoadBalancer>(sp => CreateWithRedis<LoadBalancerService>(sp));
        services.AddScoped<ICapacityPlanner>(sp => CreateWithRedis<CapacityPlannerService>(sp));
        services.AddScoped<IAutoScaler, AutoScalerService>();
        services.AddScoped<IPodOrchestrator>(sp => CreateWithRedis<PodOrchestratorService>(sp));

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IOrchestratorNotificationService, NoOpOrchestratorNotificationService>();
        }
        else
        {
            services.AddScoped<IOrchestratorNotificationService, OrchestratorNotificationService>();
        }

        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<PodHealthWorker>();
            services.AddHostedService<AutoScalerWorker>();
            services.AddHostedService<CapacityPlannerWorker>();
        }

        return services;
    }

    private static TService CreateWithRedis<TService>(IServiceProvider sp)
        where TService : class
    {
        var redis = sp.GetService<SchedulerInfrastructure>()?.Redis;
        return redis is null
            ? ActivatorUtilities.CreateInstance<TService>(sp)
            : ActivatorUtilities.CreateInstance<TService>(sp, redis);
    }
}
