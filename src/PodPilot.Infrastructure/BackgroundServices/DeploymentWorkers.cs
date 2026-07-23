using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Enums;
using PodPilot.Infrastructure.Deployments;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Advances pending / provisioning / starting / runtime / configure / health deployments.
/// </summary>
public sealed class DeploymentWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<DeploymentWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentWorker"/> class.
    /// </summary>
    public DeploymentWorker(IServiceScopeFactory scopeFactory, ILogger<DeploymentWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deployment worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> ids;
        using (var scope = scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            // Explicit OR — MySQL EF cannot type-map enum[] for Contains/IN.
            ids = await db.AiDeployments
                .Where(d =>
                    d.Status == DeploymentStatus.Pending
                    || d.Status == DeploymentStatus.Provisioning
                    || d.Status == DeploymentStatus.Starting
                    || d.Status == DeploymentStatus.InstallingRuntime
                    || d.Status == DeploymentStatus.Configuring
                    || d.Status == DeploymentStatus.HealthCheck)
                .OrderBy(d => d.UpdatedAt ?? d.CreatedAt)
                .Select(d => d.Id)
                .Take(10)
                .ToListAsync(cancellationToken);
        }

        foreach (var id in ids)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IDeploymentService>();
                await service.ProcessPendingStepAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed processing deployment {DeploymentId}", id);
            }
        }
    }
}

/// <summary>
/// Processes model download steps for deployments.
/// </summary>
public sealed class ModelDownloadWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<ModelDownloadWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelDownloadWorker"/> class.
    /// </summary>
    public ModelDownloadWorker(IServiceScopeFactory scopeFactory, ILogger<ModelDownloadWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<Guid> ids;
                using (var scope = scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                    ids = await db.AiDeployments
                        .Where(d => d.Status == DeploymentStatus.DownloadingModels)
                        .OrderBy(d => d.UpdatedAt ?? d.CreatedAt)
                        .Select(d => d.Id)
                        .Take(5)
                        .ToListAsync(stoppingToken);
                }

                foreach (var id in ids)
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<IDeploymentService>();
                        await service.ProcessPendingStepAsync(id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Model download step failed for {DeploymentId}", id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Model download worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}

/// <summary>
/// Periodically health-checks ready deployments.
/// </summary>
public sealed class DeploymentHealthWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<DeploymentHealthWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentHealthWorker"/> class.
    /// </summary>
    public DeploymentHealthWorker(IServiceScopeFactory scopeFactory, ILogger<DeploymentHealthWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<(Guid OrgId, Guid Id)> items;
                using (var scope = scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                    items = await db.AiDeployments
                        .Where(d => d.Status == DeploymentStatus.Ready)
                        .Select(d => new ValueTuple<Guid, Guid>(d.OrganizationId, d.Id))
                        .Take(20)
                        .ToListAsync(stoppingToken);
                }

                foreach (var (orgId, id) in items)
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<IDeploymentService>();
                        await service.RunHealthCheckAsync(orgId, id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Health check failed for deployment {DeploymentId}", id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deployment health worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}

/// <summary>
/// Completes deletion of deployments in Deleting status.
/// </summary>
public sealed class DeploymentCleanupWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<DeploymentCleanupWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentCleanupWorker"/> class.
    /// </summary>
    public DeploymentCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<DeploymentCleanupWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<Guid> ids;
                using (var scope = scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                    ids = await db.AiDeployments
                        .Where(d => d.Status == DeploymentStatus.Deleting)
                        .Select(d => d.Id)
                        .Take(10)
                        .ToListAsync(stoppingToken);
                }

                foreach (var id in ids)
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<IDeploymentService>();
                        await service.ProcessPendingStepAsync(id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Cleanup failed for deployment {DeploymentId}", id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deployment cleanup worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}

/// <summary>
/// Retries failed deployments with remaining retry budget.
/// </summary>
public sealed class DeploymentRetryWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<DeploymentRetryWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentRetryWorker"/> class.
    /// </summary>
    public DeploymentRetryWorker(IServiceScopeFactory scopeFactory, ILogger<DeploymentRetryWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var dateTime = scope.ServiceProvider.GetRequiredService<IDateTimeService>();

                var failed = await db.AiDeployments
                    .Where(d => d.Status == DeploymentStatus.Failed && d.RetryCount < 3 && !d.CancellationRequested)
                    .OrderBy(d => d.UpdatedAt ?? d.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var deployment in failed)
                {
                    try
                    {
                        var lastRecoverable = await db.DeploymentHistoryEntries
                            .Where(h =>
                                h.DeploymentId == deployment.Id
                                && (h.ToStatus == DeploymentStatus.Provisioning
                                    || h.ToStatus == DeploymentStatus.Starting
                                    || h.ToStatus == DeploymentStatus.InstallingRuntime
                                    || h.ToStatus == DeploymentStatus.DownloadingModels
                                    || h.ToStatus == DeploymentStatus.Configuring
                                    || h.ToStatus == DeploymentStatus.HealthCheck))
                            .OrderByDescending(h => h.TimestampUtc)
                            .Select(h => (DeploymentStatus?)h.ToStatus)
                            .FirstOrDefaultAsync(stoppingToken);

                        var target = lastRecoverable
                            ?? (deployment.GpuPodId.HasValue
                                ? DeploymentStatus.Starting
                                : DeploymentStatus.Pending);

                        var now = dateTime.UtcNow;
                        deployment.RetryCount += 1;
                        deployment.Status = target;
                        deployment.ErrorMessage = null;
                        deployment.StatusMessage = $"Retry {deployment.RetryCount}/3 from {target}";
                        deployment.UpdatedAt = now;

                        await db.AddDeploymentHistoryAsync(
                            new Domain.Entities.DeploymentHistory
                            {
                                Id = Guid.NewGuid(),
                                DeploymentId = deployment.Id,
                                FromStatus = DeploymentStatus.Failed,
                                ToStatus = target,
                                Message = deployment.StatusMessage,
                                TimestampUtc = now,
                            },
                            stoppingToken);
                        await db.SaveChangesAsync(stoppingToken);
                        logger.LogInformation(
                            "Retrying deployment {DeploymentId} at status {Status} (attempt {Retry})",
                            deployment.Id,
                            target,
                            deployment.RetryCount);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Retry scheduling failed for {DeploymentId}", deployment.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deployment retry worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
