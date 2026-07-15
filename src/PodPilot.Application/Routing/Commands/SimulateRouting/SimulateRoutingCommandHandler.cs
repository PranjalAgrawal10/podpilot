using MediatR;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Routing;
using PodPilot.Contracts.Routing;
using PodPilot.Domain.Authorization;

namespace PodPilot.Application.Routing.Commands.SimulateRouting;

/// <summary>Handles routing simulation.</summary>
public sealed class SimulateRoutingCommandHandler : IRequestHandler<SimulateRoutingCommand, SimulateRoutingResponse>
{
    private readonly ICurrentUserService currentUserService;
    private readonly IOrganizationAuthorizationService organizationAuthorizationService;
    private readonly IRoutingEngine routingEngine;

    /// <summary>Initializes a new instance of the <see cref="SimulateRoutingCommandHandler"/> class.</summary>
    public SimulateRoutingCommandHandler(
        ICurrentUserService currentUserService,
        IOrganizationAuthorizationService organizationAuthorizationService,
        IRoutingEngine routingEngine)
    {
        this.currentUserService = currentUserService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.routingEngine = routingEngine;
    }

    /// <inheritdoc />
    public async Task<SimulateRoutingResponse> Handle(
        SimulateRoutingCommand request,
        CancellationToken cancellationToken)
    {
        var (userId, organizationId) = RoutingAccess.RequireOrganizationContext(currentUserService);
        await RoutingAccess.EnsurePermissionAsync(
            organizationAuthorizationService, organizationId, userId, PermissionNames.RoutingManage, cancellationToken);

        var decision = await routingEngine.SimulateAsync(
            new RoutingEngineRequest
            {
                OrganizationId = organizationId,
                Prompt = request.Prompt,
                Path = request.Path ?? "/v1/chat/completions",
                StrategyOverride = request.Strategy,
                ModelHint = request.ModelHint,
                IsSimulation = true,
            },
            cancellationToken);

        return RoutingMapper.ToSimulateResponse(decision);
    }
}
