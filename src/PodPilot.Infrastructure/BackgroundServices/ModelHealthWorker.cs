using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically checks Ollama model health on running pods.
/// </summary>
public sealed class ModelHealthWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ModelHealthWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelHealthWorker"/> class.
    /// </summary>
    public ModelHealthWorker(IServiceScopeFactory serviceScopeFactory, ILogger<ModelHealthWorker> logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckModelsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Model health worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckModelsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var healthService = scope.ServiceProvider.GetRequiredService<IModelHealthService>();

        var pods = await dbContext.GpuPods
            .Where(p => p.Status == PodStatus.Running && p.Endpoint != null)
            .ToListAsync(cancellationToken);

        foreach (var pod in pods)
        {
            try
            {
                await healthService.CheckPodModelsAsync(pod, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Model health check failed for pod {PodId}", pod.Id);
            }
        }
    }
}
