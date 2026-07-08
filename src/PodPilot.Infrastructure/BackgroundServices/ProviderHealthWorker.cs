using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically checks compute provider health and records history.
/// </summary>
public sealed class ProviderHealthWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ProviderHealthWorker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderHealthWorker"/> class.
    /// </summary>
    public ProviderHealthWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProviderHealthWorker> logger)
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
                logger.LogError(ex, "Provider health worker failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckProvidersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var providerService = scope.ServiceProvider.GetRequiredService<IProviderService>();

        var providers = await dbContext.ComputeProviders
            .Where(p => p.IsEnabled && p.IsValidated)
            .Include(p => p.Credential)
            .ToListAsync(cancellationToken);

        foreach (var provider in providers)
        {
            try
            {
                await providerService.CheckAndPersistHealthAsync(provider, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Health check failed for provider {ProviderId}",
                    provider.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
