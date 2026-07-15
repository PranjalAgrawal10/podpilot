using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.AiProviders;
using PodPilot.Application.Models.Routing;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.AiProviders;

/// <summary>
/// Resolves AI inference routes via the intelligent routing engine with legacy fallback.
/// </summary>
public sealed class AiInferenceRouter : IAiInferenceRouter
{
    private readonly IRoutingEngine routingEngine;
    private readonly ILegacyAiInferenceRouter legacyRouter;
    private readonly IApplicationDbContext dbContext;
    private readonly IAiProviderService aiProviderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiInferenceRouter"/> class.
    /// </summary>
    public AiInferenceRouter(
        IRoutingEngine routingEngine,
        ILegacyAiInferenceRouter legacyRouter,
        IApplicationDbContext dbContext,
        IAiProviderService aiProviderService)
    {
        this.routingEngine = routingEngine;
        this.legacyRouter = legacyRouter;
        this.dbContext = dbContext;
        this.aiProviderService = aiProviderService;
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

        return await legacyRouter.TryResolveAsync(organizationId, model, cancellationToken);
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
}
