using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Infrastructure.BackgroundServices;
using PodPilot.Infrastructure.Observability;
using PodPilot.Infrastructure.Scheduler;
using PodPilot.Infrastructure.Services;
using StackExchange.Redis;

namespace PodPilot.Infrastructure;

/// <summary>
/// Observability dependency injection extensions.
/// </summary>
public static class ObservabilityDependencyInjection
{
    /// <summary>
    /// Registers observability services.
    /// </summary>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<IMetricsCollector, MetricsCollectorService>();
        services.AddScoped<ICostCalculator, CostCalculatorService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IMonitoringService>(sp => CreateWithRedis<MonitoringService>(sp));
        services.AddScoped<IMetricsAggregator>(sp => CreateWithRedis<MetricsAggregatorService>(sp));
        services.AddScoped<IObservabilityExportService, ObservabilityExportService>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IObservabilityNotificationService, NoOpObservabilityNotificationService>();
        }
        else
        {
            services.AddScoped<IObservabilityNotificationService, ObservabilityNotificationService>();
        }

        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<MetricsCollectionWorker>();
            services.AddHostedService<CostSnapshotWorker>();
            services.AddHostedService<MonitoringWorker>();
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
