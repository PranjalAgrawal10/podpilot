using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Resolves routes using static policies and catalog priority.
/// </summary>
public sealed class LegacyAiInferenceRouter : ILegacyAiInferenceRouter
{
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderService aiProviderService;
    private readonly ILogger<LegacyAiInferenceRouter> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyAiInferenceRouter"/> class.
    /// </summary>
    public LegacyAiInferenceRouter(
        IApplicationDbContext dbContext,
        IAiProviderService aiProviderService,
        ILogger<LegacyAiInferenceRouter> logger)
    {
        this.dbContext = dbContext;
        this.aiProviderService = aiProviderService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<AiInferenceRoute?> TryResolveAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken = default)
    {
        var policies = await dbContext.AiRoutingPolicies
            .AsNoTracking()
            .Include(p => p.PrimaryProvider)
            .ThenInclude(p => p!.Credential)
            .Where(p => p.OrganizationId == organizationId && p.IsEnabled)
            .ToListAsync(cancellationToken);

        Domain.Entities.AiRoutingPolicy? matchedPolicy = null;
        if (!string.IsNullOrWhiteSpace(model))
        {
            matchedPolicy = policies.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.ModelName) &&
                string.Equals(p.ModelName, model, StringComparison.OrdinalIgnoreCase));
        }

        matchedPolicy ??= policies.FirstOrDefault(p => p.IsDefault && p.PrimaryProviderId.HasValue);
        matchedPolicy ??= policies.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.ModelName) && p.PrimaryProviderId.HasValue);

        if (matchedPolicy?.PrimaryProvider is not null &&
            matchedPolicy.PrimaryProvider.IsEnabled &&
            matchedPolicy.PrimaryProvider.IsValidated)
        {
            var primary = await aiProviderService.CreateConnectionAsync(matchedPolicy.PrimaryProvider, cancellationToken);
            var fallbackIds = ParseFallbackIds(matchedPolicy.FallbackProviderIdsJson);
            var fallbackConnections = new List<AiProviderConnection>();
            foreach (var fallbackId in fallbackIds)
            {
                var fallback = await dbContext.AiInferenceProviders
                    .Include(p => p.Credential)
                    .FirstOrDefaultAsync(
                        p => p.Id == fallbackId &&
                             p.OrganizationId == organizationId &&
                             p.IsEnabled &&
                             p.IsValidated,
                        cancellationToken);
                if (fallback is null)
                {
                    continue;
                }

                fallbackConnections.Add(await aiProviderService.CreateConnectionAsync(fallback, cancellationToken));
            }

            return new AiInferenceRoute
            {
                Connection = primary,
                Model = model ?? matchedPolicy.ModelName ?? string.Empty,
                FallbackConnections = fallbackConnections,
                FailoverStrategy = matchedPolicy.FailoverStrategy,
                MaxRetries = matchedPolicy.MaxRetries,
                RoutingPolicyId = matchedPolicy.Id,
            };
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return null;
        }

        var catalogMatch = await dbContext.AiProviderModels
            .AsNoTracking()
            .Include(m => m.AiProvider)
            .ThenInclude(p => p.Credential)
            .Where(m =>
                m.OrganizationId == organizationId &&
                m.IsEnabled &&
                m.ModelName == model &&
                m.AiProvider.IsEnabled &&
                m.AiProvider.IsValidated)
            .OrderBy(m => m.AiProvider.Priority)
            .ThenBy(m => m.AiProvider.Name)
            .Select(m => m.AiProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (catalogMatch is null)
        {
            logger.LogDebug("No AI provider route for model {Model} in org {OrganizationId}", model, organizationId);
            return null;
        }

        var connection = await aiProviderService.CreateConnectionAsync(catalogMatch, cancellationToken);
        return new AiInferenceRoute
        {
            Connection = connection,
            Model = model,
            FallbackConnections = [],
            FailoverStrategy = AiFailoverStrategy.RetryThenFailover,
            MaxRetries = 2,
        };
    }

    private static IReadOnlyList<Guid> ParseFallbackIds(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
