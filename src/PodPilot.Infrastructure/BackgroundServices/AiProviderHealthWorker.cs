using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically checks AI inference provider health.
/// </summary>
public sealed class AiProviderHealthWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<AiProviderHealthWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiProviderHealthWorker"/> class.
    /// </summary>
    public AiProviderHealthWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AiProviderHealthWorker> logger)
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
                await CheckProvidersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AI provider health worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckProvidersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var providerService = scope.ServiceProvider.GetRequiredService<IAiProviderService>();
        var notifications = scope.ServiceProvider.GetRequiredService<IAiProviderNotificationService>();

        var providers = await dbContext.AiInferenceProviders
            .Where(p => p.IsEnabled && p.IsValidated)
            .Select(p => new { p.Id, p.OrganizationId })
            .ToListAsync(cancellationToken);

        foreach (var provider in providers)
        {
            try
            {
                var health = await providerService.CheckHealthAsync(
                    provider.OrganizationId,
                    provider.Id,
                    cancellationToken);
                await notifications.NotifyProviderHealthChangedAsync(
                    provider.OrganizationId,
                    provider.Id,
                    health.Status,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Health check failed for AI provider {ProviderId}", provider.Id);
            }
        }
    }
}
