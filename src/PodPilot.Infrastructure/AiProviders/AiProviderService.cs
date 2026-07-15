using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Contracts.AiProviders;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Application service for AI inference provider management.
/// </summary>
public sealed class AiProviderService : IAiProviderService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderFactory providerFactory;
    private readonly IAiProviderRegistry providerRegistry;
    private readonly IEncryptionService encryptionService;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<AiProviderService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiProviderService"/> class.
    /// </summary>
    public AiProviderService(
        IApplicationDbContext dbContext,
        IAiProviderFactory providerFactory,
        IAiProviderRegistry providerRegistry,
        IEncryptionService encryptionService,
        IDateTimeService dateTimeService,
        ILogger<AiProviderService> logger)
    {
        this.dbContext = dbContext;
        this.providerFactory = providerFactory;
        this.providerRegistry = providerRegistry;
        this.encryptionService = encryptionService;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task SyncModelsAsync(
        AiInferenceProvider provider,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var connection = BuildConnection(provider, apiKey);
        var aiProvider = providerFactory.GetProvider(provider.ProviderKind);
        IReadOnlyList<AiModelInfo> models;
        try
        {
            models = await aiProvider.ListModelsAsync(connection, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to sync models for AI provider {ProviderId}", provider.Id);
            return;
        }

        var existing = await dbContext.AiProviderModels
            .Where(m => m.AiProviderId == provider.Id)
            .ToListAsync(cancellationToken);

        var now = dateTimeService.UtcNow;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var model in models)
        {
            if (string.IsNullOrWhiteSpace(model.ModelName) || !seen.Add(model.ModelName))
            {
                continue;
            }

            var match = existing.FirstOrDefault(m =>
                string.Equals(m.ModelName, model.ModelName, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                await dbContext.AddAiProviderModelAsync(
                    new AiProviderModel
                    {
                        OrganizationId = provider.OrganizationId,
                        AiProviderId = provider.Id,
                        ModelName = model.ModelName,
                        DisplayName = model.DisplayName,
                        ContextLength = model.ContextLength,
                        Parameters = model.Parameters,
                        SupportsStreaming = model.SupportsStreaming,
                        SupportsVision = model.SupportsVision,
                        SupportsTools = model.SupportsTools,
                        SupportsEmbeddings = model.SupportsEmbeddings,
                        InputCostPerMillionTokens = model.InputCostPerMillionTokens,
                        OutputCostPerMillionTokens = model.OutputCostPerMillionTokens,
                        IsEnabled = true,
                        SyncedAt = now,
                        CreatedAt = now,
                    },
                    cancellationToken);
            }
            else
            {
                match.DisplayName = model.DisplayName;
                match.ContextLength = model.ContextLength;
                match.Parameters = model.Parameters;
                match.SupportsStreaming = model.SupportsStreaming;
                match.SupportsVision = model.SupportsVision;
                match.SupportsTools = model.SupportsTools;
                match.SupportsEmbeddings = model.SupportsEmbeddings;
                match.InputCostPerMillionTokens = model.InputCostPerMillionTokens;
                match.OutputCostPerMillionTokens = model.OutputCostPerMillionTokens;
                match.SyncedAt = now;
                match.UpdatedAt = now;
            }
        }
    }

    /// <inheritdoc />
    public async Task<AiProviderHealthResponse> CheckHealthAsync(
        Guid organizationId,
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var provider = await dbContext.AiInferenceProviders
            .Include(p => p.Credential)
            .Include(p => p.Health)
            .FirstOrDefaultAsync(
                p => p.Id == providerId && p.OrganizationId == organizationId,
                cancellationToken)
            ?? throw new Application.Common.Exceptions.NotFoundException("AI provider", providerId);

        var connection = await CreateConnectionAsync(provider, cancellationToken);
        var aiProvider = providerFactory.GetProvider(provider.ProviderKind);
        var result = await aiProvider.HealthAsync(connection, cancellationToken);
        var now = dateTimeService.UtcNow;

        var health = provider.Health;
        if (health is null)
        {
            health = new AiProviderHealth { AiProviderId = provider.Id };
            provider.Health = health;
            await dbContext.AddAiProviderHealthAsync(health, cancellationToken);
        }

        health.Status = result.IsHealthy ? AiProviderHealthState.Healthy : AiProviderHealthState.Unhealthy;
        health.LatencyMs = result.LatencyMs;
        health.ErrorMessage = result.IsHealthy ? null : result.Message;
        health.LastCheckedAt = now;
        health.ConsecutiveFailures = result.IsHealthy ? 0 : health.ConsecutiveFailures + 1;
        if (!result.IsHealthy && health.ConsecutiveFailures >= 3)
        {
            health.Status = AiProviderHealthState.Unhealthy;
        }
        else if (!result.IsHealthy)
        {
            health.Status = AiProviderHealthState.Degraded;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AiProviderHealthResponse
        {
            ProviderId = provider.Id,
            Status = health.Status.ToString(),
            LatencyMs = health.LatencyMs,
            ErrorRate = health.ErrorRate,
            ErrorMessage = health.ErrorMessage,
            LastCheckedAt = health.LastCheckedAt,
            ConsecutiveFailures = health.ConsecutiveFailures,
        };
    }

    /// <inheritdoc />
    public Task<AiProviderConnection> CreateConnectionAsync(
        AiInferenceProvider provider,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (provider.Credential is null)
        {
            throw new InvalidOperationException($"AI provider '{provider.Id}' has no credential.");
        }

        var apiKey = encryptionService.Decrypt(provider.Credential.EncryptedApiKey);
        return Task.FromResult(BuildConnection(provider, apiKey));
    }

    /// <inheritdoc />
    public async Task<AiProviderDashboard> GetDashboardAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var providers = await dbContext.AiInferenceProviders
            .AsNoTracking()
            .Include(p => p.Health)
            .Where(p => p.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var enabled = providers.Where(p => p.IsEnabled).ToList();
        var healthy = enabled.Count(p => p.Health?.Status == AiProviderHealthState.Healthy);
        var unhealthy = enabled.Count(p =>
            p.Health?.Status is AiProviderHealthState.Unhealthy or AiProviderHealthState.Degraded);
        var modelCount = await dbContext.AiProviderModels
            .CountAsync(m => m.OrganizationId == organizationId && m.IsEnabled, cancellationToken);
        var latencies = enabled
            .Select(p => p.Health?.LatencyMs)
            .Where(l => l.HasValue)
            .Select(l => (double)l!.Value)
            .ToList();
        var errorRates = enabled
            .Select(p => p.Health?.ErrorRate ?? 0)
            .ToList();

        return new AiProviderDashboard
        {
            ConnectedProviders = healthy,
            TotalProviders = enabled.Count,
            AvailableModels = modelCount,
            UnhealthyProviders = unhealthy,
            StreamingSessions = 0,
            AverageLatencyMs = latencies.Count == 0 ? 0 : latencies.Average(),
            AverageErrorRate = errorRates.Count == 0 ? 0 : errorRates.Average(),
        };
    }

    private AiProviderConnection BuildConnection(AiInferenceProvider provider, string apiKey)
    {
        var metadata = providerRegistry.GetMetadata(provider.ProviderKind);
        var baseUrl = string.IsNullOrWhiteSpace(provider.BaseUrl)
            ? metadata.DefaultBaseUrl
            : provider.BaseUrl;

        return new AiProviderConnection
        {
            OrganizationId = provider.OrganizationId,
            ProviderId = provider.Id,
            ProviderKind = provider.ProviderKind,
            ApiKey = apiKey,
            BaseUrl = baseUrl,
            DeploymentName = provider.DeploymentName,
            ApiVersion = provider.ApiVersion,
        };
    }
}
