using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Resolves AI inference routes via the intelligent routing engine with policy/catalog fallback.
/// </summary>
public sealed class AiInferenceRouter : IAiInferenceRouter
{
    private readonly IRoutingEngine routingEngine;
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderService aiProviderService;
    private readonly ILogger<AiInferenceRouter> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiInferenceRouter"/> class.
    /// </summary>
    public AiInferenceRouter(
        IRoutingEngine routingEngine,
        IApplicationDbContext dbContext,
        IAiProviderService aiProviderService,
        ILogger<AiInferenceRouter> logger)
    {
        this.routingEngine = routingEngine;
        this.dbContext = dbContext;
        this.aiProviderService = aiProviderService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public Task<AiInferenceRoute?> TryResolveAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken = default) =>
        TryResolveAsync(organizationId, model, path: null, bodyJson: null, cancellationToken);

    /// <inheritdoc />
    public async Task<AiInferenceRoute?> TryResolveAsync(
        Guid organizationId,
        string? model,
        string? path,
        string? bodyJson,
        CancellationToken cancellationToken = default)
    {
        var decision = await routingEngine.RouteAsync(
            new RoutingEngineRequest
            {
                OrganizationId = organizationId,
                ModelHint = model,
                Path = path,
                BodyJson = bodyJson,
                IsSimulation = false,
            },
            cancellationToken);

        if (decision.Selected is not null)
        {
            var route = await BuildRouteFromDecisionAsync(organizationId, decision, cancellationToken);
            if (route is not null)
            {
                return route;
            }
        }

        return await TryResolveLegacyAsync(organizationId, model, cancellationToken);
    }

    private async Task<AiInferenceRoute?> BuildRouteFromDecisionAsync(
        Guid organizationId,
        RoutingDecision decision,
        CancellationToken cancellationToken)
    {
        var selected = decision.Selected!;
        var primaryProvider = await dbContext.AiInferenceProviders
            .Include(p => p.Credential)
            .FirstOrDefaultAsync(
                p => p.Id == selected.ProviderId &&
                     p.OrganizationId == organizationId &&
                     p.IsEnabled &&
                     p.IsValidated,
                cancellationToken);

        if (primaryProvider is null)
        {
            return null;
        }

        var primary = await aiProviderService.CreateConnectionAsync(primaryProvider, cancellationToken);
        var fallbackConnections = new List<AiProviderConnection>();
        foreach (var fallback in decision.Fallbacks)
        {
            var provider = await dbContext.AiInferenceProviders
                .Include(p => p.Credential)
                .FirstOrDefaultAsync(
                    p => p.Id == fallback.ProviderId &&
                         p.OrganizationId == organizationId &&
                         p.IsEnabled &&
                         p.IsValidated,
                    cancellationToken);
            if (provider is null)
            {
                continue;
            }

            fallbackConnections.Add(await aiProviderService.CreateConnectionAsync(provider, cancellationToken));
        }

        var policy = decision.PolicyId.HasValue
            ? await dbContext.AiRoutingPolicies.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == decision.PolicyId.Value, cancellationToken)
            : null;

        return new AiInferenceRoute
        {
            Connection = primary,
            Model = selected.ModelName,
            FallbackConnections = fallbackConnections,
            FailoverStrategy = policy?.FailoverStrategy ?? AiFailoverStrategy.RetryThenFailover,
            MaxRetries = policy?.MaxRetries ?? 2,
            RoutingPolicyId = decision.PolicyId,
        };
    }

    private async Task<AiInferenceRoute?> TryResolveLegacyAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken)
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
