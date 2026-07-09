using Microsoft.EntityFrameworkCore;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;
using PodPilot.Application.Models.Orchestration;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Resolves gateway routes to GPU pods.
/// </summary>
public sealed class GatewayRouter : IGatewayRouter
{
    private readonly IApplicationDbContext dbContext;
    private readonly IPodOrchestrator podOrchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayRouter"/> class.
    /// </summary>
    public GatewayRouter(IApplicationDbContext dbContext, IPodOrchestrator podOrchestrator)
    {
        this.dbContext = dbContext;
        this.podOrchestrator = podOrchestrator;
    }

    /// <inheritdoc />
    public async Task<GatewayRouteResult> ResolveAsync(
        Guid organizationId,
        string? model,
        CancellationToken cancellationToken = default)
    {
        var orchestratorResult = await podOrchestrator.ResolvePodAsync(
            new OrchestratorRouteRequest
            {
                OrganizationId = organizationId,
                ModelName = model,
            },
            cancellationToken);

        if (orchestratorResult is not null)
        {
            return new GatewayRouteResult
            {
                Pod = orchestratorResult.Pod,
                Model = orchestratorResult.Model ?? model,
                BaseUrl = orchestratorResult.BaseUrl,
            };
        }

        GatewayRoute? route = null;

        if (!string.IsNullOrWhiteSpace(model))
        {
            route = await dbContext.GatewayRoutes
                .Where(r => r.OrganizationId == organizationId && r.ModelName == model)
                .FirstOrDefaultAsync(cancellationToken);
        }

        route ??= await dbContext.GatewayRoutes
            .Where(r => r.OrganizationId == organizationId && r.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        GpuPod? pod;
        if (route is not null)
        {
            pod = await dbContext.GpuPods
                .Where(p => p.Id == route.GpuPodId && p.OrganizationId == organizationId && p.Status != PodStatus.Deleted)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            pod = await dbContext.GpuPods
                .Where(p => p.OrganizationId == organizationId
                    && p.Status != PodStatus.Deleted
                    && p.Endpoint != null)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (pod is null)
        {
            throw new InvalidOperationException("No GPU pod is available for gateway routing.");
        }

        return new GatewayRouteResult
        {
            Pod = pod,
            Model = model ?? route?.ModelName,
            BaseUrl = GatewayUrlHelper.GetOllamaBaseUrl(pod),
        };
    }
}
