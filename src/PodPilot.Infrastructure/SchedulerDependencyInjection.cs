using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Scheduler;
using PodPilot.Infrastructure.BackgroundServices;
using PodPilot.Infrastructure.Scheduler;

namespace PodPilot.Infrastructure;

/// <summary>
/// Scheduler dependency injection extensions.
/// </summary>
public static class SchedulerDependencyInjection
{
    /// <summary>
    /// Registers scheduler services.
    /// </summary>
    public static IServiceCollection AddSchedulerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton(sp => SchedulerInfrastructure.Create(
            sp.GetRequiredService<IConfiguration>(),
            sp.GetRequiredService<IHostEnvironment>(),
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("PodPilot.Scheduler")));

        services.AddSingleton<IRequestQueue>(sp => sp.GetRequiredService<SchedulerInfrastructure>().Queue);
        services.AddSingleton<IDistributedLockService>(sp => sp.GetRequiredService<SchedulerInfrastructure>().LockService);

        services.AddScoped<IRequestDispatcher>(sp =>
        {
            var redis = sp.GetRequiredService<SchedulerInfrastructure>().Redis;
            return redis is null
                ? ActivatorUtilities.CreateInstance<RequestDispatcher>(sp)
                : ActivatorUtilities.CreateInstance<RequestDispatcher>(sp, redis);
        });

        services.AddSingleton<IRequestTracker, RequestTracker>();
        services.AddScoped<IRequestPriorityResolver, RequestPriorityResolver>();
        services.AddScoped<IRequestScheduler, RequestScheduler>();

        if (environment.IsEnvironment("Testing"))
        {
            services.AddScoped<ISchedulerNotificationService, NoOpSchedulerNotificationService>();
        }
        else
        {
            services.AddScoped<ISchedulerNotificationService, SchedulerNotificationService>();
        }

        return services;
    }

    /// <summary>
    /// Registers scheduler background workers.
    /// </summary>
    public static IServiceCollection AddSchedulerHostedServices(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<SchedulerDispatchWorker>();
            services.AddHostedService<SchedulerRetryWorker>();
            services.AddHostedService<SchedulerTimeoutWorker>();
            services.AddHostedService<SchedulerCleanupWorker>();
        }

        return services;
    }
}
